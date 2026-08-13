using System;
using System.Collections.Generic;
using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// Phase 3 of F_0.6.0 — void re-pricing (<c>design/02-betting-math.md</c> § *Void: re-price on the
/// survivors*) and the duplicate-leg ban (§ *Impossible combinations are blocked, not priced*).
///
/// <para><c>Gate1_*</c> is the equivalence invariant carried onto the VOID path — an ordinary ticket
/// must still drop a voided leg out of its product, bit-for-bit, through the real Mulligan flow — and
/// the multi-void hands the survivor-subset table now supports. <c>Gate2_*</c> is the re-price itself
/// and the at-or-below-evens floor beside it, <c>Gate3_*</c> the duplicate ban and the placement-time
/// sub-evens refusal, <c>Gate4_*</c> the voided-down-to-one-leg case.</para>
///
/// <para><b>Amended 2026-08-12</b> for the three rulings: multiple voids are supported (the earlier
/// OPEN is closed — every survivor subset is priced at lock, not just the single-void row), a
/// replacement price floors at 1.0 and pays the stake back, and a Profit Boost travels with its
/// leg.</para>
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

    /// <summary>The ticket's SURVIVOR bitmask — the index into the locked subset table.</summary>
    private static int SurvivorMask(Ticket ticket)
    {
        int mask = 0;
        for (int i = 0; i < ticket.Legs.Count; i++) if (!ticket.Legs[i].IsVoided) mask |= 1 << i;
        return mask;
    }

    /// <summary>Canon's replacement formula, rebuilt from <c>JointModel</c>:
    /// <c>1 / (p_joint(survivors) × κ × (1 + Ω)^|survivors|)</c>.</summary>
    private static double FormulaPrice(Matchup matchup, MarketSelection[] survivors, RunConfig config)
        => 1.0 / (JointModel.JointProbability(matchup, survivors).pJoint * config.SgpMargin
            * Math.Pow(1.0 + config.Overround, survivors.Length));

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
        int deepest = 0;

        for (int seed = 0; seed < 120; seed++)
        {
            var run = new Run($"sgp-void-ordinary-{seed}", SweepConfig());
            // Six Slips, not two: the ordinary path accepts any number of voids and always has, so the
            // sweep must be able to drive one ticket to THREE dead legs rather than stopping at two.
            for (int i = 0; i < 6; i++) run.GrantConsumable(Mulligan());

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
                Assert.Empty(ticket.LockedSubsetPrices);

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
                    deepest = Math.Max(deepest, VoidedCount(ticket));

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
            + $"({multiVoids} of them a second-or-later void on one ticket; deepest = {deepest} "
            + "voids on a single ticket) — all exact.");
        Assert.True(voids > 30, $"sweep too thin to mean anything: {voids} voids");
        Assert.True(multiVoids > 0,
            "an ordinary ticket must still accept more than one void — that path is untouched");
        Assert.True(deepest >= 3,
            $"the sweep never drove a ticket past {deepest} voids, so it does not test the depth "
            + "the same-match path now supports");
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
    /// single-void scenario, locked at ticket lock" means. (The whole table, multi-void rows included,
    /// is swept by <see cref="Every_survivor_subset_is_priced_at_lock_and_matches_the_formula"/>.)
    ///
    /// <para>It also reports the tightest replacement price on the board. Canon's evens refusal is a
    /// PLACEMENT rule and says nothing about a price a void produces; measuring the gap here is what
    /// established that the shipped board bottoms out at ~1.118 at κ = 1, which is the figure the
    /// 2026-08-12 floor ruling is calibrated against.</para>
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
        // At the shipped κ = 1 the floor never binds — this is the headroom measurement, and if it
        // ever stops holding the default dial has moved into territory canon prices at the stake.
        Assert.True(tightest > 1.0,
            $"a void re-priced a ticket to {tightest:0.0000} <= evens at the DEFAULT kappa; the floor "
            + "is meant to be a κ-campaign safeguard, not the shipped board's normal behaviour");
    }

    /// <summary>A voided leg's own Profit Boost dies with it, and a surviving leg's boost carries into
    /// the replacement price — exactly what the ordinary path does, where the boosted odds are simply
    /// in or out of the product. Canon states this as of 2026-08-12 ("a Profit Boost travels with its
    /// leg"); this is the pin.
    ///
    /// <para>Asserted across the ENTIRE subset table of a three-leg ticket, not just the single-void
    /// row: with multiple voids the boosted leg can be struck out first, last, or alongside another,
    /// and the rule has to be the same in all fifteen readings. The test is exhaustive over the
    /// table — for every survivor set the boosted price is the plain price times the factor exactly
    /// when the boosted leg's bit is set.</para></summary>
    [Fact]
    public void A_profit_boost_carries_into_the_replacement_price_unless_its_own_leg_voids()
    {
        var config = SweepConfig();
        Pick[] picks =
        {
            new Pick(0, MarketSelection.TotalGoals(1.5, true)),
            new Pick(0, MarketSelection.BothTeamsToScore(true)),
            new Pick(0, MarketSelection.TotalCorners(9.5, true)),
        };

        for (int boostLeg = 0; boostLeg < picks.Length; boostLeg++)
        {
            var plain = new Run("sgp-void-boost", config);
            var boosted = new Run("sgp-void-boost", config);
            boosted.GrantConsumable(RelicCatalog.Consumables.First(c => c.Id == "profit_boost"));

            Ticket a = plain.PlaceTicket(picks, 10);
            Ticket b = boosted.PlaceTicket(picks, 10, profitBoostLeg: boostLeg);

            int full = (1 << picks.Length) - 1;
            Assert.Equal(full + 1, a.LockedSubsetPrices.Count);

            for (int mask = 1; mask <= full; mask++)
            {
                bool boostSurvives = (mask & (1 << boostLeg)) != 0;
                double expected = a.LockedSubsetPrices[mask]
                    * (boostSurvives ? RelicCatalog.ProfitBoostMult : 1.0);
                Assert.True(b.LockedSubsetPrices[mask] == expected,
                    $"boost on leg {boostLeg}, survivor mask 0x{mask:X}: "
                    + $"{b.LockedSubsetPrices[mask]:R} != {expected:R}");
            }

            // The single-void row inherits it, which is the pre-multi-void assertion, unchanged.
            Assert.True(b.LockedVoidPrices[boostLeg] == a.LockedVoidPrices[boostLeg],
                "a voided leg's boost outlived it");
        }
    }

    // =======================================================================================
    // EXIT GATE 1 (multi-void) — a second and third void are the same lookup as the first.
    // =======================================================================================

    /// <summary>
    /// TWO voids on a three-leg SAME MATCH ticket re-price from the locked subset table, with no
    /// exception anywhere (<c>design/02</c> § *Void: re-price on the survivors* — multiple voids CLOSED
    /// 2026-08-12, reversing the earlier OPEN). Two Mulligan Slips on a three-leg ticket is an ordinary
    /// hand, and this is the test that says so.
    ///
    /// <para>The whole subset table is snapshotted BEFORE the round even locks, and every assertion
    /// below compares against that snapshot: "computed once at lock, never re-derived at settlement" is
    /// only a claim if the numbers are captured up front. The expected figure is independently rebuilt
    /// from <c>JointModel</c> on the survivor, so the table cannot pass by agreeing with itself.</para>
    /// </summary>
    [Fact]
    public void Gate1_two_voids_on_a_same_match_ticket_re_price_from_the_locked_subset_table()
    {
        var config = SweepConfig();
        MarketSelection[] picked =
        {
            MarketSelection.TotalGoals(3.5, true),
            MarketSelection.TotalCorners(10.5, true),
            MarketSelection.TotalCards(5.5, true),
        };

        for (int seed = 0; seed < 400; seed++)
        {
            var run = new Run($"sgp-multivoid-{seed}", config);
            run.GrantConsumable(Mulligan());
            run.GrantConsumable(Mulligan());
            Matchup matchup = run.CurrentSlate.Matchups[0];

            Ticket ticket = run.PlaceTicket(picked.Select(s => new Pick(0, s)).ToArray(), 10);
            Assert.NotNull(ticket.SameMatch);
            Assert.Equal(1 << 3, ticket.LockedSubsetPrices.Count);
            double[] lockedAtPlacement = ticket.LockedSubsetPrices.ToArray();

            run.LockRound();
            SweatSession sweat = run.Sweats[0];

            if (!StepToPendingLoss(sweat)) continue;
            run.PlayMulliganSlip(sweat);
            Assert.Equal(1, VoidedCount(ticket));
            if (!StepToPendingLoss(sweat)) continue;

            double pricedOnOneVoid = ticket.SameMatchContractPrice;

            // THE SECOND VOID. No throw, and the Slip is spent like any other.
            run.PlayMulliganSlip(sweat);
            Assert.Equal(2, VoidedCount(ticket));
            Assert.DoesNotContain(run.OwnedConsumables, c => c.Id == "mulligan_slip");

            int survivorMask = SurvivorMask(ticket);
            MarketSelection[] survivors = picked
                .Where((_, i) => (survivorMask & (1 << i)) != 0).ToArray();
            Assert.Single(survivors);

            double expected = FormulaPrice(matchup, survivors, config);
            Assert.True(lockedAtPlacement[survivorMask] == expected,
                $"two-void replacement {lockedAtPlacement[survivorMask]:R} != {expected:R}");
            Assert.True(ticket.LockedSubsetPrices[survivorMask] == lockedAtPlacement[survivorMask],
                "the two-void replacement price was re-derived after placement");
            Assert.True(ticket.SameMatchContractPrice == lockedAtPlacement[survivorMask],
                "the ticket did not re-price onto its locked two-void replacement");
            Assert.True(ticket.PotentialPayout == 10 * lockedAtPlacement[survivorMask] * ticket.PayoutMultiplier,
                $"payout {ticket.PotentialPayout:R} is not the twice-voided figure");
            Assert.True(ticket.SameMatchContractPrice != pricedOnOneVoid,
                "the price must move again on the second void");

            // And it survives settlement — where the number becomes money.
            double atSweat = ticket.PotentialPayout;
            run.FastForwardRound();
            Assert.True(ticket.PotentialPayout == atSweat,
                "the twice-voided figure did not survive settlement");

            _output.WriteLine($"seed {seed}: two voids; {ticket.LockedPrice:0.0000} -> "
                + $"{pricedOnOneVoid:0.0000} -> {ticket.SameMatchContractPrice:0.0000} "
                + $"(survivor mask 0x{survivorMask:X}); ticket {ticket.State}");
            return;
        }

        Assert.Fail("no seed produced a same-match ticket with two dead legs");
    }

    /// <summary>
    /// THREE voids on a four-leg SAME MATCH ticket — the deepest hand <c>MaxLegs = 4</c> allows, and
    /// the one the single-void table could not have priced at all. It lands on the survivor's own
    /// subset entry, which at κ = 1 is the board's price for that single.
    /// </summary>
    [Fact]
    public void Gate1_three_voids_on_a_four_leg_same_match_ticket_price_down_to_the_last_survivor()
    {
        var config = SweepConfig();
        MarketSelection[] picked =
        {
            MarketSelection.TotalGoals(3.5, true),
            MarketSelection.TotalCorners(10.5, true),
            MarketSelection.TotalCards(5.5, true),
            MarketSelection.BothTeamsToScore(true),
        };

        for (int seed = 0; seed < 600; seed++)
        {
            var run = new Run($"sgp-triplevoid-{seed}", config);
            for (int i = 0; i < 3; i++) run.GrantConsumable(Mulligan());
            Matchup matchup = run.CurrentSlate.Matchups[0];

            Ticket ticket = run.PlaceTicket(picked.Select(s => new Pick(0, s)).ToArray(), 10);
            Assert.NotNull(ticket.SameMatch);
            Assert.Equal(1 << 4, ticket.LockedSubsetPrices.Count); // 16 slots, 15 live prices
            double[] lockedAtPlacement = ticket.LockedSubsetPrices.ToArray();

            run.LockRound();
            SweatSession sweat = run.Sweats[0];

            var trail = new List<double> { ticket.LockedPrice };
            while (VoidedCount(ticket) < 3 && StepToPendingLoss(sweat))
            {
                if (!sweat.CanMulliganPendingLoss) break;
                run.PlayMulliganSlip(sweat);
                trail.Add(ticket.SameMatchContractPrice);
            }
            if (VoidedCount(ticket) < 3) continue;

            int survivorMask = SurvivorMask(ticket);
            MarketSelection[] survivors = picked
                .Where((_, i) => (survivorMask & (1 << i)) != 0).ToArray();
            Assert.Single(survivors);

            double expected = FormulaPrice(matchup, survivors, config);
            Assert.True(lockedAtPlacement[survivorMask] == expected,
                $"three-void replacement {lockedAtPlacement[survivorMask]:R} != {expected:R}");
            Assert.True(ticket.SameMatchContractPrice == expected,
                "the ticket did not re-price onto its locked three-void replacement");

            // At the shipped κ = 1 the last survivor pays exactly what backing it alone would have.
            Assert.True(config.SgpMargin == 1.0);
            Assert.True(ticket.SameMatchContractPrice == matchup.Odds(survivors[0]),
                $"{ticket.SameMatchContractPrice:R} vs the board's single {matchup.Odds(survivors[0]):R}");
            Assert.True(ticket.PotentialPayout == 10 * matchup.Odds(survivors[0]) * ticket.PayoutMultiplier);

            _output.WriteLine($"seed {seed}: three voids; price trail "
                + string.Join(" -> ", trail.Select(p => p.ToString("0.0000"))));
            return;
        }

        Assert.Fail("no seed produced a four-leg same-match ticket with three dead legs");
    }

    /// <summary>
    /// The whole subset table, swept: for every SAME MATCH leg set the shipped board can build, every
    /// one of the <c>2^n − 1</c> survivor subsets must equal
    /// <c>1 / (p_joint(survivors) × κ × (1 + Ω)^|survivors|)</c>, rebuilt independently from
    /// <c>JointModel</c>. This is what "price every subset at lock" means, and it covers the two- and
    /// three-void scenarios arithmetically over a population rather than at the one seed a sweat
    /// happened to reach.
    ///
    /// <para>It also pins the table's shape (the full mask IS the ticket's own price, the single-void
    /// row is a view over it) and reports the tightest replacement anywhere on the board at κ = 1 —
    /// the figure canon's floor ruling is calibrated against.</para>
    /// </summary>
    [Fact]
    public void Every_survivor_subset_is_priced_at_lock_and_matches_the_formula()
    {
        var config = new RunConfig();
        long subsets = 0;
        double worst = 0.0;
        double tightest = double.MaxValue;
        string tightestAt = "none";

        foreach (Matchup matchup in Population(2, 1))
        {
            MarketSelection[] board = Board(matchup);
            for (int i = 0; i < board.Length; i++)
                for (int j = i + 1; j < board.Length; j++)
                {
                    var selections = new List<MarketSelection> { board[i], board[j] };
                    if ((i + j) % 5 == 0) selections.Add(board[(i + j + 7) % board.Length]);
                    if ((i + j) % 11 == 0) selections.Add(board[(i + j + 3) % board.Length]);
                    if (selections.Distinct().Count() != selections.Count) continue;

                    var legs = selections.Select(s => LegFor(matchup, s)).ToList();
                    SameMatchPrice price = SameMatchModel.Price(legs, config.Overround, config.SgpMargin);

                    if (price.Impossible)
                    {
                        Assert.Empty(price.SubsetPrices); // refused, so it never reaches a void
                        Assert.Empty(price.VoidPrices);
                        continue;
                    }

                    int full = (1 << legs.Count) - 1;
                    Assert.Equal(full + 1, price.SubsetPrices.Count);
                    Assert.True(price.SubsetPrices[0] == 0.0, "slot 0 is the unreachable empty survivor set");
                    Assert.True(price.SubsetPrices[full] == price.Price,
                        "the full mask must BE the ticket's own price, to the bit");

                    // The single-void row is a view over the table, not a second source of truth.
                    Assert.Equal(legs.Count, price.VoidPrices.Count);
                    for (int v = 0; v < legs.Count; v++)
                        Assert.True(price.VoidPrices[v] == price.SubsetPrices[full & ~(1 << v)],
                            "the single-void row drifted from the subset table");

                    for (int mask = 1; mask < full; mask++)
                    {
                        MarketSelection[] survivors = selections
                            .Where((_, k) => (mask & (1 << k)) != 0).ToArray();
                        double pSurvivors = JointModel.JointProbability(matchup, survivors).pJoint;
                        double expected = 1.0 / (pSurvivors * config.SgpMargin
                            * Math.Pow(1.0 + config.Overround, survivors.Length));

                        worst = Math.Max(worst, Math.Abs(price.SubsetPrices[mask] - expected));
                        subsets++;
                        if (price.SubsetPrices[mask] >= tightest) continue;
                        tightest = price.SubsetPrices[mask];
                        tightestAt = string.Join(" + ", survivors.Select(s => MatchModel.DisplayLabel(matchup, s)));
                    }
                }
        }

        _output.WriteLine($"{subsets} survivor subsets priced; worst |o' - formula| = {worst:E3}");
        _output.WriteLine($"tightest replacement at kappa = {config.SgpMargin}: {tightest:0.0000} on {tightestAt}");
        Assert.True(subsets > 20_000, $"sweep too thin: {subsets} subsets");
        Assert.True(worst == 0.0, $"a subset price drifted from the formula by {worst:E3}");
    }

    // =======================================================================================
    // EXIT GATE 2 — the evens floor: a replacement at or below 1.0 pays exactly the stake.
    // =======================================================================================

    /// <summary>
    /// Locates a SAME MATCH pair on a real board whose ticket is sellable but whose ONE-leg survivor
    /// set prices at or below evens, then drives the real Mulligan flow until that void actually
    /// happens. Returns null if no seed under <paramref name="seeds"/> produced it.
    ///
    /// <para>The pair is ordered so leg 0 is the survivor that floors, which is why the search accepts
    /// only a sweat that kills leg 1.</para>
    /// </summary>
    private (Run run, Ticket ticket, double rawReplacement)? FloorScenario(
        RunConfig config, TicketModifier modifier, string tag, int seeds)
    {
        for (int seed = 0; seed < seeds; seed++)
        {
            var run = new Run($"{tag}-{seed}", config);
            run.GrantConsumable(Mulligan());
            if (modifier == TicketModifier.DoubleOrNothing)
                run.GrantConsumable(RelicCatalog.Consumables.First(c => c.Id == "double_or_nothing"));
            Matchup matchup = run.CurrentSlate.Matchups[0];
            MarketSelection[] board = Board(matchup);

            Pick[]? picks = null;
            double raw = 0.0;
            for (int a = 0; a < board.Length && picks == null; a++)
                for (int b = 0; b < board.Length && picks == null; b++)
                {
                    if (a == b) continue;
                    var legs = new List<Leg> { LegFor(matchup, board[a]), LegFor(matchup, board[b]) };
                    SameMatchPrice p = SameMatchModel.Price(legs, config.Overround, config.SgpMargin);
                    if (p.Impossible || p.NaiveFallback || p.Price <= 1.0) continue;
                    if (p.SubsetPrices[0b01] > 1.0) continue; // leg 0 alone must be at or below evens

                    raw = p.SubsetPrices[0b01];
                    picks = new[] { new Pick(0, board[a]), new Pick(0, board[b]) };
                }
            if (picks == null) continue;

            Ticket ticket = run.PlaceTicket(picks, 10, modifier: modifier);
            run.LockRound();
            SweatSession sweat = run.Sweats[0];

            if (!StepToPendingLoss(sweat)) continue;
            if (sweat.PendingDeadLegIndex != 1 || !sweat.CanMulliganPendingLoss) continue;

            run.PlayMulliganSlip(sweat);
            return (run, ticket, raw);
        }
        return null;
    }

    /// <summary>
    /// A replacement price that would land at or below evens pays exactly the stake back
    /// (<c>design/02</c> § *Void: re-price on the survivors* — "a replacement price floors at 1.0").
    /// Reachable for real: the tightest replacement on the shipped board is ~1.118 at κ = 1, so any κ
    /// past that — inside the range the gate campaign explores — re-prices a LIVE ticket under evens,
    /// and the ticket is already sold by then, so placement-time refusal is unavailable.
    ///
    /// <para>The raw locked figure is asserted to be genuinely below 1.0 first: without that this test
    /// would pass on a board where the floor never binds and prove nothing.</para>
    /// </summary>
    [Fact]
    public void Gate2_a_replacement_at_or_below_evens_pays_exactly_the_stake()
    {
        var config = SweepConfig();
        config.SgpMargin = 1.5;

        var found = FloorScenario(config, TicketModifier.None, "sgp-void-floor", 200);
        Assert.True(found.HasValue, "no seed voided a same-match ticket down to a sub-evens survivor");
        (Run run, Ticket ticket, double raw) = found!.Value;

        Assert.True(raw <= 1.0, $"the scenario did not actually floor: raw replacement {raw:R}");
        Assert.True(ticket.PayoutMultiplier == 1.0, "no relic is in play, so the multiplier is 1");

        Assert.True(ticket.SameMatchContractPrice == 1.0,
            $"the floored contract price is {ticket.SameMatchContractPrice:R}, not evens");
        Assert.True(ticket.PotentialPayout == ticket.Stake,
            $"a floored ticket paid {ticket.PotentialPayout:R} rather than its stake {ticket.Stake:R}");

        // The raw figure is still on the ticket, unfloored: the floor is a reading rule on the
        // contract price, not a rewrite of what was locked.
        Assert.True(ticket.LockedSubsetPrices[0b01] == raw,
            "the locked table was rewritten by the floor");

        // And it settles at the stake: a win returns exactly what was risked, no profit, no loss.
        double bank = run.Bank;
        run.FastForwardRound();
        if (ticket.State == TicketState.Won)
            Assert.True(run.Bank == bank + ticket.Stake,
                "settlement credited something other than the stake");
        Assert.True(ticket.PotentialPayout == ticket.Stake,
            "the floored figure did not survive settlement");

        _output.WriteLine($"raw replacement {raw:0.0000} floored to 1.0000; "
            + $"stake {ticket.Stake} paid back exactly; ticket {ticket.State}");
    }

    /// <summary>
    /// The floor is applied to the PRICE, so it composes with <c>PayoutMultiplier</c> down the existing
    /// payout path instead of overriding it: a floored ticket carrying Double or Nothing pays
    /// <c>stake × 2</c>, exactly as a floored price × the ticket's factor product.
    ///
    /// <para>Canon says a floored replacement "pays exactly the stake back", which is the bare case
    /// (multiplier 1). It does not say what a payout relic does to that stake-back, and this test pins
    /// the answer the ruling's own wording implies — "reached through the existing payout path rather
    /// than a special-case branch" — so the choice is visible rather than incidental.</para>
    /// </summary>
    [Fact]
    public void The_evens_floor_composes_with_the_payout_multiplier_rather_than_replacing_it()
    {
        var config = SweepConfig();
        config.SgpMargin = 1.5;

        var found = FloorScenario(config, TicketModifier.DoubleOrNothing, "sgp-void-floor-don", 200);
        Assert.True(found.HasValue, "no seed floored a Double or Nothing same-match ticket");
        (Run _, Ticket ticket, double raw) = found!.Value;

        Assert.True(raw <= 1.0, $"the scenario did not actually floor: raw replacement {raw:R}");
        Assert.True(ticket.PayoutMultiplier == RelicCatalog.DoubleOrNothingMult,
            $"expected the DoN factor alone, got {ticket.PayoutMultiplier:R}");

        Assert.True(ticket.SameMatchContractPrice == 1.0);
        Assert.True(ticket.PotentialPayout == ticket.Stake * RelicCatalog.DoubleOrNothingMult,
            $"{ticket.PotentialPayout:R} != stake x {RelicCatalog.DoubleOrNothingMult:R}");

        _output.WriteLine($"floored + Double or Nothing: raw {raw:0.0000} -> 1.0000, "
            + $"payout {ticket.PotentialPayout:0.00} on a stake of {ticket.Stake:0.00}");
    }

    // =======================================================================================
    // EXIT GATE 3 — the duplicate ban and the evens backstop at placement.
    // =======================================================================================

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
