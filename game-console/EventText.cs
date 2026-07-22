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
        if (e.Type == DramaEventType.LegFinal) return Prefix(leg) + "FINAL WHISTLE";

        // Market legs (O/U, BTTS) have no picked TEAM — anchor the narrative on the home side;
        // the market prefix carries the pick, and up/down still tracks the pick's win prob.
        // Real market-aware vocabulary is Phase 3 (F_0.4.0 plan).
        bool pickedHome = leg.Selection.Kind != MarketKind.Moneyline
            || leg.Selection.Choice == MarketChoice.Home;
        string picked = Short(pickedHome ? leg.Matchup.Home.Name : leg.Matchup.Away.Name);
        string other = Short(pickedHome ? leg.Matchup.Away.Name : leg.Matchup.Home.Name);
        bool up = e.WinProbAfter >= prevProb;

        // Count narration is honest two ways (F_0.4.0 P3 review, mirrored from the TV):
        // an increment's mood is fixed by the SELECTION (Over hopes, Under dreads), and a
        // count event is only narrated on a beat moving TOWARD the count-rich outcome —
        // a beat drifting toward Under is a quiet spell, not a corner.
        if (leg.Selection.Kind == MarketKind.TotalCorners || leg.Selection.Kind == MarketKind.TotalCards)
        {
            bool countHelps = leg.Selection.Choice == MarketChoice.Over;
            bool countEventBeat = up == countHelps;
            if (!countEventBeat)
                return Prefix(leg) + Base(DramaEventType.Momentum, up, picked, other, e.Step);
            return Prefix(leg) + (leg.Selection.Kind == MarketKind.TotalCorners
                ? CornerLine(countHelps, e.Step)
                : BookingLine(countHelps, e.Step));
        }

        // Tag overrides win over the base table.
        if (e.Tag == TensionTag.NearMiss)
            return Prefix(leg) + (up ? "off the bar — a miracle brewing?!" : "…cleared off the line. it's slipping away");

        string line = Base(e.Type, up, picked, other, e.Step);
        if (e.Tag == TensionTag.LeadChange) line += " — LEAD CHANGE";
        return Prefix(leg) + line;
    }

    private static string Prefix(Leg leg)
        => leg.Selection.Kind == MarketKind.Moneyline ? ""
            : $"[{leg.Selection.Kind} {leg.Selection.Line:0.0} {leg.Selection.Choice}] ";

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

    private static string CornerLine(bool forPicked, int step)
        => (forPicked ? CornerFor : CornerAgainst)[step % (forPicked ? CornerFor.Length : CornerAgainst.Length)];

    private static string BookingLine(bool forPicked, int step)
        => (forPicked ? BookingFor : BookingAgainst)[step % (forPicked ? BookingFor.Length : BookingAgainst.Length)];

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

    private static readonly string[] CornerFor =
    {
        "corner won — another little number for the ledger.",
        "the flag goes up; pressure becomes a corner.",
        "whipped into the corner — the count moves your way.",
    };

    private static readonly string[] CornerAgainst =
    {
        "corner conceded. the number leans the wrong way.",
        "they win the flag — an under bettor hears the groan.",
        "deflected wide. corner to them, naturally.",
    };

    private static readonly string[] BookingFor =
    {
        "yellow card in the spell — the picked number improves.",
        "the referee reaches for the card. discipline pays.",
        "late tackle, clear booking. the cards count ticks.",
    };

    private static readonly string[] BookingAgainst =
    {
        "yellow card against the pick. the count bites.",
        "whistle, card, paperwork — that is not what you wanted.",
        "another booking. the number turns sour.",
    };

    /// <summary>The team's noun (last word of the "City Noun" name) — punchier for the ticker.</summary>
    private static string Short(string teamName)
    {
        int i = teamName.LastIndexOf(' ');
        return i >= 0 ? teamName.Substring(i + 1) : teamName;
    }
}
