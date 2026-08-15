using System.Collections.Generic;
using SBR.Engine;
using Xunit;

namespace SBR.Engine.Tests;

/// <summary>
/// T87-am owes <b>a goalless match to full time</b> — the settlement beat with both tickets
/// resolving, a draw-backer's and a team-backer's, so the loud half and the quiet half sit in one
/// set. No 0–0 full-time frame exists in evidence: the `LEVEL 0–0` readings on hand are mid-match
/// (11', 32'), which is the progress line doing its job and says nothing about the ending.
///
/// <para>This finds the seed. <c>Run.LockRound</c> resolves <b>every</b> game on the slate whether it
/// was bet or not — <i>"outcomes for a seed are identical no matter what the player wagered (the
/// fixed universe)"</i> — so a seed can be searched for a 0–0 without modelling the tickets at all,
/// and the tickets can then be placed on the matchup that already ends level.</para>
///
/// <para>Searched through the same path the capture will take (<c>new Run(seed)</c> with the default
/// config, exactly what <c>RunDirector.StartNewRun</c> builds) rather than by sampling the model
/// directly, so the seed that passes here is the seed that renders there.</para>
/// </summary>
public class GoallessDrawSeedTests
{
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public GoallessDrawSeedTests(Xunit.Abstractions.ITestOutputHelper output) => _output = output;

    private const int Seeds = 400;

    [Fact]
    public void T87_am_find_a_seed_whose_match_ends_goalless()
    {
        var hits = new List<string>();
        int drawsSeen = 0, matchesSeen = 0;

        for (int i = 0; i < Seeds && hits.Count < 8; i++)
        {
            string seed = $"GOALLESS-{i}";
            var run = new Run(seed);
            run.LockRound();

            for (int m = 0; m < run.CurrentSlate.Matchups.Count; m++)
            {
                Matchup matchup = run.CurrentSlate.Matchups[m];
                var line = matchup.StatLine;
                if (line == null) continue;
                matchesSeen++;
                if (line.Result == MatchResult.Draw) drawsSeen++;
                if (line.HomeGoals != 0 || line.AwayGoals != 0) continue;

                hits.Add($"{seed} matchup {m}  {matchup.Home.Name} 0 - 0 {matchup.Away.Name}  " +
                         $"result={line.Result}");
            }
        }

        _output.WriteLine($"seeds searched : {Seeds}");
        _output.WriteLine($"matches seen   : {matchesSeen:N0}");
        _output.WriteLine($"draws seen     : {drawsSeen:N0}");
        _output.WriteLine($"GOALLESS       : {hits.Count} found");
        foreach (string h in hits) _output.WriteLine($"  {h}");

        Assert.True(drawsSeen > 0, "the engine must produce draws at all — 1X2 is live");
        Assert.True(hits.Count > 0,
            "no goalless match in the searched seeds; widen the search before concluding 0-0 is unreachable");
    }
}
