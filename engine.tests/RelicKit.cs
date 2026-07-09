using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// Shared setup for the Week 3 relic tests. Relics are only acquirable through the shop, so these
/// helpers play round 1 with no bets, buy the requested relics from that shop, and hand back a run
/// sitting in round 2's Betting phase. Seeds were found by scanning (see the report) so that round 1
/// offers exactly the relics each test needs. Since the tester picks each leg's side, any leg can be
/// made to win (pick the result side) or lose (pick the opposite), so outcomes need no seed hunt —
/// only the round-2 results, which <see cref="Round2Results"/> reveals via an abstainer clone.
/// </summary>
internal static class RelicKit
{
    // Cheap targets so round 1 always clears into the shop; roomy bank so several relics are affordable.
    public static RunConfig Cheap(double bank = 2000) => new RunConfig
    {
        StartingBank = bank,
        Targets = Enumerable.Repeat(1.0, 12).ToArray(),
    };

    /// <summary>Round 1, no bets, settled into the shop (Phase.Shop).</summary>
    public static Run Round1ToShop(string seed, RunConfig cfg)
    {
        var run = new Run(seed, cfg);
        run.LockRound();
        run.FastForwardRound();
        run.Settle();
        Assert.Equal(Phase.Shop, run.Phase);
        return run;
    }

    /// <summary>Round 1 shop, buy the given relics (in the given order → acquisition order), then round 2 Betting.</summary>
    public static Run Round2Owning(string seed, RunConfig cfg, params string[] relicIds)
    {
        Run run = Round1ToShop(seed, cfg);
        foreach (string id in relicIds)
            run.BuyRelic(OfferIndex(run, id));
        run.ExitShop();
        Assert.Equal(Phase.Betting, run.Phase);
        Assert.Equal(2, run.Round);
        return run;
    }

    public static int OfferIndex(Run run, string id)
    {
        for (int i = 0; i < run.ShopOffers.Count; i++)
            if (run.ShopOffers[i].Id == id) return i;
        throw new System.InvalidOperationException(
            $"Shop did not offer '{id}': [{string.Join(",", run.ShopOffers.Select(o => o.Id))}]");
    }

    /// <summary>Results at the given round (abstainer clone, no bets), so tests can pick winning/losing
    /// sides. Relics never touch the Outcomes/Slate streams, so these hold for a run owning any relics.</summary>
    public static Side[] RoundResults(string seed, RunConfig cfg, int round)
    {
        var run = new Run(seed, cfg);
        for (int r = 1; r < round; r++)
        {
            run.LockRound(); run.FastForwardRound(); run.Settle(); run.ExitShop();
        }
        run.LockRound();
        return run.CurrentSlate.Matchups.Select(m => m.Result!.Value).ToArray();
    }

    public static Side Win(Side[] results, int matchup) => results[matchup];
    public static Side Lose(Side[] results, int matchup) => results[matchup] == Side.Home ? Side.Away : Side.Home;

    public static Pick[] Picks(params (int m, Side s)[] p) => p.Select(x => new Pick(x.m, x.s)).ToArray();
}
