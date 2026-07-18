using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Game
{
    /// <summary>
    /// Pure-C# model behind the match theater (F_0.2.0). No UnityEngine — EditMode-testable.
    /// Owns what every theater surface must AGREE on: deterministic team colors from a
    /// non-reserved pool (design/08 palette law: green/red/gold are money signals, cyan is
    /// VOID — never team identity), the beat direction rule (EventText's law: sign of the
    /// win-prob move for the picked side, first beat against the leg's TrueProb anchor),
    /// and the beat history the stage/tape/scorebug all read. Consumes no engine RNG.
    /// </summary>
    public sealed class SweatPresentationModel
    {
        /// <summary>One recorded beat, presentation-local.</summary>
        public readonly struct BeatRecord
        {
            public readonly int LegIndex;
            public readonly int Step;
            public readonly DramaEventType Type;
            public readonly TensionTag Tag;
            /// <summary>True when the beat moved the PICKED side's win probability up (ties count up).</summary>
            public readonly bool Up;
            public readonly double ProbAfter;

            public BeatRecord(int legIndex, int step, DramaEventType type, TensionTag tag, bool up, double probAfter)
            {
                LegIndex = legIndex;
                Step = step;
                Type = type;
                Tag = tag;
                Up = up;
                ProbAfter = probAfter;
            }
        }

        private readonly List<BeatRecord> _beats = new List<BeatRecord>();
        private int _anchorLeg = -1;
        private double _prevProb;

        public IReadOnlyList<BeatRecord> Beats => _beats;

        /// <summary>Records a beat and returns its direction (the shared rule — one authority).</summary>
        public bool RecordBeat(DramaEvent evt, Leg leg)
        {
            if (evt.LegIndex != _anchorLeg)
            {
                _anchorLeg = evt.LegIndex;
                _prevProb = leg.TrueProb; // the pre-event anchor, exactly EventText's rule
            }
            bool up = evt.WinProbAfter >= _prevProb;
            _prevProb = evt.WinProbAfter;
            _beats.Add(new BeatRecord(evt.LegIndex, evt.Step, evt.Type, evt.Tag, up, evt.WinProbAfter));
            return up;
        }

        /// <summary>New ticket — beat history and the direction anchor reset.</summary>
        public void ResetForTicket()
        {
            _beats.Clear();
            _anchorLeg = -1;
            _prevProb = 0.0;
        }
    }

    /// <summary>
    /// The theater's team-color law (F_0.2.0): colors come from a fixed pool that excludes
    /// every reserved signal color — phosphor green (money-good), hot red (money-bad),
    /// gold (cash-out), cyan (VOID). Assignment is deterministic from the team NAME
    /// (FNV-1a, presentation-local — a team keeps its color across rounds and replays,
    /// no engine RNG involved); the away team takes the next distinct pool entry.
    /// </summary>
    public static class TheaterPalette
    {
        /// <summary>0xRRGGBB. Electric blue, magenta, orange, violet, broadcast white.</summary>
        public static readonly uint[] TeamPool = { 0x3D7BFF, 0xE84DD0, 0xFF8A2B, 0x9B5CF6, 0xF0F3F6 };

        public static (uint Home, uint Away) TeamColors(string homeName, string awayName)
        {
            int home = (int)(Fnv1a(homeName ?? string.Empty) % (uint)TeamPool.Length);
            int away = (int)(Fnv1a(awayName ?? string.Empty) % (uint)TeamPool.Length);
            if (away == home) away = (away + 1) % TeamPool.Length;
            return (TeamPool[home], TeamPool[away]);
        }

        private static uint Fnv1a(string s)
        {
            unchecked
            {
                uint h = 2166136261;
                for (int i = 0; i < s.Length; i++)
                {
                    h ^= s[i];
                    h *= 16777619;
                }
                return h;
            }
        }
    }
}
