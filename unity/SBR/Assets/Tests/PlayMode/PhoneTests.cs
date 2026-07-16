using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SBR.Tests.PlayMode
{
    /// <summary>
    /// M5 PlayMode: the phone alone owns read state, both desk prompts survive construction, and the
    /// shared camera claim is closed across the two glide race windows. Wall-clock waits preserve
    /// M3's batch-mode lesson; the adapter test drives a real deterministic float end to end.
    /// </summary>
    public class PhoneTests
    {
        [UnityTest]
        public IEnumerator Laptop_focus_does_not_clear_unread_but_phone_focus_does()
        {
            yield return LoadRoom();

            BookieFeed feed = UnityEngine.Object.FindAnyObjectByType<BookieFeed>();
            FindFocuses(out DeskFocus laptop, out DeskFocus phone);
            Assert.IsNotNull(feed);
            Assert.IsNotNull(laptop);
            Assert.IsNotNull(phone);
            Assert.AreEqual("Use laptop", laptop.Prompt);
            Assert.AreEqual("Check phone", phone.Prompt);

            laptop.transitionDuration = 0.01f;
            phone.transitionDuration = 0.01f;
            yield return WaitUntil(() => feed.UnreadCount > 0, 10f, "welcome never became unread");

            laptop.OnInteract(null);
            yield return WaitUntil(() => DeskFocus.Active == laptop, 10f, "laptop never claimed focus");
            yield return WaitRealtime(0.05f);
            Assert.Greater(feed.UnreadCount, 0, "laptop focus must not read the phone");

            laptop.OnInteract(null);
            yield return WaitUntil(() => DeskFocus.Active == null, 10f, "laptop never released focus");
            phone.OnInteract(null);
            yield return WaitUntil(() => DeskFocus.Active == phone, 10f, "phone never claimed focus");
            yield return WaitUntil(() => feed.UnreadCount == 0, 10f, "phone focus never marked the thread read");
        }

        [UnityTest]
        public IEnumerator Second_focus_is_rejected_during_focus_in_and_focus_out()
        {
            yield return LoadRoom();
            FindFocuses(out DeskFocus laptop, out DeskFocus phone);
            Assert.IsNotNull(laptop);
            Assert.IsNotNull(phone);
            laptop.transitionDuration = 0.20f;
            phone.transitionDuration = 0.01f;

            laptop.OnInteract(null);
            phone.OnInteract(null); // the old completion-time claim let this second glide start
            Assert.AreSame(laptop, DeskFocus.Active, "focus ownership must be claimed before glide-in");
            Assert.AreEqual("Check phone", phone.Prompt, "rejected phone must remain idle");
            yield return WaitRealtime(0.05f);
            Assert.AreSame(laptop, DeskFocus.Active);
            yield return WaitRealtime(0.25f);

            laptop.OnInteract(null);
            phone.OnInteract(null); // claim remains held until the glide home completes
            Assert.AreSame(laptop, DeskFocus.Active, "focus ownership must survive glide-out");
            Assert.AreEqual("Check phone", phone.Prompt, "rejected phone must remain idle");
            yield return WaitUntil(() => DeskFocus.Active == null, 10f, "focus-out never released ownership");

            phone.OnInteract(null);
            yield return WaitUntil(() => DeskFocus.Active == phone, 10f, "phone could not claim after release");
            // Active is claimed BEFORE the glide (that is the contract under test), so give the
            // 0.01s glide-in a beat to reach Focused - an interact mid-transition is ignored.
            yield return WaitRealtime(0.05f);
            phone.OnInteract(null);
            yield return WaitUntil(() => DeskFocus.Active == null, 10f, "phone did not release cleanly");
        }

        [UnityTest]
        public IEnumerator Disabling_focus_mid_glide_restores_camera_controller_and_cursor()
        {
            yield return LoadRoom();
            FindFocuses(out _, out DeskFocus phone);
            var controller = UnityEngine.Object.FindAnyObjectByType<FirstPersonController>();
            Assert.IsNotNull(phone);
            Assert.IsNotNull(controller);

            phone.transitionDuration = 1f;
            Vector3 home = controller.cameraTransform.position;
            float standingFov = controller.cameraTransform.GetComponent<Camera>().fieldOfView;

            phone.OnInteract(null);
            yield return WaitRealtime(0.08f);
            Assert.AreSame(phone, DeskFocus.Active);
            Assert.AreEqual(FirstPersonController.LookMode.External, controller.Mode);

            phone.enabled = false;
            Assert.AreEqual(home.x, controller.cameraTransform.position.x, 0.001f);
            Assert.AreEqual(home.y, controller.cameraTransform.position.y, 0.001f);
            Assert.AreEqual(home.z, controller.cameraTransform.position.z, 0.001f);
            Assert.AreEqual(standingFov, controller.cameraTransform.GetComponent<Camera>().fieldOfView, 0.001f);
            Assert.AreEqual(FirstPersonController.LookMode.Normal, controller.Mode);
            Assert.IsFalse(controller.MovementLocked);
            // CursorFree is the code-owned cursor signal; raw Cursor.lockState is not honored in
            // headless batch mode, so asserting it there fails on the environment, not the code.
            Assert.IsFalse(controller.CursorFree);
            Assert.IsNull(DeskFocus.Active);
        }

        [UnityTest]
        public IEnumerator Real_adapter_walks_a_no_bet_run_to_the_cliff_and_the_collection_text()
        {
            yield return LoadRoom();
            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            var feed = UnityEngine.Object.FindAnyObjectByType<BookieFeed>();
            Assert.IsNotNull(director);
            Assert.IsNotNull(feed);
            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");

            // Fully deterministic for ANY seed: no bets, bank 350 against payments 60+70+85+105 =
            // 320, so round 5's 155 collects with 30 in hand. Zero-ticket locks settle inline.
            director.StartNewRun("M5-CLIFF");
            Run run = director.Run;
            while (run.Phase != Phase.RunLost)
            {
                yield return null; // a frame per betting phase, so the feed observes each demand
                director.LockRound();
                if (run.Phase == Phase.Shop) director.ExitShop();
            }

            Assert.AreEqual(5, run.LastSettlement!.Value.Round, "the cliff should collect at round 5");

            yield return WaitUntil(() => HasKind(feed.Messages, BookieMessageKind.COLLECTION),
                10f, "real BookieFeed never observed the collection");

            BookieMessage collection = FindKind(feed.Messages, BookieMessageKind.COLLECTION);
            Assert.AreEqual(5, collection.Round, "adapter must stamp the report round");

            // The round-5 demand (155 vs 105 clears the ≥1.45× cliff ratio) texts at its open.
            BookieMessage demand = FindKind(feed.Messages, BookieMessageKind.CLIFF_DEMAND);
            Assert.AreEqual(5, demand.Round);
            StringAssert.Contains(Money(155), demand.Text);
        }

        private static void FindFocuses(out DeskFocus laptop, out DeskFocus phone)
        {
            laptop = null;
            phone = null;
            foreach (DeskFocus focus in UnityEngine.Object.FindObjectsByType<DeskFocus>())
            {
                if (focus.prompt == "Use laptop") laptop = focus;
                if (focus.prompt == "Check phone") phone = focus;
            }
        }

        private static bool HasKind(IReadOnlyList<BookieMessage> messages, BookieMessageKind kind)
        {
            foreach (BookieMessage message in messages)
                if (message.Kind == kind) return true;
            return false;
        }

        private static BookieMessage FindKind(IReadOnlyList<BookieMessage> messages, BookieMessageKind kind)
        {
            foreach (BookieMessage message in messages)
                if (message.Kind == kind) return message;
            Assert.Fail($"message kind {kind} missing");
            return default;
        }

        private static string Money(double value)
        {
            long rounded = (long)Math.Round(value, MidpointRounding.AwayFromZero);
            return "$" + rounded.ToString("N0", CultureInfo.InvariantCulture);
        }

        private static IEnumerator LoadRoom()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("Room", LoadSceneMode.Single);
            Assert.IsNotNull(load, "Room scene not in build settings - run SBR.GrayboxRoomBuilder.Build first.");
            while (!load.isDone) yield return null;
        }

        private static IEnumerator WaitUntil(Func<bool> condition, float maxSeconds, string failMessage)
        {
            float start = Time.realtimeSinceStartup;
            while (!condition())
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
