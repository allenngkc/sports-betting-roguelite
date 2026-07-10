# /sim — Monte Carlo balance report

- Date: 2026-07-09 23:28
- Engine: workspace is not a git repo — `git describe` unavailable
- Config: bank $500, targets [400, 460, 520, 650, 800, 1000, 1500, 2800] (avg ×1.32), overround 5.0%, cash-out margin 8.0%, debt juice 50.0%, min stake $10, max stake 100.0% of bank, 6 matchups/round, 3 tickets/round, 5 relic slots
- Strategies: naive, random, skilled
- Runs per batch: 10,000
- Total runs (incl. audit/combos): 110,000
- Wall time: 2.93 s

## 1. Survival curves

Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).

| Metric | naive | random | skilled |
|---|---|---|---|
| enter R1 | 100.0% | 100.0% | 100.0% |
| enter R2 | 100.0% | 100.0% | 100.0% |
| enter R3 | 54.7% | 53.5% | 100.0% |
| enter R4 | 44.3% | 23.1% | 96.0% |
| enter R5 | 36.0% | 12.5% | 81.8% |
| enter R6 | 18.8% | 7.0% | 67.1% |
| enter R7 | 9.5% | 4.7% | 54.1% |
| enter R8 | 4.1% | 3.1% | 42.4% |
| **won %** | **0.0%** | **1.1%** | **11.5%** |
| **median death round** | **3** | **3** | **7** |
| mean rounds reached | 3.67 | 3.05 | 6.53 |
| mean floats per run | 1.12 | 1.03 | 1.01 |
| in-debt deaths (% of deaths) | 99.3% | 99.1% | 82.1% |

`S3 (naive median death 3–4): PASS (actual 3)`
`S4 (skilled median death ≥7): PASS (actual 7)`

> Takeaway: naive dies at round 3, skilled reaches 7 — skill buys real extra survival; compare against the 3–4 / ≥7 targets above.

## 2. Relic power audit

Skilled baseline: median death 7, mean rounds 6.53, won 11.5%. Each row is skilled with that one relic granted free at run start. Rows are sorted by **Δ mean rounds survived** (the steadiest signal); Δ won % is the run-winning read.

| Relic | mean rounds | Δ mean | median death | won % | Δ won % | flag |
|---|---|---|---|---|---|---|
| Bankroll Insurance | 6.85 | +0.32 | 8 | 15.8% | +4.3pp | DOMINANT |
| Early Payout | 6.76 | +0.23 | 7 | 18.0% | +6.5pp |  |
| Lucky Charm | 6.75 | +0.22 | 7 | 14.2% | +2.7pp |  |
| High Roller | 6.72 | +0.2 | 7 | 19.2% | +7.7pp |  |
| Promo Code | 6.69 | +0.16 | 7 | 16.2% | +4.7pp |  |
| Piggy Bank | 6.64 | +0.11 | 7 | 13.3% | +1.7pp |  |
| Mulligan | 6.53 | 0 | 7 | 11.6% | +0.1pp | DEAD (~0 effect) |
| Boosted Odds | 6.44 | -0.09 | 7 | 19.1% | +7.6pp | hurts (bot mis-uses it) |

> Takeaway: strongest relic is Bankroll Insurance (+0.32 mean rounds); 1 look dead, 1 dominant. Note the audit grants relics FREE — organic play also pays the shop price out of target headroom.

## 3. Variance feel

Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.

| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |
|---|---|---|---|---|---|---|---|---|
| naive | $125 | $214 | $385 | $738 | $2,911 | $2,913 | $2,914 | $2,914 |
| random | $128 | $219 | $629 | $3,083 | $3,174 | $5,380 | $16,919 | $53,582 |
| skilled | $427 | $1,000 | $2,858 | $4,548 | $3,023 | $3,023 | $3,024 | $3,185 |

> Takeaway: swing p50 vs p99 shows whether the ride is flat (boring) or spiky (feels rigged); a p99 many multiples of p50 is the intended rare-blowup shape.

## 4. Ratchet tuning

_N/A in v0 — there is no limiting/ratchet mechanic in the prototype (parked to a later phase)._

## 5. EV-arc

Mean *true* per-ticket EV at lock, by round (the economy doctrine's crossing from −vig toward positive).

| Round | naive mean EV | random mean EV | skilled mean EV |
|---|---|---|---|
| R1 | -$12 | -$14 | -$2 |
| R2 | -$12 | -$10 | -$4 |
| R3 | -$14 | -$10 | -$9 |
| R4 | -$15 | -$12 | -$24 |
| R5 | -$17 | -$13 | -$36 |
| R6 | -$21 | -$11 | -$47 |
| R7 | -$26 | -$3 | -$67 |
| R8 | -$36 | $16 | -$148 |

- Skilled mean EV never crosses zero (target ≈ round 4).
- Naive mean EV never crosses zero — as intended.

> Takeaway: skilled never turns +EV; naive stays underwater — relics aren't flipping the arc.

## 6. Band-3 audit

Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).

| Strategy | p99 final bank | top-1% mean | max |
|---|---|---|---|
| naive | $1,828 | $2,015 | $2,914 |
| random | $3,190 | $8,968 | $82,274 |
| skilled | $4,232 | $4,494 | $4,900 |

_Combo scan not run — pass `--combos N` (e.g. 2000) for the pairwise synergy table._

> Takeaway: brokenness should live in the tail (max ≫ p99) and, if combos ran, in a few hard-to-assemble pairs rather than trivial cheap ones.

## 7. Grind metric

Median decisions per round (tickets + cash-outs + buys). Fewer decisions late than mid = flat repetition.

| Round | naive | random | skilled |
|---|---|---|---|
| R1 | 1 | 3 | 1 |
| R2 | 1 | 3 | 1 |
| R3 | 1 | 3 | 1 |
| R4 | 1 | 3 | 1 |
| R5 | 1 | 3 | 1 |
| R6 | 1 | 3 | 1 |
| R7 | 1 | 3 | 1 |
| R8 | 1 | 3 | 1 |

> Takeaway: decision count holds up or rises into late rounds — no flat-repetition flag.

