# /sim — Monte Carlo balance report

- Date: 2026-07-15 21:11
- Engine: workspace is not a git repo — `git describe` unavailable
- Config: bank $350, PAYMENTS [60, 70, 85, 105, 155, 375, 710, 1350] (avg ×1.56), overround 5.0%, cash-out margin 8.0%, totem juice 50.0%, min stake $10, max stake 100.0% of bank, 6 matchups/round, 3 tickets/round, 5 relic + 3 consumable slots
- Strategies: naive, skilled, noshop, martyr, chalk, hoarder, ironhands, martyr-worst
- Runs per batch: 50,000
- Total runs (incl. audit/combos): 7,550,000
- Wall time: 1141.93 s

## 0. Gate campaign (PLAN.md acceptance criteria)

| Gate | Criterion | Verdict | Actual |
|---|---|---|---|
| G1 | honest gambling: naive win <1%, dies before the cliff resolves (median ≤6) | **PASS** | median 4, won 0.0% |
| G2 | engine mandatory: no-shop skilled win <2%, median death 5–6 | **PASS** | median 5, won 0.0% |
| G3 | skilled + items wins: median death ≥5, win 5–8% (re-banded by Allen 2026-07-15 — the dealt hand's build variance is the roguelite shape) | **PASS** | median 5, won 7.0% |
| G4 | the EV arc exists: skilled mean ticket EV crosses zero in rounds 3–7 | **PASS** | crosses at R3 |
| G5 | composition superadditive: Multiplier+Scar pair Δwin > sum of solo Δwins | **PASS** | synergy excess +0.1pp |
| G6 | martyr guard (worst case granted): loss-farming win ≤ skilled +2pp | **PASS** | martyr-worst 5.9% vs skilled 7.0% (organic martyr 1.5%) |

Item flags: none — no DEAD items, no DOMINANT item, Totem in the healthy band.
- ℹ PLAYTEST-GATED: Ask for the Manager audits ≈0 through bots (Δwon +0.1±0.4pp) — playtest #9 votes

> **ALL GATES PASS — the economy holds.**

## 1. Survival curves

Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).

| Metric | naive | skilled | noshop | martyr | chalk | hoarder | ironhands | martyr-worst |
|---|---|---|---|---|---|---|---|---|
| enter R1 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R2 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R3 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R4 | 86.2% | 98.9% | 100.0% | 39.4% | 100.0% | 100.0% | 98.7% | 100.0% |
| enter R5 | 44.1% | 77.6% | 68.2% | 26.8% | 80.2% | 72.9% | 75.2% | 45.2% |
| enter R6 | 10.8% | 49.0% | 17.7% | 14.7% | 36.6% | 29.4% | 45.5% | 34.5% |
| enter R7 | 0.3% | 28.1% | 1.6% | 7.1% | 14.4% | 8.1% | 27.0% | 19.4% |
| enter R8 | 0.0% | 15.9% | 0.0% | 3.2% | 6.4% | 2.2% | 16.5% | 10.5% |
| **won %** | **0.0%** | **7.0%** | **0.0%** | **1.5%** | **2.1%** | **0.3%** | **8.4%** | **5.9%** |
| **median death round** | **4** | **5** | **5** | **3** | **5** | **5** | **5** | **4** |
| mean rounds reached | 4.41 | 5.77 | 4.88 | 3.93 | 5.40 | 5.13 | 5.71 | 5.15 |
| totem fire rate | 0.0% | 37.4% | 0.0% | 0.0% | 15.8% | 11.1% | 26.8% | 0.0% |
| close-call deaths (% of deaths) | 13.0% | 5.6% | 3.4% | 4.5% | 4.8% | 4.1% | 5.9% | 5.8% |
| mean bookie gifts per run | 0.46 | 0.41 | 0.75 | 0.87 | 0.51 | 0.67 | 0.37 | 1.03 |

> Takeaway: naive dies at round 4, skilled reaches 5 — skill buys real extra survival; compare against the 3–4 / ≥7 targets above.

## 2. Item power audit (3 passives + 3 consumables)

Skilled baseline: median death 5, mean rounds 5.77, won 7.0%. Passives granted at run start; consumables refilled every round. Exposure = uses (consumables) or wound-up runs (ratchets); ±  is the paired-seed SE on Δwon. Sorted by Δ mean rounds survived.

| Item | kind | mean rounds | Δ mean | median death | won % | Δ won % (±SE) | exposure | totem fires |
|---|---|---|---|---|---|---|---|---|
| Mulligan Slip | consumable | 7.17 | +1.4 | 7 | 26.8% | +19.7pp (±0.2) | 199,589 uses | — |
| Free Bet Token | consumable | 6.61 | +0.84 | 7 | 2.1% | -4.9pp (±0.1) | 312,653 uses | — |
| Totem of Undying | passive | 6.34 | +0.58 | 6 | 8.2% | +1.2pp (±0.1) | — | 88.6% |
| Double or Nothing Slip | consumable | 6.26 | +0.49 | 6 | 18.4% | +11.4pp (±0.1) | 136,975 uses | — |
| Unopened Bobblehead | passive | 6.21 | +0.44 | 6 | 15.3% | +8.2pp (±0.2) | — | — |
| Profit Boost | consumable | 6.15 | +0.38 | 6 | 13.9% | +6.9pp (±0.1) | 281,480 uses | — |
| The Multiplier | passive | 5.94 | +0.17 | 5 | 23.8% | +16.7pp (±0.2) | — | — |
| Bookie's Marker | consumable | 5.92 | +0.15 | 6 | 6.7% | -0.3pp (±0.1) | 105,409 uses | — |
| Scar Tissue | passive | 5.89 | +0.12 | 6 | 8.2% | +1.1pp (±0.1) | 20,798 wound | — |
| Ref's Whistle | consumable | 5.86 | +0.1 | 6 | 7.3% | +0.3pp (±0.1) | 19,153 uses | — |
| The Rake's Rebate | passive | 5.84 | +0.07 | 6 | 9.4% | +2.4pp (±0.1) | — | — |
| Ask for the Manager | consumable | 5.77 | 0 | 5 | 7.1% | +0.1pp (±0.1) | 234,911 uses | — |
| Comp'd Suite | passive | 5.72 | -0.05 | 5 | 7.2% | +0.2pp (±0.1) | — | — |
| Golden Parachute | passive | 5.69 | -0.08 | 5 | 1.6% | -5.4pp (±0.1) | — | — |
| House Key | passive | 5.65 | -0.12 | 5 | 19.0% | +12.0pp (±0.2) | — | — |
| Bad Beat Jar | passive | 5.57 | -0.19 | 5 | 15.3% | +8.3pp (±0.2) | 29,389 wound | — |
| The Collection | passive | 5.57 | -0.2 | 5 | 15.6% | +8.6pp (±0.2) | — | — |
| The System | passive | 5.54 | -0.23 | 5 | 16.3% | +9.3pp (±0.2) | 18,849 wound | — |
| Chalk Eater | passive | 5.54 | -0.23 | 5 | 15.9% | +8.9pp (±0.2) | 47,517 wound | — |
| Iron Hands | passive | 5.50 | -0.26 | 5 | 14.9% | +7.9pp (±0.2) | 10,605 wound | — |
| Longshot Larry's Photo | passive | 5.50 | -0.27 | 5 | 14.5% | +7.5pp (±0.2) | — | — |
| Whale Card | passive | 4.96 | -0.81 | 4 | 11.3% | +4.3pp (±0.2) | — | — |

> Takeaway: the gate section's item flags carry the verdicts (DEAD / DOMINANT / totem band). Note the audit grants items FREE — organic play also pays shop prices out of payment headroom.

## 3. Variance feel

Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.

| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |
|---|---|---|---|---|---|---|---|---|
| naive | $87 | $113 | $184 | $222 | n/a | n/a | n/a | n/a |
| skilled | $71 | $137 | $876 | $2,818 | $108 | $1,279 | $4,543 | $12,553 |
| noshop | $51 | $118 | $191 | $368 | n/a | n/a | n/a | n/a |
| martyr | $87 | $87 | $842 | $4,816 | $1 | $2,456 | $19,053 | $112,522 |
| chalk | $48 | $107 | $348 | $1,752 | $65 | $653 | $3,210 | $18,934 |
| hoarder | $51 | $119 | $209 | $725 | $38 | $283 | $1,655 | $7,266 |
| ironhands | $77 | $139 | $1,070 | $3,380 | $106 | $1,469 | $5,742 | $20,504 |
| martyr-worst | $87 | $87 | $1,478 | $15,613 | $362 | $3,350 | $23,837 | $119,036 |

> Takeaway: swing p50 vs p99 shows whether the ride is flat (boring) or spiky (feels rigged); a p99 many multiples of p50 is the intended rare-blowup shape.

## 4. Ratchet telemetry (Scar Tissue)

Mean peak stacks (pp) and mean carrier burns per run — is the ratchet winding and cashing?

| Strategy | mean peak stacks | mean burns/run |
|---|---|---|
| naive | 0.0pp | 0.00 |
| skilled | 2.1pp | 0.00 |
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
| R2 | -$7 | $5 | -$6 | $8 | -$2 | -$6 | $7 | $36 |
| R3 | -$5 | $15 | -$2 | $16 | $2 | -$0 | $14 | $55 |
| R4 | -$3 | $27 | -$4 | $27 | $4 | $3 | $29 | $83 |
| R5 | -$3 | $46 | -$7 | $46 | $10 | $4 | $49 | $134 |
| R6 | -$3 | $62 | -$5 | $91 | $19 | $6 | $73 | $269 |
| R7 | -$2 | $87 | -$6 | $187 | $44 | $26 | $100 | $509 |
| R8 | — | $115 | -$8 | $380 | $84 | $89 | $127 | $1,030 |

- Skilled mean EV first crosses zero at **round 3** (target ≈ round 4).
- **Survivorship caveat:** the round-3+ means average only the few runs that got there (R3 n=48168 tickets vs R1 n=50000). The MEDIAN skilled run dies at round 2, so it never reaches the +EV band — the arc is real for the surviving tail, not the typical player.
- Naive mean EV never crosses zero — as intended.

> Takeaway: among survivors the +EV relics do flip the arc (crosses ~R3, target ≈R4), but almost nobody survives to bank it — the mechanic works, the economy gates it away.

## 6. Band-3 audit

Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).

| Strategy | p99 final bank | top-1% mean | max |
|---|---|---|---|
| naive | $254 | $306 | $375 |
| skilled | $3,763 | $6,966 | $38,025 |
| noshop | $142 | $217 | $629 |
| martyr | $1,191 | $13,333 | $468,831 |
| chalk | $1,205 | $3,593 | $135,178 |
| hoarder | $434 | $898 | $11,311 |
| ironhands | $5,230 | $11,158 | $147,312 |
| martyr-worst | $15,194 | $46,605 | $581,623 |


Pairwise relic synergy (50,000 runs/config, baseline won 0.0%). Synergy excess = pair Δwon − (soloA Δ + soloB Δ). Top 10:

| Pair | pair won % | synergy excess (pp) | tag |
|---|---|---|---|
| The Multiplier + House Key | 4.2% | +3.04 | degenerate: cheap pair, trivially assembled |
| The Multiplier + Whale Card | 3.7% | +2.52 | degenerate: cheap pair, trivially assembled |
| Whale Card + House Key | 1.4% | +1.06 | degenerate: cheap pair, trivially assembled |
| The Multiplier + The System | 1.8% | +0.74 | marginal (no real loop) |
| The Multiplier + Chalk Eater | 1.7% | +0.68 | marginal (no real loop) |
| The Multiplier + Iron Hands | 1.5% | +0.51 | marginal (no real loop) |
| The Multiplier + The Collection | 1.5% | +0.45 | marginal (no real loop) |
| The Multiplier + Bad Beat Jar | 1.4% | +0.4 | marginal (no real loop) |
| Whale Card + Comp'd Suite | 0.6% | +0.38 | marginal (no real loop) |
| Chalk Eater + House Key | 0.6% | +0.34 | marginal (no real loop) |

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
| R7 | 1 | 2 | 1 | 1 | 2 | 2 | 2 | 2 |
| R8 | — | 2 | 1 | 1 | 2 | 2 | 2 | 2 |

> Takeaway: late rounds carry FEWER decisions than mid for: skilled, martyr, chalk, hoarder, ironhands, martyr-worst — repetition-risk flag.

