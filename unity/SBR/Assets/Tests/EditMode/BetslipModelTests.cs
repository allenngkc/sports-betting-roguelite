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
        [Test]
        public void Toggle_adds_switches_and_removes()
        {
            var run = new Run("SLIP-TOGGLE");
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

        [Test]
        public void Toggle_caps_new_legs_at_max_but_still_switches_existing()
        {
            var run = new Run("SLIP-CAP", new RunConfig { MaxLegs = 2 });
            var slip = new BetslipModel(run);

            Assert.IsTrue(slip.Toggle(0, Side.Home));
            Assert.IsTrue(slip.Toggle(1, Side.Home));
            Assert.IsFalse(slip.Toggle(2, Side.Home), "a third NEW leg exceeds MaxLegs=2");
            Assert.AreEqual(2, slip.Picks.Count);

            Assert.IsTrue(slip.Toggle(1, Side.Away), "switching an existing leg is always allowed");
            Assert.AreEqual(Side.Away, slip.SideOn(1));
        }

        [Test]
        public void Stake_chips_and_nudges_clamp_to_min_and_bank()
        {
            var run = new Run("SLIP-STAKE"); // starting bank 500
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
            var run = new Run("SLIP-PREVIEW");
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
            var run = new Run("SLIP-REANCHOR"); // bank 500
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
            var run = new Run("SLIP-BLOCK");
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
            var run = new Run("SLIP-PURE");
            var slip = new BetslipModel(run);
            slip.Toggle(0, Side.Home);
            slip.Toggle(2, Side.Away);
            slip.SetStakeFraction(0.5);
            _ = slip.CombinedOdds;
            _ = slip.ToWin;
            slip.Place();

            var control = new Run("SLIP-PURE");
            run.LockRound();
            control.LockRound();
            for (int i = 0; i < control.CurrentSlate.Matchups.Count; i++)
                Assert.AreEqual(control.CurrentSlate.Matchups[i].Result, run.CurrentSlate.Matchups[i].Result,
                    "slip math must never perturb the fixed outcome universe");
        }
    }
}
