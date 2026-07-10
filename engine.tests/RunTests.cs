using SBR.Engine;

namespace SBR.Engine.Tests;

public class RunTests
{
    private static Run NewRun(string seed = "RUN-TEST", RunConfig? cfg = null) => new(seed, cfg);

    [Fact]
    public void New_run_starts_in_betting_with_prd_defaults()
    {
        // §8 defaults as recalibrated 2026-07-09 (debt-as-HP retune): flat-early targets, R1 = 400.
        var run = NewRun();
        Assert.Equal(Phase.Betting, run.Phase);
        Assert.Equal(1, run.Round);
        Assert.Equal(500, run.Bank);
        Assert.Equal(400, run.CurrentTarget);
        Assert.Equal(0.0, run.Debt);
        Assert.Equal(run.CurrentTarget, run.Requirement);
        Assert.Equal(6, run.CurrentSlate.Matchups.Count);
    }

    [Fact]
    public void Placing_a_ticket_deducts_stake_and_locks_offered_odds()
    {
        var run = NewRun();
        Matchup m = run.CurrentSlate.Matchups[0];
        Ticket t = run.PlaceTicket(new[] { new Pick(0, Side.Home) }, 100);

        Assert.Equal(400, run.Bank);
        Assert.Equal(TicketState.Open, t.State);
        Assert.Equal(m.HomeOdds, t.Legs[0].OfferedOdds, 12);
        Assert.Equal(100 * m.HomeOdds, t.PotentialPayout, 10);
    }

    [Fact]
    public void Ticket_vig_matches_the_design02_formula()
    {
        var run = NewRun();
        Matchup m = run.CurrentSlate.Matchups[0];
        Ticket t = run.PlaceTicket(new[] { new Pick(0, Side.Home) }, 100);

        double expected = OddsMath.VigPaid(100, m.HomeOdds, m.FairOdds(Side.Home));
        Assert.Equal(expected, t.VigPaid, 10);
        Assert.True(t.VigPaid > 0);
    }

    [Fact]
    public void Ticket_validation_rejects_bad_input()
    {
        var run = NewRun();
        Assert.Throws<ArgumentException>(() => run.PlaceTicket(new[] { new Pick(0, Side.Home) }, 5));
        Assert.Throws<ArgumentException>(() => run.PlaceTicket(new[] { new Pick(0, Side.Home) }, 501));
        Assert.Throws<ArgumentException>(() => run.PlaceTicket(Array.Empty<Pick>(), 100));
        Assert.Throws<ArgumentException>(
            () => run.PlaceTicket(new[] { new Pick(0, Side.Home), new Pick(0, Side.Away) }, 100));
        Assert.Throws<ArgumentException>(
            () => run.PlaceTicket(new[] { new Pick(1, Side.Home), new Pick(2, Side.Away), new Pick(1, Side.Away) }, 100));
    }

    [Fact]
    public void Fourth_ticket_is_rejected()
    {
        var run = NewRun();
        for (int i = 0; i < 3; i++)
            run.PlaceTicket(new[] { new Pick(i, Side.Home) }, 10);
        Assert.Throws<InvalidOperationException>(() => run.PlaceTicket(new[] { new Pick(3, Side.Home) }, 10));
    }

    [Fact]
    public void Seven_leg_parlay_is_rejected_by_config()
    {
        var cfg = new RunConfig { MatchupsPerSlate = 8 };
        var run = NewRun(cfg: cfg);
        var picks = Enumerable.Range(0, 7).Select(i => new Pick(i, Side.Home)).ToArray();
        Assert.Throws<ArgumentException>(() => run.PlaceTicket(picks, 100));
    }

    [Fact]
    public void Locking_resolves_every_matchup_and_settles_tickets()
    {
        var run = NewRun();
        run.PlaceTicket(new[] { new Pick(0, Side.Home), new Pick(1, Side.Away) }, 50);
        run.LockRound();
        run.FastForwardRound();

        Assert.Equal(Phase.Settlement, run.Phase);
        Assert.All(run.CurrentSlate.Matchups, m => Assert.NotNull(m.Result));
        Assert.All(run.Tickets, t => Assert.NotEqual(TicketState.Open, t.State));
    }

    [Fact]
    public void Bank_accounting_is_consistent_with_ticket_results()
    {
        var run = NewRun("ACCOUNTING");
        run.PlaceTicket(new[] { new Pick(0, Side.Home) }, 100);
        run.PlaceTicket(new[] { new Pick(1, Side.Away), new Pick(2, Side.Home) }, 50);
        double bankBeforeLock = run.Bank;

        run.LockRound();
        run.FastForwardRound();

        double expectedWinnings = run.Tickets
            .Where(t => t.State == TicketState.Won)
            .Sum(t => t.PotentialPayout);
        Assert.Equal(bankBeforeLock + expectedWinnings, run.Bank, 8);
    }

    [Fact]
    public void Tickets_sharing_a_matchup_share_its_outcome()
    {
        var run = NewRun("SHARED-OUTCOME");
        Ticket a = run.PlaceTicket(new[] { new Pick(0, Side.Home) }, 10);
        Ticket b = run.PlaceTicket(new[] { new Pick(0, Side.Home) }, 10);
        run.LockRound();
        run.FastForwardRound();

        Assert.Equal(a.State, b.State);
        Assert.Equal(a.Legs[0].State, b.Legs[0].State);
    }

    [Fact]
    public void Outcomes_for_a_seed_do_not_depend_on_what_was_bet()
    {
        var bettor = NewRun("FIXED-UNIVERSE");
        bettor.PlaceTicket(new[] { new Pick(3, Side.Away) }, 250);
        bettor.LockRound();

        var abstainer = NewRun("FIXED-UNIVERSE");
        abstainer.LockRound();

        for (int i = 0; i < 6; i++)
            Assert.Equal(abstainer.CurrentSlate.Matchups[i].Result, bettor.CurrentSlate.Matchups[i].Result);
    }

    [Fact]
    public void Missing_the_target_floats_you_into_the_shop()
    {
        // Debt-as-HP (DECISIONS.md 2026-07-09): a clean miss no longer kills the run — the bookie
        // floats the bank up to the target and books the shortfall at (1 + juice).
        var cfg = new RunConfig { StartingBank = 500, Targets = new double[] { 800, 1200 }, DebtJuiceRate = 0.5 };
        var run = NewRun(cfg: cfg);
        run.LockRound();
        run.FastForwardRound();
        run.Settle();

        Assert.Equal(Phase.Shop, run.Phase);
        Assert.Equal(800, run.Bank, 10);                    // topped up to the target
        Assert.Equal((800 - 500) * 1.5, run.Debt, 10);      // shortfall × 1.5
    }

    [Fact]
    public void Two_consecutive_misses_lose_the_run()
    {
        var cfg = new RunConfig { StartingBank = 500, Targets = new double[] { 800, 1200, 1900 }, DebtJuiceRate = 0.5 };
        var run = NewRun(cfg: cfg);
        run.LockRound(); run.FastForwardRound(); run.Settle(); // miss 1: float (bank 800, debt 450)
        Assert.Equal(Phase.Shop, run.Phase);
        run.ExitShop();
        run.LockRound(); run.FastForwardRound(); run.Settle(); // miss 2: 800 < 1200 + 450 while indebted

        Assert.Equal(Phase.RunLost, run.Phase);
        Assert.True(run.Debt > 0); // died owing the bookie
    }

    [Fact]
    public void Meeting_the_target_advances_through_the_shop()
    {
        var cfg = new RunConfig { Targets = new double[] { 100, 100 } };
        var run = NewRun(cfg: cfg);
        run.LockRound();
        run.FastForwardRound();
        run.Settle();
        Assert.Equal(Phase.Shop, run.Phase);

        run.ExitShop();
        Assert.Equal(2, run.Round);
        Assert.Equal(Phase.Betting, run.Phase);
        Assert.Empty(run.Tickets);
        Assert.Equal(100, run.CurrentTarget);
    }

    [Fact]
    public void Clearing_the_final_round_wins_the_run()
    {
        var cfg = new RunConfig { Targets = new double[] { 100, 100 } };
        var run = NewRun(cfg: cfg);

        run.LockRound();
        run.FastForwardRound();
        run.Settle();
        run.ExitShop();
        run.LockRound();
        run.FastForwardRound();
        run.Settle();

        Assert.Equal(Phase.RunWon, run.Phase);
    }

    [Fact]
    public void Fresh_slate_each_round_differs()
    {
        var cfg = new RunConfig { Targets = new double[] { 100, 100 } };
        var run = NewRun(cfg: cfg);
        double round1Prob = run.CurrentSlate.Matchups[0].TrueHomeProb;

        run.LockRound();
        run.FastForwardRound();
        run.Settle();
        run.ExitShop();

        Assert.Equal(2, run.CurrentSlate.Round);
        Assert.NotEqual(round1Prob, run.CurrentSlate.Matchups[0].TrueHomeProb);
    }

    [Fact]
    public void Phase_guards_reject_out_of_order_calls()
    {
        var run = NewRun();
        Assert.Throws<InvalidOperationException>(() => run.Settle());
        Assert.Throws<InvalidOperationException>(() => run.ExitShop());

        run.LockRound();
        Assert.Throws<InvalidOperationException>(() => run.LockRound());
        Assert.Throws<InvalidOperationException>(() => run.PlaceTicket(new[] { new Pick(0, Side.Home) }, 100));
    }

    [Fact]
    public void Entire_run_is_reproducible_from_its_seed()
    {
        static (Phase, double) PlayScriptedRun(string seed)
        {
            var run = new Run(seed);
            run.PlaceTicket(new[] { new Pick(0, Side.Home), new Pick(2, Side.Away) }, 120);
            run.PlaceTicket(new[] { new Pick(4, Side.Home) }, 60);
            run.LockRound();
            run.FastForwardRound();
            run.Settle();
            return (run.Phase, run.Bank);
        }

        (Phase phaseA, double bankA) = PlayScriptedRun("GOLDEN-1");
        (Phase phaseB, double bankB) = PlayScriptedRun("GOLDEN-1");
        Assert.Equal(phaseA, phaseB);
        Assert.Equal(bankA, bankB, 12);
    }
}
