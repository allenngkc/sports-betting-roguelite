using System;
using System.Collections.Generic;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// Phase 2B (PRD §7, §9): pure, EditMode-only coverage for <see cref="TheaterScenePlanner"/>,
    /// <see cref="TheaterScenePlan"/>, and <see cref="TheaterSceneHistory"/>. Nothing here touches
    /// <see cref="TheaterStage"/> or <see cref="TheaterChoreographer"/> — this dispatch does not
    /// wire the planner into playback (Phase 2C does). No engine RNG, no <c>UnityEngine.Random</c>,
    /// no wall clock, no frame count anywhere below.
    /// </summary>
    public class TheaterScenePlannerTests
    {
        // ---- spec builders -------------------------------------------------------------------

        private static SceneSpec MakeGoalSpec(SceneTemplate template, bool commits, bool forPicked = true, int variant = 0)
        {
            var goal = new ScoreLedger.StagedGoal(forPicked, commits);
            return new SceneSpec(template, variant, false, false, forPicked, goal, 5f);
        }

        private static SceneSpec MakeNearMissSpec(SceneTemplate template, bool up = true, int variant = 0)
            => new SceneSpec(template, variant, false, false, up, null, 5f);

        private static SceneSpec MakeTerritorySpec(SceneTemplate template, bool forPicked = true, int variant = 0)
            => new SceneSpec(template, variant, false, false, forPicked, null, 5f);

        private static SceneSpec MakeCornerSpec(bool countHelps, bool beneficiaryIsHome, int variant = 0)
        {
            var count = new CountLedger.StagedCount(beneficiaryIsHome ? 1 : 0, beneficiaryIsHome ? 0 : 1, 0);
            SceneTemplate template = countHelps ? SceneTemplate.CornerFor : SceneTemplate.CornerAgainst;
            return new SceneSpec(template, variant, false, false, countHelps, null, count, null,
                MarketKind.TotalCorners, 5f, count.BeneficiaryIsHome);
        }

        private static SceneSpec MakeBookingSpec(bool countHelps, bool beneficiaryIsHome, int variant = 0)
        {
            var count = new CountLedger.StagedCount(beneficiaryIsHome ? 1 : 0, beneficiaryIsHome ? 0 : 1, 0);
            return new SceneSpec(SceneTemplate.Booking, variant, false, false, countHelps, null, count, null,
                MarketKind.TotalCards, 5f, count.BeneficiaryIsHome);
        }

        private static SceneSpec MakeFinalSpec(bool won, int variant = 0)
            => new SceneSpec(won ? SceneTemplate.LegFinalWon : SceneTemplate.LegFinalLost, variant, false, false, won, null, 5f);

        private static SceneSpec MakeKickoffSpec(int variant = 0)
            => new SceneSpec(SceneTemplate.Kickoff, variant, false, false, true, null, 5f);

        private static PresentationSceneKey MakeKey(int eventStep, string seed = "SEED-2B", int round = 1,
            int ticket = 0, int match = 0, SceneTemplate template = SceneTemplate.GoalFor, bool beneficiary = true)
            => new PresentationSceneKey(seed, round, ticket, match, eventStep, template, beneficiary);

        // ---- §7.2 truth table, table-driven across the whole catalog -------------------------

        private static readonly Dictionary<SceneFactContract, HashSet<ScenePayoff>> AllowedPayoffs =
            new Dictionary<SceneFactContract, HashSet<ScenePayoff>>
            {
                { SceneFactContract.Goal, new HashSet<ScenePayoff> { ScenePayoff.Goal } },
                { SceneFactContract.ChalkedGoal, new HashSet<ScenePayoff> { ScenePayoff.Goal } },
                { SceneFactContract.NearMiss, new HashSet<ScenePayoff> {
                    ScenePayoff.Block, ScenePayoff.Interception, ScenePayoff.KeeperSave,
                    ScenePayoff.Clearance, ScenePayoff.Post, ScenePayoff.NearWide } },
                { SceneFactContract.Territory, new HashSet<ScenePayoff> {
                    ScenePayoff.RetainedPossession, ScenePayoff.ForcedBackPass,
                    ScenePayoff.Interception, ScenePayoff.Clearance } },
                { SceneFactContract.Corner, new HashSet<ScenePayoff> { ScenePayoff.CornerDelivery } },
                { SceneFactContract.Booking, new HashSet<ScenePayoff> { ScenePayoff.BookingMarker } },
                { SceneFactContract.Final, new HashSet<ScenePayoff> { ScenePayoff.FinalWhistleWin, ScenePayoff.FinalWhistleLoss } },
                { SceneFactContract.Structural, new HashSet<ScenePayoff> { ScenePayoff.NoStatisticalPayoff } },
            };

        private static readonly Dictionary<SceneFactContract, HashSet<MovementGrammar>> AllowedGrammar =
            new Dictionary<SceneFactContract, HashSet<MovementGrammar>>
            {
                { SceneFactContract.Goal, new HashSet<MovementGrammar> {
                    MovementGrammar.Central, MovementGrammar.Wing, MovementGrammar.Switch,
                    MovementGrammar.Counter, MovementGrammar.SetPiece } },
                { SceneFactContract.ChalkedGoal, new HashSet<MovementGrammar> {
                    MovementGrammar.Central, MovementGrammar.Wing, MovementGrammar.Switch,
                    MovementGrammar.Counter, MovementGrammar.SetPiece } },
                { SceneFactContract.NearMiss, new HashSet<MovementGrammar> {
                    MovementGrammar.Central, MovementGrammar.Wing, MovementGrammar.Switch,
                    MovementGrammar.Counter, MovementGrammar.SetPiece } },
                { SceneFactContract.Territory, new HashSet<MovementGrammar> {
                    MovementGrammar.Central, MovementGrammar.Wing, MovementGrammar.Switch, MovementGrammar.Counter } },
                { SceneFactContract.Corner, new HashSet<MovementGrammar> {
                    MovementGrammar.NearPost, MovementGrammar.FarPost, MovementGrammar.Cleared } },
                { SceneFactContract.Booking, new HashSet<MovementGrammar> {
                    MovementGrammar.Central, MovementGrammar.Wing, MovementGrammar.Switch, MovementGrammar.Counter } },
                { SceneFactContract.Final, new HashSet<MovementGrammar> { MovementGrammar.FinalSequence } },
                { SceneFactContract.Structural, new HashSet<MovementGrammar> {
                    MovementGrammar.Restart, MovementGrammar.Central, MovementGrammar.Wing,
                    MovementGrammar.Switch, MovementGrammar.Counter } },
            };

        [Test]
        public void Every_catalog_candidate_carries_a_payoff_legal_for_its_own_fact_contract()
        {
            foreach (SceneCandidate c in TheaterScenePlanner.Catalog)
                Assert.IsTrue(AllowedPayoffs[c.Contract].Contains(c.Payoff),
                    $"{c.Contract} candidate illegally carries payoff {c.Payoff}");
        }

        [Test]
        public void Every_catalog_candidate_carries_a_grammar_legal_for_its_own_fact_contract()
        {
            foreach (SceneCandidate c in TheaterScenePlanner.Catalog)
                Assert.IsTrue(AllowedGrammar[c.Contract].Contains(c.Grammar),
                    $"{c.Contract} candidate illegally carries grammar {c.Grammar}");
        }

        // ---- forbidden implications (§7.2's table, right-hand column) ------------------------

        [Test]
        public void Near_miss_never_produces_a_goal_payoff()
        {
            foreach (SceneCandidate c in TheaterScenePlanner.Catalog)
                if (c.Contract == SceneFactContract.NearMiss)
                    Assert.AreNotEqual(ScenePayoff.Goal, c.Payoff);
        }

        [Test]
        public void Possession_scenes_never_produce_a_shot_a_count_or_a_booking()
        {
            var forbidden = new HashSet<ScenePayoff> {
                ScenePayoff.Goal, ScenePayoff.Block, ScenePayoff.KeeperSave, ScenePayoff.Post,
                ScenePayoff.NearWide, ScenePayoff.CornerDelivery, ScenePayoff.BookingMarker,
            };
            foreach (SceneCandidate c in TheaterScenePlanner.Catalog)
                if (c.Contract == SceneFactContract.Territory)
                    Assert.IsFalse(forbidden.Contains(c.Payoff), $"Territory candidate illegally carries {c.Payoff}");
        }

        [Test]
        public void Corner_scenes_never_imply_a_booking_and_booking_scenes_never_imply_a_corner()
        {
            foreach (SceneCandidate c in TheaterScenePlanner.Catalog)
            {
                if (c.Contract == SceneFactContract.Corner)
                    Assert.AreNotEqual(ScenePayoff.BookingMarker, c.Payoff);
                if (c.Contract == SceneFactContract.Booking)
                    Assert.AreNotEqual(ScenePayoff.CornerDelivery, c.Payoff);
            }
        }

        [Test]
        public void Chalked_goal_never_stages_a_celebrate_reaction()
        {
            foreach (SceneCandidate c in TheaterScenePlanner.Catalog)
                if (c.Contract == SceneFactContract.ChalkedGoal)
                    Assert.AreNotEqual(ReactionPattern.Celebrate, c.Reaction,
                        "a chalked goal committed nothing to celebrate (§7.2: forbidden implication is a score increment)");
        }

        [Test]
        public void Structural_scenes_never_carry_a_statistical_payoff()
        {
            foreach (SceneCandidate c in TheaterScenePlanner.Catalog)
                if (c.Contract == SceneFactContract.Structural)
                    Assert.AreEqual(ScenePayoff.NoStatisticalPayoff, c.Payoff);
        }

        // ---- fact-contract classification -----------------------------------------------------

        private static readonly Dictionary<SceneTemplate, SceneFactContract> ExpectedContract =
            new Dictionary<SceneTemplate, SceneFactContract>
            {
                { SceneTemplate.GoalFor, SceneFactContract.Goal },
                { SceneTemplate.GoalAgainst, SceneFactContract.Goal },
                { SceneTemplate.BreakawayFor, SceneFactContract.Goal },
                { SceneTemplate.BreakawayAgainst, SceneFactContract.Goal },
                { SceneTemplate.TerritoryFor, SceneFactContract.Territory },
                { SceneTemplate.TerritoryAgainst, SceneFactContract.Territory },
                { SceneTemplate.NearMissHope, SceneFactContract.NearMiss },
                { SceneTemplate.NearMissScare, SceneFactContract.NearMiss },
                { SceneTemplate.CalmPossession, SceneFactContract.Territory },
                { SceneTemplate.LegFinalWon, SceneFactContract.Final },
                { SceneTemplate.LegFinalLost, SceneFactContract.Final },
                { SceneTemplate.Kickoff, SceneFactContract.Structural },
                { SceneTemplate.Fallback, SceneFactContract.Structural },
                { SceneTemplate.CornerFor, SceneFactContract.Corner },
                { SceneTemplate.CornerAgainst, SceneFactContract.Corner },
                { SceneTemplate.Booking, SceneFactContract.Booking },
            };

        [Test]
        public void Every_real_template_classifies_to_its_PRD_7_2_row_and_plans_without_throwing()
        {
            var planner = new TheaterScenePlanner();
            foreach (KeyValuePair<SceneTemplate, SceneFactContract> kv in ExpectedContract)
            {
                var spec = new SceneSpec(kv.Key, 0, false, false, true, null, 5f);
                Assert.AreEqual(kv.Value, TheaterScenePlanner.ClassifyFactContract(spec), kv.Key.ToString());

                PresentationSceneKey key = MakeKey(0, template: kv.Key);
                Assert.DoesNotThrow(() => planner.Plan(spec, key, new TheaterSceneHistory()), kv.Key.ToString());
            }
        }

        [Test]
        public void ClassifyFactContract_defaults_a_goal_template_with_no_staged_goal_to_committing()
        {
            // Mirrors TheaterStage.cs's own "?? true" convention for a bare template-completion
            // spec that never went through TheaterChoreographer.ResolveBeat.
            var spec = new SceneSpec(SceneTemplate.GoalFor, 0, false, false, true, null, 5f);
            Assert.AreEqual(SceneFactContract.Goal, TheaterScenePlanner.ClassifyFactContract(spec));
        }

        [Test]
        public void ClassifyFactContract_reads_chalked_goal_from_a_non_committing_staged_goal()
        {
            SceneSpec spec = MakeGoalSpec(SceneTemplate.GoalFor, commits: false);
            Assert.AreEqual(SceneFactContract.ChalkedGoal, TheaterScenePlanner.ClassifyFactContract(spec));
        }

        // ---- determinism (PRD §4.3, §10: "same key → same ScenePlan") ------------------------

        [Test]
        public void Same_key_and_equal_but_separate_history_instances_produce_an_identical_plan_twice()
        {
            var planner = new TheaterScenePlanner();
            SceneSpec spec = MakeGoalSpec(SceneTemplate.GoalFor, commits: true);
            PresentationSceneKey key = MakeKey(42, template: SceneTemplate.GoalFor);

            var priorSig = new PlanSignature(SceneTemplate.NearMissHope, MovementGrammar.Wing, ChanceShape.Cross,
                ScenePayoff.Block, PressureMode.MidPress, SpacingMode.Balanced, ReactionPattern.Step);

            var historyA = new TheaterSceneHistory();
            historyA.Record(priorSig, SceneFactContract.NearMiss, false);
            var historyB = new TheaterSceneHistory();
            historyB.Record(priorSig, SceneFactContract.NearMiss, false);

            TheaterScenePlan planA = planner.Plan(spec, key, historyA);
            TheaterScenePlan planB = planner.Plan(spec, key, historyB);

            Assert.AreEqual(planA.Signature, planB.Signature);
            Assert.AreEqual(planA.Grammar, planB.Grammar);
            Assert.AreEqual(planA.ChanceShape, planB.ChanceShape);
            Assert.AreEqual(planA.Payoff, planB.Payoff);
            Assert.AreEqual(planA.Pressure, planB.Pressure);
            Assert.AreEqual(planA.Spacing, planB.Spacing);
            Assert.AreEqual(planA.Reaction, planB.Reaction);
            Assert.AreEqual(planA.Lane, planB.Lane);
            Assert.AreEqual(planA.Diagnostics.CollapsedToSingleCandidate, planB.Diagnostics.CollapsedToSingleCandidate);
            Assert.AreEqual(planA.Diagnostics.CollapseReason, planB.Diagnostics.CollapseReason);
        }

        // ---- §7.4 rule 3 ------------------------------------------------------------------------

        [Test]
        public void Immediately_previous_signature_is_rejected_when_an_alternative_exists()
        {
            var planner = new TheaterScenePlanner();
            SceneSpec spec = MakeGoalSpec(SceneTemplate.GoalFor, commits: true);
            PresentationSceneKey key = MakeKey(7, template: SceneTemplate.GoalFor);

            var history = new TheaterSceneHistory();
            TheaterScenePlan first = planner.Plan(spec, key, history);
            Assert.IsFalse(first.Diagnostics.RejectedImmediatePrevious, "nothing to reject on the very first call");

            history.Record(first.Signature, first.FactContract, false);

            // Same key on purpose: absent rule 3, ranking alone would reproduce first's signature
            // exactly a second time.
            TheaterScenePlan second = planner.Plan(spec, key, history);

            Assert.AreNotEqual(first.Signature, second.Signature,
                "the immediately-previous signature must be rejected when a legal alternative exists");
            Assert.IsTrue(second.Diagnostics.RejectedImmediatePrevious);
        }

        // ---- §7.4 rule 4 ------------------------------------------------------------------------

        [Test]
        public void No_grammar_repeats_more_than_twice_in_a_rolling_four_scene_window_when_alternatives_are_legal()
        {
            var planner = new TheaterScenePlanner();
            var history = new TheaterSceneHistory();
            var grammars = new List<MovementGrammar>();

            for (int step = 0; step < 16; step++)
            {
                SceneSpec spec = MakeNearMissSpec(SceneTemplate.NearMissHope, up: true, variant: ScenePlaybook.VariantFor(step));
                PresentationSceneKey key = MakeKey(step, template: SceneTemplate.NearMissHope);
                TheaterScenePlan plan = planner.Plan(spec, key, history);
                history.Record(plan.Signature, plan.FactContract, false);
                grammars.Add(plan.Grammar);
            }

            for (int start = 0; start + 4 <= grammars.Count; start++)
            {
                var window = grammars.GetRange(start, 4);
                var seen = new HashSet<MovementGrammar>(window);
                foreach (MovementGrammar g in seen)
                {
                    int count = 0;
                    foreach (MovementGrammar w in window) if (w == g) count++;
                    Assert.LessOrEqual(count, 2,
                        $"grammar {g} repeated {count} times in the window starting at index {start}: [{string.Join(",", window)}]");
                }
            }
        }

        // ---- §7.4 rule 6 ------------------------------------------------------------------------

        [Test]
        public void Final_and_kickoff_collapse_to_a_single_candidate_and_record_a_diagnostic_reason()
        {
            var planner = new TheaterScenePlanner();

            TheaterScenePlan wonFinal = planner.Plan(MakeFinalSpec(won: true),
                MakeKey(1, template: SceneTemplate.LegFinalWon), new TheaterSceneHistory());
            Assert.IsTrue(wonFinal.Diagnostics.CollapsedToSingleCandidate);
            Assert.AreEqual(1, wonFinal.Diagnostics.ValidCandidateCount);
            Assert.IsFalse(string.IsNullOrEmpty(wonFinal.Diagnostics.CollapseReason));

            TheaterScenePlan lostFinal = planner.Plan(MakeFinalSpec(won: false),
                MakeKey(2, template: SceneTemplate.LegFinalLost), new TheaterSceneHistory());
            Assert.IsTrue(lostFinal.Diagnostics.CollapsedToSingleCandidate);
            Assert.AreEqual(1, lostFinal.Diagnostics.ValidCandidateCount);
            Assert.IsFalse(string.IsNullOrEmpty(lostFinal.Diagnostics.CollapseReason));

            TheaterScenePlan kickoff = planner.Plan(MakeKickoffSpec(),
                MakeKey(3, template: SceneTemplate.Kickoff), new TheaterSceneHistory());
            Assert.IsTrue(kickoff.Diagnostics.CollapsedToSingleCandidate);
            Assert.AreEqual(1, kickoff.Diagnostics.ValidCandidateCount);
            Assert.IsFalse(string.IsNullOrEmpty(kickoff.Diagnostics.CollapseReason));
        }

        [Test]
        public void A_richly_populated_contract_does_not_falsely_report_a_collapse()
        {
            var planner = new TheaterScenePlanner();
            TheaterScenePlan plan = planner.Plan(MakeGoalSpec(SceneTemplate.GoalFor, true),
                MakeKey(9, template: SceneTemplate.GoalFor), new TheaterSceneHistory());

            Assert.IsFalse(plan.Diagnostics.CollapsedToSingleCandidate);
            Assert.IsNull(plan.Diagnostics.CollapseReason);
            Assert.Greater(plan.Diagnostics.ValidCandidateCount, 1);
        }

        // ---- §7.3 variety floor: assert catalog counts, do not trust them --------------------

        [Test]
        public void Catalog_meets_the_variety_floor_for_movement_and_chance_shape()
        {
            var goalGrammars = new HashSet<MovementGrammar>();
            var goalChanceShapes = new HashSet<ChanceShape>();
            bool hasSetPieceDirectPairing = false;

            foreach (SceneCandidate c in TheaterScenePlanner.Catalog)
            {
                if (c.Contract != SceneFactContract.Goal) continue;
                goalGrammars.Add(c.Grammar);
                if (c.ChanceShape.HasValue) goalChanceShapes.Add(c.ChanceShape.Value);
                if (c.Grammar == MovementGrammar.SetPiece && c.ChanceShape == ChanceShape.Direct)
                    hasSetPieceDirectPairing = true;
            }

            foreach (MovementGrammar g in new[] { MovementGrammar.Central, MovementGrammar.Wing, MovementGrammar.Switch, MovementGrammar.Counter })
                Assert.IsTrue(goalGrammars.Contains(g), $"required buildup grammar {g} missing");

            foreach (ChanceShape cs in new[] { ChanceShape.ThroughBall, ChanceShape.Cross, ChanceShape.Cutback, ChanceShape.Rebound, ChanceShape.Direct })
                Assert.IsTrue(goalChanceShapes.Contains(cs), $"required chance shape {cs} missing");

            Assert.IsTrue(hasSetPieceDirectPairing,
                "§7.3's 5th 'set piece' scoring shape requires a SetPiece-movement/Direct-chance-shape candidate to exist");
        }

        [Test]
        public void Catalog_meets_the_variety_floor_for_non_goal_endings_and_corner_shapes()
        {
            var nonGoalEndings = new HashSet<ScenePayoff>();
            foreach (SceneCandidate c in TheaterScenePlanner.Catalog)
                if (c.Contract == SceneFactContract.NearMiss) nonGoalEndings.Add(c.Payoff);

            foreach (ScenePayoff p in new[] {
                ScenePayoff.Block, ScenePayoff.Interception, ScenePayoff.KeeperSave,
                ScenePayoff.Clearance, ScenePayoff.Post, ScenePayoff.NearWide })
                Assert.IsTrue(nonGoalEndings.Contains(p), $"required non-goal ending {p} missing");

            var cornerShapes = new HashSet<MovementGrammar>();
            foreach (SceneCandidate c in TheaterScenePlanner.Catalog)
            {
                if (c.Contract != SceneFactContract.Corner) continue;
                cornerShapes.Add(c.Grammar);
                Assert.IsTrue(c.HasWinTheCornerLeadIn, "every Corner candidate must carry the win-the-corner lead-in");
            }
            foreach (MovementGrammar shape in new[] { MovementGrammar.NearPost, MovementGrammar.FarPost, MovementGrammar.Cleared })
                Assert.IsTrue(cornerShapes.Contains(shape), $"required corner shape {shape} missing");
        }

        [Test]
        public void Catalog_meets_the_variety_floor_for_pressure_and_spacing_modes()
        {
            var pressures = new HashSet<PressureMode>();
            var spacings = new HashSet<SpacingMode>();
            foreach (SceneCandidate c in TheaterScenePlanner.Catalog)
            {
                pressures.Add(c.Pressure);
                spacings.Add(c.Spacing);
            }
            Assert.AreEqual(3, pressures.Count);
            Assert.AreEqual(3, spacings.Count);
        }

        [Test]
        public void Each_pressure_mode_owns_an_exclusive_defending_reaction_no_other_mode_reaches()
        {
            var byPressure = new Dictionary<PressureMode, HashSet<ReactionPattern>>
            {
                { PressureMode.LowBlock, new HashSet<ReactionPattern>() },
                { PressureMode.MidPress, new HashSet<ReactionPattern>() },
                { PressureMode.HighPress, new HashSet<ReactionPattern>() },
            };
            foreach (SceneCandidate c in TheaterScenePlanner.Catalog)
                byPressure[c.Pressure].Add(c.Reaction);

            Assert.IsTrue(byPressure[PressureMode.LowBlock].Contains(ReactionPattern.Drop));
            Assert.IsTrue(byPressure[PressureMode.MidPress].Contains(ReactionPattern.Step));
            Assert.IsTrue(byPressure[PressureMode.HighPress].Contains(ReactionPattern.Chase));

            Assert.IsFalse(byPressure[PressureMode.MidPress].Contains(ReactionPattern.Drop));
            Assert.IsFalse(byPressure[PressureMode.HighPress].Contains(ReactionPattern.Drop));
            Assert.IsFalse(byPressure[PressureMode.LowBlock].Contains(ReactionPattern.Step));
            Assert.IsFalse(byPressure[PressureMode.HighPress].Contains(ReactionPattern.Step));
            Assert.IsFalse(byPressure[PressureMode.LowBlock].Contains(ReactionPattern.Chase));
            Assert.IsFalse(byPressure[PressureMode.MidPress].Contains(ReactionPattern.Chase));
        }

        // ---- §7.6 attribution: read from the fact, never chosen by the planner ---------------

        [Test]
        public void Corner_attribution_passes_through_from_the_spec_and_never_influences_which_candidate_is_chosen()
        {
            var planner = new TheaterScenePlanner();
            PresentationSceneKey key = MakeKey(5, template: SceneTemplate.CornerFor);

            TheaterScenePlan planHome = planner.Plan(MakeCornerSpec(true, beneficiaryIsHome: true), key, new TheaterSceneHistory());
            TheaterScenePlan planAway = planner.Plan(MakeCornerSpec(true, beneficiaryIsHome: false), key, new TheaterSceneHistory());

            Assert.AreEqual(true, planHome.BeneficiaryIsHome);
            Assert.AreEqual(false, planAway.BeneficiaryIsHome);
            Assert.IsTrue(planHome.HasWinTheCornerLeadIn);
            Assert.IsTrue(planAway.HasWinTheCornerLeadIn);

            // Attribution must never be what decides movement/payoff/pressure/spacing/reaction.
            Assert.AreEqual(planHome.Grammar, planAway.Grammar);
            Assert.AreEqual(planHome.Payoff, planAway.Payoff);
            Assert.AreEqual(planHome.Pressure, planAway.Pressure);
            Assert.AreEqual(planHome.Spacing, planAway.Spacing);
            Assert.AreEqual(planHome.Reaction, planAway.Reaction);
        }

        [Test]
        public void Booking_setups_exist_for_both_fouling_sides_and_attribution_is_never_chosen_by_the_planner()
        {
            var planner = new TheaterScenePlanner();
            PresentationSceneKey key = MakeKey(6, template: SceneTemplate.Booking);

            TheaterScenePlan planHome = planner.Plan(MakeBookingSpec(true, beneficiaryIsHome: true), key, new TheaterSceneHistory());
            TheaterScenePlan planAway = planner.Plan(MakeBookingSpec(true, beneficiaryIsHome: false), key, new TheaterSceneHistory());

            Assert.AreEqual(true, planHome.BeneficiaryIsHome);
            Assert.AreEqual(false, planAway.BeneficiaryIsHome);
            Assert.AreEqual(planHome.Grammar, planAway.Grammar);
            Assert.AreEqual(planHome.Reaction, planAway.Reaction);
        }

        // ---- history ring buffer ----------------------------------------------------------------

        [Test]
        public void History_evicts_the_oldest_entry_past_capacity()
        {
            var history = new TheaterSceneHistory(capacity: 2);
            var sigA = new PlanSignature(SceneTemplate.NearMissHope, MovementGrammar.Central, ChanceShape.Cross, ScenePayoff.Block, PressureMode.LowBlock, SpacingMode.Compact, ReactionPattern.Drop);
            var sigB = new PlanSignature(SceneTemplate.NearMissHope, MovementGrammar.Wing, ChanceShape.Cross, ScenePayoff.Block, PressureMode.LowBlock, SpacingMode.Compact, ReactionPattern.Drop);
            var sigC = new PlanSignature(SceneTemplate.NearMissHope, MovementGrammar.Switch, ChanceShape.Cross, ScenePayoff.Block, PressureMode.LowBlock, SpacingMode.Compact, ReactionPattern.Drop);

            history.Record(sigA, SceneFactContract.NearMiss, false);
            history.Record(sigB, SceneFactContract.NearMiss, false);
            history.Record(sigC, SceneFactContract.NearMiss, false);

            Assert.AreEqual(2, history.Count);
            Assert.AreEqual(sigC, history.MostRecent.Value.Signature);

            HashSet<MovementGrammar> recent = history.RecentNonStructuralGrammars(3);
            Assert.IsFalse(recent.Contains(MovementGrammar.Central), "evicted entry must not still influence the window");
            Assert.IsTrue(recent.Contains(MovementGrammar.Wing));
            Assert.IsTrue(recent.Contains(MovementGrammar.Switch));
        }

        [Test]
        public void Structural_entries_are_real_for_MostRecent_but_invisible_to_the_non_structural_grammar_window()
        {
            var history = new TheaterSceneHistory();
            var goalSig = new PlanSignature(SceneTemplate.GoalFor, MovementGrammar.Wing, ChanceShape.Cross,
                ScenePayoff.Goal, PressureMode.MidPress, SpacingMode.Balanced, ReactionPattern.Celebrate);
            var kickoffSig = new PlanSignature(SceneTemplate.Kickoff, MovementGrammar.Restart, null,
                ScenePayoff.NoStatisticalPayoff, PressureMode.LowBlock, SpacingMode.Balanced, ReactionPattern.Recover);

            history.Record(goalSig, SceneFactContract.Goal, false);
            history.Record(kickoffSig, SceneFactContract.Structural, true);

            // Rule 3 ("immediately previous") sees the kickoff — it is real, ordinary history.
            Assert.AreEqual(kickoffSig, history.MostRecent.Value.Signature);

            // Rule 4's non-structural window skips straight past it to the goal's grammar.
            HashSet<MovementGrammar> recent = history.RecentNonStructuralGrammars(3);
            Assert.IsTrue(recent.Contains(MovementGrammar.Wing));
            Assert.IsFalse(recent.Contains(MovementGrammar.Restart));
        }
    }
}
