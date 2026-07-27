using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SBR.Engine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SBR.Game
{
    public enum RevealedLegState { Pending, Live, Won, Lost, Voided }

    /// <summary>Read-only presentation data copied from the TV's own visible chrome.</summary>
    public sealed class RevealedLeg
    {
        public int Index { get; internal set; }
        public string TeamName { get; internal set; }
        public string MarketLabel { get; internal set; }
        public string AmericanOdds { get; internal set; }
        public uint TeamColor { get; internal set; }
        public RevealedLegState State { get; internal set; }
    }

    public sealed class RevealedTicket
    {
        public int Index { get; internal set; }
        public double Stake { get; internal set; }
        public double PotentialPayout { get; internal set; }
        public RevealedTicketState State { get; internal set; }
        public IReadOnlyList<RevealedLeg> Legs { get; internal set; }
    }

    public enum RevealedTicketState { Riding, Won, Lost, CashedOut }

    /// <summary>
    /// The TV-owned causal mirror. Laptop MY BETS may read this object, but it cannot access the sweat
    /// session or its live offer. Values advance only at the same reveal points as the TV chrome.
    /// </summary>
    public sealed class RevealedView
    {
        private readonly List<RevealedTicket> _tickets = new List<RevealedTicket>();

        /// <summary>Structural changes only (tickets, legs, states, reset/clear) — the OS
        /// rebuilds its UI on this. Fast display values live on DisplayRevision.</summary>
        public int Revision { get; internal set; }

        /// <summary>Per-frame display values (clock, prob, score, suspension). Consumers
        /// update cached labels in place — a ticking minute must never rebuild a canvas
        /// (Sol, F_0.3.0 performance finding).</summary>
        public int DisplayRevision { get; internal set; }
        public bool HasTicket { get; internal set; }
        public int CurrentTicketIndex { get; internal set; }
        public int TicketCount { get; internal set; }
        public float WinProbability { get; internal set; }
        public string ScoreText { get; internal set; } = string.Empty;
        public string ClockText { get; internal set; } = string.Empty;
        public bool MarketSuspended { get; internal set; }
        public IReadOnlyList<RevealedTicket> Tickets => _tickets;

        internal void Reset(Run run, Ticket current, int currentIndex)
        {
            var previous = new List<RevealedTicket>(_tickets);
            _tickets.Clear();
            if (run != null)
            {
                for (int i = 0; i < run.Tickets.Count; i++)
                    _tickets.Add(i < currentIndex && i < previous.Count
                        ? previous[i] : CopyTicket(run.Tickets[i], i));
            }
            CurrentTicketIndex = currentIndex;
            TicketCount = _tickets.Count;
            HasTicket = current != null && current.Legs.Count > 0;
            WinProbability = HasTicket ? (float)current.Legs[0].TrueProb : 0f;
            ScoreText = string.Empty;
            ClockText = "PRE";
            MarketSuspended = false;
            Revision++;
        }

        internal void BeginLeg(int legIndex, Leg leg)
        {
            if (!HasTicket || leg == null) return;
            RevealedTicket ticket = CurrentTicket;
            if (ticket == null || legIndex < 0 || legIndex >= ticket.Legs.Count) return;
            ticket.Legs[legIndex].State = RevealedLegState.Live;
            Revision++;
        }

        internal void SetProbability(float probability)
        {
            if (!HasTicket) return;
            WinProbability = probability;
            DisplayRevision++;
        }

        internal void SetClock(string clock)
        {
            if (!HasTicket) return;
            ClockText = clock ?? string.Empty;
            DisplayRevision++;
        }

        internal void SetScore(string score)
        {
            if (!HasTicket) return;
            ScoreText = score ?? string.Empty;
            DisplayRevision++;
        }

        internal void SetMarketSuspended(bool suspended)
        {
            if (!HasTicket) return;
            MarketSuspended = suspended;
            DisplayRevision++;
        }

        internal void ResolveLeg(int legIndex, LegGrade grade)
        {
            RevealedTicket ticket = CurrentTicket;
            if (ticket == null || legIndex < 0 || legIndex >= ticket.Legs.Count) return;
            ticket.Legs[legIndex].State = grade == LegGrade.Won ? RevealedLegState.Won
                : grade == LegGrade.Lost ? RevealedLegState.Lost : RevealedLegState.Voided;
            if (grade == LegGrade.Lost) ticket.State = RevealedTicketState.Lost;
            else
            {
                bool allResolved = true;
                for (int i = 0; i < ticket.Legs.Count; i++)
                    if (ticket.Legs[i].State == RevealedLegState.Pending || ticket.Legs[i].State == RevealedLegState.Live)
                        allResolved = false;
                if (allResolved) ticket.State = RevealedTicketState.Won;
            }
            Revision++;
        }

        /// <summary>The sweat is over and the TV moved on — the mirror empties with it
        /// (a stale round under a live banner is a lie; Sol, F_0.3.0 finding 2).</summary>
        internal void Clear()
        {
            _tickets.Clear();
            HasTicket = false;
            TicketCount = 0;
            CurrentTicketIndex = 0;
            WinProbability = 0f;
            ScoreText = string.Empty;
            ClockText = string.Empty;
            MarketSuspended = false;
            Revision++;
        }

        internal void MarkCashedOut()
        {
            RevealedTicket ticket = CurrentTicket;
            if (ticket == null) return;
            ticket.State = RevealedTicketState.CashedOut;
            Revision++;
        }

        private RevealedTicket CurrentTicket
            => CurrentTicketIndex >= 0 && CurrentTicketIndex < _tickets.Count ? _tickets[CurrentTicketIndex] : null;

        private static RevealedTicket CopyTicket(Ticket source, int index)
        {
            var legs = new List<RevealedLeg>(source.Legs.Count);
            foreach (Leg leg in source.Legs)
            {
                (uint home, uint away) = TheaterPalette.TeamColors(leg.Matchup.Home.Name, leg.Matchup.Away.Name);
                bool pickedHome = SweatFlavor.PickedHomeForPresentation(leg);
                legs.Add(new RevealedLeg
                {
                    Index = legs.Count,
                    TeamName = leg.Selection.Kind == MarketKind.Moneyline
                        ? SweatFlavor.Short(pickedHome ? leg.Matchup.Home.Name : leg.Matchup.Away.Name).ToUpperInvariant()
                        : leg.DisplayLabel,
                    MarketLabel = leg.DisplayLabel,
                    AmericanOdds = OddsFormat.American(leg.OfferedOdds),
                    TeamColor = pickedHome ? home : away,
                    State = RevealedLegState.Pending
                });
            }
            return new RevealedTicket
            {
                Index = index,
                Stake = source.Stake,
                PotentialPayout = source.PotentialPayout,
                State = RevealedTicketState.Riding,
                Legs = legs
            };
        }
    }

    /// <summary>
    /// The TV plays the sweat, live from the real engine (M3's emotional core; M4 wires it to the real
    /// round). A world-space UGUI canvas sits on the TV screen inset (0.98 x 0.55) in front of the
    /// emissive quad; a coroutine walks the locked round's sweats SERIALLY (M4 grill decision) - a ~2s
    /// auto-advance ticket card between sweats, a settle card after the last (target met / the bookie
    /// floats you), then the SHOP OPEN nudge while the laptop glows. Events step ONLY while the player
    /// is seated (design/04 - sitting starts/resumes, standing pauses mid-event with the offer frozen).
    /// Outside the sweat the TV idles per phase: PLACE YOUR BETS during Betting, SHOP OPEN during Shop,
    /// and the run verdict card on RunWon/RunLost with the room light dropping cold (M4 grill decision).
    ///
    /// Beats are all code-driven: GREEN = green flood + emissive spike, DEAD = static then the red DEAD
    /// line + the screen dropping darker, ticket-dead = a dim-to-black beat, cash-out = a gold flood
    /// with the amount big. The TvLight makes the room the reaction shot.
    ///
    /// Palette is law (design/08): green = money-good only, red = money-bad only, gold = cash-out.
    /// Pacing ports the console's table into <see cref="PacingFor"/> with serialized dials; no engine RNG
    /// is consumed by presentation (only MoveNext / CashOut* are called - everything is baked at lock).
    /// </summary>
    public sealed class TvSweatScreen : MonoBehaviour
    {
        [Header("Wiring (set by GrayboxRoomBuilder)")]
        public RunDirector director;
        public Renderer emissiveScreen;      // the TV quad behind the canvas - its phosphor glow
        public TvLight tvLight;
        public InputActionAsset actions;     // for the Interact (E) = cash-out accept

        [Header("Layout")]
        public Vector2 screenWorldSize = new Vector2(0.98f, 0.55f);
        [Tooltip("Metres the canvas floats in front of the emissive quad (toward the couch).")]
        public float canvasOffset = 0.006f;
        [Tooltip("Reference canvas width in px; height follows the screen aspect.")]
        public int referencePixelsWide = 980;

        [Header("Pacing dials (ms, ported from the console)")]
        public float calmMs = 450f;
        public float swingMs = 650f;
        public float leadChangeMs = 800f;
        public float nearMissMs = 1000f;
        public float decisiveMs = 1200f;
        [Tooltip("Extra suspense held on the beat right before a leg's final whistle.")]
        public float preFinalExtraMs = 300f;
        [Tooltip("Everything on the ticket's final leg is slowed by this factor.")]
        public float finalLegMultiplier = 1.5f;
        [Tooltip("Test/debug hook: multiplies every pacing delay and beat duration. 1 = ship pacing, " +
                 "tiny = fast-forward, 0 = as fast as the frame rate allows.")]
        public float TimeScaleOverride = 1f;

        [Header("Beat dials (seconds)")]
        [Tooltip("The auto-advance ticket card between sweats (TICKET i/n, legs, stake to win).")]
        public float ticketCardDuration = 2.0f;
        [Tooltip("The settle card after the round's last sweat (target met / the bookie floats you).")]
        public float settleCardDuration = 3.0f;
        public float greenFloodDuration = 0.3f;
        public float deadStaticDuration = 0.6f;
        public int staticRegens = 5;
        public float deadLineDuration = 0.7f;
        public float ticketDeadDimDuration = 0.9f;
        [Tooltip("Silence after a dead ticket dims before the consolation line speaks.")]
        public float ticketDeadSilenceDuration = 0.8f;
        public float ticketDeadConsolationDuration = 1.0f;
        public float cashOutFloodDuration = 0.8f;
        public float winFloodDuration = 1.0f;
        public float cashOutTickDuration = 0.4f;
        public double cashOutRoundMultiple = 100.0;
        public float winTallyDuration = 1.2f;
        public float winConfettiDuration = 2.0f;
        public int winConfettiCount = 40;

        [Header("Feel dials")]
        public float breathAmplitude = 0.06f;
        public float breathSlowHz = 0.7f;
        public float breathFastHz = 2.6f;
        [Tooltip("Idle phosphor emission flicker, fraction of the emissive quad's idle emission.")]
        public float idleEmissionFlicker = 0.05f;
        public float emissionDecay = 3.2f;
        [Range(0f, 1f)] public float scanlineAlpha = 0.15f;

        [Header("Audio v0 (procedural, diegetic)")]
        [Range(0f, 1f)] public float masterVolume = 0.5f;
        [Range(0f, 1f)] public float crowdVolume = 0.6f;
        [Range(0f, 1f)] public float stingVolume = 0.8f;

        [Header("Theater (F_0.2.0 — the match theater stage)")]
        [Tooltip("The match theater stage (M-T2/T3). Off = the pre-theater text-ticker layout, kept " +
                 "as the A/B fallback through M-T4 per the plan's reversibility clause.")]
        public bool theaterEnabled = true;
        public Color pitchLineColor = new Color(0.85f, 0.92f, 0.95f, 0.50f);
        public Color pitchBgColor = new Color(0.012f, 0.016f, 0.022f, 0.95f);
        [Tooltip("Scene-class → seconds (M-T3). The duration-acceptance test pins these bands.")]
        public SweatPacer pacer = new SweatPacer();
        [Tooltip("Idle gap between beat scenes, ms (the ≤1s filler law). Doubles as the " +
                 "guaranteed open-market window per beat (playtest #15).")]
        public float interSceneGapMs = 900f;

        [Header("Palette (design/08)")]
        [ColorUsage(false, true)] public Color phosphorGreen = new Color(0.20f, 1.15f, 0.40f);
        [ColorUsage(false, true)] public Color hotRed = new Color(1.10f, 0.16f, 0.13f);
        [ColorUsage(false, true)] public Color gold = new Color(1.15f, 0.82f, 0.18f);
        public Color chromeCyan = new Color(0.62f, 0.86f, 0.96f, 0.95f);
        public Color flavorColor = new Color(0.90f, 0.95f, 0.98f, 1f);
        public Color screenBg = new Color(0.015f, 0.03f, 0.022f, 0.86f);
        public Color barBgColor = new Color(0.05f, 0.08f, 0.06f, 0.92f);

        // ---- public test/debug surface ----
        public int EventsEmitted => _eventsEmitted;
        public bool SweatComplete => _session != null && _session.IsComplete;
        public RevealedView RevealedView { get; } = new RevealedView();
        /// <summary>Test/debug hook: force the seated state (simulates sitting / looking away) without the
        /// couch. Normal play drives this through SitSpot.SeatedChanged.</summary>
        public void ForceSeated(bool seated) => SetSeated(seated);
        /// <summary>Test/debug hook (TVS-H01 regression): true while the cash-out amount is mid-tween
        /// (AnimateCashOut running). _cashOutAnimation is otherwise unobservable from outside the
        /// sweat, and this is the exact condition CanAcceptCashOutNow also refuses.</summary>
        public bool DebugCashOutAnimating => _cashOutAnimation != null;

        // ---- state ----
        private bool _seated;
        // TVS-H02: accumulates real time only while seated. Every TvSweatScreen-owned timer,
        // coroutine, and per-frame animator reads dt through SeatedDeltaTime (or samples this clock
        // in place of Time.time) instead of Unity's clock directly, so one gate freezes all of them
        // instead of 21 scattered `if (!_seated)` checks, and resuming never has to catch up.
        // TheaterStage freezes independently through its own SetFrozen(!_seated) gate, propagated
        // the instant _seated changes by SetSeated() below (see that method for why this must not
        // wait for this object's own Update()).
        private float _seatedClock;
        private SweatSession _session;
        private Ticket _ticket;
        private string _idleKey; // last idle/verdict render, so per-phase screens paint once
        private int _eventsEmitted;
        private int _flavorLegSeen = -1;
        private double _prevProb;
        private float _probTarget;
        private float _probShown;
        private float _flavorScale = 1f;
        private double _lastCashOutAmount;
        private double _cashOutShown;
        private bool _hasCashOutShown;
        private Coroutine _cashOutAnimation;
        private float _cashOutScale = 1f;
        private float _cashOutFlash;
        private int _cashOutRoundShown;
        private float _barPunch = 1f;

        // ---- input ----
        private InputAction _interact;

        // ---- emission (phosphor) ----
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private MaterialPropertyBlock _emissBlock;
        private Color _emissIdle;
        private Color _emissRest;
        private Color _emissFlash;
        private float _emissFlash01;
        private float _emissSeed;

        // ---- UI ----
        private Font _font;
        private float _innerWidth;
        private float _barHeight;
        private int _resolvedThrough; // legs below this index are PRESENTED as resolved (not engine truth)
        private Text _tMatchup, _tRecords, _tLeg, _tClock, _tFlavor, _tWinPct, _tCashOut, _tChrome, _tAttract, _tBigAmount, _tSlipStrip, _tConsolation;
        private Image _backing, _barBg, _barFill, _greenFlood, _goldFlood, _dimOverlay;
        private RawImage _staticNoise, _scanlines;
        private Texture2D _noiseTex;

        // ---- theater (F_0.2.0) ----
        private TheaterStage _stage;
        private MomentumTape _tape;
        private Transform _canvasRoot;
        private float _canvasWidth;
        private float _canvasHeight;
        private readonly SweatPresentationModel _presModel = new SweatPresentationModel();
        private readonly ScoreLedger _ledger = new ScoreLedger();
        private CountLedger _countLedger;
        private TheaterChoreographer _choreo;
        private int _stageLeg = -1;
        private int _stageBeatCount;
        private bool _lastBeatUp;
        private double _lastBeatDelta;
        // Causal reveal (M-T3.1): the beat's chrome is computed at MoveNext but LANDS at the
        // scene's payoff moment — the number must never spoil the goal.
        private float _pendingProb;
        private string _pendingFlavor;
        private bool _finalSequenceActive;
        private int _stoppageGoalCount;
        // The continuously ticking match clock (playtest #13): minutes advance 1' 2' 3'
        // through each scene toward the beat's baked minute — constant time flow the player
        // can read, still causal (the target is position-derived, never outcome-derived).
        private float _clockShownMin;   // fractional minute currently displayed
        private float _clockTargetMin;  // the minute this run arrives at
        private float _clockRate;       // minutes per realtime second for the active run
        private bool _clockTicking;
        // Market suspend (M-T3.1, Allen's ruling): the engine reprices at MoveNext, so while a
        // scene plays the market is SUSPENDED — no stale-price accepts, no spoiler price.
        private bool _marketSuspended;
        private double _pendingBeatDelta;
        private Color _pendingBeatBeneficiary;
        private bool _pendingTapeBeat;
        private TvAudioDirector _audio;
        private float _audioUrgency;

        private struct ConfettiPiece
        {
            public RectTransform Rect;
            public Vector2 Velocity;
            public float Spin;
        }

        private readonly List<ConfettiPiece> _confetti = new List<ConfettiPiece>();
        private System.Random _confettiRandom = new System.Random(0x534252);

        // =====================================================================================

        private void Awake()
        {
            _font = LoadFont();
            _choreo = new TheaterChoreographer(pacer);
            _emissBlock = new MaterialPropertyBlock();
            _emissSeed = UnityEngine.Random.value * 100f;

            _emissIdle = emissiveScreen != null && emissiveScreen.sharedMaterial != null
                ? emissiveScreen.sharedMaterial.GetColor(EmissionColorId)
                : new Color(0.010f, 0.045f, 0.020f);
            _emissRest = _emissIdle;
            _emissFlash = _emissIdle;

            BuildCanvas();
        }

        private void OnEnable()
        {
            ResolveInput();
            SitSpot.SeatedChanged += OnSeatedChanged;
            SitSpot.InteractStandSuppressed = CashOutLive; // E is cash-out while an offer shows, not stand
            SetSeated(SitSpot.Active != null);
            _audio?.Show(true);
            StartCoroutine(RunChannel());
        }

        private void OnDisable()
        {
            SitSpot.SeatedChanged -= OnSeatedChanged;
            if (SitSpot.InteractStandSuppressed == (Func<bool>)CashOutLive)
                SitSpot.InteractStandSuppressed = null;
            StopAllCoroutines();
            CleanupConfetti();
            _audio?.Show(false);
        }

        private void OnSeatedChanged(bool seated) => SetSeated(seated);

        /// <summary>TVS-H02 race fix: the single place _seated is ever assigned. Propagates the
        /// freeze to TheaterStage SYNCHRONOUSLY, the instant seating changes, instead of waiting for
        /// this object's own next Update() to reach `_stage.SetFrozen(!_seated)`. TvSweatScreen and
        /// TheaterStage are two independent MonoBehaviours with no guaranteed relative Update() order;
        /// if TheaterStage.Update() happened to run before TvSweatScreen.Update() in the frame standing
        /// occurred, it would step once more on THIS frame's stale (pre-stand) frozen flag - long enough
        /// to fire a scene's reveal (a new cash-out price) or complete the scene outright and unblock
        /// the next beat's SuspendMarket(), overwriting the frozen cash-out text one tick after the
        /// player stood (TVS-H02, the `MARKET SUSPENDED` flake in BUG-LEDGER.md §4C.4). Setting the
        /// stage's frozen flag here, at the moment of change rather than at the next Update(), means
        /// every Update() this frame or later - regardless of which script's Update() runs first -
        /// observes a consistent frozen state, closing that window.</summary>
        private void SetSeated(bool seated)
        {
            _seated = seated;
            if (_stage != null) _stage.SetFrozen(!_seated);
        }

        private void ResolveInput()
        {
            if (actions == null) return;
            InputActionMap map = actions.FindActionMap("Player", throwIfNotFound: false);
            if (map == null) return;
            _interact = map.FindAction("Interact");
            map.Enable();
        }

        /// <summary>An offer is showing that E should accept (rather than stand the player up).
        /// Must agree exactly with TryCashOut's acceptance gate (TVS-H01) — both read
        /// CanAcceptCashOutNow so a future edit cannot let the two drift apart again.</summary>
        private bool CashOutLive() => CanAcceptCashOutNow();

        /// <summary>The single truth for "is there a cash-out offer Interact may legally accept right
        /// now" (TVS-H01; VISUAL-DESIGN.md §8.5). Open only when seated, the session is live, at
        /// least one event has revealed, the market is not suspended, the shown price is not mid-tween,
        /// and the engine is actually quoting an offer. Both the stand-suppression contract
        /// (CashOutLive, wired to SitSpot.InteractStandSuppressed) and TryCashOut consult this one
        /// predicate, so a suspended or updating offer can never reserve Interact without also being
        /// acceptable — and a legal open offer always reserves it.</summary>
        private bool CanAcceptCashOutNow()
            => _seated
            && _session != null && !_session.IsComplete
            && _eventsEmitted >= 1
            && !_marketSuspended
            && _cashOutAnimation == null
            && _session.CashOutOffer().HasValue;

        // ---------------------------------------------------------------- channel loop

        private IEnumerator RunChannel()
        {
            while (true)
            {
                if (director == null || director.Run == null) { yield return null; continue; }

                switch (director.Run.Phase)
                {
                    case Phase.Sweat:
                        yield return PresentRound();
                        break;

                    case Phase.Betting:
                        RenderIdle("betting", "PLACE YOUR BETS",
                            "the book is open on the laptop", moneyIdle: true);
                        yield return null;
                        break;

                    case Phase.Shop:
                        RenderIdle("shop", "SHOP OPEN",
                            "gear up at the laptop, then the next round", moneyIdle: true);
                        yield return null;
                        break;

                    case Phase.RunWon:
                    case Phase.RunLost:
                        RenderRunOver();
                        yield return null;
                        break;

                    default: // Settlement is transient (zero-ticket locks settle inside the director)
                        yield return null;
                        break;
                }
            }
        }

        /// <summary>The locked round, serially: ticket card → sweat → per-ticket beat, for each ticket,
        /// then FinishAndSettle and the settle card. The director owns the index; we own the ceremony.</summary>
        private IEnumerator PresentRound()
        {
            _idleKey = null;

            while (director.Run.Phase == Phase.Sweat && director.CurrentSession != null)
            {
                _session = director.CurrentSession;
                _ticket = director.CurrentTicket;

                yield return TicketCardBeat();
                yield return PlaySweat();
                yield return SettlementBeat();

                if (!director.AdvanceSweat()) break;
            }

            if (director.Run.Phase == Phase.Sweat)
            {
                director.FinishAndSettle();
                yield return SettleCardBeat();
            }
        }

        private IEnumerator TicketCardBeat()
        {
            ResetForNewSession(); // clears floods/dim/static, resets the light, shows the attract
            _tAttract.text = "SIT TO WATCH THE SWEAT";
            _tAttract.color = new Color(phosphorGreen.r, phosphorGreen.g, phosphorGreen.b, 1f);
            RenderTicketCard();

            yield return WaitSeated();
            _tAttract.enabled = false;
            yield return SeatedHold(ticketCardDuration * 1000f);
        }

        private IEnumerator PlaySweat()
        {
            RenderPregame();
            int lastLeg = _ticket.Legs.Count - 1;

            while (_session != null && !_session.IsComplete)
            {
                yield return WaitSeated();            // step ONLY while seated
                if (_session.IsComplete) break;      // e.g. cashed out while we waited

                if (!_session.MoveNext(out DramaEvent evt)) break;
                _eventsEmitted++;
                RenderEvent(evt);

                if (_stage != null)
                {
                    // M-T3: the scenes own pacing AND the resolution ceremony.
                    yield return TheaterBeat(evt);
                    continue;
                }

                bool onFinalLeg = evt.LegIndex == lastLeg;
                if (evt.Type == DramaEventType.LegFinal)
                    yield return ResolveBeat(evt);

                // The pending-loss window (charm expansion): a dead leg suspended the session
                // while a save is held — the drama freezes on the player's timed decision.
                if (_session.HasPendingLoss)
                    yield return PendingWindowBeat();

                if (_session.IsComplete) break;
                yield return SeatedHold(PacingFor(evt, onFinalLeg));
            }
        }

        // ---------------------------------------------------------------- theater beats (M-T3)

        /// <summary>One beat as theater: non-final beats play their resolved scene (the scene's
        /// duration IS the pacing); a LegFinal plays the final whistle sequence with the
        /// ledger's stoppage-time plan; a pending loss suspends the kill scene at the shot
        /// mid-flight, holds through the save window, and resumes with the continuation chosen
        /// from the FINAL ticket-local grade (never WinProbAfter).</summary>
        private IEnumerator TheaterBeat(DramaEvent evt)
        {
            // TVS-H02 race fix: PlaySweat's `yield return WaitSeated();` gates ENTRY to this loop
            // iteration, but that check and this method's first side effect (SuspendMarket /
            // RevealBeatChrome, both unconditional) are not the same instant - when PlaySweat hands
            // off `yield return TheaterBeat(evt)`, Unity does not always give this freshly-returned
            // coroutine its first step in the same frame as the WaitSeated() check that admitted it;
            // it can land one frame later. Standing in exactly that gap let a new beat announce
            // itself (SuspendMarket's "MARKET SUSPENDED" overwriting the frozen cash-out text) after
            // the player had already stood - confirmed by direct instrumentation: WaitSeated() passed
            // while _seated was still true, ForceSeated(false) ran later that same frame, and
            // TheaterBeat's own entry (and SuspendMarket) then ran on the NEXT frame with _seated
            // already false, with nothing in between re-checking it. Re-verifying here, as literally
            // the first thing this method does, closes that gap regardless of which frame Unity
            // chooses to hand this coroutine its first step.
            while (!_seated) yield return null;

            Leg leg = _ticket.Legs[evt.LegIndex];
            _stage.timeScale = Mathf.Max(0f, TimeScaleOverride);

            if (evt.Type != DramaEventType.LegFinal)
            {
                SceneSpec spec = _choreo.ResolveBeat(evt, _lastBeatUp, _lastBeatDelta, _ledger,
                    leg, _countLedger);
                bool nearMiss = spec.Template == SceneTemplate.NearMissHope
                    || spec.Template == SceneTemplate.NearMissScare;
                _audioUrgency = spec.Urgent ? 1f : 0f;
                if (spec.Template == SceneTemplate.CornerFor || spec.Template == SceneTemplate.CornerAgainst)
                    _audio?.CornerRiser(spec.Duration * 0.80f);
                if (nearMiss)
                {
                    float riserSeconds = spec.Duration * 0.66f
                        * Mathf.Max(0.0001f, TimeScaleOverride);
                    _audio?.NearMissRiser(riserSeconds);
                }
                StartClockRun(SweatFlavor.Minute(evt), spec.Duration);

                // A staged goal owns the beat's story (Sol, M-T4.1): the flavor speaks the
                // goal call and the TAPE dot wears the SCORER'S color — both keyed to the
                // goal's beneficiary, never the tie-broken beat direction (a flat floor
                // beat reconciles for the opponent while the tie-break says "up").
                if (spec.Goal.HasValue)
                {
                    // Scorer's color = the side that SCORES; the goal-call language = the
                    // money direction. Decoupled for market legs (Sol, F_0.4.0 P3).
                    _pendingBeatBeneficiary = TeamColor(leg, spec.Goal.Value.ScoredByPicked);
                    if (evt.Type == DramaEventType.Momentum)
                        _pendingFlavor = SweatFlavor.GoalLine(spec.Goal.Value.ForPicked, leg, evt.Step);
                    PrepareScoringActor(leg, spec.Goal.Value);
                }

                // A batched count reveal says so out loud — one corner animation can carry
                // several flags from the spell (amount-aware commentary; Sol, F_0.4.0 P3).
                bool countLeg = leg.Selection.Kind == MarketKind.TotalCorners
                    || leg.Selection.Kind == MarketKind.TotalCards;
                bool countScene = spec.Count.HasValue && spec.Count.Value.TotalDelta > 0;
                if (countScene && spec.Count.Value.TotalDelta > 1)
                    _pendingFlavor += $" ({spec.Count.Value.TotalDelta} in the spell)";
                // A zero batch fell through to ordinary play: the pre-computed corner/booking
                // line would narrate an event the pitch never shows (Sol, F_0.4.0 P3 r2).
                // But a staged goal owns the beat's story wherever it lands (M-T4.1) — the
                // goal call wins over neutral possession (Sol, F_0.4.0 P3 r3).
                if (countLeg && !countScene)
                    _pendingFlavor = spec.Goal.HasValue
                        ? SweatFlavor.GoalLine(spec.Goal.Value.ForPicked, leg, evt.Step)
                        : SweatFlavor.NeutralLine(evt, leg, _lastBeatUp);

                // Market suspension is for DANGEROUS scenes only (playtest #13 — blanket
                // suspension left almost no window to cash out): goal chances and near-misses
                // suspend until their reveal, exactly like a real book on a dangerous attack;
                // possession scenes carry so little information that the market drifts openly
                // — their chrome reveals at scene start.
                bool dangerous = spec.Goal.HasValue
                    || spec.Count.HasValue
                    || spec.Template == SceneTemplate.NearMissHope
                    || spec.Template == SceneTemplate.NearMissScare;
                if (dangerous)
                {
                    SuspendMarket();
                    _stage.PlayScene(spec, OnGoalPlayed,
                        nearMiss || spec.Count.HasValue ? RevealBeatAudio : RevealBeatChrome, null,
                        OnCountPlayed);
                }
                else
                {
                    RevealBeatChrome(); // low-information beat: prob drifts, price stays live
                    _stage.PlayScene(spec, OnGoalPlayed, null, null, OnCountPlayed);
                }
                yield return WaitSceneDone();
                _audioUrgency = 0f;
                if (_session.IsComplete) yield break; // cashed out mid-scene
                yield return SeatedHold(interSceneGapMs); // idle filler ≤1s between scenes
                yield break;
            }

            _audioUrgency = 1f;
            SuspendMarket(); // a final is always a dangerous attack
            BeginFinalSequenceClock();
            if (_session.HasPendingLoss)
            {
                // The clippable moment: buildup → the shot freezes mid-flight → the prompt.
                // The chrome still speaks the PRE-KILL state (the honest displayed value the
                // whistle rolls against) — the 0% never shows before the story resolves.
                _stage.SuspendKillShot(ScenePlaybook.VariantFor(evt.Step));
                while (_stage.ScenePlaying && !_stage.SuspendedAtShot) yield return null;
                yield return PendingWindowBeat();

                LegGrade grade = leg.IsVoided ? LegGrade.Voided
                    : leg.GradesWon ? LegGrade.Won : LegGrade.Lost;
                ScoreLedger.FinalPlan plan = _ledger.PlanFinal(grade);
                // TVS-S01 fix: PlanFinal derives each remaining batch's team attribution from
                // its own HomeDelta/AwayDelta now — no bet-derived flag to compute here.
                CountLedger.FinalPlan? countPlan = _countLedger?.PlanFinal();
                _stage.ResumeSuspended(plan, countPlan, leg.Selection.Kind, OnGoalPlayed, OnCountPlayed, null);
                yield return WaitSceneDone();
                yield return FinalSlam(evt, grade);
            }
            else
            {
                LegGrade grade = leg.IsVoided ? LegGrade.Voided
                    : leg.GradesWon ? LegGrade.Won : LegGrade.Lost;
                SceneSpec spec = _choreo.ResolveFinal(grade, evt.Step, _ledger, _countLedger, leg);
                ScoreLedger.FinalPlan plan = _ledger.PlanFinal(grade);
                _stage.PlayFinalScene(spec, plan, spec.CountFinal, OnGoalPlayed, OnCountPlayed, null);
                yield return WaitSceneDone();
                yield return FinalSlam(evt, grade);
            }
        }

        /// <summary>Waits out the active scene. Seating freezes the stage (the scene stalls, so
        /// this stalls with it); a cash-out abandons the scene — the gold flood takes the screen.</summary>
        private IEnumerator WaitSceneDone()
        {
            while (_stage != null && _stage.ScenePlaying)
            {
                if (_ticket != null && _ticket.State == TicketState.CashedOut)
                {
                    _stage.CancelScene();
                    yield break;
                }
                yield return null;
            }
        }

        /// <summary>The GREEN/DEAD slam, fired as the final scene completes (TvLight sync) —
        /// which is also the final's REVEAL moment: only now do the clock read FT and the bar
        /// snap to the outcome. The VOID path already spoke inside the pending window (the
        /// slip ceremony), and a voided match keeps its pre-kill bar — 0/1 never applied.</summary>
        private IEnumerator FinalSlam(DramaEvent evt, LegGrade grade)
        {
            _tClock.text = "FT";
            RevealedView.SetClock(_tClock.text);
            _finalSequenceActive = false;
            _clockTicking = false;
            if (grade == LegGrade.Won)
            {
                _probTarget = 1f;
                _tWinPct.text = "WIN 100%";
            }
            else if (grade == LegGrade.Lost)
            {
                _probTarget = 0f;
                _tWinPct.text = "WIN 0%";
            }
            RevealedView.SetProbability(_probTarget);
            RevealedView.ResolveLeg(evt.LegIndex, grade);

            _resolvedThrough = evt.LegIndex + 1;
            UpdateSlipStrip(evt.LegIndex + 1);
            _tape?.ResolveLeg(evt.LegIndex, grade);
            PunchWinProbBar();
            int k = evt.LegIndex + 1;
            if (grade == LegGrade.Won)
            {
                _audio?.Whistle();
                _audio?.SlamWon();
                yield return GreenLegBeat(k);
            }
            else if (grade == LegGrade.Lost)
            {
                _audio?.Whistle();
                _audio?.SlamLost();
                yield return DeadLegBeat(k);
            }
            _audioUrgency = 0f;
            ReopenMarket(); // between legs (or at the end) the fresh price may speak again
        }

        /// <summary>A staged goal's playback completed: the ledger commits (or the chalk-off
        /// stands), the scorebug speaks, VAR gets its line. The ONLY score path (goal-playback
        /// invariant: the board can never move without a goal on the pitch).</summary>
        private void OnGoalPlayed(ScoreLedger.StagedGoal goal)
        {
            _audio?.GoalHit(goal.Commits);
            Player scorer = ScorerFor(goal, _ticket != null && _stageLeg >= 0 && _stageLeg < _ticket.Legs.Count
                ? _ticket.Legs[_stageLeg] : null);
            _ledger.CompleteGoal(goal);
            if (_finalSequenceActive)
            {
                _stoppageGoalCount++;
                _tClock.text = $"90'+{_stoppageGoalCount}";
                RevealedView.SetClock(_tClock.text);
            }
            if (_ticket != null && _stageLeg >= 0 && _stageLeg < _ticket.Legs.Count)
                UpdateScorebug(_ticket.Legs[_stageLeg]);
            if (!goal.Commits)
            {
                _tFlavor.color = flavorColor;
                _tFlavor.text = "VAR — NO GOAL";
                _flavorScale = 1.12f;
            }
            else if (scorer != null && _ticket != null && _stageLeg >= 0 && _stageLeg < _ticket.Legs.Count)
            {
                Leg leg = _ticket.Legs[_stageLeg];
                bool pickedScorer = leg.Selection.Kind == MarketKind.AnytimeScorer
                    && object.ReferenceEquals(scorer, leg.Matchup.PlayerAt(leg.Selection.PlayerIndex));
                _tFlavor.color = pickedScorer ? LaptopOs.MoneyGood : flavorColor;
                _tFlavor.text = pickedScorer
                    ? Surname(scorer.Name) + " STRIKES — THAT'S YOUR MAN"
                    : Surname(scorer.Name) + " FINDS THE NET";
                _flavorScale = 1.12f;
            }
        }

        /// <summary>A corner kick or booking reaches its payoff. Count and market direction
        /// move together here; the stage callback fires before the chrome reveal callback.</summary>
        private void OnCountPlayed(CountLedger.StagedCount count)
        {
            if (_countLedger == null) return;
            _countLedger.CompleteCount(count);
            if (_countLedger.TargetTotal > 0)
            {
                if (_ticket != null && _stageLeg >= 0 && _stageLeg < _ticket.Legs.Count)
                    UpdateScorebug(_ticket.Legs[_stageLeg]);
            }
            if (_ticket != null && _stageLeg >= 0 && _stageLeg < _ticket.Legs.Count
                && _ticket.Legs[_stageLeg].Selection.Kind == MarketKind.TotalCards)
                _audio?.BookingWhistle();
            else
                _audio?.CutRiser();
        }

        /// <summary>Regular time is over when a final scene starts. The stoppage counter is
        /// presentation structure, not outcome information; it advances only from visible
        /// goal-playback callbacks and is replaced by FT at the final slam.</summary>
        private void BeginFinalSequenceClock()
        {
            _finalSequenceActive = true;
            _stoppageGoalCount = 0;
            // Tick the remaining minutes away during the final's pre-reveal hold.
            StartClockRun(90, pacer.FinalSceneSeconds(0) * 0.5f);
        }

        /// <summary>Arms the ticking clock: from the currently shown minute to
        /// <paramref name="targetMinute"/> over <paramref name="sceneSeconds"/> of story time.
        /// The ticker in Update honors seating and the stage's own freezes.</summary>
        private void StartClockRun(int targetMinute, float sceneSeconds)
        {
            _clockTargetMin = Mathf.Max(_clockShownMin, targetMinute);
            float dur = Mathf.Max(0.05f, sceneSeconds * Mathf.Max(0.0001f, TimeScaleOverride));
            _clockRate = (_clockTargetMin - _clockShownMin) / dur;
            _clockTicking = _clockRate > 0f;
            RenderClockMinute();
        }

        /// <summary>Advances the ticking clock while the show is actually rolling: seated, a
        /// scene playing, not frozen at the suspension point. The pending window and stand-up
        /// pause stop time itself — the frozen clock is part of the dread.</summary>
        private void TickClock()
        {
            if (!_clockTicking || _stage == null) return;
            if (!_seated || !_stage.ScenePlaying || _stage.SuspendedAtShot) return;
            int before = Mathf.FloorToInt(_clockShownMin);
            _clockShownMin = Mathf.Min(_clockTargetMin, _clockShownMin + _clockRate * Time.deltaTime);
            if (_clockShownMin >= _clockTargetMin) _clockTicking = false;
            if (Mathf.FloorToInt(_clockShownMin) != before) RenderClockMinute();
        }

        private void RenderClockMinute()
        {
            if (_finalSequenceActive && _stoppageGoalCount > 0) return; // 90'+n owns the text
            _tClock.text = $"{Mathf.Max(1, Mathf.FloorToInt(_clockShownMin))}'";
            RevealedView.SetClock(_tClock.text);
        }

        /// <summary>The pending-loss window (charm expansion): [M] plays a Mulligan (leg voided,
        /// sweat resumes), [R] plays the Ref's Whistle (the call goes to review at the odds you
        /// were living on — overturned it STANDS at full odds, confirmed it dies), [N] declines.
        /// The drama holds as long as the decision does — the pause IS the moment. Without a
        /// keyboard (batch tests) the window declines immediately, so autoplay never hangs.</summary>
        private IEnumerator PendingWindowBeat()
        {
            if (Keyboard.current == null)
            {
                _session.DeclinePendingLoss();
                yield break;
            }

            bool canM = director.Run.OwnsConsumable("mulligan_slip") && _session.CanMulliganPendingLoss;
            bool canR = director.Run.OwnsConsumable("refs_whistle");
            string verbs = (canM ? "[M] MULLIGAN   ·   " : "")
                + (canR ? $"[R] SEND TO REVIEW ({Mathf.RoundToInt((float)(_session.PendingLossProbBefore * 100))}%)   ·   " : "");
            _tCashOut.enabled = true;
            _tCashOut.color = new Color(gold.r, gold.g, gold.b, 1f); // the prompt is actionable — gold
            _tCashOut.text = verbs + "[N] LET IT DIE";

            while (_session.HasPendingLoss)
            {
                if (_seated && canM && Keyboard.current.mKey.wasPressedThisFrame)
                {
                    director.Run.PlayMulliganSlip(_session);
                    _tCashOut.enabled = false;
                    _tFlavor.color = chromeCyan;
                    _tFlavor.text = "THE SLIP COMES OUT — LEG VOIDED, THE TICKET LIVES";
                    _emissRest = _emissIdle; // the DEAD dim lifts: the ticket breathes again
                    tvLight?.ResetToIdle();
                    UpdateSlipStrip(Mathf.Min(_resolvedThrough, _ticket.Legs.Count - 1));
                    yield return ScaledWait(deadLineDuration);
                    yield break;
                }
                if (_seated && canR && Keyboard.current.rKey.wasPressedThisFrame)
                {
                    director.Run.PlayRefsWhistle(_session);
                    _tCashOut.enabled = false;
                    if (!_session.IsComplete)
                    {
                        _tFlavor.color = new Color(phosphorGreen.r, phosphorGreen.g, phosphorGreen.b, 1f);
                        _tFlavor.text = "REVIEWED — OVERTURNED. THE LEG STANDS.";
                        _emissRest = _emissIdle;
                        tvLight?.ResetToIdle();
                        UpdateSlipStrip(Mathf.Min(_resolvedThrough, _ticket.Legs.Count - 1));
                    }
                    else
                    {
                        _tFlavor.color = new Color(hotRed.r, hotRed.g, hotRed.b, 1f);
                        _tFlavor.text = "REVIEWED — THE CALL IS CONFIRMED.";
                    }
                    yield return ScaledWait(deadLineDuration);
                    yield break;
                }
                if (_seated && Keyboard.current.nKey.wasPressedThisFrame)
                {
                    _session.DeclinePendingLoss();
                    _tCashOut.enabled = false;
                    yield break;
                }
                yield return null;
            }
            _tCashOut.enabled = false;
        }

        private IEnumerator SettlementBeat()
        {
            if (_ticket == null) yield break;

            switch (_ticket.State)
            {
                case TicketState.CashedOut:
                    yield return ScaledWait(cashOutFloodDuration); // the gold flood (fired on accept) breathes
                    break;
                case TicketState.Lost:
                    yield return TicketDeadBeat();
                    break;
                default: // still Open, grades won -> the payout moment
                    yield return WinBeat();
                    break;
            }
        }

        // ---------------------------------------------------------------- rendering

        private void ResetForNewSession()
        {
            _eventsEmitted = 0;
            _flavorLegSeen = -1;
            _presModel.ResetForTicket();
            _tape?.ResetForTicket(_ticket != null ? _ticket.Legs.Count : 0);
            _tape?.Show(false);
            CleanupConfetti();
            StopCashOutAnimation();
            _hasCashOutShown = false;
            _cashOutShown = 0.0;
            _cashOutScale = 1f;
            _cashOutFlash = 0f;
            _barPunch = 1f;
            _pendingTapeBeat = false;
            _stageLeg = -1;
            _stageBeatCount = 0;
            _countLedger = null;
            _finalSequenceActive = false;
            _audioUrgency = 0f;
            _stoppageGoalCount = 0;
            _marketSuspended = false;
            RevealedView.Reset(director != null ? director.Run : null, _ticket,
                director != null ? director.SweatIndex : 0);
            _tCashOut.color = new Color(gold.r, gold.g, gold.b, 1f);
            _stage?.Show(false);

            _emissRest = _emissIdle;
            tvLight?.ResetToIdle();

            SetAlpha(_greenFlood, 0f);
            SetAlpha(_goldFlood, 0f);
            SetAlpha(_dimOverlay, 0f);
            SetRawAlpha(_staticNoise, 0f);
            _tBigAmount.text = string.Empty;
            if (_tConsolation != null) _tConsolation.enabled = false;
            _tCashOut.enabled = false;
            _tCashOut.rectTransform.localScale = Vector3.one;
            _tAttract.enabled = true;
            _tFlavor.color = flavorColor;

            RenderPregame();
        }

        private void RenderPregame()
        {
            if (_ticket == null || _ticket.Legs.Count == 0) return;
            Leg leg = _ticket.Legs[0];
            _tMatchup.text = MatchupLine(leg);
            _tRecords.text = RecordsLine(leg);
            _tLeg.text = $"LEG 1/{_ticket.Legs.Count}";
            _tClock.text = "PRE";
            _tFlavor.text = "the board is set.";
            _probTarget = (float)leg.TrueProb;
            _probShown = _probTarget;
            _tWinPct.text = $"WIN {Mathf.RoundToInt(_probTarget * 100f)}%";
            _resolvedThrough = 0;
            UpdateSlipStrip(0);
            BeginStageLeg(0, leg, 0);
        }

        /// <summary>Kicks the stage off for a leg: model-owned team colors (deterministic,
        /// non-reserved pool), the picked side attacking right, territory opening at TrueProb.
        /// New leg = new match: the score ledger resets and the scorebug re-speaks.</summary>
        private void BeginStageLeg(int legIndex, Leg leg, int beatCount)
        {
            if (_stage == null) return;
            _stageLeg = legIndex;
            _stageBeatCount = beatCount;
            _tape?.Show(true);
            _ledger.ResetForLeg();
            _ledger.ConfigureEndpoint(leg);
            _countLedger = null;
            if (leg.Selection.Kind == MarketKind.TotalCorners || leg.Selection.Kind == MarketKind.TotalCards)
            {
                _countLedger = new CountLedger();
                _countLedger.ConfigureEndpoint(leg.Matchup.StatLine, leg.Selection.Kind, Math.Max(1, beatCount));
            }
            // Kickoff: the match clock returns to zero and the final-sequence state clears.
            _clockShownMin = 0f;
            _clockTicking = false;
            _finalSequenceActive = false;
            _stoppageGoalCount = 0;
            (uint home, uint away) = TheaterPalette.TeamColors(leg.Matchup.Home.Name, leg.Matchup.Away.Name);
            _stage.Show(true);
            _stage.BeginLeg(TheaterStage.FromRgb(home), TheaterStage.FromRgb(away),
                pickedIsHome: SweatFlavor.PickedHomeForPresentation(leg), openingProb: (float)leg.TrueProb);
            RevealedView.BeginLeg(legIndex, leg);
            UpdateScorebug(leg);
        }

        /// <summary>The theater scorebug (M-T3, playtest #10 finding #2 pulled forward from
        /// chrome v2): team names IN their dot colors so the stage is instantly attributable,
        /// the running synthesized score between them, the picked side marked. Away @ home
        /// order, matching the slate's convention everywhere else.</summary>
        private void UpdateScorebug(Leg leg)
        {
            if (_tMatchup == null) return;
            (uint homeRgb, uint awayRgb) = TheaterPalette.TeamColors(leg.Matchup.Home.Name, leg.Matchup.Away.Name);
            bool pickedHome = SweatFlavor.PickedHomeForPresentation(leg);
            int homeScore = pickedHome ? _ledger.Picked : _ledger.Opponent;
            int awayScore = pickedHome ? _ledger.Opponent : _ledger.Picked;

            string away = SweatFlavor.Short(leg.Matchup.Away.Name).ToUpperInvariant();
            string home = SweatFlavor.Short(leg.Matchup.Home.Name).ToUpperInvariant();
            // The pick dot marks YOUR TEAM — a market leg has none, so it wears no dot
            // (the slip strip's market label carries the pick until Phase 3's market chip).
            bool isMl = leg.Selection.Kind == MarketKind.Moneyline;
            string awayMark = isMl && !pickedHome ? "● " : "";
            string homeMark = isMl && pickedHome ? " ●" : "";
            _tMatchup.text =
                $"<color=#{awayRgb:X6}>{awayMark}{away}</color>  {awayScore} — {homeScore}  " +
                $"<color=#{homeRgb:X6}>{home}{homeMark}</color>";
            string chip = MarketChip(leg);
            _tRecords.text = RecordsLine(leg) + (chip.Length > 0 ? $"   |   {chip}" : string.Empty);
            RevealedView.SetScore($"{away} {awayScore} — {home} {homeScore}");
        }

        private string MarketChip(Leg leg)
        {
            MarketSelection selection = leg.Selection;
            MatchStatLine line = leg.Matchup.StatLine;
            if (line == null) return string.Empty;
            string choice = selection.Choice == MarketChoice.Over ? "O"
                : selection.Choice == MarketChoice.Under ? "U" : string.Empty;
            switch (selection.Kind)
            {
                // The chip shows only REVEALED counts (0 before any payoff) — falling back to
                // the baked stat line would leak the outcome (causal reveal law).
                case MarketKind.TotalCorners:
                    return $"CORNERS {_countLedger?.Home ?? 0}-{_countLedger?.Away ?? 0} | {choice} {selection.Line:0.0}";
                case MarketKind.TotalCards:
                    return $"CARDS {_countLedger?.Total ?? 0} | {choice} {selection.Line:0.0}";
                case MarketKind.BothTeamsToScore:
                    // Teams-scored progress, not a scoreline — BTTS asks "how many of the
                    // two have scored", so the chip reads n/2 (Sol, F_0.4.0 P3).
                    return $"BTTS {(_ledger.Picked > 0 ? 1 : 0) + (_ledger.Opponent > 0 ? 1 : 0)}/2";
                case MarketKind.TotalGoals:
                    return $"GOALS {_ledger.Picked}-{_ledger.Opponent} | {choice} {selection.Line:0.0}";
                case MarketKind.AnytimeScorer:
                    // Scorer IDENTITY is this market's outcome — mid-sweat the chip makes no
                    // count claim at all (mapping revealed board goals onto the baked scorer
                    // list resolves the market ahead of the live price; the shown state must
                    // never say more than the quote does). Identity lands at the whistle.
                    Player player = leg.Matchup.PlayerAt(selection.PlayerIndex);
                    return $"{Surname(player.Name)} ANYTIME";
                default:
                    return string.Empty;
            }
        }

        private void PrepareScoringActor(Leg leg, ScoreLedger.StagedGoal goal)
        {
            Player scorer = ScorerFor(goal, leg);
            if (scorer == null || _stage == null) return;
            bool pickedHome = SweatFlavor.PickedHomeForPresentation(leg);
            bool scorerHome = goal.ScoredByPicked ? pickedHome : !pickedHome;
            var roster = scorerHome ? leg.Matchup.Home.Players : leg.Matchup.Away.Players;
            int index = 0;
            for (; index < roster.Count; index++)
                if (object.ReferenceEquals(roster[index], scorer)) break;
            _stage.SetScoringActor(scorerHome, index, scorer.Name);
        }

        private Player ScorerFor(ScoreLedger.StagedGoal goal, Leg leg)
        {
            if (!goal.Commits || leg == null || leg.Matchup.StatLine == null) return null;
            // On the SWEATED scorer leg, identity IS the market outcome — naming mid-sweat
            // goals from the baked list would resolve the market ahead of the live price.
            // Identity stays suspended until the final sequence (the payoff moment).
            if (leg.Selection.Kind == MarketKind.AnytimeScorer && !_finalSequenceActive) return null;
            bool pickedHome = SweatFlavor.PickedHomeForPresentation(leg);
            bool scorerHome = goal.ScoredByPicked ? pickedHome : !pickedHome;
            int index = goal.ScoredByPicked ? _ledger.Picked : _ledger.Opponent;
            var scorers = scorerHome ? leg.Matchup.StatLine.HomeScorers : leg.Matchup.StatLine.AwayScorers;
            return index >= 0 && index < scorers.Count ? scorers[index] : null;
        }

        private static string Surname(string name)
        {
            int i = name.LastIndexOf(' ');
            return (i >= 0 ? name.Substring(i + 1) : name).ToUpperInvariant();
        }

        private static Color TeamColor(Leg leg, bool pickedSide)
        {
            (uint home, uint away) = TheaterPalette.TeamColors(leg.Matchup.Home.Name, leg.Matchup.Away.Name);
            bool pickedHome = SweatFlavor.PickedHomeForPresentation(leg);
            uint picked = pickedHome ? home : away;
            uint opponent = pickedHome ? away : home;
            return TheaterStage.FromRgb(pickedSide ? picked : opponent);
        }

        /// <summary>The always-on slip strip during a sweat (playtest #6: "what did I place, how much
        /// is at risk, at what odds?"). Rich-text legs colored by PRESENTED status — resolved legs use
        /// the presentation cursor, never engine truth (outcomes are baked at lock; the strip must not
        /// leak them early): green W / red L / cyan VOID. Unresolved legs wear THEIR TEAM'S theater
        /// color (playtest #13 — "which team am I sweating for?"): live at full strength, pending
        /// dimmed; the money colors still take over the instant a leg resolves.</summary>
        private void UpdateSlipStrip(int liveLeg)
        {
            if (_ticket == null) { _tSlipStrip.text = string.Empty; return; }

            string strip = $"RISK ${Money(_ticket.Stake)} → PAYS ${Money(_ticket.PotentialPayout)}     ";
            for (int i = 0; i < _ticket.Legs.Count; i++)
            {
                Leg leg = _ticket.Legs[i];
                string label = $"{leg.DisplayLabel} {OddsFormat.American(leg.OfferedOdds)}";
                (uint homeRgb, uint awayRgb) = TheaterPalette.TeamColors(leg.Matchup.Home.Name, leg.Matchup.Away.Name);
                uint pickedRgb = SweatFlavor.PickedHomeForPresentation(leg) ? homeRgb : awayRgb;

                if (i > 0) strip += "  ·  ";
                if (i < _resolvedThrough)
                {
                    strip += leg.IsVoided ? $"<color=#9EDCF6>{label} VOID</color>"
                        : leg.GradesWon ? $"<color=#3CE873>{label} W</color>"
                        : $"<color=#FF4038>{label} L</color>";
                }
                else if (i == liveLeg)
                {
                    strip += $"<color=#{pickedRgb:X6}>{label} LIVE</color>";
                }
                else
                {
                    strip += $"<color=#{pickedRgb:X6}99>{label}</color>"; // dimmed team color
                }
            }
            _tSlipStrip.text = strip;
        }

        /// <summary>The auto-advance interstitial (M4): TICKET i/n, the legs line, stake → to-win.</summary>
        private void RenderTicketCard()
        {
            _stage?.Show(false); // the interstitial card is stage-free; pregame re-raises it
            _stageLeg = -1;
            _tLeg.text = string.Empty;
            _tClock.text = "PRE";
            _tMatchup.text = $"TICKET {director.SweatIndex + 1}/{director.Run.Sweats.Count}";

            string legs = string.Empty;
            foreach (Leg leg in _ticket.Legs)
            {
                if (legs.Length > 0) legs += "   ·   ";
                legs += $"{leg.DisplayLabel} {OddsFormat.American(leg.OfferedOdds)}";
            }
            _tRecords.text = legs;
            _tSlipStrip.text = string.Empty;

            _tFlavor.color = flavorColor;
            _tFlavor.text = $"${Money(_ticket.Stake)} TO WIN ${Money(_ticket.PotentialPayout)}";

            float p0 = (float)_ticket.Legs[0].TrueProb;
            _probTarget = _probShown = p0;
            _tWinPct.text = $"WIN {Mathf.RoundToInt(p0 * 100f)}%";
        }

        /// <summary>The round's settle card short of a run end (payment model): PAYMENT MADE green,
        /// or the Totem burning. RunWon/RunLost skip this — the persistent verdict card owns them.</summary>
        private IEnumerator SettleCardBeat()
        {
            SettlementReport? maybe = director.LastSettle;
            if (maybe == null) yield break;
            SettlementReport s = maybe.Value;
            if (s.Outcome == Phase.RunWon || s.Outcome == Phase.RunLost) yield break;

            SetAlpha(_dimOverlay, 0f);
            SetRawAlpha(_staticNoise, 0f);
            _tCashOut.enabled = false;
            _tAttract.enabled = false;
            _tLeg.text = string.Empty;
            _tClock.text = string.Empty;
            _tWinPct.text = string.Empty;
            _tBigAmount.text = string.Empty;
            _tSlipStrip.text = string.Empty;

            if (s.TotemFired)
            {
                _tMatchup.text = $"SHORT — ${Money(s.BankBefore)} AGAINST ${Money(s.Payment)}";
                _tFlavor.color = new Color(hotRed.r, hotRed.g, hotRed.b, 1f);
                _tFlavor.text = "THE TOTEM BURNS";
                double juiced = s.Payment * (1.0 + (director?.Run?.Config.TotemJuiceRate ?? 0.5));
                _tRecords.text = $"PAYMENT DEFERRED — YOUR BANK STANDS. THE NEXT ONE GROWS BY ${Money(juiced)}";
                _emissRest = new Color(_emissIdle.r * 0.3f, _emissIdle.g * 0.12f, _emissIdle.b * 0.12f);
                EmissionFlash(new Color(0.25f, 0.02f, 0.02f));
                tvLight?.SetRest(new Color(0.7f, 0.18f, 0.15f), 0.32f);
            }
            else
            {
                _tMatchup.text = "PAYMENT MADE";
                _tFlavor.color = new Color(phosphorGreen.r, phosphorGreen.g, phosphorGreen.b, 1f);
                _tFlavor.text = $"−${Money(s.Payment)}   ·   BANK ${Money(s.BankAfter)}";
                _tRecords.text = string.Empty;
                EmissionFlash(phosphorGreen);
                tvLight?.Flash(new Color(0.30f, 1f, 0.45f), 3.0f);
            }

            yield return ScaledWait(settleCardDuration);
        }

        /// <summary>A per-phase idle screen (Betting / Shop), painted once per key.</summary>
        private void RenderIdle(string key, string title, string sub, bool moneyIdle)
        {
            if (_idleKey == key) return;
            _idleKey = key;
            _session = null;
            _ticket = null;
            RevealedView.Clear();

            ClearToBlankScreen();
            _tAttract.enabled = true;
            _tAttract.color = new Color(phosphorGreen.r, phosphorGreen.g, phosphorGreen.b, 1f);
            _tAttract.text = title;
            _tWinPct.text = sub;

            if (moneyIdle)
            {
                _emissRest = _emissIdle;
                tvLight?.ResetToIdle();
            }
        }

        /// <summary>The persistent run verdict card; the room light drops cold on a loss, warm gold on
        /// the win (M4 grill decision - the cheap lighting fake of the room states).</summary>
        private void RenderRunOver()
        {
            Run r = director.Run;
            bool won = r.Phase == Phase.RunWon;
            string key = $"over-{r.Phase}-{(long)r.Bank}";
            if (_idleKey == key) return;
            _idleKey = key;
            _session = null;
            _ticket = null;
            RevealedView.Clear();

            ClearToBlankScreen();
            _tAttract.enabled = true;
            _tAttract.text = won ? "THE HOUSE BLINKS FIRST" : "THE BOOKIE COLLECTS";
            _tAttract.color = won
                ? new Color(gold.r, gold.g, gold.b, 1f)
                : new Color(hotRed.r, hotRed.g, hotRed.b, 1f);
            _tWinPct.text = $"FINAL BANK ${Money(r.Bank)}  —  NEW RUN AT THE LAPTOP";

            if (won)
            {
                _emissRest = gold * 0.08f;
                EmissionFlash(gold);
                tvLight?.Flash(new Color(1f, 0.82f, 0.25f), 3.4f);
                tvLight?.SetRest(new Color(1f, 0.82f, 0.35f), 0.45f);
            }
            else
            {
                // Cold and dark: desaturated blue-grey, barely lit - the room mourns.
                _emissRest = new Color(0.008f, 0.010f, 0.018f);
                EmissionFlash(new Color(0.10f, 0.02f, 0.02f));
                tvLight?.SetRest(new Color(0.30f, 0.34f, 0.48f), 0.10f);
            }
        }

        private void ClearToBlankScreen()
        {
            _stage?.Show(false);
            _tape?.Show(false);
            if (_tConsolation != null) _tConsolation.enabled = false;
            SetAlpha(_greenFlood, 0f);
            SetAlpha(_goldFlood, 0f);
            SetAlpha(_dimOverlay, 0f);
            SetRawAlpha(_staticNoise, 0f);
            _tCashOut.enabled = false;
            _tBigAmount.text = string.Empty;
            _tLeg.text = string.Empty;
            _tClock.text = string.Empty;
            _tMatchup.text = string.Empty;
            _tRecords.text = string.Empty;
            _tSlipStrip.text = string.Empty;
            _tWinPct.text = string.Empty;
            _tFlavor.text = string.Empty;
        }

        private void RenderEvent(DramaEvent evt)
        {
            Leg leg = _ticket.Legs[evt.LegIndex];

            _tRecords.text = RecordsLine(leg);
            _tLeg.text = $"LEG {evt.LegIndex + 1}/{_ticket.Legs.Count}";

            if (evt.LegIndex != _flavorLegSeen)
            {
                _flavorLegSeen = evt.LegIndex;
                _prevProb = leg.TrueProb; // pre-event anchor for this leg's first beat
            }
            string flavor = SweatFlavor.For(evt, leg, _prevProb);
            _prevProb = evt.WinProbAfter;

            // The stage speaks the same beat (model owns the direction rule — one authority).
            _lastBeatUp = _presModel.RecordBeat(evt, leg);
            SweatPresentationModel.BeatRecord beat = _presModel.Beats[_presModel.Beats.Count - 1];
            _lastBeatDelta = beat.Delta;
            _pendingBeatDelta = beat.Delta;
            _pendingTapeBeat = evt.Type != DramaEventType.LegFinal;
            _pendingBeatBeneficiary = TeamColor(leg, _lastBeatUp);
            if (_stage != null)
            {
                // Causal reveal (M-T3.1): identity chrome may update now, but the win-prob,
                // WIN %, flavor, and clock are STASHED — they land at the scene's payoff
                // (RevealBeatChrome / FinalSlam), never before the pitch has shown the story.
                _pendingProb = (float)evt.WinProbAfter;
                _pendingFlavor = flavor;

                if (evt.LegIndex != _stageLeg || _stageBeatCount != evt.TotalSteps)
                    BeginStageLeg(evt.LegIndex, leg, evt.TotalSteps);
                _stage.SetLiveProb((float)evt.WinProbAfter);
                UpdateScorebug(leg); // colored identity + running score (M-T3 scorebug)
            }
            else
            {
                _tClock.text = SweatFlavor.Clock(evt);
                _tFlavor.color = flavorColor;
                _tFlavor.text = flavor;
                _flavorScale = 1.12f; // punch
                _probTarget = (float)evt.WinProbAfter;
                _tWinPct.text = $"WIN {Mathf.RoundToInt(_probTarget * 100f)}%";
                _tMatchup.text = MatchupLine(leg);
                RevealedView.SetProbability(_probTarget);
                RevealedView.SetClock(_tClock.text);
                UpdateCashOutLabel();
            }

            _tAttract.enabled = false;
            UpdateSlipStrip(evt.LegIndex);
        }

        /// <summary>The beat's payoff moment on the stage: NOW the chrome may speak — the
        /// win-prob bar moves, the flavor line lands, the clock ticks, the market re-opens
        /// at the fresh price. Fired by the scene's onReveal (goal / save / scene end).</summary>
        private void RevealBeatChrome()
        {
            if (_pendingTapeBeat)
            {
                _tape?.AppendBeat(_stageLeg, _pendingBeatBeneficiary,
                    SweatPresentationModel.MagnitudeBand(_pendingBeatDelta));
                _pendingTapeBeat = false;
            }
            _probTarget = _pendingProb;
            _tWinPct.text = $"WIN {Mathf.RoundToInt(_probTarget * 100f)}%";
            RevealedView.SetProbability(_probTarget);
            RevealedView.SetClock(_tClock.text);
            _tFlavor.color = flavorColor;
            _tFlavor.text = _pendingFlavor;
            _flavorScale = 1.12f;
            PunchWinProbBar();
            ReopenMarket();
        }

        private void RevealBeatAudio()
        {
            _audio?.CutRiser();
            RevealBeatChrome();
        }

        /// <summary>While a scene plays, the book suspends the market (M-T3.1): the engine has
        /// already repriced, so the only honest options are the new price (a spoiler) or no
        /// price. Real books suspend on a dangerous attack — so does ours.</summary>
        private void SuspendMarket()
        {
            _marketSuspended = true;
            RevealedView.SetMarketSuspended(true);
            StopCashOutAnimation();
            bool offerExists = _session != null && !_session.IsComplete
                && _eventsEmitted >= 1 && _session.CashOutOffer().HasValue;
            if (offerExists)
            {
                // Neutral pending-gray (the slip strip's not-yet color) — NOT cyan: in sweat
                // semantics cyan is reserved for VOID (design/08), and a suspended market at
                // peak tension must never read as a voided leg.
                _tCashOut.enabled = true;
                _tCashOut.color = new Color(0.48f, 0.53f, 0.56f, 1f);
                _tCashOut.text = "MARKET SUSPENDED";
            }
            else
            {
                _tCashOut.enabled = false;
            }
        }

        private void ReopenMarket()
        {
            _marketSuspended = false;
            RevealedView.SetMarketSuspended(false);
            _tCashOut.color = new Color(gold.r, gold.g, gold.b, 1f);
            UpdateCashOutLabel();
        }

        private void UpdateCashOutLabel()
        {
            double? offer = _session != null && !_session.IsComplete && _eventsEmitted >= 1
                ? _session.CashOutOffer()
                : null;
            if (offer.HasValue)
            {
                _tCashOut.enabled = true;
                _tCashOut.color = new Color(gold.r, gold.g, gold.b, 1f); // suspend dims it; live gold restores
                SetCashOutOffer(offer.Value);
            }
            else
            {
                StopCashOutAnimation();
                _tCashOut.enabled = false;
            }
        }

        private void SetCashOutOffer(double offer)
        {
            if (!_hasCashOutShown)
            {
                _hasCashOutShown = true;
                _cashOutShown = offer;
                _cashOutRoundShown = RoundBucket(offer);
                RenderCashOut(offer);
                return;
            }

            if (Math.Abs(offer - _cashOutShown) < 0.005)
            {
                RenderCashOut(_cashOutShown);
                return;
            }

            bool dropped = offer < _cashOutShown;
            if (dropped) _cashOutFlash = 1f; // gold taunt, never a money-bad red signal
            StopCashOutAnimation();
            _cashOutAnimation = StartCoroutine(AnimateCashOut(_cashOutShown, offer));
        }

        private IEnumerator AnimateCashOut(double from, double to)
        {
            float duration = Mathf.Max(0f, cashOutTickDuration * Mathf.Max(0f, TimeScaleOverride));
            if (duration <= 0f)
            {
                _cashOutShown = to;
                _cashOutRoundShown = RoundBucket(to);
                RenderCashOut(to);
                _cashOutAnimation = null;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += SeatedDeltaTime; // TVS-H02: freezes exactly while standing
                float t = Mathf.Clamp01(elapsed / duration);
                _cashOutShown = from + (to - from) * t;
                int bucket = RoundBucket(_cashOutShown);
                if (bucket != _cashOutRoundShown)
                {
                    _cashOutRoundShown = bucket;
                    _cashOutScale = 1.18f;
                }
                RenderCashOut(_cashOutShown);
                yield return null;
            }

            _cashOutShown = to;
            _cashOutRoundShown = RoundBucket(to);
            RenderCashOut(to);
            _cashOutAnimation = null;
        }

        private int RoundBucket(double amount)
        {
            double multiple = Math.Max(1.0, cashOutRoundMultiple);
            return (int)Math.Floor(amount / multiple);
        }

        private void RenderCashOut(double amount)
        {
            if (_tCashOut == null) return;
            _tCashOut.text = $"CASH OUT ${Money(amount)}   [E]";
        }

        private void StopCashOutAnimation()
        {
            if (_cashOutAnimation != null)
            {
                StopCoroutine(_cashOutAnimation);
                _cashOutAnimation = null;
            }
        }

        private string MatchupLine(Leg leg)
        {
            string away = SweatFlavor.Short(leg.Matchup.Away.Name);
            string home = SweatFlavor.Short(leg.Matchup.Home.Name);
            // Mark the picked side with a dot so the player knows which team is theirs.
            // Market legs have no team — no dot (their pick reads from the market label).
            bool isMl = leg.Selection.Kind == MarketKind.Moneyline;
            bool pickedHome = SweatFlavor.PickedHomeForPresentation(leg);
            string awayMark = isMl && !pickedHome ? "● " : "";
            string homeMark = isMl && pickedHome ? " ●" : "";
            return $"{awayMark}{away.ToUpperInvariant()}  @  {home.ToUpperInvariant()}{homeMark}";
        }

        private static string RecordsLine(Leg leg)
            => $"({leg.Matchup.Away.Record})          ({leg.Matchup.Home.Record})"; // season W-L

        // ---------------------------------------------------------------- beats

        private IEnumerator ResolveBeat(DramaEvent evt)
        {
            Leg leg = _ticket.Legs[evt.LegIndex];
            int k = evt.LegIndex + 1;

            if (leg.IsVoided)
            {
                _tFlavor.color = chromeCyan;
                _tFlavor.text = $"LEG {k} - VOIDED, the ticket lives";
                yield return ScaledWait(deadLineDuration);
            }
            else if (leg.GradesWon)
            {
                yield return GreenLegBeat(k);
            }
            else
            {
                yield return DeadLegBeat(k);
            }

            _resolvedThrough = evt.LegIndex + 1;
            UpdateSlipStrip(evt.LegIndex + 1); // next leg reads LIVE once its events start
        }

        private IEnumerator GreenLegBeat(int k)
        {
            _tFlavor.color = new Color(phosphorGreen.r, phosphorGreen.g, phosphorGreen.b, 1f);
            _tFlavor.text = $"LEG {k} - GREEN";
            EmissionFlash(phosphorGreen * 1.0f);
            tvLight?.Flash(new Color(0.30f, 1f, 0.45f), 3.0f);
            yield return FloodPulse(_greenFlood, new Color(0.15f, 1f, 0.35f), 0.55f, greenFloodDuration);
        }

        private IEnumerator DeadLegBeat(int k)
        {
            // 1) static - regenerate the noise a few times so it crawls.
            SetRawAlpha(_staticNoise, 0.85f);
            tvLight?.Flash(new Color(1f, 0.2f, 0.15f), 2.2f);
            float dur = Mathf.Max(0f, deadStaticDuration * Mathf.Max(0f, TimeScaleOverride));
            int regens = Mathf.Max(1, staticRegens);
            float per = dur / regens;
            for (int i = 0; i < regens; i++)
            {
                RegenNoise();
                yield return WaitRealtime(per);
            }
            SetRawAlpha(_staticNoise, 0f);

            // 2) the red DEAD line + the screen dropping darker.
            _tFlavor.color = new Color(hotRed.r, hotRed.g, hotRed.b, 1f);
            _tFlavor.text = $"LEG {k} - DEAD";
            _emissRest = new Color(_emissIdle.r * 0.3f, _emissIdle.g * 0.12f, _emissIdle.b * 0.12f); // darker, redder
            EmissionFlash(new Color(0.25f, 0.02f, 0.02f));
            tvLight?.SetRest(new Color(0.7f, 0.18f, 0.15f), 0.32f);
            yield return ScaledWait(deadLineDuration);
        }

        private IEnumerator TicketDeadBeat()
        {
            // TV dims to near-black for a beat before the next demo ticket.
            tvLight?.SetRest(new Color(0.5f, 0.12f, 0.1f), 0.18f);
            float dur = Mathf.Max(0f, ticketDeadDimDuration * Mathf.Max(0f, TimeScaleOverride));
            float t = 0f;
            while (t < dur)
            {
                t += SeatedDeltaTime; // TVS-H02: freezes exactly while standing
                SetAlpha(_dimOverlay, Mathf.Lerp(0f, 0.94f, t / dur));
                yield return null;
            }
            SetAlpha(_dimOverlay, 0.94f);
            yield return ScaledWait(ticketDeadSilenceDuration);

            // The consolation renders on ITS OWN element built above the dim overlay —
            // _tFlavor sits beneath the 94% dim and would be unreadable (Sol, M-T4).
            string[] consolation =
            {
                "the book thanks you for your patronage.",
                "so close. they always are.",
                "a courtesy: nobody saw that.",
                "the model remains extremely confident."
            };
            int ticketIndex = director != null ? director.SweatIndex : 0;
            _tConsolation.text = consolation[Math.Abs(ticketIndex) % consolation.Length];
            _tConsolation.enabled = true;
            yield return ScaledWait(ticketDeadConsolationDuration);
            _tConsolation.enabled = false;
        }

        private IEnumerator WinBeat()
        {
            double payout = _ticket.PotentialPayout;
            _audio?.SlamWon();
            _tBigAmount.color = new Color(gold.r, gold.g, gold.b, 1f);
            _tBigAmount.text = "+$0";
            EmissionFlash(gold);
            tvLight?.Flash(new Color(1f, 0.82f, 0.25f), 3.4f);
            StartCoroutine(FloodPulse(_goldFlood, new Color(1f, 0.78f, 0.15f), 0.5f, winFloodDuration));
            StartCoroutine(WinConfetti());

            float duration = Mathf.Max(0f, winTallyDuration * Mathf.Max(0f, TimeScaleOverride));
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += SeatedDeltaTime; // TVS-H02: freezes exactly while standing
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                _tBigAmount.text = $"+${Money(payout * t)}";
                yield return null;
            }
            _tBigAmount.text = $"+${Money(payout)}";
            yield return ScaledWait(Mathf.Max(0f, winConfettiDuration - winTallyDuration));
            _tBigAmount.text = string.Empty;
        }

        private IEnumerator WinConfetti()
        {
            CleanupConfetti();
            int count = Mathf.Max(0, winConfettiCount);
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"WinConfetti_{i}", typeof(Image));
                go.transform.SetParent(_canvasRoot, false);
                var image = go.GetComponent<Image>();
                image.raycastTarget = false;
                image.color = _confettiRandom.NextDouble() < 0.68
                    ? new Color(gold.r, gold.g, gold.b, 1f)
                    : Color.white;
                var rt = image.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                float side = 8f + (float)_confettiRandom.NextDouble() * 6f;
                rt.sizeDelta = new Vector2(side, side);
                rt.anchoredPosition = new Vector2(
                    (float)(_confettiRandom.NextDouble() * _canvasWidth - _canvasWidth * 0.5),
                    _canvasHeight * 0.5f + 8f + (float)_confettiRandom.NextDouble() * 32f);
                _confetti.Add(new ConfettiPiece
                {
                    Rect = rt,
                    Velocity = new Vector2((float)(_confettiRandom.NextDouble() * 70f - 35f),
                        -(70f + (float)_confettiRandom.NextDouble() * 80f)),
                    Spin = (float)(_confettiRandom.NextDouble() * 360f - 180f)
                });
            }

            float duration = Mathf.Max(0f, winConfettiDuration * Mathf.Max(0f, TimeScaleOverride));
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += SeatedDeltaTime; // TVS-H02: freezes exactly while standing
                float storyDt = SeatedDeltaTime / Mathf.Max(0.0001f, TimeScaleOverride);
                for (int i = 0; i < _confetti.Count; i++)
                {
                    ConfettiPiece piece = _confetti[i];
                    if (piece.Rect == null) continue;
                    piece.Velocity.y -= 150f * storyDt;
                    piece.Rect.anchoredPosition += piece.Velocity * storyDt;
                    piece.Rect.Rotate(0f, 0f, piece.Spin * storyDt);
                    _confetti[i] = piece;
                }
                yield return null;
            }
            CleanupConfetti();
        }

        private void CleanupConfetti()
        {
            for (int i = 0; i < _confetti.Count; i++)
                if (_confetti[i].Rect != null) Destroy(_confetti[i].Rect.gameObject);
            _confetti.Clear();
        }

        /// <summary>Fired from Update the instant E is accepted, so the gold hit is responsive.</summary>
        private IEnumerator CashOutFloodBeat(double amount)
        {
            _tBigAmount.color = new Color(gold.r, gold.g, gold.b, 1f);
            _tBigAmount.text = $"${Money(amount)}";
            _tCashOut.enabled = false;
            EmissionFlash(gold);
            tvLight?.Flash(new Color(1f, 0.82f, 0.25f), 3.4f);
            yield return FloodPulse(_goldFlood, new Color(1f, 0.78f, 0.15f), 0.55f, cashOutFloodDuration);
            _tBigAmount.text = string.Empty;
        }

        private IEnumerator FloodPulse(Image flood, Color color, float peakAlpha, float baseDuration)
        {
            flood.color = new Color(color.r, color.g, color.b, 0f);
            float dur = Mathf.Max(0f, baseDuration * Mathf.Max(0f, TimeScaleOverride));
            float t = 0f;
            while (t < dur)
            {
                t += SeatedDeltaTime; // TVS-H02: freezes exactly while standing
                float a = Mathf.Sin(Mathf.Clamp01(t / dur) * Mathf.PI) * peakAlpha; // rise then settle
                SetAlpha(flood, a);
                yield return null;
            }
            SetAlpha(flood, 0f);
        }

        // ---------------------------------------------------------------- input (Update)

        /// <summary>Frozen substitute for Time.deltaTime (TVS-H02): 0 while standing, the real
        /// per-frame delta while seated. Every timer/coroutine/animator this class owns reads dt
        /// through this gate (or through _seatedClock for Time.time reads) instead of Unity's clock
        /// directly, so one flag freezes all of them and sitting back down resumes with no catch-up.
        /// </summary>
        private float SeatedDeltaTime => _seated ? Time.deltaTime : 0f;

        private void Update()
        {
            _seatedClock += SeatedDeltaTime; // TVS-H02: frozen substitute for Time.time while standing

            RefreshChrome();
            ApplyEmission();
            AnimateBar();
            AnimateFlavorPunch();
            AnimateCashOutTaunt();
            TickClock();

            if (_audio != null)
            {
                _audio.masterVolume = masterVolume;
                _audio.crowdVolume = crowdVolume;
                _audio.stingVolume = stingVolume;
                _audio.SetTension(1f - Mathf.Abs(2f * _probShown - 1f), _audioUrgency);
                bool dread = !_seated || (_session != null && _session.HasPendingLoss);
                _audio.Duck(dread, dread ? 0.15f : 0.8f);
            }

            // The stage freezes with the viewing contract: standing pauses mid-motion. The
            // pending-loss window's freeze is the stage's own suspension point (M-T3) — the
            // kill scene's buildup must PLAY before the shot hangs mid-flight.
            // The frozen flag itself is already kept in sync by SetSeated() the instant seating
            // changes (TVS-H02 race fix) — this call is a harmless, idempotent restatement, not
            // the primary propagation path.
            if (_stage != null)
            {
                _stage.SetFrozen(!_seated);
                _stage.timeScale = Mathf.Max(0f, TimeScaleOverride);
            }

            if (_interact != null && _interact.WasPressedThisFrame())
                TryCashOut();
        }

        private void TryCashOut()
        {
            if (!CanAcceptCashOutNow()) return; // TVS-H01: same predicate as the stand-suppression gate
            double? offer = _session.CashOutOffer();
            if (!offer.HasValue) return; // defensive; CanAcceptCashOutNow already checked this

            _lastCashOutAmount = offer.Value;
            _session.AcceptCashOut();               // credits the bank; marks the ticket CashedOut
            RevealedView.MarkCashedOut();
            _audio?.CashOutKaChunk();
            StartCoroutine(CashOutFloodBeat(_lastCashOutAmount));
        }

        private void RefreshChrome()
        {
            Run r = director != null ? director.Run : null;
            if (r == null) { _tChrome.text = string.Empty; return; }
            // Wound-up ratchet state rides the chrome (rev 5 §20) — compact, only when live.
            var stacks = new StringBuilder();
            foreach (EffectStat s in r.EffectStates)
                if (s.Value > 0)
                    stacks.Append("   ·   ").Append(s.Label).Append(' ')
                        .Append(s.Value.ToString("0.#", CultureInfo.InvariantCulture));
            _tChrome.text =
                $"R{r.Round}/{r.Config.Rounds}   ·   BANK ${Money(r.Bank)}   ·   PAY ${Money(r.CurrentPayment)}" +
                $"   ·   COMPS {r.Comps.ToString("0.#", CultureInfo.InvariantCulture)}{stacks}   ·   {r.Rng.RunSeed}";
        }

        private void ApplyEmission()
        {
            if (emissiveScreen == null) return;
            // TVS-H02: both driven from the seated-only clock so the idle flicker holds exactly
            // (not just slower) while standing, with no phase jump on resume.
            _emissFlash01 = Mathf.MoveTowards(_emissFlash01, 0f, emissionDecay * SeatedDeltaTime);
            Color e = Color.Lerp(_emissRest, _emissFlash, _emissFlash01);
            float flick = 1f + (Mathf.PerlinNoise(_emissSeed, _seatedClock * 9f) - 0.5f) * 2f * idleEmissionFlicker;
            _emissBlock.SetColor(EmissionColorId, e * Mathf.Max(0f, flick));
            emissiveScreen.SetPropertyBlock(_emissBlock);
        }

        private void EmissionFlash(Color color)
        {
            _emissFlash = color;
            _emissFlash01 = 1f;
        }

        private void AnimateBar()
        {
            if (_barFill == null) return;
            // TVS-H02: seated-only dt/clock throughout, so the win-prob animation (§4.4) holds
            // exactly while standing, with no phase jump in the breathing wave on resume.
            _probShown = Mathf.Lerp(_probShown, _probTarget, 1f - Mathf.Exp(-9f * SeatedDeltaTime));
            float w = Mathf.Clamp01(_probShown) * _innerWidth;
            float hz = Mathf.Lerp(breathSlowHz, breathFastHz, Mathf.Abs(2f * _probShown - 1f));
            float breathe = 1f + Mathf.Sin(_seatedClock * hz * 2f * Mathf.PI) * breathAmplitude;
            float punchDt = SeatedDeltaTime / Mathf.Max(0.0001f, TimeScaleOverride);
            _barPunch = Mathf.MoveTowards(_barPunch, 1f, 2.8f * punchDt);
            _barFill.rectTransform.sizeDelta = new Vector2(w, _barHeight - 8f);
            _barFill.rectTransform.localScale = new Vector3(1f, breathe * _barPunch, 1f);
        }

        private void PunchWinProbBar() => _barPunch = 1.15f;

        private void AnimateCashOutTaunt()
        {
            if (_tCashOut == null) return;
            float scaledDt = SeatedDeltaTime / Mathf.Max(0.0001f, TimeScaleOverride); // TVS-H02
            _cashOutScale = Mathf.MoveTowards(_cashOutScale, 1f, 3.2f * scaledDt);
            _cashOutFlash = Mathf.MoveTowards(_cashOutFlash, 0f, 4.5f * scaledDt);
            _tCashOut.rectTransform.localScale = Vector3.one * _cashOutScale;
            if (_tCashOut.enabled && !_marketSuspended)
            {
                Color brightGold = Color.Lerp(gold, Color.white, 0.28f);
                _tCashOut.color = Color.Lerp(gold, brightGold, _cashOutFlash);
            }
        }

        private void AnimateFlavorPunch()
        {
            if (_tFlavor == null) return;
            _flavorScale = Mathf.MoveTowards(_flavorScale, 1f, 1.4f * SeatedDeltaTime); // TVS-H02
            _tFlavor.rectTransform.localScale = Vector3.one * _flavorScale;
        }

        // ---------------------------------------------------------------- pacing / waits

        /// <summary>The pacing table (ported from SweatRenderer.PacingFor): base delay by tension tag, an
        /// extra beat right before a leg's final whistle, and everything slowed on the ticket's final leg.</summary>
        private float PacingFor(DramaEvent e, bool isFinalLeg)
        {
            float ms = e.Tag switch
            {
                TensionTag.Calm => calmMs,
                TensionTag.Swing => swingMs,
                TensionTag.LeadChange => leadChangeMs,
                TensionTag.NearMiss => nearMissMs,
                TensionTag.Decisive => decisiveMs,
                _ => calmMs,
            };
            if (e.Step == e.TotalSteps - 1) ms += preFinalExtraMs;
            if (isFinalLeg) ms *= finalLegMultiplier;
            return ms;
        }

        private IEnumerator WaitSeated()
        {
            while (!_seated) yield return null;
        }

        /// <summary>Holds for the given ms (scaled), counting time ONLY while seated - standing freezes it
        /// mid-event and the offer stays frozen. Bails immediately once the session completes.</summary>
        private IEnumerator SeatedHold(float ms)
        {
            float remaining = ms * 0.001f * Mathf.Max(0f, TimeScaleOverride);
            while (remaining > 0f)
            {
                if (_session == null || _session.IsComplete) yield break;
                if (_seated) remaining -= Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>Holds for the given scaled seconds, counting time ONLY while seated (TVS-H02) —
        /// every ceremony/effect/transition hold in this file (dead-leg, ticket-dead, win-beat,
        /// settlement, settle-card, pending-window) is built from this one gated primitive.</summary>
        private IEnumerator ScaledWait(float seconds)
        {
            float dur = Mathf.Max(0f, seconds * Mathf.Max(0f, TimeScaleOverride));
            float t = 0f;
            while (t < dur) { t += SeatedDeltaTime; yield return null; }
        }

        /// <summary>As ScaledWait, but ignores TimeScaleOverride (used for the dead-leg static
        /// regen crawl). Still seated-gated (TVS-H02): standing freezes it mid-regen.</summary>
        private IEnumerator WaitRealtime(float seconds)
        {
            float t = 0f;
            while (t < seconds) { t += SeatedDeltaTime; yield return null; }
        }

        // ---------------------------------------------------------------- canvas construction

        private void BuildCanvas()
        {
            int w = referencePixelsWide;
            int h = Mathf.RoundToInt(referencePixelsWide * screenWorldSize.y / screenWorldSize.x);
            _barHeight = 30f;
            float barWidth = 700f;
            _innerWidth = barWidth - 8f;

            var canvasGo = new GameObject("SweatCanvas", typeof(Canvas));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var canvasRt = canvas.GetComponent<RectTransform>();
            canvasRt.sizeDelta = new Vector2(w, h);

            // Float toward the couch, but aim +Z INTO the wall: UGUI text reads correctly from the
            // canvas's -Z side (playtest #4 fix - +Z at the viewer shows the back face, mirrored).
            Vector3 normal = emissiveScreen != null ? -emissiveScreen.transform.forward : Vector3.left;
            Vector3 pos = (emissiveScreen != null ? emissiveScreen.transform.position : new Vector3(1.232f, 1.1f, 0.3f))
                          + normal * canvasOffset;
            canvasGo.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(-normal, Vector3.up));
            canvasGo.transform.localScale = Vector3.one * (screenWorldSize.x / w);

            Transform root = canvasGo.transform;
            _canvasRoot = root;
            _canvasWidth = w;
            _canvasHeight = h;
            float halfW = w / 2f, halfH = h / 2f;

            // Backing panel (near-black; the phosphor glow bleeds through its slight transparency).
            _backing = MakeStretchImage(root, "Backing", screenBg);

            // --- top scorebug ---
            _tLeg = MakeText(root, "Leg", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(16f, -12f), new Vector2(240f, 40f), 22, TextAnchor.UpperLeft, chromeCyan);
            _tClock = MakeText(root, "Clock", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-16f, -12f), new Vector2(280f, 40f), 22, TextAnchor.UpperRight, chromeCyan);
            _tMatchup = MakeText(root, "Matchup", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -14f), new Vector2(w - 120f, 56f), 34, TextAnchor.UpperCenter, flavorColor, FontStyle.Bold);
            _tRecords = MakeText(root, "Records", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -74f), new Vector2(w - 120f, 30f), 20, TextAnchor.UpperCenter, chromeCyan);
            // The slip strip (playtest #6): stake at risk + every leg with odds and presented status.
            _tSlipStrip = MakeText(root, "SlipStrip", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -106f), new Vector2(w - 80f, 26f), 15, TextAnchor.UpperCenter, chromeCyan);

            // --- middle: flavour ticker + win-prob bar ---
            _tFlavor = MakeText(root, "Flavor", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 60f), new Vector2(w - 60f, 96f), 40, TextAnchor.MiddleCenter, flavorColor, FontStyle.Bold);

            _barBg = MakePanel(root, "BarBg", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -46f), new Vector2(barWidth, _barHeight), barBgColor);
            var fillGo = new GameObject("BarFill", typeof(Image));
            fillGo.transform.SetParent(_barBg.transform, false);
            _barFill = fillGo.GetComponent<Image>();
            _barFill.color = new Color(phosphorGreen.r, phosphorGreen.g, phosphorGreen.b, 1f);
            _barFill.raycastTarget = false;
            var frt = _barFill.rectTransform;
            frt.anchorMin = frt.anchorMax = new Vector2(0f, 0.5f);
            frt.pivot = new Vector2(0f, 0.5f);
            frt.sizeDelta = new Vector2(0f, _barHeight - 8f);
            // Anchor is already the bar's LEFT edge; only the 4px inset remains (playtest #4 fix -
            // the old -barWidth/2 offset assumed a center anchor and hung the fill outside the TV).
            frt.anchoredPosition = new Vector2(4f, 0f);

            _tWinPct = MakeText(root, "WinPct", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -86f), new Vector2(barWidth, 34f), 24, TextAnchor.MiddleCenter, flavorColor, FontStyle.Bold);

            // --- bottom: cash-out + chrome ---
            _tCashOut = MakeText(root, "CashOut", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -140f), new Vector2(w - 60f, 50f), 34,
                TextAnchor.MiddleCenter, new Color(gold.r, gold.g, gold.b, 1f), FontStyle.Bold);
            _tCashOut.enabled = false;

            _tChrome = MakeText(root, "Chrome", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 12f), new Vector2(w - 30f, 28f), 16, TextAnchor.LowerCenter, chromeCyan);

            // --- attract state (before the sweat is live) ---
            _tAttract = MakeText(root, "Attract", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -6f), new Vector2(w - 60f, 130f), 46,
                TextAnchor.MiddleCenter, new Color(phosphorGreen.r, phosphorGreen.g, phosphorGreen.b, 1f), FontStyle.Bold);
            _tAttract.text = "SIT TO WATCH THE SWEAT";

            // --- the match theater stage (F_0.2.0 M-T2) ---
            if (theaterEnabled)
            {
                _stage = TheaterStage.Build(root, new Vector2(0f, 8f), new Vector2(720f, 252f),
                    pitchLineColor, pitchBgColor);
                _stage.paceScale = pacer.paceMultiplier; // stage playback matches the pacer's arithmetic
                ApplyTheaterLayout(w);
                _tape = MomentumTape.Build(root, new Vector2(-330f, -252f), new Vector2(300f, 50f));
                Transform audioAnchor = emissiveScreen != null ? emissiveScreen.transform : transform;
                _audio = TvAudioDirector.Build(audioAnchor);
                if (_audio != null)
                {
                    _audio.masterVolume = masterVolume;
                    _audio.crowdVolume = crowdVolume;
                    _audio.stingVolume = stingVolume;
                }
            }

            // --- overlays (front to back after content) ---
            _greenFlood = MakeStretchImage(root, "GreenFlood", new Color(0.15f, 1f, 0.35f, 0f));
            _staticNoise = MakeStretchRaw(root, "StaticNoise", new Color(1f, 1f, 1f, 0f));
            _dimOverlay = MakeStretchImage(root, "DimOverlay", new Color(0f, 0f, 0f, 0f));
            _goldFlood = MakeStretchImage(root, "GoldFlood", new Color(1f, 0.78f, 0.15f, 0f));
            _tBigAmount = MakeText(root, "BigAmount", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 0f), new Vector2(w - 40f, 200f), 96,
                TextAnchor.MiddleCenter, new Color(gold.r, gold.g, gold.b, 1f), FontStyle.Bold);
            _tBigAmount.text = string.Empty;

            // The bad-beat consolation line — built ABOVE the dim overlay so the sting stays
            // readable through the 94% dim (Sol, M-T4); neutral chrome, never money-red.
            _tConsolation = MakeText(root, "Consolation", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -20f), new Vector2(w - 80f, 44f), 28,
                TextAnchor.MiddleCenter, flavorColor, FontStyle.Italic);
            _tConsolation.enabled = false;

            // Scanlines on very top - a thin repeating dark line at ~15% alpha.
            _scanlines = MakeStretchRaw(root, "Scanlines", Color.white);
            _scanlines.texture = BuildScanlineTexture();
            _scanlines.uvRect = new Rect(0f, 0f, 1f, h / 4f); // one 4px line pair per ~4 screen px

            _noiseTex = new Texture2D(160, 90, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            RegenNoise();
            _staticNoise.texture = _noiseTex;

            _probShown = _probTarget = 0.5f;
        }

        /// <summary>With the stage in the center, the middle chrome re-flows beneath it: the
        /// flavor line shrinks to a commentary strip under the pitch, the win-prob bar and
        /// cash-out drop toward the bottom edge. Positions are graybox dials — Allen tunes at
        /// the M-T2 gate.</summary>
        private void ApplyTheaterLayout(int w)
        {
            _tFlavor.fontSize = 22;
            _tFlavor.rectTransform.sizeDelta = new Vector2(w - 80f, 30f);
            _tFlavor.rectTransform.anchoredPosition = new Vector2(0f, -142f);

            _barBg.rectTransform.anchoredPosition = new Vector2(0f, -176f);

            _tWinPct.fontSize = 20;
            _tWinPct.rectTransform.sizeDelta = new Vector2(360f, 26f);
            _tWinPct.rectTransform.anchoredPosition = new Vector2(0f, -206f);

            _tCashOut.fontSize = 26;
            _tCashOut.rectTransform.sizeDelta = new Vector2(w - 60f, 34f);
            _tCashOut.rectTransform.anchoredPosition = new Vector2(0f, -238f);
        }

        private Text MakeText(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 pos,
            Vector2 size, int fontSize, TextAnchor align, Color color, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            if (_font != null) t.font = _font;
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.alignment = align;
            t.color = color;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return t;
        }

        private static Image MakePanel(Transform parent, string name, Vector2 anchor, Vector2 pivot,
            Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return img;
        }

        private static Image MakeStretchImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            Stretch(img.rectTransform);
            return img;
        }

        private static RawImage MakeStretchRaw(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RawImage));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<RawImage>();
            img.color = color;
            img.raycastTarget = false;
            Stretch(img.rectTransform);
            return img;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private Texture2D BuildScanlineTexture()
        {
            var tex = new Texture2D(1, 4, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Point,
            };
            var dark = new Color(0f, 0f, 0f, scanlineAlpha);
            var clear = new Color(0f, 0f, 0f, 0f);
            tex.SetPixel(0, 0, dark);
            tex.SetPixel(0, 1, dark);
            tex.SetPixel(0, 2, clear);
            tex.SetPixel(0, 3, clear);
            tex.Apply();
            return tex;
        }

        private void RegenNoise()
        {
            if (_noiseTex == null) return;
            var px = new Color32[_noiseTex.width * _noiseTex.height];
            for (int i = 0; i < px.Length; i++)
            {
                byte v = (byte)UnityEngine.Random.Range(0, 256);
                px[i] = new Color32(v, v, v, 255);
            }
            _noiseTex.SetPixels32(px);
            _noiseTex.Apply();
        }

        // ---------------------------------------------------------------- small helpers

        private static void SetAlpha(Image img, float a)
        {
            if (img == null) return;
            Color c = img.color;
            c.a = a;
            img.color = c;
        }

        private static void SetRawAlpha(RawImage img, float a)
        {
            if (img == null) return;
            Color c = img.color;
            c.a = a;
            img.color = c;
        }

        private static string Money(double v)
        {
            long n = (long)Math.Round(v, MidpointRounding.AwayFromZero);
            return n.ToString("N0", CultureInfo.InvariantCulture);
        }

        private static Font LoadFont()
        {
            try { return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch
            {
                Debug.LogWarning("[TvSweatScreen] built-in font not found; text will not render.");
                return null;
            }
        }
    }
}
