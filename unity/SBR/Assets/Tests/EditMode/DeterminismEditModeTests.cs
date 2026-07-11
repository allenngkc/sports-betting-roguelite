using System.Collections.Generic;
using NUnit.Framework;
using SBR.Engine;
using SBR.Probe;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// M1, Phase 2 — proves SBR.Engine.dll reproduces the Week 2 determinism pins inside Unity's
    /// (Mono) editor runtime. Ported from engine.tests/GoldenSeedTests.cs (the golden event stream,
    /// first-ten win probabilities, and settled bank) plus one engine.tests/DebtTests.cs case so the
    /// newest engine surface (debt-as-HP) is exercised through the DLL in Unity too. The pin data lives
    /// once in <see cref="GoldenReplay"/>; these tests assert the DLL's actual values against it.
    /// </summary>
    public class DeterminismEditModeTests
    {
        [Test]
        public void Golden_seed_event_stream_is_pinned()
        {
            Run run = GoldenReplay.ScriptedRound();
            List<DramaEvent> events = GoldenReplay.DrainAll(run);

            Assert.AreEqual(47, events.Count, "expected exactly 47 events");
            Assert.AreEqual(GoldenReplay.ExpectedEvents.Length, events.Count);

            for (int i = 0; i < events.Count; i++)
            {
                DramaEvent e = events[i];
                var x = GoldenReplay.ExpectedEvents[i];
                Assert.AreEqual(x.leg, e.LegIndex, $"LegIndex @ event {i}");
                Assert.AreEqual(x.step, e.Step, $"Step @ event {i}");
                Assert.AreEqual(x.type, e.Type, $"Type @ event {i}");
                Assert.AreEqual(x.tag, e.Tag, $"Tag @ event {i}");
            }

            // 6 dp, matching engine.tests' Assert.Equal(expected, actual, 6).
            for (int i = 0; i < GoldenReplay.ExpectedFirstTenWinProb.Length; i++)
                Assert.AreEqual(GoldenReplay.ExpectedFirstTenWinProb[i], events[i].WinProbAfter, 1e-6,
                    $"WinProbAfter @ event {i}");
        }

        [Test]
        public void Golden_seed_settles_to_pinned_bank_and_phase()
        {
            Run run = GoldenReplay.ScriptedRound();
            GoldenReplay.DrainAll(run);
            run.FinishSweat();

            Assert.AreEqual(Phase.Settlement, run.Phase);
            Assert.AreEqual(TicketState.Lost, run.Tickets[0].State); // parlay died on its final leg
            Assert.AreEqual(TicketState.Won, run.Tickets[1].State);  // single hit
            Assert.AreEqual(GoldenReplay.ExpectedBank, run.Bank, 1e-5); // 5 dp
        }

        [Test]
        public void Golden_seed_replays_identically()
        {
            List<DramaEvent> a = GoldenReplay.DrainAll(GoldenReplay.ScriptedRound());
            List<DramaEvent> b = GoldenReplay.DrainAll(GoldenReplay.ScriptedRound());

            Assert.AreEqual(a.Count, b.Count);
            for (int i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a[i].LegIndex, b[i].LegIndex);
                Assert.AreEqual(a[i].Step, b[i].Step);
                Assert.AreEqual(a[i].Type, b[i].Type);
                Assert.AreEqual(a[i].Tag, b[i].Tag);
                Assert.AreEqual(a[i].WinProbAfter, b[i].WinProbAfter, 1e-15);
            }
        }

        [Test]
        public void Probe_verify_passes_and_reports_event_stream_hash()
        {
            GoldenReplay.Result r = GoldenReplay.Verify();
            Assert.IsTrue(r.Pass, r.Detail);
            Assert.IsNotNull(r.Hash);
            // Surfaces the golden fingerprint in the test log for cross-platform comparison.
            TestContext.WriteLine($"[EditMode/Mono] event-stream FNV-1a hash = {r.Hash}");
        }

        // Ported from engine.tests/DebtTests.cs (A_clean_miss_borrows...): a no-bet miss with no debt
        // makes the bookie float you — bank topped to the target, shortfall booked at x(1 + 0.5).
        [Test]
        public void Debt_clean_miss_borrows_bank_topped_to_target_debt_is_juiced_shortfall()
        {
            var run = new Run("DEBT-BORROW", new RunConfig
            {
                StartingBank = 500,
                Targets = new double[] { 800, 1200 },
                DebtJuiceRate = 0.5,
            });
            run.LockRound();
            run.FastForwardRound();
            run.Settle();

            Assert.AreEqual(Phase.Shop, run.Phase);                     // the run continues
            Assert.AreEqual(800.0, run.Bank, 1e-9);                     // working capital up to the target
            Assert.AreEqual((800.0 - 500.0) * 1.5, run.Debt, 1e-9);     // shortfall x (1 + 0.5)
            Assert.Greater(run.ShopOffers.Count, 0);                    // the float lands in a real shop
        }
    }
}
