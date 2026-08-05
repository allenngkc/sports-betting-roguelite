# Room refinement — design review package (R5, R6, R7)

**From:** Room refinement lead · **To:** Design Director, via orchestrator → Allen
**Date:** 2026-07-31 · **Branch:** `room-refinement` · **Editor:** not required to review this

Three refinement items are implemented and awaiting design review. One of the three did not
deliver and is parked. This package is the evidence and the open questions.

**You do not need repo access to read this.** Every number below is stated inline; the image
paths are for whoever pulls them for you.

---

## 0. What is being reviewed, and what is not

**In scope for your call:** whether R5/R6 land the approved direction, and three open questions
in §5.

**Not in scope, already settled:** the direction itself (B — *Vice Grip*, stylised, Palette 1,
approved 2026-07-25) and the room's layout, collision and camera anchors (signed off 2026-07-28,
8/8 gates). Nothing in R5–R7 changed layout, collision or interaction. Collider count has been
27 throughout.

**One thing to hold constant while looking:** per the **C2 interim ruling**, the TV's green
spill is temporary — the target is `tv-sweat/DESIGN.md` §5 cold white-grey at TV Phase 3. The
green in every capture is placeholder. Please do not judge the room's colour balance against it,
and note that all R7 wear colour is authored neutral specifically so it survives that correction.

## 1. How to read the captures

Same three camera poses throughout — standing 68°, seated-at-TV 17°, focused-laptop 30° — all
Play Mode, all identical exposure. Comparable frame to frame.

| Folder under `artifacts/room-visual-pass/` | State |
|---|---|
| `concepts/` | the approved target language (AI concept, style reference only — not layout truth) |
| `baseline/` | graybox, before any art |
| `graded/` | end of the signed-off pass |
| `pbr/` | **R5** — full PBR surface maps |
| `apv/` | **R6** — indirect light |
| `r7-tier1b/` | **R7** — localised wear (current HEAD) |

The most informative pair is **`pbr/` → `apv/`**: same room, same exposure, one change.

## 2. R5 — full PBR surface maps (`cd62855`)

Albedo, normal, metallic/gloss and occlusion for plaster, worn floor, ceiling stain and fabric
weave, all derived from one deterministic height field.

**The maps are correct and the relief did not read.** Measured normal-map channel variance
(a flat map is sd ≈ 0):

| Surface | sd | Relief visible? |
|---|---:|---|
| Ceiling stain | **~9** (weakest) | **most in the room** |
| Worn floor | ~17 | barely |
| Plaster | ~24 | barely |
| Fabric weave | **~80** (strongest) | **none** |

That inversion rules out any explanation based on map strength. Lambertian sensitivity to a
normal perturbation scales with **sin θ** — light arriving perpendicular to a surface reveals no
relief, grazing light reveals the most. The fluorescent hangs 0.25 m under the ceiling and rakes
it at θ ≈ 87°; the floor is lit from above at θ ≈ 10–30°; the couch sits in shadow.

**Design consequence:** surface detail is gated by lighting, not by texture authoring. Asking for
"more surface detail" will not produce it; asking for light that varies across a surface will.

## 3. R6 — indirect light, Adaptive Probe Volumes (`fb44ac2`)

The lever R5 identified. Six static lights moved to Mixed so their *indirect* contribution bakes
into a probe volume; direct light and shadows still render in realtime, so every light pool tuned
during the approved pass is untouched. The TV and phone lights stay fully realtime.

Local relief contrast, standing view, same exposure:

| Region | before | after | |
|---|---:|---:|---:|
| Right wall (plaster) | 1.4% | 8.7% | **×6.3** |
| Far wall by window | 3.0% | 6.8% | **×2.3** |
| Whole frame | 3.69% | 6.17% | **×1.67** |
| Couch (in shadow) | 1.9% | 2.3% | ×1.26 |
| Floor | 1.8% | 2.1% | ×1.17 |

**Mean luminance did not move** — 33.0→32.0, 38.6→38.4, 29.0→28.5 by region. The room is not
brighter; the texture became visible. The signed-off value structure is intact.

Two caveats worth knowing before you look:

- **The gain is on the walls.** They were the surfaces receiving nothing but flat ambient. Floor
  and ceiling, already lit directly, gained least.
- **Two of the three gate views barely change** — seated-TV is ×1.00 and focused-laptop ×1.04.
  Both are narrow-FOV close-ups framed on emissive screens, with almost no GI-lit surface in
  frame. That is expected, not a failure.

**Honest miss:** the couch carries the room's strongest normal map and still reads at 2.3%. That
corner is genuinely dark and bounce does not rescue it. See R10 in §6.

## 4. R7 — localised wear (`df80c71`) — PARKED, did not deliver

Intent: break the uniformity of tiling surface maps by tying dirt to causes the room's
construction supplies — a radiator, a cold window, one walking lane, gravity. Shipped: skirting
grime on all four walls, radiator rust, window condensation, floor traffic path, stool scuff,
conduit drip.

**It is very nearly invisible: 1.92% of pixels changed, against 1.69% before it.** Parked on my
recommendation; Allen accepted. Two reasons, and the first is a planning error of mine:

1. **Placement vs camera.** I placed wear against physical causes without checking those causes
   against what the cameras see. The standing camera sits at y = 1.64 looking **level** with a
   68° vertical FOV, so the floor only enters frame from z = +1.03 — the near 2.4 m of the
   traffic path is below the frame — and the couch/bunk assembly occludes half the far-wall
   skirting.
2. **The technique may be wrong.** Generated quads were chosen because URP's Decal Renderer
   Feature is not configured and enabling it is a shared-renderer change across three worktrees.
   That constraint is still true; see §5.2.

Correction for the record: I twice reported wear footprints of ~10.7% as progress. Those were a
rendering artifact, not wear. An A/B isolating the element showed the true figure is 1.92%.

What R7 *did* leave behind is sound: wear is excluded from the probe bake, decals no longer cast
shadows, stains now multiply rather than overlay, and a fluorescent conduit that had been feeding
bare ceiling since the Phase 6 light move now reaches the fixture.

## 5. Open questions for you

### 5.1 Does ceiling soot matter enough to keep pursuing?

A soot halo above the failing fluorescent is held back. Its *placement* is well motivated and the
ceiling is the room's most visible surface — but it currently renders as a hard-edged rectangle
for reasons I have not explained, and the ceiling is also the one surface where relief already
reads well (§2). I will not trade it for a stain on a guess.

**Your call:** is ceiling staining important to the direction, or is a clean ceiling with strong
raking light the better read? If it matters, it becomes an integration task; if not, it is
dropped and R7 stays parked as-is.

### 5.2 URP Decal Renderer Feature — deferred to the next integration window, with your input

Proper decals would remove the whole class of problems R7 hit. The cost is a shared-renderer
change affecting the room, TV and SureThing slices simultaneously. Deferred by the orchestrator
to the integration window.

**Your input wanted on:** whether localised wear is important enough to the approved direction to
justify a shared-renderer change, or whether the room reads well enough without it.

### 5.3 How far from the concept are we willing to sit?

The concept render (`concepts/concept-b-tactile-pressure-box.png`) shows light arriving from
several grazing directions at once. The room has one overhead tube plus three local sources. R6
closed much of that gap on the walls. The remaining distance is mostly lighting, and the two
levers for it are already with you as R9/R10 (see §6).

**Your call:** is the current state close enough to sign off as the refinement target, or is the
concept still the bar?

## 6. Already routed, not duplicated here

**R9 (ambient rebalance)** and **R10 (couch-corner grazing source)** were routed to you on
2026-07-31 in `docs/6-memo/2026-07-31-room-to-design-director-R9-R10.md`. Both are Candidates,
neither is built. R9 needs an 8/8 gate re-run because ambient is a global exposure lever over a
signed-off value structure. R10 carries an explicit warning that four previous attempts at that
class of fix were reverted for lighting a bunk mattress.

## 7. Constraints that survived every change

Worth stating because they bound anything you ask for next.

| Constraint | Status |
|---|---|
| Bunk 2 "legible as occupied, never legible as empty" | held — mattress luminance 43.7 → 43.9 → 43.97 across R5/R6/R7 |
| Collision and walkable clearance unchanged | 27 colliders throughout; dressing and wear add none |
| Deterministic, idempotent rebuild | held — nothing in the scene is hand-authored |
| Emissive screens (TV, laptop, phone, window, indicator) | all intact |

Two of these are load-bearing on any lighting request: **anything that lights the second bunk
breaks a ratified requirement**, and that is exactly what makes the couch corner (R10) hard.

## 8. Deeper detail, if you want it

- `docs/room-visual-pass/PHASE_A_FINDINGS.md` — R5, the sin θ measurement, two rejected hypotheses
- `docs/room-visual-pass/PHASE_B_INDIRECT_LIGHT.md` — R6, full method and numbers
- `docs/room-visual-pass/R7_WEAR_PLAN.md` — R7 as planned, including why decals were ruled out
- `docs/room-visual-pass/SIGNOFF.md` — the 2026-07-28 acceptance this all builds on
