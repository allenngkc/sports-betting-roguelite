using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// The rework shop (all unowned passives + drawn consumable offers), sell-back at half price,
/// the Totem's once-per-run purchase rule, and the bookie's gift channel (design/10 D).
/// </summary>
public class ShopAndGiftTests
{
    private static Run ShopRun(string seed = "SHOP-1", int rounds = 6)
    {
        var payments = new double[rounds];
        for (int i = 0; i < rounds; i++) payments[i] = 10;
        var run = new Run(seed, new RunConfig { Payments = payments });
        run.LockRound();
        run.FastForwardRound();
        run.Settle();
        return run; // in Phase.Shop
    }

    [Fact]
    public void Shop_offers_every_unowned_passive_and_draws_consumables()
    {
        Run run = ShopRun();
        Assert.Equal(3, run.ShopOffers.Count);
        Assert.Equal(2, run.ConsumableOffers.Count);
        Assert.NotEqual(run.ConsumableOffers[0].Id, run.ConsumableOffers[1].Id); // distinct draws
    }

    [Fact]
    public void Buying_deducts_and_owns_and_removes_the_offer()
    {
        Run run = ShopRun();
        double bank = run.Bank;
        int idx = IndexOf(run, RelicCatalog.MultiplierId);

        run.BuyRelic(idx);
        Assert.Equal(bank - 250, run.Bank, 10);
        Assert.Single(run.OwnedRelics);
        Assert.Equal(2, run.ShopOffers.Count);
    }

    [Fact]
    public void Sell_back_credits_half_the_list_price()
    {
        Run run = ShopRun();
        run.BuyRelic(IndexOf(run, RelicCatalog.MultiplierId));
        double bank = run.Bank;

        run.SellRelic(0);
        Assert.Equal(bank + 125, run.Bank, 10);
        Assert.Empty(run.OwnedRelics);
    }

    [Fact]
    public void Totem_is_never_offered_again_after_a_purchase_even_if_sold()
    {
        Run run = ShopRun();
        run.BuyRelic(IndexOf(run, RelicCatalog.TotemId));
        run.SellRelic(0); // sold back — but the once-per-run right is spent
        run.ExitShop();

        run.LockRound(); run.FastForwardRound(); run.Settle();
        Assert.DoesNotContain(run.ShopOffers, o => o.Id == RelicCatalog.TotemId);
        Assert.Equal(2, run.ShopOffers.Count); // multiplier + scar only
    }

    [Fact]
    public void Consumable_slots_cap_purchases()
    {
        Run run = ShopRun();
        run.GrantConsumable(RelicCatalog.Consumables[0]);
        run.GrantConsumable(RelicCatalog.Consumables[1]);

        Assert.Throws<InvalidOperationException>(() => run.BuyConsumable(0));
    }

    /// <summary>A guaranteed-loss round for ANY seed: proportional dutching — stake ⌊100/odds⌋ on
    /// both sides of matchup 0, so every outcome pays ≤ 100 against ~103–105 staked (the vig).</summary>
    private static void PlayGuaranteedLosingRound(Run run)
    {
        Matchup m = run.CurrentSlate.Matchups[0];
        run.PlaceTicket(new[] { new Pick(0, Side.Home) }, Math.Floor(100 / m.HomeOdds));
        run.PlaceTicket(new[] { new Pick(0, Side.Away) }, Math.Floor(100 / m.AwayOdds));
        run.LockRound();
        run.FastForwardRound();
        run.Settle();
    }

    [Fact]
    public void Two_consecutive_losing_rounds_draw_the_bookies_gift()
    {
        var run = new Run("GIFT-1", new RunConfig { Payments = new double[] { 10, 10, 10, 10 } });

        for (int round = 0; round < 2; round++)
        {
            PlayGuaranteedLosingRound(run);
            Assert.Equal(Phase.Shop, run.Phase);
            run.ExitShop();
        }

        // Two consecutive losing rounds → the bookie texts a promo at the round-3 open.
        Assert.NotNull(run.LastGift);
        Assert.Single(run.OwnedConsumables);
    }

    [Fact]
    public void Winning_rounds_reset_the_gift_counter()
    {
        var run = new Run("GIFT-2", new RunConfig { Payments = new double[] { 10, 10, 10 } });

        run.LockRound(); run.FastForwardRound(); run.Settle(); // no bets: PnL 0, not a losing round
        run.ExitShop();
        Assert.Null(run.LastGift);

        PlayGuaranteedLosingRound(run); // one losing round only
        run.ExitShop();
        Assert.Null(run.LastGift); // needs two consecutive
    }

    private static int IndexOf(Run run, string relicId)
    {
        for (int i = 0; i < run.ShopOffers.Count; i++)
            if (run.ShopOffers[i].Id == relicId) return i;
        throw new InvalidOperationException($"{relicId} not offered");
    }
}
