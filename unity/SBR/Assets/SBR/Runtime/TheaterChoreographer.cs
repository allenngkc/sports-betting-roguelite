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
    ///   2. A corners leg resolves to #16/#17 and a cards leg to #18. The beat direction is
    ///      the selection direction: Under and No are down when the count rises.
    ///   3. Tag == NearMiss → #7 (up, "miracle brewing") / #8 (down, "slipping away").
    ///      Never a goal, regardless of Type, for goal-family legs.
    ///   4. Base scene by (Type, dir) — Score: #1/#2 · BigPlay: #3/#4 · Momentum: #5/#6
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

        /// <summary>Resolves a beat. <paramref name="up"/> and <paramref name="delta"/> are the
        /// model's direction and signed movement (the one authority — BeatRecord);
        /// <paramref name="ledger"/> decides goal commit vs chalked-off, including the
        /// prob-reconciliation source whose sign gate needs the raw delta (flat ≠ up).</summary>
        public SceneSpec ResolveBeat(DramaEvent evt, bool up, double delta, ScoreLedger ledger)
            => ResolveBeat(evt, up, delta, ledger, null, null);

        public SceneSpec ResolveBeat(DramaEvent evt, bool up, double delta, ScoreLedger ledger,
            Leg leg, CountLedger countLedger)
        {
            int variant = ScenePlaybook.VariantFor(evt.Step);

            MarketKind market = leg == null ? MarketKind.Moneyline : leg.Selection.Kind;
            // LegFinal must fall through to the total outcome-scene case below — the count
            // branch consuming a scheduled batch on a final would corrupt PlanFinal's remainder.
            if (evt.Type != DramaEventType.LegFinal
                && (market == MarketKind.TotalCorners || market == MarketKind.TotalCards))
            {
                // A count event's hope/dread is fixed by the SELECTION, never the beat's prob
                // direction: an increment helps Over and bites Under, even on a beat whose
                // price drifted the bettor's way — corners can come, just slower than the
                // line needs (Sol, F_0.4.0 P3 r2). The price still moves honestly at payoff.
                bool countHelps = leg.Selection.Choice == MarketChoice.Over;
                CountLedger.StagedCount? count = countLedger == null ? null : countLedger.StageBeat(countHelps);
                // A zero batch stages NO count event — the beat falls through to ordinary
                // play (a booking scene with nothing booked reads as a lie; Sol, F_0.4.0 P3).
                if (count.HasValue && count.Value.TotalDelta > 0)
                {
                    bool corners = market == MarketKind.TotalCorners;
                    SceneTemplate countTemplate = corners
                        ? (countHelps ? SceneTemplate.CornerFor : SceneTemplate.CornerAgainst)
                        : SceneTemplate.Booking;
                    bool countIntro = evt.Tag == TensionTag.LeadChange;
                    return new SceneSpec(countTemplate, variant, countIntro, evt.Tag == TensionTag.Swing,
                        countHelps, null, count, null, market,
                        _pacer.SceneSeconds(countTemplate, countIntro));
                }
            }

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

            // The ledger owns BOTH goal sources (type attribution + prob reconciliation,
            // playtest #14). A reconciliation goal on a momentum beat UPGRADES the scene to
            // the goal template — the board only ever moves behind a staged goal, and a goal
            // must look like one.
            ScoreLedger.StagedGoal? goal = ledger.StageBeatGoal(evt.Type, up, delta, evt.WinProbAfter);
            if (goal.HasValue && !ScenePlaybook.ProducesGoal(template))
                template = goal.Value.ScoredByPicked ? SceneTemplate.GoalFor : SceneTemplate.GoalAgainst;
            else if (goal.HasValue)
            {
                // A staged goal's scene must attack from the SCORER'S side — on a market leg
                // the money direction and the scoring team can disagree (Sol, F_0.4.0 P3).
                bool breakaway = template == SceneTemplate.BreakawayFor
                    || template == SceneTemplate.BreakawayAgainst;
                template = breakaway
                    ? (goal.Value.ScoredByPicked ? SceneTemplate.BreakawayFor : SceneTemplate.BreakawayAgainst)
                    : (goal.Value.ScoredByPicked ? SceneTemplate.GoalFor : SceneTemplate.GoalAgainst);
            }

            return new SceneSpec(template, variant, leadChange, urgent, up, goal, null, null,
                market, _pacer.SceneSeconds(template, leadChange));
        }

        /// <summary>The real LegFinal staging: scene #12/#13 from the FINAL ticket-local grade,
        /// with the ledger's correction plan riding along. Works for both the unsuspended final
        /// and a suspended scene's continuation.</summary>
        public SceneSpec ResolveFinal(LegGrade grade, int step)
            => ResolveFinal(grade, step, null, null, null);

        public SceneSpec ResolveFinal(LegGrade grade, int step, ScoreLedger ledger,
            CountLedger countLedger, Leg leg)
        {
            SceneTemplate template = grade == LegGrade.Won ? SceneTemplate.LegFinalWon : SceneTemplate.LegFinalLost;
            MarketKind market = leg == null ? MarketKind.Moneyline : leg.Selection.Kind;
            bool countForPicked = leg == null || leg.Selection.Choice == MarketChoice.Over;
            CountLedger.FinalPlan? countFinal = countLedger == null ? null : countLedger.PlanFinal(countForPicked);
            return new SceneSpec(template, ScenePlaybook.VariantFor(step), false, false,
                grade == LegGrade.Won, null, null, countFinal, market, _pacer.SceneSeconds(template, false));
        }
    }
}
