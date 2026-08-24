# /sim — Monte Carlo balance report

- Date: 2026-08-24 12:08
- Engine: workspace is not a git repo — `git describe` unavailable
- Config: bank $350, PAYMENTS [60, 70, 85, 105, 155, 375, 710, 1350] (avg ×1.56), overround 5.0%, SGP margin κ 1.0, cash-out margin 8.0%, totem juice 50.0%, min stake $10, max stake 100.0% of bank, 6 matchups/round, 3 tickets/round, 5 relic + 3 consumable slots
- Workers: 16 (manual, --workers); 22 logical cores; server GC
- Strategies: naive, random, skilled, noshop, martyr, chalk, hoarder, ironhands, samematch, martyr-worst
- Seed: **pinned** — run i uses engine seed "SIM-{i}". Same arguments reproduce this report's body byte-for-byte; the header's date and wall time are the exceptions and carry no verdict. `--verify` is the standing self-check.
- Runs per batch: 10,000
- Total runs (incl. audit/combos): 1,530,000
- Wall time: 2720.45 s

## 0. Gate campaign (PLAN.md acceptance criteria)

| Gate | Criterion | Verdict | Actual | Resolution |
|---|---|---|---|---|
| G1 | honest gambling: naive win <1%, dies before the cliff resolves (median ≤6) | **PASS** | median 4, won 0.1% | — |
| G2 | engine mandatory: no-shop skilled win <2%, median death 5–6 | **PASS** | median 5, won 0.0% | — |
| G3 | skilled + items wins: median death ≥5, win 4.5–8% (re-banded by Allen 2026-08-08 from 5–8%: the economy reads 5.4–5.5%, only 0.4–0.5pp above the old floor, so the gate could not separate its own reading from its own edge — three campaigns at 4,600 / 10,000 / 18,500 established that no sample size fixes a gap that small. Prior band Allen 2026-07-15 — the dealt hand's build variance is the roguelite shape) | **PASS** | median 5, won 5.2% (0.7pp from the nearest band edge) | ±0.44pp (2 SE) — band 3.5pp is 7.9× resolution; resolves its whole band |
| G4 | the EV arc exists: skilled mean ticket EV crosses zero in rounds 3–7 | **PASS** | crosses at R3 | — |
| G5 | composition superadditive: the exemplar pair's synergy excess ≥ 1pp (exemplar moved by Allen 2026-08-12 to Longshot Larry's Photo + House Key on the draws re-baseline: the prior exemplar, The Multiplier + House Key, fell +2.96pp → +1.22pp under draws — its synergy still real at 5.4× its own error, but the floor ended up inside the reading's resolution and the gate stopped adjudicating. Escalation to 18,500 was computed to buy ~1.3× and REFUSED. THE FLOOR IS UNCHANGED at 1.0pp — it is the report's own marginal/superadditive line, set AFTER the error was measured, and moving it to fit a reading was the alternative not taken. Prior exemplar Allen 2026-08-08, itself moved from Multiplier + Scar Tissue at +0.1pp against ±0.06pp — real, but the weakest loop in the table) | **PASS** | synergy excess +3.0pp | ±0.36pp (2 SE, paired seeds) — one-sided floor at 1pp, so no band width to state; this reading clears it by +1.95pp, 5.4× resolution |
| G6 | martyr guard (worst case granted): loss-farming win ≤ skilled +2pp | **PASS** | martyr-worst 4.8% vs skilled 5.2% (organic martyr 1.4%) — margin -0.4pp, 2.4pp from the +2pp line | ±0.61pp (2 SE) — band 2pp is 3.3× resolution; fails reliably on a breach ≥0.61pp past the edge, no closer |
| G7 | market coverage: every shipped MarketKind is exercised by the skilled bot (LegsPlaced > 0) or on the named bot-excluded list | **PASS** | all shipped markets covered | exact — a leg count is not a sample; no resolution limit |
| G7-SGP | same-match coverage: the SAME MATCH probe placed AND settled same-match tickets, EVERY shipped MarketKind reached a same-match ticket (or is on the named exclusion list with a reason), and zero tickets were sold at the no-label naive-product fallback (a ticket shape is invisible to G7's MarketKind roll-call; a market the probe never pairs is a joint nothing priced; and a silent fallback is a money leak worth up to +274% EV on an implication pair), AND same-match tickets were cashed out (the conditional quote is live product code and a campaign that never quotes it is not covering it) | **PASS** | placed 106,806, settled 71,339, kinds covered 14/14, no-label fallbacks 0, refusals tripped 34,665, voids re-priced 11,601, cashed out 35,467 (15,060 early / 10,406 mid / 10,001 last-leg) | exact — a ticket count is not a sample; no resolution limit |
| G8-ARMA | T140 arm A restructure: the sweat resolves per (ticket, FIXTURE), not per leg — every leg riding one fixture is live for that fixture's whole telling and grades at its single whistle. A fixture emitting more than one LegFinal means the per-leg sweat is still running; a clock fault is T135's rewind returned (checked on multi-fixture tickets too, where the fixture index legitimately advances). Passes only when the campaign actually exercised a shared telling (sharedTellings > 0 — the coverage arm; without it this gate would pass on a campaign that never built a same-match ticket, i.e. a gate that cannot fail), zero fixtures emitted an extra whistle, zero clock faults, and every shared whistle graded exactly its own live legs. The clock rules are asserted a SECOND time over the skilled batch, which is where MULTI-fixture tickets live: the same-match probe builds only single-fixture tickets, so on its own it witnesses N-legs-one-whistle and never a fixture BOUNDARY — and T140-am's over-reach (reading a correct multi-fixture broadcast as a rewind) can only be ruled out on tickets that have one | **PASS** | tellings 106,806, shared tellings 106,806, whistles 71,339, extra whistles 0, clock faults 0, grades landed at shared whistles 176,747 (expected 176,747), mismatches 0, windows opened 12,316, multi-death windows 5,526 | boundary arm (skilled): multi-fixture tickets 29,019, tellings 81,559, clock faults 0, extra whistles 0 | exact — a beat count is not a sample; no resolution limit |

Gates evaluated: **9** · passed: **9** · produced a verdict: **9**.

- ⚑ UNDEREXPOSED: Chalk Eater (0 wound-up runs < 200)
- ℹ 1X2 SPLIT: skilled placed 2,212 DRAW legs of 92,026 moneyline legs (2.4%) — telemetry for the draws re-baseline, not a gate criterion
- ℹ BOT-EXCLUDED: BTTS — near-even two-way market: under exact de-vig it never strictly wins a tie and its odds never clear the longshot threshold, so a sharp correctly declines it (M1, expires at v2 pricing) — measured reachable: random places 4,743 BTTS legs where skilled places 0, so the market is DECLINED, not blocked
- ℹ BOT-EXCLUDED: Anytime Scorer — YES-only market, bots do not price it (declared policy)
- ℹ BOT-EXCLUDED: DoubleChance — its three selections OVERLAP — 1X and X2 both contain the draw — so normalizing the implied probabilities is double counting, not de-vig. Structural, and it does not expire at v2 pricing the way BTTS does — NOT MEASURED REACHABLE: random placed 0 legs too — this exclusion is unproven and must not be trusted
- ℹ BOT-EXCLUDED: CorrectScore — the board is TRUNCATED at the ratified 2% probability floor, so the offered scores are not an exhaustive outcome set; normalizing them would over-normalize and manufacture an edge out of the missing rows — measured reachable: random places 33,089 legs where skilled places 0, so it is DECLINED, not blocked
- ℹ BOT-EXCLUDED: WinningMargin — one-way buckets that deliberately omit the draw (margin 0), so the set is not a partition and de-vig has no denominator — measured reachable: random places 7,067 legs where skilled places 0, so it is DECLINED, not blocked
- ℹ BOT-EXCLUDED: TotalGoalsOddEven — THE most near-even market on the board: measured across the latent box under draws it prices odd 0.490–0.499 / even 0.501–0.510, i.e. odds 1.87–1.94 on both sides, and NEITHER side reaches the 3.0 longshot threshold at any sampled latent point (0 of 105). Under exact de-vig it therefore never strictly wins a tie and no owned item can lift it, so a sharp correctly declines it — the BTTS shape, and stronger. Worth recording WHY it is this balanced: every draw carries an EVEN goal total (h+h), so the old no-draws truncation was deleting even mass and had skewed parity to 64/36; restoring draws restored it to ~50/50. Expires at v2 pricing, same as BTTS — measured reachable: random places 4,651 legs where skilled places 0, so it is DECLINED, not blocked
- ℹ BOT-EXCLUDED: PlayerMultiScorer — YES-only player market on a floor-truncated board — inherits the AnytimeScorer human-agency policy, and its offered rows do not sum to the outcome space so there is nothing to de-vig against (declared policy)
- ℹ SAME-MATCH-EXCLUDED: DoubleChance — left the OFFERED SET 2026-08-24 (spec-doublechance-removal): the enum member stays so in-flight legs still grade, but no board offers it, so no same-match ticket can contain one. Not a coverage hole — unreachable by construction
- ℹ PLAYTEST-GATED: Ask for the Manager audits ≈0 through bots (Δwon +0.2±0.9pp) — RATIFIED KEEP, playtest #9

> **NOT DONE — iterate the knobs (item numbers / prices / curve) and rerun.**

## 0b. SAME MATCH exposure (informational — NOT a gate)

From the `samematch` batch. Whether the feature is covered is G7-SGP's verdict; how thinly each relation is covered is this table's, and the two are deliberately different instruments.

Tickets placed: **106,806** · settled: **71,339** · cashed out: **35,467** · legs voided and re-priced: **11,601** · refusals tripped: **34,665**

Of the **106,806** same-match tickets that reached an outcome, **66.8 %** were graded and **33.2 %** were cashed out (**403,376** banked). Cash-outs by position in the sweat: **15,060** with nothing settled · **10,406** mid-sweat · **10,001** on the last leg.

| Relation | Relations priced | Tickets carrying it | Times principal |
|---|---:|---:|---:|
| Implies | 47,280 | 28,691 | 17,076 |
| SharedScoreline | 87,055 | 46,086 | 28,036 |
| ScorerOfSide | 39,662 | 21,825 | 15,813 |
| SharedCount | 31,759 | 25,880 | 25,880 |
| Independent | 43,517 | 25,880 | 0 |

Refusal rules exercised: ImpossibleCombination × 24,664 · DuplicateSelection × 10,001 · SubEvens × 0. SubEvens reads zero at the shipped κ = 1 by construction — the sub-evens price and its full-ticket refund need κ ≳ 1.3, so that path stays unit-test-only in this campaign.

Not exercised: MutuallyExclusive. MutuallyExclusive can never appear here by construction — it is the label on a combination the engine REFUSES, so it is never on a placed ticket; the refusal counters above are where it is read. Any other name in this line is a real hole in the probe's catalogue.

| Market kind | Same-match legs | Tickets carrying it |
|---|---:|---:|
| Moneyline | 44,135 | 44,135 |
| Total Goals | 23,184 | 22,603 |
| BTTS | 28,323 | 28,323 |
| Total Corners | 65,882 | 45,881 |
| Total Cards | 15,880 | 15,880 |
| Anytime Scorer | 21,825 | 21,825 |
| DoubleChance | 0 | 0 |
| Handicap | 5,938 | 5,938 |
| TeamTotalGoals | 17,618 | 11,815 |
| CorrectScore | 10,357 | 10,357 |
| WinningMargin | 5,938 | 5,938 |
| TotalGoalsOddEven | 16,099 | 16,099 |
| TeamTotalCorners | 5,879 | 5,879 |
| TeamTotalCards | 5,879 | 5,879 |
| PlayerMultiScorer | 6,012 | 6,012 |

NOT IN ANY SAME-MATCH TICKET: DoubleChance. G7-SGP's per-kind arm is the verdict on that list — a kind is either exercised or named on the arm's exclusion list with a reason.

## 1. Survival curves

Percent of runs still alive *entering* each round (a run that dies in round R was alive entering R).

| Metric | naive | random | skilled | noshop | martyr | chalk | hoarder | ironhands | samematch | martyr-worst |
|---|---|---|---|---|---|---|---|---|---|---|
| enter R1 | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R2 | 100.0% | 99.8% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R3 | 100.0% | 72.6% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R4 | 61.7% | 32.3% | 100.0% | 100.0% | 23.5% | 100.0% | 100.0% | 100.0% | 100.0% | 100.0% |
| enter R5 | 32.4% | 15.1% | 75.9% | 59.4% | 21.1% | 68.8% | 62.5% | 73.8% | 68.7% | 27.2% |
| enter R6 | 13.6% | 7.9% | 44.9% | 19.6% | 11.3% | 34.9% | 25.4% | 42.6% | 6.6% | 27.0% |
| enter R7 | 2.7% | 3.6% | 22.7% | 1.6% | 5.7% | 16.3% | 6.8% | 23.1% | 0.0% | 14.7% |
| enter R8 | 0.5% | 1.9% | 12.6% | 0.0% | 2.8% | 9.2% | 1.9% | 12.7% | 0.0% | 8.2% |
| **won %** | **0.1%** | **0.8%** | **5.2%** | **0.0%** | **1.4%** | **4.7%** | **0.7%** | **5.3%** | **0.0%** | **4.8%** |
| **median death round** | **4** | **3** | **5** | **5** | **3** | **5** | **5** | **5** | **5** | **4** |
| mean rounds reached | 4.11 | 3.34 | 5.61 | 4.81 | 3.66 | 5.34 | 4.97 | 5.57 | 4.75 | 4.82 |
| totem fire rate | 0.0% | 4.5% | 31.3% | 0.0% | 0.0% | 19.4% | 8.8% | 20.3% | 0.0% | 0.0% |
| close-call deaths (% of deaths) | 16.5% | 15.6% | 5.3% | 5.2% | 2.7% | 6.6% | 6.2% | 5.5% | 19.6% | 2.6% |
| mean bookie gifts per run | 0.73 | 0.51 | 0.41 | 0.51 | 1.01 | 0.30 | 0.51 | 0.43 | 0.62 | 1.09 |

> Takeaway: naive dies at round 4, skilled reaches 5 — skill buys real extra survival; compare against the 3–4 / ≥7 targets above.

## 2. Market exposure

Placed legs and equal-split stake share by market kind. `mean leg EV` is the UNWEIGHTED per-leg return (converges to −vig under fair pricing — the sanity column); `stake-wtd EV` is stake-weighted and fat-tailed under compounding banks (a few monster tickets dominate it — read it as variance, not edge). Both are single-leg, before parlay multiplication, cash-outs, voids, or relic factors.

| Strategy | market | legs placed | stake share | mean leg EV | stake-wtd EV |
|---|---|---|---|---|---|
| naive | Moneyline | 81,388 | 100.0% | -4.4pp | -4.2pp |
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
| random | Moneyline | 7,081 | 0.0% | -3.9pp | -23.4pp |
| random | Total Goals | 14,053 | 11.6% | -1.1pp | +22.4pp |
| random | BTTS | 4,743 | 0.0% | -1.6pp | +31.9pp |
| random | Total Corners | 14,089 | 32.4% | -2.9pp | -50.6pp |
| random | Total Cards | 14,092 | 11.6% | -3.5pp | +112.8pp |
| random | Anytime Scorer | 0 | 0.0% | — | — |
| random | DoubleChance | 0 | 0.0% | — | — |
| random | Handicap | 9,227 | 0.0% | -4.1pp | -36.7pp |
| random | TeamTotalGoals | 18,748 | 0.0% | -3.4pp | -50.2pp |
| random | CorrectScore | 33,089 | 11.6% | +4.4pp | +1317.6pp |
| random | WinningMargin | 7,067 | 0.0% | -5.0pp | +77.3pp |
| random | TotalGoalsOddEven | 4,651 | 0.0% | -3.8pp | +50.6pp |
| random | TeamTotalCorners | 9,590 | 0.0% | -2.4pp | +59.3pp |
| random | TeamTotalCards | 9,335 | 0.0% | -3.5pp | -69.2pp |
| random | PlayerMultiScorer | 13,670 | 32.8% | +6.3pp | -100.0pp |
| skilled | Moneyline | 92,026 | 56.7% | +7.7pp | +85.8pp |
| skilled | Total Goals | 3,121 | 2.5% | +23.4pp | +36.0pp |
| skilled | BTTS | 0 | 0.0% | — | — |
| skilled | Total Corners | 2,067 | 14.8% | +22.5pp | -81.0pp |
| skilled | Total Cards | 2,488 | 14.6% | +30.2pp | +200.6pp |
| skilled | Anytime Scorer | 0 | 0.0% | — | — |
| skilled | DoubleChance | 0 | 0.0% | — | — |
| skilled | Handicap | 2,587 | 1.9% | +28.6pp | +42.5pp |
| skilled | TeamTotalGoals | 4,171 | 7.3% | +20.9pp | +90.4pp |
| skilled | CorrectScore | 0 | 0.0% | — | — |
| skilled | WinningMargin | 0 | 0.0% | — | — |
| skilled | TotalGoalsOddEven | 0 | 0.0% | — | — |
| skilled | TeamTotalCorners | 983 | 1.1% | +23.7pp | -21.1pp |
| skilled | TeamTotalCards | 1,825 | 1.2% | +20.5pp | -3.3pp |
| skilled | PlayerMultiScorer | 0 | 0.0% | — | — |
| noshop | Moneyline | 76,202 | 100.0% | -3.7pp | -4.4pp |
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
| martyr | Moneyline | 104,709 | 100.0% | -4.7pp | -0.3pp |
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
| chalk | Moneyline | 103,064 | 100.0% | +6.3pp | +8.2pp |
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
| hoarder | Moneyline | 83,233 | 100.0% | +2.1pp | +0.1pp |
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
| ironhands | Moneyline | 108,163 | 100.0% | +10.7pp | +20.2pp |
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
| samematch | Moneyline | 44,135 | 14.9% | -0.8pp | -0.1pp |
| samematch | Total Goals | 23,184 | 9.5% | -4.6pp | -4.4pp |
| samematch | BTTS | 28,323 | 11.1% | -2.6pp | -2.4pp |
| samematch | Total Corners | 65,882 | 29.4% | +0.6pp | +0.6pp |
| samematch | Total Cards | 15,880 | 7.0% | -4.5pp | -4.2pp |
| samematch | Anytime Scorer | 21,825 | 7.4% | -1.4pp | -1.8pp |
| samematch | DoubleChance | 0 | 0.0% | — | — |
| samematch | Handicap | 5,938 | 1.9% | -1.7pp | -1.9pp |
| samematch | TeamTotalGoals | 17,618 | 4.2% | -0.9pp | -1.2pp |
| samematch | CorrectScore | 10,357 | 3.2% | +2.8pp | +1.2pp |
| samematch | WinningMargin | 5,938 | 1.9% | +5.0pp | +4.4pp |
| samematch | TotalGoalsOddEven | 16,099 | 5.6% | -2.1pp | -1.1pp |
| samematch | TeamTotalCorners | 5,879 | 1.4% | +1.1pp | +0.7pp |
| samematch | TeamTotalCards | 5,879 | 1.4% | -1.0pp | -1.3pp |
| samematch | PlayerMultiScorer | 6,012 | 1.4% | +12.3pp | +11.0pp |
| martyr-worst | Moneyline | 131,312 | 100.0% | -4.8pp | -0.4pp |
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

Skilled baseline: median death 5, mean rounds 5.61, won 5.2%. Passives granted at run start; consumables refilled every round. Exposure = uses (consumables) or wound-up runs (ratchets); ±  is the paired-seed SE on Δwon. Sorted by Δ mean rounds survived.

| Item | kind | mean rounds | Δ mean | median death | won % | Δ won % (±SE) | exposure | totem fires |
|---|---|---|---|---|---|---|---|---|---|
| Free Bet Token | consumable | 6.31 | +0.7 | 6 | 3.5% | -1.7pp (±0.2) | 57,111 uses | — |
| Totem of Undying | passive | 6.29 | +0.68 | 6 | 6.2% | +1.0pp (±0.3) | — | 90.9% |
| Mulligan Slip | consumable | 6.26 | +0.65 | 6 | 13.0% | +7.8pp (±0.3) | 30,854 uses | — |
| Profit Boost | consumable | 5.95 | +0.34 | 6 | 9.4% | +4.2pp (±0.2) | 55,565 uses | — |
| Ref's Whistle | consumable | 5.87 | +0.26 | 6 | 6.8% | +1.7pp (±0.2) | 9,298 uses | — |
| Unopened Bobblehead | passive | 5.79 | +0.17 | 5 | 7.5% | +2.4pp (±0.3) | — | — |
| Bookie's Marker | consumable | 5.78 | +0.17 | 6 | 4.9% | -0.3pp (±0.2) | 19,978 uses | — |
| Longshot Larry's Photo | passive | 5.77 | +0.15 | 6 | 4.7% | -0.5pp (±0.3) | — | — |
| House Key | passive | 5.71 | +0.1 | 5 | 10.4% | +5.2pp (±0.4) | — | — |
| Bad Beat Jar | passive | 5.71 | +0.1 | 5 | 7.2% | +2.0pp (±0.3) | 8,040 wound | — |
| Double or Nothing Slip | consumable | 5.70 | +0.09 | 5 | 8.3% | +3.1pp (±0.2) | 14,920 uses | — |
| The Rake's Rebate | passive | 5.68 | +0.06 | 5 | 5.9% | +0.8pp (±0.3) | — | — |
| Scar Tissue | passive | 5.67 | +0.06 | 5 | 6.0% | +0.8pp (±0.3) | 6,179 wound | — |
| The System | passive | 5.64 | +0.03 | 5 | 6.9% | +1.7pp (±0.3) | 2,040 wound | — |
| The Collection | passive | 5.64 | +0.03 | 5 | 6.2% | +1.1pp (±0.3) | — | — |
| Comp'd Suite | passive | 5.63 | +0.02 | 5 | 5.8% | +0.6pp (±0.3) | — | — |
| Iron Hands | passive | 5.61 | 0 | 5 | 6.6% | +1.4pp (±0.3) | 4,053 wound | — |
| Ask for the Manager | consumable | 5.61 | 0 | 5 | 5.4% | +0.2pp (±0.3) | 45,567 uses | — |
| Chalk Eater | passive | 5.56 | -0.05 | 5 | 5.7% | +0.5pp (±0.3) | — | — |
| Golden Parachute | passive | 5.54 | -0.08 | 5 | 3.2% | -2.0pp (±0.3) | — | — |
| Whale Card | passive | 5.08 | -0.53 | 5 | 0.4% | -4.7pp (±0.2) | — | — |
| The Multiplier | passive | 5.00 | -0.61 | 4 | 11.8% | +6.6pp (±0.4) | — | — |

> Takeaway: the gate section's item flags carry the verdicts (DEAD / DOMINANT / totem band). Note the audit grants items FREE — organic play also pays shop prices out of payment headroom.

## 3. Variance feel

Biggest single-ticket swing per run (won payout / cash-out taken / stake lost), and final bank of winning runs.

| Strategy | swing p10 | p50 | p90 | p99 | win-bank p10 | p50 | p90 | p99 |
|---|---|---|---|---|---|---|---|---|---|
| naive | $87 | $87 | $330 | $673 | $279 | $1,393 | $2,550 | $3,441 |
| random | $83 | $137 | $509 | $5,484 | $353 | $2,516 | $748,445 | $7,082,784,535,985,252,352 |
| skilled | $57 | $112 | $798 | $8,614 | $122 | $1,647 | $22,563 | $451,842 |
| noshop | $55 | $111 | $173 | $337 | n/a | n/a | n/a | n/a |
| martyr | $87 | $87 | $872 | $6,523 | $1 | $1,235 | $15,614 | $229,111 |
| chalk | $57 | $112 | $510 | $4,905 | $99 | $1,690 | $16,546 | $89,224 |
| hoarder | $55 | $111 | $194 | $1,104 | $63 | $686 | $11,038 | $105,448 |
| ironhands | $57 | $129 | $805 | $9,191 | $111 | $1,410 | $21,716 | $238,194 |
| samematch | $22 | $38 | $88 | $205 | n/a | n/a | n/a | n/a |
| martyr-worst | $87 | $87 | $1,452 | $21,557 | $146 | $4,117 | $35,531 | $263,851 |

> Takeaway: swing p50 vs p99 shows whether the ride is flat (boring) or spiky (feels rigged); a p99 many multiples of p50 is the intended rare-blowup shape.

## 4. Ratchet telemetry (Scar Tissue)

Mean peak stacks (pp) and mean carrier burns per run — is the ratchet winding and cashing?

| Strategy | mean peak stacks | mean burns/run |
|---|---|---|
| naive | 0.0pp | 0.00 |
| random | 0.4pp | 0.00 |
| skilled | 1.9pp | 0.00 |
| noshop | 0.0pp | 0.00 |
| martyr | 5.6pp | 0.00 |
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
| R2 | -$6 | -$8 | $3 | -$3 | $9 | $3 | -$3 | $15 | -$1 | $38 |
| R3 | -$5 | -$8 | $23 | -$1 | $29 | $17 | -$0 | $22 | -$1 | $68 |
| R4 | -$5 | -$17 | $20 | -$2 | $34 | $18 | $5 | $19 | -$1 | $151 |
| R5 | -$6 | -$29 | $33 | -$3 | $73 | $39 | $6 | $34 | -$1 | $227 |
| R6 | -$6 | -$5,031 | $76 | -$4 | $111 | $68 | $7 | $89 | -$3 | $429 |
| R7 | -$11 | -$4,602 | $402 | -$4 | $230 | $141 | $50 | $339 | — | $797 |
| R8 | -$18 | -$16,139,407 | $1,813 | — | $1,085 | $302 | $174 | $728 | — | $1,929 |

- Skilled mean EV first crosses zero at **round 3** (target ≈ round 4).
- **Survivorship caveat:** the round-3+ means average only the few runs that got there (R3 n=9661 tickets vs R1 n=10000). The MEDIAN skilled run dies at round 2, so it never reaches the +EV band — the arc is real for the surviving tail, not the typical player.
- Naive mean EV never crosses zero — as intended.

> Takeaway: among survivors the +EV relics do flip the arc (crosses ~R3, target ≈R4), but almost nobody survives to bank it — the mechanic works, the economy gates it away.

## 6. Band-3 audit

Top-1% final banks (is 'sanctioned brokenness' reachable but rare?).

| Strategy | p99 final bank | top-1% mean | max |
|---|---|---|---|
| naive | $323 | $554 | $3,540 |
| random | $586 | $393,488,028,855,764,928 | $39,348,802,825,272,066,048 |
| skilled | $9,299 | $271,557 | $19,579,801 |
| noshop | $150 | $221 | $361 |
| martyr | $863 | $17,420 | $400,044 |
| chalk | $6,542 | $54,040 | $2,420,145 |
| hoarder | $492 | $4,807 | $228,306 |
| ironhands | $9,520 | $252,833 | $18,151,272 |
| samematch | $150 | $191 | $357 |
| martyr-worst | $17,262 | $78,983 | $731,510 |


Pairwise relic synergy (10,000 runs/config, baseline won 0.0%). Synergy excess = pair Δwon − (soloA Δ + soloB Δ). Top 10:

| Pair | pair won % | synergy excess (pp) | ±2 SE (paired) | tag |
|---|---|---|---|---|
| Longshot Larry's Photo + House Key | 5.1% | +2.95 | ±0.36 | superadditive — 8.2× its own error |
| Longshot Larry's Photo + Whale Card | 4.4% | +2.27 | ±0.32 | superadditive — 7.1× its own error |
| The Multiplier + House Key | 3.3% | +1.64 | ±0.26 | superadditive — 6.4× its own error |
| The Multiplier + Whale Card | 3.2% | +1.63 | ±0.25 | superadditive — 6.4× its own error |
| The Multiplier + Bad Beat Jar | 2.5% | +0.89 | ±0.19 | marginal — 4.6× its own error |
| Longshot Larry's Photo + Bad Beat Jar | 3.0% | +0.86 | ±0.22 | marginal — 3.9× its own error |
| Whale Card + House Key | 0.8% | +0.81 | ±0.18 | marginal — 4.5× its own error |
| Longshot Larry's Photo + The System | 2.9% | +0.74 | ±0.21 | marginal — 3.6× its own error |
| Totem of Undying + Longshot Larry's Photo | 2.8% | +0.64 | ±0.16 | marginal — 3.9× its own error |
| House Key + The System | 0.5% | +0.55 | ±0.16 | marginal — 3.5× its own error |

Ranked by excess; the ±2 SE column is paired by seed, so it is the error of the *combination*, not of any one arm. A row whose excess is inside its own error is tagged as such and its rank means nothing.

> Takeaway: brokenness should live in the tail (max ≫ p99) and, if combos ran, in a few pairs that clear their own error rather than across the whole table.

## 7. Grind metric

Median decisions per round (tickets + cash-outs + buys). Fewer decisions late than mid = flat repetition.

| Round | naive | random | skilled | noshop | martyr | chalk | hoarder | ironhands | samematch | martyr-worst |
|---|---|---|---|---|---|---|---|---|---|---|
| R1 | 1 | 3 | 2 | 1 | 3 | 3 | 1 | 2 | 4 | 3 |
| R2 | 1 | 3 | 3 | 1 | 3 | 4 | 1 | 4 | 5 | 3 |
| R3 | 1 | 3 | 3 | 1 | 0 | 3 | 3 | 4 | 5 | 3 |
| R4 | 1 | 2 | 3 | 1 | 3 | 2 | 2 | 3 | 3 | 0 |
| R5 | 1 | 3 | 2 | 1 | 2 | 2 | 1 | 2 | 0 | 3 |
| R6 | 1 | 3 | 2 | 1 | 1 | 2 | 1 | 2 | 0 | 1 |
| R7 | 1 | 3 | 3 | 1 | 1 | 3 | 2 | 3 | 0 | 2 |
| R8 | 1 | 3 | 2 | — | 1 | 2 | 2 | 2 | — | 2 |

> Takeaway: late rounds carry FEWER decisions than mid for: skilled, martyr, hoarder, ironhands, samematch, martyr-worst — repetition-risk flag.

