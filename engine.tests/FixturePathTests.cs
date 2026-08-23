using System;
using System.Collections.Generic;
using System.Reflection;
using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// T140 arm A: <see cref="DramaGenerator.BuildTicketPaths"/> now returns one path per
/// (ticket, FIXTURE) rather than one per leg (see engine/DramaGenerator.cs, engine/DramaEvent.cs
/// doc comments for the full reasoning). These tests build tickets WITHOUT a <see cref="Run"/> or
/// a <see cref="SweatSession"/> — straight off a generated <see cref="Slate"/> — so the
/// fixture-grouping behaviour can be checked in isolation from placement/refusal/session
/// machinery.
///
/// <para><b>A finding, not a workaround of convenience.</b> <see cref="Matchup.StatLine"/> has an
/// `internal` setter, and <c>SBR.Engine</c> grants no <c>[InternalsVisibleTo]</c> to
/// <c>SBR.Engine.Tests</c> anywhere in this repo (checked). So the direct assignment
/// <c>matchup.StatLine = MatchModel.SampleStatLine(matchup, rng)</c> — the literal call shape
/// <c>engine/Run.cs</c> <c>LockRound</c> uses at ~line 399 — does not compile from this assembly.
/// <see cref="SetStatLine"/> below pokes the same setter through reflection instead. It still does
/// not construct a <see cref="SweatSession"/> or call <see cref="Run.LockRound"/>.</para>
/// </summary>
public class FixturePathTests
{
    /// <summary>Generates a slate and resolves every matchup's stat line, mirroring
    /// <c>Run.LockRound</c>'s own loop (engine/Run.cs ~396-400) through a public
    /// <see cref="RngHub"/> instead of a live <see cref="Run"/>.</summary>
    private static Slate ResolvedSlate(string seed)
    {
        var hub = new RngHub(seed);
        Slate slate = SlateGenerator.Generate(1, hub.Slate, new RunConfig());
        foreach (Matchup m in slate.Matchups)
            SetStatLine(m, MatchModel.SampleStatLine(m, hub.Outcomes));
        return slate;
    }

    /// <summary>Sets <see cref="Matchup.StatLine"/> via its `internal` setter. See the type-level
    /// doc comment above: there is no public path to this from <c>SBR.Engine.Tests</c>.</summary>
    private static void SetStatLine(Matchup matchup, MatchStatLine line)
    {
        PropertyInfo? prop = typeof(Matchup).GetProperty(nameof(Matchup.StatLine),
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop == null || prop.SetMethod == null)
            throw new InvalidOperationException(
                "Matchup.StatLine setter not found by reflection; the internal-access workaround broke.");
        prop.SetValue(matchup, line);
    }

    private static Leg BuildLeg(Matchup matchup, Side side)
        => new Leg(matchup, MarketSelection.Moneyline(side), matchup.Odds(side));

    private static Ticket BuildTicket(params Leg[] legs)
        => new Ticket(legs, stake: 100.0, vigPaid: 0.0, lockedPrice: 2.0);

    /// <summary>Group C/D's shared proof: every field bit-identical, <c>WinProbAfter</c> with
    /// `==` — not an epsilon.</summary>
    private static void AssertPathsBitIdentical(
        IReadOnlyList<IReadOnlyList<DramaEvent>> expected,
        IReadOnlyList<IReadOnlyList<DramaEvent>> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (int p = 0; p < expected.Count; p++)
        {
            IReadOnlyList<DramaEvent> ep = expected[p];
            IReadOnlyList<DramaEvent> ap = actual[p];
            Assert.Equal(ep.Count, ap.Count);
            for (int i = 0; i < ep.Count; i++)
            {
                DramaEvent e = ep[i];
                DramaEvent a = ap[i];
                Assert.Equal(e.LegIndex, a.LegIndex);
                Assert.Equal(e.FixtureIndex, a.FixtureIndex);
                Assert.Equal(e.Step, a.Step);
                Assert.Equal(e.TotalSteps, a.TotalSteps);
                Assert.Equal(e.Type, a.Type);
                Assert.Equal(e.Tag, a.Tag);
                Assert.True(e.WinProbAfter == a.WinProbAfter,
                    $"path {p} beat {i}: WinProbAfter {e.WinProbAfter} != {a.WinProbAfter}");
            }
        }
    }

    // ============================================================================== group A

    [Fact]
    public void Solo_tellings_are_unchanged_in_shape()
    {
        Slate slate = ResolvedSlate("FP-A-SLATE");
        Ticket ticket = BuildTicket(
            BuildLeg(slate.Matchups[0], Side.Home),
            BuildLeg(slate.Matchups[1], Side.Away),
            BuildLeg(slate.Matchups[2], Side.Home));

        var cfg = new RunConfig();
        var hub = new RngHub("FP-A-DRAMA");
        IReadOnlyList<IReadOnlyList<DramaEvent>> paths =
            DramaGenerator.BuildTicketPaths(ticket, hub.Drama, cfg.Drama, round: 1);

        Assert.Equal(ticket.Legs.Count, paths.Count);

        for (int legIndex = 0; legIndex < paths.Count; legIndex++)
        {
            IReadOnlyList<DramaEvent> path = paths[legIndex];
            int totalSteps = path[0].TotalSteps;
            int legFinalCount = 0;

            for (int i = 0; i < path.Count; i++)
            {
                DramaEvent beat = path[i];
                Assert.Equal(legIndex, beat.FixtureIndex);
                Assert.False(beat.IsSharedTelling);
                Assert.Equal(legIndex, Assert.Single(beat.LegIndices));
                double onlyProb = Assert.Single(beat.LegProbs);
                Assert.True(onlyProb == beat.WinProbAfter,
                    $"leg {legIndex} beat {i}: LegProbs[0] {onlyProb} != WinProbAfter {beat.WinProbAfter}");
                Assert.Equal(totalSteps, beat.TotalSteps);
                Assert.Equal(i + 1, beat.Step);
                if (beat.Type == DramaEventType.LegFinal) legFinalCount++;
            }

            Assert.Equal(1, legFinalCount);
            Assert.Equal(totalSteps, path.Count); // Step runs 1..TotalSteps with no gaps
            Assert.Equal(DramaEventType.LegFinal, path[path.Count - 1].Type);
        }
    }

    // ============================================================================== group B

    [Fact]
    public void Shared_telling_groups_non_contiguous_legs_into_one_whistle()
    {
        Slate slate = ResolvedSlate("FP-B-SLATE");
        Matchup x = slate.Matchups[0];
        Matchup y = slate.Matchups[1];

        // legs 0 and 2 ride matchup X (deliberately non-contiguous); leg 1 rides matchup Y.
        Ticket ticket = BuildTicket(
            BuildLeg(x, Side.Home),
            BuildLeg(y, Side.Home),
            BuildLeg(x, Side.Away));

        var cfg = new RunConfig();
        var hub = new RngHub("FP-B-DRAMA");
        IReadOnlyList<IReadOnlyList<DramaEvent>> paths =
            DramaGenerator.BuildTicketPaths(ticket, hub.Drama, cfg.Drama, round: 1);

        // Arm A's falsifier: three legs, two fixtures. Before the change this would have been 3.
        Assert.Equal(2, paths.Count);

        IReadOnlyList<DramaEvent> shared = paths[0]; // matchup X: first appearance at leg 0
        IReadOnlyList<DramaEvent> solo = paths[1];   // matchup Y: first appearance at leg 1

        int sharedTotalSteps = shared[0].TotalSteps;
        int legFinalCount = 0;
        for (int i = 0; i < shared.Count; i++)
        {
            DramaEvent beat = shared[i];
            Assert.True(beat.IsSharedTelling);
            Assert.Equal(2, beat.LegIndices.Count);
            Assert.Equal(0, beat.LegIndices[0]); // ascending ticket order: leg 0 first
            Assert.Equal(2, beat.LegIndices[1]); // then leg 2
            Assert.Equal(beat.LegIndices.Count, beat.LegProbs.Count);
            Assert.Equal(0, beat.FixtureIndex);
            Assert.Equal(sharedTotalSteps, beat.TotalSteps);
            Assert.Equal(i + 1, beat.Step); // the shared clock: no regression, no gap
            if (beat.Type == DramaEventType.LegFinal) legFinalCount++;
        }
        Assert.Equal(1, legFinalCount);
        Assert.Equal(sharedTotalSteps, shared.Count);
        DramaEvent finalBeat = shared[shared.Count - 1];
        Assert.Equal(DramaEventType.LegFinal, finalBeat.Type);

        // The point of the whole restructure: one whistle, each leg arrives at its OWN result.
        // Read the outcome off the engine, never assume which side won.
        double expectedLeg0 = ticket.Legs[0].State == LegState.Won ? 1.0 : 0.0;
        double expectedLeg2 = ticket.Legs[2].State == LegState.Won ? 1.0 : 0.0;
        Assert.True(finalBeat.LegProbs[0] == expectedLeg0,
            $"leg 0 final LegProbs {finalBeat.LegProbs[0]} != its own outcome {expectedLeg0}");
        Assert.True(finalBeat.LegProbs[1] == expectedLeg2,
            $"leg 2 final LegProbs {finalBeat.LegProbs[1]} != its own outcome {expectedLeg2}");

        Assert.All(solo, beat => Assert.Equal(1, beat.FixtureIndex));
        Assert.NotEqual(shared[0].FixtureIndex, solo[0].FixtureIndex);
    }

    // ============================================================================== group C

    [Fact]
    public void Same_match_ticket_leaves_the_drama_stream_where_an_equal_leg_count_ticket_would()
    {
        Slate slate = ResolvedSlate("FP-C-SLATE");

        // A_same: 3 legs, two riding one matchup (emits 2 paths, not 3).
        Ticket aSame = BuildTicket(
            BuildLeg(slate.Matchups[0], Side.Home),
            BuildLeg(slate.Matchups[0], Side.Away),
            BuildLeg(slate.Matchups[1], Side.Home));

        // A_ordinary: the same LEG COUNT (3), every leg its own matchup.
        Ticket aOrdinary = BuildTicket(
            BuildLeg(slate.Matchups[2], Side.Home),
            BuildLeg(slate.Matchups[3], Side.Home),
            BuildLeg(slate.Matchups[4], Side.Home));

        // B: a later, unrelated ticket in the round. The SAME ticket object is reused for both
        // runs below — it is never mutated by BuildTicketPaths, so reuse cannot itself explain a
        // match; only the Drama stream's position when B is built can.
        Ticket ticketB = BuildTicket(
            BuildLeg(slate.Matchups[5], Side.Home),
            BuildLeg(slate.Matchups[0], Side.Away));

        var cfg = new RunConfig();
        const string dramaSeed = "FP-C-DRAMA";

        var hub1 = new RngHub(dramaSeed);
        DramaGenerator.BuildTicketPaths(aSame, hub1.Drama, cfg.Drama, round: 1);
        IReadOnlyList<IReadOnlyList<DramaEvent>> pathsB1 =
            DramaGenerator.BuildTicketPaths(ticketB, hub1.Drama, cfg.Drama, round: 1);

        var hub2 = new RngHub(dramaSeed); // fresh Pcg32 stream, identical seed
        DramaGenerator.BuildTicketPaths(aOrdinary, hub2.Drama, cfg.Drama, round: 1);
        IReadOnlyList<IReadOnlyList<DramaEvent>> pathsB2 =
            DramaGenerator.BuildTicketPaths(ticketB, hub2.Drama, cfg.Drama, round: 1);

        // THE falsifier: if arm A had regenerated per-fixture draws instead of only reassembling
        // per-leg ones, B would read a shifted stream in one run and this would fail.
        AssertPathsBitIdentical(pathsB1, pathsB2);
    }

    // ============================================================================== group D

    [Fact]
    public void Same_seed_same_ticket_two_independent_hubs_produce_identical_paths()
    {
        Slate slate = ResolvedSlate("FP-D-SLATE");
        Ticket ticket = BuildTicket(
            BuildLeg(slate.Matchups[0], Side.Home),
            BuildLeg(slate.Matchups[0], Side.Away),
            BuildLeg(slate.Matchups[1], Side.Home));

        var cfg = new RunConfig();
        const string dramaSeed = "FP-D-DRAMA";
        var hubA = new RngHub(dramaSeed);
        var hubB = new RngHub(dramaSeed);

        IReadOnlyList<IReadOnlyList<DramaEvent>> pathsA =
            DramaGenerator.BuildTicketPaths(ticket, hubA.Drama, cfg.Drama, round: 1);
        IReadOnlyList<IReadOnlyList<DramaEvent>> pathsB =
            DramaGenerator.BuildTicketPaths(ticket, hubB.Drama, cfg.Drama, round: 1);

        AssertPathsBitIdentical(pathsA, pathsB);
    }
}
