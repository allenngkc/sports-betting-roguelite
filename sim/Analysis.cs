using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>Item power audit: skilled with each catalog item granted free vs baseline.
/// Passives are granted at run start; consumables are refilled every round (a single-use item
/// granted once would audit as noise). (Timeout was cut at playtest #8 — Allen's verdict matched
/// this audit's ≈0 read.)</summary>
public sealed class AuditData
{
    public BatchSummary Baseline = null!;
    public readonly List<Entry> Entries = new();

    public sealed class Entry
    {
        public string Id = "";
        public string Name = "";
        public bool IsConsumable;
        public double MedianDeath;
        public double WonPct;
        public double MedianDelta;
        public double WonDelta;
        public double MeanDeath;
        public double MeanDelta;      // Δ mean rounds survived — the discriminating signal
        public double TotemFireRate;  // only meaningful for the totem row

        // ---- charm expansion (rev 5 §15): paired-seed arrays + exposure ----
        public bool[] WonFlags = Array.Empty<bool>();
        public int[] DeathRounds = Array.Empty<int>();
        public int Used;               // consumable plays / bobblehead flips in the batch
        public int StatePositiveRuns;  // runs where the ratchet actually wound (stat > 0)
        public double WonDeltaSe;      // paired-seed SE of WonDelta, in pp
    }

    public static AuditData Compute(int runs, string seedPrefix, RunConfig cfg, BatchSummary skilledBaseline)
    {
        var audit = new AuditData { Baseline = skilledBaseline };
        var strat = new SkilledStrategy();

        foreach (RelicDefinition def in RelicCatalog.All)
            audit.Entries.Add(Entry_(strat, runs, seedPrefix, cfg, skilledBaseline,
                def.Id, def.Name, isConsumable: false));

        foreach (ConsumableDefinition def in RelicCatalog.Consumables)
            audit.Entries.Add(Entry_(strat, runs, seedPrefix, cfg, skilledBaseline,
                def.Id, def.Name, isConsumable: true));

        audit.Entries.Sort((a, b) => b.MeanDelta.CompareTo(a.MeanDelta));
        return audit;
    }

    private static Entry Entry_(SkilledStrategy strat, int runs, string seedPrefix, RunConfig cfg,
        BatchSummary baseline, string id, string name, bool isConsumable)
    {
        RunResult[] granted = isConsumable
            ? Harness.RunBatch(strat, runs, seedPrefix, cfg, grantedConsumable: id)
            : Harness.RunBatch(strat, runs, seedPrefix, cfg, new[] { id });
        BatchSummary s = BatchSummary.From("skilled+" + id, granted);
        return new Entry
        {
            Id = id,
            Name = name,
            IsConsumable = isConsumable,
            MedianDeath = s.MedianDeath,
            WonPct = s.WonPct,
            MedianDelta = s.MedianDeath - baseline.MedianDeath,
            WonDelta = s.WonPct - baseline.WonPct,
            MeanDeath = s.MeanDeath,
            MeanDelta = s.MeanDeath - baseline.MeanDeath,
            TotemFireRate = s.TotemFireRate,
            WonFlags = s.WonFlags,
            DeathRounds = s.DeathRounds,
            Used = s.ItemTotals.TryGetValue(id, out ItemEvents? e) ? e.Used : 0,
            StatePositiveRuns = s.StatePositiveRuns.TryGetValue(id, out int n) ? n : 0,
            WonDeltaSe = PairedSePp(s.WonFlags, baseline.WonFlags),
        };
    }

    /// <summary>Paired-seed SE of a win-rate delta, in percentage points (rev 5 §15).</summary>
    public static double PairedSePp(bool[] a, bool[] b)
    {
        int n = Math.Min(a.Length, b.Length);
        if (n < 2) return double.PositiveInfinity;
        double mean = 0;
        for (int i = 0; i < n; i++) mean += (a[i] ? 1 : 0) - (b[i] ? 1 : 0);
        mean /= n;
        double ss = 0;
        for (int i = 0; i < n; i++)
        {
            double d = ((a[i] ? 1 : 0) - (b[i] ? 1 : 0)) - mean;
            ss += d * d;
        }
        return 100.0 * Math.Sqrt(ss / (n - 1) / n);
    }

    /// <summary>Paired-seed SE of a mean-death delta (rounds).</summary>
    public static double PairedSeRounds(int[] a, int[] b)
    {
        int n = Math.Min(a.Length, b.Length);
        if (n < 2) return double.PositiveInfinity;
        double mean = 0;
        for (int i = 0; i < n; i++) mean += a[i] - b[i];
        mean /= n;
        double ss = 0;
        for (int i = 0; i < n; i++)
        {
            double d = (a[i] - b[i]) - mean;
            ss += d * d;
        }
        return Math.Sqrt(ss / (n - 1) / n);
    }
}

/// <summary>Pairwise passive combo scan: synergy excess over the sum of solo win-rate deltas.
/// With 3 passives this is 3 pairs — and the Multiplier+Scar pair is gate G5's superadditivity
/// evidence (composition through the shared PayoutMultiplier product).</summary>
public sealed class ComboData
{
    public int RunsPerConfig;
    public double BaselineWonPct;
    public readonly Dictionary<string, double> SoloWonPct = new();
    public readonly List<Pair> Pairs = new();

    /// <summary>Per-run win flags for the four arms the synergy excess is built from. Kept because
    /// the excess's ERROR cannot be computed from the four percentages alone — the arms share a
    /// seed prefix, so run i is the same dealt hand in all four, and the noise that cancels is only
    /// visible per run (Allen 2026-08-08: measure the error before setting the threshold).</summary>
    public bool[] BaselineWonFlags = Array.Empty<bool>();
    public readonly Dictionary<string, bool[]> SoloWonFlags = new();

    public sealed class Pair
    {
        public string IdA = "";
        public string IdB = "";
        public double PairWonPct;
        public double SynergyExcess;

        /// <summary>Two standard errors on <see cref="SynergyExcess"/>, in pp, paired by seed.
        /// NaN when fewer than two runs make a variance meaningless.</summary>
        public double SynergyExcessTwoSePp = double.NaN;
    }

    /// <summary>Paired-seed 2 SE of the synergy excess, in percentage points.
    ///
    /// The excess is <c>pair − soloA − soloB + baseline</c> and all four arms run on the SAME seed
    /// prefix, so run i is the same dealt hand in each and most of the run-to-run noise cancels
    /// INSIDE the combination. Differencing per seed and taking the variance of the difference is
    /// therefore the honest instrument; treating the four rates as independent would inflate the
    /// error and make G5 look blinder than it is. This is what rev 5 §15 already does for the
    /// two-arm audit (<see cref="AuditData.PairedSePp"/>) — the same idea over four arms.
    ///
    /// What it cannot see (C25): this is the error of the MEASUREMENT, not of the design claim. A
    /// synergy that is real, tiny and reliably positive would show a small excess with a small
    /// error and pass — which is exactly why the size worth requiring is a separate question, and
    /// Allen's.</summary>
    public static double PairedSynergyTwoSePp(bool[] pair, bool[] soloA, bool[] soloB, bool[] baseline)
    {
        int n = Math.Min(Math.Min(pair.Length, soloA.Length), Math.Min(soloB.Length, baseline.Length));
        if (n < 2) return double.NaN;

        double D(int i) => (pair[i] ? 1 : 0) - (soloA[i] ? 1 : 0) - (soloB[i] ? 1 : 0)
            + (baseline[i] ? 1 : 0);

        double mean = 0.0;
        for (int i = 0; i < n; i++) mean += D(i);
        mean /= n;
        double ss = 0.0;
        for (int i = 0; i < n; i++) { double d = D(i) - mean; ss += d * d; }
        return 2.0 * 100.0 * Math.Sqrt(ss / (n - 1) / n);
    }

    public Pair? Find(string a, string b)
    {
        foreach (Pair p in Pairs)
            if ((p.IdA == a && p.IdB == b) || (p.IdA == b && p.IdB == a)) return p;
        return null;
    }

    public static ComboData Compute(int runs, string seedPrefix, RunConfig cfg)
    {
        // G5 measurement (Allen-approved round-1 fix): the fixed-discipline bot removes the
        // ownership-changes-behavior confound, so pair-vs-solo deltas read pure composition.
        // The baseline is recomputed with the same bot for a like-for-like comparison.
        var strat = new FixedDisciplineStrategy();
        BatchSummary baseSummary = BatchSummary.From("f", Harness.RunBatch(strat, runs, seedPrefix, cfg));
        double fixedBaseline = baseSummary.WonPct;
        var data = new ComboData
        {
            RunsPerConfig = runs,
            BaselineWonPct = fixedBaseline,
            BaselineWonFlags = baseSummary.WonFlags,
        };
        IReadOnlyList<RelicDefinition> all = RelicCatalog.All;

        foreach (RelicDefinition def in all)
        {
            RunResult[] r = Harness.RunBatch(strat, runs, seedPrefix, cfg, new[] { def.Id });
            BatchSummary s = BatchSummary.From("s", r);
            data.SoloWonPct[def.Id] = s.WonPct;
            data.SoloWonFlags[def.Id] = s.WonFlags;
        }

        for (int i = 0; i < all.Count; i++)
        for (int j = i + 1; j < all.Count; j++)
        {
            string a = all[i].Id, b = all[j].Id;
            RunResult[] r = Harness.RunBatch(strat, runs, seedPrefix, cfg, new[] { a, b });
            BatchSummary ps = BatchSummary.From("s", r);
            double pairWon = ps.WonPct;
            double soloDeltaA = data.SoloWonPct[a] - fixedBaseline;
            double soloDeltaB = data.SoloWonPct[b] - fixedBaseline;
            double excess = (pairWon - fixedBaseline) - (soloDeltaA + soloDeltaB);
            data.Pairs.Add(new Pair
            {
                IdA = a, IdB = b, PairWonPct = pairWon, SynergyExcess = excess,
                SynergyExcessTwoSePp = PairedSynergyTwoSePp(
                    ps.WonFlags, data.SoloWonFlags[a], data.SoloWonFlags[b], data.BaselineWonFlags),
            });
        }

        data.Pairs.Sort((x, y) => y.SynergyExcess.CompareTo(x.SynergyExcess));
        return data;
    }
}

/// <summary>The gate table (PLAN.md, Allen-approved): the acceptance criteria of the economy
/// rework's sim campaign, computed from the batches + audit + combo data.</summary>
public sealed class GateData
{
    public sealed class Gate
    {
        public string Id = "";
        public string Description = "";
        public bool Pass;
        public string Actual = "";

        /// <summary>C32: every gate states its RESOLUTION — the smallest difference this reading can
        /// actually distinguish from noise. A verdict without one invites the failure C32 was
        /// promoted from: G3 adjudicating a 3pp band with a ±0.75pp instrument, unable to fail for
        /// exactly the kind of drift it exists to catch. Empty for gates whose criterion is
        /// structural rather than statistical (a coverage list either names a market or it does
        /// not; there is no sampling error in that).</summary>
        public string Resolution = "";

        /// <summary>C28's shape applied to one gate. A reading whose criterion edge falls INSIDE
        /// its own 95% interval cannot reject "the true value sits exactly on the line", so its
        /// verdict — whichever way it fell — is not a verdict. It is not a pass and not a fail; it
        /// is a re-run at <see cref="EscalationRuns"/>, the path Allen recorded on 2026-08-07.
        /// Reported, never inferred: the report drops "ALL GATES PASS" while one of these is live.
        /// It does NOT change the campaign's exit contract — Allen ruled a re-run, not a failure —
        /// so the banner is what carries it, and the banner is what a human reads.</summary>
        public bool Adjudicated = true;
    }

    /// <summary>The gate campaign's ruled sample size — Allen, 2026-08-07, on the G6 defect.
    /// G6 compares TWO measured rates, so its resolution is their combined 2 SE: **±2.15pp at
    /// n=1,000 against a 2pp band**. Its tolerance was narrower than its own noise, which is why it
    /// passed all session while being unable to fail for the thing it exists to catch. Resolution
    /// scales as 1/√n, so ±1.00pp costs 1,000 × (2.15/1.00)² ≈ 4,600 runs. That was the first half
    /// of the ruling and it was verified at 4,600: predicted ±1.00pp, MEASURED ±0.97pp.
    ///
    /// **The floor is 10,000 — Allen's second call, same day, after the escalation settled.** It is
    /// not a bigger guess than 4,600; it is the value a bare `--gates` had all along. The defect was
    /// never the default: it was this seat typing `--runs 1000` by hand, all session, with no code
    /// path objecting. Restoring 10,000 as a RULED floor rather than an unremarked default is what
    /// stops that recurring, and it buys resolution rather than spending it — G6 lands **±0.65pp,
    /// a 3.1× band** (measured 2026-08-08, not scaled: an arithmetic 2.15/√10 predicts ±0.68pp and
    /// 2.9×, which this comment carried until the floor was actually run), comfortably past the
    /// ±1.00pp the ruling asked for.
    ///
    /// Note what 3.1× still does not buy: it is under 4×, so a reading that lands near its line is
    /// still not adjudicated here and still escalates. The two-rung structure survives the floor
    /// change — 10,000 makes the gate able to FAIL, <see cref="EscalationRuns"/> makes it able to
    /// ADJUDICATE. Cost at 10,000 is UNMEASURED at the time of writing; measured neighbours are
    /// 10.4 and 13.3 min at 4,600 and 58.6 min at 18,500, and after three failed attempts to
    /// predict this machine's wall clock from a formula, the honest entry is the two measurements
    /// either side and no interpolation between them.</summary>
    public const int CampaignRuns = 10000;

    /// <summary>Allen's recorded escalation, same ruling: any near-line result re-runs here.
    /// 1,000 × (2.15/0.50)² ≈ 18,500 gives ±0.50pp, which puts the 2pp band at exactly 4×
    /// resolution — the threshold at which <see cref="BandVerdict"/> stops qualifying a reading.
    /// The two rungs are not arbitrary and are worth keeping in that order: 4,600 makes the gate
    /// able to FAIL, 18,500 makes it able to ADJUDICATE its own band — both confirmed on 2026-08-07:
    /// ±0.48pp measured, 2pp band at 4.1×, G3 and G6 both adjudicating. Cost: **58.6 min measured**
    /// (2,812,000 total runs). That is the wall time, not a formula — a "~42–54 min" range derived
    /// here from two 4,600 samples under-predicted it by 9–40%, so quote measurements.</summary>
    public const int EscalationRuns = 18500;

    /// <summary>Two standard errors on a percentage, the smallest move a proportion measured over
    /// <paramref name="n"/> runs can be trusted to show. Reported, never used to widen a verdict:
    /// the gate still passes or fails on its stated criterion, and this says how much of that
    /// verdict is signal.</summary>
    private static double TwoSePp(double pct, int n)
    {
        if (n <= 0) return double.NaN;
        double p = pct / 100.0;
        return 2.0 * 100.0 * Math.Sqrt(Math.Max(p * (1.0 - p), 0.0) / n);
    }

    /// <summary>C32's second half — a gate whose acceptance band is not comfortably wider than its
    /// own resolution cannot adjudicate, and must say so rather than reporting a clean PASS.
    ///
    /// Three tiers, and the two thresholds are exactly the two rungs of Allen's 2026-08-07 ruling:
    ///   ≥4× — the reading resolves the whole band; a verdict anywhere inside it is trustworthy.
    ///   ≥2× — the gate CAN fail: a true breach one resolution past the edge reads as a breach.
    ///          It still cannot separate a reading that sits closer to the edge than that.
    ///   &lt;2× — what G6 was at n=1,000: unable to fail for the drift it exists to catch.
    ///
    /// <paramref name="distanceToEdgePp"/> is THIS reading's distance from the nearest criterion
    /// edge — the second half of the honest answer, because a band wide enough in general says
    /// nothing about a reading that happens to land on the line. NaN where a gate has no banded
    /// edge to be near.</summary>
    private static string BandVerdict(double bandWidthPp, double twoSePp,
        double distanceToEdgePp = double.NaN)
    {
        if (double.IsNaN(twoSePp) || twoSePp <= 0.0) return "";
        double ratio = bandWidthPp / twoSePp;
        string head = $"±{twoSePp:0.00}pp (2 SE) — band {bandWidthPp:0.#}pp is {ratio:0.0}× resolution";

        // The band tier and this reading's position are two different facts and both are reported.
        // They are not alternatives: the weakest tier is exactly where a reading is most likely to
        // be sitting on its own line, so returning early on the tier — as this did until the first
        // smoke run showed the count claiming an unadjudicated gate the column never named — hides
        // the near-line case precisely where it matters most.
        string reach =
            ratio < 2.0
                ? "; **this gate cannot reliably fail for a drift smaller than its own noise — "
                  + "widen the band or raise --runs**"
                : ratio >= 4.0
                    ? "; resolves its whole band"
                    : $"; fails reliably on a breach ≥{twoSePp:0.00}pp past the edge, no closer";

        string nearLine = Adjudicates(twoSePp, distanceToEdgePp)
            ? ""
            : $"; **NOT ADJUDICATED — this reading sits {distanceToEdgePp:0.00}pp from the edge, "
              + $"inside its own resolution; re-run at `--runs {EscalationRuns}`**";

        return head + reach + nearLine;
    }

    /// <summary>Whether a reading is far enough from its criterion edge to have produced a verdict.
    /// A reading whose edge falls inside its own 95% interval cannot reject "the true value is
    /// exactly on the line" — so it decided nothing, whichever side it fell. The remedy is not a
    /// wider band or a softer claim: it is Allen's <see cref="EscalationRuns"/> re-run, named in
    /// the gate's own cell so the escalation cannot be skipped by not noticing it.</summary>
    private static bool Adjudicates(double twoSePp, double distanceToEdgePp)
        => double.IsNaN(distanceToEdgePp) || double.IsNaN(twoSePp) || distanceToEdgePp > twoSePp;

    public readonly List<Gate> Gates = new();
    public readonly List<string> ItemFlags = new();

    /// <summary>Informational notes (declared playtest-gated exemptions) — rendered with the
    /// flags but never blocking.</summary>
    public readonly List<string> Notes = new();

    /// <summary><paramref name="randomBot"/> is not judged by any gate — it is the CONTROL. G7's
    /// M1 exclusion needs evidence that a market the sharp declines is one he could have taken, and
    /// the blind bot is the only thing in the campaign that answers that.</summary>
    public static GateData Evaluate(BatchSummary? naive, BatchSummary? skilled, BatchSummary? noshop,
        BatchSummary? martyr, BatchSummary? martyrWorst, AuditData? audit, ComboData? combos,
        BatchSummary? randomBot = null)
    {
        var g = new GateData();

        // Bands re-ratified by Allen 2026-07-13 (design/10 F) after campaign round 1.
        if (naive != null)
            g.Add("G1", "honest gambling: naive win <1%, dies before the cliff resolves (median ≤6)",
                naive.MedianDeath <= 6.0 && naive.WonPct < 1.0,
                $"median {naive.MedianDeath:0.#}, won {naive.WonPct:F1}%");

        if (noshop != null)
            g.Add("G2", "engine mandatory: no-shop skilled win <2%, median death 5–6",
                noshop.MedianDeath >= 5.0 && noshop.MedianDeath <= 6.0 && noshop.WonPct < 2.0,
                $"median {noshop.MedianDeath:0.#}, won {noshop.WonPct:F1}%");

        if (skilled != null)
        {
            // C32 was promoted from THIS gate: a 3pp band (5–8%) adjudicated by an instrument
            // whose own 2 SE at the OLD default --runs was ~1.5pp. Arm B is the worked example —
            // a 0.8pp "movement" at n=1000 vanished at n=10,000, having been noise on both sides,
            // including in the before-arm. State the resolution beside the verdict so nobody reads
            // a one-point drift as a finding, or misses a real one.
            // The band and the width handed to BandVerdict are ONE fact and are declared once.
            // They were two literals — 5.0/8.0 in the criterion and a bare 3.0 in the resolution
            // call — which is a re-band away from a gate quoting a width it no longer has.
            const double floor = 4.5, ceiling = 8.0;
            const double width = ceiling - floor;
            double se = TwoSePp(skilled.WonPct, skilled.N);
            double edge = Math.Abs(Math.Min(skilled.WonPct - floor, ceiling - skilled.WonPct));
            g.Add("G3", $"skilled + items wins: median death ≥5, win {floor:0.#}–{ceiling:0.#}% "
                + "(re-banded by Allen 2026-08-08 from 5–8%: the economy reads 5.4–5.5%, only "
                + "0.4–0.5pp above the old floor, so the gate could not separate its own reading "
                + "from its own edge — three campaigns at 4,600 / 10,000 / 18,500 established that "
                + "no sample size fixes a gap that small. Prior band Allen 2026-07-15 — the dealt "
                + "hand's build variance is the roguelite shape)",
                skilled.MedianDeath >= 5.0 && skilled.WonPct >= floor && skilled.WonPct <= ceiling,
                $"median {skilled.MedianDeath:0.#}, won {skilled.WonPct:F1}% ({edge:0.0}pp from the "
                + "nearest band edge)",
                BandVerdict(width, se, edge), Adjudicates(se, edge));
        }

        if (skilled != null)
        {
            int cross = skilled.EvZeroCrossRound();
            g.Add("G4", "the EV arc exists: skilled mean ticket EV crosses zero in rounds 3–7",
                cross >= 3 && cross <= 7,
                cross == 0 ? "never crosses" : $"crosses at R{cross}");
        }

        if (combos != null && audit != null)
        {
            ComboData.Pair? pair = combos.Find(RelicCatalog.MultiplierId, RelicCatalog.ScarTissueId);
            if (pair != null)
            {
                // C32 for a gate that has NO BAND. G5's criterion is a threshold at exactly zero,
                // so there is no width to divide the error by and BandVerdict must not be called —
                // quoting a ratio would invent a band this gate does not have, which is the same
                // class of error as quoting a stale one. What can honestly be stated is the
                // instrument's own error and whether this reading clears it.
                // Allen 2026-08-08, on the lead's escalation: measure the error FIRST, then pick
                // the minimum worth requiring with the resolution known — the two gates before
                // this one (G6, G3) were both set where their instruments could not read them.
                double se = pair.SynergyExcessTwoSePp;
                double reading = Math.Abs(pair.SynergyExcess);
                // A zero variance here is DEGENERACY, not precision, and printing "±0.00pp" would
                // read as the most precise instrument in the campaign while carrying no signal at
                // all. It means the four arms never disagreed on a single run — the items changed
                // no outcome anywhere in the batch. Found by the first smoke at n=400; the failure
                // mode is the one this whole lane exists to catch, arriving inside the fix for it.
                string resolution = double.IsNaN(se)
                    ? "unmeasured — needs ≥2 paired runs"
                    : se <= 0.0
                        ? "**±0.00pp is degeneracy, not precision** — every paired run produced an "
                          + "identical combination, so the four arms never once disagreed on a "
                          + "single run. That is an absence of signal; raise --runs until the arms "
                          + "separate, and read nothing off this gate until they do."
                        : $"±{se:0.00}pp (2 SE, paired seeds) — **threshold at 0: no band, so no "
                          + "ratio to state**; this reading is "
                          + (reading > se
                              ? $"{reading / se:0.0}× its own error"
                              : "INSIDE its own error, indistinguishable from zero")
                          + ". The minimum worth requiring is Allen's to set, and is now set with "
                          + "this number in hand rather than before it.";

                g.Add("G5", "composition superadditive: Multiplier+Scar pair Δwin > sum of solo Δwins",
                    pair.SynergyExcess > 0.0,
                    $"synergy excess {pair.SynergyExcess:+0.0;-0.0}pp",
                    resolution,
                    // A threshold at zero makes the distance-to-edge simply |reading|. A zero or
                    // absent error adjudicates nothing, whichever side the reading fell.
                    se > 0.0 && Adjudicates(se, reading));
            }
        }

        // G6 (rev 5 §17): the WORST-CASE granted batch gates (Scar + Jar granted, Free Bet
        // refilled every round); the organic martyr is telemetry beside it.
        BatchSummary? guard = martyrWorst ?? martyr;
        if (guard != null && skilled != null)
        {
            // C32: this gate compares TWO measured rates, so its resolution is the combined error
            // of both, not either alone — a margin inside that is not a margin. This is why G6 is
            // WORSE than G3, the gate C32 was promoted from, and why it is the gate whose n Allen
            // ruled: the same run count buys it roughly √2 less resolution than a single-rate gate
            // (exactly √2 only where the two rates are equal — measured 1.34× vs the guard's own
            // error and 1.50× vs skilled's at n=1,000, which is the range, not the constant).
            double se = Math.Sqrt(
                Math.Pow(TwoSePp(guard.WonPct, guard.N), 2)
                + Math.Pow(TwoSePp(skilled.WonPct, skilled.N), 2));
            double margin = guard.WonPct - skilled.WonPct;
            double edge = Math.Abs(margin - 2.0);
            g.Add("G6", "martyr guard (worst case granted): loss-farming win ≤ skilled +2pp",
                guard.WonPct <= skilled.WonPct + 2.0,
                $"{guard.Name} {guard.WonPct:F1}% vs skilled {skilled.WonPct:F1}%"
                + (martyrWorst != null && martyr != null ? $" (organic martyr {martyr.WonPct:F1}%)" : "")
                + $" — margin {margin:+0.0;-0.0}pp, {edge:0.0}pp from the +2pp line",
                BandVerdict(2.0, se, edge), Adjudicates(se, edge));
        }

        // G7 (market coverage): every shipped MarketKind must be either exercised by the
        // skilled bot (LegsPlaced > 0) or on the named bot-excluded list below, with a reason —
        // no silent/magic skip. Each excluded kind also gets a Notes line so the coverage hole
        // is visible in every report rather than tolerated invisibly. Do NOT add a kind here
        // just because it currently reads zero legs — "unexercised" and "policy-excluded" are
        // different things, and conflating them is exactly the failure G7 exists to catch.
        if (skilled != null)
        {
            // M1 narrows this population. BTTS reading red was a FALSE red: a sharp declining an
            // edgeless near-even market is the bot being CORRECT, not the gate catching a hole.
            // The exclusion is measured rather than asserted — the number below distinguishes
            // "declined" from "blocked", which is the only thing that makes an exclusion honest.
            // Expiry: v2 pricing. Once the book carries noise the bot can find a real edge in, BTTS
            // becomes reachable and this entry comes out — an exclusion that outlives its reason is
            // the silent skip G7 was built to prevent.
            int randomBttsLegs = 0;
            if (randomBot != null
                && randomBot.MarketExposure.TryGetValue(MarketKind.BothTeamsToScore, out MarketExposure? rb))
                randomBttsLegs = rb.LegsPlaced;
            string bttsEvidence = randomBot == null
                ? "unmeasured this run — needs the random bot in the batch"
                : randomBttsLegs > 0
                    ? $"measured reachable: random places {randomBttsLegs:N0} BTTS legs where skilled places 0, "
                      + "so the market is DECLINED, not blocked"
                    : "NOT MEASURED REACHABLE: random placed 0 BTTS legs too — this exclusion is "
                      + "unproven and must not be trusted";

            var botExcluded = new Dictionary<MarketKind, string>
            {
                [MarketKind.AnytimeScorer] = "YES-only market, bots do not price it (declared policy)",
                [MarketKind.BothTeamsToScore] =
                    "near-even two-way market: under exact de-vig it never strictly wins a tie and "
                    + "its odds never clear the longshot threshold, so a sharp correctly declines it "
                    + $"(M1, expires at v2 pricing) — {bttsEvidence}",
            };

            var uncovered = new List<MarketKind>();
            foreach (MarketKind kind in Enum.GetValues(typeof(MarketKind)))
            {
                if (botExcluded.TryGetValue(kind, out string? reason))
                {
                    g.Notes.Add($"BOT-EXCLUDED: {Report.MarketName(kind)} — {reason}");
                    continue;
                }
                bool exercised = skilled.MarketExposure.TryGetValue(kind, out MarketExposure? exposure)
                    && exposure.LegsPlaced > 0;
                if (!exercised) uncovered.Add(kind);
            }

            g.Add("G7", "market coverage: every shipped MarketKind is exercised by the skilled "
                + "bot (LegsPlaced > 0) or on the named bot-excluded list",
                uncovered.Count == 0,
                uncovered.Count == 0
                    ? "all shipped markets covered"
                    : $"uncovered: {string.Join(", ", uncovered.ConvertAll(Report.MarketName))}",
                // C32: this criterion is structural, not statistical — a market either recorded a
                // leg or it did not, so there is no sampling error to quote and no band to widen.
                // Its exposure is a different matter: a market covered by a handful of legs is
                // covered, but thinly, and the exposure table is where that is read.
                "exact — a leg count is not a sample; no resolution limit");
        }

        if (audit != null)
        {
            // Statistical control (rev 5 §15): paired-seed CIs, Bonferroni z across the whole
            // endpoint family (every DEAD test + the two within-kind dominance contrasts).
            int family = audit.Entries.Count + 2;
            double z = BonferroniZ(0.05, family);

            // Always-on passives trigger by construction; ratchets must show wound-up runs.
            var ratchets = new HashSet<string>
                { "scar_tissue", "chalk_eater", "iron_hands", "bad_beat_jar", "the_system" };

            // PLAYTEST-GATED (declared, HOLDOUT burned → HOLDOUT2): items whose value is human
            // agency a greedy bot cannot monetize. The Manager's redeal audits ≈0 through a bot
            // that buys almost any hand — its worth is choosing. Timeout precedent: the same
            // exemption, then the playtest voted (and cut it). The Manager's vote went the
            // other way: RATIFIED KEEP at playtest #9 (2026-07-15) — the exemption is permanent.
            var playtestGated = new HashSet<string> { "ask_manager" };

            foreach (AuditData.Entry e in audit.Entries)
            {
                if (playtestGated.Contains(e.Id))
                {
                    g.Notes.Add($"PLAYTEST-GATED: {e.Name} audits ≈0 through bots "
                        + $"(Δwon {e.WonDelta:+0.0;-0.0}±{z * e.WonDeltaSe:0.0}pp) — RATIFIED KEEP, playtest #9");
                    continue;
                }
                // Exposure first (rev 5 §15, declared thresholds): an unexercised item's delta
                // is meaningless — UNDEREXPOSED blocks instead of flagging DEAD.
                if (e.IsConsumable && e.Used < MinUses)
                {
                    g.ItemFlags.Add($"UNDEREXPOSED: {e.Name} ({e.Used} uses < {MinUses} — fix the policy, not the item)");
                    continue;
                }
                if (!e.IsConsumable && ratchets.Contains(e.Id) && e.StatePositiveRuns < MinUses)
                {
                    g.ItemFlags.Add($"UNDEREXPOSED: {e.Name} ({e.StatePositiveRuns} wound-up runs < {MinUses})");
                    continue;
                }

                double meanSe = AuditData.PairedSeRounds(e.DeathRounds, audit.Baseline.DeathRounds);
                bool deadWon = e.WonDelta + z * e.WonDeltaSe < 1.0;
                bool deadMean = Math.Abs(e.MeanDelta) + z * meanSe < 0.05;
                if (deadWon && deadMean)
                    g.ItemFlags.Add($"DEAD: {e.Name} (Δwon {e.WonDelta:+0.0;-0.0}±{z * e.WonDeltaSe:0.0}pp, "
                        + $"Δmean {e.MeanDelta:+0.00;-0.00})");
            }

            // Dominance within KIND (rev 5 §15): the explicit contrast best − 2×next, CI lower
            // bound > 0 with a +0.5pp practical margin, winner non-DEAD by construction (its
            // Δwon must exceed 2×next + 0.5 ≥ 0.5, and the DEAD test would contradict it only
            // below 1.0pp — checked explicitly).
            foreach (bool consumables in new[] { false, true })
            {
                AuditData.Entry? best = null, next = null;
                foreach (AuditData.Entry e in audit.Entries)
                {
                    if (e.IsConsumable != consumables) continue;
                    if (best == null || e.WonDelta > best.WonDelta) { next = best; best = e; }
                    else if (next == null || e.WonDelta > next.WonDelta) next = e;
                }
                if (best == null || next == null || next.WonDelta <= 0) continue;

                double contrast = 0, ss = 0;
                int n = Math.Min(Math.Min(best.WonFlags.Length, next.WonFlags.Length),
                    audit.Baseline.WonFlags.Length);
                if (n < 2) continue;
                for (int i = 0; i < n; i++)
                    contrast += (best.WonFlags[i] ? 1 : 0) + (audit.Baseline.WonFlags[i] ? 1 : 0)
                        - 2 * (next.WonFlags[i] ? 1 : 0);
                contrast = 100.0 * contrast / n;
                double meanC = contrast / 100.0;
                for (int i = 0; i < n; i++)
                {
                    double d = (best.WonFlags[i] ? 1 : 0) + (audit.Baseline.WonFlags[i] ? 1 : 0)
                        - 2 * (next.WonFlags[i] ? 1 : 0) - meanC;
                    ss += d * d;
                }
                double se = 100.0 * Math.Sqrt(ss / (n - 1) / n);
                bool winnerAlive = best.WonDelta >= 1.0;
                if (contrast - z * se > 0 && contrast > 0.5 && winnerAlive)
                    g.ItemFlags.Add($"DOMINANT ({(consumables ? "consumable" : "passive")}): {best.Name} "
                        + $"(best−2×next = {contrast:+0.0}±{z * se:0.0}pp)");
            }

            foreach (AuditData.Entry e in audit.Entries)
            {
                if (e.Id != RelicCatalog.TotemId) continue;
                // Fire-rate band on the ORGANIC skilled batch (bought at shop price), not the
                // audit batch — a totem granted free to 100% of runs fires near-always by
                // construction (the artifact ratified around in sim-report-2, now encoded).
                double organicFire = skilled?.TotemFireRate ?? e.TotemFireRate;
                bool healthy = e.MeanDelta >= 0.3 && organicFire >= 25.0 && organicFire <= 60.0;
                if (!healthy)
                    g.ItemFlags.Add($"TOTEM: Δmean {e.MeanDelta:+0.00;-0.00} (want ≥0.3), " +
                        $"organic fire rate {organicFire:F0}% (want 25–60%)");
            }
        }

        return g;
    }

    /// <summary>Declared exposure threshold (PLAN.md rev 5 §15): below it the audit BLOCKS.</summary>
    public const int MinUses = 200;

    /// <summary>Two-sided Bonferroni-corrected z for the family (rational approximation of
    /// Φ⁻¹; exact enough for a flag threshold).</summary>
    public static double BonferroniZ(double alpha, int family)
    {
        double p = 1.0 - alpha / family / 2.0;
        // Beasley-Springer-Moro-ish approximation for the upper tail.
        double t = Math.Sqrt(-2.0 * Math.Log(1.0 - p));
        return t - (2.30753 + 0.27061 * t) / (1.0 + 0.99229 * t + 0.04481 * t * t);
    }

    private void Add(string id, string desc, bool pass, string actual, string resolution = "",
        bool adjudicated = true)
        => Gates.Add(new Gate
        {
            Id = id, Description = desc, Pass = pass, Actual = actual, Resolution = resolution,
            Adjudicated = adjudicated,
        });

    public bool AllPass
    {
        get
        {
            foreach (Gate gate in Gates) if (!gate.Pass) return false;
            return Gates.Count > 0;
        }
    }

    /// <summary>How many gates actually produced a verdict (C28: coverage is reported, never
    /// inferred — "no FAIL" is not "all passed"). Differs from <see cref="AllPass"/> on purpose:
    /// a gate can pass and still have decided nothing, which is the state Allen's escalation
    /// re-run exists to resolve.</summary>
    public int AdjudicatedCount
    {
        get
        {
            int n = 0;
            foreach (Gate gate in Gates) if (gate.Adjudicated) n++;
            return n;
        }
    }
}
