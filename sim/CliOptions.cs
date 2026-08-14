using System;
using System.Collections.Generic;
using System.Globalization;

namespace SBR.Sim;

/// <summary>Parsed command line (see Program for the usage banner).</summary>
public sealed class CliOptions
{
    public int Runs = 10000;
    public string Strategy = "all";     // naive | random | skilled | noshop | martyr | all
    public string SeedPrefix = "SIM";
    public bool Audit;
    public int Combos;                  // 0 = off; else runs per passive pair
    public bool Gates;                  // the full G1–G7 campaign (implies audit + combos + all bots)
    public bool Grid;                   // the payment-curve grid (growth × P1), gates-lite per cell
    public bool ScorerEv;               // bot-independent AnytimeScorer calibration; own mode, never bundled
    public string? ReportPath;
    public bool Verify;

    /// <summary>True when --runs was given explicitly. The gate campaign's n is RULED, not a
    /// caller's default (Allen 2026-08-07): G6's resolution is a function of n, and at the old
    /// n=1,000 the gate could not fail. A bare --gates therefore runs at GateData.CampaignRuns and
    /// can never again be silently under-powered — while an explicit --runs still wins, which is
    /// how the GateData.EscalationRuns re-run is asked for.</summary>
    public bool RunsExplicit;

    public static readonly string[] AllStrategies = { "naive", "random", "skilled", "noshop", "martyr" };

    /// <summary>What <c>--strategy</c> ACCEPTS, which is deliberately wider than what "all" runs.
    /// The samematch probe is a coverage instrument for G7's SGP arm, not one of the economy bots
    /// the default report compares — adding it to <see cref="AllStrategies"/> would put it in every
    /// default batch and in <c>--verify</c>, changing runs that have nothing to do with it. It is
    /// selectable so it can be smoke-tested on its own, and the gate campaign names it explicitly
    /// (Program's gates roster, alongside the archetype bots, which are reachable the same way).</summary>
    public static readonly string[] SelectableStrategies =
        { "naive", "random", "skilled", "noshop", "martyr", "samematch", "chalk", "hoarder", "ironhands" };

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
                    options.RunsExplicit = true;
                    break;
                case "--strategy":
                    if (!TryTake(args, ref i, out options.Strategy!, out error)) return false;
                    if (options.Strategy != "all" && Array.IndexOf(SelectableStrategies, options.Strategy) < 0)
                    {
                        error = $"--strategy must be all|{string.Join("|", SelectableStrategies)}, "
                            + $"got '{options.Strategy}'";
                        return false;
                    }
                    break;
                case "--gates":
                    options.Gates = true;
                    break;
                case "--grid":
                    options.Grid = true;
                    break;
                case "--scorer-ev":
                    options.ScorerEv = true;
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

        // Applied after the loop so it holds whichever order the flags arrive in.
        if (options.Gates && !options.RunsExplicit) options.Runs = GateData.CampaignRuns;
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
