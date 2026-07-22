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

            Assert.AreEqual(14, events.Count, "expected exactly 14 events (F_0.4.0 Phase 1 re-pin)");
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
            Assert.AreEqual(TicketState.Lost, run.Tickets[0].State); // parlay died on its second leg
            Assert.AreEqual(TicketState.Won, run.Tickets[1].State);  // single hit — payout crediting stays pinned
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

        [Test]
        public void Payment_shortfall_with_the_totem_defers_and_surcharges_the_next_payment()
        {
            // The economy rework's mercy path (design/10, playtest #8): the Totem DEFERS a
            // non-final payment — the bank is untouched (working capital survives) and the
            // NEXT payment grows by payment × (1 + juice).
            var run = new Run("PAY-TOTEM", new RunConfig
            {
                StartingBank = 500,
                Payments = new double[] { 800, 400 },
                TotemJuiceRate = 0.5,
            });
            foreach (RelicDefinition def in RelicCatalog.All)
                if (def.Id == RelicCatalog.TotemId) run.GrantRelic(def);
            run.LockRound();
            run.FastForwardRound();
            run.Settle();

            Assert.AreEqual(Phase.Shop, run.Phase);                       // the run continues
            Assert.AreEqual(500.0, run.Bank, 1e-9);                       // the bank stands
            Assert.IsTrue(run.LastSettlement!.Value.TotemFired);
            run.ExitShop();
            Assert.AreEqual(400.0 + 800.0 * 1.5, run.CurrentPayment, 1e-9); // payment × 1.5 lands on P2
        }
    }
}
