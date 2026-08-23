using System.Collections.Generic;
using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// <c>T140</c> ARM A, at the session level: a fixture is broadcast ONCE per ticket, every leg riding
/// it is live for the whole telling, and they all grade at that single whistle.
///
/// <para><c>FixturePathTests</c> pins the PATHS the generator hands over; this pins what the SESSION
/// does with them — the whistle count, the grades landing together, the one window, and the clock
/// that must not regress. The two together are the phase's engine-side evidence; the population
/// counts are <c>G8-ARMA</c>'s in the sim.</para>
///
/// <para>Outcomes are never assumed here. Every test that needs a leg to win or lose SEARCHES seeds
/// for one and asserts relative to <c>Leg.State</c>, because which side of a market comes in is the
/// engine's to say and a hard-coded guess has already cost this lane one false failure.</para>
/// </summary>
public class SharedTellingTests
{
    /// <summary>A ticket with two legs on ONE matchup: the nested goal pair, the same shape the
    /// same-match suites use, so a board change cannot quietly stop producing it.</summary>
    private static (Run run, Ticket ticket, SweatSession session) SameMatchPair(string seed, bool grantMulligan)
    {
        var run = new Run(seed);
        RunConfig cfg = run.Config;
        Pick[] picks =
        {
            new Pick(0, MarketSelection.TotalGoals(cfg.GoalLines[1], true)),
            new Pick(0, MarketSelection.TotalGoals(cfg.GoalLines[0], true)),
        };
        if (run.RefusalFor(picks) != null) return (run, null!, null!);

        Ticket ticket = run.PlaceTicket(picks, 10);
        if (grantMulligan)
            run.GrantConsumable(RelicCatalog.Consumables.First(c => c.Id == "mulligan_slip"));
        run.LockRound();
        return (run, ticket, run.Sweats[0]);
    }

    /// <summary>Walks seeds until one produces the requested pattern of leg outcomes.</summary>
    private static (Run run, Ticket ticket, SweatSession session)? FindOutcome(
        bool grantMulligan, int wantWon, int wantLost, int seeds = 400)
    {
        for (int i = 0; i < seeds; i++)
        {
            var found = SameMatchPair($"shared-telling-{i}", grantMulligan);
            if (found.ticket == null) continue;

            int won = found.ticket.Legs.Count(l => l.State == LegState.Won);
            int lost = found.ticket.Legs.Count(l => l.State == LegState.Lost);
            if (won == wantWon && lost == wantLost) return found;
        }
        return null;
    }

    // ===================================================================== the telling's shape

    [Fact]
    public void Two_legs_on_one_matchup_are_ONE_telling_with_ONE_whistle()
    {
        var (_, ticket, session) = SameMatchPair("shared-telling-0", grantMulligan: false);
        Assert.NotNull(ticket);
        Assert.NotNull(ticket.SameMatch); // the precondition: this really is a same-match ticket

        // TWO legs, ONE fixture. Before arm A this ticket was two tellings of the same match, which
        // is what put `FT` on screen and then rewound the clock to `1'` (T135).
        Assert.Equal(2, ticket.Legs.Count);
        Assert.Equal(1, session.FixtureCount);
        Assert.Equal(new[] { 0, 1 }, session.CurrentFixtureLegs.ToArray());

        int whistles = 0;
        while (session.MoveNext(out DramaEvent? e))
            if (e!.Type == DramaEventType.LegFinal) whistles++;

        // THE FALSIFIER. Two legs settled by one match used to produce two of these.
        Assert.Equal(1, whistles);
    }

    [Fact]
    public void Both_legs_stay_live_for_the_whole_telling_and_grade_together()
    {
        var (_, ticket, session) = SameMatchPair("shared-telling-0", grantMulligan: false);
        Assert.NotNull(ticket);

        var beforeTheWhistle = new List<DramaEvent>();
        DramaEvent? whistle = null;
        while (session.MoveNext(out DramaEvent? e))
        {
            if (e!.Type == DramaEventType.LegFinal) { whistle = e; break; }

            // EVERY leg on the fixture is live for the whole telling (spec §3.2): neither has a
            // grade yet, and both are named on every beat.
            Assert.Equal(LegState.Pending, session.RevealedLegState(0));
            Assert.Equal(LegState.Pending, session.RevealedLegState(1));
            Assert.True(e.IsSharedTelling);
            Assert.Equal(new[] { 0, 1 }, e.LegIndices.ToArray());
            beforeTheWhistle.Add(e);
        }

        Assert.NotNull(whistle);
        Assert.NotEmpty(beforeTheWhistle); // a telling with no beats before its whistle proves nothing

        // N GRADES AT ONE WHISTLE. Both legs resolve at the same beat, and each to its OWN result.
        Assert.NotEqual(LegState.Pending, session.RevealedLegState(0));
        Assert.NotEqual(LegState.Pending, session.RevealedLegState(1));
        Assert.Equal(ticket.Legs[0].State, session.RevealedLegState(0));
        Assert.Equal(ticket.Legs[1].State, session.RevealedLegState(1));

        // The whistle names both legs and carries each one's own final number.
        Assert.Equal(new[] { 0, 1 }, whistle!.LegIndices.ToArray());
        for (int i = 0; i < 2; i++)
            Assert.Equal(ticket.Legs[i].State == LegState.Won ? 1.0 : 0.0, whistle.LegProbs[i]);
    }

    [Fact]
    public void The_clock_never_regresses_inside_a_telling()
    {
        var (_, ticket, session) = SameMatchPair("shared-telling-0", grantMulligan: false);
        Assert.NotNull(ticket);

        // T135's measurement turned into an assertion: FT then 1' was the defect, and it existed
        // because the same fixture was told twice. One telling, one monotone clock.
        int expectedStep = 1;
        int total = -1;
        int beats = 0;
        while (session.MoveNext(out DramaEvent? e))
        {
            Assert.Equal(0, e!.FixtureIndex); // one fixture, so every beat belongs to it
            Assert.Equal(expectedStep, e.Step);
            if (total < 0) total = e.TotalSteps;
            Assert.Equal(total, e.TotalSteps);
            expectedStep++;
            beats++;
        }

        Assert.True(beats > 1, $"a one-beat telling cannot show a regression either way; got {beats}");
        Assert.Equal(total, beats); // the telling ran its whole clock and stopped there
    }

    // ===================================================================== the window, once

    [Fact]
    public void Both_legs_dead_at_one_whistle_opens_ONE_window_naming_both()
    {
        var found = FindOutcome(grantMulligan: true, wantWon: 0, wantLost: 2);
        Assert.True(found.HasValue, "no seed produced a same-match pair losing BOTH legs");
        var (_, ticket, session) = found!.Value;

        int windows = 0;
        while (session.MoveNext(out DramaEvent? _))
            if (session.HasPendingLoss) { windows++; break; }
        if (session.HasPendingLoss && windows == 0) windows = 1;

        // ONCE PER WHISTLE, after every grade on that fixture has landed (T143) — never once per
        // dead leg, because N windows is N interruptions and that is the rewind's cousin.
        Assert.Equal(1, windows);
        Assert.True(session.HasPendingLoss);

        // It NAMES every leg that died there, in ticket order.
        Assert.Equal(new[] { 0, 1 }, session.PendingDeadLegIndices.ToArray());
        Assert.Equal(0, session.PendingDeadLegIndex); // the legacy scalar is the FIRST of them

        // S85: no single call saves this ticket, and the surface may state that BEFORE the offer.
        Assert.True(session.NoSingleCallSaves);

        // Both grades landed before the window opened — the window is not a substitute for a grade.
        Assert.Equal(LegState.Lost, session.RevealedLegState(0));
        Assert.Equal(LegState.Lost, session.RevealedLegState(1));
        Assert.Equal(0.0, session.TicketWinProbability);
    }

    [Fact]
    public void One_leg_dead_at_the_whistle_is_a_window_a_single_call_CAN_save()
    {
        var found = FindOutcome(grantMulligan: true, wantWon: 1, wantLost: 1);
        Assert.True(found.HasValue, "no seed produced a same-match pair splitting one win, one loss");
        var (_, ticket, session) = found!.Value;

        while (session.MoveNext(out DramaEvent? _))
            if (session.HasPendingLoss) break;

        Assert.True(session.HasPendingLoss);
        Assert.Single(session.PendingDeadLegIndices);

        // The distinction S85 turns on: one death is savable, so the warning must NOT fire here.
        Assert.False(session.NoSingleCallSaves);

        int dead = session.PendingDeadLegIndices[0];
        Assert.Equal(LegState.Lost, ticket.Legs[dead].State);
        Assert.Equal(LegState.Won, ticket.Legs[1 - dead].State); // the other one graded at the same whistle
    }

    [Fact]
    public void A_mulligan_on_the_only_death_saves_the_ticket_and_closes_the_window()
    {
        var found = FindOutcome(grantMulligan: true, wantWon: 1, wantLost: 1);
        Assert.True(found.HasValue, "no seed produced a same-match pair splitting one win, one loss");
        var (run, ticket, session) = found!.Value;

        while (session.MoveNext(out DramaEvent? _))
            if (session.HasPendingLoss) break;
        Assert.True(session.CanMulliganPendingLoss);

        int dead = session.PendingDeadLegIndices[0];
        run.PlayMulliganSlip(session);

        // One death, one call, and the ticket lives: the window closes and the sweat moves on.
        Assert.False(session.HasPendingLoss);
        Assert.True(ticket.Legs[dead].IsVoided);
        Assert.NotEqual(TicketState.Lost, ticket.State);
    }
}
