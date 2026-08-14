using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBR.Engine;
using Xunit;

namespace SBR.Engine.Tests;

/// <summary>
/// Step 4 of F_0.6.0 — the assumptions the SAME MATCH gate probe (<c>sim/SameMatchStrategy.cs</c>)
/// rests on, pinned at engine level.
///
/// <para><b>Why the shapes are restated here.</b> <c>engine.tests</c> references the engine and
/// nothing else, so these tests cannot construct the probe. They pin the two things that would make
/// the probe silently stop covering the feature while still reporting green: that each catalogue
/// pair still carries the relation kind it was chosen for, and that each deliberately invalid slip
/// still refuses with the rule it was aimed at and hands back a remedy that places. The campaign's
/// exposure table is the live check; this is the one that fails in a second.</para>
///
/// <para><b>Every assertion here is DRAW-INDEPENDENT, on purpose.</b> Lane 1 is adding draws to
/// <c>MatchModel</c> while this lands, and a label that depends on the board being draw-free would
/// become a landmine under their merge. Concretely: on today's board BTTS yes forces at least 2-1
/// and therefore ENTAILS over 2.5, so that pair is an <see cref="RelationKind.Implies"/> today and a
/// <see cref="RelationKind.SharedScoreline"/> once 1-1 is legal. The probe's catalogue deliberately
/// does not use it for SharedScoreline, and nothing here asserts its label.</para>
///
/// <para><c>Catalogue_*</c> is one test per relation kind the probe claims to exercise;
/// <c>Refusal_*</c> is the invalid slips and their verified remedies; <c>Independence_*</c> is the
/// assumption the whole gate campaign rests on — that adding a batch cannot move another batch's
/// numbers.</para>
/// </summary>
public class SameMatchProbeTests
{
    /// <summary>Bank and ticket caps lifted so one round can hold the whole catalogue; every other
    /// dial — lines, rates, κ — stays exactly as the board ships, because those are what the labels
    /// are being read off.</summary>
    private static RunConfig ProbeConfig() => new RunConfig
    {
        StartingBank = 1_000_000,
        MaxTicketsPerRound = 1_000,
    };

    /// <summary>The shortest-priced HOME scorer, chosen the way the probe chooses it: off the
    /// offered odds, which is public information, never off the roster's scoring weights.</summary>
    private static MarketSelection ShortestHomeScorer(Matchup m)
    {
        MarketSelection? best = null;
        double bestOdds = double.MaxValue;
        foreach (MarketOffer offer in m.Markets)
        {
            if (offer.Selection.Kind != MarketKind.AnytimeScorer) continue;
            if (m.PlayerSide(offer.Selection.PlayerIndex) != Side.Home) continue;
            if (offer.Odds >= bestOdds) continue;
            bestOdds = offer.Odds;
            best = offer.Selection;
        }
        Assert.NotNull(best);
        return best!.Value;
    }

    /// <summary>Places the pair and returns the ONE relation the model labelled on it. Goes through
    /// the real <c>PlaceTicket</c> rather than the joint model directly: what the probe covers is
    /// what the engine SELLS, and a pair that prices but will not place covers nothing.</summary>
    private static Relation SoleRelationOf(Run run, params MarketSelection[] selections)
    {
        Pick[] picks = selections.Select(s => new Pick(0, s)).ToArray();
        Assert.Null(run.RefusalFor(picks));
        Ticket ticket = run.PlaceTicket(picks, 100);
        Assert.NotNull(ticket.SameMatch);
        return Assert.Single(ticket.SameMatch!.Relations);
    }

    // =======================================================================================
    // CATALOGUE — one pair per relation kind the probe claims to exercise.
    // =======================================================================================

    /// <summary>A nested pair: OVER the higher line entails OVER the lower one, so p_joint = min p.
    /// Pure set containment, so no board change can reach it.</summary>
    [Fact]
    public void Catalogue_the_nested_goal_pair_is_an_implication()
    {
        var run = new Run("sgp-probe-implies", ProbeConfig());
        RunConfig cfg = run.Config;
        Relation r = SoleRelationOf(run,
            MarketSelection.TotalGoals(cfg.GoalLines[1], true),
            MarketSelection.TotalGoals(cfg.GoalLines[0], true));

        Assert.Equal(RelationKind.Implies, r.Kind);
        // Legs[0] implies Legs[1] — the higher line is the implying one, and it was picked first.
        Assert.Equal(new[] { 0, 1 }, r.Legs);
    }

    /// <summary>A scorer beside his own side's moneyline: the same goals settle both. Structural —
    /// the label follows from one leg being a scorer and the other reading that side's goals.</summary>
    [Fact]
    public void Catalogue_a_scorer_beside_his_own_moneyline_is_scorer_of_side()
    {
        var run = new Run("sgp-probe-scorer", ProbeConfig());
        Relation r = SoleRelationOf(run,
            ShortestHomeScorer(run.CurrentSlate.Matchups[0]),
            MarketSelection.Moneyline(Side.Home));

        Assert.Equal(RelationKind.ScorerOfSide, r.Kind);
        Assert.Equal(Side.Home, r.ScorerSide);
        Assert.Equal(SelectionFamily.Goal, r.Family);
    }

    /// <summary>Corners beside cards: one matchup, two families, no shared draw. This is the only
    /// kind in the catalogue with a single source, which is why the probe's rotation gives it the
    /// first and best-funded round.</summary>
    [Fact]
    public void Catalogue_a_cross_family_pair_is_independent()
    {
        var run = new Run("sgp-probe-independent", ProbeConfig());
        RunConfig cfg = run.Config;
        Relation r = SoleRelationOf(run,
            MarketSelection.TotalCorners(cfg.CornerLines[0], true),
            MarketSelection.TotalCards(cfg.CardLines[0], true));

        Assert.Equal(RelationKind.Independent, r.Kind);
        // Independent AND uncorrelated: a cross-family pair the model found correlation on but could
        // not label is exactly the no-label fallback, and the probe must never be the thing that
        // fires it.
        Assert.False(r.Correlated);
    }

    /// <summary>The moneyline beside BTTS: two GOAL-family legs, neither entailing the other, on any
    /// board. A home win does not require the away side to score, and both sides scoring does not
    /// pick a winner — which is why the catalogue uses THIS pair for the shape and not OVER + BTTS.</summary>
    [Fact]
    public void Catalogue_the_moneyline_beside_btts_is_a_shared_scoreline()
    {
        var run = new Run("sgp-probe-scoreline", ProbeConfig());
        Relation r = SoleRelationOf(run,
            MarketSelection.Moneyline(Side.Home),
            MarketSelection.BothTeamsToScore(true));

        Assert.Equal(RelationKind.SharedScoreline, r.Kind);
        Assert.Equal(SelectionFamily.Goal, r.Family);
        Assert.Null(r.ScorerSide);
    }

    /// <summary>A corners BAND — two legs of one COUNT family, neither impossible nor an
    /// implication. This is the shape SharedCount was added to the vocabulary for.</summary>
    [Fact]
    public void Catalogue_a_corner_band_is_a_shared_count()
    {
        var run = new Run("sgp-probe-band", ProbeConfig());
        RunConfig cfg = run.Config;
        Relation r = SoleRelationOf(run,
            MarketSelection.TotalCorners(cfg.CornerLines[0], true),
            MarketSelection.TotalCorners(cfg.CornerLines[^1], false));

        Assert.Equal(RelationKind.SharedCount, r.Kind);
        Assert.Equal(SelectionFamily.Corner, r.Family);
    }

    /// <summary>The probe's 3-leg composite carries SEVERAL relations, which is the only thing in the
    /// campaign that makes the model NOMINATE one. Without it `principal` ships unexercised outside
    /// unit tests.</summary>
    [Fact]
    public void Catalogue_the_composite_carries_several_relations_and_nominates_a_principal()
    {
        var run = new Run("sgp-probe-composite", ProbeConfig());
        RunConfig cfg = run.Config;
        Pick[] picks =
        {
            new Pick(0, ShortestHomeScorer(run.CurrentSlate.Matchups[0])),
            new Pick(0, MarketSelection.Moneyline(Side.Home)),
            new Pick(0, MarketSelection.TotalGoals(cfg.GoalLines[0], true)),
        };
        Assert.Null(run.RefusalFor(picks));
        Ticket ticket = run.PlaceTicket(picks, 100);

        Assert.NotNull(ticket.SameMatch);
        Assert.Equal(3, ticket.SameMatch!.Relations.Count); // three legs, three pairs
        Assert.NotNull(ticket.SameMatch.Principal);
        // Three legs means a 2^3 survivor table, so a second and third void are lookups too.
        Assert.Equal(8, ticket.LockedSubsetPrices.Count);
    }

    // =======================================================================================
    // MARKET-KIND ROLL-CALL — the probe's second catalogue, restated.
    // =======================================================================================

    /// <summary>The probe's MARKET-KIND catalogue, restated leg for leg: the six composites T2 rotates
    /// over. Same restating discipline as the relation pairs above, and the same reason —
    /// <c>engine.tests</c> cannot construct the probe.</summary>
    private static List<MarketSelection[]> KindCatalogue(Run run, Matchup m)
    {
        RunConfig cfg = run.Config;
        var entries = new List<MarketSelection[]>
        {
            // 0 — the scorer composite.
            new[]
            {
                ShortestHomeScorer(m),
                MarketSelection.Moneyline(Side.Home),
                MarketSelection.TotalGoals(cfg.GoalLines[0], true),
            },
            // 2 — the result spine: a one-goal home win, said four ways.
            new[]
            {
                MarketSelection.DoubleChance(MarketChoice.HomeOrDraw),
                MarketSelection.Handicap(Side.Away, cfg.HandicapLines[0]),
                MarketSelection.WinningMargin(1),
                MarketSelection.Moneyline(Side.Home),
            },
            // 3 — the count spine: each count family at both scopes.
            new[]
            {
                MarketSelection.TeamTotalCorners(Side.Home, cfg.TeamCornerLines[0], true),
                MarketSelection.TotalCorners(cfg.CornerLines[^1], true),
                MarketSelection.TeamTotalCards(Side.Home, cfg.TeamCardLines[0], true),
                MarketSelection.TotalCards(cfg.CardLines[0], true),
            },
            // 5 — the team-goals spine: home 2+ and away 1+, which entail the BTTS and the OVER.
            new[]
            {
                MarketSelection.TeamTotalGoals(Side.Home, cfg.TeamGoalLines[1], true),
                MarketSelection.TeamTotalGoals(Side.Away, cfg.TeamGoalLines[0], true),
                MarketSelection.BothTeamsToScore(true),
                MarketSelection.TotalGoals(cfg.GoalLines[1], true),
            },
        };

        // 1 — the correct score, and the three things one cell already decides.
        MarketSelection row = CorrectScoreRow(m);
        int h = row.ScoreHome, a = row.ScoreAway;
        entries.Add(new[]
        {
            row,
            MarketSelection.Moneyline(h > a ? MatchResult.Home : h < a ? MatchResult.Away : MatchResult.Draw),
            MarketSelection.TotalGoalsOddEven((h + a) % 2 == 1),
            MarketSelection.BothTeamsToScore(h > 0 && a > 0),
        });

        // 4 — the multi scorer beside what his second goal implies. Its board is floor-truncated, so
        // the row is read off the matchup the caller chose for exactly that reason.
        MarketSelection multi = ShortestMultiScorer(m)
            ?? throw new InvalidOperationException("caller must pass a matchup that offers a 2+ scorer");
        Side side = m.PlayerSide(multi.PlayerIndex);
        entries.Add(new[]
        {
            multi,
            MarketSelection.AnytimeScorer(multi.PlayerIndex),
            MarketSelection.TeamTotalGoals(side, cfg.TeamGoalLines[1], true),
            MarketSelection.Moneyline(side),
        });

        return entries;
    }

    /// <summary>
    /// EVERY SHIPPED MARKET KIND REACHES A SAME-MATCH TICKET — the unit-speed twin of G7-SGP's
    /// per-kind arm, and the tripwire the gap this catalogue was extended to close went through.
    ///
    /// <para>The board grew from six kinds to fifteen when the draws merge landed, and the probe kept
    /// building tickets out of the original six. Nothing went red, because the campaign asserted that
    /// same-match tickets were placed — never that the markets on them were the markets that shipped.
    /// This test fails the moment a sixteenth kind is added, which is the point: the roll-call is
    /// taken off <see cref="MarketKind"/> itself, so it cannot be satisfied by a catalogue that
    /// stopped tracking the board.</para>
    ///
    /// <para>It also pins the two things that make the coverage real rather than nominal: every
    /// composite PLACES (a slip the board refuses covers nothing, and the restrictive kinds — a
    /// correct score pins one cell, a double chance overlaps the moneyline — are exactly the ones a
    /// naive catalogue gets refused on), and none of them prices at the NO-LABEL FALLBACK, which is
    /// the silent money leak G7-SGP's third half asserts zero of.</para>
    /// </summary>
    [Fact]
    public void Catalogue_every_shipped_market_kind_reaches_a_same_match_ticket()
    {
        var run = new Run("sgp-probe-kinds", ProbeConfig());
        Matchup m = run.CurrentSlate.Matchups.First(x => ShortestMultiScorer(x) != null);

        var covered = new HashSet<MarketKind>();
        var slips = new List<MarketSelection[]>(KindCatalogue(run, m));

        // The relation catalogue and the refusal remedies are coverage too — the arm counts legs on
        // placed tickets, not legs from one catalogue — so the roll-call is taken over everything the
        // probe actually sells.
        RunConfig cfg = run.Config;
        slips.Add(new[]
        {
            MarketSelection.TotalCorners(cfg.CornerLines[0], true),
            MarketSelection.TotalCards(cfg.CardLines[0], true),
        });

        foreach (MarketSelection[] slip in slips)
        {
            Pick[] picks = slip.Select(s => new Pick(m.Index, s)).ToArray();
            Assert.Null(run.RefusalFor(picks));
            Ticket ticket = run.PlaceTicket(picks, 100);
            Assert.NotNull(ticket.SameMatch);
            // A labelled price, not the product of the legs. The fallback is the +274% leak.
            Assert.False(ticket.SameMatch!.NaiveFallback);
            foreach (Leg leg in ticket.Legs) covered.Add(leg.Selection.Kind);
        }

        MarketKind[] missing = Enum.GetValues<MarketKind>().Where(k => !covered.Contains(k)).ToArray();
        Assert.True(missing.Length == 0,
            "market kinds the probe never puts in a same-match ticket: "
            + string.Join(", ", missing));
    }

    /// <summary>The shortest-priced 2+ scorer offered, or null where the ratified 2% floor truncated
    /// the whole multi-scorer board for this matchup — a real state, which is why the probe's entry
    /// stands down on it rather than constructing a row the board never sold.</summary>
    private static MarketSelection? ShortestMultiScorer(Matchup m)
    {
        MarketSelection? best = null;
        double bestOdds = double.MaxValue;
        foreach (MarketOffer offer in m.Markets)
        {
            if (offer.Selection.Kind != MarketKind.PlayerMultiScorer) continue;
            if (offer.Odds >= bestOdds) continue;
            bestOdds = offer.Odds;
            best = offer.Selection;
        }
        return best;
    }

    /// <summary>The first offered correct-score cell — the probe rotates over them by matchup index;
    /// what is pinned here is that a cell's own implications place, which holds for every row.</summary>
    private static MarketSelection CorrectScoreRow(Matchup m)
    {
        MarketOffer? row = m.Markets.FirstOrDefault(o => o.Selection.Kind == MarketKind.CorrectScore);
        Assert.NotNull(row);
        return row!.Selection;
    }

    // =======================================================================================
    // REFUSAL — the invalid slips, and remedies that are spent rather than described.
    // =======================================================================================

    /// <summary>The impossible slip. BTTS yes needs both sides to score — at least two goals — so it
    /// cannot sit beside UNDER the lowest goal line on any board, drawn or not. The remedy drops the
    /// leg added LAST, which is what leaves a same-match pair standing rather than a single.</summary>
    [Fact]
    public void Refusal_the_impossible_slip_refuses_and_its_remedy_places()
    {
        var run = new Run("sgp-probe-impossible", ProbeConfig());
        RunConfig cfg = run.Config;
        Pick[] picks =
        {
            new Pick(0, MarketSelection.TotalGoals(cfg.GoalLines[1], true)),
            new Pick(0, MarketSelection.BothTeamsToScore(true)),
            new Pick(0, MarketSelection.TotalGoals(cfg.GoalLines[0], false)),
        };

        TicketRefusedException ex =
            Assert.Throws<TicketRefusedException>(() => run.PlaceTicket(picks, 100));
        Assert.Equal(RefusalKind.ImpossibleCombination, ex.Refusal.Kind);
        Assert.Equal(0.0, ex.Refusal.Price);
        Assert.True(ex.Refusal.HasRemedy);
        Assert.Equal(new[] { 2 }, ex.Refusal.RemedyLegs);

        // Nothing was sold: a refusal runs before any state changes, so the slot is still free.
        Assert.Empty(run.Tickets);

        Ticket remedied = run.PlaceTicket(Without(picks, ex.Refusal.RemedyLegs), 100);
        Assert.NotNull(remedied.SameMatch); // still SAME MATCH — the slot buys coverage, not a single
    }

    /// <summary>The duplicate slip. The joint is idempotent over a repeated selection while the
    /// per-leg margin is not, which is the whole reason the rule exists; the remedy again drops the
    /// last leg and leaves the corners band standing.</summary>
    [Fact]
    public void Refusal_the_duplicate_slip_refuses_and_its_remedy_places()
    {
        var run = new Run("sgp-probe-duplicate", ProbeConfig());
        RunConfig cfg = run.Config;
        Pick[] picks =
        {
            new Pick(0, MarketSelection.TotalCorners(cfg.CornerLines[0], true)),
            new Pick(0, MarketSelection.TotalCorners(cfg.CornerLines[^1], false)),
            new Pick(0, MarketSelection.TotalCorners(cfg.CornerLines[0], true)),
        };

        TicketRefusedException ex =
            Assert.Throws<TicketRefusedException>(() => run.PlaceTicket(picks, 100));
        Assert.Equal(RefusalKind.DuplicateSelection, ex.Refusal.Kind);
        Assert.Equal(new[] { 0, 2 }, ex.Refusal.CauseLegs);
        Assert.Equal(new[] { 2 }, ex.Refusal.RemedyLegs);
        Assert.Empty(run.Tickets);

        Ticket remedied = run.PlaceTicket(Without(picks, ex.Refusal.RemedyLegs), 100);
        Relation r = Assert.Single(remedied.SameMatch!.Relations);
        Assert.Equal(RelationKind.SharedCount, r.Kind);
    }

    /// <summary>The EXCLUSION slip — the one refusal cause on the board that is a set complement
    /// rather than an arithmetic conflict. "12" is precisely "not the draw", so the draw beside it
    /// wins on no outcome whatever; this is the overlap double chance was expected to produce once it
    /// joined the board, and it is unreachable without a three-way moneyline. The remedy drops the
    /// last leg and leaves 12 beside a corners leg, so the slot still sells a same-match ticket.</summary>
    [Fact]
    public void Refusal_the_double_chance_exclusion_refuses_and_its_remedy_places()
    {
        var run = new Run("sgp-probe-exclusion", ProbeConfig());
        RunConfig cfg = run.Config;
        Pick[] picks =
        {
            new Pick(0, MarketSelection.DoubleChance(MarketChoice.HomeOrAway)),
            new Pick(0, MarketSelection.TotalCorners(cfg.CornerLines[0], true)),
            new Pick(0, MarketSelection.Moneyline(MatchResult.Draw)),
        };

        TicketRefusedException ex =
            Assert.Throws<TicketRefusedException>(() => run.PlaceTicket(picks, 100));
        Assert.Equal(RefusalKind.ImpossibleCombination, ex.Refusal.Kind);
        // MINIMAL: the corners leg is innocent, and naming it would send a surface to block the wrong
        // control. The two result legs are the whole conflict.
        Assert.Equal(new[] { 0, 2 }, ex.Refusal.CauseLegs);
        Assert.True(ex.Refusal.HasRemedy);
        Assert.Empty(run.Tickets);

        Ticket remedied = run.PlaceTicket(Without(picks, ex.Refusal.RemedyLegs), 100);
        Assert.NotNull(remedied.SameMatch);
    }

    /// <summary>The FIXED-TOTAL slip. A correct-score cell settles the goal total exactly, so one side
    /// of the lowest goal line can never come in with it — which side depends on the cell, so the
    /// probe reads it off the row rather than assuming a scoreline. The remedy leaves the score
    /// beside the result it names.</summary>
    [Fact]
    public void Refusal_the_correct_score_total_conflict_refuses_and_its_remedy_places()
    {
        var run = new Run("sgp-probe-fixedtotal", ProbeConfig());
        RunConfig cfg = run.Config;
        MarketSelection row = CorrectScoreRow(run.CurrentSlate.Matchups[0]);
        int total = row.ScoreHome + row.ScoreAway;
        MatchResult result = row.ScoreHome > row.ScoreAway ? MatchResult.Home
            : row.ScoreHome < row.ScoreAway ? MatchResult.Away : MatchResult.Draw;
        Pick[] picks =
        {
            new Pick(0, row),
            new Pick(0, MarketSelection.Moneyline(result)),
            new Pick(0, MarketSelection.TotalGoals(cfg.GoalLines[0], total < cfg.GoalLines[0])),
        };

        TicketRefusedException ex =
            Assert.Throws<TicketRefusedException>(() => run.PlaceTicket(picks, 100));
        Assert.Equal(RefusalKind.ImpossibleCombination, ex.Refusal.Kind);
        Assert.Equal(new[] { 0, 2 }, ex.Refusal.CauseLegs);
        Assert.Equal(new[] { 2 }, ex.Refusal.RemedyLegs);
        Assert.Empty(run.Tickets);

        Ticket remedied = run.PlaceTicket(Without(picks, ex.Refusal.RemedyLegs), 100);
        Relation r = Assert.Single(remedied.SameMatch!.Relations);
        // A cell entails its own result — the score is the implying leg, and it was picked first.
        Assert.Equal(RelationKind.Implies, r.Kind);
        Assert.Equal(new[] { 0, 1 }, r.Legs);
    }

    /// <summary>The sub-evens rule is the one the campaign CANNOT reach: at the shipped κ = 1 no
    /// combination on this board prices at or below evens. Asserted rather than assumed, because
    /// "unreachable" is what the probe's own doc comment and the report's exposure line both claim,
    /// and an unstated absence is indistinguishable from a hole.</summary>
    [Fact]
    public void Refusal_sub_evens_is_out_of_reach_at_the_shipped_kappa()
    {
        var run = new Run("sgp-probe-subevens", ProbeConfig());
        RunConfig cfg = run.Config;
        Assert.Equal(1.0, cfg.SgpMargin);

        Matchup m = run.CurrentSlate.Matchups[0];
        MarketSelection[] board = m.Markets.Select(o => o.Selection).ToArray();
        for (int i = 0; i < board.Length; i++)
            for (int j = i + 1; j < board.Length; j++)
            {
                TicketRefusal? refusal = run.RefusalFor(new[] { new Pick(0, board[i]), new Pick(0, board[j]) });
                if (refusal != null)
                    Assert.NotEqual(RefusalKind.SubEvens, refusal.Kind);
            }
    }

    // =======================================================================================
    // INDEPENDENCE — the assumption the gate campaign rests on.
    // =======================================================================================

    /// <summary>
    /// ADDING A BATCH CANNOT MOVE ANOTHER BATCH'S NUMBERS. The campaign gains a SAME MATCH probe
    /// batch, and G1–G6 are read off other bots in the same process — so the claim that the probe is
    /// invisible to them is load-bearing, and it is a claim about SHARED STATE, not about seeds.
    ///
    /// <para>The seed half is true by construction (run i takes seed "prefix-i" and builds its own
    /// Run and its own generator). The half that could actually fail is the engine holding process
    /// -wide state that a preceding workload mutates — so this plays a reference run to a full
    /// signature, runs a same-match workload through the very paths the probe uses (joint pricing,
    /// both refusal rules, a Mulligan void re-priced off the locked subset table), replays the
    /// reference run, and demands the same bytes back.</para>
    /// </summary>
    [Fact]
    public void Independence_a_same_match_workload_cannot_perturb_a_later_run()
    {
        string before = PlayoutSignature("SGP-INDEPENDENCE");
        int voided = SameMatchWorkload();
        string after = PlayoutSignature("SGP-INDEPENDENCE");

        Assert.True(voided > 0, "the workload must actually void a leg, or it proves less than it claims");
        Assert.Equal(before, after);
    }

    /// <summary>A whole run under a fixed scripted policy, rendered as bytes. Deliberately reads the
    /// bank, every ticket's terminal state and payout, the resolved scorelines and the shop's dealt
    /// hand — four independent consumers of the run's generators, so a perturbation anywhere in the
    /// stream shows up here rather than hiding in a number nobody sampled.</summary>
    private static string PlayoutSignature(string seed)
    {
        var run = new Run(seed, new RunConfig());
        var sb = new StringBuilder();
        while (true)
        {
            sb.Append($"R{run.Round} bank={run.Bank.ToString("F8")} ");

            // The scripted policy backs two moneylines every round, and since draws landed those lose
            // often enough to leave a bank that cannot fund the minimum stake while the run is still
            // alive. Stopping here is part of the signature, not an escape from it: the bank is already
            // written above, so a run that ends by attrition still renders to distinct bytes.
            if (run.Bank < run.Config.MinStake) return sb.ToString();

            run.PlaceTicket(new[] { new Pick(0, Side.Home), new Pick(1, Side.Away) }, run.Config.MinStake);
            run.LockRound();
            run.FastForwardRound();
            foreach (Ticket t in run.Tickets)
                sb.Append($"[{t.Id} {t.State} {t.PotentialPayout:F8}]");
            foreach (Matchup m in run.CurrentSlate.Matchups)
                sb.Append($"({m.StatLine!.HomeGoals}-{m.StatLine.AwayGoals}/{m.StatLine.HomeCorners}/{m.StatLine.HomeCards})");
            run.Settle();
            sb.Append($" -> {run.Phase} {run.Bank.ToString("F8")}\n");
            if (run.Phase != Phase.Shop) return sb.ToString();
            foreach (RelicDefinition o in run.ShopOffers) sb.Append($"{o.Id};");
            foreach (ConsumableDefinition o in run.ConsumableOffers) sb.Append($"{o.Id};");
            sb.Append('\n');
            run.ExitShop();
        }
    }

    /// <summary>The probe's paths, run for their side effects only: same-match pricing, both refusal
    /// rules, and a Mulligan void that re-prices off the locked survivor table. Returns how many legs
    /// it actually voided, so the test can refuse to pass on a workload that did nothing.</summary>
    private static int SameMatchWorkload()
    {
        int voided = 0;
        for (int seedIndex = 0; seedIndex < 8; seedIndex++)
        {
            var run = new Run($"SGP-WORKLOAD-{seedIndex}", ProbeConfig());
            RunConfig cfg = run.Config;
            run.GrantConsumable(RelicCatalog.Consumables.First(c => c.Id == "mulligan_slip"));

            // Priced and sold on the joint.
            run.PlaceTicket(new[]
            {
                new Pick(0, MarketSelection.TotalGoals(cfg.GoalLines[1], true)),
                new Pick(0, MarketSelection.TotalGoals(cfg.GoalLines[0], true)),
                new Pick(0, MarketSelection.BothTeamsToScore(true)),
            }, 100);

            // Refused — both rules, through the throwing path a betslip actually takes.
            Assert.Throws<TicketRefusedException>(() => run.PlaceTicket(new[]
            {
                new Pick(1, MarketSelection.BothTeamsToScore(true)),
                new Pick(1, MarketSelection.TotalGoals(cfg.GoalLines[0], false)),
            }, 100));
            Assert.Throws<TicketRefusedException>(() => run.PlaceTicket(new[]
            {
                new Pick(1, MarketSelection.Moneyline(Side.Home)),
                new Pick(1, MarketSelection.Moneyline(Side.Home)),
            }, 100));

            run.LockRound();
            SweatSession sweat = run.Sweats[0];
            while (sweat.HasPendingLoss || sweat.MoveNext(out _))
            {
                if (!sweat.HasPendingLoss) continue;
                if (run.OwnsConsumable("mulligan_slip") && sweat.CanMulliganPendingLoss
                    && run.Tickets[0].State == TicketState.Open)
                {
                    run.PlayMulliganSlip(sweat);
                    voided++;
                    continue;
                }
                sweat.DeclinePendingLoss();
                break;
            }
            run.FastForwardRound(); // drains whatever the mulligan loop left and finishes the sweat
            run.Settle();
        }
        return voided;
    }

    /// <summary>The picks that survive a remedy — the ticket the refusal says will place.</summary>
    private static Pick[] Without(IReadOnlyList<Pick> picks, IReadOnlyList<int> drop)
        => picks.Where((_, i) => !drop.Contains(i)).ToArray();
}
