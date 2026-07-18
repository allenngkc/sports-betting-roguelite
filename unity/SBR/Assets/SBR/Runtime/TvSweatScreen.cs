using System;
using System.Collections;
using System.Globalization;
using System.Text;
using SBR.Engine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SBR.Game
{
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
        public float cashOutFloodDuration = 0.8f;
        public float winFloodDuration = 1.0f;

        [Header("Feel dials")]
        public float breathAmplitude = 0.06f;
        public float breathSlowHz = 0.7f;
        public float breathFastHz = 2.6f;
        [Tooltip("Idle phosphor emission flicker, fraction of the emissive quad's idle emission.")]
        public float idleEmissionFlicker = 0.05f;
        public float emissionDecay = 3.2f;
        [Range(0f, 1f)] public float scanlineAlpha = 0.15f;

        [Header("Theater (F_0.2.0 — the match theater stage)")]
        [Tooltip("The match theater stage (M-T2). Off = the pre-theater text-ticker layout, kept " +
                 "as the A/B fallback through M-T4 per the plan's reversibility clause.")]
        public bool theaterEnabled = true;
        public Color pitchLineColor = new Color(0.85f, 0.92f, 0.95f, 0.50f);
        public Color pitchBgColor = new Color(0.012f, 0.016f, 0.022f, 0.95f);

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
        /// <summary>Test/debug hook: force the seated state (simulates sitting / looking away) without the
        /// couch. Normal play drives this through SitSpot.SeatedChanged.</summary>
        public void ForceSeated(bool seated) => _seated = seated;

        // ---- state ----
        private bool _seated;
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
        private Text _tMatchup, _tRecords, _tLeg, _tClock, _tFlavor, _tWinPct, _tCashOut, _tChrome, _tAttract, _tBigAmount, _tSlipStrip;
        private Image _backing, _barBg, _barFill, _greenFlood, _goldFlood, _dimOverlay;
        private RawImage _staticNoise, _scanlines;
        private Texture2D _noiseTex;

        // ---- theater (F_0.2.0) ----
        private TheaterStage _stage;
        private readonly SweatPresentationModel _presModel = new SweatPresentationModel();
        private int _stageLeg = -1;

        // =====================================================================================

        private void Awake()
        {
            _font = LoadFont();
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
            _seated = SitSpot.Active != null;
            StartCoroutine(RunChannel());
        }

        private void OnDisable()
        {
            SitSpot.SeatedChanged -= OnSeatedChanged;
            if (SitSpot.InteractStandSuppressed == (Func<bool>)CashOutLive)
                SitSpot.InteractStandSuppressed = null;
            StopAllCoroutines();
        }

        private void OnSeatedChanged(bool seated) => _seated = seated;

        private void ResolveInput()
        {
            if (actions == null) return;
            InputActionMap map = actions.FindActionMap("Player", throwIfNotFound: false);
            if (map == null) return;
            _interact = map.FindAction("Interact");
            map.Enable();
        }

        /// <summary>An offer is showing that E should accept (rather than stand the player up).</summary>
        private bool CashOutLive()
            => _session != null && !_session.IsComplete && _eventsEmitted >= 1 && _session.CashOutOffer().HasValue;

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
            _stageLeg = -1;
            _stage?.Show(false);

            _emissRest = _emissIdle;
            tvLight?.ResetToIdle();

            SetAlpha(_greenFlood, 0f);
            SetAlpha(_goldFlood, 0f);
            SetAlpha(_dimOverlay, 0f);
            SetRawAlpha(_staticNoise, 0f);
            _tBigAmount.text = string.Empty;
            _tCashOut.enabled = false;
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
            BeginStageLeg(0, leg);
        }

        /// <summary>Kicks the stage off for a leg: model-owned team colors (deterministic,
        /// non-reserved pool), the picked side attacking right, territory opening at TrueProb.</summary>
        private void BeginStageLeg(int legIndex, Leg leg)
        {
            if (_stage == null) return;
            _stageLeg = legIndex;
            (uint home, uint away) = TheaterPalette.TeamColors(leg.Matchup.Home.Name, leg.Matchup.Away.Name);
            _stage.Show(true);
            _stage.BeginLeg(TheaterStage.FromRgb(home), TheaterStage.FromRgb(away),
                pickedIsHome: leg.Side == Side.Home, openingProb: (float)leg.TrueProb);
        }

        /// <summary>The always-on slip strip during a sweat (playtest #6: "what did I place, how much
        /// is at risk, at what odds?"). Rich-text legs colored by PRESENTED status — resolved legs use
        /// the presentation cursor, never engine truth (outcomes are baked at lock; the strip must not
        /// leak them early): green W / red L / cyan VOID / white LIVE / dim pending.</summary>
        private void UpdateSlipStrip(int liveLeg)
        {
            if (_ticket == null) { _tSlipStrip.text = string.Empty; return; }

            string strip = $"RISK ${Money(_ticket.Stake)} → PAYS ${Money(_ticket.PotentialPayout)}     ";
            for (int i = 0; i < _ticket.Legs.Count; i++)
            {
                Leg leg = _ticket.Legs[i];
                string side = SweatFlavor.Short(
                    leg.Side == Side.Home ? leg.Matchup.Home.Name : leg.Matchup.Away.Name).ToUpperInvariant();
                string label = $"{side} {OddsFormat.American(leg.OfferedOdds)}";

                if (i > 0) strip += "  ·  ";
                if (i < _resolvedThrough)
                {
                    strip += leg.IsVoided ? $"<color=#9EDCF6>{label} VOID</color>"
                        : leg.GradesWon ? $"<color=#3CE873>{label} W</color>"
                        : $"<color=#FF4038>{label} L</color>";
                }
                else if (i == liveLeg)
                {
                    strip += $"<color=#E8F2F8>{label} LIVE</color>";
                }
                else
                {
                    strip += $"<color=#7A8890>{label}</color>";
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
                string side = SweatFlavor.Short(
                    leg.Side == Side.Home ? leg.Matchup.Home.Name : leg.Matchup.Away.Name);
                legs += $"{side.ToUpperInvariant()} {OddsFormat.American(leg.OfferedOdds)}";
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

            _tMatchup.text = MatchupLine(leg);
            _tRecords.text = RecordsLine(leg);
            _tLeg.text = $"LEG {evt.LegIndex + 1}/{_ticket.Legs.Count}";
            _tClock.text = SweatFlavor.Clock(evt);

            if (evt.LegIndex != _flavorLegSeen)
            {
                _flavorLegSeen = evt.LegIndex;
                _prevProb = leg.TrueProb; // pre-event anchor for this leg's first beat
            }
            _tFlavor.color = flavorColor;
            _tFlavor.text = SweatFlavor.For(evt, leg, _prevProb);
            _prevProb = evt.WinProbAfter;
            _flavorScale = 1.12f; // punch

            _probTarget = (float)evt.WinProbAfter;
            _tWinPct.text = $"WIN {Mathf.RoundToInt(_probTarget * 100f)}%";

            // The stage speaks the same beat (model owns the direction rule — one authority).
            bool up = _presModel.RecordBeat(evt, leg);
            if (_stage != null)
            {
                if (evt.LegIndex != _stageLeg) BeginStageLeg(evt.LegIndex, leg);
                _stage.SetLiveProb((float)evt.WinProbAfter);
                if (evt.Type != DramaEventType.LegFinal) _stage.Pulse(up, evt.Tag);
            }

            _tAttract.enabled = false;
            UpdateSlipStrip(evt.LegIndex);
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
                _tCashOut.text = $"CASH OUT ${Money(offer.Value)}   [E]";
            }
            else
            {
                _tCashOut.enabled = false;
            }
        }

        private string MatchupLine(Leg leg)
        {
            string away = SweatFlavor.Short(leg.Matchup.Away.Name);
            string home = SweatFlavor.Short(leg.Matchup.Home.Name);
            // Mark the picked side with a dot so the player knows which team is theirs.
            string awayMark = leg.Side == Side.Away ? "● " : "";
            string homeMark = leg.Side == Side.Home ? " ●" : "";
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
                t += Time.deltaTime;
                SetAlpha(_dimOverlay, Mathf.Lerp(0f, 0.94f, t / dur));
                yield return null;
            }
            SetAlpha(_dimOverlay, 0.94f);
        }

        private IEnumerator WinBeat()
        {
            double payout = _ticket.PotentialPayout;
            _tBigAmount.color = new Color(gold.r, gold.g, gold.b, 1f);
            _tBigAmount.text = $"+${Money(payout)}";
            EmissionFlash(gold);
            tvLight?.Flash(new Color(1f, 0.82f, 0.25f), 3.4f);
            yield return FloodPulse(_goldFlood, new Color(1f, 0.78f, 0.15f), 0.5f, winFloodDuration);
            _tBigAmount.text = string.Empty;
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
                t += Time.deltaTime;
                float a = Mathf.Sin(Mathf.Clamp01(t / dur) * Mathf.PI) * peakAlpha; // rise then settle
                SetAlpha(flood, a);
                yield return null;
            }
            SetAlpha(flood, 0f);
        }

        // ---------------------------------------------------------------- input (Update)

        private void Update()
        {
            RefreshChrome();
            ApplyEmission();
            AnimateBar();
            AnimateFlavorPunch();

            // The stage freezes with the viewing contract: standing pauses mid-motion, and the
            // pending-loss window holds the frame — the frozen ball is the dread (F_0.2.0).
            _stage?.SetFrozen(!_seated || (_session != null && _session.HasPendingLoss));

            if (_interact != null && _interact.WasPressedThisFrame())
                TryCashOut();
        }

        private void TryCashOut()
        {
            if (!_seated || _session == null || _session.IsComplete || _eventsEmitted < 1) return;
            double? offer = _session.CashOutOffer();
            if (!offer.HasValue) return;

            _lastCashOutAmount = offer.Value;
            _session.AcceptCashOut();               // credits the bank; marks the ticket CashedOut
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
            _emissFlash01 = Mathf.MoveTowards(_emissFlash01, 0f, emissionDecay * Time.deltaTime);
            Color e = Color.Lerp(_emissRest, _emissFlash, _emissFlash01);
            float flick = 1f + (Mathf.PerlinNoise(_emissSeed, Time.time * 9f) - 0.5f) * 2f * idleEmissionFlicker;
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
            _probShown = Mathf.Lerp(_probShown, _probTarget, 1f - Mathf.Exp(-9f * Time.deltaTime));
            float w = Mathf.Clamp01(_probShown) * _innerWidth;
            float hz = Mathf.Lerp(breathSlowHz, breathFastHz, Mathf.Abs(2f * _probShown - 1f));
            float breathe = 1f + Mathf.Sin(Time.time * hz * 2f * Mathf.PI) * breathAmplitude;
            _barFill.rectTransform.sizeDelta = new Vector2(w, _barHeight - 8f);
            _barFill.rectTransform.localScale = new Vector3(1f, breathe, 1f);
        }

        private void AnimateFlavorPunch()
        {
            if (_tFlavor == null) return;
            _flavorScale = Mathf.MoveTowards(_flavorScale, 1f, 1.4f * Time.deltaTime);
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

        private IEnumerator ScaledWait(float seconds)
        {
            float dur = Mathf.Max(0f, seconds * Mathf.Max(0f, TimeScaleOverride));
            float t = 0f;
            while (t < dur) { t += Time.deltaTime; yield return null; }
        }

        private IEnumerator WaitRealtime(float seconds)
        {
            float t = 0f;
            while (t < seconds) { t += Time.deltaTime; yield return null; }
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
                ApplyTheaterLayout(w);
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
