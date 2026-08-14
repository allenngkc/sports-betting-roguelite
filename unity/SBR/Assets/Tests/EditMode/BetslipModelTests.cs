using System.Collections.Generic;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// M4 EditMode: the betslip model's toggle semantics, chip/nudge stake math, preview accuracy
    /// against the engine's own payout, and place blockers. Pure engine + model — no Unity runtime.
    /// </summary>
    public class BetslipModelTests
    {
        // The chip/reanchor math below was written against bank 500; the economy rework moved the
        // default to 350, so every run pins its config.
        private static RunConfig Bank500() => new RunConfig { StartingBank = 500 };

        [Test]
        public void Toggle_adds_switches_and_removes()
        {
            var run = new Run("SLIP-TOGGLE", Bank500());
            var slip = new BetslipModel(run);

            Assert.IsTrue(slip.Toggle(0, Side.Home));
            Assert.AreEqual(Side.Home, slip.SideOn(0));

            Assert.IsTrue(slip.Toggle(0, Side.Away), "clicking the other side switches");
            Assert.AreEqual(Side.Away, slip.SideOn(0));
            Assert.AreEqual(1, slip.Picks.Count, "switching must not duplicate the leg");

            Assert.IsTrue(slip.Toggle(0, Side.Away), "clicking the same side removes");
            Assert.IsNull(slip.SideOn(0));
            Assert.AreEqual(0, slip.Picks.Count);
        }

        /// <summary>THE DRAW IS NOT A TEAM (DD batch 49), pinned on the surface that asks the
        /// question. This is a check that STOPS rather than prints: the old shape returned Away for
        /// a moneyline draw, which is a silently wrong team on a slip whose job is showing what you
        /// backed. It is unreachable today only because no surface can build a draw pick yet, so
        /// without this test the defect would ship dormant and wake up during Phase S.
        ///
        /// Found by sweeping the population after the gift-streak dutch, rather than by waiting for
        /// the next one to fail (C46).</summary>
        [Test]
        public void A_moneyline_draw_has_no_side_and_never_reports_away()
        {
            var run = new Run("SLIP-DRAW", Bank500());
            var slip = new BetslipModel(run);

            slip.Toggle(0, MarketSelection.MoneylineDraw());

            Assert.AreEqual(1, slip.Picks.Count, "the draw is a real, backable selection");
            Assert.AreEqual(MarketChoice.Draw, slip.SelectionOn(0)!.Value.Choice);
            Assert.IsNull(slip.SideOn(0), "a draw has no team — reporting Away here is the defect");

            // And the team sides still answer, so the null is specific to the draw rather than a
            // blanket refusal that would break the two rows the surface already renders.
            slip.Toggle(1, Side.Home);
            Assert.AreEqual(Side.Home, slip.SideOn(1));
            slip.Toggle(2, Side.Away);
            Assert.AreEqual(Side.Away, slip.SideOn(2));
        }

        [Test]
        public void Toggle_caps_new_legs_at_max_but_still_switches_existing()
        {
            var run = new Run("SLIP-CAP", new RunConfig { MaxLegs = 2, StartingBank = 500 });
            var slip = new BetslipModel(run);

            Assert.IsTrue(slip.Toggle(0, Side.Home));
            Assert.IsTrue(slip.Toggle(1, Side.Home));
            Assert.IsFalse(slip.Toggle(2, Side.Home), "a third NEW leg exceeds MaxLegs=2");
            Assert.AreEqual(2, slip.Picks.Count);

            Assert.IsTrue(slip.Toggle(1, Side.Away), "switching an existing leg is always allowed");
            Assert.AreEqual(Side.Away, slip.SideOn(1));
        }

        [Test]
        public void Market_toggle_replaces_a_match_leg_and_same_selection_removes_it()
        {
            var run = new Run("SLIP-MARKET-TOGGLE", Bank500());
            var slip = new BetslipModel(run);
            MarketSelection over = MarketSelection.TotalGoals(2.5, true);
            MarketSelection under = MarketSelection.TotalGoals(3.5, false);

            Assert.IsTrue(slip.Toggle(0, over));
            Assert.AreEqual(over, slip.SelectionOn(0));
            Assert.IsNull(slip.SideOn(0), "market legs must not be projected to a moneyline side");

            Assert.IsTrue(slip.Toggle(0, under), "a different market replaces the existing matchup leg");
            Assert.AreEqual(1, slip.Picks.Count);
            Assert.AreEqual(under, slip.SelectionOn(0));

            Assert.IsTrue(slip.Toggle(0, under), "clicking the same market removes the leg");
            Assert.AreEqual(0, slip.Picks.Count);
        }

        [Test]
        public void Market_preview_and_place_preserve_selection_and_market_label()
        {
            var run = new Run("SLIP-MARKET-PLACE", Bank500());
            var slip = new BetslipModel(run);
            MarketSelection goals = MarketSelection.TotalGoals(2.5, true);
            MarketSelection btts = MarketSelection.BothTeamsToScore(true);

            slip.Toggle(0, goals);
            slip.Toggle(1, btts);
            double expectedOdds = run.CurrentSlate.Matchups[0].Odds(goals)
                * run.CurrentSlate.Matchups[1].Odds(btts);
            Assert.AreEqual(expectedOdds, slip.CombinedOdds, 1e-12);

            Ticket ticket = slip.Place();
            Assert.AreEqual(goals, ticket.Legs[0].Selection);
            Assert.AreEqual(btts, ticket.Legs[1].Selection);
            Assert.IsTrue(ticket.Legs[0].DisplayLabel.Contains("GOALS"));
            Assert.IsTrue(ticket.Legs[1].DisplayLabel.Contains("BTTS"));
        }

        [Test]
        public void Market_toggle_respects_max_legs_for_new_matchups()
        {
            var run = new Run("SLIP-MARKET-CAP", new RunConfig { MaxLegs = 1, StartingBank = 500 });
            var slip = new BetslipModel(run);

            Assert.IsTrue(slip.Toggle(0, MarketSelection.TotalCorners(8.5, true)));
            Assert.IsFalse(slip.Toggle(1, MarketSelection.TotalCards(3.5, false)));
            Assert.IsTrue(slip.Toggle(0, MarketSelection.BothTeamsToScore(false)));
            Assert.AreEqual(1, slip.Picks.Count);
        }

        [Test]
        public void Stake_chips_and_nudges_clamp_to_min_and_bank()
        {
            var run = new Run("SLIP-STAKE", Bank500()); // starting bank 500
            var slip = new BetslipModel(run);

            Assert.AreEqual(50.0, slip.Stake, 1e-9, "default chip is 10% of bank");

            slip.SetStakeFraction(0.25);
            Assert.AreEqual(125.0, slip.Stake, 1e-9);

            slip.SetStakeFraction(1.0);
            Assert.AreEqual(500.0, slip.Stake, 1e-9, "MAX chip is the whole bank (uncapped stakes)");

            slip.Nudge(+10);
            Assert.AreEqual(500.0, slip.Stake, 1e-9, "nudge clamps at the bank");

            slip.SetStakeFraction(0.0);
            Assert.AreEqual(run.Config.MinStake, slip.Stake, 1e-9, "chips clamp up to the min stake");

            slip.Nudge(-10);
            Assert.AreEqual(run.Config.MinStake, slip.Stake, 1e-9, "nudge clamps at the min stake");
        }

        [Test]
        public void ToWin_preview_matches_the_engine_ticket_payout_without_relics()
        {
            var run = new Run("SLIP-PREVIEW", Bank500());
            var slip = new BetslipModel(run);

            slip.Toggle(0, Side.Home);
            slip.Toggle(3, Side.Away);
            slip.SetStakeFraction(0.25);
            double preview = slip.ToWin;

            Ticket ticket = slip.Place();
            Assert.AreEqual(preview, ticket.PotentialPayout, 1e-9,
                "with no compose-time relics the base-odds preview IS the engine payout");
        }

        [Test]
        public void Place_clears_the_slip_and_reanchors_the_stake_to_the_new_bank()
        {
            var run = new Run("SLIP-REANCHOR", Bank500()); // bank 500
            var slip = new BetslipModel(run);

            slip.Toggle(0, Side.Home);
            slip.SetStakeFraction(0.25); // 125
            slip.Place();

            Assert.AreEqual(0, slip.Picks.Count);
            Assert.AreEqual(375.0, run.Bank, 1e-9);
            Assert.AreEqual(37.0, slip.Stake, 1e-9, "stake re-anchors to floor(10% of the new bank)");
        }

        [Test]
        public void PlaceBlocker_walks_the_reasons()
        {
            var run = new Run("SLIP-BLOCK", Bank500());
            var slip = new BetslipModel(run);

            Assert.AreEqual("pick a side", slip.PlaceBlocker);

            slip.Toggle(0, Side.Home);
            Assert.IsNull(slip.PlaceBlocker);

            for (int i = 0; i < run.Config.MaxTicketsPerRound; i++)
            {
                run.PlaceTicket(new List<Pick> { new Pick(i + 1, Side.Home) }, 10);
            }
            Assert.AreEqual($"max {run.Config.MaxTicketsPerRound} tickets", slip.PlaceBlocker);

            run.LockRound();
            Assert.AreEqual("betting is closed", slip.PlaceBlocker);
        }

        [Test]
        public void Model_consumes_no_engine_rng()
        {
            var run = new Run("SLIP-PURE", Bank500());
            var slip = new BetslipModel(run);
            slip.Toggle(0, Side.Home);
            slip.Toggle(2, Side.Away);
            slip.SetStakeFraction(0.5);
            _ = slip.CombinedOdds;
            _ = slip.ToWin;
            slip.Place();

            var control = new Run("SLIP-PURE", Bank500());
            run.LockRound();
            control.LockRound();
            for (int i = 0; i < control.CurrentSlate.Matchups.Count; i++)
                Assert.AreEqual(control.CurrentSlate.Matchups[i].Result, run.CurrentSlate.Matchups[i].Result,
                    "slip math must never perturb the fixed outcome universe");
        }
    }
}
