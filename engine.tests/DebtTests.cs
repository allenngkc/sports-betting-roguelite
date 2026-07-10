using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// Debt-as-HP (DECISIONS.md 2026-07-09). Every test pins DebtJuiceRate = 0.5 and its own targets so
/// the assertions are immune to §8 recalibration: a clean miss books shortfall × 1.5 and tops the
/// bank to the target; a settle at ≥ target + debt repays in cash; a miss while indebted (or any
/// final-round miss) loses the run. No-bet rounds keep every bank value exact.
/// </summary>
public class DebtTests
{
    private static RunConfig Cfg(params double[] targets) => new RunConfig
    {
        StartingBank = 500,
        Targets = targets,
        DebtJuiceRate = 0.5, // pinned: these tests assert the ×1.5 booking regardless of calibration
    };

    private static void PlayRoundNoBets(Run run)
    {
        run.LockRound();
        run.FastForwardRound();
        run.Settle();
    }

    [Fact]
    public void Requirement_is_target_plus_debt()
    {
        var run = new Run("DEBT-REQ", Cfg(800, 1200));
        Assert.Equal(0.0, run.Debt);
        Assert.Equal(run.CurrentTarget, run.Requirement);   // clean: requirement == target

        PlayRoundNoBets(run); // miss 800 with bank 500 → float
        run.ExitShop();
        Assert.Equal(450.0, run.Debt, 10);
        Assert.Equal(1200.0 + 450.0, run.Requirement, 10);  // indebted: target + debt
    }

    [Fact]
    public void A_clean_miss_borrows_bank_topped_to_target_debt_is_juiced_shortfall()
    {
        var run = new Run("DEBT-BORROW", Cfg(800, 1200));
        PlayRoundNoBets(run);

        Assert.Equal(Phase.Shop, run.Phase);                // the run continues
        Assert.Equal(800.0, run.Bank, 10);                  // working capital up to the target
        Assert.Equal((800.0 - 500.0) * 1.5, run.Debt, 10);  // shortfall × (1 + 0.5)
        Assert.NotEmpty(run.ShopOffers);                    // the float lands in a real shop
    }

    [Fact]
    public void Repayment_deducts_exactly_the_debt_and_lands_at_or_above_target()
    {
        // Round 2's target is tiny, so the floated bank (800) clears the requirement (100 + 450 = 550).
        var run = new Run("DEBT-REPAY", Cfg(800, 100));
        PlayRoundNoBets(run); // float: bank 800, debt 450
        run.ExitShop();

        double bankBefore = run.Bank; // 800
        double debt = run.Debt;       // 450
        PlayRoundNoBets(run);         // clears 550 → repaid in cash

        Assert.Equal(0.0, run.Debt, 12);
        Assert.Equal(bankBefore - debt, run.Bank, 10);      // deducted exactly the debt
        Assert.True(run.Bank >= run.CurrentTarget);         // by construction ≥ target
        Assert.Equal(Phase.RunWon, run.Phase);              // round 2 was final and it cleared
    }

    [Fact]
    public void Missing_while_indebted_loses_the_run()
    {
        var run = new Run("DEBT-COLLECT", Cfg(800, 5000, 5000));
        PlayRoundNoBets(run); // float: bank 800, debt 450
        run.ExitShop();
        PlayRoundNoBets(run); // 800 < 5000 + 450 while indebted → the bookie collects

        Assert.Equal(Phase.RunLost, run.Phase);
        Assert.True(run.Debt > 0); // died owing — the run-over screen distinguishes this death
    }

    [Fact]
    public void A_final_round_miss_never_borrows()
    {
        var run = new Run("DEBT-FINAL", Cfg(800)); // one round: round 1 IS the final round
        PlayRoundNoBets(run);

        Assert.Equal(Phase.RunLost, run.Phase);
        Assert.Equal(0.0, run.Debt, 12); // no float on the final round
    }

    [Fact]
    public void A_final_round_clear_while_indebted_repays_and_wins()
    {
        var run = new Run("DEBT-FINAL-REPAY", Cfg(800, 100));
        PlayRoundNoBets(run); // float
        run.ExitShop();
        PlayRoundNoBets(run); // final round: 800 ≥ 550 → repay 450 → bank 350 ≥ 100 → won

        Assert.Equal(Phase.RunWon, run.Phase);
        Assert.Equal(0.0, run.Debt, 12);
        Assert.Equal(350.0, run.Bank, 10);
    }

    [Fact]
    public void Borrow_then_repay_then_shop_flow_across_two_rounds()
    {
        var run = new Run("DEBT-FLOW", Cfg(800, 100, 100));
        PlayRoundNoBets(run);                    // R1: float (bank 800, debt 450) → Shop
        Assert.Equal(Phase.Shop, run.Phase);
        run.ExitShop();

        PlayRoundNoBets(run);                    // R2: 800 ≥ 100 + 450 → repaid → Shop (not final)
        Assert.Equal(Phase.Shop, run.Phase);
        Assert.Equal(0.0, run.Debt, 12);
        Assert.Equal(350.0, run.Bank, 10);
        run.ExitShop();

        Assert.Equal(3, run.Round);              // the run continues clean
        Assert.Equal(Phase.Betting, run.Phase);
        Assert.Equal(run.CurrentTarget, run.Requirement);
    }

    [Fact]
    public void Debt_trajectories_are_deterministic_for_a_seed()
    {
        static List<(double bank, double debt, Phase phase)> Trajectory(string seed)
        {
            var run = new Run(seed, new RunConfig
            {
                StartingBank = 500,
                Targets = new double[] { 900, 1400, 2200, 3400 },
                DebtJuiceRate = 0.5,
            });
            var traj = new List<(double, double, Phase)>();
            while (true)
            {
                // A live betting policy so banks actually move: 30% of bank on a 2-leg parlay.
                double stake = Math.Floor(0.30 * run.Bank);
                if (stake >= run.Config.MinStake)
                    run.PlaceTicket(new[] { new Pick(0, Side.Home), new Pick(1, Side.Away) }, stake);
                run.LockRound();
                run.FastForwardRound();
                run.Settle();
                traj.Add((run.Bank, run.Debt, run.Phase));
                if (run.Phase != Phase.Shop) return traj;
                run.ExitShop();
            }
        }

        var a = Trajectory("DEBT-DET-7");
        var b = Trajectory("DEBT-DET-7");
        Assert.Equal(a.Count, b.Count);
        for (int i = 0; i < a.Count; i++)
        {
            Assert.Equal(a[i].bank, b[i].bank, 12);
            Assert.Equal(a[i].debt, b[i].debt, 12);
            Assert.Equal(a[i].phase, b[i].phase);
        }
        Assert.Contains(a, s => s.debt > 0); // the seed genuinely exercised a float
    }
}
