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
- **Correlated parlays (same match):** the naive product is not the ticket's probability. It is wrong in *both* directions and is sometimes exactly zero. Closed 2026-08-12 — see **Same-game tickets** below. The note that stood here proposed a copula-lite shared "momentum" latent; that is obsolete. The sim's match model is already an exact finite joint, so the answer is enumerated, not approximated — and the "book prices them independently, so it's a free exploit" framing died with it, because our book can price them correctly and does. The exploit's home moved to RELICS — a relic breaking a book the player has learned to trust is worth more than a bug found in a book that was never correct (S73/batch 47).
- Arbitrage across two books: guaranteed profit when `1/o_A(book1) + 1/o_B(book2) < 1`; stake split `s_i ∝ 1/o_i`.
- Hedge of a live parlay: with one leg left at live odds `o_live` for the *opposite* outcome, staking `h` on the hedge locks a band between outcomes — the "take guaranteed money" tool the player buys access to.

## Same-game tickets — the correlation model

Designed 2026-08-12 (sgp lane, F_0.6.0) on measured evidence, not judgement: `docs/sgp/correlation-recon.md` (exact reconnaissance, verification gate at 2.554e-15 over 437,832 checks), `docs/sgp/model-candidates.md` (margin-method literature), `docs/sgp/research-sgp-pricing.md` (real-book practice).

Two legs on one matchup are correlated, so `Π p_i` is not the ticket's probability. This is not a matter of accuracy. On the shipped board, **355 two-leg shapes have joint probability of exactly zero** — tickets with no winning outcome, which the naive product would sell at finite odds. That is why the one-leg-per-match guard existed, and lifting it required everything below.

*(Re-measured 2026-08-13 on the merged draws board, 77 selections per matchup. The pre-draws figures were 22 two-leg shapes and 57 three-leg shapes over 36 selections; the argument did not change, its scale did — see* The correlation ratio *below.)*

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

`ρ` is a pure property of the joint distribution and is **independent of the overround**. Re-measured 2026-08-13 on the merged draws board (15 market kinds, 77 selections per matchup), against the pre-draws board it replaced:

| | pre-draws (36 selections) | now (77 selections) |
|---|---|---|
| `ρ` range, 2 legs | `[0, 3.11]` | **`[0, 14.23]`** |
| `ρ` range, 3 legs | `[0, 11.88]` | **`[0, 71.69]`** |
| `ρ` range, 4 legs | `[0, 14.82]` | **`≥[0, 219.66]`** (sampled) |
| impossible share (2 legs) | 3.49% | **12.13%** (355 shapes) |
| implication share (2 legs) | 3.49% | **8.51%** (249 shapes) |
| exactly independent (2 legs) | 51.4% | **42.65%** |

**Read the direction, not just the numbers.** Roughly one two-leg combination in eight is now *impossible* rather than one in thirty, and the naive product's error at four legs reaches two orders of magnitude on correct-score-heavy tickets. Every argument in this section got stronger; none of them changed. The teaching rule's "he is right about half the time" is now 42.65% rather than 51.4% — still about half, but drifting, and worth re-checking whenever the board grows again.

**`ρ` is a diagnostic, not an interface.** It is what the audit and the reconnaissance report. It is *not* what the model hands downstream — see the next section, where shipping it as a bare scalar is a named and prohibited failure.

### What the model emits: a joint probability *and* a relation label

**Binding constraint (S73, batch 45, canon).** The ticket is its own instrument. The surface never shows a product-of-legs or an adjustment line; the relationship is marked as an annotation. So the model must emit, beside its joint probability, a **relation label** — structured data carrying enough for presentation to compose a *sentence*, never a formula. And:

> **Where the model finds correlation it cannot label, the price does not move.**

The prohibited implementation is `p_joint` collapsed to a bare scalar `ρ`: that leaves step 5 holding a price it cannot explain, and the rework lands back here. **The model's output type is `(p_joint, relations[])`, not a number.**

**The relation vocabulary.** Correlation on this board is not diffuse — it arises from a closed set of structural causes, which is why this constraint is satisfiable rather than aspirational:

| Relation | Arises when | The sentence it must support |
|---|---|---|
| `MutuallyExclusive` | `p_joint = 0` | these legs cannot **all** win |
| `Implies(a → b)` | `p_joint = min p_i`; one leg strictly entails another | b has already happened whenever a does |
| `SharedScoreline(reinforcing \| opposing)` | two GOAL-family legs read the same scoreline | one makes the other likelier / less likely |
| `SharedCount(family, sign)` | two legs of the same COUNT family reading an **overlapping side** — the same corner or card draw | one makes the other likelier / less likely |
| `ScorerOfSide(side)` | a scorer leg beside a leg on that team's goals | the same goals settle both |
| `Independent` | legs drawn from different families | unrelated — no adjustment |

**`SharedCount` was a hole in this table, found in build (2026-08-12) and ratified here.** The board ships three corner lines and three card lines, so a *band* — corners `OVER 8.5` with `UNDER 10.5` — is correlated, is not an implication, and is not impossible. It had no label, and under this section's own no-label fallback it would have priced at the naive product. Corner×corner `ρ` reaches 4.13, so that was a real leak, not a rounding-scale one. Six pair shapes per matchup.

**`SharedCount` was NARROWED, not widened, 2026-08-13 — and the direction matters.** Team totals split each count family across two *independent* draws, so `HOME corners` beside `AWAY corners` is same-family yet exactly the product. Labelling that `SharedCount` would assert a correlation the model had just measured as absent, so the relation now requires an **overlapping side**, and such a pair takes `Independent`. The `Independent` row's gloss — "legs drawn from different families" — is consequently narrower than the board it describes; its *binding* half, "unrelated — no adjustment", is what governs. Flagged for the Design Director rather than smuggled, because it slightly widens what an unmarked pair can be.

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

**A refusal must emit cause AND remedy, structurally** (S73-am4, `docs/design/surething-design.md` §3.3 — the owning doc). A refused combination is a *Blocked* state, and that row has always required both halves: naming what cannot happen is the cause, **naming which leg to drop is the remedy**. So a rejection is not an exception string — it carries the offending leg set and the drop that makes the ticket valid. The surface stamps it; the model supplies the parts, as with `principal`.

**The remedy is a SET, not a leg — corrected 2026-08-13, and the correction understated it 2026-08-14.** This first said "a droppable leg", singular. The first correction said multi-leg remedies appear only above the shipped `κ = 1`, measured over 1,961 refusals on the pre-draws board.

**That is no longer true. On the merged 15-market board, remedies of up to THREE legs occur at `κ = 1`** — for both `DuplicateSelection` and `ImpossibleCombination`, measured over 645 refusals, every one of which carried a remedy that placed after being spent. The board growing from 36 to 77 selections is what did it: conflicts and repeats now stack deeper than a single drop can clear. So the plural remedy is a **present requirement, not a contingency riding on the margin dial.**

Two consequences that bind any consumer:

- **A surface that reads the first element of the remedy leaves the slip still refused.** The whole set must be spent.
- **Remove high index to low.** Removing an earlier leg first shifts the indices of the later ones.

A remedy always exists, since dropping to a single leg is always placeable.

**The remedy is CONJUNCTIVE, and the cause is N-valued — DD ruling, batches 66–67 (c467df3).** Both halves of a Blocked row must state the whole set as one instruction:

- **"or" / "one of" / a menu is banned in a remedy.** A remedy offering a choice fails when followed: dropping one element of a three-leg remedy leaves the slip still refused. It is *drop all of these*, never *drop any of these*.
- **A cause may name more than two legs, so two-valued phrasing is wrong.** "These cannot both land" breaks the moment three legs are jointly impossible with every pair among them fine — which this model produces, and which is why ticket-level exclusion exists at all. The sentence to support is *these cannot all win*.

This binds the model's emitted parts and any copy composed from them.

The cause is likewise **minimal**: the smallest leg set that still reaches `p = 0`. A two-leg conflict inside a four-leg ticket names two legs, not four. The joint-only-impossible triples name three, because no smaller true answer exists.

Two things the same law fixes in place, both already true here: the engine **prices the individual leg and refuses only the combination**, so a refused leg stays reachable on its own; and a bet that cannot win is never purchasable, because a price is a factual claim about an outcome.

Logical implications (`p_joint = min p_i`, one leg strictly implying another) are **not** blocked, and this is now settled rather than open: the leg is legal, correctly priced, and added, with the fact stated in its own space. The player pays two legs of vig for one leg of risk — a bad bet, not a broken one. A house that stops him being stupid is not this product; one that tells him and lets him proceed is.

Two of these shapes — `BTTS YES + Under 2.5`, and the implication `Under 2.5 ⊂ BTTS NO` — were artefacts of draws being unrepresentable. **Draws shipped, and both left their sets exactly as predicted** (verified 2026-08-13): `BTTS YES + Under 2.5` is now possible, and its joint is *exactly* `P(1–1)`; `Under 2.5 ⊂ BTTS NO` is no longer an implication but an opposing shared scoreline. The prediction was made in step 1 from the research alone, before either was implemented, and it held — which is the best evidence available that the model and the world agree.

### Void: re-price on the survivors

When a leg voids, the ticket re-prices against the surviving legs' joint:

```
o_sgp' = 1 / (p_joint(surviving legs) × κ × (1 + Ω)^{n−1})
```

Dropping a voided leg's factor out of a product — today's behaviour — is **wrong under a joint price, and is what no real book does**. Real books split between re-pricing on the remainder and voiding the whole ticket. Re-pricing is chosen here because the price was a statement about a joint event: remove a leg and the event itself has changed.

Void-replacement prices are **computed and locked at ticket lock**, never re-derived at settlement. That keeps settlement deterministic and independent of when a void is discovered.

**Multiple voids are supported — CLOSED 2026-08-12, reversing the earlier OPEN.** The original note deferred this because the one documented commercial mechanism covers a single void only. That reasoning does not transfer: books limit themselves for latency and volume reasons we do not have. Two Mulligan Slips on a three-leg ticket is an ordinary hand, and refusing it dead-ends real play. With `MaxLegs = 4` the complete set of survivor subsets is at most **15 prices per ticket**, computed once at lock — so price *every* subset, not just the single-void row.

**A replacement at or below evens voids the ticket and returns the stake — CORRECTED 2026-08-12.** A replacement can price at or below evens, and placement-time refusal is unavailable by then because the ticket is already sold.

**Superseded by the draws board, 2026-08-13:** the `≈1.3` threshold below was measured pre-draws. Draws made `BTTS YES` materially likelier and dragged the leg it entails with it, so sub-evens is now reachable sooner — at `κ = 2`, `OVER 1.5 + BTTS YES` prices at **0.962** and is refused. The exact new threshold is unmeasured; what is established is that it sits at or below 2, and that the rule still never fires at the shipped `κ = 1`.

**The two rulings in this section interact, and stating them independently understated the threshold.** The `1.1181` figure first quoted here was measured against *one-leg* survivors — but the κ-drop below puts a lone survivor permanently above evens, since it is just the board's own single, which `MatchModel.Offer` already guarantees prices above 1.0. With that accounted for, the real threshold is **≈1.3**, and a sub-evens replacement now **requires a correlated group to survive**: a distinct-matchup remainder is a product of board singles and cannot go sub-evens at any `κ`. Still inside the range the gate campaign will explore, so the rule stands — but it fires later and more narrowly than first written.

This section first specified *flooring the price* at 1.0 while justifying it as "the same outcome the full-void camp of real books produces." **Those are not the same outcome**, and the gap is not academic: a live ticket priced at 1.0 returns the stake only *if it wins*, and still loses everything if it does not. That is strictly worse for the player than the full void it claimed to imitate, and it produces the absurd contract *win and receive nothing*. The rule is the one that was actually argued for: **the ticket voids in full and the stake is returned, unconditionally.**

A refund is not a payout, so payout multipliers and ticket modifiers do not act on it — otherwise a Double or Nothing ticket would convert a void into a profit, which is a void the player would seek out. The refund follows the engine's existing stake-return path.

**κ applies only while a same-match group survives.** Voiding can leave survivors that no longer share a matchup: two legs on one match plus a third elsewhere, with one of the pair voided, leaves an ordinary parlay. `κ` is the price of correlation, so with no correlation left there is nothing to charge for and the survivors re-price at `κ = 1`. Moot at today's default; real the moment the gate campaign moves the dial.

**A Profit Boost travels with its leg.** It survives if that leg survives and dies if that leg voids, mirroring the ordinary-ticket path exactly. Any other rule would let a void change the value of a relic applied to a different leg.

**What a full void does to the rest of the ticket's economy** (ruled 2026-08-12; mine, reversible, flagged for veto):

- **The Free Bet token returns.** Its benefit is a refund on a *loss*, and a voided ticket neither won nor lost, so the token was never resolved and should not be burnt. This is also what real books do with a free bet on a void.
- **The Mulligan Slip does not return.** The player spent it to void a leg and it voided the leg — consumed doing exactly its job. That the downstream re-price then voided the ticket is a consequence of the bet, not a failure of the consumable.
- **A void feeds no loss-triggered mechanic** — not Scar Tissue, not the Bad Beat Jar, and it does not enter the bust chain. A void is not a loss. Any other reading would let a player farm loss-triggered rewards by voiding into them, which is a mechanic nobody designed.

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
