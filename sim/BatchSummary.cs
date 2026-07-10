using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>Sequentially-reduced aggregates for one strategy batch. All loops run in run-index order so
/// the numbers are byte-identical regardless of how the batch was scheduled across threads.</summary>
public sealed class BatchSummary
{
    public string Name = "";
    public int N;

    public readonly int[] AliveEntering = new int[9];   // [r] = runs that played round r (1..8)
    public readonly int[] PlayedByRound = new int[9];    // same as AliveEntering, named for the grind read
    public int Wins;
    public double WonPct => N == 0 ? 0 : 100.0 * Wins / N;

    public int[] DeathRounds = Array.Empty<int>();       // 1..8, or 9 for a won run
    public double MedianDeath;
    public double MeanDeath;                             // mean of DeathRounds — discriminates when median saturates

    public double[] BiggestSwings = Array.Empty<double>();
    public double[] FinalBanks = Array.Empty<double>();
    public readonly List<double> WinningFinalBanks = new();

    public readonly double[] MeanEvByRound = new double[9];
    public readonly int[] EvSampleByRound = new int[9];
    public readonly double[] MedianDecisionsByRound = new double[9];

    // ---- debt-as-HP (DECISIONS.md 2026-07-09) ----

    /// <summary>Mean bookie floats taken per run.</summary>
    public double MeanFloats;

    /// <summary>Deaths that happened while indebted, as a % of all deaths (the rest are plain
    /// final-round misses). 0 when the batch had no deaths.</summary>
    public double InDebtDeathPct;

    public static BatchSummary From(string name, RunResult[] results)
    {
        var s = new BatchSummary { Name = name, N = results.Length };
        s.DeathRounds = new int[results.Length];
        s.BiggestSwings = new double[results.Length];
        s.FinalBanks = new double[results.Length];

        var evByRound = new List<double>[9];
        var decByRound = new List<double>[9];
        for (int r = 1; r <= 8; r++) { evByRound[r] = new List<double>(); decByRound[r] = new List<double>(); }

        long floats = 0;
        int deaths = 0, inDebtDeaths = 0;
        for (int i = 0; i < results.Length; i++)
        {
            RunResult rr = results[i];
            s.DeathRounds[i] = rr.DeathRound;
            s.BiggestSwings[i] = rr.BiggestSwing;
            s.FinalBanks[i] = rr.FinalBank;
            if (rr.Won) { s.Wins++; s.WinningFinalBanks.Add(rr.FinalBank); }
            else { deaths++; if (rr.DiedInDebt) inDebtDeaths++; }
            floats += rr.FloatsTaken;

            for (int r = 1; r <= 8; r++)
                if (rr.DeathRound >= r) s.AliveEntering[r]++;

            foreach (RoundMetrics rm in rr.Rounds)
            {
                int r = rm.Round;
                s.PlayedByRound[r]++;
                decByRound[r].Add(rm.Decisions);
                foreach (double ev in rm.TicketEvsAtLock) evByRound[r].Add(ev);
            }
        }

        s.MeanFloats = results.Length == 0 ? 0.0 : (double)floats / results.Length;
        s.InDebtDeathPct = deaths == 0 ? 0.0 : 100.0 * inDebtDeaths / deaths;

        s.MedianDeath = Stats.MedianInt(s.DeathRounds);
        var deathAsDouble = new double[s.DeathRounds.Length];
        for (int i = 0; i < s.DeathRounds.Length; i++) deathAsDouble[i] = s.DeathRounds[i];
        s.MeanDeath = Stats.Mean(deathAsDouble);
        for (int r = 1; r <= 8; r++)
        {
            s.EvSampleByRound[r] = evByRound[r].Count;
            s.MeanEvByRound[r] = Stats.Mean(evByRound[r]);
            s.MedianDecisionsByRound[r] = Stats.Median(decByRound[r]);
        }
        return s;
    }

    /// <summary>First round (1..8) whose mean per-ticket EV at lock is ≥ 0, or 0 if it never crosses.</summary>
    public int EvZeroCrossRound()
    {
        for (int r = 1; r <= 8; r++)
            if (EvSampleByRound[r] > 0 && MeanEvByRound[r] >= 0.0) return r;
        return 0;
    }
}
