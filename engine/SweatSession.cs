using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SBR.Engine;

/// <summary>
/// The steppable sweat for one ticket (design/05 "sweat as a steppable process"). Legs are
/// presented serially; the caller advances one event at a time and may cash out between events.
///
/// Reveal discipline: engine truth (Leg.State via Matchup.Result) is known from lock time, but the
/// session exposes only the REVEALED view so presentation cannot leak the future. Once a revealed
/// leg is Lost the remaining legs are never played out — the ticket is dead, unless a relic converts
/// the loss (mulligan voids the leg and play continues; lucky charm flips the final leg to a win for
/// this ticket only). early_payout credits per green leg; insurance and piggy fire when a ticket busts.
///
/// Determinism: all drama paths AND the lucky-charm roll are baked at construction, so cursor stepping,
/// cash-out, and every relic reaction here draw no RNG — whether/when the player cashes out or which
/// relics react can never perturb the run seed.
/// </summary>
public sealed class SweatSession
{
    private readonly Ticket _ticket;
    private readonly IReadOnlyList<IReadOnlyList<DramaEvent>> _paths;
    private readonly RunConfig _config;
    private readonly Action<double> _creditBank;
    private readonly EffectEngine _effects;
    private readonly bool _luckyBakedSuccess;
    private readonly Action<Ticket> _onBust;
    private readonly LegState[] _revealed;

    private int _currentLeg;   // leg currently being emitted (also the count of resolved/settled legs)
    private int _cursorInLeg;  // index of the next event to emit within the current leg
    private double _liveProb;  // latest live win-prob of the current (in-progress) leg
    private bool _complete;

    /// <param name="creditBank">The bank seam: adds the given amount to the run's bank (cash-out, early payout).</param>
    /// <param name="effects">Owned relics, in acquisition order; drives loss resolution and early payout.</param>
    /// <param name="luckyBakedSuccess">The lucky-charm second-chance roll, baked for this ticket at lock.</param>
    /// <param name="onBust">Invoked once when the ticket busts, to run the insurance/piggy chain.</param>
    internal SweatSession(Ticket ticket, IReadOnlyList<IReadOnlyList<DramaEvent>> paths, RunConfig config,
        Action<double> creditBank, EffectEngine effects, bool luckyBakedSuccess, Action<Ticket> onBust)
    {
        _ticket = ticket ?? throw new ArgumentNullException(nameof(ticket));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _creditBank = creditBank ?? throw new ArgumentNullException(nameof(creditBank));
        _effects = effects ?? throw new ArgumentNullException(nameof(effects));
        _luckyBakedSuccess = luckyBakedSuccess;
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

    /// <summary>Advances one event. Returns false (with a null event) once the session is complete.</summary>
    public bool MoveNext([MaybeNullWhen(false)] out DramaEvent evt)
    {
        if (_complete)
        {
            evt = null;
            return false;
        }

        DramaEvent e = _paths[_currentLeg][_cursorInLeg];
        _cursorInLeg++;
        _liveProb = e.WinProbAfter;

        if (e.Type == DramaEventType.LegFinal)
            ResolveLegFinal();

        evt = e;
        return true;
    }

    private void ResolveLegFinal()
    {
        Leg leg = _ticket.Legs[_currentLeg];
        bool isFinalLeg = _currentLeg == _paths.Count - 1;
        LegState outcome = leg.State; // Won or Lost — revealed only now

        if (outcome == LegState.Won)
        {
            _revealed[_currentLeg] = LegState.Won;
            CreditEarlyPayout(leg);
            AdvanceOrComplete();
            return;
        }

        // outcome == Lost: let owned relics convert it, in acquisition order.
        switch (_effects.ResolveLoss(isFinalLeg, _luckyBakedSuccess, _ticket.Legs.Count))
        {
            case LossResolution.LuckyFlip:
                leg.LuckyFlippedWon = true;
                _revealed[_currentLeg] = LegState.Won; // this ticket grades it Won
                CreditEarlyPayout(leg);
                AdvanceOrComplete();
                break;

            case LossResolution.Mulligan:
                leg.IsVoided = true;
                _revealed[_currentLeg] = LegState.Lost; // revealed lost, but struck from the ticket
                AdvanceOrComplete();                    // no early payout for a voided leg
                break;

            default: // Bust
                _revealed[_currentLeg] = LegState.Lost;
                _ticket.State = TicketState.Lost;
                _complete = true;
                _onBust(_ticket);
                break;
        }
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

    private void CreditEarlyPayout(Leg leg)
    {
        if (leg.IsVoided || !_effects.HasEarlyPayout) return;
        _creditBank(_effects.EarlyPayoutFraction * _ticket.Stake);
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
    ///   × Π(trueProb × o) of legs not yet started, with voided legs dropped — the corrected
    /// design/02 formula, plus (with early_payout) the EV of the future per-leg partials.
    /// </summary>
    public double? CashOutFair()
    {
        if (!CashOutAvailable()) return null;

        double resolvedOddsProduct = 1.0;
        for (int j = 0; j < _currentLeg; j++)
            if (!_ticket.Legs[j].IsVoided)
                resolvedOddsProduct *= _ticket.Legs[j].OfferedOdds;

        var remaining = new List<(double p, double o)>(_ticket.Legs.Count - _currentLeg);
        remaining.Add((_liveProb, _ticket.Legs[_currentLeg].OfferedOdds));
        for (int j = _currentLeg + 1; j < _ticket.Legs.Count; j++)
            if (!_ticket.Legs[j].IsVoided)
                remaining.Add((_ticket.Legs[j].TrueProb, _ticket.Legs[j].OfferedOdds));

        // The relic payout multiplier (high_roller) scales the terminal payout, so it scales its EV;
        // early-payout partials are stake-based and deliberately unscaled.
        double fair = OddsMath.CashOutFair(_ticket.Stake, resolvedOddsProduct, remaining) * _ticket.PayoutMultiplier;
        fair += FutureEarlyPayoutEv();
        return fair;
    }

    // Sum of the not-yet-paid early-payout partials, in expectation: b × (q1 + q1·q2 + …).
    private double FutureEarlyPayoutEv()
    {
        if (!_effects.HasEarlyPayout) return 0.0;

        double b = _effects.EarlyPayoutFraction * _ticket.Stake;
        double cumulative = _liveProb; // current leg's live win prob
        double ev = b * cumulative;
        for (int j = _currentLeg + 1; j < _ticket.Legs.Count; j++)
        {
            if (_ticket.Legs[j].IsVoided) continue;
            cumulative *= _ticket.Legs[j].TrueProb;
            ev += b * cumulative;
        }
        return ev;
    }

    /// <summary>The offered cash-out (fair × (1 − margin)), or null when unavailable.</summary>
    public double? CashOutOffer()
    {
        double? fair = CashOutFair();
        return fair.HasValue ? OddsMath.CashOutOffer(fair.Value, _config.CashOutMargin) : (double?)null;
    }

    /// <summary>Takes the current offer: credits it to the bank, marks the ticket CashedOut, ends the session.</summary>
    public void AcceptCashOut()
    {
        double? offer = CashOutOffer();
        if (!offer.HasValue)
            throw new InvalidOperationException("No cash-out offer is currently available.");

        _creditBank(offer.Value);
        _ticket.State = TicketState.CashedOut;
        _complete = true;
    }

    /// <summary>
    /// The live-intervention seam (design/05). Ships unused in v0 — live relics arrive with the
    /// relic system in a later phase; the stepping structure exists so the door stays open.
    /// </summary>
    public void ApplyLiveEffect(object effect)
        => throw new NotSupportedException("Live effects arrive with the relic system; this is the intervention seam (design/05).");

    // Cash-out is PRD F6 multi-leg-only, live only while the ticket is still an open, undecided sweat.
    // A voided (mulligan'd) leg does not count as a killing loss.
    private bool CashOutAvailable()
    {
        if (_ticket.Legs.Count < 2) return false;
        if (_complete) return false;
        if (_ticket.State != TicketState.Open) return false;
        for (int j = 0; j < _currentLeg; j++)
            if (_revealed[j] == LegState.Lost && !_ticket.Legs[j].IsVoided) return false;
        return true;
    }
}
