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
        run.GrantComps(100); // shop currency (design/10 F) for the buy/sell scripts
        return run; // in Phase.Shop
    }

    [Fact]
    public void Shop_deals_a_hand_of_distinct_unowned_passives_and_distinct_consumables()
    {
        Run run = ShopRun();
        // The DEALT HAND (charm campaign tuning): 4 passives + 3 consumables per visit.
        Assert.Equal(4, run.ShopOffers.Count);
        Assert.Equal(3, run.ConsumableOffers.Count);
        Assert.Equal(4, run.ShopOffers.Select(o => o.Id).Distinct().Count());
        Assert.Equal(3, run.ConsumableOffers.Select(o => o.Id).Distinct().Count());
        foreach (RelicDefinition o in run.ShopOffers)
            Assert.DoesNotContain(run.OwnedRelics, r => r.Id == o.Id);
    }

    [Fact]
    public void Buying_deducts_and_owns_and_removes_the_offer()
    {
        Run run = ShopRun();
        double bankBefore = run.Bank;
        double comps = run.Comps;
        RelicDefinition def = run.ShopOffers[0];

        run.BuyRelic(0);
        Assert.Equal(comps - def.Price, run.Comps, 10);
        Assert.Equal(bankBefore, run.Bank, 10); // items never touch cash (design/10 F)
        Assert.Single(run.OwnedRelics);
        Assert.Equal(3, run.ShopOffers.Count);
    }

    [Fact]
    public void Sell_back_credits_the_resale_value_in_comps()
    {
        Run run = ShopRun();
        RelicDefinition def = run.ShopOffers[0];
        run.BuyRelic(0);
        double comps = run.Comps;
        double resale = run.GetResaleValue(def); // the single resale truth (rev 5 §10)

        run.SellRelic(0);
        Assert.Equal(comps + resale, run.Comps, 10);
        Assert.Empty(run.OwnedRelics);
    }

    [Fact]
    public void Totem_is_never_dealt_again_after_a_purchase_even_if_sold()
    {
        Run run = ShopRun(rounds: 12);
        run.GrantRelic(RelicCatalog.All.First(r => r.Id == RelicCatalog.TotemId)); // marks purchased
        run.SellRelic(0); // sold back — but the once-per-run right is spent

        // Every hand dealt AFTER the purchase excludes the totem from the pool, forever.
        int handsScanned = 0;
        while (run.Phase == Phase.Shop && handsScanned < 8)
        {
            run.ExitShop();
            run.LockRound(); run.FastForwardRound(); run.Settle();
            if (run.Phase != Phase.Shop) break;
            Assert.DoesNotContain(run.ShopOffers, o => o.Id == RelicCatalog.TotemId);
            handsScanned++;
        }
        Assert.True(handsScanned >= 5, $"only {handsScanned} hands scanned — extend the run");
    }

    [Fact]
    public void Consumable_slots_cap_purchases()
    {
        Run run = ShopRun();
        run.GrantConsumable(RelicCatalog.Consumables[0]);
        run.GrantConsumable(RelicCatalog.Consumables[1]);
        run.GrantConsumable(RelicCatalog.Consumables[0]); // 3 slots since playtest #8

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
