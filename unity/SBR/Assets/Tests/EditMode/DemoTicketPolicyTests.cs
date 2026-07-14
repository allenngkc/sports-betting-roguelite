using System;
using System.Collections.Generic;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// M3 EditMode: the demo channel's ticket policy is deterministic per seed (same seed and round
    /// yield the same picks and stake), picks the shortest-priced sides, sizes at 25% of bank (min $10),
    /// and produces a ticket the engine actually accepts. Uses the engine API directly - no Unity runtime.
    /// </summary>
    public class DemoTicketPolicyTests
    {
        [Test]
        public void Same_seed_yields_identical_picks_and_stake()
        {
            var a = new Run("POLICY-SEED-42");
            var b = new Run("POLICY-SEED-42");

            (IReadOnlyList<Pick> pa, double sa) = DemoTicketPolicy.Choose(a);
            (IReadOnlyList<Pick> pb, double sb) = DemoTicketPolicy.Choose(b);

            Assert.AreEqual(sa, sb, 1e-9, "stake differs for the same seed");
            Assert.AreEqual(pa.Count, pb.Count, "leg count differs for the same seed");
            for (int i = 0; i < pa.Count; i++)
            {
                Assert.AreEqual(pa[i].MatchupIndex, pb[i].MatchupIndex, $"pick {i} matchup differs");
                Assert.AreEqual(pa[i].Side, pb[i].Side, $"pick {i} side differs");
            }
        }

        [Test]
        public void Choose_is_pure_calling_twice_on_one_run_matches_and_consumes_no_rng()
        {
            var run = new Run("POLICY-PURE");
            (IReadOnlyList<Pick> p1, double s1) = DemoTicketPolicy.Choose(run);
            (IReadOnlyList<Pick> p2, double s2) = DemoTicketPolicy.Choose(run);

            Assert.AreEqual(s1, s2, 1e-9);
            Assert.AreEqual(p1.Count, p2.Count);
            for (int i = 0; i < p1.Count; i++)
            {
                Assert.AreEqual(p1[i].MatchupIndex, p2[i].MatchupIndex);
                Assert.AreEqual(p1[i].Side, p2[i].Side);
            }

            // Consuming no RNG means the run still locks/settles to the same result as an untouched run.
            var control = new Run("POLICY-PURE");
            run.LockRound();
            control.LockRound();
            Assert.AreEqual(control.CurrentSlate.Matchups[0].Result, run.CurrentSlate.Matchups[0].Result);
        }

        [Test]
        public void Picks_are_the_shortest_priced_sides_two_or_three_distinct_legs()
        {
            var run = new Run("POLICY-SHORT");
            (IReadOnlyList<Pick> picks, double _) = DemoTicketPolicy.Choose(run);

            Assert.That(picks.Count, Is.InRange(2, 3), "demo tickets are 2-3 legs");

            var seen = new HashSet<int>();
            foreach (Pick p in picks)
                Assert.IsTrue(seen.Add(p.MatchupIndex), "a ticket cannot repeat a matchup");

            // Each pick is the shorter-priced side of its matchup.
            foreach (Pick p in picks)
            {
                Matchup m = run.CurrentSlate.Matchups[p.MatchupIndex];
                Side shorter = m.HomeOdds <= m.AwayOdds ? Side.Home : Side.Away;
                Assert.AreEqual(shorter, p.Side, $"matchup {p.MatchupIndex} pick is not the favourite");
            }

            // The chosen matchups are the N with the shortest favourite prices on the slate.
            var favByMatchup = new List<(int idx, double fav)>();
            for (int i = 0; i < run.CurrentSlate.Matchups.Count; i++)
            {
                Matchup m = run.CurrentSlate.Matchups[i];
                favByMatchup.Add((i, Math.Min(m.HomeOdds, m.AwayOdds)));
            }
            favByMatchup.Sort((x, y) =>
            {
                int c = x.fav.CompareTo(y.fav);
                return c != 0 ? c : x.idx.CompareTo(y.idx);
            });
            for (int i = 0; i < picks.Count; i++)
                Assert.AreEqual(favByMatchup[i].idx, picks[i].MatchupIndex,
                    $"pick {i} is not the {i}-th shortest-priced matchup");
        }

        [Test]
        public void Stake_is_quarter_bank_min_ten_and_the_ticket_is_engine_valid()
        {
            var run = new Run("POLICY-STAKE", new RunConfig { StartingBank = 500 }); // 0.25*500 = 125
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);

            double expected = Math.Max(run.Config.MinStake, Math.Floor(0.25 * run.Bank));
            Assert.AreEqual(expected, stake, 1e-9);
            Assert.AreEqual(125.0, stake, 1e-9);

            // The engine accepts the policy's ticket (valid legs, stake within bounds).
            Assert.DoesNotThrow(() => run.PlaceTicket(picks, stake));
            Assert.AreEqual(1, run.Tickets.Count);
        }
    }
}
