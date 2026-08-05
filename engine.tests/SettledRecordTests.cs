using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// S36: the engine retains what a cashed-out ticket actually returned, and keeps the run's
/// settled tickets across every round so the LEDGER has a run-long source. Before this,
/// <see cref="Run.Tickets"/> was cleared at ExitShop and the cash-out figure was never stored at
/// all, so a settled record could print neither the row nor the money.
/// </summary>
public class SettledRecordTests
{
    private static Pick[] Picks(params (int m, Side s)[] p)
    {
        var picks = new Pick[p.Length];
        for (int i = 0; i < p.Length; i++) picks[i] = new Pick(p[i].m, p[i].s);
        return picks;
    }

    private static RunConfig EasyPayments(params double[] payments)
        => new RunConfig { Payments = payments, StartingBank = 500 };

    /// <summary>Cashes a ticket out in round 1, then plays the run to its end and asserts the
    /// figure is still there and still exact. The round boundary is the specific thing that used
    /// to destroy it — ExitShop clears the working set — so the assertion is made AFTER the run
    /// has left that round behind, not merely after the cash-out.</summary>
    [Fact]
    public void A_cashed_out_figure_is_retained_and_survives_to_run_end()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10, 10));

        Ticket cashed = run.PlaceTicket(Picks((0, Side.Away), (2, Side.Away)), 100);
        run.LockRound();
        run.Sweats[0].MoveNext(out _);

        double offer = run.Sweats[0].CashOutOffer()!.Value;
        Assert.True(offer > 0.0, "the fixture must actually have a live cash-out offer");
        run.Sweats[0].AcceptCashOut();

        // Retained at the moment of acceptance, exactly.
        Assert.Equal(TicketState.CashedOut, cashed.State);
        Assert.NotNull(cashed.CashedOutFor);
        Assert.Equal(offer, cashed.CashedOutFor!.Value, 10);

        // Play out the remaining rounds. Each ExitShop clears Tickets — the old failure mode.
        run.FastForwardRound();
        run.Settle();
        while (run.Phase == Phase.Shop)
        {
            run.ExitShop();                             // -> Betting, and Tickets is cleared here
            run.PlaceTicket(Picks((0, Side.Away)), 10); // a round needs a locked ticket to sweat
            run.LockRound();
            run.FastForwardRound();
            run.Settle();
        }

        Assert.True(run.Phase == Phase.RunWon || run.Phase == Phase.RunLost,
            $"the run must have reached a terminal phase, was {run.Phase}");

        // The working set no longer holds it...
        Assert.DoesNotContain(cashed, run.Tickets);
        // ...but the run-long settled record does, with the figure intact.
        Assert.Contains(cashed, run.SettledTickets);
        Ticket recorded = Assert.Single(run.SettledTickets, t => ReferenceEquals(t, cashed));
        Assert.Equal(offer, recorded.CashedOutFor!.Value, 10);
    }

    /// <summary>Only a cash-out retains a figure. A win's return is stake × odds and a loss's is
    /// nothing — both re-derivable — so storing a number for them would be a second source of
    /// truth for money the engine already knows how to compute.</summary>
    [Fact]
    public void Only_cashed_out_tickets_carry_a_retained_figure()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.PlaceTicket(Picks((0, Side.Away)), 50);
        run.PlaceTicket(Picks((2, Side.Away)), 50);
        run.LockRound();
        run.FastForwardRound();
        run.Settle();

        Assert.NotEmpty(run.SettledTickets);
        foreach (Ticket t in run.SettledTickets)
        {
            Assert.NotEqual(TicketState.Open, t.State);
            if (t.State == TicketState.CashedOut) Assert.NotNull(t.CashedOutFor);
            else Assert.Null(t.CashedOutFor);
        }
    }

    /// <summary>The settled record accumulates across rounds rather than being replaced by the
    /// latest one — the property the LEDGER's "this run" scope depends on.</summary>
    [Fact]
    public void Settled_tickets_accumulate_across_rounds()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10, 10));
        int expected = 0;

        for (int round = 0; round < 3; round++)
        {
            if (round > 0) run.ExitShop();
            run.PlaceTicket(Picks((0, Side.Away)), 20);
            run.PlaceTicket(Picks((1, Side.Home)), 20);
            expected += 2;
            run.LockRound();
            run.FastForwardRound();
            run.Settle();

            Assert.Equal(expected, run.SettledTickets.Count);
            if (run.Phase != Phase.Shop) break;
        }

        // Ids carry the round, so the record can say which round a row belongs to without any
        // parallel bookkeeping.
        Assert.Contains(run.SettledTickets, t => t.Id.StartsWith("1."));
        Assert.Contains(run.SettledTickets, t => t.Id.StartsWith("2."));
    }
}
