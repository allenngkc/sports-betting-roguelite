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
    /// F_0.3.0: the laptop OS boots inside the room, apps and tabs switch, phase defaults
    /// land where the plan says, and the causal mirror advances ONLY with the TV's own
    /// presentation — nothing revealed while the couch is empty, terminal truth only after
    /// the round settles. All waits are wall-clock (batch runs unthrottled).
    /// </summary>
    public class LaptopOsTests
    {
        [UnityTest]
        public IEnumerator Os_boots_switches_apps_and_keeps_chrome_over_every_tab()
        {
            yield return LoadRoom();
            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            var laptop = UnityEngine.Object.FindAnyObjectByType<LaptopScreen>();
            var tv = UnityEngine.Object.FindAnyObjectByType<TvSweatScreen>();
            Assert.IsNotNull(laptop, "LaptopScreen missing");
            Assert.IsNotNull(laptop.tv, "the laptop's TV reference must self-heal at runtime");
            Assert.AreSame(tv, laptop.tv, "one TV, one mirror source");
            yield return WaitUntil(() => director.Run != null, 10f, "no run");
            yield return null; // one OS tick

            // Phase default: a fresh run opens in Betting → SureThing LOBBY.
            Assert.IsFalse(laptop.Os.OnDesktop, "Betting boots straight into the book");
            Assert.AreEqual(SportsbookApp.Tab.Lobby, laptop.Os.CurrentTab);

            // Every tab keeps the chrome header visible (Sol finding 1 regression pin):
            // the header must exist and no full-screen panel may follow it as a sibling.
            foreach (SportsbookApp.Tab tab in new[]
                { SportsbookApp.Tab.Lobby, SportsbookApp.Tab.MyBets, SportsbookApp.Tab.Rewards })
            {
                laptop.Os.OpenSportsbook(tab);
                yield return null;
                Transform app = FindDeep(laptop.transform, "App");
                Assert.IsNotNull(app, $"{tab}: app root missing");
                Transform chrome = app.Find("Chrome");
                Assert.IsNotNull(chrome, $"{tab}: the chrome header must render on every tab");
                Assert.IsNull(app.Find("MirrorPanel"), $"{tab}: no chrome-burying panel may exist");
            }

            // Desktop round-trip.
            laptop.Os.OpenDesktop();
            yield return null;
            Assert.IsTrue(laptop.Os.OnDesktop);
            Assert.IsNotNull(FindDeep(laptop.transform, "Wallpaper"), "the wallpaper child exists");
            laptop.Os.OpenSportsbook(SportsbookApp.Tab.Lobby);
        }

        [UnityTest]
        public IEnumerator Mirror_reveals_nothing_unseated_and_lands_engine_truth_at_settle()
        {
            yield return LoadRoom();
            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            var screen = UnityEngine.Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = UnityEngine.Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director);
            Assert.IsNotNull(screen);
            screen.TimeScaleOverride = 0.0001f;
            if (couch != null) couch.transitionDuration = 0.01f;
            yield return WaitUntil(() => director.Run != null, 10f, "no run");
            Run run = director.Run;

            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);

            // Nobody is seated: the TV cannot play, so the mirror must hold at pregame —
            // ticket visible (identity is lock-public), clock PRE, and NO leg resolved.
            yield return WaitUntil(() => screen.RevealedView.HasTicket, 10f, "pregame mirror never armed");
            yield return WaitRealtime(0.4f);
            RevealedView view = screen.RevealedView;
            Assert.AreEqual("PRE", view.ClockText, "unseated: the clock must still read PRE");
            double anchor = run.Tickets[0].Legs[0].TrueProb;
            Assert.AreEqual((float)anchor, view.WinProbability, 1e-4f,
                "unseated: the mirror's prob is the pregame anchor, nothing more");
            foreach (RevealedLeg leg in view.Tickets[0].Legs)
                Assert.IsTrue(leg.State == RevealedLegState.Pending || leg.State == RevealedLegState.Live,
                    "unseated: no outcome may be revealed anywhere");

            // Sit at a HUMAN-ish pace first: the hard causal interval (Sol) — during a
            // suspended dangerous scene the engine has already consumed the beat, but the
            // mirror's number must HOLD at the preceding payoff until the scene reveals.
            var laptop = UnityEngine.Object.FindAnyObjectByType<LaptopScreen>();
            screen.TimeScaleOverride = 0.2f;
            screen.ForceSeated(true);
            yield return WaitUntil(() => view.MarketSuspended, 30f, "no dangerous scene ever suspended");
            Assert.GreaterOrEqual(screen.EventsEmitted, 1, "suspension implies a consumed beat");
            Assert.AreEqual(SportsbookApp.Tab.MyBets, laptop.Os.CurrentTab,
                "the Sweat phase defaults the book to MY BETS");
            float heldProb = view.WinProbability;
            float w0 = Time.realtimeSinceStartup;
            while (view.MarketSuspended && Time.realtimeSinceStartup - w0 < 10f)
            {
                Assert.AreEqual(heldProb, view.WinProbability, 1e-5f,
                    "the mirror's number must hold through the suspended scene — the reveal owns it");
                yield return null;
            }
            Assert.IsFalse(view.MarketSuspended, "the payoff must eventually release the market");

            // Fast-forward the rest to settle, snapshotting the mirror on the way (it is
            // cleared the moment the TV leaves the sweat — finding 2).
            screen.TimeScaleOverride = 0.0001f;
            var lastStates = new Dictionary<int, RevealedTicketState>();
            float start = Time.realtimeSinceStartup;
            while (run.Phase == Phase.Sweat)
            {
                if (view.Tickets.Count > 0)
                    foreach (RevealedTicket rt in view.Tickets)
                        lastStates[rt.Index] = rt.State;
                if (Time.realtimeSinceStartup - start > 60f)
                {
                    Assert.Fail("the round never settled (waited 60s)");
                    yield break;
                }
                yield return null;
            }

            Assert.Greater(lastStates.Count, 0, "the mirror must have presented the ticket");
            for (int i = 0; i < run.Tickets.Count; i++)
            {
                if (!lastStates.TryGetValue(i, out RevealedTicketState mirrored)) continue;
                Ticket t = run.Tickets[i];
                bool matches =
                    (t.State == TicketState.CashedOut && mirrored == RevealedTicketState.CashedOut) ||
                    (t.State == TicketState.Lost && mirrored == RevealedTicketState.Lost) ||
                    (t.State != TicketState.Lost && t.State != TicketState.CashedOut
                        && mirrored == RevealedTicketState.Won);
                Assert.IsTrue(matches, $"mirror state {mirrored} must match engine {t.State}");
            }

            // And the finding-2 pin itself: once the TV idles into the shop, the mirror is empty.
            yield return null;
            yield return null;
            Assert.IsFalse(view.HasTicket, "leaving the sweat must clear the mirror");
            if (run.Phase == Phase.Shop)
                Assert.AreEqual(SportsbookApp.Tab.Rewards, laptop.Os.CurrentTab,
                    "the Shop phase defaults the book to REWARDS");
        }

        // ---- helpers ----

        private static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform hit = FindDeep(root.GetChild(i), name);
                if (hit != null) return hit;
            }
            return null;
        }

        private static IEnumerator LoadRoom()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("Room", LoadSceneMode.Single);
            Assert.IsNotNull(load, "Room scene not in build settings - run SBR.GrayboxRoomBuilder.Build first.");
            while (!load.isDone) yield return null;
        }

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
