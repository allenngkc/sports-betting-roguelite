using System;

namespace SBR.Game
{
    /// <summary>
    /// The theater's tempo (F_0.2.0 M-T3): resolved-scene-class → seconds. All dials, all
    /// presentation config — the engine never sees these. Serialized on TvSweatScreen so
    /// Allen tunes them at the M-T3 gate; pure C# (no UnityEngine) so EditMode pins the
    /// bands: every beat-scene lands in [3, 8]s, ledger-correction goals are separately
    /// timed sub-scenes (2.5s each, at most 2 by the clamp), so a full final whistle
    /// sequence may run 8–13s — the slowest moment is deliberately the reveal.
    /// </summary>
    [Serializable]
    public sealed class SweatPacer
    {
        public float territorySeconds = 3.0f;
        public float calmSeconds = 3.0f;
        public float goalSeconds = 4.5f;
        public float breakawaySeconds = 5.0f;
        /// <summary>#9 overlay: the steal → transition intro adds this on top of the base scene.</summary>
        public float leadChangeIntroExtra = 0.8f;
        public float nearMissSeconds = 6.5f;
        /// <summary>The dead-air hold after the ball skims wide / hits the bar.</summary>
        public float nearMissHold = 0.5f;
        /// <summary>LegFinal base (pre-reveal hold + decisive sequence + whistle), before corrections.</summary>
        public float legFinalSeconds = 7.5f;
        /// <summary>Each ledger-correction goal, appended to the LegFinal as its own sub-scene.</summary>
        public float correctionGoalSeconds = 2.5f;
        public float fallbackSeconds = 3.0f;
        /// <summary>Structural kickoff/restart (#14) — outside the beat-scene band by classification.</summary>
        public float kickoffSeconds = 1.5f;

        /// <summary>The BEAT-SCENE duration for a resolved template (+overlays). Near-miss
        /// includes its hold; finals return their base — corrections are added by
        /// <see cref="FinalSceneSeconds"/>.</summary>
        public float SceneSeconds(SceneTemplate template, bool leadChangeIntro)
        {
            float s = template switch
            {
                SceneTemplate.GoalFor => goalSeconds,
                SceneTemplate.GoalAgainst => goalSeconds,
                SceneTemplate.BreakawayFor => breakawaySeconds,
                SceneTemplate.BreakawayAgainst => breakawaySeconds,
                SceneTemplate.TerritoryFor => territorySeconds,
                SceneTemplate.TerritoryAgainst => territorySeconds,
                SceneTemplate.NearMissHope => nearMissSeconds + nearMissHold,
                SceneTemplate.NearMissScare => nearMissSeconds + nearMissHold,
                SceneTemplate.CalmPossession => calmSeconds,
                SceneTemplate.LegFinalWon => legFinalSeconds,
                SceneTemplate.LegFinalLost => legFinalSeconds,
                SceneTemplate.Kickoff => kickoffSeconds,
                _ => fallbackSeconds,
            };
            if (leadChangeIntro) s += leadChangeIntroExtra;
            return s;
        }

        /// <summary>The full final whistle sequence: base + one sub-scene per staged correction
        /// goal (the killing shot on a Lost is one of them).</summary>
        public float FinalSceneSeconds(int stagedGoals) => legFinalSeconds + stagedGoals * correctionGoalSeconds;
    }
}
