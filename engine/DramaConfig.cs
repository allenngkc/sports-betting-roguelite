namespace SBR.Engine;

/// <summary>
/// Pacing dials for the drama generator (design/04). Defaults live here; the /sim
/// harness (Week 5) tunes them. All values feed the win-probability bridge and the
/// near-miss post-pass; none of them touch the sampled outcome — drama is authored
/// toward a result decided at lock, never the reverse (design/04 integrity rule).
/// </summary>
public sealed class DramaConfig
{
    /// <summary>Inclusive lower bound on the per-leg event count drawn from the Drama stream.</summary>
    public int MinEventsPerLeg { get; set; } = 5;

    /// <summary>Inclusive upper bound on the per-leg event count drawn from the Drama stream.</summary>
    public int MaxEventsPerLeg { get; set; } = 10;

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
