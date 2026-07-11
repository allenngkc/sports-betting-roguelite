using System;
using System.Collections.Generic;
using System.Text;
using SBR.Engine;
using UnityEngine;

namespace SBR.Game
{
    /// <summary>
    /// The demo channel driver (M3 architecture decision 1; M4 replaces it with the real loop). On scene
    /// start it creates a <see cref="Run"/> from a seed (a serialized string, or a fresh random 8-char
    /// A-Z0-9 seed, logged), auto-places one demo ticket via <see cref="DemoTicketPolicy"/>, and locks the
    /// round so a <see cref="SweatSession"/> is ready. After TvSweatScreen finishes presenting a sweat it
    /// calls <see cref="SettleAndAdvance"/>: FinishSweat, Settle, then either ExitShop (buying nothing) and
    /// place the next ticket, or - on RunWon/RunLost - start a brand new run. An endless TV channel.
    ///
    /// It exposes the current session/ticket for the screen to pull, chrome values (ROUND/BANK/TARGET/
    /// DEBT/SEED), and settlement telemetry the PlayMode test asserts against. Only MoveNext/CashOut-free
    /// engine APIs are called here, so presentation never consumes engine RNG.
    /// </summary>
    public sealed class DemoRunDriver : MonoBehaviour
    {
        [Header("Wiring (set by GrayboxRoomBuilder)")]
        public TvSweatScreen screen; // the screen pulls state from us; this back-ref is informational

        [Header("Dials")]
        [Tooltip("Fixed run seed for the FIRST run. Blank = a fresh random 8-char A-Z0-9 seed (logged). " +
                 "After a run ends (won/lost) the channel always rolls a new random seed.")]
        public string seed = "";

        public Run Run { get; private set; }
        public SweatSession CurrentSession { get; private set; }
        public Ticket CurrentTicket { get; private set; }

        // Settlement telemetry - the last round the screen handed back for settling.
        public int SettleCount { get; private set; }
        public TicketState LastTicketState { get; private set; }
        public double LastBankBeforeFinish { get; private set; }
        public double LastBankAfterFinish { get; private set; }
        public double LastPotentialPayout { get; private set; }

        private static readonly char[] SeedAlphabet =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        private System.Random _seedRng;

        private void Awake()
        {
            _seedRng = new System.Random();
        }

        private void Start()
        {
            StartNewRun(string.IsNullOrWhiteSpace(seed) ? NewSeed() : seed.Trim());
        }

        private string NewSeed()
        {
            var sb = new StringBuilder(8);
            for (int i = 0; i < 8; i++) sb.Append(SeedAlphabet[_seedRng.Next(SeedAlphabet.Length)]);
            return sb.ToString();
        }

        private void StartNewRun(string runSeed)
        {
            Run = new Run(runSeed);
            Debug.Log($"[DemoRunDriver] NEW RUN seed={runSeed} bank={Run.Bank} target={Run.CurrentTarget}");
            PlaceAndLock();
        }

        private void PlaceAndLock()
        {
            CurrentTicket = null;
            try
            {
                (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(Run);
                CurrentTicket = Run.PlaceTicket(picks, stake);
            }
            catch (Exception ex)
            {
                // Never expected at healthy banks (debt-as-HP keeps the bank >= the target); the round
                // still locks so the channel keeps moving.
                Debug.LogWarning($"[DemoRunDriver] demo ticket rejected: {ex.Message}");
            }

            Run.LockRound();
            CurrentSession = Run.Sweats.Count > 0 ? Run.Sweats[0] : null;
        }

        /// <summary>
        /// Called by TvSweatScreen once it has finished presenting the current sweat (naturally, on a
        /// bust, or on a cash-out). Settles the round and queues the next demo ticket.
        /// </summary>
        public void SettleAndAdvance()
        {
            if (Run == null) return;

            if (Run.Phase == Phase.Sweat)
            {
                LastBankBeforeFinish = Run.Bank;
                Run.FinishSweat();
                LastBankAfterFinish = Run.Bank;
                LastTicketState = CurrentTicket != null ? CurrentTicket.State : TicketState.Open;
                LastPotentialPayout = CurrentTicket != null ? CurrentTicket.PotentialPayout : 0.0;
                Run.Settle();
                SettleCount++;
            }

            switch (Run.Phase)
            {
                case Phase.Shop:
                    Run.ExitShop(); // the demo buys nothing
                    PlaceAndLock();
                    break;

                case Phase.RunWon:
                case Phase.RunLost:
                    StartNewRun(NewSeed()); // new run, new seed, keep the channel rolling
                    break;
            }
        }
    }
}
