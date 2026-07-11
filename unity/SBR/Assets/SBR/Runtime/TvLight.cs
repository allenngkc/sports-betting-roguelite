using UnityEngine;

namespace SBR.Game
{
    /// <summary>
    /// The room's reaction shot (design/08: "the room is the reaction shot"). A point light near the TV
    /// whose colour/intensity follows the screen state - phosphor idle, a green flare on a GREEN leg, a
    /// red wash on a DEAD leg, a gold pulse on cash-out. It eases a transient Flash back to whatever the
    /// current rest mood is, and adds a faint intensity flicker so the phosphor never sits perfectly
    /// still. TvSweatScreen drives it; all code, no assets.
    /// </summary>
    public sealed class TvLight : MonoBehaviour
    {
        [Header("Wiring (set by GrayboxRoomBuilder)")]
        public Light pointLight;

        [Header("Idle (resting mood)")]
        [ColorUsage(false)] public Color idleColor = new Color(0.35f, 1f, 0.5f);
        public float idleIntensity = 0.5f;

        [Header("Flash + flicker dials")]
        [Tooltip("How fast a flash eases back to the rest mood, per second.")]
        public float flashDecay = 2.6f;
        [Tooltip("Idle intensity flicker amplitude (phosphor hum), as a fraction of intensity.")]
        public float flickerAmp = 0.05f;
        public float flickerHz = 11f;

        private Color _restColor;
        private float _restIntensity;
        private Color _flashColor;
        private float _flashIntensity;
        private float _flash01;
        private float _flickerSeed;

        private void Awake()
        {
            if (pointLight == null) pointLight = GetComponent<Light>();
            _restColor = idleColor;
            _restIntensity = idleIntensity;
            _flashColor = idleColor;
            _flashIntensity = idleIntensity;
            _flickerSeed = Random.value * 100f;
        }

        /// <summary>Sets the steady mood the flash eases back toward (e.g. dimmer + redder after a bust).</summary>
        public void SetRest(Color color, float intensity)
        {
            _restColor = color;
            _restIntensity = intensity;
        }

        public void ResetToIdle() => SetRest(idleColor, idleIntensity);

        /// <summary>Kicks a transient flare of the given colour/intensity that decays back to rest.</summary>
        public void Flash(Color color, float intensity)
        {
            _flashColor = color;
            _flashIntensity = intensity;
            _flash01 = 1f;
        }

        private void Update()
        {
            if (pointLight == null) return;

            _flash01 = Mathf.MoveTowards(_flash01, 0f, flashDecay * Time.deltaTime);
            Color c = Color.Lerp(_restColor, _flashColor, _flash01);
            float baseI = Mathf.Lerp(_restIntensity, _flashIntensity, _flash01);
            float flick = 1f + (Mathf.PerlinNoise(_flickerSeed, Time.time * flickerHz) - 0.5f) * 2f * flickerAmp;

            pointLight.color = c;
            pointLight.intensity = Mathf.Max(0f, baseI * flick);
        }
    }
}
