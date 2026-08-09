using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// T61: tests the round-advance hypothesis behind TV's scorer finding — that a harness polling
/// <c>Tickets[0]</c> while completion is driven by the current SESSION can read a settled ticket as
/// a hang, because the two need not refer to the same object.
///
/// These are engine-level because the divergence is an engine lifecycle fact, not a TV one:
/// <c>RunDirector</c> pairs <c>CurrentSession = Run.Sweats[SweatIndex]</c> with
/// <c>CurrentTicket = Run.Tickets[SweatIndex]</c>, so index 0 is the live ticket only while
/// SweatIndex is 0. Proving it here means it holds for any consumer, not just the one harness.
/// </summary>
public class SweatPollingContractTests
{
    private static Pick[] Picks(params (int m, Side s)[] p)
    {
        var picks = new Pick[p.Length];
        for (int i = 0; i < p.Length; i++) picks[i] = new Pick(p[i].m, p[i].s);
        return picks;
    }

    private static RunConfig EasyPayments(params double[] payments)
        => new RunConfig { Payments = payments, StartingBank = 500 };

    /// <summary>The hypothesis, first half — and the sharp result. Draining a session does NOT
    /// settle its ticket in general: a ticket that SURVIVES its sweat stays Open until FinishSweat,
    /// while a ticket that DIES is marked Lost the moment its leg dies (Run.cs's settle loop skips
    /// it as "already settled"). Cash-out is the same shape.
    ///
    /// So whether `Tickets[0]` is terminal mid-round depends on the OUTCOME, not the position — and
    /// that is exactly why a poller built on it fails intermittently rather than always. A harness
    /// waiting for `Tickets[0]` to leave Open gets an early false "done" on a losing ticket and no
    /// signal at all on a winning one, from the same code, decided by a seed.</summary>
    [Fact]
    public void Whether_ticket_zero_settles_mid_sweat_depends_on_its_outcome_not_its_position()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        Ticket first = run.PlaceTicket(Picks((0, Side.Away)), 20);
        Ticket second = run.PlaceTicket(Picks((1, Side.Home)), 20);
        run.LockRound();

        Assert.Equal(2, run.Sweats.Count);
        Assert.Equal(TicketState.Open, first.State);

        // Drain ONLY the first ticket's session — exactly what the director does before it calls
        // AdvanceSweat and moves CurrentSession onto the second ticket.
        while (run.Sweats[0].MoveNext(out _)) { }
        Assert.Equal(Phase.Sweat, run.Phase);

        bool anyLegLost = false;
        foreach (Leg leg in first.Legs) if (leg.State == LegState.Lost) anyLegLost = true;

        if (anyLegLost)
            Assert.Equal(TicketState.Lost, first.State);   // dies during the sweat
        else
            Assert.Equal(TicketState.Open, first.State);   // survives: still Open until FinishSweat

        // Either way the RUN is not finished, which is the only thing a poller may conclude from.
        Assert.Equal(Phase.Sweat, run.Phase);
    }

    /// <summary>The hypothesis, second half — the round-advance case. `Run.Tickets` is the CURRENT
    /// round's working set and is cleared at ExitShop, so a reference captured as `Tickets[0]`
    /// survives as a settled object that can never change again while `Tickets[0]` itself now names
    /// a different, open ticket. A poller holding the captured reference waits forever on something
    /// already terminal — which is precisely "a settled ticket reads as a hang".</summary>
    [Fact]
    public void A_captured_ticket_zero_goes_stale_across_a_round_advance()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10, 10));
        Ticket capturedRoundOne = run.PlaceTicket(Picks((0, Side.Away)), 20);
        Assert.Same(capturedRoundOne, run.Tickets[0]);

        run.LockRound();
        run.FastForwardRound();
        run.Settle();
        Assert.Equal(Phase.Shop, run.Phase);
        run.ExitShop();

        Assert.Empty(run.Tickets);                       // the working set is cleared, not carried
        Ticket roundTwo = run.PlaceTicket(Picks((0, Side.Away)), 20);

        // `Tickets[0]` now names a DIFFERENT object, and the captured one is terminal forever.
        Assert.NotSame(capturedRoundOne, run.Tickets[0]);
        Assert.Same(roundTwo, run.Tickets[0]);
        Assert.NotEqual(TicketState.Open, capturedRoundOne.State);
        Assert.Equal(TicketState.Open, roundTwo.State);

        // And the settled record still holds the old one, which is what a ledger should read.
        Assert.Contains(capturedRoundOne, run.SettledTickets);
    }

    /// <summary>The safe contract, stated positively so a harness author can copy it: completion is
    /// a property of the RUN's phase, never of any one ticket. Phase leaves Sweat exactly once,
    /// after every session is drained — which is the condition a poller should wait on.</summary>
    [Fact]
    public void Run_phase_is_the_only_sound_completion_signal()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.PlaceTicket(Picks((0, Side.Away)), 20);
        run.PlaceTicket(Picks((1, Side.Home)), 20);
        run.LockRound();

        var terminalWhileSweating = new List<TicketState>();
        foreach (SweatSession session in run.Sweats)
        {
            while (session.MoveNext(out _)) { }
            if (run.Phase == Phase.Sweat)
                foreach (Ticket t in run.Tickets)
                    if (t.State != TicketState.Open) terminalWhileSweating.Add(t.State);
        }

        // At least one ticket was terminal while the run was still in Sweat — the trap.
        Assert.NotEmpty(terminalWhileSweating);

        run.FinishSweat();
        Assert.NotEqual(Phase.Sweat, run.Phase);
        foreach (Ticket t in run.Tickets) Assert.NotEqual(TicketState.Open, t.State);
    }
}
