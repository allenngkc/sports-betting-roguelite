using System;
using System.Globalization;

namespace SBR.Game
{
    /// <summary>
    /// The bookie's text voice (M5, remapped for the payment economy — design/10 F: the bookie is
    /// the CREDITOR; the schedule is his). Lowercase, deadpan, warm only while you're paying.
    /// Lines are presentation-only and chosen by a stable hash of seed, stamped round, and trigger
    /// kind, so reading the phone never consumes engine RNG.
    /// </summary>
    public static class BookieScript
    {
        private static readonly string[] RunStart =
        {
            "you know the schedule. first one's {0}. don't be a stranger.",
            "new book, same arrangement. {0} due at the first settle.",
        };

        private static readonly string[] CliffDemand =
        {
            "this one's bigger. {0}. hope you've been building.",
            "the number jumps today. {0}. no surprises between us.",
        };

        private static readonly string[] FinalDemand =
        {
            "last payment. {0}. all of it.",
            "final settle. {0}. then we're done, one way or the other.",
        };

        private static readonly string[] TotemBurned =
        {
            "covered you. once. the next one grows.",
            "that trinket bought you a week. don't make it a habit.",
        };

        private static readonly string[] CloseCallReceipt =
        {
            "received. you're cutting it close.",
            "paid. barely. i notice these things.",
        };

        private static readonly string[] Gift =
        {
            "rough stretch. house sends its regards - a {0}, on me.",
            "cold streak, huh. here's a {0}. keep betting.",
        };

        private static readonly string[] Collection =
        {
            "short. we're past texts now.",
            "that's that. someone will come by.",
        };

        private static readonly string[] VerdictWon =
        {
            "paid in full. the house blinks first. don't come back.",
            "all of it, settled. i almost respect it.",
        };

        /// <summary>Writes the line for a trigger: {0} is the money amount — or, for GIFT, the
        /// item name passed as <paramref name="detail"/>.</summary>
        public static string Write(string runSeed, int round, BookieMessageKind kind,
            double amount = 0.0, string detail = null)
        {
            string[] pool = Pool(kind);
            uint hash = DemoTicketPolicy.StableHash($"{runSeed}#{round}#{kind}");
            string line = pool[(int)(hash % (uint)pool.Length)];
            if (line.IndexOf("{0}", StringComparison.Ordinal) < 0) return line;
            return string.Format(CultureInfo.InvariantCulture, line, detail ?? Money(amount));
        }

        public static string Money(double value)
        {
            long rounded = (long)Math.Round(value, MidpointRounding.AwayFromZero);
            return "$" + rounded.ToString("N0", CultureInfo.InvariantCulture);
        }

        private static string[] Pool(BookieMessageKind kind)
        {
            switch (kind)
            {
                case BookieMessageKind.RUN_START: return RunStart;
                case BookieMessageKind.CLIFF_DEMAND: return CliffDemand;
                case BookieMessageKind.FINAL_DEMAND: return FinalDemand;
                case BookieMessageKind.TOTEM_BURNED: return TotemBurned;
                case BookieMessageKind.CLOSE_CALL_RECEIPT: return CloseCallReceipt;
                case BookieMessageKind.GIFT: return Gift;
                case BookieMessageKind.COLLECTION: return Collection;
                case BookieMessageKind.VERDICT_WON: return VerdictWon;
                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }
    }
}
