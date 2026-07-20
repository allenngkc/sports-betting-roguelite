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
            Color emission = idleEmission;
            if (wantsYou && !engaged)
            {
                float breathe = 0.5f + 0.5f * Mathf.Sin(Time.time * attentionBreathHz * 2f * Mathf.PI);
                emission = Color.Lerp(idleEmission, attentionEmission, breathe);
            }
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
