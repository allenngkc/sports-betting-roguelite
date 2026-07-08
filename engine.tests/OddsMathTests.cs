using SBR.Engine;

namespace SBR.Engine.Tests;

public class OddsMathTests
{
    [Theory]
    [InlineData(2.0, 0.5)]
    [InlineData(4.0, 0.25)]
    [InlineData(1.25, 0.8)]
    public void ImpliedProb_inverts_decimal_odds(double odds, double expected)
        => Assert.Equal(expected, OddsMath.ImpliedProb(odds), 10);

    [Theory]
    [InlineData(2.5, 150)]
    [InlineData(3.0, 200)]
    [InlineData(1.5, -200)]
    [InlineData(2.0, 100)]
    public void American_from_decimal(double dec, int american)
        => Assert.Equal(american, OddsMath.AmericanFromDecimal(dec));

    [Theory]
    [InlineData(150, 2.5)]
    [InlineData(-200, 1.5)]
    [InlineData(100, 2.0)]
    [InlineData(-110, 1.909090909)]
    public void Decimal_from_american(int american, double dec)
        => Assert.Equal(dec, OddsMath.DecimalFromAmerican(american), 6);

    [Fact]
    public void American_round_trips_through_decimal()
    {
        foreach (int american in new[] { -300, -110, -105, 100, 120, 250, 900 })
            Assert.Equal(american, OddsMath.AmericanFromDecimal(OddsMath.DecimalFromAmerican(american)));
    }

    [Fact]
    public void Decimal_from_american_rejects_the_dead_zone()
        => Assert.Throws<ArgumentException>(() => OddsMath.DecimalFromAmerican(50));

    [Fact]
    public void Overround_implied_probabilities_sum_to_one_plus_vig()
    {
        (double home, double away) = OddsMath.OverroundOdds(0.5, 0.05);
        Assert.Equal(1.05, OddsMath.ImpliedProb(home) + OddsMath.ImpliedProb(away), 10);
        Assert.Equal(1.904761904, home, 6);
        Assert.Equal(1.904761904, away, 6);
    }

    [Fact]
    public void Overround_odds_are_worse_than_fair()
    {
        (double home, double away) = OddsMath.OverroundOdds(0.6, 0.05);
        Assert.True(home < OddsMath.FairDecimal(0.6));
        Assert.True(away < OddsMath.FairDecimal(0.4));
    }

    [Fact]
    public void Ev_is_zero_at_fair_odds()
        => Assert.Equal(0.0, OddsMath.Ev(0.4, 100, OddsMath.FairDecimal(0.4)), 10);

    [Fact]
    public void Ev_is_positive_exactly_when_p_exceeds_implied()
    {
        Assert.True(OddsMath.Ev(0.6, 100, 2.0) > 0);
        Assert.Equal(20.0, OddsMath.Ev(0.6, 100, 2.0), 10);
        Assert.True(OddsMath.Ev(0.45, 100, 2.0) < 0);
    }

    [Fact]
    public void Parlay_odds_and_probability_are_products()
    {
        Assert.Equal(2.0 * 1.5 * 3.0, OddsMath.ParlayDecimal(new[] { 2.0, 1.5, 3.0 }), 10);
        Assert.Equal(0.5 * 0.6 * 0.3, OddsMath.ParlayProb(new[] { 0.5, 0.6, 0.3 }), 10);
    }

    [Fact]
    public void Vig_paid_matches_design02_worked_example()
    {
        // s=100 on p=0.5: fair 2.0, offered at 5% overround 1.904762 → vig ≈ 4.7619
        (double offered, _) = OddsMath.OverroundOdds(0.5, 0.05);
        Assert.Equal(4.761904, OddsMath.VigPaid(100, offered, 2.0), 4);
    }

    [Fact]
    public void Vig_paid_is_zero_at_fair_odds()
        => Assert.Equal(0.0, OddsMath.VigPaid(100, 2.0, 2.0), 10);

    // The two sanity anchors from design/02 (these caught the original formula bug).

    [Fact]
    public void Cashout_with_nothing_resolved_is_win_prob_times_payout()
    {
        var legs = new (double p, double o)[] { (0.5, 1.9), (0.6, 1.6), (0.4, 2.4) };
        double payout = 100 * OddsMath.ParlayDecimal(new[] { 1.9, 1.6, 2.4 });
        double winProb = OddsMath.ParlayProb(new[] { 0.5, 0.6, 0.4 });

        Assert.Equal(winProb * payout, OddsMath.CashOutFair(100, 1.0, legs), 8);
    }

    [Fact]
    public void Cashout_with_everything_resolved_is_full_payout()
    {
        double resolved = 1.9 * 1.6 * 2.4;
        Assert.Equal(100 * resolved, OddsMath.CashOutFair(100, resolved, Array.Empty<(double, double)>()), 8);
    }

    [Fact]
    public void Cashout_mid_parlay_worked_example()
    {
        // $200, legs 1-2 green at 1.9048 and 1.45; last leg live p=0.61, locked odds 1.8
        double fair = OddsMath.CashOutFair(200, 1.9048 * 1.45, new[] { (0.61, 1.8) });
        Assert.Equal(200 * 1.9048 * 1.45 * 0.61 * 1.8, fair, 8);
        Assert.Equal(fair * 0.92, OddsMath.CashOutOffer(fair, 0.08), 8);
    }

    [Fact]
    public void Invalid_inputs_throw()
    {
        Assert.Throws<ArgumentException>(() => OddsMath.ImpliedProb(1.0));
        Assert.Throws<ArgumentException>(() => OddsMath.ImpliedProb(0.5));
        Assert.Throws<ArgumentException>(() => OddsMath.FairDecimal(0.0));
        Assert.Throws<ArgumentException>(() => OddsMath.FairDecimal(1.0));
        Assert.Throws<ArgumentException>(() => OddsMath.Ev(1.5, 100, 2.0));
        Assert.Throws<ArgumentException>(() => OddsMath.OverroundOdds(0.5, -0.01));
    }
}
