using System.Collections.Generic;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// The resolver's laws (F_0.2.0 M-T3): totality over all 40 (Type × dir × Tag) combos,
    /// NearMiss never a goal, Momentum×Calm → the #11 calm variant, overlays never choose the
    /// template; the pacer's declared bands (beat-scenes in [3, 8]s, correction sub-scenes
    /// 2.5s × ≤2 so a final runs ≤13s); and the DECLARED duration-acceptance criterion over
    /// ≥1,000 full-density 3-leg generator paths (median 60–90s, ≥80% in 50–100s, worst ≤110s).
    ///
    /// Unreachable combos, documented per the plan (they resolve through the same path,
    /// no special-casing): BigPlay×Calm and Momentum×Swing cannot occur since Swing needs
    /// |Δp| ≥ 0.10 > the 0.07 Score threshold; Decisive appears only on LegFinal.
    /// </summary>
    public class TheaterChoreographerTests
    {
        private static readonly DramaEventType[] AllTypes =
        {
            DramaEventType.Momentum, DramaEventType.Score, DramaEventType.BigPlay, DramaEventType.LegFinal,
        };

        private static readonly TensionTag[] AllTags =
        {
            TensionTag.Calm, TensionTag.Swing, TensionTag.LeadChange, TensionTag.NearMiss, TensionTag.Decisive,
        };

        private static DramaEvent Evt(DramaEventType type, TensionTag tag, double probAfter, int step = 2)
            => new DramaEvent(0, step, 6, type, probAfter, tag);

        [Test]
        public void Resolver_is_total_over_all_40_combos()
        {
            var choreo = new TheaterChoreographer(new SweatPacer());
            int combos = 0;
            foreach (DramaEventType type in AllTypes)
                foreach (TensionTag tag in AllTags)
                    foreach (bool up in new[] { true, false })
                    {
                        combos++;
                        SceneSpec spec = choreo.ResolveBeat(
                            Evt(type, tag, up ? 0.7 : 0.3), up, new ScoreLedger());
                        Assert.Greater(spec.Duration, 0f, $"({type}, {tag}, up={up}) got no duration");
                        Assert.IsTrue(ScenePlaybook.IsBeatScene(spec.Template),
                            $"({type}, {tag}, up={up}) resolved to a structural template");
                    }
            Assert.AreEqual(40, combos, "the full (Type × Tag × dir) space");
        }

        [Test]
        public void Near_miss_never_stages_a_goal_regardless_of_type()
        {
            var choreo = new TheaterChoreographer(new SweatPacer());
            foreach (DramaEventType type in new[] { DramaEventType.Score, DramaEventType.BigPlay, DramaEventType.Momentum })
            {
                SceneSpec hope = choreo.ResolveBeat(Evt(type, TensionTag.NearMiss, 0.8), true, new ScoreLedger());
                Assert.AreEqual(SceneTemplate.NearMissHope, hope.Template, $"{type} up");
                Assert.IsNull(hope.Goal, $"{type}: a near-miss is never a goal");

                SceneSpec scare = choreo.ResolveBeat(Evt(type, TensionTag.NearMiss, 0.2), false, new ScoreLedger());
                Assert.AreEqual(SceneTemplate.NearMissScare, scare.Template, $"{type} down");
                Assert.IsNull(scare.Goal);
            }
        }

        [Test]
        public void Base_table_maps_type_and_direction()
        {
            var choreo = new TheaterChoreographer(new SweatPacer());
            var ledger = new ScoreLedger();
            Assert.AreEqual(SceneTemplate.GoalFor,
                choreo.ResolveBeat(Evt(DramaEventType.Score, TensionTag.Swing, 0.7), true, ledger).Template);
            Assert.AreEqual(SceneTemplate.GoalAgainst,
                choreo.ResolveBeat(Evt(DramaEventType.Score, TensionTag.Swing, 0.3), false, ledger).Template);
            Assert.AreEqual(SceneTemplate.BreakawayFor,
                choreo.ResolveBeat(Evt(DramaEventType.BigPlay, TensionTag.Swing, 0.8), true, ledger).Template);
            Assert.AreEqual(SceneTemplate.BreakawayAgainst,
                choreo.ResolveBeat(Evt(DramaEventType.BigPlay, TensionTag.Swing, 0.2), false, ledger).Template);
            Assert.AreEqual(SceneTemplate.TerritoryFor,
                choreo.ResolveBeat(Evt(DramaEventType.Momentum, TensionTag.LeadChange, 0.55), true, ledger).Template);
            Assert.AreEqual(SceneTemplate.TerritoryAgainst,
                choreo.ResolveBeat(Evt(DramaEventType.Momentum, TensionTag.LeadChange, 0.45), false, ledger).Template);
            Assert.AreEqual(SceneTemplate.CalmPossession,
                choreo.ResolveBeat(Evt(DramaEventType.Momentum, TensionTag.Calm, 0.55), true, ledger).Template,
                "Momentum with Tag==Calm uses the #11 calm variant");
        }

        [Test]
        public void Overlays_modify_playback_but_never_choose_the_template()
        {
            var choreo = new TheaterChoreographer(new SweatPacer());
            SceneSpec plain = choreo.ResolveBeat(Evt(DramaEventType.Score, TensionTag.Calm, 0.7), true, new ScoreLedger());
            SceneSpec lead = choreo.ResolveBeat(Evt(DramaEventType.Score, TensionTag.LeadChange, 0.7), true, new ScoreLedger());
            SceneSpec swing = choreo.ResolveBeat(Evt(DramaEventType.Score, TensionTag.Swing, 0.7), true, new ScoreLedger());

            Assert.AreEqual(plain.Template, lead.Template, "#9 is an overlay, not a template");
            Assert.AreEqual(plain.Template, swing.Template, "#10 is an overlay, not a template");
            Assert.IsTrue(lead.LeadChangeIntro);
            Assert.IsFalse(plain.LeadChangeIntro);
            Assert.IsTrue(swing.Urgent);
            Assert.Greater(lead.Duration, plain.Duration, "the intro adds its extra on top");
        }

        [Test]
        public void Goal_scenes_carry_the_ledger_ruling_chalked_when_clamped()
        {
            var choreo = new TheaterChoreographer(new SweatPacer());
            var ledger = new ScoreLedger();
            ledger.CompleteGoal(ledger.StageBeatGoal(DramaEventType.Score, true).Value); // 1-0

            SceneSpec spec = choreo.ResolveBeat(Evt(DramaEventType.Score, TensionTag.Swing, 0.9), true, ledger);
            Assert.IsTrue(spec.Goal.HasValue, "a Score beat stages a goal playback");
            Assert.IsFalse(spec.Goal.Value.Commits, "the 2-0 goal stages as the chalked-off variant");
        }

        // ---------------------------------------------------------------- pacer bands

        [Test]
        public void Every_beat_scene_lands_in_the_3_to_8_second_band()
        {
            var pacer = new SweatPacer();
            var beatScenes = new[]
            {
                SceneTemplate.GoalFor, SceneTemplate.GoalAgainst,
                SceneTemplate.BreakawayFor, SceneTemplate.BreakawayAgainst,
                SceneTemplate.TerritoryFor, SceneTemplate.TerritoryAgainst,
                SceneTemplate.NearMissHope, SceneTemplate.NearMissScare,
                SceneTemplate.CalmPossession, SceneTemplate.LegFinalWon,
                SceneTemplate.LegFinalLost, SceneTemplate.Fallback,
            };
            foreach (SceneTemplate t in beatScenes)
            {
                // The #9 intro only ever composes onto non-final scenes: resolver rule 1
                // returns a final BEFORE the overlay step, so (final, intro) is unreachable.
                bool[] intros = t == SceneTemplate.LegFinalWon || t == SceneTemplate.LegFinalLost
                    ? new[] { false }
                    : new[] { false, true };
                foreach (bool intro in intros)
                {
                    float s = pacer.SceneSeconds(t, intro);
                    Assert.GreaterOrEqual(s, 3f * pacer.paceMultiplier, $"{t} (intro={intro}) under the band");
                    Assert.LessOrEqual(s, 8f * pacer.paceMultiplier, $"{t} (intro={intro}) over the band");
                }
            }
        }

        [Test]
        public void Correction_sub_scenes_are_separately_timed_and_the_final_caps_at_13s()
        {
            var pacer = new SweatPacer();
            float correctionSeconds = pacer.correctionGoalSeconds * pacer.paceMultiplier;
            Assert.AreEqual(2.5f * pacer.paceMultiplier, correctionSeconds, 1e-4f,
                "correction sub-scenes are 2.5s by declaration");
            for (int n = 0; n <= 2; n++)
            {
                float total = pacer.FinalSceneSeconds(n);
                Assert.AreEqual((pacer.legFinalSeconds + n * 2.5f) * pacer.paceMultiplier, total, 1e-4f);
                Assert.LessOrEqual(total, 13f * pacer.paceMultiplier, "a full final whistle sequence runs at most 13s");
            }
        }

        // ---------------------------------------------------------------- duration acceptance

        [Test]
        public void Duration_acceptance_median_60_to_90_over_full_density_paths()
        {
            // The DECLARED criterion (plan §SweatPacer, stated before tuning): ≥1,000
            // engine-generated full-density 3-leg beat paths; summed scene durations
            // (scenes + holds + the actual per-path correction count, idle gaps and
            // pending-window wait excluded): median in [60, 90]s, ≥80% in [50, 100]s,
            // worst ≤ 110s. Early rounds are exempt (progressive density is onboarding).
            var pacer = new SweatPacer();
            var choreo = new TheaterChoreographer(pacer);
            var durations = new List<double>(1000);

            for (int i = 0; i < 1000; i++)
            {
                var run = new Run($"DUR-{i}", new RunConfig());
                Ticket ticket = run.PlaceTicket(
                    new[] { new Pick(0, Side.Home), new Pick(1, Side.Away), new Pick(2, Side.Home) }, 50);
                run.LockRound();

                var cfg = new DramaConfig();
                var paths = DramaGenerator.BuildTicketPaths(
                    ticket, new Pcg32((ulong)(77000 + i), 54), cfg, round: cfg.DensityRampRounds);

                var model = new SweatPresentationModel();
                double total = 0;
                for (int legIx = 0; legIx < paths.Count; legIx++)
                {
                    Leg leg = ticket.Legs[legIx];
                    var ledger = new ScoreLedger();
                    foreach (DramaEvent evt in paths[legIx])
                    {
                        bool up = model.RecordBeat(evt, leg);
                        if (evt.Type == DramaEventType.LegFinal)
                        {
                            ScoreLedger.FinalPlan plan =
                                ledger.PlanFinal(leg.GradesWon ? LegGrade.Won : LegGrade.Lost);
                            total += pacer.FinalSceneSeconds(plan.Goals.Length);
                            foreach (ScoreLedger.StagedGoal g in plan.Goals) ledger.CompleteGoal(g);
                        }
                        else
                        {
                            SceneSpec spec = choreo.ResolveBeat(evt, up, ledger);
                            total += spec.Duration;
                            if (spec.Goal.HasValue) ledger.CompleteGoal(spec.Goal.Value);
                        }
                    }
                }
                durations.Add(total);
            }

            durations.Sort();
            double median = durations[durations.Count / 2];
            int inBand = 0;
            foreach (double d in durations)
                if (d >= 50 && d <= 100) inBand++;
            double bandShare = inBand / (double)durations.Count;
            double worst = durations[durations.Count - 1];

            Assert.GreaterOrEqual(median, 60.0, $"median {median:0.0}s under the declared band");
            Assert.LessOrEqual(median, 90.0, $"median {median:0.0}s over the declared band");
            Assert.GreaterOrEqual(bandShare, 0.80, $"only {bandShare:P0} of paths in [50, 100]s");
            Assert.LessOrEqual(worst, 110.0, $"worst path {worst:0.0}s over the ceiling");
        }
    }
}
