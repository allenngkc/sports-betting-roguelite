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

/// <summary>Exposure for one SAME MATCH relation kind (F_0.6.0). Counted three ways because they
/// answer three different questions: how many labelled pairs the campaign priced, how many tickets
/// carried the kind at all, and how often it was the one relation the slip would have STATED.
///
/// <para>Deliberately no stake column. A ticket carries several relations at once, so splitting its
/// stake among them would invent an attribution the model does not make — unlike
/// <see cref="MarketExposure"/>, where a leg belongs to exactly one market.</para></summary>
public sealed class SameMatchExposure
{
    /// <summary>Relation instances (leg pairs) of this kind on placed tickets.</summary>
    public int Relations;

    /// <summary>Placed tickets carrying at least one relation of this kind.</summary>
    public int Tickets;

    /// <summary>Times this kind was the ticket's PRINCIPAL relation — the one the slip states.</summary>
    public int Principal;
}

/// <summary>Exposure for one MARKET KIND inside SAME MATCH tickets (F_0.6.0 step 4b). Separate from
/// <see cref="MarketExposure"/>, which counts every leg a bot placed anywhere: a kind can be heavily
/// exposed there and never once reach a same-match slip, and that difference is precisely the hole
/// G7-SGP's per-kind arm exists to catch — the merged draws board shipped nine kinds the probe's
/// catalogue had never heard of while every coverage number stayed green.
///
/// <para>Only legs that are actually IN a same-match group are counted: a ticket may mix matchups,
/// and a leg alone on its own matchup is an ordinary parlay leg riding along, not evidence that the
/// kind was ever priced against a correlated sibling.</para></summary>
public sealed class SameMatchKindExposure
{
    /// <summary>Same-match legs of this kind placed.</summary>
    public int Legs;

    /// <summary>Placed same-match tickets carrying at least one leg of this kind.</summary>
    public int Tickets;
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

    // ---- SAME MATCH coverage (F_0.6.0 step 4, what G7's SGP arm is evaluated on) ----
    //
    // PER-RUN FIELDS, REDUCED AFTERWARDS — the discipline Harness.cs states at the top of the file.
    // RunBatch fills a pre-sized results[i] under Parallel.For and every aggregate is reduced
    // sequentially from that array, so a same-match total must live here and be summed in
    // BatchSummary, NOT in a second process-wide counter. (SameMatchModel.NoLabelFallbacks IS
    // process-wide, and is interlocked and correct — but it reads as one campaign-level number,
    // which is the right shape for a zero-assertion and the wrong one for per-bot attribution.)

    /// <summary>Same-match tickets placed this run (a ticket whose legs share a matchup).</summary>
    public int SameMatchPlaced;

    /// <summary>Same-match tickets GRADED — won, lost or voided. Placed and settled are separate
    /// facts: a ticket can be sold and never graded.
    ///
    /// <para><b>Cashed-out tickets are NOT counted here</b> (F_0.6.0 phase 4 follow-on). They used to
    /// be, harmlessly, because the probe never cashed out and the two sets could not overlap. Now that
    /// it does, "settled" and "cashed out" are disjoint outcomes of the same population and counting a
    /// cash-out as a settlement would let the settlement volume G7-SGP's arm reads be propped up by
    /// tickets that were never graded at all.</para></summary>
    public int SameMatchSettled;

    /// <summary>Same-match tickets the bot CASHED OUT of — the conditional-quote path (phase 4), and
    /// its only end-to-end exercise in a campaign.</summary>
    public int SameMatchCashedOut;

    /// <summary>Of those, the ones taken with NOTHING settled yet — the position where the quote is
    /// the ticket's own locked joint.</summary>
    public int SameMatchCashOutsEarly;

    /// <summary>Of those, the ones taken on the LAST leg — where the conditional collapses onto the
    /// live leg's own set and the certainty carve-out is reachable. The remainder are mid-sweat.</summary>
    public int SameMatchCashOutsLate;

    /// <summary>Money banked out of same-match tickets, so the split can be read in credit as well as
    /// in ticket counts.</summary>
    public double SameMatchCashOutCredit;

    /// <summary>Legs voided out of a same-match ticket by a Mulligan — every one of these is a live
    /// re-price onto the survivors' locked subset price, the path step 3 added.</summary>
    public int SameMatchVoids;

    /// <summary>Refusals the bot provoked on purpose, and refusals it met unexpectedly (a defect
    /// signal), copied off the BotState at run end — see the seam's note there.</summary>
    public int SameMatchRefusals;
    public int SameMatchUnexpectedRefusals;
    public readonly Dictionary<RefusalKind, int> SameMatchRefusalKinds = new();

    /// <summary>Per-relation-kind exposure over the run's placed same-match tickets.</summary>
    public readonly Dictionary<RelationKind, SameMatchExposure> SameMatchRelations = new();

    /// <summary>Per-MARKET-kind exposure over the run's placed same-match tickets — the roll-call
    /// G7-SGP's per-kind arm reads. Same per-run-field discipline as everything above it.</summary>
    public readonly Dictionary<MarketKind, SameMatchKindExposure> SameMatchKinds = new();

    public SameMatchExposure Relation(RelationKind kind)
    {
        if (!SameMatchRelations.TryGetValue(kind, out SameMatchExposure? e))
            SameMatchRelations[kind] = e = new SameMatchExposure();
        return e;
    }

    public SameMatchKindExposure Kind(MarketKind kind)
    {
        if (!SameMatchKinds.TryGetValue(kind, out SameMatchKindExposure? e))
            SameMatchKinds[kind] = e = new SameMatchKindExposure();
        return e;
    }

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
