using System;
using SBR.Engine;

namespace SBR.Game
{
    /// <summary>
    /// The TV's flavour renderer. Ported from game-console/EventText.cs (the proven text-era voice):
    /// engine DramaEvents carry no words on purpose (design/04 - the "sport" is a reskin of the event
    /// vocabulary), so the presentation layer owns the language. Lines are keyed by (Type, direction)
    /// where direction is the sign of the win-prob move for the picked side; the first beat of a leg
    /// compares against the leg's pre-event TrueProb anchor. Variant chosen by Step (deterministic, no
    /// RNG). Tone: deadpan broadcast with a dark-comedy undertow (design/00).
    ///
    /// Also owns the fake broadcast clock, derived from Step/TotalSteps exactly like the console did.
    /// </summary>
    public static class SweatFlavor
    {
        /// <summary><paramref name="up"/> is the beat's DIRECTION, computed once by
        /// <see cref="SweatPresentationModel.RecordBeat"/> and handed here.
        ///
        /// <para><b>This used to take <c>prevProb</c> and derive its own <c>up</c> from
        /// <c>e.WinProbAfter</c> — a SECOND implementation of a rule the model's own summary already
        /// claimed as "the shared rule, one authority".</b> Two copies of a direction rule can
        /// disagree, and after `T164` they would have: the model's is the TICKET's move and a local
        /// recomputation off <c>WinProbAfter</c> is the ANCHOR LEG's. The parameter is the fix — the
        /// caller passes what the authority decided.</para></summary>
        public static string For(DramaEvent e, Leg leg, bool up)
        {
            if (e.Type == DramaEventType.LegFinal) return "FINAL WHISTLE";

            bool pickedHome = PickedHomeForPresentation(leg);
            string picked = Short(pickedHome ? leg.Matchup.Home.Name : leg.Matchup.Away.Name);
            string other = Short(pickedHome ? leg.Matchup.Away.Name : leg.Matchup.Home.Name);

            // Count lines are keyed by the SELECTION's sense of an increment (Over hopes,
            // Under dreads), never the beat's prob direction (Sol, F_0.4.0 P3 r2). These are
            // the default; the orchestrator overrides with NeutralLine when the resolved
            // scene turns out to carry no count event.
            if (leg.Selection.Kind == MarketKind.TotalCorners)
                return CornerLine(leg.Selection.Choice == MarketChoice.Over, leg, e.Step);
            if (leg.Selection.Kind == MarketKind.TotalCards)
                return BookingLine(leg.Selection.Choice == MarketChoice.Over, leg, e.Step);

            // Tag overrides win over the base table.
            if (e.Tag == TensionTag.NearMiss)
                // T39: "a miracle brewing?!" was the hype half of this pair; the other half was
                // already the correct voice. Both are observed now.
                // T44: the ellipsis is the character, not three periods — the sibling line in the
                // console twin already used it, so this file was the odd one out.
                return up ? "off the bar and away." : "…cleared off the line. it's slipping away.";

            // T98 (batch 70) — `— LEAD CHANGE` WAS APPENDED HERE ON TensionTag.LeadChange, AND THE
            // WORD IS BANNED. The tag is real and this was NOT T97's law a third time:
            // DramaGenerator assigns it on the WIN PROBABILITY crossing 0.5, never on the scoreline,
            // so nothing phantom happened and T97's guard would have suppressed a REAL fact — the
            // wrong remedy reached through the wrong diagnosis. It comes off because §8 stands: the
            // theatre prints facts and offers, never opinions. A price is an offer the player
            // transacts against; a probability is the house's opinion, and a line announcing that it
            // crossed 50% is the deleted win-prob numeral's MEANING without its digits. The fact is
            // not lost — the cash-out price prices off WinProbAfter, so the crossing is already
            // visible AS AN OFFER, the price moving through its own midpoint.
            //
            // A TAG MAY DRIVE TIMING AND STAGING WITHOUT EARNING A WORD. The tag itself is untouched
            // and correct at TheaterChoreographer's #9 turnover intro and TvSweatScreen's
            // leadChangeMs; both are pinned by TheaterChoreographerTests'
            // Overlays_modify_playback_but_never_choose_the_template. If a SCORELINE lead change is
            // ever ruled to earn a word it takes its own tag and its own authored word, never this.
            //
            // ONE FIX, TWO DEFECTS: the suffix appended UPPERCASE to a sentence-case line, against
            // §8's one casing, one dash. T44/TV-32's em-dash convention is unaffected — it is marked
            // at the authored lines that carry a dash of their own.
            return Base(e.Type, up, picked, other, e.Step);
        }

        /// <summary>The broadcast clock, soccer-shaped (F_0.2.0 M-T3): a 90-minute match clock
        /// counting UP, derived from the event's position in its leg. The final whistle reads FT.</summary>
        public static string Clock(DramaEvent e)
        {
            if (e.Type == DramaEventType.LegFinal) return "FT";
            return $"{Minute(e)}'";
        }

        /// <summary>The beat's baked minute (position-derived, never outcome-derived) — the
        /// target the theater's continuously ticking clock runs toward. Caps at 89: the 90th
        /// minute belongs to the final sequence's stoppage time.</summary>
        public static int Minute(DramaEvent e)
        {
            double f = e.TotalSteps <= 1 ? 0.99 : Math.Min(0.99, (double)e.Step / e.TotalSteps);
            return Math.Min(89, Math.Max(1, (int)Math.Round(f * 90)));
        }

        private static string Base(DramaEventType type, bool up, string picked, string other, int step)
        {
            string[] variants = Table(type, up);
            return variants[step % variants.Length]
                .Replace("{picked}", picked)
                .Replace("{other}", other);
        }

        private static string[] Table(DramaEventType type, bool up) => (type, up) switch
        {
            (DramaEventType.Score, true) => ScoreUp,
            (DramaEventType.Score, false) => ScoreDown,
            (DramaEventType.BigPlay, true) => BigUp,
            (DramaEventType.BigPlay, false) => BigDown,
            (DramaEventType.Momentum, true) => MomUp,
            _ => MomDown,
        };

        // ---------------------------------------------------------------- T163's NEITHER branch

        /// <summary>The *neither*-branch line for a beat — the branch where the prose has NO anchor
        /// club, so no line may name one.
        ///
        /// <para><b>`T163` (batch 167) rules three branches.</b> Where every live leg on the fixture
        /// that names a side names the SAME side, that side is <c>picked</c> and the ordinary tables
        /// apply — that subsumes today's single-leg case exactly. Where live legs name OPPOSITE
        /// sides, and where NO live leg names a side at all (totals, BTTS, correct score, odd/even,
        /// margin), the honest answer is <b>NEITHER</b> and these lines are what it says.</para>
        ///
        /// <para><b>Why they name no club, rather than naming both as themselves.</b> `T163` reasoned
        /// that *neither* would leave the two clubs named AS THEMSELVES; batch 171 AMENDED that,
        /// because filling a slot from the EVENT needs the event's actor and <c>DramaEvent</c> has
        /// none — no scorer, no possession side. Measured on both surfaces. So the slot change is
        /// unbuildable without an engine change, and the club-free set (spec §3/§5) is what
        /// ships.</para>
        ///
        /// <para><b>Casing follows §5.1's CORRECTED rule</b> — a club-free line takes the casing its
        /// own FILE uses for club-free copy, NOT the casing of the table it joins. A table whose
        /// other lines open with an interpolated club noun has no casing of its own to match, which
        /// is how one branch elsewhere ended up split two capitalised and two lowercase. This file's
        /// club-free convention is lowercase with a terminal period —
        /// <c>"off the bar and away."</c>, <c>"whipped into the corner — the count moves again."</c>
        /// — so these take that, verbatim from the spec.</para>
        ///
        /// <para><b>THREE variants each, deliberately.</b> <see cref="Base"/> indexes
        /// <c>variants[step % variants.Length]</c>, so a single-element table makes every beat in the
        /// branch read identically — the defect §5 exists to close.</para>
        ///
        /// <para>⚠ <b>DO NOT ROUTE THIS THROUGH <see cref="NeutralLine"/>.</b> `T163` names that trap
        /// by name: <c>NeutralLine</c> is neutral about the COUNT FAMILY, not about the anchor — it
        /// computes <c>pickedHome</c>, <c>picked</c> and <c>other</c> exactly like every other line.
        /// Wiring *neither* to it would ship a HOME anchor under a neutral name, silently, on
        /// precisely the kinds <c>K17</c> already flags.</para></summary>
        public static string NeitherLine(DramaEventType type, bool up, int step)
        {
            string[] variants = NeitherTable(type, up);
            return variants[((step % variants.Length) + variants.Length) % variants.Length];
        }

        private static string[] NeitherTable(DramaEventType type, bool up) => (type, up) switch
        {
            // Score and BigPlay are both GOAL families — §5 authors one goal set per direction, and
            // splitting it here would invent two sets the spec does not have.
            (DramaEventType.Score, true) => NeitherGoalUp,
            (DramaEventType.BigPlay, true) => NeitherGoalUp,
            (DramaEventType.Score, false) => NeitherGoalDown,
            (DramaEventType.BigPlay, false) => NeitherGoalDown,
            (DramaEventType.Momentum, true) => NeitherMomUp,
            _ => NeitherMomDown,
        };

        private static readonly string[] NeitherGoalUp =
        {
            "a goal — the number ticks with it.",
            "a goal in the churn; the number moves.",
            "one goes in — the slip gains.",
        };

        private static readonly string[] NeitherGoalDown =
        {
            // "a goal against the slip" states that the goal works against the ticket WITHOUT
            // naming a side it works FOR, which is exactly what this branch needs. `the slip` is
            // this surface's own established word — ScoreDown already ships "the slip flinches".
            "a goal against the slip.",
            "a goal; the slip flinches.",
            "one goes in, the wrong way for the slip.",
        };

        private static readonly string[] NeitherMomUp =
        {
            "the half tightens.",
            "territory, and the clock with it.",
            "the pitch shrinks.",
        };

        private static readonly string[] NeitherMomDown =
        {
            // `territory` and `sideways` are paired on purpose, so the two momentum directions read
            // as one axis rather than as two moods.
            "the ball stays in midfield.",
            "slow through the middle, and no one in a hurry.",
            "sideways, and the clock with it.",
        };

        private static readonly string[] ScoreUp =
        {
            "{picked} slot it home.",
            "{picked} score — far post says yes.", // T44: em dash
            "Goal for {picked} — the number ticks with it.",
        };

        private static readonly string[] ScoreDown =
        {
            "{other} answer right back.",
            // T44: "Ugly." is the strip editorialising. The event strip observes; the register is
            // "incisive, nocturnal, dry, orderly", and a verdict on the goal is none of those.
            "{other} poke one in at the near post.",
            "{other} on the board; the slip flinches.",
        };

        private static readonly string[] BigUp =
        {
            // T39: no second person, no hype. This surface reports the match; it does not address
            // the player and it does not celebrate. "IT'S IN!" is a commentator's shout and the
            // crowd's reaction is not a match fact — the correct voice was already one line away
            // ("...cleared off the line. it's slipping away"): flat, third person, observed.
            "{picked} tear away and finish.",
            "{picked} break the line and score.",
            // T44: "This is happening." predicts the outcome — CF's "Never imply a guaranteed win",
            // and the same shape as the "a miracle brewing?!" T39 removed from the pair above.
            "{picked} counter at full sprint.",
        };

        private static readonly string[] BigDown =
        {
            // T44: "Disaster" is the superlative the ruling names, and it is the strip taking the
            // player's side — the same fault as the gold wash, in words.
            "{other} go the length of the pitch.",
            "{other} rip through on the break.", // T39: "Cover your eyes" instructs the viewer
            "{other} walk it in.", // T44: "That one hurt." editorialises; the fact is the whole line
        };

        private static readonly string[] MomUp =
        {
            "{picked} squeezing the half.",
            "{picked} pin them deep — passes and patience.", // T44: em dash
            "{picked} tighten the screws.",
        };

        private static readonly string[] MomDown =
        {
            "{other} keeping the ball.",
            "{other} pass it around, slow and mean.",
            "{other} settle in; the drift runs the other way.",
        };

        /// <summary>The goal call for a reconciliation-upgraded beat (playtest #14 — the board
        /// catches the bar): the Score tables by the GOAL's beneficiary, so a possession beat
        /// that scores never reads "passes and patience" while the net ripples (Sol, M-T4.1).</summary>
        public static string GoalLine(bool forPicked, Leg leg, int step)
        {
            bool pickedHome = PickedHomeForPresentation(leg);
            string picked = Short(pickedHome ? leg.Matchup.Home.Name : leg.Matchup.Away.Name);
            string other = Short(pickedHome ? leg.Matchup.Away.Name : leg.Matchup.Home.Name);
            return Base(DramaEventType.Score, forPicked, picked, other, step);
        }

        /// <summary>Ordinary-play line for a count-market beat whose resolved scene carries no
        /// count event (a zero batch fell through) — corner/booking words would be a lie there
        /// (Sol, F_0.4.0 P3 r2). Plain possession language, direction from the beat.</summary>
        /// <summary>T97's sweep, recorded AS DATA rather than as prose in a commit message — which
        /// member of each big-play family ASSERTS A GOAL, and which asserts only a dangerous move.
        ///
        /// <para>The DD asked for the four goal-asserting arrays swept string by string. `ScoreUp`
        /// and `ScoreDown` are goal-asserting in every member, so they have no table here: with no
        /// goal in the resolved scene there is nothing in them that may be spoken. `BigUp` and
        /// `BigDown` are MIXED, and the ruling's scope is "the parts of BigUp/BigDown that finish" —
        /// so the parts that do not finish stay reachable, because they remain true of a dangerous
        /// move that produced no goal.</para>
        ///
        /// <para>Kept as a parallel table rather than by reordering the arrays: the line for a step
        /// is chosen positionally, so reordering would silently change which sentence an existing
        /// seed prints. This encodes the audit without moving anything.</para></summary>
        private static readonly bool[] BigUpAssertsGoal = { true, true, false };
        private static readonly bool[] BigDownAssertsGoal = { false, false, true };

        /// <summary>T97: the line for a beat whose RESOLVED SCENE CARRIES NO GOAL.
        ///
        /// <para>A big play that did not finish is still a big play, so it keeps its own authored
        /// voice — the members that assert only a dangerous move. Everything else falls to the
        /// neutral possession line, which is the remedy the ruling names and the one
        /// <see cref="NeutralLine"/> has always provided for the count families.</para></summary>
        public static string NoGoalLine(DramaEvent e, Leg leg, bool up)
        {
            string[] family = e.Type == DramaEventType.BigPlay ? (up ? BigUp : BigDown) : null;
            bool[] assertsGoal = e.Type == DramaEventType.BigPlay
                ? (up ? BigUpAssertsGoal : BigDownAssertsGoal) : null;
            if (family == null) return NeutralLine(e, leg, up);

            // Walk from the step's own position so the choice stays deterministic and still varies
            // by beat, landing on the first member that does not claim a goal.
            for (int i = 0; i < family.Length; i++)
            {
                int idx = (e.Step + i) % family.Length;
                if (assertsGoal[idx]) continue;
                bool pickedHome = PickedHomeForPresentation(leg);
                return family[idx]
                    .Replace("{picked}", Short(pickedHome ? leg.Matchup.Home.Name : leg.Matchup.Away.Name))
                    .Replace("{other}", Short(pickedHome ? leg.Matchup.Away.Name : leg.Matchup.Home.Name));
            }
            return NeutralLine(e, leg, up);
        }

        public static string NeutralLine(DramaEvent e, Leg leg, bool up)
        {
            bool pickedHome = PickedHomeForPresentation(leg);
            string picked = Short(pickedHome ? leg.Matchup.Home.Name : leg.Matchup.Away.Name);
            string other = Short(pickedHome ? leg.Matchup.Away.Name : leg.Matchup.Home.Name);
            return Base(DramaEventType.Momentum, up, picked, other, e.Step);
        }

        public static string CornerLine(bool forPicked, Leg leg, int step)
        {
            string[] lines = forPicked ? CornerFor : CornerAgainst;
            return lines[step % lines.Length];
        }

        public static string BookingLine(bool forPicked, Leg leg, int step)
        {
            string[] lines = forPicked ? BookingFor : BookingAgainst;
            return lines[step % lines.Length];
        }

        private static readonly string[] CornerFor =
        {
            "whipped into the corner — the count moves again.",
            "corner kick won. another little number for the ledger.",
            "the flag goes up; pressure becomes a corner.",
        };

        private static readonly string[] CornerAgainst =
        {
            "corner conceded. the number leans the wrong way.",
            "they win the flag — an under bettor hears the groan.",
            "deflected wide. corner to them, naturally.",
        };

        private static readonly string[] BookingFor =
        {
            "yellow card in the spell — the picked number improves.",
            "the referee reaches for the card. discipline pays.",
            "late tackle, clear booking. the cards count ticks.",
        };

        private static readonly string[] BookingAgainst =
        {
            "yellow card against the pick. the count bites.",
            "whistle, card, paperwork.",
            "another booking. the number turns sour.",
        };

        /// <summary>spec-count-theater-2026-08-17.md §3.5 / strings-owed-2026-08-17.md §4 — THE
        /// DISJOINT DECISIVE-BEAT POOL. The measured defect: of seven count events, the approach
        /// (43') printed corner #1's ORDINARY line verbatim, and the crossing (53') — the moment
        /// the bet was won — printed corner #2's. RULED: the approach and the turn draw from a
        /// pool <see cref="CornerFor"/>/<see cref="CornerAgainst"/>/<see cref="BookingFor"/>/
        /// <see cref="BookingAgainst"/> cannot reach — DISJOINT, not merely distinct, so
        /// recycling onto a decisive beat is unconstructible rather than unlikely (`T108`
        /// clause 1's standard). That is why these four lines live in their OWN arrays, never
        /// appended to or read from the four above: the separation IS the property, visible in
        /// the code rather than asserted in a comment. SweatActiveLegModelTests asserts the two
        /// pools' string sets do not intersect.
        ///
        /// <para>Four cells, not two: <c>Approach</c>/<c>Turn</c> (<see cref="CountSignificance"/>,
        /// the classification <c>SweatActiveLegModel.Classify</c> already computes) each split by
        /// the leg's own valence — the same <c>countHelps</c> Over/Under mood (never the beat's
        /// team or probability direction) that already chooses <see cref="CornerFor"/> vs
        /// <see cref="CornerAgainst"/>. Not new machinery, per strings-owed-2026-08-17.md §4.2.
        /// </para>
        ///
        /// <para>ONE line per cell, not a step-indexed deck like the arrays above (§4.3): "a
        /// decisive beat fires at most once per leg", so there is nothing to vary a selection
        /// against. Kept as single-element arrays rather than plain constants to match this
        /// file's deck convention (<c>private static readonly string[]</c>) and so a
        /// disjointness sweep can walk every pool the same way.</para>
        ///
        /// <para><b>Both APPROACH cells share one short form</b> (<c>"one short."</c>) —
        /// authored, not an oversight: "at the fallback rung the fact is what survives, and the
        /// valence is already carried by the scene's own register" (§4.2). TURN's two short
        /// forms differ.</para>
        ///
        /// <para><b>UNDER is authored but UNREACHABLE today.</b> spec-count-theater-2026-08-17.md
        /// §6 scopes the distance gate to OVER only ("the Under case is the mirror distance
        /// profile, not in evidence") — <c>TheaterChoreographer.ResolveBeat</c>'s own
        /// `gateEligible` requires `countHelps` (Over), so an Under leg is never classified
        /// <c>Approach</c>/<c>Turn</c> in this build and <see cref="DecisiveLine"/> is never
        /// called with <c>over: false</c> in production. Authored anyway because
        /// strings-owed-2026-08-17.md §4.2 authored all four cells against the under mirror this
        /// spec explicitly defers, not against what today's build can reach.</para></summary>
        private static readonly string[] ApproachOver =
        {
            "one short. the ledger is holding its breath.",
        };

        private static readonly string[] ApproachUnder =
        {
            "one short, and the ledger would rather it stopped here.",
        };

        private static readonly string[] TurnOver =
        {
            "that clears it. the line is beaten.",
        };

        private static readonly string[] TurnUnder =
        {
            "the line goes. the ledger closes this one.",
        };

        private const string ApproachShort = "one short.";
        private const string TurnOverShort = "the line is beaten.";
        private const string TurnUnderShort = "the line goes.";

        /// <summary>The decisive-beat line (pool doc above) for <paramref name="significance"/>
        /// (<c>Approach</c> or <c>Turn</c> ONLY — anything else throws, DEFAULT LOUD: an
        /// <c>Ordinary</c>/<c>Decided</c> beat must draw from <see cref="CornerLine"/>/
        /// <see cref="BookingLine"/> instead, and a caller that cannot yet tell the two apart
        /// must not call this at all), split by <paramref name="over"/> — the leg's
        /// <c>countHelps</c> valence, true for Over.
        ///
        /// <para>NO CALL SITE EXISTS IN PRODUCTION YET. <see cref="CountSignificance"/> is
        /// computed inside <c>TheaterChoreographer.ResolveBeat</c> as a local variable and is
        /// never threaded onto the <c>SceneSpec</c> it returns (checked directly: SceneSpec's
        /// fields are Template/Variant/LeadChangeIntro/Urgent/ForPicked/Goal/Count/CountFinal/
        /// Market/Duration/CountBeneficiaryIsHome/QuietCount/QuietGoal — none carries
        /// significance), so <c>TvSweatScreen</c> has no way to learn it at the point the
        /// flavour line is chosen — <c>countScene</c> there is only
        /// <c>spec.Count.HasValue &amp;&amp; spec.Count.Value.TotalDelta &gt; 0</c>, true for
        /// both a gated decisive beat AND an ungated one (cards, Under, a Score/BigPlay-typed or
        /// NearMiss-tagged corner beat all reach the same CornerFor/CornerAgainst/Booking
        /// template unconditionally — see ResolveBeat's own `gateEligible`/`computable` gates).
        /// Recomputing <c>Classify</c> at that call site would mean re-deriving those gates too
        /// — a second classifier, not a reuse of the one that exists — so this selector is
        /// authored WITHOUT wiring it in, ahead of a future field threading the significance
        /// through SceneSpec. Same shape as <c>SceneSpec.QuietCount</c> having been authored
        /// ahead of its own gate: the pool exists so the wiring lands on top of it rather than
        /// inventing one under time pressure.</para></summary>
        public static string DecisiveLine(CountSignificance significance, bool over) =>
            (significance, over) switch
            {
                (CountSignificance.Approach, true) => ApproachOver[0],
                (CountSignificance.Approach, false) => ApproachUnder[0],
                (CountSignificance.Turn, true) => TurnOver[0],
                (CountSignificance.Turn, false) => TurnUnder[0],
                _ => throw new ArgumentOutOfRangeException(nameof(significance), significance,
                    "SweatFlavor's decisive pool covers only Approach/Turn — Ordinary/Decided " +
                    "must never reach it; that is the disjointness spec §3.5 rules, not a range " +
                    "to widen."),
            };

        /// <summary>The decisive-beat line's fallback rung (`T110-am`/`C46`) — same parameters as
        /// <see cref="DecisiveLine"/>. Approach's two cells collapse to one shared short form;
        /// Turn's do not (see the pool's own doc above).</summary>
        public static string DecisiveShortLine(CountSignificance significance, bool over) =>
            significance switch
            {
                CountSignificance.Approach => ApproachShort,
                CountSignificance.Turn => over ? TurnOverShort : TurnUnderShort,
                _ => throw new ArgumentOutOfRangeException(nameof(significance), significance,
                    "SweatFlavor's decisive pool covers only Approach/Turn — Ordinary/Decided " +
                    "must never reach it; that is the disjointness spec §3.5 rules, not a range " +
                    "to widen."),
            };

        /// <summary>Market legs (O/U, BTTS) have no picked TEAM — presentation anchors them on the
        /// home side; the market label carries the pick. Shared by every renderer so the anchor
        /// can never disagree across surfaces. Moneyline legs answer with their real side.
        ///
        /// <para><b>A MONEYLINE DRAW HAS NO SIDE, and this returned AWAY for it.</b> Routed here by
        /// name from the markets lane's class sweep (`a3d184c`: *"SweatFlavor:206 — draw counts as
        /// away for flavour, ROUTED → tv-sweat"*), and it survived that sweep for the reason worth
        /// recording: **it is in this surface's file, not theirs.** A cross-lane sweep scoped by
        /// ownership misses exactly the code another lane owns.</para>
        ///
        /// <para><b>The defect was a fall-through, not a decision.</b> The old expression asked
        /// `Choice == MarketChoice.Home` and let everything else be false — written when `Choice` on a
        /// moneyline could only be Home or Away, so "not Home" meant Away. `MarketChoice.Draw` made
        /// that inference wrong without touching the line: the third value silently inherited the
        /// second one's branch. C46's shape in an expression rather than in a box.</para>
        ///
        /// <para><b>The fix is the rule this summary already states, applied to a case written before
        /// draws existed:</b> a draw has no picked team, exactly like O/U and BTTS, so it takes the
        /// same HOME anchor they do and the market label carries the pick. It is not new design — the
        /// only new thing is that a third no-team case now exists.</para>
        ///
        /// <para><b>Deliberately NOT the null the markets lane used</b> (`BetslipModel.SideOn` returns
        /// null for a draw and is pinned for it). That answers a different question: `SideOn` reports
        /// WHICH SIDE YOU BACKED, where a draw's honest answer is "neither". This answers WHICH TEAM
        /// THE PROSE ANCHORS ON, where every leg needs an answer and "neither" would leave the
        /// flavour with no names. Same finding, two functions, two correct shapes.</para>
        ///
        /// <para><b>Routed, not authored:</b> whether the flavour's VOICE reads correctly on a
        /// draw-backed leg — `{picked}`/`{other}` naming the home side while the pick is the draw — is
        /// a copy question for the DD. The direction is unaffected either way: up/down comes from the
        /// leg's own win-prob move, not from the name anchor.</para></summary>
        public static bool PickedHomeForPresentation(Leg leg)
            => leg.Selection.Kind == MarketKind.AnytimeScorer
                ? leg.Matchup.PlayerSide(leg.Selection.PlayerIndex) == Side.Home
                : leg.Selection.Kind != MarketKind.Moneyline
                    || leg.Selection.Choice == MarketChoice.Home
                    || leg.Selection.Choice == MarketChoice.Draw;

        /// <summary>The team's noun (last word of the "City Noun" name) - punchier for the ticker.</summary>
        public static string Short(string teamName)
        {
            int i = teamName.LastIndexOf(' ');
            return i >= 0 ? teamName.Substring(i + 1) : teamName;
        }
    }
}
