# /sim — Monte Carlo balance report

- Date: 2026-07-23 21:57
- Engine: workspace is not a git repo — `git describe` unavailable
- Config: bank $350, PAYMENTS [60, 70, 85, 105, 155, 375, 710, 1350] (avg ×1.56), overround 5.0%, cash-out margin 8.0%, totem juice 50.0%, min stake $10, max stake 100.0% of bank, 6 matchups/round, 3 tickets/round, 5 relic + 3 consumable slots
- Strategies: naive, random, skilled, noshop, martyr, chalk, hoarder, ironhands, martyr-worst
- Runs per batch: 5,000
- Total runs (incl. audit/combos): 760,000
- Wall time: 534.24 s

## 0. Gate campaign (PLAN.md acceptance criteria)

| Gate | Criterion | Verdict | Actual |
|---|---|---|---|
| G1 | honest gambling: naive win <1%, dies before the cliff resolves (median ≤6) | **PASS** | median 4, won 0.0% |
| G2 | engine mandatory: no-shop skilled win <2%, median death 5–6 | **PASS** | median 5, won 0.0% |
| G3 | skilled + items wins: median death ≥5, win 5–8% (re-banded by Allen 2026-07-15 — the dealt hand's build variance is the roguelite shape) | **PASS** | median 6, won 5.6% |
| G4 | the EV arc exists: skilled mean ticket EV crosses zero in rounds 3–7 | **PASS** | crosses at R3 |
| G5 | composition superadditive: Multiplier+Scar pair Δwin > sum of solo Δwins | **PASS** | synergy excess +0.1pp |
| G6 | martyr guard (worst case granted): loss-farming win ≤ skilled +2pp | **PASS** | martyr-worst 6.0% vs skilled 5.6% (organic martyr 1.8%) |

Item flags: none — no DEAD items, no DOMINANT item, Totem in the healthy band.
- ℹ PLAYTEST-GATED: Ask for the Manager audits ≈0 through bots (Δwon +0.2±1.4pp) — RATIFIED KEEP, playtest #9

> **ALL GATES PASS — the economy holds.**

## 1. Survival curves

Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).

| Metric | naive | random | skilled | noshop | martyr | chalk | hoarder | ironhands | martyr-worst |
|---|---|---|---|---|---|---|---|---|---|
| enter R1 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R2 | 100.0% | 99.9% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R3 | 100.0% | 81.7% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R4 | 86.8% | 46.2% | 100.0% | 100.0% | 40.4% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R5 | 44.1% | 24.0% | 81.4% | 70.1% | 26.9% | 79.5% | 73.8% | 82.5% | 46.3% |
| enter R6 | 10.6% | 11.2% | 53.2% | 17.8% | 15.1% | 46.4% | 31.6% | 55.5% | 34.8% |
| enter R7 | 0.2% | 4.8% | 28.0% | 0.2% | 7.2% | 21.9% | 8.0% | 31.8% | 19.9% |
| enter R8 | 0.0% | 1.9% | 14.0% | 0.0% | 3.2% | 11.9% | 2.2% | 18.4% | 10.9% |
| **won %** | **0.0%** | **0.8%** | **5.6%** | **0.0%** | **1.8%** | **6.7%** | **0.8%** | **8.9%** | **6.0%** |
| **median death round** | **4** | **3** | **6** | **5** | **3** | **5** | **5** | **6** | **4** |
| mean rounds reached | 4.42 | 3.71 | 5.82 | 4.88 | 3.95 | 5.66 | 5.16 | 5.97 | 5.18 |
| totem fire rate | 0.0% | 5.1% | 39.4% | 0.0% | 0.0% | 28.7% | 13.8% | 30.1% | 0.0% |
| close-call deaths (% of deaths) | 13.8% | 14.3% | 5.4% | 1.8% | 4.3% | 6.3% | 6.0% | 5.8% | 6.0% |
| mean bookie gifts per run | 0.46 | 0.50 | 0.30 | 0.30 | 0.87 | 0.17 | 0.27 | 0.25 | 1.03 |

> Takeaway: naive dies at round 4, skilled reaches 6 — skill buys real extra survival; compare against the 3–4 / ≥7 targets above.

## 2. Market exposure

Placed legs and equal-split stake share by market kind. `mean leg EV` is the UNWEIGHTED per-leg return (converges to −vig under fair pricing — the sanity column); `stake-wtd EV` is stake-weighted and fat-tailed under compounding banks (a few monster tickets dominate it — read it as variance, not edge). Both are single-leg, before parlay multiplication, cash-outs, voids, or relic factors.

| Strategy | market | legs placed | stake share | mean leg EV | stake-wtd EV |
|---|---|---|---|---|---|
| naive | Moneyline | 43,778 | 100.0% | -4.6pp | -4.8pp |
| naive | Total Goals | 0 | 0.0% | — | — |
| naive | BTTS | 0 | 0.0% | — | — |
| naive | Total Corners | 0 | 0.0% | — | — |
| naive | Total Cards | 0 | 0.0% | — | — |
| naive | Anytime Scorer | 0 | 0.0% | — | — |
| random | Moneyline | 7,913 | 1.1% | -3.8pp | +2.9pp |
| random | Total Goals | 23,721 | 3.6% | -2.5pp | -5.6pp |
| random | BTTS | 7,896 | 1.1% | -4.0pp | -2.9pp |
| random | Total Corners | 23,986 | 47.8% | -1.9pp | +104.3pp |
| random | Total Cards | 24,202 | 46.3% | -3.7pp | +88.2pp |
| random | Anytime Scorer | 0 | 0.0% | — | — |
| skilled | Moneyline | 47,960 | 77.4% | +5.6pp | +15.0pp |
| skilled | Total Goals | 11,251 | 22.6% | +21.4pp | +16.9pp |
| skilled | BTTS | 0 | 0.0% | — | — |
| skilled | Total Corners | 0 | 0.0% | — | — |
| skilled | Total Cards | 0 | 0.0% | — | — |
| skilled | Anytime Scorer | 0 | 0.0% | — | — |
| noshop | Moneyline | 46,831 | 100.0% | -4.2pp | -4.2pp |
| noshop | Total Goals | 0 | 0.0% | — | — |
| noshop | BTTS | 0 | 0.0% | — | — |
| noshop | Total Corners | 0 | 0.0% | — | — |
| noshop | Total Cards | 0 | 0.0% | — | — |
| noshop | Anytime Scorer | 0 | 0.0% | — | — |
| martyr | Moneyline | 57,840 | 100.0% | -4.6pp | -6.4pp |
| martyr | Total Goals | 0 | 0.0% | — | — |
| martyr | BTTS | 0 | 0.0% | — | — |
| martyr | Total Corners | 0 | 0.0% | — | — |
| martyr | Total Cards | 0 | 0.0% | — | — |
| martyr | Anytime Scorer | 0 | 0.0% | — | — |
| chalk | Moneyline | 59,554 | 100.0% | +3.9pp | +5.7pp |
| chalk | Total Goals | 0 | 0.0% | — | — |
| chalk | BTTS | 0 | 0.0% | — | — |
| chalk | Total Corners | 0 | 0.0% | — | — |
| chalk | Total Cards | 0 | 0.0% | — | — |
| chalk | Anytime Scorer | 0 | 0.0% | — | — |
| hoarder | Moneyline | 50,660 | 100.0% | +1.9pp | +0.6pp |
| hoarder | Total Goals | 0 | 0.0% | — | — |
| hoarder | BTTS | 0 | 0.0% | — | — |
| hoarder | Total Corners | 0 | 0.0% | — | — |
| hoarder | Total Cards | 0 | 0.0% | — | — |
| hoarder | Anytime Scorer | 0 | 0.0% | — | — |
| ironhands | Moneyline | 61,051 | 100.0% | +9.4pp | +12.5pp |
| ironhands | Total Goals | 0 | 0.0% | — | — |
| ironhands | BTTS | 0 | 0.0% | — | — |
| ironhands | Total Corners | 0 | 0.0% | — | — |
| ironhands | Total Cards | 0 | 0.0% | — | — |
| ironhands | Anytime Scorer | 0 | 0.0% | — | — |
| martyr-worst | Moneyline | 75,167 | 100.0% | -4.6pp | -5.9pp |
| martyr-worst | Total Goals | 0 | 0.0% | — | — |
| martyr-worst | BTTS | 0 | 0.0% | — | — |
| martyr-worst | Total Corners | 0 | 0.0% | — | — |
| martyr-worst | Total Cards | 0 | 0.0% | — | — |
| martyr-worst | Anytime Scorer | 0 | 0.0% | — | — |

> Anytime Scorer is intentionally zero: it is a declared human-agency market, excluded from every bot.

## 2. Item power audit (3 passives + 3 consumables)

Skilled baseline: median death 6, mean rounds 5.82, won 5.6%. Passives granted at run start; consumables refilled every round. Exposure = uses (consumables) or wound-up runs (ratchets); ±  is the paired-seed SE on Δwon. Sorted by Δ mean rounds survived.

| Item | kind | mean rounds | Δ mean | median death | won % | Δ won % (±SE) | exposure | totem fires |
|---|---|---|---|---|---|---|---|---|
| Mulligan Slip | consumable | 6.45 | +0.63 | 6 | 14.1% | +8.6pp (±0.4) | 14,818 uses | — |
| Totem of Undying | passive | 6.40 | +0.58 | 6 | 6.4% | +0.8pp (±0.4) | — | 91.4% |
| Free Bet Token | consumable | 6.37 | +0.55 | 6 | 3.6% | -2.0pp (±0.4) | 28,768 uses | — |
| Profit Boost | consumable | 6.25 | +0.42 | 6 | 11.2% | +5.6pp (±0.4) | 28,818 uses | — |
| House Key | passive | 6.18 | +0.36 | 6 | 13.2% | +7.6pp (±0.6) | — | — |
| Unopened Bobblehead | passive | 6.06 | +0.24 | 6 | 10.1% | +4.5pp (±0.5) | — | — |
| Bad Beat Jar | passive | 6.06 | +0.23 | 6 | 9.2% | +3.6pp (±0.5) | 3,353 wound | — |
| The Collection | passive | 6.05 | +0.23 | 6 | 10.0% | +4.4pp (±0.5) | — | — |
| The System | passive | 6.05 | +0.23 | 6 | 10.9% | +5.3pp (±0.5) | 1,285 wound | — |
| Iron Hands | passive | 6.02 | +0.2 | 6 | 9.4% | +3.8pp (±0.5) | 1,779 wound | — |
| Ref's Whistle | consumable | 6.02 | +0.2 | 6 | 6.7% | +1.1pp (±0.3) | 4,105 uses | — |
| Bookie's Marker | consumable | 6.00 | +0.17 | 6 | 5.1% | -0.5pp (±0.3) | 10,956 uses | — |
| Chalk Eater | passive | 5.99 | +0.17 | 6 | 9.8% | +4.2pp (±0.5) | 3,968 wound | — |
| Double or Nothing Slip | consumable | 5.95 | +0.13 | 6 | 9.6% | +4.0pp (±0.3) | 8,240 uses | — |
| The Rake's Rebate | passive | 5.92 | +0.1 | 6 | 7.2% | +1.6pp (±0.5) | — | — |
| Scar Tissue | passive | 5.91 | +0.09 | 6 | 6.2% | +0.6pp (±0.4) | 2,736 wound | — |
| Longshot Larry's Photo | passive | 5.83 | +0.01 | 6 | 5.0% | -0.6pp (±0.4) | — | — |
| Comp'd Suite | passive | 5.83 | +0.01 | 6 | 5.7% | +0.1pp (±0.4) | — | — |
| Ask for the Manager | consumable | 5.81 | -0.01 | 6 | 5.8% | +0.2pp (±0.4) | 23,784 uses | — |
| The Multiplier | passive | 5.77 | -0.05 | 5 | 17.2% | +11.6pp (±0.6) | — | — |
| Golden Parachute | passive | 5.68 | -0.14 | 6 | 3.0% | -2.6pp (±0.4) | — | — |
| Whale Card | passive | 5.26 | -0.56 | 5 | 1.1% | -4.5pp (±0.3) | — | — |

> Takeaway: the gate section's item flags carry the verdicts (DEAD / DOMINANT / totem band). Note the audit grants items FREE — organic play also pays shop prices out of payment headroom.

## 3. Variance feel

Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.

| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |
|---|---|---|---|---|---|---|---|---|
| naive | $87 | $106 | $184 | $223 | n/a | n/a | n/a | n/a |
| random | $87 | $148 | $497 | $3,367 | $338 | $5,743 | $267,550 | $655,989,697,538 |
| skilled | $82 | $114 | $789 | $11,153 | $123 | $1,891 | $31,192 | $994,396 |
| noshop | $78 | $112 | $137 | $165 | n/a | n/a | n/a | n/a |
| martyr | $87 | $87 | $862 | $5,362 | $1 | $2,126 | $13,766 | $44,815 |
| chalk | $82 | $112 | $697 | $7,467 | $188 | $3,839 | $16,268 | $89,530 |
| hoarder | $79 | $112 | $173 | $1,003 | $148 | $3,339 | $11,314 | $14,631 |
| ironhands | $83 | $157 | $1,281 | $17,181 | $102 | $2,326 | $25,259 | $132,237 |
| martyr-worst | $87 | $87 | $1,567 | $16,218 | $414 | $3,230 | $22,371 | $76,603 |

> Takeaway: swing p50 vs p99 shows whether the ride is flat (boring) or spiky (feels rigged); a p99 many multiples of p50 is the intended rare-blowup shape.

## 4. Ratchet telemetry (Scar Tissue)

Mean peak stacks (pp) and mean carrier burns per run — is the ratchet winding and cashing?

| Strategy | mean peak stacks | mean burns/run |
|---|---|---|
| naive | 0.0pp | 0.00 |
| random | 0.4pp | 0.00 |
| skilled | 2.7pp | 0.00 |
| noshop | 0.0pp | 0.00 |
| martyr | 6.6pp | 0.00 |
| chalk | 0.4pp | 0.00 |
| hoarder | 0.1pp | 0.00 |
| ironhands | 0.7pp | 0.00 |
| martyr-worst | 28.3pp | 0.00 |

> Takeaway: zero across the board means nobody buys/holds the scar (price or power problem); huge stacks with zero burns means winding without cashing (the carrier rule isn't landing).

## 5. EV-arc

Mean *true* per-ticket EV at lock, by round (the economy doctrine's crossing from −vig toward positive).

| Round | naive mean EV | random mean EV | skilled mean EV | noshop mean EV | martyr mean EV | chalk mean EV | hoarder mean EV | ironhands mean EV | martyr-worst mean EV |
|---|---|---|---|---|---|---|---|---|---|
| R1 | -$8 | -$10 | -$5 | -$5 | -$7 | -$5 | -$5 | -$5 | $32 |
| R2 | -$6 | -$7 | $3 | -$4 | $8 | $3 | -$4 | $25 | $37 |
| R3 | -$5 | -$6 | $24 | -$3 | $17 | $21 | $2 | $31 | $58 |
| R4 | -$3 | -$7 | $25 | -$3 | $26 | $31 | $6 | $31 | $86 |
| R5 | -$3 | -$12 | $37 | -$4 | $49 | $50 | $5 | $50 | $138 |
| R6 | -$3 | -$16 | $62 | -$4 | $105 | $91 | $17 | $88 | $273 |
| R7 | -$2 | -$102 | $234 | -$4 | $182 | $180 | $67 | $182 | $506 |
| R8 | — | -$10,312 | $626 | — | $276 | $312 | $143 | $388 | $959 |

- Skilled mean EV first crosses zero at **round 3** (target ≈ round 4).
- **Survivorship caveat:** the round-3+ means average only the few runs that got there (R3 n=4692 tickets vs R1 n=5000). The MEDIAN skilled run dies at round 2, so it never reaches the +EV band — the arc is real for the surviving tail, not the typical player.
- Naive mean EV never crosses zero — as intended.

> Takeaway: among survivors the +EV relics do flip the arc (crosses ~R3, target ≈R4), but almost nobody survives to bank it — the mechanic works, the economy gates it away.

## 6. Band-3 audit

Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).

| Strategy | p99 final bank | top-1% mean | max |
|---|---|---|---|
| naive | $256 | $313 | $363 |
| random | $599 | $21,161,001,578 | $1,058,046,957,893 |
| skilled | $13,627 | $224,129 | $2,597,937 |
| noshop | $130 | $193 | $355 |
| martyr | $1,516 | $10,218 | $80,111 |
| chalk | $13,313 | $32,993 | $198,565 |
| hoarder | $651 | $3,591 | $15,122 |
| ironhands | $23,284 | $72,124 | $734,964 |
| martyr-worst | $15,113 | $33,647 | $102,438 |


Pairwise relic synergy (5,000 runs/config, baseline won 0.0%). Synergy excess = pair Δwon − (soloA Δ + soloB Δ). Top 10:

| Pair | pair won % | synergy excess (pp) | tag |
|---|---|---|---|
| Longshot Larry's Photo + House Key | 4.7% | +3 | degenerate: cheap pair, trivially assembled |
| The Multiplier + House Key | 3.8% | +2.9 | degenerate: cheap pair, trivially assembled |
| Longshot Larry's Photo + Whale Card | 4.1% | +2.38 | degenerate: cheap pair, trivially assembled |
| The Multiplier + Whale Card | 3.0% | +2.12 | degenerate: cheap pair, trivially assembled |
| The Multiplier + Longshot Larry's Photo | 3.4% | +0.88 | marginal (no real loop) |
| Longshot Larry's Photo + The System | 2.5% | +0.86 | marginal (no real loop) |
| The Multiplier + The System | 1.7% | +0.86 | marginal (no real loop) |
| The Multiplier + Chalk Eater | 1.7% | +0.82 | marginal (no real loop) |
| Longshot Larry's Photo + Bad Beat Jar | 2.3% | +0.66 | marginal (no real loop) |
| The Multiplier + Iron Hands | 1.5% | +0.64 | marginal (no real loop) |

> Takeaway: brokenness should live in the tail (max ≫ p99) and, if combos ran, in a few hard-to-assemble pairs rather than trivial cheap ones.

## 7. Grind metric

Median decisions per round (tickets + cash-outs + buys). Fewer decisions late than mid = flat repetition.

| Round | naive | random | skilled | noshop | martyr | chalk | hoarder | ironhands | martyr-worst |
|---|---|---|---|---|---|---|---|---|---|
| R1 | 1 | 3 | 3 | 1 | 3 | 3 | 1 | 3 | 3 |
| R2 | 1 | 3 | 4 | 1 | 3 | 4 | 2 | 4 | 3 |
| R3 | 1 | 3 | 4 | 1 | 0 | 3 | 4 | 4 | 3 |
| R4 | 1 | 2 | 3 | 1 | 2 | 3 | 2 | 3 | 1 |
| R5 | 1 | 2 | 2 | 1 | 2 | 2 | 1 | 2 | 3 |
| R6 | 1 | 3 | 2 | 1 | 1 | 2 | 1 | 2 | 1 |
| R7 | 1 | 3 | 2 | 1 | 1 | 2 | 1 | 3 | 2 |
| R8 | — | 3 | 2 | — | 1 | 2 | 2 | 2 | 1 |

> Takeaway: late rounds carry FEWER decisions than mid for: skilled, martyr, chalk, hoarder, ironhands, martyr-worst — repetition-risk flag.

