using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace SBR.Tests.PlayMode
{
    /// <summary>
    /// M4 PlayMode: the real round loop through the room's surfaces. Tickets are placed through the
    /// engine (DemoTicketPolicy is the auto-play fixture — the laptop UI is a thin renderer over the
    /// EditMode-tested BetslipModel), the director locks, the TV walks the sweats serially while
    /// seated, the round settles and the shop opens. Also: standing mid-sweat freezes the cursor
    /// (design/04), and a zero-ticket lock settles on the spot.
    /// All waits are wall-clock — batch mode runs unthrottled (M3's lesson).
    /// Requires the scene in EditorBuildSettings - run SBR.GrayboxRoomBuilder.Build first.
    /// </summary>
    public class TvSweatScreenTests
    {
        [UnityTest]
        public IEnumerator FullRound_TwoTickets_SweatsSeriallyToSettleAndShop()
        {
            yield return LoadRoom();

            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            var screen = UnityEngine.Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = UnityEngine.Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director, "RunDirector missing");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");

            screen.TimeScaleOverride = 0.0001f; // fast-forward pacing, beats and cards
            couch.transitionDuration = 0.01f;

            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");
            Run run = director.Run;
            Assert.AreEqual(Phase.Betting, run.Phase, "a fresh run opens in Betting");

            // Ticket 1 via the auto-play fixture; ticket 2 a single-leg bet on an unused matchup.
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            run.PlaceTicket(new List<Pick> { new Pick(UnusedMatchup(run, picks), Side.Home) }, 10);

            director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);
            Assert.AreEqual(2, run.Sweats.Count, "one session per ticket");

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            yield return WaitUntil(
                () => run.Phase == Phase.Shop || run.Phase == Phase.RunWon || run.Phase == Phase.RunLost,
                60f, "the round never settled");

            Assert.IsTrue(director.LastSettle.HasValue, "settle card telemetry missing");
            foreach (Ticket t in run.Tickets)
                Assert.AreNotEqual(TicketState.Open, t.State, "every ticket must reach a terminal state");
            Assert.Greater(run.Bank, 0.0, "bank went non-positive");

            if (run.Phase == Phase.Shop)
            {
                int round = run.Round;
                director.ExitShop();
                Assert.AreEqual(Phase.Betting, run.Phase, "leaving the shop opens the next round");
                Assert.AreEqual(round + 1, run.Round);
                Assert.AreEqual(0, run.Tickets.Count, "the new round starts with a clean slate");
            }
        }

        [UnityTest]
        public IEnumerator StandingMidSweatFreezesTheEventCursor()
        {
            yield return LoadRoom();

            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            var screen = UnityEngine.Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = UnityEngine.Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director);
            Assert.IsNotNull(screen);
            Assert.IsNotNull(couch);

            // Moderate pacing so events land ~70-180ms apart in real time - a broken pause would
            // measurably advance the cursor inside the freeze windows below at any frame rate.
            screen.TimeScaleOverride = 0.15f;
            couch.transitionDuration = 0.01f;

            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");
            Run run = director.Run;

            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            director.LockRound();

            // Before sitting, the sweat must not advance at all (card and steps gate on seated).
            yield return WaitRealtime(0.3f);
            Assert.AreEqual(0, screen.EventsEmitted, "sweat advanced before the player sat down");

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            yield return WaitUntil(() => screen.EventsEmitted >= 2 && !screen.SweatComplete, 20f,
                "sweat never made mid-run progress");

            // Look away / stand: the cursor must freeze.
            screen.ForceSeated(false);
            yield return WaitRealtime(0.3f); // absorb at most one in-flight step
            int frozenAt = screen.EventsEmitted;
            Assert.IsFalse(screen.SweatComplete, "sweat should still be mid-run when we stand");

            yield return WaitRealtime(0.6f); // several event-lengths of real time
            Assert.AreEqual(frozenAt, screen.EventsEmitted, "event cursor advanced while standing");
            Assert.IsFalse(screen.SweatComplete, "sweat should stay paused while standing");
        }

        [UnityTest]
        public IEnumerator ZeroTicketLockSettlesOnTheSpot()
        {
            yield return LoadRoom();

            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            Assert.IsNotNull(director);
            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");

            director.LockRound(); // no tickets: the director settles without TV ceremony

            // Starting bank 350 >= round-1 payment 60, so a no-bet round always pays into the shop.
            Assert.AreEqual(Phase.Shop, director.Run.Phase, "no-bet round should settle straight to Shop");
            Assert.IsTrue(director.LastSettle.HasValue);
            Assert.IsTrue(director.LastSettle.Value.Paid);
        }

        // ---- TVS-H01 regression: CashOutLive and TryCashOut must agree (docs/tv-sweat-refinement/
        // BUG-LEDGER.md, phase-1a-execution-report.md §2.1). All three presses go through the private
        // TryCashOut (via reflection, since batchmode has no keyboard to actually press Interact — see
        // PendingWindowBeat's own `Keyboard.current == null` handling) and the couch's real OnInteract
        // for the stand attempt, exactly mirroring the two independent Update() listeners on one press.

        [UnityTest]
        public IEnumerator Interact_DuringSuspendedMarket_StandsAndDoesNotCashOut()
        {
            yield return LoadRoom();
            (RunDirector director, TvSweatScreen screen, SitSpot couch) = FindTrio();

            screen.TimeScaleOverride = 0.15f;
            couch.transitionDuration = 0.01f;

            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");
            Run run = director.Run;
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run); // 2-3 legs: cash-out eligible
            run.PlaceTicket(picks, stake);
            director.LockRound();

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            yield return WaitUntil(() =>
                director.CurrentSession != null && !director.CurrentSession.IsComplete
                && screen.EventsEmitted >= 1 && screen.RevealedView.MarketSuspended
                && director.CurrentSession.CashOutOffer().HasValue,
                20f, "never observed a suspended market with a live underlying offer");

            Assert.IsFalse(SitSpot.InteractStandSuppressed(),
                "TVS-H01: CashOutLive must not reserve Interact while the market is suspended");

            TicketState stateBefore = director.CurrentTicket.State;
            couch.OnInteract(null);           // the stand attempt
            PressCashOutInteract(screen);     // the same physical press's cash-out attempt

            Assert.IsNull(SitSpot.Active,
                "TVS-H01: Interact must follow the normal stand contract while suspended (VISUAL-DESIGN.md §8.5)");
            Assert.AreEqual(stateBefore, director.CurrentTicket.State,
                "TVS-H01: a suspended market must not accept a cash-out");
            Assert.IsFalse(director.CurrentSession.IsComplete, "cash-out must not have completed the session");
        }

        [UnityTest]
        public IEnumerator Interact_DuringCashOutPriceAnimation_StandsAndDoesNotCashOut()
        {
            yield return LoadRoom();
            (RunDirector director, TvSweatScreen screen, SitSpot couch) = FindTrio();

            // TimeScaleOverride's own tooltip: "1 = ship pacing, tiny = fast-forward". The old 0.2f
            // therefore ran the sweat FIVE TIMES FASTER, and that is what kept killing these two
            // tests: the session sprinted to completion and the live cash-out offer window shut
            // before a tween could be driven into it. Measured at 0.2f: 4 failures in 12 runs, every
            // one of them with the session already null at the timeout. Ship pacing gives the widest
            // window, and it is the honest speed at which to assert a TVS-H01 input contract.
            screen.TimeScaleOverride = 1f;
            // AnimateCashOut's duration is cashOutTickDuration * TimeScaleOverride. The 30f this
            // replaces was compensating for the 0.2f shrink; at ship pacing the original 4f is a
            // real 4s tween, and SetCashOutOffer's per-frame re-targeting keeps the screen in the
            // animating state far longer than the assertions below need.
            screen.cashOutTickDuration = 4f;
            couch.transitionDuration = 0.01f;

            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");
            // C34, and the lesson TvSweatCaptureHarness already paid for in full: PIN THE SEED.
            // Start() rolls a random one, and DemoTicketPolicy derives the ticket's leg count from it
            // (`2 + hash(seed#round) % 2`), so an unpinned run sometimes builds a ticket that dies on
            // an early leg. The sweat then ends after two events and a sustained cash-out offer never
            // exists — the wait is UNSATISFIABLE, not slow. That is why raising deadlines and slowing
            // the pacing each only changed how long it took to fail. Measured unpinned at ship
            // pacing: 3 failures in 30, every one reporting exactly "events emitted: 2" with the
            // session already complete.
            director.StartNewRun(PinnedSweatSeed);
            Run run = director.Run;
            // C34.1: "an unasserted pin is a comment."
            Assert.AreEqual(PinnedSweatSeed, run.Rng.RunSeed,
                "C34: the run is not carrying the pinned seed, so nothing below is reproducible");
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            director.LockRound();

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            // Drive the tween rather than waiting for the simulation to produce one.
            // See DriveCashOutTween: one displacement is not enough, and the reason is instructive.
            yield return DriveCashOutTween(director, screen);

            // TVS-H01's PREMISE, asserted rather than assumed. Everything below is a claim about
            // behaviour "while the price is animating"; if the tween has ended by now the test would
            // pass VACUOUSLY against a settled slot. This is the check that stops a TimeScaleOverride
            // change — this one or a later one — from quietly altering what TVS-H01 means.
            Assert.IsTrue(screen.DebugCashOutAnimating,
                "TVS-H01 premise lost: the cash-out is not animating, so the assertions below would "
                + "not be exercising the animating-price contract at all");

            Assert.IsFalse(SitSpot.InteractStandSuppressed(),
                "TVS-H01: CashOutLive must not reserve Interact while the price is animating");

            TicketState stateBefore = director.CurrentTicket.State;
            couch.OnInteract(null);           // the stand attempt
            PressCashOutInteract(screen);     // the same physical press's cash-out attempt

            Assert.IsNull(SitSpot.Active,
                "TVS-H01: Interact must follow the normal stand contract while the price updates (VISUAL-DESIGN.md §8.5)");
            Assert.AreEqual(stateBefore, director.CurrentTicket.State,
                "TVS-H01: an updating price must not accept a cash-out");
            Assert.IsFalse(director.CurrentSession.IsComplete, "cash-out must not have completed the session");
        }

        [UnityTest]
        public IEnumerator Interact_DuringLegalOpenOffer_CashesOutAndDoesNotStand()
        {
            yield return LoadRoom();
            (RunDirector director, TvSweatScreen screen, SitSpot couch) = FindTrio();

            screen.TimeScaleOverride = 0.15f;
            couch.transitionDuration = 0.01f;

            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");
            Run run = director.Run;
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            director.LockRound();

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            yield return WaitUntil(() =>
                director.CurrentSession != null && !director.CurrentSession.IsComplete
                && screen.EventsEmitted >= 1 && !screen.RevealedView.MarketSuspended
                && !screen.DebugCashOutAnimating
                && director.CurrentSession.CashOutOffer().HasValue,
                20f, "never reached an open, stable cash-out window");

            Assert.IsTrue(SitSpot.InteractStandSuppressed(),
                "an open legal offer must reserve Interact for acceptance (VISUAL-DESIGN.md §8.5)");

            // Stand attempt FIRST, while the offer is still untouched: this is the only ordering that
            // isolates TVS-H01 from the pre-existing, out-of-scope race between PlayerInteractor's and
            // TvSweatScreen's independent per-frame WasPressedThisFrame() polls (whichever fires first
            // in a real frame can observe the other's side effect; that ordering hazard already exists
            // in the unfixed code too and is not part of this dispatch).
            couch.OnInteract(null);
            Assert.IsNotNull(SitSpot.Active,
                "TVS-H01: a legal open offer must not stand the player on Interact");

            PressCashOutInteract(screen);
            Assert.AreEqual(TicketState.CashedOut, director.CurrentTicket.State,
                "TVS-H01: Interact during a legal open offer must cash out");
            Assert.IsTrue(director.CurrentSession.IsComplete);
        }

        // ---- TVS-H02 regression: standing freezes every formerly-unguarded timer exactly, and
        // sitting resumes with no hidden catch-up. Four categories covering the mechanism classes
        // identified in phase-1a-execution-report.md §2.2, rather than one giant test:
        //  A. continuous per-frame animators (ApplyEmission/AnimateBar/AnimateFlavorPunch/
        //     AnimateCashOutTaunt — unconditional every Update());
        //  B. the AnimateCashOut price-tween coroutine;
        //  C. a resolution-effect coroutine (FloodPulse via the cash-out gold flood);
        //  D. the ScaledWait/WaitRealtime family of ceremony/settlement holds, proven through their
        //     functional consequence — standing must not let round progression advance.

        [UnityTest]
        public IEnumerator Standing_Freezes_ContinuousPerFrameAnimators_NoResumeCatchUp()
        {
            yield return LoadRoom();
            (RunDirector director, TvSweatScreen screen, SitSpot couch) = FindTrio();

            screen.TimeScaleOverride = 0.15f;
            couch.transitionDuration = 0.01f;

            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");
            Run run = director.Run;
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            director.LockRound();

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            TMP_Text flavor = FindChildComponent<TMP_Text>(screen, "Flavor");
            Assert.IsNotNull(flavor, "Flavor text not found - canvas layout changed?");

            // Every beat reveal punches the flavor scale to 1.12, then AnimateFlavorPunch decays it
            // back toward 1 at a fixed 1.4/s (unscaled by TimeScaleOverride) - catch it above 1.02 so
            // >= ~70ms of real decay life remains at the moment we stand.
            yield return WaitUntil(() => flavor.rectTransform.localScale.x > 1.02f, 20f,
                "never caught the flavor punch mid-decay");

            float frozen = flavor.rectTransform.localScale.x;
            screen.ForceSeated(false);
            yield return WaitRealtime(0.05f);
            Assert.AreEqual(frozen, flavor.rectTransform.localScale.x, 0.0001f,
                "TVS-H02: flavor punch scale advanced while standing");
            yield return WaitRealtime(0.2f); // several times the natural remaining decay life
            Assert.AreEqual(frozen, flavor.rectTransform.localScale.x, 0.0001f,
                "TVS-H02: flavor punch scale kept decaying (hidden catch-up) while standing");

            screen.ForceSeated(true);
            yield return WaitUntil(() => Mathf.Abs(flavor.rectTransform.localScale.x - frozen) > 0.0001f,
                5f, "flavor punch never resumed decaying after sitting back down");
        }

        [UnityTest]
        public IEnumerator Standing_Freezes_CashOutTween_NoResumeCatchUp()
        {
            yield return LoadRoom();
            (RunDirector director, TvSweatScreen screen, SitSpot couch) = FindTrio();

            // TimeScaleOverride's own tooltip: "1 = ship pacing, tiny = fast-forward". The old 0.2f
            // therefore ran the sweat FIVE TIMES FASTER, and that is what kept killing these two
            // tests: the session sprinted to completion and the live cash-out offer window shut
            // before a tween could be driven into it. Measured at 0.2f: 4 failures in 12 runs, every
            // one of them with the session already null at the timeout. Ship pacing gives the widest
            // window, and it is the honest speed at which to assert a TVS-H01 input contract.
            screen.TimeScaleOverride = 1f;
            // AnimateCashOut's duration is cashOutTickDuration * TimeScaleOverride. The 30f this
            // replaces was compensating for the 0.2f shrink; at ship pacing the original 4f is a
            // real 4s tween, and SetCashOutOffer's per-frame re-targeting keeps the screen in the
            // animating state far longer than the assertions below need.
            screen.cashOutTickDuration = 4f;
            couch.transitionDuration = 0.01f;

            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");
            // C34, and the lesson TvSweatCaptureHarness already paid for in full: PIN THE SEED.
            // Start() rolls a random one, and DemoTicketPolicy derives the ticket's leg count from it
            // (`2 + hash(seed#round) % 2`), so an unpinned run sometimes builds a ticket that dies on
            // an early leg. The sweat then ends after two events and a sustained cash-out offer never
            // exists — the wait is UNSATISFIABLE, not slow. That is why raising deadlines and slowing
            // the pacing each only changed how long it took to fail. Measured unpinned at ship
            // pacing: 3 failures in 30, every one reporting exactly "events emitted: 2" with the
            // session already complete.
            director.StartNewRun(PinnedSweatSeed);
            Run run = director.Run;
            // C34.1: "an unasserted pin is a comment."
            Assert.AreEqual(PinnedSweatSeed, run.Rng.RunSeed,
                "C34: the run is not carrying the pinned seed, so nothing below is reproducible");
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            director.LockRound();

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            // Drive the tween rather than waiting for the simulation to produce one.
            // See DriveCashOutTween: one displacement is not enough, and the reason is instructive.
            yield return DriveCashOutTween(director, screen);

            TMP_Text cashOut = FindChildComponent<TMP_Text>(screen, "CashOut");
            Assert.IsNotNull(cashOut, "CashOut text not found - canvas layout changed?");
            // TVS-H02's premise, same reason: freezing a figure that was not moving proves nothing.
            Assert.IsTrue(screen.DebugCashOutAnimating,
                "TVS-H02 premise lost: nothing was animating, so the freeze assertions below would "
                + "hold trivially");
            string frozen = cashOut.text;

            screen.ForceSeated(false);
            yield return WaitRealtime(0.1f);
            Assert.AreEqual(frozen, cashOut.text, "TVS-H02: cash-out amount kept ticking while standing");
            yield return WaitRealtime(0.3f);
            Assert.AreEqual(frozen, cashOut.text,
                "TVS-H02: cash-out amount kept ticking (hidden catch-up) while standing");

            screen.ForceSeated(true);
            yield return WaitUntil(() => cashOut.text != frozen || !screen.DebugCashOutAnimating, 5f,
                "cash-out tween never resumed after sitting back down");
        }

        [UnityTest]
        public IEnumerator Standing_Freezes_ResolutionEffectFlood_NoResumeCatchUp()
        {
            yield return LoadRoom();
            (RunDirector director, TvSweatScreen screen, SitSpot couch) = FindTrio();

            screen.TimeScaleOverride = 0.3f;
            screen.cashOutFloodDuration = 3f; // widen the flood window for reliable polling
            couch.transitionDuration = 0.01f;

            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");
            Run run = director.Run;
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            director.LockRound();

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            yield return WaitUntil(() =>
                director.CurrentSession != null && !director.CurrentSession.IsComplete
                && screen.EventsEmitted >= 1 && !screen.RevealedView.MarketSuspended
                && !screen.DebugCashOutAnimating
                && director.CurrentSession.CashOutOffer().HasValue,
                20f, "never reached an open cash-out window");

            PressCashOutInteract(screen);
            Assert.AreEqual(TicketState.CashedOut, director.CurrentTicket.State, "setup: cash-out must accept");

            // RE-POINTED batch 27, not deleted. This test's SUBJECT is TVS-H02 — standing freezes the
            // accept beat and sitting resumes it with no catch-up. Its former INSTRUMENT was the
            // gold flood's alpha, and T40 struck the flood. The contract is unchanged; what is
            // observable changed, so the observation moves to the beat's own visible state.
            //
            // The accepted figure holds in the slot for ScaledWait(cashOutFloodDuration) and the slot
            // clears when that wait completes. So "still showing after standing well past the beat's
            // own length" is exactly the freeze, and it is a stronger read than an alpha mid-curve:
            // the beat here runs 1.0 * 0.3 = 0.3s, and the stand below is 1.0s — more than three
            // times over. If the wait advanced at all while standing, the slot would be gone.
            screen.cashOutFloodDuration = 1f;
            TMP_Text figure = FindChildComponent<TMP_Text>(screen, "CashOut");
            Assert.IsNotNull(figure, "CashOut figure not found - canvas layout changed?");

            yield return WaitUntil(() => figure.enabled && figure.text.StartsWith("CASHED OUT"),
                5f, "the accept beat never showed its figure in the slot");

            screen.ForceSeated(false);
            yield return WaitRealtime(1.0f);
            Assert.IsTrue(figure.enabled && figure.text.StartsWith("CASHED OUT"),
                "TVS-H02: the accept beat advanced while standing - the slot cleared, which it can "
                + "only do by completing a wait that is supposed to be frozen");

            screen.ForceSeated(true);
            yield return WaitUntil(() => !figure.enabled, 10f,
                "the accept beat never completed after sitting back down");
        }

        [UnityTest]
        public IEnumerator Standing_Freezes_SettlementHold_NoResumeCatchUp()
        {
            yield return LoadRoom();
            (RunDirector director, TvSweatScreen screen, SitSpot couch) = FindTrio();

            screen.TimeScaleOverride = 0.2f;
            couch.transitionDuration = 0.01f;

            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");
            Run run = director.Run;
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake); // ticket 0: cash-out eligible
            run.PlaceTicket(new List<Pick> { new Pick(UnusedMatchup(run, picks), Side.Home) }, 10); // ticket 1
            director.LockRound();

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            yield return WaitUntil(() =>
                director.CurrentSession != null && !director.CurrentSession.IsComplete
                && screen.EventsEmitted >= 1 && !screen.RevealedView.MarketSuspended
                && !screen.DebugCashOutAnimating
                && director.CurrentSession.CashOutOffer().HasValue,
                20f, "never reached an open cash-out window on ticket 0");

            int sweatIndexBefore = director.SweatIndex;
            PressCashOutInteract(screen);
            Assert.AreEqual(TicketState.CashedOut, director.CurrentTicket.State, "setup: cash-out must accept");

            // Force-stand before yielding a single frame: SettlementBeat's ScaledWait(cashOutFloodDuration)
            // has not started counting yet (PlaySweat/WaitSceneDone need a few frames to unwind onto it),
            // so this reliably catches the hold from its very first tick.
            screen.ForceSeated(false);

            yield return WaitRealtime(1.0f); // many multiples of cashOutFloodDuration * TimeScaleOverride
            Assert.AreEqual(sweatIndexBefore, director.SweatIndex,
                "TVS-H02: the cash-out settlement hold advanced to the next ticket while standing");
            Assert.AreEqual(Phase.Sweat, run.Phase, "TVS-H02: round progression must not advance while standing");

            screen.ForceSeated(true);
            yield return WaitUntil(() => director.SweatIndex != sweatIndexBefore || run.Phase != Phase.Sweat,
                20f, "settlement never resumed after sitting back down");
        }

        // ---- helpers ----

        /// <summary>T75-am: `_tBigAmount` owes an assignment and an assertion, not a frame.
        ///
        /// <para>The DD's original carve-out asked for that slot to be verified tabular on frames.
        /// It cannot be — the element renders nothing since both payoff figures moved into the
        /// cash-out slot (T68-am/T71), so it appears in no capture and never will. T75-am re-cast
        /// the debt, and this is where it is paid.</para>
        ///
        /// <para>`AreSame`, not `AreEqual`: the ruling is that the slot carries the SHARED regular
        /// asset. A per-slot copy would satisfy any check written against the font's name, pass
        /// review, and quietly double the atlas.</para></summary>
        [UnityTest]
        public IEnumerator BigAmount_CarriesTheSharedRegularFontAsset()
        {
            yield return LoadRoom();
            (_, TvSweatScreen screen, _) = FindTrio();

            Assert.IsNotNull(screen.DebugRegularFont,
                "the regular TV face did not load — every assertion below would be vacuous");
            Assert.IsNotNull(screen.DebugBigAmountFont,
                "T75-am: BigAmount has no font asset assigned at all");
            Assert.AreSame(screen.DebugRegularFont, screen.DebugBigAmountFont,
                "T75-am: BigAmount must carry the SHARED regular asset, not an equal-looking copy");
        }

        /// <summary>The run seed the two mid-tween tests pin (C34).
        ///
        /// <para><c>48151623</c> is <c>CaptureSeeds[0]</c> from TvSweatCaptureHarness — the one seed
        /// in this repo with a written record of sustaining a full sweat. That harness documents the
        /// same defect these tests hit: "Seed 01 completes in 60s; seeds 02–05 burned the full budget
        /// waiting for something that was never going to happen," and "that is exactly why this
        /// harness failed on four seeds and passed on one, from the same code." The flood-removal
        /// frames were shot on it too.</para></summary>
        private const string PinnedSweatSeed = "48151623";

        /// <summary>How far the mid-tween tests displace the shown cash-out figure.
        ///
        /// <para>Deliberately far larger than the 0.005 that arms a tween. SetCashOutOffer re-targets
        /// every frame while the shown figure is still more than 0.005 from the live offer, so the
        /// screen converges on the real price exponentially and stays in the animating state for
        /// thousands of frames at these durations. The assertions downstream therefore cannot race
        /// the tween's end — which is the property the old "catch it in flight" wait lacked.</para>
        ///
        /// <para>Positive, so the shown figure never goes negative on a small offer. The cost is
        /// that the next real offer reads as a DROP and arms the gold taunt (_cashOutFlash) — a
        /// cosmetic side effect neither of these tests asserts on, noted so it is not a surprise
        /// to whoever reads a frame from one of them.</para></summary>
        private const double CashOutDisplacement = 60.0;

        /// <summary>Put the screen into a cash-out tween and leave it there, without depending on
        /// the simulation to move the price.
        ///
        /// <para>Displacing the shown figure only BECOMES a tween when SetCashOutOffer next runs
        /// with a live offer — and the market closes and reopens repeatedly across a sweat, so a
        /// lone displacement can sit unconsumed for an arbitrary stretch. That is exactly how the
        /// first version of this hardening failed: it displaced once, then waited a fixed 5s, and
        /// the market happened to be shut for all of it. Trading a wide timing dependency for a
        /// narrow one is not the same as removing it.</para>
        ///
        /// <para>So: displace whenever the offer is live, and keep doing it until the tween is
        /// actually observed. Once live, the very next Update consumes the displacement, so this
        /// costs a frame or two — it is a retry, not a spin. The failure carries the state that
        /// explains it, because the first version's message did not and cost a diagnosis.</para>
        /// </summary>
        private static IEnumerator DriveCashOutTween(
            // 60s, not 25: at ship pacing the sweat's first events arrive five times later than they
            // did at the old 0.2f fast-forward, and this budget must cover reaching a live offer.
            RunDirector director, TvSweatScreen screen, float maxSeconds = 60f)
        {
            float start = Time.realtimeSinceStartup;
            bool everLive = false;
            while (!screen.DebugCashOutAnimating)
            {
                if (Time.realtimeSinceStartup - start > maxSeconds)
                {
                    Assert.Fail(
                        $"never drove the cash-out into a tween (waited {maxSeconds}s) — " +
                        $"offer was live at least once: {everLive}; " +
                        $"figure shown: {screen.DebugHasCashOutShown}; " +
                        $"events emitted: {screen.EventsEmitted}; " +
                        $"session complete: {director.CurrentSession?.IsComplete}; " +
                        $"offer now: {director.CurrentSession?.CashOutOffer()}");
                    yield break;
                }

                bool live = director.CurrentSession != null
                            && !director.CurrentSession.IsComplete
                            && screen.EventsEmitted >= 1
                            && director.CurrentSession.CashOutOffer().HasValue
                            && screen.DebugHasCashOutShown;
                if (live)
                {
                    everLive = true;
                    screen.ForceCashOutDisplacement(CashOutDisplacement);
                }
                yield return null;
            }
        }

        // ---- T88 / C48: the gesture's own falsifiers ---------------------------------------------
        //
        // EVERY cash-out test above reaches the accept through `PressCashOutInteract`, which invokes
        // the private TryCashOut by reflection because "batchmode has no keyboard to actually press
        // Interact". THAT BYPASS IS WHY PRESS-TO-COMMIT SURVIVED A WHOLE PHASE. The money control's
        // accept path was only ever exercised from BELOW the input layer, and the gesture the copy
        // promised lives above it — so no test here could notice that a press committed instantly,
        // and none did. When the input changed under T88, the suite stayed green either way.
        //
        // A test that cannot press the key cannot falsify the fix. These add a VIRTUAL keyboard and
        // drive the real InputAction: `InputSystem.AddDevice` is in the runtime assembly rather than
        // the input test framework, so a headless run can hold a key down after all.
        //
        // The device is added INSIDE each test and removed after, deliberately not in a [SetUp]:
        // PendingWindowBeat declines immediately when `Keyboard.current == null`, which is what stops
        // batch autoplay hanging on the pending window, so a keyboard present for every test in this
        // class would change the behaviour of tests that have nothing to do with input.

        /// <summary>Queue a keyboard state and let the FRAME process it. Deliberately not followed by
        /// a manual <c>InputSystem.Update()</c>: `wasPressedThisFrame` is only true during the frame
        /// the event is processed in, so the press has to land in the same input update the
        /// MonoBehaviour Update() reads — which is what yielding one frame gives.</summary>
        private static void HoldKeys(Keyboard kb, params Key[] down)
            => InputSystem.QueueStateEvent(kb, new KeyboardState(down));

        /// <summary>A HELD key does not survive a headless run without this, and the first version of
        /// these tests did not know it.
        ///
        /// <para>Batch mode is never focused, and the Input System's documented response to lost focus
        /// is to <c>ResetDevice</c> every device that cannot run in the background. So a queued key
        /// registered and was then wiped before the next frame read it — which is invisible in a test
        /// whose assertion is that NOTHING happened. The press-commits-nothing pin passed green while
        /// the key was never actually down: S51's shape exactly, a suite going green by recording
        /// nothing. Both tests now prove the input ARRIVED before they assert what it did not do.</para></summary>
        private static (InputSettings.BackgroundBehavior, InputSettings.EditorInputBehaviorInPlayMode) LetDevicesRunUnfocused()
        {
            var previous = (InputSystem.settings.backgroundBehavior,
                            InputSystem.settings.editorInputBehaviorInPlayMode);
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            return previous;
        }

        private static void RestoreFocusBehaviour(
            (InputSettings.BackgroundBehavior, InputSettings.EditorInputBehaviorInPlayMode) previous)
        {
            InputSystem.settings.backgroundBehavior = previous.Item1;
            InputSystem.settings.editorInputBehaviorInPlayMode = previous.Item2;
        }

        /// <summary>Reads the private preview amount — non-zero means the hold actually reached
        /// §8.10's machinery, which is the precondition every "and then nothing happened" assertion
        /// below depends on.</summary>
        private static double PreviewAmount(TvSweatScreen screen)
            => (double)typeof(TvSweatScreen)
                .GetField("_cashOutPreviewAmount", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(screen);

        [UnityTest]
        public IEnumerator T88_a_press_commits_nothing_and_release_abandons()
        {
            yield return LoadRoom();
            (RunDirector director, TvSweatScreen screen, SitSpot couch) = FindTrio();
            screen.TimeScaleOverride = 0.15f;
            couch.transitionDuration = 0.01f;

            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");
            Run run = director.Run;
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            director.LockRound();

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            var previousFocus = LetDevicesRunUnfocused();
            Keyboard kb = InputSystem.AddDevice<Keyboard>();
            try
            {
                yield return WaitUntil(() => SitSpot.InteractStandSuppressed != null
                    && SitSpot.InteractStandSuppressed(), 25f, "never observed a live acceptable offer");

                TicketState before = director.CurrentTicket.State;

                // HOLD E. Five frames is far past the one frame the old input needed to spend it.
                HoldKeys(kb, Key.E);
                for (int i = 0; i < 5; i++) yield return null;

                // THE PRECONDITION, asserted first. Without it every assertion below passes whenever
                // the key silently failed to arrive, which is precisely how this test first went green.
                Assert.IsTrue(kb.eKey.isPressed, "the virtual key never stayed down — the test proves nothing");
                Assert.Greater(PreviewAmount(screen), 0.0,
                    "the hold must ENTER §8.10's preview; if it did not, 'the press committed nothing' is unfalsifiable");

                Assert.AreEqual(before, director.CurrentTicket.State,
                    "T88: a press must commit NOTHING — this is the defect itself, money spent on the first frame of input");
                Assert.IsNotNull(SitSpot.Active,
                    "the hold must not stand the player up: a fresh press on the press-path is the hazard room's SitSpot answer named");

                // RELEASE ABANDONS — always, T22.
                HoldKeys(kb);
                for (int i = 0; i < 3; i++) yield return null;

                Assert.AreEqual(before, director.CurrentTicket.State,
                    "T88: release abandons — a hold that ends without the second key must leave the ticket untouched");
                Assert.AreEqual(0.0, PreviewAmount(screen),
                    "release is a full revert (§8.10): the preview must be gone, not merely uncommitted");
            }
            finally { InputSystem.RemoveDevice(kb); RestoreFocusBehaviour(previousFocus); }
        }

        [UnityTest]
        public IEnumerator T88_the_second_key_during_the_hold_commits_the_previewed_amount()
        {
            yield return LoadRoom();
            (RunDirector director, TvSweatScreen screen, SitSpot couch) = FindTrio();
            screen.TimeScaleOverride = 0.15f;
            couch.transitionDuration = 0.01f;

            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");
            Run run = director.Run;
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            director.LockRound();

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            var previousFocus = LetDevicesRunUnfocused();
            Keyboard kb = InputSystem.AddDevice<Keyboard>();
            try
            {
                yield return WaitUntil(() => SitSpot.InteractStandSuppressed != null
                    && SitSpot.InteractStandSuppressed(), 25f, "never observed a live acceptable offer");

                HoldKeys(kb, Key.E);
                yield return null;
                yield return null;

                // The preview is what the player is being shown, and §8.10 says the accepted number
                // IS that number. Read it before the commit clears it.
                double previewed = PreviewAmount(screen);
                Assert.Greater(previewed, 0.0,
                    "holding E must ENTER the preview — §8.10's machinery had no caller at all before T88");

                // The second key, during the hold.
                HoldKeys(kb, Key.E, Key.Enter);
                yield return null;
                yield return null;

                Assert.AreEqual(TicketState.CashedOut, director.CurrentTicket.State,
                    "T88: a second key during the hold COMMITS — otherwise the control cannot be finished at all");

                FieldInfo lastAmount = typeof(TvSweatScreen)
                    .GetField("_lastCashOutAmount", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.AreEqual(previewed, (double)lastAmount.GetValue(screen),
                    "§8.10: the previewed and accepted numbers can never differ — T59's worst outcome on a money control");
            }
            finally { InputSystem.RemoveDevice(kb); RestoreFocusBehaviour(previousFocus); }
        }

        /// <summary>The pending-loss window is seed-decided, so the seed is PINNED and the pin is
        /// ASSERTED — a window that stops opening must fail loudly rather than let the gesture
        /// assertions pass on a sweat that never reached them.
        ///
        /// <para><b>Not searched for: taken from the engine's own table.</b>
        /// `CharmExpansionTests.Whistle_rescues_at_full_odds_or_busts_honestly` pins this same seed
        /// with the same hand-built pair and records what it produces — <i>"leg 0 (matchup 1, Home)
        /// dies; leg 1 (matchup 0, Away) would win"</i> — and that test is green in the engine suite.
        /// Reusing it means the two suites move together instead of drifting onto separate seeds that
        /// each look fine alone.</para>
        ///
        /// <para>The ticket is HAND-BUILT rather than taken from `DemoTicketPolicy`, the capture
        /// harness's precedent for the same reason: the policy's picks are moneyline-only and chosen
        /// from whatever the slate offers, so they are not the pair the engine's pin describes.
        /// Both consumables are granted, which is also what OPENS the window at all — the session
        /// suspends only when a legal save is held.</para></summary>
        private const string PendingLossSeed = "GOLDEN-W2";

        private static ConsumableDefinition Consumable(string id)
        {
            foreach (ConsumableDefinition c in RelicCatalog.Consumables)
                if (c.Id == id) return c;
            throw new ArgumentException($"no consumable '{id}' in RelicCatalog");
        }

        [UnityTest]
        public IEnumerator T88_the_intervention_prompt_spends_nothing_on_a_press_and_commits_on_the_second_key()
        {
            yield return LoadRoom();
            (RunDirector director, TvSweatScreen screen, SitSpot couch) = FindTrio();
            screen.TimeScaleOverride = 0.15f;
            couch.transitionDuration = 0.01f;

            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");

            director.StartNewRun(PendingLossSeed);   // overrides whatever Start() rolled
            Run run = director.Run;
            run.GrantConsumable(Consumable("mulligan_slip"));
            run.GrantConsumable(Consumable("refs_whistle"));
            run.PlaceTicket(new List<Pick> { new Pick(1, Side.Home), new Pick(0, Side.Away) }, 20);
            director.LockRound();

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            var previousFocus = LetDevicesRunUnfocused();
            Keyboard kb = InputSystem.AddDevice<Keyboard>();
            try
            {
                // Found by object NAME rather than by private field, the way the extent sweep finds
                // it: the name is what the instrument and the DD's table already refer to, and it
                // does not couple this test to a field's lifecycle.
                TMP_Text prompt = null;
                foreach (TMP_Text t in screen.GetComponentsInChildren<TMP_Text>(true))
                    if (t.gameObject.name == "InterventionPrompt") { prompt = t; break; }
                Assert.IsNotNull(prompt, "InterventionPrompt is not built on this screen");

                // THE PIN-ASSERT, and it waits on the RENDERED state rather than the engine's.
                // `HasPendingLoss` goes true the moment the session suspends, but the theatre reaches
                // PendingWindowBeat some frames later — waiting on the engine flag alone pressed the
                // key at a surface that had not drawn the prompt yet. The gesture acts on what is
                // shown, so what is shown is the precondition.
                yield return WaitUntil(() => director.CurrentSession != null
                    && director.CurrentSession.HasPendingLoss && prompt.enabled, 40f,
                    $"seed '{PendingLossSeed}' did not put a rendered pending-loss window on the surface — " +
                    "the pin has drifted. Re-derive it from CharmExpansionTests' table before trusting anything below.");

                Assert.IsTrue(run.OwnsConsumable("mulligan_slip"), "precondition: the slip must be held");

                // HOLD M. A press spends nothing.
                HoldKeys(kb, Key.M);
                for (int i = 0; i < 5; i++) yield return null;

                Assert.IsTrue(kb.mKey.isPressed, "the virtual key never stayed down — the test proves nothing");
                string held = prompt.text ?? "<null>";
                int screens = UnityEngine.Object
                    .FindObjectsByType<TvSweatScreen>(FindObjectsSortMode.None).Length;
                Assert.IsTrue(held.Contains("CONFIRMS"),
                    "the hold must render its PREVIEW — the option, its cost, and how to finish or " +
                    $"abandon it. Actual: '{held.Replace("\n", "\\n")}' · promptEnabled={prompt.enabled}" +
                    $" · screensInScene={screens} · sameScreen={ReferenceEquals(prompt.transform.root.GetComponentInChildren<TvSweatScreen>(true), screen)}");
                Assert.IsTrue(run.OwnsConsumable("mulligan_slip"),
                    "T88: a press must spend NOTHING — this is the irreversible spend on one frame of input");
                Assert.IsTrue(director.CurrentSession.HasPendingLoss,
                    "T88: the window must still be open — a press resolved the leg's grading");

                // RELEASE ABANDONS.
                HoldKeys(kb);
                for (int i = 0; i < 3; i++) yield return null;

                Assert.IsTrue(run.OwnsConsumable("mulligan_slip"), "T88: release abandons — nothing is spent");
                Assert.IsTrue(director.CurrentSession.HasPendingLoss, "T88: release leaves the window open");
                // Batch 56: the decline lost its HOLD, because it takes a press (T88(c)).
                Assert.IsTrue(prompt.text.Contains("N LET IT DIE"),
                    "release returns the OFFER LIST — the preview must not be residue. " +
                    $"Actual: '{prompt.text.Replace("\n", "\\n")}'");

                // THE SECOND KEY DURING THE HOLD COMMITS.
                HoldKeys(kb, Key.M);
                yield return null;
                yield return null;
                HoldKeys(kb, Key.M, Key.Enter);
                for (int i = 0; i < 4; i++) yield return null;

                Assert.IsFalse(run.OwnsConsumable("mulligan_slip"),
                    "T88: a second key during the hold COMMITS — the slip is spent");
                Assert.IsFalse(director.CurrentSession.HasPendingLoss,
                    "T88: committing resolves the window");
            }
            finally { InputSystem.RemoveDevice(kb); RestoreFocusBehaviour(previousFocus); }
        }

        private static (RunDirector, TvSweatScreen, SitSpot) FindTrio()
        {
            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            var screen = UnityEngine.Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = UnityEngine.Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director, "RunDirector missing");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");
            return (director, screen, couch);
        }

        /// <summary>Invokes TvSweatScreen's private TryCashOut exactly as Update() does on an Interact
        /// press. Batchmode has no keyboard device (PendingWindowBeat's own `Keyboard.current == null`
        /// branch shows this is a known property of this harness), so a real key press cannot be
        /// simulated; reflection calls the same production method instead of duplicating its logic.</summary>
        private static void PressCashOutInteract(TvSweatScreen screen)
        {
            MethodInfo method = typeof(TvSweatScreen).GetMethod("TryCashOut",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "TvSweatScreen.TryCashOut not found by reflection - was it renamed?");
            method.Invoke(screen, null);
        }

        // ---- PRD §8.8 — the stats panel's three contracts, pinned.
        //
        // The suites already prove the panel causes no REGRESSION. These prove the panel's own
        // promises, which is a different claim: an unasserted pin is a comment (C34.1).

        private TvSweatScreen _statsScreen;

        /// <summary>The engine's closed club-noun pool — <c>SlateGenerator.Nouns</c>
        /// (engine/SlateGenerator.cs:15-21), private there and so unreachable by reference from a
        /// test; transcribed here ONCE, verbatim, cross-checked against
        /// <c>TvExtentSweep.ClubNouns</c>'s own transcription of the same pool
        /// (Assets/SBR/Editor/TvExtentSweep.cs). Shared by
        /// <see cref="Evidence_C46_the_stats_panel_strings_against_their_boxes"/> and its non-Explicit
        /// guard twin, <see cref="Stats_panel_value_column_holds_the_full_club_pool_at_max_ink_fraction"/>
        /// (S84's binding), so a pool change is one edit here, not two silently-drifting ones.</summary>
        private static readonly string[] ClosedClubPool =
        {
            "Yams", "Startups", "Bricklayers", "Longhaulers", "Mallards", "Spreadsheets",
            "Turnips", "Middlemen", "Regulators", "Plumbers", "Meatballs", "Auditors",
            "Ferrets", "Overheads", "Gravediggers", "Notaries", "Muskrats", "Zambonis",
            "Loopholes", "Refunds",
        };

        /// <summary>TMP's own unconstrained preferred-width — TvExtentSweep's own constant and
        /// rationale (Assets/SBR/Editor/TvExtentSweep.cs) — one instrument, not a second one invented
        /// here. Shared by C46 and its guard twin below.</summary>
        private const float Unconstrained = 100000f;

        /// <summary>Seats the player into a live sweat and opens the panel, asserting the
        /// precondition on the way. The panel mirrors whichever leg the scorebug is showing, so
        /// until a beat has rendered it has no leg and every assertion downstream would pass by
        /// measuring nothing — S51's shape, and exactly how this lane's first press pin went green
        /// while the key was never down.</summary>
        private IEnumerator OpenStatsPanelOnALiveLeg()
        {
            yield return LoadRoom();
            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            var screen = UnityEngine.Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = UnityEngine.Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director, "RunDirector missing");
            Assert.IsNotNull(screen, "TvSweatScreen missing");
            Assert.IsNotNull(couch, "SitSpot missing");

            screen.TimeScaleOverride = 0.0001f;
            couch.transitionDuration = 0.01f;
            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");

            Run run = director.Run;
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            director.LockRound();
            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            // Seated AND running is the state the freeze pin needs to be able to observe a change.
            yield return WaitUntil(() => screen.DebugSeatedDeltaTime > 0f, 20f,
                "the screen never became seated-and-running");
            for (int i = 0; i < 30; i++) yield return null; // let the first beat render a scorebug

            screen.ForceStatsPanel(true);
            Assert.IsNotNull(screen.DebugStatsRow(0),
                "the panel rendered no rows — it never had a leg, so nothing below is proven");
            _statsScreen = screen;
        }

        /// <summary>TIME STOPS (Allen, 2026-08-15). Asserted at the SINGLE AUTHORITY rather than on
        /// one of its consequences: every channel §8.8 enumerates — cursor, scene step, ball, actors,
        /// clock, probability, cash-out animation and OFFER, callout, resolution, transition and the
        /// pending-window timer — reads this one expression, so pinning it pins all of them. A pin on
        /// the cash-out tween alone would leave the other ten unasserted.</summary>
        [UnityTest]
        public IEnumerator Stats_panel_opening_stops_time_at_its_single_authority()
        {
            yield return OpenStatsPanelOnALiveLeg();
            TvSweatScreen screen = _statsScreen;

            screen.ForceStatsPanel(false);
            yield return null;
            Assert.Greater(screen.DebugSeatedDeltaTime, 0f,
                "PRECONDITION: seated with the panel CLOSED, the clock must be running — without "
                + "this the assertion below passes on a surface that was already frozen");

            screen.ForceStatsPanel(true);
            yield return null;
            Assert.AreEqual(0f, screen.DebugSeatedDeltaTime, 1e-6f,
                "TIME STOPS: opening the panel freezes SeatedDeltaTime, including the cash-out "
                + "OFFER — which is why the panel cannot be used to buy thinking time on a money "
                + "decision. The contradicting PRD clause was struck for this ruling.");
        }

        /// <summary>THE COLLISION IS ALLOWED, so the overlay carries the cost (Allen, 2026-08-15):
        /// the panel may open during a pending-intervention window, and the pending decision is
        /// never out of sight.</summary>
        [UnityTest]
        public IEnumerator Stats_panel_never_covers_the_intervention_overlay()
        {
            yield return OpenStatsPanelOnALiveLeg();
            TvSweatScreen screen = _statsScreen;

            var prompt = screen.DebugInterventionPrompt;
            Transform panel = screen.DebugStatsPanel;
            Assert.IsNotNull(prompt, "no InterventionPrompt element");
            Assert.IsNotNull(panel, "no StatsPanel element");
            Assert.AreSame(panel.parent, prompt.transform.parent,
                "a sibling index only orders SIBLINGS. If these ever stop sharing a parent this pin "
                + "is measuring nothing and must be re-derived, never deleted");

            // WHAT IS SHOWN IS THE PRECONDITION (§0-GC): the raise keys on the prompt being ENABLED,
            // so the test puts the RENDERED state in place rather than the engine state behind it.
            prompt.enabled = true;
            screen.ForceStatsPanel(false);
            screen.ForceStatsPanel(true);
            yield return null;

            Assert.Greater(prompt.transform.GetSiblingIndex(), panel.GetSiblingIndex(),
                "the intervention overlay must stay ON TOP of the panel — the collision is permitted "
                + "precisely because the decision he is being asked to make stays visible");
        }

        /// <summary>THE EVENT STRIP IS NOT COVERED, and the DD's question (batch 79) is answered by
        /// GEOMETRY rather than by inspection.
        ///
        /// <para>The concern is exact and it is the right one: a timed statement that FIRES and then
        /// HOLDS behind an opaque panel was never ruled, because <b>a held statement is not static
        /// even when the clock is</b> — the freeze argument that defers the scorebug does not reach
        /// it. It does not arise here. The panel spans <c>y 0 → bottomY</c>; the whole BOTTOM ROW
        /// begins at <c>bottomY</c> — <c>CashOut</c> on the left, <c>EventStrip</c> on the right —
        /// so the panel stops exactly where the strip starts.</para>
        ///
        /// <para>Asserted rather than argued, and asserted as NON-OVERLAP rather than as a pair of
        /// remembered constants, so a grid change that moved either one into the other fails here
        /// instead of on a frame.</para></summary>
        [UnityTest]
        public IEnumerator Stats_panel_covers_neither_the_event_strip_nor_the_cash_out_row()
        {
            yield return OpenStatsPanelOnALiveLeg();
            TvSweatScreen screen = _statsScreen;

            var panel = screen.DebugStatsPanel as RectTransform;
            var strip = FindChildComponent<RectTransform>(screen, "EventStripZone");
            Assert.IsNotNull(panel, "no StatsPanel element");
            Assert.IsNotNull(strip, "no EventStripZone element — re-point this pin, never delete it");

            // Top-left anchored, y running DOWN the canvas: a zone occupies [-y, -y + height].
            float panelTop = -panel.anchoredPosition.y;
            float panelBottom = panelTop + panel.rect.height;
            float stripTop = -strip.anchoredPosition.y;

            Assert.LessOrEqual(panelBottom, stripTop + 0.01f,
                $"the panel (bottom {panelBottom}) must stop at or above the event strip (top "
                + $"{stripTop}). A statement that fires and then HOLDS behind an opaque panel is not "
                + "deferred by the freeze the way a static fact is — that is the one case the "
                + "scorebug's own argument does not cover.");
        }

        /// <summary>THE SCOREBUG IS NEVER COVERED (DD batch 87, Allen): the §8.8 resize dropped the
        /// panel's top BELOW the scorebug band rather than narrowing the panel around it, so the two
        /// zones never share a pixel on EITHER axis — asserted as NON-OVERLAP against the live rects,
        /// never against remembered constants, so a grid or panel change that pushed either zone into
        /// the other fails here instead of on a frame (same approach as the event-strip pin above).
        ///
        /// <para>A half-covered scorebug is worse than a fully covered one: a partly-obscured score or
        /// clock reads as a rendering bug, where a fully covered one at least reads as "this element is
        /// elsewhere". Full 2D overlap is asserted rather than a single-axis comparison, so a future
        /// change that only narrows the vertical gap while the columns still cross horizontally is
        /// still caught as the partial-coverage failure it would be.</para></summary>
        [UnityTest]
        public IEnumerator Stats_panel_does_not_cover_the_scorebug()
        {
            yield return OpenStatsPanelOnALiveLeg();
            TvSweatScreen screen = _statsScreen;

            var panel = screen.DebugStatsPanel as RectTransform;
            var bug = FindChildComponent<RectTransform>(screen, "ScoreBugZone");
            Assert.IsNotNull(panel, "no StatsPanel element");
            Assert.IsNotNull(bug, "no ScoreBugZone element — re-point this pin, never delete it");

            // Top-left anchored, y running DOWN the canvas: a zone occupies
            // [x, x + width] x [-y, -y + height].
            var panelRect = new Rect(panel.anchoredPosition.x, -panel.anchoredPosition.y,
                panel.rect.width, panel.rect.height);
            var bugRect = new Rect(bug.anchoredPosition.x, -bug.anchoredPosition.y,
                bug.rect.width, bug.rect.height);

            Assert.IsFalse(panelRect.Overlaps(bugRect),
                $"the stats panel {panelRect} must NOT overlap the scorebug {bugRect} — a "
                + "half-covered scorebug is worse than a fully covered one (a partly-obscured score "
                + "or clock reads as a rendering bug), so ANY partial coverage is the failure this "
                + "guards, not just full coverage.");
        }

        /// <summary>§8.8 CONTENT-FIT, PINNED AS A RELATIONSHIP, NOT AS NUMBERS (DD batch 87 + Allen;
        /// register batch 79, S74-am3). The DD ruled this panel oversized — "a surface that takes
        /// the entire stage and returns three rows hasn't earned the stage" — and BuildStatsPanel
        /// was resized so `pad` is now the ONLY spacing value on the panel: left inset, both
        /// inter-column gaps, right inset, top inset and bottom inset all derive from it.
        ///
        /// <para>Two of the panel's numbers are still with the DD for ruling (<c>contentMargin</c>,
        /// and possibly the panel's placement), so a pin that hardcoded today's
        /// 172/132/236/400/564/246 would go red on a RULING rather than on a DEFECT. Register batch
        /// 79, S74-am3 records this studio's standard exactly: a RULED VALUE goes stale the moment
        /// anything near it moves; a BUILT RELATIONSHIP carries. So this pins the RELATIONSHIP those
        /// numbers are required to have, independent of what either unratified number turns out to
        /// be: SYMMETRIC INSETS on both axes. The left inset (the label column's x) must equal the
        /// right inset (panel width minus the rightmost value column's right edge); the top inset
        /// (the title's distance from the panel top) must equal the bottom inset (panel height minus
        /// the bottom row's bottom edge). That is what "one spacing value, spent on all six edges"
        /// means geometrically, and it is measured off the LIVE BUILT RECTS — never a copied
        /// constant — so it survives whatever the DD rules on <c>contentMargin</c> and still catches
        /// the one thing that actually matters: space the content did not ask for.</para>
        ///
        /// <para>Row/column population is discovered live off the panel's own children too, never
        /// assumed from a copied row count, so an added or removed row cannot leave this pin quietly
        /// checking the wrong one.</para></summary>
        [UnityTest]
        public IEnumerator Stats_panel_is_sized_exactly_to_its_content()
        {
            yield return OpenStatsPanelOnALiveLeg();
            TvSweatScreen screen = _statsScreen;

            var panel = screen.DebugStatsPanel as RectTransform;
            Assert.IsNotNull(panel, "no StatsPanel element");
            var title = FindChildComponent<RectTransform>(screen, "StatsTitle");
            Assert.IsNotNull(title, "no StatsTitle element — re-point this pin, never delete it");

            // Discovered LIVE off the panel's own children, never off a copied row count — the
            // bottom row and rightmost column below are whichever slots actually exist.
            var labelSlots = new List<RectTransform>();
            var aSlots = new List<RectTransform>();
            var bSlots = new List<RectTransform>();
            foreach (RectTransform rt in panel.GetComponentsInChildren<RectTransform>(true))
            {
                if (rt.name.StartsWith("StatsLabel")) labelSlots.Add(rt);
                else if (rt.name.StartsWith("StatsA")) aSlots.Add(rt);
                else if (rt.name.StartsWith("StatsB")) bSlots.Add(rt);
            }
            Assert.IsTrue(labelSlots.Count > 0 && aSlots.Count > 0 && bSlots.Count > 0,
                "no StatsLabel{i}/StatsA{i}/StatsB{i} slots found on the live panel — "
                + "BuildStatsPanel's naming moved, re-point this pin");

            // Top-left anchored, y running DOWN the canvas (same convention as the scorebug/event
            // strip pins above): a slot's own right edge is anchoredPosition.x + width, and its own
            // bottom edge is -anchoredPosition.y + height.
            float leftInset = labelSlots[0].anchoredPosition.x; // the label column's x

            float rightmostValueEdge = float.MinValue;
            foreach (RectTransform rt in aSlots)
                rightmostValueEdge = Mathf.Max(rightmostValueEdge, rt.anchoredPosition.x + rt.rect.width);
            foreach (RectTransform rt in bSlots)
                rightmostValueEdge = Mathf.Max(rightmostValueEdge, rt.anchoredPosition.x + rt.rect.width);
            float rightInset = panel.rect.width - rightmostValueEdge;

            float topInset = -title.anchoredPosition.y; // the title's distance from the panel top

            float bottomRowBottom = float.MinValue;
            foreach (RectTransform rt in labelSlots)
                bottomRowBottom = Mathf.Max(bottomRowBottom, -rt.anchoredPosition.y + rt.rect.height);
            foreach (RectTransform rt in aSlots)
                bottomRowBottom = Mathf.Max(bottomRowBottom, -rt.anchoredPosition.y + rt.rect.height);
            foreach (RectTransform rt in bSlots)
                bottomRowBottom = Mathf.Max(bottomRowBottom, -rt.anchoredPosition.y + rt.rect.height);
            float bottomInset = panel.rect.height - bottomRowBottom;

            const float tol = 0.5f;
            Assert.AreEqual(leftInset, rightInset, tol,
                $"HORIZONTAL asymmetry: left inset {leftInset:0.0}px, right inset {rightInset:0.0}px "
                + "— the panel is carrying horizontal space its content did not ask for.");
            Assert.AreEqual(topInset, bottomInset, tol,
                $"VERTICAL asymmetry: top inset {topInset:0.0}px, bottom inset {bottomInset:0.0}px "
                + "— the panel is carrying vertical space its content did not ask for.");
        }

        /// <summary>THE UNREVEALED MARK. §8.8: a stat not causally revealed is absent or shown as the
        /// mark, NEVER as its true final value — "a leak here is a blocker, not a polish item". The
        /// row still prints, so the gap is VISIBLE rather than hidden (Allen, 2026-08-15).</summary>
        [UnityTest]
        public IEnumerator Stats_panel_marks_corners_and_cards_unrevealed_off_a_count_leg()
        {
            yield return OpenStatsPanelOnALiveLeg();
            TvSweatScreen screen = _statsScreen;
            string mark = screen.DebugStatsUnrevealedMark;

            // DemoTicketPolicy picks MONEYLINE only, so no count ledger exists on this leg and
            // neither count has been revealed at all.
            Assert.AreEqual($"CORNERS|{mark}|{mark}", screen.DebugStatsRow(1),
                "corners are unrevealed off a corners leg, and the row still prints");
            Assert.AreEqual($"CARDS|{mark}|{mark}", screen.DebugStatsRow(2),
                "cards are unrevealed off a cards leg, and the row still prints");

            // NON-VACUITY: a build that marked EVERY row would satisfy both assertions above. The
            // goals row IS revealed and must carry figures, or this test proves only that the panel
            // says nothing.
            string goals = screen.DebugStatsRow(0);
            StringAssert.StartsWith("GOALS|", goals);
            StringAssert.DoesNotContain(mark, goals,
                "the GOALS row is revealed-ledger data and must never carry the unrevealed mark — "
                + "if it does, the panel is marking everything and the two assertions above are "
                + "passing on silence");
        }

        /// <summary>C46: THE PANEL'S OWN STRINGS AGAINST THEIR OWN BOXES (T101, register batch 85).
        ///
        /// <para>Every fixed box in <c>BuildStatsPanel</c> carries an unstated assumption that its
        /// content fits. This measures every string the panel can be asked to render against the box
        /// it renders in — <b>and offers no fit verdict</b>: C46 is a measurement lane, not a
        /// judgement. The only assertion here is that every panel text slot got measured (COVERAGE,
        /// below), never that any string fits its box.</para>
        ///
        /// <para><b>Population, enumerated from source, not invented.</b> The title and the three row
        /// labels are the constants <c>RenderStatsPanel</c> assigns (TvSweatScreen.cs:3801, 3819,
        /// 3824, 3827). Team headers are every club in the engine's closed pool (the shared
        /// <see cref="ClosedClubPool"/> field above — <c>SlateGenerator.Nouns</c>,
        /// engine/SlateGenerator.cs:15-21, private there and so unreachable by reference from a test)
        /// through <c>SweatFlavor.Short</c>. Values are every digit form the LIVE RUN's own config can
        /// realize per row, plus the unrevealed mark where the source can actually produce it — see
        /// the DIGIT-COUNT ASSUMPTION comment below for the citation.</para>
        ///
        /// <para><b>Face borrowed from the RENDERED components</b>, off the live, seated panel opened
        /// by <see cref="OpenStatsPanelOnALiveLeg"/> — never a lookalike node. No TV-surface
        /// <c>MeasureWidth</c> helper exists in Runtime (only the laptop has one, <c>LaptopUi</c>), so
        /// this uses TMP's own preferred-width on the real component
        /// (<c>TMP_Text.GetPreferredValues</c>) — the same call <c>TvExtentSweep</c>, this repo's own
        /// sweep instrument, already uses for exactly this question, at the same unconstrained width
        /// for the same reason (a wrapping query at 0 width measures the widest GLYPH, not the widest
        /// STRING; these components build with <c>enableWordWrapping = false</c> so it cannot bite
        /// here, but the constant is kept so this stays one instrument rather than two).</para></summary>
        [UnityTest]
        [Explicit("C46 evidence for the DD: §8.8 stats panel string widths against their own boxes — "
            + "closed club pool, row labels/title, goals/corners/cards value range. Measures only, "
            + "asserts no fit. Run by filter only.")]
        public IEnumerator Evidence_C46_the_stats_panel_strings_against_their_boxes()
        {
            yield return OpenStatsPanelOnALiveLeg();
            TvSweatScreen screen = _statsScreen;

            var measuredSlotNames = new HashSet<string>();
            int measured = 0;

            float Log(string slot, TMP_Text t, string s)
            {
                float box = t.rectTransform.rect.width;
                float w = t.GetPreferredValues(s, Unconstrained, 0f).x;
                UnityEngine.Debug.Log($"[C46-PANEL] {slot,-12} \"{s}\" box {box:0.0}px measured "
                    + $"{w:0.0}px clearance {box - w:0.0}px");
                measuredSlotNames.Add(slot);
                measured++;
                return w;
            }

            // ---- Resolve every rendered component off the LIVE PANEL, by name — borrows font, size
            // and tracking from the real thing (MakeText wires them once, per component), never from
            // a lookalike node. Doubles as the coverage population below.
            var panelSlots = new Dictionary<string, TMP_Text>();
            foreach (TMP_Text t in screen.DebugStatsPanel.GetComponentsInChildren<TMP_Text>(true))
                panelSlots[t.gameObject.name] = t;

            TMP_Text Slot(string name)
            {
                Assert.IsTrue(panelSlots.TryGetValue(name, out TMP_Text t),
                    $"{name} not found on the live stats panel — BuildStatsPanel's construction moved");
                return t;
            }

            TMP_Text title = Slot("StatsTitle");
            TMP_Text teamA = Slot("StatsTeamA");
            TMP_Text teamB = Slot("StatsTeamB");
            var labels = new[] { Slot("StatsLabel0"), Slot("StatsLabel1"), Slot("StatsLabel2") };
            var aSlots = new[] { Slot("StatsA0"), Slot("StatsA1"), Slot("StatsA2") };
            var bSlots = new[] { Slot("StatsB0"), Slot("StatsB1"), Slot("StatsB2") };

            // ---- (1) TITLE — the one constant (TvSweatScreen.cs:3801).
            Log("StatsTitle", title, "MATCH STATS");

            // ---- (2) ROW LABELS — the three constants (TvSweatScreen.cs:3819, 3824, 3827).
            string[] rowLabels = { "GOALS", "CORNERS", "CARDS" };
            for (int i = 0; i < rowLabels.Length; i++)
                Log($"StatsLabel{i}", labels[i], rowLabels[i]);

            // ---- (3) TEAM HEADERS — every club in the engine's closed pool (the shared
            // ClosedClubPool field above), both columns (StatsTeamA and StatsTeamB are two DIFFERENT
            // rendered components; measured separately rather than assumed symmetric).
            float worstClubW = float.MinValue; string worstClub = "";
            foreach (string noun in ClosedClubPool)
            {
                // SlateGenerator.MakeTeam builds `"{city} {noun}"`; Short returns the substring after
                // the LAST space, so the city prefix is inert — any placeholder city reproduces
                // exactly what the live surface would render for that noun.
                string shortName = SweatFlavor.Short($"City {noun}");
                float wa = Log("StatsTeamA", teamA, shortName);
                float wb = Log("StatsTeamB", teamB, shortName);
                if (wa > worstClubW) { worstClubW = wa; worstClub = shortName; }
                if (wb > worstClubW) { worstClubW = wb; worstClub = shortName; }
            }
            UnityEngine.Debug.Log(
                $"[C46-PANEL] WIDEST CLUB SHORT-NAME \"{worstClub}\" {worstClubW:0.0}px");

            // ---- (4) VALUES — digits and the mark.
            //
            // DIGIT-COUNT ASSUMPTION, read off the LIVE RUN's own config rather than a copied
            // constant. `MatchModel.SampleStatLine` draws goals from `HomeWinScores`/`AwayWinScores`/
            // `DrawScores`, built by loops bounded at `config.MaxGoalsGrid` per side (MatchModel.cs:
            // 197-198, 716-717); corners/cards are drawn by `SampleFromRaw` over `RawPoisson` arrays
            // sized `[0, config.MaxCornerGrid]` / `[0, config.MaxCardGrid]` (MatchModel.cs ~51-54,
            // 779). These are HARD CLAMPS on the REALIZED simulation output, not a pricing-only grid,
            // so a side's goals/corners/cards can never exceed its own Max*Grid. The three rows share
            // ONE box/type class (StatsA{i}/StatsB{i} — valueW x 34, TypeProgress, no tracking), so
            // every value from 0 to that row's own max is measured — never assuming the ceiling number
            // renders widest, the same "do not pick a champion" principle the team-header population
            // uses; a proportional font need not put its widest glyphs in its largest number.
            Run liveRun = UnityEngine.Object.FindAnyObjectByType<RunDirector>().Run;
            int maxGoals = liveRun.Config.MaxGoalsGrid;
            int maxCorners = liveRun.Config.MaxCornerGrid;
            int maxCards = liveRun.Config.MaxCardGrid;
            string mark = screen.DebugStatsUnrevealedMark;

            void SweepValueSlot(string slotName, TMP_Text t, int max, bool markReachable)
            {
                float worst = float.MinValue; string worstS = "";
                for (int v = 0; v <= max; v++)
                {
                    string s = v.ToString();
                    float w = Log(slotName, t, s);
                    if (w > worst) { worst = w; worstS = s; }
                }
                if (markReachable)
                {
                    float w = Log(slotName, t, mark);
                    if (w > worst) { worst = w; worstS = mark; }
                }
                UnityEngine.Debug.Log($"[C46-PANEL] WIDEST {slotName} \"{worstS}\" {worst:0.0}px");
            }

            // GOALS (row 0) never shows the mark — RenderStatsPanel sets it unconditionally:
            // `SetStatsRow(0, "GOALS", goalsAway.ToString(), goalsHome.ToString())` (TvSweatScreen.cs
            // :3819 — T102/S84's column swap put AWAY in the "a" slot and HOME in "b"). CORNERS/CARDS
            // are conditional on the live leg's market (:3821-3829), so both forms are reachable there
            // and both are measured.
            SweepValueSlot("StatsA0", aSlots[0], maxGoals, markReachable: false);
            SweepValueSlot("StatsB0", bSlots[0], maxGoals, markReachable: false);
            SweepValueSlot("StatsA1", aSlots[1], maxCorners, markReachable: true);
            SweepValueSlot("StatsB1", bSlots[1], maxCorners, markReachable: true);
            SweepValueSlot("StatsA2", aSlots[2], maxCards, markReachable: true);
            SweepValueSlot("StatsB2", bSlots[2], maxCards, markReachable: true);

            // ---- COVERAGE, derived rather than asserted as prose (T89-B's shape, TvExtentSweep):
            // every text slot that actually exists on the live panel, diffed against every slot this
            // sweep touched. UNACCOUNTED-FOR must be zero, or a slot went unmeasured rather than
            // dropped silently.
            var uncovered = new List<string>();
            foreach (string name in panelSlots.Keys)
                if (!measuredSlotNames.Contains(name)) uncovered.Add(name);
            uncovered.Sort();

            UnityEngine.Debug.Log($"[C46-PANEL] COVERAGE: {measured} strings measured across "
                + $"{measuredSlotNames.Count} of {panelSlots.Count} panel text slots · "
                + $"UNACCOUNTED-FOR: {uncovered.Count}"
                + (uncovered.Count > 0 ? $" ({string.Join(", ", uncovered)})" : ""));

            Assert.AreEqual(0, uncovered.Count,
                "panel text slot(s) exist that this sweep never measured: "
                + string.Join(", ", uncovered));
        }

        /// <summary>T102/S84's GUARD (DD batch 89): the value column must be sized against THE
        /// ENUMERATED CLOSED POOL, not a sampled widest — and this has to stay true as the pool
        /// changes. C46 above is the measurement lane and offers no fit verdict, and it is
        /// <c>[Explicit]</c> so it never runs in a routine suite — exactly why a 21st club could
        /// overflow the box silently. THIS is the verdict, and it is deliberately NOT
        /// <c>[Explicit]</c>: it has to gate.
        ///
        /// <para>Measured off the LIVE rendered team-header components — StatsTeamA/StatsTeamB are
        /// the only slots a club short-name ever actually renders into
        /// (<c>RenderStatsPanel</c>, TvSweatScreen.cs:3810-3811) — borrowing their real font, size and
        /// tracking, never a lookalike node. Same instrument C46 uses:
        /// <c>TMP_Text.GetPreferredValues</c> at the shared <see cref="Unconstrained"/> width. The
        /// ratio itself is read live off <see cref="TvSweatScreen.DebugStatsMaxInkFraction"/> rather
        /// than a second, driftable "0.8" literal here.</para>
        ///
        /// <para><b>If this fires</b>, the pool grew a short name that no longer fits at
        /// <c>MaxInkFraction</c>. The fix is to RE-DERIVE labelW/valueW in <c>BuildStatsPanel</c> from
        /// the new widest under the same 80% rule — never to shorten the string to fit the old
        /// box.</para></summary>
        [UnityTest]
        public IEnumerator Stats_panel_value_column_holds_the_full_club_pool_at_max_ink_fraction()
        {
            yield return OpenStatsPanelOnALiveLeg();
            TvSweatScreen screen = _statsScreen;

            TMP_Text teamA = FindChildComponent<TMP_Text>(screen, "StatsTeamA");
            TMP_Text teamB = FindChildComponent<TMP_Text>(screen, "StatsTeamB");
            Assert.IsNotNull(teamA,
                "StatsTeamA not found on the live stats panel — BuildStatsPanel's naming moved");
            Assert.IsNotNull(teamB,
                "StatsTeamB not found on the live stats panel — BuildStatsPanel's naming moved");

            // Both columns are built to the same valueW (BuildStatsPanel) but measured independently
            // rather than assumed identical — the same caution C46 takes with these two slots.
            float boxA = teamA.rectTransform.rect.width;
            float boxB = teamB.rectTransform.rect.width;
            Assert.AreEqual(boxA, boxB, 0.01f,
                "StatsTeamA/StatsTeamB boxes disagree — BuildStatsPanel no longer builds one shared "
                + "valueW for both columns");

            float maxInkFraction = screen.DebugStatsMaxInkFraction;
            float worst = float.MinValue; string worstClub = "";
            foreach (string noun in ClosedClubPool)
            {
                // SlateGenerator.MakeTeam builds `"{city} {noun}"`; Short returns the substring after
                // the LAST space, so the city prefix is inert — any placeholder city reproduces
                // exactly what the live surface would render for that noun (same reasoning C46 uses).
                string shortName = SweatFlavor.Short($"City {noun}");
                float wa = teamA.GetPreferredValues(shortName, Unconstrained, 0f).x;
                float wb = teamB.GetPreferredValues(shortName, Unconstrained, 0f).x;
                if (wa > worst) { worst = wa; worstClub = shortName; }
                if (wb > worst) { worst = wb; worstClub = shortName; }
            }

            float limit = boxA * maxInkFraction;
            UnityEngine.Debug.Log($"[T102-GUARD] pool {ClosedClubPool.Length} clubs · widest "
                + $"\"{worstClub}\" {worst:0.0}px · box {boxA:0.0}px · {maxInkFraction:0.00} limit "
                + $"{limit:0.0}px");

            Assert.LessOrEqual(worst, limit,
                $"\"{worstClub}\" measures {worst:0.0}px against a {boxA:0.0}px box — over the "
                + $"{maxInkFraction:0.00} max-ink-fraction limit of {limit:0.0}px. The club pool grew "
                + "past the box: RE-DERIVE labelW/valueW in BuildStatsPanel from this new widest under "
                + "MaxInkFraction — never shorten the string to fit.");
        }

        private static T FindChildComponent<T>(TvSweatScreen screen, string childName) where T : Component
        {
            foreach (T c in screen.GetComponentsInChildren<T>(true))
                if (c.name == childName) return c;
            return null;
        }

        private static int UnusedMatchup(Run run, IReadOnlyList<Pick> used)
        {
            for (int i = 0; i < run.CurrentSlate.Matchups.Count; i++)
            {
                bool taken = false;
                foreach (Pick p in used)
                    if (p.MatchupIndex == i) { taken = true; break; }
                if (!taken) return i;
            }
            throw new InvalidOperationException("no unused matchup on the slate");
        }

        private static IEnumerator LoadRoom()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("Room", LoadSceneMode.Single);
            Assert.IsNotNull(load, "Room scene not in build settings - run SBR.GrayboxRoomBuilder.Build first.");
            while (!load.isDone) yield return null;
        }

        // Waits are wall-clock, not frame-count: batch mode runs unthrottled (thousands of fps),
        // so frame budgets starve anything driven by Time.deltaTime (e.g. the couch camera lerp).
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
