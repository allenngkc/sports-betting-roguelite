using System;
using System.Collections.Generic;

namespace SBR.Engine;

/// <summary>
/// Pure betting math per design/02. Everything is decimal odds internally;
/// American odds exist only at the display boundary.
/// </summary>
public static class OddsMath
{
    public static double ImpliedProb(double decimalOdds)
    {
        RequireOdds(decimalOdds);
        return 1.0 / decimalOdds;
    }

    public static double FairDecimal(double p)
    {
        RequireProb(p);
        return 1.0 / p;
    }

    public static int AmericanFromDecimal(double decimalOdds)
    {
        RequireOdds(decimalOdds);
        return decimalOdds >= 2.0
            ? (int)Math.Round(100.0 * (decimalOdds - 1.0))
            : -(int)Math.Round(100.0 / (decimalOdds - 1.0));
    }

    public static double DecimalFromAmerican(int american)
    {
        if (american >= 100) return 1.0 + american / 100.0;
        if (american <= -100) return 1.0 + 100.0 / -american;
        throw new ArgumentException($"American odds must be >= +100 or <= -100, got {american}");
    }

    /// <summary>
    /// Prices a two-way market at true probability plus a proportional overround,
    /// so implied probabilities sum to 1 + overround (design/02 vig definition).
    /// </summary>
    public static (double home, double away) OverroundOdds(double trueHomeProb, double overround)
    {
        RequireProb(trueHomeProb);
        if (overround < 0) throw new ArgumentException("overround must be >= 0");
        double qHome = trueHomeProb * (1.0 + overround);
        double qAway = (1.0 - trueHomeProb) * (1.0 + overround);
        return (1.0 / qHome, 1.0 / qAway);
    }

    /// <summary>The same proportional overround over an n-way market — 1X2 is the first, and
    /// correct score will be the next. Identical rule to the two-way form: implied probabilities
    /// sum to 1 + overround, so per-outcome EV is −vig regardless of how many outcomes there are.
    /// The probabilities must be a complete, mutually exclusive partition; a set that does not sum
    /// to 1 is a caller bug and is rejected rather than silently renormalized, because silently
    /// renormalizing an incomplete set is how a market ships priced against the wrong universe.</summary>
    public static double[] OverroundOdds(IReadOnlyList<double> trueProbs, double overround)
    {
        if (trueProbs == null || trueProbs.Count < 2)
            throw new ArgumentException("an n-way market needs at least two outcomes", nameof(trueProbs));
        if (overround < 0) throw new ArgumentException("overround must be >= 0");
        double sum = 0.0;
        foreach (double p in trueProbs) { RequireProb(p); sum += p; }
        if (Math.Abs(sum - 1.0) > 1e-9)
            throw new ArgumentException(
                $"n-way true probabilities must partition the outcome space (sum {sum:0.############}, expected 1)",
                nameof(trueProbs));

        var odds = new double[trueProbs.Count];
        for (int i = 0; i < trueProbs.Count; i++)
        {
            double q = trueProbs[i] * (1.0 + overround);
            double o = 1.0 / q;
            if (o <= 1.0)
                throw new ArgumentException(
                    $"outcome {i} prices at {o:0.000} <= 1.0 (true prob {trueProbs[i]:0.####} too high "
                    + "for the configured overround)", nameof(trueProbs));
            odds[i] = o;
        }
        return odds;
    }

    public static double Ev(double p, double stake, double decimalOdds)
    {
        RequireProb(p);
        RequireOdds(decimalOdds);
        return stake * (p * decimalOdds - 1.0);
    }

    public static double ParlayDecimal(IReadOnlyList<double> legOdds)
    {
        double product = 1.0;
        for (int i = 0; i < legOdds.Count; i++)
        {
            RequireOdds(legOdds[i]);
            product *= legOdds[i];
        }
        return product;
    }

    public static double ParlayProb(IReadOnlyList<double> legProbs)
    {
        double product = 1.0;
        for (int i = 0; i < legProbs.Count; i++)
        {
            RequireProb(legProbs[i]);
            product *= legProbs[i];
        }
        return product;
    }

    /// <summary>The EV haircut the book took at ticket lock: s × (1 − o_offered / o_fair).</summary>
    public static double VigPaid(double stake, double offeredDecimal, double fairDecimal)
    {
        RequireOdds(offeredDecimal);
        RequireOdds(fairDecimal);
        return stake * (1.0 - offeredDecimal / fairDecimal);
    }

    /// <summary>
    /// Fair value of a partially resolved parlay: s × Π(o_resolved) × Π(p_j × o_j remaining).
    /// Anchors: nothing resolved → P(win) × payout; everything resolved → full payout.
    /// </summary>
    public static double CashOutFair(double stake, double resolvedOddsProduct, IReadOnlyList<(double p, double o)> remainingLegs)
    {
        double value = stake * resolvedOddsProduct;
        for (int i = 0; i < remainingLegs.Count; i++)
        {
            RequireProb(remainingLegs[i].p);
            RequireOdds(remainingLegs[i].o);
            value *= remainingLegs[i].p * remainingLegs[i].o;
        }
        return value;
    }

    public static double CashOutOffer(double fairValue, double margin) => fairValue * (1.0 - margin);

    private static void RequireOdds(double o)
    {
        if (o <= 1.0 || double.IsNaN(o) || double.IsInfinity(o))
            throw new ArgumentException($"Decimal odds must be > 1, got {o}");
    }

    private static void RequireProb(double p)
    {
        if (p <= 0.0 || p >= 1.0 || double.IsNaN(p))
            throw new ArgumentException($"Probability must be in (0, 1), got {p}");
    }
}
