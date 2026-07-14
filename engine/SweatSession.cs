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

    private int _pendingDeadLeg = -1; // the revealed-dead leg awaiting a Mulligan Slip decision
    private double? _heldFair;        // offer hold: frozen fair value
    private int _holdEventsLeft;      // offer hold: events the hold survives

    /// <param name="creditBank">The bank seam: adds the given amount to the run's bank (cash-out).</param>
    /// <param name="effects">Owned passives; notified on bust (scar feeds) and realize (scar burns).</param>
    /// <param name="mulliganAvailable">Whether a Mulligan Slip is currently held — opens the
    /// pending-loss window on a revealed dead leg (consumption happens at play time via Run).</param>
    /// <param name="onBust">Invoked once when the ticket busts.</param>
    internal SweatSession(Ticket ticket, IReadOnlyList<IReadOnlyList<DramaEvent>> paths, RunConfig config,
        Action<double> creditBank, EffectEngine effects, Func<bool> mulliganAvailable, Action<Ticket> onBust)
    {
        _ticket = ticket ?? throw new ArgumentNullException(nameof(ticket));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _creditBank = creditBank ?? throw new ArgumentNullException(nameof(creditBank));
        _effects = effects ?? throw new ArgumentNullException(nameof(effects));
        _mulliganAvailable = mulliganAvailable ?? throw new ArgumentNullException(nameof(mulliganAvailable));
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

    public bool IsComplete => _complete;

    /// <summary>A dead leg is awaiting the Mulligan Slip decision. While pending: cash-out is
    /// unavailable, and the next MoveNext AUTO-DECLINES (so autoplay never hangs on a window).</summary>
    public bool HasPendingLoss => _pendingDeadLeg >= 0;

    /// <summary>The pending dead leg's index, or -1.</summary>
    public int PendingDeadLegIndex => _pendingDeadLeg;

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
        _liveProb = e.WinProbAfter;

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
            AdvanceOrComplete();
            return;
        }

        _revealed[_currentLeg] = LegState.Lost;

        // The Mulligan Slip window (design/10 D): opens only if a slip is HELD right now and
        // voiding would leave the ticket at least one active leg. No slip → the bust is instant.
        if (_mulliganAvailable() && ActiveLegCount() >= 2)
        {
            _pendingDeadLeg = _currentLeg;
            return;
        }

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

        _ticket.Legs[_pendingDeadLeg].IsVoided = true; // revealed Lost, but struck from the ticket
        _pendingDeadLeg = -1;
        AdvanceOrComplete();
    }

    /// <summary>Declines the window: the bust proceeds. Also invoked by advancing past it.</summary>
    public void DeclinePendingLoss()
    {
        if (_pendingDeadLeg < 0)
            throw new InvalidOperationException("No pending loss to decline");

        _pendingDeadLeg = -1;
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
    /// Fair cash-out value of the ticket in its current live state, or null when unavailable.
    /// = stake × Π(offered odds of settled-Won legs) × (p_live × o) of the current leg
    ///   × Π(trueProb × o) of legs not yet started, with voided legs dropped, all scaled by the
    /// payout product (design/02: cash-out prices the ticket's full remaining payoff function —
    /// Multiplier and a carried Scar included). Under an offer hold, the held value is returned
    /// instead while the hold lasts.
    /// </summary>
    public double? CashOutFair()
    {
        if (!CashOutAvailable()) return null;
        if (_holdEventsLeft > 0 && _heldFair.HasValue) return _heldFair;
        return RawCashOutFair();
    }

    private double RawCashOutFair()
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

        return OddsMath.CashOutFair(_ticket.Stake, resolvedOddsProduct, remaining) * _ticket.PayoutMultiplier;
    }

    /// <summary>The offered cash-out (fair × (1 − margin)), or null when unavailable.</summary>
    public double? CashOutOffer()
    {
        double? fair = CashOutFair();
        return fair.HasValue ? OddsMath.CashOutOffer(fair.Value, _config.CashOutMargin) : (double?)null;
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
    // sweat. A voided (mulligan'd) leg does not count as a killing loss; a PENDING dead leg does —
    // the window is not a price shelter.
    private bool CashOutAvailable()
    {
        if (_ticket.Legs.Count < 2) return false;
        if (_complete) return false;
        if (_pendingDeadLeg >= 0) return false;
        if (_ticket.State != TicketState.Open) return false;
        for (int j = 0; j < _currentLeg; j++)
            if (_revealed[j] == LegState.Lost && !_ticket.Legs[j].IsVoided) return false;
        return true;
    }
}
