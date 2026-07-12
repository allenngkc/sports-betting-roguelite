using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Game
{
    /// <summary>
    /// Deterministic auto-play ticket policy: one ticket of 2-3 legs on the shortest-priced sides
    /// (the favourites), staked at 25% of the current bank (min $10). Born as M3's demo channel; M4
    /// replaced the channel with the real betting loop and kept this as the test / auto-play fixture
    /// (PlayMode full-loop tests, and a future attract mode). Pure and deterministic from run state -
    /// same seed and round produce the same picks and stake - and it consumes NO engine RNG (it only
    /// reads the already-baked slate, the bank, and the seed string).
    /// </summary>
    public static class DemoTicketPolicy
    {
        public static (IReadOnlyList<Pick> picks, double stake) Choose(Run run)
        {
            if (run == null) throw new ArgumentNullException(nameof(run));
            Slate slate = run.CurrentSlate;
            int n = slate.Matchups.Count;

            // Rank matchups by their shortest-priced (favourite) side, ascending; index breaks ties so
            // the ordering is total and deterministic.
            var order = new List<int>(n);
            for (int i = 0; i < n; i++) order.Add(i);
            order.Sort((a, b) =>
            {
                double fa = Math.Min(slate.Matchups[a].HomeOdds, slate.Matchups[a].AwayOdds);
                double fb = Math.Min(slate.Matchups[b].HomeOdds, slate.Matchups[b].AwayOdds);
                int c = fa.CompareTo(fb);
                return c != 0 ? c : a.CompareTo(b);
            });

            // Leg count varies 2-3 for channel variety, derived from a stable hash of seed+round (NOT
            // string.GetHashCode, which is randomised per process).
            int legCount = 2 + (int)(StableHash($"{run.Rng.RunSeed}#{run.Round}") % 2u);
            legCount = Math.Min(legCount, Math.Min(n, run.Config.MaxLegs));

            var picks = new List<Pick>(legCount);
            for (int i = 0; i < legCount; i++)
            {
                Matchup m = slate.Matchups[order[i]];
                Side side = m.HomeOdds <= m.AwayOdds ? Side.Home : Side.Away; // the shorter-priced side
                picks.Add(new Pick(m.Index, side));
            }

            double stake = Math.Max(run.Config.MinStake, Math.Floor(0.25 * run.Bank));
            double maxStake = run.Config.MaxStakeFraction * run.Bank;
            if (stake > maxStake) stake = Math.Floor(maxStake); // pathological low-bank guard; never trips at healthy banks
            return (picks, stake);
        }

        /// <summary>Deterministic FNV-1a over the string - stable across processes and platforms.</summary>
        public static uint StableHash(string s)
        {
            uint h = 2166136261u;
            foreach (char c in s)
            {
                h ^= c;
                h *= 16777619u;
            }
            return h;
        }
    }
}
