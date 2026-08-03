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
            // S31: the persistent four-tab strip lives here, once, and OldSlipsApp.BuildLedgerChrome
            // calls the same static method rather than fabricating a second "LEDGER" tab of its own.
            BuildTabStrip(tabs, tab, run.Phase, "SHEET 1 OF 1", _font, _selectTab);
            RectTransform mast = LaptopUi.MakePanel(top, "FormMasthead", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -72f), new Vector2(1024f, 68f), LaptopOs.Ink);
            // F1: Masthead.jsx's own border-bottom (--rule-w-strong solid var(--rule)) — same
            // missing seam, into the board below. Board and masthead share LaptopOs.Ink, which is
            // why this one never showed up as a flat-colour pixel step even though the kit calls
            // for it unconditionally.
            LaptopUi.MakeRule(mast, "MastheadRule", new Vector2(0f, 0f), new Vector2(0f, 0f), Vector2.zero, new Vector2(1024f, 2f), LaptopOs.Rule);
            LaptopUi.MakeText(mast, "Brand", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -8f), new Vector2(300f, 28f), 26, TextAnchor.UpperLeft, LaptopOs.White, "SURETHING FORM", _fontCond);
            LaptopUi.MakeText(mast, "Run", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(17f, -38f), new Vector2(340f, 20f), 13, TextAnchor.UpperLeft, LaptopOs.Muted, $"ROUND {run.Round} OF {run.Config.Rounds}  ·  PRICES FINAL", _font);
            // S31: the masthead's run figures, shared with OldSlipsApp.BuildLedgerChrome so LEDGER
            // carries the exact same BANK/TARGET/TICKETS figures rather than a parallel string.
            BuildRunFigures(mast, run, _font);
        }

        /// <summary>S31: SectionTabs.jsx's own strip — the border-bottom, the four tabs and the
        /// meta line — built once here so every destination that carries it (including LEDGER,
        /// via OldSlipsApp.BuildLedgerChrome) shares this exact mechanism instead of a second
        /// hand-rolled copy. <paramref name="active"/> is null wherever the current destination
        /// is not one of the four tabs (LEDGER): SectionTabs.jsx itself renders every tab
        /// unselected when `active` matches none of `tabs`, so this reproduces that by
        /// construction rather than special-casing it.</summary>
        internal static void BuildTabStrip(RectTransform tabs, Tab? active, Phase phase, string meta,
            Font font, Action<Tab> selectTab)
        {
            // F1: SectionTabs.jsx's own border-bottom (--rule-w-strong solid var(--rule)) — flat
            // colour step into the masthead below, no seam drawn.
            LaptopUi.MakeRule(tabs, "TabsRule", new Vector2(0f, 0f), new Vector2(0f, 0f), Vector2.zero, new Vector2(1024f, 2f), LaptopOs.Rule);
            MakeTab(tabs, "FORM", Tab.Lobby, active, phase == Phase.Shop, font, selectTab);
            MakeTab(tabs, "ENTRY", Tab.Detail, active, phase == Phase.Shop, font, selectTab);
            MakeTab(tabs, "MY BETS", Tab.MyBets, active, phase == Phase.Shop, font, selectTab);
            MakeTab(tabs, "REWARDS", Tab.Rewards, active, phase != Phase.Shop, font, selectTab);
            LaptopUi.MakeText(tabs, "Sheet", new Vector2(1f, .5f), new Vector2(1f, .5f), new Vector2(-14f, 0f), new Vector2(170f, 24f), 13, TextAnchor.MiddleRight, LaptopOs.Muted, meta, font);
        }

        private static void MakeTab(RectTransform top, string label, Tab tab, Tab? selected, bool disabled,
            Font font, Action<Tab> selectTab)
        {
            float x = tab == Tab.Lobby ? 14f : tab == Tab.Detail ? 122f : tab == Tab.MyBets ? 230f : 358f;
            bool active = selected.HasValue && tab == selected.Value;
            LaptopUi.MakeButton(top, label, label, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(x, 3f), new Vector2(tab == Tab.MyBets ? 116f : 100f, 32f), 13,
                active ? LaptopOs.Ink : LaptopOs.Surface,
                disabled ? LaptopUi.Dim(LaptopOs.Muted) : active ? LaptopOs.White : LaptopOs.Muted,
                disabled ? null : () => { selectTab(tab); }, font, !disabled);
        }

        /// <summary>S31: the masthead's run figures (BANK/TARGET/TICKETS) — the register calls
        /// these "unchanged" across every destination that carries the masthead, so this is
        /// written once and OldSlipsApp.BuildLedgerChrome calls it too, instead of substituting a
        /// parallel condensed string.</summary>
        internal static void BuildRunFigures(RectTransform mast, Run run, Font font)
        {
            LaptopUi.MakeText(mast, "Figures", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-16f, -10f), new Vector2(610f, 48f), 21, TextAnchor.UpperRight, LaptopOs.White, $"BANK {LaptopUi.Money(run.Bank)}    TARGET {LaptopUi.Money(run.CurrentPayment)}    TICKETS {run.Tickets.Count}/{run.Config.MaxTicketsPerRound}", font);
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
            // S34: the 26px ruled-paper ground (margin.jsx), shared with every passive margin on
            // this surface via the one MarginRuledPaperGraphic class — added first so it sits
            // behind the header/legs/actions below it.
            LaptopUi.MakeMarginRuledPaper(panel, "RuledPaper");
            // F2: screens.jsx's sheet.borderRight (2px solid var(--rule)) — every screen's 700px
            // sheet and 324px margin meet with no seam between them. Drawn as this margin's own
            // left edge (global x=700) rather than the sheet's right edge so FORM and ENTRY, which
            // both call BuildSlip for this one panel, get it from a single call.
            LaptopUi.MakeRule(panel, "SheetDivider", new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(2f, 530f), LaptopOs.Rule);
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
            // S18: a wax primary action is field + wax-ink + a 2px wax-deep edge — MakeWaxPrimary
            // builds all three so this and LEAVE — NEXT ROUND can't drift apart.
            LaptopUi.MakeWaxPrimary(panel, "Place", "PLACE TICKET",
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
                    // C19 / S25 amended: REWARDS is the one list a ruling deliberately caps (S17),
                    // so its count line prints in --toner (LaptopOs.White) — was TonerSecondary
                    // (--toner-2), one step dimmer than the comment above already said it should be.
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
        private readonly Font _font;
        private readonly Font _fontCond; // see SportsbookApp's field comment — same seam
        private readonly Action _home;
        private readonly Action _sportsbook;
        // S31: drives the reused four-tab strip's navigation — clicking FORM/ENTRY/MY BETS/
        // REWARDS from LEDGER jumps straight to that destination, same as SectionTabs.jsx's own
        // onSelect (app.jsx:120). Distinct from _sportsbook above, which only drops the running
        // app to whichever tab it last showed (the tray's "SURETHING" slot).
        private readonly Action<SportsbookApp.Tab> _selectTab;

        public OldSlipsApp(RectTransform root, Font font, Font fontCond, Action home, Action sportsbook,
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

            var settled = new List<Ticket>();
            for (int i = 0; i < run.Tickets.Count; i++)
                if (run.Tickets[i].State != TicketState.Open) settled.Add(run.Tickets[i]);

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
            int cashedCount = 0;
            double settledStake = 0.0;
            double knownWinPayout = 0.0;
            float y = -52f;
            for (int i = 0; i < settled.Count; i++)
            {
                Ticket ticket = settled[i];
                BuildLedgerTicket(board, ticket, i, run.Round, y);
                // S32: LedgerEntry.jsx's own borderBottom (--rule-w solid --rule-soft) is the
                // separator now, drawn as this entry's own bottom edge inside BuildLedgerTicket —
                // so entries sit flush and the next one starts exactly one entry height down, not
                // one entry height plus the blank 2px gap this file used to leave.
                y -= LedgerEntryHeight(ticket);
                settledStake += ticket.Stake;
                if (ticket.State == TicketState.Won)
                    knownWinPayout += ticket.PotentialPayout;
                else if (ticket.State == TicketState.CashedOut)
                    cashedCount++;
            }
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

            BuildRecordSummary(margin, settled.Count, cashedCount, settledStake, knownWinPayout);

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
            // own Brand box exactly — "LEDGER" needs far less room than "SURETHING FORM" already
            // fits in 300). The old 420px had no neighbour to clear (the pre-S31 right-side text
            // started at local x=648); BuildRunFigures below now starts at x=398, and 420 would
            // have overlapped it by up to 38px.
            LaptopUi.MakeText(masthead, "Brand", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(16f, -8f), new Vector2(300f, 28f), 26, TextAnchor.UpperLeft,
                LaptopOs.White, "LEDGER", _fontCond);
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
            SportsbookApp.BuildRunFigures(masthead, run, _font);
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
        // untouched by the legs cell's deletion, still just "TICKET n.n"). Terminal keeps its own
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

        private void BuildLedgerTicket(RectTransform board, Ticket ticket, int index, int round, float y)
        {
            float height = LedgerEntryHeight(ticket);
            RectTransform row = LaptopUi.MakePanel(board, "LedgerTicket" + index, new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, y), new Vector2(700f, height), LaptopOs.Ink);
            string identity = string.IsNullOrEmpty(ticket.Id) ? $"{round}.{index + 1}" : ticket.Id;
            string state = ticket.State == TicketState.Won ? "WON"
                : ticket.State == TicketState.Lost ? "LOST"
                : ticket.State == TicketState.CashedOut ? "CASHED OUT" : "OPEN";
            // S36: the engine retains no cash-out amount. The absence is honest — never a
            // fabricated $0 and never "AMOUNT NOT RETAINED" — so the RETURNED value prints a plain
            // em dash, coloured toner-3 below, until engine retention lands.
            string returnedValue = ticket.State == TicketState.Won ? LaptopUi.Money(ticket.PotentialPayout)
                : ticket.State == TicketState.Lost ? LaptopUi.Money(0)
                : "—";
            // F5/F6 / LedgerEntry.jsx: `color: won ? var(--wax) : var(--toner-3)` applies to BOTH
            // the terminal word and the RETURNED value. S15 resolved LOST more precisely: oxide
            // belongs only to the strike drawn ACROSS the word (LedgerDeadStrike, below,
            // unchanged), never to a glyph fill — the word and the RETURNED value both recede to
            // toner-3 (LaptopOs.Muted) instead.
            // S36: CASHED OUT is wax, paired with WON exactly as the kit pairs them, on the
            // terminal word only. That pairing stops at RETURNED — an em dash is an absence, not a
            // fact to celebrate, so it stays toner-3 even beside a wax word.
            Color stateColor = ticket.State == TicketState.Won || ticket.State == TicketState.CashedOut
                ? LaptopOs.MoneyGold
                : ticket.State == TicketState.Lost ? LaptopOs.Muted : LaptopOs.TonerSecondary;
            bool lost = ticket.State == TicketState.Lost;
            Color returnedColor = lost || ticket.State == TicketState.CashedOut ? LaptopOs.Muted : stateColor;

            // S39: one baseline. Every cell below keeps its own snug, top-anchored box (unchanged
            // sizes from before this ruling) but is re-centred on y=-21, this band's own midpoint
            // (LedgerSummaryHeight/2 = 21) — canon's alignItems:center, reached now that the keys
            // that used to force identity/value/terminal onto separate lines are gone (S38).

            // --- identity (112px). LedgerEntry.jsx colours this --toner-2 unconditionally — no
            // won/lost branch — so it does not dim on a LOST ticket; the terminal word and
            // RETURNED value already carry that signal.
            Text identityText = LaptopUi.MakeText(row, "TicketIdentity", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(LedgerNumberX, -9f), new Vector2(LedgerNumberWidth, 24f), 16, TextAnchor.UpperLeft,
                LaptopOs.TonerSecondary, "TICKET " + identity, _fontCond);
            identityText.horizontalOverflow = HorizontalWrapMode.Overflow; // canon: whiteSpace nowrap

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
            Text ticketStateText = LaptopUi.MakeText(row, "TicketState", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-LedgerPadX, -13f), new Vector2(LedgerTerminalWidth, 24f), 13, TextAnchor.UpperRight,
                stateColor, state, _fontCond);
            ticketStateText.horizontalOverflow = HorizontalWrapMode.Overflow; // canon: whiteSpace nowrap
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
                new Vector2(0f, -LedgerSummaryHeight), new Vector2(700f, 1f));

            for (int legIndex = 0; legIndex < ticket.Legs.Count; legIndex++)
            {
                Leg leg = ticket.Legs[legIndex];
                RectTransform legRow = LaptopUi.MakePanel(row, "LedgerLeg" + legIndex,
                    new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(0f, -LedgerSummaryHeight - legIndex * LedgerLegRowHeight),
                    new Vector2(700f, 23f), LaptopOs.Ink);
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
                string legIdentityText = LaptopUi.FitLabelKeepingSuffix(_fontCond, $"{legIndex + 1}. ",
                    SportsbookApp.CompactLegLabel(leg.Matchup, leg.Selection),
                    $"  {OddsFormat.American(leg.OfferedOdds)}", 13, legIdentityWidth);
                LaptopUi.MakeText(legRow, "LegIdentity", new Vector2(0f, .5f), new Vector2(0f, .5f),
                    new Vector2(28f, 0f), new Vector2(legIdentityWidth, 22f), 13, TextAnchor.MiddleLeft,
                    legLost ? LaptopUi.Dim(LaptopOs.TonerSecondary) : LaptopOs.TonerSecondary,
                    legIdentityText, _fontCond);
                LaptopUi.MakeText(legRow, "LegState", new Vector2(1f, .5f), new Vector2(1f, .5f),
                    new Vector2(-14f, 0f), new Vector2(140f, 22f), 13, TextAnchor.MiddleRight,
                    legLost ? LaptopUi.Dim(LaptopOs.Muted) : LaptopOs.Muted, legState, _fontCond);
                LaptopUi.MakeRule(legRow, "LegRule", new Vector2(0f, 0f), new Vector2(0f, 0f),
                    Vector2.zero, new Vector2(700f, 1f));
            }

            // S32 canon: "every entry carries a borderBottom in --rule-soft" (LedgerEntry.jsx:
            // borderBottom: var(--rule-w) solid var(--rule-soft)). Replaces the blank 2px gap this
            // file used to leave between entries — entries now sit flush and this hairline is the
            // only separator, drawn at the entry's own bottom edge so it costs no extra height.
            LaptopUi.MakeRule(row, "LedgerEntryRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -height), new Vector2(700f, 1f));
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
        private void BuildRecordSummary(RectTransform margin, int settled, int cashed, double stake,
            double knownPayout)
        {
            RectTransform summary = LaptopUi.MakePanel(margin, "RecordSummary", new Vector2(0f, 1f),
                new Vector2(0f, 1f), Vector2.zero, new Vector2(324f, 530f), LaptopOs.Ink);
            // S34: the 26px ruled-paper ground, shared with every other margin on this surface via
            // the one MarginRuledPaperGraphic class — added first so it sits behind the header,
            // rows and note below it.
            LaptopUi.MakeMarginRuledPaper(summary, "RuledPaper");

            // MarginHeader.jsx: biro title, uppercase, closed by the 2px --biro-deep rule.
            LaptopUi.MakeText(summary, "RecordTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -12f), new Vector2(296f, 22f), 16, TextAnchor.UpperLeft,
                LaptopOs.Accent, "RECORD", _fontCond);
            LaptopUi.MakeRule(summary, "RecordHeaderRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -RecordHeaderHeight), new Vector2(296f, 2f), LaptopOs.BiroDeep);

            // MarginRow.jsx x3, in the kit's order (app.jsx:94-96).
            BuildRecordRow(summary, "RecordRowSettled", "TICKETS SETTLED",
                settled.ToString(CultureInfo.InvariantCulture), LaptopOs.White, -RecordHeaderHeight);
            BuildRecordRow(summary, "RecordRowStaked", "STAKED", LaptopUi.Money(stake), LaptopOs.White,
                -(RecordHeaderHeight + RecordRowHeight));
            // S36: the engine retains no cash-out amount, so once a settled run includes even one
            // cashed-out ticket, the true RETURNED total is missing an unknown figure and cannot be
            // honestly summed. The absence prints as a plain em dash in --toner-3 — never a
            // fabricated total and never $0 — until engine retention lands (approved, landing via
            // another seat).
            string returnedValue = cashed > 0 ? "—" : LaptopUi.Money(knownPayout);
            Color returnedColor = cashed > 0 ? LaptopOs.Muted : LaptopOs.MoneyGold;
            BuildRecordRow(summary, "RecordRowReturned", "RETURNED", returnedValue, returnedColor,
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
        private void BuildRecordRow(RectTransform summary, string name, string label, string value,
            Color valueColor, float rowTop)
        {
            LaptopUi.MakeText(summary, name + "Label", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, rowTop - 9f), new Vector2(150f, 20f), 13, TextAnchor.MiddleLeft,
                LaptopOs.Muted, label, _font);
            LaptopUi.MakeText(summary, name + "Value", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-14f, rowTop - 8f), new Vector2(140f, 22f), 18, TextAnchor.MiddleRight,
                valueColor, value, _fontCond);
            LaptopUi.MakeRule(summary, name + "Rule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, rowTop - RecordRowHeight), new Vector2(296f, 1f), LaptopOs.Rule);
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
