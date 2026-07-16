using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>Item power audit: skilled with each catalog item granted free vs baseline.
/// Passives are granted at run start; consumables are refilled every round (a single-use item
/// granted once would audit as noise). (Timeout was cut at playtest #8 — Allen's verdict matched
/// this audit's ≈0 read.)</summary>
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

        // ---- charm expansion (rev 5 §15): paired-seed arrays + exposure ----
        public bool[] WonFlags = Array.Empty<bool>();
        public int[] DeathRounds = Array.Empty<int>();
        public int Used;               // consumable plays / bobblehead flips in the batch
        public int StatePositiveRuns;  // runs where the ratchet actually wound (stat > 0)
        public double WonDeltaSe;      // paired-seed SE of WonDelta, in pp
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
            WonFlags = s.WonFlags,
            DeathRounds = s.DeathRounds,
            Used = s.ItemTotals.TryGetValue(id, out ItemEvents? e) ? e.Used : 0,
            StatePositiveRuns = s.StatePositiveRuns.TryGetValue(id, out int n) ? n : 0,
            WonDeltaSe = PairedSePp(s.WonFlags, baseline.WonFlags),
        };
    }

    /// <summary>Paired-seed SE of a win-rate delta, in percentage points (rev 5 §15).</summary>
    public static double PairedSePp(bool[] a, bool[] b)
    {
        int n = Math.Min(a.Length, b.Length);
        if (n < 2) return double.PositiveInfinity;
        double mean = 0;
        for (int i = 0; i < n; i++) mean += (a[i] ? 1 : 0) - (b[i] ? 1 : 0);
        mean /= n;
        double ss = 0;
        for (int i = 0; i < n; i++)
        {
            double d = ((a[i] ? 1 : 0) - (b[i] ? 1 : 0)) - mean;
            ss += d * d;
        }
        return 100.0 * Math.Sqrt(ss / (n - 1) / n);
    }

    /// <summary>Paired-seed SE of a mean-death delta (rounds).</summary>
    public static double PairedSeRounds(int[] a, int[] b)
    {
        int n = Math.Min(a.Length, b.Length);
        if (n < 2) return double.PositiveInfinity;
        double mean = 0;
        for (int i = 0; i < n; i++) mean += a[i] - b[i];
        mean /= n;
        double ss = 0;
        for (int i = 0; i < n; i++)
        {
            double d = (a[i] - b[i]) - mean;
            ss += d * d;
        }
        return Math.Sqrt(ss / (n - 1) / n);
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

    /// <summary>Informational notes (declared playtest-gated exemptions) — rendered with the
    /// flags but never blocking.</summary>
    public readonly List<string> Notes = new();

    public static GateData Evaluate(BatchSummary? naive, BatchSummary? skilled, BatchSummary? noshop,
        BatchSummary? martyr, BatchSummary? martyrWorst, AuditData? audit, ComboData? combos)
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
            g.Add("G3", "skilled + items wins: median death ≥5, win 5–8% (re-banded by Allen "
                + "2026-07-15 — the dealt hand's build variance is the roguelite shape)",
                skilled.MedianDeath >= 5.0 && skilled.WonPct >= 5.0 && skilled.WonPct <= 8.0,
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

        // G6 (rev 5 §17): the WORST-CASE granted batch gates (Scar + Jar granted, Free Bet
        // refilled every round); the organic martyr is telemetry beside it.
        BatchSummary? guard = martyrWorst ?? martyr;
        if (guard != null && skilled != null)
            g.Add("G6", "martyr guard (worst case granted): loss-farming win ≤ skilled +2pp",
                guard.WonPct <= skilled.WonPct + 2.0,
                $"{guard.Name} {guard.WonPct:F1}% vs skilled {skilled.WonPct:F1}%"
                + (martyrWorst != null && martyr != null ? $" (organic martyr {martyr.WonPct:F1}%)" : ""));

        if (audit != null)
        {
            // Statistical control (rev 5 §15): paired-seed CIs, Bonferroni z across the whole
            // endpoint family (every DEAD test + the two within-kind dominance contrasts).
            int family = audit.Entries.Count + 2;
            double z = BonferroniZ(0.05, family);

            // Always-on passives trigger by construction; ratchets must show wound-up runs.
            var ratchets = new HashSet<string>
                { "scar_tissue", "chalk_eater", "iron_hands", "bad_beat_jar", "the_system" };

            // PLAYTEST-GATED (declared, HOLDOUT burned → HOLDOUT2): items whose value is human
            // agency a greedy bot cannot monetize. The Manager's redeal audits ≈0 through a bot
            // that buys almost any hand — its worth is choosing. Timeout precedent: the same
            // exemption, then the playtest voted (and cut it). The Manager's vote went the
            // other way: RATIFIED KEEP at playtest #9 (2026-07-15) — the exemption is permanent.
            var playtestGated = new HashSet<string> { "ask_manager" };

            foreach (AuditData.Entry e in audit.Entries)
            {
                if (playtestGated.Contains(e.Id))
                {
                    g.Notes.Add($"PLAYTEST-GATED: {e.Name} audits ≈0 through bots "
                        + $"(Δwon {e.WonDelta:+0.0;-0.0}±{z * e.WonDeltaSe:0.0}pp) — RATIFIED KEEP, playtest #9");
                    continue;
                }
                // Exposure first (rev 5 §15, declared thresholds): an unexercised item's delta
                // is meaningless — UNDEREXPOSED blocks instead of flagging DEAD.
                if (e.IsConsumable && e.Used < MinUses)
                {
                    g.ItemFlags.Add($"UNDEREXPOSED: {e.Name} ({e.Used} uses < {MinUses} — fix the policy, not the item)");
                    continue;
                }
                if (!e.IsConsumable && ratchets.Contains(e.Id) && e.StatePositiveRuns < MinUses)
                {
                    g.ItemFlags.Add($"UNDEREXPOSED: {e.Name} ({e.StatePositiveRuns} wound-up runs < {MinUses})");
                    continue;
                }

                double meanSe = AuditData.PairedSeRounds(e.DeathRounds, audit.Baseline.DeathRounds);
                bool deadWon = e.WonDelta + z * e.WonDeltaSe < 1.0;
                bool deadMean = Math.Abs(e.MeanDelta) + z * meanSe < 0.05;
                if (deadWon && deadMean)
                    g.ItemFlags.Add($"DEAD: {e.Name} (Δwon {e.WonDelta:+0.0;-0.0}±{z * e.WonDeltaSe:0.0}pp, "
                        + $"Δmean {e.MeanDelta:+0.00;-0.00})");
            }

            // Dominance within KIND (rev 5 §15): the explicit contrast best − 2×next, CI lower
            // bound > 0 with a +0.5pp practical margin, winner non-DEAD by construction (its
            // Δwon must exceed 2×next + 0.5 ≥ 0.5, and the DEAD test would contradict it only
            // below 1.0pp — checked explicitly).
            foreach (bool consumables in new[] { false, true })
            {
                AuditData.Entry? best = null, next = null;
                foreach (AuditData.Entry e in audit.Entries)
                {
                    if (e.IsConsumable != consumables) continue;
                    if (best == null || e.WonDelta > best.WonDelta) { next = best; best = e; }
                    else if (next == null || e.WonDelta > next.WonDelta) next = e;
                }
                if (best == null || next == null || next.WonDelta <= 0) continue;

                double contrast = 0, ss = 0;
                int n = Math.Min(Math.Min(best.WonFlags.Length, next.WonFlags.Length),
                    audit.Baseline.WonFlags.Length);
                if (n < 2) continue;
                for (int i = 0; i < n; i++)
                    contrast += (best.WonFlags[i] ? 1 : 0) + (audit.Baseline.WonFlags[i] ? 1 : 0)
                        - 2 * (next.WonFlags[i] ? 1 : 0);
                contrast = 100.0 * contrast / n;
                double meanC = contrast / 100.0;
                for (int i = 0; i < n; i++)
                {
                    double d = (best.WonFlags[i] ? 1 : 0) + (audit.Baseline.WonFlags[i] ? 1 : 0)
                        - 2 * (next.WonFlags[i] ? 1 : 0) - meanC;
                    ss += d * d;
                }
                double se = 100.0 * Math.Sqrt(ss / (n - 1) / n);
                bool winnerAlive = best.WonDelta >= 1.0;
                if (contrast - z * se > 0 && contrast > 0.5 && winnerAlive)
                    g.ItemFlags.Add($"DOMINANT ({(consumables ? "consumable" : "passive")}): {best.Name} "
                        + $"(best−2×next = {contrast:+0.0}±{z * se:0.0}pp)");
            }

            foreach (AuditData.Entry e in audit.Entries)
            {
                if (e.Id != RelicCatalog.TotemId) continue;
                // Fire-rate band on the ORGANIC skilled batch (bought at shop price), not the
                // audit batch — a totem granted free to 100% of runs fires near-always by
                // construction (the artifact ratified around in sim-report-2, now encoded).
                double organicFire = skilled?.TotemFireRate ?? e.TotemFireRate;
                bool healthy = e.MeanDelta >= 0.3 && organicFire >= 25.0 && organicFire <= 60.0;
                if (!healthy)
                    g.ItemFlags.Add($"TOTEM: Δmean {e.MeanDelta:+0.00;-0.00} (want ≥0.3), " +
                        $"organic fire rate {organicFire:F0}% (want 25–60%)");
            }
        }

        return g;
    }

    /// <summary>Declared exposure threshold (PLAN.md rev 5 §15): below it the audit BLOCKS.</summary>
    public const int MinUses = 200;

    /// <summary>Two-sided Bonferroni-corrected z for the family (rational approximation of
    /// Φ⁻¹; exact enough for a flag threshold).</summary>
    public static double BonferroniZ(double alpha, int family)
    {
        double p = 1.0 - alpha / family / 2.0;
        // Beasley-Springer-Moro-ish approximation for the upper tail.
        double t = Math.Sqrt(-2.0 * Math.Log(1.0 - p));
        return t - (2.30753 + 0.27061 * t) / (1.0 + 0.99229 * t + 0.04481 * t * t);
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
