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
            Assert.IsNull(ledger.StageBeatGoal(DramaEventType.Momentum, up: true, 0.05, 0.5));
            Assert.IsNull(ledger.StageBeatGoal(DramaEventType.Momentum, up: false, -0.05, 0.5));
            Assert.IsNull(ledger.StageBeatGoal(DramaEventType.LegFinal, up: true, 0.05, 0.5));
        }

        [Test]
        public void Score_and_big_play_attribute_by_direction()
        {
            var ledger = new ScoreLedger();
            ScoreLedger.StagedGoal upGoal = ledger.StageBeatGoal(DramaEventType.Score, up: true, 0.05, 0.5).Value;
            Assert.IsTrue(upGoal.ForPicked);
            Assert.IsTrue(upGoal.Commits);

            ScoreLedger.StagedGoal downGoal = ledger.StageBeatGoal(DramaEventType.BigPlay, up: false, -0.05, 0.5).Value;
            Assert.IsFalse(downGoal.ForPicked);
            Assert.IsTrue(downGoal.Commits);
        }

        [Test]
        public void Commit_timing_is_playback_completion_never_staging()
        {
            var ledger = new ScoreLedger();
            ScoreLedger.StagedGoal g = ledger.StageBeatGoal(DramaEventType.Score, up: true, 0.05, 0.5).Value;
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
                ledger.CompleteGoal(ledger.StageBeatGoal(type, up: true, 0.05, 0.5).Value); // 1-0

                ScoreLedger.StagedGoal second = ledger.StageBeatGoal(type, up: true, 0.05, 0.5).Value;
                Assert.IsFalse(second.Commits, $"{type}: a 2-0 goal must stage chalked-off");
                ledger.CompleteGoal(second);
                Assert.AreEqual(1, ledger.Picked, $"{type}: the chalked goal must not score");

                // The other direction is open: the opponent can still equalize.
                Assert.IsTrue(ledger.StageBeatGoal(type, up: false, -0.05, 0.5).Value.Commits);
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
                ScoreLedger.StagedGoal g = ledger.StageBeatGoal(DramaEventType.Score, up, -0.05, 0.5).Value;
                ledger.CompleteGoal(g);
                if (g.Commits) completedCommits++;
                Assert.AreEqual(completedCommits, ledger.CommittedGoals,
                    "every increment must correspond to exactly one completed committed goal");
                Assert.LessOrEqual(System.Math.Abs(ledger.Picked - ledger.Opponent), 1);
            }
        }

        // ------------------------------------------------------- prob reconciliation (#14)

        [Test]
        public void Ninety_percent_at_nil_nil_reconciles_the_board_upward()
        {
            // Playtest #14 scenario 2: the bar reads 90% while the board sits 0-0 — fake.
            // The next up beat must stage the picked goal even though it is only Momentum.
            var ledger = new ScoreLedger();
            ScoreLedger.StagedGoal? goal = ledger.StageBeatGoal(DramaEventType.Momentum, up: true, 0.05, 0.90);
            Assert.IsTrue(goal.HasValue, "sustained high prob at 0-0 demands the board catch up");
            Assert.IsTrue(goal.Value.ForPicked);
            Assert.IsTrue(goal.Value.Commits);
            ledger.CompleteGoal(goal.Value);

            // Once the board agrees (1-0 at 90%), momentum beats stay scoreless again.
            Assert.IsNull(ledger.StageBeatGoal(DramaEventType.Momentum, up: true, 0.05, 0.90),
                "a reconciled board stops producing goals");
        }

        [Test]
        public void Leading_at_twenty_five_percent_reconciles_the_board_downward()
        {
            // Playtest #14 scenario 1: picked leads 1-0 but the bar has collapsed to 25% —
            // consecutive down beats stage the equalizer, then the go-ahead against us.
            var ledger = new ScoreLedger();
            ledger.CompleteGoal(ledger.StageBeatGoal(DramaEventType.Score, up: true, 0.05, 0.5).Value); // 1-0

            ScoreLedger.StagedGoal? equalizer = ledger.StageBeatGoal(DramaEventType.Momentum, up: false, -0.05, 0.25);
            Assert.IsTrue(equalizer.HasValue, "a collapsed bar demands the board answer");
            Assert.IsFalse(equalizer.Value.ForPicked);
            ledger.CompleteGoal(equalizer.Value); // 1-1

            ScoreLedger.StagedGoal? goAhead = ledger.StageBeatGoal(DramaEventType.Momentum, up: false, -0.05, 0.25);
            Assert.IsTrue(goAhead.HasValue, "the board converges to the bar over two beats");
            ledger.CompleteGoal(goAhead.Value); // 1-2
            Assert.AreEqual(-1, ledger.Picked - ledger.Opponent, "board now agrees with 25%");

            Assert.IsNull(ledger.StageBeatGoal(DramaEventType.Momentum, up: false, -0.05, 0.25),
                "convergence stops at the implied lead — the clamp's one-goal story holds");
        }

        [Test]
        public void Mid_band_probabilities_never_drag_a_natural_lead_back()
        {
            // A 1-0 lead at 55% is a perfectly natural one-goal game — reconciliation only
            // acts OUTSIDE the bands, and only toward a nonzero implied lead.
            var ledger = new ScoreLedger();
            ledger.CompleteGoal(ledger.StageBeatGoal(DramaEventType.Score, up: true, 0.05, 0.5).Value); // 1-0
            Assert.IsNull(ledger.StageBeatGoal(DramaEventType.Momentum, up: false, -0.05, 0.55));
            Assert.IsNull(ledger.StageBeatGoal(DramaEventType.Momentum, up: false, -0.05, 0.31));
        }

        [Test]
        public void Reconciliation_only_moves_with_the_beat_direction()
        {
            // 90% + a DOWN beat: the board is behind the bar but the moment is against us —
            // staging a picked goal on a down beat would read as nonsense. Wait for an up beat.
            var ledger = new ScoreLedger();
            Assert.IsNull(ledger.StageBeatGoal(DramaEventType.Momentum, up: false, -0.05, 0.90));
            Assert.IsNull(ledger.StageBeatGoal(DramaEventType.Momentum, up: true, 0.05, 0.10));
        }

        [Test]
        public void Flat_beats_support_either_bands_reconciliation()
        {
            // Sol (M-T4.1): clamp-generated flat beats (delta 0 — paths riding the
            // generator's 0.03/0.97 clamp) are real inputs. A flat beat at the ceiling
            // supports the picked goal; a flat beat at the FLOOR supports the opponent
            // goal, even though the tie-break bool calls it "up". Sign-compatibility gates
            // reconciliation; the goal's side comes from the BAND, never the tie-break.
            var ledger = new ScoreLedger();
            ScoreLedger.StagedGoal? ceiling = ledger.StageBeatGoal(DramaEventType.Momentum, up: true, 0.0, 0.97);
            Assert.IsTrue(ceiling.HasValue && ceiling.Value.ForPicked, "flat at the ceiling scores for picked");

            var ledger2 = new ScoreLedger();
            ScoreLedger.StagedGoal? floor = ledger2.StageBeatGoal(DramaEventType.Momentum, up: true, 0.0, 0.03);
            Assert.IsTrue(floor.HasValue, "flat at the floor reconciles too - the asymmetry is dead");
            Assert.IsFalse(floor.Value.ForPicked, "the band, not the tie-break, picks the scorer");
        }

        // ---------------------------------------------------------------- the four final paths

        [Test]
        public void Plain_final_won_from_behind_stages_corrections_until_strictly_ahead()
        {
            var ledger = new ScoreLedger();
            ledger.CompleteGoal(ledger.StageBeatGoal(DramaEventType.Score, false, -0.05, 0.5).Value); // 0-1

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
            ledger.CompleteGoal(ledger.StageBeatGoal(DramaEventType.Score, true, 0.05, 0.5).Value); // 1-0

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
                ledger.CompleteGoal(ledger.StageBeatGoal(DramaEventType.Score, up: lead > 0, -0.05, 0.5).Value);
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
                        double delta = model.Beats[model.Beats.Count - 1].Delta;
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
                            SceneSpec spec = choreo.ResolveBeat(evt, up, delta, ledger);
                            if (spec.Goal.HasValue) ledger.CompleteGoal(spec.Goal.Value);
                        }
                        Assert.LessOrEqual(System.Math.Abs(ledger.Picked - ledger.Opponent), 1,
                            $"live lead escaped the clamp (seed CLAMP-{i}, leg {legIx})");
                    }
                }
            }
        }

        [Test]
        public void Count_schedule_sums_exactly_and_keeps_each_batch_bounded()
        {
            var ledger = new CountLedger(7, 3, 4);
            int expectedBound = (7 + 4 - 1) / 4 + (3 + 4 - 1) / 4;
            int sum = 0;
            int previous = 0;
            for (int i = 0; i < ledger.PlannedDeltas.Count; i++)
            {
                int delta = ledger.PlannedDeltas[i];
                Assert.GreaterOrEqual(delta, 0);
                Assert.LessOrEqual(delta, expectedBound);
                sum += delta;
                Assert.GreaterOrEqual(sum, previous, "partial sums must be monotone");
                previous = sum;
            }
            Assert.AreEqual(10, sum);
            Assert.Greater(ledger.MaxPerBeatDelta, 1, "count scenes may reveal batched deltas");
            Assert.AreEqual(7, ledger.HomeDeltas[0] + ledger.HomeDeltas[1]
                + ledger.HomeDeltas[2] + ledger.HomeDeltas[3]);
            Assert.AreEqual(3, ledger.AwayDeltas[0] + ledger.AwayDeltas[1]
                + ledger.AwayDeltas[2] + ledger.AwayDeltas[3]);
        }

        [Test]
        public void Count_final_plan_converges_to_the_baked_endpoint()
        {
            var ledger = new CountLedger(7, 3, 4);
            ledger.CompleteCount(ledger.StageBeat());
            ledger.CompleteCount(ledger.StageBeat());
            foreach (CountLedger.StagedCount count in ledger.PlanFinal().Counts)
                ledger.CompleteCount(count);

            Assert.AreEqual(7, ledger.Home);
            Assert.AreEqual(3, ledger.Away);
            Assert.AreEqual(10, ledger.Total);
        }

        [Test]
        public void Goal_ledger_converges_to_the_locked_scoreline_at_the_whistle()
        {
            var ledger = new ScoreLedger();
            var statLine = new MatchStatLine(4, 1, 7, 3, 2, 4);
            ledger.ConfigureEndpoint(statLine, pickedHome: true);

            foreach (ScoreLedger.StagedGoal goal in ledger.PlanFinal(LegGrade.Won).Goals)
                ledger.CompleteGoal(goal);

            Assert.AreEqual(4, ledger.Picked);
            Assert.AreEqual(1, ledger.Opponent);
            Assert.AreEqual(5, ledger.CommittedGoals);
        }

        [Test]
        public void Count_scene_direction_is_the_selections_sense_never_the_beat_direction()
        {
            // Sol, F_0.4.0 P3 r2: an increment's hope/dread is fixed by the SELECTION — a
            // corner always bites an Under bettor, even on a beat whose price drifted their
            // way (the count arriving slower than the line needs). Beat direction must not
            // leak into the count scene's mood.
            //
            // Restored unmodified (reviewer correction, TVS-S01 follow-up) after a prior
            // revision of the TVS-S01 fix incorrectly retired this test instead of fixing the
            // regression it had caught: CornerFor/CornerAgainst is the bettor's MOOD, not team
            // routing — routing is the separate CountBeneficiaryIsHome/BeneficiaryIsHome fact
            // (see Corner_mood_follows_the_bet_and_routing_follows_the_team_independently below).
            var run = new Run("COUNT-DIRECTION", new RunConfig());
            Ticket ticket = run.PlaceTicket(new[]
            {
                new Pick(0, MarketSelection.TotalCorners(9.5, false)),
            }, 10);
            run.LockRound();
            Leg leg = ticket.Legs[0];
            var counts = new CountLedger();
            counts.ConfigureEndpoint(leg.Matchup.StatLine, MarketKind.TotalCorners, 2);
            var choreo = new TheaterChoreographer(new SweatPacer());

            SceneSpec towardUnder = choreo.ResolveBeat(
                new DramaEvent(0, 1, 4, DramaEventType.Momentum, 0.25, TensionTag.Calm),
                up: true, delta: 0.05, new ScoreLedger(), leg, counts);
            SceneSpec awayFromUnder = choreo.ResolveBeat(
                new DramaEvent(0, 2, 4, DramaEventType.Momentum, 0.15, TensionTag.Calm),
                up: false, delta: -0.10, new ScoreLedger(), leg, counts);

            foreach (SceneSpec spec in new[] { towardUnder, awayFromUnder })
            {
                // Two beats over a >= 1-corner endpoint: every staged count scene on an
                // Under leg is dread; a zero batch may fall through to ordinary play instead.
                if (spec.Count.HasValue && spec.Count.Value.TotalDelta > 0)
                {
                    Assert.AreEqual(SceneTemplate.CornerAgainst, spec.Template);
                    Assert.IsFalse(spec.ForPicked);
                }
                else
                {
                    Assert.AreNotEqual(SceneTemplate.CornerFor, spec.Template);
                    Assert.AreNotEqual(SceneTemplate.CornerAgainst, spec.Template);
                }
            }

            // The Over side of the same coin: increments are hope, whatever the beat did.
            var overRun = new Run("COUNT-DIRECTION", new RunConfig());
            Ticket overTicket = overRun.PlaceTicket(new[]
            {
                new Pick(0, MarketSelection.TotalCorners(9.5, true)),
            }, 10);
            overRun.LockRound();
            Leg overLeg = overTicket.Legs[0];
            var overCounts = new CountLedger();
            overCounts.ConfigureEndpoint(overLeg.Matchup.StatLine, MarketKind.TotalCorners, 2);

            SceneSpec downBeatOver = choreo.ResolveBeat(
                new DramaEvent(0, 1, 4, DramaEventType.Momentum, 0.35, TensionTag.Calm),
                up: false, delta: -0.05, new ScoreLedger(), overLeg, overCounts);
            if (downBeatOver.Count.HasValue && downBeatOver.Count.Value.TotalDelta > 0)
            {
                Assert.AreEqual(SceneTemplate.CornerFor, downBeatOver.Template);
                Assert.IsTrue(downBeatOver.ForPicked);
            }
        }

        // ---------------------------------------------------------------- TVS-S01 regression
        //
        // Three separable concepts, never conflated (PRD §7.6, reviewer correction):
        //   1. ROUTING — which team physically wins the corner/commits the foul. The staged
        //      fact, CountLedger.StagedCount.BeneficiaryIsHome / SceneSpec.CountBeneficiaryIsHome,
        //      derived only from HomeDelta/AwayDelta. NEVER the bet.
        //   2. MOOD — whether the event helps or hurts the bettor. The selection's Over/Under
        //      sense; drives CornerFor/CornerAgainst template choice, and rides along on
        //      SceneSpec.ForPicked for Booking's single template. NEVER the team.
        //   3. ForPicked on the goal path — whether the beneficiary is the picked TEAM,
        //      meaningful only for moneyline. Untouched by this fix.
        // The original TVS-S01 defect conflated 1 and 2 by driving routing from the bet
        // (leg.Selection.Choice == MarketChoice.Over). An earlier revision of this fix
        // overcorrected and conflated them the other way, driving the CornerFor/CornerAgainst
        // TEMPLATE from the team instead of the bet — silently destroying the mood signal that
        // Count_scene_direction_is_the_selections_sense_never_the_beat_direction (below,
        // restored unmodified) exists to protect. Both directions are guarded here now.

        [Test]
        public void StagedCount_beneficiary_comes_from_deltas_never_a_flag_and_ties_break_deterministically()
        {
            // StagedCount no longer even accepts a bet-derived flag — its third constructor
            // argument is a beat index, consulted only to break a genuine tie.
            Assert.IsTrue(new CountLedger.StagedCount(2, 0, beatIndex: 0).BeneficiaryIsHome);
            Assert.IsFalse(new CountLedger.StagedCount(0, 2, beatIndex: 0).BeneficiaryIsHome);
            Assert.IsTrue(new CountLedger.StagedCount(3, 1, beatIndex: 7).BeneficiaryIsHome);
            Assert.IsFalse(new CountLedger.StagedCount(1, 3, beatIndex: 7).BeneficiaryIsHome);

            // A genuine tie (both sides credited equally in the same beat) has no factual
            // winner; the tie-break is deterministic from the beat index (PRD §4.3's "event
            // step" key component), not RNG, and not hardcoded to one side.
            Assert.IsTrue(new CountLedger.StagedCount(1, 1, beatIndex: 0).BeneficiaryIsHome);
            Assert.IsFalse(new CountLedger.StagedCount(1, 1, beatIndex: 1).BeneficiaryIsHome);
            Assert.AreEqual(new CountLedger.StagedCount(1, 1, beatIndex: 4).BeneficiaryIsHome,
                new CountLedger.StagedCount(1, 1, beatIndex: 4).BeneficiaryIsHome,
                "same input must always resolve the same way");
        }

        private static Leg BuildCountLeg(MarketSelection selection, string runId)
        {
            var run = new Run(runId, new RunConfig());
            Ticket ticket = run.PlaceTicket(new[] { new Pick(0, selection) }, 10);
            run.LockRound();
            return ticket.Legs[0];
        }

        [Test]
        public void Corner_credited_home_routes_to_home_regardless_of_over_under_pick()
        {
            // Regression #1 (corrected, reviewer follow-up): a corner the engine credits to the
            // HOME side must ROUTE the move to the home team's dots
            // (BeneficiaryIsHome/CountBeneficiaryIsHome) whether the bettor picked Over or
            // Under — the bet must never change which team physically wins the corner. This is
            // deliberately NOT an assertion on spec.Template: the template legitimately DOES
            // follow the bet (CornerFor for Over, CornerAgainst for Under) — that is the
            // separate MOOD concept, restored by
            // Count_scene_direction_is_the_selections_sense_never_the_beat_direction and pinned
            // together with routing by
            // Corner_mood_follows_the_bet_and_routing_follows_the_team_independently.
            var choreo = new TheaterChoreographer(new SweatPacer());
            var evt = new DramaEvent(0, 1, 4, DramaEventType.Momentum, 0.55, TensionTag.Calm);

            foreach (bool over in new[] { true, false })
            {
                Leg leg = BuildCountLeg(MarketSelection.TotalCorners(8.5, over), $"S01-HOME-{over}");
                var counts = new CountLedger();
                counts.ConfigureEndpoint(targetHome: 2, targetAway: 0, beatCount: 1);

                SceneSpec spec = choreo.ResolveBeat(evt, up: true, delta: 0.05, new ScoreLedger(), leg, counts);

                Assert.IsTrue(spec.Count.HasValue && spec.Count.Value.TotalDelta > 0,
                    "the single scheduled beat must stage the batch");
                Assert.IsTrue(spec.Count.Value.BeneficiaryIsHome, $"over={over}: HomeDelta beats AwayDelta");
                Assert.IsTrue(spec.CountBeneficiaryIsHome.HasValue && spec.CountBeneficiaryIsHome.Value,
                    $"over={over}: a home-credited corner must route to home regardless of the pick");
            }
        }

        [Test]
        public void Corner_credited_away_routes_to_away_regardless_of_over_under_pick()
        {
            // Regression #2 (corrected, reviewer follow-up): same law, away-credited corner —
            // routing only, template is deliberately not asserted here (see #1's comment).
            var choreo = new TheaterChoreographer(new SweatPacer());
            var evt = new DramaEvent(0, 1, 4, DramaEventType.Momentum, 0.45, TensionTag.Calm);

            foreach (bool over in new[] { true, false })
            {
                Leg leg = BuildCountLeg(MarketSelection.TotalCorners(8.5, over), $"S01-AWAY-{over}");
                var counts = new CountLedger();
                counts.ConfigureEndpoint(targetHome: 0, targetAway: 2, beatCount: 1);

                SceneSpec spec = choreo.ResolveBeat(evt, up: false, delta: -0.05, new ScoreLedger(), leg, counts);

                Assert.IsTrue(spec.Count.HasValue && spec.Count.Value.TotalDelta > 0);
                Assert.IsFalse(spec.Count.Value.BeneficiaryIsHome, $"over={over}: AwayDelta beats HomeDelta");
                Assert.IsTrue(spec.CountBeneficiaryIsHome.HasValue);
                Assert.IsFalse(spec.CountBeneficiaryIsHome.Value,
                    $"over={over}: an away-credited corner must route to away regardless of the pick");
            }
        }

        [Test]
        public void Corner_mood_follows_the_bet_and_routing_follows_the_team_independently()
        {
            // The disambiguating test the reviewer asked for, generalized to all four
            // (bet, team) combinations rather than only "Under leg, away wins": that specific
            // combination alone does NOT distinguish this fix from either direction of
            // regression, because Under=false and away=false happen to agree — a template-
            // driven-by-team bug (the prior revision) and a routing-driven-by-bet bug (the
            // original TVS-S01) would BOTH reproduce the same template/routing values for that
            // one case. The disagreeing combinations (Under+home, Over+away) are what actually
            // pin the two concepts apart; this test includes the reviewer's literal example
            // (over=false, homeWins=false) plus its three siblings.
            var choreo = new TheaterChoreographer(new SweatPacer());
            var evt = new DramaEvent(0, 1, 4, DramaEventType.Momentum, 0.5, TensionTag.Calm);

            foreach (bool over in new[] { true, false })
            {
                foreach (bool homeWins in new[] { true, false })
                {
                    Leg leg = BuildCountLeg(MarketSelection.TotalCorners(8.5, over),
                        $"S01-MOOD-ROUTE-{over}-{homeWins}");
                    var counts = new CountLedger();
                    counts.ConfigureEndpoint(
                        targetHome: homeWins ? 2 : 0, targetAway: homeWins ? 0 : 2, beatCount: 1);

                    SceneSpec spec = choreo.ResolveBeat(evt, up: true, delta: 0.05, new ScoreLedger(), leg, counts);

                    Assert.IsTrue(spec.Count.HasValue && spec.Count.Value.TotalDelta > 0,
                        $"over={over}, homeWins={homeWins}: the single scheduled beat must stage the batch");

                    SceneTemplate expectedMood = over ? SceneTemplate.CornerFor : SceneTemplate.CornerAgainst;
                    Assert.AreEqual(expectedMood, spec.Template,
                        $"over={over}, homeWins={homeWins}: mood (template) must follow the bet, never the team");

                    Assert.AreEqual(homeWins, spec.Count.Value.BeneficiaryIsHome,
                        $"over={over}, homeWins={homeWins}: routing must follow the team, never the bet");
                    Assert.IsTrue(spec.CountBeneficiaryIsHome.HasValue);
                    Assert.AreEqual(homeWins, spec.CountBeneficiaryIsHome.Value,
                        $"over={over}, homeWins={homeWins}: routing must follow the team, never the bet");
                }
            }
        }

        [Test]
        public void Booking_beneficiary_is_read_from_the_staged_fact_on_both_over_and_under_legs()
        {
            // Regression #3: bookings use one direction-neutral template (no For/Against
            // split), so the beneficiary rides SceneSpec.CountBeneficiaryIsHome directly —
            // never ForPicked, which is incoherent for a totals market with no picked team.
            var choreo = new TheaterChoreographer(new SweatPacer());
            var evt = new DramaEvent(0, 1, 4, DramaEventType.Momentum, 0.5, TensionTag.Calm);

            foreach (bool over in new[] { true, false })
            {
                Leg homeLeg = BuildCountLeg(MarketSelection.TotalCards(3.5, over), $"S01-CARD-HOME-{over}");
                var homeCounts = new CountLedger();
                homeCounts.ConfigureEndpoint(targetHome: 1, targetAway: 0, beatCount: 1);
                SceneSpec homeSpec = choreo.ResolveBeat(evt, up: true, delta: 0.05, new ScoreLedger(), homeLeg, homeCounts);
                Assert.AreEqual(SceneTemplate.Booking, homeSpec.Template);
                Assert.IsTrue(homeSpec.CountBeneficiaryIsHome.HasValue && homeSpec.CountBeneficiaryIsHome.Value,
                    $"over={over}: a home-credited booking must attribute home regardless of the pick");

                Leg awayLeg = BuildCountLeg(MarketSelection.TotalCards(3.5, over), $"S01-CARD-AWAY-{over}");
                var awayCounts = new CountLedger();
                awayCounts.ConfigureEndpoint(targetHome: 0, targetAway: 1, beatCount: 1);
                SceneSpec awaySpec = choreo.ResolveBeat(evt, up: false, delta: -0.05, new ScoreLedger(), awayLeg, awayCounts);
                Assert.AreEqual(SceneTemplate.Booking, awaySpec.Template);
                Assert.IsTrue(awaySpec.CountBeneficiaryIsHome.HasValue && !awaySpec.CountBeneficiaryIsHome.Value,
                    $"over={over}: an away-credited booking must attribute away regardless of the pick");
            }
        }

        [Test]
        public void Goal_attribution_on_a_moneyline_leg_is_unchanged_by_the_count_attribution_fix()
        {
            // Regression #4: the goal path's ForPicked/ScoredByPicked semantics are untouched,
            // and a non-count scene carries no count-beneficiary fact at all.
            var choreo = new TheaterChoreographer(new SweatPacer());
            Leg homeLeg = BuildCountLeg(MarketSelection.Moneyline(Side.Home), "S01-GOAL-ML");

            SceneSpec up = choreo.ResolveBeat(
                new DramaEvent(0, 1, 6, DramaEventType.Score, 0.7, TensionTag.Swing),
                up: true, delta: 0.05, new ScoreLedger(), homeLeg, null);
            Assert.AreEqual(SceneTemplate.GoalFor, up.Template);
            Assert.IsTrue(up.Goal.HasValue && up.Goal.Value.ForPicked);
            Assert.IsFalse(up.CountBeneficiaryIsHome.HasValue, "a goal scene carries no count-beneficiary fact");

            SceneSpec down = choreo.ResolveBeat(
                new DramaEvent(0, 2, 6, DramaEventType.Score, 0.3, TensionTag.Swing),
                up: false, delta: -0.05, new ScoreLedger(), homeLeg, null);
            Assert.AreEqual(SceneTemplate.GoalAgainst, down.Template);
            Assert.IsTrue(down.Goal.HasValue && !down.Goal.Value.ForPicked);
            Assert.IsFalse(down.CountBeneficiaryIsHome.HasValue);
        }

        [Test]
        public void Concurrent_corners_and_cards_legs_on_one_match_each_attribute_independently()
        {
            // PRD §8.2A: two legs can be live on the SAME match at once — here a corners leg
            // and a cards leg on the same fixture, placed as two separate tickets (a single
            // ticket cannot carry two legs on one matchup — Run.PlaceTicket enforces that).
            // The fix must attribute each leg's own staged batches correctly regardless of
            // interleaving, and never reach back to "the active leg" to decide it.
            var run = new Run("S01-CONCURRENT", new RunConfig());
            Ticket cornersTicket = run.PlaceTicket(new[] { new Pick(0, MarketSelection.TotalCorners(8.5, true)) }, 10);
            Ticket cardsTicket = run.PlaceTicket(new[] { new Pick(0, MarketSelection.TotalCards(3.5, false)) }, 10);
            run.LockRound();
            Leg cornersLeg = cornersTicket.Legs[0];
            Leg cardsLeg = cardsTicket.Legs[0];
            Assert.AreSame(cornersLeg.Matchup, cardsLeg.Matchup, "both legs ride the same match");

            var cornersCounts = new CountLedger();
            cornersCounts.ConfigureEndpoint(cornersLeg.Matchup.StatLine, MarketKind.TotalCorners, 6);
            var cardsCounts = new CountLedger();
            cardsCounts.ConfigureEndpoint(cardsLeg.Matchup.StatLine, MarketKind.TotalCards, 6);
            var choreo = new TheaterChoreographer(new SweatPacer());

            // Drive both legs' beats interleaved (cards, corners, cards, corners, ...) to prove
            // neither ledger's attribution depends on the other or on ordering.
            for (int step = 1; step <= 6; step++)
            {
                SceneSpec cardsSpec = choreo.ResolveBeat(
                    new DramaEvent(1, step, 6, DramaEventType.Momentum, 0.5, TensionTag.Calm),
                    up: step % 2 == 0, delta: step % 2 == 0 ? 0.05 : -0.05, new ScoreLedger(), cardsLeg, cardsCounts);
                if (cardsSpec.Count.HasValue && cardsSpec.Count.Value.TotalDelta > 0)
                {
                    CountLedger.StagedCount c = cardsSpec.Count.Value;
                    Assert.AreEqual(SceneTemplate.Booking, cardsSpec.Template);
                    if (c.HomeDelta > c.AwayDelta) Assert.IsTrue(c.BeneficiaryIsHome);
                    if (c.AwayDelta > c.HomeDelta) Assert.IsFalse(c.BeneficiaryIsHome);
                    Assert.AreEqual(c.BeneficiaryIsHome, cardsSpec.CountBeneficiaryIsHome);
                }

                SceneSpec cornersSpec = choreo.ResolveBeat(
                    new DramaEvent(0, step, 6, DramaEventType.Momentum, 0.5, TensionTag.Calm),
                    up: step % 2 == 0, delta: step % 2 == 0 ? 0.05 : -0.05, new ScoreLedger(), cornersLeg, cornersCounts);
                if (cornersSpec.Count.HasValue && cornersSpec.Count.Value.TotalDelta > 0)
                {
                    CountLedger.StagedCount c = cornersSpec.Count.Value;
                    // cornersLeg is a fixed Over pick for the whole test, so the MOOD template
                    // is always CornerFor regardless of which team the engine actually credits —
                    // template tracks the bet, never the team (reviewer correction).
                    Assert.AreEqual(SceneTemplate.CornerFor, cornersSpec.Template);
                    // ROUTING tracks the team fact instead, independently of that fixed mood.
                    if (c.HomeDelta > c.AwayDelta) Assert.IsTrue(c.BeneficiaryIsHome);
                    if (c.AwayDelta > c.HomeDelta) Assert.IsFalse(c.BeneficiaryIsHome);
                    Assert.AreEqual(c.BeneficiaryIsHome, cornersSpec.CountBeneficiaryIsHome);
                }
            }

            // Both ledgers converge to their OWN market's endpoint from the same locked match —
            // proof neither leg's schedule leaked into the other's.
            Assert.AreEqual(cornersLeg.Matchup.StatLine.HomeCorners + cornersLeg.Matchup.StatLine.AwayCorners,
                SumPlanned(cornersCounts));
            Assert.AreEqual(cardsLeg.Matchup.StatLine.HomeCards + cardsLeg.Matchup.StatLine.AwayCards,
                SumPlanned(cardsCounts));
        }

        private static int SumPlanned(CountLedger ledger)
        {
            int sum = 0;
            foreach (int d in ledger.PlannedDeltas) sum += d;
            return sum;
        }
    }
}
