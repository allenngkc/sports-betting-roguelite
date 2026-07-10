using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>Relic power audit (§2): skilled with each relic granted free vs baseline skilled.</summary>
public sealed class AuditData
{
    public BatchSummary Baseline = null!;
    public readonly List<Entry> Entries = new();

    public sealed class Entry
    {
        public string RelicId = "";
        public string RelicName = "";
        public double MedianDeath;
        public double WonPct;
        public double MedianDelta;
        public double WonDelta;
        public double MeanDeath;
        public double MeanDelta; // Δ mean rounds survived — the discriminating signal when win% saturates
    }

    public static AuditData Compute(int runs, string seedPrefix, RunConfig cfg, BatchSummary skilledBaseline)
    {
        var audit = new AuditData { Baseline = skilledBaseline };
        var strat = new SkilledStrategy();

        foreach (RelicDefinition def in RelicCatalog.All)
        {
            RunResult[] granted = Harness.RunBatch(strat, runs, seedPrefix, cfg, new[] { def.Id });
            BatchSummary s = BatchSummary.From("skilled+" + def.Id, granted);
            audit.Entries.Add(new Entry
            {
                RelicId = def.Id,
                RelicName = def.Name,
                MedianDeath = s.MedianDeath,
                WonPct = s.WonPct,
                MedianDelta = s.MedianDeath - skilledBaseline.MedianDeath,
                WonDelta = s.WonPct - skilledBaseline.WonPct,
                MeanDeath = s.MeanDeath,
                MeanDelta = s.MeanDeath - skilledBaseline.MeanDeath,
            });
        }

        // Win% and median saturate at the §8 floor, so rank by the finer Δ mean rounds survived.
        audit.Entries.Sort((a, b) => b.MeanDelta.CompareTo(a.MeanDelta));
        return audit;
    }
}

/// <summary>Pairwise relic combo scan (§6): synergy excess over the sum of solo win-rate deltas.</summary>
public sealed class ComboData
{
    public int RunsPerConfig;
    public double BaselineWonPct;
    public readonly Dictionary<string, double> SoloWonPct = new();
    public readonly List<Pair> Pairs = new();

    public sealed class Pair
    {
        public string IdA = "";
        public string IdB = "";
        public double PairWonPct;
        public double SynergyExcess;
    }

    public static ComboData Compute(int runs, string seedPrefix, RunConfig cfg, double baselineWonPct)
    {
        var data = new ComboData { RunsPerConfig = runs, BaselineWonPct = baselineWonPct };
        var strat = new SkilledStrategy();
        IReadOnlyList<RelicDefinition> all = RelicCatalog.All;

        foreach (RelicDefinition def in all)
        {
            RunResult[] r = Harness.RunBatch(strat, runs, seedPrefix, cfg, new[] { def.Id });
            data.SoloWonPct[def.Id] = BatchSummary.From("s", r).WonPct;
        }

        for (int i = 0; i < all.Count; i++)
        for (int j = i + 1; j < all.Count; j++)
        {
            string a = all[i].Id, b = all[j].Id;
            RunResult[] r = Harness.RunBatch(strat, runs, seedPrefix, cfg, new[] { a, b });
            double pairWon = BatchSummary.From("s", r).WonPct;
            double soloDeltaA = data.SoloWonPct[a] - baselineWonPct;
            double soloDeltaB = data.SoloWonPct[b] - baselineWonPct;
            double excess = (pairWon - baselineWonPct) - (soloDeltaA + soloDeltaB);
            data.Pairs.Add(new Pair { IdA = a, IdB = b, PairWonPct = pairWon, SynergyExcess = excess });
        }

        data.Pairs.Sort((x, y) => y.SynergyExcess.CompareTo(x.SynergyExcess));
        return data;
    }
}
