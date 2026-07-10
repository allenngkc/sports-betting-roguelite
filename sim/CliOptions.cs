using System;
using System.Collections.Generic;
using System.Globalization;

namespace SBR.Sim;

/// <summary>Parsed command line (see Program for the usage banner).</summary>
public sealed class CliOptions
{
    public int Runs = 10000;
    public string Strategy = "all";     // naive | random | skilled | all
    public string SeedPrefix = "SIM";
    public bool Audit;
    public int Combos;                  // 0 = off; else runs per relic pair
    public string? ReportPath;
    public bool Verify;

    public static readonly string[] AllStrategies = { "naive", "random", "skilled" };

    public IReadOnlyList<string> SelectedStrategies =>
        Strategy == "all" ? AllStrategies : new[] { Strategy };

    public static bool TryParse(string[] args, out CliOptions options, out string? error)
    {
        options = new CliOptions();
        error = null;
        for (int i = 0; i < args.Length; i++)
        {
            string a = args[i];
            switch (a)
            {
                case "--runs":
                    if (!TryTakeInt(args, ref i, out options.Runs, out error)) return false;
                    if (options.Runs < 1) { error = "--runs must be ≥ 1"; return false; }
                    break;
                case "--strategy":
                    if (!TryTake(args, ref i, out options.Strategy!, out error)) return false;
                    if (options.Strategy != "all" && Array.IndexOf(AllStrategies, options.Strategy) < 0)
                    { error = $"--strategy must be naive|random|skilled|all, got '{options.Strategy}'"; return false; }
                    break;
                case "--seed-prefix":
                    if (!TryTake(args, ref i, out options.SeedPrefix!, out error)) return false;
                    break;
                case "--audit":
                    options.Audit = true;
                    break;
                case "--combos":
                    if (!TryTakeInt(args, ref i, out options.Combos, out error)) return false;
                    if (options.Combos < 1) { error = "--combos must be ≥ 1"; return false; }
                    break;
                case "--report":
                    if (!TryTake(args, ref i, out options.ReportPath!, out error)) return false;
                    break;
                case "--verify":
                    options.Verify = true;
                    break;
                case "-h":
                case "--help":
                    error = "help";
                    return false;
                default:
                    error = $"Unknown option '{a}'";
                    return false;
            }
        }
        return true;
    }

    private static bool TryTake(string[] args, ref int i, out string value, out string? error)
    {
        if (i + 1 >= args.Length) { value = ""; error = $"{args[i]} needs a value"; return false; }
        value = args[++i];
        error = null;
        return true;
    }

    private static bool TryTakeInt(string[] args, ref int i, out int value, out string? error)
    {
        value = 0;
        if (!TryTake(args, ref i, out string s, out error)) return false;
        if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
        { error = $"{args[i - 1]} needs an integer, got '{s}'"; return false; }
        return true;
    }
}
