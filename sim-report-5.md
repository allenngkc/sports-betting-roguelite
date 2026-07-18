# /sim — Monte Carlo balance report

- Date: 2026-07-18 00:25
- Engine: workspace is not a git repo — `git describe` unavailable
- Config: bank $350, PAYMENTS [60, 70, 85, 105, 155, 375, 710, 1350] (avg ×1.56), overround 5.0%, cash-out margin 8.0%, totem juice 50.0%, min stake $10, max stake 100.0% of bank, 6 matchups/round, 3 tickets/round, 5 relic + 3 consumable slots
- Strategies: naive, skilled, noshop, martyr, chalk, hoarder, ironhands, martyr-worst
- Runs per batch: 50,000
- Total runs (incl. audit/combos): 7,550,000
- Wall time: 437.36 s

## 0. Gate campaign (PLAN.md acceptance criteria)

| Gate | Criterion | Verdict | Actual |
|---|---|---|---|
| G1 | honest gambling: naive win <1%, dies before the cliff resolves (median ≤6) | **PASS** | median 4, won 0.0% |
| G2 | engine mandatory: no-shop skilled win <2%, median death 5–6 | **PASS** | median 5, won 0.0% |
| G3 | skilled + items wins: median death ≥5, win 5–8% (re-banded by Allen 2026-07-15 — the dealt hand's build variance is the roguelite shape) | **PASS** | median 6, won 7.6% |
| G4 | the EV arc exists: skilled mean ticket EV crosses zero in rounds 3–7 | **PASS** | crosses at R3 |
| G5 | composition superadditive: Multiplier+Scar pair Δwin > sum of solo Δwins | **PASS** | synergy excess +0.0pp |
| G6 | martyr guard (worst case granted): loss-farming win ≤ skilled +2pp | **PASS** | martyr-worst 5.8% vs skilled 7.6% (organic martyr 1.4%) |

Item flags: none — no DEAD items, no DOMINANT item, Totem in the healthy band.
- ℹ PLAYTEST-GATED: Ask for the Manager audits ≈0 through bots (Δwon +0.1±0.4pp) — RATIFIED KEEP, playtest #9

> **ALL GATES PASS — the economy holds.**

## 1. Survival curves

Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).

| Metric | naive | skilled | noshop | martyr | chalk | hoarder | ironhands | martyr-worst |
|---|---|---|---|---|---|---|---|---|
| enter R1 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R2 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R3 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R4 | 86.6% | 99.0% | 100.0% | 39.3% | 100.0% | 100.0% | 98.8% | 100.0% |
| enter R5 | 43.4% | 78.5% | 68.3% | 26.6% | 81.1% | 73.1% | 76.2% | 45.0% |
| enter R6 | 10.6% | 50.6% | 17.5% | 14.7% | 38.0% | 29.9% | 47.4% | 34.3% |
| enter R7 | 0.3% | 29.4% | 1.6% | 7.0% | 15.2% | 8.7% | 28.4% | 19.4% |
| enter R8 | 0.0% | 16.8% | 0.0% | 3.1% | 6.8% | 2.4% | 17.8% | 10.6% |
| **won %** | **0.0%** | **7.6%** | **0.0%** | **1.4%** | **2.3%** | **0.3%** | **9.4%** | **5.8%** |
| **median death round** | **4** | **6** | **5** | **3** | **5** | **5** | **5** | **4** |
| mean rounds reached | 4.41 | 5.82 | 4.87 | 3.92 | 5.44 | 5.14 | 5.78 | 5.15 |
| totem fire rate | 0.0% | 37.9% | 0.0% | 0.0% | 16.2% | 11.5% | 27.1% | 0.0% |
| close-call deaths (% of deaths) | 13.3% | 5.5% | 3.2% | 4.4% | 4.9% | 4.1% | 5.9% | 6.0% |
| mean bookie gifts per run | 0.46 | 0.41 | 0.75 | 0.88 | 0.52 | 0.67 | 0.37 | 1.03 |

> Takeaway: naive dies at round 4, skilled reaches 6 — skill buys real extra survival; compare against the 3–4 / ≥7 targets above.

## 2. Item power audit (3 passives + 3 consumables)

Skilled baseline: median death 6, mean rounds 5.82, won 7.6%. Passives granted at run start; consumables refilled every round. Exposure = uses (consumables) or wound-up runs (ratchets); ±  is the paired-seed SE on Δwon. Sorted by Δ mean rounds survived.

| Item | kind | mean rounds | Δ mean | median death | won % | Δ won % (±SE) | exposure | totem fires |
|---|---|---|---|---|---|---|---|---|
| Mulligan Slip | consumable | 7.20 | +1.38 | 7 | 27.7% | +20.1pp (±0.2) | 202,006 uses | — |
| Free Bet Token | consumable | 6.60 | +0.78 | 7 | 2.2% | -5.4pp (±0.1) | 312,195 uses | — |
| Totem of Undying | passive | 6.38 | +0.57 | 6 | 8.8% | +1.2pp (±0.1) | — | 87.7% |
| Double or Nothing Slip | consumable | 6.33 | +0.51 | 6 | 19.8% | +12.2pp (±0.2) | 139,468 uses | — |
| Unopened Bobblehead | passive | 6.28 | +0.46 | 6 | 16.8% | +9.2pp (±0.2) | — | — |
| Profit Boost | consumable | 6.22 | +0.4 | 6 | 15.3% | +7.7pp (±0.1) | 285,561 uses | — |
| Ref's Whistle | consumable | 6.08 | +0.26 | 6 | 9.8% | +2.2pp (±0.1) | 42,705 uses | — |
| The Multiplier | passive | 5.99 | +0.17 | 5 | 25.2% | +17.6pp (±0.2) | — | — |
| Bookie's Marker | consumable | 5.97 | +0.15 | 6 | 7.3% | -0.3pp (±0.1) | 105,849 uses | — |
| Scar Tissue | passive | 5.94 | +0.12 | 6 | 8.7% | +1.1pp (±0.1) | 19,958 wound | — |
| The Rake's Rebate | passive | 5.90 | +0.08 | 6 | 10.3% | +2.7pp (±0.1) | — | — |
| Ask for the Manager | consumable | 5.81 | -0.01 | 6 | 7.7% | +0.1pp (±0.1) | 236,517 uses | — |
| Comp'd Suite | passive | 5.77 | -0.05 | 5 | 7.9% | +0.3pp (±0.1) | — | — |
| House Key | passive | 5.69 | -0.13 | 5 | 20.5% | +12.9pp (±0.2) | — | — |
| Golden Parachute | passive | 5.67 | -0.14 | 5 | 1.9% | -5.7pp (±0.1) | — | — |
| The Collection | passive | 5.63 | -0.19 | 5 | 16.9% | +9.3pp (±0.2) | — | — |
| Bad Beat Jar | passive | 5.62 | -0.2 | 5 | 16.4% | +8.8pp (±0.2) | 29,141 wound | — |
| Chalk Eater | passive | 5.59 | -0.23 | 5 | 17.2% | +9.6pp (±0.2) | 47,563 wound | — |
| The System | passive | 5.58 | -0.24 | 5 | 17.5% | +9.9pp (±0.2) | 20,105 wound | — |
| Iron Hands | passive | 5.55 | -0.27 | 5 | 16.2% | +8.6pp (±0.2) | 11,212 wound | — |
| Longshot Larry's Photo | passive | 5.55 | -0.27 | 5 | 15.7% | +8.1pp (±0.2) | — | — |
| Whale Card | passive | 4.98 | -0.84 | 4 | 12.0% | +4.4pp (±0.2) | — | — |

> Takeaway: the gate section's item flags carry the verdicts (DEAD / DOMINANT / totem band). Note the audit grants items FREE — organic play also pays shop prices out of payment headroom.

## 3. Variance feel

Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.

| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |
|---|---|---|---|---|---|---|---|---|
| naive | $87 | $111 | $184 | $222 | n/a | n/a | n/a | n/a |
| skilled | $72 | $138 | $989 | $2,882 | $117 | $1,193 | $4,712 | $13,179 |
| noshop | $51 | $119 | $191 | $367 | n/a | n/a | n/a | n/a |
| martyr | $87 | $87 | $844 | $4,569 | $1 | $2,224 | $16,501 | $73,407 |
| chalk | $48 | $110 | $366 | $1,780 | $73 | $638 | $3,237 | $18,022 |
| hoarder | $51 | $119 | $212 | $735 | $48 | $363 | $2,108 | $5,926 |
| ironhands | $80 | $141 | $1,224 | $3,572 | $120 | $1,439 | $6,053 | $22,030 |
| martyr-worst | $87 | $87 | $1,475 | $14,639 | $269 | $3,103 | $23,216 | $99,731 |

> Takeaway: swing p50 vs p99 shows whether the ride is flat (boring) or spiky (feels rigged); a p99 many multiples of p50 is the intended rare-blowup shape.

## 4. Ratchet telemetry (Scar Tissue)

Mean peak stacks (pp) and mean carrier burns per run — is the ratchet winding and cashing?

| Strategy | mean peak stacks | mean burns/run |
|---|---|---|
| naive | 0.0pp | 0.00 |
| skilled | 2.1pp | 0.00 |
| noshop | 0.0pp | 0.00 |
| martyr | 6.3pp | 0.00 |
| chalk | 0.2pp | 0.00 |
| hoarder | 0.0pp | 0.00 |
| ironhands | 0.7pp | 0.00 |
| martyr-worst | 28.2pp | 0.00 |

> Takeaway: zero across the board means nobody buys/holds the scar (price or power problem); huge stacks with zero burns means winding without cashing (the carrier rule isn't landing).

## 5. EV-arc

Mean *true* per-ticket EV at lock, by round (the economy doctrine's crossing from −vig toward positive).

| Round | naive mean EV | skilled mean EV | noshop mean EV | martyr mean EV | chalk mean EV | hoarder mean EV | ironhands mean EV | martyr-worst mean EV |
|---|---|---|---|---|---|---|---|---|
| R1 | -$8 | -$5 | -$5 | -$7 | -$4 | -$5 | -$5 | $32 |
| R2 | -$7 | $5 | -$6 | $8 | -$2 | -$6 | $7 | $36 |
| R3 | -$5 | $15 | -$2 | $16 | $2 | -$0 | $14 | $55 |
| R4 | -$3 | $28 | -$4 | $25 | $4 | $3 | $30 | $83 |
| R5 | -$3 | $47 | -$7 | $43 | $10 | $4 | $50 | $137 |
| R6 | -$3 | $64 | -$6 | $91 | $20 | $6 | $74 | $269 |
| R7 | -$2 | $88 | -$6 | $156 | $45 | $26 | $108 | $498 |
| R8 | — | $116 | -$6 | $331 | $94 | $82 | $137 | $967 |

- Skilled mean EV first crosses zero at **round 3** (target ≈ round 4).
- **Survivorship caveat:** the round-3+ means average only the few runs that got there (R3 n=48206 tickets vs R1 n=50000). The MEDIAN skilled run dies at round 2, so it never reaches the +EV band — the arc is real for the surviving tail, not the typical player.
- Naive mean EV never crosses zero — as intended.

> Takeaway: among survivors the +EV relics do flip the arc (crosses ~R3, target ≈R4), but almost nobody survives to bank it — the mechanic works, the economy gates it away.

## 6. Band-3 audit

Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).

| Strategy | p99 final bank | top-1% mean | max |
|---|---|---|---|
| naive | $250 | $301 | $375 |
| skilled | $3,985 | $7,346 | $50,319 |
| noshop | $143 | $221 | $655 |
| martyr | $1,044 | $10,596 | $744,515 |
| chalk | $1,258 | $3,713 | $76,840 |
| hoarder | $457 | $942 | $6,248 |
| ironhands | $5,790 | $15,664 | $808,401 |
| martyr-worst | $14,082 | $40,161 | $1,077,307 |


Pairwise relic synergy (50,000 runs/config, baseline won 0.0%). Synergy excess = pair Δwon − (soloA Δ + soloB Δ). Top 10:

| Pair | pair won % | synergy excess (pp) | tag |
|---|---|---|---|
| The Multiplier + House Key | 4.2% | +3.14 | degenerate: cheap pair, trivially assembled |
| The Multiplier + Whale Card | 3.6% | +2.6 | degenerate: cheap pair, trivially assembled |
| Whale Card + House Key | 1.4% | +1.06 | degenerate: cheap pair, trivially assembled |
| The Multiplier + The System | 1.7% | +0.75 | marginal (no real loop) |
| The Multiplier + Chalk Eater | 1.6% | +0.66 | marginal (no real loop) |
| The Multiplier + Iron Hands | 1.4% | +0.47 | marginal (no real loop) |
| The Multiplier + The Collection | 1.3% | +0.41 | marginal (no real loop) |
| The Multiplier + Comp'd Suite | 1.4% | +0.41 | marginal (no real loop) |
| The Multiplier + Bad Beat Jar | 1.3% | +0.37 | marginal (no real loop) |
| Whale Card + Comp'd Suite | 0.5% | +0.33 | marginal (no real loop) |

> Takeaway: brokenness should live in the tail (max ≫ p99) and, if combos ran, in a few hard-to-assemble pairs rather than trivial cheap ones.

## 7. Grind metric

Median decisions per round (tickets + cash-outs + buys). Fewer decisions late than mid = flat repetition.

| Round | naive | skilled | noshop | martyr | chalk | hoarder | ironhands | martyr-worst |
|---|---|---|---|---|---|---|---|---|
| R1 | 1 | 2 | 1 | 3 | 2 | 1 | 3 | 3 |
| R2 | 1 | 4 | 1 | 3 | 3 | 1 | 4 | 3 |
| R3 | 1 | 4 | 1 | 0 | 4 | 3 | 4 | 3 |
| R4 | 1 | 3 | 1 | 2 | 3 | 2 | 3 | 1 |
| R5 | 1 | 2 | 1 | 2 | 2 | 1 | 2 | 3 |
| R6 | 1 | 2 | 1 | 1 | 2 | 1 | 2 | 1 |
| R7 | 1 | 2 | 1 | 0 | 2 | 2 | 3 | 2 |
| R8 | — | 2 | 1 | 1 | 2 | 2 | 2 | 1 |

> Takeaway: late rounds carry FEWER decisions than mid for: skilled, martyr, chalk, hoarder, ironhands, martyr-worst — repetition-risk flag.

