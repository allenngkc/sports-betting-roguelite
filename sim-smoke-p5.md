# /sim — Monte Carlo balance report

- Date: 2026-07-23 00:59
- Engine: workspace is not a git repo — `git describe` unavailable
- Config: bank $350, PAYMENTS [60, 70, 85, 105, 155, 375, 710, 1350] (avg ×1.56), overround 5.0%, cash-out margin 8.0%, totem juice 50.0%, min stake $10, max stake 100.0% of bank, 6 matchups/round, 3 tickets/round, 5 relic + 3 consumable slots
- Strategies: naive, random, skilled, noshop, martyr, chalk, hoarder, ironhands, martyr-worst
- Runs per batch: 1,000
- Total runs (incl. audit/combos): 152,000
- Wall time: 56.95 s

## 0. Gate campaign (PLAN.md acceptance criteria)

| Gate | Criterion | Verdict | Actual |
|---|---|---|---|
| G1 | honest gambling: naive win <1%, dies before the cliff resolves (median ≤6) | **PASS** | median 4, won 0.0% |
| G2 | engine mandatory: no-shop skilled win <2%, median death 5–6 | **PASS** | median 5, won 0.0% |
| G3 | skilled + items wins: median death ≥5, win 5–8% (re-banded by Allen 2026-07-15 — the dealt hand's build variance is the roguelite shape) | **FAIL** | median 5, won 2.1% |
| G4 | the EV arc exists: skilled mean ticket EV crosses zero in rounds 3–7 | **PASS** | crosses at R5 |
| G5 | composition superadditive: Multiplier+Scar pair Δwin > sum of solo Δwins | **FAIL** | synergy excess +0.0pp |
| G6 | martyr guard (worst case granted): loss-farming win ≤ skilled +2pp | **FAIL** | martyr-worst 6.9% vs skilled 2.1% (organic martyr 2.0%) |

- ⚑ UNDEREXPOSED: Iron Hands (70 wound-up runs < 200)
- ⚑ UNDEREXPOSED: The System (92 wound-up runs < 200)
- ⚑ UNDEREXPOSED: Chalk Eater (0 wound-up runs < 200)
- ⚑ TOTEM: Δmean +1.04 (want ≥0.3), organic fire rate 2% (want 25–60%)
- ℹ PLAYTEST-GATED: Ask for the Manager audits ≈0 through bots (Δwon +1.1±1.8pp) — RATIFIED KEEP, playtest #9

> **NOT DONE — iterate the knobs (item numbers / prices / curve) and rerun.**

## 1. Survival curves

Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).

| Metric | naive | random | skilled | noshop | martyr | chalk | hoarder | ironhands | martyr-worst |
|---|---|---|---|---|---|---|---|---|---|
| enter R1 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R2 | 100.0% | 99.8% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R3 | 100.0% | 83.1% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R4 | 87.0% | 47.8% | 100.0% | 99.1% | 43.4% | 99.3% | 99.3% | 99.2% | 100.0% |
| enter R5 | 43.9% | 24.6% | 57.4% | 64.7% | 28.1% | 76.0% | 70.1% | 77.8% | 48.3% |
| enter R6 | 10.5% | 11.3% | 17.1% | 17.4% | 16.0% | 36.0% | 28.9% | 43.4% | 34.6% |
| enter R7 | 0.2% | 6.1% | 6.8% | 0.9% | 7.2% | 10.2% | 6.1% | 17.6% | 20.3% |
| enter R8 | 0.0% | 2.8% | 4.5% | 0.0% | 3.4% | 1.5% | 0.7% | 4.8% | 11.7% |
| **won %** | **0.0%** | **1.1%** | **2.1%** | **0.0%** | **2.0%** | **0.2%** | **0.2%** | **0.9%** | **6.9%** |
| **median death round** | **4** | **3** | **5** | **5** | **3** | **5** | **5** | **5** | **4** |
| mean rounds reached | 4.42 | 3.77 | 4.88 | 4.82 | 4.00 | 5.23 | 5.05 | 5.44 | 5.22 |
| totem fire rate | 0.0% | 5.4% | 1.9% | 0.0% | 0.0% | 23.6% | 11.7% | 30.1% | 0.0% |
| close-call deaths (% of deaths) | 12.2% | 12.6% | 11.6% | 5.8% | 4.9% | 4.6% | 5.9% | 6.2% | 5.2% |
| mean bookie gifts per run | 0.46 | 0.50 | 0.81 | 0.46 | 0.87 | 0.28 | 0.42 | 0.29 | 1.04 |

> Takeaway: naive dies at round 4, skilled reaches 5 — skill buys real extra survival; compare against the 3–4 / ≥7 targets above.

## 2. Market exposure

Placed legs and equal-split stake share by market kind. `mean leg EV` is the UNWEIGHTED per-leg return (converges to −vig under fair pricing — the sanity column); `stake-wtd EV` is stake-weighted and fat-tailed under compounding banks (a few monster tickets dominate it — read it as variance, not edge). Both are single-leg, before parlay multiplication, cash-outs, voids, or relic factors.

| Strategy | market | legs placed | stake share | mean leg EV | stake-wtd EV |
|---|---|---|---|---|---|
| naive | Moneyline | 8,756 | 100.0% | -4.9pp | -5.1pp |
| naive | Total Goals | 0 | 0.0% | — | — |
| naive | BTTS | 0 | 0.0% | — | — |
| naive | Total Corners | 0 | 0.0% | — | — |
| naive | Total Cards | 0 | 0.0% | — | — |
| naive | Anytime Scorer | 0 | 0.0% | — | — |
| random | Moneyline | 1,604 | 0.3% | -1.1pp | 0.0pp |
| random | Total Goals | 4,873 | 1.0% | -3.2pp | -11.2pp |
| random | BTTS | 1,600 | 0.3% | -4.3pp | -3.4pp |
| random | Total Corners | 4,923 | 50.0% | +0.6pp | +111.2pp |
| random | Total Cards | 4,925 | 48.4% | -3.2pp | +94.0pp |
| random | Anytime Scorer | 0 | 0.0% | — | — |
| skilled | Moneyline | 0 | 0.0% | — | — |
| skilled | Total Goals | 157 | 0.9% | +14.9pp | +10.8pp |
| skilled | BTTS | 0 | 0.0% | — | — |
| skilled | Total Corners | 10,040 | 76.7% | +19.7pp | +27.9pp |
| skilled | Total Cards | 3,621 | 22.4% | +14.3pp | +12.0pp |
| skilled | Anytime Scorer | 0 | 0.0% | — | — |
| noshop | Moneyline | 5,684 | 100.0% | -4.3pp | -5.7pp |
| noshop | Total Goals | 0 | 0.0% | — | — |
| noshop | BTTS | 0 | 0.0% | — | — |
| noshop | Total Corners | 0 | 0.0% | — | — |
| noshop | Total Cards | 0 | 0.0% | — | — |
| noshop | Anytime Scorer | 0 | 0.0% | — | — |
| martyr | Moneyline | 11,793 | 100.0% | -2.1pp | -3.8pp |
| martyr | Total Goals | 0 | 0.0% | — | — |
| martyr | BTTS | 0 | 0.0% | — | — |
| martyr | Total Corners | 0 | 0.0% | — | — |
| martyr | Total Cards | 0 | 0.0% | — | — |
| martyr | Anytime Scorer | 0 | 0.0% | — | — |
| chalk | Moneyline | 6,521 | 100.0% | +5.3pp | +0.8pp |
| chalk | Total Goals | 0 | 0.0% | — | — |
| chalk | BTTS | 0 | 0.0% | — | — |
| chalk | Total Corners | 0 | 0.0% | — | — |
| chalk | Total Cards | 0 | 0.0% | — | — |
| chalk | Anytime Scorer | 0 | 0.0% | — | — |
| hoarder | Moneyline | 6,185 | 100.0% | +3.9pp | -0.9pp |
| hoarder | Total Goals | 0 | 0.0% | — | — |
| hoarder | BTTS | 0 | 0.0% | — | — |
| hoarder | Total Corners | 0 | 0.0% | — | — |
| hoarder | Total Cards | 0 | 0.0% | — | — |
| hoarder | Anytime Scorer | 0 | 0.0% | — | — |
| ironhands | Moneyline | 6,895 | 100.0% | +9.6pp | +3.0pp |
| ironhands | Total Goals | 0 | 0.0% | — | — |
| ironhands | BTTS | 0 | 0.0% | — | — |
| ironhands | Total Corners | 0 | 0.0% | — | — |
| ironhands | Total Cards | 0 | 0.0% | — | — |
| ironhands | Anytime Scorer | 0 | 0.0% | — | — |
| martyr-worst | Moneyline | 15,257 | 100.0% | -3.1pp | -2.8pp |
| martyr-worst | Total Goals | 0 | 0.0% | — | — |
| martyr-worst | BTTS | 0 | 0.0% | — | — |
| martyr-worst | Total Corners | 0 | 0.0% | — | — |
| martyr-worst | Total Cards | 0 | 0.0% | — | — |
| martyr-worst | Anytime Scorer | 0 | 0.0% | — | — |

> Anytime Scorer is intentionally zero: it is a declared human-agency market, excluded from every bot.

## 2. Item power audit (3 passives + 3 consumables)

Skilled baseline: median death 5, mean rounds 4.88, won 2.1%. Passives granted at run start; consumables refilled every round. Exposure = uses (consumables) or wound-up runs (ratchets); ±  is the paired-seed SE on Δwon. Sorted by Δ mean rounds survived.

| Item | kind | mean rounds | Δ mean | median death | won % | Δ won % (±SE) | exposure | totem fires |
|---|---|---|---|---|---|---|---|---|
| Totem of Undying | passive | 5.92 | +1.04 | 6 | 2.6% | +0.5pp (±0.5) | — | 96.4% |
| Mulligan Slip | consumable | 5.52 | +0.64 | 5 | 7.4% | +5.3pp (±0.7) | 4,603 uses | — |
| Free Bet Token | consumable | 5.48 | +0.6 | 5 | 3.2% | +1.1pp (±0.5) | 5,128 uses | — |
| Unopened Bobblehead | passive | 5.15 | +0.27 | 5 | 3.1% | +1.0pp (±0.5) | — | — |
| Bookie's Marker | consumable | 5.13 | +0.25 | 5 | 2.3% | +0.2pp (±0.2) | 1,775 uses | — |
| The Multiplier | passive | 5.12 | +0.24 | 5 | 3.4% | +1.3pp (±0.5) | — | — |
| Longshot Larry's Photo | passive | 5.11 | +0.23 | 5 | 3.6% | +1.5pp (±0.5) | — | — |
| Ref's Whistle | consumable | 5.10 | +0.22 | 5 | 5.2% | +3.1pp (±0.6) | 1,263 uses | — |
| Golden Parachute | passive | 5.09 | +0.21 | 5 | 2.1% | 0.0pp (±0.6) | — | — |
| Profit Boost | consumable | 5.01 | +0.14 | 5 | 3.0% | +0.9pp (±0.3) | 4,691 uses | — |
| Bad Beat Jar | passive | 5.00 | +0.13 | 5 | 3.1% | +1.0pp (±0.5) | 962 wound | — |
| Scar Tissue | passive | 4.95 | +0.07 | 5 | 2.5% | +0.4pp (±0.4) | 664 wound | — |
| The Rake's Rebate | passive | 4.95 | +0.07 | 5 | 2.5% | +0.4pp (±0.5) | — | — |
| Ask for the Manager | consumable | 4.94 | +0.06 | 5 | 3.2% | +1.1pp (±0.6) | 3,905 uses | — |
| Comp'd Suite | passive | 4.92 | +0.04 | 5 | 2.4% | +0.3pp (±0.5) | — | — |
| Iron Hands | passive | 4.90 | +0.02 | 5 | 2.8% | +0.7pp (±0.5) | 70 wound | — |
| The System | passive | 4.88 | +0.01 | 5 | 2.5% | +0.4pp (±0.5) | 92 wound | — |
| Chalk Eater | passive | 4.88 | 0 | 5 | 2.7% | +0.6pp (±0.5) | — | — |
| The Collection | passive | 4.87 | -0.01 | 5 | 2.2% | +0.1pp (±0.4) | — | — |
| Double or Nothing Slip | consumable | 4.86 | -0.02 | 4 | 3.7% | +1.6pp (±0.4) | 3,440 uses | — |
| House Key | passive | 4.71 | -0.17 | 4 | 3.1% | +1.0pp (±0.5) | — | — |
| Whale Card | passive | 4.44 | -0.44 | 4 | 1.5% | -0.6pp (±0.4) | — | — |

> Takeaway: the gate section's item flags carry the verdicts (DEAD / DOMINANT / totem band). Note the audit grants items FREE — organic play also pays shop prices out of payment headroom.

## 3. Variance feel

Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.

| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |
|---|---|---|---|---|---|---|---|---|
| naive | $87 | $107 | $183 | $216 | n/a | n/a | n/a | n/a |
| random | $91 | $150 | $530 | $4,059 | $489 | $8,303 | $1,535,906 | $952,242,415,694 |
| skilled | $29 | $29 | $296 | $4,234 | $240 | $1,009 | $13,403 | $60,110 |
| noshop | $62 | $111 | $138 | $309 | n/a | n/a | n/a | n/a |
| martyr | $87 | $87 | $874 | $5,215 | $0 | $1,371 | $8,128 | $69,810 |
| chalk | $66 | $112 | $182 | $695 | $93 | $113 | $132 | $137 |
| hoarder | $62 | $111 | $162 | $415 | $214 | $239 | $264 | $270 |
| ironhands | $67 | $112 | $324 | $1,550 | $64 | $234 | $658 | $990 |
| martyr-worst | $87 | $87 | $1,620 | $16,672 | $310 | $2,571 | $19,786 | $78,506 |

> Takeaway: swing p50 vs p99 shows whether the ride is flat (boring) or spiky (feels rigged); a p99 many multiples of p50 is the intended rare-blowup shape.

## 4. Ratchet telemetry (Scar Tissue)

Mean peak stacks (pp) and mean carrier burns per run — is the ratchet winding and cashing?

| Strategy | mean peak stacks | mean burns/run |
|---|---|---|
| naive | 0.0pp | 0.00 |
| random | 0.5pp | 0.00 |
| skilled | 0.6pp | 0.00 |
| noshop | 0.0pp | 0.00 |
| martyr | 6.9pp | 0.00 |
| chalk | 0.6pp | 0.00 |
| hoarder | 0.1pp | 0.00 |
| ironhands | 0.8pp | 0.00 |
| martyr-worst | 28.6pp | 0.00 |

> Takeaway: zero across the board means nobody buys/holds the scar (price or power problem); huge stacks with zero burns means winding without cashing (the carrier rule isn't landing).

## 5. EV-arc

Mean *true* per-ticket EV at lock, by round (the economy doctrine's crossing from −vig toward positive).

| Round | naive mean EV | random mean EV | skilled mean EV | noshop mean EV | martyr mean EV | chalk mean EV | hoarder mean EV | ironhands mean EV | martyr-worst mean EV |
|---|---|---|---|---|---|---|---|---|---|
| R1 | -$8 | -$10 | -$4 | -$2 | -$7 | -$2 | -$2 | -$2 | $32 |
| R2 | -$6 | -$7 | $3 | -$3 | $10 | $2 | -$3 | $8 | $37 |
| R3 | -$5 | -$6 | $6 | -$2 | $18 | $2 | $1 | $7 | $57 |
| R4 | -$3 | -$8 | $6 | -$1 | $17 | $1 | $2 | $3 | $82 |
| R5 | -$3 | -$15 | $20 | -$2 | $49 | $1 | -$0 | $3 | $134 |
| R6 | -$3 | -$37 | $44 | -$3 | $136 | $7 | $0 | $8 | $296 |
| R7 | -$1 | -$288 | $158 | -$4 | $141 | $23 | $8 | $35 | $481 |
| R8 | — | -$33,672 | $211 | — | $207 | $61 | $37 | $62 | $815 |

- Skilled mean EV first crosses zero at **round 5** (target ≈ round 4).
- **Survivorship caveat:** the round-5+ means average only the few runs that got there (R5 n=375 tickets vs R1 n=1000). The MEDIAN skilled run dies at round 2, so it never reaches the +EV band — the arc is real for the surviving tail, not the typical player.
- Naive mean EV never crosses zero — as intended.

> Takeaway: among survivors the +EV relics do flip the arc (crosses ~R5, target ≈R4), but almost nobody survives to bank it — the mechanic works, the economy gates it away.

## 6. Band-3 audit

Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).

| Strategy | p99 final bank | top-1% mean | max |
|---|---|---|---|
| naive | $248 | $299 | $344 |
| random | $873 | $105,804,863,780 | $1,058,046,957,893 |
| skilled | $1,021 | $14,392 | $68,106 |
| noshop | $145 | $244 | $365 |
| martyr | $1,362 | $13,204 | $80,111 |
| chalk | $586 | $845 | $1,184 |
| hoarder | $292 | $475 | $1,133 |
| ironhands | $927 | $1,019 | $1,182 |
| martyr-worst | $14,392 | $37,574 | $98,178 |


Pairwise relic synergy (1,000 runs/config, baseline won 0.0%). Synergy excess = pair Δwon − (soloA Δ + soloB Δ). Top 10:

| Pair | pair won % | synergy excess (pp) | tag |
|---|---|---|---|
| House Key + The System | 1.0% | +0.8 | marginal (no real loop) |
| Whale Card + The System | 0.5% | +0.4 | marginal (no real loop) |
| Whale Card + House Key | 0.7% | +0.4 | marginal (no real loop) |
| Bad Beat Jar + House Key | 0.5% | +0.3 | marginal (no real loop) |
| Longshot Larry's Photo + Whale Card | 0.6% | +0.3 | marginal (no real loop) |
| Longshot Larry's Photo + House Key | 0.7% | +0.3 | marginal (no real loop) |
| House Key + The Collection | 0.4% | +0.2 | marginal (no real loop) |
| Longshot Larry's Photo + The System | 0.4% | +0.2 | marginal (no real loop) |
| The System + The Collection | 0.2% | +0.2 | marginal (no real loop) |
| Iron Hands + House Key | 0.4% | +0.2 | marginal (no real loop) |

> Takeaway: brokenness should live in the tail (max ≫ p99) and, if combos ran, in a few hard-to-assemble pairs rather than trivial cheap ones.

## 7. Grind metric

Median decisions per round (tickets + cash-outs + buys). Fewer decisions late than mid = flat repetition.

| Round | naive | random | skilled | noshop | martyr | chalk | hoarder | ironhands | martyr-worst |
|---|---|---|---|---|---|---|---|---|---|
| R1 | 1 | 3 | 2 | 1 | 3 | 3 | 1 | 3 | 3 |
| R2 | 1 | 3 | 4 | 1 | 3 | 3 | 2 | 4 | 3 |
| R3 | 1 | 3 | 4 | 1 | 1 | 3 | 3 | 3 | 3 |
| R4 | 1 | 2 | 3 | 1 | 2 | 2 | 2 | 3 | 1 |
| R5 | 1 | 2 | 1 | 1 | 2 | 2 | 1 | 2 | 3 |
| R6 | 1 | 3 | 2 | 1 | 1 | 2 | 2 | 2 | 1 |
| R7 | 1 | 3 | 3 | 1 | 1 | 2 | 1 | 2 | 3 |
| R8 | — | 3 | 2 | — | 2 | 2 | 1 | 2 | 2 |

> Takeaway: late rounds carry FEWER decisions than mid for: skilled, martyr, chalk, hoarder, ironhands, martyr-worst — repetition-risk flag.

