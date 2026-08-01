using System;
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

        private void BuildLobby(Run run, BetslipModel slip, bool boardFrozen)
        {
            RectTransform board = LaptopUi.MakePanel(_root, "Board", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -140f), new Vector2(700f, 530f), LaptopOs.Ink);
            LaptopUi.MakeText(board, "BoardTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -5f), new Vector2(670f, 26f), 13, TextAnchor.UpperLeft, LaptopOs.Muted,
                boardFrozen ? "NO.   MATCHUP · RECORD                         MONEYLINE     BOARD CLOSED" : "NO.   MATCHUP · SEASON RECORD                         MONEYLINE     MORE", _font);

            for (int i = 0; i < run.CurrentSlate.Matchups.Count; i++)
            {
                Matchup matchup = run.CurrentSlate.Matchups[i];
                BuildMatchupCard(board, matchup, slip, boardFrozen,
                    new Vector2(0f, -26f - i * 78f));
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
            if (boardFrozen)
                LaptopUi.MakeText(body, "LockedMarketReason", new Vector2(1f, 1f), new Vector2(1f, 1f),
                    new Vector2(-14f, -8f), new Vector2(280f, 32f), 13, TextAnchor.UpperRight,
                    LaptopOs.MoneyBad, "ROUND LOCKED — WATCH MY BETS", _font);

            if (_detailTab == DetailTab.Goals)
                BuildMarketLines(body, slip, matchup, run.Config.GoalLines, MarketKind.TotalGoals,
                    "GOALS TOTAL", boardFrozen);
            else if (_detailTab == DetailTab.Btts)
                BuildBothTeamsScore(body, slip, matchup, boardFrozen);
            else if (_detailTab == DetailTab.Corners)
                BuildMarketLines(body, slip, matchup, run.Config.CornerLines, MarketKind.TotalCorners,
                    "CORNERS TOTAL", boardFrozen);
            else if (_detailTab == DetailTab.Cards)
                BuildMarketLines(body, slip, matchup, run.Config.CardLines, MarketKind.TotalCards,
                    "CARDS TOTAL", boardFrozen);
            else
                BuildPlayerLines(body, slip, matchup, boardFrozen);

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
            double[] lines, MarketKind kind, string title, bool frozen)
        {
            LaptopUi.MakeText(parent, "MarketTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -8f), new Vector2(670f, 32f), 16, TextAnchor.UpperLeft,
                LaptopOs.White, title, _font);
            for (int i = 0; i < lines.Length; i++)
            {
                double line = lines[i];
                MarketSelection over = new MarketSelection(kind, line, MarketChoice.Over);
                MarketSelection under = new MarketSelection(kind, line, MarketChoice.Under);
                float rowY = -48f - i * 42f;
                MakeMarketOffer(parent, slip, matchup, over, $"OVER {line:0.0}",
                    i * 2, 14f, rowY, frozen);
                MakeMarketOffer(parent, slip, matchup, under, $"UNDER {line:0.0}",
                    i * 2 + 1, 354f, rowY, frozen);
            }
        }

        private void BuildBothTeamsScore(RectTransform parent, BetslipModel slip, Matchup matchup,
            bool frozen)
        {
            LaptopUi.MakeText(parent, "BttsTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -8f), new Vector2(670f, 32f), 16, TextAnchor.UpperLeft, LaptopOs.White,
                "BOTH TEAMS TO SCORE", _font);
            MakeMarketOffer(parent, slip, matchup, MarketSelection.BothTeamsToScore(true), "YES",
                0, 14f, -48f, frozen);
            MakeMarketOffer(parent, slip, matchup, MarketSelection.BothTeamsToScore(false), "NO",
                1, 354f, -48f, frozen);
        }

        private void BuildPlayerLines(RectTransform parent, BetslipModel slip, Matchup matchup, bool frozen)
        {
            LaptopUi.MakeText(parent, "PlayersTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -8f), new Vector2(670f, 32f), 16, TextAnchor.UpperLeft, LaptopOs.White,
                "ANYTIME GOALSCORER", _font);
            int row = 0;
            foreach (MarketOffer offer in matchup.Markets)
            {
                if (offer.Selection.Kind != MarketKind.AnytimeScorer) continue;
                Player player = matchup.PlayerAt(offer.Selection.PlayerIndex);
                float x = row % 2 == 0 ? 14f : 354f;
                float rowY = -48f - (row / 2) * 42f;
                MakeMarketOffer(parent, slip, matchup, offer.Selection,
                    $"{player.Name.ToUpperInvariant()} [{player.Role}]", row, x, rowY, frozen);
                row++;
            }
        }

        private void MakeMarketOffer(RectTransform parent, BetslipModel slip, Matchup matchup,
            MarketSelection selection, string label, int offerIndex, float x, float y, bool frozen)
        {
            MarketSelection? existing = slip.SelectionOn(matchup.Index);
            bool selected = existing.HasValue && existing.Value == selection;
            bool replacement = existing.HasValue && !selected;
            string key = selection.Kind + selection.Choice.ToString()
                + selection.Line.ToString(CultureInfo.InvariantCulture) + selection.PlayerIndex;
            float priceX = x + 164f;
            RectTransform offer = LaptopUi.MakePanel(parent, "MarketOffer" + offerIndex,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(priceX, y),
                new Vector2(160f, 32f), new Color(0f, 0f, 0f, 0f));
            if (selected)
            {
                Sprite ring = ResolveWideRing(matchup.Index);
                if (ring != null)
                {
                    // Y offset is POSITIVE. With a top-left pivot, anchoredPosition.y moves the rect
                    // DOWN when negative, so the long-standing (-8,-8) pushed the ring 8px below the
                    // cell instead of overshooting 8px above it: the ring spanned -8..-54 against a
                    // cell of 0..-32, sitting under the number rather than around it. Only its upper
                    // arcs reached the row, which is what read as "the ring does not close".
                    //
                    // The sprite, its import settings, the mesh (FullRect, verified) and the stretch
                    // (Image.Simple) were all correct the whole time — the runtime dump confirmed no
                    // mask anywhere in the chain. Diagnosis cost three passes because a correct wide
                    // ellipse around a short price genuinely looks like two flat strokes plus distant
                    // end caps, and that was twice mistaken for a broken ring.
                    //
                    // Size is the cell + 16 per assets/ASSETS.md and the design system's
                    // InkMark.rect(): the real cell is 160x32, so 176x48.
                    const float overshoot = 8f;
                    Vector2 cellSize = new Vector2(160f, 32f);
                    LaptopUi.MakeSprite(offer, "WideBiroRing", ring, new Vector2(0f, 1f),
                        new Vector2(0f, 1f), new Vector2(-overshoot, overshoot),
                        cellSize + new Vector2(overshoot * 2f, overshoot * 2f), LaptopOs.Accent);
                }
            }
            // Law Two: biro blue marks the selection he made, nothing else. This offer's label/
            // price used to key off "replacement" (true for every OTHER offer in a matchup that
            // already has a pick) instead of "selected" — so every unpicked row rendered blue and
            // the actual pick rendered in plain toner, exactly backwards. Keyed off "selected" now;
            // "replacement" still drives the "⇄" swap-hint affordance and its underline, just no
            // longer in biro.
            LaptopUi.MakeText(offer, "MarketLabel" + key, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(-164f, 0f), new Vector2(156f, 32f), 13, TextAnchor.MiddleLeft,
                selected ? LaptopOs.Accent : LaptopOs.TonerSecondary, label, _font);
            string price = OddsFormat.American(matchup.Odds(selection));
            LaptopUi.MakeButton(offer, "Market" + key,
                replacement ? "⇄  " + price : price, new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(160f, 32f), 19, LaptopOs.Ink,
                frozen ? LaptopUi.Dim(LaptopOs.Muted) : selected ? LaptopOs.Accent : LaptopOs.White,
                frozen ? null : () => { slip.Toggle(matchup.Index, selection); _invalidate(); }, _fontCond, !frozen);
            if (replacement)
            {
                RectTransform hint = LaptopUi.MakePanel(offer, "ReplacementHint",
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -31f),
                    new Vector2(160f, 2f), new Color(0f, 0f, 0f, 0f));
                LaptopUi.MakePanel(hint, "ReplacementUnderline" + key, Vector2.zero, Vector2.zero,
                    Vector2.zero, new Vector2(160f, 2f), LaptopOs.TonerSecondary);
            }
        }

        private void BuildSlip(Run run, BetslipModel slip, bool boardFrozen)
        {
            RectTransform panel = LaptopUi.MakePanel(_root, "Slip", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -140f), new Vector2(324f, 530f), LaptopOs.Ink);
            panel.name = "WorkingMargin";
            const float titleWidth = 300f;
            string titleText = LaptopUi.FitText(_font,
                $"MY MARKS · {slip.Picks.Count} {Pluralize(slip.Picks.Count, "SELECTION")} · {run.Tickets.Count} STAGED",
                15, titleWidth);
            LaptopUi.MakeText(panel, "Title", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -10f), new Vector2(titleWidth, 24f), 15, TextAnchor.UpperLeft, LaptopOs.White,
                titleText, _font);
            LaptopUi.MakeText(panel, "Rule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -33f), new Vector2(300f, 18f), 13, TextAnchor.UpperLeft,
                boardFrozen ? LaptopOs.MoneyGold : LaptopOs.Muted,
                boardFrozen ? "PRICES FINAL — BOARD LOCKED" : "PRICES FINAL. NOTHING YOU DO MOVES THEM.", _font);

            float y = -58f;
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
                const float legWidth = 230f;
                // Team names and prices are both condensed per MarginLeg.jsx; the "N. " index and the
                // "ML — v" connector are minor structural filler riding along in the same string, not
                // field labels, so the whole line routes through _fontCond rather than being split.
                string legText = LaptopUi.FitLabelKeepingSuffix(_fontCond, $"{i + 1}. ",
                    CompactLegLabel(matchup, pick.Selection),
                    $"   {OddsFormat.American(matchup.Odds(pick.Selection))}", 13, legWidth);
                LaptopUi.MakeText(panel, "Leg" + i, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, y), new Vector2(legWidth, 24f), 13, TextAnchor.UpperLeft, LaptopOs.White,
                    legText, _fontCond);
                int matchupIndex = pick.MatchupIndex;
                if (run.OwnsConsumable("profit_boost"))
                {
                    bool boosted = slip.BoostLeg == i;
                    int legIndex = i;
                    LaptopUi.MakeButton(panel, "Boost" + i, boosted ? "BOOST ✓" : "BOOST",
                        new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-76f, y + 8f),
                        new Vector2(58f, 24f), 13, boosted ? LaptopOs.MoneyGold : LaptopOs.SurfaceRaised,
                        LaptopOs.White, () => { slip.ToggleBoost(legIndex); _invalidate(); }, _font);
                }
                // RUB OUT is an action label, set in the condensed face — RubOutButton.prompt.md.
                LaptopUi.MakeButton(panel, "Remove" + i, "RUB OUT", new Vector2(1f, 1f), new Vector2(1f, 1f),
                    new Vector2(-12f, y + 8f), new Vector2(60f, 32f), 13, LaptopOs.Ink, LaptopOs.Muted,
                    () => { slip.Remove(matchupIndex); _lockArmed = false; _invalidate(); }, _fontCond);
                y -= 27f;
            }

            if (run.Tickets.Count > 0)
                y = BuildStagedReceipt(panel, run, y - 4f);

            y -= 4f;
            LaptopUi.MakeText(panel, "Combined", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, y), new Vector2(300f, 22f), 13, TextAnchor.UpperLeft, LaptopOs.Muted,
                slip.Picks.Count > 0 ? $"COMBINED {OddsFormat.American(slip.CombinedOdds)}" : "COMBINED —", _font);
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
            // The one solid wax field on the surface (PlaceAction.jsx). Enabled, its label is
            // --wax-ink — punched-out type on wax, not the general document Ink used everywhere else.
            LaptopUi.MakeButton(panel, "Place", "PLACE TICKET",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, y), new Vector2(296f, 44f), 17,
                blocker == null ? LaptopOs.MoneyGold : LaptopOs.Surface,
                blocker == null ? LaptopOs.WaxInk : LaptopUi.Dim(LaptopOs.Muted),
                blocker == null ? () => { slip.Place(); _lockArmed = false; _armedRound = -1; _invalidate(); } : null, _fontCond,
                blocker == null && !boardFrozen);
            if (blocker != null)
                // Same overlap class as LockReason: the Place button spans y..y-44 and this label
                // was at y-19, i.e. inside it. Built after the button it drew over the button's own
                // centred "PLACE TICKET", so the two strings collided on one line. Sits below now.
                LaptopUi.MakeText(panel, "PlaceReason", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, y - 48f), new Vector2(296f, 20f), 13, TextAnchor.UpperLeft, LaptopOs.MoneyBad, blocker.ToUpperInvariant(), _font);

            bool hasWorkingMarks = slip.Picks.Count > 0;
            string lockLabel = boardFrozen ? "THE ROUND IS LOCKED" : "LOCK IT IN";
            string lockReason = hasWorkingMarks ? "PLACE OR CLEAR THIS WORKING SLIP" : run.Tickets.Count == 0 ? "PLACE AT LEAST ONE TICKET" : string.Empty;
            bool canLock = !boardFrozen && lockReason.Length == 0;
            LaptopUi.MakeButton(panel, "Lock", lockLabel, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(14f, 52f), new Vector2(296f, 52f), 16,
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
                // The two-stray-red-"P" defect was occlusion, not text rendering. The reason label
                // sat at y 26..48 while the Skip button below it spans y 8..42, and Skip is built
                // last, so it draws on top and buries all but the top ~2px of the line. The reason
                // string is 247px wide against Skip's 230px, so exactly one glyph escaped past each
                // edge of the button — the leading and trailing "P" of "PLACE ... SLIP". That is why
                // four passes at fonts, wrap modes and rect heights all failed: the glyphs were
                // always correct, they were simply behind a button. The label now sits above LOCK
                // IT IN (y 110..130), which also reads better — the cause is a caption on the
                // control it blocks, and the two actions stay visually separate.
                LaptopUi.MakeText(panel, "LockReason", new Vector2(.5f, 0f), new Vector2(.5f, 0f),
                    new Vector2(0f, 110f), new Vector2(280f, 20f), 13, TextAnchor.MiddleCenter,
                    LaptopOs.MoneyBad, lockReason, _font).horizontalOverflow = HorizontalWrapMode.Overflow;
            }
            LaptopUi.MakeButton(panel, "Skip", _lockArmed ? "PRESS AGAIN TO SKIP" : "SKIP ROUND — PRESS TWICE", new Vector2(.5f, 0f), new Vector2(.5f, 0f), new Vector2(0f, 8f), new Vector2(230f, 34f), 13, LaptopOs.Ink, _lockArmed ? LaptopOs.MoneyBad : LaptopOs.Muted,
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

        private float BuildStagedReceipt(RectTransform parent, Run run, float y)
        {
            float totalHeight = 0f;
            for (int i = 0; i < run.Tickets.Count; i++)
                totalHeight += 30f + run.Tickets[i].Legs.Count * 18f + 8f;
            RectTransform receipts = LaptopUi.MakePanel(parent, "StagedTickets", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(14f, y), new Vector2(296f, totalHeight),
                new Color(0f, 0f, 0f, 0f));
            float receiptY = 0f;
            for (int ticketIndex = 0; ticketIndex < run.Tickets.Count; ticketIndex++)
            {
                Ticket ticket = run.Tickets[ticketIndex];
                float receiptHeight = 30f + ticket.Legs.Count * 18f;
                RectTransform receipt = LaptopUi.MakePanel(receipts, "StagedTicket" + ticketIndex,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, receiptY),
                    new Vector2(296f, receiptHeight), LaptopOs.Surface);
                double combined = 1.0;
                for (int legIndex = 0; legIndex < ticket.Legs.Count; legIndex++)
                    combined *= ticket.Legs[legIndex].OfferedOdds;
                string identity = string.IsNullOrEmpty(ticket.Id)
                    ? $"{run.Round}.{ticketIndex + 1}" : ticket.Id;
                const float receiptTextWidth = 280f;
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
                    Vector2.zero, new Vector2(296f, 2f));
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

        /// <summary>A short-form of <see cref="MatchModel.DisplayLabel"/> for the width-starved
        /// working-margin and staged-receipt columns: team names are shortened the same way the
        /// board already does (<see cref="LaptopUi.TeamShort"/>), and a moneyline pick never repeats
        /// the picked team's name a second time the way the engine's own DisplayLabel does
        /// ("Duluth Plumbers ML — Duluth Plumbers v Tulsa Loopholes"). Internal so the PlayMode
        /// fixture can assert against the exact same production formula rather than a hand-kept
        /// duplicate that could quietly drift out of sync.</summary>
        internal static string CompactLegLabel(Matchup matchup, MarketSelection selection)
        {
            string away = LaptopUi.TeamShort(matchup.Away);
            string home = LaptopUi.TeamShort(matchup.Home);
            switch (selection.Kind)
            {
                case MarketKind.Moneyline:
                    bool pickedHome = selection.Choice == MarketChoice.Home;
                    return $"{(pickedHome ? home : away)} ML — v {(pickedHome ? away : home)}";
                case MarketKind.TotalGoals:
                    return $"{selection.Choice.ToString().ToUpperInvariant()} {selection.Line:0.0} GOALS — {away} v {home}";
                case MarketKind.BothTeamsToScore:
                    return $"BTTS {selection.Choice.ToString().ToUpperInvariant()} — {away} v {home}";
                case MarketKind.TotalCorners:
                    return $"{selection.Choice.ToString().ToUpperInvariant()} {selection.Line:0.0} CORNERS — {away} v {home}";
                case MarketKind.TotalCards:
                    return $"{selection.Choice.ToString().ToUpperInvariant()} {selection.Line:0.0} CARDS — {away} v {home}";
                case MarketKind.AnytimeScorer:
                    return $"{matchup.PlayerAt(selection.PlayerIndex).Name.ToUpperInvariant()} ANYTIME — {away} v {home}";
                default:
                    return selection.Kind.ToString();
            }
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

        private void BuildMirrorTicket(RectTransform parent, RevealedTicket ticket, Vector2 position,
            float width)
        {
            RectTransform card = LaptopUi.MakePanel(parent, "MirrorTicket" + ticket.Index,
                new Vector2(0f, 1f), new Vector2(0f, 1f), position, new Vector2(width, 448f),
                LaptopOs.Ink);
            string state = ticket.State == RevealedTicketState.Won ? "GREEN" : ticket.State == RevealedTicketState.Lost
                ? "DEAD" : ticket.State == RevealedTicketState.CashedOut ? "CASHED OUT" : "RIDING";
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
            string state = leg.State == RevealedLegState.Won ? "GREEN"
                : leg.State == RevealedLegState.Lost ? "DEAD"
                : leg.State == RevealedLegState.Voided ? "VOID"
                : leg.State == RevealedLegState.Live ? "LIVE" : "PENDING";
            Color stateColor = leg.State == RevealedLegState.Won ? LaptopOs.MoneyGold
                : leg.State == RevealedLegState.Lost ? LaptopOs.Muted
                : leg.State == RevealedLegState.Live ? LaptopOs.Accent : LaptopOs.TonerSecondary;
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
                string state = ticket.State == RevealedTicketState.Won ? "GREEN"
                    : ticket.State == RevealedTicketState.Lost ? "DEAD"
                    : ticket.State == RevealedTicketState.CashedOut ? "CASHED OUT" : "RIDING";
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
                // TicketLeg rows above — condensed as one run.
                LaptopUi.MakeText(legRow, "LegIdentity", new Vector2(0f, .5f), new Vector2(0f, .5f),
                    new Vector2(28f, 0f), new Vector2(470f, 22f), 13, TextAnchor.MiddleLeft,
                    LaptopOs.TonerSecondary,
                    $"{legIndex + 1}. {leg.DisplayLabel}  {OddsFormat.American(leg.OfferedOdds)}", _fontCond);
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
