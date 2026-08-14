using System.Collections.Generic;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// The creditor-model bookie triggers (economy rework): demands stamped with the live round,
    /// settle beats with the report's round, dedup across repeated snapshots, monotone counters.
    /// Synthetic engine SettlementReports script the settle edges; the gift rides a real run
    /// (engine-internal state). Default schedule: [60,70,85,105,195,375,710,1350], bank 350.
    /// </summary>
    public class BookieFeedModelTests
    {
        private static SettlementReport Report(int round, double payment, double bankBefore,
            double bankAfter, double shortfall, bool totem, Phase outcome)
            => new SettlementReport(round, payment, bankBefore, bankAfter, shortfall, totem, outcome);

        private static void ObserveThree(BookieFeedModel m, int gen, Run run, Phase phase, int round,
            SettlementReport? report)
        {
            for (int i = 0; i < 3; i++)
                m.Observe(gen, run, phase, round, report);
        }

        private static void AssertKinds(BookieFeedModel m, params BookieMessageKind[] expected)
        {
            Assert.AreEqual(expected.Length, m.Messages.Count);
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], m.Messages[i].Kind, $"message {i}");
        }

        [Test]
        public void Welcome_carries_the_first_payment_and_is_idempotent()
        {
            var run = new Run("CRED-WELCOME");
            var m = new BookieFeedModel();
            ObserveThree(m, 1, run, Phase.Betting, 1, null);

            AssertKinds(m, BookieMessageKind.RUN_START);
            StringAssert.Contains("$60", m.Messages[0].Text);
        }

        [Test]
        public void Cliff_demand_fires_only_on_jump_rounds()
        {
            var run = new Run("CRED-CLIFF");
            var m = new BookieFeedModel();
            m.Observe(1, run, Phase.Betting, 1, null);

            // Round 2: 70 vs 60 — no jump. (Round is synthetic; the model reads the schedule.)
            ObserveThree(m, 1, run, Phase.Betting, 2, null);
            AssertKinds(m, BookieMessageKind.RUN_START);

            // Round 5: 195 vs 105 — the cliff (≥1.5×).
            ObserveThree(m, 1, run, Phase.Betting, 5, null);
            AssertKinds(m, BookieMessageKind.RUN_START, BookieMessageKind.CLIFF_DEMAND);
            Assert.AreEqual(5, m.Messages[1].Round);
        }

        [Test]
        public void Final_round_gets_the_final_demand_not_the_cliff_text()
        {
            var run = new Run("CRED-FINAL");
            var m = new BookieFeedModel();
            m.Observe(1, run, Phase.Betting, 1, null);
            ObserveThree(m, 1, run, Phase.Betting, run.Config.Rounds, null);

            AssertKinds(m, BookieMessageKind.RUN_START, BookieMessageKind.FINAL_DEMAND);
        }

        [Test]
        public void Settle_beats_totem_close_call_collection_and_quiet_paid()
        {
            var run = new Run("CRED-SETTLE");
            var m = new BookieFeedModel();
            m.Observe(1, run, Phase.Betting, 1, null);

            // Comfortable payment: silent.
            ObserveThree(m, 1, run, Phase.Shop, 1, Report(1, 60, 350, 290, 0, false, Phase.Shop));
            AssertKinds(m, BookieMessageKind.RUN_START);

            // Close call: paid with under 20% of the payment left.
            ObserveThree(m, 1, run, Phase.Shop, 2, Report(2, 70, 80, 10, 0, false, Phase.Shop));
            AssertKinds(m, BookieMessageKind.RUN_START, BookieMessageKind.CLOSE_CALL_RECEIPT);

            // Totem covers a shortfall.
            ObserveThree(m, 1, run, Phase.Shop, 3, Report(3, 85, 40, 0, 45, true, Phase.Shop));
            AssertKinds(m, BookieMessageKind.RUN_START, BookieMessageKind.CLOSE_CALL_RECEIPT,
                BookieMessageKind.TOTEM_BURNED);

            // Collection ends it.
            ObserveThree(m, 1, run, Phase.RunLost, 4, Report(4, 105, 20, 20, 85, false, Phase.RunLost));
            AssertKinds(m, BookieMessageKind.RUN_START, BookieMessageKind.CLOSE_CALL_RECEIPT,
                BookieMessageKind.TOTEM_BURNED, BookieMessageKind.COLLECTION);
            Assert.AreEqual(4, m.Messages[3].Round);
        }

        [Test]
        public void Delayed_observation_stamps_the_settled_round_not_the_live_one()
        {
            var run = new Run("CRED-DELAY");
            var m = new BookieFeedModel();

            // First snapshot arrives late: run start, a stale totem settle, and round-5 betting
            // (the cliff) all at once — reset, then settle, then demand, in that order.
            SettlementReport stale = Report(4, 105, 40, 0, 65, true, Phase.Shop);
            ObserveThree(m, 1, run, Phase.Betting, 5, stale);

            AssertKinds(m, BookieMessageKind.RUN_START, BookieMessageKind.TOTEM_BURNED,
                BookieMessageKind.CLIFF_DEMAND);
            Assert.AreEqual(4, m.Messages[1].Round); // the settle keeps its own round
            Assert.AreEqual(5, m.Messages[2].Round);
        }

        [Test]
        public void Gift_text_rides_a_real_losing_streak()
        {
            // The engine grants gifts internally; drive a real dutch-book cold streak (the
            // guaranteed-vig-loss script from the engine tests).
            //
            // ALL THREE OUTCOMES ARE BACKED, and the draw ticket is not padding. The script's
            // guarantee was never about the seed — it is about COVERING THE OUTCOME SPACE, so that
            // whatever happens returns ~100 against ~105 staked. Backing home and away alone used
            // to be complete; since the moneyline became 1X2 (F_0.5.0 D1, Allen 2026-08-12) that
            // pair covers only ~0.772 of the implied probability, the stakes fall to ~77, and the
            // "guaranteed loss" quietly turns a ~20 PROFIT — so no cold streak forms and no gift
            // is ever drawn. The engine-side copy of this script was fixed in D1; this one could
            // not be, because Unity asmdef code is invisible to `dotnet build` and only an editor
            // run can see it. That is precisely what this lease surfaced.
            var run = new Run("CRED-GIFT",
                new RunConfig { Payments = new double[] { 10, 10, 10, 10 }, StartingBank = 500 });
            var m = new BookieFeedModel();
            m.Observe(1, run, run.Phase, run.Round, run.LastSettlement);

            for (int i = 0; i < 2; i++)
            {
                Matchup match = run.CurrentSlate.Matchups[0];
                run.PlaceTicket(new[] { new Pick(0, Side.Home) }, System.Math.Floor(100 / match.HomeOdds));
                run.PlaceTicket(new[] { new Pick(0, MarketSelection.MoneylineDraw()) },
                    System.Math.Floor(100 / match.DrawOdds));
                run.PlaceTicket(new[] { new Pick(0, Side.Away) }, System.Math.Floor(100 / match.AwayOdds));
                run.LockRound();
                run.FastForwardRound();
                run.Settle();
                m.Observe(1, run, run.Phase, run.Round, run.LastSettlement);
                run.ExitShop();
                m.Observe(1, run, run.Phase, run.Round, run.LastSettlement);
            }

            Assert.IsNotNull(run.LastGift, "the cold streak should draw the gift");
            bool giftTexted = false;
            foreach (BookieMessage msg in m.Messages)
                if (msg.Kind == BookieMessageKind.GIFT) giftTexted = true;
            Assert.IsTrue(giftTexted, "the gift should arrive as a bookie text");
        }

        [Test]
        public void Reset_is_atomic_and_counters_stay_monotone()
        {
            var m = new BookieFeedModel();
            var first = new Run("CRED-RESET-A");
            m.Observe(1, first, Phase.Shop, 1, Report(1, 60, 350, 290, 0, false, Phase.Shop));
            Assert.AreEqual(1, m.Messages.Count); // welcome only (quiet paid settle)
            Assert.AreEqual(1, m.ArrivalSequence);

            long revBefore = m.Revision;
            m.MarkRead();
            Assert.Greater(m.Revision, revBefore);
            Assert.AreEqual(1, m.ArrivalSequence, "read state is not an arrival");

            long revBeforeReset = m.Revision;
            var second = new Run("CRED-RESET-B");
            m.Observe(2, second, Phase.Betting, 1, null);
            Assert.AreEqual(1, m.Messages.Count, "equal-count welcome after the reset");
            Assert.AreEqual(2, m.ArrivalSequence, "the new welcome arrives exactly once");
            Assert.Greater(m.Revision, revBeforeReset);
            Assert.AreEqual(1, m.UnreadCount);
        }

        [Test]
        public void Lines_are_deterministic_across_instances()
        {
            var runA = new Run("CRED-DET");
            var runB = new Run("CRED-DET");
            var a = new BookieFeedModel();
            var b = new BookieFeedModel();

            a.Observe(1, runA, Phase.Betting, 1, null);
            b.Observe(9, runB, Phase.Betting, 1, null);

            Assert.AreEqual(a.Messages[0].Text, b.Messages[0].Text);
        }
    }
}
