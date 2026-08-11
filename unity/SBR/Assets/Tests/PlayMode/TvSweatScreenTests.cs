using System;
using System.Collections;
using System.Collections.Generic;
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

            screen.TimeScaleOverride = 0.2f;
            // AnimateCashOut's real duration is cashOutTickDuration * TimeScaleOverride, so the
            // 0.2f above SHRINKS this fivefold: the old 4f bought a 0.8s window, not a 4s one.
            screen.cashOutTickDuration = 30f;
            couch.transitionDuration = 0.01f;

            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");
            Run run = director.Run;
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            director.LockRound();

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            // Poll the state machine, not the animation flag. Waiting on DebugCashOutAnimating
            // alone was a race by construction: a tween starts only when a NEW offer differs from
            // the shown figure by 0.005 or more, so the old wait depended on the simulation moving
            // the price inside its 20s window — unpinned generated content, which is exactly the
            // thing that fails one run in N and passes on re-run. Wait instead for the DURABLE
            // precondition (the same gate SetCashOutOffer's caller applies, plus the shown-once
            // latch), then drive the displacement rather than hoping for it.
            yield return WaitUntil(() =>
                director.CurrentSession != null && !director.CurrentSession.IsComplete
                && screen.EventsEmitted >= 1
                && director.CurrentSession.CashOutOffer().HasValue
                && screen.DebugHasCashOutShown,
                20f, "never reached a live, rendered cash-out offer to animate");

            Assert.IsTrue(screen.ForceCashOutDisplacement(CashOutDisplacement),
                "a cash-out figure was shown but could not be displaced");

            yield return WaitUntil(() => screen.DebugCashOutAnimating, 5f,
                "the displaced cash-out figure did not produce a tween");

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

            Text flavor = FindChildComponent<Text>(screen, "Flavor");
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

            screen.TimeScaleOverride = 0.2f;
            // AnimateCashOut's real duration is cashOutTickDuration * TimeScaleOverride, so the
            // 0.2f above SHRINKS this fivefold: the old 4f bought a 0.8s window, not a 4s one.
            screen.cashOutTickDuration = 30f;
            couch.transitionDuration = 0.01f;

            yield return WaitUntil(() => director.Run != null, 10f, "director never started a run");
            Run run = director.Run;
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            director.LockRound();

            couch.OnInteract(null);
            yield return WaitUntil(() => SitSpot.Active != null, 10f, "player never sat down");

            // Poll the state machine, not the animation flag. Waiting on DebugCashOutAnimating
            // alone was a race by construction: a tween starts only when a NEW offer differs from
            // the shown figure by 0.005 or more, so the old wait depended on the simulation moving
            // the price inside its 20s window — unpinned generated content, which is exactly the
            // thing that fails one run in N and passes on re-run. Wait instead for the DURABLE
            // precondition (the same gate SetCashOutOffer's caller applies, plus the shown-once
            // latch), then drive the displacement rather than hoping for it.
            yield return WaitUntil(() =>
                director.CurrentSession != null && !director.CurrentSession.IsComplete
                && screen.EventsEmitted >= 1
                && director.CurrentSession.CashOutOffer().HasValue
                && screen.DebugHasCashOutShown,
                20f, "never reached a live, rendered cash-out offer to animate");

            Assert.IsTrue(screen.ForceCashOutDisplacement(CashOutDisplacement),
                "a cash-out figure was shown but could not be displaced");

            yield return WaitUntil(() => screen.DebugCashOutAnimating, 5f,
                "the displaced cash-out figure did not produce a tween");

            Text cashOut = FindChildComponent<Text>(screen, "CashOut");
            Assert.IsNotNull(cashOut, "CashOut text not found - canvas layout changed?");
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
            Text figure = FindChildComponent<Text>(screen, "CashOut");
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
