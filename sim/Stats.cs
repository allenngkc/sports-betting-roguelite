using System;
using System.Collections.Generic;

namespace SBR.Sim;

/// <summary>Small deterministic reducers over run results. All sequential — no floating-point
/// order dependence — so aggregates are byte-identical regardless of how the runs were scheduled.</summary>
public static class Stats
{
    /// <summary>Percentile (0..100) via nearest-rank on a copy sorted ascending. Empty → 0.</summary>
    public static double Percentile(IReadOnlyList<double> values, double pct)
    {
        if (values.Count == 0) return 0.0;
        var sorted = new List<double>(values);
        sorted.Sort();
        if (sorted.Count == 1) return sorted[0];
        double rank = pct / 100.0 * (sorted.Count - 1);
        int lo = (int)Math.Floor(rank);
        int hi = (int)Math.Ceiling(rank);
        if (lo == hi) return sorted[lo];
        double frac = rank - lo;
        return sorted[lo] * (1.0 - frac) + sorted[hi] * frac;
    }

    public static double Median(IReadOnlyList<double> values) => Percentile(values, 50.0);

    /// <summary>Median of integer values (e.g. death rounds), returned as a double (x.5 for even counts).</summary>
    public static double MedianInt(IReadOnlyList<int> values)
    {
        if (values.Count == 0) return 0.0;
        var doubles = new double[values.Count];
        for (int i = 0; i < values.Count; i++) doubles[i] = values[i];
        return Median(doubles);
    }

    public static double Mean(IReadOnlyList<double> values)
    {
        if (values.Count == 0) return 0.0;
        double sum = 0.0;
        for (int i = 0; i < values.Count; i++) sum += values[i];
        return sum / values.Count;
    }
}
