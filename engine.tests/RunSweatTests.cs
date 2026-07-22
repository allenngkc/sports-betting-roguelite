using SBR.Engine;

namespace SBR.Engine.Tests;

public class RunSweatTests
{
    private static Pick[] Picks(params (int m, Side s)[] p)
    {
        var picks = new Pick[p.Length];
        for (int i = 0; i < p.Length; i++) picks[i] = new Pick(p[i].m, p[i].s);
        return picks;
    }

    [Fact]
    public void Locking_lands_in_sweat_with_one_session_per_ticket()
    {
        var run = new Run("SWEAT-1");
        run.PlaceTicket(Picks((0, Side.Home), (1, Side.Home)), 50);
        run.PlaceTicket(Picks((2, Side.Home)), 50);
        run.LockRound();

        Assert.Equal(Phase.Sweat, run.Phase);
        Assert.Equal(2, run.Sweats.Count);
        Assert.All(run.CurrentSlate.Matchups, m => Assert.NotNull(m.Result)); // fixed universe still sampled
    }

    [Fact]
    public void Sweats_is_empty_until_lock_and_after_leaving_the_shop()
    {
        var cfg = new RunConfig { Payments = new double[] { 100, 100 } };
        var run = new Run("SWEAT-EMPTY", cfg);
        Assert.Empty(run.Sweats);

        run.PlaceTicket(Picks((0, Side.Home)), 50);
        run.LockRound();
        Assert.NotEmpty(run.Sweats);

        run.FastForwardRound();
        run.Settle();
        run.ExitShop();
        Assert.Empty(run.Sweats); // cleared for the new round
    }

    [Fact]
    public void Zero_ticket_round_locks_to_sweat_and_finishes_immediately()
    {
        var run = new Run("SWEAT-ZERO");
        run.LockRound();
        Assert.Equal(Phase.Sweat, run.Phase);
        Assert.Empty(run.Sweats);

        run.FinishSweat(); // no sessions → immediately valid
        Assert.Equal(Phase.Settlement, run.Phase);
    }

    [Fact]
    public void Finishing_the_sweat_before_sessions_complete_throws()
    {
        var run = new Run("SWEAT-GUARD");
        run.PlaceTicket(Picks((0, Side.Home), (1, Side.Home)), 50);
        run.LockRound();
        Assert.Throws<InvalidOperationException>(() => run.FinishSweat());
    }

    [Fact]
    public void Fast_forward_outside_sweat_throws()
    {
        var run = new Run("SWEAT-GUARD-2");
        Assert.Throws<InvalidOperationException>(() => run.FastForwardRound()); // still in Betting
    }

    [Fact]
    public void Finish_sweat_settles_a_winning_ticket_and_a_losing_ticket()
    {
        // F_0.4.0 re-pin: (0,Away) wins, (1,Home) loses.
        var run = new Run("GOLDEN-W2");
        Ticket winner = run.PlaceTicket(Picks((0, Side.Away)), 100);
        Ticket loser = run.PlaceTicket(Picks((1, Side.Home)), 100);
        double bankBeforeLock = run.Bank;

        run.LockRound();
        run.FastForwardRound();

        Assert.Equal(TicketState.Won, winner.State);
        Assert.Equal(TicketState.Lost, loser.State);
        Assert.Equal(bankBeforeLock + winner.PotentialPayout, run.Bank, 8);
    }

    [Fact]
    public void Cashing_out_leaves_the_fixed_universe_untouched()
    {
        // A 3-leg parlay; the picks and stakes are identical across both runs.
        Pick[] picks = Picks((0, Side.Away), (2, Side.Away), (5, Side.Away)); // all win in GOLDEN-W2

        var casher = new Run("GOLDEN-W2");
        casher.PlaceTicket(picks, 100);
        casher.LockRound();
        SweatSession s = casher.Sweats[0];
        s.MoveNext(out _); // step into leg 1 (index 0)
        s.MoveNext(out _);
        s.AcceptCashOut();
        casher.FinishSweat();

        var abstainer = new Run("GOLDEN-W2");
        abstainer.PlaceTicket(picks, 100);
        abstainer.LockRound();
        abstainer.FastForwardRound();

        for (int i = 0; i < 6; i++)
            Assert.Equal(
                abstainer.CurrentSlate.Matchups[i].Result,
                casher.CurrentSlate.Matchups[i].Result);
    }

    [Fact]
    public void Phase_guards_hold_across_the_sweat_transitions()
    {
        var run = new Run("SWEAT-PHASES");
        run.PlaceTicket(Picks((0, Side.Home), (1, Side.Home)), 50);
        run.LockRound();

        // In Sweat: cannot re-lock, place tickets, settle, or exit the shop.
        Assert.Throws<InvalidOperationException>(() => run.LockRound());
        Assert.Throws<InvalidOperationException>(() => run.PlaceTicket(Picks((2, Side.Home)), 10));
        Assert.Throws<InvalidOperationException>(() => run.Settle());
        Assert.Throws<InvalidOperationException>(() => run.ExitShop());

        run.FastForwardRound();
        Assert.Equal(Phase.Settlement, run.Phase);
        Assert.Throws<InvalidOperationException>(() => run.FinishSweat()); // no longer in Sweat
    }
}
