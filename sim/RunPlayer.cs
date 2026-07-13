using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// Drives one fully-seeded run end to end (betting → sweat → settle → shop → …), invoking the
/// strategy's hooks and recording metrics. The engine owns all state; this only sequences its
/// transitions with instrumentation.
///
/// Economy-rework behaviors: a held Mulligan Slip is ALWAYS played when a sweat suspends in the
/// pending-loss window (the documented bot policy — timing skill is a human affordance the bots
/// approximate greedily); Timeout is never bot-played (playtest-gated, PLAN.md); a granted audit
/// consumable is refilled each round via <see cref="ItemGrant.RefillConsumable"/>. Totem fires,
/// scar telemetry, gifts and close-call deaths are read from engine telemetry after each settle.
///
/// The bot's own randomness is a Pcg32 seeded from the run seed, so a run is fully reproducible
/// and independent of every other run — the property parallelism relies on.
/// </summary>
public static class RunPlayer
{
    // A fixed, distinct stream id for the bot generator so it never collides with an engine stream.
    private static readonly ulong BotStream = RngHub.Fnv1a64("sim-bot");

    public static RunResult Play(IStrategy strat, string seed, RunConfig cfg,
        string[]? grantedRelics = null, string? grantedConsumable = null)
    {
        var run = new Run(seed, cfg);
        if (grantedRelics is { Length: > 0 })
            ItemGrant.GrantRelics(run, grantedRelics);

        var rng = new Pcg32(RngHub.Fnv1a64(seed + ":bot"), BotStream);
        var state = new BotState();
        var result = new RunResult();

        while (true)
        {
            if (grantedConsumable != null)
                ItemGrant.RefillConsumable(run, grantedConsumable);
            if (run.LastGift != null)
                result.GiftsReceived++;

            var rm = new RoundMetrics { Round = run.Round, BankAtStart = run.Bank };

            state.NewRound();
            strat.Bet(run, state, rng);

            rm.TicketsPlaced = run.Tickets.Count;
            foreach (Ticket t in run.Tickets)
            {
                rm.TotalStaked += t.Stake;
                rm.TicketEvsAtLock.Add(Metrics.TrueTicketEvAtLock(t));
            }

            run.LockRound();

            // cashoutByTicket[i] holds the offer taken on ticket i (0 if none), for swing scoring.
            var cashoutByTicket = new double[run.Tickets.Count];
            if (strat.ControlsSweat)
                PlaySweatWithControl(run, strat, state, rng, rm, cashoutByTicket);
            else
                run.FastForwardRound(); // naive: never cashes out; pending windows auto-decline

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

            // Phase.Shop
            int ownedBefore = run.OwnedRelics.Count + run.OwnedConsumables.Count;
            strat.Shop(run, state, rng);
            rm.Buys = run.OwnedRelics.Count + run.OwnedConsumables.Count - ownedBefore;
            run.ExitShop();
        }

        result.FinalBank = run.Bank;
        foreach (RelicDefinition d in run.OwnedRelics)
            result.RelicsAtDeath.Add(d.Id);
        foreach (RoundMetrics r in result.Rounds)
            result.TotalDecisions += r.Decisions;

        return result;
    }

    private static void PlaySweatWithControl(Run run, IStrategy strat, BotState state, Pcg32 rng,
        RoundMetrics rm, double[] cashoutByTicket)
    {
        var sweats = run.Sweats;
        for (int i = 0; i < sweats.Count; i++)
        {
            SweatSession session = sweats[i];
            Ticket ticket = run.Tickets[i];
            while (true)
            {
                bool moved = session.MoveNext(out DramaEvent? evt);

                // The pending-loss window: bot policy is greedy — a held slip is always played.
                // (MoveNext past the window would decline it; the play resumes the same session.)
                if (session.HasPendingLoss && run.OwnsConsumable("mulligan_slip"))
                {
                    run.PlayMulliganSlip(session);
                    rm.MulligansPlayed++;
                    continue;
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
}
