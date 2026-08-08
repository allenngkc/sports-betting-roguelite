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
        // Granted value. Warm near-neutral, R >= G > B, sitting just red of the room's #D8C48A
        // key so it reads as the screen's own light rather than as a separate source. It replaced
        // a violet (0.28, 0.10, 0.55) at chroma 64.1 and a cool idle (0.025, 0.035, 0.055).
        [ColorUsage(false, true)] public Color idleEmission = new Color(0.038f, 0.032f, 0.024f);

        private Canvas _canvas;
        private Font _font;
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
            _font = LoadFont();
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
            LaptopUi.MakeStretchImage(root, "DesktopBacking", Color.black).raycastTarget = false;
            _os = new LaptopOs((RectTransform)root, _font, this, w, h);
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
