# /sim — Monte Carlo balance report

- Date: 2026-08-15 15:21
- Engine: workspace is not a git repo — `git describe` unavailable
- Config: bank $350, PAYMENTS [60, 70, 85, 105, 155, 375, 710, 1350] (avg ×1.56), overround 5.0%, SGP margin κ 1.0, cash-out margin 8.0%, totem juice 50.0%, min stake $10, max stake 100.0% of bank, 6 matchups/round, 3 tickets/round, 5 relic + 3 consumable slots
- Workers: 16 (manual, --workers); 22 logical cores; server GC
- Strategies: naive, random, skilled, noshop, martyr, chalk, hoarder, ironhands, samematch, martyr-worst
- Seed: **pinned** — run i uses engine seed "HOLDOUT6-{i}". Same arguments reproduce this report's body byte-for-byte; the header's date and wall time are the exceptions and carry no verdict. `--verify` is the standing self-check.
- Runs per batch: 10,000
- Total runs (incl. audit/combos): 1,530,000
- Wall time: 1743.90 s

## 0. Gate campaign (PLAN.md acceptance criteria)

| Gate | Criterion | Verdict | Actual | Resolution |
|---|---|---|---|---|
| G1 | honest gambling: naive win <1%, dies before the cliff resolves (median ≤6) | **PASS** | median 4, won 0.0% | — |
| G2 | engine mandatory: no-shop skilled win <2%, median death 5–6 | **PASS** | median 5, won 0.0% | — |
| G3 | skilled + items wins: median death ≥5, win 4.5–8% (re-banded by Allen 2026-08-08 from 5–8%: the economy reads 5.4–5.5%, only 0.4–0.5pp above the old floor, so the gate could not separate its own reading from its own edge — three campaigns at 4,600 / 10,000 / 18,500 established that no sample size fixes a gap that small. Prior band Allen 2026-07-15 — the dealt hand's build variance is the roguelite shape) | **PASS** | median 5, won 5.3% (0.8pp from the nearest band edge) | ±0.45pp (2 SE) — band 3.5pp is 7.8× resolution; resolves its whole band |
| G4 | the EV arc exists: skilled mean ticket EV crosses zero in rounds 3–7 | **PASS** | crosses at R3 | — |
| G5 | composition superadditive: the exemplar pair's synergy excess ≥ 1pp (exemplar moved by Allen 2026-08-12 to Longshot Larry's Photo + House Key on the draws re-baseline: the prior exemplar, The Multiplier + House Key, fell +2.96pp → +1.22pp under draws — its synergy still real at 5.4× its own error, but the floor ended up inside the reading's resolution and the gate stopped adjudicating. Escalation to 18,500 was computed to buy ~1.3× and REFUSED. THE FLOOR IS UNCHANGED at 1.0pp — it is the report's own marginal/superadditive line, set AFTER the error was measured, and moving it to fit a reading was the alternative not taken. Prior exemplar Allen 2026-08-08, itself moved from Multiplier + Scar Tissue at +0.1pp against ±0.06pp — real, but the weakest loop in the table) | **PASS** | synergy excess +2.8pp | ±0.35pp (2 SE, paired seeds) — one-sided floor at 1pp, so no band width to state; this reading clears it by +1.80pp, 5.2× resolution |
| G6 | martyr guard (worst case granted): loss-farming win ≤ skilled +2pp | **PASS** | martyr-worst 5.0% vs skilled 5.3% (organic martyr 1.5%) — margin -0.3pp, 2.3pp from the +2pp line | ±0.62pp (2 SE) — band 2pp is 3.2× resolution; fails reliably on a breach ≥0.62pp past the edge, no closer |
| G7 | market coverage: every shipped MarketKind is exercised by the skilled bot (LegsPlaced > 0) or on the named bot-excluded list | **PASS** | all shipped markets covered | exact — a leg count is not a sample; no resolution limit |
| G7-SGP | same-match coverage: the SAME MATCH probe placed AND settled same-match tickets, EVERY shipped MarketKind reached a same-match ticket (or is on the named exclusion list with a reason), and zero tickets were sold at the no-label naive-product fallback (a ticket shape is invisible to G7's MarketKind roll-call; a market the probe never pairs is a joint nothing priced; and a silent fallback is a money leak worth up to +274% EV on an implication pair), AND same-match tickets were cashed out (the conditional quote is live product code and a campaign that never quotes it is not covering it) | **PASS** | placed 106,205, settled 81,097, kinds covered 15/15, no-label fallbacks 0, refusals tripped 34,437, voids re-priced 11,655, cashed out 25,108 (14,984 early / 3,396 mid / 6,728 last-leg) | exact — a ticket count is not a sample; no resolution limit |

Gates evaluated: **8** · passed: **8** · produced a verdict: **8**.

- ⚑ UNDEREXPOSED: Chalk Eater (0 wound-up runs < 200)
- ℹ 1X2 SPLIT: skilled placed 2,141 DRAW legs of 92,473 moneyline legs (2.3%) — telemetry for the draws re-baseline, not a gate criterion
- ℹ BOT-EXCLUDED: BTTS — near-even two-way market: under exact de-vig it never strictly wins a tie and its odds never clear the longshot threshold, so a sharp correctly declines it (M1, expires at v2 pricing) — measured reachable: random places 4,632 BTTS legs where skilled places 0, so the market is DECLINED, not blocked
- ℹ BOT-EXCLUDED: Anytime Scorer — YES-only market, bots do not price it (declared policy)
- ℹ BOT-EXCLUDED: DoubleChance — its three selections OVERLAP — 1X and X2 both contain the draw — so normalizing the implied probabilities is double counting, not de-vig. Structural, and it does not expire at v2 pricing the way BTTS does — measured reachable: random places 6,844 legs where skilled places 0, so it is DECLINED, not blocked
- ℹ BOT-EXCLUDED: CorrectScore — the board is TRUNCATED at the ratified 2% probability floor, so the offered scores are not an exhaustive outcome set; normalizing them would over-normalize and manufacture an edge out of the missing rows — measured reachable: random places 31,705 legs where skilled places 0, so it is DECLINED, not blocked
- ℹ BOT-EXCLUDED: WinningMargin — one-way buckets that deliberately omit the draw (margin 0), so the set is not a partition and de-vig has no denominator — measured reachable: random places 6,764 legs where skilled places 0, so it is DECLINED, not blocked
- ℹ BOT-EXCLUDED: TotalGoalsOddEven — THE most near-even market on the board: measured across the latent box under draws it prices odd 0.490–0.499 / even 0.501–0.510, i.e. odds 1.87–1.94 on both sides, and NEITHER side reaches the 3.0 longshot threshold at any sampled latent point (0 of 105). Under exact de-vig it therefore never strictly wins a tie and no owned item can lift it, so a sharp correctly declines it — the BTTS shape, and stronger. Worth recording WHY it is this balanced: every draw carries an EVEN goal total (h+h), so the old no-draws truncation was deleting even mass and had skewed parity to 64/36; restoring draws restored it to ~50/50. Expires at v2 pricing, same as BTTS — measured reachable: random places 4,523 legs where skilled places 0, so it is DECLINED, not blocked
- ℹ BOT-EXCLUDED: PlayerMultiScorer — YES-only player market on a floor-truncated board — inherits the AnytimeScorer human-agency policy, and its offered rows do not sum to the outcome space so there is nothing to de-vig against (declared policy)
- ℹ PLAYTEST-GATED: Ask for the Manager audits ≈0 through bots (Δwon -+0.0±0.9pp) — RATIFIED KEEP, playtest #9

> **NOT DONE — iterate the knobs (item numbers / prices / curve) and rerun.**

## 0b. SAME MATCH exposure (informational — NOT a gate)

From the `samematch` batch. Whether the feature is covered is G7-SGP's verdict; how thinly each relation is covered is this table's, and the two are deliberately different instruments.

Tickets placed: **106,205** · settled: **81,097** · cashed out: **25,108** · legs voided and re-priced: **11,655** · refusals tripped: **34,437**

Of the **106,205** same-match tickets that reached an outcome, **76.4 %** were graded and **23.6 %** were cashed out (**363,487** banked). Cash-outs by position in the sweat: **14,984** with nothing settled · **3,396** mid-sweat · **6,728** on the last leg.

| Relation | Relations priced | Tickets carrying it | Times principal |
|---|---:|---:|---:|
| Implies | 52,190 | 28,210 | 19,924 |
| SharedScoreline | 98,823 | 45,806 | 24,570 |
| ScorerOfSide | 39,343 | 21,739 | 15,874 |
| SharedCount | 31,670 | 25,835 | 25,835 |
| Independent | 43,342 | 25,837 | 0 |

Refusal rules exercised: ImpossibleCombination × 24,437 · DuplicateSelection × 10,000 · SubEvens × 0. SubEvens reads zero at the shipped κ = 1 by construction — the sub-evens price and its full-ticket refund need κ ≳ 1.3, so that path stays unit-test-only in this campaign.

Not exercised: MutuallyExclusive. MutuallyExclusive can never appear here by construction — it is the label on a combination the engine REFUSES, so it is never on a placed ticket; the refusal counters above are where it is read. Any other name in this line is a real hole in the probe's catalogue.

| Market kind | Same-match legs | Tickets carrying it |
|---|---:|---:|
| Moneyline | 43,690 | 43,690 |
| Total Goals | 22,994 | 22,552 |
| BTTS | 28,159 | 28,159 |
| Total Corners | 65,837 | 45,837 |
| Total Cards | 15,837 | 15,837 |
| Anytime Scorer | 21,739 | 21,739 |
| DoubleChance | 15,908 | 15,908 |
| Handicap | 5,908 | 5,908 |
| TeamTotalGoals | 17,703 | 11,784 |
| CorrectScore | 10,076 | 10,076 |
| WinningMargin | 5,908 | 5,908 |
| TotalGoalsOddEven | 5,956 | 5,956 |
| TeamTotalCorners | 5,835 | 5,835 |
| TeamTotalCards | 5,835 | 5,835 |
| PlayerMultiScorer | 5,865 | 5,865 |

Every shipped market kind reached a same-match ticket. Legs are counted only where the matchup carried at least two of the ticket's legs, so a kind's number here is joint pricing it actually went through, not a leg riding along on someone else's parlay.

## 1. Survival curves

Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).

| Metric | naive | random | skilled | noshop | martyr | chalk | hoarder | ironhands | samematch | martyr-worst |
|---|---|---|---|---|---|---|---|---|---|---|
| enter R1 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R2 | 100.0% | 99.9% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R3 | 100.0% | 72.7% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R4 | 61.7% | 33.7% | 100.0% | 100.0% | 24.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R5 | 31.7% | 15.7% | 75.1% | 59.3% | 21.6% | 68.7% | 62.4% | 74.7% | 68.9% | 27.9% |
| enter R6 | 13.2% | 8.2% | 44.8% | 20.2% | 11.7% | 35.7% | 26.1% | 43.4% | 5.1% | 27.6% |
| enter R7 | 2.7% | 3.8% | 23.0% | 1.4% | 5.8% | 16.8% | 7.4% | 23.0% | 0.0% | 15.1% |
| enter R8 | 0.5% | 2.1% | 12.5% | 0.0% | 2.7% | 9.2% | 1.9% | 12.4% | 0.0% | 8.7% |
| **won %** | **0.0%** | **1.0%** | **5.3%** | **0.0%** | **1.5%** | **4.9%** | **0.5%** | **5.2%** | **0.0%** | **5.0%** |
| **median death round** | **4** | **3** | **5** | **5** | **3** | **5** | **5** | **5** | **5** | **4** |
| mean rounds reached | 4.10 | 3.37 | 5.61 | 4.81 | 3.67 | 5.35 | 4.98 | 5.59 | 4.74 | 4.84 |
| totem fire rate | 0.0% | 4.1% | 30.5% | 0.0% | 0.0% | 19.4% | 9.3% | 20.7% | 0.0% | 0.0% |
| close-call deaths (% of deaths) | 17.5% | 15.0% | 5.1% | 5.2% | 2.9% | 6.9% | 5.6% | 4.7% | 20.1% | 2.7% |
| mean bookie gifts per run | 0.74 | 0.51 | 0.41 | 0.51 | 1.01 | 0.30 | 0.51 | 0.43 | 0.63 | 1.09 |

> Takeaway: naive dies at round 4, skilled reaches 5 — skill buys real extra survival; compare against the 3–4 / ≥7 targets above.

## 2. Market exposure

Placed legs and equal-split stake share by market kind. `mean leg EV` is the UNWEIGHTED per-leg return (converges to −vig under fair pricing — the sanity column); `stake-wtd EV` is stake-weighted and fat-tailed under compounding banks (a few monster tickets dominate it — read it as variance, not edge). Both are single-leg, before parlay multiplication, cash-outs, voids, or relic factors.

| Strategy | market | legs placed | stake share | mean leg EV | stake-wtd EV |
|---|---|---|---|---|---|
| naive | Moneyline | 81,226 | 100.0% | -5.1pp | -5.2pp |
| naive | Total Goals | 0 | 0.0% | — | — |
| naive | BTTS | 0 | 0.0% | — | — |
| naive | Total Corners | 0 | 0.0% | — | — |
| naive | Total Cards | 0 | 0.0% | — | — |
| naive | Anytime Scorer | 0 | 0.0% | — | — |
| naive | DoubleChance | 0 | 0.0% | — | — |
| naive | Handicap | 0 | 0.0% | — | — |
| naive | TeamTotalGoals | 0 | 0.0% | — | — |
| naive | CorrectScore | 0 | 0.0% | — | — |
| naive | WinningMargin | 0 | 0.0% | — | — |
| naive | TotalGoalsOddEven | 0 | 0.0% | — | — |
| naive | TeamTotalCorners | 0 | 0.0% | — | — |
| naive | TeamTotalCards | 0 | 0.0% | — | — |
| naive | PlayerMultiScorer | 0 | 0.0% | — | — |
| random | Moneyline | 6,767 | 0.1% | -1.7pp | -31.2pp |
| random | Total Goals | 13,686 | 27.0% | -3.1pp | +14.8pp |
| random | BTTS | 4,632 | 19.5% | -4.7pp | -99.7pp |
| random | Total Corners | 13,424 | 0.5% | -5.2pp | -22.3pp |
| random | Total Cards | 13,499 | 10.9% | -4.9pp | -97.8pp |
| random | Anytime Scorer | 0 | 0.0% | — | — |
| random | DoubleChance | 6,844 | 0.2% | -4.6pp | +25.2pp |
| random | Handicap | 9,048 | 0.5% | +0.4pp | -1.2pp |
| random | TeamTotalGoals | 17,857 | 24.5% | -2.5pp | -37.7pp |
| random | CorrectScore | 31,705 | 14.3% | +6.4pp | -89.2pp |
| random | WinningMargin | 6,764 | 0.6% | +3.1pp | -84.0pp |
| random | TotalGoalsOddEven | 4,523 | 0.2% | -3.4pp | +49.2pp |
| random | TeamTotalCorners | 9,075 | 0.4% | -3.5pp | -38.7pp |
| random | TeamTotalCards | 9,023 | 0.3% | -3.1pp | +14.1pp |
| random | PlayerMultiScorer | 12,980 | 1.1% | +10.7pp | -86.6pp |
| skilled | Moneyline | 92,473 | 70.2% | +7.5pp | +11.5pp |
| skilled | Total Goals | 2,979 | 7.3% | +28.7pp | +18.4pp |
| skilled | BTTS | 0 | 0.0% | — | — |
| skilled | Total Corners | 1,969 | 4.6% | +27.4pp | +196.2pp |
| skilled | Total Cards | 2,363 | 3.5% | +17.3pp | +15.3pp |
| skilled | Anytime Scorer | 0 | 0.0% | — | — |
| skilled | DoubleChance | 0 | 0.0% | — | — |
| skilled | Handicap | 2,630 | 4.3% | +30.9pp | +14.8pp |
| skilled | TeamTotalGoals | 4,050 | 5.6% | +19.8pp | +51.5pp |
| skilled | CorrectScore | 0 | 0.0% | — | — |
| skilled | WinningMargin | 0 | 0.0% | — | — |
| skilled | TotalGoalsOddEven | 0 | 0.0% | — | — |
| skilled | TeamTotalCorners | 956 | 2.1% | +17.2pp | +5.4pp |
| skilled | TeamTotalCards | 1,821 | 2.6% | +26.2pp | +64.7pp |
| skilled | PlayerMultiScorer | 0 | 0.0% | — | — |
| noshop | Moneyline | 76,496 | 100.0% | -3.7pp | -4.4pp |
| noshop | Total Goals | 0 | 0.0% | — | — |
| noshop | BTTS | 0 | 0.0% | — | — |
| noshop | Total Corners | 0 | 0.0% | — | — |
| noshop | Total Cards | 0 | 0.0% | — | — |
| noshop | Anytime Scorer | 0 | 0.0% | — | — |
| noshop | DoubleChance | 0 | 0.0% | — | — |
| noshop | Handicap | 0 | 0.0% | — | — |
| noshop | TeamTotalGoals | 0 | 0.0% | — | — |
| noshop | CorrectScore | 0 | 0.0% | — | — |
| noshop | WinningMargin | 0 | 0.0% | — | — |
| noshop | TotalGoalsOddEven | 0 | 0.0% | — | — |
| noshop | TeamTotalCorners | 0 | 0.0% | — | — |
| noshop | TeamTotalCards | 0 | 0.0% | — | — |
| noshop | PlayerMultiScorer | 0 | 0.0% | — | — |
| martyr | Moneyline | 105,388 | 100.0% | -4.1pp | -6.7pp |
| martyr | Total Goals | 0 | 0.0% | — | — |
| martyr | BTTS | 0 | 0.0% | — | — |
| martyr | Total Corners | 0 | 0.0% | — | — |
| martyr | Total Cards | 0 | 0.0% | — | — |
| martyr | Anytime Scorer | 0 | 0.0% | — | — |
| martyr | DoubleChance | 0 | 0.0% | — | — |
| martyr | Handicap | 0 | 0.0% | — | — |
| martyr | TeamTotalGoals | 0 | 0.0% | — | — |
| martyr | CorrectScore | 0 | 0.0% | — | — |
| martyr | WinningMargin | 0 | 0.0% | — | — |
| martyr | TotalGoalsOddEven | 0 | 0.0% | — | — |
| martyr | TeamTotalCorners | 0 | 0.0% | — | — |
| martyr | TeamTotalCards | 0 | 0.0% | — | — |
| martyr | PlayerMultiScorer | 0 | 0.0% | — | — |
| chalk | Moneyline | 103,563 | 100.0% | +6.4pp | +7.0pp |
| chalk | Total Goals | 0 | 0.0% | — | — |
| chalk | BTTS | 0 | 0.0% | — | — |
| chalk | Total Corners | 0 | 0.0% | — | — |
| chalk | Total Cards | 0 | 0.0% | — | — |
| chalk | Anytime Scorer | 0 | 0.0% | — | — |
| chalk | DoubleChance | 0 | 0.0% | — | — |
| chalk | Handicap | 0 | 0.0% | — | — |
| chalk | TeamTotalGoals | 0 | 0.0% | — | — |
| chalk | CorrectScore | 0 | 0.0% | — | — |
| chalk | WinningMargin | 0 | 0.0% | — | — |
| chalk | TotalGoalsOddEven | 0 | 0.0% | — | — |
| chalk | TeamTotalCorners | 0 | 0.0% | — | — |
| chalk | TeamTotalCards | 0 | 0.0% | — | — |
| chalk | PlayerMultiScorer | 0 | 0.0% | — | — |
| hoarder | Moneyline | 83,544 | 100.0% | +2.2pp | -0.2pp |
| hoarder | Total Goals | 0 | 0.0% | — | — |
| hoarder | BTTS | 0 | 0.0% | — | — |
| hoarder | Total Corners | 0 | 0.0% | — | — |
| hoarder | Total Cards | 0 | 0.0% | — | — |
| hoarder | Anytime Scorer | 0 | 0.0% | — | — |
| hoarder | DoubleChance | 0 | 0.0% | — | — |
| hoarder | Handicap | 0 | 0.0% | — | — |
| hoarder | TeamTotalGoals | 0 | 0.0% | — | — |
| hoarder | CorrectScore | 0 | 0.0% | — | — |
| hoarder | WinningMargin | 0 | 0.0% | — | — |
| hoarder | TotalGoalsOddEven | 0 | 0.0% | — | — |
| hoarder | TeamTotalCorners | 0 | 0.0% | — | — |
| hoarder | TeamTotalCards | 0 | 0.0% | — | — |
| hoarder | PlayerMultiScorer | 0 | 0.0% | — | — |
| ironhands | Moneyline | 108,268 | 100.0% | +11.0pp | +14.8pp |
| ironhands | Total Goals | 0 | 0.0% | — | — |
| ironhands | BTTS | 0 | 0.0% | — | — |
| ironhands | Total Corners | 0 | 0.0% | — | — |
| ironhands | Total Cards | 0 | 0.0% | — | — |
| ironhands | Anytime Scorer | 0 | 0.0% | — | — |
| ironhands | DoubleChance | 0 | 0.0% | — | — |
| ironhands | Handicap | 0 | 0.0% | — | — |
| ironhands | TeamTotalGoals | 0 | 0.0% | — | — |
| ironhands | CorrectScore | 0 | 0.0% | — | — |
| ironhands | WinningMargin | 0 | 0.0% | — | — |
| ironhands | TotalGoalsOddEven | 0 | 0.0% | — | — |
| ironhands | TeamTotalCorners | 0 | 0.0% | — | — |
| ironhands | TeamTotalCards | 0 | 0.0% | — | — |
| ironhands | PlayerMultiScorer | 0 | 0.0% | — | — |
| samematch | Moneyline | 43,690 | 14.4% | -1.0pp | +0.1pp |
| samematch | Total Goals | 22,994 | 9.5% | -3.9pp | -3.8pp |
| samematch | BTTS | 28,159 | 11.1% | -2.3pp | -2.4pp |
| samematch | Total Corners | 65,837 | 29.3% | +1.1pp | +1.0pp |
| samematch | Total Cards | 15,837 | 7.1% | -5.5pp | -5.8pp |
| samematch | Anytime Scorer | 21,739 | 7.4% | -3.2pp | -3.8pp |
| samematch | DoubleChance | 15,908 | 5.6% | -1.1pp | -1.5pp |
| samematch | Handicap | 5,908 | 1.4% | -2.8pp | -3.3pp |
| samematch | TeamTotalGoals | 17,703 | 4.2% | -1.3pp | -1.8pp |
| samematch | CorrectScore | 10,076 | 3.1% | +7.0pp | +5.3pp |
| samematch | WinningMargin | 5,908 | 1.4% | -0.5pp | -1.2pp |
| samematch | TotalGoalsOddEven | 5,956 | 1.4% | -4.6pp | -4.8pp |
| samematch | TeamTotalCorners | 5,835 | 1.4% | +4.0pp | +3.4pp |
| samematch | TeamTotalCards | 5,835 | 1.4% | -4.0pp | -4.0pp |
| samematch | PlayerMultiScorer | 5,865 | 1.4% | +13.8pp | +12.7pp |
| martyr-worst | Moneyline | 132,327 | 100.0% | -4.0pp | -8.4pp |
| martyr-worst | Total Goals | 0 | 0.0% | — | — |
| martyr-worst | BTTS | 0 | 0.0% | — | — |
| martyr-worst | Total Corners | 0 | 0.0% | — | — |
| martyr-worst | Total Cards | 0 | 0.0% | — | — |
| martyr-worst | Anytime Scorer | 0 | 0.0% | — | — |
| martyr-worst | DoubleChance | 0 | 0.0% | — | — |
| martyr-worst | Handicap | 0 | 0.0% | — | — |
| martyr-worst | TeamTotalGoals | 0 | 0.0% | — | — |
| martyr-worst | CorrectScore | 0 | 0.0% | — | — |
| martyr-worst | WinningMargin | 0 | 0.0% | — | — |
| martyr-worst | TotalGoalsOddEven | 0 | 0.0% | — | — |
| martyr-worst | TeamTotalCorners | 0 | 0.0% | — | — |
| martyr-worst | TeamTotalCards | 0 | 0.0% | — | — |
| martyr-worst | PlayerMultiScorer | 0 | 0.0% | — | — |

> Anytime Scorer is intentionally zero: it is a declared human-agency market, excluded from every bot.

## 2. Item power audit (3 passives + 3 consumables)

Skilled baseline: median death 5, mean rounds 5.61, won 5.3%. Passives granted at run start; consumables refilled every round. Exposure = uses (consumables) or wound-up runs (ratchets); ±  is the paired-seed SE on Δwon. Sorted by Δ mean rounds survived.

| Item | kind | mean rounds | Δ mean | median death | won % | Δ won % (±SE) | exposure | totem fires |
|---|---|---|---|---|---|---|---|---|---|
| Free Bet Token | consumable | 6.31 | +0.7 | 6 | 3.7% | -1.6pp (±0.2) | 57,110 uses | — |
| Totem of Undying | passive | 6.31 | +0.7 | 6 | 6.5% | +1.2pp (±0.3) | — | 91.0% |
| Mulligan Slip | consumable | 6.25 | +0.65 | 6 | 13.2% | +7.9pp (±0.3) | 30,721 uses | — |
| Profit Boost | consumable | 5.94 | +0.33 | 6 | 9.2% | +3.9pp (±0.2) | 55,490 uses | — |
| Ref's Whistle | consumable | 5.87 | +0.27 | 6 | 7.3% | +2.0pp (±0.2) | 9,436 uses | — |
| Unopened Bobblehead | passive | 5.81 | +0.2 | 6 | 8.0% | +2.7pp (±0.3) | — | — |
| Longshot Larry's Photo | passive | 5.77 | +0.16 | 6 | 4.6% | -0.7pp (±0.3) | — | — |
| Bookie's Marker | consumable | 5.77 | +0.16 | 6 | 5.2% | -0.1pp (±0.2) | 19,836 uses | — |
| House Key | passive | 5.73 | +0.12 | 5 | 10.2% | +4.9pp (±0.4) | — | — |
| The Rake's Rebate | passive | 5.71 | +0.1 | 5 | 6.3% | +1.0pp (±0.3) | — | — |
| Scar Tissue | passive | 5.70 | +0.09 | 5 | 5.9% | +0.6pp (±0.3) | 6,063 wound | — |
| Bad Beat Jar | passive | 5.70 | +0.09 | 5 | 6.8% | +1.5pp (±0.3) | 8,019 wound | — |
| Double or Nothing Slip | consumable | 5.69 | +0.08 | 5 | 7.5% | +2.2pp (±0.2) | 14,659 uses | — |
| The Collection | passive | 5.65 | +0.05 | 5 | 6.1% | +0.8pp (±0.3) | — | — |
| Comp'd Suite | passive | 5.65 | +0.04 | 5 | 6.0% | +0.7pp (±0.3) | — | — |
| The System | passive | 5.63 | +0.02 | 5 | 6.8% | +1.5pp (±0.3) | 2,009 wound | — |
| Ask for the Manager | consumable | 5.62 | +0.01 | 5 | 5.3% | 0.0pp (±0.3) | 45,670 uses | — |
| Iron Hands | passive | 5.60 | -0.01 | 5 | 5.9% | +0.6pp (±0.3) | 4,023 wound | — |
| Chalk Eater | passive | 5.57 | -0.04 | 5 | 5.7% | +0.4pp (±0.3) | — | — |
| Golden Parachute | passive | 5.55 | -0.06 | 5 | 3.1% | -2.2pp (±0.3) | — | — |
| Whale Card | passive | 5.08 | -0.53 | 5 | 0.5% | -4.8pp (±0.2) | — | — |
| The Multiplier | passive | 5.04 | -0.57 | 4 | 11.8% | +6.5pp (±0.4) | — | — |

> Takeaway: the gate section's item flags carry the verdicts (DEAD / DOMINANT / totem band). Note the audit grants items FREE — organic play also pays shop prices out of payment headroom.

## 3. Variance feel

Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.

| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |
|---|---|---|---|---|---|---|---|---|---|
| naive | $87 | $87 | $329 | $646 | $351 | $1,272 | $1,567 | $1,633 |
| random | $83 | $138 | $498 | $7,741 | $636 | $7,863 | $205,720 | $2,019,175,700 |
| skilled | $57 | $112 | $767 | $8,658 | $147 | $1,729 | $23,829 | $998,399 |
| noshop | $55 | $111 | $174 | $316 | n/a | n/a | n/a | n/a |
| martyr | $87 | $87 | $886 | $7,289 | $1 | $2,063 | $26,294 | $117,508 |
| chalk | $58 | $112 | $517 | $4,669 | $104 | $1,628 | $13,746 | $239,360 |
| hoarder | $56 | $111 | $196 | $1,076 | $37 | $1,252 | $6,077 | $13,326 |
| ironhands | $58 | $132 | $759 | $11,776 | $130 | $1,676 | $27,196 | $186,964 |
| samematch | $21 | $36 | $79 | $196 | n/a | n/a | n/a | n/a |
| martyr-worst | $87 | $87 | $1,518 | $25,335 | $100 | $4,091 | $38,628 | $273,975 |

> Takeaway: swing p50 vs p99 shows whether the ride is flat (boring) or spiky (feels rigged); a p99 many multiples of p50 is the intended rare-blowup shape.

## 4. Ratchet telemetry (Scar Tissue)

Mean peak stacks (pp) and mean carrier burns per run — is the ratchet winding and cashing?

| Strategy | mean peak stacks | mean burns/run |
|---|---|---|
| naive | 0.0pp | 0.00 |
| random | 0.5pp | 0.00 |
| skilled | 1.8pp | 0.00 |
| noshop | 0.0pp | 0.00 |
| martyr | 5.8pp | 0.00 |
| chalk | 0.4pp | 0.00 |
| hoarder | 0.0pp | 0.00 |
| ironhands | 0.8pp | 0.00 |
| samematch | 0.0pp | 0.00 |
| martyr-worst | 27.5pp | 0.00 |

> Takeaway: zero across the board means nobody buys/holds the scar (price or power problem); huge stacks with zero burns means winding without cashing (the carrier rule isn't landing).

## 5. EV-arc

Mean *true* per-ticket EV at lock, by round (the economy doctrine's crossing from −vig toward positive).

| Round | naive mean EV | random mean EV | skilled mean EV | noshop mean EV | martyr mean EV | chalk mean EV | hoarder mean EV | ironhands mean EV | samematch mean EV | martyr-worst mean EV |
|---|---|---|---|---|---|---|---|---|---|---|
| R1 | -$8 | -$10 | -$4 | -$4 | -$7 | -$4 | -$4 | -$4 | -$2 | $34 |
| R2 | -$6 | -$8 | $3 | -$3 | $9 | $3 | -$3 | $15 | -$1 | $39 |
| R3 | -$5 | -$10 | $21 | -$1 | $28 | $16 | -$0 | $22 | -$1 | $68 |
| R4 | -$5 | -$25 | $19 | -$2 | $36 | $18 | $5 | $18 | -$1 | $150 |
| R5 | -$5 | -$40 | $31 | -$3 | $57 | $36 | $7 | $31 | -$1 | $225 |
| R6 | -$7 | -$98 | $67 | -$3 | $125 | $68 | $8 | $62 | -$1 | $482 |
| R7 | -$10 | -$841 | $166 | -$4 | $298 | $134 | $32 | $151 | — | $909 |
| R8 | -$15 | -$128,960 | $1,189 | -$8 | $685 | $388 | $78 | $399 | — | $1,930 |

- Skilled mean EV first crosses zero at **round 3** (target ≈ round 4).
- **Survivorship caveat:** the round-3+ means average only the few runs that got there (R3 n=9636 tickets vs R1 n=10000). The MEDIAN skilled run dies at round 2, so it never reaches the +EV band — the arc is real for the surviving tail, not the typical player.
- Naive mean EV never crosses zero — as intended.

> Takeaway: among survivors the +EV relics do flip the arc (crosses ~R3, target ≈R4), but almost nobody survives to bank it — the mechanic works, the economy gates it away.

## 6. Band-3 audit

Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).

| Strategy | p99 final bank | top-1% mean | max |
|---|---|---|---|
| naive | $329 | $528 | $1,640 |
| random | $925 | $437,620,112 | $41,591,600,207 |
| skilled | $9,729 | $194,484 | $3,134,767 |
| noshop | $151 | $226 | $411 |
| martyr | $1,167 | $17,332 | $456,826 |
| chalk | $5,321 | $76,957 | $3,560,274 |
| hoarder | $450 | $1,668 | $16,294 |
| ironhands | $11,206 | $69,146 | $1,816,870 |
| samematch | $148 | $180 | $373 |
| martyr-worst | $19,554 | $70,239 | $541,616 |


Pairwise relic synergy (10,000 runs/config, baseline won 0.0%). Synergy excess = pair Δwon − (soloA Δ + soloB Δ). Top 10:

| Pair | pair won % | synergy excess (pp) | ±2 SE (paired) | tag |
|---|---|---|---|---|
| Longshot Larry's Photo + House Key | 4.9% | +2.8 | ±0.35 | superadditive — 8.1× its own error |
| Longshot Larry's Photo + Whale Card | 4.1% | +2.08 | ±0.30 | superadditive — 6.8× its own error |
| The Multiplier + Whale Card | 3.1% | +1.48 | ±0.24 | superadditive — 6.1× its own error |
| The Multiplier + House Key | 3.0% | +1.36 | ±0.24 | superadditive — 5.8× its own error |
| Longshot Larry's Photo + Bad Beat Jar | 2.9% | +0.86 | ±0.21 | marginal — 4.0× its own error |
| Totem of Undying + Longshot Larry's Photo | 2.9% | +0.78 | ±0.18 | marginal — 4.3× its own error |
| Longshot Larry's Photo + The System | 2.8% | +0.73 | ±0.21 | marginal — 3.5× its own error |
| The Multiplier + Bad Beat Jar | 2.2% | +0.62 | ±0.16 | marginal — 3.9× its own error |
| Whale Card + House Key | 0.6% | +0.62 | ±0.16 | marginal — 3.9× its own error |
| Longshot Larry's Photo + The Collection | 2.6% | +0.6 | ±0.20 | marginal — 3.1× its own error |

Ranked by excess; the ±2 SE column is paired by seed, so it is the error of the *combination*, not of any one arm. A row whose excess is inside its own error is tagged as such and its rank means nothing.

> Takeaway: brokenness should live in the tail (max ≫ p99) and, if combos ran, in a few pairs that clear their own error rather than across the whole table.

## 7. Grind metric

Median decisions per round (tickets + cash-outs + buys). Fewer decisions late than mid = flat repetition.

| Round | naive | random | skilled | noshop | martyr | chalk | hoarder | ironhands | samematch | martyr-worst |
|---|---|---|---|---|---|---|---|---|---|---|
| R1 | 1 | 3 | 3 | 1 | 3 | 3 | 1 | 2 | 4 | 3 |
| R2 | 1 | 3 | 3 | 1 | 3 | 4 | 1 | 4 | 5 | 3 |
| R3 | 1 | 2 | 3 | 1 | 0 | 3 | 3 | 4 | 5 | 3 |
| R4 | 1 | 2 | 3 | 1 | 3 | 2 | 2 | 3 | 3 | 0 |
| R5 | 1 | 3 | 2 | 1 | 2 | 2 | 1 | 2 | 0 | 3 |
| R6 | 1 | 3 | 2 | 1 | 1 | 2 | 1 | 2 | 0 | 1 |
| R7 | 1 | 3 | 3 | 1 | 1 | 2 | 1 | 3 | 0 | 2 |
| R8 | 1 | 3 | 2 | 1 | 1 | 2 | 2 | 2 | — | 1 |

> Takeaway: late rounds carry FEWER decisions than mid for: skilled, martyr, chalk, hoarder, ironhands, samematch, martyr-worst — repetition-risk flag.

