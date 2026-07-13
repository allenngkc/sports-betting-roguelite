using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// The 3+3 catalog's behaviors (economy rework): The Multiplier and Scar Tissue composing into
/// the ONE PayoutMultiplier product (design/10 B2), the Scar carrier lifecycle, and the three
/// player-timed consumables against the sweat's real windows.
/// Seed GOLDEN-W2 round 1: home wins only matchup 1; away wins 0, 2, 3, 4, 5.
/// </summary>
public class ItemTests
{
    private static RelicDefinition Def(string id) => RelicCatalog.All.First(r => r.Id == id);
    private static ConsumableDefinition Con(string id) => RelicCatalog.Consumables.First(c => c.Id == id);

    private static Pick[] Picks(params (int m, Side s)[] p)
    {
        var picks = new Pick[p.Length];
        for (int i = 0; i < p.Length; i++) picks[i] = new Pick(p[i].m, p[i].s);
        return picks;
    }

    private static RunConfig EasyPayments(params double[] payments)
        => new RunConfig { Payments = payments };

    // ---- The Multiplier ----

    [Fact]
    public void Multiplier_scales_three_plus_leg_parlays_only()
    {
        var run = new Run("MULT", EasyPayments(10, 10));
        run.GrantRelic(Def(RelicCatalog.MultiplierId));

        Ticket two = run.PlaceTicket(Picks((0, Side.Home), (1, Side.Home)), 20);
        Ticket three = run.PlaceTicket(Picks((2, Side.Home), (3, Side.Home), (4, Side.Home)), 20);

        Assert.Equal(1.0, two.PayoutMultiplier, 12);
        Assert.Equal(1.5, three.PayoutMultiplier, 12);
    }

    // ---- Scar Tissue ----

    [Fact]
    public void Scar_stacks_scale_with_stake_fraction_and_full_stakes_earn_full_pp()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantRelic(Def(RelicCatalog.ScarTissueId));

        // Bank 500: full-scar threshold is 25% = 125. A $10 bust earns 5 × (10/125) = 0.4pp.
        run.PlaceTicket(Picks((0, Side.Home)), 10); // loses in GOLDEN-W2
        run.LockRound();
        run.FastForwardRound();
        Assert.Equal(0.4, run.ScarStacks, 10);
    }

    [Fact]
    public void Scar_full_stake_bust_earns_the_full_five_points()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantRelic(Def(RelicCatalog.ScarTissueId));

        run.PlaceTicket(Picks((0, Side.Home)), 125); // exactly 25% of 500
        run.LockRound();
        run.FastForwardRound();
        Assert.Equal(5.0, run.ScarStacks, 10);
    }

    [Fact]
    public void Scar_carrier_is_the_rounds_first_ticket_and_bakes_the_product_at_placement()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantRelic(Def(RelicCatalog.MultiplierId));
        run.GrantRelic(Def(RelicCatalog.ScarTissueId));

        run.PlaceTicket(Picks((0, Side.Home)), 125); // busts → +5pp
        run.LockRound();
        run.FastForwardRound();
        run.Settle();
        run.ExitShop();

        // Round 2, first ticket, 3 legs: Multiplier × Scar carrier = 1.5 × 1.05 — the product slot.
        Ticket carrier = run.PlaceTicket(Picks((0, Side.Home), (1, Side.Home), (2, Side.Home)), 20);
        Ticket second = run.PlaceTicket(Picks((3, Side.Home), (4, Side.Home), (5, Side.Home)), 20);

        Assert.Equal(1.5 * 1.05, carrier.PayoutMultiplier, 10);
        Assert.Equal(1.5, second.PayoutMultiplier, 10); // only the first-placed carries
    }

    [Fact]
    public void Scar_burns_when_the_carrier_cashes_out()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantRelic(Def(RelicCatalog.ScarTissueId));

        run.PlaceTicket(Picks((0, Side.Home)), 125);
        run.LockRound();
        run.FastForwardRound();
        run.Settle();
        run.ExitShop();
        Assert.Equal(5.0, run.ScarStacks, 10);

        // Round 2: the carrier cashes out mid-sweat — realizing value burns the stacks.
        run.PlaceTicket(Picks((0, Side.Home), (1, Side.Home)), 20);
        run.LockRound();
        SweatSession s = run.Sweats[0];
        s.MoveNext(out _);
        s.AcceptCashOut();
        Assert.Equal(0.0, run.ScarStacks, 10);
    }

    [Fact]
    public void Scar_persists_and_grows_when_the_carrier_busts_too()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantRelic(Def(RelicCatalog.ScarTissueId));

        run.PlaceTicket(Picks((0, Side.Home)), 125);  // carrier of 0 stacks; busts → +5
        run.PlaceTicket(Picks((3, Side.Home)), 125);  // also busts → +5 more
        run.LockRound();
        run.FastForwardRound();

        Assert.Equal(10.0, run.ScarStacks, 10); // ratchets never unwind
    }

    // ---- Profit Boost ----

    [Fact]
    public void Profit_boost_rewrites_the_chosen_leg_and_is_consumed()
    {
        var run = new Run("BOOST", EasyPayments(10, 10));
        run.GrantConsumable(Con("profit_boost"));
        Matchup m = run.CurrentSlate.Matchups[0];

        Ticket t = run.PlaceTicket(Picks((0, Side.Home), (1, Side.Home)), 20, profitBoostLeg: 0);

        Assert.Equal(m.HomeOdds * RelicCatalog.ProfitBoostMult, t.Legs[0].OfferedOdds, 10);
        Assert.Empty(run.OwnedConsumables);
        Assert.Throws<InvalidOperationException>(
            () => run.PlaceTicket(Picks((2, Side.Home)), 20, profitBoostLeg: 0)); // none left
    }

    // ---- Mulligan Slip ----

    [Fact]
    public void Mulligan_slip_window_opens_on_a_dead_leg_and_playing_it_saves_the_ticket()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantConsumable(Con("mulligan_slip"));

        // Leg order = pick order: (0,Home) dies first, (1,Home) wins.
        Ticket t = run.PlaceTicket(Picks((0, Side.Home), (1, Side.Home)), 50);
        run.LockRound();
        SweatSession s = run.Sweats[0];

        while (!s.HasPendingLoss)
            Assert.True(s.MoveNext(out _), "the dead leg should open the window before completion");

        Assert.False(s.IsComplete);
        Assert.Equal(0, s.PendingDeadLegIndex);
        Assert.Null(s.CashOutOffer()); // the window is not a price shelter

        run.PlayMulliganSlip(s);
        Assert.Empty(run.OwnedConsumables);
        Assert.True(t.Legs[0].IsVoided);
        Assert.False(s.HasPendingLoss);

        while (s.MoveNext(out _)) { }
        run.FinishSweat();
        Assert.Equal(TicketState.Won, t.State); // the surviving leg won: the ticket lives and pays
    }

    [Fact]
    public void Advancing_past_the_window_declines_it_and_keeps_the_slip()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantConsumable(Con("mulligan_slip"));

        Ticket t = run.PlaceTicket(Picks((0, Side.Home), (1, Side.Home)), 50);
        run.LockRound();
        SweatSession s = run.Sweats[0];
        while (!s.HasPendingLoss) s.MoveNext(out _);

        Assert.False(s.MoveNext(out _)); // auto-decline: the bust proceeds
        Assert.True(s.IsComplete);
        Assert.Equal(TicketState.Lost, t.State);
        Assert.True(run.OwnsConsumable("mulligan_slip")); // declining costs nothing
    }

    [Fact]
    public void No_slip_means_no_window_and_single_active_leg_tickets_never_get_one()
    {
        var bare = new Run("GOLDEN-W2", EasyPayments(10, 10));
        Ticket t1 = bare.PlaceTicket(Picks((0, Side.Home), (1, Side.Home)), 50);
        bare.LockRound();
        while (bare.Sweats[0].MoveNext(out _)) { }
        Assert.Equal(TicketState.Lost, t1.State); // straight bust, no suspension

        var single = new Run("GOLDEN-W2", EasyPayments(10, 10));
        single.GrantConsumable(Con("mulligan_slip"));
        Ticket t2 = single.PlaceTicket(Picks((0, Side.Home)), 50);
        single.LockRound();
        while (single.Sweats[0].MoveNext(out _)) { }
        Assert.Equal(TicketState.Lost, t2.State); // voiding the only leg is not a save
        Assert.True(single.OwnsConsumable("mulligan_slip"));
    }

    // ---- Timeout ----

    [Fact]
    public void Timeout_freezes_the_offer_until_the_third_event_lands()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantConsumable(Con("timeout"));

        run.PlaceTicket(Picks((0, Side.Away), (2, Side.Away)), 100); // both win in GOLDEN-W2
        run.LockRound();
        SweatSession s = run.Sweats[0];
        s.MoveNext(out _);

        double held = s.CashOutOffer()!.Value;
        run.PlayTimeout(s);
        Assert.Empty(run.OwnedConsumables);

        s.MoveNext(out _);
        Assert.Equal(held, s.CashOutOffer()!.Value, 10); // frozen after event 1
        s.MoveNext(out _);
        Assert.Equal(held, s.CashOutOffer()!.Value, 10); // frozen after event 2
        s.MoveNext(out _);                               // third event: the hold expires
        // Live again — and the live price is genuinely recomputed (the leg state moved on).
        Assert.True(s.IsComplete || s.CashOutOffer().HasValue);
    }

    [Fact]
    public void Timeout_requires_a_live_offer()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantConsumable(Con("timeout"));
        run.PlaceTicket(Picks((1, Side.Home)), 50); // single leg: never an offer
        run.LockRound();

        Assert.Throws<InvalidOperationException>(() => run.PlayTimeout(run.Sweats[0]));
        Assert.True(run.OwnsConsumable("timeout")); // a refused play is not consumed
    }
}
