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

        /// <summary>Runs every session to completion. Bounded, and it ASSERTS the drain finished —
        /// an unbounded `while (!IsComplete)` would hang the suite, and a silently-unfinished drain
        /// would make both T61 tests below pass while proving nothing about the state they claim to
        /// have reached.</summary>
        private static void DrainAllSessions(Run run)
        {
            foreach (SweatSession s in run.Sweats)
            {
                int guard = 0;
                while (!s.IsComplete && guard++ < 10000) s.MoveNext(out _);
                Assert.IsTrue(s.IsComplete,
                    "a session did not drain within 10000 steps — the state this test reasons about "
                    + "was never reached, so its assertions would be vacuous");
            }
        }

        /// <summary>T61 — the contract the TV capture harness polls on, pinned at the point this
        /// worktree depends on it. Engine-side proof lives in markets' <c>SweatPollingContractTests</c>;
        /// this is the TV-side consumer test, taken deliberately INSTEAD of trusting a green re-run.
        ///
        /// <para>Why a re-run could not have served as evidence: whether ticket 0 is terminal mid-sweat
        /// depends on the ticket's OUTCOME, not its position. A ticket that dies goes Lost immediately;
        /// one that survives stays Open until FinishSweat. So a poller keyed on the ticket gets an
        /// early false "done" on a losing seed and no signal at all on a winning one — from identical
        /// code. A seed that happens to lose would have made the harness look fixed while the defect
        /// sat untouched. <b>The defect is seed-decided, so only a contract can settle it.</b></para></summary>
        [Test]
        public void T61_sweat_completion_is_a_phase_property_not_a_ticket_property()
        {
            var run = new Run("T61-CONTRACT", new RunConfig());
            Ticket ticket = run.PlaceTicket(new[] { new Pick(0, Side.Home) }, 50);
            run.LockRound();

            Assert.AreEqual(Phase.Sweat, run.Phase, "precondition: the run is sweating");
            Assert.AreEqual(TicketState.Open, ticket.State, "precondition: the ticket starts Open");

            // Drain every session. This is the state the old predicate treated as "complete".
            DrainAllSessions(run);

            // THE POINT. Every session is drained, yet the phase has NOT moved — so a harness that
            // stopped here would stop mid-sweat. And the ticket's own state is decided by its
            // OUTCOME, which is why polling it is seed-dependent rather than wrong-every-time.
            Assert.AreEqual(Phase.Sweat, run.Phase,
                "T61: draining the sessions does not end the sweat — Phase leaves Sweat exactly once, "
                + "at FinishSweat. A poller that stops on session completion stops too early.");

            run.FinishSweat();

            Assert.AreNotEqual(Phase.Sweat, run.Phase,
                "T61: FinishSweat is what moves the phase, and the phase is the completion signal");
            Assert.AreNotEqual(TicketState.Open, ticket.State,
                "a ticket resolves by the time the sweat is over, whatever its outcome");
        }

        /// <summary>T61's second half — a captured <c>Tickets[0]</c> goes stale across a round advance.
        /// This is the shape that reads as "a settled ticket looks like a hang": the held reference is
        /// permanently terminal while <c>Tickets[0]</c> now names a different, open ticket.</summary>
        [Test]
        public void T61_a_captured_ticket_reference_goes_stale_across_a_round()
        {
            var run = new Run("T61-STALE", new RunConfig());
            Ticket roundOne = run.PlaceTicket(new[] { new Pick(0, Side.Home) }, 50);
            run.LockRound();
            Assert.AreSame(roundOne, run.Tickets[0], "precondition: the capture names round one's ticket");

            DrainAllSessions(run);
            run.FinishSweat();

            Assert.AreNotEqual(TicketState.Open, roundOne.State,
                "the captured ticket is terminal and can never change again");
            Assert.AreNotEqual(Phase.Sweat, run.Phase,
                "and the phase — the real signal — has moved, which is what a poller should read");
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
            model.ResetForTicket(anchor);
            // ON A ONE-LEG TICKET THE TICKET'S PROBABILITY IS THE LEG'S (T164 says so in terms:
            // "a one-leg ticket's win probability IS that leg's probability"), so these fixtures
            // assert exactly what they always asserted — only the referent is named honestly now.
            Assert.IsTrue(model.RecordBeat(upEvt, upEvt.WinProbAfter), "a move above the ticket anchor is up");

            var downEvt = new DramaEvent(0, 2, 4, DramaEventType.Score, anchor - 0.10, TensionTag.Swing);
            Assert.IsFalse(model.RecordBeat(downEvt, downEvt.WinProbAfter), "a move below the previous beat is down");

            Assert.AreEqual(2, model.Beats.Count);
            Assert.IsTrue(model.Beats[0].Up);
            Assert.IsFalse(model.Beats[1].Up);
        }

        [Test]
        public void Magnitude_bands_partition_the_delta_space()
        {
            Assert.AreEqual(0, SweatPresentationModel.MagnitudeBand(0.0));
            Assert.AreEqual(0, SweatPresentationModel.MagnitudeBand(0.039));
            Assert.AreEqual(0, SweatPresentationModel.MagnitudeBand(-0.039), "bands use |delta|");
            Assert.AreEqual(1, SweatPresentationModel.MagnitudeBand(0.04));
            Assert.AreEqual(1, SweatPresentationModel.MagnitudeBand(-0.099));
            Assert.AreEqual(2, SweatPresentationModel.MagnitudeBand(0.10));
            Assert.AreEqual(2, SweatPresentationModel.MagnitudeBand(-0.5));
        }

        [Test]
        public void Record_beat_stores_the_signed_delta_from_the_anchor()
        {
            var run = new Run("THEATER-DELTA", new RunConfig());
            Ticket ticket = run.PlaceTicket(new[] { new Pick(0, Side.Home) }, 50);
            run.LockRound();
            Leg leg = ticket.Legs[0];

            var model = new SweatPresentationModel();
            double anchor = leg.TrueProb;
            model.ResetForTicket(anchor);
            model.RecordBeat(new DramaEvent(0, 1, 4, DramaEventType.Score, anchor + 0.08, TensionTag.Swing), anchor + 0.08);
            model.RecordBeat(new DramaEvent(0, 2, 4, DramaEventType.Score, anchor - 0.02, TensionTag.Swing), anchor - 0.02);

            Assert.AreEqual(0.08, model.Beats[0].Delta, 1e-9, "first beat measures from the ticket anchor");
            Assert.AreEqual(-0.10, model.Beats[1].Delta, 1e-9, "later beats measure from the previous beat");
        }

        [Test]
        public void Reset_clears_history_and_re_anchors()
        {
            var run = new Run("THEATER-RESET", new RunConfig());
            Ticket ticket = run.PlaceTicket(new[] { new Pick(0, Side.Home) }, 50);
            run.LockRound();
            Leg leg = ticket.Legs[0];

            var model = new SweatPresentationModel();
            model.ResetForTicket(leg.TrueProb);
            model.RecordBeat(new DramaEvent(0, 1, 4, DramaEventType.Momentum, 0.9, TensionTag.Calm), 0.9);
            model.ResetForTicket(leg.TrueProb);
            Assert.AreEqual(0, model.Beats.Count);

            // After reset the anchor is the seed again, not the stale 0.9.
            bool up = model.RecordBeat(
                new DramaEvent(0, 1, 4, DramaEventType.Momentum, leg.TrueProb + 0.01, TensionTag.Calm),
                leg.TrueProb + 0.01);
            Assert.IsTrue(up);
        }
    }
}
