using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// Drives one fully-seeded run end to end (betting → sweat → settle → shop → …), invoking the
/// strategy's hooks and recording metrics. The engine owns all state; this only sequences its
/// transitions with instrumentation.
///
/// Charm-expansion behaviors (PLAN.md rev 5): the pending-loss window consults the strategy's
/// ChoosePendingLossAction (Mulligan / Whistle / Decline — legality enforced here); the audit
/// policies for shop-time actives run here (a granted Manager is used every visit, a granted
/// Marker plays at cliff rounds, a granted Bobblehead is sold at the next shop); per-item
/// events (offered / bought / sold / used) feed the conversion telemetry.
///
/// The bot's own randomness is a Pcg32 seeded from the run seed, so a run is fully reproducible
/// and independent of every other run — the property parallelism relies on.
/// </summary>
public static class RunPlayer
{
    // A fixed, distinct stream id for the bot generator so it never collides with an engine stream.
    private static readonly ulong BotStream = RngHub.Fnv1a64("sim-bot");

    /// <summary>The Marker plays when the payment is THREATENED (bank below this multiple of it
    /// at the round's open) — need-based beats cliff-based: relief at a comfortable cliff lands
    /// on runs that were surviving anyway (campaign finding, iteration 14).</summary>
    public const double MarkerNeedRatio = 1.3;

    public static RunResult Play(IStrategy strat, string seed, RunConfig cfg,
        string[]? grantedRelics = null, string? grantedConsumable = null)
    {
        var run = new Run(seed, cfg);
        if (grantedRelics is { Length: > 0 })
            ItemGrant.GrantRelics(run, grantedRelics);

        var rng = new Pcg32(RngHub.Fnv1a64(seed + ":bot"), BotStream);
        var state = new BotState();
        var result = new RunResult();
        bool bobbleheadPending = grantedRelics != null && Array.IndexOf(grantedRelics, "bobblehead") >= 0;

        while (true)
        {
            if (grantedConsumable != null)
            {
                bool refilled = !run.OwnsConsumable(grantedConsumable);
                ItemGrant.RefillConsumable(run, grantedConsumable);
                if (refilled) result.Events(grantedConsumable).Acquired++;
            }
            if (run.LastGift != null)
            {
                result.GiftsReceived++;
                result.Events(run.LastGift.Id).Acquired++;
            }

            var rm = new RoundMetrics { Round = run.Round, BankAtStart = run.Bank };

            // Marker audit/bot policy: play when the payment is threatened.
            if (run.OwnsConsumable("bookies_marker")
                && run.Bank < MarkerNeedRatio * run.CurrentPayment)
            {
                run.PlayBookiesMarker();
                result.Events("bookies_marker").Used++;
                rm.ConsumablesPlayed++;
            }

            state.NewRound();
            Dictionary<string, int> heldBeforeBet = CountIds(run.OwnedConsumables);
            strat.Bet(run, state, rng);
            RecordUses(result, heldBeforeBet, CountIds(run.OwnedConsumables), rm);

            rm.TicketsPlaced = run.Tickets.Count;
            foreach (Ticket t in run.Tickets)
            {
                rm.TotalStaked += t.Stake;
                rm.TicketEvsAtLock.Add(Metrics.TrueTicketEvAtLock(t));
                rm.TicketPassiveEvsAtLock.Add(Metrics.TruePassiveOnlyEvAtLock(t));
                RecordMarketPlacement(rm, t);
            }

            run.LockRound();

            // cashoutByTicket[i] holds the offer taken on ticket i (0 if none), for swing scoring.
            var cashoutByTicket = new double[run.Tickets.Count];
            if (strat.ControlsSweat)
                PlaySweatWithControl(run, strat, state, rng, rm, result, cashoutByTicket);
            else
                run.FastForwardRound(); // naive: never cashes out; pending windows auto-decline

            RecordMarketRealization(rm, run.Tickets);
            ScoreSwings(run, rm, cashoutByTicket);
            result.BiggestSwing = Math.Max(result.BiggestSwing, rm.BiggestSwing);

            double scarBefore = run.ScarStacks;
            run.Settle();
            result.MaxScarStacks = Math.Max(result.MaxScarStacks, run.ScarStacks);
            if (scarBefore > 0 && run.ScarStacks == 0) result.ScarBurns++; // carrier realized this round

            SettlementReport settle = run.LastSettlement!.Value;
            if (settle.TotemFired) result.TotemFires++;
            result.Rounds.Add(rm);

            if (run.Phase == Phase.RunLost)
            {
                result.DeathRound = run.Round;
                result.Won = false;
                // Close-call deaths (report metric): the bank was within 20% of the missed payment.
                result.CloseCallDeath = settle.Shortfall <= 0.20 * settle.Payment;
                break;
            }
            if (run.Phase == Phase.RunWon)
            {
                result.DeathRound = run.Config.Rounds + 1;
                result.Won = true;
                break;
            }

            // Phase.Shop — conversion telemetry sees the dealt hand first.
            foreach (RelicDefinition o in run.ShopOffers) result.Events(o.Id).Offered++;
            foreach (ConsumableDefinition o in run.ConsumableOffers) result.Events(o.Id).Offered++;

            // Bobblehead audit policy: granted → sold at the first shop (its honest use).
            if (bobbleheadPending)
            {
                for (int i = 0; i < run.OwnedRelics.Count; i++)
                {
                    if (run.OwnedRelics[i].Id != "bobblehead") continue;
                    run.SellRelic(i);
                    result.Events("bobblehead").Sold++;
                    result.Events("bobblehead").Used++;
                    bobbleheadPending = false;
                    break;
                }
            }

            // Manager audit/bot policy: a held Manager is used every visit, before shopping.
            if (run.OwnsConsumable("ask_manager"))
            {
                run.PlayAskManager();
                result.Events("ask_manager").Used++;
                rm.ConsumablesPlayed++;
                foreach (RelicDefinition o in run.ShopOffers) result.Events(o.Id).Offered++;
                foreach (ConsumableDefinition o in run.ConsumableOffers) result.Events(o.Id).Offered++;
            }

            Dictionary<string, int> relicsBefore = CountIds(run.OwnedRelics);
            Dictionary<string, int> heldBeforeShop = CountIds(run.OwnedConsumables);
            int ownedBefore = run.OwnedRelics.Count + run.OwnedConsumables.Count;
            strat.Shop(run, state, rng);
            rm.Buys = run.OwnedRelics.Count + run.OwnedConsumables.Count - ownedBefore;
            RecordTrades(result, relicsBefore, CountIds(run.OwnedRelics));
            RecordTrades(result, heldBeforeShop, CountIds(run.OwnedConsumables));

            run.ExitShop();
        }

        result.FinalBank = run.Bank;
        foreach (RelicDefinition d in run.OwnedRelics)
            result.RelicsAtDeath.Add(d.Id);
        foreach (EffectStat s in run.EffectStates)
            result.FinalEffectStates[s.Id] = s.Value;
        foreach (RoundMetrics r in result.Rounds)
            result.TotalDecisions += r.Decisions;

        return result;
    }

    private static void PlaySweatWithControl(Run run, IStrategy strat, BotState state, Pcg32 rng,
        RoundMetrics rm, RunResult result, double[] cashoutByTicket)
    {
        var sweats = run.Sweats;
        for (int i = 0; i < sweats.Count; i++)
        {
            SweatSession session = sweats[i];
            Ticket ticket = run.Tickets[i];
            while (true)
            {
                bool moved = session.MoveNext(out DramaEvent? evt);

                // The pending-loss window (rev 5 §13): the strategy chooses; legality is
                // enforced here and an illegal choice falls back to Decline.
                if (session.HasPendingLoss)
                {
                    PendingLossAction action = strat.ChoosePendingLossAction(run, ticket, session, state, rng);
                    if (action == PendingLossAction.Mulligan
                        && run.OwnsConsumable("mulligan_slip") && session.CanMulliganPendingLoss)
                    {
                        run.PlayMulliganSlip(session);
                        rm.MulligansPlayed++;
                        result.Events("mulligan_slip").Used++;
                        continue;
                    }
                    if (action == PendingLossAction.Whistle && run.OwnsConsumable("refs_whistle"))
                    {
                        run.PlayRefsWhistle(session);
                        rm.WhistlesPlayed++;
                        result.Events("refs_whistle").Used++;
                        if (!session.IsComplete) continue; // overturned — the sweat lives
                        break;                             // confirmed — busted
                    }
                    session.DeclinePendingLoss();
                    break;
                }

                if (!moved) break;

                double? offer = session.CashOutOffer();
                if (offer is not { } o) continue;

                // The payment model: the settle deducts CurrentPayment, so that is the bot's target.
                if (strat.ShouldCashOut(run, ticket, session, evt!, o, run.Bank, run.CurrentPayment, state, rng))
                {
                    session.AcceptCashOut();
                    rm.CashOutsCount++;
                    rm.CashOutsTotal += o;
                    cashoutByTicket[i] = o;
                    break;
                }
            }
        }
        run.FinishSweat();
    }

    // Swing = the biggest single-ticket money movement this round: a won ticket's gross payout, the
    // cash-out taken, or the stake lost on a bust. States are final once the sweat has finished.
    private static void ScoreSwings(Run run, RoundMetrics rm, double[] cashoutByTicket)
    {
        for (int i = 0; i < run.Tickets.Count; i++)
        {
            Ticket t = run.Tickets[i];
            double swing = t.State switch
            {
                TicketState.Won => t.PotentialPayout,
                TicketState.CashedOut => cashoutByTicket[i],
                _ => t.Stake, // Lost
            };
            if (swing > rm.BiggestSwing) rm.BiggestSwing = swing;
        }
    }

    private static void RecordMarketPlacement(RoundMetrics rm, Ticket ticket)
    {
        if (ticket.Legs.Count == 0) return;
        double stakePerLeg = ticket.Stake / ticket.Legs.Count;
        foreach (Leg leg in ticket.Legs)
        {
            MarketExposure exposure = Exposure(rm, leg.Selection.Kind);
            exposure.LegsPlaced++;
            if (leg.Selection.Choice == MarketChoice.Draw) exposure.DrawLegs++;
            exposure.Stake += stakePerLeg;
        }
    }

    private static void RecordMarketRealization(RoundMetrics rm, IReadOnlyList<Ticket> tickets)
    {
        foreach (Ticket ticket in tickets)
        {
            if (ticket.Legs.Count == 0) continue;
            double stakePerLeg = ticket.Stake / ticket.Legs.Count;
            foreach (Leg leg in ticket.Legs)
            {
                // A void returns its allocated stake; the other market legs retain their own
                // observable result. This is intentionally before parlay/item/cash-out effects.
                if (leg.IsVoided) continue;
                MarketExposure exposure = Exposure(rm, leg.Selection.Kind);
                double unitNet = leg.GradesWon ? leg.OfferedOdds - 1.0 : -1.0;
                exposure.RealizedNet += stakePerLeg * unitNet;
                exposure.RealizedNetUnit += unitNet;
            }
        }
    }

    private static MarketExposure Exposure(RoundMetrics rm, MarketKind kind)
    {
        if (!rm.MarketExposure.TryGetValue(kind, out MarketExposure? exposure))
            rm.MarketExposure[kind] = exposure = new MarketExposure();
        return exposure;
    }

    // ---- per-item event accounting (rev 5 §16) ----

    private static Dictionary<string, int> CountIds(IReadOnlyList<ConsumableDefinition> items)
    {
        var map = new Dictionary<string, int>();
        foreach (ConsumableDefinition c in items)
            map[c.Id] = map.TryGetValue(c.Id, out int n) ? n + 1 : 1;
        return map;
    }

    private static Dictionary<string, int> CountIds(IReadOnlyList<RelicDefinition> items)
    {
        var map = new Dictionary<string, int>();
        foreach (RelicDefinition r in items)
            map[r.Id] = map.TryGetValue(r.Id, out int n) ? n + 1 : 1;
        return map;
    }

    /// <summary>Bet-phase diffs are USES (boost/free bet/DoN consumed at placement).</summary>
    private static void RecordUses(RunResult result, Dictionary<string, int> before,
        Dictionary<string, int> after, RoundMetrics rm)
    {
        foreach ((string id, int n) in before)
        {
            int now = after.TryGetValue(id, out int v) ? v : 0;
            if (now < n)
            {
                result.Events(id).Used += n - now;
                rm.ConsumablesPlayed += n - now;
            }
        }
    }

    /// <summary>Shop-phase diffs are TRADES: gained = bought, lost = sold.</summary>
    private static void RecordTrades(RunResult result, Dictionary<string, int> before,
        Dictionary<string, int> after)
    {
        foreach ((string id, int n) in after)
        {
            int was = before.TryGetValue(id, out int v) ? v : 0;
            if (n > was)
            {
                result.Events(id).Bought += n - was;
                result.Events(id).Acquired += n - was;
            }
        }
        foreach ((string id, int n) in before)
        {
            int now = after.TryGetValue(id, out int v) ? v : 0;
            if (now < n) result.Events(id).Sold += n - now;
        }
    }
}
