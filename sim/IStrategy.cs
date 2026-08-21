using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// Per-run scratch for a bot. Strategy objects are stateless singletons shared across parallel runs,
/// so anything a bot must remember within one run lives here (created fresh per run by the harness).
/// </summary>
public sealed class BotState
{
    /// <summary>The bot's estimate of P(home wins) per matchup index for the current round. Populated by
    /// the bot at bet time from the info a player could have (de-vigged odds).</summary>
    public readonly Dictionary<int, double> HomeProbEst = new();

    /// <summary>The bot's public-information probability estimates for every selection it has
    /// considered this round. MarketSelection is part of the key so non-moneyline legs never
    /// need to be coerced through Side.</summary>
    public readonly Dictionary<(int Matchup, MarketSelection Selection), double> MarketProbEst = new();

    // ---- SAME MATCH refusal telemetry (F_0.6.0 step 4) ----
    //
    // A refusal is the one same-match fact the harness CANNOT observe for itself: it happens inside
    // the bot's betting window, leaves no ticket behind, and the strategy object is a stateless
    // singleton shared across every run in a parallel batch — so a counter on the bot would be a data
    // race. This per-run scratch is the seam, and RunPlayer copies it onto the RunResult at run end,
    // which keeps the harness's own discipline: per-run fields, reduced afterwards, never a
    // process-wide counter. Deliberately NOT cleared by NewRound — these are run-scoped totals.

    /// <summary>Refusals the bot provoked ON PURPOSE (the samematch probe's invalid slip).</summary>
    public int SameMatchRefusals;

    /// <summary>Refusals a bot met on a slip it expected to be legal. Counted apart because it is a
    /// defect signal — it must never hide inside the number G7's SGP arm wants to be positive.</summary>
    public int SameMatchUnexpectedRefusals;

    /// <summary>Which rule refused, both kinds of refusal together — the breakdown is what shows a
    /// SubEvens appearing (unreachable at κ = 1) or one rule never firing at all.</summary>
    public readonly Dictionary<RefusalKind, int> SameMatchRefusalKinds = new();

    /// <summary>Where this run starts its walk through the probe's MARKET-KIND catalogue, drawn once
    /// from the bot's own per-run generator (−1 = not yet drawn). It lives here rather than on the
    /// strategy for the same reason the counters above do: the strategy object is a stateless
    /// singleton shared across a parallel batch, so a rotation cursor on it would be a data race AND
    /// would make one run's catalogue position depend on how many other runs had already bet.
    ///
    /// <para>Why a per-run start and not the round-only walk T1 uses: fifteen market kinds do not fit
    /// in the four-to-five rounds a run survives, so a round-only walk would give the late entries
    /// only the tail of the batch. See <see cref="SameMatchStrategy"/>'s class comment — the two
    /// catalogues rotate differently on purpose, and the earlier finding against a generator draw was
    /// about the RELATION catalogue, where it is still the rule.</para></summary>
    public int SameMatchSweepStart = -1;

    public void NewRound()
    {
        HomeProbEst.Clear();
        MarketProbEst.Clear();
    }
}

/// <summary>The pending-loss window verbs a bot can choose between (charm expansion, PLAN.md
/// rev 5 §13). RunPlayer validates legality (holdings, mulligan's ≥2-active-legs rule) and
/// falls back to Decline when the choice is illegal.</summary>
public enum PendingLossAction { Decline, Mulligan, Whistle }

/// <summary>
/// A pluggable strategy bot (PRD F9). Hooks: <see cref="Bet"/> (betting window), <see cref="ShouldCashOut"/>
/// (per sweat event, only when <see cref="ControlsSweat"/>), <see cref="ChoosePendingLossAction"/>
/// (the save window), and <see cref="Shop"/> (between rounds).
///
/// HONESTY RULE (enforced by convention across all bots): a bot may read only what a player could know —
/// odds, records, the bank/debt/requirement, and revealed drama events (evt.WinProbAfter). A bot must
/// NEVER read Matchup.TrueHomeProb, Leg.TrueProb, or Matchup.Result. Only the harness (Metrics) reads truth.
/// </summary>
public interface IStrategy
{
    string Name { get; }

    /// <summary>When false the harness fast-forwards the sweat (never cashing out) and never calls
    /// <see cref="ShouldCashOut"/>. Naive is the only bot that opts out.</summary>
    bool ControlsSweat { get; }

    /// <summary>Betting window: place 0..MaxTickets tickets via run.PlaceTicket.</summary>
    void Bet(Run run, BotState state, Pcg32 rng);

    /// <summary>
    /// Called once per sweat event on an open, cash-out-eligible ticket when an offer exists.
    /// Return true to take <paramref name="offer"/>. <paramref name="bankNow"/> is the live bank and
    /// <paramref name="target"/> the round's settle requirement (target + any bookie debt);
    /// <paramref name="evt"/>.WinProbAfter is the live win% of the telling's ANCHOR leg — since
    /// <c>T140</c> arm A a beat can carry N legs, and <c>T164</c> rules that the ON-SCREEN number is
    /// the ticket's (<c>SweatSession.TicketWinProbability</c>), not a leg's. Both are legitimate
    /// player information; they are no longer the same number on a same-match ticket. Use
    /// <c>evt.LegProbs</c> when a per-leg figure is what is meant.
    /// </summary>
    bool ShouldCashOut(Run run, Ticket ticket, SweatSession session, DramaEvent evt,
        double offer, double bankNow, double target, BotState state, Pcg32 rng);

    /// <summary>
    /// The pending-loss window (rev 5 §13). Default policy (the documented bot greed): a legal
    /// Mulligan is the certain save — take it; else a held Whistle fires when the captured
    /// pre-kill prob gives at least a coin-flip's worth of rescue; else decline.
    /// </summary>
    PendingLossAction ChoosePendingLossAction(Run run, Ticket ticket, SweatSession session,
        BotState state, Pcg32 rng)
    {
        if (run.OwnsConsumable("mulligan_slip") && session.CanMulliganPendingLoss)
            return PendingLossAction.Mulligan;
        if (run.OwnsConsumable("refs_whistle") && session.PendingLossProbBefore >= 0.35)
            return PendingLossAction.Whistle;
        return PendingLossAction.Decline;
    }

    /// <summary>Shop window: buy relics via run.BuyRelic.</summary>
    void Shop(Run run, BotState state, Pcg32 rng);
}
