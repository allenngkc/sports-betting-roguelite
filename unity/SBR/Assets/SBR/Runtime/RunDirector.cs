using System;
using System.Text;
using SBR.Engine;
using UnityEngine;

namespace SBR.Game
{
    /// <summary>
    /// The single owner of the engine <see cref="Run"/> in the room (M4 — replaces the demo channel).
    /// Surfaces poll it: the laptop drives Betting/Shop/RunOver actions through it, the TV walks the
    /// locked round's sweats serially via <see cref="CurrentSession"/>/<see cref="AdvanceSweat"/> and
    /// hands back with <see cref="FinishAndSettle"/>. All engine transitions funnel through here so
    /// phase ordering (Betting → Sweat → Settlement → Shop/RunWon/RunLost → Betting) lives in one place.
    ///
    /// Zero-ticket locks settle immediately (no sessions to sweat) — the laptop railroads straight to
    /// the shop or the run-over page; the confirm on LOCK IT IN is the laptop's job.
    ///
    /// <see cref="LastSettle"/> carries the settle-card telemetry the TV renders (target met, the
    /// bookie floats you, debt cleared, run verdict). Presentation never consumes engine RNG.
    /// </summary>
    public sealed class RunDirector : MonoBehaviour
    {
        [Header("Dials")]
        [Tooltip("Fixed seed for the FIRST run. Blank = a fresh random 8-char A-Z0-9 seed (logged). " +
                 "Every NEW RUN after that rolls a new random seed.")]
        public string seed = "";

        public Run Run { get; private set; }

        /// <summary>Index into Run.Sweats/Run.Tickets of the sweat being presented (placement order).</summary>
        public int SweatIndex { get; private set; }

        public SweatSession CurrentSession { get; private set; }
        public Ticket CurrentTicket { get; private set; }

        /// <summary>Set by FinishAndSettle; cleared when the next round locks. The TV's settle card.</summary>
        public SettleReport? LastSettle { get; private set; }

        /// <summary>Bumped on every StartNewRun so surfaces can cheaply detect the reset.</summary>
        public int RunGeneration { get; private set; }

        private static readonly char[] SeedAlphabet =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        private System.Random _seedRng;

        public readonly struct SettleReport
        {
            public readonly int Round;
            public readonly double Bank;
            public readonly double Target;
            public readonly double DebtBefore;
            public readonly double DebtAfter;
            public readonly Phase Outcome;

            public SettleReport(int round, double bank, double target,
                                double debtBefore, double debtAfter, Phase outcome)
            {
                Round = round;
                Bank = bank;
                Target = target;
                DebtBefore = debtBefore;
                DebtAfter = debtAfter;
                Outcome = outcome;
            }

            /// <summary>The bookie floated this settle: a clean miss became working capital + debt.</summary>
            public bool Floated => DebtBefore == 0 && DebtAfter > 0;
            public bool DebtCleared => DebtBefore > 0 && DebtAfter == 0 && Outcome != Phase.RunLost;
            public bool TargetMet => Outcome == Phase.Shop && !Floated || Outcome == Phase.RunWon;
        }

        private void Awake()
        {
            _seedRng = new System.Random();
        }

        private void Start()
        {
            StartNewRun(seed);
        }

        /// <summary>The laptop's NEW RUN button (prototype flow — a real menu replaces this later).</summary>
        public void StartNewRun() => StartNewRun(NewSeed());

        /// <summary>Starts a pinned run for deterministic presentation tests and seed entry. Input is
        /// normalized exactly like first boot: trim visible seeds; blank/null rolls a fresh one.</summary>
        public void StartNewRun(string runSeed)
        {
            runSeed = string.IsNullOrWhiteSpace(runSeed) ? NewSeed() : runSeed.Trim();
            Run = new Run(runSeed);
            SweatIndex = 0;
            CurrentSession = null;
            CurrentTicket = null;
            LastSettle = null;
            RunGeneration++;
            Debug.Log($"[RunDirector] NEW RUN seed={runSeed} bank={Run.Bank} target={Run.CurrentTarget}");
        }

        // ------------------------------------------------------------------ betting → sweat

        /// <summary>Locks the round (laptop's LOCK IT IN). With tickets, queues the first sweat for the
        /// TV; with none, the round settles on the spot and the laptop railroads onward.</summary>
        public void LockRound()
        {
            if (Run == null || Run.Phase != Phase.Betting) return;
            LastSettle = null;
            Run.LockRound();

            if (Run.Sweats.Count == 0)
            {
                FinishAndSettle(); // vacuously complete; no TV ceremony for a no-bet round
                return;
            }

            SweatIndex = 0;
            CurrentSession = Run.Sweats[0];
            CurrentTicket = Run.Tickets[0];
        }

        /// <summary>Steps to the next ticket's sweat (TV, after a ticket card). False when none remain.</summary>
        public bool AdvanceSweat()
        {
            if (Run == null || Run.Phase != Phase.Sweat) return false;
            if (SweatIndex + 1 >= Run.Sweats.Count) return false;

            SweatIndex++;
            CurrentSession = Run.Sweats[SweatIndex];
            CurrentTicket = Run.Tickets[SweatIndex];
            return true;
        }

        /// <summary>FinishSweat + Settle once every session is complete; captures the settle card.</summary>
        public void FinishAndSettle()
        {
            if (Run == null || Run.Phase != Phase.Sweat) return;

            int round = Run.Round;
            double target = Run.CurrentTarget;
            double debtBefore = Run.Debt;

            Run.FinishSweat();
            Run.Settle();

            LastSettle = new SettleReport(round, Run.Bank, target, debtBefore, Run.Debt, Run.Phase);
            CurrentSession = null;
            CurrentTicket = null;
        }

        // ------------------------------------------------------------------ shop

        /// <summary>Buy the shop offer at the index; returns the error message for the laptop to show
        /// inline (null on success). The engine's exceptions are the rulebook.</summary>
        public string TryBuyRelic(int offerIndex)
        {
            if (Run == null || Run.Phase != Phase.Shop) return "shop is closed";
            try
            {
                Run.BuyRelic(offerIndex);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public void ExitShop()
        {
            if (Run == null || Run.Phase != Phase.Shop) return;
            Run.ExitShop();
        }

        // ------------------------------------------------------------------ misc

        private string NewSeed()
        {
            var sb = new StringBuilder(8);
            for (int i = 0; i < 8; i++) sb.Append(SeedAlphabet[_seedRng.Next(SeedAlphabet.Length)]);
            return sb.ToString();
        }
    }
}
