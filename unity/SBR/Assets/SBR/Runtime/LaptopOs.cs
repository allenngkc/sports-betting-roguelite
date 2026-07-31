using System;
using System.Globalization;
using SBR.Engine;
using UnityEngine;
using UnityEngine.UI;

namespace SBR.Game
{
    /// <summary>The small, app-switching shell around the SureThing sportsbook.</summary>
    public sealed class LaptopOs
    {
        // SureThing Direction tokens. Keep the font seam separate: a licensed TMP asset replaces
        // LegacyRuntime when Allen resolves the Bell Centennial / fallback decision.
        internal static readonly Color Ink = new Color32(0x16, 0x16, 0x0F, 255);
        internal static readonly Color Surface = new Color32(0x1C, 0x1C, 0x13, 255);
        internal static readonly Color SurfaceRaised = new Color32(0x23, 0x23, 0x19, 255);
        internal static readonly Color Rule = new Color32(0x3C, 0x3C, 0x2C, 255);
        internal static readonly Color RuleSoft = new Color32(0x2C, 0x2C, 0x20, 255);
        internal static readonly Color Muted = new Color32(0x6E, 0x6B, 0x5E, 255);
        internal static readonly Color White = new Color32(0xD9, 0xD4, 0xC5, 255);
        internal static readonly Color TonerSecondary = new Color32(0x9C, 0x98, 0x88, 255);
        internal static readonly Color Accent = new Color32(0x5E, 0x86, 0xB8, 255); // biro
        internal static readonly Color BiroDeep = new Color32(0x3F, 0x69, 0x96, 255);
        internal static readonly Color MoneyGold = new Color32(0xD9, 0xA4, 0x41, 255); // wax
        internal static readonly Color WaxLit = new Color32(0xF0, 0xC0, 0x66, 255);
        internal static readonly Color WaxDeep = new Color32(0x8A, 0x66, 0x20, 255);
        internal static readonly Color MoneyGood = new Color32(0xD9, 0xA4, 0x41, 255);
        internal static readonly Color MoneyBad = new Color32(0xB4, 0x48, 0x3A, 255); // house stamp
        internal static readonly Color SignalCyan = new Color32(0x9C, 0x98, 0x88, 255);

        private enum App { Desktop, SureThing, OldSlips, Verdict }

        private readonly RectTransform _root;
        private readonly Font _font;
        private readonly LaptopScreen _host;
        private readonly int _width;
        private readonly int _height;
        private readonly RectTransform _desktop;
        private readonly RectTransform _app;
        private readonly SportsbookApp _sportsbook;
        private readonly OldSlipsApp _oldSlips;
        private App _activeApp = App.Desktop;
        private SportsbookApp.Tab _tab = SportsbookApp.Tab.Lobby;
        private Phase _lastPhase;
        private bool _hasPhase;
        private string _signature;
        private int _lastDisplayRevision = -1;
        private string _toast;
        private float _toastUntil;

        public LaptopOs(RectTransform root, Font font, LaptopScreen host, int width, int height)
        {
            _root = root;
            _font = font;
            _host = host;
            _width = width;
            _height = height;

            _desktop = LaptopUi.MakePanel(root, "Desktop", Vector2.zero, Vector2.zero,
                Vector2.zero, new Vector2(width, height), new Color(0f, 0f, 0f, 0f));
            // The wallpaper Graphic lives on its OWN child: MakePanel's GameObject already
            // carries an Image, and Unity allows one Graphic per object — AddComponent on
            // the panel returns null (the room-boot NRE this comment buries).
            var wallGo = new GameObject("Wallpaper", typeof(LaptopWallpaperGraphic));
            wallGo.transform.SetParent(_desktop, false);
            var wallpaper = wallGo.GetComponent<LaptopWallpaperGraphic>();
            wallpaper.raycastTarget = false;
            RectTransform wallRt = wallpaper.rectTransform;
            wallRt.anchorMin = Vector2.zero;
            wallRt.anchorMax = Vector2.one;
            wallRt.offsetMin = Vector2.zero;
            wallRt.offsetMax = Vector2.zero;
            wallGo.transform.SetAsFirstSibling();
            _app = LaptopUi.MakePanel(root, "App", Vector2.zero, Vector2.zero,
                Vector2.zero, new Vector2(width, height), Ink);
            _app.gameObject.SetActive(false);

            _sportsbook = new SportsbookApp(_app, _font, _host, Invalidate, SelectTab, OpenHome, OpenLedger);
            _oldSlips = new OldSlipsApp(_app, _font, OpenHome, OpenSportsbook);
            BuildDesktop();
        }

        public void Tick(Run run, BetslipModel slip)
        {
            if (_toast != null && Time.unscaledTime > _toastUntil)
            {
                _toast = null;
                _signature = null;
            }
            if (!_hasPhase || run.Phase != _lastPhase)
            {
                _lastPhase = run.Phase;
                _hasPhase = true;
                ApplyPhaseDefault(run.Phase);
                Invalidate();
            }

            // Fast display values (clock/prob/score/suspension) refresh IN PLACE — only
            // structural revisions rebuild the canvas (Sol, F_0.3.0 performance finding).
            RevealedView view = _host.tv != null ? _host.tv.RevealedView : null;
            if (view != null && view.DisplayRevision != _lastDisplayRevision
                && _activeApp == App.SureThing && _tab == SportsbookApp.Tab.MyBets)
            {
                _lastDisplayRevision = view.DisplayRevision;
                _sportsbook.UpdateMirrorDisplay(view);
            }

            string viewRevision = view != null
                ? view.Revision.ToString(CultureInfo.InvariantCulture) : "-";
            string signature = string.Concat(
                _host.director.RunGeneration, "|", run.Phase, "|", run.Round, "|",
                ((long)run.Bank).ToString(CultureInfo.InvariantCulture), "|", run.Tickets.Count, "|",
                run.ShopOffers.Count, "|", run.ConsumableOffers.Count, "|", run.OwnedRelics.Count, "|",
                run.OwnedConsumables.Count, "|", slip.Picks.Count, "|", ((long)slip.Stake).ToString(CultureInfo.InvariantCulture), "|",
                (int)slip.Modifier, "|", slip.BoostLeg, "|", _activeApp, "|", _tab, "|", viewRevision,
                "|", _toast);
            if (signature == _signature) return;
            _signature = signature;
            Rebuild(run, slip);
        }

        /// <summary>Test/debug surface — the same transitions the desktop icons and tabs
        /// drive, exposed so PlayMode can walk the OS without simulating cursor clicks.</summary>
        public bool OnDesktop => _activeApp == App.Desktop;
        public SportsbookApp.Tab CurrentTab => _tab;

        public void OpenSportsbook(SportsbookApp.Tab tab)
        {
            _activeApp = App.SureThing;
            _tab = tab;
            Invalidate();
        }

        public void OpenDesktop()
        {
            _activeApp = App.Desktop;
            Invalidate();
        }

        private void OpenLedger()
        {
            _activeApp = App.OldSlips;
            Invalidate();
        }

        public void ResetForRun()
        {
            _activeApp = App.Desktop;
            _tab = SportsbookApp.Tab.Lobby;
            _hasPhase = false;
            _signature = null;
            _toast = null;
        }

        private void ApplyPhaseDefault(Phase phase)
        {
            switch (phase)
            {
                case Phase.Betting:
                    _activeApp = App.SureThing;
                    _tab = SportsbookApp.Tab.Lobby;
                    break;
                case Phase.Sweat:
                    _activeApp = App.SureThing;
                    _tab = SportsbookApp.Tab.MyBets;
                    break;
                case Phase.Shop:
                    _activeApp = App.SureThing;
                    _tab = SportsbookApp.Tab.Rewards;
                    ShowToast("REWARDS IS OPEN — spend your comps before the next payment.");
                    break;
                case Phase.RunWon:
                case Phase.RunLost:
                    _activeApp = App.Verdict;
                    break;
            }
        }

        private void Rebuild(Run run, BetslipModel slip)
        {
            _desktop.gameObject.SetActive(_activeApp == App.Desktop);
            _app.gameObject.SetActive(_activeApp != App.Desktop);
            if (_activeApp == App.Desktop) return;

            if (_activeApp == App.SureThing)
                _sportsbook.Render(run, slip, _tab, run.Phase == Phase.Sweat);
            else if (_activeApp == App.OldSlips)
                _oldSlips.Render(run);
            else
                RenderVerdict(run);

            if (_toast != null)
            {
                LaptopUi.MakeText(_app, "Toast", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(0f, 62f), new Vector2(760f, 24f), 11, TextAnchor.LowerCenter,
                    Accent, _toast, _font);
            }
        }

        private void BuildDesktop()
        {
            LaptopUi.MakeText(_desktop, "DesktopSure", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(28f, -28f), new Vector2(76f, 36f), 23, TextAnchor.UpperLeft, White,
                "SURE", _font);
            LaptopUi.MakeText(_desktop, "DesktopThing", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(105f, -28f), new Vector2(130f, 36f), 23, TextAnchor.UpperLeft, Accent,
                "THING.", _font);
            LaptopUi.MakeText(_desktop, "DesktopTagline", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(30f, -62f), new Vector2(420f, 24f), 12, TextAnchor.UpperLeft, Muted,
                "the number never lies", _font);

            MakeDesktopIcon("SureThing", "S", "Sportsbook", new Vector2(34f, -120f), Accent,
                () => { _activeApp = App.SureThing; Invalidate(); });
            MakeDesktopIcon("OldSlips", "$", "Old Slips", new Vector2(34f, -225f), SurfaceRaised,
                () => { _activeApp = App.OldSlips; Invalidate(); });
            MakeDesktopIcon("Mail", "@", "Mail (soon)", new Vector2(34f, -330f), Muted, null);
            MakeDesktopIcon("Bank", "¤", "Bank (soon)", new Vector2(34f, -435f), Muted, null);

            RectTransform taskbar = LaptopUi.MakePanel(_desktop, "Taskbar", new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(0f, 0f), new Vector2(_width, 54f), new Color(0.025f, 0.02f, 0.05f, 0.94f));
            LaptopUi.MakeButton(taskbar, "Home", "HOME", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(18f, 0f), new Vector2(90f, 34f), 12, SurfaceRaised, White, null, _font);
            Text taskbarText = LaptopUi.MakeText(taskbar, "TaskbarText", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 0f), new Vector2(320f, 30f), 12, TextAnchor.MiddleCenter, Muted,
                "SURETHING.   ·   old slips", _font);
            // See SportsbookApp.BuildSlip's LockReason for why: MiddleCenter + the Wrap default is a
            // real Unity legacy-Text bug (glyphs bake as slivers), and MakeButton's own centered
            // labels avoid it only because they override to Overflow. Every standalone MiddleCenter
            // MakeText call does the same here.
            taskbarText.horizontalOverflow = HorizontalWrapMode.Overflow;
            LaptopUi.MakeText(taskbar, "Clock", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-24f, 0f), new Vector2(180f, 30f), 12, TextAnchor.MiddleRight, Muted,
                "03:17 AM   ·   12%", _font);
        }

        private void MakeDesktopIcon(string name, string glyph, string label, Vector2 position, Color color,
            Action onClick)
        {
            Button button = LaptopUi.MakeButton(_desktop, name, glyph, new Vector2(0f, 1f), new Vector2(0f, 1f),
                position, new Vector2(86f, 76f), 28, new Color(0f, 0f, 0f, 0.12f), color, onClick, _font,
                onClick != null);
            LaptopUi.MakeText(button.GetComponent<RectTransform>(), "Label", new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, -25f), new Vector2(150f, 22f), 11,
                TextAnchor.UpperCenter, onClick == null ? Muted : White, label, _font);
        }

        private void RenderVerdict(Run run)
        {
            LaptopUi.ClearChildren(_app);
            bool won = run.Phase == Phase.RunWon;
            LaptopUi.MakePanel(_app, "VerdictBg", Vector2.zero, Vector2.zero, Vector2.zero,
                new Vector2(_width, _height), new Color(0.03f, 0.02f, 0.06f, 1f));
            LaptopUi.MakeText(_app, "VerdictBrand", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -54f), new Vector2(800f, 36f), 22, TextAnchor.UpperCenter, White,
                "SureThing.", _font);
            Text verdict = LaptopUi.MakeText(_app, "Verdict", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 70f), new Vector2(900f, 60f), 30, TextAnchor.MiddleCenter,
                won ? MoneyGold : MoneyBad,
                won ? "THE HOUSE BLINKS FIRST" : "THE BOOKIE COLLECTS", _font);
            verdict.horizontalOverflow = HorizontalWrapMode.Overflow;
            Text final = LaptopUi.MakeText(_app, "Final", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 14f), new Vector2(900f, 36f), 18, TextAnchor.MiddleCenter, White,
                $"FINAL BANK {LaptopUi.Money(run.Bank)}   ·   SEED {run.Rng.RunSeed}", _font);
            final.horizontalOverflow = HorizontalWrapMode.Overflow;
            LaptopUi.MakeButton(_app, "NewRun", "NEW RUN", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 58f), new Vector2(300f, 52f), 19, Accent, White,
                () => { _host.director.StartNewRun(); Invalidate(); }, _font);
        }

        private void ShowToast(string toast)
        {
            _toast = toast;
            _toastUntil = Time.unscaledTime + 4f;
        }

        private void Invalidate()
        {
            _signature = null;
            if (_toast != null && Time.unscaledTime > _toastUntil) _toast = null;
        }

        internal void OpenHome()
        {
            _activeApp = App.Desktop;
            Invalidate();
        }

        internal void OpenSportsbook()
        {
            _activeApp = App.SureThing;
            if (_lastPhase == Phase.Shop) _tab = SportsbookApp.Tab.Rewards;
            else if (_lastPhase == Phase.Sweat) _tab = SportsbookApp.Tab.MyBets;
            else _tab = SportsbookApp.Tab.Lobby;
            Invalidate();
        }

        internal void OpenOldSlips()
        {
            _activeApp = App.OldSlips;
            Invalidate();
        }

        internal void SelectTab(SportsbookApp.Tab tab)
        {
            _activeApp = App.SureThing;
            _tab = tab;
            Invalidate();
        }
    }

    /// <summary>Code-built, texture-free wallpaper: four corner colors interpolate across the canvas.</summary>
    internal sealed class LaptopWallpaperGraphic : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            Color topLeft = LaptopOs.SurfaceRaised;
            Color topRight = LaptopOs.Ink;
            Color bottomLeft = LaptopOs.Ink;
            Color bottomRight = LaptopOs.Surface;
            vh.AddVert(new Vector3(r.xMin, r.yMin), (Color32)bottomLeft, Vector2.zero);
            vh.AddVert(new Vector3(r.xMin, r.yMax), (Color32)topLeft, Vector2.up);
            vh.AddVert(new Vector3(r.xMax, r.yMax), (Color32)topRight, Vector2.one);
            vh.AddVert(new Vector3(r.xMax, r.yMin), (Color32)bottomRight, Vector2.right);
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }
    }

    internal static class LaptopUi
    {
        public static void ClearChildren(RectTransform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(root.GetChild(i).gameObject);
        }

        public static Text MakeText(RectTransform parent, string name, Vector2 anchor, Vector2 pivot,
            Vector2 position, Vector2 size, int fontSize, TextAnchor align, Color color, string content, Font font)
        {
            var go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            if (font != null) text.font = font;
            text.fontSize = Mathf.Max(13, fontSize);
            text.alignment = align;
            text.color = color;
            text.text = content;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            RectTransform rt = text.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = position;
            if (name == "LockReason")
            {
                var sb = new System.Text.StringBuilder();
                sb.Append($"[DIAG2:{name}] text='{text.text}' font={(text.font != null ? text.font.name : "null")} " +
                    $"dynamic={text.font?.dynamic} actualSize={text.fontSize} " +
                    $"fontNamesArray=[{(text.font != null ? string.Join(",", text.font.fontNames) : "")}]");
                if (text.font != null)
                {
                    foreach (char c in "PLACEORWSKI ")
                    {
                        bool ok = text.font.GetCharacterInfo(c, out CharacterInfo info, text.fontSize, FontStyle.Normal);
                        sb.Append($"\n  '{c}' found={ok} advance={info.advance} glyphW={info.glyphWidth} glyphH={info.glyphHeight} " +
                            $"minX={info.minX} maxX={info.maxX} minY={info.minY} maxY={info.maxY} " +
                            $"uvBL={info.uvBottomLeft} uvBR={info.uvBottomRight} uvTL={info.uvTopLeft} uvTR={info.uvTopRight}");
                    }
                }
                Debug.Log(sb.ToString());
            }
            return text;
        }

        /// <summary>Measures a string's natural (unwrapped) width in a dynamic font at a given size,
        /// requesting the glyphs into the font's atlas first so a cold cache never reports zero.</summary>
        public static float MeasureWidth(Font font, string text, int fontSize)
        {
            if (font == null || string.IsNullOrEmpty(text)) return 0f;
            int size = Mathf.Max(13, fontSize);
            font.RequestCharactersInTexture(text, size, FontStyle.Normal);
            float width = 0f;
            for (int i = 0; i < text.Length; i++)
            {
                if (font.GetCharacterInfo(text[i], out CharacterInfo info, size, FontStyle.Normal))
                    width += info.advance;
            }
            return width;
        }

        /// <summary>Shortens content to fit maxWidth, trailing with an ellipsis rather than ever
        /// silently cutting mid-word. A no-op when the string already fits.</summary>
        public static string FitText(Font font, string content, int fontSize, float maxWidth)
        {
            if (font == null || string.IsNullOrEmpty(content)) return content;
            if (MeasureWidth(font, content, fontSize) <= maxWidth) return content;
            const string ellipsis = "…";
            float budget = maxWidth - MeasureWidth(font, ellipsis, fontSize);
            if (budget <= 0f) return ellipsis;
            int lo = 0, hi = content.Length;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (MeasureWidth(font, content.Substring(0, mid), fontSize) <= budget) lo = mid;
                else hi = mid - 1;
            }
            string trimmed = content.Substring(0, lo).TrimEnd();
            return trimmed.Length > 0 ? trimmed + ellipsis : ellipsis;
        }

        /// <summary>Fits a variable-length label between a fixed prefix and suffix (e.g. "1. " and
        /// the price) so the parts that always carry meaning — the index, the price — never get
        /// truncated; only the label in between ever loses characters, and only behind an ellipsis.</summary>
        public static string FitLabelKeepingSuffix(Font font, string prefix, string label, string suffix,
            int fontSize, float maxWidth)
        {
            if (font == null) return prefix + label + suffix;
            float reserved = MeasureWidth(font, prefix, fontSize) + MeasureWidth(font, suffix, fontSize);
            string fitLabel = FitText(font, label, fontSize, Mathf.Max(0f, maxWidth - reserved));
            return prefix + fitLabel + suffix;
        }

        public static RectTransform MakePanel(RectTransform parent, string name, Vector2 anchor, Vector2 pivot,
            Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = position;
            return rt;
        }

        public static Image MakeStretchImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return image;
        }

        public static Button MakeButton(RectTransform parent, string name, string label, Vector2 anchor, Vector2 pivot,
            Vector2 position, Vector2 size, int fontSize, Color background, Color foreground, Action onClick,
            Font font, bool interactable = true)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = background;
            image.raycastTarget = interactable;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = new Vector2(Mathf.Max(44f, size.x), Mathf.Max(32f, size.y));
            rt.anchoredPosition = position;
            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.interactable = interactable;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.fadeDuration = 0.12f;
            button.colors = colors;
            if (onClick != null) button.onClick.AddListener(() => onClick());
            Text text = MakeText(rt, "Label", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, rt.sizeDelta, fontSize, TextAnchor.MiddleCenter, foreground, label, font);
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            return button;
        }

        public static Image MakeSprite(RectTransform parent, string name, Sprite sprite, Vector2 anchor,
            Vector2 pivot, Vector2 position, Vector2 size, Color tint)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.color = tint;
            image.raycastTarget = false;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return image;
        }

        public static RectTransform MakeRule(RectTransform parent, string name, Vector2 anchor,
            Vector2 pivot, Vector2 position, Vector2 size)
            => MakePanel(parent, name, anchor, pivot, position, size, LaptopOs.RuleSoft);

        public static Color Dim(Color color) => new Color(color.r, color.g, color.b, 0.55f);

        public static string TeamShort(Team team)
        {
            int split = team.Name.LastIndexOf(' ');
            return (split >= 0 ? team.Name.Substring(split + 1) : team.Name).ToUpperInvariant();
        }

        public static string Money(double value)
        {
            long rounded = (long)Math.Round(value, MidpointRounding.AwayFromZero);
            return "$" + rounded.ToString("N0", CultureInfo.InvariantCulture);
        }

        public static Color FromRgb(uint rgb)
        {
            return new Color(((rgb >> 16) & 0xff) / 255f, ((rgb >> 8) & 0xff) / 255f,
                (rgb & 0xff) / 255f, 1f);
        }
    }
}
