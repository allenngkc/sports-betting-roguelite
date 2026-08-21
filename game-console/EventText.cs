using SBR.Engine;

namespace SBR.ConsoleGame;

/// <summary>
/// The flavour renderer. Engine DramaEvents carry no text on purpose (design/04: the "sport" is a
/// reskin of the event vocabulary), so the console owns the words. Lines are keyed by (Type, direction)
/// where direction is the sign of the win-prob move for the picked side; the first beat of a leg
/// compares against the leg's pre-event TrueProb anchor. Variant chosen by Step (deterministic, no RNG).
/// Tone: deadpan sports broadcast with a dark-comedy undertow (design/00).
///
/// <para><b><c>K16</c> — WHAT THIS FILE RETURNS IS AUTHORED PROSE AND NOTHING ELSE.</b> Every line
/// below used to be prefixed with <c>$"[{leg.Selection.Kind} {leg.Selection.Line:0.0}
/// {leg.Selection.Choice}] "</c>, which put a raw C# enum identifier on the player's screen on every
/// beat of every non-moneyline sweat — ruled a VIOLATION on a frame (DD batch 144) against three
/// standing laws at once: <c>S22</c> (the bracketed tag was STRUCK on 2026-07-31), <c>T98</c>/<c>T39</c>
/// (one casing per line — <c>TotalCorners</c> is camel case inside a sentence-case beat, the exact
/// defect the <c>LEAD CHANGE</c> comment below records fixing), and <c>T130</c>'s class (a C#
/// identifier reaching the player).</para>
///
/// <para><b>The prefix's PURPOSE was legitimate and its removal is not a deletion of it.</b> The
/// reader needs to know which leg is speaking, and he still does — the leg's ADDRESS now sits in the
/// meter line's gutter on every beat and the leg's NAME is printed in full at its state change
/// (<see cref="SweatLines"/>, which records why the identity could not stay on this line). This
/// file's contract is now exact and gateable: <b>it returns sentence-case prose, no market tag, no
/// bracket, no identifier</b> — so a future kind cannot reintroduce one by falling through a
/// <c>default:</c> here, because there is no composed tag left to fall through.</para>
/// </summary>
internal static class EventText
{
    public static string For(DramaEvent e, Leg leg, double prevProb)
    {
        if (e.Type == DramaEventType.LegFinal) return "FINAL WHISTLE";

        // The scorer board is an individual selection, and the scorer's IDENTITY is the
        // market outcome — reading the baked scorer list here would spoil the leg before
        // the walk decides it. Narration keys to the beat's direction only (the corners
        // precedent): hope reads as his chance building, dread as goals that aren't his.
        if (leg.Selection.Kind == MarketKind.AnytimeScorer
            && (e.Type == DramaEventType.Score || e.Type == DramaEventType.BigPlay))
        {
            Player pickedPlayer = leg.Matchup.PlayerAt(leg.Selection.PlayerIndex);
            bool up2 = e.WinProbAfter >= prevProb;
            // T44: "his moment is coming" predicts the outcome (CF: never imply a guaranteed win),
            // and "not your man" addresses the reader (CF: copy names the thing, not the reader).
            // Both are console-only — the TV has no twin for this branch — so they were fixed here
            // by the rule rather than mirrored. "the backed scorer" matches the TV's own wording.
            return up2
                ? $"{Surname(pickedPlayer.Name)} in the thick of it."
                : "a goal in the churn — not the backed scorer.";
        }

        // Market legs (O/U, BTTS) have no picked TEAM — anchor the narrative on the home side;
        // the market prefix carries the pick, and up/down still tracks the pick's win prob.
        // The market-aware vocabulary itself is the count-narration block below: corners/cards
        // key their mood to the selection (Over/Under), not to the home/away anchor used here.
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
                return Base(DramaEventType.Momentum, up, picked, other, e.Step);
            return leg.Selection.Kind == MarketKind.TotalCorners
                ? CornerLine(countHelps, e.Step)
                : BookingLine(countHelps, e.Step);
        }

        // Tag overrides win over the base table.
        if (e.Tag == TensionTag.NearMiss)
            // T44/T39: "a miracle brewing?!" is the line the ruling quotes by name — hype, a
            // superlative and a prediction in six words. Its own pair was already the right voice.
            return up ? "off the bar and away." : "…cleared off the line. it's slipping away.";

        // T98 (batch 70) — `— LEAD CHANGE` WAS APPENDED HERE AND THE WORD IS BANNED. Ruled on the
        // TV surface (SweatFlavor.For, this file's descendant) and swept here on Allen's call, the
        // same way T44 was: this file is the byte-for-byte ancestor, so leaving the twin would keep
        // the ruling's own quoted string alive in a project that still builds.
        //
        // The tag is REAL — DramaGenerator assigns it on the WIN PROBABILITY crossing 0.5, never on
        // the scoreline — so this is not a phantom-event guard. It comes off because a probability
        // is the house's OPINION where a price is an offer, and a line announcing the probability
        // crossed 50% is the deleted win-prob numeral's meaning without its digits.
        //
        // A TAG MAY DRIVE TIMING AND STAGING WITHOUT EARNING A WORD: SweatRenderer's
        // `TensionTag.LeadChange => 800` pacing is the twin of the TV's leadChangeMs and is
        // untouched and correct. Removing the suffix also closes the second defect — it appended
        // UPPERCASE to a sentence-case line, against the one-casing-one-dash rule.
        return Base(e.Type, up, picked, other, e.Step);
    }

    // K16 — THE PREFIX WAS HERE. It read:
    //
    //     leg.Selection.Kind == MarketKind.Moneyline ? ""
    //         : $"[{leg.Selection.Kind} {leg.Selection.Line:0.0} {leg.Selection.Choice}] ";
    //
    // It is not replaced in place, and the reason is worth writing down so nobody re-adds it: NO
    // FORM OF IT CAN LIVE ON THIS LINE. The name it should have printed is MarketSheet's, and that
    // name is UPPERCASE at the presentation layer (S96, §6.5) while a beat is sentence case — so any
    // in-sentence prefix is a second casing on the line (T98/T39), which is the very defect the
    // LEAD CHANGE comment above records fixing. It cannot be lowered to fit, either: the vocabulary
    // holds an initialism and proper nouns, so ToLowerInvariant prints `btts — yes` and `san
    // francisco spreadsheets`. And authoring a SECOND, sentence-case market vocabulary is what §6.6
    // and K8 forbid outright — one composer, two surfaces.
    //
    // IT ALSO NEVER FIT. At the widest constructible name (44 chars, S96-am/§3) an in-line prefix
    // takes the beat past the 80-column page, and the STRUCK form did too: `[DoubleChance 0.0
    // HomeOrDraw] ` is 30 columns, which puts the widest beat at 93. So the bracketed tag was a §13
    // gate-1 (C46) violation as well as a vocabulary one, unmeasured because nine kinds were
    // unbettable. SweatNamingGateTests measures both numbers off the POOL — it reflects the tables
    // below and SlateGenerator's own noun list rather than trusting a seed — and asserts the
    // remaining line fits.
    //
    // The leg is stated in SweatLines instead — address in the meter gutter on every beat, full name
    // at the state change (§9.3) — on lines that are wholly in the display register already, so each
    // carries exactly one casing.

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
        "Goal for {picked} — the number ticks with it.",
    };

    private static readonly string[] ScoreDown =
    {
        "{other} answer right back.",
        "{other} poke one in at the near post.",
        "{other} on the board; the slip flinches.",
    };

    private static readonly string[] BigUp =
    {
        "{picked} tear away and finish.",
        "{picked} break the line and score.",
        "{picked} counter at full sprint.",
    };

    private static readonly string[] BigDown =
    {
        "{other} go the length of the pitch.",
        "{other} rip through on the break.",
        "{other} walk it in.",
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
        "{other} settle in; the drift runs the other way.",
    };

    private static readonly string[] CornerFor =
    {
        "corner won — another little number for the ledger.",
        "the flag goes up; pressure becomes a corner.",
        "whipped into the corner — the count moves again.",
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
        "whistle, card, paperwork.",
        "another booking. the number turns sour.",
    };

    /// <summary>The team's noun (last word of the "City Noun" name) — punchier for the ticker.</summary>
    private static string Short(string teamName)
    {
        int i = teamName.LastIndexOf(' ');
        return i >= 0 ? teamName.Substring(i + 1) : teamName;
    }

    private static string Surname(string name)
    {
        int i = name.LastIndexOf(' ');
        return i >= 0 ? name.Substring(i + 1).ToUpperInvariant() : name.ToUpperInvariant();
    }
}
