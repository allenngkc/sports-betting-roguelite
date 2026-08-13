using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

public class AnytimeScorerTests
{
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public AnytimeScorerTests(Xunit.Abstractions.ITestOutputHelper output) => _output = output;

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

        // Re-pinned 2026-08-06: scoring weight is no longer purely role-derived. Per-player jitter
        // (RunConfig.ScoringWeightJitter) separates players within a role, so this seed's forward
        // moved 0.2146→0.2397. The pin is a DETERMINISM guard and the number is expected to move
        // when the model does; an UNINTENTIONAL change here is still a regression to investigate.
        //
        // RE-PINNED 2026-08-12, draws (Allen): 0.2397 → 0.2479. The DIRECTION is checked, not just
        // accepted — a scorer price that FELL here would have been a defect. Restoring the draw
        // class puts probability mass back on level scores (1-1, 2-2), where both teams score; the
        // old truncation forced one team strictly above the other and so suppressed the weaker
        // side's goals. P(a given team scores at least once) therefore RISES, which is the same
        // move measured independently on BTTS-yes (37.7–56.2% → 44.8–62.1%). Index 0 is an AWAY
        // player (the board lists away first), i.e. exactly the side that suppression cost most.
        Assert.Equal(0.24792434650147654, forward, 10);
        // The semantic assertion, and the one that must survive any weighting change: a forward
        // outranks a DEFENDER. The jitter is symmetric and bounded, so at the shipped weights a
        // jittered forward (3.0 × 0.65 = 1.95) still clears a jittered defender (0.5 × 1.35 =
        // 0.675) by construction rather than by luck of the seed. That is the pair this asserts,
        // and it is the only adjacent pair the construction actually separates — see
        // Role_weight_bands_are_disjoint_for_forward_over_defender_only.
        Assert.True(forward > defender);
    }

    [Fact]
    public void Role_weight_bands_are_disjoint_for_forward_over_defender_only()
    {
        var config = new RunConfig();
        double Lo(double w) => w * (1.0 - config.ScoringWeightJitter);
        double Hi(double w) => w * (1.0 + config.ScoringWeightJitter);

        // Disjoint bands — no seed can invert these, which is what "by construction" means and
        // what lets the pin above assert forward > defender unconditionally.
        Assert.True(Lo(config.ForwardScoringWeight) > Hi(config.DefenderScoringWeight));
        Assert.True(Lo(config.MidfielderScoringWeight) > Hi(config.DefenderScoringWeight));

        // NOT disjoint, and the correction this test exists to carry: forward and midfielder
        // OVERLAP at the shipped dials — forward floor 3.0 × 0.65 = 1.95 sits BELOW midfielder
        // ceiling 1.5 × 1.35 = 2.025 — so a jittered midfielder can outrank a jittered forward.
        // The change's own write-up claimed role order survived "by construction, not by luck"
        // and demonstrated it on the forward-vs-defender gap alone, never checking the adjacent
        // pair. The claim was wider than its arithmetic. Whether the overlap is desirable is a
        // design question (an attacking midfielder pricing above a fourth-choice forward is not
        // obviously wrong); that it exists is arithmetic.
        Assert.True(Hi(config.MidfielderScoringWeight) > Lo(config.ForwardScoringWeight));
    }

    [Fact]
    public void Every_generated_roster_keeps_forwards_and_midfielders_above_defenders()
    {
        int teams = 0, forwardMidfielderInversions = 0;

        for (int seed = 0; seed < 300; seed++)
            foreach (Matchup matchup in new Run($"ROLE-ORDER-{seed}").CurrentSlate.Matchups)
                foreach (Team team in new[] { matchup.Home, matchup.Away })
                {
                    teams++;
                    double[] fw = team.Players.Where(p => p.Role == PlayerRole.FW).Select(p => p.ScoringWeight).ToArray();
                    double[] mf = team.Players.Where(p => p.Role == PlayerRole.MF).Select(p => p.ScoringWeight).ToArray();
                    double[] df = team.Players.Where(p => p.Role == PlayerRole.DF).Select(p => p.ScoringWeight).ToArray();
                    if (df.Length == 0) continue;

                    // The invariant the disjoint bands guarantee, checked on real rosters rather
                    // than asserted from the arithmetic that produced them.
                    if (fw.Length > 0) Assert.True(fw.Min() > df.Max());
                    if (mf.Length > 0) Assert.True(mf.Min() > df.Max());

                    if (fw.Length > 0 && mf.Length > 0 && mf.Max() > fw.Min()) forwardMidfielderInversions++;
                }

        // Reported, not asserted — and reported rather than dropped, because a count nobody can
        // see is the vacuous-green shape this studio has spent a fortnight on. A threshold here
        // would be worse than nothing: a future narrower jitter that closed the overlap is an
        // improvement and must not turn this suite red. What this cannot see: whether an inversion
        // matters to a player, which is a question for a board, not a roster.
        _output.WriteLine($"forward/midfielder inversions: {forwardMidfielderInversions} of {teams} teams "
            + $"({100.0 * forwardMidfielderInversions / teams:0.00}%) — measured, asserted on nothing");
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
    public void Public_grading_surface_handles_every_market_kind_without_throwing()
    {
        // M-02: the 2-arg static Grades overload is now private (it can't resolve AnytimeScorer
        // on its own), so the only public grading path is this 3-arg overload. Walk every
        // MarketKind through it against one matchup/line and assert neither a throw nor a
        // meaningless result — if a future MarketKind is added, the switch below forces this
        // test to be updated rather than silently skipping it.
        var home = new[] { new Player("Home Hero", PlayerRole.FW, 3), new Player("Home Wall", PlayerRole.DF, 1) };
        var away = new[] { new Player("Away Ace", PlayerRole.FW, 3) };
        var matchup = new Matchup(0, new Team("Home Team", 1, 0, home), new Team("Away Team", 0, 1, away),
            0.6, 1.5, 2.0);
        var line = new MatchStatLine(2, 1, 5, 4, 2, 1,
            new[] { home[0], home[0] }, new[] { away[0] });

        foreach (MarketKind kind in Enum.GetValues<MarketKind>())
        {
            (MarketSelection selection, bool expected) = kind switch
            {
                MarketKind.Moneyline => (MarketSelection.Moneyline(Side.Away), false),          // home won 2-1
                MarketKind.TotalGoals => (MarketSelection.TotalGoals(3.5, true), false),         // 3 goals, not over 3.5
                MarketKind.BothTeamsToScore => (MarketSelection.BothTeamsToScore(true), true),   // both scored
                MarketKind.TotalCorners => (MarketSelection.TotalCorners(8.5, true), true),      // 9 corners, over 8.5
                MarketKind.TotalCards => (MarketSelection.TotalCards(2.5, false), false),        // 3 cards, not under 2.5
                MarketKind.AnytimeScorer => (MarketSelection.AnytimeScorer(0), true),            // away ace scored
                _ => throw new InvalidOperationException($"Unhandled {kind}; extend this test's fixture."),
            };

            Assert.Equal(expected, MatchModel.Grades(matchup, line, selection));
        }
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
