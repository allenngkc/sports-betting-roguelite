using System;
using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// M-01: the anytime-scorer board is one flat list spanning both rosters
/// (MatchModel.BuildOffers), so player names must be unique across the WHOLE matchup board,
/// not just within one team. These tests pin the fix in SlateGenerator.MakeRoster.
/// </summary>
public class RosterNameTests
{
    [Fact]
    public void Board_names_are_unique_across_both_rosters_over_many_slates()
    {
        var config = new RunConfig();
        const int slateCount = 2000;
        int matchupsChecked = 0;

        for (int s = 0; s < slateCount; s++)
        {
            Slate slate = SlateGenerator.Generate(1, new RngHub($"ROSTER-UNIQ-{s}"), config);
            foreach (Matchup m in slate.Matchups)
            {
                var boardNames = m.Away.Players.Select(p => p.Name)
                    .Concat(m.Home.Players.Select(p => p.Name))
                    .ToList();
                Assert.Equal(boardNames.Count, boardNames.Distinct().Count());
                matchupsChecked++;
            }
        }

        Assert.True(matchupsChecked >= slateCount); // one matchup per slate at minimum, sanity check
    }

    [Fact]
    public void Same_seed_regenerates_byte_identical_rosters()
    {
        var config = new RunConfig();
        const string seed = "ROSTER-DETERMINISM";

        Slate a = SlateGenerator.Generate(1, new RngHub(seed), config);
        Slate b = SlateGenerator.Generate(1, new RngHub(seed), config);

        Assert.Equal(a.Matchups.Count, b.Matchups.Count);
        for (int i = 0; i < a.Matchups.Count; i++)
        {
            AssertRosterIdentical(a.Matchups[i].Home.Players, b.Matchups[i].Home.Players);
            AssertRosterIdentical(a.Matchups[i].Away.Players, b.Matchups[i].Away.Players);
        }
    }

    private static void AssertRosterIdentical(System.Collections.Generic.IReadOnlyList<Player> x,
        System.Collections.Generic.IReadOnlyList<Player> y)
    {
        Assert.Equal(x.Count, y.Count);
        for (int i = 0; i < x.Count; i++)
        {
            Assert.Equal(x[i].Name, y[i].Name);
            Assert.Equal(x[i].Role, y[i].Role);
            Assert.Equal(x[i].ScoringWeight, y[i].ScoringWeight);
        }
    }

    /// <summary>The key invariance: AnytimeScorer pricing (MatchModel.TrueProbability) is a pure
    /// function of role/weight/side/latents and never reads Player.Name. Two matchups built with
    /// identical latents and identical role/weight vectors but DIFFERENT names must price every
    /// scorer-board index identically — proving the name-uniqueness fix cannot move a price.</summary>
    [Fact]
    public void Anytime_scorer_pricing_is_unaffected_by_player_names()
    {
        var config = new RunConfig();
        var latents = new MatchLatents(1.31, 1.65, 4.96, 6.25, 2.14, 1.96);

        var homeA = MakeRoleRoster(config, "Home-A");
        var awayA = MakeRoleRoster(config, "Away-A");
        var homeB = MakeRoleRoster(config, "Home-B-DifferentNames");
        var awayB = MakeRoleRoster(config, "Away-B-DifferentNames");

        var matchupA = new Matchup(0, new Team("Home", 5, 4, homeA), new Team("Away", 4, 5, awayA),
            0.5, 1.9, 1.9, latents, default, default, Array.Empty<MarketOffer>(), config);
        var matchupB = new Matchup(0, new Team("Home", 5, 4, homeB), new Team("Away", 4, 5, awayB),
            0.5, 1.9, 1.9, latents, default, default, Array.Empty<MarketOffer>(), config);

        int boardSize = awayA.Count + homeA.Count;
        for (int i = 0; i < boardSize; i++)
        {
            double priceA = matchupA.TrueProb(MarketSelection.AnytimeScorer(i));
            double priceB = matchupB.TrueProb(MarketSelection.AnytimeScorer(i));
            Assert.Equal(priceA, priceB, 15);
        }
    }

    [Fact]
    public void Exhausting_the_name_pool_throws_a_clear_error_naming_the_dial()
    {
        // 73 * 2 = 146 > the 144 available PlayerFirst x PlayerLast combinations.
        var config = new RunConfig { PlayersPerTeam = 73 };

        var ex = Assert.Throws<InvalidOperationException>(
            () => SlateGenerator.Generate(1, new RngHub("ROSTER-EXHAUST"), config));
        Assert.Contains("PlayersPerTeam", ex.Message);
    }

    /// <summary>Builds a roster with the exact index-derived role/weight pattern SlateGenerator
    /// uses (3 FW, 2 MF, 2 DF repeating every 7), but caller-chosen names — isolating name from
    /// role/weight so pricing comparisons are clean.</summary>
    private static System.Collections.Generic.List<Player> MakeRoleRoster(RunConfig config, string namePrefix)
    {
        var players = new System.Collections.Generic.List<Player>(config.PlayersPerTeam);
        for (int i = 0; i < config.PlayersPerTeam; i++)
        {
            PlayerRole role = i % 7 < 3 ? PlayerRole.FW : i % 7 < 5 ? PlayerRole.MF : PlayerRole.DF;
            double weight = role == PlayerRole.FW ? config.ForwardScoringWeight
                : role == PlayerRole.MF ? config.MidfielderScoringWeight : config.DefenderScoringWeight;
            players.Add(new Player($"{namePrefix}-{i}", role, weight));
        }
        return players;
    }
}
