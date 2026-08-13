using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// Phase 1 of F_0.6.0. The two exit gates are <c>Gate1_*</c> and <c>Gate2_*</c>; everything else
/// pins a named shape from <c>design/02-betting-math.md</c> § *Same-game tickets* or from the
/// reconnaissance in <c>docs/sgp/correlation-recon.md</c>.
/// </summary>
public class JointModelTests
{
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public JointModelTests(Xunit.Abstractions.ITestOutputHelper output) => _output = output;

    /// <summary>POP-A, the reconnaissance's primary population: real generated slates, so scorer
    /// legs carry genuine jittered rosters rather than synthetic flat weights.</summary>
    private static List<Matchup> Population(int seeds, int rounds)
    {
        var config = new RunConfig();
        var matchups = new List<Matchup>(seeds * rounds * config.MatchupsPerSlate);
        for (int seed = 0; seed < seeds; seed++)
        {
            var hub = new RngHub($"joint-{seed}");
            for (int round = 0; round < rounds; round++)
                matchups.AddRange(SlateGenerator.Generate(round, hub, config).Matchups);
        }
        return matchups;
    }

    private static Matchup OneMatchup() => Population(1, 1)[0];

    /// <summary>Every selection the shipped board offers on a matchup — 36 of them.</summary>
    private static MarketSelection[] Board(Matchup matchup)
    {
        var board = new MarketSelection[matchup.Markets.Count];
        for (int i = 0; i < board.Length; i++) board[i] = matchup.Markets[i].Selection;
        return board;
    }

    private static double Marginal(Matchup matchup, MarketSelection selection)
        => JointModel.JointProbability(matchup, new[] { selection }).pJoint;

    private static bool Correlated(double joint, double product)
        => Math.Abs(joint - product) > JointModel.CorrelationTolerance * product;

    private static bool AnyLabelled(IReadOnlyList<Relation> relations)
    {
        for (int i = 0; i < relations.Count; i++)
            if (relations[i].Kind != RelationKind.Independent) return true;
        return false;
    }

    private static string Describe(Matchup matchup, MarketSelection selection)
    {
        MatchModel.MarketFields f = MatchModel.Fields(matchup, selection);
        return string.IsNullOrEmpty(f.Line) ? $"{f.Market} {f.Subject}" : f.Line;
    }

    // =======================================================================================
    // EXIT GATE 1 — numerical.
    // =======================================================================================

    /// <summary>The evaluator called with a one-element list must reproduce the engine's own price
    /// for every selection on the shipped board. This is what makes the joint path safe to graft on
    /// beside the existing one: a same-game ticket that happens to hold one leg prices as today.</summary>
    [Fact]
    public void Gate1_single_selection_joint_equals_MatchModel_TrueProbability()
    {
        double worst = 0.0;
        string worstAt = "none";
        long checks = 0;
        long exact = 0;

        // POP-A as the reconnaissance defined it: 250 seeds x 8 rounds x 6 matchups = 12,000.
        List<Matchup> population = Population(250, 8);
        foreach (Matchup matchup in population)
            foreach (MarketSelection selection in Board(matchup))
            {
                (double pJoint, IReadOnlyList<Relation> relations) =
                    JointModel.JointProbability(matchup, new[] { selection });

                // One leg is one leg: nothing to relate it to.
                Assert.Empty(relations);

                double truth = MatchModel.TrueProbability(matchup, selection);
                double deviation = Math.Abs(pJoint - truth);
                checks++;
                if (deviation == 0.0) exact++;
                if (deviation > worst)
                {
                    worst = deviation;
                    worstAt = $"{Describe(matchup, selection)} (joint {pJoint:R} vs true {truth:R})";
                }
            }

        _output.WriteLine($"gate 1: {checks:N0} single-selection checks over {population.Count:N0} matchups");
        _output.WriteLine($"gate 1: max |deviation| = {worst:E3}   worst at {worstAt}");
        _output.WriteLine($"gate 1: bit-identical on {exact:N0} of {checks:N0} ({100.0 * exact / checks:0.00}%)");
        Assert.True(worst < 1e-12, $"max absolute deviation {worst:E3} at {worstAt}");
    }

    /// <summary>The goal-family sum runs over the model's outcome partition W. Whatever W's size,
    /// its weights must sum to 1 — BTTS YES and NO are exact complements in any model, draws or not,
    /// so this holds across the Lane 1 change without editing.</summary>
    [Fact]
    public void Goal_family_outcome_partition_sums_to_one()
    {
        double worst = 0.0;
        foreach (Matchup matchup in Population(10, 4))
        {
            double yes = Marginal(matchup, MarketSelection.BothTeamsToScore(true));
            double no = Marginal(matchup, MarketSelection.BothTeamsToScore(false));
            worst = Math.Max(worst, Math.Abs(yes + no - 1.0));
        }
        _output.WriteLine($"partition closure: max |P(BTTS Y) + P(BTTS N) - 1| = {worst:E3}");
        Assert.True(worst < 1e-12, $"outcome partition does not close: {worst:E3}");
    }

    // =======================================================================================
    // EXIT GATE 2 — totality of labelling.
    // =======================================================================================

    /// <summary>Design canon: "Where the model finds correlation it cannot label, the price does not
    /// move." An unlabelable correlation is therefore a silent pricing hole, so no combination on the
    /// shipped board may be correlated yet carry only <see cref="RelationKind.Independent"/> labels.</summary>
    [Fact]
    public void Gate2_no_pair_on_the_board_is_correlated_but_unlabelable()
    {
        var tally = new SortedDictionary<string, int>();
        long pairs = 0;
        long correlated = 0;
        long unlabelable = 0;
        string firstHole = "none";

        foreach (Matchup matchup in Population(20, 2))
        {
            MarketSelection[] board = Board(matchup);
            var marginals = new double[board.Length];
            for (int i = 0; i < board.Length; i++) marginals[i] = Marginal(matchup, board[i]);

            for (int i = 0; i < board.Length; i++)
                for (int j = i + 1; j < board.Length; j++)
                {
                    (double pJoint, IReadOnlyList<Relation> relations) =
                        JointModel.JointProbability(matchup, new[] { board[i], board[j] });

                    // Exactly one relation per pair — the classifier is a total function on pairs.
                    Relation relation = Assert.Single(relations);
                    tally.TryGetValue(relation.Kind.ToString(), out int seen);
                    tally[relation.Kind.ToString()] = seen + 1;

                    pairs++;
                    if (!Correlated(pJoint, marginals[i] * marginals[j])) continue;
                    correlated++;
                    if (AnyLabelled(relations)) continue;

                    unlabelable++;
                    if (firstHole == "none")
                        firstHole = $"{Describe(matchup, board[i])} + {Describe(matchup, board[j])} "
                            + $"(joint {pJoint:R}, product {marginals[i] * marginals[j]:R})";
                }
        }

        _output.WriteLine($"gate 2 pairs: {pairs:N0} evaluated, {correlated:N0} correlated, "
            + $"{unlabelable:N0} correlated-but-unlabelable");
        foreach (KeyValuePair<string, int> entry in tally)
            _output.WriteLine($"gate 2 pairs:   {entry.Key,-18} {entry.Value,8:N0}");
        Assert.True(unlabelable == 0,
            $"{unlabelable:N0} pairs are correlated but carry no label; first: {firstHole}");
        Assert.True(correlated > 0, "the sweep found no correlation at all — it is not testing anything");
    }

    /// <summary>Same gate at three legs, where the interesting failure lives: a ticket can reach
    /// p_joint = 0 with every sub-pair strictly positive (recon §6.2 counts 57 such shapes), and a
    /// purely pairwise classifier would leave those unlabelled.</summary>
    [Fact]
    public void Gate2_no_triple_on_the_board_is_correlated_but_unlabelable()
    {
        var tally = new SortedDictionary<string, int>();
        long triples = 0;
        long correlated = 0;
        long unlabelable = 0;
        long ticketLevelExclusions = 0;
        string firstHole = "none";

        foreach (Matchup matchup in Population(3, 1))
        {
            MarketSelection[] board = Board(matchup);
            var marginals = new double[board.Length];
            for (int i = 0; i < board.Length; i++) marginals[i] = Marginal(matchup, board[i]);

            for (int i = 0; i < board.Length; i++)
                for (int j = i + 1; j < board.Length; j++)
                    for (int k = j + 1; k < board.Length; k++)
                    {
                        (double pJoint, IReadOnlyList<Relation> relations) =
                            JointModel.JointProbability(matchup, new[] { board[i], board[j], board[k] });

                        Assert.True(relations.Count >= 3, "every leg pair must be classified");
                        for (int r = 0; r < relations.Count; r++)
                        {
                            tally.TryGetValue(relations[r].Kind.ToString(), out int seen);
                            tally[relations[r].Kind.ToString()] = seen + 1;
                            if (relations[r].Kind == RelationKind.MutuallyExclusive
                                && relations[r].Legs.Count == 3)
                                ticketLevelExclusions++;
                        }

                        // The zero check is a validity test, never subject to the no-label fallback.
                        if (pJoint == 0.0)
                            Assert.Contains(relations, r => r.Kind == RelationKind.MutuallyExclusive);

                        triples++;
                        double product = marginals[i] * marginals[j] * marginals[k];
                        if (!Correlated(pJoint, product)) continue;
                        correlated++;
                        if (AnyLabelled(relations)) continue;

                        unlabelable++;
                        if (firstHole == "none")
                            firstHole = $"{Describe(matchup, board[i])} + {Describe(matchup, board[j])} "
                                + $"+ {Describe(matchup, board[k])} (joint {pJoint:R}, product {product:R})";
                    }
        }

        _output.WriteLine($"gate 2 triples: {triples:N0} evaluated, {correlated:N0} correlated, "
            + $"{unlabelable:N0} correlated-but-unlabelable");
        _output.WriteLine($"gate 2 triples: {ticketLevelExclusions:N0} impossible with no impossible sub-pair "
            + "(ticket-level exclusion label)");
        foreach (KeyValuePair<string, int> entry in tally)
            _output.WriteLine($"gate 2 triples:   {entry.Key,-18} {entry.Value,8:N0}");
        Assert.True(unlabelable == 0,
            $"{unlabelable:N0} triples are correlated but carry no label; first: {firstHole}");
        Assert.True(ticketLevelExclusions > 0,
            "no triple reached zero without an impossible sub-pair — the ticket-level label is untested");
    }

    // =======================================================================================
    // Named shapes.
    // =======================================================================================

    /// <summary>Both teams scoring needs a total of at least 2. Impossible in any model, draws or
    /// not — and the naive product sells it at a mean decimal of 8.00 (recon §6.1).</summary>
    [Fact]
    public void Btts_yes_with_under_one_and_a_half_is_impossible_and_mutually_exclusive()
    {
        Matchup matchup = OneMatchup();
        var legs = new[] { MarketSelection.BothTeamsToScore(true), MarketSelection.TotalGoals(1.5, false) };

        (double pJoint, IReadOnlyList<Relation> relations) = JointModel.JointProbability(matchup, legs);

        Assert.True(pJoint == 0.0, $"expected exactly 0.0, got {pJoint:R}");
        Relation relation = Assert.Single(relations);
        Assert.Equal(RelationKind.MutuallyExclusive, relation.Kind);
        Assert.Equal(new[] { 0, 1 }, relation.Legs);
        Assert.Equal(RelationSign.None, relation.Sign);
    }

    /// <summary>Over 2.5 strictly entails Over 1.5, so the joint is the stronger leg's own price and
    /// the second leg adds zero risk. Not blocked — the joint prices it correctly and the player
    /// simply pays two legs of vig for one leg of risk.</summary>
    [Fact]
    public void Over_one_and_a_half_with_over_two_and_a_half_is_an_implication()
    {
        Matchup matchup = OneMatchup();
        var legs = new[] { MarketSelection.TotalGoals(1.5, true), MarketSelection.TotalGoals(2.5, true) };

        (double pJoint, IReadOnlyList<Relation> relations) = JointModel.JointProbability(matchup, legs);

        double stronger = MatchModel.TrueProbability(matchup, legs[1]);
        Assert.True(pJoint == stronger, $"expected exactly {stronger:R}, got {pJoint:R}");

        Relation relation = Assert.Single(relations);
        Assert.Equal(RelationKind.Implies, relation.Kind);
        // Legs[0] implies Legs[1]: Over 2.5 (index 1) implies Over 1.5 (index 0).
        Assert.Equal(new[] { 1, 0 }, relation.Legs);
        Assert.Equal(RelationSign.Reinforcing, relation.Sign);
    }

    /// <summary>Corners are drawn without reference to the score, so a goal leg beside a corner leg
    /// is the naive product exactly — a correlation model that produced anything else here would be
    /// inventing error the engine does not contain (recon §10.4).</summary>
    [Fact]
    public void Cross_family_pair_is_exactly_the_product_and_labelled_independent()
    {
        Matchup matchup = OneMatchup();
        var legs = new[] { MarketSelection.TotalGoals(2.5, true), MarketSelection.TotalCorners(9.5, true) };

        (double pJoint, IReadOnlyList<Relation> relations) = JointModel.JointProbability(matchup, legs);

        double product = MatchModel.TrueProbability(matchup, legs[0]) * MatchModel.TrueProbability(matchup, legs[1]);
        Assert.True(pJoint == product, $"expected exactly {product:R}, got {pJoint:R}");

        Relation relation = Assert.Single(relations);
        Assert.Equal(RelationKind.Independent, relation.Kind);
        Assert.Null(relation.Family);
    }

    /// <summary>The g &lt; k guard. Two players on one team both scoring inside at most one total
    /// goal is structurally impossible; without the guard the inclusion-exclusion sum cancels to
    /// ~1e-17 instead of 0 and the ticket passes every zero check.</summary>
    [Fact]
    public void Two_scorers_on_one_team_cannot_both_score_inside_one_goal()
    {
        Matchup matchup = OneMatchup();
        int first = matchup.Away.Players.Count;      // board index of the first HOME player
        int second = first + 1;
        Assert.Equal(Side.Home, matchup.PlayerSide(first));
        Assert.Equal(Side.Home, matchup.PlayerSide(second));

        var legs = new[]
        {
            MarketSelection.AnytimeScorer(first),
            MarketSelection.AnytimeScorer(second),
            MarketSelection.TotalGoals(1.5, false),
        };

        (double pJoint, IReadOnlyList<Relation> relations) = JointModel.JointProbability(matchup, legs);
        Assert.True(pJoint == 0.0, $"expected exactly 0.0, got {pJoint:R}");

        // No sub-pair is impossible on its own, so the exclusion can only come from the whole
        // ticket — this is one of the 57 triple shapes of recon §6.2.
        for (int i = 0; i < legs.Length; i++)
            for (int j = i + 1; j < legs.Length; j++)
                Assert.True(JointModel.JointProbability(matchup, new[] { legs[i], legs[j] }).pJoint > 0.0,
                    $"sub-pair ({i},{j}) should be possible on its own");
        Assert.Contains(relations, r => r.Kind == RelationKind.MutuallyExclusive && r.Legs.Count == 3);

        // The two scorers alone are perfectly possible, and the same goals settle both.
        (double pPair, IReadOnlyList<Relation> pairRelations) =
            JointModel.JointProbability(matchup, new[] { legs[0], legs[1] });
        Assert.True(pPair > 0.0);
        Relation scorerRelation = Assert.Single(pairRelations);
        Assert.Equal(RelationKind.ScorerOfSide, scorerRelation.Kind);
        Assert.Equal(Side.Home, scorerRelation.ScorerSide);
    }

    /// <summary>Why that guard is normative rather than an optimization: the raw inclusion-exclusion
    /// sum really does land on a tiny POSITIVE number for k = 2, g = 1 on this board's jittered
    /// rosters. Measured, so the claim is not taken on faith.</summary>
    [Fact]
    public void Unguarded_inclusion_exclusion_cancels_to_a_positive_epsilon_not_to_zero()
    {
        long cases = 0;
        long nonZero = 0;
        double worst = 0.0;

        foreach (Matchup matchup in Population(5, 2))
        {
            double total = 0.0;
            foreach (Player player in matchup.Home.Players) total += player.ScoringWeight;
            IReadOnlyList<Player> roster = matchup.Home.Players;

            for (int i = 0; i < roster.Count; i++)
                for (int j = i + 1; j < roster.Count; j++)
                {
                    double w1 = roster[i].ScoringWeight / total;
                    double w2 = roster[j].ScoringWeight / total;
                    // Q(g=1) for k=2, written out: 1 - (1-w1) - (1-w2) + (1-w1-w2). Exactly 0 in
                    // real arithmetic; not in IEEE double.
                    double unguarded = 1.0 - (1.0 - w1) - (1.0 - w2) + (1.0 - w1 - w2);
                    cases++;
                    if (unguarded != 0.0) nonZero++;
                    worst = Math.Max(worst, Math.Abs(unguarded));
                }
        }

        _output.WriteLine($"g < k trap: {nonZero:N0} of {cases:N0} two-player terms cancel to a non-zero "
            + $"epsilon; largest magnitude {worst:E3}");
        Assert.True(nonZero > 0, "the cancellation trap did not reproduce — re-check the guard's rationale");
        Assert.True(worst < 1e-15, "the residual should be floating-point dust, not a real quantity");
    }

    /// <summary>DESIGN GAP, pinned deliberately. The canon relation table gives
    /// <c>SharedScoreline</c> for "two GOAL-family legs" and nothing for two legs of the same COUNT
    /// family. The board ships three corner lines and three card lines, so shapes like OVER 8.5 +
    /// UNDER 10.5 corners — a band, neither impossible nor an implication — have no canon label and
    /// would fall through to the naive product. <see cref="RelationKind.SharedCount"/> is the flagged
    /// extension that closes it. If the Design Director renames or reshapes that relation, this test
    /// is the place it lands.</summary>
    [Fact]
    public void Corner_band_pairs_use_the_flagged_SharedCount_extension()
    {
        Matchup matchup = OneMatchup();
        var legs = new[] { MarketSelection.TotalCorners(8.5, true), MarketSelection.TotalCorners(10.5, false) };

        (double pJoint, IReadOnlyList<Relation> relations) = JointModel.JointProbability(matchup, legs);

        double product = Marginal(matchup, legs[0]) * Marginal(matchup, legs[1]);
        Assert.True(pJoint > 0.0, "the band is possible");
        Assert.True(Correlated(pJoint, product), "the band is correlated, so it must carry a label");

        Relation relation = Assert.Single(relations);
        Assert.Equal(RelationKind.SharedCount, relation.Kind);
        Assert.Equal(SelectionFamily.Corner, relation.Family);
        Assert.NotEqual(RelationSign.None, relation.Sign);
    }

    /// <summary>Backing the same player twice is the same event: P(A and A) = P(A). Undeduped it
    /// would inflate k and the g &lt; k guard would call an ordinary ticket impossible.</summary>
    [Fact]
    public void Backing_one_player_twice_is_idempotent()
    {
        Matchup matchup = OneMatchup();
        MarketSelection scorer = MarketSelection.AnytimeScorer(0);

        double single = Marginal(matchup, scorer);
        (double pJoint, IReadOnlyList<Relation> relations) =
            JointModel.JointProbability(matchup, new[] { scorer, scorer });

        Assert.True(pJoint == single, $"expected exactly {single:R}, got {pJoint:R}");
        Relation relation = Assert.Single(relations);
        Assert.Equal(RelationKind.Implies, relation.Kind);
    }

    [Fact]
    public void An_empty_ticket_is_rejected_rather_than_priced_at_one()
    {
        Matchup matchup = OneMatchup();
        Assert.Throws<ArgumentException>(() =>
            JointModel.JointProbability(matchup, Array.Empty<MarketSelection>()));
    }
}
