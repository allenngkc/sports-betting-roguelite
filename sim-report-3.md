# /sim — Monte Carlo balance report

- Date: 2026-07-13 23:53
- Engine: workspace is not a git repo — `git describe` unavailable
- Config: bank $350, PAYMENTS [60, 70, 85, 105, 195, 375, 710, 1350] (avg ×1.56), overround 5.0%, cash-out margin 8.0%, totem juice 50.0%, min stake $10, max stake 100.0% of bank, 6 matchups/round, 3 tickets/round, 5 relic + 3 consumable slots
- Strategies: naive, skilled, noshop, martyr
- Runs per batch: 50,000
- Total runs (incl. audit/combos): 800,000
- Wall time: 46.40 s

## 0. Gate campaign (PLAN.md acceptance criteria)

| Gate | Criterion | Verdict | Actual |
|---|---|---|---|
| G1 | honest gambling: naive win <1%, dies before the cliff resolves (median ≤6) | **PASS** | median 4, won 0.0% |
| G2 | engine mandatory: no-shop skilled win <2%, median death 5–6 | **PASS** | median 5, won 0.0% |
| G3 | skilled + items wins: median death ≥6, win 5–8% (Allen's final-product band) | **PASS** | median 6, won 6.2% |
| G4 | the EV arc exists: skilled mean ticket EV crosses zero in rounds 3–7 | **PASS** | crosses at R3 |
| G5 | composition superadditive: Multiplier+Scar pair Δwin > sum of solo Δwins | **PASS** | synergy excess +0.0pp |
| G6 | martyr guard: scar-farming bot win ≤ skilled +2pp | **PASS** | martyr 0.7% vs skilled 6.2% |

Item flags: none — no DEAD items, no DOMINANT item, Totem in the healthy band.

> **ALL GATES PASS — the economy holds.**

## 1. Survival curves

Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).

| Metric | naive | skilled | noshop | martyr |
|---|---|---|---|---|
| enter R1 | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R2 | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R3 | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R4 | 86.4% | 100.0% | 100.0% | 39.3% |
| enter R5 | 43.1% | 81.4% | 71.1% | 21.4% |
| enter R6 | 7.8% | 59.7% | 14.3% | 10.5% |
| enter R7 | 0.1% | 35.1% | 1.2% | 4.3% |
| enter R8 | 0.0% | 17.4% | 0.0% | 1.8% |
| **won %** | **0.0%** | **6.2%** | **0.0%** | **0.7%** |
| **median death round** | **4** | **6** | **5** | **3** |
| mean rounds reached | 4.37 | 6.00 | 4.87 | 3.78 |
| totem fire rate | 0.0% | 50.3% | 0.0% | 0.0% |
| close-call deaths (% of deaths) | 10.0% | 5.0% | 4.2% | 3.6% |
| mean bookie gifts per run | 0.46 | 0.60 | 0.74 | 0.84 |

> Takeaway: naive dies at round 4, skilled reaches 6 — skill buys real extra survival; compare against the 3–4 / ≥7 targets above.

## 2. Item power audit (3 passives + 3 consumables)

Skilled baseline: median death 6, mean rounds 6.00, won 6.2%. Passives granted at run start; consumables refilled every round. Sorted by Δ mean rounds survived.

| Item | kind | mean rounds | Δ mean | median death | won % | Δ won % | totem fires |
|---|---|---|---|---|---|---|---|
| Mulligan Slip | consumable | 7.42 | +1.42 | 8 | 26.7% | +20.5pp | — |
| The Multiplier | passive | 6.44 | +0.44 | 6 | 18.4% | +12.2pp | — |
| Totem of Undying | passive | 6.42 | +0.43 | 6 | 6.5% | +0.3pp | 89.3% |
| Scar Tissue | passive | 6.35 | +0.35 | 6 | 9.4% | +3.2pp | — |
| Profit Boost | consumable | 6.30 | +0.3 | 6 | 11.2% | +5.0pp | — |

> Takeaway: the gate section's item flags carry the verdicts (DEAD / DOMINANT / totem band). Note the audit grants items FREE — organic play also pays shop prices out of payment headroom.

## 3. Variance feel

Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.

| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |
|---|---|---|---|---|---|---|---|---|
| naive | $87 | $111 | $184 | $221 | n/a | n/a | n/a | n/a |
| skilled | $72 | $155 | $927 | $1,709 | $88 | $623 | $1,775 | $3,075 |
| noshop | $51 | $119 | $201 | $349 | n/a | n/a | n/a | n/a |
| martyr | $87 | $87 | $795 | $3,587 | $0 | $1,215 | $15,069 | $141,229 |

> Takeaway: swing p50 vs p99 shows whether the ride is flat (boring) or spiky (feels rigged); a p99 many multiples of p50 is the intended rare-blowup shape.

## 4. Ratchet telemetry (Scar Tissue)

Mean peak stacks (pp) and mean carrier burns per run — is the ratchet winding and cashing?

| Strategy | mean peak stacks | mean burns/run |
|---|---|---|
| naive | 0.0pp | 0.00 |
| skilled | 4.4pp | 0.00 |
| noshop | 0.0pp | 0.00 |
| martyr | 14.1pp | 0.00 |

> Takeaway: zero across the board means nobody buys/holds the scar (price or power problem); huge stacks with zero burns means winding without cashing (the carrier rule isn't landing).

## 5. EV-arc

Mean *true* per-ticket EV at lock, by round (the economy doctrine's crossing from −vig toward positive).

| Round | naive mean EV | skilled mean EV | noshop mean EV | martyr mean EV |
|---|---|---|---|---|
| R1 | -$8 | -$5 | -$5 | -$7 |
| R2 | -$6 | -$2 | -$6 | -$6 |
| R3 | -$5 | $22 | -$4 | -$6 |
| R4 | -$3 | $31 | -$4 | -$8 |
| R5 | -$3 | $41 | -$7 | -$12 |
| R6 | -$3 | $58 | -$6 | -$23 |
| R7 | -$1 | $73 | -$7 | -$31 |
| R8 | — | $94 | -$6 | -$91 |

- Skilled mean EV first crosses zero at **round 3** (target ≈ round 4).
- **Survivorship caveat:** the round-3+ means average only the few runs that got there (R3 n=50000 tickets vs R1 n=50000). The MEDIAN skilled run dies at round 2, so it never reaches the +EV band — the arc is real for the surviving tail, not the typical player.
- Naive mean EV never crosses zero — as intended.

> Takeaway: among survivors the +EV relics do flip the arc (crosses ~R3, target ≈R4), but almost nobody survives to bank it — the mechanic works, the economy gates it away.

## 6. Band-3 audit

Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).

| Strategy | p99 final bank | top-1% mean | max |
|---|---|---|---|
| naive | $228 | $281 | $375 |
| skilled | $1,688 | $2,180 | $6,356 |
| noshop | $177 | $216 | $697 |
| martyr | $527 | $6,522 | $345,554 |


Pairwise relic synergy (50,000 runs/config, baseline won 0.0%). Synergy excess = pair Δwon − (soloA Δ + soloB Δ). Top 10:

| Pair | pair won % | synergy excess (pp) | tag |
|---|---|---|---|
| The Multiplier + Totem of Undying | 0.7% | +0.05 | marginal (no real loop) |
| The Multiplier + Scar Tissue | 0.7% | +0.03 | marginal (no real loop) |
| Scar Tissue + Totem of Undying | 0.0% | 0 | marginal (no real loop) |

> Takeaway: brokenness should live in the tail (max ≫ p99) and, if combos ran, in a few hard-to-assemble pairs rather than trivial cheap ones.

## 7. Grind metric

Median decisions per round (tickets + cash-outs + buys). Fewer decisions late than mid = flat repetition.

| Round | naive | skilled | noshop | martyr |
|---|---|---|---|---|
| R1 | 1 | 2 | 1 | 3 |
| R2 | 1 | 2 | 1 | 2 |
| R3 | 1 | 3 | 1 | 0 |
| R4 | 1 | 2 | 1 | 1 |
| R5 | 1 | 2 | 1 | 0 |
| R6 | 1 | 2 | 1 | 0 |
| R7 | 1 | 2 | 1 | 0 |
| R8 | — | 1 | 1 | 0 |

> Takeaway: late rounds carry FEWER decisions than mid for: skilled, martyr — repetition-risk flag.

