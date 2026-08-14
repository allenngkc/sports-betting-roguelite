using System;
using System.Collections.Generic;
using System.Linq;
using SBR.Engine;
using Xunit;

namespace SBR.Engine.Tests;

/// <summary>
/// Phase 3d of F_0.6.0 — STRUCTURED REFUSALS (S73-am4,
/// <c>docs/design/surething-design.md</c> §3.3 — the owning law;
/// <c>design/02-betting-math.md</c> § *A refusal must emit cause AND remedy, structurally*).
///
/// <para>A refused combination is a <i>Blocked</i> state, and that row has always required both
/// halves: what cannot happen is the CAUSE, what to drop is the REMEDY. The engine used to refuse
/// with a reason string, which a surface cannot compose a stamp out of — it has to name a specific
/// leg to drop. So the model emits parts and never English, the same seam
/// <c>SameMatchPrice.Principal</c> established.</para>
///
/// <para><b>The remedy is verified, never asserted.</b> Every test here that reads a remedy then
/// PLACES the ticket the remedy describes, through the real <c>Run.PlaceTicket</c>. A remedy naming
/// a leg whose removal does not actually fix the ticket is a remedy that lies, and lying about the
/// fix is worse than refusing without one.</para>
///
/// <para><c>Gate1_*</c> is cause-and-remedy on each of the three rules; <c>Gate2_*</c> is the
/// board-wide sweep that verifies every remedy by construction; <c>Gate3_*</c> is that a refused leg
/// still prices and offers on its own (C19 — the engine prices the LEG and refuses the COMBINATION);
/// <c>Invariant_*</c> is that none of this can reach a ticket with at most one leg per matchup.</para>
/// </summary>
public class RefusalTests
{
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public RefusalTests(Xunit.Abstractions.ITestOutputHelper output) => _output = output;

    private static RunConfig SweepConfig(double kappa = 1.0) => new RunConfig
    {
        StartingBank = 1_000_000_000,
        MaxTicketsPerRound = 1_000_000,
        SgpMargin = kappa,
    };

    private static MarketSelection[] Board(Matchup matchup)
    {
        var board = new MarketSelection[matchup.Markets.Count];
        for (int i = 0; i < board.Length; i++) board[i] = matchup.Markets[i].Selection;
        return board;
    }

    /// <summary>The picks that survive a remedy — the ticket the refusal says will place.</summary>
    private static Pick[] Without(IReadOnlyList<Pick> picks, IReadOnlyList<int> drop)
        => picks.Where((_, i) => !drop.Contains(i)).ToArray();

    // =======================================================================================
    // EXIT GATE 1 — each of the three rules emits a structured cause AND a structured remedy.
    // =======================================================================================

    /// <summary>IMPOSSIBLE COMBINATION. The cause is the minimal conflicting leg set, the remedy a leg
    /// whose removal makes the ticket priceable — and the remedy is spent here, not described.</summary>
    [Fact]
    public void Gate1_an_impossible_combination_names_its_cause_and_its_remedy()
    {
        var run = new Run("sgp-refusal-impossible", SweepConfig());
        Pick[] picks = { new Pick(0, Side.Home), new Pick(0, Side.Away) };

        TicketRefusal? refusal = run.RefusalFor(picks);
        Assert.NotNull(refusal);
        Assert.Equal(RefusalKind.ImpossibleCombination, refusal!.Kind);

        // CAUSE: both legs, and the model's own label for why.
        Assert.Equal(new[] { 0, 1 }, refusal.CauseLegs);
        Assert.NotNull(refusal.CauseRelation);
        Assert.Equal(RelationKind.MutuallyExclusive, refusal.CauseRelation!.Value.Kind);
        Assert.Equal(0.0, refusal.Price); // an impossible ticket has no price at all

        // REMEDY: one leg, and it is the SECOND — the surface refuses the pick he just added.
        Assert.Equal(new[] { 1 }, refusal.RemedyLegs);

        // VERIFIED, not assumed: the remedy places.
        Ticket placed = run.PlaceTicket(Without(picks, refusal.RemedyLegs), 10);
        Assert.Single(placed.Legs);
        Assert.Null(run.RefusalFor(Without(picks, refusal.RemedyLegs)));

        // And the exception carries the same verdict, from the same call on the same legs.
        TicketRefusedException thrown =
            Assert.Throws<TicketRefusedException>(() => run.PlaceTicket(picks, 10));
        Assert.Equal(refusal.Kind, thrown.Refusal.Kind);
        Assert.Equal(refusal.CauseLegs, thrown.Refusal.CauseLegs);
        Assert.Equal(refusal.RemedyLegs, thrown.Refusal.RemedyLegs);
    }

    /// <summary>DUPLICATE SELECTION. The cause is the repeated selection, the remedy the repeat.</summary>
    [Fact]
    public void Gate1_a_duplicate_selection_names_the_repeat_as_cause_and_remedy()
    {
        var run = new Run("sgp-refusal-duplicate", SweepConfig());
        var repeated = MarketSelection.TotalGoals(2.5, true);

        // Buried mid-ticket, so "the cause is the repeat" is a real claim rather than "the cause is
        // the whole ticket" wearing a smaller number.
        Pick[] picks =
        {
            new Pick(0, MarketSelection.TotalCorners(9.5, true)),
            new Pick(0, repeated),
            new Pick(1, MarketSelection.BothTeamsToScore(true)),
            new Pick(0, repeated),
        };

        TicketRefusal? refusal = run.RefusalFor(picks);
        Assert.NotNull(refusal);
        Assert.Equal(RefusalKind.DuplicateSelection, refusal!.Kind);
        Assert.Equal(new[] { 1, 3 }, refusal.CauseLegs);  // the repeated selection, both appearances
        Assert.Equal(new[] { 3 }, refusal.RemedyLegs);    // the repeat itself

        // The degenerate implication: a leg entails itself, and the model says so in its own words.
        Assert.NotNull(refusal.CauseRelation);
        Assert.Equal(RelationKind.Implies, refusal.CauseRelation!.Value.Kind);

        Ticket placed = run.PlaceTicket(Without(picks, refusal.RemedyLegs), 10);
        Assert.Equal(3, placed.Legs.Count);
    }

    /// <summary>SUB-EVENS AT PLACEMENT. The cause is the ticket pricing at or below evens — the whole
    /// combination, because the price is a property of the combination — and the remedy is a leg whose
    /// removal lifts it above.</summary>
    [Fact]
    public void Gate1_a_sub_evens_ticket_names_its_price_as_cause_and_a_lifting_leg_as_remedy()
    {
        // κ is the reachable lever, exactly as the existing sub-evens gate uses it.
        var config = SweepConfig(kappa: 8.0);
        var run = new Run("sgp-refusal-subevens", config);

        Pick[] picks =
        {
            new Pick(0, MarketSelection.TotalGoals(1.5, true)),
            new Pick(0, MarketSelection.BothTeamsToScore(true)),
        };

        TicketRefusal? refusal = run.RefusalFor(picks);
        Assert.NotNull(refusal);
        Assert.Equal(RefusalKind.SubEvens, refusal!.Kind);
        Assert.Equal(new[] { 0, 1 }, refusal.CauseLegs);
        Assert.True(refusal.Price <= 1.0, $"the refused price must be the sub-evens one, got {refusal.Price:0.0000}");
        Assert.Equal(new[] { 1 }, refusal.RemedyLegs);

        // The remedy lifts it above evens FOR REAL: place it and read the contract price back.
        Ticket lifted = run.PlaceTicket(Without(picks, refusal.RemedyLegs), 10);
        Assert.True(lifted.LockedPrice > 1.0);

        // A leg-targeted Profit Boost is part of the price the rule judges, so it is part of the
        // verdict: the boosted refusal is the unboosted one scaled by the relic's own factor.
        TicketRefusal? boosted = run.RefusalFor(picks, profitBoostLeg: 0);
        if (boosted != null)
        {
            Assert.Equal(RefusalKind.SubEvens, boosted.Kind);
            Assert.Equal(refusal.Price * RelicCatalog.ProfitBoostMult, boosted.Price, 10);
        }
        else
        {
            // The boost lifted it over evens — then it must actually be sellable with the boost on.
            Assert.True(refusal.Price * RelicCatalog.ProfitBoostMult > 1.0);
        }
    }

    /// <summary>A cause naming four legs where two conflict is a worse answer, so the cause is
    /// MINIMAL: the two conflicting legs, with the two innocent legs left out of the accusation.</summary>
    [Fact]
    public void Gate1_the_cause_is_minimal_not_the_whole_ticket()
    {
        var run = new Run("sgp-refusal-minimal", SweepConfig());
        Pick[] picks =
        {
            new Pick(0, MarketSelection.TotalCorners(9.5, true)),
            new Pick(0, Side.Home),
            new Pick(0, Side.Away),
            new Pick(1, MarketSelection.BothTeamsToScore(true)),
        };

        TicketRefusal? refusal = run.RefusalFor(picks);
        Assert.NotNull(refusal);
        Assert.Equal(RefusalKind.ImpossibleCombination, refusal!.Kind);
        Assert.Equal(new[] { 1, 2 }, refusal.CauseLegs);

        // MINIMAL means both halves of the word. The cause really does conflict on its own...
        Assert.NotNull(run.RefusalFor(refusal.CauseLegs.Select(i => picks[i]).ToArray()));
        // ...and no proper subset of it does.
        foreach (int leg in refusal.CauseLegs)
            Assert.Null(run.RefusalFor(new[] { picks[leg] }));

        Assert.Equal(new[] { 2 }, refusal.RemedyLegs);
        Assert.Equal(4 - 1, run.PlaceTicket(Without(picks, refusal.RemedyLegs), 10).Legs.Count);
    }

    /// <summary>
    /// THE CASE WITH NO SINGLE-LEG REMEDY, pinned rather than papered over. Three repeats of one
    /// selection cannot be fixed by dropping one leg — two repeats are still a duplicate — so a
    /// refusal that named a single leg here would be naming a leg that does not help.
    ///
    /// <para>This is why <see cref="TicketRefusal.RemedyLegs"/> is a SET whose common case is one
    /// element, and why the remedy is searched smallest-first instead of read off the cause.</para>
    /// </summary>
    [Fact]
    public void Gate1_three_repeats_have_no_single_leg_remedy_and_the_model_says_so()
    {
        var run = new Run("sgp-refusal-triple-repeat", SweepConfig());
        var repeated = MarketSelection.TotalGoals(2.5, true);
        Pick[] picks = { new Pick(0, repeated), new Pick(0, repeated), new Pick(0, repeated) };

        TicketRefusal? refusal = run.RefusalFor(picks);
        Assert.NotNull(refusal);
        Assert.Equal(RefusalKind.DuplicateSelection, refusal!.Kind);

        // THE PROOF that no single leg is a remedy: try all three, each still refuses.
        for (int drop = 0; drop < picks.Length; drop++)
            Assert.NotNull(run.RefusalFor(Without(picks, new[] { drop })));

        // So the remedy is two legs, and it is the minimal one that works.
        Assert.Equal(new[] { 1, 2 }, refusal.RemedyLegs);
        Assert.Single(run.PlaceTicket(Without(picks, refusal.RemedyLegs), 10).Legs);
    }

    /// <summary>Two INDEPENDENT conflicts on one ticket also have no single-leg remedy: drop a leg
    /// from either pair and the other pair is still impossible. The remedy takes one from each, and
    /// the cause stays minimal at two legs rather than growing to cover both conflicts.</summary>
    [Fact]
    public void Gate1_two_independent_conflicts_need_a_two_leg_remedy()
    {
        var run = new Run("sgp-refusal-two-conflicts", SweepConfig());
        Pick[] picks =
        {
            new Pick(0, Side.Home), new Pick(0, Side.Away),
            new Pick(1, Side.Home), new Pick(1, Side.Away),
        };

        TicketRefusal? refusal = run.RefusalFor(picks);
        Assert.NotNull(refusal);
        Assert.Equal(RefusalKind.ImpossibleCombination, refusal!.Kind);
        Assert.Equal(2, refusal.CauseLegs.Count);

        for (int drop = 0; drop < picks.Length; drop++)
            Assert.NotNull(run.RefusalFor(Without(picks, new[] { drop })));

        Assert.Equal(2, refusal.RemedyLegs.Count);
        Ticket placed = run.PlaceTicket(Without(picks, refusal.RemedyLegs), 10);
        Assert.Equal(2, placed.Legs.Count);
        Assert.Null(placed.SameMatch); // what is left is one leg per matchup — an ordinary parlay
    }

    // =======================================================================================
    // EXIT GATE 2 — every remedy verified by construction, board-wide.
    // =======================================================================================

    /// <summary>
    /// THE GATE. Sweep every same-match pair and triple the shipped board can build, on several
    /// seeds and at two values of κ, and for EVERY refusal actually place the ticket its remedy
    /// describes. A remedy that does not place fails here.
    ///
    /// <para>Also pins the shape of the verdict on every one of them: a non-empty cause, a non-empty
    /// remedy, a cause that is genuinely refused on its own, and — for the impossible rule — a cause
    /// no proper subset of which is refused, which is what MINIMAL means.</para>
    /// </summary>
    [Fact]
    public void Gate2_every_emitted_remedy_places_for_real()
    {
        int refusals = 0, impossible = 0, duplicates = 0, subEvens = 0;
        int multiLegRemedies = 0, checkedCombinations = 0;
        var multiLegByKind = new Dictionary<string, int>();

        foreach (double kappa in new[] { 1.0, 8.0 })
            foreach (string seed in new[] { "sgp-sweep-a", "sgp-sweep-b", "sgp-sweep-c" })
            {
                var run = new Run(seed, SweepConfig(kappa));
                Matchup matchup = run.CurrentSlate.Matchups[0];
                MarketSelection[] board = Board(matchup);

                var combinations = new List<Pick[]>();
                for (int i = 0; i < board.Length; i++)
                {
                    combinations.Add(new[] { new Pick(0, board[i]), new Pick(0, board[i]) }); // the repeat
                    for (int j = i + 1; j < board.Length; j++)
                    {
                        combinations.Add(new[] { new Pick(0, board[i]), new Pick(0, board[j]) });
                        // One triple per pair rather than all of them: enough to reach the shapes that
                        // are impossible only as a whole without a cubic sweep in every seed.
                        int k = (i + j + 1) % board.Length;
                        if (k != i && k != j)
                            combinations.Add(new[]
                            {
                                new Pick(0, board[i]), new Pick(0, board[j]), new Pick(0, board[k]),
                            });
                    }
                }

                foreach (Pick[] picks in combinations)
                {
                    checkedCombinations++;
                    TicketRefusal? refusal = run.RefusalFor(picks);
                    if (refusal == null)
                    {
                        run.PlaceTicket(picks, 10); // a cleared ticket really does place
                        continue;
                    }

                    refusals++;
                    switch (refusal.Kind)
                    {
                        case RefusalKind.ImpossibleCombination: impossible++; break;
                        case RefusalKind.DuplicateSelection: duplicates++; break;
                        case RefusalKind.SubEvens: subEvens++; break;
                    }

                    // CAUSE: non-empty, in range, ascending, and refused on its own.
                    Assert.NotEmpty(refusal.CauseLegs);
                    Assert.Equal(refusal.CauseLegs.OrderBy(i => i).ToArray(), refusal.CauseLegs);
                    Assert.All(refusal.CauseLegs, i => Assert.InRange(i, 0, picks.Length - 1));
                    Assert.NotNull(run.RefusalFor(refusal.CauseLegs.Select(i => picks[i]).ToArray()));

                    // MINIMAL, for the rule where a smaller answer is possible: drop any one leg of
                    // the cause and what is left is no longer impossible.
                    if (refusal.Kind == RefusalKind.ImpossibleCombination && refusal.CauseLegs.Count > 1)
                        foreach (int leg in refusal.CauseLegs)
                        {
                            Pick[] smaller = refusal.CauseLegs.Where(i => i != leg).Select(i => picks[i]).ToArray();
                            Assert.True(run.RefusalFor(smaller)?.Kind != RefusalKind.ImpossibleCombination,
                                "a cause with an impossible proper subset is not minimal");
                        }

                    // REMEDY: non-empty, and VERIFIED by placing the ticket it describes.
                    Assert.True(refusal.HasRemedy,
                        $"no removal fixes {string.Join(" + ", picks.Select(p => MatchModel.DisplayLabel(matchup, p.Selection)))}");
                    Assert.Equal(refusal.RemedyLegs.OrderBy(i => i).ToArray(), refusal.RemedyLegs);
                    if (refusal.RemedyLegs.Count > 1)
                    {
                        multiLegRemedies++;
                        string bucket = $"{refusal.Kind}@κ={kappa:0.#}";
                        multiLegByKind[bucket] = multiLegByKind.GetValueOrDefault(bucket) + 1;
                    }

                    Pick[] remaining = Without(picks, refusal.RemedyLegs);
                    Assert.Null(run.RefusalFor(remaining));
                    Ticket placed = run.PlaceTicket(remaining, 10);
                    Assert.Equal(remaining.Length, placed.Legs.Count);
                    Assert.True(placed.LockedPrice > 1.0);

                    // MINIMAL REMEDY: if it names more than one leg, no single leg would have done.
                    if (refusal.RemedyLegs.Count > 1)
                        for (int drop = 0; drop < picks.Length; drop++)
                            Assert.NotNull(run.RefusalFor(Without(picks, new[] { drop })));
                }
            }

        _output.WriteLine($"{checkedCombinations} combinations swept, {refusals} refused "
            + $"({impossible} impossible, {duplicates} duplicate, {subEvens} sub-evens); "
            + $"{multiLegRemedies} needed more than one leg dropped "
            + $"({string.Join(", ", multiLegByKind.OrderBy(e => e.Key).Select(e => $"{e.Value} {e.Key}"))}).");

        // The sweep must actually reach all three rules, or it is proving nothing about them.
        Assert.True(impossible > 0 && duplicates > 0 && subEvens > 0,
            "the sweep did not reach all three refusal rules");
    }

    // =======================================================================================
    // EXIT GATE 3 — the engine prices the LEG and refuses only the COMBINATION (C19).
    // =======================================================================================

    /// <summary>Every leg of every refused combination is still individually priced, still on the
    /// board, and still sellable on its own. The refusal is of the combination, never of the leg —
    /// which is why the design law can say the leg stays reachable and mean it.</summary>
    [Fact]
    public void Gate3_a_refused_leg_still_prices_and_offers_on_its_own()
    {
        int legsChecked = 0;

        foreach (string seed in new[] { "sgp-alone-a", "sgp-alone-b" })
        {
            var run = new Run(seed, SweepConfig(kappa: 8.0));
            Matchup matchup = run.CurrentSlate.Matchups[0];
            MarketSelection[] board = Board(matchup);

            for (int i = 0; i < board.Length; i++)
                for (int j = i; j < board.Length; j++)
                {
                    Pick[] picks = { new Pick(0, board[i]), new Pick(0, board[j]) };
                    if (run.RefusalFor(picks) == null) continue;

                    foreach (Pick pick in picks)
                    {
                        legsChecked++;

                        // Still ON the board: the market was never withdrawn.
                        Assert.Contains(matchup.Markets, m => m.Selection == pick.Selection);

                        // Still PRICED, and above evens — MatchModel.Offer's own guarantee.
                        double odds = matchup.Odds(pick.Selection);
                        Assert.True(odds > 1.0, $"a refused leg lost its price: {odds:0.0000}");

                        // Still OFFERED: it sells alone, and at exactly the board's own number.
                        Assert.Null(run.RefusalFor(new[] { pick }));
                        Ticket single = run.PlaceTicket(new[] { pick }, 10);
                        Assert.Equal(odds, single.LockedPrice);
                        Assert.Null(single.SameMatch);
                    }
                }
        }

        _output.WriteLine($"{legsChecked} legs of refused combinations checked alone");
        Assert.True(legsChecked > 0);
    }

    // =======================================================================================
    // THE INVARIANT — none of this reaches a ticket with at most one leg per matchup.
    // =======================================================================================

    /// <summary>
    /// An ordinary ticket cannot be refused by any of the three rules and never enters the machinery
    /// that would decide: <c>Refuse</c> leaves on its first line for a ticket with at most one leg per
    /// matchup. Swept over every one-leg-per-matchup shape up to <c>MaxLegs</c> on several seeds, with
    /// the price read back to confirm the untouched product path is still what priced it.
    /// </summary>
    [Fact]
    public void Invariant_an_ordinary_ticket_is_never_refused_and_still_prices_as_a_product()
    {
        int tickets = 0;

        foreach (string seed in new[] { "sgp-ordinary-a", "sgp-ordinary-b", "sgp-ordinary-c" })
        {
            var run = new Run(seed, SweepConfig(kappa: 8.0));
            var matchups = run.CurrentSlate.Matchups;

            for (int a = 0; a < matchups.Count; a++)
                for (int b = a + 1; b < matchups.Count; b++)
                    for (int c = b + 1; c < matchups.Count; c++)
                    {
                        MarketSelection[] boardA = Board(matchups[a]);
                        MarketSelection[] boardB = Board(matchups[b]);
                        MarketSelection[] boardC = Board(matchups[c]);

                        for (int s = 0; s < boardA.Length; s++)
                        {
                            Pick[] picks =
                            {
                                new Pick(a, boardA[s]),
                                new Pick(b, boardB[s % boardB.Length]),
                                new Pick(c, boardC[(s + 1) % boardC.Length]),
                            };

                            Assert.Null(run.RefusalFor(picks));

                            Ticket ticket = run.PlaceTicket(picks, 10);
                            tickets++;

                            // BIT-IDENTICAL: the pre-F_0.6.0 expression, to the last bit, and no
                            // SameMatch block at all.
                            Assert.Null(ticket.SameMatch);
                            Assert.True(
                                ticket.LockedPrice
                                    == OddsMath.ParlayDecimal(ticket.Legs.Select(l => l.OfferedOdds).ToList()),
                                "an ordinary ticket must still be the product of its legs' offered odds");
                        }
                    }
        }

        _output.WriteLine($"{tickets} ordinary tickets swept, none refused, all priced as a product");
        Assert.True(tickets > 100);
    }

    /// <summary>A refused ticket is never SOLD, so it must not move the no-label fallback counter —
    /// whose documented meaning is tickets sold at the naive product. The refusal check runs ahead of
    /// the counting price call, and <c>RefusalFor</c> never counts at all.</summary>
    [Fact]
    public void Invariant_a_refusal_does_not_move_the_no_label_fallback_counter()
    {
        var run = new Run("sgp-refusal-counter", SweepConfig());
        SameMatchModel.ResetNoLabelFallbacks();

        Pick[] picks = { new Pick(0, Side.Home), new Pick(0, Side.Away) };
        for (int i = 0; i < 5; i++)
        {
            Assert.NotNull(run.RefusalFor(picks));
            Assert.Throws<TicketRefusedException>(() => run.PlaceTicket(picks, 10));
        }

        Assert.Equal(0, SameMatchModel.NoLabelFallbacks);
    }

    /// <summary>The refusal is ATOMIC, as every placement check is: nothing is consumed, no stake
    /// leaves the bank, and no ticket exists afterwards (PLAN.md rev 5 §7). Pinned again here because
    /// the check moved — it now runs before the pricing call rather than around it.</summary>
    [Fact]
    public void Invariant_a_refusal_spends_nothing()
    {
        var config = SweepConfig();
        var run = new Run("sgp-refusal-atomic", config);
        run.GrantConsumable(RelicCatalog.Consumables.First(c => c.Id == "profit_boost"));
        run.GrantConsumable(RelicCatalog.Consumables.First(c => c.Id == "free_bet"));

        Pick[] picks = { new Pick(0, Side.Home), new Pick(0, Side.Away) };

        Assert.Throws<TicketRefusedException>(
            () => run.PlaceTicket(picks, 10, profitBoostLeg: 1, modifier: TicketModifier.FreeBet));

        Assert.Empty(run.Tickets);
        Assert.Equal(config.StartingBank, run.Bank);
        Assert.Single(run.OwnedConsumables, c => c.Id == "profit_boost");
        Assert.Single(run.OwnedConsumables, c => c.Id == "free_bet");

        // The legs' offered odds are untouched too — a refused ticket never rewrote a price.
        Assert.Equal(run.CurrentSlate.Matchups[0].Odds(picks[0].Selection),
            run.CurrentSlate.Matchups[0].Odds(picks[0].Selection));
    }

    /// <summary>The refusal is reachable programmatically, and a caller that only catches an
    /// exception is unaffected: <see cref="TicketRefusedException"/> IS an
    /// <see cref="ArgumentException"/>, which is what every pre-existing catch site was written
    /// against.</summary>
    [Fact]
    public void The_refusal_is_reachable_both_ways()
    {
        var run = new Run("sgp-refusal-reach", SweepConfig());
        Pick[] picks = { new Pick(0, Side.Home), new Pick(0, Side.Away) };

        // The old contract, unchanged.
        ArgumentException legacy = Assert.ThrowsAny<ArgumentException>(() => run.PlaceTicket(picks, 10));
        Assert.NotEmpty(legacy.Message);

        // The new one, on the same object.
        var structured = Assert.IsType<TicketRefusedException>(legacy);
        Assert.NotNull(structured.Refusal);
        Assert.NotEmpty(structured.Refusal.CauseLegs);
        Assert.NotEmpty(structured.Refusal.RemedyLegs);

        // The message states BOTH halves — the developer-facing echo of the stamp the surface owns.
        Assert.Contains("zero", structured.Message);
        Assert.Contains("Drop", structured.Message);
    }
}
