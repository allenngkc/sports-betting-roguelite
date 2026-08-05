# Room Visual Pass — Phase 5 acceptance gates

**Direction:** B — Vice Grip, stylised, Palette 1
**Status:** pass complete, pending Allen's sign-off
**Evidence:** `after-phase4/` (edit mode), `final-playmode/` (real Play Mode), vs `baseline/`

## Gate results

| # | Gate | Result | Evidence |
|---|---|---|---|
| 1 | Builder runs clean twice | **PASS** | both runs exit 0, no exceptions |
| 2 | Exactly one of every root object | **PASS** | `RoomArtRoot`, `RoomArtGenerated`, `RoomPostFx`, 4 lights, 3 dressing groups — all count 1 |
| 3 | Dressing adds zero colliders | **PASS** | 24 colliders, byte-identical set to Phase 3 |
| 4 | Collision dimensions unchanged | **PASS** | every collider at its original world size |
| 5 | No dangling asset references | **PASS** | 0 × `m_Mesh: {fileID: 0}` after orphan cleanup |
| 6 | Laptop/desk UI readable | **PASS** | Play Mode capture, SureThing board sharp |
| 7 | TV readable at seated 17° | **PASS** | Play Mode capture, headline + subtitle + metadata row all legible |
| 8 | Walkable clearance | **STRUCTURAL ONLY** | see gap below |

### On gate 1 and byte-identity

Two rebuilds produce different SHA-256 hashes for `Room.unity`. That is **not** a failure —
Unity reassigns internal fileIDs per build. Byte-identity is the wrong gate for a generated
Unity scene; structural equivalence is the right one, and object counts match exactly across
rebuilds (76 GameObjects, 24 BoxColliders, 50 MeshRenderers, 6 Lights).

### On gate 7

Bloom puts a phosphor halo around the bright green headline. Letterforms stay intact and the
halo arguably suits a TV read. **Caveat for the theater workstream:** the small metadata row is
the tightest case and sits closest to the bloom threshold. If the TV becomes a much brighter
blue/magenta stadium-LED emitter, that row is the first thing that will suffer — see
`TV_LIGHT_RIG_ANSWER.md`, the bloom profile is a single shared global volume.

## Open gaps — stated, not hidden

1. **Walkable clearance is proven structurally, not playtested.** Every collider is verifiably at
   its original world dimensions and the dressing layer adds none, so the room's physical shape
   is provably identical to the graybox. Nobody has actually walked it. That needs a human.
2. **The room still reads green.** `TvLight` is `#59FF80` until the stadium-LED work lands.
   Once the TV becomes blue/magenta the olive/blue clash the direction was approved on will
   appear; today's captures show the pre-change state.
3. **Grime is still on the subtle side** even after the 4b contrast raise. If it should read
   harder, the lever is map contrast (currently 1.25–1.55), not more geometry.

## Cleanup outstanding at sign-off

- Delete `Assets/SBR/Editor/RoomViewCapture.cs` (+ meta) — disposable capture harness. Kept for
  now because it is the only way to regenerate the evidence captures.
- 12 orphaned chamfer meshes from the earlier bevel passes: **already cleaned** (38 → 26).

## What the pass delivered, end to end

| Phase | Delivered |
|---|---|
| 0 | Post-process volume + profile; floor/wall/ceiling split from one shared material |
| 1 | Persistent `RoomArtRoot.prefab`, deterministic instantiation, one-root assert |
| 2 | Fluorescent-key lighting rig, gradient ambient, window light |
| 3 | Procedural chamfered meshes for all 21 boxes, world-scale UVs, collision preserved |
| 4 | Tileable surface maps, conduit network, window surround + night city, clutter |
| 4b | Tuning: ambient −38%, tighter cone, warmer floor, map contrast |
| 5 | Acceptance gates, orphan cleanup, Play Mode readability verification |

Headline finding of the whole pass: **post-processing never existed in this project.** URP wires
it on `PC_Renderer` but every camera defaults to ignoring it, so the graybox rendered raw linear
output with no tonemapping, bloom or grade. That single flag was worth more than any prop.
