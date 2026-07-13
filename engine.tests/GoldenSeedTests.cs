using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// The Week 2 determinism pin. Seed GOLDEN-W2, a scripted 2-ticket round (a 3-leg parlay that
/// wins two legs then loses on its decisive final leg, plus a winning single). The full event
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
        // Parlay: (0,Away) win, (2,Away) win, (3,Home) lose-on-final.  Single: (1,Home) win.
        run.PlaceTicket(new[] { new Pick(0, Side.Away), new Pick(2, Side.Away), new Pick(3, Side.Home) }, 100);
        run.PlaceTicket(new[] { new Pick(1, Side.Home) }, 50);
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

    // (LegIndex, Step, Type, Tag) for every one of the 47 events, in fast-forward order.
    private static readonly (int leg, int step, DramaEventType type, TensionTag tag)[] Expected =
    {
        (0, 1, DramaEventType.Score,    TensionTag.Swing),
        (0, 2, DramaEventType.BigPlay,  TensionTag.Swing),
        (0, 3, DramaEventType.Score,    TensionTag.Swing),
        (0, 4, DramaEventType.BigPlay,  TensionTag.NearMiss),
        (0, 5, DramaEventType.LegFinal, TensionTag.Decisive),
        (1, 1, DramaEventType.Score,    TensionTag.Swing),
        (1, 2, DramaEventType.Score,    TensionTag.Swing),
        (1, 3, DramaEventType.Momentum, TensionTag.Calm),
        (1, 4, DramaEventType.Momentum, TensionTag.Calm),
        (1, 5, DramaEventType.BigPlay,  TensionTag.NearMiss),
        (1, 6, DramaEventType.LegFinal, TensionTag.Decisive),
        (2, 1, DramaEventType.Score,    TensionTag.Swing),
        (2, 2, DramaEventType.Score,    TensionTag.Calm),
        (2, 3, DramaEventType.Momentum, TensionTag.Calm),
        (2, 4, DramaEventType.Score,    TensionTag.Swing),
        (2, 5, DramaEventType.Momentum, TensionTag.Calm),
        (2, 6, DramaEventType.Momentum, TensionTag.Calm),
        (2, 7, DramaEventType.Momentum, TensionTag.Calm),
        (2, 8, DramaEventType.Score,    TensionTag.Swing),
        (2, 9, DramaEventType.Momentum, TensionTag.Calm),
        (2, 10, DramaEventType.Score,    TensionTag.Swing),
        (2, 11, DramaEventType.Momentum, TensionTag.Calm),
        (2, 12, DramaEventType.BigPlay,  TensionTag.NearMiss),
        (2, 13, DramaEventType.BigPlay,  TensionTag.LeadChange),
        (2, 14, DramaEventType.Momentum, TensionTag.Calm),
        (2, 15, DramaEventType.Score,    TensionTag.Calm),
        (2, 16, DramaEventType.Momentum, TensionTag.Calm),
        (2, 17, DramaEventType.Momentum, TensionTag.Calm),
        (2, 18, DramaEventType.LegFinal, TensionTag.Decisive),
        (0, 1, DramaEventType.Momentum, TensionTag.Calm),
        (0, 2, DramaEventType.Momentum, TensionTag.Calm),
        (0, 3, DramaEventType.Momentum, TensionTag.Calm),
        (0, 4, DramaEventType.Momentum, TensionTag.Calm),
        (0, 5, DramaEventType.Score,    TensionTag.Calm),
        (0, 6, DramaEventType.Momentum, TensionTag.Calm),
        (0, 7, DramaEventType.Score,    TensionTag.Swing),
        (0, 8, DramaEventType.Momentum, TensionTag.Calm),
        (0, 9, DramaEventType.Momentum, TensionTag.Calm),
        (0, 10, DramaEventType.Momentum, TensionTag.Calm),
        (0, 11, DramaEventType.Momentum, TensionTag.Calm),
        (0, 12, DramaEventType.Momentum, TensionTag.Calm),
        (0, 13, DramaEventType.Momentum, TensionTag.Calm),
        (0, 14, DramaEventType.Score,    TensionTag.Calm),
        (0, 15, DramaEventType.Momentum, TensionTag.Calm),
        (0, 16, DramaEventType.BigPlay,  TensionTag.NearMiss),
        (0, 17, DramaEventType.BigPlay,  TensionTag.LeadChange),
        (0, 18, DramaEventType.LegFinal, TensionTag.Decisive),
    };

    // WinProbAfter (6 dp) for the first ten events.
    private static readonly double[] ExpectedFirstTenWinProb =
    {
        0.664315, 0.843441, 0.970000, 0.250000, 1.000000,
        0.820891, 0.956521, 0.970000, 0.902995, 0.250000,
    };

    [Fact]
    public void Golden_seed_event_stream_is_pinned()
    {
        Run run = ScriptedRound();
        List<DramaEvent> events = DrainAll(run);

        Assert.Equal(47, events.Count);
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
        Assert.Equal(TicketState.Lost, run.Tickets[0].State); // parlay died on its final leg
        Assert.Equal(TicketState.Won, run.Tickets[1].State);  // single hit
        Assert.Equal(428.631019, run.Bank, 5);
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
