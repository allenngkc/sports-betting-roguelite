using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Game
{
    /// <summary>
    /// The six market-sheet destinations ENTRY's contents rail prints
    /// (<c>spec-market-surfaces-2026-08-17.md</c> §3). Declaration order below IS the rail's
    /// printed order: RESULT, GOALS, CORRECT SCORE, CORNERS, CARDS, PLAYERS. Per §3.1 this set is
    /// a CONSTANT — authored once, never varying by matchup, because empty groups still print. No
    /// destination ever appears or disappears because of what happens to be priced.
    /// </summary>
    public enum MarketDestination
    {
        Result,
        Goals,
        CorrectScore,
        Corners,
        Cards,
        Players,
    }

    /// <summary>
    /// Maps every <see cref="MarketKind"/> onto its one destination home, and the printed labels
    /// for both (spec §3's table). Pure logic, no UI — another workstream builds the rail against
    /// this surface. <c>C51</c>: a cross-element invariant is an assertion or it does not exist;
    /// see <c>MarketDestinationTests</c> for the exhaustiveness/totality/agreement gate spec §7.1
    /// requires.
    /// </summary>
    public static class MarketDestinations
    {
        /// <summary>The six destinations, in rail order. Spec §3.1: constant across matchups —
        /// never computed from what happens to be priced.</summary>
        public static readonly IReadOnlyList<MarketDestination> All = new[]
        {
            MarketDestination.Result,
            MarketDestination.Goals,
            MarketDestination.CorrectScore,
            MarketDestination.Corners,
            MarketDestination.Cards,
            MarketDestination.Players,
        };

        /// <summary>
        /// The one destination <paramref name="kind"/> lives on (spec §3's table). Exhaustive over
        /// <see cref="MarketKind"/> by name — no wildcard arm maps an unrecognized kind onto a
        /// valid destination. C# still requires a discard arm to accept this as a complete switch
        /// expression over an enum's open underlying value space; that arm THROWS rather than
        /// absorbing the input, so a 16th <see cref="MarketKind"/> member added without a matching
        /// arm here fails loudly (spec §7.1 gate 1: "adding a kind without a home fails the build
        /// rather than hiding on the surface") instead of silently resolving to some destination.
        /// </summary>
        public static MarketDestination For(MarketKind kind) => kind switch
        {
            // RESULT
            MarketKind.Moneyline => MarketDestination.Result,
            MarketKind.DoubleChance => MarketDestination.Result,
            MarketKind.Handicap => MarketDestination.Result,
            MarketKind.WinningMargin => MarketDestination.Result,

            // GOALS
            MarketKind.TotalGoals => MarketDestination.Goals,
            MarketKind.TeamTotalGoals => MarketDestination.Goals,
            MarketKind.TotalGoalsOddEven => MarketDestination.Goals,
            MarketKind.BothTeamsToScore => MarketDestination.Goals,

            // CORRECT SCORE
            MarketKind.CorrectScore => MarketDestination.CorrectScore,

            // CORNERS
            MarketKind.TotalCorners => MarketDestination.Corners,
            MarketKind.TeamTotalCorners => MarketDestination.Corners,

            // CARDS
            MarketKind.TotalCards => MarketDestination.Cards,
            MarketKind.TeamTotalCards => MarketDestination.Cards,

            // PLAYERS
            MarketKind.AnytimeScorer => MarketDestination.Players,
            MarketKind.PlayerMultiScorer => MarketDestination.Players,

            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind,
                $"MarketDestinations.For: {kind} has no destination mapping — give it a home in "
                + "the switch above (spec-market-surfaces-2026-08-17.md §3) rather than letting it "
                + "hide on the surface"),
        };

        /// <summary>The printed rail label for a destination.</summary>
        public static string Label(MarketDestination d) => d switch
        {
            MarketDestination.Result => "RESULT",
            MarketDestination.Goals => "GOALS",
            MarketDestination.CorrectScore => "CORRECT SCORE",
            MarketDestination.Corners => "CORNERS",
            MarketDestination.Cards => "CARDS",
            MarketDestination.Players => "PLAYERS",
            _ => throw new ArgumentOutOfRangeException(nameof(d), d,
                $"MarketDestinations.Label: {d} has no printed label"),
        };

        /// <summary>
        /// The full vocabulary, grouped by destination in spec §3 table's row order and flattened
        /// in reading order (RESULT's four rows, then GOALS' four, and on down the rail).
        ///
        /// <para>This is the one hand-authored ordering. Walking <c>Enum.GetValues(typeof(MarketKind))</c>
        /// (engine/Domain.cs declaration order) and bucketing by <see cref="For"/> reproduces the
        /// table for five of six destinations for free, but not GOALS: <see cref="MarketKind.BothTeamsToScore"/>
        /// declares 3rd of 15 in the engine enum, while the table seats it LAST within GOALS. So
        /// declaration order cannot stand in for the table here, and the order below is authored by
        /// hand instead.</para>
        ///
        /// <para><see cref="KindsIn"/> filters this list through <see cref="For"/> rather than
        /// assigning destinations itself, so the two cannot drift apart on MEMBERSHIP: moving a kind
        /// in <see cref="For"/> without moving it here only changes which destination's filter it
        /// passes through, it can never vanish or double up. (Covered by MarketDestinationTests'
        /// totality and agreement gates; this list's row ORDER is covered by its own table-order
        /// test.)</para>
        /// </summary>
        private static readonly MarketKind[] TableOrder =
        {
            MarketKind.Moneyline, MarketKind.DoubleChance, MarketKind.Handicap, MarketKind.WinningMargin,
            MarketKind.TotalGoals, MarketKind.TeamTotalGoals, MarketKind.TotalGoalsOddEven, MarketKind.BothTeamsToScore,
            MarketKind.CorrectScore,
            MarketKind.TotalCorners, MarketKind.TeamTotalCorners,
            MarketKind.TotalCards, MarketKind.TeamTotalCards,
            MarketKind.AnytimeScorer, MarketKind.PlayerMultiScorer,
        };

        /// <summary>The kinds destination <paramref name="d"/> holds, in the spec §3 table's
        /// order.</summary>
        public static IReadOnlyList<MarketKind> KindsIn(MarketDestination d)
        {
            var kinds = new List<MarketKind>();
            foreach (MarketKind kind in TableOrder)
            {
                if (For(kind) == d) kinds.Add(kind);
            }
            return kinds;
        }

        /// <summary>
        /// The printed market name for a row-group heading. Exhaustive over <see cref="MarketKind"/>
        /// by name, same no-silent-default rule as <see cref="For"/>. Ordered here to match
        /// <see cref="MarketKind"/>'s own declaration (engine/Domain.cs) so every member's presence
        /// is easy to audit top to bottom.
        /// </summary>
        public static string KindLabel(MarketKind kind) => kind switch
        {
            MarketKind.Moneyline => "MONEYLINE",
            MarketKind.TotalGoals => "TOTAL GOALS",
            MarketKind.BothTeamsToScore => "BOTH TEAMS TO SCORE",
            MarketKind.TotalCorners => "TOTAL CORNERS",
            MarketKind.TotalCards => "TOTAL CARDS",
            MarketKind.AnytimeScorer => "ANYTIME SCORER",
            MarketKind.DoubleChance => "DOUBLE CHANCE",
            MarketKind.Handicap => "HANDICAP",
            // JUDGMENT CALL (flagged in the implementing agent's report): the task's own example
            // list gives this row exactly "TEAM TOTALS", not "TEAM TOTAL GOALS" — asymmetric with
            // TeamTotalCorners/TeamTotalCards below, which do carry their stat name. Kept literal
            // because the mapping is forced by elimination (15 example labels, 15 MarketKind
            // members, every other label has an unambiguous single kind it names), not because the
            // asymmetry has an obvious rationale.
            MarketKind.TeamTotalGoals => "TEAM TOTALS",
            MarketKind.CorrectScore => "CORRECT SCORE",
            MarketKind.WinningMargin => "WINNING MARGIN",
            MarketKind.TotalGoalsOddEven => "ODD / EVEN",
            MarketKind.TeamTotalCorners => "TEAM TOTAL CORNERS",
            MarketKind.TeamTotalCards => "TEAM TOTAL CARDS",
            MarketKind.PlayerMultiScorer => "MULTI SCORER",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind,
                $"MarketDestinations.KindLabel: {kind} has no printed label"),
        };
    }
}
