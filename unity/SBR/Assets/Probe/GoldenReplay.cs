using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Probe
{
    /// <summary>
    /// PLATFORM PROBE — disposable (M1, Phase 2). NOT product code.
    ///
    /// Single source of the Week 2 determinism pins inside Unity, ported verbatim from
    /// engine.tests/GoldenSeedTests.cs: the scripted GOLDEN-W2 round (a 3-leg parlay that wins two
    /// legs then loses on its decisive final leg, plus a winning single), the 47-event
    /// (LegIndex, Step, Type, Tag) sequence, the first-ten WinProbAfter values, and the settled bank.
    /// Both the EditMode determinism test and the runtime <see cref="DeterminismProbe"/> replay
    /// through this so the pins live once. An unintentional change here is a determinism regression.
    /// </summary>
    public static class GoldenReplay
    {
        public const string Seed = "GOLDEN-W2";
        public const double ExpectedBank = 428.631019;

        // (LegIndex, Step, Type, Tag) for every one of the 47 events, in fast-forward order.
        public static readonly (int leg, int step, DramaEventType type, TensionTag tag)[] ExpectedEvents =
        {
            (0, 1, DramaEventType.Score,    TensionTag.Swing),
            (0, 2, DramaEventType.BigPlay,  TensionTag.Swing),
            (0, 3, DramaEventType.Score,    TensionTag.Swing),
            (0, 4, DramaEventType.BigPlay,  TensionTag.NearMiss),
            (0, 5, DramaEventType.LegFinal, TensionTag.Decisive),
            (1, 1, DramaEventType.Score,    TensionTag.Swing),
            (1, 2, DramaEventType.Score,    TensionTag.Swing),
            (1, 3, DramaEventType.Momentum, TensionTag.Calm),
            (1, 4, DramaEventType.Momentum, TensionTag.Calm),
            (1, 5, DramaEventType.BigPlay,  TensionTag.NearMiss),
            (1, 6, DramaEventType.LegFinal, TensionTag.Decisive),
            (2, 1, DramaEventType.Score,    TensionTag.Swing),
            (2, 2, DramaEventType.Score,    TensionTag.Calm),
            (2, 3, DramaEventType.Momentum, TensionTag.Calm),
            (2, 4, DramaEventType.Score,    TensionTag.Swing),
            (2, 5, DramaEventType.Momentum, TensionTag.Calm),
            (2, 6, DramaEventType.Momentum, TensionTag.Calm),
            (2, 7, DramaEventType.Momentum, TensionTag.Calm),
            (2, 8, DramaEventType.Score,    TensionTag.Swing),
            (2, 9, DramaEventType.Momentum, TensionTag.Calm),
            (2, 10, DramaEventType.Score,    TensionTag.Swing),
            (2, 11, DramaEventType.Momentum, TensionTag.Calm),
            (2, 12, DramaEventType.BigPlay,  TensionTag.NearMiss),
            (2, 13, DramaEventType.BigPlay,  TensionTag.LeadChange),
            (2, 14, DramaEventType.Momentum, TensionTag.Calm),
            (2, 15, DramaEventType.Score,    TensionTag.Calm),
            (2, 16, DramaEventType.Momentum, TensionTag.Calm),
            (2, 17, DramaEventType.Momentum, TensionTag.Calm),
            (2, 18, DramaEventType.LegFinal, TensionTag.Decisive),
            (0, 1, DramaEventType.Momentum, TensionTag.Calm),
            (0, 2, DramaEventType.Momentum, TensionTag.Calm),
            (0, 3, DramaEventType.Momentum, TensionTag.Calm),
            (0, 4, DramaEventType.Momentum, TensionTag.Calm),
            (0, 5, DramaEventType.Score,    TensionTag.Calm),
            (0, 6, DramaEventType.Momentum, TensionTag.Calm),
            (0, 7, DramaEventType.Score,    TensionTag.Swing),
            (0, 8, DramaEventType.Momentum, TensionTag.Calm),
            (0, 9, DramaEventType.Momentum, TensionTag.Calm),
            (0, 10, DramaEventType.Momentum, TensionTag.Calm),
            (0, 11, DramaEventType.Momentum, TensionTag.Calm),
            (0, 12, DramaEventType.Momentum, TensionTag.Calm),
            (0, 13, DramaEventType.Momentum, TensionTag.Calm),
            (0, 14, DramaEventType.Score,    TensionTag.Calm),
            (0, 15, DramaEventType.Momentum, TensionTag.Calm),
            (0, 16, DramaEventType.BigPlay,  TensionTag.NearMiss),
            (0, 17, DramaEventType.BigPlay,  TensionTag.LeadChange),
            (0, 18, DramaEventType.LegFinal, TensionTag.Decisive),
        };

        // WinProbAfter (6 dp) for the first ten events.
        public static readonly double[] ExpectedFirstTenWinProb =
        {
            0.664315, 0.843441, 0.970000, 0.250000, 1.000000,
            0.820891, 0.956521, 0.970000, 0.902995, 0.250000,
        };

        /// <summary>The scripted GOLDEN-W2 round: a 3-leg parlay + a winning single, locked and ready to sweat.</summary>
        public static Run ScriptedRound()
        {
            var run = new Run(Seed);
            // Parlay: (0,Away) win, (2,Away) win, (3,Home) lose-on-final.  Single: (1,Home) win.
            run.PlaceTicket(new[] { new Pick(0, Side.Away), new Pick(2, Side.Away), new Pick(3, Side.Home) }, 100);
            run.PlaceTicket(new[] { new Pick(1, Side.Home) }, 50);
            run.LockRound();
            return run;
        }

        public static List<DramaEvent> DrainAll(Run run)
        {
            var all = new List<DramaEvent>();
            foreach (SweatSession s in run.Sweats)
                while (s.MoveNext(out var e))
                    all.Add(e);
            return all;
        }

        public struct Result
        {
            public bool Pass;
            public string Hash;
            public string Detail;
        }

        /// <summary>
        /// Replays the round and compares every pin. Returns pass/fail, a human-readable detail, and an
        /// FNV-1a fingerprint of the event stream so divergence across platforms is visible at a glance.
        /// </summary>
        public static Result Verify()
        {
            var r = new Result { Pass = true, Detail = "all pins matched" };

            List<DramaEvent> events = DrainAll(ScriptedRound());
            r.Hash = EventStreamHash(events);

            if (events.Count != ExpectedEvents.Length)
                return Fail(ref r, $"event count {events.Count} != {ExpectedEvents.Length}");

            for (int i = 0; i < events.Count; i++)
            {
                DramaEvent e = events[i];
                var x = ExpectedEvents[i];
                if (e.LegIndex != x.leg || e.Step != x.step || e.Type != x.type || e.Tag != x.tag)
                    return Fail(ref r, $"event[{i}] = ({e.LegIndex},{e.Step},{e.Type},{e.Tag}) != ({x.leg},{x.step},{x.type},{x.tag})");
            }

            for (int i = 0; i < ExpectedFirstTenWinProb.Length; i++)
                if (Math.Abs(events[i].WinProbAfter - ExpectedFirstTenWinProb[i]) > 1e-6)
                    return Fail(ref r, $"winprob[{i}] = {events[i].WinProbAfter:0.000000} != {ExpectedFirstTenWinProb[i]:0.000000}");

            Run settle = ScriptedRound();
            DrainAll(settle);
            settle.FinishSweat();
            if (settle.Phase != Phase.Settlement)
                return Fail(ref r, $"phase {settle.Phase} != Settlement");
            if (settle.Tickets[0].State != TicketState.Lost || settle.Tickets[1].State != TicketState.Won)
                return Fail(ref r, $"ticket states ({settle.Tickets[0].State},{settle.Tickets[1].State}) != (Lost,Won)");
            if (Math.Abs(settle.Bank - ExpectedBank) > 1e-5)
                return Fail(ref r, $"bank {settle.Bank:0.000000} != {ExpectedBank:0.000000}");

            return r;
        }

        private static Result Fail(ref Result r, string why)
        {
            r.Pass = false;
            r.Detail = why;
            return r;
        }

        /// <summary>
        /// FNV-1a 64-bit fingerprint of the event stream. Integer fields (leg, step, type, tag) fold
        /// exactly; WinProbAfter folds at its pinned 6-dp precision so sub-6-dp floating-point noise can
        /// never perturb the fingerprint — a hash mismatch means the pinned behaviour genuinely diverged.
        /// </summary>
        public static string EventStreamHash(IReadOnlyList<DramaEvent> events)
        {
            const ulong prime = 1099511628211UL;
            ulong h = 14695981039346656037UL; // FNV offset basis
            foreach (DramaEvent e in events)
            {
                h = Fold(h, e.LegIndex, prime);
                h = Fold(h, e.Step, prime);
                h = Fold(h, (int)e.Type, prime);
                h = Fold(h, (int)e.Tag, prime);
                h = Fold(h, (long)Math.Round(e.WinProbAfter * 1_000_000.0), prime);
            }
            return h.ToString("x16");
        }

        private static ulong Fold(ulong h, long value, ulong prime)
        {
            ulong u = unchecked((ulong)value);
            for (int i = 0; i < 8; i++)
            {
                h ^= (u & 0xFF);
                h *= prime;
                u >>= 8;
            }
            return h;
        }
    }
}
