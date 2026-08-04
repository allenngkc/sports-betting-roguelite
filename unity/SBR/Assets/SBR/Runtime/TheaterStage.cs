using System;
using System.Collections.Generic;
using SBR.Engine;
using UnityEngine;
using UnityEngine.UI;

namespace SBR.Game
{
    /// <summary>
    /// The match theater's stage (F_0.2.0 M-T2..M-T3.1): a top-down neon-on-black pitch where
    /// anonymous team-colored dots act out the drama stream. This is a RENDERER, never a
    /// simulation — the theater laws (design/04, superseded ruling 2026-07-18):
    ///
    ///  1. Every staged moment is keyed by a DramaEvent. Between scenes the idle possession
    ///     loop RESTATES the last revealed win probability — no goals, no saves, no probability
    ///     movement is ever implied by filler.
    ///  2. Presentation-local RNG only (System.Random here; the engine's streams are sacred).
    ///  3. The palette law extends onto the stage: near-black pitch (never money-green), team
    ///     dots from TheaterPalette's non-reserved pool (never hot red), neutral broadcast
    ///     lines; the desaturated-cyan treatment appears ONLY for a VOID.
    ///
    /// The PICKED team always attacks RIGHT: the ball drifting right is money coming home.
    ///
    /// M-T3.1 coherence (playtest #11): the ball is always CARRIED — one sticky carrier, passes
    /// go dot-to-dot within the team in possession, turnovers are visible interceptions. Scene
    /// waypoints route through the actual actors of the attacking team (a shot launches from
    /// the last carrier toward the corner AWAY from the defending keeper; keepers never receive
    /// a pass — they only save or concede). Formations breathe both ways: the attacking shape
    /// pushes up while the defending team drops into a compact block; on breakaways the nearest
    /// defenders chase the carrier. Scenes also fire an onReveal callback at their payoff
    /// moment (the goal / the save / scene end) so the chrome can stay causally honest.
    ///
    /// Scene STEP TIME scales by <see cref="timeScale"/> — a DURATION multiplier with the same
    /// semantics as TvSweatScreen's TimeScaleOverride (1 = ship pacing, tiny = fast-forward,
    /// 0 = frame-rate bound); dot motion runs on real frame time (it is only ever cosmetic).
    /// Freezing (stand-up pause) halts the whole stage mid-motion — the frozen frame IS the dread.
    /// </summary>
    public sealed class TheaterStage : MonoBehaviour
    {
        [Header("Feel dials")]
        [Tooltip("Seconds between idle passes (min).")]
        public float passIntervalMin = 0.7f;
        [Tooltip("Seconds between idle passes (max).")]
        public float passIntervalMax = 1.5f;
        [Tooltip("Dot smooth-damp time; smaller = snappier.")]
        public float dotDamp = 0.55f;
        [Tooltip("Ball smooth-damp time.")]
        public float ballDamp = 0.28f;
        [Tooltip("How far a beat impulse shoves territory, in pitch fraction (idle-mode Pulse).")]
        public float pulseKick = 0.22f;
        [Tooltip("Seconds a beat impulse takes to decay back to the honest territory point.")]
        public float pulseDecay = 1.6f;
        [Tooltip("Multiplies every scene step DURATION (same semantics as TvSweatScreen's " +
                 "TimeScaleOverride, which is forwarded here): 1 = ship pacing, tiny = " +
                 "fast-forward, 0 = as fast as the frame rate allows.")]
        public float timeScale = 1f;
        [Tooltip("Presentation tempo for the ABSOLUTE step durations (correction sub-scenes, " +
                 "kill-shot buildups, continuations). TvSweatScreen forwards SweatPacer's " +
                 "paceMultiplier so the stage's playback matches the pacer's arithmetic; the " +
                 "fraction-based steps already inherit it through SceneSpec.Duration.")]
        public float paceScale = 1f;

        private RectTransform _rt;
        private float _w, _h;
        private const float Pad = 12f;

        private Image[] _homeDots;
        private Image[] _awayDots;
        private Image _homeKeeper, _awayKeeper, _ball;
        private Image _flashRing;
        private Image _bookingCard;

        private Vector2[] _homePos, _awayPos, _homeVel, _awayVel, _homeNoise, _awayNoise;
        private float[] _homeReactionLag, _awayReactionLag, _homeNoiseTimer, _awayNoiseTimer;
        private float[] _homeShapeBias, _awayShapeBias, _homeShapeBiasVel, _awayShapeBiasVel;
        private Vector2 _ballPos, _ballVel;
        private Vector2 _hkPos, _akPos, _hkVel, _akVel;

        private System.Random _rng;
        private bool _frozen;
        private bool _live;
        private bool _homeAttacksRight = true;
        private float _prob = 0.5f;      // picked side's last revealed win prob — territory truth
        private float _pulse;            // transient beat impulse (idle mode), decays to 0
        private float _pulseSign;
        private float _nextPassAt;
        private float _urgency;          // 0..1, briefly raised by big beats

        // ---- possession (M-T3.1: the ball is always carried) ----
        private bool _carrierHome;       // which team has it
        private int _carrierIx;          // outfield index of the carrier

        private static Sprite _circleSprite;
        private static Sprite _ringSprite;
        private static int s_seedSalt; // per-instance RNG salt (presentation-local, never engine)

        // ---- scene playback (M-T3) ----
        private const byte MkNone = 0, MkGoal = 1, MkSuspend = 2, MkSave = 3, MkVoid = 4,
            MkCorner = 5, MkBooking = 6;
        // Phase 2E-1 (PRD §7.2/§7.3/§10): one marker per non-goal near-miss payoff. MkSave keeps
        // its pre-existing "the save IS the near-miss payoff" meaning and is used ONLY for
        // ScenePayoff.KeeperSave — it is the one payoff with a real actor (the keeper) physically
        // reacting (KeeperLunge). The other five payoffs have no keeper action to key off, so
        // each fires FireReveal() at ITS OWN causal moment instead of borrowing MkSave's (which
        // would wrongly invoke KeeperLunge for a chance the keeper never touched) and instead of
        // relying on the "no explicit marker -> reveal at scene end" fallback (that fallback
        // exists for genuinely markerless templates like Territory/CalmPossession; a near-miss
        // payoff always has a causal moment to mark). See BuildNearMissCore for which route/actor
        // each marker pairs with.
        private const byte MkBlock = 7, MkIntercept = 8, MkClearance = 9, MkPost = 10, MkNearWide = 11;
        // Ball routing per step (M-T3.1): how the authored waypoint finds a real actor.
        private const byte RoutePass = 0;      // nearest attacking OUTFIELD dot to the waypoint — a pass
        private const byte RouteAuthored = 1;  // fly to the authored point (holds, restarts to spots)
        private const byte RouteShot = 2;      // authored point, y snapped to the corner AWAY from the keeper
        private const byte RouteBackLine = 3;  // nearest DEFENDING back-line dot (clearances, chalked restarts)
        private const byte RouteKickoff = 4;   // center spot; the conceding side collects at step end
        private const byte RouteCorner = 5;    // corner arc; the attacking side takes the kick

        private struct Step
        {
            public float Dur;      // seconds (× timeScale when advancing)
            public Vector2 Ball;   // authored target, normalized PICKED frame (right = picked attack)
            public float Terr;     // territory the formations speak during this step
            public float Tempo;    // 0..1 urgency (actor speed, pass tempo)
            public byte Marker;    // fired at step END
            public byte Route;     // ball routing mode
            public bool AtkPicked; // the side running the move THIS step (continuations switch)
            public bool Chase;     // nearest defenders hunt the ball this step
            public ScoreLedger.StagedGoal Goal; // rides MkGoal
            public CountLedger.StagedCount Count; // rides MkCorner/MkBooking
        }

        private Step[] _script;
        private int _stepIx;
        private float _stepT;
        private bool _stepEntered;
        private bool _suspendedAtShot;
        private float _sceneTerr = 0.5f;
        private float _sceneTerrVel;
        private Action<ScoreLedger.StagedGoal> _onGoalPlayed;
        private Action<CountLedger.StagedCount> _onCountPlayed;
        private Action _onReveal;
        private bool _revealFired;
        private Action _onSceneComplete;
        private int _routeDotIx = -1;    // resolved actor for RoutePass/RouteBackLine steps
        private bool _routeDotHome;
        private Vector2 _stepBallLocal;  // resolved target for authored/shot steps
        private float _flashT;           // net-ripple flash countdown
        private float _flashDur;
        private bool _flashRight;
        // TVS-H03: set only when EnterStep resolves a RoutePass step whose StagedGoal carries a
        // plan-time-bound actor — never by spatial proximity. Test/debug tracking only; nothing
        // in playback reads it back.
        private bool _boundActorActive;
        private bool _boundActorHome;
        private int _boundActorIx;

        // ---- §7.7 backed-player locator: the BINDING half ----
        //
        // PRD §7.7's hard constraint is not the decoration, it is continuity: "if the backed player
        // is marked throughout the sweat, the marked actor must be the actor that takes the visible
        // final touch at a scoring payoff." That is the constraint TVS-H03 failed once already, when
        // identity was cosmetically correct and causally unconnected — so the index is resolved by
        // the SAME expression RoutePass uses on the bound goal, not by a parallel one that merely
        // happens to agree today. `_homeDots`/`_awayDots` are built once at a fixed
        // PitchLayout.OutfieldPerTeam, so the mapping is stable for the whole sweat.
        //
        // Deliberately NOT reset by StartScript/StartScene: it survives scene changes, because
        // "continuous, not reveal-only" is the whole requirement. It is cleared per LEG.
        //
        // The TREATMENT is absent on purpose — numeral vs ring vs halo is reserved to the design
        // track by §7.7 ("not decided here"), and the two candidates are not equivalent here:
        // DESIGN.md §7's "numbered cell" is justified by "the matrix gives legible small numerals
        // for free", but §6 records that matrix as RETIRED, and this file has no Text or Font at all
        // (MakeImage/MakeDot/MakeRect only), so a numeral means a new font dependency on a stale
        // rationale. A ring is nearly free — RingSprite() already exists for the net ripple. Routed
        // rather than chosen.
        private bool _backedActorActive;
        private bool _backedActorHome;
        private int _backedActorIx;
        // Phase 2D (PRD §7.6): sticky "last real actor the routing touched" — refreshed at every
        // EnterStep transition, but only OVERWRITTEN when the OUTGOING step itself resolved a
        // real dot (RoutePass/RouteBackLine); a step whose own route never resolves an actor
        // (RouteCorner, RouteAuthored, RouteShot, RouteKickoff) leaves it exactly as it was. This
        // is what lets a test confirm a corner's "ball goes out off the defender" beat (routed via
        // RouteBackLine, immediately before the RouteCorner delivery step) genuinely touched a
        // defending-side actor — still readable at, and after, the corner marker fires even
        // though the delivery step itself routes to the authored arc, not a dot. Test/debug only;
        // nothing in playback reads it back.
        private bool _priorRouteDotActive;
        private bool _priorRouteDotHome;
        private int _priorRouteDotIx;
        // Phase 2D (PRD §7.6): the exact actor a BOOKING scene's marker attached to. Unlike
        // Corner's RouteCorner delivery, Booking's MkBooking step resolves through RoutePass (see
        // FlashBooking), so this is set once there and held until the next scene starts — a test
        // can poll it after the scene completes instead of racing the callback. Test/debug only.
        private bool _markerActorActive;
        private bool _markerActorHome;
        private int _markerActorIx;

        // ---- public test/debug surface ----
        /// <summary>The territory x the last Update spoke (honest prob + decaying pulse in idle,
        /// the authored scene territory during playback), in normalized pitch space. Test/debug
        /// only — gameplay never reads the stage.</summary>
        public float LastTerritoryX { get; private set; } = 0.5f;
        public bool IsFrozen => _frozen;
        /// <summary>A scene script is active (suspension counts — the scene is mid-story).</summary>
        public bool ScenePlaying => _script != null;
        /// <summary>Frozen at the kill shot's suspension point, awaiting ResumeSuspended.</summary>
        public bool SuspendedAtShot => _suspendedAtShot;
        /// <summary>TVS-H03 test/debug surface: the actor the CURRENT scene's routing bound to a
        /// staged goal's identity (never spatial nearest-neighbor), as of the most recently
        /// entered RoutePass step. Null when no step in the active scene carried a bound actor.
        /// Proves the STAGE's routing consumed the plan-time binding, not just that the plan
        /// carried one. Gameplay never reads this.</summary>
        public (bool IsHome, int RosterIndex)? BoundActorRouted
            => _boundActorActive ? ((bool, int)?)(_boundActorHome, _boundActorIx) : null;

        /// <summary>§7.7: the actor currently marked as the backed player, or null on any leg that
        /// is not an anytime-scorer market. Null is the correct and only answer elsewhere — a
        /// locator on a moneyline leg would point at a player the bet does not depend on.</summary>
        public (bool IsHome, int DotIndex)? BackedActorMarked
            => _backedActorActive ? ((bool, int)?)(_backedActorHome, _backedActorIx) : null;

        /// <summary>The one place a roster index becomes a dot index. RoutePass resolves the bound
        /// scorer's actor with this same expression, which is what makes §7.7's continuity contract
        /// — marked actor IS final-touch actor — hold by construction rather than by coincidence.</summary>
        private static int DotIndexFor(int rosterIndex, int dotCount)
            => dotCount > 0 ? Mathf.Abs(rosterIndex) % dotCount : -1;

        /// <summary>§7.7: mark the backed player for the whole sweat. Idempotent, and independent of
        /// grade, scene, and outcome — the locator reveals POSITION, never RESULT (§4.2), so nothing
        /// about when or whether it appears may correlate with the payoff.</summary>
        public void SetBackedPlayer(bool isHome, int rosterIndex)
        {
            Image[] dots = isHome ? _homeDots : _awayDots;
            int ix = DotIndexFor(rosterIndex, dots != null ? dots.Length : 0);
            if (ix < 0) { ClearBackedPlayer(); return; }
            _backedActorActive = true;
            _backedActorHome = isHome;
            _backedActorIx = ix;
        }

        /// <summary>Clears the locator. Called per leg, never per scene.</summary>
        public void ClearBackedPlayer()
        {
            _backedActorActive = false;
            _backedActorIx = -1;
        }
        /// <summary>Phase 2D test/debug surface: the most recent REAL actor (RoutePass/
        /// RouteBackLine only) the ball routing resolved to, sticky across any step whose own
        /// routing never resolves one — see <see cref="_priorRouteDotActive"/>'s doc for exactly
        /// what "sticky" means here. Null only when no step in the active (or most recently
        /// active) scene has ever resolved a real actor. Gameplay never reads this.</summary>
        public (bool IsHome, int RosterIndex)? LastTouchedActor
            => _priorRouteDotActive ? ((bool, int)?)(_priorRouteDotHome, _priorRouteDotIx) : null;
        /// <summary>Phase 2D test/debug surface: the actor a BOOKING scene's marker attached to
        /// (see <see cref="FlashBooking"/>) — proves the card lands on a real actor of the
        /// fouling side, not merely at the ball's coordinate. Null until a booking marker has
        /// fired for the most recently started scene. Gameplay never reads this.</summary>
        public (bool IsHome, int RosterIndex)? MarkerActorRouted
            => _markerActorActive ? ((bool, int)?)(_markerActorHome, _markerActorIx) : null;

        // ------------------------------------------------------------------ construction

        /// <summary>Builds the stage under a world-space canvas. Center/size in canvas pixels.</summary>
        /// <summary>Builds the stage into a TV canvas.
        ///
        /// <para><paramref name="centerFromTopLeft"/> is the rect's centre point measured from the
        /// canvas's TOP-LEFT, x right-positive and y NEGATIVE downward — the same space
        /// <c>TvSweatScreen</c>'s <c>AnchorTopLeft</c>/<c>AnchorCenter</c> helpers produce and every
        /// other element on that canvas is placed in. It is named for its space on purpose: see
        /// BuildInternal for what a silent disagreement here costs.</para></summary>
        public static TheaterStage Build(Transform canvasRoot, Vector2 centerFromTopLeft, Vector2 size,
            Color lineColor, Color pitchBg)
        {
            var go = new GameObject("TheaterStage", typeof(RectTransform), typeof(TheaterStage));
            go.transform.SetParent(canvasRoot, false);
            var stage = go.GetComponent<TheaterStage>();
            stage.BuildInternal(centerFromTopLeft, size, lineColor, pitchBg);
            return stage;
        }

        private void BuildInternal(Vector2 centerFromTopLeft, Vector2 size, Color lineColor, Color pitchBg)
        {
            _rt = (RectTransform)transform;
            // T25.1 — anchored TOP-LEFT, matching every other element TvSweatScreen builds.
            //
            // This was (0.5, 0.5), centre-anchored, and was correct while the only caller passed a
            // centre-relative offset (pre-3C: `Build(root, new Vector2(0f, 8f), ...)`). Phase 3C's
            // Layout B rebuild switched the call site to `AnchorCenter(grid.Stage)` — a TOP-LEFT
            // space coordinate — without changing this line, so the stage read a top-left
            // coordinate as centre-relative and drew itself roughly half a canvas down and right:
            // the pitch and every actor rendered OUTSIDE the TV's glass entirely.
            //
            // Every suite stayed green through five subsequent commits because nothing asserted the
            // stage's rect; it took seated capture frames to see it. The regression test added with
            // this fix is what makes it visible to a headless run.
            _rt.anchorMin = _rt.anchorMax = new Vector2(0f, 1f);
            _rt.pivot = new Vector2(0.5f, 0.5f);
            _rt.sizeDelta = size;
            _rt.anchoredPosition = centerFromTopLeft;
            _w = size.x;
            _h = size.y;

            // T46 (DD 2026-08-02): "the stage clips to its region." The stage's rect has always been
            // exactly the Stage zone — the grid is correct — but nothing enforced that its CHILDREN
            // stayed inside it, and one does not: NetRipple is positioned at 0.485 of the padded
            // width and then scaled to 1.7, which carries its outer edge ~155px past the stage's own
            // edge. On a left-side flash that lands well inside the ticket column, on top of the leg
            // rows, because the stage is built after them.
            //
            // T25.1's canvas mask cannot see this: its bound is the glass, and this never leaves the
            // glass — it leaves its ZONE. A clip rect here is structural in the way a corrected
            // coordinate is not; it holds for effects nobody has written yet.
            gameObject.AddComponent<RectMask2D>();

            _rng = new System.Random(unchecked(Environment.TickCount * 31 + s_seedSalt++));

            // The pitch: near-black surface (the palette law keeps money-green pure), thin
            // neutral neon lines. All code-built Images — no assets.
            MakeRect("PitchBg", Vector2.zero, size, pitchBg);
            float lw = 2f;
            Color line = lineColor;

            // Border.
            MakeRect("BorderT", new Vector2(0f, _h / 2f - lw / 2f), new Vector2(_w, lw), line);
            MakeRect("BorderB", new Vector2(0f, -_h / 2f + lw / 2f), new Vector2(_w, lw), line);
            MakeRect("BorderL", new Vector2(-_w / 2f + lw / 2f, 0f), new Vector2(lw, _h), line);
            MakeRect("BorderR", new Vector2(_w / 2f - lw / 2f, 0f), new Vector2(lw, _h), line);
            // Halfway line + center circle + spot.
            MakeRect("Halfway", Vector2.zero, new Vector2(lw, _h), line);
            Image ring = MakeImage("CenterCircle", Vector2.zero, new Vector2(_h * 0.42f, _h * 0.42f), line);
            ring.sprite = RingSprite();
            MakeDot("CenterSpot", Vector2.zero, 5f, line);

            // Penalty boxes (three strokes each) + goal mouths.
            float boxW = _w * 0.16f, boxH = _h * 0.52f;
            BuildBox(-_w / 2f, boxW, boxH, line, lw, mirrored: false);
            BuildBox(_w / 2f, boxW, boxH, line, lw, mirrored: true);
            float goalH = _h * 0.22f;
            // T41: the goal mouths were pure Color.white at alpha 1 — two permanently
            // full-brightness objects on a surface whose law permits ONE. They are markings, so
            // they take the marking colour at the top of the markings band (L2), not white.
            MakeRect("GoalL", new Vector2(-_w / 2f + 2f, 0f), new Vector2(5f, goalH), AtTier(line, TierL2));
            MakeRect("GoalR", new Vector2(_w / 2f - 2f, 0f), new Vector2(5f, goalH), AtTier(line, TierL2));

            // Actors: 8 outfield per team + keepers + the ball (built last = drawn on top).
            _homeDots = new Image[PitchLayout.OutfieldPerTeam];
            _awayDots = new Image[PitchLayout.OutfieldPerTeam];
            _homePos = new Vector2[PitchLayout.OutfieldPerTeam];
            _awayPos = new Vector2[PitchLayout.OutfieldPerTeam];
            _homeVel = new Vector2[PitchLayout.OutfieldPerTeam];
            _awayVel = new Vector2[PitchLayout.OutfieldPerTeam];
            _homeNoise = new Vector2[PitchLayout.OutfieldPerTeam];
            _awayNoise = new Vector2[PitchLayout.OutfieldPerTeam];
            _homeReactionLag = new float[PitchLayout.OutfieldPerTeam];
            _awayReactionLag = new float[PitchLayout.OutfieldPerTeam];
            _homeNoiseTimer = new float[PitchLayout.OutfieldPerTeam];
            _awayNoiseTimer = new float[PitchLayout.OutfieldPerTeam];
            _homeShapeBias = new float[PitchLayout.OutfieldPerTeam];
            _awayShapeBias = new float[PitchLayout.OutfieldPerTeam];
            _homeShapeBiasVel = new float[PitchLayout.OutfieldPerTeam];
            _awayShapeBiasVel = new float[PitchLayout.OutfieldPerTeam];
            for (int i = 0; i < PitchLayout.OutfieldPerTeam; i++)
            {
                _homeDots[i] = MakeDot($"Home{i}", Vector2.zero, 22f, Color.gray);
                _awayDots[i] = MakeDot($"Away{i}", Vector2.zero, 22f, Color.gray);
            }
            _homeKeeper = MakeDot("HomeKeeper", Vector2.zero, 26f, Color.gray);
            _awayKeeper = MakeDot("AwayKeeper", Vector2.zero, 26f, Color.gray);
            // T41: the ball sits at L3. §7 permits it L4 "and only at a payoff" — that punch is the
            // separate _ballFlash overlay the screen raises through the HDR material, so the
            // persistent ball must NOT already be there. A non-payoff ball at 1.000 is what made
            // the pitch outrank the cash-out band in every measured frame.
            _ball = MakeDot("Ball", Vector2.zero, 12f, AtTier(Color.white, TierL3));

            // The net-ripple flash (one reusable ring; positioned at whichever goal scores).
            _flashRing = MakeImage("NetRipple", Vector2.zero, new Vector2(_h * 0.5f, _h * 0.5f), Color.white);
            _flashRing.sprite = RingSprite();
            _flashRing.enabled = false;
            _bookingCard = MakeRect("BookingCard", Vector2.zero, new Vector2(16f, 26f),
                new Color(0.94f, 0.96f, 1f, 1f));
            _bookingCard.enabled = false;

            Show(false);
        }

        private void BuildBox(float goalLineX, float boxW, float boxH, Color line, float lw, bool mirrored)
        {
            float dir = mirrored ? -1f : 1f;
            float innerX = goalLineX + dir * boxW;
            MakeRect(mirrored ? "BoxRF" : "BoxLF", new Vector2(innerX, 0f), new Vector2(lw, boxH), line);
            MakeRect(mirrored ? "BoxRT" : "BoxLT",
                new Vector2(goalLineX + dir * boxW / 2f, boxH / 2f), new Vector2(boxW, lw), line);
            MakeRect(mirrored ? "BoxRB" : "BoxLB",
                new Vector2(goalLineX + dir * boxW / 2f, -boxH / 2f), new Vector2(boxW, lw), line);
        }

        // ------------------------------------------------------------------ public surface

        /// <summary>New leg: team colors from the model, picked side attacking right. Actors
        /// snap to kickoff formation; the ball to the center spot; the picked team kicks off.</summary>
        public void BeginLeg(Color homeColor, Color awayColor, bool pickedIsHome, float openingProb)
        {
            CancelScene();
            _homeAttacksRight = pickedIsHome;
            _prob = Mathf.Clamp01(openingProb);
            _pulse = 0f;
            _urgency = 0f;
            _live = true;
            _sceneTerr = PitchLayout.TerritoryX(_prob);
            LastTerritoryX = _sceneTerr;

            Color homeKeeperColor = Brighten(homeColor);
            Color awayKeeperColor = Brighten(awayColor);
            for (int i = 0; i < PitchLayout.OutfieldPerTeam; i++)
            {
                // T41: actors are L3. §7 — "actors are single lit cells ... in team hue at L3."
                // They were shipping at the colour's own alpha, which is 1.
                _homeDots[i].color = AtTier(homeColor, TierL3);
                _awayDots[i].color = AtTier(awayColor, TierL3);
                RerollNoise(ref _homeNoise[i]);
                RerollNoise(ref _awayNoise[i]);
                _homeReactionLag[i] = Rand(0.8f, 1.5f);
                _awayReactionLag[i] = Rand(0.8f, 1.5f);
                _homeNoiseTimer[i] = Rand(0.8f, 2.2f);
                _awayNoiseTimer[i] = Rand(0.8f, 2.2f);
                _homeShapeBias[i] = _awayShapeBias[i] = 0f;
                _homeShapeBiasVel[i] = _awayShapeBiasVel[i] = 0f;
                _homePos[i] = ToLocal(PitchLayout.FormationSlot(i, _homeAttacksRight, 0f));
                _awayPos[i] = ToLocal(PitchLayout.FormationSlot(i, !_homeAttacksRight, 0f));
                _homeVel[i] = _awayVel[i] = Vector2.zero;
            }
            // Keepers are actors too — Brighten() lifts their hue for legibility against the
            // outfield, but the tier is the same L3 ceiling. Brighten clamps CHANNELS to 1 and
            // leaves alpha at 1, so without this it re-introduced exactly what T41 caps.
            _homeKeeper.color = AtTier(homeKeeperColor, TierL3);
            _awayKeeper.color = AtTier(awayKeeperColor, TierL3);
            _hkPos = ToLocal(PitchLayout.Keeper(_homeAttacksRight));
            _akPos = ToLocal(PitchLayout.Keeper(!_homeAttacksRight));
            _hkVel = _akVel = Vector2.zero;
            _ball.color = Color.white;

            _ballPos = Vector2.zero;
            _ballVel = Vector2.zero;
            _carrierHome = _homeAttacksRight; // the picked side kicks off
            _carrierIx = 4;                   // central mid
            _nextPassAt = Time.time + NextPassDelay();
            ApplyPositions();
        }

        /// <summary>The honest live probability of the picked side — the territory truth the
        /// idle loop restates. Only ever called on a real beat (never invented by the stage).</summary>
        public void SetLiveProb(float pickedProb) => _prob = Mathf.Clamp01(pickedProb);

        /// <summary>A beat landed (idle-mode language, kept for the theaterless fallback and
        /// tests): shove territory toward the beneficiary's end and raise the tempo briefly.
        /// Scene playback (M-T3) supersedes this on the live theater path.</summary>
        public void Pulse(bool up, TensionTag tag)
        {
            _pulseSign = up ? 1f : -1f;
            _pulse = 1f;
            _urgency = tag == TensionTag.Calm ? Mathf.Max(_urgency, 0.25f) : 1f;
        }

        /// <summary>Freeze mid-motion (stand-up pause). The stage holds its exact frame.</summary>
        public void SetFrozen(bool frozen) => _frozen = frozen;

        public void Show(bool visible)
        {
            if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
            if (!visible)
            {
                _live = false;
                CancelScene();
            }
        }

        // ------------------------------------------------------------------ scene playback (M-T3)

        /// <summary>Plays a resolved beat scene. <paramref name="onGoalPlayed"/> fires when a
        /// staged goal's playback completes (commit AND chalked — the ledger sorts them);
        /// <paramref name="onReveal"/> fires exactly once at the scene's payoff moment (the
        /// goal, the save, or scene end for territory shapes) — the causally honest instant
        /// for the chrome to speak; <paramref name="onComplete"/> when the script exhausts.
        /// Poll <see cref="ScenePlaying"/>.</summary>
        public void PlayScene(SceneSpec spec, Action<ScoreLedger.StagedGoal> onGoalPlayed,
            Action onReveal, Action onComplete)
            => PlayScene(spec, onGoalPlayed, onReveal, onComplete, null);

        public void PlayScene(SceneSpec spec, Action<ScoreLedger.StagedGoal> onGoalPlayed,
            Action onReveal, Action onComplete, Action<CountLedger.StagedCount> onCountPlayed)
        {
            StartScript(BuildBeatScript(spec), onGoalPlayed, onCountPlayed, onReveal, onComplete);
        }

        /// <summary>Phase 2C (PRD §9): executes a <see cref="TheaterScenePlan"/> — the same
        /// factual <paramref name="spec"/> <see cref="PlayScene"/> would have played, elaborated
        /// with the planner's grammar/pressure/spacing/lane choices (see
        /// <see cref="BuildBeatScript(SceneSpec, TheaterScenePlan?)"/> and
        /// <see cref="ApplyPlanShaping"/>). <paramref name="plan"/> never changes which step
        /// carries the marker, its route, its staged Goal/Count payload, or any step's duration —
        /// only tempo, chase, and the lane-axis ball coordinate on buildup (RoutePass) steps. The
        /// caller owns recording <paramref name="plan"/>'s <c>Signature</c> into its
        /// <see cref="TheaterSceneHistory"/> once it accepts the plan — this method does not.</summary>
        public void PlayPlannedScene(TheaterScenePlan plan, SceneSpec spec,
            Action<ScoreLedger.StagedGoal> onGoalPlayed, Action onReveal, Action onComplete)
            => PlayPlannedScene(plan, spec, onGoalPlayed, onReveal, onComplete, null);

        public void PlayPlannedScene(TheaterScenePlan plan, SceneSpec spec,
            Action<ScoreLedger.StagedGoal> onGoalPlayed, Action onReveal, Action onComplete,
            Action<CountLedger.StagedCount> onCountPlayed)
        {
            StartScript(BuildBeatScript(spec, plan), onGoalPlayed, onCountPlayed, onReveal, onComplete);
        }

        // TVS-H03: SetScoringActor used to live here — it only ever renamed an unrendered
        // GameObject.name, with no read-side connection to EnterStep/CompleteStep's route/
        // carrier selection (Phase 1A). Removed in favor of a real binding: a bound StagedGoal
        // (ScoreLedger.BindAnytimeScorer) now drives EnterStep's RoutePass case directly — see
        // BoundActorRouted below and the RoutePass case in EnterStep.

        /// <summary>Plays the final whistle sequence: pre-reveal hold → the plan's staged
        /// goal(s) as separately-timed sub-scenes → celebrate/collapse. The GREEN/DEAD slam
        /// and the final chrome reveal belong to the orchestrator at completion (TvLight sync).</summary>
        public void PlayFinalScene(SceneSpec spec, ScoreLedger.FinalPlan plan,
            Action<ScoreLedger.StagedGoal> onGoalPlayed, Action onComplete)
            => PlayFinalScene(spec, plan, spec.CountFinal, onGoalPlayed, null, onComplete);

        public void PlayFinalScene(SceneSpec spec, ScoreLedger.FinalPlan plan,
            CountLedger.FinalPlan? countPlan, Action<ScoreLedger.StagedGoal> onGoalPlayed,
            Action<CountLedger.StagedCount> onCountPlayed, Action onComplete)
        {
            StartScript(BuildFinalScript(spec, plan, countPlan), onGoalPlayed, onCountPlayed, null, onComplete);
        }

        /// <summary>The pending-loss window's kill scene: opponent buildup → shot launched →
        /// FROZEN at the suspension point, mid-flight. Holds until <see cref="ResumeSuspended"/>.</summary>
        public void SuspendKillShot(int variant)
        {
            StartScript(BuildKillShotScript(variant), null, null, null, null);
        }

        /// <summary>The suspended scene's continuation, chosen from the FINAL ticket-local grade
        /// (never WinProbAfter): Voided → cyan VOID dissolve, no goals; Won → the frozen shot
        /// resolves as a save, then the counter goal(s) the correction needs; Lost → the flight
        /// completes (chalked if the entry score already satisfied Lost), then corrections.</summary>
        public void ResumeSuspended(ScoreLedger.FinalPlan plan,
            Action<ScoreLedger.StagedGoal> onGoalPlayed, Action onComplete)
            => ResumeSuspended(plan, null, MarketKind.Moneyline, onGoalPlayed, null, onComplete);

        public void ResumeSuspended(ScoreLedger.FinalPlan plan, CountLedger.FinalPlan? countPlan,
            Action<ScoreLedger.StagedGoal> onGoalPlayed, Action<CountLedger.StagedCount> onCountPlayed,
            Action onComplete)
            => ResumeSuspended(plan, countPlan, MarketKind.Moneyline, onGoalPlayed, onCountPlayed, onComplete);

        public void ResumeSuspended(ScoreLedger.FinalPlan plan, CountLedger.FinalPlan? countPlan,
            MarketKind market, Action<ScoreLedger.StagedGoal> onGoalPlayed,
            Action<CountLedger.StagedCount> onCountPlayed, Action onComplete)
        {
            _suspendedAtShot = false;
            StartScript(BuildContinuationScript(plan, countPlan, market), onGoalPlayed, onCountPlayed, null, onComplete);
        }

        /// <summary>Abandons any active scene without completing it (cash-out, leg change).</summary>
        public void CancelScene()
        {
            _script = null;
            _stepIx = 0;
            _stepT = 0f;
            _stepEntered = false;
            _suspendedAtShot = false;
            _onGoalPlayed = null;
            _onCountPlayed = null;
            _onReveal = null;
            _revealFired = false;
            _onSceneComplete = null;
            _boundActorActive = false;
            _priorRouteDotActive = false;
            _markerActorActive = false;
        }

        private void StartScript(Step[] script, Action<ScoreLedger.StagedGoal> onGoalPlayed,
            Action<CountLedger.StagedCount> onCountPlayed, Action onReveal, Action onComplete)
        {
            _script = script;
            _stepIx = 0;
            _stepT = 0f;
            _stepEntered = false;
            _suspendedAtShot = false;
            _onGoalPlayed = onGoalPlayed;
            _onCountPlayed = onCountPlayed;
            _onReveal = onReveal;
            _revealFired = false;
            _onSceneComplete = onComplete;
            _boundActorActive = false;
            _priorRouteDotActive = false;
            _markerActorActive = false;
        }

        // ------------------------------------------------------------------ update

        private void Update()
        {
            if (!_live || _frozen) return;
            float dt = Time.deltaTime;
            UpdateFlash(dt);

            if (_script != null) UpdateScene(dt);
            else UpdateIdle(dt);
        }

        private void UpdateScene(float dt)
        {
            if (_suspendedAtShot) return; // the frozen shot — the window's dread

            if (!_stepEntered) EnterStep();
            Step s = _script[_stepIx];

            // Motion (real frame time — cosmetic): formations speak the scene's territory,
            // the ball travels its routed target, tempo scales everything.
            float speed = 1f + s.Tempo * 1.6f;
            _sceneTerr = Mathf.SmoothDamp(_sceneTerr, s.Terr, ref _sceneTerrVel, 0.45f / speed);
            LastTerritoryX = _sceneTerr;
            MoveActors(_sceneTerr, speed, s.Chase, s.AtkPicked, dt);

            Vector2 target = _routeDotIx >= 0 ? DotPos(_routeDotHome, _routeDotIx) : _stepBallLocal;
            _ballPos = Vector2.SmoothDamp(_ballPos, target, ref _ballVel, ballDamp / speed);
            ApplyPositions();

            // Story time (scaled — batch tests fast-forward through here by shrinking the
            // effective duration, exactly like SeatedHold does with TimeScaleOverride).
            _stepT += dt;
            if (_stepT < s.Dur * Mathf.Max(0f, timeScale)) return;

            CompleteStep(s);
            if (_suspendedAtShot) return; // MkSuspend holds ON its step until resumed

            _stepIx++;
            _stepT = 0f;
            _stepEntered = false;
            if (_stepIx >= _script.Length)
            {
                _script = null;
                FireReveal(); // scenes with no explicit payoff reveal at their end
                Action done = _onSceneComplete;
                _onGoalPlayed = null;
                _onCountPlayed = null;
                _onReveal = null;
                _onSceneComplete = null;
                done?.Invoke();
            }
        }

        /// <summary>Resolves the step's ball routing against the ACTUAL actors — the move must
        /// read as the attacking team passing it around, never a ball flying to nobody.</summary>
        private void EnterStep()
        {
            _stepEntered = true;
            Step s = _script[_stepIx];
            bool atkHome = s.AtkPicked == _homeAttacksRight;
            // Phase 2D: stash the OUTGOING step's routed actor (if it had one) before resetting
            // for the step we're about to enter — see _priorRouteDotActive's doc.
            if (_routeDotIx >= 0)
            {
                _priorRouteDotActive = true;
                _priorRouteDotHome = _routeDotHome;
                _priorRouteDotIx = _routeDotIx;
            }
            _routeDotIx = -1;

            switch (s.Route)
            {
                case RoutePass:
                {
                    // TVS-H03: a goal bound to a roster identity at plan time (see
                    // ScoreLedger.BindAnytimeScorer) routes to THAT exact actor — never spatial
                    // nearest-neighbor. This is the run immediately before the shot (RouteShot
                    // doesn't reassign the routed dot), so the bound actor is genuinely the one
                    // the ball is seen carrying into the shot, not just a name attached after
                    // the fact. Every other step (every other market, every non-reveal goal)
                    // takes the unbound branch unchanged.
                    if (s.Goal.HasBoundScorer)
                    {
                        Image[] boundDots = s.Goal.ScorerIsHome ? _homeDots : _awayDots;
                        // DotIndexFor is shared with SetBackedPlayer (§7.7). Two expressions that
                        // merely agreed today would let a future edit break the continuity contract
                        // silently — the marked actor drifting off the final-touch actor is exactly
                        // TVS-H03's "cosmetically correct, causally unconnected" failure returning.
                        int boundIx = DotIndexFor(s.Goal.ScorerRosterIndex,
                            boundDots != null ? boundDots.Length : 0);
                        _routeDotIx = boundIx;
                        _routeDotHome = s.Goal.ScorerIsHome;
                        _boundActorActive = boundIx >= 0;
                        _boundActorHome = s.Goal.ScorerIsHome;
                        _boundActorIx = boundIx;
                        ForwardRuns(atkHome, s.AtkPicked ? 1f : -1f);
                        break;
                    }
                    _boundActorActive = false;
                    Vector2 want = ToLocal(s.Ball);
                    int ix = NearestOutfield(atkHome, want, exclude: BallCarriedBy(atkHome) ? _carrierIx : -1);
                    _routeDotIx = ix;
                    _routeDotHome = atkHome;
                    // Off-ball: teammates near the move make short forward runs, not static jitter.
                    ForwardRuns(atkHome, s.AtkPicked ? 1f : -1f);
                    break;
                }
                case RouteBackLine:
                {
                    Vector2 want = ToLocal(s.Ball);
                    _routeDotIx = NearestBackLine(!atkHome, want);
                    _routeDotHome = !atkHome;
                    break;
                }
                case RouteShot:
                {
                    // Aim at the corner AWAY from the defending keeper — a shot, not a pass.
                    Vector2 authored = s.Ball;
                    bool rightGoal = authored.x > 0.5f;
                    Vector2 keeper = rightGoal
                        ? (_homeAttacksRight ? _akPos : _hkPos)
                        : (_homeAttacksRight ? _hkPos : _akPos);
                    authored.y = keeper.y > 0f ? 0.42f : 0.58f;
                    _stepBallLocal = ToLocal(authored);
                    break;
                }
                case RouteCorner:
                    _stepBallLocal = ToLocal(s.Ball);
                    break;
                case RouteKickoff:
                default:
                    _stepBallLocal = ToLocal(s.Ball);
                    break;
            }
        }

        /// <summary>End-of-step bookkeeping: markers fire, and possession follows the story so
        /// the idle loop resumes from a coherent carrier.</summary>
        private void CompleteStep(Step s)
        {
            switch (s.Marker)
            {
                case MkGoal:
                    FlashGoal(right: s.Ball.x > 0.5f, strong: s.Goal.Commits);
                    _onGoalPlayed?.Invoke(s.Goal);
                    FireReveal(); // the net ripple IS the beat's payoff
                    break;
                case MkSuspend:
                    _suspendedAtShot = true;
                    break;
                case MkSave:
                    KeeperLunge();
                    FireReveal(); // the save IS the near-miss payoff
                    break;
                case MkBlock:
                    // The block IS the near-miss payoff: a REAL defender (RouteBackLine, see
                    // BuildNearMissCore) already stopped it this step — the ball stays in play.
                    FireReveal();
                    break;
                case MkIntercept:
                    // The interception IS the near-miss payoff: possession changed BEFORE any
                    // shot — this script never carried a RouteShot step at all.
                    FireReveal();
                    break;
                case MkClearance:
                    // The clearance IS the near-miss payoff: a REAL defender (RouteBackLine)
                    // sends it well out of danger.
                    FireReveal();
                    break;
                case MkPost:
                    // The frame contact IS the near-miss payoff: no actor touches it
                    // (RouteAuthored deflection) — never a goal flash.
                    FireReveal();
                    break;
                case MkNearWide:
                    // Passing wide IS the near-miss payoff: no keeper contact, no actor touches
                    // it (RouteAuthored — never RouteShot's forced on-target aim).
                    FireReveal();
                    break;
                case MkVoid:
                    ApplyVoidTint();
                    break;
                case MkCorner:
                    FlashCorner();
                    _onCountPlayed?.Invoke(s.Count);
                    FireReveal();
                    break;
                case MkBooking:
                    FlashBooking();
                    _onCountPlayed?.Invoke(s.Count);
                    FireReveal();
                    break;
            }

            switch (s.Route)
            {
                case RoutePass:
                case RouteBackLine:
                    if (_routeDotIx >= 0)
                    {
                        _carrierHome = _routeDotHome;
                        _carrierIx = _routeDotIx;
                    }
                    break;
                case RouteKickoff:
                    // The side that just conceded collects at the center spot.
                    _carrierHome = s.AtkPicked != _homeAttacksRight;
                    _carrierIx = 4;
                    break;
                case RouteCorner:
                    _carrierHome = s.AtkPicked == _homeAttacksRight;
                    _carrierIx = NearestOutfield(_carrierHome, _stepBallLocal, exclude: -1);
                    break;
            }
        }

        private void FireReveal()
        {
            if (_revealFired) return;
            _revealFired = true;
            _onReveal?.Invoke();
        }

        private void UpdateIdle(float dt)
        {
            _pulse = Mathf.MoveTowards(_pulse, 0f, dt / Mathf.Max(0.01f, pulseDecay));
            _urgency = Mathf.MoveTowards(_urgency, 0f, dt * 0.5f);

            // Territory: the honest prob, plus the decaying beat impulse. The picked side
            // attacks right, so territoryX already speaks the pick's language.
            float terr = PitchLayout.TerritoryX(_prob) + _pulse * _pulseSign * pulseKick;
            terr = Mathf.Clamp(terr, 0.08f, 0.92f);
            _sceneTerr = terr; // scenes resume smoothly from wherever idle left the shape
            LastTerritoryX = terr;

            float speed = 1f + _urgency * 1.4f;
            MoveActors(terr, speed, chase: false, atkPicked: terr >= 0.5f, dt);

            // Sticky possession: the carrier keeps it, passes go to TEAMMATES, and turnovers
            // are visible interceptions. Long-run possession share restates the live prob
            // (allowed: it repeats revealed state) — nothing here ever implies a goal.
            if (Time.time >= _nextPassAt)
            {
                _nextPassAt = Time.time + NextPassDelay() / speed;
                float share = Mathf.Lerp(0.25f, 0.75f, _prob); // picked side's possession target
                bool carrierPicked = _carrierHome == _homeAttacksRight;
                float keep = carrierPicked
                    ? Mathf.Lerp(0.55f, 0.92f, share)
                    : Mathf.Lerp(0.55f, 0.92f, 1f - share);
                if (_rng.NextDouble() < keep) PassToTeammate(terr);
                else TurnoverAtBall();
            }

            _ballPos = Vector2.SmoothDamp(_ballPos, DotPos(_carrierHome, _carrierIx), ref _ballVel, ballDamp / speed);
            ApplyPositions();
        }

        /// <summary>A pass within the team in possession: best of three candidates, preferring
        /// a comfortable pass length and progress toward the territory point.</summary>
        private void PassToTeammate(float terr)
        {
            Vector2 from = DotPos(_carrierHome, _carrierIx);
            float terrX = (terr - 0.5f) * (_w - Pad * 2f);
            int best = -1;
            float bestScore = float.NegativeInfinity;
            for (int tries = 0; tries < 3; tries++)
            {
                int c = _rng.Next(PitchLayout.OutfieldPerTeam);
                if (c == _carrierIx) continue;
                Vector2 p = DotPos(_carrierHome, c);
                float dist = Vector2.Distance(from, p);
                if (dist < 24f) continue; // no toe-pokes to a dot standing on the ball
                float lengthScore = -Mathf.Abs(dist - 120f) * 0.01f;          // comfortable pass
                float progressScore = -Mathf.Abs(p.x - terrX) * 0.008f;       // toward the action
                float score = lengthScore + progressScore;
                if (score > bestScore) { bestScore = score; best = c; }
            }
            if (best >= 0) _carrierIx = best;
        }

        /// <summary>A visible turnover: the nearest opponent steps INTO the ball and takes it —
        /// possession flips because somebody won it, not because the script blinked.</summary>
        private void TurnoverAtBall()
        {
            bool interceptorHome = !_carrierHome;
            int ix = NearestOutfield(interceptorHome, _ballPos, exclude: -1);
            ref Vector2 vel = ref (interceptorHome ? ref _homeVel[ix] : ref _awayVel[ix]);
            Vector2 toBall = _ballPos - DotPos(interceptorHome, ix);
            vel += toBall.normalized * Mathf.Min(toBall.magnitude * 4f, 220f);
            _carrierHome = interceptorHome;
            _carrierIx = ix;
        }

        /// <summary>Formation motion shared by idle and scenes: each dot has its own lagged
        /// territory response and noise clock, while the nearest defenders engage the carrier
        /// goal-side instead of letting the midfield line watch the move from a rigid block.</summary>
        private void MoveActors(float terr, float speed, bool chase, bool atkPicked, float dt)
        {
            float bias = (terr - 0.5f) * 2f; // [-1, 1] toward the right goal (picked frame)

            // Per-team posture: positive = on the front foot, negative = under siege.
            float homeAtk = _homeAttacksRight ? bias : -bias;
            float awayAtk = -homeAtk;
            bool inScene = _script != null;
            bool defendingHome = inScene ? atkPicked != _homeAttacksRight : !_carrierHome;
            int engaged1, engaged2, engaged3;
            FindNearestThree(defendingHome, _ballPos, out engaged1, out engaged2, out engaged3);
            int engagementCount = inScene ? (chase ? 2 : 3) : 1;
            float engagementStrength = inScene ? 1f : 0.45f;

            UpdateNoise(_homeNoise, _homeNoiseTimer, dt);
            UpdateNoise(_awayNoise, _awayNoiseTimer, dt);

            for (int i = 0; i < PitchLayout.OutfieldPerTeam; i++)
            {
                float homeBias = Mathf.SmoothDamp(_homeShapeBias[i], homeAtk,
                    ref _homeShapeBiasVel[i], ShapeDamp(true, i, speed), 4f, dt);
                float awayBias = Mathf.SmoothDamp(_awayShapeBias[i], awayAtk,
                    ref _awayShapeBiasVel[i], ShapeDamp(false, i, speed), 4f, dt);
                _homeShapeBias[i] = homeBias;
                _awayShapeBias[i] = awayBias;

                bool homeEngages = defendingHome && engagementCount > 0
                    && IsEngaged(i, engaged1, engaged2, engaged3, engagementCount);
                bool awayEngages = !defendingHome && engagementCount > 0
                    && IsEngaged(i, engaged1, engaged2, engaged3, engagementCount);
                Vector2 ht = homeEngages
                    ? DefensiveEngagementTarget(true, RankOf(i, engaged1, engaged2, engaged3), engagementStrength)
                    : ToLocal(PitchLayout.FormationSlot(i, _homeAttacksRight, homeBias,
                        Mathf.Clamp01(-homeBias) * 0.8f)) + _homeNoise[i];
                Vector2 at = awayEngages
                    ? DefensiveEngagementTarget(false, RankOf(i, engaged1, engaged2, engaged3), engagementStrength)
                    : ToLocal(PitchLayout.FormationSlot(i, !_homeAttacksRight, awayBias,
                        Mathf.Clamp01(-awayBias) * 0.8f)) + _awayNoise[i];
                _homePos[i] = Vector2.SmoothDamp(_homePos[i], ht, ref _homeVel[i],
                    PositionDamp(true, i, speed), MaxDotSpeed(speed), dt);
                _awayPos[i] = Vector2.SmoothDamp(_awayPos[i], at, ref _awayVel[i],
                    PositionDamp(false, i, speed), MaxDotSpeed(speed), dt);
            }
            _hkPos = Vector2.SmoothDamp(_hkPos, ToLocal(PitchLayout.Keeper(_homeAttacksRight)),
                ref _hkVel, dotDamp / speed, MaxDotSpeed(speed), dt);
            _akPos = Vector2.SmoothDamp(_akPos, ToLocal(PitchLayout.Keeper(!_homeAttacksRight)),
                ref _akVel, dotDamp / speed, MaxDotSpeed(speed), dt);
        }

        private float PositionDamp(bool home, int index, float speed)
            => Mathf.Max(0.01f, dotDamp * ReactionLag(home, index) / speed);

        private float ShapeDamp(bool home, int index, float speed)
        {
            // The back line absorbs shape changes last; midfielders carry the wave and
            // forwards can make the quickest read of a territory shift.
            float lineFactor = PitchLayout.IsBackLine(index) ? 1.15f : index < 6 ? 1f : 0.85f;
            return Mathf.Max(0.01f, dotDamp * ReactionLag(home, index) * lineFactor / speed);
        }

        private float ReactionLag(bool home, int index)
            => home ? _homeReactionLag[index] : _awayReactionLag[index];

        private float MaxDotSpeed(float speed)
            => Mathf.Lerp(300f, 380f, Mathf.Clamp01((speed - 1f) / 1.6f));

        private static bool IsEngaged(int index, int first, int second, int third, int count)
            => index == first || (count > 1 && index == second) || (count > 2 && index == third);

        private static int RankOf(int index, int first, int second, int third)
            => index == first ? 0 : index == second ? 1 : index == third ? 2 : 0;

        private Vector2 DefensiveEngagementTarget(bool defendingHome, int rank, float strength)
        {
            bool attacksRight = defendingHome ? _homeAttacksRight : !_homeAttacksRight;
            float ownGoalX = ToLocal(PitchLayout.Keeper(attacksRight)).x;
            float towardOwnGoal = Mathf.Sign(ownGoalX - _ballPos.x);
            if (Mathf.Abs(towardOwnGoal) < 0.01f) towardOwnGoal = attacksRight ? -1f : 1f;

            float distance = Mathf.Lerp(76f, 60f, strength);
            float laneOffset = rank == 0 ? 0f : (rank == 1 ? 18f : -18f);
            Vector2 target = _ballPos + new Vector2(towardOwnGoal * distance, laneOffset);
            float xLimit = (_w - Pad * 2f) * 0.5f;
            float yLimit = (_h - Pad * 2f) * 0.5f;
            return new Vector2(Mathf.Clamp(target.x, -xLimit, xLimit), Mathf.Clamp(target.y, -yLimit, yLimit));
        }

        private void UpdateNoise(Vector2[] noise, float[] timers, float dt)
        {
            for (int i = 0; i < noise.Length; i++)
            {
                timers[i] -= dt;
                if (timers[i] > 0f) continue;
                RerollNoise(ref noise[i]);
                timers[i] = Rand(0.8f, 2.2f);
            }
        }

        // ------------------------------------------------------------------ scene scripts

        /// <summary>Variant lanes: which flank the move runs down (EventText's variant trick).</summary>
        private static float Lane(int variant) => variant == 0 ? 0.5f : variant == 1 ? 0.32f : 0.68f;

        /// <summary>Absolute (non-fractional) step seconds, honoring the presentation tempo.</summary>
        private float P(float seconds) => seconds * Mathf.Max(0.01f, paceScale);

        private static Step S(float dur, float bx, float by, float terr, float tempo,
            byte marker = MkNone, ScoreLedger.StagedGoal goal = default, byte route = RoutePass,
            bool atkPicked = true, bool chase = false, CountLedger.StagedCount count = default)
            => new Step
            {
                Dur = dur, Ball = new Vector2(bx, by), Terr = terr, Tempo = tempo,
                Marker = marker, Goal = goal, Count = count, Route = route,
                AtkPicked = atkPicked, Chase = chase,
            };

        /// <summary>Single-step mirror — final/continuation goal reveals are authored in one
        /// frame and flipped per goal when the scorer is the other side (Sol, F_0.4.0 P3 r2:
        /// AtkPicked alone routed the actors but left the ball driving at the wrong goal).</summary>
        private static Step MirrorStep(Step s)
        {
            s.Ball = new Vector2(1f - s.Ball.x, s.Ball.y);
            s.Terr = 1f - s.Terr;
            s.AtkPicked = !s.AtkPicked;
            return s;
        }

        /// <summary>Mirrors a picked-frame script across the halfway line and hands the move to
        /// the OTHER team (for/against pairs share one author).</summary>
        private static Step[] Mirror(Step[] steps)
        {
            var m = new Step[steps.Length];
            for (int i = 0; i < m.Length; i++)
            {
                m[i] = steps[i];
                m[i].Ball = new Vector2(1f - m[i].Ball.x, m[i].Ball.y);
                m[i].Terr = 1f - m[i].Terr;
                m[i].AtkPicked = !m[i].AtkPicked;
            }
            return m;
        }

        /// <summary>Phase 2C: <paramref name="plan"/> null keeps every prior caller's exact
        /// behavior (§7.1: "the current Variant remains a compatibility input during migration"
        /// — <see cref="PlayScene(SceneSpec, Action{ScoreLedger.StagedGoal}, Action, Action)"/>
        /// still drives lane from <c>spec.Variant</c> alone). Non-null (from
        /// <see cref="PlayPlannedScene(TheaterScenePlan, SceneSpec, Action{ScoreLedger.StagedGoal}, Action, Action)"/>)
        /// drives lane from the plan's own independently-chosen <see cref="SceneLane"/> instead,
        /// and shapes the built script's tempo/chase/lane-axis via <see cref="ApplyPlanShaping"/>
        /// once the template switch below has produced its truth-authored waypoints.</summary>
        private Step[] BuildBeatScript(SceneSpec spec, TheaterScenePlan? plan = null)
        {
            float lane = plan.HasValue ? LaneOf(plan.Value.Lane) : Lane(spec.Variant);
            float T = spec.Duration;
            float u = spec.Urgent ? 1f : 0f;
            var goal = spec.Goal ?? default;
            bool commits = spec.Goal.HasValue && spec.Goal.Value.Commits;

            // The #9 LeadChange intro composes a steal step in front and squeezes the base.
            float intro = spec.LeadChangeIntro ? Mathf.Min(1.0f, T * 0.18f) : 0f;
            float B = T - intro;

            Step[] core;
            switch (spec.Template)
            {
                case SceneTemplate.GoalFor:
                case SceneTemplate.GoalAgainst:
                {
                    // Phase 2E-3 (PRD §7.1/§10): the chance shape (HOW the final ball into the
                    // box is delivered) is now its own segment — BuildChanceShapeDelivery,
                    // inserted between the grammar buildup (how the ball reaches the final
                    // third) and the truth-authored shot/marker/restart tail. Sequential
                    // composition, exactly like grammar + payoff already compose in Phase 2E-2/
                    // 2E-1, never a (grammar x chance shape) product. ChanceShape null (legacy
                    // plan-free PlayScene) falls back to Direct — BuildChanceShapeDelivery's own
                    // simplest default. Grammar null still falls back to Central as before; the
                    // grammar buildup's own budget shrinks from 0.54 to 0.40 to make room for the
                    // new 0.14 delivery segment, so the pre-shot total (0.54) is unchanged.
                    MovementGrammar? grammar = plan.HasValue ? plan.Value.Grammar : (MovementGrammar?)null;
                    ChanceShape? chanceShape = plan.HasValue ? plan.Value.ChanceShape : (ChanceShape?)null;
                    Step[] buildup = BuildGrammarBuildup(grammar ?? MovementGrammar.Central, B * 0.40f,
                        0.56f, 0.90f, lane, 0.60f, 0.72f, u, truthChase: false);
                    Step[] delivery = BuildChanceShapeDelivery(chanceShape, B * 0.14f, 0.90f, lane,
                        0.735f, u, chase: false);
                    // Celebrate (plan.Reaction) is only ever legal on a COMMITTING goal (§7.2's
                    // "forbidden implication" bars it from ChalkedGoal, and the planner's catalog
                    // never offers it there — guarded here too in case a hand-built plan
                    // disagrees) — a visible cluster near the goal before the walk-back, its own
                    // beat rather than folding straight into the restart. Every other reaction
                    // (Step/Chase/Drop/Recover) keeps the exact single-step restart this template
                    // always played — those four already arrive via the Pressure -> tempo/chase
                    // coupling ApplyPlanShaping applies below, not via anything reaction-specific
                    // here (verified against TheaterScenePlanner.PrimaryReactionFor/ReactionsFor:
                    // Step/Chase/Drop are each the exclusive primary for exactly one PressureMode).
                    bool celebrate = commits && plan.HasValue && plan.Value.Reaction == ReactionPattern.Celebrate;
                    Step[] tail = celebrate
                        ? new[]
                          {
                              S(B * 0.12f, 0.985f, 0.5f, 0.72f, 1f, MkGoal, goal, RouteShot),
                              S(B * 0.12f, 0.60f, 0.5f, 0.66f, 0f, route: RouteAuthored), // the cluster
                              S(B * 0.22f, 0.5f, 0.5f, 0.55f, 0.4f, route: RouteKickoff), // then the walk-back
                          }
                        : new[]
                          {
                              S(B * 0.12f, 0.985f, 0.5f, 0.72f, 1f, MkGoal, goal, RouteShot),
                              // The long restart tail is deliberate (playtest #15): the reveal fires
                              // at the goal (66% in), so this whole walk-back plays with the market OPEN.
                              commits
                                  ? S(B * 0.34f, 0.5f, 0.5f, 0.55f, 0.4f, route: RouteKickoff)
                                  : S(B * 0.34f, 0.84f, 0.30f, 0.60f, 0.3f, route: RouteBackLine), // chalked: defenders restart
                          };
                    core = Concat(Concat(buildup, delivery), tail);
                    if (spec.Template == SceneTemplate.GoalAgainst) core = Mirror(core);
                    break;
                }

                case SceneTemplate.BreakawayFor:
                case SceneTemplate.BreakawayAgainst:
                {
                    // Phase 2E-3: the same chance-shape-delivery composition as Goal above.
                    // Breakaway's grammar fallback stays Counter (Phase 2E-2's reasoning: "the
                    // grammar this template already WAS"); chance shape falls back to Direct,
                    // same as every other path. The grammar buildup's own budget shrinks from
                    // 0.56 to 0.42 to make room for the new 0.14 delivery segment, so the
                    // pre-shot total (0.56) is unchanged.
                    MovementGrammar? grammar = plan.HasValue ? plan.Value.Grammar : (MovementGrammar?)null;
                    ChanceShape? chanceShape = plan.HasValue ? plan.Value.ChanceShape : (ChanceShape?)null;
                    Step[] buildup = BuildGrammarBuildup(grammar ?? MovementGrammar.Counter, B * 0.42f,
                        0.30f, 0.88f, lane, 0.42f, 0.66f, u, truthChase: true);
                    Step[] delivery = BuildChanceShapeDelivery(chanceShape, B * 0.14f, 0.88f, lane,
                        0.68f, u, chase: true);
                    float shotY = spec.Variant == 2 ? 0.58f : 0.42f;
                    // Celebrate, same guard and reasoning as Goal above.
                    bool celebrate = commits && plan.HasValue && plan.Value.Reaction == ReactionPattern.Celebrate;
                    Step[] tail = celebrate
                        ? new[]
                          {
                              S(B * 0.12f, 0.965f, shotY, 0.70f, 1f, MkGoal, goal, RouteShot),
                              S(B * 0.10f, 0.60f, 0.5f, 0.64f, 0f, route: RouteAuthored),
                              S(B * 0.22f, 0.5f, 0.5f, 0.55f, 0.4f, route: RouteKickoff),
                          }
                        : new[]
                          {
                              S(B * 0.12f, 0.965f, shotY, 0.70f, 1f, MkGoal, goal, RouteShot),
                              commits
                                  ? S(B * 0.32f, 0.5f, 0.5f, 0.55f, 0.4f, route: RouteKickoff)
                                  : S(B * 0.32f, 0.84f, 0.30f, 0.60f, 0.3f, route: RouteBackLine),
                          };
                    core = Concat(Concat(buildup, delivery), tail);
                    if (spec.Template == SceneTemplate.BreakawayAgainst) core = Mirror(core);
                    break;
                }

                case SceneTemplate.CornerFor:
                case SceneTemplate.CornerAgainst:
                {
                    // Phase 2D (PRD §7.6): NearPost/FarPost/Cleared are three visibly distinct
                    // authored sequences (see BuildCornerCore), not one sequence a signature
                    // varies. A plan supplies the grammar; the legacy plan:null PlayScene path
                    // (§7.1 compatibility) still gets the full drive-in -> ball-out-off-the-
                    // defender -> delivery beat structure, it just has no grammar to pick among
                    // the three shapes, so it falls back to the NearPost shape. The count
                    // callback still fires at the kick (MkCorner), not when the approach begins.
                    MovementGrammar? grammar = plan.HasValue ? plan.Value.Grammar : (MovementGrammar?)null;
                    core = BuildCornerCore(grammar, B, lane, u, spec.Count ?? default);
                    // TVS-S01 follow-up (reviewer correction): CornerFor/CornerAgainst is the
                    // bettor's hope/dread MOOD (selection-derived, TheaterChoreographer) — a
                    // SEPARATE concept from which team physically wins the corner. Mirroring off
                    // the template would put routing back on the bet (the original TVS-S01 bug).
                    // Routing reads the staged fact directly instead. The ?? true fallback only
                    // matters for a scene built with no staged count at all (e.g. a bare
                    // template-completion test); every real Corner scene from ResolveBeat always
                    // sets this field.
                    if (!(spec.CountBeneficiaryIsHome ?? true)) core = Mirror(core);
                    break;
                }

                case SceneTemplate.Booking:
                {
                    // TVS-S01 fix (PRD §7.6): Booking is a direction-neutral template (no
                    // For/Against split), so it reads which team commits the foul from the
                    // staged fact — CountBeneficiaryIsHome — never from ForPicked (which is
                    // incoherent for a totals market with no picked team). The ?? true fallback
                    // only matters for a scene built with no staged count at all (e.g. a bare
                    // template-completion test); every real Booking scene from ResolveBeat
                    // always sets this field. Phase 2D adds the visible challenge beat and moves
                    // the marker onto that side's actor (see BuildBookingCore) — team selection
                    // itself is unchanged.
                    bool bookingAttacksHome = spec.CountBeneficiaryIsHome ?? true;
                    core = BuildBookingCore(bookingAttacksHome, B, lane, spec.Count ?? default);
                    break;
                }

                case SceneTemplate.TerritoryFor:
                case SceneTemplate.TerritoryAgainst:
                {
                    // Phase 2E-2 (PRD §7.2 Territory row: "central recycle, wing progression,
                    // switch, controlled counter start" — a possession vocabulary, not the
                    // chance-approach one Goal/Breakaway/NearMiss share, and SetPiece is not
                    // legal here at all). This template's own x/territory/tempo pacing is
                    // unchanged; only the lane axis (PossessionLanePattern) is grammar-driven.
                    // Grammar null falls back to Switch — this shape (lane -> 1-lane -> center)
                    // is exactly what Territory has always played.
                    MovementGrammar? grammar = plan.HasValue ? plan.Value.Grammar : (MovementGrammar?)null;
                    PossessionLanePattern(grammar, lane, out float ty1, out float ty2, out float ty3);
                    core = new[]
                    {
                        S(B * 0.35f, 0.55f, ty1, 0.56f, Mathf.Max(0.5f, u)),
                        S(B * 0.35f, 0.62f, ty2, 0.62f, Mathf.Max(0.5f, u)),
                        S(B * 0.30f, 0.60f, ty3, 0.62f, 0.4f),
                    };
                    if (spec.Template == SceneTemplate.TerritoryAgainst) core = Mirror(core);
                    break;
                }

                case SceneTemplate.NearMissHope:
                case SceneTemplate.NearMissScare:
                {
                    // Phase 2E-1 (PRD §7.2/§7.3/§10): six visibly distinct authored non-goal
                    // endings, chosen by plan.Payoff (BuildNearMissCore), not one keeper-save
                    // shape rendering regardless of which of the six the planner chose. The
                    // legacy plan-null PlayScene path has no payoff to pick among and falls back
                    // to KeeperSave — the EXACT shape this template always rendered before this
                    // phase (unlike Corner's plan-null fallback, which had to move OFF its old
                    // uncaused shape to fix a defect; near miss's old shape was never wrong, just
                    // the only one that ever played, so a plan-free near miss's on-screen
                    // appearance is unchanged by this phase).
                    //
                    // Phase 2E-2: the pre-marker HEAD (approach steps, before the shot/interception)
                    // is now the shared grammar buildup (BuildGrammarBuildup) — grammar null falls
                    // back to Central, same default as Goal/Breakaway.
                    // Phase 2E-3: chance shape and reaction are threaded through the same way —
                    // see BuildNearMissCore for how each payoff's own delivery budget and
                    // Collapse-reaction handling work.
                    ScenePayoff? payoff = plan.HasValue ? plan.Value.Payoff : (ScenePayoff?)null;
                    MovementGrammar? grammar = plan.HasValue ? plan.Value.Grammar : (MovementGrammar?)null;
                    ChanceShape? chanceShape = plan.HasValue ? plan.Value.ChanceShape : (ChanceShape?)null;
                    ReactionPattern? reaction = plan.HasValue ? plan.Value.Reaction : (ReactionPattern?)null;
                    core = BuildNearMissCore(payoff, grammar, chanceShape, reaction, B, lane);
                    if (spec.Template == SceneTemplate.NearMissScare) core = Mirror(core);
                    break;
                }

                case SceneTemplate.CalmPossession:
                {
                    // Phase 2E-2: shares Territory's lane-axis grammar vocabulary — this template
                    // never had a `lane` input pre-Phase-2E-2 (always 0.40/0.60/0.48 regardless of
                    // Variant/plan.Lane); passing 0.40 as PossessionLanePattern's own `lane`
                    // reproduces that exact pre-existing shape for the Switch/null fallback.
                    MovementGrammar? grammar = plan.HasValue ? plan.Value.Grammar : (MovementGrammar?)null;
                    PossessionLanePattern(grammar, 0.40f, out float cy1, out float cy2, out float cy3);
                    core = new[]
                    {
                        S(B * 0.35f, 0.46f, cy1, 0.50f, 0.2f, atkPicked: spec.ForPicked),
                        S(B * 0.35f, 0.54f, cy2, 0.50f, 0.2f, atkPicked: spec.ForPicked),
                        S(B * 0.30f, 0.48f, cy3, 0.50f, 0.15f, atkPicked: spec.ForPicked),
                    };
                    break;
                }

                case SceneTemplate.Kickoff:
                    core = new[] { S(B, 0.5f, 0.5f, 0.5f, 0.3f, route: RouteKickoff, atkPicked: spec.ForPicked) };
                    break;

                case SceneTemplate.LegFinalWon:
                case SceneTemplate.LegFinalLost:
                    // The orchestrator should route finals through PlayFinalScene; keep the
                    // resolver total anyway with a plan-free final shape.
                    return BuildFinalScript(spec, new ScoreLedger.FinalPlan(
                        spec.Template == SceneTemplate.LegFinalWon ? LegGrade.Won : LegGrade.Lost,
                        Array.Empty<ScoreLedger.StagedGoal>()));

                default: // #15 fallback: a neutral attack fizzles out
                    core = new[]
                    {
                        S(B * 0.40f, 0.60f, lane, 0.55f, 0.5f, atkPicked: spec.ForPicked),
                        S(B * 0.30f, 0.72f, 1f - lane, 0.58f, 0.5f, atkPicked: spec.ForPicked),
                        S(B * 0.30f, 0.50f, 0.5f, 0.52f, 0.3f, atkPicked: spec.ForPicked),
                    };
                    break;
            }

            // Phase 2C: shape the template's already-truth-authored waypoints with the planner's
            // choices — after the switch above (so every template's Marker/Route/Goal/Count/
            // duration stays exactly as authored) and before the #9 intro is composed (so a
            // LeadChange intro, when one plays, still starts from the SAME base `lane` the shaped
            // core now varies around).
            if (plan.HasValue) core = ApplyPlanShaping(core, plan.Value);

            if (intro <= 0f) return core;

            // Steal → transition: the ball flips flanks at midfield before the move starts.
            var withIntro = new Step[core.Length + 1];
            withIntro[0] = S(intro, 0.42f, 1f - lane, 0.46f, 1f, atkPicked: core[0].AtkPicked);
            Array.Copy(core, 0, withIntro, 1, core.Length);
            return withIntro;
        }

        /// <summary>Phase 2D (PRD §7.6): three visibly distinct authored corner sequences, not
        /// one sequence a signature varies. Every shape follows the same causal beats the PRD
        /// requires — drive into the attacking third (RoutePass), the ball going out off the
        /// DEFENDING side (RouteBackLine, a real actor, never an authored point), then the
        /// delivery from the beneficiary's attacking corner (MkCorner/RouteCorner) — and differs
        /// visibly in HOW the corner is won and WHERE the delivery goes:
        /// <see cref="MovementGrammar.NearPost"/> wins it wide and delivers short to the near
        /// side; <see cref="MovementGrammar.FarPost"/> wins it wide too but swings the delivery
        /// across the whole box to the far side (<c>1 - lane</c> instead of <c>lane</c>);
        /// <see cref="MovementGrammar.Cleared"/> cuts inside instead of going wide, and is
        /// conceded by a hurried, heavily-chased last-ditch hack behind rather than a controlled
        /// dispossession. <paramref name="grammar"/> is null only for the legacy plan-free
        /// <see cref="PlayScene(SceneSpec, Action{ScoreLedger.StagedGoal}, Action, Action)"/>
        /// path, which has no grammar to pick among the three shapes and falls back to NearPost's
        /// (still fully attributed) shape — every branch here is built in the PICKED frame and
        /// mirrored afterward by the caller, never baking home/away in directly (TVS-S01's
        /// guard). Every shape's step-duration fractions sum to 1, so the scene's total authored
        /// duration is identical regardless of which shape plays.</summary>
        private static Step[] BuildCornerCore(MovementGrammar? grammar, float B, float lane, float u,
            CountLedger.StagedCount count)
        {
            switch (grammar)
            {
                case MovementGrammar.FarPost:
                    return new[]
                    {
                        S(B * 0.22f, 0.40f, lane, 0.48f, Mathf.Max(0.5f, u), chase: true),
                        S(B * 0.22f, 0.80f, lane, 0.66f, Mathf.Max(0.8f, u), chase: true),
                        // Won it back off the defending side — a real defender, not a point.
                        S(B * 0.14f, 0.94f, lane, 0.70f, 0.9f, route: RouteBackLine, chase: true),
                        // The delivery swings all the way across the box to the far post.
                        S(B * 0.18f, 0.985f, 1f - lane, 0.74f, 1f, MkCorner, route: RouteCorner, count: count),
                        S(B * 0.24f, 0.70f, 0.55f, 0.56f, 0.35f, route: RouteBackLine),
                    };

                case MovementGrammar.Cleared:
                    return new[]
                    {
                        // Cuts inside rather than going wide first.
                        S(B * 0.24f, 0.44f, lane, 0.50f, Mathf.Max(0.55f, u), chase: true),
                        S(B * 0.20f, 0.82f, 0.5f, 0.68f, Mathf.Max(0.85f, u), chase: true),
                        // Cleared's distinct beat: a hurried, heavily-pressed last-ditch hack
                        // behind — the corner is conceded, not won by a clean dispossession.
                        S(B * 0.10f, 0.92f, lane, 0.72f, 1f, route: RouteBackLine, chase: true),
                        S(B * 0.18f, 0.975f, lane, 0.74f, 1f, MkCorner, route: RouteCorner, count: count),
                        S(B * 0.28f, 0.68f, 0.60f, 0.55f, 0.30f, route: RouteBackLine),
                    };

                case MovementGrammar.NearPost:
                default:
                    return new[]
                    {
                        S(B * 0.30f, 0.42f, lane, 0.50f, Mathf.Max(0.5f, u), chase: true),
                        S(B * 0.26f, 0.84f, lane, 0.68f, Mathf.Max(0.8f, u), chase: true),
                        // Won it back off the defending side — a real defender, not a point.
                        S(B * 0.12f, 0.94f, lane, 0.71f, 0.85f, route: RouteBackLine, chase: true),
                        // The delivery stays short, near side.
                        S(B * 0.14f, 0.97f, lane, 0.72f, 1f, MkCorner, route: RouteCorner, count: count),
                        S(B * 0.18f, 0.72f, 0.58f, 0.56f, 0.35f, route: RouteBackLine),
                    };
            }
        }

        /// <summary>Phase 2D (PRD §7.6): the booking sequence, with a visible challenge beat
        /// BEFORE the marker (a tight, high-tempo closing-down step distinct from the buildup
        /// that precedes it, so the card never appears out of nowhere) and the marker step routed
        /// via <see cref="RoutePass"/> (not <see cref="RouteAuthored"/>) so it resolves to a real
        /// actor on <paramref name="bookingAttacksHome"/>'s side — see <see cref="FlashBooking"/>,
        /// which reads that resolved actor to place the card on them instead of at the ball's raw
        /// coordinate. Team selection itself (<paramref name="bookingAttacksHome"/>, sourced from
        /// <c>CountBeneficiaryIsHome</c> by the caller) is unchanged from before Phase 2D.</summary>
        private static Step[] BuildBookingCore(bool bookingAttacksHome, float B, float lane,
            CountLedger.StagedCount count)
            => new[]
            {
                S(B * 0.28f, 0.46f, lane, 0.50f, 0.45f, atkPicked: bookingAttacksHome),
                S(B * 0.22f, 0.58f, lane, 0.54f, 0.85f, chase: true, atkPicked: bookingAttacksHome),
                // The visible challenge itself: a tight, high-tempo closing-down beat.
                S(B * 0.14f, 0.60f, lane, 0.52f, 1f, chase: true, atkPicked: bookingAttacksHome),
                S(B * 0.10f, 0.60f, lane, 0.50f, 0.2f, MkBooking, route: RoutePass,
                    atkPicked: bookingAttacksHome, count: count),
                S(B * 0.26f, 0.50f, 0.50f, 0.50f, 0.15f, route: RouteAuthored),
            };

        /// <summary>Phase 2E-1 (PRD §7.2/§7.3/§10): six visibly distinct authored non-goal
        /// endings, not one keeper-save shape a signature varies. Every shape is built in the
        /// PICKED frame and mirrored whole by the caller for <see cref="SceneTemplate.NearMissScare"/>
        /// — the payoff SHAPE (this method) and the bettor's hope/dread MOOD
        /// (NearMissHope/NearMissScare) stay independent, exactly like Corner's grammar and
        /// CornerFor/CornerAgainst mood do (TVS-S01's guard, restated for this phase). Each shape
        /// differs mechanically — not merely by signature — in its <c>Route</c> sequence, which
        /// actor (if any) the ball resolves to, and where the ball ends up:
        /// <list type="bullet">
        /// <item><description><see cref="ScenePayoff.Block"/>: the shot is struck
        /// (<see cref="RouteShot"/>), then a REAL defender steps into its path
        /// (<see cref="RouteBackLine"/>, <c>chase: true</c>) and the loose ball is recycled by
        /// the attack (<see cref="RoutePass"/>) — it stays in play, never a dead-air hold.</description></item>
        /// <item><description><see cref="ScenePayoff.Interception"/>: the ONLY shape with no
        /// <see cref="RouteShot"/> step anywhere — a REAL defender
        /// (<see cref="RouteBackLine"/>, <c>chase: true</c>) wins it back before any shot is
        /// struck, and the tail explicitly switches <c>AtkPicked</c> to the interceptor's side so
        /// the carry-away visibly reads as a possession change, settling neutrally rather than
        /// building toward a second chance (a turnover must never imply a counter-goal).</description></item>
        /// <item><description><see cref="ScenePayoff.KeeperSave"/>: unchanged from this
        /// template's pre-Phase-2E-1 shape (<see cref="MkSave"/> + <see cref="KeeperLunge"/>) —
        /// also the plan-null legacy fallback (<paramref name="payoff"/> null), so a plan-free
        /// near miss's on-screen appearance is exactly what it always was.</description></item>
        /// <item><description><see cref="ScenePayoff.Clearance"/>: the shot is struck, a REAL
        /// defender wins it (<see cref="RouteBackLine"/>, <c>chase: true</c>), then the tail ALSO
        /// routes via <see cref="RouteBackLine"/> — sent well out of danger, deep — distinct from
        /// Block's tail, which stays live via <see cref="RoutePass"/> instead.</description></item>
        /// <item><description><see cref="ScenePayoff.Post"/>: the shot is struck, then deflects
        /// off the frame via <see cref="RouteAuthored"/> — no actor resolves at the marker step at
        /// all (the frame is not a character) — never a goal flash.</description></item>
        /// <item><description><see cref="ScenePayoff.NearWide"/>: the ONLY shot-adjacent shape
        /// that never plays a <see cref="RouteShot"/> step — <see cref="RouteShot"/> forces an
        /// on-target aim (see its own doc), which would contradict "passes outside the post", so
        /// the strike is authored directly (<see cref="RouteAuthored"/>) to a point clearly
        /// outside the goal mouth band; no actor resolves at the marker step, and no keeper
        /// action fires (<see cref="MkNearWide"/> never invokes <see cref="KeeperLunge"/>).</description></item>
        /// </list>
        /// Every shape's step-duration fractions sum to 1, so the scene's total authored duration
        /// (<c>B</c>) is identical regardless of which payoff plays — the same invariant Phase
        /// 2D's <see cref="BuildCornerCore"/> holds for the three corner shapes.
        ///
        /// <para><b>Phase 2E-3 additions.</b> Each payoff's HEAD budget shrinks by exactly 0.12
        /// (e.g. Block/Clearance: 0.44 -> 0.32) to make room for a new
        /// <see cref="BuildChanceShapeDelivery"/> segment of that same 0.12, inserted between the
        /// head and the (byte-identical, untouched) tail — the total pre-tail budget is
        /// unchanged. Interception is the one payoff <see cref="ChanceShape.Rebound"/> cannot
        /// legally compose with (see that case's own comment for why and how it is handled).
        /// <see cref="ReactionPattern.Collapse"/> is legal only opposite
        /// <see cref="ScenePayoff.KeeperSave"/> (the planner's catalog never offers it to any
        /// other payoff), and only changes the marker-free aftermath of the save — see that
        /// case's <c>collapse</c> branch.</para></summary>
        private static Step[] BuildNearMissCore(ScenePayoff? payoff, MovementGrammar? grammar,
            ChanceShape? chanceShape, ReactionPattern? reaction, float B, float lane)
        {
            // Phase 2E-2 (PRD §7.2/§7.3/§10): the pre-marker HEAD (the approach before the shot,
            // or before the interception's own back-line marker) is now the shared grammar
            // buildup (see BuildGrammarBuildup) instead of two hand-fixed steps identical across
            // all six payoffs — every payoff's own TAIL (shot/marker/ending, Phase 2E-1) is
            // untouched. Grammar null falls back to Central, the same default Goal/Breakaway use.
            MovementGrammar g = grammar ?? MovementGrammar.Central;

            switch (payoff)
            {
                case ScenePayoff.Block:
                {
                    Step[] head = BuildGrammarBuildup(g, B * 0.32f, 0.60f, 0.84f, lane, 0.60f, 0.68f, 0.7f,
                        truthChase: false);
                    Step[] delivery = BuildChanceShapeDelivery(chanceShape, B * 0.12f, 0.84f, lane, 0.69f,
                        0.7f, chase: false);
                    Step[] tail =
                    {
                        S(B * 0.12f, 0.97f, 0.47f, 0.72f, 1f, route: RouteShot), // the shot is struck
                        // A REAL defender steps into its path — the ball stays in play.
                        S(B * 0.14f, 0.90f, 0.72f, 0.70f, 1f, MkBlock, route: RouteBackLine, chase: true),
                        S(B * 0.30f, 0.66f, 0.55f, 0.60f, 0.6f, route: RoutePass), // recycled, still live
                    };
                    return Concat(Concat(head, delivery), tail);
                }

                case ScenePayoff.Interception:
                {
                    // The only head with a TRUTH chase — defense visibly closing in is intrinsic
                    // to "a defender wins it before any shot", not a grammar-added flourish.
                    Step[] head = BuildGrammarBuildup(g, B * 0.40f, 0.58f, 0.78f, lane, 0.58f, 0.66f, 0.7f,
                        truthChase: true);
                    // Phase 2E-3: Rebound cannot compose with Interception. Rebound's own
                    // definition requires a shot to already have been struck (visibly blocked,
                    // before a different second touch) — Interception is the ONE payoff whose
                    // entire truth contract is "a defender wins it back BEFORE any shot is
                    // struck" (no RouteShot step exists anywhere in this shape, asserted below
                    // and by TheaterStageAttributionTests). Rendering Rebound here would either
                    // fabricate a shot that contradicts the payoff or silently drop Rebound's
                    // defining beat — neither is acceptable, so this is the one shape/payoff pair
                    // that genuinely cannot compose: Rebound falls back to Direct (the simplest
                    // of the five) for this payoff only. Every other chance shape composes
                    // normally, continuing the head's own truth chase through the delivery.
                    ChanceShape? effectiveShape = chanceShape == ChanceShape.Rebound ? ChanceShape.Direct : chanceShape;
                    Step[] delivery = BuildChanceShapeDelivery(effectiveShape, B * 0.12f, 0.78f, lane, 0.665f,
                        0.7f, chase: true);
                    Step[] tail =
                    {
                        // Won BEFORE any shot — no RouteShot step exists anywhere in this shape.
                        S(B * 0.16f, 0.82f, 0.55f, 0.66f, 1f, MkIntercept, route: RouteBackLine, chase: true),
                        // The interceptor's side visibly carries it away — a real possession flip.
                        S(B * 0.16f, 0.60f, 0.48f, 0.58f, 0.8f, route: RoutePass, atkPicked: false),
                        // Settles neutrally — never builds toward a second chance for the other side.
                        S(B * 0.16f, 0.45f, 0.50f, 0.50f, 0.3f, route: RouteAuthored, atkPicked: false),
                    };
                    return Concat(Concat(head, delivery), tail);
                }

                case ScenePayoff.Clearance:
                {
                    Step[] head = BuildGrammarBuildup(g, B * 0.32f, 0.60f, 0.84f, lane, 0.60f, 0.68f, 0.7f,
                        truthChase: false);
                    Step[] delivery = BuildChanceShapeDelivery(chanceShape, B * 0.12f, 0.84f, lane, 0.69f,
                        0.7f, chase: false);
                    Step[] tail =
                    {
                        S(B * 0.12f, 0.97f, 0.47f, 0.72f, 1f, route: RouteShot), // the shot / cross
                        // A REAL defender wins it and sends it well out of danger, deep.
                        S(B * 0.12f, 0.86f, 0.62f, 0.72f, 1f, MkClearance, route: RouteBackLine, chase: true),
                        S(B * 0.32f, 0.35f, 0.28f, 0.48f, 0.4f, route: RouteBackLine),
                    };
                    return Concat(Concat(head, delivery), tail);
                }

                case ScenePayoff.Post:
                {
                    Step[] head = BuildGrammarBuildup(g, B * 0.34f, 0.62f, 0.86f, lane, 0.62f, 0.68f, 0.7f,
                        truthChase: false);
                    Step[] delivery = BuildChanceShapeDelivery(chanceShape, B * 0.12f, 0.86f, lane, 0.69f,
                        0.7f, chase: false);
                    Step[] tail =
                    {
                        S(B * 0.12f, 0.99f, 0.47f, 0.72f, 1f, route: RouteShot), // the shot is struck
                        // Clatters off the frame — no actor touches it, never a goal flash.
                        S(B * 0.10f, 0.985f, 0.42f, 0.72f, 1f, MkPost, route: RouteAuthored),
                        S(B * 0.32f, 0.55f, 0.45f, 0.55f, 0.3f, route: RouteAuthored), // rebound drifts away
                    };
                    return Concat(Concat(head, delivery), tail);
                }

                case ScenePayoff.NearWide:
                {
                    Step[] head = BuildGrammarBuildup(g, B * 0.36f, 0.62f, 0.86f, lane, 0.62f, 0.68f, 0.7f,
                        truthChase: false);
                    Step[] delivery = BuildChanceShapeDelivery(chanceShape, B * 0.12f, 0.86f, lane, 0.69f,
                        0.7f, chase: false);
                    Step[] tail =
                    {
                        S(B * 0.14f, 0.97f, 0.80f, 0.74f, 1f, route: RoutePass), // the strike shapes up
                        // Dragged wide of the frame — never RouteShot's forced on-target aim, no
                        // actor touches it, no keeper contact.
                        S(B * 0.10f, 0.99f, 0.86f, 0.75f, 1f, MkNearWide, route: RouteAuthored),
                        S(B * 0.28f, 0.55f, 0.50f, 0.55f, 0.3f, route: RouteAuthored), // goal kick, drifts back
                    };
                    return Concat(Concat(head, delivery), tail);
                }

                case ScenePayoff.KeeperSave:
                default:
                {
                    Step[] head = BuildGrammarBuildup(g, B * 0.34f, 0.62f, 0.86f, lane, 0.62f, 0.68f, 0.7f,
                        truthChase: false);
                    Step[] delivery = BuildChanceShapeDelivery(chanceShape, B * 0.12f, 0.86f, lane, 0.69f,
                        0.7f, chase: false);
                    // Collapse (plan.Reaction) is legal only opposite KeeperSave — the planner's
                    // catalog never offers it to any other payoff. The shot and the save itself
                    // (MkSave) stay byte-identical either way; only the marker-free aftermath
                    // differs — Collapse lets the ball die near the shooter instead of bouncing
                    // clear, a longer, low-tempo hold reading as the attack's energy draining
                    // away. Every other reaction keeps the brisk recovery this template always
                    // played (Step/Chase/Drop/Recover arrive via the Pressure coupling instead —
                    // see ApplyPlanShaping).
                    bool collapse = reaction == ReactionPattern.Collapse;
                    Step[] tail = collapse
                        ? new[]
                          {
                              S(B * 0.12f, 0.99f, 0.47f, 0.72f, 1f, route: RouteShot),
                              S(B * 0.10f, 0.94f, 0.82f, 0.70f, 1f, MkSave, route: RouteAuthored),
                              S(B * 0.12f, 0.88f, 0.62f, 0.68f, 0f, route: RouteAuthored),    // dies near the shooter
                              S(B * 0.20f, 0.55f, 0.50f, 0.52f, 0.08f, route: RouteBackLine), // no urgency to recover it
                          }
                        : new[]
                          {
                              S(B * 0.12f, 0.99f, 0.47f, 0.72f, 1f, route: RouteShot),     // the shot
                              S(B * 0.10f, 0.94f, 0.82f, 0.70f, 1f, MkSave, route: RouteAuthored), // off the bar
                              S(B * 0.12f, 0.92f, 0.80f, 0.70f, 0f, route: RouteAuthored), // the hold — dead air
                              S(B * 0.20f, 0.60f, 0.35f, 0.58f, 0.3f, route: RouteBackLine), // cleared off the line
                          };
                    return Concat(Concat(head, delivery), tail);
                }
            }
        }

        /// <summary>Phase 2C: the plan's own independently-chosen <see cref="SceneLane"/> as a
        /// pitch-fraction, matching <see cref="Lane(int)"/>'s legacy value set exactly (0.5 / 0.32
        /// / 0.68) so a planned scene reads at the same on-screen widths a legacy-variant scene
        /// always has — only WHICH dimension chooses the lane changes, not what the lane values
        /// mean physically.</summary>
        private static float LaneOf(SceneLane lane)
            => lane == SceneLane.Center ? 0.5f : lane == SceneLane.NearFlank ? 0.32f : 0.68f;

        /// <summary>Phase 2C (PRD §7.1, §9): renders the planner's Pressure/Spacing choices as
        /// visible motion differences on top of a template's already-authored waypoints. Never
        /// changes which step carries the <c>Marker</c>, its <c>Route</c>, its staged
        /// <c>Goal</c>/<c>Count</c> payload, or any step's <c>Dur</c> — the template's truth
        /// contract (PRD §7.2) is exactly what the switch above already built; this only adjusts
        /// <c>Tempo</c>, adds <c>Chase</c> under high pressure, and nudges the lane-axis
        /// <c>Ball.y</c> by <c>Spacing</c> — and ONLY on <see cref="RoutePass"/> steps, the sole
        /// route whose target is resolved by nearest-outfield-dot-to-waypoint
        /// (<see cref="EnterStep"/>'s <see cref="RoutePass"/> case), so a small y nudge only ever
        /// changes which already-forming-up teammate a pass finds — it can never misdirect a shot
        /// (<see cref="RouteShot"/> recomputes its own y from the keeper's position, ignoring the
        /// authored value entirely), a restart, a corner arc, or a booking/back-line waypoint, all
        /// of which stay pixel-exact to how the template author placed them.
        ///
        /// Phase 2E-2 note: this method used to ALSO carry a per-<c>Grammar</c> lane-offset
        /// multiplier here — the thin stand-in Phase 2C shipped ("Grammar... not yet visible on
        /// screen", per that phase's own dispatch) while grammar was computed but not yet
        /// rendered. That stand-in is gone: <see cref="BuildGrammarBuildup"/> and
        /// <see cref="PossessionLanePattern"/> now author each grammar's buildup/lane shape
        /// directly, so a second, generic post-hoc nudge here would have doubled up on top of an
        /// already-authored shape. Pressure and Spacing remain genuinely independent dimensions
        /// layered on top of whichever grammar buildup already ran.</summary>
        private static Step[] ApplyPlanShaping(Step[] steps, TheaterScenePlan plan)
        {
            var shaped = new Step[steps.Length];
            float pressureTempo = plan.Pressure == PressureMode.HighPress ? 1.15f
                : plan.Pressure == PressureMode.LowBlock ? 0.85f : 1.0f;
            float spacingSpread = plan.Spacing == SpacingMode.Stretched ? 1.18f
                : plan.Spacing == SpacingMode.Compact ? 0.82f : 1.0f;

            for (int i = 0; i < steps.Length; i++)
            {
                Step s = steps[i];
                s.Tempo = Mathf.Clamp01(s.Tempo * pressureTempo);
                // High press visibly closes down the ball carrier and the back line (§7.3:
                // "visibly different defending... behavior per pressure mode") without touching
                // a template's own authored chase="true" moments (breakaways/corners already
                // chase by truth, not by pressure — this only ever ADDS chase, never removes it).
                if (plan.Pressure == PressureMode.HighPress && (s.Route == RoutePass || s.Route == RouteBackLine))
                    s.Chase = true;

                if (s.Route == RoutePass)
                {
                    float laneOffset = (s.Ball.y - 0.5f) * spacingSpread;
                    s.Ball = new Vector2(s.Ball.x, Mathf.Clamp01(0.5f + laneOffset));
                }

                shaped[i] = s;
            }
            return shaped;
        }

        /// <summary>Phase 2E-2 (PRD §7.2/§7.3/§10): the shared grammar-driven BUILDUP builder that
        /// Goal, Breakaway, and Near Miss each prefix onto their own truth-authored payoff steps,
        /// rather than every template authoring its own (template x grammar) sequence by hand.
        /// Grammar governs HOW the ball reaches the final third — the template (and, for near
        /// miss, the payoff) still owns WHAT happens once it gets there: this method never fires a
        /// marker and never carries a <c>StagedGoal</c>/<c>StagedCount</c>. Every caller passes a
        /// <paramref name="budget"/> that is a fixed fraction of the template's total <c>B</c>,
        /// and every grammar's own internal step-duration fractions sum to exactly 1, so
        /// <c>B * 1.00</c> stays intact regardless of which of the five renders — the same
        /// invariant Phase 2D's <see cref="BuildCornerCore"/> and Phase 2E-1's
        /// <see cref="BuildNearMissCore"/> hold for their own shape families.
        ///
        /// Silhouettes (VISUAL-DESIGN.md §10), rendered as real waypoint/tempo/chase/route data
        /// instead of the single generic lane nudge <see cref="ApplyPlanShaping"/> used to apply:
        /// <list type="bullet">
        /// <item><description><see cref="MovementGrammar.Central"/>: the lane axis is pulled
        /// tight toward the middle for the first two steps (compact triangles), releasing out to
        /// <paramref name="lane"/> only on the final step that hands off to the caller's payoff —
        /// the smallest lateral travel of the five. This is the documented default when no plan
        /// is present.</description></item>
        /// <item><description><see cref="MovementGrammar.Wing"/>: every step holds the SAME
        /// widened lane (<see cref="WideLane"/>) — one touchline, overloaded, never switching
        /// sides — zero lateral travel between its own steps, but the furthest offset from center
        /// of the five.</description></item>
        /// <item><description><see cref="MovementGrammar.Switch"/>: the first two steps build on
        /// one side, then the final step (its largest duration share) flips the ball to the
        /// mirrored far side in one jump — by far the largest single-step lateral travel of the
        /// five, "one long diagonal transfer".</description></item>
        /// <item><description><see cref="MovementGrammar.Counter"/>: the ONE grammar that adds
        /// its own <c>Chase</c> (from the second step on) regardless of
        /// <paramref name="truthChase"/> — stretched, high tempo throughout, the visible
        /// turnover-to-recovering-chase read.</description></item>
        /// <item><description><see cref="MovementGrammar.SetPiece"/>: the ONLY grammar whose
        /// first step routes via <see cref="RouteAuthored"/> instead of <see cref="RoutePass"/> —
        /// a brief, near-static setup (tempo fixed at 0.15, ignoring urgency) — before a
        /// synchronised run and a full-tempo singular delivery.</description></item>
        /// </list>
        /// <paramref name="truthChase"/> threads a template's own TRUTH chase fact (Breakaway's
        /// hunted carry, Near Miss Interception's closing defense) through every grammar
        /// unconditionally — grammar may ADD a chase reaction (Counter) but never suppress one the
        /// template itself staged.</summary>
        private static Step[] BuildGrammarBuildup(MovementGrammar grammar, float budget, float startX,
            float endX, float lane, float startTerr, float endTerr, float tempoFloor, bool truthChase)
        {
            switch (grammar)
            {
                case MovementGrammar.Wing:
                {
                    float wingLane = WideLane(lane);
                    return new[]
                    {
                        S(budget * 0.36f, Mathf.Lerp(startX, endX, 0.32f), wingLane,
                            Mathf.Lerp(startTerr, endTerr, 0.35f), Mathf.Max(0.55f, tempoFloor)),
                        S(budget * 0.34f, Mathf.Lerp(startX, endX, 0.66f), wingLane,
                            Mathf.Lerp(startTerr, endTerr, 0.7f), Mathf.Max(0.7f, tempoFloor), chase: truthChase),
                        S(budget * 0.30f, endX, wingLane, endTerr, 1f, chase: truthChase),
                    };
                }

                case MovementGrammar.Switch:
                {
                    float nearSide = WideLane(lane);
                    float farSide = 1f - nearSide;
                    return new[]
                    {
                        S(budget * 0.30f, Mathf.Lerp(startX, endX, 0.30f), nearSide,
                            Mathf.Lerp(startTerr, endTerr, 0.30f), Mathf.Max(0.55f, tempoFloor)),
                        S(budget * 0.26f, Mathf.Lerp(startX, endX, 0.55f), nearSide,
                            Mathf.Lerp(startTerr, endTerr, 0.55f), Mathf.Max(0.7f, tempoFloor), chase: truthChase),
                        // The long diagonal transfer — the far side opens.
                        S(budget * 0.44f, endX, farSide, endTerr, 1f, chase: truthChase),
                    };
                }

                case MovementGrammar.Counter:
                {
                    return new[]
                    {
                        // The visible turnover — won cleanly, no chase yet.
                        S(budget * 0.22f, Mathf.Lerp(startX, endX, 0.15f), lane,
                            Mathf.Lerp(startTerr, endTerr, 0.2f), Mathf.Max(0.85f, tempoFloor)),
                        // Stretched lines; the recovering chase engages from here on — Counter's
                        // own chase, independent of whatever truthChase the template carries.
                        S(budget * 0.40f, Mathf.Lerp(startX, endX, 0.62f), lane,
                            Mathf.Lerp(startTerr, endTerr, 0.6f), 1f, chase: true),
                        S(budget * 0.38f, endX, lane, endTerr, 1f, chase: true),
                    };
                }

                case MovementGrammar.SetPiece:
                    return new[]
                    {
                        // Brief static setup — the ball barely moves; a dead-ball position, held.
                        S(budget * 0.30f, startX, lane, startTerr, 0.15f, route: RouteAuthored),
                        // Synchronised runs cover the ground.
                        S(budget * 0.30f, Mathf.Lerp(startX, endX, 0.65f), lane,
                            Mathf.Lerp(startTerr, endTerr, 0.6f), Mathf.Max(0.75f, tempoFloor)),
                        // The singular delivery.
                        S(budget * 0.40f, endX, lane, endTerr, 1f),
                    };

                case MovementGrammar.Central:
                default:
                {
                    float tight = Mathf.Lerp(lane, 0.5f, 0.5f);
                    return new[]
                    {
                        S(budget * 0.42f, Mathf.Lerp(startX, endX, 0.35f), tight,
                            Mathf.Lerp(startTerr, endTerr, 0.4f), Mathf.Max(0.55f, tempoFloor)),
                        S(budget * 0.32f, Mathf.Lerp(startX, endX, 0.68f), tight,
                            Mathf.Lerp(startTerr, endTerr, 0.7f), Mathf.Max(0.7f, tempoFloor), chase: truthChase),
                        S(budget * 0.26f, endX, lane, endTerr, 1f, chase: truthChase),
                    };
                }
            }
        }

        /// <summary>Widens a lane value away from center — <see cref="MovementGrammar.Wing"/>'s
        /// "overload one touchline" and <see cref="MovementGrammar.Switch"/>'s "pressure draws to
        /// one side" both start from this. A near-center <paramref name="lane"/> (within 0.02 of
        /// 0.5 — i.e. <see cref="SceneLane.Center"/>) carries no side information to amplify, so
        /// it defaults to one fixed touchline rather than collapsing to 0.5 (which would make
        /// Wing/Switch visually indistinguishable from Central for a center-lane scene).</summary>
        private static float WideLane(float lane)
            => Mathf.Abs(lane - 0.5f) < 0.02f ? 0.80f : Mathf.Clamp01(0.5f + (lane - 0.5f) * 1.5f);

        /// <summary>Phase 2E-3 (PRD §7.1's "chance shape" dimension; §10's payoff-silhouette
        /// table): the final DELIVERY into a shooting chance, inserted between a template's
        /// grammar buildup (how the ball reaches the final third — <see cref="BuildGrammarBuildup"/>)
        /// and its own truth-authored payoff tail (what happens once the chance exists) —
        /// sequential composition, exactly like grammar and payoff already compose in Phase
        /// 2E-2/2E-1, never a (grammar x chance shape) product. This method never fires a marker
        /// and never carries a <c>StagedGoal</c>, so it composes unchanged with every Goal/
        /// Breakaway tail and every near-miss payoff tail except one (see
        /// <see cref="ChanceShape.Rebound"/>'s entry below). Every caller passes a
        /// <paramref name="budget"/> carved out of what the grammar buildup previously spent
        /// alone (the pre-shot total is unchanged), and every shape's own step-duration fractions
        /// sum to exactly the budget, so <c>B * 1.00</c> stays intact regardless of which of the
        /// five renders — the same invariant every prior phase's shape family holds.
        ///
        /// Silhouettes (VISUAL-DESIGN.md §10's payoff-silhouette table, verbatim):
        /// <list type="bullet">
        /// <item><description><see cref="ChanceShape.ThroughBall"/>: "runner crosses the back
        /// line before the final touch" — the deepest single run of the five, held at a fixed
        /// lane with almost no lateral drift.</description></item>
        /// <item><description><see cref="ChanceShape.Cross"/>: "delivery originates wide and
        /// enters the goal area laterally" — starts at <see cref="WideLane"/>, then sweeps its Y
        /// back toward center while X pushes to the edge of the area.</description></item>
        /// <item><description><see cref="ChanceShape.Cutback"/>: "ball reaches the byline, then
        /// travels backward to the shooter" — the ONLY shape whose X ever DECREASES; every other
        /// shape's X only ever advances toward the goal it is attacking.</description></item>
        /// <item><description><see cref="ChanceShape.Rebound"/>: "first shot visibly blocked/
        /// saved; a different second touch completes the fact" — the only shape that is itself
        /// three steps carrying real routes (<see cref="RouteShot"/>, then a REAL defender via
        /// <see cref="RouteBackLine"/>, then a DIFFERENT attacker via <see cref="RoutePass"/>),
        /// not a waypoint/tempo variation on the others. Carries no marker of its own — the
        /// payoff tail that follows still supplies the scene's one and only marker, so a
        /// Rebound-shaped GOAL still fires exactly one <see cref="MkGoal"/> (the rebound's own
        /// first attempt is visibly stopped by a defender; the tail's own shot completes the
        /// single staged fact) and a Rebound-shaped near miss still fires exactly one payoff
        /// marker. Structurally incompatible with <see cref="ScenePayoff.Interception"/> — see
        /// <see cref="BuildNearMissCore"/>'s Interception case for why and how that is
        /// handled.</description></item>
        /// <item><description><see cref="ChanceShape.Direct"/>: no flourish — a single
        /// high-tempo advance, the simplest of the five and the legacy plan-free fallback
        /// (<paramref name="shape"/> null).</description></item>
        /// </list>
        /// <paramref name="chase"/> threads a template's own truth chase fact (Breakaway's
        /// hunted carry, near-miss Interception's closing defense) through every shape,
        /// exactly like <see cref="BuildGrammarBuildup"/>'s <c>truthChase</c> — Rebound's own
        /// block step always chases regardless (its own defining fact, not a threaded one).</summary>
        private static Step[] BuildChanceShapeDelivery(ChanceShape? shape, float budget, float startX,
            float lane, float terr, float tempoFloor, bool chase)
        {
            switch (shape)
            {
                case ChanceShape.Cross:
                {
                    float wide = WideLane(lane);
                    return new[]
                    {
                        S(budget * 0.5f, Mathf.Lerp(startX, 0.93f, 0.7f), wide, terr,
                            Mathf.Max(0.7f, tempoFloor), chase: chase),
                        S(budget * 0.5f, 0.95f, Mathf.Lerp(wide, 0.5f, 0.65f), terr, 1f, chase: chase),
                    };
                }

                case ChanceShape.Cutback:
                    return new[]
                    {
                        S(budget * 0.5f, 0.98f, WideLane(lane), terr, Mathf.Max(0.75f, tempoFloor), chase: chase),
                        // Backward — the one shape where X ever decreases; the shooter waits centrally.
                        S(budget * 0.5f, 0.78f, 0.5f, terr, 1f, chase: chase),
                    };

                case ChanceShape.Rebound:
                    return new[]
                    {
                        // The chance shape's OWN first attempt: a shot struck...
                        S(budget * 0.34f, Mathf.Lerp(startX, 0.95f, 0.8f), lane, terr, 1f, route: RouteShot),
                        // ...blocked by a REAL defender...
                        S(budget * 0.33f, Mathf.Lerp(startX, 0.95f, 0.7f), 1f - lane, terr, 1f,
                            route: RouteBackLine, chase: true),
                        // ...and a DIFFERENT attacker recycles the loose ball — the second touch.
                        // No marker anywhere here: the payoff tail supplies the scene's one marker.
                        S(budget * 0.33f, Mathf.Lerp(startX, 0.95f, 0.9f), lane, terr, 0.8f,
                            route: RoutePass, chase: chase),
                    };

                case ChanceShape.ThroughBall:
                    return new[]
                    {
                        S(budget * 0.5f, Mathf.Lerp(startX, 0.96f, 0.6f), lane, terr,
                            Mathf.Max(0.85f, tempoFloor), chase: chase),
                        S(budget * 0.5f, 0.975f, lane, terr, 1f, chase: chase),
                    };

                case ChanceShape.Direct:
                default:
                    return new[]
                    {
                        S(budget, Mathf.Lerp(startX, 0.96f, 1f), lane, terr, Mathf.Max(0.8f, tempoFloor),
                            chase: chase),
                    };
            }
        }

        /// <summary>Concatenates a grammar-driven buildup with a template's own truth-authored
        /// payoff tail — see <see cref="BuildGrammarBuildup"/>'s doc for why this two-piece
        /// composition, rather than one array per (template x grammar) pair, is how Phase 2E-2
        /// avoids the N-times-M explosion.</summary>
        private static Step[] Concat(Step[] a, Step[] b)
        {
            var r = new Step[a.Length + b.Length];
            Array.Copy(a, 0, r, 0, a.Length);
            Array.Copy(b, 0, r, a.Length, b.Length);
            return r;
        }

        /// <summary>Phase 2E-2 (PRD §7.2's Territory row: "central recycle, wing progression,
        /// switch, controlled counter start" — a possession vocabulary, deliberately distinct from
        /// <see cref="BuildGrammarBuildup"/>'s chance-approach one; Territory/CalmPossession never
        /// build toward a shot, so there is no "handoff to a payoff" to release out of, and
        /// <see cref="MovementGrammar.SetPiece"/> is not in this row's legal set at all — §7.2:
        /// "no dead-ball recycling in a possession scene"). Both templates already author their
        /// OWN x/territory/tempo pacing and keep doing so; this method supplies only the shared
        /// lane-axis (Y) shape grammar governs, so the two templates' otherwise very different
        /// feel (Territory advances into the attacking third; CalmPossession barely leaves the
        /// middle third) stays exactly as each one already authored it.</summary>
        private static void PossessionLanePattern(MovementGrammar? grammar, float lane,
            out float y1, out float y2, out float y3)
        {
            switch (grammar ?? MovementGrammar.Switch)
            {
                case MovementGrammar.Central:
                    float tight = Mathf.Lerp(lane, 0.5f, 0.7f);
                    y1 = tight; y2 = tight; y3 = 0.5f;
                    break;
                case MovementGrammar.Wing:
                    float wingLane = WideLane(lane);
                    y1 = wingLane; y2 = wingLane; y3 = wingLane;
                    break;
                case MovementGrammar.Counter:
                    // A brisk start out wide, reined back in before it develops into a full
                    // switch — "controlled counter start", never a genuine breakaway.
                    y1 = WideLane(lane); y2 = lane; y3 = 0.5f;
                    break;
                case MovementGrammar.Switch:
                default:
                    // The pre-Phase-2E-2 shape both templates always played — lane, then the
                    // mirrored far side, then it settles central. Already a switch.
                    y1 = lane; y2 = 1f - lane; y3 = 0.5f;
                    break;
            }
        }

        private Step[] BuildFinalScript(SceneSpec spec, ScoreLedger.FinalPlan plan,
            CountLedger.FinalPlan? countPlan = null)
        {
            float lane = Lane(spec.Variant);
            bool won = spec.Template == SceneTemplate.LegFinalWon || plan.Grade == LegGrade.Won;
            float B = spec.Duration; // base seconds; corrections are absolute sub-scenes on top

            var steps = new System.Collections.Generic.List<Step>(4 + plan.Goals.Length * 2);
            if (won)
            {
                steps.Add(S(B * 0.40f, 0.62f, 0.5f, 0.64f, 0.6f));   // the pre-reveal hold
                steps.Add(S(B * 0.27f, 0.78f, lane, 0.70f, 0.85f));
                foreach (ScoreLedger.StagedGoal g in plan.Goals)
                {
                    // Authored in the picked frame, mirrored whole when the goal belongs to
                    // the other side — a won final can still reveal baked opponent goals
                    // (endpoint convergence; Sol, F_0.4.0 P3 r1+r2).
                    // TVS-H03: the run carries the SAME goal as the shot (goal: g), not just
                    // the shot — EnterStep's RoutePass case reads it to route a bound scorer
                    // (ScoreLedger.BindAnytimeScorer) to the exact actor the shot then fires
                    // from, instead of the nearest-neighbor default.
                    Step run = S(P(1.1f), 0.86f, 1f - lane, 0.72f, 1f, goal: g);
                    Step shot = S(P(1.4f), 0.975f, 0.5f, 0.74f, 1f, MkGoal, g, RouteShot);
                    steps.Add(g.ScoredByPicked ? run : MirrorStep(run));
                    steps.Add(g.ScoredByPicked ? shot : MirrorStep(shot));
                }
                AppendFinalCounts(steps, spec, countPlan, lane);
                steps.Add(S(B * 0.33f, 0.5f, 0.5f, 0.72f, 1f, route: RouteAuthored)); // whistle — celebrate
            }
            else
            {
                steps.Add(S(B * 0.40f, 0.38f, 0.5f, 0.36f, 0.6f, atkPicked: false));  // the dread hold
                steps.Add(S(B * 0.27f, 0.22f, lane, 0.30f, 0.8f, atkPicked: false));
                foreach (ScoreLedger.StagedGoal g in plan.Goals)
                {
                    // Authored in the opponent frame, mirrored whole when the goal is the
                    // picked side's (Sol, F_0.4.0 P3 r1+r2). BindAnytimeScorer only ever binds
                    // a Won plan, so g.HasBoundScorer is always false on this Lost path — goal:
                    // g is threaded through anyway for symmetry with the Won branch above.
                    Step run = S(P(1.0f), 0.13f, lane, 0.28f, 1f, atkPicked: false, chase: true, goal: g);
                    Step shot = S(P(1.5f), 0.025f, 0.5f, 0.26f, 1f, MkGoal, g, RouteShot, atkPicked: false);
                    steps.Add(g.ScoredByPicked ? MirrorStep(run) : run);
                    steps.Add(g.ScoredByPicked ? MirrorStep(shot) : shot);
                }
                AppendFinalCounts(steps, spec, countPlan, lane);
                steps.Add(S(B * 0.33f, 0.5f, 0.5f, 0.26f, 0.1f, route: RouteAuthored, atkPicked: false)); // collapse
            }
            return steps.ToArray();
        }

        private Step[] BuildKillShotScript(int variant)
        {
            float lane = Lane(variant);
            return new[]
            {
                S(P(0.9f), 0.30f, lane, 0.34f, 0.8f, atkPicked: false),                 // opponent buildup
                S(P(0.6f), 0.14f, lane, 0.30f, 1f, atkPicked: false, chase: true),      // the approach
                S(P(0.35f), 0.10f, 0.5f, 0.28f, 1f, route: RouteShot, atkPicked: false), // shot launched
                S(P(0.20f), 0.05f, 0.5f, 0.28f, 1f, MkSuspend, route: RouteAuthored, atkPicked: false), // FROZEN
            };
        }

        private Step[] BuildContinuationScript(ScoreLedger.FinalPlan plan, CountLedger.FinalPlan? countPlan = null,
            MarketKind market = MarketKind.Moneyline)
        {
            var steps = new System.Collections.Generic.List<Step>(2 + plan.Goals.Length * 2);
            switch (plan.Grade)
            {
                case LegGrade.Voided:
                    steps.Add(S(P(0.9f), 0.06f, 0.5f, 0.40f, 0.1f, MkVoid, route: RouteAuthored, atkPicked: false));
                    steps.Add(S(P(1.4f), 0.5f, 0.5f, 0.50f, 0.1f, route: RouteAuthored)); // the scene dissolves
                    break;

                case LegGrade.Won:
                    steps.Add(S(P(0.7f), 0.20f, 0.78f, 0.40f, 1f, MkSave, route: RouteAuthored, atkPicked: false));
                    foreach (ScoreLedger.StagedGoal g in plan.Goals)
                    {
                        // the sucker-punch break — picked frame, mirrored whole for the
                        // other side's goals (Sol, F_0.4.0 P3 r1+r2). TVS-H03: goal: g on the
                        // run too, same reasoning as BuildFinalScript's Won branch.
                        Step run = S(P(1.1f), 0.70f, 0.42f, 0.62f, 1f, goal: g);
                        Step shot = S(P(1.4f), 0.975f, 0.5f, 0.72f, 1f, MkGoal, g, RouteShot);
                        steps.Add(g.ScoredByPicked ? run : MirrorStep(run));
                        steps.Add(g.ScoredByPicked ? shot : MirrorStep(shot));
                    }
                    AppendFinalCounts(steps, new SceneSpec(SceneTemplate.LegFinalWon, 0, false, false,
                        true, null, null, countPlan, market, 0f), countPlan, 0.5f);
                    steps.Add(S(P(1.8f), 0.5f, 0.5f, 0.72f, 1f, route: RouteAuthored)); // whistle — celebrate
                    break;

                case LegGrade.Lost:
                default:
                    bool first = true;
                    foreach (ScoreLedger.StagedGoal g in plan.Goals)
                    {
                        if (first)
                        {
                            // The frozen flight completes (chalked at the death if the entry
                            // score already satisfied Lost — the whistle still confirms it).
                            steps.Add(S(P(0.5f), 0.02f, 0.5f, 0.26f, 1f, MkGoal, g, RouteShot, atkPicked: false));
                            first = false;
                        }
                        else
                        {
                            // post-freeze reveals: opponent frame, mirrored whole for picked-
                            // side goals; only the frozen flight above keeps its launched-side
                            // continuity (Sol, F_0.4.0 P3 r1+r2). BindAnytimeScorer never binds
                            // a Lost plan; goal: g threaded for symmetry only.
                            Step run = S(P(1.0f), 0.13f, 0.35f, 0.28f, 1f, atkPicked: false, chase: true, goal: g);
                            Step shot = S(P(1.5f), 0.025f, 0.5f, 0.26f, 1f, MkGoal, g, RouteShot, atkPicked: false);
                            steps.Add(g.ScoredByPicked ? MirrorStep(run) : run);
                            steps.Add(g.ScoredByPicked ? MirrorStep(shot) : shot);
                        }
                    }
                    AppendFinalCounts(steps, new SceneSpec(SceneTemplate.LegFinalLost, 0, false, false,
                        false, null, null, countPlan, market, 0f), countPlan, 0.5f);
                    steps.Add(S(P(1.8f), 0.5f, 0.5f, 0.26f, 0.1f, route: RouteAuthored, atkPicked: false)); // collapse
                    break;
            }
            return steps.ToArray();
        }

        private void AppendFinalCounts(List<Step> steps, SceneSpec spec, CountLedger.FinalPlan? countPlan,
            float lane)
        {
            if (!countPlan.HasValue || countPlan.Value.Counts == null) return;
            byte marker = spec.Market == MarketKind.TotalCards ? MkBooking : MkCorner;
            bool won = spec.Template == SceneTemplate.LegFinalWon;
            for (int i = 0; i < countPlan.Value.Counts.Length; i++)
            {
                CountLedger.StagedCount count = countPlan.Value.Counts[i];
                // A zero batch is nothing happening — it never earns a corner/booking scene
                // (Sol, F_0.4.0 P3 r2; PlanFinal filters these too, this is the belt).
                if (count.TotalDelta <= 0) continue;
                // TVS-S01 fix (PRD §7.6): attribution is the staged fact's beneficiary team
                // (HomeDelta/AwayDelta), never a bet-derived flag. Every market leg anchors
                // home as the presentation side (_homeAttacksRight is true for any non-
                // moneyline leg — SweatFlavor.PickedHomeForPresentation), so "attack is home"
                // is exactly the AtkPicked routing primitive below.
                bool attackHome = count.BeneficiaryIsHome;
                float x = attackHome ? 0.96f : 0.04f;
                steps.Add(S(P(0.9f), x, lane, won ? 0.70f : 0.30f, 1f,
                    marker, route: spec.Market == MarketKind.TotalCorners ? RouteCorner : RouteAuthored,
                    atkPicked: attackHome, count: count));
            }
        }

        // ------------------------------------------------------------------ scene visuals

        private void FlashGoal(bool right, bool strong)
        {
            if (_bookingCard != null) _bookingCard.enabled = false;
            _flashRight = right;
            _flashDur = strong ? 0.55f : 0.35f;
            _flashT = _flashDur;
            _flashRing.enabled = true;
            _flashRing.color = new Color(1f, 1f, 1f, strong ? 0.95f : 0.5f);
        }

        private void FlashCorner()
        {
            if (_bookingCard != null) _bookingCard.enabled = false;
            _flashRight = _ballPos.x > 0f;
            _flashDur = 0.38f;
            _flashT = _flashDur;
            _flashRing.enabled = true;
            _flashRing.color = new Color(1f, 1f, 1f, 0.68f);
        }

        private void FlashBooking()
        {
            // Phase 2D (PRD §7.6): "the marker appears on that side's actor, not merely at the
            // ball." Booking's MkBooking step routes via RoutePass (see BuildBookingCore), so
            // _routeDotIx/_routeDotHome — resolved by THIS step's own EnterStep, still valid here
            // since CompleteStep runs before the next step is entered — name the real fouling-
            // side actor the card belongs on. _ballPos is only the fallback for a hand-built step
            // that (unusually) never resolved one.
            Vector2 markerPos = _routeDotIx >= 0 ? DotPos(_routeDotHome, _routeDotIx) : _ballPos;
            if (_bookingCard != null)
            {
                _bookingCard.enabled = true;
                _bookingCard.rectTransform.anchoredPosition = markerPos;
                _bookingCard.rectTransform.localScale = Vector3.one;
                _bookingCard.transform.SetAsLastSibling();
            }
            _flashRight = markerPos.x > 0f;
            _flashDur = 0.45f;
            _flashT = _flashDur;
            _flashRing.enabled = true;
            _flashRing.color = new Color(0.92f, 0.95f, 1f, 0.72f);
            _markerActorActive = _routeDotIx >= 0;
            _markerActorHome = _routeDotHome;
            _markerActorIx = _routeDotIx;
        }

        private void UpdateFlash(float dt)
        {
            if (_flashT <= 0f) return;
            _flashT -= dt;
            float t = 1f - Mathf.Clamp01(_flashT / Mathf.Max(0.01f, _flashDur)); // 0 → 1
            float x = (_flashRight ? 0.5f : -0.5f) * (_w - Pad * 2f) * 0.97f;
            var rt = _flashRing.rectTransform;
            rt.anchoredPosition = new Vector2(x, 0f);
            rt.localScale = Vector3.one * Mathf.Lerp(0.35f, 1.7f, t);
            Color c = _flashRing.color;
            c.a = Mathf.Lerp(c.a, 0f, t * t);
            _flashRing.color = c;
            if (_flashT <= 0f)
            {
                _flashRing.enabled = false;
                if (_bookingCard != null) _bookingCard.enabled = false;
            }
        }

        private void KeeperLunge()
        {
            // The keeper defending the goal nearest the ball hurls toward it — one impulse,
            // the smooth-damp brings them home after. Keepers save; they never receive.
            bool ballRight = _ballPos.x > 0f;
            bool homeDefendsRight = !_homeAttacksRight;
            ref Vector2 vel = ref (ballRight == homeDefendsRight ? ref _hkVel : ref _akVel);
            ref Vector2 pos = ref (ballRight == homeDefendsRight ? ref _hkPos : ref _akPos);
            Vector2 toBall = _ballPos - pos;
            vel += toBall.normalized * Mathf.Min(toBall.magnitude * 3f, 260f);
        }

        private void ApplyVoidTint()
        {
            // The VOID treatment — the ONE sanctioned cyan on the stage (design/08): the match
            // stops mattering, so the teams stop having colors.
            // T41: the VOID stage tint was also at alpha 1 — a voided leg is not a payoff, so it has
            // no claim on the top of the ladder either. Actors stay at their L3 ceiling.
            var voidTint = AtTier(new Color(0.42f, 0.56f, 0.62f, 1f), TierL3);
            for (int i = 0; i < PitchLayout.OutfieldPerTeam; i++)
            {
                _homeDots[i].color = voidTint;
                _awayDots[i].color = voidTint;
            }
            _homeKeeper.color = voidTint;
            _awayKeeper.color = voidTint;
            _ball.color = AtTier(new Color(0.62f, 0.86f, 0.96f, 1f), TierL3); // T41: L3, not 0.9
        }

        // ------------------------------------------------------------------ actor lookups

        private Vector2 DotPos(bool home, int ix) => home ? _homePos[ix] : _awayPos[ix];

        private bool BallCarriedBy(bool home) => _carrierHome == home;

        private int NearestOutfield(bool home, Vector2 localPt, int exclude)
        {
            Vector2[] side = home ? _homePos : _awayPos;
            int best = 0;
            float bestD = float.PositiveInfinity;
            for (int i = 0; i < side.Length; i++)
            {
                if (i == exclude) continue;
                float d = (side[i] - localPt).sqrMagnitude;
                if (d < bestD) { bestD = d; best = i; }
            }
            return best;
        }

        private int NearestBackLine(bool home, Vector2 localPt)
        {
            Vector2[] side = home ? _homePos : _awayPos;
            int best = 0;
            float bestD = float.PositiveInfinity;
            for (int i = 0; i < side.Length; i++)
            {
                if (!PitchLayout.IsBackLine(i)) continue;
                float d = (side[i] - localPt).sqrMagnitude;
                if (d < bestD) { bestD = d; best = i; }
            }
            return best;
        }

        private void FindNearestThree(bool home, Vector2 localPt, out int first, out int second, out int third)
        {
            Vector2[] side = home ? _homePos : _awayPos;
            first = second = third = -1;
            float d1 = float.PositiveInfinity, d2 = float.PositiveInfinity, d3 = float.PositiveInfinity;
            for (int i = 0; i < side.Length; i++)
            {
                float d = (side[i] - localPt).sqrMagnitude;
                if (d < d1)
                {
                    d3 = d2; third = second;
                    d2 = d1; second = first;
                    d1 = d; first = i;
                }
                else if (d < d2)
                {
                    d3 = d2; third = second;
                    d2 = d; second = i;
                }
                else if (d < d3)
                {
                    d3 = d; third = i;
                }
            }
        }

        /// <summary>Teammates near the move make short forward runs during buildup — off-ball
        /// motion with intent, not static jitter.</summary>
        private void ForwardRuns(bool home, float dir)
        {
            Vector2[] noise = home ? _homeNoise : _awayNoise;
            for (int i = 0; i < noise.Length; i++)
                if (_rng.NextDouble() < 0.4)
                    noise[i] = new Vector2(dir * Rand(4f, 26f), Rand(-14f, 14f));
        }

        // ------------------------------------------------------------------ helpers

        private void ApplyPositions()
        {
            for (int i = 0; i < PitchLayout.OutfieldPerTeam; i++)
            {
                _homeDots[i].rectTransform.anchoredPosition = _homePos[i];
                _awayDots[i].rectTransform.anchoredPosition = _awayPos[i];
            }
            _homeKeeper.rectTransform.anchoredPosition = _hkPos;
            _awayKeeper.rectTransform.anchoredPosition = _akPos;
            _ball.rectTransform.anchoredPosition = _ballPos;
        }

        private float NextPassDelay() => Rand(passIntervalMin, passIntervalMax);

        private float Rand(float lo, float hi) => lo + (float)_rng.NextDouble() * (hi - lo);

        private void RerollNoise(ref Vector2 n) => n = new Vector2(Rand(-16f, 16f), Rand(-14f, 14f));

        /// <summary>Normalized pitch space → local canvas pixels (with the line inset).</summary>
        private Vector2 ToLocal(Vector2 norm)
            => new Vector2((norm.x - 0.5f) * (_w - Pad * 2f), (norm.y - 0.5f) * (_h - Pad * 2f));

        /// <summary>DESIGN.md §3's brightness ladder, mirrored from
        /// `main-2/docs/design/design-system/components/tv/tiers.js` with the source cited — a C#
        /// const cannot import a JS module (handoff §4A).
        ///
        /// <para>T41 (C3 violation, blocking): measured off delivered frames, the pitch ran at
        /// **1.000** while the actionable cash-out band — "the surface's only L4 element" — measured
        /// 0.671. The law did not fail because the band was dim; it failed because everything else
        /// was brighter than the one thing the player can act on. Capping the stage makes cash-out
        /// the brightest element BY CONSTRUCTION, with no change to gold.</para></summary>
        private const float TierL4 = 1f, TierL3 = 0.7f, TierL2 = 0.4f, TierL1 = 0.15f;

        /// <summary>Returns <paramref name="c"/> at a ladder tier. Multiplies alpha so a colour that
        /// is already partly transparent by design (pitch markings) composes rather than resets.</summary>
        private static Color AtTier(Color c, float tier)
        {
            c.a *= tier;
            return c;
        }

        private static Color Brighten(Color c)
            => new Color(Mathf.Min(1f, c.r * 1.25f + 0.12f), Mathf.Min(1f, c.g * 1.25f + 0.12f),
                Mathf.Min(1f, c.b * 1.25f + 0.12f), 1f);

        public static Color FromRgb(uint rgb)
            => new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);

        private Image MakeRect(string name, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(transform, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return img;
        }

        private Image MakeImage(string name, Vector2 pos, Vector2 size, Color color)
        {
            Image img = MakeRect(name, pos, size, color);
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            return img;
        }

        private Image MakeDot(string name, Vector2 pos, float diameter, Color color)
        {
            Image img = MakeRect(name, pos, new Vector2(diameter, diameter), color);
            img.sprite = CircleSprite();
            return img;
        }

        private static Sprite CircleSprite()
        {
            if (_circleSprite != null) return _circleSprite;
            // 64px texture, solid disc of radius 30 with a 1px anti-aliased rim.
            _circleSprite = BuildRadialSprite(64, d => d <= 30f ? 1f : Mathf.Clamp01(31f - d));
            return _circleSprite;
        }

        private static Sprite RingSprite()
        {
            if (_ringSprite != null) return _ringSprite;
            // 128px texture, a 4px ring at radius ~60 with anti-aliased edges.
            _ringSprite = BuildRadialSprite(128, d => Mathf.Clamp01(d - 57f) * Mathf.Clamp01(63f - d));
            return _ringSprite;
        }

        private static Sprite BuildRadialSprite(int size, Func<float, float> alphaAt)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            float c = (size - 1) / 2f;
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)); // raw pixel distance
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(alphaAt(d)) * 255f));
                }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
