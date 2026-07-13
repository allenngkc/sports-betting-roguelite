using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>Item power audit: skilled with each of the SIX items granted free vs baseline.
/// Passives are granted at run start; consumables are refilled every round (a single-use item
/// granted once would audit as noise). Timeout is included for the non-degeneracy read but is
/// EXEMPT from the DEAD flag — bots never play it (playtest-gated, PLAN.md).</summary>
public sealed class AuditData
{
    public BatchSummary Baseline = null!;
    public readonly List<Entry> Entries = new();

    public sealed class Entry
    {
        public string Id = "";
        public string Name = "";
        public bool IsConsumable;
        public double MedianDeath;
        public double WonPct;
        public double MedianDelta;
        public double WonDelta;
        public double MeanDeath;
        public double MeanDelta;      // Δ mean rounds survived — the discriminating signal
        public double TotemFireRate;  // only meaningful for the totem row
    }

    public static AuditData Compute(int runs, string seedPrefix, RunConfig cfg, BatchSummary skilledBaseline)
    {
        var audit = new AuditData { Baseline = skilledBaseline };
        var strat = new SkilledStrategy();

        foreach (RelicDefinition def in RelicCatalog.All)
            audit.Entries.Add(Entry_(strat, runs, seedPrefix, cfg, skilledBaseline,
                def.Id, def.Name, isConsumable: false));

        foreach (ConsumableDefinition def in RelicCatalog.Consumables)
            audit.Entries.Add(Entry_(strat, runs, seedPrefix, cfg, skilledBaseline,
                def.Id, def.Name, isConsumable: true));

        audit.Entries.Sort((a, b) => b.MeanDelta.CompareTo(a.MeanDelta));
        return audit;
    }

    private static Entry Entry_(SkilledStrategy strat, int runs, string seedPrefix, RunConfig cfg,
        BatchSummary baseline, string id, string name, bool isConsumable)
    {
        RunResult[] granted = isConsumable
            ? Harness.RunBatch(strat, runs, seedPrefix, cfg, grantedConsumable: id)
            : Harness.RunBatch(strat, runs, seedPrefix, cfg, new[] { id });
        BatchSummary s = BatchSummary.From("skilled+" + id, granted);
        return new Entry
        {
            Id = id,
            Name = name,
            IsConsumable = isConsumable,
            MedianDeath = s.MedianDeath,
            WonPct = s.WonPct,
            MedianDelta = s.MedianDeath - baseline.MedianDeath,
            WonDelta = s.WonPct - baseline.WonPct,
            MeanDeath = s.MeanDeath,
            MeanDelta = s.MeanDeath - baseline.MeanDeath,
            TotemFireRate = s.TotemFireRate,
        };
    }
}

/// <summary>Pairwise passive combo scan: synergy excess over the sum of solo win-rate deltas.
/// With 3 passives this is 3 pairs — and the Multiplier+Scar pair is gate G5's superadditivity
/// evidence (composition through the shared PayoutMultiplier product).</summary>
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

    public Pair? Find(string a, string b)
    {
        foreach (Pair p in Pairs)
            if ((p.IdA == a && p.IdB == b) || (p.IdA == b && p.IdB == a)) return p;
        return null;
    }

    public static ComboData Compute(int runs, string seedPrefix, RunConfig cfg)
    {
        // G5 measurement (Allen-approved round-1 fix): the fixed-discipline bot removes the
        // ownership-changes-behavior confound, so pair-vs-solo deltas read pure composition.
        // The baseline is recomputed with the same bot for a like-for-like comparison.
        var strat = new FixedDisciplineStrategy();
        double fixedBaseline = BatchSummary.From("f",
            Harness.RunBatch(strat, runs, seedPrefix, cfg)).WonPct;
        var data = new ComboData { RunsPerConfig = runs, BaselineWonPct = fixedBaseline };
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
            double soloDeltaA = data.SoloWonPct[a] - fixedBaseline;
            double soloDeltaB = data.SoloWonPct[b] - fixedBaseline;
            double excess = (pairWon - fixedBaseline) - (soloDeltaA + soloDeltaB);
            data.Pairs.Add(new Pair { IdA = a, IdB = b, PairWonPct = pairWon, SynergyExcess = excess });
        }

        data.Pairs.Sort((x, y) => y.SynergyExcess.CompareTo(x.SynergyExcess));
        return data;
    }
}

/// <summary>The gate table (PLAN.md, Allen-approved): the acceptance criteria of the economy
/// rework's sim campaign, computed from the batches + audit + combo data.</summary>
public sealed class GateData
{
    public sealed class Gate
    {
        public string Id = "";
        public string Description = "";
        public bool Pass;
        public string Actual = "";
    }

    public readonly List<Gate> Gates = new();
    public readonly List<string> ItemFlags = new();

    public static GateData Evaluate(BatchSummary? naive, BatchSummary? skilled, BatchSummary? noshop,
        BatchSummary? martyr, AuditData? audit, ComboData? combos)
    {
        var g = new GateData();

        // Bands re-ratified by Allen 2026-07-13 (design/10 F) after campaign round 1.
        if (naive != null)
            g.Add("G1", "honest gambling: naive win <1%, dies before the cliff resolves (median ≤6)",
                naive.MedianDeath <= 6.0 && naive.WonPct < 1.0,
                $"median {naive.MedianDeath:0.#}, won {naive.WonPct:F1}%");

        if (noshop != null)
            g.Add("G2", "engine mandatory: no-shop skilled win <2%, median death 5–6",
                noshop.MedianDeath >= 5.0 && noshop.MedianDeath <= 6.0 && noshop.WonPct < 2.0,
                $"median {noshop.MedianDeath:0.#}, won {noshop.WonPct:F1}%");

        if (skilled != null)
            g.Add("G3", "skilled + items wins: median death ≥6, win 5–8% (Allen's final-product band)",
                skilled.MedianDeath >= 6.0 && skilled.WonPct >= 5.0 && skilled.WonPct <= 8.0,
                $"median {skilled.MedianDeath:0.#}, won {skilled.WonPct:F1}%");

        if (skilled != null)
        {
            int cross = skilled.EvZeroCrossRound();
            g.Add("G4", "the EV arc exists: skilled mean ticket EV crosses zero in rounds 3–7",
                cross >= 3 && cross <= 7,
                cross == 0 ? "never crosses" : $"crosses at R{cross}");
        }

        if (combos != null && audit != null)
        {
            ComboData.Pair? pair = combos.Find(RelicCatalog.MultiplierId, RelicCatalog.ScarTissueId);
            if (pair != null)
                g.Add("G5", "composition superadditive: Multiplier+Scar pair Δwin > sum of solo Δwins",
                    pair.SynergyExcess > 0.0,
                    $"synergy excess {pair.SynergyExcess:+0.0;-0.0}pp");
        }

        if (martyr != null && skilled != null)
            g.Add("G6", "martyr guard: scar-farming bot win ≤ skilled +2pp",
                martyr.WonPct <= skilled.WonPct + 2.0,
                $"martyr {martyr.WonPct:F1}% vs skilled {skilled.WonPct:F1}%");

        if (audit != null)
        {
            foreach (AuditData.Entry e in audit.Entries)
            {
                if (e.Id == "timeout") continue; // playtest-gated: bots never play it
                if (e.WonDelta < 1.0 && Math.Abs(e.MeanDelta) < 0.05)
                    g.ItemFlags.Add($"DEAD: {e.Name} (Δwon {e.WonDelta:+0.0;-0.0}pp, Δmean {e.MeanDelta:+0.00;-0.00})");
            }
            // Dominance: no item's Δwon more than 2× the next best positive.
            double best = double.MinValue, second = double.MinValue;
            string bestName = "";
            foreach (AuditData.Entry e in audit.Entries)
            {
                if (e.Id == "timeout") continue;
                if (e.WonDelta > best) { second = best; best = e.WonDelta; bestName = e.Name; }
                else if (e.WonDelta > second) second = e.WonDelta;
            }
            if (second > 0 && best > 2.0 * second)
                g.ItemFlags.Add($"DOMINANT: {bestName} (Δwon {best:F1}pp > 2× next {second:F1}pp)");

            foreach (AuditData.Entry e in audit.Entries)
            {
                if (e.Id != RelicCatalog.TotemId) continue;
                bool healthy = e.MeanDelta >= 0.3 && e.TotemFireRate >= 25.0 && e.TotemFireRate <= 60.0;
                if (!healthy)
                    g.ItemFlags.Add($"TOTEM: Δmean {e.MeanDelta:+0.00;-0.00} (want ≥0.3), " +
                        $"fire rate {e.TotemFireRate:F0}% (want 25–60%)");
            }
        }

        return g;
    }

    private void Add(string id, string desc, bool pass, string actual)
        => Gates.Add(new Gate { Id = id, Description = desc, Pass = pass, Actual = actual });

    public bool AllPass
    {
        get
        {
            foreach (Gate gate in Gates) if (!gate.Pass) return false;
            return Gates.Count > 0;
        }
    }
}
