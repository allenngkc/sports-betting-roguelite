using System;
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

        private RectTransform _rt;
        private float _w, _h;
        private const float Pad = 12f;

        private Image[] _homeDots;
        private Image[] _awayDots;
        private Image _homeKeeper, _awayKeeper, _ball;
        private Image _flashRing;

        private Vector2[] _homePos, _awayPos, _homeVel, _awayVel, _homeNoise, _awayNoise;
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
        private const byte MkNone = 0, MkGoal = 1, MkSuspend = 2, MkSave = 3, MkVoid = 4;
        // Ball routing per step (M-T3.1): how the authored waypoint finds a real actor.
        private const byte RoutePass = 0;      // nearest attacking OUTFIELD dot to the waypoint — a pass
        private const byte RouteAuthored = 1;  // fly to the authored point (holds, restarts to spots)
        private const byte RouteShot = 2;      // authored point, y snapped to the corner AWAY from the keeper
        private const byte RouteBackLine = 3;  // nearest DEFENDING back-line dot (clearances, chalked restarts)
        private const byte RouteKickoff = 4;   // center spot; the conceding side collects at step end

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
        }

        private Step[] _script;
        private int _stepIx;
        private float _stepT;
        private bool _stepEntered;
        private bool _suspendedAtShot;
        private float _sceneTerr = 0.5f;
        private float _sceneTerrVel;
        private Action<ScoreLedger.StagedGoal> _onGoalPlayed;
        private Action _onReveal;
        private bool _revealFired;
        private Action _onSceneComplete;
        private int _routeDotIx = -1;    // resolved actor for RoutePass/RouteBackLine steps
        private bool _routeDotHome;
        private Vector2 _stepBallLocal;  // resolved target for authored/shot steps
        private float _flashT;           // net-ripple flash countdown
        private float _flashDur;
        private bool _flashRight;

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

        // ------------------------------------------------------------------ construction

        /// <summary>Builds the stage under a world-space canvas. Center/size in canvas pixels.</summary>
        public static TheaterStage Build(Transform canvasRoot, Vector2 center, Vector2 size, Color lineColor, Color pitchBg)
        {
            var go = new GameObject("TheaterStage", typeof(RectTransform), typeof(TheaterStage));
            go.transform.SetParent(canvasRoot, false);
            var stage = go.GetComponent<TheaterStage>();
            stage.BuildInternal(center, size, lineColor, pitchBg);
            return stage;
        }

        private void BuildInternal(Vector2 center, Vector2 size, Color lineColor, Color pitchBg)
        {
            _rt = (RectTransform)transform;
            _rt.anchorMin = _rt.anchorMax = new Vector2(0.5f, 0.5f);
            _rt.pivot = new Vector2(0.5f, 0.5f);
            _rt.sizeDelta = size;
            _rt.anchoredPosition = center;
            _w = size.x;
            _h = size.y;

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
            MakeRect("GoalL", new Vector2(-_w / 2f + 2f, 0f), new Vector2(5f, goalH), Color.white);
            MakeRect("GoalR", new Vector2(_w / 2f - 2f, 0f), new Vector2(5f, goalH), Color.white);

            // Actors: 8 outfield per team + keepers + the ball (built last = drawn on top).
            _homeDots = new Image[PitchLayout.OutfieldPerTeam];
            _awayDots = new Image[PitchLayout.OutfieldPerTeam];
            _homePos = new Vector2[PitchLayout.OutfieldPerTeam];
            _awayPos = new Vector2[PitchLayout.OutfieldPerTeam];
            _homeVel = new Vector2[PitchLayout.OutfieldPerTeam];
            _awayVel = new Vector2[PitchLayout.OutfieldPerTeam];
            _homeNoise = new Vector2[PitchLayout.OutfieldPerTeam];
            _awayNoise = new Vector2[PitchLayout.OutfieldPerTeam];
            for (int i = 0; i < PitchLayout.OutfieldPerTeam; i++)
            {
                _homeDots[i] = MakeDot($"Home{i}", Vector2.zero, 22f, Color.gray);
                _awayDots[i] = MakeDot($"Away{i}", Vector2.zero, 22f, Color.gray);
            }
            _homeKeeper = MakeDot("HomeKeeper", Vector2.zero, 26f, Color.gray);
            _awayKeeper = MakeDot("AwayKeeper", Vector2.zero, 26f, Color.gray);
            _ball = MakeDot("Ball", Vector2.zero, 12f, Color.white);

            // The net-ripple flash (one reusable ring; positioned at whichever goal scores).
            _flashRing = MakeImage("NetRipple", Vector2.zero, new Vector2(_h * 0.5f, _h * 0.5f), Color.white);
            _flashRing.sprite = RingSprite();
            _flashRing.enabled = false;

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
                _homeDots[i].color = homeColor;
                _awayDots[i].color = awayColor;
                RerollNoise(ref _homeNoise[i]);
                RerollNoise(ref _awayNoise[i]);
                _homePos[i] = ToLocal(PitchLayout.FormationSlot(i, _homeAttacksRight, 0f));
                _awayPos[i] = ToLocal(PitchLayout.FormationSlot(i, !_homeAttacksRight, 0f));
                _homeVel[i] = _awayVel[i] = Vector2.zero;
            }
            _homeKeeper.color = homeKeeperColor;
            _awayKeeper.color = awayKeeperColor;
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
        {
            StartScript(BuildBeatScript(spec), onGoalPlayed, onReveal, onComplete);
        }

        /// <summary>Plays the final whistle sequence: pre-reveal hold → the plan's staged
        /// goal(s) as separately-timed sub-scenes → celebrate/collapse. The GREEN/DEAD slam
        /// and the final chrome reveal belong to the orchestrator at completion (TvLight sync).</summary>
        public void PlayFinalScene(SceneSpec spec, ScoreLedger.FinalPlan plan,
            Action<ScoreLedger.StagedGoal> onGoalPlayed, Action onComplete)
        {
            StartScript(BuildFinalScript(spec, plan), onGoalPlayed, null, onComplete);
        }

        /// <summary>The pending-loss window's kill scene: opponent buildup → shot launched →
        /// FROZEN at the suspension point, mid-flight. Holds until <see cref="ResumeSuspended"/>.</summary>
        public void SuspendKillShot(int variant)
        {
            StartScript(BuildKillShotScript(variant), null, null, null);
        }

        /// <summary>The suspended scene's continuation, chosen from the FINAL ticket-local grade
        /// (never WinProbAfter): Voided → cyan VOID dissolve, no goals; Won → the frozen shot
        /// resolves as a save, then the counter goal(s) the correction needs; Lost → the flight
        /// completes (chalked if the entry score already satisfied Lost), then corrections.</summary>
        public void ResumeSuspended(ScoreLedger.FinalPlan plan,
            Action<ScoreLedger.StagedGoal> onGoalPlayed, Action onComplete)
        {
            _suspendedAtShot = false;
            StartScript(BuildContinuationScript(plan), onGoalPlayed, null, onComplete);
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
            _onReveal = null;
            _revealFired = false;
            _onSceneComplete = null;
        }

        private void StartScript(Step[] script, Action<ScoreLedger.StagedGoal> onGoalPlayed,
            Action onReveal, Action onComplete)
        {
            _script = script;
            _stepIx = 0;
            _stepT = 0f;
            _stepEntered = false;
            _suspendedAtShot = false;
            _onGoalPlayed = onGoalPlayed;
            _onReveal = onReveal;
            _revealFired = false;
            _onSceneComplete = onComplete;
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
            MoveActors(_sceneTerr, speed, s.Chase, s.AtkPicked);

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
            _routeDotIx = -1;

            switch (s.Route)
            {
                case RoutePass:
                {
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
                case MkVoid:
                    ApplyVoidTint();
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
            MoveActors(terr, speed, chase: false, atkPicked: terr >= 0.5f);

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

        /// <summary>Formation motion shared by idle and scenes (M-T3.1): the attacking shape
        /// pushes up while the DEFENDING team drops into a compact block; on chase steps the
        /// two defenders nearest the ball hunt the carrier instead of holding shape.</summary>
        private void MoveActors(float terr, float speed, bool chase, bool atkPicked)
        {
            float bias = (terr - 0.5f) * 2f; // [-1, 1] toward the right goal (picked frame)
            float damp = dotDamp / speed;

            // Per-team posture: positive = on the front foot, negative = under siege.
            float homeAtk = _homeAttacksRight ? bias : -bias;
            float awayAtk = -homeAtk;
            float homeCompact = Mathf.Clamp01(-homeAtk) * 0.8f;
            float awayCompact = Mathf.Clamp01(-awayAtk) * 0.8f;

            bool chaseHome = chase && atkPicked != _homeAttacksRight; // defenders chase
            int chase1 = -1, chase2 = -1;
            if (chase) FindNearestTwo(chaseHome, _ballPos, out chase1, out chase2);

            for (int i = 0; i < PitchLayout.OutfieldPerTeam; i++)
            {
                Vector2 ht = chaseHome && (i == chase1 || i == chase2)
                    ? _ballPos + new Vector2(_homeAttacksRight ? -26f : 26f, i == chase1 ? 12f : -14f)
                    : ToLocal(PitchLayout.FormationSlot(i, _homeAttacksRight, homeAtk, homeCompact)) + _homeNoise[i];
                Vector2 at = !chaseHome && chase && (i == chase1 || i == chase2)
                    ? _ballPos + new Vector2(_homeAttacksRight ? 26f : -26f, i == chase1 ? 12f : -14f)
                    : ToLocal(PitchLayout.FormationSlot(i, !_homeAttacksRight, awayAtk, awayCompact)) + _awayNoise[i];
                _homePos[i] = Vector2.SmoothDamp(_homePos[i], ht, ref _homeVel[i], damp);
                _awayPos[i] = Vector2.SmoothDamp(_awayPos[i], at, ref _awayVel[i], damp);
            }
            _hkPos = Vector2.SmoothDamp(_hkPos, ToLocal(PitchLayout.Keeper(_homeAttacksRight)), ref _hkVel, dotDamp);
            _akPos = Vector2.SmoothDamp(_akPos, ToLocal(PitchLayout.Keeper(!_homeAttacksRight)), ref _akVel, dotDamp);
        }

        // ------------------------------------------------------------------ scene scripts

        /// <summary>Variant lanes: which flank the move runs down (EventText's variant trick).</summary>
        private static float Lane(int variant) => variant == 0 ? 0.5f : variant == 1 ? 0.32f : 0.68f;

        private static Step S(float dur, float bx, float by, float terr, float tempo,
            byte marker = MkNone, ScoreLedger.StagedGoal goal = default, byte route = RoutePass,
            bool atkPicked = true, bool chase = false)
            => new Step
            {
                Dur = dur, Ball = new Vector2(bx, by), Terr = terr, Tempo = tempo,
                Marker = marker, Goal = goal, Route = route, AtkPicked = atkPicked, Chase = chase,
            };

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

        private Step[] BuildBeatScript(SceneSpec spec)
        {
            float lane = Lane(spec.Variant);
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
                    core = new[]
                    {
                        S(B * 0.30f, 0.56f, lane, 0.60f, Mathf.Max(0.55f, u)),
                        S(B * 0.24f, 0.80f, lane, 0.68f, Mathf.Max(0.7f, u)),
                        S(B * 0.14f, 0.90f, lane, 0.72f, 1f),                       // the final pass
                        S(B * 0.14f, 0.985f, 0.5f, 0.72f, 1f, MkGoal, goal, RouteShot),
                        commits
                            ? S(B * 0.18f, 0.5f, 0.5f, 0.55f, 0.4f, route: RouteKickoff)
                            : S(B * 0.18f, 0.84f, 0.30f, 0.60f, 0.3f, route: RouteBackLine), // chalked: defenders restart
                    };
                    if (spec.Template == SceneTemplate.GoalAgainst) core = Mirror(core);
                    break;

                case SceneTemplate.BreakawayFor:
                case SceneTemplate.BreakawayAgainst:
                    core = new[]
                    {
                        S(B * 0.22f, 0.30f, 0.5f, 0.42f, 0.5f),                     // won it deep
                        S(B * 0.30f, 0.70f, lane, 0.58f, 1f, chase: true),          // the long carry, hunted
                        S(B * 0.14f, 0.88f, lane, 0.66f, 1f, chase: true),
                        S(B * 0.14f, 0.965f, spec.Variant == 2 ? 0.58f : 0.42f, 0.70f, 1f, MkGoal, goal, RouteShot),
                        commits
                            ? S(B * 0.20f, 0.5f, 0.5f, 0.55f, 0.4f, route: RouteKickoff)
                            : S(B * 0.20f, 0.84f, 0.30f, 0.60f, 0.3f, route: RouteBackLine),
                    };
                    if (spec.Template == SceneTemplate.BreakawayAgainst) core = Mirror(core);
                    break;

                case SceneTemplate.TerritoryFor:
                case SceneTemplate.TerritoryAgainst:
                    core = new[]
                    {
                        S(B * 0.35f, 0.55f, lane, 0.56f, Mathf.Max(0.5f, u)),
                        S(B * 0.35f, 0.62f, 1f - lane, 0.62f, Mathf.Max(0.5f, u)),
                        S(B * 0.30f, 0.60f, 0.5f, 0.62f, 0.4f),
                    };
                    if (spec.Template == SceneTemplate.TerritoryAgainst) core = Mirror(core);
                    break;

                case SceneTemplate.NearMissHope:
                case SceneTemplate.NearMissScare:
                    core = new[]
                    {
                        S(B * 0.26f, 0.62f, lane, 0.62f, 0.7f),
                        S(B * 0.20f, 0.86f, lane, 0.68f, 1f),
                        S(B * 0.12f, 0.99f, 0.47f, 0.72f, 1f, route: RouteShot),     // the shot
                        S(B * 0.10f, 0.94f, 0.82f, 0.70f, 1f, MkSave, route: RouteAuthored), // off the bar
                        S(B * 0.12f, 0.92f, 0.80f, 0.70f, 0f, route: RouteAuthored), // the hold — dead air
                        S(B * 0.20f, 0.60f, 0.35f, 0.58f, 0.3f, route: RouteBackLine), // cleared off the line
                    };
                    if (spec.Template == SceneTemplate.NearMissScare) core = Mirror(core);
                    break;

                case SceneTemplate.CalmPossession:
                {
                    core = new[]
                    {
                        S(B * 0.35f, 0.46f, 0.40f, 0.50f, 0.2f, atkPicked: spec.ForPicked),
                        S(B * 0.35f, 0.54f, 0.60f, 0.50f, 0.2f, atkPicked: spec.ForPicked),
                        S(B * 0.30f, 0.48f, 0.5f, 0.50f, 0.15f, atkPicked: spec.ForPicked),
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

            if (intro <= 0f) return core;

            // Steal → transition: the ball flips flanks at midfield before the move starts.
            var withIntro = new Step[core.Length + 1];
            withIntro[0] = S(intro, 0.42f, 1f - lane, 0.46f, 1f, atkPicked: core[0].AtkPicked);
            Array.Copy(core, 0, withIntro, 1, core.Length);
            return withIntro;
        }

        private Step[] BuildFinalScript(SceneSpec spec, ScoreLedger.FinalPlan plan)
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
                    steps.Add(S(1.1f, 0.86f, 1f - lane, 0.72f, 1f)); // the break at the death
                    steps.Add(S(1.4f, 0.975f, 0.5f, 0.74f, 1f, MkGoal, g, RouteShot));
                }
                steps.Add(S(B * 0.33f, 0.5f, 0.5f, 0.72f, 1f, route: RouteAuthored)); // whistle — celebrate
            }
            else
            {
                steps.Add(S(B * 0.40f, 0.38f, 0.5f, 0.36f, 0.6f, atkPicked: false));  // the dread hold
                steps.Add(S(B * 0.27f, 0.22f, lane, 0.30f, 0.8f, atkPicked: false));
                foreach (ScoreLedger.StagedGoal g in plan.Goals)
                {
                    steps.Add(S(1.0f, 0.13f, lane, 0.28f, 1f, atkPicked: false, chase: true));
                    steps.Add(S(1.5f, 0.025f, 0.5f, 0.26f, 1f, MkGoal, g, RouteShot, atkPicked: false));
                }
                steps.Add(S(B * 0.33f, 0.5f, 0.5f, 0.26f, 0.1f, route: RouteAuthored, atkPicked: false)); // collapse
            }
            return steps.ToArray();
        }

        private Step[] BuildKillShotScript(int variant)
        {
            float lane = Lane(variant);
            return new[]
            {
                S(0.9f, 0.30f, lane, 0.34f, 0.8f, atkPicked: false),                 // opponent buildup
                S(0.6f, 0.14f, lane, 0.30f, 1f, atkPicked: false, chase: true),      // the approach
                S(0.35f, 0.10f, 0.5f, 0.28f, 1f, route: RouteShot, atkPicked: false), // shot launched
                S(0.20f, 0.05f, 0.5f, 0.28f, 1f, MkSuspend, route: RouteAuthored, atkPicked: false), // FROZEN
            };
        }

        private Step[] BuildContinuationScript(ScoreLedger.FinalPlan plan)
        {
            var steps = new System.Collections.Generic.List<Step>(2 + plan.Goals.Length * 2);
            switch (plan.Grade)
            {
                case LegGrade.Voided:
                    steps.Add(S(0.9f, 0.06f, 0.5f, 0.40f, 0.1f, MkVoid, route: RouteAuthored, atkPicked: false));
                    steps.Add(S(1.4f, 0.5f, 0.5f, 0.50f, 0.1f, route: RouteAuthored)); // the scene dissolves
                    break;

                case LegGrade.Won:
                    steps.Add(S(0.7f, 0.20f, 0.78f, 0.40f, 1f, MkSave, route: RouteAuthored, atkPicked: false));
                    foreach (ScoreLedger.StagedGoal g in plan.Goals)
                    {
                        steps.Add(S(1.1f, 0.70f, 0.42f, 0.62f, 1f));      // the sucker-punch break
                        steps.Add(S(1.4f, 0.975f, 0.5f, 0.72f, 1f, MkGoal, g, RouteShot));
                    }
                    steps.Add(S(1.8f, 0.5f, 0.5f, 0.72f, 1f, route: RouteAuthored)); // whistle — celebrate
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
                            steps.Add(S(0.5f, 0.02f, 0.5f, 0.26f, 1f, MkGoal, g, RouteShot, atkPicked: false));
                            first = false;
                        }
                        else
                        {
                            steps.Add(S(1.0f, 0.13f, 0.35f, 0.28f, 1f, atkPicked: false, chase: true));
                            steps.Add(S(1.5f, 0.025f, 0.5f, 0.26f, 1f, MkGoal, g, RouteShot, atkPicked: false));
                        }
                    }
                    steps.Add(S(1.8f, 0.5f, 0.5f, 0.26f, 0.1f, route: RouteAuthored, atkPicked: false)); // collapse
                    break;
            }
            return steps.ToArray();
        }

        // ------------------------------------------------------------------ scene visuals

        private void FlashGoal(bool right, bool strong)
        {
            _flashRight = right;
            _flashDur = strong ? 0.55f : 0.35f;
            _flashT = _flashDur;
            _flashRing.enabled = true;
            _flashRing.color = new Color(1f, 1f, 1f, strong ? 0.95f : 0.5f);
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
            if (_flashT <= 0f) _flashRing.enabled = false;
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
            var voidTint = new Color(0.42f, 0.56f, 0.62f, 1f);
            for (int i = 0; i < PitchLayout.OutfieldPerTeam; i++)
            {
                _homeDots[i].color = voidTint;
                _awayDots[i].color = voidTint;
            }
            _homeKeeper.color = voidTint;
            _awayKeeper.color = voidTint;
            _ball.color = new Color(0.62f, 0.86f, 0.96f, 0.9f);
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

        private void FindNearestTwo(bool home, Vector2 localPt, out int first, out int second)
        {
            Vector2[] side = home ? _homePos : _awayPos;
            first = second = -1;
            float d1 = float.PositiveInfinity, d2 = float.PositiveInfinity;
            for (int i = 0; i < side.Length; i++)
            {
                float d = (side[i] - localPt).sqrMagnitude;
                if (d < d1) { d2 = d1; second = first; d1 = d; first = i; }
                else if (d < d2) { d2 = d; second = i; }
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
