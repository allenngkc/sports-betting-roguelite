using System;
using SBR.Engine;
using UnityEngine;
using UnityEngine.UI;

namespace SBR.Game
{
    /// <summary>
    /// The match theater's stage (F_0.2.0 M-T2): a top-down neon-on-black pitch where
    /// anonymous team-colored dots hold territory while the sweat plays. This is a RENDERER,
    /// never a simulation — the theater laws (design/04, superseded ruling 2026-07-18):
    ///
    ///  1. Idle motion never signifies. Between beats the dots recycle possession around a
    ///     territory point that RESTATES the last revealed win probability — no goals, no
    ///     saves, no probability movement is ever implied by filler.
    ///  2. Presentation-local RNG only (System.Random here; the engine's streams are sacred).
    ///  3. The palette law extends onto the stage: the pitch is near-black (never money-green),
    ///     team dots come from TheaterPalette's non-reserved pool (never hot red), lines are
    ///     neutral broadcast chrome.
    ///
    /// The PICKED team always attacks RIGHT: ball drifting right = money coming home. Beats
    /// arrive via <see cref="Pulse"/> (M-T2: a territory impulse + urgency; the full scene
    /// vocabulary lands in M-T3). Freezing (stand-up pause, pending-loss window) halts the
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
        [Tooltip("How far a beat impulse shoves territory, in pitch fraction.")]
        public float pulseKick = 0.22f;
        [Tooltip("Seconds a beat impulse takes to decay back to the honest territory point.")]
        public float pulseDecay = 1.6f;

        private RectTransform _rt;
        private float _w, _h;
        private const float Pad = 12f;

        private Image[] _homeDots;
        private Image[] _awayDots;
        private Image _homeKeeper, _awayKeeper, _ball;

        private Vector2[] _homePos, _awayPos, _homeVel, _awayVel, _homeNoise, _awayNoise;
        private Vector2 _ballPos, _ballVel, _ballTarget;
        private Vector2 _hkPos, _akPos, _hkVel, _akVel;

        private System.Random _rng;
        private bool _frozen;
        private bool _live;
        private bool _homeAttacksRight = true;
        private float _prob = 0.5f;      // picked side's last revealed win prob — territory truth
        private float _pulse;            // transient beat impulse, decays to 0
        private float _pulseSign;
        private float _nextPassAt;
        private float _urgency;          // 0..1, briefly raised by big beats

        private static Sprite _circleSprite;
        private static Sprite _ringSprite;
        private static int s_seedSalt; // per-instance RNG salt (presentation-local, never engine)

        // ---- public test/debug surface ----
        /// <summary>The territory x the last Update spoke (honest prob + decaying pulse), in
        /// normalized pitch space. Test/debug only — gameplay never reads the stage.</summary>
        public float LastTerritoryX { get; private set; } = 0.5f;
        public bool IsFrozen => _frozen;

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
            _homeAttacksRight = pickedIsHome;
            _prob = Mathf.Clamp01(openingProb);
            _pulse = 0f;
            _urgency = 0f;
            _live = true;
            LastTerritoryX = PitchLayout.TerritoryX(_prob);

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

            _ballPos = _ballTarget = Vector2.zero;
            _ballVel = Vector2.zero;
            _nextPassAt = Time.time + NextPassDelay();
            ApplyPositions();
        }

        /// <summary>The honest live probability of the picked side — the territory truth the
        /// idle loop restates. Only ever called on a real beat (never invented by the stage).</summary>
        public void SetLiveProb(float pickedProb) => _prob = Mathf.Clamp01(pickedProb);

        /// <summary>A beat landed: shove territory toward the beneficiary's end and raise the
        /// tempo briefly. M-T2's minimal beat language — M-T3 replaces this with full scenes.</summary>
        public void Pulse(bool up, TensionTag tag)
        {
            _pulseSign = up ? 1f : -1f;
            _pulse = 1f;
            _urgency = tag == TensionTag.Calm ? Mathf.Max(_urgency, 0.25f) : 1f;
        }

        /// <summary>Freeze mid-motion (stand-up pause, the pending-loss window). The stage holds
        /// its exact frame — the frozen ball is the dread.</summary>
        public void SetFrozen(bool frozen) => _frozen = frozen;

        public void Show(bool visible)
        {
            if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
            if (!visible) _live = false;
        }

        // ------------------------------------------------------------------ the idle loop

        private void Update()
        {
            if (!_live || _frozen) return;
            float dt = Time.deltaTime;

            _pulse = Mathf.MoveTowards(_pulse, 0f, dt / Mathf.Max(0.01f, pulseDecay));
            _urgency = Mathf.MoveTowards(_urgency, 0f, dt * 0.5f);

            // Territory: the honest prob, plus the decaying beat impulse. The picked side
            // attacks right, so territoryX already speaks the pick's language.
            float terr = PitchLayout.TerritoryX(_prob) + _pulse * _pulseSign * pulseKick;
            terr = Mathf.Clamp(terr, 0.08f, 0.92f);
            LastTerritoryX = terr;
            float bias = (terr - 0.5f) * 2f; // [-1, 1] toward the right goal

            float speed = 1f + _urgency * 1.4f;
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
