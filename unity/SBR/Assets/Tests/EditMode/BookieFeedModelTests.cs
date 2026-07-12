using System.Collections.Generic;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// M5 EditMode: the bookie's trigger contract, stamped-round history, monotone renderer/buzz
    /// counters, and the design/00 writing selector's strict separation from every engine RNG stream.
    /// Synthetic settle reports keep each narrative edge explicit instead of seed-hunting model tests.
    /// </summary>
    public class BookieFeedModelTests
    {
        [Test]
        public void Welcome_float_clear_and_repeat_float_tiers_are_ordered_and_idempotent()
        {
            var run = new Run("FEED-TIERS");
            var model = new BookieFeedModel();

            ObserveThree(model, 1, run, Phase.Betting, 1, 0, null);
            ObserveThree(model, 1, run, Phase.Shop, 1, 110,
                Report(1, 0, 110, Phase.Shop));
            ObserveThree(model, 1, run, Phase.Shop, 2, 0,
                Report(2, 110, 0, Phase.Shop));
            ObserveThree(model, 1, run, Phase.Shop, 3, 75,
                Report(3, 0, 75, Phase.Shop));

            AssertKinds(model, BookieMessageKind.RUN_START, BookieMessageKind.FLOAT_WARM,
                BookieMessageKind.CLEARED, BookieMessageKind.FLOAT_COLD);
            AssertRounds(model, 1, 1, 2, 3);
            StringAssert.Contains("$110", model.Messages[1].Text);
            StringAssert.Contains("$75", model.Messages[3].Text);
            Assert.AreEqual(4, model.UnreadCount);
        }

        [Test]
        public void Float_then_collection_is_the_indebted_loss_text()
        {
            var run = new Run("FEED-COLLECT");
            var model = new BookieFeedModel();

            ObserveThree(model, 1, run, Phase.Betting, 1, 0, null);
            ObserveThree(model, 1, run, Phase.Shop, 1, 125,
                Report(1, 0, 125, Phase.Shop));
            ObserveThree(model, 1, run, Phase.RunLost, 2, 125,
                Report(2, 125, 125, Phase.RunLost));

            AssertKinds(model, BookieMessageKind.RUN_START, BookieMessageKind.FLOAT_WARM,
                BookieMessageKind.COLLECTION);
            AssertRounds(model, 1, 1, 2);
            StringAssert.Contains("$125", model.Messages[2].Text);
        }

        [Test]
        public void Debt_betting_and_final_round_no_more_favors_are_distinct_once_only_beats()
        {
            var run = new Run("FEED-DEBT");
            var model = new BookieFeedModel();

            ObserveThree(model, 1, run, Phase.Betting, 2, 90, null);
            ObserveThree(model, 1, run, Phase.Betting, run.Config.Rounds, 140, null);

            AssertKinds(model, BookieMessageKind.RUN_START, BookieMessageKind.DEBT_BETTING,
                BookieMessageKind.NO_MORE_FAVORS);
            AssertRounds(model, 2, 2, run.Config.Rounds);
            StringAssert.Contains("$90", model.Messages[1].Text);
            StringAssert.Contains("$140", model.Messages[2].Text);
        }

        [Test]
        public void Clean_verdicts_are_mutually_exclusive_and_idempotent()
        {
            var won = new BookieFeedModel();
            var wonRun = new Run("FEED-WON");
            ObserveThree(won, 1, wonRun, Phase.RunWon, wonRun.Config.Rounds, 0,
                Report(wonRun.Config.Rounds, 0, 0, Phase.RunWon));
            AssertKinds(won, BookieMessageKind.RUN_START, BookieMessageKind.VERDICT_WON);

            var bust = new BookieFeedModel();
            var bustRun = new Run("FEED-BUST");
            ObserveThree(bust, 1, bustRun, Phase.RunLost, 3, 0,
                Report(3, 0, 0, Phase.RunLost));
            AssertKinds(bust, BookieMessageKind.RUN_START, BookieMessageKind.VERDICT_BUST);
            AssertRounds(bust, 3, 3);
        }

        [Test]
        public void Shop_to_betting_keeps_settle_dedup_and_delayed_observation_uses_settled_round()
        {
            var run = new Run("FEED-DELAYED");
            var model = new BookieFeedModel();
            RunDirector.SettleReport floated = Report(1, 0, 110, Phase.Shop);

            // Simulates wiring after ExitShop: reset, stale settle, then live debt reminder all arrive
            // together. The settle's stamped round must not borrow the live round 2.
            ObserveThree(model, 1, run, Phase.Betting, 2, 110, floated);
            AssertKinds(model, BookieMessageKind.RUN_START, BookieMessageKind.FLOAT_WARM,
                BookieMessageKind.DEBT_BETTING);
            AssertRounds(model, 2, 1, 2);

            model.Observe(1, run, Phase.Shop, 2, 110, floated);
            model.Observe(1, run, Phase.Betting, 2, 110, floated);
            Assert.AreEqual(3, model.Messages.Count, "LastSettle persisting across ExitShop must not re-fire");
        }

        [Test]
        public void Final_round_debt_clear_then_win_emits_both_in_that_order_once()
        {
            var run = new Run("FEED-CLEAR-WIN");
            var model = new BookieFeedModel();
            int finalRound = run.Config.Rounds;

            ObserveThree(model, 1, run, Phase.RunWon, finalRound, 0,
                Report(finalRound, 80, 0, Phase.RunWon));

            AssertKinds(model, BookieMessageKind.RUN_START, BookieMessageKind.CLEARED,
                BookieMessageKind.VERDICT_WON);
            AssertRounds(model, finalRound, finalRound, finalRound);
        }

        [Test]
        public void New_run_reset_is_atomic_restores_warm_tier_and_keeps_revision_monotone()
        {
            var model = new BookieFeedModel();
            var first = new Run("FEED-RESET-A");
            model.Observe(1, first, Phase.Shop, 1, 100, Report(1, 0, 100, Phase.Shop));
            long beforeReset = model.Revision;

            var second = new Run("FEED-RESET-B");
            model.Observe(2, second, Phase.Betting, 1, 0, null);
            AssertKinds(model, BookieMessageKind.RUN_START);
            Assert.AreEqual(1, model.UnreadCount);
            Assert.Greater(model.Revision, beforeReset, "Revision never resets across runs");

            model.Observe(2, second, Phase.Shop, 1, 55, Report(1, 0, 55, Phase.Shop));
            AssertKinds(model, BookieMessageKind.RUN_START, BookieMessageKind.FLOAT_WARM);
            Assert.AreEqual(1, model.Messages[1].Round, "settle dedup keys must not leak across generations");
        }

        [Test]
        public void Variant_pick_is_deterministic_across_model_instances()
        {
            var runA = new Run("FEED-VARIANT");
            var runB = new Run("FEED-VARIANT");
            var a = new BookieFeedModel();
            var b = new BookieFeedModel();
            RunDirector.SettleReport report = Report(2, 0, 1234, Phase.Shop);

            a.Observe(1, runA, Phase.Shop, 2, 1234, report);
            b.Observe(99, runB, Phase.Shop, 2, 1234, report);

            Assert.AreEqual(a.Messages[1].Text, b.Messages[1].Text);
            Assert.AreEqual(
                BookieScript.Write("FEED-VARIANT", 2, BookieMessageKind.FLOAT_WARM, 1234),
                a.Messages[1].Text);
        }

        [Test]
        public void Arrival_sequence_changes_only_for_appends_including_equal_count_new_run_welcome()
        {
            var model = new BookieFeedModel();
            var first = new Run("FEED-ARRIVE-A");
            model.Observe(1, first, Phase.Betting, 1, 0, null);
            Assert.AreEqual(1, model.ArrivalSequence);

            long beforeReadRevision = model.Revision;
            model.MarkRead();
            Assert.AreEqual(1, model.ArrivalSequence, "read state is not an arrival");
            Assert.Greater(model.Revision, beforeReadRevision);
            model.MarkRead();
            Assert.AreEqual(1, model.ArrivalSequence);

            long beforeResetRevision = model.Revision;
            var second = new Run("FEED-ARRIVE-B");
            model.Observe(2, second, Phase.Betting, 1, 0, null);
            Assert.AreEqual(1, model.Messages.Count, "reset plus welcome preserves the visible count");
            Assert.AreEqual(2, model.ArrivalSequence,
                "the reset itself is silent; only the equal-count new welcome arrives");
            Assert.Greater(model.Revision, beforeResetRevision);

            model.Observe(2, second, Phase.Shop, 1, 50, Report(1, 0, 50, Phase.Shop));
            Assert.AreEqual(3, model.ArrivalSequence, "every appended message advances exactly once");
        }

        [Test]
        public void Full_feed_observation_consumes_no_engine_rng_strong_two_run_form()
        {
            var observed = new Run("FEED-PURE");
            var control = new Run("FEED-PURE");
            var feed = new BookieFeedModel();

            feed.Observe(1, observed, observed.Phase, observed.Round, observed.Debt, null);
            observed.PlaceTicket(new List<Pick> { new Pick(0, Side.Home) }, 100);
            control.PlaceTicket(new List<Pick> { new Pick(0, Side.Home) }, 100);

            observed.LockRound();
            feed.Observe(1, observed, observed.Phase, observed.Round, observed.Debt, null);
            control.LockRound();
            AssertRunsEqual(control, observed, "after LockRound");

            observed.FastForwardRound();
            feed.Observe(1, observed, observed.Phase, observed.Round, observed.Debt, null);
            control.FastForwardRound();
            AssertRunsEqual(control, observed, "after outcomes and FinishSweat");

            double debtBefore = observed.Debt;
            observed.Settle();
            var report = new RunDirector.SettleReport(observed.Round, observed.Bank,
                observed.CurrentTarget, debtBefore, observed.Debt, observed.Phase);
            feed.Observe(1, observed, observed.Phase, observed.Round, observed.Debt, report);
            feed.MarkRead();
            control.Settle();
            AssertRunsEqual(control, observed, "after Settle and shop generation");

            Assert.AreEqual(Phase.Shop, observed.Phase, "the fixed $100 ticket always leaves target capital");
            observed.ExitShop();
            feed.Observe(1, observed, observed.Phase, observed.Round, observed.Debt, report);
            control.ExitShop();
            AssertRunsEqual(control, observed, "after next-slate generation");
        }

        private static RunDirector.SettleReport Report(int round, double debtBefore,
                                                       double debtAfter, Phase outcome)
            => new RunDirector.SettleReport(round, 500, 400, debtBefore, debtAfter, outcome);

        private static void ObserveThree(BookieFeedModel model, int generation, Run run, Phase phase,
                                         int round, double debt, RunDirector.SettleReport? report)
        {
            for (int i = 0; i < 3; i++)
                model.Observe(generation, run, phase, round, debt, report);
        }

        private static void AssertKinds(BookieFeedModel model, params BookieMessageKind[] expected)
        {
            Assert.AreEqual(expected.Length, model.Messages.Count);
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], model.Messages[i].Kind, $"message {i}");
        }

        private static void AssertRounds(BookieFeedModel model, params int[] expected)
        {
            Assert.AreEqual(expected.Length, model.Messages.Count);
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], model.Messages[i].Round, $"message {i}");
        }

        private static void AssertRunsEqual(Run expected, Run actual, string when)
        {
            Assert.AreEqual(expected.Phase, actual.Phase, when);
            Assert.AreEqual(expected.Round, actual.Round, when);
            Assert.AreEqual(expected.Bank, actual.Bank, 1e-9, when);
            Assert.AreEqual(expected.Debt, actual.Debt, 1e-9, when);
            Assert.AreEqual(expected.PiggyBankBalance, actual.PiggyBankBalance, 1e-9, when);
            Assert.AreEqual(expected.Tickets.Count, actual.Tickets.Count, when);
            Assert.AreEqual(expected.Sweats.Count, actual.Sweats.Count, when);
            Assert.AreEqual(expected.ShopOffers.Count, actual.ShopOffers.Count, when);
            Assert.AreEqual(expected.CurrentSlate.Matchups.Count, actual.CurrentSlate.Matchups.Count, when);

            for (int i = 0; i < expected.CurrentSlate.Matchups.Count; i++)
            {
                Matchup a = expected.CurrentSlate.Matchups[i];
                Matchup b = actual.CurrentSlate.Matchups[i];
                Assert.AreEqual(a.Result, b.Result, $"{when}: outcome {i}");
                Assert.AreEqual(a.TrueHomeProb, b.TrueHomeProb, 1e-12, $"{when}: probability {i}");
                Assert.AreEqual(a.HomeOdds, b.HomeOdds, 1e-12, $"{when}: home odds {i}");
                Assert.AreEqual(a.AwayOdds, b.AwayOdds, 1e-12, $"{when}: away odds {i}");
                Assert.AreEqual(a.Home.Name, b.Home.Name, $"{when}: home team {i}");
                Assert.AreEqual(a.Away.Name, b.Away.Name, $"{when}: away team {i}");
            }

            for (int i = 0; i < expected.Tickets.Count; i++)
            {
                Assert.AreEqual(expected.Tickets[i].State, actual.Tickets[i].State, $"{when}: ticket {i}");
                Assert.AreEqual(expected.Tickets[i].PotentialPayout, actual.Tickets[i].PotentialPayout,
                    1e-9, $"{when}: payout {i}");
            }

            for (int i = 0; i < expected.ShopOffers.Count; i++)
                Assert.AreEqual(expected.ShopOffers[i].Id, actual.ShopOffers[i].Id,
                    $"{when}: shop offer {i}");
        }
    }
}
