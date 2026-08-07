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
        // R35 (DD 2026-08-05, batch 11): the violet attention glow is STRUCK -- a fourth light in
        // a three-source room, in a retired hue, on his machine. Built to the ruled DIRECTION:
        // the screen's own emitted family, warm, low chroma. THE EXACT VALUES ARE NOT RULED --
        // they are ruled on the frame this build produces, which is the first colour in the room
        // no capture can see yet. Treat these as the proposal, not the decision.
        //
        // Was: idle (0.025, 0.035, 0.055) hue 271deg, attention (0.28, 0.10, 0.55) hue 312deg
        //      chroma 64.1, measured on frame at hue 303.6 / chroma 9.17.
        //
        // BOTH ends move, and that is the direction's consequence rather than a widened scope.
        // Glow() LERPS between them, so warming only the attention end would have made the pulse
        // travel 271deg -> 312deg: a colour cycle, which is a worse artefact than the one being
        // removed. One family means one hue at two brightnesses.
        //
        //   idle       hue 83.3deg  chroma 5.4  luminance 0.0327  (was 0.0343 -- rest spill held)
        //   attention  hue 82.4deg  chroma 8.6  luminance 0.1330  = 4.07x idle (was 4.98x)
        //   hue travel across the breathe: 0.9deg -- brightness only
        //
        // Warm family placement: the room's key is #D8C48A at hue 92deg, so this sits just red of
        // the fluorescent rather than announcing itself as a separate source. Chroma drops 64.1 ->
        // 8.6, which is what stops it being a fourth light. Kept slightly quieter than before
        // (4.07x vs 4.98x) because §1.2 requires screens "quiet, with faint spill" -- but the cue
        // must still carry from the couch, which is the trade the DD is being asked to judge.
        // R35/R37 (DD 2026-08-05, batch 12) — built to the RULE, values still to be ruled on the
        // frame this produces:
        //
        //   warm near-neutral, R >= G > B   ..... 0.038 >= 0.032 > 0.024, and x3 preserves it
        //   attention differs by AMPLITUDE ONLY .. attention IS idle * 3, exactly
        //   ~3x maximum ......................... 3.00x by construction (the last build was 4.07x)
        //   idleEmission carries the same defect . both ends are one colour now
        //
        // Writing attention as idle x 3 rather than as a second hand-picked triple is the point:
        // "amplitude only" then holds BY CONSTRUCTION instead of by my matching two chromaticities
        // and asserting they agree. Any future edit to idle carries attention with it, so the two
        // cannot drift apart the way idle (cool) and attention (violet) had.
        [ColorUsage(false, true)] public Color idleEmission = new Color(0.038f, 0.032f, 0.024f);
        [ColorUsage(false, true)] public Color attentionEmission = new Color(0.114f, 0.096f, 0.072f);
        // attentionBreathHz REMOVED with the pulse (R37). Left in place it would be a dead
        // serialized dial inviting someone to reinstate the breathing it used to drive.

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
            LaptopUi.MakeStretchImage(root, "DesktopBacking", Color.black).raycastTarget = false;
            _os = new LaptopOs((RectTransform)root, _font, this, w, h);
        }

        private void Glow()
        {
            if (lidRenderer == null) return;
            Phase phase = director.Run.Phase;
            bool wantsYou = phase == Phase.Betting || phase == Phase.Shop
                || phase == Phase.RunWon || phase == Phase.RunLost;
            bool engaged = DeskFocus.Active != null;
            // R37 (DD 2026-08-05): NO PULSE. One step up, held for as long as the laptop wants him,
            // one step back. The breathing was ruled a violation in its own right and separately
            // from the colour: a continuously animated light is a fourth source that also MOVES,
            // and it read as the room breathing rather than as a machine waiting.
            //
            // There is deliberately no easing here. A lerp with a duration would be the same
            // finding wearing a shorter clock -- the ruling says step, and a step is an assignment.
            Color emission = (wantsYou && !engaged) ? attentionEmission : idleEmission;
            _emissBlock.SetColor(EmissionColorId, emission);
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
