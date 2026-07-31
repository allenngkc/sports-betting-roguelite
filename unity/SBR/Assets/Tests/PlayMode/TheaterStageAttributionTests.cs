using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace SBR.Tests.PlayMode
{
    /// <summary>
    /// TV Sweat Refinement Phase 2D (PRD §7.6): market-attributed corner and booking scenes.
    ///
    /// Phase 2C left Corner and Booking with team attribution wired correctly at the DATA level
    /// (<c>CountBeneficiaryIsHome</c> mirrors the right steps) but with no visible causal
    /// difference on screen: every corner played the SAME four-step cutaway regardless of which
    /// of the three staged grammars (near post / far post / cleared) drove it, with no beat
    /// showing the ball actually leaving the defending side; every booking showed a bare marker
    /// with no visible challenge and no actor attachment. This file proves the fix mechanically —
    /// by reading the authored <c>Step[]</c> script and the stage's real actor-routing state,
    /// never by diagnostics or by trusting a <c>PlanSignature</c> alone (a signature can differ
    /// while the rendered motion stays identical, which is exactly the bug this phase closes).
    /// </summary>
    public class TheaterStageAttributionTests
    {
        private GameObject _canvasGo;

        [TearDown]
        public void TearDown()
        {
            if (_canvasGo != null) Object.Destroy(_canvasGo);
        }

        private TheaterStage BuildStage()
        {
            _canvasGo = new GameObject("AttributionTestCanvas", typeof(Canvas));
            TheaterStage stage = TheaterStage.Build(_canvasGo.transform, Vector2.zero, new Vector2(720f, 252f),
                new Color(0.9f, 0.9f, 0.9f, 0.5f), new Color(0.01f, 0.01f, 0.02f, 1f));
            stage.Show(true);
            // Count markets always present home attacking right (SweatFlavor.PickedHomeForPresentation,
            // see AppendFinalCounts's comment in TheaterStage) — pickedIsHome: true matches that.
            stage.BeginLeg(Color.blue, Color.magenta, pickedIsHome: true, openingProb: 0.5f);
            stage.timeScale = 0.15f;
            return stage;
        }

        // ---------------------------------------------------------------- reflection into the
        // private authored Step[] script.
        //
        // TheaterStageMatrixTests already establishes reflection-into-private-state as this
        // codebase's accepted pattern for an otherwise-unobservable internal contract (its
        // _revealFired probe). This extends the same idiom to the Step[] script itself: reading
        // the authored waypoints/routes/markers directly is the only timing-INDEPENDENT way to
        // prove "the motion on screen differs" — sampling animated dot positions instead would
        // race the SmoothDamp convergence against however many real frames a fast test timeScale
        // happens to allow before a short step's logical duration elapses.

        private static readonly FieldInfo ScriptField =
            typeof(TheaterStage).GetField("_script", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly System.Type StepType = typeof(TheaterStage).GetNestedType("Step", BindingFlags.NonPublic);
        private static readonly FieldInfo BallField = StepType.GetField("Ball");
        private static readonly FieldInfo MarkerField = StepType.GetField("Marker");
        private static readonly FieldInfo RouteField = StepType.GetField("Route");
        private static readonly FieldInfo AtkPickedField = StepType.GetField("AtkPicked");
        private static readonly FieldInfo ChaseField = StepType.GetField("Chase");
        private static readonly FieldInfo TempoField = StepType.GetField("Tempo");
        private static readonly FieldInfo DurField = StepType.GetField("Dur");

        private static byte ByteConst(string name)
            => (byte)typeof(TheaterStage).GetField(name, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

        private static readonly byte MkGoalVal = ByteConst("MkGoal");
        private static readonly byte MkCornerVal = ByteConst("MkCorner");
        private static readonly byte MkBookingVal = ByteConst("MkBooking");
        private static readonly byte RouteCornerVal = ByteConst("RouteCorner");
        private static readonly byte RouteBackLineVal = ByteConst("RouteBackLine");
        // Phase 2E-1: one marker byte per non-goal near-miss payoff (MkSave reused for
        // KeeperSave only — see TheaterStage's own doc on why the other five never reuse it).
        private static readonly byte MkSaveVal = ByteConst("MkSave");
        private static readonly byte MkBlockVal = ByteConst("MkBlock");
        private static readonly byte MkInterceptVal = ByteConst("MkIntercept");
        private static readonly byte MkClearanceVal = ByteConst("MkClearance");
        private static readonly byte MkPostVal = ByteConst("MkPost");
        private static readonly byte MkNearWideVal = ByteConst("MkNearWide");
        private static readonly byte RouteShotVal = ByteConst("RouteShot");
        // Phase 2E-2: SetPiece's static-setup step is the only grammar buildup step that routes
        // via RouteAuthored instead of RoutePass — see BuildGrammarBuildup's doc.
        private static readonly byte RouteAuthoredVal = ByteConst("RouteAuthored");
        // Phase 2E-3: the goal-family restart's two truth-tied routes.
        private static readonly byte RouteKickoffVal = ByteConst("RouteKickoff");

        private struct StepView
        {
            public Vector2 Ball;
            public byte Marker;
            public byte Route;
            public bool AtkPicked;
            public bool Chase;
            public float Tempo;
            public float Dur;
        }

        private static StepView[] ReadScript(TheaterStage stage)
        {
            var arr = (System.Array)ScriptField.GetValue(stage);
            Assert.IsNotNull(arr, "no scene script active — call ReadScript right after Play(Planned)Scene");
            var result = new StepView[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                object step = arr.GetValue(i);
                result[i] = new StepView
                {
                    Ball = (Vector2)BallField.GetValue(step),
                    Marker = (byte)MarkerField.GetValue(step),
                    Route = (byte)RouteField.GetValue(step),
                    AtkPicked = (bool)AtkPickedField.GetValue(step),
                    Chase = (bool)ChaseField.GetValue(step),
                    Tempo = (float)TempoField.GetValue(step),
                    Dur = (float)DurField.GetValue(step),
                };
            }
            return result;
        }

        private static TheaterScenePlan CornerPlan(MovementGrammar grammar, bool beneficiaryIsHome, bool forPicked,
            SceneLane lane)
        {
            SceneTemplate template = beneficiaryIsHome ? SceneTemplate.CornerFor : SceneTemplate.CornerAgainst;
            var sig = new PlanSignature(template, grammar, null, ScenePayoff.CornerDelivery,
                PressureMode.MidPress, SpacingMode.Balanced, ReactionPattern.Recover);
            var diag = new TheaterScenePlanDiagnostics(1, false, false, false, false, null);
            return new TheaterScenePlan(template, SceneFactContract.Corner, grammar, null, ScenePayoff.CornerDelivery,
                PressureMode.MidPress, SpacingMode.Balanced, ReactionPattern.Recover, lane,
                beneficiaryIsHome, forPicked, null, true, sig, diag);
        }

        // ---------------------------------------------------------------- corner attribution

        private static readonly (MovementGrammar Grammar, bool BeneficiaryHome)[] CornerCells =
        {
            (MovementGrammar.NearPost, true), (MovementGrammar.NearPost, false),
            (MovementGrammar.FarPost, true), (MovementGrammar.FarPost, false),
            (MovementGrammar.Cleared, true), (MovementGrammar.Cleared, false),
        };

        [UnityTest]
        public IEnumerator Corner_grammars_render_as_three_distinct_attributed_sequences()
        {
            TheaterStage stage = BuildStage();

            // SceneLane.NearFlank (0.32), not Center (0.5): NearPost's delivery lands at `lane`
            // and FarPost's at `1 - lane` — at Center those are the SAME value (0.5), which would
            // silently hide the exact regression this test exists to catch.
            var step1YByGrammar = new Dictionary<MovementGrammar, float>();
            var deliveryYByGrammar = new Dictionary<MovementGrammar, float>();

            foreach (var cell in CornerCells)
            {
                string ctx = $"{cell.Grammar}/beneficiaryHome={cell.BeneficiaryHome}";
                var count = new CountLedger.StagedCount(cell.BeneficiaryHome ? 1 : 0, cell.BeneficiaryHome ? 0 : 1, 0);
                Assert.AreEqual(cell.BeneficiaryHome, count.BeneficiaryIsHome, $"{ctx}: test setup");

                SceneSpec spec = new SceneSpec(SceneTemplate.CornerFor, 0, false, false, true, null, count, null,
                    MarketKind.TotalCorners, new SweatPacer().SceneSeconds(SceneTemplate.CornerFor, false),
                    count.BeneficiaryIsHome);
                TheaterScenePlan plan = CornerPlan(cell.Grammar, count.BeneficiaryIsHome, true, SceneLane.NearFlank);

                int goalCalls = 0, countCalls = 0, revealCalls = 0;
                CountLedger.StagedCount? played = null;
                bool done = false;
                stage.PlayPlannedScene(plan, spec, g => goalCalls++, () => revealCalls++, () => done = true,
                    c => { countCalls++; played = c; });
                Assert.IsTrue(stage.ScenePlaying, $"{ctx}: never started");

                // Mechanical proof #1 (data level, no animation timing involved): the script
                // itself never begins as an unattributed cutaway at the flag, a REAL defending-
                // side actor is touched (RouteBackLine) immediately before the RouteCorner
                // delivery step, and no step anywhere carries a goal marker.
                StepView[] script = ReadScript(stage);
                Assert.Greater(script.Length, 3, $"{ctx}: too few steps for a drive-in/deflect/deliver shape");
                Assert.AreNotEqual(RouteCornerVal, script[0].Route,
                    $"{ctx}: a corner must not begin as an unattributed cutaway at the flag");
                Assert.AreNotEqual(MkCornerVal, script[0].Marker, $"{ctx}: the marker must not fire on step 0");

                int deliveryIx = System.Array.FindIndex(script, s => s.Marker == MkCornerVal);
                Assert.GreaterOrEqual(deliveryIx, 1, $"{ctx}: no MkCorner delivery step found");
                Assert.AreEqual(RouteCornerVal, script[deliveryIx].Route,
                    $"{ctx}: the delivery step must route via RouteCorner");
                Assert.AreEqual(RouteBackLineVal, script[deliveryIx - 1].Route,
                    $"{ctx}: the step immediately before delivery must be a real defending-side " +
                    "touch — the ball going out off the defender");
                foreach (StepView s in script)
                    Assert.AreNotEqual(MkGoalVal, s.Marker, $"{ctx}: corner scenes must never carry a goal marker");

                step1YByGrammar[cell.Grammar] = script[1].Ball.y;
                deliveryYByGrammar[cell.Grammar] = script[deliveryIx].Ball.y;

                float w = 0f;
                while (!done && w < 8f) { w += Time.deltaTime; yield return null; }
                Assert.IsTrue(done, $"{ctx}: never completed");
                Assert.IsFalse(stage.ScenePlaying, $"{ctx}: left the stage mid-scene");
                Assert.AreEqual(0, goalCalls, $"{ctx}: corner scenes must never fire a goal callback");
                Assert.AreEqual(1, countCalls, $"{ctx}: must fire the count callback exactly once");
                Assert.AreEqual(1, revealCalls, $"{ctx}: must reveal exactly once");
                Assert.IsTrue(played.HasValue, $"{ctx}: count callback never delivered a payload");
                Assert.AreEqual(count.HomeDelta, played.Value.HomeDelta, $"{ctx}: staged payload must be preserved");
                Assert.AreEqual(count.AwayDelta, played.Value.AwayDelta, $"{ctx}: staged payload must be preserved");
                Assert.AreEqual(count.BeneficiaryIsHome, played.Value.BeneficiaryIsHome,
                    $"{ctx}: staged payload must be preserved");

                // Mechanical proof #2 (playback level): the actor the ball-out-off-the-defender
                // beat actually touched is the OPPOSITE team of the staged beneficiary — proving
                // routing followed CountBeneficiaryIsHome, never the mood/template.
                Assert.IsTrue(stage.LastTouchedActor.HasValue,
                    $"{ctx}: the ball-out-off-the-defender beat never touched a real actor");
                Assert.AreEqual(!cell.BeneficiaryHome, stage.LastTouchedActor.Value.IsHome,
                    $"{ctx}: the defending-side actor touched must be the OPPOSITE of the beneficiary");
            }

            // Mechanical proof #3: the three grammars are visibly different sequences, not one
            // sequence a signature varies — NearPost and FarPost deliver to different lanes;
            // Cleared's second step stays central (0.5) instead of following `lane` like the
            // other two, because it cuts inside rather than going wide first.
            Assert.AreNotEqual(deliveryYByGrammar[MovementGrammar.NearPost], deliveryYByGrammar[MovementGrammar.FarPost],
                "NearPost and FarPost must deliver to visibly different lanes (near vs far post)");
            Assert.AreNotEqual(step1YByGrammar[MovementGrammar.NearPost], step1YByGrammar[MovementGrammar.Cleared],
                "Cleared's approach must read differently on screen from NearPost's/FarPost's wide buildup");
            Assert.AreNotEqual(step1YByGrammar[MovementGrammar.FarPost], step1YByGrammar[MovementGrammar.Cleared],
                "Cleared's approach must read differently on screen from NearPost's/FarPost's wide buildup");
        }

        [UnityTest]
        public IEnumerator Corner_mood_and_beneficiary_can_disagree_and_both_stay_correct()
        {
            TheaterStage stage = BuildStage();

            // The bettor's hope/dread MOOD (ForPicked / the CornerFor template — TheaterChoreographer's
            // reading of the selection's Over/Under sense) says "hope": ForPicked=true,
            // Template=CornerFor. The staged FACT the engine actually committed says the AWAY
            // side wins this corner. TVS-S01's guard: these must be free to disagree, and
            // routing must follow the fact, never the mood.
            var count = new CountLedger.StagedCount(0, 1, 0); // AWAY actually wins the corner
            Assert.IsFalse(count.BeneficiaryIsHome, "test setup: this batch must credit AWAY");

            SceneSpec spec = new SceneSpec(SceneTemplate.CornerFor, 0, false, false,
                forPicked: true, null, count, null, MarketKind.TotalCorners,
                new SweatPacer().SceneSeconds(SceneTemplate.CornerFor, false), count.BeneficiaryIsHome);
            TheaterScenePlan plan = CornerPlan(MovementGrammar.NearPost, count.BeneficiaryIsHome,
                spec.ForPicked, SceneLane.Center);

            int goalCalls = 0, countCalls = 0, revealCalls = 0;
            CountLedger.StagedCount? played = null;
            bool done = false;
            stage.PlayPlannedScene(plan, spec, g => goalCalls++, () => revealCalls++, () => done = true,
                c => { countCalls++; played = c; });

            float w = 0f;
            while (!done && w < 8f) { w += Time.deltaTime; yield return null; }
            Assert.IsTrue(done, "the disagreement scene never completed");
            Assert.AreEqual(0, goalCalls);
            Assert.AreEqual(1, countCalls);
            Assert.AreEqual(1, revealCalls);
            Assert.IsTrue(played.HasValue);
            Assert.IsFalse(played.Value.BeneficiaryIsHome,
                "the staged fact says AWAY won it — the count payload must say so regardless of the hope mood");
            Assert.IsTrue(stage.LastTouchedActor.HasValue);
            Assert.IsTrue(stage.LastTouchedActor.Value.IsHome,
                "AWAY is the beneficiary, so HOME must be the side mechanically touched conceding the " +
                "corner — the CornerFor/hope mood must never leak into physical routing");
        }

        // ---------------------------------------------------------------- booking attribution

        [UnityTest]
        public IEnumerator Booking_shows_a_challenge_before_the_marker_and_cards_the_fouling_actor()
        {
            TheaterStage stage = BuildStage();

            foreach (bool beneficiaryHome in new[] { true, false })
            {
                string ctx = $"beneficiaryHome={beneficiaryHome}";
                var count = new CountLedger.StagedCount(beneficiaryHome ? 1 : 0, beneficiaryHome ? 0 : 1, 0);
                Assert.AreEqual(beneficiaryHome, count.BeneficiaryIsHome, $"{ctx}: test setup");

                SceneSpec spec = new SceneSpec(SceneTemplate.Booking, 0, false, false, true, null, count, null,
                    MarketKind.TotalCards, new SweatPacer().SceneSeconds(SceneTemplate.Booking, false),
                    count.BeneficiaryIsHome);

                int goalCalls = 0, countCalls = 0, revealCalls = 0;
                CountLedger.StagedCount? played = null;
                bool done = false;
                // Plan-free (legacy PlayScene) path deliberately: Booking's fix applies there too
                // (see BuildBookingCore's call site — it is not gated on a plan being present).
                stage.PlayScene(spec, g => goalCalls++, () => revealCalls++, () => done = true,
                    c => { countCalls++; played = c; });
                Assert.IsTrue(stage.ScenePlaying, $"{ctx}: never started");

                // Mechanical proof #1: a real challenge beat (chase, high tempo) immediately
                // precedes the marker, on the SAME side, so the card never appears out of nowhere.
                StepView[] script = ReadScript(stage);
                int markerIx = System.Array.FindIndex(script, s => s.Marker == MkBookingVal);
                Assert.GreaterOrEqual(markerIx, 1, $"{ctx}: no visible run-up before the marker");
                Assert.IsTrue(script[markerIx - 1].Chase,
                    $"{ctx}: the step before the marker must be a visible challenge (chase)");
                Assert.GreaterOrEqual(script[markerIx - 1].Tempo, 0.9f,
                    $"{ctx}: the challenge beat must read as urgent contact, not idle buildup");
                Assert.AreEqual(script[markerIx].AtkPicked, script[markerIx - 1].AtkPicked,
                    $"{ctx}: the marker step must stay on the SAME side as the challenge that led into it");

                float w = 0f;
                while (!done && w < 8f) { w += Time.deltaTime; yield return null; }
                Assert.IsTrue(done, $"{ctx}: never completed");
                Assert.IsFalse(stage.ScenePlaying, $"{ctx}: left the stage mid-scene");
                Assert.AreEqual(0, goalCalls, $"{ctx}: booking scenes must never fire a goal callback");
                Assert.AreEqual(1, countCalls, $"{ctx}: must fire the count callback exactly once");
                Assert.AreEqual(1, revealCalls, $"{ctx}: must reveal exactly once");
                Assert.IsTrue(played.HasValue, $"{ctx}: count callback never delivered a payload");
                Assert.AreEqual(count.BeneficiaryIsHome, played.Value.BeneficiaryIsHome,
                    $"{ctx}: staged payload must be preserved");

                // Mechanical proof #2: the marker attached to a REAL actor on the fouling side —
                // not merely at the ball's coordinate.
                Assert.IsTrue(stage.MarkerActorRouted.HasValue, $"{ctx}: the marker never attached to a real actor");
                Assert.AreEqual(beneficiaryHome, stage.MarkerActorRouted.Value.IsHome,
                    $"{ctx}: the card must land on the fouling side's actor, matching the staged beneficiary");
            }
        }

        // ---------------------------------------------------------------- near-miss payoffs
        //
        // Phase 2E-1 (PRD §7.2/§7.3/§10): the planner already chooses among six near-miss
        // payoffs (Block/Interception/KeeperSave/Clearance/Post/NearWide), but until now the
        // stage rendered ONE shape (approach, approach, shot, "off the bar" MkSave, a dead-air
        // hold, "cleared off the line") regardless of which payoff was staged. This proves the
        // fix the same way the corner/booking tests above do: reading the authored Step[] script
        // mechanically, never trusting a PlanSignature or a diagnostics string alone.

        private static TheaterScenePlan NearMissPlan(ScenePayoff payoff, bool hope, SceneLane lane)
        {
            SceneTemplate template = hope ? SceneTemplate.NearMissHope : SceneTemplate.NearMissScare;
            var sig = new PlanSignature(template, MovementGrammar.Central, ChanceShape.Direct, payoff,
                PressureMode.MidPress, SpacingMode.Balanced, ReactionPattern.Recover);
            var diag = new TheaterScenePlanDiagnostics(1, false, false, false, false, null);
            return new TheaterScenePlan(template, SceneFactContract.NearMiss, MovementGrammar.Central,
                ChanceShape.Direct, payoff, PressureMode.MidPress, SpacingMode.Balanced,
                ReactionPattern.Recover, lane, null, true, null, false, sig, diag);
        }

        private static readonly (ScenePayoff Payoff, byte ExpectedMarker)[] NearMissCells =
        {
            (ScenePayoff.Block, MkBlockVal),
            (ScenePayoff.Interception, MkInterceptVal),
            (ScenePayoff.KeeperSave, MkSaveVal),
            (ScenePayoff.Clearance, MkClearanceVal),
            (ScenePayoff.Post, MkPostVal),
            (ScenePayoff.NearWide, MkNearWideVal),
        };

        [UnityTest]
        public IEnumerator NearMiss_payoffs_render_as_six_distinct_authored_endings()
        {
            TheaterStage stage = BuildStage();
            var routeSignatures = new Dictionary<ScenePayoff, string>();

            foreach (var cell in NearMissCells)
            {
                string ctx = $"{cell.Payoff}";
                SceneSpec spec = new SceneSpec(SceneTemplate.NearMissHope, 0, false, false, true, null,
                    new SweatPacer().SceneSeconds(SceneTemplate.NearMissHope, false));
                TheaterScenePlan plan = NearMissPlan(cell.Payoff, hope: true, SceneLane.Center);

                int goalCalls = 0, revealCalls = 0;
                bool done = false;
                stage.PlayPlannedScene(plan, spec, g => goalCalls++, () => revealCalls++, () => done = true);
                Assert.IsTrue(stage.ScenePlaying, $"{ctx}: never started");

                // Mechanical proof #1 (data level): a near miss never carries a goal or corner
                // marker, fires exactly one payoff marker matching the staged ScenePayoff, and
                // the authored duration sums to the scene's total (B * 1.00 — Phase 2D's same
                // invariant for the three corner shapes).
                StepView[] script = ReadScript(stage);
                int markerCount = 0, markerIx = -1;
                float durSum = 0f;
                foreach (StepView s in script)
                {
                    Assert.AreNotEqual(MkGoalVal, s.Marker, $"{ctx}: a near miss must never carry a goal marker");
                    Assert.AreNotEqual(MkCornerVal, s.Marker,
                        $"{ctx}: a near miss must never award a corner (no count fact exists on this spec)");
                    durSum += s.Dur;
                }
                for (int i = 0; i < script.Length; i++)
                    if (script[i].Marker != 0) { markerCount++; markerIx = i; }
                Assert.AreEqual(1, markerCount, $"{ctx}: must fire exactly one payoff marker");
                Assert.AreEqual(cell.ExpectedMarker, script[markerIx].Marker,
                    $"{ctx}: the payoff marker must match the staged ScenePayoff");
                Assert.AreEqual(spec.Duration, durSum, spec.Duration * 0.001f,
                    $"{ctx}: authored total duration must be preserved (B * 1.00)");

                // Mechanical proof #2: Interception is the only shape that wins the ball back
                // BEFORE any shot is struck — no RouteShot step anywhere in its script.
                bool hasShot = System.Array.Exists(script, s => s.Route == RouteShotVal);
                if (cell.Payoff == ScenePayoff.Interception)
                    Assert.IsFalse(hasShot, $"{ctx}: interception must win the ball back before any shot");

                routeSignatures[cell.Payoff] = string.Join(",", System.Array.ConvertAll(script, s => s.Route.ToString()));

                float w = 0f;
                while (!done && w < 8f) { w += Time.deltaTime; yield return null; }
                Assert.IsTrue(done, $"{ctx}: never completed");
                Assert.IsFalse(stage.ScenePlaying, $"{ctx}: left the stage mid-scene");
                Assert.AreEqual(0, goalCalls, $"{ctx}: a near miss must never fire a goal callback");
                Assert.AreEqual(1, revealCalls, $"{ctx}: must reveal exactly once");
            }

            // Mechanical proof #3: all six are genuinely different Route sequences, not one
            // sequence a signature varies.
            var distinctShapes = new HashSet<string>(routeSignatures.Values);
            Assert.AreEqual(6, distinctShapes.Count,
                "all six near-miss payoffs must render as mechanically distinct Route sequences");
        }

        [UnityTest]
        public IEnumerator NearMiss_payoff_shape_and_hope_dread_mood_stay_independent()
        {
            TheaterStage stage = BuildStage();

            // TVS-S01's guard, restated for near miss: the payoff SHAPE (plan.Payoff) and the
            // bettor's hope/dread MOOD (NearMissHope vs NearMissScare) must be free to vary
            // independently. Proof: for the SAME payoff, Hope's script and Scare's script must
            // carry the identical Marker/Route/Chase sequence (the shape), differing ONLY in the
            // mirror-sensitive fields (Ball.x, AtkPicked) — exactly what Mirror() does and nothing
            // else, i.e. choosing the mood never changes WHICH marker fires or the route taken.
            foreach (var cell in NearMissCells)
            {
                string ctx = $"{cell.Payoff}";
                SceneSpec hopeSpec = new SceneSpec(SceneTemplate.NearMissHope, 0, false, false, true, null,
                    new SweatPacer().SceneSeconds(SceneTemplate.NearMissHope, false));
                SceneSpec scareSpec = new SceneSpec(SceneTemplate.NearMissScare, 0, false, false, true, null,
                    new SweatPacer().SceneSeconds(SceneTemplate.NearMissScare, false));
                TheaterScenePlan hopePlan = NearMissPlan(cell.Payoff, hope: true, SceneLane.Center);
                TheaterScenePlan scarePlan = NearMissPlan(cell.Payoff, hope: false, SceneLane.Center);

                bool hopeDone = false;
                stage.PlayPlannedScene(hopePlan, hopeSpec, null, null, () => hopeDone = true);
                StepView[] hopeScript = ReadScript(stage);
                float w = 0f;
                while (!hopeDone && w < 8f) { w += Time.deltaTime; yield return null; }
                Assert.IsTrue(hopeDone, $"{ctx} hope: never completed");

                bool scareDone = false;
                stage.PlayPlannedScene(scarePlan, scareSpec, null, null, () => scareDone = true);
                StepView[] scareScript = ReadScript(stage);
                w = 0f;
                while (!scareDone && w < 8f) { w += Time.deltaTime; yield return null; }
                Assert.IsTrue(scareDone, $"{ctx} scare: never completed");

                Assert.AreEqual(hopeScript.Length, scareScript.Length, $"{ctx}: mood must not change step count");
                for (int i = 0; i < hopeScript.Length; i++)
                {
                    Assert.AreEqual(hopeScript[i].Marker, scareScript[i].Marker,
                        $"{ctx} step {i}: mood must not change which marker fires");
                    Assert.AreEqual(hopeScript[i].Route, scareScript[i].Route,
                        $"{ctx} step {i}: mood must not change the route taken");
                    Assert.AreEqual(hopeScript[i].Chase, scareScript[i].Chase,
                        $"{ctx} step {i}: mood must not change the chase beat");
                    Assert.AreEqual(!hopeScript[i].AtkPicked, scareScript[i].AtkPicked,
                        $"{ctx} step {i}: Scare must mirror AtkPicked (opposite side's story)");
                    Assert.AreEqual(1f - hopeScript[i].Ball.x, scareScript[i].Ball.x, 0.0001f,
                        $"{ctx} step {i}: Scare must mirror the ball's x coordinate");
                }
            }
        }

        // ---------------------------------------------------------------- Phase 2E-2: grammars

        private static TheaterScenePlan GrammarPlan(MovementGrammar grammar, SceneLane lane)
        {
            const SceneTemplate template = SceneTemplate.NearMissHope;
            var sig = new PlanSignature(template, grammar, ChanceShape.Direct, ScenePayoff.KeeperSave,
                PressureMode.MidPress, SpacingMode.Balanced, ReactionPattern.Recover);
            var diag = new TheaterScenePlanDiagnostics(1, false, false, false, false, null);
            return new TheaterScenePlan(template, SceneFactContract.NearMiss, grammar,
                ChanceShape.Direct, ScenePayoff.KeeperSave, PressureMode.MidPress, SpacingMode.Balanced,
                ReactionPattern.Recover, lane, null, true, null, false, sig, diag);
        }

        private static readonly MovementGrammar[] BuildupGrammars =
        {
            MovementGrammar.Central,
            MovementGrammar.Wing,
            MovementGrammar.Switch,
            MovementGrammar.Counter,
            MovementGrammar.SetPiece,
        };

        /// <summary>Phase 2E-2 (PRD §7.3, §2): the five buildup grammars must render as five
        /// mechanically different approaches, not one approach with a different lane — that is the
        /// exact defect PRD §2 names. Grammar is held as the ONLY variable here: same template,
        /// same payoff, same lane, same pressure/spacing/reaction. Anything that differs between
        /// these five scripts is therefore attributable to grammar alone.</summary>
        [UnityTest]
        public IEnumerator Buildup_grammars_render_as_five_distinct_approaches()
        {
            TheaterStage stage = BuildStage();
            var fingerprints = new Dictionary<MovementGrammar, string>();
            var totals = new Dictionary<MovementGrammar, float>();
            var lateralTravel = new Dictionary<MovementGrammar, float>();

            foreach (MovementGrammar grammar in BuildupGrammars)
            {
                string ctx = $"{grammar}";
                // NearFlank, not Center: Wing widens a lane AWAY from centre and Switch transfers
                // near-side to far-side, so a centred lane carries no side information for either
                // to express. Center is covered by the matrix; this test isolates the grammars.
                SceneSpec spec = new SceneSpec(SceneTemplate.NearMissHope, 0, false, false, true, null,
                    new SweatPacer().SceneSeconds(SceneTemplate.NearMissHope, false));
                TheaterScenePlan plan = GrammarPlan(grammar, SceneLane.NearFlank);

                int goalCalls = 0, revealCalls = 0;
                bool done = false;
                stage.PlayPlannedScene(plan, spec, g => goalCalls++, () => revealCalls++, () => done = true);
                Assert.IsTrue(stage.ScenePlaying, $"{ctx}: never started");

                StepView[] script = ReadScript(stage);
                Assert.Greater(script.Length, 0, $"{ctx}: empty script");

                float durSum = 0f, maxLateral = 0f;
                var fp = new System.Text.StringBuilder();
                for (int i = 0; i < script.Length; i++)
                {
                    StepView s = script[i];
                    durSum += s.Dur;
                    Assert.AreNotEqual(MkGoalVal, s.Marker,
                        $"{ctx} step {i}: a near miss must never carry a goal marker, whatever the grammar");
                    Assert.AreNotEqual(MkCornerVal, s.Marker,
                        $"{ctx} step {i}: no count fact on this spec, so no corner may be awarded");
                    if (i > 0)
                        maxLateral = Mathf.Max(maxLateral, Mathf.Abs(s.Ball.y - script[i - 1].Ball.y));
                    // Route + chase + quantised lane is the mechanical shape of the approach —
                    // deliberately NOT the plan signature, which is what §7.4 compares and what
                    // this test must not be able to pass by merely varying.
                    fp.Append(s.Route).Append(':').Append(s.Chase ? '1' : '0').Append(':')
                      .Append(Mathf.RoundToInt(s.Ball.y * 20f)).Append('|');
                }

                fingerprints[grammar] = fp.ToString();
                totals[grammar] = durSum;
                lateralTravel[grammar] = maxLateral;

                float timeout = spec.Duration * 4f + 2f;
                float t = 0f;
                while (!done && t < timeout) { t += Time.deltaTime; yield return null; }
                Assert.IsTrue(done, $"{ctx}: did not complete within {timeout:0.0}s");
                Assert.AreEqual(0, goalCalls, $"{ctx}: a near miss must never fire a goal callback");
                Assert.AreEqual(1, revealCalls, $"{ctx}: exactly one reveal, whatever the grammar");
            }

            // Every grammar keeps the scene's authored duration — grammar reshapes the approach,
            // it never buys or spends time (Phase 2D/2E-1 hold the same invariant).
            float reference = totals[MovementGrammar.Central];
            foreach (MovementGrammar g in BuildupGrammars)
                Assert.AreEqual(reference, totals[g], 0.001f,
                    $"{g}: authored duration must match every other grammar's ({reference:0.000}s)");

            // Pairwise mechanical distinctness — the whole point of the phase.
            foreach (MovementGrammar a in BuildupGrammars)
                foreach (MovementGrammar b in BuildupGrammars)
                {
                    if (a == b) continue;
                    Assert.AreNotEqual(fingerprints[a], fingerprints[b],
                        $"{a} and {b} render an identical approach — grammar is not being expressed");
                }

            // Switch's defining move is the long diagonal transfer: it must cross more of the
            // pitch laterally than Central, which deliberately compresses toward the middle.
            Assert.Greater(lateralTravel[MovementGrammar.Switch], lateralTravel[MovementGrammar.Central],
                "Switch must travel further laterally than Central — the transfer is its whole idea");
        }

        // ---------------------------------------------------------------- Phase 2E-3: chance
        // shapes and reactions.

        private static TheaterScenePlan GoalPlan(ChanceShape shape, ReactionPattern reaction)
        {
            const SceneTemplate template = SceneTemplate.GoalFor;
            var sig = new PlanSignature(template, MovementGrammar.Central, shape, ScenePayoff.Goal,
                PressureMode.MidPress, SpacingMode.Balanced, reaction);
            var diag = new TheaterScenePlanDiagnostics(1, false, false, false, false, null);
            return new TheaterScenePlan(template, SceneFactContract.Goal, MovementGrammar.Central,
                shape, ScenePayoff.Goal, PressureMode.MidPress, SpacingMode.Balanced,
                reaction, SceneLane.NearFlank, null, true, null, false, sig, diag);
        }

        private static readonly ChanceShape[] ChanceShapes =
        {
            ChanceShape.ThroughBall,
            ChanceShape.Cross,
            ChanceShape.Cutback,
            ChanceShape.Rebound,
            ChanceShape.Direct,
        };

        /// <summary>Phase 2E-3 (PRD §7.1, §7.3): the five chance shapes must deliver the ball into
        /// the box five mechanically different ways. Chance shape is held as the ONLY variable —
        /// same template, grammar, payoff, pressure, spacing, reaction, lane — so anything that
        /// differs between these scripts is attributable to the shape alone.
        ///
        /// The truth assertion carried here matters more than the distinctness one: a goal scene
        /// stages exactly ONE goal, so however many touches a shape takes to arrive, exactly one
        /// MkGoal marker may fire. Rebound is the shape that could break this — it authors a first
        /// attempt that is visibly stopped — so it is checked like every other shape rather than
        /// exempted.</summary>
        [UnityTest]
        public IEnumerator Chance_shapes_deliver_five_distinct_ways_and_still_stage_one_goal()
        {
            TheaterStage stage = BuildStage();
            var fingerprints = new Dictionary<ChanceShape, string>();
            var totals = new Dictionary<ChanceShape, float>();

            foreach (ChanceShape shape in ChanceShapes)
            {
                string ctx = $"{shape}";
                SceneSpec spec = new SceneSpec(SceneTemplate.GoalFor, 0, false, false, true,
                    new ScoreLedger.StagedGoal(true, true),
                    new SweatPacer().SceneSeconds(SceneTemplate.GoalFor, false));

                int goalCalls = 0, revealCalls = 0;
                bool done = false;
                stage.PlayPlannedScene(GoalPlan(shape, ReactionPattern.Celebrate), spec,
                    _ => goalCalls++, () => revealCalls++, () => done = true);
                Assert.IsTrue(stage.ScenePlaying, $"{ctx}: never started");

                StepView[] script = ReadScript(stage);
                int goalMarkers = 0;
                float durSum = 0f;
                var fp = new System.Text.StringBuilder();
                foreach (StepView s in script)
                {
                    durSum += s.Dur;
                    if (s.Marker == MkGoalVal) goalMarkers++;
                    fp.Append(s.Route).Append(':')
                      .Append(Mathf.RoundToInt(s.Ball.x * 20f)).Append(',')
                      .Append(Mathf.RoundToInt(s.Ball.y * 20f)).Append('|');
                }

                Assert.AreEqual(1, goalMarkers,
                    $"{ctx}: a goal scene stages exactly one goal — however many touches the shape takes");

                fingerprints[shape] = fp.ToString();
                totals[shape] = durSum;

                float timeout = spec.Duration * 4f + 2f;
                float t = 0f;
                while (!done && t < timeout) { t += Time.deltaTime; yield return null; }
                Assert.IsTrue(done, $"{ctx}: did not complete within {timeout:0.0}s");
                Assert.AreEqual(1, goalCalls, $"{ctx}: exactly one goal callback");
                Assert.AreEqual(1, revealCalls, $"{ctx}: exactly one reveal");
            }

            float reference = totals[ChanceShape.Direct];
            foreach (ChanceShape s in ChanceShapes)
                Assert.AreEqual(reference, totals[s], 0.001f,
                    $"{s}: authored duration must match every other shape's ({reference:0.000}s)");

            foreach (ChanceShape a in ChanceShapes)
                foreach (ChanceShape b in ChanceShapes)
                {
                    if (a == b) continue;
                    Assert.AreNotEqual(fingerprints[a], fingerprints[b],
                        $"{a} and {b} deliver identically — the chance shape is not being expressed");
                }
        }

        /// <summary>Phase 2E-3: Celebrate and Collapse are read from the plan, not from the
        /// template's hardcoded tail (Phase 2C's report flagged that they were not). The same
        /// staged goal with the same shape must therefore produce two different tails.</summary>
        [UnityTest]
        public IEnumerator Celebrate_and_Collapse_produce_different_tails_from_the_same_staged_goal()
        {
            TheaterStage stage = BuildStage();

            SceneSpec spec = new SceneSpec(SceneTemplate.GoalFor, 0, false, false, true,
                new ScoreLedger.StagedGoal(true, true),
                new SweatPacer().SceneSeconds(SceneTemplate.GoalFor, false));

            stage.PlayPlannedScene(GoalPlan(ChanceShape.Direct, ReactionPattern.Celebrate), spec,
                _ => { }, () => { }, () => { });
            StepView[] celebrate = ReadScript(stage);
            float celebrateDur = 0f;
            var celebrateFp = new System.Text.StringBuilder();
            foreach (StepView s in celebrate)
            {
                celebrateDur += s.Dur;
                celebrateFp.Append(s.Route).Append(':').Append(s.AtkPicked ? '1' : '0')
                           .Append(':').Append(Mathf.RoundToInt(s.Ball.x * 20f)).Append('|');
            }
            stage.CancelScene();
            yield return null;

            stage.PlayPlannedScene(GoalPlan(ChanceShape.Direct, ReactionPattern.Collapse), spec,
                _ => { }, () => { }, () => { });
            StepView[] collapse = ReadScript(stage);
            float collapseDur = 0f;
            var collapseFp = new System.Text.StringBuilder();
            foreach (StepView s in collapse)
            {
                collapseDur += s.Dur;
                collapseFp.Append(s.Route).Append(':').Append(s.AtkPicked ? '1' : '0')
                          .Append(':').Append(Mathf.RoundToInt(s.Ball.x * 20f)).Append('|');
            }
            stage.CancelScene();
            yield return null;

            Assert.AreNotEqual(celebrateFp.ToString(), collapseFp.ToString(),
                "Celebrate and Collapse must render different reactions to the same staged goal");
            Assert.AreEqual(celebrateDur, collapseDur, 0.001f,
                "a reaction reshapes the tail; it never buys or spends time");
        }

        /// <summary>Phase 2E-2: SetPiece and Counter each have a defining beat the other grammars
        /// must not accidentally acquire — a held dead-ball setup, and a turnover that only starts
        /// being chased once possession has changed.</summary>
        [UnityTest]
        public IEnumerator SetPiece_holds_a_static_setup_and_Counter_chases_only_after_the_turnover()
        {
            TheaterStage stage = BuildStage();

            SceneSpec spec = new SceneSpec(SceneTemplate.NearMissHope, 0, false, false, true, null,
                new SweatPacer().SceneSeconds(SceneTemplate.NearMissHope, false));

            stage.PlayPlannedScene(GrammarPlan(MovementGrammar.SetPiece, SceneLane.NearFlank), spec,
                _ => { }, () => { }, () => { });
            StepView[] setPiece = ReadScript(stage);
            Assert.AreEqual(RouteAuthoredVal, setPiece[0].Route,
                "SetPiece opens on a held dead-ball position, not a live pass");
            Assert.Less(setPiece[0].Tempo, 0.5f,
                "SetPiece's opening beat is a static setup — the ball barely moves");
            stage.CancelScene();
            yield return null;

            stage.PlayPlannedScene(GrammarPlan(MovementGrammar.Counter, SceneLane.NearFlank), spec,
                _ => { }, () => { }, () => { });
            StepView[] counter = ReadScript(stage);
            Assert.IsFalse(counter[0].Chase,
                "Counter's turnover is won cleanly — nobody is chasing yet on the first beat");
            Assert.IsTrue(System.Array.Exists(counter, s => s.Chase),
                "Counter must show a recovering chase once the break is under way");
            stage.CancelScene();
            yield return null;
        }
    }
}
