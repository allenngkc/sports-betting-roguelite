using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// RANDOM — a noise floor between naive and skilled: legal but thoughtless play.
///
/// Policy:
///   • Betting: 1–3 tickets, each 1–4 legs on random distinct matchups and random sides, each staked a
///     random 10–50% of the (shrinking) bank, floored to whole dollars and never below MinStake.
///   • Sweat: steps the sessions and cashes out with 5% probability per event whenever an offer exists.
///   • Shop: half the time, buys one random affordable offer.
///
/// HONESTY: picks sides and matchups at random — it never consults probabilities at all, true or implied.
/// </summary>
public sealed class RandomStrategy : IStrategy
{
    public string Name => "random";
    public bool ControlsSweat => true;

    private const double CashOutChancePerEvent = 0.05;
    private const double ShopBuyChance = 0.5;

    public void Bet(Run run, BotState state, Pcg32 rng)
    {
        int slateCount = run.CurrentSlate.Matchups.Count;
        int tickets = rng.NextInt(1, Math.Min(run.Config.MaxTicketsPerRound, 3) + 1); // 1..3

        for (int t = 0; t < tickets; t++)
        {
            if (run.Bank < run.Config.MinStake) break;

            int maxLegs = Math.Min(4, Math.Min(run.Config.MaxLegs, slateCount));
            int legs = rng.NextInt(1, maxLegs + 1); // 1..4

            var picks = new List<Pick>(legs);
            foreach (int m in DistinctMatchups(rng, slateCount, legs))
                picks.Add(new Pick(m, rng.NextDouble() < 0.5 ? Side.Home : Side.Away));

            double frac = 0.1 + 0.4 * rng.NextDouble(); // 10..50%
            double stake = Math.Max(run.Config.MinStake, Math.Floor(frac * run.Bank));
            if (stake > run.Bank) break;

            run.PlaceTicket(picks, stake);
        }
    }

    public bool ShouldCashOut(Run run, Ticket ticket, SweatSession session, DramaEvent evt,
        double offer, double bankNow, double target, BotState state, Pcg32 rng)
        => rng.NextDouble() < CashOutChancePerEvent;

    public void Shop(Run run, BotState state, Pcg32 rng)
    {
        if (rng.NextDouble() >= ShopBuyChance) return;
        if (run.OwnedRelics.Count >= run.Config.RelicSlots) return;

        var affordable = new List<int>();
        for (int i = 0; i < run.ShopOffers.Count; i++)
            if (run.ShopOffers[i].Price <= run.Bank) affordable.Add(i);
        if (affordable.Count == 0) return;

        run.BuyRelic(affordable[rng.NextInt(0, affordable.Count)]);
    }

    // Partial Fisher-Yates: `count` distinct indices from [0, n).
    private static IEnumerable<int> DistinctMatchups(Pcg32 rng, int n, int count)
    {
        var pool = new int[n];
        for (int i = 0; i < n; i++) pool[i] = i;
        for (int i = 0; i < count; i++)
        {
            int j = rng.NextInt(i, n);
            (pool[i], pool[j]) = (pool[j], pool[i]);
            yield return pool[i];
        }
    }
}
