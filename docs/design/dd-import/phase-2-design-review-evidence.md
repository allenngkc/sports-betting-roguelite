# T6 — Phase 2 scene grammar: evidence for Design Director review

**From:** TV sweat lead · **Date:** 2026-07-31 · **HEAD:** `220c5ec`
**Suites:** engine 160/160 · EditMode 129/129 · PlayMode 44/44
**Visual status:** `PENDING-VISUAL-EVIDENCE` — see the closing section before drawing visual conclusions

---

## What Phase 2 was for

PRD §2 diagnosed the defect precisely: *"`BuildBeatScript` changes most variants through
`Lane(variant)` — centre, upper lane, lower lane — while retaining the same path and payoff.
Increasing the count would multiply positions, not movement ideas. **Variation must be modelled as
grammar, pressure, payoff shape, and reaction.**"*

Before Phase 2 the planner chose seven dimensions and the stage rendered one of them: lane.

## What now renders

| Dimension | Values | Commit |
| --- | --- | --- |
| Corner shape | near post · far post · cleared, each with a visible win-the-corner lead-in | `987ffa7` |
| Booking | visible challenge before the marker; card on the fouling side's **actor** | `987ffa7` |
| Non-goal ending | block · interception · keeper save · clearance · post · near wide | `6dd4ead` |
| Buildup grammar | central · wing · switch · counter · set piece | `cd836e5` |
| Chance shape | through ball · cross · cutback · rebound · direct | `220c5ec` |
| Reaction | celebrate · collapse from the plan, not the template tail | `220c5ec` |

### They compose rather than multiply

Grammar owns the approach, chance shape owns the delivery, payoff owns the ending — three budgeted
segments in sequence. **Nineteen authored pieces, not the ~150 a cross-product needs.** That is what
made the phase deliverable in four dispatches, and it is the property to preserve if the Design
Director wants more variety later: add to a dimension, not to a matrix.

### Invariants held throughout

- **Every shape totals exactly `B × 1.00`.** Hand-verified per shape at each phase. Grammar reshapes
  time; it never buys or spends it.
- **Truth is untouched.** A goal stages exactly one goal and fires one `MkGoal` — including
  **rebound**, whose first attempt is visibly stopped and carries *no* marker. A near miss never
  becomes a goal. A possession scene never shows a shot.
- **Mood and physics stay independent.** `CornerFor`/`CornerAgainst` and `NearMissHope`/`Scare` are
  the *bettor's* hope/dread and drive only the mirror. Which team physically wins a corner reads
  from the staged fact. These were welded together once and the bug regressed in the opposite
  direction during the fix — both directions are now pinned by test.
- **Deterministic.** Every choice derives from a presentation key with independent named channels.
  No engine RNG, no clock, no frame count. Same key, same scene.

## Two things a designer should know

**1. The gate can be passed without the phase being delivered.** Phase 2's automated exit gate tests
*signature diversity* — no repeated signature on adjacent beats, no grammar more than twice in a
rolling four. Those passed after `446ded7`, when the planner was wired but **six near-miss payoffs
still rendered as one shape**. A signature can differ while the motion is identical. The gate has
been meaningful only since `6dd4ead`–`220c5ec`. Read it accordingly.

**2. One composition genuinely does not work, and degrades on purpose.** A rebound chance shape
feeding a payoff that is itself a save or block would author two stopped attempts and read as
nonsense. That pair collapses to a direct strike in code, deliberately, rather than rendering.

## What is not proven

Everything above proves the **step data** — routes, markers, actor routing, durations, callback
counts. `-nographics` rasterises no frame, so **none of it proves the scenes look different from a
couch.**

Specifically unverified: whether five buildup grammars read as five different ideas at four metres;
whether six non-goal endings are distinguishable at that distance; whether corner attribution is
legible as *which team won it*. Those are the questions the muted-couch gate in Phase 4 exists to
answer, and they need a GPU-backed session this worktree cannot produce.

Test names for anyone wanting to read the assertions: `TheaterStageAttributionTests` carries the
per-dimension distinctness proofs; `TheaterStageMatrixTests` carries the 48-cell matrix and the
exercised-coverage checks.
