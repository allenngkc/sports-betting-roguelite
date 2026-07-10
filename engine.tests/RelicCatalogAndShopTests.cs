using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

// Catalog shape, the between-rounds shop, and the compose-time relics. Seeds (config RelicKit.Cheap)
// and their round-1 shop offers, re-scanned 2026-07-09 after the info axis (tout_sheet, sharp_eye)
// was cut and the catalog shrank to 8 (the Fisher-Yates offer sequences all shifted):
//   R3-0 [high_roller, promo_code, mulligan]
//   R3-1 [piggy_bank, lucky_charm, bankroll_insurance]
//   R3-3 [promo_code, early_payout, mulligan]
//   R3-5 [promo_code, lucky_charm, boosted_odds]
public class RelicCatalogAndShopTests
{
    // ---- catalog ----

    [Fact]
    public void Catalog_has_eight_relics_with_unique_ids()
    {
        // 10 → 8 on 2026-07-09: the information axis (tout_sheet, sharp_eye) is parked until v2.
        Assert.Equal(8, RelicCatalog.All.Count);
        var ids = RelicCatalog.All.Select(r => r.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
        Assert.DoesNotContain("tout_sheet", ids);
        Assert.DoesNotContain("sharp_eye", ids);
    }

    [Fact]
    public void Catalog_prices_are_in_band_and_metadata_is_present()
    {
        foreach (RelicDefinition r in RelicCatalog.All)
        {
            Assert.InRange(r.Price, 150.0, 400.0);
            Assert.NotNull(r.Params);
            Assert.False(string.IsNullOrWhiteSpace(r.Name));
            Assert.False(string.IsNullOrWhiteSpace(r.Description));
            Assert.False(string.IsNullOrWhiteSpace(r.Axis));
            Assert.False(string.IsNullOrWhiteSpace(r.Op));
        }
    }

    // ---- shop ----

    [Fact]
    public void Shop_offers_three_distinct_unowned_relics()
    {
        Run run = RelicKit.Round1ToShop("R3-0", RelicKit.Cheap());
        Assert.Equal(3, run.ShopOffers.Count);
        Assert.Equal(3, run.ShopOffers.Select(o => o.Id).Distinct().Count());
        Assert.All(run.ShopOffers, o => Assert.Contains(o, RelicCatalog.All));
    }

    [Fact]
    public void Shop_offers_are_deterministic_for_a_seed()
    {
        Run a = RelicKit.Round1ToShop("R3-0", RelicKit.Cheap());
        Assert.Equal(new[] { "high_roller", "promo_code", "mulligan" }, a.ShopOffers.Select(o => o.Id).ToArray());

        Run b = RelicKit.Round1ToShop("R3-0", RelicKit.Cheap());
        Assert.Equal(a.ShopOffers.Select(o => o.Id), b.ShopOffers.Select(o => o.Id));
    }

    [Fact]
    public void Buying_deducts_price_appends_in_acquisition_order_and_removes_the_offer()
    {
        Run run = RelicKit.Round1ToShop("R3-1", RelicKit.Cheap());
        double bank0 = run.Bank;

        RelicDefinition first = run.ShopOffers[1]; // lucky_charm
        run.BuyRelic(1);
        Assert.Equal(bank0 - first.Price, run.Bank, 9);
        Assert.Equal(2, run.ShopOffers.Count);
        Assert.DoesNotContain(first, run.ShopOffers);
        Assert.Single(run.OwnedRelics);
        Assert.Same(first, run.OwnedRelics[0]);

        RelicDefinition second = run.ShopOffers[0];
        run.BuyRelic(0);
        Assert.Equal(2, run.OwnedRelics.Count);
        Assert.Same(second, run.OwnedRelics[1]); // appended after the first
    }

    [Fact]
    public void BuyRelic_rejects_wrong_phase()
    {
        var run = new Run("R3-0", RelicKit.Cheap()); // Betting, not Shop
        Assert.Throws<System.InvalidOperationException>(() => run.BuyRelic(0));
    }

    [Fact]
    public void BuyRelic_rejects_a_bad_index()
    {
        Run run = RelicKit.Round1ToShop("R3-0", RelicKit.Cheap());
        Assert.Throws<System.ArgumentOutOfRangeException>(() => run.BuyRelic(-1));
        Assert.Throws<System.ArgumentOutOfRangeException>(() => run.BuyRelic(3));
    }

    [Fact]
    public void BuyRelic_rejects_insufficient_bank()
    {
        // Bank 100 is below every catalog price (the cheapest relic is 150).
        Run run = RelicKit.Round1ToShop("R3-0", RelicKit.Cheap(bank: 100));
        Assert.Throws<System.InvalidOperationException>(() => run.BuyRelic(0));
    }

    [Fact]
    public void A_sixth_relic_is_rejected_when_all_slots_are_full()
    {
        var cfg = new RunConfig { StartingBank = 100000, Targets = Enumerable.Repeat(1.0, 12).ToArray() };
        var run = new Run("R3-0", cfg);
        for (int i = 0; i < 5; i++)
        {
            run.LockRound(); run.FastForwardRound(); run.Settle();
            Assert.Equal(Phase.Shop, run.Phase);
            run.BuyRelic(0); // each shop excludes owned, so this is a new relic every round
            run.ExitShop();
        }
        Assert.Equal(5, run.OwnedRelics.Count);

        run.LockRound(); run.FastForwardRound(); run.Settle();
        Assert.NotEmpty(run.ShopOffers);
        Assert.Throws<System.InvalidOperationException>(() => run.BuyRelic(0)); // slots full
    }

    // ---- boosted_odds ----

    [Fact]
    public void Boosted_odds_multiplies_only_leg_zero_and_feeds_vig_and_payout()
    {
        Run run = RelicKit.Round2Owning("R3-5", RelicKit.Cheap(), "boosted_odds");
        Ticket t = run.PlaceTicket(RelicKit.Picks((0, Side.Home), (1, Side.Home)), 100);

        Assert.Equal(t.Legs[0].BaseOdds * 1.15, t.Legs[0].OfferedOdds, 9);
        Assert.Equal(t.Legs[1].BaseOdds, t.Legs[1].OfferedOdds, 12); // untouched

        double offered = t.Legs[0].OfferedOdds * t.Legs[1].OfferedOdds;
        double fair = 1.0 / (t.Legs[0].TrueProb * t.Legs[1].TrueProb);
        Assert.Equal(OddsMath.VigPaid(100, offered, fair), t.VigPaid, 9); // vig computed AFTER the boost
        Assert.Equal(100 * offered, t.PotentialPayout, 9);
    }

    // ---- promo_code ----

    [Fact]
    public void Promo_code_prices_the_first_ticket_fair_and_leaves_the_second_alone()
    {
        Run run = RelicKit.Round2Owning("R3-3", RelicKit.Cheap(), "promo_code");

        Ticket first = run.PlaceTicket(RelicKit.Picks((0, Side.Home), (1, Side.Away)), 100);
        foreach (Leg leg in first.Legs)
            Assert.Equal(OddsMath.FairDecimal(leg.TrueProb), leg.OfferedOdds, 9);
        Assert.Equal(0.0, first.VigPaid, 6); // fair odds → no vig

        Ticket second = run.PlaceTicket(RelicKit.Picks((2, Side.Home)), 100);
        Assert.Equal(second.Legs[0].BaseOdds, second.Legs[0].OfferedOdds, 12);
        Assert.True(second.VigPaid > 0);
    }

    [Fact]
    public void Promo_code_makes_each_rounds_first_ticket_fair_again()
    {
        var cfg = RelicKit.Cheap();
        Run run = RelicKit.Round2Owning("R3-3", cfg, "promo_code");
        run.PlaceTicket(RelicKit.Picks((0, Side.Home)), 100);
        run.LockRound(); run.FastForwardRound(); run.Settle(); run.ExitShop(); // round 3

        Ticket first = run.PlaceTicket(RelicKit.Picks((0, Side.Home), (1, Side.Home)), 100);
        foreach (Leg leg in first.Legs)
            Assert.Equal(OddsMath.FairDecimal(leg.TrueProb), leg.OfferedOdds, 9);
    }

    // ---- ordering: promo vs boost resolve in acquisition order ----

    [Fact]
    public void Compose_effects_resolve_in_acquisition_order()
    {
        var cfg = RelicKit.Cheap();

        // Order A: promo THEN boost — promo sets leg 0 fair, boost then multiplies it by 1.15.
        Run a = RelicKit.Round2Owning("R3-5", cfg, "promo_code", "boosted_odds");
        Ticket ta = a.PlaceTicket(RelicKit.Picks((0, Side.Home), (1, Side.Home)), 100);
        Assert.Equal(1.15 * OddsMath.FairDecimal(ta.Legs[0].TrueProb), ta.Legs[0].OfferedOdds, 9);
        Assert.Equal(OddsMath.FairDecimal(ta.Legs[1].TrueProb), ta.Legs[1].OfferedOdds, 9);

        // Order B: boost THEN promo — promo overwrites leg 0 back to fair, erasing the boost.
        Run b = RelicKit.Round2Owning("R3-5", cfg, "boosted_odds", "promo_code");
        Ticket tb = b.PlaceTicket(RelicKit.Picks((0, Side.Home), (1, Side.Home)), 100);
        Assert.Equal(OddsMath.FairDecimal(tb.Legs[0].TrueProb), tb.Legs[0].OfferedOdds, 9);

        Assert.True(System.Math.Abs(ta.Legs[0].OfferedOdds - tb.Legs[0].OfferedOdds) > 1e-6);
    }

    // ---- stakes uncapped (2026-07-08) + high_roller's all-in payout bonus ----

    [Fact]
    public void Stakes_are_uncapped_up_to_the_whole_bank()
    {
        var cfg = new RunConfig { StartingBank = 500 };
        var allIn = new Run("R3-0", cfg);
        allIn.PlaceTicket(RelicKit.Picks((0, Side.Home)), 500);
        Assert.Equal(0, allIn.Bank, 10);

        var over = new Run("R3-0", new RunConfig { StartingBank = 500 });
        Assert.Throws<System.ArgumentException>(() => over.PlaceTicket(RelicKit.Picks((0, Side.Home)), 501));
    }

    [Fact]
    public void High_roller_boosts_payout_only_when_staking_at_least_half_the_bank()
    {
        Run hr = RelicKit.Round2Owning("R3-0", RelicKit.Cheap(), "high_roller");
        double bank = hr.Bank;

        Ticket big = hr.PlaceTicket(RelicKit.Picks((0, Side.Home)), 0.5 * bank);
        Assert.Equal(1.15, big.PayoutMultiplier, 12);
        Assert.Equal(0.5 * bank * big.Legs[0].OfferedOdds * 1.15, big.PotentialPayout, 8);

        Ticket small = hr.PlaceTicket(RelicKit.Picks((1, Side.Home)), 10); // far below half the reduced bank
        Assert.Equal(1.0, small.PayoutMultiplier, 12);
        Assert.Equal(10 * small.Legs[0].OfferedOdds, small.PotentialPayout, 8);
    }

    [Fact]
    public void High_roller_bonus_scales_the_cash_out_fair_value()
    {
        Run hr = RelicKit.Round2Owning("R3-0", RelicKit.Cheap(), "high_roller");
        Ticket t = hr.PlaceTicket(RelicKit.Picks((0, Side.Home), (1, Side.Home)), 0.5 * hr.Bank);
        hr.LockRound();

        double expected = 1.15 * OddsMath.CashOutFair(t.Stake, 1.0, new[]
        {
            (t.Legs[0].TrueProb, t.Legs[0].OfferedOdds),
            (t.Legs[1].TrueProb, t.Legs[1].OfferedOdds),
        });
        Assert.Equal(expected, hr.Sweats[0].CashOutFair()!.Value, 8);
    }

    [Fact]
    public void Without_high_roller_all_in_tickets_get_no_bonus()
    {
        var run = new Run("R3-0", new RunConfig { StartingBank = 500 });
        Ticket t = run.PlaceTicket(RelicKit.Picks((0, Side.Home)), 500);
        Assert.Equal(1.0, t.PayoutMultiplier, 12);
    }
}
