using System;
using SBR.Engine;
using UnityEngine;
using UnityEngine.UI;

namespace SBR.Game
{
    /// <summary>
    /// The laptop host. DeskFocus owns the zoom contract; this component owns the world-space canvas and
    /// the model seam, while <see cref="LaptopOs"/> owns the presentation pages. Engine verbs still go
    /// through BetslipModel and RunDirector, so the OS is a reskin of the old betting flow rather than a
    /// second game loop.
    /// </summary>
    public sealed class LaptopScreen : MonoBehaviour
    {
        [Header("Wiring (set by GrayboxRoomBuilder)")]
        public RunDirector director;
        public TvSweatScreen tv;
        public Renderer lidRenderer;

        [Header("Layout")]
        public Vector2 screenWorldSize = new Vector2(0.32f, 0.22f);
        [Tooltip("Metres the canvas floats in front of the lid (toward the room).")]
        public float canvasOffset = 0.004f;
        public int referencePixelsWide = 1024;

        [Header("Attention glow")]
        [ColorUsage(false, true)] public Color idleEmission = new Color(0.025f, 0.035f, 0.055f);
        [ColorUsage(false, true)] public Color attentionEmission = new Color(0.28f, 0.10f, 0.55f);
        public float attentionBreathHz = 0.6f;

        private Canvas _canvas;
        private Font _font;
        // Condensed voice seam (DESIGN.md / tokens/fonts.css --font-cond, Archivo Narrow).
        //
        // This comment used to say Archivo Narrow was not in the repo yet and that _fontCond
        // resolved to the same fallback object as _font. That stopped being true when S11 landed
        // both faces: ArchivoNarrow.ttf is in Resources/SureThing/Fonts, LoadFont resolves it with
        // no fallback warning, and the two voices genuinely render differently — measured off
        // frames 11 and 01, the same string "SURETHING" at the same rendered size spans 64px as a
        // condensed desktop caption against 78px as a roman tray label, a ratio of 0.82. Left
        // corrected rather than deleted because C15 plans the TMP migration off this seam and was
        // scoped against the old claim.
        private Font _fontCond;
        private BetslipModel _slip;
        private int _slipRunGen = -1;
        private LaptopOs _os;

        /// <summary>Test/debug surface: the OS shell (PlayMode drives apps/tabs through it).</summary>
        public LaptopOs Os => _os;
        private MaterialPropertyBlock _emissBlock;
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        /// <summary>Test/debug surface: the model behind the sportsbook slip.</summary>
        public BetslipModel Slip => _slip;

        private void Awake()
        {
            // Two-voice type seam, now carrying the ruled production faces (S11, OFL 1.1):
            // --font-data is Archivo (roman — labels, copy, OS chrome), --font-cond is Archivo
            // Narrow (condensed — figures, prices, names, terminal-state words). One superfamily,
            // so this is two voices of one hand rather than a pairing. Licences ship beside the
            // fonts in Resources/SureThing/Fonts and must stay with them.
            _font = LoadFont("SureThing/Fonts/Archivo");
            _fontCond = LoadFont("SureThing/Fonts/ArchivoNarrow");
            _emissBlock = new MaterialPropertyBlock();
            if (tv == null) tv = FindAnyObjectByType<TvSweatScreen>();
            BuildSkeleton();
        }

        private void Update()
        {
            if (director == null || director.Run == null) return;
            if (_canvas.worldCamera == null) _canvas.worldCamera = Camera.main;

            EnsureSlip();
            _os.Tick(director.Run, _slip);
            Glow();
        }

        private void EnsureSlip()
        {
            if (_slipRunGen == director.RunGeneration && _slip != null) return;
            _slip = new BetslipModel(director.Run);
            _slipRunGen = director.RunGeneration;
            _os?.ResetForRun();
        }

        private void BuildSkeleton()
        {
            int w = referencePixelsWide;
            int h = Mathf.RoundToInt(referencePixelsWide * screenWorldSize.y / screenWorldSize.x);

            var canvasGo = new GameObject("LaptopOsCanvas", typeof(Canvas), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            var canvasRt = _canvas.GetComponent<RectTransform>();
            canvasRt.sizeDelta = new Vector2(w, h);

            if (lidRenderer != null)
            {
                Transform lid = lidRenderer.transform;
                Vector3 outwardNormal = -lid.forward;
                canvasGo.transform.SetPositionAndRotation(
                    lid.position + outwardNormal * canvasOffset,
                    Quaternion.LookRotation(-outwardNormal, lid.up));
            }
            canvasGo.transform.localScale = Vector3.one * (screenWorldSize.x / w);

            Transform root = canvasGo.transform;
            LaptopUi.MakeStretchImage(root, "DesktopBacking", LaptopOs.Ink).raycastTarget = false;
            _os = new LaptopOs((RectTransform)root, _font, _fontCond, this, w, h);
        }

        private void Glow()
        {
            if (lidRenderer == null) return;
            Phase phase = director.Run.Phase;
            bool wantsYou = phase == Phase.Betting || phase == Phase.Shop
                || phase == Phase.RunWon || phase == Phase.RunLost;
            bool engaged = DeskFocus.Active != null;
            Color emission = idleEmission;
            if (wantsYou && !engaged)
            {
                float breathe = 0.5f + 0.5f * Mathf.Sin(Time.time * attentionBreathHz * 2f * Mathf.PI);
                emission = Color.Lerp(idleEmission, attentionEmission, breathe);
            }
            _emissBlock.SetColor(EmissionColorId, emission);
            lidRenderer.SetPropertyBlock(_emissBlock);
        }

        /// <summary>
        /// Resolves one of the two production faces, falling back to Unity's built-in font if the
        /// asset is missing so a bad import degrades to readable text rather than to a blank screen.
        /// A fallback is loud in the log on purpose: silently rendering the wrong face is the kind
        /// of defect that survives a review, because nothing looks broken.
        /// </summary>
        private static Font LoadFont(string resourcePath)
        {
            var font = Resources.Load<Font>(resourcePath);
            if (font != null)
            {
                WarmFontAtlas(font);
                return font;
            }

            Debug.LogWarning($"[LaptopScreen] production face '{resourcePath}' did not load; "
                + "falling back to LegacyRuntime. The surface will render in the wrong voice.");
            try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch
            {
                Debug.LogWarning("[LaptopScreen] built-in font not found; text will not render.");
                return null;
            }
            WarmFontAtlas(font);
            return font;
        }

        /// <summary>Bakes every character/size pair SureThing renders into the dynamic font's atlas
        /// once, synchronously, before the first UI build, so no Text build is ever the "first use"
        /// that makes the atlas repack. This is a robustness/hitch measure, not a bug fix: it was
        /// written against a suspected atlas race behind the "reason label paints only two glyphs"
        /// defect, and it did not fix it — that defect was pure occlusion (the Skip button was drawn
        /// over the label; see SportsbookApp.BuildSlip). Kept because pre-warming a dynamic atlas on
        /// a world-space canvas that rebuilds every interaction is worth the one-time cost. The
        /// charset is best-effort; an unlisted character still rasterizes on demand as before.</summary>
        private static void WarmFontAtlas(Font font)
        {
            if (font == null) return;
            const string charset =
                " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" +
                ".,:;!?'\"()[]/%$+-—−·■▰›←→⇄@¤✓";
            int[] sizes = { 9, 11, 12, 13, 15, 16, 17, 18, 19, 20, 21, 22, 23, 26, 28, 30, 31 };
            foreach (int size in sizes)
                font.RequestCharactersInTexture(charset, size, FontStyle.Normal);
        }
    }
}
