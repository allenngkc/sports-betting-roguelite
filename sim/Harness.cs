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
///
/// The WORKER COUNT is a scheduling knob and nothing else: it decides how many of those independent
/// runs are in flight, never which seed produces which result[i]. That is why <see cref="WorkerPolicy"/>
/// may change its answer between batches — including mid-campaign, when Allen sits down or walks away
/// — without any report body moving. The claim is checked, not asserted: a campaign at --workers 1 and
/// the same campaign at a parallel count diff byte-identical below the header.
/// </summary>
public static class Harness
{
    public static RunResult[] RunBatch(IStrategy strat, int runs, string seedPrefix, RunConfig cfg,
        string[]? grantedRelics = null, string? grantedConsumable = null)
    {
        var results = new RunResult[runs];
        // Re-probed here rather than once at startup: RunBatch is the only place every batch in the
        // campaign passes through (strategy roster, martyr-worst, the audit's per-item batches, the
        // combo scan's per-pair batches), so this one line is what makes the policy adapt everywhere.
        var options = new ParallelOptions { MaxDegreeOfParallelism = WorkerPolicy.NextBatch() };
        Parallel.For(0, runs, options, i =>
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
