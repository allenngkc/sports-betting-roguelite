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
///   • T1 — a rotating 2-leg PAIR from the five-shape RELATION catalogue, so the relation vocabulary
///     is exercised rather than one case repeated. The rotation advances two shapes per round, so a
///     run that dies at the usual round 4–5 has still walked all five.
///   • T2 — a rotating COMPOSITE from the six-entry MARKET-KIND catalogue: 3–4 legs the same match
///     settles, several relations on one ticket (which is the only thing that exercises the model's
///     <c>principal</c> ranking and an 8- or 16-entry survivor-subset table), and between them every
///     shipped <see cref="MarketKind"/> on the board.
///   • T3 — a deliberately INVALID slip, rotating over four causes, placed through
///     <c>PlaceTicket</c> so the thrown refusal is exercised as a surface would meet it — then
///     re-placed as the refusal's own REMEDY. The remedy set is chosen so what survives is itself a
///     same-match ticket, so the slot buys refusal coverage AND a placed ticket.
///
/// <para><b>The two catalogues are separate axes and are rotated differently, on purpose.</b> T1 asks
/// "which RELATIONS did the model label", and five shapes fit inside the four-to-five rounds a run
/// survives, so it walks them by round from a measured offset (see <c>Bet</c>). T2 asks "which
/// MARKETS ever reached a joint", and fifteen kinds do not fit in five rounds however they are
/// packed — a round-only walk would hand the late entries whatever a handful of round-6 survivors
/// could afford. So T2 walks from a PER-RUN start drawn once off the bot's own generator: every
/// entry gets an equal share of the batch's best-funded round instead of one entry getting round 1
/// and another getting the tail. The earlier finding that a generator draw measured WORSE for T1
/// still holds and is not contradicted: it evens the shapes out, which hurt when some shapes had a
/// second source and one did not. Here evening out is the whole objective — most kinds have exactly
/// one source, so an even split is the best split.</para>
///
/// <para><b>The refusal path is live product behaviour and a campaign that never trips it has not
/// covered it.</b> Both routes to the verdict are used, each where it reads better: T3 provokes the
/// exception because that is the path a betslip actually takes, while T1/T2 are pre-checked with the
/// non-throwing <see cref="Run.RefusalFor"/> — a probe that tripped over an unintended refusal would
/// silently lose the coverage it meant to have, so it asks first and counts anything it finds as
/// UNEXPECTED (a defect signal, reported apart from the refusals it provoked on purpose).</para>
///
/// <para><b>It must place far more than it is refused.</b> T3 refuses exactly once per round and
/// sells its remedy, so the balance is structurally three placed to one refused; the restrictive new
/// kinds (a correct score pins the scoreline to one cell, a double chance overlaps the moneyline)
/// are used in combinations that are consistent BY CONSTRUCTION rather than by luck — the correct
/// score picks the moneyline, parity and BTTS legs that its own cell implies, and the double chance
/// is paired with the result it contains. A catalogue that leaned on the refusal path for coverage
/// would certify refusals and nothing else.</para>
///
/// <para><b>It cashes out — and it used not to.</b> The standing ruling was NEVER: same-match
/// cash-out was naive-priced (it ran the ordinary product expression over correlated legs), so a
/// cash-out would have fed a known-wrong number into the campaign the probe exists to inform. That
/// ruling was conditional on the defect, and <b>F_0.6.0 PHASE 4 REPAIRED THE PRICE</b> — a same-match
/// quote is now <c>payout × P(L ∧ U | S) × (liveProb / P(L | S))</c> off the same joint evaluator the
/// ticket was sold at, with the certainty carve-out on top (<c>SweatSession.ConditionalWinProb</c>,
/// <c>SameMatchCashOutTests</c>). The reason has expired, so the coverage gap closes: a campaign that
/// never takes a same-match cash-out leaves the newest priced path in the lane proven by unit tests
/// alone, which is precisely the hole this bot exists to fill for every other same-match path.
/// <see cref="ShouldCashOut"/> carries the policy and the balance it holds.</para>
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

    /// <summary>How many pair shapes the RELATION catalogue carries — one per relation kind the model
    /// can label on a ticket it will actually sell (MutuallyExclusive is only ever a refusal).</summary>
    private const int ShapeCount = 5;

    /// <summary>How many composites the MARKET-KIND catalogue carries. Between them, and with T1 and
    /// T3's remedies, every shipped <see cref="MarketKind"/> reaches a same-match ticket — which is
    /// the fact G7-SGP's per-kind arm asserts, so this number is a consequence of that criterion and
    /// not a free dial.</summary>
    private const int CompositeCount = 6;

    /// <summary>How many invalid-slip causes T3 rotates over.</summary>
    private const int RefusalVariants = 4;

    public string Name => "samematch";

    /// <summary>Both windows the sweat offers: the pending-loss one (where the Mulligan lands) and the
    /// cash-out one. A bot that opts out of sweat control is never asked about either.</summary>
    public bool ControlsSweat => true;

    /// <summary>How many ticket slots share one cash-out mark: exactly ONE of the round's three
    /// tickets is marked and the other two ride to settlement. A marked slot only fires if the ticket
    /// lives to its position, so the realised cash-out rate is lower again; see
    /// <see cref="ShouldCashOut"/> for why the balance is set from the settlement side.</summary>
    private const int MarkedSlotCycle = 3;

    /// <summary>The period of the POSITION walk, deliberately co-prime-ish with
    /// <see cref="MarkedSlotCycle"/> — at equal periods the two walks lock and the position becomes a
    /// pure function of the slot, which starves whichever position lands on a slot that cannot host
    /// it. Measured, not reasoned: at period 6 with a stride-2 slot walk the probe took 5,689
    /// cash-outs and NOT ONE of them was mid-sweat, because the only ticket with a mid position — the
    /// 3-to-4-leg composite — drew that mark solely in round 6, which almost no run reaches.</summary>
    private const int PositionCycle = 4;

    /// <summary>
    /// The cash-out policy — <b>coverage, not judgement</b>. This bot has no opinion about whether a
    /// quote is worth taking; the skilled bot's EV rule is the instrument for that and G1–G6 are where
    /// it is read. What this one owes the campaign is that the same-match cash-out path is REACHED,
    /// and reached at the places where the conditional's shape actually differs.
    ///
    /// <para><b>The balance is set from the settlement side.</b> A probe that cashed out of everything
    /// would cover cash-out and stop covering settlement, void re-pricing and the pending-loss window
    /// — three paths it is the only bot that covers — so cash-out gets the minority share. Two ticket
    /// slots in every <see cref="CashOutCycle"/> are MARKED, the other four ride to settlement; and
    /// because a marked slot only fires if the ticket lives to its position, the realised rate is
    /// lower again. The settled/cashed-out split is printed in report section 0b, which is where that
    /// balance is checked rather than assumed.</para>
    ///
    /// <para><b>Three positions, because one would be a single point on a curve.</b> The conditional
    /// is a different object at each: with nothing settled the quote is the ticket's own locked joint
    /// (the re-weight is 1); mid-sweat it is a genuine ratio of two joints over different leg sets;
    /// on the last leg the numerator and denominator share a leg set, so the quote walks to the full
    /// payout — and that is also the position where the certainty carve-out can fire, since a settled
    /// leg entailing the live one leaves <c>P(L | S) = 1</c>. EARLY and LATE are named in the marking;
    /// MID is what a LATE mark degrades to on a ticket whose last leg is never reached.</para>
    ///
    /// <para><b>Marked by a pure function of the ticket, never by a draw off <paramref name="rng"/>.</b>
    /// That generator is the same per-run stream the probe's MARKET-KIND catalogue start is drawn from
    /// and that <c>Shop</c> spends; drawing here would shift every later draw and silently re-cut the
    /// coverage rotations this bot exists to walk. The key is built from the round and the ticket's
    /// slot only — both stable, both already deterministic per seed.</para>
    ///
    /// <para>Ordinary tickets are left alone. The probe places none by construction, and the guard is
    /// here so a future catalogue entry that produced one could not quietly route this bot into the
    /// product path — which is not the path it was extended to cover.</para>
    /// </summary>
    public bool ShouldCashOut(Run run, Ticket ticket, SweatSession session, DramaEvent evt,
        double offer, double bankNow, double target, BotState state, Pcg32 rng)
    {
        if (ticket.SameMatch == null) return false;

        // A leg boundary is not a quoting position: the cursor has moved but the next event will
        // re-seed the live number, so the quote here is the previous leg's tail. Every other bot
        // skips it for the same reason and the probe keeps the convention.
        if (evt.Type == DramaEventType.LegFinal) return false;

        int slot = SlotOf(run, ticket);
        if (slot < 0) return false;

        // WHICH slot is marked walks the three slots with the round, so no shape is permanently the
        // one that cashes out; exactly one of the round's three tickets is marked.
        if ((run.Round + slot * 2) % MarkedSlotCycle != 0) return false;

        int cursor = evt.LegIndex;
        int lastLeg = ticket.Legs.Count - 1;

        // WHERE the marked ticket cashes out walks on its own period, so the two walks do not lock.
        // The allocation that falls out is the one worth having: rounds 1 and 2 — the only rounds
        // every run reaches — draw MID and LATE, which are the positions that need a ticket to
        // survive, while EARLY, which always fires, takes the starved later rounds.
        int position = run.Round % PositionCycle;
        if (position == 1 && ticket.Legs.Count < 3)
            position = 2; // no mid position exists on a 2-leg ticket; take the last leg instead

        return position switch
        {
            // MID — a leg settled and a leg still pending, so the conditional is a genuine ratio over
            // two different leg sets, and the only position where it is.
            1 => cursor >= 1 && cursor < lastLeg,
            // LATE — the last leg, where numerator and denominator share a leg set, the quote walks
            // to the full payout, and a settled leg that entails the live one makes the certainty
            // carve-out reachable. Fires only on tickets that survive that far, which is the point.
            2 => cursor == lastLeg,
            // EARLY — nothing has settled yet: the quote is the ticket's own locked joint, moved only
            // by how far the live number has drifted from the marginal it was sold at.
            _ => cursor == 0,
        };
    }

    /// <summary>Which of the round's ticket slots this ticket is, by reference — the ticket list is
    /// the round's, in placement order, so the slot is T1 / T2 / T3-remedy. −1 if it is not in the
    /// current round's list, which cannot happen from inside its own sweat but is not worth
    /// assuming.</summary>
    private static int SlotOf(Run run, Ticket ticket)
    {
        for (int i = 0; i < run.Tickets.Count; i++)
            if (ReferenceEquals(run.Tickets[i], ticket)) return i;
        return -1;
    }

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

        // T1 — the rotating relation pair: two shapes forward per round, so the catalogue is WALKED,
        // not sampled, and a run that reaches round 5 has seen all five.
        //
        // The +2 start is measured, not decorative. Rounds are not equally funded — the probe
        // reserves the payment, and by round 5 only a few percent of runs can still afford a third
        // ticket — so which shape lands in which round decides how hard it is exercised (at 200 runs
        // the round-5 shape placed 6 tickets against the round-1 shape's 200). This offset puts
        // INDEPENDENT in round 1, because it is the one kind with no second source: every other
        // shape is also produced by the composite or by a refusal remedy, so a starved late round
        // costs those nothing. A draw off the bot's generator was tried instead and measured worse —
        // it evens the SHAPES out, which flattens the kinds that had a second source down toward the
        // one that does not. (T2 below takes the opposite decision for the opposite reason; the two
        // catalogues answer different questions and the class comment sets out why.)
        int shape = ((run.Round - 1) * 2 + 2) % ShapeCount;
        budget = PlaceChecked(run, state, PairFor(shape, m1, run.Config), stake, budget);

        // T2 — the rotating kind composite: several relations on one ticket, so `principal` has a
        // ranking to do, and every market kind on the board reaches a joint across the batch.
        if (state.SameMatchSweepStart < 0) state.SameMatchSweepStart = rng.NextInt(0, CompositeCount);
        int entry = (state.SameMatchSweepStart + run.Round - 1) % CompositeCount;
        budget = PlaceChecked(run, state, Composite(entry, m2, run.Config), stake, budget);

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

    // ---- the RELATION catalogue (T1) ------------------------------------------------------------

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

    // ---- the MARKET-KIND catalogue (T2) ---------------------------------------------------------

    /// <summary>One composite per group of market kinds, each 3–4 legs the same match settles.
    ///
    /// <para><b>Every entry is consistent BY CONSTRUCTION, never by luck.</b> The restrictive kinds
    /// are the point of the extension and also its trap: a correct score pins the scoreline to one
    /// cell and a double chance overlaps the moneyline, so a catalogue that combined them naively
    /// would be refused far more often than sold and the campaign would certify the refusal path
    /// alone. So each entry DERIVES its siblings from the restrictive leg — the correct-score entry
    /// reads its own cell for the result, the parity and whether both teams scored; the margin entry
    /// picks a handicap and a double chance that both CONTAIN a one-goal home win. The pre-check in
    /// <see cref="PlaceChecked"/> is the alarm on that reasoning, not a substitute for it.</para>
    ///
    /// <para>Selections that the board TRUNCATES — correct-score cells and 2+ scorer rows both sit
    /// behind the ratified 2% probability floor — are read off <c>m.Markets</c> rather than
    /// constructed, because <c>Matchup.Odds</c> throws on a selection that was never offered and the
    /// truncation moves with the matchup's latents. Everything else is built from config lines, the
    /// same discipline the relation catalogue uses.</para></summary>
    private static List<Pick>? Composite(int entry, Matchup m, RunConfig cfg)
    {
        int i = m.Index;
        switch (entry)
        {
            // 0 — THE SCORER COMPOSITE (unchanged from the original catalogue): a scorer, his side's
            // moneyline, and the goals line — three legs the same goals settle. Two ScorerOfSide
            // relations and a SharedScoreline, so the model must NOMINATE one.
            case 0:
            {
                if (cfg.GoalLines.Length < 1) return null;
                MarketSelection? scorer = ShortestHomeScorer(m);
                return scorer is not { } s
                    ? null
                    : new List<Pick>
                    {
                        new Pick(i, s),
                        new Pick(i, MarketSelection.Moneyline(Side.Home)),
                        new Pick(i, MarketSelection.TotalGoals(cfg.GoalLines[0], true)),
                    };
            }

            // 1 — THE CORRECT SCORE, and the three things a single cell already decides: who won
            // (which is where the DRAW reaches a same-match slip, since a level cell names it), the
            // parity of the total, and whether both teams scored. Every one of those is an Implies
            // off the score, so the slip is legal on any board and its price is the score's own.
            case 1:
            {
                if (CorrectScoreRow(m, m.Index) is not { } row) return null;
                int h = row.ScoreHome, a = row.ScoreAway;
                MatchResult result = h > a ? MatchResult.Home : h < a ? MatchResult.Away : MatchResult.Draw;
                return new List<Pick>
                {
                    new Pick(i, row),
                    new Pick(i, MarketSelection.Moneyline(result)),
                    new Pick(i, MarketSelection.TotalGoalsOddEven((h + a) % 2 == 1)),
                    new Pick(i, MarketSelection.BothTeamsToScore(h > 0 && a > 0)),
                };
            }

            // 2 — THE RESULT SPINE: a one-goal home win, said four ways. The double chance CONTAINS
            // it (1X ⊇ 1), the away side's +line covers a one-goal loss, the margin bucket is exactly
            // one, and the moneyline is the result itself. Heavy overlap is deliberate — these four
            // kinds all read the same scoreline, which is precisely the correlation the joint exists
            // to price, and the naive product would sell it at a large multiple of its worth.
            case 2:
            {
                if (cfg.HandicapLines.Length < 1) return null;
                return new List<Pick>
                {
                    new Pick(i, MarketSelection.DoubleChance(MarketChoice.HomeOrDraw)),
                    new Pick(i, MarketSelection.Handicap(Side.Away, cfg.HandicapLines[0])),
                    new Pick(i, MarketSelection.WinningMargin(1)),
                    new Pick(i, MarketSelection.Moneyline(Side.Home)),
                };
            }

            // 3 — THE COUNT SPINE: each count family read at both scopes, team and match. Two
            // SharedCount relations (a team's corners inside the match total, the same for cards) and
            // four cross-family Independents, which is the busiest relation set the probe builds.
            case 3:
            {
                if (cfg.CornerLines.Length < 1 || cfg.CardLines.Length < 1
                    || cfg.TeamCornerLines.Length < 1 || cfg.TeamCardLines.Length < 1) return null;
                return new List<Pick>
                {
                    new Pick(i, MarketSelection.TeamTotalCorners(Side.Home, cfg.TeamCornerLines[0], true)),
                    new Pick(i, MarketSelection.TotalCorners(cfg.CornerLines[^1], true)),
                    new Pick(i, MarketSelection.TeamTotalCards(Side.Home, cfg.TeamCardLines[0], true)),
                    new Pick(i, MarketSelection.TotalCards(cfg.CardLines[0], true)),
                };
            }

            // 4 — THE MULTI SCORER, beside the two things his second goal already implies: he scored
            // at all, and his side scored twice. The scorer legs are the only place PlayerMultiScorer
            // can reach a joint, and its board is floor-truncated, so the row is read off the board
            // and the whole entry stands down when the matchup offers none.
            case 4:
            {
                if (cfg.TeamGoalLines.Length < 2) return null;
                if (ShortestMultiScorer(m) is not { } multi) return null;
                Side side = m.PlayerSide(multi.PlayerIndex);
                return new List<Pick>
                {
                    new Pick(i, multi),
                    new Pick(i, MarketSelection.AnytimeScorer(multi.PlayerIndex)),
                    new Pick(i, MarketSelection.TeamTotalGoals(side, cfg.TeamGoalLines[1], true)),
                    new Pick(i, MarketSelection.Moneyline(side)),
                };
            }

            // 5 — THE TEAM GOALS SPINE: home 2+ and away 1+, which together entail both the BTTS yes
            // and the OVER. TeamTotalGoals' second source, so the kind does not depend on the
            // truncated multi-scorer board above it having a row on this matchup.
            default:
            {
                if (cfg.TeamGoalLines.Length < 2 || cfg.GoalLines.Length < 2) return null;
                return new List<Pick>
                {
                    new Pick(i, MarketSelection.TeamTotalGoals(Side.Home, cfg.TeamGoalLines[1], true)),
                    new Pick(i, MarketSelection.TeamTotalGoals(Side.Away, cfg.TeamGoalLines[0], true)),
                    new Pick(i, MarketSelection.BothTeamsToScore(true)),
                    new Pick(i, MarketSelection.TotalGoals(cfg.GoalLines[1], true)),
                };
            }
        }
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

    /// <summary>The shortest-priced 2+ scorer OFFERED on this matchup — the likeliest one, which is
    /// also the one whose joint with his side's goals is worth the most to check. Null where the 2%
    /// floor truncated the whole multi-scorer board, which is a real state of a low-scoring
    /// matchup.</summary>
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

    /// <summary>One OFFERED correct-score cell, rotated by <paramref name="pick"/> so the campaign
    /// does not spend every ticket on one scoreline — the rows differ in what they imply (a level
    /// cell names the DRAW, a 0-0 names BTTS no), and a single row would cover one of those. Null
    /// when the floor left the matchup no rows at all.</summary>
    private static MarketSelection? CorrectScoreRow(Matchup m, int pick)
    {
        int count = 0;
        foreach (MarketOffer offer in m.Markets)
            if (offer.Selection.Kind == MarketKind.CorrectScore) count++;
        if (count == 0) return null;

        int wanted = ((pick % count) + count) % count;
        int seen = 0;
        foreach (MarketOffer offer in m.Markets)
        {
            if (offer.Selection.Kind != MarketKind.CorrectScore) continue;
            if (seen++ == wanted) return offer.Selection;
        }
        return null;
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

    /// <summary>The deliberately invalid slip, rotating over the causes a κ = 1 board can actually
    /// reach, then placing the refusal's own remedy.
    ///
    /// <para>Goes through <see cref="Run.PlaceTicket"/> rather than the query: the thrown refusal is
    /// how a betslip meets this rule, and placement is atomic — nothing is spent and no ticket is
    /// added — so provoking it costs the probe nothing but the coverage it came for.</para>
    ///
    /// <para>Every slip is built so that DROPPING THE LAST LEG leaves a same-match pair: the remedy
    /// prefers the legs added last, so the corrected ticket is itself coverage rather than a
    /// consolation single. The remedy is applied as the engine reported it — indices into the leg
    /// list, which is pick order — and never re-derived here, so a remedy that named a different set
    /// still places whatever the engine verified.</para>
    ///
    /// <para>Only two RULES are reachable (SubEvens needs κ ≳ 1.3), but a rule is not a cause: the
    /// four variants trip ImpossibleCombination through three structurally different conflicts — a
    /// scoreline that cannot hold two goal legs, a double chance that EXCLUDES the result beside it,
    /// and a correct-score cell that fixes the total — plus the duplicate rule. The exposure table
    /// reports what the model labelled, so no claim about which is made here.</para></summary>
    private static void PlaceRefusalProbe(Run run, BotState state, Matchup m, double stake,
        double budget)
    {
        if (budget < stake) return;
        List<Pick>? invalid = InvalidSlip(run.Round % RefusalVariants, m, run.Config);
        if (invalid == null) return;

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

    /// <summary>The four invalid slips. Each is three legs whose LAST one is the offender, so the
    /// verified remedy leaves a same-match pair — and each pair left standing carries kinds the rest
    /// of the catalogue reaches thinly or not at all (a double chance beside corners; a correct score
    /// beside the result it names).</summary>
    private static List<Pick>? InvalidSlip(int variant, Matchup m, RunConfig cfg)
    {
        int i = m.Index;
        switch (variant)
        {
            // IMPOSSIBLE, on the scoreline: BTTS yes needs two goals, UNDER the low line allows one.
            case 1:
                if (cfg.GoalLines.Length < 2) return null;
                return new List<Pick>
                {
                    new Pick(i, MarketSelection.TotalGoals(cfg.GoalLines[1], true)),
                    new Pick(i, MarketSelection.BothTeamsToScore(true)),
                    new Pick(i, MarketSelection.TotalGoals(cfg.GoalLines[0], false)),
                };

            // DUPLICATE: the same corner line twice, which the joint is idempotent over.
            case 2:
                if (cfg.CornerLines.Length < 2) return null;
                return new List<Pick>
                {
                    new Pick(i, MarketSelection.TotalCorners(cfg.CornerLines[0], true)),
                    new Pick(i, MarketSelection.TotalCorners(cfg.CornerLines[^1], false)),
                    new Pick(i, MarketSelection.TotalCorners(cfg.CornerLines[0], true)),
                };

            // IMPOSSIBLE, by EXCLUSION: 12 is precisely "not the draw", so the draw beside it wins on
            // no outcome at all. This is the overlap double chance was expected to produce and the
            // one refusal cause on the board that is a set-complement rather than an arithmetic
            // conflict. The remedy leaves 12 beside a corners leg — a cross-family pair.
            case 3:
                if (cfg.CornerLines.Length < 1) return null;
                return new List<Pick>
                {
                    new Pick(i, MarketSelection.DoubleChance(MarketChoice.HomeOrAway)),
                    new Pick(i, MarketSelection.TotalCorners(cfg.CornerLines[0], true)),
                    new Pick(i, MarketSelection.Moneyline(MatchResult.Draw)),
                };

            // IMPOSSIBLE, by a FIXED TOTAL: a correct-score cell settles the goal total exactly, so
            // the goal line it falls the wrong side of can never come in with it. Chosen off the
            // cell rather than assumed, so it holds for every row the floor leaves offered.
            default:
            {
                if (cfg.GoalLines.Length < 1) return null;
                if (CorrectScoreRow(m, m.Index) is not { } row) return null;
                int total = row.ScoreHome + row.ScoreAway;
                MatchResult result = row.ScoreHome > row.ScoreAway ? MatchResult.Home
                    : row.ScoreHome < row.ScoreAway ? MatchResult.Away : MatchResult.Draw;
                bool over = total < cfg.GoalLines[0]; // over the line the cell sits under, or vice versa
                return new List<Pick>
                {
                    new Pick(i, row),
                    new Pick(i, MarketSelection.Moneyline(result)),
                    new Pick(i, MarketSelection.TotalGoals(cfg.GoalLines[0], over)),
                };
            }
        }
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
