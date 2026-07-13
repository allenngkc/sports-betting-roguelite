# /sim — Monte Carlo balance report

- Date: 2026-07-12 23:42
- Engine: workspace is not a git repo — `git describe` unavailable
- Config: bank $350, PAYMENTS [60, 70, 85, 105, 195, 375, 710, 1350] (avg ×1.56), overround 5.0%, cash-out margin 8.0%, totem juice 50.0%, min stake $10, max stake 100.0% of bank, 6 matchups/round, 3 tickets/round, 5 relic + 2 consumable slots
- Strategies: naive, skilled, noshop, martyr
- Runs per batch: 10,000
- Total runs (incl. audit/combos): 170,000
- Wall time: 11.76 s

## 0. Gate campaign (PLAN.md acceptance criteria)

| Gate | Criterion | Verdict | Actual |
|---|---|---|---|
| G1 | honest gambling: naive win <1%, dies before the cliff resolves (median ≤6) | **PASS** | median 4, won 0.0% |
| G2 | engine mandatory: no-shop skilled win <2%, median death 5–6 | **PASS** | median 5, won 0.0% |
| G3 | skilled + items wins: median death ≥6, win 5–8% (Allen's final-product band) | **PASS** | median 6, won 6.4% |
| G4 | the EV arc exists: skilled mean ticket EV crosses zero in rounds 3–7 | **PASS** | crosses at R3 |
| G5 | composition superadditive: Multiplier+Scar pair Δwin > sum of solo Δwins | **PASS** | synergy excess +0.0pp |
| G6 | martyr guard: scar-farming bot win ≤ skilled +2pp | **PASS** | martyr 0.7% vs skilled 6.4% |

- ⚑ TOTEM: Δmean +0.44 (want ≥0.3), fire rate 89% (want 25–60%)

> **NOT DONE — iterate the knobs (item numbers / prices / curve) and rerun.**

## 1. Survival curves

Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).

| Metric | naive | skilled | noshop | martyr |
|---|---|---|---|---|
| enter R1 | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R2 | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R3 | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R4 | 86.5% | 100.0% | 100.0% | 39.6% |
| enter R5 | 42.8% | 80.2% | 68.1% | 21.7% |
| enter R6 | 7.4% | 58.5% | 13.6% | 10.0% |
| enter R7 | 0.1% | 32.8% | 1.2% | 4.3% |
| enter R8 | 0.0% | 16.6% | 0.0% | 1.7% |
| **won %** | **0.0%** | **6.4%** | **0.0%** | **0.7%** |
| **median death round** | **4** | **6** | **5** | **3** |
| mean rounds reached | 4.37 | 5.95 | 4.83 | 3.78 |
| totem fire rate | 0.0% | 48.9% | 0.0% | 0.0% |
| close-call deaths (% of deaths) | 10.3% | 4.0% | 4.0% | 3.5% |
| mean bookie gifts per run | 0.46 | 0.47 | 0.74 | 0.84 |

> Takeaway: naive dies at round 4, skilled reaches 6 — skill buys real extra survival; compare against the 3–4 / ≥7 targets above.

## 2. Item power audit (3 passives + 3 consumables)

Skilled baseline: median death 6, mean rounds 5.95, won 6.4%. Passives granted at run start; consumables refilled every round. Timeout is exempt from the DEAD flag — bots never play it (playtest-gated). Sorted by Δ mean rounds survived.

| Item | kind | mean rounds | Δ mean | median death | won % | Δ won % | totem fires |
|---|---|---|---|---|---|---|---|
| Mulligan Slip | consumable | 7.31 | +1.37 | 8 | 23.6% | +17.1pp | — |
| The Multiplier | passive | 6.68 | +0.73 | 6 | 23.0% | +16.6pp | — |
| Totem of Undying | passive | 6.38 | +0.44 | 6 | 7.0% | +0.5pp | 89.0% |
| Scar Tissue | passive | 6.32 | +0.37 | 6 | 9.8% | +3.3pp | — |
| Profit Boost | consumable | 6.22 | +0.28 | 6 | 10.7% | +4.3pp | — |
| Timeout | consumable | 5.89 | -0.06 | 6 | 5.0% | -1.4pp | — |

> Takeaway: the gate section's item flags carry the verdicts (DEAD / DOMINANT / totem band). Note the audit grants items FREE — organic play also pays shop prices out of payment headroom.

## 3. Variance feel

Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.

| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |
|---|---|---|---|---|---|---|---|---|
| naive | $87 | $114 | $183 | $221 | n/a | n/a | n/a | n/a |
| skilled | $70 | $138 | $724 | $1,706 | $108 | $611 | $1,634 | $2,529 |
| noshop | $50 | $119 | $196 | $342 | n/a | n/a | n/a | n/a |
| martyr | $87 | $87 | $800 | $3,587 | $0 | $985 | $11,535 | $74,469 |

> Takeaway: swing p50 vs p99 shows whether the ride is flat (boring) or spiky (feels rigged); a p99 many multiples of p50 is the intended rare-blowup shape.

## 4. Ratchet telemetry (Scar Tissue)

Mean peak stacks (pp) and mean carrier burns per run — is the ratchet winding and cashing?

| Strategy | mean peak stacks | mean burns/run |
|---|---|---|
| naive | 0.0pp | 0.00 |
| skilled | 4.3pp | 0.00 |
| noshop | 0.0pp | 0.00 |
| martyr | 14.2pp | 0.00 |

> Takeaway: zero across the board means nobody buys/holds the scar (price or power problem); huge stacks with zero burns means winding without cashing (the carrier rule isn't landing).

## 5. EV-arc

Mean *true* per-ticket EV at lock, by round (the economy doctrine's crossing from −vig toward positive).

| Round | naive mean EV | skilled mean EV | noshop mean EV | martyr mean EV |
|---|---|---|---|---|
| R1 | -$8 | -$5 | -$5 | -$7 |
| R2 | -$7 | -$2 | -$6 | -$6 |
| R3 | -$5 | $16 | -$5 | -$6 |
| R4 | -$3 | $28 | -$5 | -$7 |
| R5 | -$3 | $40 | -$7 | -$11 |
| R6 | -$3 | $61 | -$6 | -$24 |
| R7 | -$2 | $79 | -$7 | -$35 |
| R8 | — | $89 | -$2 | -$65 |

- Skilled mean EV first crosses zero at **round 3** (target ≈ round 4).
- **Survivorship caveat:** the round-3+ means average only the few runs that got there (R3 n=10000 tickets vs R1 n=10000). The MEDIAN skilled run dies at round 2, so it never reaches the +EV band — the arc is real for the surviving tail, not the typical player.
- Naive mean EV never crosses zero — as intended.

> Takeaway: among survivors the +EV relics do flip the arc (crosses ~R3, target ≈R4), but almost nobody survives to bank it — the mechanic works, the economy gates it away.

## 6. Band-3 audit

Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).

| Strategy | p99 final bank | top-1% mean | max |
|---|---|---|---|
| naive | $220 | $278 | $370 |
| skilled | $1,470 | $1,898 | $3,555 |
| noshop | $172 | $214 | $697 |
| martyr | $472 | $4,244 | $76,460 |


Pairwise relic synergy (10,000 runs/config, baseline won 0.0%). Synergy excess = pair Δwon − (soloA Δ + soloB Δ). Top 10:

| Pair | pair won % | synergy excess (pp) | tag |
|---|---|---|---|
| The Multiplier + Scar Tissue | 0.6% | +0.03 | marginal (no real loop) |
| The Multiplier + Totem of Undying | 0.5% | 0 | marginal (no real loop) |
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
| R6 | 1 | 1 | 1 | 0 |
| R7 | 1 | 0 | 1 | 0 |
| R8 | — | 1 | 1 | 0 |

> Takeaway: late rounds carry FEWER decisions than mid for: skilled, martyr — repetition-risk flag.

