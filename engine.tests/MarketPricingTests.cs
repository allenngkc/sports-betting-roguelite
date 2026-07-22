using System;
using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

public class MarketPricingTests
{
    [Fact]
    public void Phase_one_board_contains_moneyline_btts_and_three_line_ladders()
    {
        var run = new Run("MARKET-BOARD");
        Matchup m = run.CurrentSlate.Matchups[0];

        // Phase 4 adds one YES-only scorer offer for every listed outfield player, not a
        // magic board-size constant. The old Phase 1 board remains the 22 two-way offers.
        Assert.Equal(22 + m.Home.Players.Count + m.Away.Players.Count, m.Markets.Count);
        Assert.Equal(2, m.Markets.Count(x => x.Selection.Kind == MarketKind.Moneyline));
        Assert.Equal(2, m.Markets.Count(x => x.Selection.Kind == MarketKind.BothTeamsToScore));
        Assert.Equal(6, m.Markets.Count(x => x.Selection.Kind == MarketKind.TotalGoals));
        Assert.Equal(6, m.Markets.Count(x => x.Selection.Kind == MarketKind.TotalCorners));
        Assert.Equal(6, m.Markets.Count(x => x.Selection.Kind == MarketKind.TotalCards));
        Assert.Equal(m.Home.Players.Count + m.Away.Players.Count,
            m.Markets.Count(x => x.Selection.Kind == MarketKind.AnytimeScorer));
    }

    [Fact]
    public void Counting_market_over_probability_decreases_up_the_ladder()
    {
        Matchup m = new Run("MARKET-LADDER").CurrentSlate.Matchups[0];
        double goals15 = m.TrueProb(MarketSelection.TotalGoals(1.5, true));
        double goals25 = m.TrueProb(MarketSelection.TotalGoals(2.5, true));
        double goals35 = m.TrueProb(MarketSelection.TotalGoals(3.5, true));
        double corners85 = m.TrueProb(MarketSelection.TotalCorners(8.5, true));
        double corners95 = m.TrueProb(MarketSelection.TotalCorners(9.5, true));
        double corners105 = m.TrueProb(MarketSelection.TotalCorners(10.5, true));
        double cards35 = m.TrueProb(MarketSelection.TotalCards(3.5, true));
        double cards45 = m.TrueProb(MarketSelection.TotalCards(4.5, true));
        double cards55 = m.TrueProb(MarketSelection.TotalCards(5.5, true));

        Assert.True(goals15 > goals25 && goals25 > goals35);
        Assert.True(corners85 > corners95 && corners95 > corners105);
        Assert.True(cards35 > cards45 && cards45 > cards55);
    }

    [Fact]
    public void Every_two_way_offer_has_the_configured_implied_probability_sum()
    {
        var run = new Run("MARKET-VIG");
        Matchup m = run.CurrentSlate.Matchups[0];
        foreach (MarketKind kind in new[] { MarketKind.Moneyline, MarketKind.TotalGoals,
            MarketKind.TotalCorners, MarketKind.TotalCards })
        {
            var groups = m.Markets.Where(x => x.Selection.Kind == kind)
                .GroupBy(x => x.Selection.Line);
            foreach (var group in groups)
                Assert.Equal(1.05, group.Sum(x => 1.0 / x.Odds), 10);
        }
        var btts = m.Markets.Where(x => x.Selection.Kind == MarketKind.BothTeamsToScore);
        Assert.Equal(1.05, btts.Sum(x => 1.0 / x.Odds), 10);
    }

    [Fact]
    public void A_line_that_prices_at_or_below_even_odds_is_rejected_at_slate_build()
    {
        // Over 0.5 cards is a near-certainty: 1/(p × 1.05) <= 1.0 must throw at pricing time,
        // not when a parlay product first trips over the leg (locked odds are the contract).
        var config = new RunConfig { CardLines = new[] { 0.5 } };
        Assert.Throws<InvalidOperationException>(() => new Run("MARKET-FLOOR", config));
    }

    [Fact]
    public void Market_true_probability_is_the_complement_of_its_other_side()
    {
        Matchup m = new Run("MARKET-COMPLEMENT").CurrentSlate.Matchups[0];
        foreach (var pair in new[] {
            (MarketSelection.TotalGoals(2.5, true), MarketSelection.TotalGoals(2.5, false)),
            (MarketSelection.TotalCorners(9.5, true), MarketSelection.TotalCorners(9.5, false)),
            (MarketSelection.TotalCards(4.5, true), MarketSelection.TotalCards(4.5, false)),
            (MarketSelection.BothTeamsToScore(true), MarketSelection.BothTeamsToScore(false)) })
            Assert.Equal(1.0, m.TrueProb(pair.Item1) + m.TrueProb(pair.Item2), 10);
    }
}
