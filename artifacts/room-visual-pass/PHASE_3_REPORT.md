# Room Visual Pass — Phase 3 report

**Direction:** B — Vice Grip, stylised, Palette 1
**Scope:** Phase 3 of 5 — geometry. Phases 0–2 in `PHASE_0-2_REPORT.md`.
**Evidence:** `after-phase3/*.png` vs `after-phase2/*.png` vs `baseline/*.png`, identical poses.

## What shipped

`Assets/SBR/Editor/ChamferedBoxMesh.cs` — a procedural chamfered-box generator, plus a rewrite
of `GrayboxRoomBuilder.Box()` to use it. Every renderable box in the room is now a generated
mesh; **zero builtin primitive cubes remain in the scene.**

### Three decisions worth recording

1. **Meshes are built at true world size, `localScale` stays 1.** Scaling a unit cube stretches
   its bevel, so a 0.1m wall and a 0.42m couch would end up with visibly different edge widths.
   Generating per size keeps the chamfer a constant physical width across the whole room.
2. **Bevel auto-scales to each object's smallest dimension** — `clamp(min * 0.18, 0.004, 0.050)`.
   No per-call-site tuning, and it degrades safely: the guard `bevel <= min(size) * 0.45` stops
   thin objects collapsing. The phone chassis lands at 0.0036m, the couch at the 0.05m cap.
3. **World-scale planar UVs (1 unit = 1 UV) on every face.** This hands Phase 4 uniform texel
   density for free, which is the thing that normally goes wrong when texturing mixed-size props.

### Bevel results

| Object | Size (m) | Bevel (m) |
|---|---|---|
| Couch seat | 0.70 × 0.42 × 1.80 | 0.050 (cap) |
| Stool | 0.35 × 0.45 × 0.35 | 0.050 (cap) |
| Mini-fridge | 0.50 × 0.85 × 0.50 | 0.050 (cap) |
| Floor / ceiling | 2.80 × 0.10 × 4.20 | 0.018 |
| Long walls | 0.10 × 2.30 × 4.00 | 0.018 |
| Bunk post | 0.06 × 1.54 × 0.06 | 0.0108 |
| Desk leg | 0.05 × 0.71 × 0.05 | 0.009 |
| Laptop base | 0.22 × 0.02 × 0.32 | 0.004 |
| Phone chassis | 0.075 × 0.008 × 0.15 | 0.0036 |

26 mesh assets on disk; 21 boxes resolve to 14 unique meshes at the current bevel setting
(the four desk legs and two bunk posts each share one). The 12 assets from the first, more
conservative bevel pass are now orphaned and can be deleted at sign-off.

## Collision: verified unchanged

This was the phase's main regression risk. The old primitive carried a unit `BoxCollider` scaled
by the transform; the replacement carries an explicit `BoxCollider` of the same world dimensions
with `localScale` back at 1. Verified against the saved scene — 24 colliders, every one at its
original size:

- long walls `0.1 × 2.3 × 4`, end walls `2.8 × 2.3 × 0.1`, floor/ceiling `2.8 × 0.1 × 4.2`
- couch seat `0.7 × 0.42 × 1.8`, bunk slab `0.8 × 0.08 × 1.9`, backrest `0.15 × 0.4 × 1.8`
- 4 × desk leg `0.05 × 0.71 × 0.05`, 2 × bunk post `0.06 × 1.54 × 0.06`
- stool `0.35 × 0.45 × 0.35`, fridge `0.5 × 0.85 × 0.5`, desktop `0.5 × 0.04 × 1.1`
- TV body `0.06 × 0.65 × 1.1`, laptop `0.22 × 0.02 × 0.32`, phone `0.075 × 0.008 × 0.15`
- plus the 3 interaction triggers (couch, laptop, phone), unchanged

21 box meshes + 3 triggers = 24. Walkable clearance, interaction rays and the
CharacterController are structurally identical to the graybox.

**Not yet playtested.** Structural equivalence is proven from the serialized scene; nobody has
walked the room. That belongs at the Phase 5 gate.

## Honest assessment

**Working:** the chamfers read. The stool has clearly defined chunky corners, the desk lip and
couch front edge catch bevel highlights, the bunk slab has a rim, the TV bezel has a rounded
profile. Forms now read as solid objects with thickness instead of flat planes — which is
exactly what a sharp 90° edge cannot do, and it is the "stylise through form" rule working.

**Still missing:** the room does not read as Vice Grip. There is no grime, no surface texture,
no conduit, no clutter, no window treatment. All Phase 4. Judge this capture on silhouette and
edge definition only.

**Gain is real but modest at this camera.** At 68° from ~3m the bevels are most legible on the
stool and desk edge. The first pass at `0.12 / 0.03m` was too conservative and read as thin
highlights; `0.18 / 0.05m` is the tuned value. Going much chunkier would start to look
deliberately toy-like rather than heavy.

## The blocker, now third time raised

`TvLight` still owns the right half of the frame in green. Phase 3 changed geometry, not colour,
so this is exactly as it was. Phase 4 is where colour gets baked into materials, decals and
grime — **this is the last cheap moment to decide.** Options unchanged from the Phase 0–2 report:
accept green as canon and lean the room warmer so green reads as intrusion, or the TV idle state
stops being a large green emitter.
