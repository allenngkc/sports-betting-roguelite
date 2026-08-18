using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SBR.EditorTools
{
    /// <summary>
    /// T84's sweep: every slot whose extent, or whose neighbour's position, was derived against the
    /// pre-migration face — inventory naming its members, per C18 §4.1.
    ///
    /// <para><b>What the exposure actually is.</b> This surface computes no geometry from content:
    /// §6 forbids it and the row builder says so at its own call site — <i>"Widths are fixed, never
    /// derived from content (§6): the chip reserves canon's 38px"</i>. So nothing here has to be
    /// re-derived from a measurement. What every fixed box DOES carry is an assumption that its
    /// content fits, and every one of those was sized against a face 20% narrower than the one the
    /// surface now renders. The boxes did not move; the strings grew inside them.</para>
    ///
    /// <para>Two members were found by eye, in two of nine moments, and the DD recorded that nobody
    /// had looked at the other seven. This looks at all of them at once, by measurement rather than
    /// by frame inspection — a frame only shows the string that seed happened to produce, while a
    /// reserved box either holds the longest form it can be asked to render or it does not.</para>
    ///
    /// <para><b>Blind spot, stated rather than implied:</b> a slot is only as swept as its string set
    /// is complete. Where the longest renderable form is authored in a deck this reads that deck.
    /// Where content is generated — team names, money, clocks — it uses the widest realistic form and
    /// says so. Any slot whose content this could not enumerate is reported UNSWEPT rather than
    /// silently passed.</para>
    /// </summary>
    public static class TvExtentSweep
    {
        /// <summary>Measure width with NO wrapping constraint.
        ///
        /// <para>The first run of this sweep passed 0 and reported the compact statement slot at
        /// 12.5px — one character. That slot is the only one with wrapping enabled, and a wrapping
        /// component asked for its preferred size at zero width wraps every character, so the answer
        /// is the widest glyph rather than the widest string.</para>
        ///
        /// <para>It matters far beyond this instrument: the runtime's own <c>Fits</c> and
        /// <c>FitToColumn</c> make the same zero-width call, on the same wrapping slot. UGUI's
        /// <c>GetPreferredWidth</c> returned the unwrapped width regardless of wrap mode, so the
        /// behaviour did not survive the port. Reported as a finding of this sweep.</para></summary>
        private const float Unconstrained = 100000f;

        /// <summary>The engine's CLOSED name pools, transcribed from `SlateGenerator`.
        ///
        /// <para><b>The traceability pass's finding, and it is the direction T89-C says is invisible in
        /// frames.</b> Both generated arms of the leg row were swept against a PICKED champion rather
        /// than an enumerated pool — `REGULATORS ML` for the club arm, when `Gravediggers` and
        /// `Spreadsheets` are both in the pool and both two characters longer. The surface's own source
        /// names the case this missed: <i>"Fallback for an unlucky club — `GRAVEDIGGERS TO WIN` is 19
        /// and over budget."</i> The code knew the worst club; the sweep did not use it.</para>
        ///
        /// <para><b>And the two directions were not independent.</b> The old set also carried
        /// `BRICKLAYERS ANYTIME` — a CLUB noun in the SURNAME slot, which
        /// <c>{Surname} ANYTIME</c> cannot emit. That invented string was the widest member of its
        /// set, so it SET the measured worst case and hid the fact that the real widest producible
        /// form had never been measured. `PEND`'s class of error was concealing the opposite class.
        /// An over-generated string is not merely noise: while it is the maximum, it suppresses the
        /// under-generation it sits on top of.</para>
        ///
        /// <para>Picked champions are retired here. The pools are closed and small — 20 nouns and 12
        /// surnames — so every producible form is generated and measured, and no future name added to
        /// either pool can be missed by a champion nobody re-picked.</para></summary>
        private static readonly string[] ClubNouns =
        {
            "Yams", "Startups", "Bricklayers", "Longhaulers", "Mallards", "Spreadsheets",
            "Turnips", "Middlemen", "Regulators", "Plumbers", "Meatballs", "Auditors",
            "Ferrets", "Overheads", "Gravediggers", "Notaries", "Muskrats", "Zambonis",
            "Loopholes", "Refunds",
        };

        /// <summary>`SlateGenerator.PlayerLast`. The scorer arms take a SURNAME, never a club.</summary>
        private static readonly string[] Surnames =
        {
            "Ledger", "Cinder", "Muffin", "Pavement", "Coupon", "Wobble",
            "Gasket", "Pylon", "Ketchup", "Lanyard", "Racket", "Stapler",
        };

        /// <summary>A LADDER's next rung, or null where the slot has none.
        ///
        /// <para>G1-am7 authored the moneyline NEED arm as two rungs selected BY MEASUREMENT —
        /// <c>{CLUB} TO WIN</c> where it fits, <c>{CLUB} WIN</c> for the five clubs where it does not
        /// — and <c>FitOrFallback</c> is what chooses. So <c>SPREADSHEETS TO WIN</c> at 289.9px is a
        /// form the surface CAN COMPOSE and can never DRAW.</para>
        ///
        /// <para><b>Sweeping it flat would report a false overrun</b>, and it is the same error as
        /// sweeping `BRICKLAYERS ANYTIME` — certifying a box against a string that cannot reach it —
        /// arriving from the opposite direction. The sweep's contract is the longest RENDERABLE form,
        /// and with a measured ladder the renderable form is whichever rung the measurement picks.
        /// Derived here rather than hard-coded as "these five use rung 2", because a hard-coded split
        /// goes stale the moment the box or the face moves, which this lane has already paid for
        /// once.</para>
        /// <para><b>The moneyline arm is not the only ladder, and modelling one exposed the other.</b>
        /// With `{CLUB} TO WIN` laddered, the slot's widest became `ONE TEAM SCORELESS` at 272.4 —
        /// which is ALSO a rung: `ActiveLegCopy` gives it `needFallback: "ONE TEAM BLANKED"`, authored
        /// complete for exactly this. The false overrun simply moved one arm over, which is what a
        /// ladder-blind sweep does to every laddered arm it has.</para>
        ///
        /// <para><b>THREE arms are ladders, and the first two versions of this table said otherwise.</b>
        /// I wrote here that the scorer arm need not be modelled "because no surname form reaches the
        /// box at all" — and the very next run falsified it: `PAVEMENT TO SCORE` is 264.9px against
        /// the 261.0px column. Each time a ladder was modelled the false overrun moved one arm over,
        /// which is what a ladder-blind sweep does to every laddered arm it has. All three are
        /// transcribed from `ActiveLegCopy`'s construction sites now, not inferred.</para>
        ///
        /// <para><b>The scorer arm's bare rung was routed and is now RULED (G1-am8).</b> It was
        /// `TO SCORE`, which names no player — the property G1-am7 retired bare `TO WIN` for, and
        /// worse here, because the backed-side marker renders only on moneyline legs so a scorer leg
        /// has no marker at all. Rung 2 is `{SURNAME} SCORES`. **One rule across both arms: drop the
        /// infinitive marker and conjugate to the subject** — clubs are plural and take `WIN`, a
        /// surname is singular and takes `SCORES`.</para></summary>
        private static string LadderFallback(string slot, string s)
        {
            if (slot != "LegRowNeed0") return null;
            if (s.EndsWith(" TO WIN")) return s.Substring(0, s.Length - 7) + " WIN";       // G1-am7
            if (s == "ONE TEAM SCORELESS") return "ONE TEAM BLANKED";                      // G1, authored
            if (s.EndsWith(" TO SCORE")) return s.Substring(0, s.Length - 9) + " SCORES";  // G1-am8
            if (s == "LEVEL AT FULL TIME") return "LEVEL AT FT";                           // T96, authored
            return null;
        }

        /// <summary>Every member of a closed pool through one authored format, upper-cased the way the
        /// surface upper-cases it. Generating beats picking: the sweep then re-derives its own worst
        /// case whenever a pool grows.</summary>
        private static string[] From(string[] pool, string format)
        {
            var forms = new string[pool.Length];
            for (int i = 0; i < pool.Length; i++) forms[i] = string.Format(format, pool[i].ToUpperInvariant());
            return forms;
        }

        private static string[] And(params string[][] sets)
        {
            var all = new List<string>();
            foreach (string[] s in sets) all.AddRange(s);
            return all.ToArray();
        }

        /// <summary>Every (template × pool member) combination, exactly one <c>{0}</c> placeholder
        /// per template, CASE PRESERVED. SweatFlavor's six placeholder decks (<c>Base</c>'s Score/
        /// BigPlay/Momentum tables, SweatFlavor.cs:84-100) splice in <c>SweatFlavor.Short(...)</c>
        /// verbatim (SweatFlavor.cs:298-302) — the pool's own casing, never upper-cased on the way
        /// out — unlike the leg row's decks, which <see cref="From"/> above serves with an explicit
        /// uppercase pass. A second helper rather than a parameter on <see cref="From"/>: the two
        /// surfaces disagree on casing because the SURFACES do (tracked uppercase for the leg row
        /// vs. the strip's lower-case narration, observed directly in the decks below), not by
        /// omission.</summary>
        private static string[] FromEach(string[] templates, string[] pool)
        {
            var forms = new List<string>();
            foreach (string template in templates)
                foreach (string name in pool)
                    forms.Add(string.Format(template, name));
            return forms.ToArray();
        }

        /// <summary>The <c>{picked}</c> half of <c>SweatFlavor.Base</c>'s table: ScoreUp, BigUp,
        /// MomUp (SweatFlavor.cs:92-99 <c>Table</c>). Every member takes exactly one
        /// <c>{picked}</c> (SweatFlavor.cs:88), replaced with the BACKED side's <c>Short()</c>
        /// name — <c>{0}</c> stands in for it here. Transcribed verbatim — wording, punctuation,
        /// and the em dashes copied rather than retyped — from SweatFlavor.cs:102-145.</summary>
        private static readonly string[] FlavorPickedTemplates =
        {
            // ScoreUp, SweatFlavor.cs:102-107
            "{0} slot it home.",
            "{0} score — far post says yes.",
            "Goal for {0} — the number ticks with it.",
            // BigUp, SweatFlavor.cs:118-129
            "{0} tear away and finish.",
            "{0} break the line and score.",
            "{0} counter at full sprint.",
            // MomUp, SweatFlavor.cs:140-145
            "{0} squeezing the half.",
            "{0} pin them deep — passes and patience.",
            "{0} tighten the screws.",
        };

        /// <summary>The <c>{other}</c> half: ScoreDown, BigDown, MomDown (SweatFlavor.cs:92-99
        /// <c>Table</c>) — same rule, one <c>{other}</c> per member (SweatFlavor.cs:89), replaced
        /// with the OPPONENT side's <c>Short()</c> name. Transcribed verbatim from
        /// SweatFlavor.cs:109-152.</summary>
        private static readonly string[] FlavorOtherTemplates =
        {
            // ScoreDown, SweatFlavor.cs:109-116
            "{0} answer right back.",
            "{0} poke one in at the near post.",
            "{0} on the board; the slip flinches.",
            // BigDown, SweatFlavor.cs:131-138
            "{0} go the length of the pitch.",
            "{0} rip through on the break.",
            "{0} walk it in.",
            // MomDown, SweatFlavor.cs:147-152
            "{0} keeping the ball.",
            "{0} pass it around, slow and mean.",
            "{0} settle in; the drift runs the other way.",
        };

        /// <summary>CornerFor + CornerAgainst, verbatim from SweatFlavor.cs:231-243 — no
        /// placeholder, so no pool substitution is needed. Both templates are reachable on the SAME
        /// leg (Over picks CornerFor, Under picks CornerAgainst — SweatFlavor.cs:219-223), so both
        /// belong in one slot's sweep.</summary>
        private static readonly string[] CornerForAgainst =
        {
            // CornerFor, SweatFlavor.cs:231-236
            "whipped into the corner — the count moves again.",
            "corner kick won. another little number for the ledger.",
            "the flag goes up; pressure becomes a corner.",
            // CornerAgainst, SweatFlavor.cs:238-243
            "corner conceded. the number leans the wrong way.",
            "they win the flag — an under bettor hears the groan.",
            "deflected wide. corner to them, naturally.",
        };

        /// <summary>BookingFor + BookingAgainst, verbatim from SweatFlavor.cs:245-257. Same
        /// reachable-on-the-same-leg reasoning as <see cref="CornerForAgainst"/> (Over picks
        /// BookingFor, Under picks BookingAgainst — SweatFlavor.cs:225-229).</summary>
        private static readonly string[] BookingForAgainst =
        {
            // BookingFor, SweatFlavor.cs:245-250
            "yellow card in the spell — the picked number improves.",
            "the referee reaches for the card. discipline pays.",
            "late tackle, clear booking. the cards count ticks.",
            // BookingAgainst, SweatFlavor.cs:252-257
            "yellow card against the pick. the count bites.",
            "whistle, card, paperwork.",
            "another booking. the number turns sour.",
        };

        /// <summary>The `Flavor` strings that reach this box WITHOUT coming from one of the ten
        /// decks. C46's population is the BOX's renderable set, not any one generator's output, and
        /// these five are reachable on the same slot as everything above.
        ///
        /// <para>Two arrive ahead of the deck table inside <c>SweatFlavor.For</c> — the
        /// <c>LegFinal</c> early return and the <c>NearMiss</c> tag override, which the file's own
        /// comment calls a tag override that "wins over the base table" — and two are assigned to
        /// <c>_tFlavor</c> directly by the screen, never through <c>SweatFlavor</c> at all. A pool
        /// built only from the decks cannot see the last two by construction.</para></summary>
        private static readonly string[] FlavorNonDeckLines =
        {
            "FINAL WHISTLE",                                  // SweatFlavor.cs:20, LegFinal early return
            "off the bar and away.",                          // SweatFlavor.cs:41, NearMiss up
            "…cleared off the line. it's slipping away.",      // SweatFlavor.cs:42, NearMiss down (T44's ellipsis is U+2026, one glyph)
            "THE BOARD IS SET",                               // TvSweatScreen.cs:2402, pregame
            "THE MATCH ENDS LEVEL",                           // TvSweatScreen.cs:3127, drawn full time (batch 68)
        };

        /// <summary>The count-batch suffix's number, at its reachable maximum. TvSweatScreen.cs:
        /// 1694-1695 appends <c>$" ({spec.Count.Value.TotalDelta} in the spell)"</c>, where
        /// TotalDelta is ONE beat's HomeDelta+AwayDelta from <c>CountLedger.Distribute</c>'s even
        /// split (SweatPresentationModel.cs:667-672) of the locked per-side target across the
        /// leg's beat count.
        ///
        /// <para>Per-side target caps at RunConfig.MaxCornerGrid=20 / MaxCardGrid=12
        /// (RunConfig.cs:65-66) — the sampler's hard per-side ceiling (MatchModel.cs:245-248
        /// <c>SampleStatLine</c>'s four <c>SampleFromRaw</c> calls; MatchModel.cs:764-773
        /// <c>SampleFromRaw</c> itself, whose returned index cannot exceed the raw array's own
        /// length).</para>
        ///
        /// <para>Beat count's reachable MINIMUM is 2: DramaConfig.EarlyMinEventsPerLeg=2
        /// (DramaConfig.cs:18), read at round 1 by <c>EventBoundsForRound</c> (DramaConfig.cs:
        /// 33-52) and drawn by <c>DramaGenerator.BuildLegPath</c>'s
        /// <c>NextInt(minEvents, maxEvents+1)</c> (DramaGenerator.cs:63) — this floor applies to
        /// every leg regardless of market, count legs included. TvSweatScreen threads this same
        /// TotalSteps through as CountLedger's beatCount (TvSweatScreen.cs:2415, 2427).</para>
        ///
        /// <para>With beats=2 and a target at its cap, <c>Distribute</c>'s quotient split
        /// (target/beats, remainder spread over the earliest beats) gives 10 corners / 6 cards per
        /// side at EACH of the two beats, and <c>StageBeat</c> serves the SAME index from both the
        /// home and away schedules (SweatPresentationModel.cs:610-617) — so one beat carries both
        /// halves at once: TotalDelta = 10+10 = 20 (corners), 6+6 = 12 (cards).</para></summary>
        private const int MaxCornerBatchDelta = 20;
        private const int MaxCardBatchDelta = 12;

        /// <summary>Appends the count-batch suffix (TvSweatScreen.cs:1695) to every member of a
        /// deck, verbatim (<c>+=</c>, no other transform).</summary>
        private static string[] Suffixed(string[] lines, int totalDelta)
        {
            var forms = new string[lines.Length];
            for (int i = 0; i < lines.Length; i++) forms[i] = $"{lines[i]} ({totalDelta} in the spell)";
            return forms;
        }

        /// <summary>Per slot: the strings it can be asked to render, longest-form first where known.
        /// Sources are named so a reader can check the set rather than trust it.</summary>
        private static readonly (string slot, string source, string[] strings)[] Cases =
        {
            // RE-ENUMERATED by the T89-C pass. The old set picked `LANYARD TO SCORE` out of a
            // 12-surname pool and carried `MIDDLEMEN ML` — an ML form, which is the LINE slot's
            // vocabulary and one NEED never emits (its moneyline form is `{CLUB} TO WIN`). Both
            // generated arms are now generated. The authored constants and the two fallbacks are
            // verbatim from ActiveLegCopy's construction sites.
            // G1-am7 (batch 62): the moneyline arm is a TWO-RUNG ladder, so BOTH rungs are swept —
            // rung 1 `{CLUB} TO WIN` for the 15 that fit, rung 2 `{CLUB} WIN` for the other five.
            // Bare `TO WIN` is RETIRED and deliberately absent: it must not be reachable on a
            // moneyline leg (T94's desync), so sweeping it would certify a string the surface may
            // never render — the over-generation half of the traceability pass, one arm over.
            ("LegRowNeed0", "G1's NEED deck over the closed pools; the moneyline arm is a LADDER (see LadderFallback)",
                And(From(ClubNouns, "{0} TO WIN"), From(Surnames, "{0} TO SCORE"), new[]
                { "ONE TEAM SCORELESS", "ONE TEAM BLANKED", "BOTH TEAMS SCORE", "NOT YET",
                  // T96 (batch 68): the DRAW's own row, from the amended deck. `LEVEL AT FULL TIME`
                  // is 18 chars at the NEED budget and carries `LEVEL AT FT` as its authored shorter
                  // line — the same ladder shape as ONE TEAM SCORELESS / ONE TEAM BLANKED.
                  "LEVEL AT FULL TIME", "LEVEL AT FT" })),
            // `TO SCORE` is GONE from the set with G1-am8: bare is retired on the scorer arm and must
            // not be reachable, so sweeping it would certify a string the surface may never render.
            // CORRECTED by the traceability pass, in BOTH directions. The old set was
            // {UNDER 10.5 CORNERS, UNDER 10.5 CNRS, LANYARD TO SCORE, BOTH TEAMS SCORE, MIDDLEMEN ML}.
            // Two of those — LANYARD TO SCORE and BOTH TEAMS SCORE — are forms LegStatement does NOT
            // produce (it emits `{SURNAME} ANYTIME` and `BTTS YES`/`BTTS NO`), and the set missed the
            // GOALS and CARDS totals entirely. Enumerated from LegStatement's switch, every arm.
            // RE-ENUMERATED by the T89-C pass, and this is the slot whose 1.8px relief rested on the
            // old set. `BRICKLAYERS ANYTIME` was unproducible (a club noun in the surname slot) and
            // was the set's own maximum; `REGULATORS ML` was a picked champion two characters short
            // of the pool's longest. Both arms are generated now.
            ("LegRowLine0", "LegStatement's six arms: ML and ANYTIME generated over the closed pools — see the UNBOUNDED note on its default",
                And(From(ClubNouns, "{0} ML"), From(Surnames, "{0} ANYTIME"), new[]
                { "UNDER 10.5 CORNERS", "UNDER 10.5 GOALS", "UNDER 10.5 CARDS",
                  "BTTS YES", "BTTS NO" })),
            ("LegRowPrice0", "price forms, generated", new[] { "+450", "-110", "2.75", "+1200" }),
            // CORRECTED. The first set here was {LIVE, NEXT, WON, LOST, VOID, PEND} and I invented
            // it — the plausible vocabulary for a state chip, not this build's. Enumerated properly
            // from every SetRowChip call site, the chip renders exactly five things, and PEND is not
            // among them. The 6px overrun this slot reported was on a string it cannot display.
            // My own rule, broken by me on the one slot I did not grep: a slot is only as swept as
            // its string set is complete.
            ("LegRowState0", "every SetRowChip call site: 2215, 2221, 2230, 2254, 2287",
                new[] { "VOID", "NEXT", "W", "L", "" }),
            // REBUILT. Not one member of the old set {"0-0, 62' PLAYED", "NEEDS 1 MORE, 78'",
            // "2-1, 88' PLAYED"} is a string this build can emit — it named a trailing clock stamp
            // on the LIVE line that does not exist anywhere in SweatActiveLegModel, so this
            // family's sweep was vacuous. Enumerated from SweatActiveLegModel.Describe's `Live`
            // field (SweatActiveLegModel.cs), every arm — including the WON/LOST forms this lane's
            // unit 1 added (the NEED-0 fix, SweatActiveLegModel.cs:367-370/471-475) and every arm
            // that survived unchanged.
            //
            // Numeric fields are instantiated at the run's actual reachable bounds, sourced rather
            // than guessed:
            //   - goals: RunConfig.MaxGoalsGrid = 8 caps EACH side (engine/RunConfig.cs:64),
            //     enforced by the sampler's grid (engine/MatchModel.cs:236-250 SampleStatLine,
            //     705-728 EnumerateClass, 753-761 SampleScore never returning outside the enumerated
            //     h,a <= MaxGoalsGrid set) — so a side's revealed goals are 0..8, a match's total
            //     goals 0..16.
            //   - corners/cards: RunConfig.MaxCornerGrid = 20 / MaxCardGrid = 12 cap EACH side
            //     (engine/RunConfig.cs:65-66), enforced the same way via SampleFromRaw
            //     (engine/MatchModel.cs:764-773, whose returned index cannot exceed the raw array's
            //     length) — so a match's total is 0..40 corners, 0..24 cards.
            //   - NEED/MORE (over) and LIMIT (under) are `threshold - total` / `maxAllowed - total`
            //     (SweatActiveLegModel.cs:592-603), and threshold/maxAllowed are read off the
            //     LARGEST configured line — GoalLines max 3.5 -> threshold 4 (engine/RunConfig.cs:
            //     67), CornerLines max 10.5 -> threshold 11 (:68), CardLines max 5.5 -> threshold 6
            //     (:69); maxAllowed is one less in each case. total and the suffix number always
            //     sum to that fixed threshold in this branch, so they trade off digit count — both
            //     edges (total=0, and total=threshold-1) are given for CORNERS, the one noun whose
            //     threshold crosses into two digits (0+11 vs 10+1); goals/cards never reach a
            //     second digit on either side of the split, so one edge stands for both there.
            // NEED 0 is UNCONSTRUCTIBLE and deliberately absent: that was the exact defect this
            // lane's unit 1 fixed (a cleared requirement routes to WON, never to a clamped
            // "NEED 0" — SweatActiveLegModel.cs:367-370 names it for goals, :471-475 for
            // corners/cards, with "10 CORNERS • NEED 0" as the old defect's own example). LIMIT 0 is
            // the opposite case — a real, still-live allowance — and IS included per
            // SweatActiveLegModel.cs:498-502: "LIMIT 0 IS TRUE AND STAYS... Do not 'fix' this to
            // Won or Lost."
            ("LegRowProgress0", "SweatActiveLegModel.Describe's Live field, every arm — numeric bounds sourced in the comment above",
                new[]
                {
                    // Moneyline draw, SweatActiveLegModel.cs:317 (the MoneylineDraw factory's row)
                    "LEVEL", "NOT LEVEL",
                    // Moneyline non-draw, SweatActiveLegModel.cs:323-325; score is
                    // "{RevealedGoalsFor}{Dash}{RevealedGoalsAgainst}" (line 295), Dash = U+2013 EN
                    // DASH (line 206, copied verbatim, never retyped as an ASCII hyphen). Each side
                    // 0..8 (MaxGoalsGrid); an adjacent max pair stresses both digits at once.
                    "LEADING 8–7", "TRAILING 7–8", "LEVEL 8–8",
                    // TotalGoals Over, SweatActiveLegModel.cs:359-389 (dispatch: Describe's switch,
                    // SweatActiveLegModel.cs:217-220). Bullet = U+2022 BULLET (line 208, copied
                    // verbatim). Total 0..16 (2 x MaxGoalsGrid); threshold 4 at GoalLines' max 3.5.
                    "16 GOALS • WON",     // cleared, line 375; total at its match-wide max
                    "0 GOALS • 4 MORE",   // undecided, line 380; total=0 edge
                    "3 GOALS • 1 MORE",   // undecided, line 380; total=threshold-1 edge
                    // THE SINGULAR TOTAL, routed here by T108-am2 and MISSING from this pool's
                    // first cut. Every count arm formats `{total} {NOUN}` with the noun as a fixed
                    // literal and no arity branch anywhere in the file, so a revealed total of ONE
                    // renders `1 GOALS`. Goals certainly reach it — the docked
                    // goals-control-2026-08-16 set holds a revealed `1 — 0` from 30' to 90'+1 — and
                    // the corners arm may not, since that seed's batch deltas were 2 throughout and
                    // a step of 2 can jump the line without landing on it.
                    //
                    // Enumerated, NOT ruled: this pool's job is to make every renderable form
                    // measurable, and whether `1 GOALS` is acceptable copy is a grammar question the
                    // DD holds. It is here because a form the code can emit and the pool cannot see
                    // is the C46 failure this instrument exists to catch — and the first cut of this
                    // very pool had it, which is the point worth keeping.
                    "1 GOALS • WON",      // over 0.5, total 1: cleared at the smallest total that can
                    "1 GOALS • 3 MORE",   // over 3.5, total 1
                    "1 GOALS • LIMIT 2",  // under 3.5, total 1
                    // TotalGoals Under, SweatActiveLegModel.cs:391-419.
                    "16 GOALS • LOST",    // line 403; total at its match-wide max
                    "0 GOALS • LIMIT 3",  // undecided, line 410; total=0 edge
                    "3 GOALS • LIMIT 0",  // undecided, line 410; LIMIT 0 stays live (see comment above)
                    // Whole-number-line fallthrough, SweatActiveLegModel.cs:386 (Over) / :416
                    // (Under) — both produce this identical literal. RunConfig.GoalLines currently
                    // generates ONLY half-integers (engine/RunConfig.cs:67), so this specific arm is
                    // UNREACHABLE with today's config — included anyway per the brief; the model
                    // itself names the same defensive-only caveat at SweatActiveLegModel.cs:576-583.
                    "16 GOALS",
                    // TotalCorners, SweatActiveLegModel.cs:225-226 dispatch -> DescribeCount(noun:
                    // "CORNERS"). Total 0..40 (2 x MaxCornerGrid=20); threshold 11 / maxAllowed 10
                    // at CornerLines' max 10.5 — the one noun whose threshold clears 9, so both
                    // edges of NEED/LIMIT are kept (see the digit-count-split note above).
                    "40 CORNERS • WON",       // line 480
                    "0 CORNERS • NEED 11",    // undecided, line 485; total=0 edge
                    "1 CORNERS • NEED 10",    // the singular total — see the T108-am2 note above.
                                              // Kept even though this seed's batch deltas were 2
                                              // throughout: reachability is a property of the
                                              // generator, not of one capture, and a pool sized to
                                              // one seed's deltas is S84's failure in miniature.
                    "10 CORNERS • NEED 1",    // undecided, line 485; total=threshold-1 edge
                    "40 CORNERS • LOST",      // line 494
                    "0 CORNERS • LIMIT 10",   // undecided, line 504; total=0 edge
                    "10 CORNERS • LIMIT 0",   // undecided, line 504; LIMIT 0 stays live
                    "40 CORNERS",             // line 513; same unreachable-today caveat as GOALS
                    // TotalCards, SweatActiveLegModel.cs:227-228 dispatch -> DescribeCount(noun:
                    // "CARDS"). Total 0..24 (2 x MaxCardGrid=12); threshold 6 / maxAllowed 5 at
                    // CardLines' max 5.5.
                    "24 CARDS • WON",        // line 480
                    "0 CARDS • NEED 6",      // undecided, line 485; total=0 edge
                    "1 CARDS • NEED 5",      // the singular total — see the T108-am2 note above
                    "5 CARDS • NEED 1",      // undecided, line 485; total=threshold-1 edge
                    "24 CARDS • LOST",       // line 494
                    "0 CARDS • LIMIT 5",     // undecided, line 504; total=0 edge
                    "5 CARDS • LIMIT 0",     // undecided, line 504; LIMIT 0 stays live
                    "24 CARDS",              // line 513; same unreachable-today caveat
                    // BTTS Yes, SweatActiveLegModel.cs:423-434. scored is 0, 1, or 2 — every
                    // reachable value; all single-digit, so there is no stress choice to make.
                    "0/2 TEAMS SCORED", "1/2 TEAMS SCORED", "2/2 TEAMS SCORED",
                    // BTTS No, SweatActiveLegModel.cs:436-450. Two literals, no numeric field.
                    "BOTH HAVE SCORED", "CLEAN-SHEET PATH LIVE",
                    // Anytime scorer, SweatActiveLegModel.cs:521-563 (ScorerRevealed gate). Two
                    // literals, no numeric field.
                    "SCORED", "NOT YET",
                }),
            ("CashOut", "§6.1 money control, six states", new[]
                { "MARKET SUSPENDED", "CASHED OUT $1,240", "CASH OUT $1,240", "CASH OUT $183" }),
            // T88's gesture gave the status word a THIRD state: under a held preview the only act left
            // is the commit, so the word says so. Added with the wiring rather than after it.
            ("CashOutStatus", "§6.1 status words — all three states of CashOutStatusWord()",
                new[] { "UPDATING", "HOLD E", "ENTER TO CASH OUT" }),
            // T74-am5 (batch 59): the footer is ONE ROW with BOTH ENDS ANCHORED, so the five-space
            // spacer that was being measured no longer exists and each half is swept on its own.
            //
            // "UNBOUNDED" is retired here, and it was the wrong word: `PotentialPayout` is
            // parlay-multiplied but MaxLegs is finite, so it was UN-ENUMERATED. Enumerated in
            // `engine.tests/PayoutMaximumTests` over 648,000 priced offers — max single-leg odds
            // 52.0359, so the parlay term is 52.0359^4 = 7,331,837.65 x stake. The stake ceiling is
            // the BANK (MaxStakeFraction 1.0), which is run state, so the longest form is stated per
            // bank assumption: $73,318,376,502 at a bank of $10,000 is eleven digits, and eleven
            // digits is what the row holds.
            // AMENDED. The footer's first word is no longer hard-coded "RISK" — it is
            // SweatActiveLegModel.StakeWord(legOutcomes) (SweatActiveLegModel.cs:279-280), rendered
            // at TvSweatScreen.cs:2781 as `$"{StakeWord(...)} ${Money(_ticket.Stake)}"`. STAKE
            // prints once every remaining leg is Won or Voided (TicketCannotLose,
            // SweatActiveLegModel.cs:258-268); RISK otherwise. The figure's own bound is unchanged
            // (see Pays below); only the leading word is now two-valued, so every existing amount
            // gets a matching STAKE form alongside the RISK form it already had.
            ("RiskPays", "the footer's LEFT half; stake-bounded; first word is StakeWord() (RISK/STAKE) — SweatActiveLegModel.cs:279-280, TvSweatScreen.cs:2781", new[]
                { "RISK $13,639", "RISK $1,234", "RISK $50",
                  "STAKE $13,639", "STAKE $1,234", "STAKE $50" }),
            ("Pays", "the footer's RIGHT half; parlay term 7,331,837.65 x stake — see PayoutMaximumTests", new[]
                { "PAYS $73,318,376,502", "PAYS $7,331,837,650", "PAYS $12,340" }),
            // Added when the POPULATION line below named them: the sweep's own §4.2 statement found
            // three slots that were neither swept nor a sibling row index, and two of them are
            // trivially enumerable. Closing the gap beats excusing it — a named gap is still a gap.
            ("Leg", "the scorebug's leg counter, bounded by MaxLegs (4)", new[]
                { "LEG 4/4", "LEG 1/4", "LEG 1/1" }),
            // `MomentumLabel` is GONE (T90-am/T93, batch 61) — dropped, not shortened, because its
            // overlap with the NEED line was positional. The slot no longer exists to sweep, and the
            // population line below will simply stop counting it.
            ("Matchup", "scoreline, generated team names", new[]
                { "ZAMBONIS 0 — REGULATORS 1", "BRICKLAYERS 0 — MIDDLEMEN 0", "STARTUPS 1 — PLUMBERS 2" }),
            ("Score", "the punch overlay mirrors Matchup", new[] { "ZAMBONIS 0 — REGULATORS 1" }),
            ("Clock", "clock forms seen in the after-set manifest", new[] { "90'+2", "90'+1", "PRE", "FT", "45'" }),
            ("TicketHeader", "tv.card.html:20", new[] { "TICKET 1 OF 2", "TICKET 2 OF 2" }),
            // REBUILT. The old set {"REGULATORS BREAK AWAY DOWN THE RIGHT", "ZAMBONIS CLEAR THE
            // LINE", "GOAL"} was invented ALL-CAPS narration this surface never authors — none of
            // those three lines exists in SweatFlavor.cs, and the strip's actual voice is lower-case
            // (observed directly in every deck below). The real content is SweatFlavor's ten
            // authored decks (ScoreUp, ScoreDown, BigUp, BigDown, MomUp, MomDown, CornerFor,
            // CornerAgainst, BookingFor, BookingAgainst — SweatFlavor.cs:102-257), transcribed
            // verbatim, plus the count-batch suffix TvSweatScreen.cs:1694-1695 appends.
            //
            // Six decks carry a `{picked}`/`{other}` placeholder (SweatFlavor.cs:84-90 Base()):
            // always a TEAM name (SweatFlavor.For computes picked/other from leg.Matchup.Home/Away
            // for every market kind, SweatFlavor.cs:22-24), so S84 generates it over the SAME closed
            // ClubNouns pool this file already uses for the leg row — SweatFlavor.Short() keeps only
            // the last word of the full name (SweatFlavor.cs:298-302), so the noun pool alone is the
            // complete input; no city pool is needed. FromEach cross-multiplies every template
            // against every noun, case PRESERVED (Short() never upper-cases — see FromEach's doc).
            //
            // The other four (Corner*/Booking*) are already complete literals — no placeholder.
            //
            // The suffix: read at its call site (TvSweatScreen.cs:1689-1703), `spec.Count.HasValue`
            // (required for the suffix) is set ONLY when the ACTIVE leg's own market is
            // TotalCorners/TotalCards (TheaterChoreographer.cs:56-64) — the exact condition under
            // which SweatFlavor.For always resolves via CornerLine/BookingLine (SweatFlavor.cs:
            // 31-34), ahead of the NearMiss override and Base()'s six-deck table, and under which
            // ResolveBeat hands the SceneSpec a null Goal (TheaterChoreographer.cs:91-93), so the
            // GoalLine overwrite at TvSweatScreen.cs:1679 cannot have run first. So exactly these
            // four decks can carry the suffix; the six Score/BigPlay/Momentum decks never reach it —
            // found by reading the call site, not assumed to be all ten.
            //
            // NOW INCLUDED, and the correction is the lead's not the dispatch's. These five were
            // first named as out-of-scope — correctly, against a brief that scoped this pool to "the
            // ten decks plus the suffix". THE BRIEF WAS THE GAP: `FINAL WHISTLE` (SweatFlavor.cs:20),
            // the two NearMiss lines (SweatFlavor.cs:41-42), `THE BOARD IS SET` (pregame,
            // TvSweatScreen.cs:2402) and `THE MATCH ENDS LEVEL` (drawn full time,
            // TvSweatScreen.cs:3127) all render in THIS box, so C46's population is the BOX's
            // renderable set, never the deck's.
            //
            // §5.1's rule cuts one way only here: a pool that INVENTS a string is caught the moment
            // anyone looks at it, while a pool that MISSES one is invisible in the frames — which is
            // exactly how a box passes on what is in it today and fails on the string nobody
            // captured. Naming an omission is honest; carrying it is better.
            //
            // No claim is made here about whether any of the five is wide. That is the sweep's
            // output (batch 95: the widest string in a column is a MEASUREMENT).
            ("Flavor", "SweatFlavor's ten authored decks (verbatim) + the count-batch suffix (TvSweatScreen.cs:1695) — see the block comment above for which four decks the suffix can reach and why, and for the named gaps",
                And(FromEach(FlavorPickedTemplates, ClubNouns), FromEach(FlavorOtherTemplates, ClubNouns),
                    CornerForAgainst, BookingForAgainst,
                    Suffixed(CornerForAgainst, MaxCornerBatchDelta), Suffixed(BookingForAgainst, MaxCardBatchDelta),
                    FlavorNonDeckLines)),
            ("Chrome", "PRD §8.1 chrome row", new[] { "ROUND 3   BANK $1,240   PAYMENT $800   SEED 48151623" }),

            // The six that were UNSWEPT, now enumerated from their assignment sites rather than
            // guessed. Five resolve to literals or to formats with bounded fields; the sixth is
            // marked CONSTRUCTED because its content is engine-generated and has no bound readable
            // from this surface.
            // T86-am2 (batch 56): states 4 and 5 were both violations — one a §3.1 gold-ration breach,
            // one the laptop's own verdict headline — and BOTH collapse to `BOARD CLOSED`. The
            // theatre reports that the board is shut and says nothing about how the run ended.
            ("Attract", "4 assignment sites: 2 literals, the run-over string, RenderIdle titles", new[]
                { "ROUND 10 OF 12 · BOARD OPEN", "SIT TO WATCH THE SWEAT", "BOARD CLOSED", "SHOP OPEN" }),
            ("TakeoverTitle", "3 assignment sites (a fourth clears it)", new[]
                { "SHORT — $12,340 AGAINST $20,000", "TICKET 1 OF 2", "PAYMENT MADE" }),
            // T89-B: "CONSTRUCTED" is RETIRED. This slot was carried as having no bounded worst case
            // at all, and that was the payout maximum's error a second time — un-enumerated is not
            // unbounded. `engine.tests/TakeoverSubBoundTests` enumerates it over the same 648,000
            // offers: the longest single entry is 91 chars, and the joined worst case at MaxLegs 4
            // with `   ·   ` separators is 385 chars.
            //
            // The longest entry is a MONEYLINE label, and that is not incidental — it is the engine's
            // concatenated form, `{CLUB} ML — {HOME} v {AWAY}`, which T69 ruled against on the leg row
            // as "a fact named twice". This slot still renders it raw.
            //
            // Both forms are swept: the JOINED string is what the slot renders today, and the SINGLE
            // ENTRY is what one row carries under T74-am3's list ruling — so the list's own row width
            // is answered here rather than after it is built.
            // T92 (batch 60): the entries are G1's COMPACT forms now, one per row, so the swept set is
            // the ROW — the engine's concatenated `DisplayLabel` is gone from this slot. The compact
            // forms are the same deck LegRowLine0 sweeps, plus the American price.
            // T92-am (batch 61): the leg list LEFT this slot — it restated a list the ticket column
            // already carries permanently. What remains is the deferral line, which was always the
            // widest string here once the list's over-wide entry was replaced.
            ("TakeoverSub", "the deferral line — the leg list left at T92-am", new[]
                { "PAYMENT DEFERRED — YOUR BANK STANDS. THE NEXT ONE GROWS BY $1,200" }),
            ("Subtitle", "RenderIdle sub, plus the run-over line", new[]
                { "FINAL BANK $12,340  —  NEW RUN AT THE LAPTOP",
                  "gear up at the laptop, then the next round" }),
            ("Consolation", "the authored four-line deck, verbatim", new[]
                { "the model remains extremely confident.", "the book thanks you for your patronage.",
                  "a courtesy: nobody saw that.", "so close. they always are." }),
            // The prompt is a LIST now (DD batch 50), so its renderable forms are ROWS rather than one
            // run-on line, and a row is what this sweep's widest-line measure was always reporting.
            // T88's gesture adds the two held-preview rows. Height is not this instrument's question
            // and is answered by `SBR/TV/T88 prompt composition`, which owns the zone's 90px.
            // Batch 56: `SHOT FROZEN` left the zone (the stage already says it) and the decline lost
            // its HOLD — it takes a press, so the copy says so.
            ("InterventionPrompt", "the list's rows + the held preview's, from PendingWindowBeat", new[]
                { "HOLD M MULLIGAN (ONE MULLIGAN SLIP)",
                  "HOLD R SEND TO REVIEW (ONE REF'S WHISTLE)", "N LET IT DIE",
                  "ENTER CONFIRMS   ·   RELEASE ABANDONS" }),
        };

        /// <summary>Slots reported but NOT swept. Empty now: the six that stood here were enumerated
        /// from their assignment sites, which is what "a slot is only as swept as its string set is
        /// complete" obliges.
        ///
        /// <para><b>One residual, carried rather than cleared.</b> `TakeoverSub` renders the ticket's
        /// leg list — <c>DisplayLabel</c> joined by a separator, one entry per leg — and
        /// <c>DisplayLabel</c> is the ENGINE's label, the long concatenated form T69 ruled against on
        /// the leg row. Its length is not bounded by anything readable on this surface, so its entry
        /// is a CONSTRUCTED three-leg worst case, not an enumeration. A longer ticket or a longer
        /// fixture makes it longer, and this sweep cannot say by how much.</para></summary>
        private static readonly string[] Unswept =
        {
            // DELIBERATELY excluded, and it is the only one. `_tBigAmount` is built, cleared on reset
            // and NEVER GIVEN CONTENT — both payoff figures moved into the cash-out slot at T68-am/T71
            // and nothing has requested it since. It renders no string, so it has no longest
            // renderable form to sweep. Named here rather than silently skipped, because "renders
            // nothing" and "was overlooked" are indistinguishable from a count alone.
            "BigAmount",
        };

        [MenuItem("SBR/TV/T84 extent sweep")]
        public static void Sweep()
        {
            var go = new GameObject("ExtentSweep");
            go.SetActive(false);
            try
            {
                var screen = go.AddComponent<SBR.Game.TvSweatScreen>();
                screen.theaterEnabled = false;
                typeof(SBR.Game.TvSweatScreen)
                    .GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(screen, null);

                var all = new Dictionary<string, TMP_Text>();
                foreach (TMP_Text t in screen.GetComponentsInChildren<TMP_Text>(true))
                    all[t.gameObject.name] = t;

                Debug.Log($"[T84] sweeping {Cases.Length} slots with string sets; " +
                          $"{Unswept.Length} reported unswept; {all.Count} text slots exist on the surface");

                int overrunning = 0;
                foreach ((string slot, string source, string[] strings) in Cases)
                {
                    if (!all.TryGetValue(slot, out TMP_Text t))
                    {
                        Debug.Log($"[T84] {slot,-16} NOT BUILT in this configuration — not swept");
                        continue;
                    }
                    float box = t.rectTransform.rect.width;
                    float[] digitPx = MeasureDigits(t);
                    // The tabular advance, as an UPPER BOUND measured on this component rather than
                    // assumed. The first cut used 0.5 * fontSize — 1000 of the font's 2000 upem — and
                    // that is only true of the DEFAULT instance. These assets are other instances of
                    // a variable family, where the same glyph's advance scales, and the tell was
                    // LegRowPrice screening NARROWER on a string of narrow digits, which no tabular
                    // set can do. Using the widest proportional digit instead: a tabular set is
                    // uniform and sits at or below its family's widest figure (1000 against 1036 in
                    // the default instance here), so this never understates, which is the only
                    // direction a screen may err in.
                    float tabularPx = 0f;
                    foreach (float d in digitPx) tabularPx = Mathf.Max(tabularPx, d);

                    float worst = float.MinValue, worstTab = float.MinValue;
                    string worstS = "", worstTabS = "";
                    int laddered = 0;
                    foreach (string composed in strings)
                    {
                        // LADDER SELECTION, applied before measurement so the sweep tests what the
                        // surface will DRAW rather than what it can compose. A rung that overruns is
                        // never rendered — FitOrFallback picks the next one — so measuring it would
                        // certify the box against a string it can never receive.
                        string s = composed;
                        float w = t.GetPreferredValues(s, Unconstrained, 0f).x;
                        if (w > box)
                        {
                            string next = LadderFallback(slot, s);
                            if (next != null)
                            {
                                s = next;
                                w = t.GetPreferredValues(s, Unconstrained, 0f).x;
                                laddered++;
                            }
                        }
                        if (w > worst) { worst = w; worstS = s; }
                        float tab = w;
                        foreach (char c in s)
                            if (c >= '0' && c <= '9') tab += tabularPx - digitPx[c - '0'];
                        if (tab > worstTab) { worstTab = tab; worstTabS = s; }
                    }

                    bool over = worstTab > box;
                    if (over) overrunning++;
                    // T89-B wants the tabular BASIS per row, and "no digits" was doing two jobs: it
                    // printed for a string with no figures in it AND for a string whose figures are
                    // already uniform, which are different facts. `RISK $1,234     PAYS $12,340` is
                    // full of digits and still screened to zero — because the derived tabular font is
                    // wired and the advances are already equal. Reporting that as "no digits" hid the
                    // very confirmation the condition asks for.
                    bool hasDigits = false;
                    foreach (char c in worstTabS) if (c >= '0' && c <= '9') { hasDigits = true; break; }
                    string digitNote = !hasDigits ? "no digits in the widest form"
                        : Mathf.Approximately(worstTab, worst) ? "digits ALREADY TABULAR — screen is a no-op"
                        : $"screened from '{Show(worstTabS)}'";
                    Debug.Log($"[T84] {slot,-16} box {box,6:0.0}px  widest '{Show(worstS)}' {worst,6:0.0}px  " +
                              $"TABULAR {worstTab,6:0.0}px ({digitNote})  " +
                              // `t.font` is the PRIMARY asset, not the arm that renders. A slot built
                              // at FontWeight.Bold draws through the bold asset wired by WireBold
                              // while `font.name` still reads the regular one — so printing the face
                              // alone said `EncodeSansCondensed SDF` for CashOut, which is real
                              // Condensed BOLD 700 (T73) and is recorded as such in the DD's own
                              // batch-38 inventory. The weight is what disambiguates the pair, so the
                              // weight ships beside the face.
                              $"[face '{t.font?.name}' w{(int)t.fontWeight} style {t.fontStyle} " +
                              $"tracking {t.characterSpacing / 100f:0.000}em type {t.fontSize:0.#}px]  " +
                              $"{(over ? $"OVERRUNS by {worstTab - box:0.0}px" : $"fits, {box - worstTab:0.0}px spare")}  " +
                              $"{(laddered > 0 ? $"[{laddered} forms took the ladder's next rung] " : "")}" +
                              $"· set: {source}");
                }

                // The two-into-one-box members: §6.1's money control and the leg row's three spans
                // share a rectangle from opposite edges, so each one's slack is the other's overrun.
                // RETIRED at batch 56, and retiring it is the point: T74-am3 put the figure and the
                // status word on SEPARATE ROWS, so they no longer share a rectangle from opposite
                // edges and no pair of strings can collide on this control at all. Leaving the check
                // in would keep reporting collisions for a composition that no longer exists — a
                // measurement of the old build, printed with the confidence of the new one, which is
                // the same class as the stale expected-value this lane already paid for once.
                //
                // Each member's own width is swept individually above, which is now the whole
                // question for this control.

                foreach (string s in Unswept)
                    Debug.Log($"[T84] {s,-16} UNSWEPT — longest renderable form not enumerable from here");

                Debug.Log($"[T84] slots overrunning their fixed box: {overrunning} of {Cases.Length} swept");

                // T89-B §4.2 — WHAT THE SWEPT COUNT IS A COUNT OF, computed rather than asserted.
                //
                // The report has been saying "N of 20 swept" beside "48 text slots exist", and those
                // two numbers invite exactly one wrong reading: that 28 slots go unexamined. They do
                // not — the leg row is built six times from one construction, so its five elements
                // appear at six indices and the sweep covers index 0. Saying so in prose would be a
                // claim; this derives it, and NAMES anything that is neither swept nor a sibling,
                // which is the only part that could hide a gap.
                var sweptNames = new HashSet<string>();
                foreach ((string slot, string _, string[] __) in Cases) sweptNames.Add(slot);

                var declared = new HashSet<string>(Unswept);
                var siblings = new List<string>();
                var excluded = new List<string>();
                var uncovered = new List<string>();
                foreach (string name in all.Keys)
                {
                    if (sweptNames.Contains(name)) continue;
                    if (declared.Contains(name)) { excluded.Add(name); continue; }
                    // `LegRowNeed3` is `LegRowNeed0`'s construction at another index: strip the
                    // trailing digits and ask whether index 0 of the same family is swept.
                    string stem = name.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
                    if (stem.Length < name.Length && sweptNames.Contains(stem + "0")) siblings.Add(name);
                    else uncovered.Add(name);
                }
                uncovered.Sort();
                excluded.Sort();

                Debug.Log($"[T84] POPULATION: {all.Count} text slots exist · {Cases.Length} swept · " +
                          $"{siblings.Count} the same construction at another row index (covered by index 0) · " +
                          $"{excluded.Count} declared unswept · {uncovered.Count} unaccounted for");
                if (excluded.Count > 0)
                    Debug.Log($"[T84] DECLARED UNSWEPT (renders no string): {string.Join(", ", excluded)}");
                if (uncovered.Count > 0)
                    Debug.Log($"[T84] UNACCOUNTED FOR — this number must be 0: {string.Join(", ", uncovered)}");
            }
            finally { Object.DestroyImmediate(go); }
        }

        /// <summary>A multi-line string on one log line. `InterventionPrompt` carries a newline, and
        /// its row came out fractured across three log lines with its verdict on the third — a table
        /// that cannot be read as a table. The measurement was right; the report was not.</summary>
        private static string Show(string s) => s.Replace("\n", "\\n");

        /// <summary>Each digit's CURRENT advance on this component, in px, measured as ten of it.
        ///
        /// <para>Batch 41 binds digit rows to TABULAR metrics: the assets are still built from the
        /// source face, so every digit-bearing row measured as-shipped is measured proportionally,
        /// and against the widest realistic digits that UNDERSTATES what the wired surface will do.
        /// The screen replaces each digit's proportional advance with the tabular one — 1000 of the
        /// font's 2000 upem, exactly half an em, which the derived font makes true for all ten.</para>
        ///
        /// <para>Measured rather than read from hmtx so the number is in the same units, on the same
        /// component, as the string widths it corrects. Screening only: a flagged box takes rendered
        /// confirmation once the wiring lands.</para></summary>
        private static float[] MeasureDigits(TMP_Text t)
        {
            var px = new float[10];
            for (int d = 0; d < 10; d++)
                px[d] = t.GetPreferredValues(new string((char)('0' + d), 10), Unconstrained, 0f).x / 10f;
            return px;
        }

        /// <summary>Two slots anchored from opposite edges of one rectangle. Neither overruns its own
        /// box; together they overprint, which is the shape the pair caught and which no single-slot
        /// measurement can see.</summary>
        private static void Pair(Dictionary<string, TMP_Text> all, string aName, string aText,
                                 string bName, string bText)
        {
            if (!all.TryGetValue(aName, out TMP_Text a) || !all.TryGetValue(bName, out TMP_Text b)) return;
            float box = a.rectTransform.rect.width;
            float aw = a.GetPreferredValues(aText, 0f, 0f).x;
            float bw = b.GetPreferredValues(bText, 0f, 0f).x;
            float slack = box - (aw + bw);
            Debug.Log($"[T84] PAIR {aName}+{bName}  box {box:0.0}px  '{aText}' {aw:0.0} + '{bText}' {bw:0.0} " +
                      $"= {aw + bw:0.0}px  {(slack < 0f ? $"COLLIDES by {-slack:0.0}px" : $"clears by {slack:0.0}px")}");
        }
    }
}
