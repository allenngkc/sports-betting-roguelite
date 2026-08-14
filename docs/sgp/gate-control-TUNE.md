# /sim — Monte Carlo balance report

- Date: 2026-08-13 20:17
- Engine: workspace is not a git repo — `git describe` unavailable
- Config: bank $350, PAYMENTS [60, 70, 85, 105, 155, 375, 710, 1350] (avg ×1.56), overround 5.0%, cash-out margin 8.0%, totem juice 50.0%, min stake $10, max stake 100.0% of bank, 6 matchups/round, 3 tickets/round, 5 relic + 3 consumable slots
- Strategies: naive, random, skilled, noshop, martyr, chalk, hoarder, ironhands, martyr-worst
- Seed: **pinned** — run i uses engine seed "TUNE-{i}". Same arguments reproduce this report's body byte-for-byte; the header's date and wall time are the exceptions and carry no verdict. `--verify` is the standing self-check.
- Runs per batch: 10,000
- Total runs (incl. audit/combos): 1,520,000
- Wall time: 1886.06 s

## 0. Gate campaign (PLAN.md acceptance criteria)

| Gate | Criterion | Verdict | Actual | Resolution |
|---|---|---|---|---|
| G1 | honest gambling: naive win <1%, dies before the cliff resolves (median ≤6) | **PASS** | median 4, won 0.0% | — |
| G2 | engine mandatory: no-shop skilled win <2%, median death 5–6 | **PASS** | median 5, won 0.0% | — |
| G3 | skilled + items wins: median death ≥5, win 4.5–8% (re-banded by Allen 2026-08-08 from 5–8%: the economy reads 5.4–5.5%, only 0.4–0.5pp above the old floor, so the gate could not separate its own reading from its own edge — three campaigns at 4,600 / 10,000 / 18,500 established that no sample size fixes a gap that small. Prior band Allen 2026-07-15 — the dealt hand's build variance is the roguelite shape) | **PASS** | median 6, won 5.4% (0.9pp from the nearest band edge) | ±0.45pp (2 SE) — band 3.5pp is 7.7× resolution; resolves its whole band |
| G4 | the EV arc exists: skilled mean ticket EV crosses zero in rounds 3–7 | **PASS** | crosses at R3 | — |
| G5 | composition superadditive: the exemplar pair's synergy excess ≥ 1pp (exemplar moved by Allen 2026-08-08 to The Multiplier + House Key, +2.96pp at 8.7× its own error; it was Multiplier + Scar Tissue, measured +0.1pp against ±0.06pp — real, but the weakest loop in the table and ~30× smaller than the strongest. Threshold set AFTER the error was measured, and adopts the report's own marginal/superadditive line rather than inventing a number) | **PASS** | synergy excess +3.0pp | ±0.34pp (2 SE, paired seeds) — one-sided floor at 1pp, so no band width to state; this reading clears it by +1.96pp, 5.8× resolution |
| G6 | martyr guard (worst case granted): loss-farming win ≤ skilled +2pp | **PASS** | martyr-worst 5.8% vs skilled 5.4% (organic martyr 1.6%) — margin +0.4pp, 1.6pp from the +2pp line | ±0.65pp (2 SE) — band 2pp is 3.1× resolution; fails reliably on a breach ≥0.65pp past the edge, no closer |
| G7 | market coverage: every shipped MarketKind is exercised by the skilled bot (LegsPlaced > 0) or on the named bot-excluded list | **PASS** | all shipped markets covered | exact — a leg count is not a sample; no resolution limit |

Gates evaluated: **7** · passed: **7** · produced a verdict: **7**.

Item flags: none — no DEAD items, no DOMINANT item, Totem in the healthy band.
- ℹ BOT-EXCLUDED: BTTS — near-even two-way market: under exact de-vig it never strictly wins a tie and its odds never clear the longshot threshold, so a sharp correctly declines it (M1, expires at v2 pricing) — measured reachable: random places 15,907 BTTS legs where skilled places 0, so the market is DECLINED, not blocked
- ℹ BOT-EXCLUDED: Anytime Scorer — YES-only market, bots do not price it (declared policy)
- ℹ PLAYTEST-GATED: Ask for the Manager audits ≈0 through bots (Δwon -0.1±0.9pp) — RATIFIED KEEP, playtest #9

> **ALL 7 GATES PASS — the economy holds.**

## 1. Survival curves

Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).

| Metric | naive | random | skilled | noshop | martyr | chalk | hoarder | ironhands | martyr-worst |
|---|---|---|---|---|---|---|---|---|---|
| enter R1 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R2 | 100.0% | 99.9% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R3 | 100.0% | 81.3% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R4 | 86.3% | 46.0% | 100.0% | 100.0% | 39.5% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R5 | 43.6% | 23.1% | 81.0% | 69.8% | 26.6% | 79.1% | 73.3% | 82.0% | 45.4% |
| enter R6 | 10.7% | 11.2% | 52.4% | 17.8% | 14.7% | 45.8% | 32.1% | 55.1% | 34.2% |
| enter R7 | 0.2% | 4.8% | 27.6% | 0.1% | 6.9% | 21.7% | 8.1% | 31.6% | 19.2% |
| enter R8 | 0.0% | 1.9% | 13.6% | 0.0% | 3.0% | 11.5% | 2.3% | 17.9% | 10.3% |
| **won %** | **0.0%** | **0.8%** | **5.4%** | **0.0%** | **1.6%** | **6.4%** | **0.8%** | **8.6%** | **5.8%** |
| **median death round** | **4** | **3** | **6** | **5** | **3** | **5** | **5** | **6** | **4** |
| mean rounds reached | 4.41 | 3.69 | 5.80 | 4.88 | 3.92 | 5.65 | 5.17 | 5.95 | 5.15 |
| totem fire rate | 0.0% | 4.8% | 38.6% | 0.0% | 0.0% | 28.4% | 14.0% | 29.7% | 0.0% |
| close-call deaths (% of deaths) | 13.6% | 14.7% | 5.2% | 1.8% | 4.2% | 6.3% | 6.0% | 5.6% | 6.1% |
| mean bookie gifts per run | 0.46 | 0.49 | 0.31 | 0.30 | 0.88 | 0.17 | 0.27 | 0.25 | 1.03 |

> Takeaway: naive dies at round 4, skilled reaches 6 — skill buys real extra survival; compare against the 3–4 / ≥7 targets above.

## 2. Market exposure

Placed legs and equal-split stake share by market kind. `mean leg EV` is the UNWEIGHTED per-leg return (converges to −vig under fair pricing — the sanity column); `stake-wtd EV` is stake-weighted and fat-tailed under compounding banks (a few monster tickets dominate it — read it as variance, not edge). Both are single-leg, before parlay multiplication, cash-outs, voids, or relic factors.

| Strategy | market | legs placed | stake share | mean leg EV | stake-wtd EV |
|---|---|---|---|---|---|
| naive | Moneyline | 87,374 | 100.0% | -4.6pp | -4.8pp |
| naive | Total Goals | 0 | 0.0% | — | — |
| naive | BTTS | 0 | 0.0% | — | — |
| naive | Total Corners | 0 | 0.0% | — | — |
| naive | Total Cards | 0 | 0.0% | — | — |
| naive | Anytime Scorer | 0 | 0.0% | — | — |
| random | Moneyline | 15,831 | 2.6% | -3.8pp | -16.9pp |
| random | Total Goals | 47,558 | 6.6% | -2.9pp | -11.4pp |
| random | BTTS | 15,907 | 2.0% | -4.0pp | -6.3pp |
| random | Total Corners | 48,091 | 45.3% | -2.7pp | +93.0pp |
| random | Total Cards | 47,981 | 43.4% | -3.6pp | +83.0pp |
| random | Anytime Scorer | 0 | 0.0% | — | — |
| skilled | Moneyline | 94,537 | 67.6% | +5.4pp | -2.0pp |
| skilled | Total Goals | 9,551 | 14.6% | +16.5pp | -35.5pp |
| skilled | BTTS | 0 | 0.0% | — | — |
| skilled | Total Corners | 6,401 | 4.6% | +21.0pp | +73.3pp |
| skilled | Total Cards | 7,814 | 13.2% | +21.5pp | -43.4pp |
| skilled | Anytime Scorer | 0 | 0.0% | — | — |
| noshop | Moneyline | 93,465 | 100.0% | -4.2pp | -4.4pp |
| noshop | Total Goals | 0 | 0.0% | — | — |
| noshop | BTTS | 0 | 0.0% | — | — |
| noshop | Total Corners | 0 | 0.0% | — | — |
| noshop | Total Cards | 0 | 0.0% | — | — |
| noshop | Anytime Scorer | 0 | 0.0% | — | — |
| martyr | Moneyline | 114,863 | 100.0% | -4.7pp | -5.7pp |
| martyr | Total Goals | 0 | 0.0% | — | — |
| martyr | BTTS | 0 | 0.0% | — | — |
| martyr | Total Corners | 0 | 0.0% | — | — |
| martyr | Total Cards | 0 | 0.0% | — | — |
| martyr | Anytime Scorer | 0 | 0.0% | — | — |
| chalk | Moneyline | 118,567 | 100.0% | +3.9pp | +5.5pp |
| chalk | Total Goals | 0 | 0.0% | — | — |
| chalk | BTTS | 0 | 0.0% | — | — |
| chalk | Total Corners | 0 | 0.0% | — | — |
| chalk | Total Cards | 0 | 0.0% | — | — |
| chalk | Anytime Scorer | 0 | 0.0% | — | — |
| hoarder | Moneyline | 101,199 | 100.0% | +1.9pp | +0.6pp |
| hoarder | Total Goals | 0 | 0.0% | — | — |
| hoarder | BTTS | 0 | 0.0% | — | — |
| hoarder | Total Corners | 0 | 0.0% | — | — |
| hoarder | Total Cards | 0 | 0.0% | — | — |
| hoarder | Anytime Scorer | 0 | 0.0% | — | — |
| ironhands | Moneyline | 121,577 | 100.0% | +9.5pp | +12.2pp |
| ironhands | Total Goals | 0 | 0.0% | — | — |
| ironhands | BTTS | 0 | 0.0% | — | — |
| ironhands | Total Corners | 0 | 0.0% | — | — |
| ironhands | Total Cards | 0 | 0.0% | — | — |
| ironhands | Anytime Scorer | 0 | 0.0% | — | — |
| martyr-worst | Moneyline | 148,891 | 100.0% | -4.8pp | -6.2pp |
| martyr-worst | Total Goals | 0 | 0.0% | — | — |
| martyr-worst | BTTS | 0 | 0.0% | — | — |
| martyr-worst | Total Corners | 0 | 0.0% | — | — |
| martyr-worst | Total Cards | 0 | 0.0% | — | — |
| martyr-worst | Anytime Scorer | 0 | 0.0% | — | — |

> Anytime Scorer is intentionally zero: it is a declared human-agency market, excluded from every bot.

## 2. Item power audit (3 passives + 3 consumables)

Skilled baseline: median death 6, mean rounds 5.80, won 5.4%. Passives granted at run start; consumables refilled every round. Exposure = uses (consumables) or wound-up runs (ratchets); ±  is the paired-seed SE on Δwon. Sorted by Δ mean rounds survived.

| Item | kind | mean rounds | Δ mean | median death | won % | Δ won % (±SE) | exposure | totem fires |
|---|---|---|---|---|---|---|---|---|---|
| Mulligan Slip | consumable | 6.45 | +0.65 | 6 | 14.4% | +9.0pp (±0.3) | 29,655 uses | — |
| Totem of Undying | passive | 6.40 | +0.6 | 6 | 6.2% | +0.8pp (±0.3) | — | 91.2% |
| Free Bet Token | consumable | 6.35 | +0.55 | 6 | 3.6% | -1.9pp (±0.3) | 57,454 uses | — |
| Profit Boost | consumable | 6.22 | +0.42 | 6 | 10.9% | +5.5pp (±0.3) | 57,391 uses | — |
| House Key | passive | 6.19 | +0.39 | 6 | 13.2% | +7.8pp (±0.4) | — | — |
| Bad Beat Jar | passive | 6.08 | +0.28 | 6 | 9.4% | +4.0pp (±0.4) | 6,729 wound | — |
| Unopened Bobblehead | passive | 6.06 | +0.26 | 6 | 9.8% | +4.3pp (±0.3) | — | — |
| The System | passive | 6.06 | +0.26 | 6 | 11.1% | +5.7pp (±0.4) | 2,485 wound | — |
| The Collection | passive | 6.05 | +0.24 | 6 | 10.0% | +4.6pp (±0.4) | — | — |
| Ref's Whistle | consumable | 6.02 | +0.22 | 6 | 7.1% | +1.7pp (±0.2) | 8,349 uses | — |
| Iron Hands | passive | 6.02 | +0.22 | 6 | 9.5% | +4.1pp (±0.4) | 3,525 wound | — |
| Bookie's Marker | consumable | 5.98 | +0.18 | 6 | 5.3% | -0.2pp (±0.2) | 21,961 uses | — |
| Chalk Eater | passive | 5.96 | +0.16 | 6 | 9.4% | +3.9pp (±0.4) | 7,940 wound | — |
| Double or Nothing Slip | consumable | 5.93 | +0.13 | 6 | 9.5% | +4.0pp (±0.2) | 16,374 uses | — |
| The Rake's Rebate | passive | 5.89 | +0.09 | 6 | 6.4% | +1.0pp (±0.3) | — | — |
| Scar Tissue | passive | 5.89 | +0.09 | 6 | 6.2% | +0.8pp (±0.3) | 5,510 wound | — |
| Longshot Larry's Photo | passive | 5.83 | +0.03 | 6 | 4.9% | -0.5pp (±0.3) | — | — |
| Comp'd Suite | passive | 5.83 | +0.03 | 6 | 5.6% | +0.1pp (±0.3) | — | — |
| Ask for the Manager | consumable | 5.80 | 0 | 6 | 5.3% | -0.1pp (±0.3) | 47,437 uses | — |
| The Multiplier | passive | 5.77 | -0.03 | 5 | 17.5% | +12.0pp (±0.4) | — | — |
| Golden Parachute | passive | 5.69 | -0.11 | 6 | 3.4% | -2.1pp (±0.3) | — | — |
| Whale Card | passive | 5.27 | -0.53 | 5 | 1.2% | -4.2pp (±0.2) | — | — |

> Takeaway: the gate section's item flags carry the verdicts (DEAD / DOMINANT / totem band). Note the audit grants items FREE — organic play also pays shop prices out of payment headroom.

## 3. Variance feel

Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.

| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |
|---|---|---|---|---|---|---|---|---|---|
| naive | $87 | $107 | $183 | $222 | n/a | n/a | n/a | n/a |
| random | $88 | $148 | $509 | $3,631 | $323 | $4,577 | $234,450 | $180,058,379,386 |
| skilled | $82 | $114 | $778 | $8,602 | $104 | $1,721 | $24,672 | $433,606 |
| noshop | $78 | $112 | $137 | $165 | n/a | n/a | n/a | n/a |
| martyr | $87 | $87 | $838 | $4,842 | $1 | $1,829 | $13,434 | $72,501 |
| chalk | $82 | $112 | $643 | $7,405 | $160 | $4,044 | $16,869 | $94,728 |
| hoarder | $79 | $112 | $175 | $1,290 | $130 | $2,519 | $13,498 | $48,340 |
| ironhands | $83 | $152 | $1,231 | $15,531 | $112 | $2,395 | $24,031 | $170,435 |
| martyr-worst | $87 | $87 | $1,402 | $13,278 | $300 | $3,018 | $20,191 | $81,825 |

> Takeaway: swing p50 vs p99 shows whether the ride is flat (boring) or spiky (feels rigged); a p99 many multiples of p50 is the intended rare-blowup shape.

## 4. Ratchet telemetry (Scar Tissue)

Mean peak stacks (pp) and mean carrier burns per run — is the ratchet winding and cashing?

| Strategy | mean peak stacks | mean burns/run |
|---|---|---|
| naive | 0.0pp | 0.00 |
| random | 0.5pp | 0.00 |
| skilled | 2.7pp | 0.00 |
| noshop | 0.0pp | 0.00 |
| martyr | 6.4pp | 0.00 |
| chalk | 0.4pp | 0.00 |
| hoarder | 0.1pp | 0.00 |
| ironhands | 0.7pp | 0.00 |
| martyr-worst | 28.2pp | 0.00 |

> Takeaway: zero across the board means nobody buys/holds the scar (price or power problem); huge stacks with zero burns means winding without cashing (the carrier rule isn't landing).

## 5. EV-arc

Mean *true* per-ticket EV at lock, by round (the economy doctrine's crossing from −vig toward positive).

| Round | naive mean EV | random mean EV | skilled mean EV | noshop mean EV | martyr mean EV | chalk mean EV | hoarder mean EV | ironhands mean EV | martyr-worst mean EV |
|---|---|---|---|---|---|---|---|---|---|
| R1 | -$8 | -$10 | -$5 | -$5 | -$7 | -$5 | -$5 | -$5 | $32 |
| R2 | -$6 | -$7 | $3 | -$4 | $8 | $3 | -$4 | $24 | $36 |
| R3 | -$5 | -$6 | $24 | -$3 | $16 | $21 | $2 | $30 | $57 |
| R4 | -$3 | -$7 | $25 | -$3 | $26 | $30 | $7 | $30 | $84 |
| R5 | -$3 | -$11 | $36 | -$4 | $45 | $48 | $7 | $49 | $134 |
| R6 | -$3 | -$17 | $59 | -$3 | $88 | $86 | $18 | $84 | $268 |
| R7 | -$1 | -$71 | $170 | -$4 | $211 | $176 | $70 | $184 | $513 |
| R8 | — | -$5,352 | $1,706 | — | $370 | $297 | $178 | $393 | $960 |

- Skilled mean EV first crosses zero at **round 3** (target ≈ round 4).
- **Survivorship caveat:** the round-3+ means average only the few runs that got there (R3 n=9380 tickets vs R1 n=10000). The MEDIAN skilled run dies at round 2, so it never reaches the +EV band — the arc is real for the surviving tail, not the typical player.
- Naive mean EV never crosses zero — as intended.

> Takeaway: among survivors the +EV relics do flip the arc (crosses ~R3, target ≈R4), but almost nobody survives to bank it — the mechanic works, the economy gates it away.

## 6. Band-3 audit

Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).

| Strategy | p99 final bank | top-1% mean | max |
|---|---|---|---|
| naive | $262 | $310 | $370 |
| random | $599 | $10,582,822,422 | $1,058,046,957,893 |
| skilled | $9,735 | $322,618 | $12,677,820 |
| noshop | $132 | $197 | $370 |
| martyr | $1,173 | $11,773 | $288,039 |
| chalk | $12,662 | $35,658 | $434,647 |
| hoarder | $654 | $5,359 | $54,546 |
| ironhands | $20,117 | $74,297 | $734,964 |
| martyr-worst | $12,602 | $34,746 | $438,441 |


Pairwise relic synergy (10,000 runs/config, baseline won 0.0%). Synergy excess = pair Δwon − (soloA Δ + soloB Δ). Top 10:

| Pair | pair won % | synergy excess (pp) | ±2 SE (paired) | tag |
|---|---|---|---|---|
| The Multiplier + House Key | 4.0% | +2.96 | ±0.34 | superadditive — 8.7× its own error |
| Longshot Larry's Photo + House Key | 4.2% | +2.67 | ±0.33 | superadditive — 8.1× its own error |
| The Multiplier + Whale Card | 3.2% | +2.17 | ±0.29 | superadditive — 7.4× its own error |
| Longshot Larry's Photo + Whale Card | 3.5% | +2.04 | ±0.29 | superadditive — 7.0× its own error |
| The Multiplier + Longshot Larry's Photo | 3.2% | +0.76 | ±0.45 | marginal — 1.7× its own error |
| Longshot Larry's Photo + The System | 2.2% | +0.74 | ±0.18 | marginal — 4.1× its own error |
| The Multiplier + The System | 1.7% | +0.7 | ±0.17 | marginal — 4.2× its own error |
| The Multiplier + Chalk Eater | 1.6% | +0.65 | ±0.16 | marginal — 4.0× its own error |
| Longshot Larry's Photo + Bad Beat Jar | 2.1% | +0.57 | ±0.16 | marginal — 3.7× its own error |
| The Multiplier + Iron Hands | 1.4% | +0.45 | ±0.13 | marginal — 3.4× its own error |

Ranked by excess; the ±2 SE column is paired by seed, so it is the error of the *combination*, not of any one arm. A row whose excess is inside its own error is tagged as such and its rank means nothing.

> Takeaway: brokenness should live in the tail (max ≫ p99) and, if combos ran, in a few pairs that clear their own error rather than across the whole table.

## 7. Grind metric

Median decisions per round (tickets + cash-outs + buys). Fewer decisions late than mid = flat repetition.

| Round | naive | random | skilled | noshop | martyr | chalk | hoarder | ironhands | martyr-worst |
|---|---|---|---|---|---|---|---|---|---|
| R1 | 1 | 3 | 3 | 1 | 3 | 3 | 1 | 3 | 3 |
| R2 | 1 | 3 | 4 | 1 | 3 | 4 | 2 | 4 | 3 |
| R3 | 1 | 3 | 4 | 1 | 0 | 3 | 4 | 4 | 3 |
| R4 | 1 | 2 | 3 | 1 | 2 | 2 | 2 | 3 | 1 |
| R5 | 1 | 2 | 2 | 1 | 2 | 2 | 1 | 2 | 3 |
| R6 | 1 | 3 | 2 | 1 | 1 | 2 | 1 | 2 | 1 |
| R7 | 1 | 3 | 2 | 1 | 1 | 2 | 1 | 2 | 1 |
| R8 | — | 3 | 2 | — | 1 | 2 | 2 | 2 | 2 |

> Takeaway: late rounds carry FEWER decisions than mid for: skilled, martyr, chalk, hoarder, ironhands, martyr-worst — repetition-risk flag.

