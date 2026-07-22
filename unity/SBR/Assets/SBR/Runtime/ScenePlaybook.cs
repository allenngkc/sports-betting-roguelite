using System;
using SBR.Engine;

namespace SBR.Game
{
    /// <summary>
    /// The theater's scene vocabulary (F_0.2.0 M-T3): goal, count, and outcome templates,
    /// numbered per the plan's
    /// table. #9 (LeadChange turnover intro) and #10 (Swing urgency) are OVERLAYS — playback
    /// modifiers on a <see cref="SceneSpec"/>, never templates — so they carry no enum entry.
    /// #14 (kickoff/restart) is structural playback the stage composes into goal scenes and
    /// leg opens; the resolver never returns it for a beat.
    /// </summary>
    public enum SceneTemplate
    {
        GoalFor = 1,
        GoalAgainst = 2,
        BreakawayFor = 3,
        BreakawayAgainst = 4,
        TerritoryFor = 5,
        TerritoryAgainst = 6,
        NearMissHope = 7,
        NearMissScare = 8,
        CalmPossession = 11,
        LegFinalWon = 12,
        LegFinalLost = 13,
        Kickoff = 14,
        Fallback = 15,
        CornerFor = 16,
        CornerAgainst = 17,
        Booking = 18,
    }

    /// <summary>
    /// One resolved staging order: template + overlays + the ledger's goal decision + the
    /// pacer's duration. Pure data — the stage turns it into motion, the tests turn it into
    /// pinned laws. A chalked-off goal is a <see cref="Goal"/> whose Commits is false: the
    /// full goal drama plays, VAR disallows, the ledger never moves.
    /// </summary>
    public readonly struct SceneSpec
    {
        public readonly SceneTemplate Template;
        /// <summary>Spatial variant 0..2, chosen by Step (deterministic — EventText's trick).</summary>
        public readonly int Variant;
        /// <summary>#9 overlay: steal → transition intro composed onto the base scene.</summary>
        public readonly bool LeadChangeIntro;
        /// <summary>#10 overlay: urgency modifiers (actor speed, pass tempo).</summary>
        public readonly bool Urgent;
        /// <summary>Whether the beat's beneficiary (the side running the move) is the picked
        /// team — the stage routes passes through that team's dots. For/Against templates
        /// already encode it; direction-neutral templates (calm, fallback) rely on this.</summary>
        public readonly bool ForPicked;
        /// <summary>The goal playback this scene stages, if any (ledger-decided commit/chalk).</summary>
        public readonly ScoreLedger.StagedGoal? Goal;
        /// <summary>The count playback this scene stages, if this is a corner/booking scene.</summary>
        public readonly CountLedger.StagedCount? Count;
        /// <summary>Remaining endpoint count playbacks carried by a final scene.</summary>
        public readonly CountLedger.FinalPlan? CountFinal;
        public readonly MarketKind Market;
        /// <summary>Beat-scene seconds from the pacer (finals add correction sub-scenes separately).</summary>
        public readonly float Duration;

        public SceneSpec(SceneTemplate template, int variant, bool leadChangeIntro, bool urgent,
            bool forPicked, ScoreLedger.StagedGoal? goal, float duration)
            : this(template, variant, leadChangeIntro, urgent, forPicked, goal, null, null,
                MarketKind.Moneyline, duration) { }

        public SceneSpec(SceneTemplate template, int variant, bool leadChangeIntro, bool urgent,
            bool forPicked, ScoreLedger.StagedGoal? goal, CountLedger.StagedCount? count,
            CountLedger.FinalPlan? countFinal, MarketKind market, float duration)
        {
            Template = template;
            Variant = variant;
            LeadChangeIntro = leadChangeIntro;
            Urgent = urgent;
            ForPicked = forPicked;
            Goal = goal;
            Count = count;
            CountFinal = countFinal;
            Market = market;
            Duration = duration;
        }
    }

    /// <summary>Template metadata the resolver, pacer, and tests share.</summary>
    public static class ScenePlaybook
    {
        /// <summary>Spatial variants per template (EventText's deterministic variant trick).</summary>
        public const int VariantCount = 3;

        public static int VariantFor(int step)
        {
            int v = step % VariantCount;
            return v < 0 ? v + VariantCount : v;
        }

        /// <summary>Templates that stage a goal playback (and therefore own a chalked-off variant).</summary>
        public static bool ProducesGoal(SceneTemplate t)
            => t == SceneTemplate.GoalFor || t == SceneTemplate.GoalAgainst
            || t == SceneTemplate.BreakawayFor || t == SceneTemplate.BreakawayAgainst;

        public static bool ProducesCorner(SceneTemplate t)
            => t == SceneTemplate.CornerFor || t == SceneTemplate.CornerAgainst;

        public static bool ProducesBooking(SceneTemplate t)
            => t == SceneTemplate.Booking;

        public static bool ProducesCount(SceneTemplate t)
            => ProducesCorner(t) || ProducesBooking(t);

        /// <summary>Beat-scene templates — the ones the [3, 8]s pacer band governs. Finals are
        /// included at their BASE duration; correction sub-scenes are separately timed by
        /// classification. Kickoff (#14) is structural and exempt.</summary>
        public static bool IsBeatScene(SceneTemplate t) => t != SceneTemplate.Kickoff;
    }
}
