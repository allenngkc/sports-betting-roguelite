using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// Per-round metrics for one run. Collected by the harness, which — unlike the bots — MAY read
/// engine truth (Matchup.TrueHomeProb, Leg.TrueProb, ticket states) to score outcomes and EV.
/// </summary>
public sealed class RoundMetrics
{
    public int Round;
    public double BankAtStart;
    public int TicketsPlaced;
    public double TotalStaked;

    /// <summary>True EV of each ticket measured at lock (see <see cref="Metrics.TrueTicketEvAtLock"/>).</summary>
    public readonly List<double> TicketEvsAtLock = new();

    public int CashOutsCount;
    public double CashOutsTotal;

    /// <summary>Largest single-ticket money swing this round (won payout / cash-out taken / stake lost).</summary>
    public double BiggestSwing;

    public int Buys;

    /// <summary>Player-facing decisions attributable to this round: tickets + cash-outs
    /// + purchases in this round's shop.</summary>
    public int Decisions => TicketsPlaced + CashOutsCount + Buys;
}

/// <summary>Everything the report needs from a single seeded run.</summary>
public sealed class RunResult
{
    /// <summary>Round the run died in (1..8), or 9 when all eight rounds were cleared.</summary>
    public int DeathRound;
    public bool Won;
    public double FinalBank;

    public readonly List<RoundMetrics> Rounds = new();
    public readonly List<string> RelicsAtDeath = new();

    /// <summary>Largest single-ticket swing across the whole run.</summary>
    public double BiggestSwing;

    public int TotalDecisions;

    /// <summary>Debt-as-HP events: how many times the bookie floated this run (a clean miss borrowed).</summary>
    public int FloatsTaken;

    /// <summary>True when the run died owing the bookie (missed a settle while indebted). False deaths
    /// are plain final-round misses — the only other way to lose under debt-as-HP.</summary>
    public bool DiedInDebt;
}

/// <summary>
/// Truth-reading scoring helpers. These are the ONE place true probabilities are read for scoring;
/// the strategy bots must never call into engine truth (see the honesty rule in each bot file).
/// </summary>
public static class Metrics
{
    /// <summary>
    /// True expected value of a ticket at lock: stake × (Π p_true × Π o_offered × payoutMult − 1),
    /// plus the sequential early-payout partial EV when the run owns early_payout
    /// (b × (p1 + p1·p2 + …), b = fractionOfStake × stake). Uses true probs — harness-only.
    /// </summary>
    public static double TrueTicketEvAtLock(Ticket ticket, IReadOnlyList<RelicDefinition> owned)
    {
        double pProd = 1.0;
        double oProd = 1.0;
        foreach (Leg leg in ticket.Legs)
        {
            pProd *= leg.Matchup.TrueProb(leg.Side); // truth: harness scoring only
            oProd *= leg.OfferedOdds;
        }

        double ev = ticket.Stake * (pProd * oProd * ticket.PayoutMultiplier - 1.0);
        ev += EarlyPayoutEv(ticket, owned);
        return ev;
    }

    private static double EarlyPayoutEv(Ticket ticket, IReadOnlyList<RelicDefinition> owned)
    {
        RelicDefinition? ep = null;
        foreach (RelicDefinition d in owned)
            if (d.Id == "early_payout") { ep = d; break; }
        if (ep == null) return 0.0;

        double fraction = ep.Params.TryGetValue("fractionOfStake", out double f) ? f : 0.0;
        double b = fraction * ticket.Stake;
        double cumulative = 1.0;
        double ev = 0.0;
        foreach (Leg leg in ticket.Legs)
        {
            cumulative *= leg.Matchup.TrueProb(leg.Side); // truth: harness scoring only
            ev += b * cumulative;
        }
        return ev;
    }
}
