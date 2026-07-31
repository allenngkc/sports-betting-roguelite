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

        public SportsbookApp(RectTransform root, Font font, LaptopScreen host, Action invalidate,
            Action<Tab> selectTab, Action home, Action ledger)
        {
            _root = root;
            _font = font;
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
            RectTransform rail = LaptopUi.MakePanel(top, "NotebookRail", new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(1024f, 34f), LaptopOs.SurfaceRaised);
            LaptopUi.MakeText(rail, "Machine", new Vector2(0f, .5f), new Vector2(0f, .5f), new Vector2(14f, 0f), new Vector2(200f, 24f), 12, TextAnchor.MiddleLeft, LaptopOs.White, "■  NOTEBOOK", _font);
            LaptopUi.MakeText(rail, "Sticker", new Vector2(0f, .5f), new Vector2(0f, .5f), new Vector2(150f, 0f), new Vector2(160f, 24f), 12, TextAnchor.MiddleLeft, LaptopOs.Accent, "PROPERTY OF NOBODY", _font);
            LaptopUi.MakeText(rail, "Clock", new Vector2(1f, .5f), new Vector2(1f, .5f), new Vector2(-14f, 0f), new Vector2(140f, 24f), 12, TextAnchor.MiddleRight, LaptopOs.Muted, "02:47   ▰", _font);
            RectTransform tabs = LaptopUi.MakePanel(top, "FormTabs", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -34f), new Vector2(1024f, 38f), LaptopOs.Surface);
            MakeTab(tabs, "FORM", Tab.Lobby, tab, run.Phase == Phase.Shop);
            MakeTab(tabs, "ENTRY", Tab.Detail, tab, run.Phase == Phase.Shop);
            MakeTab(tabs, "MY BETS", Tab.MyBets, tab, run.Phase == Phase.Shop);
            MakeTab(tabs, "REWARDS", Tab.Rewards, tab, run.Phase != Phase.Shop);
            LaptopUi.MakeText(tabs, "Sheet", new Vector2(1f, .5f), new Vector2(1f, .5f), new Vector2(-14f, 0f), new Vector2(170f, 24f), 13, TextAnchor.MiddleRight, LaptopOs.Muted, "SHEET 1 OF 1", _font);
            RectTransform mast = LaptopUi.MakePanel(top, "FormMasthead", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -72f), new Vector2(1024f, 68f), LaptopOs.Ink);
            LaptopUi.MakeText(mast, "Brand", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -8f), new Vector2(300f, 28f), 26, TextAnchor.UpperLeft, LaptopOs.White, "SURETHING FORM", _font);
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
            LaptopUi.MakeText(card, "Number", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -10f), new Vector2(34f, 56f), 15, TextAnchor.UpperLeft, LaptopOs.Muted, (matchup.Index + 1).ToString("00"), _font);
            LaptopUi.MakeText(card, "Teams", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(54f, -8f), new Vector2(360f, 60f), 19, TextAnchor.UpperLeft, LaptopOs.White,
                $"{LaptopUi.TeamShort(matchup.Away)}  {matchup.Away.Record}\n{LaptopUi.TeamShort(matchup.Home)}  {matchup.Home.Record}", _font);

            bool awaySelected = slip.SelectionOn(matchup.Index) == MarketSelection.Moneyline(Side.Away);
            bool homeSelected = slip.SelectionOn(matchup.Index) == MarketSelection.Moneyline(Side.Home);
            LaptopUi.MakeButton(card, "AwayOdds", $"AWAY  {OddsFormat.American(matchup.AwayOdds)}",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(462f, -8f), new Vector2(112f, 32f), 19,
                LaptopOs.Ink, frozen ? LaptopUi.Dim(LaptopOs.Muted) : LaptopOs.White,
                frozen ? null : () => { slip.Toggle(matchup.Index, MarketSelection.Moneyline(Side.Away)); _invalidate(); }, _font, !frozen);
            LaptopUi.MakeButton(card, "HomeOdds", $"HOME  {OddsFormat.American(matchup.HomeOdds)}",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(462f, -43f), new Vector2(112f, 32f), 19,
                LaptopOs.Ink, frozen ? LaptopUi.Dim(LaptopOs.Muted) : LaptopOs.White,
                frozen ? null : () => { slip.Toggle(matchup.Index, MarketSelection.Moneyline(Side.Home)); _invalidate(); }, _font, !frozen);
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
            LaptopUi.MakeButton(card, "Details", "MORE ›", new Vector2(1f, .5f), new Vector2(1f, .5f),
                new Vector2(-12f, 0f), new Vector2(74f, 44f), 13, LaptopOs.Ink, LaptopOs.Muted,
                () => OpenDetail(matchup.Index), _font);
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

        private static Sprite ResolveStrike(int identity)
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
                $"{LaptopUi.TeamShort(matchup.Away)}  @  {LaptopUi.TeamShort(matchup.Home)}", _font);
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
                    LaptopUi.MakeSprite(offer, "WideBiroRing", ring, new Vector2(0f, 1f),
                        new Vector2(0f, 1f), new Vector2(-8f, -8f),
                        new Vector2(176f, 46f), LaptopOs.Accent);
            }
            LaptopUi.MakeText(offer, "MarketLabel" + key, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(-164f, 0f), new Vector2(156f, 32f), 13, TextAnchor.MiddleLeft,
                replacement ? LaptopOs.Accent : LaptopOs.TonerSecondary, label, _font);
            string price = OddsFormat.American(matchup.Odds(selection));
            LaptopUi.MakeButton(offer, "Market" + key,
                replacement ? "⇄  " + price : price, new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(160f, 32f), 19, LaptopOs.Ink,
                frozen ? LaptopUi.Dim(LaptopOs.Muted) : replacement ? LaptopOs.Accent : LaptopOs.White,
                frozen ? null : () => { slip.Toggle(matchup.Index, selection); _invalidate(); }, _font, !frozen);
            if (replacement)
            {
                RectTransform hint = LaptopUi.MakePanel(offer, "ReplacementHint",
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -31f),
                    new Vector2(160f, 2f), new Color(0f, 0f, 0f, 0f));
                LaptopUi.MakePanel(hint, "ReplacementUnderline" + key, Vector2.zero, Vector2.zero,
                    Vector2.zero, new Vector2(160f, 2f), LaptopOs.BiroDeep);
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
                string legText = LaptopUi.FitLabelKeepingSuffix(_font, $"{i + 1}. ",
                    CompactLegLabel(matchup, pick.Selection),
                    $"   {OddsFormat.American(matchup.Odds(pick.Selection))}", 13, legWidth);
                LaptopUi.MakeText(panel, "Leg" + i, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, y), new Vector2(legWidth, 24f), 13, TextAnchor.UpperLeft, LaptopOs.White,
                    legText, _font);
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
                LaptopUi.MakeButton(panel, "Remove" + i, "RUB OUT", new Vector2(1f, 1f), new Vector2(1f, 1f),
                    new Vector2(-12f, y + 8f), new Vector2(60f, 32f), 13, LaptopOs.Ink, LaptopOs.Muted,
                    () => { slip.Remove(matchupIndex); _lockArmed = false; _invalidate(); }, _font);
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
            MakeChip(panel, "−$10", 14f, y, () => slip.Nudge(-10), 88f);
            MakeChip(panel, "+$10", 110f, y, () => slip.Nudge(10), 88f);
            y -= 32f;
            LaptopUi.MakeText(panel, "Stake", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, y), new Vector2(300f, 24f), 16, TextAnchor.UpperLeft, LaptopOs.White,
                $"STAKE {LaptopUi.Money(slip.Stake)}", _font);
            y -= 32f;
            LaptopUi.MakeText(panel, "Payout", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, y), new Vector2(300f, 36f), 31, TextAnchor.UpperLeft, LaptopOs.MoneyGold, $"{LaptopUi.Money(slip.ToWin)}", _font);
            y -= 40f;

            string blocker = slip.PlaceBlocker;
            LaptopUi.MakeButton(panel, "Place", "PLACE TICKET",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, y), new Vector2(296f, 44f), 17,
                blocker == null ? LaptopOs.MoneyGold : LaptopOs.Surface,
                blocker == null ? LaptopOs.Ink : LaptopUi.Dim(LaptopOs.Muted),
                blocker == null ? () => { slip.Place(); _lockArmed = false; _armedRound = -1; _invalidate(); } : null, _font,
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
                } : null, _font, canLock);
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
                string receiptHeaderText = LaptopUi.FitLabelKeepingSuffix(_font, string.Empty,
                    $"TICKET {identity} · {LaptopUi.Money(ticket.Stake)} · {OddsFormat.American(combined)}",
                    $" · PAYS {LaptopUi.Money(ticket.PotentialPayout)}", 13, receiptTextWidth);
                LaptopUi.MakeText(receipt, "ReceiptHeader", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(8f, -4f), new Vector2(receiptTextWidth, 22f), 13, TextAnchor.UpperLeft,
                    LaptopOs.MoneyGold, receiptHeaderText, _font);
                for (int legIndex = 0; legIndex < ticket.Legs.Count; legIndex++)
                {
                    Leg leg = ticket.Legs[legIndex];
                    string ticketLegText = LaptopUi.FitLabelKeepingSuffix(_font, $"{legIndex + 1}. ",
                        CompactLegLabel(leg.Matchup, leg.Selection),
                        $"  {OddsFormat.American(leg.OfferedOdds)}", 13, receiptTextWidth);
                    LaptopUi.MakeText(receipt, "TicketLeg" + legIndex, new Vector2(0f, 1f),
                        new Vector2(0f, 1f), new Vector2(8f, -26f - legIndex * 18f),
                        new Vector2(receiptTextWidth, 18f), 13, TextAnchor.UpperLeft, LaptopOs.TonerSecondary,
                        ticketLegText, _font);
                }
                LaptopUi.MakeRule(receipt, "ReceiptRule", new Vector2(0f, 0f), new Vector2(0f, 0f),
                    Vector2.zero, new Vector2(296f, 2f));
                receiptY -= receiptHeight + 8f;
            }
            return y - totalHeight;
        }

        private static string Pluralize(int count, string singular) => count == 1 ? singular : singular + "S";

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

        private void MakeChip(RectTransform parent, string label, float x, float y, Action onClick, float width = 68f)
        {
            LaptopUi.MakeButton(parent, "Chip" + label, label, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x, y), new Vector2(width, 32f), 13, LaptopOs.SurfaceRaised, LaptopOs.White,
                () => { onClick(); _invalidate(); }, _font);
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
            LaptopUi.MakeText(card, "TicketTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, -8f), new Vector2(width - 16f, 24f), 16, TextAnchor.UpperLeft,
                stateColor, $"TICKET {ticket.Index + 1}  ·  {state}", _font);
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
            LaptopUi.MakeText(row, "LegLabel", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, -4f), new Vector2(width - 16f, 22f), 13, TextAnchor.UpperLeft,
                leg.State == RevealedLegState.Lost ? LaptopOs.Muted : LaptopOs.White, label, _font);
            LaptopUi.MakeText(row, "LegPrice", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, -27f), new Vector2(84f, 22f), 13, TextAnchor.UpperLeft,
                stateColor, leg.AmericanOdds, _font);
            Text stateText = LaptopUi.MakeText(row, "LegState", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-8f, -27f), new Vector2(112f, 22f), 13, TextAnchor.UpperRight,
                stateColor, state, _font);
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
                    LaptopUi.MakeSprite(row, "DeadStrike", strike, new Vector2(1f, 1f),
                        new Vector2(1f, 1f), new Vector2(-4f, -20f),
                        new Vector2(112f, 46f), LaptopOs.MoneyBad);
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
                float y = -74f;
                for (int i = 0; i < run.ShopOffers.Count; i++)
                {
                    BuildRewardOffer(board, run, run.ShopOffers[i], i, y);
                    y -= 72f;
                }
                for (int i = 0; i < run.ConsumableOffers.Count; i++)
                {
                    BuildConsumableOffer(board, run, run.ConsumableOffers[i], i, y);
                    y -= 72f;
                }
            }

            BuildRewardsMargin(margin, run);
        }

        private void BuildRewardOffer(RectTransform board, Run run, RelicDefinition offer, int index, float y)
        {
            RectTransform row = LaptopUi.MakePanel(board, "RewardOffer" + index, new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, y), new Vector2(700f, 68f), LaptopOs.Ink);
            bool enoughComps = offer.Price <= run.Comps;
            bool hasSlot = run.OwnedRelics.Count < run.Config.RelicSlots;
            bool canBuy = enoughComps && hasSlot && run.Phase == Phase.Shop;
            string reason = !hasSlot ? "RELIC SLOTS FULL"
                : !enoughComps ? $"NEED {(offer.Price - run.Comps).ToString("0.#", CultureInfo.InvariantCulture)} COMPS"
                : "AFFORDABLE";
            LaptopUi.MakeText(row, "OfferName", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -5f), new Vector2(430f, 22f), 15, TextAnchor.UpperLeft,
                LaptopOs.White, offer.Name.ToUpperInvariant(), _font);
            LaptopUi.MakeText(row, "OfferDescription", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -28f), new Vector2(430f, 36f), 13, TextAnchor.UpperLeft,
                LaptopOs.TonerSecondary, offer.Description, _font);
            LaptopUi.MakeText(row, "Affordability", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-124f, -5f), new Vector2(118f, 22f), 13, TextAnchor.UpperRight,
                canBuy ? LaptopOs.MoneyGold : LaptopOs.MoneyBad,
                $"{offer.Price.ToString("0.#", CultureInfo.InvariantCulture)} COMPS", _font);
            LaptopUi.MakeText(row, "BuyReason", new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-124f, 3f), new Vector2(160f, 20f), 13, TextAnchor.LowerRight,
                canBuy ? LaptopOs.TonerSecondary : LaptopOs.MoneyBad, reason, _font);
            LaptopUi.MakeButton(row, "Buy", "BUY", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-14f, 0f), new Vector2(96f, 40f), 13,
                canBuy ? LaptopOs.Accent : LaptopOs.SurfaceRaised,
                canBuy ? LaptopOs.White : LaptopUi.Dim(LaptopOs.Muted),
                canBuy ? () =>
                {
                    string error = _host.director.TryBuyRelic(index);
                    SetShopMessage("RELIC PURCHASE RECORDED", error);
                    _invalidate();
                } : null, _font, canBuy);
            LaptopUi.MakeRule(row, "OfferRule", new Vector2(0f, 0f), new Vector2(0f, 0f),
                Vector2.zero, new Vector2(700f, 2f));
        }

        private void BuildConsumableOffer(RectTransform board, Run run, ConsumableDefinition offer,
            int index, float y)
        {
            RectTransform row = LaptopUi.MakePanel(board, "ConsumableOffer" + index, new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, y), new Vector2(700f, 68f), LaptopOs.Ink);
            bool enoughComps = offer.Price <= run.Comps;
            bool hasSlot = run.OwnedConsumables.Count < run.Config.ConsumableSlots;
            bool canBuy = enoughComps && hasSlot && run.Phase == Phase.Shop;
            string reason = !hasSlot ? "CHARM SLOTS FULL"
                : !enoughComps ? $"NEED {(offer.Price - run.Comps).ToString("0.#", CultureInfo.InvariantCulture)} COMPS"
                : "AFFORDABLE";
            LaptopUi.MakeText(row, "OfferName", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -5f), new Vector2(430f, 22f), 15, TextAnchor.UpperLeft,
                LaptopOs.White, offer.Name.ToUpperInvariant() + "  ·  SINGLE USE", _font);
            LaptopUi.MakeText(row, "OfferDescription", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -28f), new Vector2(430f, 36f), 13, TextAnchor.UpperLeft,
                LaptopOs.TonerSecondary, offer.Description, _font);
            LaptopUi.MakeText(row, "Affordability", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-124f, -5f), new Vector2(118f, 22f), 13, TextAnchor.UpperRight,
                canBuy ? LaptopOs.MoneyGold : LaptopOs.MoneyBad,
                $"{offer.Price.ToString("0.#", CultureInfo.InvariantCulture)} COMPS", _font);
            LaptopUi.MakeText(row, "BuyReason", new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-124f, 3f), new Vector2(160f, 20f), 13, TextAnchor.LowerRight,
                canBuy ? LaptopOs.TonerSecondary : LaptopOs.MoneyBad, reason, _font);
            LaptopUi.MakeButton(row, "Buy", "BUY", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-14f, 0f), new Vector2(96f, 40f), 13,
                canBuy ? LaptopOs.Accent : LaptopOs.SurfaceRaised,
                canBuy ? LaptopOs.White : LaptopUi.Dim(LaptopOs.Muted),
                canBuy ? () =>
                {
                    string error = _host.director.TryBuyConsumable(index);
                    SetShopMessage("CHARM PURCHASE RECORDED", error);
                    _invalidate();
                } : null, _font, canBuy);
            LaptopUi.MakeRule(row, "OfferRule", new Vector2(0f, 0f), new Vector2(0f, 0f),
                Vector2.zero, new Vector2(700f, 2f));
        }

        private void BuildRewardsMargin(RectTransform margin, Run run)
        {
            LaptopUi.MakeText(margin, "RewardsTally", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -8f), new Vector2(296f, 28f), 20, TextAnchor.UpperLeft,
                LaptopOs.MoneyGold,
                $"{run.Comps.ToString("0.#", CultureInfo.InvariantCulture)} COMPS", _font);
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
                    LaptopOs.White, relic.Name.ToUpperInvariant(), _font);
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
                    LaptopOs.White, consumable.Name.ToUpperInvariant(), _font);
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
            LaptopUi.MakeButton(margin, "LeaveRewards", "LEAVE — NEXT ROUND",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(14f, 12f),
                new Vector2(296f, 48f), 15, canLeave ? LaptopOs.Accent : LaptopOs.SurfaceRaised,
                canLeave ? LaptopOs.White : LaptopUi.Dim(LaptopOs.Muted),
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
            RectTransform taskbar = LaptopUi.MakePanel(_root, "Taskbar", new Vector2(0f, 0f), new Vector2(0f, 0f),
                Vector2.zero, new Vector2(_root.sizeDelta.x, 34f), LaptopOs.SurfaceRaised);
            taskbar.name = "NotebookTray";
            LaptopUi.MakeButton(taskbar, "Home", "SURETHING", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(12f, 0f), new Vector2(110f, 22f), 12, LaptopOs.Ink, LaptopOs.White,
                _home, _font);
            LaptopUi.MakeButton(taskbar, "Ledger", "LEDGER", new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f), new Vector2(132f, 0f), new Vector2(88f, 32f), 12,
                LaptopOs.SurfaceRaised, LaptopOs.Muted, _ledger, _font);
            LaptopUi.MakeText(taskbar, "AppName", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(232f, 0f), new Vector2(210f, 24f), 12, TextAnchor.MiddleLeft, LaptopOs.Muted,
                "MESSAGES  1", _font);
            LaptopUi.MakeText(taskbar, "Clock", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-14f, 0f), new Vector2(260f, 24f), 12, TextAnchor.MiddleRight, LaptopOs.Muted,
                "DISK 61% FULL    NO UPDATES", _font);
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
        private readonly Action _home;
        private readonly Action _sportsbook;

        public OldSlipsApp(RectTransform root, Font font, Action home, Action sportsbook)
        {
            _root = root;
            _font = font;
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
            LaptopUi.MakeText(board, "LedgerColumnHead", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -8f), new Vector2(672f, 24f), 13, TextAnchor.UpperLeft,
                LaptopOs.Muted, "TICKET              STATE        STAKE             PAYOUT", _font);
            LaptopUi.MakeText(board, "LedgerScope", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -34f), new Vector2(672f, 24f), 13, TextAnchor.UpperLeft,
                LaptopOs.TonerSecondary, "SETTLED CURRENT-RUN RECORDS  ·  READ ONLY", _font);
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
            RectTransform rail = LaptopUi.MakePanel(chrome, "NotebookRail", new Vector2(0f, 1f),
                new Vector2(0f, 1f), Vector2.zero, new Vector2(1024f, 34f), LaptopOs.SurfaceRaised);
            LaptopUi.MakeText(rail, "Machine", new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(14f, 0f), new Vector2(200f, 24f), 13, TextAnchor.MiddleLeft,
                LaptopOs.White, "■  NOTEBOOK", _font);
            LaptopUi.MakeText(rail, "Sticker", new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(150f, 0f), new Vector2(200f, 24f), 13, TextAnchor.MiddleLeft,
                LaptopOs.Accent, "PROPERTY OF NOBODY", _font);
            LaptopUi.MakeText(rail, "Clock", new Vector2(1f, .5f), new Vector2(1f, .5f),
                new Vector2(-14f, 0f), new Vector2(140f, 24f), 13, TextAnchor.MiddleRight,
                LaptopOs.Muted, "02:47   ▰", _font);

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
                LaptopOs.White, "SURETHING LEDGER", _font);
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
            LaptopUi.MakeText(row, "TicketIdentity", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -4f), new Vector2(180f, 24f), 15, TextAnchor.UpperLeft,
                LaptopOs.White, "TICKET " + identity, _font);
            LaptopUi.MakeText(row, "TicketState", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(198f, -4f), new Vector2(120f, 24f), 13, TextAnchor.UpperLeft,
                stateColor, state, _font);
            LaptopUi.MakeText(row, "TicketStake", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(322f, -4f), new Vector2(132f, 24f), 13, TextAnchor.UpperLeft,
                LaptopOs.TonerSecondary, "STAKE " + LaptopUi.Money(ticket.Stake), _font);
            LaptopUi.MakeText(row, "TicketPayout", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-14f, -4f), new Vector2(228f, 24f), 13, TextAnchor.UpperRight,
                stateColor, "PAYOUT " + payout, _font);
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
                LaptopUi.MakeText(legRow, "LegIdentity", new Vector2(0f, .5f), new Vector2(0f, .5f),
                    new Vector2(28f, 0f), new Vector2(470f, 22f), 13, TextAnchor.MiddleLeft,
                    LaptopOs.TonerSecondary,
                    $"{legIndex + 1}. {leg.DisplayLabel}  {OddsFormat.American(leg.OfferedOdds)}", _font);
                LaptopUi.MakeText(legRow, "LegState", new Vector2(1f, .5f), new Vector2(1f, .5f),
                    new Vector2(-14f, 0f), new Vector2(140f, 22f), 13, TextAnchor.MiddleRight,
                    LaptopOs.Muted, legState, _font);
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
            LaptopUi.MakeText(summary, "TerminalCounts", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -136f), new Vector2(296f, 52f), 13, TextAnchor.UpperLeft,
                LaptopOs.TonerSecondary, $"WON  {won}\nLOST  {lost}\nCASHED OUT  {cashed}", _font);
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
            LaptopUi.MakeText(summary, "RoundIdentity", new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(14f, 16f), new Vector2(296f, 24f), 13, TextAnchor.LowerLeft,
                LaptopOs.Muted, $"ROUND {run.Round}  ·  READ ONLY", _font);
        }

        private void BuildLedgerTray()
        {
            RectTransform tray = LaptopUi.MakePanel(_root, "NotebookTray", new Vector2(0f, 0f),
                new Vector2(0f, 0f), Vector2.zero, new Vector2(1024f, 34f), LaptopOs.SurfaceRaised);
            LaptopUi.MakeButton(tray, "SureThing", "SURETHING", new Vector2(0f, .5f),
                new Vector2(0f, .5f), new Vector2(12f, 0f), new Vector2(110f, 32f), 13,
                LaptopOs.Ink, LaptopOs.White, _sportsbook, _font);
            LaptopUi.MakeText(tray, "LedgerActive", new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(136f, 0f), new Vector2(90f, 24f), 13, TextAnchor.MiddleLeft,
                LaptopOs.White, "LEDGER", _font);
            LaptopUi.MakeText(tray, "Messages", new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(232f, 0f), new Vector2(150f, 24f), 13, TextAnchor.MiddleLeft,
                LaptopOs.Muted, "MESSAGES  1", _font);
            LaptopUi.MakeButton(tray, "Home", "HOME", new Vector2(1f, .5f), new Vector2(1f, .5f),
                new Vector2(-300f, 0f), new Vector2(72f, 32f), 13, LaptopOs.SurfaceRaised,
                LaptopOs.Muted, _home, _font);
            LaptopUi.MakeText(tray, "SystemFacts", new Vector2(1f, .5f), new Vector2(1f, .5f),
                new Vector2(-14f, 0f), new Vector2(270f, 24f), 13, TextAnchor.MiddleRight,
                LaptopOs.Muted, "DISK 61% FULL    NO UPDATES", _font);
        }
    }
}
