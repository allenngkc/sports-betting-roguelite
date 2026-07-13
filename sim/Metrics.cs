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

    /// <summary>Mulligan Slips played in this round's sweats (a real player decision).</summary>
    public int MulligansPlayed;

    /// <summary>Largest single-ticket money swing this round (won payout / cash-out taken / stake lost).</summary>
    public double BiggestSwing;

    public int Buys;

    /// <summary>Player-facing decisions attributable to this round: tickets + cash-outs
    /// + slips played + purchases in this round's shop.</summary>
    public int Decisions => TicketsPlaced + CashOutsCount + MulligansPlayed + Buys;
}

/// <summary>Everything the report needs from a single seeded run.</summary>
public sealed class RunResult
{
    /// <summary>Round the run died in (1..Rounds), or Rounds+1 when every payment was made.</summary>
    public int DeathRound;
    public bool Won;
    public double FinalBank;

    public readonly List<RoundMetrics> Rounds = new();
    public readonly List<string> RelicsAtDeath = new();

    /// <summary>Largest single-ticket swing across the whole run.</summary>
    public double BiggestSwing;

    public int TotalDecisions;

    // ---- economy-rework telemetry ----

    /// <summary>Times the Totem of Undying covered a payment this run (0 or 1 by design).</summary>
    public int TotemFires;

    /// <summary>Death with the bank within 20% of the missed payment — the near-miss failure mode
    /// (report metric: near-miss deaths feel earned; blowout deaths feel rigged).</summary>
    public bool CloseCallDeath;

    /// <summary>Highest Scar Tissue stack level reached (pp), and how many times a carrier burned.</summary>
    public double MaxScarStacks;
    public int ScarBurns;

    /// <summary>Consumables the bookie gifted (the pity channel firing).</summary>
    public int GiftsReceived;
}

/// <summary>
/// Truth-reading scoring helpers. These are the ONE place true probabilities are read for scoring;
/// the strategy bots must never call into engine truth (see the honesty rule in each bot file).
/// </summary>
public static class Metrics
{
    /// <summary>
    /// True expected value of a ticket at lock: stake × (Π p_true × Π o_offered × payoutMult − 1).
    /// PayoutMultiplier carries the whole product slot (Multiplier × Scar carrier), so item power
    /// flows into the EV arc automatically. Uses true probs — harness-only.
    /// </summary>
    public static double TrueTicketEvAtLock(Ticket ticket)
    {
        double pProd = 1.0;
        double oProd = 1.0;
        foreach (Leg leg in ticket.Legs)
        {
            pProd *= leg.Matchup.TrueProb(leg.Side); // truth: harness scoring only
            oProd *= leg.OfferedOdds;
        }

        return ticket.Stake * (pProd * oProd * ticket.PayoutMultiplier - 1.0);
    }
}
