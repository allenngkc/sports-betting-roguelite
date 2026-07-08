# 02 — Betting Math

The math core. Everything here must end up as pure, unit-tested C# in the headless engine, and every claim here gets validated by Monte Carlo before we trust it.

## The four-number model

Every bet is exactly:

| Symbol | Meaning | Who controls it |
|---|---|---|
| `p` | True win probability of the outcome | The sim generates it; **information mechanics reveal it**; some relics *change* it |
| `o` | Offered (decimal) odds | The book sets it with vig; **odds mechanics improve it** |
| `s` | Stake | Player, within limits; **capital mechanics expand it** |
| `payout` | `s × o` on win | **Payout mechanics multiply it** |

Design law (Pillar 3): every relic/guru/event manipulates at least one of these, and its tooltip should make which one obvious.

## Core formulas

- Implied probability of offered odds: `q = 1/o`
- Vig: for a two-way market, `(1/o_A + 1/o_B) − 1 > 0`. Vig is the house edge and our master difficulty dial.
- Expected value: `EV = p × s × (o − 1) − (1 − p) × s`. Positive when `p > 1/o` — that is the entire meaning of "+EV" and the game teaches it by *showing* p when the player has earned the information.
- Parlay (independent legs): `p_parlay = Π p_i`, `o_parlay = Π o_i`. Vig compounds per leg — parlays are mathematically terrible, which is the joke and the tension.
- **Correlated parlays:** if leg outcomes correlate (same match, same storyline), true joint probability exceeds the naive product while the book prices them independently — a real-world exploit that should be a discoverable mechanic. Needs a correlation model in the sim (copula-lite: shared latent "momentum" factor per match/team is probably enough). (OPEN)
- Arbitrage across two books: guaranteed profit when `1/o_A(book1) + 1/o_B(book2) < 1`; stake split `s_i ∝ 1/o_i`.
- Hedge of a live parlay: with one leg left at live odds `o_live` for the *opposite* outcome, staking `h` on the hedge locks a band between outcomes — the "take guaranteed money" tool the player buys access to.

## Generalized payoff functions

The four numbers parameterize the *simplest* bet: pay `s`, receive `s·o` on win, `0` otherwise. Formally a bet is a mapping from outcome-space to cash flows, and relics may rewrite that mapping. Two canonical examples (Allen, 2026-07-07):

- **Per-leg payout relic ("Early Payout" shape):** each hitting leg pays `b_i` immediately while the full parlay payout is retained. Variant 1 — any hitting leg pays regardless of others: `EV_bonus = Σ b_i·p_i`. Variant 2 — legs pay sequentially until one dies: `EV_bonus = Σ b_i·Π_{j≤i} p_j`. Variant 2 is cheaper, and matches the serial sweat. (Variant choice: OPEN)
- **Vig rebate engine ("Rakeback" shape):** accrues `k ×` cumulative vig paid into a piggy bank, redeemable on a trigger. At `k = 2.0`, net EV of churning volume is `+V` where `V` is vig paid — betting volume itself becomes profitable, deliberately creating the *rakeback grinder* archetype. `k` and the redemption trigger are sim-tuning targets.

Rule that keeps this sane: any payoff rewrite must declare its cash flows at the same hooks the engine already fires, and `/sim` must be able to compute its EV. No mechanic whose value can't be audited.

### Vig accounting (first-class engine stat)

Charged at ticket lock: `vig_paid = s × (1 − o_offered / o_fair)` where `o_fair = 1/p_book` (the book's no-vig line). Equals the EV haircut relative to fair odds; compounds naturally across parlay legs. Tracked per run regardless of relics — "total vig paid" is an end-of-run stat with satirical value on its own.

## Cash-out pricing

The live cash-out offer on a partially resolved parlay:

```
fair_value = s × (Π o_resolved_legs) × (Π p_j × o_j, remaining legs, live)
offer      = fair_value × (1 − cashout_margin)
```

(Corrected 2026-07-07: original draft omitted the remaining legs' odds — with zero legs resolved it priced the ticket at `s × Πp` instead of `Πp × payout`. Sanity anchors: no legs resolved → `fair = P(win) × payout`; all legs resolved → `fair = payout`. Both are unit tests in the engine.)

Interaction rule (found via the per-leg payout relic): cashing out kills the ticket, so `fair_value` must include the EV of any *future* payoff-function cash flows (e.g., unresolved per-leg partials), while partials already paid are kept and excluded. Otherwise cash-out is systematically underpriced for payoff-rewriting relics and the two mechanics silently anti-synergize. General principle: cash-out prices the ticket's full remaining payoff function, whatever a relic has made it.

`cashout_margin` is a design dial (real books use ~5–10%). Relics can shrink it, or briefly make it negative (a "mispriced cash-out" exploit item). Live `p` drifts during the sweat as in-match events fire — that drift is what makes the offer tick up and down on screen.

## Economy doctrine: the EV arc (Allen's direction, formalized 2026-07-07)

A run's narrative is the player's per-ticket EV crossing from negative to positive to absurd. Parlay randomness is the medium; **agency is the trajectory** — information narrows `p` uncertainty, composition picks the legs, timing is the cash-out, and the ticket portfolio manages variance.

| Band | Rounds (of 8) | Player identity | EV state | Balance policy |
|---|---|---|---|---|
| 1. Scarcity | ~1–3 | a mark | ≈ −vig | **tight** — every dollar contested |
| 2. Engine-building | ~3–6 | an operator | crossing zero | loose — variance welcomed |
| 3. Sanctioned brokenness | ~6–8+ | rigged the system | absurdly positive | **uncapped by design** — gate *when* it's reachable, never *how high* |

Design target (tunable): median *skilled* player crosses EV zero ≈ round 4; naive play never crosses. Band 3 is the dark-comedy inversion — IRL the book rigs you; our endgame is rigging it back. Anti-grind principle: layered progression, and across-run unlocks add **breadth, not power** (lean — prestige-as-power would flatten the arc by starting runs in Band 2, deleting the scarcity tension; final call OPEN).

Synergy audit policy: loops are the point. The audit *classifies*, not kills — a loop is **delicious** if hard to assemble and bounded by run length; **degenerate** only if trivially assembled or if it removes decisions.

## What Monte Carlo must answer before we build UI

1. Run-survival curve: with no relics, what % of naive-strategy runs clear round N? (Tune targets so unaided play dies ~round 3–4.)
2. Relic power audit: each relic's marginal effect on survival — catches degenerate combos and dead items.
3. Variance feel: distribution of biggest single-ticket swing per run. Too flat = boring; too spiky = feels rigged.
4. Ratchet tuning: does the limiting mechanic create the intended "diversify or die" fork, or just frustration?
5. EV-arc verification: distribution of the round where per-ticket EV crosses zero, per strategy bot. Skilled ≈ round 4; naive never.
6. Band-3 audit: top-1% run payout magnitude (brokenness must be *reachable* but rare), plus an automated relic-combo scan (pairs/triples) classifying loops as delicious vs degenerate per the doctrine above.
7. Grind metric: median minutes and decisions per round in late game — flat repetition without new decisions = grind flag.

## Open questions

- Odds format shown to player: American (-110), decimal, or fractional? (American is the meme-native format; decimal is readable. Maybe a toggle, default American for flavor.)
- Should true `p` ever be fully visible, or always a confidence interval even at max information? (Leaning: interval — preserves sweat.)
- Pity/anti-frustration systems: bad-beat insurance as an item rather than a hidden system?
