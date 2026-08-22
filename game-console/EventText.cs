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

        // K17-cl (DD 2026-08-21 batch 170, VIOLATION) — THE ANCHOR IS THE BACKED SIDE, AND WHERE
        // THE LEG NAMES NO SIDE THERE IS NO ANCHOR. What stood on these lines was:
        //
        //     // Market legs (O/U, BTTS) have no picked TEAM — anchor the narrative on the home side
        //     bool pickedHome = leg.Selection.Kind != MarketKind.Moneyline
        //         || leg.Selection.Choice == MarketChoice.Home;
        //
        // THAT COMMENT WAS TRUE WHEN IT WAS WRITTEN, and that is the whole of why this is a
        // violation only now: every non-moneyline kind then was team-agnostic, so a HOME anchor
        // was arbitrary and harmless. F_0.5.0 added five kinds that DO carry a side —
        // DoubleChance, Handicap and the three team totals — and the pick grammar made all of
        // them bettable. For each one the predicate above returns true and the prose anchors
        // HOME, so BACKING THE AWAY SIDE NARRATED THE OPPONENT AS THE PLAYER'S TEAM while the
        // leg's own verdict row named the club he actually backed: two zones of one surface
        // disagreeing about whose side he is on (T59's family, T94's shape on the fixture axis).
        // The same expression also answered AWAY for the X of 1X2 — "not Home" meant Away only
        // while Choice had two values — which is the defect BetslipModel.SideOn's own docstring
        // records fixing on the other surface.
        Side? backed = BackedSide(leg.Selection);
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
                return Anchored(DramaEventType.Momentum, up, backed, leg.Matchup, e.Step);
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
        return Anchored(e.Type, up, backed, leg.Matchup, e.Step);
    }

    /// <summary>
    /// <b><c>K17-cl</c> — WHICH SIDE THIS LEG BACKS, where the honest answer can be NEITHER.</b>
    /// This is <c>BetslipModel.SideOn</c>'s SHAPE — <c>Side?</c>, a draw answering neither — which
    /// is what the ruling names as the fix. It is deliberately NOT a call to that method, and the
    /// reason is worth stating so nobody "simplifies" this into one:
    ///
    /// <list type="number">
    /// <item><c>SideOn</c> short-circuits on <c>Kind != Moneyline</c> and returns null, so it
    /// answers NEITHER for all five of the side-carrying kinds this ruling is about. Calling it
    /// here would delete the false HOME anchor and never name the correct side — half a fix.</item>
    /// <item>Its signature takes a MATCHUP INDEX and scans a slip. A sweat beat holds ONE leg and
    /// no slip; there is nothing here to scan.</item>
    /// </list>
    ///
    /// <para><b>It is also NOT <c>SweatFlavor.PickedHomeForPresentation</c> widened, and the
    /// ruling forbids making it that.</b> That function answers WHICH TEAM THE PROSE ANCHORS ON,
    /// where every leg needs an answer and (its own docstring) <i>"neither" would leave the
    /// flavour with no names</i>. This one answers WHICH SIDE YOU BACKED. Two questions, two
    /// correct shapes; collapsing them re-creates the conflation <c>T143-am</c> split apart.</para>
    ///
    /// <para><b>Every arm reads the engine's own selection shape rather than restating what a kind
    /// is believed to do</b>, and the switch NAMES ALL FIFTEEN KINDS with no silent default — a
    /// <c>default:</c> that guesses a side is precisely how the struck predicate above came to be
    /// wrong. The throw mirrors <c>SweatLines.LegName</c>'s: a sixteenth kind fails loudly here
    /// rather than inheriting some other kind's answer.</para>
    /// </summary>
    internal static Side? BackedSide(MarketSelection s) => s.Kind switch
    {
        // The engine's own factories map a backed Side ONTO Choice — MarketSelection.Moneyline(Side)
        // and Handicap(Side backed, double line), whose summary says the line "is applied TO THE
        // BACKED SIDE". Reading Choice back is that mapping inverted. MoneylineDraw() sets
        // Choice.Draw, of which the engine says "Has no Side by construction": the X of 1X2 is
        // NEITHER, never Away (DD batch 49 — the draw is not a team, ever).
        MarketKind.Moneyline or MarketKind.Handicap => s.Choice switch
        {
            MarketChoice.Home => Side.Home,
            MarketChoice.Away => Side.Away,
            _ => null,
        },

        // Double chance names its selection as a UNION of results rather than reusing Home/Away —
        // MarketChoice's own comment: "1X is not Home, and a reader who assumes it is has a losing
        // bet graded as a winner". The backed side is the ONE CLUB in the union. 12 (HomeOrAway)
        // holds both clubs, so no club is his: NEITHER.
        MarketKind.DoubleChance => s.Choice switch
        {
            MarketChoice.HomeOrDraw => Side.Home,
            MarketChoice.AwayOrDraw => Side.Away,
            _ => null,
        },

        // The three team totals carry their team in a NAMED field, which exists so exactly this
        // question has an answer that is read and not decoded. Read it.
        MarketKind.TeamTotalGoals or MarketKind.TeamTotalCorners or MarketKind.TeamTotalCards
            => s.Team,

        // Match-scoped kinds. Selection.Team is null by construction on all of them and their
        // Choice carries Over/Under/Yes/No/Odd/Even — there is no club anywhere in the selection.
        // T163 branch (3) names this set outright: totals, BTTS, correct score, odd/even, margin
        // — NEITHER.
        MarketKind.TotalGoals or MarketKind.BothTeamsToScore or MarketKind.TotalCorners
            or MarketKind.TotalCards or MarketKind.CorrectScore or MarketKind.WinningMargin
            or MarketKind.TotalGoalsOddEven
            => null,

        // The player markets. A scorer's CLUB is knowable (Matchup.PlayerSide), and this still
        // answers NEITHER, because the question is which side he BACKED: a man can score in a
        // 3–1 defeat and the leg wins, so his club is not the player's side. The TV's
        // PickedHomeForPresentation does anchor these on PlayerSide — that is the other question
        // answered correctly for itself, and the divergence is the two shapes working as ruled.
        MarketKind.AnytimeScorer or MarketKind.PlayerMultiScorer => null,

        _ => throw new ArgumentOutOfRangeException(nameof(s), s.Kind,
            "EventText.BackedSide has no arm for this market kind. Add one — deliberately, having "
            + "decided whether the kind names a side. Nothing here falls through to a guess: that "
            + "fallback IS K17-cl."),
    };

    private static Side Opposite(Side side) => side == Side.Home ? Side.Away : Side.Home;

    private static string NameOf(Matchup matchup, Side side)
        => side == Side.Home ? matchup.Home.Name : matchup.Away.Name;

    /// <summary>The base tables, filled from the backed side — or, where the leg backs neither,
    /// from the club-free set below. <c>T163</c>'s trap is why this is one function and not two
    /// call sites: a "neutral" path that still computed <c>picked</c>/<c>other</c> would ship a
    /// HOME anchor under a neutral name, silently, on precisely the kinds this row rules on.</summary>
    private static string Anchored(DramaEventType type, bool up, Side? backed, Matchup matchup, int step)
        => backed is Side side
            ? Base(type, up, Short(NameOf(matchup, side)), Short(NameOf(matchup, Opposite(side))), step)
            : NeitherLine(type, up, step);

    /// <summary>
    /// <b>The <i>neither</i> branch — a beat that names no club at all.</b> Selected by the same
    /// (type, direction) key as <see cref="Table"/> so the two cannot drift apart.
    ///
    /// <para><b>WHY THESE ARE CLUB-FREE RATHER THAN CLUB-NAMING, which is not what the TV's spec
    /// asks for.</b> <c>spec-neither-branch-lines-2026-08-21.md</c> §1 keeps the club in the
    /// sentence and moves the REFERENT: in the neither branch <c>{picked}</c>/<c>{other}</c>
    /// resolve to the club the EVENT belongs to — the scorer on a goal, the side in possession on
    /// a momentum beat. <b>That mechanism has nothing to read here.</b>
    /// <c>engine/DramaEvent.cs</c> carries LegIndex, Step, TotalSteps, Type, WinProbAfter and Tag
    /// and no actor of any kind — no scorer, no possession side — and the engine is not this
    /// lane's to change. So §3 of that spec fires, by its own terms: <i>"If the actor is
    /// unavailable, the momentum beat takes a CLUB-FREE line."</i></para>
    ///
    /// <para>§3 authored four momentum lines and they are used verbatim below. It authored none
    /// for a GOAL, because on the TV the scorer was assumed knowable; on this surface it is not,
    /// so the two goal lines are assembled from clauses already authored for this exact case —
    /// see each field. <b>The line copy is the part of <c>K17-cl</c> the DD left NOT RULED and it
    /// is reported as a lane finding, not settled here.</b></para>
    /// </summary>
    private static string NeitherLine(DramaEventType type, bool up, int step)
    {
        string[] variants = (type, up) switch
        {
            (DramaEventType.Score, true) or (DramaEventType.BigPlay, true) => NeitherGoalUp,
            (DramaEventType.Score, false) or (DramaEventType.BigPlay, false) => NeitherGoalDown,
            (DramaEventType.Momentum, true) => NeitherMomUp,
            _ => NeitherMomDown,
        };
        return variants[step % variants.Length];
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

    // ---- K17-cl / T163: the *neither* branch. No slot, so no club can reach these lines. ----

    /// <summary>ASSEMBLED, NOT AUTHORED, and flagged as such. <c>ScoreUp</c>'s third variant already
    /// ships <c>"Goal for {picked} — the number ticks with it."</c>; this is that clause with the
    /// club-naming subject replaced by the file's own club-free goal subject (<c>"a goal in the
    /// churn — …"</c>, the scorer branch). No new idiom is introduced. The DD did not rule the
    /// console's neither-branch copy and this is the lane's smallest honest stand-in for it.</summary>
    private static readonly string[] NeitherGoalUp =
    {
        "a goal — the number ticks with it.",
    };

    /// <summary>ASSEMBLED, NOT AUTHORED. <c>spec-neither-branch-lines</c> §2 authored
    /// <c>"{other} score against the slip."</c> for exactly this branch and says why the phrase
    /// works: it states that the goal works against the ticket WITHOUT NAMING A SIDE IT WORKS FOR.
    /// With no actor to fill the slot, the club-free subject carries it instead.</summary>
    private static readonly string[] NeitherGoalDown =
    {
        "a goal against the slip.",
    };

    /// <summary><c>spec-neither-branch-lines-2026-08-21.md</c> §3, VERBATIM — the DD's own
    /// club-free momentum fallback, authored for the case where the event's actor is unknowable,
    /// which is this surface's case on every beat. Not re-cased: §4 rules that the new lines match
    /// the table they join exactly and that re-casing a shipped table is its own question.</summary>
    private static readonly string[] NeitherMomUp =
    {
        "The half tightens.",
        "Territory, and the clock with it.",
    };

    /// <summary><c>spec-neither-branch-lines-2026-08-21.md</c> §3, VERBATIM.</summary>
    private static readonly string[] NeitherMomDown =
    {
        "The ball stays in midfield.",
        "Slow through the middle, and no one in a hurry.",
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
