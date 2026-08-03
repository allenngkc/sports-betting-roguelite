# Room refinement — lead ownership handoff

**Handoff date:** 2026-07-28  
**Ownership returned to Claude:** 2026-07-30 (Allen's call — Claude remains the leads)  
**Incoming owner:** Claude (Opus 5) acting as room art and technical lead  
**Worktree:** `C:\Users\Allen\orca\workspaces\sports-betting-roguelite\room-refinement`  
**Branch:** `room-refinement`  
**Starting HEAD:** `7d01eb74e6d09451a98d1af96362ca9ba1721f41`  
**Starting state:** clean before this handoff file was added  
**Current gate:** the first room visual pass is accepted; this is a refinement pass

> **2026-07-30 update:** Leads were handed to GPT/Codex on 2026-07-28; Allen has returned
> ownership to Claude. The body below reflects the 2026-07-28 state. Commits since then —
> `cd62855` (emission fix + full PBR surface maps), `8620c5a`/`5329c0f` (Phase A/B evidence),
> `fb44ac2` (indirect light via Adaptive Probe Volumes) — supersede the "no fresh Unity run"
> note, and §5's emission investigation appears already addressed by `cd62855`; verify rather
> than redo it. §7's image generation runs through Allen or external tools; this harness has
> no image generation.
>
> Decision routing has also changed: critical or strategy decisions escalate lead →
> orchestrator (`main-2`) → Allen, and all design decisions (visual direction, UI,
> interaction, art, 3D) belong to the Design Director — this lead implements approved
> specs and makes essentially no design calls. Where this document says "ask Allen",
> route accordingly. See `main-2/docs/5-orchestration/STUDIO.md`.

> **2026-07-31 studio briefing:**
> - A dedicated Orchestrator session (Fable 5, `main-2`) is live: it sweeps worktrees,
>   owns `main-2/docs/5-orchestration/STATUS.md`, merge order, and integration. It may
>   message this terminal via Orca; treat its dispatches as coordination — Allen's word
>   is final.
> - A Design Director seat (Claude Design) is live and inherits every existing design
>   decision; a studio design system is being built from the approved packages, and
>   future specs will cite it. Do not preempt the pending Allen rulings: C1 TV
>   "Decision A", C2 TV light-spill colour, T8 scanlines/static.
> - Report telegraphically (Done / Next / Risk / Need Allen); keep evidence local;
>   never send raw logs upward.
> - Sweep flag for this worktree: commit `handoff.md`.
> - **Delegation directive (Allen, 2026-07-31):** grunt work — implementation, testing,
>   validation, bulk reading — goes to bounded sub-agents (Sonnet 5 by default, max two
>   at once); you plan, dispatch, review diffs, and integrate. Doing sustained grunt
>   work yourself is now a contract deviation. Every dispatch names allowed files,
>   forbidden files, required evidence, and an exit gate; sub-agents never commit
>   unless the dispatch says so. Use an Opus sub-agent only for genuinely hard tasks.
> - **Autonomy update (Allen, 2026-07-31):** per-phase approval is retired. The
>   orchestrator verifies your evidence against the phase's exit criteria and advances
>   you — do not park waiting for Allen between phases. Allen still gates: new design
>   direction, scope, licensing, spend, and anything irreversible. `Need Allen` now
>   means one of those, nothing else. See STUDIO.md "Autonomy policy".

## 1. Ownership transfer

Take full ownership of this worktree. Drive the room toward a near-final vertical slice while
preserving the accepted layout, interactions, and screen readability.

Do not ask Allen to approve routine files, tests, captures, or small visual tuning. Ask only for
a material art-direction choice, scope expansion, licensed external asset, or conflict with
another worktree.

Communicate in simple telegraphic language:

- result first;
- short sentences;
- no giant walls of text;
- no raw tool logs unless Allen asks;
- finish updates with `Done`, `Next`, `Risk`, and `Need Allen`;
- use `Need Allen: nothing` when unblocked.

## 2. Current authority and status

The authoritative acceptance record is:

`docs/room-visual-pass/SIGNOFF.md`

It records Allen's 2026-07-28 acceptance of:

- Direction B — **Vice Grip**, stylised, Palette 1;
- two-bunk layout;
- riveted institutional TV housing;
- persistent `RoomArtRoot`;
- unified room/TV grade;
- all eight functional and visual gates.

Important: `artifacts/room-visual-pass/ROOM_VISUAL_SIGNOFF.md` is an older pre-approval board. It
still says implementation has not started and recommends Direction A. Do not treat it as current.

Shipped commits:

- `ba7391f` — Vice Grip visual pass, phases 0–5
- `588f84e` — two-bunk layout, display housing, unified grade
- `4650390` — sign-off, evidence, capture-harness retirement
- `7d01eb7` — tracked artifacts and palette-law reconciliation

No fresh Unity run was performed during this ownership handoff. The last recorded gate evidence
is the accepted evidence above.

## 3. Mission for this refinement

Keep the accepted room composition. Close the visible fidelity gap between:

- target language:
  `artifacts/room-visual-pass/concepts/concept-b-tactile-pressure-box.png`
- current graded runtime views:
  `artifacts/room-visual-pass/graded/`

The generated concept is a style/material reference, not layout truth. It may contain impossible
lighting or geometry. Runtime camera anchors, collision, interaction rays, screen bounds, and
walkable clearance remain authoritative.

The next gain should come mainly from surface response, wear, contact, and light transport—not
from replacing the room with generated geometry.

## 4. What already ships

- deterministic scene generation through `GrayboxRoomBuilder`;
- persistent `Assets/SBR/Environment/Prefabs/RoomArtRoot.prefab`;
- collider-free generated dressing through `RoomArtDressing`;
- procedural chamfered boxes and conduit meshes at true world scale;
- world-scale UVs;
- procedural plaster, floor, ceiling, fabric, and city textures;
- post-process volume and unified grade;
- fluorescent key/bounce, short-reach window light, desk lamp, TV light, and phone light;
- two bunks, radiator, conduit, institutional display housing, clutter, and city window;
- accepted standing, couch, and laptop compositions.

The disposable `RoomViewCapture.cs` harness was removed at sign-off. It can be recovered from
commit `588f84e` for evidence work, then removed again.

## 5. First investigation: emission is a hypothesis

The outgoing Claude lead reported a possible emission problem:

`unity/SBR/Assets/SBR/Editor/GrayboxRoomBuilder.cs:260`

```csharp
mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
```

It proposed changing this to `RealtimeEmissive`, claiming Editor import strips `_EMISSION`.
Treat that as an unverified diagnosis.

Current evidence:

- `_EMISSION` is enabled in the affected material files;
- their serialized `m_LightmapFlags` is `0`;
- `None` may suppress GI contribution without suppressing visible surface emission.

Reproduce in Editor and in a rebuilt scene. Inspect the renderer's runtime material, keyword,
emission color/map, and captured output. Add a regression check if practical. Change the flag
only if the failure is proven and the intended GI behavior is clear.

## 6. Refinement priorities

### A. Full material response

Highest priority.

- Extend the deterministic procedural texture pipeline beyond albedo.
- Add appropriate normal, smoothness/roughness, and occlusion information.
- Preserve world-scale tiling and reproducible seeds.
- Use correct Unity import settings, especially normal-map type.
- Tune per material family: plaster, worn floor, ceiling, fabric, painted steel, rust, and grime.
- Keep generated assets stable across rebuilds.

The current pipeline mainly assigns `_BaseMap` plus scalar smoothness. Do not fake the whole
improvement with stronger color noise.

### B. Localized wear and decals

- Add peeling edges, damp boundaries, rust streaks, drips, corner dirt, contact grime, and paint
  chips where the room's construction explains them.
- Keep dressing collider-free.
- Prefer the existing low-risk generated-quad approach unless URP decal support is already
  configured and the benefit justifies the integration.
- Keep TV, laptop, phone, interaction rays, and readable text unobstructed.

### C. Lighting quality

Current code creates seven lights; only the directional/window-moon light and fluorescent key
cast shadows. Improve contact and bounce deliberately.

- Preserve three distinguishable sources: warm fluorescent, short-reach cool window, quiet
  screens.
- Do not let the room become a blue wash.
- Do not compensate for weak materials by overexposing lights or bloom.
- Evaluate more selective soft shadows, light probes/APV, or restrained fake bounce.
- Treat APV, renderer, and project-setting changes as integration work, not a casual toggle.
- Recheck the unified grade after every lighting change.

### D. Geometry, last

Only add geometry where silhouette or contact clearly needs it:

- sofa cushion folds;
- radiator fins;
- pipe joints/brackets;
- welds, fasteners, and high-value edge breakup.

Prefer deterministic procedural or curated, clean assets. Do not import unreviewed
image-to-3D triangle soup.

## 7. Image-generation policy

Image generation is useful here, but keep it bounded.

Good uses:

- small art-direction sheets;
- isolated original posters, stains, fabric, paint damage, labels, and decal candidates;
- material-reference variants before committing production textures.

Bad uses:

- a flattened full-room image used as the scene;
- functional TV/laptop/phone UI;
- readable AI-generated text;
- real brands or copied game assets;
- generated models imported without topology, UV, scale, collision, and license review.

Every production candidate must be inspected, cleaned, cropped, color-corrected, made
tileable/decal-ready where needed, and checked in all three runtime views.

## 8. File ownership and conflict prevention

Primary owned files:

- `unity/SBR/Assets/SBR/Editor/ProceduralSurfaceTextures.cs`
- `unity/SBR/Assets/SBR/Editor/RoomArtDressing.cs`
- `unity/SBR/Assets/SBR/Editor/ChamferedBoxMesh.cs`
- `unity/SBR/Assets/SBR/Editor/ConduitMesh.cs`
- `unity/SBR/Assets/SBR/Environment/**`
- room-only materials
- room-specific tests and evidence

Shared integration hotspots owned by the room lead during this slice:

- `unity/SBR/Assets/SBR/Editor/GrayboxRoomBuilder.cs`
- `unity/SBR/Assets/Scenes/Room.unity`

Keep edits to those two files minimal and isolated. Tell the principal before another worktree
needs either file.

Read-only:

- `engine/**`
- `SportsbookApp.cs`, `LaptopOs.cs`, and other SureThing files
- `TvSweatScreen.cs`, theater/pacing code, and TV UI
- `RunDirector.cs`
- `ProjectSettings/**`, unless an explicit integration decision is made

Two known external TV dependencies remain owned by the TV workstream:

1. `TvLight` still produces green spill instead of the newer cold white-grey intent.
2. The TV canvas cannot yet supply HDR values for true screen bloom.

Do not solve those by editing TV files in this worktree.

To prevent documentation conflicts, do not edit shared canonical files such as
`docs/ARCHI.md`, `DECISIONS.md`, or root planning documents. Record the exact canonical update
needed for the principal integrator. Slice-local room docs and evidence are owned here.

## 9. Validation gate

After each meaningful material/lighting batch:

1. Warm-compile Unity before `-executeMethod`.
2. Run the builder twice.
3. Wait for both the Unity process and `Temp/UnityLockfile` to clear between runs.
4. Verify generated artifacts; exit code 0 alone does not prove the method ran.
5. Confirm exactly one `RoomArtRoot`, generated root, post-FX volume, and expected lights.
6. Confirm generated dressing adds zero colliders.
7. Confirm functional collider dimensions and walkable clearance are unchanged.
8. Confirm no dangling meshes or asset references.
9. Run targeted RoomSmoke, LaptopOS, and TV PlayMode coverage.
10. Capture the same standing 68°, seated TV 17°, and focused laptop 30° views.
11. Compare against the accepted captures at matching exposure and framing.

Play Mode capture in batch was previously unreliable because the domain reload dropped the
harness. Use the proven workflow or capture interactively; do not claim visual validation from
Edit Mode alone when live screen readability is part of the gate.

## 10. Definition of near-final

- walls, floor, ceiling, fabric, and metal respond as different materials;
- wear follows construction and contact rather than looking like uniform noise;
- objects feel planted through contact shadows and local occlusion;
- the warm key, local blue window, and quiet screens remain separable;
- the room reads as Vice Grip, not a graybox and not a horror cell;
- standing, couch, and laptop compositions remain functional;
- no regression to movement, interaction, collisions, or UI readability;
- rebuild remains deterministic and idempotent;
- no conflicts with TV or SureThing-owned files.

## 11. First update to Allen

Keep it short:

```text
Room handoff loaded.
Done: confirmed the accepted baseline and protected the room/TV/SureThing file boundaries.
Next: reproduce the emission claim, then start the deterministic PBR surface pass.
Risk: emission diagnosis is not yet proven.
Need Allen: nothing.
```

---

## 12. C15 — TextMeshPro migration scope for this surface

**Ruled by Allen 2026-08-02: Option 1, both surfaces migrate to TMP. Scheduled phase, not now** —
sequenced after the current conformance wave, orchestrator schedules per surface. Signed type
deviations hold until a surface migrates, then expire. Scoped here ahead of the phase, per the
ruling. **No build work has been done.**

### 12.1 The room is not one of "both surfaces" — but it owns text anyway

C15's two surfaces are the **laptop** and the **TV**. Neither is this slice. The room nevertheless
builds and owns text, so it cannot simply sit the phase out:

| what | where | render mode | current type |
|---|---|---|---|
| Interaction prompt | `InteractionHud.cs` | ScreenSpaceOverlay | 1 × `UI.Text`, `LegacyRuntime.ttf`, size 20 |
| Phone messages + badge | `PhoneScreen.cs` | **WorldSpace** | multiple `UI.Text` via its own `MakeText` |

Both are `CanvasRenderer` today.

### 12.2 The phone is unclaimed, and that needs a ruling before the phase

`BuildPhone` in `GrayboxRoomBuilder` builds the prop **and** attaches `PhoneScreen`, so the room
builds it. But §8's read-only list names SportsbookApp, LaptopOs, "other SureThing files",
TvSweatScreen, theater/pacing and TV UI — **it does not name `PhoneScreen`**, and C15's "both
surfaces" does not cover it either.

So the phone is a third text surface that no ruling currently assigns. It should not migrate by
accident, and it should not be missed because each seat assumed the other had it. **Ask before the
phase, not during it.**

### 12.3 The one real hazard, and it is a repeat

`MarkStaticForGI` sweeps **`MeshRenderer`** and marks everything it finds `ContributeGI` for the
Adaptive Probe Volume bake.

TMP ships two components. `TextMeshProUGUI` is a `CanvasRenderer` — invisible to that sweep, exactly
as `UI.Text` is today. The plain `TextMeshPro` component is a **`MeshRenderer`**, and it is the
natural-looking choice for world-space text like the phone.

**Pick that one and the phone's glyphs get baked into the probe volume.** That is R7.0 all over
again — thin quads entering the GI bake, occluding at probe scale and able to invalidate the probes
behind them — and it took a full editor lease to diagnose the first time.

Mitigation, in order of preference: use `TextMeshProUGUI` and keep the world-space Canvas; or, if
the 3D component is genuinely wanted, extend the wear-root exclusion in `MarkStaticForGI` to cover
text before the first bake, never after.

### 12.4 Other room-side constraints the phase must respect

- **The collider inventory is ratified at 29** (T53): 27 BoxCollider + 2 MeshCollider, on
  `LaptopScreen` and `PhoneScreen`. The phone screen's MeshCollider is one of the two named members.
  Restructuring that object during migration changes a gated number — re-run `tools/room_gate_check.py`.
- **The emission-keyword protection does not extend to TMP.** `Mat()` sets
  `RealtimeEmissive` specifically because URP's postprocessor recomputes `_EMISSION` from that field
  and silently stripped it once. TMP materials do not go through `Mat()`, so they inherit none of
  that guard. Do not assume they are covered.
- **Screens sit inside the unified grade**, so any TMP material is graded with the room and is not
  exempt — the same rule that governs the existing panels.
- **C3's one-token invariant and the HDR material path are TV-side concerns.** The room has no HDR
  text and no L4 occupant. Flagged only so the room is not scoped as though it does.

### 12.5 Estimate

Small — 1 × `UI.Text` certain, plus the phone's handful if it is ruled ours. The risk is not volume;
it is the GI sweep in 12.3 and the unowned surface in 12.2. Both are cheap to handle **before** the
phase and expensive to discover during it.
