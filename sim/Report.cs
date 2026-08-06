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
    private static readonly Dictionary<string, double> Price = BuildPrices();
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
        sb.AppendLine("| Gate | Criterion | Verdict | Actual |");
        sb.AppendLine("|---|---|---|---|");
        foreach (GateData.Gate g in gates.Gates)
            sb.AppendLine($"| {g.Id} | {g.Description} | {(g.Pass ? "**PASS**" : "**FAIL**")} | {g.Actual} |");
        sb.AppendLine();
        if (gates.ItemFlags.Count == 0)
            sb.AppendLine("Item flags: none — no DEAD items, no DOMINANT item, Totem in the healthy band.");
        else
            foreach (string flag in gates.ItemFlags)
                sb.AppendLine($"- ⚑ {flag}");
        foreach (string note in gates.Notes)
            sb.AppendLine($"- ℹ {note}");
        sb.AppendLine();
        sb.AppendLine($"> **{(gates.AllPass && gates.ItemFlags.Count == 0 ? "ALL GATES PASS — the economy holds." : "NOT DONE — iterate the knobs (item numbers / prices / curve) and rerun.")}**");
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
            + $"overround {Pct(cfg.Overround * 100)}, cash-out margin {Pct(cfg.CashOutMargin * 100)}, "
            + $"totem juice {Pct(cfg.TotemJuiceRate * 100)}, "
            + $"min stake {Money(cfg.MinStake)}, max stake {Pct(cfg.MaxStakeFraction * 100)} of bank, "
            + $"{cfg.MatchupsPerSlate} matchups/round, {cfg.MaxTicketsPerRound} tickets/round, "
            + $"{cfg.RelicSlots} relic + {cfg.ConsumableSlots} consumable slots");
        sb.AppendLine($"- Strategies: {string.Join(", ", Names(batches))}");
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

        sb.AppendLine("| priced p band | offers | samples | mean priced p | realised freq | Δ (pp) | realised EV (pp) | SE (pp) |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|");
        foreach (ScorerCalibrationData.Bucket b in data.ByProbabilityBand())
            AppendCalibrationRow(sb, b);
        sb.AppendLine();

        sb.AppendLine("By role — scoring weight (and so priced probability) is assigned purely by "
            + "role, so a role-shaped miscalibration is the fastest possible diagnosis:");
        sb.AppendLine();
        sb.AppendLine("| role | offers | samples | mean priced p | realised freq | Δ (pp) | realised EV (pp) | SE (pp) |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|");
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

        sb.AppendLine($"> Takeaway: {ScorerEvTakeaway(data)}");
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
            + $"| {SignedPct(b.EvFraction * 100.0)} | ±{(b.SeFraction * 100.0).ToString("F1", Inv)}pp |");
    }

    /// <summary>An honest read of the buckets above, not a hand-authored verdict: flags bands
    /// whose |Δ| exceeds 2 SE (≈95% two-sided) rather than asserting calibration either way.</summary>
    private static string ScorerEvTakeaway(ScorerCalibrationData data)
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
        if (populated == 0) return "no offers sampled — widen --runs.";
        if (outside2Se == 0)
            return $"all {populated} probability bands sit within 2 SE of their priced probability "
                + "— calibration holds at this sample size.";
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
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|");
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
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|");
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
        sb.AppendLine("> Takeaway: brokenness should live in the tail (max ≫ p99) and, if combos ran, in a few "
            + "hard-to-assemble pairs rather than trivial cheap ones.");
        sb.AppendLine();
    }

    private static void Combos(StringBuilder sb, ComboData combos)
    {
        sb.AppendLine();
        sb.AppendLine($"Pairwise relic synergy ({combos.RunsPerConfig.ToString("N0", Inv)} runs/config, baseline won {Pct(combos.BaselineWonPct)}). "
            + "Synergy excess = pair Δwon − (soloA Δ + soloB Δ). Top 10:");
        sb.AppendLine();
        sb.AppendLine("| Pair | pair won % | synergy excess (pp) | tag |");
        sb.AppendLine("|---|---|---|---|");
        int shown = 0;
        foreach (ComboData.Pair p in combos.Pairs)
        {
            if (shown++ >= 10) break;
            string names = $"{RelicName.GetValueOrDefault(p.IdA, p.IdA)} + {RelicName.GetValueOrDefault(p.IdB, p.IdB)}";
            sb.AppendLine($"| {names} | {Pct(p.PairWonPct)} | {Signed(p.SynergyExcess)} | {ComboTag(p)} |");
        }
    }

    private static string ComboTag(ComboData.Pair p)
    {
        if (p.SynergyExcess <= 1.0) return "marginal (no real loop)";
        double combined = Price.GetValueOrDefault(p.IdA) + Price.GetValueOrDefault(p.IdB);
        return combined <= 450.0
            ? "degenerate: cheap pair, trivially assembled"
            : "delicious: costly pair, bounded by run length";
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

    private static Dictionary<string, double> BuildPrices()
    {
        var d = new Dictionary<string, double>();
        foreach (RelicDefinition r in RelicCatalog.All) d[r.Id] = r.Price;
        return d;
    }

    private static Dictionary<string, string> BuildNames()
    {
        var d = new Dictionary<string, string>();
        foreach (RelicDefinition r in RelicCatalog.All) d[r.Id] = r.Name;
        return d;
    }
}
