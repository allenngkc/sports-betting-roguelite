using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// SAME MATCH — the F_0.6.0 coverage probe, and the bot G7's SGP arm reads.
///
/// <para><b>Why this is a dedicated bot and not a change to the skilled bot.</b> It follows from the
/// joint model's own EV-parity property: at the shipped <c>κ = 1</c> a same-match ticket is
/// EV-identical to an equivalent independent parlay, so <i>every rational bot is indifferent and
/// will never build one on merit</i>. Teaching the skilled bot to build them anyway would inject
/// arbitrary behaviour into the primary economy measurement (G1–G6 read the skilled bot) purely to
/// light up a gate — gaming the gate, not covering the feature. <see cref="MartyrStrategy"/> is the
/// established precedent: a bot that exists to exercise one path for one gate. This is the second.
/// G1–G6 never see it; it is its own batch, and a batch derives every seed from the prefix.</para>
///
/// Policy — each betting window it spends its three ticket slots on:
///   • T1 — a rotating 2-leg PAIR from the five-shape catalogue below, so the relation vocabulary is
///     exercised rather than one case repeated. The rotation advances two shapes per round, so a run
///     that dies at the usual round 4–5 has still walked all five.
///   • T2 — a 3-leg COMPOSITE (a scorer, his side's moneyline, and the goals line all three read):
///     several relations on one ticket, which is the only thing that exercises the model's
///     <c>principal</c> ranking, and an 8-entry survivor-subset table instead of a 4-entry one.
///   • T3 — a deliberately INVALID slip, alternating impossible / duplicate by round, placed through
///     <c>PlaceTicket</c> so the thrown refusal is exercised as a surface would meet it — then
///     re-placed as the refusal's own REMEDY. The remedy set is chosen so what survives is itself a
///     same-match ticket, so the slot buys refusal coverage AND a placed ticket.
///
/// <para><b>The refusal path is live product behaviour and a campaign that never trips it has not
/// covered it.</b> Both routes to the verdict are used, each where it reads better: T3 provokes the
/// exception because that is the path a betslip actually takes, while T1/T2 are pre-checked with the
/// non-throwing <see cref="Run.RefusalFor"/> — a probe that tripped over an unintended refusal would
/// silently lose the coverage it meant to have, so it asks first and counts anything it finds as
/// UNEXPECTED (a defect signal, reported apart from the refusals it provoked on purpose).</para>
///
/// <para><b>Never cashes out</b> — Allen's ruling, not a preference. Same-match cash-out is still
/// naive-priced (phase 4, deferred), so a cash-out would feed a known-wrong number into the
/// campaign. <see cref="ControlsSweat"/> stays true only so the pending-loss window is offered.</para>
///
/// <para><b>Shops for Mulligan Slips and takes them.</b> Void re-pricing is the riskiest path step 3
/// added and is otherwise proven only by unit tests; a probe that never voids leaves it unexercised
/// end to end. It buys nothing else, so the relic confound is one consumable wide — and it does not
/// matter regardless: this is a coverage instrument, not an economy gate.</para>
///
/// <para><b>Out of reach at κ = 1:</b> the sub-evens replacement and its full-ticket refund need
/// κ ≳ 1.3, so that path stays unit-test-only in this campaign. Stated, not quietly uncovered.</para>
///
/// HONESTY: reads odds, the board's player sides and the bank only — like every bot.
/// </summary>
public sealed class SameMatchStrategy : IStrategy
{
    /// <summary>Stake as a fraction of bank. Small on purpose: the probe's coverage is measured in
    /// ROUNDS SURVIVED × tickets, so it stakes to stay alive and to accrue the comps a Mulligan
    /// costs, not to win. It reserves the whole payment for the same reason.</summary>
    private const double StakeFraction = 0.04;

    /// <summary>How many pair shapes the catalogue carries — one per relation kind the model can
    /// label on a ticket it will actually sell (MutuallyExclusive is only ever a refusal).</summary>
    private const int ShapeCount = 5;

    public string Name => "samematch";

    /// <summary>True only so the pending-loss window is offered — <see cref="ShouldCashOut"/> is a
    /// constant false. A bot that opts out of sweat control is never asked about a dying leg, and the
    /// Mulligan is the whole point of this one.</summary>
    public bool ControlsSweat => true;

    /// <summary>NEVER — Allen's ruling. Same-match cash-out is naive-priced until phase 4, so taking
    /// one would put a known-wrong number into the campaign the probe exists to inform.</summary>
    public bool ShouldCashOut(Run run, Ticket ticket, SweatSession session, DramaEvent evt,
        double offer, double bankNow, double target, BotState state, Pcg32 rng) => false;

    /// <summary>Mulligan or decline, explicitly — the interface default would also fire a gifted
    /// Ref's Whistle, and a whistle re-rolls a grading instead of voiding a leg. The void is the
    /// coverage target, so the probe declines everything that is not one.</summary>
    public PendingLossAction ChoosePendingLossAction(Run run, Ticket ticket, SweatSession session,
        BotState state, Pcg32 rng)
        => run.OwnsConsumable("mulligan_slip") && session.CanMulliganPendingLoss
            ? PendingLossAction.Mulligan
            : PendingLossAction.Decline;

    public void Bet(Run run, BotState state, Pcg32 rng)
    {
        IReadOnlyList<Matchup> slate = run.CurrentSlate.Matchups;
        if (slate.Count < 3) return; // three tickets, three matchups — never two shapes on one match

        double budget = run.Bank - run.CurrentPayment; // the payment is reserved: rounds are coverage
        double stake = Math.Max(run.Config.MinStake, Math.Floor(StakeFraction * run.Bank));
        if (budget < stake) return;

        // A distinct matchup per ticket, rotating with the round so the probe does not spend every
        // run on matchup 1..3 — the board's per-match latents differ, and so do the joints.
        int baseIndex = (run.Round - 1) % slate.Count;
        Matchup m1 = slate[baseIndex];
        Matchup m2 = slate[(baseIndex + 1) % slate.Count];
        Matchup m3 = slate[(baseIndex + 2) % slate.Count];

        // T1 — the rotating pair: two shapes forward per round, so the catalogue is WALKED, not
        // sampled, and a run that reaches round 5 has seen all five.
        //
        // The +2 start is measured, not decorative. Rounds are not equally funded — the probe
        // reserves the payment, and by round 5 only a few percent of runs can still afford a third
        // ticket — so which shape lands in which round decides how hard it is exercised (at 200 runs
        // the round-5 shape placed 6 tickets against the round-1 shape's 200). This offset puts
        // INDEPENDENT in round 1, because it is the one kind with no second source: every other
        // shape is also produced by the composite or by a refusal remedy, so a starved late round
        // costs those nothing. A draw off the bot's generator was tried instead and measured worse —
        // it evens the SHAPES out, which flattens the kinds that had a second source down toward the
        // one that does not.
        int shape = ((run.Round - 1) * 2 + 2) % ShapeCount;
        budget = PlaceChecked(run, state, PairFor(shape, m1, run.Config), stake, budget);

        // T2 — the composite: several relations on one ticket, so `principal` has a ranking to do.
        budget = PlaceChecked(run, state, Composite(m2, run.Config), stake, budget);

        // T3 — the refusal probe, then its own verified remedy.
        PlaceRefusalProbe(run, state, m3, stake, budget);
    }

    /// <summary>Buys Mulligan Slips and NOTHING else. Every other item would be an economy confound
    /// bought for no coverage; the Mulligan is the void path's only route into a real run.</summary>
    public void Shop(Run run, BotState state, Pcg32 rng)
    {
        bool bought = true;
        while (bought)
        {
            bought = false;
            if (run.OwnedConsumables.Count >= run.Config.ConsumableSlots) return;
            for (int i = 0; i < run.ConsumableOffers.Count; i++)
            {
                if (run.ConsumableOffers[i].Id != "mulligan_slip") continue;
                if (run.ConsumableOffers[i].Price > run.Comps) continue;
                run.BuyConsumable(i);
                bought = true;
                break;
            }
        }
    }

    // ---- the shape catalogue -------------------------------------------------------------------

    /// <summary>One 2-leg slip per relation kind the model can label. Lines come from the config
    /// rather than from literals: a re-tune of GoalLines/CornerLines/CardLines otherwise leaves this
    /// probe asking for a selection the board no longer offers.</summary>
    private static List<Pick>? PairFor(int shape, Matchup m, RunConfig cfg)
    {
        if (cfg.GoalLines.Length < 2 || cfg.CornerLines.Length < 2 || cfg.CardLines.Length < 1)
            return null; // the board cannot express these shapes; place nothing rather than guess
        double gLo = cfg.GoalLines[0], gHi = cfg.GoalLines[1];
        double cLo = cfg.CornerLines[0], cHi = cfg.CornerLines[^1];
        double kLo = cfg.CardLines[0];
        int i = m.Index;

        switch (shape)
        {
            case 0: // Implies — a NESTED pair: OVER gHi entails OVER gLo, so p_joint = min p.
                return new List<Pick>
                {
                    new Pick(i, MarketSelection.TotalGoals(gHi, true)),
                    new Pick(i, MarketSelection.TotalGoals(gLo, true)),
                };
            case 1: // ScorerOfSide — a scorer beside his own team's goals (the moneyline reads them).
            {
                MarketSelection? scorer = ShortestHomeScorer(m);
                return scorer is not { } s
                    ? null
                    : new List<Pick>
                    {
                        new Pick(i, s),
                        new Pick(i, MarketSelection.Moneyline(Side.Home)),
                    };
            }
            case 2: // Independent — CROSS-FAMILY: corners beside cards, one match, no shared draw.
                return new List<Pick>
                {
                    new Pick(i, MarketSelection.TotalCorners(cLo, true)),
                    new Pick(i, MarketSelection.TotalCards(kLo, true)),
                };
            // SharedScoreline — two GOAL-family legs, NEITHER of which entails the other. The
            // moneyline beside BTTS is the pair that stays true to that on any board: a home win
            // does not require the away side to score, and both sides scoring does not pick a
            // winner. The obvious alternative — OVER gHi beside BTTS — used to be Implies rather
            // than this shape, because with no draws BTTS yes forced at least 2-1 and so entailed
            // OVER 2.5. LANE 1'S DRAWS LANDED (2026-08-13): 1-1 is legal, so that pair is now a
            // genuine SharedScoreline too — and this catalogue was written not to depend on which,
            // which is why the merge cost it no edit. (See the refusal probe, which leaves exactly
            // that pair standing on purpose.)
            case 3:
                return new List<Pick>
                {
                    new Pick(i, MarketSelection.Moneyline(Side.Home)),
                    new Pick(i, MarketSelection.BothTeamsToScore(true)),
                };
            default: // SharedCount — a corners BAND: two legs reading one COUNT family's draw.
                return new List<Pick>
                {
                    new Pick(i, MarketSelection.TotalCorners(cLo, true)),
                    new Pick(i, MarketSelection.TotalCorners(cHi, false)),
                };
        }
    }

    /// <summary>A scorer, his side's moneyline, and the goals line — three legs the same goals
    /// settle. Two ScorerOfSide relations and a SharedScoreline, so the model must NOMINATE one,
    /// which is the only exercise the principal ranking gets in a campaign.</summary>
    private static List<Pick>? Composite(Matchup m, RunConfig cfg)
    {
        if (cfg.GoalLines.Length < 1) return null;
        MarketSelection? scorer = ShortestHomeScorer(m);
        if (scorer is not { } s) return null;
        int i = m.Index;
        return new List<Pick>
        {
            new Pick(i, s),
            new Pick(i, MarketSelection.Moneyline(Side.Home)),
            new Pick(i, MarketSelection.TotalGoals(cfg.GoalLines[0], true)),
        };
    }

    /// <summary>The shortest-priced HOME scorer on the board, read off the offered odds — public
    /// information, unlike the roster's scoring weights. Null when the board carries no scorer
    /// market for the home side.</summary>
    private static MarketSelection? ShortestHomeScorer(Matchup m)
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
        return best;
    }

    // ---- placement -----------------------------------------------------------------------------

    /// <summary>Places a slip the probe expects to be legal, asking <see cref="Run.RefusalFor"/>
    /// first. A refusal here is NOT the coverage T3 provides — it means this catalogue built
    /// something the board will not sell, which is a defect in the probe or a movement in κ. It is
    /// counted apart so it can never hide inside the number G7's arm wants to be positive.</summary>
    private static double PlaceChecked(Run run, BotState state, List<Pick>? picks, double stake,
        double budget)
    {
        if (picks == null || budget < stake) return budget;

        TicketRefusal? refusal = run.RefusalFor(picks);
        if (refusal != null)
        {
            state.SameMatchUnexpectedRefusals++;
            Count(state, refusal.Kind);
            return budget;
        }

        run.PlaceTicket(picks, stake);
        return budget - stake;
    }

    /// <summary>The deliberately invalid slip, alternating the two refusal rules a κ = 1 board can
    /// actually reach, then placing the refusal's own remedy.
    ///
    /// <para>Goes through <see cref="Run.PlaceTicket"/> rather than the query: the thrown refusal is
    /// how a betslip meets this rule, and placement is atomic — nothing is spent and no ticket is
    /// added — so provoking it costs the probe nothing but the coverage it came for.</para>
    ///
    /// <para>Both slips are built so that DROPPING THE LAST LEG leaves a same-match pair: the remedy
    /// prefers the legs added last, so the corrected ticket is itself coverage rather than a
    /// consolation single. The remedy is applied as the engine reported it — indices into the leg
    /// list, which is pick order — and never re-derived here. What survives is an OVER gHi + BTTS
    /// pair (labelled Implies on the current draw-free board, SharedScoreline once draws land) and a
    /// corners band; the exposure table reports what the model actually labelled, so neither claim
    /// has to be maintained here.</para></summary>
    private static void PlaceRefusalProbe(Run run, BotState state, Matchup m, double stake,
        double budget)
    {
        if (budget < stake) return;
        RunConfig cfg = run.Config;
        if (cfg.GoalLines.Length < 2 || cfg.CornerLines.Length < 2) return;
        int i = m.Index;

        // Odd rounds: IMPOSSIBLE — BTTS yes needs two goals, UNDER the low line allows at most one.
        // Even rounds: DUPLICATE — the same corner line twice, which the joint is idempotent over.
        List<Pick> invalid = run.Round % 2 == 1
            ? new List<Pick>
            {
                new Pick(i, MarketSelection.TotalGoals(cfg.GoalLines[1], true)),
                new Pick(i, MarketSelection.BothTeamsToScore(true)),
                new Pick(i, MarketSelection.TotalGoals(cfg.GoalLines[0], false)),
            }
            : new List<Pick>
            {
                new Pick(i, MarketSelection.TotalCorners(cfg.CornerLines[0], true)),
                new Pick(i, MarketSelection.TotalCorners(cfg.CornerLines[^1], false)),
                new Pick(i, MarketSelection.TotalCorners(cfg.CornerLines[0], true)),
            };

        TicketRefusal refusal;
        try
        {
            run.PlaceTicket(invalid, stake);
            return; // NOT refused: the rule the probe was aiming at did not fire. Nothing to remedy.
        }
        catch (TicketRefusedException ex)
        {
            refusal = ex.Refusal;
        }

        state.SameMatchRefusals++;
        Count(state, refusal.Kind);
        if (!refusal.HasRemedy) return;

        var remedied = new List<Pick>(invalid.Count);
        for (int leg = 0; leg < invalid.Count; leg++)
            if (!Contains(refusal.RemedyLegs, leg))
                remedied.Add(invalid[leg]);
        if (remedied.Count == 0) return;

        // The remedy is VERIFIED by the engine — every set it emits was run back through all three
        // rules. Placed directly, so a remedy that did not actually fix the ticket would surface as
        // an unhandled refusal here rather than being quietly swallowed by a second pre-check.
        run.PlaceTicket(remedied, stake);
    }

    private static bool Contains(IReadOnlyList<int> legs, int leg)
    {
        for (int i = 0; i < legs.Count; i++) if (legs[i] == leg) return true;
        return false;
    }

    private static void Count(BotState state, RefusalKind kind)
        => state.SameMatchRefusalKinds[kind] =
            state.SameMatchRefusalKinds.TryGetValue(kind, out int n) ? n + 1 : 1;
}
