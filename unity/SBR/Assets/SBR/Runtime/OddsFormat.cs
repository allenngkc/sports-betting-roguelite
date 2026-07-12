using System;

namespace SBR.Game
{
    /// <summary>
    /// Display-only odds formatting (playtest #6: American odds are the default for now). The engine
    /// stays decimal everywhere; only the surfaces convert. Decimal d pays d per 1 staked, so profit
    /// is d−1: profit ≥ 1 → "+{profit×100}", profit < 1 → "−{100/profit}", rounded to the integer
    /// like every real book. 2.00 → +100 exactly.
    /// </summary>
    public static class OddsFormat
    {
        public static string American(double decimalOdds)
        {
            double profit = decimalOdds - 1.0;
            if (profit <= 0.0) return "-"; // degenerate price; never produced by the book

            long a = profit >= 1.0
                ? (long)Math.Round(profit * 100.0, MidpointRounding.AwayFromZero)
                : -(long)Math.Round(100.0 / profit, MidpointRounding.AwayFromZero);
            return a > 0 ? $"+{a}" : a.ToString();
        }
    }
}
