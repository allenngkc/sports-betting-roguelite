using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Engine.Tests;

// Seed GOLDEN-W2's round-1 outcomes are fixed per seed, but they are NOT all as an earlier version
// of this header claimed (it said away wins matchups 0, 2, 3, 4, 5; matchup 2's away side loses).
// Tests that depend on an outcome read it off Leg.State rather than asserting against a guess.
//
// T164: the displayed win-probability seeds from the TICKET, never from a leg. These pin
// SweatSession.TicketWinProbability against the same machinery CashOutFair already prices off —
// the single-leg coincidence, the ordinary multi-leg product at t=0 and while stepping, the two
// terminal states (a dead leg / every leg won), and a same-match ticket's ulp-level agreement with
// the price it was sold at.
public class TicketWinProbabilityTests
{
    private const string Seed = "GOLDEN-W2";

    private static (Run run, Ticket ticket, SweatSession session) Lock(Pick[] picks, double stake = 100)
    {
        var run = new Run(Seed);
        Ticket ticket = run.PlaceTicket(picks, stake);
        run.LockRound();
        return (run, ticket, run.Sweats[0]);
    }

    private static List<DramaEvent> Drain(SweatSession s)
    {
        var list = new List<DramaEvent>();
        while (s.MoveNext(out var e)) list.Add(e);
        return list;
    }

    private static Pick[] Picks(params (int m, Side s)[] p)
    {
        var picks = new Pick[p.Length];
        for (int i = 0; i < p.Length; i++) picks[i] = new Pick(p[i].m, p[i].s);
        return picks;
    }

    // ---- t = 0 anchors ----

    [Fact]
    public void Single_leg_ticket_win_probability_is_the_legs_own_true_prob()
    {
        var (_, ticket, session) = Lock(Picks((1, Side.Home))); // single, wins
        Assert.True(ticket.Legs[0].TrueProb == session.TicketWinProbability,
            $"expected {ticket.Legs[0].TrueProb:R}, got {session.TicketWinProbability:R}");
    }

    [Fact]
    public void Multi_leg_ordinary_ticket_at_start_is_the_product_of_true_probs_in_ascending_order()
    {
        var (_, ticket, session) = Lock(Picks((0, Side.Away), (1, Side.Home), (2, Side.Away)));

        double expected = ticket.Legs[0].TrueProb;
        for (int j = 1; j < ticket.Legs.Count; j++) expected *= ticket.Legs[j].TrueProb;

        Assert.True(expected == session.TicketWinProbability,
            $"expected {expected:R}, got {session.TicketWinProbability:R}");
    }

    // ---- tracking the drama ----

    [Fact]
    public void It_tracks_the_drama_after_every_step()
    {
        var (_, ticket, session) = Lock(Picks((0, Side.Away), (1, Side.Home), (2, Side.Away))); // all three win

        while (session.MoveNext(out var evt))
        {
            double expected = evt.WinProbAfter;
            for (int j = evt.LegIndex + 1; j < ticket.Legs.Count; j++)
                expected *= ticket.Legs[j].TrueProb;

            Assert.True(expected == session.TicketWinProbability,
                $"leg {evt.LegIndex} step {evt.Step}/{evt.TotalSteps}: "
                + $"expected {expected:R}, got {session.TicketWinProbability:R}");
        }
    }

    // ---- terminal states ----

    [Fact]
    public void A_revealed_dead_leg_with_no_save_held_drives_it_to_zero()
    {
        // leg 0 loses (matchup 0 Home, away actually wins); no mulligan/whistle is held so the bust
        // is instant.
        var (_, ticket, session) = Lock(Picks((0, Side.Home), (1, Side.Away)));
        Drain(session);

        Assert.True(session.IsComplete);
        Assert.Equal(LegState.Lost, session.RevealedLegState(0));
        Assert.Equal(TicketState.Lost, ticket.State);
        Assert.Equal(0.0, session.TicketWinProbability);
    }

    [Fact]
    public void Every_leg_won_drives_it_to_one_once_complete()
    {
        // WHICH PICKS WIN IS THE ENGINE'S TO SAY, NOT THIS TEST'S. The first version of this test
        // hard-coded a pair commented "both win" — matchup 2's away side in fact LOSES on this seed,
        // so it asserted 1.0 against a busted ticket and failed for a reason that had nothing to do
        // with what it was measuring. Search the board instead and assert relative to the outcome.
        int matchups = new Run(Seed).CurrentSlate.Matchups.Count;
        for (int a = 0; a < matchups; a++)
            for (int b = a + 1; b < matchups; b++)
                foreach (Side sa in new[] { Side.Home, Side.Away })
                    foreach (Side sb in new[] { Side.Home, Side.Away })
                    {
                        var (_, ticket, session) = Lock(Picks((a, sa), (b, sb)));
                        if (ticket.Legs[0].State != LegState.Won || ticket.Legs[1].State != LegState.Won)
                            continue;

                        Drain(session);
                        Assert.True(session.IsComplete);
                        Assert.Equal(LegState.Won, session.RevealedLegState(0));
                        Assert.Equal(LegState.Won, session.RevealedLegState(1));
                        Assert.Equal(1.0, session.TicketWinProbability);
                        return;
                    }

        Assert.Fail("no two-leg ticket on this seed wins both legs; the terminal 1.0 is unproven");
    }

    // ---- same-match ticket ----

    [Fact]
    public void Same_match_ticket_at_start_is_within_a_few_ulp_of_the_sold_price()
    {
        // The nested goal pair (over the higher line entails over the lower one) — pure set
        // containment, so no board change can reach it (SameMatchProbeTests.Catalogue_the_nested_
        // goal_pair_is_an_implication uses the same shape for the same reason).
        var run = new Run("twp-samematch-1");
        RunConfig cfg = run.Config;
        Pick[] picks =
        {
            new Pick(0, MarketSelection.TotalGoals(cfg.GoalLines[1], true)),
            new Pick(0, MarketSelection.TotalGoals(cfg.GoalLines[0], true)),
        };
        Assert.Null(run.RefusalFor(picks));
        Ticket ticket = run.PlaceTicket(picks, 10);
        Assert.NotNull(ticket.SameMatch);

        run.LockRound();
        SweatSession session = run.Sweats[0];

        double diff = System.Math.Abs(session.TicketWinProbability - ticket.SameMatch!.PTicket);
        Assert.True(diff < 1e-9,
            $"expected within 1e-9 of PTicket={ticket.SameMatch!.PTicket:R}, "
            + $"got {session.TicketWinProbability:R} (diff {diff:E3})");
    }

    // ---- the money did not move ----

    [Fact]
    public void Two_leg_ordinary_ticket_matches_live_prob_times_the_other_legs_true_prob_at_several_cursors()
    {
        var (_, ticket, session) = Lock(Picks((0, Side.Away), (2, Side.Away)), 200); // both win
        Leg l0 = ticket.Legs[0], l1 = ticket.Legs[1];

        // t = 0: nothing has happened yet; the live leg is leg 0 at its own TrueProb.
        Assert.True(l0.TrueProb * l1.TrueProb == session.TicketWinProbability,
            $"expected {l0.TrueProb * l1.TrueProb:R}, got {session.TicketWinProbability:R}");

        // Mid-path: a non-final beat of leg 0 moves the live factor; leg 1 still at its own TrueProb.
        Assert.True(session.MoveNext(out var beat));
        double expectedMid = beat!.WinProbAfter * l1.TrueProb;
        Assert.True(expectedMid == session.TicketWinProbability,
            $"expected {expectedMid:R}, got {session.TicketWinProbability:R}");

        // Leg 0 settles Won: the live leg is now leg 1, at its own TrueProb (nothing left after it).
        while (session.RevealedLegState(0) == LegState.Pending)
            session.MoveNext(out _);
        Assert.True(l1.TrueProb == session.TicketWinProbability,
            $"expected {l1.TrueProb:R}, got {session.TicketWinProbability:R}");
    }
}
