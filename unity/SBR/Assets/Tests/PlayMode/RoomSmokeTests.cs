using System.Collections;
using NUnit.Framework;
using SBR.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SBR.Tests.PlayMode
{
    /// <summary>
    /// Room smoke test (M2, updated for M3/M4): Room.unity loads, the player rig / HUD exist,
    /// and exactly 3 interactables remain — the couch SitSpot, the laptop DeskFocus (M4: the
    /// book replaced its ScreenStub) and the phone's ScreenStub. The M4 surfaces (TvSweatScreen,
    /// TvLight, RunDirector, LaptopScreen) and the EventSystem are present; 60 frames tick
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
            Assert.AreEqual(1, CountOfType<DeskFocus>(interactables),
                "expected the laptop's DeskFocus (M4 replaced its ScreenStub)");
            Assert.AreEqual(1, CountOfType<ScreenStub>(interactables),
                "expected only the phone's ScreenStub after M4");

            // M4 surfaces.
            Assert.IsNotNull(Object.FindAnyObjectByType<TvSweatScreen>(), "TvSweatScreen missing");
            Assert.IsNotNull(Object.FindAnyObjectByType<TvLight>(), "TvLight missing");
            Assert.IsNotNull(Object.FindAnyObjectByType<RunDirector>(), "RunDirector missing");
            Assert.IsNotNull(Object.FindAnyObjectByType<LaptopScreen>(), "LaptopScreen missing");
            Assert.IsNotNull(Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>(),
                "EventSystem missing (the laptop UI cannot take clicks)");

            var hud = Object.FindAnyObjectByType<InteractionHud>();
            Assert.IsNotNull(hud, "InteractionHud missing");
            Assert.IsNotNull(hud.GetComponentInChildren<Canvas>(), "HUD canvas was not built");

            Camera cam = Camera.main;
            Assert.IsNotNull(cam, "MainCamera missing");
            AssertInsideRoom(cam.transform.position, "at load");

            for (int i = 0; i < 60; i++)
                yield return null;

            AssertInsideRoom(cam.transform.position, "after 60 simulated frames");
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
