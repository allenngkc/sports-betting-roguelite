using System;
using SBR.Engine;
using TMPro;
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

        [Header("Lid emission")]
        // S63-am2 (DD 2026-08-07, batch 13). TWO rulings landed on this field, and they point
        // opposite ways:
        //
        //   1. THE COLOUR IS GRANTED AND SHIPS, BOTH ENDS. Chroma 68.97 -> 0.24; the room cast
        //      moved 355.7deg red -> 85.1deg against the room's key at 92deg. idleEmission is
        //      always on and must not be cool for 99% of the running time, so that half was
        //      ruled unconditional.
        //
        //   2. THE ATTENTION CUE IS STRUCK. It was suspended pending one Play Mode frame with
        //      wantsYou && !engaged true from a pose containing the laptop; the disposition was
        //      pre-committed both ways. The frame was shot (artifacts/room-visual-pass/
        //      s63am2-glow-cue, RoomViewCapture.CaptureLidEmissionInPlay) and the cue cannot be
        //      framed:
        //
        //        seated pose ... the lid is OUT OF FRUSTUM (viewport x -0.638). When the cue
        //                        fires the player is at the TV, and the laptop is not in shot.
        //        focused pose .. IDENTICAL, 0.00%. At 0.52m, dead centre, the lid filling ~80%
        //                        of the frame, a 3x step changes NOTHING - BuildSkeleton() puts
        //                        the SureThing canvas 4mm in front at the lid's own world size,
        //                        so the surface is behind an opaque quad.
        //        standing pose . 233 px above JND out of 3.69M (0.0063%), max 9 levels, and the
        //                        picture shows what they are: a one-pixel rim line on the lid's
        //                        exposed edge.
        //        room cast ..... 0.000 on right wall, floor aisle and ceiling plaster.
        //
        //      The ~3x ceiling had already been struck as a bound that did not bind. Note that
        //      raising the amplitude could not have rescued this either: occlusion and framing
        //      are both amplitude-invariant. That is why the ceiling was never what stopped it.
        //
        // attentionEmission is REMOVED, not zeroed, for the same reason attentionBreathHz was
        // (R37): a dead serialized field is an invitation to reinstate the behaviour it drove.
        // The struck value survives only as a quoted comparand inside the capture harness.
        //
        // What remains is one colour on one surface. The lid does not signal; it is lit.
        /// <summary>
        /// The granted lid colour, and the ONE definition of it. L* 21.09, chroma 5.4, hue 83.3deg,
        /// R >= G > B — warm near-neutral, sitting just red of the room's #D8C48A key so it reads as
        /// the screen's own light rather than as a separate source. It replaced a violet
        /// (0.28, 0.10, 0.55) at chroma 64.1 and a cool idle (0.025, 0.035, 0.055).
        ///
        /// R40 (DD 2026-08-07, batch 14) is why this is a shared constant rather than a literal here
        /// and another in the builder. The ScreenLaptop MATERIAL carried (0.025, 0.055, 0.035) —
        /// hue 155.5deg, green-dominant, 72deg and 2.5x chroma away from this — while the runtime
        /// property block wrote the granted value over the top. The player saw the ruling; the APV
        /// bake and every Edit Mode capture saw the contradiction, including the captures that
        /// settled this colour one batch earlier. Two literals is how that happens quietly, and
        /// R19(b) already had to un-drift this project's institutional metal once for the same
        /// reason — the fix there was one shared factory, HousingSteelMat(), and this is that.
        ///
        /// PhoneScreen takes its rest value from here too: the phone is HIS (§6), same personal
        /// register as the laptop, so both of his screens rest on one authored chromaticity.
        /// </summary>
        public static readonly Color GrantedLidEmission = new Color(0.038f, 0.032f, 0.024f);

        [ColorUsage(false, true)] public Color idleEmission = GrantedLidEmission;

        private Canvas _canvas;
        private TMP_FontAsset _font;
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
        private TMP_FontAsset _fontCond;
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
            // C15: the two TMP assets S11's ruling named, generated by
            // SBR/Editor/SureThingTmpFontAssets from these same TTFs. The " SDF" suffix is the
            // generator's naming and the only thing that changed at this seam — LoadFont was already
            // the single place both voices resolve, which is why C15 scoped the migration off it.
            _font = LoadFont("SureThing/Fonts/Archivo SDF");
            _fontCond = LoadFont("SureThing/Fonts/ArchivoNarrow SDF");
            _emissBlock = new MaterialPropertyBlock();
            if (tv == null) tv = FindAnyObjectByType<TvSweatScreen>();
            BuildSkeleton();
            ApplyLidEmission();
        }

        private void Update()
        {
            if (director == null || director.Run == null) return;
            if (_canvas.worldCamera == null) _canvas.worldCamera = Camera.main;

            EnsureSlip();
            _os.Tick(director.Run, _slip);
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

        /// <summary>
        /// Writes the granted lid colour once. Not per-frame: with the cue struck there is no
        /// state left to track, and re-asserting a constant every Update would imply one.
        ///
        /// This still has to run, because the property block is what makes the lid the ruled
        /// colour -- the ScreenLaptop MATERIAL carries a different, older emission and disagrees
        /// with the ruling. That disagreement is routed, not fixed here: the material's value is
        /// what the APV bake and every Edit Mode capture see, so changing it re-opens the bake
        /// and the structural gates.
        /// </summary>
        private void ApplyLidEmission()
        {
            if (lidRenderer == null) return;
            _emissBlock.SetColor(EmissionColorId, idleEmission);
            lidRenderer.SetPropertyBlock(_emissBlock);
        }

        /// <summary>
        /// Resolves one of the two production faces, falling back to Unity's built-in font if the
        /// asset is missing so a bad import degrades to readable text rather than to a blank screen.
        /// A fallback is loud in the log on purpose: silently rendering the wrong face is the kind
        /// of defect that survives a review, because nothing looks broken.
        /// </summary>
        private static TMP_FontAsset LoadFont(string resourcePath)
        {
            var font = Resources.Load<TMP_FontAsset>(resourcePath);
            if (font != null)
            {
                WarmFontAtlas(font);
                return font;
            }

            // C15: the fallback is TMP's own default face now rather than LegacyRuntime, because a
            // UnityEngine.Font cannot be handed to a TextMeshProUGUI at all — the failure mode
            // changed from "renders in the wrong voice" to "renders nothing", which is worse and
            // silent. TMP_Settings.defaultFontAsset is whatever the essential resources installed.
            Debug.LogWarning($"[LaptopScreen] production face '{resourcePath}' did not load; "
                + "falling back to TMP's default asset. The surface will render in the wrong voice.");
            font = TMP_Settings.defaultFontAsset;
            if (font == null)
                Debug.LogWarning("[LaptopScreen] TMP default font asset is null; text will not render. "
                    + "TMP essential resources are probably missing — run tools/tmp-phase-l-bootstrap.ps1.");
            else WarmFontAtlas(font);
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
        private static void WarmFontAtlas(TMP_FontAsset font)
        {
            if (font == null) return;
            const string charset =
                " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" +
                ".,:;!?'\"()[]/%$+-—−·■▰›←→⇄@¤✓";

            // **The per-size loop is gone, and that is the migration, not an omission.** A dynamic
            // UGUI font rasterises a separate bitmap per character AND size, so warming meant
            // seventeen passes over the charset. An SDF atlas holds one signed-distance rendering per
            // character and scales it, so size is a shader parameter and one pass is the whole job.
            //
            // **And this now reports what it could only swallow before.** RequestCharactersInTexture
            // returned nothing useful, so a glyph the face lacked was silently absent — the S2/C18
            // shape, and the specific risk I accepted when choosing a Dynamic atlas over a Static
            // one. TryAddCharacters hands back exactly which characters failed, so a missing glyph is
            // now a line in the log at boot instead of a hole somebody finds on a frame.
            //
            // This charset is also the honest answer to "what does this surface print" — it is the
            // list the UGUI warm-up already maintained, including U+2212 MINUS (S30) and the middot.
            // Generated team names are ASCII and covered by the letter ranges.
            if (!font.TryAddCharacters(charset, out string missing) && !string.IsNullOrEmpty(missing))
                Debug.LogWarning($"[LaptopScreen] '{font.name}' is missing glyphs for: {missing} — "
                    + "these render as nothing, not as a fallback box.");
        }
    }
}
