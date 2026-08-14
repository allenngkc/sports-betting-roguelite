using System;
using SBR.Engine;

namespace SBR.Game
{
    /// <summary>
    /// The TV's flavour renderer. Ported from game-console/EventText.cs (the proven text-era voice):
    /// engine DramaEvents carry no words on purpose (design/04 - the "sport" is a reskin of the event
    /// vocabulary), so the presentation layer owns the language. Lines are keyed by (Type, direction)
    /// where direction is the sign of the win-prob move for the picked side; the first beat of a leg
    /// compares against the leg's pre-event TrueProb anchor. Variant chosen by Step (deterministic, no
    /// RNG). Tone: deadpan broadcast with a dark-comedy undertow (design/00).
    ///
    /// Also owns the fake broadcast clock, derived from Step/TotalSteps exactly like the console did.
    /// </summary>
    public static class SweatFlavor
    {
        public static string For(DramaEvent e, Leg leg, double prevProb)
        {
            if (e.Type == DramaEventType.LegFinal) return "FINAL WHISTLE";

            bool pickedHome = PickedHomeForPresentation(leg);
            string picked = Short(pickedHome ? leg.Matchup.Home.Name : leg.Matchup.Away.Name);
            string other = Short(pickedHome ? leg.Matchup.Away.Name : leg.Matchup.Home.Name);
            bool up = e.WinProbAfter >= prevProb;

            // Count lines are keyed by the SELECTION's sense of an increment (Over hopes,
            // Under dreads), never the beat's prob direction (Sol, F_0.4.0 P3 r2). These are
            // the default; the orchestrator overrides with NeutralLine when the resolved
            // scene turns out to carry no count event.
            if (leg.Selection.Kind == MarketKind.TotalCorners)
                return CornerLine(leg.Selection.Choice == MarketChoice.Over, leg, e.Step);
            if (leg.Selection.Kind == MarketKind.TotalCards)
                return BookingLine(leg.Selection.Choice == MarketChoice.Over, leg, e.Step);

            // Tag overrides win over the base table.
            if (e.Tag == TensionTag.NearMiss)
                // T39: "a miracle brewing?!" was the hype half of this pair; the other half was
                // already the correct voice. Both are observed now.
                // T44: the ellipsis is the character, not three periods — the sibling line in the
                // console twin already used it, so this file was the odd one out.
                return up ? "off the bar and away." : "…cleared off the line. it's slipping away.";

            string line = Base(e.Type, up, picked, other, e.Step);
            // T44: em dash. TV-32 set this convention and T39's scan did not carry it here — it swept
            // for second person and hype only, so every ASCII sentence dash in this file survived it.
            if (e.Tag == TensionTag.LeadChange) line += " — LEAD CHANGE";
            return line;
        }

        /// <summary>The broadcast clock, soccer-shaped (F_0.2.0 M-T3): a 90-minute match clock
        /// counting UP, derived from the event's position in its leg. The final whistle reads FT.</summary>
        public static string Clock(DramaEvent e)
        {
            if (e.Type == DramaEventType.LegFinal) return "FT";
            return $"{Minute(e)}'";
        }

        /// <summary>The beat's baked minute (position-derived, never outcome-derived) — the
        /// target the theater's continuously ticking clock runs toward. Caps at 89: the 90th
        /// minute belongs to the final sequence's stoppage time.</summary>
        public static int Minute(DramaEvent e)
        {
            double f = e.TotalSteps <= 1 ? 0.99 : Math.Min(0.99, (double)e.Step / e.TotalSteps);
            return Math.Min(89, Math.Max(1, (int)Math.Round(f * 90)));
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
            "{picked} score — far post says yes.", // T44: em dash
            "Goal for {picked} — the number ticks with it.",
        };

        private static readonly string[] ScoreDown =
        {
            "{other} answer right back.",
            // T44: "Ugly." is the strip editorialising. The event strip observes; the register is
            // "incisive, nocturnal, dry, orderly", and a verdict on the goal is none of those.
            "{other} poke one in at the near post.",
            "{other} on the board; the slip flinches.",
        };

        private static readonly string[] BigUp =
        {
            // T39: no second person, no hype. This surface reports the match; it does not address
            // the player and it does not celebrate. "IT'S IN!" is a commentator's shout and the
            // crowd's reaction is not a match fact — the correct voice was already one line away
            // ("...cleared off the line. it's slipping away"): flat, third person, observed.
            "{picked} tear away and finish.",
            "{picked} break the line and score.",
            // T44: "This is happening." predicts the outcome — CF's "Never imply a guaranteed win",
            // and the same shape as the "a miracle brewing?!" T39 removed from the pair above.
            "{picked} counter at full sprint.",
        };

        private static readonly string[] BigDown =
        {
            // T44: "Disaster" is the superlative the ruling names, and it is the strip taking the
            // player's side — the same fault as the gold wash, in words.
            "{other} go the length of the pitch.",
            "{other} rip through on the break.", // T39: "Cover your eyes" instructs the viewer
            "{other} walk it in.", // T44: "That one hurt." editorialises; the fact is the whole line
        };

        private static readonly string[] MomUp =
        {
            "{picked} squeezing the half.",
            "{picked} pin them deep — passes and patience.", // T44: em dash
            "{picked} tighten the screws.",
        };

        private static readonly string[] MomDown =
        {
            "{other} keeping the ball.",
            "{other} pass it around, slow and mean.",
            "{other} settle in; the drift runs the other way.",
        };

        /// <summary>The goal call for a reconciliation-upgraded beat (playtest #14 — the board
        /// catches the bar): the Score tables by the GOAL's beneficiary, so a possession beat
        /// that scores never reads "passes and patience" while the net ripples (Sol, M-T4.1).</summary>
        public static string GoalLine(bool forPicked, Leg leg, int step)
        {
            bool pickedHome = PickedHomeForPresentation(leg);
            string picked = Short(pickedHome ? leg.Matchup.Home.Name : leg.Matchup.Away.Name);
            string other = Short(pickedHome ? leg.Matchup.Away.Name : leg.Matchup.Home.Name);
            return Base(DramaEventType.Score, forPicked, picked, other, step);
        }

        /// <summary>Ordinary-play line for a count-market beat whose resolved scene carries no
        /// count event (a zero batch fell through) — corner/booking words would be a lie there
        /// (Sol, F_0.4.0 P3 r2). Plain possession language, direction from the beat.</summary>
        public static string NeutralLine(DramaEvent e, Leg leg, bool up)
        {
            bool pickedHome = PickedHomeForPresentation(leg);
            string picked = Short(pickedHome ? leg.Matchup.Home.Name : leg.Matchup.Away.Name);
            string other = Short(pickedHome ? leg.Matchup.Away.Name : leg.Matchup.Home.Name);
            return Base(DramaEventType.Momentum, up, picked, other, e.Step);
        }

        public static string CornerLine(bool forPicked, Leg leg, int step)
        {
            string[] lines = forPicked ? CornerFor : CornerAgainst;
            return lines[step % lines.Length];
        }

        public static string BookingLine(bool forPicked, Leg leg, int step)
        {
            string[] lines = forPicked ? BookingFor : BookingAgainst;
            return lines[step % lines.Length];
        }

        private static readonly string[] CornerFor =
        {
            "whipped into the corner — the count moves again.",
            "corner kick won. another little number for the ledger.",
            "the flag goes up; pressure becomes a corner.",
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

        /// <summary>Market legs (O/U, BTTS) have no picked TEAM — presentation anchors them on the
        /// home side; the market label carries the pick. Shared by every renderer so the anchor
        /// can never disagree across surfaces. Moneyline legs answer with their real side.
        ///
        /// <para><b>A MONEYLINE DRAW HAS NO SIDE, and this returned AWAY for it.</b> Routed here by
        /// name from the markets lane's class sweep (`a3d184c`: *"SweatFlavor:206 — draw counts as
        /// away for flavour, ROUTED → tv-sweat"*), and it survived that sweep for the reason worth
        /// recording: **it is in this surface's file, not theirs.** A cross-lane sweep scoped by
        /// ownership misses exactly the code another lane owns.</para>
        ///
        /// <para><b>The defect was a fall-through, not a decision.</b> The old expression asked
        /// `Choice == MarketChoice.Home` and let everything else be false — written when `Choice` on a
        /// moneyline could only be Home or Away, so "not Home" meant Away. `MarketChoice.Draw` made
        /// that inference wrong without touching the line: the third value silently inherited the
        /// second one's branch. C46's shape in an expression rather than in a box.</para>
        ///
        /// <para><b>The fix is the rule this summary already states, applied to a case written before
        /// draws existed:</b> a draw has no picked team, exactly like O/U and BTTS, so it takes the
        /// same HOME anchor they do and the market label carries the pick. It is not new design — the
        /// only new thing is that a third no-team case now exists.</para>
        ///
        /// <para><b>Deliberately NOT the null the markets lane used</b> (`BetslipModel.SideOn` returns
        /// null for a draw and is pinned for it). That answers a different question: `SideOn` reports
        /// WHICH SIDE YOU BACKED, where a draw's honest answer is "neither". This answers WHICH TEAM
        /// THE PROSE ANCHORS ON, where every leg needs an answer and "neither" would leave the
        /// flavour with no names. Same finding, two functions, two correct shapes.</para>
        ///
        /// <para><b>Routed, not authored:</b> whether the flavour's VOICE reads correctly on a
        /// draw-backed leg — `{picked}`/`{other}` naming the home side while the pick is the draw — is
        /// a copy question for the DD. The direction is unaffected either way: up/down comes from the
        /// leg's own win-prob move, not from the name anchor.</para></summary>
        public static bool PickedHomeForPresentation(Leg leg)
            => leg.Selection.Kind == MarketKind.AnytimeScorer
                ? leg.Matchup.PlayerSide(leg.Selection.PlayerIndex) == Side.Home
                : leg.Selection.Kind != MarketKind.Moneyline
                    || leg.Selection.Choice == MarketChoice.Home
                    || leg.Selection.Choice == MarketChoice.Draw;

        /// <summary>The team's noun (last word of the "City Noun" name) - punchier for the ticker.</summary>
        public static string Short(string teamName)
        {
            int i = teamName.LastIndexOf(' ');
            return i >= 0 ? teamName.Substring(i + 1) : teamName;
        }
    }
}
