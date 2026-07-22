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

        private enum DetailTab { Goals, Corners, Cards }

        private readonly RectTransform _root;
        private readonly Font _font;
        private readonly LaptopScreen _host;
        private readonly Action _invalidate;
        private readonly Action<Tab> _selectTab;
        private readonly Action _home;
        private bool _lockArmed;
        private string _shopError = string.Empty;
        private int _detailMatchup = -1;
        private DetailTab _detailTab = DetailTab.Goals;

        public SportsbookApp(RectTransform root, Font font, LaptopScreen host, Action invalidate,
            Action<Tab> selectTab, Action home)
        {
            _root = root;
            _font = font;
            _host = host;
            _invalidate = invalidate;
            _selectTab = selectTab;
            _home = home;
        }

        public void Render(Run run, BetslipModel slip, Tab tab, bool boardFrozen)
        {
            LaptopUi.ClearChildren(_root);
            LaptopUi.MakePanel(_root, "AppBacking", Vector2.zero, Vector2.zero, Vector2.zero,
                _root.sizeDelta, LaptopOs.Ink);
            BuildChrome(run, tab, boardFrozen);
            if (tab == Tab.Lobby) BuildLobby(run, slip, boardFrozen);
            else if (tab == Tab.Detail) BuildDetail(run, slip, boardFrozen);
            else if (tab == Tab.MyBets) BuildMyBets(_host.tv != null ? _host.tv.RevealedView : null, boardFrozen);
            else BuildRewards(run);
            BuildTaskbar();
        }

        private void BuildChrome(Run run, Tab tab, bool boardFrozen)
        {
            RectTransform top = LaptopUi.MakePanel(_root, "Chrome", new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(_root.sizeDelta.x, 104f), new Color(0.035f, 0.025f, 0.075f, 1f));
            LaptopUi.MakeText(top, "Sure", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -14f), new Vector2(92f, 30f), 22, TextAnchor.UpperLeft, LaptopOs.White,
                "SURE", _font);
            LaptopUi.MakeText(top, "Thing", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(110f, -14f), new Vector2(124f, 30f), 22, TextAnchor.UpperLeft, LaptopOs.Accent,
                "THING.", _font);
            LaptopUi.MakeText(top, "Tagline", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(25f, -44f), new Vector2(260f, 20f), 11, TextAnchor.UpperLeft, LaptopOs.Muted,
                "the number never lies", _font);

            string phase = boardFrozen ? "BOARD CLOSED" : run.Phase == Phase.Shop ? "REWARDS" : "LIVE BOARD";
            LaptopUi.MakeText(top, "Status", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-24f, -16f), new Vector2(420f, 22f), 12, TextAnchor.UpperRight,
                boardFrozen ? LaptopOs.Muted : LaptopOs.Accent,
                $"R{run.Round}/{run.Config.Rounds}   ·   {phase}", _font);
            LaptopUi.MakeText(top, "Bank", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-24f, -42f), new Vector2(420f, 22f), 12, TextAnchor.UpperRight, LaptopOs.White,
                $"BANK {LaptopUi.Money(run.Bank)}   ·   DUE {LaptopUi.Money(run.CurrentPayment)}   ·   COMPS {run.Comps.ToString("0.#", CultureInfo.InvariantCulture)}", _font);
            if (run.OwnedRelics.Count > 0)
            {
                string relics = "";
                foreach (RelicDefinition relic in run.OwnedRelics) relics += relic.Name.ToUpperInvariant() + "   ";
                LaptopUi.MakeText(top, "Relics", new Vector2(1f, 1f), new Vector2(1f, 1f),
                    new Vector2(-24f, -66f), new Vector2(420f, 20f), 9, TextAnchor.UpperRight,
                    LaptopOs.Muted, relics, _font);
            }

            MakeTab(top, "LOBBY", Tab.Lobby, tab, run.Phase == Phase.Shop);
            MakeTab(top, "MY BETS", Tab.MyBets, tab, run.Phase == Phase.Shop);
            MakeTab(top, "REWARDS", Tab.Rewards, tab, run.Phase != Phase.Shop);
        }

        private void MakeTab(RectTransform top, string label, Tab tab, Tab selected, bool disabled)
        {
            float x = tab == Tab.Lobby ? 292f : tab == Tab.MyBets ? 390f : 510f;
            bool active = tab == selected;
            LaptopUi.MakeButton(top, label, label, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(x, 10f), new Vector2(tab == Tab.MyBets ? 112f : 94f, 32f), 12,
                active ? LaptopOs.Accent : new Color(0f, 0f, 0f, 0f),
                disabled ? LaptopUi.Dim(LaptopOs.Muted) : active ? LaptopOs.White : LaptopOs.Muted,
                disabled ? null : () => { _selectTab(tab); }, _font, !disabled);
        }

        private void BuildLobby(Run run, BetslipModel slip, bool boardFrozen)
        {
            RectTransform board = LaptopUi.MakePanel(_root, "Board", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(10f, -114f), new Vector2(660f, _root.sizeDelta.y - 178f), new Color(0f, 0f, 0f, 0f));
            LaptopUi.MakeText(board, "BoardTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, -4f), new Vector2(640f, 24f), 13, TextAnchor.UpperLeft,
                boardFrozen ? LaptopOs.Muted : LaptopOs.Accent,
                boardFrozen ? "THE SHOW IS ON THE TV   ·   BOARD CLOSED" : "TODAY'S BOARD   ·   MONEYLINE", _font);
            LaptopUi.MakeText(board, "BoardSub", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, -27f), new Vector2(640f, 20f), 10, TextAnchor.UpperLeft, LaptopOs.Muted,
                "records are season W-L   ·   every price is American odds", _font);
            if (!boardFrozen && run.OwnsConsumable("bookies_marker"))
                LaptopUi.MakeButton(board, "Marker", "MARKER −25%", new Vector2(1f, 1f), new Vector2(1f, 1f),
                    new Vector2(-8f, -4f), new Vector2(132f, 25f), 10, LaptopOs.SurfaceRaised, LaptopOs.MoneyGold,
                    () => { _host.director.TryPlayMarker(); _invalidate(); }, _font);

            for (int i = 0; i < run.CurrentSlate.Matchups.Count; i++)
            {
                Matchup matchup = run.CurrentSlate.Matchups[i];
                int column = i % 2;
                int row = i / 2;
                BuildMatchupCard(board, matchup, slip, boardFrozen,
                    new Vector2(8f + column * 326f, -52f - row * 136f));
            }

            BuildSlip(run, slip, boardFrozen);
        }

        private void BuildMatchupCard(RectTransform parent, Matchup matchup, BetslipModel slip, bool frozen,
            Vector2 position)
        {
            RectTransform card = LaptopUi.MakePanel(parent, "Matchup" + matchup.Index, new Vector2(0f, 1f),
                new Vector2(0f, 1f), position, new Vector2(310f, 126f), LaptopOs.Surface);
            (uint homeRgb, uint awayRgb) = TheaterPalette.TeamColors(matchup.Home.Name, matchup.Away.Name);
            MakeDot(card, "AwayDot", new Vector2(14f, -19f), LaptopUi.FromRgb(awayRgb));
            MakeDot(card, "HomeDot", new Vector2(14f, -47f), LaptopUi.FromRgb(homeRgb));
            LaptopUi.MakeText(card, "Teams", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(28f, -8f), new Vector2(180f, 54f), 14, TextAnchor.UpperLeft, LaptopOs.White,
                $"<color=#{awayRgb:X6}>{LaptopUi.TeamShort(matchup.Away)}</color>  @\n" +
                $"<color=#{homeRgb:X6}>{LaptopUi.TeamShort(matchup.Home)}</color>", _font);
            LaptopUi.MakeText(card, "Records", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-12f, -10f), new Vector2(110f, 46f), 11, TextAnchor.UpperRight, LaptopOs.Muted,
                $"{matchup.Away.Record}\n{matchup.Home.Record}", _font);

            bool awaySelected = slip.SelectionOn(matchup.Index) == MarketSelection.Moneyline(Side.Away);
            bool homeSelected = slip.SelectionOn(matchup.Index) == MarketSelection.Moneyline(Side.Home);
            LaptopUi.MakeButton(card, "AwayOdds", $"AWAY  {OddsFormat.American(matchup.AwayOdds)}",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(12f, 30f), new Vector2(136f, 28f), 11,
                awaySelected ? LaptopOs.Accent : LaptopOs.SurfaceRaised,
                frozen ? LaptopUi.Dim(LaptopOs.Muted) : LaptopOs.White,
                frozen ? null : () => { slip.Toggle(matchup.Index, MarketSelection.Moneyline(Side.Away)); _invalidate(); }, _font, !frozen);
            LaptopUi.MakeButton(card, "HomeOdds", $"HOME  {OddsFormat.American(matchup.HomeOdds)}",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(158f, 30f), new Vector2(136f, 28f), 11,
                homeSelected ? LaptopOs.Accent : LaptopOs.SurfaceRaised,
                frozen ? LaptopUi.Dim(LaptopOs.Muted) : LaptopOs.White,
                frozen ? null : () => { slip.Toggle(matchup.Index, MarketSelection.Moneyline(Side.Home)); _invalidate(); }, _font, !frozen);
            LaptopUi.MakeButton(card, "Details", "DETAILS", new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-12f, 7f), new Vector2(104f, 18f), 9, LaptopOs.SurfaceRaised, LaptopOs.Accent,
                () => OpenDetail(matchup.Index), _font);
            LaptopUi.MakeText(card, "Soon", new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(14f, 7f), new Vector2(170f, 18f), 9, TextAnchor.LowerLeft, LaptopUi.Dim(LaptopOs.Muted),
                "goals · corners · cards · BTTS", _font);
        }

        private void OpenDetail(int matchupIndex)
        {
            _detailMatchup = matchupIndex;
            _detailTab = DetailTab.Goals;
            _selectTab(Tab.Detail);
        }

        private void BuildDetail(Run run, BetslipModel slip, bool boardFrozen)
        {
            if (_detailMatchup < 0 || _detailMatchup >= run.CurrentSlate.Matchups.Count)
            {
                _selectTab(Tab.Lobby);
                return;
            }

            Matchup matchup = run.CurrentSlate.Matchups[_detailMatchup];
            RectTransform panel = LaptopUi.MakePanel(_root, "Detail", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(18f, -114f), new Vector2(_root.sizeDelta.x - 36f, _root.sizeDelta.y - 178f), LaptopOs.Surface);
            LaptopUi.MakeButton(panel, "Back", "← BOARD", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -10f), new Vector2(104f, 28f), 10, LaptopOs.SurfaceRaised, LaptopOs.Accent,
                () => { _detailMatchup = -1; _selectTab(Tab.Lobby); }, _font);
            LaptopUi.MakeText(panel, "Header", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(132f, -8f), new Vector2(620f, 28f), 18, TextAnchor.UpperLeft, LaptopOs.White,
                $"{LaptopUi.TeamShort(matchup.Away)}  @  {LaptopUi.TeamShort(matchup.Home)}", _font);
            LaptopUi.MakeText(panel, "Records", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-20f, -12f), new Vector2(240f, 22f), 11, TextAnchor.UpperRight, LaptopOs.Muted,
                $"{matchup.Away.Record}   ·   {matchup.Home.Record}", _font);

            LaptopUi.MakeText(panel, "Stats", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(18f, -48f), new Vector2(850f, 24f), 11, TextAnchor.UpperLeft, LaptopOs.Muted,
                $"FORM  {matchup.Away.Name}: GF {matchup.AwayStats.GoalsFor:0.0}  COR {matchup.AwayStats.Corners:0.0}  CRD {matchup.AwayStats.Cards:0.0}    " +
                $"{matchup.Home.Name}: GF {matchup.HomeStats.GoalsFor:0.0}  COR {matchup.HomeStats.Corners:0.0}  CRD {matchup.HomeStats.Cards:0.0}", _font);

            MakeDetailTab(panel, "GOALS", DetailTab.Goals, 18f);
            MakeDetailTab(panel, "CORNERS", DetailTab.Corners, 126f);
            MakeDetailTab(panel, "CARDS", DetailTab.Cards, 236f);

            if (_detailTab == DetailTab.Goals)
            {
                BuildMarketLines(panel, run, slip, matchup, run.Config.GoalLines, MarketKind.TotalGoals, "GOALS", boardFrozen, -122f);
                // BTTS lives on the Goals tab, clear of the full ladder (title 28 + rows at 38 each).
                BuildBothTeamsScore(panel, slip, matchup, boardFrozen,
                    -122f - 28f - run.Config.GoalLines.Length * 38f - 10f);
            }
            else if (_detailTab == DetailTab.Corners)
                BuildMarketLines(panel, run, slip, matchup, run.Config.CornerLines, MarketKind.TotalCorners, "CORNERS", boardFrozen, -122f);
            else
                BuildMarketLines(panel, run, slip, matchup, run.Config.CardLines, MarketKind.TotalCards, "CARDS", boardFrozen, -122f);
        }

        private void MakeDetailTab(RectTransform parent, string label, DetailTab tab, float x)
        {
            bool active = _detailTab == tab;
            LaptopUi.MakeButton(parent, "DetailTab" + label, label, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x, -78f), new Vector2(label == "CORNERS" ? 102f : 94f, 28f), 10,
                active ? LaptopOs.Accent : LaptopOs.SurfaceRaised, LaptopOs.White,
                () => { _detailTab = tab; _invalidate(); }, _font);
        }

        private void BuildMarketLines(RectTransform parent, Run run, BetslipModel slip, Matchup matchup,
            double[] lines, MarketKind kind, string title, bool frozen, float y)
        {
            LaptopUi.MakeText(parent, "MarketTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(18f, y), new Vector2(340f, 22f), 12, TextAnchor.UpperLeft, LaptopOs.Accent,
                $"{title} TOTALS", _font);
            for (int i = 0; i < lines.Length; i++)
            {
                double line = lines[i];
                MarketSelection over = new MarketSelection(kind, line, MarketChoice.Over);
                MarketSelection under = new MarketSelection(kind, line, MarketChoice.Under);
                float rowY = y - 28f - i * 38f;
                MakeMarketButton(parent, slip, matchup, over, $"OVER {line:0.0}", 18f, rowY, frozen);
                MakeMarketButton(parent, slip, matchup, under, $"UNDER {line:0.0}", 196f, rowY, frozen);
            }
        }

        private void BuildBothTeamsScore(RectTransform parent, BetslipModel slip, Matchup matchup,
            bool frozen, float y)
        {
            LaptopUi.MakeText(parent, "BttsTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(18f, y), new Vector2(340f, 22f), 12, TextAnchor.UpperLeft, LaptopOs.Accent,
                "BOTH TEAMS TO SCORE", _font);
            MakeMarketButton(parent, slip, matchup, MarketSelection.BothTeamsToScore(true), "YES", 18f, y - 28f, frozen);
            MakeMarketButton(parent, slip, matchup, MarketSelection.BothTeamsToScore(false), "NO", 196f, y - 28f, frozen);
        }

        private void MakeMarketButton(RectTransform parent, BetslipModel slip, Matchup matchup,
            MarketSelection selection, string label, float x, float y, bool frozen)
        {
            bool selected = slip.SelectionOn(matchup.Index) == selection;
            LaptopUi.MakeButton(parent, "Market" + selection.Kind + selection.Choice + selection.Line.ToString(CultureInfo.InvariantCulture),
                $"{label}  {OddsFormat.American(matchup.Odds(selection))}", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x, y), new Vector2(164f, 30f), 11, selected ? LaptopOs.Accent : LaptopOs.SurfaceRaised,
                frozen ? LaptopUi.Dim(LaptopOs.Muted) : LaptopOs.White,
                frozen ? null : () => { slip.Toggle(matchup.Index, selection); _invalidate(); }, _font, !frozen);
        }

        private void BuildSlip(Run run, BetslipModel slip, bool boardFrozen)
        {
            RectTransform panel = LaptopUi.MakePanel(_root, "Slip", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-12f, -114f), new Vector2(330f, _root.sizeDelta.y - 178f), LaptopOs.Surface);
            LaptopUi.MakeText(panel, "Title", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -10f), new Vector2(300f, 24f), 15, TextAnchor.UpperLeft, LaptopOs.White,
                $"BETSLIP   ·   {run.Tickets.Count}/{run.Config.MaxTicketsPerRound}", _font);
            LaptopUi.MakeText(panel, "Rule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -33f), new Vector2(300f, 18f), 10, TextAnchor.UpperLeft,
                boardFrozen ? LaptopOs.MoneyGold : LaptopOs.Muted,
                boardFrozen ? "prices locked while the TV sweats" : "build a parlay, then lock it in", _font);

            float y = -58f;
            if (slip.Picks.Count == 0)
            {
                LaptopUi.MakeText(panel, "Empty", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, y), new Vector2(300f, 26f), 12, TextAnchor.UpperLeft, LaptopOs.Muted,
                    "your slip is empty", _font);
                y -= 30f;
            }
            for (int i = 0; i < slip.Picks.Count; i++)
            {
                Pick pick = slip.Picks[i];
                Matchup matchup = run.CurrentSlate.Matchups[pick.MatchupIndex];
                LaptopUi.MakeText(panel, "Leg" + i, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(14f, y), new Vector2(238f, 24f), 12, TextAnchor.UpperLeft, LaptopOs.White,
                    $"{i + 1}. {MatchModel.DisplayLabel(matchup, pick.Selection)}   {OddsFormat.American(matchup.Odds(pick.Selection))}", _font);
                int matchupIndex = pick.MatchupIndex;
                if (run.OwnsConsumable("profit_boost"))
                {
                    bool boosted = slip.BoostLeg == i;
                    int legIndex = i;
                    LaptopUi.MakeButton(panel, "Boost" + i, boosted ? "BOOST ✓" : "BOOST",
                        new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-76f, y + 8f),
                        new Vector2(58f, 24f), 9, boosted ? LaptopOs.MoneyGold : LaptopOs.SurfaceRaised,
                        LaptopOs.White, () => { slip.ToggleBoost(legIndex); _invalidate(); }, _font);
                }
                LaptopUi.MakeButton(panel, "Remove" + i, "×", new Vector2(1f, 1f), new Vector2(1f, 1f),
                    new Vector2(-12f, y + 8f), new Vector2(25f, 24f), 16, LaptopOs.SurfaceRaised, LaptopOs.MoneyBad,
                    () => { slip.Remove(matchupIndex); _lockArmed = false; _invalidate(); }, _font);
                y -= 27f;
            }

            y -= 4f;
            LaptopUi.MakeText(panel, "Combined", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, y), new Vector2(300f, 22f), 12, TextAnchor.UpperLeft, LaptopOs.Muted,
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
            MakeChip(panel, "10%", chipX, y, () => slip.SetStakeFraction(0.10)); chipX += 62f;
            MakeChip(panel, "25%", chipX, y, () => slip.SetStakeFraction(0.25)); chipX += 62f;
            MakeChip(panel, "50%", chipX, y, () => slip.SetStakeFraction(0.50)); chipX += 62f;
            MakeChip(panel, "MAX", chipX, y, () => slip.SetStakeFraction(1.00));
            y -= 34f;
            MakeChip(panel, "−$10", 14f, y, () => slip.Nudge(-10), 70f);
            MakeChip(panel, "+$10", 90f, y, () => slip.Nudge(10), 70f);
            y -= 32f;
            LaptopUi.MakeText(panel, "Stake", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, y), new Vector2(300f, 24f), 15, TextAnchor.UpperLeft, LaptopOs.White,
                $"STAKE {LaptopUi.Money(slip.Stake)}   →   TO WIN {LaptopUi.Money(slip.ToWin)}", _font);
            y -= 32f;

            string blocker = slip.PlaceBlocker;
            LaptopUi.MakeButton(panel, "Place", blocker == null ? "PLACE TICKET" : blocker.ToUpperInvariant(),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, y), new Vector2(302f, 34f), 12,
                blocker == null ? LaptopOs.SurfaceRaised : LaptopOs.Surface,
                blocker == null ? LaptopOs.MoneyGood : LaptopUi.Dim(LaptopOs.Muted),
                blocker == null ? () => { slip.Place(); _lockArmed = false; _invalidate(); } : null, _font,
                blocker == null && !boardFrozen);

            string lockLabel = boardFrozen ? "THE ROUND IS LOCKED" : run.Tickets.Count > 0 ? "LOCK IT IN" :
                _lockArmed ? "NO BETS — SURE?" : "LOCK IT IN (NO BETS)";
            bool canLock = !boardFrozen;
            LaptopUi.MakeButton(panel, "Lock", lockLabel, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(14f, 12f), new Vector2(302f, 40f), 15,
                _lockArmed ? LaptopOs.MoneyBad : LaptopOs.Accent,
                LaptopOs.White,
                canLock ? () =>
                {
                    if (run.Tickets.Count == 0 && !_lockArmed) { _lockArmed = true; _invalidate(); return; }
                    _lockArmed = false;
                    _host.director.LockRound();
                    _invalidate();
                } : null, _font, canLock);
        }

        private void MakeModifier(RectTransform parent, string label, TicketModifier modifier, BetslipModel slip,
            float x, float y)
        {
            bool selected = slip.Modifier == modifier;
            LaptopUi.MakeButton(parent, label, selected ? label + " ✓" : label,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, y), new Vector2(132f, 27f), 9,
                selected ? LaptopOs.Accent : LaptopOs.SurfaceRaised, LaptopOs.White,
                () => { slip.ToggleModifier(modifier); _invalidate(); }, _font);
        }

        private void MakeChip(RectTransform parent, string label, float x, float y, Action onClick, float width = 56f)
        {
            LaptopUi.MakeButton(parent, "Chip" + label, label, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x, y), new Vector2(width, 26f), 10, LaptopOs.SurfaceRaised, LaptopOs.White,
                () => { onClick(); _invalidate(); }, _font);
        }

        private Text _mirrorMarket;

        /// <summary>In-place refresh of the fast mirror values (clock, score, prob,
        /// suspension). Called every OS tick while MY BETS is visible — a ticking minute
        /// must never rebuild the canvas (Sol, F_0.3.0 performance finding).</summary>
        public void UpdateMirrorDisplay(RevealedView view)
        {
            if (view == null || !view.HasTicket) return;
            if (_mirrorMarket != null)
            {
                _mirrorMarket.text = view.MarketSuspended
                    ? "MARKET SUSPENDED — the scene is still playing"
                    : "MARKET LIVE — cash-out remains on the TV";
                _mirrorMarket.color = view.MarketSuspended ? LaptopOs.Muted : LaptopOs.White;
            }
        }

        private void BuildMyBets(RevealedView view, bool boardFrozen)
        {
            // No backing panel here: the chrome header and tabs are earlier siblings and an
            // opaque full-screen panel would bury them (Sol, F_0.3.0 finding 1).
            LaptopUi.MakeText(_root, "Banner", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -130f), new Vector2(950f, 34f), 16, TextAnchor.UpperLeft,
                boardFrozen ? LaptopOs.Accent : LaptopOs.Muted,
                boardFrozen ? "the show is on the TV, press E while seated"
                    : "no active sweat — the board is open", _font);
            LaptopUi.MakeText(_root, "MirrorRule", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -160f), new Vector2(950f, 22f), 11, TextAnchor.UpperLeft, LaptopOs.Muted,
                "MY BETS is a read-only mirror. The TV reveals the number when the scene pays off.", _font);

            _mirrorMarket = null;
            if (view == null || !view.HasTicket)
            {
                LaptopUi.MakeText(_root, "Waiting", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 0f), new Vector2(800f, 40f), 16, TextAnchor.MiddleCenter, LaptopOs.Muted,
                    boardFrozen ? "waiting for the TV's revealed view…" : "settled slips live in Old Slips", _font);
                return;
            }

            // Identity only (playtest #17): the clock, score, and win% belong to the TV —
            // duplicating them here read as noise. The header is static per rebuild.
            LaptopUi.MakeText(_root, "LiveHeader", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -204f), new Vector2(950f, 30f), 16, TextAnchor.UpperLeft, LaptopOs.White,
                $"MY BETS   ·   TICKET {view.CurrentTicketIndex + 1}/{view.TicketCount}", _font);
            _mirrorMarket = LaptopUi.MakeText(_root, "Market", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -234f), new Vector2(950f, 22f), 11, TextAnchor.UpperLeft, LaptopOs.White,
                string.Empty, _font);
            UpdateMirrorDisplay(view);

            for (int i = 0; i < view.Tickets.Count; i++)
                BuildMirrorTicket(view.Tickets[i], new Vector2(24f + (i % 2) * 480f, -274f - (i / 2) * 142f));
        }

        private void BuildMirrorTicket(RevealedTicket ticket, Vector2 position)
        {
            RectTransform card = LaptopUi.MakePanel(_root, "MirrorTicket" + ticket.Index, new Vector2(0f, 1f),
                new Vector2(0f, 1f), position, new Vector2(456f, 128f), LaptopOs.Surface);
            string state = ticket.State == RevealedTicketState.Won ? "GREEN" : ticket.State == RevealedTicketState.Lost
                ? "DEAD" : ticket.State == RevealedTicketState.CashedOut ? "CASHED OUT" : "RIDING";
            Color stateColor = ticket.State == RevealedTicketState.Won ? LaptopOs.MoneyGood
                : ticket.State == RevealedTicketState.Lost ? LaptopOs.MoneyBad
                : ticket.State == RevealedTicketState.CashedOut ? LaptopOs.MoneyGold : LaptopOs.White;
            LaptopUi.MakeText(card, "TicketTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -10f), new Vector2(420f, 24f), 14, TextAnchor.UpperLeft, stateColor,
                $"TICKET {ticket.Index + 1}   ·   {state}   ·   {LaptopUi.Money(ticket.Stake)} → {LaptopUi.Money(ticket.PotentialPayout)}", _font);
            string legs = string.Empty;
            for (int i = 0; i < ticket.Legs.Count; i++)
            {
                if (i > 0) legs += "   ·   ";
                RevealedLeg leg = ticket.Legs[i];
                string label = string.IsNullOrEmpty(leg.MarketLabel) ? leg.TeamName : leg.MarketLabel;
                // Money colors only on money outcomes; unresolved legs wear their team color
                // (live full, pending dimmed) — the same law as the TV's slip strip.
                legs += leg.State == RevealedLegState.Won ? $"<color=#3CE873>{label} {leg.AmericanOdds} W</color>"
                    : leg.State == RevealedLegState.Lost ? $"<color=#FF4038>{label} {leg.AmericanOdds} L</color>"
                    : leg.State == RevealedLegState.Voided ? $"<color=#9EDCF6>{label} {leg.AmericanOdds} VOID</color>"
                    : leg.State == RevealedLegState.Live ? $"<color=#{leg.TeamColor:X6}>{label} {leg.AmericanOdds} LIVE</color>"
                    : $"<color=#{leg.TeamColor:X6}99>{label} {leg.AmericanOdds}</color>";
            }
            LaptopUi.MakeText(card, "Legs", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -40f), new Vector2(425f, 50f), 12, TextAnchor.UpperLeft, LaptopOs.White, legs, _font);
        }

        private void BuildRewards(Run run)
        {
            LaptopUi.MakeText(_root, "RewardsTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -128f), new Vector2(600f, 32f), 19, TextAnchor.UpperLeft, LaptopOs.Accent,
                $"REWARDS   ·   ROUND {run.Round} PAID   ·   {run.Comps.ToString("0.#", CultureInfo.InvariantCulture)} COMPS", _font);
            LaptopUi.MakeText(_root, "RewardsSub", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -158f), new Vector2(900f, 22f), 11, TextAnchor.UpperLeft, LaptopOs.Muted,
                "cheap rewards for expensive opinions", _font);
            if (run.OwnsConsumable("ask_manager"))
                LaptopUi.MakeButton(_root, "Manager", "ASK THE MANAGER — REDEAL",
                    new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -132f), new Vector2(260f, 34f), 11,
                    LaptopOs.SurfaceRaised, LaptopOs.Accent,
                    () => { _shopError = _host.director.TryPlayManager() ?? string.Empty; _invalidate(); }, _font);

            for (int i = 0; i < run.ShopOffers.Count; i++)
                BuildRewardCard(run, run.ShopOffers[i], i, false, new Vector2(24f + (i % 2) * 310f, -196f - (i / 2) * 92f));
            for (int i = 0; i < run.ConsumableOffers.Count; i++)
                BuildConsumableCard(run, run.ConsumableOffers[i], i, new Vector2(650f, -196f - i * 92f));

            if (_shopError.Length > 0)
                LaptopUi.MakeText(_root, "ShopError", new Vector2(0f, 0f), new Vector2(0f, 0f),
                    new Vector2(24f, 70f), new Vector2(940f, 24f), 12, TextAnchor.LowerLeft, LaptopOs.MoneyBad,
                    _shopError, _font);
            BuildSellbacks(run);
            LaptopUi.MakeButton(_root, "LeaveShop", "LEAVE REWARDS — NEXT ROUND", new Vector2(1f, 0f),
                new Vector2(1f, 0f), new Vector2(-24f, 66f), new Vector2(310f, 36f), 12, LaptopOs.Accent,
                LaptopOs.White, () => { _host.director.ExitShop(); _shopError = string.Empty; _invalidate(); }, _font);
        }

        private void BuildRewardCard(Run run, RelicDefinition offer, int index, bool unused, Vector2 position)
        {
            RectTransform card = LaptopUi.MakePanel(_root, "Reward" + index, new Vector2(0f, 1f), new Vector2(0f, 1f),
                position, new Vector2(292f, 82f), LaptopOs.Surface);
            LaptopUi.MakeText(card, "Name", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -7f),
                new Vector2(200f, 20f), 12, TextAnchor.UpperLeft, LaptopOs.White, offer.Name.ToUpperInvariant(), _font);
            LaptopUi.MakeText(card, "Desc", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -29f),
                new Vector2(180f, 40f), 9, TextAnchor.UpperLeft, LaptopOs.Muted, offer.Description, _font);
            bool affordable = offer.Price <= run.Comps && run.OwnedRelics.Count < run.Config.RelicSlots;
            LaptopUi.MakeButton(card, "Buy", $"BUY {offer.Price:0.#}c", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-8f, 0f), new Vector2(92f, 30f), 10, affordable ? LaptopOs.Accent : LaptopOs.SurfaceRaised,
                affordable ? LaptopOs.White : LaptopUi.Dim(LaptopOs.Muted),
                affordable ? () => { _shopError = _host.director.TryBuyRelic(index) ?? string.Empty; _invalidate(); } : null,
                _font, affordable);
        }

        private void BuildConsumableCard(Run run, ConsumableDefinition offer, int index, Vector2 position)
        {
            RectTransform card = LaptopUi.MakePanel(_root, "Consumable" + index, new Vector2(0f, 1f), new Vector2(0f, 1f),
                position, new Vector2(340f, 82f), LaptopOs.Surface);
            LaptopUi.MakeText(card, "Name", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -7f),
                new Vector2(230f, 20f), 12, TextAnchor.UpperLeft, LaptopOs.White, offer.Name.ToUpperInvariant(), _font);
            LaptopUi.MakeText(card, "Desc", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -29f),
                new Vector2(220f, 40f), 9, TextAnchor.UpperLeft, LaptopOs.Muted, offer.Description, _font);
            bool affordable = offer.Price <= run.Comps && run.OwnedConsumables.Count < run.Config.ConsumableSlots;
            LaptopUi.MakeButton(card, "Buy", $"BUY {offer.Price:0.#}c", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-8f, 0f), new Vector2(96f, 30f), 10, affordable ? LaptopOs.Accent : LaptopOs.SurfaceRaised,
                affordable ? LaptopOs.White : LaptopUi.Dim(LaptopOs.Muted),
                affordable ? () => { _shopError = _host.director.TryBuyConsumable(index) ?? string.Empty; _invalidate(); } : null,
                _font, affordable);
        }

        private void BuildSellbacks(Run run)
        {
            if (run.OwnedRelics.Count + run.OwnedConsumables.Count == 0) return;
            string text = "SELL BACK   ";
            foreach (RelicDefinition relic in run.OwnedRelics) text += $"{relic.Name} +{run.GetResaleValue(relic):0.#}c   ·   ";
            foreach (ConsumableDefinition consumable in run.OwnedConsumables)
                text += $"{consumable.Name} +{consumable.Price * run.Config.SellBackFraction:0.#}c   ·   ";
            LaptopUi.MakeText(_root, "Sellbacks", new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(24f, 40f), new Vector2(900f, 22f), 9, TextAnchor.LowerLeft, LaptopOs.Muted, text, _font);
            float x = 24f;
            for (int i = 0; i < run.OwnedRelics.Count; i++)
            {
                int index = i;
                LaptopUi.MakeButton(_root, "SellRelic" + i, "SELL " + run.OwnedRelics[i].Name,
                    new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(x, 86f), new Vector2(150f, 26f), 9,
                    LaptopOs.SurfaceRaised, LaptopOs.MoneyBad,
                    () => { _shopError = _host.director.TrySellRelic(index) ?? string.Empty; _invalidate(); }, _font);
                x += 158f;
            }
            for (int i = 0; i < run.OwnedConsumables.Count; i++)
            {
                int index = i;
                LaptopUi.MakeButton(_root, "SellConsumable" + i, "SELL " + run.OwnedConsumables[i].Name,
                    new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(x, 86f), new Vector2(150f, 26f), 9,
                    LaptopOs.SurfaceRaised, LaptopOs.MoneyBad,
                    () => { _shopError = _host.director.TrySellConsumable(index) ?? string.Empty; _invalidate(); }, _font);
                x += 158f;
            }
        }

        private void BuildTaskbar()
        {
            RectTransform taskbar = LaptopUi.MakePanel(_root, "Taskbar", new Vector2(0f, 0f), new Vector2(0f, 0f),
                Vector2.zero, new Vector2(_root.sizeDelta.x, 54f), new Color(0.025f, 0.02f, 0.05f, 0.96f));
            LaptopUi.MakeButton(taskbar, "Home", "HOME", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(18f, 0f), new Vector2(90f, 34f), 12, LaptopOs.SurfaceRaised, LaptopOs.White,
                _home, _font);
            LaptopUi.MakeText(taskbar, "AppName", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(126f, 0f), new Vector2(240f, 28f), 12, TextAnchor.MiddleLeft, LaptopOs.Muted,
                "SureThing.", _font);
            LaptopUi.MakeText(taskbar, "Clock", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-24f, 0f), new Vector2(180f, 28f), 12, TextAnchor.MiddleRight, LaptopOs.Muted,
                "03:17 AM   ·   battery low", _font);
        }

        private void MakeDot(RectTransform parent, string name, Vector2 position, Color color)
        {
            RectTransform dot = LaptopUi.MakePanel(parent, name, new Vector2(0f, 1f), new Vector2(0f, 1f),
                position, new Vector2(8f, 8f), color);
            dot.GetComponent<Image>().raycastTarget = false;
        }
    }

    /// <summary>Flavor app: only tickets already settled in the current Run.Tickets list are shown.</summary>
    internal sealed class OldSlipsApp
    {
        private readonly RectTransform _root;
        private readonly Font _font;
        private readonly Action _home;

        public OldSlipsApp(RectTransform root, Font font, Action home)
        {
            _root = root;
            _font = font;
            _home = home;
        }

        public void Render(Run run)
        {
            LaptopUi.ClearChildren(_root);
            LaptopUi.MakePanel(_root, "Backing", Vector2.zero, Vector2.zero, Vector2.zero,
                _root.sizeDelta, new Color(0.035f, 0.03f, 0.07f, 1f));
            LaptopUi.MakeText(_root, "Brand", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -38f), new Vector2(600f, 34f), 22, TextAnchor.UpperLeft, LaptopOs.White,
                "Old Slips", _font);
            LaptopUi.MakeText(_root, "Sub", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(25f, -70f), new Vector2(800f, 22f), 11, TextAnchor.UpperLeft, LaptopOs.Muted,
                "the run's settled tickets, preserved for absolutely no reason", _font);

            int row = 0;
            for (int i = 0; i < run.Tickets.Count; i++)
            {
                Ticket ticket = run.Tickets[i];
                if (ticket.State == TicketState.Open) continue;
                string payout = ticket.State == TicketState.Won ? LaptopUi.Money(ticket.PotentialPayout)
                    : ticket.State == TicketState.CashedOut ? "cash-out" : LaptopUi.Money(0);
                Color stateColor = ticket.State == TicketState.Won ? LaptopOs.MoneyGood
                    : ticket.State == TicketState.CashedOut ? LaptopOs.MoneyGold : LaptopOs.MoneyBad;
                LaptopUi.MakeText(_root, "Slip" + i, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(26f, -126f - row * 42f), new Vector2(900f, 30f), 14, TextAnchor.UpperLeft,
                    stateColor, $"#{i + 1}   {ticket.State.ToString().ToUpperInvariant()}   ·   STAKE {LaptopUi.Money(ticket.Stake)}   ·   PAYOUT {payout}", _font);
                row++;
            }
            if (row == 0)
                LaptopUi.MakeText(_root, "Empty", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 0f), new Vector2(850f, 34f), 15, TextAnchor.MiddleCenter, LaptopOs.Muted,
                    "no settled tickets in this run yet", _font);

            RectTransform taskbar = LaptopUi.MakePanel(_root, "Taskbar", new Vector2(0f, 0f), new Vector2(0f, 0f),
                Vector2.zero, new Vector2(_root.sizeDelta.x, 54f), new Color(0.025f, 0.02f, 0.05f, 0.96f));
            LaptopUi.MakeButton(taskbar, "Home", "HOME", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(18f, 0f), new Vector2(90f, 34f), 12, LaptopOs.SurfaceRaised, LaptopOs.White,
                _home, _font);
            LaptopUi.MakeText(taskbar, "Name", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(126f, 0f), new Vector2(240f, 28f), 12, TextAnchor.MiddleLeft, LaptopOs.Muted,
                "Old Slips", _font);
        }
    }
}
