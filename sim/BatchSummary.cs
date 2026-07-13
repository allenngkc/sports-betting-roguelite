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

    // ---- economy-rework telemetry ----

    /// <summary>Fraction of runs whose Totem fired (each run fires at most once).</summary>
    public double TotemFireRate;

    /// <summary>Deaths with the bank within 20% of the missed payment, as a % of all deaths —
    /// the near-miss failure mode (report metric). 0 when the batch had no deaths.</summary>
    public double CloseCallDeathPct;

    /// <summary>Scar ratchet telemetry: mean peak stacks (pp) and mean carrier burns per run.</summary>
    public double MeanMaxScar;
    public double MeanScarBurns;

    /// <summary>Mean bookie gifts received per run (the pity channel firing).</summary>
    public double MeanGifts;

    public static BatchSummary From(string name, RunResult[] results)
    {
        var s = new BatchSummary { Name = name, N = results.Length };
        s.DeathRounds = new int[results.Length];
        s.BiggestSwings = new double[results.Length];
        s.FinalBanks = new double[results.Length];

        var evByRound = new List<double>[9];
        var decByRound = new List<double>[9];
        for (int r = 1; r <= 8; r++) { evByRound[r] = new List<double>(); decByRound[r] = new List<double>(); }

        long totemFires = 0, scarBurns = 0, gifts = 0;
        double maxScarSum = 0;
        int deaths = 0, closeCalls = 0;
        for (int i = 0; i < results.Length; i++)
        {
            RunResult rr = results[i];
            s.DeathRounds[i] = rr.DeathRound;
            s.BiggestSwings[i] = rr.BiggestSwing;
            s.FinalBanks[i] = rr.FinalBank;
            if (rr.Won) { s.Wins++; s.WinningFinalBanks.Add(rr.FinalBank); }
            else { deaths++; if (rr.CloseCallDeath) closeCalls++; }
            totemFires += rr.TotemFires;
            scarBurns += rr.ScarBurns;
            gifts += rr.GiftsReceived;
            maxScarSum += rr.MaxScarStacks;

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

        s.TotemFireRate = results.Length == 0 ? 0.0 : 100.0 * totemFires / results.Length;
        s.CloseCallDeathPct = deaths == 0 ? 0.0 : 100.0 * closeCalls / deaths;
        s.MeanMaxScar = results.Length == 0 ? 0.0 : maxScarSum / results.Length;
        s.MeanScarBurns = results.Length == 0 ? 0.0 : (double)scarBurns / results.Length;
        s.MeanGifts = results.Length == 0 ? 0.0 : (double)gifts / results.Length;

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
