using System;
using System.Collections.Generic;
using System.Globalization;
using SBR.Engine;
using UnityEngine;
using UnityEngine.UI;

namespace SBR.Game
{
    /// <summary>SureThing's card-book app. It is a renderer over BetslipModel, RunDirector, and the TV view.</summary>
    public sealed class SportsbookApp
    {
        public enum Tab { Lobby, Detail, MyBets, Rewards }

        private enum DetailTab { Goals, Btts, Corners, Cards, Players }
        private readonly RectTransform _root;
        private readonly Font _font;
        // --font-cond (Archivo Narrow) seam: figures, prices, team names and the wax/lock/rub-out
        // action labels route through this instead of _font. See LaptopScreen's field comment — both
        // currently resolve to the same fallback Font on purpose.
        private readonly Font _fontCond;
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
        private DetailTab _detailTab = DetailTab.Goals;

        /// <summary>Fixed gap between an offer row's label cell and its price cell (MakeOfferRow).
        /// Shared as a class constant so every destination's row layout and MakeOfferRow's own
        /// price placement can never silently drift apart.</summary>
        private const float OfferLabelGap = 8f;

        /// <summary>A1 ruling: every destination's offer row is 54px tall, full content width,
        /// single column. Shared so BuildMarketLines/BuildBothTeamsScore/BuildPlayerLines,
        /// BuildScrollingBody's content-height math, and MakeOfferRow's own row rect can never
        /// independently drift.</summary>
        private const float OfferRowHeight = 54f;

        public SportsbookApp(RectTransform root, Font font, Font fontCond, LaptopScreen host, Action invalidate,
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
            MakeTab(tabs, "FORM", Tab.Lobby, tab, run.Phase == Phase.Shop);
            MakeTab(tabs, "ENTRY", Tab.Detail, tab, run.Phase == Phase.Shop);
            MakeTab(tabs, "MY BETS", Tab.MyBets, tab, run.Phase == Phase.Shop);
            MakeTab(tabs, "REWARDS", Tab.Rewards, tab, run.Phase != Phase.Shop);
            LaptopUi.MakeText(tabs, "Sheet", new Vector2(1f, .5f), new Vector2(1f, .5f), new Vector2(-14f, 0f), new Vector2(170f, 24f), 13, TextAnchor.MiddleRight, LaptopOs.Muted, "SHEET 1 OF 1", _font);
            RectTransform mast = LaptopUi.MakePanel(top, "FormMasthead", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -72f), new Vector2(1024f, 68f), LaptopOs.Ink);
            LaptopUi.MakeText(mast, "Brand", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -8f), new Vector2(300f, 28f), 26, TextAnchor.UpperLeft, LaptopOs.White, "SURETHING FORM", _fontCond);
            LaptopUi.MakeText(mast, "Run", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(17f, -38f), new Vector2(340f, 20f), 13, TextAnchor.UpperLeft, LaptopOs.Muted, $"ROUND {run.Round} OF {run.Config.Rounds}  ·  PRICES FINAL", _font);
            LaptopUi.MakeText(mast, "Figures", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-16f, -10f), new Vector2(610f, 48f), 21, TextAnchor.UpperRight, LaptopOs.White, $"BANK {LaptopUi.Money(run.Bank)}    TARGET {LaptopUi.Money(run.CurrentPayment)}    TICKETS {run.Tickets.Count}/{run.Config.MaxTicketsPerRound}", _font);
        }

        private void MakeTab(RectTransform top, string label, Tab tab, Tab selected, bool disabled)
        {
            float x = tab == Tab.Lobby ? 14f : tab == Tab.Detail ? 122f : tab == Tab.MyBets ? 230f : 358f;
            bool active = tab == selected;
            LaptopUi.MakeButton(top, label, label, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(x, 3f), new Vector2(tab == Tab.MyBets ? 116f : 100f, 32f), 13,
                active ? LaptopOs.Ink : LaptopOs.Surface,
                disabled ? LaptopUi.Dim(LaptopOs.Muted) : active ? LaptopOs.White : LaptopOs.Muted,
                disabled ? null : () => { _selectTab(tab); }, _font, !disabled);
        }

        /// <summary>Allen ruling (2026-08-03): the lobby's card pitch. Duplicated from
        /// <see cref="BuildMatchupCard"/>'s own card size (<c>Vector2(700f, 78f)</c>) rather than
        /// factored into a constant shared with that method, because the ruling froze
        /// BuildMatchupCard's own geometry out of scope for this change. Feeds
        /// <see cref="BuildScrollingBody"/>'s row-height math and the cards' scrolled Y offset in
        /// <see cref="BuildLobby"/> — if BuildMatchupCard's own 78f ever moves, this must move
        /// with it.</summary>
        private const float MatchupCardPitch = 78f;

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
                new Vector2(0f, 1f), position, new Vector2(700f, 78f), LaptopOs.Surface);
            bool awaySelected = slip.SelectionOn(matchup.Index) == MarketSelection.Moneyline(Side.Away);
            bool homeSelected = slip.SelectionOn(matchup.Index) == MarketSelection.Moneyline(Side.Home);
            // The wash behind a form entry he has marked (palette-surething.css --marked-wash).
            // Added first, before any text/buttons, so it sits behind them; sized to fill the whole
            // card so it is trivially contained within it.
            if (awaySelected || homeSelected)
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
            TeamLine(card, "Home", LaptopUi.TeamShort(matchup.Home), matchup.Home.Record, -44f);

            LaptopUi.MakeButton(card, "AwayOdds", $"AWAY  {OddsFormat.American(matchup.AwayOdds)}",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(462f, -8f), new Vector2(112f, 32f), 19,
                LaptopOs.Ink, frozen ? LaptopUi.Dim(LaptopOs.Muted) : LaptopOs.White,
                frozen ? null : () => { slip.Toggle(matchup.Index, MarketSelection.Moneyline(Side.Away)); _invalidate(); }, _fontCond, !frozen);
            LaptopUi.MakeButton(card, "HomeOdds", $"HOME  {OddsFormat.American(matchup.HomeOdds)}",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(462f, -43f), new Vector2(112f, 32f), 19,
                LaptopOs.Ink, frozen ? LaptopUi.Dim(LaptopOs.Muted) : LaptopOs.White,
                frozen ? null : () => { slip.Toggle(matchup.Index, MarketSelection.Moneyline(Side.Home)); _invalidate(); }, _fontCond, !frozen);
            if (awaySelected || homeSelected)
            {
                Sprite ring = ResolvePriceRing(matchup.Index);
                if (ring != null)
                {
                    // The price cell IS the odds button (112x32) — it is already wider than the
                    // 96x30 cell docs/design/direction-concepts/assets/ASSETS.md assumed, because the
                    // "AWAY  -341" label needs the room. Overshoot the ring 8px past every edge of
                    // the REAL cell so the pen stroke frames the price instead of crossing it.
                    const float overshoot = 8f;
                    Vector2 cellPosition = new Vector2(462f, awaySelected ? -8f : -43f);
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

            Text nameText = LaptopUi.MakeText(card, "Team" + side, new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(nameX, y), new Vector2(250f, lineHeight), 19,
                TextAnchor.MiddleLeft, LaptopOs.White, name, _fontCond);
            // A long name must push its record along, never wrap onto a second line inside a 30px box.
            nameText.horizontalOverflow = HorizontalWrapMode.Overflow;

            LaptopUi.MakeText(card, "Record" + side, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(nameX + nameText.preferredWidth + gap, y), new Vector2(90f, lineHeight),
                13, TextAnchor.MiddleLeft, LaptopOs.Muted, record, _font)
                .horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        private void OpenDetail(int matchupIndex)
        {
            _detailMatchup = matchupIndex;
            _detailTab = DetailTab.Goals;
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

            RectTransform destinations = LaptopUi.MakePanel(panel, "MarketDestinations",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -76f),
                new Vector2(700f, 42f), LaptopOs.Surface);
            MakeDetailTab(destinations, "GOALS", DetailTab.Goals, 14f);
            MakeDetailTab(destinations, "BTTS", DetailTab.Btts, 118f);
            MakeDetailTab(destinations, "CORNERS", DetailTab.Corners, 222f);
            MakeDetailTab(destinations, "CARDS", DetailTab.Cards, 338f);
            MakeDetailTab(destinations, "PLAYERS", DetailTab.Players, 442f);

            RectTransform body = LaptopUi.MakePanel(panel, "MarketBody", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, -118f), new Vector2(700f, 412f), LaptopOs.Ink);

            // A2 ruling: the per-destination panel title ("GOALS TOTAL" etc.) is deleted — each row
            // now names its own market and the tab strip already names the destination; the kit has
            // no such heading.
            if (_detailTab == DetailTab.Goals)
                BuildMarketLines(body, slip, matchup, run.Config.GoalLines, MarketKind.TotalGoals, boardFrozen, run);
            else if (_detailTab == DetailTab.Btts)
                BuildBothTeamsScore(body, slip, matchup, boardFrozen, run);
            else if (_detailTab == DetailTab.Corners)
                BuildMarketLines(body, slip, matchup, run.Config.CornerLines, MarketKind.TotalCorners, boardFrozen, run);
            else if (_detailTab == DetailTab.Cards)
                BuildMarketLines(body, slip, matchup, run.Config.CardLines, MarketKind.TotalCards, boardFrozen, run);
            else
                BuildPlayerLines(body, slip, matchup, boardFrozen, run);

            // Drawn last (after the market body's scroll content) so it always renders on top of
            // row 0 instead of being hidden behind that row's opaque price-cell button. Under the
            // old fixed layout this banner floated clear of the offer rows because a title row
            // reserved the first 48px; A2 deleted that title, so the list now begins flush with the
            // body's top edge and this has to out-order it instead.
            if (boardFrozen)
                LaptopUi.MakeText(body, "LockedMarketReason", new Vector2(1f, 1f), new Vector2(1f, 1f),
                    new Vector2(-14f, -8f), new Vector2(280f, 32f), 13, TextAnchor.UpperRight,
                    LaptopOs.MoneyBad, "ROUND LOCKED — WATCH MY BETS", _font);

            BuildSlip(run, slip, boardFrozen);
        }

        private void MakeDetailTab(RectTransform parent, string label, DetailTab tab, float x)
        {
            bool active = _detailTab == tab;
            LaptopUi.MakeButton(parent, "DetailTab" + label, label, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x, -5f), new Vector2(label == "CORNERS" || label == "PLAYERS" ? 108f : 96f, 32f), 13,
                active ? LaptopOs.Ink : LaptopOs.Surface, active ? LaptopOs.White : LaptopOs.TonerSecondary,
                () => { _detailTab = tab; _invalidate(); }, _font);
        }

        private void BuildMarketLines(RectTransform parent, BetslipModel slip, Matchup matchup,
            double[] lines, MarketKind kind, bool frozen, Run run)
        {
            RectTransform content = BuildScrollingBody(parent, lines.Length * 2, run, out float rowWidth,
                out float rowsOffsetY);
            for (int i = 0; i < lines.Length; i++)
            {
                double line = lines[i];
                MarketSelection over = new MarketSelection(kind, line, MarketChoice.Over);
                MarketSelection under = new MarketSelection(kind, line, MarketChoice.Under);
                // A2 ruling: the label is the engine's own DD-verbatim string (MatchModel.Fields'
                // Line), not a locally re-formatted "OVER {line:0.0}" — the local format dropped
                // the noun ("GOALS"/"CORNERS"/"CARDS").
                string overLabel = MatchModel.Fields(matchup, over).Line;
                string underLabel = MatchModel.Fields(matchup, under).Line;
                // E-07: rows are pushed down by rowsOffsetY, the height BuildScrollingBody's own
                // "PLACED THIS ROUND" block (if any) already consumed at the top of content.
                MakeOfferRow(content, slip, matchup, over, overLabel, null, i * 2,
                    -rowsOffsetY - (i * 2) * OfferRowHeight, rowWidth, frozen);
                MakeOfferRow(content, slip, matchup, under, underLabel, null, i * 2 + 1,
                    -rowsOffsetY - (i * 2 + 1) * OfferRowHeight, rowWidth, frozen);
            }
        }

        private void BuildBothTeamsScore(RectTransform parent, BetslipModel slip, Matchup matchup,
            bool frozen, Run run)
        {
            RectTransform content = BuildScrollingBody(parent, 2, run, out float rowWidth, out float rowsOffsetY);
            MarketSelection yes = MarketSelection.BothTeamsToScore(true);
            MarketSelection no = MarketSelection.BothTeamsToScore(false);
            // A2 ruling: BTTS's choice lives in Fields.Market ("BTTS — YES"/"BTTS — NO"); Fields.Line
            // is just "BOTH TEAMS TO SCORE" and does not carry which side this row is.
            MakeOfferRow(content, slip, matchup, yes, MatchModel.Fields(matchup, yes).Market, null,
                0, -rowsOffsetY, rowWidth, frozen);
            MakeOfferRow(content, slip, matchup, no, MatchModel.Fields(matchup, no).Market, null,
                1, -rowsOffsetY - OfferRowHeight, rowWidth, frozen);
        }

        private void BuildPlayerLines(RectTransform parent, BetslipModel slip, Matchup matchup, bool frozen,
            Run run)
        {
            // S25 amended / A5 / C19: the withdrawn fixed-body cap is gone — every priced scorer
            // offer renders, full stop. The interior list scrolls instead (A4/S27) rather than
            // truncating with an "N NOT SHOWN" remainder.
            var scorers = new List<MarketOffer>();
            foreach (MarketOffer offer in matchup.Markets)
                if (offer.Selection.Kind == MarketKind.AnytimeScorer) scorers.Add(offer);

            RectTransform content = BuildScrollingBody(parent, scorers.Count, run, out float rowWidth,
                out float rowsOffsetY);
            for (int row = 0; row < scorers.Count; row++)
            {
                MarketSelection selection = scorers[row].Selection;
                // S22/A3/E-24: the name is the fact (Fields.Line carries the noun — "VALE ANYTIME");
                // the role is its own field (Fields.Role), engine-emitted, never concatenated into
                // the name's own string or colour.
                MatchModel.MarketFields fields = MatchModel.Fields(matchup, selection);
                MakeOfferRow(content, slip, matchup, selection, fields.Line, fields.Role, row,
                    -rowsOffsetY - row * OfferRowHeight, rowWidth, frozen);
            }
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
        /// a fourth caller, passing its own 78px card pitch (<see cref="MatchupCardPitch"/>) — the
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
        {
            const float railReserve = 8f; // 4px RuleSoft track + 4px clearance (A4: never under the rail).
            float bodyWidth = body.rect.width;
            float bodyHeight = body.rect.height;
            rowsOffsetY = MeasurePlacedThisRoundHeight(run);
            float contentHeight = rowsOffsetY + rowCount * rowHeight;
            bool overflows = contentHeight > bodyHeight;
            rowWidth = overflows ? bodyWidth - railReserve : bodyWidth;

            RectTransform scroll = LaptopUi.MakePanel(body, "MarketScroll", new Vector2(0f, 1f),
                new Vector2(0f, 1f), Vector2.zero, new Vector2(bodyWidth, bodyHeight),
                new Color(0f, 0f, 0f, 0f));
            ScrollRect scrollRect = scroll.gameObject.AddComponent<ScrollRect>();

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
                totalHeight += 30f + run.Tickets[i].Legs.Count * 18f + 8f;
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
        /// independently drift.</summary>
        private void MakeOfferRow(RectTransform parent, BetslipModel slip, Matchup matchup,
            MarketSelection selection, string label, string role, int offerIndex, float y,
            float rowWidth, bool frozen)
        {
            const float leftPad = 14f;
            const float rightPad = 14f;
            const float priceCellWidth = 176f;
            const float priceCellHeight = 32f;
            const float priceCellY = -(OfferRowHeight - priceCellHeight) / 2f; // vertical centre of the row.

            MarketSelection? existing = slip.SelectionOn(matchup.Index);
            bool selected = existing.HasValue && existing.Value == selection;
            bool replacement = existing.HasValue && !selected;
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
            Text labelText = LaptopUi.MakeText(row, "MarketLabel" + key, new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(leftPad, 0f), new Vector2(labelWidth, OfferRowHeight),
                19, TextAnchor.MiddleLeft, LaptopOs.White, label, _fontCond);

            if (!string.IsNullOrEmpty(role))
            {
                // The role is a LABEL, not a figure (audit E-24, named load-bearing by S28). With
                // tracking unreachable, the only channels left to separate label from fact are the
                // colour split (--toner-3 label against --toner fact) and the two-voice type split
                // (roman label against condensed figure). Both must therefore be exact here: roman
                // face, fact floor, --toner-3 — a condensed 19px role would read as a second fact.
                float roleX = leftPad + labelText.preferredWidth + 8f;
                float roleWidth = Mathf.Max(0f, leftPad + labelWidth - roleX);
                LaptopUi.MakeText(row, "MarketRole" + key, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(roleX, 0f), new Vector2(roleWidth, OfferRowHeight), 13,
                    TextAnchor.MiddleLeft, LaptopOs.Muted, role, _font)
                    .horizontalOverflow = HorizontalWrapMode.Overflow;
            }

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
                replacement ? "⇄  " + price : price, new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(priceCellWidth, priceCellHeight), 19, new Color(0f, 0f, 0f, 0f),
                frozen ? LaptopUi.Dim(LaptopOs.Muted) : LaptopOs.White,
                frozen ? null : () => { slip.Toggle(matchup.Index, selection); _invalidate(); }, _fontCond, !frozen);
            if (replacement)
            {
                RectTransform hint = LaptopUi.MakePanel(offer, "ReplacementHint",
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -(priceCellHeight - 1f)),
                    new Vector2(priceCellWidth, 2f), new Color(0f, 0f, 0f, 0f));
                LaptopUi.MakePanel(hint, "ReplacementUnderline" + key, Vector2.zero, Vector2.zero,
                    Vector2.zero, new Vector2(priceCellWidth, 2f), LaptopOs.TonerSecondary);
            }

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

        private void BuildSlip(Run run, BetslipModel slip, bool boardFrozen)
        {
            RectTransform panel = LaptopUi.MakePanel(_root, "Slip", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -140f), new Vector2(324f, 530f), LaptopOs.Ink);
            panel.name = "WorkingMargin";
            // S34 ruling: the 26px ruled-paper ground is one shared Graphic subclass emitting
            // untextured geometry (RuledPaperGraphic in LaptopOs.cs, beside LaptopWallpaperGraphic
            // and MarkedWashGraphic) rather than a texture asset or a second implementation. Added
            // first, before any text/rule, so it sits behind everything the margin draws below —
            // same draw-order rule LaptopUi.MakeMarkedWash already documents.
            LaptopUi.MakeRuledPaper(panel, "RuledPaper");

            // B1-1/M-08 ruling (load-bearing under S28): MarginHeader.jsx (kit) is a title plus a
            // right-flushed count and its own 2px biro-deep rule — not one joined White string.
            // With letter-spacing unreachable (S28), colour is the only channel left telling the
            // literal title ("MY MARKS", --biro — this is the player's OWN margin, S33 confirms
            // biro-ruled headers name whose margin it is) from the dynamic count fact (--toner-2).
            // The old joined string also folded the staged-ticket count into the title; that fact
            // survives here in the count slot, since the kit's own count prop only defines the
            // selections half of it (margin.jsx:20) and nowhere else on this panel prints it.
            const float headerRight = 296f; // 324 - 14 - 14, the content width every row below uses.
            Text headerTitle = LaptopUi.MakeText(panel, "Title", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -10f), new Vector2(150f, 24f), 16, TextAnchor.UpperLeft, LaptopOs.Accent,
                "MY MARKS", _fontCond);
            string countText = $"{slip.Picks.Count} {Pluralize(slip.Picks.Count, "SELECTION")} · {run.Tickets.Count} STAGED";
            float countMaxWidth = Mathf.Max(0f, headerRight - headerTitle.preferredWidth - 8f);
            countText = LaptopUi.FitText(_font, countText, 13, countMaxWidth);
            LaptopUi.MakeText(panel, "Count", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-14f, -12f), new Vector2(countMaxWidth, 18f), 13, TextAnchor.UpperRight,
                LaptopOs.TonerSecondary, countText, _font);
            LaptopUi.MakePanel(panel, "HeaderRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -34f), new Vector2(headerRight, 2f), LaptopOs.BiroDeep);

            // S50 §1: the house status line is DELETED — 18px. It restated the board header's own
            // "ROUND n OF 8 · PRICES FINAL", which S37 forbids outright (the masthead carries the
            // run's scope, the board header the screen's, and nothing restates either), and the
            // markets C14 audit already carried it as invented (M-09). This was never an open
            // question — an unexecuted ruling is not a pending one.
            float y = -44f;
            if (slip.Picks.Count == 0)
            {
                LaptopUi.MakeText(panel, "Empty", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, y), new Vector2(300f, 26f), 13, TextAnchor.UpperLeft, LaptopOs.Muted,
                    "YOUR MARGIN IS CLEAR", _font);
                y -= 30f;
            }
            for (int i = 0; i < slip.Picks.Count; i++)
            {
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
                string away = LaptopUi.TeamShort(matchup.Away);
                string home = LaptopUi.TeamShort(matchup.Home);
                bool subjectIsHome = fields.Subject == matchup.Home.Name;
                bool subjectIsAway = !subjectIsHome && fields.Subject == matchup.Away.Name;
                string subjectRaw = subjectIsHome ? home : subjectIsAway ? away : fields.Subject;
                string subject = string.IsNullOrEmpty(subjectRaw) ? fields.Line : subjectRaw;
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
                LaptopUi.MakeText(panel, "LegCheck" + i, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, y), new Vector2(checkBoxWidth, 20f), 15, TextAnchor.UpperLeft,
                    LaptopOs.Accent, "✓", _font);
                // Named "Leg" + i (not e.g. "LegTeam") — SureThingEntryTests' entry-persistence
                // snapshot looks up "Leg0" by that exact name; this is the closest surviving analog
                // to the old single joined-string node, so the lookup still resolves.
                LaptopUi.MakeText(panel, "Leg" + i, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(contentX, y), new Vector2(teamWidth, 20f), 16, TextAnchor.UpperLeft,
                    LaptopOs.White, subject, _fontCond)
                    .horizontalOverflow = HorizontalWrapMode.Overflow;
                LaptopUi.MakeText(panel, "LegPrice" + i, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(priceX, y), new Vector2(priceWidth, 20f), 16, TextAnchor.UpperRight,
                    LaptopOs.White, price, _fontCond);

                // Line 2: "{market} · ENTRY {entry}" — roman, fact floor, --toner-3. Indented to the
                // content column past the check (kit: the check sits outside the flex column that
                // holds both lines), not the row's own left edge.
                LaptopUi.MakeText(panel, "LegDetail" + i, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(contentX, y - 20f), new Vector2(rowRight - contentX, 15f), 13,
                    TextAnchor.UpperLeft, LaptopOs.Muted, $"{fields.Market} · ENTRY {entry}", _font);

                // 1px --rule bottom rule (M-02), spanning the FULL row including the RUB OUT column
                // (kit: the border sits on the outer flex row, check+content+button together) — the
                // shared LaptopUi.MakeRule is hardcoded to --rule-soft and cannot be reused here.
                LaptopUi.MakePanel(panel, "LegRule" + i, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, y - 34f), new Vector2(headerRight, 1f), LaptopOs.Rule);

                int matchupIndex = pick.MatchupIndex;
                if (run.OwnsConsumable("profit_boost"))
                {
                    bool boosted = slip.BoostLeg == i;
                    int legIndex = i;
                    LaptopUi.MakeButton(panel, "Boost" + i, boosted ? "BOOST ✓" : "BOOST",
                        new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-76f, y + 5f),
                        new Vector2(58f, 24f), 13, boosted ? LaptopOs.MoneyGold : LaptopOs.SurfaceRaised,
                        LaptopOs.White, () => { slip.ToggleBoost(legIndex); _invalidate(); }, _font);
                }
                // RUB OUT stays 60x32 — S50 §3 keeps it at size deliberately, because a mis-click
                // here costs money. Vertically centred against the identity/market pair rather than
                // pinned to the first baseline, per S50 §2.
                LaptopUi.MakeButton(panel, "Remove" + i, "RUB OUT", new Vector2(1f, 1f), new Vector2(1f, 1f),
                    new Vector2(-12f, y + 1.5f), new Vector2(60f, 32f), 13, LaptopOs.Ink, LaptopOs.Muted,
                    () => { slip.Remove(matchupIndex); _lockArmed = false; _invalidate(); }, _fontCond);
                // S50 §2: the leg row takes S39's one-baseline discipline — a margin leg is the same
                // kind of object as a settled record (identity, price, market, state) and has no
                // business carrying a different vertical grammar. The yield comes from SPACING, which
                // is first in S50's standing order (spacing, then repetition, then nothing): the two
                // baselines close from 22px to 20px apart and the rule from 38 to 34, taking the step
                // 42 -> 35. Nothing that states a product fact was deleted to make the layout fit.
                y -= LegRowPitch;
            }

            // E-07 ruling: staged ticket receipts no longer render here. The 324px margin has no
            // room for up to MaxTicketsPerRound of them above the anchored action band (T47) — they
            // now render in the ENTRY sheet's own scrolling body instead (BuildScrollingBody), which
            // is where the kit puts them (screens.jsx:49-58, "PLACED THIS ROUND").
            y -= 4f;
            // B1-3/M-03 ruling (load-bearing under S28): MarginRow.jsx is a label/value pair with
            // its own 1px --rule bottom rule, not one joined "COMBINED {odds}" string in one face
            // and one colour — S28 names this the exact failure that leaves label and fact
            // indistinguishable once tracking is dropped. Value is condensed and right-flushed
            // across the full row width (kit: value gets marginLeft:auto); label stays roman,
            // --toner-3. "Combined" (not e.g. "CombinedValue") is kept on the value node because
            // SureThingMilestoneOneTests' contract-floor test looks this node up by that name.
            LaptopUi.MakeText(panel, "CombinedLabel", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, y), new Vector2(120f, 18f), 13, TextAnchor.UpperLeft, LaptopOs.Muted,
                "COMBINED", _font);
            LaptopUi.MakeText(panel, "Combined", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, y), new Vector2(headerRight, 22f), 18, TextAnchor.UpperRight, LaptopOs.White,
                slip.Picks.Count > 0 ? OddsFormat.American(slip.CombinedOdds) : "—", _fontCond);
            LaptopUi.MakePanel(panel, "CombinedRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, y - 24f), new Vector2(headerRight, 1f), LaptopOs.Rule);
            y -= 28f;

            bool freeHeld = run.OwnsConsumable("free_bet");
            bool donHeld = run.OwnsConsumable("double_or_nothing");
            if (freeHeld || donHeld)
            {
                if (freeHeld)
                    MakeModifier(panel, "FREE BET", TicketModifier.FreeBet, slip, 14f, y);
                if (donHeld)
                    MakeModifier(panel, "DOUBLE OR NOTHING", TicketModifier.DoubleOrNothing, slip,
                        freeHeld ? 148f : 14f, y);
                y -= 34f;
            }

            float chipX = 14f;
            MakeChip(panel, "10%", chipX, y, () => slip.SetStakeFraction(0.10)); chipX += 76f;
            MakeChip(panel, "25%", chipX, y, () => slip.SetStakeFraction(0.25)); chipX += 76f;
            MakeChip(panel, "50%", chipX, y, () => slip.SetStakeFraction(0.50)); chipX += 76f;
            MakeChip(panel, "MAX", chipX, y, () => slip.SetStakeFraction(1.00));
            y -= 34f;
            // Nudge keys are "raised chrome" per StakeButton.jsx and set in the condensed face;
            // the quick fraction chips above (10%/25%/50%/MAX) stay on the data face.
            MakeChip(panel, "−$10", 14f, y, () => slip.Nudge(-10), 88f, _fontCond);
            MakeChip(panel, "+$10", 110f, y, () => slip.Nudge(10), 88f, _fontCond);
            y -= 32f;
            LaptopUi.MakeText(panel, "Stake", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, y), new Vector2(300f, 24f), 16, TextAnchor.UpperLeft, LaptopOs.White,
                $"STAKE {LaptopUi.Money(slip.Stake)}", _font);
            y -= 32f;
            // B1-5/M-06: PayoutFigure.jsx (kit) carries a "POTENTIAL PAYOUT" label — roman, fact
            // floor, --toner-3 — above the value; shipped had none. Added by moving the cursor down
            // to make room, not by touching the 31px wax figure's own size/colour/font or the hand-
            // laid highlight band math below it, which still reads off whatever `y` the figure ends
            // up at (PayoutFigure.jsx:6,10).
            LaptopUi.MakeText(panel, "PayoutLabel", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, y), new Vector2(300f, 16f), 13, TextAnchor.UpperLeft, LaptopOs.Muted,
                "POTENTIAL PAYOUT", _font);
            y -= 18f;
            Text payout = LaptopUi.MakeText(panel, "Payout", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, y), new Vector2(300f, 36f), 31, TextAnchor.UpperLeft, LaptopOs.MoneyGold, $"{LaptopUi.Money(slip.ToWin)}", _fontCond);
            // Hand-laid wax highlight behind the one loud figure (palette-surething.css
            // --wax-highlight-*): a thin amber band, tilted, sized from the figure's own measured
            // width the same way InkRingGeometry sizes a ring — plus the highlight's own -3/+5 left/
            // right overshoot, not the ring's symmetric +8. Created after the text, then the text is
            // moved back to the top of the sibling order so it still draws over the band.
            float highlightWidth = Mathf.Max(40f, payout.preferredWidth) + 8f;
            RectTransform highlight = LaptopUi.MakePanel(panel, "PayoutHighlight", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(14f - 3f, y - 34f),
                new Vector2(highlightWidth, LaptopOs.WaxHighlightHeight), LaptopOs.MoneyGold);
            highlight.GetComponent<Image>().color = new Color(LaptopOs.MoneyGold.r, LaptopOs.MoneyGold.g,
                LaptopOs.MoneyGold.b, LaptopOs.WaxHighlightOpacity);
            highlight.localEulerAngles = new Vector3(0f, 0f, LaptopOs.WaxHighlightRotateDeg);
            payout.transform.SetAsLastSibling();
            y -= 40f;

            string blocker = slip.PlaceBlocker;
            // T47: the action stack STAYS ANCHORED. PLACE used to flow from the leg list, which is
            // how it walked down into LOCK as legs were added — and an un-anchored stack means the
            // most consequential control in the game sits at a different height because you bet
            // more. PLACE now joins LOCK and SKIP in the bottom-anchored band, matching the kit,
            // where all three live in one `marginTop:auto` group (margin.jsx:44-52).
            Button placeButton = LaptopUi.MakeButton(panel, "Place", "PLACE TICKET",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(14f, PlaceBandY), new Vector2(296f, 44f), 17,
                blocker == null ? LaptopOs.MoneyGold : LaptopOs.Surface,
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
                placeLabelRect.sizeDelta = new Vector2(296f, 26f);
                placeLabelRect.anchoredPosition = Vector2.zero;
                LaptopUi.MakeText(placeRect, "PlaceReason", new Vector2(.5f, 0f), new Vector2(.5f, 0f),
                    new Vector2(0f, 1f), new Vector2(288f, 17f), 13, TextAnchor.MiddleCenter,
                    LaptopOs.MoneyBad, blocker.ToUpperInvariant(), _font).horizontalOverflow = HorizontalWrapMode.Overflow;
            }

            bool hasWorkingMarks = slip.Picks.Count > 0;
            string lockLabel = boardFrozen ? "THE ROUND IS LOCKED" : "LOCK IT IN";
            string lockReason = hasWorkingMarks ? "PLACE OR CLEAR THIS WORKING SLIP" : run.Tickets.Count == 0 ? "PLACE AT LEAST ONE TICKET" : string.Empty;
            bool canLock = !boardFrozen && lockReason.Length == 0;
            Button lockButton = LaptopUi.MakeButton(panel, "Lock", lockLabel, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(14f, LockBandY), new Vector2(296f, LockBandH), 16,
                LaptopOs.Ink, canLock ? LaptopOs.White : LaptopOs.Muted,
                canLock ? () =>
                {
                    _lockArmed = false;
                    _armedRound = -1;
                    _host.director.LockRound();
                    _invalidate();
                } : null, _fontCond, canLock);
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
                    LaptopOs.MoneyBad, lockReason, _font).horizontalOverflow = HorizontalWrapMode.Overflow;
            }
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
                }, _font, !boardFrozen);
        }

        /// <summary>Renders <c>run.Tickets</c> as a top-down receipt stack starting at
        /// <paramref name="y"/>, sized off <paramref name="width"/> — the caller's own available
        /// width (E-07: previously always the 324px working margin, so the 296/280 numbers below were
        /// hardcoded to it; now <paramref name="width"/> is <see cref="BuildScrollingBody"/>'s
        /// rowWidth, so the same 14px/8px insets are applied to whatever width the ENTRY sheet's
        /// content actually has). Returns the new <paramref name="y"/> cursor below the last
        /// receipt.</summary>
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
                float receiptHeight = 30f + ticket.Legs.Count * 18f;
                RectTransform receipt = LaptopUi.MakePanel(receipts, "StagedTicket" + ticketIndex,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, receiptY),
                    new Vector2(receiptWidth, receiptHeight), LaptopOs.Surface);
                double combined = 1.0;
                for (int legIndex = 0; legIndex < ticket.Legs.Count; legIndex++)
                    combined *= ticket.Legs[legIndex].OfferedOdds;
                string identity = string.IsNullOrEmpty(ticket.Id)
                    ? $"{run.Round}.{ticketIndex + 1}" : ticket.Id;
                // "STAGED" is redundant (this whole block IS the staged-ticket receipt) and is the
                // first thing dropped to make room. The payout is the figure that matters most on
                // this line, so it is a protected suffix — FitText only ever trims the label ahead
                // of it, and only ever behind an ellipsis, never a silent cut.
                // Ticket identity/stake/combined/payout are all condensed per TicketReceipt.jsx; the
                // "TICKET"/"PAYS" words are structural, not field labels, so the whole header string
                // routes through _fontCond rather than being split across two Text components.
                string receiptHeaderText = LaptopUi.FitLabelKeepingSuffix(_fontCond, string.Empty,
                    $"TICKET {identity} · {LaptopUi.Money(ticket.Stake)} · {OddsFormat.American(combined)}",
                    $" · PAYS {LaptopUi.Money(ticket.PotentialPayout)}", 13, receiptTextWidth);
                LaptopUi.MakeText(receipt, "ReceiptHeader", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(8f, -4f), new Vector2(receiptTextWidth, 22f), 13, TextAnchor.UpperLeft,
                    LaptopOs.MoneyGold, receiptHeaderText, _fontCond);
                for (int legIndex = 0; legIndex < ticket.Legs.Count; legIndex++)
                {
                    Leg leg = ticket.Legs[legIndex];
                    string ticketLegText = LaptopUi.FitLabelKeepingSuffix(_fontCond, $"{legIndex + 1}. ",
                        CompactLegLabel(leg.Matchup, leg.Selection),
                        $"  {OddsFormat.American(leg.OfferedOdds)}", 13, receiptTextWidth);
                    LaptopUi.MakeText(receipt, "TicketLeg" + legIndex, new Vector2(0f, 1f),
                        new Vector2(0f, 1f), new Vector2(8f, -26f - legIndex * 18f),
                        new Vector2(receiptTextWidth, 18f), 13, TextAnchor.UpperLeft, LaptopOs.TonerSecondary,
                        ticketLegText, _fontCond);
                }
                LaptopUi.MakeRule(receipt, "ReceiptRule", new Vector2(0f, 0f), new Vector2(0f, 0f),
                    Vector2.zero, new Vector2(receiptWidth, 2f));
                receiptY -= receiptHeight + 8f;
            }
            return y - totalHeight;
        }

        private static string Pluralize(int count, string singular) => count == 1 ? singular : singular + "S";

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
        internal static (Vector2 position, Vector2 size) InkRingGeometry(Text text,
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
            float width = 68f, Font font = null)
        {
            LaptopUi.MakeButton(parent, "Chip" + label, label, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x, y), new Vector2(width, 32f), 13, LaptopOs.SurfaceRaised, LaptopOs.White,
                () => { onClick(); _invalidate(); }, font != null ? font : _font);
        }

        private Text _mirrorMarket;

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
            // Ticket identity ("TICKET n") and the terminal state word are both condensed per
            // TicketReceipt.jsx / RevealedState.jsx.
            LaptopUi.MakeText(card, "TicketTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, -8f), new Vector2(width - 16f, 24f), 16, TextAnchor.UpperLeft,
                stateColor, $"TICKET {ticket.Index + 1}  ·  {state}", _fontCond);
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
            Color stateColor = leg.State == RevealedLegState.Won ? LaptopOs.MoneyGold
                : leg.State == RevealedLegState.Lost ? LaptopOs.Muted
                : leg.State == RevealedLegState.Live ? LaptopOs.White : LaptopOs.TonerSecondary;
            // RevealedLeg.jsx's "team" slot (occupied here by either the team name or the market
            // label, whichever the leg carries) and its price are condensed; the state word matches
            // RevealedState.jsx.
            LaptopUi.MakeText(row, "LegLabel", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, -4f), new Vector2(width - 16f, 22f), 13, TextAnchor.UpperLeft,
                leg.State == RevealedLegState.Lost ? LaptopOs.Muted : LaptopOs.White, label, _fontCond);
            LaptopUi.MakeText(row, "LegPrice", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, -27f), new Vector2(84f, 22f), 13, TextAnchor.UpperLeft,
                stateColor, leg.AmericanOdds, _fontCond);
            Text stateText = LaptopUi.MakeText(row, "LegState", new Vector2(1f, 1f), new Vector2(1f, 1f),
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
            LaptopUi.MakeText(margin, "MirrorMarginTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -10f), new Vector2(296f, 24f), 16, TextAnchor.UpperLeft,
                LaptopOs.White, "TV-OWNED TALLY", _font);
            LaptopUi.MakeText(margin, "MirrorMarginRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -38f), new Vector2(296f, 24f), 13, TextAnchor.UpperLeft,
                LaptopOs.Muted, "READ ONLY  ·  NO SCORE  ·  NO PROBABILITY", _font);
            LaptopUi.MakeRule(margin, "MirrorMarginHeaderRule", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, -68f), new Vector2(324f, 2f));
            if (view == null || !view.HasTicket)
            {
                LaptopUi.MakeText(margin, "MirrorMarginEmpty", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, -90f), new Vector2(296f, 44f), 13, TextAnchor.UpperLeft,
                    LaptopOs.Muted, "THE TV HAS NOT RELEASED A RECEIPT.", _font);
                return;
            }
            for (int i = 0; i < view.Tickets.Count; i++)
            {
                RevealedTicket ticket = view.Tickets[i];
                string state = TicketStateWord(ticket.State);
                LaptopUi.MakeText(margin, "TicketSummary" + ticket.Index, new Vector2(0f, 1f),
                    new Vector2(0f, 1f), new Vector2(14f, -90f - i * 58f),
                    new Vector2(296f, 50f), 13, TextAnchor.UpperLeft,
                    ticket.Index == view.CurrentTicketIndex ? LaptopOs.White : LaptopOs.Muted,
                    $"TICKET {ticket.Index + 1}  ·  {state}\n{ticket.Legs.Count} LEGS  ·  {LaptopUi.Money(ticket.Stake)} → {LaptopUi.Money(ticket.PotentialPayout)}",
                    _font);
            }
        }

        private void BuildRewards(Run run)
        {
            RectTransform board = LaptopUi.MakePanel(_root, "RewardsBoard", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, -140f), new Vector2(700f, 530f), LaptopOs.Ink);
            RectTransform margin = LaptopUi.MakePanel(_root, "RewardsMargin", new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(0f, -140f), new Vector2(324f, 530f), LaptopOs.Ink);
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
                    LaptopOs.Muted, "NO OFFERS REMAIN ON THIS SHEET", _font);
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
                const float boardBottomPadding = 10f;
                float y = -74f;
                float floorY = -(530f - boardBottomPadding);
                int shown = 0;
                int total = run.ShopOffers.Count + run.ConsumableOffers.Count;

                for (int i = 0; i < run.ShopOffers.Count; i++)
                {
                    if (y - EstimateOfferHeight(run.ShopOffers[i].Description) < floorY) break;
                    y -= BuildRewardOffer(board, run, run.ShopOffers[i], i, y);
                    shown++;
                }
                for (int i = 0; i < run.ConsumableOffers.Count; i++)
                {
                    if (y - EstimateOfferHeight(run.ConsumableOffers[i].Description) < floorY) break;
                    y -= BuildConsumableOffer(board, run, run.ConsumableOffers[i], i, y);
                    shown++;
                }

                if (shown < total)
                {
                    // Hiding a purchasable offer without saying so would be the same class of
                    // untruth as the truncation this replaced: the screen would read as the whole
                    // shop. Stated as a plain fact, in toner — it is the house's document telling
                    // him what is on it, not a blocked action, so it is not the oxide stamp.
                    LaptopUi.MakeText(board, "OffersNotShown", new Vector2(0f, 0f), new Vector2(0f, 0f),
                        new Vector2(14f, 8f), new Vector2(672f, 20f), 13, TextAnchor.LowerLeft,
                        LaptopOs.TonerSecondary,
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

        private float BuildRewardOffer(RectTransform board, Run run, RelicDefinition offer, int index, float y)
        {
            RectTransform row = LaptopUi.MakePanel(board, "RewardOffer" + index, new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, y), new Vector2(700f, 56f), LaptopOs.Ink);
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
            Text description = LaptopUi.MakeText(row, "OfferDescription", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(14f, -29f), new Vector2(430f, 22f), 13,
                TextAnchor.UpperLeft, LaptopOs.TonerSecondary, offer.Description, _font);
            float descriptionHeight = Mathf.Max(18f, description.preferredHeight);
            description.rectTransform.sizeDelta = new Vector2(430f, descriptionHeight);
            float rowHeight = 29f + descriptionHeight + 9f;
            row.sizeDelta = new Vector2(700f, rowHeight);
            // S9 defect 1: a price is a printed figure, not the house's mark — wax regardless of
            // affordability. The BLOCKED reason beside it stays oxide; that IS the house acting.
            LaptopUi.MakeText(row, "Affordability", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-124f, -5f), new Vector2(118f, 22f), 13, TextAnchor.UpperRight,
                LaptopOs.MoneyGold, FormatComps(offer.Price), _fontCond);
            LaptopUi.MakeText(row, "BuyReason", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-124f, -29f), new Vector2(160f, 20f), 13, TextAnchor.UpperRight,
                canBuy ? LaptopOs.TonerSecondary : LaptopOs.MoneyBad, reason, _font);
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
                Vector2.zero, new Vector2(700f, 2f));
            return rowHeight;
        }

        private float BuildConsumableOffer(RectTransform board, Run run, ConsumableDefinition offer,
            int index, float y)
        {
            RectTransform row = LaptopUi.MakePanel(board, "ConsumableOffer" + index, new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, y), new Vector2(700f, 56f), LaptopOs.Ink);
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
            Text description = LaptopUi.MakeText(row, "OfferDescription", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(14f, -29f), new Vector2(430f, 22f), 13,
                TextAnchor.UpperLeft, LaptopOs.TonerSecondary, offer.Description, _font);
            float descriptionHeight = Mathf.Max(18f, description.preferredHeight);
            description.rectTransform.sizeDelta = new Vector2(430f, descriptionHeight);
            float rowHeight = 29f + descriptionHeight + 9f;
            row.sizeDelta = new Vector2(700f, rowHeight);
            // S9 defect 1: price is wax regardless of affordability; see BuildRewardOffer above.
            LaptopUi.MakeText(row, "Affordability", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-124f, -5f), new Vector2(118f, 22f), 13, TextAnchor.UpperRight,
                LaptopOs.MoneyGold, FormatComps(offer.Price), _fontCond);
            LaptopUi.MakeText(row, "BuyReason", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-124f, -29f), new Vector2(160f, 20f), 13, TextAnchor.UpperRight,
                canBuy ? LaptopOs.TonerSecondary : LaptopOs.MoneyBad, reason, _font);
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
                Vector2.zero, new Vector2(700f, 2f));
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
                    TextAnchor.UpperLeft, LaptopOs.Muted, "NO OWNED REWARDS TO SELL BACK.", _font);
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
            LaptopUi.MakeButton(margin, "LeaveRewards", "LEAVE — NEXT ROUND",
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
        private readonly Font _font;
        private readonly Font _fontCond; // see SportsbookApp's field comment — same seam
        private readonly Action _home;
        private readonly Action _sportsbook;

        public OldSlipsApp(RectTransform root, Font font, Font fontCond, Action home, Action sportsbook)
        {
            _root = root;
            _font = font;
            _fontCond = fontCond;
            _home = home;
            _sportsbook = sportsbook;
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
            // S9 defect 8: the column head used to sit ABOVE the scope caption, which put the caveat
            // prose between the head and the rows it labels — head and rows are separated now
            // (defect 7 below), so the caption reads first, then the head sits directly over its
            // table.
            LaptopUi.MakeText(board, "LedgerScope", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -8f), new Vector2(672f, 24f), 13, TextAnchor.UpperLeft,
                // S9 defect 7: "READ ONLY" also said once by the masthead's Scope line 64px above
                // (BuildLedgerChrome) — same meaning, said twice. The masthead is the one, prominent,
                // always-visible statement; this caption keeps only what it alone conveys — that the
                // list below is scoped to settled current-run records.
                LaptopOs.TonerSecondary, "SETTLED CURRENT-RUN RECORDS", _font);
            LaptopUi.MakeText(board, "LedgerColumnHead", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -34f), new Vector2(672f, 24f), 13, TextAnchor.UpperLeft,
                LaptopOs.Muted, "TICKET              STATE        STAKE             PAYOUT", _font);
            LaptopUi.MakeRule(board, "LedgerHeaderRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -66f), new Vector2(700f, 2f));

            int settledCount = 0;
            int wonCount = 0;
            int lostCount = 0;
            int cashedCount = 0;
            double settledStake = 0.0;
            double knownWinPayout = 0.0;
            float y = -72f;
            for (int i = 0; i < run.Tickets.Count; i++)
            {
                Ticket ticket = run.Tickets[i];
                if (ticket.State == TicketState.Open) continue;
                BuildLedgerTicket(board, ticket, settledCount, run.Round, y);
                y -= 48f + ticket.Legs.Count * 24f;
                settledCount++;
                settledStake += ticket.Stake;
                if (ticket.State == TicketState.Won)
                {
                    wonCount++;
                    knownWinPayout += ticket.PotentialPayout;
                }
                else if (ticket.State == TicketState.Lost)
                    lostCount++;
                else if (ticket.State == TicketState.CashedOut)
                    cashedCount++;
            }
            if (settledCount == 0)
            {
                LaptopUi.MakeText(board, "LedgerEmpty", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, -96f), new Vector2(672f, 30f), 16, TextAnchor.UpperLeft,
                    LaptopOs.Muted, "NO SETTLED TICKETS IN THE CURRENT RUN", _font);
                LaptopUi.MakeText(board, "LedgerEmptyScope", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, -130f), new Vector2(672f, 48f), 13, TextAnchor.UpperLeft,
                    LaptopOs.TonerSecondary,
                    "THIS LEDGER DOES NOT STORE CROSS-RUN HISTORY.\nOPEN TICKETS ARE NOT SETTLED RECORDS.",
                    _font);
            }

            BuildRecordSummary(margin, run, settledCount, wonCount, lostCount, cashedCount,
                settledStake, knownWinPayout);
            BuildLedgerTray();
        }

        private void BuildLedgerChrome(Run run)
        {
            RectTransform chrome = LaptopUi.MakePanel(_root, "Chrome", new Vector2(0f, 1f),
                new Vector2(0f, 1f), Vector2.zero, new Vector2(1024f, 140f), LaptopOs.Ink);
            NotebookChrome.BuildRail(chrome, 1024f, _font);

            RectTransform tabs = LaptopUi.MakePanel(chrome, "FormTabs", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, -34f), new Vector2(1024f, 38f), LaptopOs.Surface);
            RectTransform active = LaptopUi.MakePanel(tabs, "LedgerTab", new Vector2(0f, 0f),
                new Vector2(0f, 0f), new Vector2(14f, 3f), new Vector2(100f, 32f), LaptopOs.Ink);
            Text ledgerTabLabel = LaptopUi.MakeText(active, "LedgerTabLabel", new Vector2(.5f, .5f), new Vector2(.5f, .5f),
                Vector2.zero, new Vector2(100f, 30f), 13, TextAnchor.MiddleCenter,
                LaptopOs.White, "LEDGER", _font);
            ledgerTabLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            LaptopUi.MakeText(tabs, "Sheet", new Vector2(1f, .5f), new Vector2(1f, .5f),
                new Vector2(-14f, 0f), new Vector2(170f, 24f), 13, TextAnchor.MiddleRight,
                LaptopOs.Muted, "SHEET 1 OF 1", _font);

            RectTransform masthead = LaptopUi.MakePanel(chrome, "FormMasthead", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, -72f), new Vector2(1024f, 68f), LaptopOs.Ink);
            LaptopUi.MakeText(masthead, "Brand", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(16f, -8f), new Vector2(420f, 28f), 26, TextAnchor.UpperLeft,
                LaptopOs.White, "LEDGER", _fontCond);
            LaptopUi.MakeText(masthead, "Scope", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(17f, -38f), new Vector2(520f, 20f), 13, TextAnchor.UpperLeft,
                LaptopOs.Muted, "CURRENT RUN  ·  SETTLED TICKETS ONLY  ·  READ ONLY", _font);
            LaptopUi.MakeText(masthead, "Run", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-16f, -10f), new Vector2(360f, 30f), 20, TextAnchor.UpperRight,
                LaptopOs.White, $"ROUND {run.Round}  ·  BANK {LaptopUi.Money(run.Bank)}", _font);
        }

        private void BuildLedgerTicket(RectTransform board, Ticket ticket, int index, int round, float y)
        {
            float height = 46f + ticket.Legs.Count * 24f;
            RectTransform row = LaptopUi.MakePanel(board, "LedgerTicket" + index, new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, y), new Vector2(700f, height), LaptopOs.Ink);
            string identity = string.IsNullOrEmpty(ticket.Id) ? $"{round}.{index + 1}" : ticket.Id;
            string state = ticket.State == TicketState.Won ? "WON"
                : ticket.State == TicketState.Lost ? "LOST"
                : ticket.State == TicketState.CashedOut ? "CASHED OUT" : "OPEN";
            string payout = ticket.State == TicketState.Won ? LaptopUi.Money(ticket.PotentialPayout)
                : ticket.State == TicketState.Lost ? LaptopUi.Money(0)
                : "AMOUNT NOT RETAINED";
            Color stateColor = ticket.State == TicketState.Won ? LaptopOs.MoneyGold
                : ticket.State == TicketState.Lost ? LaptopOs.MoneyBad : LaptopOs.TonerSecondary;
            // Ruling S15: oxide is the house's stamp, not a generic loss wash. Only the struck
            // LOST word keeps the stamp color; the rest of this row recedes to toner-3, and the
            // returned $0 is pinned to plain toner — never oxide, never wax.
            bool lost = ticket.State == TicketState.Lost;
            Color identityColor = lost ? LaptopOs.Muted : LaptopOs.White;
            Color stakeColor = lost ? LaptopOs.Muted : LaptopOs.TonerSecondary;
            Color payoutColor = lost ? LaptopOs.White : stateColor;
            // "TICKET n" is condensed for the same reason as the MY BETS mirror's TicketTitle; the
            // state word matches LedgerEntry.jsx's terminal field / RevealedState.jsx.
            LaptopUi.MakeText(row, "TicketIdentity", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -4f), new Vector2(180f, 24f), 15, TextAnchor.UpperLeft,
                identityColor, "TICKET " + identity, _fontCond);
            // Anchor/pivot (1,1) plus right-aligned content, matching BuildMirrorLeg's LegState —
            // InkRingGeometry requires that exact top-right convention to size/place a strike.
            Text ticketStateText = LaptopUi.MakeText(row, "TicketState", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-382f, -4f), new Vector2(120f, 24f), 13, TextAnchor.UpperRight,
                stateColor, state, _fontCond);
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
            LaptopUi.MakeText(row, "TicketStake", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(322f, -4f), new Vector2(132f, 24f), 13, TextAnchor.UpperLeft,
                stakeColor, "STAKE " + LaptopUi.Money(ticket.Stake), _font);
            LaptopUi.MakeText(row, "TicketPayout", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-14f, -4f), new Vector2(228f, 24f), 13, TextAnchor.UpperRight,
                payoutColor, "PAYOUT " + payout, _font);
            LaptopUi.MakeRule(row, "TicketRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -30f), new Vector2(700f, 1f));

            for (int legIndex = 0; legIndex < ticket.Legs.Count; legIndex++)
            {
                Leg leg = ticket.Legs[legIndex];
                RectTransform legRow = LaptopUi.MakePanel(row, "LedgerLeg" + legIndex,
                    new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(0f, -32f - legIndex * 24f), new Vector2(700f, 23f), LaptopOs.Ink);
                string legState = leg.IsVoided ? "VOID"
                    : leg.RescuedWon || leg.State == LegState.Won ? "WON"
                    : leg.State == LegState.Lost ? "LOST" : "PENDING";
                // Same team-name/price-dominated combined string as the working margin's Leg/
                // TicketLeg rows above — condensed as one run. Composed via CompactLegLabel (S22
                // ruling) rather than leg.DisplayLabel, so the ledger and the working margin always
                // agree on how a leg reads.
                LaptopUi.MakeText(legRow, "LegIdentity", new Vector2(0f, .5f), new Vector2(0f, .5f),
                    new Vector2(28f, 0f), new Vector2(470f, 22f), 13, TextAnchor.MiddleLeft,
                    LaptopOs.TonerSecondary,
                    $"{legIndex + 1}. {SportsbookApp.CompactLegLabel(leg.Matchup, leg.Selection)}  {OddsFormat.American(leg.OfferedOdds)}", _fontCond);
                LaptopUi.MakeText(legRow, "LegState", new Vector2(1f, .5f), new Vector2(1f, .5f),
                    new Vector2(-14f, 0f), new Vector2(140f, 22f), 13, TextAnchor.MiddleRight,
                    LaptopOs.Muted, legState, _fontCond);
                LaptopUi.MakeRule(legRow, "LegRule", new Vector2(0f, 0f), new Vector2(0f, 0f),
                    Vector2.zero, new Vector2(700f, 1f));
            }
        }

        private void BuildRecordSummary(RectTransform margin, Run run, int settled, int won,
            int lost, int cashed, double stake, double knownPayout)
        {
            RectTransform summary = LaptopUi.MakePanel(margin, "RecordSummary", new Vector2(0f, 1f),
                new Vector2(0f, 1f), Vector2.zero, new Vector2(324f, 530f), LaptopOs.Ink);
            LaptopUi.MakeText(summary, "RecordTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -10f), new Vector2(296f, 26f), 18, TextAnchor.UpperLeft,
                LaptopOs.White, "CURRENT-RUN RECORD", _font);
            LaptopUi.MakeText(summary, "RecordScope", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -40f), new Vector2(296f, 44f), 13, TextAnchor.UpperLeft,
                LaptopOs.Muted, "SETTLED TICKETS EXPOSED BY\nRUN.TICKETS ONLY", _font);
            LaptopUi.MakeRule(summary, "RecordRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -88f), new Vector2(324f, 2f));
            LaptopUi.MakeText(summary, "SettledCount", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -104f), new Vector2(296f, 24f), 15, TextAnchor.UpperLeft,
                LaptopOs.White, $"SETTLED  {settled}", _font);
            // Each line leads with a terminal-state word (WON/LOST/CASHED OUT), not a field label —
            // same convention as LedgerEntry.jsx's terminal field, so condensed.
            LaptopUi.MakeText(summary, "TerminalCounts", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -136f), new Vector2(296f, 52f), 13, TextAnchor.UpperLeft,
                LaptopOs.TonerSecondary, $"WON  {won}\nLOST  {lost}\nCASHED OUT  {cashed}", _fontCond);
            LaptopUi.MakeText(summary, "SettledStake", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -208f), new Vector2(296f, 24f), 13, TextAnchor.UpperLeft,
                LaptopOs.TonerSecondary, "SETTLED STAKE  " + LaptopUi.Money(stake), _font);
            LaptopUi.MakeText(summary, "KnownPayout", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -240f), new Vector2(296f, 48f), 16, TextAnchor.UpperLeft,
                LaptopOs.MoneyGold, "KNOWN WIN PAYOUTS\n" + LaptopUi.Money(knownPayout), _font);
            LaptopUi.MakeRule(summary, "RecordScopeRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -304f), new Vector2(324f, 2f));
            LaptopUi.MakeText(summary, "CashOutDisclosure", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(14f, -322f), new Vector2(296f, 74f), 13,
                TextAnchor.UpperLeft, LaptopOs.Muted,
                "CASH-OUT AMOUNTS ARE NOT RETAINED.\nNO CROSS-RUN HISTORY IS INVENTED.", _font);
            // S9 defect 7: third "READ ONLY" of four — see BuildLedgerChrome's Scope, the one place
            // that now says it. This footer keeps only its own information, the round identity.
            LaptopUi.MakeText(summary, "RoundIdentity", new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(14f, 16f), new Vector2(296f, 24f), 13, TextAnchor.LowerLeft,
                LaptopOs.Muted, $"ROUND {run.Round}", _font);
        }

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
