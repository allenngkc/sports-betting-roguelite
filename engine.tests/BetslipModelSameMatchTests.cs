using System;
using System.Collections.Generic;
using System.Linq;
using SBR.Engine;
using SBR.Game;
using Xunit;

namespace SBR.Engine.Tests;

/// <summary>
/// THE SLIP'S MODEL HALF (F_0.6.0 step 5, P1–P3) — its four exit gates, verified headlessly.
///
/// <para><see cref="BetslipModel"/> lives in the Unity assembly and is read a second time from here
/// by source include (see the csproj). Unity's EditMode runner needs an editor lease this lane does
/// not hold; the whole point of keeping the model Unity-free was that it would not need one.</para>
///
/// <para><b>GATE 1 — constructible.</b> A matchup may carry two or more legs, built with
/// <c>AddLeg</c>, removed with <c>RemoveLeg</c>/<c>RemoveSelection</c>, capped by <c>MaxLegs</c>, and
/// placed. <c>Toggle</c> still REPLACES.</para>
///
/// <para><b>GATE 2 — bit-identity.</b> Every slip with at most one leg per matchup prices to the
/// exact double the old <c>OddsMath.ParlayDecimal</c> produced. Asserted with <c>==</c>: a tolerance
/// would pass on the very drift the gate exists to forbid.</para>
///
/// <para><b>GATE 3 — refusal before commit.</b> A refused combination is a structured verdict with a
/// minimal cause and a remedy, read without throwing and without placing, and the remedy is spent as
/// a SET and then actually places.</para>
///
/// <para><b>GATE 4 — nothing broke for the screen.</b> The surface accessors behave as they did, and
/// what the MATCHUP-KEYED ones answer on a several-leg matchup is pinned here so the screen lane can
/// read the contract instead of guessing.</para>
/// </summary>
public class BetslipModelSameMatchTests
{
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public BetslipModelSameMatchTests(Xunit.Abstractions.ITestOutputHelper output) => _output = output;

    // =========================================================================== fixtures

    /// <summary>A bank and a ticket cap large enough that a sweep never trips a placement rule it
    /// is not testing. Shaped exactly like <c>RefusalTests.SweepConfig</c> so the seeds that file
    /// pinned reproduce here.</summary>
    private static RunConfig Sandbox(double kappa = 1.0, int maxTickets = 1_000_000) => new RunConfig
    {
        StartingBank = 1_000_000_000,
        MaxTicketsPerRound = maxTickets,
        SgpMargin = kappa,
    };

    /// <summary>A slip staked at the floor. <c>PlaceTicket</c> debits the stake, and the default
    /// 10%-of-bank anchor would drain even the sandbox bank inside ten placements.</summary>
    private static BetslipModel CheapSlip(Run run)
    {
        var slip = new BetslipModel(run);
        slip.SetStakeFraction(0.0);   // clamps UP to MinStake
        return slip;
    }

    private static void AdvanceRound(Run run)
    {
        run.LockRound();
        run.FastForwardRound();
        run.Settle();
        run.ExitShop();
    }

    /// <summary>Spends a refusal's remedy. The remedy is a SET of indices into Picks, not a leg, so
    /// it is removed high-to-low: an earlier removal would shift every later index.</summary>
    private static void SpendRemedy(BetslipModel slip, TicketRefusal refusal)
    {
        foreach (int leg in refusal.RemedyLegs.OrderByDescending(i => i))
            Assert.True(slip.RemoveLeg(leg), $"remedy leg {leg} was not removable");
    }

    /// <summary>Test-local PRNG. The run's own <c>RngHub</c> is deliberately untouched — drawing
    /// from it would move the streams every golden pin in this suite depends on.</summary>
    private sealed class Lcg
    {
        private ulong _s;
        public Lcg(ulong seed) => _s = seed * 6364136223846793005UL + 1442695040888963407UL;

        public int Next(int bound)
        {
            _s = _s * 6364136223846793005UL + 1442695040888963407UL;
            return (int)((_s >> 33) % (ulong)bound);
        }
    }

    // ===================================================================================
    // GATE 1 — the instrument is CONSTRUCTIBLE.
    // ===================================================================================

    [Fact]
    public void Gate1_two_legs_on_one_matchup_build_and_place()
    {
        var run = new Run("sgp-slip-constructible", Sandbox());
        var slip = CheapSlip(run);

        MarketSelection goals = MarketSelection.TotalGoals(2.5, true);
        MarketSelection corners = MarketSelection.TotalCorners(9.5, true);

        Assert.True(slip.AddLeg(0, goals));
        Assert.True(slip.AddLeg(0, corners));

        Assert.Equal(2, slip.Picks.Count);
        Assert.Equal(2, slip.LegCountOn(0));
        Assert.True(slip.IsSameMatch);
        Assert.Equal(new[] { 0, 1 }, slip.LegIndicesOn(0));
        Assert.True(slip.Contains(0, goals));
        Assert.True(slip.Contains(0, corners));
        Assert.False(slip.Contains(1, goals));

        Assert.Null(slip.Refusal);
        Assert.Null(slip.PlaceBlocker);
        Assert.True(slip.CanPlace);

        // THROUGH THE ENGINE, not merely held: the instrument is only constructible if it sells.
        Ticket ticket = slip.Place();
        Assert.Equal(2, ticket.Legs.Count);
        Assert.Same(ticket.Legs[0].Matchup, ticket.Legs[1].Matchup);
        Assert.NotNull(ticket.SameMatch);
        Assert.Single(run.Tickets);
        Assert.Empty(slip.Picks);
    }

    [Fact]
    public void Gate1_RemoveLeg_takes_one_leg_and_RemoveSelection_takes_the_named_one()
    {
        var run = new Run("sgp-slip-removal", Sandbox());
        var slip = CheapSlip(run);

        MarketSelection a = MarketSelection.TotalGoals(2.5, true);
        MarketSelection b = MarketSelection.TotalCorners(9.5, true);
        MarketSelection c = MarketSelection.TotalCards(4.5, true);

        Assert.True(slip.AddLeg(0, a));
        Assert.True(slip.AddLeg(0, b));
        Assert.True(slip.AddLeg(0, c));

        // BY INDEX: the middle leg, and only it.
        Assert.True(slip.RemoveLeg(1));
        Assert.Equal(new[] { a, c }, slip.Picks.Select(p => p.Selection));
        Assert.Equal(2, slip.LegCountOn(0));

        // BY NAME: index-free, so a surface rebuilt between the render and the click cannot take
        // the wrong leg.
        Assert.True(slip.RemoveSelection(0, a));
        Assert.Equal(new[] { c }, slip.Picks.Select(p => p.Selection));

        // And both are honest about what they did not find.
        Assert.False(slip.RemoveSelection(0, b));   // not on the slip
        Assert.False(slip.RemoveSelection(1, c));   // right selection, wrong matchup
        Assert.False(slip.RemoveLeg(-1));
        Assert.False(slip.RemoveLeg(slip.Picks.Count));
        Assert.Single(slip.Picks);
    }

    [Fact]
    public void Gate1_MaxLegs_still_binds_when_every_leg_is_on_one_matchup()
    {
        var run = new Run("sgp-slip-maxlegs", Sandbox());
        int max = run.Config.MaxLegs;
        IReadOnlyList<MarketOffer> board = run.CurrentSlate.Matchups[0].Markets;
        var slip = CheapSlip(run);

        // The cap is a SLIP cap, not a per-matchup cap: fill it from one match's board.
        for (int i = 0; i < max; i++) Assert.True(slip.AddLeg(0, board[i].Selection));
        Assert.Equal(max, slip.Picks.Count);

        Assert.False(slip.AddLeg(0, board[max].Selection));                  // same matchup
        Assert.False(slip.AddLeg(1, MarketSelection.Moneyline(Side.Home)));  // another matchup
        Assert.False(slip.Toggle(1, Side.Home));                             // the old gesture too
        Assert.Equal(max, slip.Picks.Count);

        // A REPLACEMENT is still allowed at the cap — it does not grow the slip.
        Assert.True(slip.Toggle(0, board[max].Selection));
        Assert.Equal(max, slip.Picks.Count);

        // Off-slate matchups are refused, not thrown.
        Assert.False(slip.AddLeg(-1, MarketSelection.Moneyline(Side.Home)));
        Assert.False(slip.AddLeg(run.CurrentSlate.Matchups.Count, MarketSelection.Moneyline(Side.Home)));

        // A selection that is not on this matchup's board still throws, as Toggle always has.
        Assert.Throws<ArgumentException>(() => slip.AddLeg(0, MarketSelection.CorrectScore(9, 9)));
    }

    [Fact]
    public void Gate1_Toggle_still_REPLACES_and_never_adds()
    {
        // PINNED ON ITS OWN, DELIBERATELY. Whether a second market on a match should ADD rather than
        // REPLACE is an interaction decision the surface owns, not this lane. A well-meaning later
        // change that made Toggle add would pass every other test in this file.
        var run = new Run("sgp-slip-toggle", Sandbox());
        var slip = CheapSlip(run);

        MarketSelection a = MarketSelection.TotalGoals(2.5, true);
        MarketSelection b = MarketSelection.TotalCorners(9.5, true);
        MarketSelection c = MarketSelection.BothTeamsToScore(true);

        Assert.True(slip.Toggle(0, a));
        Assert.Single(slip.Picks);

        Assert.True(slip.Toggle(0, b));      // a DIFFERENT market on the SAME matchup...
        Assert.Single(slip.Picks);           // ... replaced it. It did not add.
        Assert.False(slip.Contains(0, a));
        Assert.True(slip.Contains(0, b));
        Assert.False(slip.IsSameMatch);

        Assert.True(slip.Toggle(0, b));      // the same one again un-clicks
        Assert.Empty(slip.Picks);

        // On a group AddLeg built, Toggle replaces the FIRST leg and leaves the rest standing.
        Assert.True(slip.AddLeg(0, a));
        Assert.True(slip.AddLeg(0, b));
        Assert.True(slip.Toggle(0, c));
        Assert.Equal(2, slip.Picks.Count);
        Assert.Equal(new[] { c, b }, slip.Picks.Select(p => p.Selection));

        // ... and un-clicking still finds the exact leg wherever it sits, rather than reading the
        // click as a replacement and manufacturing a duplicate.
        Assert.True(slip.Toggle(0, b));
        Assert.Equal(new[] { c }, slip.Picks.Select(p => p.Selection));
    }

    // ===================================================================================
    // GATE 2 — BIT-IDENTITY. The highest-risk claim in step 5.
    // ===================================================================================

    [Fact]
    public void Gate2_every_ordinary_slip_prices_BIT_IDENTICALLY_to_the_old_ParlayDecimal()
    {
        // This is what stops the screen's displayed number moving for ordinary tickets, and it is
        // the same invariant that governed step 3. Swept over a generated population rather than a
        // handful of shapes: many seeds, many rounds, every width from 1 to MaxLegs, selections
        // drawn from the whole board (moneyline incl. the draw, totals, scorers, the V1 vocabulary).
        const int seeds = 24, rounds = 5, trialsPerRound = 40;
        int maxLegs = new RunConfig().MaxLegs;

        int slips = 0, legsSeen = 0;
        var widths = new SortedSet<int>();

        for (int seed = 0; seed < seeds; seed++)
        {
            var run = new Run($"sgp-bit-identity-{seed}", Sandbox());

            for (int round = 0; round < rounds; round++)
            {
                var rng = new Lcg((ulong)(seed * 1_000_003 + round * 7919 + 11));
                Slate slate = run.CurrentSlate;

                for (int trial = 0; trial < trialsPerRound; trial++)
                {
                    var slip = CheapSlip(run);
                    var used = new List<int>();
                    var offered = new List<double>();

                    int want = 1 + rng.Next(run.Config.MaxLegs);
                    for (int k = 0; k < want; k++)
                    {
                        int m = rng.Next(slate.Matchups.Count);
                        if (used.Contains(m)) continue;          // AT MOST ONE LEG PER MATCHUP
                        Matchup matchup = slate.Matchups[m];
                        MarketSelection sel = matchup.Markets[rng.Next(matchup.Markets.Count)].Selection;
                        Assert.True(slip.AddLeg(m, sel));
                        used.Add(m);
                        offered.Add(matchup.Odds(sel));
                    }

                    Assert.False(slip.IsSameMatch);
                    Assert.Null(slip.SameMatchPricing);
                    Assert.Null(slip.Refusal);

                    double old = OddsMath.ParlayDecimal(offered);

                    // EXACT. Not Assert.Equal(expected, actual, precision): a tolerance here would
                    // pass on the very drift this gate exists to forbid.
                    Assert.True(slip.TicketOdds == old,
                        $"seed {seed} round {round} trial {trial} ({offered.Count} legs): "
                        + $"TicketOdds {slip.TicketOdds:R} != ParlayDecimal {old:R}");
                    Assert.True(slip.CombinedOdds == old,
                        "CombinedOdds must be TicketOdds, to the bit");
                    Assert.True(slip.ToWin == slip.Stake * old,
                        $"ToWin {slip.ToWin:R} != stake x price {slip.Stake * old:R}");

                    slips++;
                    legsSeen += slip.Picks.Count;
                    widths.Add(slip.Picks.Count);
                }

                if (round < rounds - 1)
                {
                    run.PlaceTicket(new[] { new Pick(0, Side.Home) }, run.Config.MinStake);
                    AdvanceRound(run);
                }
            }
        }

        _output.WriteLine($"BIT-IDENTITY: {slips} slips / {legsSeen} legs, {seeds} seeds x {rounds} "
            + $"rounds ({seeds * rounds} slates), widths {string.Join(",", widths)} — all exact.");

        // The sweep has to be wide enough to be evidence rather than an anecdote.
        Assert.True(slips >= 4_000, $"only {slips} slips swept");
        Assert.Equal(Enumerable.Range(1, maxLegs), widths);   // every width 1..MaxLegs reached
        Assert.True(legsSeen > slips, "the sweep must not have collapsed to all-singles");
    }

    // ===================================================================================
    // GATE 3 — the REFUSAL arrives BEFORE the commit, and its remedy is a fix.
    // ===================================================================================

    [Fact]
    public void Gate3_an_impossible_combination_refuses_before_commit_and_the_remedy_places()
    {
        var run = new Run("sgp-slip-impossible", Sandbox());
        var slip = CheapSlip(run);

        Assert.True(slip.AddLeg(0, Side.Home));
        Assert.True(slip.AddLeg(0, Side.Away));

        // Reading the verdict must not throw, and must not commit anything.
        TicketRefusal refusal = slip.Refusal;
        Assert.NotNull(refusal);
        Assert.Equal(RefusalKind.ImpossibleCombination, refusal.Kind);
        Assert.Empty(run.Tickets);

        // CAUSE: minimal, spoken in indices into Picks, with the model's own label for why.
        Assert.Equal(new[] { 0, 1 }, refusal.CauseLegs);
        Assert.NotNull(refusal.CauseRelation);
        Assert.Equal(RelationKind.MutuallyExclusive, refusal.CauseRelation!.Value.Kind);
        Assert.Equal(0.0, refusal.Price);   // an impossible ticket has no price at all

        // REMEDY: a SET, non-empty.
        Assert.True(refusal.HasRemedy);
        Assert.NotEmpty(refusal.RemedyLegs);

        // The blocker is a machine TOKEN, never copy, and PLACE is dead while it stands.
        Assert.Equal("refused:" + RefusalKind.ImpossibleCombination, slip.PlaceBlocker);
        Assert.False(slip.CanPlace);

        // VERIFIED, not assumed: spending the remedy yields a slip that places.
        SpendRemedy(slip, refusal);
        Assert.Null(slip.Refusal);
        Assert.Null(slip.PlaceBlocker);
        Assert.True(slip.CanPlace);

        Ticket placed = slip.Place();
        Assert.Single(placed.Legs);
        Assert.Single(run.Tickets);
    }

    [Fact]
    public void Gate3_a_duplicate_selection_refuses_before_commit_and_the_remedy_places()
    {
        var run = new Run("sgp-slip-duplicate", Sandbox());
        var slip = CheapSlip(run);
        MarketSelection repeated = MarketSelection.TotalGoals(2.5, true);

        // Buried mid-slip, so "the cause is the repeat" is a real claim rather than "the cause is
        // the whole slip" wearing a smaller number. AddLeg accepts the repeat deliberately: the
        // verdict needs the combination to exist before it can name a cause and a remedy for it.
        Assert.True(slip.AddLeg(0, MarketSelection.TotalCorners(9.5, true)));
        Assert.True(slip.AddLeg(0, repeated));
        Assert.True(slip.AddLeg(1, MarketSelection.BothTeamsToScore(true)));
        Assert.True(slip.AddLeg(0, repeated));

        TicketRefusal refusal = slip.Refusal;
        Assert.NotNull(refusal);
        Assert.Equal(RefusalKind.DuplicateSelection, refusal.Kind);
        Assert.Equal(new[] { 1, 3 }, refusal.CauseLegs);   // both appearances, and nothing else
        Assert.Equal(new[] { 3 }, refusal.RemedyLegs);     // the repeat itself
        Assert.NotNull(refusal.CauseRelation);
        Assert.Equal(RelationKind.Implies, refusal.CauseRelation!.Value.Kind);

        Assert.Equal("refused:" + RefusalKind.DuplicateSelection, slip.PlaceBlocker);
        Assert.False(slip.CanPlace);
        Assert.Empty(run.Tickets);

        SpendRemedy(slip, refusal);
        Assert.Null(slip.Refusal);
        Ticket placed = slip.Place();
        Assert.Equal(3, placed.Legs.Count);
    }

    [Fact]
    public void Gate3_a_sub_evens_slip_refuses_and_the_remedy_lifts_it_over_evens()
    {
        // Seed and config are RefusalTests' own sub-evens pin, so this reads the SAME verdict
        // through the slip that the engine test reads through Run.
        var run = new Run("sgp-refusal-subevens", Sandbox(kappa: 8.0));
        var slip = CheapSlip(run);

        Assert.True(slip.AddLeg(0, MarketSelection.TotalGoals(1.5, true)));
        Assert.True(slip.AddLeg(0, MarketSelection.BothTeamsToScore(true)));

        TicketRefusal refusal = slip.Refusal;
        Assert.NotNull(refusal);
        Assert.Equal(RefusalKind.SubEvens, refusal.Kind);
        Assert.True(refusal.Price <= 1.0, $"refused price {refusal.Price:0.0000} is not sub-evens");
        Assert.Equal("refused:" + RefusalKind.SubEvens, slip.PlaceBlocker);
        Assert.False(slip.CanPlace);
        Assert.Empty(run.Tickets);

        SpendRemedy(slip, refusal);
        Assert.Null(slip.Refusal);
        Ticket lifted = slip.Place();
        Assert.True(lifted.LockedPrice > 1.0);
    }

    [Fact]
    public void Gate3_every_refusal_the_sweep_finds_carries_a_remedy_that_actually_places()
    {
        // The remedy is a claim about the world, not advice: spend it and the slip must SELL.
        // Swept at the shipped kappa AND at a kappa high enough to reach the sub-evens rule, over
        // slips built entirely on one matchup so refusals are common rather than incidental.
        int refusals = 0, remediable = 0, placedAfterRemedy = 0, widestRemedy = 0;
        var kinds = new SortedSet<string>();
        var widestAt = new SortedDictionary<double, int>();
        var setRemedyKinds = new SortedSet<string>();

        foreach (double kappa in new[] { 1.0, 8.0 })
        {
            widestAt[kappa] = 0;
            for (int seed = 0; seed < 12; seed++)
            {
                var run = new Run($"sgp-remedy-{kappa:0.0}-{seed}", Sandbox(kappa));
                var rng = new Lcg((ulong)(seed * 31 + (int)(kappa * 13) + 5));
                Matchup matchup = run.CurrentSlate.Matchups[0];

                for (int trial = 0; trial < 60; trial++)
                {
                    var slip = CheapSlip(run);
                    int want = 2 + rng.Next(run.Config.MaxLegs - 1);   // 2..MaxLegs, all on matchup 0
                    for (int k = 0; k < want; k++)
                        Assert.True(slip.AddLeg(0, matchup.Markets[rng.Next(matchup.Markets.Count)].Selection));

                    Assert.True(slip.IsSameMatch);

                    TicketRefusal refusal = slip.Refusal;   // never throws, whatever is on the slip
                    if (refusal == null)
                    {
                        Assert.True(slip.CanPlace);
                        continue;
                    }

                    refusals++;
                    kinds.Add(refusal.Kind.ToString());
                    Assert.NotEmpty(refusal.CauseLegs);
                    Assert.False(slip.CanPlace);
                    Assert.Equal("refused:" + refusal.Kind, slip.PlaceBlocker);

                    if (!refusal.HasRemedy) continue;
                    remediable++;
                    widestRemedy = Math.Max(widestRemedy, refusal.RemedyLegs.Count);
                    widestAt[kappa] = Math.Max(widestAt[kappa], refusal.RemedyLegs.Count);
                    if (refusal.RemedyLegs.Count > 1) setRemedyKinds.Add($"{refusal.Kind}@k{kappa:0.0}");

                    // Spent as a SET — however many legs it names.
                    int before = slip.Picks.Count;
                    SpendRemedy(slip, refusal);
                    Assert.Equal(before - refusal.RemedyLegs.Count, slip.Picks.Count);

                    Assert.Null(slip.Refusal);
                    Assert.True(slip.CanPlace);
                    Assert.NotNull(slip.Place());
                    placedAfterRemedy++;
                }
            }
        }

        _output.WriteLine($"REMEDIES: {refusals} refusals ({string.Join(",", kinds)}), {remediable} "
            + $"with a remedy, {placedAfterRemedy} placed after spending it; widest remedy "
            + $"{widestRemedy} leg(s), by kappa "
            + string.Join(" ", widestAt.Select(kv => $"k={kv.Key:0.0}:{kv.Value}"))
            + $"; multi-leg remedies seen for {string.Join(",", setRemedyKinds)}");

        Assert.True(refusals >= 100, $"only {refusals} refusals reached; the sweep proves too little");
        Assert.True(kinds.Count >= 2, $"only one refusal kind reached: {string.Join(",", kinds)}");
        Assert.Equal(refusals, remediable);            // every refusal named a way out...
        Assert.Equal(remediable, placedAfterRemedy);   // ... and every way out sold.

        // THE REMEDY IS A SET, AND THE SWEEP PROVES IT RATHER THAN ASSUMING IT. If this ever drops
        // to 1 the set-spending path above has gone untested and SpendRemedy's ordering — remove
        // high-to-low, or the earlier removal shifts every later index — is no longer exercised.
        Assert.True(widestRemedy > 1,
            $"no multi-leg remedy was reached (widest {widestRemedy}); the SET claim is untested");
    }

    // ===================================================================================
    // GATE 4 — nothing broke for the screen, and the new contract is written down.
    // ===================================================================================

    [Fact]
    public void Gate4_PlaceBlocker_keeps_its_legacy_strings_and_its_legacy_order()
    {
        // The UI renders these verbatim and they belong to another lane. Every answer reachable
        // before P1 must still be the answer it was, and the refusal is checked LAST.
        var run = new Run("sgp-slip-blockers", Sandbox(maxTickets: 2));
        var slip = CheapSlip(run);
        Assert.Equal("pick a side", slip.PlaceBlocker);
        Assert.False(slip.CanPlace);

        Assert.True(slip.Toggle(0, Side.Home));
        Assert.Null(slip.PlaceBlocker);

        run.PlaceTicket(new[] { new Pick(1, Side.Home) }, run.Config.MinStake);
        run.PlaceTicket(new[] { new Pick(2, Side.Home) }, run.Config.MinStake);
        Assert.Equal("max 2 tickets", slip.PlaceBlocker);

        // "betting is closed" OUTRANKS a refusal — the order is unchanged.
        var closed = new Run("sgp-slip-blockers-closed", Sandbox());
        var closedSlip = CheapSlip(closed);
        Assert.True(closedSlip.AddLeg(0, Side.Home));
        Assert.True(closedSlip.AddLeg(0, Side.Away));
        Assert.NotNull(closedSlip.Refusal);
        Assert.StartsWith("refused:", closedSlip.PlaceBlocker);
        closed.LockRound();
        Assert.Equal("betting is closed", closedSlip.PlaceBlocker);

        var poor = new Run("sgp-slip-blockers-poor",
            new RunConfig { StartingBank = 350, MinStake = 1_000_000 });
        var poorSlip = new BetslipModel(poor);
        Assert.True(poorSlip.Toggle(0, Side.Home));
        Assert.Equal("bank too small", poorSlip.PlaceBlocker);

        // The stake is anchored, then the bank moves under it.
        var drained = new Run("sgp-slip-blockers-drained",
            new RunConfig { StartingBank = 1000, MaxTicketsPerRound = 5 });
        var drainedSlip = new BetslipModel(drained);
        Assert.True(drainedSlip.Toggle(0, Side.Home));
        drainedSlip.SetStakeFraction(1.0);
        Assert.Equal(1000.0, drainedSlip.Stake);
        drained.PlaceTicket(new[] { new Pick(1, Side.Home) }, 900);
        Assert.Equal("stake exceeds bank", drainedSlip.PlaceBlocker);
    }

    [Fact]
    public void Gate4_the_matchup_keyed_accessors_answer_for_the_FIRST_leg_on_the_matchup()
    {
        // THE CONTRACT THE SCREEN LANE MUST READ, stated explicitly rather than left to be guessed.
        // SelectionOn and SideOn are matchup-keyed, a matchup can now carry several legs, and they
        // answer for the FIRST of them in slip order — the pre-P1 behaviour preserved exactly. A
        // surface rendering a SAME MATCH group must address LEGS: LegIndicesOn / Contains.
        var run = new Run("sgp-slip-accessors", Sandbox());
        MarketSelection goals = MarketSelection.TotalGoals(2.5, true);
        MarketSelection home = MarketSelection.Moneyline(Side.Home);

        // Non-moneyline FIRST, moneyline second: SideOn answers NULL even though a side is on the
        // slip, because it reads the first leg on the matchup and stops.
        var slip = CheapSlip(run);
        Assert.True(slip.AddLeg(0, goals));
        Assert.True(slip.AddLeg(0, Side.Home));
        Assert.Equal(goals, slip.SelectionOn(0));
        Assert.Null(slip.SideOn(0));
        Assert.True(slip.Contains(0, home));           // ... but it IS on the slip
        Assert.Equal(2, slip.LegCountOn(0));
        Assert.Equal(new[] { 0, 1 }, slip.LegIndicesOn(0));

        // The SAME two legs in the other order answer differently. That is the whole point.
        var other = CheapSlip(run);
        Assert.True(other.AddLeg(0, Side.Home));
        Assert.True(other.AddLeg(0, goals));
        Assert.Equal(home, other.SelectionOn(0));
        Assert.Equal(Side.Home, other.SideOn(0));
        Assert.True(other.Contains(0, goals));

        // A matchup with nothing on it answers empty, and never throws.
        Assert.Null(slip.SelectionOn(3));
        Assert.Null(slip.SideOn(3));
        Assert.Equal(0, slip.LegCountOn(3));
        Assert.Empty(slip.LegIndicesOn(3));
        Assert.False(slip.Contains(3, goals));
    }

    [Fact]
    public void Gate4_a_moneyline_DRAW_leg_has_no_side_and_is_still_addressable()
    {
        // The draw is not a team, ever (DD batch 49). Reachable now that the board has its third row.
        var run = new Run("sgp-slip-draw", Sandbox());
        var slip = CheapSlip(run);
        MarketSelection draw = MarketSelection.MoneylineDraw();

        Assert.True(slip.AddLeg(0, draw));
        Assert.Equal(draw, slip.SelectionOn(0));
        Assert.Null(slip.SideOn(0));
        Assert.True(slip.Contains(0, draw));
        Assert.Equal(1, slip.LegCountOn(0));
        Assert.True(slip.CanPlace);
    }

    [Fact]
    public void Gate4_Remove_takes_the_WHOLE_group_and_Clear_resets_every_derived_reading()
    {
        var run = new Run("sgp-slip-group-remove", Sandbox());
        var slip = CheapSlip(run);

        Assert.True(slip.AddLeg(0, MarketSelection.TotalGoals(2.5, true)));
        Assert.True(slip.AddLeg(0, MarketSelection.TotalCorners(9.5, true)));
        Assert.True(slip.AddLeg(1, MarketSelection.BothTeamsToScore(true)));

        // Matchup-keyed remove takes EVERY leg on the matchup — the honest reading, and unchanged
        // from before P1 where "every" was always "the one".
        slip.Remove(0);
        Assert.Single(slip.Picks);
        Assert.Equal(1, slip.Picks[0].MatchupIndex);
        Assert.Equal(0, slip.LegCountOn(0));
        Assert.False(slip.IsSameMatch);

        slip.Remove(4);   // a matchup with nothing on it is a no-op, not a throw
        Assert.Single(slip.Picks);

        slip.Clear();
        Assert.Empty(slip.Picks);
        Assert.Equal(0.0, slip.CombinedOdds);
        Assert.Equal(0.0, slip.TicketOdds);
        Assert.Equal(0.0, slip.ToWin);
        Assert.Null(slip.SameMatchPricing);
        Assert.Null(slip.Refusal);
        Assert.Equal("pick a side", slip.PlaceBlocker);
    }

    [Fact]
    public void Gate4_the_ordinary_moneyline_slip_behaves_exactly_as_the_screen_expects()
    {
        // LaptopOs / LaptopScreen / SportsbookApp consume these and belong to another lane that
        // cannot be edited from here.
        var run = new Run("sgp-slip-screen", Sandbox());
        Matchup m0 = run.CurrentSlate.Matchups[0];
        Matchup m1 = run.CurrentSlate.Matchups[1];
        var slip = CheapSlip(run);

        Assert.False(slip.Toggle(-1, Side.Home));
        Assert.False(slip.Toggle(run.CurrentSlate.Matchups.Count, Side.Home));

        Assert.True(slip.Toggle(0, Side.Home));
        Assert.Equal(Side.Home, slip.SideOn(0));
        Assert.Equal(MarketSelection.Moneyline(Side.Home), slip.SelectionOn(0));
        Assert.True(slip.CombinedOdds == m0.Odds(Side.Home));
        Assert.True(slip.ToWin == slip.Stake * m0.Odds(Side.Home));
        Assert.Null(slip.SameMatchPricing);
        Assert.Null(slip.PlaceBlocker);

        Assert.True(slip.Toggle(0, Side.Away));
        Assert.Equal(Side.Away, slip.SideOn(0));
        Assert.True(slip.CombinedOdds == m0.Odds(Side.Away));

        // A second matchup is an ordinary parlay and prices as the product, to the bit.
        Assert.True(slip.Toggle(1, Side.Home));
        Assert.True(slip.CombinedOdds
            == OddsMath.ParlayDecimal(new[] { m0.Odds(Side.Away), m1.Odds(Side.Home) }));

        Assert.True(slip.Toggle(1, Side.Home));   // un-click is seen, not served from the cache
        Assert.True(slip.CombinedOdds == m0.Odds(Side.Away));

        slip.Remove(0);
        Assert.Empty(slip.Picks);
        Assert.Equal(0.0, slip.ToWin);
        Assert.Equal("pick a side", slip.PlaceBlocker);
    }

    [Fact]
    public void Gate4_the_same_match_price_is_the_ENGINES_and_never_the_product_of_legs()
    {
        var run = new Run("sgp-slip-engine-price", Sandbox());
        Matchup m0 = run.CurrentSlate.Matchups[0];
        var slip = CheapSlip(run);

        MarketSelection a = MarketSelection.TotalGoals(2.5, true);
        MarketSelection b = MarketSelection.BothTeamsToScore(true);
        Assert.True(slip.AddLeg(0, a));
        Assert.True(slip.AddLeg(0, b));
        Assert.Null(slip.Refusal);

        SameMatchPrice pricing = slip.SameMatchPricing;
        Assert.NotNull(pricing);
        Assert.False(pricing.NaiveFallback);

        // The same figure under both names, and it IS the engine's joint price.
        Assert.True(slip.TicketOdds == pricing.Price);
        Assert.True(slip.CombinedOdds == pricing.Price);
        Assert.True(slip.ToWin == slip.Stake * pricing.Price);

        var legs = new[] { new Leg(m0, a, m0.Odds(a)), new Leg(m0, b, m0.Odds(b)) };
        SameMatchPrice engine = SameMatchModel.Price(legs, run.Config.Overround, run.Config.SgpMargin);
        Assert.True(slip.TicketOdds == engine.Price);

        // And it is NOT the product of the legs — the one number a SAME MATCH surface may never show.
        double product = OddsMath.ParlayDecimal(new[] { m0.Odds(a), m0.Odds(b) });
        Assert.True(slip.TicketOdds != product,
            $"correlated legs priced at the naive product {product:R}");

        // The placed contract carries the same joint pricing the preview read.
        Ticket placed = slip.Place();
        Assert.NotNull(placed.SameMatch);
        Assert.True(placed.SameMatch!.Price == engine.Price);
    }

    [Fact]
    public void Gate4_SameMatchPricing_states_ONE_principal_relation_for_a_multi_relation_ticket()
    {
        // The screen states one relation per slip and cannot choose which, so the model chooses.
        var run = new Run("sgp-slip-principal", Sandbox());
        Matchup m0 = run.CurrentSlate.Matchups[0];

        // Sourced from the matchup's OWN board, so every candidate is genuinely offered.
        MarketSelection[] candidates = m0.Markets
            .Select(o => o.Selection)
            .Where(s => s.Kind == MarketKind.TotalGoals
                     || s.Kind == MarketKind.BothTeamsToScore
                     || s.Kind == MarketKind.TotalCorners
                     || s.Kind == MarketKind.Moneyline
                     || s.Kind == MarketKind.TotalGoalsOddEven)
            .Take(10)
            .ToArray();
        Assert.True(candidates.Length >= 6, $"only {candidates.Length} candidate markets on the board");

        SameMatchPrice found = null!;
        for (int i = 0; i < candidates.Length && found == null; i++)
        for (int j = i + 1; j < candidates.Length && found == null; j++)
        for (int k = j + 1; k < candidates.Length && found == null; k++)
        {
            var slip = CheapSlip(run);
            Assert.True(slip.AddLeg(0, candidates[i]));
            Assert.True(slip.AddLeg(0, candidates[j]));
            Assert.True(slip.AddLeg(0, candidates[k]));

            SameMatchPrice p = slip.SameMatchPricing;
            if (slip.Refusal != null || p == null || p.NaiveFallback) continue;
            if (p.Relations.Count < 2 || p.Principal == null) continue;
            found = p;
        }

        Assert.NotNull(found);
        Assert.True(found.Relations.Count >= 2);
        Assert.NotNull(found.Principal);
        Assert.Contains(found.Principal!.Value, found.Relations);
    }

    [Fact]
    public void Gate4_the_priced_cache_tracks_the_slip_and_never_serves_a_stale_price()
    {
        // The price is now an engine call and is cached, while CombinedOdds is a property an
        // immediate-mode UI reads on every rebuild. The cache key must be the whole of what the
        // answer depends on, or the screen shows a number for a slip that no longer exists.
        var run = new Run("sgp-slip-cache", Sandbox());
        MarketSelection goals = MarketSelection.TotalGoals(2.5, true);
        var slip = CheapSlip(run);

        Assert.True(slip.AddLeg(0, goals));
        double single = slip.TicketOdds;
        Assert.True(slip.TicketOdds == single);                      // stable across reads
        Assert.True(single == run.CurrentSlate.Matchups[0].Odds(goals));

        Assert.True(slip.AddLeg(0, MarketSelection.BothTeamsToScore(true)));
        Assert.True(slip.TicketOdds != single);                      // it moved when the slip did
        Assert.NotNull(slip.SameMatchPricing);

        Assert.True(slip.RemoveLeg(1));
        Assert.True(slip.TicketOdds == single);                      // ... and moved back
        Assert.Null(slip.SameMatchPricing);

        // A NEW SLATE is part of the key: the same pick on next round's board re-prices.
        run.PlaceTicket(new[] { new Pick(0, Side.Home) }, run.Config.MinStake);
        AdvanceRound(run);
        Assert.True(slip.TicketOdds == run.CurrentSlate.Matchups[0].Odds(goals));
    }
}
