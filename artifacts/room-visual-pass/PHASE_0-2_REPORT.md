# Room Visual Pass — Phases 0–2 report

**Direction:** B — Vice Grip, stylised, Palette 1 (approved 2026-07-25)
**Scope run:** Phases 0–2 of 5 (Allen approved this scope, review before mesh work)
**Branch:** `room-refinement` · **Builder runs:** clean, twice
**Evidence:** `baseline/*.png` vs `after-phase2/*.png`, identical camera poses

## What shipped

| Phase | Work | Status |
|---|---|---|
| 0 | Global post-process volume + profile asset; camera post-processing enabled | done |
| 0 | Floor / wall / ceiling split into three materials with per-surface smoothness | done |
| 1 | Persistent `RoomArtRoot.prefab`, deterministic instantiation, one-root assert | done |
| 2 | Lighting rework: fluorescent key, bounce, window, gradient ambient | done |

### Files touched

- `Assets/SBR/Editor/GrayboxRoomBuilder.cs` — owned
- `Assets/SBR/Editor/SBR.Game.Editor.asmdef` — added Core + Universal RP references
- `Assets/SBR/Editor/RoomViewCapture.cs` — **disposable harness, delete at sign-off**
- `Assets/SBR/Environment/PostFx/RoomVolume.asset` — new
- `Assets/SBR/Environment/Prefabs/RoomArtRoot.prefab` — new, empty groups
- `Assets/SBR/Materials/{FloorWorn,CeilingStained}.mat` — new
- `Assets/SBR/Materials/*.mat` — repalettes of existing
- `Assets/Scenes/Room.unity` — rebuilt by the builder

No forbidden file was modified beyond the builder's two pre-existing writes
(`ProjectSettings/TagManager.asset`, `ProjectSettings/EditorBuildSettings.asset`).

## Key numbers

Post-processing did not exist in this project before this pass. URP wires post-process data on
`PC_Renderer`, but every camera defaults to ignoring it, so the graybox rendered raw linear
output — no tonemapping, no bloom, no grade.

| | Before | After |
|---|---|---|
| Shell materials | 1 shared (`#1A1A1E`) for floor + ceiling + 4 walls | 3 distinct, mid-dark range |
| Smoothness | 0.15 flat on all 8 materials | 0.04–0.35 per surface role |
| Ambient | Flat `#0D0E13` | Trilight, sky `#25231A` → ground `#0F0D0A` |
| Key light | none (moon directional 0.25) | `FluorescentKey` spot, 9.0, 96°, shadowed |
| Post-processing | none | Neutral tonemap, bloom, grade, vignette, grain |

## Honest assessment

**What the evidence proves:** the room is no longer an undifferentiated black box. Floor, walls
and ceiling now separate by value and hue, the couch and bunk read as objects with structure,
and the fluorescent reads as a real practical throwing a directional pool. The value structure
now supports art. That was the Phase 0–2 objective and it is met.

**What it does not prove:** the room does not yet read as Vice Grip. There is no grime, no
chunk, no conduit, no clutter — all of which are Phases 3–4. Judge this capture on lighting and
material structure only.

### Risk that got worse, not better

`TvLight` is still effectively the room's co-key. It is a point light at `#59FF80`, intensity
0.5, range 3.2, re-driven every frame by `TvLight.cs` — a forbidden file. Raising the
fluorescent to 9.0 narrowed the gap but did not close it: the right half of the room is still
substantially green, and green plus the tube's yellow lands close to the monochrome-olive
problem already flagged against the approved concept.

Room-side mitigations are largely spent. The remaining levers are outside this workstream:
either the TV idle state changes (Allen has called it a placeholder), or the room accepts a
green co-key as canon. **This needs a decision before Phase 3**, because mesh and material work
downstream of it is expensive to redo.

## Verification

- Builder run twice, clean both times; art-root assert passed both runs.
- Scene contains exactly one `RoomArtRoot`, one `RoomPostFx`, one `FluorescentKey`.
- `m_AmbientMode: 1` (Trilight), `m_RenderPostProcessing: 1`, SMAA high.
- Laptop/desk view captured in Play Mode: SureThing UI sharp and fully readable, no
  regression from post-processing or SMAA.

### Gap

Seated TV/couch readability was **not** verified in Play Mode. The play-mode capture path is
unreliable in batch (see below) and the one successful run was overwritten before inspection.
This is a Phase 5 acceptance-gate item and is deferred there deliberately, not silently.

## Unity batchmode traps hit (for whoever runs this next)

1. **`-executeMethod` is silently dropped if scripts compile on the same run.** The log ends at
   `Begin MonoManager ReloadAssembly` and the process exits 0 having done nothing. Always run a
   warm compile pass first, then the real invocation.
2. **Chained Unity invocations collide on the project lock.** The second process exits
   immediately after `Successfully changed project path` with return code 1 and a ~22-line log.
   Wait for the process to disappear *and* `Temp/UnityLockfile` to clear between runs.
3. **Exit code 0 does not mean the method ran.** Verify against artifacts on disk, never the
   exit code.
4. **Play Mode capture in batch is unreliable** — `EnterPlaymode()` triggers a domain reload
   that drops the harness even with `SessionState` re-hooking. `CaptureEditMode` avoids it and
   is valid for lighting/material evidence, but shows no live screen UI.
