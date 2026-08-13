using System;
using System.Collections.Generic;
using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// Phase 2 of F_0.6.0 — ticket pricing and the guard. <c>Gate1_*</c> is the equivalence invariant,
/// <c>Gate2_*</c> the validity rejection and the hand-computed joint, <c>Gate3_*</c> the Profit Boost.
/// Everything else pins a named clause of <c>design/02-betting-math.md</c> § *Same-game tickets*.
/// </summary>
public class SameMatchPricingTests
{
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public SameMatchPricingTests(Xunit.Abstractions.ITestOutputHelper output) => _output = output;

    /// <summary>A bank deep enough that a long placement sweep never trips the stake cap, and a
    /// per-round ticket cap high enough to place the whole sweep inside one betting phase. Neither
    /// dial touches pricing.</summary>
    private static RunConfig SweepConfig() => new RunConfig
    {
        StartingBank = 1_000_000_000,
        MaxTicketsPerRound = 100_000,
    };

    private static List<Matchup> Population(int seeds, int rounds)
    {
        var config = new RunConfig();
        var matchups = new List<Matchup>(seeds * rounds * config.MatchupsPerSlate);
        for (int seed = 0; seed < seeds; seed++)
        {
            var hub = new RngHub($"sgp-price-{seed}");
            for (int round = 0; round < rounds; round++)
                matchups.AddRange(SlateGenerator.Generate(round, hub, config).Matchups);
        }
        return matchups;
    }

    private static MarketSelection[] Board(Matchup matchup)
    {
        var board = new MarketSelection[matchup.Markets.Count];
        for (int i = 0; i < board.Length; i++) board[i] = matchup.Markets[i].Selection;
        return board;
    }

    private static Leg LegFor(Matchup matchup, MarketSelection selection)
        => new Leg(matchup, selection, matchup.Odds(selection));

    /// <summary>The pre-F_0.6.0 payout expression, transcribed. The invariant is that the engine still
    /// produces exactly this for any ticket with at most one leg per matchup.</summary>
    private static double OldPayout(Ticket ticket)
    {
        var odds = new List<double>();
        foreach (Leg leg in ticket.Legs) if (!leg.IsVoided) odds.Add(leg.OfferedOdds);
        return ticket.Stake * OddsMath.ParlayDecimal(odds) * ticket.PayoutMultiplier;
    }

    private static double OldOffered(Ticket ticket)
    {
        var odds = new double[ticket.Legs.Count];
        for (int i = 0; i < odds.Length; i++) odds[i] = ticket.Legs[i].OfferedOdds;
        return OddsMath.ParlayDecimal(odds);
    }

    private static double OldVig(Ticket ticket)
    {
        var probs = new double[ticket.Legs.Count];
        for (int i = 0; i < probs.Length; i++) probs[i] = ticket.Legs[i].TrueProb;
        return OddsMath.VigPaid(ticket.Stake, OldOffered(ticket), OddsMath.FairDecimal(OddsMath.ParlayProb(probs)));
    }

    /// <summary>Deterministic, self-contained stepping so the swept population is a property of the
    /// test rather than of a shared RNG's call order.</summary>
    private static int Step(int state) => (state * 1103515245 + 12345) & 0x7FFFFFFF;

    // =======================================================================================
    // EXIT GATE 1 — the equivalence invariant. This is the whole safety story.
    // =======================================================================================

    /// <summary>
    /// A ticket with at most one leg per matchup must price, pay and account for vig BIT-IDENTICALLY
    /// to before F_0.6.0. Not "within a tolerance" — <c>==</c>. <c>GoldenSeedTests</c> and the entire
    /// gate baseline sit downstream of these bits.
    ///
    /// <para>The property is enforced structurally: such a ticket never enters the joint path at all,
    /// so this test is a check on that routing rather than on an agreement between two formulas.</para>
    /// </summary>
    [Fact]
    public void Gate1_distinct_matchup_tickets_price_and_pay_bit_identically_to_the_old_path()
    {
        long tickets = 0;
        long legs = 0;

        for (int seed = 0; seed < 40; seed++)
        {
            var run = new Run($"equiv-{seed}", SweepConfig());
            int slateSize = run.CurrentSlate.Matchups.Count;
            int state = Step(seed + 1);

            for (int shape = 0; shape < 40; shape++)
            {
                int legCount = 1 + (shape % 4);
                var picks = new List<Pick>(legCount);
                var used = new HashSet<int>();
                for (int i = 0; i < legCount; i++)
                {
                    state = Step(state);
                    int matchupIndex = state % slateSize;
                    // Distinct matchups, by construction — this sweep is about the OLD path only.
                    for (int probe = 0; probe < slateSize && !used.Add(matchupIndex); probe++)
                    {
                        matchupIndex = (matchupIndex + 1) % slateSize;
                        if (used.Add(matchupIndex)) break;
                    }
                    Matchup matchup = run.CurrentSlate.Matchups[matchupIndex];
                    MarketSelection[] board = Board(matchup);
                    state = Step(state);
                    picks.Add(new Pick(matchupIndex, board[state % board.Length]));
                }
                if (picks.Count != used.Count) continue;

                Ticket ticket = run.PlaceTicket(picks, 10);
                tickets++;
                legs += ticket.Legs.Count;

                Assert.Null(ticket.SameMatch);
                Assert.True(ticket.PotentialPayout == OldPayout(ticket),
                    $"payout drifted: {ticket.PotentialPayout:R} vs {OldPayout(ticket):R}");
                Assert.True(ticket.LockedPrice == OldOffered(ticket),
                    $"price drifted: {ticket.LockedPrice:R} vs {OldOffered(ticket):R}");
                Assert.True(ticket.VigPaid == OldVig(ticket),
                    $"vig drifted: {ticket.VigPaid:R} vs {OldVig(ticket):R}");
            }
        }

        _output.WriteLine($"{tickets} distinct-matchup tickets ({legs} legs) all exact.");
        Assert.True(tickets > 1000, $"sweep too thin to mean anything: {tickets} tickets");
    }

    /// <summary>The invariant covers voiding too: a voided leg must still drop out of an ordinary
    /// ticket's payout product, which it only does because that ticket keeps reading its ACTIVE legs
    /// rather than a price frozen at placement.</summary>
    [Fact]
    public void Gate1_voiding_a_leg_on_an_ordinary_ticket_still_drops_it_from_the_product()
    {
        // Same seed and pick shape as ItemTests' mulligan case: leg 0 dies first on this slate.
        var run = new Run("GOLDEN-W2", SweepConfig());
        run.GrantConsumable(RelicCatalog.Consumables.First(c => c.Id == "mulligan_slip"));

        Ticket ticket = run.PlaceTicket(new[] { new Pick(1, Side.Home), new Pick(0, Side.Away) }, 50);
        Assert.Null(ticket.SameMatch);
        double beforeVoid = ticket.PotentialPayout;

        run.LockRound();
        SweatSession sweat = run.Sweats[0];
        while (!sweat.HasPendingLoss)
            Assert.True(sweat.MoveNext(out _), "expected a dead leg on this seed");
        run.PlayMulliganSlip(sweat);

        Assert.True(ticket.Legs[0].IsVoided);
        Assert.True(ticket.PotentialPayout == OldPayout(ticket));
        Assert.True(ticket.PotentialPayout == 50 * ticket.Legs[1].OfferedOdds * ticket.PayoutMultiplier,
            "a voided leg must leave the payout product");
        Assert.NotEqual(beforeVoid, ticket.PotentialPayout);
    }

    // =======================================================================================
    // EXIT GATE 2 — the guard, and the joint price itself.
    // =======================================================================================

    /// <summary>The one-pick-per-matchup guard is REPLACED, not deleted: an impossible combination
    /// still refuses, now because it has no price rather than because it shares a matchup.</summary>
    [Fact]
    public void Gate2_an_impossible_same_match_pair_is_refused_by_PlaceTicket()
    {
        var run = new Run("sgp-impossible", SweepConfig());

        ArgumentException thrown = Assert.Throws<ArgumentException>(() => run.PlaceTicket(
            new[] { new Pick(0, Side.Home), new Pick(0, Side.Away) }, 10));
        Assert.Contains("zero", thrown.Message);

        // A disjoint band on the same draw is the same verdict from a different cause.
        Assert.Throws<ArgumentException>(() => run.PlaceTicket(new[]
        {
            new Pick(0, MarketSelection.TotalGoals(2.5, true)),
            new Pick(0, MarketSelection.TotalGoals(1.5, false)),
        }, 10));

        Assert.Empty(run.Tickets);
        Assert.Equal(1_000_000_000, run.Bank);
    }

    /// <summary>Recon §6.2's 57 impossible triples reach zero only jointly — every sub-pair is
    /// perfectly possible, so a purely pairwise guard would sell them. The shape is discovered from
    /// the live board rather than hard-coded, because which triples qualify is a property of the
    /// model's outcome partition and moves when that partition does (Lane 1's draws).</summary>
    [Fact]
    public void Gate2_a_ticket_impossible_only_as_a_whole_is_refused_too()
    {
        var run = new Run("sgp-triple", SweepConfig());
        Matchup matchup = run.CurrentSlate.Matchups[0];
        MarketSelection[] board = Board(matchup);

        MarketSelection[]? found = null;
        for (int i = 0; i < board.Length && found == null; i++)
            for (int j = i + 1; j < board.Length && found == null; j++)
            {
                if (JointModel.JointProbability(matchup, new[] { board[i], board[j] }).pJoint == 0.0) continue;
                for (int k = j + 1; k < board.Length; k++)
                {
                    if (JointModel.JointProbability(matchup, new[] { board[i], board[k] }).pJoint == 0.0) continue;
                    if (JointModel.JointProbability(matchup, new[] { board[j], board[k] }).pJoint == 0.0) continue;
                    if (JointModel.JointProbability(matchup, new[] { board[i], board[j], board[k] }).pJoint != 0.0)
                        continue;
                    found = new[] { board[i], board[j], board[k] };
                    break;
                }
            }

        Assert.NotNull(found);
        _output.WriteLine("impossible only as a whole: "
            + string.Join(" + ", found!.Select(s => MatchModel.DisplayLabel(matchup, s))));

        Assert.Throws<ArgumentException>(() => run.PlaceTicket(
            found!.Select(s => new Pick(0, s)).ToArray(), 10));
    }

    /// <summary>A possible same-match ticket prices at its exact joint. The joint is hand-computed
    /// here without JointModel: UNDER 1.5 strictly implies UNDER 2.5, so the pair's probability is the
    /// engine's own marginal for UNDER 1.5 and nothing else.</summary>
    [Fact]
    public void Gate2_a_possible_same_match_ticket_prices_at_the_hand_computed_joint()
    {
        var config = SweepConfig();
        var run = new Run("sgp-implies", config);
        Matchup matchup = run.CurrentSlate.Matchups[0];

        var under15 = MarketSelection.TotalGoals(1.5, false);
        var under25 = MarketSelection.TotalGoals(2.5, false);

        double expectedJoint = matchup.TrueProb(under15); // hand-computed: the implication's own marginal
        double expectedPrice = 1.0 / (expectedJoint * config.SgpMargin * Math.Pow(1.0 + config.Overround, 2));

        Ticket ticket = run.PlaceTicket(new[] { new Pick(0, under15), new Pick(0, under25) }, 10);

        Assert.NotNull(ticket.SameMatch);
        Assert.Equal(expectedJoint, ticket.SameMatch!.PTicket, 15);
        Assert.Equal(expectedPrice, ticket.LockedPrice, 12);
        Assert.Equal(10 * expectedPrice, ticket.PotentialPayout, 10);

        // The price actually MOVED: a correlated ticket must not land on the board's product.
        double product = matchup.Odds(under15) * matchup.Odds(under25);
        Assert.True(ticket.LockedPrice < product * 0.9,
            $"an implication must price far under the naive product ({ticket.LockedPrice:0.000} vs {product:0.000})");

        // design/02: o_sgp / PROD o_i = 1 / (kappa * rho).
        double rho = expectedJoint / (matchup.TrueProb(under15) * matchup.TrueProb(under25));
        Assert.Equal(1.0 / (config.SgpMargin * rho), ticket.LockedPrice / product, 9);
    }

    /// <summary>An independent same-match combination (different draws) must price where the board's
    /// product already prices it — the design's stated central property at κ = 1: the price departs
    /// from the product only where correlation actually exists.</summary>
    [Fact]
    public void An_independent_same_match_pair_prices_at_the_boards_own_product()
    {
        var run = new Run("sgp-independent", SweepConfig());
        Matchup matchup = run.CurrentSlate.Matchups[0];

        var goals = MarketSelection.TotalGoals(2.5, true);
        var corners = MarketSelection.TotalCorners(9.5, true);

        Ticket ticket = run.PlaceTicket(new[] { new Pick(0, goals), new Pick(0, corners) }, 10);

        Assert.NotNull(ticket.SameMatch);
        double product = matchup.Odds(goals) * matchup.Odds(corners);
        Assert.Equal(product, ticket.LockedPrice, 10);
    }

    // =======================================================================================
    // EXIT GATE 3 — the Profit Boost keeps its exact value.
    // =======================================================================================

    /// <summary>A leg-targeted Profit Boost multiplies the TICKET price by exactly its factor, so the
    /// relic is worth on a same-match ticket precisely what it is worth on any other.</summary>
    [Fact]
    public void Gate3_a_profit_boost_multiplies_a_same_match_tickets_price_by_exactly_its_factor()
    {
        var plainRun = new Run("sgp-boost", SweepConfig());
        var boostedRun = new Run("sgp-boost", SweepConfig());
        boostedRun.GrantConsumable(RelicCatalog.Consumables.First(c => c.Id == "profit_boost"));

        Pick[] picks =
        {
            new Pick(0, MarketSelection.TotalGoals(1.5, true)),
            new Pick(0, MarketSelection.BothTeamsToScore(true)),
        };

        Ticket plain = plainRun.PlaceTicket(picks, 10);
        Ticket boosted = boostedRun.PlaceTicket(picks, 10, profitBoostLeg: 0);

        Assert.NotNull(plain.SameMatch);
        Assert.NotNull(boosted.SameMatch);
        Assert.True(boosted.LockedPrice == plain.LockedPrice * RelicCatalog.ProfitBoostMult,
            $"boosted price {boosted.LockedPrice:R} != {plain.LockedPrice:R} x {RelicCatalog.ProfitBoostMult:R}");
        Assert.True(boosted.PotentialPayout == plain.PotentialPayout * RelicCatalog.ProfitBoostMult);
        Assert.Empty(boostedRun.OwnedConsumables);
    }

    // =======================================================================================
    // The output contract: principal, and the canon names.
    // =======================================================================================

    /// <summary>The slip states one relation, so the model nominates it. Across the shipped board the
    /// nomination must always be a labelable kind carrying the largest |ln rho| on the ticket.</summary>
    [Fact]
    public void The_principal_relation_is_the_strongest_labelable_one_on_the_ticket()
    {
        long tickets = 0;
        long withPrincipal = 0;

        foreach (Matchup matchup in Population(6, 1))
        {
            MarketSelection[] board = Board(matchup);
            for (int i = 0; i < board.Length; i++)
                for (int j = i + 1; j < board.Length; j++)
                {
                    (double pJoint, IReadOnlyList<Relation> relations, Relation? principal) =
                        JointModel.JointProbability(matchup, new[] { board[i], board[j] });
                    tickets++;

                    if (principal == null)
                    {
                        foreach (Relation relation in relations)
                            Assert.True(relation.Kind == RelationKind.Independent
                                || relation.Kind == RelationKind.MutuallyExclusive,
                                $"{relation.Kind} was statable but no principal was nominated");
                        continue;
                    }

                    withPrincipal++;
                    Assert.NotEqual(RelationKind.Independent, principal.Value.Kind);
                    Assert.NotEqual(RelationKind.MutuallyExclusive, principal.Value.Kind);
                    foreach (Relation relation in relations)
                        if (relation.Kind != RelationKind.Independent
                            && relation.Kind != RelationKind.MutuallyExclusive)
                            Assert.True(principal.Value.Strength >= relation.Strength,
                                "a stronger labelable relation was passed over");
                    Assert.True(pJoint > 0.0);
                }
        }

        _output.WriteLine($"{withPrincipal} of {tickets} board pairs nominate a principal relation.");
        Assert.True(withPrincipal > 0);
    }

    /// <summary>Ties break by precedence Implies &gt; ScorerOfSide &gt; SharedScoreline &gt;
    /// SharedCount. Equal-strength relations do not occur on the shipped board, so the rule is pinned
    /// against constructed relations rather than left to chance.</summary>
    [Fact]
    public void Equal_strength_relations_break_by_precedence()
    {
        var legs = new[] { 0, 1 };
        Relation Make(RelationKind kind) =>
            new Relation(kind, legs, RelationSign.Reinforcing, SelectionFamily.Goal, null, 0.25, true);

        Assert.Equal(RelationKind.Implies, JointModel.SelectPrincipal(new[]
        {
            Make(RelationKind.SharedCount), Make(RelationKind.SharedScoreline),
            Make(RelationKind.ScorerOfSide), Make(RelationKind.Implies),
        })!.Value.Kind);

        Assert.Equal(RelationKind.ScorerOfSide, JointModel.SelectPrincipal(new[]
        {
            Make(RelationKind.SharedCount), Make(RelationKind.SharedScoreline), Make(RelationKind.ScorerOfSide),
        })!.Value.Kind);

        Assert.Equal(RelationKind.SharedScoreline, JointModel.SelectPrincipal(new[]
        {
            Make(RelationKind.SharedCount), Make(RelationKind.SharedScoreline),
        })!.Value.Kind);

        // Strength outranks precedence: the relation doing the most work on the price wins.
        Assert.Equal(RelationKind.SharedCount, JointModel.SelectPrincipal(new[]
        {
            Make(RelationKind.Implies),
            new Relation(RelationKind.SharedCount, legs, RelationSign.Opposing, SelectionFamily.Corner,
                null, 0.9, true),
        })!.Value.Kind);

        // Neither ineligible kind can be nominated, whatever its strength.
        Assert.Null(JointModel.SelectPrincipal(new[]
        {
            new Relation(RelationKind.Independent, legs, RelationSign.None, null, null, 3.0, false),
            new Relation(RelationKind.MutuallyExclusive, legs, RelationSign.None, null, null, 3.0, true),
        }));
    }

    /// <summary>A ticket spans matchups, so its principal is ranked across all of them — and every
    /// relation it carries indexes the TICKET's legs, not some group's private list.</summary>
    [Fact]
    public void Relations_are_re_keyed_onto_the_ticket_and_the_principal_ranks_across_matchups()
    {
        List<Matchup> population = Population(1, 1);
        Matchup weak = population[0];
        Matchup strong = population[1];

        var legs = new List<Leg>
        {
            LegFor(weak, MarketSelection.TotalCorners(8.5, true)),      // matchup A, leg 0  } a band
            LegFor(strong, MarketSelection.TotalGoals(1.5, false)),     // matchup B, leg 1  } an
            LegFor(weak, MarketSelection.TotalCorners(10.5, false)),    // matchup A, leg 2  } implication
            LegFor(strong, MarketSelection.TotalGoals(2.5, false)),     // matchup B, leg 3
        };

        SameMatchPrice price = SameMatchModel.Price(legs, 0.05, 1.0);

        Assert.Equal(2, price.Relations.Count);
        foreach (Relation relation in price.Relations)
            foreach (int leg in relation.Legs)
                Assert.InRange(leg, 0, legs.Count - 1);

        // Group A's pair is (0, 2) and group B's is (1, 3) — never (0, 1) twice.
        Assert.Contains(price.Relations, r => r.Legs.Contains(0) && r.Legs.Contains(2));
        Assert.Contains(price.Relations, r => r.Legs.Contains(1) && r.Legs.Contains(3));

        // The nomination ranks across BOTH matchups, not within one.
        Assert.NotNull(price.Principal);
        double strongest = price.Relations.Max(r => r.Strength);
        Assert.Equal(strongest, price.Principal!.Value.Strength);
        Assert.Equal(RelationKind.Implies, price.Principal!.Value.Kind);
        Assert.Contains(price.Principal!.Value.Legs, leg => leg == 1 || leg == 3);

        // p_ticket factorizes across matchups, exactly.
        double pA = JointModel.JointProbability(weak,
            new[] { legs[0].Selection, legs[2].Selection }).pJoint;
        double pB = JointModel.JointProbability(strong,
            new[] { legs[1].Selection, legs[3].Selection }).pJoint;
        Assert.True(price.PTicket == pA * pB);
    }

    /// <summary>The canon names are baked in, so step 5 inherits no placeholder. They are NAMES: the
    /// mark's name is never emitted beside a mark, a price or a relation, and nothing in the engine
    /// composes an English sentence out of either.</summary>
    [Fact]
    public void The_canon_names_are_constants()
    {
        Assert.Equal("SAME MATCH", SameMatchModel.Instrument);
        Assert.Equal("THE HOUSE'S LINE", SameMatchModel.Mark);
    }

    // =======================================================================================
    // The margin dial, and the design's central EV property.
    // =======================================================================================

    /// <summary>κ is a real dial with default 1.0, and it moves the price the way the formula says.</summary>
    [Fact]
    public void The_same_match_margin_dial_is_real_and_defaults_to_one()
    {
        Assert.Equal(1.0, new RunConfig().SgpMargin);

        var config = SweepConfig();
        config.SgpMargin = 2.0;
        var dialled = new Run("sgp-kappa", config);
        var plain = new Run("sgp-kappa", SweepConfig());

        Pick[] picks =
        {
            new Pick(0, MarketSelection.TotalGoals(1.5, true)),
            new Pick(0, MarketSelection.BothTeamsToScore(true)),
        };

        Ticket a = plain.PlaceTicket(picks, 10);
        Ticket b = dialled.PlaceTicket(picks, 10);
        Assert.Equal(a.LockedPrice / 2.0, b.LockedPrice, 12);
    }

    /// <summary>design/02's central property: at κ = 1 a same-match ticket's EV per unit stake is
    /// (1 + Ω)^−n, identical to an independent n-leg parlay. Correlation changes the odds shown and
    /// never the house edge, so no arbitrage exists between the two products.</summary>
    [Fact]
    public void At_kappa_one_a_same_match_ticket_is_EV_identical_to_an_independent_parlay()
    {
        var config = new RunConfig();
        double worst = 0.0;
        long checks = 0;

        foreach (Matchup matchup in Population(4, 1))
        {
            MarketSelection[] board = Board(matchup);
            for (int i = 0; i < board.Length; i++)
                for (int j = i + 1; j < board.Length; j++)
                {
                    var legs = new[] { LegFor(matchup, board[i]), LegFor(matchup, board[j]) };
                    SameMatchPrice price = SameMatchModel.Price(legs, config.Overround, 1.0);
                    if (price.Impossible) continue;

                    double ev = price.PTicket * price.Price - 1.0;
                    double expected = Math.Pow(1.0 + config.Overround, -2) - 1.0;
                    worst = Math.Max(worst, Math.Abs(ev - expected));
                    checks++;
                }
        }

        _output.WriteLine($"{checks} two-leg same-match tickets, worst |EV - (1+O)^-n + 1| = {worst:E3}");
        Assert.True(worst < 1e-12, $"EV drifted from the independent parlay by {worst:E3}");
    }

    // =======================================================================================
    // The no-label fallback — instrumented, and silent on this board.
    // =======================================================================================

    /// <summary>
    /// The fallback must never fire on the shipped board — Phase 1 verified the vocabulary is total —
    /// and it must be COUNTED rather than logged, because a silent fallback is a money leak worth up
    /// to +274% EV on an implication pair.
    ///
    /// <para>The sweep prices every two-leg and a stride of three-leg same-match tickets and asserts
    /// the counter never moves, and that no relation is correlated-yet-unlabelled (the only shape that
    /// could move it).</para>
    /// </summary>
    [Fact]
    public void The_no_label_fallback_is_observable_and_never_fires_on_the_shipped_board()
    {
        SameMatchModel.ResetNoLabelFallbacks();
        Assert.Equal(0, SameMatchModel.NoLabelFallbacks);

        var config = new RunConfig();
        long priced = 0;
        long impossible = 0;

        foreach (Matchup matchup in Population(6, 1))
        {
            MarketSelection[] board = Board(matchup);
            for (int i = 0; i < board.Length; i++)
                for (int j = i + 1; j < board.Length; j++)
                {
                    var legs = new List<Leg> { LegFor(matchup, board[i]), LegFor(matchup, board[j]) };
                    if ((i + j) % 5 == 0)
                        legs.Add(LegFor(matchup, board[(i + j + 7) % board.Length]));

                    SameMatchPrice price = SameMatchModel.Price(legs, config.Overround, config.SgpMargin);
                    if (price.Impossible) { impossible++; continue; }

                    Assert.False(price.NaiveFallback);
                    foreach (Relation relation in price.Relations)
                        Assert.False(relation.Kind == RelationKind.Independent && relation.Correlated,
                            "correlation the vocabulary cannot label — the fallback's trigger");
                    priced++;
                }
        }

        _output.WriteLine($"{priced} same-match tickets priced on the exact joint, {impossible} refused; "
            + $"fallbacks = {SameMatchModel.NoLabelFallbacks}");
        Assert.Equal(0, SameMatchModel.NoLabelFallbacks);
        Assert.True(priced > 10_000);
    }

    /// <summary>
    /// Every same-match price the shipped board can produce must be decimal odds above 1.0. Below
    /// that the ticket is unsellable and <c>OddsMath</c> refuses it outright — a hard throw out of
    /// <c>PlaceTicket</c>, not a soft mispricing. The board only guarantees p &lt; 1/(1+Ω) per leg,
    /// which does NOT by itself bound p_ticket × (1+Ω)^n below 1, so this is checked rather than
    /// assumed.
    ///
    /// <para>The empirical half sweeps every two-leg shape and finds daylight. The structural half
    /// does NOT: p_ticket never exceeds the smallest marginal on the ticket, so a ticket every one of
    /// whose legs sits on one market of marginal p prices at 1/(p × (1+Ω)^n) — and the board's dearest
    /// marginal is above (1+Ω)^−MaxLegs, so four repeats of it fall under evens. That ticket is
    /// unsellable however it is handled; what this pins is that it is refused with its reason rather
    /// than throwing out of OddsMath's arithmetic. Whether repeat legs should be refused outright is
    /// a design question this test deliberately does not answer.</para>
    /// </summary>
    [Fact]
    public void Every_same_match_price_the_board_can_produce_is_above_evens()
    {
        var config = new RunConfig();
        double tightest = double.MaxValue;
        string tightestAt = "none";
        double dearest = 0.0;
        string dearestAt = "none";
        double floor = Math.Pow(1.0 + config.Overround, -config.MaxLegs);

        foreach (Matchup matchup in Population(8, 1))
        {
            MarketSelection[] board = Board(matchup);
            for (int i = 0; i < board.Length; i++)
            {
                double marginal = JointModel.JointProbability(matchup, new[] { board[i] }).pJoint;
                if (marginal > dearest)
                {
                    dearest = marginal;
                    dearestAt = MatchModel.DisplayLabel(matchup, board[i]);
                }

                for (int j = i + 1; j < board.Length; j++)
                {
                    var legs = new[] { LegFor(matchup, board[i]), LegFor(matchup, board[j]) };
                    SameMatchPrice price = SameMatchModel.Price(legs, config.Overround, config.SgpMargin);
                    if (price.Impossible) continue;
                    if (price.Price < tightest)
                    {
                        tightest = price.Price;
                        tightestAt = $"{MatchModel.DisplayLabel(matchup, board[i])} + "
                            + $"{MatchModel.DisplayLabel(matchup, board[j])} @ {price.Price:0.0000}";
                    }
                }
            }
        }

        _output.WriteLine($"tightest two-leg same-match price: {tightestAt}");
        _output.WriteLine($"dearest single marginal: {dearest:0.0000} ({dearestAt}); "
            + $"headroom floor at {config.MaxLegs} legs = {floor:0.0000} — "
            + (dearest < floor ? "no sub-evens ticket is constructible"
                               : "REPEAT LEGS CAN REACH SUB-EVENS, and must refuse legibly"));

        Assert.True(tightest > 1.0,
            $"a same-match ticket priced at or below evens ({tightest:0.0000}) — unsellable: {tightestAt}");
    }

    /// <summary>The reachable sub-evens case, pinned: four repeats of one high-probability market.
    /// P(A and A) = P(A), so the joint barely moves while (1+Ω)^n keeps climbing, and the price falls
    /// through 1.0. It must refuse with its reason — not throw out of OddsMath's arithmetic — and it
    /// must cost the player nothing.</summary>
    [Fact]
    public void A_repeat_leg_ticket_that_prices_below_evens_refuses_with_its_reason()
    {
        var config = SweepConfig();
        var run = new Run("sgp-repeat", config);

        // The dearest market on this slate: the one with the least room under the margin.
        int bestMatchup = 0;
        MarketSelection dearest = default;
        double dearestP = 0.0;
        for (int m = 0; m < run.CurrentSlate.Matchups.Count; m++)
            foreach (MarketSelection selection in Board(run.CurrentSlate.Matchups[m]))
            {
                double p = run.CurrentSlate.Matchups[m].TrueProb(selection);
                if (p <= dearestP) continue;
                dearestP = p;
                dearest = selection;
                bestMatchup = m;
            }

        double price = 1.0 / (dearestP * config.SgpMargin * Math.Pow(1.0 + config.Overround, 4));
        _output.WriteLine($"4 repeats of {MatchModel.DisplayLabel(run.CurrentSlate.Matchups[bestMatchup], dearest)} "
            + $"(p = {dearestP:0.0000}) price at {price:0.0000}");

        var picks = new[]
        {
            new Pick(bestMatchup, dearest), new Pick(bestMatchup, dearest),
            new Pick(bestMatchup, dearest), new Pick(bestMatchup, dearest),
        };

        if (price > 1.0)
        {
            // Still sellable on this slate: assert it prices coherently rather than silently skipping.
            Ticket ticket = run.PlaceTicket(picks, 10);
            Assert.Equal(price, ticket.LockedPrice, 10);
            return;
        }

        ArgumentException thrown = Assert.Throws<ArgumentException>(() => run.PlaceTicket(picks, 10));
        Assert.Contains("cannot be offered", thrown.Message);
        Assert.Empty(run.Tickets);
    }
}
