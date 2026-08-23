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

    // THE TELLING, not the leg (T140 arm A). A fixture is broadcast once per ticket and every leg
    // riding it resolves at that single whistle, so the cursor walks FIXTURES. On a ticket with at
    // most one leg per matchup there is exactly one leg per fixture and _currentFixture IS the old
    // _currentLeg, which is what makes every expression below reduce to its pre-arm-A form there.
    private readonly IReadOnlyList<IReadOnlyList<int>> _fixtures; // legs per fixture, ticket order
    private readonly int[] _fixtureOfLeg;

    private int _currentFixture;   // telling being emitted (also the count of told fixtures)
    private int _cursorInFixture;  // index of the next event to emit within the current telling
    private readonly double[] _legLiveProb;  // per leg: its own latest live win-prob
    private double _liveProb;      // the LIVE SET's drama number - the product over this telling's
                                   // unvoided legs, and simply the leg's own prob on a solo telling
    private bool _complete;

    // THE WINDOW OPENS ONCE PER WHISTLE, after every grade on the fixture has landed (T143) - never
    // once per dead leg, because N windows is N interruptions. It NAMES every leg that died there.
    private readonly List<int> _deadThisWhistle = new List<int>();
    private bool _pendingWindowOpen;
    private readonly double[] _legProbBefore;  // per leg: its own prob before the beat being emitted
    private double _ticketProbBefore;          // the TICKET's displayed prob before that beat
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
    private double _pSettledLive;  // p_joint(S u L)    — plus every leg on the telling in flight
    private double _pActive;       // p_joint(S u L u U)— plus every leg still pending
    private readonly double[] _legCondMarginal; // P(j | S) for each LIVE leg — the drama's baseline

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
        // The SAME grouping helper, in the SAME first-appearance order, the joint price factorises
        // through - so the sweat's idea of a fixture and the pricing model's cannot drift apart.
        (_, List<List<int>> groups) = SameMatchModel.GroupByMatchup(ticket.Legs);
        _fixtures = groups;
        if (paths.Count != _fixtures.Count)
            throw new ArgumentException("Path count must match the ticket's fixture count.", nameof(paths));

        _fixtureOfLeg = new int[ticket.Legs.Count];
        for (int f = 0; f < groups.Count; f++)
            foreach (int j in groups[f]) _fixtureOfLeg[j] = f;

        _revealed = new LegState[ticket.Legs.Count];
        for (int i = 0; i < _revealed.Length; i++) _revealed[i] = LegState.Pending;

        // Every leg carries its own live number from the start, so an unstarted leg already reads its
        // board marginal and no re-seed is needed at a fixture boundary. That removes the site T164
        // named as a seam rather than re-pointing it.
        _legLiveProb = new double[ticket.Legs.Count];
        _legProbBefore = new double[ticket.Legs.Count];
        _legCondMarginal = new double[ticket.Legs.Count];
        for (int i = 0; i < _legLiveProb.Length; i++) _legLiveProb[i] = ticket.Legs[i].TrueProb;

        _currentFixture = 0;
        _cursorInFixture = 0;
        _complete = _fixtures.Count == 0;
        _liveProb = _fixtures.Count > 0 ? LiveSetProb() : 0.0;
        _ticketProbBefore = _liveProb;
    }

    /// <summary>The drama number for the legs LIVE right now: the product over this telling's unvoided
    /// legs of their own live probabilities. On a solo telling that product is the one leg's number, to
    /// the bit, which is why every expression that used <c>_liveProb</c> before arm A still reads the
    /// same value on a ticket with no same-match pair.</summary>
    private double LiveSetProb()
    {
        double p = 1.0;
        foreach (int j in _fixtures[_currentFixture])
            if (!_ticket.Legs[j].IsVoided) p *= _legLiveProb[j];
        return p;
    }

    /// <summary>The ticket this session sweats (Run's whistle/photo seams need it).</summary>
    internal Ticket TicketRef => _ticket;

    public bool IsComplete => _complete;

    /// <summary>A dead leg is awaiting the save decision (Mulligan void / Whistle re-roll /
    /// decline). While pending: cash-out is unavailable, and the next MoveNext AUTO-DECLINES
    /// (so autoplay never hangs on a window).</summary>
    public bool HasPendingLoss => _pendingWindowOpen;

    /// <summary>The FIRST dead leg of this whistle in ticket order, or -1. Retained because
    /// <c>Run.PlayRefsWhistle</c> keys its derived roll off it and the console reads it; the honest
    /// surface under arm A is <see cref="PendingDeadLegIndices"/>, since N legs can die at one
    /// whistle.</summary>
    public int PendingDeadLegIndex => _pendingWindowOpen ? _deadThisWhistle[0] : -1;

    /// <summary>Every leg that died at this whistle, in ticket order — <c>T143</c>: the window NAMES
    /// them all rather than opening once per death. Empty when no window is open.</summary>
    public IReadOnlyList<int> PendingDeadLegIndices =>
        _pendingWindowOpen ? _deadThisWhistle : (IReadOnlyList<int>)Array.Empty<int>();

    /// <summary><b>No single call saves this ticket</b> — two or more legs died at this one whistle,
    /// and both saves act on ONE leg, so whichever is spent the ticket still dies. <c>S85</c>: a
    /// refusal knowable before the act is shown before it, and the player must not spend a Whistle to
    /// discover the ticket was already dead twice over. The saves stay LEGAL (DD batch 169) — this is
    /// the fact the surface states first, not a lock on the offer.</summary>
    public bool NoSingleCallSaves => _pendingWindowOpen && _deadThisWhistle.Count >= 2;

    /// <summary>The PendingLossContext (PLAN.md rev 5 §4): the FIRST dead leg's own win-prob from
    /// BEFORE the killing event — the immutable value a played Whistle rolls against. 0 when no
    /// window is open.
    ///
    /// <para><b>This stays LEG-scoped while the window's DISPLAY is ticket-scoped</b> (<c>T143-am</c>,
    /// DD batch 169). The Whistle re-rolls one leg's grading, so it must roll at the number that leg
    /// was living on; re-basing it to the ticket would quietly weaken the item on every multi-leg
    /// ticket, which is an economy change and not a presentation one. The quantity <c>T143</c> ruled
    /// the window displays is <see cref="PendingLossTicketProbBefore"/>.</para></summary>
    public double PendingLossProbBefore => _pendingWindowOpen ? _legProbBefore[_deadThisWhistle[0]] : 0.0;

    /// <summary>The TICKET's displayed win-probability from before the killing beat — what the window
    /// SHOWS (<c>T143</c>, as amended by <c>T143-am</c>). Frozen at the same instant as
    /// <see cref="PendingLossProbBefore"/> so the two describe one moment.</summary>
    public double PendingLossTicketProbBefore => _pendingWindowOpen ? _ticketProbBefore : 0.0;

    /// <summary>Whether the pending loss can be resolved as a Mulligan (≥2 active legs — the
    /// whistle-opened window on a single-leg ticket cannot be mulliganed). Legal is not the same as
    /// SUFFICIENT: see <see cref="NoSingleCallSaves"/>.</summary>
    public bool CanMulliganPendingLoss => _pendingWindowOpen && ActiveLegCount() >= 2;

    /// <summary>How many tellings this ticket has — its FIXTURE count. Equal to the leg count unless
    /// the ticket carries a same-match pair.</summary>
    public int FixtureCount => _fixtures.Count;

    /// <summary>Which telling is in flight (0-based). Equals the count of tellings already finished.</summary>
    public int CurrentFixtureIndex => _currentFixture;

    /// <summary>The legs LIVE right now, in ticket order — every leg on the fixture being broadcast.
    /// Empty once the sweat is over. This is the live set presentation renders.</summary>
    public IReadOnlyList<int> CurrentFixtureLegs =>
        _currentFixture < _fixtures.Count ? _fixtures[_currentFixture] : (IReadOnlyList<int>)Array.Empty<int>();

    /// <summary>One leg's own live win-probability. <b>Engine and consumable use only</b> — <c>T143</c>
    /// rules that no leg's probability is ever SHOWN alone; presentation reads
    /// <see cref="TicketWinProbability"/>. Present because the Whistle's roll is per-leg.</summary>
    public double LiveLegProbability(int legIndex)
    {
        if (legIndex < 0 || legIndex >= _legLiveProb.Length)
            throw new ArgumentOutOfRangeException(nameof(legIndex));
        return _legLiveProb[legIndex];
    }

    /// <summary>Advances one event. Returns false (with a null event) once the session is complete.
    /// Advancing past a pending loss declines it — the bust proceeds.</summary>
    public bool MoveNext([MaybeNullWhen(false)] out DramaEvent evt)
    {
        if (_pendingWindowOpen)
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

        DramaEvent e = _paths[_currentFixture][_cursorInFixture];
        _cursorInFixture++;

        // Captured BEFORE the event lands (rev 5 §4: the whistle rolls at the prob the player was
        // living on, not 0). Per leg for the roll, and the ticket's for what the window displays.
        _ticketProbBefore = TicketWinProbability;
        foreach (int j in _fixtures[_currentFixture]) _legProbBefore[j] = _legLiveProb[j];

        // The solo branch is not an optimisation: it keeps the hot path allocation-free, since
        // LegIndices/LegProbs materialise their one-element view on access.
        if (!e.IsSharedTelling)
        {
            _legLiveProb[e.LegIndex] = e.WinProbAfter;
        }
        else
        {
            IReadOnlyList<int> idx = e.LegIndices;
            IReadOnlyList<double> pr = e.LegProbs;
            for (int i = 0; i < idx.Count; i++) _legLiveProb[idx[i]] = pr[i];
        }
        _liveProb = LiveSetProb();

        if (_holdEventsLeft > 0)
            _holdEventsLeft--;

        if (e.Type == DramaEventType.LegFinal)
            ResolveFixtureFinal();

        evt = e;
        return true;
    }

    /// <summary>ONE WHISTLE, N GRADES (<c>T140</c> arm A). Every leg on the fixture resolves here, and
    /// <b>the grades land in LEG ORDER after ONE hold</b> — <c>T87-am2</c>: N legs, N grades, one hold,
    /// not N. Leg order means TICKET order, not resolution order, which is why this walks the fixture's
    /// members rather than the dead set.
    ///
    /// <para>The window then opens ONCE, after every grade on the fixture has landed (<c>T143</c>),
    /// naming every leg that died at it.</para></summary>
    private void ResolveFixtureFinal()
    {
        IReadOnlyList<int> members = _fixtures[_currentFixture];
        _deadThisWhistle.Clear();

        foreach (int j in members)
        {
            Leg leg = _ticket.Legs[j];
            if (leg.IsVoided) continue;

            LegState outcome = leg.State; // Won or Lost — revealed only now
            _revealed[j] = outcome;

            if (outcome == LegState.Won)
                _effects.OnLegResolved(_ticket, leg, LegGrade.Won); // no window opens on a won leg
            else
                _deadThisWhistle.Add(j); // grade DEFERRED until the window closes (rev 5 §1)
        }

        if (_deadThisWhistle.Count == 0)
        {
            AdvanceOrComplete();
            return;
        }

        // The pending-loss window (generalized, rev 5 §4): opens when any LEGAL save is held —
        // a Mulligan (needs ≥2 active legs) or a Whistle (any ticket, single-leg included).
        // OnLegResolved is deferred until the window CLOSES (rev 5 §1): the final grade isn't
        // known yet. No save held → the bust is instant.
        if (AnySaveHeld())
        {
            _pendingWindowOpen = true;
            return;
        }

        CloseWindowAsBust();
    }

    /// <summary>Whether any LEGAL save is currently held — a Mulligan needs two active legs, a Whistle
    /// works on any ticket. The window opens on this and, under arm A, STAYS open on it.</summary>
    private bool AnySaveHeld()
        => (_mulliganAvailable() && ActiveLegCount() >= 2) || _whistleAvailable();

    /// <summary>Closes the window after ONE save has been spent. <b>N legs can die at one whistle, and
    /// <c>NoSingleCallSaves</c> says exactly that — no SINGLE call is enough, not that no sequence is.</b>
    /// So while dead legs remain and another legal save is held, the window STAYS OPEN and the player
    /// may spend again; the engine has never capped how many legs of one ticket may void
    /// (<c>Run.PlayMulliganSlip</c>: "No cap on how many legs of one ticket may void"), and closing here
    /// would have removed that capability silently on exactly the tickets arm A restructures.
    ///
    /// <para>Found by the suite rather than by reasoning: <c>VoidRepricingTests</c> pins two voids on a
    /// same-match ticket and three on a four-leg one, and both went unreachable when this busted after
    /// the first save.</para></summary>
    private void CloseOrKeepOpenAfterSave()
    {
        if (_deadThisWhistle.Count == 0)
        {
            _pendingWindowOpen = false;
            AdvanceOrComplete();
            return;
        }

        if (AnySaveHeld()) return; // still dead legs, still something to spend — the window holds
        CloseWindowAsBust();
    }

    /// <summary>Grades every leg still dead at this whistle and busts. The deferred grades land here,
    /// in ticket order, so a save spent on one leg cannot change how the others were recorded.</summary>
    private void CloseWindowAsBust()
    {
        _pendingWindowOpen = false;
        foreach (int j in _deadThisWhistle)
            _effects.OnLegResolved(_ticket, _ticket.Legs[j], LegGrade.Lost);
        _deadThisWhistle.Clear();
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
        if (!_pendingWindowOpen)
            throw new InvalidOperationException("No pending loss to mulligan");
        if (ActiveLegCount() < 2)
            throw new InvalidOperationException("Voiding the only active leg is not a save");

        // Acts on ONE leg — the first that died at this whistle, in ticket order. Where several died
        // the slip is still spendable (DD batch 169) and one of them is not enough on its own, which
        // is what NoSingleCallSaves states before the offer; a SECOND save can still finish the job.
        int dead = _deadThisWhistle[0];
        Leg leg = _ticket.Legs[dead];
        leg.IsVoided = true; // revealed Lost, but struck from the ticket
        _deadThisWhistle.RemoveAt(0);
        _effects.OnLegResolved(_ticket, leg, LegGrade.Voided);

        CloseOrKeepOpenAfterSave();
    }

    /// <summary>Run.PlayRefsWhistle's seam (rev 5 §4-5): the grading re-rolls once at the
    /// captured pre-kill prob. Overturned → this ticket's leg grades Won at FULL odds, the
    /// revealed state repairs (cash-out comes back to life), play advances. Confirmed → bust.
    /// The shared Matchup.Result never changes.</summary>
    internal void ResolvePendingLossWithWhistle(Pcg32 roll)
    {
        if (!_pendingWindowOpen)
            throw new InvalidOperationException("No pending loss to whistle");

        // Re-rolls ONE leg, at THAT leg's own pre-kill probability (T143-am): the item bends one
        // grading, so it rolls at the number that grading was living on.
        int dead = _deadThisWhistle[0];
        Leg leg = _ticket.Legs[dead];
        bool overturned = roll.NextDouble() < _legProbBefore[dead];

        if (overturned)
        {
            leg.RescuedWon = true;
            _revealed[dead] = LegState.Won; // the session's view repairs too (rev 5 §5)
            _deadThisWhistle.RemoveAt(0);
            _effects.OnLegResolved(_ticket, leg, LegGrade.Won);
            CloseOrKeepOpenAfterSave();
        }
        else
        {
            CloseWindowAsBust();
        }
    }

    /// <summary>Declines the window: the bust proceeds. Also invoked by advancing past it.</summary>
    public void DeclinePendingLoss()
    {
        if (!_pendingWindowOpen)
            throw new InvalidOperationException("No pending loss to decline");

        CloseWindowAsBust();
    }

    private void Bust()
    {
        _ticket.State = TicketState.Lost;
        _complete = true;
        _onBust(_ticket);
    }

    private void AdvanceOrComplete()
    {
        _currentFixture++;
        _cursorInFixture = 0;
        _jointsValid = false; // a fixture settled (or a leg was voided/repaired on the way here)
        if (_currentFixture >= _paths.Count)
            _complete = true; // every fixture told; ticket stays Open for Run.FinishSweat
        else
            _liveProb = LiveSetProb(); // no re-seed: each leg already carries its own board marginal
    }

    /// <summary>The revealed view of a leg: Pending until its LegFinal beat has been emitted.</summary>
    public LegState RevealedLegState(int legIndex)
    {
        if (legIndex < 0 || legIndex >= _revealed.Length)
            throw new ArgumentOutOfRangeException(nameof(legIndex));
        return _revealed[legIndex];
    }

    /// <summary>
    /// The win-probability shown to the player (<c>T164</c>): it seeds from the TICKET, never from
    /// whichever leg happens to be live — a dead, unvoided leg reads 0.0, a ticket with every leg
    /// settled reads 1.0, and otherwise it is the SAME <c>p_win</c> <see cref="CashOutFair"/> already
    /// prices off (<see cref="ProductWinProb"/> on an ordinary ticket, <see cref="ConditionalWinProb"/>
    /// on a SAME MATCH one). The bar on screen and the cash-out offer beneath it read one number, not
    /// two that happen to agree — <c>T143</c> rules that no leg's probability is ever shown alone, and
    /// a second, leg-shaped notion of "win probability" living beside the ticket's own would be exactly
    /// that.
    ///
    /// <para><b><c>_liveProb</c> is deliberately left untouched.</b> It is a PRICING input —
    /// <see cref="ProductCashOutFair"/> multiplies it in as the live leg's own factor, and
    /// <see cref="ConditionalWinProb"/> divides by that leg's conditional marginal of it — so
    /// re-seeding it for display would silently move the cash-out and the ratio the certainty
    /// carve-out tests against. This property only READS it, exactly as the cash-out path does.</para>
    ///
    /// <para><b>Anchors, and neither is a new tolerance.</b> At t = 0 on an ordinary ticket this is
    /// EXACTLY <c>Π TrueProb</c> over the active legs in ascending ticket order — see
    /// <see cref="ProductWinProb"/>, which is that same product, bit for bit. On a SAME MATCH ticket it
    /// lands within a few ulp of <c>SameMatchPrice.PTicket</c>, the price the ticket was sold at: the
    /// documented goal-family slack <see cref="ConditionalWinProb"/> already carries at its own t = 0
    /// anchor (<c>_liveProb</c> is seeded off the board's marginal, the conditional comes out of the
    /// joint evaluator, and the two agree to ulp rather than to the bit — see that method's
    /// remarks).</para>
    /// </summary>
    public double TicketWinProbability
    {
        get
        {
            // Any leg revealed Lost and not voided kills the ticket. Loop ALL legs, not just the
            // settled ones: during a pending-loss window the dead legs sit ON the live fixture, not
            // behind it, and a save has not yet been decided.
            for (int j = 0; j < _ticket.Legs.Count; j++)
                if (_revealed[j] == LegState.Lost && !_ticket.Legs[j].IsVoided) return 0.0;

            // Every telling finished and none lost: the ticket has won.
            if (_currentFixture >= _fixtures.Count) return 1.0;

            // Otherwise: the same p_win the cash-out prices off.
            return _ticket.SameMatch == null ? ProductWinProb() : ConditionalWinProb();
        }
    }

    /// <summary>The product-path win-probability behind <see cref="TicketWinProbability"/>: the live
    /// leg's current probability times every remaining leg's <see cref="Leg.TrueProb"/>, voided legs
    /// dropped, multiplied in ASCENDING ticket order starting from <c>_liveProb</c>. Bit-identical to
    /// the <c>pAll</c> <see cref="ProductCashOutFair"/> computes for its Free Bet refund term — same
    /// legs, same order, same starting value — which is the point: the ticket's displayed
    /// win-probability and the probability the cash-out quote already prices off must be the SAME
    /// number, not merely algebraically equal ones.</summary>
    private double ProductWinProb()
    {
        double p = _liveProb; // the live SET's product — one leg's number on a solo telling
        for (int j = 0; j < _ticket.Legs.Count; j++)
            if (_fixtureOfLeg[j] > _currentFixture && !_ticket.Legs[j].IsVoided)
                p *= _ticket.Legs[j].TrueProb;
        return p;
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
        // PARTITIONED BY TELLING, SCANNED IN TICKET ORDER. The stage test moved from "before the leg
        // cursor" to "on an earlier fixture", but the WALK is still ascending over the ticket's legs —
        // multiplication order decides the last bits, and on a ticket with one leg per fixture this is
        // the pre-arm-A expression term for term.
        double resolvedOddsProduct = 1.0;
        for (int j = 0; j < _ticket.Legs.Count; j++)
            if (_fixtureOfLeg[j] < _currentFixture && !_ticket.Legs[j].IsVoided)
                resolvedOddsProduct *= _ticket.Legs[j].OfferedOdds;

        var remaining = new List<(double p, double o)>(_ticket.Legs.Count);
        for (int j = 0; j < _ticket.Legs.Count; j++)
        {
            if (_ticket.Legs[j].IsVoided) continue;
            int f = _fixtureOfLeg[j];
            if (f < _currentFixture) continue;
            // Every leg on the LIVE fixture enters at its own live number — N of them under arm A,
            // one before it. Legs on later fixtures enter at the board's marginal, as they always did.
            remaining.Add(f == _currentFixture
                ? (_legLiveProb[j], _ticket.Legs[j].OfferedOdds)
                : (_ticket.Legs[j].TrueProb, _ticket.Legs[j].OfferedOdds));
        }

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
    /// <para><b>The conditional is exact, not an approximation.</b> The sweat resolves one FIXTURE at
    /// a time (a drama path per telling, one <c>_currentFixture</c> cursor), so every leg on an
    /// earlier fixture has a settled outcome and conditioning on it is a restriction of the sample
    /// space rather than a model of one. <b>Arm A did not weaken this</b> — it made the LIVE set
    /// bigger, not the settled set less decided, and the settled set is the only one conditioned on.
    /// Cash-out is only offered while the ticket is undecided, so every settled leg has
    /// WON — the constraint is simply that their predicates hold. (A whistle-rescued leg grades Won
    /// for THIS ticket whatever the shared match says, and is conditioned on as won: the quote prices
    /// the ticket's contract, and its payout reads the same repair.) Both conditionals are a RATIO OF
    /// TWO JOINTS the existing evaluator already computes, so no new sample space and no new machinery
    /// is introduced; legs on different matchups factorise out of numerator and denominator exactly as
    /// they do at placement. (Simultaneous live legs ARRIVED with <c>T140</c> arm A and still do not
    /// change this: N legs in flight are N legs not yet conditioned on, and the sample space the
    /// settled set restricts is the same one.)</para>
    ///
    /// <para><b>The trailing factor is the approved drama re-weight</b> (Allen, 2026-08-14).
    /// <see cref="LiveDramaJoint"/> is the drama-generated number for the legs in flight — one leg's
    /// live probability before <c>T140</c> arm A, and after it the live set's model joint drifted by
    /// what the drama has done to each leg; <c>P(L | S)</c> is that same set's conditional joint.
    /// Their ratio re-weights the correlated conditional so the quote tracks the beats, while keeping
    /// the correlation structure that multiplying raw numbers would have destroyed. <b>Both halves
    /// are joints</b>, which is what keeps <c>condAll &lt;= condLive</c> and therefore keeps the quote
    /// bounded — see <see cref="LiveDramaJoint"/> for the version of this that was not.</para>
    ///
    /// <para><b>THE CERTAINTY CARVE-OUT</b> (Allen, 2026-08-14 — a ruling on this rule's one visible
    /// consequence). When the settled legs ENTAIL the live one — a settled OVER 3.5 beside a live
    /// OVER 2.5 — <c>P(L | S)</c> is 1 while <c>liveProb</c> is the board's unconditional marginal
    /// and is not. The ratio is then <c>liveProb</c> itself, and the quote is scaled DOWN on a leg
    /// that cannot lose: the house would be offering less than the ticket is demonstrably worth.
    /// The ruling is that <b>a certainty never quotes below its worth</b> — at <c>P(L | S) = 1</c>
    /// the re-weight is dropped and the quote is the pure conditional <c>payout × P(L ∧ U | S)</c>.
    /// Everything else re-weights exactly as ratified; this is a carve-out at the boundary, not a
    /// softening of the rule.</para>
    ///
    /// <para><b>Why a tolerance and why THIS one.</b> <c>== 1.0</c> would be the wrong test: both
    /// halves of <c>condLive</c> come out of the joint evaluator, whose own marginals carry tens of
    /// ulp of slack against the board (see the goal-family note above and the diagnostic in
    /// <c>SameMatchCashOutTests</c>), so a true entailment can land a few ulp under 1. But the
    /// carve-out must not widen into an approximation of itself either — a leg at 0.999 is a
    /// near-certainty and MUST still re-weight, because that is the ratified rule. <see
    /// cref="CertaintySlack"/> is set at <c>1e-12</c>: roughly four thousand ulp at 1.0, which
    /// comfortably covers evaluator slack measured in tens, and roughly nine orders of magnitude
    /// below any probability the board can express — so nothing that is merely LIKELY can reach it.
    /// It selects entailment, and only entailment.</para>
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
        if (condLive >= 1.0 - CertaintySlack) return condAll; // the certainty carve-out

        return condAll * (LiveDramaJoint(condLive) / condLive);
    }

    /// <summary>
    /// The DRAMA's estimate of <c>P(every live leg wins | S)</c> — the numerator of the re-weight.
    ///
    /// <para><b>With one live leg it is that leg's live probability, exactly</b>, which is what the
    /// expression was before <c>T140</c> arm A. Every ticket without a same-match pair, and every
    /// solo fixture on a ticket with one, takes that line and does not move by a bit.</para>
    ///
    /// <para><b>With N live legs it is the model's joint for the set, DRIFTED by what the drama has
    /// done to each leg since the whistle it was priced at.</b> Getting this wrong is easy and it was
    /// got wrong once here, so the reasoning is recorded rather than the conclusion:</para>
    ///
    /// <para>The obvious move is to make the numerator the product of the live legs' drama numbers and
    /// the denominator the product of their marginals. It anchors correctly at t = 0 — and it BREAKS
    /// THE BOUND. The quote is bounded because <c>condAll &lt;= condLive</c>, which holds only when
    /// <c>condLive</c> is the JOINT: adding U's constraints can only lower a joint. A product of
    /// marginals is not a joint, and on positively correlated legs it sits BELOW one, so dividing by it
    /// inflates the quote. Measured: a quote of 127.33 against a payout of 110.14 — a cash-out worth
    /// more than the ticket can ever pay, which is a money printer, which is the one failure mode this
    /// file's guards exist for.</para>
    ///
    /// <para>So the denominator stays the joint and the DRIFT moves to the numerator: each live leg
    /// contributes <c>live_j / P(j | S)</c>, the factor by which the drama has moved it away from the
    /// baseline it was priced at, and the joint carries the correlation exactly once. At t = 0 every
    /// drift is 1 to within the evaluator's own ulp slack, so the quote is <c>payout x p_ticket</c> —
    /// the number the ticket was SOLD at — which is the anchor that must hold.</para>
    ///
    /// <para><b>The clamp is not a fudge.</b> Drift is unbounded above: two legs can both run hot and
    /// their product exceed 1. This is an estimate of a PROBABILITY, so 1 is its ceiling, and without
    /// the clamp the boundedness argument would hold everywhere except exactly where it is needed.</para>
    /// </summary>
    private double LiveDramaJoint(double condLive)
    {
        IReadOnlyList<int> live = _fixtures[_currentFixture];

        int only = -1;
        int n = 0;
        foreach (int j in live)
            if (!_ticket.Legs[j].IsVoided) { only = j; n++; }
        if (n <= 1) return only >= 0 ? _legLiveProb[only] : condLive;

        double drifted = condLive;
        foreach (int j in live)
        {
            if (_ticket.Legs[j].IsVoided) continue;
            double baseline = _legCondMarginal[j];
            if (!(baseline > 0.0)) continue; // no baseline to drift from; leave the joint as it stands
            drifted *= _legLiveProb[j] / baseline;
        }
        return drifted > 1.0 ? 1.0 : drifted;
    }

    /// <summary>How far under 1.0 <c>P(L | S)</c> may land and still be read as an ENTAILMENT rather
    /// than a near-certainty — the carve-out's threshold, argued in <see cref="ConditionalWinProb"/>.
    /// Wide enough to swallow the joint evaluator's own ulp-scale slack, and far too narrow for any
    /// probability the board can express to fall inside it.</summary>
    private const double CertaintySlack = 1e-12;

    /// <summary>Recomputes the three joints the conditional is a ratio of, if a leg boundary has moved
    /// since the last quote. The leg sets are built in TICKET ORDER, which is what makes
    /// <c>p_joint(active)</c> on an unvoided ticket the same number, to the bit, as the
    /// <c>SameMatchPrice.PTicket</c> it was sold at.</summary>
    private void EnsureJoints()
    {
        if (_jointsValid) return;

        // ALL THREE LISTS ARE BUILT BY SCANNING THE TICKET IN ORDER, which is what keeps
        // p_joint(active) equal, to the bit, to the SameMatchPrice.PTicket the ticket was sold at.
        // Under arm A a fixture's legs need not be contiguous in the ticket, so appending the live set
        // to the settled one would NOT be ticket order — the stage is a filter here, never the walk.
        var settled = new List<Leg>(_ticket.Legs.Count);
        var active = new List<Leg>(_ticket.Legs.Count);
        for (int j = 0; j < _ticket.Legs.Count; j++)
        {
            // A voided leg is out of every set. The legs on the LIVE fixture are never voided: a
            // mulligan voids one and the window it was open on either busts or advances past it.
            if (_ticket.Legs[j].IsVoided) continue;
            if (_fixtureOfLeg[j] < _currentFixture) settled.Add(_ticket.Legs[j]);
            active.Add(_ticket.Legs[j]);
        }

        var settledLive = new List<Leg>(_ticket.Legs.Count);
        for (int j = 0; j < _ticket.Legs.Count; j++)
        {
            if (_ticket.Legs[j].IsVoided) continue;
            if (_fixtureOfLeg[j] <= _currentFixture) settledLive.Add(_ticket.Legs[j]);
        }

        _pSettled = SameMatchModel.JointProbabilityOf(settled);
        _pSettledLive = SameMatchModel.JointProbabilityOf(settledLive);
        _pActive = SameMatchModel.JointProbabilityOf(active);

        // Each LIVE leg's own conditional marginal P(j | S). Not the denominator — see
        // ConditionalWinProb, where the denominator stays the JOINT because that is what the
        // boundedness argument rests on. These are the BASELINES the drama's per-leg numbers are
        // measured against, so that "how far has this leg drifted from where it was sold" is a
        // quantity the re-weight can multiply without smuggling a correlation term in with it.
        if (_pSettled > 0.0 && _currentFixture < _fixtures.Count)
        {
            var withLive = new List<Leg>(_ticket.Legs.Count);
            foreach (int j in _fixtures[_currentFixture])
            {
                if (_ticket.Legs[j].IsVoided) continue;
                withLive.Clear();
                for (int q = 0; q < _ticket.Legs.Count; q++) // ticket order, as above, for the same reason
                {
                    if (_ticket.Legs[q].IsVoided) continue;
                    if (_fixtureOfLeg[q] < _currentFixture || q == j) withLive.Add(_ticket.Legs[q]);
                }
                _legCondMarginal[j] = SameMatchModel.JointProbabilityOf(withLive) / _pSettled;
            }
        }
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
        if (_pendingWindowOpen) return false;
        if (_ticket.State != TicketState.Open) return false;
        // Any revealed dead leg, wherever it sits. Scanning every leg rather than the told prefix is
        // the same test on a ticket with one leg per fixture — an unstarted leg reads Pending — and
        // the correct one once N legs reveal at a single whistle.
        for (int j = 0; j < _ticket.Legs.Count; j++)
            if (_revealed[j] == LegState.Lost && !_ticket.Legs[j].IsVoided) return false;
        return true;
    }
}
