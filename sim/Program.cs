using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// The /sim Monte Carlo harness (PRD F9): plays N seeded runs per strategy bot over the FROZEN engine
/// and emits the markdown balance report that the Week 6 kill/continue verdict will cite.
///
///   dotnet run --project sim -- [--runs N] [--strategy naive|random|skilled|all] [--seed-prefix STR]
///                               [--audit] [--combos N] [--report PATH] [--verify]
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (!CliOptions.TryParse(args, out CliOptions opt, out string? error))
        {
            if (error != "help") Console.Error.WriteLine($"error: {error}");
            Console.Error.WriteLine(Usage);
            return error == "help" ? 0 : 2;
        }

        var cfg = new RunConfig(); // PRD §8 defaults — the sim reports on these, never mutates them

        if (opt.Verify) return Verify(opt, cfg);

        return Run(opt, cfg);
    }

    private static int Run(CliOptions opt, RunConfig cfg)
    {
        var sw = Stopwatch.StartNew();
        var batches = new List<BatchSummary>();
        BatchSummary? skilled = null;
        long totalRuns = 0;

        foreach (string name in opt.SelectedStrategies)
        {
            IStrategy strat = Harness.Create(name);
            RunResult[] results = Harness.RunBatch(strat, opt.Runs, opt.SeedPrefix, cfg);
            totalRuns += opt.Runs;
            BatchSummary summary = BatchSummary.From(name, results);
            batches.Add(summary);
            if (name == "skilled") skilled = summary;
        }

        AuditData? audit = null;
        if (opt.Audit)
        {
            skilled ??= SkilledBaseline(opt, cfg, ref totalRuns);
            audit = AuditData.Compute(opt.Runs, opt.SeedPrefix, cfg, skilled);
            totalRuns += (long)RelicCatalog.All.Count * opt.Runs;
        }

        ComboData? combos = null;
        if (opt.Combos > 0)
        {
            skilled ??= SkilledBaseline(opt, cfg, ref totalRuns);
            double baselineWon = ComboBaseline(opt, cfg, skilled);
            combos = ComboData.Compute(opt.Combos, opt.SeedPrefix, cfg, baselineWon);
            int n = RelicCatalog.All.Count;
            totalRuns += (long)(n + n * (n - 1) / 2) * opt.Combos; // solos + pairs
        }

        sw.Stop();

        string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        string report = Report.Full(opt, batches, audit, combos, date, sw.Elapsed.TotalSeconds, cfg, totalRuns);

        Console.Out.Write(report);
        if (opt.ReportPath != null)
        {
            File.WriteAllText(opt.ReportPath, report);
            Console.Error.WriteLine($"[wrote {opt.ReportPath}]");
        }
        return 0;
    }

    // The combo baseline win% must come from a batch at --combos N (not --runs N) so its deltas line up
    // with the solo/pair batches; recompute if the counts differ.
    private static double ComboBaseline(CliOptions opt, RunConfig cfg, BatchSummary skilledAtRuns)
    {
        if (opt.Combos == opt.Runs) return skilledAtRuns.WonPct;
        RunResult[] r = Harness.RunBatch(new SkilledStrategy(), opt.Combos, opt.SeedPrefix, cfg);
        return BatchSummary.From("skilled", r).WonPct;
    }

    private static BatchSummary SkilledBaseline(CliOptions opt, RunConfig cfg, ref long totalRuns)
    {
        RunResult[] r = Harness.RunBatch(new SkilledStrategy(), opt.Runs, opt.SeedPrefix, cfg);
        totalRuns += opt.Runs;
        return BatchSummary.From("skilled", r);
    }

    // Determinism self-check: two independent 200-run batches per strategy must produce byte-identical
    // report bodies (the header's date/wall-time are excluded by construction).
    private static int Verify(CliOptions opt, RunConfig cfg)
    {
        const int Runs = 200;
        string body1 = VerifyBody(cfg, opt.SeedPrefix, Runs);
        string body2 = VerifyBody(cfg, opt.SeedPrefix, Runs);

        if (body1 == body2)
        {
            Console.WriteLine("VERIFY OK — 200 runs × all strategies reproduced byte-identical aggregates.");
            return 0;
        }

        Console.WriteLine("VERIFY FAILED — aggregate report bodies differ between identical runs.");
        Console.WriteLine(FirstDiff(body1, body2));
        return 1;
    }

    private static string VerifyBody(RunConfig cfg, string seedPrefix, int runs)
    {
        var batches = new List<BatchSummary>();
        foreach (string name in CliOptions.AllStrategies)
        {
            RunResult[] r = Harness.RunBatch(Harness.Create(name), runs, seedPrefix, cfg);
            batches.Add(BatchSummary.From(name, r));
        }
        var opt = new CliOptions { Runs = runs, Strategy = "all", SeedPrefix = seedPrefix };
        return Report.Body(opt, batches, null, null);
    }

    private static string FirstDiff(string a, string b)
    {
        int n = Math.Min(a.Length, b.Length);
        for (int i = 0; i < n; i++)
            if (a[i] != b[i])
                return $"  first difference at offset {i}: '{Snip(a, i)}' vs '{Snip(b, i)}'";
        return $"  lengths differ: {a.Length} vs {b.Length}";
    }

    private static string Snip(string s, int i) => s.Substring(i, Math.Min(40, s.Length - i)).Replace("\n", "\\n");

    private const string Usage =
        "usage: dotnet run --project sim -- [options]\n" +
        "  --runs N              runs per strategy batch (default 10000)\n" +
        "  --strategy S          naive | random | skilled | all (default all)\n" +
        "  --seed-prefix STR     run i uses engine seed \"{STR}-{i}\" (default SIM)\n" +
        "  --audit               add the relic power audit (skilled + each relic granted free)\n" +
        "  --combos N            pairwise relic combo scan, N runs per pair (default off; try 2000)\n" +
        "  --report PATH         also write the markdown report to PATH\n" +
        "  --verify              determinism self-check (200 runs twice), then exit\n";
}
