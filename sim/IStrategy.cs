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

    public void NewRound() => HomeProbEst.Clear();
}

/// <summary>
/// A pluggable strategy bot (PRD F9). Hooks: <see cref="Bet"/> (betting window), <see cref="ShouldCashOut"/>
/// (per sweat event, only when <see cref="ControlsSweat"/>), and <see cref="Shop"/> (between rounds).
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
    /// <paramref name="evt"/>.WinProbAfter is the on-screen live win% of the current leg
    /// (legitimate player information).
    /// </summary>
    bool ShouldCashOut(Run run, Ticket ticket, SweatSession session, DramaEvent evt,
        double offer, double bankNow, double target, BotState state, Pcg32 rng);

    /// <summary>Shop window: buy relics via run.BuyRelic.</summary>
    void Shop(Run run, BotState state, Pcg32 rng);
}
