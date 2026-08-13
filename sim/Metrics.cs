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

    /// <summary>Full-contract EV of each ticket at lock (telemetry series).</summary>
    public readonly List<double> TicketEvsAtLock = new();

    /// <summary>Passive-only counterfactual EV at lock — G4's gate series (see
    /// <see cref="Metrics.TruePassiveOnlyEvAtLock"/>).</summary>
    public readonly List<double> TicketPassiveEvsAtLock = new();

    /// <summary>Per-market leg exposure. Stake is split evenly across a ticket's placed legs so
    /// shares sum to the total ticket stake rather than double-counting parlays.</summary>
    public readonly Dictionary<MarketKind, MarketExposure> MarketExposure = new();

    public int CashOutsCount;
    public double CashOutsTotal;

    /// <summary>Mulligan Slips played in this round's sweats (a real player decision).</summary>
    public int MulligansPlayed;

    /// <summary>Ref's Whistles played in this round's sweats (charm expansion).</summary>
    public int WhistlesPlayed;

    /// <summary>Other consumables played this round (boost / free bet / DoN / marker / manager).</summary>
    public int ConsumablesPlayed;

    /// <summary>Largest single-ticket money swing this round (won payout / cash-out taken / stake lost).</summary>
    public double BiggestSwing;

    public int Buys;

    /// <summary>Player-facing decisions attributable to this round: tickets + cash-outs
    /// + saves and consumables played + purchases in this round's shop.</summary>
    public int Decisions => TicketsPlaced + CashOutsCount + MulligansPlayed + WhistlesPlayed
        + ConsumablesPlayed + Buys;
}

/// <summary>Placed-leg exposure and its realized, pre-item single-leg return. This deliberately
/// observes the market selection itself, before parlay multiplication, cash-outs, or relic
/// factors make a ticket-level return impossible to attribute to one kind.</summary>
public sealed class MarketExposure
{
    public int LegsPlaced;
    /// <summary>Moneyline only: how many of <see cref="LegsPlaced"/> backed the DRAW. Exposure is
    /// keyed by MarketKind, and the draw is a CHOICE inside the moneyline rather than a kind of its
    /// own — so without this counter the newest market on the board would be invisible, folded into
    /// the Moneyline row, and no report could say whether a bot ever touched it. G7's doctrine is
    /// that a coverage question must be readable in every report, not reconstructed later.</summary>
    public int DrawLegs;
    public double Stake;
    public double RealizedNet;
    /// <summary>Sum of per-leg unit returns ((odds−1) on a win, −1 on a loss), unweighted by
    /// stake. Compounding banks make the stake-weighted number a lottery readout — a few
    /// monster tickets dominate it; THIS one converges to −vig under fair pricing and is the
    /// sanity statistic (F_0.4.0 P5 review).</summary>
    public double RealizedNetUnit;
}

/// <summary>Per-item event counters (PLAN.md rev 5 §16): offered / acquired / bought / sold /
/// used. Conversion (offered → bought) feeds the dealt-hand starvation watch; the audit's
/// exposure thresholds read Acquired and Used.</summary>
public sealed class ItemEvents
{
    public int Offered;
    public int Acquired; // bought + granted + gifted + refilled
    public int Bought;
    public int Sold;
    public int Used;
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

    /// <summary>Per-item event counters (rev 5 §16), keyed by catalog id.</summary>
    public readonly Dictionary<string, ItemEvents> ItemEvents = new();

    /// <summary>Final visible effect state (ratchet stacks, streaks) at run end, keyed by id.</summary>
    public readonly Dictionary<string, double> FinalEffectStates = new();

    public ItemEvents Events(string id)
    {
        if (!ItemEvents.TryGetValue(id, out ItemEvents? e))
        {
            e = new ItemEvents();
            ItemEvents[id] = e;
        }
        return e;
    }
}

/// <summary>
/// Truth-reading scoring helpers. These are the ONE place true probabilities are read for scoring;
/// the strategy bots must never call into engine truth (see the honesty rule in each bot file).
/// </summary>
public static class Metrics
{
    /// <summary>
    /// True CONTRACT EV of a ticket at lock (G4, PLAN.md rev 5 §17): ExpectedTerminalCredit at
    /// true probabilities MINUS the stake. Every LOCKED modifier is contract — the factor map
    /// (Multiplier, Photo, DoN, …) scales the win credit and a Free Bet adds the loss-side
    /// refund — while saves and cash-out decisions are POLICY, tracked separately. Uses true
    /// probs — harness-only.
    /// </summary>
    public static double TrueTicketEvAtLock(Ticket ticket)
    {
        double pProd = 1.0;
        double oProd = 1.0;
        foreach (Leg leg in ticket.Legs)
        {
            pProd *= leg.Matchup.TrueProb(leg.Selection); // truth: harness scoring only
            oProd *= leg.OfferedOdds;
        }

        double winCredit = ticket.Stake * oProd * ticket.PayoutMultiplier;
        double refund = ticket.Modifier == TicketModifier.FreeBet ? ticket.Stake : 0.0;
        return pProd * winCredit + (1.0 - pProd) * refund - ticket.Stake;
    }

    /// <summary>
    /// The PASSIVE-ONLY counterfactual EV (G4's gate series — the tuning campaign's amendment,
    /// logged in PLAN-REVIEW-LOG): the ticket priced at BASE odds (boost stripped), no refund
    /// leg, no DoN factor — when does the BUILD (passives) beat the BOOK? Codex round 2 named
    /// this counterfactual as the legitimate alternative to full-contract EV; full-contract
    /// (above) stays as the telemetry series. Cheap +EV promos are the catalog's FANTASY —
    /// gating on them measures time-to-first-promo, not the arc.
    /// </summary>
    public static double TruePassiveOnlyEvAtLock(Ticket ticket)
    {
        double pProd = 1.0;
        double oProd = 1.0;
        foreach (Leg leg in ticket.Legs)
        {
            pProd *= leg.Matchup.TrueProb(leg.Selection); // truth: harness scoring only
            oProd *= leg.BaseOdds;                   // boost stripped
        }

        double mult = ticket.PayoutMultiplier;
        if (ticket.Modifier == TicketModifier.DoubleOrNothing)
            mult /= RelicCatalog.DoubleOrNothingMult;

        return ticket.Stake * (pProd * oProd * mult - 1.0);
    }
}
