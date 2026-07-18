using SBR.Engine;

namespace SBR.Game
{
    /// <summary>
    /// DramaEvent → SceneSpec (F_0.2.0 M-T3): the ORDERED resolver over the actual event
    /// fields — not a flat key tuple. Total by construction over all 40 (Type × dir × Tag)
    /// combos; unreachable combos per generator invariants (BigPlay×Calm, Momentum×Swing,
    /// non-final Decisive) resolve through the same path with no special-casing, and the
    /// default arm falls back to #15 rather than throwing so future enum additions play.
    ///
    ///   1. Type == LegFinal → outcome scene. The UNSUSPENDED path chooses #12/#13 from the
    ///      revealed grade via <see cref="ResolveFinal"/>; <see cref="ResolveBeat"/> keeps the
    ///      resolver total by reading WinProbAfter (1.0 → Won). A SUSPENDED LegFinal's
    ///      continuation is chosen from the FINAL ticket-local grade after resolution —
    ///      never from WinProbAfter (single presentation authority).
    ///   2. Tag == NearMiss → #7 (up, "miracle brewing") / #8 (down, "slipping away").
    ///      Never a goal, regardless of Type.
    ///   3. Base scene by (Type, dir) — Score: #1/#2 · BigPlay: #3/#4 · Momentum: #5/#6
    ///      (Momentum with Tag==Calm uses the #11 calm variant).
    ///   4. Overlays (playback modifiers, never template choice): LeadChange → #9 intro,
    ///      Swing → #10 urgency.
    ///
    /// Pure C# — EditMode pins totality, goal staging, and the duration acceptance criterion.
    /// </summary>
    public sealed class TheaterChoreographer
    {
        private readonly SweatPacer _pacer;

        public TheaterChoreographer(SweatPacer pacer)
        {
            _pacer = pacer ?? new SweatPacer();
        }

        /// <summary>Resolves a beat. <paramref name="up"/> is the model's direction (the one
        /// authority); <paramref name="ledger"/> decides goal commit vs chalked-off.</summary>
        public SceneSpec ResolveBeat(DramaEvent evt, bool up, ScoreLedger ledger)
        {
            int variant = ScenePlaybook.VariantFor(evt.Step);

            // 1. LegFinal — outcome scene (kept total here; the orchestrator's real final path
            //    goes through ResolveFinal with the revealed grade and the correction plan).
            if (evt.Type == DramaEventType.LegFinal)
            {
                SceneTemplate final = evt.WinProbAfter >= 0.5
                    ? SceneTemplate.LegFinalWon
                    : SceneTemplate.LegFinalLost;
                return new SceneSpec(final, variant, false, false,
                    final == SceneTemplate.LegFinalWon, null, _pacer.SceneSeconds(final, false));
            }

            // 2. NearMiss wins over the base table — never a goal, regardless of Type.
            if (evt.Tag == TensionTag.NearMiss)
            {
                SceneTemplate miss = up ? SceneTemplate.NearMissHope : SceneTemplate.NearMissScare;
                return new SceneSpec(miss, variant, false, false, up, null,
                    _pacer.SceneSeconds(miss, false));
            }

            // 3. Base scene by (Type, dir); 4. overlays never change the template.
            bool leadChange = evt.Tag == TensionTag.LeadChange;
            bool urgent = evt.Tag == TensionTag.Swing;
            SceneTemplate template = evt.Type switch
            {
                DramaEventType.Score => up ? SceneTemplate.GoalFor : SceneTemplate.GoalAgainst,
                DramaEventType.BigPlay => up ? SceneTemplate.BreakawayFor : SceneTemplate.BreakawayAgainst,
                DramaEventType.Momentum => evt.Tag == TensionTag.Calm
                    ? SceneTemplate.CalmPossession
                    : up ? SceneTemplate.TerritoryFor : SceneTemplate.TerritoryAgainst,
                _ => SceneTemplate.Fallback, // future enum additions play, never throw
            };

            ScoreLedger.StagedGoal? goal = ScenePlaybook.ProducesGoal(template)
                ? ledger.StageBeatGoal(evt.Type, up)
                : null;

            return new SceneSpec(template, variant, leadChange, urgent, up, goal,
                _pacer.SceneSeconds(template, leadChange));
        }

        /// <summary>The real LegFinal staging: scene #12/#13 from the FINAL ticket-local grade,
        /// with the ledger's correction plan riding along. Works for both the unsuspended final
        /// and a suspended scene's continuation.</summary>
        public SceneSpec ResolveFinal(LegGrade grade, int step)
        {
            SceneTemplate template = grade == LegGrade.Won ? SceneTemplate.LegFinalWon : SceneTemplate.LegFinalLost;
            return new SceneSpec(template, ScenePlaybook.VariantFor(step), false, false,
                grade == LegGrade.Won, null, _pacer.SceneSeconds(template, false));
        }
    }
}
