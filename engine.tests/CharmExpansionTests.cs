using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// The charm-expansion test matrix (PLAN.md rev 5 §19): the 17 new items, the deci-comp
/// accounting, the generalized pending window + Ref's Whistle, the dealt-hand shop, the
/// legality matrix, and the determinism pins. Behavior-parameter tests construct custom
/// RelicDefinitions where slate odds would otherwise make fixtures fragile; catalog numbers
/// are pinned separately in <see cref="Catalog_params_pin"/>.
/// Seed GOLDEN-W2 round 1: home wins only matchup 1; away wins 0, 2, 3, 4, 5.
/// </summary>
public class CharmExpansionTests
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
        => new RunConfig { Payments = payments, StartingBank = 500 };

    private static RelicDefinition Custom(string id, string op, params (string k, double v)[] p)
    {
        var bag = new Dictionary<string, double>();
        foreach ((string k, double v) in p) bag[k] = v;
        return new RelicDefinition(id, id, "test", "Test", op, 1, bag);
    }

    // ---------------------------------------------------------------- catalog pins

    [Fact]
    public void Catalog_params_pin()
    {
        // Values as tuned by the charm campaign (sim-report-4); the pin catches accidental drift.
        Assert.Equal(1.50, Def("chalk_eater").Params["maxOdds"], 10);
        Assert.Equal(3.00, Def("longshot_photo").Params["minOdds"], 10);
        Assert.Equal(1.08, Def("golden_parachute").Params["scale"], 10);
        Assert.True(Def("golden_parachute").Params["scale"] <= 1.087, "Parachute above the margin reciprocal");
        Assert.Equal(0.10, Def("rakes_rebate").Params["rate"], 10);
        Assert.Equal(1.15, Def("house_key").Params["paymentFactor"], 10);
        Assert.Equal(2.0, Def("bobblehead").Params["resaleMult"], 10);
        Assert.Equal(2.0, RelicCatalog.DoubleOrNothingMult, 10);
        Assert.Equal(0.25, RelicCatalog.MarkerRelief, 10);
    }

    // ---------------------------------------------------------------- deci-comps

    [Fact]
    public void Comps_accrual_commits_once_at_lock_split_vs_combined_invariant()
    {
        var a = new Run("COMPS-A", EasyPayments(10, 10));
        a.PlaceTicket(Picks((0, Side.Away)), 100);
        Assert.Equal(0.0, a.Comps, 10); // pooled raw — invisible until the lock commit
        a.LockRound();
        Assert.Equal(12.0, a.Comps, 10); // $100 × 0.12/dollar (tuned rate)

        var b = new Run("COMPS-A", EasyPayments(10, 10));
        b.PlaceTicket(Picks((0, Side.Away)), 50);
        b.PlaceTicket(Picks((1, Side.Home)), 50);
        b.LockRound();
        Assert.Equal(a.Comps, b.Comps, 10); // split vs combined: identical by construction
    }

    [Fact]
    public void Comps_interest_and_grants_move_exact_tenths()
    {
        var run = new Run("COMPS-B", EasyPayments(10, 10));
        run.GrantComps(1.55); // AwayFromZero → 1.6
        Assert.Equal(1.6, run.Comps, 10);
        run.ApplyCompsInterest(0.10); // 16 deci × 0.10 = 1.6 → 2 deci
        Assert.Equal(1.8, run.Comps, 10);
    }

    // ---------------------------------------------------------------- chalk eater

    [Fact]
    public void Chalk_winds_on_revealed_won_legs_only_and_factors_next_lock()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantRelic(Custom("chalk_eater", "LegBandRatchet", ("maxOdds", 10.0), ("ppPerLeg", 1.0)));

        run.PlaceTicket(Picks((0, Side.Away), (2, Side.Away)), 20); // both win
        run.PlaceTicket(Picks((1, Side.Away)), 10);                 // loses
        run.LockRound();
        run.FastForwardRound();
        run.Settle();
        run.ExitShop();

        // 2 revealed wins wound the ratchet; the loss did not.
        Ticket next = run.PlaceTicket(Picks((0, Side.Home)), 10);
        run.LockRound();
        Assert.Equal(1.02, next.PayoutMultiplier, 10);
    }

    [Fact]
    public void Chalk_ignores_legs_outside_the_band_and_legs_unrevealed_at_cash_out()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantRelic(Custom("chalk_eater", "LegBandRatchet", ("maxOdds", 10.0), ("ppPerLeg", 1.0)));

        run.PlaceTicket(Picks((0, Side.Away), (2, Side.Away)), 20);
        run.LockRound();
        SweatSession s = run.Sweats[0];
        // Sweat only leg 0 to its reveal, then cash out — leg 1 stays unrevealed forever.
        while (s.RevealedLegState(0) == LegState.Pending) s.MoveNext(out _);
        s.AcceptCashOut();
        run.FinishSweat();
        run.Settle();
        run.ExitShop();

        Ticket next = run.PlaceTicket(Picks((0, Side.Home)), 10);
        run.LockRound();
        Assert.Equal(1.01, next.PayoutMultiplier, 10); // exactly ONE wind

        var banded = new Run("GOLDEN-W2", EasyPayments(10, 10));
        banded.GrantRelic(Custom("chalk_eater", "LegBandRatchet", ("maxOdds", 0.5), ("ppPerLeg", 1.0)));
        banded.PlaceTicket(Picks((0, Side.Away)), 10);
        banded.LockRound();
        banded.FastForwardRound();
        banded.Settle();
        banded.ExitShop();
        Ticket t2 = banded.PlaceTicket(Picks((0, Side.Home)), 10);
        banded.LockRound();
        Assert.Equal(1.0, t2.PayoutMultiplier, 10); // no leg fits a 0.5 band
    }

    // ---------------------------------------------------------------- longshot photo

    [Fact]
    public void Photo_factors_at_lock_and_prices_into_the_cash_out()
    {
        var with = new Run("GOLDEN-W2", EasyPayments(10, 10));
        with.GrantRelic(Custom("longshot_photo", "LegBandProductFlag", ("minOdds", 1.0), ("mult", 1.6)));
        Ticket t = with.PlaceTicket(Picks((0, Side.Away), (2, Side.Away)), 20);
        with.LockRound();
        Assert.Equal(1.6, t.PayoutMultiplier, 10);

        var without = new Run("GOLDEN-W2", EasyPayments(10, 10));
        Ticket u = without.PlaceTicket(Picks((0, Side.Away), (2, Side.Away)), 20);
        without.LockRound();

        with.Sweats[0].MoveNext(out _);
        without.Sweats[0].MoveNext(out _);
        double a = with.Sweats[0].CashOutOffer()!.Value;
        double b = without.Sweats[0].CashOutOffer()!.Value;
        Assert.Equal(1.6, a / b, 6); // design/02: the quote prices the full payoff function
    }

    [Fact]
    public void Photo_drops_when_the_last_qualifying_leg_is_voided()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        // F_0.4.0 re-pin: qualify ONLY the leg that dies (matchup 1, Home side loses).
        // Precondition: its odds exceed the surviving leg's odds by construction of the band below.
        Ticket probe = null!;
        {
            var scout = new Run("GOLDEN-W2", EasyPayments(10, 10));
            probe = scout.PlaceTicket(Picks((1, Side.Home), (0, Side.Away)), 20);
        }
        double dyingOdds = probe.Legs[0].OfferedOdds;
        double survivorOdds = probe.Legs[1].OfferedOdds;
        Assert.True(dyingOdds > survivorOdds,
            $"fixture drift: dying {dyingOdds} must out-price survivor {survivorOdds}");

        run.GrantRelic(Custom("longshot_photo", "LegBandProductFlag",
            ("minOdds", dyingOdds - 1e-9), ("mult", 1.6)));
        run.GrantConsumable(Con("mulligan_slip"));

        Ticket t = run.PlaceTicket(Picks((1, Side.Home), (0, Side.Away)), 20);
        run.LockRound();
        Assert.Equal(1.6, t.PayoutMultiplier, 10);
        Assert.Equal(LegState.Lost, t.Legs[0].State); // fixture drift: the dying leg must die, or the wait below never ends

        SweatSession s = run.Sweats[0];
        while (!s.HasPendingLoss) s.MoveNext(out _); // matchup 1 Home reveals dead
        run.PlayMulliganSlip(s);                     // void strips the only qualifying leg

        Assert.Equal(1.0, t.PayoutMultiplier, 10);   // the photo factor toggled off — only it
    }

    // ---------------------------------------------------------------- iron hands

    [Fact]
    public void Iron_hands_stacks_full_ride_wins_and_any_cash_out_resets()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10, 10, 10, 10));
        run.GrantRelic(Def("iron_hands"));

        run.PlaceTicket(Picks((1, Side.Away)), 10); // wins at full ride (F_0.4.0 universe)
        run.LockRound();
        run.FastForwardRound();
        run.Settle();
        run.ExitShop();

        Ticket second = run.PlaceTicket(Picks((0, Side.Home)), 10);
        run.LockRound();
        Assert.Equal(1.04, second.PayoutMultiplier, 10); // +4pp after one full-ride win

        // Round 2 also holds a 2-leg winner we cash out — the stack shatters.
        run.FastForwardRound();
        run.Settle();
        run.ExitShop();

        run.PlaceTicket(Picks((0, Side.Away), (2, Side.Away)), 20);
        run.LockRound();
        SweatSession s = run.Sweats[0];
        s.MoveNext(out _);
        s.AcceptCashOut();
        run.FinishSweat();
        run.Settle();
        run.ExitShop();

        Ticket after = run.PlaceTicket(Picks((0, Side.Home)), 10);
        run.LockRound();
        Assert.Equal(1.0, after.PayoutMultiplier, 10); // reset by the cash-out
    }

    // ---------------------------------------------------------------- golden parachute

    [Fact]
    public void Parachute_scales_the_offer_and_the_credit_together()
    {
        var plain = new Run("GOLDEN-W2", EasyPayments(10, 10));
        var chute = new Run("GOLDEN-W2", EasyPayments(10, 10));
        chute.GrantRelic(Def("golden_parachute"));

        foreach (Run r in new[] { plain, chute })
        {
            r.PlaceTicket(Picks((0, Side.Away), (2, Side.Away)), 100);
            r.LockRound();
            r.Sweats[0].MoveNext(out _);
        }

        double offerPlain = plain.Sweats[0].CashOutOffer()!.Value;
        double offerChute = chute.Sweats[0].CashOutOffer()!.Value;
        Assert.Equal(1.08, offerChute / offerPlain, 8);

        double bankBefore = chute.Bank;
        chute.Sweats[0].AcceptCashOut();
        Assert.Equal(offerChute, chute.Bank - bankBefore, 8); // credit == quoted offer
    }

    // ---------------------------------------------------------------- rake's rebate + manager

    [Fact]
    public void Rebate_pays_on_shop_entry_but_never_on_a_manager_redeal()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantRelic(Def("rakes_rebate"));
        run.GrantComps(100);
        run.GrantConsumable(Con("ask_manager"));

        run.LockRound();
        run.FastForwardRound();
        run.Settle(); // shop entry → +10%
        Assert.Equal(110.0, run.Comps, 10);

        run.PlayAskManager(); // redeal only — no second interest payment
        Assert.Equal(110.0, run.Comps, 10);
    }

    [Fact]
    public void Manager_is_once_per_visit_and_leaves_future_deals_untouched()
    {
        var a = new Run("SHOP-DET", EasyPayments(10, 10, 10));
        var b = new Run("SHOP-DET", EasyPayments(10, 10, 10));
        a.GrantConsumable(Con("ask_manager"));
        a.GrantConsumable(Con("ask_manager"));

        foreach (Run r in new[] { a, b }) { r.LockRound(); r.FastForwardRound(); r.Settle(); }

        a.PlayAskManager();
        Assert.Throws<InvalidOperationException>(() => a.PlayAskManager()); // the visit latch

        foreach (Run r in new[] { a, b }) { r.ExitShop(); r.LockRound(); r.FastForwardRound(); r.Settle(); }

        // The redeal drew from a DERIVED stream: the next visit's hands are identical.
        Assert.Equal(b.ShopOffers.Select(o => o.Id), a.ShopOffers.Select(o => o.Id));
        Assert.Equal(b.ConsumableOffers.Select(o => o.Id), a.ConsumableOffers.Select(o => o.Id));
    }

    // ---------------------------------------------------------------- whale card

    [Fact]
    public void Whale_snapshots_the_committed_balance_at_lock()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantRelic(Def("whale_card"));
        run.GrantComps(100);

        Ticket t = run.PlaceTicket(Picks((0, Side.Away)), 10);
        run.LockRound(); // accrual ($10 × 0.12 = 1.2) commits first → snapshot sees 101.2
        Assert.Equal(1.0 + 0.005 * run.Comps, t.PayoutMultiplier, 10);
        Assert.Equal(101.2, run.Comps, 10);
    }

    // ---------------------------------------------------------------- bad beat jar

    [Fact]
    public void Jar_counts_all_loss_rounds_only_and_refunded_losses_still_count()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10, 10, 10, 10));
        run.GrantRelic(Def("bad_beat_jar"));
        run.GrantConsumable(Con("free_bet"));

        // R1: one ticket, refunded Free Bet loss → the jar still counts the round.
        run.PlaceTicket(Picks((0, Side.Home)), 10, modifier: TicketModifier.FreeBet);
        run.LockRound(); run.FastForwardRound(); run.Settle(); run.ExitShop();

        // R2: a dutch on matchup 0 covering EVERY outcome — one ticket ALWAYS wins, so this round
        // can never qualify no matter what the fresh slate rolled. The draw ticket is not padding:
        // under 1X2 (D1, 2026-08-12) a home/away pair leaves the draw uncovered, and on a drawn
        // match BOTH tickets lose — which made this round qualify and banked a second jar wind.
        Ticket r2 = run.PlaceTicket(Picks((0, Side.Home)), 10);
        run.PlaceTicket(new[] { new Pick(0, MarketSelection.MoneylineDraw()) }, 10);
        run.PlaceTicket(Picks((0, Side.Away)), 10);
        run.LockRound();
        Assert.Equal(1.10, r2.PayoutMultiplier, 10); // +10pp banked from R1 only (tuned)
        run.FastForwardRound(); run.Settle(); run.ExitShop();

        // R3: a cashed-out ticket disqualifies the round regardless of everything else.
        run.PlaceTicket(Picks((1, Side.Away)), 10);
        run.PlaceTicket(Picks((0, Side.Away), (2, Side.Away)), 20);
        run.LockRound();
        SweatSession winner = run.Sweats[1];
        winner.MoveNext(out _);
        winner.AcceptCashOut();
        run.FastForwardRound();
        run.Settle(); run.ExitShop();

        Ticket r4 = run.PlaceTicket(Picks((0, Side.Home)), 10);
        run.LockRound();
        Assert.Equal(1.10, r4.PayoutMultiplier, 10); // unchanged — still exactly one wind
    }

    [Fact]
    public void Jar_ignores_zero_bet_rounds()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10, 10));
        run.GrantRelic(Def("bad_beat_jar"));
        run.LockRound(); run.FastForwardRound(); run.Settle(); run.ExitShop(); // no tickets

        Ticket t = run.PlaceTicket(Picks((0, Side.Home)), 10);
        run.LockRound();
        Assert.Equal(1.0, t.PayoutMultiplier, 10);
    }

    // ---------------------------------------------------------------- house key

    [Fact]
    public void House_key_worked_numbers_pin_totem_surcharge_books_at_base()
    {
        // Codex r2 #3's exact shape: base current 100, base next 200, Key held. Effective
        // current 115 misses on a 90 bank → the totem books 100 × 1.5 = 150 into the BASE next
        // (200 + 150 = 350); the Key reads it as 402.5 through the getter; selling → 350.
        var run = new Run("HOUSEKEY-PIN", new RunConfig
        {
            StartingBank = 90,
            Payments = new double[] { 100, 200 },
            TotemJuiceRate = 0.5,
        });
        run.GrantRelic(Def("house_key"));
        run.GrantRelic(Def(RelicCatalog.TotemId));

        Assert.Equal(115.0, run.CurrentPayment, 10);
        run.LockRound(); run.FastForwardRound(); run.Settle();

        Assert.True(run.LastSettlement!.Value.TotemFired);
        Assert.Equal(90.0, run.Bank, 10);            // deferral: untouched
        Assert.Equal(402.5, run.NextPayment!.Value, 10);

        int keyIndex = -1;
        for (int i = 0; i < run.OwnedRelics.Count; i++)
            if (run.OwnedRelics[i].Id == "house_key") keyIndex = i;
        run.SellRelic(keyIndex);
        Assert.Equal(350.0, run.NextPayment!.Value, 10); // factor gone, surcharge stands
    }

    [Fact]
    public void House_key_multiplies_payouts()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantRelic(Def("house_key"));
        Ticket t = run.PlaceTicket(Picks((1, Side.Home)), 10);
        run.LockRound();
        Assert.Equal(1.4, t.PayoutMultiplier, 10);
    }

    // ---------------------------------------------------------------- the system

    [Fact]
    public void System_streak_builds_on_profit_and_resets_on_flat_or_down()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10, 10, 10));
        run.GrantRelic(Def("the_system"));

        run.PlaceTicket(Picks((0, Side.Away)), 20); // wins → PnL > 0 in the Phase 1 pin
        run.LockRound(); run.FastForwardRound(); run.Settle(); run.ExitShop();

        Ticket r2 = run.PlaceTicket(Picks((0, Side.Away)), 10);
        run.LockRound();
        Assert.Equal(1.12, r2.PayoutMultiplier, 10); // streak 1 at tuned 12pp

        run.FastForwardRound(); run.Settle(); run.ExitShop(); // r2 won → streak 2... unless
        // NOTE: r2 (matchup 0 Away) WINS in round 2? Round 2's slate is fresh — outcome unknown
        // to the fixture, so assert on the streak SEMANTICS instead: a zero-bet round resets.
        run.LockRound(); run.FastForwardRound(); run.Settle(); run.ExitShop(); // zero-bet → reset

        Ticket after = run.PlaceTicket(Picks((0, Side.Home)), 10);
        run.LockRound();
        Assert.Equal(1.0, after.PayoutMultiplier, 10);
    }

    // ---------------------------------------------------------------- comp'd suite

    [Fact]
    public void Suite_pays_comps_on_a_four_leg_win()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantRelic(Def("compd_suite"));

        // Leg set re-selected for the draws universe (D1, 2026-08-12): matchup 4 now finishes a
        // DRAW on this seed, so the old (4, Away) leg lost and the ticket never reached the win
        // this test exists to price. Matchup 3 (Home) replaces it — same seed, same four-leg
        // shape, all four winning again. GOLDEN-W2 now reads m0 Away, m1 Away, m2 Draw, m3 Home,
        // m4 Draw, m5 Away.
        run.PlaceTicket(Picks((0, Side.Away), (1, Side.Away), (3, Side.Home), (5, Side.Away)), 40);
        run.LockRound();
        double afterLock = run.Comps; // accrual committed (4.8)
        run.FastForwardRound();       // all four win → +8 comps at realize
        Assert.Equal(afterLock + 8.0, run.Comps, 10);
    }

    // ---------------------------------------------------------------- bobblehead + collection

    [Fact]
    public void Bobblehead_resells_at_double_and_the_collection_prices_owned_passives()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantRelic(Def("bobblehead"));
        run.GrantRelic(Def("the_collection"));

        Assert.Equal(4.0, run.GetResaleValue(Def("bobblehead")), 10);      // 2 × 2.0 (tuned)
        Assert.Equal(2.5, run.GetResaleValue(Def("the_collection")), 10);  // 5 × 0.5

        Ticket t = run.PlaceTicket(Picks((1, Side.Home)), 10);
        run.LockRound();
        Assert.Equal(1.0 + 0.01 * 6.5, t.PayoutMultiplier, 10); // bobble 4 + itself 2.5
    }

    [Fact]
    public void Collection_counts_a_spent_totem_at_zero()
    {
        var run = new Run("COLLECT-TOTEM", new RunConfig
        {
            StartingBank = 50,
            Payments = new double[] { 100, 10, 10 },
        });
        run.GrantRelic(Def(RelicCatalog.TotemId));
        run.GrantRelic(Def("the_collection"));

        run.LockRound(); run.FastForwardRound(); run.Settle(); // 50 < 100 → totem fires
        Assert.True(run.LastSettlement!.Value.TotemFired);
        Assert.Equal(0.0, run.GetResaleValue(Def(RelicCatalog.TotemId)), 10);
        run.ExitShop();

        Ticket t = run.PlaceTicket(Picks((0, Side.Home)), 10);
        run.LockRound();
        Assert.Equal(1.0 + 0.01 * 2.5, t.PayoutMultiplier, 10); // collection 2.5 only
    }

    // ---------------------------------------------------------------- free bet

    [Fact]
    public void Free_bet_refunds_exactly_once_and_the_scar_still_feeds()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantRelic(Def(RelicCatalog.ScarTissueId));
        run.GrantConsumable(Con("free_bet"));

        double bankBefore = run.Bank;
        Ticket t = run.PlaceTicket(Picks((0, Side.Home), (1, Side.Home)), 50,
            modifier: TicketModifier.FreeBet); // leg 0 dies mid-sweat (early bust path)
        run.LockRound();
        run.FastForwardRound();

        Assert.Equal(TicketState.Lost, t.State);
        Assert.True(t.Refunded);
        Assert.Equal(bankBefore, run.Bank, 10); // stake out, stake back — exactly once
        Assert.True(run.ScarStacks > 0);        // the bust still fed the ratchet
        Assert.False(run.OwnsConsumable("free_bet")); // consumed at placement
    }

    [Fact]
    public void Free_bet_prices_the_loss_side_into_the_cash_out_quote()
    {
        var plain = new Run("GOLDEN-W2", EasyPayments(10, 10));
        var freeb = new Run("GOLDEN-W2", EasyPayments(10, 10));
        freeb.GrantConsumable(Con("free_bet"));

        plain.PlaceTicket(Picks((0, Side.Away), (2, Side.Away)), 100);
        freeb.PlaceTicket(Picks((0, Side.Away), (2, Side.Away)), 100, modifier: TicketModifier.FreeBet);
        plain.LockRound();
        freeb.LockRound();
        plain.Sweats[0].MoveNext(out _);
        freeb.Sweats[0].MoveNext(out _);

        double a = plain.Sweats[0].CashOutOffer()!.Value;
        double b = freeb.Sweats[0].CashOutOffer()!.Value;
        Assert.True(b > a, "the refund leg must be worth something in the quote");
    }

    // ---------------------------------------------------------------- double or nothing

    [Fact]
    public void Don_doubles_the_win_suppresses_cash_out_and_counts_for_iron_hands()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10, 10));
        run.GrantRelic(Def("iron_hands"));
        run.GrantConsumable(Con("double_or_nothing"));

        Ticket t = run.PlaceTicket(Picks((0, Side.Away), (3, Side.Home)), 20,
            modifier: TicketModifier.DoubleOrNothing);
        run.LockRound();
        Assert.Equal(2.0, t.PayoutMultiplier, 10);

        SweatSession s = run.Sweats[0];
        s.MoveNext(out _);
        Assert.Null(s.CashOutOffer()); // no exits, ever
        run.FastForwardRound();        // both legs win
        run.Settle(); run.ExitShop();

        Ticket next = run.PlaceTicket(Picks((0, Side.Home)), 10);
        run.LockRound();
        Assert.Equal(1.04, next.PayoutMultiplier, 10); // a DoN win is a full-ride win
    }

    // ---------------------------------------------------------------- bookie's marker

    [Fact]
    public void Marker_shaves_the_payment_once_per_round()
    {
        var run = new Run("GOLDEN-W2", new RunConfig { Payments = new double[] { 100, 10 } });
        run.GrantConsumable(Con("bookies_marker"));
        run.GrantConsumable(Con("bookies_marker"));

        run.PlayBookiesMarker();
        Assert.Equal(75.0, run.CurrentPayment, 10);
        Assert.Throws<InvalidOperationException>(() => run.PlayBookiesMarker()); // once per round
        Assert.True(run.OwnsConsumable("bookies_marker")); // the second one was not consumed
    }

    // ---------------------------------------------------------------- legality

    [Fact]
    public void Modifier_placement_is_atomic_nothing_consumed_on_a_refused_play()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantConsumable(Con("free_bet"));

        // Profit Boost demanded but not held: the whole placement refuses BEFORE consuming.
        Assert.Throws<InvalidOperationException>(() =>
            run.PlaceTicket(Picks((0, Side.Away)), 10, profitBoostLeg: 0, modifier: TicketModifier.FreeBet));
        Assert.True(run.OwnsConsumable("free_bet"));
        Assert.Empty(run.Tickets);

        Assert.Throws<InvalidOperationException>(() =>
            run.PlaceTicket(Picks((0, Side.Away)), 10, modifier: TicketModifier.DoubleOrNothing));
    }

    // ---------------------------------------------------------------- ref's whistle

    private static bool PredictWhistle(string seed, int round, string ticketId, int legIndex, double prob)
        => new RngHub(seed).Derive(round, ticketId, legIndex, "whistle", 0).NextDouble() < prob;

    [Fact]
    public void Whistle_rescues_at_full_odds_or_busts_honestly_and_repairs_the_session()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantConsumable(Con("refs_whistle"));
        run.GrantConsumable(Con("refs_whistle"));

        // F_0.4.0 re-pin: leg 0 (matchup 1, Home) dies; leg 1 (matchup 0, Away) would win.
        Ticket t = run.PlaceTicket(Picks((1, Side.Home), (0, Side.Away)), 20);
        run.LockRound();
        SweatSession s = run.Sweats[0];
        while (!s.HasPendingLoss) s.MoveNext(out _);

        double captured = s.PendingLossProbBefore;
        Assert.True(captured > 0, "the captured prob must be the pre-kill value, not 0");
        bool expectRescue = PredictWhistle("GOLDEN-W2", 1, t.Id, 0, captured);

        run.PlayRefsWhistle(s);

        if (expectRescue)
        {
            Assert.True(t.Legs[0].GradesWon);                       // full odds, this slip only
            Assert.Equal(LegState.Won, s.RevealedLegState(0));      // the session view repairs
            Assert.False(s.IsComplete);
            s.MoveNext(out _);
            Assert.NotNull(s.CashOutOffer());                       // cash-out back to life
        }
        else
        {
            Assert.Equal(TicketState.Lost, t.State);
            Assert.True(s.IsComplete);
        }
    }

    [Fact]
    public void Whistle_bends_one_slip_never_the_shared_matchup()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        run.GrantConsumable(Con("refs_whistle"));

        Ticket whistled = run.PlaceTicket(Picks((1, Side.Home)), 10); // single-leg: whistle-only window
        Ticket bystander = run.PlaceTicket(Picks((1, Side.Home)), 10);
        run.LockRound();

        SweatSession s = run.Sweats[0];
        while (!s.HasPendingLoss && s.MoveNext(out _)) { }
        Assert.True(s.HasPendingLoss); // opened on a SINGLE-leg ticket (whistle eligibility)
        Assert.False(s.CanMulliganPendingLoss); // ...but a mulligan could never save it

        bool expectRescue = PredictWhistle("GOLDEN-W2", 1, whistled.Id, 0, s.PendingLossProbBefore);
        run.PlayRefsWhistle(s);

        foreach (SweatSession other in new[] { run.Sweats[1] })
            while (other.MoveNext(out _)) { }
        run.FinishSweat();

        Assert.Equal(expectRescue ? TicketState.Won : TicketState.Lost, whistled.State);
        Assert.Equal(TicketState.Lost, bystander.State); // the shared result never bent
        Assert.Equal(MatchResult.Away, whistled.Legs[0].Matchup.Result);
    }

    // ---------------------------------------------------------------- determinism

    [Fact]
    public void Consumable_timing_never_perturbs_the_fixed_universe()
    {
        var quiet = new Run("DERIVE-DET", EasyPayments(10, 10, 10));
        var noisy = new Run("DERIVE-DET", EasyPayments(10, 10, 10));
        noisy.GrantConsumable(Con("ask_manager"));
        noisy.GrantConsumable(Con("refs_whistle"));

        foreach (Run r in new[] { quiet, noisy })
        {
            r.PlaceTicket(Picks((0, Side.Home)), 10);
            r.LockRound();
        }
        // noisy plays its whistle if a window opens; quiet auto-declines.
        SweatSession ns = noisy.Sweats[0];
        while (ns.MoveNext(out _)) { }
        if (ns.HasPendingLoss) noisy.PlayRefsWhistle(ns);
        while (ns.MoveNext(out _)) { }
        foreach (SweatSession s in quiet.Sweats) while (s.MoveNext(out _)) { }

        quiet.FinishSweat(); quiet.Settle();
        noisy.FinishSweat(); noisy.Settle();
        if (noisy.Phase == Phase.Shop && noisy.OwnsConsumable("ask_manager")) noisy.PlayAskManager();
        if (quiet.Phase == Phase.Shop) quiet.ExitShop();
        if (noisy.Phase == Phase.Shop) noisy.ExitShop();

        if (quiet.Phase == Phase.Betting && noisy.Phase == Phase.Betting)
        {
            quiet.LockRound();
            noisy.LockRound();
            for (int m = 0; m < quiet.CurrentSlate.Matchups.Count; m++)
                Assert.Equal(quiet.CurrentSlate.Matchups[m].Result,
                    noisy.CurrentSlate.Matchups[m].Result); // the universe never moved
        }
    }

    // ---------------------------------------------------------------- dealt hand exhaustion

    [Fact]
    public void Short_pools_deal_what_remains()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10));
        foreach (RelicDefinition d in RelicCatalog.All)
            if (d.Id != "bobblehead" && d.Id != "the_collection") run.GrantRelic(d);

        run.LockRound(); run.FastForwardRound(); run.Settle(); // shop
        Assert.Equal(2, run.ShopOffers.Count); // only two unowned remain
    }

    // ---------------------------------------------------------------- stateful reset on sale

    [Fact]
    public void Sold_ratchets_reset_and_reacquisition_starts_fresh()
    {
        var run = new Run("GOLDEN-W2", EasyPayments(10, 10, 10));
        RelicDefinition chalk = Custom("chalk_eater", "LegBandRatchet", ("maxOdds", 10.0), ("ppPerLeg", 1.0));
        run.GrantRelic(chalk);

        run.PlaceTicket(Picks((1, Side.Home)), 10); // wins → 1 stack
        run.LockRound(); run.FastForwardRound(); run.Settle();

        run.SellRelic(0);        // stacks die with the behavior
        run.GrantRelic(chalk);   // fresh instance
        run.ExitShop();

        Ticket t = run.PlaceTicket(Picks((0, Side.Home)), 10);
        run.LockRound();
        Assert.Equal(1.0, t.PayoutMultiplier, 10);
    }
}
