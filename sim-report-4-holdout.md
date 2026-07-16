# /sim — Monte Carlo balance report

- Date: 2026-07-15 20:49
- Engine: workspace is not a git repo — `git describe` unavailable
- Config: bank $350, PAYMENTS [60, 70, 85, 105, 155, 375, 710, 1350] (avg ×1.56), overround 5.0%, cash-out margin 8.0%, totem juice 50.0%, min stake $10, max stake 100.0% of bank, 6 matchups/round, 3 tickets/round, 5 relic + 3 consumable slots
- Strategies: naive, skilled, noshop, martyr, chalk, hoarder, ironhands, martyr-worst
- Runs per batch: 50,000
- Total runs (incl. audit/combos): 7,550,000
- Wall time: 1086.25 s

## 0. Gate campaign (PLAN.md acceptance criteria)

| Gate | Criterion | Verdict | Actual |
|---|---|---|---|
| G1 | honest gambling: naive win <1%, dies before the cliff resolves (median ≤6) | **PASS** | median 4, won 0.0% |
| G2 | engine mandatory: no-shop skilled win <2%, median death 5–6 | **PASS** | median 5, won 0.0% |
| G3 | skilled + items wins: median death ≥5, win 5–8% (re-banded by Allen 2026-07-15 — the dealt hand's build variance is the roguelite shape) | **PASS** | median 5, won 7.0% |
| G4 | the EV arc exists: skilled mean ticket EV crosses zero in rounds 3–7 | **PASS** | crosses at R3 |
| G5 | composition superadditive: Multiplier+Scar pair Δwin > sum of solo Δwins | **PASS** | synergy excess +0.1pp |
| G6 | martyr guard (worst case granted): loss-farming win ≤ skilled +2pp | **PASS** | martyr-worst 5.9% vs skilled 7.0% (organic martyr 1.4%) |

- ⚑ DEAD: Ask for the Manager (Δwon -0.1±0.4pp, Δmean -0.01)

> **NOT DONE — iterate the knobs (item numbers / prices / curve) and rerun.**

## 1. Survival curves

Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).

| Metric | naive | skilled | noshop | martyr | chalk | hoarder | ironhands | martyr-worst |
|---|---|---|---|---|---|---|---|---|
| enter R1 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R2 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R3 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R4 | 86.3% | 99.0% | 100.0% | 39.4% | 100.0% | 100.0% | 98.7% | 100.0% |
| enter R5 | 43.2% | 77.8% | 67.9% | 26.8% | 80.4% | 72.7% | 75.3% | 44.9% |
| enter R6 | 10.2% | 49.3% | 17.6% | 14.7% | 36.1% | 29.5% | 45.8% | 34.2% |
| enter R7 | 0.2% | 28.0% | 1.6% | 7.0% | 14.0% | 8.3% | 27.0% | 19.3% |
| enter R8 | 0.0% | 15.7% | 0.0% | 3.1% | 6.1% | 2.2% | 16.6% | 10.7% |
| **won %** | **0.0%** | **7.0%** | **0.0%** | **1.4%** | **2.0%** | **0.3%** | **8.3%** | **5.9%** |
| **median death round** | **4** | **5** | **5** | **3** | **5** | **5** | **5** | **4** |
| mean rounds reached | 4.40 | 5.77 | 4.87 | 3.92 | 5.39 | 5.13 | 5.72 | 5.15 |
| totem fire rate | 0.0% | 37.5% | 0.0% | 0.0% | 15.7% | 11.6% | 27.0% | 0.0% |
| close-call deaths (% of deaths) | 13.4% | 5.3% | 3.3% | 4.6% | 4.9% | 4.2% | 5.7% | 5.9% |
| mean bookie gifts per run | 0.46 | 0.42 | 0.75 | 0.88 | 0.51 | 0.67 | 0.38 | 1.03 |

> Takeaway: naive dies at round 4, skilled reaches 5 — skill buys real extra survival; compare against the 3–4 / ≥7 targets above.

## 2. Item power audit (3 passives + 3 consumables)

Skilled baseline: median death 5, mean rounds 5.77, won 7.0%. Passives granted at run start; consumables refilled every round. Exposure = uses (consumables) or wound-up runs (ratchets); ±  is the paired-seed SE on Δwon. Sorted by Δ mean rounds survived.

| Item | kind | mean rounds | Δ mean | median death | won % | Δ won % (±SE) | exposure | totem fires |
|---|---|---|---|---|---|---|---|---|
| Mulligan Slip | consumable | 7.17 | +1.4 | 7 | 26.6% | +19.6pp (±0.2) | 200,081 uses | — |
| Free Bet Token | consumable | 6.61 | +0.84 | 7 | 1.9% | -5.0pp (±0.1) | 312,849 uses | — |
| Totem of Undying | passive | 6.34 | +0.58 | 6 | 8.1% | +1.1pp (±0.1) | — | 88.6% |
| Double or Nothing Slip | consumable | 6.27 | +0.5 | 6 | 18.3% | +11.3pp (±0.2) | 137,316 uses | — |
| Unopened Bobblehead | passive | 6.20 | +0.43 | 6 | 14.8% | +7.8pp (±0.2) | — | — |
| Profit Boost | consumable | 6.16 | +0.39 | 6 | 13.9% | +6.9pp (±0.1) | 282,002 uses | — |
| The Multiplier | passive | 5.92 | +0.15 | 5 | 23.4% | +16.4pp (±0.2) | — | — |
| Bookie's Marker | consumable | 5.92 | +0.15 | 6 | 6.6% | -0.4pp (±0.1) | 105,336 uses | — |
| Scar Tissue | passive | 5.89 | +0.12 | 6 | 7.9% | +0.9pp (±0.1) | 20,625 wound | — |
| Ref's Whistle | consumable | 5.87 | +0.1 | 6 | 7.3% | +0.3pp (±0.1) | 19,092 uses | — |
| The Rake's Rebate | passive | 5.84 | +0.07 | 6 | 9.4% | +2.5pp (±0.1) | — | — |
| Ask for the Manager | consumable | 5.76 | -0.01 | 5 | 6.9% | -0.1pp (±0.1) | 234,545 uses | — |
| Comp'd Suite | passive | 5.72 | -0.05 | 5 | 7.1% | +0.1pp (±0.1) | — | — |
| Golden Parachute | passive | 5.70 | -0.07 | 5 | 1.6% | -5.4pp (±0.1) | — | — |
| House Key | passive | 5.64 | -0.13 | 5 | 18.8% | +11.8pp (±0.2) | — | — |
| The Collection | passive | 5.57 | -0.2 | 5 | 15.4% | +8.4pp (±0.2) | — | — |
| Bad Beat Jar | passive | 5.55 | -0.22 | 5 | 14.8% | +7.8pp (±0.2) | 29,459 wound | — |
| The System | passive | 5.53 | -0.24 | 5 | 16.0% | +9.0pp (±0.2) | 18,636 wound | — |
| Chalk Eater | passive | 5.53 | -0.24 | 5 | 15.6% | +8.6pp (±0.2) | 47,528 wound | — |
| Iron Hands | passive | 5.49 | -0.28 | 5 | 14.6% | +7.7pp (±0.2) | 10,693 wound | — |
| Longshot Larry's Photo | passive | 5.49 | -0.28 | 5 | 14.2% | +7.2pp (±0.2) | — | — |
| Whale Card | passive | 4.95 | -0.82 | 4 | 11.0% | +4.0pp (±0.2) | — | — |

> Takeaway: the gate section's item flags carry the verdicts (DEAD / DOMINANT / totem band). Note the audit grants items FREE — organic play also pays shop prices out of payment headroom.

## 3. Variance feel

Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.

| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |
|---|---|---|---|---|---|---|---|---|
| naive | $87 | $109 | $184 | $221 | n/a | n/a | n/a | n/a |
| skilled | $71 | $137 | $867 | $2,787 | $97 | $1,181 | $4,594 | $13,584 |
| noshop | $50 | $118 | $190 | $368 | n/a | n/a | n/a | n/a |
| martyr | $87 | $87 | $840 | $4,607 | $1 | $2,052 | $17,766 | $85,580 |
| chalk | $48 | $107 | $331 | $1,687 | $56 | $593 | $3,448 | $22,894 |
| hoarder | $51 | $119 | $204 | $719 | $37 | $300 | $1,679 | $4,572 |
| ironhands | $79 | $139 | $1,072 | $3,352 | $104 | $1,418 | $6,032 | $18,314 |
| martyr-worst | $87 | $87 | $1,506 | $14,039 | $263 | $3,054 | $22,022 | $110,977 |

> Takeaway: swing p50 vs p99 shows whether the ride is flat (boring) or spiky (feels rigged); a p99 many multiples of p50 is the intended rare-blowup shape.

## 4. Ratchet telemetry (Scar Tissue)

Mean peak stacks (pp) and mean carrier burns per run — is the ratchet winding and cashing?

| Strategy | mean peak stacks | mean burns/run |
|---|---|---|
| naive | 0.0pp | 0.00 |
| skilled | 2.2pp | 0.00 |
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
| R2 | -$6 | $5 | -$6 | $8 | -$2 | -$6 | $6 | $36 |
| R3 | -$5 | $15 | -$2 | $16 | $2 | -$0 | $14 | $56 |
| R4 | -$3 | $27 | -$4 | $25 | $4 | $3 | $29 | $84 |
| R5 | -$3 | $45 | -$7 | $43 | $10 | $4 | $48 | $137 |
| R6 | -$3 | $63 | -$5 | $80 | $19 | $6 | $70 | $275 |
| R7 | -$2 | $88 | -$6 | $157 | $43 | $26 | $99 | $508 |
| R8 | — | $111 | -$7 | $277 | $87 | $78 | $121 | $977 |

- Skilled mean EV first crosses zero at **round 3** (target ≈ round 4).
- **Survivorship caveat:** the round-3+ means average only the few runs that got there (R3 n=48212 tickets vs R1 n=50000). The MEDIAN skilled run dies at round 2, so it never reaches the +EV band — the arc is real for the surviving tail, not the typical player.
- Naive mean EV never crosses zero — as intended.

> Takeaway: among survivors the +EV relics do flip the arc (crosses ~R3, target ≈R4), but almost nobody survives to bank it — the mechanic works, the economy gates it away.

## 6. Band-3 audit

Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).

| Strategy | p99 final bank | top-1% mean | max |
|---|---|---|---|
| naive | $250 | $302 | $375 |
| skilled | $3,741 | $7,193 | $98,878 |
| noshop | $144 | $221 | $679 |
| martyr | $1,009 | $9,657 | $268,495 |
| chalk | $1,177 | $4,074 | $230,659 |
| hoarder | $433 | $873 | $9,040 |
| ironhands | $5,343 | $10,609 | $260,544 |
| martyr-worst | $13,702 | $41,013 | $953,524 |


Pairwise relic synergy (50,000 runs/config, baseline won 0.0%). Synergy excess = pair Δwon − (soloA Δ + soloB Δ). Top 10:

| Pair | pair won % | synergy excess (pp) | tag |
|---|---|---|---|
| The Multiplier + House Key | 4.1% | +2.87 | degenerate: cheap pair, trivially assembled |
| The Multiplier + Whale Card | 3.6% | +2.53 | degenerate: cheap pair, trivially assembled |
| Whale Card + House Key | 1.4% | +1.04 | degenerate: cheap pair, trivially assembled |
| The Multiplier + The System | 1.7% | +0.69 | marginal (no real loop) |
| The Multiplier + Chalk Eater | 1.7% | +0.69 | marginal (no real loop) |
| The Multiplier + Iron Hands | 1.5% | +0.49 | marginal (no real loop) |
| The Multiplier + The Collection | 1.4% | +0.42 | marginal (no real loop) |
| The Multiplier + Bad Beat Jar | 1.4% | +0.39 | marginal (no real loop) |
| Whale Card + Comp'd Suite | 0.6% | +0.38 | marginal (no real loop) |
| The Multiplier + Comp'd Suite | 1.4% | +0.33 | marginal (no real loop) |

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
| R7 | 1 | 2 | 1 | 1 | 2 | 1 | 2 | 2 |
| R8 | — | 2 | 1 | 0 | 2 | 2 | 2 | 1 |

> Takeaway: late rounds carry FEWER decisions than mid for: skilled, martyr, chalk, hoarder, ironhands, martyr-worst — repetition-risk flag.

