using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SBR.Game;
using UnityEngine;
using UnityEngine.UI;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// TV sweat refinement, Parts 2 &amp; 3: the beat-flash re-mapping onto DESIGN.md §4 (gold rationed
    /// to money, loss is darkness, everything else cold white/grey — green and red retired outright),
    /// and the canvas HDR path that lets §3's L4 tier exceed 1.0. These construct a `TvSweatScreen` in
    /// isolation (never entering play mode, `theaterEnabled = false` so `BuildCanvas` never touches
    /// `TheaterStage`/audio) and call the private `Awake`/`BuildCanvas` directly by reflection —
    /// mirroring the existing PlayMode suite's `PressCashOutInteract` pattern for exercising
    /// production methods instead of duplicating their logic.
    /// </summary>
    public class TvSweatScreenPaletteTests
    {
        private static float Luminance(Color c) => (c.r + c.g + c.b) / 3f;

        // Calibrated against the OLD retired literals so a reintroduction of either hue at a similar
        // magnitude is caught, without false-flagging the approved gold (r-dominant but g moderate)
        // or the approved cyan/white/grey (no channel dominates by this margin).
        private static bool LooksLikeRetiredRed(Color c) => c.r > 0.7f && c.g < 0.25f && c.b < 0.25f;
        private static bool LooksLikeRetiredGreen(Color c) => c.g > 0.7f && c.r < 0.35f && c.b < 0.6f;

        [Test]
        public void Retired_green_and_red_fields_no_longer_exist_on_the_type()
        {
            Assert.IsNull(typeof(TvSweatScreen).GetField("phosphorGreen"),
                "phosphorGreen must be gone — DESIGN.md §4 retires green outright");
            Assert.IsNull(typeof(TvSweatScreen).GetField("hotRed"),
                "hotRed must be gone — DESIGN.md §4 retires red outright");
        }

        [Test]
        public void No_public_colour_field_reads_as_the_retired_saturated_red_or_green()
        {
            var go = new GameObject("PaletteScan");
            go.SetActive(false); // field defaults only — never let Awake/OnEnable fire here
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();
                var offenders = typeof(TvSweatScreen)
                    .GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.FieldType == typeof(Color))
                    .Select(f => (f.Name, Color: (Color)f.GetValue(screen)))
                    .Where(x => LooksLikeRetiredRed(x.Color) || LooksLikeRetiredGreen(x.Color))
                    .Select(x => x.Name)
                    .ToList();

                Assert.IsEmpty(offenders,
                    $"these public Color fields still read as the retired money-good-green / " +
                    $"money-bad-red language: {string.Join(", ", offenders)}");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Ordering_gold_below_goldL4_and_deadDark_below_gold_holds()
        {
            var go = new GameObject("OrderingCheck");
            go.SetActive(false); // field defaults only — never let Awake/OnEnable fire here
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();

                // idle < flash < L4 (room-lead-reply.md §1): the quad's idle floor sits at ~0.068
                // (set in GrayboxRoomBuilder, out of this file's reach). A money/won flash must clear
                // it, and the true L4 moment (cash-out accept, payout tally) must clear the ordinary
                // money flash in turn — otherwise a win reads darker than rest, or L4 reads the same
                // as a routine leg win.
                const float quadIdleFloor = 0.068f;
                Assert.Greater(Luminance(screen.gold), quadIdleFloor,
                    "a money/won flash must read brighter than the quad's idle floor");
                Assert.Greater(Luminance(screen.goldL4), Luminance(screen.gold),
                    "the L4 tier (cash-out accept / payout tally) must read brighter than a routine gold flash");

                // Loss is darkness, not merely dim: it must drop BELOW the idle floor, the opposite
                // direction from every money flash, or the "dead" beat stops reading as a dip.
                Assert.Less(Luminance(screen.deadDark), quadIdleFloor,
                    "a loss/dead flash must drop below the idle floor to read as darkness, not a dim flash");
                Assert.Less(Luminance(screen.deadDark), Luminance(screen.gold),
                    "loss must never be as bright as a money beat");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GoldL4_carries_genuine_HDR_magnitude()
        {
            var go = new GameObject("HdrMagnitude");
            go.SetActive(false); // field defaults only — never let Awake/OnEnable fire here
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();
                Assert.Greater(screen.goldL4.r, 1f,
                    "goldL4 must exceed 1.0 at the source so it has something to hand the HDR-boosted " +
                    "canvas material / the shared bloom volume once it clears the UGUI vertex-colour clamp");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Canvas_black_floor_matches_the_rooms_quad_lift()
        {
            var go = new GameObject("BlackFloor");
            go.SetActive(false); // field defaults only — never let Awake/OnEnable fire here
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();
                var floor = new Color(0.048f, 0.055f, 0.068f);
                const float tol = 1e-4f;

                AssertRgbApprox(floor, screen.screenBg, tol, nameof(screen.screenBg));
                AssertRgbApprox(floor, screen.barBgColor, tol, nameof(screen.barBgColor));
                AssertRgbApprox(floor, screen.pitchBgColor, tol, nameof(screen.pitchBgColor));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        private static void AssertRgbApprox(Color expected, Color actual, float tol, string label)
        {
            Assert.AreEqual(expected.r, actual.r, tol, $"{label}.r below the room's quad-lift floor");
            Assert.AreEqual(expected.g, actual.g, tol, $"{label}.g below the room's quad-lift floor");
            Assert.AreEqual(expected.b, actual.b, tol, $"{label}.b below the room's quad-lift floor");
        }

        [Test]
        public void Hdr_ui_shader_is_present_in_the_build()
        {
            Shader shader = Shader.Find("SBR/TvSweatHdrUI");
            Assert.IsNotNull(shader,
                "SBR/TvSweatHdrUI must be importable — without it the L4 canvas elements silently " +
                "fall back to the LDR-clamped default UI material (TvSweatScreen.MakeHdrMaterial's " +
                "documented, non-throwing fallback)");
        }

        [Test]
        public void L4_canvas_elements_get_the_hdr_material_and_L3_elements_stay_default()
        {
            var go = new GameObject("CanvasWiring");
            go.SetActive(false); // defer Awake so BuildCanvas runs once, under our control
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();
                screen.theaterEnabled = false; // keep BuildCanvas from touching TheaterStage/audio

                InvokePrivate(screen, "Awake");

                Text cashOut = FindChild<Text>(screen, "CashOut");
                Text bigAmount = FindChild<Text>(screen, "BigAmount");
                Image goldFlood = FindChild<Image>(screen, "GoldFlood");
                Image wonFlood = FindChild<Image>(screen, "WonFlood");
                Text flavor = FindChild<Text>(screen, "Flavor");

                Assert.IsNotNull(cashOut, "CashOut text not found — canvas layout changed?");
                Assert.IsNotNull(bigAmount, "BigAmount text not found — canvas layout changed?");
                Assert.IsNotNull(goldFlood, "GoldFlood image not found — canvas layout changed?");
                Assert.IsNotNull(wonFlood, "WonFlood image not found (renamed from GreenFlood) — canvas layout changed?");
                Assert.IsNotNull(flavor, "Flavor text not found — canvas layout changed?");

                Assert.AreEqual("SBR/TvSweatHdrUI", cashOut.material.shader.name,
                    "the cash-out band must be able to reach L4 (§8.5 Actionable)");
                Assert.AreEqual("SBR/TvSweatHdrUI", bigAmount.material.shader.name,
                    "the big win/cash-out amount must be able to reach L4 (§3: the payoff at its callback)");
                Assert.AreEqual("SBR/TvSweatHdrUI", goldFlood.material.shader.name,
                    "the gold flood must be able to reach L4 for the cash-out/payout beats");

                // Flavor text carries routine (non-L4) beat copy — it must NOT have opted into the
                // HDR material, or every beat would silently compete for the one L4 slot DESIGN.md §3
                // reserves for a single element.
                Assert.AreNotEqual("SBR/TvSweatHdrUI", flavor.material.shader.name,
                    "routine beat text must stay on the default (LDR) UI material — only one L4 element at a time");

                // No trace of the old GameObject name survives the WonLegBeat rename.
                Assert.IsNull(FindChild<Image>(screen, "GreenFlood"), "GreenFlood must be renamed to WonFlood");

                // The renamed flood's construction colour is gold-hued, not the retired green.
                Assert.IsFalse(LooksLikeRetiredGreen(new Color(wonFlood.color.r, wonFlood.color.g, wonFlood.color.b)),
                    "WonFlood's colour must be gold, not the retired green");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        private static void InvokePrivate(object target, string method)
        {
            MethodInfo m = target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(m, $"{target.GetType().Name}.{method} not found by reflection — was it renamed?");
            m.Invoke(target, null);
        }

        private static T FindChild<T>(Component root, string childName) where T : Component
        {
            foreach (T c in root.GetComponentsInChildren<T>(true))
                if (c.name == childName) return c;
            return null;
        }
    }
}
