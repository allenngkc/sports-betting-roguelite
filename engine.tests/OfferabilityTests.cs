using System;
using System.Collections.Generic;
using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// THE CRASH GUARD. `MatchModel.Offer` throws when a selection prices at odds ≤ 1.0, i.e. true
/// probability ≥ 1/(1+overround) = 0.95238. That is correct behaviour — a book cannot offer a
/// price below evens — but it means an over-generous LINE is not a bad number, it is an
/// exception, and it surfaces wherever the latent box happens to reach that corner.
///
/// Without this sweep that discovery costs a campaign: the gate run locks on the order of 10⁷
/// matchups, so a line that only crashes at p≈0.75 with low tempo will find it, twenty minutes in.
/// With it, the same mistake fails here in seconds.
///
/// Two lines were excluded from V1 on exactly this evidence, and both are re-asserted below so
/// they cannot quietly return: handicap ±2.5 (favourite side reaches p 0.984) and team-total 2.5
/// (under reaches 0.949 under draws — legal, but one rounding step from a crash and a 1.00 price).
/// </summary>
public class OfferabilityTests
{
    /// <summary>The whole latent box the generator can produce: strength across its full band, and
    /// every tempo at both extremes and centre. Corners, not just centres — the centre of this box
    /// prices nothing dangerously.</summary>
    private static IEnumerable<Matchup> SweepTheLatentBox()
    {
        var config = new RunConfig();
        foreach (double p in new[] { config.MinTrueProb, 0.35, 0.5, 0.65, config.MaxTrueProb })
            foreach (double tg in new[] { 1 - config.GoalTempoSpread, 1.0, 1 + config.GoalTempoSpread })
                foreach (double tc in new[] { 1 - config.CornerTempoSpread, 1.0, 1 + config.CornerTempoSpread })
                    foreach (double td in new[] { 1 - config.DisciplineSpread, 1.0, 1 + config.DisciplineSpread })
                    {
                        MatchLatents l = MatchModel.LatentsFor(p, tg, tc, td, config);
                        var home = new Team("Home Side", 5, 4, Roster(config, "H"));
                        var away = new Team("Away Side", 4, 5, Roster(config, "A"));
                        yield return new Matchup(0, home, away, p, 2.0, 2.0, l,
                            default, default, Array.Empty<MarketOffer>(), config);
                    }
    }

    private static IReadOnlyList<Player> Roster(RunConfig config, string tag)
    {
        var players = new List<Player>(config.PlayersPerTeam);
        for (int i = 0; i < config.PlayersPerTeam; i++)
        {
            PlayerRole role = i % 7 < 3 ? PlayerRole.FW : i % 7 < 5 ? PlayerRole.MF : PlayerRole.DF;
            double w = role == PlayerRole.FW ? config.ForwardScoringWeight
                : role == PlayerRole.MF ? config.MidfielderScoringWeight : config.DefenderScoringWeight;
            players.Add(new Player($"{tag}{i} Player", role, w));
        }
        return players;
    }

    [Fact]
    public void Every_offered_selection_prices_above_evens_across_the_whole_latent_box()
    {
        var config = new RunConfig();
        foreach (Matchup m in SweepTheLatentBox())
        {
            IReadOnlyList<MarketOffer> offers = MatchModel.BuildOffers(m, config);
            Assert.NotEmpty(offers);
            foreach (MarketOffer o in offers)
                Assert.True(o.Odds > 1.0,
                    $"{o.Selection.Kind}/{o.Selection.Choice} line {o.Selection.Line} priced at "
                    + $"{o.Odds:0.000} (p={o.TrueProb:0.0000}) — Offer() would throw at this seed");
        }
    }

    [Fact]
    public void The_excluded_lines_are_excluded_because_they_CRASH_and_here_is_the_proof()
    {
        var config = new RunConfig();
        bool handicap25Throws = false, teamTotal25Dangerous = false;
        double worstHandicap = 0.0, worstTeamTotal = 0.0;

        foreach (Matchup m in SweepTheLatentBox())
        {
            // Handicap ±2.5: the DOG side covers everything but a 3+ defeat, so its probability is
            // the one that runs at the ceiling.
            double dog = MatchModel.TrueProbability(m, MarketSelection.Handicap(Side.Away, 2.5));
            worstHandicap = Math.Max(worstHandicap, dog);
            if (dog >= 1.0 / (1.0 + config.Overround)) handicap25Throws = true;

            double tt = MatchModel.TrueProbability(m, MarketSelection.TeamTotalGoals(Side.Home, 2.5, false));
            worstTeamTotal = Math.Max(worstTeamTotal, tt);
            if (tt >= 0.94) teamTotal25Dangerous = true;
        }

        Assert.True(handicap25Throws,
            $"handicap ±2.5 was excluded because it CRASHES; worst reading was only {worstHandicap:P1}, "
            + "so either the model moved or the exclusion is now unjustified — re-derive it, do not "
            + "just re-enable the line");
        Assert.True(teamTotal25Dangerous,
            $"team-total 2.5 was dropped for pricing at the ceiling; worst under reading was "
            + $"{worstTeamTotal:P1}");

        // Stated as measurements rather than asserted bounds: these are what the exclusions rest on.
        Assert.InRange(worstHandicap, 0.95, 1.0);
        Assert.InRange(worstTeamTotal, 0.90, 0.96);
    }

    /// <summary>Pricing and grading must agree on every V1 market, on a forced stat line. The
    /// divergence failure mode (priced off one distribution, graded off another) is killed
    /// structurally by the shared enumeration — this is the assertion that says so out loud.</summary>
    [Fact]
    public void Every_market_grades_consistently_with_the_score_it_is_priced_from()
    {
        var run = new Run("V1-GRADE");
        run.LockRound();
        Matchup m = run.CurrentSlate.Matchups[0];
        MatchStatLine line = m.StatLine!;
        int h = line.HomeGoals, a = line.AwayGoals;

        foreach (MarketOffer offer in m.Markets)
        {
            MarketSelection s = offer.Selection;
            bool? expected = s.Kind switch
            {
                MarketKind.DoubleChance => s.Choice switch
                {
                    MarketChoice.HomeOrDraw => h >= a,
                    MarketChoice.AwayOrDraw => a >= h,
                    _ => h != a,
                },
                MarketKind.Handicap => s.Choice == MarketChoice.Home
                    ? h + s.Line > a
                    : a + s.Line > h,
                MarketKind.TeamTotalGoals => Over(s, s.Team == Side.Home ? h : a),
                MarketKind.CorrectScore => h == s.ScoreHome && a == s.ScoreAway,
                MarketKind.WinningMargin => (int)s.Line >= 3
                    ? Math.Abs(h - a) >= (int)s.Line
                    : Math.Abs(h - a) == (int)s.Line,
                MarketKind.TotalGoalsOddEven => (((h + a) & 1) == 1) == (s.Choice == MarketChoice.Odd),
                MarketKind.TeamTotalCorners => Over(s, s.Team == Side.Home ? line.HomeCorners : line.AwayCorners),
                MarketKind.TeamTotalCards => Over(s, s.Team == Side.Home ? line.HomeCards : line.AwayCards),
                _ => null, // older kinds and the player markets have their own tests
            };
            if (expected is bool want)
                Assert.Equal(want, m.Grades(s));
        }
    }

    private static bool Over(MarketSelection s, int value)
        => s.Choice == MarketChoice.Over ? value > s.Line : value < s.Line;
}
