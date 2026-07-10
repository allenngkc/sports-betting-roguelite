# /sim — Monte Carlo balance report

- Date: 2026-07-09 22:07
- Engine: workspace is not a git repo — `git describe` unavailable
- Config: bank $500, targets [800, 1200, 1900, 3000, 4800, 7800, 12500, 20000] (~×1.6), overround 5.0%, cash-out margin 8.0%, min stake $10, max stake 100.0% of bank, 6 matchups/round, 3 tickets/round, 5 relic slots
- Strategies: naive, random, skilled
- Runs per batch: 2,000
- Total runs (incl. audit/combos): 26,000
- Wall time: 0.39 s

## 1. Survival curves

Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).

| Metric | naive | random | skilled |
|---|---|---|---|
| enter R1 | 100.0% | 100.0% | 100.0% |
| enter R2 | 0.0% | 7.5% | 50.4% |
| enter R3 | 0.0% | 1.8% | 15.9% |
| enter R4 | 0.0% | 0.5% | 4.8% |
| enter R5 | 0.0% | 0.3% | 1.5% |
| enter R6 | 0.0% | 0.1% | 1.0% |
| enter R7 | 0.0% | 0.1% | 0.6% |
| enter R8 | 0.0% | 0.0% | 0.4% |
| **won %** | **0.0%** | **0.0%** | **0.1%** |
| **median death round** | **1** | **1** | **2** |
| mean rounds reached | 1.00 | 1.10 | 1.75 |

`S3 (naive median death 3–4): FAIL (actual 1)`
`S4 (skilled median death ≥7): FAIL (actual 2)`

> Takeaway: naive dies at round 1, skilled reaches 2 — skill buys real extra survival; compare against the 3–4 / ≥7 targets above.

## 2. Relic power audit

Skilled baseline: median death 2, mean rounds 1.75, won 0.1%. Each row is skilled with that one relic granted free at run start. Because win%/median saturate at the §8 floor, rows are sorted by **Δ mean rounds survived** — the only column with resolving power here.

| Relic | mean rounds | Δ mean | median death | won % | Δ won % | flag |
|---|---|---|---|---|---|---|
| Early Payout | 1.83 | +0.08 | 2 | 0.6% | +0.4pp |  |
| Lucky Charm | 1.82 | +0.07 | 2 | 0.4% | +0.2pp |  |
| Promo Code | 1.80 | +0.06 | 2 | 0.8% | +0.6pp |  |
| High Roller | 1.79 | +0.04 | 2 | 0.3% | +0.2pp |  |
| Boosted Odds | 1.78 | +0.04 | 2 | 0.3% | +0.2pp |  |
| Mulligan | 1.76 | +0.01 | 2 | 0.3% | +0.1pp | DEAD (~0 effect) |
| Piggy Bank | 1.75 | +0.01 | 2 | 0.3% | +0.2pp | DEAD (~0 effect) |
| Bankroll Insurance | 1.75 | 0 | 2 | 0.1% | 0.0pp | DEAD (~0 effect) |
| Tout Sheet | 1.74 | 0 | 2 | 0.1% | 0.0pp | DEAD (~0 effect) |
| Sharp Eye | 1.74 | -0.01 | 2 | 0.1% | 0.0pp | DEAD (~0 effect) |

> Takeaway: strongest relic is Early Payout (+0.08 mean rounds); 5 look dead, 0 dominant — but every relic is measured against a floored economy, so re-audit once targets let runs breathe.

## 3. Variance feel

Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.

| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |
|---|---|---|---|---|---|---|---|---|
| naive | $125 | $125 | $248 | $293 | n/a | n/a | n/a | n/a |
| random | $86 | $180 | $424 | $1,352 | n/a | n/a | n/a | n/a |
| skilled | $409 | $616 | $1,133 | $3,283 | $23,127 | $24,840 | $25,364 | $25,482 |

> Takeaway: swing p50 vs p99 shows whether the ride is flat (boring) or spiky (feels rigged); a p99 many multiples of p50 is the intended rare-blowup shape.

## 4. Ratchet tuning

_N/A in v0 — there is no limiting/ratchet mechanic in the prototype (parked to a later phase)._

## 5. EV-arc

Mean *true* per-ticket EV at lock, by round (the economy doctrine's crossing from −vig toward positive).

| Round | naive mean EV | random mean EV | skilled mean EV |
|---|---|---|---|
| R1 | -$12 | -$14 | -$27 |
| R2 | — | -$28 | -$26 |
| R3 | — | -$41 | $10 |
| R4 | — | -$89 | $140 |
| R5 | — | -$97 | $265 |
| R6 | — | -$99 | $407 |
| R7 | — | -$218 | $705 |
| R8 | — | — | $815 |

- Skilled mean EV first crosses zero at **round 3** (target ≈ round 4).
- **Survivorship caveat:** the round-3+ means average only the few runs that got there (R3 n=430 tickets vs R1 n=2000). The MEDIAN skilled run dies at round 2, so it never reaches the +EV band — the arc is real for the surviving tail, not the typical player.
- Naive mean EV never crosses zero — as intended.

> Takeaway: among survivors the +EV relics do flip the arc (crosses ~R3, target ≈R4), but almost nobody survives to bank it — the mechanic works, the economy gates it away.

## 6. Band-3 audit

Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).

| Strategy | p99 final bank | top-1% mean | max |
|---|---|---|---|
| naive | $668 | $686 | $733 |
| random | $1,253 | $2,574 | $9,747 |
| skilled | $1,860 | $7,085 | $25,495 |

_Combo scan not run — pass `--combos N` (e.g. 2000) for the pairwise synergy table._

> Takeaway: brokenness should live in the tail (max ≫ p99) and, if combos ran, in a few hard-to-assemble pairs rather than trivial cheap ones.

## 7. Grind metric

Median decisions per round (tickets + sharp-eye + cash-outs + buys). Fewer decisions late than mid = flat repetition.

| Round | naive | random | skilled |
|---|---|---|---|
| R1 | 1 | 3 | 2 |
| R2 | — | 3 | 2 |
| R3 | — | 2.5 | 2 |
| R4 | — | 4 | 3 |
| R5 | — | 3 | 3 |
| R6 | — | 3 | 3 |
| R7 | — | 3 | 2 |
| R8 | — | — | 2 |

> Takeaway: late rounds carry FEWER decisions than mid for: random, skilled — repetition-risk flag.

