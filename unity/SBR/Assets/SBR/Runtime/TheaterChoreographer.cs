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
            // spec-count-theater-2026-08-17.md §4: a batch the distance gate below declines to
            // stage as a scene must still be committed — StageBeat() advances the ledger's
            // cursor unconditionally the instant it is called, so a declined batch is a fact
            // that already happened, not one that gets to vanish. Set only inside the count
            // branch's gate; threaded onto whichever SceneSpec the fall-through actually builds,
            // as QuietCount (never Count — see that field's own doc for why the two must never
            // conflate).
            CountLedger.StagedCount? pendingQuietCount = null;
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

                    // spec-count-theater-2026-08-17.md §3, THE DISTANCE GATE: "an event earns
                    // its treatment from its DISTANCE TO THE LINE, not from having arrived."
                    // This gates the count branch's ENTRY — it never computes both and prefers
                    // calm; the spec is explicit that the cheaper-looking "compute both, prefer
                    // calm" edit is the wrong one, because it leaves the short-circuit's cost
                    // (CalmPossession never even gets CONSTRUCTED) in place. Three scope
                    // restrictions, each named in §6/§3.4, each load-bearing on its own — do not
                    // widen any of them beyond what is written here:
                    //
                    //  1. CORNERS ONLY (`corners`). §6: cards are "the opposite problem — a
                    //     booking arrives carrying its own significance and needs catching, not
                    //     ramping... No cards arm has ever been shot." A cards leg (Booking
                    //     template) always takes the `!quiet` branch below, unconditionally —
                    //     today's behaviour exactly.
                    //  2. OVER ONLY. `countHelps`, already computed above for mood, IS the Over
                    //     check here — it is literally `Choice == Over` for both corners and
                    //     cards, so it doubles as this scope test without a second field. §6: the
                    //     Under case is "the mirror distance profile, not in evidence." An Under
                    //     leg always takes the `!quiet` branch too — today's behaviour exactly.
                    //  3. MOMENTUM BEATS ONLY, AND NEVER A NEAR-MISS
                    //     (`evt.Type == Momentum && evt.Tag != NearMiss`) — the restriction that
                    //     protects a spec-level constraint, so the one most important not to
                    //     relax. NOT because a Momentum beat structurally CANNOT stage a goal —
                    //     it CAN: on a corners/cards leg ScoreLedger's goal sense is neutral
                    //     (`_goalSense == 0`, "the neutral home-anchored goal decoration",
                    //     SweatPresentationModel.cs), the exact same shape as a moneyline leg, so
                    //     StageBeatGoal's prob-RECONCILIATION arm is reachable on ANY beat type,
                    //     Momentum included — that arm is what the goal-suppression below exists
                    //     for. A Score/BigPlay beat is worse, not merely different: its
                    //     TYPE-ATTRIBUTION arm (`if (typeGoal)`) is UNCONDITIONAL — it always
                    //     returns a StagedGoal, chalked or committing, regardless of probability
                    //     bands — so quieting one would CERTAINLY, not just possibly, fall
                    //     through into a goal scene. Restricting to Momentum is instead because
                    //     T113 (the calm-beat probe) measured that the reclaimable calm beats are
                    //     EXACTLY the Momentum ones — that is the set the spec's own evidence
                    //     covers, so confining the gate to it keeps the change's blast radius no
                    //     wider than the measurement behind it. NearMiss is excluded separately
                    //     and for a different reason: the generator CAN tag a Momentum-typed beat
                    //     NearMiss (the near-miss injection clamps toward 0.75/0.25 independently
                    //     of the Type-classifying delta — engine/DramaGenerator.cs — and this
                    //     exact combination is already live in
                    //     TheaterChoreographerTests.Reconciliation_upgrades_a_momentum_beat_to_a_goal_scene),
                    //     and rule 2 immediately below (NearMiss wins over the base table) has no
                    //     QuietCount slot to carry a declined batch through — see its own comment.
                    //     A Score/BigPlay beat, and a NearMiss-tagged beat of any type, keep
                    //     today's count-branch behaviour whatever their distance.
                    bool gateEligible = corners && countHelps
                        && evt.Type == DramaEventType.Momentum && evt.Tag != TensionTag.NearMiss;

                    // Non-half-line lines admit no exact distance (a push is possible —
                    // SweatActiveLegModel.HalfLineThreshold's own contract), so significance
                    // cannot be computed at all here, not merely computed as Ordinary. DEFAULT
                    // LOUD: falling back to quiet would silently mute a market on a config no
                    // generator produces today (RunConfig lines are all half-integers — this
                    // branch is defensive only); falling back to loud is merely the status quo.
                    // Checked BEFORE Classify runs — see Classify's own doc for why the split.
                    bool computable = gateEligible
                        && SweatActiveLegModel.HalfLineThreshold(leg.Selection.Line, out _);

                    bool quiet = false;
                    // §3.5's carrier. NULLABLE, and the null case is load-bearing: it means THIS
                    // BEAT WAS NEVER CLASSIFIED, which is NOT the same as classifying Ordinary.
                    //
                    // The `!quiet` branch below is reached two ways, and only one of them is a
                    // decisive beat. GATED: the classifier ran and returned Approach or Turn.
                    // UNGATED: `computable` was false — a cards leg, an Under leg, a Score/BigPlay
                    // beat, a NearMiss beat, a whole-number line — so `quiet` stayed false by
                    // DEFAULT and the count scene plays regardless of distance, which is an
                    // ordinary event.
                    //
                    // A bool would conflate those two and hand an ordinary corner the decisive
                    // pool's copy, which is the recycling defect §3.5 exists to make
                    // unconstructible, arriving from the opposite direction.
                    CountSignificance? decisive = null;
                    if (computable)
                    {
                        // Revealed, never locked: countLedger.Home/Away are mutated ONLY by
                        // CompleteCount (from a narrated scene's OnCountPlayed payoff, or from
                        // TvSweatScreen's QuietCount commit path) — StageBeat() above never
                        // touches them, so this is the state strictly BEFORE this beat's own
                        // batch, exactly the "revealedTotal" the classifier's contract wants.
                        int revealedTotal = countLedger.Home + countLedger.Away;
                        CountSignificance significance = SweatActiveLegModel.Classify(
                            revealedTotal, count.Value.TotalDelta, leg.Selection.Line);
                        // Quiet = Ordinary or Decided (spec §3's own mapping); Significant =
                        // Approach or Turn keeps today's scene, unchanged, below.
                        quiet = significance == CountSignificance.Ordinary
                            || significance == CountSignificance.Decided;
                        // Recorded only where the classifier actually ran — see `decisive`'s note.
                        decisive = significance;
                    }

                    if (!quiet)
                    {
                        SceneTemplate countTemplate = corners
                            ? (countHelps ? SceneTemplate.CornerFor : SceneTemplate.CornerAgainst)
                            : SceneTemplate.Booking;
                        bool countIntro = evt.Tag == TensionTag.LeadChange;
                        // spec-count-theater-2026-08-17.md §2 STEPS 1+2 (T109-cl, ruled FINAL):
                        // this early return is the mechanism's item 2, verbatim — "the count
                        // branch RETURNS before ledger.StageBeatGoal(...) is ever called...
                        // every [non-final beat on a corners leg] took that branch [so] goal
                        // staging was never reached." Reached now, unconditionally, whether or
                        // not this beat's own event type would independently have attributed a
                        // goal (Score/BigPlay can still land here whenever their beat also
                        // carries a nonzero count batch — the count branch takes priority over
                        // type attribution by construction, unchanged). Carried as quietGoal,
                        // never as this SceneSpec's Goal slot (left null, as before): STEP 2
                        // forbids a goal from hijacking "the corner/calm scene the grammar
                        // chose" — countTemplate plays exactly as computed above, and
                        // TvSweatScreen.CommitRevealedGoal (mirroring CommitRevealedCount)
                        // commits the goal on this same beat regardless, since no goal-scene
                        // payoff will ever fire for a CornerFor/CornerAgainst/Booking template.
                        // Side-effect free (see StageBeatGoal's own doc: CompleteGoal is the
                        // ledger's ONLY mutator), so computing it here rather than never changes
                        // nothing about the ledger except that the answer is no longer discarded.
                        // Named distinctly from the fall-through path's own `quietGoal` below:
                        // C# forbids reusing the name across the enclosing scope, and the two are
                        // genuinely different cases — this one rides a COUNT SCENE that is showing,
                        // that one rides a beat whose scene was quieted.
                        // T164 — THE LEG-SCOPED READS IN THIS METHOD, STATED ONCE (the two sites
                        // below reference this note rather than restating it). Goal staging and the
                        // final template are per-LEG dramatic facts and legitimately stay
                        // leg-scoped: neither is a displayed probability, so T143/T164's "only the
                        // ticket's number may be shown" does not reach them. What DID change is the
                        // source: after the fixture restructure a telling is a (ticket, FIXTURE),
                        // two or more legs can be live on one telling, and evt.WinProbAfter is only
                        // the ANCHOR leg's — so reading it would silently hand one leg's number to
                        // every leg. LegProbs is the parallel per-leg list (ticket order, alongside
                        // LegIndices) and is what these read instead. Value-identical today:
                        // LegProbs[0] IS WinProbAfter on every event. WHICH leg the N-live answer
                        // should key on is step 2's question (§6a), not this change's.
                        ScoreLedger.StagedGoal? countSceneQuietGoal =
                            ledger.StageBeatGoal(evt.Type, up, delta, evt.LegProbs[0]);
                        return new SceneSpec(countTemplate, variant, countIntro, evt.Tag == TensionTag.Swing,
                            countHelps, null, count, null, market,
                            _pacer.SceneSeconds(countTemplate, countIntro), beneficiaryIsHome,
                            quietGoal: countSceneQuietGoal, decisive: decisive);
                    }

                    // Quiet: StageBeat() already consumed this batch above — §4's binding says it
                    // must still be committed even though it earns no scene here. Stash it; the
                    // fall-through below threads it onto whatever SceneSpec it actually builds.
                    pendingQuietCount = count;
                }
            }

            // 1. LegFinal — outcome scene (kept total here; the orchestrator's real final path
            //    goes through ResolveFinal with the revealed grade and the correction plan).
            if (evt.Type == DramaEventType.LegFinal)
            {
                // T164: leg-scoped, reading LegProbs rather than the anchor-only WinProbAfter —
                // see the note at the StageBeatGoal call above.
                SceneTemplate final = evt.LegProbs[0] >= 0.5
                    ? SceneTemplate.LegFinalWon
                    : SceneTemplate.LegFinalLost;
                return new SceneSpec(final, variant, false, false,
                    final == SceneTemplate.LegFinalWon, null, _pacer.SceneSeconds(final, false));
            }

            // 2. NearMiss wins over the base table — never a goal, regardless of Type. This
            // return CANNOT be reached carrying a pendingQuietCount: the distance gate above
            // excludes every NearMiss-tagged beat from gateEligible precisely BECAUSE this
            // return has no QuietCount slot (its SceneSpec constructor overload hard-codes
            // quietCount: null via the chained 7-arg overload) — so a quieted batch is never at
            // risk of landing here silently dropped. Left otherwise untouched.
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
            // must look like one. That upgrade is exactly what STEP 2 below must prevent for a
            // beat the count grammar already quieted.
            //
            // spec-count-theater-2026-08-17.md §2 (T109-cl, ruled FINAL) STEP 1: Phase B's
            // blanket suppression — skipping this call entirely whenever pendingQuietCount was
            // set — is RETIRED here, not merely deleted. It existed only so §3's reclaimed calm
            // beats could not accidentally deliver §2 while §2 was still unbuilt and
            // conditional ("do not let §3 acquire a dependency on §2 during the build — that is
            // the one thing that would make the flag expensive"). §2 is ruled A now — the score
            // is always true — so this beat's goal decision is always computed, never thrown
            // away sight unseen. Still side-effect free either way: StageBeatGoal only READS
            // Picked/Opponent/the targets (CompleteGoal is the ledger's ONLY mutator, its own
            // doc says so), so calling it unconditionally changes nothing about WHEN the ledger
            // can move — only whether this method still discards the answer.
            // T164: leg-scoped, reading LegProbs rather than the anchor-only WinProbAfter — see the
            // note at the count branch's StageBeatGoal call above.
            ScoreLedger.StagedGoal? stagedGoal = ledger.StageBeatGoal(evt.Type, up, delta, evt.LegProbs[0]);

            // STEP 2 — THE SCENE STAYS TICKET-KEYED, ONLY THE SCOREBUG MOVES: pendingQuietCount
            // set means THIS beat's own batch was declined a scene by the distance gate above,
            // so Calm/TerritoryFor/TerritoryAgainst plays instead (never a goal template —
            // gateEligible above requires Momentum, so `template` below can never already be
            // GoalFor/GoalAgainst/BreakawayFor/BreakawayAgainst when this is true). That
            // declined scene IS "the corner/calm scene the grammar chose" STEP 2 protects: a
            // goal must not punch through and upgrade it, so it is carried as quietGoal
            // instead, and TvSweatScreen.CommitRevealedGoal (mirroring CommitRevealedCount's
            // shape exactly) commits it on this same beat regardless — no goal-scene payoff
            // will ever fire for a Calm/Territory template to do it otherwise.
            //
            // A beat that never entered the count branch's quiet path at all — a zero batch, or
            // a non-count-leg market, either way pendingQuietCount stays null — is UNCHANGED
            // from before this dispatch: its goal still earns/upgrades its own scene exactly as
            // always. §5: "zero batches already fall through ... already correct" for the count
            // side; STEP 2's own words for the other — "a goal on a goals-market leg keeps
            // today's behaviour exactly."
            //
            // ROUTED, NOT DECIDED HERE: whether a count leg's quiet goal should ALSO earn its
            // own scene, rather than only moving the scorebug, is an open design question the
            // lead has put to the Design Director (§2: "a goal the corners player does not need
            // is exactly the departure from calm his watch is missing" cuts toward yes, but it
            // is not ruled). This builds the conservative reading; flipping it later is routing
            // quietGoal into goal below instead of splitting them here.
            //
            // STEP 3 — ORTHOGONALITY: this split reads only pendingQuietCount, a FACT the count
            // branch already recorded above without ever reading the ledger — never Classify's
            // output or any other significance state directly. The significance gate above, in
            // turn, never reads the ledger or either goal variable below. Neither clause can
            // acquire a dependency on the other through this line.
            ScoreLedger.StagedGoal? goal = pendingQuietCount.HasValue ? null : stagedGoal;
            ScoreLedger.StagedGoal? quietGoal = pendingQuietCount.HasValue ? stagedGoal : null;

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
                market, _pacer.SceneSeconds(template, leadChange), quietCount: pendingQuietCount,
                quietGoal: quietGoal);
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
