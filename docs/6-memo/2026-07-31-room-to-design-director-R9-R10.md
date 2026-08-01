# Room → Design Director: R9 and R10 candidate specs

**From:** Room refinement lead (worktree `room-refinement`, branch `room-refinement`)
**Date:** 2026-07-31 · **Route:** lead → orchestrator → Design Director
**Ask:** approve, reject, or amend. Neither is built. Neither will be built without a ruling.

Both items came out of R6 (indirect light, `fb44ac2`). Full measurement and reasoning:
`docs/room-visual-pass/PHASE_B_INDIRECT_LIGHT.md` §7.

---

## R9 — rebalance ambient down

**What.** Reduce the Trilight `RenderSettings` ambient, currently sky `(0.090, 0.087, 0.061)`,
equator `(0.057, 0.057, 0.048)`, ground `(0.036, 0.032, 0.025)`.

**Why.** Flat ambient is now doing work that directional bounce does better, and it actively
*suppresses* the thing R6 just bought. Ambient arrives equally from every direction, so a
perturbed surface normal changes nothing — it is the worst possible case for relief. Real bounce
is directional (light off the floor grazes the wall) and is what made the right wall's relief
contrast rise ×6.3. Every unit of flat ambient still in the mix is diluting that.

Expected: relief rises further, shadows deepen, and the room moves toward the concept's value
separation rather than away from it.

**Cost, and why it is not a tuning tweak.** Ambient is a global exposure lever. Lowering it
changes the value structure Allen signed off at 8/8 gates on 2026-07-28, so it needs the full
gate re-run, not a nudge and a screenshot. It also feeds the probe bake as environment lighting,
so every change costs a rebake (~6 min per iteration).

**Risk.** The room is already dark by direction. Overshoot reads as a horror cell, which §10 of
the handoff names as an explicit failure state. Recommend a bounded first step (~30–40%
reduction) measured against the gate ladder rather than an open-ended tune.

**Recommendation:** approve as a bounded experiment with the gate re-run attached. If the ladder
fails, revert — the R6 result stands on its own without this.

## R10 — couch-corner grazing source

**What.** One dim grazing light for the couch corner, constrained to sit **below the bunk-1 slab
underside at `y = 1.50`** so it can touch neither bunk.

**Why.** The couch fabric carries the room's strongest normal map — channel sd ≈ 80, against
plaster's ≈ 24 and the ceiling's ≈ 9 — and still measures 2.3% relief after R6, against the
right wall's 8.7%. It is the largest remaining gap between map quality and what reaches the
screen. Bounce alone does not rescue a corner that dark.

**Risk, and it is the important part.** Two previous attempts at exactly this class of fix were
reverted, both for the same reason: a grazing light needs an offset from the wall to be bright,
that offset puts it out into the room, and the geometry here puts a bunk in front of every wall
worth grazing. Both attempts lit a mattress and broke the ratified *legible as occupied, never
legible as empty* treatment. Bunk 2's mattress currently measures 43.9 mean luminance and that
number is the acceptance test.

The existing `CouchGraze` (intensity 0.32, below the slab) is the proof the constraint is
satisfiable — it just is not strong enough to make the weave read.

**Recommendation:** approve with the `y < 1.50` placement constraint written into the spec, and
the bunk-2 luminance check as the gate. Lower confidence than R9 — the history here is four
failed attempts, and I would rather flag that than present this as a safe win.

---

## Dependency note for scheduling

R7 (wear and contact grime, approved, plan at `docs/room-visual-pass/R7_WEAR_PLAN.md`) proceeds
without either of these. But R7.5's couch contact wear will under-read until R10 lands, since it
sits in the same unlit corner. That is accepted and noted rather than worked around — R7 will
not compensate for a lighting gap with texture, which is the mistake Phase A already documented.

No blocker. R7 starts now.
