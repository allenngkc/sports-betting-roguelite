# Phase B — indirect light (Adaptive Probe Volumes)

**Date:** 2026-07-30 · **Scope:** bounce lighting via URP Adaptive Probe Volumes
**Outcome:** shipped. Surface relief now reads on the walls — measured **×4–5** on the right
wall, **×1.67** across the whole standing view — at **no cost in exposure** and with every
ratified constraint intact.

This is the lever [PHASE_A_FINDINGS.md](PHASE_A_FINDINGS.md) §4 identified and deferred. It
worked, and it worked for the reason predicted there.

---

## 1. What was wrong

A URP realtime light bounces nothing. A surface it does not hit directly received the flat
`RenderSettings` ambient value and nothing else. Phase A established that a normal map can only
modulate light that *varies across it*, and that the variation comes from grazing incidence
(sensitivity ∝ sin θ) — so the room had exactly one surface where relief read at all, the
ceiling, because the tube hangs 0.25 m under it.

Flat ambient is the worst possible case: it arrives equally from every direction, so a
perturbed normal changes nothing at all. Every wall in the room was in that state.

Bounce light is the opposite. It is *directional* — light off the floor arrives at a wall from
below, which is a graze — and it comes from many directions at once, so it reaches surfaces no
lamp points at. That is what the walls needed.

## 2. What shipped

| Change | Where |
|---|---|
| `m_LightProbeSystem: 0 → 1` (Probe Volumes), `m_ProbeVolumeSHBands: 1 → 2` (L2) | `Assets/Settings/PC_RPAsset.asset` |
| `m_LightProbeSystem: 0 → 1` | `Assets/Settings/Mobile_RPAsset.asset` |
| 6 static lights `Realtime → Mixed`; `AdaptiveProbeVolume` sized to the interior | `GrayboxRoomBuilder.BuildLighting` |
| 103 renderers marked `ContributeGI` + `receiveGI = LightProbes` | `GrayboxRoomBuilder.MarkStaticForGI` |
| Headless bake entry point | `RoomLightingBake.cs` (new) |

**Mixed, not Baked.** Direct light and shadows still render in realtime, so every pool tuned in
phases 4–6 — the window's short throw, the desk lamp's cone, the tube's hard contact shadows —
is untouched. Only the *indirect* contribution is baked.

`TvLight` and `PhoneBuzzLight` stay **Realtime**. TvLight belongs to the TV sweat slice and
changes colour every frame; PhoneBuzzLight is a flash. A baked bounce from either would be a lie
the moment they change.

**Probes only, no lightmaps.** Every mesh in this room is generated at runtime and carries no
UV2, so lightmapping would need an unwrap pass per mesh. APV is volumetric and needs no UVs.
The core package's own bake driver disables lightmaps for this bake type. Nothing about the
meshes had to change.

Probe spacing is **0.25 m**. APV's 1.0 m default would put roughly three probes across a 2.6 m
room and the bounce would collapse to a single flat value — the exact failure being fixed.

## 3. Results — measured

Local relief contrast, defined as mean |ΔL| between pixels 4 apart, normalised by regional mean
luminance. A stride above the film-grain correlation length, so it measures surface detail
rather than grain. Same camera poses, both captures in Play Mode.

### Standing overview — the only gate view that shows the room

| Region | relief before | after | change |
|---|---:|---:|---:|
| Right wall (plaster) | 1.4% | 8.7% | **×6.3** |
| Far wall by window | 3.0% | 6.8% | **×2.3** |
| Couch, left, in shadow | 1.9% | 2.3% | ×1.26 |
| Ceiling | 1.4% | 1.7% | ×1.21 |
| Floor, centre aisle | 1.8% | 2.1% | ×1.17 |
| **Whole frame (4×3 grid)** | **3.69%** | **6.17%** | **×1.67** |

**Exposure did not move.** Mean luminance per region: 33.0→32.0, 38.6→38.4, 29.0→28.5,
31.7→33.0, 35.3→35.2. The room looks brighter only because texture became visible; it is not
brighter. The value structure signed off at 8/8 gates is preserved.

### The other two gate views barely change — and that is expected

`seated-tv-couch` is **×1.00** and `focused-laptop-desk` is **×1.04**. Both are narrow-FOV
close-ups (17° and 30°) framed almost entirely on emissive screens and the surfaces the desk
lamp lights directly. Neither contains much GI-lit surface, so neither had anything to gain.
Recorded so nobody later reads those two numbers as a failure.

### Where the gain landed, and why it confirms Phase A

The gain is concentrated almost entirely on the **walls** — the vertical surfaces that
previously received nothing but flat ambient. The ceiling and floor, which were already getting
direct light, improved least. That is the sin θ argument playing out in the opposite direction:
the surfaces that gained are exactly the ones that previously had no *directional* light at all.

The couch is the honest miss. It carries the room's strongest normal map (channel sd ~80) and
still reads at 2.3%. That corner is genuinely dark and bounce alone does not rescue it.

## 4. Ratified constraints — all verified intact

| Constraint | Check | Result |
|---|---|---|
| Bunk 2 "occupied, never empty" | mattress mean luminance | 43.7 → 43.9 (**+0.5%**) |
| Collision unchanged | `BoxCollider` count in scene | **27** |
| Phase A emission fix holds | the 5 emissive materials | all `m_LightmapFlags: 1` |

The bunk-2 number is the one that mattered. Two previous attempts at surface relief were
reverted for lighting that mattress; bounce light does not, because it arrives dim and diffuse
rather than as a lamp pointed at a wall.

## 5. Workflow trap — read this before rebuilding

**The builder deletes and recreates `Room.unity` on every run, which discards the bake with it.**

    SBR/Build Graybox Room    →  SBR/Bake Room Indirect Light

Always in that order, always both. A room that has been built but not baked still looks like a
room — it just has no bounce, no colour bleed and flat walls again. That is what makes this easy
to miss, so the builder now logs a warning at the end of every run.

Headless, note **no `-quit`** on the bake (it exits itself once the async bake finishes) and
**no `-nographics`** on either (dilation runs compute shaders):

```
Unity.exe -batchmode -quit -projectPath <p> -executeMethod SBR.GrayboxRoomBuilder.Build
Unity.exe -batchmode       -projectPath <p> -executeMethod SBR.RoomLightingBake.Bake
```

### One non-obvious requirement

`AdaptiveProbeVolumes.BakeAsync()` returns `false` and reports nothing else unless
`ProbeReferenceVolume.instance` is initialised — and the only thing that ever initialises it is
URP's own constructor and render loop (`UniversalRenderPipeline.cs:409` and `:922`). Unity
constructs the pipeline lazily on first render, and a plain `-executeMethod` batch run never
renders. `RoomLightingBake.WarmRenderPipeline()` renders one throwaway 64×64 frame to force it.
Without that the bake silently refuses to start.

## 6. Repository cost

The bake writes ~4.4 MB to `Assets/Scenes/Room/`. Committed deliberately: without it the scene
has no indirect light for anyone who opens it. `CellSupportData.bytes` (2 MB) is editor-only and
Unity strips it from player builds.

## 7. Next levers

1. **Rebalance ambient down.** Flat `RenderSettings` ambient is now doing work that directional
   bounce does better, and it actively suppresses relief by filling from every direction at
   once. Lowering it should raise relief further and deepen the shadows. Not done here because
   it changes the value structure that was signed off at 8/8 gates, so it needs a gate re-run
   rather than a tuning tweak.
2. **The couch corner.** The strongest normal map in the room still reads at 2.3%. It needs its
   own dim grazing source, below the bunk-1 slab so it touches neither bunk.
3. Do **not** respond to any of this by strengthening the normal maps. Phase A §2–3 measured
   that lever as exhausted, and Phase B is the evidence that lighting was the gate all along.
