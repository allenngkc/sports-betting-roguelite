using System;
using System.Globalization;

namespace SBR.Game
{
    /// <summary>
    /// The bookie's text voice (M5, design/00): lowercase, deadpan, and friendly only while the
    /// favors are cheap. Lines are presentation-only and chosen by a stable hash of seed, stamped
    /// round, and trigger kind, so reading the phone never consumes engine RNG.
    /// </summary>
    public static class BookieScript
    {
        private static readonly string[] RunStart =
        {
            "new run. fresh numbers. try not to make either of us sentimental.",
            "board's open. i believe in you in the legally nonbinding sense.",
        };

        private static readonly string[] FloatWarm =
        {
            "covered you. you're into me for {0}. happens to the best customers.",
            "i floated the room. {0} on the arm. keep it moving.",
            "short night. i spotted you {0}. call it professional optimism.",
        };

        private static readonly string[] FloatCold =
        {
            "another {0}. we're developing a pattern and i don't collect patterns.",
            "you're into me for {0}. again. the friendship rate has expired.",
            "i covered {0}. this is the last time it feels conversational.",
        };

        private static readonly string[] DebtBetting =
        {
            "my {0} is in play too. pick like you remember that.",
            "you owe {0}. tonight the vig has a face.",
        };

        private static readonly string[] NoMoreFavors =
        {
            "final round. {0} due. there is no next favor.",
            "last board. bring back my {0} or don't bring back excuses.",
        };

        private static readonly string[] Cleared =
        {
            "we're square. knew you had it. mostly.",
            "debt cleared. deleting the draft with your address in it.",
        };

        private static readonly string[] Collection =
        {
            "that's the run. i still have your number. and {0} of your attention.",
            "account closed. balance isn't. {0} has entered the collection phase.",
        };

        private static readonly string[] VerdictWon =
        {
            "you got there. take the win before it learns your name.",
            "run cleared. proud of you in a way my accountant discourages.",
        };

        private static readonly string[] VerdictBust =
        {
            "busted clean. no debt, just evidence.",
            "run's over. good news: you only owe yourself an explanation.",
        };

        public static string Write(string runSeed, int round, BookieMessageKind kind, double amount = 0.0)
        {
            string[] pool = Pool(kind);
            uint hash = DemoTicketPolicy.StableHash($"{runSeed}#{round}#{kind}");
            string line = pool[(int)(hash % (uint)pool.Length)];
            return line.IndexOf("{0}", StringComparison.Ordinal) >= 0
                ? string.Format(CultureInfo.InvariantCulture, line, Money(amount))
                : line;
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
                case BookieMessageKind.FLOAT_WARM: return FloatWarm;
                case BookieMessageKind.FLOAT_COLD: return FloatCold;
                case BookieMessageKind.DEBT_BETTING: return DebtBetting;
                case BookieMessageKind.NO_MORE_FAVORS: return NoMoreFavors;
                case BookieMessageKind.CLEARED: return Cleared;
                case BookieMessageKind.COLLECTION: return Collection;
                case BookieMessageKind.VERDICT_WON: return VerdictWon;
                case BookieMessageKind.VERDICT_BUST: return VerdictBust;
                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }
    }
}
