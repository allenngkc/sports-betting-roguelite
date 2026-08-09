using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>Covers <see cref="MatchModel.Fields"/> (S22 ruling, Design Director batch 4): the exact
/// DS-vocabulary literals for every <see cref="MarketKind"/> (including the em dash in
/// "BTTS — YES"), the Subject population rule, Role spelled out as a full word, and that Fields
/// never throws for any MarketKind.</summary>
public class MarketFieldsTests
{
    private static Matchup MakeMatchup()
    {
        var home = new[]
        {
            new Player("Home Forward", PlayerRole.FW, 3),
            new Player("Home Midfielder", PlayerRole.MF, 2),
            new Player("Home Defender", PlayerRole.DF, 1),
        };
        var away = new[]
        {
            new Player("Away Forward", PlayerRole.FW, 3),
            new Player("Away Midfielder", PlayerRole.MF, 2),
            new Player("Away Defender", PlayerRole.DF, 1),
        };
        return new Matchup(0, new Team("Home Team", 1, 0, home), new Team("Away Team", 0, 1, away),
            0.6, 1.5, 2.0);
    }

    /// <summary>The NECESSARY condition for S22: Fields must carry enough to tell any two
    /// selections on one matchup apart. Under the old single-string DisplayLabel this was implicit;
    /// once the engine emits fields and each surface composes, a composer that reaches for the
    /// wrong field silently merges OVER 2.5 and UNDER 3.5 into one indistinguishable row — the
    /// bettor cannot tell which side of the total they backed, while settlement still grades them
    /// differently. That exact defect was caught in review of the S22 implementation.
    /// This fact proves the information is AVAILABLE. It cannot prove a given composer uses it —
    /// that guard is per-surface and lives with the composer (see the PlayMode fixture's
    /// CompactLegLabel uniqueness test).</summary>
    [Fact]
    public void Every_selection_on_a_matchup_is_uniquely_identified_by_its_fields()
    {
        Matchup matchup = MakeMatchup();
        var config = new RunConfig();
        var selections = new List<MarketSelection>
        {
            MarketSelection.Moneyline(Side.Home),
            MarketSelection.Moneyline(Side.Away),
            MarketSelection.BothTeamsToScore(true),
            MarketSelection.BothTeamsToScore(false),
        };
        foreach (double line in config.GoalLines)
        {
            selections.Add(MarketSelection.TotalGoals(line, true));
            selections.Add(MarketSelection.TotalGoals(line, false));
        }
        foreach (double line in config.CornerLines)
        {
            selections.Add(MarketSelection.TotalCorners(line, true));
            selections.Add(MarketSelection.TotalCorners(line, false));
        }
        foreach (double line in config.CardLines)
        {
            selections.Add(MarketSelection.TotalCards(line, true));
            selections.Add(MarketSelection.TotalCards(line, false));
        }
        for (int i = 0; i < matchup.Away.Players.Count + matchup.Home.Players.Count; i++)
            selections.Add(MarketSelection.AnytimeScorer(i));

        // Identity = the discriminating fields a surface has to compose from. If two distinct
        // selections share all of them, no composer built on Fields can tell them apart.
        var seen = new Dictionary<string, MarketSelection>();
        foreach (MarketSelection selection in selections)
        {
            MatchModel.MarketFields f = MatchModel.Fields(matchup, selection);
            string identity = $"{f.Market}|{f.Subject}|{f.Line}";
            Assert.False(seen.ContainsKey(identity),
                $"{selection.Kind} {selection.Choice} {selection.Line} collapses onto "
                + $"{(seen.TryGetValue(identity, out MarketSelection prior) ? $"{prior.Kind} {prior.Choice} {prior.Line}" : "?")} "
                + $"— both identify as \"{identity}\"");
            seen[identity] = selection;
        }
        Assert.Equal(selections.Count, seen.Count);
    }

    [Fact]
    public void Moneyline_fields_are_DS_verbatim_and_carry_the_picked_team_as_subject()
    {
        Matchup matchup = MakeMatchup();

        MatchModel.MarketFields home = MatchModel.Fields(matchup, MarketSelection.Moneyline(Side.Home));
        Assert.Equal("MONEYLINE", home.Market);
        Assert.Equal("", home.Line);
        Assert.Equal("Home Team", home.Subject);
        Assert.Equal("", home.Role);
        Assert.Equal("Away Team v Home Team", home.Fixture);

        MatchModel.MarketFields away = MatchModel.Fields(matchup, MarketSelection.Moneyline(Side.Away));
        Assert.Equal("MONEYLINE", away.Market);
        Assert.Equal("", away.Line);
        Assert.Equal("Away Team", away.Subject);
    }

    [Fact]
    public void Total_goals_over_and_under_match_the_DS_line_form_and_have_no_subject()
    {
        Matchup matchup = MakeMatchup();

        MatchModel.MarketFields over = MatchModel.Fields(matchup, MarketSelection.TotalGoals(2.5, true));
        Assert.Equal("TOTAL GOALS", over.Market);
        Assert.Equal("OVER 2.5 GOALS", over.Line);
        Assert.Equal("", over.Subject);
        Assert.Equal("", over.Role);

        MatchModel.MarketFields under = MatchModel.Fields(matchup, MarketSelection.TotalGoals(2.5, false));
        Assert.Equal("TOTAL GOALS", under.Market);
        Assert.Equal("UNDER 2.5 GOALS", under.Line);
        Assert.Equal("", under.Subject);
    }

    [Fact]
    public void Both_teams_to_score_carries_the_em_dash_and_yes_no_lives_in_market_not_line()
    {
        Matchup matchup = MakeMatchup();

        MatchModel.MarketFields yes = MatchModel.Fields(matchup, MarketSelection.BothTeamsToScore(true));
        Assert.Equal("BTTS — YES", yes.Market); // U+2014 em dash, spaced — DS-verbatim
        Assert.Equal("BOTH TEAMS TO SCORE", yes.Line);
        Assert.Equal("", yes.Subject);

        // "BTTS — NO" is a pattern-extension of the DS-enumerated "BTTS — YES", not itself DS-verbatim.
        MatchModel.MarketFields no = MatchModel.Fields(matchup, MarketSelection.BothTeamsToScore(false));
        Assert.Equal("BTTS — NO", no.Market);
        Assert.Equal("BOTH TEAMS TO SCORE", no.Line);
    }

    [Fact]
    public void Total_corners_over_and_under_are_pattern_extensions_of_total_goals()
    {
        Matchup matchup = MakeMatchup();

        MatchModel.MarketFields over = MatchModel.Fields(matchup, MarketSelection.TotalCorners(9.5, true));
        Assert.Equal("TOTAL CORNERS", over.Market);
        Assert.Equal("OVER 9.5 CORNERS", over.Line);

        MatchModel.MarketFields under = MatchModel.Fields(matchup, MarketSelection.TotalCorners(9.5, false));
        Assert.Equal("TOTAL CORNERS", under.Market);
        Assert.Equal("UNDER 9.5 CORNERS", under.Line);
    }

    [Fact]
    public void Total_cards_over_and_under_are_pattern_extensions_of_total_goals()
    {
        Matchup matchup = MakeMatchup();

        MatchModel.MarketFields over = MatchModel.Fields(matchup, MarketSelection.TotalCards(4.5, true));
        Assert.Equal("TOTAL CARDS", over.Market);
        Assert.Equal("OVER 4.5 CARDS", over.Line);

        MatchModel.MarketFields under = MatchModel.Fields(matchup, MarketSelection.TotalCards(4.5, false));
        Assert.Equal("TOTAL CARDS", under.Market);
        Assert.Equal("UNDER 4.5 CARDS", under.Line);
    }

    [Fact]
    public void Anytime_scorer_fields_are_DS_verbatim_and_carry_the_player_as_subject()
    {
        Matchup matchup = MakeMatchup();
        // Player index 0 is the away roster's first player (away roster first, then home —
        // Matchup.PlayerAt's stable ordering).
        MatchModel.MarketFields forward = MatchModel.Fields(matchup, MarketSelection.AnytimeScorer(0));
        Assert.Equal("ANYTIME SCORER", forward.Market);
        Assert.Equal("AWAY FORWARD ANYTIME", forward.Line);
        Assert.Equal("AWAY FORWARD", forward.Subject);
        Assert.Equal("FORWARD", forward.Role);
        Assert.Equal("Away Team v Home Team", forward.Fixture);
    }

    [Fact]
    public void Role_is_the_full_word_for_every_position_and_empty_for_every_non_scorer_market()
    {
        Matchup matchup = MakeMatchup();

        Assert.Equal("FORWARD", MatchModel.Fields(matchup, MarketSelection.AnytimeScorer(0)).Role);
        Assert.Equal("MIDFIELDER", MatchModel.Fields(matchup, MarketSelection.AnytimeScorer(1)).Role);
        Assert.Equal("DEFENDER", MatchModel.Fields(matchup, MarketSelection.AnytimeScorer(2)).Role);

        Assert.Equal("", MatchModel.Fields(matchup, MarketSelection.Moneyline(Side.Home)).Role);
        Assert.Equal("", MatchModel.Fields(matchup, MarketSelection.TotalGoals(2.5, true)).Role);
        Assert.Equal("", MatchModel.Fields(matchup, MarketSelection.BothTeamsToScore(true)).Role);
        Assert.Equal("", MatchModel.Fields(matchup, MarketSelection.TotalCorners(9.5, true)).Role);
        Assert.Equal("", MatchModel.Fields(matchup, MarketSelection.TotalCards(4.5, true)).Role);
    }

    [Fact]
    public void Fixture_is_away_v_home_for_every_market_kind()
    {
        Matchup matchup = MakeMatchup();
        foreach (MarketKind kind in Enum.GetValues<MarketKind>())
            Assert.Equal("Away Team v Home Team", MatchModel.Fields(matchup, SelectionFor(kind)).Fixture);
    }

    [Fact]
    public void Fields_never_throws_for_any_market_kind()
    {
        Matchup matchup = MakeMatchup();
        foreach (MarketKind kind in Enum.GetValues<MarketKind>())
        {
            MatchModel.MarketFields fields = MatchModel.Fields(matchup, SelectionFor(kind));
            Assert.NotNull(fields.Market);
        }
    }

    private static MarketSelection SelectionFor(MarketKind kind) => kind switch
    {
        MarketKind.Moneyline => MarketSelection.Moneyline(Side.Home),
        MarketKind.TotalGoals => MarketSelection.TotalGoals(2.5, true),
        MarketKind.BothTeamsToScore => MarketSelection.BothTeamsToScore(true),
        MarketKind.TotalCorners => MarketSelection.TotalCorners(9.5, true),
        MarketKind.TotalCards => MarketSelection.TotalCards(4.5, true),
        MarketKind.AnytimeScorer => MarketSelection.AnytimeScorer(0),
        _ => throw new InvalidOperationException($"Unhandled {kind}; extend this test's fixture."),
    };
}
