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
        /// <summary>On a goal/moneyline-family scene: whether the beat's beneficiary (the side
        /// running the move) is the picked team — the stage routes passes through that team's
        /// dots. For/Against templates already encode it; direction-neutral templates (calm,
        /// fallback) rely on this. Coherent for team markets (moneyline) where the pick IS a
        /// team.
        /// On a count scene (corner/booking): totals markets have no picked team at all
        /// (PRD §7.6), so this field instead carries the bettor's hope/dread MOOD — the
        /// selection's Over/Under sense (F_0.4.0 P3 r2), the same value that chose
        /// CornerFor/CornerAgainst for a corner. It rides along for Booking too (which has no
        /// For/Against template split to carry the mood instead) but is not currently read by
        /// any renderer there — reserved for a future mood-differentiated Booking treatment
        /// (sting, flavor line) rather than silently dropped. This field must NEVER drive which
        /// team's dots physically move for a count scene, for either template — that is
        /// <see cref="CountBeneficiaryIsHome"/>'s job, exclusively (TVS-S01 follow-up: routing
        /// driven by this field, in either direction, is the class of bug being guarded against).</summary>
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
        /// <summary>TVS-S01 fix (PRD §7.6): which TEAM the engine actually credited this
        /// corner/booking batch to — home when true, away when false — mirroring
        /// <see cref="CountLedger.StagedCount.BeneficiaryIsHome"/>. Null for every scene that
        /// is not a count scene. This is intentionally a SEPARATE field from
        /// <see cref="ForPicked"/> rather than a repurposing of it: "which team" (absolute,
        /// home/away) and "is it my team"/"does this help my bet" (relative to a picked side or
        /// selection) are different concepts, and totals markets only have the former.
        /// Consulted directly, and EXCLUSIVELY, by the stage for routing on both count
        /// templates: Booking's single-template `atkPicked`, and Corner's For/Against Mirror
        /// decision. The CornerFor/CornerAgainst TEMPLATE choice itself carries no routing
        /// information any more — it is <see cref="ForPicked"/>'s hope/dread mood, entirely
        /// independent of this field (reviewer correction: a prior revision of this fix
        /// conflated the two, making Corner's routing follow the template/bet again).</summary>
        public readonly bool? CountBeneficiaryIsHome;
        /// <summary>spec-count-theater-2026-08-17.md §4, "a quiet corner must still count": a count
        /// batch this beat CONSUMED (<see cref="CountLedger.StageBeat"/> already advanced the
        /// ledger's cursor unconditionally when this spec was resolved) but DECLINED to stage as a
        /// scene. The binding rule: "no beat may consume a count batch without committing it. A
        /// corner that earns no scene is still a corner — the count is a fact; only the drama is
        /// discretionary." When set, the batch must still be committed (TvSweatScreen's
        /// CommitRevealedCount) — with no flavour text and no audio, because this field itself
        /// carries no drama, only the fact.
        ///
        /// <para>Deliberately a SEPARATE field from <see cref="Count"/>, never a reuse of it:
        /// <c>Count</c> means "this beat narrates a count event" and drives real presentation —
        /// TvSweatScreen's <c>countScene</c> check, the " ({n} in the spell)" flavour suffix, and
        /// the dangerous-scene reveal/audio routing all key off <c>Count.HasValue</c>. Staging a
        /// quiet batch through <c>Count</c> would narrate and sound a corner that earned no scene —
        /// the exact opposite of the rule this field exists to satisfy. A resolved beat carries AT
        /// MOST ONE of <c>Count</c>/<c>QuietCount</c> populated — the resolver either stages the
        /// batch as a scene or commits it silently, never both.</para>
        ///
        /// <para><b>This phase (the commit path only) never populates this field — it stays null on
        /// every path.</b> Nothing yet declines a batch (see <c>TheaterChoreographer.ResolveBeat</c>'s
        /// count branch: every batch with <c>TotalDelta &gt; 0</c> still stages a scene via
        /// <c>Count</c>), so no existing call site sets this and no existing fixture ever sees it
        /// populated. It exists ahead of the later significance gate that will set it, so that gate
        /// lands on top of a commit path that already exists rather than inventing one under
        /// time pressure.</para></summary>
        public readonly CountLedger.StagedCount? QuietCount;

        public SceneSpec(SceneTemplate template, int variant, bool leadChangeIntro, bool urgent,
            bool forPicked, ScoreLedger.StagedGoal? goal, float duration)
            : this(template, variant, leadChangeIntro, urgent, forPicked, goal, null, null,
                MarketKind.Moneyline, duration, null) { }

        public SceneSpec(SceneTemplate template, int variant, bool leadChangeIntro, bool urgent,
            bool forPicked, ScoreLedger.StagedGoal? goal, CountLedger.StagedCount? count,
            CountLedger.FinalPlan? countFinal, MarketKind market, float duration,
            bool? countBeneficiaryIsHome = null, CountLedger.StagedCount? quietCount = null)
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
            CountBeneficiaryIsHome = countBeneficiaryIsHome;
            QuietCount = quietCount;
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
