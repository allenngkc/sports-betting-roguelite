using System.Collections.Generic;

namespace SBR.Engine;

/// <summary>Coarse shape of a drama event, driven by the win-prob delta it carries.</summary>
public enum DramaEventType { Momentum, Score, BigPlay, LegFinal }

/// <summary>Presentation-facing tension flavour; the director (design/04) cuts on these.</summary>
public enum TensionTag { Calm, Swing, LeadChange, NearMiss, Decisive }

/// <summary>
/// One authored beat of the sweat, pure data so any renderer (text ticker v0, broadcast
/// HUD later) can play it back. Carries no sport specifics — the "sport" is a reskin of
/// the vocabulary over this stream (design/04).
///
/// <para><b>A beat belongs to a TELLING, and a telling is a (ticket, FIXTURE)</b> — <c>T140</c> arm A.
/// Every leg of a ticket riding one fixture is live for that whole telling and resolves at its single
/// whistle, so a beat can carry N legs. <see cref="LegIndices"/> and <see cref="LegProbs"/> are the
/// honest surface; <see cref="LegIndex"/> and <see cref="WinProbAfter"/> are retained and describe the
/// telling's ANCHOR leg — the lowest ticket-order leg on that fixture. On a ticket with at most one leg
/// per matchup every telling has exactly one leg, so all four agree and nothing moves.</para>
///
/// <para><b><see cref="WinProbAfter"/> is not a display quantity.</b> <c>T143</c> and <c>T164</c> rule
/// that the shown win-probability is the TICKET's and that no leg's probability is ever displayed
/// alone; presentation reads <c>SweatSession.TicketWinProbability</c>. This stays here because
/// cash-out prices off it and because a per-leg number is what the Whistle's roll needs.</para>
/// </summary>
public sealed class DramaEvent
{
    // Null on a single-leg telling, which is EVERY telling on a ticket with no same-match pair.
    // The accessors materialise the one-element view on demand, so the shape that ships today
    // allocates exactly what it allocated before arm A — the gate campaign runs tens of thousands
    // of sweats and this is its hot path. On a shared telling the index array is built ONCE per
    // path and every beat on that path holds the same instance.
    private readonly IReadOnlyList<int>? _legIndices;
    private readonly IReadOnlyList<double>? _legProbs;

    /// <summary>Position within its ticket of this telling's ANCHOR leg — the lowest ticket-order leg
    /// riding this fixture (0-based). Equal to the only leg's index on a single-leg telling. Consumers
    /// that mean "every leg resolving here" want <see cref="LegIndices"/>.</summary>
    public int LegIndex { get; }

    /// <summary>Which telling this beat belongs to (0-based, fixtures in first-appearance order over
    /// the ticket's legs — the same grouping and the same order the joint price uses).</summary>
    public int FixtureIndex { get; }

    /// <summary>Whether this telling carries more than one leg. Allocation-free, unlike counting
    /// <see cref="LegIndices"/> — which matters because it is the discriminator consumers use to keep
    /// the pre-arm-A arithmetic verbatim on the shape that has a baseline.</summary>
    public bool IsSharedTelling => _legIndices != null;

    /// <summary>Every leg live on this telling, in ticket order. Length 1 unless the ticket carries a
    /// same-match pair.</summary>
    public IReadOnlyList<int> LegIndices => _legIndices ?? new[] { LegIndex };

    /// <summary>Each live leg's win probability after this beat, parallel to <see cref="LegIndices"/>.
    /// Per-leg by necessity — the legs on one fixture can be heading to opposite outcomes.</summary>
    public IReadOnlyList<double> LegProbs => _legProbs ?? new[] { WinProbAfter };

    /// <summary>1-based step within the TELLING. Under arm A this is the shared clock every leg on the
    /// fixture agrees on, which is what makes "the same beat" mean something when two legs are live.</summary>
    public int Step { get; }

    /// <summary>Total events in this telling (the final leg of a ticket carries 2x the budget).</summary>
    public int TotalSteps { get; }

    public DramaEventType Type { get; }

    /// <summary>Live win probability of the ANCHOR leg's picked side after this beat, in [0.03, 0.97]
    /// until the final beat, which is 0 or 1. Cash-out prices off it; presentation must not show it.</summary>
    public double WinProbAfter { get; }

    public TensionTag Tag { get; }

    /// <summary>A single-leg telling. Retained verbatim: this is the shape every ticket without a
    /// same-match pair produces, and it is the constructor existing tests build against.</summary>
    public DramaEvent(int legIndex, int step, int totalSteps, DramaEventType type, double winProbAfter, TensionTag tag)
        : this(legIndex, legIndex, step, totalSteps, type, winProbAfter, tag, null, null)
    {
    }

    /// <summary>A shared telling: <paramref name="legIndices"/> in ticket order with
    /// <paramref name="legProbs"/> parallel to it, and the anchor's values in the scalar slots.</summary>
    public DramaEvent(int legIndex, int fixtureIndex, int step, int totalSteps, DramaEventType type,
        double winProbAfter, TensionTag tag, IReadOnlyList<int>? legIndices, IReadOnlyList<double>? legProbs)
    {
        LegIndex = legIndex;
        FixtureIndex = fixtureIndex;
        Step = step;
        TotalSteps = totalSteps;
        Type = type;
        WinProbAfter = winProbAfter;
        Tag = tag;
        _legIndices = legIndices;
        _legProbs = legProbs;
    }
}
