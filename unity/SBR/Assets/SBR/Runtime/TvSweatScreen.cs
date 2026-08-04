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
    /// Beats are all code-driven: WON = a gold flood + emissive spike, DEAD = static then the beat
    /// dropping the screen toward darkness, ticket-dead = a dim-to-black beat, cash-out = a gold flood
    /// with the amount big. The TvLight makes the room the reaction shot.
    ///
    /// Palette is law (DESIGN.md §4): gold is rationed to money/won/cash-out only, everything else is
    /// cold white or grey, and loss is darkness — never a hue. Green and red are the retired
    /// `design/08-art-direction.md` money language ("money-good green, money-bad red") and appear
    /// nowhere in this file; see room-lead-reply.md §3.
    /// Pacing ports the console's table into <see cref="PacingFor"/> with serialized dials; no engine RNG
    /// is consumed by presentation (only MoveNext / CashOut* are called - everything is baked at lock).
    ///
    /// Phase 3C (DESIGN.md §6, VISUAL-DESIGN.md §2): the canvas is Layout B, "Ticket Rail" — a
    /// full-height ticket column at the left (26-28% of the surface), a compact scorebug/stage/event
    /// strip filling the right region, and the cash-out slot anchored at the foot of the ticket column.
    /// Every zone's position comes from <see cref="LayoutGrid"/>, an explicit fixed grid computed ONCE
    /// per canvas build from the canvas's own configured pixel size — never from what is currently
    /// displayed. See BuildCanvas and the "canvas construction" region below.
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
        public float wonFloodDuration = 0.3f;
        [Tooltip("The dark hold on a dead leg. T8: kept at its original length after the static "
            + "crawl was removed — the hold is pacing, the static was decoration.")]
        public float deadStaticDuration = 0.6f;
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
        [Tooltip("C3: how long the score-at-a-goal / ball-at-a-payoff momentary L4 punch holds "
            + "the HDR token before yielding it back.")]
        public float hdrPunchDuration = 0.4f;

        [Header("Feel dials")]
        [Tooltip("Idle phosphor emission flicker, fraction of the emissive quad's idle emission.")]
        public float idleEmissionFlicker = 0.05f;
        public float emissionDecay = 3.2f;
        [Tooltip("DESIGN.md §8/§9: the LIVE leg row's slow pulse, in Hz — the surface's one " +
                 "permitted pulse kind. Off the shared _seatedClock, so every LIVE row pulses in " +
                 "phase and the pulse freezes exactly while standing (TVS-H02).")]
        public float livePulseHz = 0.8f;

        [Header("Audio v0 (procedural, diegetic)")]
        [Range(0f, 1f)] public float masterVolume = 0.5f;
        [Range(0f, 1f)] public float crowdVolume = 0.6f;
        [Range(0f, 1f)] public float stingVolume = 0.8f;

        [Header("Theater (F_0.2.0 — the match theater stage)")]
        [Tooltip("The match theater stage (M-T2/T3). Off = build the fixed grid without the stage " +
                 "(EditMode isolation for the palette/geometry test suites — see " +
                 "TvSweatScreenPaletteTests.cs's own doc comment).")]
        public bool theaterEnabled = true;
        // T41: markings sit in the L1–L2 band (§7: "the pitch is a place, not an event"). Alpha was
        // 0.50 — above the L2 ceiling of 0.40 — which put the markings between the actor tier and
        // the marking tier and contributed to a pitch measuring 1.000 against a 0.671 cash-out band.
        // The HUE is a separate open item: canon's `--tv-pitch` is #3E4A3C green and this is a cold
        // white-grey. T41 ruled the TIER; the hue is not mine to change on my own reading.
        public Color pitchLineColor = new Color(0.85f, 0.92f, 0.95f, 0.40f);
        // Canvas black floor (unified-grade-spec.md §2 / DESIGN.md §2A): opaque canvas pixels must
        // never sit darker than the room's deepest shadow. RGB matches the room team's emissive-quad
        // lift of (0.048, 0.055, 0.068) so the pitch's near-black backdrop and the quad's off-state
        // agree on what "unlit" looks like; alpha unchanged.
        public Color pitchBgColor = new Color(0.048f, 0.055f, 0.068f, 0.95f);
        [Tooltip("Scene-class → seconds (M-T3). The duration-acceptance test pins these bands.")]
        public SweatPacer pacer = new SweatPacer();
        [Tooltip("Idle gap between beat scenes, ms (the ≤1s filler law). Doubles as the " +
                 "guaranteed open-market window per beat (playtest #15).")]
        public float interSceneGapMs = 900f;

        // DESIGN.md §4: cold + quiet, gold rationed to money, loss is darkness. Green and red are
        // the retired `design/08-art-direction.md` money language ("money-good green, money-bad red")
        // and do not appear anywhere below — see room-lead-reply.md §3.
        [Header("Palette (DESIGN.md §4)")]
        [ColorUsage(false, true)] public Color gold = new Color(1.15f, 0.82f, 0.18f); // money, won, payout, cash-out — L3
        // §3's L4: "exactly one element at a time" at full brightness — the cash-out accept punch,
        // the ticket's final payout tally, the run's win card. Brighter than `gold` on purpose so the
        // ordering idle < flash < L4 holds when both are driven through EmissionFlash/TvLight.Flash.
        [ColorUsage(false, true)] public Color goldL4 = new Color(1.84f, 1.31f, 0.29f);
        // T9 (Phase 3B): retired from general chrome duty — cyan has no role in §4's role table.
        // The ONE surviving use is §8's `VOID` leg state ("L2 cyan, struck through on the matrix");
        // every other call site that used to read this field has moved to flavorColor/contextGrey/
        // structureGrey below, judged per its actual §4 role.
        public Color chromeCyan = new Color(0.62f, 0.86f, 0.96f, 0.95f); // §8 VOID leg treatment only

        /// <summary>TV-20: canon's `--tv-void` #7FB2C4 (`palette-tv.css:25`). The VOID leg treatment
        /// was using `chromeCyan` #9EDBF5, which is markedly brighter and lighter than the token.
        /// Kept as its own field rather than retuning chromeCyan, which is still the field name
        /// serialized in `Room.unity` (a §11 forbidden file) and cannot be renamed from here.</summary>
        public Color tvVoid = new Color(0x7F / 255f, 0xB2 / 255f, 0xC4 / 255f, 1f);

        /// <summary>TV-03: canon's `--tv-gold-ink` #0A0C10 (`palette-tv.css:20-21`). The actionable
        /// cash-out state is INVERTED — a solid gold field with the type punched out of it, not gold
        /// type on dark. That inversion is the one canonical L4 treatment on this surface and was
        /// never built; "brightness is a promise about input" is carried by the field, not the
        /// letters.</summary>
        public Color goldInk = new Color(0x0A / 255f, 0x0C / 255f, 0x10 / 255f, 1f);

        /// <summary>TV-21: canon's `--tv-extinguished` #151B21 (`palette-tv.css:32-33`). A lost leg is
        /// "unlit pixel structure" — the ROW carries this background and the text drops to L1
        /// (`TvLegRow.jsx:22,36`). Note the subtlety: the state table says L0, but a dead row's text
        /// renders at L1, because L0 on the text alone would erase the structure the law asks to
        /// keep. Loss is darkness, not absence.</summary>
        public Color extinguished = new Color(0x15 / 255f, 0x1B / 255f, 0x21 / 255f, 1f);
        public Color flavorColor = new Color(0.90f, 0.95f, 0.98f, 1f); // §4 Fact: cold white
        // §4 Context: grey — for beat copy that is neither a live fact nor money (a loss confirmed, a
        // deferred payment) but still needs to stay legible against the lifted black floor below.
        public Color contextGrey = new Color(0.50f, 0.53f, 0.58f, 1f);
        // T9 (Phase 3B): §4 Structure/pending — dim grey at L1. §7 Scorebug: "Ticket/leg index at
        // L1, present but subordinate." Distinct from (and dimmer than) contextGrey's L2.
        public Color structureGrey = new Color(0.14f, 0.15f, 0.16f, 1f);
        // T42 (§4 violation, DD 2026-08-02) — the pitch dots' ONLY two hues.
        //
        // Canon: main-2/docs/design/design-system/tokens/palette-tv.css:22-23
        //   --tv-team-a:#5C7BA8  muted blue — pitch dots only
        //   --tv-team-b:#B2739E  muted pink — pitch dots only
        // and that file's own header, line 4: "Team hues are muted and confined to the pitch dots."
        // The component spec agrees on the model: TvStage.prompt.md types an actor's side as
        // team:"a" | "b" — two sides of THIS match, not a per-club identity.
        //
        // What was here: TheaterPalette.TeamPool, five fully-saturated hues (electric blue #3D7BFF,
        // magenta #E84DD0, orange #FF8A2B, violet #9B5CF6, broadcast white #F0F3F6), assigned by a
        // hash of the team NAME. Two of those hues are not in the TV palette at all, and the DD
        // measured the result as "full chroma" against a palette that says muted. The pool is left
        // untouched because SportsbookApp.cs — the laptop, another worktree's surface — draws its
        // matchup cards from it; per C4 the money colour is already per-surface, and canon gives the
        // TV its own token file for exactly this reason.
        //
        // Consequence worth stating: a club no longer keeps a colour across matches, because canon
        // has only two and they mean "the side you backed / the other side". Identity is carried by
        // the NAME in the scorebug, which is cold white per T32.1. If the two sides ever read as
        // inseparable at four metres, T42 names the remedy and it is not this file's to take:
        // "the fix is form (filled vs hollow dot), never louder colour."
        public Color teamHueA = new Color(0x5C / 255f, 0x7B / 255f, 0xA8 / 255f, 1f);
        public Color teamHueB = new Color(0xB2 / 255f, 0x73 / 255f, 0x9E / 255f, 1f);
        // Phase 3C: §5 scale table pins "Risk / pays: 0.40, L2, gold" — the one place gold sits at L2
        // rather than L3/L4. A dimmed `gold`, comfortably above the black floor on every channel
        // (unlike RunWonRest's 8% dim, which needed clamping) so no floor clamp is required here.
        public Color goldL2 = new Color(0.575f, 0.41f, 0.09f, 1f);
        // §3/§4/§8: "Loss is still darkness ... the old green/red money language stays retired." A
        // lost beat drops the quad/room-light toward this near-neutral, near-zero value instead of
        // flashing red. Never used above ~0.1 magnitude — it must stay below `gold` unconditionally.
        [ColorUsage(false, true)] public Color deadDark = new Color(0.045f, 0.05f, 0.065f, 1f);
        // Canvas black floor (unified-grade-spec.md §2 / DESIGN.md §2A): where the canvas draws
        // OPAQUE pixels (this backing panel, the bar trough) the visible black is the canvas's own,
        // not the room's emissive-quad lift, which only shows through transparent regions. RGB
        // matches that lift, (0.048, 0.055, 0.068), so both read as the same "off" state.
        public Color screenBg = new Color(0.048f, 0.055f, 0.068f, 0.86f);
        public Color barBgColor = new Color(0.048f, 0.055f, 0.068f, 0.92f);

        // ---- public test/debug surface ----
        public int EventsEmitted => _eventsEmitted;
        public bool SweatComplete => _session != null && _session.IsComplete;
        public RevealedView RevealedView { get; } = new RevealedView();
        /// <summary>Test/debug hook: force the seated state (simulates sitting / looking away) without the
        /// couch. Normal play drives this through SitSpot.SeatedChanged.</summary>
        public void ForceSeated(bool seated) => SetSeated(seated);
        /// <summary>Test/debug hook (TVS-H01 regression): true while the cash-out amount is mid-tween
        /// (AnimateCashOut running). Reads _cashOutTweening, not _cashOutAnimation directly — the
        /// Coroutine handle isn't assigned until StartCoroutine returns, one instant after the
        /// tween's own first render already ran (TVS-H02 fix, see _cashOutTweening's declaration).
        /// _cashOutAnimation is otherwise unobservable from outside the sweat, and this is the exact
        /// condition CanAcceptCashOutNow also refuses.</summary>
        public bool DebugCashOutAnimating => _cashOutTweening;

        /// <summary>PRD §9's read-only diagnostic surface: the scene grammar the stage is currently
        /// playing, as its <c>SceneTemplate</c> name.
        ///
        /// <para>Exists for evidence, not for gameplay — nothing in playback reads it back, exactly
        /// like <c>BoundActorRouted</c>. T26 refused the T6 visual half because "nothing that
        /// distinguishes one scene grammar from another is visible at four metres" and the bundle
        /// "carries no scene index, no per-frame grammar label". A reviewer cannot check whether the
        /// grammars differ without knowing which grammar each frame IS, and the capture harness had
        /// no way to ask. This is that question, answered from the same <c>SceneSpec</c> the stage
        /// actually played rather than from a planner re-run that might disagree.</para></summary>
        public string DebugSceneTemplate { get; private set; } = string.Empty;

        /// <summary>PRD §9 diagnostic: the L4 HDR boost this build is compiled with.
        ///
        /// <para>Exists because the first C8·a A/B was UNDELIVERABLE. Both arms were captured
        /// correctly and neither frame carried any token saying which arm it was — the manifest
        /// asserted the pairing and the images could not corroborate it, so the pair proved nothing
        /// on its own. Same failure as an unlabelled grammar set, one review apart. A frame that
        /// needs a document to say what it is has not been delivered; every capture now states its
        /// own boost in its filename.</para></summary>
        public float DebugHdrBoostL4 => HdrBoostL4;

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
        private float _probTarget; // data-only now (RevealedView.WinProbability) — Layout B carries
                                    // no standalone win% visual; DESIGN.md §7's component list has no
                                    // slot for one, and the ticket column's NEED/LIVE copy is the
                                    // PRD-sanctioned channel for "what does the leg still need".
        private float _flavorScale = 1f;
        private double _lastCashOutAmount;
        private double _cashOutShown;
        private bool _hasCashOutShown;
        private Coroutine _cashOutAnimation;
        // TVS-H02 (freeze regression, Phase 3C): tracks "a tween is in flight" SEPARATELY from
        // _cashOutAnimation's own Coroutine handle. StartCoroutine runs AnimateCashOut's body
        // synchronously up to its first yield BEFORE returning that handle, so RenderCashOut's
        // very first call for a new tween used to see _cashOutAnimation still null (the value
        // StopCashOutAnimation had just left it at) and paint the actionable "[E]" label instead
        // of "UPDATING" for exactly one frame. If a poll caught that one mis-rendered frame right
        // before standing, the coroutine's very next iteration (now correctly holding the
        // assigned handle) repainted with the correct "UPDATING" suffix at the SAME frozen
        // amount — a text change with no underlying tick, misread as TVS-H02. This flag is set
        // true before StartCoroutine is called (so the coroutine's own first render already sees
        // it) and false before each tween's final settle render (see AnimateCashOut/
        // StopCashOutAnimation), so RenderCashOut's animating/idle branch is never a frame stale.
        private bool _cashOutTweening;
        // T43 (state lie, DD 2026-08-02): the slot's PRESENTATION state, distinct from
        // _marketSuspended's MARKET state. Two sites paint "MARKET SUSPENDED" — SuspendMarket (the
        // market really is suspended) and PendingWindowBeat (§8.7's intervention overlay, which
        // renders the same slate while _marketSuspended is still false, because ResolveBeat never
        // suspends). Keying the presentation off _marketSuspended alone would leave the pending
        // window's gold field lit under the suspended word for the whole window. This flag is what
        // ShowMarketSuspended sets and what ApplyCashOutSlotState reads, so BOTH sites get the same
        // slate. It is deliberately NOT wired into CanAcceptCashOutNow: that predicate is TVS-H01's
        // input contract, and changing what E accepts during a pending window is a design call for
        // the DD, not a presentation fix. See ApplyCashOutSlotState.
        private bool _cashOutSlotSuspended;
        private float _cashOutScale = 1f;
        private float _cashOutFlash;
        private int _cashOutRoundShown;
        // Anytime-scorer, per leg: true only at the leg's causal identity payoff — see
        // DescribeActiveLeg / OnGoalPlayed. Reset per-leg in BeginStageLeg.
        private bool _scorerRevealedForActiveLeg;

        // ---- input ----
        private InputAction _interact;

        // ---- §8.10 held cash-out preview ----
        //
        /// <summary>True while the player is previewing the settled future (PRD §8.10). The preview
        /// is RENDER-AWARE rather than a one-shot overwrite: <see cref="UpdateTicketColumn"/> and
        /// <see cref="RefreshChrome"/> both consult this flag and recompute from truth, so §8.10's
        /// "release is a full revert — no partial state, no lingering strike-throughs, no bank
        /// flicker" holds BY CONSTRUCTION. A snapshot-and-restore implementation would make that
        /// guarantee depend on remembering to restore every field a future edit adds.
        ///
        /// <para>It is not a second truth source (§4.2): it renders the same revealed facts and the
        /// same offer the cash-out slot is already showing, and consults no locked endpoint.</para></summary>
        private bool _cashOutPreview;

        /// <summary>The amount the preview is quoting — captured at entry from the very offer the
        /// slot displays, so the previewed and accepted numbers can never differ (§8.10).</summary>
        private double _cashOutPreviewAmount;

        /// <summary>The live-leg index the ticket column was last rendered with, so entering and
        /// leaving the preview can re-render the same column without inventing a live leg.</summary>
        private int _liveLegIndexShown = -1;

        /// <summary>The cash-out slot's text as it stood before the preview, restored verbatim on
        /// release (§8.10's "no residue").</summary>
        private string _cashOutTextBeforePreview = string.Empty;

        /// <summary>DESIGN.md §3's tiers, mirrored from
        /// main-2/docs/design/design-system/components/tv/tiers.js: L4 1 · L3 0.7 · L2 0.4 · L1 0.15
        /// · L0 0. Used to step a previewed row exactly one level down.</summary>
        private const float TierL4 = 1f, TierL3 = 0.7f, TierL2 = 0.4f, TierL1 = 0.15f, TierL0 = 0f;

        /// <summary>TV-S1: returns <paramref name="c"/> at a canon brightness tier.
        ///
        /// <para>The ladder is the surface's PRIMARY semantic channel (`palette-tv.css`,
        /// `tiers.js`) — "brightness is the state" — and it was declared here but applied to
        /// exactly one element, so score, clock, NEED, progress and the event strip all rendered at
        /// identical maximum brightness and the ladder carried no hierarchy at all. Every slot now
        /// states its tier at the point it is built, so a reader can check a slot against canon
        /// without tracing where its colour came from.</para>
        ///
        /// <para>Multiplies alpha rather than replacing it, so a colour that is already partly
        /// transparent by design (pitch lines) composes instead of being overridden.</para></summary>
        private static Color AtTier(Color c, float tier)
        {
            c.a *= tier;
            return c;
        }

        // ---- emission (the quad's own glow) ----
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private MaterialPropertyBlock _emissBlock;
        private Color _emissIdle;
        private Color _emissRest;
        private Color _emissFlash;
        private float _emissFlash01;
        private float _emissSeed;

        // ---- canvas HDR path (DESIGN.md §3 / unified-grade-spec.md §4) ----
        // UGUI bakes Graphic.color into a Color32 vertex attribute, which clamps at 1.0 regardless of
        // camera/URP HDR settings — a world-space canvas Image/Text can never exceed 1.0 through the
        // ordinary `.color` setter, so the brightness ladder's L4 tier had nothing for the shared
        // bloom volume to grab. TvSweatHdrUI.shader multiplies the (still 0-1) vertex colour by an
        // unclamped `_HdrBoost` float material property instead, so elements that opt in (given
        // their own material instance below) can exceed 1.0 — everything else keeps the ordinary,
        // SRP-batchable default UI material.
        //
        // C3 (Design Director ruling): "eligibility is not simultaneity." Five graphics now carry
        // this material — CashOut, BigAmount, GoldFlood, Score, Ball — which is strictly ELIGIBILITY
        // (who is physically capable of exceeding 1.0), not a promise that only one of them is ever
        // asked to. DESIGN.md §3's "at most one element at L4 at any instant" is enforced separately,
        // by the named one-token invariant below (_l4Holder / RequestL4 / ReleaseL4) — every call
        // site that wants L4 asks there, never sets a material's boost directly. The live-leg pulse
        // was explicitly ruled to stay OUT of the eligible set.
        private const float HdrBoostL3 = 1f;   // default / "price animating" — DESIGN.md §8.5: never L4
        // The single L4 magnitude (C3 rule 5: "a single value" — no second, per-element scale).
        // C8·a: the floor is now settled on FRAMES, not on this number — "measured on rendered
        // frames at the seated distance, not asserted from a boost value. The 1.8 boost stays only
        // if the scoreline holds there." Both arms of that pair are captured (one seed,
        // same moments); this value stays 1.8 until the DD rules on the images.
        private const float HdrBoostL4 = 1.8f;
        private static readonly int HdrBoostId = Shader.PropertyToID("_HdrBoost");
        private Shader _hdrUiShader;
        private bool _hdrShaderMissing;
        private Material _cashOutHdrMat, _bigAmountHdrMat, _goldFloodHdrMat, _scoreHdrMat, _ballHdrMat;

        /// <summary>C3: the five HDR-eligible focuses. <see cref="Payout"/> drives BOTH the
        /// BigAmount and GoldFlood materials together — a ticket's payout tally and its gold wash
        /// are one visual moment (the payoff at its callback), not two independently competing
        /// ones, so they move as a single participant in the one-token invariant below.</summary>
        private enum HdrFocus { CashOut, Payout, Score, Ball }

        /// <summary>C3's one-token invariant, named and enforced here rather than left as a
        /// convention: at most one <see cref="HdrFocus"/> holds the L4 token at any instant. Null
        /// means nothing currently sits at L4.</summary>
        private HdrFocus? _l4Holder;

        // ---- Layout B, "Ticket Rail" grid (DESIGN.md §6, VISUAL-DESIGN.md §2, PRD §8.1) ----
        //
        // "Every zone position comes from an explicit fixed layout grid defined once in code, never
        // computed from content" (DESIGN.md §6). Every constant below is authored, not tuned per
        // instance — see the header doc for why these are `const`, not serialized fields — and
        // LayoutGrid's fields are a pure function of the canvas's own configured pixel size (w, h),
        // which is itself configuration (screenWorldSize / referencePixelsWide), never the sweat's
        // current content (leg count, text length, market kind). Ticket column width is corrected to
        // 26-28% of the surface per DESIGN.md §6; TicketColumnWidthFraction sits at the middle.
        private const float TicketColumnWidthFraction = 0.27f;
        private const float ChromeStripHeight = 18f;   // PRD §8.1: system chrome stays lowest-priority
        // T20: 56 -> 62. The score is the canon scale's largest element (36px) and shares this zone
        // with the momentum tape's fixed 14px foot; 56 left it 42px, which a 36px line does not
        // clear once its line box is counted. Raised rather than shrinking the score, because
        // DESIGN.md §5's ratio table makes the score the thing nothing may outgrow.
        private const float ScoreBugHeight = 62f;
        private const float BottomRowHeight = 52f;     // shared row: cash-out | event strip
        private const float TicketHeaderHeight = 24f;
        private const float TicketFooterHeight = 40f;  // RISK / PAYS — T20: 36 -> 40 to hold 24px
        // RunConfig.MaxLegs defaults to 6 (engine\RunConfig.cs). BuildCanvas runs from Awake, before
        // GrayboxRoomBuilder assigns `director` (AddComponent fires Awake synchronously, before the
        // caller's next line runs) — the row-slot count cannot be read from the live run and must be
        // a fixed constant per rule 1. Unused slots simply go dark (rule 2): a ticket with fewer legs
        // never reflows the grid, and one with more (a future config change) silently truncates rather
        // than resize anything.
        private const int TicketRowSlots = 6;
        // T16 (Design Director ruling): the momentum tape sits at the FOOT of the scorebug zone —
        // a thin strip hugging its inside-bottom edge, matching MomentumTape's own fixed RowHeight
        // so a single-row ticket fits exactly.
        private const float MomentumTapeHeight = 14f;

        // ---- T20: the canon TV type scale ------------------------------------------------------
        //
        // Mirrored from `main-2/docs/design/design-system/tokens/typography.css` (studio canon).
        // §4A's rule: reference the design system, never fork it — but a C# const cannot import a
        // CSS custom property, so the values are mirrored here WITH their source cited. None of
        // these numbers was chosen to make something fit.
        //
        // They are reference px against the 980x550 world-space canvas, which is exactly what
        // `referencePixelsWide` builds, so they map 1:1 onto Unity canvas units.
        //
        // DESIGN.md §5's RATIO table is the law and this px table is its instantiation:
        // score 1 > cash-out .70 > team .55 > clock/need .50 > progress/risk .40 > event .36 >
        // leg .34 > label .22. Nothing on the surface may outgrow the score.
        //
        // T20 re-derivation (DD, 2026-07-31): progress was 23px, written against a ticket column at
        // ~37%. DESIGN.md §6 corrected the column to 26-28%, and at that width §6's own authored
        // progress strings ("LIVE • 0 GOALS • 3 MORE") no longer fit one line at 23px, while §3
        // permits only the NEED statement to wrap. 19px fits and keeps NEED 28 > progress 19 >
        // eyebrow 15 intact. Shortening the authored strings was rejected BY NAME: paraphrasing
        // authored copy to fit a stale measurement is how the statement line was lost once already.
        private const int TypeScore = 36;
        private const int TypeCashOut = 29;
        private const int TypeTeam = 28;
        private const int TypeClock = 28;
        private const int TypeNeed = 28;
        private const int TypeRisk = 24;
        private const int TypeEvent = 22;
        private const int TypeProgress = 19;
        private const int TypeLeg = 19;
        private const int TypeEyebrow = 15;

        /// <summary>Unity lays one line of <c>Text</c> out in roughly this multiple of its
        /// fontSize. Used to budget the leg row's stacked lines against the FIXED row height.
        /// Deliberately generous, and pinned by a test rather than trusted — a knife-edge fit here
        /// clips glyphs on the real font, which no headless run can see.</summary>
        private const float LineBox = 1.18f;

        /// <summary>PRD §8.1's five stable zones (plus system chrome), computed once per canvas
        /// build. Rects use a top-left origin (x/y grow right/down, matching how the grid reads on
        /// paper); AnchorTopLeft/AnchorTopRight/AnchorTopCenter/AnchorCenter below convert into
        /// Unity's canvas-top-left-anchored coordinate space used throughout BuildCanvas.</summary>
        private readonly struct LayoutGrid
        {
            public readonly Rect TicketColumn;   // header + leg rows + risk/pays footer (not cash-out)
            public readonly Rect TicketHeader;
            public readonly Rect TicketFooter;   // RISK / PAYS
            public readonly Rect CashOut;        // DESIGN.md §6/§7: "anchored at the foot of the ticket column"
            public readonly Rect ScoreBug;
            public readonly Rect Stage;
            public readonly Rect EventStrip;
            public readonly Rect ChromeStrip;
            public readonly float TicketRowHeight;

            public LayoutGrid(float w, float h)
            {
                float ticketW = Mathf.Round(w * TicketColumnWidthFraction);
                float contentH = h - ChromeStripHeight;
                float rightX = ticketW;
                float rightW = w - ticketW;
                float bottomY = contentH - BottomRowHeight;

                ScoreBug = new Rect(rightX, 0f, rightW, ScoreBugHeight);
                Stage = new Rect(rightX, ScoreBugHeight, rightW, bottomY - ScoreBugHeight);
                EventStrip = new Rect(rightX, bottomY, rightW, BottomRowHeight);
                CashOut = new Rect(0f, bottomY, ticketW, BottomRowHeight);
                TicketColumn = new Rect(0f, 0f, ticketW, bottomY);
                TicketHeader = new Rect(0f, 0f, ticketW, TicketHeaderHeight);
                TicketFooter = new Rect(0f, bottomY - TicketFooterHeight, ticketW, TicketFooterHeight);
                ChromeStrip = new Rect(0f, contentH, w, ChromeStripHeight);
                TicketRowHeight = (bottomY - TicketHeaderHeight - TicketFooterHeight) / TicketRowSlots;
            }

            /// <summary>Row <paramref name="index"/>'s rect (0-based, ticket order). A pure function
            /// of the grid and the index alone — never of leg state, text, or which leg is live.</summary>
            public Rect TicketRow(int index) => new Rect(
                TicketColumn.x,
                TicketHeader.yMax + index * TicketRowHeight,
                TicketColumn.width,
                TicketRowHeight);
        }

        // ---- UI ----
        //
        // Canon splits the surface across TWO faces (tokens/fonts.css), and which slot gets which is
        // not a stylistic choice — it is read off the component references one by one. Condensed
        // carries the dense, numeric and long-string slots (NEED, the compact statement, price,
        // progress, the cash-out band, risk/pays figures, team names); regular carries the market
        // eyebrow, the state chip, the event line and the SCORE figures.
        //
        // NOT YET WIRED: _fontCond is loaded but MakeText still assigns _font to every slot, so the
        // whole surface renders regular. Tracked as TV-19 in docs/tv-sweat-refinement/C14-gap-list.md.
        // Stated here rather than left as a comment that describes an intention as if it were the
        // code — the audit caught an earlier version of this comment claiming call sites that did
        // not exist.
        private Font _font;       // --font-tv           : "Encode Sans"
        private Font _fontCond;   // --font-tv-cond      : "Encode Sans Condensed"

        /// <summary>Which canon face a text slot is set in. Named rather than a bool so a call site
        /// reads as the component reference does, and so adding a third face later is not a
        /// boolean-blindness bug waiting to happen.</summary>
        private enum Face { Regular, Condensed }
        private int _resolvedThrough; // legs below this index are PRESENTED as resolved (not engine truth)
        private Text _tMatchup, _tLeg, _tClock, _tFlavor, _tCashOut, _tChrome, _tAttract, _tBigAmount, _tConsolation;
        private Text _tTicketHeader, _tRiskPays, _tInterventionPrompt, _tTakeoverTitle, _tTakeoverSub, _tSubtitle;
        // TV-03/TV-04: the cash-out slot is three things, not one — an actionable FIELD, the money
        // figure, and a status word at label scale beside it.
        private Image _cashOutField;
        private Text _tCashOutStatus;
        // C3: the score's momentary L4 punch overlay — see BuildScoreBug and OnGoalPlayed.
        private Text _tScoreFlash;
        private Image _backing, _wonFlood, _goldFlood, _dimOverlay;
        // C3: the ball's momentary L4 punch overlay — built unconditionally (never gated behind
        // theaterEnabled) so eligibility does not depend on whether the theater stage exists.
        // TheaterStage.cs owns the real ball actor privately and is outside this phase's file
        // boundary, so this stands in for it at the Stage zone's centre — see BuildCanvas and
        // OnGoalPlayed.
        private Image _ballFlash;

        /// <summary>One ticket-column leg row (DESIGN.md §7: "each live row ... carries its own
        /// NEED and its own revealed progress"). Every slot is built once, in BuildTicketColumn, at
        /// a fixed rect from LayoutGrid.TicketRow(i) — a row's <c>IsLive</c> flag changes what text
        /// and colour it carries, never where it sits.
        ///
        /// <para>T20 split this from one <c>Detail</c> element into two. NEED and progress are
        /// different sizes in canon (28 and 19), and a single <c>Text</c> cannot carry two sizes —
        /// the old <c>$"{Need}\n{Live}"</c> rendered both at 12px, so the re-derivation was not
        /// even expressible before the split. <c>Line</c> is the compact single-line form used by
        /// resolved and pending rows; <c>Need</c>/<c>Progress</c> are the live form. Exactly one of
        /// the two forms carries text at a time.</para>
        ///
        /// <para><b>Deviation from the DD's TvLegRow reference, deliberate and load-bearing:</b> the
        /// live form has NO market/price/state meta line. The reference is a web component whose
        /// rows expand; these rows are a fixed height by approved Layout B law. Canon's three-line
        /// live row costs (15+28+19)*LineBox ≈ 73px against the 70px slot, and reclaiming header and
        /// footer px only reaches ~73 — a knife-edge that clips glyphs in the real font. Two lines
        /// fit with room to spare. State survives the cut because canon itself orders it that way:
        /// "the state is carried by brightness first, by the literal state word second" — a live row
        /// is the pulsing one. Price survives on the compact form and on the ticket card. Flagged to
        /// the Design Director rather than absorbed silently.</para></summary>
        private struct LegRowUi
        {
            // TV-14: the compact form is THREE spans, not one string. Canon
            // (TvLegRow.jsx:56-63) sets statement · price · state chip, and the build concatenated
            // them into `"NEXT   {statement} {price}"` — one colour, state word leading. That is
            // wrong twice over: the price must carry --tv-context rather than inherit the row's
            // state hue, and the state belongs in a right-aligned chip, not in front of the fact.
            // The statement is what the row is FOR; it should start at the row's left edge.
            public Text Line;      // compact: the authored statement
            public Text Price;     // compact: the price, --tv-context, never the state hue
            public Text State;     // compact: the right-aligned state chip
            public Text Need;      // live: the authored §6 statement, printed verbatim
            public Text Progress;  // live: the revealed causal progress line
            public Image Strike;      // §8 VOID only: the struck-through rule
            public Image Extinguish;  // TV-21: a LOST row's unlit background, --tv-extinguished
            public bool IsLive;
        }

        private LegRowUi[] _legRow;

        // ---- theater (F_0.2.0) ----
        private TheaterStage _stage;
        // T16 (Design Director ruling): the momentum tape is back — restored at the foot of the
        // scorebug, no numerals, no hue, never above L2. See BuildCanvas (construction, inside the
        // theaterEnabled block alongside _stage), BeginStageLeg/ResetForNewSession (Show), RenderEvent
        // (_pendingTapeBeat), RevealBeatChrome (AppendBeat), and FinalSlam (ResolveLeg).
        private MomentumTape _tape;
        private bool _pendingTapeBeat;
        private Transform _canvasRoot;
        private float _canvasWidth;
        private float _canvasHeight;
        private readonly SweatPresentationModel _presModel = new SweatPresentationModel();
        private readonly ScoreLedger _ledger = new ScoreLedger();
        private CountLedger _countLedger;
        private TheaterChoreographer _choreo;
        // Phase 2C (PRD §9): the planner elaborates the choreographer's factual SceneSpec into a
        // TheaterScenePlan; TheaterStage executes that plan. This screen is the session
        // orchestrator, so it owns the PRD §7.4 repetition-control history across the WHOLE
        // session's beats (every leg, every ticket) — constructed once, never reset at a leg or
        // ticket boundary, because "the same move again" is a couch-viewer judgment that does not
        // reset just because a new leg started; the ring buffer's own capacity naturally ages out
        // stale entries. Recorded into only once a plan is actually accepted for playback — the
        // planner deliberately never records for itself (see TheaterScenePlanner's type doc).
        private readonly TheaterScenePlanner _scenePlanner = new TheaterScenePlanner();
        private readonly TheaterSceneHistory _sceneHistory = new TheaterSceneHistory();
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
            _fontCond = LoadFontCondensed();
            _choreo = new TheaterChoreographer(pacer);
            _emissBlock = new MaterialPropertyBlock();
            _emissSeed = UnityEngine.Random.value * 100f;

            _emissIdle = emissiveScreen != null && emissiveScreen.sharedMaterial != null
                ? emissiveScreen.sharedMaterial.GetColor(EmissionColorId)
                // Defensive fallback only (no emissiveScreen wired) — neutral cold-dim, not the old
                // green-tinted (0.010, 0.045, 0.020) guess.
                : new Color(0.012f, 0.014f, 0.018f);
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
            // §8.10: "Standing while held cancels the preview and freezes per §4.4. The preview is
            // not a way to hold the sweat still." Cancelled BEFORE the freeze below, so the reverted
            // rows are what gets frozen — freezing a previewed ticket would leave a standing player
            // looking at struck legs that were never actually cashed out.
            if (!seated) ExitCashOutPreview();
            if (_stage != null) _stage.SetFrozen(!_seated);
        }

        /// <summary>PRD §8.10. Enters only when the offer could be ACCEPTED right now — the gate is
        /// <see cref="CanAcceptCashOutNow"/>, exactly as repaired in TVS-H01: "if cash-out cannot be
        /// accepted right now, it cannot be previewed right now." That is also what keeps the
        /// previewed amount and the acceptable amount the same number, since a mid-tween offer is
        /// refused by both.</summary>
        private bool EnterCashOutPreview()
        {
            if (_cashOutPreview) return true;
            if (!CanAcceptCashOutNow()) return false;
            double? offer = _session.CashOutOffer();
            if (!offer.HasValue) return false;
            _cashOutPreview = true;
            _cashOutPreviewAmount = offer.Value;
            // "the ticket in its cashed-out state; the accepted amount, which is the amount
            // currently displayed." Snapshot-and-restore for this one field, because unlike the rows
            // and the bank the slot's text is written at discrete moments rather than rebuilt every
            // frame — there is no render pass to make preview-aware.
            if (_tCashOut != null)
            {
                _cashOutTextBeforePreview = _tCashOut.text;
                _tCashOut.text = $"CASHED OUT ${Money(_cashOutPreviewAmount)}";
            }
            UpdateTicketColumn(_liveLegIndexShown);
            return true;
        }

        /// <summary>Full revert (§8.10). Clearing the flag and re-rendering is the whole revert:
        /// every row is recomputed from leg state, so no residue can survive by construction.
        /// Idempotent — a release, a stand, and a settle may all call it for the same preview.</summary>
        private void ExitCashOutPreview()
        {
            if (!_cashOutPreview) return;
            _cashOutPreview = false;
            _cashOutPreviewAmount = 0.0;
            if (_tCashOut != null) _tCashOut.text = _cashOutTextBeforePreview;
            _cashOutTextBeforePreview = string.Empty;
            UpdateTicketColumn(_liveLegIndexShown);
        }

        /// <summary>Steps a colour exactly one brightness level, by the ratio of DESIGN.md §3's
        /// tiers. Alpha only: the hue is the element's role and a level change must not restate it.</summary>
        private static Color SteppedDown(Color c, float fromTier, float toTier)
        {
            c.a *= toTier / fromTier;
            return c;
        }

        /// <summary>The bank as the preview quotes it (§8.10: "the bank at its post-cash-out
        /// value"). Derived from the same offer the slot displays plus the player's own balance —
        /// never from a locked endpoint, which is what keeps the preview admissible under §4.1: it
        /// previews a CONSEQUENCE of this action, not a match fact.</summary>
        private double PreviewedBank(Run r)
            => _cashOutPreview ? r.Bank + _cashOutPreviewAmount : r.Bank;

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
                        // T27: "PLACE YOUR BETS" was banned on two counts — celebratory exhortation,
                        // and a retired hue at L4. The idle screen states where the run is and
                        // nothing else; the TV never instructs the player to bet. moneyIdle is off
                        // because the ruling is explicit that the bar carries no hue.
                        //
                        // T25.4 takes the subtitle with it: "the book is open on the laptop" sat at
                        // the visual centre of the match theatre at fact brightness in every frame.
                        // Satire never occupies a slot where a fact belongs.
                        RenderIdle("betting",
                            $"ROUND {director.Run.Round} OF {director.Run.Config.Rounds} · BOARD OPEN",
                            string.Empty, moneyIdle: false);
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
            _tAttract.color = flavorColor; // an instructional prompt, not money — §4 Fact: cold white
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
                // PRD §9 diagnostic — set from the spec the stage is about to play, at every one of the
            // three resolution sites, so a capture frame can name its own grammar (T26).
            SceneSpec spec = _choreo.ResolveBeat(evt, _lastBeatUp, _lastBeatDelta, _ledger,
                    leg, _countLedger);
                DebugSceneTemplate = spec.Template.ToString();
                // Phase 2C: the planner elaborates this factual spec into a rich, deterministic
                // TheaterScenePlan (PRD §9) — it never changes spec's truth contract, only picks
                // grammar/pressure/spacing/payoff/reaction/lane from the presentation key. The
                // beat is unconditionally staged below (no rejection path from here), so the plan
                // is "accepted for playback" the instant it is built — recorded right here, per
                // TheaterSceneHistory's own contract that only an accepting caller ever records
                // (the planner deliberately never records for itself).
                PresentationSceneKey sceneKey = BuildSceneKey(evt, spec, leg);
                TheaterScenePlan scenePlan = _scenePlanner.Plan(spec, sceneKey, _sceneHistory);
                _sceneHistory.Record(scenePlan.Signature, scenePlan.FactContract,
                    scenePlan.FactContract == SceneFactContract.Structural);
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
                // goal call, keyed to the goal's beneficiary, never the tie-broken beat
                // direction (a flat floor beat reconciles for the opponent while the
                // tie-break says "up").
                if (spec.Goal.HasValue)
                {
                    if (evt.Type == DramaEventType.Momentum)
                        _pendingFlavor = SweatFlavor.GoalLine(spec.Goal.Value.ForPicked, leg, evt.Step);
                    // TVS-H03 fix: the old PrepareScoringActor/SetScoringActor call here was a
                    // no-op for every anytime-scorer leg (ScorerFor already suppressed identity
                    // pre-final) and, for every other market, stamped a GameObject.name nothing
                    // ever read (Phase 1A: "zero read-side connection to EnterStep/CompleteStep's
                    // ...route/carrier selection"). Removed rather than left as a second, inert
                    // "scorer actor" mechanism alongside the real plan-time binding below — see
                    // ScoreLedger.BindAnytimeScorer and TheaterStage.EnterStep's RoutePass case.
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
                    _stage.PlayPlannedScene(scenePlan, spec, OnGoalPlayed,
                        nearMiss || spec.Count.HasValue ? RevealBeatAudio : RevealBeatChrome, null,
                        OnCountPlayed);
                }
                else
                {
                    RevealBeatChrome(); // low-information beat: prob drifts, price stays live
                    _stage.PlayPlannedScene(scenePlan, spec, OnGoalPlayed, null, null, OnCountPlayed);
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
                // Phase 2C: no SceneSpec drives rendering for a suspended continuation (it stays
                // on BuildContinuationScript, untouched), but the beat still belongs in the shared
                // repetition-control history — otherwise a final that follows a suspended shot
                // would silently vanish from what §7.4's rules can see. The null-ledger
                // ResolveFinal overload is side-effect-free (it never reads _ledger/_countLedger),
                // so this cannot double-consume either ledger ahead of the real planning below.
                SceneSpec plannedFinalSpec = _choreo.ResolveFinal(grade, evt.Step);
                DebugSceneTemplate = plannedFinalSpec.Template.ToString();
                PresentationSceneKey finalKey = BuildSceneKey(evt, plannedFinalSpec, leg);
                TheaterScenePlan finalScenePlan = _scenePlanner.Plan(plannedFinalSpec, finalKey, _sceneHistory);
                _sceneHistory.Record(finalScenePlan.Signature, finalScenePlan.FactContract, false);

                ScoreLedger.FinalPlan plan = _ledger.PlanFinal(grade);
                // TVS-S01 fix: PlanFinal derives each remaining batch's team attribution from
                // its own HomeDelta/AwayDelta now — no bet-derived flag to compute here.
                // TVS-H03 fix: bound here, at plan time, before ResumeSuspended ever plays a
                // frame — the reveal copy (ScorerFor) and the stage's actor routing both read
                // this exact binding, never a post-hoc reconciliation of the two.
                plan = ScoreLedger.BindAnytimeScorer(plan, leg);
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
                DebugSceneTemplate = spec.Template.ToString();
                // Phase 2C: history bookkeeping only (see the pending-loss branch above for why) —
                // the Final fact contract's catalog is structurally degenerate (PRD §7.2: exactly
                // one legal candidate per grade), so there is nothing for plan-shaping to vary;
                // BuildFinalScript stays untouched.
                PresentationSceneKey finalKey = BuildSceneKey(evt, spec, leg);
                TheaterScenePlan finalScenePlan = _scenePlanner.Plan(spec, finalKey, _sceneHistory);
                _sceneHistory.Record(finalScenePlan.Signature, finalScenePlan.FactContract, false);

                ScoreLedger.FinalPlan plan = _ledger.PlanFinal(grade);
                // TVS-H03 fix: see the ResumeSuspended branch above — same plan-time binding.
                plan = ScoreLedger.BindAnytimeScorer(plan, leg);
                _stage.PlayFinalScene(spec, plan, spec.CountFinal, OnGoalPlayed, OnCountPlayed, null);
                yield return WaitSceneDone();
                yield return FinalSlam(evt, grade);
            }
        }

        /// <summary>PRD §4.3's presentation key, built from whatever this beat's SceneSpec and
        /// Leg actually carry. Match index is <c>Leg.Matchup.Index</c> — §4.3's amendment (match-
        /// scoped, not leg-scoped; see <see cref="PresentationSceneKey"/>'s type doc). Beneficiary
        /// prefers the count fact's explicit home/away beneficiary when this is a count scene,
        /// falling back to <c>ForPicked</c> otherwise — mirroring the field's own documented
        /// "which side benefits" convention. Never touches engine RNG.</summary>
        private PresentationSceneKey BuildSceneKey(DramaEvent evt, SceneSpec spec, Leg leg)
        {
            Run run = director != null ? director.Run : null;
            string seed = run != null ? run.Rng.RunSeed : string.Empty;
            int round = run != null ? run.Round : 0;
            int ticket = director != null ? director.SweatIndex : 0;
            int match = leg != null && leg.Matchup != null ? leg.Matchup.Index : 0;
            bool beneficiary = spec.CountBeneficiaryIsHome ?? spec.ForPicked;
            return new PresentationSceneKey(seed, round, ticket, match, evt.Step, spec.Template, beneficiary);
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
            if (grade == LegGrade.Won) _probTarget = 1f;
            else if (grade == LegGrade.Lost) _probTarget = 0f;
            RevealedView.SetProbability(_probTarget);
            RevealedView.ResolveLeg(evt.LegIndex, grade);
            _tape?.ResolveLeg(evt.LegIndex, grade); // T16: collapses the strip to its resolution cap

            _resolvedThrough = evt.LegIndex + 1;
            UpdateTicketColumn(evt.LegIndex + 1);
            int k = evt.LegIndex + 1;
            if (grade == LegGrade.Won)
            {
                _audio?.Whistle();
                _audio?.SlamWon();
                yield return WonLegBeat(k);
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

            // C3 (Design Director ruling): every staged goal attempt reaches its payoff HERE,
            // whether or not it commits. A commit means the score itself is the story — DESIGN.md
            // §5: "Score numerals ... L3, L4 at a goal" — so the score punches to L4. A chalk-off
            // has no score change to headline, so the ball's payoff punch takes the moment instead
            // — DESIGN.md §7: "the ball is the only object permitted L4, and only at a payoff."
            // Mutually exclusive by construction (never both for the same goal), which is what
            // keeps the one-token invariant from ever having to arbitrate between these two.
            if (goal.Commits && _ticket != null && _stageLeg >= 0 && _stageLeg < _ticket.Legs.Count)
                PlayScorePunch(ScoreOnlyLine(_ticket.Legs[_stageLeg]));
            else if (!goal.Commits)
                PlayBallPunch();

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
                // §4: money/won is gold, not the retired saturated green — LaptopOs.MoneyGood is
                // the laptop OS's own retired-green token and has no role on this surface.
                _tFlavor.color = pickedScorer ? new Color(gold.r, gold.g, gold.b, 1f) : flavorColor;
                // T44: "THAT'S YOUR MAN" addressed the reader. CF: "Copy is impersonal and
                // transactional — it names the thing, not the reader"; second person is reserved for
                // genuine instructions, and this is a fact about the goal. "BACKED" is the surface's
                // own established word for it (§7.7's backed-player locator, T23), so the fact
                // survives the rewrite intact — the strip states who scored and what he was to the
                // ticket, which is strictly more information than the possessive carried.
                _tFlavor.text = pickedScorer
                    ? Surname(scorer.Name) + " STRIKES — THE BACKED SCORER"
                    : Surname(scorer.Name) + " FINDS THE NET";
                _flavorScale = 1.12f;
                // SweatActiveLegModel's ScorerRevealed gate: true only at this exact causal
                // identity payoff, matching the model's own documented contract.
                if (pickedScorer) _scorerRevealedForActiveLeg = true;
            }
        }

        /// <summary>The score-only numerals (no team names — PRD §4.2's "one revealed source of
        /// truth": the SAME <c>_ledger.Picked</c>/<c>Opponent</c> fields UpdateScorebug already
        /// reads, never re-derived). Feeds the momentary C3 score punch.</summary>
        private string ScoreOnlyLine(Leg leg)
        {
            bool pickedHome = SweatFlavor.PickedHomeForPresentation(leg);
            int homeScore = pickedHome ? _ledger.Picked : _ledger.Opponent;
            int awayScore = pickedHome ? _ledger.Opponent : _ledger.Picked;
            return $"{awayScore} — {homeScore}";
        }

        /// <summary>C3: "the score at a goal" — a momentary L4 punch, layered over Matchup's own
        /// persistent L3 score truth for <see cref="hdrPunchDuration"/>, then released.</summary>
        private void PlayScorePunch(string scoreText)
        {
            if (_tScoreFlash == null) return;

            // T38: the punch mirrors what is ALREADY on screen — it never carries a string of its
            // own. It used to be handed the INCOMING score while _tMatchup still showed the
            // outgoing one, two scorelines at the same rect, same size, one gold over one white.
            // Different digits superimposed read as a crossed-out score: the margin's rub-out
            // gesture, on a scoreboard, where a number is either true or it is not.
            //
            // DESIGN.md §9: "State changes are quantised — a brightness level swaps in a discrete
            // step." The score swaps discretely and the punch is BRIGHTNESS ONLY. Sourcing the text
            // from _tMatchup rather than the caller makes that structural: this overlay cannot
            // display a value the surface is not already showing, whatever a future caller passes.
            _tScoreFlash.text = _tMatchup != null ? _tMatchup.text : scoreText;
            _tScoreFlash.enabled = true;
            StartCoroutine(ScorePunchRoutine());
        }

        private IEnumerator ScorePunchRoutine()
        {
            RequestL4(HdrFocus.Score, momentary: true);
            yield return ScaledWait(hdrPunchDuration);
            ReleaseL4(HdrFocus.Score);
            if (_tScoreFlash != null) _tScoreFlash.enabled = false;
        }

        /// <summary>C3: "the ball at a payoff" — a momentary L4 punch centred on the Stage zone
        /// for <see cref="hdrPunchDuration"/>, then released.</summary>
        private void PlayBallPunch()
        {
            if (_ballFlash == null) return;
            _ballFlash.enabled = true;
            StartCoroutine(BallPunchRoutine());
        }

        private IEnumerator BallPunchRoutine()
        {
            RequestL4(HdrFocus.Ball, momentary: true);
            yield return ScaledWait(hdrPunchDuration);
            ReleaseL4(HdrFocus.Ball);
            if (_ballFlash != null) _ballFlash.enabled = false;
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
        /// keyboard (batch tests) the window declines immediately, so autoplay never hangs.
        ///
        /// PRD §8.7 / DESIGN.md §8.5: "intervention controls live in their own overlay, never in
        /// [the cash-out] row." The cash-out slot stays MARKET SUSPENDED (structureGrey, L1) for
        /// the duration; the M/R/N verbs render on the separate InterventionPrompt element.</summary>
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
            // T43: §8.5 Pending window: "As suspended" — L1 unlit slate. This site used to hand-set
            // the word and its colour and nothing else, which is why the slate never reached the
            // field, the status word or the L4 token here. One call, one slate, both sites.
            ShowMarketSuspended();
            _tInterventionPrompt.enabled = true;
            _tInterventionPrompt.color = new Color(gold.r, gold.g, gold.b, 1f);
            _tInterventionPrompt.text = "SHOT FROZEN\n" + verbs + "[N] LET IT DIE";

            while (_session.HasPendingLoss)
            {
                if (_seated && canM && Keyboard.current.mKey.wasPressedThisFrame)
                {
                    director.Run.PlayMulliganSlip(_session);
                    HideCashOutSlot(); // T43: the field and status leave with the figure, same frame
                    _tInterventionPrompt.enabled = false;
                    _tFlavor.color = chromeCyan; // §8 VOID — the mulligan voids the leg, not chrome
                    _tFlavor.text = "THE SLIP COMES OUT — LEG VOIDED, THE TICKET LIVES";
                    _emissRest = _emissIdle; // the DEAD dim lifts: the ticket breathes again
                    tvLight?.ResetToIdle();
                    UpdateTicketColumn(Mathf.Min(_resolvedThrough, _ticket.Legs.Count - 1));
                    yield return ScaledWait(deadLineDuration);
                    yield break;
                }
                if (_seated && canR && Keyboard.current.rKey.wasPressedThisFrame)
                {
                    director.Run.PlayRefsWhistle(_session);
                    HideCashOutSlot(); // T43
                    _tInterventionPrompt.enabled = false;
                    if (!_session.IsComplete)
                    {
                        // The leg is reinstated live — a fact, not a payout yet. §4 Fact: cold white.
                        _tFlavor.color = flavorColor;
                        _tFlavor.text = "REVIEWED — OVERTURNED. THE LEG STANDS.";
                        _emissRest = _emissIdle;
                        tvLight?.ResetToIdle();
                        UpdateTicketColumn(Mathf.Min(_resolvedThrough, _ticket.Legs.Count - 1));
                    }
                    else
                    {
                        // A loss confirmed — context, not a hue. §4/§8: loss is darkness, never red;
                        // the text itself still has to stay legible, so it reads in grey, not black.
                        _tFlavor.color = contextGrey;
                        _tFlavor.text = "REVIEWED — THE CALL IS CONFIRMED.";
                    }
                    yield return ScaledWait(deadLineDuration);
                    yield break;
                }
                if (_seated && Keyboard.current.nKey.wasPressedThisFrame)
                {
                    _session.DeclinePendingLoss();
                    HideCashOutSlot(); // T43
                    _tInterventionPrompt.enabled = false;
                    yield break;
                }
                yield return null;
            }
            HideCashOutSlot(); // T43
            _tInterventionPrompt.enabled = false;
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
            CleanupConfetti();
            StopCashOutAnimation();
            _hasCashOutShown = false;
            _cashOutShown = 0.0;
            _cashOutScale = 1f;
            _cashOutFlash = 0f;
            _stageLeg = -1;
            _stageBeatCount = 0;
            _countLedger = null;
            _finalSequenceActive = false;
            _audioUrgency = 0f;
            _stoppageGoalCount = 0;
            _marketSuspended = false;
            _scorerRevealedForActiveLeg = false;
            RevealedView.Reset(director != null ? director.Run : null, _ticket,
                director != null ? director.SweatIndex : 0);
            _tCashOut.color = new Color(gold.r, gold.g, gold.b, 1f);
            _tInterventionPrompt.enabled = false;
            _stage?.Show(false);
            // T16: fresh ticket, fresh tape — a stale strip from the last ticket must not survive.
            _tape?.ResetForTicket(_ticket != null ? _ticket.Legs.Count : 0);
            _tape?.Show(false);
            _pendingTapeBeat = false;
            // C3: no punch overlay survives across a session boundary.
            if (_tScoreFlash != null) _tScoreFlash.enabled = false;
            if (_ballFlash != null) _ballFlash.enabled = false;

            _emissRest = _emissIdle;
            tvLight?.ResetToIdle();

            SetAlpha(_wonFlood, 0f);
            SetAlpha(_goldFlood, 0f);
            SetAlpha(_dimOverlay, 0f);
            _tBigAmount.text = string.Empty;
            if (_tConsolation != null) _tConsolation.enabled = false;
            HideCashOutSlot(); // T43
            _tCashOut.rectTransform.localScale = Vector3.one;
            _tAttract.enabled = true;
            _tFlavor.color = flavorColor;
            // A new session starts with no L4 element live — see AnimateCashOutTaunt/WinBeat/
            // CashOutFloodBeat/OnGoalPlayed for where these get pushed back above HdrBoostL3.
            ResetL4();

            RenderPregame();
        }

        private void RenderPregame()
        {
            if (_ticket == null || _ticket.Legs.Count == 0) return;
            Leg leg = _ticket.Legs[0];
            _tMatchup.text = MatchupLine(leg);
            _tLeg.text = $"LEG 1/{_ticket.Legs.Count}";
            _tClock.text = "PRE";
            // T44 casing: CF puts state words in tracked uppercase, and every sibling state line on
            // this element is (VAR — NO GOAL, THE TOTEM BURNS, LEG n — WON). Lowercase sentence case
            // is for the beat corpus's running text, which this is not.
            _tFlavor.text = "THE BOARD IS SET";
            _probTarget = (float)leg.TrueProb; // data only — RevealedView, no visible bar
            // The ticket-card takeover copy clears the instant the live sweat begins.
            _tTakeoverTitle.text = string.Empty;
            _tTakeoverSub.text = string.Empty;
            _resolvedThrough = 0;
            UpdateTicketColumn(0);
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
            _scorerRevealedForActiveLeg = false; // fresh leg = fresh scorer-identity gate
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
            // §7.7: the backed player is marked for the WHOLE leg, set here at kickoff rather than at
            // the reveal — "continuous, not reveal-only". The identity comes from the leg's own
            // Selection.PlayerIndex, exactly as BindAnytimeScorer derives it, so the locator and the
            // payoff routing cannot disagree. Cleared on every non-scorer leg: a locator on a
            // moneyline leg would point at a player the bet does not depend on.
            //
            // This is the BINDING half only. The visible treatment (numeral vs ring vs halo) is
            // reserved to the design track by §7.7 and is not decided here — see TheaterStage's
            // _backedActor fields for why the two candidates are not interchangeable in this engine.
            if (leg.Selection.Kind == MarketKind.AnytimeScorer)
            {
                bool backedIsHome = leg.Matchup.PlayerSide(leg.Selection.PlayerIndex) == Side.Home;
                int rosterIndex = backedIsHome
                    ? leg.Selection.PlayerIndex - leg.Matchup.Away.Players.Count
                    : leg.Selection.PlayerIndex;
                _stage.SetBackedPlayer(backedIsHome, rosterIndex);
            }
            else
            {
                _stage.ClearBackedPlayer();
            }

            // T42: the pitch dots take canon's two muted hues, home = team-a, away = team-b. This is
            // the ONE place on this surface where a team hue is legal at all (palette-tv.css:4,
            // "confined to the pitch dots"), so it is also the only place that reads these fields.
            _stage.Show(true);
            _stage.BeginLeg(teamHueA, teamHueB,
                pickedIsHome: SweatFlavor.PickedHomeForPresentation(leg), openingProb: (float)leg.TrueProb);
            RevealedView.BeginLeg(legIndex, leg);
            UpdateScorebug(leg);
            _tape?.Show(true); // T16
        }

        /// <summary>The theater scorebug (M-T3, playtest #10 finding #2 pulled forward from
        /// chrome v2): team names IN their dot colors so the stage is instantly attributable,
        /// the running synthesized score between them, the picked side marked. Away @ home
        /// order, matching the slate's convention everywhere else.
        ///
        /// DESIGN.md §7 / PRD §8.3: "Records are removed from the primary scorebug during live
        /// playback." No RecordsLine/market-chip suffix here — the market chip's information now
        /// lives in the ticket column's active-leg row (SweatActiveLegModel), which is the
        /// PRD-sanctioned channel for "what does this leg need".</summary>
        private void UpdateScorebug(Leg leg)
        {
            if (_tMatchup == null) return;
            // T42/T32.1: the scorebug fetched both team RGBs here and, since the hue came off the
            // names, used neither. A live handle to a hue in the one zone canon forbids it in is how
            // the violation returns — retired with the rule, not left "harmless".
            bool pickedHome = SweatFlavor.PickedHomeForPresentation(leg);
            int homeScore = pickedHome ? _ledger.Picked : _ledger.Opponent;
            int awayScore = pickedHome ? _ledger.Opponent : _ledger.Picked;

            string away = SweatFlavor.Short(leg.Matchup.Away.Name).ToUpperInvariant();
            string home = SweatFlavor.Short(leg.Matchup.Home.Name).ToUpperInvariant();
            // The pick dot marks YOUR TEAM — a market leg has none, so it wears no dot
            // (the ticket column's leg row carries the pick for market legs).
            bool isMl = leg.Selection.Kind == MarketKind.Moneyline;
            string awayMark = isMl && !pickedHome ? "● " : "";
            string homeMark = isMl && pickedHome ? " ●" : "";
            // T32.1 / T25.2: the scoreline carries NO team hue. §4 is explicit — facts are cold
            // white, identity is carried by the words in the ticket column, and the two muted team
            // hues are confined to the pitch dots. The names were injected here as
            // <color=#RRGGBB> markup straight from the saturated team pool, which is both the wrong
            // hue and the wrong place: markup is still palette, and a colour system a string can
            // bypass is not enforced. The element's own --tv-fact colour now carries the whole line.
            _tMatchup.text = $"{awayMark}{away}  {awayScore} — {homeScore}  {home}{homeMark}";
            RevealedView.SetScore($"{away} {awayScore} — {home} {homeScore}");
        }

        /// <summary>Builds this leg's revealed-only <see cref="SweatActiveLegModel.ActiveLegInput"/>
        /// and formats it. PRD §8.2/§9: every value passed here is the SAME revealed field the
        /// scorebug already renders from (<c>_ledger.Picked/Opponent</c>, <c>_countLedger.Home/
        /// Away</c>) — never re-derived from <c>Leg</c>/<c>ScoreLedger</c>/<c>CountLedger</c>/
        /// <c>MatchStatLine</c> directly, and never a locked endpoint. The model's own factory
        /// signatures (plain int/double/bool/string) make that the only thing this method CAN
        /// pass, by construction.</summary>
        private SweatActiveLegModel.ActiveLegCopy DescribeActiveLeg(Leg leg)
        {
            switch (leg.Selection.Kind)
            {
                case MarketKind.Moneyline:
                {
                    bool pickedHome = SweatFlavor.PickedHomeForPresentation(leg);
                    string team = SweatFlavor.Short(pickedHome ? leg.Matchup.Home.Name : leg.Matchup.Away.Name);
                    return SweatActiveLegModel.Describe(
                        SweatActiveLegModel.ActiveLegInput.Moneyline(team, _ledger.Picked, _ledger.Opponent));
                }
                case MarketKind.TotalGoals:
                    return SweatActiveLegModel.Describe(SweatActiveLegModel.ActiveLegInput.TotalGoals(
                        leg.Selection.Choice == MarketChoice.Over, leg.Selection.Line,
                        _ledger.Picked, _ledger.Opponent));
                case MarketKind.BothTeamsToScore:
                    return SweatActiveLegModel.Describe(SweatActiveLegModel.ActiveLegInput.BothTeamsToScore(
                        leg.Selection.Choice == MarketChoice.Yes, _ledger.Picked, _ledger.Opponent));
                case MarketKind.TotalCorners:
                    return SweatActiveLegModel.Describe(SweatActiveLegModel.ActiveLegInput.TotalCorners(
                        leg.Selection.Choice == MarketChoice.Over, leg.Selection.Line,
                        _countLedger?.Home ?? 0, _countLedger?.Away ?? 0));
                case MarketKind.TotalCards:
                    return SweatActiveLegModel.Describe(SweatActiveLegModel.ActiveLegInput.TotalCards(
                        leg.Selection.Choice == MarketChoice.Over, leg.Selection.Line,
                        _countLedger?.Home ?? 0, _countLedger?.Away ?? 0));
                case MarketKind.AnytimeScorer:
                {
                    Player player = leg.Matchup.PlayerAt(leg.Selection.PlayerIndex);
                    return SweatActiveLegModel.Describe(SweatActiveLegModel.ActiveLegInput.AnytimeScorer(
                        player.Name, _scorerRevealedForActiveLeg));
                }
                default:
                    return new SweatActiveLegModel.ActiveLegCopy(string.Empty, string.Empty, false, string.Empty);
            }
        }

        private Player ScorerFor(ScoreLedger.StagedGoal goal, Leg leg)
        {
            if (!goal.Commits || leg == null || leg.Matchup.StatLine == null) return null;
            // TVS-H03 fix: an anytime-scorer leg's identity is bound at PLAN time
            // (ScoreLedger.BindAnytimeScorer, called before ResumeSuspended/PlayFinalScene ever
            // plays a frame) directly onto the one goal that carries it. Read that binding here,
            // verbatim — never the old HomeScorers/AwayScorers[index] reconstruction, which had
            // no causal link to which actor the stage was about to animate as the shot-taker.
            // Identity stays suspended until the final sequence (the payoff moment) exactly as
            // before: BindAnytimeScorer only ever runs from the LegFinal branch, and
            // StageBeatGoal (the only producer of a pre-final StagedGoal) never sets
            // HasBoundScorer, so the !_finalSequenceActive guard below is now belt-and-braces,
            // not the only thing preventing an early reveal.
            if (leg.Selection.Kind == MarketKind.AnytimeScorer)
            {
                if (!_finalSequenceActive || !goal.HasBoundScorer) return null;
                var bound = goal.ScorerIsHome ? leg.Matchup.Home.Players : leg.Matchup.Away.Players;
                return goal.ScorerRosterIndex >= 0 && goal.ScorerRosterIndex < bound.Count
                    ? bound[goal.ScorerRosterIndex] : null;
            }
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

        // T42: `TeamColor(Leg, bool)` lived here and had no callers — a ready-made saturated hue for
        // whatever wanted one next. Deleted rather than kept: the surface's only legal team hue is
        // teamHueA/teamHueB on the stage dots, and there is now exactly one call site for it.

        /// <summary>The ticket column (DESIGN.md §6/§7, PRD §8.1/§8.2/§8.4): header (ticket
        /// index), <see cref="TicketRowSlots"/> fixed leg-row slots in ticket order, and the
        /// RISK/PAYS footer. Every row's RECT is fixed (LayoutGrid.TicketRow); only its text and
        /// colour change with state — DESIGN.md §8's Leg-state table decides which:
        /// NEXT = L1 structureGrey, LIVE = L3 flavorColor (pulsed by AnimateLegPulse), W = L3
        /// gold solid, L = L0 deadDark, VOID = L2 chromeCyan. The live row additionally carries
        /// its NEED/LIVE line from <see cref="DescribeActiveLeg"/> (SweatActiveLegModel).
        ///
        /// PRD §8.2A tolerance: this reads legs as a collection and checks each row's own live
        /// state independently (never a single hard-coded "the active leg" index), so it does not
        /// need a rewrite if concurrent live legs are ever re-authorized — today at most one row
        /// is ever live, since the engine forbids two legs on one matchup.</summary>
        private void UpdateTicketColumn(int liveLegIndex)
        {
            _liveLegIndexShown = liveLegIndex;
            if (_ticket == null)
            {
                _tTicketHeader.text = string.Empty;
                for (int i = 0; i < _legRow.Length; i++) ClearLegRow(i);
                _tRiskPays.text = string.Empty;
                return;
            }

            _tTicketHeader.text = director != null
                ? $"TICKET {director.SweatIndex + 1}/{director.Run.Sweats.Count}"
                : string.Empty;

            for (int i = 0; i < _legRow.Length; i++)
            {
                if (i >= _ticket.Legs.Count) { ClearLegRow(i); continue; }

                Leg leg = _ticket.Legs[i];
                // TV-14: statement and price are separate facts and render as separate spans. They
                // were concatenated into one string, which forced the price to wear whatever hue
                // the row's state carried — a lost leg's price rendered as part of the loss.
                string statement = leg.DisplayLabel;
                string price = OddsFormat.American(leg.OfferedOdds);
                bool isLive = i == liveLegIndex && i >= _resolvedThrough;
                _legRow[i].IsLive = isLive;

                if (i < _resolvedThrough)
                {
                    // TV-S1/TV-20/TV-21: every state states its tier here. The ladder is the primary
                    // semantic channel and was previously declared but never applied, so every one
                    // of these rendered at identical maximum brightness.
                    bool dead = !leg.IsVoided && !leg.GradesWon;
                    if (leg.IsVoided)
                    {
                        _legRow[i].Line.color = AtTier(tvVoid, TierL2);   // §8 VOID: L2, --tv-void
                        _legRow[i].Line.text = statement;
                        SetRowChip(i, "VOID", AtTier(tvVoid, TierL2), price);
                    }
                    else if (leg.GradesWon)
                    {
                        _legRow[i].Line.color = AtTier(gold, TierL3);     // §8 W: L3 gold
                        _legRow[i].Line.text = statement;
                        SetRowChip(i, "W", AtTier(gold, TierL3), price);
                    }
                    else
                    {
                        // A dead row's TEXT sits at L1, not L0 (TvLegRow.jsx:22) — L0 on the text
                        // would erase the "unlit pixel structure" the law asks to keep. The
                        // extinguishment is the row's background, below.
                        _legRow[i].Line.color = AtTier(flavorColor, TierL1);
                        _legRow[i].Line.text = statement;
                        SetRowChip(i, "L", AtTier(flavorColor, TierL1), price);
                    }
                    if (_legRow[i].Extinguish != null) _legRow[i].Extinguish.enabled = dead;
                    // §8: the strike belongs to VOID and to nothing else. A struck W or L would
                    // read as cancelled, which is the one thing the strike must never say.
                    if (_legRow[i].Strike != null) _legRow[i].Strike.enabled = leg.IsVoided;
                    _legRow[i].Need.text = string.Empty;
                    _legRow[i].Progress.text = string.Empty;
                }
                else if (isLive)
                {
                    // §8 LIVE: L3 white — AnimateLegPulse drives the one permitted pulse, which is
                    // what carries "live" now that the row has no state word of its own (T20; canon:
                    // "the state is carried by brightness first, by the literal state word second").
                    //
                    // The compact line is blanked rather than reused: it printed leg.DisplayLabel,
                    // which IS the authored statement, so leaving it would print the statement twice
                    // at two different sizes. NEED is the one place it appears on a live row.
                    SweatActiveLegModel.ActiveLegCopy copy = DescribeActiveLeg(leg);
                    // The live form replaces the compact one entirely — statement, price and chip
                    // all clear. §7 bans duplicating a fact already on the surface, and the live
                    // row's NEED carries the statement (T24: "state survives in the word, price in
                    // the compact form").
                    _legRow[i].Line.text = string.Empty;
                    SetRowChip(i, string.Empty, flavorColor, string.Empty);
                    // §8.10: while previewing, a remaining leg is struck (cashing out ends it) and
                    // drops ONE level — L3 to L2. It uses the VOID strike, never the LOST
                    // extinguish: a leg being CANCELLED must not read as a leg LOST at the exact
                    // moment the player is deciding whether to cancel it.
                    // §8 LIVE: L3. Previewing steps it one level to L2 (§8.10).
                    Color liveInk = AtTier(flavorColor, _cashOutPreview ? TierL2 : TierL3);
                    if (_legRow[i].Strike != null) _legRow[i].Strike.enabled = _cashOutPreview;
                    if (_legRow[i].Extinguish != null) _legRow[i].Extinguish.enabled = false;
                    _legRow[i].Need.color = liveInk;
                    _legRow[i].Need.text = copy.Need;         // §6 verbatim — never paraphrased
                    _legRow[i].Progress.color = liveInk;
                    _legRow[i].Progress.text = copy.Live;
                }
                else
                {
                    // T25.6: NEXT rows go to L2, NOT L1. "Every tier except L0 is a legible tier, and
                    // L0 means the thing is dead. A NEXT leg is not dead — it is the next thing that
                    // can take his money." This overrides canon's own LEVEL.NEXT = L1.
                    _legRow[i].Line.color = AtTier(flavorColor, TierL2);
                    _legRow[i].Line.text = statement;
                    SetRowChip(i, "NEXT", AtTier(flavorColor, TierL2), price);
                    if (_legRow[i].Extinguish != null) _legRow[i].Extinguish.enabled = false;
                    // A pending leg is equally ended by cashing out, so it is struck too. It does
                    // NOT step down: NEXT already sits at L1 and the next level is L0, which is the
                    // LOST extinguish this preview must never borrow.
                    if (_legRow[i].Strike != null) _legRow[i].Strike.enabled = _cashOutPreview;
                    _legRow[i].Need.text = string.Empty;
                    _legRow[i].Progress.text = string.Empty;
                }
            }

            // §7: "Risk and pays sit at the foot in gold at L2."
            _tRiskPays.text = $"RISK ${Money(_ticket.Stake)}     PAYS ${Money(_ticket.PotentialPayout)}";
        }

        /// <summary>TV-14: sets a compact row's price and state chip together.
        ///
        /// <para>The chip takes the row's state hue; the price does NOT — canon gives it
        /// <c>--tv-context</c> at L2 (`TvLegRow.jsx:62` + `:25`), because a price is a market fact
        /// and not part of the outcome. Concatenated into the statement as it was before, a lost
        /// leg's price rendered at L1 in the loss's own colour, which reads as the price having
        /// lost too.</para></summary>
        private void SetRowChip(int i, string state, Color stateInk, string price)
        {
            if (_legRow[i].State != null)
            {
                _legRow[i].State.text = state;
                _legRow[i].State.color = stateInk;
            }
            if (_legRow[i].Price != null)
            {
                _legRow[i].Price.text = price;
                _legRow[i].Price.color = AtTier(contextGrey, TierL2);
            }
        }

        private void ClearLegRow(int i)
        {
            _legRow[i].IsLive = false;
            if (_legRow[i].Line != null) _legRow[i].Line.text = string.Empty;
            if (_legRow[i].Price != null) _legRow[i].Price.text = string.Empty;
            if (_legRow[i].State != null) _legRow[i].State.text = string.Empty;
            if (_legRow[i].Need != null) _legRow[i].Need.text = string.Empty;
            if (_legRow[i].Progress != null) _legRow[i].Progress.text = string.Empty;
            if (_legRow[i].Strike != null) _legRow[i].Strike.enabled = false;
            if (_legRow[i].Extinguish != null) _legRow[i].Extinguish.enabled = false;
        }

        /// <summary>The auto-advance interstitial (M4): TICKET i/n, the legs line, stake → to-win.
        /// PRD §8.9: clears the prior stage/score/tape but the ticket column is NOT one of the
        /// things that clears — DESIGN.md §6: "The ticket column is stable. It does not resize
        /// between markets." RenderPregame already populated it (via ResetForNewSession, which
        /// runs immediately before this) with the incoming ticket's legs, all NEXT.</summary>
        private void RenderTicketCard()
        {
            _stage?.Show(false); // the interstitial card is stage-free; pregame re-raises it
            _stageLeg = -1;
            _tClock.text = "PRE";

            // TV-31: canon prints "TICKET 2 OF 2" (TvTicketCard.prompt.md:4, ui_kits data.js:54),
            // not the slashed form.
            _tTakeoverTitle.text = $"TICKET {director.SweatIndex + 1} OF {director.Run.Sweats.Count}";

            string legs = string.Empty;
            foreach (Leg leg in _ticket.Legs)
            {
                if (legs.Length > 0) legs += "   ·   ";
                legs += $"{leg.DisplayLabel} {OddsFormat.American(leg.OfferedOdds)}";
            }
            _tTakeoverSub.text = legs;

            _tFlavor.color = flavorColor;
            _tFlavor.text = $"${Money(_ticket.Stake)} TO WIN ${Money(_ticket.PotentialPayout)}";

            _probTarget = (float)_ticket.Legs[0].TrueProb; // data only
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
            HideCashOutSlot(); // T43
            _tAttract.enabled = false;
            _tLeg.text = string.Empty;
            _tClock.text = string.Empty;
            _tBigAmount.text = string.Empty;

            if (s.TotemFired)
            {
                _tTakeoverTitle.text = $"SHORT — ${Money(s.BankBefore)} AGAINST ${Money(s.Payment)}";
                // A deferred payment is bad news but not a payout — no gold. §4/§8: the bad-outcome
                // treatment is darkness, never the retired money-bad red; text stays legible in grey.
                _tFlavor.color = contextGrey;
                _tFlavor.text = "THE TOTEM BURNS";
                double juiced = s.Payment * (1.0 + (director?.Run?.Config.TotemJuiceRate ?? 0.5));
                _tTakeoverSub.text = $"PAYMENT DEFERRED — YOUR BANK STANDS. THE NEXT ONE GROWS BY ${Money(juiced)}";
                _emissRest = deadDark;
                EmissionFlash(deadDark);
                tvLight?.SetRest(deadDark, 0.08f);
            }
            else
            {
                _tTakeoverTitle.text = "PAYMENT MADE";
                // A payment landing is money — gold, per §4.
                _tFlavor.color = new Color(gold.r, gold.g, gold.b, 1f);
                _tFlavor.text = $"−${Money(s.Payment)}   ·   BANK ${Money(s.BankAfter)}";
                _tTakeoverSub.text = string.Empty;
                EmissionFlash(gold);
                tvLight?.Flash(gold, 3.0f);
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
            _tAttract.color = flavorColor; // an instructional prompt, not money — §4 Fact: cold white
            _tAttract.text = title;
            _tSubtitle.text = sub;

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
            // Won = money, gold. Lost = context, not a hue — legible grey, never the retired red.
            _tAttract.color = won
                ? new Color(gold.r, gold.g, gold.b, 1f)
                : contextGrey;
            _tSubtitle.text = $"FINAL BANK ${Money(r.Bank)}  —  NEW RUN AT THE LAPTOP";

            if (won)
            {
                // The run's final payout — §3's L4, "the payoff at its callback".
                _emissRest = RunWonRest();
                EmissionFlash(goldL4);
                tvLight?.Flash(new Color(1f, 0.82f, 0.25f), 3.4f);
                tvLight?.SetRest(new Color(1f, 0.82f, 0.35f), 0.45f);
            }
            else
            {
                // Cold and dark: desaturated blue-grey, barely lit - the room mourns. Already
                // compliant with §4 (no hue involved) — only the flash below was still the retired red.
                _emissRest = RunLostRest();
                EmissionFlash(deadDark);
                tvLight?.SetRest(new Color(0.30f, 0.34f, 0.48f), 0.10f);
            }
        }

        // T10 (Phase 3B): the agreed black floor (unified-grade-spec.md §2 / DESIGN.md §2A), matching
        // screenBg/barBgColor/pitchBgColor exactly — the room's emissive-quad lift. `_emissRest` must
        // never sit darker than this on any channel outside the one documented exception, `deadDark`
        // (a deliberate per-leg dip; see Ordering_gold_below_goldL4_and_deadDark_below_gold_holds).
        private static readonly Color EmissBlackFloor = new Color(0.048f, 0.055f, 0.068f);

        /// <summary>Rest glow for the RunWon verdict card. DESIGN.md §4: money/won stays gold, so this
        /// keeps a dim warm-gold afterglow (M4 grill decision) instead of falling back to the room's
        /// neutral idle — but clamped component-wise to <see cref="EmissBlackFloor"/>. Unclamped,
        /// `gold`'s low blue channel at 8% (0.0144) sits under the floor's blue (0.068) even though red
        /// and green already clear it, which would locally undo the room's black-floor lift on that
        /// channel alone (T10, found while auditing the two flagged literals).</summary>
        private Color RunWonRest()
        {
            Color dim = gold * 0.08f;
            return new Color(
                Mathf.Max(dim.r, EmissBlackFloor.r),
                Mathf.Max(dim.g, EmissBlackFloor.g),
                Mathf.Max(dim.b, EmissBlackFloor.b),
                dim.a);
        }

        /// <summary>Rest glow for the RunLost verdict card: cold and barely lit, the room mourns — but
        /// clamped to <see cref="EmissBlackFloor"/> rather than the old (0.008, 0.010, 0.018), which
        /// sat roughly 6x darker than the floor on every channel and locally undid the room's lift
        /// (T10). All three channels of the old mourning colour were below the floor, so the clamp
        /// resolves to the floor itself — already a cool near-black, so "barely lit, the room mourns"
        /// still reads; only the floor violation is gone.</summary>
        private Color RunLostRest() => EmissBlackFloor;

        private void ClearToBlankScreen()
        {
            _stage?.Show(false);
            _tape?.Show(false); // T16
            if (_tConsolation != null) _tConsolation.enabled = false;
            SetAlpha(_wonFlood, 0f);
            SetAlpha(_goldFlood, 0f);
            SetAlpha(_dimOverlay, 0f);
            HideCashOutSlot(); // T43
            _tInterventionPrompt.enabled = false;
            _tBigAmount.text = string.Empty;
            _tLeg.text = string.Empty;
            _tClock.text = string.Empty;
            _tMatchup.text = string.Empty;
            _tFlavor.text = string.Empty;
            _tTakeoverTitle.text = string.Empty;
            _tTakeoverSub.text = string.Empty;
            UpdateTicketColumn(-1);
        }

        private void RenderEvent(DramaEvent evt)
        {
            Leg leg = _ticket.Legs[evt.LegIndex];

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
            // T16: a non-final beat's dot lands at its reveal (RevealBeatChrome), never here —
            // looking away must never spoil a beat.
            _pendingTapeBeat = evt.Type != DramaEventType.LegFinal;
            if (_stage != null)
            {
                // Causal reveal (M-T3.1): identity chrome may update now, but the win-prob,
                // flavor, and clock are STASHED — they land at the scene's payoff
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
                _tMatchup.text = MatchupLine(leg);
                RevealedView.SetProbability(_probTarget);
                RevealedView.SetClock(_tClock.text);
                UpdateCashOutLabel();
            }

            _tAttract.enabled = false;
            UpdateTicketColumn(evt.LegIndex);
        }

        /// <summary>The beat's payoff moment on the stage: NOW the chrome may speak — the
        /// flavor line lands, the clock ticks, the market re-opens at the fresh price. Fired by
        /// the scene's onReveal (goal / save / scene end).</summary>
        private void RevealBeatChrome()
        {
            // T16: the tape's dot lands HERE, at the beat's reveal — not at RenderEvent — so
            // looking away never spoils it. contextGrey, not a team hue (the ruling's "no hue"):
            // the dot differentiates by size (the delta band) alone, never by colour, and stays
            // at contextGrey's L2 ceiling, never brighter.
            if (_pendingTapeBeat)
            {
                _tape?.AppendBeat(_stageLeg, contextGrey, SweatPresentationModel.MagnitudeBand(_lastBeatDelta));
                _pendingTapeBeat = false;
            }
            _probTarget = _pendingProb; // data only — RevealedView
            RevealedView.SetProbability(_probTarget);
            RevealedView.SetClock(_tClock.text);
            _tFlavor.color = flavorColor;
            _tFlavor.text = _pendingFlavor;
            _flavorScale = 1.12f;
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
            if (offerExists) ShowMarketSuspended();
            else HideCashOutSlot();
        }

        /// <summary>The suspended slate, whole, on ONE frame — T43's fix.
        ///
        /// The DD measured `MARKET SUSPENDED` rendered on solid gold, dimming "a frame later", and
        /// ruled: "suspended is L1 unlit slate from its FIRST frame; dim lands on the same frame as
        /// the label change." The cause was ordering, not colour. The label was set here, at the
        /// transition, but the slot's other three elements — the gold field, the status word and the
        /// L4 token — were derived only in <c>Update</c> (<c>AnimateCashOutTaunt</c>). Coroutines run
        /// after <c>Update</c>, so a suspend on frame N painted the word grey and left the field gold
        /// until frame N+1 repainted it. Exactly one frame of the surface promising input it would
        /// refuse — the same class as TVS-H02, a derived visual state lagging its own transition.
        ///
        /// Fixed by RULE, not by site (the lesson `WonLegBeat` and T39 both paid for): the slate is
        /// one method, it ends by re-deriving the whole slot, and both authoring sites call it. A
        /// future path that suspends inherits the fix rather than re-opening the bug.
        ///
        /// The status word goes with it. `HOLD E` beside `MARKET SUSPENDED` is TV-12/13's named
        /// violation — "MARKET SUSPENDED owns the cash-out slot exclusively, no actionable offer
        /// beside it" — and it was not a one-frame lie: the old guard only cleared the status when
        /// the slot was INVISIBLE, so a suspended-but-visible slot kept telling the player to hold a
        /// key the accept gate would refuse, for the whole suspension.</summary>
        private void ShowMarketSuspended()
        {
            // §8.5 Suspended: "L1, unlit slate" — structureGrey, not contextGrey. NOT cyan either:
            // cyan is reserved for VOID (§8), and a suspended market at peak tension must never read
            // as a voided leg.
            _cashOutSlotSuspended = true;
            _tCashOut.enabled = true;
            _tCashOut.color = structureGrey;
            _tCashOut.text = "MARKET SUSPENDED";
            ApplyCashOutSlotState();
        }

        /// <summary>No offer to show. Goes through the same re-derivation so the field and status can
        /// never outlive the figure they belong to.</summary>
        private void HideCashOutSlot()
        {
            _cashOutSlotSuspended = false;
            _tCashOut.enabled = false;
            ApplyCashOutSlotState();
        }

        private void ReopenMarket()
        {
            _marketSuspended = false;
            _cashOutSlotSuspended = false;
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
                // T43: the slate lifts on the same frame the figure returns. Clearing the
                // presentation flag here (not only in ReopenMarket) means any path that paints a
                // live offer also re-lights the field and restores the status word at once —
                // there is no frame where a gold figure sits on an unlit field.
                _cashOutSlotSuspended = false;
                _tCashOut.enabled = true;
                _tCashOut.color = new Color(gold.r, gold.g, gold.b, 1f); // suspend dims it; live gold restores
                SetCashOutOffer(offer.Value);
            }
            else
            {
                StopCashOutAnimation();
                HideCashOutSlot();
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
            // TVS-H02 fix: flip the flag BEFORE StartCoroutine, not after. StartCoroutine runs
            // AnimateCashOut synchronously up to its first `yield return null` — including that
            // first RenderCashOut call — before it returns the handle this line assigns; setting
            // _cashOutTweening here means that very first render already sees "tweening", instead
            // of reading the stale false StopCashOutAnimation just left behind.
            _cashOutTweening = true;
            _cashOutAnimation = StartCoroutine(AnimateCashOut(_cashOutShown, offer));
        }

        private IEnumerator AnimateCashOut(double from, double to)
        {
            float duration = Mathf.Max(0f, cashOutTickDuration * Mathf.Max(0f, TimeScaleOverride));
            if (duration <= 0f)
            {
                _cashOutShown = to;
                _cashOutRoundShown = RoundBucket(to);
                _cashOutTweening = false; // settle before the render, so it paints "[E]" not "UPDATING"
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
            _cashOutTweening = false; // settle before the render, so it paints "[E]" not "UPDATING"
            RenderCashOut(to);
            _cashOutAnimation = null;
        }

        private int RoundBucket(double amount)
        {
            double multiple = Math.Max(1.0, cashOutRoundMultiple);
            return (int)Math.Floor(amount / multiple);
        }

        /// <summary>§8.5 "Price animating: gold at L3, amount visibly settling, UPDATING." — the
        /// mid-tween label differs from the actionable one so the copy never promises input the
        /// gate (CanAcceptCashOutNow, which also requires <c>_cashOutAnimation == null</c>) would
        /// refuse. Reads <c>_cashOutTweening</c>, not <c>_cashOutAnimation != null</c> directly —
        /// see that field's declaration for why (TVS-H02: the Coroutine handle lags its own
        /// tween's first render by one synchronous step).</summary>
        private void RenderCashOut(double amount)
        {
            if (_tCashOut == null) return;
            // TV-04: the figure alone. T22 retires `[E]` outright — "not a label, it is a debug
            // token, and it is on a shipped surface in every frame" — and rules the slot to read
            // `CASH OUT $183` with `HOLD E` beneath. Where another product would draw a glyph, this
            // one prints the word.
            _tCashOut.text = $"CASH OUT ${Money(amount)}";
            if (_tCashOutStatus != null) _tCashOutStatus.text = _cashOutTweening ? "UPDATING" : "HOLD E";
            // T43: whether the status word SHOWS is not this method's call — a suspended slot carries
            // none at all (TV-12/13). One authority derives visibility, at the transition and in
            // Update alike, so the two can never disagree for a frame.
            ApplyCashOutSlotState();
        }

        private void StopCashOutAnimation()
        {
            if (_cashOutAnimation != null)
            {
                StopCoroutine(_cashOutAnimation);
                _cashOutAnimation = null;
            }
            _cashOutTweening = false; // unconditional: always leaves "not tweening", idempotently
        }

        private string MatchupLine(Leg leg)
        {
            string away = SweatFlavor.Short(leg.Matchup.Away.Name);
            string home = SweatFlavor.Short(leg.Matchup.Home.Name);
            // Mark the picked side with a dot so the player knows which team is theirs.
            // Market legs have no team — no dot (their pick reads from the ticket column).
            bool isMl = leg.Selection.Kind == MarketKind.Moneyline;
            bool pickedHome = SweatFlavor.PickedHomeForPresentation(leg);
            string awayMark = isMl && !pickedHome ? "● " : "";
            string homeMark = isMl && pickedHome ? " ●" : "";
            return $"{awayMark}{away.ToUpperInvariant()}  @  {home.ToUpperInvariant()}{homeMark}";
        }

        // ---------------------------------------------------------------- beats

        private IEnumerator ResolveBeat(DramaEvent evt)
        {
            Leg leg = _ticket.Legs[evt.LegIndex];
            int k = evt.LegIndex + 1;

            if (leg.IsVoided)
            {
                // TV-05: the event strip is neutral — "it never uses money hues" and "stays neutral
                // even when the event helps or hurts; money semantics live on the leg rows and the
                // cash-out slot" (TvEventStrip.jsx:5, prompt.md:7). The VOID hue belongs to the leg
                // row, which already carries it. TV-32: em dash, not a hyphen.
                _tFlavor.color = AtTier(flavorColor, TierL2);
                _tFlavor.text = $"LEG {k} — VOIDED, THE TICKET LIVES";
                yield return ScaledWait(deadLineDuration);
            }
            else if (leg.GradesWon)
            {
                yield return WonLegBeat(k);
            }
            else
            {
                yield return DeadLegBeat(k);
            }

            _resolvedThrough = evt.LegIndex + 1;
            UpdateTicketColumn(evt.LegIndex + 1); // next leg reads LIVE once its events start
        }

        private IEnumerator WonLegBeat(int k)
        {
            // T40: the full-field gold wash is GONE. It flooded the whole canvas at 0.5 alpha, and
            // §4 rations gold to won leg rows, risk/pays and the cash-out band — a screen-wide wash
            // is gold everywhere, which is the precise inverse of rationing. The leg's win is
            // already stated where §8 puts it: the row itself goes L3 gold.
            //
            // TV-05: the event strip stays NEUTRAL. "It never uses money hues ... money semantics
            // live on the leg rows and the cash-out slot" (TvEventStrip.jsx:5, prompt.md:7). The
            // VOID and DEAD paths were corrected earlier and this one was missed — the same
            // violation, three beats apart.
            //
            // TV-32: em dash, the system's own dash.
            _tFlavor.color = AtTier(flavorColor, TierL2);
            _tFlavor.text = $"LEG {k} — WON";
            // The panel's own glow and the room light still warm: those are the TV being a lit
            // object in a room, not the canvas painting itself gold.
            EmissionFlash(gold);
            tvLight?.Flash(gold, 3.0f);
            yield return ScaledWait(wonFloodDuration);
        }

        private IEnumerator DeadLegBeat(int k)
        {
            // 1) The dark beat. T8 (Allen, 2026-07-31): the static-noise crawl that used to fill
            // this hold is REMOVED — DESIGN.md §2 bans interference noise by name as a signature of
            // the deprecated design/08-art-direction.md world. The hold itself is kept at exactly
            // its old length (deadStaticDuration, still scaled by TimeScaleOverride) because it is
            // load-bearing pacing, not decoration: the light's ease toward black plays across it and
            // the hard cut to the dim "mourning" rest lands at its end. Removing the effect must not
            // silently shorten the beat.
            //
            // Losing is darkness, which is what this beat now is, with nothing laid over it
            // (DESIGN.md §4: "Loss is still darkness ... the old green/red money language stays
            // retired").
            tvLight?.Flash(Color.black, 0f);
            yield return WaitRealtime(Mathf.Max(0f, deadStaticDuration * Mathf.Max(0f, TimeScaleOverride)));

            // 2) the DEAD line + the screen dropping darker — darkness, not the retired red (§4/§8:
            // "Loss is still darkness ... the old green/red money language stays retired").
            _tFlavor.color = AtTier(contextGrey, TierL2);
            _tFlavor.text = $"LEG {k} — DEAD"; // TV-32: em dash, the system's own dash
            _emissRest = deadDark;
            EmissionFlash(deadDark);
            tvLight?.SetRest(deadDark, 0.08f);
            yield return ScaledWait(deadLineDuration);
        }

        private IEnumerator TicketDeadBeat()
        {
            // TV dims to near-black for a beat before the next demo ticket. Same darkness treatment
            // as a single dead leg, just dimmer still — the whole ticket is gone.
            tvLight?.SetRest(deadDark, 0.05f);
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
            // The ticket's payout tally — §3's L4, "the payoff at its callback", brighter than a
            // routine won-leg flash so the ordering idle < flash < L4 stays visible. C3: a momentary
            // punch, so it takes the token from anything else currently holding it.
            EmissionFlash(goldL4);
            tvLight?.Flash(new Color(1f, 0.82f, 0.25f), 3.4f);
            RequestL4(HdrFocus.Payout, momentary: true);
            StartCoroutine(FloodPulse(_goldFlood, gold, 0.5f, winFloodDuration));
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
            // §8.5 Accepted: "gold, brief L4 punch, then CASHED OUT $x at L3 into the settle
            // transition."
            _tBigAmount.text = $"CASHED OUT ${Money(amount)}";
            HideCashOutSlot(); // T43: nothing of the offer outlives the accept
            EmissionFlash(goldL4);
            tvLight?.Flash(new Color(1f, 0.82f, 0.25f), 3.4f);
            RequestL4(HdrFocus.Payout, momentary: true); // C3: a momentary punch preempts CashOut's hold
            yield return FloodPulse(_goldFlood, gold, 0.55f, cashOutFloodDuration);
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
            AnimateLegPulse();
            AnimateFlavorPunch();
            AnimateCashOutTaunt();
            TickClock();

            if (_audio != null)
            {
                _audio.masterVolume = masterVolume;
                _audio.crowdVolume = crowdVolume;
                _audio.stingVolume = stingVolume;
                _audio.SetTension(1f - Mathf.Abs(2f * (RevealedView.WinProbability) - 1f), _audioUrgency);
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
            // §8.10: the bank shows its post-cash-out value while the preview is held. Rendered
            // here, inside the per-frame chrome rebuild, rather than written once at entry — an
            // overwrite would be stomped on the very next frame and read as the "bank flicker"
            // §8.10 forbids by name.
            _tChrome.text =
                $"R{r.Round}/{r.Config.Rounds}   ·   BANK ${Money(PreviewedBank(r))}   ·   PAY ${Money(r.CurrentPayment)}" +
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

        /// <summary>DESIGN.md §8/§9: the LIVE leg row's slow pulse — "the surface's only slow
        /// pulse. Nothing else pulses, so this is unmistakable." Driven off the shared
        /// _seatedClock (one clock for every LIVE row, "in phase"), which already only
        /// accumulates while seated, so this freezes exactly on stand with no extra gating.</summary>
        private void AnimateLegPulse()
        {
            if (_legRow == null) return;
            float pulse01 = 0.72f + 0.28f * (0.5f + 0.5f * Mathf.Sin(_seatedClock * livePulseHz * 2f * Mathf.PI));
            for (int i = 0; i < _legRow.Length; i++)
            {
                if (!_legRow[i].IsLive) continue;
                // T20: a live row's two elements are NEED and progress — the compact Line is blank
                // while live, so pulsing it would animate nothing. Both live lines share the one
                // phase, which is what makes the row read as a single breathing thing.
                // TV-S1: the pulse rides ON the L3 tier rather than replacing it. Pulsing raw
                // flavorColor put a live row at alpha 1.0 at the top of every cycle, which is the
                // L4 tier — the one the surface reserves for a single element at a time.
                Color c = AtTier(flavorColor, TierL3); c.a *= pulse01;
                if (_legRow[i].Need != null) _legRow[i].Need.color = c;
                if (_legRow[i].Progress != null) _legRow[i].Progress.color = c;
            }
        }

        /// <summary>The cash-out slot's four elements — the money figure, the gold field behind it,
        /// the status word beside it and the L4 token it may hold — are ONE state, derived here and
        /// nowhere else.
        ///
        /// TV-03, render-aware rather than set at eight call sites. The slot is disabled from many
        /// places; making the field and the status word FOLLOW the money element's own state means a
        /// future path that hides the slot cannot leave a gold field or a stale status word behind it.
        /// Same reasoning as §8.10's preview: recompute from truth.
        ///
        /// The inversion is gated on CanAcceptCashOutNow, not on visibility — DESIGN.md §8:
        /// "brightness is a promise about input. L4 means the key will work right now." A gold field
        /// over an offer that would be refused is the surface lying.
        ///
        /// T43 moved this OUT of Update. Deriving it only per-frame was the whole defect: every
        /// transition rendered one frame of the previous state's field. It is idempotent, so calling
        /// it from both the transitions and Update costs nothing and closes the window.</summary>
        private void ApplyCashOutSlotState()
        {
            if (_tCashOut == null) return;
            bool slotVisible = _tCashOut.enabled;
            // Both terms are load-bearing and they are NOT redundant. CanAcceptCashOutNow reads
            // _marketSuspended (the market's state); _cashOutSlotSuspended reads the slot's own
            // (§8.7's pending window renders the suspended slate while the market is still open,
            // because ResolveBeat never calls SuspendMarket). Without the second term the pending
            // window kept a lit gold field and an L4 token under the word MARKET SUSPENDED — not for
            // a frame, but for as long as the player took to decide.
            bool live = slotVisible && !_cashOutSlotSuspended;
            // `!_cashOutTweening` is NOT redundant with CanAcceptCashOutNow's own
            // `_cashOutAnimation == null`, and leaving it out re-opened TVS-H02 in a new place —
            // caught in diff review of this very change, before it ever ran.
            //
            // T43 moved this derivation out of Update, which is what fixed the transition frame. But
            // it also means RenderCashOut now reaches here — including the RenderCashOut that runs
            // SYNCHRONOUSLY inside StartCoroutine, before the handle is assigned to
            // _cashOutAnimation. At that instant the handle is still the null StopCashOutAnimation
            // just left, so CanAcceptCashOutNow answers TRUE mid-tween and the gold field and the L4
            // token would light for exactly one frame of a price update — §8.5's "brightness is a
            // promise about input", broken at the brightest tier, by the fix for a state lie.
            //
            // _cashOutTweening exists precisely because that handle lags its own first render by one
            // synchronous step (see its declaration, and §4B of the handoff). It is set true BEFORE
            // StartCoroutine and false before each settle render, so it is true across exactly the
            // window the handle is wrong about. Read the flag, never the handle.
            bool fieldLit = live && !_cashOutTweening && CanAcceptCashOutNow();
            if (_cashOutField != null) _cashOutField.enabled = fieldLit;
            // TV-12/13: suspended owns the slot exclusively. The status word is the offer speaking,
            // so it is absent whenever the offer is not — never merely dimmed (C10).
            if (_tCashOutStatus != null) _tCashOutStatus.enabled = live;
            // §8.5: the slot's brightness is a promise about input — L4 only while a press would
            // actually be accepted right now (same predicate as the accept gate itself, so this can
            // never promise more than TryCashOut will honor). Suspended and mid-tween stay LDR.
            // C3 rule 5: the boost is 1.8, a single value — no second, per-element scale on top of it
            // (the old taunt-flash lerp up to HdrBoostL4 * 1.15 is retired). CashOut's request is
            // SUSTAINED: it re-asks every frame while actionable, and yields the instant a momentary
            // punch (a goal's score, a payoff's ball, a win/cash-out tally) takes the token instead.
            if (_cashOutHdrMat != null)
            {
                if (fieldLit) RequestL4(HdrFocus.CashOut, momentary: false);
                else ReleaseL4(HdrFocus.CashOut);
            }
        }

        private void AnimateCashOutTaunt()
        {
            if (_tCashOut == null) return;

            ApplyCashOutSlotState();

            float scaledDt = SeatedDeltaTime / Mathf.Max(0.0001f, TimeScaleOverride); // TVS-H02
            _cashOutScale = Mathf.MoveTowards(_cashOutScale, 1f, 3.2f * scaledDt);
            _cashOutFlash = Mathf.MoveTowards(_cashOutFlash, 0f, 4.5f * scaledDt);
            _tCashOut.rectTransform.localScale = Vector3.one * _cashOutScale;
            // T43, the third instance and the loudest: this taunt REPAINTS the money word gold every
            // frame, and it was gated on _marketSuspended alone. §8.7's pending window renders the
            // suspended slate while the market is still open (ResolveBeat never suspends), so this
            // line overwrote structureGrey and drew the literal words MARKET SUSPENDED in full
            // brightness gold — not for one frame, but for as long as the player took to decide. The
            // gate is the slot's presentation state, which both suspend sites set.
            if (_tCashOut.enabled && !_marketSuspended && !_cashOutSlotSuspended)
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

            var canvasGo = new GameObject("SweatCanvas", typeof(Canvas));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var canvasRt = canvas.GetComponent<RectTransform>();
            canvasRt.sizeDelta = new Vector2(w, h);

            // T25.1 — THE GLASS CLIPS. Allen, direct observation: the stage actors, the tape and
            // plain text lines were all passing in and out of the TV panel, drawn into the room.
            //
            // A UGUI canvas does NOT clip its children by default, and the escapes had three
            // different causes: the stage was misanchored (fixed separately), MomentumTape's dot
            // cursor advances per beat with no row-width bound, and this canvas's Text is
            // overflow-enabled so long copy renders past its own rect. Fixing placement per layer
            // cannot answer "nothing may leave the panel" — it only removes today's offenders and
            // leaves the next one to be found by eye.
            //
            // RectMask2D makes containment STRUCTURAL: the glass is a clip rect, so anything drawn
            // outside it stops existing on screen no matter which layer misplaces itself or how far
            // a future element overflows. Verified compatible with the HDR path before adding it —
            // TvSweatHdrUI.shader carries `#pragma multi_compile_local _ UNITY_UI_CLIP_RECT` and
            // applies UnityGet2DClipping, so the L4-eligible elements clip too rather than being
            // the one layer that still escapes.
            canvasGo.AddComponent<RectMask2D>();

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

            // Backing panel — near-black but lifted to the room's floor (screenBg), never pure black;
            // the quad's own glow bleeds through its slight transparency.
            _backing = MakeStretchImage(root, "Backing", screenBg);

            var grid = new LayoutGrid(w, h);

            BuildHairlines(root, grid);
            BuildTicketColumn(root, grid);
            BuildScoreBug(root, grid);
            BuildEventStrip(root, grid);
            BuildCashOutZone(root, grid);
            BuildChromeStrip(root, grid);

            // --- attract state (before the sweat is live) ---
            _tAttract = MakeText(root, "Attract", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(w - 60f, 130f), 46,
                TextAnchor.MiddleCenter, flavorColor, FontStyle.Bold); // §4 Fact: cold white, not money
            _tAttract.text = "SIT TO WATCH THE SWEAT";

            // Ticket-card / settle-card takeover copy (PRD §8.9): the stage goes quiet during these
            // transitions, but the ticket column never clears (DESIGN.md §6: "does not resize
            // between markets"). Sits inside the fixed Stage zone rather than floating over the
            // whole canvas, so it never competes with the ticket rail.
            _tTakeoverTitle = MakeText(root, "TakeoverTitle", new Vector2(0f, 1f), new Vector2(0.5f, 0.5f),
                AnchorCenter(grid.Stage) + new Vector2(0f, 40f), new Vector2(grid.Stage.width - 60f, 60f), 30,
                TextAnchor.MiddleCenter, flavorColor, FontStyle.Bold);
            _tTakeoverSub = MakeText(root, "TakeoverSub", new Vector2(0f, 1f), new Vector2(0.5f, 0.5f),
                AnchorCenter(grid.Stage) + new Vector2(0f, -20f), new Vector2(grid.Stage.width - 60f, 60f), 18,
                TextAnchor.MiddleCenter, contextGrey);

            // Subtitle line reused ONLY by the idle/run-over screens (non-sweat states); never
            // shown during a live sweat — DESIGN.md §7's component list has no standalone win%/
            // subtitle slot for the live grid.
            _tSubtitle = MakeText(root, "Subtitle", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -60f), new Vector2(w - 120f, 34f), 22, TextAnchor.MiddleCenter, flavorColor);

            // C3 (Design Director ruling): "the ball at a payoff" joins the HDR-eligible set. Built
            // HERE, unconditionally — never gated behind `if (theaterEnabled)` — so eligibility does
            // not depend on whether the theater stage exists (the closed-world L4 test runs with
            // theaterEnabled=false). TheaterStage.cs owns the real ball actor privately and sits
            // outside this phase's file boundary with no public hook for it, so this is a dedicated
            // payoff-flash standing in for it, centred on the Stage zone, normally invisible.
            _ballFlash = MakePanel(root, "Ball", new Vector2(0f, 1f), new Vector2(0.5f, 0.5f),
                AnchorCenter(grid.Stage), new Vector2(28f, 28f),
                new Color(flavorColor.r, flavorColor.g, flavorColor.b, 1f));
            _ballFlash.enabled = false;
            _ballHdrMat = MakeHdrMaterial();
            if (_ballHdrMat != null) _ballFlash.material = _ballHdrMat;

            // T16: "restore it: at the foot of the scorebug, no numerals, no hue, never above L2."
            // An invisible anchor panel hugging the inside-bottom edge of grid.ScoreBug —
            // MomentumTape.Build anchors CENTER-relative to whatever transform it is given, so this
            // gives it a precisely positioned, fixed-grid parent without touching MomentumTape.cs
            // itself. The win-probability numeral stays OUT permanently (§7's duplication ban) —
            // this tape is dots and caps only, never a numeral.
            //
            // Built HERE, unconditionally, and deliberately NOT inside the `theaterEnabled` block
            // below — for the same reason the ball flash above is not: the tape is SCOREBUG
            // furniture, and the DD ruled it in at the scorebug foot. Whether the theater stage
            // exists is a separate question, and coupling the two meant the tape silently vanished
            // in any configuration without a stage.
            Rect tapeFoot = new Rect(grid.ScoreBug.x, grid.ScoreBug.yMax - MomentumTapeHeight,
                grid.ScoreBug.width, MomentumTapeHeight);
            Image tapeAnchor = MakePanel(root, "MomentumTapeAnchor", new Vector2(0f, 1f), new Vector2(0f, 1f),
                AnchorTopLeft(tapeFoot), new Vector2(tapeFoot.width, tapeFoot.height), Color.clear);
            // The regular face for the tape's MOMENTUM label — canon marks the tape's own chrome
            // --font-tv, and only the dense numeric slots condensed.
            _tape = MomentumTape.Build(tapeAnchor.transform, Vector2.zero,
                new Vector2(tapeFoot.width - 20f, tapeFoot.height), _font);

            // --- the match theater stage (F_0.2.0 M-T2), built INTO the fixed Stage zone ---
            if (theaterEnabled)
            {
                _stage = TheaterStage.Build(root, AnchorCenter(grid.Stage),
                    new Vector2(grid.Stage.width, grid.Stage.height), pitchLineColor, pitchBgColor);
                _stage.paceScale = pacer.paceMultiplier; // stage playback matches the pacer's arithmetic

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
            // A won leg is money, gold — not the retired green.
            _wonFlood = MakeStretchImage(root, "WonFlood", new Color(gold.r, gold.g, gold.b, 0f));
            // T8 (Allen, 2026-07-31): the StaticNoise overlay is REMOVED — DESIGN.md §2 bans
            // interference noise by name. Nothing replaces it; loss is darkness, which DimOverlay
            // below already provides.
            // Black floor (unified-grade-spec.md §2): even the "everything just went dark" overlay
            // must not sit below the room's deepest shadow, so its RGB matches the same floor as
            // screenBg/barBgColor/pitchBgColor rather than true (0,0,0). Only alpha animates.
            _dimOverlay = MakeStretchImage(root, "DimOverlay", new Color(0.048f, 0.055f, 0.068f, 0f));
            _goldFlood = MakeStretchImage(root, "GoldFlood", new Color(gold.r, gold.g, gold.b, 0f));
            _goldFloodHdrMat = MakeHdrMaterial();
            if (_goldFloodHdrMat != null) _goldFlood.material = _goldFloodHdrMat;
            _tBigAmount = MakeText(root, "BigAmount", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(w - 40f, 200f), 96,
                TextAnchor.MiddleCenter, new Color(gold.r, gold.g, gold.b, 1f), FontStyle.Bold);
            _tBigAmount.text = string.Empty;
            _bigAmountHdrMat = MakeHdrMaterial();
            if (_bigAmountHdrMat != null) _tBigAmount.material = _bigAmountHdrMat;

            // The bad-beat consolation line — built ABOVE the dim overlay so the sting stays
            // readable through the 94% dim (Sol, M-T4); neutral chrome, never money-red.
            _tConsolation = MakeText(root, "Consolation", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -20f), new Vector2(w - 80f, 44f), 28,
                TextAnchor.MiddleCenter, flavorColor, FontStyle.Italic);
            _tConsolation.enabled = false;

            // T8 (Allen, 2026-07-31): the Scanlines overlay is REMOVED — DESIGN.md §2 bans
            // scanlines by name as a signature artifact of the deprecated
            // design/08-art-direction.md world. The panel is a maintained modern display, not a
            // failing CRT; nothing is laid over the surface in its place.

            _probTarget = 0.5f;
        }

        // ---- zone builders (Layout B, DESIGN.md §6/§7) ----

        /// <summary>C8 (Design Director ruling, DESIGN.md §12 question 4 /
        /// docs/tv-sweat-refinement/c3-hdr-canvas-proposal.md): the bloom-floor protected set —
        /// canvas elements that may NOT be sacrificed as the room's shared bloom volume brightens.
        /// Contrasted with system chrome (the ChromeStrip's round/bank/payout/seed row), which PRD
        /// §8.1 puts at lowest priority and is explicitly allowed to degrade. Originally the score,
        /// the clock, each live leg's NEED line, and the cash-out state
        /// (docs/tv-sweat-refinement/room-lead-reply.md §3); C8 adds risk/pays. This worktree
        /// cannot itself verify the bloom outcome — that needs a seated capture on a GPU-backed
        /// session only the room lead can produce — so this is the POLICY that capture gets checked
        /// against, named here so a reviewer never has to re-derive it from prose.</summary>
        private static readonly string[] BloomFloorProtectedElements =
        {
            "Matchup",          // the score
            "Clock",
            // T20 renamed these: the old LegRowDetail{i} carried NEED and progress in one element,
            // and the protected set follows the NEED statement, which is now its own element.
            "LegRowNeed0", "LegRowNeed1", "LegRowNeed2",
            "LegRowNeed3", "LegRowNeed4", "LegRowNeed5", // each live leg's NEED line
            "CashOut",          // the cash-out state
            "RiskPays",         // C8: joins the protected set
        };

        private void BuildTicketColumn(Transform root, LayoutGrid grid)
        {
            // Unpadded, exactly the zone's own rect — same colour as Backing (purely structural;
            // also the grid's own geometric anchor for the Phase 3C layout-geometry tests).
            MakePanel(root, "TicketColumnZone", new Vector2(0f, 1f), new Vector2(0f, 1f),
                AnchorTopLeft(grid.TicketColumn), new Vector2(grid.TicketColumn.width, grid.TicketColumn.height),
                screenBg);

            // §7 Ticket column header — ticket index, L1 structure (one level up from the
            // scorebug's per-leg index).
            _tTicketHeader = MakeText(root, "TicketHeader", new Vector2(0f, 1f), new Vector2(0f, 1f),
                AnchorTopLeft(grid.TicketHeader, 8f, 4f),
                new Vector2(grid.TicketHeader.width - 16f, grid.TicketHeader.height - 4f), TypeEyebrow,
                TextAnchor.UpperLeft, structureGrey);

            _legRow = new LegRowUi[TicketRowSlots];
            // T20 row stack, budgeted from the canon type scale rather than hand-placed. The two
            // live lines must fit inside TicketRowHeight with the row's own padding; asserted by
            // TvSweatScreenLayoutGridTests so a future size change cannot silently start clipping.
            float lineW = grid.TicketColumn.width - 16f;
            float needH = Mathf.Ceil(TypeNeed * LineBox);
            float progressH = Mathf.Ceil(TypeProgress * LineBox);
            float compactH = Mathf.Ceil(TypeEyebrow * LineBox);
            for (int i = 0; i < TicketRowSlots; i++)
            {
                Rect row = grid.TicketRow(i);
                // TV-21: built FIRST so it sits behind the row's text — a lost leg is "unlit pixel
                // structure", which is a field the words sit on, not a tint over them.
                Image extinguish = MakePanel(root, $"LegRowExtinguish{i}", new Vector2(0f, 1f),
                    new Vector2(0f, 1f), AnchorTopLeft(row), new Vector2(row.width, row.height),
                    extinguished);
                extinguish.enabled = false;
                // Compact form (resolved / pending), TV-14: THREE spans across one line at the
                // eyebrow scale. Canon drops the market eyebrow here rather than shrinking it —
                // every authored statement already names its own market — and orders the row
                // statement · price · state, with the state right-aligned in its own chip.
                //
                // Widths are fixed, never derived from content (§6): the chip reserves canon's 38px
                // at the right edge, the price reserves a column left of it, and the statement takes
                // the remainder and ellipsises. A price that moved with the statement's length would
                // make the column's right edge ragged across six rows.
                const float chipW = 38f, priceW = 52f, gap = 8f;
                float stmtW = lineW - chipW - priceW - gap * 2f;

                Text line = MakeText(root, $"LegRowLine{i}", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    AnchorTopLeft(row, 8f, 4f), new Vector2(stmtW, compactH), TypeEyebrow,
                    TextAnchor.UpperLeft, structureGrey, FontStyle.Bold, Face.Condensed); // TvLegRow.jsx:57-61
                line.horizontalOverflow = HorizontalWrapMode.Wrap; // so the statement clips, not sprawls

                Text price = MakeText(root, $"LegRowPrice{i}", new Vector2(0f, 1f), new Vector2(1f, 1f),
                    AnchorTopLeft(row, 8f + stmtW + gap + priceW, 4f), new Vector2(priceW, compactH),
                    TypeEyebrow, TextAnchor.UpperRight, AtTier(contextGrey, TierL2),
                    FontStyle.Normal, Face.Condensed); // TvLegRow.jsx:62 — --tv-context, its own tier

                Text state = MakeText(root, $"LegRowState{i}", new Vector2(0f, 1f), new Vector2(1f, 1f),
                    AnchorTopLeft(row, 8f + lineW, 4f), new Vector2(chipW, compactH), TypeEyebrow,
                    TextAnchor.UpperRight, structureGrey); // TvLegRow.jsx:27-31 — regular face, min 38px
                // Live form: the authored NEED statement, then the revealed progress beneath it.
                Text need = MakeText(root, $"LegRowNeed{i}", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    AnchorTopLeft(row, 8f, 4f), new Vector2(lineW, needH), TypeNeed,
                    TextAnchor.UpperLeft, flavorColor, FontStyle.Bold, Face.Condensed); // inherits TvLegRow.jsx:35
                Text progress = MakeText(root, $"LegRowProgress{i}", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    AnchorTopLeft(row, 8f, 4f + needH), new Vector2(lineW, progressH), TypeProgress,
                    TextAnchor.UpperLeft, flavorColor, FontStyle.Normal, Face.Condensed); // TvLegRow.jsx:82
                // T20/3D — §8's VOID treatment is "L2 cyan, STRUCK THROUGH on the matrix". Colour
                // alone was carrying the whole state before; this is the strike. A fixed-width rule
                // across the compact line, never measured from the text: §6 forbids geometry
                // computed from content, and a rule that resizes per statement would be exactly
                // that. Legacy UI.Text has no strikethrough of its own — TextMeshPro does, but the
                // whole surface is UI.Text and swapping one row's renderer for a glyph effect is a
                // far larger change than drawing the line the design already calls a matrix rule.
                Image strike = MakePanel(root, $"LegRowStrike{i}", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    AnchorTopLeft(row, 8f, 4f + compactH * 0.5f), new Vector2(lineW, 1.5f),
                    AtTier(tvVoid, TierL2)); // TV-20: the rule is the VOID token at its own tier
                strike.enabled = false;
                _legRow[i] = new LegRowUi
                {
                    Line = line, Price = price, State = state,
                    Need = need, Progress = progress, Strike = strike,
                    Extinguish = extinguish, IsLive = false
                };
            }

            // §7: "Risk and pays sit at the foot in gold at L2."
            _tRiskPays = MakeText(root, "RiskPays", new Vector2(0f, 1f), new Vector2(0f, 1f),
                AnchorTopLeft(grid.TicketFooter, 8f, 8f),
                new Vector2(grid.TicketFooter.width - 16f, grid.TicketFooter.height - 8f), TypeRisk,
                TextAnchor.UpperLeft, goldL2, FontStyle.Bold, Face.Condensed); // TvRiskPays.jsx:14
        }

        private void BuildScoreBug(Transform root, LayoutGrid grid)
        {
            Image zone = MakePanel(root, "ScoreBugZone", new Vector2(0f, 1f), new Vector2(0f, 1f),
                AnchorTopLeft(grid.ScoreBug), new Vector2(grid.ScoreBug.width, grid.ScoreBug.height), screenBg);
            Transform sbRoot = ZoneRoot(zone); // T46 — see ZoneRoot
            // Zone-LOCAL geometry. The children now anchor to the zone's own top-left instead of the
            // canvas's, so every offset drops grid.ScoreBug's origin: identical pixels on screen,
            // expressed against the thing that owns them. §6's fixed grid is untouched — this rect
            // is still derived once, from the grid, never from content.
            Rect sb = new Rect(0f, 0f, grid.ScoreBug.width, grid.ScoreBug.height);
            // §7 Scorebug: "Ticket/leg index at L1, present but subordinate."
            _tLeg = MakeText(sbRoot, "Leg", new Vector2(0f, 1f), new Vector2(0f, 1f),
                AnchorTopLeft(sb, 10f, 8f), new Vector2(140f, Mathf.Ceil(TypeEyebrow * LineBox)),
                TypeEyebrow, TextAnchor.UpperLeft, structureGrey);
            // §7: "Clock remains fixed at the right edge."
            _tClock = MakeText(sbRoot, "Clock", new Vector2(0f, 1f), new Vector2(1f, 1f),
                AnchorTopRight(sb, 10f, 8f), new Vector2(140f, Mathf.Ceil(TypeClock * LineBox)),
                TypeClock, TextAnchor.UpperRight, flavorColor);
            // §4 Fact: "Score, clock, live leg names, market lines" — cold white at L3.
            _tMatchup = MakeText(sbRoot, "Matchup", new Vector2(0f, 1f), new Vector2(0.5f, 1f),
                AnchorTopCenter(sb, 8f), new Vector2(sb.width - 40f, sb.height - MomentumTapeHeight), TypeScore,
                TextAnchor.UpperCenter, flavorColor, FontStyle.Bold);

            // C3 (Design Director ruling): "the score at a goal" joins the HDR-eligible set.
            // Matchup above is the PERSISTENT, always-on score truth at L3 — this is a punch-only
            // overlay at the SAME rect, normally hidden (§7's duplication ban is exactly why this
            // must not also read as an always-visible second score display), shown only for the
            // instant a goal commits and boosted through the shared HDR material to L4.
            _tScoreFlash = MakeText(sbRoot, "Score", new Vector2(0f, 1f), new Vector2(0.5f, 1f),
                AnchorTopCenter(sb, 8f), new Vector2(sb.width - 40f, sb.height - MomentumTapeHeight), TypeScore,
                TextAnchor.UpperCenter, new Color(gold.r, gold.g, gold.b, 1f), FontStyle.Bold);
            _tScoreFlash.enabled = false;
            _scoreHdrMat = MakeHdrMaterial();
            if (_scoreHdrMat != null) _tScoreFlash.material = _scoreHdrMat;
        }

        private void BuildEventStrip(Transform root, LayoutGrid grid)
        {
            Image zone = MakePanel(root, "EventStripZone", new Vector2(0f, 1f), new Vector2(0f, 1f),
                AnchorTopLeft(grid.EventStrip), new Vector2(grid.EventStrip.width, grid.EventStrip.height), screenBg);
            Transform esRoot = ZoneRoot(zone); // T46
            Rect es = new Rect(0f, 0f, grid.EventStrip.width, grid.EventStrip.height);

            // §7 Event strip: "One line, white, L2 at rest, punching to L3 at its reveal
            // callback." Reuses the "Flavor" GameObject name — required by PlayMode regression
            // coverage (TVS-H02's flavor-punch freeze test) and by every live-beat message call
            // site elsewhere in this file.
            _tFlavor = MakeText(esRoot, "Flavor", new Vector2(0f, 1f), new Vector2(0.5f, 0.5f),
                AnchorCenter(es), new Vector2(es.width - 24f, es.height - 8f),
                TypeEvent, TextAnchor.MiddleCenter, flavorColor, FontStyle.Bold);
        }

        private void BuildCashOutZone(Transform root, LayoutGrid grid)
        {
            MakePanel(root, "CashOutZone", new Vector2(0f, 1f), new Vector2(0f, 1f),
                AnchorTopLeft(grid.CashOut), new Vector2(grid.CashOut.width, grid.CashOut.height), screenBg);

            // TV-03: the actionable field. Built BEFORE the type so the type punches out of it, and
            // sized to the zone exactly — canon's inversion is a solid field, not a tinted panel.
            // Disabled by default: the field IS the actionable state, so it exists only while the
            // key will actually work.
            _cashOutField = MakePanel(root, "CashOutField", new Vector2(0f, 1f), new Vector2(0f, 1f),
                AnchorTopLeft(grid.CashOut), new Vector2(grid.CashOut.width, grid.CashOut.height),
                new Color(gold.r, gold.g, gold.b, 1f));
            _cashOutField.enabled = false;

            // TV-04: money and status are SEPARATE elements. Canon is explicit that "the status word
            // rides at label scale beside the figure, NEVER at money scale" (TvCashOutSlot.jsx:35-47)
            // — the build rendered `CASH OUT $184   •   UPDATING` as one 29px string, which says the
            // status is as important as the number. Money keeps the name "CashOut" because the L4
            // token, the bloom-floor protected set and three tests all address it by that name.
            _tCashOut = MakeText(root, "CashOut", new Vector2(0f, 1f), new Vector2(0f, 0.5f),
                AnchorCenter(grid.CashOut) + new Vector2(-grid.CashOut.width * 0.5f + 12f, 0f),
                new Vector2(grid.CashOut.width - 24f, grid.CashOut.height - 8f), TypeCashOut,
                TextAnchor.MiddleLeft, new Color(gold.r, gold.g, gold.b, 1f), FontStyle.Bold,
                Face.Condensed); // TvCashOutSlot.jsx:33
            _tCashOut.enabled = false;

            _tCashOutStatus = MakeText(root, "CashOutStatus", new Vector2(0f, 1f), new Vector2(1f, 0.5f),
                AnchorCenter(grid.CashOut) + new Vector2(grid.CashOut.width * 0.5f - 12f, 0f),
                new Vector2(grid.CashOut.width - 24f, Mathf.Ceil(TypeEyebrow * LineBox)), TypeEyebrow,
                TextAnchor.MiddleRight, AtTier(contextGrey, TierL2));
            _tCashOutStatus.enabled = false;
            _cashOutHdrMat = MakeHdrMaterial();
            if (_cashOutHdrMat != null) _tCashOut.material = _cashOutHdrMat;

            // §8.7 / §8.5 Pending window: "intervention controls live in their own overlay, never
            // in [the cash-out] row." Centered over the stage's safe area, where the frozen shot
            // remains visible.
            _tInterventionPrompt = MakeText(root, "InterventionPrompt", new Vector2(0f, 1f), new Vector2(0.5f, 0.5f),
                AnchorCenter(grid.Stage), new Vector2(grid.Stage.width - 80f, 90f), 22,
                TextAnchor.MiddleCenter, new Color(gold.r, gold.g, gold.b, 1f), FontStyle.Bold);
            _tInterventionPrompt.enabled = false;
        }

        private void BuildChromeStrip(Transform root, LayoutGrid grid)
        {
            // §8.1: "System chrome (round, bank, payment, seed) remains lowest priority and may
            // stay small." A thin reserved strip along the very bottom edge, outside the five
            // sweat zones.
            // T25.1: the height compensates for the 2px top pad. Padding shifted the row down
            // without shrinking it, so its last 2px sat BELOW the glass — the containment audit
            // caught this one, which no capture ever would have: 2px of the lowest-priority row is
            // invisible to the eye and still a layer rendering off the panel.
            _tChrome = MakeText(root, "Chrome", new Vector2(0f, 1f), new Vector2(0.5f, 1f),
                AnchorTopCenter(grid.ChromeStrip, 2f),
                new Vector2(grid.ChromeStrip.width - 30f, grid.ChromeStrip.height - 2f),
                14, TextAnchor.UpperCenter, contextGrey);
        }

        /// <summary>DESIGN.md §6: "Zones may be separated by hairline rules or by unlit gutters
        /// ... What remains banned is a stroked box around a region." Five single dividing lines —
        /// never a border loop around any zone.</summary>
        private void BuildHairlines(Transform root, LayoutGrid grid)
        {
            const float hairline = 1.5f;
            // Ticket rail | right region.
            MakeHairlineV(root, "GridDividerVertical", grid.TicketColumn.width, 0f, grid.CashOut.yMax, hairline);
            // Scorebug | stage.
            MakeHairlineH(root, "GridDividerScoreStage", grid.Stage.x, grid.Stage.y, grid.Stage.width, hairline);
            // [ticket column + stage] | [cash-out + event strip].
            MakeHairlineH(root, "GridDividerBottomRow", 0f, grid.CashOut.y, grid.ScoreBug.xMax, hairline);
            // Ticket header | leg rows.
            MakeHairlineH(root, "GridDividerTicketHeader", 0f, grid.TicketHeader.yMax, grid.TicketColumn.width, hairline);
            // Leg rows | RISK/PAYS footer.
            MakeHairlineH(root, "GridDividerTicketFooter", 0f, grid.TicketFooter.y, grid.TicketColumn.width, hairline);
        }

        private void MakeHairlineH(Transform root, string name, float x, float yTop, float width, float thickness)
        {
            MakePanel(root, name, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x, -yTop), new Vector2(width, thickness), structureGrey);
        }

        private void MakeHairlineV(Transform root, string name, float x, float yTop, float height, float thickness)
        {
            MakePanel(root, name, new Vector2(0f, 1f), new Vector2(0.5f, 1f),
                new Vector2(x, -yTop), new Vector2(thickness, height), structureGrey);
        }

        // ---- grid → canvas-space conversion ----
        // All zone builders anchor their elements at (0,1) — the canvas's own top-left corner —
        // and vary only the element's PIVOT to get left/right/center alignment. This keeps every
        // element's anchoredPosition a direct read of LayoutGrid's top-left-origin Rects, with no
        // per-element coordinate-system bookkeeping.

        private static Vector2 AnchorTopLeft(Rect zone, float padX = 0f, float padY = 0f)
            => new Vector2(zone.x + padX, -(zone.y + padY));

        private static Vector2 AnchorTopRight(Rect zone, float padX = 0f, float padY = 0f)
            => new Vector2(zone.xMax - padX, -(zone.y + padY));

        private static Vector2 AnchorTopCenter(Rect zone, float padY = 0f)
            => new Vector2(zone.x + zone.width * 0.5f, -(zone.y + padY));

        private static Vector2 AnchorCenter(Rect zone)
            => new Vector2(zone.x + zone.width * 0.5f, -(zone.y + zone.height * 0.5f));

        private Text MakeText(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 pos,
            Vector2 size, int fontSize, TextAnchor align, Color color,
            FontStyle style = FontStyle.Normal, Face face = Face.Regular)
        {
            var go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            // TV-19: canon assigns the face per slot (tokens/fonts.css), read off the component
            // references one at a time — it is not a stylistic default. Falls back to the regular
            // face if the condensed asset is missing, rather than rendering nothing.
            Font want = face == Face.Condensed && _fontCond != null ? _fontCond : _font;
            if (want != null) t.font = want;
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

        /// <summary>T46 (layout defect, DD 2026-08-02): makes a zone panel the OWNER of its content
        /// rather than a backdrop its content happens to sit beside, and clips it to its own region.
        ///
        /// The finding was the scoreline and the pitch painted over the ticket column's leg text —
        /// "struck-through identities, BIFF RACKET TO SCORE cut mid-word". The grid was never the
        /// problem: <c>ScoreBug</c> and <c>Stage</c> start at exactly <c>TicketColumn</c>'s right
        /// edge. Three structural facts produced the overdraw, and none of them is a number:
        ///
        /// <list type="number">
        /// <item>Every zone's content was a direct child of the canvas, so no zone owned anything.</item>
        /// <item><see cref="MakeText"/> builds with <see cref="HorizontalWrapMode.Overflow"/>, so a
        /// long fixture centred in the score bug's 675px box spills symmetrically — and past ~357px
        /// of spill the left edge crosses into the column.</item>
        /// <item>The right-hand zones are built AFTER <c>BuildTicketColumn</c>, so wherever they
        /// reach into it they win the z-fight.</item>
        /// </list>
        ///
        /// The ruling is "the ticket column owns its width absolutely; the stage clips to its
        /// region". A clip rect is the only one of the three that is structural: it does not depend
        /// on any string staying short, on build order, or on a future element remembering a rule.
        /// §6 is untouched — nothing here is computed from content; overflow simply stops at the
        /// zone edge instead of continuing into a neighbour.
        ///
        /// The canvas-level <see cref="RectMask2D"/> (T25.1, "the glass clips") is unaffected and
        /// still bounds everything at the screen edge; masks nest, so the two intersect. HDR
        /// elements clip correctly through both because <c>TvSweatHdrUI.shader</c> carries
        /// <c>UNITY_UI_CLIP_RECT</c> — which is exactly why T25.1's fix bound the brightest layer,
        /// and why <c>Score</c>'s L4 punch overlay can live inside this mask.</summary>
        private static Transform ZoneRoot(Image zone)
        {
            zone.gameObject.AddComponent<RectMask2D>();
            return zone.transform;
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


        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // T8 (Allen, 2026-07-31): BuildScanlineTexture and RegenNoise are REMOVED with the two
        // overlays they fed, along with MakeStretchRaw/SetRawAlpha, which had no other callers once
        // both RawImage overlays were gone.
        //
        // RegenNoise was one of two UnityEngine.Random uses in this file; the other survives at
        // _emissSeed's initialisation. PRD §4.3 bans that API for any *discrete scene choice* — a
        // flicker phase seed is not one, so it is out of T8's scope and is left alone. It is
        // recorded in docs/handoffs/tv-sweat.md §6 rather than changed here, because it means the idle
        // emission flicker differs run to run.

        // ---------------------------------------------------------------- small helpers

        /// <summary>A fresh instance of the HDR-capable UI material (SBR/TvSweatHdrUI.shader), for
        /// the handful of Graphics that must carry §3's L4 tier above 1.0 — the ordinary UGUI
        /// pipeline bakes Graphic.color into a Color32 vertex attribute and clamps there regardless
        /// of camera/URP HDR settings, so this shader routes the boost through an unclamped float
        /// material property (`_HdrBoost`) instead. Returns null (caller keeps the default UI
        /// material) if the shader isn't in the build — never throws, never silently misrenders.</summary>
        private Material MakeHdrMaterial()
        {
            if (_hdrUiShader == null && !_hdrShaderMissing)
            {
                _hdrUiShader = Shader.Find("SBR/TvSweatHdrUI");
                _hdrShaderMissing = _hdrUiShader == null;
                if (_hdrShaderMissing)
                    Debug.LogWarning("[TvSweatScreen] SBR/TvSweatHdrUI shader not found; the L4 " +
                        "cash-out/payout elements will render LDR-clamped at 1.0.");
            }
            return _hdrUiShader != null ? new Material(_hdrUiShader) : null;
        }

        /// <summary>C3's one-token invariant: the ONLY place any material's <c>_HdrBoost</c> is
        /// pushed to <see cref="HdrBoostL4"/> or back to <see cref="HdrBoostL3"/>. Boosts the
        /// requested <paramref name="focus"/> and, if it took the token from a different focus,
        /// drops that previous holder back to L3 in the SAME call — so a loser never has to wait
        /// for its own next frame to notice it lost (C3 rule 4: "the sustained element yields").
        /// <paramref name="momentary"/> punches (a goal's score, a payoff's ball, a win/cash-out
        /// tally) always take the token from whatever currently holds it. A sustained hold (the
        /// cash-out band staying gold while actionable) only succeeds while nothing else holds the
        /// token. Returns whether <paramref name="focus"/> now holds L4.</summary>
        private bool RequestL4(HdrFocus focus, bool momentary)
        {
            if (_l4Holder == focus) return true; // already holding — idempotent, no re-apply needed
            if (_l4Holder != null && !momentary) return false; // a sustained request never preempts

            HdrFocus? previous = _l4Holder;
            _l4Holder = focus;
            if (previous.HasValue) ApplyBoost(previous.Value, HdrBoostL3);
            ApplyBoost(focus, HdrBoostL4);
            return true;
        }

        /// <summary>Gives the token back up, if <paramref name="focus"/> is the one currently
        /// holding it (a no-op otherwise — releasing a token you never held must not clobber
        /// whoever holds it now).</summary>
        private void ReleaseL4(HdrFocus focus)
        {
            if (_l4Holder != focus) return;
            _l4Holder = null;
            ApplyBoost(focus, HdrBoostL3);
        }

        /// <summary>Clean slate: no focus holds the token and every HDR-eligible material sits at
        /// L3. Called at the top of every new session (ResetForNewSession) — "a new session starts
        /// with no L4 element live."</summary>
        private void ResetL4()
        {
            _l4Holder = null;
            _cashOutHdrMat?.SetFloat(HdrBoostId, HdrBoostL3);
            _bigAmountHdrMat?.SetFloat(HdrBoostId, HdrBoostL3);
            _goldFloodHdrMat?.SetFloat(HdrBoostId, HdrBoostL3);
            _scoreHdrMat?.SetFloat(HdrBoostId, HdrBoostL3);
            _ballHdrMat?.SetFloat(HdrBoostId, HdrBoostL3);
        }

        private void ApplyBoost(HdrFocus focus, float boost)
        {
            switch (focus)
            {
                case HdrFocus.CashOut:
                    _cashOutHdrMat?.SetFloat(HdrBoostId, boost);
                    break;
                case HdrFocus.Payout:
                    _bigAmountHdrMat?.SetFloat(HdrBoostId, boost);
                    _goldFloodHdrMat?.SetFloat(HdrBoostId, boost);
                    break;
                case HdrFocus.Score:
                    _scoreHdrMat?.SetFloat(HdrBoostId, boost);
                    break;
                case HdrFocus.Ball:
                    _ballHdrMat?.SetFloat(HdrBoostId, boost);
                    break;
            }
        }

        private static void SetAlpha(Image img, float a)
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

        /// <summary>The TV's typeface. Canon names it — `tokens/fonts.css`:
        /// <c>--font-tv: "Encode Sans"</c> — and until now this surface rendered
        /// <c>LegacyRuntime.ttf</c> instead, so it had never once been seen in its own face.
        ///
        /// <para>That was not only a fidelity gap. T20 re-derived the whole px scale from canon
        /// values that were measured against Encode Sans, then shipped them into a WIDER face, which
        /// is why the seated captures show <c>MARKET SUSPENDED</c> clipped to <c>ARKET SUSPENDED</c>
        /// and leg copy running out of the ticket column. The strings are correct and §6 forbids
        /// shortening them; the face was wrong.</para>
        ///
        /// <para>Falls back to the built-in font rather than returning null: a missing font asset
        /// should degrade to readable-but-wrong, never to an invisible surface. The fallback is
        /// logged loudly because silently rendering in the wrong face is exactly the failure this
        /// change exists to end.</para></summary>
        private static Font LoadFont() => LoadFace("Tv/Fonts/EncodeSans", "--font-tv");

        private static Font LoadFontCondensed()
            => LoadFace("Tv/Fonts/EncodeSansCondensed", "--font-tv-cond");

        /// <summary>Resolves one of canon's two TV faces (`tokens/fonts.css`). Falls back to the
        /// built-in face rather than to null: a missing font asset should degrade to
        /// readable-but-wrong, never to an invisible surface.
        ///
        /// <para>The fallback logs loudly and names the token, because every px value on this
        /// surface was derived against Encode Sans — T20's whole re-derivation is a metrics
        /// argument — so copy fit and the type scale are NOT valid in the fallback. Silently
        /// rendering in the wrong face is the failure this exists to end.</para></summary>
        private static Font LoadFace(string resourcePath, string token)
        {
            Font face = Resources.Load<Font>(resourcePath);
            if (face != null) return face;

            Debug.LogWarning($"[TvSweatScreen] {token} not found at Resources/{resourcePath} — falling " +
                "back to the built-in face. Copy fit and the T20 type scale are NOT valid in the fallback.");
            try { return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch
            {
                Debug.LogWarning("[TvSweatScreen] built-in font not found either; text will not render.");
                return null;
            }
        }
    }
}
