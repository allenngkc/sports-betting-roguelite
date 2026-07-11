using System.Collections;
using NUnit.Framework;
using SBR.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SBR.Tests.PlayMode
{
    /// <summary>
    /// Room smoke test (M2, updated for M3): Room.unity loads, the player rig / HUD exist,
    /// and after M3 exactly 3 interactables remain (couch, laptop, phone - the TV became the
    /// live sweat surface, no longer interactable). The M3 TV trio (TvSweatScreen, TvLight,
    /// DemoRunDriver) is present; 60 frames tick without exceptions or error logs (the runner
    /// fails on unexpected Debug.LogError automatically), and the camera stays inside the room.
    /// Requires the scene in EditorBuildSettings - run SBR.GrayboxRoomBuilder.Build first.
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
                "expected exactly 3 interactables after M3: couch, laptop, phone (the TV is no longer interactable)");
            Assert.AreEqual(1, CountOfType<SitSpot>(interactables), "expected exactly one SitSpot");
            Assert.AreEqual(2, CountOfType<ScreenStub>(interactables),
                "expected laptop and phone ScreenStubs (the TV's went away in M3)");

            // M3 TV trio.
            Assert.IsNotNull(Object.FindAnyObjectByType<TvSweatScreen>(), "TvSweatScreen missing");
            Assert.IsNotNull(Object.FindAnyObjectByType<TvLight>(), "TvLight missing");
            Assert.IsNotNull(Object.FindAnyObjectByType<DemoRunDriver>(), "DemoRunDriver missing");

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
