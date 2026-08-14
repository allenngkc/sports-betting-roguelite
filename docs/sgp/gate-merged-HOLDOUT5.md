# /sim — Monte Carlo balance report

- Date: 2026-08-13 23:55
- Engine: workspace is not a git repo — `git describe` unavailable
- Config: bank $350, PAYMENTS [60, 70, 85, 105, 155, 375, 710, 1350] (avg ×1.56), overround 5.0%, SGP margin κ 1.0, cash-out margin 8.0%, totem juice 50.0%, min stake $10, max stake 100.0% of bank, 6 matchups/round, 3 tickets/round, 5 relic + 3 consumable slots
- Strategies: naive, random, skilled, noshop, martyr, chalk, hoarder, ironhands, samematch, martyr-worst
- Seed: **pinned** — run i uses engine seed "HOLDOUT5-{i}". Same arguments reproduce this report's body byte-for-byte; the header's date and wall time are the exceptions and carry no verdict. `--verify` is the standing self-check.
- Runs per batch: 10,000
- Total runs (incl. audit/combos): 1,530,000
- Wall time: 4717.48 s

## 0. Gate campaign (PLAN.md acceptance criteria)

| Gate | Criterion | Verdict | Actual | Resolution |
|---|---|---|---|---|
| G1 | honest gambling: naive win <1%, dies before the cliff resolves (median ≤6) | **PASS** | median 4, won 0.0% | — |
| G2 | engine mandatory: no-shop skilled win <2%, median death 5–6 | **PASS** | median 5, won 0.0% | — |
| G3 | skilled + items wins: median death ≥5, win 4.5–8% (re-banded by Allen 2026-08-08 from 5–8%: the economy reads 5.4–5.5%, only 0.4–0.5pp above the old floor, so the gate could not separate its own reading from its own edge — three campaigns at 4,600 / 10,000 / 18,500 established that no sample size fixes a gap that small. Prior band Allen 2026-07-15 — the dealt hand's build variance is the roguelite shape) | **PASS** | median 5, won 5.0% (0.5pp from the nearest band edge) | ±0.44pp (2 SE) — band 3.5pp is 8.0× resolution; resolves its whole band |
| G4 | the EV arc exists: skilled mean ticket EV crosses zero in rounds 3–7 | **PASS** | crosses at R3 | — |
| G5 | composition superadditive: the exemplar pair's synergy excess ≥ 1pp (exemplar moved by Allen 2026-08-12 to Longshot Larry's Photo + House Key on the draws re-baseline: the prior exemplar, The Multiplier + House Key, fell +2.96pp → +1.22pp under draws — its synergy still real at 5.4× its own error, but the floor ended up inside the reading's resolution and the gate stopped adjudicating. Escalation to 18,500 was computed to buy ~1.3× and REFUSED. THE FLOOR IS UNCHANGED at 1.0pp — it is the report's own marginal/superadditive line, set AFTER the error was measured, and moving it to fit a reading was the alternative not taken. Prior exemplar Allen 2026-08-08, itself moved from Multiplier + Scar Tissue at +0.1pp against ±0.06pp — real, but the weakest loop in the table) | **PASS** | synergy excess +2.6pp | ±0.35pp (2 SE, paired seeds) — one-sided floor at 1pp, so no band width to state; this reading clears it by +1.57pp, 4.5× resolution |
| G6 | martyr guard (worst case granted): loss-farming win ≤ skilled +2pp | **PASS** | martyr-worst 4.4% vs skilled 5.0% (organic martyr 1.1%) — margin -0.6pp, 2.6pp from the +2pp line | ±0.60pp (2 SE) — band 2pp is 3.3× resolution; fails reliably on a breach ≥0.60pp past the edge, no closer |
| G7 | market coverage: every shipped MarketKind is exercised by the skilled bot (LegsPlaced > 0) or on the named bot-excluded list | **PASS** | all shipped markets covered | exact — a leg count is not a sample; no resolution limit |
| G7-SGP | same-match coverage: the SAME MATCH probe placed AND settled same-match tickets, EVERY shipped MarketKind reached a same-match ticket (or is on the named exclusion list with a reason), and zero tickets were sold at the no-label naive-product fallback (a ticket shape is invisible to G7's MarketKind roll-call; a market the probe never pairs is a joint nothing priced; and a silent fallback is a money leak worth up to +274% EV on an implication pair) | **PASS** | placed 106,568, settled 106,568, kinds covered 15/15, no-label fallbacks 0, refusals tripped 34,752, voids re-priced 11,541 | exact — a ticket count is not a sample; no resolution limit |

Gates evaluated: **8** · passed: **8** · produced a verdict: **8**.

- ⚑ UNDEREXPOSED: Chalk Eater (0 wound-up runs < 200)
- ℹ 1X2 SPLIT: skilled placed 2,214 DRAW legs of 91,722 moneyline legs (2.4%) — telemetry for the draws re-baseline, not a gate criterion
- ℹ BOT-EXCLUDED: BTTS — near-even two-way market: under exact de-vig it never strictly wins a tie and its odds never clear the longshot threshold, so a sharp correctly declines it (M1, expires at v2 pricing) — measured reachable: random places 4,337 BTTS legs where skilled places 0, so the market is DECLINED, not blocked
- ℹ BOT-EXCLUDED: Anytime Scorer — YES-only market, bots do not price it (declared policy)
- ℹ BOT-EXCLUDED: DoubleChance — its three selections OVERLAP — 1X and X2 both contain the draw — so normalizing the implied probabilities is double counting, not de-vig. Structural, and it does not expire at v2 pricing the way BTTS does — measured reachable: random places 6,843 legs where skilled places 0, so it is DECLINED, not blocked
- ℹ BOT-EXCLUDED: CorrectScore — the board is TRUNCATED at the ratified 2% probability floor, so the offered scores are not an exhaustive outcome set; normalizing them would over-normalize and manufacture an edge out of the missing rows — measured reachable: random places 31,832 legs where skilled places 0, so it is DECLINED, not blocked
- ℹ BOT-EXCLUDED: WinningMargin — one-way buckets that deliberately omit the draw (margin 0), so the set is not a partition and de-vig has no denominator — measured reachable: random places 6,799 legs where skilled places 0, so it is DECLINED, not blocked
- ℹ BOT-EXCLUDED: TotalGoalsOddEven — THE most near-even market on the board: measured across the latent box under draws it prices odd 0.490–0.499 / even 0.501–0.510, i.e. odds 1.87–1.94 on both sides, and NEITHER side reaches the 3.0 longshot threshold at any sampled latent point (0 of 105). Under exact de-vig it therefore never strictly wins a tie and no owned item can lift it, so a sharp correctly declines it — the BTTS shape, and stronger. Worth recording WHY it is this balanced: every draw carries an EVEN goal total (h+h), so the old no-draws truncation was deleting even mass and had skewed parity to 64/36; restoring draws restored it to ~50/50. Expires at v2 pricing, same as BTTS — measured reachable: random places 4,623 legs where skilled places 0, so it is DECLINED, not blocked
- ℹ BOT-EXCLUDED: PlayerMultiScorer — YES-only player market on a floor-truncated board — inherits the AnytimeScorer human-agency policy, and its offered rows do not sum to the outcome space so there is nothing to de-vig against (declared policy)
- ℹ PLAYTEST-GATED: Ask for the Manager audits ≈0 through bots (Δwon -0.3±0.9pp) — RATIFIED KEEP, playtest #9

> **NOT DONE — iterate the knobs (item numbers / prices / curve) and rerun.**

## 0b. SAME MATCH exposure (informational — NOT a gate)

From the `samematch` batch. Whether the feature is covered is G7-SGP's verdict; how thinly each relation is covered is this table's, and the two are deliberately different instruments.

Tickets placed: **106,568** · settled: **106,568** · legs voided and re-priced: **11,541** · refusals tripped: **34,752**

| Relation | Relations priced | Tickets carrying it | Times principal |
|---|---:|---:|---:|
| Implies | 52,721 | 28,616 | 20,224 |
| SharedScoreline | 99,030 | 45,860 | 24,561 |
| ScorerOfSide | 39,545 | 21,839 | 15,972 |
| SharedCount | 31,615 | 25,808 | 25,808 |
| Independent | 43,231 | 25,810 | 0 |

Refusal rules exercised: ImpossibleCombination × 24,751 · DuplicateSelection × 10,001 · SubEvens × 0. SubEvens reads zero at the shipped κ = 1 by construction — the sub-evens price and its full-ticket refund need κ ≳ 1.3, so that path stays unit-test-only in this campaign.

Not exercised: MutuallyExclusive. MutuallyExclusive can never appear here by construction — it is the label on a combination the engine REFUSES, so it is never on a placed ticket; the refusal counters above are where it is read. Any other name in this line is a real hole in the probe's catalogue.

| Market kind | Same-match legs | Tickets carrying it |
|---|---:|---:|
| Moneyline | 43,815 | 43,815 |
| Total Goals | 23,518 | 22,914 |
| BTTS | 28,009 | 28,009 |
| Total Corners | 65,812 | 45,811 |
| Total Cards | 15,810 | 15,810 |
| Anytime Scorer | 21,839 | 21,839 |
| DoubleChance | 16,012 | 16,012 |
| Handicap | 6,012 | 6,012 |
| TeamTotalGoals | 17,627 | 11,747 |
| CorrectScore | 10,253 | 10,253 |
| WinningMargin | 6,012 | 6,012 |
| TotalGoalsOddEven | 5,960 | 5,960 |
| TeamTotalCorners | 5,807 | 5,807 |
| TeamTotalCards | 5,807 | 5,807 |
| PlayerMultiScorer | 5,867 | 5,867 |

Every shipped market kind reached a same-match ticket. Legs are counted only where the matchup carried at least two of the ticket's legs, so a kind's number here is joint pricing it actually went through, not a leg riding along on someone else's parlay.

## 1. Survival curves

Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).

| Metric | naive | random | skilled | noshop | martyr | chalk | hoarder | ironhands | samematch | martyr-worst |
|---|---|---|---|---|---|---|---|---|---|---|
| enter R1 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R2 | 100.0% | 99.9% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R3 | 100.0% | 72.7% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R4 | 60.7% | 33.7% | 100.0% | 100.0% | 23.7% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R5 | 32.0% | 15.7% | 74.7% | 59.3% | 21.4% | 68.7% | 62.4% | 74.2% | 65.1% | 27.6% |
| enter R6 | 13.5% | 8.4% | 44.6% | 20.1% | 11.4% | 35.3% | 26.5% | 42.2% | 7.0% | 27.4% |
| enter R7 | 2.8% | 4.2% | 22.8% | 1.4% | 5.2% | 16.5% | 7.1% | 23.0% | 0.1% | 14.8% |
| enter R8 | 0.5% | 2.1% | 12.6% | 0.0% | 2.4% | 9.2% | 2.0% | 12.5% | 0.0% | 7.8% |
| **won %** | **0.0%** | **1.0%** | **5.0%** | **0.0%** | **1.1%** | **4.6%** | **0.6%** | **5.4%** | **0.0%** | **4.4%** |
| **median death round** | **4** | **3** | **5** | **5** | **3** | **5** | **5** | **5** | **5** | **4** |
| mean rounds reached | 4.09 | 3.38 | 5.60 | 4.81 | 3.65 | 5.34 | 4.99 | 5.57 | 4.72 | 4.82 |
| totem fire rate | 0.0% | 4.2% | 31.2% | 0.0% | 0.0% | 19.5% | 9.1% | 19.8% | 0.0% | 0.0% |
| close-call deaths (% of deaths) | 17.1% | 15.0% | 4.8% | 5.2% | 2.9% | 6.3% | 5.8% | 5.4% | 18.8% | 2.8% |
| mean bookie gifts per run | 0.74 | 0.51 | 0.42 | 0.52 | 1.01 | 0.31 | 0.51 | 0.43 | 0.58 | 1.10 |

> Takeaway: naive dies at round 4, skilled reaches 5 — skill buys real extra survival; compare against the 3–4 / ≥7 targets above.

## 2. Market exposure

Placed legs and equal-split stake share by market kind. `mean leg EV` is the UNWEIGHTED per-leg return (converges to −vig under fair pricing — the sanity column); `stake-wtd EV` is stake-weighted and fat-tailed under compounding banks (a few monster tickets dominate it — read it as variance, not edge). Both are single-leg, before parlay multiplication, cash-outs, voids, or relic factors.

| Strategy | market | legs placed | stake share | mean leg EV | stake-wtd EV |
|---|---|---|---|---|---|
| naive | Moneyline | 81,064 | 100.0% | -4.8pp | -4.6pp |
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
| random | Moneyline | 6,782 | 16.1% | -3.2pp | -99.8pp |
| random | Total Goals | 13,621 | 0.1% | -3.3pp | -49.4pp |
| random | BTTS | 4,337 | 0.1% | -4.1pp | -80.3pp |
| random | Total Corners | 13,764 | 17.0% | -3.3pp | -37.0pp |
| random | Total Cards | 13,524 | 0.2% | -3.2pp | -18.4pp |
| random | Anytime Scorer | 0 | 0.0% | — | — |
| random | DoubleChance | 6,843 | 0.0% | -4.7pp | -14.4pp |
| random | Handicap | 9,109 | 0.0% | -4.7pp | -34.9pp |
| random | TeamTotalGoals | 18,119 | 0.1% | -3.6pp | -4.7pp |
| random | CorrectScore | 31,832 | 17.1% | +6.1pp | -98.7pp |
| random | WinningMargin | 6,799 | 16.1% | -4.1pp | -99.9pp |
| random | TotalGoalsOddEven | 4,623 | 0.0% | -2.9pp | -29.6pp |
| random | TeamTotalCorners | 9,092 | 0.0% | -4.1pp | -11.3pp |
| random | TeamTotalCards | 8,891 | 16.1% | -3.4pp | +38.7pp |
| random | PlayerMultiScorer | 12,951 | 17.0% | +11.0pp | +1417.0pp |
| skilled | Moneyline | 91,722 | 77.5% | +7.5pp | +12.4pp |
| skilled | Total Goals | 3,243 | 4.8% | +17.9pp | +2.1pp |
| skilled | BTTS | 0 | 0.0% | — | — |
| skilled | Total Corners | 1,998 | 2.1% | +22.9pp | +21.1pp |
| skilled | Total Cards | 2,407 | 2.9% | +21.9pp | +2.4pp |
| skilled | Anytime Scorer | 0 | 0.0% | — | — |
| skilled | DoubleChance | 0 | 0.0% | — | — |
| skilled | Handicap | 2,604 | 3.5% | +28.8pp | +44.9pp |
| skilled | TeamTotalGoals | 4,153 | 5.5% | +24.3pp | +22.9pp |
| skilled | CorrectScore | 0 | 0.0% | — | — |
| skilled | WinningMargin | 0 | 0.0% | — | — |
| skilled | TotalGoalsOddEven | 0 | 0.0% | — | — |
| skilled | TeamTotalCorners | 1,045 | 1.4% | +29.3pp | +19.2pp |
| skilled | TeamTotalCards | 1,745 | 2.3% | +20.8pp | +17.2pp |
| skilled | PlayerMultiScorer | 0 | 0.0% | — | — |
| noshop | Moneyline | 76,454 | 100.0% | -3.2pp | -4.0pp |
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
| martyr | Moneyline | 104,657 | 100.0% | -5.6pp | -10.5pp |
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
| chalk | Moneyline | 103,329 | 100.0% | +6.6pp | +9.4pp |
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
| hoarder | Moneyline | 83,586 | 100.0% | +2.5pp | +1.2pp |
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
| ironhands | Moneyline | 108,329 | 100.0% | +11.3pp | +24.3pp |
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
| samematch | Moneyline | 43,815 | 14.4% | +0.1pp | +1.2pp |
| samematch | Total Goals | 23,518 | 9.7% | -3.6pp | -3.6pp |
| samematch | BTTS | 28,009 | 11.0% | -3.4pp | -3.4pp |
| samematch | Total Corners | 65,812 | 29.2% | +0.8pp | +0.8pp |
| samematch | Total Cards | 15,810 | 7.0% | -5.0pp | -5.4pp |
| samematch | Anytime Scorer | 21,839 | 7.4% | +11.1pp | +14.4pp |
| samematch | DoubleChance | 16,012 | 5.6% | -3.4pp | -3.6pp |
| samematch | Handicap | 6,012 | 1.4% | -3.7pp | -3.8pp |
| samematch | TeamTotalGoals | 17,627 | 4.1% | -3.1pp | -3.2pp |
| samematch | CorrectScore | 10,253 | 3.2% | +1.4pp | -1.4pp |
| samematch | WinningMargin | 6,012 | 1.4% | -1.4pp | -1.4pp |
| samematch | TotalGoalsOddEven | 5,960 | 1.4% | -4.1pp | -3.9pp |
| samematch | TeamTotalCorners | 5,807 | 1.4% | -1.3pp | -1.0pp |
| samematch | TeamTotalCards | 5,807 | 1.4% | -3.2pp | -3.1pp |
| samematch | PlayerMultiScorer | 5,867 | 1.4% | +4.8pp | +4.2pp |
| martyr-worst | Moneyline | 131,398 | 100.0% | -5.5pp | -8.4pp |
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

Skilled baseline: median death 5, mean rounds 5.60, won 5.0%. Passives granted at run start; consumables refilled every round. Exposure = uses (consumables) or wound-up runs (ratchets); ±  is the paired-seed SE on Δwon. Sorted by Δ mean rounds survived.

| Item | kind | mean rounds | Δ mean | median death | won % | Δ won % (±SE) | exposure | totem fires |
|---|---|---|---|---|---|---|---|---|---|
| Free Bet Token | consumable | 6.31 | +0.71 | 6 | 3.8% | -1.3pp (±0.2) | 57,062 uses | — |
| Totem of Undying | passive | 6.29 | +0.69 | 6 | 5.7% | +0.7pp (±0.3) | — | 91.4% |
| Mulligan Slip | consumable | 6.25 | +0.65 | 6 | 13.0% | +8.0pp (±0.3) | 30,816 uses | — |
| Profit Boost | consumable | 5.94 | +0.34 | 6 | 9.4% | +4.4pp (±0.2) | 55,307 uses | — |
| Ref's Whistle | consumable | 5.85 | +0.26 | 6 | 7.0% | +2.0pp (±0.2) | 9,174 uses | — |
| Unopened Bobblehead | passive | 5.78 | +0.18 | 5 | 8.0% | +3.0pp (±0.3) | — | — |
| Longshot Larry's Photo | passive | 5.76 | +0.16 | 6 | 4.6% | -0.5pp (±0.3) | — | — |
| Bookie's Marker | consumable | 5.76 | +0.16 | 6 | 4.7% | -0.4pp (±0.2) | 19,994 uses | — |
| Bad Beat Jar | passive | 5.71 | +0.11 | 5 | 6.8% | +1.8pp (±0.3) | 7,989 wound | — |
| The Rake's Rebate | passive | 5.70 | +0.1 | 5 | 5.8% | +0.8pp (±0.3) | — | — |
| House Key | passive | 5.69 | +0.09 | 5 | 9.9% | +4.9pp (±0.4) | — | — |
| Scar Tissue | passive | 5.68 | +0.08 | 5 | 5.6% | +0.6pp (±0.3) | 6,085 wound | — |
| Double or Nothing Slip | consumable | 5.67 | +0.08 | 5 | 7.5% | +2.4pp (±0.2) | 14,704 uses | — |
| Comp'd Suite | passive | 5.67 | +0.07 | 5 | 6.0% | +1.0pp (±0.3) | — | — |
| The Collection | passive | 5.65 | +0.05 | 5 | 6.6% | +1.5pp (±0.3) | — | — |
| The System | passive | 5.64 | +0.04 | 5 | 7.0% | +1.9pp (±0.3) | 2,016 wound | — |
| Iron Hands | passive | 5.62 | +0.02 | 5 | 6.2% | +1.2pp (±0.3) | 3,905 wound | — |
| Ask for the Manager | consumable | 5.61 | +0.02 | 5 | 4.8% | -0.3pp (±0.3) | 45,661 uses | — |
| Chalk Eater | passive | 5.60 | +0.01 | 5 | 6.0% | +1.0pp (±0.3) | — | — |
| Golden Parachute | passive | 5.56 | -0.04 | 5 | 3.5% | -1.5pp (±0.3) | — | — |
| Whale Card | passive | 5.10 | -0.5 | 5 | 0.5% | -4.5pp (±0.2) | — | — |
| The Multiplier | passive | 4.99 | -0.61 | 4 | 11.7% | +6.7pp (±0.4) | — | — |

> Takeaway: the gate section's item flags carry the verdicts (DEAD / DOMINANT / totem band). Note the audit grants items FREE — organic play also pays shop prices out of payment headroom.

## 3. Variance feel

Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.

| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |
|---|---|---|---|---|---|---|---|---|---|
| naive | $87 | $87 | $333 | $676 | $1,890 | $3,656 | $5,421 | $5,818 |
| random | $83 | $137 | $539 | $7,321 | $311 | $5,165 | $102,719 | $282,909,139,276,518 |
| skilled | $57 | $112 | $769 | $8,939 | $109 | $1,473 | $24,056 | $188,650 |
| noshop | $55 | $111 | $173 | $306 | n/a | n/a | n/a | n/a |
| martyr | $87 | $87 | $838 | $5,984 | $0 | $2,970 | $32,394 | $234,103 |
| chalk | $57 | $112 | $503 | $4,657 | $86 | $1,735 | $12,762 | $515,293 |
| hoarder | $56 | $111 | $198 | $1,172 | $135 | $1,129 | $15,463 | $91,765 |
| ironhands | $58 | $129 | $806 | $8,859 | $112 | $1,372 | $27,331 | $622,938 |
| samematch | $25 | $42 | $91 | $212 | n/a | n/a | n/a | n/a |
| martyr-worst | $87 | $87 | $1,446 | $19,529 | $170 | $3,451 | $43,699 | $336,028 |

> Takeaway: swing p50 vs p99 shows whether the ride is flat (boring) or spiky (feels rigged); a p99 many multiples of p50 is the intended rare-blowup shape.

## 4. Ratchet telemetry (Scar Tissue)

Mean peak stacks (pp) and mean carrier burns per run — is the ratchet winding and cashing?

| Strategy | mean peak stacks | mean burns/run |
|---|---|---|
| naive | 0.0pp | 0.00 |
| random | 0.5pp | 0.00 |
| skilled | 1.9pp | 0.00 |
| noshop | 0.0pp | 0.00 |
| martyr | 5.7pp | 0.00 |
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
| R2 | -$6 | -$7 | $3 | -$3 | $8 | $3 | -$3 | $16 | -$1 | $38 |
| R3 | -$5 | -$9 | $22 | -$1 | $32 | $17 | -$0 | $22 | -$1 | $68 |
| R4 | -$5 | -$17 | $21 | -$2 | $38 | $18 | $5 | $19 | -$1 | $148 |
| R5 | -$6 | -$106 | $42 | -$3 | $74 | $40 | $6 | $31 | -$1 | $231 |
| R6 | -$7 | -$47 | $78 | -$3 | $259 | $66 | $9 | $76 | -$2 | $507 |
| R7 | -$10 | $456 | $178 | -$4 | $606 | $132 | $51 | $317 | — | $1,009 |
| R8 | -$14 | $1,862,885 | $345 | -$7 | $992 | $256 | $155 | $828 | — | $2,532 |

- Skilled mean EV first crosses zero at **round 3** (target ≈ round 4).
- **Survivorship caveat:** the round-3+ means average only the few runs that got there (R3 n=9632 tickets vs R1 n=10000). The MEDIAN skilled run dies at round 2, so it never reaches the +EV band — the arc is real for the surviving tail, not the typical player.
- Naive mean EV never crosses zero — as intended.

> Takeaway: among survivors the +EV relics do flip the arc (crosses ~R3, target ≈R4), but almost nobody survives to bank it — the mechanic works, the economy gates it away.

## 6. Band-3 audit

Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).

| Strategy | p99 final bank | top-1% mean | max |
|---|---|---|---|
| naive | $336 | $553 | $5,862 |
| random | $909 | $141,451,531,401,228 | $14,145,145,160,130,804 |
| skilled | $9,585 | $58,793 | $1,139,437 |
| noshop | $154 | $240 | $437 |
| martyr | $755 | $24,365 | $779,230 |
| chalk | $6,152 | $202,426 | $11,042,996 |
| hoarder | $537 | $4,659 | $181,705 |
| ironhands | $9,235 | $318,936 | $21,166,960 |
| samematch | $152 | $201 | $381 |
| martyr-worst | $13,842 | $85,578 | $1,453,313 |


Pairwise relic synergy (10,000 runs/config, baseline won 0.0%). Synergy excess = pair Δwon − (soloA Δ + soloB Δ). Top 10:

| Pair | pair won % | synergy excess (pp) | ±2 SE (paired) | tag |
|---|---|---|---|---|
| Longshot Larry's Photo + House Key | 4.8% | +2.57 | ±0.35 | superadditive — 7.4× its own error |
| Longshot Larry's Photo + Whale Card | 4.1% | +1.92 | ±0.30 | superadditive — 6.3× its own error |
| The Multiplier + Whale Card | 3.4% | +1.42 | ±0.24 | superadditive — 6.0× its own error |
| The Multiplier + House Key | 3.4% | +1.33 | ±0.24 | superadditive — 5.6× its own error |
| Longshot Larry's Photo + Bad Beat Jar | 2.9% | +0.75 | ±0.22 | marginal — 3.4× its own error |
| Whale Card + House Key | 0.8% | +0.73 | ±0.17 | marginal — 4.2× its own error |
| Totem of Undying + Longshot Larry's Photo | 2.8% | +0.6 | ±0.16 | marginal — 3.7× its own error |
| The Multiplier + Bad Beat Jar | 2.6% | +0.59 | ±0.16 | marginal — 3.8× its own error |
| Longshot Larry's Photo + The System | 2.8% | +0.58 | ±0.20 | marginal — 2.9× its own error |
| Longshot Larry's Photo + The Collection | 2.6% | +0.49 | ±0.19 | marginal — 2.6× its own error |

Ranked by excess; the ±2 SE column is paired by seed, so it is the error of the *combination*, not of any one arm. A row whose excess is inside its own error is tagged as such and its rank means nothing.

> Takeaway: brokenness should live in the tail (max ≫ p99) and, if combos ran, in a few pairs that clear their own error rather than across the whole table.

## 7. Grind metric

Median decisions per round (tickets + cash-outs + buys). Fewer decisions late than mid = flat repetition.

| Round | naive | random | skilled | noshop | martyr | chalk | hoarder | ironhands | samematch | martyr-worst |
|---|---|---|---|---|---|---|---|---|---|---|
| R1 | 1 | 3 | 2 | 1 | 3 | 3 | 1 | 2 | 3 | 3 |
| R2 | 1 | 3 | 3 | 1 | 3 | 4 | 1 | 4 | 4 | 3 |
| R3 | 1 | 2 | 3 | 1 | 0 | 3 | 3 | 4 | 4 | 3 |
| R4 | 1 | 2 | 3 | 1 | 3 | 2 | 2 | 3 | 2 | 0 |
| R5 | 1 | 3 | 2 | 1 | 2 | 2 | 1 | 2 | 0 | 3 |
| R6 | 1 | 3 | 2 | 1 | 1 | 2 | 1 | 2 | 0 | 1 |
| R7 | 1 | 3 | 3 | 1 | 1 | 2 | 2 | 2 | 0 | 1 |
| R8 | 1 | 2 | 2 | 1 | 1 | 2 | 2 | 2 | — | 1 |

> Takeaway: late rounds carry FEWER decisions than mid for: skilled, martyr, chalk, hoarder, ironhands, samematch, martyr-worst — repetition-risk flag.

