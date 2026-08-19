using System;
using System.Collections.Generic;
using System.Globalization;
using SBR.Engine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SBR.Game
{
    /// <summary>SureThing's card-book app. It is a renderer over BetslipModel, RunDirector, and the TV view.</summary>
    public sealed class SportsbookApp
    {
        public enum Tab { Lobby, Detail, MyBets, Rewards }

        private readonly RectTransform _root;
        private readonly TMP_FontAsset _font;
        // --font-cond (Archivo Narrow) seam: figures, prices, team names and the wax/lock/rub-out
        // action labels route through this instead of _font. See LaptopScreen's field comment — both
        // currently resolve to the same fallback face on purpose.
        private readonly TMP_FontAsset _fontCond;
        private readonly LaptopScreen _host;
        private readonly Action _invalidate;
        private readonly Action<Tab> _selectTab;
        private readonly Action _home;
        private readonly Action _ledger;
        private bool _lockArmed;
        private int _armedRound = -1;
        private string _shopError = string.Empty;
        private bool _shopMessageIsError;
        private int _detailMatchup = -1;

        /// <summary>ENTRY's selected destination (spec-market-surfaces-2026-08-17.md §3). Defaults
        /// to RESULT — the first stop on <see cref="MarketDestinations.All"/>'s rail order, and the
        /// market every matchup always prices.</summary>
        private MarketDestination _destination = MarketDestination.Result;

        /// <summary>§5.2's printed contents block, open over the market body. State, not a second
        /// navigation tier: the RAIL stays one level and this is a page you read.</summary>
        private bool _contentsOpen;

        /// <summary>Fixed gap between an offer row's label cell and its price cell (MakeOfferRow).
        /// Shared as a class constant so every destination's row layout and MakeOfferRow's own
        /// price placement can never silently drift apart.</summary>
        internal const float OfferLabelGap = 8f;

        /// <summary>The offer row's own page margins and price cell, promoted out of
        /// <see cref="MakeOfferRow"/>'s locals for one reason: <c>C46</c>'s width gate has to be
        /// measured against THE ROW'S OWN numbers, not against a second copy of them written down
        /// beside it. Same discipline as <see cref="OfferLabelGap"/>.</summary>
        internal const float OfferLeftPad = 14f;

        internal const float OfferRightPad = 14f;

        internal const float OfferPriceCellWidth = 176f;

        /// <summary>4px RuleSoft track + 4px clearance (A4: a row never runs under the position
        /// rail). Named because it is the difference between the two row widths this surface has —
        /// <see cref="EntryBoardWidth"/> when the sheet fits, and that less this when it scrolls —
        /// and <c>C46</c>'s gate is judged against the NARROW one.</summary>
        internal const float ScrollRailReserve = 8f;

        /// <summary>The offer row's width when the sheet scrolls, which on the market sheet is
        /// nearly always. The narrow case, and therefore the case a name has to fit.</summary>
        internal const float ScrollingOfferRowWidth = EntryBoardWidth - ScrollRailReserve;

        /// <summary>The width the printed row NAME actually has, on a row of
        /// <paramref name="rowWidth"/>: from the left pad to the price cell's left edge, less the
        /// annotation gap. DERIVED here so <see cref="MakeOfferRow"/> and <c>C46</c>'s gate cannot
        /// disagree about how much room a name has.
        ///
        /// <para><paramref name="rowWidth"/> is <see cref="EntryBoardWidth"/> when the sheet fits
        /// and 8px less when it scrolls (BuildScrollingBody's rail reserve) — the scrolling case is
        /// the narrow one and is what a gate must be judged against.</para></summary>
        internal static float OfferNameCellWidth(float rowWidth)
            => rowWidth - OfferRightPad - OfferPriceCellWidth - OfferLabelGap - OfferLeftPad;

        /// <summary>
        /// <c>S96</c> (DD batch 113) — <b>the sheet UPPERCASES row names, and it does it HERE, at
        /// the presentation layer.</b>
        ///
        /// <para><c>A2</c> is NOT overridden and the distinction is the whole ruling: <c>A2</c>
        /// fixes the WORDS — <see cref="MarketSheetRow.Name"/> stays the engine's own field,
        /// verbatim, same words in the same order, and <c>MarketSheetTests</c> still asserts that.
        /// CASE IS TYPOGRAPHY, and typography is the surface's: this row already sets the face, the
        /// size, the tracking and the colour of that same string.</para>
        ///
        /// <para>The ruling is a read off the docked frames — <c>Moose Jaw Overheads</c> sitting in
        /// title case in the same column, at the same size, directly beneath an uppercase
        /// <c>MONEYLINE</c> heading and beside an uppercase <c>DRAW</c> and <c>EITHER TEAM</c>. The
        /// group headings (<c>MarketDestinations.KindLabel</c>) and the scorer ROLE word
        /// (<c>MatchModel.RoleWord</c>) were CHECKED rather than assumed: both are already
        /// uppercase at their source, so this is the one string on the row that was drifting.</para>
        ///
        /// <para>Named so <c>C46</c>'s width gate can measure the string the row actually prints
        /// rather than its own copy of this rule.</para>
        /// </summary>
        internal static string PrintedRowName(string name)
            => string.IsNullOrEmpty(name) ? string.Empty : name.ToUpperInvariant();

        /// <summary>A1 ruling: every destination's offer row is 54px tall, full content width,
        /// single column. Shared so BuildMarketLines/BuildBothTeamsScore/BuildPlayerLines,
        /// BuildScrollingBody's content-height math, and MakeOfferRow's own row rect can never
        /// independently drift.</summary>
        private const float OfferRowHeight = 54f;

        public SportsbookApp(RectTransform root, TMP_FontAsset font, TMP_FontAsset fontCond, LaptopScreen host, Action invalidate,
            Action<Tab> selectTab, Action home, Action ledger)
        {
            _root = root;
            _font = font;
            _fontCond = fontCond;
            _host = host;
            _invalidate = invalidate;
            _selectTab = selectTab;
            _home = home;
            _ledger = ledger;
        }

        public void Render(Run run, BetslipModel slip, Tab tab, bool boardFrozen)
        {
            // A confirmation only survives a rebuild of this same betting lobby.
            if (tab != Tab.Lobby || run.Phase != Phase.Betting || _armedRound != run.Round)
            {
                _lockArmed = false;
                _armedRound = -1;
            }
            LaptopUi.ClearChildren(_root);
            LaptopUi.MakePanel(_root, "AppBacking", Vector2.zero, Vector2.zero, Vector2.zero,
                _root.sizeDelta, LaptopOs.Ink);
            BuildChrome(run, tab, boardFrozen);
            if (tab == Tab.Lobby) BuildLobby(run, slip, boardFrozen);
            else if (tab == Tab.Detail) BuildDetail(run, slip, boardFrozen);
            else if (tab == Tab.MyBets) BuildMyBets(_host.tv != null ? _host.tv.RevealedView : null);
            else BuildRewards(run);
            BuildTaskbar();
        }

        private void BuildChrome(Run run, Tab tab, bool boardFrozen)
        {
            RectTransform top = LaptopUi.MakePanel(_root, "Chrome", new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(_root.sizeDelta.x, 140f), LaptopOs.Ink);
            NotebookChrome.BuildRail(top, 1024f, _font);
            RectTransform tabs = LaptopUi.MakePanel(top, "FormTabs", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -34f), new Vector2(1024f, 38f), LaptopOs.Surface);
            // S31: the persistent four-tab strip lives here, once, and OldSlipsApp.BuildLedgerChrome
            // calls the same static method rather than fabricating a second "LEDGER" tab of its own.
            BuildTabStrip(tabs, tab, run.Phase, "SHEET 1 OF 1", _font, _selectTab);
            RectTransform mast = LaptopUi.MakePanel(top, "FormMasthead", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -72f), new Vector2(1024f, 68f), LaptopOs.Ink);
            // F1: Masthead.jsx's own border-bottom (--rule-w-strong solid var(--rule)) — same
            // missing seam, into the board below. Board and masthead share LaptopOs.Ink, which is
            // why this one never showed up as a flat-colour pixel step even though the kit calls
            // for it unconditionally.
            LaptopUi.MakeRule(mast, "MastheadRule", new Vector2(0f, 0f), new Vector2(0f, 0f), Vector2.zero, new Vector2(1024f, 2f), LaptopOs.Rule);
            // S46: the brand is the name and nothing else. FORM is a screen, not part of the name —
            // the tab strip 38px above already says FORM, and "SURETHING FORM" is the same
            // construction S16 deleted in "SURETHING LEDGER". The 300px box is unchanged: it was
            // already sized for the longer string, and BuildRunFigures starts at x=398.
            LaptopUi.MakeText(mast, "Brand", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -8f), new Vector2(300f, 28f), 26, TextAnchor.UpperLeft, LaptopOs.White, "SURETHING", _fontCond, LaptopTrack.Names);
            // Batch 21: PRICES FINAL leaves this subline. The masthead carries the run's SCOPE and
            // nothing else — whether a price can still move is the board's fact, not the run's, and
            // stating it here made one 13px line carry two registers. Text is deleted INSIDE the
            // existing 340x20 box: no re-derivation, and the box was already sized for the longer
            // string, so nothing below it moves.
            //
            // It also closes the disagreement S67 filed. This site and the LEDGER's "Scope"
            // (BuildLedgerChrome, ~:2261) print the identical string now, and SureThingLedgerTests
            // has asserted that string on the LEDGER side since batch 7 — where S37's first instance
            // was resolved the same way, by deleting the restating clause rather than the line.
            // The two sites still differ in NAME ("Run" here, "Scope" there) and still build the
            // string inline rather than through a LaptopUi helper. That is S67's actual finding and
            // this ruling did not touch it: it is left alone rather than tidied in passing.
            LaptopUi.MakeText(mast, "Run", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(17f, -38f), new Vector2(340f, 20f), 13, TextAnchor.UpperLeft, LaptopOs.Muted, $"ROUND {run.Round} OF {run.Config.Rounds}", _font);
            // S31: the masthead's run figures, shared with OldSlipsApp.BuildLedgerChrome so LEDGER
            // carries the exact same BANK/TARGET/TICKETS figures rather than a parallel string.
            BuildRunFigures(mast, run, _fontCond);
        }

        /// <summary>S31: SectionTabs.jsx's own strip — the border-bottom, the four tabs and the
        /// meta line — built once here so every destination that carries it (including LEDGER,
        /// via OldSlipsApp.BuildLedgerChrome) shares this exact mechanism instead of a second
        /// hand-rolled copy. <paramref name="active"/> is null wherever the current destination
        /// is not one of the four tabs (LEDGER): SectionTabs.jsx itself renders every tab
        /// unselected when `active` matches none of `tabs`, so this reproduces that by
        /// construction rather than special-casing it.</summary>
        internal static void BuildTabStrip(RectTransform tabs, Tab? active, Phase phase, string meta,
            TMP_FontAsset font, Action<Tab> selectTab)
        {
            // F1: SectionTabs.jsx's own border-bottom (--rule-w-strong solid var(--rule)) — flat
            // colour step into the masthead below, no seam drawn.
            LaptopUi.MakeRule(tabs, "TabsRule", new Vector2(0f, 0f), new Vector2(0f, 0f), Vector2.zero, new Vector2(1024f, 2f), LaptopOs.Rule);
            MakeTab(tabs, "FORM", Tab.Lobby, active, phase == Phase.Shop, font, selectTab);
            MakeTab(tabs, "ENTRY", Tab.Detail, active, phase == Phase.Shop, font, selectTab);
            MakeTab(tabs, "MY BETS", Tab.MyBets, active, phase == Phase.Shop, font, selectTab);
            MakeTab(tabs, "REWARDS", Tab.Rewards, active, phase != Phase.Shop, font, selectTab);
            // The strip's meta (`SHEET 1 OF 1`, and `READ ONLY` on the LEDGER) takes the tab
            // tracking too: it sits in the tabs band and reads as part of the strip rather than as
            // a fact of the screen below it.
            LaptopUi.MakeText(tabs, "Sheet", new Vector2(1f, .5f), new Vector2(1f, .5f), new Vector2(-14f, 0f), new Vector2(170f, 24f), 13, TextAnchor.MiddleRight, LaptopOs.Muted, meta, font, LaptopTrack.Tabs);
        }

        private static void MakeTab(RectTransform top, string label, Tab tab, Tab? selected, bool disabled,
            TMP_FontAsset font, Action<Tab> selectTab)
        {
            float x = tab == Tab.Lobby ? 14f : tab == Tab.Detail ? 122f : tab == Tab.MyBets ? 230f : 358f;
            bool active = selected.HasValue && tab == selected.Value;
            LaptopUi.MakeButton(top, label, label, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(x, 3f), new Vector2(tab == Tab.MyBets ? 116f : 100f, 32f), 13,
                active ? LaptopOs.Ink : LaptopOs.Surface,
                disabled ? LaptopUi.Dim(LaptopOs.Muted) : active ? LaptopOs.White : LaptopOs.Muted,
                // C15/S28: `.11` (owning doc §4.3), overriding MakeButton's `.14` action default.
                // A tab is a place, not an act — the strip persists across every destination and is
                // non-interactive on the LEDGER entirely (S31-am), which is precisely why it does not
                // wear the action tracking.
                disabled ? null : () => { selectTab(tab); }, font, !disabled, LaptopTrack.Tabs);
        }

        /// <summary>S31: the masthead's run figures (BANK/TARGET/TICKETS) — the register calls
        /// these "unchanged" across every destination that carries the masthead, so this is
        /// written once and OldSlipsApp.BuildLedgerChrome calls it too.
        ///
        /// **S29 CLOSES here, and the face is why.** These rendered in the ROMAN face, and at the
        /// ruled Regular 400 Archivo's digits are proportional — spread 4.7656 units, 1.112px at this
        /// 21px — so BANK and TARGET changed width as the bank changed. That is the horizontal jitter
        /// tabular figures exist to stop, on the most-watched facts on the screen, and TMP cannot fix
        /// it: OTL_FeatureTag declares only kern, liga, mark and mkmk, so there is no tnum to enable.
        ///
        /// It was getting tabular digits by accident before, because the surface was at the wrong
        /// weight: Archivo's DEFAULT face is SemiBold, which is near-tabular at spread 0.1875.
        /// Correcting the roman voice to Regular 400 removed the accident and exposed this.
        ///
        /// Archivo Narrow measures spread 0 — every digit 41.05, tabular by construction — and owning
        /// doc §4.1 already assigns BOTH "figures" and "masthead" to the condensed face. So this is a
        /// conformance gap closing rather than a redesign, which is the only way a Design-verified
        /// masthead changes (ruled by Allen, 2026-08-08).</summary>
        internal static void BuildRunFigures(RectTransform mast, Run run, TMP_FontAsset fontCond)
        {
            LaptopUi.MakeText(mast, "Figures", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-16f, -10f), new Vector2(610f, 48f), 21, TextAnchor.UpperRight, LaptopOs.White, $"BANK {LaptopUi.Money(run.Bank)}    TARGET {LaptopUi.Money(run.CurrentPayment)}    TICKETS {run.Tickets.Count}/{run.Config.MaxTicketsPerRound}", fontCond);
        }

        /// <summary>The lobby's card pitch — the single source for it. <see cref="BuildMatchupCard"/>
        /// builds each card at this height, <see cref="BuildScrollingBody"/> measures the scroll
        /// content from it, and <see cref="BuildLobby"/> steps the cards down by it, so the three
        /// cannot disagree about how tall a matchup row is.
        ///
        /// It briefly existed twice — this constant plus a literal in BuildMatchupCard's own size —
        /// which is the drift the one-shared-component discipline exists to prevent, arriving in the
        /// same change that removed a duplicate elsewhere. Folded on Allen's instruction
        /// (2026-08-03). A row height read by a mask, a rail and a layout is exactly the kind of
        /// number that must not be written down more than once.</summary>
        /// <para><b>S74-am (batch 65) RE-DERIVED THIS: 78 → 116, because the block is now THREE
        /// lines.</b> A fixed grid constant re-derived ONCE AT DESIGN TIME is explicitly legal
        /// (§2, T51, S40); a zone resizing to content at runtime is not — so the block is 116px
        /// whether or not a given matchup prices a draw, and a match with no draw price renders that
        /// line EMPTY rather than collapsing the block. That is the ruling's pre-commitment (2), and
        /// a ragged board whose block height depends on the market is the thing §2 forbids.</para>
        ///
        /// <para><b>The derivation, measured rather than estimated.</b> The block's line pitch is
        /// 38px — it is the gap between the two <see cref="TeamLine"/> calls (−6 and −44), not a
        /// number invented here. One more line is one more pitch: <b>78 + 38 = 116</b>. Every
        /// existing relationship is preserved by construction because the AWAY line does not move at
        /// all, the DRAW takes the position HOME used to hold, and HOME moves down exactly one
        /// pitch — so the 3px of slack between the last price cell and the card's rule is the same
        /// 3px it was at 78 (81 + 32 = 113 against 116, as 43 + 32 = 75 was against 78).</para>
        ///
        /// <para><b>The visible count, which S74-am left OWED and eyeballed as "about four".</b> The
        /// list area is <c>530 − 26</c> = <b>504px</b> (BoardBody, title strip excluded). 504/78 =
        /// 6.46 → <b>six blocks today</b>; 504/116 = 4.34 → <b>four</b>. The measurement agrees with
        /// the DD's read off the frame. C19 is not breached: the list SCROLLS (S25-am) with S27's
        /// printed position rail, so every priced offer stays reachable by a mechanism that already
        /// exists, and §2's yield order is NOT invoked — a third outcome is a product fact arriving,
        /// not a layout overflowing.</para></summary>
        private const float MatchupCardPitch = 116f;

        private void BuildLobby(Run run, BetslipModel slip, bool boardFrozen)
        {
            RectTransform board = LaptopUi.MakePanel(_root, "Board", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -140f), new Vector2(700f, 530f), LaptopOs.Ink);
            LaptopUi.MakeText(board, "BoardTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -5f), new Vector2(670f, 26f), 13, TextAnchor.UpperLeft, LaptopOs.Muted,
                boardFrozen ? "NO.   MATCHUP · RECORD                         MONEYLINE     BOARD CLOSED" : "NO.   MATCHUP · SEASON RECORD                         MONEYLINE     MORE", _font);

            // Allen ruling (2026-08-03): placed tickets now draw on FORM too, above the six
            // matchup cards, via the SAME BuildPlacedThisRound/BuildStagedReceipt already used on
            // ENTRY (~line 492/964) — one shared component consumed twice, not a second
            // implementation, so the two screens can never drift apart. One 2-leg receipt runs
            // ~99px and up to MaxTicketsPerRound can stage; the board's old fixed layout (26px
            // title + 6*78px cards = 494 of the panel's 530) left only 36px of slack, and no
            // rearrangement of numbers closes that gap. So this list scrolls, reusing
            // BuildScrollingBody exactly as ENTRY's market body already does (S25/S27) — a ruled
            // mechanism, not an invented one. This deviates from the kit, which only ever draws a
            // receipt stack on the event screen (screens.jsx:49-58) — flagged here for the DD to
            // carry as a canon amendment.
            //
            // BoardTitle above is a column head for the list below it ("NO. MATCHUP ...
            // MONEYLINE"), not itself a row of that list, so it stays fixed here and only the
            // region beneath it scrolls.
            const float titleStripHeight = 26f; // the gap the first card always sat below (old i=0 -> y=-26).
            RectTransform boardBody = LaptopUi.MakePanel(board, "BoardBody", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, -titleStripHeight),
                new Vector2(700f, 530f - titleStripHeight), new Color(0f, 0f, 0f, 0f));

            RectTransform content = BuildScrollingBody(boardBody, run.CurrentSlate.Matchups.Count, run,
                out float rowWidth, out float rowsOffsetY, MatchupCardPitch);

            for (int i = 0; i < run.CurrentSlate.Matchups.Count; i++)
            {
                Matchup matchup = run.CurrentSlate.Matchups[i];
                // BuildMatchupCard keeps its own fixed 700px card width (frozen out of scope by
                // the ruling above) rather than taking rowWidth. Its live content already clears
                // the rail's 8px reserve — the MORE button's right edge sits at x=686 (700 - 14),
                // inside the 692px rail-safe width — so the only thing the rail can ever draw over
                // is the card's own decorative Surface background and rule; nothing interactive is
                // lost.
                BuildMatchupCard(content, matchup, slip, boardFrozen,
                    new Vector2(0f, -rowsOffsetY - i * MatchupCardPitch));
            }

            BuildSlip(run, slip, boardFrozen);
        }

        private void BuildMatchupCard(RectTransform parent, Matchup matchup, BetslipModel slip, bool frozen,
            Vector2 position)
        {
            RectTransform card = LaptopUi.MakePanel(parent, "Matchup" + matchup.Index, new Vector2(0f, 1f),
                new Vector2(0f, 1f), position, new Vector2(700f, MatchupCardPitch), LaptopOs.Surface);
            bool awaySelected = slip.Contains(matchup.Index, MarketSelection.Moneyline(Side.Away));
            bool homeSelected = slip.Contains(matchup.Index, MarketSelection.Moneyline(Side.Home));
            // S74-am: the draw is a third markable outcome on this block, so every place that asked
            // "away or home" is now a three-way question. Swept together rather than one site at a
            // time — a marked draw that lit no wash and drew no ring would be a selection the board
            // renders as unselected, which is the state lie T43 cost this studio a batch over.
            bool drawSelected = slip.Contains(matchup.Index, MarketSelection.MoneylineDraw());
            // The wash behind a form entry he has marked (palette-surething.css --marked-wash).
            // Added first, before any text/buttons, so it sits behind them; sized to fill the whole
            // card so it is trivially contained within it.
            if (awaySelected || homeSelected || drawSelected)
                LaptopUi.MakeMarkedWash(card, "MarkedWash");
            LaptopUi.MakeText(card, "Number", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -10f), new Vector2(30f, 56f), 15, TextAnchor.UpperLeft, LaptopOs.Muted, (matchup.Index + 1).ToString("00"), _fontCond);
            // A record belongs to the name it follows, so it is set on the same line, 9px after it —
            // the design system's FormEntry.line() lays out exactly that: one 30px flex line holding
            // the name in the condensed voice and the record in the data voice beside it.
            //
            // An earlier pass split them into two fixed columns, names at x=54 and records parked at
            // x=310. That satisfied the two type sizes the spec asks for but broke the association:
            // the record floated in open space with nothing tying it to its team. Fixed columns
            // cannot express "immediately after", because the name's width varies per team, so each
            // record is positioned off its own name's measured width instead.
            TeamLine(card, "Away", LaptopUi.TeamShort(matchup.Away), matchup.Away.Record, -6f);
            // S74-am — THE MATCHUP COLUMN IS EMPTY ON THE DRAW'S LINE (−44), and the absence of a
            // TeamLine call here is the ruling, not an omission. Empty is the CORRECT rendering of
            // "neither". This is NOT S24's dead cell: S24 refused an offer slot with no OFFER; here
            // the SUBJECT slot has no subject, because the draw has no team. Naming anything there
            // would invent the third competitor `Side` exists to refuse. No team treatment either —
            // no dot, no crest, no hue (T2 gives muted blue and pink to the two SIDES, and a draw
            // has no side).
            TeamLine(card, "Home", LaptopUi.TeamShort(matchup.Home), matchup.Home.Record, -82f);

            LaptopUi.MakeButton(card, "AwayOdds", $"AWAY  {OddsFormat.American(matchup.AwayOdds)}",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(462f, AwayCellY), new Vector2(112f, 32f), 19,
                OfferField(OfferIsTakeable(slip, _host.director.Run, matchup.Index, MarketSelection.Moneyline(Side.Away))), frozen ? LaptopUi.Dim(LaptopOs.Muted) : LaptopOs.White,
                // C15/S28: `.03`, NOT MakeButton's `.14` action default. A price control is a price
                // first — `PriceCell.jsx` and `MarketOffer.jsx` both set `--st-track-name` on it, and
                // the token's own comment reads "team names, prices, masthead". The previous commit's
                // premise (a button label is an action label) is wrong for exactly this class, and
                // the kit is what says so.
                frozen || !OfferIsTakeable(slip, _host.director.Run, matchup.Index, MarketSelection.Moneyline(Side.Away)) ? (System.Action)null
                    : () => { PickOffer(slip, matchup.Index, MarketSelection.Moneyline(Side.Away)); }, _fontCond,
                    !frozen && OfferIsTakeable(slip, _host.director.Run, matchup.Index, MarketSelection.Moneyline(Side.Away)), LaptopTrack.Names);
            // S74-am — THE DRAW GOES IN THE PRICE CELL, in the slot HOME used to hold.
            // The price cell is the one that names the OUTCOME, never the matchup column, which
            // names TEAMS: the board already reads `AWAY −156` rather than `NOTARIES −156`, so the
            // draw's grammatical slot was already here and NOTHING IS INVENTED. Its line sits
            // physically between the two teams' lines, attached to neither, which is exactly what
            // the outcome is — S74 ruled the middle position is meaning rather than borrowed
            // convention, and in this layout it is meaning you can see.
            //
            // WHICH PRE-COMMITMENT FIRES, traced rather than assumed. `DrawOdds` is set by SLATE
            // GENERATION (SlateGenerator.cs:91) once the latents are known — the 1X2 triple cannot
            // be priced before the distributions exist — and by NEITHER Matchup constructor. So on
            // any generated board every matchup prices a draw and pre-commitment (1) fires: three
            // lines, uniform, closed with no further ruling. A hand-built matchup carries DrawOdds 0
            // and takes pre-commitment (2)'s empty line. THE BLOCK IS THREE LINES EITHER WAY — the
            // height is a design-time constant, never a response to what this matchup priced.
            //
            // THE MIDDLE IS NOW DERIVED, NOT WRITTEN (DD, DRAW-frame read 2026-08-15: −43 → −44.5).
            // S74 rules the middle position as MEANING — "the draw's line sits physically between
            // the two teams', attached to neither" — and at −43 it was not the middle: 35px below
            // AWAY and 38px above HOME, centred by intent and not by measurement. The frame read it.
            // `DrawCellY` is the midpoint of the two team cells and is COMPUTED from them, so the
            // claim the design makes is true by construction and cannot drift again if either team
            // line moves.
            if (matchup.DrawOdds > 1.0)
                LaptopUi.MakeButton(card, "DrawOdds", $"DRAW  {OddsFormat.American(matchup.DrawOdds)}",
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(462f, DrawCellY), new Vector2(112f, 32f), 19,
                    OfferField(OfferIsTakeable(slip, _host.director.Run, matchup.Index, MarketSelection.MoneylineDraw())), frozen ? LaptopUi.Dim(LaptopOs.Muted) : LaptopOs.White,
                    frozen || !OfferIsTakeable(slip, _host.director.Run, matchup.Index, MarketSelection.MoneylineDraw()) ? (System.Action)null
                    : () => { PickOffer(slip, matchup.Index, MarketSelection.MoneylineDraw()); }, _fontCond,
                    !frozen && OfferIsTakeable(slip, _host.director.Run, matchup.Index, MarketSelection.MoneylineDraw()), LaptopTrack.Names);
            // HOME sits one line pitch below the draw. It is the only thing that moved when the draw
            // landed: AWAY does not shift at all and the card's bottom slack is unchanged, which is
            // what makes the re-derived pitch above a pure insertion rather than a re-layout.
            LaptopUi.MakeButton(card, "HomeOdds", $"HOME  {OddsFormat.American(matchup.HomeOdds)}",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(462f, HomeCellY), new Vector2(112f, 32f), 19,
                OfferField(OfferIsTakeable(slip, _host.director.Run, matchup.Index, MarketSelection.Moneyline(Side.Home))), frozen ? LaptopUi.Dim(LaptopOs.Muted) : LaptopOs.White,
                frozen || !OfferIsTakeable(slip, _host.director.Run, matchup.Index, MarketSelection.Moneyline(Side.Home)) ? (System.Action)null
                    : () => { PickOffer(slip, matchup.Index, MarketSelection.Moneyline(Side.Home)); }, _fontCond,
                    !frozen && OfferIsTakeable(slip, _host.director.Run, matchup.Index, MarketSelection.Moneyline(Side.Home)), LaptopTrack.Names);
            if (awaySelected || homeSelected || drawSelected)
            {
                Sprite ring = ResolvePriceRing(matchup.Index);
                if (ring != null)
                {
                    // The price cell IS the odds button (112x32) — it is already wider than the
                    // 96x30 cell docs/design/direction-concepts/assets/ASSETS.md assumed, because the
                    // "AWAY  -341" label needs the room. Overshoot the ring 8px past every edge of
                    // the REAL cell so the pen stroke frames the price instead of crossing it.
                    const float overshoot = 8f;
                    // Three outcomes, three cell positions — and the ternary is written against the
                    // SAME constants the three MakeButton calls use, so a cell that moves cannot
                    // leave its ring behind at the old y. That divergence is the shape T95 caught on
                    // the TV: two elements agreeing by convention rather than by construction.
                    Vector2 cellPosition = new Vector2(462f,
                        awaySelected ? AwayCellY : drawSelected ? DrawCellY : HomeCellY);
                    Vector2 cellSize = new Vector2(112f, 32f);
                    RectTransform ink = LaptopUi.MakePanel(card, "BiroRing", new Vector2(0f, 1f), new Vector2(0f, 1f),
                        cellPosition + new Vector2(-overshoot, overshoot),
                        cellSize + new Vector2(overshoot * 2f, overshoot * 2f), LaptopOs.Accent);
                    Image image = ink.GetComponent<Image>();
                    image.sprite = ring;
                    image.type = Image.Type.Simple;
                    image.preserveAspect = false;
                    image.raycastTarget = false;
                }
            }
            // Button stays the Shapes-exact 74x44 (line 223 of DESIGN.md); the -14 offset (matching
            // the 14px padding used everywhere else on this row) reserves the full 78px More
            // column from the Lobby contract, with the button flush against its right edge.
            LaptopUi.MakeButton(card, "Details", "MORE ›", new Vector2(1f, .5f), new Vector2(1f, .5f),
                new Vector2(-14f, 0f), new Vector2(74f, 44f), 13, LaptopOs.Ink, LaptopOs.Muted,
                () => OpenDetail(matchup.Index), _font);
            LaptopUi.MakeRule(card, "EntryRule", new Vector2(0f, 0f), new Vector2(0f, 0f),
                Vector2.zero, new Vector2(700f, 1f));
        }

        /// <summary>
        /// One 30px line of a lobby entry: the team name in the condensed voice with its W-L record
        /// set 9px after it in the data voice, per the design system's FormEntry.line().
        ///
        /// The record's x comes from the name's own measured width rather than a column constant,
        /// because team names differ in length and the record has to stay attached to its name. Both
        /// are middle-aligned in the same 30px box so the 19px name and 13px record sit on a shared
        /// centre line — UGUI Text gives no baseline alignment, and centring is the closer read.
        /// </summary>
        private void TeamLine(RectTransform card, string side, string name, string record, float y)
        {
            const float nameX = 54f;
            const float gap = 9f;
            const float lineHeight = 30f;

            TMP_Text nameText = LaptopUi.MakeText(card, "Team" + side, new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(nameX, y), new Vector2(250f, lineHeight), 19,
                TextAnchor.MiddleLeft, LaptopOs.White, name, _fontCond, LaptopTrack.Names);
            // A long name must push its record along, never wrap onto a second line inside a 30px box.
            nameText.enableWordWrapping = false;

            LaptopUi.MakeText(card, "Record" + side, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(nameX + nameText.preferredWidth + gap, y), new Vector2(90f, lineHeight),
                // C15/S28: `.08` — FormEntry.jsx:27 sets --st-track-rec on the record beside the name.
                // Its x origin reads nameText.preferredWidth, which TMP now computes WITH the name's
                // own `.03`, so the record follows a wider name automatically rather than needing the
                // gap re-derived.
                13, TextAnchor.MiddleLeft, LaptopOs.Muted, record, _font, LaptopTrack.Records)
                .enableWordWrapping = false;
        }

        private void OpenDetail(int matchupIndex)
        {
            _detailMatchup = matchupIndex;
            _destination = MarketDestination.Result;
            _contentsOpen = false;
            _selectTab(Tab.Detail);
        }

        private static Sprite ResolvePriceRing(int matchupIndex)
            => ResolveInkSprite("ring-price-", matchupIndex);

        private static Sprite ResolveWideRing(int identity)
            => ResolveInkSprite("ring-wide-", identity);

        // Internal rather than private: OldSlipsApp (a sibling class in this file) reuses the same
        // deterministic strike lookup for the ledger's LOST-ticket treatment (Ruling S15) instead
        // of inventing a second strike mechanism.
        internal static Sprite ResolveStrike(int identity)
            => ResolveInkSprite("strike-", identity);

        private static Sprite ResolveInkSprite(string familyPrefix, int identity)
        {
            // Avoid a static Resources cache: a domain/import rebuild can touch this type before
            // the newly imported sprites are available. Family filtering prevents wide rings and
            // strikes from entering price-ring selection as the local asset set grows.
            Sprite[] imported = Resources.LoadAll<Sprite>("SureThing/Ink");
            if (imported == null || imported.Length == 0) return null;
            Sprite[] family = Array.FindAll(imported, sprite => sprite != null
                && sprite.name.StartsWith(familyPrefix, StringComparison.Ordinal));
            if (family.Length == 0) return null;
            Array.Sort(family, (left, right) => string.CompareOrdinal(left.name, right.name));
            return family[(identity & int.MaxValue) % family.Length];
        }

        private void BuildDetail(Run run, BetslipModel slip, bool boardFrozen)
        {
            if (_detailMatchup < 0 || _detailMatchup >= run.CurrentSlate.Matchups.Count)
            {
                _selectTab(Tab.Lobby);
                return;
            }

            Matchup matchup = run.CurrentSlate.Matchups[_detailMatchup];
            RectTransform panel = LaptopUi.MakePanel(_root, "EntryBoard", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -140f), new Vector2(700f, 530f), LaptopOs.Ink);
            LaptopUi.MakeButton(panel, "BackToForm", "← FORM", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -8f), new Vector2(104f, 32f), 13, LaptopOs.SurfaceRaised, LaptopOs.Accent,
                () => { _detailMatchup = -1; _selectTab(Tab.Lobby); }, _font);
            LaptopUi.MakeText(panel, "EventIdentity", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(132f, -6f), new Vector2(398f, 32f), 19, TextAnchor.UpperLeft, LaptopOs.White,
                $"{LaptopUi.TeamShort(matchup.Away)}  @  {LaptopUi.TeamShort(matchup.Home)}", _fontCond);
            LaptopUi.MakeText(panel, "EventRecords", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-14f, -7f), new Vector2(150f, 32f), 13, TextAnchor.UpperRight, LaptopOs.TonerSecondary,
                $"{matchup.Away.Record}   ·   {matchup.Home.Record}", _font);

            LaptopUi.MakeText(panel, "EventForm", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -42f), new Vector2(670f, 32f), 13, TextAnchor.UpperLeft, LaptopOs.Muted,
                $"FORM  {matchup.Away.Name}: GF {matchup.AwayStats.GoalsFor:0.0}  COR {matchup.AwayStats.Corners:0.0}  CRD {matchup.AwayStats.Cards:0.0}    " +
                    $"{matchup.Home.Name}: GF {matchup.HomeStats.GoalsFor:0.0}  COR {matchup.HomeStats.Corners:0.0}  CRD {matchup.HomeStats.Cards:0.0}", _font);
            LaptopUi.MakeRule(panel, "EventRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -74f), new Vector2(700f, 2f));

            // §3: the sheet is DERIVED once for the whole matchup, not per destination. The folio's
            // denominator and the contents block's line ranges are two readings of that ONE
            // numbering, which is the only reason they cannot disagree with each other or with the
            // rows printed below (MarketSheet's own class comment, S74-am3).
            MarketSheet sheet = MarketSheet.Build(matchup);
            MarketSheetSection section = sheet.Section(_destination);

            RectTransform destinations = LaptopUi.MakePanel(panel, "MarketDestinations",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -76f),
                new Vector2(EntryBoardWidth, RailBandHeight), LaptopOs.Surface);
            BuildDestinationRail(destinations);

            RectTransform body = LaptopUi.MakePanel(panel, "MarketBody", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, -EntryHeaderHeight),
                new Vector2(EntryBoardWidth, MarketBodyHeight), LaptopOs.Ink);

            // Built BEFORE the body so the folio's text object exists for BuildMarketSheet to bind
            // to the live scroll position. The band itself is below the body on the page.
            TMP_Text folio = BuildFolioBand(panel);

            // A2 ruling: the per-destination panel title ("GOALS TOTAL" etc.) is deleted — each row
            // now names its own market and the rail already names the destination; the kit has no
            // such heading. §5.3's GROUP headings are a different thing: they are the market's own
            // name and count inside the destination, and they print even when empty.
            BuildMarketSheet(body, slip, matchup, sheet, section, boardFrozen, run, folio);

            // Drawn last (after the market body's scroll content) so it always renders on top of
            // row 0 instead of being hidden behind that row's opaque price-cell button. Under the
            // old fixed layout this banner floated clear of the offer rows because a title row
            // reserved the first 48px; A2 deleted that title, so the list now begins flush with the
            // body's top edge and this has to out-order it instead.
            if (boardFrozen)
                LaptopUi.MakeText(body, "LockedMarketReason", new Vector2(1f, 1f), new Vector2(1f, 1f),
                    new Vector2(-14f, -8f), new Vector2(280f, 32f), 13, TextAnchor.UpperRight,
                    LaptopOs.MoneyBad, "ROUND LOCKED — WATCH MY BETS", _font);

            // §5.2: a page laid OVER the sheet, and therefore built after it. It is not a second
            // navigation tier — the rail stays one level — it is a printed contents list you read.
            if (_contentsOpen) BuildContentsBlock(panel, sheet);

            BuildSlip(run, slip, boardFrozen);
        }

        // ── §3 / §3.1 · THE DESTINATION RAIL ────────────────────────────────────────────────────
        //
        // The rail is a CONSTANT: all six destinations, always, in MarketDestinations.All order,
        // whatever happens to be priced (§3.1 — which only holds because §5.3 prints empty groups).
        //
        // What is NOT a constant is where each stop lands. That is MEASURED from the labels
        // themselves and packed left to right at a fixed gutter. The five hand-typed x-offsets this
        // replaced (14/118/222/338/442, with a width of 96f or 108f chosen by comparing the label
        // against two string literals) are exactly what S74-am3 names: a constant that happened to
        // equal the right answer for five short labels, and stopped equalling it the moment
        // CORRECT SCORE — 137px of type against a 108px box — joined the strip.

        /// <summary>ENTRY's board width. The market column is 700px, NOT the full 1024: the working
        /// margin (BuildSlip, 324px) is anchored to the right of the same screen. Named here because
        /// the rail's fit is decided against this number and nothing else.</summary>
        internal const float EntryBoardWidth = 700f;

        /// <summary>The rail band's own height; its tabs are centred in it.</summary>
        internal const float RailBandHeight = 42f;

        /// <summary>The page margin every band on this board already uses (back button, event form,
        /// offer rows, working margin). The rail is packed inside it on both sides.</summary>
        internal const float RailPageMargin = 14f;

        /// <summary>The gap between two destination tabs — read off the strip this replaced, where
        /// every one of the four gaps measured exactly 8px.</summary>
        internal const float RailGutter = 8f;

        /// <summary>Per-side padding between a tab's label and its own box.
        ///
        /// <para><b>Measured, and the measurement is the whole finding.</b> The strip this replaced
        /// ran its tightest box at 15.08px per side (CORNERS: a 108f box around 77.84px of type).
        /// Against the REAL 700px column the ceiling for six destinations is 14.26px per side — so
        /// the old strip's own padding grammar, carried forward unexamined, would have overflowed
        /// the rail by about 10px. 12f is inside the ceiling with room, and
        /// <see cref="PackDestinationRail"/> asserts the fit rather than trusting this note.</para></summary>
        internal const float RailTabPadX = 12f;

        internal const float RailTabHeight = 32f;

        /// <summary>§4.5: the 13px product-fact floor is law and type does not shrink. The rail is
        /// sized at the floor, so a rail that does not fit cannot be made to fit here.</summary>
        internal const int RailLabelSize = 13;

        /// <summary>MakeButton's own default. Named rather than left implicit because
        /// <see cref="LaptopUi.MeasureWidth"/> must be handed the same number the label renders
        /// with or the pack measures narrow (see MeasureWidth's own comment).</summary>
        internal const float RailLabelTracking = LaptopTrack.Actions;

        /// <summary>§3's rail, packed: one measured label width, one box and one x per destination,
        /// plus the total the fit is judged against. Every field is DERIVED — nothing in here is a
        /// number anybody typed.</summary>
        internal sealed class DestinationRailPack
        {
            public float[] LabelWidth;
            public float[] TabWidth;
            public float[] TabX;

            /// <summary>The width the rail has to play with (ENTRY's 700px column).</summary>
            public float RailWidth;

            /// <summary>Left margin + every tab + every gutter + right margin.</summary>
            public float PackedWidth;

            public float Slack => RailWidth - PackedWidth;

            public bool Fits => PackedWidth <= RailWidth + 0.001f;

            /// <summary>Every measured number, for the gate's failure message and for a report. A
            /// rail that does not fit is a Design Director call, and a DD cannot make it without
            /// the numbers.</summary>
            public string Report()
            {
                var text = new System.Text.StringBuilder();
                text.Append("rail ").Append(Fmt(RailWidth)).Append("px · packed ").Append(Fmt(PackedWidth))
                    .Append("px · slack ").Append(Fmt(Slack)).Append("px · margins ")
                    .Append(Fmt(RailPageMargin)).Append("px · gutter ").Append(Fmt(RailGutter))
                    .Append("px · pad ").Append(Fmt(RailTabPadX)).Append("px/side · ")
                    .Append(RailLabelSize).Append("px");
                IReadOnlyList<MarketDestination> all = MarketDestinations.All;
                for (int i = 0; i < all.Count; i++)
                    text.Append(" | ").Append(MarketDestinations.Label(all[i])).Append(" label ")
                        .Append(Fmt(LabelWidth[i])).Append(" box ").Append(Fmt(TabWidth[i]))
                        .Append(" at x=").Append(Fmt(TabX[i]));
                return text.ToString();
            }

            private static string Fmt(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Measures every destination label and packs the rail left to right. Pure arithmetic over
        /// <see cref="LaptopUi.MeasureWidth"/> — no rendering, nothing authored — so the gate and
        /// the builder read the identical pack and cannot disagree about whether it fits.
        /// </summary>
        internal static DestinationRailPack PackDestinationRail(TMP_FontAsset font, float railWidth)
        {
            IReadOnlyList<MarketDestination> all = MarketDestinations.All;
            var pack = new DestinationRailPack
            {
                LabelWidth = new float[all.Count],
                TabWidth = new float[all.Count],
                TabX = new float[all.Count],
                RailWidth = railWidth,
            };

            float x = RailPageMargin;
            for (int i = 0; i < all.Count; i++)
            {
                float labelWidth = LaptopUi.MeasureWidth(font, MarketDestinations.Label(all[i]),
                    RailLabelSize, RailLabelTracking);
                // MakeButton's own 44px hit-target floor is applied HERE as well, so the pack's
                // arithmetic is the arithmetic the built button actually gets rather than an
                // optimistic version of it that a short label would quietly break.
                float tabWidth = Mathf.Max(44f, labelWidth + RailTabPadX * 2f);
                pack.LabelWidth[i] = labelWidth;
                pack.TabWidth[i] = tabWidth;
                pack.TabX[i] = x;
                x += tabWidth + RailGutter;
            }

            pack.PackedWidth = x - RailGutter + RailPageMargin;
            return pack;
        }

        /// <summary>
        /// §7's C51 assertion for the rail: it fits, or the build fails with the numbers.
        ///
        /// <para><b>Deliberately a throw and deliberately not a repair.</b> Every way of making an
        /// over-long rail "fit" is a design call this lane does not hold: shrinking the type breaks
        /// §4.5's 13px floor, abbreviating CORRECT SCORE changes a ruled label, a second tier is
        /// forbidden outright by §5.2, and scrolling the rail destroys §3.1's never-reflows
        /// property. So it fails loudly with every measured width instead.</para>
        /// </summary>
        internal static DestinationRailPack RequireDestinationRailFits(TMP_FontAsset font, float railWidth)
        {
            DestinationRailPack pack = PackDestinationRail(font, railWidth);
            if (!pack.Fits)
                throw new InvalidOperationException(
                    "spec-market-surfaces-2026-08-17.md §9 — THE DESTINATION RAIL DOES NOT FIT: "
                    + pack.Report()
                    + ". Type does not shrink (§4.5), a second rail tier is forbidden (§5.2) and the "
                    + "rail must never reflow (§3.1) — this is a Design Director ruling, not a "
                    + "layout fix.");
            return pack;
        }

        private void BuildDestinationRail(RectTransform band)
        {
            DestinationRailPack pack = RequireDestinationRailFits(_font, band.rect.width);
            IReadOnlyList<MarketDestination> all = MarketDestinations.All;
            float tabY = -(RailBandHeight - RailTabHeight) / 2f;

            for (int i = 0; i < all.Count; i++)
            {
                MarketDestination destination = all[i];
                bool active = _destination == destination;
                // Named by the ENUM MEMBER, not the printed label: "CORRECT SCORE" carries a space,
                // and a scene-graph name is an identity rather than a caption.
                LaptopUi.MakeButton(band, "DetailTab" + destination, MarketDestinations.Label(destination),
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(pack.TabX[i], tabY),
                    new Vector2(pack.TabWidth[i], RailTabHeight), RailLabelSize,
                    active ? LaptopOs.Ink : LaptopOs.Surface,
                    active ? LaptopOs.White : LaptopOs.TonerSecondary,
                    () => { _destination = destination; _contentsOpen = false; _invalidate(); },
                    _font, true, RailLabelTracking);
            }
        }

        // ── §5 · THE BODY, THE FOLIO AND THE CONTENTS BLOCK ─────────────────────────────────────

        /// <summary>ENTRY's fixed header band measured from the board's top edge — back button,
        /// identity, records, form, the 2px rule and §3's destination rail. The market body starts
        /// immediately below it.</summary>
        private const float EntryHeaderHeight = 118f;

        /// <summary>§5.1's folio band along the bottom of the board: the printed position fact and
        /// §5.2's contents control. 34px because MakeButton floors a control at 32px tall and this
        /// band carries one.</summary>
        private const float FolioBandHeight = 34f;

        /// <summary>What is left of the 530px board for the market list — 378px, which is exactly
        /// seven 54px rows. DERIVED from the two bands above so moving either one can never silently
        /// overlap the body or leave a strip of ground showing.</summary>
        private const float MarketBodyHeight = 530f - EntryHeaderHeight - FolioBandHeight;

        /// <summary>§5.3 / S89's group heading band — the market's own name, leaders, and either its
        /// count or `no prices offered`.</summary>
        private const float GroupHeadingHeight = 26f;

        /// <summary>
        /// §5.2/§5.3's sheet: every group the destination holds, in order, each as a printed heading
        /// followed by its rows — <b>and the empty ones print too</b>. A racecard prints the race
        /// even when it is abandoned, and it is that rule which makes §3.1's rail a constant.
        ///
        /// <para>§5.4: it scrolls, and it is NOT virtualised. Every row is a real row, because a
        /// folio reading "46 of 80" has to be backed by eighty of them.</para>
        /// </summary>
        private void BuildMarketSheet(RectTransform body, BetslipModel slip, Matchup matchup,
            MarketSheet sheet, MarketSheetSection section, bool frozen, Run run, TMP_Text folio)
        {
            float rowsHeight = 0f;
            foreach (MarketSheetGroup group in section.Groups)
                rowsHeight += GroupHeadingHeight + group.Count * OfferRowHeight;

            RectTransform content = BuildScrollingBody(body, rowsHeight, run, out float rowWidth,
                out float rowsOffsetY, out ScrollRect scrollRect, out float viewportHeight,
                out float contentHeight);

            // The folio is derived from THESE two lists — the rows that were actually built and the
            // global line each one carries — never from a count or an estimate. §5.1: a folio that
            // lies is worse than no folio.
            var rowTop = new List<float>(section.Count);
            var rowLine = new List<int>(section.Count);

            float y = rowsOffsetY;
            int offerIndex = 0;
            foreach (MarketSheetGroup group in section.Groups)
            {
                MakeGroupHeading(content, group, y, rowWidth);
                y += GroupHeadingHeight;
                foreach (MarketSheetRow row in group.Rows)
                {
                    // E-07: rows sit below rowsOffsetY, the height BuildScrollingBody's own
                    // "PLACED THIS ROUND" block (if any) already consumed at the top of content.
                    MakeOfferRow(content, slip, matchup, row.Offer.Selection, row.Name, row.Role,
                        offerIndex, -y, rowWidth, frozen);
                    rowTop.Add(y);
                    rowLine.Add(row.Line);
                    y += OfferRowHeight;
                    offerIndex++;
                }
            }

            BindFolio(folio, sheet, scrollRect, rowTop, rowLine, viewportHeight, contentHeight);
        }

        /// <summary>§5.3 / S89's heading form: the market's name on the left, its count on the
        /// right, leaders between them — and `no prices offered` in the count's slot when the group
        /// holds nothing. Both strings come from <see cref="MarketSheetGroup"/>; neither is composed
        /// here, so the heading and the contents block cannot word the same fact differently.</summary>
        private void MakeGroupHeading(RectTransform content, MarketSheetGroup group, float y, float rowWidth)
        {
            const float pad = 14f;
            RectTransform head = LaptopUi.MakePanel(content, "MarketGroup" + group.Kind,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -y),
                new Vector2(rowWidth, GroupHeadingHeight), new Color(0f, 0f, 0f, 0f));

            TMP_Text label = LaptopUi.MakeText(head, "GroupLabel", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(pad, 0f),
                new Vector2(rowWidth - pad * 2f, GroupHeadingHeight), 13, TextAnchor.MiddleLeft,
                LaptopOs.TonerSecondary, group.Label, _font, LaptopTrack.FieldKeys);
            label.enableWordWrapping = false;

            string count = group.CountText;
            float countWidth = LaptopUi.MeasureWidth(_font, count, 13, LaptopTrack.FieldKeys);
            LaptopUi.MakeText(head, "GroupCount", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-pad, 0f), new Vector2(countWidth + 2f, GroupHeadingHeight), 13,
                TextAnchor.MiddleRight, group.IsEmpty ? LaptopOs.Muted : LaptopOs.TonerSecondary,
                count, _font, LaptopTrack.FieldKeys)
                .enableWordWrapping = false;

            // S89 already puts leader dots in the product ("CORNERS ..... 11"); §4.3 is that same
            // device one level down, so the heading wears it too.
            MakeLeaders(head, "GroupLeaders", pad + label.preferredWidth, rowWidth - pad - countWidth,
                GroupHeadingHeight);

            // The stronger --rule token, not the row rule's --rule-soft: this is a seam between
            // document bands (a market ends, another begins) rather than a rule inside one.
            LaptopUi.MakeRule(head, "GroupRule", new Vector2(0f, 0f), new Vector2(0f, 0f),
                Vector2.zero, new Vector2(rowWidth, 1f), LaptopOs.Rule);
        }

        /// <summary>The leader run's own rhythm, in em. NOT one of §4.3's type-tracking tokens and
        /// deliberately not bent to the nearest one — a leader is a RULE made of dots, not a label,
        /// and this value is what separates the two on the page (Archivo's period advances 3.9px at
        /// 13px, which sets solid; this opens it to a 6.5px step). Named for the same reason
        /// LaptopTrack.Chrome is: a named exception with one member is still named. MeasureWidth is
        /// handed this exact number, which is the only reason the dot count can be derived.</summary>
        private const float LeaderTracking = 0.2f;

        /// <summary>
        /// §4.3's leader dots — the device that makes an offer row ONE statement instead of two
        /// facts at opposite ends of a gap that measures 188–461px on the real 700px column.
        ///
        /// <para><b>Everything here is measured.</b> The run starts after the name's own rendered
        /// width and stops before the figure's own left edge, and the dot COUNT is derived from the
        /// span that is actually left over. So it works at any row width — the 700px a fitting
        /// destination gives it and the 692px a scrolling one does — and when the span will not hold
        /// a single dot inside its clearances, nothing is drawn at all rather than something that
        /// collides with the type at either end.</para>
        ///
        /// <para>Set right-aligned, because leaders on a racecard arrive AT the number.</para>
        /// </summary>
        private void MakeLeaders(RectTransform parent, string name, float fromX, float toX, float height)
        {
            // 10px, not a hairline: the price cell's selection ring overshoots its own cell by 8px
            // to the left (MakeOfferRow), so a smaller clearance would let the last dot sit under
            // the ring's shoulder on exactly the rows the player has marked.
            const float clearance = 10f;
            const int size = 13;

            float left = fromX + clearance;
            float span = (toX - clearance) - left;
            if (span <= 0f) return;

            float unit = LaptopUi.MeasureWidth(_font, ".", size, LeaderTracking);
            if (unit <= 0f || span < unit) return;
            int count = Mathf.FloorToInt(span / unit);
            if (count <= 0) return;

            LaptopUi.MakeText(parent, name, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(left, 0f), new Vector2(span, height), size, TextAnchor.MiddleRight,
                LaptopOs.RuleSoft, new string('.', count), _font, LeaderTracking)
                .enableWordWrapping = false;
        }

        /// <summary>
        /// §5.1's folio — "46–66 of 80" — printed beside the position rail, not instead of it, and
        /// recomputed on every scroll.
        ///
        /// <para><b>Nothing here is authored.</b> The window is the set of rows whose rects actually
        /// intersect the viewport at the live scroll position; the numbers are those rows' own
        /// global line numbers; the total is the sheet's own row count, which
        /// <see cref="MarketSheet.Folio"/> refuses to accept from a caller. When no offer row is in
        /// view at all — a section that prices nothing, or a scroll position showing only the staged
        /// receipts above the list — the folio prints NOTHING rather than a number it cannot stand
        /// behind. A page with no lines on it has no folio.</para>
        /// </summary>
        private static void BindFolio(TMP_Text folio, MarketSheet sheet, ScrollRect scrollRect,
            List<float> rowTop, List<int> rowLine, float viewportHeight, float contentHeight)
        {
            if (folio == null) return;
            float travel = Mathf.Max(0f, contentHeight - viewportHeight);

            void Print(float normalizedY)
            {
                // Guarded rather than trusted: a ScrollRect whose content fits reports a normalized
                // position that is not meaningful (and can be NaN), and travel is 0 in exactly that
                // case — so the whole list is visible and the window starts at the top.
                float top = travel <= 0f ? 0f : travel * (1f - Mathf.Clamp01(normalizedY));
                float bottom = top + viewportHeight;
                int first = 0;
                int last = 0;
                for (int i = 0; i < rowTop.Count; i++)
                {
                    if (rowTop[i] >= bottom) break;
                    if (rowTop[i] + OfferRowHeight <= top) continue;
                    if (first == 0) first = rowLine[i];
                    last = rowLine[i];
                }
                folio.text = first == 0 ? string.Empty : sheet.Folio(first, last);
            }

            Print(1f);
            if (scrollRect != null && travel > 0f)
                scrollRect.onValueChanged.AddListener(value => Print(value.y));
        }

        /// <summary>§5.1/§5.2's band under the market body: the contents control on the left, the
        /// folio on the right. Returns the folio's own text object so
        /// <see cref="BuildMarketSheet"/> can bind it to the list it just printed — the folio is
        /// never given a string here, because this method does not know what is on screen.</summary>
        private TMP_Text BuildFolioBand(RectTransform panel)
        {
            RectTransform band = LaptopUi.MakePanel(panel, "FolioBand", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, -(EntryHeaderHeight + MarketBodyHeight)),
                new Vector2(EntryBoardWidth, FolioBandHeight), LaptopOs.Surface);
            LaptopUi.MakeRule(band, "FolioRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(EntryBoardWidth, 1f), LaptopOs.Rule);

            // §6: this stands IN PLACE OF a search field, which the spec refuses outright — a web
            // register is foreign to ruled paper. The label states which way it acts.
            string label = _contentsOpen ? "CLOSE CONTENTS" : "CONTENTS";
            float width = Mathf.Max(44f,
                LaptopUi.MeasureWidth(_font, label, RailLabelSize, RailLabelTracking) + RailTabPadX * 2f);
            LaptopUi.MakeButton(band, "ContentsToggle", label, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(RailPageMargin, -(FolioBandHeight - RailTabHeight) / 2f),
                new Vector2(width, RailTabHeight), RailLabelSize,
                _contentsOpen ? LaptopOs.Ink : LaptopOs.Surface,
                _contentsOpen ? LaptopOs.White : LaptopOs.TonerSecondary,
                () => { _contentsOpen = !_contentsOpen; _invalidate(); }, _font);

            return LaptopUi.MakeText(band, "Folio", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-RailPageMargin, 0f), new Vector2(260f, FolioBandHeight), 13,
                TextAnchor.MiddleRight, LaptopOs.TonerSecondary, string.Empty, _font,
                LaptopTrack.FieldKeys);
        }

        private const float ContentsDestinationHeight = 24f;
        private const float ContentsKindHeight = 20f;

        /// <summary>
        /// §5.2's printed contents block: the destination AND the markets inside it, each with the
        /// line range it actually occupies. This is the move that makes §3's grouping safe — every
        /// market is named here whichever destination holds it, so BTTS living inside GOALS costs
        /// the player nothing.
        ///
        /// <para><b>It is not the double-tiered rail.</b> The rail stays ONE level; this is a page
        /// you read, and that distinction is available only because we are made of paper. Worst-case
        /// navigation stays three interactions: contents → destination → row.</para>
        ///
        /// <para>Every range comes from <c>RangeText</c> on the section or group itself, so the
        /// contents cannot disagree with the page — §7's gate 4.</para>
        /// </summary>
        private void BuildContentsBlock(RectTransform panel, MarketSheet sheet)
        {
            RectTransform overlay = LaptopUi.MakePanel(panel, "ContentsBlock", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, -EntryHeaderHeight),
                new Vector2(EntryBoardWidth, MarketBodyHeight), LaptopOs.Ink);
            // Opaque AND raycastable: the sheet is still built underneath, and a page laid over it
            // has to take the clicks rather than let them fall through to rows it is hiding.
            overlay.GetComponent<Image>().raycastTarget = true;

            const float headHeight = 26f;
            LaptopUi.MakeText(overlay, "ContentsTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(RailPageMargin, 0f), new Vector2(420f, headHeight), 13,
                TextAnchor.MiddleLeft, LaptopOs.Muted, "CONTENTS", _font, LaptopTrack.FieldKeys);
            // The contents covers the whole sheet, so its own folio is the whole sheet — derived
            // through the same Folio() the band below uses, never composed here. Guarded because
            // Folio refuses an empty sheet outright rather than printing a folio for no page.
            LaptopUi.MakeText(overlay, "ContentsTotal", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-RailPageMargin, 0f), new Vector2(260f, headHeight), 13,
                TextAnchor.MiddleRight, LaptopOs.Muted,
                sheet.TotalRows > 0 ? sheet.Folio(1, sheet.TotalRows) : MarketSheet.NoPricesOffered,
                _font, LaptopTrack.FieldKeys);
            LaptopUi.MakeRule(overlay, "ContentsTitleRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -headHeight), new Vector2(EntryBoardWidth, 1f), LaptopOs.Rule);

            float listHeight = MarketBodyHeight - headHeight;
            RectTransform content = LaptopUi.MakeScrollBody(overlay, "ContentsScroll",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -headHeight),
                new Vector2(EntryBoardWidth, listHeight), out RectTransform host,
                out ScrollRect scrollRect);

            float width = EntryBoardWidth - LaptopUi.RailReserve;
            float y = 0f;
            foreach (MarketSheetSection section in sheet.Sections)
            {
                MakeContentsLine(content, "ContentsDestination" + section.Destination, section.Label,
                    section.RangeText, RailPageMargin, y, width, ContentsDestinationHeight,
                    LaptopOs.White, section.Destination);
                y += ContentsDestinationHeight;
                foreach (MarketSheetGroup group in section.Groups)
                {
                    MakeContentsLine(content, "ContentsKind" + group.Kind, group.Label,
                        group.RangeText, RailPageMargin + 26f, y, width, ContentsKindHeight,
                        group.IsEmpty ? LaptopOs.Muted : LaptopOs.TonerSecondary, null);
                    y += ContentsKindHeight;
                }
            }

            LaptopUi.FinishScrollBody(host, scrollRect, content, y, listHeight);
        }

        private void MakeContentsLine(RectTransform parent, string name, string label, string range,
            float indent, float y, float width, float height, Color ink, MarketDestination? destination)
        {
            const float pad = 14f;
            RectTransform line = LaptopUi.MakePanel(parent, name, new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, -y), new Vector2(width, height),
                new Color(0f, 0f, 0f, 0f));

            if (destination.HasValue)
            {
                // The destination lines are the ONE interaction the contents block offers, and they
                // are what keeps §5.2's worst case at three. A Button is added to the line's own
                // rect rather than built through MakeButton because that helper floors a control at
                // 32px tall, and a contents line is a printed line — not a control the size of one.
                Image field = line.GetComponent<Image>();
                field.raycastTarget = true;
                Button button = line.gameObject.AddComponent<Button>();
                button.targetGraphic = field;
                MarketDestination target = destination.Value;
                button.onClick.AddListener(() =>
                {
                    _destination = target;
                    _contentsOpen = false;
                    _invalidate();
                });
            }

            TMP_Text text = LaptopUi.MakeText(line, "ContentsLabel", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(indent, 0f),
                new Vector2(Mathf.Max(0f, width - indent - pad), height), 13, TextAnchor.MiddleLeft,
                ink, label, _font, LaptopTrack.FieldKeys);
            text.enableWordWrapping = false;

            float rangeWidth = LaptopUi.MeasureWidth(_font, range, 13, LaptopTrack.FieldKeys);
            LaptopUi.MakeText(line, "ContentsRange", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-pad, 0f), new Vector2(rangeWidth + 2f, height), 13,
                TextAnchor.MiddleRight, ink, range, _font, LaptopTrack.FieldKeys)
                .enableWordWrapping = false;

            MakeLeaders(line, "ContentsLeaders", indent + text.preferredWidth,
                width - pad - rangeWidth, height);
        }

        /// <summary>A4/S27: builds the market body's scroll plumbing — a masked <see cref="ScrollRect"/>
        /// viewport sized to <paramref name="body"/>'s own rect (kept fixed per the ruling) and a
        /// content <see cref="RectTransform"/> whose height is the true content height
        /// (<paramref name="rowCount"/> × <paramref name="rowHeight"/>, plus the staged-receipt
        /// block below), never a capacity clamp. Draws the S27 position rail — present only when the
        /// content overflows the viewport, absent when it fits — and returns the row width every row
        /// must use so content never runs under the rail (out <paramref name="rowWidth"/>). Shared by
        /// BuildMarketLines, BuildBothTeamsScore and BuildPlayerLines (A1 ruling) so their scroll/rail
        /// plumbing can never independently drift.
        ///
        /// <paramref name="rowHeight"/> defaults to <see cref="OfferRowHeight"/> so those three
        /// existing callers are unchanged in behaviour. Allen ruling (2026-08-03): BuildLobby is now
        /// a fourth caller, passing its own card pitch (<see cref="MatchupCardPitch"/>, 116px since
        /// S74-am put a third line in the block — named by the constant here, never by a literal) — the
        /// lobby's staged-receipt slack problem is solved by reusing this one scrolling-body
        /// implementation rather than forking a second one, so its row geometry has to be a
        /// parameter instead of the ENTRY-only constant this used to hardcode.
        ///
        /// E-07 ruling: also renders <c>run.Tickets</c>' staged receipts under a "PLACED THIS ROUND"
        /// header (kit: screens.jsx:49-58) at the top of the content, inside the same scroll — done
        /// once here rather than in each caller, since every caller (five ENTRY destinations plus,
        /// per the ruling above, FORM) shares this one path. <paramref name="rowsOffsetY"/> is the
        /// height that block consumed (0 when <c>run.Tickets</c> is empty); every caller must push
        /// its own rows down by this before they contribute to their own row-height math, and it is
        /// already folded into <paramref name="rowWidth"/>'s contentHeight/overflow decision below so
        /// the rail and mask both size off the true total.</summary>
        private RectTransform BuildScrollingBody(RectTransform body, int rowCount, Run run,
            out float rowWidth, out float rowsOffsetY, float rowHeight = OfferRowHeight)
            => BuildScrollingBody(body, rowCount * rowHeight, run, out rowWidth, out rowsOffsetY,
                out ScrollRect _, out float _, out float _);

        /// <summary>The same body, opened against a MEASURED total row height rather than a uniform
        /// row count — which is what §5.3's market sheet needs, because its content is group headings
        /// interleaved with rows and no single pitch describes it. Also hands back the plumbing
        /// §5.1's folio has to read: the ScrollRect it tracks, and the two heights whose difference
        /// is the scroll travel. The row-count form above is this one with the multiplication done
        /// for it, so the two can never size a body differently.</summary>
        private RectTransform BuildScrollingBody(RectTransform body, float rowsHeight, Run run,
            out float rowWidth, out float rowsOffsetY, out ScrollRect scrollRect,
            out float viewportHeight, out float contentHeight)
        {
            const float railReserve = ScrollRailReserve;
            float bodyWidth = body.rect.width;
            float bodyHeight = body.rect.height;
            viewportHeight = bodyHeight;
            rowsOffsetY = MeasurePlacedThisRoundHeight(run);
            contentHeight = rowsOffsetY + rowsHeight;
            bool overflows = contentHeight > bodyHeight;
            rowWidth = overflows ? bodyWidth - railReserve : bodyWidth;

            RectTransform scroll = LaptopUi.MakePanel(body, "MarketScroll", new Vector2(0f, 1f),
                new Vector2(0f, 1f), Vector2.zero, new Vector2(bodyWidth, bodyHeight),
                new Color(0f, 0f, 0f, 0f));
            scrollRect = scroll.gameObject.AddComponent<ScrollRect>();

            RectTransform viewport = LaptopUi.MakePanel(scroll, "MarketViewport", new Vector2(0f, 1f),
                new Vector2(0f, 1f), Vector2.zero, new Vector2(bodyWidth, bodyHeight),
                new Color(0f, 0f, 0f, 0f));
            // Same lightweight, Graphic-free mask already used on this stack (PhoneScreen.cs's
            // _threadRoot) rather than a new masking mechanism.
            viewport.gameObject.AddComponent<RectMask2D>();
            // MakePanel defaults every Image to raycastTarget=false (decorative by default
            // everywhere else in this file). A ScrollRect needs the opposite: EventSystem only
            // routes wheel/drag to a handler by first hitting a raycastable Graphic and bubbling up
            // from there, so without this, wheel/drag over the empty space between rows — most of
            // the body — would never reach the ScrollRect at all. Row buttons stay on top and keep
            // receiving clicks as normal; this only fills in the gaps between them.
            viewport.GetComponent<Image>().raycastTarget = true;

            RectTransform content = LaptopUi.MakePanel(viewport, "MarketContent", new Vector2(0f, 1f),
                new Vector2(0f, 1f), Vector2.zero, new Vector2(rowWidth, contentHeight),
                new Color(0f, 0f, 0f, 0f));

            if (run.Tickets.Count > 0)
                BuildPlacedThisRound(content, run, rowWidth);

            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = false;
            scrollRect.viewport = viewport;
            scrollRect.content = content;

            // Rebuild-safety (A4): scrollRect.verticalNormalizedPosition is only ever read inside
            // BuildPositionRail, and BuildPositionRail is only ever called in this branch — so a
            // content height <= viewport height (the degenerate 0/0 case) never risks a NaN or a
            // divide-by-zero; that branch simply never touches it.
            if (overflows)
                BuildPositionRail(body, scrollRect, bodyHeight, contentHeight);

            return content;
        }

        /// <summary>13px header height and the kit's own 7px gap before the first receipt
        /// (screens.jsx:52, <c>marginBottom: 7</c>) — the two pieces of the "PLACED THIS ROUND"
        /// block that sit above <see cref="BuildStagedReceipt"/>'s own per-ticket geometry.</summary>
        private const float PlacedHeaderHeight = 18f;
        private const float PlacedHeaderGap = 7f;

        /// <summary>Total height of the staged-receipt block (header + gap + every ticket), or 0
        /// when <c>run.Tickets</c> is empty — the number <see cref="BuildScrollingBody"/> reserves in
        /// its content height and every row must be pushed down by. Pure measurement, no rendering,
        /// so it can run before <see cref="BuildScrollingBody"/> decides rowWidth/overflow, which
        /// <see cref="BuildPlacedThisRound"/>'s own rendering then depends on.</summary>
        private static float MeasurePlacedThisRoundHeight(Run run)
        {
            if (run.Tickets.Count == 0) return 0f;
            return PlacedHeaderHeight + PlacedHeaderGap + MeasureStagedTicketsHeight(run);
        }

        /// <summary>Sum of every staged ticket's own rendered height (30px header + 18px per leg),
        /// each followed by an 8px gap (kit: TicketReceipt's <c>marginBottom: 8</c>). Factored out of
        /// <see cref="BuildStagedReceipt"/> so this and <see cref="MeasurePlacedThisRoundHeight"/>
        /// can never independently drift from what actually renders.</summary>
        private static float MeasureStagedTicketsHeight(Run run)
        {
            float totalHeight = 0f;
            for (int i = 0; i < run.Tickets.Count; i++)
                // S70(3): reads the same source the receipt builds from. This line restated the
                // formula, so the footer would have grown the receipts and left the scroll content
                // short by 36px per ticket — clipping the last one with every test green.
                totalHeight += StagedReceiptHeight(run.Tickets[i]) + 8f;
            return totalHeight;
        }

        /// <summary>E-07 ruling: the "PLACED THIS ROUND" key header (kit: screens.jsx:52 — roman,
        /// fact floor, <see cref="LaptopOs.Muted"/>) above <see cref="BuildStagedReceipt"/>'s own
        /// receipt stack, both inside <paramref name="content"/> (A4's MarketContent) so they scroll
        /// with the market list instead of pinning over it. Only ever called when
        /// <c>run.Tickets.Count &gt; 0</c> (<see cref="BuildScrollingBody"/>).</summary>
        private void BuildPlacedThisRound(RectTransform content, Run run, float width)
        {
            const float pad = 14f;
            LaptopUi.MakeText(content, "PlacedThisRoundHeader", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(pad, 0f), new Vector2(width - pad * 2f, PlacedHeaderHeight), 13,
                TextAnchor.UpperLeft, LaptopOs.Muted, "PLACED THIS ROUND", _font);
            BuildStagedReceipt(content, run, -(PlacedHeaderHeight + PlacedHeaderGap), width);
        }

        /// <summary>S27 ruling: the printed position rail for a scrolling interior list — exactly
        /// two <see cref="Image"/>s, never a Unity <see cref="Scrollbar"/> (no drag handle, no
        /// glyph; never fades, auto-hides or overlays content). The track is
        /// <see cref="LaptopOs.RuleSoft"/>, full body height, flush with the body's right edge; the
        /// thumb is <see cref="LaptopOs.Muted"/>, sized to the visible fraction with a 24px floor
        /// (and never taller than the track), and tracks the ScrollRect's live normalized position
        /// via <c>onValueChanged</c> so a wheel/drag scroll updates it without a full canvas
        /// rebuild. Only ever called when the content overflows (<see cref="BuildScrollingBody"/>)
        /// — the rail is absent otherwise, per the ruling.</summary>
        private static void BuildPositionRail(RectTransform body, ScrollRect scrollRect,
            float trackHeight, float contentHeight)
        {
            const float trackWidth = 4f;
            float visibleFraction = Mathf.Clamp01(trackHeight / contentHeight);
            float thumbHeight = Mathf.Clamp(visibleFraction * trackHeight, 24f, trackHeight);

            LaptopUi.MakePanel(body, "PositionRailTrack", new Vector2(1f, 1f), new Vector2(1f, 1f),
                Vector2.zero, new Vector2(trackWidth, trackHeight), LaptopOs.RuleSoft);
            RectTransform thumb = LaptopUi.MakePanel(body, "PositionRailThumb", new Vector2(1f, 1f),
                new Vector2(1f, 1f), Vector2.zero, new Vector2(trackWidth, thumbHeight), LaptopOs.Muted);

            float travel = trackHeight - thumbHeight;
            void Reposition(Vector2 normalized)
            {
                float hidden = 1f - Mathf.Clamp01(normalized.y);
                thumb.anchoredPosition = new Vector2(thumb.anchoredPosition.x, -hidden * travel);
            }
            Reposition(new Vector2(0f, scrollRect.verticalNormalizedPosition));
            scrollRect.onValueChanged.AddListener(Reposition);
        }

        /// <summary>A1's shared single-column offer row (S27 ruling): 54px tall, full content
        /// width, a fact-coloured line/name label on the left (<see cref="LaptopOs.White"/>,
        /// condensed, 19px, uppercase — E-12, load-bearing under S28) and a 176px right-aligned
        /// price cell, with a 1px <see cref="LaptopOs.RuleSoft"/> rule along the row's bottom edge
        /// (kit: screens.jsx:59-72). <paramref name="role"/> is the S22/E-24 scorer-role word
        /// printed after the name in <see cref="LaptopOs.Muted"/> — null/empty for every
        /// non-scorer row. Shared by BuildMarketLines, BuildBothTeamsScore and BuildPlayerLines so
        /// their row geometry, the selection ring and the replacement-hint plumbing can never
        /// independently drift.
        ///
        /// <para><b>"uppercase" above was a CLAIM until S96.</b> E-12 has asserted it since the
        /// audit, but the row printed the engine's field verbatim — so the names that reach it as a
        /// Line ("OVER 2.5 GOALS", "DRAW") happened to be uppercase at source, while the ones built
        /// from a club name (the moneyline's two sides, "{Club} OR DRAW", "{Club} OVER 0.5 GOALS")
        /// printed in title case directly beneath an uppercase heading.
        /// <see cref="PrintedRowName"/> is what makes the sentence true.</para></summary>
        private void MakeOfferRow(RectTransform parent, BetslipModel slip, Matchup matchup,
            MarketSelection selection, string label, string role, int offerIndex, float y,
            float rowWidth, bool frozen)
        {
            const float leftPad = OfferLeftPad;
            const float rightPad = OfferRightPad;
            const float priceCellWidth = OfferPriceCellWidth;
            const float priceCellHeight = 32f;
            const float priceCellY = -(OfferRowHeight - priceCellHeight) / 2f; // vertical centre of the row.

            // THE REPLACE AFFORDANCE IS GONE, and it had to go with the gesture rather than after it.
            //
            // This asked `SelectionOn(matchup.Index)` — the FIRST leg on the match — and derived a
            // `replacement` state from it: every other offer on a match you had picked drew a `⇄`
            // before its price and a 2px underline beneath it, promising the player that his next
            // pick would REPLACE this one. The promise is now false. A second pick sticks.
            //
            // It is not merely stale copy either: `SelectionOn` answers for the first leg in slip
            // order and stops, so on a same-match slip the affordance was about to start marking
            // offers against whichever leg happened to be added first.
            bool selected = slip.Contains(matchup.Index, selection);
            string key = selection.Kind + selection.Choice.ToString()
                + selection.Line.ToString(CultureInfo.InvariantCulture) + selection.PlayerIndex;

            RectTransform row = LaptopUi.MakePanel(parent, "MarketOffer" + offerIndex,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, y),
                new Vector2(rowWidth, OfferRowHeight), new Color(0f, 0f, 0f, 0f));

            float priceX = rowWidth - rightPad - priceCellWidth;
            float labelWidth = priceX - OfferLabelGap - leftPad;

            // The market line is a FACT: condensed, --st-size-price, --toner. It does NOT turn biro
            // when picked — MarketOffer.jsx:11 sets the picked figure to var(--toner) and gives the
            // ring alone the biro. Tinting the type as well spends the player's ink on something he
            // did not write, which is what the two-ink law forbids (audit E-13).
            // S96 — THE ROW NAME IS UPPERCASED HERE, at the render site, and nowhere else. The
            // derivation layer keeps the engine's field verbatim (A2); the case is this surface's,
            // exactly like the face, the size and the tracking set on the same line. See
            // PrintedRowName for the ruling, and MarketRowNameWidthTests for C46's width gate.
            TMP_Text labelText = LaptopUi.MakeText(row, "MarketLabel" + key, new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(leftPad, 0f), new Vector2(labelWidth, OfferRowHeight),
                // C15/S28: `.08` — MarketOffer.jsx's `line` span carries --st-track-rec. (The kit puts
                // that span at --st-size-fact where this renders at --st-size-price; that size
                // difference predates the migration and is not touched here, only the tracking.)
                19, TextAnchor.MiddleLeft, LaptopOs.White, PrintedRowName(label), _fontCond,
                LaptopTrack.Records);

            // The rendered end of the NAME — the market name, plus the scorer role when there is
            // one. §4.3's leaders start here rather than at the end of the label CELL, which on this
            // row is nearly always hundreds of pixels wider than the type inside it.
            float nameEnd = leftPad + labelText.preferredWidth;

            if (!string.IsNullOrEmpty(role))
            {
                // The role is a LABEL, not a figure (audit E-24, named load-bearing by S28). With
                // tracking unreachable, the only channels left to separate label from fact are the
                // colour split (--toner-3 label against --toner fact) and the two-voice type split
                // (roman label against condensed figure). Both must therefore be exact here: roman
                // face, fact floor, --toner-3 — a condensed 19px role would read as a second fact.
                float roleX = leftPad + labelText.preferredWidth + 8f;
                float roleWidth = Mathf.Max(0f, leftPad + labelWidth - roleX);
                TMP_Text roleText = LaptopUi.MakeText(row, "MarketRole" + key, new Vector2(0f, 1f),
                    new Vector2(0f, 1f), new Vector2(roleX, 0f),
                    new Vector2(roleWidth, OfferRowHeight), 13, TextAnchor.MiddleLeft,
                    LaptopOs.Muted, role, _font);
                roleText.enableWordWrapping = false;
                nameEnd = roleX + roleText.preferredWidth;
            }

            // §4.3, RULED: the offer row is ONE statement, not two facts at opposite ends. The gap
            // is the annotation gap (S92), and a gap that is doing work should look like it.
            MakeLeaders(row, "OfferLeaders" + key, nameEnd, priceX, OfferRowHeight);

            RectTransform offer = LaptopUi.MakePanel(row, "PriceCell" + key, new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(priceX, priceCellY),
                new Vector2(priceCellWidth, priceCellHeight), new Color(0f, 0f, 0f, 0f));

            if (selected)
            {
                Sprite ring = ResolveWideRing(matchup.Index);
                if (ring != null)
                {
                    // Y offset is POSITIVE — see the historic note this geometry inherits (the old
                    // MakeMarketOffer, same math): with a top-left pivot a negative anchoredPosition.y
                    // moves the rect DOWN, so the ring must overshoot with a positive Y to sit above
                    // the cell rather than under it.
                    //
                    // Size is the cell + 16 per ASSETS.md/InkMark.rect(): A1 widened the price cell
                    // from 160 to 176, so the ring is now 192x48 (was 176x48).
                    const float overshoot = 8f;
                    Vector2 cellSize = new Vector2(priceCellWidth, priceCellHeight);
                    LaptopUi.MakeSprite(offer, "WideBiroRing", ring, new Vector2(0f, 1f),
                        new Vector2(0f, 1f), new Vector2(-overshoot, overshoot),
                        cellSize + new Vector2(overshoot * 2f, overshoot * 2f), LaptopOs.Accent);
                }
            }
            string price = OddsFormat.American(matchup.Odds(selection));
            // Ground is TRANSPARENT, not Ink (MarketOffer.jsx:19-23 `background:"transparent"`). The
            // ring is a child created BEFORE this button, so an opaque ground paints over its
            // shoulder arcs — the long-standing "the ring does not close" read (audit E-15). A
            // zero-alpha Image still raycasts, so the control keeps its hit area.
            // The figure stays --toner when picked; only the ring is biro (E-13, as on the label).
            LaptopUi.MakeButton(offer, "Market" + key,
                price, new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(priceCellWidth, priceCellHeight), 19, new Color(0f, 0f, 0f, 0f),
                // §4.4 — THE ONE PRICE-INK SITE ON THIS SURFACE, and it is now a COLOUR rather than
                // a switch.
                //
                // S97 (DD batch 113): THE PRICE DOES NOT TAKE THE AMBER. S91 half two is CLOSED —
                // the price stays in toner. Read off the S4/S5 pair this seam existed to shoot:
                //   · amber made the price the most saturated element in the column and INVERTED
                //     the name-first hierarchy S91 had just ratified;
                //   · it diluted amber — two amber things on one sheet meaning different things;
                //   · and scarcity is what makes an action colour work at all.
                // The build already defaulted to toner, so nothing shipped changes; what changes is
                // that the question is closed in the code. The `PriceTakesAmber` toggle and the
                // `PriceInk` indirection are gone and this reads LaptopOs.White directly.
                //
                // NAMED, NOT RULED: amber's real claim is the SELECTED price — the moment a price
                // stops being the house's offer and becomes the player's stake. That belongs with
                // the selection treatment (today the biro ring above), and S97 does not rule it.
                //
                // The frozen arm was never part of the question and is unchanged: a locked board's
                // price is not an offer, so it greys (§4.5: suspended is greyed, non-clickable AND
                // stated — the LockedMarketReason banner in BuildDetail is the stating half).
                frozen ? LaptopUi.Dim(LaptopOs.Muted) : LaptopOs.White,
                // `.03` for the same reason as the moneyline buttons: MarketOffer.jsx sets
                // --st-track-name on the price cell. This is the interior-list price and the two
                // must match — one is the same object one screen deeper.
                // S85 §6 — THE BEHAVIOURAL HALF ONLY, AND THE REASON IS A FINDING. At the cap this
                // stops accepting the click, exactly as the board's cells do. What it CANNOT do here
                // is show it: the board says "offer" with a FIELD and this cell has none to remove —
                // `MarketOffer.jsx` gives the interior price cell no fill, so the transparent
                // treatment that expresses the rule on the board is a no-op one screen deeper.
                //
                // Which surfaces a scope fact §6 asked for and the frame could not show: the two
                // surfaces do not share an offer affordance today. On the board a live offer sits in
                // a field; on this sheet a live offer is already bare type. So an ENTRY offer that
                // has gone dead is indistinguishable from a live one BECAUSE a live one already
                // looks like a fact. Reported, not solved — giving these cells a field would
                // contradict the kit, and that is a ruling rather than a lane's call.
                frozen || !OfferIsTakeable(slip, _host.director.Run, matchup.Index, selection)
                    ? (System.Action)null
                    : () => { PickOffer(slip, matchup.Index, selection); }, _fontCond,
                !frozen && OfferIsTakeable(slip, _host.director.Run, matchup.Index, selection),
                LaptopTrack.Names);

            // S27 ruling: the printed row rule (kit: screens.jsx:64, 1px --rule-soft).
            LaptopUi.MakeRule(row, "OfferRowRule" + offerIndex, new Vector2(0f, 0f), new Vector2(0f, 0f),
                Vector2.zero, new Vector2(rowWidth, 1f));
        }

        // T47: the action stack is ANCHORED to the margin's bottom edge and its height is RESERVED,
        // so the flow region above it and the action band below can never meet. Every offset here is
        // measured up from the panel's bottom; nothing in this band depends on how many legs are
        // marked, which is the point — LOCK IT IN must not move because the player bet more.
        /// <summary>Per-leg vertical step in the working margin. S50 §2 collapsed it from 42 to 35
        /// by closing spacing only — 7px x 4 legs = 28px, which with the 18px deleted status line
        /// clears T47's 44px deficit. S39's one-baseline grammar, applied where it already belonged.</summary>
        private const float LegRowPitch = 35f;

        private const float SkipBandY = 8f;
        private const float SkipBandH = 34f;
        private const float LockBandY = 52f;
        private const float LockBandH = 52f;   // label in the upper 30, reason nested in the lower 20
        private const float PlaceBandY = 110f;
        private const float PlaceBandH = 44f;

        /// <summary>The reserved height of the anchored action band, including a 6px separation from
        /// the flow region. MaxLegs makes the flow's worst case computable, so the reservation is a
        /// constant rather than a hope — <see cref="MarginFlowBudget"/> is what the flow must fit in.
        /// Guarded by the PlayMode margin invariant, which states its own blind spots.</summary>
        internal const float ActionBandReservedHeight = PlaceBandY + PlaceBandH + 6f;

        /// <summary>The vertical budget available to everything above the action band, given the
        /// margin's fixed 530px panel.</summary>
        internal const float MarginFlowBudget = 530f - ActionBandReservedHeight;

        // ------------------------------------------------------------------ S83: the margin's three zones
        //
        // ZONE 1 HEAD (fixed) · ZONE 2 THE SLIP (scrolls) · ZONE 3 THE COMMIT (anchored, reserved).
        //
        // The split is derived rather than chosen. Zone 1 is the board's own grammar one screen over
        // — `MY MARKS · n SELECTIONS` is a column head for the legs under it, and a count that
        // scrolls away from the things it counts is a head that has become a row. Zone 3 answers the
        // objection the spec raised against its own option: the payout can otherwise sit below the
        // fold while he presses PLACE, which is the exact defect S17/S73 exist to prevent — a cost he
        // cannot see at the point of spending. So the two figures the commit is about, what he stakes
        // and what he would win, are anchored WITH the controls that commit them. The stake block is
        // not split across the boundary either: M-05 put the figure first because the figure is the
        // fact, and separating a figure from its own controls to save pixels would undo that.
        //
        // T47 IS EXTENDED, NOT WEAKENED. The rule was always that the flow region and the action band
        // can never meet; the band now contains everything the commit depends on. PLACE, LOCK and
        // SKIP do not move by a pixel — every constant below builds UP from
        // `ActionBandReservedHeight` rather than moving it.
        internal const float SlipHeadHeightPx = 40f;                                // zone 1
        private const float SlipHeadHeight = SlipHeadHeightPx;
        // Zone 3, measured up from the panel's floor. The band's own drop is the kit's
        // (`--st-size-payout` 31 x `--st-lh-fig` 1.1, plus its `bottom:-2px`) — the same derivation
        // S51 closed on, reused here rather than restated as a number.
        private const float CommitPayoutTop = ActionBandReservedHeight + (31f * 1.1f + 2f);
        private const float CommitPayoutLabelTop = CommitPayoutTop + 16f;
        // CommitNudgeTop is GONE — S82-am2 / S80-am2-cl2 (batch 107, 2026-08-17) DELETED the nudge
        // row (−$10/+$10 stake chips) on redundancy: the fraction chips already set the stake. This
        // is the whole edit — CommitChipTop now builds directly on CommitPayoutLabelTop, reusing the
        // same +34f the chip row always advanced by (its own height plus trailing gap), previously
        // spent reaching the nudge row and now reaching the payout label straight away. The removed
        // hop is exactly CommitNudgeTop's old +32f, so this recovers 32px with nothing hand-patched
        // downstream to compensate — CommitZoneReserved and SlipViewportHeight just fall out of it.
        private const float CommitChipTop = CommitPayoutLabelTop + 34f;
        /// <summary>Zone 3's full reservation, measured up from the panel floor: the action band plus
        /// the stake and payout blocks that commit depends on.</summary>
        internal const float CommitZoneReserved = CommitChipTop + 34f;
        /// <summary>Zone 2's viewport — what is left once the head and the commit are reserved.
        /// DERIVED, never chosen, and it is the one number C is sized by.</summary>
        internal const float SlipViewportHeight = 530f - SlipHeadHeight - CommitZoneReserved;

        /// <summary>The three outcome cells' y in a matchup card, and the one derivation among them.
        ///
        /// <para><b>`DrawCellY` is COMPUTED, and that is the point.</b> S74 rules the draw's middle
        /// position as MEANING — its line sits physically between the two teams', attached to
        /// neither — and the shipped literal `−43` was not the middle: 35px below AWAY and 38px above
        /// HOME. The DD's DRAW-frame read caught it and moved it to −44.5, which is exactly
        /// `(−8 + −81) / 2`. Written as the midpoint rather than as that number so the design's own
        /// claim holds by construction, and so a future move of either team line carries the draw
        /// with it instead of stranding it.</para>
        ///
        /// <para>All four sites read these: the three `MakeButton` calls and the biro ring's own
        /// ternary. The ring's comment already named the hazard — "two elements agreeing by
        /// convention rather than by construction" — and a literal moved in one place and not the
        /// other is precisely that.</para></summary>
        private const float AwayCellY = -8f;
        private const float HomeCellY = -81f;
        private const float DrawCellY = (AwayCellY + HomeCellY) / 2f;   // −44.5

        /// <summary>Gutter x and stroke weight for <c>THE HOUSE'S LINE</c>. The gutter is the strip
        /// between the 2px sheet divider at x=0 and the leg rows' own left pad at x=14 — the margin
        /// of the margin, which is where an annotating hand has room to write.</summary>
        /// <summary>P5's slot: two 13px lines. The family's longest member does not fit one line in
        /// the 296px content column, and the copy is ruled (S78) — so the slot is authored around the
        /// approved sentence rather than the sentence cut to fit an unspecified box.
        ///
        /// <para><b>TWO LINES ALWAYS, and this is the part that matters to the flow-cost decision.</b>
        /// Seven of the nine sentences fit ONE line — measured — so a slot that sized itself to the
        /// sentence would cost 15px instead of 30 in most cases. **§2 forbids exactly that**: a fixed
        /// grid constant re-derived once at design time is legal, a zone resizing in response to
        /// content is not, and the draws block was already held to this rule ("an empty line is
        /// honest where a collapsing block is not").
        ///
        /// So the flow cost of this statement is NOT reducible by sizing to the common case. Anyone
        /// weighing the ~36px against T47's budget is weighing 36px, not "36px sometimes".</para>
        /// </summary>
        internal const float RelationStatementHeight = 30f;


        private const float HouseLineX = 7f;
        private const float HouseLineWeight = 2f;
        private const float HouseLineSpur = 5f;

        /// <summary>One connected group's mark: a spine down the gutter with a spur at each member
        /// row. See the call site for why the spurs are load-bearing rather than ornamental.</summary>
        private static void DrawHouseLine(RectTransform panel, int markIndex, List<int> members,
            List<float> legRowY)
        {
            // The identity line of a leg row is 20px tall from the row's own top (see LegCheck/Leg).
            const float legIdentityHeight = 20f;
            float top = legRowY[members[0]];
            float bottom = legRowY[members[^1]] - legIdentityHeight;
            LaptopUi.MakePanel(panel, "HouseLine" + markIndex, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(HouseLineX, top), new Vector2(HouseLineWeight, top - bottom),
                LaptopOs.MoneyBad);
            for (int m = 0; m < members.Count; m++)
                LaptopUi.MakePanel(panel, $"HouseLineSpur{markIndex}_{m}", new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(HouseLineX, legRowY[members[m]] - legIdentityHeight / 2f),
                    new Vector2(HouseLineSpur, HouseLineWeight), LaptopOs.MoneyBad);
        }

        private void BuildSlip(Run run, BetslipModel slip, bool boardFrozen)
        {
            RectTransform panel = LaptopUi.MakePanel(_root, "Slip", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -140f), new Vector2(324f, 530f), LaptopOs.Ink);
            panel.name = "WorkingMargin";
            // MERGE (markets-2 × main, 2026-08-05): main's ruled-paper ground wins outright. Both
            // sides built S34's Graphic independently; main's also carries the CanvasRenderer fix,
            // without which a Graphic is never asked for geometry at all — mine was constructed via
            // the GameObject type list, which ignores [RequireComponent], so it had never drawn a
            // single line while every test stayed green. Main's explicit sizeDelta matters for the
            // same reason: an anchor-stretched rect reads zero on this imperatively-built canvas
            // and UGUI culls it before OnPopulateMesh.
            LaptopUi.MakeMarginRuledPaper(panel, "RuledPaper");
            // F2: screens.jsx's sheet.borderRight (2px solid var(--rule)) — every screen's 700px
            // sheet and 324px margin meet with no seam between them. Drawn as this margin's own
            // left edge (global x=700) rather than the sheet's right edge so FORM and ENTRY, which
            // both call BuildSlip for this one panel, get it from a single call.
            LaptopUi.MakeRule(panel, "SheetDivider", new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(2f, 530f), LaptopOs.Rule);
            // MERGE: main's F2 sheet divider above is kept as-is — it is their work and does not
            // collide with mine. Main's "Title" and "Rule" text blocks are deliberately NOT kept
            // here: the header immediately below supersedes them (M-08/B1-1 splits the joined
            // string into a biro title plus a right-flushed count with its own --biro-deep rule),
            // and re-adding main's "Rule" line would resurrect "PRICES FINAL. NOTHING YOU DO MOVES
            // THEM." — the restatement S37 forbids and S50 §1 ordered deleted, which is where 18 of
            // the 44px came from. Taking both blocks would also have drawn two headers.

            // B1-1/M-08 ruling (load-bearing under S28): MarginHeader.jsx (kit) is a title plus a
            // right-flushed count and its own 2px biro-deep rule — not one joined White string.
            // With letter-spacing unreachable (S28), colour is the only channel left telling the
            // literal title ("MY MARKS", --biro — this is the player's OWN margin, S33 confirms
            // biro-ruled headers name whose margin it is) from the dynamic count fact (--toner-2).
            // The old joined string also folded the staged-ticket count into the title; that fact
            // survives here in the count slot, since the kit's own count prop only defines the
            // selections half of it (margin.jsx:20) and nowhere else on this panel prints it.
            const float headerRight = 296f; // 324 - 14 - 14, the content width every row below uses.
            TMP_Text headerTitle = LaptopUi.MakeText(panel, "Title", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -10f), new Vector2(150f, 24f), 16, TextAnchor.UpperLeft, LaptopOs.Accent,
                "MY MARKS", _fontCond);
            // S85 §1 — THE CAUSE IS STATED ONCE, HERE. Fourteen dead cells are ONE fact about the
            // slip, not fourteen facts about prices: a refusal printed on every refused control is
            // T69/T70's defect at fourteen sites, and would put more words on the board than the
            // board has prices. This head already states how many he holds, and the cap is a fact
            // about that count — so the count states it, in its own grammar.
            //
            // `4 OF 4 SELECTIONS` rather than a new sentence: no vocabulary is added, the denominator
            // IS the cause, and it appears only at the cap so it never reads as decoration.
            bool slipFull = slip.Picks.Count >= run.Config.MaxLegs;
            string countText = slipFull
                ? $"{slip.Picks.Count} OF {run.Config.MaxLegs} SELECTIONS · {run.Tickets.Count} STAGED"
                : $"{slip.Picks.Count} {Pluralize(slip.Picks.Count, "SELECTION")} · {run.Tickets.Count} STAGED";
            float countMaxWidth = Mathf.Max(0f, headerRight - headerTitle.preferredWidth - 8f);
            countText = LaptopUi.FitText(_font, countText, 13, countMaxWidth);
            LaptopUi.MakeText(panel, "Count", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-14f, -12f), new Vector2(countMaxWidth, 18f), 13, TextAnchor.UpperRight,
                LaptopOs.TonerSecondary, countText, _font);
            LaptopUi.MakePanel(panel, "HeaderRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -34f), new Vector2(headerRight, 2f), LaptopOs.BiroDeep);

            // S50 §1: the house status line is DELETED — 18px. It restated the scope line, which at
            // the time read "ROUND n OF 8 · PRICES FINAL" — batch 21 has since taken PRICES FINAL
            // off the masthead subline (see :98), so that quotation is HISTORICAL and is kept only
            // because it is what S50 was ruled against. The restatement it describes was of the
            // SCOPE, which the masthead still carries, so the ruling is untouched by that change.
            // S37 forbids it outright (the masthead carries the run's scope, the board header the
            // screen's, and nothing restates either), and the markets C14 audit already carried it
            // as invented (M-09). This was never an open question — an unexecuted ruling is not a
            // pending one.
            // S82 option A, harvest 1 of 3 — the header's own gap, 8px measured down to 4px.
            // The header's content ends at −36 (the 2px rule under a 24px title) and the first leg
            // opened at −44. S50's standing order is spacing first, and 4px still separates the rule
            // from the row beneath it. Measured, not guessed: this gap read 8.0px on the tree.
            //
            // S83 — ZONE 2 OPENS HERE. Everything from this point to the COMMIT block is built into
            // a scrolling body rather than onto the panel: the leg rows, THE HOUSE'S LINE, the
            // relation statement, the price row and the modifiers row. The cursor is now LOCAL to
            // that content, so it starts at 0 rather than at the head's height.
            RectTransform slipBody = LaptopUi.MakeScrollBody(panel, "SlipScroll",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -SlipHeadHeight),
                new Vector2(324f, SlipViewportHeight), out RectTransform slipHost,
                out ScrollRect slipScroll);
            RectTransform flow = slipBody;
            float y = 0f;
            if (slip.Picks.Count == 0)
            {
                // S71: names the STATE, not the owner. This read "YOUR MARGIN IS CLEAR" — someone
                // addressing him, three lines under "MY MARKS", which is him, in the one column the
                // owning doc says is his. §6 puts second person in genuine imperatives only, and this
                // is a statement; it also allows first person exactly once on the surface, and that
                // once is the header directly above. A second speaker is most expensive here.
                //
                // The ownership does not need saying: the header states it and the column is drawn
                // in the ink that means "what he chose".
                LaptopUi.MakeText(flow, "Empty", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, y), new Vector2(300f, 26f), 13, TextAnchor.UpperLeft, LaptopOs.Muted,
                    "NO MARKS ON THIS SHEET", _font);
                // S71-am3 (DD 2026-08-09, batch 28): member 1 takes the KIT'S PAIR. The kit authors
                // this slot as a statement then a genuine imperative — margin.jsx:24, "No marks on
                // this sheet. Circle a price to start a ticket." S71 took the second SPEAKER out of
                // the statement and did not put the remedy back, because S71 was a ruling about
                // ownership and the remedy was never in view. The imperative carries no pronoun, so
                // §6 is satisfied by it rather than strained.
                //
                // Built as the surface's existing pair, not as one joined string: statement in
                // --toner at the state's own weight, remedy in --toner-2 beneath it, which is how
                // MyBetsEmpty/Remedy and RewardsEmpty/Remedy already read. The kit's single text node
                // is its grammar, not its layout.
                //
                // The stop is S72-p: this is a sentence, and sentences take one. The statement above
                // is a fragment and correctly does not.
                LaptopUi.MakeText(flow, "EmptyRemedy", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, y - 30f), new Vector2(300f, 26f), 13, TextAnchor.UpperLeft,
                    LaptopOs.TonerSecondary, "CIRCLE A PRICE TO START A TICKET.", _font);
                y -= 60f;
            }
            // P4 needs each leg's own row position to mark the connected ones, and the cursor below
            // is consumed by the loop, so the rows are recorded as they are drawn.
            var legRowY = new List<float>(slip.Picks.Count);
            // S77: the legs a refusal's remedy refers to are marked in the flow rather than named in
            // the stamp. Read once here — the model caches its pricing, but the loop runs per leg.
            HashSet<int> refusalMarks = null;
            if (StampComposedRefusal && slip.Refusal != null && slip.Refusal.HasRemedy)
                refusalMarks = new HashSet<int>(slip.Refusal.RemedyLegs);
            for (int i = 0; i < slip.Picks.Count; i++)
            {
                legRowY.Add(y);
                Pick pick = slip.Picks[i];
                Matchup matchup = run.CurrentSlate.Matchups[pick.MatchupIndex];
                MatchModel.MarketFields fields = MatchModel.Fields(matchup, pick.Selection);

                // B1-2/M-02 ruling (load-bearing under S28): MarginLeg.jsx is two lines, not one
                // joined string — a biro check + condensed team/price on line 1, and a roman
                // "{market} · ENTRY {entry}" fact line under it in --toner-3. With tracking gone,
                // the colour split (--toner team/price vs --toner-3 market/entry) and the face split
                // (condensed fact vs roman label) are the only two channels left telling them apart.
                // Subject falls back to Line when empty, same as CompactLegLabel's own switch —
                // every market but Moneyline/AnytimeScorer backs the match itself, not one subject.
                // Team names are shortened the same way CompactLegLabel/the board already do; a
                // moneyline pick never repeats the picked team's own full name.
                string subject = MarginLegSubject(matchup, pick.Selection);
                string price = OddsFormat.American(matchup.Odds(pick.Selection));
                // ENTRY is the matchup's own FORM board position — same "(index+1):00" the board's
                // Number badge already prints (BuildMatchupCard) — not a per-selection identity.
                string entry = (matchup.Index + 1).ToString("00");

                const float rowRight = 244f; // 14 (row left) + 230 (old legWidth) — clear of RUB OUT.
                const float checkBoxWidth = 18f; // kit's check column is 15px; padded for the glyph.
                const float contentX = 38f; // leftPad(14) + kit check column(15) + kit gap(9).
                const float priceWidth = 70f;
                const float priceX = rowRight - priceWidth;
                const float teamWidth = priceX - 6f - contentX;

                // Line 1: biro check, team/subject (condensed, toner), price (condensed, toner,
                // right-flushed in its own cell so it stays flush regardless of team width).
                LaptopUi.MakeText(flow, "LegCheck" + i, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, y), new Vector2(checkBoxWidth, 20f), 15, TextAnchor.UpperLeft,
                    LaptopOs.Accent, "✓", _font);
                // Named "Leg" + i (not e.g. "LegTeam") — SureThingEntryTests' entry-persistence
                // snapshot looks up "Leg0" by that exact name; this is the closest surviving analog
                // to the old single joined-string node, so the lookup still resolves.
                LaptopUi.MakeText(flow, "Leg" + i, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(contentX, y), new Vector2(teamWidth, 20f), 16, TextAnchor.UpperLeft,
                    LaptopOs.White, subject, _fontCond)
                    .enableWordWrapping = false;
                LaptopUi.MakeText(flow, "LegPrice" + i, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(priceX, y), new Vector2(priceWidth, 20f), 16, TextAnchor.UpperRight,
                    LaptopOs.White, price, _fontCond, LaptopTrack.Names);

                // Line 2: "{market} · ENTRY {entry}" — roman, fact floor, --toner-3. Indented to the
                // content column past the check (kit: the check sits outside the flex column that
                // holds both lines), not the row's own left edge.
                LaptopUi.MakeText(flow, "LegDetail" + i, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(contentX, y - 20f), new Vector2(rowRight - contentX, 15f), 13,
                    TextAnchor.UpperLeft, LaptopOs.Muted, $"{fields.Market} · ENTRY {entry}", _font);

                // 1px --rule bottom rule (M-02), spanning the FULL row including the RUB OUT column
                // (kit: the border sits on the outer flex row, check+content+button together) — the
                // shared LaptopUi.MakeRule is hardcoded to --rule-soft and cannot be reused here.
                LaptopUi.MakePanel(flow, "LegRule" + i, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, y - 34f), new Vector2(headerRight, 1f), LaptopOs.Rule);

                // LEG-ADDRESSED, not matchup-keyed. This called Remove(matchupIndex), which drops
                // THE leg on a matchup — with two legs on one match it cannot address one of them,
                // and would take both. One of the seven sites the same-match survey found.
                int legIndexForRemoval = i;
                if (run.OwnsConsumable("profit_boost"))
                {
                    bool boosted = slip.BoostLeg == i;
                    int legIndex = i;
                    LaptopUi.MakeButton(flow, "Boost" + i, boosted ? "BOOST ✓" : "BOOST",
                        new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-76f, y + 5f),
                        new Vector2(58f, 24f), 13, boosted ? LaptopOs.MoneyGold : LaptopOs.SurfaceRaised,
                        LaptopOs.White, () => { slip.ToggleBoost(legIndex); _invalidate(); }, _font);
                }
                // RUB OUT stays 60x32 — S50 §3 keeps it at size deliberately, because a mis-click
                // here costs money. Vertically centred against the identity/market pair rather than
                // pinned to the first baseline, per S50 §2.
                // S77's mark. Where the slip is refused, the legs the remedy refers to are marked on
                // their own rows — and the mark is THIS control, in the house's ink. The stamp says
                // "RUB OUT THE MARKED LEG"; the control that performs it is the thing lit, so the
                // instruction and its target are the same object rather than two strings to match.
                // Stamp is the house acting on the document (§3.1); a lit RUB OUT is exactly that.
                bool markedForRemoval = refusalMarks != null && refusalMarks.Contains(i);
                LaptopUi.MakeButton(flow, "Remove" + i, "RUB OUT", new Vector2(1f, 1f), new Vector2(1f, 1f),
                    new Vector2(-12f, y + 1.5f), new Vector2(60f, 32f), 13, LaptopOs.Ink,
                    markedForRemoval ? LaptopOs.MoneyBad : LaptopOs.Muted,
                    () => { slip.RemoveLeg(legIndexForRemoval); _lockArmed = false; _invalidate(); },
                    _fontCond);
                // S50 §2: the leg row takes S39's one-baseline discipline — a margin leg is the same
                // kind of object as a settled record (identity, price, market, state) and has no
                // business carrying a different vertical grammar. The yield comes from SPACING, which
                // is first in S50's standing order (spacing, then repetition, then nothing): the two
                // baselines close from 22px to 20px apart and the rule from 38 to 34, taking the step
                // 42 -> 35. Nothing that states a product fact was deleted to make the layout fit.
                y -= LegRowPitch;
            }

            // P4 — THE HOUSE'S LINE (§3.1, S73). Where two of his picks are priced as related, the
            // house marks the connection between them IN ITS OWN INK: he picks in biro, the house
            // marks in Stamp. Drawn in the margin's own left gutter, between the sheet divider and
            // the check column, because that is where a hand annotating a document would put it.
            //
            // DRAWN, NOT CAPTIONED — no label, no name, no tag beside it. §3.1 is explicit that the
            // name is what the thing is CALLED (rules copy, the ledger, a first explanation) and
            // never a caption on every occurrence: "a mark that needs a caption every time is a mark
            // that is not working, and the house does not narrate its own presence on his document".
            //
            // The spine spans the group's first row to its last, and each MEMBER row takes its own
            // spur. The spurs are not decoration: slip order is insertion order, so two legs on one
            // match can sit either side of a leg on a different match, and a bare spanning stroke
            // would mark a row it has nothing to do with. The spurs say which rows the line is about.
            //
            // GEOMETRY IS A CANDIDATE, not canon — §3.1 rules the ink, the connection and the
            // absence of a caption, which is what is implemented here; the stroke weight and the
            // spur length want frames, exactly as the VOID row's rub-out does.
            if (slip.Picks.Count > 1)
            {
                var groups = new Dictionary<int, List<int>>();
                for (int i = 0; i < slip.Picks.Count; i++)
                {
                    if (!groups.TryGetValue(slip.Picks[i].MatchupIndex, out List<int> members))
                        groups[slip.Picks[i].MatchupIndex] = members = new List<int>();
                    members.Add(i);
                }
                int markIndex = 0;
                foreach (KeyValuePair<int, List<int>> group in groups)
                {
                    if (group.Value.Count < 2) continue;   // one leg on a match is not a connection
                    DrawHouseLine(flow, markIndex++, group.Value, legRowY);
                }
            }

            // P5 — THE STATEMENT (S78). One relation per slip, in toner, composed from `principal`.
            // Never a formula, never a coefficient, never an English string from the engine — the
            // model emits parts and this composes the sentence.
            //
            // TWO LINES, because the family's longest member does not fit one at 13px and the copy is
            // ruled: `THE SAME TEAM'S GOALS SETTLE THESE OPPOSITE WAYS.` measures past the 296px
            // content column. The slot is authored around the approved copy rather than the copy
            // being cut to a slot that was never specified — the owning doc gives this statement
            // "toner, once per slip" and no box.
            //
            // Lengthening is NOT remarked (§8): this states the relation and never that the price
            // moved in his favour.
            string relationStatement = RelationStatement(slip.SameMatchPricing, slip.Picks);
            if (relationStatement != null)
            {
                LaptopUi.MakeText(flow, "RelationStatement", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, y - 4f), new Vector2(headerRight, RelationStatementHeight), 13,
                    TextAnchor.UpperLeft, LaptopOs.White, relationStatement, _font);
                y -= RelationStatementHeight + 6f;
            }

            // E-07 ruling: staged ticket receipts no longer render here. The 324px margin has no
            // room for up to MaxTicketsPerRound of them above the anchored action band (T47) — they
            // now render in the ENTRY sheet's own scrolling body instead (BuildScrollingBody), which
            // is where the kit puts them (screens.jsx:49-58, "PLACED THIS ROUND").
            //
            // S82 option A, harvest 2 of 3 — the bare 4px that stood here is GONE. The spec named it
            // by line: a gap with no derivation, left behind when the receipts it used to separate
            // moved out to the sheet. The leg list's last row already ends in its own 1px rule, so
            // the COMBINED row below it is separated by a ruled edge rather than by air.
            // B1-3/M-03 ruling (load-bearing under S28): MarginRow.jsx is a label/value pair with
            // its own 1px --rule bottom rule, not one joined "COMBINED {odds}" string in one face
            // and one colour — S28 names this the exact failure that leaves label and fact
            // indistinguishable once tracking is dropped. Value is condensed and right-flushed
            // across the full row width (kit: value gets marginLeft:auto); label stays roman,
            // --toner-3. "Combined" (not e.g. "CombinedValue") is kept on the value node because
            // SureThingMilestoneOneTests' contract-floor test looks this node up by that name.
            // P4's other half — THE INSTRUMENT IS NAMED. `SAME MATCH` is the instrument's name
            // (Allen, 2026-08-12), uppercase like the rest of the market vocabulary: a role printed
            // as a word, a fact rather than a brand. `SGP` is industry jargon and never reaches him.
            //
            // It lands on THIS label rather than beside the mark, and the choice is forced rather
            // than aesthetic. §454 rules that a same-match ticket is "its own instrument — never a
            // parlay with an adjustment", and `COMBINED` names the price as a combination arrived at
            // by multiplying, which is precisely the reading canon forbids for this ticket. The
            // label was not merely silent about the instrument; on a same-match slip it was WRONG.
            // The figure beside it is already the engine's joint price rather than a product, so the
            // label was the last thing on this row still describing a parlay.
            //
            // Untracked, per the plan — the market vocabulary's own treatment, not a badge's.
            // And not beside THE HOUSE'S LINE: §3.1's "drawn, not captioned" governs the MARK, and
            // naming the instrument on the slip's own price row is not captioning the mark.
            LaptopUi.MakeText(flow, "CombinedLabel", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, y), new Vector2(120f, 18f), 13, TextAnchor.UpperLeft, LaptopOs.Muted,
                slip.IsSameMatch ? "SAME MATCH" : "COMBINED", _font);
            LaptopUi.MakeText(flow, "Combined", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, y), new Vector2(headerRight, 22f), 18, TextAnchor.UpperRight, LaptopOs.White,
                slip.Picks.Count > 0 ? OddsFormat.American(slip.CombinedOdds) : "—", _fontCond);
            LaptopUi.MakePanel(flow, "CombinedRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, y - 24f), new Vector2(headerRight, 1f), LaptopOs.Rule);
            y -= 28f;

            bool freeHeld = run.OwnsConsumable("free_bet");
            bool donHeld = run.OwnsConsumable("double_or_nothing");
            if (freeHeld || donHeld)
            {
                if (freeHeld)
                    MakeModifier(flow, "FREE BET", TicketModifier.FreeBet, slip, 14f, y);
                if (donHeld)
                    MakeModifier(flow, "DOUBLE OR NOTHING", TicketModifier.DoubleOrNothing, slip,
                        freeHeld ? 148f : 14f, y);
                y -= 34f;
            }

            // M-04 + M-05, against kit StakeControls.jsx:11-29. Two defects, one block, one fix.
            //
            // M-05 — the kit LEADS with the figure: one baseline row (label left, figure right),
            // then the fractions, then the nudges. Shipped ran that order backwards, so the fact the
            // controls exist to set was printed underneath the controls. The figure is the fact; it
            // goes first.
            //
            // M-04 — shipped fused label and figure into one `"STAKE $N"` string at 16px roman,
            // left-aligned. That is the exact failure S28 names: one string, one size, one colour
            // leaves the label and the fact indistinguishable. The COMBINED row twenty lines up was
            // already corrected to the kit's two-node form, so this is the same treatment, not a new
            // pattern — label roman at the 13px fact floor in `--toner-3` carrying its own .12em
            // track (reachable since the TMP migration expired S28), figure condensed at the kit's
            // `--st-size-stake` 26px in `--toner`, right-flushed across the row.
            //
            // Both nodes share one 30px row and are LOWER-anchored so their baselines meet, which is
            // what the kit's `alignItems:"baseline"` asks for; top-anchoring two boxes 13px apart in
            // size would not. The value node keeps the name "Stake" — SureThingEntryTests reads that
            // node by name across a destination switch.
            // S83 — ZONE 2 CLOSES. The rail is drawn only when the content actually exceeds the
            // viewport (LaptopUi.FinishScrollBody), which is the spec's "scroll only when genuinely
            // needed" — and after option A the ordinary compositions no longer exceed it, so the
            // scroll engages where it is needed rather than always being slightly engaged. A
            // scrollbar that appears for a tenth of a pixel is worse than no scrollbar.
            //
            // The scroll rests at the TOP: one mechanism and one behaviour with the board (S25-am /
            // S27's printed rail), and the head names what is under it.
            LaptopUi.FinishScrollBody(slipHost, slipScroll, flow, -y, SlipViewportHeight);

            // S83 — ZONE 3 OPENS. The commit block is ANCHORED and RESERVED: its cursor is a fixed
            // height above the panel's floor rather than an accumulation of everything above it, so
            // nothing in zone 2 can move it. That is the whole point — the two figures the commit is
            // about are on screen whenever PLACE is.
            y = -(530f - CommitZoneReserved);
            TMP_Text stakeLabel = LaptopUi.MakeText(panel, "StakeLabel", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, y), new Vector2(120f, 30f), 13, TextAnchor.LowerLeft, LaptopOs.Muted,
                "STAKE", _font, 0.12f);
            TMP_Text stakeFigure = LaptopUi.MakeText(panel, "Stake", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, y), new Vector2(headerRight, 30f), 26, TextAnchor.LowerRight, LaptopOs.White,
                LaptopUi.Money(slip.Stake), _fontCond);
            // The kit asks for alignItems:"baseline". Bottom-anchoring aligned the two DESCENDER
            // lines instead, and a 26px face's descender is deeper than a 13px one's, so the two
            // baselines missed by exactly that difference — measured 3px on the frame at
            // 20260809-002525-948, label sitting low. TMP aligns a baseline natively, and both nodes
            // share one rect top and height, so pinning both to it lands the baselines on the same
            // line. This REMOVES a font-metric dependency rather than adding a computed offset: it
            // stays correct if either face is replaced, which a hand-derived nudge would not.
            stakeLabel.alignment = TextAlignmentOptions.BaselineLeft;
            stakeFigure.alignment = TextAlignmentOptions.BaselineRight;
            // 34px = the kit's own arithmetic: a 26px figure (`--st-size-stake`, line-height tight)
            // plus the 8px it puts above the fractions. The 30px box is deliberately taller than the
            // 26px line so a face whose ascent+descent exceeds its em cannot clip; it overlaps into
            // the 8px gap, which draws nothing. The old block advanced 32px for a 16px figure, so
            // being 1:1 here costs the flow +2px — see the S51 note below, this does not absorb.
            y -= 34f;
            float chipX = 14f;
            MakeChip(panel, "10%", chipX, y, () => slip.SetStakeFraction(0.10)); chipX += 76f;
            MakeChip(panel, "25%", chipX, y, () => slip.SetStakeFraction(0.25)); chipX += 76f;
            MakeChip(panel, "50%", chipX, y, () => slip.SetStakeFraction(0.50)); chipX += 76f;
            MakeChip(panel, "MAX", chipX, y, () => slip.SetStakeFraction(1.00));
            y -= 34f;
            // S82-am2 / S80-am2-cl2 (batch 107, 2026-08-17): nudge row (−$10/+$10 stake chips)
            // DELETED — redundant, the fraction chips above already set the stake. The 34px advance
            // above is untouched (it was always the chip row's own trailing gap into whatever comes
            // next); the payout label below now takes that position directly, so the nudge row's own
            // +32f hop is simply gone from the chain rather than left as a dead gap. `BetslipModel.
            // Nudge(double)` stays — only this player-facing row is deleted.
            // B1-5/M-06: PayoutFigure.jsx (kit) carries a "POTENTIAL PAYOUT" label — roman, fact
            // floor, --toner-3 — above the value; shipped had none. Added by moving the cursor down
            // to make room, not by touching the 31px wax figure's own size/colour/font or the hand-
            // laid highlight band math below it, which still reads off whatever `y` the figure ends
            // up at (PayoutFigure.jsx:6,10).
            LaptopUi.MakeText(panel, "PayoutLabel", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, y), new Vector2(300f, 16f), 13, TextAnchor.UpperLeft, LaptopOs.Muted,
                "POTENTIAL PAYOUT", _font);
            // S82 option A, harvest 3 of 3 — 18px advance for a 16px box, measured as 2.0px of air.
            // The label's own box already carries ~3px around its 13px glyphs, so the figure below it
            // is still separated by whitespace rather than by an advance.
            y -= 16f;
            TMP_Text payout = LaptopUi.MakeText(panel, "Payout", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, y), new Vector2(300f, 36f), 31, TextAnchor.UpperLeft, LaptopOs.MoneyGold, $"{LaptopUi.Money(slip.ToWin)}", _fontCond);
            // Hand-laid wax highlight behind the one loud figure (palette-surething.css
            // --wax-highlight-*): a thin amber band, tilted, sized from the figure's own measured
            // width the same way InkRingGeometry sizes a ring — plus the highlight's own -3/+5 left/
            // right overshoot, not the ring's symmetric +8. Created after the text, then the text is
            // moved back to the top of the sibling order so it still draws over the band.
            float highlightWidth = Mathf.Max(40f, payout.preferredWidth) + 8f;
            // S51 CLOSED — KIT FIDELITY (DD batch 66, 2026-08-14). The band sat 40px below the
            // figure's top (a 34px drop plus its own 6px), and PayoutFigure.jsx puts it at 36.1:
            // `bottom:-2px` against a line box of `--st-size-payout` 31px x `--st-lh-fig` 1.1.
            // That 3.9px WAS the structural overrun past T47's reservation — one cause, two
            // symptoms, since the frame also read the band as a detached rule under the figure
            // rather than the highlighter behind it that this comment describes.
            //
            // THE BAND MOVES, THE BLOCK DOES NOT — the DD refused all three seating options, and
            // "never shrink the figure to fit" still stands. Derived from the kit's own tokens
            // rather than written as 30.1 so the arithmetic is checkable against the source.
            const float payoutLineBoxPx = 31f * 1.1f;                          // --st-size-payout x --st-lh-fig
            const float bandBottomBelowFigureTop = payoutLineBoxPx + 2f;       // the kit's bottom:-2px => 36.1
            float bandTopOffset = bandBottomBelowFigureTop - LaptopOs.WaxHighlightHeight;
            RectTransform highlight = LaptopUi.MakePanel(panel, "PayoutHighlight", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(14f - 3f, y - bandTopOffset),
                new Vector2(highlightWidth, LaptopOs.WaxHighlightHeight), LaptopOs.MoneyGold);
            highlight.GetComponent<Image>().color = new Color(LaptopOs.MoneyGold.r, LaptopOs.MoneyGold.g,
                LaptopOs.MoneyGold.b, LaptopOs.WaxHighlightOpacity);
            highlight.localEulerAngles = new Vector3(0f, 0f, LaptopOs.WaxHighlightRotateDeg);
            payout.transform.SetAsLastSibling();
            y -= 40f;

            string blocker = slip.PlaceBlocker;
            // P3 (F_0.6.0 step 5). A refused COMBINATION is the one blocker no single string can
            // carry, so the model returns the machine token "refused:<Kind>" and requires the surface
            // to branch on `Refusal` and stamp the parts. Printing that token is a bug the model made
            // loud on purpose — and it was the live behaviour here until this branch existed, since
            // the line below upper-cases whatever `PlaceBlocker` returned.
            //
            // Legs are named with MarginLegSubject — the same call the rows above render — so the
            // instruction reads against the rows in front of him rather than needing translation
            // (S73-am5).
            //
            // HELD behind StampComposedRefusal while the sizing call is with the DD — see that
            // property. With the hold on, this control keeps its pre-P3 behaviour, which for a
            // refusal is the model's token; that is unreachable in play, because `Toggle` still
            // replaces and no additive gesture exists yet.
            TicketRefusal refusal = StampComposedRefusal ? slip.Refusal : null;
            string refusalRemedy = null;
            if (refusal != null)
            {
                blocker = RefusalCause(refusal);
                refusalRemedy = RefusalRemedy(refusal);
            }
            // MERGE (markets-2 × main, 2026-08-05): both intents kept, because they are orthogonal.
            //
            // From main — S18: a wax primary action is field + wax-ink + a 2px wax-deep edge, and
            // MakeWaxPrimary builds all three so this and LEAVE — NEXT ROUND cannot drift apart.
            // It wraps MakeButton and returns the same Button, so it takes the anchoring below
            // unchanged and the nested-reason split further down still resolves.
            //
            // From here — T47: the action stack STAYS ANCHORED. PLACE used to flow from the leg
            // list, which is how it walked down into LOCK as legs were added; an un-anchored stack
            // means the most consequential control in the game sits at a different height because
            // the player bet more. So it keeps the bottom anchor and PlaceBandY, not main's
            // top-anchored `y` cursor — the kit puts all three actions in one marginTop:auto group
            // (margin.jsx:44-52), which is what the anchored band is.
            Button placeButton = LaptopUi.MakeWaxPrimary(panel, "Place", "PLACE TICKET",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(14f, PlaceBandY), new Vector2(296f, 44f), 17,
                // S69: a blocked PLACE fills --ground-3 (SurfaceRaised), per PlaceAction.jsx — it was
                // --ground-2. The ruled distinction does real work: **PLACE is a field that has gone
                // inert, LOCK is a rule that has not yet been earned**, so PLACE keeps a fill and
                // LOCK keeps only its rule.
                blocker == null ? LaptopOs.MoneyGold : LaptopOs.SurfaceRaised,
                blocker == null ? LaptopOs.WaxInk : LaptopUi.Dim(LaptopOs.Muted),
                blocker == null ? () => { slip.Place(); _lockArmed = false; _armedRound = -1; _invalidate(); } : null, _fontCond,
                blocker == null && !boardFrozen);
            if (blocker != null)
            {
                // Nested inside the control for the same reason as LockReason (T47): a blocked
                // action states its cause on the control it blocks, and a reason with no position
                // of its own cannot drift onto anything above it. Same split as LOCK — label in the
                // upper band, reason in the lower — so the two blocked actions read identically.
                var placeRect = (RectTransform)placeButton.transform;
                var placeLabelRect = (RectTransform)placeRect.Find("Label");
                placeLabelRect.anchorMin = placeLabelRect.anchorMax = new Vector2(.5f, 1f);
                placeLabelRect.pivot = new Vector2(.5f, 1f);
                // S77 option (2), taken: two lines INSIDE the existing 44px box at 13px — "a real
                // option, not a last resort". The control does not grow, because every pixel of
                // control height comes 1:1 out of MarginFlowBudget and S51 has just shown that budget
                // is already overhung: a copy problem is not paid for out of a geometry budget that
                // is already over. The label yields 26px -> 16px instead; 16 + 13 + 13 + pad = 44.
                //
                // Two NODES rather than one wrapping node, so the break lands between cause and
                // remedy by construction rather than wherever the fitter happens to put it. Cause
                // above, remedy below — the order §3.3 states them in.
                bool twoLine = refusalRemedy != null;
                placeLabelRect.sizeDelta = new Vector2(296f, twoLine ? 16f : 26f);
                placeLabelRect.anchoredPosition = Vector2.zero;
                if (twoLine)
                    LaptopUi.MakeText(placeRect, "PlaceRemedy", new Vector2(.5f, 0f), new Vector2(.5f, 0f),
                        new Vector2(0f, 1f), new Vector2(288f, 14f), 13, TextAnchor.MiddleCenter,
                        LaptopOs.MoneyBad, refusalRemedy.ToUpperInvariant(), _font,
                        LaptopTrack.StampReason).enableWordWrapping = false;
                LaptopUi.MakeText(placeRect, "PlaceReason", new Vector2(.5f, 0f), new Vector2(.5f, 0f),
                    new Vector2(0f, twoLine ? 15f : 1f), new Vector2(288f, twoLine ? 14f : 17f), 13,
                    TextAnchor.MiddleCenter,
                    // S68: `.04em`, StampReason.jsx's own value. A blocked reason states a cause and
                    // a remedy (T47, owning doc §6) and is read as a sentence, not scanned as a
                    // label — which is why it takes the smallest tracking on the surface.
                    LaptopOs.MoneyBad, blocker.ToUpperInvariant(), _font,
                    LaptopTrack.StampReason).enableWordWrapping = false;
            }

            bool hasWorkingMarks = slip.Picks.Count > 0;
            string lockLabel = boardFrozen ? "THE ROUND IS LOCKED" : "LOCK IT IN";
            string lockReason = hasWorkingMarks ? "PLACE OR CLEAR THIS WORKING SLIP" : run.Tickets.Count == 0 ? "PLACE AT LEAST ONE TICKET" : string.Empty;
            bool canLock = !boardFrozen && lockReason.Length == 0;
            Button lockButton = LaptopUi.MakeButton(panel, "Lock", lockLabel, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(14f, LockBandY), new Vector2(296f, LockBandH), 16,
                // S69: a disabled LOCK is TRANSPARENT, per LockAction.jsx — it carried a fill and no
                // border, which is the inversion of the kit. Its rule is added below in both states,
                // because owning doc §2.2 calls LOCK "a 52px ruled control in both states"; only the
                // FILL is state-dependent.
                canLock ? LaptopOs.Ink : new Color(0f, 0f, 0f, 0f),
                canLock ? LaptopOs.White : LaptopOs.Muted,
                canLock ? () =>
                {
                    _lockArmed = false;
                    _armedRound = -1;
                    _host.director.LockRound();
                    _invalidate();
                } : null, _fontCond, canLock);

            // S69: the rule that makes LOCK "a 52px ruled control in both states" (owning doc §2.2).
            // Four 1px --rule edges rather than a border image, the same way MakeWaxPrimary builds
            // its 2px wax-deep edge — this surface has no rounded corners and no cards (§2.2), so an
            // edge is four rects and nothing more. Built as children of the button so they cannot be
            // occluded by a sibling drawn later, which is the defect the LockReason history below
            // records.
            {
                var lockEdge = (RectTransform)lockButton.transform;
                Vector2 lockSize = lockEdge.sizeDelta;
                const float ruleW = 1f;
                LaptopUi.MakePanel(lockEdge, "LockRuleTop", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    Vector2.zero, new Vector2(lockSize.x, ruleW), LaptopOs.Rule);
                LaptopUi.MakePanel(lockEdge, "LockRuleBottom", new Vector2(0f, 0f), new Vector2(0f, 0f),
                    Vector2.zero, new Vector2(lockSize.x, ruleW), LaptopOs.Rule);
                LaptopUi.MakePanel(lockEdge, "LockRuleLeft", new Vector2(0f, 0f), new Vector2(0f, 0f),
                    new Vector2(0f, ruleW), new Vector2(ruleW, lockSize.y - ruleW * 2f), LaptopOs.Rule);
                LaptopUi.MakePanel(lockEdge, "LockRuleRight", new Vector2(1f, 0f), new Vector2(1f, 0f),
                    new Vector2(0f, ruleW), new Vector2(ruleW, lockSize.y - ruleW * 2f), LaptopOs.Rule);
            }

            if (!canLock)
            {
                // T47: the reason belongs INSIDE the control, per LockAction.jsx:24 — label on top,
                // reason beneath it, both within the button's own rect.
                //
                // The history here matters, because the previous fix solved the wrong problem. The
                // two-stray-red-"P" defect was occlusion: the reason sat at y 26..48 while Skip
                // spans y 8..42, and Skip is built last, so it buried all but the top ~2px — one
                // glyph escaping past each edge of the 230px button, the leading and trailing "P"
                // of "PLACE … SLIP". The response was to move the reason ABOVE the Lock button
                // (y 110..130), which cured the occlusion and created a free-floating oxide band
                // that, at 4 legs, landed on the payout figure: the house's mark on the player's
                // money. Nesting it inside Lock cures both — a child cannot be occluded by a
                // sibling built later, and it can never travel, because it has no position of its
                // own any more.
                var lockRect = (RectTransform)lockButton.transform;
                // Split the control's interior: label in the upper band, reason in the lower.
                // MakeButton centres its Label across the whole rect, which would sit it on top of
                // the reason, so the label is re-anchored rather than the button made taller —
                // growing Lock would eat the flow region T47 requires be reserved.
                var lockLabelRect = (RectTransform)lockRect.Find("Label");
                lockLabelRect.anchorMin = lockLabelRect.anchorMax = new Vector2(.5f, 1f);
                lockLabelRect.pivot = new Vector2(.5f, 1f);
                lockLabelRect.sizeDelta = new Vector2(296f, 30f);
                lockLabelRect.anchoredPosition = Vector2.zero;
                LaptopUi.MakeText(lockRect, "LockReason", new Vector2(.5f, 0f), new Vector2(.5f, 0f),
                    new Vector2(0f, 2f), new Vector2(280f, 20f), 13, TextAnchor.MiddleCenter,
                    // S68: `.04em` — the same StampReason treatment as PLACE's blocker above.
                    LaptopOs.MoneyBad, lockReason, _font,
                    LaptopTrack.StampReason).enableWordWrapping = false;
            }
            // S68: this takes `.08em` via --st-track-rec, which is what SkipAction.jsx sets — NOT the
            // `.14em` action value the whole stack inherited when tracking became reachable. The
            // string is a label PLUS an instruction, and §4.3's principle is that short labels are
            // tracked uppercase while factual copy stays literal.
            //
            // **The headroom recovers by construction.** At `.14em` this ran ~228px inside its 230px
            // control; no authored string is shortened and no geometry moves to get the room back.
            // S50's yield order is spacing, then repetition, then nothing — the deficit existed
            // because spacing was spent where the kit did not authorise it, and it was about to be
            // paid for out of a string (T24: authored strings do not bend to measurements).
            LaptopUi.MakeButton(panel, "Skip", _lockArmed ? "PRESS AGAIN TO SKIP" : "SKIP ROUND — PRESS TWICE", new Vector2(.5f, 0f), new Vector2(.5f, 0f), new Vector2(0f, SkipBandY), new Vector2(230f, SkipBandH), 13, LaptopOs.Ink, _lockArmed ? LaptopOs.MoneyBad : LaptopOs.Muted,
                boardFrozen ? null : () =>
                {
                    if (!_lockArmed)
                    {
                        _lockArmed = true;
                        _armedRound = run.Round;
                        _invalidate();
                        return;
                    }
                    _lockArmed = false;
                    _armedRound = -1;
                    _host.director.LockRound();
                    _invalidate();
                }, _font, !boardFrozen, LaptopTrack.Records);
        }

        /// <summary>Renders <c>run.Tickets</c> as a top-down receipt stack starting at
        /// <paramref name="y"/>, sized off <paramref name="width"/> — the caller's own available
        /// width (E-07: previously always the 324px working margin, so the 296/280 numbers below were
        /// hardcoded to it; now <paramref name="width"/> is <see cref="BuildScrollingBody"/>'s
        /// rowWidth, so the same 14px/8px insets are applied to whatever width the ENTRY sheet's
        /// content actually has). Returns the new <paramref name="y"/> cursor below the last
        /// receipt.</summary>
        // S70(3): the staged receipt's bands, named once. The height was computed by the same
        // expression in TWO places — here and BuildScrollingBody's content math — which is the
        // duplication S67 exists to inventory, and it became load-bearing the moment the footer
        // changed the formula. One source now; the fixed-grid rule (§6, T51) is that a band's height
        // is derived at design time and read everywhere, never restated.
        private const float ReceiptHeaderH = 30f;   // identity + leg count, one band
        private const float ReceiptLegH = 18f;      // one leg row
        private const float ReceiptFooterH = 36f;   // STAKE / COMBINED / PAYS, key above value

        /// <summary>The staged receipt's own height. Read by the receipt and by the scroll body's
        /// content math, so the two cannot disagree about how tall a ticket is.</summary>
        private static float StagedReceiptHeight(Ticket ticket)
            => ReceiptHeaderH + ticket.Legs.Count * ReceiptLegH + ReceiptFooterH;

        /// <summary>TicketReceipt.jsx's footer row: three key-above-value cells, keys at
        /// `--st-track-label` in `--toner-3`, values condensed. **PAYS is the only wax on the
        /// receipt** — S3's law is that wax is money, and this is the money.</summary>
        private void BuildReceiptFooter(RectTransform receipt, Ticket ticket, double combined, float receiptWidth)
        {
            RectTransform footer = LaptopUi.MakePanel(receipt, "ReceiptFooter",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 2f),
                new Vector2(receiptWidth, ReceiptFooterH), new Color(0f, 0f, 0f, 0f));
            LaptopUi.MakeRule(footer, "ReceiptFooterRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, 0f), new Vector2(receiptWidth - 16f, 1f), LaptopOs.Rule);

            Cell("Stake", "STAKE", LaptopUi.Money(ticket.Stake), LaptopOs.White, 8f, TextAnchor.UpperLeft, 0f);
            Cell("Combined", "COMBINED", OddsFormat.American(combined), LaptopOs.White, 110f, TextAnchor.UpperLeft, 0f);
            Cell("Pays", "PAYS", LaptopUi.Money(ticket.PotentialPayout), LaptopOs.MoneyGold, -8f, TextAnchor.UpperRight, 1f);

            void Cell(string name, string key, string value, Color valueInk, float x, TextAnchor align, float anchorX)
            {
                var a = new Vector2(anchorX, 1f);
                LaptopUi.MakeText(footer, "Receipt" + name + "Key", a, a, new Vector2(x, -4f),
                    new Vector2(120f, 14f), 13, align, LaptopOs.Muted, key, _font, LaptopTrack.FieldKeys);
                LaptopUi.MakeText(footer, "Receipt" + name + "Value", a, a, new Vector2(x, -17f),
                    new Vector2(120f, 18f), 13, align, valueInk, value, _fontCond, LaptopTrack.Names);
            }
        }

        private float BuildStagedReceipt(RectTransform parent, Run run, float y, float width)
        {
            const float pad = 14f;
            float receiptWidth = width - pad * 2f;
            float receiptTextWidth = receiptWidth - 16f; // 8px inset each side, as before.
            float totalHeight = MeasureStagedTicketsHeight(run);
            RectTransform receipts = LaptopUi.MakePanel(parent, "StagedTickets", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(pad, y), new Vector2(receiptWidth, totalHeight),
                new Color(0f, 0f, 0f, 0f));
            float receiptY = 0f;
            for (int ticketIndex = 0; ticketIndex < run.Tickets.Count; ticketIndex++)
            {
                Ticket ticket = run.Tickets[ticketIndex];
                float receiptHeight = StagedReceiptHeight(ticket);
                RectTransform receipt = LaptopUi.MakePanel(receipts, "StagedTicket" + ticketIndex,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, receiptY),
                    new Vector2(receiptWidth, receiptHeight), LaptopOs.Surface);
                double combined = 1.0;
                for (int legIndex = 0; legIndex < ticket.Legs.Count; legIndex++)
                    combined *= ticket.Legs[legIndex].OfferedOdds;
                // S62: TICKET 02, without the round. A staged receipt is always the current round
                // and the masthead above it already states which — printing R1 here would restate
                // the run's scope, which is the thing S37 forbids. The LEDGER prints the round
                // because its list spans them.
                string identity = LaptopUi.TicketIdentity(ticket.Id, run.Round, ticketIndex, withRound: false);
                // S70(3): the header is the kit's grammar — identity, then count — each at its own
                // tracking, not one string carrying four facts. `TicketReceipt.jsx` puts the number
                // at `--st-track-action` and the leg count in its `key` style at `--st-track-label`,
                // and the money facts in a footer row. Collapsing all of it into one line lost the
                // distinction between what the house printed and what the sweat added.
                //
                // **Two consequences worth naming, because neither was the point of the ruling.**
                //
                // Wax returns to money. This line was drawn entirely in `--wax` because it carried
                // the payout, so the ticket's IDENTITY was rendered in the money ink — S3 says wax is
                // money, and an identity is not. With PAYS in the footer the header takes `--toner`
                // and only the payout stays wax.
                //
                // And the protected-suffix fit disappears with it. `FitLabelKeepingSuffix` existed
                // here to trim the label while never cutting the payout; identity and count are both
                // short, bounded strings that cannot overflow this width, so there is nothing left to
                // fit. The mechanism is not lost — the leg rows below still use it, which is where
                // S26's no-silent-truncation rule actually bites.
                LaptopUi.MakeText(receipt, "ReceiptHeader", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(8f, -4f), new Vector2(receiptTextWidth * .6f, 22f), 13,
                    TextAnchor.UpperLeft, LaptopOs.White, identity, _fontCond, LaptopTrack.Actions);
                LaptopUi.MakeText(receipt, "ReceiptLegCount", new Vector2(1f, 1f), new Vector2(1f, 1f),
                    new Vector2(-8f, -4f), new Vector2(receiptTextWidth * .35f, 22f), 13,
                    TextAnchor.UpperRight, LaptopOs.Muted,
                    $"{ticket.Legs.Count} {Pluralize(ticket.Legs.Count, "LEG")}", _font,
                    LaptopTrack.FieldKeys);
                for (int legIndex = 0; legIndex < ticket.Legs.Count; legIndex++)
                {
                    Leg leg = ticket.Legs[legIndex];
                    // C15/S28: `.08` — TicketReceipt.jsx:33 sets --st-track-rec on a receipt leg's
                    // market.
                    //
                    // **The same value goes to the measurement and to the render, and that is the
                    // whole reason MeasureWidth took a tracking parameter.** This is the one slot in
                    // the six groups where tracking changes what FITS: a wider string against an
                    // unchanged receiptTextWidth trims earlier. Measuring without the tracking it
                    // renders with would under-report by length x .08 x 13 and put the ellipsis in
                    // the wrong place — silently, on the screen S26 makes load-bearing at the point
                    // of spending. The odds suffix stays protected either way.
                    string ticketLegText = LaptopUi.FitLabelKeepingSuffix(_fontCond, $"{legIndex + 1}. ",
                        CompactLegLabel(leg.Matchup, leg.Selection),
                        $"  {OddsFormat.American(leg.OfferedOdds)}", 13, receiptTextWidth,
                        LaptopTrack.Records);
                    LaptopUi.MakeText(receipt, "TicketLeg" + legIndex, new Vector2(0f, 1f),
                        new Vector2(0f, 1f), new Vector2(8f, -26f - legIndex * 18f),
                        new Vector2(receiptTextWidth, 18f), 13, TextAnchor.UpperLeft, LaptopOs.TonerSecondary,
                        ticketLegText, _fontCond, LaptopTrack.Records);
                }
                BuildReceiptFooter(receipt, ticket, combined, receiptWidth);
                LaptopUi.MakeRule(receipt, "ReceiptRule", new Vector2(0f, 0f), new Vector2(0f, 0f),
                    Vector2.zero, new Vector2(receiptWidth, 2f));
                receiptY -= receiptHeight + 8f;
            }
            return y - totalHeight;
        }

        // Internal rather than private: OldSlipsApp's board header (S31, LedgerScreen()'s "N
        // RECORDS") reuses this same grammar call instead of re-deciding singular/plural itself.
        internal static string Pluralize(int count, string singular) => count == 1 ? singular : singular + "S";

        /// <summary>"N COMP"/"N COMPS" — the shop's second currency, grammatically agreed (S9 defect
        /// 3: "1 COMPS"). Decides singular off the FORMATTED value, not the raw double, so "1.0"
        /// reads the same as "1" and no fractional amount (e.g. "0.5") is ever mistaken for one.
        /// Routes the actual singular/plural call through the existing Pluralize rather than
        /// re-deciding it here.</summary>
        private static string FormatComps(double amount)
        {
            string formatted = amount.ToString("0.#", CultureInfo.InvariantCulture);
            return formatted + " " + Pluralize(formatted == "1" ? 1 : 0, "COMP");
        }

        /// <summary>Composes the width-starved working-margin and staged-receipt leg text from
        /// <see cref="MatchModel.Fields"/> (S22 ruling) instead of its own market-vocabulary switch:
        /// this maps onto the DS <c>MarginLeg</c>/<c>ReceiptLeg</c> shape (<c>{ team, market, price }</c>
        /// — <c>components/margin/MarginLeg.d.ts</c>). The DS's own composed example keeps the ladder
        /// value — <c>LedgerEntry.d.ts</c> prints "Bricklayers ML −260 · Over 2.5 −110" — so the
        /// over/under and its number must survive composition; see the head rule below.
        /// Team names are shortened the same way the board already does
        /// (<see cref="LaptopUi.TeamShort"/>); a moneyline pick never repeats the picked team's name
        /// a second time (the way the engine's own <see cref="MatchModel.DisplayLabel"/> does) —
        /// generalized here by checking whether Fields.Subject IS one of the two fixture teams,
        /// rather than switching on MarketKind. Internal so the PlayMode fixture can assert against
        /// the exact same production formula rather than a hand-kept duplicate that could quietly
        /// drift out of sync.</summary>
        /// <summary>The identity a margin leg row prints on its own first line — the string the
        /// player is looking at when the stamp names a leg.
        ///
        /// <para>Factored out of BuildSlip so the refusal stamp names legs with the SAME function the
        /// row renders, rather than a second copy of the expression. S73-am5 requires legs to be
        /// named "by the exact string on their own row, so he never has to translate an instruction
        /// against the rows in front of him" — a duplicated expression makes that true by coincidence
        /// and only until one of the two is edited. One function, two call sites, cannot drift.</para>
        ///
        /// <para>Not <see cref="CompactLegLabel"/>: that carries a fixture tail ("— HAWKS v RIVETS")
        /// which the margin row does not print, so naming a leg by it would be naming it by a string
        /// that is not on the row.</para></summary>
        /// <summary>**RELEASED 2026-08-14** — the hold this carried is over, and it is left as a
        /// constant `true` for one revision so the history is legible rather than silently deleted.
        ///
        /// <para>The first build of the stamp NAMED its legs and overflowed: 412–469px against a
        /// 288px box in the common case, 5.5–6.0× in the worst, six lines, and unbounded in principle
        /// because it scaled with whatever the board called a team. Allen held the wiring rather than
        /// ship a visibly overflowing control while the question was with the DD.</para>
        ///
        /// <para>S77 answered it by changing the SHAPE, not the size: the stamp states the act and
        /// its arity, and the legs are marked in the flow. That collapsed the population to
        /// arity-keyed forms which `MaxLegs = 4` bounds — so the gate that could only REPORT a
        /// measurement now ASSERTS one, over the whole population, exactly.</para></summary>
        internal const bool StampComposedRefusal = true;

        /// <summary>The Blocked control's stamped literal reason for a refused COMBINATION — cause
        /// and remedy, per §3.3's own row and S73-am4/am5. Composed here because the model emits
        /// parts and never English; `TicketRefusedException`'s message is a developer courtesy and
        /// says so itself.
        ///
        /// <para><b>The cause is N-VALUED and the forms are AUTHORED, not templated.</b> S73-am5:
        /// "two authored forms chosen by arity, never one template with a substituted word." Three
        /// legs can be jointly impossible with every pair among them fine, so "cannot both win" is
        /// not merely awkward at three — it is false. The two-leg and three-or-more sentences are
        /// written out separately below rather than switching a word inside one string.</para>
        ///
        /// <para><b>A duplicate and an impossibility take ONE treatment and TWO causes.</b> §3.3
        /// requires a *literal* reason, and one sentence vague enough to cover both is exactly what
        /// that word exists to prevent.</para>
        ///
        /// <para><b>The remedy is CONJUNCTIVE and spends the WHOLE set.</b> Remedies of up to three
        /// legs occur at the shipped κ across 645 refusals; dropping one element of a three-leg
        /// remedy leaves the slip refused, so a menu-shaped remedy would fail when followed — and
        /// S73-am4 requires a *verified* one. Every one of those 645 remedies placed successfully
        /// after being spent, so "TO PLACE" is a guarantee rather than a hope.</para>
        ///
        /// <para><b>NO LEG IS NAMED HERE (S77, 2026-08-14).</b> The first build of this named them,
        /// and it overflowed: three names inside a 296×44 control is unbounded in the worst case and
        /// the instruction is not. The DD ruled neither sizing nor shortening — the names do not
        /// belong in the stamp at all. **The stamp states the act and its arity; the legs it refers
        /// to are MARKED on their own rows in the flow directly above.** That is T69/T70's principle
        /// one control over: the subject is already on screen, so do not reprint it, and pointing at
        /// a referent serves the no-translation goal better than matching strings does.
        ///
        /// The check that makes it safe, and it passes: the flow is bounded by MaxLegs = 4 in a 370px
        /// region and does not scroll, so every marked row is on screen whenever the stamp is. It
        /// also dissolves the leg-name disjunction this lane reported — "TUSCALOOSA LONGHAULERS OR
        /// DRAW" cannot read as a menu inside a remedy that never says it.</para>
        ///
        /// <para><b>Removal order never reaches the player</b> (S73-am5). High-to-low is an
        /// implementation constraint of the caller, not part of the instruction.</para></summary>
        /// <summary>P5 — the slip's ONE relation statement, composed from `principal` (S78, batch 71).
        /// Toner, once per slip, stating what the legs SHARE. Null when nothing is statable.
        ///
        /// <para><b>The seven are a FAMILY and are not to be re-authored apart</b> (S78). The shape
        /// is not a template applied to save effort — the shape IS the claim: every one of these
        /// relations is literally *one shared thing settles both legs*, so the sentences differ
        /// exactly where the relations differ and are identical exactly where they are identical.
        /// After the first encounter he reads only the DIFFERENCE; four idioms would make him
        /// re-parse a whole sentence to learn something he already knows.</para>
        ///
        /// <para><b>Sign is carried</b> — reinforcing and opposing are opposite claims about the same
        /// shared thing, and one sentence per relation would state one of them falsely about the
        /// other. Seven sentences for four relations is the honest count.</para>
        ///
        /// <para><b>`ScorerSide` is deliberately NOT spoken</b> (S78, confirmed). Naming the team
        /// would be a name where the rubric asks for the relation, and the team is on both rows in
        /// front of him — S77's *mark, don't name* and T69/T70's *the subject is already on screen*.
        /// Where the two rows do not visibly share a club the sentence is under-determined; that case
        /// is reported by the evidence harness, and its remedy is at the MARK, never in this
        /// sentence.</para>
        ///
        /// <para><b>Null principal states NOTHING, and that is ruled correct</b> (S79). A null
        /// principal means the price did not move, so there is no cost to disclose — the statement
        /// exists to explain a price that shortened, and where nothing shortened nothing is owed. A
        /// high blank rate is what a correctly-behaving model looks like from the surface; a
        /// statement is never authored to fill it.</para></summary>
        internal static string RelationStatement(SameMatchPrice pricing, IReadOnlyList<Pick> picks)
        {
            if (pricing?.Principal == null) return null;
            Relation p = pricing.Principal.Value;
            bool opposing = p.Sign == RelationSign.Opposing;
            switch (p.Kind)
            {
                // S78: NOT the drafted "ONE OF THESE ALREADY COVERS THE OTHER" — that was refused as
                // a regression. §3.3 already authors this situation (a legal-but-pointless leg is NOT
                // a Blocked state and does not take Stamp; the machine states the fact in toner) and
                // these are ONE statement, not two — two code paths would have shipped two toner
                // sentences for one fact.
                //
                // The draft dropped the COST, which is the whole reason the statement exists: S17 is
                // about him being quietly charged for a leg that cannot lose. And it withheld WHICH
                // leg — right everywhere else in this batch, wrong here, because here the naming is
                // the actionable part. He may choose to rub that leg out.
                //
                // Said by POSITION, not by name. `Relation.Legs` is ordered by MEANING — Legs[0]
                // implies Legs[1], so Legs[1] is the leg that adds nothing — and that order is NOT
                // slip order, so the ordinal is derived from where the two legs actually sit on the
                // slip rather than from the relation's own array. Two authored forms, one per case.
                case RelationKind.Implies when p.Legs.Count >= 2 && p.Legs[1] > p.Legs[0]:
                    return "THE SECOND ADDS NOTHING; THE FIRST ALREADY COVERS IT.";
                case RelationKind.Implies:
                    return "THE FIRST ADDS NOTHING; THE SECOND ALREADY COVERS IT.";

                // GOALS / CORNERS / CARDS — a clean triple of countable match events, and that
                // parallelism is what makes the family read as one. `SCORELINE` was considered and
                // refused for breaking it.
                case RelationKind.SharedScoreline:
                    return opposing ? "THE SAME GOALS SETTLE THESE OPPOSITE WAYS."
                        : "THE SAME GOALS SETTLE BOTH.";
                // RELEASED by DD batch 72 — shipped exactly as approved, with no mark, after the
                // mark-treatment pre-commit was withdrawn by its author.
                //
                // THE MEASUREMENT THAT WAS RAISED AGAINST IT IS RECORDED HERE RATHER THAN DELETED,
                // because it is the kind of thing a later reader will otherwise re-derive from
                // scratch. Over 1,712 ScorerOfSide slips the two marked rows named the shared club
                // in ZERO of them, and the failure is not the under-determination S78 anticipated:
                //
                //     rows "MIDDLEMEN" + "LANCE MUFFIN", and the sentence's team is BRICKLAYERS.
                //
                // A scorer row names the PLAYER and never his club, so the only club on screen is
                // the OTHER team's. The seat that owns this copy has ruled it ships; this note is
                // the evidence trail, not a dissent still running.
                case RelationKind.ScorerOfSide:
                    return opposing ? "THE SAME TEAM'S GOALS SETTLE THESE OPPOSITE WAYS."
                        : "THE SAME TEAM'S GOALS SETTLE BOTH.";
                case RelationKind.SharedCount when p.Family == SelectionFamily.Corner:
                    return opposing ? "THE SAME CORNERS SETTLE THESE OPPOSITE WAYS."
                        : "THE SAME CORNERS SETTLE BOTH.";
                case RelationKind.SharedCount when p.Family == SelectionFamily.Card:
                    return opposing ? "THE SAME CARDS SETTLE THESE OPPOSITE WAYS."
                        : "THE SAME CARDS SETTLE BOTH.";

                // MutuallyExclusive is a refusal and Independent is nothing to state; neither is ever
                // nominated as principal. Silence rather than a manufactured sentence if that ever
                // changes — S79's rule is that a statement is never authored to fill a blank.
                default: return null;
            }
        }

        internal static string RefusalCause(TicketRefusal refusal)
        {
            int n = refusal.CauseLegs.Count;
            switch (refusal.Kind)
            {
                // AUTHORED PER ARITY, not one template with a numeral pushed into it. MaxLegs = 4
                // bounds the domain at two, three and four, so every form can be written out — which
                // is what makes S73-am5's "never one template with a substituted word" satisfiable
                // rather than merely aspired to. The both/all split is the load-bearing one: at three
                // legs the claim changes, since no pair among them need conflict.
                case RefusalKind.ImpossibleCombination when n <= 2: return "THESE TWO CANNOT BOTH WIN.";
                case RefusalKind.ImpossibleCombination when n == 3: return "THESE THREE CANNOT ALL WIN.";
                case RefusalKind.ImpossibleCombination: return "THESE FOUR CANNOT ALL WIN.";

                // A duplicate is NOT an impossibility and takes its own cause (§3.3 wants a literal
                // reason; one sentence covering both is what that word exists to prevent). The repeat
                // can win — it adds no risk while costing a full extra leg of margin.
                case RefusalKind.DuplicateSelection when n <= 2: return "THIS PICK IS HERE TWICE.";
                case RefusalKind.DuplicateSelection when n == 3: return "THIS PICK IS HERE THREE TIMES.";
                case RefusalKind.DuplicateSelection: return "THIS PICK IS HERE FOUR TIMES.";

                // The third cause. Sub-evens is about the PRICE and not about any leg — which is why
                // its CauseLegs names every leg: no proper subset prices any worse. No arity, because
                // the arity is not what is wrong.
                case RefusalKind.SubEvens: return "THIS PAYS LESS THAN IT COSTS.";
                default: return "THIS COMBINATION IS REFUSED.";
            }
        }

        /// <summary>The remedy half — the ACT and its ARITY, pointing at marks rather than naming
        /// legs (S77). The verb is the surface's own: the control on each row says `RUB OUT`, so the
        /// instruction and the thing that performs it are the same word.</summary>
        internal static string RefusalRemedy(TicketRefusal refusal)
        {
            switch (refusal.RemedyLegs.Count)
            {
                // S77-am: the previous line here read "NO RUB OUT FIXES THIS SLIP." and was refused
                // as a CAUSE-SHAPED string in the remedy slot — it told him only that the thing he
                // was about to try would not work and left him no act at all. "A refusal that closes
                // every door is the one case where he most needs to be told which door is open."
                //
                // The act is named in the word the ACTUAL control uses. There is no clear-all
                // control on this slip — the only removal control is the per-row RUB OUT — so the
                // act is rubbing out every leg, and this is bound to the control that exists rather
                // than to a CLEAR button that does not.
                case 0: return "RUB OUT EVERY LEG AND START OVER.";
                case 1: return "RUB OUT THE MARKED LEG TO PLACE.";
                // Conjunctive and arity-keyed. "BOTH" and "ALL THREE" are the whole set by
                // construction — there is no reading of either that spends less than all of it.
                // The plurals say MARKS rather than MARKED LEGS to hold the ≥13px line inside 288px:
                // S77's own order puts "a shorter authored form" first and geometry last, and the
                // longer plural forms measured 304px in a 288px box.
                case 2: return "RUB OUT BOTH MARKS TO PLACE.";
                default: return "RUB OUT ALL THREE MARKS TO PLACE.";
            }
        }

        /// <summary>THE ADDITIVE GESTURE — a pick STICKS.
        ///
        /// <para>Clicking an offer adds it; clicking the same offer again takes it off; and a second
        /// market on a match no longer replaces the first. That last clause is the whole change: the
        /// slip has been leg-addressed since sgp's model half merged, and this is the surface finally
        /// spending that capability.</para>
        ///
        /// <para><b>Composed here rather than by changing `Toggle`.</b> `BetslipModel` belongs to the
        /// sgp lane and `Toggle`'s replace behaviour is pinned by its tests — deliberately, so that
        /// changing the gesture is a decision someone makes rather than a regression someone
        /// discovers. So the gesture is built from the leg-addressed API at the surface that owns it:
        /// `Contains` to ask, `RemoveSelection` to take off, `AddLeg` to put on.</para>
        ///
        /// <para><b>One function, four call sites</b> — the two moneylines, the draw, and every
        /// market offer on the detail screen. The survey that found those sites found them wrong in
        /// four different ways because each had its own copy of the question; there is now one
        /// answer for them to share.</para>
        ///
        /// <para><b>`MaxLegs` still binds and `AddLeg` returns false at the cap.</b> A pick refused
        /// for the cap currently does nothing visible — see the report; no treatment is ruled for it
        /// and none is invented here.</para></summary>
        /// <summary>S85 — whether a price is still an OFFER, or has become a fact only.
        ///
        /// <para>§8's distinction is the ruling: the theatre prints FACTS and OFFERS, and a price
        /// cell is both at once — the house's line on an outcome, and a thing he may take. At the leg
        /// cap it is still true and no longer takeable, so it stops being an offer and remains a
        /// fact.</para>
        ///
        /// <para>A leg already on the slip stays takeable in the other direction: clicking it
        /// removes it, and removing is the remedy. **A remedy may never be disabled** (S73-am4).
        /// That is why this asks `Contains` first rather than testing the cap alone.</para></summary>
        private static bool OfferIsTakeable(BetslipModel slip, Run run, int matchupIndex,
            MarketSelection selection)
            => slip.Contains(matchupIndex, selection) || slip.Picks.Count < run.Config.MaxLegs;

        /// <summary>S85's treatment: the FIELD carries the offer, the TYPE carries the fact.
        ///
        /// <para><b>Not invented — S69's own move.</b> This surface already says "unavailable" by
        /// taking the fill away and leaving the control's type: *"a disabled LOCK is TRANSPARENT, per
        /// LockAction.jsx — it carried a fill and no border, which is the inversion of the kit."* A
        /// capped price is the same class of thing: legible, and not currently takeable.</para>
        ///
        /// <para><b>It satisfies §7's constraints by SUBTRACTION, which is why it needs no new
        /// ink.</b> §3.1's table is Wax, Biro and Stamp and nothing may borrow the three — so the
        /// treatment adds no colour at all. It removes one.</para>
        ///
        /// <para><b>And it is distinguishable from `frozen` in the only way that matters — the
        /// opposite channel.</b> `frozen` means the round is locked, so the price is no longer live
        /// information and the FACT dims. The cap means the slip is full, the price is still true,
        /// and the OFFER goes. One dims the type and keeps the field; the other keeps the type and
        /// removes the field. A player who learned the dim as "the board has closed" is never shown
        /// it for a full slip.</para></summary>
        private static Color OfferField(bool takeable) =>
            takeable ? LaptopOs.Ink : new Color(0f, 0f, 0f, 0f);

        private void PickOffer(BetslipModel slip, int matchupIndex, MarketSelection selection)
        {
            if (slip.Contains(matchupIndex, selection)) slip.RemoveSelection(matchupIndex, selection);
            else slip.AddLeg(matchupIndex, selection);
            _invalidate();
        }

        internal static string MarginLegSubject(Matchup matchup, MarketSelection selection)
        {
            MatchModel.MarketFields fields = MatchModel.Fields(matchup, selection);
            string away = LaptopUi.TeamShort(matchup.Away);
            string home = LaptopUi.TeamShort(matchup.Home);
            bool subjectIsHome = fields.Subject == matchup.Home.Name;
            bool subjectIsAway = !subjectIsHome && fields.Subject == matchup.Away.Name;
            string subjectRaw = subjectIsHome ? home : subjectIsAway ? away : fields.Subject;
            return string.IsNullOrEmpty(subjectRaw) ? fields.Line : subjectRaw;
        }

        internal static string CompactLegLabel(Matchup matchup, MarketSelection selection)
        {
            MatchModel.MarketFields fields = MatchModel.Fields(matchup, selection);
            string away = LaptopUi.TeamShort(matchup.Away);
            string home = LaptopUi.TeamShort(matchup.Home);
            bool subjectIsHome = fields.Subject == matchup.Home.Name;
            bool subjectIsAway = !subjectIsHome && fields.Subject == matchup.Away.Name;
            string subject = subjectIsHome ? home : subjectIsAway ? away : fields.Subject;
            string fixtureTail = subjectIsHome ? $"v {away}" : subjectIsAway ? $"v {home}" : $"{away} v {home}";

            // The head must UNIQUELY identify the selection: two selections on one matchup may
            // never compose to the same row. Which field carries that discriminator genuinely
            // differs by market, so the surface picks it rather than the engine pre-composing —
            // Line carries it for the ladders ("OVER 2.5 GOALS") and the scorer ("VALE ANYTIME"),
            // Market carries it for BTTS ("BTTS — YES" / "— NO"), and moneyline needs the picked
            // team. Composing subject+Market uniformly reads fine and is wrong: it collapses
            // OVER 2.5 and UNDER 3.5 into one identical row, so the bettor cannot tell which side
            // of the total they backed. Pinned by MarketFieldsTests' uniqueness fact.
            string head = selection.Kind switch
            {
                MarketKind.Moneyline => $"{subject} {fields.Market}",
                MarketKind.BothTeamsToScore => fields.Market,
                _ => fields.Line,
            };
            return $"{head} — {fixtureTail}";
        }

        /// <summary>Sizes and places an ink ring so it overshoots the text it frames by a fixed
        /// 8px on every edge, per docs/design/direction-concepts/assets/ASSETS.md, instead of a box
        /// that was only ever sized for the widest word that box could hold. text's RectTransform
        /// must use anchor/pivot (1,1) (top-right), matching how "LegState" is built. Internal so
        /// the PlayMode fixture can assert the exact same geometry the render pass uses.</summary>
        internal static (Vector2 position, Vector2 size) InkRingGeometry(TMP_Text text,
            float overshoot = 8f, float minWidth = 40f, float minHeight = 18f)
        {
            Vector2 size = new Vector2(Mathf.Max(minWidth, text.preferredWidth) + overshoot * 2f,
                Mathf.Max(minHeight, text.preferredHeight) + overshoot * 2f);
            Vector2 position = text.rectTransform.anchoredPosition + new Vector2(overshoot, overshoot);
            return (position, size);
        }

        private void MakeModifier(RectTransform parent, string label, TicketModifier modifier, BetslipModel slip,
            float x, float y)
        {
            bool selected = slip.Modifier == modifier;
            LaptopUi.MakeButton(parent, label, selected ? label + " ✓" : label,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, y), new Vector2(132f, 27f), 13,
                selected ? LaptopOs.Accent : LaptopOs.SurfaceRaised, LaptopOs.White,
                () => { slip.ToggleModifier(modifier); _invalidate(); }, _font);
        }

        private void MakeChip(RectTransform parent, string label, float x, float y, Action onClick,
            float width = 68f, TMP_FontAsset font = null)
        {
            LaptopUi.MakeButton(parent, "Chip" + label, label, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x, y), new Vector2(width, 32f), 13, LaptopOs.SurfaceRaised, LaptopOs.White,
                () => { onClick(); _invalidate(); }, font != null ? font : _font);
        }

        private TMP_Text _mirrorMarket;

        /// <summary>Refresh only the TV-owned market-availability line. Score, clock and
        /// probability deliberately remain exclusive to the broadcast surface.</summary>
        public void UpdateMirrorDisplay(RevealedView view)
        {
            if (view == null || !view.HasTicket) return;
            if (_mirrorMarket != null)
            {
                _mirrorMarket.text = view.MarketSuspended
                    ? "TV REVEAL IN PROGRESS  ·  MARKET SUSPENDED"
                    : "TV REVEAL IN PROGRESS  ·  MARKET LIVE";
                _mirrorMarket.color = view.MarketSuspended ? LaptopOs.Muted : LaptopOs.TonerSecondary;
            }
        }

        private void BuildMyBets(RevealedView view)
        {
            RectTransform board = LaptopUi.MakePanel(_root, "MyBetsBoard", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, -140f), new Vector2(700f, 530f), LaptopOs.Ink);
            RectTransform margin = LaptopUi.MakePanel(_root, "MyBetsMargin", new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(0f, -140f), new Vector2(324f, 530f), LaptopOs.Ink);
            // S34: same shared ruled-paper ground as the working margin — see BuildSlip.
            LaptopUi.MakeMarginRuledPaper(margin, "RuledPaper");
            // F2: same sheet/margin seam as every other screen — see BuildSlip's SheetDivider.
            LaptopUi.MakeRule(margin, "SheetDivider", new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(2f, 530f), LaptopOs.Rule);
            LaptopUi.MakeText(board, "MirrorOwnership", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -8f), new Vector2(670f, 26f), 16, TextAnchor.UpperLeft,
                LaptopOs.White, "MY BETS  ·  READ-ONLY TV MIRROR", _font);
            LaptopUi.MakeText(board, "MirrorRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -36f), new Vector2(670f, 24f), 13, TextAnchor.UpperLeft,
                LaptopOs.Muted, "ONLY STATES ALREADY SHOWN ON THE TV APPEAR HERE.", _font);
            LaptopUi.MakeRule(board, "MirrorHeaderRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -68f), new Vector2(700f, 2f));
            _mirrorMarket = null;
            if (view == null || !view.HasTicket)
            {
                RectTransform waiting = LaptopUi.MakePanel(board, "MirrorWaiting",
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -94f),
                    new Vector2(672f, 72f), new Color(0f, 0f, 0f, 0f));
                LaptopUi.MakeText(waiting, "MyBetsEmpty", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    Vector2.zero, new Vector2(672f, 36f), 16, TextAnchor.UpperLeft,
                    LaptopOs.Muted, "NO TV-REVEALED TICKET YET", _font);
                LaptopUi.MakeText(waiting, "MyBetsEmptyRemedy", new Vector2(0f, 1f),
                    new Vector2(0f, 1f), new Vector2(0f, -36f), new Vector2(672f, 28f),
                    13, TextAnchor.UpperLeft,
                    LaptopOs.TonerSecondary, "PLACE AND LOCK A TICKET, THEN WATCH THE BROADCAST.", _font);
                BuildMirrorMargin(margin, null);
                return;
            }

            _mirrorMarket = LaptopUi.MakeText(board, "MirrorMarket", new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(-14f, -8f), new Vector2(330f, 26f), 13,
                TextAnchor.UpperRight, LaptopOs.TonerSecondary,
                string.Empty, _font);
            UpdateMirrorDisplay(view);
            RectTransform tickets = LaptopUi.MakePanel(board, "MirrorTickets", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, -70f), new Vector2(700f, 460f),
                new Color(0f, 0f, 0f, 0f));
            int count = Math.Max(1, view.Tickets.Count);
            float columnWidth = (672f - (count - 1) * 8f) / count;
            for (int i = 0; i < view.Tickets.Count; i++)
                BuildMirrorTicket(tickets, view.Tickets[i], new Vector2(14f + i * (columnWidth + 8f), 0f),
                    columnWidth);
            BuildMirrorMargin(margin, view);
        }

        /// <summary>The ticket-level state word (S23 ruling: RIDING is ticket-level only). Never
        /// returns "LIVE" — that word belongs to <see cref="LegStateWord"/> alone. Extracted so
        /// every call site prints the same word the same way, and so the contract is testable.</summary>
        internal static string TicketStateWord(RevealedTicketState state)
            => state == RevealedTicketState.Won ? "GREEN"
                : state == RevealedTicketState.Lost ? "DEAD"
                : state == RevealedTicketState.CashedOut ? "CASHED OUT" : "RIDING";

        /// <summary>The leg-level state word (S23 ruling: LIVE is leg-level only). Never returns
        /// "RIDING" — that word belongs to <see cref="TicketStateWord"/> alone.</summary>
        internal static string LegStateWord(RevealedLegState state)
            => state == RevealedLegState.Won ? "GREEN"
                : state == RevealedLegState.Lost ? "DEAD"
                : state == RevealedLegState.Voided ? "VOID"
                : state == RevealedLegState.Live ? "LIVE" : "PENDING";

        /// <summary>The leg-level state INK, extracted for the same two reasons
        /// <see cref="LegStateWord"/> was: every call site renders one state one way, and the mapping
        /// becomes something a test can hold.
        ///
        /// S65 is why it exists. The mapping lived as an inline ternary inside BuildMirrorLeg with a
        /// shared `else` covering PENDING and VOID, so it rendered PENDING at `--toner-2` where S43
        /// and `RevealedState.jsx` both require `--toner-3` — and nothing could assert the contract
        /// because there was no contract to point at. That is S67's shape one level down: a value
        /// composed by hand where a named thing should produce it.</summary>
        internal static Color LegStateInk(RevealedLegState state)
            => state == RevealedLegState.Won ? LaptopOs.MoneyGold
                : state == RevealedLegState.Lost ? LaptopOs.Muted
                : state == RevealedLegState.Live ? LaptopOs.White
                : state == RevealedLegState.Voided ? LaptopOs.TonerSecondary
                : LaptopOs.Muted;

        private void BuildMirrorTicket(RectTransform parent, RevealedTicket ticket, Vector2 position,
            float width)
        {
            RectTransform card = LaptopUi.MakePanel(parent, "MirrorTicket" + ticket.Index,
                new Vector2(0f, 1f), new Vector2(0f, 1f), position, new Vector2(width, 448f),
                LaptopOs.Ink);
            string state = TicketStateWord(ticket.State);
            Color stateColor = ticket.State == RevealedTicketState.Won ? LaptopOs.MoneyGold
                : ticket.State == RevealedTicketState.Lost ? LaptopOs.Muted
                : ticket.State == RevealedTicketState.CashedOut ? LaptopOs.MoneyGold
                : LaptopOs.White;
            // S64: this printed `TICKET 1` — the one unpadded identity on a surface whose other two
            // print the kit's form. It was not a different rule; it was a hand-built string that
            // never called the shared formatter, so S62 landed on TicketIdentity and this screen
            // never heard it. `TicketReceipt.prompt.md` names MY BETS by name as one of the three
            // screens that component serves, and `TicketReceipt.d.ts` gives the form as `TICKET 01`:
            // one component, three screens, one identity.
            //
            // `withRound: false` is the staged receipt's call for the staged receipt's reason — MY
            // BETS mirrors the current round and the masthead above already states it, so a round
            // qualifier here is S37 restatement.
            //
            // The mirror carries no engine ticket id (S35c: it holds only what the TV released), so
            // this takes the helper's fallback path. **The round argument is unread while withRound
            // is false** — it is 0 rather than a real round because there is no honest round to pass,
            // and anything that flips this call to `true` must supply one first.
            //
            // Identity and the terminal state word are both condensed per
            // TicketReceipt.jsx / RevealedState.jsx.
            string identity = LaptopUi.TicketIdentity(null, 0, ticket.Index, withRound: false);
            LaptopUi.MakeText(card, "TicketTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, -8f), new Vector2(width - 16f, 24f), 16, TextAnchor.UpperLeft,
                stateColor, $"{identity}  ·  {state}", _fontCond);
            LaptopUi.MakeText(card, "TicketFigures", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, -32f), new Vector2(width - 16f, 22f), 13, TextAnchor.UpperLeft,
                LaptopOs.TonerSecondary,
                $"STAKE {LaptopUi.Money(ticket.Stake)}  ·  PAYS {LaptopUi.Money(ticket.PotentialPayout)}",
                _font);
            LaptopUi.MakeRule(card, "TicketRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -58f), new Vector2(width, 2f));
            for (int i = 0; i < ticket.Legs.Count; i++)
            {
                RevealedLeg leg = ticket.Legs[i];
                string label = string.IsNullOrEmpty(leg.MarketLabel) ? leg.TeamName : leg.MarketLabel;
                BuildMirrorLeg(card, ticket.Index, leg, label, -64f - i * 58f, width);
            }
        }

        private void BuildMirrorLeg(RectTransform parent, int ticketIndex, RevealedLeg leg,
            string label, float y, float width)
        {
            Color ground = leg.State == RevealedLegState.Lost ? LaptopOs.Surface : LaptopOs.Ink;
            RectTransform row = LaptopUi.MakePanel(parent, "MirrorLeg" + leg.Index,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, y),
                new Vector2(width, 54f), ground);
            string state = LegStateWord(leg.State);
            // S65: PENDING is `--toner-3`. It used to fall into a shared `else` with VOID at
            // `--toner-2` — one step too bright, measured 158,154,138 on `04b-my-bets-pending`
            // against the token's 110,107,94.
            //
            // The mapping now lives in LegStateInk rather than inline here, because the collapsed
            // `else` WAS the defect: the kit distinguishes PENDING (`--toner-3`) from VOID
            // (`--toner-2`) and a fallthrough cannot. Moving the fallthrough would have fixed PENDING
            // by breaking VOID.
            //
            // PENDING now sits level with DEAD, escalated rather than shipped, and the ruling holds
            // it: DEAD carries three channels to PENDING's one (word, oxide strike, row drained to
            // .55 — owning doc §3.3), `--toner-3` is this system's structure tone and a leg that has
            // not happened is structure, and the TV reaches the same answer independently with NEXT
            // at L1 against L0.
            Color stateColor = LegStateInk(leg.State);
            // RevealedLeg.jsx's "team" slot (occupied here by either the team name or the market
            // label, whichever the leg carries) and its price are condensed; the state word matches
            // RevealedState.jsx.
            LaptopUi.MakeText(row, "LegLabel", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, -4f), new Vector2(width - 16f, 22f), 13, TextAnchor.UpperLeft,
                leg.State == RevealedLegState.Lost ? LaptopOs.Muted : LaptopOs.White, label, _fontCond);
            LaptopUi.MakeText(row, "LegPrice", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, -27f), new Vector2(84f, 22f), 13, TextAnchor.UpperLeft,
                stateColor, leg.AmericanOdds, _fontCond);
            TMP_Text stateText = LaptopUi.MakeText(row, "LegState", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-8f, -27f), new Vector2(112f, 22f), 13, TextAnchor.UpperRight,
                stateColor, state, _fontCond);
            int identity = ticketIndex * 17 + leg.Index;
            if (leg.State == RevealedLegState.Won)
            {
                Sprite ring = ResolvePriceRing(identity);
                if (ring != null)
                {
                    // "GREEN" is far narrower than the 112px box the state label sits in, and the
                    // label is right-aligned flush with the box's edge — a ring built to that box's
                    // width, not the word's, has its widest point land exactly on the last letters.
                    // Size and place the ring from the text's own measured bounds instead.
                    (Vector2 position, Vector2 size) = InkRingGeometry(stateText);
                    LaptopUi.MakeSprite(row, "GreenRing", ring, new Vector2(1f, 1f),
                        new Vector2(1f, 1f), position, size, LaptopOs.MoneyGold);
                }
            }
            else if (leg.State == RevealedLegState.Lost)
            {
                Sprite strike = ResolveStrike(identity);
                if (strike != null)
                {
                    // Was a fixed 112x46 box at a hand-picked (-4,-20) offset — the strike-a
                    // sprite's own native size, not derived from "DEAD" at all. "DEAD" measures
                    // well under half that width, so the mark ran far past the word on its left
                    // side while landing short of a full 8px overshoot on the right. Same fix as
                    // GreenRing above: derive position/size from the state text's own measured
                    // bounds via InkRingGeometry so the strike overshoots the actual word, not a
                    // stale asset-sized box.
                    (Vector2 position, Vector2 size) = InkRingGeometry(stateText);
                    LaptopUi.MakeSprite(row, "DeadStrike", strike, new Vector2(1f, 1f),
                        new Vector2(1f, 1f), position, size, LaptopOs.MoneyBad);
                }
            }
            LaptopUi.MakeRule(row, "LegRule", new Vector2(0f, 0f), new Vector2(0f, 0f),
                Vector2.zero, new Vector2(width, 1f));
        }

        private void BuildMirrorMargin(RectTransform margin, RevealedView view)
        {
            // S60 + S61, and the two rulings finish each other.
            //
            // S60 put this header in biro over a 2px --biro-deep rule, as S33 specifies on every
            // destination. The objection is worth keeping in the source: the tally is the TV's, so
            // why is its header in HIS ink? Because the biro marks the COLUMN as his, not the
            // content as his choice. The margin is his column on every screen; what fills it varies.
            //
            // S61: this screen stated its scope FOUR times on one 1024px canvas — board header,
            // board subline, margin header, margin subline. The board header owns "read-only TV
            // mirror" and owns it well; three restatements followed it. The margin's subline
            // (READ ONLY · NO SCORE · NO PROBABILITY) is deleted, and the header no longer
            // re-asserts ownership either — "TV-OWNED" was the third statement of a fact the two
            // lines above it already make, and the biro now marks the column anyway. What is left
            // names what the column CONTAINS, which is the one thing nothing else on the screen says.
            //
            // Shape worth remembering: S58 asked this column to stop restating the SHEET and it did
            // — then restated the SCOPE instead. A restatement removed from one register can
            // reappear in another.
            //
            // With the subline gone this is structurally identical to the ledger's RECORD header, so
            // it is now literally the same component (LaptopUi.MakeMarginHeader) rather than a
            // second copy holding the same values.
            float headerHeight = LaptopUi.MakeMarginHeader(margin, "TALLY", _fontCond);
            if (view == null || !view.HasTicket)
            {
                LaptopUi.MakeText(margin, "MirrorMarginEmpty", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, -(headerHeight + 12f)), new Vector2(296f, 44f), 13,
                    TextAnchor.UpperLeft, LaptopOs.Muted, "THE TV HAS NOT RELEASED A RECEIPT.", _font);
                return;
            }
            // S58: this column printed one block per ticket — `TICKET 1 · DEAD` over
            // `2 LEGS · $35 → $97` — while the sheet ~500px to its left printed `TICKET 1 · DEAD`
            // over `STAKE $35 · PAYS $97`. The same four facts, twice, on one screen, with a leg
            // count as the only addition and the sheet right there to count them on. S37 forbids
            // exactly that.
            //
            // The margin's job here is **run context**: what he has on this round, what is still
            // live, and what comes back if it all lands. None of those are on the sheet, because the
            // sheet is per-ticket and these are sums across it.
            //
            // Derived only from the TV-owned mirror, never from the engine — the header two lines
            // above promises READ ONLY and the causal-mirror rules mean this column may not know
            // anything the TV has not released.
            int riding = 0;
            double atRisk = 0.0;
            double ifAllLand = 0.0;
            for (int i = 0; i < view.Tickets.Count; i++)
            {
                RevealedTicket ticket = view.Tickets[i];
                if (ticket.State != RevealedTicketState.Riding) continue;
                riding++;
                atRisk += ticket.Stake;
                ifAllLand += ticket.PotentialPayout;
            }

            // AT RISK counts only tickets still riding, because a dead ticket's stake is not at
            // risk — it is gone, and the sheet already says so. A fully-resolved round therefore
            // reads $0 here, which is the true answer to "what is still live".
            LaptopUi.MakeMarginRow(margin, "TallyTickets", "TICKETS THIS ROUND",
                view.Tickets.Count.ToString(CultureInfo.InvariantCulture), LaptopOs.White,
                -headerHeight, MirrorTallyRowHeight, _font, _fontCond);
            LaptopUi.MakeMarginRow(margin, "TallyAtRisk", $"AT RISK  ·  {riding} RIDING",
                LaptopUi.Money(atRisk), LaptopOs.White,
                -(headerHeight + MirrorTallyRowHeight), MirrorTallyRowHeight, _font, _fontCond);
            LaptopUi.MakeMarginRow(margin, "TallyIfAllLand", "IF EVERYTHING LANDS",
                LaptopUi.Money(ifAllLand), LaptopOs.MoneyGold,
                -(headerHeight + MirrorTallyRowHeight * 2f), MirrorTallyRowHeight, _font, _fontCond);
        }

        // The same 38px row the ledger's RECORD margin uses, so the two margins scan identically.
        // The rows' own top now comes from MakeMarginHeader's returned height rather than a second
        // hand-kept offset — S61 shortened that header and a duplicated constant would have been
        // the next thing to drift out of step with it.
        private const float MirrorTallyRowHeight = 38f;

        private void BuildRewards(Run run)
        {
            RectTransform board = LaptopUi.MakePanel(_root, "RewardsBoard", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, -140f), new Vector2(700f, 530f), LaptopOs.Ink);
            RectTransform margin = LaptopUi.MakePanel(_root, "RewardsMargin", new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(0f, -140f), new Vector2(324f, 530f), LaptopOs.Ink);
            // S34: same shared ruled-paper ground as the working margin — see BuildSlip.
            LaptopUi.MakeMarginRuledPaper(margin, "RuledPaper");
            // F2: same sheet/margin seam as every other screen — see BuildSlip's SheetDivider.
            LaptopUi.MakeRule(margin, "SheetDivider", new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(2f, 530f), LaptopOs.Rule);
            LaptopUi.MakeText(board, "RewardsTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -8f), new Vector2(672f, 26f), 18, TextAnchor.UpperLeft,
                LaptopOs.White, "REWARDS  ·  CLAIM FORM", _font);
            LaptopUi.MakeText(board, "RewardsSub", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -36f), new Vector2(672f, 24f), 13, TextAnchor.UpperLeft,
                LaptopOs.Muted, run.Phase == Phase.Shop
                    ? "RULED OFFERS  ·  ONE PURCHASE PER LINE"
                    : "REWARDS DESK CLOSED", _font);
            LaptopUi.MakeRule(board, "RewardsHeaderRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -68f), new Vector2(700f, 2f));

            if (run.Phase != Phase.Shop)
            {
                LaptopUi.MakeText(board, "RewardsLocked", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, -94f), new Vector2(672f, 28f), 16, TextAnchor.UpperLeft,
                    LaptopOs.Muted, "REWARDS ARE LOCKED", _font);
                LaptopUi.MakeText(board, "RewardsLockedRemedy", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, -126f), new Vector2(672f, 28f), 13, TextAnchor.UpperLeft,
                    LaptopOs.TonerSecondary, "SETTLE THE ROUND TO OPEN THIS DESK.", _font);
            }
            else if (run.ShopOffers.Count + run.ConsumableOffers.Count == 0)
            {
                LaptopUi.MakeText(board, "RewardsEmpty", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, -94f), new Vector2(672f, 28f), 16, TextAnchor.UpperLeft,
                    // S72-p (batch 28): a sentence takes a stop, a fragment does not. This one has a
                    // verb — "remain" — so it is a sentence and was missing its stop. Members 1, 2, 8
                    // and 10 are fragments and correctly carry none.
                    LaptopOs.Muted, "NO OFFERS REMAIN ON THIS SHEET.", _font);
                LaptopUi.MakeText(board, "RewardsEmptyRemedy", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, -126f), new Vector2(672f, 28f), 13, TextAnchor.UpperLeft,
                    LaptopOs.TonerSecondary, "LEAVE REWARDS TO CONTINUE THE RUN.", _font);
            }
            else
            {
                // S9 defect 5: at 72px pitch, the dealt maximum (PassiveOfferCount 4 +
                // ConsumableOfferCount 3 = 7 rows) needs ~500px against the 456px actually free
                // below the header rule, so the last row ran under the taskbar. 60px (56px row + 4px
                // gap, same ratio as before) fits all seven with room to spare, matching the
                // narrower row height BuildRewardOffer/BuildConsumableOffer now use.
                // Rows are no longer a fixed pitch: each one is as tall as its own rule text, which
                // is the point of Allen's 2026-07-31 ruling — descriptions keep their rule text and
                // the board shows however many offers fit. Nothing may run off the sheet and nothing
                // may be silently dropped, so the count that did not fit is stated in place.
                //
                // BoardBottomPadding leaves the last rule clear of the tray rather than flush to it.
                //
                // S25(amended)/S27: the offer list itself now scrolls, same as LEDGER/ENTRY — but
                // S17's own cap survives it under S25(amended)'s express exception ("N NOT SHOWN
                // ... binds only a deliberately capped list — REWARDS under S17"). The two are
                // different questions, answered by different mechanisms below: S17 decides which
                // offers get INSTANTIATED at all (an offer's own rule text — cost, downside — is
                // never truncated to make room; show fewer offers instead, exactly as before,
                // same 446px budget, same pessimistic EstimateOfferHeight, same shown/total
                // accounting, unchanged by this edit). S27 decides how the resulting — possibly
                // shorter-than-viewport — content is PRESENTED: reachable by scroll rather than
                // hard-clipped, with a rail iff it actually runs long. Because EstimateOfferHeight
                // is deliberately pessimistic (see its own comment: "an over-estimate costs at
                // most one offer that would just have fitted"), real rendered content is typically
                // a little SHORTER than the 446px budget it was built against, so in the ordinary
                // case the rail never appears at all — the scroll body is the honest backstop for
                // the estimate's own slack, not a second, looser cap replacing S17's.
                const float boardBottomPadding = 10f;
                const float offersTop = -74f;
                float viewportHeight = 530f - boardBottomPadding - (-offersTop);
                RectTransform content = LaptopUi.MakeScrollBody(board, "RewardsScroll",
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, offersTop),
                    new Vector2(700f, viewportHeight), out RectTransform scrollHost,
                    out ScrollRect scrollRect);

                float y = 0f;
                float floorY = -viewportHeight;
                int shown = 0;
                int total = run.ShopOffers.Count + run.ConsumableOffers.Count;

                for (int i = 0; i < run.ShopOffers.Count; i++)
                {
                    if (y - EstimateOfferHeight(run.ShopOffers[i].Description) < floorY) break;
                    y -= BuildRewardOffer(content, run, run.ShopOffers[i], i, y);
                    shown++;
                }
                for (int i = 0; i < run.ConsumableOffers.Count; i++)
                {
                    if (y - EstimateOfferHeight(run.ConsumableOffers[i].Description) < floorY) break;
                    y -= BuildConsumableOffer(content, run, run.ConsumableOffers[i], i, y);
                    shown++;
                }
                LaptopUi.FinishScrollBody(scrollHost, scrollRect, content, -y, viewportHeight);

                if (shown < total)
                {
                    // Hiding a purchasable offer without saying so would be the same class of
                    // untruth as the truncation this replaced: the screen would read as the whole
                    // shop. Stated as a plain fact, in toner — it is the house's document telling
                    // him what is on it, not a blocked action, so it is not the oxide stamp.
                    // C19 / S25 amended: REWARDS is the one list a ruling deliberately caps (S17),
                    // so its count line prints in --toner (LaptopOs.White) — was TonerSecondary
                    // (--toner-2), one step dimmer than the comment above already said it should be.
                    // Kept OUTSIDE the scroll body (fixed to the board, not the content): it is
                    // the sheet's own statement about the list, not a row inside it.
                    LaptopUi.MakeText(board, "OffersNotShown", new Vector2(0f, 0f), new Vector2(0f, 0f),
                        new Vector2(14f, 8f), new Vector2(672f, 20f), 13, TextAnchor.LowerLeft,
                        LaptopOs.White,
                        $"{total - shown} MORE {Pluralize(total - shown, "OFFER")} THIS ROUND — NOT ENOUGH SHEET",
                        _font);
                }
            }

            BuildRewardsMargin(margin, run);
        }

        /// <summary>
        /// Predicts a row's height before it is built, so the board can decide whether the next
        /// offer fits without creating it and then destroying it again. Mirrors the real layout in
        /// BuildRewardOffer: 29px of name block, the wrapped description, 9px of tail.
        ///
        /// Deliberately pessimistic — it assumes a slightly narrower line than the 430px box really
        /// allows, so it over-estimates rather than under-estimates. An over-estimate costs at most
        /// one offer that would just have fitted; an under-estimate puts a row under the taskbar,
        /// which is the defect this replaced.
        /// </summary>
        private float EstimateOfferHeight(string description)
        {
            const float lineHeight = 17f;
            const float charsPerLine = 66f;
            int lines = Mathf.Max(1, Mathf.CeilToInt((description ?? string.Empty).Length / charsPerLine));
            return 29f + lines * lineHeight + 9f;
        }

        // S27: every scrolling body reserves the rail's own 4px on the right, whether or not the
        // rail ends up drawn (LaptopUi.RailReserve) — REWARDS' offer rows now live inside one
        // (BuildRewards), so their own full-width elements (the row panel, OfferRule) are sized
        // to the same narrower content column rather than the board's full 700px, matching the
        // ledger's LedgerRowWidth. Elements anchored from an edge (Affordability/BuyReason/Buy,
        // all pivoted right) already reflow with it via Unity's own anchor math and need no
        // separate constant.
        private const float RewardsRowWidth = 700f - LaptopUi.RailReserve;

        private float BuildRewardOffer(RectTransform parent, Run run, RelicDefinition offer, int index, float y)
        {
            RectTransform row = LaptopUi.MakePanel(parent, "RewardOffer" + index, new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, y), new Vector2(RewardsRowWidth, 56f), LaptopOs.Ink);
            bool enoughComps = offer.Price <= run.Comps;
            bool hasSlot = run.OwnedRelics.Count < run.Config.RelicSlots;
            bool canBuy = enoughComps && hasSlot && run.Phase == Phase.Shop;
            string reason = !hasSlot ? "RELIC SLOTS FULL"
                : !enoughComps ? "NEED " + FormatComps(offer.Price - run.Comps)
                : "AFFORDABLE";
            // Name and price are condensed per OfferEntry.jsx; description, reason and the BUY button
            // itself stay on the data face.
            LaptopUi.MakeText(row, "OfferName", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -5f), new Vector2(430f, 22f), 15, TextAnchor.UpperLeft,
                LaptopOs.White, offer.Name.ToUpperInvariant(), _fontCond);
            // Ruled by Allen 2026-07-31: an offer's description NEVER loses its rule text. These
            // are the mechanics the player is spending comps on, and the earlier one-line fit kept
            // each entry's opening clause while dropping the rule itself — on two entries it
            // dropped the downside, which is worse than clipping, because it reads as complete.
            // The copy is rendered whole and the row grows to hold it; the board shows however many
            // offers fit and says how many it could not (see BuildRewards).
            TMP_Text description = LaptopUi.MakeText(row, "OfferDescription", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(14f, -29f), new Vector2(430f, 22f), 13,
                TextAnchor.UpperLeft, LaptopOs.TonerSecondary, offer.Description, _font);
            float descriptionHeight = Mathf.Max(18f, description.preferredHeight);
            description.rectTransform.sizeDelta = new Vector2(430f, descriptionHeight);
            float rowHeight = 29f + descriptionHeight + 9f;
            row.sizeDelta = new Vector2(RewardsRowWidth, rowHeight);
            // S9 defect 1: a price is a printed figure, not the house's mark — wax regardless of
            // affordability. The BLOCKED reason beside it stays oxide; that IS the house acting.
            LaptopUi.MakeText(row, "Affordability", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-124f, -5f), new Vector2(118f, 22f), 13, TextAnchor.UpperRight,
                LaptopOs.MoneyGold, FormatComps(offer.Price), _fontCond);
            LaptopUi.MakeText(row, "BuyReason", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-124f, -29f), new Vector2(160f, 20f), 13, TextAnchor.UpperRight,
                // S68: `.04em`. Same StampReason component — this one carries toner when the offer is
                // affordable and the house's stamp when it is not, but it is factual copy either way.
                canBuy ? LaptopOs.TonerSecondary : LaptopOs.MoneyBad, reason, _font,
                LaptopTrack.StampReason);
            // Top-anchored, not centred: the row's height now follows its description, and a
            // vertically centred button would slide down the taller rows and sit on the copy.
            LaptopUi.MakeButton(row, "Buy", "BUY", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-14f, -4f), new Vector2(96f, 40f), 13,
                // Law Two: BUY spends money, so an affordable one is wax with punched-out type,
                // exactly like PLACE TICKET and LEAVE. Biro is reserved for marks the player made;
                // a purchase control is not one. (LEAVE was corrected under S9 defect 2 and this
                // was the same violation one control over, missed because no offer was affordable
                // in the capture the defect list was written from.)
                canBuy ? LaptopOs.MoneyGold : LaptopOs.SurfaceRaised,
                canBuy ? LaptopOs.WaxInk : LaptopUi.Dim(LaptopOs.Muted),
                canBuy ? () =>
                {
                    string error = _host.director.TryBuyRelic(index);
                    SetShopMessage("RELIC PURCHASE RECORDED", error);
                    _invalidate();
                } : null, _font, canBuy);
            LaptopUi.MakeRule(row, "OfferRule", new Vector2(0f, 0f), new Vector2(0f, 0f),
                Vector2.zero, new Vector2(RewardsRowWidth, 2f));
            return rowHeight;
        }

        private float BuildConsumableOffer(RectTransform parent, Run run, ConsumableDefinition offer,
            int index, float y)
        {
            RectTransform row = LaptopUi.MakePanel(parent, "ConsumableOffer" + index, new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, y), new Vector2(RewardsRowWidth, 56f), LaptopOs.Ink);
            bool enoughComps = offer.Price <= run.Comps;
            bool hasSlot = run.OwnedConsumables.Count < run.Config.ConsumableSlots;
            bool canBuy = enoughComps && hasSlot && run.Phase == Phase.Shop;
            string reason = !hasSlot ? "CHARM SLOTS FULL"
                : !enoughComps ? "NEED " + FormatComps(offer.Price - run.Comps)
                : "AFFORDABLE";
            // Same OfferEntry.jsx split as BuildRewardOffer above. "SINGLE USE" is a trailing
            // qualifier riding along with the name rather than a field label, so the combined string
            // stays on the condensed face as one run rather than being split.
            LaptopUi.MakeText(row, "OfferName", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -5f), new Vector2(430f, 22f), 15, TextAnchor.UpperLeft,
                LaptopOs.White, offer.Name.ToUpperInvariant() + "  ·  SINGLE USE", _fontCond);
            // Ruled by Allen 2026-07-31: an offer's description NEVER loses its rule text. These
            // are the mechanics the player is spending comps on, and the earlier one-line fit kept
            // each entry's opening clause while dropping the rule itself — on two entries it
            // dropped the downside, which is worse than clipping, because it reads as complete.
            // The copy is rendered whole and the row grows to hold it; the board shows however many
            // offers fit and says how many it could not (see BuildRewards).
            TMP_Text description = LaptopUi.MakeText(row, "OfferDescription", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(14f, -29f), new Vector2(430f, 22f), 13,
                TextAnchor.UpperLeft, LaptopOs.TonerSecondary, offer.Description, _font);
            float descriptionHeight = Mathf.Max(18f, description.preferredHeight);
            description.rectTransform.sizeDelta = new Vector2(430f, descriptionHeight);
            float rowHeight = 29f + descriptionHeight + 9f;
            row.sizeDelta = new Vector2(RewardsRowWidth, rowHeight);
            // S9 defect 1: price is wax regardless of affordability; see BuildRewardOffer above.
            LaptopUi.MakeText(row, "Affordability", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-124f, -5f), new Vector2(118f, 22f), 13, TextAnchor.UpperRight,
                LaptopOs.MoneyGold, FormatComps(offer.Price), _fontCond);
            LaptopUi.MakeText(row, "BuyReason", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-124f, -29f), new Vector2(160f, 20f), 13, TextAnchor.UpperRight,
                // S68: `.04em`. Same StampReason component — this one carries toner when the offer is
                // affordable and the house's stamp when it is not, but it is factual copy either way.
                canBuy ? LaptopOs.TonerSecondary : LaptopOs.MoneyBad, reason, _font,
                LaptopTrack.StampReason);
            // Top-anchored, not centred: the row's height now follows its description, and a
            // vertically centred button would slide down the taller rows and sit on the copy.
            LaptopUi.MakeButton(row, "Buy", "BUY", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-14f, -4f), new Vector2(96f, 40f), 13,
                // Law Two: BUY spends money, so an affordable one is wax with punched-out type,
                // exactly like PLACE TICKET and LEAVE. Biro is reserved for marks the player made;
                // a purchase control is not one. (LEAVE was corrected under S9 defect 2 and this
                // was the same violation one control over, missed because no offer was affordable
                // in the capture the defect list was written from.)
                canBuy ? LaptopOs.MoneyGold : LaptopOs.SurfaceRaised,
                canBuy ? LaptopOs.WaxInk : LaptopUi.Dim(LaptopOs.Muted),
                canBuy ? () =>
                {
                    string error = _host.director.TryBuyConsumable(index);
                    SetShopMessage("CHARM PURCHASE RECORDED", error);
                    _invalidate();
                } : null, _font, canBuy);
            LaptopUi.MakeRule(row, "OfferRule", new Vector2(0f, 0f), new Vector2(0f, 0f),
                Vector2.zero, new Vector2(RewardsRowWidth, 2f));
            return rowHeight;
        }

        private void BuildRewardsMargin(RectTransform margin, Run run)
        {
            LaptopUi.MakeText(margin, "RewardsTally", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -8f), new Vector2(296f, 28f), 20, TextAnchor.UpperLeft,
                LaptopOs.MoneyGold,
                $"{run.Comps.ToString("0.#", CultureInfo.InvariantCulture)} COMPS", _fontCond);
            LaptopUi.MakeText(margin, "RewardsResources", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -38f), new Vector2(296f, 24f), 13, TextAnchor.UpperLeft,
                LaptopOs.Muted,
                $"RELICS {run.OwnedRelics.Count}/{run.Config.RelicSlots}  ·  CHARMS {run.OwnedConsumables.Count}/{run.Config.ConsumableSlots}",
                _font);
            LaptopUi.MakeRule(margin, "RewardsMarginRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -68f), new Vector2(324f, 2f));

            float y = -76f;
            if (run.OwnedRelics.Count + run.OwnedConsumables.Count == 0)
            {
                LaptopUi.MakeText(margin, "RewardsInventoryEmpty", new Vector2(0f, 1f),
                    new Vector2(0f, 1f), new Vector2(14f, y), new Vector2(296f, 42f), 13,
                    // S72-p (batch 28): the mirror of the change on RewardsEmpty. This has no verb —
                    // it is a fragment — so the stop it carried comes off. S71-am3 separately
                    // ratifies this member as built in every other respect: nothing is bought from
                    // this screen in this state, so there is no honest next action to pair it with
                    // and a manufactured imperative would be worse than none.
                    TextAnchor.UpperLeft, LaptopOs.Muted, "NO OWNED REWARDS TO SELL BACK", _font);
                y -= 46f;
            }
            for (int i = 0; i < run.OwnedRelics.Count; i++)
            {
                int index = i;
                RelicDefinition relic = run.OwnedRelics[i];
                RectTransform row = LaptopUi.MakePanel(margin, "OwnedRelic" + i, new Vector2(0f, 1f),
                    new Vector2(0f, 1f), new Vector2(0f, y), new Vector2(324f, 44f), LaptopOs.Ink);
                LaptopUi.MakeText(row, "OwnedName", new Vector2(0f, .5f), new Vector2(0f, .5f),
                    new Vector2(14f, 0f), new Vector2(176f, 36f), 13, TextAnchor.MiddleLeft,
                    LaptopOs.White, relic.Name.ToUpperInvariant(), _fontCond);
                LaptopUi.MakeButton(row, "Sell", $"+{run.GetResaleValue(relic):0.#}C  SELL",
                    new Vector2(1f, .5f), new Vector2(1f, .5f), new Vector2(-14f, 0f),
                    new Vector2(112f, 32f), 13, LaptopOs.SurfaceRaised, LaptopOs.MoneyBad,
                    () =>
                    {
                        string error = _host.director.TrySellRelic(index);
                        SetShopMessage("RELIC SELL-BACK RECORDED", error);
                        _invalidate();
                    }, _font, run.Phase == Phase.Shop);
                LaptopUi.MakeRule(row, "OwnedRule", new Vector2(0f, 0f), new Vector2(0f, 0f),
                    Vector2.zero, new Vector2(324f, 1f));
                y -= 44f;
            }
            for (int i = 0; i < run.OwnedConsumables.Count; i++)
            {
                int index = i;
                ConsumableDefinition consumable = run.OwnedConsumables[i];
                RectTransform row = LaptopUi.MakePanel(margin, "OwnedConsumable" + i,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, y),
                    new Vector2(324f, 44f), LaptopOs.Ink);
                LaptopUi.MakeText(row, "OwnedName", new Vector2(0f, .5f), new Vector2(0f, .5f),
                    new Vector2(14f, 0f), new Vector2(176f, 36f), 13, TextAnchor.MiddleLeft,
                    LaptopOs.White, consumable.Name.ToUpperInvariant(), _fontCond);
                LaptopUi.MakeButton(row, "Sell",
                    $"+{(consumable.Price * run.Config.SellBackFraction):0.#}C  SELL",
                    new Vector2(1f, .5f), new Vector2(1f, .5f), new Vector2(-14f, 0f),
                    new Vector2(112f, 32f), 13, LaptopOs.SurfaceRaised, LaptopOs.MoneyBad,
                    () =>
                    {
                        string error = _host.director.TrySellConsumable(index);
                        SetShopMessage("CHARM SELL-BACK RECORDED", error);
                        _invalidate();
                    }, _font, run.Phase == Phase.Shop);
                LaptopUi.MakeRule(row, "OwnedRule", new Vector2(0f, 0f), new Vector2(0f, 0f),
                    Vector2.zero, new Vector2(324f, 1f));
                y -= 44f;
            }

            if (run.OwnsConsumable("ask_manager"))
                LaptopUi.MakeButton(margin, "Manager", "ASK MANAGER — REDEAL",
                    new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(14f, 114f),
                    new Vector2(296f, 40f), 13, LaptopOs.SurfaceRaised, LaptopOs.Accent,
                    () =>
                    {
                        string error = _host.director.TryPlayManager();
                        SetShopMessage("MANAGER REDEALT THE SHEET", error);
                        _invalidate();
                    }, _font, run.Phase == Phase.Shop);

            if (_shopError.Length > 0)
                LaptopUi.MakeText(margin, _shopMessageIsError ? "ShopError" : "ShopResult",
                    new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(14f, 74f),
                    new Vector2(296f, 32f), 13, TextAnchor.LowerLeft,
                    _shopMessageIsError ? LaptopOs.MoneyBad : LaptopOs.MoneyGold,
                    _shopError, _font);

            bool canLeave = run.Phase == Phase.Shop;
            // S9 defect 2: this is the primary, phase-advancing action on the screen — not a mark HE
            // chose — so it is wax like PLACE TICKET (Law Two), not biro. WaxInk is the same
            // punched-out-type-on-wax convention PLACE TICKET uses, not the general document Ink.
            // S18: routed through MakeWaxPrimary so the field + wax-ink + 2px wax-deep edge treatment
            // is written once, shared with PLACE TICKET.
            LaptopUi.MakeWaxPrimary(margin, "LeaveRewards", "LEAVE — NEXT ROUND",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(14f, 12f),
                new Vector2(296f, 48f), 15, canLeave ? LaptopOs.MoneyGold : LaptopOs.SurfaceRaised,
                canLeave ? LaptopOs.WaxInk : LaptopUi.Dim(LaptopOs.Muted),
                canLeave ? () =>
                {
                    _host.director.ExitShop();
                    _shopError = string.Empty;
                    _shopMessageIsError = false;
                    _invalidate();
                } : null, _font, canLeave);
            if (!canLeave)
                LaptopUi.MakeText(margin, "LeaveReason", new Vector2(.5f, 0f), new Vector2(.5f, 0f),
                    new Vector2(0f, 2f), new Vector2(296f, 18f), 13, TextAnchor.LowerCenter,
                    LaptopOs.MoneyBad, "SETTLE THE ROUND FIRST", _font);
        }

        private void SetShopMessage(string success, string error)
        {
            _shopMessageIsError = !string.IsNullOrEmpty(error);
            _shopError = _shopMessageIsError ? error : success;
        }

        private void BuildTaskbar()
        {
            NotebookChrome.BuildTray(_root, _root.sizeDelta.x, _font,
                NotebookChrome.Running.Sportsbook, null, _ledger, _home);
        }

        private void MakeDot(RectTransform parent, string name, Vector2 position, Color color)
        {
            RectTransform dot = LaptopUi.MakePanel(parent, name, new Vector2(0f, 1f), new Vector2(0f, 1f),
                position, new Vector2(8f, 8f), color);
            dot.GetComponent<Image>().raycastTarget = false;
        }
    }

    /// <summary>Read-only ledger over settled tickets still exposed by the current Run.</summary>
    internal sealed class OldSlipsApp
    {
        private readonly RectTransform _root;
        private readonly TMP_FontAsset _font;
        private readonly TMP_FontAsset _fontCond; // see SportsbookApp's field comment — same seam
        private readonly Action _home;
        private readonly Action _sportsbook;
        // S31: drives the reused four-tab strip's navigation — clicking FORM/ENTRY/MY BETS/
        // REWARDS from LEDGER jumps straight to that destination, same as SectionTabs.jsx's own
        // onSelect (app.jsx:120). Distinct from _sportsbook above, which only drops the running
        // app to whichever tab it last showed (the tray's "SURETHING" slot).
        private readonly Action<SportsbookApp.Tab> _selectTab;

        public OldSlipsApp(RectTransform root, TMP_FontAsset font, TMP_FontAsset fontCond, Action home, Action sportsbook,
            Action<SportsbookApp.Tab> selectTab)
        {
            _root = root;
            _font = font;
            _fontCond = fontCond;
            _home = home;
            _sportsbook = sportsbook;
            _selectTab = selectTab;
        }

        public void Render(Run run)
        {
            LaptopUi.ClearChildren(_root);
            LaptopUi.MakePanel(_root, "LedgerBacking", Vector2.zero, Vector2.zero, Vector2.zero,
                _root.sizeDelta, LaptopOs.Ink);
            BuildLedgerChrome(run);

            RectTransform board = LaptopUi.MakePanel(_root, "LedgerBoard", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, -140f), new Vector2(700f, 530f), LaptopOs.Ink);
            RectTransform margin = LaptopUi.MakePanel(_root, "LedgerMargin", new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(0f, -140f), new Vector2(324f, 530f), LaptopOs.Ink);

            // Retention consumed. This read `run.Tickets`, which `ExitShop` clears every round, so a
            // player who bet in rounds 1-3 and opened the LEDGER in round 4 met an empty screen
            // captioned SETTLED TICKETS · THIS RUN — the screen contradicting itself, and the defect
            // the engine work was approved to fix. `run.SettledTickets` is the retained history.
            //
            // Both lists, not just the retained one. The engine folds a round's tickets into
            // SettledTickets at Settle(), so a ticket that goes terminal mid-round — a cash-out, or
            // a dead-leg loss (S43) — is in `Tickets` and not yet in `SettledTickets`. Reading only
            // the retained list would make those vanish from the ledger until the round settled,
            // which is a new defect traded for the old one. De-duplicated by reference because
            // after Settle() and before ExitShop() the same tickets are legitimately in both.
            var settled = new List<Ticket>();
            for (int i = 0; i < run.SettledTickets.Count; i++)
                if (run.SettledTickets[i].State != TicketState.Open) settled.Add(run.SettledTickets[i]);
            for (int i = 0; i < run.Tickets.Count; i++)
            {
                Ticket live = run.Tickets[i];
                if (live.State != TicketState.Open && !settled.Contains(live)) settled.Add(live);
            }

            // S31: LedgerScreen()'s own 44px --ground-2 board header — SETTLED TICKETS · THIS RUN
            // left, N RECORDS right-flushed, a --rule bottom border. This is now the one place on
            // the screen stating that the list below is scoped to settled current-run records —
            // the old "LedgerScope" caption that said the same thing in different words (S9
            // defect 7's caption, not its meta) is retired rather than kept beside it, which would
            // restate the header's own fact (S37).
            BuildLedgerBoardHeader(board, settled.Count);

            // A separate padded-string column-head row (a pre-S31 leftover, one string between the
            // board header and the first entry) stays deleted. It never worked — one string padded
            // with spaces in a proportional face, so it could not line up with the columns it
            // claimed to head; measured on the populated capture, its STAKE head sat at x=122
            // against its value at x=351. That mechanism is gone for good, not replaced.
            //
            // S38 ruled violation, the kit is wrong · DD 2026-08-02 batch 7: STAKE/RETURNED belong
            // to the list, not the row — LedgerEntry.jsx:20,24 prints both once per record, so they
            // repeated three times on a three-record frame, invisible at N=1 (a component preview
            // is an N=1 surface). The build was 1:1 with that defect. The fix is not the deleted
            // padded-string row above; it is real column heads inside the 44px --ground-2 board
            // header S31 already built (BuildLedgerBoardHeader below), positioned at the exact same
            // x/width as the figures beneath them (LedgerStakeX/LedgerReturnedX — S40 re-derives
            // both against this same header band), not a hand-padded guess.
            //
            // S42/S27: the settled list itself scrolls, under the fixed header above — no cap, no
            // truncation, no sub-row clipped by the tray. S40's leg sub-rows made row cost a
            // function of leg count (up to 3 tickets x 6 legs against this board), which is the
            // overflow S42 names as its reason to rule now rather than wait for it to be
            // hypothetical. Every settled record stays reachable per C19: hiding a kept record is
            // indistinguishable from never having kept it.
            const float ledgerScrollTop = -44f; // just below the 44px board header
            const float ledgerViewportHeight = 530f + ledgerScrollTop; // 486 — the rest of the board
            RectTransform content = LaptopUi.MakeScrollBody(board, "LedgerScroll",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, ledgerScrollTop),
                new Vector2(700f, ledgerViewportHeight), out RectTransform scrollHost,
                out ScrollRect scrollRect);

            // S41: renamed from knownWinPayout — it is no longer wins-only. A retained cash-out is
            // money the player got back and belongs in RETURNED with the payouts.
            double settledStake = 0.0;
            double knownReturned = 0.0;
            float y = -8f; // top pad inside the scroll content (was -52 = -44 header - 8 pad)
            for (int i = 0; i < settled.Count; i++)
            {
                Ticket ticket = settled[i];
                BuildLedgerTicket(content, ticket, i, run.Round, y);
                // S32: LedgerEntry.jsx's own borderBottom (--rule-w solid --rule-soft) is the
                // separator now, drawn as this entry's own bottom edge inside BuildLedgerTicket —
                // so entries sit flush and the next one starts exactly one entry height down, not
                // one entry height plus the blank 2px gap this file used to leave.
                y -= LedgerEntryHeight(ticket);
                settledStake += ticket.Stake;
                if (ticket.State == TicketState.Won)
                    knownReturned += ticket.PotentialPayout;
                // S41: a retained cash-out is a known return and joins the sum. A record whose
                // amount is genuinely unknowable simply contributes nothing — it does not suppress
                // the total, and its own cell carries the absence (BuildLedgerTicket).
                else if (ticket.State == TicketState.CashedOut && ticket.CashedOutFor.HasValue)
                    knownReturned += ticket.CashedOutFor.Value;
                // VOID (F_0.6.0 step 5): the returned stake joins the sum, by S41's own test — it is
                // a KNOWN return, and only genuinely unknowable amounts contribute nothing.
                //
                // This is the money half of the void arm and it was the worst of the three: the
                // stake already counted in `settledStake` above while the refund counted nowhere, so
                // a refunded ticket read on the totals row as a ticket the player had simply lost.
                // The record's own cell was merely wrong; this line was wrong ABOUT MONEY.
                else if (ticket.State == TicketState.Voided)
                    knownReturned += ticket.Stake;
            }
            LaptopUi.FinishScrollBody(scrollHost, scrollRect, content, -y, ledgerViewportHeight);
            if (settled.Count == 0)
            {
                LaptopUi.MakeText(board, "LedgerEmpty", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, -140f), new Vector2(672f, 30f), 16, TextAnchor.UpperLeft,
                    LaptopOs.Muted, "NO SETTLED TICKETS IN THE CURRENT RUN", _font);
                LaptopUi.MakeText(board, "LedgerEmptyScope", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, -174f), new Vector2(672f, 48f), 13, TextAnchor.UpperLeft,
                    LaptopOs.TonerSecondary,
                    "THIS LEDGER DOES NOT STORE CROSS-RUN HISTORY.\nOPEN TICKETS ARE NOT SETTLED RECORDS.",
                    _font);
            }

            BuildRecordSummary(margin, settled.Count, settledStake, knownReturned);

            // F2: the same sheet/margin seam every screen carries — see BuildSlip's SheetDivider.
            // Built LAST here, and only here, for a reason worth keeping: BuildRecordSummary above
            // lays an opaque full-bleed 324x530 panel over this whole margin, so a divider created
            // before it is painted out. It was, and the seam was missing from this one screen while
            // rendering correctly on every other — caught by sampling x=700 on both, not by eye.
            // If this call moves back up, the seam disappears again and nothing will fail.
            LaptopUi.MakeRule(margin, "SheetDivider", new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(2f, 530f), LaptopOs.Rule);
            BuildLedgerTray();
        }

        private void BuildLedgerChrome(Run run)
        {
            RectTransform chrome = LaptopUi.MakePanel(_root, "Chrome", new Vector2(0f, 1f),
                new Vector2(0f, 1f), Vector2.zero, new Vector2(1024f, 140f), LaptopOs.Ink);
            NotebookChrome.BuildRail(chrome, 1024f, _font);

            // S31: the persistent four-tab strip is the sportsbook's own — reused via
            // SportsbookApp.BuildTabStrip rather than fabricated as a single "LEDGER" tab
            // standing in FORM's slot, which is exactly the failure the never-rebuilds clause
            // exists to prevent. `active: null` because LEDGER is not one of the four tabs this
            // strip carries, so every tab renders unselected — SectionTabs.jsx's own behaviour
            // when `active` matches none of `tabs`. The meta line already reads READ ONLY here
            // (F4); it is not repeated anywhere else on this screen (S9 defect 7 / S37).
            RectTransform tabs = LaptopUi.MakePanel(chrome, "FormTabs", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, -34f), new Vector2(1024f, 38f), LaptopOs.Surface);
            SportsbookApp.BuildTabStrip(tabs, null, run.Phase, "READ ONLY", _font, _selectTab);

            RectTransform masthead = LaptopUi.MakePanel(chrome, "FormMasthead", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, -72f), new Vector2(1024f, 68f), LaptopOs.Ink);
            // F1: Masthead.jsx's own border-bottom (--rule-w-strong solid var(--rule)); same
            // duplication note as TabsRule above.
            LaptopUi.MakeRule(masthead, "MastheadRule", new Vector2(0f, 0f), new Vector2(0f, 0f),
                Vector2.zero, new Vector2(1024f, 2f), LaptopOs.Rule);
            // Width trimmed from the pre-S31 420px to 300px (matching SportsbookApp.BuildChrome's
            // own Brand box exactly — "LEDGER" needs far less room than the sportsbook's own brand
            // already fits in 300). The old 420px had no neighbour to clear (the pre-S31 right-side text
            // started at local x=648); BuildRunFigures below now starts at x=398, and 420 would
            // have overlapped it by up to 38px.
            LaptopUi.MakeText(masthead, "Brand", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(16f, -8f), new Vector2(300f, 28f), 26, TextAnchor.UpperLeft,
                LaptopOs.White, "LEDGER", _fontCond, LaptopTrack.Names);
            // F4: "READ ONLY" is said once, by the tabs meta above, not here. S37: the live round
            // number appears exactly once on the surface — every other destination states it in
            // this exact slot (SportsbookApp.BuildChrome's "Run" text, ROUND R OF N · ...), and it
            // now does here too, replacing the condensed "ROUND R · BANK $X" string that used to
            // occupy the masthead's run-figures slot instead (see BuildRunFigures below).
            // Width trimmed from the pre-S31 520px (room for the old "CURRENT RUN · SETTLED
            // TICKETS ONLY" wording, which had no neighbour to its right) to 370px: BuildRunFigures
            // below now occupies this masthead too, starting at local x=398, and 520 would have
            // overlapped it by up to 139px — an invisible box collision the fact-floor/target
            // rules forbid even when neither string is long enough to visibly touch.
            //
            // S37's live instance (DD 2026-08-02 batch 7): this subline used to carry its own
            // " · SETTLED TICKETS ONLY" clause, restating the screen's scope in the masthead's
            // slot — the exact duplication S37 exists to forbid, just missed on the first pass
            // because the board header's own "SETTLED TICKETS · THIS RUN" (BuildLedgerBoardHeader)
            // wasn't read side by side with this string. Deleted; the masthead states only the
            // run's scope (the live round number), the board header states the screen's.
            LaptopUi.MakeText(masthead, "Scope", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(17f, -38f), new Vector2(370f, 20f), 13, TextAnchor.UpperLeft,
                LaptopOs.Muted, $"ROUND {run.Round} OF {run.Config.Rounds}", _font);
            // S31: the masthead's run figures — unchanged from the rest of the surface. Reuses
            // SportsbookApp's own mechanism (BANK/TARGET/TICKETS) instead of the single condensed
            // "ROUND R · BANK $X" string this slot used to carry.
            SportsbookApp.BuildRunFigures(masthead, run, _fontCond);
        }

        // S38+S39+S40 column geometry (DD 2026-08-02 batch 7 — one change, one commit). Canon
        // (LedgerEntry.jsx) still names five flex cells — number(112) / legs(flex:1) / STAKE(96) /
        // RETURNED(104) / terminal(104) — but this build now departs from it on the DD's own
        // ruling, not by drift: S38 moves the STAKE/RETURNED keys out of the row into the board
        // header (once, not per-record); S40 deletes the legs cell outright (canon's flex slot,
        // reserved and rendered empty — "a blank gap mid-row," never populated in this build either
        // — is a defect, not a layout) and re-derives the remaining four column origins once, at
        // design time, against the header band (T51: a fixed grid constant may be re-derived once;
        // a zone may never resize to content at runtime).
        //
        // "Against the header band" is literal: BuildLedgerBoardHeader already carries
        // "SETTLED TICKETS · THIS RUN" (ending x=294, 14 + 280) on the left and "N RECORDS"
        // (starting x=526, 700 − 14 − 160) on the right. The STAKE/RETURNED heads S38 adds have to
        // fit the 232px gap between those two without touching either — no two live boxes may
        // overlap, even invisibly. STAKE(96) + 16 gap + RETURNED(104) = 216px fits that 232px gap
        // with 8px clear on both sides, which is where LedgerStakeX/LedgerReturnedX below come
        // from — not from the old five-cell flex formula (350/462), which packed STAKE/RETURNED
        // against the row's right edge behind the now-deleted legs cell and would sit under the
        // header's own "N RECORDS" text, not under new heads. The row below uses these exact same
        // two x/width pairs so the figures land beneath their heads (S38's own wording).
        //
        // Identity keeps its own x (14, the row's left pad) and width (112, canon's number cell —
        // untouched by the legs cell's deletion, still just the identity cell — `R2 · TICKET 02`
        // since S62, which retired the "TICKET n.n" this line used to name). Terminal keeps its own
        // mechanism too: pivot (1,1) at -14 from the row's right edge, independent of every other
        // column and unaffected by any of this (S32's ruling on it is closed on rendered evidence,
        // out of scope here). What's deliberately gone is the LedgerLegsFlexWidth constant that
        // used to sit between identity and the money columns, reserving 192px this build never
        // rendered into — S40's "blank gap mid-row," deleted rather than closed with more of the
        // same by packing STAKE/RETURNED flush against terminal, which is what removing only the
        // constant (and not re-deriving against the header) would otherwise have produced.
        private const float LedgerPadX = 14f;
        private const float LedgerColGap = 16f;
        private const float LedgerNumberWidth = 112f;
        private const float LedgerStakeWidth = 96f;
        private const float LedgerReturnedWidth = 104f;
        private const float LedgerTerminalWidth = 104f;
        private const float LedgerNumberX = LedgerPadX; // 14
        private const float LedgerStakeX = 302f; // re-derived against the header band, see above
        private const float LedgerReturnedX = LedgerStakeX + LedgerStakeWidth + LedgerColGap; // 414

        // S39: one baseline per record row. Canon stacks a 13px key over a 16px value inside each
        // of the STAKE/RETURNED cells (two lines) with identity/legs/terminal on their own lines
        // again by virtue of the row's flex alignItems:center only aligning cells that ARE single
        // lines — four baselines in total on the build this replaces. With the keys gone to the
        // header band (S38), every remaining cell — identity, the two condensed tabular figures,
        // the terminal word — is one line, so canon's own alignItems:center is now reachable:
        // BuildLedgerTicket keeps each cell's own snug, top-anchored box (unchanged sizes — 24px
        // for the 16px identity/terminal lines, 20px for the 16px value lines, matching this
        // file's pre-existing convention exactly, and required so InkRingGeometry's strike-sprite
        // math — which reads a text box's own anchored corner, not its rendered glyph — still
        // lines up) but re-picks each box's y offset so every box's own vertical centre lands on
        // the same midpoint of this (shorter) band. That is alignItems:center by construction, not
        // a stretched Middle-anchored box, and it is what "one baseline" means for a row that mixes
        // a 13px terminal word with 16px figures — canon centres them, it does not baseline-lock
        // them either. 42px (11px top pad + a 20px single line + 11px bottom pad, matching canon's
        // own "11px var(--st-pad-x)" padding) is this build's own honest arithmetic for the band;
        // the register estimates ~19px returned per record against the kit's real metrics
        // (56 → ~37), this gives 56 → 42 (14px) — sure by reading on structure (one line, heads
        // gone, every cell centred on one midpoint), a by-eye pick on the exact figure like the 56
        // it replaces, needs a capture to pin.
        private const float LedgerSummaryHeight = 42f;
        private const float LedgerLegRowHeight = 24f;

        private static float LedgerEntryHeight(Ticket ticket)
            => LedgerSummaryHeight + ticket.Legs.Count * LedgerLegRowHeight;

        /// <summary>S31: LedgerScreen()'s own 44px board header (screens.jsx) — a --ground-2 band
        /// with a --rule bottom border, "SETTLED TICKETS · THIS RUN" left and a right-flushed
        /// "N RECORDS". This is the one statement on the screen of what the list below is scoped
        /// to; the passive margin's note (S33, BuildRecordSummary) says something else entirely
        /// (that the ledger derives nothing) precisely so the two never restate each other.
        ///
        /// S38 adds the STAKE and RETURNED column heads here — once, not once per record
        /// (LedgerEntry.jsx:20,24's defect) — at the exact x/width the record row's own STAKE and
        /// RETURNED figures use (LedgerStakeX/LedgerReturnedX), so the figures sit aligned beneath
        /// them. Positioned in the 232px gap between the scope caption (ends x=294) and the record
        /// count (starts x=526) with 8px clear on both sides — see the geometry comment above
        /// LedgerPadX for the full derivation.</summary>
        private void BuildLedgerBoardHeader(RectTransform board, int settledCount)
        {
            RectTransform header = LaptopUi.MakePanel(board, "LedgerBoardHeader", new Vector2(0f, 1f),
                new Vector2(0f, 1f), Vector2.zero, new Vector2(700f, 44f), LaptopOs.Surface);
            LaptopUi.MakeRule(header, "LedgerBoardHeaderRule", new Vector2(0f, 0f), new Vector2(0f, 0f),
                Vector2.zero, new Vector2(700f, 1f), LaptopOs.Rule);
            LaptopUi.MakeText(header, "LedgerBoardHeaderScope", new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(14f, 0f), new Vector2(280f, 24f), 13, TextAnchor.MiddleLeft,
                LaptopOs.Muted, "SETTLED TICKETS · THIS RUN", _font);
            LaptopUi.MakeText(header, "LedgerBoardHeaderStake", new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(LedgerStakeX, 0f), new Vector2(LedgerStakeWidth, 24f), 13, TextAnchor.MiddleLeft,
                LaptopOs.Muted, "STAKE", _font);
            LaptopUi.MakeText(header, "LedgerBoardHeaderReturned", new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(LedgerReturnedX, 0f), new Vector2(LedgerReturnedWidth, 24f), 13, TextAnchor.MiddleLeft,
                LaptopOs.Muted, "RETURNED", _font);
            LaptopUi.MakeText(header, "LedgerBoardHeaderCount", new Vector2(1f, .5f), new Vector2(1f, .5f),
                new Vector2(-14f, 0f), new Vector2(160f, 24f), 13, TextAnchor.MiddleRight,
                LaptopOs.Muted, $"{settledCount} {SportsbookApp.Pluralize(settledCount, "RECORD")}", _font);
        }

        /// <summary>S43: the leg sub-row's terminal word — VOID beats everything (a voided leg is
        /// never anything else, regardless of State), then WON (grading or a whistle rescue), then
        /// LOST, and PENDING only as the fallback for a leg whose State is still LegState.Pending.
        /// Factored out of BuildLedgerTicket so SureThingLedgerTests can exercise the PENDING
        /// branch directly against a hand-built Leg (S43: "the render path must handle it rather
        /// than treat it as dead code") without needing a live ticket to reach that state, which —
        /// per W4's audit, unchanged by this ruling — nothing currently does for WON/LOST/CashedOut
        /// alike in this engine.</summary>
        internal static string LegStateWord(Leg leg) => leg.IsVoided ? "VOID"
            : leg.RescuedWon || leg.State == LegState.Won ? "WON"
            : leg.State == LegState.Lost ? "LOST" : "PENDING";

        /// <summary>The SETTLED record's terminal word, over the engine's own <see cref="TicketState"/>
        /// — not <see cref="SportsbookApp.TicketStateWord"/>, which reads the TV's revealed mirror
        /// and owns RIDING.
        ///
        /// <para><b>VOID is the arm this exists to close (F_0.6.0 step 5).</b> Every branch here used
        /// to fall through to "OPEN", so a ticket that had settled and been refunded printed as though
        /// it were still live. A voided ticket reaches this list — the ledger collects on
        /// <c>State != Open</c> — so that was rendered, not unreachable.</para>
        ///
        /// <para>The word is VOID and is not invented here twice: C47 rules that a market returning
        /// the stake "is a VOID, which the enum already carries", and <see cref="LegStateWord"/>
        /// already prints exactly that for a voided LEG. A ticket takes its legs' vocabulary.</para>
        ///
        /// <para>Factored out for S43's reason, which applies here more strongly than it did there:
        /// "the render path must handle it rather than treat it as dead code". Nothing in this engine
        /// drives a laptop ticket to Voided without a same-match slip, so the branch is reachable by
        /// test long before it is reachable by play.</para></summary>
        internal static string LedgerTicketStateWord(TicketState state) => state == TicketState.Won ? "WON"
            : state == TicketState.Lost ? "LOST"
            : state == TicketState.CashedOut ? "CASHED OUT"
            : state == TicketState.Voided ? "VOID" : "OPEN";

        /// <summary>The record's terminal ink. Factored alongside the word for the same reason S65
        /// factored <see cref="SportsbookApp.LegStateInk"/>: an inline ternary whose final `else`
        /// covers two states cannot state which of them it meant, and that is exactly how PENDING
        /// once shipped wearing VOID's tone.
        ///
        /// <para><b>VOID's toner-2 is one of S76's binding negatives, not a fallthrough.</b> The DD
        /// ruled VOID a third TERMINAL STATE rather than a third result (batch 67, approved by
        /// Allen): it is never drained to DEAD's `.55` and never takes DEAD's own toner-3. Wax is
        /// refused for the opposite reason — wax is money the player CAME AWAY WITH, and a refund is
        /// being made whole, not coming out ahead. Toner-2 is the weight of a fact that is neither.
        /// </para></summary>
        internal static Color LedgerTicketStateInk(TicketState state) =>
            state == TicketState.Won || state == TicketState.CashedOut ? LaptopOs.MoneyGold
            : state == TicketState.Lost ? LaptopOs.Muted
            : LaptopOs.TonerSecondary;

        /// <summary>Whether the record wears the oxide strike drawn ACROSS its word.
        ///
        /// <para><b>S76's other binding negative: a VOID never takes it.</b> The strike is what DEAD
        /// means here — S15 put the oxide in the strike alone and never in a glyph fill — and a void
        /// is not a loss. Written as its own predicate rather than an inline `== Lost` so the rule
        /// has something to be asserted against, and so a later state cannot be added to the strike
        /// by widening a condition nobody re-read.</para></summary>
        internal static bool LedgerShowsDeadStrike(TicketState state) => state == TicketState.Lost;

        // S27: every scrolling body reserves the rail's own 4px on the right, whether or not the
        // rail ends up drawn (LaptopUi.RailReserve) — settled-ticket rows now live inside one
        // (OldSlipsApp.Render), so their own full-width elements (the row panel, and every
        // divider/rule inside it) are sized to this narrower content column rather than the
        // board's full 700px. Elements anchored from the row's right edge (TicketState/
        // LedgerDeadStrike, LegState) already reflow with it via Unity's own anchor math and need
        // no separate constant.
        private const float LedgerRowWidth = 700f - LaptopUi.RailReserve;

        private void BuildLedgerTicket(RectTransform parent, Ticket ticket, int index, int round, float y)
        {
            float height = LedgerEntryHeight(ticket);
            RectTransform row = LaptopUi.MakePanel(parent, "LedgerTicket" + index, new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, y), new Vector2(LedgerRowWidth, height), LaptopOs.Ink);
            // S62: R2 · TICKET 02 — never the engine's own key. The round qualifier belongs here
            // specifically: this list spans rounds, so it is what makes two rows tell themselves
            // apart, and it is read from the TICKET's round rather than the screen's current one.
            string identity = LaptopUi.TicketIdentity(ticket.Id, round, index, withRound: true);
            string state = LedgerTicketStateWord(ticket.State);
            // S41: S36's designed absence expires here. Engine retention landed (`9e55d0d`, on this
            // tree since the merge), so `Ticket.CashedOutFor` carries the figure and it PRINTS —
            // never the em dash that stood in for it, and never the fabricated $0 that S36 refused.
            //
            // The em dash survives for exactly one case: a record whose amount is genuinely
            // unknowable (`CashedOutFor` null). That is still an absence and still prints honest.
            // S41 puts it in the record's own cell and nowhere else — see the RETURNED total.
            //
            // VOID (F_0.6.0 step 5): the stake, printed. This case used to reach the em dash, and the
            // dash was the wrong token for it in S41's own terms — the dash means an amount that is
            // GENUINELY UNKNOWABLE, and a voided ticket's return is exactly known. The engine states
            // it outright: a ticket that voids in full "returns the stake unconditionally" and has a
            // PotentialPayout of zero (Domain.cs, VoidedInFull). So the stake is read here, never
            // PotentialPayout, which would print $0 for a ticket that cost the player nothing.
            //
            // The DD put this more strongly than I had (batch 67, approved by Allen): **S41 EXPIRED
            // the em dash here** — the VOID row is "the word + the stake printed as a KNOWN sum",
            // and the dash is a binding negative, not a case I happened to reassign. I had written
            // that S41 was "kept, not spent"; the ruling is that for this row it is spent.
            //
            // The dash still prints for the cash-out whose retained figure is genuinely unknown.
            // That case was not before the DD and is left exactly as S41 left it.
            string returnedValue = ticket.State == TicketState.Won ? LaptopUi.Money(ticket.PotentialPayout)
                : ticket.State == TicketState.Lost ? LaptopUi.Money(0)
                : ticket.State == TicketState.Voided ? LaptopUi.Money(ticket.Stake)
                : ticket.State == TicketState.CashedOut && ticket.CashedOutFor.HasValue
                    ? LaptopUi.Money(ticket.CashedOutFor.Value)
                    : "—";
            // F5/F6 / LedgerEntry.jsx: `color: won ? var(--wax) : var(--toner-3)` applies to BOTH
            // the terminal word and the RETURNED value. S15 resolved LOST more precisely: oxide
            // belongs only to the strike drawn ACROSS the word (LedgerDeadStrike, below,
            // unchanged), never to a glyph fill — the word and the RETURNED value both recede to
            // toner-3 (LaptopOs.Muted) instead.
            // S36 paired CASHED OUT with WON in wax on the terminal word only, and held RETURNED at
            // toner-3 because "an em dash is an absence, not a fact to celebrate".
            //
            // S41: now that the figure prints, that reasoning applies to the em dash rather than to
            // every cashed-out row. A retained cash-out amount is money the player actually got
            // back, so it takes wax with its word, exactly as WON's payout does. Only the
            // unknowable case still recedes to toner-3 — the absence dims, the fact does not.
            // VOID takes --toner-2 for BOTH cells, and takes it deliberately rather than by falling
            // through to it. Two rulings meet here and agree:
            //  · S65 already ruled a VOID leg stays --toner-2 and must not be dragged down with
            //    PENDING. A voided ticket is the same fact at ticket scale, so it reads the same.
            //  · it is not wax. Wax is money the player CAME AWAY WITH — S41 gave it to a retained
            //    cash-out on exactly that ground. A refund is not a winning: C47 and the engine both
            //    call a void neither a win nor a loss, and paying it in wax would stage being made
            //    whole as though it were coming out ahead.
            // Nor does it dim to --toner-3: a returned stake is a fact, and S41's line is that the
            // absence dims, the fact does not. Same weight as its word, which is what toner-2 is.
            Color stateColor = LedgerTicketStateInk(ticket.State);
            bool lost = LedgerShowsDeadStrike(ticket.State);
            bool unknowableReturn =
                ticket.State == TicketState.CashedOut && !ticket.CashedOutFor.HasValue;
            Color returnedColor = lost || unknowableReturn ? LaptopOs.Muted : stateColor;

            // S39: one baseline. Every cell below keeps its own snug, top-anchored box (unchanged
            // sizes from before this ruling) but is re-centred on y=-21, this band's own midpoint
            // (LedgerSummaryHeight/2 = 21) — canon's alignItems:center, reached now that the keys
            // that used to force identity/value/terminal onto separate lines are gone (S38).

            // --- identity (112px). LedgerEntry.jsx colours this --toner-2 unconditionally — no
            // won/lost branch — so it does not dim on a LOST ticket; the terminal word and
            // RETURNED value already carry that signal.
            TMP_Text identityText = LaptopUi.MakeText(row, "TicketIdentity", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(LedgerNumberX, -9f), new Vector2(LedgerNumberWidth, 24f), 16, TextAnchor.UpperLeft,
                LaptopOs.TonerSecondary, identity, _fontCond);
            identityText.enableWordWrapping = false; // canon: whiteSpace nowrap

            // S40: the legs flex cell (canon's ~192px slot between identity and STAKE) is deleted,
            // not left reserved. It never carried the per-leg summary LedgerEntry.jsx puts there —
            // this build carries full per-leg sub-rows below instead (odds, per-leg state) that the
            // canon string can't — but a cell reserved and rendered empty is a blank gap mid-row,
            // not a layout, so nothing stands in its place any more; STAKE/RETURNED are re-derived
            // against the header band above rather than kept where the deleted cell used to end.

            // --- STAKE (96px): one condensed tabular figure now, under the board header's STAKE
            // head (S38) instead of a key line repeated inside every row. LedgerEntry.jsx colours
            // the value --toner unconditionally (no won/lost branch).
            LaptopUi.MakeText(row, "TicketStakeValue", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(LedgerStakeX, -11f), new Vector2(LedgerStakeWidth, 20f), 16, TextAnchor.UpperLeft,
                LaptopOs.White, LaptopUi.Money(ticket.Stake), _fontCond);

            // --- RETURNED (104px): one condensed tabular figure under the board header's RETURNED
            // head; colour carries won/lost/cashed exactly as before.
            LaptopUi.MakeText(row, "TicketReturnedValue", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(LedgerReturnedX, -11f), new Vector2(LedgerReturnedWidth, 20f), 16, TextAnchor.UpperLeft,
                returnedColor, returnedValue, _fontCond);

            // --- terminal word (104px, flex:none, textAlign right) — S32: the row's rightmost
            // element and its last scan point. Anchor/pivot (1,1) top-right, matching
            // InkRingGeometry's required convention (as BuildMirrorLeg's LegState already does).
            //
            // A consequence of being rightmost: nothing is laid out to its right by construction,
            // so the collision this file used to work around (a lost ticket's strike overshooting
            // into a neighbouring column, previously fixed by moving STAKE to x=338) cannot happen
            // here. The strike still overshoots 8px past this box's right edge (InkRingGeometry),
            // eating into the row's 14px right pad and landing 6px shy of the row edge (700). That
            // 6-of-14px clearance is the requirement the old fix protected; it still holds here,
            // satisfied by geometry instead of a moved column. If a future change ever puts a
            // control right of this word, it must clear that same 8px overshoot.
            // −13, not −9, and the 4px is measured rather than eyeballed. S39 asks for one baseline
            // across the record, and top-anchored text aligns TOPS, not baselines — so a 13px
            // terminal word beside a 16px identity rides high by the difference in cap height. On
            // the populated frame the identity's ink bottomed at y=218 and the terminal's at y=214.
            //
            // The box moves rather than the text's alignment inside it. InkRingGeometry places the
            // strike from this rect's own anchoredPosition, so centring the glyphs would slide the
            // word off its strike; moving the box takes the strike with it, which is what a struck
            // word wants.
            TMP_Text ticketStateText = LaptopUi.MakeText(row, "TicketState", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-LedgerPadX, -13f), new Vector2(LedgerTerminalWidth, 24f), 13, TextAnchor.UpperRight,
                stateColor, state, _fontCond);
            ticketStateText.enableWordWrapping = false; // canon: whiteSpace nowrap
            if (lost)
            {
                Sprite strike = SportsbookApp.ResolveStrike(index);
                if (strike != null)
                {
                    // Same fix already applied to the MY BETS dead leg: size/place the strike from
                    // the state text's own measured bounds, never a fixed sprite-native box.
                    (Vector2 position, Vector2 size) = SportsbookApp.InkRingGeometry(ticketStateText);
                    LaptopUi.MakeSprite(row, "LedgerDeadStrike", strike, new Vector2(1f, 1f),
                        new Vector2(1f, 1f), position, size, LaptopOs.MoneyBad);
                }
            }

            // Summary-band / leg-sub-row divider — internal to this entry, separate from the canon
            // borderBottom (LedgerEntryRule, at the very bottom of the whole entry, below).
            LaptopUi.MakeRule(row, "TicketRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -LedgerSummaryHeight), new Vector2(LedgerRowWidth, 1f));

            for (int legIndex = 0; legIndex < ticket.Legs.Count; legIndex++)
            {
                Leg leg = ticket.Legs[legIndex];
                RectTransform legRow = LaptopUi.MakePanel(row, "LedgerLeg" + legIndex,
                    new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(0f, -LedgerSummaryHeight - legIndex * LedgerLegRowHeight),
                    new Vector2(LedgerRowWidth, 23f), LaptopOs.Ink);
                // S43 ruled · DD 2026-08-02 batch 7: PENDING is legal in exactly one place — a
                // CASHED OUT ticket, where he left before the match ended. W4's earlier audit
                // (Run.LockRound samples every matchup's StatLine, bet or not, before a single
                // SweatSession exists, so leg.State reads Pending only while Matchup.StatLine is
                // still null) is accurate for THIS engine: no ticket currently reaches this loop
                // with a Pending leg, WON/LOST/CashedOut alike — SureThingLedgerTests locks that
                // invariant down for WON/LOST. But the DD overruled treating that as licence to
                // drop the branch: leg.State's own definition (engine/Domain.cs) makes Pending a
                // real, constructible data state, not dead code, and only a CASHED OUT ticket may
                // legally carry one — the render path must resolve it correctly (the literal word
                // "PENDING", never a fabricated terminal word, in --toner-3/Muted below — already
                // true, unconditionally, for every non-lost leg) rather than assume it can't
                // arrive. LegStateWord is exercised directly by SureThingLedgerTests against a
                // hand-built Pending leg for exactly this reason.
                string legState = LegStateWord(leg);
                // S35(c): RevealedLeg.jsx is the spec of record for a leg row's state — the ✓,
                // the word, the strike and opacity .55 carry it, never a per-outcome hue. Both
                // colours below were already flat regardless of outcome, so there was no hue to
                // remove; what was missing was the .55 dim RevealedLeg applies to the whole row
                // once a leg is dead (LaptopUi.Dim already implements that exact alpha), so a
                // settled LOST leg now recedes the way the word beside it already says it should.
                bool legLost = legState == "LOST";
                // F7: routes through CompactLegLabel + FitLabelKeepingSuffix exactly as BuildSlip's
                // Leg rows and BuildStagedReceipt's TicketLeg rows already do (same call shape,
                // same 2-space odds separator as the latter), instead of the engine's own
                // DisplayLabel — which repeats the picked team a second time ("DULUTH PLUMBERS ML
                // — DULUTH PLUMBERS V TULSA LOOPHOLES"). The odds suffix is protected from the trim
                // the same way theirs is.
                const float legIdentityWidth = 470f;
                // S70: `--st-track-name` (.03em). LedgerEntry.jsx:17 hard-coded `.02em` here, which
                // matched no token — and this line is a run of names and prices, which is exactly
                // what that token is for. The 0.01em difference is below anything this surface can
                // resolve; the point is that the value now has a name.
                //
                // **Measured with the tracking it renders with**, like every other fitted slot —
                // fifth instance of that coupling, and the reason FitLabelKeepingSuffix takes the
                // parameter at all. The odds suffix stays protected either way.
                string legIdentityText = LaptopUi.FitLabelKeepingSuffix(_fontCond, $"{legIndex + 1}. ",
                    SportsbookApp.CompactLegLabel(leg.Matchup, leg.Selection),
                    $"  {OddsFormat.American(leg.OfferedOdds)}", 13, legIdentityWidth,
                    LaptopTrack.Names);
                LaptopUi.MakeText(legRow, "LegIdentity", new Vector2(0f, .5f), new Vector2(0f, .5f),
                    new Vector2(28f, 0f), new Vector2(legIdentityWidth, 22f), 13, TextAnchor.MiddleLeft,
                    legLost ? LaptopUi.Dim(LaptopOs.TonerSecondary) : LaptopOs.TonerSecondary,
                    legIdentityText, _fontCond, LaptopTrack.Names);
                LaptopUi.MakeText(legRow, "LegState", new Vector2(1f, .5f), new Vector2(1f, .5f),
                    new Vector2(-14f, 0f), new Vector2(140f, 22f), 13, TextAnchor.MiddleRight,
                    legLost ? LaptopUi.Dim(LaptopOs.Muted) : LaptopOs.Muted, legState, _fontCond);
                LaptopUi.MakeRule(legRow, "LegRule", new Vector2(0f, 0f), new Vector2(0f, 0f),
                    Vector2.zero, new Vector2(LedgerRowWidth, 1f));
            }

            // S32 canon: "every entry carries a borderBottom in --rule-soft" (LedgerEntry.jsx:
            // borderBottom: var(--rule-w) solid var(--rule-soft)). Replaces the blank 2px gap this
            // file used to leave between entries — entries now sit flush and this hairline is the
            // only separator, drawn at the entry's own bottom edge so it costs no extra height.
            LaptopUi.MakeRule(row, "LedgerEntryRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -height), new Vector2(LedgerRowWidth, 1f));
        }

        // MarginHeader.jsx: 12px top / 9px bottom padding around the biro title before its own
        // 2px --biro-deep rule.
        private const float RecordHeaderHeight = 41f;
        // MarginRow.jsx: 9px padding above and below one label/value line, closed by its own 1px
        // --rule divider.
        private const float RecordRowHeight = 38f;

        /// <summary>S33: PassiveMargin over the ledger — the biro-ruled MarginHeader (title +
        /// 2px --biro-deep rule) stays exactly as it does on every other destination (read-only
        /// describes the house's record, not whose margin it is), followed by exactly three
        /// MarginRows and one note, in the kit's own order (app.jsx:94-97): TICKETS SETTLED,
        /// STAKED, RETURNED, then the note. Replaces the previous seven-block panel (a toner
        /// header, a soft rule, and five more text blocks) that carried no biro anywhere.</summary>
        private void BuildRecordSummary(RectTransform margin, int settled, double stake,
            double returned)
        {
            RectTransform summary = LaptopUi.MakePanel(margin, "RecordSummary", new Vector2(0f, 1f),
                new Vector2(0f, 1f), Vector2.zero, new Vector2(324f, 530f), LaptopOs.Ink);
            // S34: the 26px ruled-paper ground, shared with every other margin on this surface via
            // the one MarginRuledPaperGraphic class — added first so it sits behind the header,
            // rows and note below it.
            LaptopUi.MakeMarginRuledPaper(summary, "RuledPaper");

            // MarginHeader.jsx: biro title, uppercase, closed by the 2px --biro-deep rule.
            // S60/S61: this rendering was the correct one, and it is now the shared component both
            // margins draw rather than the copy the other one was compared against.
            LaptopUi.MakeMarginHeader(summary, "RECORD", _fontCond);

            // MarginRow.jsx x3, in the kit's order (app.jsx:94-96).
            BuildRecordRow(summary, "RecordRowSettled", "TICKETS SETTLED",
                settled.ToString(CultureInfo.InvariantCulture), LaptopOs.White, -RecordHeaderHeight);
            BuildRecordRow(summary, "RecordRowStaked", "STAKED", LaptopUi.Money(stake), LaptopOs.White,
                -(RecordHeaderHeight + RecordRowHeight));
            // S41: **this total never prints an em dash.** Under S36 a single cashed-out ticket
            // blanked the whole row, because the run then held an unknown figure and the sum could
            // not be honest. Retention removed the unknown — cash-out amounts are retained now — so
            // the sum is the sum, and it prints in wax like the money it is.
            //
            // The ruling also settled the case retention does not cover. If a record whose amount is
            // genuinely unknowable ever exists, **the total still prints the known sum** and the
            // absence stays in that record's own cell (BuildLedgerTicket). An absence in one row is
            // a fact about that row; blanking the total makes it a fact about the whole run, which
            // it is not. That is why this no longer takes a `cashed` count — nothing is left for it
            // to decide.
            BuildRecordRow(summary, "RecordRowReturned", "RETURNED", LaptopUi.Money(returned),
                LaptopOs.MoneyGold,
                -(RecordHeaderHeight + RecordRowHeight * 2f));

            // PassiveMargin's one note (app.jsx:97) — bottom-anchored per marginShell's fixed
            // vertical order, and never the board header's own wording (S31's trap: that wording
            // belongs to BuildLedgerBoardHeader alone).
            LaptopUi.MakeText(summary, "RecordNote", new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(14f, 15f), new Vector2(296f, 40f), 13, TextAnchor.LowerLeft,
                LaptopOs.Muted, "READ-ONLY. THE LEDGER COPIES SETTLED TICKETS AND DERIVES NOTHING.",
                _font);
            // S37: the live round number appears exactly once on the surface, in the masthead
            // (BuildLedgerChrome's "Scope" text). This margin carries none of it — no
            // "RoundIdentity" restatement, same as before.
        }

        /// <summary>MarginRow.jsx: one label/value line — label 13px roman --toner-3, value
        /// condensed --toner (or the caller's own tone), right-flushed, closed by a 1px --rule
        /// divider. <paramref name="rowTop"/> is the row's own top edge, matching
        /// RecordHeaderRule/the previous row's own bottom edge exactly so rows sit flush with no
        /// gap and no overlap.</summary>
        // S58: the body moved to LaptopUi.MakeMarginRow so MY BETS' tally renders the same
        // MarginRow this does, from one place rather than two copies.
        private void BuildRecordRow(RectTransform summary, string name, string label, string value,
            Color valueColor, float rowTop)
            => LaptopUi.MakeMarginRow(summary, name, label, value, valueColor, rowTop,
                RecordRowHeight, _font, _fontCond);

        private void BuildLedgerTray()
        {
            // The ledger's separate HOME button is gone: the running app's own tray slot drops to
            // the desktop, so HOME was a second control for a job the tray already did — and only
            // this screen had it, which is exactly the asymmetry the shared chrome removes.
            NotebookChrome.BuildTray(_root, 1024f, _font,
                NotebookChrome.Running.Ledger, _sportsbook, null, _home);
        }
    }
}
