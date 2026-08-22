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
/// With 3 passives this is 3 pairs — and the exemplar pair is gate G5's superadditivity
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
    /// <param name="sameMatch">The SAME MATCH probe's batch (F_0.6.0 step 4) — G7's SGP arm. Null
    /// outside the full campaign, which is the only mode that runs the probe.</param>
    /// <param name="noLabelFallbacks">Campaign-wide count of tickets SOLD at the no-label naive
    /// product, read off the engine's process-wide counter after every batch. Passed in rather than
    /// read here so the reading is taken once, at a stated point, by the caller that scoped it.</param>
    public static GateData Evaluate(BatchSummary? naive, BatchSummary? skilled, BatchSummary? noshop,
        BatchSummary? martyr, BatchSummary? martyrWorst, AuditData? audit, ComboData? combos,
        BatchSummary? randomBot = null, BatchSummary? sameMatch = null, long noLabelFallbacks = 0)
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
            // EXEMPLAR MOVED — Allen 2026-08-08. G5 certified the composition pillar on
            // Multiplier + Scar Tissue, whose synergy measured **+0.1pp against ±0.06pp**: real,
            // but the weakest loop in the table and ~30× smaller than the strongest. The pillar
            // now certifies on real magnitude. The exemplar and its threshold were both set AFTER
            // the error was measured, which is the order Allen imposed on this gate and the first
            // time in this family it was followed — G6 and G3 were each set where their instrument
            // could not read them, and each cost campaigns to discover it.
            // EXEMPLAR MOVED AGAIN — Allen 2026-08-12, on Campaign A (the draws re-baseline).
            // Multiplier + House Key fell +2.96pp → +1.22pp under draws. The SYNERGY stayed real
            // (5.4× its own error, still classed superadditive) but the 1.0pp floor ended up 0.22pp
            // under the reading against ±0.23pp resolution, so the gate could no longer separate
            // "clears the floor" from "sits on it". Escalation was computed and REFUSED: at 18,500
            // the resolution only reaches ~±0.17pp for a ~0.22pp clearance — ~1.3×, still under the
            // 2× tier at which a gate can reliably fail, so a 58-minute run would have bought no
            // verdict. Lowering the floor was named and not taken: it would move the report's own
            // marginal/superadditive taxonomy to fit a reading.
            //
            // Longshot Larry's Photo + House Key STRENGTHENED under draws (+2.67pp → +2.87pp) and
            // clears the unchanged floor by 1.87pp at ~5.3× resolution. THE FLOOR IS UNCHANGED at
            // 1.0pp — only the exemplar moved.
            //
            // Prior exemplars, kept beneath rather than overwritten (the standing form):
            //   2026-08-08 Allen — Multiplier + House Key (was Multiplier + Scar Tissue, +0.1pp
            //   against ±0.06pp: real, but the weakest loop in the table).
            const string exemplarA = "longshot_photo", exemplarB = "house_key";

            // Threshold 1.0pp, and deliberately NOT a fresh invention: it is the line the report's
            // own taxonomy already draws between "marginal" and "superadditive". Against the new
            // exemplar's ±0.34pp it sits ~3× the error, so the gate can actually fail for the thing
            // it exists to catch. `> 0` would now be trivially satisfied by a +2.96pp reading and
            // would certify the pillar on any positive noise the day the exemplar drifts.
            const double minExcessPp = 1.0;

            ComboData.Pair? pair = combos.Find(exemplarA, exemplarB);
            if (pair == null)
            {
                // A missing exemplar must never be a missing GATE. Find() returns null on a typo or
                // a catalog rename, and the old shape simply skipped G5 — the campaign would report
                // six gates where seven were intended and pass, which is the vacuous-green failure
                // this lane has now found eleven times. It fails loudly instead.
                g.Add("G5", "composition superadditive: exemplar pair Δwin ≥ threshold",
                    false,
                    $"EXEMPLAR PAIR NOT FOUND in the combo scan ('{exemplarA}' + '{exemplarB}') — "
                    + "renamed or mis-keyed relic id; the gate cannot run and is failing rather "
                    + "than silently absenting itself",
                    "not applicable — the gate did not run");
            }
            else
            {
                // C32 for a gate with no BAND. The criterion is a ONE-SIDED floor — a minimum with
                // no ceiling — so there is still no width to divide the error by and BandVerdict
                // must not be called; quoting a ratio would invent a band this gate does not have,
                // the same class of error as quoting a stale one. What is honest here is the error,
                // the clearance over the floor, and what that clearance is worth in error units.
                // Allen 2026-08-08, on the lead's escalation: measure the error FIRST, then set the
                // minimum with the resolution known — the two gates before this one (G6, G3) were
                // both set where their instruments could not read them, and each cost campaigns.
                double se = pair.SynergyExcessTwoSePp;
                double clearance = Math.Abs(pair.SynergyExcess - minExcessPp);
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
                        : $"±{se:0.00}pp (2 SE, paired seeds) — one-sided floor at "
                          + $"{minExcessPp:0.#}pp, so no band width to state; this reading clears it "
                          + $"by {pair.SynergyExcess - minExcessPp:+0.00;-0.00}pp, "
                          + (clearance > se
                              ? $"{clearance / se:0.0}× resolution"
                              : "**INSIDE its own resolution — NOT ADJUDICATED**");

                g.Add("G5", "composition superadditive: the exemplar pair's synergy excess ≥ "
                    + $"{minExcessPp:0.#}pp (exemplar moved by Allen 2026-08-12 to Longshot Larry's "
                    + "Photo + House Key on the draws re-baseline: the prior exemplar, The "
                    + "Multiplier + House Key, fell +2.96pp → +1.22pp under draws — its synergy "
                    + "still real at 5.4× its own error, but the floor ended up inside the reading's "
                    + "resolution and the gate stopped adjudicating. Escalation to 18,500 was "
                    + "computed to buy ~1.3× and REFUSED. THE FLOOR IS UNCHANGED at 1.0pp — it is "
                    + "the report's own marginal/superadditive line, set AFTER the error was "
                    + "measured, and moving it to fit a reading was the alternative not taken. "
                    + "Prior exemplar Allen 2026-08-08, itself moved from Multiplier + Scar Tissue "
                    + "at +0.1pp against ±0.06pp — real, but the weakest loop in the table)",
                    pair.SynergyExcess >= minExcessPp,
                    $"synergy excess {pair.SynergyExcess:+0.0;-0.0}pp",
                    resolution,
                    // A one-sided floor makes the distance-to-edge |reading − floor|. A zero or
                    // absent error adjudicates nothing, whichever side the reading fell.
                    se > 0.0 && Adjudicates(se, clearance));
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

            // F_0.5.0 V1. Reachability is MEASURED the same way M1 measured BTTS: the random bot
            // prices nothing, so a market it places legs in is one the sharp DECLINED rather than
            // one he was blocked from. An exclusion without that evidence is an assertion, and the
            // whole point of G7 is that assertions are how coverage holes hide.
            string Reach(MarketKind kind)
            {
                if (randomBot == null) return "unmeasured this run — needs the random bot in the batch";
                int legs = randomBot.MarketExposure.TryGetValue(kind, out MarketExposure? rx) ? rx.LegsPlaced : 0;
                return legs > 0
                    ? $"measured reachable: random places {legs:N0} legs where skilled places 0, so it is DECLINED, not blocked"
                    : "NOT MEASURED REACHABLE: random placed 0 legs too — this exclusion is unproven and must not be trusted";
            }

            var botExcluded = new Dictionary<MarketKind, string>
            {
                [MarketKind.AnytimeScorer] = "YES-only market, bots do not price it (declared policy)",
                [MarketKind.PlayerMultiScorer] =
                    "YES-only player market on a floor-truncated board — inherits the AnytimeScorer "
                    + "human-agency policy, and its offered rows do not sum to the outcome space so "
                    + "there is nothing to de-vig against (declared policy)",
                [MarketKind.CorrectScore] =
                    "the board is TRUNCATED at the ratified 2% probability floor, so the offered "
                    + "scores are not an exhaustive outcome set; normalizing them would over-normalize "
                    + $"and manufacture an edge out of the missing rows — {Reach(MarketKind.CorrectScore)}",
                [MarketKind.WinningMargin] =
                    "one-way buckets that deliberately omit the draw (margin 0), so the set is not a "
                    + $"partition and de-vig has no denominator — {Reach(MarketKind.WinningMargin)}",
                [MarketKind.DoubleChance] =
                    "its three selections OVERLAP — 1X and X2 both contain the draw — so normalizing "
                    + "the implied probabilities is double counting, not de-vig. Structural, and it "
                    + $"does not expire at v2 pricing the way BTTS does — {Reach(MarketKind.DoubleChance)}",
                [MarketKind.TotalGoalsOddEven] =
                    "THE most near-even market on the board: measured across the latent box under "
                    + "draws it prices odd 0.490–0.499 / even 0.501–0.510, i.e. odds 1.87–1.94 on "
                    + "both sides, and NEITHER side reaches the 3.0 longshot threshold at any "
                    + "sampled latent point (0 of 105). Under exact de-vig it therefore never "
                    + "strictly wins a tie and no owned item can lift it, so a sharp correctly "
                    + "declines it — the BTTS shape, and stronger. Worth recording WHY it is this "
                    + "balanced: every draw carries an EVEN goal total (h+h), so the old no-draws "
                    + "truncation was deleting even mass and had skewed parity to 64/36; restoring "
                    + "draws restored it to ~50/50. Expires at v2 pricing, same as BTTS — "
                    + $"{Reach(MarketKind.TotalGoalsOddEven)}",
                [MarketKind.BothTeamsToScore] =
                    "near-even two-way market: under exact de-vig it never strictly wins a tie and "
                    + "its odds never clear the longshot threshold, so a sharp correctly declines it "
                    + $"(M1, expires at v2 pricing) — {bttsEvidence}",
            };

            // The 1X2 split, reported because a leg count per KIND cannot answer it. The draw is a
            // CHOICE inside the moneyline (D1, Allen 2026-08-12), so "Moneyline: 3,724 legs" is
            // true whether the bot took the draw 3,724 times or never. G7 exists to stop a market
            // being invisibly untouched, and the newest market on the board is exactly the one that
            // would hide here. This is telemetry, not a criterion: what the right draw share IS is
            // Campaign A's reading and then Allen's, and inventing a band for it now would be the
            // "threshold set before the instrument was measured" mistake this campaign keeps paying
            // for.
            if (skilled.MarketExposure.TryGetValue(MarketKind.Moneyline, out MarketExposure? ml))
            {
                g.Notes.Add(ml.LegsPlaced == 0
                    ? "1X2 SPLIT: skilled placed no moneyline legs at all this run"
                    : $"1X2 SPLIT: skilled placed {ml.DrawLegs:N0} DRAW legs of {ml.LegsPlaced:N0} "
                      + $"moneyline legs ({100.0 * ml.DrawLegs / ml.LegsPlaced:F1}%) — telemetry for "
                      + "the draws re-baseline, not a gate criterion");
            }

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

        // G7's SGP ARM (F_0.6.0 step 4). G7 above keys on MarketKind, and a SAME MATCH ticket is a
        // ticket SHAPE rather than a market — its legs are ordinary Total Goals / BTTS / Scorer rows,
        // so no MarketKind column can ever show one. Without its own criterion the campaign would
        // report full market coverage while the whole joint-pricing feature went unexercised, which
        // is the silent skip G7 exists to prevent, arriving through the one door G7 cannot watch.
        //
        // Two facts, both structural, both pass/fail:
        //   • the probe PLACED and SETTLED same-match tickets — placement alone would certify the
        //     pricing path while leaving void, grading and payout untouched;
        //   • ZERO tickets were sold at the no-label naive-product fallback. A silent fallback sells
        //     a correlated ticket at the product of its legs — worth up to +274% EV to the player on
        //     an implication pair — and canon requires it COUNTED AND SURFACED by the campaign, not
        //     logged. This is the line that discharges that obligation.
        //
        // Relation-kind coverage is NOT gated here. It is reported as an exposure table, mirroring
        // G7's own note above: coverage is structural, while how thinly something is covered is a
        // reading you take from the table, not a verdict a gate can pronounce.
        //
        // MARKET-kind coverage IS gated, and the difference is not a taste. A relation is a property
        // of the model's vocabulary and is fixed until the vocabulary changes; the MARKET list grows
        // every time a lane ships one, and it grew from six kinds to fifteen while this probe's
        // catalogue still built tickets out of the original six. The campaign went on certifying
        // same-match pricing on a board that no longer existed, and nothing anywhere went red,
        // because nothing was checking. This arm is that check: a new market either reaches a
        // same-match ticket or it turns the campaign red until someone names it and says why.
        if (sameMatch != null)
        {
            bool placed = sameMatch.SameMatchPlaced > 0;
            bool settled = sameMatch.SameMatchSettled > 0;
            bool clean = noLabelFallbacks == 0;
            // Gated, not merely reported (F_0.6.0 phase 4): same-match cash-out prices off the
            // conditional joint, and the probe held every ticket to settlement until that price
            // existed. If it ever stops cashing out, the campaign silently returns to covering a
            // path it no longer exercises — which is the failure this arm was built to make loud.
            bool cashedOut = sameMatch.SameMatchCashedOut > 0;

            // Named exclusions, mirroring G7's bot-excluded list exactly: a kind that genuinely
            // cannot reach a same-match ticket is listed HERE with its reason and surfaced as a note,
            // never dropped from the roll-call. EMPTY as shipped — every one of the fifteen kinds is
            // reachable and the probe's catalogue reaches it — and it is not dead code: it is the
            // seam that keeps the next uncoverable market from being handled by weakening the
            // criterion. Do NOT add a kind here because it currently reads zero legs; zero legs is
            // what this arm is for.
            var sgpExcluded = new Dictionary<MarketKind, string>();

            var sgpUncovered = new List<MarketKind>();
            foreach (MarketKind kind in Enum.GetValues(typeof(MarketKind)))
            {
                if (sgpExcluded.TryGetValue(kind, out string? why))
                {
                    g.Notes.Add($"SAME-MATCH-EXCLUDED: {Report.MarketName(kind)} — {why}");
                    continue;
                }
                bool exercised = sameMatch.SameMatchKinds.TryGetValue(kind, out SameMatchKindExposure? e)
                    && e.Legs > 0;
                if (!exercised) sgpUncovered.Add(kind);
            }
            bool allKinds = sgpUncovered.Count == 0;

            string shortfall = placed && settled && clean && allKinds && cashedOut
                ? ""
                : " — FAILED ON: "
                  + string.Join(", ", Missing(placed, settled, clean, cashedOut, sgpUncovered));

            g.Add("G7-SGP", "same-match coverage: the SAME MATCH probe placed AND settled same-match "
                + "tickets, EVERY shipped MarketKind reached a same-match ticket (or is on the named "
                + "exclusion list with a reason), and zero tickets were sold at the no-label "
                + "naive-product fallback (a ticket shape is invisible to G7's MarketKind roll-call; "
                + "a market the probe never pairs is a joint nothing priced; and a silent fallback is "
                + "a money leak worth up to +274% EV on an implication pair), AND same-match tickets "
                + "were cashed out (the conditional quote is live product code and a campaign that "
                + "never quotes it is not covering it)",
                placed && settled && clean && allKinds && cashedOut,
                $"placed {sameMatch.SameMatchPlaced:N0}, settled {sameMatch.SameMatchSettled:N0}, "
                + $"kinds covered {Enum.GetValues(typeof(MarketKind)).Length - sgpUncovered.Count - sgpExcluded.Count}"
                + $"/{Enum.GetValues(typeof(MarketKind)).Length - sgpExcluded.Count}, "
                + $"no-label fallbacks {noLabelFallbacks:N0}, refusals tripped "
                + $"{sameMatch.SameMatchRefusals:N0}, voids re-priced {sameMatch.SameMatchVoids:N0}, "
                + $"cashed out {sameMatch.SameMatchCashedOut:N0} "
                + $"({sameMatch.SameMatchCashOutsEarly:N0} early / "
                + $"{sameMatch.SameMatchCashedOut - sameMatch.SameMatchCashOutsEarly - sameMatch.SameMatchCashOutsLate:N0}"
                + $" mid / {sameMatch.SameMatchCashOutsLate:N0} last-leg)"
                + shortfall,
                // Same structural argument as G7's: a ticket was either sold or it was not.
                "exact — a ticket count is not a sample; no resolution limit");

            // The probe's own catalogue being refused is a DEFECT signal, not coverage, and it is
            // deliberately not folded into the arm's verdict — it is surfaced where an exclusion
            // note is surfaced, so it cannot hide inside a number the arm wants to be positive.
            if (sameMatch.SameMatchUnexpectedRefusals > 0)
                g.Notes.Add($"SAME MATCH: {sameMatch.SameMatchUnexpectedRefusals:N0} UNEXPECTED "
                    + "refusals — the probe built slips the board would not sell. Its catalogue or "
                    + "κ has moved; the relation exposure section is short by that much.");

            // CASH-OUT COVERAGE is REPORTED, not gated — deliberately, and the boundary is the same
            // one this arm already draws. The arm's own criteria are facts about the FEATURE (a shape
            // was sold, a market reached a joint, no ticket took the silent fallback); whether the
            // probe's own policy fired is a fact about the INSTRUMENT, and turning the campaign red
            // for it would be the arm judging its own bot. It is a hole worth seeing all the same:
            // the conditional quote (phase 4) is the newest priced path in the lane, and zero
            // cash-outs means it is proven by unit tests alone. Surfaced as a note, and the
            // settled/cashed balance is printed in report section 0b.
            if (sameMatch.SameMatchCashedOut == 0)
                g.Notes.Add("SAME MATCH: ZERO cash-outs taken — the conditional same-match cash-out "
                    + "path went unexercised end to end. The probe's policy fires on marked ticket "
                    + "slots only, so a shorter campaign can legitimately read low, but zero means "
                    + "the policy never reached a live quote at all.");
        }

        // G8-ARMA (T140 arm A): the sweat was restructured from per-LEG to per-(ticket, FIXTURE) —
        // a fixture is broadcast ONCE per ticket and every leg riding it grades at that one whistle.
        // The falsifier is simple and is the whole point: before arm A an N-leg fixture emitted N
        // LegFinal beats; after it, exactly one. G7 and G7-SGP cannot see this at all — both key on
        // MarketKind / ticket shape, not on how many beats one fixture's grading took.
        //
        // Read off the "samematch" batch, same as G7-SGP: it is the one bot whose catalogue actually
        // builds same-match tickets, i.e. fixtures carrying more than one leg — the only shape a
        // per-leg regression could hide inside. The coverage arm below is not decoration: without it
        // this gate would pass on a campaign that never built a same-match ticket at all, which is a
        // gate that cannot fail.
        if (sameMatch != null)
        {
            bool coverage = sameMatch.ArmASharedTellings > 0;
            bool noExtraWhistles = sameMatch.ArmAExtraWhistles == 0;
            bool noClockFaults = sameMatch.ArmAClockFaults == 0;
            bool noMismatches = sameMatch.ArmAWhistleGradeMismatches == 0;

            // THE SECOND COVERAGE ARM, AND IT IS NOT SYMMETRY FOR ITS OWN SAKE. The same-match probe's
            // catalogue builds tickets whose legs all ride ONE matchup, so on that batch every telling
            // is a shared one and `tellings == sharedTellings`. That proves N-legs-one-whistle and
            // proves NOTHING about the other half of the rule: a MULTI-fixture ticket, where the
            // fixture index advances and the clock legitimately restarts at 1 between tellings.
            //
            // `T140-am` is why that half needs a witness rather than an argument. A gate written "per
            // ticket" instead of "per (ticket, fixture)" reads a correct multi-fixture broadcast as a
            // rewind and fails it — the exact over-reach the spec was corrected for. The skilled batch
            // is where those tickets live (ordinary parlays across matchups), so the clock rules are
            // asserted THERE, over a population that actually has boundaries in it.
            bool boundaryCoverage = skilled != null && skilled.ArmAMultiFixtureTickets > 0;
            bool noBoundaryClockFaults = skilled == null || skilled.ArmAClockFaults == 0;
            bool noBoundaryExtraWhistles = skilled == null || skilled.ArmAExtraWhistles == 0;

            bool armAPass = coverage && noExtraWhistles && noClockFaults && noMismatches
                && boundaryCoverage && noBoundaryClockFaults && noBoundaryExtraWhistles;

            long midWhistleGrades = sameMatch.ArmASharedWhistleGradesLanded;
            long midWhistleExpected = sameMatch.ArmASharedWhistleLegsExpected;

            string shortfall = armAPass
                ? ""
                : " — FAILED ON: " + string.Join(", ",
                    MissingArmA(coverage, noExtraWhistles, noClockFaults, noMismatches,
                        boundaryCoverage, noBoundaryClockFaults, noBoundaryExtraWhistles));

            g.Add("G8-ARMA", "T140 arm A restructure: the sweat resolves per (ticket, FIXTURE), not "
                + "per leg — every leg riding one fixture is live for that fixture's whole telling "
                + "and grades at its single whistle. A fixture emitting more than one LegFinal means "
                + "the per-leg sweat is still running; a clock fault is T135's rewind returned "
                + "(checked on multi-fixture tickets too, where the fixture index legitimately "
                + "advances). Passes only when the campaign actually exercised a shared telling "
                + "(sharedTellings > 0 — the coverage arm; without it this gate would pass on a "
                + "campaign that never built a same-match ticket, i.e. a gate that cannot fail), zero "
                + "fixtures emitted an extra whistle, zero clock faults, and every shared whistle "
                + "graded exactly its own live legs. The clock rules are asserted a SECOND time over "
                + "the skilled batch, which is where MULTI-fixture tickets live: the same-match probe "
                + "builds only single-fixture tickets, so on its own it witnesses N-legs-one-whistle "
                + "and never a fixture BOUNDARY — and T140-am's over-reach (reading a correct "
                + "multi-fixture broadcast as a rewind) can only be ruled out on tickets that have "
                + "one",
                armAPass,
                $"tellings {sameMatch.ArmATellings:N0}, shared tellings {sameMatch.ArmASharedTellings:N0}, "
                + $"whistles {sameMatch.ArmAWhistles:N0}, extra whistles {sameMatch.ArmAExtraWhistles:N0}, "
                + $"clock faults {sameMatch.ArmAClockFaults:N0}, grades landed at shared whistles "
                + $"{midWhistleGrades:N0} (expected {midWhistleExpected:N0}), mismatches "
                + $"{sameMatch.ArmAWhistleGradeMismatches:N0}, windows opened "
                + $"{sameMatch.ArmAWindowsOpened:N0}, multi-death windows "
                + $"{sameMatch.ArmAMultiDeathWindows:N0}"
                + (skilled == null
                    ? " | boundary arm: NO SKILLED BATCH"
                    : $" | boundary arm (skilled): multi-fixture tickets "
                      + $"{skilled.ArmAMultiFixtureTickets:N0}, tellings {skilled.ArmATellings:N0}, "
                      + $"clock faults {skilled.ArmAClockFaults:N0}, extra whistles "
                      + $"{skilled.ArmAExtraWhistles:N0}")
                + shortfall,
                // Same structural argument as G7 / G7-SGP: a beat count is not a sample.
                "exact — a beat count is not a sample; no resolution limit");
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

    /// <summary>Which halves of G7's SGP arm failed. Named individually rather than collapsed into
    /// one "did not cover" — "nothing was placed" and "everything was placed but a fallback fired"
    /// are different defects with different fixes, and a verdict that cannot tell them apart sends
    /// the reader back to the code to find out which one happened.</summary>
    private static IEnumerable<string> Missing(bool placed, bool settled, bool clean, bool cashedOut,
        IReadOnlyList<MarketKind> uncoveredKinds)
    {
        if (!placed) yield return "no same-match ticket was placed";
        if (!settled) yield return "no same-match ticket settled";
        if (!clean) yield return "tickets were SOLD at the no-label naive product";
        if (!cashedOut) yield return "no same-match ticket was cashed out (the conditional quote is unexercised)";
        // Named, never counted: "3 kinds uncovered" sends the reader to the exposure table to find
        // out which, and the whole point of this arm is that the answer arrives with the verdict.
        if (uncoveredKinds.Count > 0)
            yield return "never in a same-match ticket: "
                + string.Join(", ", uncoveredKinds.Select(Report.MarketName));
    }

    /// <summary>Which condition(s) of G8-ARMA failed. Named individually for the same reason as
    /// <see cref="Missing"/> above it: "the gate failed" sends the reader back to the counters to
    /// find out which invariant broke, and the whole point of a falsifier is that the answer arrives
    /// with the verdict.</summary>
    private static IEnumerable<string> MissingArmA(bool coverage, bool noExtraWhistles,
        bool noClockFaults, bool noMismatches, bool boundaryCoverage, bool noBoundaryClockFaults,
        bool noBoundaryExtraWhistles)
    {
        if (!coverage) yield return "no shared telling was ever exercised (the gate decided nothing)";
        if (!noExtraWhistles) yield return "a fixture emitted more than one whistle (the per-leg sweat is still running)";
        if (!noClockFaults) yield return "the rendered clock regressed inside a telling (T135's rewind)";
        if (!noMismatches) yield return "a shared whistle did not grade exactly its own live legs";
        if (!boundaryCoverage) yield return "no MULTI-fixture ticket was ever swept, so the fixture boundary is unwitnessed (T140-am)";
        if (!noBoundaryClockFaults) yield return "a clock fault across a fixture boundary on the skilled batch";
        if (!noBoundaryExtraWhistles) yield return "an extra whistle on the skilled batch";
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
