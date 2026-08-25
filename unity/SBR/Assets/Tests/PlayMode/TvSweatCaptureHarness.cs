using System;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using TMPro;
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
                if (SweatEnded(director))
                {
                    // GRACE PERIOD, and it is load-bearing. The phase leaves Sweat BEFORE the revealed
                    // leg states settle, so bailing the instant it flips skipped scorer-leg-resolved
                    // on a seed where it demonstrably does occur. A wait that silently drops a real
                    // moment is worse than the deadline failure it replaced, because the evidence goes
                    // missing instead of the run going red.
                    //
                    // So the phase change means "stop waiting indefinitely", not "the answer is no".
                    // Keep polling the condition for a bounded settle window and only then call it
                    // absent. This matters MORE under T61's contract than it did before: a surviving
                    // ticket settles at FinishSweat, i.e. at the very edge of the phase change, so the
                    // legitimate resolution and the stop signal now arrive within a frame of each
                    // other. Without this window the harness would miss exactly the moment it exists
                    // to photograph, on precisely the seeds that win.
                    for (int i = 0; i < 120; i++) // ~2s at 60fps
                    {
                        if (until()) { s_conditionMet = true; yield break; }
                        yield return null;
                    }
                    Debug.Log($"[TvSweatCaptureHarness] seed={_seed}: {what} — ABSENT, the run left "
                        + "Sweat and the condition did not settle within the grace window. Recorded "
                        + "as an outcome of this sweat, not a failed run.");
                    yield break;
                }
                yield return null;
            }
            Assert.Fail($"{what} — deadline reached with the run STILL IN Phase.Sweat, which is a "
                + "genuine hang. Under T61's contract this no longer confuses a settled ticket for a "
                + "stall: the phase is the completion signal, so if it has not moved, the sweat "
                + "really has not ended.");
        }

        /// <summary>T61's contract, at the one place this harness depends on it: **completion is a
        /// property of the RUN's phase, never of any one ticket or session.** Phase leaves
        /// <see cref="Phase.Sweat"/> exactly once, after every session is drained.
        ///
        /// <para>This replaces <c>CurrentSession.IsComplete</c>, and the reason is worth keeping,
        /// because the naive fix is wrong in a way that hides itself. The markets lead tested the
        /// hypothesis this harness's own finding proposed and found it HALF right — the wrong half
        /// being the useful one:</para>
        ///
        /// <list type="bullet">
        /// <item>A ticket that <b>dies</b> is marked Lost the moment its leg dies → a poller keyed on
        /// the ticket gets an early false "done".</item>
        /// <item>A ticket that <b>survives</b> stays Open until FinishSweat → the same poller gets
        /// <b>no signal at all</b>.</item>
        /// </list>
        ///
        /// <para>So whether ticket 0 is terminal mid-sweat depends on the OUTCOME, not the position —
        /// which is exactly why this harness failed on four seeds and passed on one, from the same
        /// code. <b>A green re-run would therefore have proved nothing</b>: a seed that happens to
        /// lose looks fixed while the defect is untouched. That is why this is a contract taken from
        /// a test rather than a re-run taken as evidence.</para>
        ///
        /// <para>Also confirmed there, and the reason this must never key on a captured reference:
        /// <c>Run.Tickets</c> is the current round's working set and is <b>cleared at ExitShop</b>, so
        /// a held <c>Tickets[0]</c> becomes a permanently terminal object while <c>Tickets[0]</c>
        /// itself names a different, open ticket.</para>
        ///
        /// <para>Pinned by <c>T61_sweat_completion_is_a_phase_property_not_a_ticket_property</c> in
        /// the EditMode suite; proven engine-side by markets' <c>SweatPollingContractTests</c>.</para></summary>
        internal static bool SweatEnded(RunDirector director)
            => director == null || director.Run == null || director.Run.Phase != Phase.Sweat;

        /// <summary>Undoes the frame-lock. Load-bearing, not tidiness: <c>Time.captureDeltaTime</c> is
        /// global and persists for the whole editor session, so leaving it set would silently put
        /// every later PlayMode test on a synthetic clock — the kind of cross-suite contamination
        /// that is very hard to attribute once it bites. Runs after every case, pass or fail.</summary>
        [TearDown]
        public void ReleaseFrameLock()
        {
            Time.captureDeltaTime = 0f;
            TheaterStage.PresentationSeedOverride = null;
        }

        /// <summary>A seed derived from the capture seed string that is stable ACROSS PROCESSES.
        /// Deliberately not <c>string.GetHashCode</c>: .NET randomises that per process, so two arms
        /// shot in separate editor runs would seed differently and the frame-lock would silently do
        /// nothing — the exact failure this fix exists to remove. FNV-1a, same shape as
        /// TheaterPalette's.</summary>
        private static int StableSeed(string s)
        {
            unchecked
            {
                uint h = 2166136261;
                foreach (char c in s) { h ^= c; h *= 16777619; }
                return (int)(h & 0x7FFFFFFF);
            }
        }

        // Leg 2 (0-based) of the fixed ticket built below is always the AnytimeScorer leg.
        private const int ScorerLegIndex = 2;

        // ANCHORED TO Application.dataPath, NOT Directory.GetCurrentDirectory(). The process's
        // working directory is not a stable base for an output path — Unity's batchmode cwd
        // happens to be the project path (unity/SBR) TODAY, but that is a property of how this
        // particular launcher invokes Unity, not a guarantee; a run launched with a different cwd
        // would silently write frames somewhere else. This lane already paid for that once: a poll
        // watched <repo>/artifacts and reported files=0 for a run that was writing frames the whole
        // time — they were landing one level down, at unity/SBR/artifacts/tv-sweat-capture, purely
        // because that run's cwd happened to be the project path.
        //
        // unity/SBR/artifacts/tv-sweat-capture is a DELIBERATE CLAIM of this location, not an
        // accident of cwd: 1,300+ frames and every docked evidence set already live here, so the
        // destination is kept exactly where it has always been rather than re-pointed. It is only
        // the DEPENDENCY on cwd that is being killed.
        //
        // Application.dataPath is <repo>/unity/SBR/Assets, so ONE level up ("..") lands at
        // <repo>/unity/SBR — NOT three levels, which is what SureThingVisualCaptureTests.cs uses to
        // reach the repo ROOT for artifacts/surething-ui. That harness needs to climb one level
        // further out than this one does; copying its "..", "..", ".." blindly would land this
        // harness's frames at <repo>/artifacts/tv-sweat-capture, one directory short of every
        // existing frame. Pinned in TvSweatScreenTests.cs (this class is disposable, opt-in
        // evidence infrastructure — the pin lives where it actually runs).
        internal static string OutputDir =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", "artifacts", "tv-sweat-capture"));

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
        /// <summary>Batch-22 evidence, one shoot: the G1 statement fit, and BOTH payoff beats.
        ///
        /// <para>Separate from the named-moments sweat because none of these three states occur in
        /// it. The sweat photographs what the match happens to produce; these are states that have
        /// to be ENTERED. The accept beat needs a press, WinBeat needs the ticket to win, and G1's
        /// case is the LONGEST statement each market can produce — none of which a seed reliably
        /// hands you.</para>
        ///
        /// <para>The two payoff beats are invoked directly, which is the point rather than a
        /// shortcut: the DD required both siblings shot, and inferring one from the other is the
        /// error that produced T68. They are driven through the production coroutines, so what is
        /// photographed is the shipped beat, not a re-staging of it.</para>
        ///
        /// <para>Bursts run at <c>intervalSeconds: 0</c> — one capture per rendered frame, each
        /// advancing the sim by <c>captureDeltaTime</c> (1/50s). That is what makes "is the ground
        /// static ACROSS the beat" answerable: the frames are a time series through the flood's
        /// pulse, not samples at an arbitrary instant. C35/V8 is a per-beat property and a single
        /// frame cannot report it.</para></summary>
        [Explicit("Batch-22 evidence capture: statement fit + both payoff beats. Run by filter only.")]
        [Timeout(480000)]
        [UnityTest]
        public IEnumerator Capture_Batch22_StatementFit_And_PayoffBeats()
        {
            _seed = "48151623";
            s_sceneIndex = 0;
            Directory.CreateDirectory(OutputDir);
            TheaterStage.PresentationSeedOverride = StableSeed(_seed);
            Time.captureDeltaTime = 1f / 50f; // frame-locked arms, per the named-moments rationale

            yield return LoadRoom();

            var director = Object.FindAnyObjectByType<RunDirector>();
            var screen = Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director, "RunDirector missing - run SBR.GrayboxRoomBuilder.Build first.");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");
            Camera cam = Camera.main;
            Assert.IsNotNull(cam, "MainCamera (PlayerCamera) missing");

            screen.TimeScaleOverride = 1f;
            couch.transitionDuration = 0.01f;
            yield return WaitUntilOrFail(() => director.Run != null,
                Time.realtimeSinceStartup + 10f, "director never started a run");
            director.StartNewRun(_seed);
            Run run = director.Run;

            // ---- G1: one leg per market kind, on the LONGEST names this slate offers.
            //
            // "Shoot the at-budget forms specifically — the longest statement each market can
            // produce." The variable half is the club or player name, so the ticket is built against
            // whichever matchup carries the longest SHORT name — the same shortening the statement
            // itself applies, so this picks the longest RENDERED form rather than the longest raw one.
            //
            // SCOPE, and it is stated on the frames too: this is the longest the SEED offers, not the
            // longest the generator can produce. A worst case beyond this slate is not photographed.
            var byClub = new List<Matchup>(run.CurrentSlate.Matchups);
            byClub.Sort((x, y) => LongestClub(y).CompareTo(LongestClub(x)));
            Assert.GreaterOrEqual(byClub.Count, 3, "need 3 matchups for a one-leg-per-market ticket");

            // One leg per matchup: a ticket may not carry two legs on the same fixture, so the
            // markets are spread across DISTINCT matchups rather than stacked on the longest one.
            Matchup longestMl = byClub[0];
            // LEG ORDER IS THE INSTRUMENT HERE, not a detail. The live row is the FIRST unresolved
            // leg, so whichever market sits at index 0 is the only one whose requirement/state pair
            // composes on screen. Every other market renders as a compact row and its pair is never
            // seen at all.
            //
            // Learned by shooting it wrong: a first pass put moneyline first and BTTS third, and the
            // BTTS pair — the one a ruling was waiting on — simply never appeared. The run was green
            // and the frames were fine; the wanted state was just not in them. **To photograph a
            // market's PAIR, put that market at index 0.** BTTS holds it here because its resolved
            // line is the open question; move it to shoot a different market's pair.
            var picks = new List<Pick>
            {
                new Pick(byClub[2].Index, MarketSelection.BothTeamsToScore(yes: false)), // ONE TEAM SCORELESS
                new Pick(longestMl.Index, longestMl.HomeOdds <= longestMl.AwayOdds ? Side.Home : Side.Away),
                new Pick(byClub[1].Index, MarketSelection.TotalGoals(2.5, over: true)),
            };
            // The scorer leg needs a FOURTH matchup with a roster. If the slate has none, the ticket
            // ships three markets and the scorer form is simply not in this set — stated rather than
            // silently dropped, because a missing market reads as a passing one otherwise.
            Matchup scorer = null;
            for (int i = 3; i < byClub.Count; i++)
                if (byClub[i].Away.Players.Count + byClub[i].Home.Players.Count > 0) { scorer = byClub[i]; break; }
            if (scorer != null)
                picks.Add(new Pick(scorer.Index, MarketSelection.AnytimeScorer(LongestPlayerIndex(scorer))));
            else
                Debug.LogWarning("[Batch22] no fourth matchup with a roster — AnytimeScorer not in this set");
            (_, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);

            couch.OnInteract(null);
            yield return WaitUntilOrFail(() => SitSpot.Active != null,
                Time.realtimeSinceStartup + 10f, "player never sat down");
            yield return WaitRealtime(0.25f);
            var controller = Object.FindAnyObjectByType<FirstPersonController>();
            if (controller != null) controller.enabled = false;
            cam.transform.SetPositionAndRotation(SeatedEye,
                Quaternion.LookRotation(TvScreenCenter - SeatedEye, Vector3.up));
            cam.fieldOfView = SeatedFovDeg;

            // The column with every market present: four compact statements at their measured fit.
            yield return CaptureBurst(screen, cam, "g1-column-all-markets", 2, 0.2f);

            // Let the sweat run so the live row composes a real requirement/state pair (T70-am).
            float deadline = Time.realtimeSinceStartup + 90f;
            string baseline = screen.RevealedView.ScoreText;
            yield return WaitUntilOrAbsent(() => screen.RevealedView.ScoreText != baseline,
                director, deadline, "no event landed to compose a live pair against");
            if (s_conditionMet) yield return CaptureBurst(screen, cam, "t70am-live-pair", 4, 0.25f);

            // ---- T68-am: the accept beat, sampled ACROSS the flood pulse.
            IEnumerator accept = (IEnumerator)typeof(TvSweatScreen)
                .GetMethod("CashOutFloodBeat", System.Reflection.BindingFlags.NonPublic
                                             | System.Reflection.BindingFlags.Instance)
                .Invoke(screen, new object[] { 199.0 });
            screen.StartCoroutine(accept);
            yield return CaptureBurst(screen, cam, "t68am-accept-slot", 30, 0f);

            // ---- T71: the win tally, the same treatment on its sibling beat.
            yield return WaitRealtime(0.5f);
            IEnumerator win = (IEnumerator)typeof(TvSweatScreen)
                .GetMethod("WinBeat", System.Reflection.BindingFlags.NonPublic
                                    | System.Reflection.BindingFlags.Instance)
                .Invoke(screen, null);
            screen.StartCoroutine(win);
            yield return CaptureBurst(screen, cam, "t71-win-tally-slot", 30, 0f);
        }

        /// <summary>Length of the longer SHORT club name in a matchup — the shortening the statement
        /// itself applies, so this ranks by rendered length rather than raw length.</summary>
        private static int LongestClub(Matchup m)
            => Mathf.Max(SweatFlavor.Short(m.Away.Name).Length, SweatFlavor.Short(m.Home.Name).Length);

        /// <summary>Index of the roster player with the longest SURNAME — G1 names players by
        /// surname, so the worst case for `{SURNAME} TO SCORE` is the longest surname, not the
        /// longest full name.</summary>
        private static int LongestPlayerIndex(Matchup m)
        {
            int best = 0, bestLen = -1;
            for (int i = 0; i < m.Away.Players.Count; i++)
            {
                string n = m.Away.Players[i].Name ?? string.Empty;
                int cut = n.LastIndexOf(' ');
                int len = (cut >= 0 ? n.Substring(cut + 1) : n).Length;
                if (len > bestLen) { bestLen = len; best = i; }
            }
            return best;
        }

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

            // FRAME-LOCK (2026-08-04), so two arms of an A/B can be diffed per-pixel.
            //
            // The T49 bloom pair could not answer the question it was shot for. Its arms did not
            // share sim state — actors sat in different places at the same seed, scene, grammar and
            // frame index — so the whole-frame diff measured actors that had moved, not bloom. The DD
            // fell back to fixed-box region statistics and recorded the limitation: "an A/B whose
            // arms are not frame-locked cannot support a per-pixel comparison," noting that the
            // larger, more impressive-looking number was the invalid one.
            //
            // Three things had to be pinned, and all three are presentation-local — the ENGINE was
            // always deterministic from the run seed, which is why the sweat's events already
            // matched across arms while its pixels did not:
            //   1. TheaterStage's presentation RNG, previously salted from Environment.TickCount.
            //   2. The idle emission flicker's phase (TvSweatScreen reads the same override).
            //   3. Time.deltaTime itself. Pinning the RNG makes both arms take the same DECISIONS;
            //      it does not make them integrate the same MOTION, because a real frame time varies
            //      run to run. captureDeltaTime replaces the wall clock with a fixed step, so the
            //      sim advances identically per rendered frame.
            //
            // Ship pacing is preserved in the sense that matters: the sweat still plays out over the
            // same number of SIMULATED seconds at the same per-frame step. What changes is that
            // those seconds are no longer tied to how long the machine took to render them — which is
            // the property that makes a capture reproducible. Named consequence, stated because it
            // works against the budget above: wall-clock cost per simulated second now depends on
            // render speed, and these are 2560x1440 frames.
            TheaterStage.PresentationSeedOverride = StableSeed(seed);
            Time.captureDeltaTime = 1f / 50f;

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

            // BUDGET PARTITION (2026-08-04). This loop is OPPORTUNISTIC — it gathers as many
            // dangerous beats as the sweat happens to offer — while the scorer-leg wait below is a
            // NAMED moment the set is expected to contain. Sharing one wall-clock deadline let the
            // opportunistic collector starve the named one completely: measured on the T49 A/B, the
            // four failing seeds captured ZERO dangerous beats, so this loop ran from entry to the
            // 420s wall doing nothing, and the wait below then began with its deadline already gone
            // and failed instantly with "the session is still LIVE".
            //
            // An opportunistic collector must never be able to consume a named moment's budget.
            // The total stays 420s — it must, or the NUnit [Timeout] fires first and the harness's
            // own diagnostic message is replaced by an opaque framework kill (see the note above
            // the attribute). So the slice is reserved, not added.
            const float ScorerWaitFloorSeconds = 150f;
            float dangerousDeadline = deadline - ScorerWaitFloorSeconds;
            string dangerousExit = "budget";
            while (Time.realtimeSinceStartup < dangerousDeadline && dangerousBeatsCaptured < MaxDangerousBeats)
            {
                RevealedTicket ticket = FirstTicketOrNull(screen);
                RevealedLegState? legState = ticket != null && ScorerLegIndex < ticket.Legs.Count
                    ? ticket.Legs[ScorerLegIndex].State : (RevealedLegState?)null;
                bool legLive = legState == RevealedLegState.Live;
                bool legDone = legState.HasValue && legState != RevealedLegState.Live
                    && legState != RevealedLegState.Pending;
                if (legDone) { dangerousExit = "leg resolved"; break; }

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
            if (dangerousBeatsCaptured >= MaxDangerousBeats) dangerousExit = "cap reached";
            // C18 — the collector states what it did and what it therefore cannot show. "budget"
            // means it was cut short with time reserved for the named moment below, so a scorer-leg
            // failure after this line is about the SWEAT, not about this loop having eaten the clock.
            Debug.Log($"[TvSweatCaptureHarness] seed={_seed}: dangerous-beat collector exited on "
                + $"{dangerousExit} with {dangerousBeatsCaptured}/{MaxDangerousBeats} captured; "
                + $"{Mathf.Max(0f, deadline - Time.realtimeSinceStartup):0.#}s left for the scorer wait "
                + $"(floor {ScorerWaitFloorSeconds:0}s).");

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
        /// <summary>T99 (batch 79) — THE STATS PANEL OVER A NON-LEVEL SCOREBUG.
        ///
        /// <para><b>The one binding condition is a refusal to shoot at 0–0</b>: a stats panel over a
        /// goalless scorebug proves nothing, because the covered scorebug is carrying no information
        /// and so no reading of it can fail. This waits for a REVEALED non-level score with at least
        /// one goal and <b>fails loudly rather than shooting the wrong thing</b> — the shape the
        /// goalless set used when it asserted its 0–0 at lock.</para>
        ///
        /// <para><b>Three bursts, because check 4 is a COMPARISON, not a state.</b> "On close the
        /// scorebug returns with its values unchanged" cannot be read off a single set: it needs the
        /// band before, the panel over it, and the band after. The closed bursts bracket the open
        /// one, so the frames answer it without a second instrument — and the harness asserts the
        /// same equality, so a drift that the eye might forgive still fails the run.</para>
        ///
        /// <para><b>Frame-contiguous (interval 0) is the control.</b> `Time.captureDeltaTime` ties
        /// SIM time to RENDERED frames, so a burst spaced in realtime advances the match by however
        /// many frames the host happened to render — which produced four passing captures of the
        /// wrong beat in this lane. It also matters more than usual here: the panel's whole claim is
        /// that time is STOPPED, and a set that let the match move between frames would be arguing
        /// against itself.</para></summary>
        /// <summary>T100 (batch 85) — THE PANEL WITH A POPULATED COUNT ROW.
        ///
        /// <para>T99's set shot the panel on a MONEYLINE leg, so `CORNERS` and `CARDS` carried the
        /// unrevealed mark. That is ruled behaviour and not a defect, but it left two of three rows
        /// in their empty form, and <b>a panel judged on that state would be judged on its thinnest
        /// possible content</b>. So the composition was raised and deliberately not ruled.</para>
        ///
        /// <para><b>Same discipline as T99's own 0–0 condition: a surface shot in its emptiest state
        /// cannot be read for how it FILLS.</b> This ticket carries a CORNERS leg, and the run waits
        /// for the count ledger to have revealed something before it shoots.</para>
        ///
        /// <para><b>The selection is READ OFF THE BOARD, never constructed.</b> The corners line is
        /// generated per matchup, so an invented `TotalCorners(9.5, over)` would be a selection this
        /// matchup may not offer — the exact class of error that had this lane withdraw three
        /// findings built on strings the surface cannot emit. The pick takes an offer that exists.</para></summary>
        [Explicit("T100 (batch 85) evidence capture: the stats panel with a populated count row. Run by filter only.")]
        [UnityTest]
        public IEnumerator Capture_StatsPanel_WithAPopulatedCountRow()
        {
            _seed = "STATS-COUNT-1";
            s_sceneIndex = 0;
            Directory.CreateDirectory(OutputDir);

            TheaterStage.PresentationSeedOverride = StableSeed(_seed);
            Time.captureDeltaTime = 1f / 50f;

            yield return LoadRoom();

            var director = Object.FindAnyObjectByType<RunDirector>();
            var screen = Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director, "RunDirector missing - run SBR.GrayboxRoomBuilder.Build first.");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");

            Camera cam = Camera.main;
            Assert.IsNotNull(cam, "MainCamera (PlayerCamera) missing - cannot capture without it");

            screen.TimeScaleOverride = 1f;
            couch.transitionDuration = 0.01f;

            yield return WaitUntilOrFail(() => director.Run != null,
                Time.realtimeSinceStartup + 10f, "director never started a run");

            director.StartNewRun(_seed);
            Run run = director.Run;
            Assert.AreEqual(Phase.Betting, run.Phase, "a fresh run opens in Betting");

            // Take an OFFERED corners selection off the board rather than constructing a line.
            int countMatchupIndex = -1;
            MarketSelection countSelection = default;
            foreach (Matchup mm in run.CurrentSlate.Matchups)
            {
                foreach (MarketOffer off in mm.Markets)
                {
                    if (off.Selection.Kind != MarketKind.TotalCorners) continue;
                    countMatchupIndex = mm.Index;
                    countSelection = off.Selection;
                    break;
                }
                if (countMatchupIndex >= 0) break;
            }
            Assert.GreaterOrEqual(countMatchupIndex, 0,
                "no matchup on this slate offers TotalCorners - T100 needs a COUNT leg, so this is a "
                + "re-seed rather than a reason to shoot the moneyline state again");

            const double Stake = 25.0;
            run.PlaceTicket(new List<Pick> { new Pick(countMatchupIndex, countSelection) }, Stake);
            director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);

            couch.OnInteract(null);
            yield return WaitUntilOrFail(() => SitSpot.Active != null,
                Time.realtimeSinceStartup + 15f, "player never sat down");

            // T100's BINDING CONDITION. -1 means there is no count ledger at all (not a count leg);
            // 0/0 means a count leg that has revealed nothing YET. Only a revealed count fills a row,
            // and only a filled row can be read for how the table composes.
            yield return WaitUntilOrFail(
                () => screen.DebugRevealedCountHome >= 0
                      && screen.DebugRevealedCountHome + screen.DebugRevealedCountAway > 0,
                Time.realtimeSinceStartup + 300f,
                "the corners leg never revealed a count. T100 exists because the emptiest state "
                + "cannot be read for how the panel FILLS, so this is a re-seed, never a reason to "
                + "shoot the empty form a second time.");

            int cHome = screen.DebugRevealedCountHome, cAway = screen.DebugRevealedCountAway;
            Debug.Log($"[TvSweatCaptureHarness] T100 condition met: corners {cHome}-{cAway} "
                + $"score='{screen.RevealedView.ScoreText}' clock='{screen.RevealedView.ClockText}'");

            Assert.IsFalse(screen.DebugStatsPanelOpen, "precondition: the panel starts closed");
            yield return CaptureBurst(screen, cam, "countrow-closed-before", 20, 0f);

            screen.ForceStatsPanel(true);
            yield return null;
            Assert.IsTrue(screen.DebugStatsPanelOpen, "the panel did not open - nothing below is the shot");

            // THE ROW IS ACTUALLY POPULATED, asserted before the frames are spent. A set shot on a
            // row still carrying the mark would be T99's set again under a new name.
            string cornersRow = screen.DebugStatsRow(1);
            string mark = screen.DebugStatsUnrevealedMark;
            Assert.AreNotEqual($"CORNERS|{mark}|{mark}", cornersRow,
                $"T100 needs a POPULATED corners row and this one reads '{cornersRow}'");
            Debug.Log($"[TvSweatCaptureHarness] T100 rows :: '{screen.DebugStatsRow(0)}' :: "
                + $"'{cornersRow}' :: '{screen.DebugStatsRow(2)}'");

            string clockAtOpen = screen.RevealedView.ClockText;
            yield return CaptureBurst(screen, cam, "countrow-open", 30, 0f);
            Assert.AreEqual(clockAtOpen, screen.RevealedView.ClockText,
                "T99's standing condition holds here too: the match clock must not advance behind "
                + "the panel");

            screen.ForceStatsPanel(false);
            yield return null;
            yield return CaptureBurst(screen, cam, "countrow-closed-after", 20, 0f);

            Assert.AreEqual(cHome, screen.DebugRevealedCountHome,
                "the revealed corners count must be unchanged across the overlay");
            Assert.AreEqual(cAway, screen.DebugRevealedCountAway,
                "the revealed corners count must be unchanged across the overlay");
        }

        /// <summary>DD batch 93 — THE PANEL WITH A ROW SET BEING SELECTED.
        ///
        /// <para>T100's ticket carries exactly one count leg, so it can only ever prove "a row this
        /// ticket bought can fill in". It cannot prove the ROW SET ITSELF is selected by the ticket
        /// — CORNERS and CARDS both present because the ticket carries both, independent of which
        /// leg happens to be live — because a single-count ticket only ever has one such row to
        /// show. This ticket carries BOTH a TotalCorners leg and a TotalCards leg, on two DIFFERENT
        /// matchups, so the shot proves what T100 structurally cannot.</para>
        ///
        /// <para><b>Same discipline as T99's 0-0 and T100's empty-row conditions:</b> this waits for
        /// at least one of the two counts to have revealed something before it shoots, and fails
        /// loudly rather than shoot a table that is still all mark.</para>
        ///
        /// <para><b>Both selections are READ OFF THE BOARD, never constructed</b> — same reasoning as
        /// T100: the corners/cards line is generated per matchup, so an invented selection may not be
        /// one that matchup actually offers. The cards search explicitly excludes the corners
        /// matchup, so the ticket carries at most one leg per fixture and prices on the ordinary
        /// path — no same-match correlation model enters this capture at all.</para></summary>
        [Explicit("DD batch 93 evidence capture: the stats panel with a multi-count (CORNERS + CARDS) ticket. Run by filter only.")]
        [UnityTest]
        public IEnumerator Capture_StatsPanel_MultiCountTicket()
        {
            _seed = "STATS-MULTI-1";
            s_sceneIndex = 0;
            Directory.CreateDirectory(OutputDir);

            TheaterStage.PresentationSeedOverride = StableSeed(_seed);
            Time.captureDeltaTime = 1f / 50f;

            yield return LoadRoom();

            var director = Object.FindAnyObjectByType<RunDirector>();
            var screen = Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director, "RunDirector missing - run SBR.GrayboxRoomBuilder.Build first.");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");

            Camera cam = Camera.main;
            Assert.IsNotNull(cam, "MainCamera (PlayerCamera) missing - cannot capture without it");

            screen.TimeScaleOverride = 1f;
            couch.transitionDuration = 0.01f;

            yield return WaitUntilOrFail(() => director.Run != null,
                Time.realtimeSinceStartup + 10f, "director never started a run");

            director.StartNewRun(_seed);
            Run run = director.Run;
            Assert.AreEqual(Phase.Betting, run.Phase, "a fresh run opens in Betting");

            // Take an OFFERED corners selection off the board first.
            int cornersMatchupIndex = -1;
            MarketSelection cornersSelection = default;
            foreach (Matchup mm in run.CurrentSlate.Matchups)
            {
                foreach (MarketOffer off in mm.Markets)
                {
                    if (off.Selection.Kind != MarketKind.TotalCorners) continue;
                    cornersMatchupIndex = mm.Index;
                    cornersSelection = off.Selection;
                    break;
                }
                if (cornersMatchupIndex >= 0) break;
            }
            Assert.GreaterOrEqual(cornersMatchupIndex, 0,
                "no matchup on this slate offers TotalCorners - this needs a COUNT leg, so this is a "
                + "re-seed rather than a reason to invent a selection the board did not offer");

            // Then an OFFERED cards selection, from a DIFFERENT matchup — never the same fixture as
            // corners, so this ticket carries at most one leg per matchup and prices on the ordinary
            // path (no same-match correlation model).
            int cardsMatchupIndex = -1;
            MarketSelection cardsSelection = default;
            foreach (Matchup mm in run.CurrentSlate.Matchups)
            {
                if (mm.Index == cornersMatchupIndex) continue;
                foreach (MarketOffer off in mm.Markets)
                {
                    if (off.Selection.Kind != MarketKind.TotalCards) continue;
                    cardsMatchupIndex = mm.Index;
                    cardsSelection = off.Selection;
                    break;
                }
                if (cardsMatchupIndex >= 0) break;
            }
            Assert.GreaterOrEqual(cardsMatchupIndex, 0,
                "no OTHER matchup on this slate offers TotalCards - this needs a second, distinct "
                + "COUNT leg, so this is a re-seed rather than a reason to invent a selection the "
                + "board did not offer");

            const double Stake = 25.0;
            run.PlaceTicket(new List<Pick>
            {
                new Pick(cornersMatchupIndex, cornersSelection),
                new Pick(cardsMatchupIndex, cardsSelection),
            }, Stake);
            director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);

            couch.OnInteract(null);
            yield return WaitUntilOrFail(() => SitSpot.Active != null,
                Time.realtimeSinceStartup + 15f, "player never sat down");

            // THE BINDING CONDITION, same shape as T100's: -1 means no count ledger at all; 0/0 means
            // a count leg that has revealed nothing YET. Only a revealed count fills a row, and only
            // a filled row can be read for how the table composes.
            yield return WaitUntilOrFail(
                () => screen.DebugRevealedCountHome >= 0
                      && screen.DebugRevealedCountHome + screen.DebugRevealedCountAway > 0,
                Time.realtimeSinceStartup + 300f,
                "neither count leg ever revealed anything. Same discipline as T100: this is a "
                + "re-seed, never a reason to shoot the empty form a second time.");

            int cHome = screen.DebugRevealedCountHome, cAway = screen.DebugRevealedCountAway;
            Debug.Log($"[TvSweatCaptureHarness] MultiCountTicket condition met: live count {cHome}-"
                + $"{cAway} score='{screen.RevealedView.ScoreText}' clock='{screen.RevealedView.ClockText}'");

            Assert.IsFalse(screen.DebugStatsPanelOpen, "precondition: the panel starts closed");
            yield return CaptureBurst(screen, cam, "multicount-closed-before", 20, 0f);

            screen.ForceStatsPanel(true);
            yield return null;
            Assert.IsTrue(screen.DebugStatsPanelOpen, "the panel did not open - nothing below is the shot");

            // THE ROW SET IS ACTUALLY SELECTED, asserted before the frames are spent. This is what a
            // single-count ticket structurally cannot show: BOTH rows present because the TICKET
            // carries both leg kinds, never mind which one is currently live.
            string cornersRow = screen.DebugStatsRow(1);
            string cardsRow = screen.DebugStatsRow(2);
            string mark = screen.DebugStatsUnrevealedMark;
            Assert.IsTrue(
                cornersRow != null && cornersRow.StartsWith("CORNERS|")
                && cardsRow != null && cardsRow.StartsWith("CARDS|"),
                "DD batch 93: a multi-count ticket must show BOTH the CORNERS and CARDS rows - this "
                + $"is the whole point of the shot. Got corners='{cornersRow}' cards='{cardsRow}'");
            Assert.IsTrue(cornersRow != $"CORNERS|{mark}|{mark}" || cardsRow != $"CARDS|{mark}|{mark}",
                "T100's own binding condition carried forward: a set shot on rows that BOTH still "
                + $"carry the mark proves nothing. corners='{cornersRow}' cards='{cardsRow}'");
            Debug.Log($"[TvSweatCaptureHarness] MultiCountTicket rows :: '{screen.DebugStatsRow(0)}' "
                + $":: '{cornersRow}' :: '{cardsRow}'");

            string clockAtOpen = screen.RevealedView.ClockText;
            yield return CaptureBurst(screen, cam, "multicount-open", 30, 0f);
            Assert.AreEqual(clockAtOpen, screen.RevealedView.ClockText,
                "T99's standing condition holds here too: the match clock must not advance behind "
                + "the panel");

            screen.ForceStatsPanel(false);
            yield return null;
            yield return CaptureBurst(screen, cam, "multicount-closed-after", 20, 0f);

            Assert.AreEqual(cHome, screen.DebugRevealedCountHome,
                "the revealed count must be unchanged across the overlay");
            Assert.AreEqual(cAway, screen.DebugRevealedCountAway,
                "the revealed count must be unchanged across the overlay");
        }

        // CORRECTED — this comment used to claim "[Explicit] is on the CLASS"; that reading was
        // WRONG, and this fix is what disproves it. There is no class-level [Explicit] anywhere in
        // this file, and never was. What actually happened: a T87-am [Explicit]+[Timeout] pair
        // written for Capture_GoallessDraw_BothTicketsToFullTime sat above three stacked XML
        // doc-comments (its own, then T99's, then T100's) with no real declaration between them —
        // and C# binds attributes to the next actual member declaration regardless of doc-comment
        // trivia in between, so both attributes landed on Capture_StatsPanel_WithAPopulatedCountRow
        // instead of the method they described. The CS0579 that produced the old (wrong) reading was
        // that misbound [Explicit] colliding with a method-level one — not evidence of a class-level
        // attribute. Every capture method in this file now carries its own [Explicit], attached
        // directly to it, so the guard cannot drift by textual adjacency again.
        [Explicit("T99 (batch 79) evidence capture: the stats panel over a non-level scorebug. Run by filter only.")]
        [UnityTest]
        public IEnumerator Capture_StatsPanel_OverANonLevelScorebug()
        {
            _seed = "STATS-1";
            s_sceneIndex = 0;
            Directory.CreateDirectory(OutputDir);

            TheaterStage.PresentationSeedOverride = StableSeed(_seed);
            Time.captureDeltaTime = 1f / 50f;

            yield return LoadRoom();

            var director = Object.FindAnyObjectByType<RunDirector>();
            var screen = Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director, "RunDirector missing - run SBR.GrayboxRoomBuilder.Build first.");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");

            Camera cam = Camera.main;
            Assert.IsNotNull(cam, "MainCamera (PlayerCamera) missing - cannot capture without it");

            screen.TimeScaleOverride = 1f;   // ship pacing
            couch.transitionDuration = 0.01f;

            yield return WaitUntilOrFail(() => director.Run != null,
                Time.realtimeSinceStartup + 10f, "director never started a run");

            director.StartNewRun(_seed);
            Run run = director.Run;
            Assert.AreEqual(Phase.Betting, run.Phase, "a fresh run opens in Betting");

            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);

            couch.OnInteract(null);
            yield return WaitUntilOrFail(() => SitSpot.Active != null,
                Time.realtimeSinceStartup + 15f, "player never sat down");

            // THE BINDING CONDITION. Not "a goal happened" - a NON-LEVEL REVEALED score, which is the
            // fact the covered band would be carrying.
            yield return WaitUntilOrFail(
                () => screen.DebugRevealedPicked != screen.DebugRevealedOpponent
                      && screen.DebugRevealedPicked + screen.DebugRevealedOpponent > 0,
                Time.realtimeSinceStartup + 300f,
                "the sweat never revealed a NON-LEVEL scoreline with at least one goal. T99 forbids "
                + "shooting this at 0-0, so this is a RE-SEED, never a reason to shoot anyway.");

            int pickedAtOpen = screen.DebugRevealedPicked;
            int oppAtOpen = screen.DebugRevealedOpponent;
            Debug.Log($"[TvSweatCaptureHarness] T99 condition met: revealed {pickedAtOpen}-{oppAtOpen} "
                + $"score='{screen.RevealedView.ScoreText}' clock='{screen.RevealedView.ClockText}'");
            Assert.IsFalse(screen.DebugStatsPanelOpen, "precondition: the panel starts closed");

            yield return CaptureBurst(screen, cam, "statspanel-closed-before", 20, 0f);

            screen.ForceStatsPanel(true);
            yield return null;
            Assert.IsTrue(screen.DebugStatsPanelOpen, "the panel did not open - nothing below is the shot");
            string clockAtOpen = screen.RevealedView.ClockText;
            yield return CaptureBurst(screen, cam, "statspanel-open", 30, 0f);

            // THE CLOCK, ASSERTED ACROSS THE OPEN BURST - and this assertion exists because the first
            // run of this capture FAILED it silently. The panel froze the score and the minute ticked
            // 18' -> 21' behind it: the clock advanced on Time.deltaTime while the freeze authority
            // said stop. A covered fact that CAN move is LOST, so the set would have claimed a freeze
            // its own per-frame log disproved. The frames caught what the pin could not, because a
            // channel that never reads the authority is invisible to a pin on the authority.
            Assert.AreEqual(clockAtOpen, screen.RevealedView.ClockText,
                "T99: the match clock must not advance behind the panel. If this fires, a channel has "
                + "stopped reading SeatedDeltaTime - find it by grepping the quantity, not by memory.");

            screen.ForceStatsPanel(false);
            yield return null;
            yield return CaptureBurst(screen, cam, "statspanel-closed-after", 20, 0f);

            // CHECK 4, asserted as well as photographed. The frames are what the DD reads; this is
            // what stops a drift the eye would forgive from riding out in a passing capture.
            Assert.AreEqual(pickedAtOpen, screen.DebugRevealedPicked,
                "T99 check 4: the freeze - the revealed score must be unchanged across the overlay");
            Assert.AreEqual(oppAtOpen, screen.DebugRevealedOpponent,
                "T99 check 4: the freeze - the revealed score must be unchanged across the overlay");
        }

        /// <summary>T87-am: THE SCORELESS DRAW, to full time, with BOTH tickets resolving.
        ///
        /// <para>T87 ruled the drawn beat is the match <b>ending level, stated</b>, and every mechanism
        /// it names is goal-independent — so none is absent at 0–0. What 0–0 changes is the RISK: the
        /// surface has not punched all match, so a quiet ending arrives against a quiet match, and
        /// <b>the one state it must never be mistaken for is idle.</b></para>
        ///
        /// <para><b>And a draw is quiet for the room and LOUD for one ticket.</b> The draw-backer has
        /// won, on a match where nothing happened. That is why both tickets are in one set: the loud
        /// half and the quiet half have to be readable side by side, on the same settlement moment.</para>
        ///
        /// <para><b>No 0–0 full-time frame existed in evidence</b> — the `LEVEL 0–0` readings on hand
        /// were mid-match (11', 32'), the progress line doing its job and saying nothing about the
        /// ending. C11 binds: a claim about how the ending reads is made against a rendered frame of
        /// the ending.</para>
        ///
        /// <para><b>The seed is found, not hoped for.</b> `engine.tests/GoallessDrawSeedTests` searched
        /// 400 seeds through the same path this takes (`new Run(seed)`, default config, exactly what
        /// `RunDirector.StartNewRun` builds) and found eight goalless matches; `GOALLESS-5` matchup 0
        /// is `Atlanta Middlemen 0 – 0 Scranton Mallards`. `LockRound` resolves every game on the
        /// slate whether it was bet or not — <i>"outcomes for a seed are identical no matter what the
        /// player wagered"</i> — so the tickets are placed onto a result that already exists rather
        /// than steering it.</para>
        ///
        /// <para><b>Capture and dock — the read is NOT made here.</b> Three dispositions are
        /// pre-committed at the DD seat and this harness deliberately asserts nothing about how the
        /// ending looks; its assertions are plumbing only, as everywhere else in this file.</para></summary>
        [Explicit("T87-am evidence capture: the goalless draw to full time, both tickets. Run by filter only.")]
        [Timeout(1500000)]
        [UnityTest]
        public IEnumerator Capture_GoallessDraw_BothTicketsToFullTime()
        {
            _seed = "GOALLESS-5";
            s_sceneIndex = 0;
            Directory.CreateDirectory(OutputDir);

            TheaterStage.PresentationSeedOverride = StableSeed(_seed);
            Time.captureDeltaTime = 1f / 50f;
            // T97/T87-am2: the DD asked for every strip write logged with its call site across a
            // LegFinal beat and could not run it. This run answers it.
            TvSweatScreen.TraceFlavorWrites = true;

            yield return LoadRoom();

            var director = Object.FindAnyObjectByType<RunDirector>();
            var screen = Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director, "RunDirector missing - run SBR.GrayboxRoomBuilder.Build first.");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");

            Camera cam = Camera.main;
            Assert.IsNotNull(cam, "MainCamera (PlayerCamera) missing - cannot capture without it");

            screen.TimeScaleOverride = 1f;      // ship pacing — the ending's rhythm is the subject
            couch.transitionDuration = 0.01f;

            yield return WaitUntilOrFail(() => director.Run != null,
                Time.realtimeSinceStartup + 10f, "director never started a run");

            director.StartNewRun(_seed);
            Run run = director.Run;
            Assert.AreEqual(Phase.Betting, run.Phase, "a fresh run opens in Betting");

            // Matchup 0 is the one the seed search found goalless. TWO tickets on it, so the sweat
            // plays them serially and both endings land in one set:
            //   ticket 1 — the DRAW.  It WINS on a match where nothing happened.
            //   ticket 2 — a TEAM.    It LOSES to the same 0-0.
            Matchup m = run.CurrentSlate.Matchups[0];
            Assert.IsNotNull(m, "the goalless matchup is missing from this slate");

            // A PICK ADDRESSES `Matchup.Index`, NOT THE SLATE POSITION, and the first cut of this
            // used the position. They are not guaranteed to coincide, and the harness's other tickets
            // have always used `.Index` for exactly that reason — a pick on the wrong matchup grades
            // against the wrong match, which is how a ticket that backed a 0-0 draw came back LOST.
            int goalless = m.Index;

            // A MODEST, EXPLICIT STAKE rather than DemoTicketPolicy's — its formula sizes ONE bet
            // against the whole bank, and two of them do not both fit a 350 opening bank. The first
            // run of this capture placed one ticket where it meant to place two, and the symptom was
            // silent: the sweat loop simply had nothing to advance to.
            const double Stake = 25.0;
            run.PlaceTicket(new List<Pick> { new Pick(goalless, MarketSelection.MoneylineDraw()) }, Stake);
            run.PlaceTicket(new List<Pick> { new Pick(goalless, Side.Home) }, Stake);
            director.LockRound();

            // TWO tickets is the whole point of the set — the loud half and the quiet half on one
            // settlement. Asserted so "only one sweat played" can never again look like a timeout.
            Assert.AreEqual(2, run.Tickets.Count, "both tickets must be placed — the set needs both halves");
            Assert.AreEqual(2, run.Sweats.Count, "each ticket gets its own sweat");

            // The result is the seed's, not this harness's — asserted so a drifted seed fails loudly
            // here rather than producing a set that quietly shows the wrong ending.
            Assert.IsNotNull(m.StatLine, "the match did not resolve at lock");
            Assert.AreEqual(0, m.StatLine.HomeGoals, $"seed '{_seed}' matchup {goalless} is no longer goalless");
            Assert.AreEqual(0, m.StatLine.AwayGoals, $"seed '{_seed}' matchup {goalless} is no longer goalless");
            Assert.AreEqual(MatchResult.Draw, m.StatLine.Result, "the goalless match must resolve as a draw");

            // The tickets landed on the match this test asserted about — checked, because the whole
            // set is worthless if they graded against a different fixture.
            foreach (Ticket placed in run.Tickets)
                Assert.AreEqual(goalless, placed.Legs[0].Matchup.Index,
                    "a ticket was placed on a different matchup than the goalless one");

            couch.OnInteract(null);
            yield return WaitUntilOrFail(() => SitSpot.Active != null,
                Time.realtimeSinceStartup + 15f, "player never sat down");

            // CAPTURE EVERY FULL TIME AS IT HAPPENS, rather than waiting for each sweat to start.
            //
            // The per-sweat wait was the wrong shape and cost several runs: the round races on once a
            // session completes, so by the time a predicate about sweat N+1 was evaluated the phase
            // had already reached Shop. Watching the CLOCK instead means the trigger is the thing the
            // set is about, and the sweat index is read AT the moment rather than waited for.
            //
            // ONE BURST PER ENDING, contiguous from the whistle. The settle beat is short — shorter
            // than a 12-frame burst at 0.2s — so splitting "full-time" from "settlement" put the
            // second burst on the next sweat's replay at 38'. Both tickets sit on the same matchup, so
            // that replay looks superficially plausible, which is exactly what makes a mistimed burst
            // dangerous. The frames are named for the ENDING and read in frame-index order, which is
            // this harness's own stated convention.
            // THE SUPPLEMENTAL SHOT (batch 69): the docked set asserted T96's LIVE NEED clause —
            // `LEVEL AT FULL TIME` over `LEVEL` — while all 120 frames were SETTLED, so the clause
            // had no frame behind it. Any mid-match frame of a draw-backed leg carries it, and the
            // draw-backer's sweat is the first one, so it costs no extra window.
            float midDeadline = Time.realtimeSinceStartup + 300f;
            yield return WaitUntilOrFail(
                () => MinuteOf(screen.RevealedView.ClockText) >= 30 || director.Run.Phase != Phase.Sweat,
                midDeadline,
                $"the draw-backer's leg never reached a mid-match minute · clock='{screen.RevealedView.ClockText}'");
            if (director.Run.Phase == Phase.Sweat)
                yield return CaptureBurst(screen, cam, "goalless-draw-backer-live-need", 8, 0f);

            int endingsCaptured = 0;
            float runDeadline = Time.realtimeSinceStartup + 900f;
            while (endingsCaptured < 2 && director.Run.Phase == Phase.Sweat)
            {
                yield return WaitUntilOrFail(
                    () => screen.RevealedView.ClockText == "FT" || director.Run.Phase != Phase.Sweat,
                    runDeadline,
                    $"ending {endingsCaptured} never reached full time · " +
                    $"SweatIndex={director.SweatIndex} phase={director.Run?.Phase} clock='{screen.RevealedView.ClockText}'");
                if (director.Run.Phase != Phase.Sweat) break;

                int idx = director.SweatIndex;
                string label = idx == 0 ? "draw-backer" : "team-backer";
                Ticket at = director.CurrentTicket;
                // FRAME-CONTIGUOUS (interval 0), and that is the whole trick here.
                //
                // `Time.captureDeltaTime` ties SIM time to RENDERED frames — 0.02s each — so a burst
                // spaced by REALTIME advances the match by however many frames the host happened to
                // render in that wall-clock gap. At 0.12s spacing the draw-backer's "ending" read
                // FT, then PRE, 11', 30', 55', 74': four frames of the actual whistle and then the
                // whole of the NEXT match. Both tickets sit on the same matchup, so that replay
                // looks superficially plausible — the same trap C50 was promoted for.
                //
                // At interval 0 each capture is one rendered frame, so 60 frames is 1.2 SIM-seconds
                // of contiguous coverage from the whistle forward, and the clock in each frame's own
                // log line says exactly which beat it is rather than the label claiming it.
                // T129 arm 1: 60 -> 150 frames (3.0 sim-seconds at this harness's 50fps capture
                // rate). THE GAP T125 MEASURED: the drawn-ending hold consumes 1.02s of the old
                // 1.2s window, so the win's TALLY, FLOOD and ROOM GLOW all fall OUTSIDE it — the
                // set could show the ending beginning and never whether it resolves.
                //
                // NOTHING ELSE ABOUT THIS ENTRY POINT CHANGES — same seed, same matchup, same
                // stake, same two picks. That is T129's binding condition (a), and it is the whole
                // arm: a re-shoot that moves any other variable is not comparable to the docked set
                // and the point is lost.
                yield return CaptureBurst(screen, cam, $"goalless-{label}-ending", 150, 0f);
                Debug.Log($"[TvSweatCaptureHarness] ending {endingsCaptured}: sweat {idx} ({label}) " +
                          $"ticket state '{(at == null ? "null" : at.State.ToString())}' " +
                          $"— it leaves Open at ROUND settlement, not at its own sweat's end");
                endingsCaptured++;

                // Let this whistle pass so the next FT is a new one rather than the same one again.
                yield return WaitUntilOrFail(
                    () => screen.RevealedView.ClockText != "FT" || director.Run.Phase != Phase.Sweat,
                    runDeadline, "the clock never left FT");
            }

            Assert.AreEqual(2, endingsCaptured,
                "both endings must be in the set — the loud half and the quiet half are the point");


            Debug.Log($"[TvSweatCaptureHarness] seed={_seed} goalless capture complete -> {OutputDir}");
        }

        /// <summary>`T133` — THE RUNG-2 FRAME. Forced, and the forcing is disclosed on every
        /// filename it writes.
        ///
        /// <para><b>Why forced at all.</b> The subject is the footer's RIGHT half at the ENUMERATED
        /// WORST CASE — `$73,318,376,502`, eleven digits established over 648,000 priced offers by
        /// `PayoutMaximumTests`. **That amount cannot be dealt for in a capture.** `S3`'s precedent
        /// is exactly this: it reached an otherwise-unreachable empty group with a non-shipped
        /// `CorrectScoreFloor = 0.08` and put the disclosure on the frame's face. Same device, same
        /// reason.</para>
        ///
        /// <para><b>Why a frame at all, when the widths are already measured.</b> Because they
        /// answer different questions. Measured: `RETURNED` overruns by 51.9px, `PAID` fits with
        /// 13.2px — MORE headroom than the incumbent `PAYS`'s 9.3px. **Width is settled. What no px
        /// number can say is whether the word READS in that slot at the acceptance view**, and
        /// `C11` puts that on a frame.</para>
        ///
        /// <para><b>Three states, so the comparison is on one ruler:</b> the incumbent `PAYS`, the
        /// ruled-but-overrunning `RETURNED`, and the rung-2 candidate `PAID`. Shooting the candidate
        /// alone would show a string that looks fine with nothing to look fine AGAINST.</para>
        ///
        /// <para><b>WHAT THIS SET DOES NOT CLAIM:</b> nothing about whether `PAID` should be
        /// adopted — batch 108 rejected it for colliding at the root with `PAY $60` on the same
        /// screen, and that objection is untouched by any of this. **The width case against `PAID`
        /// is closed; the collision case is not, and it is a copy call this lane does not
        /// hold.**</para></summary>
        [Explicit("T133 rung-2 frame: the footer's right half at the enumerated worst case, in three "
            + "states, FORCED and disclosed. Writes frames. Run by filter only.")]
        [Timeout(900000)]
        [UnityTest]
        public IEnumerator Capture_T133_PaysRungTwo_Forced()
        {
            _seed = "GOALLESS-5";
            s_sceneIndex = 0;
            Directory.CreateDirectory(OutputDir);
            TheaterStage.PresentationSeedOverride = StableSeed(_seed);
            Time.captureDeltaTime = 1f / 50f;

            yield return LoadRoom();
            var director = Object.FindAnyObjectByType<RunDirector>();
            var screen = Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director, "RunDirector missing");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");
            Camera cam = Camera.main;   // the seated in-room camera — the acceptance view is the
                                        // only one that can answer "does it read".
            Assert.IsNotNull(cam, "no main camera");

            screen.TimeScaleOverride = 1f;
            couch.transitionDuration = 0.01f;
            yield return WaitUntilOrFail(() => director.Run != null,
                Time.realtimeSinceStartup + 10f, "director never started a run");

            director.StartNewRun(_seed);
            Run run = director.Run;
            Matchup m = run.CurrentSlate.Matchups[0];
            run.PlaceTicket(new List<Pick> { new Pick(m.Index, MarketSelection.MoneylineDraw()) }, 25.0);
            director.LockRound();
            couch.OnInteract(null);
            yield return WaitUntilOrFail(() => SitSpot.Active != null,
                Time.realtimeSinceStartup + 15f, "player never sat down");
            for (int i = 0; i < 30; i++) yield return null;   // let the column render

            // Three states on one ruler. Each is re-forced immediately before its own burst because
            // ANY repaint overwrites the literal — the force latches nothing by design, so a burst
            // that assumed it persisted would silently photograph the real string instead.
            foreach ((string label, string literal) in new[]
            {
                ("incumbent-PAYS",   "PAYS $73,318,376,502"),
                ("ruled-RETURNED",   "RETURNED $73,318,376,502"),
                ("rung2-PAID",       "PAID $73,318,376,502"),
            })
            {
                screen.ForcePaysTextForCapture(literal);
                Debug.Log($"[T133-FORCED] {label} :: pays='{screen.DebugTicketPaysText}' " +
                          $"footer='{screen.DebugTicketRiskText}' — FORCED, NOT A SHIPPED STATE");
                yield return CaptureBurst(screen, cam, $"FORCED-t133-{label}", 6, 0f);
            }

            Debug.Log($"[T133-FORCED] complete -> {OutputDir} — every frame here is FORCED and the " +
                      "worst-case amount is unreachable in play; see the dock README");
        }

        /// <summary>The named element's rect, found the way <c>TvExtentSweep</c> finds its slots —
        /// by GameObject name off the live hierarchy — rather than by adding another Debug accessor
        /// to production for a test's convenience.</summary>
        private static RectTransform NamedRect(TvSweatScreen screen, string name)
        {
            foreach (TMP_Text t in screen.GetComponentsInChildren<TMP_Text>(true))
                if (t.gameObject.name == name) return t.rectTransform;
            return null;
        }

        /// <summary>The named text component itself, for the callers that need its colour or its
        /// string rather than its rect.</summary>
        private static TMP_Text NamedText(TvSweatScreen screen, string name)
        {
            foreach (TMP_Text t in screen.GetComponentsInChildren<TMP_Text>(true))
                if (t.gameObject.name == name) return t;
            return null;
        }

        /// <summary>The named Image, for the row furniture that carries no text — the VOID strike
        /// and the lost row's extinguish panel. Both are state the chip cannot report, and
        /// <c>T149</c>'s criterion is about exactly the difference between them.</summary>
        private static UnityEngine.UI.Image NamedImage(TvSweatScreen screen, string name)
        {
            foreach (UnityEngine.UI.Image g in screen.GetComponentsInChildren<UnityEngine.UI.Image>(true))
                if (g.gameObject.name == name) return g;
            return null;
        }

        /// <summary><c>C55</c>: THE SUBJECT MUST BE IN FRAME, and the verdict is taken in LOCAL
        /// space. A green capture proves nothing if the thing it was shot for is not in the picture.
        ///
        /// <para>The viewport is the sweat canvas itself, which carries a <c>RectMask2D</c> — its own
        /// build note says anything outside it "stops existing on screen no matter which layer
        /// misplaces itself." So containment in the canvas IS the in-frame question on this surface,
        /// and it is exactly the question a composition change can break.</para>
        ///
        /// <para><b>LOCAL space, for the reason the laptop's own helper gives:</b> this is a
        /// WORLD-SPACE canvas hanging on a TV in a room, scaled to about a metre across, so every
        /// element rounds to the same two world digits and the plane is rotated to face the couch.
        /// Local space is the space the layout was authored in and its units are the pixels every
        /// constant in this build is written in.</para>
        ///
        /// <para><b>BOTH AXES, unlike the laptop's vertical-only cut.</b> There the horizontal term
        /// produced false negatives on a scrolling list whose lines span the block by construction.
        /// Here nothing scrolls, the column's width is locked (T46/R30), and the change under
        /// examination moved things in BOTH directions — the footer grew 40 → 60 and the rows
        /// re-pitched 69.3 → 99.0. A vertical-only verdict would not be testing this change.</para></summary>
        private static void AssertSubjectInFrame(TvSweatScreen screen, string subjectName, string burst)
        {
            RectTransform target = NamedRect(screen, subjectName);
            Assert.IsNotNull(target,
                $"C55: '{subjectName}' does not exist in the hierarchy, so the burst '{burst}' "
                + "cannot possibly contain it.");
            var graphic = target.GetComponent<TMP_Text>();
            Canvas canvas = graphic != null ? graphic.canvas : null;
            Assert.IsNotNull(canvas, $"C55: '{subjectName}' renders to no canvas — nothing to be in frame OF.");
            var viewport = canvas.transform as RectTransform;

            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 local = viewport.InverseTransformPoint(corners[i]);
                minX = Mathf.Min(minX, local.x); maxX = Mathf.Max(maxX, local.x);
                minY = Mathf.Min(minY, local.y); maxY = Mathf.Max(maxY, local.y);
            }
            Rect r = viewport.rect;
            bool inFrame = minX >= r.xMin - 0.5f && maxX <= r.xMax + 0.5f
                        && minY >= r.yMin - 0.5f && maxY <= r.yMax + 0.5f;

            Debug.Log($"[T147-C55] {subjectName,-16} local x {minX,7:0.0}..{maxX,7:0.0} "
                      + $"y {minY,7:0.0}..{maxY,7:0.0}  against canvas x {r.xMin:0.0}..{r.xMax:0.0} "
                      + $"y {r.yMin:0.0}..{r.yMax:0.0} — {(inFrame ? "IN FRAME" : "OUT OF FRAME")}");
            Assert.IsTrue(inFrame,
                $"C55: '{subjectName}' is NOT inside the canvas for burst '{burst}'. Local extent "
                + $"x {minX:0.0}..{maxX:0.0}, y {minY:0.0}..{maxY:0.0}; canvas x {r.xMin:0.0}.."
                + $"{r.xMax:0.0}, y {r.yMin:0.0}..{r.yMax:0.0}. The RectMask2D clips it, so the "
                + "frame would not contain its own subject.");
        }

        /// <summary>C55's cousin, REPORTED not asserted: a subject can be in frame as a RECT and
        /// still have its INK clipped, because these components are Overflow by construction. The
        /// live case is `RETURNED $73,318,376,502`, which overruns its own 249.0px row by 51.9px —
        /// T133, open with the Design Director. Whether that ink survives to the canvas edge or is
        /// cut by the mask is a fact the frames should carry rather than one a reader guesses.</summary>
        private static void ReportInkAgainstCanvas(TvSweatScreen screen, string subjectName)
        {
            RectTransform target = NamedRect(screen, subjectName);
            var graphic = target != null ? target.GetComponent<TMP_Text>() : null;
            if (graphic == null || graphic.canvas == null) return;
            var viewport = graphic.canvas.transform as RectTransform;

            // The BOX in the canvas's own local space, from its corners — the same space and the
            // same method AssertSubjectInFrame judges in, so a printed number and a verdict cannot
            // disagree.
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            float boxLeft = float.MaxValue, boxRight = float.MinValue;
            for (int i = 0; i < corners.Length; i++)
            {
                float x = viewport.InverseTransformPoint(corners[i]).x;
                boxLeft = Mathf.Min(boxLeft, x); boxRight = Mathf.Max(boxRight, x);
            }

            float inkW = graphic.GetPreferredValues(graphic.text, 100000f, 0f).x;
            float boxW = boxRight - boxLeft;

            // ⚠ THE INK GROWS AWAY FROM THE ALIGNED EDGE, AND THE FIRST CUT OF THIS HELPER FORGOT
            // IT. It assumed left alignment and computed `boxLeft + inkW` for every case, so on the
            // RIGHT-anchored arm it reported the ink running rightward from the left edge — and
            // printed "ink survives the mask" for a string that is in fact cut by it. A right-aligned
            // overrun spills LEFTWARD, off the column's outer edge, where this canvas's RectMask2D
            // is waiting. Same bug shape as the stale footer reader: a number that stopped meaning
            // what its label said, on a report nothing asserts against.
            bool rightAligned = graphic.alignment == TextAlignmentOptions.TopRight
                             || graphic.alignment == TextAlignmentOptions.Right
                             || graphic.alignment == TextAlignmentOptions.BottomRight;
            float inkLeft = rightAligned ? boxRight - inkW : boxLeft;
            float inkRight = rightAligned ? boxRight : boxLeft + inkW;

            Rect v = viewport.rect;
            float clippedLeft = Mathf.Max(0f, v.xMin - inkLeft);
            float clippedRight = Mathf.Max(0f, inkRight - v.xMax);
            bool clipped = clippedLeft > 0.5f || clippedRight > 0.5f;

            Debug.Log($"[T147-C55-ink] {subjectName} '{graphic.text}' "
                      + $"[{(rightAligned ? "RIGHT" : "LEFT")}-aligned] ink {inkW:0.0}px vs box "
                      + $"{boxW:0.0}px ({(inkW > boxW ? $"over by {inkW - boxW:0.0}" : "inside")}); "
                      + $"ink spans local x {inkLeft:0.0}..{inkRight:0.0} against canvas "
                      + $"{v.xMin:0.0}..{v.xMax:0.0} — "
                      + (clipped
                         ? $"INK IS CLIPPED BY THE MASK ({clippedLeft:0.0}px off the left, "
                           + $"{clippedRight:0.0}px off the right)"
                         : "ink survives the mask"));
        }

        /// <summary>T147 — THE RE-RULED TWO-ROW FOOTER, AND THE COST IT WAS PAID OUT OF.
        ///
        /// <para>`T144`/`T74-am6` ruled the two money facts onto SEPARATE ROWS: each half fits the
        /// 249.0px column alone and the PAIR does not, at ordinary values. Built at `T147-am` with
        /// <c>TicketFooterHeight</c> 40 → 60 and <c>TicketRowSlots</c> 6 → 4 — the slot count was
        /// reserving two rows the engine can never fill (<c>RunConfig.MaxLegs = 4</c>, ENFORCED at
        /// <c>Run.cs:190-191</c>), and that is what paid for the taller footer with `T24`'s margin
        /// intact.</para>
        ///
        /// <para><b>E3 IS THE ONE TO HOLD ON, and it is why this arm shoots a multi-leg ticket.</b>
        /// The footer is easy to shoot and easy to like; the cost lands elsewhere on the same
        /// screen — the leg rows went 69.3px → 99.0px for 58.8px of live ink. A set showing only
        /// the footer has not shown the change. Every burst here carries a live leg row in frame.</para>
        ///
        /// <para><b>WHY E1's OWN CASE IS FORCED.</b> `T74-am6` names `$1,234` staked paying
        /// `$12,340` — a plain 10x parlay and deliberately NOT a tail case. It is nonetheless
        /// unreachable at a fresh run's bank of 350 (<c>RunConfig.StartingBank</c>), so it cannot be
        /// dealt for. This arm therefore shoots BOTH: an UNFORCED ordinary state at whatever the run
        /// really affords, and the cited pair FORCED. They differ in provenance, not just in
        /// magnitude, and the filenames say which is which.</para>
        ///
        /// <para><b>THE OPPOSITE-ANCHOR ARM (`T147-am2`).</b> The ruling builds left/left, but the
        /// money control kept OPPOSITE anchors when it split onto two rows, so the alignment is an
        /// open choice with a precedent against it. Both arms are shot on one ruler, on the SETTLED
        /// pair: `RISK`/`PAYS` are both four characters and align either way, while `STAKE`/
        /// `RETURNED` are five and eight and are where left/left goes ragged.</para>
        ///
        /// <para><b>WHAT THIS SET DOES NOT CLAIM.</b> Nothing about `RETURNED`'s own width. It
        /// measures 300.9 against the 249.0 row and OVERRUNS BY 51.9px — separate rows fixes the
        /// PAIR collision and was never claimed to fix that. It is `T133`, still open, and it is
        /// visible in these frames rather than hidden by them.</para></summary>
        [Explicit("T147 E1/E2/E3: the re-ruled two-row footer — an unforced ordinary state, T74-am6's "
            + "cited pair FORCED, the enumerated fact floor FORCED, and the opposite-anchor arm, all "
            + "with a live leg row in frame. Writes frames. Run by filter only.")]
        [Timeout(900000)]
        [UnityTest]
        public IEnumerator Capture_T147_TwoRowFooter()
        {
            _seed = "GOALLESS-5";
            s_sceneIndex = 0;
            Directory.CreateDirectory(OutputDir);
            TheaterStage.PresentationSeedOverride = StableSeed(_seed);
            Time.captureDeltaTime = 1f / 50f;

            yield return LoadRoom();
            var director = Object.FindAnyObjectByType<RunDirector>();
            var screen = Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director, "RunDirector missing");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");
            Camera cam = Camera.main;
            Assert.IsNotNull(cam, "no main camera");

            screen.TimeScaleOverride = 1f;
            couch.transitionDuration = 0.01f;
            yield return WaitUntilOrFail(() => director.Run != null,
                Time.realtimeSinceStartup + 10f, "director never started a run");

            director.StartNewRun(_seed);
            Run run = director.Run;
            // MULTI-LEG, for E3: a single-leg ticket has no second row to show the new pitch against,
            // and the whole point of E3 is that the height came from the rows. Two matchups, base
            // moneyline selections — PlaceTicket REFUSES an unoffered selection at runtime, so this
            // takes markets the slate certainly carries rather than inventing a line.
            Matchup m0 = run.CurrentSlate.Matchups[0];
            Matchup m1 = run.CurrentSlate.Matchups[1];
            run.PlaceTicket(new List<Pick>
            {
                new Pick(m0.Index, MarketSelection.MoneylineDraw()),
                new Pick(m1.Index, MarketSelection.MoneylineDraw()),
            }, 25.0);
            director.LockRound();
            couch.OnInteract(null);
            yield return WaitUntilOrFail(() => SitSpot.Active != null,
                Time.realtimeSinceStartup + 15f, "player never sat down");

            // E3's binding condition: a LIVE leg row must be on screen, not merely a ticket. The live
            // form is the only one that carries progress text, so that is the signal — the same one
            // TicketFooterWord_NeverDisagreesWithAnyRow reads.
            yield return WaitUntilOrFail(
                () => !string.IsNullOrEmpty(screen.DebugLegProgress(0))
                   || !string.IsNullOrEmpty(screen.DebugLegProgress(1)),
                Time.realtimeSinceStartup + 60f,
                "no leg ever went live — E3 cannot be satisfied by a frame with no live row");

            Debug.Log($"[T147-CAP] live row present :: leg0='{screen.DebugLegProgress(0)}' "
                      + $"leg1='{screen.DebugLegProgress(1)}' footer='{screen.DebugTicketRiskText}' "
                      + $"/ '{screen.DebugTicketPaysText}'");

            // C55, BEFORE the shutter and before every burst: both money rows and the live row's own
            // two lines must be inside the canvas. The composition just moved all of them.
            string liveNeed = !string.IsNullOrEmpty(screen.DebugLegProgress(0))
                ? "LegRowNeed0" : "LegRowNeed1";
            string liveProgress = liveNeed == "LegRowNeed0" ? "LegRowProgress0" : "LegRowProgress1";
            // "Chrome" is in the list on the precommit's §1.2: the open objection against `PAID` is
            // that it collides at the ROOT with `PAY $60`, and that read is impossible unless both
            // are in one frame. A frame missing the chrome row cannot answer the question it is for.
            foreach (string subject in new[] { "RiskPays", "Pays", "Chrome", liveNeed, liveProgress })
                AssertSubjectInFrame(screen, subject, "t147-E1E3-unforced-live-row");

            // E1 + E3, UNFORCED: the real ticket at what the run actually affords, with a live row in
            // frame. This is the only burst here whose strings the product produced by itself.
            yield return CaptureBurst(screen, cam, "t147-E1E3-unforced-live-row", 6, 0f);

            // Each force is re-applied immediately before its own burst: the force LATCHES NOTHING by
            // design, so any repaint between force and shutter photographs the real string instead.
            foreach ((string label, string risk, string pays) in new[]
            {
                // E1's cited case — T74-am6's plain 10x parlay, the one that collides in the old
                // one-row form at ORDINARY values.
                ("E1-ordinary-cited", "RISK $1,234", "PAYS $12,340"),
                // E2 — the enumerated fact floor, bank $10,000, from PayoutMaximumTests.
                ("E2-fact-floor", "RISK $13,639", "PAYS $73,318,376,502"),
            })
            {
                screen.ForceRiskPaysTextForCapture(risk);
                screen.ForcePaysTextForCapture(pays);
                Debug.Log($"[T147-CAP] {label} :: '{screen.DebugTicketRiskText}' / "
                          + $"'{screen.DebugTicketPaysText}' — FORCED, NOT A SHIPPED STATE");
                AssertSubjectInFrame(screen, "RiskPays", label);
                AssertSubjectInFrame(screen, "Pays", label);
                AssertSubjectInFrame(screen, "Chrome", label);
                AssertSubjectInFrame(screen, liveNeed, label);
                ReportInkAgainstCanvas(screen, "Pays");
                yield return CaptureBurst(screen, cam, $"FORCED-t147-{label}", 6, 0f);
            }

            // ================= THE SETTLED STATE, SETTLED FOR REAL =================
            //
            // ⚠ THE FIRST CUT OF THIS ARM FORCED `STAKE`/`RETURNED` ONTO AN OPEN TICKET, and that
            // frame set was DELETED rather than docked. The settled footer renders ONLY when
            // `_ticket.State` is CashedOut or Lost (TvSweatScreen's settled branch), so forcing its
            // strings onto a live ticket photographs a composition the code path cannot produce —
            // the footer precommit's §0, and `T133-am2`'s own mistake one screen over.
            //
            // It is worse than a wrong label here: on a settled ticket T147's own build CANCELS the
            // remaining rows (chip silent, struck), so a forced-settled frame over live rows shows a
            // state the product no longer has at all.
            //
            // So: take a REAL cash-out, and force only the AMOUNT afterwards. The composition is
            // then the product's own and only the digits are authored.
            yield return WaitUntilOrFail(
                () => director.CurrentSession != null && !director.CurrentSession.IsComplete
                   && screen.EventsEmitted >= 1 && !screen.RevealedView.MarketSuspended
                   && !screen.DebugCashOutAnimating
                   && director.CurrentSession.CashOutOffer().HasValue,
                Time.realtimeSinceStartup + 90f, "never reached an open cash-out window");

            // DRIVE THE PLAYER'S OWN SEQUENCE: preview, THEN accept. Not a nicety —
            // `ExitCashOutPreview` early-returns unless a preview is open, and IT is the thing
            // that calls `UpdateTicketColumn`. Accepting without previewing leaves the column
            // unrepainted: the settled branch never runs, so the footer keeps its LIVE words and
            // the rows keep their live/NEXT chips on a position that is already closed.
            //
            // The first cut of this arm did exactly that, and the run log caught it —
            // `state=CashedOut` printed beside `footer='RISK $25' / 'PAYS $330'` and leg 1 still
            // reading `NEXT`. **That was THIS HARNESS taking a shortcut the product does not
            // offer, not a defect in the product**: in play the hold opens the preview and the
            // accept closes it, and closing it is what repaints. Worth the words, because a frame
            // shot that way would have been read as a product defect by whoever opened it next.
            const System.Reflection.BindingFlags Priv =
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            System.Reflection.MethodInfo enterPreview =
                typeof(TvSweatScreen).GetMethod("EnterCashOutPreview", Priv);
            System.Reflection.MethodInfo tryCashOut =
                typeof(TvSweatScreen).GetMethod("TryCashOut", Priv);
            Assert.IsNotNull(enterPreview, "TvSweatScreen.EnterCashOutPreview not found by reflection — renamed?");
            Assert.IsNotNull(tryCashOut, "TvSweatScreen.TryCashOut not found by reflection — renamed?");
            Assert.IsTrue((bool)enterPreview.Invoke(screen, null),
                "the cash-out preview refused to open, so the accept path cannot be driven as the player drives it");
            // NO YIELD BETWEEN THESE TWO. Update's gesture resolution closes the preview whenever no
            // key is held — and with no InputSystem device driving this arm, that is every frame. A
            // frame's gap here closes the preview BEFORE the accept, so the repaint happens while the
            // ticket is still live and the settle then repaints nothing. Enter and accept in one
            // frame; the NEXT Update finds the preview open and the offer gone, and that is the
            // repaint that renders the settled column.
            tryCashOut.Invoke(screen, null);
            Assert.AreEqual(TicketState.CashedOut, director.CurrentTicket.State,
                "the settle must ACTUALLY take: a forced string on an open ticket is exactly the "
                + "precommit's §0 mistake, and this assertion is what stops it recurring.");
            for (int i = 0; i < 30; i++) yield return null;   // let the column repaint settled

            Debug.Log($"[T147-CAP] SETTLED for real :: state={director.CurrentTicket.State} "
                      + $"footer='{screen.DebugTicketRiskText}' / '{screen.DebugTicketPaysText}' "
                      + $"leg0 chip='{screen.DebugLegState(0)}' leg1 chip='{screen.DebugLegState(1)}' "
                      + "— the chips are the T147 cancelled-row treatment, unforced");

            // THE PIN THAT STOPS THE SHORTCUT RECURRING. Reaching CashedOut is not the same as
            // RENDERING it, and the difference is a whole frame set. If the column has not repainted,
            // the footer still carries its live word and every burst below would photograph a live
            // composition on a closed position — the precommit's §0 in its subtlest form, because
            // the ticket state really IS settled and only the picture is stale.
            Assert.IsTrue(screen.DebugTicketRiskText.StartsWith("STAKE"),
                $"the settled footer never RENDERED: state is {director.CurrentTicket.State} but the "
                + $"footer still reads '{screen.DebugTicketRiskText}'. The column did not repaint "
                + "after the settle, so these frames would show a live composition on a closed "
                + "position. Drive the preview before the accept — closing the preview is what repaints.");

            // S1 — the settled composition ENTIRELY UNFORCED. Never shot before: it carries T147's
            // cancelled rows (a pending leg on a settled ticket goes silent and takes §8.10's
            // strike) as well as the two-row settled footer.
            foreach (string subject in new[] { "RiskPays", "Pays", "Chrome" })
                AssertSubjectInFrame(screen, subject, "t147-S1-settled-unforced");
            ReportInkAgainstCanvas(screen, "Pays");
            yield return CaptureBurst(screen, cam, "t147-S1-settled-unforced", 6, 0f);

            // S2/S3 — the enumerated fact floor on that same real settle, both alignment arms on one
            // ruler. Only the DIGITS are forced now; the state, the rows and the branch are real.
            // `RETURNED $73,318,376,502` is T148's overrun and it is shown, not hidden.
            foreach ((string label, bool rightAnchored) in new[]
            {
                ("S2-settled-factfloor-left-left", false),
                ("S3-settled-factfloor-opposite-anchor", true),
            })
            {
                screen.ForcePaysAnchorForCapture(rightAnchored);
                screen.ForceRiskPaysTextForCapture("STAKE $13,639");
                screen.ForcePaysTextForCapture("RETURNED $73,318,376,502");
                Debug.Log($"[T147-CAP] {label} :: settled ticket ({director.CurrentTicket.State}), "
                          + $"AMOUNT forced to the fact floor, row 2 "
                          + $"{(rightAnchored ? "RIGHT" : "LEFT")}-anchored"
                          + (rightAnchored ? " — FORCED LAYOUT, NOT THE SHIPPED COMPOSITION" : ""));
                foreach (string subject in new[] { "RiskPays", "Pays", "Chrome" })
                    AssertSubjectInFrame(screen, subject, label);
                ReportInkAgainstCanvas(screen, "Pays");
                yield return CaptureBurst(screen, cam, $"FORCED-t147-{label}", 6, 0f);
            }
            screen.ForcePaysAnchorForCapture(false);   // leave the scene as the product ships it

            Debug.Log($"[T147-CAP] complete -> {OutputDir}. UNFORCED: the E1E3 burst. FORCED and "
                      + "disclosed in the filename: everything else. The opposite-anchor burst forces "
                      + "LAYOUT, not just copy — it is the arm T147-am2 sent to a frame, not a state "
                      + "the product has.");
        }

        /// <summary>T149 CRITERION 2 — THE BUSTED TICKET'S CANCELLED ROWS, AGAINST ITS LOST ROW.
        ///
        /// <para>Shot against <c>docs/design/t149-bust-precommit-2026-08-20.md</c>, which binds this
        /// window. Criterion 2 asks whether an unplayed leg on a busted ticket reads as CANCELLED
        /// rather than as LOST. Four channels separate them, per that document's source read: text
        /// tier L1 vs L2, chip <c>L</c> vs blank, background extinguished vs not, strike off vs on.
        /// All four are logged here so the frame can be judged against the build rather than
        /// against a memory of it.</para>
        ///
        /// <para><b>THE CONDITION THAT KILLED THE TWO EARLIER ATTEMPTS IS ASSERTED, NOT ASSUMED.</b>
        /// A one-leg bust has nothing after the loser to strike, and a cash-out is not a bust at all
        /// — the precommit's §1 records both failures. So this arm FINDS the lost leg and FAILS
        /// unless at least one leg sits AFTER it. It does not trust the recipe to have produced the
        /// state; it checks.</para>
        ///
        /// <para><b>THE PENDING-LOSS WINDOW IS DECLINED IF IT OPENS.</b> <c>ResolveLegFinal</c> busts
        /// instantly only when no save is legal, and <c>mulliganLegal</c> is
        /// <c>_mulliganAvailable() &amp;&amp; ActiveLegCount() &gt;= 2</c> — which a three-leg ticket
        /// satisfies. Left alone, the recipe would sit in the window and never bust.</para>
        ///
        /// <para><b>UNFORCED, ENTIRELY.</b> No string and no layout is forced here, so no filename
        /// carries <c>FORCED-</c>. The state is dealt for, which is what makes it evidence.</para></summary>
        [Explicit("T149 criterion 2: a REAL busted ticket's cancelled rows beside its lost row, in "
            + "one frame. Writes frames. Run by filter only.")]
        [Timeout(900000)]
        [UnityTest]
        public IEnumerator Capture_T149_BustedTicket()
        {
            _seed = "GOALLESS-5";
            s_sceneIndex = 0;
            Directory.CreateDirectory(OutputDir);
            TheaterStage.PresentationSeedOverride = StableSeed(_seed);
            Time.captureDeltaTime = 1f / 50f;

            yield return LoadRoom();
            var director = Object.FindAnyObjectByType<RunDirector>();
            var screen = Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director, "RunDirector missing");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");
            Camera cam = Camera.main;
            Assert.IsNotNull(cam, "no main camera");

            screen.TimeScaleOverride = 1f;
            couch.transitionDuration = 0.01f;
            yield return WaitUntilOrFail(() => director.Run != null,
                Time.realtimeSinceStartup + 10f, "director never started a run");

            director.StartNewRun(_seed);
            Run run = director.Run;
            // The precommit's offered recipe: a team backer that loses on this seed, then two draws
            // behind it. THREE legs, and the loser first — rows AFTER the loser are the struck ones
            // this criterion is about. PlaceTicket refuses an unoffered selection, so these are base
            // moneyline markets the slate certainly carries.
            Matchup m0 = run.CurrentSlate.Matchups[0];
            Matchup m1 = run.CurrentSlate.Matchups[1];
            Matchup m2 = run.CurrentSlate.Matchups[2];
            run.PlaceTicket(new List<Pick>
            {
                new Pick(m0.Index, MarketSelection.Moneyline(Side.Home)),
                new Pick(m1.Index, MarketSelection.MoneylineDraw()),
                new Pick(m2.Index, MarketSelection.MoneylineDraw()),
            }, 25.0);
            director.LockRound();
            couch.OnInteract(null);
            yield return WaitUntilOrFail(() => SitSpot.Active != null,
                Time.realtimeSinceStartup + 15f, "player never sat down");

            // Decline the pending-loss window the moment it opens — see the note above. Polled in
            // the same loop that waits for the settle, because the window can open at any leg.
            float deadline = Time.realtimeSinceStartup + 180f;
            bool declined = false;
            while (Time.realtimeSinceStartup < deadline)
            {
                SweatSession session = director.CurrentSession;
                if (session != null && session.HasPendingLoss)
                {
                    session.DeclinePendingLoss();
                    declined = true;
                    Debug.Log("[T149-CAP] pending-loss window opened and was DECLINED — a three-leg "
                              + "ticket makes a mulligan legal, so the bust is not instant here");
                }
                if (director.CurrentTicket != null
                    && director.CurrentTicket.State == TicketState.Lost
                    && screen.DebugTicketRiskText.StartsWith("STAKE"))
                    break;
                yield return null;
            }

            // CONDITION 4, and it is the T133-am2 check restated for this state: reaching Lost is not
            // the same as RENDERING it. If the footer still reads RISK/PAYS the settled branch did
            // not run and the frame is not of this subject.
            Assert.AreEqual(TicketState.Lost, director.CurrentTicket.State,
                $"the ticket never busted within the window (declined a pending loss: {declined}). "
                + "A frame of a live ticket cannot test criterion 2.");
            Assert.IsTrue(screen.DebugTicketRiskText.StartsWith("STAKE"),
                $"the ticket is Lost but the footer reads '{screen.DebugTicketRiskText}' — the settled "
                + "branch did not render, so this frame would show a live composition on a dead ticket.");

            // CONDITION 1: find the LOST leg and prove at least one leg sits AFTER it. This is the
            // condition a one-leg bust and a last-leg bust each fail, and it is checked rather than
            // assumed because both earlier attempts died on it.
            int legCount = director.CurrentTicket.Legs.Count;
            int lost = -1;
            for (int i = 0; i < legCount; i++)
                if (screen.DebugLegState(i) == "L") { lost = i; break; }
            Assert.GreaterOrEqual(lost, 0,
                "no row shows the L chip, so there is no lost leg in frame to compare against");
            Assert.Less(lost, legCount - 1,
                $"the losing leg is the LAST of {legCount} — there is no unplayed leg behind it to "
                + "strike, which is exactly the state the precommit's §1 found untestable.");

            // CRITERION 1: no row prints NEXT, and every unplayed leg carries the strike.
            for (int i = lost + 1; i < legCount; i++)
            {
                Assert.AreNotEqual("NEXT", screen.DebugLegState(i),
                    $"leg {i} still prints NEXT on a busted ticket — criterion 1 fails at source, "
                    + "before any question of how it reads.");
                UnityEngine.UI.Image strike = NamedImage(screen, $"LegRowStrike{i}");
                Assert.IsNotNull(strike, $"LegRowStrike{i} not found");
                Assert.IsTrue(strike.enabled,
                    $"leg {i} is unplayed on a busted ticket but carries no VOID strike");
            }

            // The four channels, logged per row so the frame is self-describing and can be read
            // against the precommit's own source table rather than from memory.
            for (int i = 0; i < legCount; i++)
            {
                TMP_Text line = NamedText(screen, $"LegRowLine{i}");
                UnityEngine.UI.Image strike = NamedImage(screen, $"LegRowStrike{i}");
                UnityEngine.UI.Image ext = NamedImage(screen, $"LegRowExtinguish{i}");
                Debug.Log($"[T149-CH] leg {i} {(i == lost ? "LOST" : i < lost ? "resolved" : "CANCELLED")}"
                          + $" :: chip='{screen.DebugLegState(i)}' text='{(line != null ? line.text : "")}'"
                          + $" alpha={(line != null ? line.color.a : 0f):0.00}"
                          + $" strike={(strike != null && strike.enabled)}"
                          + $" extinguished={(ext != null && ext.enabled)}");
            }
            Debug.Log($"[T149-CAP] busted :: state={director.CurrentTicket.State} "
                      + $"footer='{screen.DebugTicketRiskText}' / '{screen.DebugTicketPaysText}' "
                      + $"lost leg={lost} of {legCount}, cancelled behind it={legCount - lost - 1}");

            // C55, condition 3: the LOST row and at least one STRUCK row must be in ONE frame, plus
            // the chrome row and both footer rows (condition 4). Two frames cannot make this
            // comparison, which is why every subject is asserted before the one shutter.
            foreach (string subject in new[]
                     { $"LegRowLine{lost}", $"LegRowLine{lost + 1}", "Chrome", "RiskPays", "Pays" })
                AssertSubjectInFrame(screen, subject, "t149-bust-cancelled-rows");

            yield return CaptureBurst(screen, cam, "t149-bust-cancelled-rows", 8, 0f);

            Debug.Log($"[T149-CAP] complete -> {OutputDir}. UNFORCED throughout: no string and no "
                      + "layout was forced, so nothing here carries FORCED-. The state was dealt for.");
        }

        /// <summary>T129 ARM 2 — COUNT LEGS SETTLING LEVEL.
        ///
        /// <para>A goalless draw settles a whole family the docked set has never carried:
        /// <c>UNDER 1.5/2.5/3.5 GOALS</c> all win, <c>BTTS — NO</c> wins, <c>TOTAL GOALS EVEN</c>
        /// wins on zero. <b>None has ever been shot at its ending.</b> One ticket carrying an under
        /// leg and a BTTS-NO leg covers it.</para>
        ///
        /// <para><b>Same seed and same goalless matchup as arm 1, deliberately.</b> A 0–0 settles
        /// this whole family at once, and sharing the fixture keeps the three arms comparable with
        /// each other as well as with the docked set — a second fixture would buy nothing and cost
        /// the comparison.</para>
        ///
        /// <para><b>Selections come OFF THE BOARD.</b> <c>PlaceTicket</c> throws
        /// "Market selection is not offered" for anything the board never priced — the engine
        /// enforcing this lane's own discipline, and it has already cost a run. A matchup that
        /// prices neither line is a RE-SEED, never a substitution.</para>
        ///
        /// <para><b>WHAT THIS SET DOES NOT CLAIM:</b> nothing about a NON-goalless draw (§6.8 rules
        /// this the drawn match's line, not the goalless one, and generality is not what is
        /// missing); nothing about cards; and no judgement on whether the ending reads — that is a
        /// C11 call at the acceptance view.</para></summary>
        [Explicit("T129 arm 2: count legs settling level on a goalless draw — an under-goals leg and "
            + "a BTTS-NO leg, shot at their ending. Writes frames. Run by filter only.")]
        [Timeout(1200000)]
        [UnityTest]
        public IEnumerator Capture_CountLegsSettlingLevel()
        {
            yield return RunGoallessSingleTicketCapture(
                "level-count-legs",
                (matchupIndex, matchup) =>
                {
                    MarketSelection under = FirstOfferedSelection(matchup, MarketKind.TotalGoals,
                        s => s.Choice == MarketChoice.Under,
                        "no UNDER total-goals line on the goalless matchup — a RE-SEED, never a "
                        + "substitution and never a constructed selection");
                    MarketSelection bttsNo = FirstOfferedSelection(matchup, MarketKind.BothTeamsToScore,
                        s => s.Choice == MarketChoice.No,
                        "no BTTS-NO line on the goalless matchup — a RE-SEED, never a substitution");
                    return new List<Pick> { new Pick(matchupIndex, under), new Pick(matchupIndex, bttsNo) };
                });
        }

        /// <summary>T129 ARM 3 — CORRECT SCORE <c>0-0</c>.
        ///
        /// <para><b>New territory: <c>CorrectScore</c> had no reachable home until <c>S95</c>, so NO
        /// CAPTURE OF ANY KIND EXISTS.</b> The longest price on the board settling on the quietest
        /// possible match is this phase's extreme case.</para>
        ///
        /// <para><b>T129 condition (b) bites hardest here: the subject is a specific STRING</b>, so
        /// the matchup is PINNED and the <c>0-0</c> cell is located explicitly. <b>It is not dealt
        /// for.</b> A different correct-score cell would produce a complete, structurally-fine set
        /// that answers nothing — the failure this lane has already recorded twice — so a board that
        /// does not price <c>0-0</c> here fails loudly as a re-seed.</para>
        ///
        /// <para><b>WHAT THIS SET DOES NOT CLAIM:</b> nothing about any other correct-score cell,
        /// and nothing about how the price READS at the acceptance view (C11).</para></summary>
        [Explicit("T129 arm 3: correct score 0-0 settling on a goalless draw — no capture of this "
            + "market has ever existed. Writes frames. Run by filter only.")]
        [Timeout(1200000)]
        [UnityTest]
        public IEnumerator Capture_CorrectScoreNilNil()
        {
            yield return RunGoallessSingleTicketCapture(
                "correct-score-0-0",
                (matchupIndex, matchup) =>
                {
                    MarketSelection cs = FirstOfferedSelection(matchup, MarketKind.CorrectScore,
                        s => s.ScoreHome == 0 && s.ScoreAway == 0,
                        "the goalless matchup does not price the CORRECT SCORE 0-0 cell — a RE-SEED. "
                        + "T129 (b) says PIN OR FORCE the matchup because the subject is a specific "
                        + "STRING; shooting a different cell would answer nothing while looking fine");
                    return new List<Pick> { new Pick(matchupIndex, cs) };
                });
        }

        /// <summary>THE ANCHOR WINDOW, FRAME A — <c>T163</c>'s NEITHER branch on the strip.
        ///
        /// <para>Shot against <c>docs/design/anchor-precommit-2026-08-24.md</c>. Composition is
        /// arm 2's exactly — <c>GOALLESS-5</c>, matchup 0, stake 25, <c>UNDER 1.5 GOALS</c> +
        /// <c>BTTS — NO</c> — so §5's docked predecessor stays readable against it for the COLUMN.</para>
        ///
        /// <para><b>THE MOMENT IS NOT ARM 2's, AND IT CANNOT BE.</b> Arm 2 bursts 150 frames FROM THE
        /// WHISTLE FORWARD, and on a drawn match the strip at FT carries <c>THE MATCH ENDS LEVEL</c> —
        /// written DIRECTLY at the call site in <c>FinalSlam</c> and <c>RenderEvent</c> when the
        /// revealed ledger is level, never drawn from a table (and <c>SweatFlavor.For</c> returns
        /// <c>FINAL WHISTLE</c> on a LegFinal for the same reason). <b>That line is club-free by
        /// AUTHORSHIP, not by the anchor.</b> A frame shot to match arm 2's moment would satisfy §5's
        /// economy and FAIL condition 1 while looking perfectly clean — the failure condition 1 exists
        /// to prevent, one medium over. So this bursts at a MOMENTUM beat mid-sweat. §5 is explicitly
        /// <i>not a condition, an economy</i>; condition 1 binds.</para>
        ///
        /// <para><b>One property of the seed to expect rather than discover:</b> <c>GOALLESS-5</c>
        /// ends 0-0, so there are no goal beats at all — the only anchor-interpolating lines reachable
        /// here would be <c>MomUp</c>/<c>MomDown</c>. THE FRAMES DISPROVED THIS, and the shot is the
        /// better for it: the burst caught <c>NeitherGoalUp[1]</c> at 1'. A <c>Score</c> beat plays
        /// in FULL on a goalless match — the ledger's live-lead clamp stages it as the CHALKED-OFF
        /// variant — so the narrative beat and the scoreline are different things, and the goal
        /// family IS reachable here.</para></summary>
        [Explicit("Anchor window frame A: T163 neither branch on a totals/BTTS leg. Writes frames. "
            + "Run by filter only.")]
        [Timeout(1200000)]
        [UnityTest]
        public IEnumerator Capture_AnchorNeitherBranch_TotalsLeg()
        {
            _seed = "GOALLESS-5";
            s_sceneIndex = 0;
            Directory.CreateDirectory(OutputDir);
            TheaterStage.PresentationSeedOverride = StableSeed(_seed);
            Time.captureDeltaTime = 1f / 50f;

            yield return LoadRoom();
            var director = Object.FindAnyObjectByType<RunDirector>();
            var screen = Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = Object.FindAnyObjectByType<SitSpot>();
            Camera cam = Camera.main;
            Assert.IsNotNull(director, "RunDirector missing");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");
            Assert.IsNotNull(cam, "no main camera — the room band would be lost");

            screen.TimeScaleOverride = 1f;
            couch.transitionDuration = 0.01f;
            yield return WaitUntilOrFail(() => director.Run != null,
                Time.realtimeSinceStartup + 10f, "director never started a run");

            director.StartNewRun(_seed);
            Run run = director.Run;
            Matchup m = run.CurrentSlate.Matchups[0];
            Assert.IsNotNull(m, "the goalless matchup is missing from this slate");

            MarketSelection under = FirstOfferedSelection(m, MarketKind.TotalGoals,
                sel => sel.Choice == MarketChoice.Under,
                "no UNDER total-goals line on the goalless matchup — a RE-SEED, never a substitution");
            MarketSelection bttsNo = FirstOfferedSelection(m, MarketKind.BothTeamsToScore,
                sel => sel.Choice == MarketChoice.No,
                "no BTTS-NO line on the goalless matchup — a RE-SEED, never a substitution");
            run.PlaceTicket(new List<Pick> { new Pick(m.Index, under), new Pick(m.Index, bttsNo) }, 25.0);
            director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);

            // CONDITION 3 — no live leg on this fixture names a side, or T163 branch (1) fires and the
            // frame tests the opposite branch. Asserted off the ENGINE's table, never by kind name.
            Ticket ticket = director.CurrentTicket;
            for (int i = 0; i < ticket.Legs.Count; i++)
                Assert.IsNull(MatchModel.AnchorSide(ticket.Legs[i]),
                    $"leg {i} ({ticket.Legs[i].Selection.Kind}) NAMES A SIDE, so T163 branch (1) fires "
                    + "and this frame would test the side branch instead of the neither branch");

            couch.OnInteract(null);
            yield return WaitUntilOrFail(() => SitSpot.Active != null,
                Time.realtimeSinceStartup + 15f, "player never sat down");

            // CONDITION 1 — wait for a strip line that came from an ANCHOR-INTERPOLATING table. On
            // this branch that means a member of the club-free set: reaching it proves the call went
            // through For/NeutralLine and took T163's neither path, which a count-family line or the
            // authored drawn ending would not.
            string line = null;
            float deadline = Time.realtimeSinceStartup + 900f;
            while (Time.realtimeSinceStartup < deadline)
            {
                string now = screen.DebugFlavorText;
                if (!string.IsNullOrEmpty(now) && IsClubFreeAnchorLine(now)) { line = now; break; }
                if (screen.RevealedView.ClockText == "FT" || run.Phase != Phase.Sweat) break;
                yield return null;
            }
            Assert.IsNotNull(line,
                "no strip line from an anchor-interpolating table appeared before full time. CONDITION 1 "
                + "is unmet and this burst would prove nothing about the anchor. Last strip text: "
                + $"'{screen.DebugFlavorText}'");

            // CONDITION 4 — the assertion itself: the rendered line names NO club.
            string home = SweatFlavor.Short(m.Home.Name), away = SweatFlavor.Short(m.Away.Name);
            StringAssert.DoesNotContain(home, line, $"the strip names the HOME club: '{line}'");
            StringAssert.DoesNotContain(away, line, $"the strip names the AWAY club: '{line}'");

            Debug.Log($"[ANCHOR-A] strip='{line}' home='{home}' away='{away}' "
                + $"clock={screen.RevealedView.ClockText}");

            AssertSubjectInFrame(screen, "Flavor", "anchor-neither-totals");
            yield return CaptureBurst(screen, cam, "anchor-neither-totals", 60, 0f);

            Debug.Log($"[TvSweatCaptureHarness] seed={_seed} frame A complete -> {OutputDir}");
        }

        /// <summary>CONDITION 1's classifier: is this rendered line a member of <c>T163</c>'s
        /// club-free set? Read off <see cref="SweatFlavor.NeitherLine"/> itself rather than restated,
        /// so the harness cannot drift from the table it checks.
        ///
        /// <para>This is the assertion the pre-commitment asks for IN THE HARNESS rather than by eye:
        /// a strip carrying a count-family line (<c>CornerFor</c>/<c>BookingFor</c>), the near-miss
        /// pair, or the authored <c>THE MATCH ENDS LEVEL</c> is ALSO club-free — and would read as a
        /// clean pass while proving nothing about the anchor.</para></summary>
        private static bool IsClubFreeAnchorLine(string rendered)
        {
            foreach (DramaEventType type in new[] { DramaEventType.Score, DramaEventType.Momentum })
                foreach (bool up in new[] { true, false })
                    for (int step = 0; step < 3; step++)
                        if (rendered == SweatFlavor.NeitherLine(type, up, step)) return true;
            return false;
        }

        /// <summary>THE ANCHOR WINDOW, FRAME B — <c>T163</c>'s SIDE branch, on a leg that names one.
        ///
        /// <para>Shot against <c>docs/design/anchor-precommit-2026-08-24.md</c> §2 conditions 5 and 6.</para>
        ///
        /// <para><b>THE HANDICAP BACKS AWAY, and that is condition 5.</b> A home-backed handicap
        /// passes under the OLD defect as well as the new ruling — <c>PickedHomeForPresentation</c>
        /// returned HOME unconditionally for every non-moneyline kind — so it would photograph
        /// perfectly and prove nothing. The pre-commitment names this as the condition most likely to
        /// be met in spirit and missed in fact, citing <c>T149-am</c>.</para>
        ///
        /// <para><b>THREE ZONES IN ONE FRAME, and that is condition 6.</b> The lane's own ask ordered
        /// two — strip and scorebug — and the DD amended it to three, correctly: the <c>●</c> backed
        /// marker renders on MONEYLINE legs only, so on a handicap the scorebug says which club is
        /// away but nothing says which club he BACKED. Without the leg's own row the frame shows an
        /// anchor with nothing to check it against. <c>K17-cl</c>'s console defect was exactly this
        /// disagreement — backing one club while the strip narrated the other — and it is invisible
        /// with two zones.</para></summary>
        [Explicit("Anchor window frame B: T163 side branch on an away-backed handicap. Writes frames. "
            + "Run by filter only.")]
        [Timeout(1200000)]
        [UnityTest]
        public IEnumerator Capture_AnchorSideBranch_AwayHandicap()
        {
            _seed = "ANCHOR-B";
            s_sceneIndex = 0;
            Directory.CreateDirectory(OutputDir);
            TheaterStage.PresentationSeedOverride = StableSeed(_seed);
            Time.captureDeltaTime = 1f / 50f;

            yield return LoadRoom();
            var director = Object.FindAnyObjectByType<RunDirector>();
            var screen = Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = Object.FindAnyObjectByType<SitSpot>();
            Camera cam = Camera.main;
            Assert.IsNotNull(director, "RunDirector missing");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");
            Assert.IsNotNull(cam, "no main camera — the room band would be lost");

            screen.TimeScaleOverride = 1f;
            couch.transitionDuration = 0.01f;
            yield return WaitUntilOrFail(() => director.Run != null,
                Time.realtimeSinceStartup + 10f, "director never started a run");

            director.StartNewRun(_seed);
            Run run = director.Run;

            // Searched off the board, never constructed: the first matchup that prices an AWAY
            // handicap. A board that prices none fails as a RE-SEED rather than quietly shooting a
            // home-backed leg, which is the one substitution condition 5 forbids.
            Matchup m = null;
            MarketSelection hcap = default;
            foreach (Matchup candidate in run.CurrentSlate.Matchups)
            {
                foreach (MarketOffer offer in candidate.Markets)
                    if (offer.Selection.Kind == MarketKind.Handicap
                        && offer.Selection.Choice == MarketChoice.Away)
                    { m = candidate; hcap = offer.Selection; break; }
                if (m != null) break;
            }
            Assert.IsNotNull(m,
                $"seed '{_seed}' prices no AWAY handicap on any matchup — a RE-SEED. Condition 5 "
                + "forbids substituting a home-backed leg, which would pass under the old defect too");

            run.PlaceTicket(new List<Pick> { new Pick(m.Index, hcap) }, 25.0);
            director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);

            // CONDITION 5, asserted off the ENGINE's table rather than off the pick we just made:
            // the anchor must be AWAY. This is the whole premise of the frame.
            Ticket ticket = director.CurrentTicket;
            Leg leg = ticket.Legs[0];
            Assert.AreEqual(Side.Away, MatchModel.AnchorSide(leg),
                $"the engine's anchor for this leg is not AWAY ({MatchModel.AnchorSide(leg)}), so the "
                + "frame would test a case that passes under the old defect as well");

            couch.OnInteract(null);
            yield return WaitUntilOrFail(() => SitSpot.Active != null,
                Time.realtimeSinceStartup + 15f, "player never sat down");

            // CONDITION 1 — the strip line must come from an ANCHOR-INTERPOLATING table. Established
            // POSITIVELY, by generating what SweatFlavor.For can produce for THIS leg at THIS anchor
            // and requiring the rendered line to be one of them. Read off the function rather than a
            // restated copy, so the harness cannot drift from the tables it checks.
            var interpolated = new HashSet<string>();
            foreach (DramaEventType type in new[]
                { DramaEventType.Score, DramaEventType.BigPlay, DramaEventType.Momentum })
                foreach (bool dir in new[] { true, false })
                    for (int step = 0; step < 3; step++)
                        interpolated.Add(SweatFlavor.For(
                            new DramaEvent(0, step, 12, type, dir ? 0.62 : 0.38, TensionTag.Swing),
                            leg, dir, Side.Away));

            string line = null;
            float deadline = Time.realtimeSinceStartup + 900f;
            while (Time.realtimeSinceStartup < deadline)
            {
                string now = screen.DebugFlavorText;
                if (!string.IsNullOrEmpty(now) && interpolated.Contains(now)) { line = now; break; }
                if (screen.RevealedView.ClockText == "FT" || run.Phase != Phase.Sweat) break;
                yield return null;
            }
            Assert.IsNotNull(line,
                "no strip line from an anchor-interpolating table appeared before full time, so this "
                + "burst would prove nothing about the anchor. Last strip text: "
                + $"'{screen.DebugFlavorText}'");

            // B1 — THE BINARY, and NOT the pre-commitment's literal wording, which the frames
            // disproved. B1 reads "the club the strip names is the club the leg backs". THAT HOLDS
            // ONLY ON UP-BEATS: the tables interpolate {picked} when the number rises and {other}
            // when it falls, so on a DOWN-beat the correct line names the club he did NOT back. The
            // first shot failed here on `Gravediggers pass it around, slow and mean.` — MomDown's
            // {other} slot, filled with the home club on an away-anchored leg, which is the anchor
            // WORKING.
            //
            // The discriminating form is direction-free and strictly stronger: the rendered line must
            // be producible at anchor AWAY and NOT producible at anchor HOME. The two sets are
            // disjoint line by line — at anchor Home that same template reads "{away club} pass it
            // around" — so this proves WHICH anchor produced the line rather than which club appears
            // in it.
            string home = SweatFlavor.Short(m.Home.Name), away = SweatFlavor.Short(m.Away.Name);
            Assume.That(home, Is.Not.EqualTo(away), "the two clubs must differ for this to test anything");

            var atHome = new HashSet<string>();
            foreach (DramaEventType type in new[]
                { DramaEventType.Score, DramaEventType.BigPlay, DramaEventType.Momentum })
                foreach (bool dir in new[] { true, false })
                    for (int step = 0; step < 3; step++)
                        atHome.Add(SweatFlavor.For(
                            new DramaEvent(0, step, 12, type, dir ? 0.62 : 0.38, TensionTag.Swing),
                            leg, dir, Side.Home));

            Assert.IsTrue(interpolated.Contains(line),
                $"the strip line is not producible at anchor AWAY. strip='{line}'");
            Assert.IsFalse(atHome.Contains(line),
                "the strip line is EQUALLY producible at anchor HOME, so it cannot show which anchor "
                + $"rendered it — K17-cl's defect would look identical. strip='{line}'");

            // CONDITION 6 — three zones, and the ROW must actually carry the backed club or the
            // frame has an anchor with nothing to check it against.
            //
            // READ PER ROW, NEVER PER SPAN, and the first shot proved why: it read the compact line
            // alone and got the empty string. A LIVE row BLANKS its compact line by design and
            // carries its identity on the NEED span instead — T130's own summary states exactly this
            // ("emptiness of a SPAN is normal and correct; emptiness of the WHOLE ROW is the
            // defect"), and that per-ROW discipline is the right reading of condition 6 too.
            string rowLine = LegRowLineTextInHarness(screen, 0) ?? string.Empty;
            string rowNeed = screen.DebugLegNeed(0) ?? string.Empty;
            string rowProgress = screen.DebugLegProgress(0) ?? string.Empty;
            string row = string.Join(" | ", rowLine, rowNeed, rowProgress);
            Assert.IsNotEmpty(row.Replace("|", string.Empty).Trim(),
                "the leg row carries no text in ANY span, so condition 6's third zone is not on screen");
            // CASE-INSENSITIVE, and the last shot is why. The row's name comes through MarketSheet,
            // which UPPERCASES at the presentation layer exactly where RowGeometry.OfferRow does
            // (S96, §6.5) — it read `DULUTH AUDITORS`. SweatFlavor.Short does NOT uppercase; it
            // returns `Auditors`. Comparing them case-sensitively failed on a row that named the
            // backed club perfectly well, which would have read as a build defect and was a test one.
            Assert.IsTrue(row.ToUpperInvariant().Contains(away.ToUpperInvariant()),
                "the leg's own row does not name the backed club in any span, so the frame cannot "
                + $"show the agreement condition 6 is about. row='{row}' backed='{away}'");

            Debug.Log($"[ANCHOR-B] row='{row}' strip='{line}' home='{home}' away='{away}' "
                + $"score='{screen.RevealedView.ScoreText}' clock={screen.RevealedView.ClockText}");

            // C55 on all THREE subjects — the agreement is the subject, so a frame missing any one
            // of them cannot be read.
            AssertSubjectInFrame(screen, "LegRowLine0", "anchor-side-away-handicap");
            AssertSubjectInFrame(screen, "Flavor", "anchor-side-away-handicap");
            AssertSubjectInFrame(screen, "Matchup", "anchor-side-away-handicap");

            yield return CaptureBurst(screen, cam, "anchor-side-away-handicap", 60, 0f);

            Debug.Log($"[TvSweatCaptureHarness] seed={_seed} frame B complete -> {OutputDir}");
        }

        /// <summary>The compact statement's text by GameObject name — the same lookup the sweat tests
        /// use. Local to this harness so the two files stay independent.</summary>
        private static string LegRowLineTextInHarness(TvSweatScreen screen, int i)
        {
            foreach (TMP_Text t in screen.GetComponentsInChildren<TMP_Text>(true))
                if (t.gameObject.name == $"LegRowLine{i}") return t.text;
            return null;
        }

        /// <summary>The first offer of <paramref name="kind"/> on this matchup matching
        /// <paramref name="want"/>, read OFF the board and never constructed. Fails with the
        /// caller's own re-seed message when the board does not price it.</summary>
        private static MarketSelection FirstOfferedSelection(
            Matchup matchup, MarketKind kind, System.Func<MarketSelection, bool> want, string reseedMessage)
        {
            foreach (MarketOffer offer in matchup.Markets)
                if (offer.Selection.Kind == kind && want(offer.Selection))
                    return offer.Selection;
            Assert.Fail(reseedMessage);
            return default;
        }

        /// <summary>Arms 2 and 3 share everything except which picks they place, so the shoot lives
        /// here once. Same seed, same goalless matchup, same stake and the same 150-frame
        /// frame-contiguous ending burst as arm 1 — T129's conditions (a), (c) and (d) hold by
        /// construction, and (e)'s payout-slot trace rides CaptureBurst's own per-frame log.</summary>
        private IEnumerator RunGoallessSingleTicketCapture(
            string label, System.Func<int, Matchup, List<Pick>> buildPicks)
        {
            _seed = "GOALLESS-5";
            s_sceneIndex = 0;
            Directory.CreateDirectory(OutputDir);
            TheaterStage.PresentationSeedOverride = StableSeed(_seed);
            Time.captureDeltaTime = 1f / 50f;

            yield return LoadRoom();
            var director = Object.FindAnyObjectByType<RunDirector>();
            var screen = Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director, "RunDirector missing");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");
            Camera cam = Camera.main;   // the SEATED IN-ROOM camera — T129 (d): the room band is
                                        // captured, not cropped, and it carries §6.8's own claim.
            Assert.IsNotNull(cam, "no main camera — the room band would be lost");

            screen.TimeScaleOverride = 1f;
            couch.transitionDuration = 0.01f;
            yield return WaitUntilOrFail(() => director.Run != null,
                Time.realtimeSinceStartup + 10f, "director never started a run");

            director.StartNewRun(_seed);
            Run run = director.Run;
            Assert.AreEqual(Phase.Betting, run.Phase, "a fresh run opens in Betting");

            // Matchup 0 is the seed-searched goalless one, and a Pick addresses Matchup.Index rather
            // than the slate position — the two are not guaranteed to coincide, and using the
            // position is how a ticket that backed a 0-0 draw once came back LOST.
            Matchup m = run.CurrentSlate.Matchups[0];
            Assert.IsNotNull(m, "the goalless matchup is missing from this slate");

            const double Stake = 25.0;
            run.PlaceTicket(buildPicks(m.Index, m), Stake);
            director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);

            // T129 (a)'s premise, checked rather than trusted: both arms' whole claim is a leg
            // set that settles on a 0-0, and this is a FRESH sweat with its own StartNewRun — arm
            // 1's own goalless assertion (same seed, same Matchups[0]) does not carry across test
            // runs. A drifted seed or slate must fail loudly here, exactly as arm 1 fails loudly,
            // rather than quietly capture a ticket that does not settle the way the row claims.
            Assert.IsNotNull(m.StatLine, "the match did not resolve at lock");
            Assert.AreEqual(0, m.StatLine.HomeGoals,
                $"seed '{_seed}' matchup {m.Index} is no longer goalless — arm '{label}' needs a 0-0");
            Assert.AreEqual(0, m.StatLine.AwayGoals,
                $"seed '{_seed}' matchup {m.Index} is no longer goalless — arm '{label}' needs a 0-0");

            couch.OnInteract(null);
            yield return WaitUntilOrFail(() => SitSpot.Active != null,
                Time.realtimeSinceStartup + 15f, "player never sat down");

            float runDeadline = Time.realtimeSinceStartup + 900f;
            yield return WaitUntilOrFail(
                () => screen.RevealedView.ClockText == "FT" || run.Phase != Phase.Sweat,
                runDeadline,
                $"the ending never reached full time · clock='{screen.RevealedView.ClockText}'");
            Assert.AreEqual(Phase.Sweat, run.Phase,
                "the sweat ended before its own whistle could be shot");

            // 150 frames at interval 0 — 3.0 contiguous SIM-seconds from the whistle forward, the
            // length T125's measurement says the ending's second half needs (T129 arm 1's own
            // sizing, reused so the three arms are read on one ruler).
            yield return CaptureBurst(screen, cam, $"goalless-{label}-ending", 150, 0f);

            Debug.Log($"[TvSweatCaptureHarness] seed={_seed} arm '{label}' complete -> {OutputDir}");
        }

        /// <summary>CAPTURE CHARTER 2026-08-16, shoot 2 — THE SIZING PROBE. WRITES NO FRAMES.
        ///
        /// <para><b>Why a probe exists at all.</b> The charter asks for a corners sweat captured
        /// "full sweat end to end, frame-contiguous". Frame-contiguous is <c>intervalSeconds: 0</c>
        /// — one capture per RENDERED frame — and every rendered frame advances the sim by
        /// <c>captureDeltaTime</c> (1/50s). So the frame count of an end-to-end roll is not a dial
        /// this harness chooses: it is <i>fifty times the sweat's sim duration</i>, a quantity
        /// nobody in this lane has ever measured. At the docked sets' measured 2.57 MB/frame, the
        /// difference between a 20-second and a 60-second sweat is the difference between a 2.6 GB
        /// dock and a 7.7 GB one.</para>
        ///
        /// <para><b>Measured, not assumed</b> — this lane's own law, and the reason this runs before
        /// a single frame is spent rather than after a window is burnt. Guessing the cap risks the
        /// two failures that actually matter: an undockable set, or a roll that stops before the
        /// whistle and is therefore not "end to end" at all.</para>
        ///
        /// <para>It also logs the sweat's STATE-CHANGE PROFILE — every clock, score, strip and
        /// revealed-corners transition with the frame it happened on. That is raw observation the
        /// README can quote; <b>this seat draws no conclusion from it.</b> Whether a count bet
        /// watches flat is the Design Director's read, and a harness log is not evidence for it —
        /// the frames are.</para></summary>
        [Explicit("Capture charter 2026-08-16 shoot 2: SIZING PROBE, writes no frames. Run by filter only.")]
        [UnityTest]
        public IEnumerator Probe_CornersSweat_LengthAndStateChanges()
        {
            _seed = "CORNERS-SWEAT-1";
            s_sceneIndex = 0;

            TheaterStage.PresentationSeedOverride = StableSeed(_seed);
            Time.captureDeltaTime = 1f / 50f;

            yield return LoadRoom();

            var director = Object.FindAnyObjectByType<RunDirector>();
            var screen = Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director, "RunDirector missing - run SBR.GrayboxRoomBuilder.Build first.");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");

            screen.TimeScaleOverride = 1f;      // ship pacing: how long the watch actually IS
            couch.transitionDuration = 0.01f;

            yield return WaitUntilOrFail(() => director.Run != null,
                Time.realtimeSinceStartup + 10f, "director never started a run");

            director.StartNewRun(_seed);
            Run run = director.Run;
            Assert.AreEqual(Phase.Betting, run.Phase, "a fresh run opens in Betting");

            // Taken OFF THE BOARD, never constructed — T100's reasoning: the corners line is
            // generated per matchup, so an invented selection may be one this matchup never offers.
            int cornersMatchup = -1;
            MarketSelection cornersSelection = default;
            foreach (Matchup mm in run.CurrentSlate.Matchups)
            {
                foreach (MarketOffer off in mm.Markets)
                {
                    if (off.Selection.Kind != MarketKind.TotalCorners) continue;
                    cornersMatchup = mm.Index;
                    cornersSelection = off.Selection;
                    break;
                }
                if (cornersMatchup >= 0) break;
            }
            Assert.GreaterOrEqual(cornersMatchup, 0,
                "no matchup on this slate offers TotalCorners — this is a re-seed, never a reason to "
                + "probe the moneyline sweat instead");

            // ONE leg, so the roll is the corners watch and nothing else. A second leg would put
            // another market's beats inside a set whose whole subject is this one.
            run.PlaceTicket(new List<Pick> { new Pick(cornersMatchup, cornersSelection) }, 25.0);
            director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);

            couch.OnInteract(null);
            yield return WaitUntilOrFail(() => SitSpot.Active != null,
                Time.realtimeSinceStartup + 15f, "player never sat down");

            int frames = 0, changes = 0;
            string lastClock = null, lastScore = null, lastFlavor = null, lastCounts = null;
            float deadline = Time.realtimeSinceStartup + 420f;

            while (!SweatEnded(director) && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                frames++;

                string clock = screen.RevealedView.ClockText;
                string score = screen.RevealedView.ScoreText;
                string flavor = screen.DebugFlavorText;
                string counts = $"{screen.DebugRevealedCountHome}-{screen.DebugRevealedCountAway}";

                if (clock != lastClock || score != lastScore || flavor != lastFlavor
                    || counts != lastCounts)
                {
                    changes++;
                    Debug.Log($"[probe] f={frames:0000} sim={frames / 50f:0.00}s clock='{clock}' "
                        + $"score='{score}' corners={counts} strip='{flavor}'");
                    lastClock = clock; lastScore = score; lastFlavor = flavor; lastCounts = counts;
                }
            }

            bool ended = SweatEnded(director);
            Debug.Log($"[probe] CORNERS SWEAT SIZING :: frames={frames} sim={frames / 50f:0.00}s "
                + $"stateChanges={changes} reachedEnd={ended} "
                + $"estimatedDockMB={frames * 2.57f:0} (at the docked sets' measured 2.57 MB/frame)");

            Assert.IsTrue(ended,
                $"the sweat did not reach its end inside the probe's own deadline (stopped at "
                + $"{frames} frames). The sizing number is therefore a FLOOR, not the length.");
        }

        /// <summary>CAPTURE CHARTER 2026-08-16, shoot 2 — THE CORNERS SWEAT, END TO END.
        ///
        /// <para><b>THE ARITHMETIC THAT SHAPED THIS SET, stated because it changes what the set can
        /// support.</b> <see cref="Probe_CornersSweat_LengthAndStateChanges"/> measured this exact
        /// sweat at <b>2,221 rendered frames over 44.42 sim-seconds</b>. A literally continuous
        /// frame-contiguous roll is therefore 2,221 frames at the docked sets' measured 2.57 MB —
        /// <b>~5.7 GB</b>, seventeen times the largest set ever docked here (128 frames, 329 MB).
        /// That roll was NOT shot. This set instead tiles the same 44.42 seconds with
        /// frame-contiguous WINDOWS, so the control §0-B69 names (<c>intervalSeconds: 0</c>) holds
        /// inside every window and the whole arc is still represented.</para>
        ///
        /// <para><b>Every window is fired by a LOGICAL STATE CHANGE, never a frame index.</b> The
        /// probe's frame numbers are deliberately not reused: this lane's own rule is that a moment
        /// predicate is a state check, because a frame count silently shoots the wrong beat the
        /// first time host timing moves. So a corner window fires when the revealed count actually
        /// changes, and the ending's windows fire on the score and on FT.</para>
        ///
        /// <para><b>The dead air is shot on purpose and in proportion.</b> A set containing only the
        /// events would show a corners sweat as a sequence of things happening, which is the exact
        /// question under review and would answer it by construction. The periodic window fires
        /// through the stretches where nothing but the minute moves, so those stretches are IN the
        /// set rather than edited out of it.</para>
        ///
        /// <para><b>This seat makes NO claim about whether a count bet watches flat.</b> The frames
        /// are the evidence and the read is the Design Director's.</para></summary>
        [Explicit("Capture charter 2026-08-16 shoot 2: the corners sweat end to end. Run by filter only.")]
        [UnityTest]
        public IEnumerator Capture_CornersSweat_EndToEnd()
        {
            // "CORNERS-SWEAT-1" — the probe's seed, so the measured profile in the doc comment
            // above still describes THIS run. requiredChoice: null keeps today's behaviour
            // byte-for-byte: the first TotalCorners offer of ANY direction, exactly what this body
            // did before it moved into the shared helper below (2026-08-18 extraction, made to let
            // the near-line watch's OVER/UNDER siblings reuse this exact tiling/window scheme).
            yield return RunCornersSweatCapture("CORNERS-SWEAT-1", requiredChoice: null, logTag: "shoot2");
        }

        /// <summary>THE SHARED CORNERS-SWEAT CAPTURE BODY — extracted 2026-08-18 from
        /// <see cref="Capture_CornersSweat_EndToEnd"/> so the near-line watch's two owed shots
        /// (theater spec §8 item 3) can reuse it exactly rather than fork it. Everything below is
        /// that method's original body, UNCHANGED, with only the two things that were ever specific
        /// to one caller pulled into parameters — every other assertion and every comment explaining
        /// why the set is shaped as it is (the tiling-period reasoning, the dead-air floor, the
        /// "re-seed, never shoot some other market's sweat under this set's name" guard) are exactly
        /// as they were.
        ///
        /// <para><paramref name="seed"/> drives <c>RunDirector.StartNewRun</c> and the frame-lock,
        /// same as before.</para>
        ///
        /// <para><paramref name="requiredChoice"/> is the one behavioural fork. <c>null</c> takes the
        /// first <c>TotalCorners</c> offer on the board of ANY direction — <see
        /// cref="Capture_CornersSweat_EndToEnd"/>'s exact original search, preserved so that already
        /// docked set (<c>dd-import/corners-sweat-after-2026-08-18</c>) stays reproducible. Set to a
        /// specific <see cref="MarketChoice"/>, the search additionally requires that direction and,
        /// if the board prices none, FAILS as a re-seed — it never falls back to the other direction
        /// and never constructs a selection the board did not offer, because
        /// <c>Matchup.Odds</c>/<c>PlaceTicket</c> already enforces exactly that at runtime
        /// ("Market selection is not offered"), which has already cost this lane a run.</para>
        ///
        /// <para><paramref name="logTag"/> replaces the old hard-coded <c>[shoot2]</c> log prefix, so
        /// the three sets this helper now drives (end-to-end, near-miss OVER, near-allowance UNDER)
        /// stay separable in one combined log.</para></summary>
        private IEnumerator RunCornersSweatCapture(string seed, MarketChoice? requiredChoice, string logTag)
        {
            // THE PLACEHOLDER MUST FAIL LOUDLY, and it does NOT fail on its own — which is the
            // whole reason this guard exists.
            //
            // The trap-gate seed in TvSweatScreenTests could be left unguarded because its
            // placeholder was refused downstream: PlaceTicket throws for a LINE the board never
            // priced. A SEED is different. Every string is a valid seed, so `new Run("PENDING-...")`
            // builds a perfectly good slate and this capture would shoot a real, complete,
            // structurally-fine set OF THE WRONG MATCH — frames that close their windows and answer
            // nothing. That is this lane's recorded trap in its purest form: a capture can arrive
            // and still not close its item.
            Assert.IsFalse(seed != null && seed.StartsWith("PENDING-", System.StringComparison.Ordinal),
                $"seed '{seed}' is still the un-pinned placeholder. Run "
                + "engine.tests/NearLineSeedSearch.cs, read its [NEAR-LINE] table, and PIN a measured "
                + "seed before shooting. Measure, then pin - never guess, and never let a placeholder "
                + "reach the shutter, because this set would otherwise LOOK correct.");

            _seed = seed;
            s_sceneIndex = 0;
            Directory.CreateDirectory(OutputDir);

            TheaterStage.PresentationSeedOverride = StableSeed(_seed);
            Time.captureDeltaTime = 1f / 50f;

            yield return LoadRoom();

            var director = Object.FindAnyObjectByType<RunDirector>();
            var screen = Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director, "RunDirector missing - run SBR.GrayboxRoomBuilder.Build first.");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");

            Camera cam = Camera.main;
            Assert.IsNotNull(cam, "MainCamera (PlayerCamera) missing - cannot capture without it");

            screen.TimeScaleOverride = 1f;      // ship pacing: the rhythm of the watch IS the subject
            couch.transitionDuration = 0.01f;

            yield return WaitUntilOrFail(() => director.Run != null,
                Time.realtimeSinceStartup + 10f, "director never started a run");

            director.StartNewRun(_seed);
            Run run = director.Run;
            Assert.AreEqual(Phase.Betting, run.Phase, "a fresh run opens in Betting");

            int cornersMatchup = -1;
            MarketSelection cornersSelection = default;
            foreach (Matchup mm in run.CurrentSlate.Matchups)
            {
                foreach (MarketOffer off in mm.Markets)
                {
                    if (off.Selection.Kind != MarketKind.TotalCorners) continue;
                    // The one new filter: null keeps the original "any direction" search; set, the
                    // offer must also match, and this never falls back to the other direction and
                    // never builds a selection the board did not offer.
                    if (requiredChoice.HasValue && off.Selection.Choice != requiredChoice.Value) continue;
                    cornersMatchup = mm.Index;
                    cornersSelection = off.Selection;
                    break;
                }
                if (cornersMatchup >= 0) break;
            }
            Assert.GreaterOrEqual(cornersMatchup, 0,
                requiredChoice.HasValue
                    ? $"no matchup on this slate offers a TotalCorners {requiredChoice.Value} — a "
                        + "re-seed, never a reason to fall back to the other direction or construct "
                        + "a selection the board did not offer"
                    : "no matchup on this slate offers TotalCorners — a re-seed, never a reason to shoot "
                        + "some other market's sweat under this set's name");

            run.PlaceTicket(new List<Pick> { new Pick(cornersMatchup, cornersSelection) }, 25.0);
            director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);

            couch.OnInteract(null);
            yield return WaitUntilOrFail(() => SitSpot.Active != null,
                Time.realtimeSinceStartup + 15f, "player never sat down");

            // The watch as it opens, before anything has happened at all.
            yield return CaptureBurst(screen, cam, "sweat-opens", 10, 0f);

            string lastCounts = $"{screen.DebugRevealedCountHome}-{screen.DebugRevealedCountAway}";
            string lastScore = screen.RevealedView.ScoreText;
            string lastClock = screen.RevealedView.ClockText;
            int corners = 0, deadAir = 0, scoreShots = 0;
            // THE TILING PERIOD, and it is measured FROM THE LAST WINDOW OF ANY KIND rather than
            // from the last event. Keyed to the last EVENT (the first cut of this set) the probe's
            // ~4s corner cadence outran a 6s timer and only 2 dead-air windows ever fired — so the
            // stretches where nothing but the minute moves went unshot, which is precisely the
            // stretch the set was sent to photograph. Uniform tiling cannot have that failure mode.
            const float TilePeriodSeconds = 2.5f;
            float lastWindowSim = 0f;
            int frames = 0;
            float endBy = Time.realtimeSinceStartup + 600f;

            while (!SweatEnded(director) && Time.realtimeSinceStartup < endBy)
            {
                yield return null;
                frames++;

                string counts = $"{screen.DebugRevealedCountHome}-{screen.DebugRevealedCountAway}";
                string score = screen.RevealedView.ScoreText;
                string clock = screen.RevealedView.ClockText;

                if (counts != lastCounts)
                {
                    lastCounts = counts;
                    corners++;
                    Debug.Log($"[{logTag}] corner {corners} at sim={frames / 50f:0.00}s counts={counts} "
                        + $"clock='{clock}' strip='{screen.DebugFlavorText}'");
                    yield return CaptureBurst(screen, cam, $"corner{corners:00}-count-{counts}", 10, 0f);
                    lastWindowSim = frames / 50f;
                }
                else if (score != lastScore)
                {
                    lastScore = score;
                    scoreShots++;
                    yield return CaptureBurst(screen, cam, $"score{scoreShots:00}-reveal", 12, 0f);
                    lastWindowSim = frames / 50f;
                }
                else if (clock != lastClock && clock == "FT")
                {
                    lastClock = clock;
                    yield return CaptureBurst(screen, cam, "full-time", 12, 0f);
                    lastWindowSim = frames / 50f;
                }
                else if (frames / 50f - lastWindowSim >= TilePeriodSeconds)
                {
                    deadAir++;
                    // THE STRETCHES WHERE ONLY THE MINUTE MOVES. Shot deliberately — see the class
                    // note: a set of events only would answer the question it was sent to ask.
                    yield return CaptureBurst(screen, cam, $"deadair{deadAir:00}", 6, 0f);
                    lastWindowSim = frames / 50f;
                }

                lastClock = clock;
            }

            // The grade and the settle — the end of the watch, not merely the end of the match.
            yield return CaptureBurst(screen, cam, "sweat-ends", 12, 0f);

            Assert.IsTrue(SweatEnded(director),
                $"the sweat did not reach its end — this set is NOT end to end (stopped at {frames} frames)");
            Assert.GreaterOrEqual(corners, 1,
                "a corners sweat that never moved its count cannot be read for how a count bet "
                + "watches — that is a re-seed, not a set");
            // A FLOOR ON THE TILING, not a formality. The first cut of this set fired 2 dead-air
            // windows because the timer was keyed to the last EVENT; the set looked complete and
            // had almost none of the flat stretch in it. This fails loudly if that returns.
            Assert.GreaterOrEqual(deadAir, 5,
                $"only {deadAir} dead-air windows fired, so the set is mostly events and cannot "
                + "answer the question it was shot for — the flat stretches must be IN it");

            Debug.Log($"[{logTag}] CORNERS SWEAT SET :: cornerWindows={corners} deadAirWindows={deadAir} "
                + $"scoreWindows={scoreShots} sweptFrames={frames} sim={frames / 50f:0.00}s -> {OutputDir}");
        }

        // PENDING PLACEHOLDER — MEASURE, THEN PIN (this lane's own law; see TvSweatScreenTests.cs's
        // TrapGateSeed for the same discipline on its own gate). This value is not a candidate seed,
        // it is a marker: the real seed is found by engine.tests/NearLineSeedSearch.cs, which has
        // not been written yet, searching for a seed whose OVER corners leg finishes ONE SHORT of
        // its line. Guessing a plausible-looking seed here would be exactly the failure mode
        // "measure, then pin" exists to prevent, so this stays an obvious sentinel until the search
        // runs and hands back a MEASURED replacement. Until then, Capture_CornersNearMiss_
        // OverFailsToCross is EXPECTED to fail at its own precondition (RunCornersSweatCapture's
        // offer-search guard, or simply a capture that does not show a near miss) — that failure is
        // the placeholder doing its job, not a defect in this harness.
        // MEASURED, NOT GUESSED — engine.tests/NearLineSeedSearch.cs, run 2026-08-18 over 40
        // candidates. This is the TIGHTEST possible miss on the board:
        //
        //   seed=APPROACH-WATCH-2  matchup=#0  OVER line=8.5  matchTotal=8  threshold=9
        //   margin=-1  state=Lost  tag=OVER-MISS-BY-1
        //
        // The count reaches 8 against a threshold of 9, so the APPROACH fires at distance 1 and
        // THE TURN NEVER COMES. That is the whole point of the set: §8 item 3 says the ramp's
        // value lives in the case never shot, and a leg that misses by five would be a quiet watch
        // proving nothing about the ramp. Proximity IS the subject, so the smallest margin wins.
        private const string CornersNearMissOverSeed = "APPROACH-WATCH-2";

        /// <summary>THE NEAR-LINE WATCH, OVER ARM (theater spec §8 item 3's owed shot A) — an OVER
        /// corners leg that FAILS TO CROSS its line, ideally finishing exactly one short, so the
        /// near-line watch reaches the approach and the cross itself never comes.
        ///
        /// <para>Pure delegation to <see cref="RunCornersSweatCapture"/> with
        /// <c>requiredChoice: MarketChoice.Over</c> — same tiling/window scheme, same assertions,
        /// same re-seed discipline as <see cref="Capture_CornersSweat_EndToEnd"/>; the only
        /// difference is the seed and the forced OVER direction on the offer search.</para>
        ///
        /// <para><b>What this set will NOT claim.</b> This is not a comfortable winner — the entire
        /// point is a leg that stays under its own line, so the set can show what the watch looks
        /// like when the approach is reached and the turn never comes. Nothing here asserts that the
        /// frames actually READ as a near miss; that is a Design Director call against the frames,
        /// exactly as everywhere else in this file.</para>
        ///
        /// <para><see cref="CornersNearMissOverSeed"/> is a PENDING placeholder — see its own comment.
        /// Until <c>engine.tests/NearLineSeedSearch.cs</c> measures a real seed, this entry point is
        /// expected to fail at its own precondition, and that is correct, not a bug.</para></summary>
        [Explicit("Near-line watch, OVER arm (theater spec §8 item 3): an OVER corners leg that fails "
            + "to cross its line. Seed is a PENDING placeholder (CornersNearMissOverSeed) - fails at "
            + "its own precondition until engine.tests/NearLineSeedSearch.cs fills it in. Run by "
            + "filter only.")]
        [Timeout(900000)]
        [UnityTest]
        public IEnumerator Capture_CornersNearMiss_OverFailsToCross()
        {
            yield return RunCornersSweatCapture(CornersNearMissOverSeed, requiredChoice: MarketChoice.Over,
                logTag: "nearmiss-over");
        }

        // PENDING PLACEHOLDER — same discipline as CornersNearMissOverSeed directly above, and the
        // same law: measure, then pin, never guess. The UNDER mirror's real seed also comes from
        // engine.tests/NearLineSeedSearch.cs (not yet written) — this one searching for a seed whose
        // UNDER corners leg sits near its own allowance. Until that search runs and replaces this
        // sentinel with a MEASURED seed, Capture_CornersUnder_Mirror is EXPECTED to fail at its own
        // precondition — the placeholder doing its job, not a defect.
        // MEASURED, NOT GUESSED — same search, same run:
        //
        //   seed=APPROACH-WATCH-2  matchup=#0  UNDER line=8.5  matchTotal=8  maxAllowed=8
        //   margin=0  state=Won  tag=UNDER-HOLDS-EXACTLY
        //
        // The under holds with NOTHING TO SPARE — `LIMIT 0`, the tightest hold available.
        //
        // AND IT IS THE SAME SEED AND THE SAME MATCH AS THE OVER ARM ABOVE, which is better than
        // two unrelated fixtures and was not designed for — the search found it. One match, one
        // variable (the direction), so the near-line pair is a MATCHED PAIR on the instrument
        // discipline this phase's whole read already rests on: the corners/goals pair differed
        // only by market, and these two differ only by side of the same line.
        //
        // NOTE WHAT THIS ARM IS: a BEFORE-state, not an after. §6 puts the UNDER case OUT of the
        // distance gate's scope — "the mirror distance profile, not in evidence" — so this leg
        // still gets today's flat treatment. The set shows what the mirror looks like UNCHANGED,
        // which is the evidence needed to decide whether it wants a ramp at all. Gating it too
        // would have made this shot measure an invention rather than the question.
        private const string CornersUnderMirrorSeed = "APPROACH-WATCH-2";

        /// <summary>THE NEAR-LINE WATCH, UNDER MIRROR (theater spec §8 item 3's owed shot B) — the
        /// UNDER mirror of <see cref="Capture_CornersNearMiss_OverFailsToCross"/>: an under corners
        /// leg finishing near its own allowance.
        ///
        /// <para>Pure delegation to <see cref="RunCornersSweatCapture"/> with
        /// <c>requiredChoice: MarketChoice.Under</c> — otherwise identical in every respect to the
        /// OVER arm above: same helper, same tiling/window scheme, same assertions, same re-seed
        /// discipline on the offer search (never falls back to OVER, never constructs a
        /// selection).</para>
        ///
        /// <para><b>What this set will NOT claim.</b> Same discipline as the OVER arm: not a
        /// comfortable winner, and this seat asserts nothing about whether the frames read as "near
        /// its allowance" — the frames are the evidence, the read is the Design Director's.</para>
        ///
        /// <para><see cref="CornersUnderMirrorSeed"/> is a PENDING placeholder — see its own comment.
        /// Until <c>engine.tests/NearLineSeedSearch.cs</c> measures a real seed, this entry point is
        /// expected to fail at its own precondition, and that is correct, not a bug.</para></summary>
        [Explicit("Near-line watch, UNDER mirror (theater spec §8 item 3): an under corners leg near "
            + "its allowance. Seed is a PENDING placeholder (CornersUnderMirrorSeed) - fails at its "
            + "own precondition until engine.tests/NearLineSeedSearch.cs fills it in. Run by filter "
            + "only.")]
        [Timeout(900000)]
        [UnityTest]
        public IEnumerator Capture_CornersUnder_Mirror()
        {
            yield return RunCornersSweatCapture(CornersUnderMirrorSeed, requiredChoice: MarketChoice.Under,
                logTag: "nearmiss-under");
        }

        /// <summary>CAPTURE CHARTER 2026-08-16, shoot 3 — THE GOALS CONTROL ARM.
        ///
        /// <para><b>A control arm exists to isolate ONE variable, so everything else is held.</b>
        /// Same seed as <see cref="Capture_CornersSweat_EndToEnd"/>, <b>the same matchup</b> (found
        /// by the identical board search, so the fixture is literally the same match), the same ship
        /// pacing, the same 1/50 frame lock, the same 2560×1440, and the same window scheme fired by
        /// the same logical predicates. The ONLY difference is the market the ticket carries.</para>
        ///
        /// <para><b>Why TOTAL GOALS specifically, and not the moneyline.</b> The corners leg is an
        /// over/under against a line. A moneyline control would change the market's SHAPE as well as
        /// its subject, and the comparison would then be unable to say which of the two produced any
        /// difference. Total goals is the same shape — a running count against a line — over a
        /// different counted thing, which is exactly the variable the flatness question is about.</para>
        ///
        /// <para><b>The structural difference this arm is built to expose.</b> <c>_countLedger</c> is
        /// null unless the live leg is a corners or cards leg, so a goals leg has no count ledger at
        /// all. The count-change branch below therefore cannot fire, and <b>that non-firing is
        /// asserted rather than assumed</b> — if a count event ever fires here, this is not a control
        /// arm and the run says so instead of docking a set that quietly is not one.</para>
        ///
        /// <para><b>This seat still makes no claim.</b> The pair of sets is the evidence; which of
        /// them watches flatter, and whether either does, is the Design Director's read.</para></summary>
        [Explicit("Capture charter 2026-08-16 shoot 3: the goals control arm. Run by filter only.")]
        [UnityTest]
        public IEnumerator Capture_GoalsControl_EndToEnd()
        {
            _seed = "CORNERS-SWEAT-1";   // THE SAME SEED. The control is worthless on a different match.
            s_sceneIndex = 0;
            Directory.CreateDirectory(OutputDir);

            TheaterStage.PresentationSeedOverride = StableSeed(_seed);
            Time.captureDeltaTime = 1f / 50f;

            yield return LoadRoom();

            var director = Object.FindAnyObjectByType<RunDirector>();
            var screen = Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director, "RunDirector missing - run SBR.GrayboxRoomBuilder.Build first.");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");

            Camera cam = Camera.main;
            Assert.IsNotNull(cam, "MainCamera (PlayerCamera) missing - cannot capture without it");

            screen.TimeScaleOverride = 1f;
            couch.transitionDuration = 0.01f;

            yield return WaitUntilOrFail(() => director.Run != null,
                Time.realtimeSinceStartup + 10f, "director never started a run");

            director.StartNewRun(_seed);
            Run run = director.Run;
            Assert.AreEqual(Phase.Betting, run.Phase, "a fresh run opens in Betting");

            // THE SAME MATCHUP, found by the corners arm's identical search, so the two sets watch
            // the same fixture. Then the goals offer is taken off THAT matchup's board.
            int matchup = -1;
            foreach (Matchup mm in run.CurrentSlate.Matchups)
            {
                foreach (MarketOffer off in mm.Markets)
                {
                    if (off.Selection.Kind != MarketKind.TotalCorners) continue;
                    matchup = mm.Index;
                    break;
                }
                if (matchup >= 0) break;
            }
            Assert.GreaterOrEqual(matchup, 0,
                "the corners arm's matchup could not be located — the two sets would not be watching "
                + "the same match and the pair would not be a control");

            MarketSelection goalsSelection = default;
            bool found = false;
            foreach (MarketOffer off in run.CurrentSlate.Matchups[matchup].Markets)
            {
                if (off.Selection.Kind != MarketKind.TotalGoals) continue;
                if (off.Selection.Choice != MarketChoice.Over) continue;   // OVER, matching the corners leg
                goalsSelection = off.Selection;
                found = true;
                break;
            }
            Assert.IsTrue(found,
                "this matchup offers no OVER total-goals line, so the control cannot be built on it");

            run.PlaceTicket(new List<Pick> { new Pick(matchup, goalsSelection) }, 25.0);
            director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);

            couch.OnInteract(null);
            yield return WaitUntilOrFail(() => SitSpot.Active != null,
                Time.realtimeSinceStartup + 15f, "player never sat down");

            yield return CaptureBurst(screen, cam, "sweat-opens", 10, 0f);

            string lastCounts = $"{screen.DebugRevealedCountHome}-{screen.DebugRevealedCountAway}";
            string lastScore = screen.RevealedView.ScoreText;
            string lastClock = screen.RevealedView.ClockText;
            int countEvents = 0, deadAir = 0, scoreShots = 0;
            const float TilePeriodSeconds = 2.5f;   // identical to the corners arm
            float lastWindowSim = 0f;
            int frames = 0;
            float endBy = Time.realtimeSinceStartup + 600f;

            while (!SweatEnded(director) && Time.realtimeSinceStartup < endBy)
            {
                yield return null;
                frames++;

                string counts = $"{screen.DebugRevealedCountHome}-{screen.DebugRevealedCountAway}";
                string score = screen.RevealedView.ScoreText;
                string clock = screen.RevealedView.ClockText;

                if (counts != lastCounts)
                {
                    lastCounts = counts;
                    countEvents++;
                    Debug.Log($"[shoot3] UNEXPECTED count event {countEvents} at sim={frames / 50f:0.00}s "
                        + $"counts={counts} — a goals leg should have no count ledger");
                    yield return CaptureBurst(screen, cam, $"count{countEvents:00}-{counts}", 10, 0f);
                    lastWindowSim = frames / 50f;
                }
                else if (score != lastScore)
                {
                    lastScore = score;
                    scoreShots++;
                    Debug.Log($"[shoot3] goal {scoreShots} at sim={frames / 50f:0.00}s score='{score}' "
                        + $"clock='{clock}' strip='{screen.DebugFlavorText}'");
                    yield return CaptureBurst(screen, cam, $"goal{scoreShots:00}-{clock}", 12, 0f);
                    lastWindowSim = frames / 50f;
                }
                else if (clock != lastClock && clock == "FT")
                {
                    lastClock = clock;
                    yield return CaptureBurst(screen, cam, "full-time", 12, 0f);
                    lastWindowSim = frames / 50f;
                }
                else if (frames / 50f - lastWindowSim >= TilePeriodSeconds)
                {
                    deadAir++;
                    yield return CaptureBurst(screen, cam, $"deadair{deadAir:00}", 6, 0f);
                    lastWindowSim = frames / 50f;
                }

                lastClock = clock;
            }

            yield return CaptureBurst(screen, cam, "sweat-ends", 12, 0f);

            Assert.IsTrue(SweatEnded(director),
                $"the sweat did not reach its end — this set is NOT end to end (stopped at {frames} frames)");
            // THE CONTROL'S OWN DEFINING PROPERTY, asserted rather than described. A goals leg has no
            // count ledger; if one moved here the two sets differ by more than their market and the
            // pair cannot be read as a control.
            Assert.AreEqual(0, countEvents,
                $"{countEvents} count events fired on a goals leg — this is not a control arm");
            Assert.GreaterOrEqual(deadAir, 5,
                $"only {deadAir} dead-air windows fired, so the set is mostly events and cannot be "
                + "compared against the corners arm's tiling");

            Debug.Log($"[shoot3] GOALS CONTROL SET :: goalWindows={scoreShots} deadAirWindows={deadAir} "
                + $"countEvents={countEvents} sweptFrames={frames} sim={frames / 50f:0.00}s "
                + $"countLedger={screen.DebugRevealedCountHome} -> {OutputDir}");
        }

        /// <summary>The match minute a clock string is showing, or -1 for the non-minute states
        /// (`PRE`, `FT`, `90'+2`). Deliberately narrow: it exists to say "we are mid-match", so a
        /// stoppage or a terminal clock answering -1 is the correct answer, not a parse failure.</summary>
        private static int MinuteOf(string clock)
        {
            if (string.IsNullOrEmpty(clock) || !clock.EndsWith("'")) return -1;
            return int.TryParse(clock.Substring(0, clock.Length - 1), out int m) ? m : -1;
        }

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
                // The STRIP TEXT rides in the per-frame line (batch 69): T87-am2 is verifiable only as
                // "the line was visible, for multiple frames, before the grade", and a set whose whole
                // claim is about what the strip said should be able to answer that from its own log
                // rather than from a second instrument.
                // T129 condition (e) and T128's carried diagnosis, both added to the EXISTING
                // per-frame line rather than to a second logger.
                //
                // (e) says every ending must run PAST ITS OWN TALLY, "verified by the payout slot
                // changing and then settling". That is only verifiable if the slot is recorded per
                // frame — "150 frames ought to be enough" is not evidence, and a window that ends
                // mid-tally cannot answer whether the ending resolves.
                //
                // T128 asks its question be carried into this window "as an assertion rather than
                // left to be noticed": on the docked pre-T108 set, at FT on a settled 0-0, the
                // column held `MIDDLEMEN TO WIN` for 51 frames and `RISK $25` on a WON ticket.
                // T108 has since been built and verified on CORNERS material but never on a drawn
                // ending, where the NEED sits at full time for a full second rather than passing
                // through. So the leg row and the footer word are recorded here too.
                //
                // LOGGED, NOT ASSERTED. Either answer produces the same ruling, and which one the
                // frames show is the lead's to report rather than this harness's to presuppose.
                Debug.Log($"[TvSweatCaptureHarness] {file} :: score='{screen.RevealedView.ScoreText}' " +
                    $"clock='{screen.RevealedView.ClockText}' suspended={screen.RevealedView.MarketSuspended} " +
                    $"strip='{screen.DebugFlavorText}' " +
                    $"cashout='{screen.DebugCashOutText}' footer='{screen.DebugTicketRiskText}' " +
                    $"pays='{screen.DebugTicketPaysText}' " +
                    $"leg0need='{screen.DebugLegNeed(0)}' leg0prog='{screen.DebugLegProgress(0)}' " +
                    $"leg0chip='{screen.DebugLegState(0)}'");

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
