# PRD — Prototype v0 ("Is the sweat fun in text?")

**Status:** SIGNED OFF 2026-07-07 (Allen). Phase 1 active. Only §8 numbers are amendable before the Week 6 verdict.
**Timebox:** 5–6 weeks part-time. If it runs past 8, we cut scope, not extend time.
**Prime directive:** everything not in this document is *parked, not lost* (it lives in `design/03` and `OPEN-QUESTIONS.md`). The prototype's job is to answer one question, not to be the game.

---

## 1. Hypothesis under test

> Building a parlay under profit-target pressure, then sweating it leg by leg with a live cash-out offer, is tense and re-playable **even as plain text**, and relics create meaningfully different runs.

If this is false with juice stripped away, no amount of VFX saves the game and we kill or pivot cheaply.

## 2. Success / kill criteria (evaluated after tuning week)

| # | Criterion | Pass |
|---|---|---|
| S1 | Allen voluntarily plays full runs after the novelty wears off | ≥10 complete runs, self-motivated |
| S2 | The cash-out decision produces genuine tension | ≥1 "hovered over the button" moment per run, honestly self-reported |
| S3 | Sim: naive strategy (bet favorites, never cash out, no relics) dies early | median death round 3–4 |
| S4 | Sim: skilled play (relic-informed, disciplined cash-out) survives deep | median death round 7+ |
| S5 | Relics change behavior | at least 5 of 10 relics visibly alter how Allen plays a round |

Kill signal: S1 or S2 fails after two tuning passes → stop, write post-mortem, re-examine at design level before any Unity work.

Also capture during evaluation: the run # at which the sweat first feels repetitive. v0 is cash-out-only by design; this datum feeds the mid-sweat agency ladder (`design/04`) for the full game.

## 3. One round, played (the spec-by-example)

```
ROUND 3 — Bank: $1,340 / Target: $1,900          Seed: 8F3K-22
────────────────────────────────────────────────────────────
SLATE (6 matchups, moneyline only)
 1. Yams (7-2)      -145  vs  Startups (4-5)   +125
 2. Mallards (3-6)  +210  vs  Bricklayers (8-1) -260
 ...
[Tout Sheet reveals: Matchup 2 true win% Bricklayers 68–78%]

BUILD TICKET 1 of 3   Stake: $200
 Leg 1: Bricklayers -260   Leg 2: Yams -145   Leg 3: Longhaulers +180
 Potential payout: $1,742        [LOCK IT IN]

THE SWEAT — Leg 2/3: Yams vs Startups
 Q3 ... Startups intercept! Yams 17-14                    win% 61% ▼
 CASH OUT NOW: $412                                   [C] to take it
 Q4 ... Yams grind the clock ... FINAL: Yams 24-14        LEG GREEN ✔
 [Early Payout pays $30]
 Leg 3/3 ... (final leg gets 2× drama events)
────────────────────────────────────────────────────────────
SETTLE: Ticket 1 GREEN +$1,742 · Bank $2,882 ≥ $1,900 ✔ → SHOP
SHOP: [Mulligan $250] [High Roller $300] [Sharp Eye $200]  slots 2/5
```

If this screen loop reads fun to you, the PRD is aimed right. If not, say so before we build it.

## 4. In scope (functional requirements)

- **F1 — Run structure:** 8 rounds, one betting window per round, up to 3 tickets per window, bank must meet the round target at settle or the run ends. No meta progression; run seed shown at start and end.
- **F2 — Slate generation:** 6 matchups/round, one fictional sport, procedurally named teams with W-L records. True `p` per side drawn from [0.25, 0.75]; records are a *noisy* signal of `p` (the only free information).
- **F3 — Odds engine:** one book, two-way moneyline, book prices at true `p` + fixed 5% overround (book is sharp; no exploitable mispricing except via relics). American odds display. Vig tracked per ticket at lock (formula in `design/02`).
- **F4 — Tickets:** singles to 6-leg parlays, independent legs (no correlation model in v0), stake $10 minimum, payout = stake × Π odds.
- **F5 — The sweat (text drama generator v0):** legs resolve serially; each leg = 5–10 timed ticker events (~0.5s cadence) with live win% drifting per event, honest conditional math; outcome sampled before presentation; final leg of each ticket gets 2× event budget and a late decisive moment. Fast-forward exists but is gated off during the final leg.
- **F6 — Cash-out:** live offer on every multi-leg ticket, `fair value × (1 − 8% margin)`, updated every event, one-key accept, prices the full remaining payoff function (incl. Early Payout future partials).
- **F7 — Shop:** after each survived round, 3 relics offered from the pool of 10, priced $150–$400 paid from bankroll (deliberate tension: power vs target headroom), max 5 relic slots, no rerolls in v0.
- **F8 — Relics:** exactly the 10 below, implemented as data (JSON) + effect ops on engine hooks — the effect system IS part of what v0 validates.
- **F9 — `/sim` Monte Carlo harness:** headless runner with pluggable bot strategies (naive / random / relic-aware) producing survival curves, relic power audit (marginal win-rate per relic), per-run variance stats, and the **EV-arc report** (round where per-ticket EV crosses zero, per strategy — target: skilled ≈ round 4, naive never; see economy doctrine in `design/02`). Answers S3–S5.
- **F10 — Determinism:** named RNG streams (`outcomes, drama, slate, shop`); identical seed → identical run; one golden-seed regression test.

## 5. The 10 relics

| Relic | Axis | Hook(s) | Effect (v0 numbers — sim will retune) |
|---|---|---|---|
| Tout Sheet | Info | OnSlateGenerated | Reveals true win% ±5pp for 2 matchups/round |
| Sharp Eye | Info | OnOddsOffered | Reveals the exact true win% of one chosen line per round (redesigned 2026-07-08: the original "+EV flag" is always false against a vig-priced book — dead content; see DECISIONS.md) |
| Boosted Odds | Odds | OnBetComposed | +15% decimal odds on each ticket's first leg |
| Promo Code | Odds | OnTicketLocked | First ticket each round is priced at fair odds (vig = 0) |
| High Roller | Capital | OnBetComposed | Stake ≥ half your bank → that ticket's payout +15% (redesigned 2026-07-08 with the stake-cap removal; see DECISIONS.md) |
| Bankroll Insurance | Capital | OnTicketSettled | First busted ticket each round refunds 50% of stake |
| Mulligan | Resolution | OnLegResolved | Once/round: void a dead leg; ticket degrades to remaining legs, payout recomputed |
| Lucky Charm | Resolution | OnLegStarted | +3pp true win% on every ticket's final leg |
| **Early Payout** | Payoff structure | OnLegResolved | Sequential variant: each green leg pays 15% of stake immediately; full parlay payout retained |
| **Piggy Bank** | Accounting | OnTicketLocked / OnTicketSettled | Accrues 2× vig paid; auto-smashes (pays out) when a ticket busts |

Spread check: 2 info, 2 odds, 2 capital, 2 resolution, 1 payoff-structure, 1 accounting — every axis represented, both of Allen's relics in.

## 6. Explicitly OUT of v0 (parked, with home)

Gurus & insider tips (`design/03` Axis 1) · multiple books, arbitrage, line shopping, limiting (Axis 2, `01`) · hedging & live betting (`04`) · correlated parlays (`02`) · loans/debt (Axis 3) · events between rounds · meta progression/unlocks · Unity, art, sound, all juice (`06` — Phase 2) · second/third sports · difficulty tiers · save system (runs are ~30 min; v0 runs complete in one sitting).

Cutting gurus hurts most — they're our differentiation — but they're an *addition* to a loop that must first exist. They headline Phase 2/3 scope.

## 7. Technical scope

Per `design/05`, all three projects get born, minimally:

- `/engine` — netstandard2.1 class library, zero Unity refs: run state machine, slate/odds, ticket math, drama generator v0 (emits event stream as data), effect system + 10 relics as JSON, RNG streams. Unit tests: exhaustive on odds/EV/parlay/cash-out math, golden-seed on drama.
- `/game-console` — thin .NET console app: renders event streams with timing, keyboard input, ANSI color. Disposable by design; Unity replaces it in Phase 2. (Named `game-console`, not `/game`, so nobody gets attached.)
- `/sim` — console runner: N-thousand runs per strategy, prints the S3–S5 report as markdown.

Definition of done, technical: `dotnet test` green; `sim --runs 10000 --strategy naive` completes < 5 min; a full seeded run is reproducible.

## 8. Tuning defaults (v0 starting values — the sim's job is to move these)

Starting bank $500 · targets [800, 1200, 1900, 3000, 4800, 7800, 12500, 20000] (≈ ×1.6) · overround 5% flat · cash-out margin 8% · min stake $10, stakes uncapped up to the whole bank (cap lifted 2026-07-08 after playtest #1; High Roller redesigned to an all-in payout bonus) · shop prices $150–400 · 6 matchups/round · 3 tickets/round max · 5 relic slots.

## 9. Milestones

| Week | Deliverable | Checkpoint |
|---|---|---|
| 1 | `/engine` core: state machine, slate, odds, ticket math + tests | math tests green |
| 2 | Drama generator v0 + cash-out pricing + determinism | golden seed replays |
| 3 | Effect system + 10 relics + shop | all relics EV-audited by hand |
| 4 | `/game-console` playable end-to-end | Allen plays run #1 |
| 5 | `/sim` harness + strategy bots | S3/S4 report exists |
| 6 | Tuning passes vs S-criteria; verdict | **kill/continue decision, logged** |

## 10. Risks

- **Text sweat undersells the design** → mitigation: cadence/timing IS in scope (F5); a sweat with no timing is a spreadsheet. If S2 fails, test once more with minimal sound (a tick + a slam) before ruling.
- **Scope creep from the idea flood** → mitigation: this document. New ideas go to `OPEN-QUESTIONS.md`, not into v0. The only amendable section pre-verdict is §8 numbers.
- **Effect-system overengineering** → mitigation: the 10 relics are the spec; build exactly the ops they need, nothing speculative.
