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
                (double pJoint, IReadOnlyList<Relation> relations, _) =
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
    /// shipped board may be correlated yet carry only <see cref="RelationKind.Independent"/> labels.
    ///
    /// <para>EVERY pair on the board, on every matchup of the population. The population shrank from
    /// 240 matchups to 48 when F_0.5.0 took the board from 36 selections to 83 — the pair count per
    /// matchup went up 5.3x, and a label is a property of the SHAPE, so re-testing the same shapes on
    /// five times more latents bought minutes and no coverage.</para></summary>
    [Fact]
    public void Gate2_no_pair_on_the_board_is_correlated_but_unlabelable()
    {
        var tally = new SortedDictionary<string, int>();
        long pairs = 0;
        long correlated = 0;
        long unlabelable = 0;
        string firstHole = "none";

        foreach (Matchup matchup in Population(8, 1))
        {
            MarketSelection[] board = Board(matchup);
            var marginals = new double[board.Length];
            for (int i = 0; i < board.Length; i++) marginals[i] = Marginal(matchup, board[i]);

            for (int i = 0; i < board.Length; i++)
                for (int j = i + 1; j < board.Length; j++)
                {
                    (double pJoint, IReadOnlyList<Relation> relations, _) =
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
    /// p_joint = 0 with every sub-pair strictly positive (recon §6.2 counts 57 such shapes pre-draws),
    /// and a purely pairwise classifier would leave those unlabelled.
    ///
    /// <para>A LARGE DETERMINISTIC SAMPLE, not an exhaustive sweep. The 83-selection board carries
    /// 91,881 triples per matchup — 13x the pre-F_0.5.0 count — and exhausting them across the
    /// population ran for nine minutes to re-test the same shapes on more latents. The sample is drawn
    /// with a fixed-seed <see cref="Pcg32"/> so it is the same set on every run and in CI, and it is
    /// drawn from the WHOLE enumeration rather than a stride, so it cannot alias with the board's own
    /// ordering by market kind.</para></summary>
    [Fact]
    public void Gate2_no_triple_on_the_board_is_correlated_but_unlabelable()
    {
        const double SampleRate = 0.08;

        var tally = new SortedDictionary<string, int>();
        long offered = 0;
        long triples = 0;
        long correlated = 0;
        long unlabelable = 0;
        long ticketLevelExclusions = 0;
        string firstHole = "none";

        var sampler = new Pcg32(0xC0FFEE, 0x5A3);
        foreach (Matchup matchup in Population(3, 1))
        {
            MarketSelection[] board = Board(matchup);
            var marginals = new double[board.Length];
            for (int i = 0; i < board.Length; i++) marginals[i] = Marginal(matchup, board[i]);

            for (int i = 0; i < board.Length; i++)
                for (int j = i + 1; j < board.Length; j++)
                    for (int k = j + 1; k < board.Length; k++)
                    {
                        offered++;
                        if (sampler.NextDouble() >= SampleRate) continue;

                        (double pJoint, IReadOnlyList<Relation> relations, _) =
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

        _output.WriteLine($"gate 2 triples: {triples:N0} evaluated of {offered:N0} on the board "
            + $"({100.0 * triples / offered:0.0}% sample), {correlated:N0} correlated, "
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

        (double pJoint, IReadOnlyList<Relation> relations, _) = JointModel.JointProbability(matchup, legs);

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

        (double pJoint, IReadOnlyList<Relation> relations, _) = JointModel.JointProbability(matchup, legs);

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

        (double pJoint, IReadOnlyList<Relation> relations, _) = JointModel.JointProbability(matchup, legs);

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

        (double pJoint, IReadOnlyList<Relation> relations, _) = JointModel.JointProbability(matchup, legs);
        Assert.True(pJoint == 0.0, $"expected exactly 0.0, got {pJoint:R}");

        // No sub-pair is impossible on its own, so the exclusion can only come from the whole
        // ticket — this is one of the 57 triple shapes of recon §6.2.
        for (int i = 0; i < legs.Length; i++)
            for (int j = i + 1; j < legs.Length; j++)
                Assert.True(JointModel.JointProbability(matchup, new[] { legs[i], legs[j] }).pJoint > 0.0,
                    $"sub-pair ({i},{j}) should be possible on its own");
        Assert.Contains(relations, r => r.Kind == RelationKind.MutuallyExclusive && r.Legs.Count == 3);

        // The two scorers alone are perfectly possible, and the same goals settle both.
        (double pPair, IReadOnlyList<Relation> pairRelations, _) =
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

        (double pJoint, IReadOnlyList<Relation> relations, _) = JointModel.JointProbability(matchup, legs);

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
        (double pJoint, IReadOnlyList<Relation> relations, _) =
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

    // =======================================================================================
    // The F_0.5.0 merge: fifteen market kinds priced through the engine's own grader.
    // =======================================================================================

    /// <summary>
    /// THE ASSUMPTION THE WHOLE 15-KIND MERGE RESTS ON. The joint no longer restates any market's
    /// predicate: it synthesizes one stat line per grid cell — carrying only the family being
    /// enumerated, zeros everywhere else — and asks <c>MatchModel.Grades</c>. That is only sound if
    /// each kind's grader reads exactly the family it was routed to.
    ///
    /// <para>So: hold a selection's OWN family's fields fixed, vary the other two families' fields
    /// across a spread of values, and the verdict must not move. A goal predicate that peeked at corner
    /// counts would fail here rather than silently pricing every same-match ticket on the board off a
    /// zero.</para>
    ///
    /// <para>Scorer markets are excluded and are the two kinds this trick does NOT cover: they grade
    /// off <c>MatchStatLine.HomeScorers</c>, which a synthetic cell has no way to carry. They are
    /// priced by the joint's own scorer term instead, and the model throws rather than handing one to
    /// the grader.</para></summary>
    [Fact]
    public void Grades_reads_only_its_own_familys_fields()
    {
        (int h, int a)[] goalCells = { (0, 0), (1, 0), (0, 1), (1, 1), (2, 1), (3, 0), (4, 4) };
        (int h, int a)[] cornerCells = { (0, 0), (2, 7), (5, 5), (11, 9) };
        (int h, int a)[] cardCells = { (0, 0), (1, 2), (3, 3), (6, 1) };

        Matchup matchup = OneMatchup();
        long checks = 0;
        var kinds = new SortedSet<string>();

        foreach (MarketSelection selection in Board(matchup))
        {
            if (selection.Kind == MarketKind.AnytimeScorer
                || selection.Kind == MarketKind.PlayerMultiScorer) continue;
            kinds.Add(selection.Kind.ToString());

            SelectionFamily family = JointModel.FamilyOf(selection);
            (int h, int a)[] own = family switch
            {
                SelectionFamily.Goal => goalCells,
                SelectionFamily.Corner => cornerCells,
                _ => cardCells,
            };

            foreach ((int h, int a) cell in own)
            {
                bool? reference = null;
                foreach ((int h, int a) goals in family == SelectionFamily.Goal ? new[] { cell } : goalCells)
                    foreach ((int h, int a) corners in family == SelectionFamily.Corner ? new[] { cell } : cornerCells)
                        foreach ((int h, int a) cards in family == SelectionFamily.Card ? new[] { cell } : cardCells)
                        {
                            var line = new MatchStatLine(goals.h, goals.a, corners.h, corners.a, cards.h, cards.a);
                            bool graded = MatchModel.Grades(matchup, line, selection);
                            checks++;
                            reference ??= graded;
                            Assert.True(graded == reference,
                                $"{Describe(matchup, selection)} ({selection.Kind}) changed its verdict when a "
                                + $"field outside its own {family} family moved: {goals.h}-{goals.a} goals, "
                                + $"{corners.h}/{corners.a} corners, {cards.h}/{cards.a} cards");
                        }
            }
        }

        _output.WriteLine($"grader isolation: {checks:N0} checks over {kinds.Count} market kinds "
            + $"({string.Join(", ", kinds)})");
        Assert.Equal(13, kinds.Count); // fifteen kinds less the two scorer markets
    }

    /// <summary>Every one of the fifteen kinds actually reaches the joint. Without this the gates
    /// could stay green over a board that quietly stopped offering a kind, which is the same blind
    /// spot the merge itself exposed.</summary>
    [Fact]
    public void Every_market_kind_on_the_board_is_familied_and_priced()
    {
        Matchup matchup = OneMatchup();
        var kinds = new SortedSet<MarketKind>();
        foreach (MarketSelection selection in Board(matchup))
        {
            kinds.Add(selection.Kind);
            JointModel.FamilyOf(selection);
            Assert.True(Marginal(matchup, selection) > 0.0, $"{selection.Kind} priced at zero on its own");
        }

        foreach (MarketKind kind in Enum.GetValues<MarketKind>())
            Assert.True(kinds.Contains(kind), $"{kind} is not on the shipped board — the gates do not cover it");
    }

    /// <summary>An unmapped market kind THROWS rather than defaulting to a family. Guessing GOAL for a
    /// card market would price off the wrong draw and fail no test; this is the guard that turned
    /// F_0.5.0's nine new kinds into a red build instead of a silently mispriced board.</summary>
    [Fact]
    public void An_unfamilied_market_kind_throws_rather_than_guessing_a_draw()
    {
        var bogus = new MarketSelection((MarketKind)999, 0.0, MarketChoice.Yes);
        Assert.Throws<ArgumentOutOfRangeException>(() => JointModel.FamilyOf(bogus));
    }

    /// <summary>The outcome partition's closure check is a LOUD failure, not a clamp: a partition that
    /// does not sum to 1 misprices every goal-family ticket by an amount no player can see. Reached
    /// here with an impossible conditional home probability, which drives one class negative.</summary>
    [Fact]
    public void A_partition_that_is_not_a_partition_refuses_to_price()
    {
        Matchup real = OneMatchup();
        var broken = new Matchup(0, real.Home, real.Away, trueHomeProb: 5.0, 2.0, 2.0,
            real.Latents, real.HomeStats, real.AwayStats, Array.Empty<MarketOffer>(), real.ModelConfig);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            JointModel.JointProbability(broken, new[] { MarketSelection.BothTeamsToScore(true) }));
        Assert.Contains("partition", ex.Message);
    }

    /// <summary>MULTI-SCORER, THE ONE GENUINELY NEW SHAPE. "At least n" is not a union of "scores"
    /// events, so the joint's scorer term is the exact multinomial short-fall inclusion-exclusion, not
    /// the anytime formula with a fudge. Checked against the engine's own binomial price for every 2+
    /// row the board offers, across a real population.</summary>
    [Fact]
    public void A_multi_scorer_leg_prices_at_the_engines_own_at_least_n_probability()
    {
        double worst = 0.0;
        long checks = 0;
        foreach (Matchup matchup in Population(20, 2))
            foreach (MarketSelection selection in Board(matchup))
            {
                if (selection.Kind != MarketKind.PlayerMultiScorer) continue;
                worst = Math.Max(worst,
                    Math.Abs(Marginal(matchup, selection) - MatchModel.TrueProbability(matchup, selection)));
                checks++;
            }

        _output.WriteLine($"multi-scorer: {checks:N0} rows, max |deviation| = {worst:E3}");
        Assert.True(checks > 0, "the board offered no 2+ rows — the shape is untested");
        Assert.True(worst < 1e-12, $"max absolute deviation {worst:E3}");
    }

    /// <summary>A 2+ leg strictly entails the same player's anytime leg, and the model prices the pair
    /// at the stronger one exactly — the merged threshold, not a product of two scorer terms.</summary>
    [Fact]
    public void Two_plus_entails_the_same_players_anytime_leg()
    {
        Matchup matchup = OneMatchup();
        int player = FirstMultiScorerIndex(matchup);
        var legs = new[] { MarketSelection.AnytimeScorer(player), MarketSelection.PlayerMultiScorer(player) };

        (double pJoint, IReadOnlyList<Relation> relations, _) = JointModel.JointProbability(matchup, legs);

        double stronger = Marginal(matchup, legs[1]);
        Assert.True(pJoint == stronger, $"expected exactly {stronger:R}, got {pJoint:R}");
        Relation relation = Assert.Single(relations);
        Assert.Equal(RelationKind.Implies, relation.Kind);
        Assert.Equal(new[] { 1, 0 }, relation.Legs); // the 2+ leg implies the anytime leg
    }

    /// <summary>The short-fall guard generalizes the design doc's <c>g &lt; k</c> to
    /// <c>g &lt; SUM n_i</c>: one player scoring twice cannot fit inside one total goal. Exactly zero,
    /// not floating-point dust that passes every zero check.</summary>
    [Fact]
    public void One_player_cannot_score_twice_inside_a_single_goal()
    {
        Matchup matchup = OneMatchup();
        int player = FirstMultiScorerIndex(matchup);
        var legs = new[]
        {
            MarketSelection.PlayerMultiScorer(player),
            MarketSelection.TotalGoals(1.5, false),
        };

        (double pJoint, IReadOnlyList<Relation> relations, _) = JointModel.JointProbability(matchup, legs);
        Assert.True(pJoint == 0.0, $"expected exactly 0.0, got {pJoint:R}");
        Assert.Equal(RelationKind.MutuallyExclusive, Assert.Single(relations).Kind);
    }

    /// <summary>Two players each needing two goals need four between them, so the pair cannot fit
    /// inside a UNDER 3.5 ticket — a shape that has no analogue before 2+ existed.</summary>
    [Fact]
    public void Two_players_needing_two_goals_each_cannot_fit_under_three_and_a_half()
    {
        Matchup matchup = OneMatchup();
        int first = matchup.Away.Players.Count; // board index of the first HOME player
        var legs = new[]
        {
            MarketSelection.PlayerMultiScorer(first),
            MarketSelection.PlayerMultiScorer(first + 1),
            MarketSelection.TotalGoals(3.5, false),
        };

        (double pJoint, IReadOnlyList<Relation> relations, _) = JointModel.JointProbability(matchup, legs);
        Assert.True(pJoint == 0.0, $"expected exactly 0.0, got {pJoint:R}");
        Assert.Contains(relations, r => r.Kind == RelationKind.MutuallyExclusive);

        // The two 2+ legs alone are possible, and the same goals settle both.
        (double pPair, IReadOnlyList<Relation> pairRelations, _) =
            JointModel.JointProbability(matchup, new[] { legs[0], legs[1] });
        Assert.True(pPair > 0.0);
        Assert.Equal(RelationKind.ScorerOfSide, Assert.Single(pairRelations).Kind);
    }

    /// <summary>SAME FAMILY IS NOT THE SAME DRAW since team totals landed. HOME corners beside AWAY
    /// corners share the CORNER family and read two independent draws, so the joint is the product and
    /// the honest label is Independent — <c>SharedCount</c> would state a correlation the model has
    /// just measured as absent. Canon's gloss on the Independent row ("different families") is
    /// narrower than the board and is flagged for the Design Director.</summary>
    [Fact]
    public void Team_counts_on_opposite_sides_are_exactly_the_product_and_labelled_independent()
    {
        Matchup matchup = OneMatchup();
        double line = matchup.ModelConfig.TeamCornerLines[0];
        var legs = new[]
        {
            MarketSelection.TeamTotalCorners(Side.Home, line, true),
            MarketSelection.TeamTotalCorners(Side.Away, line, true),
        };

        (double pJoint, IReadOnlyList<Relation> relations, _) = JointModel.JointProbability(matchup, legs);
        double product = Marginal(matchup, legs[0]) * Marginal(matchup, legs[1]);

        Assert.False(Correlated(pJoint, product), $"joint {pJoint:R} vs product {product:R}");
        Relation relation = Assert.Single(relations);
        Assert.Equal(RelationKind.Independent, relation.Kind);
        Assert.False(relation.Correlated);

        // The SAME side still shares its draw, so the extension is narrowed, not abandoned.
        Relation shared = Assert.Single(JointModel.JointProbability(matchup, new[]
        {
            MarketSelection.TeamTotalCorners(Side.Home, line, true),
            MarketSelection.TotalCorners(matchup.ModelConfig.CornerLines[^1], false),
        }).relations);
        Assert.Equal(RelationKind.SharedCount, shared.Kind);
    }

    // =======================================================================================
    // What draws did to the two shapes canon called artefacts.
    // =======================================================================================

    /// <summary>CANON PREDICTED THIS. <c>BTTS YES + UNDER 2.5</c> was impossible only because a level
    /// score was unrepresentable: 1-1 restores it. The pair leaves the impossible set and becomes
    /// merely unlikely, and it carries an ordinary shared-scoreline label.</summary>
    [Fact]
    public void Btts_yes_with_under_two_and_a_half_is_no_longer_impossible()
    {
        Matchup matchup = OneMatchup();
        var legs = new[] { MarketSelection.BothTeamsToScore(true), MarketSelection.TotalGoals(2.5, false) };

        (double pJoint, IReadOnlyList<Relation> relations, _) = JointModel.JointProbability(matchup, legs);
        double oneOne = MatchModel.TrueProbability(matchup, MarketSelection.CorrectScore(1, 1));

        _output.WriteLine($"BTTS YES + UNDER 2.5 = {pJoint:R} (P(1-1) = {oneOne:R})");
        Assert.True(pJoint > 0.0, "draws make 1-1 the winning outcome of this pair");
        Assert.True(pJoint == oneOne, "1-1 is the ONLY outcome that wins it, so the joint IS P(1-1)");

        Relation relation = Assert.Single(relations);
        Assert.Equal(RelationKind.SharedScoreline, relation.Kind);
    }

    /// <summary>CANON PREDICTED THIS TOO. <c>UNDER 2.5 ⊂ BTTS NO</c> stopped being an implication:
    /// 1-1 is under 2.5 and both teams scored, so the containment is broken and the pair prices as an
    /// ordinary opposing shared scoreline.</summary>
    [Fact]
    public void Under_two_and_a_half_no_longer_implies_btts_no()
    {
        Matchup matchup = OneMatchup();
        var legs = new[] { MarketSelection.TotalGoals(2.5, false), MarketSelection.BothTeamsToScore(false) };

        (double pJoint, IReadOnlyList<Relation> relations, _) = JointModel.JointProbability(matchup, legs);
        double under = Marginal(matchup, legs[0]);

        Assert.True(pJoint < under - JointModel.ImplicationTolerance,
            $"UNDER 2.5 still entails BTTS NO: joint {pJoint:R} vs UNDER {under:R}");
        Relation relation = Assert.Single(relations);
        Assert.NotEqual(RelationKind.Implies, relation.Kind);
        Assert.Equal(RelationKind.SharedScoreline, relation.Kind);
    }

    // =======================================================================================
    // CANON RE-MEASURE. design/02-betting-math.md § *Pending: draws* is explicit that every
    // measured figure in that section was taken against the pre-draws model and "must be
    // re-measured once draws land". This is that re-run; the numbers land in the test output for
    // the Design Director to fold back into the doc.
    // =======================================================================================

    /// <summary>
    /// The section's measured quantities, re-taken on the merged fifteen-kind board: impossible and
    /// implication SHAPE counts, the `ρ` range at two, three and four legs, and the independent share.
    ///
    /// <para>A SHAPE is a pair of selections that behaves the same way on every matchup, which is what
    /// makes it a property of the board rather than of one set of latents. Measured over the
    /// selections every matchup in the population offers — the probability floors on correct score and
    /// 2+ admit slightly different rows per matchup, so the intersection is the honest board to count
    /// shapes on.</para>
    ///
    /// <para>Asserted only where canon makes a structural claim; the counts themselves are REPORTED,
    /// not pinned, because pinning a measurement makes the next model change look like a regression.</para>
    /// </summary>
    [Fact]
    public void Canon_figures_remeasured_on_the_fifteen_kind_board()
    {
        List<Matchup> population = Population(2, 1);
        MarketSelection[] board = CommonBoard(population);

        // ---- two legs: EXHAUSTIVE, over every matchup in the population.
        long pairs = 0, impossible = 0, implications = 0, independent = 0;
        double rho2Lo = double.MaxValue, rho2Hi = 0.0;
        var alwaysImpossible = new Dictionary<(int, int), int>();
        var alwaysImplication = new Dictionary<(int, int), int>();

        foreach (Matchup matchup in population)
        {
            double[] marginals = Marginals(matchup, board);
            for (int i = 0; i < board.Length; i++)
                for (int j = i + 1; j < board.Length; j++)
                {
                    double pJoint = JointModel.JointProbability(matchup, new[] { board[i], board[j] }).pJoint;
                    double product = marginals[i] * marginals[j];
                    pairs++;

                    if (pJoint == 0.0)
                    {
                        impossible++;
                        Bump(alwaysImpossible, (i, j));
                    }
                    else if (Math.Abs(pJoint - Math.Min(marginals[i], marginals[j])) <= JointModel.ImplicationTolerance
                             && Correlated(pJoint, product))
                    {
                        implications++;
                        Bump(alwaysImplication, (i, j));
                    }

                    if (!Correlated(pJoint, product)) independent++;
                    if (product > 0.0)
                    {
                        double rho = pJoint / product;
                        rho2Lo = Math.Min(rho2Lo, rho);
                        rho2Hi = Math.Max(rho2Hi, rho);
                    }
                }
        }

        int impossibleShapes = HoldsOnAll(alwaysImpossible, population.Count);
        int implicationShapes = HoldsOnAll(alwaysImplication, population.Count);

        // ---- three legs: EXHAUSTIVE on one matchup. A rho extreme is a property of the SHAPE, so one
        // full sweep bounds it honestly where a sample across many matchups only bounds it from below.
        // The joint-only-impossible count comes free from the same pass: the model's own ticket-level
        // exclusion label IS "zero with no impossible sub-pair", so no sub-pairs are re-priced.
        Matchup one = population[0];
        double[] oneMarginals = Marginals(one, board);
        double rho3Lo = double.MaxValue, rho3Hi = 0.0;
        long triples = 0, triplesZero = 0, jointOnlyZero = 0;

        for (int i = 0; i < board.Length; i++)
            for (int j = i + 1; j < board.Length; j++)
                for (int k = j + 1; k < board.Length; k++)
                {
                    (double pJoint, IReadOnlyList<Relation> relations, _) =
                        JointModel.JointProbability(one, new[] { board[i], board[j], board[k] });
                    triples++;
                    if (pJoint == 0.0)
                    {
                        triplesZero++;
                        foreach (Relation r in relations)
                            if (r.Kind == RelationKind.MutuallyExclusive && r.Legs.Count == 3) jointOnlyZero++;
                    }

                    double product = oneMarginals[i] * oneMarginals[j] * oneMarginals[k];
                    if (product <= 0.0) continue;
                    double rho = pJoint / product;
                    rho3Lo = Math.Min(rho3Lo, rho);
                    rho3Hi = Math.Max(rho3Hi, rho);
                }

        // ---- four legs: a deterministic sample on the same matchup. C(board, 4) is over a million,
        // so this is a LOWER BOUND on the range and is reported as one.
        var sampler = new Pcg32(0xCA11, 0x9);
        double rho4Lo = double.MaxValue, rho4Hi = 0.0;
        long quads = 0;
        for (int t = 0; t < 60_000; t++)
        {
            int[] pick = Draw(sampler, board.Length, 4);
            var legs = new MarketSelection[4];
            double product = 1.0;
            for (int i = 0; i < 4; i++) { legs[i] = board[pick[i]]; product *= oneMarginals[pick[i]]; }
            if (product <= 0.0) continue;

            double rho = JointModel.JointProbability(one, legs).pJoint / product;
            rho4Lo = Math.Min(rho4Lo, rho);
            rho4Hi = Math.Max(rho4Hi, rho);
            quads++;
        }

        _output.WriteLine("== canon re-measure, fifteen-kind board WITH draws (design/02 § Pending: draws) ==");
        _output.WriteLine($"board: {board.Length} selections offered on all {population.Count} matchups "
            + $"(was 36 pre-F_0.5.0); {pairs:N0} pair evaluations, {triples:N0} triples on one matchup");
        _output.WriteLine($"impossible PAIR shapes:   {impossibleShapes,6}      [canon, pre-draws: 22]");
        _output.WriteLine($"implication PAIR shapes:  {implicationShapes,6}      [canon, pre-draws: not counted]");
        _output.WriteLine($"impossible share:  {100.0 * impossible / pairs,6:0.00}%     [canon, pre-draws: 3.49%]");
        _output.WriteLine($"implication share: {100.0 * implications / pairs,6:0.00}%     [canon, pre-draws: 3.49%]");
        _output.WriteLine($"independent share: {100.0 * independent / pairs,6:0.00}%     [canon, pre-draws: 51.4%]");
        _output.WriteLine($"rho, 2 legs (exhaustive):  [{rho2Lo:0.000}, {rho2Hi:0.000}]   [canon, pre-draws: [0, 3.11]]");
        _output.WriteLine($"rho, 3 legs (exhaustive, 1 matchup): [{rho3Lo:0.000}, {rho3Hi:0.000}]   "
            + "[canon, pre-draws: [0, 11.88]]");
        _output.WriteLine($"rho, 4 legs (sample of {quads:N0}, LOWER BOUND): [{rho4Lo:0.000}, {rho4Hi:0.000}]   "
            + "[canon, pre-draws: [0, 14.82]]");
        _output.WriteLine($"impossible TRIPLES on one matchup: {triplesZero:N0}, of which {jointOnlyZero:N0} "
            + "reach zero with no impossible sub-pair   [canon, pre-draws: 57 shapes]");

        // The structural claims canon makes, which survive the re-measure or the doc is wrong.
        Assert.True(impossibleShapes > 0, "impossible pairs are what the one-leg-per-match guard existed for");
        Assert.True(implicationShapes > 0, "the board still carries logical implications");
        Assert.True(jointOnlyZero > 0, "a ticket must still be able to be impossible with no impossible pair");
        Assert.True(rho2Lo == 0.0, "rho reaches 0 exactly, on the impossible pairs");
        Assert.True(rho2Hi > 1.0, "rho must exceed 1 somewhere or nothing is reinforcing");
    }

    private static double[] Marginals(Matchup matchup, MarketSelection[] board)
    {
        var marginals = new double[board.Length];
        for (int i = 0; i < board.Length; i++) marginals[i] = Marginal(matchup, board[i]);
        return marginals;
    }

    private static void Bump(Dictionary<(int, int), int> tally, (int, int) key)
    {
        tally.TryGetValue(key, out int seen);
        tally[key] = seen + 1;
    }

    private static int HoldsOnAll(Dictionary<(int, int), int> tally, int matchups)
    {
        int all = 0;
        foreach (KeyValuePair<(int, int), int> entry in tally) if (entry.Value == matchups) all++;
        return all;
    }

    /// <summary>The selections every matchup in the population offers. The correct-score and 2+ boards
    /// are trimmed per matchup by the probability floor, so a shape count taken over the union would
    /// be counting rows that do not exist on most matchups.</summary>
    private static MarketSelection[] CommonBoard(IReadOnlyList<Matchup> population)
    {
        var counts = new Dictionary<MarketSelection, int>();
        foreach (Matchup matchup in population)
            foreach (MarketSelection selection in Board(matchup))
            {
                counts.TryGetValue(selection, out int seen);
                counts[selection] = seen + 1;
            }

        // Board order of the first matchup, so the sweep order is the board's own.
        var common = new List<MarketSelection>();
        foreach (MarketSelection selection in Board(population[0]))
            if (counts[selection] == population.Count) common.Add(selection);
        return common.ToArray();
    }

    /// <summary>A sorted set of <paramref name="k"/> distinct board indices.</summary>
    private static int[] Draw(Pcg32 rng, int size, int k)
    {
        var pick = new int[k];
        for (int i = 0; i < k; i++)
        {
            int candidate;
            bool repeat;
            do
            {
                candidate = rng.NextInt(0, size);
                repeat = false;
                for (int j = 0; j < i; j++) if (pick[j] == candidate) repeat = true;
            } while (repeat);
            pick[i] = candidate;
        }
        Array.Sort(pick);
        return pick;
    }

    /// <summary>The board's own index of the first 2+ row, so the multi-scorer shapes are exercised on
    /// a player the board actually offers rather than one the probability floor removed.</summary>
    private static int FirstMultiScorerIndex(Matchup matchup)
    {
        foreach (MarketOffer offer in matchup.Markets)
            if (offer.Selection.Kind == MarketKind.PlayerMultiScorer) return offer.Selection.PlayerIndex;
        throw new InvalidOperationException("the board offers no 2+ rows");
    }
}
