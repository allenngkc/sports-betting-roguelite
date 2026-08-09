using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace SBR.Tests.PlayMode
{
    public class SureThingMyBetsTests
    {
        [UnityTest, Order(1)]
        public IEnumerator MyBets_walks_empty_pending_live_green_and_dead_as_a_read_only_mirror()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            laptop.Os.OpenSportsbook(SportsbookApp.Tab.MyBets);
            yield return WaitForRebuild();

            Transform board = Required(App(laptop), "MyBetsBoard");
            Transform margin = Required(App(laptop), "MyBetsMargin");
            AssertRect(board as RectTransform, 700f, 530f, "MY BETS board");
            AssertRect(margin as RectTransform, 324f, 530f, "MY BETS margin");
            Assert.IsNotNull(Required(board, "MyBetsEmpty"));
            Assert.IsNotNull(Required(margin, "MirrorMarginEmpty"));
            AssertNoMutatingButtons(board, margin);

            Ticket ticket = PlaceTwoLegTicket(laptop);
            RevealedView view = laptop.tv.RevealedView;
            InvokeView(view, "Reset", laptop.director.Run, ticket, 0);
            yield return WaitForRebuild();

            Transform mirrorTicket = Required(Required(App(laptop), "MyBetsBoard"), "MirrorTicket0");
            Transform mirrorLeg0 = Required(mirrorTicket, "MirrorLeg0");
            Transform mirrorLeg1 = Required(mirrorTicket, "MirrorLeg1");
            Assert.AreEqual("PENDING", TextOf(Required(mirrorLeg0, "LegState")));
            Assert.AreEqual("PENDING", TextOf(Required(mirrorLeg1, "LegState")));
            StringAssert.Contains("RIDING", TextOf(Required(mirrorTicket, "TicketTitle")));

            // S64: the identity is the shared formatter's output, not a hand-built string. Asserted
            // against LaptopUi.TicketIdentity itself rather than against the literal "TICKET 01", so
            // the mirror and the ledger cannot drift apart again — which is exactly what happened
            // when S62 landed on the formatter and this screen never called it.
            StringAssert.Contains(LaptopUi.TicketIdentity(null, 0, 0, withRound: false),
                TextOf(Required(mirrorTicket, "TicketTitle")),
                "S64: MY BETS prints the same ticket identity every other screen prints");

            InvokeView(view, "BeginLeg", 0, ticket.Legs[0]);
            yield return WaitForRebuild();
            mirrorTicket = Required(Required(App(laptop), "MyBetsBoard"), "MirrorTicket0");
            mirrorLeg0 = Required(mirrorTicket, "MirrorLeg0");
            mirrorLeg1 = Required(mirrorTicket, "MirrorLeg1");
            Assert.AreEqual("LIVE", TextOf(Required(mirrorLeg0, "LegState")));
            Assert.AreEqual("PENDING", TextOf(Required(mirrorLeg1, "LegState")));

            InvokeView(view, "ResolveLeg", 0, LegGrade.Won);
            yield return WaitForRebuild();
            mirrorTicket = Required(Required(App(laptop), "MyBetsBoard"), "MirrorTicket0");
            mirrorLeg0 = Required(mirrorTicket, "MirrorLeg0");
            Assert.AreEqual("GREEN", TextOf(Required(mirrorLeg0, "LegState")));
            Image green = DecorativeInk(mirrorLeg0, "GreenRing", "ring-price-");
            // The ring is sized off the actual "GREEN" text bounds (not a fixed box) so its pen
            // stroke overshoots the word instead of crossing it — assert against that same formula
            // rather than a magic number that would silently drift from the render code.
            TMP_Text greenState = Required(mirrorLeg0, "LegState").GetComponent<TMP_Text>();
            (Vector2 expectedPosition, Vector2 expectedSize) = SportsbookApp.InkRingGeometry(greenState);
            AssertRect(green.rectTransform, expectedSize.x, expectedSize.y, "GREEN price ring");
            Assert.AreEqual(expectedPosition, green.rectTransform.anchoredPosition,
                "GREEN price ring position must overshoot the state text by a fixed 8px");
            string greenVariant = green.sprite.name;

            InvokeView(view, "BeginLeg", 1, ticket.Legs[1]);
            InvokeView(view, "ResolveLeg", 1, LegGrade.Lost);
            yield return WaitForRebuild();
            board = Required(App(laptop), "MyBetsBoard");
            margin = Required(App(laptop), "MyBetsMargin");
            mirrorTicket = Required(board, "MirrorTicket0");
            mirrorLeg0 = Required(mirrorTicket, "MirrorLeg0");
            mirrorLeg1 = Required(mirrorTicket, "MirrorLeg1");
            Assert.AreEqual("GREEN", TextOf(Required(mirrorLeg0, "LegState")));
            Assert.AreEqual("DEAD", TextOf(Required(mirrorLeg1, "LegState")));
            StringAssert.Contains("DEAD", TextOf(Required(mirrorTicket, "TicketTitle")));
            green = DecorativeInk(mirrorLeg0, "GreenRing", "ring-price-");
            Image strike = DecorativeInk(mirrorLeg1, "DeadStrike", "strike-");
            // Was 112x46 — the strike sprite's own native size, fixed regardless of what it struck
            // through. "DEAD" is about 38px wide, so the strike ran roughly 70px past the word and
            // out toward the panel edge. It now derives from the text via InkRingGeometry, the same
            // way the GREEN ring beside it always did. Confirmed against the rendered capture: the
            // strike sits across the word with a short overshoot each side, as a pen would leave it.
            AssertRect(strike.rectTransform, 56f, 34f, "DEAD strike");
            string strikeVariant = strike.sprite.name;
            AssertNoMutatingButtons(board, margin);

            laptop.Os.OpenSportsbook(SportsbookApp.Tab.MyBets);
            yield return WaitForRebuild();
            mirrorTicket = Required(Required(App(laptop), "MyBetsBoard"), "MirrorTicket0");
            Assert.AreEqual(greenVariant,
                DecorativeInk(Required(mirrorTicket, "MirrorLeg0"), "GreenRing", "ring-price-").sprite.name,
                "GREEN ring identity must be deterministic across a rebuild");
            Assert.AreEqual(strikeVariant,
                DecorativeInk(Required(mirrorTicket, "MirrorLeg1"), "DeadStrike", "strike-").sprite.name,
                "DEAD strike identity must be deterministic across a rebuild");
            Assert.AreEqual(TicketState.Open, ticket.State,
                "presentation-only grades must not mutate the engine ticket");
        }

        [UnityTest, Order(2)]
        public IEnumerator MyBets_renders_only_revealed_state_and_leaks_no_score_clock_probability_or_engine_outcome()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            TvSweatScreen screen = laptop.tv;
            screen.ForceSeated(false);
            Ticket ticket = PlaceTwoLegTicket(laptop);
            laptop.director.LockRound();
            Assert.AreEqual(Phase.Sweat, laptop.director.Run.Phase);

            RevealedView view = screen.RevealedView;
            yield return WaitUntil(() => view.HasTicket, 10f,
                "TV-owned RevealedView never reached its waiting state");
            for (int i = 0; i < ticket.Legs.Count; i++)
                Assert.AreNotEqual(LegState.Pending, ticket.Legs[i].State,
                    $"engine leg {i} must carry baked truth for this leakage test");

            const string scoreSentinel = "SCORE-SENTINEL-9-8";
            const string clockSentinel = "CLOCK-SENTINEL-88:88";
            const float probabilitySentinel = 0.371234f;
            InvokeView(view, "BeginLeg", 0, ticket.Legs[0]);
            InvokeView(view, "SetScore", scoreSentinel);
            InvokeView(view, "SetClock", clockSentinel);
            InvokeView(view, "SetProbability", probabilitySentinel);
            yield return WaitForRebuild();

            Transform board = Required(App(laptop), "MyBetsBoard");
            Transform margin = Required(App(laptop), "MyBetsMargin");
            Transform mirrorTicket = Required(board, "MirrorTicket0");
            Transform mirrorLeg0 = Required(mirrorTicket, "MirrorLeg0");
            Transform mirrorLeg1 = Required(mirrorTicket, "MirrorLeg1");
            Assert.AreEqual("LIVE", TextOf(Required(mirrorLeg0, "LegState")));
            Assert.AreEqual("PENDING", TextOf(Required(mirrorLeg1, "LegState")));
            StringAssert.Contains("RIDING", TextOf(Required(mirrorTicket, "TicketTitle")));
            AssertNoMutatingButtons(board, margin);

            string rendered = AllText(board, margin);
            StringAssert.DoesNotContain(scoreSentinel, rendered);
            StringAssert.DoesNotContain(clockSentinel, rendered);
            StringAssert.DoesNotContain(
                probabilitySentinel.ToString("R", CultureInfo.InvariantCulture), rendered);
            StringAssert.DoesNotContain("37.1234%", rendered);
            StringAssert.DoesNotContain("37%", rendered);
            StringAssert.DoesNotContain("GREEN", rendered,
                "baked winning truth must not outrun RevealedView");
            StringAssert.DoesNotContain("DEAD", rendered,
                "baked losing truth must not outrun RevealedView");

            for (int i = 0; i < ticket.Legs.Count; i++)
            {
                string hiddenOutcome = ticket.Legs[i].State == LegState.Won ? "GREEN" : "DEAD";
                Transform row = Required(mirrorTicket, "MirrorLeg" + i);
                Assert.AreNotEqual(hiddenOutcome, TextOf(Required(row, "LegState")),
                    $"engine outcome for leg {i} must remain hidden until RevealedView resolves it");
            }
        }

        private static Ticket PlaceTwoLegTicket(LaptopScreen laptop)
        {
            Assert.IsTrue(laptop.Slip.Toggle(0, Side.Away));
            Assert.IsTrue(laptop.Slip.Toggle(1, Side.Home));
            Assert.AreEqual(2, laptop.Slip.Picks.Count);
            Ticket ticket = laptop.Slip.Place();
            Assert.AreEqual(2, ticket.Legs.Count);
            return ticket;
        }

        private static void InvokeView(RevealedView view, string methodName, params object[] args)
        {
            MethodInfo method = typeof(RevealedView).GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"RevealedView test seam '{methodName}' missing");
            try
            {
                method.Invoke(view, args);
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException ?? exception;
            }
        }

        private static Image DecorativeInk(Transform row, string name, string prefix)
        {
            Transform node = Required(row, name);
            Image image = node.GetComponent<Image>();
            Assert.IsNotNull(image, $"{name} must be an Image");
            Assert.IsNotNull(image.sprite, $"{name} sprite missing");
            StringAssert.StartsWith(prefix, image.sprite.name);
            Assert.IsFalse(image.raycastTarget, $"{name} must not intercept input");
            return image;
        }

        private static void AssertNoMutatingButtons(params Transform[] roots)
        {
            foreach (Transform root in roots)
                Assert.Zero(root.GetComponentsInChildren<Button>(true).Length,
                    $"{root.name} must remain read-only");
        }

        private static IEnumerator Boot()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("Room", LoadSceneMode.Single);
            Assert.IsNotNull(load, "Room scene is not available");
            while (!load.isDone) yield return null;

            LaptopScreen laptop = Laptop();
            float start = Time.realtimeSinceStartup;
            while (laptop.director == null || laptop.director.Run == null || laptop.Os.OnDesktop)
            {
                if (Time.realtimeSinceStartup - start > 10f)
                {
                    Assert.Fail("SureThing did not reach the betting lobby within 10 seconds");
                    yield break;
                }
                yield return null;
            }
            yield return null;
        }

        private static IEnumerator WaitForRebuild()
        {
            yield return null;
            yield return null;
        }

        private static IEnumerator WaitUntil(Func<bool> condition, float seconds, string failure)
        {
            float start = Time.realtimeSinceStartup;
            while (!condition())
            {
                if (Time.realtimeSinceStartup - start > seconds)
                {
                    Assert.Fail($"{failure} (waited {seconds:0.#}s)");
                    yield break;
                }
                yield return null;
            }
            yield return null;
        }

        private static LaptopScreen Laptop()
        {
            LaptopScreen laptop = UnityEngine.Object.FindAnyObjectByType<LaptopScreen>();
            Assert.IsNotNull(laptop, "LaptopScreen missing");
            Assert.IsNotNull(laptop.tv, "Laptop TV reference missing");
            return laptop;
        }

        private static Transform App(LaptopScreen laptop) => Required(laptop.transform, "App");

        private static Transform Required(Transform root, string name)
        {
            Transform found = Find(root, name);
            Assert.IsNotNull(found, $"Required named UI node '{name}' missing beneath '{root.name}'");
            return found;
        }

        private static Transform Find(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = Find(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        private static string TextOf(Transform node)
        {
            TMP_Text text = node.GetComponent<TMP_Text>();
            if (text == null) text = node.GetComponentInChildren<TMP_Text>();
            Assert.IsNotNull(text, $"{node.name} has no readable text");
            return text.text;
        }

        private static string AllText(params Transform[] roots)
        {
            var content = new List<string>();
            foreach (Transform root in roots)
                foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                    content.Add(text.text);
            return string.Join("\n", content);
        }

        private static void AssertRect(RectTransform rect, float width, float height, string label)
        {
            Assert.IsNotNull(rect, $"{label} RectTransform missing");
            Assert.AreEqual(width, rect.sizeDelta.x, 0.01f, $"{label} width");
            Assert.AreEqual(height, rect.sizeDelta.y, 0.01f, $"{label} height");
        }
    }
}
