using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>`T130`'s gate, and the drawn-ending split's §1.2: <b>A RENDERED LEG ROW IS NEVER
        /// EMPTY.</b> The spec's own words are that it <i>"would have caught arm 3 before it
        /// shot"</i> — an arm was captured with a blank column and nothing asserted otherwise.
        ///
        /// <para><b>The assertion is per ROW, never per SPAN.</b> A live row deliberately blanks its
        /// compact line; a cancelled row on a settled ticket deliberately blanks its state chip
        /// (`T149`). Emptiness of a SPAN is normal and correct. Emptiness of the WHOLE ROW — no
        /// statement, no NEED, no progress — is the defect, because the player is looking at a leg
        /// of his ticket that says nothing about itself.</para>
        ///
        /// <para><b>It asserts nothing about WHICH leg is live</b>, deliberately. The engine's
        /// per-(ticket, fixture) restructure makes `DramaEvent.LegIndex` the telling's ANCHOR leg and
        /// puts several legs live at once; a gate phrased around the live leg would fail on a legal
        /// arm, and a gate that fails on a legal arm is not a gate. Phrased per row, this reads
        /// identically before and after that change.</para>
        ///
        /// <para><b>THE GUARD MATTERS AS MUCH AS THE ASSERTION.</b> `ClearLegRow` blanks every span,
        /// and `UpdateTicketColumn` calls it for every row when `_ticket` is null — so a genuinely
        /// blank column is CORRECT whenever no ticket is being rendered, and an unguarded
        /// every-frame assertion would fail on those frames rather than on a defect. The footer is
        /// non-empty exactly when a ticket is rendered, so it is the proxy this gate scopes
        /// itself by.</para></summary>
        [UnityTest]
        public IEnumerator T130_a_rendered_leg_row_is_never_empty()
        {
            yield return LoadRoom();
            (RunDirector director, TvSweatScreen screen, SitSpot couch) = FindTrio();

            screen.TimeScaleOverride = 0.0001f;
            couch.transitionDuration = 0.01f;

            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");
            Run run = director.Run;
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            director.LockRound();
            int legCount = picks.Count;

            // Which kinds this run actually exercised. Logged rather than asserted: a gate that
            // never met a given market has not been proven against it, and saying so plainly is
            // better than a green that reads as coverage it does not have.
            var kinds = new List<string>();
            for (int i = 0; i < director.CurrentTicket.Legs.Count; i++)
                kinds.Add(director.CurrentTicket.Legs[i].Selection.Kind.ToString());

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            int framesSampled = 0, framesAsserted = 0;
            float start = Time.realtimeSinceStartup;
            const float maxSeconds = 60f;
            while (run.Phase == Phase.Sweat)
            {
                if (Time.realtimeSinceStartup - start > maxSeconds)
                {
                    Assert.Fail($"the sweat never settled within {maxSeconds}s wall-clock "
                        + $"(frames sampled: {framesSampled})");
                    yield break;
                }
                framesSampled++;

                // See the summary: a blank column is correct when no ticket is rendered.
                if (!string.IsNullOrEmpty(screen.DebugTicketRiskText))
                {
                    framesAsserted++;
                    for (int i = 0; i < legCount; i++)
                    {
                        string line = LegRowLineText(screen, i);
                        string need = screen.DebugLegNeed(i);
                        string progress = screen.DebugLegProgress(i);
                        bool carriesSomething = !string.IsNullOrEmpty(line)
                                             || !string.IsNullOrEmpty(need)
                                             || !string.IsNullOrEmpty(progress);
                        Assert.IsTrue(carriesSomething,
                            $"frame {framesAsserted}, leg {i} of {legCount}: the row carries NO text in "
                            + "any form — compact line, NEED and progress are all empty on a ticket that "
                            + $"IS being rendered (footer reads '{screen.DebugTicketRiskText}'). A leg of "
                            + "his ticket is saying nothing about itself. Leg kinds this run: "
                            + $"{string.Join(", ", kinds)}");
                    }
                }
                yield return null;
            }

            UnityEngine.Debug.Log($"[T130] legs={legCount} kinds=[{string.Join(",", kinds)}] "
                + $"framesSampled={framesSampled} framesAsserted={framesAsserted}");

            // A gate that never ran is not a passing gate. C29's shape, one layer down.
            Assert.Greater(framesAsserted, 0,
                "the gate never asserted on a single frame — no frame rendered a ticket, so this "
                + "result proves nothing about whether a rendered row can be empty.");
        }

        /// <summary>The compact statement's text, by GameObject name. There is no Debug accessor for
        /// it and this adds none to production for a test's convenience — the same lookup the
        /// capture harness uses.</summary>
        private static string LegRowLineText(TvSweatScreen screen, int i)
        {
            foreach (TMP_Text t in screen.GetComponentsInChildren<TMP_Text>(true))
                if (t.gameObject.name == $"LegRowLine{i}") return t.text;
            return null;
        }

        // ---- the footer word (RISK/STAKE) must never disagree with what the rows show, and no live
        // row's progress may ever print NEED 0 — watched on a REAL sweat rather than through the pure
        // model directly. SweatActiveLegModelTests already proves both predicates exhaustively off
        // pure inputs (item 7 "NEED 0 is unconstructible", item 10 "TicketCannotLose is true iff every
        // leg is won or voided"); this proves the WIRING — the footer text and each row's own
        // chip/progress text are painted by two different call sites in TvSweatScreen and could drift
        // apart even with a correct model underneath.

        /// <summary>Polls EVERY FRAME across a real sweat — a pin that samples one frame proves
        /// nothing, since both defects this guards are about a MOMENT the two surfaces disagree, not a
        /// steady state. Reads exactly what the player is looking at
        /// (<see cref="TvSweatScreen.DebugTicketRiskText"/>, <c>DebugLegProgress</c>,
        /// <c>DebugLegState</c>), never a re-derivation of it.
        ///
        /// <para><c>DemoTicketPolicy</c> derives a ticket's leg count as
        /// <c>2 + hash(seed#round) % 2</c> — see the note on <see cref="PinnedSweatSeed"/> above —
        /// which is always 2 or 3, never 1, so an un-pinned run already gives a multi-leg ticket. If
        /// that ever stops holding and a single-leg ticket is drawn, the NEXT-chip half of the second
        /// assertion below simply never fires (there is no pending row to show it) — logged plainly
        /// rather than faked or failed.</para></summary>
        [UnityTest]
        public IEnumerator TicketFooterWord_NeverDisagreesWithAnyRow_AndNoLiveRowEverPrintsNeedZero()
        {
            yield return LoadRoom();
            (RunDirector director, TvSweatScreen screen, SitSpot couch) = FindTrio();

            // Fast-forward, same as the other full-sweat tests (FullRound_TwoTickets,
            // SeatOnAMultiCountTicket): batch mode still renders at thousands of fps regardless of
            // TimeScaleOverride (see WaitUntil's own note below), so "every frame" here still means
            // many hundreds of samples over a wall-clock window of a few seconds.
            screen.TimeScaleOverride = 0.0001f;
            couch.transitionDuration = 0.01f;

            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");
            Run run = director.Run;
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            director.LockRound();
            int legCount = picks.Count;

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            int framesSampled = 0;
            var footerWords = new HashSet<string>();
            var progressStrings = new HashSet<string>();
            bool sawNextChip = false;
            bool sawDecidedLeg = false; // any row reached W/L/VOID, or the footer ever read STAKE

            float start = Time.realtimeSinceStartup;
            const float maxSeconds = 60f;
            while (run.Phase == Phase.Sweat)
            {
                if (Time.realtimeSinceStartup - start > maxSeconds)
                {
                    Assert.Fail($"the sweat never settled within {maxSeconds}s wall-clock "
                        + $"(frames sampled so far: {framesSampled})");
                    yield break;
                }

                framesSampled++;
                string footer = screen.DebugTicketRiskText; // e.g. "RISK $35.00" / "STAKE $35.00"
                string footerWord = footer.Length > 0 ? footer.Split(' ')[0] : string.Empty;
                if (footerWord.Length > 0) footerWords.Add(footerWord);
                if (footerWord == "STAKE") sawDecidedLeg = true;
                // Per-FRAME, unlike sawDecidedLeg above which is per-RUN: assertion 3 below is
                // about this frame's surface, not about anything the run reached earlier.
                bool anyDecidedChipThisFrame = false;

                for (int i = 0; i < legCount; i++)
                {
                    string progress = screen.DebugLegProgress(i);
                    string chip = screen.DebugLegState(i);

                    // 1. THE CAPTURED DEFECT: a live row's progress must never read NEED 0.
                    if (!string.IsNullOrEmpty(progress))
                    {
                        progressStrings.Add(progress);
                        Assert.IsFalse(progress.Contains("NEED 0"),
                            $"frame {framesSampled}, leg {i}: progress '{progress}' contains NEED 0 — "
                            + "the exact state-lie this dispatch fixes");
                    }

                    if (chip == "NEXT")
                    {
                        sawNextChip = true;
                        // 2, RISK half — the trap named in the brief: a leg still to come must not
                        // have let the footer say STAKE. RISK is a TICKET word.
                        Assert.AreEqual("RISK", footerWord,
                            $"frame {framesSampled}: leg {i} shows the NEXT chip but the footer reads "
                            + $"'{footerWord}' — a leg still to come must not flip RISK to STAKE");
                    }
                    else if (chip == "W" || chip == "L" || chip == "VOID")
                    {
                        sawDecidedLeg = true;
                        anyDecidedChipThisFrame = true;
                    }

                    // 2, STAKE half — a decided ticket must not still be showing a live requirement
                    // anywhere. (The NEXT-chip half of this same claim is asserted above.)
                    if (footerWord == "STAKE" && !string.IsNullOrEmpty(progress))
                    {
                        bool namesALiveRequirement = progress.Contains("NEED")
                            || progress.Contains("MORE") || progress.Contains("LIMIT");
                        Assert.IsFalse(namesALiveRequirement,
                            $"frame {framesSampled}: footer reads STAKE but leg {i} progress "
                            + $"'{progress}' still names a live requirement — STAKE claims the whole "
                            + "ticket is decided");
                    }
                }

                // 3. THE REVEAL GATE. A footer reading STAKE has named the ticket SETTLED; on a
                // dead ticket the surface may only say that once a row has SHOWN the loss.
                // `SweatSession.MoveNext` resolves a LegFinal and busts BEFORE it hands the event
                // back (SweatSession.cs:150-154, :184-185), while `_presentedResolved` is marked only
                // in FinalSlam, after the whole final scene has played — so a footer reading raw
                // `_ticket.State` prints the ending during the scene that delivers it.
                //
                // ASSERTION 2 ABOVE CANNOT CATCH THAT, and the distinction is the reason this
                // exists: assertion 2 compares the footer against the ROWS, so a change that
                // settles the rows early makes both agree while both are still early — green, and
                // still telling the ending. This one compares the footer against the REVEAL,
                // which is the thing actually being raced.
                //
                // Cash-out is exempt and must be: it is a PLAYER ACTION with no hidden outcome
                // behind it, it settles synchronously at the moment he acts, and no row shows a
                // decided chip for it — there is nothing for a reveal to be ahead of.
                if (footerWord == "STAKE"
                    && director.CurrentTicket != null
                    && director.CurrentTicket.State != TicketState.CashedOut
                    && !anyDecidedChipThisFrame)
                {
                    Assert.Fail($"frame {framesSampled}: the footer reads STAKE on a ticket that "
                        + "is not cashed out, but NO row shows a decided chip (W/L/VOID) — the "
                        + "ending is being told before the theater shows it");
                }

                yield return null;
            }

            UnityEngine.Debug.Log(
                $"[TICKET-WORD] legs={legCount} frames={framesSampled} "
                + $"footerWordCount={footerWords.Count} footerWords=[{string.Join(",", footerWords)}] "
                + $"distinctProgressStrings={progressStrings.Count} "
                + $"sawNextChip={sawNextChip} sawDecidedLeg={sawDecidedLeg}");

            // LEFT AS A LOG, deliberately — do not promote this to an assertion. This pin walks an
            // UN-SEEDED demo run (DemoTicketPolicy's natural draw, not a pinned seed), and the final
            // leg's grade can land AFTER this while loop's own exit condition (run.Phase no longer
            // Sweat) already stopped sampling. Asserting on sawDecidedLeg here would be exactly the
            // load-dependent flake this suite has been bitten by before (SeatOnAMultiCountTicket's
            // own comment above documents the identical shape: an easy draw clears a wall-clock
            // budget an unlucky one misses under full-suite load). The deterministic half of this
            // duty is carried instead by
            // TicketFooterWord_LegOneWon_RiskWhileLegTwoLive_StakeWhenLegTwoWonEarly below, on a
            // pinned seed — a structural guarantee, not a wall-clock gamble — which is why THIS
            // branch is allowed to stay a log rather than a gate.
            if (!sawDecidedLeg)
                UnityEngine.Debug.Log("[TICKET-WORD] the run never reached a decided leg within the "
                    + "sampling window — the STAKE half of assertion 2 was never exercised");

            // HARD GATE, not a log: DemoTicketPolicy derives a ticket's leg count as
            // 2 + hash(seed#round) % 2 (see PinnedSweatSeed's own note above), which is always 2 or
            // 3 — NEVER fewer than 2. A ticket with under 2 legs here means that policy itself broke,
            // not an unlucky draw, so this is asserted rather than logged: a log would hide a broken
            // policy behind a still-green test, the exact C29 shape this dispatch closes elsewhere.
            Assert.GreaterOrEqual(legCount, 2,
                $"ticket had only {legCount} leg(s) — DemoTicketPolicy's own arity floor is broken, "
                + "not an unlucky draw; the NEXT-chip half of assertion 2 could never fire");
        }

        // ---- DD ruling-t108-trigger-2026-08-17.md §5 — THE TWO NAMED STATES, exercised by
        // construction. The pin above proves the BROADER claim (any decided leg forces RISK while
        // any other leg is undecided) on SeatOnAMultiCountTicket's own seed. The ruling wants
        // something more precise the broader claim cannot certify: state 1 needs leg 0 SPECIFICALLY
        // Won (not merely "decided" — W/L/VOID conflated is not enough), and there is a SECOND state
        // — leg 1 already won on its own REVEALED COUNT while still the live row, ahead of its own
        // whistle — that the broader pin cannot even distinguish, because leg 1's chip reads
        // identically (blank/live) in both states. Only the footer word tells them apart, which is
        // exactly ruling §4's "decided, but not yet resolved" third state: the model has graded the
        // leg, the chip has not caught up, and the footer is the one surface reading the model's
        // grade rather than the whistle.
        //
        // No force-hook exists to drive the ledger to that second state on demand, and adding one to
        // production is out of scope for this dispatch. So the construction is a SEED CHOICE:
        // "measure, then pin" — this lane's own precedent, the exact method that chose
        // SeatOnAMultiCountTicket's own STATS-MULTI-1 (see that helper's comment above). Deliverable
        // A below is the measurement; Deliverable B is the pin, seed left as a named placeholder
        // pending A's result.

        /// <summary>Shared by the seed search (Deliverable A) and the pinned gate (Deliverable B):
        /// the LOWEST-line OVER offer of <paramref name="kind"/>, on any matchup other than
        /// <paramref name="excludeMatchup"/> (-1 excludes nothing).
        ///
        /// <para>OVER-ONLY, by ruling (DD correction, 2026-08-17): state 2 needs a leg won on the
        /// REVEALED COUNT before its own whistle, and only an OVER leg can be revealed-Won early —
        /// the count crosses the threshold and the leg can no longer lose. AN UNDER LEG HAS NO EARLY
        /// WON; its only pre-whistle verdict is Lost (the count busts its allowance), so an UNDER
        /// fixture cannot certify state 2, no matter which seed carries it.</para>
        ///
        /// <para>LOWEST LINE among the OVERs offered, deliberately: the lower the line, the more
        /// match time remains after it clears, so the lowest-line OVER is the one most likely to be
        /// revealed-Won while its leg is still live — a bias toward REACHING the state being
        /// searched for, not toward a fitted outcome. The assertions this feeds are unchanged either
        /// way; this only affects which slate lines get a chance to demonstrate them.</para>
        ///
        /// <para>Returns false if the slate offers no such selection — "re-seed, never invent a
        /// selection the board did not offer" (<see cref="SeatOnAMultiCountTicket"/>'s own
        /// discipline) holds for a seed-searched slate exactly as it does for a pinned one.</para>
        /// </summary>
        private static bool TryFindLowestLineOver(Run run, MarketKind kind, int excludeMatchup,
            out int matchupIndex, out MarketSelection selection)
        {
            matchupIndex = -1;
            selection = default;
            double bestLine = double.MaxValue;
            foreach (Matchup mm in run.CurrentSlate.Matchups)
            {
                if (mm.Index == excludeMatchup) continue; // DIFFERENT matchup - ordinary pricing path
                foreach (MarketOffer off in mm.Markets)
                {
                    if (off.Selection.Kind != kind) continue;
                    if (off.Selection.Choice != MarketChoice.Over) continue;
                    if (off.Selection.Line >= bestLine) continue;
                    bestLine = off.Selection.Line;
                    matchupIndex = mm.Index;
                    selection = off.Selection;
                }
            }
            return matchupIndex >= 0;
        }

        /// <summary>Deliverable A's candidate pool. A plain static field, not a local, so it is
        /// trivially editable without touching the search logic below. Includes
        /// <c>STATS-MULTI-1</c> (<see cref="SeatOnAMultiCountTicket"/>'s own seed — already known to
        /// offer BOTH a TotalCorners and a TotalCards market on different matchups, though NOT
        /// confirmed OVER specifically until this search checks it) and <c>48151623</c> (already a
        /// stable pinned seed elsewhere in this suite family —
        /// <c>TvSweatCaptureHarness.Capture_Batch22_StatementFit_And_PayoffBeats</c>); the rest are
        /// plausible A-Z0-9 draws for breadth. No claim any of them reach either named state — that
        /// is what the search below measures.</summary>
        private static readonly string[] TrapSeedCandidates =
        {
            "STATS-MULTI-1", "48151623", "STATS-MULTI-2", "STATS-MULTI-3", "STATS-MULTI-4",
            "STATS-MULTI-5", "STATS-MULTI-6", "TRAP-1", "TRAP-2", "TRAP-3", "TRAP-4", "TRAP-5",
        };

        /// <summary>DELIVERABLE A — a MEASUREMENT instrument, not a gate. Same "measures only,
        /// asserts no fit" shape as
        /// <see cref="Evidence_C46_the_stats_panel_strings_against_their_boxes"/>, and
        /// <c>[Explicit]</c> for the same reason: it walks up to <c>TrapSeedCandidates.Length</c>
        /// fresh rooms end to end, which has no place in a routine suite — that guard is
        /// load-bearing on this surface (see the capture harness's own <c>[Explicit]</c> pins).
        ///
        /// <para>For each candidate seed: start a run, find the lowest-line CORNERS-OVER and
        /// CARDS-OVER offers on different matchups (skip the seed, logged, if either is absent —
        /// never invent a selection the board did not offer), place the 2-leg ticket, lock, seat,
        /// fast-forward, and poll every frame for whether leg 0 ever reads <c>W</c> and, while it
        /// does and leg 1 is the live row, whether the footer ever reads <c>RISK</c> (state 1) or
        /// <c>STAKE</c> (state 2). One <c>Debug.Log</c> line per seed — the lead reads the table and
        /// hand-picks a seed for Deliverable B's placeholder constant below.</para>
        ///
        /// <para>Attributes kept directly adjacent to this signature, nothing between them and it.
        /// <c>TvSweatCaptureHarness.cs</c> carries the standing account of why: a T87-am
        /// <c>[Explicit]+[Timeout]</c> pair once sat above THREE stacked XML doc-comments with no
        /// real declaration between them, and C# binds attributes to the next actual member
        /// regardless of doc-comment trivia in between — so both attributes silently landed on the
        /// WRONG method. This doc comment therefore ends immediately before the attributes, with no
        /// further comment text between them and the signature.</para></summary>
        [Explicit("Seed search for the T108 two-state trap gate (DD ruling-t108-trigger-2026-08-17.md "
            + "§5): finds a seed whose corners+cards ticket demonstrates BOTH leg-0-Won-leg-1-RISK "
            + "and leg-0-Won-leg-1-STAKE-before-whistle. Logs one line per candidate. Run by filter "
            + "only.")]
        [Timeout(1500000)]
        [UnityTest]
        public IEnumerator SeedSearch_TrapGateCandidates_LogsWhichSeedsReachBothStates()
        {
            foreach (string seed in TrapSeedCandidates)
            {
                bool leg0EverWon = false;
                bool state1Seen = false; // leg0==W, leg1 live, footer==RISK
                bool state2Seen = false; // leg0==W, leg1 live, footer==STAKE

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

                director.StartNewRun(seed);
                Run run = director.Run;

                // WHY OVER-ONLY (TryFindLowestLineOver does the actual filtering, but the reason
                // belongs here too, in the search pin, not only in the shared helper it calls): an
                // UNDER leg has no early Won; its only pre-whistle verdict is Lost, so an UNDER
                // fixture cannot certify state 2, no matter which seed carries it.
                //
                // Sequential checks below, NOT combined via && — each out-selection must come from
                // an UNCONDITIONAL call so it is definitely assigned on the path that reads it below.
                bool haveCorners = TryFindLowestLineOver(run, MarketKind.TotalCorners, -1,
                    out int cornersMatchup, out MarketSelection cornersSelection);
                if (!haveCorners)
                {
                    UnityEngine.Debug.Log($"[TRAP-SEARCH] seed={seed} SKIPPED (no CORNERS-OVER offer "
                        + $"on this slate) leg0EverWon={leg0EverWon} state1(RISK)={state1Seen} "
                        + $"state2(STAKE)={state2Seen}");
                    continue; // a seed failing the precondition is a RESULT, not a reason to invent
                              // a selection the board did not offer
                }

                bool haveCards = TryFindLowestLineOver(run, MarketKind.TotalCards, cornersMatchup,
                    out int cardsMatchup, out MarketSelection cardsSelection);
                if (!haveCards)
                {
                    UnityEngine.Debug.Log($"[TRAP-SEARCH] seed={seed} SKIPPED (no CARDS-OVER offer "
                        + $"on a DIFFERENT matchup) leg0EverWon={leg0EverWon} "
                        + $"state1(RISK)={state1Seen} state2(STAKE)={state2Seen}");
                    continue;
                }

                const double Stake = 25.0;
                run.PlaceTicket(new List<Pick>
                {
                    new Pick(cornersMatchup, cornersSelection),
                    new Pick(cardsMatchup, cardsSelection),
                }, Stake);
                director.LockRound();

                couch.OnInteract(null);
                yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");
                yield return WaitUntil(() => screen.DebugSeatedDeltaTime > 0f, 20f,
                    "the screen never became seated-and-running");
                for (int i = 0; i < 30; i++) yield return null; // first beat renders a scorebug

                string endNote = "swept to completion";
                float start = Time.realtimeSinceStartup;
                const float maxSeconds = 60f; // per-seed budget — a slow seed is skipped, never
                                               // fatal to the survey
                while (run.Phase == Phase.Sweat)
                {
                    if (Time.realtimeSinceStartup - start > maxSeconds)
                    {
                        endNote = $"TIMED OUT after {maxSeconds}s";
                        break;
                    }

                    string chip0 = screen.DebugLegState(0);
                    string chip1 = screen.DebugLegState(1);
                    if (chip0 == "W") leg0EverWon = true;

                    if (chip0 == "W" && chip1 == string.Empty) // leg 0 Won, leg 1 IS the live row
                    {
                        string footer = screen.DebugTicketRiskText;
                        string footerWord = footer.Length > 0 ? footer.Split(' ')[0] : string.Empty;
                        if (footerWord == "RISK") state1Seen = true;
                        else if (footerWord == "STAKE") state2Seen = true;
                    }

                    yield return null;
                }

                UnityEngine.Debug.Log($"[TRAP-SEARCH] seed={seed} {endNote} "
                    + $"leg0EverWon={leg0EverWon} state1(RISK)={state1Seen} "
                    + $"state2(STAKE)={state2Seen} cornersLine={cornersSelection.Line} "
                    + $"cardsLine={cardsSelection.Line}");
            }

            // MEASUREMENT ONLY, same shape as C46 — this pin asserts nothing about which seed wins.
            // The lead reads the [TRAP-SEARCH] lines above and hand-picks a seed with
            // state1(RISK)=True AND state2(STAKE)=True for Deliverable B's placeholder constant.
        }

        // MEASURED, NOT GUESSED — "measure, then pin", the same route that chose STATS-MULTI-1
        // itself (see SeatOnAMultiCountTicket's own comment). The seed search directly above was run
        // 2026-08-17 over all twelve candidates; its [TRAP-SEARCH] lines are the record, and this is
        // the ONLY candidate that reached BOTH states:
        //
        //   seed=STATS-MULTI-5  leg0EverWon=True  state1(RISK)=True  state2(STAKE)=True
        //                       cornersLine=8.5   cardsLine=3.5
        //
        // The near misses are kept here because they are what makes the choice non-arbitrary, and
        // because a later seat re-running the search should recognise the shape rather than re-derive
        // it: STATS-MULTI-1/-3, TRAP-2 and TRAP-5 reach state 1 but never state 2 (leg 1 never clears
        // its line before its own whistle); STATS-MULTI-2 reaches state 2 but never state 1; and five
        // of the twelve never get leg 0 won at all. ONE seed in twelve carries both, which is exactly
        // why the ruling refused to let this gate depend on an unpinned draw.
        //
        // If this seed ever stops qualifying, RE-RUN THE SEARCH — never widen the gate to match
        // whatever the seed now does.
        private const string TrapGateSeed = "STATS-MULTI-5";

        /// <summary>DELIVERABLE B — THE WIRING GATE (DD ruling-t108-trigger-2026-08-17.md §5),
        /// built to exercise both named states BY CONSTRUCTION rather than by luck.
        ///
        /// <para><b>A broader pin was drafted here and DELETED rather than kept, and the reason is
        /// the whole point of this gate.</b> It asserted that any decided leg forces <c>RISK</c>
        /// while any other leg is undecided — which reads as a safe superset and is in fact FALSE
        /// under the ruling: state 2 is precisely leg 0 decided, leg 1 undecided-by-chip, and the
        /// footer correctly reading <c>STAKE</c>. That pin would have failed on the exact state this
        /// fix exists to produce. A "broader" assertion over a state space you have not enumerated
        /// is not a stronger claim, it is an unenumerated one.</para>
        ///
        /// <para>The two states the ruling names cannot be told apart by the rows at all — leg 1's
        /// chip reads identically blank/live in both — so the footer word is the only discriminator,
        /// and it is bucketed by what was OBSERVED rather than checked against a re-derivation of the
        /// model:</para>
        ///
        /// <para>STATE 1 — leg 0 resolved <c>Won</c>, leg 1 the live row → footer reads <c>RISK</c>.
        /// The trap: a won leg must not reach the ticket word.</para>
        /// <para>STATE 2 — leg 0 resolved <c>Won</c>, leg 1 STILL the live row (chip not yet
        /// W/L/VOID) but ALREADY won on its revealed count, ahead of its own whistle → footer reads
        /// <c>STAKE</c>. This is the fix actually working on a multi-leg ticket — the state the
        /// whole spec was written for (ruling §4's "decided, but not yet resolved" third
        /// state).</para>
        ///
        /// <para>OVER-only selections, lowest line: see <see cref="TryFindLowestLineOver"/> — an
        /// UNDER leg has no early Won, so an UNDER fixture cannot reach state 2 on any seed.</para>
        ///
        /// <para>THE SEED IS A NAMED PLACEHOLDER (<see
        /// cref="TrapGateSeed"/>), pending Deliverable A's measurement — see
        /// that constant's own comment. No force-hook drives the ledger and adding one to production
        /// is out of scope for this dispatch, so the construction is a SEED CHOICE, not a hook.
        /// </para>
        ///
        /// <para>DOES NOT PROVE: how <c>WON</c>/<c>STAKE</c> read at review distance — that is a C11
        /// frame claim neither this gate nor the one above states anything about (ruling
        /// §6).</para></summary>
        [UnityTest]
        public IEnumerator TicketFooterWord_LegOneWon_RiskWhileLegTwoLive_StakeWhenLegTwoWonEarly()
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

            director.StartNewRun(TrapGateSeed);
            Run run = director.Run;
            Assert.AreEqual(Phase.Betting, run.Phase, "a fresh run opens in Betting");

            bool haveCorners = TryFindLowestLineOver(run, MarketKind.TotalCorners, -1,
                out int cornersMatchup, out MarketSelection cornersSelection);
            Assert.IsTrue(haveCorners,
                $"seed '{TrapGateSeed}' offers no CORNERS-OVER selection — "
                + "the pinned seed has stopped qualifying — RE-RUN the seed search above and re-pin, "
                + "never invent a selection the "
                + "board did not offer");

            bool haveCards = TryFindLowestLineOver(run, MarketKind.TotalCards, cornersMatchup,
                out int cardsMatchup, out MarketSelection cardsSelection);
            Assert.IsTrue(haveCards,
                $"seed '{TrapGateSeed}' offers no CARDS-OVER selection on a "
                + "DIFFERENT matchup — the pinned seed has stopped qualifying; RE-RUN the seed search "
                + "above and re-pin, never invent a selection the board did not "
                + "offer");

            const double Stake = 25.0;
            run.PlaceTicket(new List<Pick>
            {
                new Pick(cornersMatchup, cornersSelection),
                new Pick(cardsMatchup, cardsSelection),
            }, Stake);
            director.LockRound();

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");
            yield return WaitUntil(() => screen.DebugSeatedDeltaTime > 0f, 20f,
                "the screen never became seated-and-running");
            for (int i = 0; i < 30; i++) yield return null; // let the first beat render a scorebug

            int framesSampled = 0;
            int state1Cases = 0;
            int state2Cases = 0;

            float start = Time.realtimeSinceStartup;
            // Same failsafe shape as the other every-frame pins in this file: a hang is a FAILURE,
            // never a silent pass.
            const float maxSeconds = 60f;
            while (run.Phase == Phase.Sweat)
            {
                if (Time.realtimeSinceStartup - start > maxSeconds)
                {
                    Assert.Fail($"the sweat never settled within {maxSeconds}s wall-clock (frames "
                        + $"sampled so far: {framesSampled}, state1={state1Cases}, "
                        + $"state2={state2Cases})");
                    yield break;
                }

                framesSampled++;
                string chip0 = screen.DebugLegState(0);
                string chip1 = screen.DebugLegState(1);

                // THE QUALIFYING FRAME for both states: leg 0 resolved Won, leg 1 IS the live row
                // (blank chip — DebugLegState's own documented meaning of "live"). Off this frame,
                // the footer word is the ONLY thing that tells states 1 and 2 apart — leg 1's OWN
                // chip reads identically (blank) in both, which IS ruling §4's "decided, but not yet
                // resolved" third state: the model has already graded the leg, the chip has not
                // caught up, and the footer is the one surface reading the model's grade rather than
                // the whistle. Bucketing by the observed word (never re-deriving an expectation from
                // the model) is reading the ruling's own definition of each state literally.
                if (chip0 == "W" && chip1 == string.Empty)
                {
                    string footer = screen.DebugTicketRiskText;
                    string footerWord = footer.Length > 0 ? footer.Split(' ')[0] : string.Empty;

                    if (footerWord == "RISK")
                    {
                        state1Cases++;
                    }
                    else if (footerWord == "STAKE")
                    {
                        state2Cases++;
                    }
                    else
                    {
                        Assert.Fail($"frame {framesSampled}: leg 0 is Won and leg 1 is the live row, "
                            + $"so the footer must read RISK or STAKE — got '{footerWord}'");
                    }
                }

                yield return null;
            }

            UnityEngine.Debug.Log($"[TRAP-GATE] seed={TrapGateSeed} "
                + $"frames={framesSampled} state1Cases={state1Cases} state2Cases={state2Cases}");

            // C29: a gate reports its executed case count and fails on zero. Both states are RULED
            // requirements (§5), not one — a run that only ever showed state 1 has not certified the
            // fix "actually working on a multi-leg ticket" (§5 item 2), the state the whole spec was
            // written for.
            Assert.Greater(state1Cases, 0,
                $"state 1 (leg 0 Won, leg 1 live, footer RISK) was never observed across "
                + $"{framesSampled} frames on seed '{TrapGateSeed}' — this gate "
                + "has proven nothing about the trap (C29)");
            Assert.Greater(state2Cases, 0,
                $"state 2 (leg 0 Won, leg 1 live, footer STAKE on the revealed count) was never "
                + $"observed across {framesSampled} frames on seed "
                + $"'{TrapGateSeed}' — this gate has proven nothing about the "
                + "fix actually working on a multi-leg ticket (C29)");
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

        /// <summary>DD batch 93 item 5: THE ROW SET KEYS TO THE TICKET, so its premise changed —
        /// a row the ticket never bought is not "unrevealed", it does not exist. On a moneyline-only
        /// ticket (DemoTicketPolicy's picks) the ticket carries no TotalCorners/TotalCards leg, so
        /// CORNERS and CARDS must be ABSENT rows, never printed with
        /// <see cref="TvSweatScreen.DebugStatsUnrevealedMark"/>. The mark is reserved for a row the
        /// ticket DID buy but has not yet revealed (see the multi-count pins below) — "a leak here is
        /// a blocker, not a polish item" still holds, just narrowed to bought rows (Allen,
        /// 2026-08-15).
        ///
        /// <para><b>DD batch 95 re-authoring:</b> "ABSENT" changed FORM. It used to mean a row printed
        /// blank (label/A/B all empty strings, still occupying its slot's height) — <c>"||"</c> off
        /// <see cref="TvSweatScreen.DebugStatsRow"/>. "An unbought row is not a silent row, it is NO
        /// row": the slot itself is gone, so <c>DebugStatsRow</c> now returns <c>null</c> for it. This
        /// pin is renamed (was <c>Stats_panel_omits_corners_and_cards_rows_off_a_moneyline_ticket</c>)
        /// because "omits ... rows" described the old blank-in-place form; the claim now is
        /// absence.</para></summary>
        [UnityTest]
        public IEnumerator Stats_panel_corners_and_cards_rows_are_absent_off_a_moneyline_ticket()
        {
            yield return OpenStatsPanelOnALiveLeg();
            TvSweatScreen screen = _statsScreen;

            // DemoTicketPolicy picks MONEYLINE only, so the ticket's row set carries neither count
            // kind — ABSENT means no slot at all (null), not a blank one and not the unrevealed mark.
            Assert.IsNull(screen.DebugStatsRow(1),
                "a moneyline ticket carries no TotalCorners leg, so the CORNERS row must be ABSENT — "
                + "no slot at all, not blank and not the unrevealed mark. Got: "
                + screen.DebugStatsRow(1));
            Assert.IsNull(screen.DebugStatsRow(2),
                "a moneyline ticket carries no TotalCards leg, so the CARDS row must be ABSENT — no "
                + "slot at all, not blank and not the unrevealed mark. Got: " + screen.DebugStatsRow(2));

            // NON-VACUITY: a build that reported every row absent would satisfy both assertions above.
            // The goals row IS revealed, must still be PRESENT, and must carry figures, or this test
            // proves only that the panel says nothing.
            string goals = screen.DebugStatsRow(0);
            Assert.IsNotNull(goals,
                "the GOALS row is unconditional and must be PRESENT even on a moneyline ticket");
            StringAssert.StartsWith("GOALS|", goals);
            string mark = screen.DebugStatsUnrevealedMark;
            StringAssert.DoesNotContain(mark, goals,
                "the GOALS row is revealed-ledger data and must never carry the unrevealed mark — "
                + "if it does, the panel is blanking/marking everything and the assertions above are "
                + "passing on silence");
        }

        /// <summary>DD batch 93 items 5-6: seats the player on a ticket carrying BOTH a
        /// TotalCorners leg (leg 0) AND a TotalCards leg (leg 1) — a single-count ticket cannot show
        /// a row set being SELECTED, which is the whole point of the pins that use this. Both
        /// selections are taken OFF THE BOARD (<c>matchup.Markets</c>), never constructed — the
        /// corners/cards line is generated per matchup, so an invented selection may not be a line
        /// that matchup actually offers.
        ///
        /// <para>The two legs are deliberately pinned to DIFFERENT matchups (a second pass excludes
        /// the corners matchup from the cards search) so the ticket prices on the ordinary
        /// independent-legs path — no same-match correlation model enters at all — and so the two
        /// legs' matches are distinguishable by their own scorebug text, which the pins below use to
        /// detect a leg change.</para></summary>
        private IEnumerator SeatOnAMultiCountTicket()
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

            // PINNED SEED, not the natural random draw. Two pins built on this helper each wait for
            // a full leg to reach ITS OWN LegFinal (matchupText changing) inside a bounded wall-clock
            // window — an un-seeded run's beat count and time-to-LegFinal vary draw to draw, so under
            // full-suite load (fewer frames per wall-clock second, not fewer needed) an unlucky draw
            // can miss a budget that an easy one clears with room to spare. That is exactly what
            // happened: the FILTERED run (this class alone) passed 25/25; the FULL 122-test run
            // failed only this helper's retention pin on "never advanced past the corners leg (waited
            // 90s)" — the guard did its job and refused to assert on a leg change it never observed,
            // rather than passing on a state it never reached.
            //
            // "STATS-MULTI-1" is not a guess: it is the exact seed DD batch 93's own capture,
            // TvSweatCaptureHarness.Capture_StatsPanel_MultiCountTicket, already runs — its slate is
            // KNOWN (from that capture's own log) to offer both a TotalCorners and a TotalCards
            // market, and its corners leg is KNOWN to reveal a nonzero count by match minute 32,
            // inside 37 real seconds total, AT SHIP PACING (TimeScaleOverride 1, i.e. with NO
            // acceleration at all). At this file's 0.0001 fast-forward that resolves in a handful of
            // frames — deterministically, not as a wall-clock gamble that degrades again under load.
            director.StartNewRun("STATS-MULTI-1");
            Run run = director.Run;
            Assert.AreEqual(Phase.Betting, run.Phase, "a fresh run opens in Betting");

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
                "no matchup on this slate offers TotalCorners - this is a re-seed, never a reason to "
                + "invent a selection the board did not offer");

            int cardsMatchup = -1;
            MarketSelection cardsSelection = default;
            foreach (Matchup mm in run.CurrentSlate.Matchups)
            {
                if (mm.Index == cornersMatchup) continue; // DIFFERENT matchup - ordinary pricing path
                foreach (MarketOffer off in mm.Markets)
                {
                    if (off.Selection.Kind != MarketKind.TotalCards) continue;
                    cardsMatchup = mm.Index;
                    cardsSelection = off.Selection;
                    break;
                }
                if (cardsMatchup >= 0) break;
            }
            Assert.GreaterOrEqual(cardsMatchup, 0,
                "no OTHER matchup on this slate offers TotalCards - this is a re-seed, never a reason "
                + "to invent a selection the board did not offer");

            const double Stake = 25.0;
            run.PlaceTicket(new List<Pick>
            {
                new Pick(cornersMatchup, cornersSelection),
                new Pick(cardsMatchup, cardsSelection),
            }, Stake);
            director.LockRound();

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");
            yield return WaitUntil(() => screen.DebugSeatedDeltaTime > 0f, 20f,
                "the screen never became seated-and-running");
            for (int i = 0; i < 30; i++) yield return null; // let the first beat render a scorebug

            _statsScreen = screen;
        }

        /// <summary>DD batch 93 item 5: the row SET keys to the TICKET and does not change as legs
        /// advance. A multi-count ticket yields BOTH the CORNERS and CARDS rows from the moment it is
        /// adopted — asserted twice, across a real leg change, so a row that flickered in and out as
        /// the live leg moved would fail here exactly as the pre-existing "always three rows, marked
        /// off the live leg" defect would have on the OLD assertion shape.</summary>
        [UnityTest]
        public IEnumerator Stats_panel_row_set_keys_to_the_ticket_and_holds_across_a_leg_change()
        {
            yield return SeatOnAMultiCountTicket();
            TvSweatScreen screen = _statsScreen;

            TMP_Text matchupText = FindChildComponent<TMP_Text>(screen, "Matchup");
            Assert.IsNotNull(matchupText, "no Matchup element — re-point this pin, never delete it");
            string matchupAtFirstLeg = matchupText.text;

            screen.ForceStatsPanel(true);
            Assert.IsNotNull(screen.DebugStatsRow(0),
                "the panel rendered no rows — it never had a leg, so nothing below is proven");
            StringAssert.StartsWith("CORNERS|", screen.DebugStatsRow(1),
                "a multi-count ticket must yield a CORNERS row from the moment it is adopted, "
                + "regardless of which leg is currently live");
            StringAssert.StartsWith("CARDS|", screen.DebugStatsRow(2),
                "a multi-count ticket must yield a CARDS row from the moment it is adopted, "
                + "regardless of which leg is currently live");
            screen.ForceStatsPanel(false);

            // Advance OFF the first leg. The two legs sit on deliberately DIFFERENT matchups
            // (SeatOnAMultiCountTicket), so the scorebug's matchup text changing is the leg-change
            // signal.
            yield return WaitUntil(() => matchupText.text != matchupAtFirstLeg, 90f,
                "the sweat never advanced off the first leg's matchup — cannot prove the row set "
                + "survives a leg change without one actually happening");

            screen.ForceStatsPanel(true);
            StringAssert.StartsWith("CORNERS|", screen.DebugStatsRow(1),
                "the row SET must not change as legs advance — CORNERS vanished after the leg "
                + "change, which is the exact defect DD batch 93 item 1 replaces");
            StringAssert.StartsWith("CARDS|", screen.DebugStatsRow(2),
                "the row SET must not change as legs advance — CARDS vanished after the leg change, "
                + "which is the exact defect DD batch 93 item 1 replaces");
        }

        /// <summary>DD batch 93 item 5 (item 2's trap, pinned directly): a revealed count is RETAINED
        /// once its leg stops being live. <c>_countLedger</c> is replaced the instant the next leg
        /// begins (<c>BeginStageLeg</c>) — reading it directly for the CORNERS row after the CARDS
        /// leg has gone live would read CARDS' fresh 0/0, not corners' own revealed total, which is
        /// precisely how a revealed fact would un-reveal itself under the player.
        ///
        /// <para>Deliberately does NOT pin the exact number across the transition: corners can keep
        /// revealing right up to its own LegFinal, so a value snapshotted while still live can be
        /// stale by the time the leg actually ends (a real race this pin hit once during authoring —
        /// see git history). The claim item 2 makes is narrower and does not need that number: a
        /// row that showed a REAL reveal while its leg was live must still show a REAL (non-mark)
        /// reveal once that leg is no longer live — never revert to <see cref="TvSweatScreen.
        /// DebugStatsUnrevealedMark"/>. Reading the panel row rather than the raw ledger hooks also
        /// means the assertion exercises exactly what a player sees.</para></summary>
        [UnityTest]
        public IEnumerator Stats_panel_retains_a_revealed_count_after_its_leg_stops_being_live()
        {
            yield return SeatOnAMultiCountTicket();
            TvSweatScreen screen = _statsScreen;
            string mark = screen.DebugStatsUnrevealedMark;

            TMP_Text matchupText = FindChildComponent<TMP_Text>(screen, "Matchup");
            Assert.IsNotNull(matchupText, "no Matchup element — re-point this pin, never delete it");
            string matchupAtFirstLeg = matchupText.text;

            // The FIRST leg is CORNERS (SeatOnAMultiCountTicket's pick order). Wait for it to reveal
            // something WHILE STILL LIVE — the emptiest state cannot be read for whether retention
            // works.
            yield return WaitUntil(
                () => screen.DebugRevealedCountHome >= 0
                      && screen.DebugRevealedCountHome + screen.DebugRevealedCountAway > 0,
                90f,
                "the corners leg never revealed a count - this is a re-seed, never a reason to prove "
                + "retention off an empty ledger");

            screen.ForceStatsPanel(true);
            string cornersRowWhileLive = screen.DebugStatsRow(1);
            Assert.AreNotEqual($"CORNERS|{mark}|{mark}", cornersRowWhileLive,
                $"PRECONDITION: corners must show a REAL reveal while its own leg is live, got "
                + $"'{cornersRowWhileLive}' — nothing below is proven without this");
            screen.ForceStatsPanel(false);

            // Advance OFF the corners leg — _countLedger is replaced under the player the instant the
            // cards leg begins. This is the exact trap item 2 exists to close.
            yield return WaitUntil(() => matchupText.text != matchupAtFirstLeg, 90f,
                "the sweat never advanced past the corners leg — cannot prove retention without a "
                + "leg change actually happening");

            screen.ForceStatsPanel(true);
            string cornersRowAfter = screen.DebugStatsRow(1);
            StringAssert.StartsWith("CORNERS|", cornersRowAfter,
                "the row itself must still be PRESENT once the cards leg is live — an absent row "
                + "here would be item 1's defect, not item 2's");
            Assert.AreNotEqual($"CORNERS|{mark}|{mark}", cornersRowAfter,
                $"the corners reveal must be RETAINED once the cards leg is live — got "
                + $"'{cornersRowAfter}' (the unrevealed mark). A revealed fact un-revealing itself is "
                + "worse than the mark it would be replacing (DD batch 93 item 2)");
        }

        /// <summary>spec-count-theater-2026-08-17.md §7 item 2 — described in §4 as "the assertion
        /// that matters most": <b>a fixture running a full sweat must finish with the column's total
        /// equal to the match's own.</b> §4's binding is that <c>StageBeat()</c> advances the count
        /// ledger's cursor unconditionally, so any beat that consumes a batch without committing it
        /// leaves the revealed column short of a total the match actually reached — this pin is the
        /// gate against exactly that.
        ///
        /// <para>Reuses <see cref="SeatOnAMultiCountTicket"/> rather than a new fixture (pinned seed
        /// <c>STATS-MULTI-1</c>, leg 0 CORNERS / leg 1 CARDS — see that helper's own comment) and
        /// runs the WHOLE ticket to settlement, the same wall-clock-failsafe shape as
        /// <see cref="TicketFooterWord_LegOneWon_RiskWhileLegTwoLive_StakeWhenLegTwoWonEarly"/> and
        /// <see cref="TicketFooterWord_NeverDisagreesWithAnyRow_AndNoLiveRowEverPrintsNeedZero"/>: a
        /// hang is a FAILURE (<c>Assert.Fail</c>), never a silent pass. Running the whole ticket
        /// (not just corners' own leg) exercises cards' identical StageBeat/commit path too, not
        /// only corners'.</para>
        ///
        /// <para><b>Why the corners figure is captured MID-LOOP, the instant leg 0 stops being live,
        /// rather than read off the screen after the whole ticket settles:</b> <c>_countLedger</c> is
        /// REPLACED the instant leg 1 (cards) begins (T62 note on <c>RepaintRevealedScore</c>;
        /// <see cref="Stats_panel_retains_a_revealed_count_after_its_leg_stops_being_live"/>'s own
        /// trap), so <c>DebugRevealedCountHome/Away</c> would read CARDS by the end, not CORNERS. The
        /// STATS PANEL's RETAINED row (index 1, "CORNERS|away|home") is what DD batch 93 item 2 built
        /// so a count revealed earlier survives its leg going non-live — captured here at the
        /// earliest frame that row can hold corners' FINAL figure (leg 0 just decided) rather than a
        /// value snapshotted mid-reveal, which <see cref="Stats_panel_retains_a_revealed_count_after_its_leg_stops_being_live"/>'s
        /// own comment already warns can be stale. This also sidesteps ever reading screen state
        /// after <c>run.Phase</c> leaves Sweat, which nothing in this suite has previously
        /// exercised. Opening the panel FREEZES time (§8.8's "TIME STOPS" — see
        /// <see cref="Stats_panel_opening_stops_time_at_its_single_authority"/>), so it is closed
        /// again in the same frame, before the next <c>yield return null</c> — left open it would
        /// freeze leg 1 forever and this pin would fail on its own wall-clock budget instead of
        /// proving anything.</para>
        ///
        /// <para>"The match's own total" is read directly off <c>Leg.Matchup.StatLine</c> — the
        /// exact ground-truth field <c>CountLedger.ConfigureEndpoint(MatchStatLine, ...)</c> itself
        /// reads to set <c>TargetHome</c>/<c>TargetAway</c> in the first place
        /// (SweatPresentationModel.cs), and the same field <c>ScoreLedgerTests.cs</c> already reads
        /// for the identical comparison (<c>cornersLeg.Matchup.StatLine.HomeCorners + AwayCorners</c>).
        /// Not a number computed here, and not a new TvSweatScreen accessor: no production surface
        /// gains any new way to leak the locked target, because none is touched — this reads a field
        /// already public on the engine's own <c>Matchup</c>, exactly as production code and
        /// <c>ScoreLedgerTests.cs</c> already do.</para>
        ///
        /// <para><b>What this does NOT prove</b> (see <see cref="SceneSpec.QuietCount"/>'s own doc):
        /// this phase is a no-op by design — nothing yet populates <c>QuietCount</c>, so every count
        /// batch in this run commits through the narrated <c>Count</c> path exactly as before this
        /// dispatch. This pin certifies the invariant still holds (it always did — DD batch 93's
        /// commit path was already unconditional per beat) now that the commit call has been
        /// factored out, AHEAD of the later significance gate that will actually decline a batch and
        /// route it through <c>QuietCount</c> instead. It says nothing about whether any particular
        /// beat was quiet, because in this phase none are.</para></summary>
        [UnityTest]
        public IEnumerator FullSweat_RevealedCountColumnFinishesEqualToTheMatchsOwnTotal()
        {
            yield return SeatOnAMultiCountTicket();
            TvSweatScreen screen = _statsScreen;
            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            Assert.IsNotNull(director, "RunDirector missing");
            Run run = director.Run;
            Assert.IsNotNull(run, "no run - SeatOnAMultiCountTicket did not seat a live run");

            Leg cornersLeg = director.CurrentTicket.Legs[0];
            Assert.AreEqual(MarketKind.TotalCorners, cornersLeg.Selection.Kind,
                "SeatOnAMultiCountTicket's own pick order puts CORNERS at leg 0 - re-point this pin "
                + "if that order ever changes, never assume it silently");
            int matchHome = cornersLeg.Matchup.StatLine.HomeCorners;
            int matchAway = cornersLeg.Matchup.StatLine.AwayCorners;

            string mark = screen.DebugStatsUnrevealedMark;
            string cornersRowAtLegEnd = null;

            int framesSampled = 0;
            float start = Time.realtimeSinceStartup;
            const float maxSeconds = 60f;
            // Same failsafe shape as the file's other full-sweat pins: a hang is a FAILURE, never a
            // silent pass. Runs the WHOLE ticket (both legs) to settlement per §7 item 2, not just
            // corners' own leg.
            while (run.Phase == Phase.Sweat)
            {
                if (Time.realtimeSinceStartup - start > maxSeconds)
                {
                    Assert.Fail($"the sweat never settled within {maxSeconds}s wall-clock (frames "
                        + $"sampled so far: {framesSampled})");
                    yield break;
                }
                framesSampled++;

                // Snapshot the CORNERS retained row the FIRST frame leg 0 is no longer the live row
                // (DebugLegState(0) leaves "" for "W"/"L"/"VOID" — never "NEXT", since corners is
                // always the first leg to go live) — the earliest moment its own total can have
                // stopped changing, while the screen is definitely still mid-Sweat and valid.
                if (cornersRowAtLegEnd == null && screen.DebugLegState(0) != string.Empty)
                {
                    screen.ForceStatsPanel(true);
                    cornersRowAtLegEnd = screen.DebugStatsRow(1);
                    screen.ForceStatsPanel(false); // must close in-frame - open freezes time (§8.8)
                }

                yield return null;
            }

            // C29: a gate reports its executed case count and fails on zero - a fixture that never
            // reached settlement, or whose leg 0 never actually decided, has proven nothing.
            Assert.Greater(framesSampled, 0,
                "zero frames sampled before the run left Phase.Sweat - the fixture never actually "
                + "ran, so nothing below is proven (C29)");
            Assert.IsNotNull(cornersRowAtLegEnd,
                "leg 0 (corners) never reached a decided state across the whole sweat - cannot prove "
                + "the count-commit invariant without corners actually finishing (C29)");

            StringAssert.StartsWith("CORNERS|", cornersRowAtLegEnd,
                $"row 1 was not CORNERS when leg 0 finished — got '{cornersRowAtLegEnd}'");
            Assert.AreNotEqual($"CORNERS|{mark}|{mark}", cornersRowAtLegEnd,
                "the CORNERS row still carried the unrevealed mark once its own leg finished - the "
                + "leg settled without ever committing a count, which is §4's exact defect");

            string[] parts = cornersRowAtLegEnd.Split('|');
            Assert.AreEqual(3, parts.Length, $"unexpected CORNERS row shape: '{cornersRowAtLegEnd}'");
            Assert.IsTrue(int.TryParse(parts[1], out int revealedAway),
                $"CORNERS row's away figure did not parse as an int: '{parts[1]}'");
            Assert.IsTrue(int.TryParse(parts[2], out int revealedHome),
                $"CORNERS row's home figure did not parse as an int: '{parts[2]}'");

            // §7 item 2, the assertion that matters most: the REVEALED column must finish EQUAL to
            // the MATCH'S OWN total - not "close", exactly equal, or a batch was consumed by
            // StageBeat() without ever being committed (§4) and the column fell short of a total the
            // match actually reached. Component-wise, not just the sum, so one side over-counting
            // while the other under-counts by the same amount cannot hide behind a matching total.
            Assert.AreEqual(matchAway, revealedAway,
                $"revealed AWAY corners {revealedAway} != the match's own AWAY corners {matchAway}");
            Assert.AreEqual(matchHome, revealedHome,
                $"revealed HOME corners {revealedHome} != the match's own HOME corners {matchHome}");
            Assert.AreEqual(matchAway + matchHome, revealedAway + revealedHome,
                $"revealed corners total {revealedAway + revealedHome} != the match's own total "
                + $"{matchAway + matchHome} after its leg finished - a count batch was consumed "
                + "without being committed (§4)");

            UnityEngine.Debug.Log($"[COUNT-COMMIT] frames={framesSampled} "
                + $"revealedTotal={revealedAway + revealedHome} matchTotal={matchAway + matchHome}");
        }

        /// <summary>spec-count-theater-2026-08-17.md §7 item 1: "a count event below the
        /// significance threshold does NOT produce a count scene, and the beat reaches the base
        /// table." Reuses <see cref="SeatOnAMultiCountTicket"/> (pinned seed
        /// <c>STATS-MULTI-1</c>, leg 0 CORNERS) rather than a new fixture, per this phase's own
        /// instruction to use the existing pinned corners fixture.
        ///
        /// <para>The corners leg's OVER Choice is not a property of this seed's draw — it is
        /// guaranteed by construction: <c>engine/MatchModel.cs</c>'s <c>BuildOffers</c> always
        /// adds the OVER offer before the UNDER offer for a matchup's TotalCorners market, and
        /// both this fixture's own search and <c>TvSweatCaptureHarness</c>'s control arm take
        /// "the first TotalCorners offer" — so this leg is OVER for any seed, not by luck of
        /// this one. Asserted below rather than merely assumed, so a future change to that
        /// ordering fails loudly here instead of silently proving nothing.</para>
        ///
        /// <para><b>Asserts on the SURFACE, never re-deriving from the model</b>
        /// (<c>SweatActiveLegModel.Classify</c> is not called here — comparing the surface
        /// against the model would only prove the surface agrees with itself). Samples every
        /// frame the corners leg is still live (<c>DebugLegState(0) == ""</c> — <c>_countLedger</c>
        /// is still corners' own, the same trap <see
        /// cref="Stats_panel_retains_a_revealed_count_after_its_leg_stops_being_live"/> already
        /// documents) and looks for a REVEALED-COUNT INCREASE whose scene was one of the base
        /// table's own Momentum templates (CalmPossession/TerritoryFor/TerritoryAgainst) rather
        /// than a count template — the observable signature of a quieted batch reaching the base
        /// table (§3) while still being committed (§4, the same invariant <see
        /// cref="FullSweat_RevealedCountColumnFinishesEqualToTheMatchsOwnTotal"/> pins at the
        /// leg's end). Deliberately NOT "any non-Corner template", which would also match the
        /// unrelated LegFinal correction window this sampling window structurally excludes
        /// anyway (the loop exits the instant <c>DebugLegState(0)</c> leaves "") — the tighter
        /// check means this pin cannot be satisfied by that confound even if the exclusion were
        /// ever weakened. C29: the executed case count is reported and the pin fails on
        /// zero.</para></summary>
        [UnityTest]
        public IEnumerator QuietCountBeat_ProducesNoCountScene_AndTheCountStillCommits()
        {
            // A CORNERS-ONLY fixture and a SHORT warm-up, both forced by measurement. This pin first
            // ran on SeatOnAMultiCountTicket and reported templatesSeen=[Booking,LegFinalLost,
            // NearMissHope] finalTotal=2 on a corners leg whose real total was 11 - IT WAS WATCHING
            // THE CARDS LEG. _countLedger is per-leg and is REPLACED when leg 1 goes live, so
            // DebugRevealedCountHome/Away stop reading corners the moment cards start. Phase A's pin
            // dodges that by reading the RETAINED row; a pin that must also watch SCENE TEMPLATES
            // cannot, because the templates it needs are leg 0's. One count leg removes the confound
            // at the root. The 4-frame warm-up is the other half: 30 frames consumed leg 0's beats
            // outright on the sibling fixture, so sampling must start while beats are still to come.
            yield return SeatOnACornersOnlyTicket(warmUpFrames: 4);
            TvSweatScreen screen = _statsScreen;
            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            Assert.IsNotNull(director, "RunDirector missing");
            Run run = director.Run;
            Assert.IsNotNull(run, "no run - SeatOnAMultiCountTicket did not seat a live run");

            Leg cornersLeg = director.CurrentTicket.Legs[0];
            Assert.AreEqual(MarketKind.TotalCorners, cornersLeg.Selection.Kind,
                "SeatOnAMultiCountTicket's own pick order puts CORNERS at leg 0 - re-point this "
                + "pin if that order ever changes, never assume it silently");
            Assert.AreEqual(1, director.CurrentTicket.Legs.Count,
                "T115: this pin needs a CORNERS-ONLY ticket - see the fixture note above");
            Assert.AreEqual(MarketChoice.Over, cornersLeg.Selection.Choice,
                "engine/MatchModel.cs always offers OVER before UNDER for TotalCorners, and both "
                + "this fixture and TvSweatCaptureHarness's control arm take the first offer - if "
                + "this ever reads Under, the board's generation order changed and this pin needs "
                + "re-pointing, not silent adaptation (the distance gate is OVER-only by spec §6)");

            int quietCommits = 0;
            int countIncreases = 0;
            var templatesSeen = new SortedSet<string>();
            var templatesAtIncrease = new List<string>();
            int priorTotal = screen.DebugRevealedCountHome + screen.DebugRevealedCountAway;
            int framesSampled = 0;
            float start = Time.realtimeSinceStartup;
            const float maxSeconds = 60f;

            // THE LOOP BOUND WAS `while (DebugLegState(0) == string.Empty)` — leg 0 still live —
            // AND IT SAMPLED ZERO FRAMES. Measured: a whole sweat runs ~22 frames past this
            // fixture's seating at its 0.0001 fast-forward, and SeatOnAMultiCountTicket burns 30
            // warm-up frames of its own, so leg 0 had already decided before the loop was entered.
            // The pin's own C29 guard caught it and refused to certify anything, which is the guard
            // working — but the bound has to go.
            //
            // THE CONFOUND IT WAS GUARDING AGAINST IS ALREADY EXCLUDED, AND BY A STRONGER MEANS.
            // The worry was the LegFinal correction window — a real, separate, pre-existing
            // count-commit path just past leg 0's end. But the `reclaimedMomentumScene` whitelist
            // below counts ONLY CalmPossession/TerritoryFor/TerritoryAgainst, and a LegFinal
            // correction renders LegFinalWon/LegFinalLost, which are not in it. So the confound is
            // excluded BY TEMPLATE, not by timing — which is the more robust of the two, and the
            // timing half is exactly what just proved fragile.
            while (run.Phase == Phase.Sweat)
            {
                if (Time.realtimeSinceStartup - start > maxSeconds)
                {
                    Assert.Fail($"the sweat never settled within {maxSeconds}s wall-clock "
                        + $"(frames sampled so far: {framesSampled}, quiet commits so far: {quietCommits})");
                    yield break;
                }
                framesSampled++;

                if (!string.IsNullOrEmpty(screen.DebugSceneTemplate))
                    templatesSeen.Add(screen.DebugSceneTemplate);

                int total = screen.DebugRevealedCountHome + screen.DebugRevealedCountAway;
                if (total > priorTotal)
                {
                    countIncreases++;
                    templatesAtIncrease.Add($"{screen.DebugSceneTemplate}:{priorTotal}->{total}");
                    string template = screen.DebugSceneTemplate;
                    bool reclaimedMomentumScene = template == SceneTemplate.CalmPossession.ToString()
                        || template == SceneTemplate.TerritoryFor.ToString()
                        || template == SceneTemplate.TerritoryAgainst.ToString();
                    if (reclaimedMomentumScene) quietCommits++;
                }
                priorTotal = total;

                yield return null;
            }

            // C29: a gate reports its executed case count and fails on zero - a fixture that
            // never reached settlement, or whose leg 0 never actually decided, has proven
            // nothing.
            // DIAGNOSTICS BEFORE THE ASSERTIONS, deliberately. A gate that fails without saying
            // what it saw costs a whole editor window to re-run for the answer, and this window is
            // a shared, serialized resource. Everything needed to tell "the gate never fired" from
            // "the gate fired and this pin cannot see it" is printed first.
            UnityEngine.Debug.Log($"[QUIET-COUNT-GATE] frames={framesSampled} quietCommits={quietCommits} "
                + $"cornersLine={cornersLeg.Selection.Line} choice={cornersLeg.Selection.Choice} "
                + $"countIncreases={countIncreases} finalTotal={priorTotal} "
                + $"templatesSeen=[{string.Join(",", templatesSeen)}] "
                + $"templatesAtIncrease=[{string.Join(",", templatesAtIncrease)}]");

            Assert.Greater(framesSampled, 0,
                "zero frames sampled across the whole sweat - the fixture never actually ran, so "
                + "nothing below is proven (C29)");
            Assert.Greater(quietCommits, 0,
                "no quiet count commit was observed across the whole corners leg (C29) - either "
                + "the distance gate never found a beat below its significance threshold on this "
                + "seed's own path (a re-seed/re-point question, not necessarily a code defect), "
                + "or the gate is not wired - see this pin's own doc for what it samples and why");

            UnityEngine.Debug.Log($"[QUIET-COUNT-GATE] frames={framesSampled} quietCommits={quietCommits}");
        }

        /// <summary>spec-count-theater-2026-08-17.md §7 item 5 / §2 THE A-REVEAL (T109-cl, ruled
        /// FINAL): "assert the scoreline reveals independently of the ticket's market, on a
        /// corners fixture whose match scores." Reuses <see cref="SeatOnACornersOnlyTicket"/>
        /// (pinned seed <c>STATS-COUNT-1</c>, leg 0 CORNERS, no CARDS leg) rather than inventing a
        /// fixture, per this dispatch's own instruction.
        ///
        /// <para>A SHORT warm-up (4, not the 30 default) for the SAME measured reason
        /// <see cref="QuietCountBeat_ProducesNoCountScene_AndTheCountStillCommits"/> already gives
        /// its own: at this fixture's 0.0001 fast-forward a whole sweat runs only a few dozen
        /// frames, and 30 warm-up frames has already been measured consuming a leg's beats
        /// outright on the multi-count sibling fixture (see that pin's own comment). This gate
        /// needs frames still ahead of leg 0 to sample across, not a leg that has already
        /// decided.</para>
        ///
        /// <para><b>The precondition this gate cannot assume:</b> whether STATS-COUNT-1's own
        /// match actually scores is a property of the seed's simulation, not of this test's code,
        /// so it is asserted explicitly, first, straight off <c>Leg.Matchup.StatLine</c> — the
        /// same ground-truth field <see cref="FullSweat_RevealedCountColumnFinishesEqualToTheMatchsOwnTotal"/>
        /// already reads for corners. A scoreless match on this seed is a RE-POINT (a different
        /// seed, or a forced-goal harness), never a silent pass — the failure message says so
        /// rather than letting the gate below pass vacuously.</para>
        ///
        /// <para><b>What "before full time" means here, operationally:</b> sampled while leg 0 is
        /// still LIVE (<c>DebugLegState(0) == ""</c> — the same live-window instrument
        /// <see cref="QuietCountBeat_ProducesNoCountScene_AndTheCountStillCommits"/> already uses),
        /// which is every ordinary beat strictly BEFORE leg 0's own LegFinal correction plays. If
        /// the revealed total (<c>DebugRevealedPicked + DebugRevealedOpponent</c> — the exact pair
        /// <c>UpdateScorebug</c> itself reads, never re-derived) is already nonzero at any point in
        /// that window, the reveal happened on an ordinary beat, not only in <c>PlanFinal</c>'s
        /// stoppage-time correction — which is exactly the "two-step ending at the death" defect
        /// §1 measured (the corners arm shown 0-0 for 86% of a match that finished 5-1), restated
        /// as a pass/fail condition.</para>
        ///
        /// <para><b>What this does NOT prove:</b> that the FINAL revealed scoreline exactly equals
        /// the match's own final goal total (no equivalent of §4's count-total pin exists here for
        /// goals — this gate is about WHEN the reveal starts, not whether it is exact by the
        /// whistle); that the SCENE stays ticket-keyed (STEP 2 — a separate claim about the
        /// TEMPLATE; this test never reads <c>DebugSceneTemplate</c>); anything about cards (out
        /// of scope, §6); or which BEAT or minute the reveal lands on — there is no goal minute in
        /// the engine (<c>MatchStatLine</c> carries only final totals, never timing), so "before
        /// full time" is read off leg-liveness, never a clock value. C29: the executed case count
        /// is reported and the pin fails on zero.</para></summary>
        [UnityTest]
        public IEnumerator RevealedScoreline_OnACornersLeg_AdvancesBeforeFullTime()
        {
            yield return SeatOnACornersOnlyTicket(warmUpFrames: 4);
            TvSweatScreen screen = _statsScreen;
            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            Assert.IsNotNull(director, "RunDirector missing");
            Run run = director.Run;
            Assert.IsNotNull(run, "no run - SeatOnACornersOnlyTicket did not seat a live run");

            Leg cornersLeg = director.CurrentTicket.Legs[0];
            Assert.AreEqual(MarketKind.TotalCorners, cornersLeg.Selection.Kind,
                "SeatOnACornersOnlyTicket's own pick is CORNERS - re-point this pin if that ever "
                + "changes, never assume it silently");
            Assert.AreEqual(1, director.CurrentTicket.Legs.Count,
                "this pin needs a CORNERS-ONLY ticket, same precondition "
                + "QuietCountBeat_ProducesNoCountScene_AndTheCountStillCommits already asserts");

            int matchGoals = cornersLeg.Matchup.StatLine.HomeGoals + cornersLeg.Matchup.StatLine.AwayGoals;
            Assert.Greater(matchGoals, 0,
                "PRECONDITION: this gate needs a corners fixture WHOSE MATCH SCORES (spec §7 item "
                + "5) - STATS-COUNT-1's match has zero total goals on this seed, so nothing below "
                + "can be proven. This is a RE-POINT (a different seed or a forced-goal harness), "
                + "never a silent pass.");

            bool revealedBeforeFullTime = false;
            int revealedAtFirstAdvance = 0;
            int framesSampled = 0;
            int framesLegLive = 0;
            float start = Time.realtimeSinceStartup;
            const float maxSeconds = 60f;

            // Same failsafe shape as this file's other full-sweat pins: a hang is a FAILURE, never
            // a silent pass.
            while (run.Phase == Phase.Sweat)
            {
                if (Time.realtimeSinceStartup - start > maxSeconds)
                {
                    Assert.Fail($"the sweat never settled within {maxSeconds}s wall-clock (frames "
                        + $"sampled so far: {framesSampled})");
                    yield break;
                }
                framesSampled++;

                // DebugLegState(0) == "" is the SAME live-window instrument
                // QuietCountBeat_ProducesNoCountScene_AndTheCountStillCommits already uses: leg 0
                // is still live, i.e. this is an ORDINARY beat, strictly before its own LegFinal
                // correction plays.
                if (string.IsNullOrEmpty(screen.DebugLegState(0)))
                {
                    framesLegLive++;
                    int revealedTotal = screen.DebugRevealedPicked + screen.DebugRevealedOpponent;
                    if (!revealedBeforeFullTime && revealedTotal > 0)
                    {
                        revealedBeforeFullTime = true;
                        revealedAtFirstAdvance = revealedTotal;
                    }
                }

                yield return null;
            }

            // DIAGNOSTICS BEFORE THE ASSERTIONS, deliberately - same discipline as the sibling
            // quiet-count gate: everything needed to tell "the gate never fired" from "the gate
            // fired and this pin cannot see it" is printed first.
            UnityEngine.Debug.Log($"[SCORE-REVEAL-GATE] frames={framesSampled} framesLegLive={framesLegLive} "
                + $"matchGoals={matchGoals} revealedBeforeFullTime={revealedBeforeFullTime} "
                + $"revealedAtFirstAdvance={revealedAtFirstAdvance}");

            // C29: a gate reports its executed case count and fails on zero - a fixture that never
            // ran, or whose leg 0 was never actually observed live, has proven nothing.
            Assert.Greater(framesSampled, 0,
                "zero frames sampled before the run left Phase.Sweat - the fixture never actually "
                + "ran, so nothing below is proven (C29)");
            Assert.Greater(framesLegLive, 0,
                "leg 0 (corners) was never observed live across the whole sweat - cannot prove "
                + "anything about beats before its own final without at least one live frame (C29)");

            // THE GATE ITSELF (spec §7 item 5): the revealed scoreline must move on an ORDINARY
            // beat, independent of the ticket riding a corners market - never held back to reveal
            // only in one lump at the whistle (§1's measured "two-step ending... at the death").
            Assert.IsTrue(revealedBeforeFullTime,
                $"the revealed scoreline (Picked+Opponent) never advanced above zero while leg 0 "
                + $"was still live, across {framesLegLive} live frames, even though the match "
                + $"itself scores {matchGoals} goal(s) - the reveal is being withheld until the "
                + "whistle, which is §2's exact defect (the false 0-0 and the two-step ending)");
        }

        /// <summary>DD batch 95: seats the player on a ticket carrying EXACTLY ONE count leg — a
        /// TotalCorners leg, and no TotalCards leg — the exact row set (GOALS+CORNERS, CARDS ABSENT)
        /// the §8.8 closing ruling's own binary criterion names ("on a corners-only ticket there is
        /// NOTHING BENEATH CORNERS"). Same "read the selection off the board, never construct it"
        /// discipline as <see cref="SeatOnAMultiCountTicket"/>, and the SAME seed T100's own capture
        /// (TvSweatCaptureHarness.cs, <c>Capture_StatsPanel_WithAPopulatedCountRow</c>) already proved
        /// offers a TotalCorners market — reused here rather than gambling on an unproven one.</summary>
        private IEnumerator SeatOnACornersOnlyTicket(int warmUpFrames = 30)
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

            // "STATS-COUNT-1" — T100's own known-good seed (TvSweatCaptureHarness.cs), documented
            // there to offer a TotalCorners market on this slate.
            director.StartNewRun("STATS-COUNT-1");
            Run run = director.Run;
            Assert.AreEqual(Phase.Betting, run.Phase, "a fresh run opens in Betting");

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
                "no matchup on this slate offers TotalCorners - this is a re-seed, never a reason to "
                + "invent a selection the board did not offer");

            const double Stake = 25.0;
            run.PlaceTicket(new List<Pick> { new Pick(cornersMatchup, cornersSelection) }, Stake);
            director.LockRound();

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");
            yield return WaitUntil(() => screen.DebugSeatedDeltaTime > 0f, 20f,
                "the screen never became seated-and-running");
            // T115: the 30 is now a DEFAULT, not a constant. At this fixture's 0.0001 fast-forward a
            // whole sweat runs only a few dozen frames, so 30 warm-up frames can consume the very
            // beats a pin means to watch — measured, on the multi-count sibling, as a loop that
            // sampled ZERO frames because its leg had already decided. Callers that need to observe
            // early beats pass a smaller number; every existing caller keeps 30 by defaulting.
            for (int i = 0; i < warmUpFrames; i++) yield return null; // let the first beat render a scorebug

            _statsScreen = screen;
        }

        /// <summary>DD batch 95 closing ruling, THE BINARY CRITERION: "on a corners-only ticket there
        /// is NOTHING BENEATH CORNERS — no empty slot, no reserved space; the panel's bottom inset
        /// sits directly under the CORNERS row." Asserted against LIVE rects throughout, never a
        /// remembered constant, so a future ruling on pad/pitch/row height moves this pin's own
        /// expectation for free rather than going red on a ruling.</summary>
        [UnityTest]
        public IEnumerator Stats_panel_unbought_row_occupies_no_height_on_a_corners_only_ticket()
        {
            yield return SeatOnACornersOnlyTicket();
            TvSweatScreen screen = _statsScreen;
            screen.ForceStatsPanel(true);
            yield return null;

            var panel = screen.DebugStatsPanel as RectTransform;
            Assert.IsNotNull(panel, "no StatsPanel element");

            Assert.IsNotNull(screen.DebugStatsRow(0), "GOALS must be PRESENT — it is unconditional");
            StringAssert.StartsWith("CORNERS|", screen.DebugStatsRow(1),
                "a corners-only ticket must show the CORNERS row");
            Assert.IsNull(screen.DebugStatsRow(2),
                "a corners-only ticket carries no TotalCards leg — CARDS must be ABSENT (DD batch 95: "
                + "'an unbought row is not a silent row, it is NO row'), not an empty reserved slot. "
                + "Got: " + screen.DebugStatsRow(2));

            // Discovered LIVE off the panel's own ACTIVE children only — same instrument as
            // Stats_panel_is_sized_exactly_to_its_content (GetComponentsInChildren<RectTransform>
            // (true), so it still SEES inactive slots too), filtered to activeInHierarchy here because
            // this pin's specific claim is that an ABSENT row contributes NOTHING to the bound.
            float activeBottom = float.MinValue;
            int activeLabelCount = 0;
            foreach (RectTransform rt in panel.GetComponentsInChildren<RectTransform>(true))
            {
                bool isRowSlot = rt.name.StartsWith("StatsLabel") || rt.name.StartsWith("StatsA")
                    || rt.name.StartsWith("StatsB");
                if (!isRowSlot || !rt.gameObject.activeInHierarchy) continue;
                if (rt.name.StartsWith("StatsLabel")) activeLabelCount++;
                activeBottom = Mathf.Max(activeBottom, -rt.anchoredPosition.y + rt.rect.height);
            }
            Assert.AreEqual(2, activeLabelCount,
                $"a corners-only ticket must show exactly 2 ACTIVE rows (GOALS, CORNERS) — found "
                + $"{activeLabelCount}. DD batch 93's row set gives GOALS+CORNERS here — TWO rows, "
                + "not the closing ruling's own 'ONE ROW' prose; built to the ruling's BINARY "
                + "criterion (nothing beneath CORNERS) rather than to that count, per the DD brief.");

            var title = FindChildComponent<RectTransform>(screen, "StatsTitle");
            Assert.IsNotNull(title, "no StatsTitle element — re-point this pin, never delete it");
            float topInset = -title.anchoredPosition.y;
            float bottomInset = panel.rect.height - activeBottom;

            const float tol = 0.5f;
            Assert.AreEqual(topInset, bottomInset, tol,
                $"THE BINARY CRITERION: nothing beneath CORNERS. The panel's bottom inset "
                + $"({bottomInset:0.0}px) must equal its top inset ({topInset:0.0}px) — measured "
                + "against the LIVE active rows, never a remembered constant — so the panel's bottom "
                + "sits directly under CORNERS with no reserved space for the CARDS row this ticket "
                + "never bought.");

            // The slot that WOULD have carried CARDS must not be the thing quietly extending the
            // panel: it is discoverable (the GameObject still exists, collapsed) but must be INACTIVE.
            var cardsLabel = FindChildComponent<RectTransform>(screen, "StatsLabel2");
            Assert.IsNotNull(cardsLabel, "no StatsLabel2 element — re-point this pin, never delete it");
            Assert.IsFalse(cardsLabel.gameObject.activeInHierarchy,
                "the row slot beneath CORNERS must be INACTIVE on a corners-only ticket — an "
                + "active-but-blank slot would still be occupying space by DD batch 95's own ruling "
                + "('an unbought row is not a silent row, it is NO row')");
        }

        /// <summary>DD batch 95, "ADD or extend a pin that a moneyline ticket (no count legs) yields a
        /// panel shorter still": a BUILT RELATIONSHIP, not a pair of remembered constants — the LIVE
        /// panel height a moneyline ticket (one row: GOALS) actually builds must be strictly less than
        /// the LIVE panel height a corners-only ticket (two rows: GOALS+CORNERS) actually builds, so a
        /// future ruling on pad/pitch/row height moves both sides of this comparison together and the
        /// relationship still holds. Proves the panel's height truly FOLLOWS ITS ROWS rather than
        /// coincidentally landing on the same number across different ticket row sets.</summary>
        [UnityTest]
        public IEnumerator Stats_panel_a_moneyline_ticket_yields_a_panel_shorter_still()
        {
            yield return SeatOnACornersOnlyTicket();
            TvSweatScreen cornersScreen = _statsScreen;
            cornersScreen.ForceStatsPanel(true);
            yield return null;
            var cornersPanel = cornersScreen.DebugStatsPanel as RectTransform;
            Assert.IsNotNull(cornersPanel, "no StatsPanel element");
            Assert.IsNotNull(cornersScreen.DebugStatsRow(1),
                "PRECONDITION: the corners-only ticket must show a CORNERS row — nothing below is "
                + "proven without this");
            float cornersOnlyHeight = cornersPanel.rect.height;

            yield return OpenStatsPanelOnALiveLeg(); // fresh room + a moneyline ticket
            TvSweatScreen moneyScreen = _statsScreen;
            var moneyPanel = moneyScreen.DebugStatsPanel as RectTransform;
            Assert.IsNotNull(moneyPanel, "no StatsPanel element");
            Assert.IsNull(moneyScreen.DebugStatsRow(1),
                "PRECONDITION: a moneyline ticket must show no CORNERS row — nothing below is proven "
                + "without this");
            float moneylineHeight = moneyPanel.rect.height;

            Assert.Less(moneylineHeight, cornersOnlyHeight,
                $"a moneyline ticket (one row: GOALS) must yield a SHORTER panel than a corners-only "
                + $"ticket (two rows: GOALS+CORNERS) — got moneyline {moneylineHeight:0.0}px, "
                + $"corners-only {cornersOnlyHeight:0.0}px. The panel's height must FOLLOW ITS ROWS, "
                + "not stay fixed across different ticket row sets.");
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

            // ---- (1) TITLE — the one constant, DD batch 94: "COUNTS" (TvSweatScreen.cs:3875).
            Log("StatsTitle", title, "COUNTS");

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

        /// <summary>PIN for the batchmode-cwd trap (tv-sweat-refinement lane). <c>TvSweatCaptureHarness</c>'s
        /// output directory used to be <c>Path.Combine(Directory.GetCurrentDirectory(), ...)</c> —
        /// LAUNCHER-DEPENDENT, because Unity's batchmode cwd happens to be the project path
        /// (unity/SBR) today but is not guaranteed to be. This lane already paid for that once: a
        /// poll watched &lt;repo&gt;/artifacts and reported files=0 for a run that was writing frames
        /// the whole time, one level down at unity/SBR/artifacts/tv-sweat-capture. The harness now
        /// anchors to <see cref="Application.dataPath"/> instead, which does not move with cwd.
        ///
        /// <para>Not in <c>TvSweatCaptureHarness.cs</c> itself — every entry point in that class is
        /// either <c>[Explicit]</c> or disposable evidence infrastructure by its own class doc
        /// ("DELETE THIS FILE"), so a pin placed there would not reliably gate. This reads the
        /// harness's OWN resolved value via its <c>internal</c> <see cref="TvSweatCaptureHarness.OutputDir"/>
        /// getter — never a second copy of the derivation — so a regression in the harness's formula
        /// fails HERE rather than passing a self-referential check.</para>
        ///
        /// <para><c>[Test]</c>, not <c>[UnityTest]</c>: fully synchronous, so the temporary
        /// <c>Directory.SetCurrentDirectory</c> swap below has no yield point where another
        /// coroutine could observe the mutated cwd, and it is restored in <c>finally</c> even if an
        /// assertion throws.</para></summary>
        [Test]
        public void TvSweatCaptureHarness_output_directory_is_anchored_to_dataPath_not_cwd()
        {
            // Independently derived expected path — NOT the harness's own "Application.dataPath +
            // .." combine, so an off-by-one in the harness's relative-segment count would still be
            // caught here rather than agreeing with itself.
            string sbrProjectRoot = Directory.GetParent(Application.dataPath).FullName;
            string expected = Path.GetFullPath(Path.Combine(sbrProjectRoot, "artifacts", "tv-sweat-capture"));

            string resolved = TvSweatCaptureHarness.OutputDir;

            Assert.AreEqual(expected, resolved,
                "TvSweatCaptureHarness.OutputDir drifted from Application.dataPath's parent + "
                + "artifacts/tv-sweat-capture — the deliberately-claimed evidence location where "
                + "1,300+ frames already live.");

            Assert.IsTrue(resolved.StartsWith(sbrProjectRoot, StringComparison.Ordinal),
                $"'{resolved}' is not rooted at Application.dataPath's parent ('{sbrProjectRoot}')");

            string normalized = resolved.Replace('\\', '/');
            Assert.IsTrue(normalized.EndsWith("unity/SBR/artifacts/tv-sweat-capture", StringComparison.Ordinal),
                $"'{resolved}' does not end with the claimed unity/SBR/artifacts/tv-sweat-capture segments");

            // THE REGRESSION THIS PIN EXISTS FOR: OutputDir must never read
            // Directory.GetCurrentDirectory() again. Proven directly, not just inferred from the
            // dataPath-rooted check above — flip cwd to an unrelated directory and confirm the
            // resolved value does not move.
            string originalCwd = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(Path.GetTempPath());
                Assert.AreEqual(expected, TvSweatCaptureHarness.OutputDir,
                    "OutputDir moved when Directory.GetCurrentDirectory() changed — it must be "
                    + "anchored to Application.dataPath only. Unity's batchmode cwd happens to be the "
                    + "project path today; a launcher that starts Unity from elsewhere must not "
                    + "silently redirect where frames land (this lane already lost a poll to exactly "
                    + "that).");
            }
            finally
            {
                Directory.SetCurrentDirectory(originalCwd);
            }
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
