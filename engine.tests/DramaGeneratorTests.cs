using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Engine.Tests;

public class DramaGeneratorTests
{
    // Builds a single ticket's full drama paths. The round is locked (so leg outcomes exist),
    // then paths are regenerated from a fresh Drama stream — identical to the session's, since a
    // one-ticket round consumes the Drama stream from position 0.
    private static (Ticket ticket, IReadOnlyList<IReadOnlyList<DramaEvent>> paths) BuildPaths(
        string seed, Pick[] picks, RunConfig? cfg = null)
    {
        cfg ??= new RunConfig();
        var run = new Run(seed, cfg);
        Ticket ticket = run.PlaceTicket(picks, 100);
        run.LockRound();
        var paths = DramaGenerator.BuildTicketPaths(ticket, new RngHub(seed).Drama, cfg.Drama, round: 1);
        return (ticket, paths);
    }

    private static Pick[] Parlay(params int[] matchups)
    {
        var picks = new Pick[matchups.Length];
        for (int i = 0; i < matchups.Length; i++) picks[i] = new Pick(matchups[i], Side.Home);
        return picks;
    }

    [Fact]
    public void First_beat_of_every_leg_is_a_valid_live_probability()
    {
        var (_, paths) = BuildPaths("DRAMA-A", Parlay(0, 1, 2));
        foreach (var leg in paths)
            Assert.InRange(leg[0].WinProbAfter, DramaGenerator.FloorProb, DramaGenerator.CeilProb);
    }

    [Fact]
    public void Final_beat_snaps_to_the_revealed_outcome()
    {
        var (ticket, paths) = BuildPaths("DRAMA-B", Parlay(0, 1, 2));
        for (int i = 0; i < paths.Count; i++)
        {
            DramaEvent last = paths[i][paths[i].Count - 1];
            Assert.Equal(DramaEventType.LegFinal, last.Type);
            double expected = ticket.Legs[i].State == LegState.Won ? 1.0 : 0.0;
            Assert.Equal(expected, last.WinProbAfter, 12);
        }
    }

    [Fact]
    public void Intermediate_beats_stay_clamped_final_beats_are_endpoints()
    {
        var (_, paths) = BuildPaths("DRAMA-C", Parlay(0, 1, 2, 3));
        foreach (var leg in paths)
        {
            for (int i = 0; i < leg.Count; i++)
            {
                bool isFinal = i == leg.Count - 1;
                if (isFinal)
                    Assert.True(leg[i].WinProbAfter == 0.0 || leg[i].WinProbAfter == 1.0);
                else
                    Assert.InRange(leg[i].WinProbAfter, DramaGenerator.FloorProb, DramaGenerator.CeilProb);
            }
        }
    }

    [Fact]
    public void Final_leg_of_a_multi_leg_ticket_gets_double_the_budget()
    {
        // Pin the draw so the budget is exact: every base count is 6, the final leg doubles to 12.
        // DensityRampRounds = 1 disables the progressive-density ramp so round 1 uses the full band.
        var cfg = new RunConfig { Drama = new DramaConfig { MinEventsPerLeg = 6, MaxEventsPerLeg = 6, DensityRampRounds = 1 } };
        var (_, paths) = BuildPaths("DRAMA-D", Parlay(0, 1, 2), cfg);

        Assert.Equal(6, paths[0].Count);
        Assert.Equal(6, paths[1].Count);
        Assert.Equal(12, paths[2].Count);
        // Every beat also reports the same TotalSteps as its leg's length.
        foreach (var leg in paths)
            Assert.All(leg, e => Assert.Equal(leg.Count, e.TotalSteps));
    }

    [Fact]
    public void Event_counts_fall_in_the_configured_band()
    {
        var cfg = new RunConfig();
        var (_, paths) = BuildPaths("DRAMA-E", Parlay(0, 1, 2), cfg);
        // Round 1 is on the progressive-density ramp: the band is EventBoundsForRound(1), not the full band.
        (int min, int max) = cfg.Drama.EventBoundsForRound(1);
        for (int i = 0; i < paths.Count; i++)
        {
            bool isFinal = i == paths.Count - 1;
            int lo = isFinal ? min * 2 : min;
            int hi = isFinal ? max * 2 : max;
            Assert.InRange(paths[i].Count, lo, hi);
        }
    }

    [Fact]
    public void Event_bounds_ramp_from_early_to_full_band()
    {
        var d = new DramaConfig(); // Early 2–4 → full 3–5 over DensityRampRounds = 3
        Assert.Equal((2, 4), d.EventBoundsForRound(1));
        Assert.Equal((3, 5), d.EventBoundsForRound(2)); // t = 0.5: 2.5→3, 4.5→5 (AwayFromZero)
        Assert.Equal((3, 5), d.EventBoundsForRound(3));
        Assert.Equal((3, 5), d.EventBoundsForRound(8));
    }

    [Fact]
    public void Event_bounds_normalize_degenerate_configs_in_both_branches()
    {
        // Full-band branch: zero bounds clamp to (1, 1) — the sim sweeps configs programmatically.
        var zero = new DramaConfig { MinEventsPerLeg = 0, MaxEventsPerLeg = 0, DensityRampRounds = 1 };
        Assert.Equal((1, 1), zero.EventBoundsForRound(1));

        // Inverted full band clamps max up to min.
        var inverted = new DramaConfig { MinEventsPerLeg = 5, MaxEventsPerLeg = 2, DensityRampRounds = 1 };
        Assert.Equal((5, 5), inverted.EventBoundsForRound(4));

        // Ramp branch: degenerate early bounds clamp the same way.
        var earlyZero = new DramaConfig { EarlyMinEventsPerLeg = 0, EarlyMaxEventsPerLeg = 0, DensityRampRounds = 5 };
        Assert.Equal((1, 1), earlyZero.EventBoundsForRound(1));

        // Ramp disabled (≤ 1) always returns the normalized full band.
        var disabled = new DramaConfig { DensityRampRounds = 0 };
        Assert.Equal((3, 5), disabled.EventBoundsForRound(1));
    }

    [Fact]
    public void Event_type_follows_the_win_prob_delta_thresholds()
    {
        var (ticket, paths) = BuildPaths("DRAMA-F", Parlay(0, 1, 2));
        for (int i = 0; i < paths.Count; i++)
        {
            IReadOnlyList<DramaEvent> leg = paths[i];
            double prev = ticket.Legs[i].TrueProb; // p_0 anchor
            for (int s = 0; s < leg.Count; s++)
            {
                DramaEvent e = leg[s];
                bool isFinal = s == leg.Count - 1;
                double absDelta = System.Math.Abs(e.WinProbAfter - prev);

                if (isFinal)
                {
                    Assert.Equal(DramaEventType.LegFinal, e.Type);
                    Assert.Equal(TensionTag.Decisive, e.Tag);
                }
                else
                {
                    DramaEventType expected =
                        absDelta >= DramaGenerator.BigPlayDelta ? DramaEventType.BigPlay
                        : absDelta >= DramaGenerator.ScoreDelta ? DramaEventType.Score
                        : DramaEventType.Momentum;
                    Assert.Equal(expected, e.Type);
                    Assert.NotEqual(TensionTag.Decisive, e.Tag); // Decisive is the final beat only
                }
                prev = e.WinProbAfter;
            }
        }
    }

    [Fact]
    public void Paths_are_deterministic_for_a_seed()
    {
        var a = BuildPaths("DRAMA-DET", Parlay(0, 1, 2)).paths;
        var b = BuildPaths("DRAMA-DET", Parlay(0, 1, 2)).paths;
        Assert.Equal(a.Count, b.Count);
        for (int i = 0; i < a.Count; i++)
        {
            Assert.Equal(a[i].Count, b[i].Count);
            for (int s = 0; s < a[i].Count; s++)
            {
                Assert.Equal(a[i][s].WinProbAfter, b[i][s].WinProbAfter, 15);
                Assert.Equal(a[i][s].Type, b[i][s].Type);
                Assert.Equal(a[i][s].Tag, b[i][s].Tag);
            }
        }
    }

    [Fact]
    public void Degenerate_config_still_emits_at_least_the_final_beat()
    {
        // Multiplier 0 would give the final leg an empty path and crash the session cursor.
        var cfg = new RunConfig { Drama = new DramaConfig { FinalLegBudgetMultiplier = 0.0 } };
        var (_, paths) = BuildPaths("DRAMA-K0", Parlay(0, 1), cfg);

        Assert.True(paths[1].Count >= 1);
        Assert.Equal(DramaEventType.LegFinal, paths[1][paths[1].Count - 1].Type);
    }

    [Fact]
    public void Building_paths_before_lock_throws()
    {
        var run = new Run("DRAMA-PENDING");
        Ticket ticket = run.PlaceTicket(Parlay(0, 1), 100);
        // Not locked: leg outcomes are still Pending.
        Assert.Throws<InvalidOperationException>(
            () => DramaGenerator.BuildTicketPaths(ticket, run.Rng.Drama, run.Config.Drama, round: 1));
    }

    // Deterministic near-miss pins: seeds found by scan, hard-coded so a break is a regression.

    [Fact]
    public void Near_miss_lifts_a_losing_leg_to_a_late_hope_spike()
    {
        // Seed NM2-10 (re-scanned for the F_0.2.0 M-T1 budgets): the final leg (index 2) is Lost
        // and carries an authored hope-spike >= 0.75.
        var (ticket, paths) = BuildPaths("NM2-10", Parlay(0, 1, 2));
        IReadOnlyList<DramaEvent> lastLeg = paths[2];
        Assert.Equal(LegState.Lost, ticket.Legs[2].State);

        bool hopeSpike = false;
        foreach (DramaEvent e in lastLeg)
            if (e.Tag == TensionTag.NearMiss && e.WinProbAfter >= 0.75 && e.Step > 2 * e.TotalSteps / 3)
                hopeSpike = true;
        Assert.True(hopeSpike, "expected a late NearMiss beat >= 0.75 on the losing final leg");
    }

    [Fact]
    public void Near_miss_dips_a_winning_leg_to_a_late_scare()
    {
        // Seed SC-99 (re-scanned for the F_0.2.0 M-T1 budgets): leg 0 (matchup 0, Away) is Won
        // and carries an authored scare-dip <= 0.25.
        var (ticket, paths) = BuildPaths("SC-99", new[] { new Pick(0, Side.Away), new Pick(2, Side.Away) });
        IReadOnlyList<DramaEvent> firstLeg = paths[0];
        Assert.Equal(LegState.Won, ticket.Legs[0].State);

        bool scareDip = false;
        foreach (DramaEvent e in firstLeg)
            if (e.Tag == TensionTag.NearMiss && e.WinProbAfter <= 0.25 && e.Step > 2 * e.TotalSteps / 3)
                scareDip = true;
        Assert.True(scareDip, "expected a late NearMiss beat <= 0.25 on the winning leg");
    }
}
