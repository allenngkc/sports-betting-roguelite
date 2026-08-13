using System;
using System.Collections.Generic;
using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// Phase 3 of F_0.6.0 — void re-pricing (<c>design/02-betting-math.md</c> § *Void: re-price on the
/// survivors*) and the duplicate-leg ban (§ *Impossible combinations are blocked, not priced*).
///
/// <para><c>Gate1_*</c> is the equivalence invariant carried onto the VOID path: an ordinary ticket
/// must still drop a voided leg out of its product, bit-for-bit, through the real Mulligan flow.
/// <c>Gate2_*</c> is the re-price itself, <c>Gate3_*</c> the duplicate ban and the sub-evens backstop
/// beside it, <c>Gate4_*</c> the voided-down-to-one-leg case.</para>
///
/// <para>Every void here goes through <c>Run.PlayMulliganSlip</c>. <c>Leg.IsVoided</c> has an
/// internal setter and the test assembly has no access to it, which is a feature: there is no way to
/// fake a void, so these tests exercise the only path the game has.</para>
/// </summary>
public class VoidRepricingTests
{
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public VoidRepricingTests(Xunit.Abstractions.ITestOutputHelper output) => _output = output;

    private static RunConfig SweepConfig() => new RunConfig
    {
        StartingBank = 1_000_000_000,
        MaxTicketsPerRound = 100_000,
    };

    private static ConsumableDefinition Mulligan()
        => RelicCatalog.Consumables.First(c => c.Id == "mulligan_slip");

    private static Leg LegFor(Matchup matchup, MarketSelection selection)
        => new Leg(matchup, selection, matchup.Odds(selection));

    private static MarketSelection[] Board(Matchup matchup)
    {
        var board = new MarketSelection[matchup.Markets.Count];
        for (int i = 0; i < board.Length; i++) board[i] = matchup.Markets[i].Selection;
        return board;
    }

    private static List<Matchup> Population(int seeds, int rounds)
    {
        var config = new RunConfig();
        var matchups = new List<Matchup>(seeds * rounds * config.MatchupsPerSlate);
        for (int seed = 0; seed < seeds; seed++)
        {
            var hub = new RngHub($"sgp-void-{seed}");
            for (int round = 0; round < rounds; round++)
                matchups.AddRange(SlateGenerator.Generate(round, hub, config).Matchups);
        }
        return matchups;
    }

    /// <summary>The pre-F_0.6.0 payout expression, transcribed — the same one Phase 2's Gate 1 uses.
    /// The invariant is that the engine still produces exactly this for any ticket with at most one
    /// leg per matchup, voided legs included.</summary>
    private static double OldPayout(Ticket ticket)
    {
        var odds = new List<double>();
        foreach (Leg leg in ticket.Legs) if (!leg.IsVoided) odds.Add(leg.OfferedOdds);
        return ticket.Stake * OddsMath.ParlayDecimal(odds) * ticket.PayoutMultiplier;
    }

    private static int Step(int state) => (state * 1103515245 + 12345) & 0x7FFFFFFF;

    /// <summary>Advances a session to its next pending-loss window. False once the session finishes
    /// without opening one. Never calls MoveNext while a window is open — that would decline it.</summary>
    private static bool StepToPendingLoss(SweatSession sweat)
    {
        while (!sweat.HasPendingLoss)
            if (!sweat.MoveNext(out _)) return false;
        return true;
    }

    private static int VoidedCount(Ticket ticket)
    {
        int n = 0;
        foreach (Leg leg in ticket.Legs) if (leg.IsVoided) n++;
        return n;
    }

    private static int VoidedIndex(Ticket ticket)
    {
        for (int i = 0; i < ticket.Legs.Count; i++) if (ticket.Legs[i].IsVoided) return i;
        return -1;
    }

    // =======================================================================================
    // EXIT GATE 1 — the equivalence invariant, on the void path.
    // =======================================================================================

    /// <summary>
    /// A ticket with at most one leg per matchup must VOID bit-identically to before F_0.6.0: the
    /// voided leg's offered odds simply leave the product. Not "within a tolerance" — <c>==</c>, over a
    /// swept population, through the real Mulligan Slip flow rather than a synthetic void.
    ///
    /// <para>The property is structural — such a ticket carries no <c>SameMatch</c> block, so it never
    /// reaches a locked replacement price and keeps reading its ACTIVE legs — and this sweep is the
    /// check on that routing. It asserts after the void AND after settlement, because settlement is
    /// where the number becomes money.</para>
    /// </summary>
    [Fact]
    public void Gate1_voiding_an_ordinary_ticket_is_bit_identical_through_the_real_mulligan_flow()
    {
        long voids = 0;
        long tickets = 0;
        long multiVoids = 0;

        for (int seed = 0; seed < 120; seed++)
        {
            var run = new Run($"sgp-void-ordinary-{seed}", SweepConfig());
            run.GrantConsumable(Mulligan());
            run.GrantConsumable(Mulligan());

            int slateSize = run.CurrentSlate.Matchups.Count;
            int state = Step(seed + 1);

            for (int shape = 0; shape < 3; shape++)
            {
                int legCount = 2 + (shape % 3);
                var picks = new List<Pick>(legCount);
                var used = new HashSet<int>();
                for (int i = 0; i < legCount; i++)
                {
                    state = Step(state);
                    int matchupIndex = state % slateSize;
                    for (int probe = 0; probe < slateSize && !used.Add(matchupIndex); probe++)
                        matchupIndex = (matchupIndex + 1) % slateSize;
                    Matchup matchup = run.CurrentSlate.Matchups[matchupIndex];
                    MarketSelection[] board = Board(matchup);
                    state = Step(state);
                    picks.Add(new Pick(matchupIndex, board[state % board.Length]));
                }
                if (picks.Count != used.Count) continue;

                run.PlaceTicket(picks, 10);
                tickets++;
            }

            if (run.Tickets.Count == 0) continue;
            run.LockRound();

            for (int i = 0; i < run.Sweats.Count; i++)
            {
                SweatSession sweat = run.Sweats[i];
                Ticket ticket = run.Tickets[i];
                Assert.Null(ticket.SameMatch);
                Assert.Empty(ticket.LockedVoidPrices);

                while (StepToPendingLoss(sweat))
                {
                    if (!sweat.CanMulliganPendingLoss
                        || !run.OwnedConsumables.Any(c => c.Id == "mulligan_slip"))
                    {
                        sweat.MoveNext(out _); // decline; the bust proceeds
                        break;
                    }

                    double before = ticket.PotentialPayout;
                    run.PlayMulliganSlip(sweat);
                    voids++;
                    if (VoidedCount(ticket) > 1) multiVoids++;

                    Assert.True(ticket.PotentialPayout == OldPayout(ticket),
                        $"payout drifted on void: {ticket.PotentialPayout:R} vs {OldPayout(ticket):R}");
                    Assert.True(ticket.PotentialPayout != before,
                        "a voided leg must leave the payout product");
                }
            }

            run.FastForwardRound();
            foreach (Ticket ticket in run.Tickets)
                Assert.True(ticket.PotentialPayout == OldPayout(ticket),
                    $"payout drifted at settlement: {ticket.PotentialPayout:R} vs {OldPayout(ticket):R}");
        }

        _output.WriteLine($"{voids} real mulligan voids over {tickets} ordinary tickets "
            + $"({multiVoids} of them a SECOND void on one ticket) — all exact.");
        Assert.True(voids > 30, $"sweep too thin to mean anything: {voids} voids");
        Assert.True(multiVoids > 0,
            "an ordinary ticket must still accept more than one void — that path is untouched");
    }

    // =======================================================================================
    // EXIT GATE 2 — the re-price itself.
    // =======================================================================================

    /// <summary>
    /// A SAME MATCH ticket re-prices against the SURVIVORS' joint when a leg voids, at the figure
    /// locked when the ticket locked:
    /// <c>o' = 1 / (p_joint(survivors) × κ × (1 + Ω)^(n−1))</c>.
    ///
    /// <para>The seed search insists the survivors are still CORRELATED, which is what makes the test
    /// discriminating: for a correlated remainder the re-price is a different number from the old
    /// behaviour of dropping the voided leg's factor out of a product, so this cannot pass by
    /// coincidence. Note n drops with the leg — the ticket pays one fewer leg of margin — and the
    /// expected figure is recomputed here from <c>JointModel</c> rather than read back off the
    /// ticket.</para>
    /// </summary>
    [Fact]
    public void Gate2_a_same_match_ticket_re_prices_on_the_survivors_joint_when_a_leg_voids()
    {
        var config = SweepConfig();
        MarketSelection[] picked =
        {
            MarketSelection.TotalGoals(1.5, true),
            MarketSelection.BothTeamsToScore(true),
            MarketSelection.TotalCorners(9.5, true),
        };

        for (int seed = 0; seed < 400; seed++)
        {
            var run = new Run($"sgp-void-same-{seed}", config);
            run.GrantConsumable(Mulligan());
            Matchup matchup = run.CurrentSlate.Matchups[0];

            Ticket ticket = run.PlaceTicket(picked.Select(s => new Pick(0, s)).ToArray(), 10);
            Assert.NotNull(ticket.SameMatch);
            Assert.Equal(3, ticket.LockedVoidPrices.Count);

            // Snapshot BEFORE the round even locks: this is the "locked at ticket lock, never
            // re-derived at settlement" claim, and it is only a claim if it is captured up front.
            double[] lockedAtPlacement = ticket.LockedVoidPrices.ToArray();

            run.LockRound();
            SweatSession sweat = run.Sweats[0];
            if (!StepToPendingLoss(sweat)) continue;

            int dead = sweat.PendingDeadLegIndex;
            MarketSelection[] survivors = picked.Where((_, i) => i != dead).ToArray();

            double pSurvivors = JointModel.JointProbability(matchup, survivors).pJoint;
            double naiveSurvivorProduct = survivors.Aggregate(1.0, (p, s) => p * matchup.Odds(s));
            double rho = pSurvivors / survivors.Aggregate(1.0, (p, s) => p * matchup.TrueProb(s));
            if (Math.Abs(rho - 1.0) < 1e-9) continue; // insist on a correlated remainder

            run.PlayMulliganSlip(sweat);
            Assert.True(ticket.Legs[dead].IsVoided);

            // n - 1: margin is charged once per SURVIVING leg, so it drops with the leg.
            double expected = 1.0 / (pSurvivors * config.SgpMargin
                * Math.Pow(1.0 + config.Overround, picked.Length - 1));

            Assert.True(lockedAtPlacement[dead] == expected,
                $"replacement price {lockedAtPlacement[dead]:R} != {expected:R}");
            Assert.True(ticket.LockedVoidPrices[dead] == lockedAtPlacement[dead],
                "the replacement price was re-derived after placement");
            Assert.True(ticket.SameMatchContractPrice == lockedAtPlacement[dead],
                "the ticket did not re-price onto its locked replacement");
            Assert.True(ticket.PotentialPayout == 10 * lockedAtPlacement[dead] * ticket.PayoutMultiplier,
                $"payout {ticket.PotentialPayout:R} is not the re-priced figure");

            // It is a REAL re-price: neither the old "leave the locked price alone" behaviour, nor
            // the product path's "drop the leg out and multiply the rest".
            Assert.True(ticket.SameMatchContractPrice != ticket.LockedPrice,
                "the price must move when the joint event changes");
            Assert.True(Math.Abs(ticket.SameMatchContractPrice - naiveSurvivorProduct) > 1e-9,
                $"a correlated remainder must not price at the survivors' product (rho = {rho:0.0000})");

            _output.WriteLine($"seed {seed}: leg {dead} voided; {ticket.LockedPrice:0.0000} -> "
                + $"{ticket.SameMatchContractPrice:0.0000} (survivors' product would be "
                + $"{naiveSurvivorProduct:0.0000}, rho = {rho:0.0000})");
            return;
        }

        Assert.Fail("no seed produced a same-match void with a correlated remainder");
    }

    /// <summary>
    /// Every single-void replacement price the shipped board can produce obeys the formula, swept.
    /// The expectation is rebuilt from <c>JointModel</c> on the survivor set, so this pins the whole
    /// table rather than the one scenario a sweat happened to reach — which is what "one per
    /// single-void scenario, locked at ticket lock" means.
    ///
    /// <para>It also reports the tightest replacement price on the board. Canon's evens refusal is a
    /// PLACEMENT rule and says nothing about a price a void produces, so this measures whether the
    /// gap is reachable rather than assuming it is not.</para>
    /// </summary>
    [Fact]
    public void Void_replacement_prices_cover_every_single_void_scenario_and_match_the_formula()
    {
        var config = new RunConfig();
        long scenarios = 0;
        double worst = 0.0;
        double tightest = double.MaxValue;
        string tightestAt = "none";

        foreach (Matchup matchup in Population(4, 1))
        {
            MarketSelection[] board = Board(matchup);
            for (int i = 0; i < board.Length; i++)
                for (int j = i + 1; j < board.Length; j++)
                {
                    var selections = new List<MarketSelection> { board[i], board[j] };
                    if ((i + j) % 5 == 0) selections.Add(board[(i + j + 7) % board.Length]);
                    if (selections.Distinct().Count() != selections.Count) continue;

                    var legs = selections.Select(s => LegFor(matchup, s)).ToList();
                    SameMatchPrice price = SameMatchModel.Price(legs, config.Overround, config.SgpMargin);

                    if (price.Impossible)
                    {
                        Assert.Empty(price.VoidPrices); // refused, so it never reaches a void
                        continue;
                    }

                    Assert.Equal(legs.Count, price.VoidPrices.Count);
                    for (int v = 0; v < legs.Count; v++)
                    {
                        MarketSelection[] survivors = selections.Where((_, k) => k != v).ToArray();
                        double pSurvivors = JointModel.JointProbability(matchup, survivors).pJoint;
                        double expected = 1.0 / (pSurvivors * config.SgpMargin
                            * Math.Pow(1.0 + config.Overround, legs.Count - 1));

                        worst = Math.Max(worst, Math.Abs(price.VoidPrices[v] - expected));
                        scenarios++;
                        if (price.VoidPrices[v] >= tightest) continue;
                        tightest = price.VoidPrices[v];
                        tightestAt = string.Join(" + ", survivors.Select(s => MatchModel.DisplayLabel(matchup, s)));
                    }
                }
        }

        _output.WriteLine($"{scenarios} single-void scenarios priced; worst |o' - formula| = {worst:E3}");
        _output.WriteLine($"tightest replacement price: {tightest:0.0000} on {tightestAt}");
        Assert.True(scenarios > 10_000, $"sweep too thin: {scenarios} scenarios");
        Assert.True(worst == 0.0, $"a replacement price drifted from the formula by {worst:E3}");
        Assert.True(tightest > 1.0,
            $"a void re-priced a ticket to {tightest:0.0000} <= evens — canon has no rule for that");
    }

    /// <summary>A voided leg's own Profit Boost dies with it, and a surviving leg's boost carries into
    /// the replacement price — exactly what the ordinary path does, where the boosted odds are simply
    /// in or out of the product. Canon does not cover the interaction; this pins the choice so it is
    /// visible rather than incidental.</summary>
    [Fact]
    public void A_profit_boost_carries_into_the_replacement_price_unless_its_own_leg_voids()
    {
        var config = SweepConfig();
        Pick[] picks =
        {
            new Pick(0, MarketSelection.TotalGoals(1.5, true)),
            new Pick(0, MarketSelection.BothTeamsToScore(true)),
        };

        var plain = new Run("sgp-void-boost", config);
        var boosted = new Run("sgp-void-boost", config);
        boosted.GrantConsumable(RelicCatalog.Consumables.First(c => c.Id == "profit_boost"));

        Ticket a = plain.PlaceTicket(picks, 10);
        Ticket b = boosted.PlaceTicket(picks, 10, profitBoostLeg: 0);

        // Leg 1 voids: leg 0 (the boosted one) survives, so the boost survives with it.
        Assert.True(b.LockedVoidPrices[1] == a.LockedVoidPrices[1] * RelicCatalog.ProfitBoostMult,
            $"{b.LockedVoidPrices[1]:R} != {a.LockedVoidPrices[1]:R} x {RelicCatalog.ProfitBoostMult:R}");

        // Leg 0 voids: the boosted leg is the one struck out, so the boost goes with it.
        Assert.True(b.LockedVoidPrices[0] == a.LockedVoidPrices[0],
            $"a voided leg's boost outlived it: {b.LockedVoidPrices[0]:R} vs {a.LockedVoidPrices[0]:R}");
    }

    // =======================================================================================
    // EXIT GATE 3 — multiple voids fail loudly; the duplicate ban; the evens backstop.
    // =======================================================================================

    /// <summary>
    /// Canon covers a SINGLE void — "the one documented commercial mechanism covers a single void
    /// only" — and leaves multiple simultaneous voids OPEN. A second void on a SAME MATCH ticket must
    /// therefore fail loudly rather than silently settle at something invented, and it must cost the
    /// player nothing: the refusal lands before the second Slip is consumed.
    /// </summary>
    [Fact]
    public void Gate3_a_second_void_on_a_same_match_ticket_fails_loudly_and_costs_nothing()
    {
        var config = SweepConfig();
        Pick[] picks =
        {
            new Pick(0, MarketSelection.TotalGoals(3.5, true)),
            new Pick(0, MarketSelection.TotalCorners(10.5, true)),
            new Pick(0, MarketSelection.TotalCards(5.5, true)),
        };

        for (int seed = 0; seed < 400; seed++)
        {
            var run = new Run($"sgp-multivoid-{seed}", config);
            run.GrantConsumable(Mulligan());
            run.GrantConsumable(Mulligan());

            Ticket ticket = run.PlaceTicket(picks, 10);
            Assert.NotNull(ticket.SameMatch);

            run.LockRound();
            SweatSession sweat = run.Sweats[0];

            if (!StepToPendingLoss(sweat)) continue;
            run.PlayMulliganSlip(sweat);
            Assert.Equal(1, VoidedCount(ticket));
            if (!StepToPendingLoss(sweat)) continue;

            double pricedOnOneVoid = ticket.SameMatchContractPrice;
            int stillVoided = VoidedIndex(ticket);

            NotSupportedException thrown =
                Assert.Throws<NotSupportedException>(() => run.PlayMulliganSlip(sweat));
            Assert.Contains("SINGLE", thrown.Message);

            // Nothing moved: the second Slip is still held, the leg is still un-voided, and the
            // ticket still prices at its ONE-void replacement.
            Assert.Single(run.OwnedConsumables, c => c.Id == "mulligan_slip");
            Assert.Equal(1, VoidedCount(ticket));
            Assert.Equal(stillVoided, VoidedIndex(ticket));
            Assert.True(ticket.SameMatchContractPrice == pricedOnOneVoid);

            _output.WriteLine($"seed {seed}: second void on a same-match ticket refused, slip retained.");
            return;
        }

        Assert.Fail("no seed produced a same-match ticket with two dead legs");
    }

    /// <summary>A selection may not appear twice on a ticket (design/02, ruled 2026-08-12). The repeat
    /// adds no risk — the joint is idempotent — while (1 + Ω)^n charges a full extra leg of margin for
    /// it. The refusal is legible and atomic: no ticket, no stake, and a held Profit Boost is still
    /// held afterwards.</summary>
    [Fact]
    public void Gate3_a_selection_may_not_appear_twice_on_a_ticket()
    {
        var config = SweepConfig();
        var run = new Run("sgp-duplicate", config);
        run.GrantConsumable(RelicCatalog.Consumables.First(c => c.Id == "profit_boost"));

        var repeated = MarketSelection.TotalGoals(2.5, true);
        Pick[] picks = { new Pick(0, repeated), new Pick(0, repeated) };

        ArgumentException thrown = Assert.Throws<ArgumentException>(
            () => run.PlaceTicket(picks, 10, profitBoostLeg: 0));
        Assert.Contains("may not appear twice", thrown.Message);

        Assert.Empty(run.Tickets);
        Assert.Equal(config.StartingBank, run.Bank);
        Assert.Single(run.OwnedConsumables, c => c.Id == "profit_boost");

        // The SAME selection on a DIFFERENT matchup is a different selection, and still sellable.
        Ticket ok = run.PlaceTicket(new[] { new Pick(0, repeated), new Pick(1, repeated) }, 10);
        Assert.Null(ok.SameMatch);

        // A repeat buried inside a longer same-match ticket is caught too.
        Assert.Throws<ArgumentException>(() => run.PlaceTicket(new[]
        {
            new Pick(0, MarketSelection.TotalCorners(9.5, true)),
            new Pick(0, repeated),
            new Pick(0, repeated),
        }, 10));
    }

    /// <summary>
    /// The duplicate ban and the sub-evens refusal are INDEPENDENT rules and canon keeps both. This is
    /// the case that proves the evens check does not subsume the duplicate ban: two repeats of a LONG
    /// leg price far above evens, so the price check waves them through while the ticket is still a
    /// pure ripoff — double margin for identical risk.
    /// </summary>
    [Fact]
    public void Gate3_the_evens_check_does_not_subsume_the_duplicate_ban()
    {
        var config = SweepConfig();
        var run = new Run("sgp-long-repeat", config);
        Matchup matchup = run.CurrentSlate.Matchups[0];

        // The longest market on the matchup: the one furthest from tripping the evens check.
        MarketSelection longest = Board(matchup)
            .OrderBy(s => matchup.TrueProb(s)).First();

        var legs = new[] { LegFor(matchup, longest), LegFor(matchup, longest) };
        SameMatchPrice priced = SameMatchModel.Price(legs, config.Overround, config.SgpMargin);
        double single = matchup.Odds(longest);

        _output.WriteLine($"2 x {MatchModel.DisplayLabel(matchup, longest)} "
            + $"(p = {matchup.TrueProb(longest):0.0000}) would price at {priced.Price:0.000}, "
            + $"a single pays {single:0.000}");

        // Far above evens — the price check has no objection at all.
        Assert.True(priced.Price > 1.0);
        Assert.True(priced.Price > 10.0, $"expected a long leg, got {priced.Price:0.000}");

        // And identical risk: P(A and A) = P(A), so the repeat buys nothing while paying margin.
        Assert.Equal(matchup.TrueProb(longest), priced.PTicket, 12);
        Assert.True(priced.Price < single,
            "the repeat pays LESS than the single it duplicates — the ripoff, in one line");

        Assert.Throws<ArgumentException>(() => run.PlaceTicket(
            new[] { new Pick(0, longest), new Pick(0, longest) }, 10));
    }

    /// <summary>The sub-evens refusal survives the duplicate ban and still fires on DISTINCT
    /// selections. κ is the reachable lever — the gate campaign tunes it and canon only requires
    /// κ ≥ 1 — so this pins the backstop without relying on a repeat leg to reach it.</summary>
    [Fact]
    public void Gate3_the_sub_evens_refusal_still_fires_on_distinct_selections()
    {
        var config = SweepConfig();
        config.SgpMargin = 8.0;
        var run = new Run("sgp-subevens", config);

        Pick[] picks =
        {
            new Pick(0, MarketSelection.TotalGoals(1.5, true)),
            new Pick(0, MarketSelection.BothTeamsToScore(true)),
        };

        ArgumentException thrown = Assert.Throws<ArgumentException>(() => run.PlaceTicket(picks, 10));
        Assert.Contains("cannot be offered", thrown.Message);
        Assert.Empty(run.Tickets);
        Assert.Equal(config.StartingBank, run.Bank);
    }

    // =======================================================================================
    // EXIT GATE 4 — voided down to a single leg.
    // =======================================================================================

    /// <summary>
    /// n − 1 = 1 is a real case, not an edge to skip: a two-leg SAME MATCH ticket mulliganed down to
    /// one surviving leg. The formula still holds, and at κ = 1 it lands exactly on the board's own
    /// price for that single — <c>1/(p × (1+Ω))</c> is what <c>MatchModel.Offer</c> quotes — so a
    /// ticket voided down to one leg pays precisely what backing that leg alone would have paid.
    /// Asserted with <c>==</c>, because the two expressions agreeing to the last bit is the claim.
    /// </summary>
    [Fact]
    public void Gate4_a_same_match_ticket_voided_down_to_one_leg_prices_as_that_single()
    {
        var config = SweepConfig();
        MarketSelection[] picked =
        {
            MarketSelection.TotalGoals(1.5, true),
            MarketSelection.BothTeamsToScore(true),
        };

        for (int seed = 0; seed < 400; seed++)
        {
            var run = new Run($"sgp-void-single-{seed}", config);
            run.GrantConsumable(Mulligan());
            Matchup matchup = run.CurrentSlate.Matchups[0];

            Ticket ticket = run.PlaceTicket(picked.Select(s => new Pick(0, s)).ToArray(), 10);
            Assert.NotNull(ticket.SameMatch);
            double lockedPrice = ticket.LockedPrice;

            run.LockRound();
            SweatSession sweat = run.Sweats[0];
            if (!StepToPendingLoss(sweat)) continue;

            int dead = sweat.PendingDeadLegIndex;
            Assert.True(sweat.CanMulliganPendingLoss,
                "a two-leg ticket must still be mulliganable down to one leg");
            run.PlayMulliganSlip(sweat);

            MarketSelection survivor = picked[1 - dead];
            Assert.Equal(1, ticket.Legs.Count(l => !l.IsVoided));

            double expected = 1.0 / (matchup.TrueProb(survivor) * config.SgpMargin
                * Math.Pow(1.0 + config.Overround, 1));
            Assert.True(ticket.SameMatchContractPrice == expected,
                $"{ticket.SameMatchContractPrice:R} != {expected:R}");
            Assert.True(ticket.SameMatchContractPrice == matchup.Odds(survivor),
                $"voided down to one leg, the ticket must pay the board's own single: "
                + $"{ticket.SameMatchContractPrice:R} vs {matchup.Odds(survivor):R}");
            Assert.True(ticket.PotentialPayout == 10 * matchup.Odds(survivor) * ticket.PayoutMultiplier);
            Assert.NotEqual(lockedPrice, ticket.SameMatchContractPrice);

            // And it SETTLES on that figure. Written as `bank + payout` rather than a subtraction
            // because that is literally the engine's own `Bank += PotentialPayout`, and a nine-figure
            // sweep bank cannot represent the difference of a two-figure payout exactly.
            double bank = run.Bank;
            run.FastForwardRound();
            if (ticket.State == TicketState.Won)
                Assert.True(run.Bank == bank + 10 * matchup.Odds(survivor) * ticket.PayoutMultiplier,
                    "settlement credited something other than the re-priced payout");
            Assert.True(ticket.PotentialPayout == 10 * matchup.Odds(survivor) * ticket.PayoutMultiplier,
                "the re-priced figure did not survive settlement");

            _output.WriteLine($"seed {seed}: 2-leg same-match voided to 1 leg; "
                + $"{lockedPrice:0.0000} -> {ticket.SameMatchContractPrice:0.0000} "
                + $"(= the board's single, {matchup.Odds(survivor):0.0000}); ticket {ticket.State}");
            return;
        }

        Assert.Fail("no seed voided a two-leg same-match ticket down to one leg");
    }
}
