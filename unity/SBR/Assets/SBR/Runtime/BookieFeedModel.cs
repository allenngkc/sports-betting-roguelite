using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Game
{
    public enum BookieMessageKind
    {
        RUN_START,
        CLIFF_DEMAND,
        FINAL_DEMAND,
        TOTEM_BURNED,
        CLOSE_CALL_RECEIPT,
        GIFT,
        COLLECTION,
        VERDICT_WON,
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
    /// Pure trigger state over RunDirector snapshots — the creditor model (design/10 F: the
    /// payments are HIS; the phone carries demands, receipts, and mercy). Settle beats stamp the
    /// report's round so a delayed observation after ExitShop cannot rewrite history; betting
    /// beats (demands, gifts) use the live round. Per-snapshot order: reset → settle beats →
    /// gift → demand. Reset is atomic per run; Revision and ArrivalSequence stay monotone —
    /// Revision drives rendering, ArrivalSequence (append-only) drives the buzz.
    /// </summary>
    public sealed class BookieFeedModel
    {
        /// <summary>A payment this much bigger than the last one draws the cliff-demand text.
        /// 1.5 → 1.45 with the charm-campaign curve (R5 155/105 = 1.476): the phase-two turn
        /// still deserves the growl.</summary>
        public const double CliffRatio = 1.45;

        /// <summary>Paying with less than this fraction of the payment left over reads as a
        /// close call — the bookie notices.</summary>
        public const double CloseCallFraction = 0.20;

        private readonly List<BookieMessage> _messages = new List<BookieMessage>();
        private readonly HashSet<int> _seenSettleRounds = new HashSet<int>();
        private readonly HashSet<int> _seenBettingRounds = new HashSet<int>();

        private bool _hasRun;
        private int _runGeneration;

        public IReadOnlyList<BookieMessage> Messages => _messages;
        public int UnreadCount { get; private set; }
        public long Revision { get; private set; }
        public long ArrivalSequence { get; private set; }

        public void Observe(int runGeneration, Run run, Phase phase, int round,
                            SettlementReport? lastSettlement)
        {
            if (run == null)
                return;

            if (!_hasRun || runGeneration != _runGeneration)
            {
                ResetForRun(runGeneration);
                Append(run, round, BookieMessageKind.RUN_START, run.PaymentSchedule[0]);
            }

            if (lastSettlement.HasValue)
                ProcessSettle(run, lastSettlement.Value);

            if (phase == Phase.Betting && _seenBettingRounds.Add(round))
            {
                // The gift text precedes the demand: mercy first, then business.
                if (run.LastGift != null)
                    Append(run, round, BookieMessageKind.GIFT, 0.0, run.LastGift.Name);

                // The OBSERVED round's payment — never Run.CurrentPayment, which tracks the run's
                // live round and diverges from a synthetic or delayed observation.
                double payment = run.PaymentSchedule[round - 1];
                if (round == run.Config.Rounds)
                    Append(run, round, BookieMessageKind.FINAL_DEMAND, payment);
                else if (round >= 2 && payment >= CliffRatio * run.PaymentSchedule[round - 2])
                    Append(run, round, BookieMessageKind.CLIFF_DEMAND, payment);
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
            UnreadCount = 0;
            _runGeneration = runGeneration;
            _hasRun = true;
            Revision++;
        }

        private void ProcessSettle(Run run, SettlementReport report)
        {
            if (!_seenSettleRounds.Add(report.Round))
                return;

            if (report.Outcome == Phase.RunLost)
            {
                Append(run, report.Round, BookieMessageKind.COLLECTION, report.Shortfall);
            }
            else if (report.Outcome == Phase.RunWon)
            {
                Append(run, report.Round, BookieMessageKind.VERDICT_WON);
            }
            else if (report.TotemFired)
            {
                Append(run, report.Round, BookieMessageKind.TOTEM_BURNED, report.Shortfall);
            }
            else if (report.Paid && report.BankAfter < CloseCallFraction * report.Payment)
            {
                Append(run, report.Round, BookieMessageKind.CLOSE_CALL_RECEIPT, report.BankAfter);
            }
        }

        private void Append(Run run, int round, BookieMessageKind kind, double amount = 0.0,
            string detail = null)
        {
            _messages.Add(new BookieMessage(kind, round,
                BookieScript.Write(run.Rng.RunSeed, round, kind, amount, detail)));
            UnreadCount++;
            Revision++;
            ArrivalSequence++;
        }
    }
}
