using System;
using System.Collections.Generic;
using System.Globalization;
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

            /// <summary>`T152`'s handicap arm. <paramref name="backedClubShort"/> is ALREADY the
            /// club's short form — the caller applies `SweatFlavor.Short`, as every other arm here
            /// requires, because this file may not reference UnityEngine or `SweatFlavor`.
            ///
            /// <para><b><paramref name="revealedFor"/>/<paramref name="revealedAgainst"/> must be
            /// anchored to the BACKED side, and that is not what the caller's ledger holds.</b>
            /// `SweatFlavor.PickedHomeForPresentation` returns true UNCONDITIONALLY for every kind
            /// that is not Moneyline or AnytimeScorer (`T152-am`), so `ScoreLedger.Picked` is HOME
            /// for a handicap whichever side was backed. `T152-am` is the row that found this: *"a
            /// team total on the AWAY side anchors HOME"*. The caller therefore swaps the pair when
            /// the away side is backed, and this factory's parameter NAMES the requirement so a
            /// later caller cannot pass the ledger straight through.</para>
            ///
            /// <para><paramref name="line"/> is SIGNED and applied to the backed side — home −1.5
            /// must win by two, away +1.5 may lose by one (`Domain.cs`'s own words).</para></summary>
            public static ActiveLegInput Handicap(string backedClubShort, double line,
                int revealedFor, int revealedAgainst)
                => new ActiveLegInput(MarketKind.Handicap, line < 0 ? MarketChoice.Home : MarketChoice.Away,
                    line, backedClubShort, null, revealedFor, revealedAgainst, 0, 0, false);

            /// <summary>`T151`'s winning-margin arm. <paramref name="bucket"/> is the margin bucket
            /// and <paramref name="isTopBucket"/> says whether it is the "or more" one — the caller
            /// reads that from the ENGINE's own published field rather than from a constant this
            /// assembly cannot see (`MatchModel.TopMarginBucket` is `internal` to the engine).
            ///
            /// <para>The revealed pair is HOME then AWAY, as `CorrectScore`'s is and for the same
            /// stated reason: a margin market backs no side, so the caller's picked/opponent pair IS
            /// home/away under `T152-am`'s unconditional-home rule.</para></summary>
            public static ActiveLegInput WinningMargin(int bucket, bool isTopBucket,
                int revealedHome, int revealedAway)
                => new ActiveLegInput(MarketKind.WinningMargin,
                    isTopBucket ? MarketChoice.Over : MarketChoice.Yes, bucket, null, null,
                    revealedHome, revealedAway, 0, 0, false);

            /// <summary>`T151`'s odd/even arm. Revealed pair is HOME then AWAY — same basis as
            /// <see cref="WinningMargin"/>; only their SUM is read, so the order is immaterial and
            /// is kept consistent rather than left to chance.</summary>
            public static ActiveLegInput TotalGoalsOddEven(bool odd, int revealedHome, int revealedAway)
                => new ActiveLegInput(MarketKind.TotalGoalsOddEven,
                    odd ? MarketChoice.Odd : MarketChoice.Even, 0.0, null, null,
                    revealedHome, revealedAway, 0, 0, false);

            /// <summary>`T152`'s multi-scorer arm. <paramref name="backedPlayerName"/> is the FULL
            /// name — <see cref="Surname"/> is applied inside the describer, exactly as
            /// <see cref="AnytimeScorer"/> does, so the surname convention has one owner.
            ///
            /// <para><paramref name="revealedGoals"/> is the backed player's goal count as the TV
            /// has REVEALED it, never a count read off `MatchStatLine` — the same discipline every
            /// other field in this struct is under (PRD §8.2/§9).</para></summary>
            public static ActiveLegInput PlayerMultiScorer(string backedPlayerName, int needed, int revealedGoals)
                => new ActiveLegInput(MarketKind.PlayerMultiScorer, MarketChoice.Yes, needed, null,
                    backedPlayerName, revealedGoals, 0, 0, 0, revealedGoals > 0);
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

            /// <summary>`G1-am11` §3.2/§3.3: a THIRD authored rung, null where the ladder has two.
            ///
            /// <para>Added because two of the four kinds `T169` orders built need three and the
            /// register says so in terms — `WinningMargin` takes `2 APART AT FT` under
            /// `2 GOALS APART AT FT`, and `Handicap` takes `{CLUB} ±1.5` under `{CLUB} WITHIN 1`.
            /// TV measured rungs 1 AND 2 missing the 261.0px band for every margin bucket (366.4 /
            /// 268.8 and 380.8 / 283.2), so without this field those legs land on the truncation
            /// floor — which `T161` measured as the dangling `2 GOALS APART AT`.</para>
            ///
            /// <para><b>A field rather than a wider change to <c>FitOrFallback</c>.</b> That method's
            /// two-rung signature is reached BY REFLECTION from four EditMode gates; widening it to
            /// <c>params</c> silently breaks every one of them. <c>TvSweatScreen.FitLadder</c> is the
            /// N-rung walker and <c>FitOrFallback</c> now delegates to it, so there is still exactly
            /// one definition of what "fits" means.</para></summary>
            public readonly string NeedFallback2;

            public ActiveLegCopy(string need, string live, bool isTeamMarket, string identity,
                                 string needFallback = null,
                                 RevealedLegOutcome outcome = RevealedLegOutcome.Undecided,
                                 string needFallback2 = null)
            {
                Need = need;
                Live = live;
                IsTeamMarket = isTeamMarket;
                Identity = identity;
                NeedFallback = needFallback;
                Outcome = outcome;
                NeedFallback2 = needFallback2;
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
                // ---- T169's FOUR. The copy is not authored here: `T151` (batch 137) and `T152`
                // (batch 138) authored the forms and `G1-am11` §3 (batch 159) added the third rungs.
                // What was missing was an ARM, which is `C57`'s discriminator exactly — in the
                // register, absent from the build. Nothing below is re-authored.
                case MarketKind.Handicap:
                    return DescribeHandicap(input);
                case MarketKind.WinningMargin:
                    return DescribeWinningMargin(input);
                case MarketKind.TotalGoalsOddEven:
                    return DescribeTotalGoalsOddEven(input);
                case MarketKind.PlayerMultiScorer:
                    return DescribePlayerMultiScorer(input);
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

        // ------------------------------------------------------------------------- T169's four
        //
        // FOUR ARMS, ZERO NEW COPY. Every string below is `T151`/`T152`/`G1-am11` §3 verbatim, and
        // every rung was measured against the 261.0px band before this was written (the commit that
        // added `T169_measure_the_four_kinds_authored_rungs`). What the register carried and the
        // build did not was an ARM — `C57`'s discriminator, and the reason `T130`'s blank-row fix
        // left six kinds still reaching `DescribeActiveLeg`'s `default:`.

        /// <summary>`T152`'s handicap arm — NEED over a three-rung ladder, progress on the ADJUSTED
        /// margin.
        ///
        /// <para><b>The moneyline's progress report is REFUSED here and `T152` says why:</b> at
        /// <c>1-0</c> with <c>-1.5</c> the bet is LOSING while <c>LEADING 1-0</c> says otherwise —
        /// `T108`'s family, a word naming a state that does not apply. So the pair is
        /// <c>CLEAR BY {n}</c> / <c>TRAILING BY {n}</c> and it reports the handicap position, never
        /// the scoreline.</para>
        ///
        /// <para><b>⚠ WHAT <c>{n}</c> COUNTS IS NOT RULED, AND THIS IS THE LANE'S READING, ROUTED.</b>
        /// `T152` authored the two forms and did not define the number. Implemented as <b>the goals
        /// that would have to change to flip the leg</b> — <c>ceil(|margin + line|)</c> — because
        /// that is the only reading that is both revealed-only and actionable: at <c>2-0</c> on
        /// <c>-1.5</c> the adjusted margin is <c>+0.5</c>, ONE goal against flips it, and the row
        /// reads <c>CLEAR BY 1</c>. A reading that printed <c>0.5</c> would put a half-goal on a
        /// surface that counts in goals. <b>The DD may rule otherwise; this is one line to
        /// change.</b></para>
        ///
        /// <para><c>ON THE LINE</c> is UNCONSTRUCTIBLE and `T152` struck it for that reason: the
        /// engine offers ±1.5 ONLY (`RunConfig.HandicapLines`), so the adjusted margin is a
        /// half-integer and never zero. Two states suffice — asserted by construction here rather
        /// than guarded, which is `T108`'s own preferred shape.</para>
        ///
        /// <para>The outcome is ALWAYS <c>Undecided</c>: a handicap is settled by the scoreline at
        /// the whistle and any goal up to it can flip the adjusted margin, exactly as the moneyline
        /// arm above argues for itself.</para></summary>
        private static ActiveLegCopy DescribeHandicap(ActiveLegInput l)
        {
            string club = l.BackedTeamName;
            // The revealed pair arrives BACKED-side anchored (see the factory), so this subtraction
            // is the backed side's margin without needing to know which side that was.
            int margin = l.RevealedGoalsFor - l.RevealedGoalsAgainst;
            double adjusted = margin + l.Line;
            int flip = (int)Math.Ceiling(Math.Abs(adjusted));
            string live = adjusted > 0 ? $"CLEAR BY {flip}" : $"TRAILING BY {flip}";
            // `+0.0;-0.0` is `MatchModel.Fields`' own handicap format, reused rather than re-typed:
            // a sign convention that drifts between two surfaces describing one bet is `T62`'s class.
            string signed = l.Line.ToString("+0.0;-0.0", CultureInfo.InvariantCulture);
            // G1-am11 §3.3's ladder. Rung 2 rescues SHORT clubs only — `MUSKRATS WITHIN 1` is the
            // widest FITTING string in the whole band at 259.2px — and rung 3 is the market's own
            // notation, two tokens, so its only truncation is the bare club: an identity loss, never
            // a misstatement. Measured over the saturated 20-noun pool: TRUNCATED 0 on both signs.
            string rung1 = l.Line < 0 ? $"{club} TO WIN BY 2+" : $"{club} WITHIN 1 GOAL";
            string rung2 = l.Line < 0 ? $"{club} BY 2+" : $"{club} WITHIN 1";
            return new ActiveLegCopy(rung1, live, isTeamMarket: false, identity: MarketPick,
                                     needFallback: rung2, outcome: RevealedLegOutcome.Undecided,
                                     needFallback2: $"{club} {signed}");
        }

        /// <summary>`T151`'s winning-margin arm — three rungs, and the third is the one `G1-am11`
        /// §3.2 added because the first two both miss.
        ///
        /// <para><b>This arm's absence was not a silence, it was a WRONG STRING.</b> Unauthored, the
        /// kind reached <c>LegStatement</c>'s <c>default:</c> → <c>MatchModel.Fields</c> →
        /// <c>3+ GOALS</c> — and `T151` chose `MARGIN`/`APART` precisely to avoid that: *"the
        /// engine's bare `2 GOALS` collides with the total-goals family's own forms on the same
        /// column."* `T169` raised its priority for exactly this reason.</para>
        ///
        /// <para><b>`GOALS` is the token that goes</b> on rung 3, not `APART`: on a surface whose
        /// scorebug prints the scoreline two slots away *apart* is not ambiguous, and `T151` already
        /// chose `APART` as this market's distinguishing word. `MARGIN` was CONSIDERED for rung 3 and
        /// REFUSED — it is `MarketKind.WinningMargin`'s own root AND the laptop ships
        /// `YOUR MARGIN IS CLEAR` meaning *winning comfortably*: one word, two meanings, two
        /// surfaces. The compact keeps `MARGIN` and is not reopened.</para>
        ///
        /// <para><b>BUCKET 1 IS AUTHORED — `T151-am3` (DD batch 196), on this lane's routed
        /// question.</b> It was the one bucket with no form, it is OFFERED (12 seen in one run on a
        /// real board), and the DD's ruling names why that mattered: <b>a one-goal margin is the
        /// commonest result in the sport, so this is not an edge bucket — it is THE bucket.</b>
        /// Unauthored it rendered the engine's bare <c>1 GOAL</c>, the exact collision `T151` exists
        /// to prevent. <b>Nothing was coined here while it was unruled</b>, which cost one commit and
        /// is what `G1` asks for.</para>
        ///
        /// <para>The singular <c>GOAL</c> matches the engine's own
        /// <c>{b} GOAL{(b == 1 ? "" : "S")}</c>, and the word placement is `G1-am11`'s rather than a
        /// new call: <c>MARGIN</c> stays IN the compact and OUT of the NEED band, and rung 3 drops
        /// <c>GOALS</c> and keeps <c>APART</c> exactly as <c>3+ APART AT FT</c> does.</para>
        ///
        /// <para>`G1`'s monotonicity table has the margin as NOT monotone — *a margin can SHRINK* —
        /// so the progress pair is `MET`/`NOT YET` and the outcome is always <c>Undecided</c>: a
        /// backer can be exactly right in the 60th minute and entirely unsettled.</para></summary>
        private static ActiveLegCopy DescribeWinningMargin(ActiveLegInput l)
        {
            int bucket = (int)l.Line;
            if (bucket < 1)
                throw new ArgumentOutOfRangeException(nameof(l), bucket,
                    "WinningMargin buckets start at 1 (MarketSelection says so and throws there too).");
            // Revealed-only, both sides. `>=` for the top bucket is the market's own rule
            // (MatchModel: `bucket >= TopMarginBucket ? margin >= bucket : margin == bucket`),
            // carried here through the factory's flag rather than through a constant this assembly
            // cannot see.
            int margin = Math.Abs(l.RevealedGoalsFor - l.RevealedGoalsAgainst);
            bool top = l.Choice == MarketChoice.Over;
            bool met = top ? margin >= bucket : margin == bucket;
            string b = top ? $"{bucket}+" : bucket.ToString(CultureInfo.InvariantCulture);
            // `GOAL` SINGULAR AT BUCKET 1 — `T151-am3`, and it matches the engine's own
            // `{b} GOAL{(b == 1 ? "" : "S")}` rather than restating the rule differently. The top
            // bucket is "1+ OR MORE" and can never be one, so the singular is reachable only at the
            // exact-1 bucket, which is exactly where the engine puts it.
            string goals = !top && bucket == 1 ? "GOAL" : "GOALS";
            return new ActiveLegCopy($"{b} {goals} APART AT FULL TIME", met ? "MET" : "NOT YET",
                                     isTeamMarket: false, identity: MarketPick,
                                     needFallback: $"{b} {goals} APART AT FT",
                                     outcome: RevealedLegOutcome.Undecided,
                                     needFallback2: $"{b} APART AT FT");
        }

        /// <summary>`T151`'s odd/even arm. Two rungs, and the second is the ladder the DRAW already
        /// uses — <c>AT FULL TIME</c> → <c>AT FT</c>, nothing new invented.
        ///
        /// <para><b>Rung 1 does not render, and that is measured rather than assumed.</b>
        /// <c>ODD TOTAL AT FULL TIME</c> is 314.9px and <c>EVEN TOTAL AT FULL TIME</c> 326.5px
        /// against the 261.0px band; rung 2 carries both at 217.3 and 228.9. `T161` read TV's
        /// per-form pass as *"the ladder saves it"* and the spec asked for that CONFIRMED rather
        /// than inherited — it is, and the confirmation is what shows rung 1 is unreachable.</para>
        ///
        /// <para>`G1`'s table has the parity as NOT monotone — *every goal flips it* — so the pair is
        /// `MET`/`NOT YET` and the outcome stays <c>Undecided</c>.</para></summary>
        private static ActiveLegCopy DescribeTotalGoalsOddEven(ActiveLegInput l)
        {
            // Read off the two REVEALED counters, never a projection from the locked endpoint.
            int total = l.RevealedGoalsFor + l.RevealedGoalsAgainst;
            bool odd = l.Choice == MarketChoice.Odd;
            bool met = (total % 2 == 1) == odd;
            // `ODD TOTAL`, not `TOTAL ODD`: the compact is the identity line and reads total→parity;
            // the NEED states the requirement and its subject is the parity. Both are T151's.
            string word = odd ? "ODD" : "EVEN";
            return new ActiveLegCopy($"{word} TOTAL AT FULL TIME", met ? "MET" : "NOT YET",
                                     isTeamMarket: false, identity: MarketPick,
                                     needFallback: $"{word} TOTAL AT FT",
                                     outcome: RevealedLegOutcome.Undecided);
        }

        /// <summary>`T152`'s multi-scorer arm — two rungs, and rung 2 is DELIBERATELY IDENTICAL to
        /// the compact, which <c>LegStatement</c>'s own doc sanctions: *"where those two questions
        /// have the same answer, the two strings are IDENTICAL, and that is correct rather than a
        /// duplication to design away."*
        ///
        /// <para><b><c>{SURNAME} SCORES 2+</c> WAS REJECTED and the reason is test 2 working rather
        /// than taste:</b> it truncates to <c>{SURNAME} SCORES</c>, which is the shipped
        /// AnytimeScorer rung — a 2+ leg rendering as an anytime leg. Rejected on collision.</para>
        ///
        /// <para><b>The progress line is the COUNT grammar, not AnytimeScorer's binary flag</b>, and
        /// `T152` states why: at one goal on a <c>2+</c> leg the player HAS scored and the leg is
        /// NOT won, so <c>SCORED</c> would read as a win. It takes `DescribeCount`'s own
        /// <c>{n} GOALS • NEED {m}</c> shape on the player's revealed goals, and `T108`'s rule holds
        /// by the same construction that arm uses — <c>NEED 0</c> is unconstructible because the WON
        /// form is selected by the outcome instead of the remainder being clamped.</para>
        ///
        /// <para><b>⚠ THE REVEALED COUNT IS NEW PLUMBING AND ITS SURFACE HALF IS UNPROVEN ON A
        /// BEAT.</b> This arm is gated in EditMode over constructed inputs, which is what this
        /// file's plain-int design is FOR. The counter that feeds it lives in
        /// <c>TvSweatScreen.OnGoalPlayed</c> and no PlayMode fixture has yet driven a multi-scorer
        /// leg through a goal — stated plainly rather than left for a later seat to discover, and
        /// it is this lane's own trap #1: a gate that runs while its case does not.</para></summary>
        private static ActiveLegCopy DescribePlayerMultiScorer(ActiveLegInput l)
        {
            // The surname is taken HERE, from the same helper AnytimeScorer uses — two copies of one
            // convention is how the two halves of a statement drift apart (this file's own words).
            string surname = Surname(l.BackedPlayerName);
            int needed = (int)l.Line;
            int scored = l.RevealedGoalsFor;
            int remaining = needed - scored;
            string live;
            RevealedLegOutcome outcome;
            if (remaining <= 0)
            {
                outcome = RevealedLegOutcome.Won;
                live = $"{scored} GOALS {Bullet} WON";
            }
            else
            {
                outcome = RevealedLegOutcome.Undecided;
                live = $"{scored} GOALS {Bullet} NEED {remaining}";
            }
            return new ActiveLegCopy($"{surname} TO SCORE {needed}+", live,
                                     isTeamMarket: false, identity: MarketPick,
                                     needFallback: $"{surname} {needed}+", outcome: outcome);
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
