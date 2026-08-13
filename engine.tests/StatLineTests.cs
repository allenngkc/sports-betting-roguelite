using System;
using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

public class StatLineTests
{
    /// <summary>Was <c>Lock_bakes_one_non_draw_stat_line_for_every_matchup</c>, and it asserted
    /// <c>NotEqual(HomeGoals, AwayGoals)</c> — the no-draws constraint, enshrined as a test. Allen
    /// lifted the constraint 2026-08-12, so that assertion is deleted rather than relaxed, and what
    /// replaces it is the invariant that actually matters now: the result AGREES WITH THE SCORE in
    /// all three directions, draws included.</summary>
    [Fact]
    public void Lock_bakes_a_stat_line_whose_result_agrees_with_its_score()
    {
        var run = new Run("STAT-LINE");
        run.LockRound();

        foreach (Matchup m in run.CurrentSlate.Matchups)
        {
            Assert.NotNull(m.StatLine);
            Assert.NotNull(m.Result);
            Assert.Equal(m.Result, m.StatLine!.Result);
            Assert.Equal(
                m.StatLine.HomeGoals > m.StatLine.AwayGoals ? MatchResult.Home
                    : m.StatLine.HomeGoals < m.StatLine.AwayGoals ? MatchResult.Away
                    : MatchResult.Draw,
                m.StatLine.Result);
            Assert.True(m.StatLine.HomeCorners >= 0 && m.StatLine.AwayCorners >= 0);
            Assert.True(m.StatLine.HomeCards >= 0 && m.StatLine.AwayCards >= 0);
        }
    }

    /// <summary>The draw is REACHABLE, not merely representable. A model that can express a level
    /// score but never samples one would pass every other test in this file while shipping the old
    /// behaviour — so this asserts the sampler actually visits the class, across enough rounds that
    /// a ~25% outcome missing entirely is not chance.</summary>
    [Fact]
    public void The_sampler_actually_produces_draws()
    {
        int drawn = 0, total = 0;
        foreach (string seed in new[]
                 { "DRAW-A", "DRAW-B", "DRAW-C", "DRAW-D", "DRAW-E", "DRAW-F", "DRAW-G", "DRAW-H" })
        {
            var run = new Run(seed);
            run.LockRound();
            foreach (Matchup m in run.CurrentSlate.Matchups)
            {
                total++;
                if (m.Result == MatchResult.Draw) drawn++;
            }
        }
        // 8 seeds x 6 matchups. At the implied ~25% draw rate, every one of the 48 coming back
        // decisive has probability ~1e-6, so a zero here is a defect and not a bad day.
        Assert.Equal(48, total);
        // Deliberately a presence check with wide air, not a rate band: the draw RATE is a
        // campaign measurement (the latents imply 22.6%-28.4%), and pinning it here would turn a
        // future model re-tune into a red suite for no defect. What must never silently return is
        // ZERO draws.
        Assert.True(drawn > 0, $"the sampler produced no draws in {total} matchups");
    }

    [Fact]
    public void Market_legs_grade_against_the_shared_baked_stat_line()
    {
        var run = new Run("STAT-GRADE");
        Matchup m = run.CurrentSlate.Matchups[0];
        var selections = m.Markets.Select(x => x.Selection).ToArray();
        run.PlaceTicket(new[] { new Pick(0, selections[0]) }, 10);
        run.LockRound();

        foreach (MarketSelection selection in selections)
        {
            bool expected = selection.Kind == MarketKind.Moneyline
                ? selection.Choice == (m.Result switch
                    {
                        MatchResult.Home => MarketChoice.Home,
                        MatchResult.Away => MarketChoice.Away,
                        _ => MarketChoice.Draw,
                    })
                : selection.Kind == MarketKind.BothTeamsToScore
                    ? (m.StatLine!.HomeGoals > 0 && m.StatLine.AwayGoals > 0) == (selection.Choice == MarketChoice.Yes)
                    : selection.Kind == MarketKind.TotalGoals
                        ? Compare(m.StatLine!.HomeGoals + m.StatLine.AwayGoals, selection)
                        : selection.Kind == MarketKind.TotalCorners
                            ? Compare(m.StatLine!.HomeCorners + m.StatLine.AwayCorners, selection)
                            : selection.Kind == MarketKind.TotalCards
                                ? Compare(m.StatLine!.HomeCards + m.StatLine.AwayCards, selection)
                                : selection.Kind == MarketKind.AnytimeScorer
                                    ? (m.PlayerSide(selection.PlayerIndex) == Side.Home
                                        ? m.StatLine!.HomeScorers : m.StatLine!.AwayScorers)
                                        .Any(p => object.ReferenceEquals(p, m.PlayerAt(selection.PlayerIndex)))
                                    : false;
            Assert.Equal(expected, m.Grades(selection));
        }
    }

    [Fact]
    public void Betting_choices_do_not_change_the_baked_stat_lines()
    {
        var betting = new Run("STAT-FIXED");
        betting.PlaceTicket(new[] { new Pick(0, MarketSelection.TotalGoals(2.5, true)) }, 10);
        betting.LockRound();
        var abstaining = new Run("STAT-FIXED");
        abstaining.LockRound();

        for (int i = 0; i < betting.CurrentSlate.Matchups.Count; i++)
        {
            MatchStatLine a = betting.CurrentSlate.Matchups[i].StatLine!;
            MatchStatLine b = abstaining.CurrentSlate.Matchups[i].StatLine!;
            Assert.Equal(a.HomeGoals, b.HomeGoals);
            Assert.Equal(a.AwayGoals, b.AwayGoals);
            Assert.Equal(a.HomeCorners, b.HomeCorners);
            Assert.Equal(a.AwayCorners, b.AwayCorners);
            Assert.Equal(a.HomeCards, b.HomeCards);
            Assert.Equal(a.AwayCards, b.AwayCards);
        }
    }

    [Fact]
    public void First_beat_calibration_is_shared_by_moneyline_and_counting_markets()
    {
        double mlError = FirstBeatError(MarketSelection.Moneyline(Side.Home));
        double goalsError = FirstBeatError(MarketSelection.TotalGoals(2.5, true));
        Assert.InRange(mlError, 0.0, 0.12);
        Assert.InRange(goalsError, 0.0, 0.12);
    }

    private static double FirstBeatError(MarketSelection selection)
    {
        double observed = 0.0;
        double anchor = 0.0;
        const int samples = 80;
        for (int i = 0; i < samples; i++)
        {
            var run = new Run("CALIBRATION-" + i);
            Matchup m = run.CurrentSlate.Matchups[0];
            anchor += m.TrueProb(selection);
            run.PlaceTicket(new[] { new Pick(0, selection) }, 10);
            run.LockRound();
            Assert.True(run.Sweats[0].MoveNext(out DramaEvent? e));
            observed += e!.WinProbAfter;
        }
        return Math.Abs(observed / samples - anchor / samples);
    }

    private static bool Compare(int value, MarketSelection selection)
        => selection.Choice == MarketChoice.Over ? value > selection.Line : value < selection.Line;
}
