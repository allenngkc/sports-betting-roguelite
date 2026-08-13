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
- **Correlated parlays (same match):** the naive product is not the ticket's probability. It is wrong in *both* directions and is sometimes exactly zero. Closed 2026-08-12 — see **Same-game tickets** below. The note that stood here proposed a copula-lite shared "momentum" latent; that is obsolete. The sim's match model is already an exact finite joint, so the answer is enumerated, not approximated — and the "book prices them independently, so it's a free exploit" framing died with it, because our book can price them correctly and does.
- Arbitrage across two books: guaranteed profit when `1/o_A(book1) + 1/o_B(book2) < 1`; stake split `s_i ∝ 1/o_i`.
- Hedge of a live parlay: with one leg left at live odds `o_live` for the *opposite* outcome, staking `h` on the hedge locks a band between outcomes — the "take guaranteed money" tool the player buys access to.

## Same-game tickets — the correlation model

Designed 2026-08-12 (sgp lane, F_0.6.0) on measured evidence, not judgement: `docs/sgp/correlation-recon.md` (exact reconnaissance, verification gate at 2.554e-15 over 437,832 checks), `docs/sgp/model-candidates.md` (margin-method literature), `docs/sgp/research-sgp-pricing.md` (real-book practice).

Two legs on one matchup are correlated, so `Π p_i` is not the ticket's probability. This is not a matter of accuracy. On the shipped board, **22 two-leg shapes and 57 further three-leg shapes have joint probability of exactly zero** — tickets with no winning outcome, which the naive product would sell at finite odds up to a mean decimal of 2070.70. That is why the one-leg-per-match guard exists, and lifting it requires everything below.

### The joint is computed, not modelled

The sim's match model is already a finite joint distribution, so the true joint probability of any set of selections is exact. No copula, no latent factor, no simulation.

Selections partition into three families — **GOAL** (moneyline, total goals, BTTS, anytime scorer), **CORNER**, **CARD**. Corner and card draws are independent of the score draw and of each other, measured at `max |ρ − 1| = 4.4e-14` across 3.94M pairs, so the joint factorizes:

```
p_joint = p_GOAL × p_CORNER × p_CARD
```

**Goal family** — one pass over the winner-conditioned score enumeration:

```
p_GOAL = Σ_{w ∈ {H,A}} P(w) · Σ_{(h,a) ∈ S_w} P(h,a | w) · 1[non-scorer goal predicates hold] · Π_t Q_t(g_t)
```

**Scorer term** — for `k` backed players on team `t` holding roster-normalized weights `w_1..w_k`, against `g` goals by that team:

```
Q_t(g) = 0                                             if g < k
Q_t(g) = Σ_{S ⊆ {1..k}} (−1)^{|S|} (1 − Σ_{i∈S} w_i)^g  otherwise
```

The `g < k → 0` case is **normative, not an optimization**. The sum cancels to ≈1e-17 rather than 0 in IEEE double, which turns a structurally impossible ticket — two players both scoring inside one goal — into a vanishingly small *positive* probability that passes every zero-check. Twelve impossible triple shapes were misclassified exactly this way before the guard existed.

**Count families** — corners, and cards identically:

```
p_CORNER = Σ_{c_h} Σ_{c_a} P(c_h) · P(c_a) · 1[corner predicates hold]
```

### The correlation ratio

```
ρ = p_joint / Π p_i
```

`ρ` is a pure property of the joint distribution and is **independent of the overround**. Measured range on the shipped board: `[0, 3.11]` at two legs, `[0, 11.88]` at three, `[0, 14.82]` at four. At two legs, 51.4% of combinations are exactly independent (`ρ = 1`), 3.49% are impossible (`ρ = 0`), and 3.49% are logical implications where one leg strictly implies the other.

### Pricing

```
o_sgp = 1 / (p_joint × κ × (1 + Ω)^n)
```

Proportional margin — the same operation the book already applies to every single leg. The sophisticated alternatives are *unavailable*, not merely unchosen: Shin, the power method and the odds-ratio method each solve for a parameter across a **complete book** whose implied probabilities sum to `1 + Ω`, and a parlay price has no such book. Forced onto a synthetic "hits vs. misses" pair, Shin collapses to the additive method, which needs `p ≥ Ω/2` for the miss side to be a probability at all — below 2.5% at our configured margin it has no valid price to offer, and 3–4 leg tickets live below 2.5% routinely.

**Vig compounds per leg exactly as it does for an independent parlay** — that is the `(1 + Ω)^n` term. `κ ≥ 1` is the SGP margin dial, default **1.0**, tuned by the gate campaign.

At `κ = 1` this yields the design's central property:

```
EV per unit stake = p_joint × o_sgp − 1 = (1 + Ω)^{−n} − 1
```

**Identical to an independent n-leg parlay.** Correlation changes the odds shown; it never changes the house edge. Same-game and cross-game tickets are EV-equivalent, so no arbitrage exists between the two products and the "parlays are mathematically terrible" tension survives intact.

It also means the price departs from the board's product **only where correlation actually exists**:

```
o_sgp / Π o_i = 1 / (κ · ρ)
```

At `κ = 1` an independent combination prices *identically* to the legs multiplied off the board — 51.4% of two-leg tickets — while a positively-correlated one pays less and a negatively-correlated one pays more. (`correlation-recon.md` §8 tabulates this ratio under the alternative rule of charging margin once, where it is `(1+Ω)^{n−1}/ρ`; under the rule adopted here it is `1/(κρ)`.)

### Impossible combinations are blocked, not priced

`p_joint = 0` has no finite price. These combinations are **rejected at slip construction** — that is what replaces the one-leg-per-match guard, rather than the guard simply being deleted.

Logical implications (`p_joint = min p_i`, one leg strictly implying another) are **not** blocked: the joint prices them correctly and automatically. The player pays two legs of vig for one leg of risk, which is a bad bet rather than a broken one. Whether the interface should discourage them is a presentation question, not a math one.

Two impossible shapes — `BTTS YES + Under 2.5`, and `Under 2.5 ⊂ BTTS NO` — exist *only* because draws are unrepresentable in v1. They are artefacts of that constraint, not of football, and need revisiting if draws are ever added. (OPEN, coupled to the no-draws decision.)

### Void: re-price on the survivors

When a leg voids, the ticket re-prices against the surviving legs' joint:

```
o_sgp' = 1 / (p_joint(surviving legs) × κ × (1 + Ω)^{n−1})
```

Dropping a voided leg's factor out of a product — today's behaviour — is **wrong under a joint price, and is what no real book does**. Real books split between re-pricing on the remainder and voiding the whole ticket. Re-pricing is chosen here because the price was a statement about a joint event: remove a leg and the event itself has changed.

Void-replacement prices are **computed and locked at ticket lock**, one per single-void scenario, never re-derived at settlement. That keeps settlement deterministic and independent of when a void is discovered. (Multiple simultaneous voids: OPEN — the one documented commercial mechanism covers a single void only.)

## Generalized payoff functions

The four numbers parameterize the *simplest* bet: pay `s`, receive `s·o` on win, `0` otherwise. Formally a bet is a mapping from outcome-space to cash flows, and relics may rewrite that mapping. Two canonical examples (Allen, 2026-07-07):

- **Per-leg payout relic ("Early Payout" shape):** each hitting leg pays `b_i` immediately while the full parlay payout is retained. Variant 1 — any hitting leg pays regardless of others: `EV_bonus = Σ b_i·p_i`. Variant 2 — legs pay sequentially until one dies: `EV_bonus = Σ b_i·Π_{j≤i} p_j`. Variant 2 is cheaper, and matches the serial sweat. (Variant choice: OPEN)
- **Vig rebate engine ("Rakeback" shape):** accrues `k ×` cumulative vig paid into a piggy bank, redeemable on a trigger. At `k = 2.0`, net EV of churning volume is `+V` where `V` is vig paid — betting volume itself becomes profitable, deliberately creating the *rakeback grinder* archetype. `k` and the redemption trigger are sim-tuning targets.

Rule that keeps this sane: any payoff rewrite must declare its cash flows at the same hooks the engine already fires, and `/sim` must be able to compute its EV. No mechanic whose value can't be audited.

### Vig accounting (first-class engine stat)

Charged at ticket lock: `vig_paid = s × (1 − o_offered / o_fair)` where `o_fair = 1/p_book` (the book's no-vig line). Equals the EV haircut relative to fair odds; compounds naturally across parlay legs. Tracked per run regardless of relics — "total vig paid" is an end-of-run stat with satirical value on its own.

For a same-game ticket the no-vig line is the joint, `o_fair = 1/p_joint`, so the whole ticket's haircut collapses to `vig_paid = s × (1 − 1/(κ(1 + Ω)^n))` — the same figure an independent n-leg parlay pays at `κ = 1`. The stat stays comparable across both products, which is the point of tracking it.

## Cash-out pricing

The live cash-out offer on a partially resolved parlay:

```
fair_value = s × (Π o_resolved_legs) × (Π p_j × o_j, remaining legs, live)
offer      = fair_value × (1 − cashout_margin)
```

(Corrected 2026-07-07: original draft omitted the remaining legs' odds — with zero legs resolved it priced the ticket at `s × Πp` instead of `Πp × payout`. Sanity anchors: no legs resolved → `fair = P(win) × payout`; all legs resolved → `fair = payout`. Both are unit tests in the engine.)

**Same-game cash-out (2026-08-12).** The formula above assumes the remaining legs are independent. On a same-game ticket that fails twice over: the surviving legs are correlated with each other, *and* every settled leg is information that shifts the survivors' probabilities. The general form both cases share:

```
fair_value = payout × P(every unsettled leg hits | revealed match state)
```

with `payout = s × o_sgp` for a same-game ticket. The independent case recovers the formula above exactly, since `P(…)` factorizes to `Π p_j` there — and both sanity anchors survive unchanged. The conditional is computed by restricting the same enumeration to outcomes consistent with what the match has already revealed.

This is materially harder than pre-match pricing and is the most expensive thing this model implies. Real books commonly dodge it by excluding same-game tickets from cash-out altogether. We do not — cash-out is a core mechanic here, so it conditions. (Implementation cost: OPEN, sized in the step-2 plan rather than here.)

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
8. Same-game economy: with same-game tickets enabled, does the EV arc still cross where it should, and what value of the margin dial `κ` holds it there? Ticket pricing drives run economy, so this is a gate-campaign question, not a spot check — and the market-coverage gate has to exercise a same-game ticket, or the whole feature is invisible to the campaign.

## Open questions

- Odds format shown to player: American (-110), decimal, or fractional? (American is the meme-native format; decimal is readable. Maybe a toggle, default American for flavor.)
- Should true `p` ever be fully visible, or always a confidence interval even at max information? (Leaning: interval — preserves sweat.)
- Pity/anti-frustration systems: bad-beat insurance as an item rather than a hidden system?
