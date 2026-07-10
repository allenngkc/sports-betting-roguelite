using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// Drives one fully-seeded run end to end (betting → sweat → settle → shop → …), invoking the
/// strategy's hooks and recording metrics. The engine owns all state; this only sequences its
/// transitions, exactly like game-console's AutoRun but with a pluggable bot and instrumentation.
///
/// The bot's own randomness is a Pcg32 seeded from the run seed (RngHub.Fnv1a64(seed + ":bot")), so a
/// run is fully reproducible and independent of every other run — the property parallelism relies on.
/// </summary>
public static class RunPlayer
{
    // A fixed, distinct stream id for the bot generator so it never collides with an engine stream.
    private static readonly ulong BotStream = RngHub.Fnv1a64("sim-bot");

    public static RunResult Play(IStrategy strat, string seed, RunConfig cfg, string[]? grantedRelics = null)
    {
        var run = new Run(seed, cfg);
        if (grantedRelics is { Length: > 0 })
            RelicGrant.Grant(run, grantedRelics);

        var rng = new Pcg32(RngHub.Fnv1a64(seed + ":bot"), BotStream);
        var state = new BotState();
        var result = new RunResult();

        while (true)
        {
            var rm = new RoundMetrics { Round = run.Round, BankAtStart = run.Bank };

            state.NewRound();
            strat.Bet(run, state, rng);
            rm.SharpEyeUses = state.SharpEyeUses;

            rm.TicketsPlaced = run.Tickets.Count;
            foreach (Ticket t in run.Tickets)
            {
                rm.TotalStaked += t.Stake;
                rm.TicketEvsAtLock.Add(Metrics.TrueTicketEvAtLock(t, run.OwnedRelics));
            }

            run.LockRound();

            // cashoutByTicket[i] holds the offer taken on ticket i (0 if none), for swing scoring.
            var cashoutByTicket = new double[run.Tickets.Count];
            if (strat.ControlsSweat)
                PlaySweatWithControl(run, strat, state, rng, rm, cashoutByTicket);
            else
                run.FastForwardRound(); // naive: never cashes out

            ScoreSwings(run, rm, cashoutByTicket);
            result.BiggestSwing = Math.Max(result.BiggestSwing, rm.BiggestSwing);

            run.Settle();
            result.Rounds.Add(rm);

            if (run.Phase == Phase.RunLost)
            {
                result.DeathRound = run.Round;
                result.Won = false;
                break;
            }
            if (run.Phase == Phase.RunWon)
            {
                result.DeathRound = 9;
                result.Won = true;
                break;
            }

            // Phase.Shop
            int ownedBefore = run.OwnedRelics.Count;
            strat.Shop(run, state, rng);
            rm.Buys = run.OwnedRelics.Count - ownedBefore;
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
            while (session.MoveNext(out DramaEvent? evt))
            {
                double? offer = session.CashOutOffer();
                if (offer is not { } o) continue;

                if (strat.ShouldCashOut(run, ticket, session, evt!, o, run.Bank, run.CurrentTarget, state, rng))
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
