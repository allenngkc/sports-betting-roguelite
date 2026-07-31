using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Game
{
    /// <summary>
    /// Phase 3A (PRD §8.2, §9): a pure, standalone formatter for active-leg market copy — the
    /// <c>NEED</c> and <c>LIVE</c> sentences the active-leg card shows for each live leg. This
    /// class is not wired into <see cref="TvSweatScreen"/> yet; constructing or querying it has
    /// no effect on anything that currently renders. Wiring is a later step (the same
    /// build-standalone-then-wire pattern Phases 2A and 2B used).
    ///
    /// <para><b>The no-leak law, enforced by the signature, not by discipline (PRD §4.2, §8.2:
    /// "The formatter may not read unrevealed endpoint values to create progress copy").</b>
    /// <see cref="ActiveLegInput"/> is built from named, public factory methods
    /// (<see cref="ActiveLegInput.Moneyline"/>, <see cref="ActiveLegInput.TotalGoals"/>,
    /// <see cref="ActiveLegInput.BothTeamsToScore"/>, <see cref="ActiveLegInput.TotalCorners"/>,
    /// <see cref="ActiveLegInput.TotalCards"/>, <see cref="ActiveLegInput.AnytimeScorer"/>) whose
    /// parameters are exclusively <c>int</c>, <c>double</c>, <c>bool</c>, and <c>string</c> —
    /// plain revealed counts and betting-time facts (market line, backed team/player display
    /// name). There is no parameter, field, or property anywhere in this file typed as
    /// <c>Leg</c>, <c>ScoreLedger</c>, <c>CountLedger</c>, or <c>MatchStatLine</c> — the four
    /// types that can reach a locked endpoint or target
    /// (<c>ScoreLedger.TargetPicked</c>/<c>TargetOpponent</c>, <c>CountLedger.TargetHome</c>/
    /// <c>TargetAway</c>/<c>TargetTotal</c>, or <c>MatchStatLine</c> itself). A reviewer can
    /// confirm the no-leak property by reading this file's type signatures alone: there is
    /// nothing here to leak from, because nothing carrying a hidden outcome is ever accepted.
    /// Callers must pass already-revealed values (e.g. <c>ScoreLedger.Picked</c>/
    /// <c>Opponent</c>, which are mutated only by <c>CompleteGoal</c> on a completed payoff, or
    /// <c>CountLedger.Home</c>/<c>Away</c>, mutated only by <c>CompleteCount</c>) and a
    /// causal-reveal flag for the anytime-scorer market (<see cref="ActiveLegInput.ScorerRevealed"/>)
    /// that the caller sets true only at the same causal payoff <c>TvSweatScreen.ScorerFor</c>
    /// already gates on.</para>
    ///
    /// <para><b>Concurrency tolerance (PRD §8.2A).</b> The phrase "the active leg", singular, is
    /// retired. <see cref="DescribeAll"/> takes <c>IReadOnlyList&lt;ActiveLegInput&gt;</c> so a
    /// caller with zero, one, or several concurrent live legs on one match uses the same entry
    /// point; nothing in this file's signatures hard-codes a single live leg.</para>
    ///
    /// <para><b>Pure and deterministic.</b> No <c>UnityEngine</c> types, no <c>MonoBehaviour</c>,
    /// no RNG, no clock, no static mutable state. Every method is a pure function of its
    /// arguments, constructible and assertable in EditMode with no scene.</para>
    ///
    /// <para><b>Copy is uppercase</b> per <c>DESIGN.md</c> §5 ("Uppercase for labels, states, and
    /// team names").</para>
    /// </summary>
    public static class SweatActiveLegModel
    {
        /// <summary>
        /// Revealed-only input for one live leg's market copy. Every field is a plain value
        /// type or string — never an object that could carry a locked endpoint. Construct via
        /// the market-specific factory methods below; the private constructor keeps the field
        /// set from being assembled ad hoc with mismatched values for the wrong market.
        /// </summary>
        public readonly struct ActiveLegInput
        {
            public readonly MarketKind Kind;
            public readonly MarketChoice Choice;
            public readonly double Line;
            public readonly string BackedTeamName;
            public readonly string BackedPlayerName;

            /// <summary>Moneyline/total-goals/BTTS: REVEALED goals so far for the leg's own
            /// "for" anchor (moneyline: the backed side; totals/BTTS: either side — both feed
            /// the same total). Must already be sourced from a revealed-only counter such as
            /// <c>ScoreLedger.Picked</c>, never a locked target.</summary>
            public readonly int RevealedGoalsFor;

            /// <summary>The other side's REVEALED goals so far (moneyline: the opponent;
            /// totals/BTTS: the complementary side). Same provenance rule as
            /// <see cref="RevealedGoalsFor"/> — e.g. <c>ScoreLedger.Opponent</c>.</summary>
            public readonly int RevealedGoalsAgainst;

            /// <summary>Corners/cards: REVEALED home-side count so far (e.g. <c>CountLedger.Home</c>).</summary>
            public readonly int RevealedCountHome;

            /// <summary>Corners/cards: REVEALED away-side count so far (e.g. <c>CountLedger.Away</c>).</summary>
            public readonly int RevealedCountAway;

            /// <summary>Anytime scorer only: true only at the leg's causal identity payoff — the
            /// same instant <c>TvSweatScreen.ScorerFor</c> would first return the bound actor.
            /// False for every frame before that, including every frame where the backed
            /// player's team has already scored via a different actor.</summary>
            public readonly bool ScorerRevealed;

            private ActiveLegInput(MarketKind kind, MarketChoice choice, double line,
                string backedTeamName, string backedPlayerName,
                int revealedGoalsFor, int revealedGoalsAgainst,
                int revealedCountHome, int revealedCountAway, bool scorerRevealed)
            {
                Kind = kind;
                Choice = choice;
                Line = line;
                BackedTeamName = backedTeamName;
                BackedPlayerName = backedPlayerName;
                RevealedGoalsFor = revealedGoalsFor;
                RevealedGoalsAgainst = revealedGoalsAgainst;
                RevealedCountHome = revealedCountHome;
                RevealedCountAway = revealedCountAway;
                ScorerRevealed = scorerRevealed;
            }

            /// <summary><paramref name="revealedGoalsFor"/>/<paramref name="revealedGoalsAgainst"/>
            /// must already be anchored to the backed side (mirroring
            /// <c>SweatFlavor.PickedHomeForPresentation</c>) — e.g. <c>ScoreLedger.Picked</c> and
            /// <c>.Opponent</c> directly, which that ledger only ever mutates on a completed goal.</summary>
            public static ActiveLegInput Moneyline(string backedTeamName, int revealedGoalsFor, int revealedGoalsAgainst)
                => new ActiveLegInput(MarketKind.Moneyline, MarketChoice.Home, 0.0, backedTeamName, null,
                    revealedGoalsFor, revealedGoalsAgainst, 0, 0, false);

            public static ActiveLegInput TotalGoals(bool over, double line, int revealedGoalsFor, int revealedGoalsAgainst)
                => new ActiveLegInput(MarketKind.TotalGoals, over ? MarketChoice.Over : MarketChoice.Under, line,
                    null, null, revealedGoalsFor, revealedGoalsAgainst, 0, 0, false);

            public static ActiveLegInput BothTeamsToScore(bool yes, int revealedGoalsFor, int revealedGoalsAgainst)
                => new ActiveLegInput(MarketKind.BothTeamsToScore, yes ? MarketChoice.Yes : MarketChoice.No, 0.0,
                    null, null, revealedGoalsFor, revealedGoalsAgainst, 0, 0, false);

            public static ActiveLegInput TotalCorners(bool over, double line, int revealedHome, int revealedAway)
                => new ActiveLegInput(MarketKind.TotalCorners, over ? MarketChoice.Over : MarketChoice.Under, line,
                    null, null, 0, 0, revealedHome, revealedAway, false);

            public static ActiveLegInput TotalCards(bool over, double line, int revealedHome, int revealedAway)
                => new ActiveLegInput(MarketKind.TotalCards, over ? MarketChoice.Over : MarketChoice.Under, line,
                    null, null, 0, 0, revealedHome, revealedAway, false);

            public static ActiveLegInput AnytimeScorer(string backedPlayerName, bool scorerRevealed)
                => new ActiveLegInput(MarketKind.AnytimeScorer, MarketChoice.Yes, 0.0, null, backedPlayerName,
                    0, 0, 0, 0, scorerRevealed);
        }

        /// <summary>One live leg's formatted card copy (PRD §8.2).</summary>
        public readonly struct ActiveLegCopy
        {
            /// <summary>The plain-language requirement, e.g. <c>"ARSENAL TO WIN"</c>.</summary>
            public readonly string Need;

            /// <summary>The causal, revealed-only progress statement, e.g. <c>"LEADING 2–1"</c>.</summary>
            public readonly string Live;

            /// <summary>True only for moneyline — the one market with a real backed team side.</summary>
            public readonly bool IsTeamMarket;

            /// <summary>Backed-team display name (uppercase) for team markets; the literal
            /// <c>"MARKET PICK"</c> for every non-team market. Never a fabricated team.</summary>
            public readonly string Identity;

            public ActiveLegCopy(string need, string live, bool isTeamMarket, string identity)
            {
                Need = need;
                Live = live;
                IsTeamMarket = isTeamMarket;
                Identity = identity;
            }
        }

        private const string MarketPick = "MARKET PICK";
        // U+2013 EN DASH, matching the PRD's literal "n–n" scoreline copy.
        private const char Dash = '–';
        // U+2022 BULLET, matching the PRD's literal "n GOALS • m MORE" separator.
        private const char Bullet = '•';

        /// <summary>Formats one live leg's card copy. Pure function of <paramref name="input"/>.</summary>
        public static ActiveLegCopy Describe(ActiveLegInput input)
        {
            switch (input.Kind)
            {
                case MarketKind.Moneyline:
                    return DescribeMoneyline(input);
                case MarketKind.TotalGoals:
                    return input.Choice == MarketChoice.Over
                        ? DescribeTotalGoalsOver(input)
                        : DescribeTotalGoalsUnder(input);
                case MarketKind.BothTeamsToScore:
                    return input.Choice == MarketChoice.Yes
                        ? DescribeBttsYes(input)
                        : DescribeBttsNo(input);
                case MarketKind.TotalCorners:
                    return DescribeCount(input, "CORNERS");
                case MarketKind.TotalCards:
                    return DescribeCount(input, "CARDS");
                case MarketKind.AnytimeScorer:
                    return DescribeAnytimeScorer(input);
                default:
                    throw new ArgumentOutOfRangeException(nameof(input), input.Kind,
                        "SweatActiveLegModel: unsupported market kind");
            }
        }

        /// <summary>Formats every live leg's card copy in ticket order. PRD §8.2A: there is no
        /// "the" active leg — this is the one entry point for zero, one, or several concurrent
        /// live legs on a match.</summary>
        public static IReadOnlyList<ActiveLegCopy> DescribeAll(IReadOnlyList<ActiveLegInput> liveLegs)
        {
            if (liveLegs == null || liveLegs.Count == 0) return Array.Empty<ActiveLegCopy>();
            var result = new ActiveLegCopy[liveLegs.Count];
            for (int i = 0; i < liveLegs.Count; i++) result[i] = Describe(liveLegs[i]);
            return result;
        }

        // ------------------------------------------------------------------------- moneyline

        private static ActiveLegCopy DescribeMoneyline(ActiveLegInput l)
        {
            string team = (l.BackedTeamName ?? string.Empty).ToUpperInvariant();
            string need = $"{team} TO WIN";
            string score = $"{l.RevealedGoalsFor}{Dash}{l.RevealedGoalsAgainst}";
            string live = l.RevealedGoalsFor > l.RevealedGoalsAgainst ? $"LEADING {score}"
                : l.RevealedGoalsFor < l.RevealedGoalsAgainst ? $"TRAILING {score}"
                : $"LEVEL {score}";
            return new ActiveLegCopy(need, live, isTeamMarket: true, identity: team);
        }

        // ------------------------------------------------------------------------- total goals

        private static ActiveLegCopy DescribeTotalGoalsOver(ActiveLegInput l)
        {
            string need = $"OVER {l.Line:0.0} GOALS";
            int total = l.RevealedGoalsFor + l.RevealedGoalsAgainst;
            string live = HalfLineThreshold(l.Line, out int threshold)
                ? $"{total} GOALS {Bullet} {Math.Max(0, threshold - total)} MORE"
                : $"{total} GOALS";
            return new ActiveLegCopy(need, live, isTeamMarket: false, identity: MarketPick);
        }

        private static ActiveLegCopy DescribeTotalGoalsUnder(ActiveLegInput l)
        {
            string need = $"UNDER {l.Line:0.0} GOALS";
            int total = l.RevealedGoalsFor + l.RevealedGoalsAgainst;
            string live = HalfLineMaxAllowed(l.Line, out int maxAllowed)
                ? $"{total} GOALS {Bullet} LIMIT {Math.Max(0, maxAllowed - total)}"
                : $"{total} GOALS";
            return new ActiveLegCopy(need, live, isTeamMarket: false, identity: MarketPick);
        }

        // ------------------------------------------------------------------------- BTTS

        private static ActiveLegCopy DescribeBttsYes(ActiveLegInput l)
        {
            int scored = (l.RevealedGoalsFor > 0 ? 1 : 0) + (l.RevealedGoalsAgainst > 0 ? 1 : 0);
            string live = $"{scored}/2 TEAMS SCORED";
            return new ActiveLegCopy("BOTH TEAMS TO SCORE", live, isTeamMarket: false, identity: MarketPick);
        }

        private static ActiveLegCopy DescribeBttsNo(ActiveLegInput l)
        {
            // "BOTH HAVE SCORED" is read off two REVEALED counters, both already causally
            // gated by ScoreLedger.CompleteGoal — never a projection from the locked endpoint.
            bool bothScored = l.RevealedGoalsFor > 0 && l.RevealedGoalsAgainst > 0;
            string live = bothScored ? "BOTH HAVE SCORED" : "CLEAN-SHEET PATH LIVE";
            return new ActiveLegCopy("KEEP ONE TEAM SCORELESS", live, isTeamMarket: false, identity: MarketPick);
        }

        // ------------------------------------------------------------------------- corners / cards

        private static ActiveLegCopy DescribeCount(ActiveLegInput l, string noun)
        {
            bool over = l.Choice == MarketChoice.Over;
            string need = $"{(over ? "OVER" : "UNDER")} {l.Line:0.0} {noun}";
            int total = l.RevealedCountHome + l.RevealedCountAway;
            string live;
            if (over && HalfLineThreshold(l.Line, out int threshold))
                live = $"{total} {noun} {Bullet} NEED {Math.Max(0, threshold - total)}";
            else if (!over && HalfLineMaxAllowed(l.Line, out int maxAllowed))
                live = $"{total} {noun} {Bullet} LIMIT {Math.Max(0, maxAllowed - total)}";
            else
                live = $"{total} {noun}";
            return new ActiveLegCopy(need, live, isTeamMarket: false, identity: MarketPick);
        }

        // ------------------------------------------------------------------------- anytime scorer

        private static ActiveLegCopy DescribeAnytimeScorer(ActiveLegInput l)
        {
            string player = (l.BackedPlayerName ?? string.Empty).ToUpperInvariant();
            string need = $"{player} TO SCORE";
            // SCORED is admissible ONLY at the causal identity payoff (input.ScorerRevealed),
            // which the caller sets from the same gate as TvSweatScreen.ScorerFor — never
            // inferred here from a revealed goal count, since the backed player's own team can
            // score via a different actor without the backed player having scored (PRD §4.1,
            // TVS-H03's exact defect class).
            string live = l.ScorerRevealed ? "SCORED" : $"WAITING FOR {Surname(l.BackedPlayerName)}";
            return new ActiveLegCopy(need, live, isTeamMarket: false, identity: MarketPick);
        }

        private static string Surname(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return string.Empty;
            int i = fullName.LastIndexOf(' ');
            return (i >= 0 ? fullName.Substring(i + 1) : fullName).ToUpperInvariant();
        }

        // ------------------------------------------------------------------------- half-line math
        //
        // Every line this run's config generates (RunConfig.GoalLines/CornerLines/CardLines) is a
        // half-integer (x.5), which admits no push and therefore an exact "still needed"/"still
        // allowed" count (PRD §8.2: "where the half-line permits exact remaining copy"). A whole-
        // number line is defensive-only: no generator in this codebase currently produces one, but
        // if one ever did, a push becomes possible and this class declines to fabricate an exact
        // remaining count for it rather than guess.

        private static bool IsHalfLine(double line)
        {
            double frac = line - Math.Floor(line);
            return Math.Abs(frac - 0.5) < 1e-9;
        }

        /// <summary>The smallest total that CLEARS an Over <paramref name="line"/> outright.</summary>
        private static bool HalfLineThreshold(double line, out int threshold)
        {
            threshold = (int)Math.Floor(line) + 1;
            return IsHalfLine(line);
        }

        /// <summary>The largest total an Under <paramref name="line"/> can still tolerate.</summary>
        private static bool HalfLineMaxAllowed(double line, out int maxAllowed)
        {
            maxAllowed = (int)Math.Floor(line);
            return IsHalfLine(line);
        }
    }
}
