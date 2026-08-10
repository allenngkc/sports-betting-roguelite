# Blur bundle — pre-committed disposition

**Seat:** Design Director (`main-2` terminal) · **Filed:** 2026-08-09, before the bundle landed
**Status:** disposition, not a ruling. Nothing here is canon until it is a row in `REGISTER.md` (C22).

> **FIRED 2026-08-09 — see `register-entries-2026-08-09-batch-20.md` §0.** Branch 3 (the instrument
> convicted) is **falsified** — the harness reads sharper than the path the complaint came from, so
> nothing in the hunt retracts and batch 15's L2 finding is not disturbed. Branch 1 fires on the
> **locus** axis. The **cause of Allen's additional ~56%** is the only thing still open, and it is
> narrower than any branch written here anticipated. This file is closed; it stays on the record as
> the pre-commitment the ruling was audited against.

Filed for the reason R39-am closed without a second ruling: a disposition written *before* the
evidence is a disposition the evidence can fire. Written after, it is a reaction.

---

## The question

Allen's R22 walk (2026-08-08) reported one finding: the SureThing UI on the laptop reads very
blurry in the room. His ruling now covers the phone as well. The hunt has pinned a glyph edge ramp
at ~1.6px screen-space, frame-wide, post-resolve, surviving 1.5× supersampling; six causes are
exonerated on measurement; the capture harness is neither convicted nor cleared.

**Is the ramp in the player's path, or is it the instrument's?**

Two arms decide it, and they answer different questions:

- **Arm A — Allen's display-path PNGs.** Decides whether the blur is *real*. His display is the path
  the complaint came from; it is the only arm that does not run through the harness under audit.
- **Arm B — the phone canvas on the same frame.** Decides the *locus*. The phone and the laptop share
  the resolve and post chain and share nothing above it.

---

## Pre-commitments

**1. Ramp in Allen's PNG **and** on the phone → REAL and SHARED-PATH.**
Cause sits at or below the resolve — post chain, SMAA, upscale/display scaling — not in per-surface
authoring. Nothing is remedied by re-authoring type, re-baking an atlas, or moving a canvas value.
Opens one new **cross-surface** item at the pipeline; no SureThing item re-opens on authoring grounds.
The phone routes here too, not to a phone spec: the phone stays a stub by design (C9), so a legibility
finding on it is the pipeline's, never authored content's (R28/R28-am).

**2. Ramp in Allen's PNG, **not** on the phone → REAL and PER-SURFACE.**
Locus is the laptop canvas's own path (atlas, material, canvas scale). A SureThing item, ruled on the
same frames.

**3. Allen's PNG clean, harness reproduces the ramp → the INSTRUMENT is convicted.**
This is the expensive branch and its consequence is pre-drawn, so it is not argued after the fact:

- Every claim that lives in the **glyph-edge channel** returns to unadjudicated (§2.6 — a confounded
  measurement closes nothing). Named now: batch 15's L2 finding that 12px and 13px are
  indistinguishable on frame. An instrument that softens edges cannot be the instrument that rules
  two type sizes identical at the edge.
- Every claim that does **not** live in that channel is untouched — composition, colour, ladder
  position, geometry, inversion contrast, emission. The Design-verified grants rest on those.
  **This boundary is drawn before the result, deliberately.** A convicted instrument is not a licence
  to re-litigate the record; it invalidates exactly the channel it distorts and nothing adjacent.

**4. Ramp absent everywhere at the review pose → NOT a null; re-shoot.**
Per the hunt's own second adopted law: a null is invalid if success would sit under the instrument's
own floor. A 1.6px ramp measured at a pose where 1.6px is sub-resolution is not evidence of absence.
Returned unadjudicated and re-shot at a pose and scale where the ramp would be visible if present.

## Standing in every branch

- **`_Sharpness` is not the remedy.** Maxed it buys 9.6% against a floor — that is tuning an effect
  that failed on mechanism toward invisibility (C10), on the one channel where product facts live.
- **Room gates 6/7/8 are not held on this.** Unchanged from batch 19: the re-issue is Allen's call,
  and this seat does not hold a human gate hostage to a defect it has not yet ruled.
- **The two instrument laws are ruled on their own merits**, whatever the blur's cause: a control must
  be able to witness the failure it guards; a null is invalid if success would sit under the
  instrument's own floor. Both are C18 §4.2's shape and both stand independent of this outcome.

## What the bundle must carry for any of the above to fire

Allen's PNG and the harness frame **of the same pose**, stated scope, stated resolution, and the
space each number is measured in (C25, C32, C33-am3). A ramp quoted in one space against a ramp
quoted in another is the batch-17 error repeated on a softer channel.

---

# Amendment — the timing gate (2026-08-09, before the bundle staged)

The bundle arrives **split**: the build-side half is closed evidence; the display-path half is OPEN
pending Allen's check-3 shot at 100% scale. Orchestrator's instruction: rule what the evidence
supports, the open half rides.

## Every branch above rides

Branches 1–4 all key off **Arm A**, and Arm A is the open half. **None of them fires on the
build-side evidence.** Recorded explicitly so that nothing ruled build-side is read as a verdict on
the blur: the build-side half can say what the ramp *is* and what it is *not caused by*. It cannot
say whether the player sees it. That is the whole question and it stays open.

## What the build-side half can close

1. **The two instrument laws**, now arriving with their founding cases. Adopted at batch 19 ahead of
   the evidence; the cases are what promote them from adopted to numbered. Ruled on their own merits
   in every branch — their standing never depended on the blur's cause. Next free ID is **C36**
   (C35 = element-and-ground, batch 19).
2. **The six exonerations**, individually, each ratifiable iff it states scope, resolution and space
   (C25/C32/C33-am3). An exoneration is a null result, so law two applies to each one in turn: a null
   is invalid if success would have sat under that instrument's own floor. Six nulls are six
   opportunities for that failure, not one.
3. **The structural consequence of a frame-wide, post-resolve ramp that survives 1.5× supersampling**
   — wherever it originates, it is below the canvas, so no re-authoring remedy is available to any
   surface. This holds independent of Arm A.
4. **The harness's own audit status.** "Neither convicted nor cleared, and its backbuffer arm cannot
   run in batchmode" is itself an instrument finding in C18 §4.2's shape and can be recorded as one
   without waiting on Allen.

## Check-3 must be a control that can witness the failure it guards

Filed while it is still worth filing. 100% display scale is **necessary and not sufficient** — a
shot that removes the OS resample and keeps a player resample has moved the confound, not removed
it. For check-3 to separate render blur from display blur, the whole chain is 1:1:

- OS display scaling at 100%;
- player backbuffer at the display's **native** resolution — no windowed downscale, no fullscreen
  stretch from a non-native mode;
- PNG written without resample, and **read at 100% zoom** — a 1.6px ramp inspected in a viewer at
  67% is measured through a third resample;
- the **same pose** as the harness frame, or the two arms are not comparable.

Any link not held 1:1 is stated on the shot's own line. A check-3 that cannot say which links were
1:1 answers nothing, and the studio has spent this fortnight learning that a control nobody can
audit is a control everyone learns to ignore.

## The one number that converts the ramp into a verdict

**State the canvas-pixel → screen-pixel ratio at the acceptance view.** A 1.6px ramp is unanchored
until it is expressed against the stroke it sits on: at the seated pose the laptop's 1024-wide canvas
maps to some smaller number of screen pixels, and a 13px product fact (S2's floor) lands at whatever
that ratio makes it. The ramp as a **fraction of the glyph stroke** is the number that decides
whether this is cosmetic softness or a breach of the product-fact floor. Owed with the bundle; it is
arithmetic from figures the room lane already holds, not a new capture.
