# Room Visual Pass — Product and Visual Design Gate

**Status:** REVIEW REQUIRED — no implementation is authorized until Allen approves a direction  
**Scope:** Worktree 3 / room art only  
**Decision owner:** Allen  
**Product/design lead:** Staff TPM coordinator  
**Execution model after approval:** GPT-5.6 Terra medium worker; coordinator supervises and reviews  
**Current delivery branch:** `room-refinement` (requested integration branch name was `slice/room-art-pass`; resolve before implementation)

## 1. Decision requested

Approve one full-room visual direction before any production environment work:

1. **A — Blue-Hour Pressure** (recommended): stylized, believable, intimate, worn apartment.
2. **B — Tactile Pressure Box:** chunkier retro indie treatment with compressed, grimy, high-contrast forms.
3. **C — Pixel Night:** low-resolution 3D/pixel hybrid with deliberate texel density and dithered lighting.

Approval covers the room's visual target, the proposed source-of-truth architecture, and the static palette/clutter limits. It does **not** approve changes to layout, interactions, functional screen UI, or gameplay.

## 2. Product intent

Move the room from graybox to a recognizable game place: a financially pressured sports bettor's tiny apartment at night. The environment should tell that story before the player reads any screen, while remaining an intimate living space rather than a horror cell, casino, or luxury apartment.

The room is the frame around three diegetic gameplay surfaces:

- **TV:** primary spectacle and sweat surface.
- **Laptop:** primary decision/work surface.
- **Phone:** secondary pressure/notification surface.

The visual pass succeeds when a still image from the standing, couch, or desk view is identifiable as this game and the player can still use every existing interaction without relearning the room.

## 3. Existing source of truth

`Assets/SBR/Editor/GrayboxRoomBuilder.cs` currently:

- creates a fresh `Room.unity`;
- deletes the previous scene asset;
- rebuilds the room shell, furniture, screens, player, interactions, lighting, and materials;
- saves the rebuilt scene.

Therefore, manual art saved only into `Room.unity` is disposable and fails this PRD.

### Proposed persistent architecture

Use a hybrid builder + prefab model:

- Keep `GrayboxRoomBuilder` authoritative for all functional geometry, transforms, colliders, cameras, interactions, screen wiring, and scene creation.
- Add a persistent `Assets/SBR/Environment/Prefabs/RoomArtRoot.prefab`.
- Make the builder instantiate exactly one `RoomArtRoot` at world origin on every rebuild.
- Put nonfunctional visual dressing in that prefab: trim, bedding, cushions, blinds, cables, wall details, clutter, decal projectors, and noninteractive prop meshes.
- Store approved materials and textures under `Assets/SBR/Environment/**`; the builder loads those persistent assets instead of regenerating their authored values.
- New art should be collider-free unless a collider is explicitly required and validated. Existing functional collider volumes remain authoritative.

This gives artists a persistent authoring surface while preserving deterministic rebuilds and thin scene state.

## 4. Locked invariants

These do not change in the room art pass.

### Layout and geometry

- Interior footprint remains 2.6 m wide × 4.0 m long × 2.3 m high.
- Couch/bunk remains on the left long wall, facing the TV.
- TV remains on the right long wall opposite the couch.
- Desk, laptop, and phone remain at the far end of the right wall.
- Window remains on the far wall beside the TV zone.
- Mini fridge remains in the near/door-end left corner.
- Player spawn, walkable path, prop placement, and interaction ranges remain unchanged.

### Camera and interaction

- Standing camera remains the current player camera and FOV.
- Couch uses the existing seat anchor and 17° seated FOV.
- Laptop uses the existing focus anchor and 30° focus FOV.
- Sitting, standing, desk focus, focus exit, phone focus, screen clicks, and screen readability remain unchanged.
- No art element intercepts interaction raycasts or covers a functional screen.

### Ownership boundaries

Allowed:

- `Room.unity`
- `GrayboxRoomBuilder.cs`
- new `Assets/SBR/Environment/**`
- room materials, prefabs, textures, decals, and room-specific tests
- optional `RoomArtRoot` prefab/component

Forbidden:

- TV/theater scripts
- Laptop/SureThing scripts
- `RunDirector.cs`
- `TvLight.cs`
- `ProjectSettings/**`
- `engine/**`

### Presentation

- Functional UI remains code-native; generated imagery never replaces screen typography or buttons.
- No people, kitchen, casino interior, luxury finish, real sportsbook/game branding, watermarks, or readable AI-generated text.
- Green/red/gold remain reserved for monetary events. Static room art uses near-black blue, charcoal, faded neutral fabric, dim cyan/white, and restrained desaturated accents.

## 5. Visual pillars

### 5.1 Financial pressure through ordinary wear

The room looks used and cheap, not abandoned:

- repainted/scuffed wall patches;
- inexpensive laminate with worn edges;
- old fabric with compression, pilling, and one repaired area;
- fingerprints and dust around screens;
- restrained bills, receipts, cables, and takeout clutter;
- small signs of deferred maintenance.

Avoid gore, occult language, overt horror dressing, trash mountains, and caricatured poverty.

### 5.2 Screens establish the hierarchy

At night, screen light is the organizing principle:

1. TV is the brightest environmental anchor in the standing and seated compositions.
2. Laptop owns the desk composition without being visually contaminated by nearby clutter.
3. Phone reads as a small cyan/white secondary signal.
4. Window provides deep blue separation, not a competing focal point.

Static materials must not borrow the monetary green/red/gold language.

### 5.3 Compressed but navigable

The room should feel Tokyo-small through vertical storage, close furniture clearances, and layered silhouettes—not through narrowed paths. New dressing attaches to walls, existing furniture, or dead corners. The center aisle remains visibly and physically clear.

### 5.4 Stylized, feasible, repeatable

The target is an indie-production environment:

- simple meshes with strong silhouettes;
- authored material variation and decals doing most of the work;
- selective hero detail at the couch, TV wall, and desktop;
- limited unique textures and no dependency on a flattened full-room AI image.

## 6. Three camera compositions

Baseline screenshots will be stored under `artifacts/room-visual-pass/baseline/`.

| Gate view | Code-derived final camera pose | Vertical FOV |
|---|---|---:|
| Standing overview | world position `(0.30, 1.64, -1.40)`, facing room-forward `+Z` | 68° |
| Seated TV/couch | world position `(-0.95, 1.15, 0.30)`, aimed at TV center `(1.232, 1.10, 0.30)` | 17° |
| Focused laptop/desk | world position `(0.738982, 1.051217, 1.620000)`, forward `(0.939693, -0.342020, 0)`, aligned normal to the existing tilted laptop lid | 30° |

The captures, not hand-entered scene cameras, are the visual evidence. Final implementation comparisons must use the existing runtime anchors so later tuning to those anchors is inherited automatically.

### 6.1 Standing overview

**Current role:** first read of the entire room from the door-end spawn.

**Target read, in order:**

1. TV/window glow establishes nighttime and direction of travel.
2. Bunk-over-couch silhouette makes the room immediately recognizable.
3. Desk/laptop and phone complete the lived-in apartment read; the mini fridge remains correctly outside this camera frame at entry-left.
4. Clear central aisle communicates movement affordance.

**Art emphasis:** wall/floor material break, couch/bunk silhouette, window/blind silhouette, cable routing, and restrained depth layers.

**Do not:** add a foreground prop that narrows the aisle, create high-contrast wall art that outranks the TV, or hide the laptop.

### 6.2 Seated TV/couch

**Current role:** the signature sweat composition; the 17° FOV intentionally frames almost only the TV.

**Target read:**

- TV remains perfectly readable.
- The thin visible room edge sells a cheap wall, imperfect mount/cable run, and screen spill.
- Environmental reaction lighting supports TV events without static green/red/gold contamination.

**Art emphasis:** bezel/material quality, subtle wall wear around mount, cable raceway, dust/fingerprint response, restrained bloom target.

**Do not:** place posters, decals, lights, or reflective trim close enough to compete with the code-native TV surface.

### 6.3 Focused laptop/desk

**Current role:** primary betting/shop interaction; the laptop lid occupies most of the 30° focus view.

**Target read:**

- UI remains razor-readable and fully clickable.
- Peripheral desk material, laptop chassis, cable, phone, and one or two paper corners provide tactile context.
- Clutter frames the screen but never overlaps it or creates bright specular noise.

**Art emphasis:** worn laminate edge, keyboard/chassis breakup, cable routing, fingerprints, restrained bills/receipt shapes, local cyan rim.

**Do not:** pixelate or bake the UI into a texture, cover screen corners, move phone/laptop, or introduce interaction-blocking colliders.

## 7. Direction briefs

### A — Blue-Hour Pressure (recommended)

**Promise:** the most believable version of the established art direction.

- Stylized 3D/PBR-lite rendering with hand-authored wear.
- Deep blue-black room, dim cyan screen chrome, faded gray-brown fabric.
- Cool window fill plus controlled screen spill; intimate and legible.
- Cheap painted drywall, inexpensive laminate, powder-coated bunk frame, old woven couch.
- Detail density is selective and naturalistic.

**Why recommend it:** highest screen readability, closest match to the signed art-direction document, lowest implementation risk, and easiest base for later room-state variants.

**Primary risk:** can feel generic if silhouette, decals, and cable/repair storytelling are under-authored.

### B — Tactile Pressure Box

**Promise:** more immediate roguelite identity and visual pressure.

- Original chunky retro-indie forms; compressed proportions without moving the floor plan.
- Rough, tactile surfaces, thick bezels, exaggerated wear edges, sharper light falloff.
- Strong silhouette grouping and dense contact shadows.
- Draws from the requested CloverPit-like qualities—small first-person room, tactile grime, oppressive economy—without copying its assets, slot-machine imagery, branding, or exact palette.

**Primary risk:** can drift into horror, become derivative, or make the apartment feel like a cell. The “intimate, not horror” constraint needs active review.

### C — Pixel Night

**Promise:** the strongest authored stylization and potentially the clearest indie signature.

- Low-resolution 3D texture language with consistent texel density.
- Nearest-filtered material detail, chunky geometry, restrained dithering/posterized shadows.
- Deep navy/charcoal base with cyan screen separation.
- Environment is pixel-styled; TV/laptop/phone UI stays code-native and unpixelated.

**Primary risk:** pixel shimmer and dither can reduce screen readability, especially at 17°/30° FOV. It also introduces shader/material-system work that is less useful to a later naturalistic room-state expansion.

## 8. Surface and prop treatment

| Zone | Production target | Permitted story detail | Readability guard |
|---|---|---|---|
| Walls | worn painted finish, repaired patches, low-frequency grime | one taped repair, faint furniture rubs | no bright/high-frequency pattern behind screens |
| Floor | cheap dark laminate or vinyl, subtle scuff path | furniture pressure marks | central aisle remains clean and low contrast |
| Couch | compressed cushions, old woven fabric, one repaired seam | rumpled throw/pillow if silhouette permits | sit hover volume and seat read remain obvious |
| Bunk | powder-coated metal/cheap frame, thin bedding | one hanging garment or strap only if nonblocking | no geometry intrudes into standing camera |
| TV wall | bezel, mount shadow, cable/raceway, wall discoloration | dust/fingerprints | no poster or decal competing with TV |
| Window | dirty glass/blinds, blue-black exterior, condensation hints | distant abstract city light only | no readable signage or bright green/red/gold |
| Desk | worn laminate, softened front edge, cable management | bills/receipts, mug or takeout remnant, max 2–3 small clusters | laptop and phone fully visible/clickable |
| Mini fridge | dented painted metal, rubber seam, handle | magnet/tape shapes without text | silhouette remains readable from spawn |

## 9. Lighting specification

- Base ambient: deep neutral/navy, dark enough for screen dominance but not crushed.
- Window: cool blue separation and a soft far-end gradient.
- TV: existing functional event lighting remains untouched; art materials must respond coherently.
- Laptop/phone: emission supports local form only, with no colored pool that competes with UI.
- Static green/red/gold lights are prohibited.
- Avoid glossy room-scale reflections, strong bloom halos over typography, and pitch-black collision edges.
- Any new light should be non-shadowing unless it replaces an existing visual role and passes a performance/readability review.

Approval is based on relationships, not final numeric Unity light values. Values are tuned after direction selection against the three exact camera captures.

## 10. Asset policy

### Full-room generation

Generated full-room images are concept references only. They are never used as a skybox, room projection, camera overlay, or flattened environment.

### Production image generation after approval

Permitted candidate generation:

- original poster/advertisement concepts without brands or text;
- bills/receipt/stain/wall-decal shapes;
- worn wall, laminate, metal, and fabric references;
- screen-adjacent nonfunctional graphics.

Every generated production candidate must be:

1. inspected for unwanted text/branding/artifacts;
2. cleaned and cropped;
3. made tileable or decal-ready as required;
4. color-corrected to the palette law;
5. validated in all three views;
6. stored under `Assets/SBR/Environment/**`.

## 11. Post-approval implementation sequence

1. Freeze the chosen concept, palette, clutter level, and architecture.
2. Add persistent `RoomArtRoot` prefab and deterministic builder instantiation.
3. Move room material ownership into persistent environment assets.
4. Translate major surfaces and lighting first; rebuild and compare three views.
5. Add silhouette props/trim and collision-free dressing.
6. Generate only the approved individual production candidates; clean and validate them.
7. Tune TV/couch and desk micro-compositions.
8. Run rebuild-idempotence, interaction, collision, readability, and test gates.
9. Save `Room.unity` only through the builder and provide final three-view evidence.

## 12. Acceptance gate

### Visual

- Standing view reads as a compact bettor's apartment at night, not a graybox.
- Bunk-over-couch, TV, desk/laptop, window, phone, and mini fridge are recognizable.
- Couch view supports the TV as the hero without competing décor.
- Desk view feels tactile while the laptop UI remains sharp.
- Static palette obeys the green/red/gold money-only rule.
- Room feels slightly dingy and financially pressured, intimate rather than horror.

### Functional

- Movement path and collisions are unchanged.
- Sit/stand and seated camera framing are unchanged.
- Laptop focus/exit, pointer behavior, and clicks are unchanged.
- Phone focus remains unchanged.
- TV and laptop screen readability do not regress.

### Source of truth

- Running the builder twice produces exactly one complete art root and retains the approved look.
- No important art exists only as a manual `Room.unity` edit.
- No forbidden file is modified.

### Evidence required

- Before/after captures from the same standing, seated, and desk camera poses.
- Rebuild-twice hierarchy check.
- Git diff ownership audit.
- Unity room-specific tests plus existing relevant test suite.
- Manual smoke checklist for movement, couch, laptop, phone, and screen readability.

## 13. Explicit non-goals

- Changing room dimensions or prop placement.
- Reworking TV/theater or SureThing/laptop behavior.
- Adding full heater/sweating/buried room variants.
- Adding a kitchen, player body, new gameplay interaction, casino dressing, or branded sports content.
- Replacing code-native functional UI with generated raster art.
- Shipping AI-generated full-room imagery as production environment art.

## 14. Sign-off checklist

Allen to confirm:

- [ ] Direction A, B, or C.
- [ ] Proposed hybrid builder + persistent `RoomArtRoot` architecture.
- [ ] Default clutter level: restrained (recommended).
- [ ] Mood: intimate pressure, not horror (recommended).
- [ ] Static monetary-color prohibition.
- [ ] Implementation may begin only after this gate is explicitly approved.
