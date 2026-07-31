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
        // Punched-out type on a solid wax field (PLACE TICKET's label). Distinct from Ink: Ink is the
        // general document ground, WaxInk is specifically the colour type takes when it sits ON wax.
        internal static readonly Color WaxInk = new Color32(0x1A, 0x13, 0x05, 255);

        // Document-layer tokens with no Color shape (opacity/rotation/height), kept here alongside the
        // palette so every SureThing constant traces back to one place.
        internal const float MarkedWashAlpha = 0.07f; // --marked-wash: rgba(94,134,184,.07) == Accent at this alpha
        internal const float WaxHighlightOpacity = 0.26f; // --wax-highlight-opacity
        internal const float WaxHighlightRotateDeg = -0.5f; // --wax-highlight-rotate
        internal const float WaxHighlightHeight = 6f; // --wax-highlight-h, px
        internal const float TonerGrainOpacity = 0.05f; // --toner-grain-opacity

        private enum App { Desktop, SureThing, OldSlips, Verdict }

        private readonly RectTransform _root;
        private readonly Font _font;
        private readonly Font _fontCond;
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

        public LaptopOs(RectTransform root, Font font, Font fontCond, LaptopScreen host, int width, int height)
        {
            _root = root;
            _font = font;
            _fontCond = fontCond;
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

            _sportsbook = new SportsbookApp(_app, _font, _fontCond, _host, Invalidate, SelectTab, OpenHome, OpenLedger);
            _oldSlips = new OldSlipsApp(_app, _font, _fontCond, OpenHome, OpenSportsbook);
            BuildDesktop();

            // The document's own toner grain (palette-surething.css --toner-grain-opacity), built
            // once here and parented directly to the top-level canvas root rather than to Desktop or
            // App. Render()/ClearChildren only ever touch _app's children, so this survives every
            // rebuild untouched — zero per-rebuild cost — and being the last sibling under _root, it
            // sits above whichever of Desktop/App is currently active, matching the reference kit
            // (app.jsx z-index:9 over the whole 1024x704 sheet).
            // DISABLED — the implementation is wrong, not the token. Measured off the captures with
            // grain on, the ground went from (24,24,16) to (52,52,48): more than double the
            // luminance, and neutral grey where the ground is warm olive (#16160F has R=G above B).
            //
            // The cause is structural rather than a value to tune. MakeTonerGrain lays pure white
            // texels at a mean alpha near 0.5, tinted by a 0.05 Image alpha, over the whole sheet.
            // Under normal alpha blending a white overlay can only ever lighten, so it lifts and
            // desaturates the ground instead of texturing it. Lowering the opacity would only make
            // a fainter version of the same wrong thing — real grain has to darken as well as
            // lighten, which needs an overlay/soft-light blend and therefore a custom UI shader.
            //
            // Kept rather than deleted: the tile generation, the once-per-laptop placement outside
            // _app, and the zero-per-rebuild cost are all correct and worth reusing when the shader
            // exists. The other three document-layer elements are unaffected and stay on.
            // LaptopUi.MakeTonerGrain(_root);
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
            MakeDesktopIcon("OldSlips", "$", "LEDGER", new Vector2(34f, -225f), SurfaceRaised,
                () => { _activeApp = App.OldSlips; Invalidate(); });
            MakeDesktopIcon("Mail", "@", "Mail (soon)", new Vector2(34f, -330f), Muted, null);
            MakeDesktopIcon("Bank", "¤", "Bank (soon)", new Vector2(34f, -435f), Muted, null);

            // Was rgba(0.025, 0.02, 0.05, 0.94): effectively black, and blue-tinted. That broke two
            // laws at once — nothing on this screen may be pure black, and the room physically
            // cannot return a saturated cool colour, so a cool-cast bar reads as composited into
            // the scene rather than photographed in it. Uses the same lifted warm ground as the
            // in-app tray now; the desktop is the same machine.
            RectTransform taskbar = LaptopUi.MakePanel(_desktop, "Taskbar", new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(0f, 0f), new Vector2(_width, 54f), SurfaceRaised);
            LaptopUi.MakeButton(taskbar, "Home", "HOME", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(18f, 0f), new Vector2(90f, 34f), 12, SurfaceRaised, White, null, _font);
            Text taskbarText = LaptopUi.MakeText(taskbar, "TaskbarText", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 0f), new Vector2(320f, 30f), 12, TextAnchor.MiddleCenter, Muted,
                "SURETHING.   ·   LEDGER", _font);
            // Overflow rather than the Wrap default because this label is a single line that must
            // not re-flow. (An earlier comment here blamed a Unity legacy-Text bug for the
            // "renders only a couple of glyphs" defect; that diagnosis was wrong — the real cause
            // was one control being drawn on top of another. See SportsbookApp.BuildSlip.)
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

    /// <summary>Code-built wash behind a marked form entry: a left-to-right fade from a flat colour
    /// to fully transparent by 70% of the element's width, matching the reference kit's
    /// <c>linear-gradient(90deg, var(--marked-wash), transparent 70%)</c>. Same per-vertex-gradient
    /// technique as <see cref="LaptopWallpaperGraphic"/> — four vertices, no texture — because the
    /// rest of the fade (70%-100%) is already fully transparent and costs nothing left undrawn.</summary>
    internal sealed class MarkedWashGraphic : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            Color32 tint = color;
            Color32 clear = new Color32(tint.r, tint.g, tint.b, 0);
            float stopX = r.xMin + r.width * 0.7f;
            vh.AddVert(new Vector3(r.xMin, r.yMin), tint, Vector2.zero);
            vh.AddVert(new Vector3(r.xMin, r.yMax), tint, Vector2.up);
            vh.AddVert(new Vector3(stopX, r.yMax), clear, Vector2.one);
            vh.AddVert(new Vector3(stopX, r.yMin), clear, Vector2.right);
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

            // Truncate clips whole LINES, so a box shorter than one line of its own font renders
            // NOTHING — not a clipped glyph, nothing at all. Silent and total, with every test still
            // green because the Text object exists and holds the right string.
            //
            // Wiring the production faces cost three display elements exactly this way in one go:
            // Archivo's line metrics are taller than the built-in fallback's, so boxes authored at
            // 1.08x and 1.16x their font size stopped fitting a single line, and the masthead and
            // the payout figure simply vanished.
            //
            // A box may legitimately be shorter than its content — clipping a long wrapped paragraph
            // is a real layout choice. It is never useful for a box to be too short for its FIRST
            // line, so that case falls back to Overflow rather than rendering emptiness. Callers keep
            // their authored height and position; only the failure mode changes. Evaluated after
            // sizeDelta because preferredHeight depends on the wrap width.
            if (text.preferredHeight > size.y)
                text.verticalOverflow = VerticalWrapMode.Overflow;
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

        /// <summary>The marked-form-entry wash (palette-surething.css --marked-wash), stretched to
        /// fill <paramref name="parent"/> exactly — sized this way (rather than a hand-picked rect)
        /// so it is trivially contained within whatever row it marks. Caller is responsible for only
        /// adding this when the row is actually selected, and for adding it before any sibling text/
        /// buttons so it draws underneath them.</summary>
        public static void MakeMarkedWash(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(MarkedWashGraphic));
            go.transform.SetParent(parent, false);
            MarkedWashGraphic wash = go.GetComponent<MarkedWashGraphic>();
            wash.color = new Color(LaptopOs.Accent.r, LaptopOs.Accent.g, LaptopOs.Accent.b,
                LaptopOs.MarkedWashAlpha);
            wash.raycastTarget = false;
            RectTransform rt = wash.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>Builds the document's own toner grain (palette-surething.css
        /// --toner-grain-opacity) exactly once and stretches it to fill <paramref name="root"/>.
        /// Cost: one small (128x128) runtime RGBA32 texture and one Image, built a single time per
        /// laptop — never regenerated, never touched by a rebuild. This is a deliberate exception to
        /// this file's usual texture-free approach (see <see cref="LaptopWallpaperGraphic"/>): true
        /// per-pixel grain has no per-vertex-gradient equivalent, so a baked noise texture is the only
        /// way to get it in UGUI. The reference kit's SVG feTurbulence filter is not reproduced —
        /// this is a flat static noise tile, an approximation of it, not a match.</summary>
        public static void MakeTonerGrain(RectTransform root)
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "SureThingTonerGrain",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            var rng = new System.Random(0xC0FFEE);
            var pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                // Alpha-only noise on a flat white texel: tinted by the Image's own low overall alpha
                // below, so this reads as faint toner static rather than a visible checker pattern.
                byte a = (byte)rng.Next(40, 216);
                pixels[i] = new Color32(255, 255, 255, a);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f), size);

            var go = new GameObject("TonerGrain", typeof(Image));
            go.transform.SetParent(root, false);
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Tiled;
            image.color = new Color(1f, 1f, 1f, LaptopOs.TonerGrainOpacity);
            // Full-bleed and on top of everything: without this it silently eats every click on the
            // laptop screen.
            image.raycastTarget = false;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

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

    /// <summary>
    /// The machine's own chrome — the rail across the top and the tray across the bottom. It
    /// belongs to the notebook, not to whichever app is running, so it is defined once here and
    /// every screen calls into it.
    ///
    /// It used to be copy-pasted into each screen's builder, which let the two copies drift: the
    /// sportsbook drew the rail at 12px and the ledger drew the same rail at 13px, so the machine's
    /// own furniture changed size when you switched app. The same roles also carried different
    /// object names in each copy ("AppName" vs "Messages", "Clock" vs "SystemFacts"), which made
    /// the duplication invisible to a name-based test.
    ///
    /// Chrome text is 12px by deliberate exception: the type floor is 13px for anything stating a
    /// product fact, and 12px is allowed only for OS furniture carrying no product meaning. Every
    /// string here — machine mark, sticker, clock, unread count, disk and update state — is set
    /// dressing. Nothing a player needs to make a decision may be added at this size.
    /// </summary>
    internal static class NotebookChrome
    {
        public const float RailHeight = 34f;
        public const float TrayHeight = 34f;

        /// OS-furniture size. See the class note before reusing it for anything else.
        private const int ChromeText = 12;

        public const string MachineMark = "■  NOTEBOOK";
        public const string StickerText = "PROPERTY OF NOBODY";

        /// Fixed fiction, not a live clock: the shared spec pins the machine at 02:47 so every
        /// capture and every direction concept is comparable. The trailing mark is the battery.
        public const string ClockText = "02:47   ▰";

        private const string MessagesText = "MESSAGES  1";
        private const string SystemFactsText = "DISK 61% FULL    NO UPDATES";

        public enum Running { Sportsbook, Ledger }

        public static RectTransform BuildRail(RectTransform parent, float width, Font font)
        {
            RectTransform rail = LaptopUi.MakePanel(parent, "NotebookRail", new Vector2(0f, 1f),
                new Vector2(0f, 1f), Vector2.zero, new Vector2(width, RailHeight),
                LaptopOs.SurfaceRaised);
            LaptopUi.MakeText(rail, "Machine", new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(14f, 0f), new Vector2(200f, 24f), ChromeText, TextAnchor.MiddleLeft,
                LaptopOs.White, MachineMark, font);
            LaptopUi.MakeText(rail, "Sticker", new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(150f, 0f), new Vector2(200f, 24f), ChromeText, TextAnchor.MiddleLeft,
                LaptopOs.Accent, StickerText, font);
            LaptopUi.MakeText(rail, "Clock", new Vector2(1f, .5f), new Vector2(1f, .5f),
                new Vector2(-14f, 0f), new Vector2(140f, 24f), ChromeText, TextAnchor.MiddleRight,
                LaptopOs.Muted, ClockText, font);
            return rail;
        }

        /// <param name="minimize">
        /// Invoked by the slot of the app that is already running. A tray slot for the running app
        /// cannot "launch" it, so it drops to the desktop instead — the same thing a real taskbar
        /// button does. This is why the running slot stays clickable rather than being disabled.
        /// </param>
        public static RectTransform BuildTray(RectTransform parent, float width, Font font,
            Running running, Action openSportsbook, Action openLedger, Action minimize)
        {
            RectTransform tray = LaptopUi.MakePanel(parent, "NotebookTray", new Vector2(0f, 0f),
                new Vector2(0f, 0f), Vector2.zero, new Vector2(width, TrayHeight),
                LaptopOs.SurfaceRaised);

            bool sportsbookRunning = running == Running.Sportsbook;
            MakeSlot(tray, "SureThing", "SURETHING", 12f, 110f, sportsbookRunning,
                sportsbookRunning ? minimize : openSportsbook, font);
            MakeSlot(tray, "Ledger", "LEDGER", 132f, 88f, !sportsbookRunning,
                sportsbookRunning ? openLedger : minimize, font);

            LaptopUi.MakeText(tray, "Messages", new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(232f, 0f), new Vector2(210f, 24f), ChromeText, TextAnchor.MiddleLeft,
                LaptopOs.Muted, MessagesText, font);
            LaptopUi.MakeText(tray, "SystemFacts", new Vector2(1f, .5f), new Vector2(1f, .5f),
                new Vector2(-14f, 0f), new Vector2(270f, 24f), ChromeText, TextAnchor.MiddleRight,
                LaptopOs.Muted, SystemFactsText, font);
            return tray;
        }

        /// The running app reads as pressed-in — ink ground, full-strength label. A backgrounded
        /// app reads as raised and muted. That is the only state difference, and it is carried by
        /// ground and weight rather than colour alone.
        private static void MakeSlot(RectTransform tray, string name, string label, float x,
            float width, bool running, Action onClick, Font font)
        {
            LaptopUi.MakeButton(tray, name, label, new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(x, 0f), new Vector2(width, 32f), ChromeText,
                running ? LaptopOs.Ink : LaptopOs.SurfaceRaised,
                running ? LaptopOs.White : LaptopOs.Muted,
                onClick, font);
        }
    }
}
