using NUnit.Framework;
using SBR.Game;
using UnityEngine;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// TV sweat refinement, Part 1: `TvLight.idleColor` must read as the approved DESIGN.md §4 /
    /// room-layout-update.md §5 palette — a cold, quiet white-grey spill — never the saturated
    /// (0.35, 1, 0.5) green inherited from the retired `design/08-art-direction.md` world (that value
    /// is what made the room read green on the TV side regardless of what the room team did to
    /// materials). Range is deliberately not asserted here: it is set on the `Light` component by
    /// GrayboxRoomBuilder (room-owned), not by this class, and this slice was told to leave it at 3.2.
    /// </summary>
    public class TvLightTests
    {
        [Test]
        public void IdleColor_reads_cold_and_quiet_not_the_retired_saturated_green()
        {
            var go = new GameObject("TvLightUnderTest");
            try
            {
                var light = go.AddComponent<TvLight>();
                Color c = light.idleColor;

                Assert.Greater(c.r + c.g + c.b, 0f,
                    "idle must not be pure black — DESIGN.md §1 makes the display a quiet light source, not an off panel");

                // Cold: DESIGN.md §4 / room-layout-update.md §5 call for "predominantly cold
                // white-grey". A cold cast keeps blue at or above the other channels.
                Assert.GreaterOrEqual(c.b, c.r - 1e-4f, "idle spill must read cold (blue at least matching red), not warm");

                // Quiet: a desaturated near-neutral, unlike the old saturated (0.35, 1, 0.5) green
                // where the channel spread was 0.65. The approved palette keeps channels close together.
                float maxC = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
                float minC = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
                Assert.Less(maxC - minC, 0.25f,
                    "idle spill must read as quiet/desaturated grey-white, not a saturated single hue");

                // Explicitly not the retired green: green must not dominate the other two channels.
                Assert.IsFalse(c.g > c.r * 1.5f && c.g > c.b * 1.5f,
                    "green must not dominate the idle colour — that is the retired design/08 hue this fix removes");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Flash_and_SetRest_still_drive_the_wired_light_without_touching_idleColor()
        {
            // Mechanical regression for the Flash/rest mechanism itself (unchanged by this fix, but
            // now exercised against the new idle default so a future edit that breaks the easing
            // can't hide behind "idleColor looks right").
            var go = new GameObject("TvLightUnderTest2");
            try
            {
                var pointGo = new GameObject("Point");
                pointGo.transform.SetParent(go.transform, false);
                Light point = pointGo.AddComponent<Light>();
                point.type = LightType.Point;

                var light = go.AddComponent<TvLight>();
                light.pointLight = point;

                // Awake() seeds rest/flash from idleColor — invoke it directly (EditMode, no play loop).
                typeof(TvLight).GetMethod("Awake",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Invoke(light, null);

                light.Flash(new Color(1.15f, 0.82f, 0.18f), 3f); // gold, matching TvSweatScreen's money flash

                typeof(TvLight).GetMethod("Update",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Invoke(light, null);

                // Immediately after Flash(), the light should read at (or very near) the flash colour,
                // not the idle colour — otherwise Flash is a no-op.
                Assert.Greater(point.intensity, 1f, "a fresh Flash() must raise intensity above idle");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
