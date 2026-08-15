# P5 — the relation statement: four drafts, and the coverage that decides them

**From:** `surething-ui` (Claude, Opus 5) · **For:** the Design Director · **2026-08-15**
**Against:** the DD's rubric — *relation, not name; toner; once per slip; never announces the
apparatus; no engine terms; no formulae* — plus the S73 dependency: **a sentence is only shippable
if it names a relation the model actually emits.**

Everything below is measured off the live board, not reasoned about. The harness is
`SureThingEntryTests.Evidence_S77_form_widths_and_refusal_arity_distribution` (filter-only).

---

## 1. What the model actually nominates as `principal`

6,109 placeable same-match slips, two matchups, all pairs. Read off `SameMatchPricing.Principal` —
the model's parts, not prose.

| Emitted principal | share |
|---|---|
| `Implies` / Reinforcing | 10.6% |
| `SharedScoreline` / Reinforcing / Goal | 7.9% |
| `SharedScoreline` / Opposing / Goal | 6.5% |
| `ScorerOfSide` / Reinforcing / Goal / Home | 7.7% |
| `ScorerOfSide` / Opposing / Goal / Away | 7.3% |
| `ScorerOfSide` / Reinforcing / Goal / Away | 7.1% |
| `ScorerOfSide` / Opposing / Goal / Home | 5.0% |
| `SharedCount` / Opposing / Corner | 0.5% |
| `SharedCount` / Opposing / Card | 0.5% |
| `SharedCount` / Reinforcing / Corner | 0.4% |
| `SharedCount` / Reinforcing / Card | 0.4% |
| **no statable relation (`Principal` is null)** | **46.1%** |

**Two things this settles before any copy is read.**

**(a) The four relations are the right four**, and `MutuallyExclusive` / `Independent` correctly
never appear — the first is a refusal, the second is nothing to state.

**(b) SIGN IS NOT DECORATION, and the drafts have to carry it.** Every relation but `Implies` is
emitted in *both* signs, and reinforcing and opposing are opposite claims about the same shared
thing. A single sentence per relation would state one of them falsely about the other. So each draft
below is a relation with its sign variants — four relations, seven sentences.

**(c) Nearly half of all placeable same-match slips state NOTHING.** That is not a gap to fill: it is
canon working ("where a correlation cannot be labelled, the price does not move"). **The silence
needs ruling as correct**, because the obvious review reaction to 46% blank is that something is
missing.

---

## 2. The four drafts

Toner, once per slip, composed from `principal`. No apparatus named, no engine term, no figure.

### Draft 1 — `Implies`

> **ONE OF THESE ALREADY COVERS THE OTHER.**

*Emitted:* `RelationKind.Implies`, `Sign = Reinforcing`, `Legs[0]` implies `Legs[1]`.
Only Reinforcing is ever emitted, so this relation takes one sentence.
*Rubric:* states the entailment, names no leg, no apparatus, no engine word, no figure.

### Draft 2 — `SharedScoreline`

> Reinforcing: **THE SAME GOALS SETTLE BOTH.**
> Opposing: **THE SAME GOALS SETTLE THESE OPPOSITE WAYS.**

*Emitted:* `RelationKind.SharedScoreline`, `Family = Goal`, both signs.
*Rubric:* "the same goals" is what the legs *share* — the relation itself. `Goal` is the only family
this relation is ever emitted with, so the word is authored rather than substituted.

### Draft 3 — `ScorerOfSide`

> Reinforcing: **THE SAME TEAM'S GOALS SETTLE BOTH.**
> Opposing: **THE SAME TEAM'S GOALS SETTLE THESE OPPOSITE WAYS.**

*Emitted:* `RelationKind.ScorerOfSide`, `Family = Goal`, `ScorerSide` ∈ {Home, Away}, both signs.
*Rubric — and the deliberate omission:* `ScorerSide` is carried by the model and is **not spoken**.
Naming the team would be a name, and the rubric asks for the relation. The team is already on both
rows he is looking at.

### Draft 4 — `SharedCount`

> Corner, reinforcing: **THE SAME CORNERS SETTLE BOTH.**
> Corner, opposing: **THE SAME CORNERS SETTLE THESE OPPOSITE WAYS.**
> Card, reinforcing: **THE SAME CARDS SETTLE BOTH.**
> Card, opposing: **THE SAME CARDS SETTLE THESE OPPOSITE WAYS.**

*Emitted:* `RelationKind.SharedCount`, `Family` ∈ {Corner, Card}, both signs. (`Goal` is never
emitted for this relation — it goes to `SharedScoreline`.)
*Rubric:* the counted thing is the substance of the relation, not a name.

**One thing to rule rather than assume:** these four are written as one shape — *the same X settles
both* — deliberately, so the surface teaches one idea in four instances rather than four idioms. If
that reads as templating rather than a family, they should be re-authored apart.

---

## 3. The S77 forms, measured against the control's own box

`296 × 44`, at both the shipped 13px and the 17px S77's analysis quoted.

| form | 13px | 17px | vs 296 |
|---|---|---|---|
| THESE TWO CANNOT BOTH WIN. | 184.1 | 240.7 | 62% / 81% |
| THESE THREE CANNOT ALL WIN. | 185.2 | 242.2 | 63% / 82% |
| THESE FOUR CANNOT ALL WIN. | 178.8 | 233.8 | 60% / 79% |
| THIS PICK IS HERE TWICE. | 150.5 | 196.8 | 51% / 66% |
| THIS PICK IS HERE THREE TIMES. | 190.9 | 249.7 | 65% / 84% |
| THIS PICK IS HERE FOUR TIMES. | 184.5 | 241.3 | 62% / 82% |
| THIS PAYS LESS THAN IT COSTS. | 188.1 | 245.9 | 64% / 83% |
| NO RUB OUT FIXES THIS SLIP. | 171.0 | 223.7 | 58% / 76% |
| RUB OUT THE MARKED LEG TO PLACE. | 220.4 | 288.2 | 74% / **97%** |
| RUB OUT BOTH MARKS TO PLACE. | 194.6 | 254.5 | 66% / 86% |
| RUB OUT ALL THREE MARKS TO PLACE. | 225.7 | 295.1 | 76% / **100%** |

**The 17px column is the answer to why the stamp is at 13px.** At 17px the widest form measures
295.1px in a 296px control — it fits by nine tenths of a pixel, and the second-widest by 8px. At
13px the widest is 76%. **13px is not merely the ≥13px floor being respected; it is the only one of
the two with headroom**, and headroom is what stops the next authored form from reopening this.

Two of the forms only fit at all because they were shortened under S77's step (1): the sub-evens
cause measured 319.6px at 13px before rewording, and the three-leg remedy ~304px. Neither was
truncated and the type was not shrunk.

---

## 4. Which forms actually fire

41,726 refusals over 121,137 combinations (six matchups, all pairs; matchup 0, all triples).

| | share |
|---|---|
| `ImpossibleCombination` | 82.3% |
| `DuplicateSelection` | 17.7% |
| cause arity 2 | 97.1% |
| cause arity 3 | 2.9% |
| remedy arity 1 | 93.9% |
| remedy arity 2 | 6.1% |

**Plural remedies are real, not hypothetical** — 6.1% of refusals need two legs spent, which is what
S73-am5 was ruled on and what a menu-shaped remedy would have failed.

**What this sweep did NOT reach, stated rather than implied:** it covers pairs and triples, so a
four-leg slip was never built. Cause arity 4 and remedy arity 3 therefore *cannot* appear here, and
their absence is coverage, not evidence — sgp measured remedies of up to three legs at the shipped
`κ`. `SubEvens` also did not fire once, so **three authored forms are currently unexercised by any
observed refusal**: `THESE FOUR CANNOT ALL WIN.`, `RUB OUT ALL THREE MARKS TO PLACE.`, and
`THIS PAYS LESS THAN IT COSTS.` They are written and gated, and they are not yet witnessed.

---

## 5. What is asked

1. **The seven sentences** — a word, or a re-authoring.
2. **The 46.1% silence** — ruled correct, so it is not read later as a missing statement.
3. **`ScorerSide` withheld** — confirm the relation is stated without naming the team.
4. **The one-shape family** in draft 2–4 — a family, or four idioms.
5. **The S77 forms themselves** are mine under step (1) but they are copy; they are drafts too.
