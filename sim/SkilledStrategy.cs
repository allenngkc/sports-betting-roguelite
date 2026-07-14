using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// SKILLED — the G3 measuring stick (economy rework: median death ≥7, win 10–15% with items).
/// Estimates like a sharp, then sizes and times like one.
///
/// Estimation (p̂ per side): pure two-way de-vig of the offered odds — NORMALIZE the implied probs.
/// Because v0's book prices proportionally, this recovers true p exactly (noted for the record; the
/// information axis returns in v2 with pricing noise).
///
/// Betting (payment model): the settle DEDUCTS CurrentPayment, so the sharp plans to hold
/// bank ≥ payment at settle — sizing escalates when the payment is out of reach. A held Profit
/// Boost is played on the primary ticket's longest-odds leg (the biggest absolute odds gain).
///
/// Shop: passives in priority order (Multiplier → Scar → Totem: the static engine compounds all
/// run, the ratchet earns from variance, the totem is bought when affordable insurance), then a
/// Mulligan Slip / Profit Boost while a consumable slot is free — all only above a working-capital
/// floor of the NEXT payment.
///
/// HONESTY: reads only public state — odds, bank/payment, offers, revealed WinProbAfter, own items.
/// Never Matchup.TrueHomeProb / Leg.TrueProb / Matchup.Result.
/// </summary>
public class SkilledStrategy : IStrategy
{
    // ---- dials ----
    private const double StakeMin = 0.10;          // engine-bet floor (fraction of spare capital)
    private const double EngineStakeCap = 0.50;    // engine-bet cap (fraction of spare capital)
    private const double ClearBuffer = 1.08;       // survival aims ~8% past the payment
    private const double CashOutEvRatio = 0.95;    // accept a cash-out at ≥95% of estimated hold value
    private const double ShopHeadroomFloor = 1.00; // keep the NEXT payment intact when buying
    private const int MaxPrimaryLegs = 3;

    // Passive buy priority (highest first); consumables bought after passives while slots free.
    private static readonly string[] RelicPriority =
        { RelicCatalog.MultiplierId, RelicCatalog.ScarTissueId, RelicCatalog.TotemId };
    private static readonly string[] ConsumablePriority = { "mulligan_slip", "profit_boost" };

    private readonly bool _shops;

    public SkilledStrategy() : this(shops: true) { }
    protected SkilledStrategy(bool shops) => _shops = shops;

    /// <summary>True in the G5 measurement variant: stake policy ignores item ownership.</summary>
    protected virtual bool FixedDiscipline => false;

    public virtual string Name => "skilled";
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

        foreach (Matchup m in slate)
            state.HomeProbEst[m.Index] = DevigHome(m);

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
    }

    private void PlacePrimary(Run run, List<Cand> cands)
    {
        // The payment-model sharp (rework insight, first grid run): max-win-prob singles just
        // bleed vig while the payments drain the bank — wins come from the RIGHT TAIL. So:
        //  • SURVIVAL mode (this round's payment already out of reach at rest): escalate — the
        //    plan clearing the payment with the highest win probability, all-in if demanded.
        //  • ENGINE mode (payment covered): reserve the payment, and put the spare capital on the
        //    top-favorites 3-leg parlay — with The Multiplier owned it is strongly +EV, and one
        //    hit covers payments for rounds. Sized toward denting the REMAINING schedule, capped.
        double bank = run.Bank;
        double payment = run.CurrentPayment;

        if (bank < payment * ClearBuffer)
        {
            // Survival: existing escalation logic against the payment itself.
            double aimMult = payment * ClearBuffer / bank;
            var rescue = ChooseTicket(cands, aimMult, run.Config.MaxStakeFraction);
            if (rescue is not { } r) return;
            double rf = Math.Clamp(ReqFrac(aimMult, r.Odds), StakeMin, run.Config.MaxStakeFraction);
            double rs = Math.Clamp(Math.Floor(rf * bank), run.Config.MinStake, bank);
            if (rs < run.Config.MinStake) return;
            run.PlaceTicket(r.Picks, rs, BoostLeg(run, r.Picks));
            return;
        }

        // Engine mode.
        double spare = bank - payment;
        if (spare < run.Config.MinStake) return;

        int legs = Math.Min(MaxPrimaryLegs, cands.Count);
        double odds = 1.0, win = 1.0;
        var picks = new List<Pick>(legs);
        for (int i = 0; i < legs; i++)
        {
            odds *= cands[i].Odds;
            win *= cands[i].PHat;
            picks.Add(new Pick(cands[i].Matchup, cands[i].Side));
        }

        // Size toward denting what's left of the schedule: a hit should cover ~half the remaining
        // payments, within [Kelly-lite floor, EngineStakeCap × spare].
        double remaining = 0;
        for (int r2 = run.Round - 1; r2 < run.PaymentSchedule.Count; r2++)
            remaining += run.PaymentSchedule[r2];
        // Pre-engine, the comps era (design/10 F): the bank is ~2–3 payments, so income is
        // mandatory NOW — size the parlay so a hit funds this payment plus a seed of the next
        // (quota sizing), which also pumps comp volume toward the engine. Post-engine, size
        // toward denting the remaining schedule.
        bool engined = OwnsMultiplier(run) && picks.Count >= 3;
        double stake;
        if (FixedDiscipline)
        {
            // The G5 measurement bot (Allen-approved fix): stakes 25% of spare REGARDLESS of item
            // ownership, so pair-vs-solo deltas measure composition, not ownership-induced
            // aggression (round 1's confound: the engine tempts bigger bets and earlier deaths).
            stake = Math.Clamp(Math.Floor(0.25 * spare), run.Config.MinStake, Math.Floor(spare));
            if (stake < run.Config.MinStake) return;
        }
        else if (!engined)
        {
            double quota = payment + 0.5 * (run.NextPayment ?? 0.0);
            double quotaStake = odds > 1.0 ? quota * ClearBuffer / odds : run.Config.MinStake;
            stake = Math.Clamp(Math.Floor(quotaStake), run.Config.MinStake, Math.Floor(spare));
            if (stake < run.Config.MinStake) return;
        }
        else
        {
            double targetPayout = 0.5 * remaining;
            double stakeForDent = targetPayout / (odds * 1.5);
            double cap = Math.Floor(EngineStakeCap * spare);
            if (cap < run.Config.MinStake) return; // spare too thin for an engine bet this round
            stake = Math.Clamp(Math.Floor(Math.Max(StakeMin * spare, stakeForDent)),
                run.Config.MinStake, cap);
        }

        run.PlaceTicket(picks, stake, BoostLeg(run, picks));
    }

    /// <summary>A held Profit Boost lands on the longest-odds leg — the largest absolute gain.</summary>
    private static int BoostLeg(Run run, List<Pick> picks)
    {
        if (!run.OwnsConsumable("profit_boost")) return -1;
        int boostLeg = -1;
        double bestOdds = -1.0;
        for (int i = 0; i < picks.Count; i++)
        {
            Matchup m = run.CurrentSlate.Matchups[picks[i].MatchupIndex];
            double o = m.Odds(picks[i].Side);
            if (o > bestOdds) { bestOdds = o; boostLeg = i; }
        }
        return boostLeg;
    }

    private static bool OwnsMultiplier(Run run)
    {
        foreach (RelicDefinition d in run.OwnedRelics)
            if (d.Id == RelicCatalog.MultiplierId) return true;
        return false;
    }

    private readonly struct Plan
    {
        public readonly List<Pick> Picks;
        public readonly double Odds;
        public readonly double WinProb;
        public Plan(List<Pick> picks, double odds, double winProb) { Picks = picks; Odds = odds; WinProb = winProb; }
    }

    // Best clearing ticket by estimated win probability; falls back to the widest top-favorite
    // parlay when nothing clears (payment out of reach — bet for the biggest multiplier).
    private static Plan? ChooseTicket(List<Cand> cands, double aimMult, double escMax)
    {
        Plan? best = null;
        double bestWin = -1.0;

        foreach (Cand c in cands)
        {
            if (ReqFrac(aimMult, c.Odds) > escMax) continue;
            if (c.PHat > bestWin)
            {
                bestWin = c.PHat;
                best = new Plan(new List<Pick> { new Pick(c.Matchup, c.Side) }, c.Odds, c.PHat);
            }
        }

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

        int legs = Math.Min(MaxPrimaryLegs, cands.Count);
        double o2 = 1.0, w2 = 1.0;
        var p2 = new List<Pick>(legs);
        for (int i = 0; i < legs; i++) { o2 *= cands[i].Odds; w2 *= cands[i].PHat; p2.Add(new Pick(cands[i].Matchup, cands[i].Side)); }
        return new Plan(p2, o2, w2);
    }

    public bool ShouldCashOut(Run run, Ticket ticket, SweatSession session, DramaEvent evt,
        double offer, double bankNow, double target, BotState state, Pcg32 rng)
    {
        if (evt.Type == DramaEventType.LegFinal) return false;

        // `target` is the harness-passed CurrentPayment — survival math must HOLD it at settle.
        double remainingNeeded = target - bankNow;
        if (remainingNeeded > 0 && offer >= remainingNeeded) return true; // survival trumps EV

        double estHold = EstHoldEv(ticket, session, evt, state);
        if (estHold <= 0.0) return false;
        return offer >= CashOutEvRatio * estHold;
    }

    // Estimated value of holding, from revealed state only: settled-won legs (real odds) × current leg
    // (revealed live win% × odds) × un-started legs (p̂ × odds) × the payout product.
    private static double EstHoldEv(Ticket ticket, SweatSession session, DramaEvent evt, BotState state)
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
        return val * ticket.PayoutMultiplier;
    }

    public virtual void Shop(Run run, BotState state, Pcg32 rng)
    {
        if (!_shops) return;

        // Comps are shop-only currency (design/10 F) — no cash tension, buy in priority while
        // comps and slots allow.
        bool bought = true;
        while (bought)
        {
            bought = false;
            if (run.OwnedRelics.Count < run.Config.RelicSlots)
            {
                int buyIndex = -1;
                int bestRank = int.MaxValue;
                for (int i = 0; i < run.ShopOffers.Count; i++)
                {
                    RelicDefinition o = run.ShopOffers[i];
                    if (o.Price > run.Comps) continue;
                    int rank = RankOf(RelicPriority, o.Id);
                    if (rank < bestRank) { bestRank = rank; buyIndex = i; }
                }
                if (buyIndex >= 0) { run.BuyRelic(buyIndex); bought = true; continue; }
            }

            if (run.OwnedConsumables.Count < run.Config.ConsumableSlots)
            {
                int buyIndex = -1;
                int bestRank = int.MaxValue;
                for (int i = 0; i < run.ConsumableOffers.Count; i++)
                {
                    ConsumableDefinition o = run.ConsumableOffers[i];
                    if (o.Price > run.Comps) continue;
                    int rank = RankOf(ConsumablePriority, o.Id);
                    if (rank < bestRank) { bestRank = rank; buyIndex = i; }
                }
                if (buyIndex >= 0) { run.BuyConsumable(buyIndex); bought = true; }
            }
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

    // Kelly fraction for a single parlay outcome: edge / (O−1), floored at 0.
    private static double Kelly(double pProd, double odds)
    {
        double edge = pProd * odds - 1.0;
        return edge <= 0.0 ? 0.0 : edge / (odds - 1.0);
    }

    private static int RankOf(string[] priority, string id)
    {
        for (int i = 0; i < priority.Length; i++)
            if (priority[i] == id) return i;
        return int.MaxValue;
    }
}

/// <summary>NOSHOP — the G2 measuring stick: skilled play that never buys anything (gifted
/// consumables still arrive through the bookie's pity channel and are used — gifts aren't buys).
/// If this bot still wins often, the curve is too flat and items are decoration.</summary>
public sealed class NoShopStrategy : SkilledStrategy
{
    public NoShopStrategy() : base(shops: false) { }
    public override string Name => "noshop";
}

/// <summary>The G5 measurement bot: skilled shopping OFF, stake discipline FIXED — granted-item
/// batches through this bot isolate what the items DO from how ownership changes behavior.</summary>
public sealed class FixedDisciplineStrategy : SkilledStrategy
{
    public FixedDisciplineStrategy() : base(shops: false) { }
    protected override bool FixedDiscipline => true;
    public override string Name => "fixed";
}
