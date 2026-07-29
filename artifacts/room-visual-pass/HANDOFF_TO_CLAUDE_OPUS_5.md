# Room Visual Pass — Full Ownership Handoff to Claude Opus 5

**Handoff date:** 2026-07-24  
**Incoming owner:** Claude Opus 5, room-art lead and staff technical product manager  
**Worktree:** `C:\Users\Allen\orca\workspaces\sports-betting-roguelite\room-refinement`  
**Current branch:** `room-refinement`  
**Requested delivery branch:** `slice/room-art-pass`  
**Current HEAD:** `d66543898f2841e1b8e0f33c7c33a49ed9d1594b`  
**Unity:** `6000.5.3f1`  
**Gate status:** design review required; implementation is not authorized yet  

## 1. Your role and authority

You now own this workstream. Act as the staff technical product manager and technical/art lead:

- maintain the product and visual-design gate;
- obtain Allen's explicit approval before implementation;
- supervise scope, architecture, visual fidelity, and acceptance criteria;
- review all implementation and validation evidence;
- delegate heavy implementation, test, capture, and validation work to a **GPT-5.6 Terra medium** worker;
- do not personally absorb heavy grunt work that should be delegated;
- keep Allen informed and stop for material product decisions.

This is a full ownership transfer. The previous Codex lead is no longer supervising or monitoring.

## 2. Product objective

Move the compact first-person room from graybox to an intentional, recognizable apartment used by a financially pressured sports bettor. Preserve the working floor plan, movement, collisions, sitting, desk focus, screen interaction, and screen readability.

The room should read correctly from:

1. standing overview;
2. seated TV/couch composition;
3. focused laptop/desk composition.

The mood is intimate pressure at night, slightly dingy but believable—not horror, a casino, a prison cell, or a luxury apartment.

## 3. Required approval gate

Allen has **not yet explicitly approved** a visual direction or implementation. Do not begin heavy Unity implementation, production-image generation, testing, or save `Room.unity` until he approves:

- Direction A, B, or C;
- hybrid builder + persistent `RoomArtRoot` architecture;
- restrained clutter;
- intimate pressure, not horror;
- green/red/gold reserved for monetary events.

Recommended response shorthand:

`Direction A/B/C + hybrid RoomArtRoot + restrained clutter + intimate pressure + money-only green/red/gold`

## 4. Design package — read all of these first

All paths are relative to the worktree root:

- `artifacts/room-visual-pass/ROOM_VISUAL_SIGNOFF.md` — primary decision board.
- `artifacts/room-visual-pass/ROOM_VISUAL_PASS_PRD.md` — complete product/visual specification.
- `artifacts/room-visual-pass/room-treatment-map.svg` — layout-authoritative 2D treatment map.
- `artifacts/room-visual-pass/room-treatment-map.png` — raster review copy.
- `artifacts/room-visual-pass/CONCEPT_DIRECTION_PROMPTS.md` — complete image-generation prompts and constraints.
- `artifacts/room-visual-pass/baseline/room-recon.md` — exact runtime capture method, camera poses, hashes, builder findings, and observed constraints.

Exact Unity baselines:

- `artifacts/room-visual-pass/baseline/standing-overview.png`
- `artifacts/room-visual-pass/baseline/seated-tv-couch.png`
- `artifacts/room-visual-pass/baseline/focused-laptop-desk.png`

Concept references:

- `artifacts/room-visual-pass/concepts/concept-a-blue-hour-pressure.png`
- `artifacts/room-visual-pass/concepts/concept-b-tactile-pressure-box.png`
- `artifacts/room-visual-pass/concepts/concept-c-pixel-night.png`

These files are intentionally under the git-ignored `artifacts/` directory. Remain in this worktree until their disposition is decided; a separate clean worktree would not automatically contain them.

## 5. Concept decision

### A — Blue-Hour Pressure (recommended)

Believable PBR-lite treatment, worn inexpensive materials, cool window separation, controlled cyan/white screen spill, and selective lived-in detail. This has the best screen-readability confidence, closest fit to current art direction, lowest implementation risk, and strongest room-state flexibility.

The first generated A image incorrectly moved the mini fridge beside the desk. That result was rejected; the retained A image was surgically corrected. The fridge remains at entry-left, outside the standing frame.

### B — Tactile Pressure Box

Original chunky retro-indie forms, rougher surfaces, compressed light falloff, heavier silhouettes, cables, contact shadows, and stronger material breakup. Use only broad tactile/pressure traits from the requested CloverPit-style reference. Do not copy assets, branding, slot imagery, palette, or exact room design. Guard aggressively against horror/cell drift.

### C — Pixel Night

PSX-era-inspired environment materials, consistent texel density, chunky geometry, restrained dithering, and posterized shadows. Functional TV, laptop, and phone UI must remain smooth and code-native. No full-screen pixel filter or shimmer contaminating the 17° TV and 30° laptop views.

## 6. Exact runtime view evidence

The three 2560×1440 images were rendered from the existing `PlayerCamera` in real Play Mode after eight player-loop frames so the TV and SureThing UI initialized:

- standing: `(0.300, 1.640, -1.400)`, forward `+Z`, FOV `68°`;
- couch: `(-0.950, 1.150, 0.300)`, aimed at TV center `(1.232, 1.100, 0.300)`, FOV `17°`;
- laptop: `(0.738982, 1.051217, 1.620000)`, forward `(0.939693, -0.342020, 0)`, FOV `30°`.

The disposable Editor capture harness and its `.meta` were removed. Capture-only `EditorBuildSettings.asset` drift was restored. The tracked worktree was clean after capture. `Room.unity` remained identical to `HEAD`, SHA-1:

`dec08cc864ef0f859b91896b57788132f3ce9004`

## 7. Critical source-of-truth architecture

`unity/SBR/Assets/SBR/Editor/GrayboxRoomBuilder.cs` creates a fresh scene, deletes/recreates `Room.unity`, rebuilds functional content, and saves it. Important art left only as manual scene edits will be erased.

Recommended architecture after approval:

1. Keep `GrayboxRoomBuilder` authoritative for functional transforms, colliders, cameras, screens, interactions, lighting wiring, and scene creation.
2. Add persistent `Assets/SBR/Environment/Prefabs/RoomArtRoot.prefab`.
3. Make the builder instantiate exactly one `RoomArtRoot` at world origin on every rebuild.
4. Put only nonfunctional visual dressing there: trim, bedding, cushions, blinds, cables, decals, clutter, and noninteractive silhouettes.
5. Keep new dressing collider-free by default.
6. Store persistent authored materials/textures under `Assets/SBR/Environment/**`; the builder loads them rather than overwriting authored values.
7. Validate builder idempotence by rebuilding twice and confirming exactly one complete art root.

No flattened AI-generated full-room image may enter the game. Functional UI remains code-native.

## 8. Ownership boundaries

Owned:

- `Room.unity`
- `GrayboxRoomBuilder.cs`
- new `Assets/SBR/Environment/**`
- room materials, prefabs, textures, decals, and room-specific tests
- optional `RoomArtRoot` prefab/component

Must not touch:

- TV/theater scripts
- Laptop/SureThing scripts
- `RunDirector.cs`
- `TvLight.cs`
- `ProjectSettings/**`
- `engine/**`

This is the only room-art worktree allowed to open and save `Room.unity`. The principal owns `RunDirector`, canonical docs, project settings, and final Unity scene validation.

## 9. Image-generation policy

The completed full-room generations are concept references only. After Allen approves a direction:

1. translate the direction into Unity surfaces, materials, lighting, and props first;
2. stabilize that translation against all three runtime views;
3. only then generate individual production candidates such as original poster concepts, bills, stains, wall decals, fabric/material references, or screen-adjacent nonfunctional graphics;
4. inspect, clean, crop, color-correct, make tileable/decal-ready, and validate each candidate before use.

No readable AI text, copied branding, watermarks, casino imagery, or generated functional UI.

## 10. Post-approval execution sequence

Delegate the heavy execution to GPT-5.6 Terra medium and supervise:

1. resolve the branch-name mismatch with Allen before implementation;
2. freeze chosen direction, palette, clutter, and architecture;
3. implement persistent `RoomArtRoot` and deterministic builder instantiation;
4. translate major surfaces and lighting;
5. capture the same three runtime views and review;
6. add collider-free silhouette dressing, props, and trim;
7. generate/clean only approved individual production candidates;
8. tune couch/TV and desk micro-compositions;
9. run rebuild-twice, collision, interaction, readability, ownership, and test gates;
10. provide matched before/after evidence and code review.

Implementation integration order remains:

1. TV sweat refinement;
2. SureThing revamp;
3. room art pass last.

## 11. Acceptance gate

Visual:

- standing view unmistakably reads as the game's compact bettor apartment at night;
- couch view preserves the TV as hero and perfectly readable;
- desk view feels tactile while laptop UI remains sharp and clickable;
- static room palette obeys monetary-color restrictions;
- pressure is intimate, not horror.

Functional:

- movement path and collisions unchanged;
- sit/stand and seated framing unchanged;
- laptop focus/exit, pointer behavior, and clicks unchanged;
- phone focus unchanged;
- TV/laptop readability does not regress.

Source of truth:

- builder rebuild twice retains the approved look and produces exactly one art root;
- no important art exists only in manual `Room.unity` edits;
- no forbidden files modified.

## 12. Immediate next action

Read the complete design package, inspect all six baseline/concept images directly, then introduce yourself to Allen as the new owner. Summarize the pending decision and ask for explicit A/B/C plus architecture/mood/clutter/color approval. Do not begin implementation merely because Allen said the package looked nice.
