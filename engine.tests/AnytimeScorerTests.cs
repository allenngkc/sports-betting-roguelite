using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

public class AnytimeScorerTests
{
    [Fact]
    public void Locked_scorer_lists_match_goal_totals_and_scoring_rosters()
    {
        var run = new Run("SCORER-INVARIANTS");
        run.LockRound();

        foreach (Matchup matchup in run.CurrentSlate.Matchups)
        {
            MatchStatLine line = matchup.StatLine!;
            Assert.Equal(line.HomeGoals, line.HomeScorers.Count);
            Assert.Equal(line.AwayGoals, line.AwayScorers.Count);
            Assert.All(line.HomeScorers, p => Assert.Contains(p, matchup.Home.Players));
            Assert.All(line.AwayScorers, p => Assert.Contains(p, matchup.Away.Players));
        }
    }

    [Fact]
    public void Pricing_uses_the_weighted_goal_attribution_distribution()
    {
        Matchup matchup = new Run("SCORER-WORKED-PIN").CurrentSlate.Matchups[0];
        double forward = matchup.TrueProb(MarketSelection.AnytimeScorer(0));
        double defender = matchup.TrueProb(MarketSelection.AnytimeScorer(5));

        Assert.Equal(0.21463302392628314, forward, 10);
        Assert.True(forward > defender);
    }

    [Fact]
    public void Scorer_grading_uses_the_baked_player_identity()
    {
        var home = new[] { new Player("Home Hero", PlayerRole.FW, 3), new Player("Home Wall", PlayerRole.DF, 1) };
        var away = new[] { new Player("Away Ace", PlayerRole.FW, 3) };
        var matchup = new Matchup(0, new Team("Home Team", 1, 0, home), new Team("Away Team", 0, 1, away),
            0.6, 1.5, 2.0);
        // Grades through the public static overload — forcing a stat line onto a Matchup is
        // engine-internal by design (the fixed universe), and tests stay on the public API.
        var line = new MatchStatLine(2, 1, 0, 0, 0, 0,
            new[] { home[0], home[0] }, new[] { away[0] });

        Assert.True(MatchModel.Grades(matchup, line, MarketSelection.AnytimeScorer(0)));  // away ace
        Assert.True(MatchModel.Grades(matchup, line, MarketSelection.AnytimeScorer(1)));  // home hero
        Assert.False(MatchModel.Grades(matchup, line, MarketSelection.AnytimeScorer(2))); // home wall
    }

    [Fact]
    public void Rosters_and_scorers_are_deterministic_and_match_streams_are_isolated()
    {
        var first = new Run("SCORER-REPLAY");
        var second = new Run("SCORER-REPLAY");
        Assert.Equal(first.CurrentSlate.Matchups[0].Home.Players.Select(p => p.Name),
            second.CurrentSlate.Matchups[0].Home.Players.Select(p => p.Name));
        first.LockRound();
        second.LockRound();
        Assert.Equal(first.CurrentSlate.Matchups[0].StatLine!.HomeScorers.Select(p => p.Name),
            second.CurrentSlate.Matchups[0].StatLine!.HomeScorers.Select(p => p.Name));

        var hub = new RngHub("SCORER-STREAMS");
        Pcg32 roster = hub.DeriveMatch(1, 0, "roster");
        Pcg32 scorers = hub.DeriveMatch(1, 0, "scorers");
        bool diverged = false;
        for (int i = 0; i < 12; i++) diverged |= roster.NextUInt() != scorers.NextUInt();
        Assert.True(diverged);
    }
}
