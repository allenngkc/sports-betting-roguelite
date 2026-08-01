# R7 — localised wear, decals, contact grime (plan)

**Date:** 2026-07-31 · **Register item:** R7, approved direction · **Source:** `docs/handoffs/room-refinement.md` §6B
**Status:** plan only. No Unity run performed. Implementation needs an editor lease.

---

## 1. Why this is the right next item

R6 (indirect light, `fb44ac2`) **created** the need for R7.

Before it, the corners, the undersides of both bunks, the skirting line and the back of the
radiator were near-black. Their lack of dirt was invisible because *they* were invisible. Bounce
light now reaches all of them, so the remaining tell is no longer "this room is flat" — it is
"this room is uniformly dirty".

That is exactly the failure §10 of the handoff names: *wear follows construction and contact
rather than looking like uniform noise*. Every surface map today is a tiling field applied at
world scale, so a wall is identically grimy at the ceiling, at eye height and at the skirting.
Real rooms are not. Dirt has causes, and this room's construction supplies them: a water
fixture, a cold window, one walking lane, and four things people touch.

R7 is therefore about **localisation**, not more texture. Do not answer it by raising map
contrast — that is the lever Phase A measured as exhausted.

## 2. Technique decision

**URP decals are not configured.** `PC_Renderer.asset` carries exactly one renderer feature,
`ScreenSpaceAmbientOcclusion` (intensity 0.40). Adding the Decal Renderer Feature is a shared
renderer change affecting all three worktrees, and §6B explicitly prefers the low-risk path
absent existing support. **Decision: generated quads and thin geometry, no URP decals.** Revisit
only if R7 proves the quad path insufficient.

Three techniques, chosen per effect rather than one for everything:

| Technique | Use for | Why |
|---|---|---|
| **Alpha-clipped opaque quad** | streaks, drips, scuffs, chips, traffic path | writes depth, so it receives SSAO and sorts correctly; hard-ish edges are correct for dirt |
| **Thin opaque geometry** (`ArtBox`) | skirting grime, contact bands | zero new tech, uses the proven chamfered-box path, gives the junction a real edge |
| **Transparent quad** | damp blooms only | soft falloff is the whole point; accepts no SSAO, so keep the count in single digits |

Surface offset **2–3 mm** along the face normal. The meshes are true world size with no
transform scaling, so this is a literal millimetre figure, not a scale-dependent fudge.

New generator `ProceduralWearTextures.cs` alongside the existing surface pipeline: streak/drip
masks, edge-dirt gradients, blotch masks. Same discipline as `ProceduralSurfaceTextures` —
deterministic seed, tileable where tiled, stable across rebuilds.

## 3. R7.0 — prerequisite, must land first

**`MarkStaticForGI` will pull every wear quad into the probe bake.** It marks every `MeshRenderer`
in the scene except those under `Player`. Thin quads floating 2 mm off a wall are poor GI
geometry: they act as occluders at probe scale, and can invalidate or leak the probes behind
them. The room's indirect light would degrade as a side effect of adding dirt, which would be a
confusing regression to diagnose after the fact.

**Fix before any quad lands:** parent all wear under a dedicated root and skip that subtree in
the sweep, the same way `Player` is skipped. Wear contributes nothing to bounce and should not
pretend to.

This is the single hard technical dependency in R7. Everything else is content.

## 4. Work items

Coordinates are the builder's own. Interior: floor top `y=0`, inner wall faces `x=±1.3` and
`z=±2.0`, ceiling inner face `y=2.3`. Door end is `-Z`, window wall is `+Z`, couch is `-X`,
desk and display are `+X`.

### Tier 1 — the four that carry the read

**R7.1 Skirting grime.** A 60–100 mm dark band along every floor/wall junction, thin opaque
geometry. Highest value single item: it is the longest continuous line in the room, it is now
lit by bounce, and it is the most reliable real-world dirt gradient there is. Breaks the "same
grime everywhere" tell in one pass. Must stay collider-free.

**R7.2 Radiator rust and damp.** The radiator is a water fixture: body at `(0, 0.34, 1.88)`,
feed pipe at `(0.36, 0.16, 1.86)`. Rust streak descending from the pipe joint to the floor; a
damp bloom on the far wall behind and above the body; corrosion darkening at the fin roots.
Best-motivated wear in the room — the cause is visible in the same frame as the effect.

**R7.3 Window condensation run.** Pane at `(0, 1.4, 1.99)`, a cold surface on an exterior wall
above a heat source. Condensation runs down the reveal onto the sill, darkens the sill's front
lip, and streaks the wall below toward the radiator. Ties R7.2 and R7.3 into one damp story
rather than two unrelated stains.

**R7.4 Floor traffic path.** The walkable lane is bounded by the couch front face (`x=-0.60`)
and the desk legs (`x=0.855`), running the full `z` length. A worn track along it, plus a scuff
arc where the stool at `(0.55, 0.225, 1.45)` gets pushed in and out. This is the one piece of
wear that implies a person without showing one.

### Tier 2 — contact

**R7.5 Contact polish.** Darker, smoother, slightly higher-gloss bands where hands and bodies
actually land:
- couch seat front edge, `x=-0.60`, `y=0.42`, spanning `z=-0.6..1.2`
- couch backrest top, `y=0.82`
- desk top front edge, from `DeskTop` at `(1.05, 0.73, 1.45)`
- bunk posts at hand height `y≈1.0–1.3` — `x=-0.53` (`z=-0.62`, `1.22`) and `x=0.53` (`z=0.57`, `1.93`)

Contact wear is *the opposite* of grime — it removes dirt and raises gloss. Getting that
inversion right is what separates this from more noise.

### Tier 3 — if the budget holds

**R7.6** Soot halo on the ceiling above the fluorescent at `(0.85, 2.05, -0.05)`; localise what
the ceiling map currently spreads everywhere.
**R7.7** Paint chips on struck edges: display housing corners, radiator fin edges, conduit
clamps.
**R7.8** Mini-fridge drip stain under `(-0.95, 0.425, -1.65)`.

Stop at Tier 2 if Tier 1 already reads. The approved dressing gate is *restrained — a few heavy
pieces, not a trash mountain*, and that constraint governs wear too.

## 5. Explicitly out of scope

- **R9 ambient rebalance** — Candidate, routed to the Design Director. Not tweaked here, even
  though R7 would benefit from it.
- **R10 couch-corner grazing source** — Candidate, routed. R7.5's couch contact wear will
  under-read until R10 lands; that is accepted, not worked around.
- **Grade tuning.** Per the C2 interim ruling, the shipped TV green is temporary and the target
  is `DESIGN.md` §5 cold white-grey at TV Phase 3. **Do not tune the grade or any wear colour
  against the current green.** Wear near the display must be authored neutral so it survives
  the colour correction.
- **Geometry detail (R8)** stays last priority and is not pulled forward.

## 6. Validation gate

The handoff §9 gate plus one new mandatory step:

> **Build → bake → capture.** R7 changes scene geometry, so the probe bake is stale the moment
> the builder runs. `SBR/Build Graybox Room` then `SBR/Bake Room Indirect Light`, always both.
> A built-but-unbaked room still looks like a room, just flat — this fails silently.

Then: exactly one `RoomArtRoot`, dressing adds **zero** colliders (expect **27** total),
no dangling mesh refs, the five emissive materials still at `m_LightmapFlags: 1`, and the three
gate views at 68° / 17° / 30° captured in Play Mode for comparison against
`artifacts/room-visual-pass/apv/`.

Two acceptance measures specific to R7:

1. **Localisation, not uniformity.** Relief/darkening variance *between* regions of the same
   material should rise. If skirting, mid-wall and ceiling-junction bands still measure the
   same, the wear has not localised and the item has failed regardless of how it looks.
2. **Bunk 2 stays dark.** Mattress mean luminance held at 43.9 through R6; wear must not lift
   it. This is the ratified constraint two earlier attempts were reverted for.

## 7. Sequencing

1. R7.0 GI opt-out — no visual change, lands alone, verified by an unchanged bake.
2. `ProceduralWearTextures.cs` — generator plus one test decal, verified in isolation.
3. Tier 1, one build/bake/capture cycle for all four.
4. Tier 2, second cycle.
5. Tier 3 only if Tier 1–2 measured well.

Each cycle needs a Unity editor lease (~6 min: build ~2, bake ~3, capture ~1). Announce to the
orchestrator before launching — one editor instance studio-wide.
