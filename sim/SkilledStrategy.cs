using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// SKILLED — the G3 measuring stick (economy rework: median death ≥7, win 10–15% with items).
/// Estimates like a sharp, then sizes and times like one.
///
/// Estimation (p̂ per selection): pure two-way de-vig of the offered pair — NORMALIZE the
/// implied probs, for EVERY market kind. Because v0's book prices proportionally from truth,
/// this recovers true p exactly (the information axis returns in v2 with pricing noise; a
/// stats-signal estimator was tried and rejected in the F_0.4.0 P5 review — against a
/// truth-priced book it only manufactures phantom edges).
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
/// HONESTY: reads only public state — odds, displayed stats, bank/payment, offers, revealed
/// WinProbAfter, own items. Never Matchup.TrueHomeProb, Latents, StatLine, Matchup.Dist,
/// engine pricing helpers, Leg.TrueProb, or Matchup.Result.
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

    // Passive buy priority (highest first) over the FULL 15-passive catalog (rev 5 §13); the
    // dealt hand means the bot buys the best of what it is shown, tier-ranked. Bobblehead is
    // deliberately absent — it is handled by the flip rule (buy 2, sell 6: free money whenever
    // dealt), never held. Archetype bots override this list.
    private static readonly string[] DefaultRelicPriority =
    {
        RelicCatalog.MultiplierId, "longshot_photo", RelicCatalog.TotemId, "the_system",
        "chalk_eater", RelicCatalog.ScarTissueId, "whale_card", "bad_beat_jar",
        "iron_hands", "compd_suite", "rakes_rebate", "house_key",
        "golden_parachute", "the_collection",
    };

    private static readonly string[] ConsumablePriority =
        { "mulligan_slip", "refs_whistle", "free_bet", "profit_boost", "bookies_marker",
          "double_or_nothing", "ask_manager" };

    /// <summary>The tier list (archetype bots override).</summary>
    protected virtual string[] RelicPriorityList => DefaultRelicPriority;

    /// <summary>Comps the bot refuses to spend below (the Whale/Rebate hoard tension).</summary>
    protected virtual double CompsHoldFloor(Run run)
        => OwnsRelic(run, "whale_card") ? 20.0 : 0.0;

    /// <summary>Max parlay legs: a 4th leg when Comp'd Suite makes leg count pay.</summary>
    protected virtual int PrimaryLegCap(Run run)
        => OwnsRelic(run, "compd_suite") ? 4 : MaxPrimaryLegs;

    /// <summary>Archetype telemetry bots retain their historical moneyline-only identity.
    /// Skilled and its measurement variants use the public market board.</summary>
    protected virtual bool IncludesMarketOffers => true;

    /// <summary>Which locked contract modifier to attach to the primary ticket (one per ticket —
    /// the one-modifier law). DoN doubles the committed engine parlay; Free Bet insures everything
    /// else. The 0.30 win-prob gate is ML-era scaffolding (a 3-leg favorites plan sits ~0.34) —
    /// under the full board the sharp plays singles/pairs/triples by EV, and a +EV single at 0.6
    /// prob is a better DoN spot than a 3-leg parlay at 0.05. Gate on TICKET EV (win × odds − 1)
    /// instead: the modifier's value scales with the ticket's edge, not its shape.</summary>
    protected virtual TicketModifier PickModifier(Run run, double planWinProb, double planTicketEv)
    {
        if (run.OwnsConsumable("double_or_nothing") && planTicketEv > 0.0)
            return TicketModifier.DoubleOrNothing;
        if (run.OwnsConsumable("free_bet")) return TicketModifier.FreeBet;
        return TicketModifier.None;
    }

    /// <summary>The bot's engine test, dealt-hand aware (the tuning campaign's first finding):
    /// ANY owned product source is an engine — waiting for The Multiplier specifically starves
    /// the build 4 shops out of 5.</summary>
    protected static bool OwnsAnyEngine(Run run)
    {
        foreach (RelicDefinition d in run.OwnedRelics)
        {
            switch (d.Id)
            {
                case "the_multiplier":
                case "longshot_photo":
                case "the_system":
                case "chalk_eater":
                case "bad_beat_jar":
                case "iron_hands":
                case "whale_card":
                case "the_collection":
                case "house_key":
                    return true;
            }
        }
        return false;
    }

    protected static bool OwnsRelic(Run run, string id)
    {
        foreach (RelicDefinition d in run.OwnedRelics)
            if (d.Id == id) return true;
        return false;
    }

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
        public readonly MarketSelection Selection;
        public readonly double PHat;
        public readonly double Odds;
        public readonly double Ev;
        public Cand(int matchup, MarketSelection selection, double pHat, double odds)
        {
            Matchup = matchup; Selection = selection; PHat = pHat; Odds = odds;
            Ev = pHat * odds - 1.0;
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
            Cand? best = null;
            foreach (MarketOffer offer in m.Markets)
            {
                MarketSelection selection = offer.Selection;
                if (selection.Kind == MarketKind.AnytimeScorer) continue; // declared human-agency market
                if (!IncludesMarketOffers && selection.Kind != MarketKind.Moneyline) continue;

                double pHat = EstimateProbability(m, selection, state.HomeProbEst[m.Index], run.Config);
                state.MarketProbEst[(m.Index, selection)] = pHat;
                var candidate = new Cand(m.Index, selection, pHat, offer.Odds);
                if (best is null || candidate.Ev > best.Value.Ev) best = candidate;
            }
            if (best is { } chosen) cands.Add(chosen);
        }
        cands.Sort((a, b) => b.Ev.CompareTo(a.Ev)); // the sharp plays EDGES, not confidence —
        // under the F_0.4.0 board, sorting by pHat converges on chalk totals (0.8 prob, 1.18 odds)
        // which are high-confidence vigs with payout too thin to fund the payment schedule.
        // EV sort keeps the ML-era plan shape (3 legs ≈ 0.34 win, odds ≈ 4.4) by putting the
        // bot's genuine disagreements (small +EV edges at mid/long odds) at the top.

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
            // Survival: existing escalation logic against the payment itself. A held Free Bet
            // insures the rescue ticket — the exact spot the refund is worth the most.
            double aimMult = payment * ClearBuffer / bank;
            var rescue = ChooseTicket(cands, aimMult, run.Config.MaxStakeFraction);
            if (rescue is not { } r) return;
            double rf = Math.Clamp(ReqFrac(aimMult, r.Odds), StakeMin, run.Config.MaxStakeFraction);
            double rs = Math.Clamp(Math.Floor(rf * bank), run.Config.MinStake, bank);
            if (rs < run.Config.MinStake) return;
            TicketModifier rescueMod = run.OwnsConsumable("free_bet")
                ? TicketModifier.FreeBet : TicketModifier.None;
            run.PlaceTicket(r.Picks, rs, BoostLeg(run, r.Picks), rescueMod);
            return;
        }

        // Engine mode. (Campaign note: reserving toward the NEXT payment too was tried and
        // BACKFIRED — won 7.0% → 4.0%. In an income race, under-betting starves the engine;
        // the game punishes timidity. Reserve today's payment only.)
        double spare = bank - payment;
        if (spare < run.Config.MinStake) return;

        // Pick the ticket that maximizes the sharp's ACTUAL objective (F_0.4.0 P5 review):
        // the old code always parlayed the top-3 by Ev, but under the full board the best
        // edges are at mid/long odds and 3-legging them drops win prob to ~0.05 — the ticket
        // almost never wins and the rare win doesn't cover cumulative vig. ChooseTicket
        // considers singles, pairs, and triples and picks the combination whose win×odds
        // best clears the payment quota; that IS the engine ticket, not a fixed 3-legger.
        double engineAimMult = payment * ClearBuffer / spare;
        var plan = ChooseTicket(cands, engineAimMult, run.Config.MaxStakeFraction);
        if (plan is not { } p) return;
        var picks = p.Picks;
        double odds = p.Odds;
        double win = p.WinProb;

        // Size toward denting what's left of the schedule: a hit should cover ~half the remaining
        // payments, within [Kelly-lite floor, EngineStakeCap × spare].
        double remaining = 0;
        for (int r2 = run.Round - 1; r2 < run.PaymentSchedule.Count; r2++)
            remaining += run.PaymentSchedule[r2];
        // Pre-engine, the comps era (design/10 F): the bank is ~2–3 payments, so income is
        // mandatory NOW — size the parlay so a hit funds this payment plus a seed of the next
        // (quota sizing), which also pumps comp volume toward the engine. Post-engine, size
        // toward denting the remaining schedule.
        bool engined = OwnsAnyEngine(run) && picks.Count >= 3;
        double stake;
        if (FixedDiscipline)
        {
            // The G5 measurement bot (Allen-approved fix): stakes 25% of spare REGARDLESS of item
            // ownership, so pair-vs-solo deltas measure composition, not ownership-induced
            // aggression (round 1's confound: the engine tempts bigger bets and earlier deaths).
            stake = Math.Clamp(Math.Floor(0.25 * spare), run.Config.MinStake, Math.Floor(spare));
            if (stake < run.Config.MinStake) return;
        }
        else
        {
            // ENGINE MODE regardless of leg count (F_0.4.0 P5 review): the old pre-engine quota
            // sizing (payment + 0.5·next) was tuned for ML-era 3-leg parlays at odds ~4.4.
            // Under the full board the sharp plays singles/pairs at odds 2–3, and quota sizing
            // under-bets by 2–3× — the engine starves and dies at R5. The Multiplier and DoN
            // scale off PAYOUT, so a +EV single at odds 2.5 with the engine on is worth more
            // than a 3-leg parlay at odds 5 with it off. Size for the payout, not the quota.
            double targetPayout = engined ? 0.5 * remaining : payment + 0.5 * (run.NextPayment ?? 0.0);
            double stakeForTarget = targetPayout / odds;
            double cap = Math.Floor(EngineStakeCap * spare);
            if (cap < run.Config.MinStake) return; // spare too thin for an engine bet this round
            stake = Math.Clamp(Math.Floor(Math.Max(StakeMin * spare, stakeForTarget)),
                run.Config.MinStake, cap);
        }

        run.PlaceTicket(picks, stake, BoostLeg(run, picks), PickModifier(run, win, win * odds - 1.0));
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
            double o = m.Odds(picks[i].Selection);
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

    // Best clearing ticket by ESTIMATED TICKET EV (win × odds − 1 per unit stake), the sharp's
    // actual objective (F_0.4.0 P5 review). The old win-prob ranking preferred chalk singles and
    // 3-leg parlays by shape alone; EV ranking lets the math decide between a safe single at
    // +8% and a parlay at +50%, which is what the ML-era plan number 0.30 was the RESULT of,
    // never the target. Falls back to the widest top-EV parlay when nothing clears.
    private static Plan? ChooseTicket(List<Cand> cands, double aimMult, double escMax)
    {
        Plan? best = null;
        double bestEv = double.MinValue;

        foreach (Cand c in cands)
        {
            if (ReqFrac(aimMult, c.Odds) > escMax) continue;
            double ev = c.PHat * c.Odds - 1.0;
            if (ev > bestEv)
            {
                bestEv = ev;
                best = new Plan(new List<Pick> { new Pick(c.Matchup, c.Selection) }, c.Odds, c.PHat);
            }
        }

        double odds = 1.0, win = 1.0;
        var picks = new List<Pick>(3);
        for (int i = 0; i < Math.Min(MaxPrimaryLegs, cands.Count); i++)
        {
            odds *= cands[i].Odds;
            win *= cands[i].PHat;
            picks.Add(new Pick(cands[i].Matchup, cands[i].Selection));
            if (i + 1 < 2) continue; // only consider L >= 2 here
            double ticketEv = win * odds - 1.0;
            if (ReqFrac(aimMult, odds) <= escMax && ticketEv > bestEv)
            {
                bestEv = ticketEv;
                best = new Plan(new List<Pick>(picks), odds, win);
            }
        }

        if (best != null) return best;

        int legs = Math.Min(MaxPrimaryLegs, cands.Count);
        double o2 = 1.0, w2 = 1.0;
        var p2 = new List<Pick>(legs);
        for (int i = 0; i < legs; i++) { o2 *= cands[i].Odds; w2 *= cands[i].PHat; p2.Add(new Pick(cands[i].Matchup, cands[i].Selection)); }
        return new Plan(p2, o2, w2);
    }

    private static double PlanOdds(List<Cand> picks)
    {
        double o = 1.0;
        foreach (Cand c in picks) o *= c.Odds;
        return o;
    }

    public virtual bool ShouldCashOut(Run run, Ticket ticket, SweatSession session, DramaEvent evt,
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

        // The Bobblehead flip: buy at 2, sell at 6 — free comps whenever it is dealt and a
        // slot can host it for a moment (the shop-flipper baseline every sharp would run).
        for (int i = 0; i < run.ShopOffers.Count; i++)
        {
            if (run.ShopOffers[i].Id != "bobblehead") continue;
            if (run.ShopOffers[i].Price > run.Comps) break;
            if (run.OwnedRelics.Count >= run.Config.RelicSlots) break;
            run.BuyRelic(i);
            for (int j = 0; j < run.OwnedRelics.Count; j++)
                if (run.OwnedRelics[j].Id == "bobblehead") { run.SellRelic(j); break; }
            break;
        }

        double holdFloor = CompsHoldFloor(run);
        string[] priority = RelicPriorityList;

        // Buy discipline (tuning campaign finding #2): buying the best of a BAD hand burns the
        // comps an engine needs. Pre-engine, only top-tier items are worth a slot; afterwards
        // mid-tier joins; the bottom of the list is a luxury for a rich late bank.
        bool engined = OwnsAnyEngine(run);
        int cutoff = !engined ? 6 : run.Comps >= 15.0 ? int.MaxValue : 10;

        bool bought = true;
        while (bought)
        {
            bought = false;

            // Best-ranked affordable passive in the dealt hand (respecting the hoard floor).
            int buyIndex = -1;
            int bestRank = int.MaxValue;
            for (int i = 0; i < run.ShopOffers.Count; i++)
            {
                RelicDefinition o = run.ShopOffers[i];
                if (o.Price > run.Comps - holdFloor) continue;
                int rank = RankOf(priority, o.Id);
                if (rank >= cutoff) continue;
                if (rank < bestRank) { bestRank = rank; buyIndex = i; }
            }

            if (buyIndex >= 0)
            {
                if (run.OwnedRelics.Count < run.Config.RelicSlots)
                {
                    run.BuyRelic(buyIndex);
                    bought = true;
                    continue;
                }

                // Replacement (rev 5 §13): slots full — sell the worst-ranked owned passive
                // when the dealt item ranks strictly better and the trade nets out affordable.
                int worstOwned = -1, worstRank = -1;
                for (int j = 0; j < run.OwnedRelics.Count; j++)
                {
                    string id = run.OwnedRelics[j].Id;
                    if (id == RelicCatalog.TotemId) continue; // never resellable value
                    int rank = RankOf(priority, id);
                    if (rank > worstRank) { worstRank = rank; worstOwned = j; }
                }
                RelicDefinition dealt = run.ShopOffers[buyIndex];
                if (worstOwned >= 0 && bestRank < worstRank
                    && run.Comps + run.GetResaleValue(run.OwnedRelics[worstOwned]) - holdFloor >= dealt.Price)
                {
                    run.SellRelic(worstOwned);
                    // Offer indexes are untouched by a sell; re-find the dealt item defensively.
                    for (int i = 0; i < run.ShopOffers.Count; i++)
                        if (run.ShopOffers[i].Id == dealt.Id) { run.BuyRelic(i); break; }
                    bought = true;
                    continue;
                }
            }

            if (run.OwnedConsumables.Count < run.Config.ConsumableSlots)
            {
                int cIndex = -1;
                int cRank = int.MaxValue;
                for (int i = 0; i < run.ConsumableOffers.Count; i++)
                {
                    ConsumableDefinition o = run.ConsumableOffers[i];
                    if (o.Price > run.Comps - holdFloor) continue;
                    int rank = RankOf(ConsumablePriority, o.Id);
                    if (rank < cRank) { cRank = rank; cIndex = i; }
                }
                if (cIndex >= 0) { run.BuyConsumable(cIndex); bought = true; }
            }
        }
    }

    // ---- estimation helpers (public info only) ----

    /// <summary>The sharp's edge: it reads the DISPLAYED TeamStats signal (public info) and
    /// estimates latent rates with the OPTIMAL posterior mean under the signal's noise model —
    /// shrinkage factor = spreadVar / (spreadVar + noiseVar), which for the tuned dials is ~0.75
    /// (trust the signal 75%, shrink 25% toward the base rate). This is deliberately NOT de-vig:
    /// the book prices at the true latent, and a weaker (noisier) estimator finds genuine
    /// disagreement ~30% of the time with ~6% RMSE vs 5% vig — small edges, real edges.
    /// Moneyline stays de-vigged (records are its signal; pHome is the public estimate).</summary>
    private static double EstimateProbability(Matchup matchup, MarketSelection selection, double pHome, RunConfig config)
    {
        if (selection.Kind == MarketKind.Moneyline)
            return selection.Choice == MarketChoice.Home ? pHome : 1.0 - pHome;

        TeamStats home = matchup.HomeStats;
        TeamStats away = matchup.AwayStats;
        double homeGoals = Shrink(home.GoalsFor, config.BaseGoalRate, config);
        double awayGoals = Shrink(away.GoalsFor, config.BaseGoalRate, config);
        double homeCorners = Shrink(home.Corners, config.BaseCornerRate, config);
        double awayCorners = Shrink(away.Corners, config.BaseCornerRate, config);
        double homeCards = Shrink(home.Cards, config.BaseCardRate, config);
        double awayCards = Shrink(away.Cards, config.BaseCardRate, config);

        switch (selection.Kind)
        {
            case MarketKind.TotalGoals:
            {
                double over = ScoreGridProbability(homeGoals, awayGoals, pHome, config.MaxGoalsGrid,
                    (h, a) => h + a > selection.Line);
                return selection.Choice == MarketChoice.Over ? over : 1.0 - over;
            }
            case MarketKind.BothTeamsToScore:
            {
                double yes = ScoreGridProbability(homeGoals, awayGoals, pHome, config.MaxGoalsGrid,
                    (h, a) => h >= 1 && a >= 1);
                return selection.Choice == MarketChoice.Yes ? yes : 1.0 - yes;
            }
            case MarketKind.TotalCorners:
            {
                double over = CountGridProbability(homeCorners, awayCorners, config.MaxCornerGrid, selection.Line);
                return selection.Choice == MarketChoice.Over ? over : 1.0 - over;
            }
            case MarketKind.TotalCards:
            {
                double over = CountGridProbability(homeCards, awayCards, config.MaxCardGrid, selection.Line);
                return selection.Choice == MarketChoice.Over ? over : 1.0 - over;
            }
            default:
                throw new ArgumentException($"Bots do not price {selection.Kind}");
        }
    }

    /// <summary>Posterior mean of the latent rate given the noisy displayed signal, with a
    /// deliberate EXTRA shrinkage factor (< 1.0) on top of the MMSE optimal. The raw MMSE
    /// (75% trust) finds +17-29% phantom edges at the extremes — the signal is noisier than
    /// the theoretical model because the prior is narrower conditional on the matchup (teams
    /// with different pHome have correlated rates). TrustDamp shrinks toward the book's price
    /// (base rate) more aggressively so the bot's estimates stay near-truth except when the
    /// signal is genuinely strong; it is a sim-side dial, never a RunConfig knob.</summary>
    private const double TrustDamp = 0.30;

    private static double Shrink(double displayed, double baseRate, RunConfig config)
    {
        double spread = baseRate == config.BaseGoalRate ? config.GoalTempoSpread
            : baseRate == config.BaseCornerRate ? config.CornerTempoSpread
            : config.DisciplineSpread;
        double spreadVar = baseRate * baseRate * spread * spread / 3.0;
        double sigma = config.SignalNoise * baseRate / Math.Sqrt(config.PriorGames);
        double noiseVar = sigma * sigma / 3.0;
        double trust = spreadVar / (spreadVar + noiseVar) * TrustDamp;
        return baseRate + (displayed - baseRate) * trust;
    }

    // The engine's v1 score process conditions the no-draw score grid on the winner, so the
    // bettor mixes its own two conditional grids by its public moneyline estimate. Deliberately
    // local strategy math, not a call into MatchModel — the bot prices from PUBLIC signal only.
    private static double ScoreGridProbability(double homeRate, double awayRate, double pHome,
        int maxGoals, Func<int, int, bool> predicate)
    {
        double homeMass = 0.0, awayMass = 0.0;
        double homeHit = 0.0, awayHit = 0.0;
        for (int h = 0; h <= maxGoals; h++)
        for (int a = 0; a <= maxGoals; a++)
        {
            if (h == a) continue;
            double mass = PoissonPmf(h, homeRate) * PoissonPmf(a, awayRate);
            if (h > a)
            {
                homeMass += mass;
                if (predicate(h, a)) homeHit += mass;
            }
            else
            {
                awayMass += mass;
                if (predicate(h, a)) awayHit += mass;
            }
        }
        return pHome * homeHit / homeMass + (1.0 - pHome) * awayHit / awayMass;
    }

    private static double CountGridProbability(double homeRate, double awayRate, int max, double line)
    {
        double[] home = RawPoisson(homeRate, max, out double homeTotal);
        double[] away = RawPoisson(awayRate, max, out double awayTotal);
        double over = 0.0;
        for (int h = 0; h < home.Length; h++)
        for (int a = 0; a < away.Length; a++)
            if (h + a > line) over += home[h] / homeTotal * (away[a] / awayTotal);
        return over;
    }

    private static double[] RawPoisson(double rate, int max, out double total)
    {
        var terms = new double[max + 1];
        total = 0.0;
        for (int i = 0; i <= max; i++)
        {
            terms[i] = PoissonPmf(i, rate);
            total += terms[i];
        }
        return terms;
    }

    private static double PoissonPmf(int k, double rate)
    {
        double p = Math.Exp(-rate);
        for (int i = 1; i <= k; i++) p *= rate / i;
        return p;
    }

    private static MarketSelection Opposite(MarketSelection s) => s.Kind switch
    {
        MarketKind.TotalGoals => MarketSelection.TotalGoals(s.Line, s.Choice != MarketChoice.Over),
        MarketKind.TotalCorners => MarketSelection.TotalCorners(s.Line, s.Choice != MarketChoice.Over),
        MarketKind.TotalCards => MarketSelection.TotalCards(s.Line, s.Choice != MarketChoice.Over),
        MarketKind.BothTeamsToScore => MarketSelection.BothTeamsToScore(s.Choice != MarketChoice.Yes),
        _ => throw new ArgumentException($"Bots do not price {s.Kind}"),
    };

    private static double DevigHome(Matchup m)
    {
        double implHome = 1.0 / m.HomeOdds;
        double implAway = 1.0 / m.AwayOdds;
        return implHome / (implHome + implAway);
    }

    private static double PHat(BotState state, Leg leg)
    {
        if (state.MarketProbEst.TryGetValue((leg.Matchup.Index, leg.Selection), out double pHat))
            return pHat;
        int idx = leg.Matchup.Index;
        double pHome = state.HomeProbEst.TryGetValue(idx, out double v) ? v : DevigHome(leg.Matchup);
        return EstimateProbability(leg.Matchup, leg.Selection, pHome, leg.Matchup.ModelConfig ?? new RunConfig());
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
    /// <summary>G2 measures "skilled without items" on the moneyline surface — the same
    /// reference arm G3 is banded against. Widening THIS bot to the whole board silently
    /// redefines both gates (F_0.4.0 P5 review: one inheritance default changed the meaning
    /// of the campaign; stay moneyline like the archetypes).</summary>
    protected override bool IncludesMarketOffers => false;
}

/// <summary>The G5 measurement bot: skilled shopping OFF, stake discipline FIXED — granted-item
/// batches through this bot isolate what the items DO from how ownership changes behavior.</summary>
public sealed class FixedDisciplineStrategy : SkilledStrategy
{
    public FixedDisciplineStrategy() : base(shops: false) { }
    protected override bool FixedDiscipline => true;
    public override string Name => "fixed";
    /// <summary>The G5 measurement bot: its granted-item deltas must isolate what the items DO
    /// from how the betting surface moves — same moneyline surface as the banded reference
    /// arms (F_0.4.0 P5 review).</summary>
    protected override bool IncludesMarketOffers => false;
}
