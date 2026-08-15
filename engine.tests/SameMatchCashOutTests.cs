using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// Phase 4 of F_0.6.0 — conditional cash-out on a SAME MATCH ticket
/// (<c>docs/sgp/phase-4-cashout-scope.md</c>; <c>design/02-betting-math.md</c> § *Cash-out pricing* →
/// *Same-game cash-out*).
///
/// <para><b>What was wrong.</b> <c>SweatSession</c> priced the remaining legs as
/// <c>Π(p_j × o_j)</c> off each leg's PRE-MATCH marginal, over a product of resolved leg odds. On a
/// same-match ticket that is wrong twice over: the surviving legs are correlated with each other, and
/// the ticket's price is a locked joint rather than a product of legs.</para>
///
/// <para><b>The rule.</b> With <c>S</c> the settled legs, <c>L</c> the leg in progress and <c>U</c>
/// the pending legs, <c>fair = payout × P(L ∧ U | S) × ( liveProb / P(L | S) )</c>, where each
/// conditional is a ratio of two joints the existing evaluator already computes and <c>payout</c> is
/// read off the survivor-subset table locked at placement. Exact — not an approximation — because the
/// sweat resolves ONE leg at a time, so the settled legs have settled outcomes; and cash-out is only
/// offered while the ticket is undecided, so every settled leg has WON.</para>
///
/// <para><c>Gate1_*</c> is THE INVARIANT: an ordinary ticket's quote is bit-identical to before, swept
/// through the real sweat and asserted with <c>==</c>. <c>Gate2_*</c> is the conditional itself and
/// its anchors, <c>Gate3_*</c> the Free Bet refund and the offer hold, <c>Gate4_*</c> the void seam,
/// <c>Gate5_*</c> the margin-vs-κ study the scope doc asked for.</para>
/// </summary>
public class SameMatchCashOutTests
{
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public SameMatchCashOutTests(Xunit.Abstractions.ITestOutputHelper output) => _output = output;

    /// <summary>Bank and ticket caps lifted so one round can hold a whole sweep; every PRICING dial —
    /// overround, cash-out margin, κ — stays exactly as the board ships, because those are what the
    /// quotes are being read off.</summary>
    private static RunConfig SweepConfig(double? sgpMargin = null, double? cashOutMargin = null)
    {
        var config = new RunConfig
        {
            StartingBank = 1_000_000_000,
            MaxTicketsPerRound = 100_000,
        };
        if (sgpMargin.HasValue) config.SgpMargin = sgpMargin.Value;
        if (cashOutMargin.HasValue) config.CashOutMargin = cashOutMargin.Value;
        return config;
    }

    private static ConsumableDefinition Mulligan()
        => RelicCatalog.Consumables.First(c => c.Id == "mulligan_slip");

    private static ConsumableDefinition FreeBet()
        => RelicCatalog.Consumables.First(c => c.Id == "free_bet");

    private static MarketSelection[] Board(Matchup matchup)
    {
        var board = new MarketSelection[matchup.Markets.Count];
        for (int i = 0; i < board.Length; i++) board[i] = matchup.Markets[i].Selection;
        return board;
    }

    private static int Step(int state) => (state * 1103515245 + 12345) & 0x7FFFFFFF;

    // =======================================================================================
    // The two rules, transcribed from the scope doc rather than from the engine.
    // =======================================================================================

    /// <summary>The pre-F_0.6.0 cash-out expression, transcribed. The invariant is that the engine
    /// still produces exactly this for any ticket with at most one leg per matchup.</summary>
    private static double OldFair(Ticket ticket, int currentLeg, double liveProb)
    {
        double resolvedOddsProduct = 1.0;
        for (int j = 0; j < currentLeg; j++)
            if (!ticket.Legs[j].IsVoided)
                resolvedOddsProduct *= ticket.Legs[j].OfferedOdds;

        var remaining = new List<(double p, double o)>
        {
            (liveProb, ticket.Legs[currentLeg].OfferedOdds),
        };
        for (int j = currentLeg + 1; j < ticket.Legs.Count; j++)
            if (!ticket.Legs[j].IsVoided)
                remaining.Add((ticket.Legs[j].TrueProb, ticket.Legs[j].OfferedOdds));

        double fair = OddsMath.CashOutFair(ticket.Stake, resolvedOddsProduct, remaining)
            * ticket.PayoutMultiplier;

        if (ticket.Modifier == TicketModifier.FreeBet && !ticket.Refunded)
        {
            double pAll = 1.0;
            foreach ((double p, double _) in remaining) pAll *= p;
            fair += (1.0 - pAll) * ticket.Stake;
        }
        return fair;
    }

    /// <summary>The active legs split into the three sets the conditional is written over, in TICKET
    /// ORDER: what has settled, that plus the leg in progress, that plus everything still pending.</summary>
    private static (List<Leg> settled, List<Leg> settledLive, List<Leg> active) Sets(Ticket ticket, int currentLeg)
    {
        var settled = new List<Leg>();
        for (int j = 0; j < currentLeg; j++)
            if (!ticket.Legs[j].IsVoided) settled.Add(ticket.Legs[j]);

        var settledLive = new List<Leg>(settled) { ticket.Legs[currentLeg] };

        var active = new List<Leg>(settledLive);
        for (int j = currentLeg + 1; j < ticket.Legs.Count; j++)
            if (!ticket.Legs[j].IsVoided) active.Add(ticket.Legs[j]);

        return (settled, settledLive, active);
    }

    /// <summary>The scope doc's rule, transcribed: <c>P(L ∧ U | S) × ( liveProb / P(L | S) )</c>.</summary>
    private static double ConditionalWinProb(Ticket ticket, int currentLeg, double liveProb)
    {
        (List<Leg> settled, List<Leg> settledLive, List<Leg> active) = Sets(ticket, currentLeg);

        double pSettled = SameMatchModel.JointProbabilityOf(settled);
        double condAll = SameMatchModel.JointProbabilityOf(active) / pSettled;
        double condLive = SameMatchModel.JointProbabilityOf(settledLive) / pSettled;
        return condAll * (liveProb / condLive);
    }

    private static double JointFair(Ticket ticket, int currentLeg, double liveProb)
    {
        double pWin = ConditionalWinProb(ticket, currentLeg, liveProb);
        double fair = ticket.PotentialPayout * pWin;
        if (ticket.Modifier == TicketModifier.FreeBet && !ticket.Refunded)
            fair += (1.0 - pWin) * ticket.Stake;
        return fair;
    }

    // =======================================================================================
    // Walking a sweat while mirroring the two pieces of state a quote depends on.
    // =======================================================================================

    /// <summary>One quote taken mid-sweat, with the session state it was taken at.</summary>
    private readonly struct Quote
    {
        public Quote(int currentLeg, double liveProb, double fair, double offer)
        {
            CurrentLeg = currentLeg;
            LiveProb = liveProb;
            Fair = fair;
            Offer = offer;
        }

        public int CurrentLeg { get; }
        public double LiveProb { get; }
        public double Fair { get; }
        public double Offer { get; }
    }

    /// <summary>
    /// Drains a session, collecting the quote at every point one is live, together with the
    /// <c>currentLeg</c> / <c>liveProb</c> pair the session priced it at.
    ///
    /// <para>Both are mirrored from the PUBLIC stream rather than read out of the session: the cursor
    /// advances on a won <see cref="DramaEventType.LegFinal"/>, and the live probability is the last
    /// event's <see cref="DramaEvent.WinProbAfter"/> except immediately after a leg boundary, where
    /// the session re-seeds it to the next leg's pre-match marginal. Grant no consumables and a dead
    /// leg busts instantly, so the only boundary this has to model is the winning one.</para>
    /// </summary>
    private static List<Quote> Sweat(SweatSession session, Ticket ticket)
    {
        var quotes = new List<Quote>();
        int currentLeg = 0;
        double liveProb = ticket.Legs[0].TrueProb;

        Take(quotes, session, currentLeg, liveProb);
        while (session.MoveNext(out DramaEvent? e))
        {
            liveProb = e!.WinProbAfter;
            if (e.Type == DramaEventType.LegFinal)
            {
                if (session.RevealedLegState(e.LegIndex) != LegState.Won) break; // busted
                currentLeg++;
                if (session.IsComplete) break;
                liveProb = ticket.Legs[currentLeg].TrueProb;
            }
            Take(quotes, session, currentLeg, liveProb);
        }
        return quotes;
    }

    private static void Take(List<Quote> quotes, SweatSession session, int currentLeg, double liveProb)
    {
        double? fair = session.CashOutFair();
        if (!fair.HasValue) return;
        quotes.Add(new Quote(currentLeg, liveProb, fair.Value, session.CashOutOffer()!.Value));
    }

    // =======================================================================================
    // Populations.
    // =======================================================================================

    /// <summary>Ordinary tickets: 2–4 legs, every leg on a DIFFERENT matchup, markets drawn off the
    /// real board. The population THE INVARIANT is swept over.</summary>
    private static void PlaceOrdinary(Run run, int seed, int shapes)
    {
        int slateSize = run.CurrentSlate.Matchups.Count;
        int state = Step(seed + 1);

        for (int shape = 0; shape < shapes; shape++)
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
                MarketSelection[] board = Board(run.CurrentSlate.Matchups[matchupIndex]);
                state = Step(state);
                picks.Add(new Pick(matchupIndex, board[state % board.Length]));
            }
            if (picks.Count != used.Count) continue;
            run.PlaceTicket(picks, 10);
        }
    }

    /// <summary>SAME MATCH tickets: 2–3 legs on ONE matchup, optionally with a leg on a second, drawn
    /// off the real board and filtered through the real refusal so only sellable slips are swept.</summary>
    private static void PlaceSameMatch(Run run, int seed, int shapes)
    {
        int slateSize = run.CurrentSlate.Matchups.Count;
        int state = Step(seed + 7);

        for (int shape = 0; shape < shapes; shape++)
        {
            state = Step(state);
            int home = state % slateSize;
            MarketSelection[] board = Board(run.CurrentSlate.Matchups[home]);

            int groupSize = 2 + (shape % 2);              // two or three legs on the one matchup
            bool spread = shape % 3 == 2;                  // and sometimes a leg elsewhere

            var picks = new List<Pick>();
            var seen = new HashSet<MarketSelection>();
            for (int i = 0; i < groupSize; i++)
            {
                state = Step(state);
                MarketSelection selection = board[state % board.Length];
                if (!seen.Add(selection)) continue;
                picks.Add(new Pick(home, selection));
            }
            if (picks.Count < 2) continue;

            if (spread)
            {
                int other = (home + 1 + (state % (slateSize - 1))) % slateSize;
                MarketSelection[] otherBoard = Board(run.CurrentSlate.Matchups[other]);
                state = Step(state);
                picks.Add(new Pick(other, otherBoard[state % otherBoard.Length]));
            }

            if (run.RefusalFor(picks) != null) continue;   // the book would not sell it
            run.PlaceTicket(picks, 10);
        }
    }

    // =======================================================================================
    // EXIT GATE 1 — THE INVARIANT.
    // =======================================================================================

    /// <summary>
    /// A ticket with at most one leg per matchup must quote a cash-out figure BIT-IDENTICAL to before
    /// F_0.6.0 Phase 4. Not "within a tolerance" — <c>==</c>, over a swept population, through the real
    /// sweat rather than a synthetic one, at EVERY event a quote is live at.
    ///
    /// <para>The property is structural: such a ticket carries no <c>SameMatch</c> block, so
    /// <c>RawCashOutFair</c> leaves on its first line into the untouched product expression. This
    /// sweep is the check on that routing, and it asserts the OFFER too — the number that becomes
    /// money — not only the fair value behind it.</para>
    /// </summary>
    [Fact]
    public void Gate1_an_ordinary_ticket_quotes_bit_identically_through_the_real_sweat()
    {
        long tickets = 0, comparisons = 0, afterASettle = 0;

        for (int seed = 0; seed < 90; seed++)
        {
            var run = new Run($"sgp-cashout-ordinary-{seed}", SweepConfig());
            PlaceOrdinary(run, seed, shapes: 6);
            if (run.Tickets.Count == 0) continue;
            run.LockRound();

            for (int i = 0; i < run.Sweats.Count; i++)
            {
                Ticket ticket = run.Tickets[i];
                Assert.Null(ticket.SameMatch); // the guard's precondition, asserted not assumed
                tickets++;

                SweatSession session = run.Sweats[i];
                int currentLeg = 0;
                double liveProb = ticket.Legs[0].TrueProb;

                while (true)
                {
                    double? fair = session.CashOutFair();
                    if (fair.HasValue)
                    {
                        double expected = OldFair(ticket, currentLeg, liveProb);
                        Assert.True(expected == fair.Value,
                            $"seed {seed} ticket {i} leg {currentLeg}: {expected:R} != {fair.Value:R}");
                        Assert.True(OddsMath.CashOutOffer(expected, run.Config.CashOutMargin)
                            == session.CashOutOffer()!.Value);
                        comparisons++;
                        if (currentLeg > 0) afterASettle++;
                    }

                    if (!session.MoveNext(out DramaEvent? e)) break;
                    liveProb = e!.WinProbAfter;
                    if (e.Type != DramaEventType.LegFinal) continue;
                    if (session.RevealedLegState(e.LegIndex) != LegState.Won) break;
                    currentLeg++;
                    if (session.IsComplete) break;
                    liveProb = ticket.Legs[currentLeg].TrueProb;
                }
            }
        }

        _output.WriteLine($"Gate 1: {comparisons} exact comparisons over {tickets} ordinary tickets "
            + $"({afterASettle} of them after at least one leg had settled).");
        Assert.True(tickets >= 100, $"only {tickets} ordinary tickets swept");
        Assert.True(comparisons >= 1000, $"only {comparisons} comparisons");
        Assert.True(afterASettle >= 200, $"only {afterASettle} comparisons after a settle");
    }

    /// <summary>The same population under a MOVED κ. κ is the SAME MATCH margin dial and cannot reach
    /// a ticket with at most one leg per matchup; if a quote ever moved with it, the routing would be
    /// wrong however well the arithmetic agreed.</summary>
    [Fact]
    public void Gate1_moving_kappa_cannot_move_an_ordinary_tickets_quote()
    {
        long comparisons = 0;

        for (int seed = 0; seed < 40; seed++)
        {
            var shipped = new Run($"sgp-cashout-kappa-{seed}", SweepConfig());
            var raised = new Run($"sgp-cashout-kappa-{seed}", SweepConfig(sgpMargin: 2.5));
            PlaceOrdinary(shipped, seed, shapes: 6);
            PlaceOrdinary(raised, seed, shapes: 6);
            if (shipped.Tickets.Count == 0) continue;

            shipped.LockRound();
            raised.LockRound();
            Assert.Equal(shipped.Sweats.Count, raised.Sweats.Count);

            for (int i = 0; i < shipped.Sweats.Count; i++)
            {
                List<Quote> a = Sweat(shipped.Sweats[i], shipped.Tickets[i]);
                List<Quote> b = Sweat(raised.Sweats[i], raised.Tickets[i]);
                Assert.Equal(a.Count, b.Count);
                for (int q = 0; q < a.Count; q++)
                {
                    Assert.True(a[q].Fair == b[q].Fair, $"seed {seed} quote {q}: κ moved an ordinary fair");
                    Assert.True(a[q].Offer == b[q].Offer, $"seed {seed} quote {q}: κ moved an ordinary offer");
                    comparisons++;
                }
            }
        }

        Assert.True(comparisons >= 300, $"only {comparisons} comparisons");
    }

    // =======================================================================================
    // EXIT GATE 2 — the conditional, and the anchors.
    // =======================================================================================

    /// <summary>
    /// A SAME MATCH ticket prices off the conditional joint with the drama re-weight, at every event a
    /// quote is live at, over a swept population — and exactly, because the test restates the RULE
    /// (payout off the locked table, joints off the model) rather than the engine's expression.
    /// </summary>
    [Fact]
    public void Gate2_a_same_match_ticket_prices_off_the_conditional_joint()
    {
        long tickets = 0, comparisons = 0, afterASettle = 0;

        for (int seed = 0; seed < 40; seed++)
        {
            var run = new Run($"sgp-cashout-joint-{seed}", SweepConfig());
            PlaceSameMatch(run, seed, shapes: 6);
            if (run.Tickets.Count == 0) continue;
            run.LockRound();

            for (int i = 0; i < run.Sweats.Count; i++)
            {
                Ticket ticket = run.Tickets[i];
                Assert.NotNull(ticket.SameMatch);
                Assert.False(ticket.SameMatch!.NaiveFallback); // the shipped board labels everything
                tickets++;

                foreach (Quote q in Sweat(run.Sweats[i], ticket))
                {
                    double expected = JointFair(ticket, q.CurrentLeg, q.LiveProb);
                    Assert.True(expected == q.Fair,
                        $"seed {seed} ticket {i} leg {q.CurrentLeg}: {expected:R} != {q.Fair:R}");
                    comparisons++;
                    if (q.CurrentLeg > 0) afterASettle++;
                }
            }
        }

        _output.WriteLine($"Gate 2: {comparisons} exact comparisons over {tickets} SAME MATCH tickets "
            + $"({afterASettle} of them after at least one leg had settled).");
        Assert.True(tickets >= 60, $"only {tickets} same-match tickets swept");
        Assert.True(comparisons >= 500, $"only {comparisons} comparisons");
        Assert.True(afterASettle >= 100, $"only {afterASettle} comparisons after a settle — "
            + "the sweep never reached the state the whole phase exists for");
    }

    /// <summary>
    /// ANCHOR 1 — nothing settled, so the quote is <c>P(win) × payout</c>.
    ///
    /// <para>The load-bearing half IS exact and is asserted with <c>==</c>: with <c>S</c> empty the
    /// conditional's denominator is the empty conjunction 1.0, so the numerator is the whole ticket's
    /// joint — and that joint is the very number the ticket was SOLD at, bit for bit
    /// (<c>SameMatchPrice.PTicket</c>), because it runs the same grouping in the same order.</para>
    ///
    /// <para><b>The quote itself lands within a few ulp of it rather than on it, and the reason is
    /// upstream.</b> The re-weight at this point is <c>liveProb / p_joint({L})</c>, and the sweat
    /// seeds <c>liveProb</c> from <see cref="Leg.TrueProb"/> — the BOARD's marginal. Canon (the
    /// goal-family note in <c>JointModel</c>) claims a single-selection enumeration reproduces that
    /// board price to the bit; it does not, for the scorer and several count markets — see
    /// <c>Gate2_diagnostic_where_the_joint_stops_agreeing_with_the_board_to_the_bit</c>, which
    /// measures up to ~70 ulp on <c>PlayerMultiScorer</c>. So the ratio is 1.0 to within those ulp
    /// instead of exactly 1.0. Mixing the two evaluators inside one ratio to force the anchor would be
    /// the wrong fix: <c>P(L | S)</c> must come from the same evaluator as <c>P(L ∧ U | S)</c> or it
    /// is not a conditional.</para>
    /// </summary>
    [Fact]
    public void Gate2_with_nothing_settled_the_quote_is_the_payout_times_the_ticket_joint()
    {
        long anchors = 0;
        double worst = 0.0;

        for (int seed = 0; seed < 40; seed++)
        {
            var run = new Run($"sgp-cashout-anchor-{seed}", SweepConfig());
            PlaceSameMatch(run, seed, shapes: 6);
            if (run.Tickets.Count == 0) continue;
            run.LockRound();

            for (int i = 0; i < run.Sweats.Count; i++)
            {
                Ticket ticket = run.Tickets[i];

                // The exact half: the conditional's numerator IS the locked joint, to the bit.
                Assert.True(SameMatchModel.JointProbabilityOf(ticket.Legs) == ticket.SameMatch!.PTicket,
                    $"seed {seed} ticket {i}: the unconditional joint drifted off the locked PTicket");

                double fair = run.Sweats[i].CashOutFair()!.Value; // before a single event
                double expected = ticket.PotentialPayout * ticket.SameMatch!.PTicket;
                worst = Math.Max(worst, Math.Abs(expected - fair) / expected);
                anchors++;
            }
        }

        _output.WriteLine($"Gate 2 anchor 1: {anchors} anchors; largest RELATIVE gap {worst:E3} "
            + $"({worst / 2.220446049250313e-16:F0} ulp) — the board-vs-model marginal, not the conditional.");
        Assert.True(anchors >= 60, $"only {anchors} anchors");
        Assert.True(worst < 1e-12,
            $"the anchor drifted by {worst:E3} relative, far beyond the marginal's measured ulp");
    }

    /// <summary>DIAGNOSTIC for the anchor above: where, if anywhere, does the conditional's arithmetic
    /// stop agreeing to the bit with the numbers the ticket was sold at. Two claims are under test —
    /// that a joint over the whole leg list reproduces the locked <c>PTicket</c>, and that a
    /// single-leg joint reproduces the board's own <c>Leg.TrueProb</c> (which
    /// <c>JointModel</c>'s goal-family note asserts outright).</summary>
    [Fact]
    public void Gate2_diagnostic_where_the_joint_stops_agreeing_with_the_board_to_the_bit()
    {
        long legs = 0, legMismatch = 0, tickets = 0, ticketMismatch = 0;
        var byKind = new SortedDictionary<string, (long n, long bad, double worstUlp)>();

        for (int seed = 0; seed < 20; seed++)
        {
            var run = new Run($"sgp-cashout-anchor-{seed}", SweepConfig());
            PlaceSameMatch(run, seed, shapes: 6);
            foreach (Ticket ticket in run.Tickets)
            {
                tickets++;
                if (SameMatchModel.JointProbabilityOf(ticket.Legs) != ticket.SameMatch!.PTicket)
                    ticketMismatch++;

                foreach (Leg leg in ticket.Legs)
                {
                    legs++;
                    double model = SameMatchModel.JointProbabilityOf(new[] { leg });
                    double board = leg.TrueProb;
                    string kind = leg.Selection.Kind.ToString();
                    byKind.TryGetValue(kind, out (long n, long bad, double worstUlp) row);
                    double ulps = model == board
                        ? 0.0
                        : Math.Abs(model - board) / Math.Max(double.Epsilon,
                            Math.Abs(board) * 2.220446049250313e-16);
                    if (model != board) { legMismatch++; row.bad++; }
                    byKind[kind] = (row.n + 1, row.bad, Math.Max(row.worstUlp, ulps));
                }
            }
        }

        _output.WriteLine($"p_joint(all legs) vs locked PTicket: {ticketMismatch} of {tickets} disagree.");
        _output.WriteLine($"p_joint(one leg) vs Leg.TrueProb: {legMismatch} of {legs} disagree.");
        foreach ((string kind, (long n, long bad, double worstUlp) row) in byKind)
            _output.WriteLine($"  {kind,-22} n={row.n,4}  disagreeing={row.bad,4}  worst={row.worstUlp:F1} ulp");

        // The locked joint is the one that MUST agree — the conditional's numerator is that same
        // product, through that same grouping, in that same order.
        Assert.Equal(0, ticketMismatch);
    }

    /// <summary>ANCHOR 2, in the only form the session can express it. Once the leg in progress is the
    /// LAST one, <c>P(L ∧ U | S)</c> and <c>P(L | S)</c> are joints over the same leg set, so the
    /// quote is <c>payout × liveProb</c> — and walks to the full payout as that leg closes out. Pinned
    /// to a relative ulp rather than <c>==</c>: the rule is written as the scope doc writes it, two
    /// ratios, and <c>c × (liveProb / c)</c> is not required to round back to <c>liveProb</c>.</summary>
    [Fact]
    public void Gate2_with_only_the_last_leg_left_the_quote_is_the_payout_times_the_live_number()
    {
        long anchors = 0;

        for (int seed = 0; seed < 40; seed++)
        {
            var run = new Run($"sgp-cashout-last-{seed}", SweepConfig());
            PlaceSameMatch(run, seed, shapes: 6);
            if (run.Tickets.Count == 0) continue;
            run.LockRound();

            for (int i = 0; i < run.Sweats.Count; i++)
            {
                Ticket ticket = run.Tickets[i];
                int lastActive = ticket.Legs.Count - 1;
                foreach (Quote q in Sweat(run.Sweats[i], ticket))
                {
                    if (q.CurrentLeg != lastActive) continue;
                    double expected = ticket.PotentialPayout * q.LiveProb;
                    Assert.Equal(expected, q.Fair, 12);
                    Assert.True(Math.Abs(expected - q.Fair) <= 4.0 * Math.Abs(expected) * 2.220446049250313e-16,
                        $"seed {seed}: {expected:R} vs {q.Fair:R} is more than 4 ulp apart");
                    anchors++;
                }
            }
        }

        _output.WriteLine($"Gate 2 anchor 2: {anchors} last-leg quotes.");
        Assert.True(anchors >= 100, $"only {anchors} last-leg quotes");
    }

    /// <summary>
    /// Conditioning MOVES the number: on the swept population the conditional quote differs from the
    /// old independent-product quote, and does so materially rather than in the last bits. If it did
    /// not, the bug the phase exists to fix would not have been reachable damage.
    ///
    /// <para>Direction is not asserted globally — a same-match ticket can carry positive and negative
    /// relations at once — but both directions must be OBSERVED, or the sweep is only exercising one
    /// side of the correlation structure.</para>
    /// </summary>
    [Fact]
    public void Gate2_the_conditional_moves_the_quote_off_the_independent_product()
    {
        long compared = 0, richer = 0, poorer = 0;
        double worst = 0.0;
        string worstShape = "";
        var moves = new List<double>();

        for (int seed = 0; seed < 40; seed++)
        {
            var run = new Run($"sgp-cashout-delta-{seed}", SweepConfig());
            PlaceSameMatch(run, seed, shapes: 6);
            if (run.Tickets.Count == 0) continue;
            run.LockRound();

            for (int i = 0; i < run.Sweats.Count; i++)
            {
                Ticket ticket = run.Tickets[i];
                foreach (Quote q in Sweat(run.Sweats[i], ticket))
                {
                    double old = OldFair(ticket, q.CurrentLeg, q.LiveProb);
                    double delta = (q.Fair - old) / old;
                    if (Math.Abs(delta) > 1e-9)
                    {
                        if (delta > 0) richer++; else poorer++;
                        moves.Add(Math.Abs(delta));
                        if (Math.Abs(delta) > Math.Abs(worst))
                        {
                            worst = delta;
                            worstShape = string.Join(" + ",
                                ticket.Legs.Select(l => l.Selection.Kind.ToString()))
                                + " | " + string.Join(",",
                                    ticket.SameMatch!.Relations.Select(r => r.Kind.ToString()))
                                + $" | settled={q.CurrentLeg}";
                        }
                    }
                    compared++;
                }
            }
        }

        moves.Sort();
        double median = moves.Count == 0 ? 0.0 : moves[moves.Count / 2];
        double p95 = moves.Count == 0 ? 0.0 : moves[(int)(moves.Count * 0.95)];
        _output.WriteLine($"Gate 2 delta: {richer + poorer} of {compared} quotes move off the old product "
            + $"({richer} richer, {poorer} poorer); |move| median {median:P2}, p95 {p95:P2}, "
            + $"largest {worst:P2}.");
        _output.WriteLine($"  largest mover: {worstShape}");
        Assert.True(richer > 0, "no quote was richer under the conditional");
        Assert.True(poorer > 0, "no quote was poorer under the conditional");
        Assert.True(Math.Abs(worst) > 0.01, $"the largest move was only {worst:P4} — not reachable damage");
    }

    /// <summary>
    /// The quote can never exceed the payout, structurally: adding the pending legs' constraints can
    /// only lower a joint, so <c>P(L ∧ U | S) ≤ P(L | S)</c> and the conditional win probability is at
    /// most <c>liveProb &lt; 1</c>. Also the reachability proof for the two guards in
    /// <c>ConditionalWinProb</c> — both denominators are joints over subsets of a ticket that placed,
    /// so neither can be zero, and the sweep confirms it on the shipped board.
    /// </summary>
    [Fact]
    public void Gate2_the_conditional_is_bounded_by_the_live_number_and_never_divides_by_zero()
    {
        long checks = 0;
        double maxRatio = 0.0, minDenominator = double.MaxValue;

        for (int seed = 0; seed < 40; seed++)
        {
            var run = new Run($"sgp-cashout-bound-{seed}", SweepConfig());
            PlaceSameMatch(run, seed, shapes: 6);
            if (run.Tickets.Count == 0) continue;
            run.LockRound();

            for (int i = 0; i < run.Sweats.Count; i++)
            {
                Ticket ticket = run.Tickets[i];
                foreach (Quote q in Sweat(run.Sweats[i], ticket))
                {
                    (List<Leg> settled, List<Leg> settledLive, _) = Sets(ticket, q.CurrentLeg);
                    double pSettled = SameMatchModel.JointProbabilityOf(settled);
                    double pSettledLive = SameMatchModel.JointProbabilityOf(settledLive);
                    Assert.True(pSettled > 0.0, "p_joint(S) reached zero on a sold ticket");
                    Assert.True(pSettledLive > 0.0, "p_joint(S ∪ L) reached zero on a sold ticket");
                    minDenominator = Math.Min(minDenominator, pSettledLive / pSettled);

                    double pWin = q.Fair / ticket.PotentialPayout;
                    Assert.True(pWin <= q.LiveProb + 1e-12, $"p_win {pWin:R} exceeded liveProb {q.LiveProb:R}");
                    Assert.True(q.Fair < ticket.PotentialPayout);
                    maxRatio = Math.Max(maxRatio, pWin / q.LiveProb);
                    checks++;
                }
            }
        }

        _output.WriteLine($"Gate 2 bound: {checks} quotes; max p_win/liveProb {maxRatio:F12}; "
            + $"smallest P(L|S) seen {minDenominator:R}.");
        Assert.True(checks >= 500);
        Assert.True(maxRatio <= 1.0 + 1e-12);
    }

    // =======================================================================================
    // EXIT GATE 3 — the Free Bet refund, and the offer hold.
    // =======================================================================================

    /// <summary>
    /// Free Bet's loss-side refund fires exactly when the ticket does NOT win, so under a joint it must
    /// be <c>(1 − p_win) × stake</c> for the SAME conditional <c>p_win</c> the win side uses — not
    /// <c>(1 − Π p)</c> over the remaining legs' marginals, which is not the probability a correlated
    /// ticket still wins. Pinned by comparing the plain and Free Bet quotes on the SAME slip: the gap
    /// between them IS the refund, and it must be the conditional's.
    /// </summary>
    [Fact]
    public void Gate3_the_free_bet_refund_uses_the_conditional_not_the_product()
    {
        long tickets = 0, quotes = 0;
        double worstAgainstOldRule = 0.0;

        for (int seed = 0; seed < 40; seed++)
        {
            var plain = new Run($"sgp-cashout-freebet-{seed}", SweepConfig());
            var freeb = new Run($"sgp-cashout-freebet-{seed}", SweepConfig());
            for (int i = 0; i < 64; i++) freeb.GrantConsumable(FreeBet());

            var slips = new List<IReadOnlyList<Pick>>();
            CollectSameMatchSlips(plain, seed, shapes: 6, into: slips);
            foreach (IReadOnlyList<Pick> slip in slips)
            {
                plain.PlaceTicket(slip, 10);
                freeb.PlaceTicket(slip, 10, modifier: TicketModifier.FreeBet);
            }
            if (plain.Tickets.Count == 0) continue;

            plain.LockRound();
            freeb.LockRound();

            for (int i = 0; i < plain.Sweats.Count; i++)
            {
                Ticket plainTicket = plain.Tickets[i];
                Ticket freeTicket = freeb.Tickets[i];
                Assert.Equal(TicketModifier.FreeBet, freeTicket.Modifier);
                Assert.NotNull(freeTicket.SameMatch);
                tickets++;

                List<Quote> a = Sweat(plain.Sweats[i], plainTicket);
                List<Quote> b = Sweat(freeb.Sweats[i], freeTicket);
                Assert.Equal(a.Count, b.Count);

                for (int q = 0; q < a.Count; q++)
                {
                    double pWin = ConditionalWinProb(freeTicket, b[q].CurrentLeg, b[q].LiveProb);
                    double refund = (1.0 - pWin) * freeTicket.Stake;

                    // The engine's own gap between the two quotes IS the refund it applied.
                    Assert.Equal(refund, b[q].Fair - a[q].Fair, 9);
                    Assert.True(refund > 0.0, "a refund term went negative");

                    // ...and it is NOT the superseded product rule.
                    double oldPAll = OldProductWinProb(freeTicket, b[q].CurrentLeg, b[q].LiveProb);
                    double oldRefund = (1.0 - oldPAll) * freeTicket.Stake;
                    worstAgainstOldRule = Math.Max(worstAgainstOldRule,
                        Math.Abs(oldRefund - refund) / freeTicket.Stake);
                    quotes++;
                }
            }
        }

        _output.WriteLine($"Gate 3: {quotes} Free Bet quotes over {tickets} SAME MATCH tickets; "
            + $"largest divergence from the old product refund {worstAgainstOldRule:P2} of stake.");
        Assert.True(tickets >= 40, $"only {tickets} Free Bet tickets");
        Assert.True(worstAgainstOldRule > 0.01,
            $"the conditional refund never diverged from the product one by more than {worstAgainstOldRule:P4}");
    }

    /// <summary>The superseded rule's <c>Π p</c> over the remaining legs, for the divergence check.</summary>
    private static double OldProductWinProb(Ticket ticket, int currentLeg, double liveProb)
    {
        double pAll = liveProb;
        for (int j = currentLeg + 1; j < ticket.Legs.Count; j++)
            if (!ticket.Legs[j].IsVoided) pAll *= ticket.Legs[j].TrueProb;
        return pAll;
    }

    /// <summary>The slips <see cref="PlaceSameMatch"/> would place, without placing them — so the same
    /// slip can be dealt to two runs (plain and Free Bet) and compared quote for quote.</summary>
    private static void CollectSameMatchSlips(Run run, int seed, int shapes, List<IReadOnlyList<Pick>> into)
    {
        int slateSize = run.CurrentSlate.Matchups.Count;
        int state = Step(seed + 7);

        for (int shape = 0; shape < shapes; shape++)
        {
            state = Step(state);
            int home = state % slateSize;
            MarketSelection[] board = Board(run.CurrentSlate.Matchups[home]);

            int groupSize = 2 + (shape % 2);
            bool spread = shape % 3 == 2;

            var picks = new List<Pick>();
            var seen = new HashSet<MarketSelection>();
            for (int i = 0; i < groupSize; i++)
            {
                state = Step(state);
                MarketSelection selection = board[state % board.Length];
                if (!seen.Add(selection)) continue;
                picks.Add(new Pick(home, selection));
            }
            if (picks.Count < 2) continue;

            if (spread)
            {
                int other = (home + 1 + (state % (slateSize - 1))) % slateSize;
                MarketSelection[] otherBoard = Board(run.CurrentSlate.Matchups[other]);
                state = Step(state);
                picks.Add(new Pick(other, otherBoard[state % otherBoard.Length]));
            }

            if (run.RefusalFor(picks) != null) continue;
            into.Add(picks);
        }
    }

    /// <summary>
    /// The offer-hold seam still freezes the quote on a SAME MATCH ticket: the held FAIR value survives
    /// its event count unchanged even as the live number ticks under it, and the quote comes back to
    /// life — at a different number — once the hold lapses.
    ///
    /// <para>The seam's own accounting is unchanged and is respected here rather than restated:
    /// <c>MoveNext</c> decrements the counter BEFORE the quote is read, so an
    /// <c>OfferHoldEffect(n)</c> freezes the quote across <c>n − 1</c> subsequent events and the
    /// <c>n</c>-th releases it.</para>
    /// </summary>
    [Fact]
    public void Gate3_an_offer_hold_still_freezes_a_same_match_quote()
    {
        const int HoldEvents = 4;
        long held = 0, lapsed = 0, moved = 0;

        for (int seed = 0; seed < 60 && held < 24; seed++)
        {
            var run = new Run($"sgp-cashout-hold-{seed}", SweepConfig());
            PlaceSameMatch(run, seed, shapes: 6);
            if (run.Tickets.Count == 0) continue;
            run.LockRound();

            for (int i = 0; i < run.Sweats.Count; i++)
            {
                SweatSession session = run.Sweats[i];
                Assert.NotNull(run.Tickets[i].SameMatch);

                session.MoveNext(out _); // one beat in, so the live number has already moved
                if (session.IsComplete || session.CashOutFair() == null) continue;

                double frozen = session.CashOutFair()!.Value;
                session.ApplyLiveEffect(new OfferHoldEffect(HoldEvents));

                bool stillHeld = true;
                for (int e = 0; e < HoldEvents - 1 && stillHeld; e++)
                {
                    if (!session.MoveNext(out _)) { stillHeld = false; break; }
                    double? quote = session.CashOutFair();
                    if (!quote.HasValue) { stillHeld = false; break; }
                    Assert.True(frozen == quote.Value, "the hold did not freeze the SAME MATCH quote");
                    held++;
                }

                if (!stillHeld) continue;
                if (!session.MoveNext(out _)) continue;
                double? after = session.CashOutFair();
                if (!after.HasValue) continue;
                lapsed++;
                if (after.Value != frozen) moved++; // the hold really did lapse, not merely expire
            }
        }

        _output.WriteLine($"Gate 3 hold: {held} frozen quotes, {lapsed} holds lapsed, "
            + $"{moved} of them onto a different number.");
        Assert.True(held >= 24, $"only {held} frozen quotes observed");
        Assert.True(moved >= 1, "no hold was observed lapsing back onto a live, different quote");
    }

    // =======================================================================================
    // EXIT GATE 4 — the void seam. The scope doc's flagged risk: cash-out and the survivor-subset
    // re-price had never met.
    // =======================================================================================

    /// <summary>
    /// After a Mulligan voids a leg of a SAME MATCH ticket, the quote must re-price on the SURVIVORS:
    /// the payout comes from the locked survivor-subset table (never re-derived here) and the
    /// conditional runs over the surviving legs only. Driven through the real
    /// <c>Run.PlayMulliganSlip</c> — <c>Leg.IsVoided</c> has an internal setter, so there is no way to
    /// fake a void.
    /// </summary>
    [Fact]
    public void Gate4_a_voided_same_match_ticket_quotes_off_the_locked_replacement_price()
    {
        long voided = 0, quotes = 0;

        for (int seed = 0; seed < 90 && voided < 8; seed++)
        {
            var run = new Run($"sgp-cashout-void-{seed}", SweepConfig());
            for (int i = 0; i < 8; i++) run.GrantConsumable(Mulligan());
            PlaceSameMatch(run, seed, shapes: 8);
            if (run.Tickets.Count == 0) continue;
            run.LockRound();

            for (int i = 0; i < run.Sweats.Count; i++)
            {
                SweatSession session = run.Sweats[i];
                Ticket ticket = run.Tickets[i];
                if (ticket.Legs.Count < 3) continue; // a mulligan needs two ACTIVE legs to leave behind

                int currentLeg = 0;
                double liveProb = ticket.Legs[0].TrueProb;
                bool sawVoid = false;

                while (true)
                {
                    if (session.HasPendingLoss)
                    {
                        if (!session.CanMulliganPendingLoss
                            || !run.OwnedConsumables.Any(c => c.Id == "mulligan_slip"))
                        {
                            session.MoveNext(out _); // decline; the bust proceeds
                            break;
                        }
                        run.PlayMulliganSlip(session);
                        currentLeg++;
                        sawVoid = true;
                        if (session.IsComplete || ticket.State != TicketState.Open) break;
                        liveProb = ticket.Legs[currentLeg].TrueProb;
                    }

                    if (sawVoid)
                    {
                        double? fair = session.CashOutFair();
                        if (fair.HasValue)
                        {
                            // The payout is the LOCKED replacement, read not re-derived.
                            int survivors = 0;
                            for (int j = 0; j < ticket.Legs.Count; j++)
                                if (!ticket.Legs[j].IsVoided) survivors |= 1 << j;
                            Assert.Equal(ticket.LockedSubsetPrices[survivors], ticket.SameMatchContractPrice);

                            double expected = JointFair(ticket, currentLeg, liveProb);
                            Assert.True(expected == fair.Value,
                                $"seed {seed}: post-void {expected:R} != {fair.Value:R}");
                            quotes++;
                        }
                    }

                    if (!session.MoveNext(out DramaEvent? e)) break;
                    liveProb = e!.WinProbAfter;
                    if (e.Type != DramaEventType.LegFinal) continue;
                    if (session.HasPendingLoss) continue;
                    if (session.RevealedLegState(e.LegIndex) != LegState.Won) break;
                    currentLeg++;
                    if (session.IsComplete) break;
                    liveProb = ticket.Legs[currentLeg].TrueProb;
                }

                if (sawVoid) voided++;
            }
        }

        _output.WriteLine($"Gate 4: {voided} voided SAME MATCH tickets, {quotes} post-void quotes checked.");
        Assert.True(voided >= 8, $"only {voided} voided same-match tickets found");
        Assert.True(quotes >= 8, $"only {quotes} post-void quotes");
    }

    // =======================================================================================
    // EXIT GATE 5 — the margin-vs-κ study the scope doc asked for. This gate REPORTS; the only
    // thing it asserts is the degeneracy question it was asked to answer.
    // =======================================================================================

    /// <summary>
    /// <c>docs/sgp/phase-4-cashout-scope.md</c>: "Cash-out margin interacts with κ and neither has
    /// been tuned against the other." This measures the two dials against each other over a swept
    /// population of SAME MATCH tickets and asserts the one thing that would be a defect: that no
    /// combination produces a quote at or above the ticket's own potential payout, or otherwise
    /// degenerate. It TUNES NOTHING — the figures go to the lead's report.
    /// </summary>
    [Fact]
    public void Gate5_margin_against_kappa_is_measured_and_never_degenerates()
    {
        double[] kappas = { 1.0, 1.25, 1.5, 2.0, 3.0 };
        double[] margins = { 0.0, 0.08, 0.15, 0.25 };

        _output.WriteLine("kappa  margin | sold  quotes | max off/pay  max off/stake  min off/stake  "
            + "min pay/stake |  >=payout  >=stake");

        var soldAt = new Dictionary<double, int>();
        var offerAtKappa = new Dictionary<double, double>();

        foreach (double kappa in kappas)
        {
            foreach (double margin in margins)
            {
                int sold = 0, quotes = 0, atOrAbovePayout = 0, atOrAboveStake = 0;
                double maxOfferOverPayout = 0.0, maxOfferOverStake = 0.0;
                double minOfferOverStake = double.MaxValue, minPayoutOverStake = double.MaxValue;
                double firstOffer = 0.0;

                for (int seed = 0; seed < 25; seed++)
                {
                    var run = new Run($"sgp-cashout-study-{seed}",
                        SweepConfig(sgpMargin: kappa, cashOutMargin: margin));
                    PlaceSameMatch(run, seed, shapes: 6);
                    if (run.Tickets.Count == 0) continue;
                    run.LockRound();

                    for (int i = 0; i < run.Sweats.Count; i++)
                    {
                        Ticket ticket = run.Tickets[i];
                        sold++;
                        minPayoutOverStake = Math.Min(minPayoutOverStake,
                            ticket.PotentialPayout / ticket.Stake);

                        foreach (Quote q in Sweat(run.Sweats[i], ticket))
                        {
                            double overPayout = q.Offer / ticket.PotentialPayout;
                            double overStake = q.Offer / ticket.Stake;
                            maxOfferOverPayout = Math.Max(maxOfferOverPayout, overPayout);
                            maxOfferOverStake = Math.Max(maxOfferOverStake, overStake);
                            minOfferOverStake = Math.Min(minOfferOverStake, overStake);
                            if (q.Offer >= ticket.PotentialPayout) atOrAbovePayout++;
                            if (q.Offer >= ticket.Stake) atOrAboveStake++;
                            if (quotes == 0 && seed == 0) firstOffer = q.Offer;
                            quotes++;

                            // THE DEGENERACY QUESTION, asserted rather than merely tabulated.
                            Assert.True(q.Offer < ticket.PotentialPayout,
                                $"κ={kappa} margin={margin}: a quote of {q.Offer:R} reached the "
                                + $"payout {ticket.PotentialPayout:R}");
                            Assert.True(q.Offer > 0.0 && !double.IsNaN(q.Offer)
                                && !double.IsInfinity(q.Offer),
                                $"κ={kappa} margin={margin}: degenerate quote {q.Offer:R}");
                        }
                    }
                }

                soldAt[kappa] = sold;
                if (margin == 0.08) offerAtKappa[kappa] = firstOffer;

                _output.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "{0,5:F2}  {1,6:F2} | {2,4}  {3,6} | {4,11:F6}  {5,13:F4}  {6,13:F6}  {7,13:F4} |"
                    + " {8,8}  {9,7}",
                    kappa, margin, sold, quotes, maxOfferOverPayout, maxOfferOverStake,
                    minOfferOverStake, minPayoutOverStake, atOrAbovePayout, atOrAboveStake));
            }
        }

        // The separability finding: κ scales a SAME MATCH price by exactly 1/κ, and the cash-out
        // margin is a separate multiplicative factor, so the two dials compose rather than interact —
        // ON THE TICKETS THAT STILL SELL. Whether a slip sells at all is where the real interaction
        // lives, which is what the `sold` column is for.
        _output.WriteLine("");
        _output.WriteLine("kappa | tickets sold | offer(kappa) x kappa / offer(1) at margin 0.08");
        double baseline = offerAtKappa[1.0];
        foreach (double kappa in kappas)
            _output.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "{0,5:F2} | {1,12} | {2:F12}", kappa, soldAt[kappa],
                offerAtKappa[kappa] * kappa / baseline));
    }
}
