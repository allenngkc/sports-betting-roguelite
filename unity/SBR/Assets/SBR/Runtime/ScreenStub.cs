using System.Collections;
using UnityEngine;

namespace SBR.Game
{
    /// <summary>
    /// Placeholder screen interaction for the laptop, phone, and TV until M3/M4 hang
    /// real surfaces on them. OnInteract logs and pulses the screen quad's emission
    /// via a MaterialPropertyBlock (the material must have the _EMISSION keyword on,
    /// which GrayboxRoomBuilder guarantees for every screen material).
    /// The screen renderer must NOT be in highlightRenderers - hover tint and the
    /// pulse use separate property blocks on separate renderers.
    /// </summary>
    public sealed class ScreenStub : Interactable
    {
        [Header("Wiring (set by GrayboxRoomBuilder)")]
        public string prompt = "Use";
        public Renderer screenRenderer;
        [ColorUsage(false, true)]
        public Color idleEmission = new Color(0.02f, 0.05f, 0.03f);
        [ColorUsage(false, true)]
        public Color pulseEmission = new Color(0.12f, 1.1f, 0.35f);

        [Header("Dials")]
        public float pulseDuration = 0.6f;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private MaterialPropertyBlock _screenBlock;
        private Coroutine _pulse;

        public override string Prompt => prompt;

        protected override void Awake()
        {
            base.Awake();
            _screenBlock = new MaterialPropertyBlock();
        }

        public override void OnInteract(PlayerInteractor player)
        {
            Debug.Log($"[ScreenStub] '{name}': {prompt} (placeholder until M3/M4)");
            if (screenRenderer == null)
                return;
            if (_pulse != null)
                StopCoroutine(_pulse);
            _pulse = StartCoroutine(Pulse());
        }

        private IEnumerator Pulse()
        {
            float duration = Mathf.Max(0.05f, pulseDuration);
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / duration);
                float wave = Mathf.Sin(t * Mathf.PI); // rise then settle
                _screenBlock.SetColor(EmissionColorId, Color.Lerp(idleEmission, pulseEmission, wave));
                screenRenderer.SetPropertyBlock(_screenBlock);
                yield return null;
            }

            _screenBlock.Clear(); // back to the material's own emission
            screenRenderer.SetPropertyBlock(_screenBlock);
            _pulse = null;
        }
    }
}
