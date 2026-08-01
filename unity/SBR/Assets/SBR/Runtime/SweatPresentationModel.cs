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
            /// <summary>Money direction: does this goal move toward the PICKED OUTCOME? Drives
            /// the GoalFor/GoalAgainst template and hope/dread. For an Under/No bettor a goal
            /// is always dread, whichever team scores (Sol, F_0.4.0 P3 review).</summary>
            public readonly bool ForPicked;
            /// <summary>Which SIDE scores (presentation anchor): the counter this goal moves and
            /// the team the stage animates. Equals ForPicked on moneyline legs, where the pick
            /// IS a team; market legs decouple the two.</summary>
            public readonly bool ScoredByPicked;
            public readonly bool Commits;
            /// <summary>Endpoint reconciliation may reveal several baked goals in one
            /// stoppage-time playback so the 60–90s sweat law does not turn a rare blowout
            /// into an unbounded scene sequence.</summary>
            public readonly int Amount;
            /// <summary>TVS-H03: bound at PLAN time (see <see cref="ScoreLedger.BindAnytimeScorer"/>),
            /// never reconciled afterward — the one roster identity both the reveal copy and the
            /// stage's actor routing must read, so they can never disagree. False (the struct
            /// default) for every goal on every non-anytime-scorer leg, and for every goal on an
            /// anytime-scorer leg except its single causal reveal goal on a Won leg.</summary>
            public readonly bool HasBoundScorer;
            /// <summary>True when the bound actor plays for the home side. Meaningless unless
            /// <see cref="HasBoundScorer"/>.</summary>
            public readonly bool ScorerIsHome;
            /// <summary>The bound actor's index within their own team's roster (never the
            /// matchup-global <c>Leg.Selection.PlayerIndex</c>). Meaningless unless
            /// <see cref="HasBoundScorer"/>.</summary>
            public readonly int ScorerRosterIndex;

            public StagedGoal(bool forPicked, bool commits, int amount = 1)
                : this(forPicked, forPicked, commits, amount) { }

            public StagedGoal(bool forPicked, bool scoredByPicked, bool commits, int amount)
                : this(forPicked, scoredByPicked, commits, amount, false, false, -1) { }

            private StagedGoal(bool forPicked, bool scoredByPicked, bool commits, int amount,
                bool hasBoundScorer, bool scorerIsHome, int scorerRosterIndex)
            {
                ForPicked = forPicked;
                ScoredByPicked = scoredByPicked;
                Commits = commits;
                Amount = Math.Max(0, amount);
                HasBoundScorer = hasBoundScorer;
                ScorerIsHome = scorerIsHome;
                ScorerRosterIndex = scorerRosterIndex;
            }

            /// <summary>Returns this goal with a roster identity bound to it. Presentation-only:
            /// the identity comes from the leg's own backed selection (see
            /// <see cref="ScoreLedger.BindAnytimeScorer"/>), never RNG, never spatial proximity.</summary>
            public StagedGoal WithBoundScorer(bool isHome, int rosterIndex)
                => new StagedGoal(ForPicked, ScoredByPicked, Commits, Amount, true, isHome, rosterIndex);
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

        private bool _hasEndpoint;
        private int _targetPicked;
        private int _targetOpponent;

        /// <summary>How goals relate to the picked outcome (Sol, F_0.4.0 P3 review):
        /// 0 = moneyline (a picked-team goal helps); +1 = goals help the pick (Over, BTTS Yes);
        /// −1 = goals hurt the pick (Under, BTTS No).</summary>
        private int _goalSense;

        /// <summary>Scorer legs: goals stage as neutral theater, but the ML prob↔lead
        /// reconciliation bands must not fire (the prob is not a lead).</summary>
        private bool _suppressBandReconcile;

        /// <summary>T17 (Design Director: "reserve, don't spend"): how many of the backed side's
        /// baked goals ordinary beats may NOT spend, so the final sequence always still has one
        /// left to reveal. 1 on an anytime-scorer leg that bakes at least one backed-side goal,
        /// 0 on every other market — whose arithmetic is therefore bit-for-bit unchanged.
        ///
        /// <para>The gap this closes: <see cref="BindAnytimeScorer"/> binds the backed player onto
        /// a goal that is ACTUALLY ABOUT TO PLAY, and deliberately never invents one — inventing
        /// would move the causal reveal point (PRD §4.1). So when ordinary beats had spent every
        /// backed-side baked goal, <see cref="PlanFinal"/> had no backed-side correction left,
        /// nothing bound, and a WON anytime-scorer bet paid out without ever revealing who scored.
        /// Claiming the goal up front, before the beats can spend it, is why the fix lives here and
        /// not in BindAnytimeScorer: the reveal point is unmoved, it is merely still available.</para></summary>
        private int _reservePicked;

        /// <summary>Set once <see cref="PlanFinal"/> has run. The final sequence is the reserve's
        /// entire purpose, so from that point the backed side may spend down to its true endpoint
        /// and the scoreline still converges exactly on the locked stat line.</summary>
        private bool _reserveReleased;

        /// <summary>The picked-side ceiling ORDINARY play may reach. Equal to
        /// <see cref="TargetPicked"/> for every non-scorer leg, and for every leg once the reserve
        /// is released.</summary>
        private int SpendableTargetPicked
            => _reserveReleased ? _targetPicked : Math.Max(0, _targetPicked - _reservePicked);

        /// <summary>Whether this ledger has been bound to the locked match stat line.</summary>
        public bool HasEndpoint => _hasEndpoint;
        public int TargetPicked => _targetPicked;
        public int TargetOpponent => _targetOpponent;

        /// <summary>T17: the backed-side goal ordinary beats are forbidden to spend (0 or 1).
        /// Exposed so a test can assert the reserve HELD, rather than inferring it from a score.</summary>
        public int ReservedPicked => _reserveReleased ? 0 : _reservePicked;

        /// <summary>Binds the presentation ledger to the locked score endpoint. Non-moneyline
        /// markets use the home side as their presentation anchor; the market itself, not a
        /// phantom team pick, remains the source of direction and grading.</summary>
        public void ConfigureEndpoint(MatchStatLine statLine, bool pickedHome)
        {
            if (statLine == null) throw new ArgumentNullException(nameof(statLine));
            _hasEndpoint = true;
            _goalSense = 0; // moneyline unless the Leg overload widens it
            _suppressBandReconcile = false;
            _reservePicked = 0; // no reserve unless the Leg overload declares a scorer leg
            _targetPicked = pickedHome ? statLine.HomeGoals : statLine.AwayGoals;
            _targetOpponent = pickedHome ? statLine.AwayGoals : statLine.HomeGoals;
            ResetForLeg();
        }

        /// <summary>Convenience binding for a ticket leg. The locked stat line is read-only
        /// presentation input; no engine cursor or RNG is touched.</summary>
        public void ConfigureEndpoint(Leg leg)
        {
            if (leg == null) throw new ArgumentNullException(nameof(leg));
            // TVS-H03 follow-up: this MUST be the exact same "picked" anchor the stage and every
            // other renderer use (SweatFlavor.PickedHomeForPresentation), not a locally-duplicated
            // formula. They agreed for every market except anytime-scorer (whose backed player can
            // be on either side) — a duplicated formula here silently forced this ledger's
            // Picked/Opponent counters onto home/away while the stage's attacking-side convention
            // followed the backed player's real side, so a goal this ledger called "Picked" could
            // be the stage's "Opponent" for an away-backed scorer leg. Calling the shared helper
            // makes the two conventions structurally identical instead of coincidentally aligned.
            ConfigureEndpoint(leg.Matchup.StatLine, SweatFlavor.PickedHomeForPresentation(leg));
            // Goal sense binds only where GOALS are the market. Corners/cards legs keep the
            // neutral home-anchored goal decoration — their market coupling lives in the
            // CountLedger, not here.
            _goalSense = leg.Selection.Kind != MarketKind.TotalGoals
                    && leg.Selection.Kind != MarketKind.BothTeamsToScore ? 0
                : leg.Selection.Choice == MarketChoice.Over || leg.Selection.Choice == MarketChoice.Yes ? 1
                : -1;
            // The ML prob↔lead reconciliation bands assume the live prob tracks a LEAD. On a
            // scorer leg it tracks one player's chance — the bands are meaningless there
            // (F_0.4.0 P4 review; same law as the market-leg skip above).
            _suppressBandReconcile = leg.Selection.Kind == MarketKind.AnytimeScorer;
            // T17: claim the reveal goal HERE, at configure time, before a single beat has run.
            // Guarded on TargetPicked so a stat line that bakes no backed-side goal reserves
            // nothing — reserving 1 of 0 would starve the ordinary beats to no purpose, and such a
            // leg cannot grade Won as an anytime-scorer anyway.
            _reservePicked = leg.Selection.Kind == MarketKind.AnytimeScorer && _targetPicked >= 1 ? 1 : 0;
        }

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

            // Market goal sense (Sol, F_0.4.0 P3): on a totals/BTTS leg a goal's money
            // direction is fixed by the SELECTION, not by which team scores. A goal can only
            // stage on a beat whose direction matches that sense (an improving Under beat can
            // never show a committing goal), it goes to whichever side still has baked goals
            // remaining, and the ML prob↔lead reconciliation below is meaningless here — the
            // live prob tracks the TOTAL against the line, not a lead — so market legs skip it.
            if (_goalSense != 0)
            {
                if (!typeGoal) return null;
                bool goalHelps = _goalSense > 0;
                if (up != goalHelps) return null;
                int remPicked = _targetPicked - Picked;
                int remOpponent = _targetOpponent - Opponent;
                if (remPicked <= 0 && remOpponent <= 0)
                    return new StagedGoal(goalHelps, scoredByPicked: true, commits: false, amount: 1);
                bool scoredByPicked = remPicked >= remOpponent;
                return new StagedGoal(goalHelps, scoredByPicked, commits: true, amount: 1);
            }

            if (typeGoal)
            {
                // Type goals keep the original direction rule (ties up — EventText's law);
                // their |delta| ≥ 0.07 means they are never actually flat.
                // SpendableTargetPicked, not _targetPicked (T17): on a scorer leg the last
                // backed-side goal is reserved for the reveal, so a beat that would spend it
                // stages CHALKED-OFF instead. The beat still plays; only the commit is withheld.
                if (_hasEndpoint && ((up && Picked >= SpendableTargetPicked) || (!up && Opponent >= _targetOpponent)))
                    return new StagedGoal(forPicked: up, commits: false);
                int typeLeadAfter = up ? Picked + 1 - Opponent : Opponent + 1 - Picked;
                return new StagedGoal(forPicked: up, commits: typeLeadAfter <= MaxLiveLead);
            }

            if (_suppressBandReconcile) return null;

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
            if (_hasEndpoint && ((forPicked && Picked >= SpendableTargetPicked)
                || (!forPicked && Opponent >= _targetOpponent)))
                return new StagedGoal(forPicked, commits: false);
            int leadAfter = forPicked ? Picked + 1 - Opponent : Opponent + 1 - Picked;
            return new StagedGoal(forPicked, commits: leadAfter <= MaxLiveLead);
        }

        /// <summary>The ONLY score mutator — called when a staged goal's playback completes.
        /// A chalked-off goal (Commits false) completes without moving anything.</summary>
        public void CompleteGoal(StagedGoal goal)
        {
            if (!goal.Commits) return;
            int amount = Math.Max(0, goal.Amount);
            if (goal.ScoredByPicked)
            {
                // Clamped to the SPENDABLE ceiling (T17), not the raw endpoint. This is the only
                // score mutator, so enforcing the reserve here holds it for every caller — a beat
                // routed through StageBeatGoal, and equally a goal handed straight to this method.
                int applied = _hasEndpoint ? Math.Min(amount, Math.Max(0, SpendableTargetPicked - Picked)) : amount;
                Picked += applied;
                CommittedGoals += applied;
            }
            else
            {
                int applied = _hasEndpoint ? Math.Min(amount, Math.Max(0, _targetOpponent - Opponent)) : amount;
                Opponent += applied;
                CommittedGoals += applied;
            }
        }

        /// <summary>The stoppage-time staging order for a LegFinal, from the FINAL ticket-local
        /// grade (single presentation authority — a suspended scene's ending is never chosen
        /// from WinProbAfter). Pure planning: nothing commits until playback completes.</summary>
        public FinalPlan PlanFinal(LegGrade grade)
        {
            // T17: the final sequence is what the reserve was being held FOR, so release it here.
            // Both production callers (TvSweatScreen's two LegFinal branches) call this and then
            // BindAnytimeScorer immediately, with playback next — never speculatively — so the
            // release cannot hand a beat the goal it was denied. Idempotent, and it restores the
            // exact-convergence guarantee below: Picked can be at most one short of its endpoint,
            // and this method emits that remainder.
            _reserveReleased = true;
            var goals = new System.Collections.Generic.List<StagedGoal>(3);
            if (_hasEndpoint)
            {
                // The locked stat line is the endpoint authority. The normal-time clamp keeps
                // the story legible; the final sequence is allowed to reveal the remaining
                // baked goals so the score converges exactly, even for a real blowout.
                if (Picked < _targetPicked)
                    goals.Add(new StagedGoal(true, true, _targetPicked - Picked));
                if (Opponent < _targetOpponent)
                    goals.Add(new StagedGoal(false, true, _targetOpponent - Opponent));
                return new FinalPlan(grade, goals.ToArray());
            }
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

        /// <summary>TVS-H03: binds an anytime-scorer leg's backed player onto the one committing
        /// goal in <paramref name="plan"/> that represents that player's own side — at PLAN time,
        /// before any playback, so the reveal copy (<c>TvSweatScreen.ScorerFor</c>) and the
        /// stage's actor routing (<c>TheaterStage.EnterStep</c>) can only ever read the same
        /// identity from the same field. No RNG, no wall clock, no frame timing: the actor is
        /// read straight from the leg's own <c>Selection.PlayerIndex</c>, and which plan entry it
        /// binds to falls out of <see cref="StagedGoal.ScoredByPicked"/> — deterministic from the
        /// leg alone, so two or more legs live on the same match (PRD §8.2A) each bind from their
        /// own leg and plan, never a shared "current leg" notion.
        ///
        /// A Lost (or Voided) leg binds nothing — <see cref="StagedGoal.HasBoundScorer"/> stays
        /// false on every goal, so no identity can ever surface for it (PRD §4.1: no scorer
        /// identity on a losing pick).
        ///
        /// <para>This method still only ever attaches an identity to a goal that is actually about
        /// to play, and still never invents one to force a reveal — that would move the causal
        /// reveal point. It is unchanged by T17. What changed is upstream: the ledger now RESERVES
        /// the backed side's last baked goal (see <c>_reservePicked</c>) so ordinary beats can no
        /// longer spend the very goal this loop needs. A Won anytime-scorer leg whose stat line
        /// bakes at least one backed-side goal therefore always reaches here with a committing
        /// <c>ScoredByPicked</c> entry to bind, and the starvation case that produced the
        /// scorer-reveal gap is unreachable rather than merely unlikely.</para></summary>
        public static FinalPlan BindAnytimeScorer(FinalPlan plan, Leg leg)
        {
            if (leg == null || leg.Selection.Kind != MarketKind.AnytimeScorer || plan.Grade != LegGrade.Won)
                return plan;
            if (plan.Goals == null || plan.Goals.Length == 0) return plan;

            bool backedIsHome = leg.Matchup.PlayerSide(leg.Selection.PlayerIndex) == Side.Home;
            int rosterIndex = backedIsHome
                ? leg.Selection.PlayerIndex - leg.Matchup.Away.Players.Count
                : leg.Selection.PlayerIndex;

            StagedGoal[] goals = plan.Goals;
            for (int i = 0; i < goals.Length; i++)
            {
                // ScoredByPicked is this leg's own presentation anchor (fixed above, in
                // ConfigureEndpoint(Leg), to agree with SweatFlavor.PickedHomeForPresentation) —
                // true means "the backed player's own side", never a literal home/away read.
                if (!goals[i].Commits || !goals[i].ScoredByPicked) continue;
                goals[i] = goals[i].WithBoundScorer(backedIsHome, rosterIndex);
                return new FinalPlan(plan.Grade, goals);
            }
            return plan;
        }

        /// <summary>New leg, new match: the scoreline resets.</summary>
        public void ResetForLeg()
        {
            Picked = 0;
            Opponent = 0;
            CommittedGoals = 0;
            // The reserve itself is leg CONFIGURATION and survives; only its spent/released state
            // is playback state, so a re-run of the same leg re-arms the reveal.
            _reserveReleased = false;
        }
    }

    /// <summary>
    /// A deterministic, presentation-only count ledger for corners or cards. It plans the
    /// home/away batched deltas before playback starts, then mutates only at a scene payoff.
    /// Each plan is monotone, every delta is non-negative, and the plan sums exactly to the
    /// locked stat-line endpoint.
    /// </summary>
    public sealed class CountLedger
    {
        public readonly struct StagedCount
        {
            public readonly int HomeDelta;
            public readonly int AwayDelta;
            /// <summary>Which TEAM the engine actually committed this batch to (TVS-S01 fix,
            /// PRD §7.6): true when the beneficiary is the home side. Derived ONLY from
            /// <see cref="HomeDelta"/>/<see cref="AwayDelta"/> — the staged fact — never from
            /// the bettor's Over/Under pick. A batch that credits both sides equally in the
            /// same beat has no factual winner; the tie-break is a deterministic presentation
            /// choice keyed on the beat index (PRD §4.3's "event step" key component), never
            /// RNG or wall clock.</summary>
            public readonly bool BeneficiaryIsHome;
            public int TotalDelta => HomeDelta + AwayDelta;

            public StagedCount(int homeDelta, int awayDelta, int beatIndex)
            {
                HomeDelta = homeDelta;
                AwayDelta = awayDelta;
                BeneficiaryIsHome = homeDelta != awayDelta ? homeDelta > awayDelta : (beatIndex % 2 == 0);
            }
        }

        public readonly struct FinalPlan
        {
            public readonly StagedCount[] Counts;

            public FinalPlan(StagedCount[] counts) => Counts = counts ?? Array.Empty<StagedCount>();
        }

        private readonly List<int> _homeDeltas = new List<int>();
        private readonly List<int> _awayDeltas = new List<int>();
        private int _nextBeat;
        private int _targetHome;
        private int _targetAway;

        public int TargetHome => _targetHome;
        public int TargetAway => _targetAway;
        public int TargetTotal => _targetHome + _targetAway;
        public int Home { get; private set; }
        public int Away { get; private set; }
        public int Total => Home + Away;
        public int ConsumedBeats => _nextBeat;
        public int MaxPerBeatDelta
        {
            get
            {
                int max = 0;
                for (int i = 0; i < _homeDeltas.Count; i++)
                    max = Math.Max(max, _homeDeltas[i] + _awayDeltas[i]);
                return max;
            }
        }
        public IReadOnlyList<int> HomeDeltas => _homeDeltas;
        public IReadOnlyList<int> AwayDeltas => _awayDeltas;
        public IReadOnlyList<int> PlannedDeltas
        {
            get
            {
                var total = new int[_homeDeltas.Count];
                for (int i = 0; i < total.Length; i++) total[i] = _homeDeltas[i] + _awayDeltas[i];
                return total;
            }
        }

        public CountLedger() { }

        public CountLedger(int targetHome, int targetAway, int beatCount)
        {
            ConfigureEndpoint(targetHome, targetAway, beatCount);
        }

        public void ConfigureEndpoint(MatchStatLine statLine, MarketKind kind, int beatCount)
        {
            if (statLine == null) throw new ArgumentNullException(nameof(statLine));
            if (kind == MarketKind.TotalCorners)
                ConfigureEndpoint(statLine.HomeCorners, statLine.AwayCorners, beatCount);
            else if (kind == MarketKind.TotalCards)
                ConfigureEndpoint(statLine.HomeCards, statLine.AwayCards, beatCount);
            else
                throw new ArgumentException("CountLedger supports corners or cards only", nameof(kind));
        }

        public void ConfigureEndpoint(int targetHome, int targetAway, int beatCount)
        {
            if (targetHome < 0 || targetAway < 0) throw new ArgumentOutOfRangeException(nameof(targetHome));
            _targetHome = targetHome;
            _targetAway = targetAway;
            PlanForBeats(beatCount);
            ResetForLeg();
        }

        /// <summary>Creates the complete batched schedule. Remainders are spread across the
        /// earliest beats so partial sums are monotone and the final beat remains bounded.</summary>
        public void PlanForBeats(int beatCount)
        {
            beatCount = Math.Max(1, beatCount);
            _homeDeltas.Clear();
            _awayDeltas.Clear();
            Distribute(_targetHome, beatCount, _homeDeltas);
            Distribute(_targetAway, beatCount, _awayDeltas);
            _nextBeat = 0;
        }

        /// <summary>Stages the next pre-planned batch. No bet input: the schedule (and
        /// therefore whether/how much happens) comes entirely from <see cref="PlanForBeats"/>
        /// against the locked stat line; only the returned <see cref="StagedCount"/>'s own
        /// beneficiary computation is index-derived (TVS-S01 fix, PRD §7.6).</summary>
        public StagedCount StageBeat()
        {
            if (_nextBeat >= _homeDeltas.Count)
                return new StagedCount(0, 0, _nextBeat);
            int index = _nextBeat;
            _nextBeat++;
            return new StagedCount(_homeDeltas[index], _awayDeltas[index], index);
        }

        public void CompleteCount(StagedCount count)
        {
            Home = Math.Min(_targetHome, Home + Math.Max(0, count.HomeDelta));
            Away = Math.Min(_targetAway, Away + Math.Max(0, count.AwayDelta));
        }

        /// <summary>Returns the remaining planned beats. In normal playback this is exactly the
        /// final scheduled batch; returning all remaining entries also keeps forced-test usage
        /// bounded when a caller jumps directly to the whistle.</summary>
        public FinalPlan PlanFinal()
        {
            var remaining = new List<StagedCount>();
            while (_nextBeat < _homeDeltas.Count)
            {
                // Zero batches never earn a scene — nothing happened (Sol, F_0.4.0 P3 r2).
                if (_homeDeltas[_nextBeat] + _awayDeltas[_nextBeat] > 0)
                    remaining.Add(new StagedCount(_homeDeltas[_nextBeat], _awayDeltas[_nextBeat], _nextBeat));
                _nextBeat++;
            }
            if (remaining.Count == 0 && (Home < _targetHome || Away < _targetAway))
            {
                // A forced jump to the whistle must still obey the largest pre-planned
                // per-beat batch. Never hide an arbitrary reconciliation dump in LegFinal.
                int max = Math.Max(1, MaxPerBeatDelta);
                int home = Math.Max(0, _targetHome - Home);
                int away = Math.Max(0, _targetAway - Away);
                int forcedIndex = _nextBeat; // continues the same deterministic tie-break sequence
                while (home > 0 || away > 0)
                {
                    int homeBatch = Math.Min(home, max);
                    int awayBatch = Math.Min(away, max - homeBatch);
                    if (homeBatch == 0 && awayBatch == 0) awayBatch = Math.Min(away, max);
                    remaining.Add(new StagedCount(homeBatch, awayBatch, forcedIndex));
                    forcedIndex++;
                    home -= homeBatch;
                    away -= awayBatch;
                }
            }
            return new FinalPlan(remaining.ToArray());
        }

        public void ResetForLeg()
        {
            Home = 0;
            Away = 0;
            _nextBeat = 0;
        }

        private static void Distribute(int target, int beats, List<int> output)
        {
            int quotient = target / beats;
            int remainder = target % beats;
            for (int i = 0; i < beats; i++) output.Add(quotient + (i < remainder ? 1 : 0));
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
