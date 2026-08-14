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
    public readonly double[] MeanPassiveEvByRound = new double[9]; // G4's gate series
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

    /// <summary>Per-run win flags in run-index order — paired-seed CI math for the item flags
    /// (rev 5 §15) subtracts these across batches that share a seed list.</summary>
    public bool[] WonFlags = Array.Empty<bool>();

    /// <summary>Batch-total per-item events (rev 5 §16): offered/acquired/bought/sold/used.</summary>
    public readonly Dictionary<string, ItemEvents> ItemTotals = new();

    /// <summary>Runs whose final effect state for the id was > 0 (ratchets that actually wound).</summary>
    public readonly Dictionary<string, int> StatePositiveRuns = new();

    /// <summary>Batch-total market exposure. Scorer rows remain zero by declared bot policy.</summary>
    public readonly Dictionary<MarketKind, MarketExposure> MarketExposure = new();

    // ---- SAME MATCH coverage (F_0.6.0 step 4) — reduced from the per-run fields, in run-index
    // order, exactly like every other aggregate on this type. Zero for every bot but the probe.

    public long SameMatchPlaced;
    public long SameMatchSettled;
    public long SameMatchVoids;
    public long SameMatchRefusals;
    public long SameMatchUnexpectedRefusals;
    public readonly Dictionary<RefusalKind, int> SameMatchRefusalKinds = new();
    public readonly Dictionary<RelationKind, SameMatchExposure> SameMatchRelations = new();

    /// <summary>Per-MARKET-kind same-match exposure — G7-SGP's per-kind arm reads this one, and
    /// section 0b prints it. Zero for every bot but the probe.</summary>
    public readonly Dictionary<MarketKind, SameMatchKindExposure> SameMatchKinds = new();

    public static BatchSummary From(string name, RunResult[] results)
    {
        var s = new BatchSummary { Name = name, N = results.Length };
        s.DeathRounds = new int[results.Length];
        s.BiggestSwings = new double[results.Length];
        s.FinalBanks = new double[results.Length];
        s.WonFlags = new bool[results.Length];

        var evByRound = new List<double>[9];
        var pevByRound = new List<double>[9];
        var decByRound = new List<double>[9];
        for (int r = 1; r <= 8; r++)
        {
            evByRound[r] = new List<double>();
            pevByRound[r] = new List<double>();
            decByRound[r] = new List<double>();
        }

        long totemFires = 0, scarBurns = 0, gifts = 0;
        double maxScarSum = 0;
        int deaths = 0, closeCalls = 0;
        for (int i = 0; i < results.Length; i++)
        {
            RunResult rr = results[i];
            s.DeathRounds[i] = rr.DeathRound;
            s.BiggestSwings[i] = rr.BiggestSwing;
            s.FinalBanks[i] = rr.FinalBank;
            s.WonFlags[i] = rr.Won;
            if (rr.Won) { s.Wins++; s.WinningFinalBanks.Add(rr.FinalBank); }
            else { deaths++; if (rr.CloseCallDeath) closeCalls++; }

            foreach ((string id, ItemEvents e) in rr.ItemEvents)
            {
                if (!s.ItemTotals.TryGetValue(id, out ItemEvents? tot))
                    s.ItemTotals[id] = tot = new ItemEvents();
                tot.Offered += e.Offered;
                tot.Acquired += e.Acquired;
                tot.Bought += e.Bought;
                tot.Sold += e.Sold;
                tot.Used += e.Used;
            }
            foreach ((string id, double v) in rr.FinalEffectStates)
                if (v > 0)
                    s.StatePositiveRuns[id] = s.StatePositiveRuns.TryGetValue(id, out int n) ? n + 1 : 1;
            totemFires += rr.TotemFires;
            scarBurns += rr.ScarBurns;
            gifts += rr.GiftsReceived;
            maxScarSum += rr.MaxScarStacks;

            s.SameMatchPlaced += rr.SameMatchPlaced;
            s.SameMatchSettled += rr.SameMatchSettled;
            s.SameMatchVoids += rr.SameMatchVoids;
            s.SameMatchRefusals += rr.SameMatchRefusals;
            s.SameMatchUnexpectedRefusals += rr.SameMatchUnexpectedRefusals;
            foreach ((RefusalKind kind, int n) in rr.SameMatchRefusalKinds)
                s.SameMatchRefusalKinds[kind] =
                    s.SameMatchRefusalKinds.TryGetValue(kind, out int had) ? had + n : n;
            foreach ((RelationKind kind, SameMatchExposure e) in rr.SameMatchRelations)
            {
                if (!s.SameMatchRelations.TryGetValue(kind, out SameMatchExposure? total))
                    s.SameMatchRelations[kind] = total = new SameMatchExposure();
                total.Relations += e.Relations;
                total.Tickets += e.Tickets;
                total.Principal += e.Principal;
            }
            foreach ((MarketKind kind, SameMatchKindExposure e) in rr.SameMatchKinds)
            {
                if (!s.SameMatchKinds.TryGetValue(kind, out SameMatchKindExposure? total))
                    s.SameMatchKinds[kind] = total = new SameMatchKindExposure();
                total.Legs += e.Legs;
                total.Tickets += e.Tickets;
            }

            for (int r = 1; r <= 8; r++)
                if (rr.DeathRound >= r) s.AliveEntering[r]++;

            foreach (RoundMetrics rm in rr.Rounds)
            {
                int r = rm.Round;
                s.PlayedByRound[r]++;
                decByRound[r].Add(rm.Decisions);
                foreach (double ev in rm.TicketEvsAtLock) evByRound[r].Add(ev);
                foreach (double ev in rm.TicketPassiveEvsAtLock) pevByRound[r].Add(ev);
                foreach ((MarketKind kind, MarketExposure exposure) in rm.MarketExposure)
                {
                    if (!s.MarketExposure.TryGetValue(kind, out MarketExposure? total))
                        s.MarketExposure[kind] = total = new MarketExposure();
                    total.LegsPlaced += exposure.LegsPlaced;
                    total.DrawLegs += exposure.DrawLegs;
                    total.Stake += exposure.Stake;
                    total.RealizedNet += exposure.RealizedNet;
                    total.RealizedNetUnit += exposure.RealizedNetUnit;
                }
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
            s.MeanPassiveEvByRound[r] = Stats.Mean(pevByRound[r]);
            s.MedianDecisionsByRound[r] = Stats.Median(decByRound[r]);
        }
        return s;
    }

    /// <summary>First round (1..8) whose mean per-ticket PASSIVE-ONLY EV at lock is ≥ 0, or 0 if
    /// it never crosses — G4's gate series (the full-contract series is telemetry).</summary>
    public int EvZeroCrossRound()
    {
        for (int r = 1; r <= 8; r++)
            if (EvSampleByRound[r] > 0 && MeanPassiveEvByRound[r] >= 0.0) return r;
        return 0;
    }
}
