using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Game
{
    public enum BookieMessageKind
    {
        RUN_START,
        FLOAT_WARM,
        FLOAT_COLD,
        DEBT_BETTING,
        NO_MORE_FAVORS,
        CLEARED,
        COLLECTION,
        VERDICT_WON,
        VERDICT_BUST,
    }

    public readonly struct BookieMessage
    {
        public BookieMessageKind Kind { get; }
        public int Round { get; }
        public string Text { get; }

        public BookieMessage(BookieMessageKind kind, int round, string text)
        {
            Kind = kind;
            Round = round;
            Text = text;
        }
    }

    /// <summary>
    /// Pure M5 trigger state over RunDirector snapshots. Settle beats use the report's round so a
    /// delayed observation after ExitShop cannot rewrite history; betting beats use the live round.
    /// Reset is atomic per run, while Revision and ArrivalSequence stay monotone for render and buzz.
    /// </summary>
    public sealed class BookieFeedModel
    {
        private readonly List<BookieMessage> _messages = new List<BookieMessage>();
        private readonly HashSet<int> _seenSettleRounds = new HashSet<int>();
        private readonly HashSet<int> _seenBettingRounds = new HashSet<int>();

        private bool _hasRun;
        private int _runGeneration;
        private int _floatCount;

        public IReadOnlyList<BookieMessage> Messages => _messages;
        public int UnreadCount { get; private set; }
        public long Revision { get; private set; }
        public long ArrivalSequence { get; private set; }

        public void Observe(int runGeneration, Run run, Phase phase, int round, double debt,
                            RunDirector.SettleReport? lastSettle)
        {
            if (run == null)
                return;

            if (!_hasRun || runGeneration != _runGeneration)
            {
                ResetForRun(runGeneration);
                Append(run, round, BookieMessageKind.RUN_START);
            }

            // A delayed frame can expose settle and next-round betting together. The debt origin
            // must speak before its reminder, matching the TV card's narrative order.
            if (lastSettle.HasValue)
                ProcessSettle(run, lastSettle.Value);

            if (phase == Phase.Betting && debt > 0.0 && _seenBettingRounds.Add(round))
            {
                BookieMessageKind kind = round == run.Config.Rounds
                    ? BookieMessageKind.NO_MORE_FAVORS
                    : BookieMessageKind.DEBT_BETTING;
                Append(run, round, kind, debt);
            }
        }

        public void MarkRead()
        {
            if (UnreadCount == 0)
                return;
            UnreadCount = 0;
            Revision++;
        }

        private void ResetForRun(int runGeneration)
        {
            _messages.Clear();
            _seenSettleRounds.Clear();
            _seenBettingRounds.Clear();
            _floatCount = 0;
            UnreadCount = 0;
            _runGeneration = runGeneration;
            _hasRun = true;
            Revision++;
        }

        private void ProcessSettle(Run run, RunDirector.SettleReport report)
        {
            if (!_seenSettleRounds.Add(report.Round))
                return;

            if (report.Floated)
            {
                BookieMessageKind kind = _floatCount == 0
                    ? BookieMessageKind.FLOAT_WARM
                    : BookieMessageKind.FLOAT_COLD;
                _floatCount++;
                Append(run, report.Round, kind, report.DebtAfter);
            }

            // Clearing debt is its own beat and always precedes a final verdict in the same report.
            if (report.DebtCleared)
                Append(run, report.Round, BookieMessageKind.CLEARED);

            if (report.Outcome == Phase.RunWon)
            {
                Append(run, report.Round, BookieMessageKind.VERDICT_WON);
            }
            else if (report.Outcome == Phase.RunLost)
            {
                Append(run, report.Round,
                    report.DebtAfter > 0.0 ? BookieMessageKind.COLLECTION : BookieMessageKind.VERDICT_BUST,
                    report.DebtAfter);
            }
        }

        private void Append(Run run, int round, BookieMessageKind kind, double amount = 0.0)
        {
            _messages.Add(new BookieMessage(kind, round,
                BookieScript.Write(run.Rng.RunSeed, round, kind, amount)));
            UnreadCount++;
            Revision++;
            ArrivalSequence++;
        }
    }
}
