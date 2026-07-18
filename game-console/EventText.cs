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
            return up ? "off the bar — a miracle brewing?!" : "…cleared off the line. it's slipping away";

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
        "{picked} slot it home.",
        "{picked} score — far post says yes.",
        "Goal for {picked} — the number ticks your way.",
    };

    private static readonly string[] ScoreDown =
    {
        "{other} answer right back.",
        "{other} poke one in at the near post. Ugly.",
        "{other} on the board; your slip flinches.",
    };

    private static readonly string[] BigUp =
    {
        "{picked} tear away — IT'S IN!",
        "{picked} break the line and finish — the crowd loses it.",
        "{picked} counter at full sprint. This is happening.",
    };

    private static readonly string[] BigDown =
    {
        "Disaster — {other} go the length of the pitch.",
        "{other} rip through on the break. Cover your eyes.",
        "{other} walk it in. That one hurt.",
    };

    private static readonly string[] MomUp =
    {
        "{picked} squeezing the half.",
        "{picked} pin them deep — passes and patience.",
        "{picked} tighten the screws.",
    };

    private static readonly string[] MomDown =
    {
        "{other} keeping the ball.",
        "{other} pass it around, slow and mean.",
        "{other} settle in; the drift is against you.",
    };

    /// <summary>The team's noun (last word of the "City Noun" name) — punchier for the ticker.</summary>
    private static string Short(string teamName)
    {
        int i = teamName.LastIndexOf(' ');
        return i >= 0 ? teamName.Substring(i + 1) : teamName;
    }
}
