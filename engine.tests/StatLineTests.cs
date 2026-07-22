using System;
using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

public class StatLineTests
{
    [Fact]
    public void Lock_bakes_one_non_draw_stat_line_for_every_matchup()
    {
        var run = new Run("STAT-LINE");
        run.LockRound();

        foreach (Matchup m in run.CurrentSlate.Matchups)
        {
            Assert.NotNull(m.StatLine);
            Assert.NotNull(m.Result);
            Assert.Equal(m.Result, m.StatLine!.Winner);
            Assert.NotEqual(m.StatLine.HomeGoals, m.StatLine.AwayGoals);
            Assert.True(m.StatLine.HomeCorners >= 0 && m.StatLine.AwayCorners >= 0);
            Assert.True(m.StatLine.HomeCards >= 0 && m.StatLine.AwayCards >= 0);
        }
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
                ? selection.Choice == (m.Result == Side.Home ? MarketChoice.Home : MarketChoice.Away)
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
