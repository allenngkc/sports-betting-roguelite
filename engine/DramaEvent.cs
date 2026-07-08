namespace SBR.Engine;

/// <summary>Coarse shape of a drama event, driven by the win-prob delta it carries.</summary>
public enum DramaEventType { Momentum, Score, BigPlay, LegFinal }

/// <summary>Presentation-facing tension flavour; the director (design/04) cuts on these.</summary>
public enum TensionTag { Calm, Swing, LeadChange, NearMiss, Decisive }

/// <summary>
/// One authored beat of the sweat, pure data so any renderer (text ticker v0, broadcast
/// HUD later) can play it back. Carries no sport specifics — the "sport" is a reskin of
/// the vocabulary over this stream (design/04). <see cref="WinProbAfter"/> is the honest
/// live probability of the PICKED side after this beat; cash-out prices off it.
/// </summary>
public sealed class DramaEvent
{
    /// <summary>Position of the leg within its ticket (0-based).</summary>
    public int LegIndex { get; }

    /// <summary>1-based step within the leg.</summary>
    public int Step { get; }

    /// <summary>Total events in this leg (the final leg of a ticket carries 2x the budget).</summary>
    public int TotalSteps { get; }

    public DramaEventType Type { get; }

    /// <summary>Live win probability of the picked side after this beat, in [0.03, 0.97] until the final beat, which is 0 or 1.</summary>
    public double WinProbAfter { get; }

    public TensionTag Tag { get; }

    public DramaEvent(int legIndex, int step, int totalSteps, DramaEventType type, double winProbAfter, TensionTag tag)
    {
        LegIndex = legIndex;
        Step = step;
        TotalSteps = totalSteps;
        Type = type;
        WinProbAfter = winProbAfter;
        Tag = tag;
    }
}
