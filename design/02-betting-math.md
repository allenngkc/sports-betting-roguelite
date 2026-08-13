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

**Goal family** — one pass over the scoreline distribution:

```
p_GOAL = Σ_{(h,a)} P(h,a) · 1[non-scorer goal predicates hold] · Π_t Q_t(g_t)
```

`P(h,a)` is the model's unconditional distribution over scorelines. The engine *constructs* it by drawing an outcome class and then a conditional score, `P(h,a) = Σ_{w ∈ W} P(w) · P(h,a | w)`. **Write that sum over `W`, never over a hard-coded pair of branches.** `W` is `{home, away}` in the pre-draws model and `{home, draw, away}` once draws land (Lane 1, greenlit 2026-08-12 — see *Pending: draws* below). Nothing else in this model depends on which it is, and a partition-agnostic sum costs nothing to write.

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

**`ρ` is a diagnostic, not an interface.** It is what the audit and the reconnaissance report. It is *not* what the model hands downstream — see the next section, where shipping it as a bare scalar is a named and prohibited failure.

### What the model emits: a joint probability *and* a relation label

**Binding constraint (S73, batch 45, canon).** The ticket is its own instrument. The surface never shows a product-of-legs or an adjustment line; the relationship is marked as an annotation. So the model must emit, beside its joint probability, a **relation label** — structured data carrying enough for presentation to compose a *sentence*, never a formula. And:

> **Where the model finds correlation it cannot label, the price does not move.**

The prohibited implementation is `p_joint` collapsed to a bare scalar `ρ`: that leaves step 5 holding a price it cannot explain, and the rework lands back here. **The model's output type is `(p_joint, relations[])`, not a number.**

**The relation vocabulary.** Correlation on this board is not diffuse — it arises from a closed set of structural causes, which is why this constraint is satisfiable rather than aspirational:

| Relation | Arises when | The sentence it must support |
|---|---|---|
| `MutuallyExclusive` | `p_joint = 0` | these cannot both happen |
| `Implies(a → b)` | `p_joint = min p_i`; one leg strictly entails another | b has already happened whenever a does |
| `SharedScoreline(reinforcing \| opposing)` | two GOAL-family legs read the same scoreline | one makes the other likelier / less likely |
| `SharedCount(family, sign)` | two legs of the same COUNT family read the same corner or card draw | one makes the other likelier / less likely |
| `ScorerOfSide(side)` | a scorer leg beside a leg on that team's goals | the same goals settle both |
| `Independent` | legs drawn from different families | unrelated — no adjustment |

**`SharedCount` was a hole in this table, found in build (2026-08-12) and ratified here.** The board ships three corner lines and three card lines, so a *band* — corners `OVER 8.5` with `UNDER 10.5` — is correlated, is not an implication, and is not impossible. It had no label, and under this section's own no-label fallback it would have priced at the naive product. Corner×corner `ρ` reaches 4.13, so that was a real leak, not a rounding-scale one. Six pair shapes per matchup.

**Resolution rules, so classification is total and deterministic:**

- **One relation per pair, most specific wins.** Every GOAL leg technically reads both teams' goals, so a scorer beside a goal leg satisfies both `ScorerOfSide` and `SharedScoreline`; the specific one is emitted.
- **Two scorers on opposite teams are `SharedScoreline`, not `ScorerOfSide`.** Given the scoreline they are conditionally independent — their dependence runs entirely through the shared score, which is what `SharedScoreline` names.
- **A ticket can be impossible without any impossible pair.** Some three-leg shapes reach `p_joint = 0` while every sub-pair is positive, so exclusion must also be expressible at ticket level, spanning all legs. A purely pairwise classifier would emit no exclusion label for them at all.

Presentation composes the words; **the model never emits English.** That seam keeps copy authority with the Design Director and pricing authority here.

**The two canon names, and their authoring boundary (Allen, batch 48, e729235).** The instrument is **SAME MATCH** — uppercase market vocabulary, untracked. The mark is **THE HOUSE'S LINE**.

**The mark is drawn, not captioned.** The oxide line carries no label and the name is never printed beside it. `THE HOUSE'S LINE` appears only in rules copy, the ledger, and first-encounter. Where the slip needs a *statement*, it states the **relation** — a sentence composed from the vocabulary above, once per slip.

That last clause binds this model, not presentation. **A slip states one relation, so the model must nominate which one.** A four-leg same-match ticket carries up to six pairwise relations, and choosing between them is a claim about what moved the price — which only the pricing layer is in a position to make. So the output contract is:

```
(p_joint, relations[], principal)
```

`principal` is the labelable relation carrying the largest `|ln ρ_pair|` — the one doing the most work on the price — with ties broken by precedence `Implies > ScorerOfSide > SharedScoreline > SharedCount`. `Independent` is never principal (there is nothing to state), and `MutuallyExclusive` never reaches a slip at all, because it is a rejection rather than a price. Leaving this choice to presentation would make presentation assert a pricing claim it has no basis for — precisely the failure S73 exists to prevent.

**The no-label fallback, stated precisely.** A ticket prices on its exact joint only if *every* correlated relation it carries resolves to a label. If any does not, the whole ticket prices at `Π p_i`. Partial per-relation application is not offered: a joint probability is not a product of pairwise adjustments and cannot be half-applied honestly.

**One carve-out, and it is load-bearing.** The `p_joint = 0` check is a **validity test, not a price movement**, and is never subject to the fallback. Without this carve-out an unlabelable zero would fall through to the naive product and sell an impossible ticket — reintroducing exactly the defect this model exists to remove. Validity runs first; labelling governs pricing only.

**Totality, and why the fallback must be instrumented.** Every correlated combination in the shipped vocabulary falls into the table above, so the fallback should never fire in v1 — it exists to keep vocabulary growth safe, not as a live path. But if it ever does fire on a positively-correlated ticket, the player silently collects the naive-product edge, up to +274% EV on an implication pair. **A silent fallback is a money leak.** It must be counted and surfaced by the gate campaign, not merely logged.

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

**Two further validity rules, both found in build 2026-08-12 and ruled here.** Lifting the guard let a ticket carry the *same selection more than once*, which the old guard had made unreachable:

- **A selection may not appear twice on a ticket.** The joint is idempotent — repeating a leg adds no risk — while `(1 + Ω)^n` charges a full extra leg of margin for it. It is the degenerate case of `Implies`: a leg entails itself. Real books do not accept it either.
- **A ticket priced at or below evens is refused**, with a reason, exactly as `MatchModel.Offer` already refuses a single market that prices ≤ 1.0. Four repeats of `UNDER 5.5 CARDS` priced at **0.9664** — pay one, win less than one.

Both are needed, and the second does not subsume the first. Two repeats of a *long* leg stay comfortably above evens and so pass the price check while remaining a pure ripoff: double margin for identical risk. The duplicate rule catches those; the evens rule is the backstop for anything else that drifts above `p_joint > 1/(κ(1+Ω)^n)`.

Logical implications (`p_joint = min p_i`, one leg strictly implying another) are **not** blocked: the joint prices them correctly and automatically. The player pays two legs of vig for one leg of risk, which is a bad bet rather than a broken one. Whether the interface should discourage them is a presentation question, not a math one.

Two of these shapes — `BTTS YES + Under 2.5`, and the implication `Under 2.5 ⊂ BTTS NO` — were artefacts of draws being unrepresentable, and **draws were greenlit 2026-08-12** (Lane 1). A 1–1 result restores both, so each leaves its set: the first becomes merely unlikely, the second stops being an implication at all. See *Pending: draws* below.

### Void: re-price on the survivors

When a leg voids, the ticket re-prices against the surviving legs' joint:

```
o_sgp' = 1 / (p_joint(surviving legs) × κ × (1 + Ω)^{n−1})
```

Dropping a voided leg's factor out of a product — today's behaviour — is **wrong under a joint price, and is what no real book does**. Real books split between re-pricing on the remainder and voiding the whole ticket. Re-pricing is chosen here because the price was a statement about a joint event: remove a leg and the event itself has changed.

Void-replacement prices are **computed and locked at ticket lock**, one per single-void scenario, never re-derived at settlement. That keeps settlement deterministic and independent of when a void is discovered. (Multiple simultaneous voids: OPEN — the one documented commercial mechanism covers a single void only.)

### Pending: draws (Lane 1, greenlit 2026-08-12)

Draws are being introduced by the pre-game markets lane. **Sequencing is not assumed here** — coordinate through the orchestrator, never against that lane's timeline. What the change does and does not touch:

**Unaffected — the model is structurally draws-agnostic by construction:** the pricing rule, the EV-parity property, the relation vocabulary and the no-label fallback, the void re-pricing rule, the conditional cash-out form, the count-family joints (corners and cards never read the score), and the scorer inclusion–exclusion (it conditions on a team's goal count, not on who won). The goal-family sum is already written over `W` rather than a hard-coded pair of branches, so it absorbs a third outcome class without a rewrite.

**Affected — every measured quantity in this section, without exception.** All counts and percentages above (the 22 impossible pair shapes, the 57 triple shapes, `ρ` ranges, the 51.4% independent share, the 3.49% figures) were measured against the pre-draws model and **must be re-measured once draws land**. The reconnaissance harness and its method are recorded in `docs/sgp/correlation-recon.md` §1, so a re-measure is a re-run, not a rebuild.

**Expect the sets to change in both directions.** The two artefact shapes leave. If a draw becomes a selectable market, new mutually-exclusive pairs arrive with it (`Draw` against either moneyline) — all of them labelable under the existing vocabulary, which is the point of having defined relations structurally rather than empirically.

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
