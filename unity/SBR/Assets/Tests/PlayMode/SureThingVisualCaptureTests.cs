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
    /// Twelve states are captured across four UnityTests. The first continues the single-run,
    /// ticket-carrying flow through six states (the original five plus the shared Ledger/Old
    /// Slips screen reached from the tray). The second boots a fresh run to reach REWARDS —
    /// which requires the deterministic zero-ticket lock seam SureThingRewardsTests.EnterShop
    /// uses, and so cannot share the first run's already-placed ticket — and from there also
    /// reaches the same Ledger/Old Slips screen via its other entry point, the desktop icon, and
    /// the shop again with comps to spend so an enabled BUY is actually photographed. It also stops
    /// on the desktop on the way past, which is the only state that shows the machine's wallpaper. The third
    /// runs a real place-lock-sweat cycle purely so the LEDGER can be photographed with a settled
    /// ticket in it — every other capture of that screen shows it empty, which left its entire
    /// settled-record treatment unphotographed and readable only from source. The fourth pushes
    /// three 2-leg tickets through a real place-lock-sweat cycle, cashing one out the instant the
    /// round locks, so the LEDGER can be photographed with several settled rows in as many
    /// different terminal states as the engine will honestly produce — the third fixture's single
    /// row cannot show the record's rhythm across neighbours or the CASHED OUT treatment at all.
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

            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "02-entry-selected-wide-ring", capturedPaths);

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
            Assert.IsNotNull(Required(
                Required(margin, "StagedTickets"), "StagedTicket0"));
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

            Assert.AreEqual(12, capturedPaths.Count, "six states must emit paired captures");
            foreach (string path in capturedPaths)
            {
                Assert.IsTrue(File.Exists(path), $"capture missing: {path}");
                Assert.Greater(new FileInfo(path).Length, 0L, $"capture is empty: {path}");
            }
        }

        [UnityTest]
        public IEnumerator Capture_four_more_truthful_surething_states_as_flat_and_angled_pngs()
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

            // The same shop with the comps to spend. This state exists because its absence hid a
            // real defect: every earlier rewards capture was taken at zero comps, so every BUY
            // rendered in its disabled grey and a Law Two violation — an affordable BUY drawn in
            // the player's biro rather than in wax — was invisible in every screenshot we had. A
            // control's enabled appearance is not evidence unless something captures it enabled.
            laptop.director.Run.GrantComps(1000);
            laptop.Os.OpenSportsbook(SportsbookApp.Tab.Rewards);
            yield return WaitForRebuild();
            Transform affordableBoard = Required(App(laptop), "RewardsBoard");
            Button firstBuy = FirstNamedButton(affordableBoard, "Buy");
            Assert.IsNotNull(firstBuy, "no BUY control on the rewards board");
            Assert.IsTrue(firstBuy.interactable,
                "1000 comps must make at least one offer affordable, or this state proves nothing");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "09-rewards-affordable", capturedPaths);

            // Old Slips is LaptopOs's App.OldSlips reached from the desktop icon rather than the
            // in-app tray — the same screen as 06-ledger above (OldSlipsApp.Render), just via
            // its other named entry point (LaptopOs.MakeDesktopIcon("OldSlips", ...)).
            laptop.Os.OpenDesktop();
            yield return WaitForRebuild();
            Assert.IsTrue(laptop.Os.OnDesktop, "OpenDesktop did not leave the sportsbook");

            // The desktop itself, which no capture has ever shown. It carries the machine's
            // wallpaper, and that wallpaper spent this entire project inert: LaptopWallpaperGraphic
            // was constructed without a CanvasRenderer, so UGUI never asked it for geometry and it
            // drew nothing while every test stayed green. It draws now, and nobody has seen it.
            //
            // The assertions below are the point of the state, not ceremony. A capture that cannot
            // fail proves nothing — this is the same lesson as the rewards board, where every frame
            // was taken at zero comps so a BUY-in-biro violation stayed invisible for weeks because
            // the control was always greyed out. So: confirm the wallpaper object exists, that it
            // carries the CanvasRenderer whose absence made it invisible, and that it is actually
            // enabled — then shoot.
            Transform wallpaper = Required(laptop.transform, "Wallpaper");
            var wallpaperGraphic = wallpaper.GetComponent<Graphic>();
            Assert.IsNotNull(wallpaperGraphic, "the wallpaper carries no Graphic to render");
            Assert.IsNotNull(wallpaper.GetComponent<CanvasRenderer>(),
                "the wallpaper has no CanvasRenderer, so it cannot draw and this capture proves nothing");
            Assert.IsTrue(wallpaperGraphic.enabled && wallpaper.gameObject.activeInHierarchy,
                "the wallpaper is disabled or inactive, so this capture proves nothing");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "11-desktop", capturedPaths);

            Invoke(Required(laptop.transform, "OldSlips"));
            yield return WaitForRebuild();
            Assert.IsFalse(laptop.Os.OnDesktop,
                "the desktop Old Slips icon did not leave the desktop");
            Assert.IsNotNull(Required(App(laptop), "LedgerBoard"),
                "the desktop Old Slips icon did not open the ledger screen");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "08-old-slips", capturedPaths);

            Assert.AreEqual(8, capturedPaths.Count, "four states must emit paired captures");
            foreach (string path in capturedPaths)
            {
                Assert.IsTrue(File.Exists(path), $"capture missing: {path}");
                Assert.Greater(new FileInfo(path).Length, 0L, $"capture is empty: {path}");
            }
        }

        /// <summary>
        /// The ledger with a genuinely settled ticket in it.
        ///
        /// Every other capture of this screen shows it empty, which means the entire settled-record
        /// treatment — the terminal word, the strike, the returned figure, the row's recession —
        /// has never been photographed once. The C14 audit had to read all of it out of source.
        ///
        /// That gap is not hypothetical. A Law Two violation on the rewards BUY control survived
        /// weeks of review on this surface because no capture ever showed an affordable offer, and
        /// every reviewer looked at a screenshot where the control was greyed out. This is the same
        /// shape of blind spot, one screen over.
        ///
        /// It needs its own run rather than riding along with the others: the ledger only shows
        /// tickets the engine has actually settled, so it needs a real place-lock-sweat cycle. The
        /// other fixtures deliberately fake the TV mirror instead, which populates MY BETS and
        /// leaves the ledger empty. Sequence is the one SureThingLedgerTests already proves.
        /// </summary>
        [UnityTest]
        public IEnumerator Capture_the_populated_ledger_so_settled_states_are_photographed()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            string outputDirectory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "..", "artifacts", "surething-ui"));
            Directory.CreateDirectory(outputDirectory);
            string runPrefix = DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            var capturedPaths = new List<string>();

            Run run = laptop.director.Run;
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            Ticket ticket = run.PlaceTicket(picks, stake);

            // Collapse the sweat rather than skip it: the ledger reads engine state, so the ticket
            // has to travel the real path to a terminal state, not be written into one.
            TvSweatScreen screen = laptop.tv;
            screen.TimeScaleOverride = 0.0001f;
            screen.ForceSeated(true);
            laptop.director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);

            float start = Time.realtimeSinceStartup;
            while (run.Phase == Phase.Sweat)
            {
                if (Time.realtimeSinceStartup - start > 60f)
                {
                    Assert.Fail("the ticket never settled, so there is no populated ledger to shoot");
                    yield break;
                }
                yield return null;
            }
            Assert.AreNotEqual(TicketState.Open, ticket.State,
                "a settled ledger row is the entire point of this capture");
            yield return WaitForRebuild();

            Invoke(Required(Required(App(laptop), "NotebookTray"), "Ledger"));
            yield return WaitForRebuild();
            Transform board = Required(App(laptop), "LedgerBoard");
            Assert.IsNull(Find(board, "LedgerEmpty"),
                "ledger still rendered its empty state after a ticket settled");
            Assert.IsNotNull(Required(board, "LedgerTicket0"));

            // Which terminal state this run produced decides what the capture can actually prove.
            // The LOST treatment (word in toner-3, strike in oxide, returned figure in toner-3)
            // is only verifiable from a capture that contains a lost ticket.
            Debug.Log($"[LedgerCapture] ticket settled as {ticket.State} — "
                + "LOST colour work is only verifiable from this capture if that reads Lost");

            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "10-ledger-populated", capturedPaths);

            Assert.AreEqual(2, capturedPaths.Count, "one state must emit paired captures");
            foreach (string path in capturedPaths)
            {
                Assert.IsTrue(File.Exists(path), $"capture missing: {path}");
                Assert.Greater(new FileInfo(path).Length, 0L, $"capture is empty: {path}");
            }
        }

        /// <summary>
        /// The ledger with three settled tickets in as many different terminal states as a single
        /// honest run will produce.
        ///
        /// 10-ledger-populated above proves the settled-record treatment exists, but with exactly
        /// one row in whichever terminal state the engine happened to land on that run. It cannot
        /// show the row's rhythm against a neighbour, and it cannot show CASHED OUT at all — nothing
        /// in that fixture ever reaches the cash-out window. A Design Director cannot rule on the
        /// record row's layout from a single, arbitrary row.
        ///
        /// Three 2-leg tickets are placed on disjoint matchups — (0,1), (2,3), (4,5) — the widest
        /// spread Config.MaxTicketsPerRound (3) allows across the six-matchup slate without doubling
        /// up. CASHED OUT is the one terminal state this fixture can GUARANTEE: the first ticket's
        /// session is cashed out the instant LockRound() returns, before anything yields and before
        /// any leg has resolved — the only moment SweatSession's CashOutAvailable guard (not
        /// DoubleOrNothing, >=2 legs, not complete, no pending dead leg, ticket still Open, no
        /// already-lost leg) is certain to hold, since a single further frame could resolve a leg
        /// Lost and close the window for good. The other two tickets run the real sweat and settle
        /// however the slate decides — WON and LOST are engine truth, never forced here, and if both
        /// land the same way that is the honest result, logged rather than hidden.
        /// </summary>
        [UnityTest]
        public IEnumerator Capture_the_ledger_with_three_settled_tickets_in_distinct_terminal_states()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            string outputDirectory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "..", "artifacts", "surething-ui"));
            Directory.CreateDirectory(outputDirectory);
            string runPrefix = DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            var capturedPaths = new List<string>();

            Run run = laptop.director.Run;
            Assert.GreaterOrEqual(run.CurrentSlate.Matchups.Count, 6,
                "three disjoint 2-leg tickets need six matchups on the slate");

            // Three 2-leg tickets on disjoint matchups: every matchup on the six-slot slate is used
            // exactly once, so no ticket can share a matchup with another.
            Ticket ticketA = run.PlaceTicket(
                new[] { new Pick(0, Side.Home), new Pick(1, Side.Home) }, run.Config.MinStake);
            Ticket ticketB = run.PlaceTicket(
                new[] { new Pick(2, Side.Home), new Pick(3, Side.Home) }, run.Config.MinStake);
            Ticket ticketC = run.PlaceTicket(
                new[] { new Pick(4, Side.Home), new Pick(5, Side.Home) }, run.Config.MinStake);
            Assert.AreEqual(3, run.Tickets.Count, "three tickets must be on the board before locking");
            Assert.AreEqual(2, ticketA.Legs.Count, "ticket A must carry two legs to be cash-out eligible");
            Assert.AreEqual(2, ticketB.Legs.Count);
            Assert.AreEqual(2, ticketC.Legs.Count);

            // Collapse the sweat rather than skip it, exactly as the single-ticket fixture above
            // does: the ledger reads engine state, so every ticket has to travel the real path to a
            // terminal state.
            TvSweatScreen screen = laptop.tv;
            screen.TimeScaleOverride = 0.0001f;
            screen.ForceSeated(true);
            laptop.director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);

            // Cash out ticket A's session in this same instant, before a single yield — before the
            // TV's own coroutine has even picked up director.CurrentSession, and before any drama
            // event has been emitted. Nothing has resolved yet, so CashOutAvailable's guard holds:
            // not Double or Nothing, two legs, session not complete, no pending dead leg, ticket
            // still Open, no already-lost leg. One frame later would not be good enough — a revealed
            // Lost leg on this very ticket would close the window permanently.
            Assert.AreEqual(3, run.Sweats.Count, "one sweat session per ticket, in placement order");
            run.Sweats[0].AcceptCashOut();
            Assert.AreEqual(TicketState.CashedOut, ticketA.State,
                "the cash-out must land the instant the round locks, or this guard can't be trusted");

            float start = Time.realtimeSinceStartup;
            while (run.Phase == Phase.Sweat)
            {
                if (Time.realtimeSinceStartup - start > 60f)
                {
                    Assert.Fail("the round never finished settling, so there is no multi-state ledger to shoot");
                    yield break;
                }
                yield return null;
            }
            yield return WaitForRebuild();

            // Assert before capturing: a capture that cannot fail proves nothing. This fixture has
            // already learned that lesson twice — once on the rewards board, where every frame was
            // taken at zero comps so a Law Two violation on BUY stayed invisible for weeks.
            Assert.AreEqual(3, run.Tickets.Count, "placing three tickets must not have silently dropped one");
            bool anyOpen = false;
            bool anyCashedOut = false;
            foreach (Ticket t in run.Tickets)
            {
                if (t.State == TicketState.Open) anyOpen = true;
                if (t.State == TicketState.CashedOut) anyCashedOut = true;
            }
            Assert.IsFalse(anyOpen, "every ticket must be settled before this capture means anything");
            Assert.IsTrue(anyCashedOut, "the guaranteed cash-out must have survived to settlement");

            // Which terminal states B and C actually landed on decides what this capture can prove
            // about WON and LOST — that is engine truth, read here rather than guessed from pixels.
            Debug.Log($"[LedgerCapture] ticket A (index 0) settled as {ticketA.State} — guaranteed cash-out");
            Debug.Log($"[LedgerCapture] ticket B (index 1) settled as {ticketB.State} — engine truth");
            Debug.Log($"[LedgerCapture] ticket C (index 2) settled as {ticketC.State} — engine truth");

            Invoke(Required(Required(App(laptop), "NotebookTray"), "Ledger"));
            yield return WaitForRebuild();
            Transform board = Required(App(laptop), "LedgerBoard");
            Assert.IsNull(Find(board, "LedgerEmpty"),
                "ledger still rendered its empty state after three tickets settled");
            Assert.IsNotNull(Required(board, "LedgerTicket0"), "ledger did not render a first settled row");
            Assert.IsNotNull(Required(board, "LedgerTicket1"), "ledger did not render a second settled row");
            Assert.IsNotNull(Required(board, "LedgerTicket2"), "ledger did not render a third settled row");

            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "12-ledger-populated-multi", capturedPaths);

            Assert.AreEqual(2, capturedPaths.Count, "one state must emit paired captures");
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
