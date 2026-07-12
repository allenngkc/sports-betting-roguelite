using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SBR.Tests.PlayMode
{
    /// <summary>
    /// M4 PlayMode: the real round loop through the room's surfaces. Tickets are placed through the
    /// engine (DemoTicketPolicy is the auto-play fixture — the laptop UI is a thin renderer over the
    /// EditMode-tested BetslipModel), the director locks, the TV walks the sweats serially while
    /// seated, the round settles and the shop opens. Also: standing mid-sweat freezes the cursor
    /// (design/04), and a zero-ticket lock settles on the spot.
    /// All waits are wall-clock — batch mode runs unthrottled (M3's lesson).
    /// Requires the scene in EditorBuildSettings - run SBR.GrayboxRoomBuilder.Build first.
    /// </summary>
    public class TvSweatScreenTests
    {
        [UnityTest]
        public IEnumerator FullRound_TwoTickets_SweatsSeriallyToSettleAndShop()
        {
            yield return LoadRoom();

            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            var screen = UnityEngine.Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = UnityEngine.Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director, "RunDirector missing");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");

            screen.TimeScaleOverride = 0.0001f; // fast-forward pacing, beats and cards
            couch.transitionDuration = 0.01f;

            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");
            Run run = director.Run;
            Assert.AreEqual(Phase.Betting, run.Phase, "a fresh run opens in Betting");

            // Ticket 1 via the auto-play fixture; ticket 2 a single-leg bet on an unused matchup.
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            run.PlaceTicket(new List<Pick> { new Pick(UnusedMatchup(run, picks), Side.Home) }, 10);

            director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);
            Assert.AreEqual(2, run.Sweats.Count, "one session per ticket");

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            yield return WaitUntil(
                () => run.Phase == Phase.Shop || run.Phase == Phase.RunWon || run.Phase == Phase.RunLost,
                60f, "the round never settled");

            Assert.IsTrue(director.LastSettle.HasValue, "settle card telemetry missing");
            foreach (Ticket t in run.Tickets)
                Assert.AreNotEqual(TicketState.Open, t.State, "every ticket must reach a terminal state");
            Assert.Greater(run.Bank, 0.0, "bank went non-positive");

            if (run.Phase == Phase.Shop)
            {
                int round = run.Round;
                director.ExitShop();
                Assert.AreEqual(Phase.Betting, run.Phase, "leaving the shop opens the next round");
                Assert.AreEqual(round + 1, run.Round);
                Assert.AreEqual(0, run.Tickets.Count, "the new round starts with a clean slate");
            }
        }

        [UnityTest]
        public IEnumerator StandingMidSweatFreezesTheEventCursor()
        {
            yield return LoadRoom();

            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            var screen = UnityEngine.Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = UnityEngine.Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director);
            Assert.IsNotNull(screen);
            Assert.IsNotNull(couch);

            // Moderate pacing so events land ~70-180ms apart in real time - a broken pause would
            // measurably advance the cursor inside the freeze windows below at any frame rate.
            screen.TimeScaleOverride = 0.15f;
            couch.transitionDuration = 0.01f;

            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");
            Run run = director.Run;

            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            director.LockRound();

            // Before sitting, the sweat must not advance at all (card and steps gate on seated).
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

        [UnityTest]
        public IEnumerator ZeroTicketLockSettlesOnTheSpot()
        {
            yield return LoadRoom();

            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            Assert.IsNotNull(director);
            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");

            director.LockRound(); // no tickets: the director settles without TV ceremony

            // Starting bank 500 >= round-1 target 400, so a no-bet round always clears into the shop.
            Assert.AreEqual(Phase.Shop, director.Run.Phase, "no-bet round should settle straight to Shop");
            Assert.IsTrue(director.LastSettle.HasValue);
            Assert.IsTrue(director.LastSettle.Value.TargetMet);
        }

        // ---- helpers ----

        private static int UnusedMatchup(Run run, IReadOnlyList<Pick> used)
        {
            for (int i = 0; i < run.CurrentSlate.Matchups.Count; i++)
            {
                bool taken = false;
                foreach (Pick p in used)
                    if (p.MatchupIndex == i) { taken = true; break; }
                if (!taken) return i;
            }
            throw new InvalidOperationException("no unused matchup on the slate");
        }

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
