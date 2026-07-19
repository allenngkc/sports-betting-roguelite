using System;
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
            /// <summary>Signed win-probability movement from the previous beat (or TrueProb for the first).</summary>
            public readonly double Delta;
            public readonly double ProbAfter;

            public BeatRecord(int legIndex, int step, DramaEventType type, TensionTag tag, bool up,
                double delta, double probAfter)
            {
                LegIndex = legIndex;
                Step = step;
                Type = type;
                Tag = tag;
                Up = up;
                Delta = delta;
                ProbAfter = probAfter;
            }

            public BeatRecord(int legIndex, int step, DramaEventType type, TensionTag tag, bool up,
                double probAfter)
                : this(legIndex, step, type, tag, up, 0.0, probAfter) { }
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
            double delta = evt.WinProbAfter - _prevProb;
            bool up = delta >= 0.0;
            _prevProb = evt.WinProbAfter;
            _beats.Add(new BeatRecord(evt.LegIndex, evt.Step, evt.Type, evt.Tag, up, delta, evt.WinProbAfter));
            return up;
        }

        /// <summary>Maps a beat's absolute probability movement to the tape's dot size band.</summary>
        public static int MagnitudeBand(double delta)
        {
            double magnitude = Math.Abs(delta);
            if (magnitude < 0.04) return 0;
            if (magnitude < 0.10) return 1;
            return 2;
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
    /// The synthesized score ledger (F_0.2.0 M-T3) — causal + honest. Pure C#, EditMode-testable.
    ///
    /// Laws (the plan's §Score ledger, verbatim intent):
    ///  - Attribution: a Score/BigPlay beat up ⇒ picked-team goal, down ⇒ opponent goal;
    ///    Momentum/NearMiss ⇒ no goal.
    ///  - Live-lead clamp (<see cref="MaxLiveLead"/> = ±1, a dial): a goal that would push the
    ///    live lead beyond the clamp stages as the CHALKED-OFF variant — full drama, VAR
    ///    disallow, no increment. Deliberate drama law: the theater tells one-goal-game
    ///    stories, blowout scorelines never sweat.
    ///  - Commit timing: a goal commits when its playback COMPLETES (<see cref="CompleteGoal"/>
    ///    is the only score mutator), never on MoveNext — a suspended scene has committed nothing.
    ///  - Finals (<see cref="PlanFinal"/>): the scoreline entering any LegFinal is within ±1
    ///    (clamp consequence), so a correction needs at most 2 staged goals. Won ⇒ stoppage-time
    ///    goal(s) until the picked team is strictly ahead. Lost ⇒ the killing shot commits only
    ///    if the opponent is not already strictly ahead (already −1 ⇒ chalked at the death),
    ///    then correction goal(s) until the opponent is strictly ahead. Voided ⇒ the ledger
    ///    freezes as-is under the VOID treatment — no goals, no corrections.
    ///  - Goal-playback invariant: every increment maps 1:1 to a completed staged goal.
    /// </summary>
    public sealed class ScoreLedger
    {
        /// <summary>The live-lead clamp dial (±goals). 1 = one-goal-game stories.</summary>
        public int MaxLiveLead { get; set; } = 1;

        /// <summary>Committed goals for the picked side, this leg.</summary>
        public int Picked { get; private set; }

        /// <summary>Committed goals for the opponent, this leg.</summary>
        public int Opponent { get; private set; }

        /// <summary>Total committed goal playbacks this leg (the invariant's counter).</summary>
        public int CommittedGoals { get; private set; }

        /// <summary>A goal the choreographer staged: who it's for, and whether it commits
        /// (false = the chalked-off VAR-disallow variant — plays in full, never scores).</summary>
        public readonly struct StagedGoal
        {
            public readonly bool ForPicked;
            public readonly bool Commits;

            public StagedGoal(bool forPicked, bool commits)
            {
                ForPicked = forPicked;
                Commits = commits;
            }
        }

        /// <summary>The final whistle's staging order: the goals scene #12/#13 must play
        /// (killing shot first on a Lost, then corrections), each completed via
        /// <see cref="CompleteGoal"/> as its sub-scene finishes.</summary>
        public readonly struct FinalPlan
        {
            public readonly LegGrade Grade;
            public readonly StagedGoal[] Goals;

            public FinalPlan(LegGrade grade, StagedGoal[] goals)
            {
                Grade = grade;
                Goals = goals;
            }
        }

        /// <summary>Above this live prob the scoreboard must show the picked side ahead —
        /// the score is a lagging quantized rendering of the probability (playtest #14).</summary>
        public double ReconcileHighBand { get; set; } = 0.70;

        /// <summary>Below this live prob the scoreboard must show the opponent ahead.</summary>
        public double ReconcileLowBand { get; set; } = 0.30;

        /// <summary>Attribution + clamp for a non-final beat. Null = this beat stages no goal.
        ///
        /// Two goal sources (playtest #14 amendment — "the bar and the board must agree"):
        ///  1. Type attribution (the original law): Score/BigPlay beats stage a goal for the
        ///     beat's beneficiary.
        ///  2. Prob reconciliation: when the live probability says one side should be AHEAD
        ///     (outside the reconcile bands) and the board disagrees, a beat moving in that
        ///     direction stages the reconciling goal regardless of its type — the mid-leg
        ///     generalization of the final whistle's stoppage-time correction. Without this,
        ///     the board can read 0-0 at 90% or "leading" at 25%, which plays as fake.
        ///
        /// Near-miss exemption and the live-lead clamp are enforced by the caller/clamp as
        /// before; reconciliation targets ±1 so it can never violate the clamp.</summary>
        public StagedGoal? StageBeatGoal(DramaEventType type, bool up, double delta, double probAfter)
        {
            bool typeGoal = type == DramaEventType.Score || type == DramaEventType.BigPlay;
            if (typeGoal)
            {
                // Type goals keep the original direction rule (ties up — EventText's law);
                // their |delta| ≥ 0.07 means they are never actually flat.
                int typeLeadAfter = up ? Picked + 1 - Opponent : Opponent + 1 - Picked;
                return new StagedGoal(forPicked: up, commits: typeLeadAfter <= MaxLiveLead);
            }

            // The board the probability implies: +1 (picked ahead), -1 (opponent ahead), or
            // 0 (mid-band — any scoreline within the clamp is a fine story, including 1-0
            // either way; reconciliation never drags a natural lead back to level).
            //
            // Direction gate (Sol, M-T4.1): SIGN-COMPATIBILITY, not the tie-broken bool. A
            // flat beat (delta 0 — real inputs: paths riding the generator's 0.03/0.97
            // clamp) supports EITHER side's band; contrary motion never reconciles. Strict
            // inequality would leave a ceiling-riding 97% path unreconciled forever — the
            // exact dissonance this amendment exists to kill. The staged goal's direction
            // comes from the BAND, never from the tie-break.
            int impliedLead = probAfter >= ReconcileHighBand ? 1
                : probAfter <= ReconcileLowBand ? -1 : 0;
            int lead = Picked - Opponent;
            bool reconcileUp = impliedLead > 0 && lead < impliedLead && delta >= 0.0;
            bool reconcileDown = impliedLead < 0 && lead > impliedLead && delta <= 0.0;
            if (!reconcileUp && !reconcileDown) return null;

            bool forPicked = reconcileUp;
            int leadAfter = forPicked ? Picked + 1 - Opponent : Opponent + 1 - Picked;
            return new StagedGoal(forPicked, commits: leadAfter <= MaxLiveLead);
        }

        /// <summary>The ONLY score mutator — called when a staged goal's playback completes.
        /// A chalked-off goal (Commits false) completes without moving anything.</summary>
        public void CompleteGoal(StagedGoal goal)
        {
            if (!goal.Commits) return;
            if (goal.ForPicked) Picked++;
            else Opponent++;
            CommittedGoals++;
        }

        /// <summary>The stoppage-time staging order for a LegFinal, from the FINAL ticket-local
        /// grade (single presentation authority — a suspended scene's ending is never chosen
        /// from WinProbAfter). Pure planning: nothing commits until playback completes.</summary>
        public FinalPlan PlanFinal(LegGrade grade)
        {
            var goals = new System.Collections.Generic.List<StagedGoal>(3);
            int p = Picked, o = Opponent;
            switch (grade)
            {
                case LegGrade.Won:
                    while (p <= o) { goals.Add(new StagedGoal(true, true)); p++; }
                    break;
                case LegGrade.Lost:
                    // The killing shot commits only if the opponent is not already strictly
                    // ahead; otherwise it stages chalked-off — disallowed at the death, the
                    // whistle still confirms Lost. MaxLiveLead is never violated.
                    bool killingCommits = o <= p;
                    goals.Add(new StagedGoal(false, killingCommits));
                    if (killingCommits) o++;
                    while (o <= p) { goals.Add(new StagedGoal(false, true)); o++; }
                    break;
                case LegGrade.Voided:
                default:
                    break; // the scoreline freezes as-is under the cyan VOID treatment
            }
            return new FinalPlan(grade, goals.ToArray());
        }

        /// <summary>New leg, new match: the scoreline resets.</summary>
        public void ResetForLeg()
        {
            Picked = 0;
            Opponent = 0;
            CommittedGoals = 0;
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
