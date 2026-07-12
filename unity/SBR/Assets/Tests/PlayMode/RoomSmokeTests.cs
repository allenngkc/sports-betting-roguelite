using System;
using System.Collections;
using NUnit.Framework;
using SBR.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object; // using System pulls in System.Object; keep the Unity one

namespace SBR.Tests.PlayMode
{
    /// <summary>
    /// Room smoke test (M2, updated through M5): Room.unity loads, the player rig / HUD exist,
    /// and exactly 3 interactables remain — the couch SitSpot plus laptop and phone DeskFocus.
    /// The live TV, laptop book, and bookie phone surfaces are present; a wall-clock soak ticks
    /// without exceptions or error logs (the runner fails on unexpected Debug.LogError
    /// automatically) — which also exercises the laptop's betslip page build — and the camera
    /// stays inside the room. Requires the scene in EditorBuildSettings (GrayboxRoomBuilder.Build).
    /// </summary>
    public class RoomSmokeTests
    {
        private const float RoomHalfWidth = 1.3f;  // interior X half-extent
        private const float RoomHalfLength = 2.0f; // interior Z half-extent
        private const float RoomHeight = 2.3f;

        [UnityTest]
        public IEnumerator Room_LoadsWiredAndSurvivesSixtyFrames()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("Room", LoadSceneMode.Single);
            Assert.IsNotNull(load,
                "Room scene not in build settings - run SBR.GrayboxRoomBuilder.Build first.");
            while (!load.isDone)
                yield return null;

            var controller = Object.FindAnyObjectByType<FirstPersonController>();
            Assert.IsNotNull(controller, "player rig (FirstPersonController) missing");
            Assert.IsNotNull(controller.GetComponent<CharacterController>(),
                "CharacterController missing on player rig");
            Assert.IsNotNull(Object.FindAnyObjectByType<PlayerInteractor>(),
                "PlayerInteractor missing");

            Interactable[] interactables = Object.FindObjectsByType<Interactable>();
            Assert.AreEqual(3, interactables.Length,
                "expected exactly 3 interactables: couch, laptop, phone (the TV is not interactable)");
            Assert.AreEqual(1, CountOfType<SitSpot>(interactables), "expected exactly one SitSpot");
            Assert.AreEqual(2, CountOfType<DeskFocus>(interactables),
                "expected laptop and phone DeskFocus components");
            Assert.AreEqual(0, CountComponentsNamed("ScreenStub"),
                "M5 deletes the final ScreenStub");

            // M4 surfaces.
            Assert.IsNotNull(Object.FindAnyObjectByType<TvSweatScreen>(), "TvSweatScreen missing");
            Assert.IsNotNull(Object.FindAnyObjectByType<TvLight>(), "TvLight missing");
            Assert.IsNotNull(Object.FindAnyObjectByType<RunDirector>(), "RunDirector missing");
            Assert.IsNotNull(Object.FindAnyObjectByType<LaptopScreen>(), "LaptopScreen missing");
            var phone = Object.FindAnyObjectByType<PhoneScreen>();
            var feed = Object.FindAnyObjectByType<BookieFeed>();
            Assert.IsNotNull(phone, "PhoneScreen missing");
            Assert.IsNotNull(feed, "BookieFeed missing");
            Assert.IsNotNull(Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>(),
                "EventSystem missing (the laptop UI cannot take clicks)");

            var hud = Object.FindAnyObjectByType<InteractionHud>();
            Assert.IsNotNull(hud, "InteractionHud missing");
            Assert.IsNotNull(hud.GetComponentInChildren<Canvas>(), "HUD canvas was not built");

            Camera cam = Camera.main;
            Assert.IsNotNull(cam, "MainCamera missing");
            AssertInsideRoom(cam.transform.position, "at load");

            yield return WaitUntil(() => phone.RenderedMessageCount > 0, 10f,
                "phone never rendered the welcome text");
            StringAssert.Contains("ROUND-1", phone.RenderedText);
            yield return WaitRealtime(0.25f);

            AssertInsideRoom(cam.transform.position, "after wall-clock soak");
        }

        private static int CountOfType<T>(Interactable[] all)
        {
            int count = 0;
            foreach (Interactable item in all)
            {
                if (item is T)
                    count++;
            }
            return count;
        }

        private static int CountComponentsNamed(string typeName)
        {
            int count = 0;
            foreach (MonoBehaviour behaviour in Object.FindObjectsByType<MonoBehaviour>())
                if (behaviour.GetType().Name == typeName) count++;
            return count;
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

        private static void AssertInsideRoom(Vector3 position, string when)
        {
            Assert.That(Mathf.Abs(position.x), Is.LessThan(RoomHalfWidth),
                $"camera x={position.x} outside the room {when}");
            Assert.That(Mathf.Abs(position.z), Is.LessThan(RoomHalfLength),
                $"camera z={position.z} outside the room {when}");
            Assert.That(position.y, Is.GreaterThan(0f).And.LessThan(RoomHeight),
                $"camera y={position.y} outside the room {when}");
        }
    }
}
