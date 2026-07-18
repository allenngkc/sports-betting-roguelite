using NUnit.Framework;
using SBR.Engine;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// The synthesized score ledger's laws (F_0.2.0 M-T3): causal attribution, the live-lead
    /// clamp (one-goal-game stories), playback-completion commit timing, the four LegFinal
    /// resolution paths, the full entry-lead matrix, and the goal-playback invariant — plus
    /// the property test over real generator paths (|live lead| ≤ 1 ⇒ corrections ≤ 2).
    /// </summary>
    public class ScoreLedgerTests
    {
        // ---------------------------------------------------------------- attribution + clamp

        [Test]
        public void Momentum_and_near_miss_beats_stage_no_goal()
        {
            var ledger = new ScoreLedger();
            Assert.IsNull(ledger.StageBeatGoal(DramaEventType.Momentum, up: true));
            Assert.IsNull(ledger.StageBeatGoal(DramaEventType.Momentum, up: false));
            Assert.IsNull(ledger.StageBeatGoal(DramaEventType.LegFinal, up: true));
        }

        [Test]
        public void Score_and_big_play_attribute_by_direction()
        {
            var ledger = new ScoreLedger();
            ScoreLedger.StagedGoal upGoal = ledger.StageBeatGoal(DramaEventType.Score, up: true).Value;
            Assert.IsTrue(upGoal.ForPicked);
            Assert.IsTrue(upGoal.Commits);

            ScoreLedger.StagedGoal downGoal = ledger.StageBeatGoal(DramaEventType.BigPlay, up: false).Value;
            Assert.IsFalse(downGoal.ForPicked);
            Assert.IsTrue(downGoal.Commits);
        }

        [Test]
        public void Commit_timing_is_playback_completion_never_staging()
        {
            var ledger = new ScoreLedger();
            ScoreLedger.StagedGoal g = ledger.StageBeatGoal(DramaEventType.Score, up: true).Value;
            Assert.AreEqual(0, ledger.Picked, "staging must not move the score");
            ledger.CompleteGoal(g);
            Assert.AreEqual(1, ledger.Picked, "completion is the only score mutator");
        }

        [Test]
        public void Live_lead_clamp_chalks_the_blowout_goal_for_score_and_big_play()
        {
            foreach (DramaEventType type in new[] { DramaEventType.Score, DramaEventType.BigPlay })
            {
                var ledger = new ScoreLedger();
                ledger.CompleteGoal(ledger.StageBeatGoal(type, up: true).Value); // 1-0

                ScoreLedger.StagedGoal second = ledger.StageBeatGoal(type, up: true).Value;
                Assert.IsFalse(second.Commits, $"{type}: a 2-0 goal must stage chalked-off");
                ledger.CompleteGoal(second);
                Assert.AreEqual(1, ledger.Picked, $"{type}: the chalked goal must not score");

                // The other direction is open: the opponent can still equalize.
                Assert.IsTrue(ledger.StageBeatGoal(type, up: false).Value.Commits);
            }
        }

        [Test]
        public void Goal_playback_invariant_increments_map_1_to_1()
        {
            var ledger = new ScoreLedger();
            int completedCommits = 0;
            // A busy leg: up, up (chalked), down, down (chalked after equalizer? no — swings).
            foreach (bool up in new[] { true, true, false, false, true })
            {
                ScoreLedger.StagedGoal g = ledger.StageBeatGoal(DramaEventType.Score, up).Value;
                ledger.CompleteGoal(g);
                if (g.Commits) completedCommits++;
                Assert.AreEqual(completedCommits, ledger.CommittedGoals,
                    "every increment must correspond to exactly one completed committed goal");
                Assert.LessOrEqual(System.Math.Abs(ledger.Picked - ledger.Opponent), 1);
            }
        }

        // ---------------------------------------------------------------- the four final paths

        [Test]
        public void Plain_final_won_from_behind_stages_corrections_until_strictly_ahead()
        {
            var ledger = new ScoreLedger();
            ledger.CompleteGoal(ledger.StageBeatGoal(DramaEventType.Score, false).Value); // 0-1

            ScoreLedger.FinalPlan plan = ledger.PlanFinal(LegGrade.Won);
            Assert.AreEqual(2, plan.Goals.Length, "from -1, Won needs exactly 2 stoppage-time goals");
            foreach (ScoreLedger.StagedGoal g in plan.Goals)
            {
                Assert.IsTrue(g.ForPicked);
                Assert.IsTrue(g.Commits);
                ledger.CompleteGoal(g);
            }
            Assert.Greater(ledger.Picked, ledger.Opponent, "won it at the death — strictly ahead");
        }

        [Test]
        public void Void_final_freezes_the_ledger()
        {
            var ledger = new ScoreLedger();
            ledger.CompleteGoal(ledger.StageBeatGoal(DramaEventType.Score, true).Value); // 1-0

            ScoreLedger.FinalPlan plan = ledger.PlanFinal(LegGrade.Voided);
            Assert.AreEqual(0, plan.Goals.Length, "a voided match needs no coherent final score");
            Assert.AreEqual(1, ledger.Picked);
            Assert.AreEqual(0, ledger.Opponent);
        }

        [Test]
        public void Final_resolution_entry_lead_matrix()
        {
            // (entry lead, grade) → (staged goals, killing shot commits?) — the plan's matrix.
            var cases = new (int lead, LegGrade grade, int staged, bool killingCommits)[]
            {
                (-1, LegGrade.Won, 2, true),   // flipped-to-Won from behind
                (0, LegGrade.Won, 1, true),
                (1, LegGrade.Won, 0, true),    // already ahead: whistle only
                (-1, LegGrade.Lost, 1, false), // already satisfies Lost: chalked at the death
                (0, LegGrade.Lost, 1, true),
                (1, LegGrade.Lost, 2, true),   // killing shot ties it, one more wins it for them
            };
            foreach ((int lead, LegGrade grade, int staged, bool killingCommits) in cases)
            {
                ScoreLedger ledger = LedgerAtLead(lead);
                ScoreLedger.FinalPlan plan = ledger.PlanFinal(grade);
                Assert.AreEqual(staged, plan.Goals.Length, $"lead {lead}, {grade}: staged count");
                Assert.LessOrEqual(plan.Goals.Length, 2, "the clamp bounds corrections at 2");
                if (grade == LegGrade.Lost)
                    Assert.AreEqual(killingCommits, plan.Goals[0].Commits,
                        $"lead {lead}: the killing shot's commit ruling");

                foreach (ScoreLedger.StagedGoal g in plan.Goals) ledger.CompleteGoal(g);
                if (grade == LegGrade.Won)
                    Assert.Greater(ledger.Picked, ledger.Opponent, $"lead {lead}: Won must end strictly ahead");
                else
                    Assert.Greater(ledger.Opponent, ledger.Picked, $"lead {lead}: Lost must end strictly behind");
            }
        }

        private static ScoreLedger LedgerAtLead(int lead)
        {
            var ledger = new ScoreLedger();
            for (int i = 0; i < System.Math.Abs(lead); i++)
                ledger.CompleteGoal(ledger.StageBeatGoal(DramaEventType.Score, up: lead > 0).Value);
            Assert.AreEqual(lead, ledger.Picked - ledger.Opponent, "fixture lead");
            return ledger;
        }

        // ---------------------------------------------------------------- the property test

        [Test]
        public void Generated_paths_never_break_the_clamp_and_corrections_stay_bounded()
        {
            var pacer = new SweatPacer();
            var choreo = new TheaterChoreographer(pacer);

            for (int i = 0; i < 150; i++)
            {
                var run = new Run($"CLAMP-{i}", new RunConfig());
                Ticket ticket = run.PlaceTicket(
                    new[] { new Pick(0, Side.Home), new Pick(1, Side.Away), new Pick(2, Side.Home) }, 50);
                run.LockRound();

                var cfg = new DramaConfig();
                var paths = DramaGenerator.BuildTicketPaths(
                    ticket, new Pcg32((ulong)(41000 + i), 54), cfg, round: cfg.DensityRampRounds);

                var model = new SweatPresentationModel();
                for (int legIx = 0; legIx < paths.Count; legIx++)
                {
                    Leg leg = ticket.Legs[legIx];
                    var ledger = new ScoreLedger();
                    foreach (DramaEvent evt in paths[legIx])
                    {
                        bool up = model.RecordBeat(evt, leg);
                        if (evt.Type == DramaEventType.LegFinal)
                        {
                            LegGrade grade = leg.GradesWon ? LegGrade.Won : LegGrade.Lost;
                            ScoreLedger.FinalPlan plan = ledger.PlanFinal(grade);
                            Assert.LessOrEqual(plan.Goals.Length, 2,
                                "clamp consequence: a correction needs at most 2 staged goals");
                            foreach (ScoreLedger.StagedGoal g in plan.Goals) ledger.CompleteGoal(g);
                        }
                        else
                        {
                            SceneSpec spec = choreo.ResolveBeat(evt, up, ledger);
                            if (spec.Goal.HasValue) ledger.CompleteGoal(spec.Goal.Value);
                        }
                        Assert.LessOrEqual(System.Math.Abs(ledger.Picked - ledger.Opponent), 1,
                            $"live lead escaped the clamp (seed CLAMP-{i}, leg {legIx})");
                    }
                }
            }
        }
    }
}
