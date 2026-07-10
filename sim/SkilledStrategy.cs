using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// SKILLED — the S4 measuring stick (target: median death round ≥7). Estimates like a sharp, then
/// sizes and times like one. Policy is deliberately dial-ridden (consts below) and readable; it is
/// expected to be iterated once real numbers exist.
///
/// Estimation (p̂ per side): pure two-way de-vig of the offered odds — NORMALIZE the implied probs
///   (p̂_home = (1/o_home)/(1/o_home + 1/o_away)). Because v0's book prices proportionally
///   (o = 1/(p·(1+overround))), this recovers the true p EXACTLY. The irony is noted for the record:
///   with the information relics cut from v0 (DECISIONS.md 2026-07-09), the de-vigging bot is a
///   perfectly-informed player anyway — which is fine for an S4 measuring stick; the axis returns in
///   v2 with pricing noise and multiple books, and only then does information have a price again.
///
/// Betting: a primary 1–3 leg ticket on the highest-p̂ sides, legs and stake chosen to clear the
///   round's REQUIREMENT (target + any bookie debt) on a win, escalating when behind pace; Kelly-lite
///   floor. With boosted_odds owned, a small single-leg exploit ticket captures the +EV boosted first
///   leg (promo_code, when owned, already makes the primary's first ticket fair-priced). With
///   high_roller owned and the requirement far, the primary is staked past half-bank for the bonus.
///
/// Cash-out: accept when the offer alone clears the round's requirement (survival trumps EV), or when
///   it is within 5% of the estimated value of holding (computed from revealed live win% + p̂, never
///   from true probs). Otherwise let it ride.
///
/// HONESTY: this bot reads only public state — offered odds, the bank/debt/requirement it could see
/// on screen, the cash-out offer, and each event's revealed WinProbAfter. It never reads
/// Matchup.TrueHomeProb, Leg.TrueProb, or Matchup.Result. The de-vig math uses only public odds.
/// </summary>
public sealed class SkilledStrategy : IStrategy
{
    // ---- dials ----
    private const double StakeMin = 0.10;          // Kelly-lite lower clamp (fraction of bank)
    private const double StakeMax = 0.60;          // Kelly-lite upper clamp
    private const double HighRollerFrac = 0.55;    // safely past the 0.5-bank threshold after flooring
    private const double HighRollerNeedMult = 1.15;// only reach for the bonus when the requirement is this far
    private const double ClearBuffer = 1.08;       // aim ~8% over the requirement so a win clears after flooring
    private const double ExploitFrac = 0.15;       // stake of the boosted single-leg exploit ticket
    private const double CashOutEvRatio = 0.95;    // accept a cash-out at ≥95% of estimated hold value

    // Keep ≥110% of the NEXT requirement as working capital when buying — relics are bought from
    // genuine surplus only. (Was 0.40 pre-debt: under debt-as-HP and the flat-early recalibrated
    // targets, buying down to 40% guaranteed a ~2.5× requirement multiple — an all-in coin flip —
    // after every shop, which is not what "skilled" means. Iterated 2026-07-09 with the retune.)
    private const double ShopHeadroomFloor = 1.10;
    private const int MaxPrimaryLegs = 3;

    // Shop buy priority (highest first) over the 8-relic catalog: pricing edges before capital/insurance.
    private static readonly string[] Priority =
    {
        "boosted_odds", "promo_code", "early_payout", "piggy_bank",
        "high_roller", "bankroll_insurance", "mulligan", "lucky_charm",
    };

    public string Name => "skilled";
    public bool ControlsSweat => true;

    private readonly struct Cand
    {
        public readonly int Matchup;
        public readonly Side Side;
        public readonly double PHat;
        public readonly double Odds;
        public Cand(int matchup, Side side, double pHat, double odds)
        {
            Matchup = matchup; Side = side; PHat = pHat; Odds = odds;
        }
    }

    public void Bet(Run run, BotState state, Pcg32 rng)
    {
        IReadOnlyList<Matchup> slate = run.CurrentSlate.Matchups;

        // 1. de-vig estimate for every matchup (see the class doc for why this is exact in v0).
        foreach (Matchup m in slate)
            state.HomeProbEst[m.Index] = DevigHome(m);

        // 2. candidate legs: each matchup's better side by p̂.
        var cands = new List<Cand>(slate.Count);
        foreach (Matchup m in slate)
        {
            double pHome = state.HomeProbEst[m.Index];
            Side side = pHome >= 0.5 ? Side.Home : Side.Away;
            double pHat = side == Side.Home ? pHome : 1.0 - pHome;
            cands.Add(new Cand(m.Index, side, pHat, m.Odds(side)));
        }
        cands.Sort((a, b) => b.PHat.CompareTo(a.PHat)); // highest-confidence first

        if (run.Bank < run.Config.MinStake || cands.Count < 2) return;

        PlacePrimary(run, cands);
        PlaceBoostedExploit(run, cands);
    }

    private void PlacePrimary(Run run, List<Cand> cands)
    {
        // Debt-as-HP: the settle checks Requirement (target + debt), so the sharp plans against it.
        double bank = run.Bank;
        double needMult = run.Requirement / bank;              // >1 whenever we still trail the requirement
        double aimMult = run.Requirement * ClearBuffer / bank; // aim past it so a win clears with headroom

        // When behind pace, "escalate" past the Kelly-lite cap: the sharp will size up (to all-in if the
        // requirement demands it) rather than place a bet that mathematically cannot clear. When ahead,
        // keep the disciplined 0.6 cap.
        double escMax = aimMult > 1.0 ? run.Config.MaxStakeFraction : StakeMax;

        // Pick the leg set that MAXIMISES the estimated chance of clearing: fewer legs mean a higher joint
        // win prob but need higher odds (so a bigger stake). Consider a single best side, then the top-2 and
        // top-3 favorites; keep whichever can clear at ≤ escMax with the highest win probability. This makes
        // skilled favour a big single/short bet under steep requirements and 2–3 leg parlays when they clear
        // comfortably — the survival-honest reading of "prefer parlays, but escalate when behind pace".
        var best = ChooseTicket(cands, aimMult, escMax);
        if (best is not { } plan) return;

        double frac = Math.Clamp(Math.Max(Kelly(plan.WinProb, plan.Odds), ReqFrac(aimMult, plan.Odds)), StakeMin, escMax);
        if (Owns(run, "high_roller") && needMult >= HighRollerNeedMult)
            frac = Math.Min(escMax, Math.Max(frac, HighRollerFrac));

        double stake = Math.Clamp(Math.Floor(frac * bank), run.Config.MinStake, bank);
        if (stake < run.Config.MinStake) return;

        run.PlaceTicket(plan.Picks, stake);
    }

    private readonly struct Plan
    {
        public readonly List<Pick> Picks;
        public readonly double Odds;
        public readonly double WinProb;
        public Plan(List<Pick> picks, double odds, double winProb) { Picks = picks; Odds = odds; WinProb = winProb; }
    }

    // Best clearing ticket by estimated win probability. Single leg searches every side for the highest-p̂
    // one whose odds still clear at ≤ escMax; parlays use the top-L favorites. Falls back to the highest-odds
    // top-favorite parlay if nothing can clear (requirement out of reach — bet for the biggest multiplier).
    private static Plan? ChooseTicket(List<Cand> cands, double aimMult, double escMax)
    {
        Plan? best = null;
        double bestWin = -1.0;

        // L = 1: the single side most likely to win while still clearing at ≤ escMax.
        foreach (Cand c in cands)
        {
            if (ReqFrac(aimMult, c.Odds) > escMax) continue;
            if (c.PHat > bestWin)
            {
                bestWin = c.PHat;
                best = new Plan(new List<Pick> { new Pick(c.Matchup, c.Side) }, c.Odds, c.PHat);
            }
        }

        // L = 2, 3: top-L highest-p̂ favorites.
        double odds = 1.0, win = 1.0;
        var picks = new List<Pick>(3);
        for (int i = 0; i < Math.Min(MaxPrimaryLegs, cands.Count); i++)
        {
            odds *= cands[i].Odds;
            win *= cands[i].PHat;
            picks.Add(new Pick(cands[i].Matchup, cands[i].Side));
            if (i + 1 < 2) continue; // only consider L >= 2 here
            if (ReqFrac(aimMult, odds) <= escMax && win > bestWin)
            {
                bestWin = win;
                best = new Plan(new List<Pick>(picks), odds, win);
            }
        }

        if (best != null) return best;

        // Nothing clears even all-in: bet the widest top-favorite parlay for the largest multiplier.
        int legs = Math.Min(MaxPrimaryLegs, cands.Count);
        double o = 1.0, w = 1.0;
        var p = new List<Pick>(legs);
        for (int i = 0; i < legs; i++) { o *= cands[i].Odds; w *= cands[i].PHat; p.Add(new Pick(cands[i].Matchup, cands[i].Side)); }
        return new Plan(p, o, w);
    }

    // boosted_odds boosts every ticket's first leg by +15%, which is +EV against the vig-priced book; a
    // small single-leg ticket on the strongest favorite banks that edge (promo_code, when also owned,
    // overwrites the first ticket to fair odds, so this second ticket is where the boost actually lands).
    private void PlaceBoostedExploit(Run run, List<Cand> cands)
    {
        if (!Owns(run, "boosted_odds")) return;
        if (run.Tickets.Count >= run.Config.MaxTicketsPerRound) return;
        if (run.Bank < run.Config.MinStake) return;

        double stake = Math.Clamp(Math.Floor(ExploitFrac * run.Bank), run.Config.MinStake, run.Bank);
        if (stake < run.Config.MinStake) return;

        run.PlaceTicket(new List<Pick> { new Pick(cands[0].Matchup, cands[0].Side) }, stake);
    }

    public bool ShouldCashOut(Run run, Ticket ticket, SweatSession session, DramaEvent evt,
        double offer, double bankNow, double target, BotState state, Pcg32 rng)
    {
        // Only decide during a leg's in-progress beats: on a LegFinal beat the session has already
        // advanced to (or completed) the next leg, so evt no longer describes the live cash-out state.
        if (evt.Type == DramaEventType.LegFinal) return false;

        // `target` is the harness-passed requirement (target + debt) — survival math must clear it.
        double remainingNeeded = target - bankNow;
        if (remainingNeeded > 0 && offer >= remainingNeeded) return true; // survival trumps EV

        double estHold = EstHoldEv(run, ticket, session, evt, state);
        if (estHold <= 0.0) return false;
        return offer >= CashOutEvRatio * estHold;
    }

    // Estimated value of holding, from revealed state only: settled-won legs (real odds) × current leg
    // (revealed live win% × odds) × un-started legs (p̂ × odds) × payout mult, plus future early-payout EV.
    private static double EstHoldEv(Run run, Ticket ticket, SweatSession session, DramaEvent evt, BotState state)
    {
        int cur = evt.LegIndex;
        IReadOnlyList<Leg> legs = ticket.Legs;

        double resolvedOdds = 1.0;
        for (int j = 0; j < cur; j++)
            if (!legs[j].IsVoided && session.RevealedLegState(j) == LegState.Won)
                resolvedOdds *= legs[j].OfferedOdds;

        double val = ticket.Stake * resolvedOdds * (evt.WinProbAfter * legs[cur].OfferedOdds);
        for (int j = cur + 1; j < legs.Count; j++)
            if (!legs[j].IsVoided)
                val *= PHat(state, legs[j]) * legs[j].OfferedOdds;
        val *= ticket.PayoutMultiplier;

        double epFraction = EarlyPayoutFraction(run);
        if (epFraction > 0.0)
        {
            double b = epFraction * ticket.Stake;
            double cumulative = evt.WinProbAfter;
            double ev = b * cumulative;
            for (int j = cur + 1; j < legs.Count; j++)
            {
                if (legs[j].IsVoided) continue;
                cumulative *= PHat(state, legs[j]);
                ev += b * cumulative;
            }
            val += ev;
        }
        return val;
    }

    public void Shop(Run run, BotState state, Pcg32 rng)
    {
        // The PRD frames the shop as "power vs target headroom": every dollar spent on a relic is a dollar
        // not available to clear the NEXT settle. A sharp respects that — buy in priority order, but only
        // while enough working capital survives the purchase to still have a real shot at the next round.
        // Shop runs before ExitShop increments Round, so run.Round is the round just cleared (1-based);
        // the current round sits at Targets[Round-1], hence the NEXT target is Targets[Round] — plus any
        // bookie debt still on the books (a float leaves the shop with debt outstanding).
        double nextRequirement = run.Config.Targets[run.Round] + run.Debt;
        double floor = ShopHeadroomFloor * nextRequirement;

        while (run.OwnedRelics.Count < run.Config.RelicSlots)
        {
            int buyIndex = -1;
            int bestRank = int.MaxValue;
            for (int i = 0; i < run.ShopOffers.Count; i++)
            {
                RelicDefinition o = run.ShopOffers[i];
                if (o.Price > run.Bank || run.Bank - o.Price < floor) continue; // keep the headroom
                int rank = RankOf(o.Id);
                if (rank < bestRank) { bestRank = rank; buyIndex = i; }
            }
            if (buyIndex < 0) break;
            run.BuyRelic(buyIndex);
        }
    }

    // ---- estimation helpers (public info only) ----

    private static double DevigHome(Matchup m)
    {
        double implHome = 1.0 / m.HomeOdds;
        double implAway = 1.0 / m.AwayOdds;
        return implHome / (implHome + implAway);
    }

    private static double PHat(BotState state, Leg leg)
    {
        int idx = leg.Matchup.Index;
        double pHome = state.HomeProbEst.TryGetValue(idx, out double v) ? v : DevigHome(leg.Matchup);
        return leg.Side == Side.Home ? pHome : 1.0 - pHome;
    }

    // Required stake fraction f so that f·O + (1−f) ≥ needMult on a win: f ≥ (needMult−1)/(O−1).
    private static double ReqFrac(double needMult, double odds)
    {
        if (odds <= 1.0) return double.MaxValue;
        return Math.Max(0.0, (needMult - 1.0) / (odds - 1.0));
    }

    // Kelly fraction for a single parlay outcome: edge / (O−1), floored at 0 (never a negative stake).
    private static double Kelly(double pProd, double odds)
    {
        double edge = pProd * odds - 1.0;
        return edge <= 0.0 ? 0.0 : edge / (odds - 1.0);
    }

    private static bool Owns(Run run, string id)
    {
        foreach (RelicDefinition d in run.OwnedRelics)
            if (d.Id == id) return true;
        return false;
    }

    private static double EarlyPayoutFraction(Run run)
    {
        foreach (RelicDefinition d in run.OwnedRelics)
            if (d.Id == "early_payout")
                return d.Params.TryGetValue("fractionOfStake", out double f) ? f : 0.0;
        return 0.0;
    }

    private static int RankOf(string id)
    {
        for (int i = 0; i < Priority.Length; i++)
            if (Priority[i] == id) return i;
        return int.MaxValue;
    }
}
