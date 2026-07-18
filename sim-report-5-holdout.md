# /sim — Monte Carlo balance report

- Date: 2026-07-18 01:36
- Engine: workspace is not a git repo — `git describe` unavailable
- Config: bank $350, PAYMENTS [60, 70, 85, 105, 155, 375, 710, 1350] (avg ×1.56), overround 5.0%, cash-out margin 8.0%, totem juice 50.0%, min stake $10, max stake 100.0% of bank, 6 matchups/round, 3 tickets/round, 5 relic + 3 consumable slots
- Strategies: naive, skilled, noshop, martyr, chalk, hoarder, ironhands, martyr-worst
- Runs per batch: 50,000
- Total runs (incl. audit/combos): 7,550,000
- Wall time: 806.60 s

## 0. Gate campaign (PLAN.md acceptance criteria)

| Gate | Criterion | Verdict | Actual |
|---|---|---|---|
| G1 | honest gambling: naive win <1%, dies before the cliff resolves (median ≤6) | **PASS** | median 4, won 0.0% |
| G2 | engine mandatory: no-shop skilled win <2%, median death 5–6 | **PASS** | median 5, won 0.0% |
| G3 | skilled + items wins: median death ≥5, win 5–8% (re-banded by Allen 2026-07-15 — the dealt hand's build variance is the roguelite shape) | **PASS** | median 6, won 7.6% |
| G4 | the EV arc exists: skilled mean ticket EV crosses zero in rounds 3–7 | **PASS** | crosses at R3 |
| G5 | composition superadditive: Multiplier+Scar pair Δwin > sum of solo Δwins | **PASS** | synergy excess +0.0pp |
| G6 | martyr guard (worst case granted): loss-farming win ≤ skilled +2pp | **PASS** | martyr-worst 5.9% vs skilled 7.6% (organic martyr 1.5%) |

Item flags: none — no DEAD items, no DOMINANT item, Totem in the healthy band.
- ℹ PLAYTEST-GATED: Ask for the Manager audits ≈0 through bots (Δwon -+0.0±0.4pp) — RATIFIED KEEP, playtest #9

> **ALL GATES PASS — the economy holds.**

## 1. Survival curves

Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).

| Metric | naive | skilled | noshop | martyr | chalk | hoarder | ironhands | martyr-worst |
|---|---|---|---|---|---|---|---|---|
| enter R1 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R2 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R3 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R4 | 86.6% | 99.0% | 100.0% | 39.5% | 100.0% | 100.0% | 98.8% | 100.0% |
| enter R5 | 43.3% | 78.2% | 68.4% | 26.6% | 81.1% | 73.2% | 75.9% | 45.1% |
| enter R6 | 10.3% | 50.1% | 17.5% | 14.7% | 37.7% | 30.0% | 46.8% | 34.2% |
| enter R7 | 0.3% | 29.0% | 1.7% | 6.9% | 15.1% | 8.5% | 28.2% | 19.5% |
| enter R8 | 0.0% | 16.7% | 0.0% | 3.1% | 6.6% | 2.4% | 17.6% | 10.5% |
| **won %** | **0.0%** | **7.6%** | **0.0%** | **1.5%** | **2.4%** | **0.3%** | **9.4%** | **5.9%** |
| **median death round** | **4** | **6** | **5** | **3** | **5** | **5** | **5** | **4** |
| mean rounds reached | 4.41 | 5.81 | 4.88 | 3.92 | 5.43 | 5.14 | 5.77 | 5.15 |
| totem fire rate | 0.0% | 37.4% | 0.0% | 0.0% | 15.8% | 11.4% | 26.6% | 0.0% |
| close-call deaths (% of deaths) | 13.3% | 5.6% | 3.3% | 4.4% | 5.0% | 4.2% | 5.9% | 5.8% |
| mean bookie gifts per run | 0.46 | 0.42 | 0.75 | 0.87 | 0.52 | 0.67 | 0.38 | 1.03 |

> Takeaway: naive dies at round 4, skilled reaches 6 — skill buys real extra survival; compare against the 3–4 / ≥7 targets above.

## 2. Item power audit (3 passives + 3 consumables)

Skilled baseline: median death 6, mean rounds 5.81, won 7.6%. Passives granted at run start; consumables refilled every round. Exposure = uses (consumables) or wound-up runs (ratchets); ±  is the paired-seed SE on Δwon. Sorted by Δ mean rounds survived.

| Item | kind | mean rounds | Δ mean | median death | won % | Δ won % (±SE) | exposure | totem fires |
|---|---|---|---|---|---|---|---|---|
| Mulligan Slip | consumable | 7.19 | +1.38 | 7 | 27.5% | +19.9pp (±0.2) | 201,429 uses | — |
| Free Bet Token | consumable | 6.60 | +0.79 | 7 | 2.1% | -5.4pp (±0.1) | 312,089 uses | — |
| Totem of Undying | passive | 6.38 | +0.58 | 6 | 8.9% | +1.4pp (±0.1) | — | 87.6% |
| Double or Nothing Slip | consumable | 6.33 | +0.52 | 6 | 20.0% | +12.4pp (±0.2) | 139,292 uses | — |
| Unopened Bobblehead | passive | 6.26 | +0.45 | 6 | 16.5% | +8.9pp (±0.2) | — | — |
| Profit Boost | consumable | 6.20 | +0.4 | 6 | 15.3% | +7.8pp (±0.1) | 284,795 uses | — |
| Ref's Whistle | consumable | 6.07 | +0.26 | 6 | 9.6% | +2.1pp (±0.1) | 42,754 uses | — |
| The Multiplier | passive | 5.96 | +0.16 | 5 | 24.8% | +17.2pp (±0.2) | — | — |
| Bookie's Marker | consumable | 5.96 | +0.15 | 6 | 7.3% | -0.3pp (±0.1) | 105,562 uses | — |
| Scar Tissue | passive | 5.94 | +0.13 | 6 | 8.9% | +1.4pp (±0.1) | 20,084 wound | — |
| The Rake's Rebate | passive | 5.89 | +0.08 | 6 | 10.5% | +3.0pp (±0.1) | — | — |
| Ask for the Manager | consumable | 5.81 | 0 | 6 | 7.5% | 0.0pp (±0.1) | 236,622 uses | — |
| Comp'd Suite | passive | 5.77 | -0.04 | 5 | 7.9% | +0.3pp (±0.1) | — | — |
| Golden Parachute | passive | 5.68 | -0.13 | 5 | 1.8% | -5.8pp (±0.1) | — | — |
| House Key | passive | 5.66 | -0.14 | 5 | 20.0% | +12.4pp (±0.2) | — | — |
| The Collection | passive | 5.60 | -0.2 | 5 | 16.7% | +9.2pp (±0.2) | — | — |
| Bad Beat Jar | passive | 5.60 | -0.21 | 5 | 16.1% | +8.5pp (±0.2) | 29,299 wound | — |
| Chalk Eater | passive | 5.56 | -0.24 | 5 | 16.9% | +9.4pp (±0.2) | 47,522 wound | — |
| The System | passive | 5.56 | -0.25 | 5 | 17.2% | +9.6pp (±0.2) | 20,336 wound | — |
| Iron Hands | passive | 5.53 | -0.28 | 5 | 15.9% | +8.4pp (±0.2) | 11,477 wound | — |
| Longshot Larry's Photo | passive | 5.52 | -0.28 | 5 | 15.5% | +7.9pp (±0.2) | — | — |
| Whale Card | passive | 4.97 | -0.84 | 4 | 11.9% | +4.3pp (±0.2) | — | — |

> Takeaway: the gate section's item flags carry the verdicts (DEAD / DOMINANT / totem band). Note the audit grants items FREE — organic play also pays shop prices out of payment headroom.

## 3. Variance feel

Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.

| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |
|---|---|---|---|---|---|---|---|---|
| naive | $87 | $109 | $184 | $221 | n/a | n/a | n/a | n/a |
| skilled | $73 | $138 | $995 | $2,930 | $116 | $1,294 | $4,800 | $13,178 |
| noshop | $51 | $119 | $191 | $368 | n/a | n/a | n/a | n/a |
| martyr | $87 | $87 | $847 | $5,116 | $1 | $2,191 | $16,665 | $116,254 |
| chalk | $48 | $110 | $362 | $1,789 | $65 | $662 | $3,166 | $18,465 |
| hoarder | $51 | $119 | $212 | $739 | $44 | $290 | $1,928 | $6,785 |
| ironhands | $80 | $141 | $1,211 | $3,661 | $133 | $1,492 | $6,503 | $19,760 |
| martyr-worst | $87 | $87 | $1,458 | $15,248 | $299 | $3,251 | $22,781 | $112,381 |

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
| R2 | -$6 | $5 | -$6 | $8 | -$2 | -$6 | $7 | $36 |
| R3 | -$5 | $15 | -$2 | $16 | $2 | -$1 | $14 | $56 |
| R4 | -$3 | $28 | -$4 | $25 | $4 | $3 | $30 | $83 |
| R5 | -$3 | $46 | -$7 | $46 | $10 | $4 | $51 | $137 |
| R6 | -$3 | $65 | -$6 | $98 | $20 | $5 | $76 | $273 |
| R7 | -$2 | $90 | -$5 | $192 | $45 | $25 | $105 | $506 |
| R8 | — | $121 | -$8 | $413 | $95 | $76 | $137 | $985 |

- Skilled mean EV first crosses zero at **round 3** (target ≈ round 4).
- **Survivorship caveat:** the round-3+ means average only the few runs that got there (R3 n=48162 tickets vs R1 n=50000). The MEDIAN skilled run dies at round 2, so it never reaches the +EV band — the arc is real for the surviving tail, not the typical player.
- Naive mean EV never crosses zero — as intended.

> Takeaway: among survivors the +EV relics do flip the arc (crosses ~R3, target ≈R4), but almost nobody survives to bank it — the mechanic works, the economy gates it away.

## 6. Band-3 audit

Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).

| Strategy | p99 final bank | top-1% mean | max |
|---|---|---|---|
| naive | $243 | $300 | $375 |
| skilled | $4,074 | $7,491 | $43,125 |
| noshop | $143 | $226 | $645 |
| martyr | $1,174 | $11,732 | $361,416 |
| chalk | $1,265 | $3,687 | $50,416 |
| hoarder | $478 | $966 | $11,786 |
| ironhands | $6,199 | $33,163 | $10,585,443 |
| martyr-worst | $14,813 | $41,866 | $531,767 |


Pairwise relic synergy (50,000 runs/config, baseline won 0.0%). Synergy excess = pair Δwon − (soloA Δ + soloB Δ). Top 10:

| Pair | pair won % | synergy excess (pp) | tag |
|---|---|---|---|
| The Multiplier + House Key | 4.2% | +3.01 | degenerate: cheap pair, trivially assembled |
| The Multiplier + Whale Card | 3.6% | +2.5 | degenerate: cheap pair, trivially assembled |
| Whale Card + House Key | 1.4% | +1.11 | degenerate: cheap pair, trivially assembled |
| The Multiplier + The System | 1.7% | +0.66 | marginal (no real loop) |
| The Multiplier + Chalk Eater | 1.6% | +0.62 | marginal (no real loop) |
| The Multiplier + Iron Hands | 1.4% | +0.45 | marginal (no real loop) |
| Whale Card + Comp'd Suite | 0.6% | +0.37 | marginal (no real loop) |
| The Multiplier + The Collection | 1.4% | +0.36 | marginal (no real loop) |
| Chalk Eater + House Key | 0.5% | +0.36 | marginal (no real loop) |
| House Key + The System | 0.6% | +0.36 | marginal (no real loop) |

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
| R7 | 1 | 2 | 1 | 1 | 2 | 2 | 3 | 2 |
| R8 | — | 2 | 1 | 1 | 2 | 2 | 2 | 1 |

> Takeaway: late rounds carry FEWER decisions than mid for: skilled, martyr, chalk, hoarder, ironhands, martyr-worst — repetition-risk flag.

