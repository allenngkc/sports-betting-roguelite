using System;
using System.Globalization;
using System.Text;
using SBR.Engine;
using UnityEngine;
using UnityEngine.UI;

namespace SBR.Game
{
    /// <summary>
    /// The book, on the desk laptop (M4). A code-built world-space UGUI canvas on the lid showing
    /// exactly one page dictated by the run phase (grill decision: the app railroads, no tabs):
    /// BETSLIP during Betting (two-panel: slate rows left, working slip right, fraction-chip stakes,
    /// LOCK IT IN with a zero-ticket confirm), GO WATCH while the round is locked, SHOP during Shop,
    /// RUN OVER on RunWon/RunLost (NEW RUN is the prototype restart — a menu replaces it later).
    /// A persistent header carries ROUND / BANK / TARGET / DEBT-in-red plus the relic strip.
    ///
    /// All engine mutations go through <see cref="RunDirector"/>/<see cref="BetslipModel"/>; the page
    /// rebuilds when a state signature changes, so external flips (the TV settling the round) land
    /// without wiring events. The lid breathes its emissive when the laptop wants attention.
    /// Palette is law (design/08): green = money-good, red = money-bad, gold = payout moments.
    /// </summary>
    public sealed class LaptopScreen : MonoBehaviour
    {
        [Header("Wiring (set by GrayboxRoomBuilder)")]
        public RunDirector director;
        public Renderer lidRenderer; // the emissive lid quad behind the canvas

        [Header("Layout")]
        public Vector2 screenWorldSize = new Vector2(0.32f, 0.22f);
        [Tooltip("Metres the canvas floats in front of the lid (toward the room).")]
        public float canvasOffset = 0.004f;
        public int referencePixelsWide = 1024;

        [Header("Attention glow")]
        [ColorUsage(false, true)] public Color idleEmission = new Color(0.025f, 0.055f, 0.035f);
        [ColorUsage(false, true)] public Color attentionEmission = new Color(0.10f, 0.55f, 0.22f);
        public float attentionBreathHz = 0.6f;

        [Header("Palette (design/08)")]
        public Color screenBg = new Color(0.012f, 0.022f, 0.016f, 0.97f);
        public Color panel = new Color(0.045f, 0.075f, 0.058f, 0.95f);
        public Color panelHot = new Color(0.10f, 0.16f, 0.11f, 0.98f);
        public Color chromeCyan = new Color(0.62f, 0.86f, 0.96f, 0.95f);
        public Color textColor = new Color(0.90f, 0.95f, 0.98f, 1f);
        public Color moneyGreen = new Color(0.25f, 0.95f, 0.45f, 1f);
        public Color hotRed = new Color(0.95f, 0.25f, 0.22f, 1f);
        public Color gold = new Color(0.98f, 0.78f, 0.25f, 1f);

        // ---- state ----
        private Canvas _canvas;
        private Font _font;
        private RectTransform _header;
        private RectTransform _pageBetslip, _pageShop, _pageRunOver, _pageLocked;
        private Text _tHeaderLeft, _tHeaderRight;

        private BetslipModel _slip;
        private int _slipRunGen = -1;
        private string _sig;
        private bool _lockArmed;
        private string _shopError = "";

        private MaterialPropertyBlock _emissBlock;
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        /// <summary>Test/debug surface: the model behind the betslip page.</summary>
        public BetslipModel Slip => _slip;

        // =====================================================================================

        private void Awake()
        {
            _font = LoadFont();
            _emissBlock = new MaterialPropertyBlock();
            BuildSkeleton();
        }

        private void Update()
        {
            if (director == null || director.Run == null) return;

            if (_canvas.worldCamera == null)
                _canvas.worldCamera = Camera.main;

            EnsureSlip();

            string sig = Signature();
            if (sig != _sig)
            {
                _sig = sig;
                Rebuild();
            }

            Glow();
        }

        private void EnsureSlip()
        {
            if (_slipRunGen == director.RunGeneration && _slip != null) return;
            _slip = new BetslipModel(director.Run);
            _slipRunGen = director.RunGeneration;
            _lockArmed = false;
            _shopError = "";
        }

        private string Signature()
        {
            Run r = director.Run;
            var sb = new StringBuilder(96);
            sb.Append(director.RunGeneration).Append('|').Append(r.Phase).Append('|').Append(r.Round)
              .Append('|').Append((long)r.Bank).Append('|').Append((long)r.Debt)
              .Append('|').Append(r.Tickets.Count).Append('|').Append(r.ShopOffers.Count)
              .Append('|').Append(r.OwnedRelics.Count).Append('|').Append((long)r.PiggyBankBalance)
              .Append('|').Append(_lockArmed).Append('|').Append(_shopError.Length)
              .Append('|').Append((long)(_slip?.Stake ?? 0));
            if (_slip != null)
                foreach (Pick p in _slip.Picks)
                    sb.Append('|').Append(p.MatchupIndex).Append(p.Side == Side.Home ? 'H' : 'A');
            return sb.ToString();
        }

        // ------------------------------------------------------------------ page routing

        private void Rebuild()
        {
            Run r = director.Run;

            RebuildHeader(r);

            SetPage(_pageBetslip, r.Phase == Phase.Betting);
            SetPage(_pageLocked, r.Phase == Phase.Sweat || r.Phase == Phase.Settlement);
            SetPage(_pageShop, r.Phase == Phase.Shop);
            SetPage(_pageRunOver, r.Phase == Phase.RunWon || r.Phase == Phase.RunLost);

            if (r.Phase == Phase.Betting) RebuildBetslip(r);
            else if (r.Phase == Phase.Shop) RebuildShop(r);
            else if (r.Phase == Phase.RunWon || r.Phase == Phase.RunLost) RebuildRunOver(r);
            else RebuildLocked(r);
        }

        private static void SetPage(RectTransform page, bool active)
        {
            if (page.gameObject.activeSelf != active)
                page.gameObject.SetActive(active);
        }

        private void RebuildHeader(Run r)
        {
            string debt = r.Debt > 0 ? $"   DEBT {Money(r.Debt)} DUE {Money(r.Requirement)}" : "";
            _tHeaderLeft.text =
                $"ROUND {r.Round}/{r.Config.Rounds}   BANK {Money(r.Bank)}   TGT {Money(r.CurrentTarget)}{debt}";
            _tHeaderLeft.color = r.Debt > 0 ? hotRed : chromeCyan;

            var right = new StringBuilder();
            if (r.PiggyBankBalance > 0) right.Append($"PIGGY {Money(r.PiggyBankBalance)}   ");
            if (r.OwnedRelics.Count > 0)
            {
                var names = new StringBuilder();
                foreach (RelicDefinition d in r.OwnedRelics)
                {
                    if (names.Length > 0) names.Append(" · ");
                    names.Append(d.Name.ToUpperInvariant());
                }
                right.Append(names);
            }
            right.Append($"   {r.OwnedRelics.Count}/{r.Config.RelicSlots}");
            _tHeaderRight.text = right.ToString();
        }

        // ------------------------------------------------------------------ BETSLIP page

        private void RebuildBetslip(Run r)
        {
            ClearChildren(_pageBetslip);

            float w = _pageBetslip.rect.width;
            float h = _pageBetslip.rect.height;

            // ---- left: the slate ----
            var slate = MakePanel(_pageBetslip, "Slate", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(10f, -6f), new Vector2(596f, h - 12f), new Color(0f, 0f, 0f, 0f));

            float rowH = (h - 24f) / r.CurrentSlate.Matchups.Count;
            for (int i = 0; i < r.CurrentSlate.Matchups.Count; i++)
            {
                Matchup m = r.CurrentSlate.Matchups[i];
                var row = MakePanel(slate, $"Row{i}", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(0f, -i * rowH), new Vector2(596f, rowH - 6f), panel);

                MakeText(row, "Matchup", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                    new Vector2(10f, 10f), new Vector2(300f, 30f), 19, TextAnchor.MiddleLeft, textColor,
                    $"{TeamShort(m.Away)}  @  {TeamShort(m.Home)}");
                MakeText(row, "Records", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                    new Vector2(10f, -14f), new Vector2(300f, 24f), 14, TextAnchor.MiddleLeft, chromeCyan,
                    $"{m.Away.Record}   ·   {m.Home.Record}");

                Side? on = _slip.SideOn(m.Index);
                MakeOddsButton(row, m, Side.Away, on == Side.Away, new Vector2(316f, 0f));
                MakeOddsButton(row, m, Side.Home, on == Side.Home, new Vector2(456f, 0f));
            }

            // ---- right: the working slip ----
            var slipPanel = MakePanel(_pageBetslip, "SlipPanel", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-10f, -6f), new Vector2(392f, h - 12f), panel);

            float y = -8f;
            MakeText(slipPanel, "SlipTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(12f, y), new Vector2(370f, 26f), 17, TextAnchor.UpperLeft, chromeCyan,
                $"BETSLIP   ·   {r.Tickets.Count}/{r.Config.MaxTicketsPerRound} PLACED");
            y -= 30f;

            if (_slip.Picks.Count == 0)
            {
                MakeText(slipPanel, "Empty", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(12f, y), new Vector2(370f, 28f), 15, TextAnchor.UpperLeft,
                    Dim(textColor), "click a side to add a leg");
                y -= 30f;
            }

            foreach (Pick p in _slip.Picks)
            {
                Matchup m = r.CurrentSlate.Matchups[p.MatchupIndex];
                Team team = p.Side == Side.Home ? m.Home : m.Away;
                var legRow = MakePanel(slipPanel, $"Leg{p.MatchupIndex}", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(8f, y), new Vector2(376f, 30f), new Color(0f, 0f, 0f, 0f));
                MakeText(legRow, "Line", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                    new Vector2(4f, 0f), new Vector2(300f, 28f), 15, TextAnchor.MiddleLeft, textColor,
                    $"{TeamShort(team)}   {Odds(m.Odds(p.Side))}");
                int idx = p.MatchupIndex;
                MakeButton(legRow, "X", "✕", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                    new Vector2(-4f, 0f), new Vector2(30f, 26f), 14, panelHot, hotRed,
                    () => { _slip.Remove(idx); Disarm(); });
                y -= 32f;
            }

            y -= 4f;
            MakeText(slipPanel, "Combined", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(12f, y), new Vector2(370f, 26f), 15, TextAnchor.UpperLeft, chromeCyan,
                _slip.Picks.Count > 0
                    ? $"{_slip.Picks.Count} LEG{(_slip.Picks.Count > 1 ? "S" : "")}   ·   COMBINED {Odds(_slip.CombinedOdds)}"
                    : " ");
            y -= 30f;

            // Stake chips: fractions of bank (grill decision) + $10 nudges.
            float cx = 12f;
            MakeChip(slipPanel, "10%", cx, y, () => { _slip.SetStakeFraction(0.10); Disarm(); }); cx += 74f;
            MakeChip(slipPanel, "25%", cx, y, () => { _slip.SetStakeFraction(0.25); Disarm(); }); cx += 74f;
            MakeChip(slipPanel, "50%", cx, y, () => { _slip.SetStakeFraction(0.50); Disarm(); }); cx += 74f;
            MakeChip(slipPanel, "MAX", cx, y, () => { _slip.SetStakeFraction(1.00); Disarm(); }); cx += 74f;
            MakeChip(slipPanel, "−", cx, y, () => { _slip.Nudge(-10); Disarm(); }, 34f); cx += 38f;
            MakeChip(slipPanel, "+", cx, y, () => { _slip.Nudge(+10); Disarm(); }, 34f);
            y -= 44f;

            MakeText(slipPanel, "StakeLine", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(12f, y), new Vector2(370f, 28f), 17, TextAnchor.UpperLeft, textColor,
                $"STAKE {Money(_slip.Stake)}" +
                (_slip.Picks.Count > 0 ? $"   →   TO WIN {Money(_slip.ToWin)}" : ""));
            y -= 34f;

            string blocker = _slip.PlaceBlocker;
            MakeButton(slipPanel, "Place", blocker == null ? $"PLACE {Money(_slip.Stake)}" : blocker.ToUpperInvariant(),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, y), new Vector2(368f, 44f), 18,
                blocker == null ? panelHot : panel, blocker == null ? moneyGreen : Dim(textColor),
                () =>
                {
                    if (_slip.CanPlace) { _slip.Place(); Disarm(); }
                },
                interactable: blocker == null);
            y -= 52f;

            // Placed tickets, compact.
            for (int i = 0; i < r.Tickets.Count; i++)
            {
                Ticket t = r.Tickets[i];
                MakeText(slipPanel, $"Placed{i}", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(12f, y), new Vector2(370f, 24f), 14, TextAnchor.UpperLeft, Dim(textColor),
                    $"#{i + 1}  {t.Legs.Count} leg{(t.Legs.Count > 1 ? "s" : "")}  {Money(t.Stake)} → {Money(t.PotentialPayout)}");
                y -= 26f;
            }

            // LOCK IT IN, pinned to the panel's bottom; zero tickets arms a confirm first.
            bool zeroBets = r.Tickets.Count == 0;
            string lockLabel = !zeroBets ? "LOCK IT IN"
                : _lockArmed ? "NO BETS — TARGET STILL DUE. SURE?" : "LOCK IT IN (NO BETS)";
            MakeButton(slipPanel, "Lock", lockLabel,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(368f, 46f), 17,
                _lockArmed ? panelHot : panel, _lockArmed ? hotRed : gold,
                () =>
                {
                    if (zeroBets && !_lockArmed) { _lockArmed = true; return; }
                    _lockArmed = false;
                    director.LockRound();
                });
        }

        private void MakeOddsButton(RectTransform row, Matchup m, Side side, bool selected, Vector2 x)
        {
            string label = $"{(side == Side.Away ? "AWY" : "HOM")} {Odds(m.Odds(side))}";
            MakeButton(row, side.ToString(), label, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(x.x, 0f), new Vector2(130f, 52f), 17,
                selected ? panelHot : panel, selected ? moneyGreen : textColor,
                () => { _slip.Toggle(m.Index, side); Disarm(); });
        }

        private void MakeChip(RectTransform parent, string label, float x, float y, Action onClick, float width = 66f)
        {
            MakeButton(parent, $"Chip{label}", label, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x, y), new Vector2(width, 36f), 15, panel, chromeCyan, () => onClick());
        }

        private void Disarm() => _lockArmed = false;

        // ------------------------------------------------------------------ SHOP page

        private void RebuildShop(Run r)
        {
            ClearChildren(_pageShop);
            float h = _pageShop.rect.height;

            MakeText(_pageShop, "Title", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(12f, -6f), new Vector2(700f, 30f), 20, TextAnchor.UpperLeft, gold,
                $"SHOP — ROUND {r.Round} CLEARED, GEAR UP");

            float y = -44f;
            for (int i = 0; i < r.ShopOffers.Count; i++)
            {
                RelicDefinition o = r.ShopOffers[i];
                var card = MakePanel(_pageShop, $"Offer{i}", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(12f, y), new Vector2(1000f, 118f), panel);

                MakeText(card, "Name", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(12f, -8f), new Vector2(640f, 28f), 18, TextAnchor.UpperLeft, textColor,
                    $"{o.Name.ToUpperInvariant()}   ·   {o.Axis}");
                MakeText(card, "Desc", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(12f, -38f), new Vector2(760f, 66f), 14, TextAnchor.UpperLeft, Dim(textColor),
                    o.Description);

                bool affordable = o.Price <= r.Bank && r.OwnedRelics.Count < r.Config.RelicSlots;
                int idx = i;
                MakeButton(card, "Buy", $"BUY {Money(o.Price)}",
                    new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-12f, 0f), new Vector2(190f, 52f), 17,
                    affordable ? panelHot : panel, affordable ? moneyGreen : Dim(textColor),
                    () => { _shopError = director.TryBuyRelic(idx) ?? ""; },
                    interactable: affordable);

                y -= 126f;
            }

            if (_shopError.Length > 0)
            {
                MakeText(_pageShop, "Error", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(12f, y), new Vector2(900f, 26f), 15, TextAnchor.UpperLeft, hotRed, _shopError);
            }

            MakeButton(_pageShop, "Leave", "LEAVE SHOP — NEXT ROUND",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(420f, 48f), 18,
                panelHot, gold, () => { _shopError = ""; director.ExitShop(); });
        }

        // ------------------------------------------------------------------ RUN OVER page

        private void RebuildRunOver(Run r)
        {
            ClearChildren(_pageRunOver);

            bool won = r.Phase == Phase.RunWon;
            string verdict = won
                ? $"YOU WON — ALL {r.Config.Rounds} ROUNDS CLEARED"
                : r.Debt > 0
                    ? $"THE BOOKIE COLLECTS — ROUND {r.Round}, {Money(r.Requirement)} DUE"
                    : $"BUSTED — OUT IN ROUND {r.Round}, SHORT OF {Money(r.CurrentTarget)}";

            MakeText(_pageRunOver, "Verdict", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -60f), new Vector2(960f, 60f), 30, TextAnchor.UpperCenter,
                won ? gold : hotRed, verdict);

            MakeText(_pageRunOver, "Bank", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -140f), new Vector2(960f, 34f), 20, TextAnchor.UpperCenter, textColor,
                $"FINAL BANK {Money(r.Bank)}");
            MakeText(_pageRunOver, "Seed", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -178f), new Vector2(960f, 28f), 15, TextAnchor.UpperCenter, chromeCyan,
                $"SEED {r.Rng.RunSeed}");

            // Prototype restart (Allen, M4 grill): a proper menu replaces this button later.
            MakeButton(_pageRunOver, "NewRun", "NEW RUN",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(360f, 56f), 22,
                panelHot, moneyGreen, () => director.StartNewRun());
        }

        // ------------------------------------------------------------------ LOCKED page

        private void RebuildLocked(Run r)
        {
            ClearChildren(_pageLocked);

            MakeText(_pageLocked, "Title", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 40f), new Vector2(900f, 44f), 26, TextAnchor.MiddleCenter, chromeCyan,
                "ROUND LOCKED");
            MakeText(_pageLocked, "Sub", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -10f), new Vector2(900f, 34f), 17, TextAnchor.MiddleCenter, Dim(textColor),
                r.Tickets.Count > 0
                    ? $"{r.Tickets.Count} TICKET{(r.Tickets.Count > 1 ? "S" : "")} RIDING — THE TV HAS YOUR SWEAT. GO SIT."
                    : "SETTLING…");
        }

        // ------------------------------------------------------------------ attention glow

        private void Glow()
        {
            if (lidRenderer == null) return;

            Phase p = director.Run.Phase;
            bool wantsYou = p == Phase.Betting || p == Phase.Shop || p == Phase.RunWon || p == Phase.RunLost;
            bool engaged = DeskFocus.Active != null;

            Color e = idleEmission;
            if (wantsYou && !engaged)
            {
                float breathe = 0.5f + 0.5f * Mathf.Sin(Time.time * attentionBreathHz * 2f * Mathf.PI);
                e = Color.Lerp(idleEmission, attentionEmission, breathe);
            }
            _emissBlock.SetColor(EmissionColorId, e);
            lidRenderer.SetPropertyBlock(_emissBlock);
        }

        // ------------------------------------------------------------------ canvas skeleton

        private void BuildSkeleton()
        {
            int w = referencePixelsWide;
            int h = Mathf.RoundToInt(referencePixelsWide * screenWorldSize.y / screenWorldSize.x);

            var canvasGo = new GameObject("BookCanvas", typeof(Canvas), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            var canvasRt = _canvas.GetComponent<RectTransform>();
            canvasRt.sizeDelta = new Vector2(w, h);

            // Float toward the room; +Z INTO the lid so UGUI reads correctly (the TV's mirror lesson).
            if (lidRenderer != null)
            {
                Transform lid = lidRenderer.transform;
                Vector3 outwardNormal = -lid.forward; // Quad primitive: visible face is local -Z
                canvasGo.transform.SetPositionAndRotation(
                    lid.position + outwardNormal * canvasOffset,
                    Quaternion.LookRotation(-outwardNormal, lid.up));
            }
            canvasGo.transform.localScale = Vector3.one * (screenWorldSize.x / w);

            Transform root = canvasGo.transform;
            MakeStretchImage(root, "Backing", screenBg).raycastTarget = false;

            // Header strip.
            _header = MakePanel((RectTransform)root, "Header", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, 0f), new Vector2(w, 56f), new Color(0f, 0f, 0f, 0.35f));
            _tHeaderLeft = MakeText(_header, "HeaderLeft", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(12f, 0f), new Vector2(640f, 40f), 17, TextAnchor.MiddleLeft, chromeCyan, "");
            _tHeaderRight = MakeText(_header, "HeaderRight", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-12f, 0f), new Vector2(560f, 40f), 12, TextAnchor.MiddleRight, chromeCyan, "");

            _pageBetslip = MakePage(root, "PageBetslip", w, h);
            _pageShop = MakePage(root, "PageShop", w, h);
            _pageRunOver = MakePage(root, "PageRunOver", w, h);
            _pageLocked = MakePage(root, "PageLocked", w, h);
        }

        private RectTransform MakePage(Transform root, string name, int w, int h)
        {
            var page = MakePanel((RectTransform)root, name, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(0f, 0f), new Vector2(w, h - 56f), new Color(0f, 0f, 0f, 0f));
            page.gameObject.SetActive(false);
            return page;
        }

        // ------------------------------------------------------------------ UI helpers

        private static void ClearChildren(RectTransform rt)
        {
            for (int i = rt.childCount - 1; i >= 0; i--)
                Destroy(rt.GetChild(i).gameObject);
        }

        private Text MakeText(RectTransform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 pos,
            Vector2 size, int fontSize, TextAnchor align, Color color, string content)
        {
            var go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            if (_font != null) t.font = _font;
            t.fontSize = fontSize;
            t.alignment = align;
            t.color = color;
            t.text = content;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return t;
        }

        private static RectTransform MakePanel(RectTransform parent, string name, Vector2 anchor, Vector2 pivot,
            Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return rt;
        }

        private static Image MakeStretchImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            var rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return img;
        }

        private Button MakeButton(RectTransform parent, string name, string label, Vector2 anchor, Vector2 pivot,
            Vector2 pos, Vector2 size, int fontSize, Color bg, Color fg, Action onClick, bool interactable = true)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = bg;
            img.raycastTarget = true;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            var button = go.GetComponent<Button>();
            button.targetGraphic = img;
            button.interactable = interactable;
            var colors = button.colors;
            colors.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            button.colors = colors;
            if (onClick != null)
                button.onClick.AddListener(() => onClick());

            var t = MakeText(rt, "Label", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, size, fontSize, TextAnchor.MiddleCenter, fg, label);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            return button;
        }

        private static Color Dim(Color c) => new Color(c.r, c.g, c.b, 0.55f);

        private static string TeamShort(Team t)
        {
            int i = t.Name.LastIndexOf(' ');
            return (i >= 0 ? t.Name.Substring(i + 1) : t.Name).ToUpperInvariant();
        }

        private static string Money(double v)
        {
            long n = (long)Math.Round(v, MidpointRounding.AwayFromZero);
            return "$" + n.ToString("N0", CultureInfo.InvariantCulture);
        }

        private static string Odds(double o) => o.ToString("0.00", CultureInfo.InvariantCulture);

        private static Font LoadFont()
        {
            try { return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch
            {
                Debug.LogWarning("[LaptopScreen] built-in font not found; text will not render.");
                return null;
            }
        }
    }
}
