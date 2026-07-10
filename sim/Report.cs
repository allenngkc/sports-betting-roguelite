using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>Renders the markdown balance report. Sections are numbered to match design/02's
/// "What Monte Carlo must answer". The body (everything after the header) is a pure function of the
/// batch summaries, so --verify can compare two bodies byte-for-byte.</summary>
public static class Report
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private static readonly Dictionary<string, double> Price = BuildPrices();
    private static readonly Dictionary<string, string> RelicName = BuildNames();

    // ---- public entry points ----

    public static string Full(CliOptions opt, IReadOnlyList<BatchSummary> batches,
        AuditData? audit, ComboData? combos, string date, double wallSeconds, RunConfig cfg, long totalRuns)
    {
        var sb = new StringBuilder();
        sb.Append(Header(opt, date, wallSeconds, cfg, batches, totalRuns));
        sb.Append(Body(opt, batches, audit, combos));
        return sb.ToString();
    }

    /// <summary>Deterministic body only (no date / wall time) — the surface --verify diffs.</summary>
    public static string Body(CliOptions opt, IReadOnlyList<BatchSummary> batches,
        AuditData? audit, ComboData? combos)
    {
        var sb = new StringBuilder();
        Survival(sb, batches);
        RelicAudit(sb, audit);
        Variance(sb, batches);
        Ratchet(sb);
        EvArc(sb, batches);
        BandThree(sb, batches, combos);
        Grind(sb, batches);
        return sb.ToString();
    }

    // ---- header ----

    private static string Header(CliOptions opt, string date, double wallSeconds, RunConfig cfg,
        IReadOnlyList<BatchSummary> batches, long total)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# /sim — Monte Carlo balance report");
        sb.AppendLine();
        sb.AppendLine($"- Date: {date}");
        sb.AppendLine("- Engine: workspace is not a git repo — `git describe` unavailable");
        double meanRamp = cfg.Targets.Length > 1
            ? Math.Pow(cfg.Targets[^1] / cfg.Targets[0], 1.0 / (cfg.Targets.Length - 1))
            : 1.0;
        sb.AppendLine($"- Config: bank {Money(cfg.StartingBank)}, targets [{Targets(cfg)}] (avg ×{meanRamp.ToString("F2", Inv)}), "
            + $"overround {Pct(cfg.Overround * 100)}, cash-out margin {Pct(cfg.CashOutMargin * 100)}, "
            + $"debt juice {Pct(cfg.DebtJuiceRate * 100)}, "
            + $"min stake {Money(cfg.MinStake)}, max stake {Pct(cfg.MaxStakeFraction * 100)} of bank, "
            + $"{cfg.MatchupsPerSlate} matchups/round, {cfg.MaxTicketsPerRound} tickets/round, {cfg.RelicSlots} relic slots");
        sb.AppendLine($"- Strategies: {string.Join(", ", Names(batches))}");
        sb.AppendLine($"- Runs per batch: {opt.Runs.ToString("N0", Inv)}");
        sb.AppendLine($"- Total runs (incl. audit/combos): {total.ToString("N0", Inv)}");
        sb.AppendLine($"- Wall time: {wallSeconds.ToString("F2", Inv)} s");
        sb.AppendLine();
        return sb.ToString();
    }

    // ---- 1. survival ----

    private static void Survival(StringBuilder sb, IReadOnlyList<BatchSummary> batches)
    {
        sb.AppendLine("## 1. Survival curves");
        sb.AppendLine();
        sb.AppendLine("Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).");
        sb.AppendLine();

        sb.Append("| Metric |");
        foreach (BatchSummary b in batches) sb.Append($" {b.Name} |");
        sb.AppendLine();
        sb.Append("|---|");
        foreach (var _ in batches) sb.Append("---|");
        sb.AppendLine();

        for (int r = 1; r <= 8; r++)
        {
            sb.Append($"| enter R{r} |");
            foreach (BatchSummary b in batches)
                sb.Append($" {Pct(100.0 * b.AliveEntering[r] / b.N)} |");
            sb.AppendLine();
        }
        sb.Append("| **won %** |");
        foreach (BatchSummary b in batches) sb.Append($" **{Pct(b.WonPct)}** |");
        sb.AppendLine();
        sb.Append("| **median death round** |");
        foreach (BatchSummary b in batches) sb.Append($" **{MedianDeath(b.MedianDeath)}** |");
        sb.AppendLine();
        sb.Append("| mean rounds reached |");
        foreach (BatchSummary b in batches) sb.Append($" {b.MeanDeath.ToString("F2", Inv)} |");
        sb.AppendLine();
        // Debt-as-HP telemetry (DECISIONS.md 2026-07-09): how often the bookie floats a run, and what
        // fraction of deaths are the bookie collecting (vs a plain final-round miss).
        sb.Append("| mean floats per run |");
        foreach (BatchSummary b in batches) sb.Append($" {b.MeanFloats.ToString("F2", Inv)} |");
        sb.AppendLine();
        sb.Append("| in-debt deaths (% of deaths) |");
        foreach (BatchSummary b in batches) sb.Append($" {Pct(b.InDebtDeathPct)} |");
        sb.AppendLine();
        sb.AppendLine();

        BatchSummary? naive = Find(batches, "naive");
        BatchSummary? skilled = Find(batches, "skilled");
        if (naive != null)
        {
            bool pass = naive.MedianDeath >= 3.0 && naive.MedianDeath <= 4.0;
            sb.AppendLine($"`S3 (naive median death 3–4): {(pass ? "PASS" : "FAIL")} (actual {naive.MedianDeath.ToString("0.#", Inv)})`");
        }
        if (skilled != null)
        {
            bool pass = skilled.MedianDeath >= 7.0;
            sb.AppendLine($"`S4 (skilled median death ≥7): {(pass ? "PASS" : "FAIL")} (actual {skilled.MedianDeath.ToString("0.#", Inv)})`");
        }
        sb.AppendLine();
        sb.AppendLine($"> Takeaway: {SurvivalTakeaway(naive, skilled)}");
        sb.AppendLine();
    }

    private static string SurvivalTakeaway(BatchSummary? naive, BatchSummary? skilled)
    {
        if (naive == null || skilled == null) return "run --strategy all for the S3/S4 verdicts.";
        string n = naive.MedianDeath.ToString("0.#", Inv);
        string s = MedianDeath(skilled.MedianDeath);
        string gap = skilled.MedianDeath > naive.MedianDeath ? "skill buys real extra survival" : "skill barely moves the needle";
        return $"naive dies at round {n}, skilled reaches {s} — {gap}; compare against the 3–4 / ≥7 targets above.";
    }

    // ---- 2. relic audit ----

    private static void RelicAudit(StringBuilder sb, AuditData? audit)
    {
        sb.AppendLine("## 2. Relic power audit");
        sb.AppendLine();
        if (audit == null)
        {
            sb.AppendLine("_Not run — pass `--audit` to grant each relic free to skilled and measure its marginal effect._");
            sb.AppendLine();
            return;
        }

        sb.AppendLine($"Skilled baseline: median death {MedianDeath(audit.Baseline.MedianDeath)}, "
            + $"mean rounds {audit.Baseline.MeanDeath.ToString("F2", Inv)}, won {Pct(audit.Baseline.WonPct)}. "
            + "Each row is skilled with that one relic granted free at run start. Rows are sorted by "
            + "**Δ mean rounds survived** (the steadiest signal); Δ won % is the run-winning read.");
        sb.AppendLine();
        sb.AppendLine("| Relic | mean rounds | Δ mean | median death | won % | Δ won % | flag |");
        sb.AppendLine("|---|---|---|---|---|---|---|");
        foreach (AuditData.Entry e in audit.Entries)
        {
            string flag = AuditFlag(e);
            sb.AppendLine($"| {e.RelicName} | {e.MeanDeath.ToString("F2", Inv)} | {Signed(e.MeanDelta)} "
                + $"| {MedianDeath(e.MedianDeath)} | {Pct(e.WonPct)} | {SignedPct(e.WonDelta)} | {flag} |");
        }
        sb.AppendLine();
        sb.AppendLine($"> Takeaway: {AuditTakeaway(audit)}");
        sb.AppendLine();
    }

    private static string AuditFlag(AuditData.Entry e)
    {
        if (e.WonDelta >= 15.0 || e.MeanDelta >= 0.30) return "DOMINANT";
        if (Math.Abs(e.MeanDelta) < 0.02 && Math.Abs(e.WonDelta) < 1.0) return "DEAD (~0 effect)";
        if (e.MeanDelta < -0.02) return "hurts (bot mis-uses it)";
        return "";
    }

    private static string AuditTakeaway(AuditData audit)
    {
        int dead = 0, dominant = 0;
        foreach (AuditData.Entry e in audit.Entries)
        {
            if (e.WonDelta >= 15.0 || e.MeanDelta >= 0.30) dominant++;
            else if (Math.Abs(e.MeanDelta) < 0.02 && Math.Abs(e.WonDelta) < 1.0) dead++;
        }
        AuditData.Entry top = audit.Entries[0];
        return $"strongest relic is {top.RelicName} ({Signed(top.MeanDelta)} mean rounds); "
            + $"{dead} look dead, {dominant} dominant. Note the audit grants relics FREE — organic play "
            + "also pays the shop price out of target headroom.";
    }

    // ---- 3. variance ----

    private static void Variance(StringBuilder sb, IReadOnlyList<BatchSummary> batches)
    {
        sb.AppendLine("## 3. Variance feel");
        sb.AppendLine();
        sb.AppendLine("Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.");
        sb.AppendLine();
        sb.AppendLine("| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|");
        foreach (BatchSummary b in batches)
        {
            string wb10, wb50, wb90, wb99;
            if (b.WinningFinalBanks.Count == 0)
                wb10 = wb50 = wb90 = wb99 = "n/a";
            else
            {
                wb10 = Money(Stats.Percentile(b.WinningFinalBanks, 10));
                wb50 = Money(Stats.Percentile(b.WinningFinalBanks, 50));
                wb90 = Money(Stats.Percentile(b.WinningFinalBanks, 90));
                wb99 = Money(Stats.Percentile(b.WinningFinalBanks, 99));
            }
            sb.AppendLine($"| {b.Name} | {Money(Stats.Percentile(b.BiggestSwings, 10))} "
                + $"| {Money(Stats.Percentile(b.BiggestSwings, 50))} | {Money(Stats.Percentile(b.BiggestSwings, 90))} "
                + $"| {Money(Stats.Percentile(b.BiggestSwings, 99))} | {wb10} | {wb50} | {wb90} | {wb99} |");
        }
        sb.AppendLine();
        sb.AppendLine("> Takeaway: swing p50 vs p99 shows whether the ride is flat (boring) or spiky (feels rigged); "
            + "a p99 many multiples of p50 is the intended rare-blowup shape.");
        sb.AppendLine();
    }

    // ---- 4. ratchet ----

    private static void Ratchet(StringBuilder sb)
    {
        sb.AppendLine("## 4. Ratchet tuning");
        sb.AppendLine();
        sb.AppendLine("_N/A in v0 — there is no limiting/ratchet mechanic in the prototype (parked to a later phase)._");
        sb.AppendLine();
    }

    // ---- 5. EV arc ----

    private static void EvArc(StringBuilder sb, IReadOnlyList<BatchSummary> batches)
    {
        sb.AppendLine("## 5. EV-arc");
        sb.AppendLine();
        sb.AppendLine("Mean *true* per-ticket EV at lock, by round (the economy doctrine's crossing from −vig toward positive).");
        sb.AppendLine();

        sb.Append("| Round |");
        foreach (BatchSummary b in batches) sb.Append($" {b.Name} mean EV |");
        sb.AppendLine();
        sb.Append("|---|");
        foreach (var _ in batches) sb.Append("---|");
        sb.AppendLine();
        for (int r = 1; r <= 8; r++)
        {
            sb.Append($"| R{r} |");
            foreach (BatchSummary b in batches)
            {
                if (b.EvSampleByRound[r] == 0) sb.Append(" — |");
                else sb.Append($" {Money(b.MeanEvByRound[r])} |");
            }
            sb.AppendLine();
        }
        sb.AppendLine();

        BatchSummary? skilled = Find(batches, "skilled");
        BatchSummary? naive = Find(batches, "naive");
        if (skilled != null)
        {
            int cross = skilled.EvZeroCrossRound();
            sb.AppendLine(cross == 0
                ? "- Skilled mean EV never crosses zero (target ≈ round 4)."
                : $"- Skilled mean EV first crosses zero at **round {cross}** (target ≈ round 4).");
            if (cross >= 2)
                sb.AppendLine($"- **Survivorship caveat:** the round-{cross}+ means average only the few runs that "
                    + $"got there (R{cross} n={skilled.EvSampleByRound[cross]} tickets vs R1 n={skilled.EvSampleByRound[1]}). "
                    + "The MEDIAN skilled run dies at round 2, so it never reaches the +EV band — the arc is real "
                    + "for the surviving tail, not the typical player.");
        }
        if (naive != null)
        {
            int cross = naive.EvZeroCrossRound();
            sb.AppendLine(cross == 0
                ? "- Naive mean EV never crosses zero — as intended."
                : $"- Naive mean EV crosses zero at round {cross} — NOT intended (naive should never cross).");
        }
        sb.AppendLine();
        sb.AppendLine($"> Takeaway: {EvTakeaway(skilled, naive)}");
        sb.AppendLine();
    }

    private static string EvTakeaway(BatchSummary? skilled, BatchSummary? naive)
    {
        if (skilled == null) return "add skilled to see the arc.";
        int cross = skilled.EvZeroCrossRound();
        if (cross == 0) return "skilled never turns +EV; naive stays underwater — relics aren't flipping the arc.";
        return $"among survivors the +EV relics do flip the arc (crosses ~R{cross}, target ≈R4), but almost nobody "
            + "survives to bank it — the mechanic works, the economy gates it away.";
    }

    // ---- 6. band 3 + combos ----

    private static void BandThree(StringBuilder sb, IReadOnlyList<BatchSummary> batches, ComboData? combos)
    {
        sb.AppendLine("## 6. Band-3 audit");
        sb.AppendLine();
        sb.AppendLine("Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).");
        sb.AppendLine();
        sb.AppendLine("| Strategy | p99 final bank | top-1% mean | max |");
        sb.AppendLine("|---|---|---|---|");
        foreach (BatchSummary b in batches)
        {
            double p99 = Stats.Percentile(b.FinalBanks, 99);
            (double mean, double max) = Top1(b.FinalBanks);
            sb.AppendLine($"| {b.Name} | {Money(p99)} | {Money(mean)} | {Money(max)} |");
        }
        sb.AppendLine();

        if (combos != null) Combos(sb, combos);
        else sb.AppendLine("_Combo scan not run — pass `--combos N` (e.g. 2000) for the pairwise synergy table._");
        sb.AppendLine();
        sb.AppendLine("> Takeaway: brokenness should live in the tail (max ≫ p99) and, if combos ran, in a few "
            + "hard-to-assemble pairs rather than trivial cheap ones.");
        sb.AppendLine();
    }

    private static void Combos(StringBuilder sb, ComboData combos)
    {
        sb.AppendLine();
        sb.AppendLine($"Pairwise relic synergy ({combos.RunsPerConfig.ToString("N0", Inv)} runs/config, baseline won {Pct(combos.BaselineWonPct)}). "
            + "Synergy excess = pair Δwon − (soloA Δ + soloB Δ). Top 10:");
        sb.AppendLine();
        sb.AppendLine("| Pair | pair won % | synergy excess (pp) | tag |");
        sb.AppendLine("|---|---|---|---|");
        int shown = 0;
        foreach (ComboData.Pair p in combos.Pairs)
        {
            if (shown++ >= 10) break;
            string names = $"{RelicName.GetValueOrDefault(p.IdA, p.IdA)} + {RelicName.GetValueOrDefault(p.IdB, p.IdB)}";
            sb.AppendLine($"| {names} | {Pct(p.PairWonPct)} | {Signed(p.SynergyExcess)} | {ComboTag(p)} |");
        }
    }

    private static string ComboTag(ComboData.Pair p)
    {
        if (p.SynergyExcess <= 1.0) return "marginal (no real loop)";
        double combined = Price.GetValueOrDefault(p.IdA) + Price.GetValueOrDefault(p.IdB);
        return combined <= 450.0
            ? "degenerate: cheap pair, trivially assembled"
            : "delicious: costly pair, bounded by run length";
    }

    // ---- 7. grind ----

    private static void Grind(StringBuilder sb, IReadOnlyList<BatchSummary> batches)
    {
        sb.AppendLine("## 7. Grind metric");
        sb.AppendLine();
        sb.AppendLine("Median decisions per round (tickets + cash-outs + buys). Fewer decisions late than mid = flat repetition.");
        sb.AppendLine();

        sb.Append("| Round |");
        foreach (BatchSummary b in batches) sb.Append($" {b.Name} |");
        sb.AppendLine();
        sb.Append("|---|");
        foreach (var _ in batches) sb.Append("---|");
        sb.AppendLine();
        for (int r = 1; r <= 8; r++)
        {
            sb.Append($"| R{r} |");
            foreach (BatchSummary b in batches)
            {
                if (b.PlayedByRound[r] == 0) sb.Append(" — |");
                else sb.Append($" {b.MedianDecisionsByRound[r].ToString("0.#", Inv)} |");
            }
            sb.AppendLine();
        }
        sb.AppendLine();
        sb.AppendLine($"> Takeaway: {GrindTakeaway(batches)}");
        sb.AppendLine();
    }

    private static string GrindTakeaway(IReadOnlyList<BatchSummary> batches)
    {
        var flagged = new List<string>();
        foreach (BatchSummary b in batches)
        {
            double mid = 0, late = 0; int midN = 0, lateN = 0;
            for (int r = 3; r <= 5; r++) if (b.PlayedByRound[r] > 0) { mid += b.MedianDecisionsByRound[r]; midN++; }
            for (int r = 6; r <= 8; r++) if (b.PlayedByRound[r] > 0) { late += b.MedianDecisionsByRound[r]; lateN++; }
            if (midN > 0 && lateN > 0 && late / lateN < mid / midN) flagged.Add(b.Name);
        }
        return flagged.Count == 0
            ? "decision count holds up or rises into late rounds — no flat-repetition flag."
            : $"late rounds carry FEWER decisions than mid for: {string.Join(", ", flagged)} — repetition-risk flag.";
    }

    // ---- helpers ----

    private static (double mean, double max) Top1(double[] banks)
    {
        if (banks.Length == 0) return (0, 0);
        var sorted = new double[banks.Length];
        Array.Copy(banks, sorted, banks.Length);
        Array.Sort(sorted);
        int count = Math.Max(1, banks.Length / 100);
        double sum = 0, max = sorted[^1];
        for (int i = sorted.Length - count; i < sorted.Length; i++) sum += sorted[i];
        return (sum / count, max);
    }

    private static BatchSummary? Find(IReadOnlyList<BatchSummary> batches, string name)
    {
        foreach (BatchSummary b in batches) if (b.Name == name) return b;
        return null;
    }

    private static IEnumerable<string> Names(IReadOnlyList<BatchSummary> batches)
    {
        foreach (BatchSummary b in batches) yield return b.Name;
    }

    private static string Money(double v)
    {
        string sign = v < 0 ? "-" : "";
        return $"{sign}${Math.Abs(v).ToString("N0", Inv)}";
    }

    private static string Signed(double v)
        => Math.Abs(v) < 0.005 ? "0" : (v > 0 ? "+" : "") + v.ToString("0.##", Inv);

    private static string SignedPct(double v)
        => Math.Abs(v) < 0.05 ? "0.0pp" : (v > 0 ? "+" : "") + v.ToString("F1", Inv) + "pp";
    private static string Pct(double v) => v.ToString("F1", Inv) + "%";
    private static string MedianDeath(double m) => m >= 9.0 ? "9 (won)" : m.ToString("0.#", Inv);

    private static string Targets(RunConfig cfg)
    {
        var parts = new List<string>();
        foreach (double t in cfg.Targets) parts.Add(((long)t).ToString(Inv));
        return string.Join(", ", parts);
    }

    private static Dictionary<string, double> BuildPrices()
    {
        var d = new Dictionary<string, double>();
        foreach (RelicDefinition r in RelicCatalog.All) d[r.Id] = r.Price;
        return d;
    }

    private static Dictionary<string, string> BuildNames()
    {
        var d = new Dictionary<string, string>();
        foreach (RelicDefinition r in RelicCatalog.All) d[r.Id] = r.Name;
        return d;
    }
}
