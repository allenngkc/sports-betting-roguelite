using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Game
{
    /// <summary>A leg's outcome AS THE REVEALED LEDGER ALREADY ESTABLISHES IT — never the resolved
    /// match. Distinct from <c>RevealedLegState</c> (TvSweatScreen), which reaches Won/Lost only at
    /// full time and so cannot answer the mid-match question this enum exists for. `Undecided` covers
    /// both "not yet decided" and "not derivable from revealed values". `Voided` is never produced by
    /// this file — it exists so <see cref="SweatActiveLegModel.TicketCannotLose"/> can accept a voided
    /// leg from its caller.</summary>
    public enum RevealedLegOutcome { Undecided, Won, Lost, Voided }

    /// <summary>spec-count-theater-2026-08-17.md §3: an OVER count leg's beat-by-beat
    /// SIGNIFICANCE, classified purely from its DISTANCE TO THE LINE — "an event earns its
    /// treatment from its distance to the line, not from having arrived" (§3.1). Four cases, in
    /// the PRECEDENCE <see cref="SweatActiveLegModel.Classify"/> applies them:
    ///
    /// <list type="bullet">
    /// <item><description><c>Decided</c> — the leg had already cleared its line BEFORE this
    /// beat's own batch. Wins over every other case even where the after-distance arithmetic
    /// would also match Turn or Approach (§3.4: "a resolved leg's corners have no distance to
    /// any line, so they earn nothing").</description></item>
    /// <item><description><c>Turn</c> — this beat's batch is what CROSSES the line: the
    /// decisive beat.</description></item>
    /// <item><description><c>Approach</c> — this beat lands exactly
    /// <see cref="SweatActiveLegModel.ApproachDistance"/> short of the line. "A ramp, not a
    /// switch" (§3.1) — one named rung on that ramp, not the whole of it.</description></item>
    /// <item><description><c>Ordinary</c> — everything else: too far from the line to
    /// weight.</description></item>
    /// </list>
    ///
    /// <para><b>Significant</b> = <c>Approach</c> or <c>Turn</c> — stays a count scene.
    /// <b>Quiet</b> = <c>Ordinary</c> or <c>Decided</c> — falls through to the base table, the
    /// batch committed silently via <c>SceneSpec.QuietCount</c> rather than narrated (spec §4's
    /// binding: a declined batch is still a fact and must still be counted).</para></summary>
    public enum CountSignificance { Ordinary, Approach, Turn, Decided }

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
    /// name). <b>Amended for the outcome enum:</b> that sentence describes
    /// <see cref="ActiveLegInput"/>'s factories only, and it is no longer the complete list of
    /// parameter types in this file — <see cref="ActiveLegCopy"/>'s constructor and the
    /// ticket-level <see cref="SweatActiveLegModel.TicketCannotLose"/>/
    /// <see cref="SweatActiveLegModel.StakeWord"/> also take <see cref="RevealedLegOutcome"/>, a
    /// presentation enum. The law's SUBSTANCE is unchanged: that enum has four fixed values and
    /// no field, so it cannot carry an endpoint even in principle. There is still no parameter,
    /// field, or property anywhere in this file typed as
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
    /// <para><b>Outcome derivation, and the rule that binds it to the copy (the NEED 0 fix).</b>
    /// The outcome is derived wherever the revealed values decide the leg. The STRING changes
    /// only where the old string named a requirement or an allowance that no longer exists.</para>
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

            /// <summary>CorrectScore only: the scoreline the player BACKED, home then away.
            /// <b>This is the bet's own terms, not an endpoint</b> — he chose it, it is printed on
            /// his ticket, and this file's no-leak law is about the MATCH's locked outcome rather
            /// than about the wager. Ints, so nothing that could carry a hidden result comes with
            /// them.</summary>
            public readonly int TargetHome;

            /// <summary>CorrectScore only — see <see cref="TargetHome"/>.</summary>
            public readonly int TargetAway;

            /// <summary>Anytime scorer only: true only at the leg's causal identity payoff — the
            /// same instant <c>TvSweatScreen.ScorerFor</c> would first return the bound actor.
            /// False for every frame before that, including every frame where the backed
            /// player's team has already scored via a different actor.</summary>
            public readonly bool ScorerRevealed;

            private ActiveLegInput(MarketKind kind, MarketChoice choice, double line,
                string backedTeamName, string backedPlayerName,
                int revealedGoalsFor, int revealedGoalsAgainst,
                int revealedCountHome, int revealedCountAway, bool scorerRevealed,
                int targetHome = 0, int targetAway = 0)
            {
                TargetHome = targetHome;
                TargetAway = targetAway;
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

            /// <summary>T96: the DRAW is its own row and carries no backed team — the whole defect
            /// was a draw ticket borrowing a team's string. `Choice` is what tells the describer
            /// which row to render, so it is the one thing this factory must not default.</summary>
            public static ActiveLegInput MoneylineDraw(int revealedGoalsFor, int revealedGoalsAgainst)
                => new ActiveLegInput(MarketKind.Moneyline, MarketChoice.Draw, 0.0, null, null,
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

            /// <summary>T151's CorrectScore arm. <paramref name="revealedHome"/>/
            /// <paramref name="revealedAway"/> are the REVEALED scoreline, home then away — the same
            /// pair <see cref="Moneyline"/> takes, and for this kind they are genuinely home/away
            /// rather than backed-anchored, because a scoreline market backs no side.
            /// <c>SweatFlavor.PickedHomeForPresentation</c> returns true unconditionally for every
            /// kind that is not Moneyline or AnytimeScorer (T152-am), so the caller's
            /// picked/opponent pair IS home/away here. Stated because it is a dependency, not a
            /// coincidence.</summary>
            public static ActiveLegInput CorrectScore(int targetHome, int targetAway,
                int revealedHome, int revealedAway)
                => new ActiveLegInput(MarketKind.CorrectScore, MarketChoice.Yes, 0.0, null, null,
                    revealedHome, revealedAway, 0, 0, false, targetHome, targetAway);

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

            /// <summary>G1: the shorter AUTHORED line for this requirement, or null where the form
            /// cannot overflow. §8 says copy "truncates or chooses a shorter authored line; it never
            /// shrinks" — and T69 established that truncation is the structural backstop, never the
            /// remedy: it can stop broken glyphs, it cannot produce a sentence. When the primary form
            /// misses its measured column the view takes THIS, which is authored to read whole.
            ///
            /// <para><c>"TO WIN"</c> and <c>"TO SCORE"</c> are complete sentences, not amputated ones:
            /// the backed side is already marked on the scorebug and the leg's own row is the
            /// subject. The subject is not missing, it is simply not repeated.</para></summary>
            public readonly string NeedFallback;

            /// <summary>The leg's outcome as the revealed ledger already establishes it (see
            /// <see cref="RevealedLegOutcome"/>). Defaults to <c>Undecided</c> so every call site
            /// that predates this field keeps its old behavior unchanged.</summary>
            public readonly RevealedLegOutcome Outcome;

            public ActiveLegCopy(string need, string live, bool isTeamMarket, string identity,
                                 string needFallback = null,
                                 RevealedLegOutcome outcome = RevealedLegOutcome.Undecided)
            {
                Need = need;
                Live = live;
                IsTeamMarket = isTeamMarket;
                Identity = identity;
                NeedFallback = needFallback;
                Outcome = outcome;
            }
        }

        private const string MarketPick = "MARKET PICK";
        // U+2013 EN DASH, matching the PRD's literal "n–n" scoreline copy.
        private const char Dash = '–';

        /// <summary>The same dash, for the compact statement built in <c>TvSweatScreen</c>. Exposed
        /// rather than duplicated: two copies of one convention is exactly how the two halves of a
        /// statement drift apart, which this file's AnytimeScorer arm already says in terms.</summary>
        public const char DashChar = Dash;
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
                    return DescribeCount(input, "CORNERS", shortNoun: "CNRS");
                case MarketKind.TotalCards:
                    return DescribeCount(input, "CARDS");
                case MarketKind.CorrectScore:
                    return DescribeCorrectScore(input);
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

        // ------------------------------------------------------------------------- ticket words (RISK/STAKE)

        /// <summary>`RISK` is a TICKET word. On a multi-leg ticket one leg winning changes nothing about
        /// it, so this takes EVERY leg's outcome and never a single leg's — the signature is what enforces
        /// that, not discipline.
        ///
        /// <para>True iff the ticket has legs and none of them can still lose it: every leg is Won or
        /// Voided. A voided leg returns its own stake and cannot kill the ticket. This reproduces
        /// `RevealedTicketState.Won`'s own definition and extends it to the revealed-derived case, which is
        /// the point — the enum does not reach Won until full time.</para></summary>
        public static bool TicketCannotLose(IReadOnlyList<RevealedLegOutcome> legOutcomes)
        {
            if (legOutcomes == null || legOutcomes.Count == 0) return false;
            for (int i = 0; i < legOutcomes.Count; i++)
            {
                RevealedLegOutcome outcome = legOutcomes[i];
                if (outcome != RevealedLegOutcome.Won && outcome != RevealedLegOutcome.Voided)
                    return false;
            }
            return true;
        }

        /// <summary>The first word of the ticket footer's pair. `STAKE` is already in the product — the
        /// laptop's margin prints `STAKE $35`. Same figure, same position, same amber, same box: the stake
        /// is still a true fact, it is simply no longer at risk.
        ///
        /// <para>THE DEAD TICKET IS DELIBERATELY NOT BUILT. The capture contains no losing ticket, so the
        /// spec rules only the PRINCIPLE — no word may name a jeopardy or a payout that no longer exists —
        /// and leaves the strings to a frame. A ticket with a Lost leg therefore keeps today's `RISK`.
        /// That is a deliberate omission awaiting evidence, NOT an oversight. Do not invent the
        /// string.</para></summary>
        public static string StakeWord(IReadOnlyList<RevealedLegOutcome> legOutcomes)
            => TicketCannotLose(legOutcomes) ? "STAKE" : "RISK";

        // ------------------------------------------------------------------------- moneyline

        private static ActiveLegCopy DescribeMoneyline(ActiveLegInput l)
        {
            // G1: clubs are named by their DISTINCTIVE WORD, city dropped — "Atlanta Middlemen" ->
            // "MIDDLEMEN". The convention is not new; T69 shipped it on the compact row. The variable
            // was the whole width problem here: `ATLANTA MIDDLEMEN TO WIN` is 24 chars against a
            // ~18-char budget, `MIDDLEMEN TO WIN` is 16.
            //
            // `Identity` deliberately keeps the FULL name: it is the backed-team display name other
            // callers read, and shortening it here would narrow a fact to solve a layout problem.
            string team = (l.BackedTeamName ?? string.Empty).ToUpperInvariant();
            string club = SweatFlavor.Short(l.BackedTeamName ?? string.Empty).ToUpperInvariant();
            string score = $"{l.RevealedGoalsFor}{Dash}{l.RevealedGoalsAgainst}";

            // T96 (batch 68): THE DRAW'S OWN ROW, from the amended deck — NEED `LEVEL AT FULL TIME`
            // over progress `LEVEL` / `NOT LEVEL` (S74), with `LEVEL AT FT` as the authored shorter
            // line. `FT` is this surface's own clock token rather than jargon, and the pair is the
            // same shape as `ONE TEAM SCORELESS` / `ONE TEAM BLANKED`: an 18-char NEED at the budget,
            // carrying a complete fallback rather than a truncation.
            //
            // T70-am ruled the repeated word NO BREACH: `LEVEL` above `LEVEL` is a binary state
            // answering its own requirement in the requirement's word, which is the progress line
            // doing its only job. T70 governs redundant IDENTIFICATION — a NAME printed twice — and
            // forcing a different word below would put a second name on one thing, breaking the
            // one-name-per-thing convention. The cure would be the worse defect.
            //
            // `Identity` is the MARKET PICK, not a team: a draw ticket has no backed side, and that
            // is the whole finding this row exists to fix.
            if (l.Choice == MarketChoice.Draw)
            {
                bool level = l.RevealedGoalsFor == l.RevealedGoalsAgainst;
                // A moneyline leg — the draw row included — can never be decided before full time:
                // a goal at any point up to the whistle can flip LEVEL to NOT LEVEL, so this always
                // reads Undecided from revealed values alone.
                return new ActiveLegCopy("LEVEL AT FULL TIME", level ? "LEVEL" : "NOT LEVEL",
                                         isTeamMarket: false, identity: MarketPick,
                                         needFallback: "LEVEL AT FT", outcome: RevealedLegOutcome.Undecided);
            }

            string need = $"{club} TO WIN";
            string live = l.RevealedGoalsFor > l.RevealedGoalsAgainst ? $"LEADING {score}"
                : l.RevealedGoalsFor < l.RevealedGoalsAgainst ? $"TRAILING {score}"
                : $"LEVEL {score}";
            // G1-am7 (batch 62): a TWO-RUNG LADDER, and the surface picks between the rungs BY
            // MEASUREMENT — `FitOrFallback`, never by truncating.
            //
            //   rung 1  `{CLUB} TO WIN`   fits the 261.0px column for 15 of the 20 clubs
            //   rung 2  `{CLUB} WIN`      carries the other five
            //
            // BARE `TO WIN` IS RETIRED as this arm's fallback and must not be reachable on a moneyline
            // leg. It was the cheap answer and it is not available: the column's live row advances to
            // leg N+1 the instant leg N resolves while the scorebug holds leg N's fixture until the
            // next leg stages (T94), so for the whole won/dead beat a bare form would name no side at
            // all — and the backed-side marker it would have leaned on is pointing at a different
            // fixture. The club has to be named.
            //
            // `{CLUB} WIN` rather than anything shorter: abbreviating the noun is refused (it is
            // ALREADY the club's short form — the compact rows print `MUSKRATS ML`, not
            // `Tulsa Muskrats` — so there is nothing left to shorten without coining), HOME/AWAY binds
            // to the fixture on screen and fails in exactly the window it is needed, and `{CLUB} ML`
            // states a market rather than a requirement.
            //
            // It is the slot's own register: the deck is terse declarative — `ONE TEAM BLANKED`,
            // `ONE TEAM SCORELESS` are subject + required state — and every noun in the pool is
            // plural, so `SPREADSHEETS WIN` is that same shape and grammatical.
            //
            // MEASURED, all twenty against 261.0: rung 2 overruns for NONE of them. The widest form
            // actually reached is `SPREADSHEETS WIN` at 249.5px, 11.5px spare.
            // Same reason as the draw arm above: goals can always change a moneyline result up to
            // full time, so a revealed-only read can never call this leg decided.
            return new ActiveLegCopy(need, live, isTeamMarket: true, identity: team,
                                     needFallback: $"{club} WIN", outcome: RevealedLegOutcome.Undecided);
        }

        // ------------------------------------------------------------------------- total goals

        private static ActiveLegCopy DescribeTotalGoalsOver(ActiveLegInput l)
        {
            string need = $"OVER {l.Line:0.0} GOALS";
            int total = l.RevealedGoalsFor + l.RevealedGoalsAgainst;
            string live;
            RevealedLegOutcome outcome;
            if (HalfLineThreshold(l.Line, out int threshold))
            {
                // The clamp that used to sit here (`Math.Max(0, threshold - total)`) is the NEED-0
                // defect: once `remaining` reaches zero the requirement is already satisfied, and
                // clamping kept naming it forever after. The form is now selected BY the outcome —
                // a cleared line prints WON, never a MORE count of anything.
                int remaining = threshold - total;
                if (remaining <= 0)
                {
                    outcome = RevealedLegOutcome.Won;
                    live = $"{total} GOALS {Bullet} WON";
                }
                else
                {
                    outcome = RevealedLegOutcome.Undecided;
                    live = $"{total} GOALS {Bullet} {remaining} MORE";
                }
            }
            else
            {
                outcome = RevealedLegOutcome.Undecided;
                live = $"{total} GOALS";
            }
            return new ActiveLegCopy(need, live, isTeamMarket: false, identity: MarketPick, outcome: outcome);
        }

        private static ActiveLegCopy DescribeTotalGoalsUnder(ActiveLegInput l)
        {
            string need = $"UNDER {l.Line:0.0} GOALS";
            int total = l.RevealedGoalsFor + l.RevealedGoalsAgainst;
            string live;
            RevealedLegOutcome outcome;
            if (HalfLineMaxAllowed(l.Line, out int maxAllowed))
            {
                int slack = maxAllowed - total;
                if (slack < 0)
                {
                    outcome = RevealedLegOutcome.Lost;
                    live = $"{total} GOALS {Bullet} LOST";
                }
                else
                {
                    // Same LIMIT 0 rule as DescribeCount below: zero slack is still live — one more
                    // goal kills it, but none has happened yet. Not the NEED-0 defect's shape.
                    outcome = RevealedLegOutcome.Undecided;
                    live = $"{total} GOALS {Bullet} LIMIT {slack}";
                }
            }
            else
            {
                outcome = RevealedLegOutcome.Undecided;
                live = $"{total} GOALS";
            }
            return new ActiveLegCopy(need, live, isTeamMarket: false, identity: MarketPick, outcome: outcome);
        }

        // ------------------------------------------------------------------------- BTTS

        private static ActiveLegCopy DescribeBttsYes(ActiveLegInput l)
        {
            int scored = (l.RevealedGoalsFor > 0 ? 1 : 0) + (l.RevealedGoalsAgainst > 0 ? 1 : 0);
            string live = $"{scored}/2 TEAMS SCORED";
            // G1: "BOTH TEAMS TO SCORE" (19) was a permanently marginal CONSTANT — no variable in it
            // at all, so it was over budget on every frame it ever drew. One word clears it.
            // Outcome only, no string change: "2/2 TEAMS SCORED" already names no requirement, so
            // there is nothing stale left inside it once both sides have scored.
            RevealedLegOutcome outcome = scored == 2 ? RevealedLegOutcome.Won : RevealedLegOutcome.Undecided;
            return new ActiveLegCopy("BOTH TEAMS SCORE", live, isTeamMarket: false, identity: MarketPick,
                                     outcome: outcome);
        }

        /// <summary>T151's CorrectScore forms, unblocked at T161 — one of only two of G1's nine
        /// kinds that CLEARS IN EVERY SLOT, so it needs no rung and no re-authoring.
        ///
        /// <para><b>The outcome is ALWAYS Undecided, and that is the market's defining property
        /// rather than caution on this seat's part.</b> G1's own monotonicity table has CorrectScore
        /// as the quantity that is NOT monotone — <i>you can be ON it and drift off</i>. Reporting
        /// <c>Won</c> while the score currently matches would be a state the very next goal
        /// falsifies, which is T108's family.</para></summary>
        private static ActiveLegCopy DescribeCorrectScore(ActiveLegInput l)
        {
            // Revealed-only, both sides: the comparison is between two REVEALED counters and the
            // ticket's own target. Nothing here reads the match's locked scoreline.
            string target = $"{l.TargetHome}{Dash}{l.TargetAway}";
            bool met = l.RevealedGoalsFor == l.TargetHome && l.RevealedGoalsAgainst == l.TargetAway;
            return new ActiveLegCopy($"{target} AT FULL TIME", met ? "MET" : "NOT YET",
                                     isTeamMarket: false, identity: MarketPick,
                                     needFallback: $"{target} AT FT",
                                     outcome: RevealedLegOutcome.Undecided);
        }

        private static ActiveLegCopy DescribeBttsNo(ActiveLegInput l)
        {
            // "BOTH HAVE SCORED" is read off two REVEALED counters, both already causally
            // gated by ScoreLedger.CompleteGoal — never a projection from the locked endpoint.
            bool bothScored = l.RevealedGoalsFor > 0 && l.RevealedGoalsAgainst > 0;
            string live = bothScored ? "BOTH HAVE SCORED" : "CLEAN-SHEET PATH LIVE";
            // G1: "KEEP ONE TEAM SCORELESS" (23) was over budget as a constant, and "KEEP" was also a
            // §8 register problem — an instruction to the player about a thing he cannot influence.
            // The requirement is a state of the match, so the copy names the state.
            // Outcome only, no string change: "BOTH HAVE SCORED" already states the fact that kills
            // this leg — there is no stale requirement word left inside it to fix.
            RevealedLegOutcome outcome = bothScored ? RevealedLegOutcome.Lost : RevealedLegOutcome.Undecided;
            return new ActiveLegCopy("ONE TEAM SCORELESS", live, isTeamMarket: false, identity: MarketPick,
                                     needFallback: "ONE TEAM BLANKED", outcome: outcome);
        }

        // ------------------------------------------------------------------------- corners / cards

        /// <param name="shortNoun">G1's LAST-RESORT abbreviation for this noun, or null where the
        /// full word always fits. `UNDER 10.5 CORNERS` is 18 and sits exactly at the budget; CARDS
        /// is four characters shorter and cannot overflow, so it has none. The deck is explicit that
        /// the full word is preferred and the abbreviation is reached only if the measurement says
        /// it must be.</param>
        private static ActiveLegCopy DescribeCount(ActiveLegInput l, string noun, string shortNoun = null)
        {
            bool over = l.Choice == MarketChoice.Over;
            string need = $"{(over ? "OVER" : "UNDER")} {l.Line:0.0} {noun}";
            string needFallback = shortNoun == null
                ? null
                : $"{(over ? "OVER" : "UNDER")} {l.Line:0.0} {shortNoun}";
            int total = l.RevealedCountHome + l.RevealedCountAway;
            string live;
            RevealedLegOutcome outcome;
            if (over && HalfLineThreshold(l.Line, out int threshold))
            {
                // THE DEFECT LIVED HERE: `Math.Max(0, threshold - total)` clamped a cleared
                // requirement to zero and kept printing it — "10 CORNERS • NEED 0" — for as long as
                // the leg stayed on screen after it was already won. The clamp is deleted; the form
                // is selected BY the outcome instead, so NEED 0 is unconstructible rather than
                // guarded.
                int remaining = threshold - total;
                if (remaining <= 0)
                {
                    outcome = RevealedLegOutcome.Won;
                    live = $"{total} {noun} {Bullet} WON";
                }
                else
                {
                    outcome = RevealedLegOutcome.Undecided;
                    live = $"{total} {noun} {Bullet} NEED {remaining}";
                }
            }
            else if (!over && HalfLineMaxAllowed(l.Line, out int maxAllowed))
            {
                int slack = maxAllowed - total;
                if (slack < 0)
                {
                    outcome = RevealedLegOutcome.Lost;
                    live = $"{total} {noun} {Bullet} LOST";
                }
                else
                {
                    // LIMIT 0 IS TRUE AND STAYS. This looks like the same defect as NEED 0 — a
                    // number sitting at its floor — and it is not: NEED 0 named a requirement that
                    // had already stopped existing, while LIMIT 0 names an allowance that is still
                    // real. An under leg with zero slack is still live; one more of this stat kills
                    // it, but none has happened yet. Do not "fix" this to Won or Lost.
                    outcome = RevealedLegOutcome.Undecided;
                    live = $"{total} {noun} {Bullet} LIMIT {slack}";
                }
            }
            else
            {
                // Whole-number line: a push is possible, so this class declines to fabricate an
                // exact remaining/allowed count rather than guess (see the half-line math section
                // below).
                outcome = RevealedLegOutcome.Undecided;
                live = $"{total} {noun}";
            }
            return new ActiveLegCopy(need, live, isTeamMarket: false, identity: MarketPick,
                                     needFallback: needFallback, outcome: outcome);
        }

        // ------------------------------------------------------------------------- anytime scorer

        private static ActiveLegCopy DescribeAnytimeScorer(ActiveLegInput l)
        {
            // G1: players are named by SURNAME — the convention the progress line below already used.
            // This is the T69 case itself: `RICO LANYARD TO SCORE` (21) is the string that rendered
            // as `RICO LANYARD TO`. `LANYARD TO SCORE` is 16.
            string need = $"{Surname(l.BackedPlayerName).ToUpperInvariant()} TO SCORE";
            // SCORED is admissible ONLY at the causal identity payoff (input.ScorerRevealed),
            // which the caller sets from the same gate as TvSweatScreen.ScorerFor — never
            // inferred here from a revealed goal count, since the backed player's own team can
            // score via a different actor without the backed player having scored (PRD §4.1,
            // TVS-H03's exact defect class).
            // G1, the pair-defect: NEED and this line are ONE authored pair, and they both named the
            // surname — `LANYARD TO SCORE` over `WAITING FOR LANYARD`, three lines apart, saying the
            // same thing. That is T69's defect (a fact named twice in one statement) reproduced
            // vertically instead of horizontally. The player is named ONCE, by NEED directly above.
            string live = l.ScorerRevealed ? "SCORED" : "NOT YET";
            // Outcome only, no string change: "SCORED" already names no requirement left to void.
            RevealedLegOutcome outcome = l.ScorerRevealed ? RevealedLegOutcome.Won : RevealedLegOutcome.Undecided;
            // G1-am8 (batch 63): the SAME two-rung ladder as the moneyline arm, chosen by measurement.
            //
            //   rung 1  `{SURNAME} TO SCORE`
            //   rung 2  `{SURNAME} SCORES`
            //
            // BARE `TO SCORE` IS RETIRED and must not be reachable on a scorer leg. It named no
            // player — the exact property G1-am7 retired bare `TO WIN` for — and it is WORSE here:
            // the backed-side marker renders only on MONEYLINE legs, so a scorer leg has no marker at
            // all and nothing else on the surface names the player. G1's own pair-defect ruling makes
            // it decisive: the progress line reads `NOT YET`/`SCORED` precisely BECAUSE the surname is
            // named once, by the NEED line above it. Retire the surname here and it is named nowhere.
            //
            // THE RUNG-2 RULE IS ONE RULE ACROSS BOTH ARMS: drop the infinitive marker and conjugate
            // to the subject. Clubs are plural and take `WIN`; a surname is singular and takes
            // `SCORES`. No new vocabulary, and it keeps the deck's terse-declarative register —
            // subject + required state, like `ONE TEAM BLANKED`.
            //
            // MEASURED, all twelve surnames against 261.0: rung 2 overruns for NONE. Only
            // `PAVEMENT TO SCORE` (264.9) falls to rung 2, and `PAVEMENT SCORES` is 238.4px with
            // 22.6px spare. The retired bare form was 119.8px — it always fit, and fitting was never
            // the problem with it.
            return new ActiveLegCopy(need, live, isTeamMarket: false, identity: MarketPick,
                                     needFallback: $"{Surname(l.BackedPlayerName).ToUpperInvariant()} SCORES",
                                     outcome: outcome);
        }

        /// <summary>G1's player-naming convention: surname, uppercase. Exposed because the TV's
        /// COMPACT statement needs the identical rule (`{SURNAME} ANYTIME`) — two copies of one
        /// convention is how the two halves of a statement drift apart, which is the defect class
        /// this whole deck exists to close.</summary>
        internal static string Surname(string fullName)
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

        /// <summary>The smallest total that CLEARS an Over <paramref name="line"/> outright.
        ///
        /// <para>Widened from private to internal (spec-count-theater-2026-08-17.md §3): shared,
        /// not duplicated — the ticket column and the theater's distance gate must never derive
        /// the clearing point two different ways. <c>TheaterChoreographer.ResolveBeat</c> (same
        /// assembly, SBR.Game — no <c>InternalsVisibleTo</c> needed) calls this directly to
        /// decide the non-half-line "cannot classify" fallback BEFORE ever calling
        /// <see cref="Classify"/> (§3: "when significance cannot be computed, keep today's
        /// behaviour... falling back to loud is merely the status quo"). Still not public:
        /// nothing outside this assembly needs it, and unlike <c>SBR.Tests.PlayMode</c>, the
        /// EditMode test assembly has no <c>InternalsVisibleTo</c> grant (see this project's
        /// <c>AssemblyInfo.cs</c>) — widened exactly as far as the one real caller needs, no
        /// further.</para></summary>
        internal static bool HalfLineThreshold(double line, out int threshold)
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

        // ------------------------------------------------------------------------- count significance (spec-count-theater-2026-08-17.md §3)

        /// <summary>spec-count-theater-2026-08-17.md §3.1: the ramp's one named rung before the
        /// line. THE SPEC ITSELF ROUTES THIS NUMBER TO THE DD, IN TERMS: it asks for "a ramp, not
        /// a switch" while its own gate is a binary "significance threshold" test, and it names
        /// no value for that threshold — the tension is in the spec text, not manufactured here.
        /// <c>1</c> stands in for an UNRULED number, not a ruling: it is what keeps the ramp
        /// exactly one edit wide (change this one constant) if/when the DD rules a wider approach
        /// window. Do not read this value as a design decision this file is making.</summary>
        public const int ApproachDistance = 1;

        /// <summary>spec-count-theater-2026-08-17.md §3.3: "the theater asks the question the
        /// column already answers" — <see cref="DescribeCount"/> already derives
        /// <c>threshold − total</c> from revealed values to print "8 CORNERS • NEED 1"; this
        /// reuses that exact <see cref="HalfLineThreshold"/> math rather than re-deriving a
        /// second copy of it, so the column and the theater can never quietly disagree about
        /// where the line is.
        ///
        /// <para><b>THE NO-LEAK LAW, enforced by the signature — the identical law and the
        /// identical enforcement this file's header already states for
        /// <see cref="ActiveLegInput"/></b> (spec §7 item 3: "significance is computed from
        /// REVEALED values, no path from the locked target"). The three parameters are
        /// <c>int</c>, <c>int</c>, <c>double</c> — plain already-revealed values, never
        /// <c>Leg</c>, <c>CountLedger</c>, <c>MatchStatLine</c>, or <c>ScoreLedger</c>, and
        /// nothing that can reach <c>TargetHome</c>/<c>TargetAway</c>/<c>TargetTotal</c>. A
        /// reviewer can confirm the no-leak property from this signature alone, exactly as with
        /// <see cref="ActiveLegInput"/>: there is nothing here to leak from, because nothing
        /// carrying a hidden endpoint is ever accepted. The caller
        /// (<c>TheaterChoreographer.ResolveBeat</c>) passes <c>countLedger.Home +
        /// countLedger.Away</c> as <paramref name="revealedTotal"/> — never <c>TargetHome</c>/
        /// <c>TargetAway</c>/<c>TargetTotal</c>.</para>
        ///
        /// <para><b>Precondition — the caller decides computability, this method does not:</b>
        /// callers must confirm <see cref="HalfLineThreshold"/> for <paramref name="line"/>
        /// THEMSELVES before calling this (which is exactly why that method widened from private
        /// to internal). A whole-number line admits a push, so its "threshold" is not an exact
        /// clearing point; classifying against it would be invented, not derived. Rather than
        /// smuggle a fifth, dishonest case into <see cref="CountSignificance"/> for "cannot
        /// tell", the spec's default-loud fallback ("when significance cannot be computed, keep
        /// today's behaviour") is the CALLER's policy decision, made by skipping this method
        /// entirely — see <c>TheaterChoreographer.ResolveBeat</c>'s own gate.</para></summary>
        public static CountSignificance Classify(int revealedTotal, int stagedDelta, double line)
        {
            // HalfLineThreshold's own bool return is not re-checked here — see the precondition
            // above; only its out-value is used. A caller that skips the precondition gets a
            // threshold computed against a line that may admit a push, which is exactly the
            // "invented, not derived" outcome the precondition exists to keep out of production.
            HalfLineThreshold(line, out int threshold);
            int distanceBefore = threshold - revealedTotal;
            int distanceAfter = threshold - (revealedTotal + stagedDelta);

            // Precedence is load-bearing, not incidental: Decided must win even where
            // distanceAfter's own arithmetic would also satisfy Turn (a batch staged well after
            // an already-cleared line stays Decided, never Turn) — §3.4, "a resolved leg's
            // corners have no distance to any line". A non-negative stagedDelta (the only shape
            // a real StagedCount ever takes) makes a Decided/Approach collision impossible
            // (distanceAfter <= distanceBefore <= 0 leaves no room to land on the positive
            // ApproachDistance), but the ORDER below still guards the general case.
            if (distanceBefore <= 0) return CountSignificance.Decided;
            if (distanceAfter <= 0) return CountSignificance.Turn;
            if (distanceAfter == ApproachDistance) return CountSignificance.Approach;
            return CountSignificance.Ordinary;
        }
    }
}
