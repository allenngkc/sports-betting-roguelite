using System;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace SBR.Tests.PlayMode
{
    /// <summary>
    /// DISPOSABLE evidence harness for the seated TV sweat (Phase 3 / Phase 4 gates) — same spirit
    /// as the room worktree's capture harness (room-refinement worktree, commit 588f84e,
    /// unity/SBR/Assets/SBR/Editor/RoomViewCapture.cs): built for one evidence question, never
    /// production coverage, and removable in one delete once the question is answered (their
    /// harness was in fact removed at sign-off, then briefly restored for a reopened pass).
    ///
    /// <para><b>Supersedes CaptureHarnessSpike.cs.</b> That spike's question — "does a batch
    /// PlayMode frame rasterise and survive to disk without <c>-nographics</c>" — the lead confirmed
    /// answered 2026-07-31: batchmode WITHOUT <c>-nographics</c> gives a real D3D12 device on this
    /// machine, and the spike's own negative control showed <c>-nographics</c> is exactly the
    /// null-device failure case. This file is the real thing the spike cleared the way for: it
    /// drives an ACTUAL <see cref="TvSweatScreen"/> session, seated, through the room's own
    /// production wiring (RunDirector, SitSpot, the real canvas/stage), and captures frame bursts
    /// at named, deterministically-reached moments.</para>
    ///
    /// <para><b>Seated pose.</b> Reused verbatim from the room-refinement worktree's accepted
    /// "seated TV 17°" composition — <c>RoomViewCapture.Capture</c>'s <c>seated-tv-couch.png</c>
    /// shot (read via <c>git show 588f84e:unity/SBR/Assets/SBR/Editor/RoomViewCapture.cs</c> in that
    /// worktree; not copied into this one, not modified, not re-derived): the same eye point, the
    /// same <c>LookRotation</c> at the TV screen center, the same 17° field of view. This class
    /// repositions the scene's own live <c>PlayerCamera</c> to that pose rather than building a new
    /// camera, so the capture goes through the same URP render path (HDR bloom, the unified grade
    /// volume) the seated view actually uses in play — the same reason RoomViewCapture reused the
    /// live camera instead of a synthetic one.</para>
    ///
    /// <para><b>Render path.</b> RenderTexture + ReadPixels, never <c>ScreenCapture</c> — the same
    /// choice CaptureHarnessSpike proved out, because <c>ScreenCapture</c> depends on a backbuffer
    /// this process may not own.</para>
    ///
    /// <para><b>What this harness does NOT do.</b> It never asserts anything about how a capture
    /// looks — legibility is a Design Director call. Its only assertions are plumbing-level (a scene
    /// object existed, a PNG got written, a named moment was actually reached before the deadline) —
    /// the same class of assertion CaptureHarnessSpike used to distinguish "the pipeline is broken"
    /// from "a human needs to look at this."</para>
    ///
    /// <para><b>Determinism.</b> <see cref="CaptureSeed"/> is pinned via
    /// <c>RunDirector.StartNewRun</c> (overriding whatever random seed <c>Start()</c> rolled before
    /// this coroutine gets control), and the ticket is hand-built from three fixed, distinct
    /// matchups rather than <see cref="DemoTicketPolicy"/>'s picks (its stake formula is reused,
    /// since that IS pure and deterministic — see its own doc comment — but its picks are always
    /// moneyline-only, and this harness needs a guaranteed AnytimeScorer leg). Same seed, same
    /// picks, same pacing (<c>TimeScaleOverride = 1</c>, ship pacing) ⇒ the same DramaEvent sequence
    /// every run; only wall-clock frame timing can vary with host performance, and every moment
    /// predicate below is a logical state check, never a frame count, so that variance should not
    /// change WHAT gets captured.</para>
    ///
    /// <para><b>DELETE THIS FILE</b> once Phase 3/4 evidence review is done — it is not production
    /// coverage, exactly like the room worktree's equivalent.</para>
    /// </summary>
    public class TvSweatCaptureHarness
    {
        // Room-refinement worktree, commit 588f84e, RoomViewCapture.Capture's "seated-tv-couch.png"
        // shot — "Matches the builder: LookRotation(tvScreenCenter - seatedEye, up)". Reused
        // verbatim; do not retune without a fresh design ruling on the composition.
        private static readonly Vector3 SeatedEye = new Vector3(-0.950f, 1.150f, 0.300f);
        private static readonly Vector3 TvScreenCenter = new Vector3(1.232f, 1.100f, 0.300f);
        private const float SeatedFovDeg = 17f;

        private const int CaptureWidth = 2560;
        private const int CaptureHeight = 1440;

        // Any non-blank string is a legal RunDirector seed (StartNewRun trims but does not
        // restrict the charset) — these are just easy to grep back out of an artifact filename.
        //
        // FIVE seeds, not one. A single seed produces one match, and the containment claim T25.1
        // has to prove is "no layer leaves the glass in ANY sweat" — which one match cannot show.
        // Five different matches vary scoreline, market state and scene grammar, so a layer that
        // only escapes under some conditions has somewhere to show itself. The first seed stays first
        // so the new frames are directly comparable to the 49 already in the DD's hands.
        // T31: NUMERIC seeds. The DD withdrew the finding that the footer seed was a debug string —
        // it is `Rng.RunSeed`, which PRD §8.1 specifies as chrome content ("round, bank, payment,
        // seed"). What made it READ as debug was this harness handing the run a seed shaped like
        // TVCAPTURE01. A real player's seed is a number, so the capture's seed is a number, and the
        // chrome row in every frame now shows what it will actually show in the product.
        //
        // Kept distinct and stable so a frame's provenance is still greppable out of its filename.
        private static readonly string[] CaptureSeeds =
            { "48151623", "42108675", "30941771", "16180339", "27182818" };

        // The seed of the run currently being captured; drives both StartNewRun and the filename.
        // Static because CaptureBurst is static and the test cases run one at a time.
        private static string _seed = CaptureSeeds[0];

        /// <summary>Monotonic across the whole run, so the frames sort into the order the sweat
        /// actually played them — which is the "scene index" T26 asked for. Reset per seed so an
        /// index is readable as "the Nth captured moment of THIS sweat".</summary>
        private static int s_sceneIndex;

        /// <summary>Whether the most recent <see cref="WaitUntilOrAbsent"/> actually saw its
        /// condition. False means the moment did not occur in this sweat.</summary>
        private static bool s_conditionMet;

        /// <summary>Waits for <paramref name="until"/>, for the session to END, or for the deadline —
        /// and treats the middle case as a legitimate ABSENT moment rather than a failure.
        ///
        /// <para>This is the fix for a harness defect that cost two capture runs. The scorer-leg and
        /// cash-out waits assumed every seed plays all three legs. A ticket that dies on an earlier
        /// leg never plays leg 3 at all, so "the scorer leg reached a terminal state" can never
        /// become true — the wait was UNSATISFIABLE, not slow, and raising the deadline from 240s to
        /// 420s changed only how long it took to fail. Seed 01 completes in 60s; seeds 02–05 burned
        /// the full budget waiting for something that was never going to happen.</para>
        ///
        /// <para>The harness's own contract already says so for the scorer case — "no scorer identity
        /// on a losing pick is a real, legitimate outcome this harness must still capture". A moment
        /// that did not occur is evidence about the sweat, not a broken run. The deadline still fails
        /// loudly while the session is LIVE, because that is a genuine hang.</para></summary>
        private static IEnumerator WaitUntilOrAbsent(System.Func<bool> until, RunDirector director,
            float deadline, string what)
        {
            s_conditionMet = false;
            while (Time.realtimeSinceStartup < deadline)
            {
                if (until()) { s_conditionMet = true; yield break; }
                if (director != null && director.CurrentSession != null && director.CurrentSession.IsComplete)
                {
                    // GRACE PERIOD, and it is load-bearing. The session reports complete BEFORE the
                    // revealed leg states settle, so bailing on IsComplete alone skipped
                    // scorer-leg-resolved on a seed where it demonstrably does occur — the same
                    // moment the previous run captured. A wait that silently drops a real moment is
                    // worse than the deadline failure it replaced, because the evidence goes missing
                    // instead of the run going red.
                    //
                    // So completion means "stop waiting indefinitely", not "the answer is no". Keep
                    // polling the condition for a bounded settle window and only then call it absent.
                    for (int i = 0; i < 120; i++) // ~2s at 60fps
                    {
                        if (until()) { s_conditionMet = true; yield break; }
                        yield return null;
                    }
                    Debug.Log($"[TvSweatCaptureHarness] seed={_seed}: {what} — ABSENT, the session " +
                        "ended and the condition did not settle within the grace window. Recorded " +
                        "as an outcome of this sweat, not a failed run.");
                    yield break;
                }
                yield return null;
            }
            Assert.Fail($"{what} — deadline reached with the session still LIVE, which is a genuine hang");
        }

        // Leg 2 (0-based) of the fixed ticket built below is always the AnytimeScorer leg.
        private const int ScorerLegIndex = 2;

        private static string OutputDir =>
            Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "tv-sweat-capture");

        /// <summary>OPT-IN ONLY — <c>[Explicit]</c> is load-bearing, not tidiness.
        ///
        /// <para>This runs at ship pacing with a 240s deadline. Without <c>[Explicit]</c> it joins
        /// every routine PlayMode suite and adds up to four minutes to each one. That is not merely
        /// slow: `BUG-LEDGER.md` §4C.4 documents a flake — <c>never observed the cash-out amount
        /// mid-tween</c> — that correlates specifically with **slow, loaded runs** (measured at
        /// 52-54s suites against a ~35s norm). Adding a four-minute test to the suite would very
        /// plausibly raise the flake rate for every other test in it, and that flake has already
        /// cost this slice one false regression call.</para>
        ///
        /// <para>Evidence infrastructure is run deliberately, when captures are wanted:
        /// <c>-testFilter "SBR.Tests.PlayMode.TvSweatCaptureHarness"</c>. It is not verification and
        /// nothing gates on it.</para></summary>
        [Explicit("Evidence capture, not verification: ship-paced and up to 240s. Run by filter only — "
            + "including it in routine suites would slow them enough to aggravate the documented "
            + "load-correlated flake (BUG-LEDGER §4C.4).")]
        // NUnit's default UnityTest timeout is 180s, but this harness runs at SHIP pacing behind its
        // own 240s deadline — so NUnit was killing the run before the harness's own guard could ever
        // fire. Latent since the harness was written and invisible while only one seed was used:
        // the first seed happens to finish inside 180s, and the rest do not. 300s clears the
        // internal deadline with headroom, so a genuine hang still fails on the harness's own
        // message ("...never reached a terminal state") rather than on an opaque framework timeout.
        [Timeout(480000)]
        [UnityTest]
        public IEnumerator Capture_SeatedSweat_NamedMoments(
            [ValueSource(nameof(CaptureSeeds))] string seed)
        {
            _seed = seed;
            s_sceneIndex = 0; // per seed, so an index reads as "the Nth moment of THIS sweat"
            Directory.CreateDirectory(OutputDir);

            yield return LoadRoom();

            var director = Object.FindAnyObjectByType<RunDirector>();
            var screen = Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director, "RunDirector missing - run SBR.GrayboxRoomBuilder.Build first.");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");

            Camera cam = Camera.main;
            Assert.IsNotNull(cam, "MainCamera (PlayerCamera) missing - cannot capture without it");

            screen.TimeScaleOverride = 1f; // ship pacing: real beat durations, real (non-cosmetic-only) dot timing
            couch.transitionDuration = 0.01f; // the couch's OWN camera lerp - not this harness's capture pose

            yield return WaitUntilOrFail(() => director.Run != null,
                Time.realtimeSinceStartup + 10f, "director never started a run");

            // Override whatever random seed Start() rolled - deterministic from here on.
            director.StartNewRun(_seed);
            Run run = director.Run;
            Assert.AreEqual(Phase.Betting, run.Phase, "a fresh run opens in Betting");

            // A hand-built ticket, not DemoTicketPolicy (whose picks are always moneyline-only —
            // see its own doc comment): two moneyline legs mirroring its own "shortest-priced side"
            // convention and its proven 2-3-leg cash-out-eligible shape (TvSweatScreenTests' TVS-H01
            // regressions use exactly that shape), plus one AnytimeScorer leg on a THIRD matchup so
            // a scorer-market beat is guaranteed to be somewhere in this sweat. Whether that specific
            // player actually scores is the deterministic match sim's call under CaptureSeed, not
            // this harness's — see the scorer-leg moments below for how their triggers stay valid
            // either way (PRD §4.1 / SweatPresentationModel.BindAnytimeScorer: "no scorer identity
            // on a losing pick" is a real, legitimate outcome this harness must still capture).
            Assert.GreaterOrEqual(run.CurrentSlate.Matchups.Count, 3,
                "need 3 distinct matchups for this harness's fixed ticket shape");
            Matchup mlA = run.CurrentSlate.Matchups[0];
            Matchup mlB = run.CurrentSlate.Matchups[1];
            Matchup scorerMatchup = run.CurrentSlate.Matchups[2];
            Assert.Greater(scorerMatchup.Away.Players.Count + scorerMatchup.Home.Players.Count, 0,
                "matchup index 2 has no roster - cannot place an AnytimeScorer leg on it");

            var picks = new List<Pick>
            {
                new Pick(mlA.Index, mlA.HomeOdds <= mlA.AwayOdds ? Side.Home : Side.Away),
                new Pick(mlB.Index, mlB.HomeOdds <= mlB.AwayOdds ? Side.Home : Side.Away),
                new Pick(scorerMatchup.Index, MarketSelection.AnytimeScorer(0)),
            };
            (_, double stake) = DemoTicketPolicy.Choose(run); // reuse its stake formula only, not its picks
            run.PlaceTicket(picks, stake);

            director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);
            Assert.AreEqual(1, run.Sweats.Count, "one ticket placed => one sweat session");

            couch.OnInteract(null);
            yield return WaitUntilOrFail(() => SitSpot.Active != null,
                Time.realtimeSinceStartup + 10f, "player never sat down");
            // Let SitSpot's own camera lerp (transitionDuration, set tiny above) finish before this
            // harness's capture pose overwrites it - the lerp coroutine is independent of ours and
            // would otherwise fight this override for a frame or two.
            yield return WaitRealtime(0.25f);

            // Mirrors RoomViewCapture.Capture exactly: stop the live controller writing to the
            // transform, then set the pose and FOV once, directly.
            var controller = Object.FindAnyObjectByType<FirstPersonController>();
            if (controller != null) controller.enabled = false;
            cam.transform.SetPositionAndRotation(SeatedEye,
                Quaternion.LookRotation(TvScreenCenter - SeatedEye, Vector3.up));
            cam.fieldOfView = SeatedFovDeg;

            // One shared wall-clock deadline for every wait below, rather than N independent
            // per-moment timeouts stacking worst-case. Ship pacing across three matches plus a
            // final sequence is comfortably under this in the ordinary case; if it is not, that is
            // itself useful evidence, not a harness bug.
            // 420s, raised from 240. NOT because a sweat costs that much — that was the first
            // diagnosis and it was WRONG. Seed 01 completes in 60s, and at 420s seeds 02-05 still
            // failed with the same messages: the waits were UNSATISFIABLE, not slow (see
            // WaitUntilOrAbsent, which is the actual fix). The larger budget is kept only because
            // the old one was independently too tight for a genuine hang to be distinguishable from
            // a slow sweat. Ship pacing is untouched: the point of this harness is real pacing, so
            // the budget moves, never the clock.
            float deadline = Time.realtimeSinceStartup + 420f;

            // Reference frame: the ticket card, before any event has fired. Not a named "moment" in
            // the evidence-question sense - just an anchor a reviewer can orient the rest against.
            yield return CaptureBurst(screen, cam, "sat-down", 1, 0f);

            // ---- Moment: goal payoff. RevealedView.ScoreText is the TV's own causal mirror
            // ("Read-only presentation data copied from the TV's own visible chrome" —
            // TvSweatScreen.cs's RevealedView doc) and changes exactly when UpdateScorebug runs
            // after a real goal lands on ANY of the three matchups in this sweat (see
            // TvSweatScreen.OnGoalPlayed). The same OnGoalPlayed call also sets the Flavor text to
            // the scorer's surname in the same frame, so this one trigger doubles as the "scorer
            // reveal" evidence question for every non-AnytimeScorer leg - the burst below shows both.
            string baselineScore = screen.RevealedView.ScoreText;
            // Same treatment: a sweat can legitimately end without a goal on any leg. The old form
            // failed the whole capture for an outcome the surface is supposed to be able to show.
            yield return WaitUntilOrAbsent(() => screen.RevealedView.ScoreText != baselineScore,
                director, deadline, "no goal landed on any of the 3 legs");
            if (s_conditionMet) yield return CaptureBurst(screen, cam, "goal", 8, 0.15f);

            // ---- Moment: cash-out becomes actionable. SitSpot.InteractStandSuppressed is the
            // exact predicate TvSweatScreen wires to CanAcceptCashOutNow (CashOutLive) - the same
            // signal TvSweatScreenTests' TVS-H01 regressions assert means "an open legal offer must
            // reserve Interact for acceptance" (VISUAL-DESIGN.md §8.5). This is the NEXT occurrence
            // observed after the goal capture above, not necessarily the chronological first (the
            // window opens and closes repeatedly across a sweat) - still a legitimate, deterministic
            // "cash-out is live right now" capture.
            yield return WaitUntilOrAbsent(
                () => SitSpot.InteractStandSuppressed != null && SitSpot.InteractStandSuppressed(),
                director, deadline, "cash-out never became actionable");
            if (s_conditionMet) yield return CaptureBurst(screen, cam, "cashout-actionable", 8, 0.15f);

            // ---- Moment(s): the AnytimeScorer leg's own live window turns dangerous. Fires once
            // per dangerous beat (RevealedView.MarketSuspended false -> true) while THAT leg's row
            // is Live - capped so a leg with several near-misses/chances does not flood the folder.
            // This harness does not classify which of these is the eventual final whistle (vs. an
            // earlier near-miss/chance beat); the last one captured before the "resolved" moment
            // below is it, by construction - read them in frame-index order.
            const int MaxDangerousBeats = 3;
            int dangerousBeatsCaptured = 0;
            bool prevSuspended = screen.RevealedView.MarketSuspended;
            while (Time.realtimeSinceStartup < deadline && dangerousBeatsCaptured < MaxDangerousBeats)
            {
                RevealedTicket ticket = FirstTicketOrNull(screen);
                RevealedLegState? legState = ticket != null && ScorerLegIndex < ticket.Legs.Count
                    ? ticket.Legs[ScorerLegIndex].State : (RevealedLegState?)null;
                bool legLive = legState == RevealedLegState.Live;
                bool legDone = legState.HasValue && legState != RevealedLegState.Live
                    && legState != RevealedLegState.Pending;
                if (legDone) break; // the leg resolved without another dangerous beat to catch

                bool suspendedNow = screen.RevealedView.MarketSuspended;
                if (legLive && suspendedNow && !prevSuspended)
                {
                    yield return CaptureBurst(screen, cam,
                        $"scorer-leg-dangerous-{dangerousBeatsCaptured}", 8, 0.15f);
                    dangerousBeatsCaptured++;
                }
                prevSuspended = suspendedNow;
                yield return null;
            }

            // ---- Moment: the AnytimeScorer leg's own final whistle has landed - guaranteed to
            // occur exactly once, independent of whether the picked player actually scored (PRD
            // §4.1: "no scorer identity on a losing pick"; SweatPresentationModel.BindAnytimeScorer's
            // own doc: "A Lost (or Voided) leg binds nothing"). Captures whichever the deterministic
            // sim under CaptureSeed produced - the ticket-column row reading SCORED and the routed
            // dot ("which dot takes the final touch" - the locator evidence question), or the row
            // resolving straight to L with no identity ever shown. This harness does not know or
            // predict which in advance; it only reaches the moment and captures it.
            yield return WaitUntilOrAbsent(() =>
            {
                RevealedTicket ticket = FirstTicketOrNull(screen);
                RevealedLegState? legState = ticket != null && ScorerLegIndex < ticket.Legs.Count
                    ? ticket.Legs[ScorerLegIndex].State : (RevealedLegState?)null;
                return legState.HasValue && legState != RevealedLegState.Live
                    && legState != RevealedLegState.Pending;
            }, director, deadline, "the scorer leg never reached a terminal state");
            if (s_conditionMet) yield return CaptureBurst(screen, cam, "scorer-leg-resolved", 8, 0.15f);

            Debug.Log($"[TvSweatCaptureHarness] seed={_seed} capture complete -> {OutputDir}");
        }

        // ---------------------------------------------------------------- capture

        /// <summary>Captures <paramref name="frameCount"/> frames of <paramref name="cam"/>,
        /// <paramref name="intervalSeconds"/> of real wall-clock time apart (0 = back-to-back, no
        /// wait), named to encode seed + moment + frame index (self-describing per the brief).
        /// Logs the RevealedView's own display state alongside each frame - factual telemetry, not
        /// a legibility judgment, so a reviewer can cross-check the PNG against what the TV's own
        /// causal mirror says it was showing at that instant.</summary>
        private static IEnumerator CaptureBurst(TvSweatScreen screen, Camera cam, string momentName,
            int frameCount, float intervalSeconds)
        {
            s_sceneIndex++; // one index per captured moment, in the order the sweat played them
            for (int i = 0; i < frameCount; i++)
            {
                // T26: every frame names its own scene grammar and carries a scene index. The
                // refusal was that "nothing that distinguishes one scene grammar from another is
                // visible" and the bundle "carries no scene index, no per-frame grammar label" — a
                // set whose whole claim is variation cannot be reviewed without an index saying
                // which frame is which grammar. Read from the surface's own PRD §9 diagnostic, so
                // the label is the grammar the stage PLAYED, not one a re-run might disagree about.
                string grammar = string.IsNullOrEmpty(screen.DebugSceneTemplate)
                    ? "none" : screen.DebugSceneTemplate;
                // C8·a: the boost rides in the filename so a frame is self-evidencing. The first A/B
                // was undeliverable because nothing in the image said which arm it was.
                string boost = screen.DebugHdrBoostL4.ToString("0.0", CultureInfo.InvariantCulture);
                string file = $"seed-{_seed}__boost{boost}__scene{s_sceneIndex:000}__grammar-{grammar}" +
                              $"__moment-{momentName}__frame{i:000}.png";
                string path = Path.Combine(OutputDir, file);
                CaptureCamera(cam, path, CaptureWidth, CaptureHeight);
                Debug.Log($"[TvSweatCaptureHarness] {file} :: score='{screen.RevealedView.ScoreText}' " +
                    $"clock='{screen.RevealedView.ClockText}' suspended={screen.RevealedView.MarketSuspended}");

                if (i < frameCount - 1)
                {
                    if (intervalSeconds > 0f) yield return WaitRealtime(intervalSeconds);
                    else yield return null;
                }
            }
        }

        /// <summary>RenderTexture + ReadPixels, exactly the path CaptureHarnessSpike proved works
        /// in batch PlayMode with graphics — never <c>ScreenCapture</c>, which depends on a
        /// backbuffer this process may not own.</summary>
        private static void CaptureCamera(Camera cam, string path, int width, int height)
        {
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
            RenderTexture prevTarget = cam.targetTexture;
            RenderTexture prevActive = RenderTexture.active;
            Texture2D tex = null;
            try
            {
                cam.targetTexture = rt;
                cam.Render();

                RenderTexture.active = rt;
                tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                tex.Apply();

                File.WriteAllBytes(path, tex.EncodeToPNG());

                // Plumbing-level check only (mirrors the spike's own "no PNG at all" guard) - never
                // a claim about whether the frame looks right.
                Assert.IsTrue(File.Exists(path), $"no PNG was written at all: {path}");
                Assert.Greater(new FileInfo(path).Length, 256L,
                    $"PNG exists but is implausibly small - likely an empty frame: {path}");
            }
            finally
            {
                cam.targetTexture = prevTarget;
                RenderTexture.active = prevActive;
                if (tex != null) Object.DestroyImmediate(tex);
                rt.Release();
                Object.DestroyImmediate(rt);
            }
        }

        // ---------------------------------------------------------------- helpers

        private static RevealedTicket FirstTicketOrNull(TvSweatScreen screen)
            => screen.RevealedView.Tickets.Count > 0 ? screen.RevealedView.Tickets[0] : null;

        private static IEnumerator LoadRoom()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("Room", LoadSceneMode.Single);
            Assert.IsNotNull(load, "Room scene not in build settings - run SBR.GrayboxRoomBuilder.Build first.");
            while (!load.isDone) yield return null;
        }

        // Waits are wall-clock, not frame-count: batch mode runs unthrottled (thousands of fps),
        // so a frame-count budget starves anything driven by real time (TvSweatScreenTests' own
        // documented lesson from M3).
        private static IEnumerator WaitUntilOrFail(Func<bool> cond, float deadlineRealtime, string failMessage)
        {
            while (!cond())
            {
                if (Time.realtimeSinceStartup > deadlineRealtime)
                {
                    Assert.Fail($"{failMessage} (deadline reached)");
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
