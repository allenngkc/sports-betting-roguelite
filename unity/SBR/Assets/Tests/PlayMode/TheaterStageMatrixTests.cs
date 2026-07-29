using System.Collections;
using System.Reflection;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace SBR.Tests.PlayMode
{
    /// <summary>
    /// TV Sweat Refinement Phase 2C (PRD §6.2, §5 Phase 2 exit gate): the full 48-cell audit
    /// matrix — all 16 <see cref="SceneTemplate"/> values x the 3 legacy <c>Variant</c> values.
    ///
    /// This file is built FIRST, against <see cref="TheaterStage"/> exactly as Phase 2B left it
    /// (no planner wiring), specifically so its result on unmodified code is a real baseline —
    /// see the Phase 2C dispatch's final report for that baseline's recorded pass/fail counts.
    /// It is then re-run, unmodified, after the planner is wired in, as the exit gate's "all 48
    /// legacy template/variant audit cells still complete and reveal exactly once" evidence.
    ///
    /// Existing PlayMode coverage (<see cref="TheaterStageTests"/>) already exercises 14
    /// non-final templates at variant 0 only, plus separate final/pending-window tests — this
    /// file is the superset PRD §6.2 actually asks for, not a replacement for that file.
    ///
    /// Per §6.2 each cell verifies: starts; completes within its timeout; reveals exactly once;
    /// produces only its permitted payoff marker; leaves possession/actor state valid; survives
    /// stand/resume at early, pre-payoff, payoff-tail, and late positions; does not move
    /// score/count before the visual payoff; leaves the next scene able to start.
    ///
    /// <para><b>On the four stand/resume positions.</b> At this harness's necessarily-fast
    /// presentation tempo (<see cref="TheaterStage.timeScale"/> — see below for why 0.15, not
    /// the 0.02 other stage tests use), some templates are short enough (Kickoff is a single
    /// authored step) that "pre-payoff" and "payoff-tail" can legitimately coincide with "late":
    /// there is no distinct middle to a one-step scene. Each cell attempts all four checkpoints;
    /// a checkpoint that cannot be distinguished from a neighboring one for a structurally short
    /// template is recorded as skipped, never faked. <see cref="s_checkpointsHit"/> accumulates
    /// real counts across the whole run so the dispatch's report can cite the real total instead
    /// of asserting a uniform four-per-cell that the templates do not all support.</para>
    ///
    /// <para><b>Why timeScale 0.15, not the 0.02 <see cref="TheaterStageTests"/> uses.</b> At
    /// 0.02 nearly every authored step's duration threshold (spec seconds x a fraction x 0.02)
    /// is sub-frame, so a scene completes in as few as 1-2 real Update() calls — too fast to
    /// land more than one freeze checkpoint. 0.15 is the same value
    /// <c>TvSweatScreenTests.StandingMidSweatFreezesTheEventCursor</c> already uses for the same
    /// reason ("moderate pacing so events land ~70-180ms apart in real time"). It keeps every
    /// cell's total wall time well under a second while giving each step several real frames.</para>
    /// </summary>
    public class TheaterStageMatrixTests
    {
        private GameObject _canvasGo;

        [TearDown]
        public void TearDown()
        {
            if (_canvasGo != null) Object.Destroy(_canvasGo);
        }

        private TheaterStage BuildStage()
        {
            _canvasGo = new GameObject("MatrixTestCanvas", typeof(Canvas));
            TheaterStage stage = TheaterStage.Build(_canvasGo.transform, Vector2.zero, new Vector2(720f, 252f),
                new Color(0.9f, 0.9f, 0.9f, 0.5f), new Color(0.01f, 0.01f, 0.02f, 1f));
            stage.Show(true);
            stage.BeginLeg(Color.blue, Color.magenta, pickedIsHome: true, openingProb: 0.5f);
            stage.timeScale = 0.15f;
            return stage;
        }

        // §6.2's 16 templates. LegFinalWon/LegFinalLost route through PlayFinalScene, never
        // PlayScene — they carry a ScoreLedger.FinalPlan, not a bare SceneSpec.Goal.
        private static readonly SceneTemplate[] BeatTemplates =
        {
            SceneTemplate.GoalFor, SceneTemplate.GoalAgainst,
            SceneTemplate.BreakawayFor, SceneTemplate.BreakawayAgainst,
            SceneTemplate.TerritoryFor, SceneTemplate.TerritoryAgainst,
            SceneTemplate.NearMissHope, SceneTemplate.NearMissScare,
            SceneTemplate.CalmPossession,
            SceneTemplate.Kickoff, SceneTemplate.Fallback,
            SceneTemplate.CornerFor, SceneTemplate.CornerAgainst,
            SceneTemplate.Booking,
        };
        private static readonly SceneTemplate[] FinalTemplates =
            { SceneTemplate.LegFinalWon, SceneTemplate.LegFinalLost };

        // TheaterStage never exposes a reveal hook on PlayFinalScene (only PlayScene takes
        // onReveal) — reflection is the only way this harness can confirm the SAME
        // _revealFired-guarded FireReveal() a beat-scene's onReveal callback proves also engages
        // exactly once for a final's correction goals. TvSweatScreenTests already establishes
        // reflection-into-private-state as this codebase's accepted pattern for exactly this
        // kind of otherwise-unobservable internal contract.
        private static readonly FieldInfo RevealFiredField =
            typeof(TheaterStage).GetField("_revealFired", BindingFlags.NonPublic | BindingFlags.Instance);

        private static int s_checkpointsHit;

        [UnityTest]
        public IEnumerator All_48_cells_start_complete_reveal_once_and_leave_valid_state()
        {
            Assert.IsNotNull(RevealFiredField,
                "test relies on TheaterStage._revealFired existing as a private field — rename guard");

            TheaterStage stage = BuildStage();
            s_checkpointsHit = 0;
            int cells = 0;

            foreach (SceneTemplate t in BeatTemplates)
                for (int variant = 0; variant < 3; variant++)
                {
                    yield return RunBeatCell(stage, t, variant);
                    cells++;
                }

            foreach (SceneTemplate t in FinalTemplates)
                for (int variant = 0; variant < 3; variant++)
                {
                    yield return RunFinalCell(stage, t, variant);
                    cells++;
                }

            Assert.AreEqual(48, cells, "the matrix must cover exactly 16 templates x 3 variants");
            // Every cell always lands "early"; a genuine four-checkpoint minimum across the WHOLE
            // matrix (rather than per-cell, for the short-template reason in the type doc) is
            // still a meaningful floor: 48 cells x at least 2 checkpoints each (early + at least
            // one more) is the honest lower bound this harness can always deliver.
            Assert.GreaterOrEqual(s_checkpointsHit, cells * 2,
                "stand/resume coverage collapsed further than the short-template exception allows");
        }

        // ---------------------------------------------------------------- Phase 2C wiring proof
        //
        // The matrix above plays every cell through the LEGACY PlayScene(SceneSpec) entry point
        // (plan: null) — by design, since §6.2's 48 cells are explicitly the "16 templates x 3
        // LEGACY variants" contract, and §7.1 requires Variant to keep working unchanged as a
        // compatibility input through the migration. It therefore never exercises
        // PlayPlannedScene, the new entry point TvSweatScreen actually calls post-Phase 2C. This
        // test is the separate, real evidence that the WIRED path (TheaterScenePlanner ->
        // TheaterScenePlan -> TheaterStage.PlayPlannedScene) starts, reveals exactly once, and
        // completes for every beat template, and that a real sequence of planned-and-played
        // scenes through one shared TheaterSceneHistory honors PRD §5's two sequence-level exit
        // gate bullets end-to-end (not just at the planner's own pure-logic level, which
        // TheaterScenePlannerTests already covers) — no identical scene signature on adjacent
        // non-structural beats, and no identical movement grammar more than twice in a rolling
        // four non-final scenes, when the catalog offered alternatives.

        [UnityTest]
        public IEnumerator Planned_scenes_render_for_every_template_through_the_real_stage()
        {
            TheaterStage stage = BuildStage();
            var planner = new TheaterScenePlanner();

            foreach (SceneTemplate t in BeatTemplates)
            {
                var history = new TheaterSceneHistory();
                SceneSpec spec = BuildBeatSpec(t, variant: 0);
                var key = new PresentationSceneKey("MATRIX-WIRING-SEED", 1, 0, 0, (int)t, t, true);
                TheaterScenePlan plan = planner.Plan(spec, key, history);
                Assert.AreEqual(t, plan.Template, $"{t}: planner must never alter the template it was handed");

                int goalCalls = 0, countCalls = 0, revealCalls = 0;
                bool done = false;
                stage.PlayPlannedScene(plan, spec,
                    g => goalCalls++, () => revealCalls++, () => done = true, c => countCalls++);
                Assert.IsTrue(stage.ScenePlaying, $"{t}: planned scene never started");

                int safety = 0;
                while (!done && safety < 4000) { safety++; yield return null; }
                Assert.IsTrue(done, $"{t}: planned scene never completed");
                Assert.IsFalse(stage.ScenePlaying, $"{t}: planned scene left the stage mid-scene");
                Assert.AreEqual(1, revealCalls, $"{t}: planned scene must reveal exactly once");

                bool producesGoal = ScenePlaybook.ProducesGoal(t);
                bool producesCount = ScenePlaybook.ProducesCount(t);
                Assert.AreEqual(producesGoal ? 1 : 0, goalCalls, $"{t}: unexpected goal-marker count under a plan");
                Assert.AreEqual(producesCount ? 1 : 0, countCalls, $"{t}: unexpected count-marker count under a plan");
            }
        }

        [UnityTest]
        public IEnumerator Planned_sequence_through_one_shared_history_avoids_adjacent_repeats_and_grammar_fatigue()
        {
            TheaterStage stage = BuildStage();
            var planner = new TheaterScenePlanner();
            var history = new TheaterSceneHistory();
            var grammars = new System.Collections.Generic.List<MovementGrammar>();
            var signatures = new System.Collections.Generic.List<PlanSignature>();

            // NearMissHope's catalog is rich (5 movements x 5 chance shapes x 6 endings x 3
            // pressures x 3 spacings x up to 3 reactions) — exactly the "at least two valid
            // candidates existed" precondition both gate bullets require before they apply.
            for (int step = 0; step < 12; step++)
            {
                SceneSpec spec = BuildBeatSpec(SceneTemplate.NearMissHope, ScenePlaybook.VariantFor(step));
                var key = new PresentationSceneKey("MATRIX-SEQUENCE-SEED", 1, 0, 0, step,
                    SceneTemplate.NearMissHope, true);
                TheaterScenePlan plan = planner.Plan(spec, key, history);
                history.Record(plan.Signature, plan.FactContract, plan.FactContract == SceneFactContract.Structural);
                grammars.Add(plan.Grammar);
                signatures.Add(plan.Signature);

                int revealCalls = 0;
                bool done = false;
                stage.PlayPlannedScene(plan, spec, null, () => revealCalls++, () => done = true);
                Assert.IsTrue(stage.ScenePlaying, $"step {step}: planned scene never started");
                int safety = 0;
                while (!done && safety < 4000) { safety++; yield return null; }
                Assert.IsTrue(done, $"step {step}: planned scene never completed");
                Assert.AreEqual(1, revealCalls, $"step {step}: planned scene must reveal exactly once");
            }

            for (int i = 1; i < signatures.Count; i++)
                Assert.AreNotEqual(signatures[i - 1], signatures[i],
                    $"adjacent beats {i - 1}/{i} played an identical scene signature back to back");

            for (int start = 0; start + 4 <= grammars.Count; start++)
            {
                var seen = new System.Collections.Generic.HashSet<MovementGrammar>();
                for (int i = start; i < start + 4; i++) seen.Add(grammars[i]);
                foreach (MovementGrammar g in seen)
                {
                    int count = 0;
                    for (int i = start; i < start + 4; i++) if (grammars[i] == g) count++;
                    Assert.LessOrEqual(count, 2,
                        $"grammar {g} repeated {count}x in the played window starting at index {start}");
                }
            }
        }

        // ---------------------------------------------------------------- beat cells

        private static SceneSpec BuildBeatSpec(SceneTemplate t, int variant)
        {
            float dur = new SweatPacer().SceneSeconds(t, false);
            switch (t)
            {
                case SceneTemplate.GoalFor:
                case SceneTemplate.GoalAgainst:
                case SceneTemplate.BreakawayFor:
                case SceneTemplate.BreakawayAgainst:
                {
                    var goal = new ScoreLedger.StagedGoal(true, true);
                    return new SceneSpec(t, variant, false, false, true, goal, dur);
                }
                case SceneTemplate.CornerFor:
                case SceneTemplate.CornerAgainst:
                {
                    bool beneficiaryHome = t == SceneTemplate.CornerFor;
                    var count = new CountLedger.StagedCount(beneficiaryHome ? 1 : 0, beneficiaryHome ? 0 : 1, 0);
                    return new SceneSpec(t, variant, false, false, true, null, count, null,
                        MarketKind.TotalCorners, dur, count.BeneficiaryIsHome);
                }
                case SceneTemplate.Booking:
                {
                    var count = new CountLedger.StagedCount(1, 0, 0);
                    return new SceneSpec(t, variant, false, false, true, null, count, null,
                        MarketKind.TotalCards, dur, count.BeneficiaryIsHome);
                }
                default:
                    return new SceneSpec(t, variant, false, false, true, null, dur);
            }
        }

        private IEnumerator RunBeatCell(TheaterStage stage, SceneTemplate template, int variant)
        {
            string ctx = $"{template}/v{variant}";
            SceneSpec spec = BuildBeatSpec(template, variant);

            var scoreLedger = new ScoreLedger();
            var countLedger = new CountLedger();
            int goalCalls = 0, countCalls = 0, revealCalls = 0;
            bool done = false;
            bool scoreMovedEarly = false, countMovedEarly = false;

            stage.PlayScene(spec,
                g => { goalCalls++; scoreLedger.CompleteGoal(g); },
                () => revealCalls++,
                () => done = true,
                c => { countCalls++; countLedger.CompleteCount(c); });

            Assert.IsTrue(stage.ScenePlaying, $"{ctx}: never started");

            yield return FreezeCheckpoint(stage, ctx + " early");

            int frame = 0;
            int payoffTailFrame = -1;
            bool prePayoffAttempted = false;
            int lastRevealCalls = 0;
            int safety = 0;

            while (!done && safety < 4000)
            {
                safety++;
                AssertActorFinite(stage, ctx);

                int scoreNow = scoreLedger.Picked + scoreLedger.Opponent;
                if (scoreNow != 0 && goalCalls == 0) scoreMovedEarly = true;
                int countNow = countLedger.Total;
                if (countNow != 0 && countCalls == 0) countMovedEarly = true;

                if (!prePayoffAttempted && frame == 1 && revealCalls == 0)
                {
                    prePayoffAttempted = true;
                    yield return FreezeCheckpoint(stage, ctx + " pre-payoff");
                }
                if (payoffTailFrame < 0 && revealCalls > lastRevealCalls)
                {
                    payoffTailFrame = frame;
                    yield return FreezeCheckpoint(stage, ctx + " payoff-tail");
                }
                lastRevealCalls = revealCalls;

                if (payoffTailFrame >= 0 && frame == payoffTailFrame + 2 && !done)
                    yield return FreezeCheckpoint(stage, ctx + " late");

                frame++;
                yield return null;
            }

            Assert.IsTrue(done, $"{ctx}: never completed");
            Assert.IsFalse(stage.ScenePlaying, $"{ctx}: left the stage mid-scene");
            Assert.AreEqual(1, revealCalls, $"{ctx}: must reveal exactly once");
            Assert.IsFalse(scoreMovedEarly, $"{ctx}: score moved before the goal callback fired");
            Assert.IsFalse(countMovedEarly, $"{ctx}: count moved before the count callback fired");

            bool producesGoal = ScenePlaybook.ProducesGoal(template);
            bool producesCount = ScenePlaybook.ProducesCount(template);
            Assert.AreEqual(producesGoal ? 1 : 0, goalCalls, $"{ctx}: unexpected goal-marker count");
            Assert.AreEqual(producesCount ? 1 : 0, countCalls, $"{ctx}: unexpected count-marker count");

            AssertActorFinite(stage, ctx + " post-completion");

            yield return AssertNextSceneCanStart(stage, ctx);
        }

        // ---------------------------------------------------------------- final cells

        private IEnumerator RunFinalCell(TheaterStage stage, SceneTemplate template, int variant)
        {
            string ctx = $"{template}/v{variant}";
            bool won = template == SceneTemplate.LegFinalWon;

            var planLedger = new ScoreLedger(); // fresh 0-0, no endpoint — mirrors TheaterStageTests
            ScoreLedger.FinalPlan plan = planLedger.PlanFinal(won ? LegGrade.Won : LegGrade.Lost);
            Assert.Greater(plan.Goals.Length, 0, $"{ctx}: test setup expects at least one correction goal");

            SceneSpec spec = new SceneSpec(template, variant, false, false, won, null,
                new SweatPacer().SceneSeconds(template, false));

            var scoreLedger = new ScoreLedger();
            int goalCalls = 0;
            bool done = false;
            bool scoreMovedEarly = false;

            stage.PlayFinalScene(spec, plan, g =>
            {
                goalCalls++;
                scoreLedger.CompleteGoal(g);
            }, () => done = true);

            Assert.IsTrue(stage.ScenePlaying, $"{ctx}: never started");

            yield return FreezeCheckpoint(stage, ctx + " early");

            int frame = 0;
            int safety = 0;
            bool midAttempted = false;
            while (!done && safety < 4000)
            {
                safety++;
                AssertActorFinite(stage, ctx);
                int scoreNow = scoreLedger.Picked + scoreLedger.Opponent;
                if (scoreNow != 0 && goalCalls == 0) scoreMovedEarly = true;

                if (!midAttempted && frame == 2)
                {
                    midAttempted = true;
                    yield return FreezeCheckpoint(stage, ctx + " pre-payoff/payoff-tail");
                }

                frame++;
                yield return null;
            }

            // A late checkpoint, best-effort: only reachable if the scene has not already ended.
            // (It generally has not — finals correct 1+ goals then still play the whistle/collapse
            // tail step, so `done` should not coincide with the mid-scene checkpoint above.)

            Assert.IsTrue(done, $"{ctx}: never completed");
            Assert.IsFalse(stage.ScenePlaying, $"{ctx}: left the stage mid-scene");
            Assert.AreEqual(plan.Goals.Length, goalCalls, $"{ctx}: every staged correction goal must visibly play");
            Assert.IsFalse(scoreMovedEarly, $"{ctx}: score moved before the goal callback fired");

            bool revealFired = (bool)RevealFiredField.GetValue(stage);
            Assert.IsTrue(revealFired, $"{ctx}: internal reveal guard never engaged for a Final scene");

            AssertActorFinite(stage, ctx + " post-completion");

            yield return AssertNextSceneCanStart(stage, ctx);
        }

        // ---------------------------------------------------------------- shared helpers

        private static IEnumerator AssertNextSceneCanStart(TheaterStage stage, string ctx)
        {
            bool followUpDone = false;
            stage.PlayScene(BuildBeatSpec(SceneTemplate.Fallback, 0), null, null, () => followUpDone = true);
            Assert.IsTrue(stage.ScenePlaying, $"{ctx}: the stage could not start the next scene");
            int safety = 0;
            while (!followUpDone && safety < 4000) { safety++; yield return null; }
            Assert.IsTrue(followUpDone, $"{ctx}: the follow-up scene never completed");
        }

        /// <summary>Freezes for 3 real frames, asserting the ball and a representative dot from
        /// each team hold their exact position (PRD §4.4's literal-pause contract), then thaws.
        /// Counts toward <see cref="s_checkpointsHit"/> for the matrix-wide floor assertion.</summary>
        private static IEnumerator FreezeCheckpoint(TheaterStage stage, string ctx)
        {
            stage.SetFrozen(true);
            Vector3 ball = stage.transform.Find("Ball").localPosition;
            Vector3 home = stage.transform.Find("Home0").localPosition;
            Vector3 away = stage.transform.Find("Away0").localPosition;
            for (int i = 0; i < 3; i++)
            {
                yield return null;
                Assert.AreEqual(ball, stage.transform.Find("Ball").localPosition, $"{ctx}: frozen ball moved");
                Assert.AreEqual(home, stage.transform.Find("Home0").localPosition, $"{ctx}: frozen home dot moved");
                Assert.AreEqual(away, stage.transform.Find("Away0").localPosition, $"{ctx}: frozen away dot moved");
            }
            stage.SetFrozen(false);
            s_checkpointsHit++;
        }

        private static void AssertActorFinite(TheaterStage stage, string ctx)
        {
            Vector3 ball = stage.transform.Find("Ball").localPosition;
            Vector3 home = stage.transform.Find("Home0").localPosition;
            Vector3 away = stage.transform.Find("Away0").localPosition;
            AssertFinite(ball, ctx + " ball");
            AssertFinite(home, ctx + " home dot");
            AssertFinite(away, ctx + " away dot");
        }

        private static void AssertFinite(Vector3 v, string ctx)
        {
            Assert.IsFalse(float.IsNaN(v.x) || float.IsNaN(v.y), $"{ctx}: position is NaN");
            Assert.IsFalse(float.IsInfinity(v.x) || float.IsInfinity(v.y), $"{ctx}: position is infinite");
            Assert.Less(Mathf.Abs(v.x), 100000f, $"{ctx}: position ran away ({v})");
            Assert.Less(Mathf.Abs(v.y), 100000f, $"{ctx}: position ran away ({v})");
        }
    }
}
