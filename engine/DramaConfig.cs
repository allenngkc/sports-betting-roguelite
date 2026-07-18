namespace SBR.Engine;

/// <summary>
/// Pacing dials for the drama generator (design/04). Defaults live here; the /sim
/// harness (Week 5) tunes them. All values feed the win-probability bridge and the
/// near-miss post-pass; none of them touch the sampled outcome — drama is authored
/// toward a result decided at lock, never the reverse (design/04 integrity rule).
/// </summary>
public sealed class DramaConfig
{
    /// <summary>Inclusive lower bound on the per-leg event count at full density (round ≥ <see cref="DensityRampRounds"/>).</summary>
    public int MinEventsPerLeg { get; set; } = 3;

    /// <summary>Inclusive upper bound on the per-leg event count at full density (round ≥ <see cref="DensityRampRounds"/>).</summary>
    public int MaxEventsPerLeg { get; set; } = 5;

    /// <summary>Round-1 lower bound of the progressive-density ramp (design/04: early sweats are shorter and simpler).</summary>
    public int EarlyMinEventsPerLeg { get; set; } = 2;

    /// <summary>Round-1 upper bound of the progressive-density ramp.</summary>
    public int EarlyMaxEventsPerLeg { get; set; } = 4;

    /// <summary>The round at which the ramp reaches the full band; ≤1 disables the ramp entirely.</summary>
    public int DensityRampRounds { get; set; } = 3;

    /// <summary>
    /// The per-leg event-count bounds for a 1-based round: Early* at round 1, linearly
    /// interpolated (AwayFromZero per bound) to the full band at <see cref="DensityRampRounds"/>.
    /// Normalization (min ≥ 1, max ≥ min) applies to BOTH branches — degenerate configs clamp,
    /// never crash (the /sim harness sweeps config values programmatically; same rule as the
    /// generator's k &lt; 1 clamp).
    /// </summary>
    public (int Min, int Max) EventBoundsForRound(int round)
    {
        int min, max;
        if (DensityRampRounds <= 1 || round >= DensityRampRounds)
        {
            min = MinEventsPerLeg;
            max = MaxEventsPerLeg;
        }
        else
        {
            double t = (round - 1) / (double)(DensityRampRounds - 1);
            min = (int)System.Math.Round(EarlyMinEventsPerLeg + t * (MinEventsPerLeg - EarlyMinEventsPerLeg),
                System.MidpointRounding.AwayFromZero);
            max = (int)System.Math.Round(EarlyMaxEventsPerLeg + t * (MaxEventsPerLeg - EarlyMaxEventsPerLeg),
                System.MidpointRounding.AwayFromZero);
        }
        if (min < 1) min = 1;
        if (max < min) max = min;
        return (min, max);
    }

    /// <summary>The ticket's LAST leg gets this multiple of the drawn event count (PRD F5: final leg gets 2x).</summary>
    public double FinalLegBudgetMultiplier { get; set; } = 2.0;

    /// <summary>Noise scale for the win-prob drift; larger = wilder swings per event.</summary>
    public double Volatility { get; set; } = 0.18;

    /// <summary>
    /// 0 = flat volatility across the leg; higher = quieter early, wilder late. Applied as the
    /// noise multiplier lateBias(i,k) = (1 - bias) + bias * 2 * (i/k): at i=0 it is (1 - bias),
    /// at i=k it is (1 + bias), crossing 1.0 at the midpoint. With the default 0.5 the envelope
    /// ramps 0.5 → 1.5 over the leg, so decisive noise lands late.
    /// </summary>
    public double LateDecisiveBias { get; set; } = 0.5;

    /// <summary>Probability that a leg gets a late hope-spike (if losing) or scare-dip (if winning).</summary>
    public double NearMissChance { get; set; } = 0.35;
}
