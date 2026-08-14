using System;
using System.Threading.Tasks;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// Runs a batch of N independent, fully-seeded runs for one strategy and returns their results.
///
/// Determinism under parallelism: run i uses seed $"{prefix}-{i}" and its own bot generator, so runs
/// share no mutable state. Parallel.For only fills a pre-sized results[i]; every aggregate is reduced
/// sequentially from that array afterward (see Stats/Report), so scheduling can never change a number.
/// </summary>
public static class Harness
{
    public static RunResult[] RunBatch(IStrategy strat, int runs, string seedPrefix, RunConfig cfg,
        string[]? grantedRelics = null, string? grantedConsumable = null)
    {
        var results = new RunResult[runs];
        Parallel.For(0, runs, i =>
        {
            string seed = $"{seedPrefix}-{i}";
            results[i] = RunPlayer.Play(strat, seed, cfg, grantedRelics, grantedConsumable);
        });
        return results;
    }

    public static IStrategy Create(string name) => name switch
    {
        "naive" => new NaiveStrategy(),
        "random" => new RandomStrategy(),
        "skilled" => new SkilledStrategy(),
        "noshop" => new NoShopStrategy(),
        "martyr" => new MartyrStrategy(),
        "samematch" => new SameMatchStrategy(),
        "chalk" => new ChalkGrinderStrategy(),
        "hoarder" => new VipHoarderStrategy(),
        "ironhands" => new IronHandsStrategy(),
        _ => throw new ArgumentException($"Unknown strategy '{name}'"),
    };
}
