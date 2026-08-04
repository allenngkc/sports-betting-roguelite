using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace SBR.Tests.PlayMode
{
    /// <summary>
    /// Test-only production UGUI reference capture. The fixture deliberately drives the same named
    /// controls and presentation seams as the behavioral PlayMode suite, then renders both a
    /// canvas-aligned reference and the real Main Camera at the laptop's authored focus pose.
    ///
    /// Eight states are captured across two UnityTests. The first continues the single-run,
    /// ticket-carrying flow through six states (the original five plus the shared Ledger/Old
    /// Slips screen reached from the tray). The second boots a fresh run to reach REWARDS —
    /// which requires the deterministic zero-ticket lock seam SureThingRewardsTests.EnterShop
    /// uses, and so cannot share the first run's already-placed ticket — and from there also
    /// reaches the same Ledger/Old Slips screen via its other entry point, the desktop icon.
    /// </summary>
    public class SureThingVisualCaptureTests
    {
        private const int FlatWidth = 1024;
        private const int FlatHeight = 704;
        private const int AngledWidth = 1280;
        private const int AngledHeight = 720;
        private const int CaptureLayer = 30;

        [UnityTest]
        public IEnumerator Capture_six_truthful_surething_states_as_flat_and_angled_pngs()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            string outputDirectory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "..", "artifacts", "surething-ui"));
            Directory.CreateDirectory(outputDirectory);
            string runPrefix = DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            var capturedPaths = new List<string>();

            Assert.AreEqual(SportsbookApp.Tab.Lobby, laptop.Os.CurrentTab);
            Assert.IsNotNull(Required(App(laptop), "Board"));
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "01-form-lobby", capturedPaths);

            Invoke(Required(Required(App(laptop), "Matchup0"), "Details"));
            yield return WaitForRebuild();
            Assert.AreEqual(SportsbookApp.Tab.Detail, laptop.Os.CurrentTab);
            Button entryOffer = FirstNamedButton(
                Required(App(laptop), "MarketBody"), "Market");
            Invoke(entryOffer.transform);
            yield return WaitForRebuild();
            Assert.AreEqual(1, laptop.Slip.Picks.Count);
            Image wideRing = Required(
                Required(App(laptop), "MarketBody"), "WideBiroRing").GetComponent<Image>();
            Assert.IsNotNull(wideRing);
            Assert.IsNotNull(wideRing.sprite);
            StringAssert.StartsWith("ring-wide-", wideRing.sprite.name);

            // Instrumentation for the open wide-ring defect: the ring does not close on screen,
            // and every static explanation has now been falsified. The sprite downsamples to a
            // clean ellipse at exactly this rect size; its import settings are byte-identical to
            // ring-price, which renders correctly; the mesh is FullRect and mipmaps are off. So the
            // fault is in what the rect and its ancestors actually resolve to at runtime, which
            // cannot be read from source. Dump it rather than guess again — the last two
            // hypotheses were both wrong and one of them was acted on before it was tested.
            {
                RectTransform ringRect = wideRing.rectTransform;
                Sprite sprite = wideRing.sprite;
                var chain = new System.Text.StringBuilder();
                chain.AppendLine("=== WIDE RING DIAGNOSTIC ===");
                chain.AppendLine($"sprite={sprite.name} texture={sprite.texture.width}x{sprite.texture.height} " +
                    $"rect={sprite.rect} ppu={sprite.pixelsPerUnit} pivot={sprite.pivot} " +
                    $"border={sprite.border} packed={sprite.packed} tightCount={sprite.triangles.Length}");
                chain.AppendLine($"image type={wideRing.type} preserveAspect={wideRing.preserveAspect} " +
                    $"fillCenter={wideRing.fillCenter} useSpriteMesh={wideRing.useSpriteMesh} " +
                    $"pixelsPerUnitMultiplier={wideRing.pixelsPerUnitMultiplier}");
                Transform walk = ringRect;
                while (walk != null)
                {
                    var rt = walk as RectTransform;
                    chain.AppendLine(rt != null
                        ? $"[{walk.name}] size={rt.rect.width:F1}x{rt.rect.height:F1} " +
                          $"anchoredPos={rt.anchoredPosition} anchorMin={rt.anchorMin} " +
                          $"anchorMax={rt.anchorMax} pivot={rt.pivot} localScale={rt.localScale} " +
                          $"mask2D={(walk.GetComponent<RectMask2D>() != null)} " +
                          $"mask={(walk.GetComponent<Mask>() != null)}"
                        : $"[{walk.name}] (no RectTransform) localScale={walk.localScale}");
                    walk = walk.parent;
                }
                Debug.Log(chain.ToString());
            }
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "02-entry-selected-wide-ring", capturedPaths);

            // C17: no rebuild verdict on a state no capture shows. Every previously captured ENTRY
            // state is GOALS, which fits its body and therefore correctly renders NO position rail
            // — so the scrolling branch and the S27 rail had no photograph at all. PLAYERS is the
            // one shipped destination whose list overflows (14 scorer offers against a body that
            // shows ~7 at the 54px row), so this is the state that actually exercises S25's scroll
            // and S27's track-and-thumb.
            Invoke(Required(Required(App(laptop), "MarketDestinations"), "DetailTabPLAYERS"));
            yield return WaitForRebuild();
            Assert.IsNotNull(Required(App(laptop), "PositionRailTrack"),
                "PLAYERS overflows its body, so S27's rail must be present");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "02b-entry-players-scrolling-rail", capturedPaths);

            Invoke(Required(App(laptop), "BackToForm"));
            yield return WaitForRebuild();
            Assert.AreEqual(SportsbookApp.Tab.Lobby, laptop.Os.CurrentTab);
            Invoke(Required(Required(App(laptop), "Matchup1"), "AwayOdds"));
            yield return WaitForRebuild();
            Assert.AreEqual(2, laptop.Slip.Picks.Count);
            Invoke(Required(Required(App(laptop), "WorkingMargin"), "Place"));
            yield return WaitForRebuild();

            Transform margin = Required(App(laptop), "WorkingMargin");
            Assert.AreEqual(1, laptop.director.Run.Tickets.Count);
            Assert.AreEqual(0, laptop.Slip.Picks.Count);
            // E-07 moved staged receipts out of the margin into the 700px sheet, so this looks
            // beneath the app root rather than the margin (screens.jsx:50-57).
            Assert.IsNotNull(Required(
                Required(App(laptop), "StagedTickets"), "StagedTicket0"));
            Assert.IsTrue(Required(margin, "Lock").GetComponent<Button>().interactable);
            Assert.IsNull(Find(margin, "LockReason"));
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "03-staged-receipt-lock-enabled", capturedPaths);

            Invoke(Required(Required(App(laptop), "Matchup2"), "AwayOdds"));
            yield return WaitForRebuild();
            margin = Required(App(laptop), "WorkingMargin");
            Assert.AreEqual(1, laptop.Slip.Picks.Count);
            Assert.IsFalse(Required(margin, "Lock").GetComponent<Button>().interactable);
            Assert.AreEqual("PLACE OR CLEAR THIS WORKING SLIP",
                TextOf(Required(margin, "LockReason")));
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "04-working-mark-lock-disabled", capturedPaths);

            Invoke(Required(margin, "Remove0"));
            yield return WaitForRebuild();
            Assert.AreEqual(0, laptop.Slip.Picks.Count);

            Ticket ticket = laptop.director.Run.Tickets[0];
            Assert.AreEqual(2, ticket.Legs.Count);
            RevealedView view = laptop.tv.RevealedView;
            InvokeView(view, "Reset", laptop.director.Run, ticket, 0);
            yield return WaitForRebuild();
            InvokeView(view, "BeginLeg", 0, ticket.Legs[0]);
            InvokeView(view, "ResolveLeg", 0, LegGrade.Won);
            yield return WaitForRebuild();
            InvokeView(view, "BeginLeg", 1, ticket.Legs[1]);
            InvokeView(view, "ResolveLeg", 1, LegGrade.Lost);
            yield return WaitForRebuild();

            laptop.Os.OpenSportsbook(SportsbookApp.Tab.MyBets);
            yield return WaitForRebuild();
            Transform mirrorTicket = Required(
                Required(App(laptop), "MyBetsBoard"), "MirrorTicket0");
            Transform mirrorLeg0 = Required(mirrorTicket, "MirrorLeg0");
            Transform mirrorLeg1 = Required(mirrorTicket, "MirrorLeg1");
            Assert.AreEqual("GREEN", TextOf(Required(mirrorLeg0, "LegState")));
            Assert.AreEqual("DEAD", TextOf(Required(mirrorLeg1, "LegState")));
            Assert.IsNotNull(Required(mirrorLeg0, "GreenRing").GetComponent<Image>());
            Assert.IsNotNull(Required(mirrorLeg1, "DeadStrike").GetComponent<Image>());
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "05-my-bets-green-dead", capturedPaths);

            // The rail and tray are now built once by NotebookChrome and shared between the
            // sportsbook and the ledger screen (LaptopOs.OpenLedger routes to the same
            // App.OldSlips state the desktop's "Old Slips" icon does — OldSlipsApp.Render is
            // the one screen both entry points share). Reach it exactly the way
            // SureThingLedgerTests.OpenLedgerThroughTray does: through the tray, not the API.
            Invoke(Required(Required(App(laptop), "NotebookTray"), "Ledger"));
            yield return WaitForRebuild();
            Assert.IsNotNull(Required(App(laptop), "LedgerBoard"),
                "real tray navigation did not open LEDGER");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "06-ledger", capturedPaths);

            Assert.AreEqual(14, capturedPaths.Count, "seven states must emit paired captures");
            foreach (string path in capturedPaths)
            {
                Assert.IsTrue(File.Exists(path), $"capture missing: {path}");
                Assert.Greater(new FileInfo(path).Length, 0L, $"capture is empty: {path}");
            }
        }

        /// <summary>C17: no rebuild verdict on a state no capture shows. B1 rebuilt the margin's
        /// leg row to the kit's two-line MarginLeg, and the state that decides whether that rebuild
        /// is correct is a FULL slip — which no capture showed. Allen capped MaxLegs at 4
        /// (2026-08-02) to close the overflow; this photographs the cap actually holding. Fills to
        /// run.Config.MaxLegs rather than a literal 4, so raising the dial changes what is captured
        /// instead of quietly capturing a state that is no longer the maximum. Its own Boot(),
        /// because the other two capture flows each depend on their starting ticket count.</summary>
        [UnityTest]
        public IEnumerator Capture_the_working_margin_at_the_legal_maximum_leg_count()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            string outputDirectory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "..", "artifacts", "surething-ui"));
            Directory.CreateDirectory(outputDirectory);
            string runPrefix = DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            var capturedPaths = new List<string>();

            int maxLegs = laptop.director.Run.Config.MaxLegs;
            for (int i = 0; i < maxLegs; i++)
            {
                Invoke(Required(Required(App(laptop), "Matchup" + i), "AwayOdds"));
                yield return WaitForRebuild();
            }
            Assert.AreEqual(maxLegs, laptop.Slip.Picks.Count,
                "the captured state must actually be a full slip");

            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "09-margin-max-legs", capturedPaths);

            Assert.AreEqual(2, capturedPaths.Count, "one state must emit paired captures");
            foreach (string path in capturedPaths)
            {
                Assert.IsTrue(File.Exists(path), $"capture missing: {path}");
                Assert.Greater(new FileInfo(path).Length, 0L, $"capture is empty: {path}");
            }
        }

        [UnityTest]
        public IEnumerator Capture_two_more_truthful_surething_states_as_flat_and_angled_pngs()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            string outputDirectory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "..", "artifacts", "surething-ui"));
            Directory.CreateDirectory(outputDirectory);
            string runPrefix = DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            var capturedPaths = new List<string>();

            // REWARDS is gated to Phase.Shop (SportsbookApp.BuildChrome: the REWARDS tab is
            // disabled whenever run.Phase != Phase.Shop). The only deterministic, already-
            // established way there is the zero-ticket lock seam
            // SureThingRewardsTests.EnterShop uses: on a fresh boot Run.Tickets.Count is 0, so
            // LockRound settles the round without any TV ceremony and lands directly in Shop
            // with the book defaulted to REWARDS (LaptopOs.ApplyPhaseDefault). This state must
            // come from a fresh Boot() rather than continuing the first test's run, because
            // that run already carries a placed ticket and would not take the zero-ticket path.
            Assert.AreEqual(0, laptop.director.Run.Tickets.Count,
                "zero-ticket lock is the deterministic shop-entry test seam");
            laptop.director.LockRound();
            yield return WaitForRebuild();
            Assert.AreEqual(Phase.Shop, laptop.director.Run.Phase);
            Assert.AreEqual(SportsbookApp.Tab.Rewards, laptop.Os.CurrentTab,
                "the Shop phase default did not land on REWARDS");
            Assert.IsNotNull(Required(App(laptop), "RewardsBoard"),
                "REWARDS did not render its board");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "07-rewards", capturedPaths);

            // Old Slips is LaptopOs's App.OldSlips reached from the desktop icon rather than the
            // in-app tray — the same screen as 06-ledger above (OldSlipsApp.Render), just via
            // its other named entry point (LaptopOs.MakeDesktopIcon("OldSlips", ...)).
            laptop.Os.OpenDesktop();
            yield return WaitForRebuild();
            Assert.IsTrue(laptop.Os.OnDesktop, "OpenDesktop did not leave the sportsbook");
            Invoke(Required(laptop.transform, "OldSlips"));
            yield return WaitForRebuild();
            Assert.IsFalse(laptop.Os.OnDesktop,
                "the desktop Old Slips icon did not leave the desktop");
            Assert.IsNotNull(Required(App(laptop), "LedgerBoard"),
                "the desktop Old Slips icon did not open the ledger screen");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "08-old-slips", capturedPaths);

            Assert.AreEqual(4, capturedPaths.Count, "two states must emit paired captures");
            foreach (string path in capturedPaths)
            {
                Assert.IsTrue(File.Exists(path), $"capture missing: {path}");
                Assert.Greater(new FileInfo(path).Length, 0L, $"capture is empty: {path}");
            }
        }

        private static IEnumerator CaptureState(LaptopScreen laptop, string outputDirectory,
            string runPrefix, string stateName, ICollection<string> capturedPaths)
        {
            yield return WaitForRebuild();
            UnityEngine.Canvas.ForceUpdateCanvases();

            RectTransform canvasRect = Required(
                laptop.transform, "LaptopOsCanvas") as RectTransform;
            Assert.IsNotNull(canvasRect, "LaptopOsCanvas RectTransform missing");

            string flatPath = Path.Combine(outputDirectory,
                $"{runPrefix}-{stateName}-flat-{FlatWidth}x{FlatHeight}.png");
            CaptureFlatCanvas(canvasRect, flatPath);
            capturedPaths.Add(flatPath);
            Debug.Log($"[SureThingCapture] {flatPath}");

            string angledPath = Path.Combine(outputDirectory,
                $"{runPrefix}-{stateName}-main-camera-{AngledWidth}x{AngledHeight}.png");
            CaptureAngledMainCamera(laptop, canvasRect, angledPath);
            capturedPaths.Add(angledPath);
            Debug.Log($"[SureThingCapture] {angledPath}");
        }

        private static void CaptureFlatCanvas(RectTransform canvasRect, string outputPath)
        {
            UnityEngine.Canvas canvas = canvasRect.GetComponent<UnityEngine.Canvas>();
            Assert.IsNotNull(canvas, "LaptopOsCanvas Canvas missing");
            Camera mainCamera = Camera.main;
            Assert.IsNotNull(mainCamera, "Main Camera missing");

            var corners = new Vector3[4];
            canvasRect.GetWorldCorners(corners);
            Vector3 rightVector = corners[3] - corners[0];
            Vector3 upVector = corners[1] - corners[0];
            float worldWidth = rightVector.magnitude;
            float worldHeight = upVector.magnitude;
            Assert.Greater(worldWidth, 0f, "LaptopOsCanvas world width must be positive");
            Assert.Greater(worldHeight, 0f, "LaptopOsCanvas world height must be positive");

            Vector3 right = rightVector.normalized;
            Vector3 up = upVector.normalized;
            Vector3 normal = Vector3.Cross(right, up).normalized;
            Vector3 center = (corners[0] + corners[2]) * 0.5f;
            if (Vector3.Dot(normal, mainCamera.transform.position - center) < 0f)
                normal = -normal;

            var cameraObject = new GameObject(
                "SureThingFlatCaptureCamera", typeof(Camera));
            cameraObject.hideFlags = HideFlags.HideAndDontSave;
            Camera flatCamera = cameraObject.GetComponent<Camera>();
            flatCamera.enabled = false;
            flatCamera.orthographic = true;
            flatCamera.aspect = FlatWidth / (float)FlatHeight;
            flatCamera.orthographicSize = Mathf.Max(
                worldHeight * 0.5f,
                worldWidth / (2f * flatCamera.aspect));
            flatCamera.nearClipPlane = 0.001f;
            flatCamera.farClipPlane = 10f;
            flatCamera.clearFlags = CameraClearFlags.SolidColor;
            flatCamera.backgroundColor = Color.black;
            flatCamera.cullingMask = 1 << CaptureLayer;
            flatCamera.allowHDR = false;
            flatCamera.allowMSAA = false;
            flatCamera.useOcclusionCulling = false;
            cameraObject.transform.SetPositionAndRotation(
                center + normal * 0.35f,
                Quaternion.LookRotation(-normal, up));

            Transform[] canvasHierarchy =
                canvasRect.GetComponentsInChildren<Transform>(true);
            var originalLayers = new int[canvasHierarchy.Length];
            Camera originalWorldCamera = canvas.worldCamera;
            try
            {
                for (int i = 0; i < canvasHierarchy.Length; i++)
                {
                    originalLayers[i] = canvasHierarchy[i].gameObject.layer;
                    canvasHierarchy[i].gameObject.layer = CaptureLayer;
                }
                canvas.worldCamera = flatCamera;
                UnityEngine.Canvas.ForceUpdateCanvases();
                RenderCameraToPng(flatCamera, FlatWidth, FlatHeight, outputPath);
            }
            finally
            {
                canvas.worldCamera = originalWorldCamera;
                for (int i = 0; i < canvasHierarchy.Length; i++)
                    if (canvasHierarchy[i] != null)
                        canvasHierarchy[i].gameObject.layer = originalLayers[i];
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }

        private static void CaptureAngledMainCamera(LaptopScreen laptop,
            RectTransform canvasRect, string outputPath)
        {
            Camera mainCamera = Camera.main;
            Assert.IsNotNull(mainCamera, "Main Camera missing");
            DeskFocus focus = laptop.GetComponent<DeskFocus>();
            if (focus == null) focus = laptop.GetComponentInParent<DeskFocus>();
            Assert.IsNotNull(focus, "Laptop DeskFocus missing");
            Assert.IsNotNull(focus.focusAnchor, "Laptop DeskFocus focus anchor missing");
            UnityEngine.Canvas canvas = canvasRect.GetComponent<UnityEngine.Canvas>();
            Assert.IsNotNull(canvas, "LaptopOsCanvas Canvas missing");

            Vector3 originalPosition = mainCamera.transform.position;
            Quaternion originalRotation = mainCamera.transform.rotation;
            float originalFieldOfView = mainCamera.fieldOfView;
            float originalAspect = mainCamera.aspect;
            Camera originalWorldCamera = canvas.worldCamera;
            try
            {
                mainCamera.transform.SetPositionAndRotation(
                    focus.focusAnchor.position, focus.focusAnchor.rotation);
                mainCamera.fieldOfView = focus.focusFov;
                mainCamera.aspect = AngledWidth / (float)AngledHeight;
                canvas.worldCamera = mainCamera;
                UnityEngine.Canvas.ForceUpdateCanvases();
                RenderCameraToPng(
                    mainCamera, AngledWidth, AngledHeight, outputPath);
            }
            finally
            {
                canvas.worldCamera = originalWorldCamera;
                mainCamera.fieldOfView = originalFieldOfView;
                mainCamera.aspect = originalAspect;
                mainCamera.transform.SetPositionAndRotation(
                    originalPosition, originalRotation);
            }
        }

        private static void RenderCameraToPng(
            Camera camera, int width, int height, string outputPath)
        {
            RenderTexture originalTarget = camera.targetTexture;
            RenderTexture originalActive = RenderTexture.active;
            var target = new RenderTexture(
                width, height, 24, RenderTextureFormat.ARGB32)
            {
                name = "SureThingCaptureTarget",
                hideFlags = HideFlags.HideAndDontSave,
            };
            var image = new Texture2D(
                width, height, TextureFormat.RGB24, false)
            {
                name = "SureThingCaptureImage",
                hideFlags = HideFlags.HideAndDontSave,
            };

            try
            {
                target.Create();
                camera.targetTexture = target;
                RenderTexture.active = target;
                camera.Render();
                image.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, false);
                image.Apply(false, false);
                File.WriteAllBytes(outputPath, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = originalTarget;
                RenderTexture.active = originalActive;
                target.Release();
                UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static void InvokeView(
            RevealedView view, string methodName, params object[] args)
        {
            MethodInfo method = typeof(RevealedView).GetMethod(
                methodName, BindingFlags.Instance | BindingFlags.NonPublic);
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

        private static IEnumerator Boot()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "Room", LoadSceneMode.Single);
            Assert.IsNotNull(load, "Room scene is not available");
            while (!load.isDone) yield return null;

            LaptopScreen laptop = Laptop();
            float start = Time.realtimeSinceStartup;
            while (laptop.director == null
                || laptop.director.Run == null
                || laptop.Os.OnDesktop)
            {
                if (Time.realtimeSinceStartup - start > 10f)
                {
                    Assert.Fail(
                        "SureThing did not reach the betting lobby within 10 seconds");
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

        private static LaptopScreen Laptop()
        {
            LaptopScreen laptop =
                UnityEngine.Object.FindAnyObjectByType<LaptopScreen>();
            Assert.IsNotNull(laptop, "LaptopScreen missing");
            Assert.IsNotNull(laptop.tv, "Laptop TV reference missing");
            return laptop;
        }

        private static Transform App(LaptopScreen laptop)
            => Required(laptop.transform, "App");

        private static Button FirstNamedButton(Transform root, string prefix)
        {
            var matches = new List<Button>();
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
                if (button.name.StartsWith(prefix, StringComparison.Ordinal))
                    matches.Add(button);
            matches.Sort((left, right)
                => string.CompareOrdinal(left.name, right.name));
            Assert.Greater(matches.Count, 0,
                $"No button beginning '{prefix}' exists beneath '{root.name}'");
            return matches[0];
        }

        private static Transform Required(Transform root, string name)
        {
            Transform found = Find(root, name);
            Assert.IsNotNull(found,
                $"Required named UI node '{name}' missing beneath '{root.name}'");
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

        private static void Invoke(Transform node)
        {
            Button button = node.GetComponent<Button>();
            Assert.IsNotNull(button, $"{node.name} must be a button");
            Assert.IsTrue(button.interactable, $"{node.name} must be interactable");
            button.onClick.Invoke();
        }

        private static string TextOf(Transform node)
        {
            Text text = node.GetComponent<Text>();
            if (text == null) text = node.GetComponentInChildren<Text>();
            Assert.IsNotNull(text, $"{node.name} has no readable text");
            return text.text;
        }
    }
}
