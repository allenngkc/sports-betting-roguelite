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
