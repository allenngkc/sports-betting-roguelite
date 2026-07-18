using NUnit.Framework;
using SBR.Game;
using UnityEngine;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// The stage's geometry laws (F_0.2.0 M-T2): formation mirroring is exact, attack bias
    /// pushes toward the attacked goal and clamps, territory is monotonic in the picked
    /// side's probability with midfield at even money.
    /// </summary>
    public class PitchLayoutTests
    {
        [Test]
        public void Formation_mirrors_exactly_across_the_halfway_line()
        {
            for (int i = 0; i < PitchLayout.OutfieldPerTeam; i++)
            {
                Vector2 right = PitchLayout.FormationSlot(i, attacksRight: true, attackBias: 0f);
                Vector2 left = PitchLayout.FormationSlot(i, attacksRight: false, attackBias: 0f);
                Assert.AreEqual(right.x, 1f - left.x, 1e-5f, $"slot {i} x must mirror");
                Assert.AreEqual(right.y, left.y, 1e-5f, $"slot {i} y must not mirror");
            }
            Assert.AreEqual(PitchLayout.Keeper(true).x, 1f - PitchLayout.Keeper(false).x, 1e-5f);
        }

        [Test]
        public void Attack_bias_pushes_toward_the_attacked_goal_and_clamps()
        {
            for (int i = 0; i < PitchLayout.OutfieldPerTeam; i++)
            {
                float neutral = PitchLayout.FormationSlot(i, true, 0f).x;
                float forward = PitchLayout.FormationSlot(i, true, 1f).x;
                float back = PitchLayout.FormationSlot(i, true, -1f).x;
                Assert.Greater(forward, neutral, $"slot {i}: +bias must advance toward the right goal");
                Assert.Less(back, neutral, $"slot {i}: -bias must retreat");

                // Mirrored team: the same bias advances toward the LEFT goal instead.
                float mirroredForward = PitchLayout.FormationSlot(i, false, 1f).x;
                Assert.Less(mirroredForward, PitchLayout.FormationSlot(i, false, 0f).x);
            }

            // Extreme bias never escapes the pitch margins.
            for (int i = 0; i < PitchLayout.OutfieldPerTeam; i++)
            {
                Assert.LessOrEqual(PitchLayout.FormationSlot(i, true, 5f).x, 0.94f);
                Assert.GreaterOrEqual(PitchLayout.FormationSlot(i, true, -5f).x, 0.06f);
            }
        }

        [Test]
        public void Territory_is_monotonic_and_midfield_at_even_money()
        {
            Assert.AreEqual(0.5f, PitchLayout.TerritoryX(0.5f), 1e-5f, "even money = midfield");
            Assert.AreEqual(0.22f, PitchLayout.TerritoryX(0f), 1e-5f);
            Assert.AreEqual(0.78f, PitchLayout.TerritoryX(1f), 1e-5f);

            float prev = float.NegativeInfinity;
            for (float p = 0f; p <= 1.001f; p += 0.1f)
            {
                float t = PitchLayout.TerritoryX(p);
                Assert.Greater(t, prev, $"territory must rise with prob (p={p:0.0})");
                prev = t;
            }

            // Out-of-range probabilities clamp instead of escaping the pitch.
            Assert.AreEqual(PitchLayout.TerritoryX(0f), PitchLayout.TerritoryX(-2f), 1e-5f);
            Assert.AreEqual(PitchLayout.TerritoryX(1f), PitchLayout.TerritoryX(3f), 1e-5f);
        }
    }
}
