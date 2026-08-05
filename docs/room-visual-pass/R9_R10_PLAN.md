# R9 + R10 — plan

**Date:** 2026-07-31 · **Authority:** Design Director batch-2, approved with bounds
**Status:** R9 implemented pending gate re-run; R10 planned, deliberately not started

---

## 0. Standing law R12

> **Surface detail is gated by lighting, not texture authoring.**

Promoted from the R5 finding. It is the reason R9 and R10 are lighting items rather than
material items, and it is the reason ceiling soot was dropped rather than fixed — the ceiling is
the one surface already receiving light that varies across it.

Practical form for this room: **do not answer "this surface looks flat" with a stronger map.**
Ask what direction its light arrives from. Measured evidence in `PHASE_A_FINDINGS.md` §2–3.

## 1. The two items pull against each other in the same corner

Worth stating before either lands, because it sets what "success" means.

**R9 removes flat fill.** Ambient arrives equally from every direction, which is why it
suppresses relief (R12) — but it is also the only light some parts of this room get. The couch
corner is one of them.

**R10 must make that same corner read.** It was already the room's worst surface at 2.3% relief
despite carrying the strongest normal map, and R9 makes it darker still.

So: **R9 first, then re-measure the couch, then set R10's target.** Any R10 number chosen before
R9 lands is measuring a room that is about to change. This matches the DD's sequencing and is
also the only order that produces a meaningful target.

## 2. R9 — ambient rebalance

**Approved bounds:** reduce flat ambient 30–40%; full 8/8 gate re-run; mattress 43.9 ± 1; region
means within 10%.

Taking the **midpoint, 35%** (multiplier 0.65) — the middle of an approved band is the right
first sample, and the band is narrow enough that a second iteration inside it is cheap if needed.

| Trilight channel | before | after (×0.65) |
|---|---|---|
| Sky | 0.090, 0.087, 0.061 | 0.0585, 0.0566, 0.0397 |
| Equator | 0.057, 0.057, 0.048 | 0.0371, 0.0371, 0.0312 |
| Ground | 0.036, 0.032, 0.025 | 0.0234, 0.0208, 0.0163 |

### The risk that decides whether 35% survives

**Ambient is not only a runtime term — it is also the environment input to the probe bake.**
Lowering it lowers the baked indirect as well, so the effect is larger than a naive reading of
"flat ambient down 35%" suggests, and it is **not uniform**:

- regions dominated by *direct* light (the tube pool, the desk lamp, the window) barely move;
- regions dominated by *indirect* light (the couch corner, under both bunks, the door end) take
  close to the full reduction.

The approved "region means within 10%" band is therefore **most likely to fail in the darkest
regions**, not the bright ones. If it does, that is a finding to report, not something to tune
around — the honest options would be a smaller reduction inside the 30–40% band, or accepting a
wider band on indirect-dominated regions with the DD's agreement. I will not quietly widen it.

### Gate re-run — the old numbers are stale

`PHASE_5_GATES.md` was written for the pre-bunk-2 room. The counts in gates 2 and 3 must be
restated or they will "fail" against a room that legitimately changed:

| Gate | As written (2026-07-25) | Correct for the current room |
|---|---|---|
| 2 | 4 lights, 3 dressing groups | **8 lights, 6 dressing groups**, 1 each of `RoomArtRoot`, `RoomArtGenerated`, `RoomPostFx`, `AdaptiveProbeVolume` |
| 3 | 24 colliders | **27** — the three bunk-2 boxes, added and accepted in Phase 6 |

Gates 1, 4, 5, 6, 7, 8 stand as written. Gate 8 remains structural-only, as at sign-off.

Two checks are added for R9 specifically, from the DD's bounds: mattress mean luminance
**43.9 ± 1**, and per-region mean luminance **within 10%** of the R6 reference
(`artifacts/room-visual-pass/apv/`).

## 3. R10 — directional variation, bounce first

**Route changed by the DD:** the requirement is *directional variation*, not a fifth light. R6
proved bounce is the lever, so bounce is tried first and a grazing source is the fallback
(y < 1.50 so it touches neither bunk, same gate).

Levers in order, each measured against couch relief% before moving on:

**1. Baked-only bounce light.** A light with `lightmapBakeType = Baked` contributes *only*
through the probe field — it never renders directly and casts no realtime shadow. Aimed at the
surfaces around the couch so the corner receives indirect from a specific direction rather than
from everywhere.

*Being straight about this:* it is technically still a `Light` component, so it is worth the
DD's confirmation that it meets the intent. My reading is that it does — the player never sees a
new source, they see the corner lit from a direction, which is exactly "directional variation
rather than a fifth light". If the DD reads it as a fifth light in disguise, say so and I will
drop to lever 2.

**2. Raise the albedo of the bunk-1 slab underside.** The slab sits directly over the couch, so
brightening its underside turns it into a reflector aimed down into the corner. Physically the
most honest "bounce" available and it is a material change, not a light. Weaker than lever 1,
because it is second-order — it only redirects light that already reaches the slab.

**3. Fallback: strengthen the existing `CouchGraze`.** Already at y = 1.44, below the 1.50 slab
underside, so it is inside the DD's constraint by construction. It exists at intensity 0.32 and
simply is not strong enough; this is the known-safe answer if bounce cannot carry it.

### The constraint that killed four previous attempts

Anything that lights the second bunk's mattress breaks the ratified *legible as occupied, never
legible as empty* treatment. Four attempts have been reverted for exactly this. The mattress
gate (**43.9 ± 1**) applies to R10 as well as R9, and is the first thing to check on every lever
— before looking at whether the couch improved.

## 4. Sequencing

1. R9 code change (done) → lease → build, bake, capture, 8/8 gate re-run + the two R9 bounds.
2. Re-measure the couch corner under the new ambient. **Set R10's target then, not before.**
3. R10 lever 1 → measure → lever 2 → measure → lever 3 only if bounce cannot carry it.
4. Re-review with the DD after both land; the direction's read is the bar, not the concept.

Gates 2–5 and the photometric bounds are scriptable from the saved scene and the captures; that
harness is being built separately so each lease is spent rendering, not measuring.
