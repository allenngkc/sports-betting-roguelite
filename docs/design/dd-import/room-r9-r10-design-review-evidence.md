# R9 / R10 — room: evidence for Design Director re-review

**From:** room-refinement lead · **Date:** 2026-07-31 · **Editor:** not needed to review this
**Captures:** `room-r9-r10-captures/` beside this file
**Re-review is due per batch-2's own sequencing** — R7 was parked with "re-review after R9/R10",
and both have now landed.

Readable without repo access: every number is inline.

---

## Scope

Three things for you, in descending order of consequence:

1. **A register amendment.** R12 as written would have predicted the approach that failed. §4.
2. **A design call.** The couch has headroom to push further, or not. §5.
3. **Confirmation of the re-review itself** — is the room's read now the bar. §6.

R9 and R10 are both closed and gated; neither needs approval to proceed. The interesting output
of this batch is what they measured, not that they shipped.

## 1. R9 — approved, executed, and a measured no-op

The approved 35% ambient reduction was applied and gated. Every gate passes. **It passes because
nothing changed.** A/B with everything else identical:

| region | ambient 100% | ambient 65% |
|---|---:|---:|
| ceiling plaster | 29.12 | 29.12 |
| couch corner | 35.15 | 35.14 |
| under bunk 2 | 49.36 | 49.35 |
| whole frame | 37.75 | 37.76 |

0.10% of pixels moved by more than one luminance level. That is film grain.

**Why, and it is my error to own:** R6 had already removed the flat-ambient problem as a side
effect. Every static surface samples the probe volume rather than the ambient probe, so ambient
now reaches the room only as environment input to the bake — and the room is a sealed box, so
that environment is fully occluded. I proposed R9 from pre-APV reasoning and never re-derived it
after APV landed. You approved a change I had mis-specified.

**No action needed from you.** The values are in band and harmless, the call site records the
measurement so nobody re-derives it, and the finding is more useful than the change would have
been: *there is no flat fill left in this room to remove.* If deeper shadow is ever wanted, the
levers are the grade or individual light intensities — a different ask, and one I would bring to
you rather than assume.

## 2. R10 — the ruled route ran to falsification, then the fallback worked

You changed R10's route to *directional variation, not a fifth light*, with bounce first and a
grazing source as fallback. Run in that order:

| lever | couch relief | couch mean |
|---|---:|---:|
| baked bounce — wide, close (100°, 0.45 m) | **0.93×** | +2.50% |
| baked bounce — narrow, back (50°, 1.15 m) | **0.94×** | +2.28% |
| **grazing direct** — existing CouchGraze 0.32 → 1.60 | **1.24×** | +3.82% |

Both bounce attempts made the corner **brighter and flatter**. Narrowing the source barely moved
it, so this is structural rather than a tuning miss.

The grazing source needed no new light and no new position: `CouchGraze` already sat at y = 1.44,
under the bunk slab's underside at 1.50, so your y < 1.50 constraint was met by construction.

**Gates:** bunk-2 mattress **43.97**, dead centre of 43.9 ± 1 and unchanged — the constraint four
earlier attempts at this corner were reverted for. Every region outside the couch moved
0.00–0.07%, so the light is properly local. Couch mean +3.82%, inside the 10% band.

## 3. Where the room stands now

Relief, R6 baseline → current. Higher is more surface detail reading.

| surface | R6 | now | |
|---|---:|---:|---|
| Right wall plaster | 9.99% | 9.85% | unchanged — R6 already did the work here |
| **Couch fabric** | **2.63%** | **3.27%** | **×1.24** — the only thing R9/R10 moved |
| Far wall plaster | 2.41% | 2.42% | unchanged |
| Floor aisle | 2.40% | 2.45% | unchanged |
| Ceiling plaster | 2.04% | 2.05% | unchanged |
| Whole frame | 6.55% | 6.60% | — |

**Read these only as ratios within this table.** The absolute figures come from a different
sampling than the 8.7% / 2.3% quoted in the earlier phase reports; identical pixels measure ~14%
higher here. Nothing regressed — the harness is simply reproducible where the hand measurements
were not.

## 4. Register amendment proposed — R12

**R12 as written:** *surface detail is gated by lighting, not texture authoring.*

That is true and it survived. But **as written it would have predicted the baked bounce works** —
bounce is lighting, and R12 says lighting is the gate. It cost two bakes to find out otherwise.

**Proposed sharpening:**

> Surface detail is gated by lighting, not texture authoring — and specifically by **direct light
> arriving at a grazing angle**. Bounce fills shadow; it does not reveal surface.

**The evidence, which also explains why:** relief is a *gradient divided by a mean*. Probe
lighting is spherical harmonics, smooth at the scale of a cushion, so it raises the mean without
raising the gradient — it can only ever move the denominator. Any probe-mediated addition
therefore *lowers* relief on a surface that is already lit.

**This does not contradict R6**, and the distinction is the useful part. There the walls went from
flat ambient to bounce that varied across metres of surface — real gradient where there was none.
Here the couch is already lit and the addition is smooth across the whole cushion.

The amendment also retired a lever without spending an editor lease: raising the bunk slab's
underside albedo to bounce more light down is another probe-mediated addition, so it fails for the
same reason. It was not tried.

## 5. Design call — couch headroom

The couch sits at **3.27%** against the right wall's **9.85%**. It is the room's strongest normal
map (channel sd ≈ 80, against plaster's ≈ 24) and still its weakest read.

Couch mean is at **+3.82%** against a **10%** band, so roughly 2.5× more grazing light could be
added before the gate stops it.

**The trade, stated plainly:** more grazing light raises relief further, and also lifts the
darkest corner of a room whose direction depends on the left half staying dark. That is a
composition judgement, not a technical one, which is why it is yours.

Three options, no recommendation from me beyond noting the middle one is reversible in one number:

- **Leave it.** 1.24× is a real gain, the corner reads as fabric rather than a grey mass, and the
  dark left half is preserved exactly as ratified.
- **Push to roughly half the headroom** (CouchGraze ~2.6 instead of 1.60). Expect relief around
  1.5–1.7× and couch mean near +7%. One number, one bake, fully reversible.
- **Accept the couch as the room's quiet surface.** It is in shadow by design; not every surface
  has to perform.

## 6. The re-review itself

Batch-2 set the bar as *the direction's read*, not the concept render. Against that:

- Walls carry their plaster (R6, ×6.3 on the right wall).
- The ceiling reads without help, which is why you dropped the soot.
- The couch now reads as fabric rather than a grey mass, if modestly.
- The floor is the remaining flat surface — it is lit from above at θ ≈ 10–30°, which by the
  amended R12 is the near-worst case for relief, and no lever short of a new grazing source
  changes that.

**Your call:** is the room's read now the bar, or is there a specific surface you want carried
further before this slice is closed?

## 7. What is deliberately not here

- **R7 wear** stays parked at Tier 1b per batch-2. Nothing in R9/R10 touched it.
- **R8 geometry** is held pending this review, as instructed.
- **The Decal Renderer Feature** remains not-yet; if R7 ever resumes, wear gets re-placed against
  the camera frusta first, since the last attempt put dirt where the cameras cannot see it.
- **TV green** is still the placeholder under the C2 interim ruling. Every capture here contains
  it. Please do not judge the room's colour balance against it — the target is cold white-grey at
  TV Phase 3, and the room's wear colour is authored neutral so it survives that correction.

## 8. Captures

In `room-r9-r10-captures/`:

| file | what |
|---|---|
| `01-standing-overview-CURRENT.png` | the room as it now stands — the useful one |
| `02-seated-tv-couch-CURRENT.png` | seated 17°; narrow, mostly screen |
| `03-focused-laptop-desk-CURRENT.png` | laptop 30°; narrow, mostly screen |
| `04-standing-overview-BEFORE-R9-R10.png` | R6 state, for the pair |

Compare 04 → 01. The change is confined to the couch; everything else is deliberately identical,
and the measurements in §3 say so numerically.
