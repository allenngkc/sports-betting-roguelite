using NUnit.Framework;
using SBR.Engine;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// The match theater's model laws (F_0.2.0 M-T2): team colors are deterministic, come
    /// from the non-reserved pool (palette law — green/red/gold/cyan are signals, never
    /// identity), home and away never collide; the beat direction rule matches EventText's
    /// (first beat against the leg's TrueProb anchor).
    /// </summary>
    public class SweatPresentationModelTests
    {
        // The reserved signal colors (design/08) + the VOID cyan, as 0xRRGGBB.
        private static readonly uint[] Reserved = { 0x33FF66, 0xFF4038, 0xFFD12E, 0x9EDCF6 };

        [Test]
        public void Team_pool_excludes_every_reserved_signal_color()
        {
            foreach (uint c in TheaterPalette.TeamPool)
            {
                CollectionAssert.DoesNotContain(Reserved, c);
                // No pool entry may even READ as money-green or money-red: reject dominant
                // pure-red and pure-green hues outright.
                int r = (int)((c >> 16) & 0xFF), g = (int)((c >> 8) & 0xFF), b = (int)(c & 0xFF);
                Assert.False(r > 180 && g < 90 && b < 90, $"pool color {c:X6} reads as reserved red");
                Assert.False(g > 180 && r < 90 && b < 110, $"pool color {c:X6} reads as reserved green");
            }
        }

        [Test]
        public void Team_colors_are_deterministic_and_distinct()
        {
            string[] names =
            {
                "Rustwater Lions", "Port Vane Sharks", "Gilt City Royals", "Fog Hollow Owls",
                "Bright Bay Comets", "Iron Bend Mules", "Sallow Creek Kings", "Dune Point Rays",
            };
            foreach (string home in names)
                foreach (string away in names)
                {
                    if (home == away) continue;
                    (uint h1, uint a1) = TheaterPalette.TeamColors(home, away);
                    (uint h2, uint a2) = TheaterPalette.TeamColors(home, away);
                    Assert.AreEqual(h1, h2, "home color must be stable across calls");
                    Assert.AreEqual(a1, a2, "away color must be stable across calls");
                    Assert.AreNotEqual(h1, a1, $"{home} vs {away} collided on one color");
                    CollectionAssert.Contains(TheaterPalette.TeamPool, h1);
                    CollectionAssert.Contains(TheaterPalette.TeamPool, a1);
                }
        }

        [Test]
        public void A_team_keeps_its_color_regardless_of_opponent()
        {
            (uint h1, _) = TheaterPalette.TeamColors("Rustwater Lions", "Port Vane Sharks");
            (uint h2, _) = TheaterPalette.TeamColors("Rustwater Lions", "Fog Hollow Owls");
            Assert.AreEqual(h1, h2, "the home team's color must depend only on its own name");
        }

        [Test]
        public void Beat_direction_uses_the_true_prob_anchor_per_leg()
        {
            // A real locked leg from the engine (no engine RNG consumed by the model itself).
            var run = new Run("THEATER-DIR", new RunConfig());
            Ticket ticket = run.PlaceTicket(new[] { new Pick(0, Side.Home) }, 50);
            run.LockRound();
            Leg leg = ticket.Legs[0];

            var model = new SweatPresentationModel();
            double anchor = leg.TrueProb;

            var upEvt = new DramaEvent(0, 1, 4, DramaEventType.Score, anchor + 0.05, TensionTag.Swing);
            Assert.IsTrue(model.RecordBeat(upEvt, leg), "a move above the TrueProb anchor is up");

            var downEvt = new DramaEvent(0, 2, 4, DramaEventType.Score, anchor - 0.10, TensionTag.Swing);
            Assert.IsFalse(model.RecordBeat(downEvt, leg), "a move below the previous beat is down");

            Assert.AreEqual(2, model.Beats.Count);
            Assert.IsTrue(model.Beats[0].Up);
            Assert.IsFalse(model.Beats[1].Up);
        }

        [Test]
        public void Reset_clears_history_and_re_anchors()
        {
            var run = new Run("THEATER-RESET", new RunConfig());
            Ticket ticket = run.PlaceTicket(new[] { new Pick(0, Side.Home) }, 50);
            run.LockRound();
            Leg leg = ticket.Legs[0];

            var model = new SweatPresentationModel();
            model.RecordBeat(new DramaEvent(0, 1, 4, DramaEventType.Momentum, 0.9, TensionTag.Calm), leg);
            model.ResetForTicket();
            Assert.AreEqual(0, model.Beats.Count);

            // After reset the anchor is TrueProb again, not the stale 0.9.
            bool up = model.RecordBeat(
                new DramaEvent(0, 1, 4, DramaEventType.Momentum, leg.TrueProb + 0.01, TensionTag.Calm), leg);
            Assert.IsTrue(up);
        }
    }
}
