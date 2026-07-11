using System;
using System.Collections;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SBR.Tests.PlayMode
{
    /// <summary>
    /// M3 PlayMode: the TV plays the real engine's sweat. Loads Room, forces fast pacing via
    /// TvSweatScreen.TimeScaleOverride, and (1) sits and steps a full sweat to settlement, asserting the
    /// bank moved consistently with the ticket's terminal state and nothing threw; and (2) proves standing
    /// mid-sweat freezes the event cursor (design/04: standing pauses, the offer stays frozen).
    /// Requires the scene in EditorBuildSettings - run SBR.GrayboxRoomBuilder.Build first.
    /// </summary>
    public class TvSweatScreenTests
    {
        [UnityTest]
        public IEnumerator SeatedSweatRunsToSettlementAndBankMatchesOutcome()
        {
            yield return LoadRoom();

            var driver = UnityEngine.Object.FindAnyObjectByType<DemoRunDriver>();
            var screen = UnityEngine.Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = UnityEngine.Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(driver, "DemoRunDriver missing");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");

            screen.TimeScaleOverride = 0.0001f; // fast-forward pacing and beats
            couch.transitionDuration = 0.01f;   // batch mode is unthrottled; don't spend 0.35s sitting

            yield return WaitUntil(() => driver.CurrentSession != null, 10f, "driver never locked a ticket");

            // Programmatically sit (drives the real SitSpot -> SeatedChanged wiring).
            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            yield return WaitUntil(() => driver.SettleCount >= 1, 60f, "sweat never reached settlement");

            Assert.AreNotEqual(TicketState.Open, driver.LastTicketState,
                "the settled ticket did not reach a terminal state");

            double finishDelta = driver.LastBankAfterFinish - driver.LastBankBeforeFinish;
            if (driver.LastTicketState == TicketState.Won)
                Assert.AreEqual(driver.LastPotentialPayout, finishDelta, 1e-4,
                    "a won ticket must credit its full payout at FinishSweat");
            else // Lost or CashedOut: FinishSweat credits nothing (cash-out was credited live; no relics)
                Assert.AreEqual(0.0, finishDelta, 1e-4,
                    $"{driver.LastTicketState} ticket must not change the bank at FinishSweat");

            Assert.Greater(driver.Run.Bank, 0.0, "bank went non-positive");
        }

        [UnityTest]
        public IEnumerator StandingMidSweatFreezesTheEventCursor()
        {
            yield return LoadRoom();

            var driver = UnityEngine.Object.FindAnyObjectByType<DemoRunDriver>();
            var screen = UnityEngine.Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = UnityEngine.Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(driver);
            Assert.IsNotNull(screen);
            Assert.IsNotNull(couch);

            // Moderate pacing so events land ~70-180ms apart in real time - a broken pause would
            // measurably advance the cursor inside the freeze windows below at any frame rate.
            screen.TimeScaleOverride = 0.15f;
            couch.transitionDuration = 0.01f; // batch mode is unthrottled; don't spend 0.35s sitting

            yield return WaitUntil(() => driver.CurrentSession != null, 10f, "driver never locked a ticket");

            // Before sitting, the sweat must not advance at all (stepping is gated on seated).
            yield return WaitRealtime(0.3f);
            Assert.AreEqual(0, screen.EventsEmitted, "sweat advanced before the player sat down");

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            yield return WaitUntil(() => screen.EventsEmitted >= 2 && !screen.SweatComplete, 20f,
                "sweat never made mid-run progress");

            // Look away / stand: the cursor must freeze.
            screen.ForceSeated(false);
            yield return WaitRealtime(0.3f); // absorb at most one in-flight step
            int frozenAt = screen.EventsEmitted;
            Assert.IsFalse(screen.SweatComplete, "sweat should still be mid-run when we stand");

            yield return WaitRealtime(0.6f); // several event-lengths of real time
            Assert.AreEqual(frozenAt, screen.EventsEmitted, "event cursor advanced while standing");
            Assert.IsFalse(screen.SweatComplete, "sweat should stay paused while standing");
        }

        // ---- helpers ----

        private static IEnumerator LoadRoom()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("Room", LoadSceneMode.Single);
            Assert.IsNotNull(load, "Room scene not in build settings - run SBR.GrayboxRoomBuilder.Build first.");
            while (!load.isDone) yield return null;
        }

        // Waits are wall-clock, not frame-count: batch mode runs unthrottled (thousands of fps),
        // so frame budgets starve anything driven by Time.deltaTime (e.g. the couch camera lerp).
        private static IEnumerator WaitUntil(Func<bool> cond, float maxSeconds, string failMessage)
        {
            float start = Time.realtimeSinceStartup;
            while (!cond())
            {
                if (Time.realtimeSinceStartup - start > maxSeconds)
                {
                    Assert.Fail($"{failMessage} (waited {maxSeconds}s)");
                    yield break;
                }
                yield return null;
            }
        }

        private static IEnumerator WaitRealtime(float seconds)
        {
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - start < seconds) yield return null;
        }
    }
}
