using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>Renders the markdown balance report. Sections are numbered to match design/02's
/// "What Monte Carlo must answer". The body (everything after the header) is a pure function of the
/// batch summaries, so --verify can compare two bodies byte-for-byte.</summary>
public static class Report
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    // The relic Price lookup that lived here is gone with ComboTag's price clause (Allen
    // 2026-08-08). It had exactly one reader, and that reader compared comps prices against a
    // cash-scale constant. A lookup kept "in case" is how the next such comparison gets written.
    private static readonly Dictionary<string, string> RelicName = BuildNames();

    // ---- public entry points ----

    public static string Full(CliOptions opt, IReadOnlyList<BatchSummary> batches,
        AuditData? audit, ComboData? combos, GateData? gates, string date, double wallSeconds,
        RunConfig cfg, long totalRuns)
    {
        var sb = new StringBuilder();
        sb.Append(Header(opt, date, wallSeconds, cfg, batches, totalRuns));
        sb.Append(Body(opt, batches, audit, combos, gates));
        return sb.ToString();
    }

    /// <summary>Deterministic body only (no date / wall time) — the surface --verify diffs.</summary>
    public static string Body(CliOptions opt, IReadOnlyList<BatchSummary> batches,
        AuditData? audit, ComboData? combos, GateData? gates)
    {
        var sb = new StringBuilder();
        GatesSection(sb, gates);
        SameMatchSection(sb, batches);
        Survival(sb, batches);
        MarketExposure(sb, batches);
        ItemAudit(sb, audit);
        Variance(sb, batches);
        Ratchet(sb, batches);
        EvArc(sb, batches);
        BandThree(sb, batches, combos);
        Grind(sb, batches);
        return sb.ToString();
    }

    // ---- 0. gates ----

    private static void GatesSection(StringBuilder sb, GateData? gates)
    {
        if (gates == null) return;
        sb.AppendLine("## 0. Gate campaign (PLAN.md acceptance criteria)");
        sb.AppendLine();
        // C32: the resolution travels WITH the verdict, in the same table. A PASS whose resolution
        // is coarser than the drift it exists to catch is not a clean result, and putting the
        // number in a footnote is how that goes unread.
        sb.AppendLine("| Gate | Criterion | Verdict | Actual | Resolution |");
        sb.AppendLine("|---|---|---|---|---|");
        foreach (GateData.Gate g in gates.Gates)
            sb.AppendLine($"| {g.Id} | {g.Description} | {(g.Pass ? "**PASS**" : "**FAIL**")} | {g.Actual} "
                + $"| {(g.Resolution.Length == 0 ? "—" : g.Resolution)} |");
        sb.AppendLine();
        // C28/C29: the campaign states how many gates ran, how many passed and how many actually
        // produced a verdict. A table nobody counted is a count nobody stated, and a gate that
        // could not separate its reading from its own criterion line is not one of the passes.
        int total = gates.Gates.Count, passed = 0, adjudicated = gates.AdjudicatedCount;
        var unadjudicated = new List<string>();
        foreach (GateData.Gate g in gates.Gates)
        {
            if (g.Pass) passed++;
            if (!g.Adjudicated) unadjudicated.Add(g.Id);
        }
        // C28 names every non-verdict rather than leaving a reader to find it — a count that says
        // "one of these decided nothing" without saying which is a count you cannot act on.
        sb.AppendLine($"Gates evaluated: **{total}** · passed: **{passed}** · produced a verdict: "
            + $"**{adjudicated}**"
            + (unadjudicated.Count == 0
                ? "."
                : $" — **{unadjudicated.Count} NOT ADJUDICATED: {string.Join(", ", unadjudicated)}** "
                  + "(reading within its own resolution of the criterion line; see Resolution)."));
        sb.AppendLine();
        if (gates.ItemFlags.Count == 0)
            sb.AppendLine("Item flags: none — no DEAD items, no DOMINANT item, Totem in the healthy band.");
        else
            foreach (string flag in gates.ItemFlags)
                sb.AppendLine($"- ⚑ {flag}");
        foreach (string note in gates.Notes)
            sb.AppendLine($"- ℹ {note}");
        sb.AppendLine();
        // The banner is what a human reads, so it is the one line that must not overstate. A
        // campaign carrying an unadjudicated gate has not passed everything it ran — it has a
        // re-run owed at Allen's escalation size (2026-08-07). Exit code is deliberately unchanged:
        // he ruled a re-run, not a failure.
        string banner = !gates.AllPass || gates.ItemFlags.Count > 0
            ? "NOT DONE — iterate the knobs (item numbers / prices / curve) and rerun."
            : adjudicated == total
                ? $"ALL {total} GATES PASS — the economy holds."
                : $"{passed}/{total} GATES PASS, but {string.Join(", ", unadjudicated)} DID NOT "
                  + $"ADJUDICATE — re-run at `--runs {GateData.EscalationRuns}` (Allen's recorded "
                  + "escalation, 2026-08-07) before reading this campaign as clean.";
        sb.AppendLine($"> **{banner}**");
        sb.AppendLine();
    }

    // ---- 0b. SAME MATCH relation exposure ----

    /// <summary>The relation-kind exposure table G7's SGP arm points at (F_0.6.0 step 4).
    ///
    /// <para><b>Informational, deliberately.</b> It mirrors G7's own stated split: whether a thing is
    /// covered is STRUCTURAL and belongs in a gate, while how THINLY it is covered is a reading, and
    /// a reading is a table. A relation seen once is covered — and covered once is worth knowing,
    /// which is exactly what a pass/fail cannot say.</para>
    ///
    /// <para>Kinds are the model's own vocabulary, printed verbatim rather than through a display
    /// map: these names are structured data the engine emits, and a second spelling of them here is
    /// a thing that can drift away from the enum it describes.</para></summary>
    private static void SameMatchSection(StringBuilder sb, IReadOnlyList<BatchSummary> batches)
    {
        BatchSummary? probe = null;
        foreach (BatchSummary b in batches)
            if (b.SameMatchPlaced > 0 || b.SameMatchRefusals > 0)
            {
                probe = b;
                break;
            }
        if (probe == null) return; // no bot built a same-match ticket in this run — nothing to read

        sb.AppendLine("## 0b. SAME MATCH exposure (informational — NOT a gate)");
        sb.AppendLine();
        sb.AppendLine($"From the `{probe.Name}` batch. Whether the feature is covered is G7-SGP's "
            + "verdict; how thinly each relation is covered is this table's, and the two are "
            + "deliberately different instruments.");
        sb.AppendLine();
        sb.AppendLine($"Tickets placed: **{probe.SameMatchPlaced:N0}** · settled: "
            + $"**{probe.SameMatchSettled:N0}** · legs voided and re-priced: "
            + $"**{probe.SameMatchVoids:N0}** · refusals tripped: **{probe.SameMatchRefusals:N0}**"
            + (probe.SameMatchUnexpectedRefusals > 0
                ? $" · ⚑ unexpected refusals: **{probe.SameMatchUnexpectedRefusals:N0}**"
                : ""));
        sb.AppendLine();
        sb.AppendLine("| Relation | Relations priced | Tickets carrying it | Times principal |");
        sb.AppendLine("|---|---:|---:|---:|");

        var unexercised = new List<string>();
        foreach (RelationKind kind in Enum.GetValues<RelationKind>())
        {
            probe.SameMatchRelations.TryGetValue(kind, out SameMatchExposure? e);
            if (e == null || e.Relations == 0)
            {
                unexercised.Add(kind.ToString());
                continue;
            }
            sb.AppendLine($"| {kind} | {e.Relations:N0} | {e.Tickets:N0} | {e.Principal:N0} |");
        }
        sb.AppendLine();

        // The refusal rules are exposure too, and they belong beside the relations rather than in
        // the gate table: a rule that never fired is a hole of exactly the same kind as a relation
        // that was never priced. SubEvens is expected to read zero at the shipped κ = 1 — the price
        // it judges cannot get that low — and that absence is a statement, so it is printed.
        var rules = new List<string>();
        foreach (RefusalKind kind in Enum.GetValues<RefusalKind>())
            rules.Add($"{kind} × "
                + (probe.SameMatchRefusalKinds.TryGetValue(kind, out int n) ? n : 0).ToString("N0", Inv));
        sb.AppendLine($"Refusal rules exercised: {string.Join(" · ", rules)}. "
            + $"{RefusalKind.SubEvens} reads zero at the shipped κ = 1 by construction — the "
            + "sub-evens price and its full-ticket refund need κ ≳ 1.3, so that path stays "
            + "unit-test-only in this campaign.");
        sb.AppendLine();
        if (unexercised.Count > 0)
            sb.AppendLine($"Not exercised: {string.Join(", ", unexercised)}. "
                + $"{RelationKind.MutuallyExclusive} can never appear here by construction — it is "
                + "the label on a combination the engine REFUSES, so it is never on a placed ticket; "
                + "the refusal counters above are where it is read. Any other name in this line is a "
                + "real hole in the probe's catalogue.");
        else
            sb.AppendLine("Every relation kind the model can label was priced at least once.");
        sb.AppendLine();
    }

    // ---- header ----

    private static string Header(CliOptions opt, string date, double wallSeconds, RunConfig cfg,
        IReadOnlyList<BatchSummary> batches, long total)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# /sim — Monte Carlo balance report");
        sb.AppendLine();
        sb.AppendLine($"- Date: {date}");
        sb.AppendLine("- Engine: workspace is not a git repo — `git describe` unavailable");
        double meanRamp = cfg.Payments.Length > 1
            ? Math.Pow(cfg.Payments[^1] / cfg.Payments[0], 1.0 / (cfg.Payments.Length - 1))
            : 1.0;
        sb.AppendLine($"- Config: bank {Money(cfg.StartingBank)}, PAYMENTS [{Payments(cfg)}] (avg ×{meanRamp.ToString("F2", Inv)}), "
            + $"overround {Pct(cfg.Overround * 100)}, "
            // The SAME MATCH margin dial travels with the artifact (F_0.6.0 step 4). A campaign that
            // validates a dial without recording where the dial was set cannot be read later as
            // having validated anything in particular.
            + $"SGP margin κ {cfg.SgpMargin.ToString("0.0##", Inv)}, "
            + $"cash-out margin {Pct(cfg.CashOutMargin * 100)}, "
            + $"totem juice {Pct(cfg.TotemJuiceRate * 100)}, "
            + $"min stake {Money(cfg.MinStake)}, max stake {Pct(cfg.MaxStakeFraction * 100)} of bank, "
            + $"{cfg.MatchupsPerSlate} matchups/round, {cfg.MaxTicketsPerRound} tickets/round, "
            + $"{cfg.RelicSlots} relic + {cfg.ConsumableSlots} consumable slots");
        sb.AppendLine($"- Strategies: {string.Join(", ", Names(batches))}");
        // C34 (batch 14): evidence that cannot be reproduced is not a set — a flow pins its seed
        // AND asserts it. This campaign was always pinned by construction, but until this line the
        // report never recorded WHICH prefix, so every gate table's pinning lived in the prose
        // wrapped around the artifact instead of in the artifact. --scorer-ev printed its prefix;
        // the campaign the gates are actually read off did not.
        sb.AppendLine($"- Seed: **pinned** — run i uses engine seed \"{opt.SeedPrefix}-{{i}}\". Same "
            + "arguments reproduce this report's body byte-for-byte; the header's date and wall time "
            + "are the exceptions and carry no verdict. `--verify` is the standing self-check.");
        sb.AppendLine($"- Runs per batch: {opt.Runs.ToString("N0", Inv)}");
        sb.AppendLine($"- Total runs (incl. audit/combos): {total.ToString("N0", Inv)}");
        sb.AppendLine($"- Wall time: {wallSeconds.ToString("F2", Inv)} s");
        sb.AppendLine();
        return sb.ToString();
    }

    // ---- 1. survival ----

    private static void Survival(StringBuilder sb, IReadOnlyList<BatchSummary> batches)
    {
        sb.AppendLine("## 1. Survival curves");
        sb.AppendLine();
        sb.AppendLine("Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).");
        sb.AppendLine();

        sb.Append("| Metric |");
        foreach (BatchSummary b in batches) sb.Append($" {b.Name} |");
        sb.AppendLine();
        sb.Append("|---|");
        foreach (var _ in batches) sb.Append("---|");
        sb.AppendLine();

        for (int r = 1; r <= 8; r++)
        {
            sb.Append($"| enter R{r} |");
            foreach (BatchSummary b in batches)
                sb.Append($" {Pct(100.0 * b.AliveEntering[r] / b.N)} |");
            sb.AppendLine();
        }
        sb.Append("| **won %** |");
        foreach (BatchSummary b in batches) sb.Append($" **{Pct(b.WonPct)}** |");
        sb.AppendLine();
        sb.Append("| **median death round** |");
        foreach (BatchSummary b in batches) sb.Append($" **{MedianDeath(b.MedianDeath)}** |");
        sb.AppendLine();
        sb.Append("| mean rounds reached |");
        foreach (BatchSummary b in batches) sb.Append($" {b.MeanDeath.ToString("F2", Inv)} |");
        sb.AppendLine();
        // Payment-model telemetry: totem saves, near-miss deaths, and the pity channel firing.
        sb.Append("| totem fire rate |");
        foreach (BatchSummary b in batches) sb.Append($" {Pct(b.TotemFireRate)} |");
        sb.AppendLine();
        sb.Append("| close-call deaths (% of deaths) |");
        foreach (BatchSummary b in batches) sb.Append($" {Pct(b.CloseCallDeathPct)} |");
        sb.AppendLine();
        sb.Append("| mean bookie gifts per run |");
        foreach (BatchSummary b in batches) sb.Append($" {b.MeanGifts.ToString("F2", Inv)} |");
        sb.AppendLine();
        sb.AppendLine();

        BatchSummary? naive = Find(batches, "naive");
        BatchSummary? skilled = Find(batches, "skilled");
        sb.AppendLine($"> Takeaway: {SurvivalTakeaway(naive, skilled)}");
        sb.AppendLine();
    }

    private static string SurvivalTakeaway(BatchSummary? naive, BatchSummary? skilled)
    {
        if (naive == null || skilled == null) return "run --strategy all for the S3/S4 verdicts.";
        string n = naive.MedianDeath.ToString("0.#", Inv);
        string s = MedianDeath(skilled.MedianDeath);
        string gap = skilled.MedianDeath > naive.MedianDeath ? "skill buys real extra survival" : "skill barely moves the needle";
        return $"naive dies at round {n}, skilled reaches {s} — {gap}; compare against the 3–4 / ≥7 targets above.";
    }

    // ---- market exposure ----

    private static void MarketExposure(StringBuilder sb, IReadOnlyList<BatchSummary> batches)
    {
        sb.AppendLine("## 2. Market exposure");
        sb.AppendLine();
        sb.AppendLine("Placed legs and equal-split stake share by market kind. `mean leg EV` is the "
            + "UNWEIGHTED per-leg return (converges to −vig under fair pricing — the sanity column); "
            + "`stake-wtd EV` is stake-weighted and fat-tailed under compounding banks (a few monster "
            + "tickets dominate it — read it as variance, not edge). Both are single-leg, before "
            + "parlay multiplication, cash-outs, voids, or relic factors.");
        sb.AppendLine();
        sb.AppendLine("| Strategy | market | legs placed | stake share | mean leg EV | stake-wtd EV |");
        sb.AppendLine("|---|---|---|---|---|---|");
        foreach (BatchSummary batch in batches)
        {
            double totalStake = 0.0;
            foreach (MarketExposure exposure in batch.MarketExposure.Values) totalStake += exposure.Stake;
            foreach (MarketKind kind in Enum.GetValues(typeof(MarketKind)))
            {
                batch.MarketExposure.TryGetValue(kind, out MarketExposure? exposure);
                int legs = exposure?.LegsPlaced ?? 0;
                double stake = exposure?.Stake ?? 0.0;
                string share = totalStake == 0.0 ? "0.0%" : Pct(100.0 * stake / totalStake);
                string meanEv = legs == 0 ? "—" : SignedPct(100.0 * exposure!.RealizedNetUnit / legs);
                string wtdEv = stake == 0.0 ? "—" : SignedPct(100.0 * exposure!.RealizedNet / stake);
                sb.AppendLine($"| {batch.Name} | {MarketName(kind)} | {legs.ToString("N0", Inv)} | {share} | {meanEv} | {wtdEv} |");
            }
        }
        sb.AppendLine();
        sb.AppendLine("> Anytime Scorer is intentionally zero: it is a declared human-agency market, excluded from every bot.");
        sb.AppendLine();
    }

    // ---- scorer calibration (--scorer-ev; its own standalone mode — see Program.ScorerEv) ----

    /// <summary>Renders the --scorer-ev report. Deliberately outside Full/Body: this mode never
    /// runs alongside the batch report, carries no date/wall-time, and has no reason to enter the
    /// --verify byte-diff surface those two already own — Program.ScorerEv diffs this output
    /// directly instead.</summary>
    public static string ScorerEv(ScorerCalibrationData data, RunConfig cfg)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Scorer calibration");
        sb.AppendLine();
        double r = cfg.Overround;
        double expectedEvPp = 100.0 * (1.0 / (1.0 + r) - 1.0);
        sb.AppendLine("Every bot is policy-excluded from pricing Anytime Scorer (a declared "
            + "human-agency market), so no strategy can de-vig it and no gate can verify it that "
            + "way. This measures the one thing that needs no strategy: does the probability the "
            + "engine PRICES a scorer at (`Matchup.TrueProb`) match the frequency the engine's own "
            + "sampler REALISES for that player (`SampleStatLine` + `SampleScorers`)? Each offer is "
            + $"resampled {ScorerCalibrationData.SamplesPerMatchup} times on a stream derived from "
            + "the run seed — never the sequential Outcomes/Slate streams — so measuring an offer "
            + "can never perturb the slate it was priced from. Under correct pricing at overround "
            + $"{Pct(r * 100.0)}, realised EV should sit at 1/(1+r) − 1 = **{expectedEvPp.ToString("F2", Inv)}pp** "
            + "— the same figure every two-way market in this sim converges to.");
        sb.AppendLine();

        sb.AppendLine("| priced p band | offers | samples | mean priced p | realised freq | Δ (pp) | realised EV (pp) | freq SE | EV SE |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|");
        foreach (ScorerCalibrationData.Bucket b in data.ByProbabilityBand())
            AppendCalibrationRow(sb, b);
        sb.AppendLine();

        sb.AppendLine("By role — role sets the base scoring weight and per-player jitter spreads "
            + "players within it, so this split is the fastest check for whether a miscalibration "
            + "is role-shaped rather than a general drift. What it cannot see: a miss confined to "
            + "one player inside a role, which pools away here and needs the band table:");
        sb.AppendLine();
        sb.AppendLine("| role | offers | samples | mean priced p | realised freq | Δ (pp) | realised EV (pp) | freq SE | EV SE |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|");
        foreach (ScorerCalibrationData.Bucket b in data.ByRole())
            AppendCalibrationRow(sb, b);
        sb.AppendLine();

        sb.AppendLine("What this instrument cannot see:");
        sb.AppendLine("- It measures the engine against itself (priced probability vs. that same "
            + "engine's own sampler) — a shared upstream error in the scoring-weight model would "
            + "be invisible to it.");
        sb.AppendLine("- It says nothing about whether the market is FUN, or well-priced against a "
            + "human reader of the odds — only whether the engine's two halves agree with each other.");
        sb.AppendLine("- It does not exercise settlement or the parlay path — every number here is "
            + "single-leg and pre-stake.");
        sb.AppendLine("- A bucket pools many different players and matchups; its sample count is "
            + "TOTAL resamples, not one player measured repeatedly — read the by-role table before "
            + "blaming one player for a bucket's number.");
        sb.AppendLine();

        sb.AppendLine($"> Takeaway: {ScorerEvTakeaway(data, 1.0 / (1.0 + r) - 1.0)}");
        sb.AppendLine();
        return sb.ToString();
    }

    private static void AppendCalibrationRow(StringBuilder sb, ScorerCalibrationData.Bucket b)
    {
        if (b.OfferCount == 0)
        {
            sb.AppendLine($"| {b.Label} | 0 | 0 | — | — | — | — | — |");
            return;
        }
        sb.AppendLine($"| {b.Label} | {b.OfferCount.ToString("N0", Inv)} | {b.SampleCount.ToString("N0", Inv)} "
            + $"| {Pct(b.MeanPricedProb * 100.0)} | {Pct(b.RealizedFreq * 100.0)} "
            + $"| {SignedPct(100.0 * (b.RealizedFreq - b.MeanPricedProb))} "
            + $"| {SignedPct(b.EvFraction * 100.0)} | ±{(b.SeFraction * 100.0).ToString("F1", Inv)}pp | ±{(b.EvSeFraction * 100.0).ToString("F1", Inv)}pp |");
    }

    /// <summary>An honest read of the buckets above, not a hand-authored verdict: flags bands
    /// whose |Δ| exceeds 2 SE (≈95% two-sided) rather than asserting calibration either way.</summary>
    private static string ScorerEvTakeaway(ScorerCalibrationData data, double expectedEv)
    {
        int populated = 0, outside2Se = 0;
        double worstZ = 0;
        string worstLabel = "";
        foreach (ScorerCalibrationData.Bucket b in data.ByProbabilityBand())
        {
            if (b.OfferCount == 0 || b.SeFraction <= 0.0) continue;
            populated++;
            double z = Math.Abs(b.RealizedFreq - b.MeanPricedProb) / b.SeFraction;
            if (z > 2.0) outside2Se++;
            if (z > worstZ) { worstZ = z; worstLabel = b.Label; }
        }
        // The EV column needs its OWN verdict, judged against its OWN error. Calibration and EV
        // fairness are not the same claim: at long odds a frequency error far too small to show in
        // the Δ column is multiplied by the odds into a visible EV gap, and — the trap this seat
        // fell into — an EV gap far smaller than the EV's own error looks exactly like a finding
        // if the only SE printed beside it belongs to the frequency.
        int evOutside2Se = 0;
        double worstEvZ = 0;
        string worstEvLabel = "";
        foreach (ScorerCalibrationData.Bucket b in data.ByProbabilityBand())
        {
            if (b.OfferCount == 0 || b.EvSeFraction <= 0.0) continue;
            double evZ = Math.Abs(b.EvFraction - expectedEv) / b.EvSeFraction;
            if (evZ > 2.0) evOutside2Se++;
            if (evZ > worstEvZ) { worstEvZ = evZ; worstEvLabel = b.Label; }
        }

        if (populated == 0) return "no offers sampled — widen --runs.";
        if (outside2Se == 0 && evOutside2Se == 0)
            return $"all {populated} probability bands sit within 2 SE of their priced probability, "
                + $"and every band's realised EV is within 2 SE of {(expectedEv * 100.0).ToString("F2", Inv)}pp "
                + $"(worst {worstEvLabel} at {worstEvZ.ToString("F1", Inv)} SE) — calibration and EV "
                + "fairness both hold at this sample size.";
        if (outside2Se == 0)
            return $"all {populated} bands are calibrated, but {evOutside2Se} band(s) sit outside 2 SE "
                + $"on realised EV (worst: {worstEvLabel} at {worstEvZ.ToString("F1", Inv)} SE) — the "
                + "price is honest about frequency and still not returning the intended vig there.";
        return $"{outside2Se}/{populated} probability bands sit outside 2 SE of their priced "
            + $"probability (worst: {worstLabel} at {worstZ.ToString("F1", Inv)} SE) — price and "
            + "sampler disagree there, not just noise.";
    }

    // ---- 2. item audit ----

    private static void ItemAudit(StringBuilder sb, AuditData? audit)
    {
        sb.AppendLine("## 2. Item power audit (3 passives + 3 consumables)");
        sb.AppendLine();
        if (audit == null)
        {
            sb.AppendLine("_Not run — pass `--audit` (or `--gates`) to grant each item free to skilled and measure it._");
            sb.AppendLine();
            return;
        }

        sb.AppendLine($"Skilled baseline: median death {MedianDeath(audit.Baseline.MedianDeath)}, "
            + $"mean rounds {audit.Baseline.MeanDeath.ToString("F2", Inv)}, won {Pct(audit.Baseline.WonPct)}. "
            + "Passives granted at run start; consumables refilled every round. Exposure = uses "
            + "(consumables) or wound-up runs (ratchets); ±  is the paired-seed SE on Δwon. "
            + "Sorted by Δ mean rounds survived.");
        sb.AppendLine();
        sb.AppendLine("| Item | kind | mean rounds | Δ mean | median death | won % | Δ won % (±SE) | exposure | totem fires |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|");
        foreach (AuditData.Entry e in audit.Entries)
        {
            string totem = e.Id == RelicCatalog.TotemId ? Pct(e.TotemFireRate) : "—";
            string exposure = e.IsConsumable ? $"{e.Used:N0} uses"
                : e.StatePositiveRuns > 0 ? $"{e.StatePositiveRuns:N0} wound" : "—";
            sb.AppendLine($"| {e.Name} | {(e.IsConsumable ? "consumable" : "passive")} "
                + $"| {e.MeanDeath.ToString("F2", Inv)} | {Signed(e.MeanDelta)} "
                + $"| {MedianDeath(e.MedianDeath)} | {Pct(e.WonPct)} "
                + $"| {SignedPct(e.WonDelta)} (±{e.WonDeltaSe.ToString("0.0", Inv)}) | {exposure} | {totem} |");
        }
        sb.AppendLine();
        sb.AppendLine("> Takeaway: the gate section's item flags carry the verdicts (DEAD / DOMINANT / totem band). "
            + "Note the audit grants items FREE — organic play also pays shop prices out of payment headroom.");
        sb.AppendLine();
    }

    // ---- 3. variance ----

    private static void Variance(StringBuilder sb, IReadOnlyList<BatchSummary> batches)
    {
        sb.AppendLine("## 3. Variance feel");
        sb.AppendLine();
        sb.AppendLine("Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.");
        sb.AppendLine();
        sb.AppendLine("| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|");
        foreach (BatchSummary b in batches)
        {
            string wb10, wb50, wb90, wb99;
            if (b.WinningFinalBanks.Count == 0)
                wb10 = wb50 = wb90 = wb99 = "n/a";
            else
            {
                wb10 = Money(Stats.Percentile(b.WinningFinalBanks, 10));
                wb50 = Money(Stats.Percentile(b.WinningFinalBanks, 50));
                wb90 = Money(Stats.Percentile(b.WinningFinalBanks, 90));
                wb99 = Money(Stats.Percentile(b.WinningFinalBanks, 99));
            }
            sb.AppendLine($"| {b.Name} | {Money(Stats.Percentile(b.BiggestSwings, 10))} "
                + $"| {Money(Stats.Percentile(b.BiggestSwings, 50))} | {Money(Stats.Percentile(b.BiggestSwings, 90))} "
                + $"| {Money(Stats.Percentile(b.BiggestSwings, 99))} | {wb10} | {wb50} | {wb90} | {wb99} |");
        }
        sb.AppendLine();
        sb.AppendLine("> Takeaway: swing p50 vs p99 shows whether the ride is flat (boring) or spiky (feels rigged); "
            + "a p99 many multiples of p50 is the intended rare-blowup shape.");
        sb.AppendLine();
    }

    // ---- 4. ratchet ----

    private static void Ratchet(StringBuilder sb, IReadOnlyList<BatchSummary> batches)
    {
        sb.AppendLine("## 4. Ratchet telemetry (Scar Tissue)");
        sb.AppendLine();
        sb.AppendLine("Mean peak stacks (pp) and mean carrier burns per run — is the ratchet winding and cashing?");
        sb.AppendLine();
        sb.AppendLine("| Strategy | mean peak stacks | mean burns/run |");
        sb.AppendLine("|---|---|---|");
        foreach (BatchSummary b in batches)
            sb.AppendLine($"| {b.Name} | {b.MeanMaxScar.ToString("F1", Inv)}pp | {b.MeanScarBurns.ToString("F2", Inv)} |");
        sb.AppendLine();
        sb.AppendLine("> Takeaway: zero across the board means nobody buys/holds the scar (price or power problem); "
            + "huge stacks with zero burns means winding without cashing (the carrier rule isn't landing).");
        sb.AppendLine();
    }

    // ---- 5. EV arc ----

    private static void EvArc(StringBuilder sb, IReadOnlyList<BatchSummary> batches)
    {
        sb.AppendLine("## 5. EV-arc");
        sb.AppendLine();
        sb.AppendLine("Mean *true* per-ticket EV at lock, by round (the economy doctrine's crossing from −vig toward positive).");
        sb.AppendLine();

        sb.Append("| Round |");
        foreach (BatchSummary b in batches) sb.Append($" {b.Name} mean EV |");
        sb.AppendLine();
        sb.Append("|---|");
        foreach (var _ in batches) sb.Append("---|");
        sb.AppendLine();
        for (int r = 1; r <= 8; r++)
        {
            sb.Append($"| R{r} |");
            foreach (BatchSummary b in batches)
            {
                if (b.EvSampleByRound[r] == 0) sb.Append(" — |");
                else sb.Append($" {Money(b.MeanEvByRound[r])} |");
            }
            sb.AppendLine();
        }
        sb.AppendLine();

        BatchSummary? skilled = Find(batches, "skilled");
        BatchSummary? naive = Find(batches, "naive");
        if (skilled != null)
        {
            int cross = skilled.EvZeroCrossRound();
            sb.AppendLine(cross == 0
                ? "- Skilled mean EV never crosses zero (target ≈ round 4)."
                : $"- Skilled mean EV first crosses zero at **round {cross}** (target ≈ round 4).");
            if (cross >= 2)
                sb.AppendLine($"- **Survivorship caveat:** the round-{cross}+ means average only the few runs that "
                    + $"got there (R{cross} n={skilled.EvSampleByRound[cross]} tickets vs R1 n={skilled.EvSampleByRound[1]}). "
                    + "The MEDIAN skilled run dies at round 2, so it never reaches the +EV band — the arc is real "
                    + "for the surviving tail, not the typical player.");
        }
        if (naive != null)
        {
            int cross = naive.EvZeroCrossRound();
            sb.AppendLine(cross == 0
                ? "- Naive mean EV never crosses zero — as intended."
                : $"- Naive mean EV crosses zero at round {cross} — NOT intended (naive should never cross).");
        }
        sb.AppendLine();
        sb.AppendLine($"> Takeaway: {EvTakeaway(skilled, naive)}");
        sb.AppendLine();
    }

    private static string EvTakeaway(BatchSummary? skilled, BatchSummary? naive)
    {
        if (skilled == null) return "add skilled to see the arc.";
        int cross = skilled.EvZeroCrossRound();
        if (cross == 0) return "skilled never turns +EV; naive stays underwater — relics aren't flipping the arc.";
        return $"among survivors the +EV relics do flip the arc (crosses ~R{cross}, target ≈R4), but almost nobody "
            + "survives to bank it — the mechanic works, the economy gates it away.";
    }

    // ---- 6. band 3 + combos ----

    private static void BandThree(StringBuilder sb, IReadOnlyList<BatchSummary> batches, ComboData? combos)
    {
        sb.AppendLine("## 6. Band-3 audit");
        sb.AppendLine();
        sb.AppendLine("Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).");
        sb.AppendLine();
        sb.AppendLine("| Strategy | p99 final bank | top-1% mean | max |");
        sb.AppendLine("|---|---|---|---|");
        foreach (BatchSummary b in batches)
        {
            double p99 = Stats.Percentile(b.FinalBanks, 99);
            (double mean, double max) = Top1(b.FinalBanks);
            sb.AppendLine($"| {b.Name} | {Money(p99)} | {Money(mean)} | {Money(max)} |");
        }
        sb.AppendLine();

        if (combos != null) Combos(sb, combos);
        else sb.AppendLine("_Combo scan not run — pass `--combos N` (e.g. 2000) for the pairwise synergy table._");
        sb.AppendLine();
        // The "hard-to-assemble vs trivially cheap" half of this takeaway went with ComboTag's
        // price clause: nothing in this report measures assembly difficulty, and saying it here
        // would restate the claim the tag was just stripped of one screen further down.
        sb.AppendLine("> Takeaway: brokenness should live in the tail (max ≫ p99) and, if combos ran, in a few "
            + "pairs that clear their own error rather than across the whole table.");
        sb.AppendLine();
    }

    private static void Combos(StringBuilder sb, ComboData combos)
    {
        sb.AppendLine();
        sb.AppendLine($"Pairwise relic synergy ({combos.RunsPerConfig.ToString("N0", Inv)} runs/config, baseline won {Pct(combos.BaselineWonPct)}). "
            + "Synergy excess = pair Δwon − (soloA Δ + soloB Δ). Top 10:");
        sb.AppendLine();
        sb.AppendLine("| Pair | pair won % | synergy excess (pp) | ±2 SE (paired) | tag |");
        sb.AppendLine("|---|---|---|---|---|");
        int shown = 0;
        foreach (ComboData.Pair p in combos.Pairs)
        {
            if (shown++ >= 10) break;
            string names = $"{RelicName.GetValueOrDefault(p.IdA, p.IdA)} + {RelicName.GetValueOrDefault(p.IdB, p.IdB)}";
            string se = double.IsNaN(p.SynergyExcessTwoSePp)
                ? "—"
                : $"±{p.SynergyExcessTwoSePp.ToString("0.00", Inv)}";
            sb.AppendLine($"| {names} | {Pct(p.PairWonPct)} | {Signed(p.SynergyExcess)} | {se} | {ComboTag(p)} |");
        }
        // The error column is not decoration: every excess in this table is a combination of four
        // measured rates, and the table ranked them for a fortnight with no way to tell a real
        // 2.96pp from a noisy one. G5 certifies a design pillar off one of these rows.
        sb.AppendLine();
        sb.AppendLine("Ranked by excess; the ±2 SE column is paired by seed, so it is the error of "
            + "the *combination*, not of any one arm. A row whose excess is inside its own error is "
            + "tagged as such and its rank means nothing.");
    }

    /// <summary>The pair's tag. **A taxonomy label is an instrument too** (Allen 2026-08-08) — it
    /// makes a claim, and it can be wrong in exactly the way a gate can.
    ///
    /// What this used to be: a 1pp cut, then a split on combined price ≤ 450 into "degenerate:
    /// cheap pair, trivially assembled" vs "delicious: costly pair, bounded by run length". Every
    /// relic is priced **2–7 COMPS**, so the largest pair in the catalog totals 14 — the branch was
    /// always true, "degenerate: cheap" printed on every pair above the line, and "delicious" was
    /// unreachable. A cash-scale threshold left behind when prices moved to comps (design/10 F),
    /// asserting cheapness it never measured. Allen ruled it dropped rather than re-scaled.
    ///
    /// "no real loop" went with it, and that is worth stating rather than slipping through: it was
    /// the same kind of claim — a statement about item interaction that this measurement cannot
    /// see. The tag now reports only what the numbers support, the excess against its own paired
    /// error, and says plainly when there is no signal to report at all.</summary>
    private static string ComboTag(ComboData.Pair p)
    {
        double se = p.SynergyExcessTwoSePp;
        if (double.IsNaN(se) || se <= 0.0) return "no signal — the arms never separated";
        if (Math.Abs(p.SynergyExcess) <= se) return "indistinguishable from zero";
        return p.SynergyExcess <= 1.0
            ? $"marginal — {p.SynergyExcess / se:0.0}× its own error"
            : $"superadditive — {p.SynergyExcess / se:0.0}× its own error";
    }

    // ---- 7. grind ----

    private static void Grind(StringBuilder sb, IReadOnlyList<BatchSummary> batches)
    {
        sb.AppendLine("## 7. Grind metric");
        sb.AppendLine();
        sb.AppendLine("Median decisions per round (tickets + cash-outs + buys). Fewer decisions late than mid = flat repetition.");
        sb.AppendLine();

        sb.Append("| Round |");
        foreach (BatchSummary b in batches) sb.Append($" {b.Name} |");
        sb.AppendLine();
        sb.Append("|---|");
        foreach (var _ in batches) sb.Append("---|");
        sb.AppendLine();
        for (int r = 1; r <= 8; r++)
        {
            sb.Append($"| R{r} |");
            foreach (BatchSummary b in batches)
            {
                if (b.PlayedByRound[r] == 0) sb.Append(" — |");
                else sb.Append($" {b.MedianDecisionsByRound[r].ToString("0.#", Inv)} |");
            }
            sb.AppendLine();
        }
        sb.AppendLine();
        sb.AppendLine($"> Takeaway: {GrindTakeaway(batches)}");
        sb.AppendLine();
    }

    private static string GrindTakeaway(IReadOnlyList<BatchSummary> batches)
    {
        var flagged = new List<string>();
        foreach (BatchSummary b in batches)
        {
            double mid = 0, late = 0; int midN = 0, lateN = 0;
            for (int r = 3; r <= 5; r++) if (b.PlayedByRound[r] > 0) { mid += b.MedianDecisionsByRound[r]; midN++; }
            for (int r = 6; r <= 8; r++) if (b.PlayedByRound[r] > 0) { late += b.MedianDecisionsByRound[r]; lateN++; }
            if (midN > 0 && lateN > 0 && late / lateN < mid / midN) flagged.Add(b.Name);
        }
        return flagged.Count == 0
            ? "decision count holds up or rises into late rounds — no flat-repetition flag."
            : $"late rounds carry FEWER decisions than mid for: {string.Join(", ", flagged)} — repetition-risk flag.";
    }

    // ---- helpers ----

    private static (double mean, double max) Top1(double[] banks)
    {
        if (banks.Length == 0) return (0, 0);
        var sorted = new double[banks.Length];
        Array.Copy(banks, sorted, banks.Length);
        Array.Sort(sorted);
        int count = Math.Max(1, banks.Length / 100);
        double sum = 0, max = sorted[^1];
        for (int i = sorted.Length - count; i < sorted.Length; i++) sum += sorted[i];
        return (sum / count, max);
    }

    private static BatchSummary? Find(IReadOnlyList<BatchSummary> batches, string name)
    {
        foreach (BatchSummary b in batches) if (b.Name == name) return b;
        return null;
    }

    private static IEnumerable<string> Names(IReadOnlyList<BatchSummary> batches)
    {
        foreach (BatchSummary b in batches) yield return b.Name;
    }

    private static string Money(double v)
    {
        string sign = v < 0 ? "-" : "";
        return $"{sign}${Math.Abs(v).ToString("N0", Inv)}";
    }

    private static string Signed(double v)
        => Math.Abs(v) < 0.005 ? "0" : (v > 0 ? "+" : "") + v.ToString("0.##", Inv);

    private static string SignedPct(double v)
        => Math.Abs(v) < 0.05 ? "0.0pp" : (v > 0 ? "+" : "") + v.ToString("F1", Inv) + "pp";
    /// <summary>Internal rather than private: G7's coverage gate names the uncovered kinds, and
    /// one mapping that tracks MarketKind beats two that can drift apart when a market ships.</summary>
    internal static string MarketName(MarketKind kind) => kind switch
    {
        MarketKind.Moneyline => "Moneyline",
        MarketKind.TotalGoals => "Total Goals",
        MarketKind.BothTeamsToScore => "BTTS",
        MarketKind.TotalCorners => "Total Corners",
        MarketKind.TotalCards => "Total Cards",
        MarketKind.AnytimeScorer => "Anytime Scorer",
        _ => kind.ToString(),
    };
    private static string Pct(double v) => v.ToString("F1", Inv) + "%";
    private static string MedianDeath(double m) => m >= 9.0 ? "9 (won)" : m.ToString("0.#", Inv);

    private static string Payments(RunConfig cfg)
    {
        var parts = new List<string>();
        foreach (double t in cfg.Payments) parts.Add(((long)t).ToString(Inv));
        return string.Join(", ", parts);
    }

    private static Dictionary<string, string> BuildNames()
    {
        var d = new Dictionary<string, string>();
        foreach (RelicDefinition r in RelicCatalog.All) d[r.Id] = r.Name;
        return d;
    }
}
