using SBR.Engine;

namespace SBR.ConsoleGame;

/// <summary>
/// The flavour renderer. Engine DramaEvents carry no text on purpose (design/04: the "sport" is a
/// reskin of the event vocabulary), so the console owns the words. Lines are keyed by (Type, direction)
/// where direction is the sign of the win-prob move for the picked side; the first beat of a leg
/// compares against the leg's pre-event TrueProb anchor. Variant chosen by Step (deterministic, no RNG).
/// Tone: deadpan sports broadcast with a dark-comedy undertow (design/00).
/// </summary>
internal static class EventText
{
    public static string For(DramaEvent e, Leg leg, double prevProb)
    {
        if (e.Type == DramaEventType.LegFinal) return "FINAL WHISTLE";

        string picked = Short(leg.Side == Side.Home ? leg.Matchup.Home.Name : leg.Matchup.Away.Name);
        string other = Short(leg.Side == Side.Home ? leg.Matchup.Away.Name : leg.Matchup.Home.Name);
        bool up = e.WinProbAfter >= prevProb;

        // Tag overrides win over the base table.
        if (e.Tag == TensionTag.NearMiss)
            return up ? "a miracle brewing?!" : "…it's slipping away";

        string line = Base(e.Type, up, picked, other, e.Step);
        if (e.Tag == TensionTag.LeadChange) line += " — LEAD CHANGE";
        return line;
    }

    private static string Base(DramaEventType type, bool up, string picked, string other, int step)
    {
        string[] variants = Table(type, up);
        return variants[step % variants.Length]
            .Replace("{picked}", picked)
            .Replace("{other}", other);
    }

    private static string[] Table(DramaEventType type, bool up) => (type, up) switch
    {
        (DramaEventType.Score, true) => ScoreUp,
        (DramaEventType.Score, false) => ScoreDown,
        (DramaEventType.BigPlay, true) => BigUp,
        (DramaEventType.BigPlay, false) => BigDown,
        (DramaEventType.Momentum, true) => MomUp,
        _ => MomDown,
    };

    private static readonly string[] ScoreUp =
    {
        "{picked} punch it in.",
        "{picked} find the end zone.",
        "Points for {picked} — the number ticks your way.",
    };

    private static readonly string[] ScoreDown =
    {
        "{other} answer right back.",
        "{other} punch one back. Ugly.",
        "{other} on the board; your slip flinches.",
    };

    private static readonly string[] BigUp =
    {
        "HUGE play by {picked}!",
        "{picked} break one wide open — the crowd loses it.",
        "{picked} strike deep. This is happening.",
    };

    private static readonly string[] BigDown =
    {
        "Disaster — {other} take it the distance.",
        "{other} rip off a monster play. Cover your eyes.",
        "{other} break contain. That one hurt.",
    };

    private static readonly string[] MomUp =
    {
        "{picked} grinding it out.",
        "{picked} lean on them, clock and all.",
        "{picked} tighten the screws.",
    };

    private static readonly string[] MomDown =
    {
        "{other} controlling the clock.",
        "{other} chew the clock, slow and mean.",
        "{other} settle in; the drift is against you.",
    };

    /// <summary>The team's noun (last word of the "City Noun" name) — punchier for the ticker.</summary>
    private static string Short(string teamName)
    {
        int i = teamName.LastIndexOf(' ');
        return i >= 0 ? teamName.Substring(i + 1) : teamName;
    }
}
