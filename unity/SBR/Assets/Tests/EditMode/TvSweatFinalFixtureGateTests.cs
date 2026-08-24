using System.Collections.Generic;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// THE TV's FINAL-TELLING PREDICATE — <c>T140</c> arm A, the twin of the console gate in
    /// <c>SweatFinalFixtureGateTests</c>.
    ///
    /// <para><b>Scope, stated up front so this green is not read as larger than it is.</b> The TV's
    /// <c>onFinalLeg</c> feeds ONLY <c>PacingFor</c>'s final-telling slowdown, and it is reached only
    /// on the theaterless fallback path — <c>PlaySweat</c> hands the shipping theater path to
    /// <c>TheaterBeat</c>, which owns its own pacing and never calls <c>PacingFor</c>. The console's
    /// twin gates a stated RULE (no fast-forward through the final match); this one does not. It is
    /// still a defect: on an interleaved ticket the closing telling paces like any other beat.</para>
    ///
    /// <para><b>The mutation is encoded, not described.</b> <see cref="StruckPredicate"/> is the line
    /// this replaced, kept executable, and the gate asserts it FAILS on the ticket the new one
    /// handles. A gate that only checked the fix would pass against the defect on every ordinary
    /// ticket, where the two agree — which is exactly how this survived.</para>
    ///
    /// <para>Asserted rather than driven: the value is computed inside a coroutine that sleeps, waits
    /// on seating and plays scenes, so reaching it through the loop is not something an EditMode test
    /// can do. That is why the predicate is public.</para>
    /// </summary>
    public class TvSweatFinalFixtureGateTests
    {
        /// <summary>The struck line, verbatim: <c>evt.LegIndex == _ticket.Legs.Count - 1</c>.</summary>
        private static bool StruckPredicate(DramaEvent e, Ticket ticket)
            => e.LegIndex == ticket.Legs.Count - 1;

        private sealed class Drive
        {
            public Ticket Ticket;
            public int FixtureCount;
            public int Beats;
            public List<int> Anchors = new List<int>();
            public int NewTrue;
            public int OldTrue;
            public int LastFixtureSeen = -1;
        }

        /// <summary>An interleaved <c>[matchA, matchB, matchA]</c> ticket <b>that survives its first
        /// telling</b>, searched off the board.
        ///
        /// <para>Surviving matters and is not a detail: the console's twin gate found its first
        /// candidate busting on the opening telling — the sweat ended with the middle leg's match
        /// never told — and on such a ticket NEITHER predicate fires, so the two cannot be told
        /// apart. A ticket has to reach its last telling for a final-telling rule to mean
        /// anything.</para></summary>
        private static Drive Find(params string[] seeds)
        {
            foreach (string seed in seeds)
            {
                int matchups = new Run(seed, new RunConfig()).CurrentSlate.Matchups.Count;
                for (int a = 0; a < matchups; a++)
                    for (int b = 0; b < matchups; b++)
                    {
                        if (a == b) continue;
                        var run = new Run(seed, new RunConfig());
                        RunConfig cfg = run.Config;
                        var picks = new[]
                        {
                            new Pick(a, MarketSelection.TotalGoals(cfg.GoalLines[1], true)),
                            new Pick(b, MarketSelection.Moneyline(Side.Home)),
                            new Pick(a, MarketSelection.TotalGoals(cfg.GoalLines[0], true)),
                        };
                        if (run.RefusalFor(picks) != null) continue;

                        Ticket t = run.PlaceTicket(picks, 10);
                        run.LockRound();
                        if (t.Legs.Count != 3) continue;
                        if (!ReferenceEquals(t.Legs[0].Matchup, t.Legs[2].Matchup)) continue;
                        if (ReferenceEquals(t.Legs[0].Matchup, t.Legs[1].Matchup)) continue;

                        SweatSession session = run.Sweats[0];
                        if (session.FixtureCount >= t.Legs.Count) continue;

                        var d = new Drive { Ticket = t, FixtureCount = session.FixtureCount };
                        while (session.MoveNext(out DramaEvent e))
                        {
                            if (e == null) break;
                            d.Beats++;
                            if (!d.Anchors.Contains(e.LegIndex)) d.Anchors.Add(e.LegIndex);
                            if (e.FixtureIndex > d.LastFixtureSeen) d.LastFixtureSeen = e.FixtureIndex;
                            if (TvSweatScreen.OnFinalFixture(e, session)) d.NewTrue++;
                            if (StruckPredicate(e, t)) d.OldTrue++;
                            if (session.HasPendingLoss) session.DeclinePendingLoss();
                        }

                        if (d.LastFixtureSeen != d.FixtureCount - 1) continue;
                        return d;
                    }
            }

            return null;
        }

        [Test]
        public void The_final_telling_is_reached_on_an_interleaved_ticket_and_the_struck_predicate_never_was()
        {
            Drive d = Find("TV-FINALFIX-A", "TV-FINALFIX-B", "TV-FINALFIX-C", "TV-FINALFIX-D");
            Assert.IsNotNull(d,
                "no interleaved [A,B,A] ticket on these seeds survived its first telling. The case "
                + "T140 arm A creates is then unreachable from this pool and this gate would be "
                + "vacuous — widen the seeds rather than relax the assertions");

            UnityEngine.Debug.Log($"[TV-FINALFIX] legs {d.Ticket.Legs.Count} fixtures {d.FixtureCount} "
                + $"beats {d.Beats} anchors [{string.Join(", ", d.Anchors)}] "
                + $"new true {d.NewTrue} struck true {d.OldTrue}");

            Assert.Less(d.FixtureCount, d.Ticket.Legs.Count,
                $"FixtureCount {d.FixtureCount} is not below Legs.Count {d.Ticket.Legs.Count} — this "
                + "ticket has one leg per fixture and cannot distinguish the two predicates");
            Assert.Greater(d.Beats, 0, "C29: no beat was put through either predicate");

            // THE MUTATION. The last leg index is 2, and 2 is never an anchor because leg 2 shares
            // fixture 0 with leg 0 and the anchor is the LOWEST ticket-order leg on the fixture.
            Assert.IsFalse(d.Anchors.Contains(d.Ticket.Legs.Count - 1),
                $"the last leg index {d.Ticket.Legs.Count - 1} appeared as an anchor, so the struck "
                + "predicate is reachable here and this gate is not measuring the defect");
            Assert.AreEqual(0, d.OldTrue,
                $"the struck predicate fired on {d.OldTrue} beats — it was expected to be UNREACHABLE "
                + "on an interleaved ticket, which IS the defect. If it now fires, arm A's fixture "
                + "grouping has changed and this gate needs re-deriving, not relaxing");

            // AND THE FIX.
            Assert.Greater(d.NewTrue, 0,
                "OnFinalFixture never went true, so PacingFor never applies finalLegMultiplier and "
                + "the ticket's closing telling paces like any other beat");
            Assert.AreEqual(d.FixtureCount - 1, d.LastFixtureSeen);
        }

        /// <summary>The compatibility half: on an ordinary ticket the two predicates agree beat for
        /// beat, so the fix moves nothing on the shape that ships today.
        ///
        /// <para><b>AND THE AGREEMENT MUST BE NON-TRIVIAL, which this test did not require until a
        /// mutation run caught it.</b> The first version took the first two-leg ticket it could
        /// build. That ticket BUSTED ON LEG 0, so the sweat never reached leg 1, so neither predicate
        /// ever fired — and "agreement" was 4 beats of false == false. An off-by-one mutant on
        /// <c>OnFinalFixture</c> PASSED this test, which is the proof it was measuring nothing.</para>
        ///
        /// <para>So the ticket must reach its final leg AND the predicates must actually go true
        /// there. A comparison where neither side ever fires compares nothing.</para></summary>
        [Test]
        public void On_an_ordinary_ticket_the_new_predicate_matches_the_struck_one_beat_for_beat()
        {
            Ticket ticket = null;
            var trues = 0;
            int beats = 0, agreed = 0;

            foreach (string seed in new[]
                { "TV-FINALFIX-ORD", "TV-FINALFIX-ORD2", "TV-FINALFIX-ORD3", "TV-FINALFIX-ORD4" })
            {
                int matchups = new Run(seed, new RunConfig()).CurrentSlate.Matchups.Count;
                for (int a = 0; a < matchups && ticket == null; a++)
                    for (int b = 0; b < matchups; b++)
                    {
                        if (a == b) continue;
                        var run = new Run(seed, new RunConfig());
                        var picks = new[]
                        {
                            new Pick(a, MarketSelection.Moneyline(Side.Home)),
                            new Pick(b, MarketSelection.Moneyline(Side.Home)),
                        };
                        if (run.RefusalFor(picks) != null) continue;

                        Ticket t = run.PlaceTicket(picks, 10);
                        run.LockRound();
                        SweatSession session = run.Sweats[0];
                        if (session.FixtureCount != t.Legs.Count) continue; // must be the ordinary shape

                        int bb = 0, ag = 0, tr = 0;
                        while (session.MoveNext(out DramaEvent e))
                        {
                            if (e == null) break;
                            bb++;
                            bool now = TvSweatScreen.OnFinalFixture(e, session);
                            if (now) tr++;
                            if (now == StruckPredicate(e, t)) ag++;
                            if (session.HasPendingLoss) session.DeclinePendingLoss();
                        }

                        // Reached its last leg and the predicate actually fired there — otherwise the
                        // comparison below is false == false and proves nothing.
                        if (tr == 0) continue;
                        ticket = t; beats = bb; agreed = ag; trues = tr;
                        break;
                    }
                if (ticket != null) break;
            }

            Assert.IsNotNull(ticket,
                "no ordinary two-leg ticket on these seeds survived to its final leg, so the "
                + "predicate never fired and this comparison would be vacuous — widen the seeds "
                + "rather than drop the non-triviality requirement");

            UnityEngine.Debug.Log($"[TV-FINALFIX] ordinary: {beats} beats, {agreed} in agreement, "
                + $"predicate true on {trues}");
            Assert.Greater(beats, 0, "C29: no beat was compared");
            Assert.Greater(trues, 0,
                "the predicate never went true, so agreement is false == false on every beat and "
                + "an off-by-one mutant would pass this test — it did, once");
            Assert.AreEqual(beats, agreed);
        }
    }
}
