# Room Visual Pass — Phase 4 report

**Direction:** B — Vice Grip, stylised, Palette 1
**Scope:** Phase 4 of 5 — dressing, surfaces, conduit, window.
**Evidence:** `after-phase4/*.png` vs `after-phase3/` vs `after-phase2/` vs `baseline/`.

## What shipped

Three new files, all generated and deterministic. No hand-authored assets, consistent with the
project's design/05 rule.

| File | Owner | What |
|---|---|---|
| `ProceduralSurfaceTextures.cs` | delegated | Tileable plaster / worn floor / ceiling stain / fabric weave |
| `ConduitMesh.cs` | delegated | Tube generator with parallel-transport frames + sagging cable |
| `RoomArtDressing.cs` | mine | Routing, window surround, night-city view, clutter, wiring |

### Architecture

Generated dressing builds into a fresh `RoomArtGenerated` root each build, **not** into the
`RoomArtRoot` prefab instance. That keeps generated content and any future hand-authored prefab
content separate so neither clobbers the other. Everything in the dressing layer is
**collider-free** — the room's physical shape is still exactly what the graybox established.

### Surface maps

Tiling is expressed as repeats-per-metre, which works because Phase 3's meshes carry world-scale
UVs. These are real physical sizes, not arbitrary numbers:

| Surface | Map | Scale |
|---|---|---|
| Walls | plaster mottling + patched repairs | 1 tile / 1.3 m |
| Floor | directional wear + scuffs | 1 tile / 1.7 m |
| Ceiling | water-stain blotches | 1 tile / 2.0 m |
| Couch | coarse woven fabric | 1 tile / 17 cm |

### Conduit

Real routing, not decorative squiggle — every run starts at a plausible source and ends at
something drawing power: main ceiling run + wall drop to the TV with 8 fixing clamps, a branch
feeding the fluorescent, an older secondary ceiling line, two junction boxes, and two sagging
cables (TV drop to floor socket, desk tangle).

## Honest assessment

**The window is the win.** It went from a dead blue rectangle to the emotional anchor of the
frame — recessed frame, proud sill, and a real skyline of scattered amber windows beyond. It now
does the job the concept wanted: somewhere the player is not.

**The conduit landed.** Ceiling run, clamps, junction box and wall drop read exactly as the
concept's signature detail, and the run catching the fluorescent along its top edge is the single
best-looking element in the shot.

**The room is now too bright and too evenly lit.** This is the real defect. Vice Grip is
compressed and oppressive with light pooling and fast falloff to near-black. What is on screen is
a fairly uniformly lit room with strong yellow-green across most surfaces. Causes, in order:

1. The Phase 2 ambient raise (sky `#25231A`) was tuned when the room was flat and untextured. Now
   that surfaces carry detail and the dressing adds form, it is over-lifting everything.
2. Wall base colour was nudged up ~6% to compensate for the maps darkening it. Combined with the
   ambient, the walls are reading as the brightest thing in frame rather than the tube's pool.
3. The fluorescent's 96° cone at 9.0 spreads too evenly for a "pool" read.

**The floor and couch are under-served.** The floor is a muddy dark blue-grey that neither
matches the warm walls nor shows its wear map. The couch mass is dark enough that the fabric
weave does not read at all.

**Grime is too subtle.** At standing distance the plaster mottling reads as gentle noise rather
than dirt. The direction wants patches, stains and wear that are legible as *story*, not texture.

## Recommended tuning (Phase 4b)

1. Drop ambient back roughly 35–40% across all three Trilight bands.
2. Return wall base to the pre-map value and let the maps darken it.
3. Narrow the fluorescent cone to ~78° and raise intensity slightly, for a defined pool with
   faster falloff.
4. Lift the floor's base value and warm it so it belongs to the same room as the walls.
5. Raise contrast on the plaster and floor maps so grime reads as story at 3 m.

## Not yet done

- Phase 5 acceptance gates: rebuild-twice re-verification, walkable-clearance playtest, seated
  TV readability in Play Mode.
- 12 orphaned mesh assets from the earlier bevel pass, plus `RoomViewCapture.cs`, to be deleted
  at sign-off.
