using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SBR.Engine;
using TMPro;
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

        /// <summary>Rebuilds the mirror for a new session. <paramref name="ticketWinProb"/> is the
        /// session's TICKET-level live win probability (T164) — this class holds no session handle,
        /// so the pregame seed is passed in rather than derived here.</summary>
        internal void Reset(Run run, Ticket current, int currentIndex, double ticketWinProb)
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
            // T164: the seed is the TICKET's probability, never a leg's — T143 governs (no leg's
            // probability is ever shown alone), and after the fixture restructure two or more legs
            // can be live at once, so a leg-derived seed is a visible lie the moment N > 1.
            WinProbability = HasTicket ? (float)ticketWinProb : 0f;
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

        /// <summary>T87-am2: the MINIMUM HOLD the drawn match's closing line takes before the leg's
        /// grade may displace it.
        ///
        /// <para>Batch 68 ruled the line *"holds until the leg's own grade displaces it"* and that
        /// assumed a window exists. On a won draw-backer it does not — the match's ending and the
        /// leg's resolution are the SAME INSTANT, so a line that yields to the grade yields before it
        /// is ever seen. `scene001` had `LEG 1 — WON` up at frame 000, the whistle itself.</para>
        ///
        /// <para>MATCHED to <see cref="ticketDeadConsolationDuration"/> rather than invented, on the
        /// ruling's own instruction: it is the same kind of thing — a statement the player must read
        /// before the beat moves on — and `scene002` already carried 0.62 sim-seconds of dead window
        /// by accident. This makes the gap explicit and gives it a floor.</para></summary>
        public float drawnEndingHoldDuration = 1.0f;
        public float cashOutFloodDuration = 0.8f;
        // T40 (batch 27): `winFloodDuration` is REMOVED with the flood it timed — removed, not
        // zeroed, so a serialized value in Room.unity cannot resurrect it. WinBeat's own pacing is
        // unchanged: it is `winTallyDuration` then the remainder of `winConfettiDuration`, both of
        // which the flood ran alongside rather than gated.
        public float cashOutTickDuration = 0.4f;
        public double cashOutRoundMultiple = 100.0;
        public float winTallyDuration = 1.2f;
        public float winConfettiDuration = 2.0f;
        public int winConfettiCount = 40;
        [Tooltip("C3: how long the score-at-a-goal / ball-at-a-payoff momentary L4 punch holds "
            + "the HDR token before yielding it back.")]
        public float hdrPunchDuration = 0.4f;

        [Header("Feel dials")]
        // T64 (DD 2026-08-07): `idleEmissionFlicker = 0.05` drove a 9 Hz Perlin flicker on the idle
        // emission and is STRUCK. It failed three laws, any one sufficient: (1) the display is a
        // decade old and WORKS — a flickering panel is the broken register, T8's exact ground one
        // channel over; (2) one pulse kind on the whole surface and it is LIVE — a second animated
        // channel running permanently underneath the first is R37 on the TV; (3) it had no fire
        // condition, so it was continuous involuntary motion in peripheral vision for the whole sweat.
        // REMOVED, not zeroed — per R37's own reasoning, a dead dial invites the effect back.
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

        // ---- T65: the room re-tint. Gate V6 on the owning document. -------------------------
        //
        // The defect: a LEG win fired `tvLight.Flash(gold, 3.0f)`, and gold's hue is 39.6 deg. The
        // room went with it — measured across the invert burst, the housing rotated from 130 deg to
        // 40.7 deg, saturation 40.5% -> 71.1%, Rec.709 luma 0.176 -> 0.347. That is T40's deleted
        // full-field gold wash relocated to a larger surface, and T40's words apply unamended: a
        // full-field wash spends the whole gold ration in one frame and is a celebration.
        //
        // THE RULE THIS ENFORCES: the panel glows what it SHOWS (the emissive quad follows the
        // canvas, which is honest — a lit object in a room); the room light is a SEPARATE
        // instrument bound to the ROOM's palette, never to the TV's money colour. Gold does not
        // leave the panel. Every room re-tint goes through RoomSettlementGlow() so no future site
        // can invent its own — fixed by RULE, which is the lesson `WonLegBeat` itself already paid
        // for once (see the beat below) and T39 paid for twice.
        //
        // THE VALUE, derived from the measurement rather than picked:
        //   Measured response of the room to this light, per unit (intensity x light-channel), off
        //   the frame000/frame006 pair on the `housing above panel` box:
        //       dR +82.6, dG +36.5, dB -0.9  from gold (1.0, 0.713, 0.157) at 3.0
        //   The room returns essentially NO blue from this light, so a warm re-tint necessarily
        //   raises saturation; the lever is how far. Solving for a cast at hue 88 deg (mid-band)
        //   with a 1.35x luma lift gives a red gain of 21.0 and a green gain of 15.9, i.e. a light
        //   whose channels sit at (0.818, 1.000, 0.610) — hue 88.0 deg, saturation 39%, which is no
        //   more saturated than the room already is at rest (40.5%).
        //
        // WHY THE INTENSITY IS AN UPPER BOUND (C25): the capture caught the flash mid-decay at an
        // unknown fraction, so the true response is at least what was measured and the intensity
        // needed is at most this. Expect to lower it, never raise it.
        //
        // WHY THE GATE MATTERS: the cast is monotonic in amplitude — 130 deg at zero, falling
        // through the band to ~45.5 deg as amplitude rises. It crosses 85-92 deg exactly once, over
        // an amplitude window of roughly [0.78, 1.06]. That is a +/-15% window, which is why clause
        // 4 says bounded by MEASUREMENT and not by eye. Too hot reads amber, too cold reads green;
        // V6 catches both edges because it prints the hue.
        [ColorUsage(false, false)] public Color roomSettlementWarm = new Color(0.818f, 1.000f, 0.610f);
        [Tooltip("T65: upper bound pending the V6 re-shoot. In-band window is about [0.78, 1.06].")]
        public float roomSettlementIntensity = 0.9f;
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
        /// <summary>Test/debug hooks for §8.8's stats panel, in the shape this surface already uses
        /// for <see cref="ForceSeated"/> and <see cref="DebugCashOutAnimating"/>. The panel's
        /// contract is a freeze, a z-order and an unrevealed mark, and none of the three is
        /// observable from outside the sweat without these.
        ///
        /// <para><see cref="DebugSeatedDeltaTime"/> exposes the freeze at its SINGLE AUTHORITY rather
        /// than sampling one of its consequences: every frozen channel §8.8 lists reads this one
        /// expression, so asserting it is asserting all of them, and a pin on (say) the cash-out
        /// tween alone would leave the other ten unasserted.</para></summary>
        public void ForceStatsPanel(bool open) => SetStatsPanel(open);

        /// <summary>FORCES the footer's RIGHT half to a literal, for `T133`'s rung-2 frame only.
        ///
        /// <para><b>This is an S99-style forcing and it is NOT a shipped state.</b> `S3` reached an
        /// otherwise-unreachable empty group with a non-shipped `CorrectScoreFloor = 0.08` and put
        /// the disclosure on the frame's face; this is the same device for the same reason — the
        /// enumerated worst case (`$73,318,376,502`, eleven digits over 648,000 priced offers)
        /// cannot be dealt for in a capture, and `C11` puts a copy decision on a frame rather than
        /// on a px number.</para>
        ///
        /// <para><b>Every frame it produces is named `FORCED-…` and the dock says so</b>, because a
        /// forced frame that does not disclose its forcing is evidence for a state the product does
        /// not have. The next repaint overwrites it — this sets no flag and latches nothing.</para></summary>
        public void ForcePaysTextForCapture(string literal)
        {
            if (_tPays != null) _tPays.text = literal;
        }

        /// <summary>The FIRST money row's literal, symmetric to <see cref="ForcePaysTextForCapture"/>.
        /// Added at T147 because the composition put the two facts on separate rows and the settled
        /// pair — <c>STAKE</c> over <c>RETURNED</c> — cannot be photographed with only the second row
        /// forceable. Same discipline: LATCHES NOTHING (any repaint overwrites it, so re-force
        /// immediately before each burst) and every frame taken through it carries <c>FORCED-</c> in
        /// its filename.</summary>
        public void ForceRiskPaysTextForCapture(string literal)
        {
            if (_tRiskPays != null) _tRiskPays.text = literal;
        }

        /// <summary>T147-am2's OPPOSITE-ANCHOR ARM, for E1's ruler and for nothing else.
        ///
        /// <para>The ruling builds the two money rows LEFT/LEFT, but the money control is a
        /// counter-precedent — it kept opposite anchors when it split onto two rows — so the
        /// alignment is an open choice with a precedent against it and goes to a frame. This
        /// re-anchors the second row so both arms can be shot from one build, rather than the
        /// alternative being argued from a description of itself.</para>
        ///
        /// <para><b>A FORCED STATE MUST DISCLOSE ITS FORCING.</b> Any frame shot through this call
        /// carries `FORCED-` in its filename, exactly as `ForcePaysTextForCapture`'s do: a frame
        /// that hides its forcing is evidence for a state the product does not have. The product
        /// ships left/left until this seat rules otherwise.</para>
        ///
        /// <para>Self-inverse and grid-free: it reads the row's own pivot and width, so it needs no
        /// LayoutGrid and calling it twice returns the row to where it started. Read the SETTLED
        /// state on the result — `RISK`/`PAYS` are both four characters and align either way, while
        /// `STAKE`/`RETURNED` are five and eight and are where left/left goes ragged.</para></summary>
        public void ForcePaysAnchorForCapture(bool rightAnchored)
        {
            if (_tPays == null) return;
            RectTransform rt = _tPays.rectTransform;
            bool isRightNow = rt.pivot.x > 0.5f;
            if (isRightNow == rightAnchored) return;
            float w = rt.sizeDelta.x;
            rt.pivot = new Vector2(rightAnchored ? 1f : 0f, 1f);
            rt.anchoredPosition += new Vector2(rightAnchored ? w : -w, 0f);
            _tPays.alignment = rightAnchored
                ? TextAlignmentOptions.TopRight
                : TextAlignmentOptions.TopLeft;
        }
        public bool DebugStatsPanelOpen => _statsOpen;
        public float DebugSeatedDeltaTime => SeatedDeltaTime;
        public Transform DebugStatsPanel => _statsPanel;
        public TMP_Text DebugInterventionPrompt => _tInterventionPrompt;
        /// <summary>DD batch 95: null past <see cref="StatsActiveRowCount"/> means the row is ABSENT
        /// — there is no slot there for this ticket's row set, not a blank one. A row inside that
        /// count is always PRESENT (label/A/B all populated, or the unrevealed mark) even if it has
        /// not been rendered yet this frame.</summary>
        public string DebugStatsRow(int i)
            => _tStatsLabel == null || i < 0 || i >= StatsActiveRowCount
                ? null
                : $"{_tStatsLabel[i].text}|{_tStatsA[i].text}|{_tStatsB[i].text}";
        public string DebugStatsUnrevealedMark => StatsUnrevealed;
        /// <summary>T102/S84's ratio, exposed so the guard test asserts against the SAME instrument
        /// <see cref="BuildStatsPanel"/> sizes columns with, rather than a second, driftable copy of
        /// "0.8" living in the test file.</summary>
        public float DebugStatsMaxInkFraction => MaxInkFraction;
        /// <summary>The REVEALED goals, for the capture harness's one binding condition (T99): the
        /// panel must not be shot over a 0–0, because a covered scorebug carrying no information
        /// cannot fail any reading of it. Read from the revealed ledger, never the locked StatLine —
        /// the harness must wait for a fact the player can actually see.</summary>
        public int DebugRevealedPicked => _ledger != null ? _ledger.Picked : 0;
        public int DebugRevealedOpponent => _ledger != null ? _ledger.Opponent : 0;
        /// <summary>T100's condition: the REVEALED per-team count on a count leg. <b>−1 means there
        /// is no count ledger at all</b> — the live leg is not a corners or cards leg — which is a
        /// different state from "a count leg that has revealed nothing yet", and the capture must be
        /// able to tell them apart or it will wait forever on the wrong one. Revealed totals, never
        /// <c>TargetHome</c>/<c>TargetAway</c>, which are the locked endpoint.</summary>
        public int DebugRevealedCountHome => _countLedger != null ? _countLedger.Home : -1;
        public int DebugRevealedCountAway => _countLedger != null ? _countLedger.Away : -1;
        /// <summary>Test/debug hook (TVS-H01 regression): true while the cash-out amount is mid-tween
        /// (AnimateCashOut running). Reads _cashOutTweening, not _cashOutAnimation directly — the
        /// Coroutine handle isn't assigned until StartCoroutine returns, one instant after the
        /// tween's own first render already ran (TVS-H02 fix, see _cashOutTweening's declaration).
        /// _cashOutAnimation is otherwise unobservable from outside the sweat, and this is the exact
        /// condition CanAcceptCashOutNow also refuses.</summary>
        public bool DebugCashOutAnimating => _cashOutTweening;
        /// <summary>T75-am: the shared regular face, and the face `_tBigAmount` actually holds.
        ///
        /// <para>The DD's carve-out originally asked for that slot to be verified tabular ON FRAMES.
        /// It cannot be: `_tBigAmount` renders nothing — both payoff figures moved into the cash-out
        /// slot at T68-am/T71 — so it appears in no capture and never will. T75-am re-cast the debt
        /// as an ASSIGNMENT and an ASSERTION instead, and this is the assertion's surface.</para>
        ///
        /// <para>Two references, not two names, so the test compares identity: the slot must carry
        /// the SHARED asset, not an equal-looking one. A per-slot copy would pass any check written
        /// against the font's name and would quietly double the atlas.</para></summary>
        public TMP_FontAsset DebugRegularFont => _font;
        public TMP_FontAsset DebugBigAmountFont => _tBigAmount != null ? _tBigAmount.font : null;
        /// <summary>Test/debug hook: has a cash-out figure been rendered at least once? This is the
        /// DURABLE precondition behind <see cref="DebugCashOutAnimating"/> — until it is true,
        /// SetCashOutOffer takes its first-time branch and no tween can ever start. A test that
        /// wants to observe a tween polls this (a state that latches) rather than the animation
        /// flag (a state that passes).</summary>
        public bool DebugHasCashOutShown => _hasCashOutShown;
        /// <summary>Debug accessors for the ticket footer and each leg row's own text — read-only
        /// mirrors of what actually rendered, in the same null-safe-returns-empty-string shape as
        /// <see cref="DebugStatsRow"/>, every index guarded the same way. Exist so a PlayMode pin can
        /// watch the footer word (RISK/STAKE) never disagree with a row's own progress line or state
        /// chip.</summary>
        /// <summary>The §6.1 money control's own text — the slot that carries `CASH OUT $x`,
        /// `CASHED OUT $x`, `SUSPENDED` and, at a win, the payout tally (`T71` puts `WinBeat`'s
        /// `+$X` on the same treatment).
        ///
        /// <para>Added for `T129` condition (e): *every ending runs PAST its own tally, verified by
        /// the payout slot changing and then settling.* That is only checkable if the slot can be
        /// READ per frame — otherwise a window that ends mid-tally looks identical to one that
        /// resolved, and the set cannot answer the question it was shot for. Read-only, and the same
        /// null-safe-returns-empty shape as its siblings above.</para></summary>
        public string DebugCashOutText => _tCashOut != null ? _tCashOut.text : string.Empty;
        public string DebugTicketRiskText => _tRiskPays != null ? _tRiskPays.text : string.Empty;
        /// <summary>The PAYS half of the footer — see <see cref="DebugTicketRiskText"/>.</summary>
        public string DebugTicketPaysText => _tPays != null ? _tPays.text : string.Empty;
        /// <summary>Row <paramref name="i"/>'s live progress text (empty on a resolved/NEXT row, or
        /// out of range).</summary>
        public string DebugLegProgress(int i)
            => _legRow == null || i < 0 || i >= _legRow.Length || _legRow[i].Progress == null
                ? string.Empty : _legRow[i].Progress.text;
        /// <summary>Row <paramref name="i"/>'s live NEED text (empty on a resolved/NEXT row, or out
        /// of range).</summary>
        public string DebugLegNeed(int i)
            => _legRow == null || i < 0 || i >= _legRow.Length || _legRow[i].Need == null
                ? string.Empty : _legRow[i].Need.text;
        /// <summary>Row <paramref name="i"/>'s state chip — <c>"W"</c>/<c>"L"</c>/<c>"VOID"</c>/
        /// <c>"NEXT"</c>, or empty on a live row (the live row's chip is blanked) or out of
        /// range.</summary>
        public string DebugLegState(int i)
            => _legRow == null || i < 0 || i >= _legRow.Length || _legRow[i].State == null
                ? string.Empty : _legRow[i].State.text;
        /// <summary>Test/debug hook: displace the SHOWN cash-out figure so that the next natural
        /// offer read must take the tween branch of SetCashOutOffer.
        ///
        /// <para>The PlayMode tests that assert mid-tween behaviour used to wait for the simulation
        /// to move the price on its own, inside a fixed timeout. That is a race by construction: a
        /// tween starts only when a new offer differs from the shown figure by 0.005 or more, so
        /// the wait depended on unpinned generated content arriving inside the window. It failed
        /// about one run in N on a byte-identical tree and passed on re-run.</para>
        ///
        /// <para>This does NOT fake the tween — it moves only the displayed figure, so the real
        /// production path (Update → SetCashOutOffer → AnimateCashOut) is still what runs and still
        /// what the assertions observe. Returns false if no figure has been shown yet, so a caller
        /// cannot silently displace nothing.</para></summary>
        public bool ForceCashOutDisplacement(double delta)
        {
            if (!_hasCashOutShown) return false;
            _cashOutShown += delta;
            return true;
        }

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
        private float _probTarget; // data-only now (RevealedView.WinProbability) — Layout B carries
                                    // no standalone win% visual; DESIGN.md §7's component list has no
                                    // slot for one, and the ticket column's NEED/LIVE copy is the
                                    // PRD-sanctioned channel for "what does the leg still need".
        // T164: the picked side's live prob — the tension bed's referent. Distinct from _probTarget,
        // which is the displayed TICKET number: crowd tension is a per-MATCH dramatic fact (exactly
        // like TheaterStage's territory), and driving it off a multi-leg ticket's product would sit
        // it far from the coin-flip peak forever, flattening the bed on every parlay.
        //
        // IT MOVES ONLY ON THE MIRROR'S OWN SEAMS, AND THAT IS THE WHOLE POINT OF THE PAIR BELOW.
        // Before T164 the bed read RevealedView.WinProbability, which lands at the REVEAL
        // (RevealBeatChrome / FinalSlam / Reset / Clear) and never at RenderEvent — the causal
        // reveal law, M-T3.1. A referent set when the beat is CONSUMED would swell the crowd before
        // the pitch has shown the story: an audible tell on a dangerous scene, where the mirror's
        // number is pinned to hold (LaptopOsTests, "the reveal owns it") but the bed would not.
        // So the leg's number is STASHED at RenderEvent and LANDED at the same instant _probTarget
        // is, sample for sample. TheaterStage's territory keeps its own pre-existing early timing —
        // scene playback supersedes it during a scene — and is deliberately not folded in here.
        private float _tensionProb = 0.5f;
        private float _pendingTensionProb = 0.5f;
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
        // _marketSuspended's MARKET state. Two sites paint "SUSPENDED" — SuspendMarket (the
        // market really is suspended) and PendingWindowBeat (§8.7's intervention overlay, which
        // renders the same slate while _marketSuspended is still false, because ResolveBeat never
        // suspends). Keying the presentation off _marketSuspended alone would leave the pending
        // window's gold field lit under the suspended word for the whole window. This flag is what
        // ShowMarketSuspended sets and what ApplyCashOutSlotState reads, so BOTH sites get the same
        // slate. It is deliberately NOT wired into CanAcceptCashOutNow: that predicate is TVS-H01's
        // input contract, and changing what E accepts during a pending window is a design call for
        // the DD, not a presentation fix. See ApplyCashOutSlotState.
        private bool _cashOutSlotSuspended;
        // T68: the last value ApplyCashOutSlotState derived for the field. Read by the per-frame
        // taunt so it cannot repaint the amount gold on a lit (inverted) field. Kept as a flag for
        // the same reason `_cashOutTweening` is one — the state must be readable from outside the
        // one method that computes it, without that method's predicate being duplicated.
        private bool _cashOutFieldLit;
        // T68-am / T71: §6.1's `accepted` state. Both payoff moments — the cash-out accept and the
        // ticket's win tally — render their money figure HERE, in the slot, over the flood rather
        // than against it. Set by ShowCashOutAccepted, cleared by HideCashOutSlot, and read by the
        // one derivation so the state cannot be half-entered.
        private bool _cashOutAccepted;
        private float _cashOutScale = 1f;
        private float _cashOutFlash;
        private int _cashOutRoundShown;
        // Anytime-scorer, per leg: true only at the leg's causal identity payoff — see
        // DescribeActiveLeg / OnGoalPlayed. Reset per-leg in BeginStageLeg.
        private bool _scorerRevealedForActiveLeg;

        // ---- input ----
        private InputAction _interact;

        /// <summary>T22/T36's SECOND KEY — the one that commits during a hold. C48 (batch 50) makes
        /// the label the contract on a money control, and T88 rules the gesture for every spending
        /// option: <i>hold to preview, release abandons, a second key during the hold commits, no
        /// timer, no auto-commit.</i> A press commits nothing anywhere on this surface.
        ///
        /// <para><b>Which key is UNRATIFIED and deliberately one constant.</b> T22 and T36 both
        /// specify "a second key" and neither names it, so this is the seat's choice and is routed
        /// for ratification rather than presented as canon. Enter was taken because it is bound to
        /// nothing in the shared action asset (Move/Look/Jump/Crouch/Interact/Attack/Cancel), so the
        /// gesture reserves no key the room lane is using, and because ONE commit key across both
        /// money controls is what keeps their contracts readable as the same contract. Changing it is
        /// this constant and the word below.</para>
        ///
        /// <para>Read straight off <see cref="Keyboard"/> rather than added to
        /// <c>InputSystem_Actions.inputactions</c>: that asset is the ROOM lane's contract, and the
        /// intervention window already reads its M/R/N the same way. Nothing shared is touched.</para></summary>
        private const string ConfirmKeyWord = "ENTER";

        /// <summary>The commit half of the gesture. Momentary by construction — a second key that
        /// auto-repeated would be a timer, which T22/T36 forbid.</summary>
        private static bool ConfirmPressed()
            => Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame;

        // The stand-suppression question this gesture raised is ANSWERED, and the answer removed the
        // mechanism rather than tuning it. See CashOutLive() — there is no frame arithmetic here.

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

        /// <summary>The live-leg SET the ticket column was last rendered with, so entering and
        /// leaving the preview can re-render the same column without inventing a live leg.
        ///
        /// <para>A SET rather than an index since <c>T140</c> arm A: a telling is a (ticket,
        /// FIXTURE) and every leg riding that fixture is live for the whole telling, so "the live
        /// leg" is no longer a quantity this surface can hold. EMPTY is what <c>-1</c> used to
        /// mean — no live row. Held by VALUE, not by reference to the session's own list.</para></summary>
        private readonly List<int> _liveLegsShown = new List<int>();

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

        /// <summary>The event strip's ONE painting point. Takes the raw ink and applies the tier
        /// itself, so no call site can choose a tier and none can forget one.
        ///
        /// <para>Batch 14 ruled the strip to <b>L2</b> across every site: "the loud-while-running
        /// split is NOT intended." Before this there were fourteen assignments — three at L2 (the
        /// leg-resolution beats) and eleven at raw alpha 1.0, which is the L4 tier value. Measured
        /// in C33's unit, the raw ones put the strip at <b>0.858 Rec.709</b> against a quiet
        /// scoreline of 0.866: not separated at all, on a surface whose first law is that
        /// brightness is the semantic channel.</para>
        ///
        /// <para>The history is in <see cref="AtTier"/>'s own comment, which names "score, clock,
        /// NEED, progress <i>and the event strip</i>" as the elements TV-S1 found at identical
        /// maximum brightness. TV-S1's sweep reached the first four; the strip is the one element
        /// in its own fix's list that the fix did not finish. Routing every site through here is
        /// what stops that happening a third time — the tier is now structural, not a convention
        /// each new beat has to remember.</para>
        ///
        /// <para>Hue is still the caller's: cold white for a fact, grey for a confirmed loss, cyan
        /// for VOID, gold for money. Only the TIER is taken away from the call site.</para></summary>
        private void SetEventStrip(Color ink)
        {
            if (_tFlavor != null) _tFlavor.color = AtTier(ink, TierL2);
        }

        // ---- emission (the quad's own glow) ----
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private MaterialPropertyBlock _emissBlock;
        private Color _emissIdle;
        private Color _emissRest;
        private Color _emissFlash;
        private float _emissFlash01;

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
        //
        // T49 — RULED 1.4 and SEALED (DD 2026-08-04), on the frame-for-frame A/B this slice shot.
        // The pair is very nearly a null result: pitch, cash-out band and the halo around it are
        // identical to three decimal places across all six pairs. The ONLY region that measurably
        // differs is the goal-flash scoreline — the element carrying T58's defect — where 1.8 pushed
        // an already-offending gold ~4.7% hotter (0.755-0.757 against 1.4's 0.721-0.723).
        //
        // So 1.4 is chosen on the ladder, not on taste: the arms are equivalent everywhere the
        // surface is behaving, and where they differ, 1.8 widens the gap between the designated L4
        // element and the thing outshining it. When two settings are otherwise equal, take the one
        // that does less damage to the law.
        //
        // **The bloom question is SEALED. Do not re-open it to fix anything.** The finding worth more
        // than the pick is that bloom was never the lever this question assumed: a ±0.4 change moves
        // nothing on this surface except one element that was the wrong colour. Fix findings
        // elsewhere — T58 is where that colour was fixed.
        private const float HdrBoostL4 = 1.4f;
        private static readonly int HdrBoostId = Shader.PropertyToID("_HdrBoost");
        private Shader _hdrUiShader;
        private bool _hdrShaderMissing;
        // T63: the band is TWO graphics — the money figure and the field behind it — and they need
        // SEPARATE material instances even though they move as one. Sharing a single instance
        // between a Text and an Image does not survive uGUI: the canvas batches by material, the
        // font atlas ends up bound for both, and the Image samples a transparent region of it and
        // renders NOTHING. That was measured, not guessed — the field vanished from all 8 frames of
        // a capture that had shown it solid before the change (see the same-token note in ApplyBoost).
        // `Payout` already drove two materials off one focus; this follows that precedent exactly.
        private Material _cashOutHdrMat, _cashOutFieldHdrMat, _bigAmountHdrMat, _scoreHdrMat, _ballHdrMat;

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
        // T147-am (batch 133): 40 -> 60. The two money facts take a ROW EACH (T144/T74-am6 — each
        // half fits alone and the PAIR does not), and two 24px lines at this face's measured 1.25
        // advance ratio are 60.0px. Everything else derives: TicketRowHeight is computed from this.
        private const float TicketFooterHeight = 60f;  // STAKE/RISK over RETURNED/PAYS, one row each
        // T147-am: 6 -> 4, TO MATCH THE ENGINE. The line that stood here read "RunConfig.MaxLegs
        // defaults to 6" — `RunConfig.cs:49` reads `MaxLegs = 4`, and `Run.cs:190-191` ENFORCES it
        // ("Tickets take 1 to {Config.MaxLegs} legs"), so slots 5 and 6 were not merely unlikely to
        // fill, they were UNFILLABLE. Two dark slots reserved 138.6px no ticket the engine can build
        // would ever occupy, and the footer's growth was priced against a column that had already
        // given that space away. This is what paid for the row above with T24's margin intact.
        //
        // BuildCanvas runs from Awake, before GrayboxRoomBuilder assigns `director` (AddComponent
        // fires Awake synchronously, before the caller's next line runs) — the row-slot count cannot
        // be read from the live run and must be a fixed constant per rule 1. RULE 2 SURVIVES THE
        // CHANGE: unused slots go dark, a ticket with fewer legs never reflows the grid, and one with
        // more truncates rather than resize anything — a five-leg ticket truncates at four exactly as
        // a seven-leg one truncated at six.
        private const int TicketRowSlots = 4;
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
        /// <summary><c>--tv-size-team</c>, 28px. Currently read by nothing, and RESTORED anyway after
        /// T-4 retired it on exactly that evidence.
        ///
        /// <para>Retiring it was wrong. It is not a leftover — it is canon
        /// (<c>tokens/typography.css</c>) with no slot yet able to consume it, because team names and
        /// the score share one component today and that component is sized at
        /// <see cref="TypeScore"/>. T72 (batch 32) ruled the split — name / score / name spans — and
        /// the name spans are what this is for.</para>
        ///
        /// <para>The split itself is deliberately NOT in this phase: it appears in no T-order, and
        /// C43 keeps a migration to one variable. So this sits unread until T72 is built, which is a
        /// different thing from dead. "Nothing references it" was true and was the wrong test.</para></summary>
        private const int TypeTeam = 28;
        private const int TypeClock = 28;
        private const int TypeNeed = 28;
        private const int TypeRisk = 24;
        private const int TypeEvent = 22;
        private const int TypeProgress = 19;
        private const int TypeEyebrow = 15;

        // T-4 retired `TypeTeam` and `TypeLeg` together, on the evidence that each had exactly one
        // reference — its own declaration. `TypeTeam` is back above, because that test was the wrong
        // one: it is canon awaiting T72's split, not a leftover. `TypeLeg = 19` stays retired on
        // different grounds — T20 moved resolved and pending rows 19 -> 15px, so the 19 it named is
        // superseded rather than pending. Two constants that looked identical to a reference count
        // and were not, which is the lesson worth keeping from the mistake.
        //
        // The eight below are the sizes this file used to spell as bare integers at the call site.
        // Same numbers, named — T-4 tokenises, it does not re-scale, so nothing here moves a pixel.
        // These are the slots T75 ruled regular; they carry no canon ratio of their own (§4.1 names
        // ten roles and the surface has 23), so each name states its slot and nothing more.
        private const int TypeAttract = 46;
        private const int TypeTakeoverTitle = 30;
        private const int TypeTakeoverSub = 18;
        private const int TypeSubtitle = 22;
        private const int TypeIntervention = 22;
        private const int TypeConsolation = 28;
        private const int TypeChrome = 14;
        /// <summary>The dormant payoff figure's size, kept with the others so the block is the whole
        /// list rather than the part that renders. `_tBigAmount` draws nothing (T68-am/T71 moved both
        /// payoff figures into the cash-out slot) and T79 holds the question of what that element is
        /// for — out of Phase T by C43. Named here, not woken.</summary>
        private const int TypeBigAmount = 96;

        /// <summary>This surface's tracking scale, in **em** — the unit the design system states it
        /// in. <see cref="MakeText"/> converts to TMP's hundredths-of-an-em internally, so no call
        /// site has to know TMP's unit. The laptop's LaptopTrack is the same shape for the same
        /// reason.
        ///
        /// <para><b>Reachable for the first time in Phase T.</b> UI.Text could not address tracking
        /// at all, so every one of these was rendered at 0 until now. That is why they arrive as a
        /// group rather than one at a time: they were not omitted by choice, they were unbuildable.</para>
        ///
        /// <para>Values are canon's, from <c>tokens/typography.css</c> and the TV kit — NOT the
        /// owning doc, which has no tracking clause. Named so the register can be pointed at a
        /// symbol, and so a slot and any future measurement of it cannot be handed different
        /// numbers.</para></summary>
        private static class TvTrack
        {
            /// <summary><c>--tv-track-name</c>. The authored facts: the compact statement, NEED, the
            /// progress line, the cash-out figure, the event strip.
            ///
            /// <para><b>WITHDRAWN to 0 by T85 (batch 39).</b> It was .02em, taken from the kit, and
            /// the owning doc has no tracking clause to authorise it — so it was applied on the
            /// kit's authority alone. The pair then caught two defects it contributes to: the NEED
            /// line truncates and the money control collides, and at 18 characters .02em is roughly
            /// 10px of the NEED overrun.</para>
            ///
            /// <para>Zeroed rather than deleted, and the five call sites still name it, so the kit's
            /// per-slot assignment stays legible and re-enabling is one number if a tracking clause
            /// is ever ruled. The ruled order is explicit: re-measure at 0 FIRST, and only overruns
            /// that survive that go to T74. Nothing is widened or shrunk ahead of the measurement.</para>
            ///
            /// <para>Label (.16em) and Meta (.10em) are UNTOUCHED. They rest on the same authority —
            /// kit, not owning doc — and T85 named only .02em. Flagged, not acted on.</para></summary>
            public const float Name = 0f;
            /// <summary><c>--tv-track-label</c>. The words ABOUT a fact rather than the fact: the
            /// cash-out status word, the momentum label, the ticket header.</summary>
            public const float Label = 0.16f;
            /// <summary>TvLegRow's `meta` and `stateChip` literal — the row's own chrome, the market
            /// eyebrow, the price and the state chip. Stated as a literal in the kit rather than a
            /// token; carried here as one so the three slots that share it cannot drift apart.</summary>
            public const float Meta = 0.10f;
        }

        /// <summary>The per-face scale factor Phase T reserves for preserving RENDERED size across
        /// the renderer swap, and the one place it would be applied.
        ///
        /// <para>It is 1.0 because the two renderers denote the same thing by <c>fontSize</c>: UGUI
        /// rasterises the face at that em size in canvas units, and TMP scales glyph metrics stored
        /// at the asset's sampling size by <c>fontSize / faceInfo.pointSize</c>, which lands on the
        /// same em. So the sizes above carry across untouched and this constant changes nothing
        /// today.</para>
        ///
        /// <para>It exists named and at 1.0 rather than absent because "the sizes are the same
        /// number" and "the type renders at the same size" are different claims, and only the
        /// before/after pair can settle the second. If the pair shows drift, the correction belongs
        /// here — one place, one diff — and not spread across the 22 call sites that would each look
        /// like a design change.</para></summary>
        private const float TypeScale = 1f;

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
        // WIRED, and this note is the correction of its predecessor. The comment that stood here
        // said "_fontCond is loaded but MakeText still assigns _font to every slot, so the whole
        // surface renders regular". That stopped being true at `c53d7ca` (2026-08-02), one day
        // after it was written: MakeText resolves the face and six call sites pass Face.Condensed.
        // Its own last line warned that an earlier version had claimed call sites that did not
        // exist; it then spent nine days denying call sites that did. Corrected in the commit that
        // makes it false a second time, which is the only moment anyone reliably notices.
        private TMP_FontAsset _font;       // --font-tv        : Encode Sans, Regular 400 / wdth 100
        private TMP_FontAsset _fontCond;   // --font-tv-cond   : Encode Sans, Condensed Regular 400

        /// <summary>Which canon face a text slot is set in. Named rather than a bool so a call site
        /// reads as the component reference does, and so adding a third face later is not a
        /// boolean-blindness bug waiting to happen.</summary>
        private enum Face { Regular, Condensed }
        // Which legs are PRESENTED as resolved (never engine truth). Was an int high-water mark,
        // `_resolvedThrough`, which encoded "ticket order == resolution order" — retired by T140
        // arm A's fixture restructure. A fixture's legs need not be CONTIGUOUS: a ticket
        // [matchA, matchB, matchA] has fixture 0 = legs {0, 2} and is told FIRST, so no scalar can
        // express "0 and 2 have resolved and 1 has not been told at all". A high-water mark of 3
        // there marks leg 1 resolved, and the footer's revealedLoss test would then read leg 1's
        // raw `GradesWon` and announce a death before its scene plays — the T144 leak, exactly.
        // A leg is marked ONLY at its own reveal moment; marking one early re-opens it.
        private bool[] _presentedResolved = new bool[0];
        // T68-am / T71 CONSEQUENCE, flagged rather than acted on: `_tBigAmount` no longer renders
        // anything. Both payoff figures moved into the cash-out slot, so it is now built, cleared on
        // reset, and never given content. It is left in place because its name is in the DD-gated
        // `SanctionedL4Elements` list, whose own gate says to route a change before editing it —
        // and because if the accepted treatment is revisited on frames this is the element it would
        // come back to. **Named here in the same commit that orphaned it**, which is the difference
        // between a flagged consequence and the kind of corpse `_wonFlood` became.
        private TMP_Text _tMatchup, _tLeg, _tClock, _tFlavor, _tCashOut, _tChrome, _tAttract, _tBigAmount, _tConsolation;
        private TMP_Text _tTicketHeader, _tRiskPays, _tInterventionPrompt, _tTakeoverTitle, _tTakeoverSub, _tSubtitle;

        /// <summary>PRD §8.8's match stats panel — the ONE new mid-sweat verb §3 authorises.
        /// <see cref="SeatedDeltaTime"/> reads <c>_statsOpen</c>, which is what stops time.</summary>
        private bool _statsOpen;
        private RectTransform _statsPanel;
        private TMP_Text _tStatsTitle;
        private TMP_Text[] _tStatsLabel, _tStatsA, _tStatsB;

        /// <summary>Three rows ship (Allen, 2026-08-15). Formation and player stats are NOT stubbed —
        /// the engine has neither, and a placeholder would promise a row that is not coming.
        /// <c>StatsRowSlots</c> is the PHYSICAL slot count built once by <see cref="BuildStatsPanel"/>;
        /// how many of them are ever ACTIVE for a given ticket is a separate, per-ticket question —
        /// see <see cref="StatsActiveRowCount"/>.</summary>
        private const int StatsRowSlots = 3;

        /// <summary>DD batch 95: the ONE spacing value the stats panel spends on every edge (left
        /// inset, both inter-column gaps, right inset, top inset, bottom inset — see
        /// <see cref="BuildStatsPanel"/>), hoisted to a class constant because <see
        /// cref="StatsRowY"/>/<see cref="StatsPanelHeight"/> must share the EXACT number
        /// <c>BuildStatsPanel</c> builds the rows with, never a second, driftable copy of "32".</summary>
        private const float StatsPad = 32f;

        /// <summary>DD batch 95: the Y position (top-left anchored, so negative and growing downward)
        /// of row slot <paramref name="slotIndex"/> — title at -StatsPad, row 0 at -(StatsPad+56),
        /// pitch 46 per slot after that. The ONE formula both <see cref="BuildStatsPanel"/> (building
        /// every physical slot) and <see cref="ResizeStatsPanel"/> (re-homing a slot onto an EARLIER
        /// index when it is active, or COLLAPSING it onto the last active slot when it is not) must
        /// share — two independent copies of this arithmetic is exactly how a slot ends up "hidden in
        /// place" instead of contiguous.</summary>
        private static float StatsRowY(int slotIndex) => -(StatsPad + 56f + slotIndex * 46f);

        /// <summary>DD batch 95: THE PANEL'S HEIGHT FOLLOWS ITS ROWS. A function of <paramref
        /// name="rowCount"/> rather than a fixed constant, because the row SET — and so the row COUNT
        /// — is only known once <see cref="ComputeStatsRowSet"/> runs, at ticket adoption; it can no
        /// longer be fixed at canvas-build time, when no ticket exists yet. Height = the last row's
        /// own bottom edge (-<see cref="StatsRowY"/>(rowCount-1), plus its 34px height) + StatsPad —
        /// the same "bottom inset == top inset" relationship <c>Stats_panel_is_sized_exactly_to_its_
        /// content</c> pins, now genuinely load-bearing vertically rather than incidentally true of one
        /// fixed 3-row number.</summary>
        private static float StatsPanelHeight(int rowCount) => -StatsRowY(rowCount - 1) + 34f + StatsPad;

        /// <summary>THE UNREVEALED MARK. DD batch 93: since the panel's rows now KEY TO THE TICKET
        /// (<see cref="ComputeStatsRowSet"/>) — a CORNERS/CARDS row exists at all only if the ticket
        /// bought that leg — this mark's meaning NARROWED. It no longer means "not in your ticket";
        /// that case is now an ABSENT row — DD batch 95: not merely unprinted text in an existing
        /// slot, but NO SLOT AT ALL ("an unbought row is not a silent row, it is NO row") — never a
        /// marked one. It means NOT YET REVEALED: a row the ticket DID buy, whose stat has not been
        /// causally revealed, is shown as this, NEVER as its true final value — "a leak here is a
        /// blocker, not a polish item". The glyph itself is unchanged, only the set of rows it can
        /// ever appear on (Allen, 2026-08-15 / DD batch 93; batch 95 sharpened "absent").</summary>
        private const string StatsUnrevealed = "—";

        /// <summary>T102 (DD batch 89): the stats panel's column-sizing rule, re-ruled from "widest
        /// ink + contentMargin" to "the widest measured ink is at most this fraction of its own
        /// box". <see cref="BuildStatsPanel"/> derives labelW/valueW from this ONE constant rather
        /// than restating them as independent literals, so a future ruling on the fraction moves one
        /// number, not a copied derivation.
        ///
        /// <para>S84's binding: the value column's widest ink is the engine's closed club pool, not
        /// a sampled string, and a guard test re-measures the FULL pool against this fraction on
        /// every routine run (TvSweatScreenTests.cs,
        /// <c>Stats_panel_value_column_holds_the_full_club_pool_at_max_ink_fraction</c> — unlike the
        /// C46 evidence sweep it borrows its measuring instrument from, it is deliberately NOT
        /// [Explicit], because that is exactly what lets a grown pool overflow the box
        /// silently).</para></summary>
        private const float MaxInkFraction = 0.8f;

        /// <summary>RATIFIED by Allen (T101, batch 85). Raised as this seat's own pick and explicitly
        /// unratified, the way T88 raised `ENTER` — *"correctly raised and correctly not assumed"*.
        ///
        /// <para>The reason it needed a word rather than a default: <b>`ENTER` is the studio commit
        /// key by standing ruling, and a panel toggle is a different act</b>, so it takes its own.
        /// `TAB` is bound to nothing on this surface or in the room's asset and is the genre's own
        /// scoreboard key. §8.8 requires only that it neither swallows nor is swallowed by the
        /// cash-out or stand controls (TVS-H01's shape); a distinct key gives that by construction,
        /// and the pin asserts it rather than trusting it.</para></summary>
        private const string StatsKeyWord = "TAB";
        /// <summary>T74-am5: the footer's right-anchored half. `_tRiskPays` keeps its name and carries
        /// RISK; this carries PAYS. Two elements, one row, no authored gap between them.</summary>
        private TMP_Text _tPays;
        // TV-03/TV-04: the cash-out slot is three things, not one — an actionable FIELD, the money
        // figure, and a status word at label scale beside it.
        private Image _cashOutField;
        private TMP_Text _tCashOutStatus;
        // C3: the score's momentary L4 punch overlay — see BuildScoreBug and OnGoalPlayed.
        private TMP_Text _tScoreFlash;
        // T40 ENFORCED (batch 27): `_wonFlood` and `_goldFlood` are GONE — deleted, not z-ordered and
        // not dimmed (C10). Both were `MakeStretchImage(root, …)`: full-SCREEN washes created after
        // every zone, so they painted over the ticket column, the scorebug, the stage, the event
        // strip, the chrome and the cash-out slot alike.
        //
        // T40 ruled them deleted back in batch 5 — `_wonFlood` is that ruling's first named subject —
        // and the frame proved the case better than the ruling did: at flood peak EVERY fact on the
        // surface is gold, so for 0.6s the money signal means nothing, on the exact beat three
        // batches of ladder work existed to protect. It is T65's defect one layer up: the room was
        // stopped from flooding gold and routed through one settlement point; the screen still did it.
        //
        // `_dimOverlay` is the same construction and is deliberately NOT struck — a dim is not a
        // wash and T40 does not reach it. Named so this removal cannot quietly take a third element.
        private Image _backing, _dimOverlay;
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
            public TMP_Text Line;      // compact: the authored statement
            public TMP_Text Price;     // compact: the price, --tv-context, never the state hue
            public TMP_Text State;     // compact: the right-aligned state chip
            public TMP_Text Need;      // live: the authored §6 statement, printed verbatim
            public TMP_Text Progress;  // live: the revealed causal progress line
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

        /// <summary>DD batch 93 item 1: the stats panel's ROW SET for the CURRENT ticket — CORNERS
        /// present only if the ticket carries a TotalCorners leg, CARDS only if it carries a
        /// TotalCards leg. GOALS needs no flag; it is unconditional. Computed exactly ONCE, by
        /// <see cref="ComputeStatsRowSet"/> at the same site <c>_ticket</c> itself is adopted
        /// (<c>PresentRound</c>), and never recomputed per leg or per beat — <see cref="RenderStatsPanel"/>
        /// only ever reads these two flags, never <c>Ticket.Legs</c> directly.</summary>
        private bool _statsRowHasCorners;
        private bool _statsRowHasCards;

        /// <summary>DD batch 95: how many of the panel's <see cref="StatsRowSlots"/> are ACTIVE for
        /// the CURRENT ticket — GOALS unconditionally, plus one more per count kind the row set
        /// carries. THE ONE SOURCE both <see cref="ResizeStatsPanel"/> (which physical slots to
        /// activate and how tall the panel is) and <see cref="DebugStatsRow"/> (which indices are
        /// PRESENT vs ABSENT to a test) read, so the two can never silently disagree about where the
        /// row set "ends". "An unbought row is not a silent row, it is NO row" — this is that count.</summary>
        private int StatsActiveRowCount => 1 + (_statsRowHasCorners ? 1 : 0) + (_statsRowHasCards ? 1 : 0);

        /// <summary>DD batch 93 item 2: revealed per-team counts, RETAINED for the life of the
        /// ticket — independent of <see cref="_countLedger"/>, which is REPLACED per leg (see
        /// <see cref="BeginStageLeg"/>) and holds exactly one count kind at a time. Without this, a
        /// count revealed while its leg was live would UN-REVEAL itself the instant a later leg went
        /// live — a fact un-revealing itself, strictly worse than the mark it would replace.
        ///
        /// <para>Keyed by the count MarketKind (TotalCorners/TotalCards only). Seeded the instant a
        /// count leg goes live (<see cref="BeginStageLeg"/>) and advanced on every completed count
        /// (<see cref="OnCountPlayed"/>), so it always agrees with a live <c>_countLedger</c>'s own
        /// Home/Away exactly, and keeps the last value once that ledger is replaced. Cleared ONLY in
        /// <see cref="ResetForNewSession"/> (a new ticket/session) — never on a leg change.</para></summary>
        private readonly Dictionary<MarketKind, (int Home, int Away)> _statsRetainedCounts =
            new Dictionary<MarketKind, (int Home, int Away)>();

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
            // T64: `_emissSeed` (the idle flicker's phase) went with the flicker. It was this file's
            // last UnityEngine.Random use and the reason the idle emission differed run to run —
            // §6.4's owed-to-integration note is discharged, and the frame-locked A/B has one fewer
            // presentation-local source to pin.
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
                // T114-am: bare here too. This is the HELD PREVIEW of the accepted state, so it
                // must show what acceptance will actually show — a preview that promises an amount
                // the accepted banner then drops would misstate the very act it is previewing, and
                // §6.1's own law is that a control's copy IS its input contract.
                _tCashOut.text = "CASHED OUT";
            }
            // The instruction moves with the state, at the moment the state moves. The status word is
            // otherwise written only when a price is rendered, and a preview is entered between those
            // moments — so without this the slot would sit under a held preview still reading HOLD E.
            // DERIVED rather than snapshotted, unlike the figure above: it is a pure function of state
            // the exit restores anyway, and recomputing from truth is what makes the revert total.
            if (_tCashOutStatus != null) _tCashOutStatus.text = CashOutStatusWord();
            UpdateTicketColumn(_liveLegsShown); // repaint at the SAME live set: entering a preview decides no row
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
            if (_tCashOutStatus != null) _tCashOutStatus.text = CashOutStatusWord();
            UpdateTicketColumn(_liveLegsShown); // same live set: releasing a preview decides no row either
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
        ///
        /// <para>TVS-H01 said this must agree EXACTLY with TryCashOut's acceptance gate, and the half
        /// that mattered is intact: both still read <see cref="CanAcceptCashOutNow"/>, so a suspended
        /// or mid-tween offer can no more reserve E than it can be accepted. The second term only ever
        /// EXTENDS the reservation, and only across a hold and the two frames after it — it can never
        /// make an unacceptable offer acceptable, because it is not on the accept path at all.</para>
        ///
        /// <para>It exists because T88's gesture gives E a DURATION. A press-to-commit input needed no
        /// second term; a hold does, and the term is the preview itself.</para>
        ///
        /// <para><b>The hazard is on the PRESS path, and the first design here guarded the wrong one.</b>
        /// The room lane's source answers it (merged `c8525d1`): <see cref="SitSpot"/> acts on
        /// <c>WasPressedThisFrame()</c> — press, never release — and <c>PlayerInteractor</c>'s
        /// press-poll deliberately bypasses the action's own Hold interaction. So the release that
        /// abandons a preview cannot stand anybody up, and the two-frame post-release reservation
        /// written here for that hypothetical guarded nothing while <b>introducing</b> a defect: a
        /// player who released E and immediately pressed it again to stand would have had that stand
        /// silently swallowed. It is gone.</para>
        ///
        /// <para>What the real hazard is: a FRESH press arriving while the preview is held —
        /// <c>Interact</c> carries more bindings than the E key — which on the press path would stand
        /// the player mid-sweat out from under his own held preview. <c>_cashOutPreview</c> covers
        /// exactly that window and not one frame more, so the instant a preview ends, standing behaves
        /// precisely as it did before this gesture existed.</para></summary>
        private bool CashOutLive() => CanAcceptCashOutNow() || _cashOutPreview;

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
            // T59 (RULED, DD 2026-08-04): the slot's state IS the input's state, read from the same
            // value. T43 left these deliberately separate — the presentation was fixed but E still
            // accepted during §8.7's pending window, where _marketSuspended is false because
            // ResolveBeat never suspends — and routed the question up rather than guessing, because
            // moving an input contract is not this seat's call. The ruling: "a player who presses E
            // during suspension receives a cash-out they were just told was unavailable, at a price
            // the display is not showing. On a money control, accepting an input you have declared
            // refused is the worst available outcome — worse than refusing an input you appeared to
            // offer, because the player cannot even see what they got."
            //
            // So the presentation flag now gates the accept. suspended and pending refuse E;
            // actionable accepts; updating refuses (the _cashOutAnimation term below), because the
            // offer is not yet acceptable and L3 already says so. TVS-H01 is preserved by
            // construction: CashOutLive and TryCashOut both read THIS predicate, so they cannot drift.
            && !_cashOutSlotSuspended
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
                // DD batch 93 item 1: derived HERE, at the same instant the ticket itself is
                // adopted — once per ticket, never per leg or per beat.
                ComputeStatsRowSet();

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

                // T140 arm A: the FIXTURE is the unit — see OnFinalFixture for why the old
                // `evt.LegIndex == lastLeg` cannot fire on an interleaved ticket.
                bool onFinalLeg = OnFinalFixture(evt, _session);
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
                        _pendingFlavor = SweatFlavor.GoalLine(spec.Goal.Value.ForPicked, leg, evt.Step,
                            SweatFlavor.AnchorForTelling(_ticket, evt));
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

                // §3.5 — THE DECISIVE BEATS DRAW FROM A DISJOINT POOL, so recycling onto them is
                // UNCONSTRUCTIBLE rather than unlikely (T108 clause 1's standard, arriving on copy
                // instead of on a form).
                //
                // The measured defect this closes: of seven count events, the APPROACH printed
                // corner #1's line — the least consequential event of the match — verbatim, and
                // THE CROSSING, the moment the bet was won, printed corner #2's. The two decisive
                // events were narrated with recycled openers from the two that mattered least.
                //
                // Gated on `spec.Decisive`, which is NULL wherever the classifier never ran. That
                // distinction is the whole guard: a count scene also reaches here from an UNGATED
                // beat (cards, an Under leg, a Score-typed beat, a whole-number line), and those
                // are ordinary events that must keep the ordinary deck. Reading `countScene` alone
                // cannot tell the two apart — which is exactly the blocker the build dispatch hit
                // and correctly refused to work around by re-deriving the gate here.
                //
                // Valence is read off the TICKET (`spec.ForPicked`, set from leg.Selection.Choice),
                // never off the event — ScoreLedgerTests already pins that mood follows the bet.
                bool decisiveBeat = countScene
                    && (spec.Decisive == CountSignificance.Approach
                        || spec.Decisive == CountSignificance.Turn);
                if (decisiveBeat)
                    _pendingFlavor = SweatFlavor.DecisiveLine(spec.Decisive.Value, spec.ForPicked);

                // T110-am2 (batch 108): THE COUNT-BATCH SUFFIX IS REMOVED. It read
                // `" ({n} in the spell)"` and it is gone outright — not narrowed, not shortened.
                //
                // FIVE REASONS, AND WIDTH IS DELIBERATELY THE LEAST OF THEM. The value was
                // TRUTHFUL — `spec.Count.Value.TotalDelta`, checked at source — and it goes anyway:
                //   1. `spell` is never explained to the player anywhere on this surface;
                //   2. it misreads as a running TOTAL when it is a per-event DELTA;
                //   3. the fact is already SHOWN — the count moves in the column in front of him,
                //      which is §3.1's *drawn, not captioned*;
                //   4. the widest string printed `spell` TWICE, one clause apart — T69/T70's
                //      defect inside a single string;
                //   5. and only fifth, the 94.8px overrun.
                //
                // Recorded in that order on purpose: a string is not cut for its width when four
                // better reasons were already standing. The overrun's discharge is a CONSEQUENCE
                // here rather than the purpose — the lead measured the decks WITHOUT the suffix at
                // 577.2px against a 651.0px box, fits with 73.8px spare, so removing it closes
                // T110-am as a side effect.
                //
                // The decisive-beat lines never took the suffix in the first place (§3.5), so
                // nothing about them changes.

                // spec-count-theater-2026-08-17.md §4, THE BINDING: StageBeat() already advanced
                // the count ledger's cursor unconditionally the instant _choreo.ResolveBeat ran
                // above — if the resolver declined to stage that batch as a scene (SceneSpec.
                // QuietCount rather than Count), the batch must still be committed here, or the
                // column falls short of the match's own total (spec §7 item 2's gate). Uses the
                // exact same authority a narrated count uses (CommitRevealedCount), so commit and
                // repaint land on the same frame here too — never a second, hand-kept-in-step copy.
                // NO flavour text, NO audio: only the drama is discretionary, the count is a fact.
                // Do not "fix" this silence by adding a line — it is deliberate and ruled.
                //
                // LIVE AS OF PHASE B (T115). This note read "NO-OP TODAY: no path populates
                // QuietCount yet" while phase A stood alone; the distance gate now populates it, and
                // the docked after-set shows the result — four of seven corners committing their
                // count with no scene of their own (dd-import/corners-sweat-after-2026-08-18).
                if (spec.QuietCount.HasValue)
                    CommitRevealedCount(spec.QuietCount.Value);

                // spec-count-theater-2026-08-17.md §2, THE A-REVEAL (T109-cl, ruled FINAL): the
                // mirror image of the QuietCount commit just above, for the OTHER ledger. A count
                // leg's goal (SceneSpec.QuietGoal) never gets a goal scene of its own — STEP 2
                // keeps this beat's scene exactly what the count/momentum grammar chose — so
                // OnGoalPlayed's payoff callback, the only other caller of CompleteGoal, is never
                // reached for it. Committed here instead, through the same CommitRevealedGoal
                // authority every caller of it shares, on the identical frame as the scene that
                // (deliberately) does not narrate it.
                if (spec.QuietGoal.HasValue)
                    CommitRevealedGoal(spec.QuietGoal.Value);

                // A zero batch fell through to ordinary play: the pre-computed corner/booking
                // line would narrate an event the pitch never shows (Sol, F_0.4.0 P3 r2).
                // But a staged goal owns the beat's story wherever it lands (M-T4.1) — the
                // goal call wins over neutral possession (Sol, F_0.4.0 P3 r3).
                if (countLeg && !countScene)
                    _pendingFlavor = spec.Goal.HasValue
                        ? SweatFlavor.GoalLine(spec.Goal.Value.ForPicked, leg, evt.Step,
                            SweatFlavor.AnchorForTelling(_ticket, evt))
                        : SweatFlavor.NeutralLine(evt, leg, _lastBeatUp,
                            SweatFlavor.AnchorForTelling(_ticket, evt));

                // T97 (batch 68) — THE SECOND INSTANCE OF ONE LAW, and the guard above is the first:
                //
                //   A beat's WORDS are licensed by what the RESOLVED SCENE CONTAINS, never by the
                //   beat's TYPE LABEL alone.
                //
                // The count families got this guard at F_0.4.0 P3 r2 — "corner/booking words would
                // be a lie there". The GOAL families never did, so a beat typed Score or BigPlay
                // printed a goal sentence whether or not the scene staged a goal. On a goalless
                // match that shipped `{other} on the board; the slip flinches.` over a 0–0 FT
                // scorebug: the strip asserting a goal the match never contained.
                //
                // NearMiss is excluded because its overrides are already right — they assert no goal
                // and are used precisely where none occurred, which is the model this copies.
                bool goalWords = (evt.Type == DramaEventType.Score || evt.Type == DramaEventType.BigPlay)
                    && evt.Tag != TensionTag.NearMiss;
                // `HasValue` was the WRONG QUANTITY and the trace proved it: `T97 guard goal=True`
                // fired on every Score beat of a match that finished 0–0. A scene STAGES a goal and
                // then resolves it — `Commits` false is the chalk-off that prints `VAR — NO GOAL` —
                // so `HasValue` is the beat's INTENT while `Commits` is what the scene CONTAINS.
                // The law says the words are licensed by what the RESOLVED SCENE contains, and this
                // is the difference between reading the law and implementing it.
                //
                // spec-count-theater-2026-08-17.md §2 addendum: extended to QuietGoal, not just
                // Goal, or this guard would reintroduce T97's own bug in the opposite direction. A
                // count leg's committing quiet goal (STEP 2) can coincide with a Score/BigPlay-typed
                // beat that also carries a nonzero count batch — CornerFor/CornerAgainst/Booking
                // plays, spec.Goal stays null by design, but the scoreboard truly does move. Without
                // this half, goalScene would read false and NoGoalLine would overwrite the corner's
                // own text with a claim the frame's own scorebug contradicts — a state lie of
                // exactly the shape T62/T97 exist to prevent, just newly reachable now that §2
                // lets the goal commit at all. A CHALKED quiet goal still falls to NoGoalLine below,
                // correctly — no goal actually happened, so "no goal" is not a lie there.
                // REVERTED TO THE CONSERVATIVE FORM BY THE LEAD, and the reasoning is worth keeping
                // because the alternative was well argued and is one edit away.
                //
                // The build dispatch extended this to `|| (spec.QuietGoal.HasValue &&
                // spec.QuietGoal.Value.Commits)` — letting goal words STAND when a quiet goal
                // commits — on the ground that NoGoalLine would otherwise overwrite the line while
                // the scorebug advances underneath, a T62/T97-shaped state lie newly reachable
                // because §2 lets the goal commit at all.
                //
                // IT GOES THE WRONG WAY AGAINST §2. That clause carves out the SCORE and says
                // everything else — the panel's rows, player detail, and THE FLAVOUR STRIP'S
                // SUBJECT — continues to follow the ticket. Letting goal words stand is the strip
                // following the MATCH, which is the one thing §2 did not carve out.
                //
                // And the lie is not established: `NoGoalLine` SELECTS a line that does not ASSERT
                // a goal (it skips members flagged assertsGoal). Silence about a goal is not a
                // contradiction of one. A strip that stays on corners while the scorebug shows the
                // score is exactly "the score is always true, the rest follows the ticket."
                //
                // ROUTED to the DD with the dispatch's reasoning attached: if a count-leg goal
                // should also reach the STRIP, this line is the one edit, and it is the same
                // question as whether it should earn a SCENE (see TheaterChoreographer's own
                // ROUTED note). Both are the same call and should be ruled together, not
                // separately.
                bool goalScene = spec.Goal.HasValue && spec.Goal.Value.Commits;
                if (goalWords && !goalScene)
                    _pendingFlavor = SweatFlavor.NoGoalLine(evt, leg, _lastBeatUp,
                        SweatFlavor.AnchorForTelling(_ticket, evt));
                if (goalWords) TraceFlavor($"T97 guard commits={goalScene}", _pendingFlavor);

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
            // T164: the number is the TICKET's, so a WON leg no longer snaps it to 1 — one leg
            // winning does not make the ticket certain, and 1f was announcing a certainty the
            // ticket does not have mid-ticket. The terminal values are unchanged where they were
            // already right: the engine lands exactly 1.0 when every leg is won and exactly 0.0 on
            // a revealed dead leg with no save held (pinned by engine.tests/TicketWinProbabilityTests).
            // The GUARD SHAPE is preserved — VOID still falls through untouched, keeping its
            // pre-kill number, exactly as the doc above says.
            if (grade == LegGrade.Won || grade == LegGrade.Lost)
            {
                _probTarget = (float)_session.TicketWinProbability;
                // The bed lands on the same seam and keeps its OWN pre-T164 values: this match is
                // over, so tension goes to its floor exactly as it did when the mirror carried the
                // leg's number. VOID falls through here too, holding the bed as it holds the bar.
                _tensionProb = grade == LegGrade.Won ? 1f : 0f;
            }
            RevealedView.SetProbability(_probTarget);
            // T140 arm A — EVERY leg that resolves at this whistle resolves HERE, AT ITS OWN GRADE.
            // A telling is a (ticket, FIXTURE), so N legs can end on one whistle, and `grade` is the
            // ANCHOR's alone. The legs riding one fixture can be heading to OPPOSITE outcomes —
            // `DramaEvent.LegProbs` says so in terms ("per-leg by necessity") — so pushing the
            // anchor's grade onto all N would print a WON chip on a leg that lost. That is a silent
            // lie on a mixed fixture, and it is the exact failure §6a exists to prevent: one fact,
            // one source. So each leg's grade is derived from ITS OWN leg state, by the same
            // IsVoided/GradesWon idiom the resolved-row branch and ResolveBeat already use.
            //
            // The ANCHOR keeps the passed-in `grade` rather than re-deriving it, so the single-leg
            // telling every ticket without a same-match pair produces is byte-identical to before.
            IReadOnlyList<int> resolving = evt.LegIndices;
            for (int n = 0; n < resolving.Count; n++)
            {
                int resolvedLeg = resolving[n];
                LegGrade legGrade = resolvedLeg == evt.LegIndex ? grade : GradeOf(_ticket.Legs[resolvedLeg]);
                RevealedView.ResolveLeg(resolvedLeg, legGrade);
                _tape?.ResolveLeg(resolvedLeg, legGrade); // T16: collapses the strip to its resolution cap
            }

            // T87-am2 — THE DRAWN MATCH'S LINE, WRITTEN HERE AND HELD, and the trace is why it moved.
            //
            // The batch-68 build set it on the LegFinal beat's `flavor` and let RenderEvent stash it
            // to `_pendingFlavor`. Instrumenting every strip write proved that stash IS NEVER LANDED:
            // `RevealBeatChrome` — the only thing that lands it — lives inside TheaterBeat's
            // `evt.Type != LegFinal` branch, so on the whistle the trace reads
            //
            //     RenderEvent stash LegFinal  <- 'THE MATCH ENDS LEVEL'
            //     grade WON                   <- 'LEG 1 — WON'
            //
            // with no LAND between them. The line was correct, reachable and never displayed. The DD
            // hypothesised the grade won a race; the fact is there was no race — nothing ever wrote
            // the line to the strip at all.
            //
            // So it is written DIRECTLY here, ahead of the grade beats below, and HELD: the grade may
            // not land inside the hold, and a statement replaced on its own entrance frame was never
            // made. `_ledger` is the REVEALED score, never the locked StatLine.
            if (_ledger.Picked == _ledger.Opponent)
            {
                SetEventStrip(flavorColor);
                _tFlavor.text = "THE MATCH ENDS LEVEL";
                TraceFlavor("T87-am2 drawn ending", _tFlavor.text);
                _flavorScale = 1.12f;
                yield return ScaledWait(drawnEndingHoldDuration);
            }

            // The whole live set is marked together: they end at ONE whistle, so a column that
            // marked only the anchor would leave the fixture's other leg reading LIVE on a match
            // that is over. Marked only NOW — after the final scene has played — which is what
            // keeps the reveal gate above the raw engine truth.
            MarkPresentedResolved(evt.LegIndices);
            // The NEXT FIXTURE's legs go live, not `evt.LegIndex + 1`. On a ticket where every
            // fixture holds one leg those are the same set, so this is a no-op there — which is the
            // point; the pre-emptive "next leg reads LIVE once its events start" behaviour is
            // PRESERVED, not replaced with "nothing is live between fixtures". `+ 1` only ever
            // computed the next fixture by coincidence, and on [A,B,A] it names leg 1 — a leg on a
            // fixture that was already told — while the fixture actually next is none at all.
            UpdateTicketColumn(LegsOfFixtureAfter(evt.LegIndex));
            // The human-facing number stays the ANCHOR's. What a shared telling's copy should call
            // itself ("LEG 1", "LEGS 1 & 3", something else) is a DESIGN question and it is NOT
            // ruled; inventing a form here would be this lane deciding it. Left as-is, and named.
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
                RepaintRevealedScore(_ticket.Legs[_stageLeg]);

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
                SetEventStrip(flavorColor);
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
                // FLAGGED, tier applied but hue untouched: TV-05 as quoted in ResolveBeat below says
                // the strip "never uses money hues ... money semantics live on the leg rows and the
                // cash-out slot". This branch puts gold on the strip, which contradicts that. Batch
                // 14 ruled the TIER, not the hue, so the gold stays and the question is filed.
                SetEventStrip(pickedScorer ? new Color(gold.r, gold.g, gold.b, 1f) : flavorColor);
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

        /// <summary>THE ONE COMMIT AUTHORITY for a count batch (spec-count-theater-2026-08-17.md §4:
        /// "no beat may consume a count batch without committing it"). Advances the revealed ledger,
        /// writes the stats panel's retained mirror, and repaints every surface that mirrors the
        /// revealed count — the same three steps <see cref="OnCountPlayed"/> always ran, factored
        /// out here so a beat that stages NO scene for a batch (a quiet corner,
        /// <see cref="SceneSpec.QuietCount"/>) can commit it through the IDENTICAL path a narrated
        /// one uses, rather than a second copy that must be kept in step by hand. This file's own
        /// standing reasoning binds that exactly as it binds <see cref="RepaintRevealedScore"/>
        /// itself (see that method's T62 note and the stats panel as its "third mirror"): one
        /// authority, or the mirrors drift. Two commit paths drifting apart is precisely the defect
        /// that reasoning exists to prevent — so there is only ever this one.
        ///
        /// <para>Commit and repaint land TOGETHER, never split across two call sites — this method
        /// calls <see cref="RepaintRevealedScore"/> itself rather than leaving it to each caller, so
        /// §6.2/T62's law ("a progress line lands on the same frame as its revealed payload") holds
        /// by construction for both the narrated and the quiet path.</para>
        ///
        /// <para><b>Audio is deliberately NOT here</b> — it stays with <see cref="OnCountPlayed"/>
        /// alone, the caller for a beat that actually staged a corner/booking scene. A quiet corner
        /// commits the fact and gets no riser and no whistle: "the count is a fact; only the drama
        /// is discretionary" (spec §4).</para>
        ///
        /// <para>Guarded on <c>_countLedger == null</c> exactly as the pre-refactor
        /// <c>OnCountPlayed</c> was, so a call with no live count ledger is a no-op here exactly as
        /// it always was — <see cref="OnCountPlayed"/> keeps its own copy of this same guard so a
        /// null ledger still short-circuits before its audio, unchanged from today's
        /// behaviour.</para></summary>
        private void CommitRevealedCount(CountLedger.StagedCount count)
        {
            if (_countLedger == null) return;
            _countLedger.CompleteCount(count);
            // DD batch 93 item 2: every completed count writes through to the RETAINED store, keyed
            // off the leg the panel is currently mirroring (guaranteed to be this same count kind,
            // since _countLedger only exists while the live leg is one) — never off TargetTotal or
            // any other locked-endpoint field (§8.8's leak).
            if (_statsLeg != null)
            {
                MarketKind statsLegKind = _statsLeg.Selection.Kind;
                if (statsLegKind == MarketKind.TotalCorners || statsLegKind == MarketKind.TotalCards)
                    _statsRetainedCounts[statsLegKind] = (_countLedger.Home, _countLedger.Away);
            }
            if (_countLedger.TargetTotal > 0)
            {
                if (_ticket != null && _stageLeg >= 0 && _stageLeg < _ticket.Legs.Count)
                    RepaintRevealedScore(_ticket.Legs[_stageLeg]);
            }
        }

        /// <summary>spec-count-theater-2026-08-17.md §2, THE A-REVEAL (T109-cl, ruled FINAL): THE
        /// ONE COMMIT AUTHORITY for a <see cref="SceneSpec.QuietGoal"/> — mirrors
        /// <see cref="CommitRevealedCount"/>'s shape exactly, for the score ledger instead of the
        /// count ledger, and for the identical reason: a beat whose own scene is NOT a goal scene
        /// (STEP 2 — the scene stays ticket-keyed on a count leg) can never reach
        /// <see cref="OnGoalPlayed"/>, which only ever fires from a goal scene's own payoff. "The
        /// revealed scoreline is never withheld... whether or not the ticket rides on them" means
        /// this beat's goal cannot be left to wait for a payoff that will never arrive — it commits
        /// HERE, on the beat that staged it, exactly as <see cref="CommitRevealedCount"/> commits a
        /// declined count batch on ITS staging beat.
        ///
        /// <para>Calls the SAME ledger mutator (<see cref="ScoreLedger.CompleteGoal"/>) and the
        /// SAME repaint authority (<see cref="RepaintRevealedScore"/>) <see cref="OnGoalPlayed"/>
        /// itself uses, so the scorebug, the ticket column's live row, and the stats panel mirror
        /// all move on the identical frame a narrated goal would move them on — T62's rule again,
        /// this time for a goal with no scene to carry it. A chalked-off quiet goal (Commits false)
        /// completes without moving anything, exactly like a narrated one.</para>
        ///
        /// <para><b>Deliberately NOT here:</b> audio, flavour text, the score/ball L4 punch, and the
        /// scorer reveal — all of it lives in <see cref="OnGoalPlayed"/> alone, which this method
        /// never calls and is never called by. STEP 2's own words: "committed, scoreline
        /// repainted, scene unchanged" — a quiet goal gets no riser and no punch, mirroring exactly
        /// what <see cref="CommitRevealedCount"/> withholds for a quiet count ("only the drama is
        /// discretionary", spec §4, applied here to the other ledger). <c>_ledger</c> is a plain
        /// <c>readonly</c> field, never null (unlike <c>_countLedger</c>, which is per-leg and
        /// nullable), so this needs no guard <see cref="CommitRevealedCount"/>'s equivalent
        /// carries.</para></summary>
        private void CommitRevealedGoal(ScoreLedger.StagedGoal goal)
        {
            _ledger.CompleteGoal(goal);
            if (_ticket != null && _stageLeg >= 0 && _stageLeg < _ticket.Legs.Count)
                RepaintRevealedScore(_ticket.Legs[_stageLeg]);
        }

        /// <summary>A corner kick or booking reaches its payoff. Count and market direction
        /// move together here; the stage callback fires before the chrome reveal callback. The
        /// commit itself (ledger advance, stats mirror, repaint) is <see cref="CommitRevealedCount"/>
        /// — this method adds only the audio, which is narration and belongs to a SCENE, never to a
        /// silent commit (see that method's own doc).</summary>
        private void OnCountPlayed(CountLedger.StagedCount count)
        {
            if (_countLedger == null) return;
            CommitRevealedCount(count);
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
            // THE CLOCK READS THE FREEZE AUTHORITY, not a second predicate that means the same thing
            // today. It used `Time.deltaTime`, and the `!_seated` guard above is what actually froze
            // it on stand-up — two expressions of one rule, agreeing by convention. So when §8.8's
            // panel added a THIRD freeze condition, the clock did not get it: the capture shot the
            // panel over a frozen scoreline with the MINUTE TICKING 18' -> 21' behind it, which is
            // precisely the "covered fact that CAN move is lost" case T99's licence does not reach.
            //
            // Found on frames, not by reading: the pin asserted SeatedDeltaTime, and SeatedDeltaTime
            // was correct — a channel that never read it is invisible to a pin on it. T95's rule,
            // earned again: when a ruling adds a condition, every mirror of it moves too, and the
            // mirrors are found by grepping for the quantity rather than by remembering.
            _clockShownMin = Mathf.Min(_clockTargetMin, _clockShownMin + _clockRate * SeatedDeltaTime);
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
        /// [the cash-out] row." The cash-out slot stays SUSPENDED (structureGrey, L1) for
        /// the duration; the M/R/N verbs render on the separate InterventionPrompt element.</summary>
        private IEnumerator PendingWindowBeat()
        {
            if (Keyboard.current == null)
            {
                _session.DeclinePendingLoss();
                yield break;
            }

            // S85 / T143-am2: WHERE NO SINGLE CALL SAVES THE TICKET, THE OFFER IS NOT PRESENTED —
            // AND THEREFORE IT IS NOT OFFERED TO THE KEYBOARD EITHER. Two or more legs died at this
            // one whistle; both saves act on ONE leg, so whichever is spent the ticket still dies.
            // S85's ruling is that a refusal knowable before the act is SHOWN before it, and a
            // window that renders no offer while M and R still commit would be precisely the
            // "spend a Whistle to discover the ticket was already dead twice over" it forbids. The
            // state is read ONCE and gated HERE, at the one place both the composition and the input
            // read it, so the rows and the keys cannot disagree about what is on offer.
            //
            // The saves stay LEGAL in the engine (DD batch 169) — this is the surface declining to
            // PRESENT them, which is a presentation ruling, not a lock on the consumable.
            bool noSingleCallSaves = _session.NoSingleCallSaves;
            bool canM = !noSingleCallSaves
                && director.Run.OwnsConsumable("mulligan_slip") && _session.CanMulliganPendingLoss;
            bool canR = !noSingleCallSaves && director.Run.OwnsConsumable("refs_whistle");
            // T86(a) (batch 44): the bracketed-key form is RETIRED on this surface. `[M]`/`[R]`/`[N]`
            // go the way of `[E]`, and T22's reasoning was never local to the cash-out slot — "not a
            // label, it is a debug token, and it is on a shipped surface in every frame". Its
            // replacement is the established one: where another product would draw a glyph, this one
            // prints the word, so `[E]` became `HOLD E` and these become `HOLD M` / `HOLD R` /
            // `HOLD N`.
            //
            // The exact wording beyond the retired form is the DD's to ratify; what is applied here
            // is the ruled FORM. The extent consequence routes through the sweep, per the ruling.
            // BATCH 46/47: the probability GOES. `PendingLossProbBefore` is documented in the engine
            // as "the leg's displayed win-prob", so the parenthetical was a win-probability numeral
            // on a slot — the theatre prints facts and offers, never opinions, and an offer states
            // its COST rather than its odds. The whistle costs one whistle, so that is what prints,
            // under its authored catalogue name (`RelicCatalog`: "Ref's Whistle") rather than a short
            // form nobody wrote — G1's class of defect.
            //
            // MULLIGAN was left alone deliberately last pass — "the ruling names SEND TO REVIEW, and
            // authoring copy for a slot nobody ruled is how this surface acquired strings it could
            // not render" — and T88(b) has now answered it generally: the preview shows what the
            // option does AND WHAT IT COSTS, on every spending option. So the cost prints under
            // `RelicCatalog`'s authored name ("Mulligan Slip"), the same source and the same
            // parenthetical form the whistle's already uses. No abbreviation is coined: a short form
            // nobody authored is G1's defect class, which is what that rule exists to prevent.
            //
            // THE COMPOSITION IS A LIST — DD batch 50 answering the 380px structurally, from S24's
            // shape: "N offers are a list; putting them on one line is a row pretending to be a
            // sentence." One option per row is also the only composition with room for a per-option
            // cost and a hold affordance. MEASURED on this tree by `SBR/TV/T88 prompt composition`:
            // every option row fits the 635.0px zone, the widest at 523.8px with 111.2px spare, so
            // line-to-list retires the whole 380.0px overrun on WIDTH.
            //
            // HEIGHT is the open item, and it is the DD's under the ruling's own condition, reported
            // with the zone's dimensions as that condition requires: the zone is 635.0 x 90.0 and
            // carries exactly THREE rows at 22px (27.5px each, first row 27.5). Title + three options
            // is 110.0px — over by 20.0px, and only in the worst case where the run owns both
            // consumables. §6's grid does not resize to content, so which row yields is not a call
            // this seat may absorb.
            // BATCH 56, and the correction is to this seat's own fix. `HOLD N LET IT DIE` carried
            // forward under "unchanged" — which was true — and never met T88(c), the ruling that
            // changed its class. So the prompt printed HOLD over an input that is a press: **C48's
            // own defect, surviving inside the pass that fixed C48.** A string that never meets the
            // ruling that changed its class is the same defect one level up.
            //
            // The two spending rows keep their HOLD and the difference now does real work — it tells
            // him which options cost him something.
            const string optM = "HOLD M MULLIGAN (ONE MULLIGAN SLIP)";
            const string optR = "HOLD R SEND TO REVIEW (ONE REF'S WHISTLE)";
            const string optN = "N LET IT DIE";
            // `SHOT FROZEN` LEAVES THE ZONE (batch 56) and the zone does not grow. The stage already
            // says it: the shot is frozen on screen, and a title announcing what the frame is already
            // displaying is the restatement class S37/S58/T69/T70 have each ruled. Growing a locked
            // dimension to restate something is R30's shape.
            //
            // It also removes the worst case rather than affording it: three option rows measure 82.5
            // in a 90.0 zone and fit in EVERY ownership combination, not only when one consumable is
            // held — C46 forbids leaning on the common case.

            // ---------------------------------------------------------------- T143: NAME THE DEAD
            // THE WINDOW NAMED NOTHING UNTIL THIS PASS. Three fixed rows, and `PendingDeadLegIndices`
            // — the surface the session added FOR this ruling — read by nobody. T143 requires the
            // window to name the legs that died at this whistle, and a ruling nothing reads has not
            // landed. Copy: `spec-pending-window-copy-2026-08-25` (DD batches 186/189/192/193).
            //
            // TICKET ORDER, straight off the session, because that is the order the ticket column
            // already prints these legs in — a second ordering here is how two columns come to
            // disagree about which leg is which. Bounds-checked rather than trusted: a live sweat
            // may not take the surface down over a name (SheetName's stated leniency), and the
            // degenerate empty case falls back to the shipped string below.
            var deadNames = new List<string>();
            foreach (int i in _session.PendingDeadLegIndices)
                if (_ticket != null && i >= 0 && i < _ticket.Legs.Count)
                    deadNames.Add(PendingLegName(_ticket.Legs[i]));

            string offers;
            if (noSingleCallSaves)
            {
                // ═══ N ≥ 2: THE COMPOSITION IS NOT AUTHORED YET, AND IT IS NOT GUESSED HERE. ═══
                //
                // ⚠ TODO (DD batch 193, `T143-am6`) — THIS BRANCH IS OWED ITS COPY. It is reachable,
                // it is explicit, and it deliberately renders NEITHER the §2 one-leg form nor a
                // part-built §3: a silent fallthrough to `N LET {CLUB} {LINE} DIE` would name ONE
                // leg while several died, which is the exact failure T143 exists to prevent.
                //
                // WHAT IS ALREADY RULED AND IS BUILT ABOVE: T143-am2 removes the save offer here, so
                // the two spending rows do not render and M/R do not commit (`canM`/`canR` gated on
                // `noSingleCallSaves`). That half is settled and stands whichever way the number
                // below falls.
                //
                // WHAT WAITS, AND ON WHAT: batch 189 §3 authored a two-name form and §4 recorded a
                // hole at three — FOUR rows in a three-row zone. Batch 193 shows BOTH premises
                // stale: `RunConfig.MaxLegs` ships at 4, so N is bounded at FOUR enumerable cases
                // rather than arbitrary; and the 870.4px width that made ≥3 look impossible measured
                // the RETIRED `… IS DEAD`-class placeholder, not the bare names the composition
                // uses. If two bare names share a row the ruled shape is N=2 → one name row + the
                // shared refusal row (2 rows); N=3 → 2 names + 1 name + shared (3 rows); N=4 → 2 + 2
                // + shared (3 rows) — every case inside the zone, and T143's "names every leg" never
                // bounded. That is contingent on ONE MEASUREMENT the DD is taking: whether two bare
                // names plus the separator fit 635.0. Batch 193 pre-commits both readings — (A) they
                // fit, so the shape enumerated just above is the one to build and there is no hole;
                // (B) they do not, so the ceiling stays at two names and N=3/N=4 take the bounded
                // form ruled in that batch. EITHER WAY IT IS AN AUTHORED ANSWER, and guessing which
                // one before the number lands is how a surface acquires copy nobody ruled.
                //
                // THE SEPARATOR IS ALREADY ON THIS SURFACE when the composition lands: `   ·   `
                // (three spaces, U+00B7, three spaces), the form `PreviewOf` below already prints.
                // Reuse it verbatim rather than re-typing a different spacing — and note FitToColumn
                // drops a dangling " ·" on truncation, so a shared row degrades to whole words
                // instead of a stranded glyph.
                //
                // UNTIL THEN this renders the SHIPPED, already-ratified decline row and nothing else.
                // It is not new copy, it is not §3 part-built, and it does not assert a death (§1:
                // the legs are PENDING, not dead). It is the one string that is true in this state
                // under every pre-committed reading: the offer is not presented, and declining is
                // what the player may do. `deadNames` is read and correct here — what is missing is
                // the authored form that spends it, not the data.
                offers = optN;
            }
            else
            {
                // ---- §2, THE ONE-LEG FORM. THE DECLINE ROW ABSORBS THE NAME rather than sharing a
                // row with it: `LET … DIE` supplies the verb and the tense, the name supplies its
                // referent, and NOTHING IS RESTATED. This is option 3 at its tightest — batch 186
                // ruled the copy shares the decline row; here it *is* the decline row, with no
                // separator and no second element, strictly shorter than the 528.4px shared form.
                //
                // §1 is why no form here asserts the death: TV's placeholder read
                // `DULUTH AUDITORS +1.5 IS DEAD` and THE LEG IS NOT DEAD — the window exists
                // precisely because the loss is PENDING and the player may still spend to prevent
                // it. `IS DEAD` beside `LET IT DIE` contradicts itself on one row.
                //
                // T88(c) SURVIVES INTACT: still a press, still N-prefixed, still no HOLD. Batch 56's
                // correction was about HOLD-versus-press, not about content, so absorbing the name
                // changes what the row NAMES and nothing about its class. S24 survives too: one
                // option, one row. AND THE TWO SPENDING ROWS ARE UNCHANGED — three rows total.
                //
                // WIDTH IS NOT AT ISSUE HERE, and a later seat must not treat this row as
                // width-critical. Measured on this tree at ed2a6c2 against the 635.0 x 90.0 zone:
                // `N LET SPREADSHEETS +1.5 DIE` is 334.8px with 300.2 spare (the longest club
                // short-form across four slates), and even the UN-shortened
                // `N LET SAN FRANCISCO MEATBALLS +1.5 DIE` is 477.3px and still fits. So `Short` is
                // applied for CONSISTENCY with §2 and T168-am, NOT to make the row fit. The widest
                // row is still optR at 523.8, the decline row never becomes the widest, and the
                // three-row height is still 82.5 in the 90.0 zone — the shipped worst case, unmoved.
                //
                // The shipped `N LET IT DIE` survives as the DEGENERATE fallback only: a window with
                // no nameable leg has nothing to absorb, and `N LET  DIE` would be worse than the
                // string it replaces.
                offers = (canM ? optM + "\n" : "")
                    + (canR ? optR + "\n" : "")
                    + (deadNames.Count > 0 ? $"N LET {deadNames[0]} DIE" : optN);
            }
            // The HELD composition: the option being previewed, and how to finish or abandon it.
            // UNRATIFIED copy in T22/T86(a)'s established form — print the word, not the glyph —
            // routed with the rest of this batch's strings rather than presented as canon.
            string PreviewOf(string option)
                => option + "\n" + ConfirmKeyWord + " CONFIRMS   ·   RELEASE ABANDONS";

            // T43: §8.5 Pending window: "As suspended" — L1 unlit slate. This site used to hand-set
            // the word and its colour and nothing else, which is why the slate never reached the
            // field, the status word or the L4 token here. One call, one slate, both sites.
            ShowMarketSuspended();
            _tInterventionPrompt.enabled = true;
            _tInterventionPrompt.color = new Color(gold.r, gold.g, gold.b, 1f);
            _tInterventionPrompt.text = offers;
            // The window can OPEN while the panel is already up, not only the other way round, so the
            // z-order is re-asserted from BOTH directions. Raising only on panel-open would put the
            // prompt behind it on exactly the path where the player is being asked to decide — and
            // "the pending decision is never out of sight" is the ruling, not the happy path.
            RaiseStatsChrome();

            // Which spending option is being previewed: "M", "R", or null. NEVER "N" — T88(c) rules
            // the decline out of the gesture entirely.
            string held = null;

            while (_session.HasPendingLoss)
            {
                // The keyboard is re-read EVERY FRAME, not once at entry.
                //
                // The guard at the top of this method runs a single time, and every frame of this loop
                // then dereferenced `Keyboard.current`. A keyboard that goes away mid-window — a
                // wireless device sleeping, a controller swap, or a test removing a virtual one —
                // threw a NullReferenceException INSIDE the coroutine, which kills the beat and leaves
                // the pending window hanging on an irreversible money decision. Pre-existing: the
                // press-era code dereferenced it the same way. Found because a gesture test added a
                // device and removed it.
                //
                // Treated exactly as the entry guard treats it: no keyboard, no way to decide, so the
                // window declines rather than hangs. That is the documented behaviour that stops batch
                // autoplay hanging, applied to the case where the device leaves after we started.
                Keyboard keys = Keyboard.current;
                if (keys == null)
                {
                    _session.DeclinePendingLoss();
                    HideCashOutSlot(); // T43
                    _tInterventionPrompt.enabled = false;
                    yield break;
                }

                // The hold is a STATE, sampled every frame. Nothing in this loop measures how long a
                // key has been down, and that absence IS "no timer, no auto-commit": there is no
                // elapsed quantity for an auto-commit to compare against.
                bool downM = _seated && canM && keys.mKey.isPressed;
                bool downR = _seated && canR && keys.rKey.isPressed;

                // One preview at a time, and whichever was held first keeps it until it is released.
                // Rolling a finger onto the other key mid-hold must not move the commit to a different
                // spend while the player is still reading the first one's cost.
                string nowHeld = held == "M" && downM ? "M"
                               : held == "R" && downR ? "R"
                               : downM ? "M"
                               : downR ? "R"
                               : null;

                if (nowHeld != held)
                {
                    held = nowHeld;
                    // RELEASE ABANDONS, always (T22). The offers come back and nothing has been
                    // spent — there is no state to unwind, because a preview never touched the run.
                    _tInterventionPrompt.text = held == null ? offers : PreviewOf(held == "M" ? optM : optR);
                }

                // A PRESS COMMITS NOTHING. The commit is the second key, during the hold.
                if (held == "M" && ConfirmPressed())
                {
                    director.Run.PlayMulliganSlip(_session);
                    HideCashOutSlot(); // T43: the field and status leave with the figure, same frame
                    _tInterventionPrompt.enabled = false;
                    SetEventStrip(chromeCyan); // §8 VOID — the mulligan voids the leg, not chrome
                    _tFlavor.text = "THE SLIP COMES OUT — LEG VOIDED, THE TICKET LIVES";
                    _emissRest = _emissIdle; // the DEAD dim lifts: the ticket breathes again
                    tvLight?.ResetToIdle();
                    // Re-asserts the CURRENTLY live legs, read off the session that owns them. The
                    // `Mathf.Min(_resolvedThrough, …)` clamp it replaces existed only to keep a
                    // scalar in range; `CurrentFixtureLegs` is the honest referent and needs none.
                    UpdateTicketColumn(SessionLiveLegs());
                    yield return ScaledWait(deadLineDuration);
                    yield break;
                }
                if (held == "R" && ConfirmPressed())
                {
                    director.Run.PlayRefsWhistle(_session);
                    HideCashOutSlot(); // T43
                    _tInterventionPrompt.enabled = false;
                    if (!_session.IsComplete)
                    {
                        // The leg is reinstated live — a fact, not a payout yet. §4 Fact: cold white.
                        SetEventStrip(flavorColor);
                        _tFlavor.text = "REVIEWED — OVERTURNED. THE LEG STANDS.";
                        _emissRest = _emissIdle;
                        tvLight?.ResetToIdle();
                        UpdateTicketColumn(SessionLiveLegs()); // same: the reinstated legs are the session's live set
                    }
                    else
                    {
                        // A loss confirmed — context, not a hue. §4/§8: loss is darkness, never red;
                        // the text itself still has to stay legible, so it reads in grey, not black.
                        SetEventStrip(contextGrey);
                        _tFlavor.text = "REVIEWED — THE CALL IS CONFIRMED.";
                    }
                    yield return ScaledWait(deadLineDuration);
                    yield break;
                }
                // T88(c): the decline is NOT a spend and does not take the gesture. It costs nothing
                // and is already what happens if the player does nothing, so one press is
                // proportionate — "the weight of the gesture matches the weight of the act", which is
                // also what stops the three reading as peers when two spend and one does not. A press
                // here is the ruled input, not a leftover of the one this pass removed.
                if (_seated && keys.nKey.wasPressedThisFrame)
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
            // T164: the anchor is the TICKET's sold probability, taken ONCE. The model no
            // longer re-anchors per telling, so a zero seed would make the first beat's delta
            // the whole probability.
            _presModel.ResetForTicket(_session != null ? _session.TicketWinProbability : 0.0);
            CleanupConfetti();
            StopCashOutAnimation();
            _hasCashOutShown = false;
            _cashOutShown = 0.0;
            _cashOutScale = 1f;
            _cashOutFlash = 0f;
            _stageLeg = -1;
            _stageBeatCount = 0;
            _countLedger = null;
            // DD batch 93 item 2: the RETAINED store is ticket/session-scoped — cleared HERE, on a
            // new ticket/session, and nowhere else. A leg change must never touch it (that is the
            // exact trap item 2 exists to close).
            _statsRetainedCounts.Clear();
            _finalSequenceActive = false;
            _audioUrgency = 0f;
            _stoppageGoalCount = 0;
            _marketSuspended = false;
            _scorerRevealedForActiveLeg = false;
            // T164: the bed's pregame seed is the value the mirror used to carry here — leg 0's own
            // number, NOT the ticket's — so the crowd opens exactly as it opened before this change.
            _tensionProb = _pendingTensionProb =
                _ticket != null && _ticket.Legs.Count > 0 ? (float)_ticket.Legs[0].TrueProb : 0f;
            // T164: the mirror's pregame number is the SESSION's ticket probability. _session is
            // already adopted in PresentRound before this runs, but this is a session boundary, so
            // the read is guarded rather than assumed.
            RevealedView.Reset(director != null ? director.Run : null, _ticket,
                director != null ? director.SweatIndex : 0,
                _session != null ? _session.TicketWinProbability : 0.0);
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

            SetAlpha(_dimOverlay, 0f);
            _tBigAmount.text = string.Empty;
            if (_tConsolation != null) _tConsolation.enabled = false;
            HideCashOutSlot(); // T43
            _tCashOut.rectTransform.localScale = Vector3.one;
            _tAttract.enabled = true;
            SetEventStrip(flavorColor);
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
            // T165 / T165-am (batch 178): the counter counts TELLINGS, and says so. Under T140 arm A
            // the unit of broadcast is the (ticket, FIXTURE) — two legs can ride one match — so a
            // counter reading LEG n/m would print a leg total the column contradicts: four rows
            // against `LEG 2/3`. The word is ruled MATCH on vocabulary the surface already owns
            // (`THE MATCH ENDS LEVEL`, and the scoreline slot is `Matchup`); all five candidates
            // cleared T91-cl's 2px ink floor, so width did not decide it.
            _tLeg.text = $"MATCH 1/{FixtureTotal()}";
            _tClock.text = "PRE";
            // T44 casing: CF puts state words in tracked uppercase, and every sibling state line on
            // this element is (VAR — NO GOAL, THE TOTEM BURNS, LEG n — WON). Lowercase sentence case
            // is for the beat corpus's running text, which this is not.
            _tFlavor.text = "THE BOARD IS SET";
            // T164: data only — RevealedView, no visible bar — and the number is the TICKET's, not
            // leg 0's. Guarded: this is a session boundary (see ResetForNewSession's caller note).
            _probTarget = (float)(_session != null ? _session.TicketWinProbability : 0.0);
            // The ticket-card takeover copy clears the instant the live sweat begins.
            _tTakeoverTitle.text = string.Empty;
            _tTakeoverSub.text = string.Empty;
            ResetPresentedResolved();
            // The FIRST FIXTURE's legs, which is leg 0's fixture — the ticket's first telling by
            // construction, since the fixtures are in first-appearance order over the legs. On an
            // ordinary ticket that is {0}, identical to the `0` this replaces; on [A,B,A] it is
            // {0, 2}, and both rows correctly open live because both matches kick off together.
            UpdateTicketColumn(LegsOfFixtureContaining(0));
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
                // DD batch 93 item 2: seed the RETAINED store the instant this kind goes live, so it
                // agrees with a freshly-configured ledger's own 0/0 immediately — matching the
                // pre-existing "shows 0, never the mark, from kickoff" behaviour — rather than
                // waiting for this leg's first OnCountPlayed callback to write anything at all.
                _statsRetainedCounts[leg.Selection.Kind] = (_countLedger.Home, _countLedger.Away);
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
        /// <summary>T62 (DD 2026-08-05, found on this slice's own T58 proof frames): **every surface
        /// that mirrors <c>_ledger</c> repaints together, on the frame the ledger moves.**
        ///
        /// <para>The defect: <c>_ledger.CompleteGoal</c> advances the revealed score, and
        /// <c>OnGoalPlayed</c> then repainted the SCOREBUG only. The live leg row's progress line
        /// reads the same <c>_ledger.Picked/Opponent</c> (see DescribeActiveLeg) but was only
        /// repainted by <c>UpdateTicketColumn</c> at the next beat's RenderEvent — so for a whole beat
        /// the column printed the pre-goal score while the scoreline above it printed the goal.
        /// **Same revealed value, same frame, two readings.** The DD measured it correcting a full
        /// 51 match-minutes later, which is the shape of a state lie rather than a lag.</para>
        ///
        /// <para>Fixed at the LEDGER-ADVANCE site rather than by adding one more call to one more
        /// path: this method is what <c>OnGoalPlayed</c> calls, so a future path that moves the score
        /// and repaints only half of it has to go out of its way to do so. Same rule as T43's slate
        /// and T59's gate — one value, one repaint, no window where two mirrors disagree.</para>
        ///
        /// <para>The column is refreshed at its CURRENT live index, not re-pointed: this states the
        /// score, it does not decide which row is live. Nothing here is revealed early — the ledger
        /// has already advanced and the scorebug is already showing it, so this only stops the column
        /// lagging behind a fact the surface has published.</para>
        ///
        /// <para><b>Amended: this method is not score-ledger-only.</b> It serves any revealed-LEDGER
        /// advance — score OR count — not just the score ledger the paragraphs above were written
        /// against. <c>OnGoalPlayed</c> has called it since T62; <c>OnCountPlayed</c> did not, calling
        /// <c>UpdateScorebug</c> directly instead, so a count advance repainted the scorebug but left
        /// the ticket column's progress line stale until the next beat's <c>RenderEvent</c> — T62's
        /// own defect, reproduced on the count ledger instead of the score ledger. The count arm was
        /// missing until this fix.</para></summary>
        private void RepaintRevealedScore(Leg leg)
        {
            UpdateScorebug(leg);
            UpdateTicketColumn(_liveLegsShown); // the CURRENT live set, unchanged: this states the score, it does not re-point the column
            // T62'S RULE, AND THE STATS PANEL IS ITS THIRD MIRROR. One ledger advance, every mirror
            // of it repainted in the same call — T62 existed because the column and the scorebug read
            // the same revealed value on two different repaint schedules and disagreed for a whole
            // beat. A panel that can be open across a goal is exactly that defect's next host.
            RenderStatsPanel();
        }

        private void UpdateScorebug(Leg leg)
        {
            if (_tMatchup == null) return;
            // The panel mirrors whatever leg the SCOREBUG is showing, so the two can never name
            // different fixtures. Captured here rather than in a second place for the same reason
            // T62 gave: one authority, or the mirrors drift.
            _statsLeg = leg;
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
                    // T96: a draw ticket has no backed side, so it takes the deck's own DRAW row
                    // rather than a team's. Routed here rather than inside the describer because
                    // this is the site that knows the selection.
                    if (leg.Selection.Choice == MarketChoice.Draw)
                        return SweatActiveLegModel.Describe(
                            SweatActiveLegModel.ActiveLegInput.MoneylineDraw(_ledger.Picked, _ledger.Opponent));
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
                case MarketKind.CorrectScore:
                    // T151/T161: WITHOUT THIS CASE THE ARM BELOW IS UNREACHABLE and the row falls to
                    // `default`, which returns an EMPTY copy — that is literally the blank column the
                    // drawn-ending spec's §4 records as "nothing — the column is blank". The describer
                    // arm alone does not fix it; the SITE that knows the selection has to route here.
                    //
                    // `_ledger.Picked`/`Opponent` are home/away for this kind:
                    // SweatFlavor.PickedHomeForPresentation returns true unconditionally for every
                    // kind that is not Moneyline or AnytimeScorer (T152-am), so picked IS home.
                    return SweatActiveLegModel.Describe(SweatActiveLegModel.ActiveLegInput.CorrectScore(
                        leg.Selection.ScoreHome, leg.Selection.ScoreAway,
                        _ledger.Picked, _ledger.Opponent));
                case MarketKind.AnytimeScorer:
                {
                    Player player = leg.Matchup.PlayerAt(leg.Selection.PlayerIndex);
                    return SweatActiveLegModel.Describe(SweatActiveLegModel.ActiveLegInput.AnytimeScorer(
                        player.Name, _scorerRevealedForActiveLeg));
                }
                default:
                    // ⚠ THIS ARM RETURNED AN ALL-EMPTY COPY, AND THAT IS T130's DEFECT AT SOURCE.
                    //
                    // A LIVE row blanks its compact line by design, so NEED and progress are the
                    // only spans it has. An empty copy therefore renders a leg of the player's
                    // ticket as a completely blank row — which is exactly what item 1.3 fixed for
                    // CorrectScore, and its own record says the caller was half the fix: "the arm
                    // AND the caller wiring — the arm alone would not have fixed it, the caller's
                    // default: returned an empty copy, which IS the blank column." 1.3 added the
                    // CorrectScore arm and LEFT THE DEFAULT, so every other kind kept the defect.
                    //
                    // SEVEN OFFERED KINDS REACH HERE: Handicap (4 selections on the board),
                    // TeamTotalGoals/Corners/Cards and TotalGoalsOddEven (2 each), WinningMargin
                    // and PlayerMultiScorer (1 each). Found by the anchor capture window, which
                    // forced an away-backed Handicap that no test had ever rendered live.
                    //
                    // THE FALLBACK AUTHORS NO COPY, DELIBERATELY. NEED takes the row's own identity
                    // string — the same LegStatement the compact line prints for this leg on every
                    // other row state — so the row states WHICH BET IT IS rather than nothing. That
                    // is a compromise: NEED asks "what does my money still need" and this answers
                    // "which bet is this". Authoring real NEED copy for these kinds is a DESIGN
                    // question and is routed, not invented here. What is fixed is the SILENCE.
                    return new SweatActiveLegModel.ActiveLegCopy(
                        LegStatement(leg), string.Empty, isTeamMarket: false, identity: "MARKET PICK");
            }
        }

        /// <summary>This leg's printed name, read off <see cref="MarketSheet"/> — the composer the
        /// laptop and the console both print through. Null when the selection is not on its matchup's
        /// sheet, which the caller treats as a last resort rather than a crash: the console THROWS
        /// there (a gate can afford to), but a live sweat may not take the surface down over a name.
        /// The exhaustive blank-row gate is what makes that leniency safe — a kind missing from the
        /// sheet fails there instead of rendering quietly.</summary>
        private static string SheetName(Leg leg)
        {
            if (leg == null || leg.Matchup == null) return null;
            foreach (MarketSheetRow row in MarketSheet.Build(leg.Matchup).AllRows)
                if (row.Offer.Selection.Equals(leg.Selection))
                    return row.Name.ToUpperInvariant();
            return null;
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

        // ---------------------------------------------------------------- the live set / the resolved set
        //
        // T140 arm A's two collections, and the reason neither is a scalar. A telling is a
        // (ticket, FIXTURE): every leg riding one fixture is live across the whole telling and
        // grades at its ONE whistle. So "which leg is live" and "how far has the ticket resolved"
        // are both SETS, and the ticket-order cursor that answered them before is not merely
        // imprecise — on a ticket whose fixture legs are non-contiguous it names the wrong legs.

        /// <summary>Sizes the presented-resolved set to the current ticket and clears it. Called at
        /// the pregame boundary, which is the moment the column first renders a ticket.</summary>
        private void ResetPresentedResolved()
        {
            int count = _ticket != null ? _ticket.Legs.Count : 0;
            if (_presentedResolved.Length != count) _presentedResolved = new bool[count];
            else System.Array.Clear(_presentedResolved, 0, count);
        }

        /// <summary>Marks every leg of ONE TELLING presented-resolved. They are marked together
        /// because they end together — a column that marked only the anchor would leave the
        /// fixture's other leg rendering LIVE on a match whose whistle has already blown.</summary>
        private void MarkPresentedResolved(IReadOnlyList<int> legIndices)
        {
            if (legIndices == null) return;
            for (int n = 0; n < legIndices.Count; n++)
            {
                int i = legIndices[n];
                if (i >= 0 && i < _presentedResolved.Length) _presentedResolved[i] = true;
            }
        }

        /// <summary>Whether leg <paramref name="i"/> is PRESENTED as resolved. Bounds-safe by
        /// design, not by accident: several readers walk <c>_legRow</c> (a fixed slot count) rather
        /// than the ticket's legs, so an index past the ticket's end reaches here and must read as
        /// NOT resolved — the same answer the old <c>i &lt; _resolvedThrough</c> gave it.</summary>
        private bool IsPresentedResolved(int i)
            => i >= 0 && i < _presentedResolved.Length && _presentedResolved[i];

        /// <summary>Whether leg <paramref name="i"/> is one of the legs the column is currently
        /// rendering LIVE. Bounds-safe for the same reason as <see cref="IsPresentedResolved"/>: a
        /// row index with no leg behind it is simply not in the set.</summary>
        private bool IsLiveShown(int i) => _liveLegsShown.Contains(i);

        /// <summary>The counter's DENOMINATOR — how many tellings this ticket has (`T165`).
        ///
        /// <para>The session is the authority: <c>FixtureCount</c> is the grouping the joint price
        /// itself uses, so the surface and the price cannot disagree about what a match is. Falls
        /// back to the leg count only when there is no session — the pregame/ticket-card boundary,
        /// where the two coincide anyway on every ticket without a same-match pair, and where a
        /// counter is better slightly generous than absent.</para></summary>
        private int FixtureTotal()
            => _session != null ? _session.FixtureCount
             : _ticket != null ? _ticket.Legs.Count
             : 0;

        /// <summary>The legs LIVE right now per the session — the honest referent for a repaint that
        /// re-asserts the current telling (the mulligan and the whistle) rather than choosing a new
        /// one. Empty with no session, and empty once the sweat is over.</summary>
        private IReadOnlyList<int> SessionLiveLegs()
            => _session != null ? _session.CurrentFixtureLegs : (IReadOnlyList<int>)System.Array.Empty<int>();

        /// <summary>The ticket's legs partitioned by matchup, matchups in FIRST-APPEARANCE order,
        /// each group holding its leg indices in ticket order.
        ///
        /// <para><b>This mirrors <c>SameMatchModel.GroupByMatchup</c>'s rule exactly</b> — walk the
        /// legs in ticket order; a leg joins the existing group whose <c>Matchup</c> it shares by
        /// REFERENCE (<c>ReferenceEquals</c>, as the engine does), otherwise it opens a new group.
        /// It is duplicated here ONLY because that helper is <c>internal</c> to the engine. Its own
        /// note states the constraint this copy is bound by: <c>DramaGenerator</c> "must group
        /// through THIS helper, in THIS order, or the sweat's idea of a fixture and the joint
        /// price's would be two implementations of one rule." If these two ever disagree, that is
        /// precisely what has happened, and the engine contract forbids it — so any change to the
        /// engine's rule is a change to this one, not a difference to be reconciled downstream.</para>
        ///
        /// <para>Grouping is what makes "the next fixture" computable. <c>+ 1</c> cannot: the legs of
        /// one fixture NEED NOT BE CONTIGUOUS, so on [A, B, A] fixture 0 is legs {0, 2} and is told
        /// FIRST — and leg 1, which <c>+ 1</c> would name, is on a fixture that has not been told at
        /// all.</para></summary>
        private List<List<int>> TicketFixtures()
        {
            var groups = new List<List<int>>();
            if (_ticket == null) return groups;
            var matchups = new List<Matchup>();
            for (int i = 0; i < _ticket.Legs.Count; i++)
            {
                Matchup matchup = _ticket.Legs[i].Matchup;
                int group = -1;
                for (int m = 0; m < matchups.Count; m++)
                    if (ReferenceEquals(matchups[m], matchup)) { group = m; break; }
                if (group < 0)
                {
                    matchups.Add(matchup);
                    groups.Add(new List<int>());
                    group = groups.Count - 1;
                }
                groups[group].Add(i);
            }
            return groups;
        }

        /// <summary>The legs of the fixture CONTAINING <paramref name="legIndex"/>. Empty when the
        /// ticket has no such leg. On a ticket with no same-match pair this is always the single
        /// leg itself.</summary>
        private IReadOnlyList<int> LegsOfFixtureContaining(int legIndex)
        {
            List<List<int>> fixtures = TicketFixtures();
            for (int f = 0; f < fixtures.Count; f++)
                if (fixtures[f].Contains(legIndex)) return fixtures[f];
            return System.Array.Empty<int>();
        }

        /// <summary>The legs of the fixture AFTER the one containing <paramref name="legIndex"/>,
        /// empty when that fixture is the ticket's last (or when the leg is not on the ticket).
        ///
        /// <para>This is the correct generalisation of the resolve sites' <c>evt.LegIndex + 1</c>,
        /// which pre-emptively marks the next thing live at the whistle so it reads LIVE the moment
        /// its events start. On a ticket where every fixture holds one leg the two agree exactly —
        /// which is the requirement, not a happy accident: the behaviour is preserved, and only the
        /// referent is corrected from "the next leg in ticket order" to "the next telling".</para></summary>
        private IReadOnlyList<int> LegsOfFixtureAfter(int legIndex)
        {
            List<List<int>> fixtures = TicketFixtures();
            for (int f = 0; f < fixtures.Count; f++)
                if (fixtures[f].Contains(legIndex))
                    return f + 1 < fixtures.Count
                        ? (IReadOnlyList<int>)fixtures[f + 1]
                        : System.Array.Empty<int>();
            return System.Array.Empty<int>();
        }

        /// <summary>One leg's grade, by the same <c>IsVoided</c>/<c>GradesWon</c> idiom every other
        /// site in this file reads it with (see <c>ResolveBeat</c> and the resolved-row branch).
        /// Needed per-leg because a shared telling resolves N legs at one whistle and they can grade
        /// DIFFERENTLY.</summary>
        private static LegGrade GradeOf(Leg leg)
            => leg.IsVoided ? LegGrade.Voided : leg.GradesWon ? LegGrade.Won : LegGrade.Lost;

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
        /// need a rewrite if concurrent live legs are ever authorized.
        ///
        /// <para><b>T140 (2026-08-19): the clause that stood here was stale and load-bearing.</b> It
        /// read "today at most one row is ever live, since the engine forbids two legs on one
        /// matchup." The sgp lane shipped same-game parlays (F_0.6.0 — engine, gates, conditional
        /// cash-out) and <c>JointModel</c> explicitly models "two legs on one match plus a third
        /// elsewhere" with a SameMatch block, so THE CASE IS ALREADY REACHABLE and the stated
        /// justification for "at most one row is ever live" no longer holds. What keeps this method
        /// correct is the per-row tolerance above, not the engine restriction it cited — and the
        /// per-fixture restructure T140 prices is what would first exercise it.</para>
        ///
        /// <para><b>That restructure landed, and this method now takes the live SET.</b> A telling is
        /// a (ticket, FIXTURE) and every leg riding one fixture is live across the whole telling and
        /// grades at its single whistle, so a shared telling has MORE THAN ONE live leg and a scalar
        /// `liveLegIndex` could only ever name one of them. The per-row tolerance the paragraph above
        /// credits is what made this a parameter change rather than a rewrite: each row still decides
        /// its own live state, it just tests membership instead of equality.</para></summary>
        private void UpdateTicketColumn(IReadOnlyList<int> liveLegs)
        {
            // Cached BY VALUE, and the reference check is not defensive noise: three call sites
            // repaint with `_liveLegsShown` ITSELF (both cash-out preview edges and the ledger
            // repaint, all of which re-assert the current set rather than choosing a new one), so
            // clearing before copying would erase the very set being re-asserted.
            if (!ReferenceEquals(liveLegs, _liveLegsShown))
            {
                _liveLegsShown.Clear();
                if (liveLegs != null)
                    for (int n = 0; n < liveLegs.Count; n++) _liveLegsShown.Add(liveLegs[n]);
            }
            if (_ticket == null)
            {
                _tTicketHeader.text = string.Empty;
                for (int i = 0; i < _legRow.Length; i++) ClearLegRow(i);
                _tRiskPays.text = string.Empty;
                if (_tPays != null) _tPays.text = string.Empty;
                return;
            }

            // T121/T114-am's SETTLED STATE, hoisted above the row loop because the ROWS need it
            // too. `Bust()` and the cash-out each set `_ticket.State` AND `_complete` in the same
            // breath (SweatSession.cs:252-253, :503-508), and a complete session emits no further
            // drama events (:136-140) — so every leg after the settle is NEVER revealed. The rows
            // below used to leave those legs reading LIVE and NEXT on a ticket that is over, while
            // the footer said the position was closed. `NEXT` means "the next thing that can take
            // his money" (T25.6) and after the settle nothing can.
            //
            // ⚠ THE DEAD FACT IS REVEAL-GATED, AND THAT IS THE WHOLE POINT OF THIS BLOCK.
            // `SweatSession.MoveNext` resolves a `LegFinal` BEFORE it hands the event back —
            // `if (e.Type == LegFinal) ResolveLegFinal();` then `evt = e; return true;`
            // (SweatSession.cs:150-154) — and `ResolveLegFinal` busts instantly when no save is
            // held (:184-185). So `_ticket.State` reads `Lost` from the moment the event is
            // DELIVERED, while `_presentedResolved` is only marked in `FinalSlam`, after the whole
            // final scene has played. Three repaints land inside that gap — `RenderEvent`
            // (called straight off `MoveNext`), `RepaintRevealedScore` (stoppage-time goals
            // during the final scene) and `ExitCashOutPreview` (polled every frame) — and a
            // footer reading raw `_ticket.State` would print `STAKE` / `RETURNED $0` while the
            // scene that kills him is still playing. **That is the ending, told early.**
            //
            // So the dead fact is taken from what has been REVEALED, using the SAME test the
            // resolved-row branch below renders its `L` chip from. Footer and rows then cannot
            // disagree by construction — which is the invariant the PlayMode pin asserts — and
            // no call site can leak, because the leak is closed at the source of truth rather
            // than at each of the ten repaints.
            //
            // CASHED OUT IS NOT GATED and must not be: it is a PLAYER ACTION with no hidden
            // outcome behind it (T114 says so in terms), it settles synchronously at the moment
            // he acts, and there is nothing for a reveal to be ahead of.
            //
            // Walks ALL legs and tests each one's own mark, rather than a prefix: the resolved legs
            // are not a prefix of the ticket once a fixture's legs are non-contiguous, and a prefix
            // scan over [A,B,A] would read leg 1's raw grade after fixture 0 resolved — the leak
            // this whole gate exists to close, reintroduced by the loop bound.
            bool revealedLoss = false;
            for (int i = 0; i < _ticket.Legs.Count; i++)
            {
                if (!IsPresentedResolved(i)) continue;
                Leg revealedLeg = _ticket.Legs[i];
                if (!revealedLeg.IsVoided && !revealedLeg.GradesWon) { revealedLoss = true; break; }
            }
            bool settledCashedOut = _ticket.State == TicketState.CashedOut;
            bool settledDead = _ticket.State == TicketState.Lost && revealedLoss;
            bool ticketSettled = settledCashedOut || settledDead;

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
                // T69: re-authored for this column and fitted to it. One assignment feeds every row
                // state below, so the de-duplication and the no-wrap rule cannot apply to some
                // states and not others.
                string statement = FitToColumn(_legRow[i].Line, LegStatement(leg));
                string price = OddsFormat.American(leg.OfferedOdds);
                // A SETTLED ticket has no live leg. The session is complete, so this row's events
                // will never arrive; without this clause the leg AFTER the loser keeps rendering the
                // live form — a NEED and a progress line — on a ticket that can no longer pay.
                bool isLive = IsLiveShown(i) && !IsPresentedResolved(i) && !ticketSettled;
                _legRow[i].IsLive = isLive;

                if (IsPresentedResolved(i))
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
                    // T69, second half: the live statement used to terminate at the column rule
                    // MID-WORD (`RICO LANYARD TO SCO`). TV-12/13 wants truncation on a word
                    // boundary regardless, so it now ends at one. Still verbatim — truncating is
                    // not paraphrasing, and no word is ever split.
                    //
                    // FLAGGED, because it reads against §5.1: that section says NEED is "never
                    // wrapped, never truncated — an over-long NEED is re-authored against a
                    // call-site-recorded measurement." T69 postdates it and invokes TV-12/13's
                    // word-boundary rule for this exact string, so a word boundary is strictly
                    // better than the mid-word cut it replaces. But the DURABLE fix is shorter
                    // AUTHORED copy, and what a leg statement should say is a copy decision this
                    // seat does not hold. This removes the defect; it does not settle the string.
                    _legRow[i].Need.text = FitOrFallback(_legRow[i].Need, copy.Need, copy.NeedFallback);
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
                    // §8.10's CANCELLED treatment, extended from the PREVIEW of a settle to the
                    // settle itself. The word turns on whether the leg can still take his money:
                    // while previewing it CAN (he may decline and the leg plays on), so the preview
                    // keeps `NEXT` and only strikes it; once the ticket has settled it CANNOT, and
                    // the word goes. The chip falls silent rather than being re-authored — the
                    // strike is already this surface's mark for a cancelled leg and no new string
                    // is invented here (T121 left the dead ticket's copy to a frame).
                    SetRowChip(i, ticketSettled ? string.Empty : "NEXT",
                        AtTier(flavorColor, TierL2), price);
                    if (_legRow[i].Extinguish != null) _legRow[i].Extinguish.enabled = false;
                    // A pending leg is equally ended by cashing out, so it is struck too — and it
                    // stays struck once the ticket actually settles, by cash-out or by bust. Gated
                    // on `_cashOutPreview` alone this marked the leg cancelled while the player was
                    // DECIDING and un-marked it the moment it truly was, and never fired at all on a
                    // bust. It does NOT step down: NEXT already sits at L1 and the next level is L0,
                    // which is the LOST extinguish a cancellation must never borrow.
                    if (_legRow[i].Strike != null)
                        _legRow[i].Strike.enabled = _cashOutPreview || ticketSettled;
                    _legRow[i].Need.text = string.Empty;
                    _legRow[i].Progress.text = string.Empty;
                }
            }

            // §7: "Risk and pays sit at the foot in gold at L2."
            // T74-am5: two ends of one row. The five-space spacer is GONE — it was the thing being
            // measured, not the content, and anchoring retired it.
            // Amended for the state-lie fix: the footer's first word is no longer a hard-coded
            // "RISK" — it comes from the whole ticket's leg outcomes, so it reads "STAKE" once no
            // remaining leg can still lose it (SweatActiveLegModel.StakeWord). `_tPays` is UNCHANGED.
            // T114-am (cashed out) and T121 (dead) — THE SETTLED TICKET'S FOOTER, and the two are
            // ONE job because separately they restate. On a settled ticket BOTH incumbent words are
            // false: there is no risk on a position that is closed, and it will not pay what the
            // slot promises. T121 read it on frame — `RISK $25` and `PAYS $37` in money amber while
            // `−$60` two lines below said it paid nothing.
            //
            // `STAKE` / `RETURNED` is borrowed as a PAIR from S38's laptop ledger rather than
            // assembled from two singles, which is what keeps the two halves saying one thing.
            //
            // The state comes from `_ticket.State` — the ENGINE's own TicketState, already read at
            // two other sites in this file. A cash-out is a PLAYER ACTION and is not derivable from
            // leg outcomes at all, so `StakeWord` (which takes leg outcomes) structurally cannot see
            // it — T114 says so in terms. This reads the ticket, not a second source of truth.
            //
            // `settledCashedOut` / `settledDead` are HOISTED to the top of this method: the rows
            // need the same fact, and reading it twice is how the footer and the rows came to
            // disagree in the first place. One read, one truth, both consumers.

            if (settledCashedOut || settledDead)
            {
                _tRiskPays.text = $"STAKE ${Money(_ticket.Stake)}";
                // The dead ticket returned nothing and says so; the cashed-out one returns what the
                // player actually took, which is the accepted offer rather than the potential
                // payout he gave up.
                double returned = settledCashedOut ? _lastCashOutAmount : 0.0;
                if (_tPays != null) _tPays.text = $"RETURNED ${Money(returned)}";
            }
            else
            {
                _tRiskPays.text = $"{SweatActiveLegModel.StakeWord(BuildTicketLegOutcomes())} ${Money(_ticket.Stake)}";
                if (_tPays != null) _tPays.text = $"PAYS ${Money(_ticket.PotentialPayout)}";
            }

            // ⚠ T133 — MEASURED AT BUILD TIME, AND THE EXPOSURE IS REAL. `RETURNED` goes into the
            // footer's RIGHT half, which is `Pays` — THE WIDEST-BOUNDED SLOT ON THIS SURFACE, whose
            // worst case was established by enumeration over 648,000 priced offers. Measured:
            //
            //     PAYS $73,318,376,502      239.7px  against box 249.0px  fits,  9.3px spare
            //     RETURNED $73,318,376,502  300.9px  against box 249.0px  OVERRUNS by 51.9px
            //     RETURNED $0               146.5px  against box 249.0px  fits, 102.5px spare
            //
            // `RETURNED` is EIGHT characters where `PAYS` is FOUR, and nothing in T114-am or T121
            // priced the swap. **The dead case is safe — it is always $0. The CASHED-OUT case is
            // not bounded to $0 and its worst case overruns.**
            //
            // NOT dodged by shortening or truncating the word: that is a copy decision and C11 puts
            // copy on a frame. Recorded here and routed.
        }

        /// <summary>The whole ticket's leg outcomes, for <see cref="SweatActiveLegModel.StakeWord"/>.
        /// Built from the SAME sources <see cref="UpdateTicketColumn"/>'s own rows above render
        /// from — deliberately NOT from <see cref="RevealedView"/>'s mirror: its
        /// <c>ResolveLeg</c> has exactly one call site (<c>FinalSlam</c>), so on a multi-leg ticket a
        /// leg resolved through <c>ResolveBeat</c> never leaves <c>RevealedLegState.Live</c> there.
        /// Building on that mirror would make STAKE unreachable and would silently disagree with the
        /// chips the player is looking at.
        ///
        /// <para>Resolved legs (<see cref="IsPresentedResolved"/>) read <c>leg.IsVoided</c>/
        /// <c>leg.GradesWon</c> — this MIRRORS the resolved-row branch above exactly, and is not an
        /// endpoint leak: a leg is marked only at its OWN reveal moment, and that branch already
        /// reads these same two fields behind the same guard. The live rows
        /// (<see cref="IsLiveShown"/> — a SET since T140 arm A, because a shared telling has more
        /// than one live leg) take <see cref="DescribeActiveLeg"/>'s revealed-derived outcome, the
        /// only one that can be true before full time. Every other row (pending/NEXT)
        /// is <c>Undecided</c>.</para>
        ///
        /// <para>Never throws from this render path: a null <c>_ticket</c> yields an empty list, and
        /// <see cref="SweatActiveLegModel.StakeWord"/> already reads an empty list as RISK. Every
        /// call site of <see cref="UpdateTicketColumn"/> is event-driven, not per-frame, so this
        /// small per-call allocation is not cached.</para></summary>
        private List<RevealedLegOutcome> BuildTicketLegOutcomes()
        {
            var outcomes = new List<RevealedLegOutcome>();
            if (_ticket == null) return outcomes;
            for (int i = 0; i < _ticket.Legs.Count; i++)
            {
                Leg leg = _ticket.Legs[i];
                if (IsPresentedResolved(i))
                {
                    outcomes.Add(leg.IsVoided ? RevealedLegOutcome.Voided
                        : leg.GradesWon ? RevealedLegOutcome.Won : RevealedLegOutcome.Lost);
                }
                else if (IsLiveShown(i))
                {
                    outcomes.Add(DescribeActiveLeg(leg).Outcome);
                }
                else
                {
                    outcomes.Add(RevealedLegOutcome.Undecided);
                }
            }
            return outcomes;
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

            // T92 (batch 60): the entries take G1's COMPACT forms, ONE PER ROW.
            //
            // Two things were wrong and only one of them was the composition. `DisplayLabel` is the
            // ENGINE's concatenated label — `Yonkers Auditors ML — Yonkers Auditors v Reno Muskrats`
            // — which names the team THREE times: T69's defect verbatim, and an engine label in a
            // player-facing slot. Enumerated, one entry reaches 91 chars / 760.8px against a 655.0px
            // box, so **the list ruling alone could not save it: a list of over-wide rows is still
            // over-wide.** The row is long because its ENTRY is, not because it concatenates.
            //
            // `LegStatement` is G1's compact deck — the identity form, authored to a 143px budget and
            // already measured and shipping on the leg row. No new authoring: this is the slot it was
            // written for. The scorebug carries who is playing whom, which is what made dropping the
            // fixture half legal in the first place.
            // T92-am (batch 61): THE LEG LIST LEAVES THIS SLOT. It is a restatement.
            //
            // The takeover renders while the ticket column is showing the same legs, SIMULTANEOUSLY —
            // `ResetForNewSession` runs `RenderPregame` before this card draws, so the column is
            // already populated, and the frames show both at once. T69/T70's class and S37/S58's: the
            // same fact named twice, one panel over.
            //
            // The takeover's job is the ticket's IDENTITY and its MONEY, and both are already here and
            // unaffected — `TICKET 1 OF 1` above and `$87 TO WIN $686` below.
            //
            // THE HEIGHT PROBLEM DISSOLVES rather than being paid for. The cap route was closed by
            // C19's own arithmetic (2 rows fit, 3 overrun, so a 2-leg cap plus its count row is 3 rows
            // and a cap that cannot print its own count is the hidden offer C19 forbids). No cap, no
            // rows, no box growth, no deviation.
            //
            // Checked against the pre-commitment rather than assumed: if this card could render while
            // the column was NOT showing those legs the list would be load-bearing. It cannot — the
            // column is rendered on the path into this card.
            //
            // The SLOT survives; only its leg-list use goes. The deferral line still writes here.
            _tTakeoverSub.text = string.Empty;

            SetEventStrip(flavorColor);
            _tFlavor.text = $"${Money(_ticket.Stake)} TO WIN ${Money(_ticket.PotentialPayout)}";

            // T164: data only, and the TICKET's number rather than leg 0's. Guarded — the card is a
            // session boundary (see ResetForNewSession's caller note).
            _probTarget = (float)(_session != null ? _session.TicketWinProbability : 0.0);
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
                SetEventStrip(contextGrey);
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
                SetEventStrip(new Color(gold.r, gold.g, gold.b, 1f));
                _tFlavor.text = $"−${Money(s.Payment)}   ·   BANK ${Money(s.BankAfter)}";
                _tTakeoverSub.text = string.Empty;
                EmissionFlash(gold);
                // T65: a round settling is a settlement, so it keeps a re-tint — but through the
                // one palette-bound entry point, never `gold` at 3.0.
                //
                // FLAGGED FOR THE DD, not decided here: this branch is money going OUT (the bookie
                // paid). The room warming for it is arguable — the register of a payment is not the
                // register of a payoff. Left firing because T65 ruled the re-tint's TRIGGER class
                // (settlement, not leg) and its palette, not which settlements deserve one; killing
                // it here would be a design call this seat does not hold.
                RoomSettlementGlow();
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
            // T164: the bed empties with the mirror — Clear() zeroed WinProbability, which is what
            // this line read before, so the crowd falls silent on exactly the same frame.
            _tensionProb = _pendingTensionProb = 0f;

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
            // T164: the bed empties with the mirror — Clear() zeroed WinProbability, which is what
            // this line read before, so the crowd falls silent on exactly the same frame.
            _tensionProb = _pendingTensionProb = 0f;

            ClearToBlankScreen();
            _tAttract.enabled = true;
            // T86-am2 (batch 56): BOTH states were violations and both take one authored replacement.
            //
            // `THE HOUSE BLINKS FIRST` rendered in GOLD, and §3.1 rations gold to won legs, payout
            // figures and the cash-out band — an editorial line is none of the three. That is a
            // ration violation on §3's own terms, independent of T27, and the copy was separately
            // celebratory editorial. `THE BOOKIE COLLECTS` was S53's string, the LAPTOP's losing
            // verdict headline, on a surface with a different register: the theatre does not announce
            // a verdict another surface owns.
            //
            // `BOARD CLOSED` is the surface's own vocabulary — idle prints `ROUND n OF m · BOARD
            // OPEN`, and the run ending closes the board. One word changes and the grammar is
            // identical. **The collapse to one string is the point:** the run's verdict is the
            // laptop's screen (S53/S59), so the theatre reports that the board is shut and says
            // nothing about how the run ended, because that is not its to say.
            _tAttract.text = "BOARD CLOSED";
            // flavorColor in BOTH states, like states 1–3. Not gold in either — that was the ration
            // violation, and `won` no longer selects an ink here.
            _tAttract.color = flavorColor;
            _tSubtitle.text = $"FINAL BANK ${Money(r.Bank)}  —  NEW RUN AT THE LAPTOP";

            if (won)
            {
                // The run's final payout — §3's L4, "the payoff at its callback".
                _emissRest = RunWonRest();
                EmissionFlash(goldL4);
                // T65: the run's payout is the largest settlement there is. The transient goes
                // through the one re-tint; the SUSTAINED rest is this card's alone (it is a
                // persistent verdict screen, not a beat) and takes the same room warm at a dim
                // hold rather than the 39.6 deg gold it used to sit in for the whole card.
                RoomSettlementGlow();
                tvLight?.SetRest(roomSettlementWarm, 0.45f);
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
            UpdateTicketColumn(System.Array.Empty<int>()); // no live leg — what -1 used to say
        }

        private void RenderEvent(DramaEvent evt)
        {
            Leg leg = _ticket.Legs[evt.LegIndex];

            // T165 / T165-am: the FIXTURE is the referent — see RenderPregame's note. `evt.LegIndex`
            // is only the telling's ANCHOR after arm A, so counting it would name one leg of a
            // shared telling and skip the other.
            _tLeg.text = $"MATCH {evt.FixtureIndex + 1}/{FixtureTotal()}";

            // THE DIRECTION IS DECIDED FIRST, AND THAT ORDERING IS THE POINT. The model owns the
            // rule (one authority); the flavour used to derive its OWN `up` from a local _prevProb,
            // which is a second implementation of the same rule and — after T164 re-based the
            // number to the TICKET — would have been a second implementation of a DIFFERENT rule.
            // Recording the beat first and handing the answer down leaves exactly one.
            _lastBeatUp = _presModel.RecordBeat(
                evt, _session != null ? _session.TicketWinProbability : 0.0);
            SweatPresentationModel.BeatRecord beat = _presModel.Beats[_presModel.Beats.Count - 1];
            _lastBeatDelta = beat.Delta;

            string flavor = SweatFlavor.For(evt, leg, _lastBeatUp, SweatFlavor.AnchorForTelling(_ticket, evt));

            // T87-am (batch 68) — THE DRAWN MATCH'S CLOSING LINE.
            //
            // A DECIDED match ends ON a goal, so its final beat's line IS its ending and the strip is
            // already correct. A DRAWN match ends on nothing, so the last beat's line is stale by
            // construction — there is no closing event to speak. The strip's silence at a draw is
            // STRUCTURAL, which is why it is the only result that needs an authored ending.
            //
            // `FULL TIME — LEVEL` was the obvious form and is REFUSED: the scorebug prints `FT` in
            // the clock slot directly above, so it would state one fact twice, one slot apart. The
            // strip's job is to say what the score and clock cannot.
            //
            // Nothing here is 0–0-specific, deliberately: `THE MATCH ENDS LEVEL` is true at 0–0 and
            // at 2–2, and a goalless-only line would be exactly the narrowing T87 §6.8 forbids.
            //
            // LEVEL is read from the REVEALED ledger, never from the locked StatLine — §4.1's rule
            // that presentation reads revealed facts. At the whistle the two agree, which is why the
            // honest source costs nothing here.
            if (evt.Type == DramaEventType.LegFinal && _ledger.Picked == _ledger.Opponent)
                flavor = "THE MATCH ENDS LEVEL";

            // T16: a non-final beat's dot lands at its reveal (RevealBeatChrome), never here —
            // looking away must never spoil a beat.
            _pendingTapeBeat = evt.Type != DramaEventType.LegFinal;
            if (_stage != null)
            {
                // Causal reveal (M-T3.1): identity chrome may update now, but the win-prob,
                // flavor, and clock are STASHED — they land at the scene's payoff
                // (RevealBeatChrome / FinalSlam), never before the pitch has shown the story.
                // T164: the stashed number is the TICKET's, the only probability presentation may
                // display (T143). Reading the session here is no earlier a reveal than reading the
                // beat's own WinProbAfter was — the engine has already consumed the beat at this
                // point either way, and the stash is still landed only at RevealBeatChrome.
                _pendingProb = (float)_session.TicketWinProbability;
                _pendingFlavor = flavor;
                TraceFlavor($"RenderEvent stash {evt.Type}", flavor);

                if (evt.LegIndex != _stageLeg || _stageBeatCount != evt.TotalSteps)
                    BeginStageLeg(evt.LegIndex, leg, evt.TotalSteps);
                // THE PITCH AND THE CROWD STAY LEG-SCOPED. TheaterStage._prob is the PICKED SIDE's
                // territory truth (TheaterStage.cs:83, driving PitchLayout.TerritoryX and the
                // possession share) — a per-MATCH dramatic fact, not the displayed number — so it
                // reads LegProbs, never the ticket's product (which would pin the pitch to one end
                // on any multi-leg ticket) and never WinProbAfter, which after the fixture
                // restructure is only the ANCHOR leg's and would silently show one leg's number for
                // every leg. Value-identical today: LegProbs[0] == WinProbAfter on every event.
                // The tension bed takes its referent from the SAME read, so territory and crowd
                // never disagree about which match they are describing.
                _pendingTensionProb = (float)evt.LegProbs[0];
                _stage.SetLiveProb(_pendingTensionProb);
                UpdateScorebug(leg); // colored identity + running score (M-T3 scorebug)
            }
            else
            {
                _tClock.text = SweatFlavor.Clock(evt);
                SetEventStrip(flavorColor);
                _tFlavor.text = flavor;
                _flavorScale = 1.12f; // punch
                // T164: the theaterless fallback displays the same TICKET number the staged path
                // stashes. The tension bed still needs its per-match referent, so it is driven here
                // too — this branch has no stage, but the crowd is still playing.
                _probTarget = (float)_session.TicketWinProbability;
                _tensionProb = (float)evt.LegProbs[0];
                _tMatchup.text = MatchupLine(leg);
                RevealedView.SetProbability(_probTarget);
                RevealedView.SetClock(_tClock.text);
                UpdateCashOutLabel();
            }

            _tAttract.enabled = false;
            // Every leg on THIS telling is live, not just the anchor — one fixture, one whistle.
            UpdateTicketColumn(evt.LegIndices);
        }

        /// <summary>The beat's payoff moment on the stage: NOW the chrome may speak — the
        /// flavor line lands, the clock ticks, the market re-opens at the fresh price. Fired by
        /// the scene's onReveal (goal / save / scene end).</summary>
        /// <summary>T97/T87-am2's owed diagnostic: log every write to the event strip with its CALL
        /// SITE, so the write order across a beat is a FACT rather than a hypothesis.
        ///
        /// <para>The DD asked for exactly this and could not run it — *"this seat cannot execute the
        /// code and does not claim the ordering as fact."* The strip has several writers and the
        /// authored ones are not obviously last; two rulings landed in one slot and the frames said
        /// the wrong writer won. Reasoning about the order was what produced a wrong hypothesis, so
        /// this stops reasoning and records it.</para>
        ///
        /// <para>Off by default and set only by the capture harness, so production logs nothing.</para></summary>
        public static bool TraceFlavorWrites;

        /// <summary>PRD §9 diagnostic: the event strip's current line, so a capture frame can carry
        /// its own strip text in the harness log. T87-am2 is verifiable only as "the line was VISIBLE,
        /// for multiple frames, BEFORE the grade" — a claim about frames that the frames themselves
        /// should be able to answer without a second instrument.</summary>
        public string DebugFlavorText => _tFlavor != null ? _tFlavor.text : string.Empty;

        private static void TraceFlavor(string site, string value)
        {
            if (TraceFlavorWrites) Debug.Log($"[FLAVOR] {site,-28} <- '{value}'");
        }

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
            _tensionProb = _pendingTensionProb; // T164: the bed lands on the same seam, never earlier
            RevealedView.SetProbability(_probTarget);
            RevealedView.SetClock(_tClock.text);
            SetEventStrip(flavorColor);
            _tFlavor.text = _pendingFlavor;
            TraceFlavor("RevealBeatChrome LAND", _pendingFlavor);
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
            // T112 (register batch 104): re-authored from "MARKET SUSPENDED", which overran the
            // 241.0px box by 26.7px on EVERY frame — a constant that overruns is a defect, not a
            // risk, so the fix is the shorter word, never a wider box. Measured 2026-08-17:
            // 267.7px (incumbent) vs 152.3px (this string) against the same 241.0px box, 88.7px
            // spare. The surviving constraint: the state must still be STATED, never carried by
            // grey alone — do not weaken this further to a glyph, an icon, or an empty slot.
            _tCashOut.text = "SUSPENDED";
            // T68: the ink is no longer set here. ApplyCashOutSlotState derives all three states
            // from the flag set on the line above, so the slate's grey and the lit field's punched
            // ink cannot drift apart at separate sites.
            ApplyCashOutSlotState();
        }

        /// <summary>No offer to show. Goes through the same re-derivation so the field and status can
        /// never outlive the figure they belong to.</summary>
        private void HideCashOutSlot()
        {
            _cashOutSlotSuspended = false;
            _cashOutAccepted = false; // T68-am: the accepted state ends when the slot does
            _tCashOut.enabled = false;
            ApplyCashOutSlotState();
        }

        /// <summary>T68-am / T71: §6.1's `accepted` state — the payoff figure rendered IN THE SLOT.
        ///
        /// <para>Both payoff moments used to draw their money on a 96px canvas-centre figure over a
        /// SINE-PULSING gold flood. That ground is not a field: measured in linear relative
        /// luminance, gold-on-flood runs 12.47:1 at alpha 0 down to <b>1.71:1</b> at the 0.55 peak
        /// (1.83:1 for the win tally at 0.50), and the obvious fix — dark ink — inverts the problem
        /// rather than solving it, at 1.08:1 for most of the beat. Neither static ink is right
        /// because the ground moves.</para>
        ///
        /// <para>So the figure comes back to the slot §6.1 always specified for it, where the field
        /// is stable and the inversion T68 built is already measured at 7.95:1 (9.68:1 computed at
        /// this state's L3). <b>The flood is untouched</b> — it stays as the payoff's celebration
        /// ground and simply stops being the thing a money figure has to be legible against.</para>
        ///
        /// <para>T43 is not in tension: "nothing of the offer outlives the accept" means the OFFER —
        /// the price, the `HOLD E` instruction, the actionable field's promise about input. The slot
        /// as a rectangle is the surface's own furniture and this is one of its six states.
        /// T35 is satisfied in the same move: the full-screen celebration figure is gone.</para></summary>
        private void ShowCashOutAccepted(string figure)
        {
            if (_tCashOut == null) return;
            _cashOutSlotSuspended = false;   // an accepted slot is not a slate
            _cashOutAccepted = true;
            _tCashOut.enabled = true;
            _tCashOut.text = figure;
            ApplyCashOutSlotState();         // one derivation lights the field and punches the ink out
        }

        private void ReopenMarket()
        {
            _marketSuspended = false;
            _cashOutSlotSuspended = false;
            RevealedView.SetMarketSuspended(false);
            UpdateCashOutLabel(); // T68: ink derived in ApplyCashOutSlotState, not chosen here
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
                // T68: no ink here either — clearing the flag above is what restores it, through
                // the one derivation, which also decides whether it restores LIGHT gold or the
                // punched-out dark of a lit field.
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
            if (_tCashOutStatus != null) _tCashOutStatus.text = CashOutStatusWord();
            // T43: whether the status word SHOWS is not this method's call — a suspended slot carries
            // none at all (TV-12/13). One authority derives visibility, at the transition and in
            // Update alike, so the two can never disagree for a frame.
            ApplyCashOutSlotState();
        }

        /// <summary>The status word beside the money figure. ONE authority for all three sites that
        /// write it, which is this surface's standing remedy for a value with several authors — T68's
        /// defect was never the ink, it was that the ink had five of them.
        ///
        /// <para><b>C48 (batch 50): the label is the contract.</b> The word has to describe what the
        /// input will actually do at the moment it is read, so it now has three states rather than
        /// two: at rest E previews, mid-tween nothing is acceptable, and under a held preview the only
        /// remaining act is the commit. Printing <c>HOLD E</c> during the hold would tell the player
        /// to do the thing he is already doing and never say how to finish it.</para>
        ///
        /// <para><b>UNRATIFIED:</b> the previewing string is the seat's, in T22/T86(a)'s established
        /// form (print the word, not the glyph). It is routed with the intervention prompt's strings.
        /// Its extent is measured, not assumed — this slot shares one rectangle with the money figure
        /// and the pair already collides.</para></summary>
        private string CashOutStatusWord()
            => _cashOutPreview ? $"{ConfirmKeyWord} TO CASH OUT"
             : _cashOutTweening ? "UPDATING"
             : "HOLD E";

        private void StopCashOutAnimation()
        {
            if (_cashOutAnimation != null)
            {
                StopCoroutine(_cashOutAnimation);
                _cashOutAnimation = null;
            }
            _cashOutTweening = false; // unconditional: always leaves "not tweening", idempotently
        }

        /// <summary>T69: the ticket row's statement, authored for THIS column.
        ///
        /// <para>The engine's <c>DisplayLabel</c> concatenates a pick and a fixture without noticing
        /// they can share a term: <c>"{Picked} ML — {Away} v {Home}"</c> prints the backed team
        /// TWICE — "Atlanta Middlemen ML — Atlanta Middlemen v Tulsa Startups". Only Moneyline has
        /// that shape; every other market names no team in its own half, so there is nothing to
        /// de-duplicate there and this method leaves their structure alone.</para>
        ///
        /// <para>The engine is deliberately untouched. <c>DisplayLabel</c> is shared with the
        /// console and the laptop, and it is read and re-authored here rather than changed at the
        /// source — the same shape as T42's two team constants: a per-surface presentation choice
        /// belongs on the surface. (SureThing independently landed the same "X ML — v Y" form in
        /// its own file, which is the corroboration, not the dependency.)</para>
        ///
        /// <para>Names come through <see cref="SweatFlavor.Short"/>, which is what the scorebug
        /// already uses — the row and the scorebug should not disagree about what a club is
        /// called.</para></summary>
        private string LegStatement(Leg leg)
        {
            // G1: NEED states the REQUIREMENT, this states the IDENTITY. A live row asks "what does
            // my money still need"; every other row asks "which bet is this". Where those two
            // questions have the same answer — the totals markets — the two strings are IDENTICAL,
            // and that is correct rather than a duplication to design away.
            //
            // Built from the SELECTION, not parsed out of DisplayLabel: the fixture half is dropped
            // entirely, because the scorebug already carries who is playing whom and the BACKED
            // marker already carries the side. That is what makes 143px workable at all.
            MarketSelection sel = leg.Selection;
            string overUnder = sel.Choice == MarketChoice.Over ? "OVER" : "UNDER";
            switch (sel.Kind)
            {
                case MarketKind.Moneyline:
                    // T96 (batch 68): THE DRAW IS ITS OWN ROW, and it must never borrow a team's.
                    //
                    // This branch was a two-way `pickedHome ? Home : Away` because THAT IS WHAT THE
                    // COPY DECK SAID — one Moneyline row, two-way, no draw case. The deck predates
                    // S74's draw authoring by four days and was never amended, so a draw ticket
                    // printed `MIDDLEMEN ML`: a team pick, on a ticket that backed neither team.
                    // Both tickets in the goalless set printed the same string with opposite grades.
                    //
                    // The deck now carries the row (`DRAW`, compact; `LEVEL AT FULL TIME`, NEED), and
                    // the build takes it from there. The reusable half: A COPY RULING LANDS IN THE
                    // DECK OR IT HAS NOT LANDED — S74 was ruled, folded into the owning doc, and
                    // still shipped a defect, because the deck sat between the doc and the build.
                    if (sel.Choice == MarketChoice.Draw) return "DRAW";
                    // Clubs by their distinctive word, city dropped — the convention T69 shipped.
                    bool pickedHome = SweatFlavor.PickedHomeForPresentation(leg);
                    string club = SweatFlavor.Short(
                        (pickedHome ? leg.Matchup.Home.Name : leg.Matchup.Away.Name)).ToUpperInvariant();
                    return $"{club} ML";
                case MarketKind.TotalGoals:
                    return $"{overUnder} {sel.Line:0.0} GOALS";
                case MarketKind.BothTeamsToScore:
                    return sel.Choice == MarketChoice.Yes ? "BTTS YES" : "BTTS NO";
                case MarketKind.TotalCorners:
                    return $"{overUnder} {sel.Line:0.0} CORNERS";
                case MarketKind.TotalCards:
                    return $"{overUnder} {sel.Line:0.0} CARDS";
                case MarketKind.CorrectScore:
                    // T151's compact form. The DASH IS THE SURFACE'S OWN — SweatActiveLegModel
                    // declares `Dash = '\u2013'` (EN DASH) and the T84 pool already carries
                    // `EXACT 3\u20131` with those bytes. The spec prints an ASCII hyphen; the CODE
                    // decides, and a one-character divergence here is how the last five phantoms
                    // started.
                    return $"EXACT {sel.ScoreHome}{SweatActiveLegModel.DashChar}{sel.ScoreAway}";
                case MarketKind.AnytimeScorer:
                    // Same surname rule as NEED, from the same helper — two copies of one convention
                    // is how the two halves of a statement drift apart.
                    return $"{SweatActiveLegModel.Surname(leg.Matchup.PlayerAt(sel.PlayerIndex).Name)} ANYTIME";
                default:
                    // A seventh market arrives here unauthored, and G1 names that as not covered. The
                    // old fallback was `leg.DisplayLabel` — and THE CONSOLE ALREADY RULED THAT THE
                    // DEFECT. SweatLines.LegName reads the SHEET and says so in terms: "Nothing here
                    // falls back to the enum name: THAT FALLBACK IS K16/T130." On a live Handicap
                    // leg DisplayLabel gave the bare word `Handicap` — a leg of the player's ticket
                    // naming its market TYPE instead of his bet, on a row whose compact line is
                    // blanked by design. Found by the anchor capture window.
                    //
                    // Read through MarketSheet instead — the ONE composer this surface, the laptop
                    // and the console all print through (S96, §6.5) — so an unauthored kind names
                    // the bet in the same words the BOARD offered it in. Still no copy invented
                    // here, which is what G1 actually asks for.
                    return SheetName(leg) ?? leg.DisplayLabel;
            }
        }

        /// <summary>`T143`: the pending window's name for ONE dying leg — <b>the club and its
        /// qualifier, and nothing else</b>. Copy authority is
        /// <c>docs/design/spec-pending-window-copy-2026-08-25</c> §2 (`T143-am4`), amended by
        /// `T143-am5` (DD batch 192).
        ///
        /// <para><b>Deliberately NOT <see cref="LegStatement"/>.</b> That composes a full STATEMENT
        /// for the ticket column — <c>{club} ML</c>, <c>OVER 2.5 GOALS</c> — and the pending window's
        /// decline row ABSORBS the name into a sentence it already supplies the verb and the tense
        /// for (<c>N LET … DIE</c>). A market word or a second verb inside the name restates what
        /// the row is saying around it, which is the class S37/S58/T69/T70 have each ruled.</para>
        ///
        /// <para><b>`Short` IS APPLIED HERE, AT THIS CALL SITE — `T143-am5`, and this is the whole
        /// point of that row.</b> `T168-am` ruled the club token is shortened at the RENDER and is
        /// RULED-BUT-UNBUILT at HEAD, so naming the leg the obvious way — calling
        /// <see cref="LegStatement"/> and taking what comes — reaches its <c>default:</c> arm →
        /// <see cref="SheetName"/> → <c>MarketSheet</c> → <c>fields.Line</c>, which for a handicap is
        /// <c>{hteam} ±1.5</c> with the <b>FULL</b> club name: the window would print
        /// <c>N LET DULUTH AUDITORS +1.5 DIE</c> while the spec and its measurement describe
        /// <c>N LET AUDITORS +1.5 DIE</c>. <b>New copy is built to the RULING, not to the build
        /// state</b> — a new surface built to today's defect has to be fixed twice.</para>
        ///
        /// <para><b>The club comes from <see cref="MatchModel.AnchorSide"/>, NOT from
        /// <c>SweatFlavor.PickedHomeForPresentation</c>.</b> The anchor answers <c>null</c> for
        /// NEITHER, and that distinction is exactly what this row needs: the adapter collapses
        /// <c>null</c> to HOME because a scoreline must be drawn in SOME direction, and that
        /// convention on THIS row would name a club the ticket never backed — `T96`'s defect
        /// verbatim (<c>MIDDLEMEN ML</c> printed on a draw ticket). A row naming the wrong club is
        /// worse than a row naming no club.</para>
        ///
        /// <para><b>ONE qualifier is authored, and authoring more is NOT this seat's scope.</b> §2
        /// gives the handicap <c>{CLUB} ±1.5</c> (`G1-am11` rung 3) and no other kind a short
        /// qualifier. Where none is authored the row takes <b>the club alone</b> rather than a coined
        /// abbreviation — a short form nobody wrote is G1's defect class, which is the rule that
        /// exists to stop exactly this improvisation. New qualifier copy is the DD's to author.</para>
        ///
        /// <para><b><c>TeamTotalGoals</c>/<c>TeamTotalCorners</c>/<c>TeamTotalCards</c> INHERIT
        /// `T156` AT THE SOURCE (DD batches 191/192) — known, not an oversight in this window.</b>
        /// Their name comes off the same <c>fields.Line</c> batch 191 ruled defective: it truncates
        /// to <c>RENO FERRETS OVER</c>, a direction with no market, and `Short` does not rescue it
        /// (the measured 449.5 against 261.0 is ALREADY the short-club form). No wording available
        /// inside this window can repair a name that is broken where it is composed, so `T143-am5`
        /// ruled the window <b>inherits Allen's existing scope call rather than opening a second
        /// one</b>: these three take the same path as every other kind with no authored qualifier —
        /// the club alone, no special case, no repaired name invented here.</para>
        ///
        /// <para><b>Where the leg names NO club the spec authors no form</b>, and <c>{CLUB} {LINE}</c>
        /// cannot be built at all — so the row falls back to the leg's own authored identity
        /// (<see cref="LegStatement"/>), which for every anchorless kind is club-free BY
        /// CONSTRUCTION (<c>OVER 2.5 GOALS</c>, <c>BTTS YES</c>, <c>DRAW</c>, <c>EXACT 3–1</c>,
        /// <c>ODD</c>, <c>3+ GOALS</c>, <c>EITHER TEAM</c>). `T143-am5`'s full-club hazard therefore
        /// cannot arise down this path: the hazard is a club name, and these strings carry none.
        /// Reported as a gap in the spec rather than filled with copy invented here.</para></summary>
        private string PendingLegName(Leg leg)
        {
            if (leg == null) return null;
            // NO ANCHOR, NO CLUB — take the authored identity whole rather than borrowing a side.
            //
            // `AnchorSide` THROWS on a kind it has no arm for, and that is K17-cl's deliberate
            // design, not an oversight to soften here: it is not new exposure either, because every
            // sweat already runs it per leg through `PickedHomeForPresentation` (the scorebug, the
            // stats panel, the ledger endpoint) long before this window can open. A sixteenth market
            // kind fails at the engine's table, which is where it should fail.
            Side? anchor = MatchModel.AnchorSide(leg);
            if (anchor == null) return LegStatement(leg);

            string club = SweatFlavor.Short(
                anchor == Side.Home ? leg.Matchup.Home.Name : leg.Matchup.Away.Name).ToUpperInvariant();
            // THE SIGN IS THE ENGINE'S OWN RULED FORMAT, read back rather than re-derived: the
            // handicap line is SIGNED and applied TO THE BACKED SIDE (home −1.5 must win by two,
            // away +1.5 may lose by one), and `MatchModel.Fields`' handicap arm prints it as
            // `{hteam} {Line:+0.0;-0.0}`. Reusing that format string is what stops a sign convention
            // drifting between two surfaces that must agree about one bet.
            return leg.Selection.Kind == MarketKind.Handicap
                ? $"{club} {leg.Selection.Line.ToString("+0.0;-0.0", CultureInfo.InvariantCulture)}"
                : club;
        }

        /// <summary>G1: the authored form if it fits, else the authored SHORTER line, else the
        /// truncation backstop.
        ///
        /// <para>§8: copy "truncates or chooses a shorter authored line; it never shrinks", and T69
        /// settled which is which — <b>truncation is the floor, re-authoring is the fix</b>. So a
        /// miss takes the fallback, which is authored to read as a whole sentence, and never a
        /// sentence with its end cut off. <see cref="FitToColumn"/> remains only as the structural
        /// guard against broken glyphs and should not be reached by shipped copy.</para></summary>
        private static string FitOrFallback(TMP_Text target, string primary, string fallback)
        {
            if (Fits(target, primary)) return primary;
            if (!string.IsNullOrEmpty(fallback) && Fits(target, fallback)) return fallback;
            // Both missed: clip the better of the two on a word boundary rather than emit nothing.
            return FitToColumn(target, string.IsNullOrEmpty(fallback) ? primary : fallback);
        }

        /// <summary>T69/TV-12/13: fit a statement to its measured column by dropping whole words.
        ///
        /// <para>§5.1 reserves every leg slot at a fixed height and T24 forbids a string bending to
        /// a stale measurement — so the string is re-authored against the column's ACTUAL width, at
        /// the call site, measured on the element that will render it. Three-line wrapping is the
        /// string exceeding a fixed slot, which is the one thing that section forbids.</para>
        ///
        /// <para>Truncation is on a WORD boundary, never mid-word (TV-12/13). A single word wider
        /// than the column is returned whole and left to the element's existing Wrap mode to clip —
        /// that is T46's containment backstop, and it is preferred to emitting a half-word.</para></summary>
        /// <summary>Does this string fit its element's measured column? One measurement, shared by
        /// the fallback chooser and the truncation backstop, so the two can never disagree about
        /// what "fits" means.</summary>
        private static bool Fits(TMP_Text target, string s)
        {
            if (target == null || string.IsNullOrEmpty(s)) return true;
            float max = target.rectTransform.rect.width;
            if (max <= 0f) return true; // no layout yet — do not judge against a width of zero
            // TMP's GetPreferredValues returns canvas units directly, so the UGUI form's division by
            // pixelsPerUnit has no counterpart here — carrying it across would have shrunk every
            // measured width by the canvas scale and made everything "fit".
            return PreferredWidth(target, s) <= max;
        }

        /// <summary>One preferred-width measurement, shared by the fallback chooser and the
        /// truncation backstop so the two can never disagree about what "fits" means — the property
        /// the UGUI version had by sharing one TextGenerator, kept by sharing one call.
        ///
        /// <para><b>The width argument is not decoration; passing 0 broke this.</b> T-3 ported the
        /// UGUI form as <c>GetPreferredValues(s, 0f, 0f)</c>, and on a component with wrapping
        /// enabled TMP takes that literally: it wraps at zero width and returns the widest GLYPH
        /// rather than the widest STRING. The compact statement slot is the one slot here with
        /// wrapping on, so <see cref="FitToColumn"/> compared about 12.5px against a 143px column and
        /// its loop never ran — the truncation backstop was dead from the migration until T84's sweep
        /// measured the measurer. UGUI's <c>GetPreferredWidth</c> returned the unwrapped width
        /// whatever the wrap mode, so nothing in the diff looked wrong.</para>
        ///
        /// <para>Measured unconstrained, which is what both callers mean: "how wide would this be if
        /// nothing stopped it", asked so it can be compared against the width that does.</para></summary>
        private static float PreferredWidth(TMP_Text target, string s)
            => target.GetPreferredValues(s, Unconstrained, 0f).x;

        /// <summary>A width no string on this surface can reach, standing in for "do not wrap while
        /// measuring". Not float.MaxValue: TMP multiplies the constraint into its layout maths, and a
        /// value that large returns infinities.</summary>
        private const float Unconstrained = 100000f;

        private static string FitToColumn(TMP_Text target, string s)
        {
            if (target == null || string.IsNullOrEmpty(s)) return s;
            float max = target.rectTransform.rect.width;
            if (max <= 0f) return s; // no layout yet — never truncate against a width of zero

            string cur = s;
            while (PreferredWidth(target, cur) > max)
            {
                int cut = cur.LastIndexOf(' ');
                if (cut <= 0) return cur; // one long word: clip it, do not split it
                cur = cur.Substring(0, cut).TrimEnd();
                // Drop a connector left dangling at the end. Matched as a whole trailing TOKEN, not
                // by trimming characters — trimming 'v' would eat the last letter of a club whose
                // short name ends in one.
                while (cur.EndsWith(" v", System.StringComparison.Ordinal)
                       || cur.EndsWith(" ·", System.StringComparison.Ordinal)
                       || cur.EndsWith(" —", System.StringComparison.Ordinal))
                    cur = cur.Substring(0, cur.Length - 2).TrimEnd();
            }
            return cur;
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
            // ANCHOR's number, and it stays the anchor's — same open DESIGN question as FinalSlam's
            // `k`: the copy for a shared telling is not ruled, and this lane does not get to rule it.
            int k = evt.LegIndex + 1;

            if (leg.IsVoided)
            {
                // TV-05: the event strip is neutral — "it never uses money hues" and "stays neutral
                // even when the event helps or hurts; money semantics live on the leg rows and the
                // cash-out slot" (TvEventStrip.jsx:5, prompt.md:7). The VOID hue belongs to the leg
                // row, which already carries it. TV-32: em dash, not a hyphen.
                SetEventStrip(flavorColor); // raw ink — the helper applies L2, so this is not double-tiered
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

            // Same pair as FinalSlam's, for the theaterless path: the whole live set is marked at
            // the one whistle, and the NEXT FIXTURE's legs read LIVE once their events start.
            // `+ 1` computed that only on a ticket with no same-match pair (see LegsOfFixtureAfter).
            MarkPresentedResolved(evt.LegIndices);
            UpdateTicketColumn(LegsOfFixtureAfter(evt.LegIndex));
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
            SetEventStrip(flavorColor); // raw ink — the helper applies L2, so this is not double-tiered
            _tFlavor.text = $"LEG {k} — WON";
            TraceFlavor("grade WON", _tFlavor.text);
            // T65: the two lines that used to sit here — `EmissionFlash(gold)` and
            // `tvLight.Flash(gold, 3.0f)` — are GONE, and the comment that licensed them
            // ("those are the TV being a lit object in a room, not the canvas painting itself
            // gold") was the last surviving instruction to do the banned thing. It was wrong twice
            // over: gold's 39.6 deg is not the room's palette at any amplitude, and a LEG is not a
            // settlement. Measured, that pair took the room to hue 40.7 deg at 71.1% saturation and
            // roughly double the luma — T40's deleted wash, relocated.
            //
            // This is the SECOND time this exact method has kept a violation its siblings had
            // already had fixed (the first is recorded against the suspended-slate work). The
            // remedy both times is the same: fix by rule, not by site. There is now exactly one
            // room re-tint and it is RoomSettlementGlow(), which this beat deliberately does not
            // call. The leg's win is carried where §3.1 already carries it — the row goes L3 gold.
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
            SetEventStrip(contextGrey); // raw ink — the helper applies L2, so this is not double-tiered
            _tFlavor.text = $"LEG {k} — DEAD"; // TV-32: em dash, the system's own dash
            TraceFlavor("grade DEAD", _tFlavor.text);
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
            // T71: the SAME treatment as the accept. Both payoff moments render their figure in the
            // slot, over the flood rather than against it — measured at 1.83:1 gold-on-flood at this
            // beat's 0.50 peak, which is T68's defect one beat over. Ruling them together is the
            // whole point: two payoff moments drifting apart in treatment is the class of drift that
            // produced a money control with an unreadable label.
            ShowCashOutAccepted("+$0");
            // The ticket's payout tally — §3's L4, "the payoff at its callback", brighter than a
            // routine won-leg flash so the ordering idle < flash < L4 stays visible.
            EmissionFlash(goldL4);
            RoomSettlementGlow(); // T65: the ticket paying out IS the settlement this is reserved for
            StartCoroutine(PunchThenSettle(HdrFocus.CashOut));
            StartCoroutine(WinConfetti());

            float duration = Mathf.Max(0f, winTallyDuration * Mathf.Max(0f, TimeScaleOverride));
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += SeatedDeltaTime; // TVS-H02: freezes exactly while standing
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                // The figure tallies in place; the slot's field and ink do not move under it.
                _tCashOut.text = $"+${Money(payout * t)}";
                yield return null;
            }
            _tCashOut.text = $"+${Money(payout)}";
            yield return ScaledWait(Mathf.Max(0f, winConfettiDuration - winTallyDuration));
            HideCashOutSlot();
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
            // T68-am: §6.1's accepted state — "brief L4 punch, then CASHED OUT $x at L3" — rendered
            // in the slot, not on a canvas-centre figure over the flood.
            // T114-am: THE BANNER DROPS ITS AMOUNT. `CASHED OUT` bare.
            //
            // The footer and the banner are ONE job: the footer now states the return
            // (`RETURNED $x`), so a banner also stating it would name the same fact ONE SLOT APART —
            // T69/T70's family, and authoring them separately walks straight into it.
            //
            // AND THIS DISPOSES OF T112-am WITHOUT A SEPARATE FIX. The lane routed
            // `CASHED OUT $1,240` at 255.6px against a 241.0px box — over by 14.6px — as copy
            // awaiting a frame. It was not awaiting anything: batch 108 had already ruled the drop,
            // on independent grounds and four hours before the overrun was routed. `CASHED OUT`
            // bare is ten characters against seventeen, so the overrun does not survive the ruling.
            // The lead re-runs the sweep to price that rather than assert it.
            ShowCashOutAccepted("CASHED OUT");
            EmissionFlash(goldL4);
            RoomSettlementGlow(); // T65: taking the money is a settlement
            // The punch runs ALONGSIDE the flood, not before it: blocking here would delay the
            // celebration ground by hdrPunchDuration and quietly re-pace a shipped beat.
            StartCoroutine(PunchThenSettle(HdrFocus.CashOut));
            // T40 (batch 27): the flood is struck, but the BEAT'S LENGTH is not — it is how long the
            // accepted figure holds before the slot clears, and shortening it here would re-pace a
            // shipped beat under cover of deleting an effect. The duration keeps its name because
            // three tests and the scene asset address it by that name.
            yield return ScaledWait(cashOutFloodDuration);
            HideCashOutSlot(); // the figure and its field leave together
        }

        /// <summary>§6.1's "brief L4 punch, then … at L3", as its own arc.
        ///
        /// <para>The accept beat owns the token across this window — ApplyCashOutSlotState stands
        /// aside while `accepted`, because a per-frame re-derivation would either cancel the punch
        /// on the next Update or hold it for the whole beat. Releasing is what drops the field from
        /// the punch to L3, which is the state §6.1 actually names.</para>
        ///
        /// <para>Runs detached so it cannot re-pace the beat it decorates. ReleaseL4 is a no-op if
        /// something else has since taken the token, so a late release cannot steal a punch.</para></summary>
        private IEnumerator PunchThenSettle(HdrFocus focus)
        {
            RequestL4(focus, momentary: true);
            yield return ScaledWait(hdrPunchDuration);
            ReleaseL4(focus);
        }

        // T40 (batch 27): `FloodPulse` is removed with the two floods it animated — it had no other
        // caller. Its sine envelope is exactly what made the floods indefensible as a ground: an
        // element whose luminance sweeps 0 → peak → 0 cannot be something a money figure is read
        // against, which is what the accept-beat time series measured (ink 0.064 → 0.384 tracking
        // the flood's own 0.063 → 0.507, CR collapsing 6.47 → 1.70).

        // ---------------------------------------------------------------- input (Update)

        /// <summary>Frozen substitute for Time.deltaTime (TVS-H02): 0 while standing, the real
        /// per-frame delta while seated. Every timer/coroutine/animator this class owns reads dt
        /// through this gate (or through _seatedClock for Time.time reads) instead of Unity's clock
        /// directly, so one flag freezes all of them and sitting back down resumes with no catch-up.
        /// </summary>
        /// <summary>§4.4's freeze, and PRD §8.8's — ONE expression, so the stats panel inherits the
        /// whole freeze contract instead of re-implementing any of it.
        ///
        /// <para><b>TIME STOPS (Allen, 2026-08-15).</b> §8.8 enumerates what must hold while the
        /// panel is open — event cursor, scene step, ball, actors, clock, probability animation,
        /// <b>cash-out animation AND OFFER</b>, callout lifetime, resolution effect, transition and
        /// the pending-window timer. Standing already freezes every one of them and TVS-H02 pins it,
        /// so the panel is <b>one added term here</b> rather than a second freeze that would have to
        /// be kept in step with the first. The contradicting clause — "while the panel is open the
        /// cash-out offer keeps moving" — was STRUCK from the PRD in the same pass: the panel cannot
        /// be used to buy thinking time on a money decision, because the offer is frozen too.</para>
        ///
        /// <para>Two freezes agreeing by convention is the defect T95 caught one surface over. This
        /// is the same remedy: one authority, so they cannot disagree.</para></summary>
        /// <para><b>T99's STANDING CONDITION (batch 79) — AND THIS IS THE LINE IT GOVERNS.</b> The
        /// stats panel is permitted to cover the SCOREBUG band <b>for as long as time is frozen while
        /// it is open</b>. A covered fact that cannot move is deferred; a covered fact that CAN move
        /// is lost. <b>If the match is ever allowed to run behind this panel, the scorebug must
        /// survive the overlay.</b></para>
        ///
        /// <para>The DD wrote it as a standing condition rather than a one-time approval because the
        /// danger is a later change that looks unrelated — <i>"let the match play while he reads the
        /// stats"</i> is a plausible improvement that would silently void the ruling. <b>Deleting
        /// <c>!_statsOpen</c> from this expression IS that change.</b> It is written here because
        /// here is where it would be made.</para>
        ///
        /// <para>And the licence is the freeze ALONE. The panel's GOALS row is not the justification:
        /// a statistic is not a result — the scorebug's score is the match's standing, where a GOALS
        /// row is one measure among its siblings. Arguing from the row would make the panel a
        /// REPLACEMENT for the scorebug, and a replacement owes the score in its own form, the clock,
        /// and T38's single-frame change. The panel does none of that and is not asked to. Nothing is
        /// lost because the match is not moving, not because the score is printed twice.</para></summary>
        private float SeatedDeltaTime => _seated && !_statsOpen ? Time.deltaTime : 0f;

        private Leg _statsLeg;

        /// <summary>§8.8's open/close. Opening renders first and raises the chrome after, so the
        /// panel never appears for a frame carrying the previous leg's numbers.</summary>
        private void SetStatsPanel(bool open)
        {
            if (_statsPanel == null || _statsOpen == open) return;
            _statsOpen = open;
            _statsPanel.gameObject.SetActive(open);
            if (!open) return;
            RenderStatsPanel();
            RaiseStatsChrome();
        }

        /// <summary>THE COLLISION RULING, BUILT RATHER THAN ARRANGED (Allen, 2026-08-15): the panel
        /// may open during a pending-intervention window, and the intervention overlay stays on top
        /// with the pending decision never out of sight.
        ///
        /// <para>Done by explicit sibling order at the moment of showing, NOT by relying on the order
        /// the two happen to be built in. Build order is a convention, and two elements agreeing by
        /// convention is the defect T95 caught — there, a ruling moved one box and its mirror was
        /// never re-derived. Here the panel is raised above the column and the stage, and then the
        /// prompt is raised above the panel, every time either is shown.</para></summary>
        private void RaiseStatsChrome()
        {
            if (!_statsOpen || _statsPanel == null) return;
            _statsPanel.SetAsLastSibling();
            if (_tInterventionPrompt != null && _tInterventionPrompt.enabled)
                _tInterventionPrompt.transform.SetAsLastSibling();
        }

        /// <summary>DD batch 93 item 1: derives the stats panel's ROW SET from the TICKET'S LEGS —
        /// GOALS is unconditional (the match score is always sourced); CORNERS is present only if
        /// <c>_ticket.Legs</c> carries a TotalCorners leg; CARDS only if it carries a TotalCards leg.
        ///
        /// <para>Called exactly ONCE, at the same site <c>_ticket</c> itself is adopted
        /// (<c>PresentRound</c>) — never per leg, never per beat. A table whose rows appear and
        /// vanish as legs go live is the defect this replaces, not a variant of it, so
        /// <see cref="RenderStatsPanel"/> reads only the two stored flags this method writes and
        /// never re-derives them from <c>Ticket.Legs</c> or the live leg.</para>
        ///
        /// <para><b>DD batch 95:</b> this is also the ONE site the panel's row COUNT becomes known,
        /// so it is the one site <see cref="ResizeStatsPanel"/> is called from — "the panel's height
        /// must be set when the row set is known", never before (no ticket to know it from) and never
        /// per-render (the row set does not change without a new ticket).</para></summary>
        private void ComputeStatsRowSet()
        {
            _statsRowHasCorners = false;
            _statsRowHasCards = false;
            if (_ticket != null)
            {
                foreach (Leg leg in _ticket.Legs)
                {
                    if (leg.Selection.Kind == MarketKind.TotalCorners) _statsRowHasCorners = true;
                    else if (leg.Selection.Kind == MarketKind.TotalCards) _statsRowHasCards = true;
                }
            }
            ResizeStatsPanel();
        }

        /// <summary>DD batch 95: "AN UNBOUGHT ROW IS NOT A SILENT ROW, IT IS NO ROW." Applies the row
        /// set <see cref="ComputeStatsRowSet"/> just computed to the PANEL ITSELF — resizes <see
        /// cref="_statsPanel"/>'s rect to <see cref="StatsPanelHeight"/> of <see
        /// cref="StatsActiveRowCount"/>, and for every physical row slot either:
        /// <list type="bullet">
        /// <item>ACTIVATES it at its rank's own <see cref="StatsRowY"/>, if its rank is within the
        /// active count — so a row's PHYSICAL box always sits directly under the row before it,
        /// contiguous, regardless of which specific kind (CORNERS/CARDS) that earlier row is; or</item>
        /// <item>DEACTIVATES it AND collapses its y onto the last active row's own y, if not.</item>
        /// </list>
        ///
        /// <para>The collapse (not just the deactivate) is load-bearing, not decoration: <c>Stats_
        /// panel_is_sized_exactly_to_its_content</c> discovers row slots via <c>GetComponentsInChildren
        /// &lt;RectTransform&gt;(true)</c> — <c>true</c> meaning it still SEES inactive slots — so a
        /// slot merely deactivated IN PLACE at its old build-time position would still be measured at
        /// its old, further-down y and silently re-inflate the panel's computed content bounds back
        /// out to the fixed 3-row extent this batch removes. Collapsing an inactive slot onto the last
        /// active row's own y makes it measure as contributing nothing beyond that row, which is the
        /// geometric expression of "does not exist" this pin can actually see.</para>
        ///
        /// <para>Idempotent and re-run-safe: a later ticket with a DIFFERENT row set (fewer or more
        /// rows) simply recomputes every slot's active state and y from scratch, so nothing carries
        /// over from a previous ticket's shape.</para></summary>
        private void ResizeStatsPanel()
        {
            if (_statsPanel == null || _tStatsLabel == null) return;

            int activeRows = StatsActiveRowCount;

            Vector2 size = _statsPanel.sizeDelta;
            size.y = StatsPanelHeight(activeRows);
            _statsPanel.sizeDelta = size;

            float collapseY = StatsRowY(activeRows - 1); // the last ACTIVE row's own y
            for (int i = 0; i < StatsRowSlots; i++)
            {
                bool active = i < activeRows;
                float y = active ? StatsRowY(i) : collapseY;
                CollapseStatsSlot(_tStatsLabel[i], active, y);
                CollapseStatsSlot(_tStatsA[i], active, y);
                CollapseStatsSlot(_tStatsB[i], active, y);
            }
        }

        private static void CollapseStatsSlot(TMP_Text slot, bool active, float y)
        {
            slot.gameObject.SetActive(active);
            Vector2 pos = slot.rectTransform.anchoredPosition;
            pos.y = y;
            slot.rectTransform.anchoredPosition = pos;
        }

        /// <summary>§8.8's shipped rows, all REVEALED-LEDGER values.
        ///
        /// <para><b>The leak this panel exists to avoid is one property away.</b> `CountLedger`
        /// exposes `TargetHome`/`TargetAway`/`TargetTotal` — the LOCKED endpoint, the match's true
        /// final count — right beside `Home`/`Away`, the revealed running totals. This (through
        /// <see cref="RenderStatsCountRow"/> and <see cref="_statsRetainedCounts"/>) reads
        /// `Home`/`Away` and must never read the other three: §8.8 calls a leak here a blocker, not a
        /// polish item, and the two pairs are one character apart at the call site.</para>
        ///
        /// <para><b>DD batch 93: CORNERS/CARDS no longer key to the LIVE leg.</b> Presence keys to
        /// <see cref="_statsRowHasCorners"/>/<see cref="_statsRowHasCards"/> — the ticket's own,
        /// frozen-at-placement row set (<see cref="ComputeStatsRowSet"/>) — and value keys to
        /// <see cref="_statsRetainedCounts"/>, so a count revealed earlier in the ticket stays
        /// revealed after its leg stops being live. A row the ticket bought but has not yet revealed
        /// carries <see cref="StatsUnrevealed"/> — the gap is deliberately VISIBLE rather than hidden
        /// (Allen, 2026-08-15), and the ruled row grammar keeps.</para>
        ///
        /// <para><b>DD batch 95: a row ABSENT from the ticket's row set is never written here at
        /// all.</b> Slot indices are assigned CONTIGUOUSLY, by presence-rank in this fixed priority
        /// order (GOALS, then CORNERS if present, then CARDS if present) — never at the kind's own
        /// canonical index — so an absent row leaves no gap for a present one further down the
        /// priority order to fall into. <see cref="ResizeStatsPanel"/> (run the instant the row set
        /// itself becomes known, alongside <see cref="ComputeStatsRowSet"/>) activates exactly this
        /// many slots, in this same order, so the two can never disagree about which physical slot a
        /// given row lands on.</para></summary>
        private void RenderStatsPanel()
        {
            if (!_statsOpen || _statsPanel == null || _statsLeg == null) return;

            Matchup m = _statsLeg.Matchup;
            // DD batch 94 item 1: "MATCH STATS" overstated the subject once the panel became
            // ticket-keyed (DD batch 93) — the surface's own word is COUNTS.
            _tStatsTitle.text = "COUNTS";

            // COLUMN ORDER IS THE SCOREBUG'S, NOT THIS METHOD'S OWN TO CHOOSE. The scorebug composes
            // AWAY on the left, HOME on the right (TvSweatScreen.cs ~2404:
            // `_tMatchup.text = $"{awayMark}{away}  {awayScore} — {homeScore}  {home}{homeMark}"`).
            // So column A (colA, the left value column built in BuildStatsPanel) is AWAY and column
            // B (colB, right) is HOME — on EVERY row below, headers and values together. Swapping one
            // without the other prints the right club names over the wrong numbers, a state lie that
            // is worse than the mismatched order it would replace.
            _tStatsTeamA.text = SweatFlavor.Short(m.Away.Name);
            _tStatsTeamB.text = SweatFlavor.Short(m.Home.Name);

            // The score ledger counts PICKED/OPPONENT; the count ledger counts HOME/AWAY. The panel
            // is about the MATCH, so home/away is the honest axis and the goals are mapped onto it
            // through the anchor both surfaces already share.
            bool pickedHome = SweatFlavor.PickedHomeForPresentation(_statsLeg);
            int goalsHome = pickedHome ? _ledger.Picked : _ledger.Opponent;
            int goalsAway = pickedHome ? _ledger.Opponent : _ledger.Picked;
            int slot = 0;
            SetStatsRow(slot++, "GOALS", goalsAway.ToString(), goalsHome.ToString());

            // DD batch 93 items 1-3: presence keys to the STORED, ticket-derived set; value keys to
            // the RETAINED store — neither ever reads the live leg's own kind. DD batch 95: the slot
            // index is a RANK (this row's position among the rows actually present), not a fixed
            // constant — see the type doc above.
            if (_statsRowHasCorners) RenderStatsCountRow(slot++, "CORNERS", MarketKind.TotalCorners);
            if (_statsRowHasCards) RenderStatsCountRow(slot++, "CARDS", MarketKind.TotalCards);
        }

        /// <summary>DD batch 93 items 1-3, one count row — DD batch 95 dropped the <c>inTicket</c>
        /// check: the caller (<see cref="RenderStatsPanel"/>) now only ever calls this for a row that
        /// IS in the ticket's row set, so there is no blank branch to write. Not yet in <see
        /// cref="_statsRetainedCounts"/> -> <see cref="StatsUnrevealed"/>. Revealed (this leg or an
        /// earlier one) -> the RETAINED Home/Away, so a count revealed while its leg was live does not
        /// un-reveal itself once a later leg goes live.</summary>
        private void RenderStatsCountRow(int i, string label, MarketKind kind)
        {
            if (_statsRetainedCounts.TryGetValue(kind, out (int Home, int Away) revealed))
                SetStatsRow(i, label, revealed.Away.ToString(), revealed.Home.ToString());
            else
                SetStatsRow(i, label, StatsUnrevealed, StatsUnrevealed);
        }

        private void SetStatsRow(int i, string label, string a, string b)
        {
            if (_tStatsLabel == null || i >= _tStatsLabel.Length) return;
            _tStatsLabel[i].text = label;
            _tStatsA[i].text = a;
            _tStatsB[i].text = b;
        }

        private void Update()
        {
            // §8.8's verb, and the ONLY new mid-sweat verb §3 authorises. Its own key, so it can
            // neither swallow nor be swallowed by the cash-out or stand controls — TVS-H01's shape,
            // satisfied by construction rather than by a guard. Seated only: a panel opened from the
            // couch is the only place this verb exists.
            Keyboard statsKeys = Keyboard.current;
            if (_seated && statsKeys != null && statsKeys.tabKey.wasPressedThisFrame)
                SetStatsPanel(!_statsOpen);

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
                // T164: the bed reads _tensionProb, not RevealedView.WinProbability — that mirror now
                // carries the TICKET's number, and a parlay's product sits far from the coin-flip
                // peak, so this would have gone flat and stayed flat on every multi-leg ticket. This
                // preserves today's audible bed EXACTLY (it is the same picked-side number this line
                // read before T164 moved WinProbability to the ticket) and stays correct under N-live.
                _audio.SetTension(1f - Mathf.Abs(2f * _tensionProb - 1f), _audioUrgency);
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

            ResolveCashOutGesture();
        }

        /// <summary>T22/T36's confirm gesture on §6.1's money control, wired.
        ///
        /// <para><b>What was actually wrong, because it was not a missing feature.</b> §8.10's
        /// hold-to-preview was BUILT — <see cref="EnterCashOutPreview"/>, its full-revert twin, the
        /// previewed bank, the stepped-down rows, all of it render-aware and pinned by EditMode — and
        /// it had <b>no production call site</b>. The only thing that had ever called it was a test,
        /// by reflection. Meanwhile this method's predecessor was one line,
        /// <c>if (_interact.WasPressedThisFrame()) TryCashOut()</c>, so the surface printed
        /// <c>HOLD E</c>, implemented a preview nobody could reach, and committed the money on the
        /// first frame of the press. The gesture is not new here; it is CONNECTED here.</para>
        ///
        /// <para><b>The asset's own Hold is deliberately not used.</b> <c>Interact</c> carries
        /// <c>"interactions": "Hold"</c>, and the Input System documents <c>WasPressedThisFrame</c> as
        /// true on the press "even if there is an interaction on the action that has not yet
        /// performed" — which is exactly how a declared hold went unobserved. Honouring it would be
        /// the other wrong repair: a HoldInteraction performs on a DURATION, and T22/T36 rule "no
        /// timer, no auto-commit". So the hold is read as a STATE (<c>IsPressed</c>) and the commit
        /// comes from a key, never from elapsed time. Nothing in the shared asset is edited.</para></summary>
        private void ResolveCashOutGesture()
        {
            if (_interact == null) return;
            bool held = _interact.IsPressed();

            // Entry refuses on its own gate, so holding E over a suspended or mid-tween slot previews
            // nothing — TVS-H01's predicate, unchanged, and the reason the previewed number and the
            // acceptable number cannot be different numbers.
            if (held && !_cashOutPreview) EnterCashOutPreview();
            if (!_cashOutPreview) return;

            // Release ABANDONS — always, per T22, and it is the same full revert a stand performs.
            // So does the offer going away underneath the hold: a suspension, a new price starting to
            // tween, or the session settling all make the previewed offer unacceptable, and holding a
            // preview of an offer that can no longer be taken is the display promising input the gate
            // would refuse (T59).
            if (!held || !CanAcceptCashOutNow())
            {
                ExitCashOutPreview();
                return;
            }

            if (!ConfirmPressed()) return;

            // §8.10's invariant, enforced rather than reasoned: "the previewed and accepted numbers
            // can never differ." They cannot drift while the gate above holds, but the guard costs
            // nothing and turns an argument into a check — and if they ever DO differ, abandoning is
            // the ruled outcome, since committing would hand the player a price the display was not
            // showing, which T59 names the worst available outcome on a money control.
            double previewed = _cashOutPreviewAmount;
            double? offerNow = _session.CashOutOffer();
            ExitCashOutPreview();
            if (offerNow.HasValue && offerNow.Value == previewed) TryCashOut();
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
            // T64: the 9 Hz Perlin flicker that used to multiply `e` here is struck and removed. The
            // quad's emission is now exactly the state it is in — a decade-old panel that works.
            _emissBlock.SetColor(EmissionColorId, e);
            emissiveScreen.SetPropertyBlock(_emissBlock);
        }

        private void EmissionFlash(Color color)
        {
            _emissFlash = color;
            _emissFlash01 = 1f;
        }

        /// <summary>T65: the ONE room re-tint. Every site that warms the room calls this and no
        /// site passes a colour, so the room's palette is enforced by construction rather than by
        /// five sites agreeing.
        ///
        /// <para>Fires on SETTLEMENT only — the ticket paying out, the money being taken, the run's
        /// verdict. Never on a leg. A leg win is not a payoff: there are three or four per ticket,
        /// and a room that floods on each of them has no register left for the one that pays.</para>
        ///
        /// <para>Transient by design. It eases back to whatever rest mood the room is in, so the
        /// re-tint is a reaction shot and never a new resting state — the one exception is the
        /// RunWon verdict card, which is a persistent screen and holds its own dim rest.</para></summary>
        private void RoomSettlementGlow()
        {
            tvLight?.Flash(roomSettlementWarm, roomSettlementIntensity);
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
            // `live` drives the STATUS WORD, which is why it still tests the flag directly: a
            // suspended slot carries no status word at all (TV-12/13), independently of whether the
            // engine would accept a key. Since T59 the accept gate reads _cashOutSlotSuspended too,
            // so for `fieldLit` the flag is now belt-and-braces rather than load-bearing — kept
            // because the field and the key are supposed to be the same promise, and a future edit
            // to either predicate should not be able to separate them silently.
            // T68-am: `accepted` is §6.1's sixth state and it reuses this same slot. It is NOT live —
            // the offer is over, so no status word and no accept gate — but the field IS lit, because
            // the money figure has to inherit the inversion rather than sit on a moving flood.
            bool accepted = slotVisible && _cashOutAccepted;
            bool live = slotVisible && !_cashOutSlotSuspended && !accepted;
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
            bool fieldLit = accepted || (live && !_cashOutTweening && CanAcceptCashOutNow());
            if (_cashOutField != null) _cashOutField.enabled = fieldLit;
            _cashOutFieldLit = fieldLit;

            // ---- T68: THE INVERSION IS TWO PARTS, AND ONLY ONE WAS BUILT --------------------
            //
            // §6.1 specifies actionable as "gold at L4, inverted field, dark type punched out".
            // The field inverted; the type kept its light ink and the field rose to meet it.
            // Measured at the acceptance view: field 0.807, `HOLD E` 0.793 — a contrast ratio of
            // **1.02:1**. The money control had no readable label. Against `goldInk` it is 15.3:1.
            //
            // It predates T63 (pre-fix 0.696 vs 0.827 is 1.17:1, already failing) and no ladder
            // work could have caught it: every T63 measurement compared this band to OTHER
            // elements — scoreline, ball, column, strip. Nothing measured it against its own ink.
            // A dominance gate and a legibility gate are different instruments (C33-am2).
            //
            // The ink is derived HERE, with the field, from the same predicate — because the two
            // halves of one inversion must not be separable by a future edit. That is the same
            // rule T43 applied to the slate and T66 to the event strip: one authority, and no
            // call site gets to choose. Three states, and the call sites now set none of them.
            if (_tCashOut != null)
            {
                _tCashOut.color = (_cashOutSlotSuspended && !accepted)
                    ? structureGrey                                   // §8.5 suspended: L1 unlit slate
                    : fieldLit
                        ? goldInk                                     // punched out of the lit field
                        : new Color(gold.r, gold.g, gold.b, 1f);      // unlit states keep the light ink
            }
            if (_tCashOutStatus != null)
            {
                // `HOLD E` is the confirm-gesture copy T22/T36 ruled. On the lit field it inverts
                // with the amount; unlit it keeps the L2 label grey it already had correctly.
                _tCashOutStatus.color = fieldLit ? goldInk : AtTier(contextGrey, TierL2);
            }
            // TV-12/13: suspended owns the slot exclusively. The status word is the offer speaking,
            // so it is absent whenever the offer is not — never merely dimmed (C10).
            if (_tCashOutStatus != null) _tCashOutStatus.enabled = live;
            // §8.5: the slot's brightness is a promise about input — L4 only while a press would
            // actually be accepted right now (same predicate as the accept gate itself, so this can
            // never promise more than TryCashOut will honor). Suspended and mid-tween stay LDR.
            // C3 rule 5: the boost is a single value (1.4 since T49) — no second, per-element scale on top of it
            // (the old taunt-flash lerp up to HdrBoostL4 * 1.15 is retired). CashOut's request is
            // SUSTAINED: it re-asks every frame while actionable, and yields the instant a momentary
            // punch (a goal's score, a payoff's ball, a win/cash-out tally) takes the token instead.
            // T68-am: while `accepted`, the BEAT owns the token — it fires §6.1's brief L4 punch and
            // then releases to L3, and a per-frame re-derivation here would either cancel the punch
            // on the next Update or hold it for the whole beat. The accept beat is the only state
            // whose brightness is a scripted arc rather than a standing promise about input.
            if (_cashOutHdrMat != null && !accepted)
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
            // T68: `!_cashOutFieldLit` is the fourth term and it is load-bearing. This taunt is the
            // per-frame gold repaint T43 already caught once; on an INVERTED field it would paint
            // the amount gold on gold every frame and undo the punch-out on the very next Update.
            //
            // Not repainting is also correct rather than merely safe: `goldInk`'s own declaration
            // says "brightness is a promise about input" is carried by the FIELD, not the letters.
            // While the field is lit the field is the state; the letters are static and dark, and
            // the flash rides the scale it already animates above.
            if (_tCashOut.enabled && !_marketSuspended && !_cashOutSlotSuspended && !_cashOutFieldLit)
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

        /// <summary>Whether this beat belongs to the LAST telling on the ticket — what
        /// <see cref="PacingFor"/>'s final-leg slowdown keys on.
        ///
        /// <para>WAS <c>evt.LegIndex == _ticket.Legs.Count - 1</c>, AND THAT CANNOT FIRE ON AN
        /// INTERLEAVED TICKET. After <c>T140</c> arm A a telling is a (ticket, FIXTURE) and
        /// <c>DramaEvent.LegIndex</c> is its ANCHOR — the lowest ticket-order leg on that fixture.
        /// Fixture grouping is first-appearance (<c>JointModel.GroupByMatchup</c>) and a fixture's
        /// legs need not be contiguous, so on <c>[matchA, matchB, matchA]</c> the anchors are only
        /// ever 0 and 1 — never 2, the last leg index. The ticket's closing telling then paces like
        /// any other beat.</para>
        ///
        /// <para><b>Scope, stated so it is not read as larger than it is:</b> this is the theaterless
        /// fallback's pacing only. On the shipping theater path <c>TheaterBeat</c> owns pacing and
        /// <see cref="PacingFor"/> is never called. The console's twin gates a stated RULE (no
        /// fast-forward through the final match); this one does not.</para>
        ///
        /// <para>PUBLIC because a gate asserts it, for the same reason <c>SweatLines</c> is public in
        /// the console: the value is computed inside a coroutine that sleeps, polls seating and drives
        /// a scene, so a test that could only reach it by driving that loop is a test that cannot
        /// run.</para></summary>
        public static bool OnFinalFixture(DramaEvent e, SweatSession session)
            => e.FixtureIndex == session.FixtureCount - 1;

        /// <summary>The pacing table (ported from SweatRenderer.PacingFor): base delay by tension tag, an
        /// extra beat right before a leg's final whistle, and everything slowed on the ticket's final
        /// TELLING (<c>T140</c> arm A — see <see cref="OnFinalFixture"/>).</summary>
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
            BuildStatsPanel(root, grid);
            BuildScoreBug(root, grid);
            BuildEventStrip(root, grid);
            BuildCashOutZone(root, grid);
            BuildChromeStrip(root, grid);

            // --- attract state (before the sweat is live) ---
            _tAttract = MakeText(root, "Attract", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(w - 60f, 130f), TypeAttract,
                TextAnchor.MiddleCenter, flavorColor, FontStyle.Bold); // §4 Fact: cold white, not money
            _tAttract.text = "SIT TO WATCH THE SWEAT";

            // Ticket-card / settle-card takeover copy (PRD §8.9): the stage goes quiet during these
            // transitions, but the ticket column never clears (DESIGN.md §6: "does not resize
            // between markets"). Sits inside the fixed Stage zone rather than floating over the
            // whole canvas, so it never competes with the ticket rail.
            _tTakeoverTitle = MakeText(root, "TakeoverTitle", new Vector2(0f, 1f), new Vector2(0.5f, 0.5f),
                AnchorCenter(grid.Stage) + new Vector2(0f, 40f), new Vector2(grid.Stage.width - 60f, 60f), TypeTakeoverTitle,
                TextAnchor.MiddleCenter, flavorColor, FontStyle.Bold);
            _tTakeoverSub = MakeText(root, "TakeoverSub", new Vector2(0f, 1f), new Vector2(0.5f, 0.5f),
                // T92-am: the slot widens to hold its LONGEST RENDERABLE FORM. With the leg list gone
                // the widest string is the deferral line — `PAYMENT DEFERRED — YOUR BANK STANDS. THE
                // NEXT ONE GROWS BY $1,200` at 665.9px — which overran the old 655.0px box by 10.9.
                // Refused in the ruling and not attempted here: trimming the copy (§4/T24-am),
                // abbreviating the $1,200 (C49 — money the player can lose), and shrinking the type
                // (§8 — copy never shrinks). 20px of the panel's 30px side margin buys it.
                AnchorCenter(grid.Stage) + new Vector2(0f, -20f), new Vector2(grid.Stage.width - 20f, 60f), TypeTakeoverSub,
                TextAnchor.MiddleCenter, contextGrey);

            // Subtitle line reused ONLY by the idle/run-over screens (non-sweat states); never
            // shown during a live sweat — DESIGN.md §7's component list has no standalone win%/
            // subtitle slot for the live grid.
            _tSubtitle = MakeText(root, "Subtitle", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -60f), new Vector2(w - 120f, 34f), TypeSubtitle, TextAnchor.MiddleCenter, flavorColor);

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
            // T8 (Allen, 2026-07-31): the StaticNoise overlay is REMOVED — DESIGN.md §2 bans
            // interference noise by name. Nothing replaces it; loss is darkness, which DimOverlay
            // below already provides.
            // Black floor (unified-grade-spec.md §2): even the "everything just went dark" overlay
            // must not sit below the room's deepest shadow, so its RGB matches the same floor as
            // screenBg/barBgColor/pitchBgColor rather than true (0,0,0). Only alpha animates.
            _dimOverlay = MakeStretchImage(root, "DimOverlay", new Color(0.048f, 0.055f, 0.068f, 0f));
            // T40 (batch 27): GoldFlood and WonFlood were built here. Both are struck. Nothing
            // replaces them — the payoff is carried by the slot treatment alone, and §6.1's brief
            // L4 punch is measured doing the punctuation the flood was assumed to add
            // (0.688 → 0.586 at the settle). The flood was redundant with the punch, not carrying it.
            _tBigAmount = MakeText(root, "BigAmount", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(w - 40f, 200f), TypeBigAmount,
                TextAnchor.MiddleCenter, new Color(gold.r, gold.g, gold.b, 1f), FontStyle.Bold);
            _tBigAmount.text = string.Empty;
            _bigAmountHdrMat = MakeHdrMaterial();
            if (_bigAmountHdrMat != null) _tBigAmount.material = _bigAmountHdrMat;

            // The bad-beat consolation line — built ABOVE the dim overlay so the sting stays
            // readable through the 94% dim (Sol, M-T4); neutral chrome, never money-red.
            _tConsolation = MakeText(root, "Consolation", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -20f), new Vector2(w - 80f, 44f), TypeConsolation,
                // T77 (batch 32): the synthesised italic goes and the slot drops to regular. There is
                // no italic anywhere in Encode Sans — its axes are weight and width only, so this was
                // a shear applied to an upright face, not a face. The only styled slot on the surface
                // with nothing real behind it.
                TextAnchor.MiddleCenter, flavorColor);
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
            "RiskPays",         // C8: joins the protected set — now the RISK half (see BuildTicketColumn)
            "Pays",             // T74-am5: the right-anchored half, same class, same protection
        };

        private TMP_Text _tStatsTeamA, _tStatsTeamB;

        /// <summary>PRD §8.8's match stats panel — built once, hidden, and shown by
        /// <see cref="SetStatsPanel"/>. §6 forbids geometry derived from content, so every rect here
        /// is fixed and read from the grid.
        ///
        /// <para><b>THE RECT IS WHY §8.8's HARD PROHIBITION CANNOT BE BREACHED.</b> The panel spans
        /// the ticket column and the stage, from the column's HEAD (y = 0 — that is what "opens from
        /// the head of the ticket column" means) down to <c>bottomY</c>. `CashOut` is
        /// <c>Rect(0, bottomY, ticketW, BottomRowHeight)</c>: it BEGINS exactly where this ENDS. So
        /// *"the panel may not obscure the cash-out state"* holds <b>by construction, not by
        /// care</b> — and because both rects are read from the same <see cref="LayoutGrid"/>, a
        /// future grid change moves them together instead of silently bringing them into
        /// contact.</para>
        ///
        /// <para><b>It does cover the SCOREBUG band</b>, which DESIGN's "over the ticket column and
        /// stage" does not name — the two zones cannot be spanned by one rect without it. Flagged
        /// rather than decided: time is frozen and the panel's own GOALS row carries the score, so
        /// nothing is lost, but whether the scorebug should survive the overlay is the DD's.</para>
        ///
        /// <para>The ground is <see cref="screenBg"/> at full alpha. The token is the surface's own
        /// ground and is unchanged in hue; only its 0.86 alpha is dropped, because a stats panel that
        /// lets the frozen pitch through is not readable. No new colour is introduced.</para></summary>
        private void BuildStatsPanel(Transform root, LayoutGrid grid)
        {
            const float pad = StatsPad; // the ONE spacing value — see StatsPad's own doc

            // CONTENT-FIT SIZING (DD batch 87 + Allen): "a surface that takes the entire stage and
            // returns three rows hasn't earned the stage." T102 (DD batch 89) RE-RULED the box rule:
            // no longer "widest measured ink (C46, Evidence_C46_the_stats_panel_strings_against_
            // their_boxes) + a margin" — that `contentMargin` cut was UNRATIFIED and is superseded,
            // not kept as a dead constant — but THE WIDEST MEASURED INK MUST BE AT MOST
            // MaxInkFraction OF ITS BOX. labelW/valueW are DERIVED from MaxInkFraction, never
            // restated as independent literals, so a future ruling on the fraction moves one number,
            // not a copied derivation.
            //
            // DD batch 94 item 1: the title reads "COUNTS", not "MATCH STATS" — and the label column
            // holds FOUR strings at TWO type sizes (the title at TypeProgress/19px; the three row
            // labels GOALS/CORNERS/CARDS at TypeEyebrow/15px), so a shorter title does NOT
            // automatically shrink the column — a row label could be the widest thing in it. Measured
            // live off the built components (C46, Evidence_C46_the_stats_panel_strings_against_
            // their_boxes, [C46-PANEL] log): COUNTS 88.5px, CORNERS 81.2px, GOALS/CARDS 56.6px each —
            // the title still binds, but by only 7.3px over CORNERS, so this MUST be re-measured
            // (never assumed) the next time either string set changes.
            //   label column widest ink: "COUNTS" 88.5px (title, TypeProgress/19px) -> labelW =
            //     ceil(88.5 / MaxInkFraction)
            //   value column widest ink: "Spreadsheets" 115.3px -> valueW = ceil(115.3 / MaxInkFraction)
            // S84: that 115.3px value-column figure is only honest while it stays the widest ink the
            // engine's CLOSED CLUB POOL can produce, not merely a sampled string — guarded on every
            // routine run by Stats_panel_value_column_holds_the_full_club_pool_at_max_ink_fraction
            // (TvSweatScreenTests.cs), which fails the day the pool outgrows this box.
            float labelW = Mathf.Ceil(88.5f / MaxInkFraction); // 111
            float valueW = Mathf.Ceil(115.3f / MaxInkFraction); // 145

            // pad is the ONLY spacing value on this panel: left inset, both inter-column gaps, right
            // inset, and (below) the bottom inset. colA/colB are RE-DERIVED from labelW/valueW/pad —
            // they must move whenever the boxes do, never sit as fixed pixels left over from a wider
            // panel. That was this method's exact bug before this pass: colA/colB were fixed at
            // 450.8/666.4, correct only for the old 980-wide full-stage panel and outside the bounds
            // of this narrower, content-fit one.
            float colA = pad + labelW + pad;                         // 175
            float colB = colA + valueW + pad;                        // 352
            float panelW = colB + valueW + pad;                      // 529

            // Vertical rhythm is UNCHANGED (title at -pad, rows at -(pad+56+i*46), rows 34 tall).
            // DD batch 95: the panel's own HEIGHT is no longer fixed here — the row SET (and so the
            // row COUNT) is not known until ComputeStatsRowSet runs, at ticket adoption, so a fixed
            // 3-row constant can no longer live at canvas-build time, before any ticket exists. This
            // builds with a minimal 1-row (GOALS-only) placeholder; ResizeStatsPanel (called at the
            // end of this method, and again every time ComputeStatsRowSet runs) is the ONE place that
            // sets the panel's REAL height, from the REAL row count — the panel starts inactive
            // (bottom of this method) and nothing can observe it before a ticket exists, so the
            // placeholder itself is never shown.
            float panelH = StatsPanelHeight(1);

            // DD batch 87 + Allen, option (B): the panel's TOP drops BELOW the scorebug band so the
            // two zones never share a pixel on either axis, instead of narrowing just enough to dodge
            // it — a half-covered scorebug would be worse than a fully covered one. x stays 0 so the
            // panel still sits against the ticket column's side. Verified arithmetically against
            // grid.TicketColumn.height (bottomY, where CashOut/EventStrip begin): panel bottom
            // ScoreBugHeight+panelH = 62+246 = 308 stays clear of bottomY (480 at the 980x550
            // reference canvas) by 172px — the rhythm did not need to shrink to fit. DD batch 95: 246
            // (StatsPanelHeight(StatsRowSlots), all three rows bought) is now the MAXIMUM panelH can
            // ever be, not its fixed value, so this clearance argument still holds against every
            // shorter, real ticket-derived height a session can ever build.
            var area = new Rect(0f, ScoreBugHeight, panelW, panelH);
            _statsPanel = MakePanel(root, "StatsPanel", new Vector2(0f, 1f), new Vector2(0f, 1f),
                AnchorTopLeft(area), new Vector2(area.width, area.height),
                new Color(screenBg.r, screenBg.g, screenBg.b, 1f)).rectTransform;

            _tStatsTitle = MakeText(_statsPanel, "StatsTitle", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(pad, -pad), new Vector2(labelW, 34f), TypeProgress,
                TextAnchor.UpperLeft, flavorColor, tracking: TvTrack.Label);

            // The two team columns carry the hues, which is DESIGN's "per-team rows use team hues"
            // expressed on the axis this composition actually has. T2's muted pair, unchanged.
            _tStatsTeamA = MakeText(_statsPanel, "StatsTeamA", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(colA, -pad), new Vector2(valueW, 34f), TypeEyebrow,
                TextAnchor.UpperLeft, teamHueA, tracking: TvTrack.Label);
            _tStatsTeamB = MakeText(_statsPanel, "StatsTeamB", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(colB, -pad), new Vector2(valueW, 34f), TypeEyebrow,
                TextAnchor.UpperLeft, teamHueB, tracking: TvTrack.Label);

            _tStatsLabel = new TMP_Text[StatsRowSlots];
            _tStatsA = new TMP_Text[StatsRowSlots];
            _tStatsB = new TMP_Text[StatsRowSlots];
            for (int i = 0; i < StatsRowSlots; i++)
            {
                float y = -(pad + 56f + i * 46f);
                _tStatsLabel[i] = MakeText(_statsPanel, $"StatsLabel{i}", new Vector2(0f, 1f),
                    new Vector2(0f, 1f), new Vector2(pad, y), new Vector2(labelW, 34f), TypeEyebrow,
                    TextAnchor.UpperLeft, flavorColor, tracking: TvTrack.Label);
                _tStatsA[i] = MakeText(_statsPanel, $"StatsA{i}", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(colA, y), new Vector2(valueW, 34f), TypeProgress,
                    TextAnchor.UpperLeft, flavorColor);
                _tStatsB[i] = MakeText(_statsPanel, $"StatsB{i}", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(colB, y), new Vector2(valueW, 34f), TypeProgress,
                    TextAnchor.UpperLeft, flavorColor);
            }

            // DD batch 95: no ticket has been adopted yet (_statsRowHasCorners/_statsRowHasCards are
            // both still their default false), so this sizes the panel to its true 1-row minimum and
            // deactivates/collapses row slots 1-2 — the same call ComputeStatsRowSet makes once a real
            // ticket exists, reused here rather than a second copy of the same math.
            ResizeStatsPanel();

            _statsPanel.gameObject.SetActive(false);
        }

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
                TextAnchor.UpperLeft, structureGrey, tracking: TvTrack.Label); // tv.card.html:20

            // T91-cl (batch 158): LEG n/m MOVES HERE, out of the scorebug's top band (was built in
            // BuildScoreBug, left-aligned beside Matchup). T91 settled on this lane's own ink
            // measurement that the old band did not fit Leg, Matchup and Clock at current sizes:
            // Matchup cleared Clock by 31.3px, no collision, but Leg's ink COLLIDED with Matchup's
            // by 41.7px, y bands intersecting. THE 2px INK FLOOR — T90-am's, generalised to both
            // sides of the ticket column's edge by T91-am2 — IS A PROPERTY OF ANY TWO ELEMENTS
            // WHOSE INK SHARES A y-BAND, not of one particular seam, and honouring it on both sides
            // of the old band still left the scoreline 14.3px short (569.0px available against
            // 583.3px needed): a clearance rule alone could not discharge it, so something had to
            // move. WHICH element moves was deliberately NOT ruled — T100's precedent puts position
            // and edges with TV — but the DD recommended this one: the scoreline and the clock are
            // both match facts and already clear each other, while `LEG n/m` is ticket bookkeeping,
            // and the ticket column already carries a header of exactly that kind. So it lands
            // RIGHT-aligned beside `_tTicketHeader`, in the same tracking, so the two read as one
            // row of bookkeeping rather than two. Worth recording: the leg counter itself had never
            // been ruled before T91-cl — zero register rows — which is why every prior ruling about
            // the old band was about the two elements that DO have rows (`Matchup`, `Clock`). Name
            // kept as `Leg` (the T84 sweep addresses it by that name) and its pool strings are
            // unchanged — only where and how it is placed moved.
            _tLeg = MakeText(root, "Leg", new Vector2(0f, 1f), new Vector2(1f, 1f),
                AnchorTopRight(grid.TicketHeader, 8f, 4f), new Vector2(140f, Mathf.Ceil(TypeEyebrow * LineBox)),
                TypeEyebrow, TextAnchor.UpperRight, structureGrey, tracking: TvTrack.Label);

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
                // T84/T74 relief for the compact statement, sourced from the GAPS (8f -> 6f), which
                // widens stmtW by 4px inside an unchanged column. Span, not size, and not the copy.
                //
                // WHY NOT FROM THE PRICE, which the sweep shows with 5.8px spare: that spare is an
                // artefact of the string set, not a property of the slot. `OddsFormat.American`
                // returns `+{a}` for a rounded profit and nothing bounds `a` — a profit-boost relic
                // multiplies the odds outright — so the price column has no measurable ceiling to
                // lend from. Taking its "spare" would move a pre-existing risk onto a slot that
                // cannot be swept, which is the exact mistake this sweep was written to catch.
                //
                // The gaps carry no content, so 4px out of them cannot break a measurement. The
                // partition stays consistent: 6px still separates statement from price and price
                // from chip, and the column's outer width does not move (T46, R30).
                //
                // stmtW 143 -> 147 against a tabular-screened worst case of 144.8 — 2.2px of margin
                // where there were -1.8. The chip's own 6px overrun is untouched and unrelated: it
                // carries no digits, so the wiring cannot move it, and it holds the ship rather than
                // the wiring.
                // T90-am (batch 61): THE TICKET COLUMN'S SIDE PADDING IS A RULED VALUE — 8px nominal,
                // and NO element's ink comes within 2px of the column edge. It stopped being informal
                // the moment two independent fixes proposed to spend the same allowance on different
                // rows of one column, which is C46's disease exactly: an implicit contract nobody
                // wrote down, invalidated by whoever spends it last. Both fixes below size against
                // this floor, and a third consumer is ruled against it rather than discovering it is
                // gone.
                const float ColumnInkFloor = 2f;

                // T91-am: the state chip grows rightward TO THE FLOOR, NOT PAST IT — 38 → 44, right
                // edge at the floor. The lever was never the gap and never the price: right alignment
                // pins the price's ink to its own box edge, so the clearance did not move with the
                // price at all (−280 and +1200 both left 1.3px). What bled was `NEXT` at 42.7px
                // overrunning a 38.0px box LEFTWARD. At 44.0 it sits inside its own box and the
                // existing 6.0px box gap becomes 7.3px of real ink clearance — and this retires the
                // slot's own 4.7px overrun in the same move.
                const float chipW = 44f, priceW = 52f, gap = 6f;
                float stmtW = lineW - 38f - priceW - gap * 2f;   // the statement keeps its 147px span

                TMP_Text line = MakeText(root, $"LegRowLine{i}", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    AnchorTopLeft(row, 8f, 4f), new Vector2(stmtW, compactH), TypeEyebrow,
                    TextAnchor.UpperLeft, structureGrey, FontStyle.Normal, Face.Condensed,
                    FontWeight.Bold, TvTrack.Name); // TvLegRow.jsx:57-61
                line.enableWordWrapping = true; // so the statement clips, not sprawls (was Wrap)

                TMP_Text price = MakeText(root, $"LegRowPrice{i}", new Vector2(0f, 1f), new Vector2(1f, 1f),
                    AnchorTopLeft(row, 8f + stmtW + gap + priceW, 4f), new Vector2(priceW, compactH),
                    TypeEyebrow, TextAnchor.UpperRight, AtTier(contextGrey, TierL2),
                    FontStyle.Normal, Face.Condensed, tracking: TvTrack.Meta); // TvLegRow.jsx:62 — --tv-context, its own tier

                TMP_Text state = MakeText(root, $"LegRowState{i}", new Vector2(0f, 1f), new Vector2(1f, 1f),
                    AnchorTopLeft(row, grid.TicketColumn.width - ColumnInkFloor, 4f),
                    new Vector2(chipW, compactH), TypeEyebrow,
                    TextAnchor.UpperRight, structureGrey, tracking: TvTrack.Meta); // TvLegRow.jsx:27-31 — regular face, min 38px
                // Live form: the authored NEED statement, then the revealed progress beneath it.
                //
                // T90-am: 249 → 261, taking 6px of the column's 8px side padding on each side and
                // stopping at the ruled 2px ink floor. `ONE TEAM BLANKED` is 252.5px, so it renders
                // complete with 8.5px spare and the word-boundary backstop no longer fires on it. The
                // column's OUTER width does not move (T46, R30) — this spends padding, not span.
                TMP_Text need = MakeText(root, $"LegRowNeed{i}", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    AnchorTopLeft(row, ColumnInkFloor, 4f),
                    new Vector2(grid.TicketColumn.width - ColumnInkFloor * 2f, needH), TypeNeed,
                    TextAnchor.UpperLeft, flavorColor, FontStyle.Normal, Face.Condensed,
                    FontWeight.Bold, TvTrack.Name); // inherits TvLegRow.jsx:35, tracked per :78
                TMP_Text progress = MakeText(root, $"LegRowProgress{i}", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    AnchorTopLeft(row, 8f, 4f + needH), new Vector2(lineW, progressH), TypeProgress,
                    TextAnchor.UpperLeft, flavorColor, FontStyle.Normal, Face.Condensed,
                    tracking: TvTrack.Name); // TvLegRow.jsx:82 — the .02em literal
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
            // ⚠ T74-am5 (batch 59) STOOD HERE AND IS ITSELF WITHDRAWN, by T144 / T147-am. It read
            // "ONE ROW, BOTH ENDS ANCHORED. The two-row form is withdrawn," on this argument: the
            // concatenated `RISK $1,234     PAYS $12,340` measured 296.5 only because of an AUTHORED
            // five-space spacer, so anchoring the two facts to opposite edges retires the gap and
            // "the binding constraint stops being 296.5 and becomes RISK's ink + PAYS's ink."
            //
            // THE FIRST HALF WAS TRUE AND THE SECOND HALF WAS THE DEFECT. Retiring the gap did make
            // the two inks the constraint — and T74-am6 then MEASURED those inks: at bank $10,000,
            // RISK 138.4 + PAYS 239.7 = 378.1 against 249.0, over by 129.1; at typical values 270.6,
            // over by 21.6. `$1,234` staked paying `$12,340` is a plain 10x parlay, so the pair
            // collides AT ORDINARY VALUES. The device removed the spacer, not the collision, and the
            // form it withdrew is the only one that fits — which is why it came back seventy-one
            // batches later under a second ID (C22.1: T74-am6 governs, T144 cross-references).
            //
            // T82's reason for right-anchoring PAYS — that against the tabular set it grows leftward
            // in exact digit-width steps, so clearance is predictable rather than content-dependent
            // — was real, and it PURCHASED PREDICTABLE CLEARANCE IN A ROW THAT HAD NONE TO GIVE.
            // On its own row each fact has the whole 249.0px and nothing to be clear OF.
            //
            // THE NAME `RiskPays` IS KEPT for the left half, and deliberately: it is in the C8
            // protected set and a LayoutGrid test finds it by that name. Same reasoning the money
            // figure's own name was kept under — a rename here would be a second change riding a
            // composition fix.
            // T144 / T74-am6, RULED BY ALLEN AND BUILT AT T147-am: A ROW EACH. `RISK 138.4 + PAYS
            // 239.7 = 378.1 against 249.0` — each half fits ALONE and the PAIR does not, at ORDINARY
            // values, not a tail case. Separate rows is the only composition inside the locked column
            // (T46/R30) that carries the fact floor without abbreviating (C49), truncating (T69) or
            // reopening the copy (T24-am). On its own row each fact gets the whole 249.0px.
            //
            // BOTH LEFT-ANCHORED (T147-am2). T74-am5's opposite anchoring had exactly one job — to
            // retire the authored gap BETWEEN two facts sharing one row. On separate rows there is no
            // shared gap, so the device has no subject, and keeping it would leave a stagger nobody
            // chose. ⚠ The money control is a COUNTER-PRECEDENT, not a supporting one: `CashOut` is
            // MiddleLeft (:5459) and `CashOutStatus` MiddleRight (:5471) — it KEPT opposite anchors
            // when it split onto two rows. So this is an open choice with a precedent against it and
            // it goes to a frame: left/left ships, the opposite-anchor arm is shot beside it on E1,
            // and the SETTLED state is where the two diverge (`STAKE`/`RETURNED` are five and eight
            // characters; `RISK`/`PAYS` are both four and align either way).
            //
            // THE TOP INSET IS SPENT, DELIBERATELY. 60px holds exactly two 30.0px line boxes, so the
            // 8px the single row used above its ink is what the second row is made of. The air is
            // still there: the last leg row's ink now ends ~40px above the footer inside its own 99px
            // slot. If a frame says otherwise the footer can afford 68 (rows would go 99 -> 97) —
            // recorded so the next seat re-reads it rather than rediscovering it.
            //
            // THE NAMES `RiskPays` AND `Pays` ARE KEPT: both are in the C8 protected set, the T84
            // sweep's declaration table addresses them by name, and a rename here would be a second
            // change riding a composition fix.
            float footerRowH = grid.TicketFooter.height * 0.5f;      // 30.0 — one 24px line box at 1.25
            float footerBoxW = grid.TicketFooter.width - 16f;        // 249.0 — the full inner width, twice

            _tRiskPays = MakeText(root, "RiskPays", new Vector2(0f, 1f), new Vector2(0f, 1f),
                AnchorTopLeft(grid.TicketFooter, 8f, 0f),
                new Vector2(footerBoxW, footerRowH), TypeRisk,
                TextAnchor.UpperLeft, goldL2, FontStyle.Normal, Face.Condensed, FontWeight.Bold); // TvRiskPays.jsx:14

            _tPays = MakeText(root, "Pays", new Vector2(0f, 1f), new Vector2(0f, 1f),
                AnchorTopLeft(grid.TicketFooter, 8f, footerRowH),
                new Vector2(footerBoxW, footerRowH), TypeRisk,
                TextAnchor.UpperLeft, goldL2, FontStyle.Normal, Face.Condensed, FontWeight.Bold);
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
            // §7 Scorebug: "Ticket/leg index at L1, present but subordinate." — true of the FACT,
            // no longer true of this BAND. T91-cl (batch 158) measured this band failing to fit
            // Leg, Matchup and Clock at current sizes — Leg's ink collided with Matchup's by
            // 41.7px, and even with the 2px ink floor honoured on both sides the scoreline was
            // still 14.3px short of room (569.0px available against 583.3px needed) — and
            // recommended moving LEG n/m out rather than widening the band. TV built that: `_tLeg`
            // now lives in BuildTicketColumn, right-aligned beside `_tTicketHeader`. See that
            // construction for the full ruling.
            // §7: "Clock remains fixed at the right edge."
            // T91-am (batch 61): THE TOP BAND IS PARTITIONED. The scoreline's territory and the
            // clock's are DISJOINT, each sized to its OWN longest renderable form, and the scoreline
            // centres within its own territory rather than across its neighbour's.
            //
            // Before this, `Matchup` was centred in a 675.0px box that overlapped the clock's box by
            // 130.0px. On the read seed that left 2.5px of ink clearance; on the sweep's WIDEST
            // scoreline, `BRICKLAYERS 0 — MIDDLEMEN 0`, the inks COLLIDE BY 13.7px. A box that reaches
            // into a neighbour's territory is not a layout, it is a bet on the content — C46's shape,
            // and the reason a near-miss on one frame was actually an overprint.
            //
            // The clock's longest renderable form is `90'+2` at 69.5px, well inside the 127.7px the
            // partition arithmetic allows, so disposition 1 fires: both boxes bind, nothing moves
            // position, and the clock keeps its right-anchored constant ink edge (T75-am2).
            const float ClockTerritory = 80f;   // holds 69.5 with margin
            const float BandInkFloor = 2f;      // the same keep-out T90-am ruled for the column
            const float ClockRightPad = 10f;    // the clock's own right anchor, unchanged (T75-am2)

            // T91-am2 (batch 63): THE 2px FLOOR APPLIES TO BOTH SIDES OF THE COLUMN'S EDGE. T91-am
            // bounded the scoreline against the clock and left it flush against the ticket column —
            // "an edge has two sides and a floor on one of them is half a rule". So the usable stage
            // is 711.0, not 715.0, and the territories are derived from that rather than from the
            // band's raw width.
            float stageUsableL = BandInkFloor;
            float clockTerritoryL = sb.width - ClockRightPad - ClockTerritory;
            float scoreTerritoryL = stageUsableL;
            float scoreTerritoryR = clockTerritoryL - BandInkFloor;
            float scoreTerritory = scoreTerritoryR - scoreTerritoryL;
            // Centre the scoreline in ITS OWN territory, not the band's.
            float scoreCentreShift = (scoreTerritoryL + scoreTerritoryR) * 0.5f - sb.width * 0.5f;

            // T95 — THE PUNCH OVERLAY SHARES THIS RECT, and it is hoisted so it cannot drift again.
            //
            // `Score` mirrors `Matchup` — its own build comment says so in as many words: "Same text,
            // SAME RECT, same face as _tMatchup ... so superimposing it and boosting to L4 can only
            // make the existing scoreline brighter." Both are UpperCenter, so each centres its string
            // in ITS OWN box, and two centred layers with different boxes DO NOT SUPERIMPOSE — they
            // offset by the difference of their centres.
            //
            // T91-am moved `Matchup` and I did not re-derive the mirror: measured, the boxes were
            // 593.0 against 675.0 and the centres 92.7 against 133.7, so the scoreline rendered as
            // TWO COPIES 41.0px APART on every beat the punch fired. That is exactly the doubling
            // read on the closing frames, its magnitude is exactly this seat's own `scoreCentreShift`,
            // and §3.5 obliged re-deriving everything depending on that box's centre.
            //
            // Fixed by CONSTRUCTION rather than by copying the number across: one position, one size,
            // both layers. The same remedy T68 needed when an ink had five authors and T62 needed when
            // one value had two repaint schedules.
            Vector2 scorePos = AnchorTopCenter(sb, 8f) + new Vector2(scoreCentreShift, 0f);
            Vector2 scoreSize = new Vector2(scoreTerritory, sb.height - MomentumTapeHeight);

            _tClock = MakeText(sbRoot, "Clock", new Vector2(0f, 1f), new Vector2(1f, 1f),
                AnchorTopRight(sb, 10f, 8f), new Vector2(ClockTerritory, Mathf.Ceil(TypeClock * LineBox)),
                TypeClock, TextAnchor.UpperRight, flavorColor);
            // §4 Fact: "Score, clock, live leg names, market lines" — cold white at L3.
            _tMatchup = MakeText(sbRoot, "Matchup", new Vector2(0f, 1f), new Vector2(0.5f, 1f),
                scorePos, scoreSize, TypeScore,
                TextAnchor.UpperCenter, flavorColor, FontStyle.Bold);

            // C3 (Design Director ruling): "the score at a goal" joins the HDR-eligible set.
            // Matchup above is the PERSISTENT, always-on score truth at L3 — this is a punch-only
            // overlay at the SAME rect, normally hidden (§7's duplication ban is exactly why this
            // must not also read as an always-visible second score display), shown only for the
            // instant a goal commits and boosted through the shared HDR material to L4.
            // T58 (§4 violation, DD 2026-08-04): this overlay was GOLD, and it is the only gold on the
            // surface at the goal moment — measured 56-58° at up to 67% saturation on the scoreline's
            // peak pixel, against 204-205° at ~5% at rest. Gold is rationed to money: won legs, payout
            // figures, the cash-out band. A goal is not money; it is the event that may eventually
            // produce money, which is the exact distinction the rationing rule exists to hold.
            //
            // It also re-created T41's defect in a second channel: at the flash the scoreline read
            // 0.72 while the actionable cash-out band read 0.62, so the designated L4 element was
            // again not the brightest thing on its own surface — at the precise moment the player is
            // most likely to reach for the key. T41 capped the CONTINUOUS case; the flash was never
            // measured.
            //
            // The fix is the ruling verbatim: the punch stays a brightness event on the cold-white
            // channel. Same text, same rect, same face as _tMatchup, in the SAME cold white — so
            // superimposing it and boosting to L4 can only make the existing scoreline brighter, and
            // releasing it settles back. There is no hue to change, by construction.
            // T95: `scorePos`/`scoreSize` are the SAME values `_tMatchup` was built from, deliberately
            // shared rather than restated — the invariant this comment asserts is now enforced by the
            // construction instead of by two call sites agreeing.
            _tScoreFlash = MakeText(sbRoot, "Score", new Vector2(0f, 1f), new Vector2(0.5f, 1f),
                scorePos, scoreSize, TypeScore,
                TextAnchor.UpperCenter, flavorColor, FontStyle.Bold);
            _tScoreFlash.enabled = false;
            _scoreHdrMat = MakeHdrMaterial();
            if (_scoreHdrMat != null) _tScoreFlash.material = _scoreHdrMat;
        }

        private void BuildEventStrip(Transform root, LayoutGrid grid)
        {
            Image zone = MakePanel(root, "EventStripZone", new Vector2(0f, 1f), new Vector2(0f, 1f),
                AnchorTopLeft(grid.EventStrip), new Vector2(grid.EventStrip.width, grid.EventStrip.height), screenBg);
            Transform esRoot = ZoneRoot(zone); // T46
            // T67: the TEXT zone starts 40px past the band boundary — canvas x 305-980, not
            // 265-980. The zone's ground is unchanged; only the type is inset.
            //
            // Measured on the seated acceptance view: with the cash-out field lit, bloom crosses the
            // boundary and is +0.181 mean over the first 20 canvas px, +0.015 over the next 20, and
            // exactly 0.000 from x=365. The strip's COPY never warmed — its ink begins ~174px in on
            // the captured line — but the line is CENTRED, so it only clears the halo while it is
            // short. A near-full-width authored line would put its first glyph inside the halo, and
            // no such line existed in the capture to prove it. This is the structural answer to that
            // uncovered case: every line, at any length, now begins outside the measured reach.
            //
            // Separation is the remedy the ruling pre-committed to. The bloom is sealed and is not
            // touched, and the band T63 just granted is not dimmed. A short line moves 20px (the
            // centre of a narrower box); nothing else moves.
            const float StripBloomInset = 40f;
            Rect es = new Rect(StripBloomInset, 0f,
                grid.EventStrip.width - StripBloomInset, grid.EventStrip.height);

            // §7 Event strip: "One line, white, L2 at rest, punching to L3 at its reveal
            // callback." Reuses the "Flavor" GameObject name — required by PlayMode regression
            // coverage (TVS-H02's flavor-punch freeze test) and by every live-beat message call
            // site elsewhere in this file.
            _tFlavor = MakeText(esRoot, "Flavor", new Vector2(0f, 1f), new Vector2(0.5f, 0.5f),
                AnchorCenter(es), new Vector2(es.width - 24f, es.height - 8f),
                TypeEvent, TextAnchor.MiddleCenter, flavorColor, FontStyle.Bold,
                tracking: TvTrack.Name); // TvEventStrip.jsx:12
        }

        private void BuildCashOutZone(Transform root, LayoutGrid grid)
        {
            MakePanel(root, "CashOutZone", new Vector2(0f, 1f), new Vector2(0f, 1f),
                AnchorTopLeft(grid.CashOut), new Vector2(grid.CashOut.width, grid.CashOut.height), screenBg);

            // TV-03: the actionable field. Built BEFORE the type so the type punches out of it, and
            // sized to the zone exactly — canon's inversion is a solid field, not a tinted panel.
            // Disabled by default: the field IS the actionable state, so it exists only while the
            // key will actually work.
            // T63 — THE FIELD TAKES THE L4 VALUE, because it is the element the tier is about.
            //
            // It used to be painted `gold`, which is the L3 money colour, and it carried NO HDR
            // material at all — the boost was wired to `_tCashOut`, the money figure, one element
            // over (see below). So the surface's only *sustained* L4 element could not be boosted
            // even in principle, and it was wearing L3's colour while it failed to be boosted.
            // Two levels short, by construction, which is why no amount of re-measuring the band
            // ever found it at L4: it had never been there.
            //
            // Measured on the current set, Rec.709 luma (C33's unit), frame000 cash-out actionable:
            //     field (zone mean)            0.696
            //     money figure (boosted text)  0.827   <- the 0.827 the ruling measured is THIS
            //     quiet scoreline              0.866
            //     ball at the payoff punch     0.902
            // The field — the thing that reads as "the band" at four metres — was the dimmest of
            // the four, and the figure sitting on it was what the instrument had been catching.
            //
            // THE COLOUR STAYS `gold`, AND THE REASON IS MEASURED. `goldL4` was tried here and
            // reverted: a canvas vertex colour is packed to Color32, so (1.84, 1.31, 0.29) clamps
            // to (255, 255, 74) — hue 60 deg LEMON, not gold — and at the 1.4 boost a full-width
            // field that bright blooms across the whole panel. Measured on frames: with goldL4 the
            // band, the event strip AND the risk/pays footer all read hue 60.0 at ~61% saturation,
            // because every zone's peak had become this field's bloom rather than its own content.
            // That is a worse defect than the one T63 names, and it is why the value is not the
            // lever here. `gold` clamps to (255, 209, 46), which is still gold.
            _cashOutField = MakePanel(root, "CashOutField", new Vector2(0f, 1f), new Vector2(0f, 1f),
                AnchorTopLeft(grid.CashOut), new Vector2(grid.CashOut.width, grid.CashOut.height),
                new Color(gold.r, gold.g, gold.b, 1f));
            _cashOutField.enabled = false;

            // TV-04: money and status are SEPARATE elements. Canon is explicit that "the status word
            // rides at label scale beside the figure, NEVER at money scale" (TvCashOutSlot.jsx:35-47)
            // — the build rendered `CASH OUT $184   •   UPDATING` as one 29px string, which says the
            // status is as important as the number. Money keeps the name "CashOut" because the L4
            // token, the bloom-floor protected set and three tests all address it by that name.
            // T74-am3 (batch 56): THE MEMBERS STOP SHARING ONE RECTANGLE. Anchored from opposite
            // edges of one 241px box, each one's slack was the other's overrun, so any pair of long
            // strings collided — at rest (45.0), mid-tween (47.8) and at the held preview (198.5).
            // §6.1's "one fixed rectangle owning all six states, never reflows" is untouched: a
            // two-row rectangle is still fixed and still never reflows.
            //
            // The subdivision is MEASURED, and it is the one number in this pass that does not clear.
            // TMP's preferred line box is 36.3px for the 29px figure and 18.8px for the 15px status —
            // 55.0px against a 52.0px grid row, over by 3.0px. The design constants predicted a fit
            // (29*LineBox + 15*LineBox = 51.9) because LineBox is 1.18 and TMP's real advance ratio on
            // this face is 1.25. Allocated 34/18 to the row it has rather than the row it wants; both
            // components are Overflow, so the figure's line box is 2.3px taller than its allocation
            // and renders, but this does NOT fit by the sweep's standard and is reported as such.
            const float figureRowH = 34f, statusRowH = 18f;
            // +y is UP: AnchorCenter negates the grid's top-down y (see its body), which is why the
            // takeover pair reads +40 above and -20 below. Figure on top, status beneath it.
            const float figureRowY = 9f, statusRowY = -17f;

            _tCashOut = MakeText(root, "CashOut", new Vector2(0f, 1f), new Vector2(0f, 0.5f),
                AnchorCenter(grid.CashOut) + new Vector2(-grid.CashOut.width * 0.5f + 12f, figureRowY),
                new Vector2(grid.CashOut.width - 24f, figureRowH), TypeCashOut,
                TextAnchor.MiddleLeft, new Color(gold.r, gold.g, gold.b, 1f), FontStyle.Normal,
                Face.Condensed, FontWeight.Bold,
                TvTrack.Name); // TvCashOutSlot.jsx:33/37 · T73: real Condensed Bold 700
            _tCashOut.enabled = false;

            // Its own row now, at the full width instead of the figure's leftovers. Anchors are left
            // exactly as they were on both members: the ruling moved them onto separate rows and said
            // nothing about alignment, and inventing a new one here would be the composition change
            // nobody asked for.
            _tCashOutStatus = MakeText(root, "CashOutStatus", new Vector2(0f, 1f), new Vector2(1f, 0.5f),
                AnchorCenter(grid.CashOut) + new Vector2(grid.CashOut.width * 0.5f - 12f, statusRowY),
                new Vector2(grid.CashOut.width - 24f, statusRowH), TypeEyebrow,
                TextAnchor.MiddleRight, AtTier(contextGrey, TierL2),
                tracking: TvTrack.Label); // TvCashOutSlot.jsx:44 — the status word, not the figure
            _tCashOutStatus.enabled = false;
            // T63: ONE material, both elements of the slot. The boost used to reach only the money
            // figure, so `RequestL4(HdrFocus.CashOut)` moved a number and left the field it sits on
            // at rest — the token was granted and the surface did not change where the eye reads it.
            //
            // Sharing the instance is correct HERE and is not the C15 hazard: that warning is about
            // two INDEPENDENT elements accidentally sharing one material and both going to L4. These
            // two are one slot with one token, so one material is what makes them move together and
            // makes it impossible for the field to be lit while the figure is not.
            _cashOutHdrMat = MakeHdrMaterial();
            if (_cashOutHdrMat != null) _tCashOut.material = _cashOutHdrMat;
            _cashOutFieldHdrMat = MakeHdrMaterial();
            if (_cashOutFieldHdrMat != null) _cashOutField.material = _cashOutFieldHdrMat;

            // §8.7 / §8.5 Pending window: "intervention controls live in their own overlay, never
            // in [the cash-out] row." Centered over the stage's safe area, where the frozen shot
            // remains visible.
            _tInterventionPrompt = MakeText(root, "InterventionPrompt", new Vector2(0f, 1f), new Vector2(0.5f, 0.5f),
                AnchorCenter(grid.Stage), new Vector2(grid.Stage.width - 80f, 90f), TypeIntervention,
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
                TypeChrome, TextAnchor.UpperCenter, contextGrey);
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

        /// <summary>Phase T: this builds a <see cref="TextMeshProUGUI"/>. The SIGNATURE is unchanged
        /// on purpose — <c>TextAnchor</c> and <c>FontStyle</c> stay in it and are mapped here, so the
        /// migration is a helper change and all 22 call sites keep their authored intent in the same
        /// vocabulary the rest of the file uses. The laptop's own migration kept its signature for
        /// the same reason (<c>LaptopOs.ToTmpAlignment</c>'s note).
        ///
        /// <para><b>WEIGHT IS TWO DIFFERENT THINGS HERE, and the call site says which.</b>
        /// <c>style: FontStyle.Bold</c> is TMP's material-level FAUX bold — the synthesised weight
        /// UGUI drew, kept wherever no ruling has replaced it. <c>weight: FontWeight.Bold</c> resolves
        /// through the font asset's weight table to the REAL 700 face. T73 (batch 32) ruled real
        /// Condensed Bold 700 for the four condensed slots that carry it; those pass
        /// <c>style: Normal, weight: Bold</c>, because setting both would lay faux bold on top of a
        /// real bold face and thicken it twice.</para>
        ///
        /// <para>The seven REGULAR-face slots that ask for bold are deliberately left synthesised.
        /// T73 names four sites and they are all condensed; `EncodeSans Bold SDF` is built and wired
        /// into the regular face's weight table at 700, so switching them is one argument each — but
        /// it is a design change nobody has ruled, and generating a face does not license using
        /// it.</para></summary>
        private TMP_Text MakeText(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 pos,
            Vector2 size, int fontSize, TextAnchor align, Color color,
            FontStyle style = FontStyle.Normal, Face face = Face.Regular,
            FontWeight weight = FontWeight.Regular, float tracking = 0f)
        {
            var go = new GameObject(name, typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            // TV-19: canon assigns the face per slot (tokens/fonts.css), read off the component
            // references one at a time — it is not a stylistic default. Falls back to the regular
            // face if the condensed asset is missing, rather than rendering nothing.
            TMP_FontAsset want = face == Face.Condensed && _fontCond != null ? _fontCond : _font;
            if (want != null) t.font = want;
            t.fontSize = fontSize * TypeScale;
            t.fontStyle = ToTmpStyle(style);
            // Set unconditionally, including at Regular, so every slot comes out of the same code
            // path. A weight that is "whatever TMP defaults to" is not a chosen weight, and this
            // surface has already paid once for a face nobody chose.
            t.fontWeight = weight;
            // em -> TMP's hundredths of an em, converted here so no call site knows TMP's unit.
            // Fits and FitToColumn measure through GetPreferredValues on the component itself, so
            // they pick this up automatically — a slot cannot render with tracking and measure
            // without it, which is the failure the laptop had to add a term to MeasureWidth to avoid.
            t.characterSpacing = tracking * 100f;
            t.alignment = ToTmpAlignment(align);
            t.color = color;
            t.raycastTarget = false;
            // The pair of overflow modes UGUI's HorizontalWrapMode.Overflow + VerticalWrapMode.Overflow
            // expressed: never wrap, never clip. T46's containment is done by the zone panels' own
            // clipping, not by the text component, and §5.1's fixed slots depend on a string never
            // reflowing into a second line.
            t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Overflow;
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return t;
        }

        /// <summary>Mirrors <c>LaptopOs.ToTmpAlignment</c> exactly. Duplicated rather than shared
        /// because that one is private to the laptop's class, and reaching across surfaces to
        /// consolidate it would be a second change inside a migration that is allowed one. Worth
        /// hoisting into a shared UI helper once both surfaces are on TMP; noted, not done.</summary>
        private static TextAlignmentOptions ToTmpAlignment(TextAnchor align) => align switch
        {
            TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft,
            TextAnchor.UpperCenter => TextAlignmentOptions.Top,
            TextAnchor.UpperRight => TextAlignmentOptions.TopRight,
            TextAnchor.MiddleLeft => TextAlignmentOptions.Left,
            TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
            TextAnchor.MiddleRight => TextAlignmentOptions.Right,
            TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft,
            TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
            _ => TextAlignmentOptions.BottomRight
        };

        /// <summary>UGUI's FontStyle to TMP's FontStyles. Both bold and italic here are TMP's
        /// synthesised forms, which is what UGUI was drawing — preserving the render is the whole
        /// point of this step (C43).</summary>
        private static FontStyles ToTmpStyle(FontStyle style) => style switch
        {
            FontStyle.Bold => FontStyles.Bold,
            FontStyle.Italic => FontStyles.Italic,
            FontStyle.BoldAndItalic => FontStyles.Bold | FontStyles.Italic,
            _ => FontStyles.Normal
        };

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
        // RegenNoise was one of two UnityEngine.Random uses in this file. The other lived at
        // _emissSeed's initialisation and outlived T8 because a flicker phase is not the *discrete
        // scene choice* PRD §4.3 bans. T64 struck the flicker itself, so both are now gone and this
        // file calls UnityEngine.Random nowhere.

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
            _cashOutFieldHdrMat?.SetFloat(HdrBoostId, HdrBoostL3);
            _bigAmountHdrMat?.SetFloat(HdrBoostId, HdrBoostL3);
            _scoreHdrMat?.SetFloat(HdrBoostId, HdrBoostL3);
            _ballHdrMat?.SetFloat(HdrBoostId, HdrBoostL3);
        }

        private void ApplyBoost(HdrFocus focus, float boost)
        {
            switch (focus)
            {
                // The flood rode this focus for one batch, to keep it punching after the payoff
                // figure moved off `Payout`. T40 struck the flood outright, so the question of what
                // boosts it is gone with it — the slot's own two graphics are the whole focus again.
                case HdrFocus.CashOut:
                    // T63: ONE token, BOTH graphics of the band. The figure alone used to be
                    // boosted, so granting the token moved a number and left the gold field it
                    // sits on at rest — the band could not reach L4 however the token arbitrated.
                    // Same shape as Payout below, which has always driven two materials.
                    _cashOutHdrMat?.SetFloat(HdrBoostId, boost);
                    _cashOutFieldHdrMat?.SetFloat(HdrBoostId, boost);
                    break;
                case HdrFocus.Payout:
                    _bigAmountHdrMat?.SetFloat(HdrBoostId, boost);
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
        /// <summary>Phase T: the TMP asset, not the TTF. `EncodeSans SDF` is the Regular 400 / wdth
        /// 100 NAMED INSTANCE, resolved by style name at generation time (see TvTmpFontAssets) —
        /// never faceIndex 0, whose axis defaults on this family are wght 100 / wdth 75.</summary>
        private static TMP_FontAsset LoadFont() => LoadFace("Tv/Fonts/EncodeSans SDF", "--font-tv");

        private static TMP_FontAsset LoadFontCondensed()
            => LoadFace("Tv/Fonts/EncodeSansCondensed SDF", "--font-tv-cond");

        /// <summary>Resolves one of canon's two TV faces (`tokens/fonts.css`). Falls back to the
        /// built-in face rather than to null: a missing font asset should degrade to
        /// readable-but-wrong, never to an invisible surface.
        ///
        /// <para>The fallback logs loudly and names the token, because every px value on this
        /// surface was derived against Encode Sans — T20's whole re-derivation is a metrics
        /// argument — so copy fit and the type scale are NOT valid in the fallback. Silently
        /// rendering in the wrong face is the failure this exists to end.</para></summary>
        private static TMP_FontAsset LoadFace(string resourcePath, string token)
        {
            TMP_FontAsset face = Resources.Load<TMP_FontAsset>(resourcePath);
            if (face != null) return face;

            Debug.LogWarning($"[TvSweatScreen] {token} not found at Resources/{resourcePath} — falling " +
                "back to TMP's default face. Copy fit and the T20 type scale are NOT valid in the fallback.");
            // TMP's default rather than LegacyRuntime.ttf: the components are TMP now, and handing
            // one a null font asset renders nothing at all. Degrade to readable-but-wrong, never to
            // an invisible surface — the same rule the UGUI version stated, in the new currency.
            TMP_FontAsset fallback = TMP_Settings.defaultFontAsset;
            if (fallback == null)
                Debug.LogWarning("[TvSweatScreen] TMP has no default font asset either; text will not render.");
            return fallback;
        }
    }
}
