using System.Collections;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace SBR.Tests.PlayMode
{
    /// <summary>
    /// The living stage's behavioral laws (F_0.2.0 M-T2): territory speaks the honest
    /// probability, a beat pulse kicks it toward the beneficiary and decays back, and
    /// freezing holds the exact frame (the stand-up pause / pending-window contract).
    /// Built bare on a scratch canvas — no Room scene needed.
    /// </summary>
    public class TheaterStageTests
    {
        private GameObject _canvasGo;
        private TheaterStage _stage;

        private TheaterStage BuildStage()
        {
            _canvasGo = new GameObject("TestCanvas", typeof(Canvas));
            _stage = TheaterStage.Build(_canvasGo.transform, Vector2.zero, new Vector2(720f, 252f),
                new Color(0.9f, 0.9f, 0.9f, 0.5f), new Color(0.01f, 0.01f, 0.02f, 1f));
            _stage.Show(true);
            _stage.BeginLeg(Color.blue, Color.magenta, pickedIsHome: true, openingProb: 0.5f);
            return _stage;
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvasGo != null) Object.Destroy(_canvasGo);
        }

        [UnityTest]
        public IEnumerator Territory_restates_the_live_probability()
        {
            TheaterStage stage = BuildStage();
            yield return null;
            Assert.AreEqual(PitchLayout.TerritoryX(0.5f), stage.LastTerritoryX, 0.02f,
                "even money must read midfield");

            stage.SetLiveProb(0.9f);
            yield return null;
            Assert.AreEqual(PitchLayout.TerritoryX(0.9f), stage.LastTerritoryX, 0.02f,
                "territory must follow the honest prob, and only the honest prob");
        }

        [UnityTest]
        public IEnumerator Pulse_kicks_territory_toward_the_beneficiary_then_decays()
        {
            TheaterStage stage = BuildStage();
            yield return null;
            float honest = PitchLayout.TerritoryX(0.5f);

            stage.Pulse(up: false, TensionTag.Swing);
            yield return null;
            Assert.Less(stage.LastTerritoryX, honest - 0.05f,
                "a down beat must shove territory toward the picked side's own goal");

            // The impulse decays back to the honest point — filler never keeps signifying.
            float t = 0f;
            while (t < 4f && Mathf.Abs(stage.LastTerritoryX - honest) > 0.02f)
            {
                t += Time.deltaTime;
                yield return null;
            }
            Assert.AreEqual(honest, stage.LastTerritoryX, 0.02f, "the pulse must decay to honesty");
        }

        // ------------------------------------------------------------ M-T3 scene playback

        private static SceneSpec Spec(SceneTemplate t, ScoreLedger.StagedGoal? goal = null)
            => new SceneSpec(t, 0, false, false, goal, new SweatPacer().SceneSeconds(t, false));

        [UnityTest]
        public IEnumerator One_scene_per_template_plays_to_completion()
        {
            TheaterStage stage = BuildStage();
            stage.timeScale = 0.02f; // duration multiplier — fast-forward story time

            var templates = new[]
            {
                SceneTemplate.GoalFor, SceneTemplate.GoalAgainst, SceneTemplate.BreakawayFor,
                SceneTemplate.BreakawayAgainst, SceneTemplate.TerritoryFor, SceneTemplate.TerritoryAgainst,
                SceneTemplate.NearMissHope, SceneTemplate.NearMissScare, SceneTemplate.CalmPossession,
                SceneTemplate.Kickoff, SceneTemplate.Fallback,
            };
            foreach (SceneTemplate t in templates)
            {
                bool done = false;
                ScoreLedger.StagedGoal? goal = ScenePlaybook.ProducesGoal(t)
                    ? new ScoreLedger.StagedGoal(true, true)
                    : (ScoreLedger.StagedGoal?)null;
                stage.PlayScene(Spec(t, goal), null, () => done = true);
                float w = 0f;
                while (!done && w < 8f) { w += Time.deltaTime; yield return null; }
                Assert.IsTrue(done, $"{t} never completed");
                Assert.IsFalse(stage.ScenePlaying, $"{t} left the stage mid-scene");
            }
        }

        [UnityTest]
        public IEnumerator Goal_playback_reports_commit_and_chalked_variants()
        {
            TheaterStage stage = BuildStage();
            stage.timeScale = 0.02f;

            ScoreLedger.StagedGoal? played = null;
            bool done = false;
            stage.PlayScene(Spec(SceneTemplate.GoalFor, new ScoreLedger.StagedGoal(true, true)),
                g => played = g, () => done = true);
            float w = 0f;
            while (!done && w < 8f) { w += Time.deltaTime; yield return null; }
            Assert.IsTrue(played.HasValue, "the goal playback must report");
            Assert.IsTrue(played.Value.Commits && played.Value.ForPicked);

            // The chalked-off variant: full goal drama, VAR takes it away, Commits false rides.
            played = null;
            done = false;
            stage.PlayScene(Spec(SceneTemplate.BreakawayAgainst, new ScoreLedger.StagedGoal(false, false)),
                g => played = g, () => done = true);
            w = 0f;
            while (!done && w < 8f) { w += Time.deltaTime; yield return null; }
            Assert.IsTrue(played.HasValue);
            Assert.IsFalse(played.Value.Commits, "the chalked goal must report Commits=false");
        }

        [UnityTest]
        public IEnumerator Final_scene_stages_the_plan_goals_then_completes()
        {
            TheaterStage stage = BuildStage();
            stage.timeScale = 0.02f;

            var ledger = new ScoreLedger(); // 0-0 entering the final
            ScoreLedger.FinalPlan plan = ledger.PlanFinal(LegGrade.Won); // needs 1 stoppage goal
            Assert.AreEqual(1, plan.Goals.Length);

            int goalsPlayed = 0;
            bool done = false;
            stage.PlayFinalScene(Spec(SceneTemplate.LegFinalWon), plan, g => goalsPlayed++, () => done = true);
            float w = 0f;
            while (!done && w < 10f) { w += Time.deltaTime; yield return null; }
            Assert.IsTrue(done, "the final scene never completed");
            Assert.AreEqual(plan.Goals.Length, goalsPlayed, "every staged goal must visibly play");
        }

        [UnityTest]
        public IEnumerator Pending_window_suspends_at_the_shot_and_resumes_each_way()
        {
            TheaterStage stage = BuildStage();
            stage.timeScale = 0.02f;

            // Suspend: the kill scene freezes mid-flight and HOLDS.
            stage.SuspendKillShot(0);
            float w = 0f;
            while (!stage.SuspendedAtShot && w < 8f) { w += Time.deltaTime; yield return null; }
            Assert.IsTrue(stage.SuspendedAtShot, "the kill scene never reached its suspension point");
            Assert.IsTrue(stage.ScenePlaying, "suspension is mid-scene, not scene-end");

            Vector3 frozenBall = stage.transform.Find("Ball").localPosition;
            for (int i = 0; i < 10; i++) yield return null;
            Assert.AreEqual(frozenBall, stage.transform.Find("Ball").localPosition,
                "the frozen shot must hang mid-flight");

            // Resume as Lost: the flight completes — the killing goal plays, then the collapse.
            var ledger = new ScoreLedger();
            ScoreLedger.FinalPlan lost = ledger.PlanFinal(LegGrade.Lost);
            int goals = 0;
            bool done = false;
            stage.ResumeSuspended(lost, g => goals++, () => done = true);
            w = 0f;
            while (!done && w < 10f) { w += Time.deltaTime; yield return null; }
            Assert.IsTrue(done, "the Lost continuation never completed");
            Assert.AreEqual(lost.Goals.Length, goals, "the killing shot must visibly play");
            Assert.IsFalse(stage.SuspendedAtShot);

            // Resume as VOID (fresh suspension): no goals — the stage dissolves cyan.
            stage.SuspendKillShot(1);
            w = 0f;
            while (!stage.SuspendedAtShot && w < 8f) { w += Time.deltaTime; yield return null; }
            goals = 0;
            done = false;
            stage.ResumeSuspended(new ScoreLedger().PlanFinal(LegGrade.Voided), g => goals++, () => done = true);
            w = 0f;
            while (!done && w < 10f) { w += Time.deltaTime; yield return null; }
            Assert.IsTrue(done, "the VOID continuation never completed");
            Assert.AreEqual(0, goals, "a voided leg stages no goals");
        }

        [UnityTest]
        public IEnumerator Freezing_holds_the_exact_frame()
        {
            TheaterStage stage = BuildStage();
            stage.SetLiveProb(0.8f);
            yield return null;
            yield return null;

            stage.SetFrozen(true);
            float frozenTerr = stage.LastTerritoryX;
            Vector3 frozenBall = stage.transform.Find("Ball").localPosition;
            Vector3 frozenDot = stage.transform.Find("Home3").localPosition;

            for (int i = 0; i < 12; i++) yield return null;

            Assert.IsTrue(stage.IsFrozen);
            Assert.AreEqual(frozenTerr, stage.LastTerritoryX, 1e-6f, "frozen territory must not move");
            Assert.AreEqual(frozenBall, stage.transform.Find("Ball").localPosition, "frozen ball must not move");
            Assert.AreEqual(frozenDot, stage.transform.Find("Home3").localPosition, "frozen dots must not move");

            // Thaw: motion resumes.
            stage.SetFrozen(false);
            stage.SetLiveProb(0.2f);
            yield return null;
            Assert.AreNotEqual(frozenTerr, stage.LastTerritoryX, "thawed territory must move again");
        }
    }
}
