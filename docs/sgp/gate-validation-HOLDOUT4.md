# /sim — Monte Carlo balance report

- Date: 2026-08-13 20:51
- Engine: workspace is not a git repo — `git describe` unavailable
- Config: bank $350, PAYMENTS [60, 70, 85, 105, 155, 375, 710, 1350] (avg ×1.56), overround 5.0%, SGP margin κ 1.0, cash-out margin 8.0%, totem juice 50.0%, min stake $10, max stake 100.0% of bank, 6 matchups/round, 3 tickets/round, 5 relic + 3 consumable slots
- Strategies: naive, random, skilled, noshop, martyr, chalk, hoarder, ironhands, samematch, martyr-worst
- Seed: **pinned** — run i uses engine seed "HOLDOUT4-{i}". Same arguments reproduce this report's body byte-for-byte; the header's date and wall time are the exceptions and carry no verdict. `--verify` is the standing self-check.
- Runs per batch: 10,000
- Total runs (incl. audit/combos): 1,530,000
- Wall time: 1324.54 s

## 0. Gate campaign (PLAN.md acceptance criteria)

| Gate | Criterion | Verdict | Actual | Resolution |
|---|---|---|---|---|
| G1 | honest gambling: naive win <1%, dies before the cliff resolves (median ≤6) | **PASS** | median 4, won 0.0% | — |
| G2 | engine mandatory: no-shop skilled win <2%, median death 5–6 | **PASS** | median 5, won 0.0% | — |
| G3 | skilled + items wins: median death ≥5, win 4.5–8% (re-banded by Allen 2026-08-08 from 5–8%: the economy reads 5.4–5.5%, only 0.4–0.5pp above the old floor, so the gate could not separate its own reading from its own edge — three campaigns at 4,600 / 10,000 / 18,500 established that no sample size fixes a gap that small. Prior band Allen 2026-07-15 — the dealt hand's build variance is the roguelite shape) | **PASS** | median 6, won 5.8% (1.3pp from the nearest band edge) | ±0.47pp (2 SE) — band 3.5pp is 7.5× resolution; resolves its whole band |
| G4 | the EV arc exists: skilled mean ticket EV crosses zero in rounds 3–7 | **PASS** | crosses at R3 | — |
| G5 | composition superadditive: the exemplar pair's synergy excess ≥ 1pp (exemplar moved by Allen 2026-08-08 to The Multiplier + House Key, +2.96pp at 8.7× its own error; it was Multiplier + Scar Tissue, measured +0.1pp against ±0.06pp — real, but the weakest loop in the table and ~30× smaller than the strongest. Threshold set AFTER the error was measured, and adopts the report's own marginal/superadditive line rather than inventing a number) | **PASS** | synergy excess +3.0pp | ±0.34pp (2 SE, paired seeds) — one-sided floor at 1pp, so no band width to state; this reading clears it by +2.03pp, 5.9× resolution |
| G6 | martyr guard (worst case granted): loss-farming win ≤ skilled +2pp | **PASS** | martyr-worst 5.5% vs skilled 5.8% (organic martyr 1.3%) — margin -0.2pp, 2.2pp from the +2pp line | ±0.65pp (2 SE) — band 2pp is 3.1× resolution; fails reliably on a breach ≥0.65pp past the edge, no closer |
| G7 | market coverage: every shipped MarketKind is exercised by the skilled bot (LegsPlaced > 0) or on the named bot-excluded list | **PASS** | all shipped markets covered | exact — a leg count is not a sample; no resolution limit |
| G7-SGP | same-match coverage: the SAME MATCH probe placed AND settled same-match tickets, and zero tickets were sold at the no-label naive-product fallback (a ticket shape is invisible to G7's MarketKind roll-call, and a silent fallback is a money leak worth up to +274% EV on an implication pair) | **PASS** | placed 107,073, settled 107,073, no-label fallbacks 0, refusals tripped 34,841, voids re-priced 11,658 | exact — a ticket count is not a sample; no resolution limit |

Gates evaluated: **8** · passed: **8** · produced a verdict: **8**.

Item flags: none — no DEAD items, no DOMINANT item, Totem in the healthy band.
- ℹ BOT-EXCLUDED: BTTS — near-even two-way market: under exact de-vig it never strictly wins a tie and its odds never clear the longshot threshold, so a sharp correctly declines it (M1, expires at v2 pricing) — measured reachable: random places 16,259 BTTS legs where skilled places 0, so the market is DECLINED, not blocked
- ℹ BOT-EXCLUDED: Anytime Scorer — YES-only market, bots do not price it (declared policy)
- ℹ PLAYTEST-GATED: Ask for the Manager audits ≈0 through bots (Δwon -0.1±1.0pp) — RATIFIED KEEP, playtest #9

> **ALL 8 GATES PASS — the economy holds.**

## 0b. SAME MATCH exposure (informational — NOT a gate)

From the `samematch` batch. Whether the feature is covered is G7-SGP's verdict; how thinly each relation is covered is this table's, and the two are deliberately different instruments.

Tickets placed: **107,073** · settled: **107,073** · legs voided and re-priced: **11,658** · refusals tripped: **34,841**

| Relation | Relations priced | Tickets carrying it | Times principal |
|---|---:|---:|---:|
| Implies | 20,492 | 20,492 | 20,492 |
| SharedScoreline | 41,913 | 41,913 | 6,231 |
| ScorerOfSide | 81,364 | 45,682 | 45,682 |
| SharedCount | 24,668 | 24,668 | 24,668 |
| Independent | 10,000 | 10,000 | 0 |

Refusal rules exercised: ImpossibleCombination × 20,173 · DuplicateSelection × 14,668 · SubEvens × 0. SubEvens reads zero at the shipped κ = 1 by construction — the sub-evens price and its full-ticket refund need κ ≳ 1.3, so that path stays unit-test-only in this campaign.

Not exercised: MutuallyExclusive. MutuallyExclusive can never appear here by construction — it is the label on a combination the engine REFUSES, so it is never on a placed ticket; the refusal counters above are where it is read. Any other name in this line is a real hole in the probe's catalogue.

## 1. Survival curves

Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).

| Metric | naive | random | skilled | noshop | martyr | chalk | hoarder | ironhands | samematch | martyr-worst |
|---|---|---|---|---|---|---|---|---|---|---|
| enter R1 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R2 | 100.0% | 99.9% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R3 | 100.0% | 81.5% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R4 | 86.6% | 46.0% | 100.0% | 100.0% | 39.3% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R5 | 43.9% | 23.4% | 81.3% | 69.4% | 27.0% | 78.6% | 72.6% | 81.0% | 70.1% | 45.2% |
| enter R6 | 10.8% | 11.2% | 53.7% | 17.6% | 14.7% | 44.7% | 32.2% | 54.9% | 4.1% | 34.6% |
| enter R7 | 0.2% | 4.7% | 27.5% | 0.1% | 6.6% | 21.0% | 8.7% | 32.4% | 0.0% | 19.1% |
| enter R8 | 0.0% | 2.0% | 13.9% | 0.0% | 2.9% | 11.0% | 2.5% | 18.6% | 0.0% | 10.3% |
| **won %** | **0.0%** | **0.8%** | **5.8%** | **0.0%** | **1.3%** | **5.9%** | **0.9%** | **8.9%** | **0.0%** | **5.5%** |
| **median death round** | **4** | **3** | **6** | **5** | **3** | **5** | **5** | **6** | **5** | **4** |
| mean rounds reached | 4.41 | 3.69 | 5.82 | 4.87 | 3.92 | 5.61 | 5.17 | 5.96 | 4.74 | 5.15 |
| totem fire rate | 0.0% | 4.9% | 38.7% | 0.0% | 0.0% | 27.6% | 14.2% | 29.5% | 0.0% | 0.0% |
| close-call deaths (% of deaths) | 13.6% | 15.5% | 5.7% | 1.4% | 4.5% | 6.7% | 5.6% | 5.7% | 18.1% | 5.9% |
| mean bookie gifts per run | 0.46 | 0.49 | 0.31 | 0.31 | 0.88 | 0.17 | 0.28 | 0.25 | 0.52 | 1.03 |

> Takeaway: naive dies at round 4, skilled reaches 6 — skill buys real extra survival; compare against the 3–4 / ≥7 targets above.

## 2. Market exposure

Placed legs and equal-split stake share by market kind. `mean leg EV` is the UNWEIGHTED per-leg return (converges to −vig under fair pricing — the sanity column); `stake-wtd EV` is stake-weighted and fat-tailed under compounding banks (a few monster tickets dominate it — read it as variance, not edge). Both are single-leg, before parlay multiplication, cash-outs, voids, or relic factors.

| Strategy | market | legs placed | stake share | mean leg EV | stake-wtd EV |
|---|---|---|---|---|---|
| naive | Moneyline | 87,532 | 100.0% | -4.7pp | -4.5pp |
| naive | Total Goals | 0 | 0.0% | — | — |
| naive | BTTS | 0 | 0.0% | — | — |
| naive | Total Corners | 0 | 0.0% | — | — |
| naive | Total Cards | 0 | 0.0% | — | — |
| naive | Anytime Scorer | 0 | 0.0% | — | — |
| random | Moneyline | 15,970 | 15.9% | -1.9pp | +90.8pp |
| random | Total Goals | 48,284 | 0.0% | -4.0pp | +35.1pp |
| random | BTTS | 16,259 | 0.0% | -4.3pp | -99.9pp |
| random | Total Corners | 48,247 | 0.0% | -3.3pp | -99.8pp |
| random | Total Cards | 48,101 | 84.1% | -3.8pp | -100.0pp |
| random | Anytime Scorer | 0 | 0.0% | — | — |
| skilled | Moneyline | 94,424 | 77.9% | +5.7pp | +6.0pp |
| skilled | Total Goals | 9,579 | 9.0% | +15.5pp | +21.9pp |
| skilled | BTTS | 0 | 0.0% | — | — |
| skilled | Total Corners | 6,417 | 6.1% | +22.2pp | +12.0pp |
| skilled | Total Cards | 7,875 | 6.9% | +22.5pp | +27.6pp |
| skilled | Anytime Scorer | 0 | 0.0% | — | — |
| noshop | Moneyline | 93,186 | 100.0% | -4.2pp | -4.8pp |
| noshop | Total Goals | 0 | 0.0% | — | — |
| noshop | BTTS | 0 | 0.0% | — | — |
| noshop | Total Corners | 0 | 0.0% | — | — |
| noshop | Total Cards | 0 | 0.0% | — | — |
| noshop | Anytime Scorer | 0 | 0.0% | — | — |
| martyr | Moneyline | 114,732 | 100.0% | -5.5pp | -6.2pp |
| martyr | Total Goals | 0 | 0.0% | — | — |
| martyr | BTTS | 0 | 0.0% | — | — |
| martyr | Total Corners | 0 | 0.0% | — | — |
| martyr | Total Cards | 0 | 0.0% | — | — |
| martyr | Anytime Scorer | 0 | 0.0% | — | — |
| chalk | Moneyline | 117,646 | 100.0% | +3.6pp | +16.6pp |
| chalk | Total Goals | 0 | 0.0% | — | — |
| chalk | BTTS | 0 | 0.0% | — | — |
| chalk | Total Corners | 0 | 0.0% | — | — |
| chalk | Total Cards | 0 | 0.0% | — | — |
| chalk | Anytime Scorer | 0 | 0.0% | — | — |
| hoarder | Moneyline | 101,455 | 100.0% | +1.9pp | -0.1pp |
| hoarder | Total Goals | 0 | 0.0% | — | — |
| hoarder | BTTS | 0 | 0.0% | — | — |
| hoarder | Total Corners | 0 | 0.0% | — | — |
| hoarder | Total Cards | 0 | 0.0% | — | — |
| hoarder | Anytime Scorer | 0 | 0.0% | — | — |
| ironhands | Moneyline | 121,767 | 100.0% | +9.6pp | +13.7pp |
| ironhands | Total Goals | 0 | 0.0% | — | — |
| ironhands | BTTS | 0 | 0.0% | — | — |
| ironhands | Total Corners | 0 | 0.0% | — | — |
| ironhands | Total Cards | 0 | 0.0% | — | — |
| ironhands | Anytime Scorer | 0 | 0.0% | — | — |
| samematch | Moneyline | 51,913 | 17.8% | -1.2pp | -0.6pp |
| samematch | Total Goals | 56,493 | 21.3% | -3.9pp | -3.9pp |
| samematch | BTTS | 26,404 | 12.5% | -1.2pp | -1.7pp |
| samematch | Total Corners | 59,336 | 27.5% | +1.0pp | +0.9pp |
| samematch | Total Cards | 10,000 | 5.7% | -3.5pp | -3.5pp |
| samematch | Anytime Scorer | 45,682 | 15.3% | +6.3pp | +7.1pp |
| martyr-worst | Moneyline | 148,841 | 100.0% | -5.3pp | -6.6pp |
| martyr-worst | Total Goals | 0 | 0.0% | — | — |
| martyr-worst | BTTS | 0 | 0.0% | — | — |
| martyr-worst | Total Corners | 0 | 0.0% | — | — |
| martyr-worst | Total Cards | 0 | 0.0% | — | — |
| martyr-worst | Anytime Scorer | 0 | 0.0% | — | — |

> Anytime Scorer is intentionally zero: it is a declared human-agency market, excluded from every bot.

## 2. Item power audit (3 passives + 3 consumables)

Skilled baseline: median death 6, mean rounds 5.82, won 5.8%. Passives granted at run start; consumables refilled every round. Exposure = uses (consumables) or wound-up runs (ratchets); ±  is the paired-seed SE on Δwon. Sorted by Δ mean rounds survived.

| Item | kind | mean rounds | Δ mean | median death | won % | Δ won % (±SE) | exposure | totem fires |
|---|---|---|---|---|---|---|---|---|---|
| Mulligan Slip | consumable | 6.44 | +0.62 | 6 | 13.8% | +8.0pp (±0.3) | 29,428 uses | — |
| Totem of Undying | passive | 6.42 | +0.6 | 6 | 7.1% | +1.3pp (±0.3) | — | 90.4% |
| Free Bet Token | consumable | 6.37 | +0.55 | 6 | 3.6% | -2.1pp (±0.3) | 57,739 uses | — |
| Profit Boost | consumable | 6.26 | +0.44 | 6 | 11.6% | +5.8pp (±0.3) | 57,796 uses | — |
| House Key | passive | 6.14 | +0.32 | 6 | 12.8% | +7.0pp (±0.4) | — | — |
| Unopened Bobblehead | passive | 6.07 | +0.24 | 6 | 10.1% | +4.3pp (±0.4) | — | — |
| Bad Beat Jar | passive | 6.04 | +0.22 | 6 | 9.3% | +3.6pp (±0.4) | 6,814 wound | — |
| Ref's Whistle | consumable | 6.03 | +0.21 | 6 | 7.2% | +1.5pp (±0.2) | 8,575 uses | — |
| The System | passive | 6.02 | +0.2 | 6 | 10.6% | +4.8pp (±0.4) | 2,496 wound | — |
| The Collection | passive | 6.02 | +0.2 | 6 | 9.7% | +4.0pp (±0.4) | — | — |
| Bookie's Marker | consumable | 6.00 | +0.18 | 6 | 5.2% | -0.5pp (±0.2) | 21,938 uses | — |
| Chalk Eater | passive | 6.00 | +0.18 | 6 | 9.6% | +3.9pp (±0.4) | 7,921 wound | — |
| Iron Hands | passive | 5.99 | +0.17 | 6 | 9.3% | +3.5pp (±0.4) | 3,458 wound | — |
| Double or Nothing Slip | consumable | 5.93 | +0.11 | 6 | 9.2% | +3.5pp (±0.2) | 16,336 uses | — |
| Scar Tissue | passive | 5.91 | +0.09 | 6 | 6.9% | +1.2pp (±0.3) | 5,394 wound | — |
| The Rake's Rebate | passive | 5.89 | +0.07 | 6 | 6.5% | +0.7pp (±0.3) | — | — |
| Longshot Larry's Photo | passive | 5.87 | +0.05 | 6 | 5.5% | -0.2pp (±0.3) | — | — |
| Ask for the Manager | consumable | 5.84 | +0.02 | 6 | 5.7% | -0.1pp (±0.3) | 47,806 uses | — |
| Comp'd Suite | passive | 5.82 | 0 | 6 | 5.9% | +0.2pp (±0.3) | — | — |
| The Multiplier | passive | 5.76 | -0.06 | 5 | 17.5% | +11.7pp (±0.4) | — | — |
| Golden Parachute | passive | 5.67 | -0.15 | 5 | 3.3% | -2.4pp (±0.3) | — | — |
| Whale Card | passive | 5.25 | -0.57 | 5 | 1.2% | -4.5pp (±0.3) | — | — |

> Takeaway: the gate section's item flags carry the verdicts (DEAD / DOMINANT / totem band). Note the audit grants items FREE — organic play also pays shop prices out of payment headroom.

## 3. Variance feel

Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.

| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |
|---|---|---|---|---|---|---|---|---|---|
| naive | $87 | $117 | $184 | $221 | n/a | n/a | n/a | n/a |
| random | $87 | $148 | $526 | $3,021 | $384 | $3,128 | $37,750 | $250,239,366,362,216 |
| skilled | $82 | $120 | $809 | $7,632 | $87 | $1,917 | $18,189 | $182,431 |
| noshop | $77 | $112 | $137 | $166 | n/a | n/a | n/a | n/a |
| martyr | $87 | $87 | $825 | $4,923 | $1 | $2,877 | $16,909 | $49,320 |
| chalk | $82 | $112 | $614 | $7,724 | $132 | $3,977 | $21,429 | $103,241 |
| hoarder | $78 | $112 | $178 | $1,138 | $93 | $1,929 | $7,260 | $21,721 |
| ironhands | $82 | $151 | $1,281 | $15,605 | $98 | $2,255 | $23,253 | $130,345 |
| samematch | $25 | $42 | $69 | $108 | n/a | n/a | n/a | n/a |
| martyr-worst | $87 | $87 | $1,422 | $13,747 | $307 | $3,121 | $23,255 | $68,474 |

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
| samematch | 0.0pp | 0.00 |
| martyr-worst | 28.2pp | 0.00 |

> Takeaway: zero across the board means nobody buys/holds the scar (price or power problem); huge stacks with zero burns means winding without cashing (the carrier rule isn't landing).

## 5. EV-arc

Mean *true* per-ticket EV at lock, by round (the economy doctrine's crossing from −vig toward positive).

| Round | naive mean EV | random mean EV | skilled mean EV | noshop mean EV | martyr mean EV | chalk mean EV | hoarder mean EV | ironhands mean EV | samematch mean EV | martyr-worst mean EV |
|---|---|---|---|---|---|---|---|---|---|---|
| R1 | -$8 | -$10 | -$5 | -$5 | -$7 | -$5 | -$5 | -$5 | -$2 | $32 |
| R2 | -$7 | -$7 | $3 | -$4 | $8 | $3 | -$4 | $23 | -$1 | $36 |
| R3 | -$5 | -$6 | $24 | -$3 | $17 | $22 | $2 | $30 | -$1 | $55 |
| R4 | -$3 | -$8 | $24 | -$3 | $26 | $26 | $7 | $29 | -$1 | $83 |
| R5 | -$3 | -$14 | $36 | -$4 | $44 | $49 | $7 | $49 | -$1 | $131 |
| R6 | -$3 | -$69 | $57 | -$4 | $83 | $94 | $18 | $83 | — | $261 |
| R7 | -$2 | -$304,320 | $136 | -$3 | $181 | $209 | $70 | $182 | — | $475 |
| R8 | — | -$304,157,216,474 | $295 | — | $405 | $2,249 | $168 | $420 | — | $976 |

- Skilled mean EV first crosses zero at **round 3** (target ≈ round 4).
- **Survivorship caveat:** the round-3+ means average only the few runs that got there (R3 n=9387 tickets vs R1 n=10000). The MEDIAN skilled run dies at round 2, so it never reaches the +EV band — the arc is real for the surviving tail, not the typical player.
- Naive mean EV never crosses zero — as intended.

> Takeaway: among survivors the +EV relics do flip the arc (crosses ~R3, target ≈R4), but almost nobody survives to bank it — the mechanic works, the economy gates it away.

## 6. Band-3 audit

Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).

| Strategy | p99 final bank | top-1% mean | max |
|---|---|---|---|
| naive | $249 | $302 | $375 |
| random | $637 | $9,624,591,002,224 | $962,459,093,493,050 |
| skilled | $9,185 | $357,351 | $29,224,701 |
| noshop | $125 | $195 | $360 |
| martyr | $1,097 | $8,747 | $51,569 |
| chalk | $13,924 | $510,566,319 | $51,052,074,363 |
| hoarder | $684 | $3,135 | $21,973 |
| ironhands | $19,480 | $130,042 | $3,122,050 |
| samematch | $145 | $153 | $267 |
| martyr-worst | $13,925 | $32,031 | $129,855 |


Pairwise relic synergy (10,000 runs/config, baseline won 0.0%). Synergy excess = pair Δwon − (soloA Δ + soloB Δ). Top 10:

| Pair | pair won % | synergy excess (pp) | ±2 SE (paired) | tag |
|---|---|---|---|---|
| The Multiplier + House Key | 4.1% | +3.03 | ±0.34 | superadditive — 8.8× its own error |
| Longshot Larry's Photo + House Key | 4.3% | +2.85 | ±0.34 | superadditive — 8.4× its own error |
| The Multiplier + Whale Card | 3.4% | +2.33 | ±0.30 | superadditive — 7.7× its own error |
| Longshot Larry's Photo + Whale Card | 3.4% | +1.99 | ±0.29 | superadditive — 6.9× its own error |
| The Multiplier + Longshot Larry's Photo | 3.3% | +0.83 | ±0.46 | marginal — 1.8× its own error |
| The Multiplier + The System | 1.7% | +0.66 | ±0.16 | marginal — 4.1× its own error |
| Longshot Larry's Photo + The System | 2.1% | +0.66 | ±0.18 | marginal — 3.7× its own error |
| The Multiplier + Chalk Eater | 1.7% | +0.62 | ±0.16 | marginal — 3.9× its own error |
| Longshot Larry's Photo + Bad Beat Jar | 2.0% | +0.55 | ±0.16 | marginal — 3.4× its own error |
| House Key + The System | 0.5% | +0.52 | ±0.14 | marginal — 3.6× its own error |

Ranked by excess; the ±2 SE column is paired by seed, so it is the error of the *combination*, not of any one arm. A row whose excess is inside its own error is tagged as such and its rank means nothing.

> Takeaway: brokenness should live in the tail (max ≫ p99) and, if combos ran, in a few pairs that clear their own error rather than across the whole table.

## 7. Grind metric

Median decisions per round (tickets + cash-outs + buys). Fewer decisions late than mid = flat repetition.

| Round | naive | random | skilled | noshop | martyr | chalk | hoarder | ironhands | samematch | martyr-worst |
|---|---|---|---|---|---|---|---|---|---|---|
| R1 | 1 | 3 | 3 | 1 | 3 | 3 | 1 | 3 | 3 | 3 |
| R2 | 1 | 3 | 4 | 1 | 3 | 4 | 2 | 4 | 4 | 3 |
| R3 | 1 | 3 | 4 | 1 | 0 | 3 | 4 | 4 | 4 | 3 |
| R4 | 1 | 2 | 3 | 1 | 2 | 3 | 2 | 3 | 3 | 1 |
| R5 | 1 | 3 | 2 | 1 | 2 | 2 | 1 | 2 | 0 | 3 |
| R6 | 1 | 3 | 2 | 1 | 1 | 2 | 1 | 2 | 0 | 1 |
| R7 | 1 | 3 | 2 | 1 | 0 | 2 | 1 | 2 | — | 2 |
| R8 | — | 2 | 2 | — | 0 | 2 | 2 | 2 | — | 1 |

> Takeaway: late rounds carry FEWER decisions than mid for: skilled, martyr, chalk, hoarder, ironhands, samematch, martyr-worst — repetition-risk flag.

