using System;
using UnityEngine;

namespace SBR.Game
{
    /// <summary>
    /// Procedural, diegetic audio for the TV sweat. The director owns its sources and clips so the
    /// graybox room needs no audio assets. It is presentation-only: it never changes the engine
    /// cursor or waits for a sound to finish.
    /// </summary>
    public sealed class TvAudioDirector : MonoBehaviour
    {
        public float masterVolume = 0.5f;
        public float crowdVolume = 0.6f;
        public float stingVolume = 0.8f;

        private const int SampleRate = 44100;
        private const float CrowdSeconds = 6f;
        private const float RiserSeconds = 8f;

        private AudioSource _crowdSource;
        private AudioSource _stingSource;
        private AudioSource _riserSource;
        private AudioLowPassFilter _crowdFilter;

        private AudioClip _crowdClip;
        private AudioClip _goalClip;
        private AudioClip _chalkedGoalClip;
        private AudioClip _riserClip;
        private AudioClip _whistleClip;
        private AudioClip _slamWonClip;
        private AudioClip _slamLostClip;
        private AudioClip _cashOutClip;

        private bool _shown;
        private float _crowdTarget;
        private float _crowdCurrent;
        private float _cutoffTarget = 650f;
        private float _cutoffCurrent = 650f;
        private float _duckTarget = 1f;
        private float _duckCurrent = 1f;
        private float _duckSeconds = 0.8f;
        private float _crowdDeflation = 1f;

        /// <summary>Creates the director and places all sources on the TV anchor.</summary>
        public static TvAudioDirector Build(Transform tvAnchor)
        {
            if (tvAnchor == null) return null;

            var go = new GameObject("TvAudioDirector", typeof(TvAudioDirector));
            go.transform.SetParent(tvAnchor, false);
            var director = go.GetComponent<TvAudioDirector>();

            // The crowd lives on its OWN child object: Unity audio filter components process
            // every AudioSource on their GameObject, and the 400-900Hz crowd low-pass would
            // otherwise mud the 1850Hz whistle and every sting (Fable review, M-T5).
            var crowdGo = new GameObject("CrowdBed");
            crowdGo.transform.SetParent(go.transform, false);
            director._crowdSource = ConfigureSource(crowdGo.AddComponent<AudioSource>());
            director._crowdFilter = crowdGo.AddComponent<AudioLowPassFilter>();
            director._crowdFilter.cutoffFrequency = director._cutoffCurrent;
            director._crowdFilter.lowpassResonanceQ = 1f;

            director._stingSource = ConfigureSource(go.AddComponent<AudioSource>());
            director._riserSource = ConfigureSource(go.AddComponent<AudioSource>());

            director._crowdClip = BuildCrowdBed();
            director._goalClip = BuildGoalClip(true);
            director._chalkedGoalClip = BuildGoalClip(false);
            director._riserClip = BuildRiserClip();
            director._whistleClip = BuildWhistleClip();
            director._slamWonClip = BuildSlamWonClip();
            director._slamLostClip = BuildSlamLostClip();
            director._cashOutClip = BuildCashOutClip();

            director._crowdSource.clip = director._crowdClip;
            director._crowdSource.loop = true;
            director._riserSource.clip = director._riserClip;
            director.ApplySourceVolumes();
            return director;
        }

        private static AudioSource ConfigureSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.spatialBlend = 0.8f;
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 0.4f;
            source.maxDistance = 18f;
            return source;
        }

        /// <summary>
        /// Tracks live danger and scene urgency. Parameter motion is smoothed here so the crowd
        /// swells rather than stepping with the probability bar.
        /// </summary>
        public void SetTension(float danger01, float urgency01)
        {
            danger01 = Mathf.Clamp01(danger01);
            urgency01 = Mathf.Clamp01(urgency01);
            float tension = Mathf.Clamp01(Mathf.Max(danger01, urgency01 * 0.85f));
            _crowdTarget = Mathf.Clamp01(crowdVolume) * (0.16f + tension * 0.84f);
            _cutoffTarget = Mathf.Lerp(400f, 900f, tension);

            float dt = Time.unscaledDeltaTime;
            float blend = dt <= 0f ? 0f : 1f - Mathf.Exp(-dt / 0.5f);
            _crowdCurrent = Mathf.Lerp(_crowdCurrent, _crowdTarget, blend);
            _cutoffCurrent = Mathf.Lerp(_cutoffCurrent, _cutoffTarget, blend);
            _crowdDeflation = Mathf.MoveTowards(_crowdDeflation, 1f, dt / 0.9f);
            if (_crowdFilter != null) _crowdFilter.cutoffFrequency = _cutoffCurrent;
            ApplySourceVolumes();
        }

        /// <summary>Applies the fast duck / slow recover dread floor.</summary>
        public void Duck(bool ducked, float seconds)
        {
            _duckTarget = ducked ? 0.025f : 1f;
            _duckSeconds = Mathf.Max(0.01f, seconds);
            float dt = Time.unscaledDeltaTime;
            if (dt > 0f)
                _duckCurrent = Mathf.MoveTowards(_duckCurrent, _duckTarget, dt / _duckSeconds);
            ApplySourceVolumes();
        }

        public void GoalHit(bool commits)
        {
            PlayOneShot(_stingSource, commits ? _goalClip : _chalkedGoalClip);
        }

        public void NearMissRiser(float seconds)
        {
            Stop(_riserSource);
            if (!_shown || _riserClip == null) return;
            _riserSource.pitch = Mathf.Clamp(RiserSeconds / Mathf.Max(0.05f, seconds), 0.05f, 3f);
            Play(_riserSource);
        }

        public void CutRiser()
        {
            Stop(_riserSource);
            if (_riserSource != null) _riserSource.pitch = 1f;
        }

        public void Whistle() => PlayOneShot(_stingSource, _whistleClip);

        public void SlamWon() => PlayOneShot(_stingSource, _slamWonClip);

        public void SlamLost()
        {
            _crowdDeflation = 0.08f;
            PlayOneShot(_stingSource, _slamLostClip);
        }

        public void CashOutKaChunk() => PlayOneShot(_stingSource, _cashOutClip);

        /// <summary>Shows or silences the TV's entire audio stack.</summary>
        public void Show(bool visible)
        {
            _shown = visible;
            if (!visible)
            {
                Stop(_crowdSource);
                Stop(_stingSource);
                Stop(_riserSource);
                if (_riserSource != null) _riserSource.pitch = 1f;
                return;
            }

            _duckTarget = 1f;
            _duckCurrent = 1f;
            _crowdDeflation = 1f;
            if (_crowdSource != null && !_crowdSource.isPlaying)
                Play(_crowdSource);
        }

        private void OnDisable() => Show(false);

        private void ApplySourceVolumes()
        {
            float master = Mathf.Clamp01(masterVolume) * _duckCurrent;
            if (_crowdSource != null)
                _crowdSource.volume = master * _crowdCurrent * _crowdDeflation;
            if (_stingSource != null) _stingSource.volume = master * Mathf.Clamp01(stingVolume);
            if (_riserSource != null) _riserSource.volume = master * Mathf.Clamp01(stingVolume);
        }

        private void Play(AudioSource source)
        {
            if (!_shown || source == null || Application.isBatchMode) return;
            try { source.Play(); } catch (Exception) { }
        }

        private void PlayOneShot(AudioSource source, AudioClip clip)
        {
            if (!_shown || source == null || clip == null || Application.isBatchMode) return;
            try { source.PlayOneShot(clip); } catch (Exception) { }
        }

        private static void Stop(AudioSource source)
        {
            if (source == null || Application.isBatchMode) return;
            try { source.Stop(); } catch (Exception) { }
        }

        private static AudioClip BuildCrowdBed()
        {
            int samples = Mathf.RoundToInt(CrowdSeconds * SampleRate);
            var data = new float[samples];
            var random = new System.Random(0x53425241);
            float filtered = 0f;
            float cutoff = 650f;
            float alpha = 1f - Mathf.Exp(-2f * Mathf.PI * cutoff / SampleRate);
            for (int i = 0; i < samples; i++)
            {
                float white = (float)(random.NextDouble() * 2.0 - 1.0);
                filtered += alpha * (white - filtered);
                float t = (float)i / SampleRate;
                float swell = 0.68f + 0.22f * Mathf.Sin(t * 2f * Mathf.PI * 0.11f)
                    + 0.10f * Mathf.Sin(t * 2f * Mathf.PI * 0.047f + 1.2f);
                data[i] = filtered * swell * 0.48f;
            }

            // A short crossfade keeps the loop boundary from clicking.
            int fade = Mathf.Min(SampleRate / 12, samples / 2);
            float boundary = (data[0] + data[samples - 1]) * 0.5f;
            for (int i = 0; i < fade; i++)
            {
                float t = (float)i / Mathf.Max(1, fade - 1);
                float blend = Mathf.SmoothStep(0f, 1f, t);
                data[i] = Mathf.Lerp(boundary, data[i], blend);
                data[samples - fade + i] = Mathf.Lerp(data[samples - fade + i], boundary, blend);
            }
            return CreateClip("CrowdBed", data);
        }

        private static AudioClip BuildGoalClip(bool commits)
        {
            float seconds = 0.85f;
            int samples = Mathf.RoundToInt(seconds * SampleRate);
            var data = new float[samples];
            var random = new System.Random(commits ? 0x474F414C : 0x4348414C);
            float filtered = 0f;
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float thumpT = Mathf.Min(1f, t / 0.22f);
                float thumpFreq = commits ? Mathf.Lerp(105f, 48f, thumpT) : Mathf.Lerp(125f, 62f, thumpT);
                float thump = Mathf.Sin(2f * Mathf.PI * thumpFreq * t) * Mathf.Exp(-13f * t) * 0.85f;
                float roarT = Mathf.Clamp01((t - 0.06f) / (commits ? 0.74f : 0.26f));
                float roarEnvelope = roarT * (1f - roarT) * 4f;
                float white = (float)(random.NextDouble() * 2.0 - 1.0);
                filtered += 0.12f * (white - filtered);
                float roar = filtered * roarEnvelope * (commits ? 0.95f : 0.58f);
                float groan = commits ? 0f
                    : Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(190f, 72f, roarT) * t)
                        * roarT * Mathf.Exp(-2.5f * t) * 0.42f;
                data[i] = Mathf.Clamp(thump + roar + groan, -1f, 1f);
            }
            return CreateClip(commits ? "GoalHit" : "ChalkedGoal", data);
        }

        private static AudioClip BuildRiserClip()
        {
            int samples = Mathf.RoundToInt(RiserSeconds * SampleRate);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / RiserSeconds;
                float freq = 170f * Mathf.Pow(5.5f, progress);
                float envelope = 0.06f + 0.42f * progress;
                data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t)
                    + 0.35f * Mathf.Sin(2f * Mathf.PI * freq * 2.01f * t)) * envelope;
            }
            return CreateClip("NearMissRiser", data);
        }

        private static AudioClip BuildWhistleClip()
        {
            float seconds = 0.72f;
            int samples = Mathf.RoundToInt(seconds * SampleRate);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                int peep = Mathf.FloorToInt(t / 0.23f);
                float local = t - peep * 0.23f;
                float envelope = peep < 3 && local < 0.12f
                    ? Mathf.Sin(Mathf.Clamp01(local / 0.012f) * Mathf.PI * 0.5f)
                    * (1f - Mathf.Clamp01((local - 0.08f) / 0.04f)) : 0f;
                data[i] = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 1850f * t)) * envelope * 0.28f;
            }
            return CreateClip("Whistle", data);
        }

        private static AudioClip BuildSlamWonClip()
        {
            float seconds = 0.9f;
            int samples = Mathf.RoundToInt(seconds * SampleRate);
            var data = new float[samples];
            float[] notes = { 523.25f, 659.25f, 783.99f };
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = Mathf.Sin(Mathf.Clamp01(t / seconds) * Mathf.PI) * 0.55f;
                float body = 0f;
                for (int n = 0; n < notes.Length; n++)
                    body += Mathf.Sin(2f * Mathf.PI * notes[n] * t) / notes.Length;
                data[i] = body * envelope;
            }
            return CreateClip("SlamWon", data);
        }

        private static AudioClip BuildSlamLostClip()
        {
            float seconds = 0.8f;
            int samples = Mathf.RoundToInt(seconds * SampleRate);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = Mathf.Exp(-5.5f * t);
                data[i] = (Mathf.Sin(2f * Mathf.PI * 74f * t)
                    + 0.28f * Mathf.Sin(2f * Mathf.PI * 39f * t)) * envelope * 0.62f;
            }
            return CreateClip("SlamLost", data);
        }

        private static AudioClip BuildCashOutClip()
        {
            float seconds = 0.46f;
            int samples = Mathf.RoundToInt(seconds * SampleRate);
            var data = new float[samples];
            var random = new System.Random(0x4B414348);
            float filtered = 0f;
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float click1 = Mathf.Exp(-Mathf.Abs(t - 0.045f) * 420f);
                float click2 = Mathf.Exp(-Mathf.Abs(t - 0.105f) * 420f);
                float white = (float)(random.NextDouble() * 2.0 - 1.0);
                filtered += 0.22f * (white - filtered);
                float clicks = filtered * (click1 + click2) * 0.5f;
                float thunk = Mathf.Sin(2f * Mathf.PI * 82f * Mathf.Max(0f, t - 0.17f))
                    * Mathf.Exp(-10f * Mathf.Max(0f, t - 0.17f));
                data[i] = Mathf.Clamp(clicks + thunk * 0.55f, -1f, 1f);
            }
            return CreateClip("CashOutKaChunk", data);
        }

        private static AudioClip CreateClip(string name, float[] data)
        {
            AudioClip clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
