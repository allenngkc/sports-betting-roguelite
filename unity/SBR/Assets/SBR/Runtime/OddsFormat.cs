using System;
using System.Globalization;

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
        // S30: every printed signed number uses U+2212 MINUS, prices included, never the U+002D
        // hyphen a bare long.ToString() would emit for a negative value. No per-region exception.
        private const string Minus = "−";

        public static string American(double decimalOdds)
        {
            double profit = decimalOdds - 1.0;
            // S30/S36: an absence marker, not a signed number — the book never actually prices a
            // non-positive-profit line, so this return is unreachable in play. Prints S36's "—"
            // (an honest missing figure) rather than the plain "-" this used to emit, which could
            // be misread as exactly the wrong minus glyph S30 exists to ban from a price slot.
            if (profit <= 0.0) return "—";

            long a = profit >= 1.0
                ? (long)Math.Round(profit * 100.0, MidpointRounding.AwayFromZero)
                : -(long)Math.Round(100.0 / profit, MidpointRounding.AwayFromZero);
            return a > 0 ? $"+{a}" : Minus + (-a).ToString(CultureInfo.InvariantCulture);
        }
    }
}
