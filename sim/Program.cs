using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// The /sim Monte Carlo harness: plays N seeded runs per strategy bot and emits the markdown
/// balance report. The economy rework (PLAN.md 2026-07-13) added the GATE CAMPAIGN (--gates:
/// G1–G6 + item flags, the milestone's acceptance criteria) and the payment-curve GRID (--grid:
/// growth × P1 cells, gates-lite per cell — how the curve gets chosen). SCORER CALIBRATION
/// (--scorer-ev, sim/ScorerCalibration.cs) is bot-independent — no strategy ever prices Anytime
/// Scorer, so it runs standalone instead of joining a strategy batch.
///
///   dotnet run --project sim -- [--runs N] [--strategy naive|random|skilled|noshop|martyr|all]
///       [--seed-prefix STR] [--audit] [--combos N] [--gates] [--grid] [--scorer-ev]
///       [--report PATH] [--verify]
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

        var cfg = new RunConfig(); // rework defaults — the sim reports on these, never mutates them

        if (opt.Verify) return Verify(opt, cfg);
        if (opt.ScorerEv) return ScorerEv(opt, cfg);
        if (opt.Grid) return Grid(opt);

        return Run(opt, cfg);
    }

    private static int Run(CliOptions opt, RunConfig cfg)
    {
        var sw = Stopwatch.StartNew();
        var batches = new List<BatchSummary>();
        var byName = new Dictionary<string, BatchSummary>();
        long totalRuns = 0;

        IEnumerable<string> strategyNames = opt.Gates
            ? new[] { "naive", "random", "skilled", "noshop", "martyr", "chalk", "hoarder", "ironhands" }
            : opt.SelectedStrategies; // gates roster: the gate bots + archetype telemetry

        foreach (string name in strategyNames)
        {
            IStrategy strat = Harness.Create(name);
            RunResult[] results = Harness.RunBatch(strat, opt.Runs, opt.SeedPrefix, cfg);
            totalRuns += opt.Runs;
            BatchSummary summary = BatchSummary.From(name, results);
            batches.Add(summary);
            byName[name] = summary;
        }

        // G6's actual gate (rev 5 §17): the WORST-CASE loss-farmer — Scar + Jar granted free,
        // a Free Bet refilled every round. The organic martyr above is telemetry beside it.
        BatchSummary? martyrWorst = null;
        if (opt.Gates)
        {
            RunResult[] worst = Harness.RunBatch(new MartyrStrategy(), opt.Runs, opt.SeedPrefix, cfg,
                new[] { RelicCatalog.ScarTissueId, "bad_beat_jar" }, "free_bet");
            totalRuns += opt.Runs;
            martyrWorst = BatchSummary.From("martyr-worst", worst);
            batches.Add(martyrWorst);
            byName["martyr-worst"] = martyrWorst;
        }

        AuditData? audit = null;
        if (opt.Audit || opt.Gates)
        {
            BatchSummary skilled = byName.TryGetValue("skilled", out var s)
                ? s : SkilledBaseline(opt, cfg, ref totalRuns);
            audit = AuditData.Compute(opt.Runs, opt.SeedPrefix, cfg, skilled);
            totalRuns += (long)(RelicCatalog.All.Count + RelicCatalog.Consumables.Count) * opt.Runs;
        }

        ComboData? combos = null;
        int comboRuns = opt.Gates && opt.Combos == 0 ? opt.Runs : opt.Combos;
        if (comboRuns > 0)
        {
            combos = ComboData.Compute(comboRuns, opt.SeedPrefix, cfg); // fixed-discipline bot inside
            int n = RelicCatalog.All.Count;
            totalRuns += (long)(1 + n + n * (n - 1) / 2) * comboRuns;
        }

        GateData? gates = null;
        if (opt.Gates)
        {
            gates = GateData.Evaluate(
                byName.GetValueOrDefault("naive"), byName.GetValueOrDefault("skilled"),
                byName.GetValueOrDefault("noshop"), byName.GetValueOrDefault("martyr"),
                martyrWorst, audit, combos);
        }

        sw.Stop();

        string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        string report = Report.Full(opt, batches, audit, combos, gates, date, sw.Elapsed.TotalSeconds, cfg, totalRuns);

        Console.Out.Write(report);
        if (opt.ReportPath != null)
        {
            File.WriteAllText(opt.ReportPath, report);
            Console.Error.WriteLine($"[wrote {opt.ReportPath}]");
        }
        // Campaign exit contract (rev 5): DONE requires every gate AND a clean flag sheet —
        // UNDEREXPOSED and item flags block exactly like a failed gate.
        return gates != null && (!gates.AllPass || gates.ItemFlags.Count > 0) ? 1 : 0;
    }

    // The payment-curve grid: growth × P1 cells, G1–G4 per cell (G5/G6 need the audit/combo cost —
    // they run on the chosen cell via --gates). Payments rounded to $5 for readability.
    private static int Grid(CliOptions opt)
    {
        // Two-phase (convex) curves — the CloverPit shape: gentle through R4, cliff after.
        // The constant-ratio family could not hold median death ≥7 AND win 10–15% at once.
        double[] banks = { 250, 300, 350 };
        double[] firsts = { 60, 70, 80 };
        double[] earlies = { 1.20 };
        double[] lates = { 1.75, 1.90 };
        int rounds = new RunConfig().Payments.Length;

        Console.WriteLine($"# payment-curve grid — {opt.Runs:N0} runs/bot/cell, bots: naive, skilled, noshop");
        Console.WriteLine();
        Console.WriteLine("| bank | P1 | early | late | P8 | G1 naive | G2 noshop | G3 skilled | G4 EV cross | verdict |");
        Console.WriteLine("|---|---|---|---|---|---|---|---|---|---|");

        foreach (double bank in banks)
        foreach (double p1 in firsts)
        foreach (double g1 in earlies)
        foreach (double g2 in lates)
        {
            var payments = new double[rounds];
            double p = p1;
            for (int i = 0; i < rounds; i++)
            {
                payments[i] = Math.Round(p / 5.0) * 5.0;
                p *= i < 3 ? g1 : g2; // R1–R4 gentle, R5–R8 the cliff
            }
            var cfg = new RunConfig { Payments = payments, StartingBank = bank };

            BatchSummary naive = BatchSummary.From("naive",
                Harness.RunBatch(new NaiveStrategy(), opt.Runs, opt.SeedPrefix, cfg));
            BatchSummary skilled = BatchSummary.From("skilled",
                Harness.RunBatch(new SkilledStrategy(), opt.Runs, opt.SeedPrefix, cfg));
            BatchSummary noshop = BatchSummary.From("noshop",
                Harness.RunBatch(new NoShopStrategy(), opt.Runs, opt.SeedPrefix, cfg));

            GateData gates = GateData.Evaluate(naive, skilled, noshop, null, null, null, null);
            int pass = 0;
            var cells = new List<string>();
            foreach (GateData.Gate gate in gates.Gates)
            {
                cells.Add($"{(gate.Pass ? "PASS" : "fail")} ({gate.Actual})");
                if (gate.Pass) pass++;
            }
            while (cells.Count < 4) cells.Add("—");

            string verdict = pass == gates.Gates.Count && gates.Gates.Count == 4 ? "**CANDIDATE**" : $"{pass}/4";
            Console.WriteLine($"| {bank:0} | {p1:0} | ×{g1:0.00} | ×{g2:0.00} | {payments[^1]:N0} | {cells[0]} | {cells[1]} | {cells[2]} | {cells[3]} | {verdict} |");
        }

        Console.WriteLine();
        Console.WriteLine("Run the full campaign on a CANDIDATE cell: set RunConfig.Payments, then `--gates --runs 10000`.");
        return 0;
    }

    // The scorer calibration mode (sim/ScorerCalibration.cs): bot-independent by construction, so
    // it never joins --gates/--audit/--combos or the default strategy batch — those all need a
    // bot, and AnytimeScorer has none to give them (see the file's own header for why). No
    // date/wall-time is printed here (unlike Header() below), so the whole stdout stays byte-
    // identical run to run — the determinism check has nothing else to diff against.
    private static int ScorerEv(CliOptions opt, RunConfig cfg)
    {
        ScorerCalibrationData data = ScorerCalibrationData.Compute(opt.Runs, opt.SeedPrefix, cfg);
        int offersPerMatchup = cfg.PlayersPerTeam * 2;
        Console.WriteLine($"# scorer calibration — --scorer-ev, {opt.Runs:N0} slates x {cfg.MatchupsPerSlate} "
            + $"matchups x {offersPerMatchup} scorer offers, {ScorerCalibrationData.SamplesPerMatchup} "
            + $"resamples/matchup, seed prefix \"{opt.SeedPrefix}\"");
        Console.WriteLine();
        Console.Out.Write(Report.ScorerEv(data, cfg));
        return 0;
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
        return Report.Body(opt, batches, null, null, null);
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
        "  --strategy S          naive | random | skilled | noshop | martyr | all (default all)\n" +
        "  --seed-prefix STR     run i uses engine seed \"{STR}-{i}\" (default SIM)\n" +
        "  --audit               the six-item power audit (each granted free to skilled)\n" +
        "  --combos N            pairwise passive combo scan, N runs per pair\n" +
        "  --gates               the FULL gate campaign: G1-G6 + item flags (implies audit+combos)\n" +
        "  --grid                the payment-curve grid (growth x P1), gates-lite per cell\n" +
        "  --scorer-ev           bot-independent AnytimeScorer calibration report (own mode; ignores --strategy)\n" +
        "  --report PATH         also write the markdown report to PATH\n" +
        "  --verify              determinism self-check (200 runs twice), then exit\n";
}
