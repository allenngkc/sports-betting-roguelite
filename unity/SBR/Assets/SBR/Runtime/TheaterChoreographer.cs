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
    ///   2. A corners leg resolves to #16/#17 and a cards leg to #18. Two SEPARATE facts drive
    ///      these, never conflated (TVS-S01 fix, PRD §7.6): which TEMPLATE plays — #16 vs #17,
    ///      the bettor's hope/dread — is the selection's sense (Under/No are dread when the
    ///      count rises, F_0.4.0 P3 r2), exactly like before; which TEAM's dots physically run
    ///      the move is the staged batch's own home/away beneficiary, carried separately via
    ///      <see cref="SceneSpec.CountBeneficiaryIsHome"/> and never inferred from the template.
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
                // The batch itself is pre-planned from the locked stat line and fires on
                // schedule regardless of the bet — StageBeat takes no bet input at all.
                CountLedger.StagedCount? count = countLedger?.StageBeat();
                // A zero batch stages NO count event — the beat falls through to ordinary
                // play (a booking scene with nothing booked reads as a lie; Sol, F_0.4.0 P3).
                if (count.HasValue && count.Value.TotalDelta > 0)
                {
                    // Concept 2 — MOOD: an increment's hope/dread is fixed by the SELECTION,
                    // never the beat's prob direction or which team the engine credits it to —
                    // a corner always bites an Under bettor, even when the engine credits it to
                    // the "wrong" team for that story (Sol, F_0.4.0 P3 r2). This chooses
                    // CornerFor/CornerAgainst for corners, and — since Booking has no For/Against
                    // template split — also rides along as SceneSpec.ForPicked for Booking's use.
                    // NEVER read for routing (reviewer correction, TVS-S01 follow-up): mood and
                    // routing are independent, and conflating them either way (bet driving
                    // routing, or team driving mood) is the same class of bug in two directions.
                    bool countHelps = leg.Selection.Choice == MarketChoice.Over;
                    // Concept 1 — ROUTING: which TEAM wins the corner / commits the foul is read
                    // from the staged fact's beneficiary (StagedCount.BeneficiaryIsHome, derived
                    // only from HomeDelta/AwayDelta) — never from the bettor's Over/Under pick.
                    // Totals markets have no picked TEAM (SweatFlavor.PickedHomeForPresentation),
                    // so this rides its own home/away field (SceneSpec.CountBeneficiaryIsHome)
                    // rather than overloading ForPicked's picked-relative meaning, which the goal
                    // path below still owns. The stage reads this directly for both Booking
                    // (single template) and Corner (For/Against template's Mirror decision, which
                    // must NOT be driven by which template — see TheaterStage.cs).
                    bool beneficiaryIsHome = count.Value.BeneficiaryIsHome;
                    bool corners = market == MarketKind.TotalCorners;
                    SceneTemplate countTemplate = corners
                        ? (countHelps ? SceneTemplate.CornerFor : SceneTemplate.CornerAgainst)
                        : SceneTemplate.Booking;
                    bool countIntro = evt.Tag == TensionTag.LeadChange;
                    return new SceneSpec(countTemplate, variant, countIntro, evt.Tag == TensionTag.Swing,
                        countHelps, null, count, null, market,
                        _pacer.SceneSeconds(countTemplate, countIntro), beneficiaryIsHome);
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
            // TVS-S01 fix: each remaining batch attributes from its own HomeDelta/AwayDelta —
            // CountLedger.PlanFinal no longer takes a bet-derived flag.
            CountLedger.FinalPlan? countFinal = countLedger?.PlanFinal();
            return new SceneSpec(template, ScenePlaybook.VariantFor(step), false, false,
                grade == LegGrade.Won, null, null, countFinal, market, _pacer.SceneSeconds(template, false));
        }
    }
}
