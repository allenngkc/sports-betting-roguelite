using System;
using SBR.Engine;
using UnityEngine;
using UnityEngine.UI;

namespace SBR.Game
{
    /// <summary>
    /// The match theater's stage (F_0.2.0 M-T2 + M-T3 scenes): a top-down neon-on-black pitch
    /// where anonymous team-colored dots act out the drama stream. This is a RENDERER, never a
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
    /// M-T3: beats play as authored SCENES (goal buildups, breakaways, near-misses, finals with
    /// stoppage-time corrections) via <see cref="PlayScene"/>/<see cref="PlayFinalScene"/>;
    /// the pending-loss window suspends a kill scene at the shot mid-flight
    /// (<see cref="SuspendKillShot"/>) and resumes with the grade-chosen continuation
    /// (<see cref="ResumeSuspended"/>). Scene STEP TIME scales by <see cref="timeScale"/>
    /// (TvSweatScreen forwards its TimeScaleOverride) so batch tests fast-forward; dot motion
    /// runs on real frame time (it is only ever cosmetic). Freezing (stand-up pause) halts the
    /// whole stage mid-motion — the frozen frame IS the dread.
    /// </summary>
    public sealed class TheaterStage : MonoBehaviour
    {
        [Header("Feel dials")]
        [Tooltip("Seconds between idle possession retargets (min).")]
        public float passIntervalMin = 0.7f;
        [Tooltip("Seconds between idle possession retargets (max).")]
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
        private Vector2 _ballPos, _ballVel, _ballTarget;
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

        private static Sprite _circleSprite;
        private static Sprite _ringSprite;
        private static int s_seedSalt; // per-instance RNG salt (presentation-local, never engine)

        // ---- scene playback (M-T3) ----
        private const byte MkNone = 0, MkGoal = 1, MkSuspend = 2, MkSave = 3, MkVoid = 4;

        private struct Step
        {
            public float Dur;      // seconds (scaled by timeScale when advancing)
            public Vector2 Ball;   // normalized pitch target, PICKED frame (right = picked attack)
            public float Terr;     // territory the formations speak during this step
            public float Tempo;    // 0..1 urgency (actor speed, pass tempo)
            public byte Marker;    // fired at step END
            public ScoreLedger.StagedGoal Goal; // rides MkGoal
        }

        private Step[] _script;
        private int _stepIx;
        private float _stepT;
        private bool _suspendedAtShot;
        private float _sceneTerr = 0.5f;
        private float _sceneTerrVel;
        private Action<ScoreLedger.StagedGoal> _onGoalPlayed;
        private Action _onSceneComplete;
        private float _flashT;         // net-ripple flash countdown
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
        /// snap to kickoff formation; the ball to the center spot.</summary>
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

            _ballPos = _ballTarget = Vector2.zero;
            _ballVel = Vector2.zero;
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
        /// <paramref name="onComplete"/> when the script exhausts. Poll <see cref="ScenePlaying"/>.</summary>
        public void PlayScene(SceneSpec spec, Action<ScoreLedger.StagedGoal> onGoalPlayed, Action onComplete)
        {
            StartScript(BuildBeatScript(spec), onGoalPlayed, onComplete);
        }

        /// <summary>Plays the final whistle sequence: pre-reveal hold → the plan's staged
        /// goal(s) as separately-timed sub-scenes → celebrate/collapse. The GREEN/DEAD slam
        /// itself belongs to the orchestrator (TvLight sync) at completion.</summary>
        public void PlayFinalScene(SceneSpec spec, ScoreLedger.FinalPlan plan,
            Action<ScoreLedger.StagedGoal> onGoalPlayed, Action onComplete)
        {
            StartScript(BuildFinalScript(spec, plan), onGoalPlayed, onComplete);
        }

        /// <summary>The pending-loss window's kill scene: opponent buildup → shot launched →
        /// FROZEN at the suspension point, mid-flight. Holds until <see cref="ResumeSuspended"/>.</summary>
        public void SuspendKillShot(int variant)
        {
            StartScript(BuildKillShotScript(variant), null, null);
        }

        /// <summary>The suspended scene's continuation, chosen from the FINAL ticket-local grade
        /// (never WinProbAfter): Voided → cyan VOID dissolve, no goals; Won → the frozen shot
        /// resolves as a save, then the counter goal(s) the correction needs; Lost → the flight
        /// completes (chalked if the entry score already satisfied Lost), then corrections.</summary>
        public void ResumeSuspended(ScoreLedger.FinalPlan plan,
            Action<ScoreLedger.StagedGoal> onGoalPlayed, Action onComplete)
        {
            _suspendedAtShot = false;
            StartScript(BuildContinuationScript(plan), onGoalPlayed, onComplete);
        }

        /// <summary>Abandons any active scene without completing it (cash-out, leg change).</summary>
        public void CancelScene()
        {
            _script = null;
            _stepIx = 0;
            _stepT = 0f;
            _suspendedAtShot = false;
            _onGoalPlayed = null;
            _onSceneComplete = null;
        }

        private void StartScript(Step[] script, Action<ScoreLedger.StagedGoal> onGoalPlayed, Action onComplete)
        {
            _script = script;
            _stepIx = 0;
            _stepT = 0f;
            _suspendedAtShot = false;
            _onGoalPlayed = onGoalPlayed;
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

            Step s = _script[_stepIx];

            // Motion (real frame time — cosmetic): formations speak the scene's territory,
            // the ball flies the authored route, tempo scales everything.
            float speed = 1f + s.Tempo * 1.6f;
            _sceneTerr = Mathf.SmoothDamp(_sceneTerr, s.Terr, ref _sceneTerrVel, 0.45f / speed);
            LastTerritoryX = _sceneTerr;
            MoveActors(_sceneTerr, speed, dt);
            _ballTarget = ToLocal(PickedFrame(s.Ball));
            _ballPos = Vector2.SmoothDamp(_ballPos, _ballTarget, ref _ballVel, ballDamp / speed);
            ApplyPositions();

            // Story time (scaled — batch tests fast-forward through here by shrinking the
            // effective duration, exactly like SeatedHold does with TimeScaleOverride).
            _stepT += dt;
            if (_stepT < s.Dur * Mathf.Max(0f, timeScale)) return;

            FireMarker(s);
            if (_suspendedAtShot) return; // MkSuspend holds ON its step until resumed

            _stepIx++;
            _stepT = 0f;
            if (_stepIx >= _script.Length)
            {
                _script = null;
                Action done = _onSceneComplete;
                _onGoalPlayed = null;
                _onSceneComplete = null;
                done?.Invoke();
            }
        }

        private void FireMarker(Step s)
        {
            switch (s.Marker)
            {
                case MkGoal:
                    // Net ripple at the goal the ball attacked; a chalked-off goal ripples
                    // dimmer — VAR takes it away, the flavor line says so (orchestrator).
                    FlashGoal(right: PickedFrame(s.Ball).x > 0.5f, strong: s.Goal.Commits);
                    _onGoalPlayed?.Invoke(s.Goal);
                    break;
                case MkSuspend:
                    _suspendedAtShot = true;
                    break;
                case MkSave:
                    KeeperLunge();
                    break;
                case MkVoid:
                    ApplyVoidTint();
                    break;
            }
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
            MoveActors(terr, speed, dt);

            // Possession recycling: the ball hops between actors near the territory point.
            if (Time.time >= _nextPassAt)
            {
                _nextPassAt = Time.time + NextPassDelay() / speed;
                _ballTarget = PickCarrier(terr);
                for (int i = 0; i < PitchLayout.OutfieldPerTeam; i++)
                {
                    if (_rng.NextDouble() < 0.3) RerollNoise(ref _homeNoise[i]);
                    if (_rng.NextDouble() < 0.3) RerollNoise(ref _awayNoise[i]);
                }
            }
            _ballPos = Vector2.SmoothDamp(_ballPos, _ballTarget, ref _ballVel, ballDamp / speed);

            ApplyPositions();
        }

        /// <summary>Formation motion shared by idle and scenes: dots damp toward their biased
        /// slots, keepers hold their lines.</summary>
        private void MoveActors(float terr, float speed, float dt)
        {
            float bias = (terr - 0.5f) * 2f; // [-1, 1] toward the right goal
            float damp = dotDamp / speed;

            for (int i = 0; i < PitchLayout.OutfieldPerTeam; i++)
            {
                // Home's shape pushes with the territory when it attacks right, against it otherwise.
                float homeBias = _homeAttacksRight ? bias : -bias;
                Vector2 ht = ToLocal(PitchLayout.FormationSlot(i, _homeAttacksRight, homeBias)) + _homeNoise[i];
                Vector2 at = ToLocal(PitchLayout.FormationSlot(i, !_homeAttacksRight, -homeBias)) + _awayNoise[i];
                _homePos[i] = Vector2.SmoothDamp(_homePos[i], ht, ref _homeVel[i], damp);
                _awayPos[i] = Vector2.SmoothDamp(_awayPos[i], at, ref _awayVel[i], damp);
            }
            _hkPos = Vector2.SmoothDamp(_hkPos, ToLocal(PitchLayout.Keeper(_homeAttacksRight)), ref _hkVel, dotDamp);
            _akPos = Vector2.SmoothDamp(_akPos, ToLocal(PitchLayout.Keeper(!_homeAttacksRight)), ref _akVel, dotDamp);
        }

        /// <summary>A pass target near the territory point. Possession share restates the live
        /// prob (allowed: it repeats revealed state), drawn from presentation-local RNG only.</summary>
        private Vector2 PickCarrier(float terr)
        {
            bool pickedHasIt = _rng.NextDouble() < Mathf.Lerp(0.25f, 0.75f, _prob);
            bool homeHasIt = _homeAttacksRight == pickedHasIt;
            Vector2[] side = homeHasIt ? _homePos : _awayPos;

            // Prefer carriers near the territory x — pick the closest of three random dots.
            float terrX = (terr - 0.5f) * (_w - Pad * 2f);
            Vector2 best = side[_rng.Next(side.Length)];
            for (int tries = 0; tries < 2; tries++)
            {
                Vector2 candidate = side[_rng.Next(side.Length)];
                if (Mathf.Abs(candidate.x - terrX) < Mathf.Abs(best.x - terrX)) best = candidate;
            }
            return best + new Vector2(Rand(-14f, 14f), Rand(-14f, 14f));
        }

        // ------------------------------------------------------------------ scene scripts

        /// <summary>Variant lanes: which flank the move runs down (EventText's variant trick).</summary>
        private static float Lane(int variant) => variant == 0 ? 0.5f : variant == 1 ? 0.32f : 0.68f;

        private static Step S(float dur, float bx, float by, float terr, float tempo,
            byte marker = MkNone, ScoreLedger.StagedGoal goal = default)
            => new Step { Dur = dur, Ball = new Vector2(bx, by), Terr = terr, Tempo = tempo, Marker = marker, Goal = goal };

        /// <summary>Mirrors a picked-frame script across the halfway line (for/against pairs
        /// share one author).</summary>
        private static Step[] Mirror(Step[] steps)
        {
            var m = new Step[steps.Length];
            for (int i = 0; i < m.Length; i++)
            {
                m[i] = steps[i];
                m[i].Ball = new Vector2(1f - m[i].Ball.x, m[i].Ball.y);
                m[i].Terr = 1f - m[i].Terr;
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
                        S(B * 0.14f, 0.965f, 0.5f, 0.72f, 1f),
                        S(B * 0.14f, 0.985f, 0.5f, 0.72f, 1f, MkGoal, goal),
                        commits
                            ? S(B * 0.18f, 0.5f, 0.5f, 0.55f, 0.4f)      // kickoff restart (#14)
                            : S(B * 0.18f, 0.82f, 0.25f, 0.60f, 0.3f),   // chalked: goal kick, play on
                    };
                    if (spec.Template == SceneTemplate.GoalAgainst) core = Mirror(core);
                    break;

                case SceneTemplate.BreakawayFor:
                case SceneTemplate.BreakawayAgainst:
                    core = new[]
                    {
                        S(B * 0.22f, 0.30f, 0.5f, 0.42f, 0.5f),          // they had it — turnover
                        S(B * 0.30f, 0.70f, lane, 0.58f, 1f),            // the long carry
                        S(B * 0.14f, 0.88f, lane, 0.66f, 1f),
                        S(B * 0.14f, 0.965f, spec.Variant == 2 ? 0.58f : 0.42f, 0.70f, 1f, MkGoal, goal),
                        commits
                            ? S(B * 0.20f, 0.5f, 0.5f, 0.55f, 0.4f)
                            : S(B * 0.20f, 0.82f, 0.25f, 0.60f, 0.3f),
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
                        S(B * 0.12f, 0.99f, 0.47f, 0.72f, 1f),           // the shot
                        S(B * 0.10f, 0.94f, 0.82f, 0.70f, 1f, MkSave),   // off the bar / full-stretch
                        S(B * 0.12f, 0.92f, 0.80f, 0.70f, 0f),           // the hold — dead air
                        S(B * 0.20f, 0.62f, 0.5f, 0.58f, 0.3f),          // cleared, hearts restart
                    };
                    if (spec.Template == SceneTemplate.NearMissScare) core = Mirror(core);
                    break;

                case SceneTemplate.CalmPossession:
                    core = new[]
                    {
                        S(B * 0.35f, 0.46f, 0.40f, 0.50f, 0.2f),
                        S(B * 0.35f, 0.54f, 0.60f, 0.50f, 0.2f),
                        S(B * 0.30f, 0.48f, 0.5f, 0.50f, 0.15f),
                    };
                    break;

                case SceneTemplate.Kickoff:
                    core = new[] { S(B, 0.5f, 0.5f, 0.5f, 0.3f) };
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
                        S(B * 0.40f, 0.60f, lane, 0.55f, 0.5f),
                        S(B * 0.30f, 0.72f, 1f - lane, 0.58f, 0.5f),
                        S(B * 0.30f, 0.50f, 0.5f, 0.52f, 0.3f),
                    };
                    break;
            }

            if (intro <= 0f) return core;

            // Steal → transition: the ball flips flanks at midfield before the move starts.
            var withIntro = new Step[core.Length + 1];
            withIntro[0] = S(intro, 0.42f, 1f - lane, 0.46f, 1f);
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
                    steps.Add(S(1.4f, 0.975f, 0.5f, 0.74f, 1f, MkGoal, g));
                }
                steps.Add(S(B * 0.33f, 0.5f, 0.5f, 0.72f, 1f));      // whistle — celebrate
            }
            else
            {
                steps.Add(S(B * 0.40f, 0.38f, 0.5f, 0.36f, 0.6f));   // the dread hold
                steps.Add(S(B * 0.27f, 0.22f, lane, 0.30f, 0.8f));
                foreach (ScoreLedger.StagedGoal g in plan.Goals)
                {
                    steps.Add(S(1.0f, 0.13f, lane, 0.28f, 1f));      // the killing approach
                    steps.Add(S(1.5f, 0.025f, 0.5f, 0.26f, 1f, MkGoal, g));
                }
                steps.Add(S(B * 0.33f, 0.5f, 0.5f, 0.26f, 0.1f));    // whistle — collapse
            }
            return steps.ToArray();
        }

        private Step[] BuildKillShotScript(int variant)
        {
            float lane = Lane(variant);
            return new[]
            {
                S(0.9f, 0.30f, lane, 0.34f, 0.8f),                   // opponent buildup
                S(0.6f, 0.14f, lane, 0.30f, 1f),                     // the approach
                S(0.35f, 0.10f, 0.5f, 0.28f, 1f),                    // shot launched
                S(0.20f, 0.05f, 0.5f, 0.28f, 1f, MkSuspend),         // FROZEN mid-flight
            };
        }

        private Step[] BuildContinuationScript(ScoreLedger.FinalPlan plan)
        {
            var steps = new System.Collections.Generic.List<Step>(2 + plan.Goals.Length * 2);
            switch (plan.Grade)
            {
                case LegGrade.Voided:
                    steps.Add(S(0.9f, 0.06f, 0.5f, 0.40f, 0.1f, MkVoid)); // the slip comes out
                    steps.Add(S(1.4f, 0.5f, 0.5f, 0.50f, 0.1f));          // the scene dissolves
                    break;

                case LegGrade.Won:
                    steps.Add(S(0.7f, 0.20f, 0.78f, 0.40f, 1f, MkSave));  // the shot dies — parried
                    foreach (ScoreLedger.StagedGoal g in plan.Goals)
                    {
                        steps.Add(S(1.1f, 0.70f, 0.42f, 0.62f, 1f));      // the sucker-punch break
                        steps.Add(S(1.4f, 0.975f, 0.5f, 0.72f, 1f, MkGoal, g));
                    }
                    steps.Add(S(1.8f, 0.5f, 0.5f, 0.72f, 1f));            // whistle — celebrate
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
                            steps.Add(S(0.5f, 0.02f, 0.5f, 0.26f, 1f, MkGoal, g));
                            first = false;
                        }
                        else
                        {
                            steps.Add(S(1.0f, 0.13f, 0.35f, 0.28f, 1f));
                            steps.Add(S(1.5f, 0.025f, 0.5f, 0.26f, 1f, MkGoal, g));
                        }
                    }
                    steps.Add(S(1.8f, 0.5f, 0.5f, 0.26f, 0.1f));          // whistle — collapse
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
            // the smooth-damp brings them home after.
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

        // ------------------------------------------------------------------ helpers

        /// <summary>Scripts are authored in the PICKED frame (right = picked's attack). The
        /// picked side always attacks right on this stage, so this is the identity — kept as
        /// the single seam if that law ever changes.</summary>
        private static Vector2 PickedFrame(Vector2 v) => v;

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
