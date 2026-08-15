using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SBR.Engine;

/// <summary>Offer-hold payload for the live-intervention seam: the cash-out offer holds its
/// current price for the next N events. The hold freezes the PRICE, not fate — a revealed dead
/// leg still kills the offer (a held price on a dead ticket would be a money printer).
/// (Originally Timeout's payload; the item was cut at playtest #8 — the seam stays for future
/// actives.)</summary>
public sealed class OfferHoldEffect
{
    public int Events { get; }
    public OfferHoldEffect(int events)
    {
        if (events < 1) throw new ArgumentOutOfRangeException(nameof(events));
        Events = events;
    }
}

/// <summary>
/// The steppable sweat for one ticket (design/05). Legs are presented serially; the caller
/// advances one event at a time and may cash out between events.
///
/// Reveal discipline: engine truth (Leg.State via Matchup.Result) is known from lock time, but the
/// session exposes only the REVEALED view so presentation cannot leak the future.
///
/// Economy-rework changes (PLAN.md 2026-07-13): relic loss-conversion is gone — instead, when a
/// leg reveals Lost on a multi-leg ticket and a Mulligan Slip is HELD, the session suspends in a
/// PENDING-LOSS window (the player's timed save, design/10 D): Run.PlayMulliganSlip voids the leg
/// and play continues; DeclinePendingLoss — or simply advancing — busts the ticket. Offer holds
/// arrive through ApplyLiveEffect as an OfferHoldEffect. A cash-out or win notifies the
/// EffectEngine so the Scar carrier burns its stacks.
///
/// Determinism: all drama paths are baked at construction; stepping, cash-out, holds and the
/// pending window draw NO RNG — player timing can never perturb the run seed.
/// </summary>
public sealed class SweatSession
{
    private readonly Ticket _ticket;
    private readonly IReadOnlyList<IReadOnlyList<DramaEvent>> _paths;
    private readonly RunConfig _config;
    private readonly Action<double> _creditBank;
    private readonly EffectEngine _effects;
    private readonly Func<bool> _mulliganAvailable;
    private readonly Action<Ticket> _onBust;
    private readonly LegState[] _revealed;

    private int _currentLeg;   // leg currently being emitted (also the count of resolved/settled legs)
    private int _cursorInLeg;  // index of the next event to emit within the current leg
    private double _liveProb;  // latest live win-prob of the current (in-progress) leg
    private bool _complete;

    private int _pendingDeadLeg = -1;   // the revealed-dead leg awaiting a save decision
    private double _pendingLossProb;    // PendingLossContext: displayed prob before the killer
    private double _probBeforeEvent;    // the displayed prob before the event being emitted
    private readonly Func<bool> _whistleAvailable;
    private double? _heldFair;          // offer hold: frozen fair value
    private int _holdEventsLeft;        // offer hold: events the hold survives

    // The conditional-joint cache (F_0.6.0 Phase 4), used only on a SAME MATCH ticket. All three
    // joints are functions of WHICH legs are on the ticket and HOW MANY have settled, so they move
    // only at a leg boundary — never on an ordinary event. Invalidated in AdvanceOrComplete, the one
    // funnel through which the current leg advances and a mulligan's void or a whistle's repair
    // reaches the leg set.
    private bool _jointsValid;
    private double _pSettled;      // p_joint(S)        — the settled (therefore WON) legs
    private double _pSettledLive;  // p_joint(S u L)    — plus the leg in progress
    private double _pActive;       // p_joint(S u L u U)— plus every leg still pending

    /// <param name="creditBank">The bank seam: adds the given amount to the run's bank (cash-out).</param>
    /// <param name="effects">Owned passives; notified on bust (scar feeds) and realize (scar burns).</param>
    /// <param name="mulliganAvailable">Whether a Mulligan Slip is currently held (a mulligan save
    /// also needs ≥2 active legs — voiding the only leg is not a save).</param>
    /// <param name="whistleAvailable">Whether a Ref's Whistle is currently held — opens the window
    /// on ANY ticket, single-leg included (PLAN.md rev 5 §4).</param>
    /// <param name="onBust">Invoked once when the ticket busts.</param>
    internal SweatSession(Ticket ticket, IReadOnlyList<IReadOnlyList<DramaEvent>> paths, RunConfig config,
        Action<double> creditBank, EffectEngine effects, Func<bool> mulliganAvailable,
        Func<bool> whistleAvailable, Action<Ticket> onBust)
    {
        _ticket = ticket ?? throw new ArgumentNullException(nameof(ticket));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _creditBank = creditBank ?? throw new ArgumentNullException(nameof(creditBank));
        _effects = effects ?? throw new ArgumentNullException(nameof(effects));
        _mulliganAvailable = mulliganAvailable ?? throw new ArgumentNullException(nameof(mulliganAvailable));
        _whistleAvailable = whistleAvailable ?? throw new ArgumentNullException(nameof(whistleAvailable));
        _onBust = onBust ?? throw new ArgumentNullException(nameof(onBust));
        if (paths.Count != ticket.Legs.Count)
            throw new ArgumentException("Path count must match the ticket's leg count.", nameof(paths));

        _revealed = new LegState[ticket.Legs.Count];
        for (int i = 0; i < _revealed.Length; i++) _revealed[i] = LegState.Pending;

        _currentLeg = 0;
        _cursorInLeg = 0;
        _complete = ticket.Legs.Count == 0;
        _liveProb = ticket.Legs.Count > 0 ? ticket.Legs[0].TrueProb : 0.0;
    }

    /// <summary>The ticket this session sweats (Run's whistle/photo seams need it).</summary>
    internal Ticket TicketRef => _ticket;

    public bool IsComplete => _complete;

    /// <summary>A dead leg is awaiting the save decision (Mulligan void / Whistle re-roll /
    /// decline). While pending: cash-out is unavailable, and the next MoveNext AUTO-DECLINES
    /// (so autoplay never hangs on a window).</summary>
    public bool HasPendingLoss => _pendingDeadLeg >= 0;

    /// <summary>The pending dead leg's index, or -1.</summary>
    public int PendingDeadLegIndex => _pendingDeadLeg;

    /// <summary>The PendingLossContext (PLAN.md rev 5 §4): the leg's displayed win-prob from
    /// BEFORE the killing event — the immutable value a played Whistle rolls against. 0 when no
    /// window is open. Revealed state only; engine truth never leaks through here.</summary>
    public double PendingLossProbBefore => _pendingDeadLeg >= 0 ? _pendingLossProb : 0.0;

    /// <summary>Whether the pending loss can be resolved as a Mulligan (≥2 active legs — the
    /// whistle-opened window on a single-leg ticket cannot be mulliganed).</summary>
    public bool CanMulliganPendingLoss => _pendingDeadLeg >= 0 && ActiveLegCount() >= 2;

    /// <summary>Advances one event. Returns false (with a null event) once the session is complete.
    /// Advancing past a pending loss declines it — the bust proceeds.</summary>
    public bool MoveNext([MaybeNullWhen(false)] out DramaEvent evt)
    {
        if (_pendingDeadLeg >= 0)
        {
            DeclinePendingLoss();
            evt = null;
            return false;
        }

        if (_complete)
        {
            evt = null;
            return false;
        }

        DramaEvent e = _paths[_currentLeg][_cursorInLeg];
        _cursorInLeg++;
        _probBeforeEvent = _liveProb; // captured BEFORE the event lands (rev 5 §4: the whistle
        _liveProb = e.WinProbAfter;   // rolls at the prob the player was living on, not 0)

        if (_holdEventsLeft > 0)
            _holdEventsLeft--;

        if (e.Type == DramaEventType.LegFinal)
            ResolveLegFinal();

        evt = e;
        return true;
    }

    private void ResolveLegFinal()
    {
        Leg leg = _ticket.Legs[_currentLeg];
        LegState outcome = leg.State; // Won or Lost — revealed only now

        if (outcome == LegState.Won)
        {
            _revealed[_currentLeg] = LegState.Won;
            _effects.OnLegResolved(_ticket, leg, LegGrade.Won); // no window opens on a won leg
            AdvanceOrComplete();
            return;
        }

        _revealed[_currentLeg] = LegState.Lost;

        // The pending-loss window (generalized, rev 5 §4): opens when any LEGAL save is held —
        // a Mulligan (needs ≥2 active legs) or a Whistle (any ticket, single-leg included).
        // OnLegResolved is deferred until the window CLOSES (rev 5 §1): the final grade isn't
        // known yet. No save held → the bust is instant.
        bool mulliganLegal = _mulliganAvailable() && ActiveLegCount() >= 2;
        if (mulliganLegal || _whistleAvailable())
        {
            _pendingDeadLeg = _currentLeg;
            _pendingLossProb = _probBeforeEvent;
            return;
        }

        _effects.OnLegResolved(_ticket, leg, LegGrade.Lost);
        Bust();
    }

    private int ActiveLegCount()
    {
        int n = 0;
        foreach (Leg l in _ticket.Legs)
            if (!l.IsVoided) n++;
        return n;
    }

    /// <summary>Run.PlayMulliganSlip's seam: void the pending dead leg, the sweat continues.</summary>
    internal void ResolvePendingLossAsMulligan()
    {
        if (_pendingDeadLeg < 0)
            throw new InvalidOperationException("No pending loss to mulligan");
        if (ActiveLegCount() < 2)
            throw new InvalidOperationException("Voiding the only active leg is not a save");

        Leg leg = _ticket.Legs[_pendingDeadLeg];
        leg.IsVoided = true; // revealed Lost, but struck from the ticket
        _pendingDeadLeg = -1;
        _effects.OnLegResolved(_ticket, leg, LegGrade.Voided);
        AdvanceOrComplete();
    }

    /// <summary>Run.PlayRefsWhistle's seam (rev 5 §4-5): the grading re-rolls once at the
    /// captured pre-kill prob. Overturned → this ticket's leg grades Won at FULL odds, the
    /// revealed state repairs (cash-out comes back to life), play advances. Confirmed → bust.
    /// The shared Matchup.Result never changes.</summary>
    internal void ResolvePendingLossWithWhistle(Pcg32 roll)
    {
        if (_pendingDeadLeg < 0)
            throw new InvalidOperationException("No pending loss to whistle");

        Leg leg = _ticket.Legs[_pendingDeadLeg];
        bool overturned = roll.NextDouble() < _pendingLossProb;
        _pendingDeadLeg = -1;

        if (overturned)
        {
            leg.RescuedWon = true;
            _revealed[_currentLeg] = LegState.Won; // the session's view repairs too (rev 5 §5)
            _effects.OnLegResolved(_ticket, leg, LegGrade.Won);
            AdvanceOrComplete();
        }
        else
        {
            _effects.OnLegResolved(_ticket, leg, LegGrade.Lost);
            Bust();
        }
    }

    /// <summary>Declines the window: the bust proceeds. Also invoked by advancing past it.</summary>
    public void DeclinePendingLoss()
    {
        if (_pendingDeadLeg < 0)
            throw new InvalidOperationException("No pending loss to decline");

        Leg leg = _ticket.Legs[_pendingDeadLeg];
        _pendingDeadLeg = -1;
        _effects.OnLegResolved(_ticket, leg, LegGrade.Lost);
        Bust();
    }

    private void Bust()
    {
        _ticket.State = TicketState.Lost;
        _complete = true;
        _onBust(_ticket);
    }

    private void AdvanceOrComplete()
    {
        _currentLeg++;
        _cursorInLeg = 0;
        _jointsValid = false; // a leg settled (or was voided/repaired on the way here)
        if (_currentLeg >= _paths.Count)
            _complete = true; // every leg settled; ticket stays Open for Run.FinishSweat
        else
            _liveProb = _ticket.Legs[_currentLeg].TrueProb;
    }

    /// <summary>The revealed view of a leg: Pending until its LegFinal beat has been emitted.</summary>
    public LegState RevealedLegState(int legIndex)
    {
        if (legIndex < 0 || legIndex >= _revealed.Length)
            throw new ArgumentOutOfRangeException(nameof(legIndex));
        return _revealed[legIndex];
    }

    /// <summary>
    /// Fair cash-out value: the ticket's EXPECTED TERMINAL CREDIT in its current live state
    /// (PLAN.md rev 5 §2 — one contract-payoff evaluator), or null when unavailable. Under an offer
    /// hold, the held value is returned instead while the hold lasts.
    ///
    /// <para><b>Two paths, and which one a ticket takes is decided by its structure, not by a
    /// tolerance.</b> A ticket with at most one leg per matchup carries no
    /// <see cref="Ticket.SameMatch"/> block and prices exactly as it did before F_0.6.0. A SAME MATCH
    /// ticket's surviving legs are correlated and its price is a locked joint rather than a product of
    /// legs, so it prices off the conditional joint instead — see <see cref="JointCashOutFair"/>.</para>
    ///
    /// <para>Loss side, on both paths: a locked Free Bet adds <c>(1 − p_win) × stake</c> — the refund
    /// is contract, and design/02 requires the quote to price the FULL remaining payoff function. Each
    /// path uses ITS OWN <c>p_win</c>, which is the whole point: under a joint, the product of the
    /// remaining legs' marginals is not the probability this ticket still wins.</para>
    /// </summary>
    public double? CashOutFair()
    {
        if (!CashOutAvailable()) return null;
        if (_holdEventsLeft > 0 && _heldFair.HasValue) return _heldFair;
        return RawCashOutFair();
    }

    /// <summary>THE INVARIANT'S GUARD, and it is structural rather than arithmetic: an ordinary ticket
    /// leaves on the first line and runs the pre-F_0.6.0 expression VERBATIM. The same discipline that
    /// protects <see cref="Ticket.PotentialPayout"/> and <c>SameMatchModel.Refuse</c>, for the same
    /// reason — an algebraically equal rewrite could still differ in the last bits, and the golden
    /// seeds and the whole gate baseline sit downstream of those bits.</summary>
    private double RawCashOutFair()
        => _ticket.SameMatch == null ? ProductCashOutFair() : JointCashOutFair();

    /// <summary>The untouched product path, for a ticket with at most one leg per matchup.
    /// Win side: stake × Π(offered odds of settled-Won legs) × (p_live × o) of the current leg
    ///   × Π(trueProb × o) of legs not yet started, voided legs dropped, × the payout product.
    /// Loss side: a locked Free Bet adds (1 − Π p) × stake.</summary>
    private double ProductCashOutFair()
    {
        double resolvedOddsProduct = 1.0;
        for (int j = 0; j < _currentLeg; j++)
            if (!_ticket.Legs[j].IsVoided)
                resolvedOddsProduct *= _ticket.Legs[j].OfferedOdds;

        var remaining = new List<(double p, double o)>(_ticket.Legs.Count - _currentLeg);
        remaining.Add((_liveProb, _ticket.Legs[_currentLeg].OfferedOdds));
        for (int j = _currentLeg + 1; j < _ticket.Legs.Count; j++)
            if (!_ticket.Legs[j].IsVoided)
                remaining.Add((_ticket.Legs[j].TrueProb, _ticket.Legs[j].OfferedOdds));

        double winSide = OddsMath.CashOutFair(_ticket.Stake, resolvedOddsProduct, remaining)
            * _ticket.PayoutMultiplier;

        if (_ticket.Modifier == TicketModifier.FreeBet && !_ticket.Refunded)
        {
            double pAll = 1.0;
            foreach ((double p, double _) in remaining) pAll *= p;
            winSide += (1.0 - pAll) * _ticket.Stake; // the loss-side refund is contract too
        }

        return winSide;
    }

    /// <summary>
    /// A SAME MATCH ticket's fair value: <c>payout × P(the ticket still wins | what has settled)</c>
    /// (<c>design/02-betting-math.md</c> § *Same-game cash-out*, F_0.6.0 Phase 4).
    ///
    /// <para><b>The payout is READ, never re-derived.</b> <see cref="Ticket.PotentialPayout"/> is
    /// stake × the ticket's locked price × the payout multiplier, void-adjusted through the
    /// survivor-subset table locked at placement. Re-pricing the survivors here would be a second
    /// source of truth for the one number settlement already owns, and the two would eventually
    /// disagree.</para>
    ///
    /// <para><b>The loss-side refund uses the same conditional.</b> Free Bet's refund fires exactly
    /// when the ticket does NOT win, so it must be <c>(1 − p_win) × stake</c> for the SAME
    /// <c>p_win</c> the win side is multiplied by; the product of the remaining legs' marginals is
    /// not that number on a correlated ticket.</para>
    /// </summary>
    private double JointCashOutFair()
    {
        double payout = _ticket.PotentialPayout;
        double pWin = ConditionalWinProb();

        double fair = payout * pWin;

        if (_ticket.Modifier == TicketModifier.FreeBet && !_ticket.Refunded)
            fair += (1.0 - pWin) * _ticket.Stake;

        return fair;
    }

    /// <summary>
    /// <c>P(L ∧ U | S) × ( liveProb / P(L | S) )</c> — the probability a SAME MATCH ticket still wins,
    /// given that its settled legs won, re-weighted to agree with the number on screen.
    ///
    /// <para><b>The conditional is exact, not an approximation.</b> The sweat resolves ONE leg at a
    /// time (a drama path per leg, one <c>_currentLeg</c> cursor), so every leg before the cursor has
    /// a settled outcome and conditioning on it is a restriction of the sample space rather than a
    /// model of one. Cash-out is only offered while the ticket is undecided, so every settled leg has
    /// WON — the constraint is simply that their predicates hold. (A whistle-rescued leg grades Won
    /// for THIS ticket whatever the shared match says, and is conditioned on as won: the quote prices
    /// the ticket's contract, and its payout reads the same repair.) Both conditionals are a RATIO OF
    /// TWO JOINTS the existing evaluator already computes, so no new sample space and no new machinery
    /// is introduced; legs on different matchups factorise out of numerator and denominator exactly as
    /// they do at placement. (Simultaneous live legs are a PRESENTATION gap, scoped with
    /// <c>tv-sweat</c>, and cannot change this: under sequential resolution there is only ever one
    /// leg in flight.)</para>
    ///
    /// <para><b>The trailing factor is the approved drama re-weight</b> (Allen, 2026-08-14).
    /// <c>liveProb</c> is the drama-generated number the player is watching tick;
    /// <c>P(L | S)</c> is that same leg's own conditional marginal. Their ratio re-weights the
    /// correlated conditional so the quote agrees with what is on screen, while keeping the
    /// correlation structure that multiplying the two raw numbers would have destroyed.</para>
    ///
    /// <para><b>Anchors.</b> Nothing settled: <c>p_joint(S)</c> is the empty conjunction 1.0, so the
    /// numerator is the whole ticket's joint — the very number the ticket was SOLD at, bit for bit —
    /// and <c>P(L | S)</c> is the live leg's own marginal, which the sweat also seeds
    /// <c>_liveProb</c> from, so the re-weight is 1.0 and the quote is <c>payout × p_ticket</c>. Only
    /// the last leg left: numerator and denominator of <c>P(L ∧ U | S)</c> and <c>P(L | S)</c> are the
    /// same leg set, so the quote is <c>payout × liveProb</c> and walks to the full payout as that leg
    /// closes out.</para>
    ///
    /// <para><b>Both anchors land within a few ulp rather than on the bit, and the reason is
    /// upstream.</b> The sweat seeds <c>_liveProb</c> from <see cref="Leg.TrueProb"/> — the BOARD's
    /// marginal — while <c>P(L | S)</c> comes from the joint evaluator. <see cref="JointModel"/>'s
    /// goal-family note claims a single-selection enumeration reproduces the board price to the bit;
    /// measured, it does not, for the scorer markets and several count markets (up to ~70 ulp on
    /// <c>PlayerMultiScorer</c> — see <c>SameMatchCashOutTests</c>' diagnostic). Reading the live leg's
    /// denominator off <see cref="Leg.TrueProb"/> to force the anchor is the wrong fix: <c>P(L | S)</c>
    /// must come from the same evaluator as <c>P(L ∧ U | S)</c> or their ratio is not a
    /// conditional.</para>
    ///
    /// <para><b>Bounded by construction.</b> Adding U's constraints can only lower a joint, so
    /// <c>P(L ∧ U | S) ≤ P(L | S)</c> and the result is at most <c>liveProb</c> — the quote can never
    /// exceed the payout, and the Free Bet refund term can never go negative.</para>
    ///
    /// <para><b>The two guards are unreachable on a sold ticket, and are here anyway.</b> Both
    /// denominators are joints over SUBSETS of the ticket's active legs, and a subset's joint is
    /// weakly greater than the whole ticket's, which placement already refused at zero
    /// (<c>RefusalKind.ImpossibleCombination</c>). If either ever did reach zero the quote degrades
    /// downward — an unusable history prices the ticket at nothing rather than at NaN, and an
    /// unusable live marginal drops the re-weight rather than dividing by zero — because a cash-out
    /// that silently returns infinity is a money printer.</para>
    /// </summary>
    private double ConditionalWinProb()
    {
        EnsureJoints();

        if (!(_pSettled > 0.0)) return 0.0;
        double condAll = _pActive / _pSettled;

        double condLive = _pSettledLive / _pSettled;
        if (!(condLive > 0.0)) return condAll;

        return condAll * (_liveProb / condLive);
    }

    /// <summary>Recomputes the three joints the conditional is a ratio of, if a leg boundary has moved
    /// since the last quote. The leg sets are built in TICKET ORDER, which is what makes
    /// <c>p_joint(active)</c> on an unvoided ticket the same number, to the bit, as the
    /// <c>SameMatchPrice.PTicket</c> it was sold at.</summary>
    private void EnsureJoints()
    {
        if (_jointsValid) return;

        var settled = new List<Leg>(_currentLeg);
        for (int j = 0; j < _currentLeg; j++)
            if (!_ticket.Legs[j].IsVoided) settled.Add(_ticket.Legs[j]);

        // The leg under the cursor is never a voided one: a mulligan voids the leg the cursor is on
        // and AdvanceOrComplete moves past it in the same call.
        var settledLive = new List<Leg>(settled) { _ticket.Legs[_currentLeg] };

        var active = new List<Leg>(settledLive);
        for (int j = _currentLeg + 1; j < _ticket.Legs.Count; j++)
            if (!_ticket.Legs[j].IsVoided) active.Add(_ticket.Legs[j]);

        _pSettled = SameMatchModel.JointProbabilityOf(settled);
        _pSettledLive = SameMatchModel.JointProbabilityOf(settledLive);
        _pActive = SameMatchModel.JointProbabilityOf(active);
        _jointsValid = true;
    }

    /// <summary>The offered cash-out — fair × (1 − margin) × the quote scale (Golden Parachute
    /// waives margin back) — or null when unavailable.</summary>
    public double? CashOutOffer()
    {
        double? fair = CashOutFair();
        if (!fair.HasValue) return null;
        return OddsMath.CashOutOffer(fair.Value, _config.CashOutMargin) * _effects.CashOutQuoteScale;
    }

    /// <summary>Takes the current offer: credits the bank, marks the ticket CashedOut, ends the
    /// session, and notifies the effects (the Scar carrier burns on a cash-out too).</summary>
    public void AcceptCashOut()
    {
        double? offer = CashOutOffer();
        if (!offer.HasValue)
            throw new InvalidOperationException("No cash-out offer is currently available.");

        _creditBank(offer.Value);
        _ticket.State = TicketState.CashedOut;
        // Retain the figure BEFORE the session ends (S36). The offer is a live quote off the
        // remaining legs; once _complete is set it can never be recomputed, and the run's settled
        // record would otherwise have no honest way to print money the player actually banked.
        _ticket.CashedOutFor = offer.Value;
        _complete = true;
        _effects.OnTicketRealized(_ticket);
    }

    /// <summary>The live-intervention seam (design/05). Carries OfferHoldEffect (once Timeout's
    /// payload — the item was cut at playtest #8; the seam stays for future actives).
    /// Requires a live offer; the hold freezes the current FAIR value for the effect's event count.</summary>
    public void ApplyLiveEffect(object effect)
    {
        if (effect is OfferHoldEffect hold)
        {
            if (!CashOutAvailable())
                throw new InvalidOperationException("No live cash-out to hold.");
            _heldFair = RawCashOutFair();
            _holdEventsLeft = hold.Events;
            return;
        }
        throw new NotSupportedException($"Unknown live effect '{effect?.GetType().Name ?? "null"}'.");
    }

    // Cash-out is PRD F6 multi-leg-only, live only while the ticket is still an open, undecided
    // sweat. A voided (mulligan'd) leg does not count as a killing loss, and a whistle-rescued
    // leg reads revealed-Won (the repair, rev 5 §5); a PENDING dead leg does block — the window
    // is not a price shelter. A Double or Nothing ticket has NO exit: offers never appear.
    private bool CashOutAvailable()
    {
        if (_ticket.Modifier == TicketModifier.DoubleOrNothing) return false;
        if (_ticket.Legs.Count < 2) return false;
        if (_complete) return false;
        if (_pendingDeadLeg >= 0) return false;
        if (_ticket.State != TicketState.Open) return false;
        for (int j = 0; j < _currentLeg; j++)
            if (_revealed[j] == LegState.Lost && !_ticket.Legs[j].IsVoided) return false;
        return true;
    }
}
