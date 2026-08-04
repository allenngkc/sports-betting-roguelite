# Room Visual Pass — SIGN-OFF

**Status:** ACCEPTED
**Signed off by:** Allen, 2026-07-28
**Direction:** B — "Vice Grip", stylised, Palette 1
**Branch:** `room-refinement` · **Commits:** `ba7391f` (phases 0–5), `588f84e` (layout + grade)
**Merge:** pending — Allen merging separately

This document is what formally accepts the pass. It is the authority for what was built, what was
decided, and what remains open under someone else's name.

## Acceptance gates — 8 of 8 pass

| # | Gate | Result | Evidence |
|---|---|---|---|
| 1 | Builder runs clean twice | PASS | both runs exit 0 |
| 2 | Exactly one of every root object | PASS | art root, post-fx volume, all lights, all dressing groups |
| 3 | Dressing adds zero colliders | PASS | collider set unchanged by the dressing layer |
| 4 | Collision dimensions unchanged | PASS | every collider at its original world size |
| 5 | No dangling asset references | PASS | 0 × `m_Mesh: {fileID: 0}` after orphan collection |
| 6 | Laptop/desk UI readable | PASS | Play Mode capture |
| 7 | TV readable at seated 17° | PASS | Play Mode capture, ladder + metadata row legible |
| 8 | Walkable clearance | PASS | **Allen playtested it in-editor** |

Gate 8 is the one that could not be proven from the serialized scene. Everything else is
structural; walkability needed a human and got one.

Collider count moved 24 → 27 across the pass. That delta is exactly the three bunk-2 boxes, which
are functional geometry built through the builder. The dressing layer's contribution is zero,
which is its entire contract.

## What shipped

| Phase | Delivered |
|---|---|
| 0 | Post-process volume + profile; floor/wall/ceiling split out of one shared material |
| 1 | Persistent `RoomArtRoot.prefab`, deterministic instantiation, one-root assert |
| 2 | Fluorescent-key lighting rig, gradient ambient, window light |
| 3 | Procedural chamfered meshes for every box, world-scale UVs, collision preserved |
| 4 | Tileable surface maps, conduit network, window surround + night city, clutter |
| 5 | Acceptance gates, orphan collection, Play Mode readability verification |
| 6 | Two-bunk layout, riveted display housing, radiator, desk lamp, window short-throw |
| — | Unified grade (TV sweat slice's spec, implemented in this scene) |

All art is generated. No hand-authored assets, so design/05's "nothing in the scene is ever
hand-authored" now holds for art as well as geometry.

### Findings of record

- **Post-processing never existed in this project.** URP wires post-process data on `PC_Renderer`
  but every camera defaults to ignoring it, so the graybox rendered raw linear output — no
  tonemapping, bloom or grade. That single flag was worth more than any prop.
- **Meshes are built at true world size with `localScale` 1.** Scaling a unit cube stretches its
  bevel, so a thin wall and a chunky post would carry visibly different edge widths.
- **World-scale UVs (1 unit = 1 UV)** make material tiling literally repeats-per-metre and give
  uniform texel density for free.
- **The room's palette laws were revoked** (`DECISIONS.md`, 2026-07-25). Four repo documents still
  assert the superseded cool-blue and money-colour rules.

## Removed at sign-off

`Assets/SBR/Editor/RoomViewCapture.cs` — the disposable capture harness. It was kept through the
pass because it was the only way to regenerate evidence; sign-off is what retires it. The
production Editor set is now `GrayboxRoomBuilder`, `ChamferedBoxMesh`, `ConduitMesh`,
`ProceduralSurfaceTextures`, `RoomArtDressing`.

If captures are ever needed again, the harness is recoverable from `588f84e`.

## Open, and owned elsewhere

Neither blocks this sign-off; both are in `docs/6-memo/2026-07-27-room-to-tv-sweat.md`.

1. **`TvLight` is still green `#59FF80`.** The TV slice's own 2026-07-27 note specifies cold
   white-grey. Until it changes, the new steel housing reads green rather than painted metal.
2. **The TV canvas cannot carry HDR values.** Until it can, the bright tiers will not bloom as
   light sources, and the grade's §2 black-floor fix only reaches the areas the canvas leaves
   transparent.

Two corrections were sent back to the grade spec and are worth not re-deriving:

- It names Shadows/Midtones/Highlights for lifting blacks. That component multiplies, and any
  multiplier times pure black is still pure black. The additive lift lives in `LiftGammaGain`.
- URP scales lift roughly 7× harder than the raw value implies. The spec's starting value produced
  a flat mid-grey panel that failed the spec's own §5 ladder checks.

## Evidence

All captures live in **`artifacts/room-visual-pass/`**, which is now tracked — `artifacts/` was
un-ignored on 2026-07-28, having been inherited from a .NET SDK convention this repo does not use
and silently swallowing design evidence.

| Folder | What |
|---|---|
| `baseline/` | the graybox, before anything |
| `concepts/` | the A/B/C round that chose the direction |
| `after-phase2/` `after-phase3/` `after-phase4/` `phase6/` | per-phase progression |
| `final-playmode/` | ungraded Play Mode set |
| `graded/` | final, graded — the current state |

This directory holds the written record only; captures are not duplicated here.
