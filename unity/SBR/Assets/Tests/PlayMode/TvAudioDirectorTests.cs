using System.Collections;
using NUnit.Framework;
using SBR.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace SBR.Tests.PlayMode
{
    /// <summary>
    /// Audio v0 smoke (F_0.2.0 M-T5): the director builds all procedural clips, every sting
    /// fires without throwing (batch mode has no audio device — the guards must hold), the
    /// tension/duck loop runs, and the crowd's low-pass lives on its own child so it can
    /// never mud the stings (the Fable-review fix, pinned structurally).
    /// </summary>
    public class TvAudioDirectorTests
    {
        private GameObject _anchor;

        [TearDown]
        public void TearDown()
        {
            if (_anchor != null) Object.Destroy(_anchor);
        }

        [UnityTest]
        public IEnumerator Director_builds_fires_and_ducks_without_throwing()
        {
            _anchor = new GameObject("TvAnchor");
            TvAudioDirector director = TvAudioDirector.Build(_anchor.transform);
            Assert.IsNotNull(director, "the director must build on a valid anchor");

            director.Show(true);
            director.GoalHit(commits: true);
            director.GoalHit(commits: false);
            director.NearMissRiser(4.5f);
            yield return null;
            director.CutRiser();
            director.Whistle();
            director.SlamWon();
            director.SlamLost();
            director.CashOutKaChunk();

            for (int i = 0; i < 5; i++)
            {
                director.SetTension(0.9f, 1f);
                director.Duck(i % 2 == 0, 0.15f);
                yield return null;
            }

            // The structural law: the low-pass filter sits on the crowd's CHILD object, not
            // beside the sting sources — Unity filters process every source on their GO.
            var filter = director.GetComponent<AudioLowPassFilter>();
            Assert.IsNull(filter, "no filter on the director root (it would mud the stings)");
            var crowd = director.transform.Find("CrowdBed");
            Assert.IsNotNull(crowd, "the crowd bed lives on its own child");
            Assert.IsNotNull(crowd.GetComponent<AudioLowPassFilter>(), "the crowd carries the low-pass");
            Assert.IsNotNull(crowd.GetComponent<AudioSource>());

            director.Show(false);
            yield return null;
            Assert.IsNull(TvAudioDirector.Build(null), "a null anchor builds nothing, throws nothing");
        }
    }
}
