using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

// Catalog shape, the between-rounds shop, and the compose-time / query relics.
// Seeds (config RelicKit.Cheap) and their round-1 shop offers, found by scan:
//   R3-0 [high_roller, early_payout, tout_sheet]
//   R3-1 [sharp_eye, boosted_odds, bankroll_insurance]
//   R3-3 [promo_code, high_roller, boosted_odds]
public class RelicCatalogAndShopTests
{
    // ---- catalog ----

    [Fact]
    public void Catalog_has_ten_relics_with_unique_ids()
    {
        Assert.Equal(10, RelicCatalog.All.Count);
        var ids = RelicCatalog.All.Select(r => r.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
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
        Assert.Equal(new[] { "high_roller", "early_payout", "tout_sheet" }, a.ShopOffers.Select(o => o.Id).ToArray());

        Run b = RelicKit.Round1ToShop("R3-0", RelicKit.Cheap());
        Assert.Equal(a.ShopOffers.Select(o => o.Id), b.ShopOffers.Select(o => o.Id));
    }

    [Fact]
    public void Buying_deducts_price_appends_in_acquisition_order_and_removes_the_offer()
    {
        Run run = RelicKit.Round1ToShop("R3-1", RelicKit.Cheap());
        double bank0 = run.Bank;

        RelicDefinition first = run.ShopOffers[1]; // boosted_odds
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
        // Bank 100 is below every offered price (R3-0's cheapest is 200).
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

    // ---- tout_sheet ----

    [Fact]
    public void Tout_sheet_reveals_two_bands_that_bracket_the_true_home_prob()
    {
        Run run = RelicKit.Round2Owning("R3-0", RelicKit.Cheap(), "tout_sheet");
        Assert.Equal(2, run.Intel.Count);
        Assert.Equal(2, run.Intel.Select(i => i.MatchupIndex).Distinct().Count());
        foreach (MatchupIntel intel in run.Intel)
        {
            Matchup m = run.CurrentSlate.Matchups[intel.MatchupIndex];
            Assert.InRange(m.TrueHomeProb, intel.ProbLow, intel.ProbHigh);
            Assert.Equal(0.10, intel.ProbHigh - intel.ProbLow, 6); // 0.05 each side of truth
        }
    }

    [Fact]
    public void Intel_is_empty_without_the_relic()
    {
        var run = new Run("R3-0", RelicKit.Cheap());
        Assert.Empty(run.Intel); // round 1
        run.LockRound(); run.FastForwardRound(); run.Settle(); run.ExitShop();
        Assert.Empty(run.Intel); // round 2, still no tout_sheet
    }

    [Fact]
    public void Tout_sheet_regenerates_intel_each_round()
    {
        Run run = RelicKit.Round2Owning("R3-0", RelicKit.Cheap(), "tout_sheet");
        Assert.Equal(2, run.Intel.Count);

        run.LockRound(); run.FastForwardRound(); run.Settle(); run.ExitShop(); // round 3
        Assert.Equal(2, run.Intel.Count); // fresh intel for the new slate
        foreach (MatchupIntel intel in run.Intel)
            Assert.InRange(run.CurrentSlate.Matchups[intel.MatchupIndex].TrueHomeProb, intel.ProbLow, intel.ProbHigh);
    }

    // ---- sharp_eye ----

    [Fact]
    public void Sharp_eye_reveals_the_exact_true_probability_of_the_chosen_line()
    {
        Run run = RelicKit.Round2Owning("R3-1", RelicKit.Cheap(), "sharp_eye");
        Matchup m = run.CurrentSlate.Matchups[0];
        Assert.Equal(m.TrueProb(Side.Home), run.QuerySharpEye(0, Side.Home), 12);
    }

    [Fact]
    public void Sharp_eye_reveals_the_away_side_as_the_complement()
    {
        Run run = RelicKit.Round2Owning("R3-1", RelicKit.Cheap(), "sharp_eye");
        Matchup m = run.CurrentSlate.Matchups[3];
        Assert.Equal(1.0 - m.TrueHomeProb, run.QuerySharpEye(3, Side.Away), 12);
    }

    [Fact]
    public void Sharp_eye_throws_when_not_owned_out_of_phase_or_out_of_charges()
    {
        var plain = new Run("R3-1", RelicKit.Cheap());
        Assert.Throws<System.InvalidOperationException>(() => plain.QuerySharpEye(0, Side.Home)); // not owned

        Run wrongPhase = RelicKit.Round2Owning("R3-1", RelicKit.Cheap(), "sharp_eye");
        wrongPhase.PlaceTicket(RelicKit.Picks((0, Side.Home)), 50);
        wrongPhase.LockRound(); // now Sweat
        Assert.Throws<System.InvalidOperationException>(() => wrongPhase.QuerySharpEye(0, Side.Home));

        Run noCharge = RelicKit.Round2Owning("R3-1", RelicKit.Cheap(), "sharp_eye");
        noCharge.QuerySharpEye(0, Side.Home); // consumes the one charge
        Assert.Throws<System.InvalidOperationException>(() => noCharge.QuerySharpEye(1, Side.Home));
    }

    [Fact]
    public void Sharp_eye_recharges_each_round()
    {
        Run run = RelicKit.Round2Owning("R3-1", RelicKit.Cheap(), "sharp_eye");
        run.QuerySharpEye(0, Side.Home);
        Assert.Throws<System.InvalidOperationException>(() => run.QuerySharpEye(1, Side.Home));

        run.LockRound(); run.FastForwardRound(); run.Settle(); run.ExitShop(); // round 3
        run.QuerySharpEye(0, Side.Home); // recharged, no throw
        Assert.Throws<System.InvalidOperationException>(() => run.QuerySharpEye(1, Side.Home));
    }

    // ---- boosted_odds ----

    [Fact]
    public void Boosted_odds_multiplies_only_leg_zero_and_feeds_vig_and_payout()
    {
        Run run = RelicKit.Round2Owning("R3-1", RelicKit.Cheap(), "boosted_odds");
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
        Run a = RelicKit.Round2Owning("R3-3", cfg, "promo_code", "boosted_odds");
        Ticket ta = a.PlaceTicket(RelicKit.Picks((0, Side.Home), (1, Side.Home)), 100);
        Assert.Equal(1.15 * OddsMath.FairDecimal(ta.Legs[0].TrueProb), ta.Legs[0].OfferedOdds, 9);
        Assert.Equal(OddsMath.FairDecimal(ta.Legs[1].TrueProb), ta.Legs[1].OfferedOdds, 9);

        // Order B: boost THEN promo — promo overwrites leg 0 back to fair, erasing the boost.
        Run b = RelicKit.Round2Owning("R3-3", cfg, "boosted_odds", "promo_code");
        Ticket tb = b.PlaceTicket(RelicKit.Picks((0, Side.Home), (1, Side.Home)), 100);
        Assert.Equal(OddsMath.FairDecimal(tb.Legs[0].TrueProb), tb.Legs[0].OfferedOdds, 9);

        Assert.True(System.Math.Abs(ta.Legs[0].OfferedOdds - tb.Legs[0].OfferedOdds) > 1e-6);
    }

    // ---- high_roller ----

    [Fact]
    public void High_roller_lifts_the_max_stake_from_half_bank_to_the_whole_bank()
    {
        // Baseline (no relic): 0.5 × bank is allowed, 0.75 × bank is rejected.
        var plain = new Run("R3-0", new RunConfig()); // bank 500
        plain.PlaceTicket(RelicKit.Picks((0, Side.Home)), 250); // exactly 0.5 × 500, inclusive
        var plain2 = new Run("R3-0", new RunConfig());
        Assert.Throws<System.ArgumentException>(() => plain2.PlaceTicket(RelicKit.Picks((0, Side.Home)), 375));

        // With high_roller: 0.75 × bank is fine; above the whole bank is still rejected.
        Run hr = RelicKit.Round2Owning("R3-0", RelicKit.Cheap(), "high_roller");
        hr.PlaceTicket(RelicKit.Picks((0, Side.Home)), 0.75 * hr.Bank);

        Run hr2 = RelicKit.Round2Owning("R3-0", RelicKit.Cheap(), "high_roller");
        Assert.Throws<System.ArgumentException>(() => hr2.PlaceTicket(RelicKit.Picks((0, Side.Home)), hr2.Bank + 1));
    }
}
