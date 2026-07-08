using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Engine.Tests;

// Seed GOLDEN-W2's round-1 outcomes are fixed and known: home wins only matchup 1; away wins
// matchups 0, 2, 3, 4, 5. Picks below are chosen against that to script wins and losses.
public class SweatSessionTests
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

    // ---- reveal semantics ----

    [Fact]
    public void Later_legs_read_pending_while_an_earlier_leg_is_mid_path()
    {
        var (_, ticket, session) = Lock(Picks((0, Side.Away), (2, Side.Away))); // both win

        Assert.True(session.MoveNext(out _)); // first beat of leg 0
        Assert.Equal(LegState.Pending, session.RevealedLegState(0));
        Assert.Equal(LegState.Pending, session.RevealedLegState(1));

        // Engine truth for leg 1 is already decided at lock, but the session must not leak it.
        Assert.Equal(LegState.Won, ticket.Legs[1].State);
        Assert.Equal(LegState.Pending, session.RevealedLegState(1));

        // Drain leg 0 to its LegFinal: leg 0 becomes revealed Won, leg 1 stays pending.
        while (session.RevealedLegState(0) == LegState.Pending)
            session.MoveNext(out _);
        Assert.Equal(LegState.Won, session.RevealedLegState(0));
        Assert.Equal(LegState.Pending, session.RevealedLegState(1));
    }

    [Fact]
    public void A_dead_leg_stops_the_session_and_settles_the_ticket_lost()
    {
        // leg 0 loses (matchup 0 Home); legs 1 and 2 must never be played out.
        var (_, ticket, session) = Lock(Picks((0, Side.Home), (1, Side.Home), (2, Side.Home)));
        List<DramaEvent> events = Drain(session);

        Assert.True(session.IsComplete);
        Assert.Equal(LegState.Lost, session.RevealedLegState(0));
        Assert.Equal(LegState.Pending, session.RevealedLegState(1));
        Assert.Equal(LegState.Pending, session.RevealedLegState(2));
        Assert.All(events, e => Assert.Equal(0, e.LegIndex)); // only leg 0 ever emitted
        Assert.Equal(TicketState.Lost, ticket.State);
    }

    [Fact]
    public void MoveNext_returns_false_once_complete()
    {
        var (_, _, session) = Lock(Picks((1, Side.Home))); // single, wins
        Drain(session);
        Assert.True(session.IsComplete);
        Assert.False(session.MoveNext(out _));
    }

    [Fact]
    public void Revealed_leg_state_rejects_out_of_range()
    {
        var (_, _, session) = Lock(Picks((0, Side.Away), (2, Side.Away)));
        Assert.Throws<ArgumentOutOfRangeException>(() => session.RevealedLegState(2));
    }

    // ---- cash-out availability ----

    [Fact]
    public void Single_leg_tickets_never_offer_cash_out()
    {
        var (_, _, session) = Lock(Picks((1, Side.Home))); // single
        Assert.Null(session.CashOutFair());
        Assert.Null(session.CashOutOffer());

        session.MoveNext(out _); // still single-leg after stepping
        Assert.Null(session.CashOutOffer());
    }

    [Fact]
    public void Cash_out_is_gone_after_a_revealed_loss()
    {
        var (_, _, session) = Lock(Picks((0, Side.Home), (1, Side.Home))); // leg 0 loses
        Assert.NotNull(session.CashOutOffer()); // available before anything resolves
        Drain(session);
        Assert.Null(session.CashOutOffer());
    }

    [Fact]
    public void Cash_out_is_gone_once_the_session_completes()
    {
        var (_, _, session) = Lock(Picks((0, Side.Away), (2, Side.Away))); // both win → completes
        Drain(session);
        Assert.True(session.IsComplete);
        Assert.Null(session.CashOutOffer());
    }

    // ---- cash-out pricing (hand-computed from the emitted live probabilities) ----

    [Fact]
    public void Fair_value_at_the_start_prices_every_leg_at_true_prob_times_odds()
    {
        var (_, ticket, session) = Lock(Picks((0, Side.Away), (2, Side.Away)), 200);
        Leg l0 = ticket.Legs[0], l1 = ticket.Legs[1];

        double expected = OddsMath.CashOutFair(
            200, 1.0, new[] { (l0.TrueProb, l0.OfferedOdds), (l1.TrueProb, l1.OfferedOdds) });
        Assert.Equal(expected, session.CashOutFair()!.Value, 10);
    }

    [Fact]
    public void Fair_value_tracks_the_current_legs_live_probability()
    {
        var (_, ticket, session) = Lock(Picks((0, Side.Away), (2, Side.Away)), 200);
        Leg l0 = ticket.Legs[0], l1 = ticket.Legs[1];

        Assert.True(session.MoveNext(out var first)); // a non-final beat of leg 0
        double expected = OddsMath.CashOutFair(
            200, 1.0, new[] { (first!.WinProbAfter, l0.OfferedOdds), (l1.TrueProb, l1.OfferedOdds) });
        Assert.Equal(expected, session.CashOutFair()!.Value, 10);
    }

    [Fact]
    public void Fair_value_after_a_won_leg_banks_its_odds_and_prices_the_rest()
    {
        var (_, ticket, session) = Lock(Picks((0, Side.Away), (2, Side.Away)), 200);
        Leg l0 = ticket.Legs[0], l1 = ticket.Legs[1];

        // Advance through leg 0's LegFinal (Won); the session now sits at the start of leg 1.
        while (session.RevealedLegState(0) == LegState.Pending)
            session.MoveNext(out _);

        double expected = OddsMath.CashOutFair(
            200, l0.OfferedOdds, new[] { (l1.TrueProb, l1.OfferedOdds) });
        Assert.Equal(expected, session.CashOutFair()!.Value, 10);
    }

    [Fact]
    public void Offer_is_fair_value_less_the_margin()
    {
        var (run, _, session) = Lock(Picks((0, Side.Away), (2, Side.Away)));
        double fair = session.CashOutFair()!.Value;
        double expected = OddsMath.CashOutOffer(fair, run.Config.CashOutMargin);
        Assert.Equal(expected, session.CashOutOffer()!.Value, 12);
    }

    // ---- accepting the offer ----

    [Fact]
    public void Accepting_credits_exactly_the_offer_and_closes_the_ticket()
    {
        var (run, ticket, session) = Lock(Picks((0, Side.Away), (2, Side.Away)), 200);
        session.MoveNext(out _);
        session.MoveNext(out _);

        double bankBefore = run.Bank;
        double offer = session.CashOutOffer()!.Value;
        session.AcceptCashOut();

        Assert.Equal(bankBefore + offer, run.Bank, 10);
        Assert.Equal(TicketState.CashedOut, ticket.State);
        Assert.True(session.IsComplete);
        Assert.Null(session.CashOutOffer());
    }

    [Fact]
    public void Finishing_the_sweat_does_not_double_pay_a_cashed_out_ticket()
    {
        var (run, ticket, session) = Lock(Picks((0, Side.Away), (2, Side.Away)), 200);
        session.MoveNext(out _);
        session.AcceptCashOut();
        double bankAfterCashOut = run.Bank;

        run.FinishSweat();
        Assert.Equal(bankAfterCashOut, run.Bank, 12); // no second credit
        Assert.Equal(TicketState.CashedOut, ticket.State);
    }

    [Fact]
    public void Accepting_with_no_offer_available_throws()
    {
        var (_, _, single) = Lock(Picks((1, Side.Home))); // single-leg → never an offer
        Assert.Throws<InvalidOperationException>(() => single.AcceptCashOut());
    }

    [Fact]
    public void Live_effect_seam_is_present_but_unwired_in_v0()
    {
        var (_, _, session) = Lock(Picks((0, Side.Away), (2, Side.Away)));
        Assert.Throws<NotSupportedException>(() => session.ApplyLiveEffect(new object()));
    }
}
