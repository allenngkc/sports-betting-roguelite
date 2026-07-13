# /sim — Monte Carlo balance report

- Date: 2026-07-12 22:22
- Engine: workspace is not a git repo — `git describe` unavailable
- Config: bank $750, PAYMENTS [90, 110, 130, 155, 295, 560, 1065, 2025] (avg ×1.56), overround 5.0%, cash-out margin 8.0%, totem juice 50.0%, min stake $10, max stake 100.0% of bank, 6 matchups/round, 3 tickets/round, 5 relic + 2 consumable slots
- Strategies: naive, skilled, noshop, martyr
- Runs per batch: 10,000
- Total runs (incl. audit/combos): 160,000
- Wall time: 9.36 s

## 0. Gate campaign (PLAN.md acceptance criteria)

| Gate | Criterion | Verdict | Actual |
|---|---|---|---|
| G1 | honest gambling: naive median death 3–4, win <1% | **FAIL** | median 5, won 0.0% |
| G2 | engine mandatory: no-shop skilled median death 5–6, win <2% | **PASS** | median 6, won 0.0% |
| G3 | skilled + items wins: median death ≥7, win 10–15% | **FAIL** | median 6, won 14.2% |
| G4 | the EV arc exists: skilled mean ticket EV crosses zero in rounds 4–7 | **FAIL** | crosses at R2 |
| G5 | composition superadditive: Multiplier+Scar pair Δwin > sum of solo Δwins | **FAIL** | synergy excess -9.0pp |
| G6 | martyr guard: scar-farming bot win ≤ skilled +2pp | **PASS** | martyr 1.2% vs skilled 14.2% |

- ⚑ TOTEM: Δmean +0.45 (want ≥0.3), fire rate 66% (want 25–60%)

> **NOT DONE — iterate the knobs (item numbers / prices / curve) and rerun.**

## 1. Survival curves

Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).

| Metric | naive | skilled | noshop | martyr |
|---|---|---|---|---|
| enter R1 | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R2 | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R3 | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R4 | 100.0% | 100.0% | 100.0% | 39.6% |
| enter R5 | 78.7% | 87.1% | 100.0% | 27.7% |
| enter R6 | 28.3% | 57.9% | 70.4% | 13.4% |
| enter R7 | 3.2% | 40.1% | 0.0% | 6.0% |
| enter R8 | 0.0% | 28.9% | 0.0% | 2.6% |
| **won %** | **0.0%** | **14.2%** | **0.0%** | **1.2%** |
| **median death round** | **5** | **6** | **6** | **3** |
| mean rounds reached | 5.10 | 6.28 | 5.70 | 3.90 |
| totem fire rate | 0.0% | 78.8% | 0.0% | 0.0% |
| close-call deaths (% of deaths) | 12.3% | 0.4% | 2.2% | 4.6% |
| mean bookie gifts per run | 0.54 | 0.48 | 0.95 | 0.88 |

> Takeaway: naive dies at round 5, skilled reaches 6 — skill buys real extra survival; compare against the 3–4 / ≥7 targets above.

## 2. Item power audit (3 passives + 3 consumables)

Skilled baseline: median death 6, mean rounds 6.28, won 14.2%. Passives granted at run start; consumables refilled every round. Timeout is exempt from the DEAD flag — bots never play it (playtest-gated). Sorted by Δ mean rounds survived.

| Item | kind | mean rounds | Δ mean | median death | won % | Δ won % | totem fires |
|---|---|---|---|---|---|---|---|
| Mulligan Slip | consumable | 7.16 | +0.87 | 8 | 29.4% | +15.2pp | — |
| Totem of Undying | passive | 6.73 | +0.45 | 6 | 26.9% | +12.7pp | 66.5% |
| Scar Tissue | passive | 6.64 | +0.36 | 6 | 23.9% | +9.6pp | — |
| Profit Boost | consumable | 6.54 | +0.26 | 6 | 21.1% | +6.8pp | — |
| Timeout | consumable | 6.18 | -0.1 | 6 | 9.9% | -4.4pp | — |
| The Multiplier | passive | 5.47 | -0.82 | 4 | 21.4% | +7.2pp | — |

> Takeaway: the gate section's item flags carry the verdicts (DEAD / DOMINANT / totem band). Note the audit grants items FREE — organic play also pays shop prices out of payment headroom.

## 3. Variance feel

Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.

| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |
|---|---|---|---|---|---|---|---|---|
| naive | $187 | $273 | $416 | $527 | n/a | n/a | n/a | n/a |
| skilled | $105 | $325 | $1,971 | $2,635 | $162 | $1,235 | $2,773 | $4,469 |
| noshop | $85 | $198 | $306 | $404 | n/a | n/a | n/a | n/a |
| martyr | $187 | $187 | $1,726 | $8,007 | $0 | $1,281 | $21,236 | $141,194 |

> Takeaway: swing p50 vs p99 shows whether the ride is flat (boring) or spiky (feels rigged); a p99 many multiples of p50 is the intended rare-blowup shape.

## 4. Ratchet telemetry (Scar Tissue)

Mean peak stacks (pp) and mean carrier burns per run — is the ratchet winding and cashing?

| Strategy | mean peak stacks | mean burns/run |
|---|---|---|
| naive | 0.0pp | 0.00 |
| skilled | 5.8pp | 0.00 |
| noshop | 0.0pp | 0.00 |
| martyr | 15.0pp | 0.00 |

> Takeaway: zero across the board means nobody buys/holds the scar (price or power problem); huge stacks with zero burns means winding without cashing (the carrier rule isn't landing).

## 5. EV-arc

Mean *true* per-ticket EV at lock, by round (the economy doctrine's crossing from −vig toward positive).

| Round | naive mean EV | skilled mean EV | noshop mean EV | martyr mean EV |
|---|---|---|---|---|
| R1 | -$17 | -$1 | -$1 | -$15 |
| R2 | -$15 | $55 | -$1 | -$11 |
| R3 | -$12 | $53 | -$1 | -$13 |
| R4 | -$9 | $71 | -$1 | -$12 |
| R5 | -$7 | $102 | -$2 | -$18 |
| R6 | -$6 | $119 | -$4 | -$38 |
| R7 | -$7 | $117 | — | -$48 |
| R8 | -$5 | $81 | — | -$94 |

- Skilled mean EV first crosses zero at **round 2** (target ≈ round 4).
- **Survivorship caveat:** the round-2+ means average only the few runs that got there (R2 n=10000 tickets vs R1 n=10000). The MEDIAN skilled run dies at round 2, so it never reaches the +EV band — the arc is real for the surviving tail, not the typical player.
- Naive mean EV never crosses zero — as intended.

> Takeaway: among survivors the +EV relics do flip the arc (crosses ~R2, target ≈R4), but almost nobody survives to bank it — the mechanic works, the economy gates it away.

## 6. Band-3 audit

Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).

| Strategy | p99 final bank | top-1% mean | max |
|---|---|---|---|
| naive | $510 | $632 | $1,051 |
| skilled | $3,013 | $3,757 | $6,702 |
| noshop | $286 | $334 | $525 |
| martyr | $1,040 | $10,558 | $165,967 |


Pairwise relic synergy (10,000 runs/config, baseline won 14.2%). Synergy excess = pair Δwon − (soloA Δ + soloB Δ). Top 10:

| Pair | pair won % | synergy excess (pp) | tag |
|---|---|---|---|
| Scar Tissue + Totem of Undying | 32.1% | -4.51 | marginal (no real loop) |
| The Multiplier + Scar Tissue | 22.1% | -9.04 | marginal (no real loop) |
| The Multiplier + Totem of Undying | 22.6% | -11.56 | marginal (no real loop) |

> Takeaway: brokenness should live in the tail (max ≫ p99) and, if combos ran, in a few hard-to-assemble pairs rather than trivial cheap ones.

## 7. Grind metric

Median decisions per round (tickets + cash-outs + buys). Fewer decisions late than mid = flat repetition.

| Round | naive | skilled | noshop | martyr |
|---|---|---|---|---|
| R1 | 1 | 5 | 1 | 3 |
| R2 | 1 | 2 | 1 | 2 |
| R3 | 1 | 2 | 1 | 0 |
| R4 | 1 | 1 | 1 | 2 |
| R5 | 1 | 1 | 1 | 0 |
| R6 | 1 | 1 | 1 | 0 |
| R7 | 1 | 1 | — | 0 |
| R8 | 1 | 1 | — | 0 |

> Takeaway: late rounds carry FEWER decisions than mid for: skilled, martyr — repetition-risk flag.

