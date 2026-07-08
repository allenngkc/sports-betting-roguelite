using SBR.Engine;

namespace SBR.Engine.Tests;

public class SlateGeneratorTests
{
    private static Slate NewSlate(string seed = "SLATE-TEST", RunConfig? cfg = null)
        => SlateGenerator.Generate(1, new RngHub(seed).Slate, cfg ?? new RunConfig());

    [Fact]
    public void Slate_has_configured_matchup_count()
        => Assert.Equal(6, NewSlate().Matchups.Count);

    [Fact]
    public void True_probabilities_stay_in_configured_band()
    {
        for (int i = 0; i < 50; i++)
        {
            foreach (Matchup m in NewSlate($"SEED-{i}").Matchups)
                Assert.InRange(m.TrueHomeProb, 0.25, 0.75);
        }
    }

    [Fact]
    public void Offered_odds_carry_exactly_the_configured_overround()
    {
        foreach (Matchup m in NewSlate().Matchups)
        {
            double impliedSum = OddsMath.ImpliedProb(m.HomeOdds) + OddsMath.ImpliedProb(m.AwayOdds);
            Assert.Equal(1.05, impliedSum, 10);
        }
    }

    [Fact]
    public void Team_names_are_distinct_within_a_slate()
    {
        var names = NewSlate().Matchups
            .SelectMany(m => new[] { m.Home.Name, m.Away.Name })
            .ToList();
        Assert.Equal(names.Count, names.Distinct().Count());
    }

    [Fact]
    public void Records_reflect_the_configured_prior_games()
    {
        foreach (Matchup m in NewSlate().Matchups)
        {
            Assert.Equal(9, m.Home.Wins + m.Home.Losses);
            Assert.Equal(9, m.Away.Wins + m.Away.Losses);
        }
    }

    [Fact]
    public void Same_seed_regenerates_the_identical_slate()
    {
        Slate a = NewSlate("DETERMINISM");
        Slate b = NewSlate("DETERMINISM");
        for (int i = 0; i < a.Matchups.Count; i++)
        {
            Assert.Equal(a.Matchups[i].Home.Name, b.Matchups[i].Home.Name);
            Assert.Equal(a.Matchups[i].Away.Name, b.Matchups[i].Away.Name);
            Assert.Equal(a.Matchups[i].TrueHomeProb, b.Matchups[i].TrueHomeProb, 12);
            Assert.Equal(a.Matchups[i].HomeOdds, b.Matchups[i].HomeOdds, 12);
        }
    }

    [Fact]
    public void Different_seeds_differ()
    {
        Slate a = NewSlate("SEED-A");
        Slate b = NewSlate("SEED-B");
        bool anyDifferent = false;
        for (int i = 0; i < a.Matchups.Count; i++)
            anyDifferent |= a.Matchups[i].TrueHomeProb != b.Matchups[i].TrueHomeProb;
        Assert.True(anyDifferent);
    }
}
