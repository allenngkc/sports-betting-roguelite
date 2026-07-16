# /sim — Monte Carlo balance report

- Date: 2026-07-15 21:16
- Engine: workspace is not a git repo — `git describe` unavailable
- Config: bank $350, PAYMENTS [60, 70, 85, 105, 155, 375, 710, 1350] (avg ×1.56), overround 5.0%, cash-out margin 8.0%, totem juice 50.0%, min stake $10, max stake 100.0% of bank, 6 matchups/round, 3 tickets/round, 5 relic + 3 consumable slots
- Strategies: naive, skilled, noshop, martyr, chalk, hoarder, ironhands, martyr-worst
- Runs per batch: 10,000
- Total runs (incl. audit/combos): 1,510,000
- Wall time: 218.33 s

## 0. Gate campaign (PLAN.md acceptance criteria)

| Gate | Criterion | Verdict | Actual |
|---|---|---|---|
| G1 | honest gambling: naive win <1%, dies before the cliff resolves (median ≤6) | **PASS** | median 4, won 0.0% |
| G2 | engine mandatory: no-shop skilled win <2%, median death 5–6 | **PASS** | median 5, won 0.0% |
| G3 | skilled + items wins: median death ≥5, win 5–8% (re-banded by Allen 2026-07-15 — the dealt hand's build variance is the roguelite shape) | **PASS** | median 5, won 6.8% |
| G4 | the EV arc exists: skilled mean ticket EV crosses zero in rounds 3–7 | **PASS** | crosses at R3 |
| G5 | composition superadditive: Multiplier+Scar pair Δwin > sum of solo Δwins | **PASS** | synergy excess +0.1pp |
| G6 | martyr guard (worst case granted): loss-farming win ≤ skilled +2pp | **PASS** | martyr-worst 5.4% vs skilled 6.8% (organic martyr 1.4%) |

Item flags: none — no DEAD items, no DOMINANT item, Totem in the healthy band.
- ℹ PLAYTEST-GATED: Ask for the Manager audits ≈0 through bots (Δwon +0.3±0.9pp) — playtest #9 votes

> **ALL GATES PASS — the economy holds.**

## 1. Survival curves

Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).

| Metric | naive | skilled | noshop | martyr | chalk | hoarder | ironhands | martyr-worst |
|---|---|---|---|---|---|---|---|---|
| enter R1 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R2 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R3 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R4 | 87.0% | 99.0% | 100.0% | 39.0% | 100.0% | 100.0% | 98.8% | 100.0% |
| enter R5 | 43.8% | 77.8% | 67.5% | 26.1% | 80.1% | 72.4% | 75.2% | 44.5% |
| enter R6 | 10.5% | 48.8% | 17.5% | 14.1% | 36.4% | 28.8% | 46.1% | 33.9% |
| enter R7 | 0.2% | 28.1% | 1.6% | 6.7% | 14.4% | 8.0% | 26.9% | 19.0% |
| enter R8 | 0.0% | 15.3% | 0.0% | 3.0% | 5.9% | 2.0% | 16.2% | 10.3% |
| **won %** | **0.0%** | **6.8%** | **0.0%** | **1.4%** | **1.8%** | **0.3%** | **8.3%** | **5.4%** |
| **median death round** | **4** | **5** | **5** | **3** | **5** | **5** | **5** | **4** |
| mean rounds reached | 4.41 | 5.76 | 4.87 | 3.90 | 5.39 | 5.12 | 5.71 | 5.13 |
| totem fire rate | 0.0% | 37.9% | 0.0% | 0.0% | 16.0% | 11.2% | 27.4% | 0.0% |
| close-call deaths (% of deaths) | 13.5% | 5.5% | 3.5% | 4.3% | 4.5% | 4.5% | 6.2% | 5.9% |
| mean bookie gifts per run | 0.46 | 0.42 | 0.76 | 0.88 | 0.52 | 0.68 | 0.37 | 1.03 |

> Takeaway: naive dies at round 4, skilled reaches 5 — skill buys real extra survival; compare against the 3–4 / ≥7 targets above.

## 2. Item power audit (3 passives + 3 consumables)

Skilled baseline: median death 5, mean rounds 5.76, won 6.8%. Passives granted at run start; consumables refilled every round. Exposure = uses (consumables) or wound-up runs (ratchets); ±  is the paired-seed SE on Δwon. Sorted by Δ mean rounds survived.

| Item | kind | mean rounds | Δ mean | median death | won % | Δ won % (±SE) | exposure | totem fires |
|---|---|---|---|---|---|---|---|---|
| Mulligan Slip | consumable | 7.16 | +1.4 | 7 | 26.8% | +20.0pp (±0.4) | 40,067 uses | — |
| Free Bet Token | consumable | 6.59 | +0.84 | 7 | 2.0% | -4.8pp (±0.3) | 62,481 uses | — |
| Totem of Undying | passive | 6.33 | +0.57 | 6 | 7.6% | +0.8pp (±0.3) | — | 88.9% |
| Double or Nothing Slip | consumable | 6.26 | +0.51 | 6 | 18.4% | +11.6pp (±0.3) | 27,410 uses | — |
| Unopened Bobblehead | passive | 6.18 | +0.43 | 6 | 14.8% | +8.0pp (±0.4) | — | — |
| Profit Boost | consumable | 6.15 | +0.39 | 6 | 13.7% | +6.9pp (±0.3) | 56,243 uses | — |
| The Multiplier | passive | 5.95 | +0.19 | 5 | 24.1% | +17.2pp (±0.4) | — | — |
| Bookie's Marker | consumable | 5.90 | +0.14 | 6 | 6.4% | -0.4pp (±0.2) | 21,130 uses | — |
| Scar Tissue | passive | 5.87 | +0.11 | 6 | 7.6% | +0.8pp (±0.3) | 4,177 wound | — |
| Ref's Whistle | consumable | 5.86 | +0.1 | 6 | 7.1% | +0.3pp (±0.2) | 3,868 uses | — |
| The Rake's Rebate | passive | 5.85 | +0.09 | 6 | 9.4% | +2.6pp (±0.3) | — | — |
| Ask for the Manager | consumable | 5.74 | -0.01 | 5 | 7.1% | +0.3pp (±0.3) | 46,734 uses | — |
| Comp'd Suite | passive | 5.71 | -0.04 | 5 | 6.8% | 0.0pp (±0.3) | — | — |
| Golden Parachute | passive | 5.70 | -0.06 | 6 | 1.6% | -5.2pp (±0.3) | — | — |
| House Key | passive | 5.65 | -0.11 | 5 | 18.9% | +12.1pp (±0.4) | — | — |
| The Collection | passive | 5.59 | -0.17 | 5 | 15.7% | +8.9pp (±0.4) | — | — |
| Bad Beat Jar | passive | 5.58 | -0.18 | 5 | 15.0% | +8.2pp (±0.4) | 5,843 wound | — |
| Chalk Eater | passive | 5.55 | -0.21 | 5 | 15.6% | +8.8pp (±0.4) | 9,493 wound | — |
| The System | passive | 5.54 | -0.22 | 5 | 16.0% | +9.2pp (±0.4) | 3,694 wound | — |
| Iron Hands | passive | 5.51 | -0.24 | 5 | 14.7% | +7.9pp (±0.4) | 2,073 wound | — |
| Longshot Larry's Photo | passive | 5.51 | -0.25 | 5 | 14.2% | +7.4pp (±0.4) | — | — |
| Whale Card | passive | 4.96 | -0.8 | 4 | 11.3% | +4.5pp (±0.3) | — | — |

> Takeaway: the gate section's item flags carry the verdicts (DEAD / DOMINANT / totem band). Note the audit grants items FREE — organic play also pays shop prices out of payment headroom.

## 3. Variance feel

Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.

| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |
|---|---|---|---|---|---|---|---|---|
| naive | $87 | $110 | $184 | $223 | n/a | n/a | n/a | n/a |
| skilled | $71 | $137 | $843 | $2,714 | $90 | $1,141 | $4,630 | $11,485 |
| noshop | $51 | $119 | $190 | $367 | n/a | n/a | n/a | n/a |
| martyr | $87 | $87 | $832 | $4,324 | $1 | $2,028 | $14,654 | $34,030 |
| chalk | $48 | $107 | $330 | $1,621 | $39 | $607 | $2,699 | $22,036 |
| hoarder | $51 | $119 | $197 | $700 | $88 | $441 | $2,080 | $4,528 |
| ironhands | $78 | $139 | $1,061 | $3,269 | $82 | $1,390 | $5,869 | $25,160 |
| martyr-worst | $87 | $87 | $1,417 | $14,017 | $249 | $3,145 | $24,095 | $64,166 |

> Takeaway: swing p50 vs p99 shows whether the ride is flat (boring) or spiky (feels rigged); a p99 many multiples of p50 is the intended rare-blowup shape.

## 4. Ratchet telemetry (Scar Tissue)

Mean peak stacks (pp) and mean carrier burns per run — is the ratchet winding and cashing?

| Strategy | mean peak stacks | mean burns/run |
|---|---|---|
| naive | 0.0pp | 0.00 |
| skilled | 2.2pp | 0.00 |
| noshop | 0.0pp | 0.00 |
| martyr | 6.2pp | 0.00 |
| chalk | 0.2pp | 0.00 |
| hoarder | 0.0pp | 0.00 |
| ironhands | 0.7pp | 0.00 |
| martyr-worst | 28.1pp | 0.00 |

> Takeaway: zero across the board means nobody buys/holds the scar (price or power problem); huge stacks with zero burns means winding without cashing (the carrier rule isn't landing).

## 5. EV-arc

Mean *true* per-ticket EV at lock, by round (the economy doctrine's crossing from −vig toward positive).

| Round | naive mean EV | skilled mean EV | noshop mean EV | martyr mean EV | chalk mean EV | hoarder mean EV | ironhands mean EV | martyr-worst mean EV |
|---|---|---|---|---|---|---|---|---|
| R1 | -$8 | -$5 | -$5 | -$7 | -$4 | -$5 | -$5 | $32 |
| R2 | -$6 | $4 | -$6 | $8 | -$2 | -$6 | $6 | $36 |
| R3 | -$5 | $15 | -$2 | $15 | $2 | -$1 | $14 | $55 |
| R4 | -$3 | $27 | -$4 | $24 | $4 | $3 | $28 | $81 |
| R5 | -$3 | $46 | -$7 | $41 | $10 | $4 | $49 | $131 |
| R6 | -$3 | $63 | -$5 | $75 | $18 | $8 | $71 | $258 |
| R7 | -$1 | $86 | -$6 | $154 | $42 | $27 | $103 | $471 |
| R8 | — | $112 | -$8 | $301 | $88 | $81 | $125 | $907 |

- Skilled mean EV first crosses zero at **round 3** (target ≈ round 4).
- **Survivorship caveat:** the round-3+ means average only the few runs that got there (R3 n=9646 tickets vs R1 n=10000). The MEDIAN skilled run dies at round 2, so it never reaches the +EV band — the arc is real for the surviving tail, not the typical player.
- Naive mean EV never crosses zero — as intended.

> Takeaway: among survivors the +EV relics do flip the arc (crosses ~R3, target ≈R4), but almost nobody survives to bank it — the mechanic works, the economy gates it away.

## 6. Band-3 audit

Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).

| Strategy | p99 final bank | top-1% mean | max |
|---|---|---|---|
| naive | $250 | $300 | $373 |
| skilled | $3,293 | $6,534 | $29,651 |
| noshop | $137 | $214 | $479 |
| martyr | $970 | $6,877 | $37,845 |
| chalk | $1,088 | $3,326 | $70,745 |
| hoarder | $436 | $939 | $5,014 |
| ironhands | $5,041 | $15,762 | $309,027 |
| martyr-worst | $13,967 | $30,692 | $146,364 |


Pairwise relic synergy (10,000 runs/config, baseline won 0.0%). Synergy excess = pair Δwon − (soloA Δ + soloB Δ). Top 10:

| Pair | pair won % | synergy excess (pp) | tag |
|---|---|---|---|
| The Multiplier + House Key | 4.2% | +3.08 | degenerate: cheap pair, trivially assembled |
| The Multiplier + Whale Card | 3.5% | +2.42 | degenerate: cheap pair, trivially assembled |
| Whale Card + House Key | 1.3% | +1.1 | degenerate: cheap pair, trivially assembled |
| The Multiplier + The System | 1.7% | +0.68 | marginal (no real loop) |
| The Multiplier + Chalk Eater | 1.6% | +0.6 | marginal (no real loop) |
| The Multiplier + Iron Hands | 1.4% | +0.41 | marginal (no real loop) |
| The Multiplier + The Collection | 1.4% | +0.38 | marginal (no real loop) |
| Whale Card + Comp'd Suite | 0.6% | +0.35 | marginal (no real loop) |
| Chalk Eater + House Key | 0.5% | +0.34 | marginal (no real loop) |
| House Key + The System | 0.5% | +0.33 | marginal (no real loop) |

> Takeaway: brokenness should live in the tail (max ≫ p99) and, if combos ran, in a few hard-to-assemble pairs rather than trivial cheap ones.

## 7. Grind metric

Median decisions per round (tickets + cash-outs + buys). Fewer decisions late than mid = flat repetition.

| Round | naive | skilled | noshop | martyr | chalk | hoarder | ironhands | martyr-worst |
|---|---|---|---|---|---|---|---|---|
| R1 | 1 | 3 | 1 | 3 | 2 | 1 | 3 | 3 |
| R2 | 1 | 4 | 1 | 3 | 3 | 1 | 4 | 3 |
| R3 | 1 | 4 | 1 | 0 | 4 | 3 | 4 | 3 |
| R4 | 1 | 3 | 1 | 2 | 3 | 2 | 3 | 1 |
| R5 | 1 | 2 | 1 | 2 | 2 | 1 | 2 | 3 |
| R6 | 1 | 2 | 1 | 1 | 2 | 1 | 2 | 1 |
| R7 | 1 | 2 | 1 | 0 | 2 | 1 | 2 | 2 |
| R8 | — | 2 | 1 | 1 | 2 | 2 | 2 | 1 |

> Takeaway: late rounds carry FEWER decisions than mid for: skilled, martyr, chalk, hoarder, ironhands, martyr-worst — repetition-risk flag.

