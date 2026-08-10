using UnityEngine;

namespace SBR.Game
{
    /// <summary>
    /// The room's reaction shot (DESIGN.md §1: "the surface is a light source, not a picture" — the
    /// room's lighting rig is briefed to carry this). A point light near the TV whose colour/intensity
    /// follows the screen state. Idle is a cold, quiet spill (DESIGN.md §4: cold white-grey, gold
    /// rationed to money) — never the saturated green of the retired `design/08-art-direction.md`
    /// world. Money/won beats pulse gold; loss beats drop the light toward darkness rather than
    /// flashing a money-bad red, which DESIGN.md §4 retires along with green. It eases a transient
    /// Flash back to whatever the current rest mood is. TvSweatScreen drives it; all code, no assets.
    ///
    /// <para>T64, extended by rule: this component used to "add a faint intensity flicker so the
    /// idle spill never sits perfectly still" at 11 Hz, always on. T64 struck the panel's 9 Hz
    /// emission flicker for three reasons that apply here verbatim — a display that works is not a
    /// display that flickers, the surface has exactly one pulse kind and it is LIVE, and an effect
    /// with no fire condition is continuous involuntary motion in the player's peripheral vision
    /// for the entire sweat. This one was the worse of the two: the panel's flicker moved the
    /// panel, this one moved the whole room. The ruling calls emission "C18 §4.2's largest
    /// remaining hole ... it reaches the player as light, and every instrument the studio has scans
    /// pixels" — this is that hole's other half, and fixing only the named site would have been the
    /// fix-by-site error this slice has now paid for three times. REMOVED, not zeroed.</para>
    /// </summary>
    public sealed class TvLight : MonoBehaviour
    {
        [Header("Wiring (set by GrayboxRoomBuilder)")]
        public Light pointLight;

        [Header("Idle (resting mood)")]
        // DESIGN.md §4 / room-layout-update.md §5: "predominantly cold white-grey ... a touch of gold
        // near the cash-out band." A near-neutral cool grey-white (B > G > R keeps the cast cold) with
        // R nudged toward G rather than left far below it, standing in for that small warm admixture —
        // NOT the saturated (0.35, 1, 0.5) green of the deprecated design/08 world, which is why the
        // room read green on the TV side regardless of what the room team did to materials.
        [ColorUsage(false)] public Color idleColor = new Color(0.72f, 0.75f, 0.80f);
        public float idleIntensity = 0.5f;

        [Header("Flash dial")]
        [Tooltip("How fast a flash eases back to the rest mood, per second.")]
        public float flashDecay = 2.6f;
        // T64: `flickerAmp` and `flickerHz` are struck and removed, not zeroed. Their serialized
        // values survive in Room.unity until that scene is next written — which is exactly why the
        // field is deleted rather than defaulted to 0. Batch 13 recorded this trap from the room
        // lane in the same breath: changing a public field's default does not touch an
        // already-serialized component, so the scene kept the old value and the A/B captured the
        // very thing it was meant to replace. A deleted field cannot be overridden by a stale scene.

        private Color _restColor;
        private float _restIntensity;
        private Color _flashColor;
        private float _flashIntensity;
        private float _flash01;

        private void Awake()
        {
            if (pointLight == null) pointLight = GetComponent<Light>();
            _restColor = idleColor;
            _restIntensity = idleIntensity;
            _flashColor = idleColor;
            _flashIntensity = idleIntensity;
            // T64: `_flickerSeed = Random.value * 100f` went with the flicker. This component now
            // calls UnityEngine.Random nowhere, so the room's spill is identical run to run — one
            // fewer presentation-local source for a frame-locked A/B to pin.
        }

        /// <summary>Sets the steady mood the flash eases back toward (e.g. dimmer after a bust).
        ///
        /// <para>T34: this said "dimmer + REDDER after a bust", which contradicts this file's own
        /// header — "never ... a money-bad red, which DESIGN.md §4 retires along with green" — and
        /// contradicts §4/§8's "loss is still darkness". No red is passed here by any TV caller
        /// (<c>DeadLegBeat</c> rests on <c>deadDark</c>), so the wording was the last surviving
        /// instruction to do the banned thing. A comment that licenses a violation is how the
        /// violation comes back.</para></summary>
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

            // T64: the flicker multiplier that used to scale this is gone. The room's spill is now
            // exactly the mood it is in.
            pointLight.color = c;
            pointLight.intensity = Mathf.Max(0f, baseI);
        }
    }
}
