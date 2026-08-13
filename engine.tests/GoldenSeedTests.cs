using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// The Week 2 determinism pin. Seed GOLDEN-W2, a scripted 2-ticket round (a 3-leg parlay that
/// wins its first leg then dies on its second, plus a winning single — the single keeps the
/// settlement pin sensitive to payout crediting, not just stake deduction). The full event
/// stream, the total count, and settlement are hard-coded below. An UNINTENTIONAL change here is
/// a determinism regression — investigate before re-pinning.
/// </summary>
public class GoldenSeedTests
{
    private const string Seed = "GOLDEN-W2";

    private static Run ScriptedRound()
    {
        // The Week-2 pin was taken at bank 500; the pin stays valid by pinning the config too
        // (outcomes/drama are bank-independent, but the settled-bank assertion is not).
        var run = new Run(Seed, new RunConfig { StartingBank = 500 });
        // F_0.4.0 universe: parlay (0,Away) win, (2,Away) DIES, (3,Home) never sweated.  Single: (1,Away) win.
        run.PlaceTicket(new[] { new Pick(0, Side.Away), new Pick(2, Side.Away), new Pick(3, Side.Home) }, 100);
        run.PlaceTicket(new[] { new Pick(1, Side.Away) }, 50);
        run.LockRound();
        return run;
    }

    private static List<DramaEvent> DrainAll(Run run)
    {
        var all = new List<DramaEvent>();
        foreach (SweatSession s in run.Sweats)
            while (s.MoveNext(out var e))
                all.Add(e);
        return all;
    }

    // (LegIndex, Step, Type, Tag) for every event, in fast-forward order.
    //
    // RE-PINNED TWICE, both times intentionally and both times ONCE:
    //   F_0.4.0 Phase 1 — the stat-line sampler changed the locked market universe.
    //   F_0.5.0 D1, 2026-08-12 — draws (Allen). The winner draw is now a three-way class pick, so
    //     the same seed maps to different outcomes. The SIX-DRAW CONTRACT IS UNCHANGED, which is
    //     why this is a value re-pin and not a structural one: the event COUNT is still 14.
    //
    // THE FIXTURE'S DESIGNED SHAPE SURVIVED, and that was checked rather than hoped for — a re-pin
    // that quietly guts what a pin is sensitive to is worse than a red test. Verified against the
    // new universe: the parlay still WINS its first leg and still DIES on its second (matchup 2
    // now finishes 0-0, so the Away pick loses to a DRAW rather than to a defeat), and the single
    // still WINS, which is what keeps the settlement assertion sensitive to payout crediting
    // rather than to stake deduction alone.
    private static readonly (int leg, int step, DramaEventType type, TensionTag tag)[] Expected =
    {
        (0, 1, DramaEventType.BigPlay, TensionTag.LeadChange),
        (0, 2, DramaEventType.LegFinal, TensionTag.Decisive),
        (1, 1, DramaEventType.Score, TensionTag.Calm),
        (1, 2, DramaEventType.BigPlay, TensionTag.Swing),
        (1, 3, DramaEventType.Momentum, TensionTag.Calm),
        (1, 4, DramaEventType.LegFinal, TensionTag.Decisive),
        (0, 1, DramaEventType.Score, TensionTag.Calm),
        (0, 2, DramaEventType.Momentum, TensionTag.Calm),
        (0, 3, DramaEventType.Momentum, TensionTag.Calm),
        (0, 4, DramaEventType.BigPlay, TensionTag.LeadChange),
        (0, 5, DramaEventType.Momentum, TensionTag.Calm),
        (0, 6, DramaEventType.BigPlay, TensionTag.Swing),
        (0, 7, DramaEventType.BigPlay, TensionTag.Swing),
        (0, 8, DramaEventType.LegFinal, TensionTag.Decisive),
    };

    // WinProbAfter (6 dp) for the first ten events.
    private static readonly double[] ExpectedFirstTenWinProb =
    {
        0.735459, 1.000000, 0.262228, 0.076872, 0.030000,
        0.000000, 0.418843, 0.394183, 0.418359, 0.576159,
    };

    [Fact]
    public void Golden_seed_event_stream_is_pinned()
    {
        Run run = ScriptedRound();
        List<DramaEvent> events = DrainAll(run);

        Assert.Equal(14, events.Count);
        Assert.Equal(Expected.Length, events.Count);

        for (int i = 0; i < events.Count; i++)
        {
            DramaEvent e = events[i];
            Assert.Equal(Expected[i].leg, e.LegIndex);
            Assert.Equal(Expected[i].step, e.Step);
            Assert.Equal(Expected[i].type, e.Type);
            Assert.Equal(Expected[i].tag, e.Tag);
        }

        for (int i = 0; i < ExpectedFirstTenWinProb.Length; i++)
            Assert.Equal(ExpectedFirstTenWinProb[i], events[i].WinProbAfter, 6);
    }

    [Fact]
    public void Golden_seed_settles_to_pinned_bank_and_phase()
    {
        Run run = ScriptedRound();
        DrainAll(run);
        run.FinishSweat();

        Assert.Equal(Phase.Settlement, run.Phase);
        Assert.Equal(TicketState.Lost, run.Tickets[0].State); // parlay died on its second leg
        Assert.Equal(TicketState.Won, run.Tickets[1].State);  // single hit — payout crediting stays pinned
        // Re-pinned for draws (D1, 2026-08-12), 452.559054816409 → below. Same scripted shape.
        Assert.Equal(488.27599673829775, run.Bank, 5);
    }

    [Fact]
    public void Golden_seed_replays_identically()
    {
        List<DramaEvent> a = DrainAll(ScriptedRound());
        List<DramaEvent> b = DrainAll(ScriptedRound());

        Assert.Equal(a.Count, b.Count);
        for (int i = 0; i < a.Count; i++)
        {
            Assert.Equal(a[i].LegIndex, b[i].LegIndex);
            Assert.Equal(a[i].Step, b[i].Step);
            Assert.Equal(a[i].Type, b[i].Type);
            Assert.Equal(a[i].Tag, b[i].Tag);
            Assert.Equal(a[i].WinProbAfter, b[i].WinProbAfter, 15);
        }
    }
}
