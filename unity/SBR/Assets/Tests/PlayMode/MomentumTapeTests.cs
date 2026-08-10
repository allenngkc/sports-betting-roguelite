using System.Collections;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace SBR.Tests.PlayMode
{
    /// <summary>
    /// The momentum tape's laws (F_0.2.0 M-T4): dots accumulate per leg strip, resolved legs
    /// collapse to a resolution cap, and a resolved strip accepts no further beats.
    ///
    /// <para><b>The caps carry no hue (T16, TV-01).</b> This comment used to read "green W / red L /
    /// cyan VOID" — that was the violation, not the vocabulary. T16 rules this component "no
    /// numerals, no hue, never above L2", so the three grades are separated on the palette's
    /// colourless three-step ramp instead, under an L2 ceiling. The distinctness assertion below is
    /// unchanged and still passes; what changed is the channel it is distinct in.</para>
    /// </summary>
    public class MomentumTapeTests
    {
        private GameObject _canvasGo;

        [TearDown]
        public void TearDown()
        {
            if (_canvasGo != null) Object.Destroy(_canvasGo);
        }

        [UnityTest]
        public IEnumerator Tape_accumulates_dots_and_collapses_to_caps()
        {
            _canvasGo = new GameObject("TapeCanvas", typeof(Canvas));
            MomentumTape tape = MomentumTape.Build(_canvasGo.transform, Vector2.zero, new Vector2(300f, 50f));
            tape.Show(true);
            tape.ResetForTicket(3);
            yield return null;

            Assert.AreEqual(3, tape.transform.childCount, "one row per leg");

            tape.AppendBeat(0, Color.blue, 0);
            tape.AppendBeat(0, Color.magenta, 2);
            tape.AppendBeat(1, Color.blue, 1);
            yield return null;

            Transform row0 = tape.transform.Find("LegTape_1");
            Transform row1 = tape.transform.Find("LegTape_2");
            Assert.AreEqual(3, row0.childCount, "cap + 2 dots on leg 1");
            Assert.AreEqual(2, row1.childCount, "cap + 1 dot on leg 2");
            Assert.IsFalse(row0.Find("ResolutionCap").GetComponent<UnityEngine.UI.Image>().enabled,
                "an unresolved leg shows no cap");

            // Bands size the dots: band 2 > band 0.
            float small = row0.Find("Beat_1").GetComponent<RectTransform>().sizeDelta.x;
            float big = row0.Find("Beat_2").GetComponent<RectTransform>().sizeDelta.x;
            Assert.Greater(big, small, "|delta| band drives dot size");

            // Resolution collapses the strip to the money cap; the strip goes read-only.
            tape.ResolveLeg(0, LegGrade.Won);
            yield return null;
            var cap0 = row0.Find("ResolutionCap").GetComponent<UnityEngine.UI.Image>();
            Assert.IsTrue(cap0.enabled, "a resolved leg wears its cap");
            Assert.IsFalse(row0.Find("Beat_1").GetComponent<UnityEngine.UI.Image>().enabled,
                "collapse hides the dots");
            int childrenBefore = row0.childCount;
            tape.AppendBeat(0, Color.blue, 1);
            Assert.AreEqual(childrenBefore, row0.childCount, "a resolved strip accepts no more beats");

            tape.ResolveLeg(2, LegGrade.Voided);
            yield return null;
            var cap2 = tape.transform.Find("LegTape_3/ResolutionCap").GetComponent<UnityEngine.UI.Image>();
            Assert.IsTrue(cap2.enabled);
            Assert.AreNotEqual(cap0.color, cap2.color, "W and VOID caps are distinct money signals");
        }
    }
}
