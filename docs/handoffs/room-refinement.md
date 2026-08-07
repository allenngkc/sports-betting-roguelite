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

## 11b. STATE AS OF 2026-08-02 — read this first

**Branch `room-refinement`, 11+ commits ahead of `main`. Everything below is built, committed, and
now compile-verified, built, baked and captured. The register reads `main`, so items may appear open
there that are done here.**

| item | state |
|---|---|
| T48 grade — neutral black point | built `ebdd0ed`, **verified: it worked** |
| R19a body materials · R19c drab green · R16 colliders 29 | built `35cbab6` |
| T57 true-size quads | built `a1fd6fb` |
| R20 chipped paint + battered desk | built `c79e466` — **not** outstanding |
| T54 gates state what they cannot see · Gate 4 owner-keyed | built `336a1a4` |
| R23/R26 conformance instrument | built, editor-free via `--conformance` |
| **R22 walkthrough** | **OPEN — Allen's, gates 6–8, nothing else clears it** |

### T48 verified — the grade no longer tints the room

Screens dark, graded, chroma per surface: ceiling **7.64 → 0.33**, bunk 1 **6.65 → 0.33**, right wall
**6.92 → 0.97**. The grade previously multiplied chroma 4–14×; it now sits *below* the ungraded
values. Mattress 44.64, inside 43.9 ±1. All structural gates PASS; collider inventory 29.

### R23 still FAILs — but the character changed, and this is now a design question

Two surfaces remain COOL: **far wall (3.56)** and **floor aisle (1.66)**. Both are cool *ungraded
too* (5.49, 2.97), so this is **the window's own light, not the grade**.

Design doc §1.2 sanctions exactly that: a cool window with **short reach that pools locally and does
not tint the room**. Those two region boxes sit inside that sanctioned pool. So the remaining failure
may be the instrument sampling the design working, not the room failing. **Either the regions move
off the window pool, or law 1.1's test explicitly excludes it — a DD call, not a build fix.**

### C22.1 — steel and conduit re-measured — ⚠️ **SUPERSEDED 2026-08-03, the numbers were wrong**

> The table that stood here reported the display housing at **C 2.07 h 108.3° WARM** and the conduit
> drop at **C 3.97 h 105.2° WARM**, and concluded that cool metal was unreachable in render. It also
> carried its own caveat — *"first-pass, not surface-pure harness regions; add proper ones before
> treating the numbers as ratified."* **The caveat paid. Both boxes were bleeding the warm plaster
> wall behind the metal, and reported the WALL's hue as the metal's.** 108° is plaster: the pipe's
> own neighbouring wall strips measure 101–112°.

Surface-pure boxes, now permanent as `R19_REGIONS` in `tools/room_gate_check.py`, purity sd/mean in
brackets:

| region | graded | ungraded |
|---|---|---|
| housing face (steel) `[0.020]` | C 0.52 h 257.6° **neutral** | C 0.74 h 260.0° neutral |
| conduit drop, body `[0.053]` | C 2.02 h 269.2° **COOL** | C 2.65 h 269.5° COOL |
| conduit drop, full width | C 1.35 h 258.1° neutral | C 1.45 h 253.8° neutral |
| conduit ceiling run `[0.046]` | C 8.29 h 99.4° **WARM** | C 11.80 h 98.8° WARM |

**The steel reads neutral — chroma 0.52 is below the instrument's own 1.5 floor, so no hue verdict is
supportable in either direction. The conduit reads COOL, graded and ungraded.**

**Same albedo on both conduit runs, opposite verdicts.** The ceiling run is raked by the warm tube
and reads WARM; the wall drop sits away from it in ambient/window fill and reads COOL. So rendered
hue tracks **which light reaches the surface**, not albedo alone — Law 1.7's mechanism (lighting
gates the read) applied to colour instead of relief. Do not answer any of this by lightening albedo.

Scope, per C25: the conduit body strip samples **one face of a cylinder** (the shaded one), because
a pure strip on a cylinder inevitably picks a face. Its full-width twin is carried beside it, and
that figure includes edge pixels bleeding the wall — which is precisely the failure the first-pass
boxes made at larger scale.

### Batch 8 (2026-08-03) — three of the four open questions are answered

**R19(b)-am — "colder" is STRUCK.** The ≥2 channels are now **value and finish**, and hue
temperature is no longer one of them. The reasoning is Law 1.1's own mechanism: under one warm key
on warm plaster, requiring the metal to read colder than the room requires the room to break its top
law in one region. The first-pass boxes affect *how much*, not *whether*, so this ruled without
them. **No lighting instrument** — refused explicitly; R12 grazing reveals relief, not colour
temperature, and a metal-tinting lamp is T48's rejected Option D in new clothes.

The albedo hold and the unprompted first-pass caveat were **endorsed as the standard**. Keep doing
both.

Consequence, and it was a live over-claim of mine for part of a session: `GrayboxRoomBuilder`'s
R19(a) comment justified the split on **three** channels including "warm where the steel is cool".
That is albedo arithmetic that does not survive to frame. Corrected to value + finish, with the hue
deltas recorded but explicitly not counted. Constitution draft §2.5: *measure the rendered thing,
not the source.*

#### ⚠️ Routed to the DD: R19(b)-am's conclusion stands, its stated premise does not

**This needs a ruling and I have not acted on it.** R19(b)-am reasoned that "under one warm key
every albedo in this room renders warm", so cool metal is unreachable, and ruled explicitly *before*
surface-pure numbers on the grounds that the first-pass boxes "affect *how much*, not *whether*."

The surface-pure boxes (table above) say they affected **whether**:

- the **conduit reads COOL** (269.2°) on every pure strip, graded *and* ungraded;
- the **steel reads neutral**, not warm — C 0.52, below the instrument's own chroma floor;
- only the ceiling conduit run reads warm, and that is the run the tube rakes directly.

So the metal is *already* colder than the room, in the build, with the spec albedos installed. And
§1.1 is not breached by it: the law names a blue-tinted **room**, and these are small dark fixtures
at chroma ~2.

**The channel choice may well still be right** — value and finish are more robust carriers than hue,
and they survive any relighting. That is a design call and it is the DD's. What I am flagging is only
that the *reason given* is falsified by measurement, so the next lead does not inherit "cool metal is
physically unreachable" as settled physics. It is not.

**Nothing is blocked.** R19(a) proceeds on value + finish either way, and both are satisfied.

**R28 — the room owns the phone OBJECT; the content is nobody's, and stays dark.** R19(a) already
puts it in his material register, not the institution's. A dark phone also cannot become a C13
instance.

> **Do not execute the "screen stays dark" clause without re-asking.** R28 was ruled from principle
> with the question text absent and invites the narrow re-ask. Its premise does not match this build:
> a phone surface **already exists and is functional** — `PhoneScreen.cs` renders `BookieFeed`, a
> real engine model, and carries live coverage in `Tests/PlayMode/PhoneTests.cs`,
> `Tests/EditMode/BookieFeedModelTests.cs` and `RoomSmokeTests.cs`. Blanking it deletes a working,
> tested feature on a ruling made without sight of it. Ask the narrow form first.

**Separate live finding on the same file, now unambiguously room-owned by R28:** `PhoneScreen.cs:36`
declares `chromeCyan` `(0.62, 0.86, 0.96)` and prints it at `:188` on the `BOOKIE` label, and the
file cites `design/08`'s palette law as its authority at `:9` and `:32`. `design/08` is **T3, a
deprecated anti-reference**; `chromeCyan` is **T9, a retired hue**. This is a room-side instance of
the T9/T15/T30/T34 class that the retired-hue scan did not reach. Not fixed here — the replacement
is a design call and the surface's content authority is exactly what is unresolved.

**R29 — Gate 2 active state: RULED and BUILT.** A gate certifies the configuration it ran against.
Gate 2 now reads `m_IsActive`, names any disabled same-named object, and where a `PrefabInstance`
carries no override it resolves the flag from the **source prefab asset** rather than assuming.

That immediately caught something: **`RoomArtRoot` had no `m_IsActive` override**, so the old
gate's PASS covered three of four singletons and was silent about the room's own art root — the one
object the whole slice hangs on. Resolved through the prefab (`038e2203…`, root active), so Gate 2
re-runs against the active state and PASSes 4/4 rather than recording uncovered. Its blind spot now
states the two things it still cannot do: it reads the object's own flag, so an object whose
**ancestor** is disabled reads active here, and where the flag comes from the prefab the verdict
covers the asset's default, not anything the scene states.

### The evidence gap — the harness's output was never kept

Every number this harness has produced reached the register by being **hand-copied out of a
terminal**. No gate log, no JSON, nothing in `artifacts/`. The claim was the artifact and no run was
reproducible. Fixed: `--report PATH` tees the full run to a file. First one committed at
`artifacts/room-visual-pass/gate-runs/2026-08-03-R29-gate2-active-state.txt`. **Pass it on every
run** — C11 wants the evidence, C17 wants it retained, C25 wants its scope attached, and the file
carries all three.

Related and still true: **all of R19 is committed, not frame-verified.** No instrument region samples
the laptop, TV or phone body, so R19(a)'s separation has only ever been albedo arithmetic. Under
R19(b)-am the carrying channel is **value**, which is measurable in a frame — so regions for those
bodies would convert R19(a) from asserted to measured.

### Batch 12 (2026-08-05) — the wear fork closes, and the glow gets a rule

**R8 wear: RULED option 3 — wear lives in the standing shot.** Both technique escapes refused
(scale/contrast *and* the URP Decal Renderer). **The re-placement precondition closes as
tested-and-null** — it was tried, measured, and changed nothing. Nothing further is owed here.

**`R7-F` is now INFORMATIONAL**, ruled the sixth vacuous green. The finding is that it was never a
gate: it can prove a piece is *in frame*; it cannot prove it is *visible*, and this lane showed the
gap twice in one session — `TrafficPath` "failed" on an origin-point test with a fifth of it in shot,
and `ConduitDrip` "improved" 30% → 67% coverage while going 34% → 64% hidden, changing the frame by
nothing. **A check that can go green on invisible wear is not a gate on whether wear reads.**
`CaptureWearAB` is the instrument that answers that, and option 3 was ruled on its numbers.

**R35/R37 — the glow rule, built:**

| rule | built |
|---|---|
| warm near-neutral, **R ≥ G > B** | `0.038 ≥ 0.032 > 0.024`, preserved under ×3 |
| attention differs by **amplitude only** | attention **is** idle × 3, exactly |
| **~3× maximum** | **3.00×** by construction (previous build was 4.07×) |
| `idleEmission` carries the same defect | both ends are now one colour |
| **NO PULSE** (R37) | `emission = (wantsYou && !engaged) ? attention : idle` |

Writing attention as `idle × 3` rather than a second hand-picked triple is deliberate: *amplitude
only* then holds **by construction** instead of by my matching two chromaticities and asserting they
agree. Any future edit to idle carries attention with it, so the two cannot drift apart the way idle
(cool) and attention (violet) had.

`attentionBreathHz` is **removed**, not left at zero — a dead serialized dial invites someone to
reinstate the breathing it used to drive. There is also no easing on the step: a lerp with a duration
would be R37's finding wearing a shorter clock.

**Owed:** one capture to rule the exact values on. Editor is after SureThing.

### R8 / weathering — measured, and the parking diagnosis does not survive it

**With the entire wear inventory disabled, two of the three review poses are bit-identical.**

| pose | changed | >JND |
|---|---|---|
| standing | 1.49% | **0.90%** |
| seated | 0.00% | **identical** |
| laptop | 0.00% | **identical** |

Run it with `SBR.RoomViewCapture.CaptureWearAB -outDir <dir>` then
`python tools/wear_ab_diff.py <dir>`. **This is the only instrument that answers "does the wear
read".** Box statistics on a still frame do not: a decal is small against busy geometry, so a box
around it measures the pipe fitting beside it — that method scored two of four sites as reading
where brightened crops show nothing at all.

**R7 was parked on "placement versus camera, not technique". That is falsified as the whole story.**
`ConduitDrip` was re-placed to raise its seated frustum coverage 30% → 67%; the rendered frame
changed by *exactly nothing*, because the TV housing stands in front of that wall and the move took
the quad from 34% to 64% occluded. Reverted. **Frustum coverage is not visibility.**

**Two instrument traps, both mine, both now closed:**

- **First-render artefact.** The first A/B reported 91.76% changed and its determinism control
  *passed twice* — the same fault repeats every run. The frames showed a magenta TV panel: the first
  Edit Mode render lands before the pipeline settles, and the method had captured render #1 and #2
  as the two halves. `WarmRender` discards three frames first. **Only the picture caught it.**
- **Occlusion.** R7-F now reports occluded fraction beside frustum coverage, against a table of the
  solid boxes that sit in front of wear surfaces.

**Open for the DD, three-way and all of it ruled territory:** scale/contrast on the existing decals,
the URP Decal Renderer (whose "not yet — re-place against the frusta first" precondition is now met
*and answered*), or accepting that wear lives only in the standing shot.

### ⚠️ `attentionEmission` — a saturated violet the room throws, and it is ours

Raised by SureThing's colour audit; **their read that this is room lighting is correct.**

`LaptopScreen.cs:29` — `attentionEmission = (0.28, 0.10, 0.55)`, B > R > G, **10× `idleEmission` on
blue**. `Glow()` drives the **lid renderer**, and the room builds that object and attaches the
component (`GrayboxRoomBuilder` 810, 836). A lid emitting into the room is light, not app content —
R28's split exactly, one surface over.

> **⚠️ CORRECTION (2026-08-05).** I reported the laptop panel measuring **hue 303.6° / chroma 9.17**
> as rendered evidence of `attentionEmission`. **That attribution was wrong.** The panel region is
> the SureThing canvas drawn *over* the lid, and what it is showing is the **superseded violet
> laptop package** — purple tabs, magenta team names, violet DETAILS buttons, a magenta LOCK IT IN.
> That is **C13**, already ruled an *integration item, not a room defect*. I measured their stale
> content through our camera and called it our light.
>
> Proof: the strike landed in the build and the panel still reads **308.7°** in `batch11`. If the
> violet had been ours, that number would have moved.
>
> **The finding against `attentionEmission` still stands** — on the source value (`0.28, 0.10, 0.55`,
> hue 312°, chroma 64.1), on ownership (lid renderer, room-built object), and on the direction. The
> DD struck it *without needing a frame*, which was the right call and is now visibly why.
>
> **Standing consequence: room captures near the laptop are contaminated.** Any colour claim in that
> region is reading C13's stale package until the surfaces' content is re-integrated.

It fires when `wantsYou && !engaged` — Betting/Shop/RunWon/RunLost with the player **away from the
desk**, i.e. precisely while seated at the TV, and it *breathes*.

Against: purple is retired project-wide; §2's palette carries no violet (its only cool is the window
`#5679C2`); §1.2 requires screens "quiet, with faint spill"; §1.1's named failure mode is a
saturated cool cast.

**Not fixed — the replacement colour is a design call, not a lead's.** Constraint space if it helps:
the laptop is *his* machine (§6), so its register is personal, and SureThing's own wax amber
`#D9A441` would be coherent with the surface it sits on. Whatever replaces it must satisfy §1.2's
"quiet".

### Batch 10 (2026-08-05) — the slice closes: 10 PASS, 0 FAIL, no VOIDs

**Gates 6–8 re-certified on a FRESH walk.** Allen walked the post-retirement build and passed. The
record says *fresh walkthrough*, not standing verdict — `--certify-basis` exists precisely so a
replayed verdict can never be mistaken for a walk of the build in front of you.

**C29 retrofitted — `tools/check_test_results.py`.** Run it on every NUnit result before believing a
green suite. It enforces three things, each of which has actually gone wrong here:

1. **the results file exists** — `-runTests` given `-quit` exits 0 having written nothing;
2. **`testcasecount > 0`** — C29's own case, a filter matching zero tests reported as a pass;
3. **`failed == 0` plus a declared per-suite floor** — 20 cases is not zero, but it is not 39 either.

```
python tools/check_test_results.py <results.xml> --min-cases "editmode=70,playmode=18"
```

**NEVER pass `-quit` with `-runTests`.** The runner closes the editor itself and `-quit` races it.
The same shape is retrofitted inside the harness: R19/R20 FAIL rather than print an empty line if
they measure zero regions.

**C30 got its instrument — R33 palette conformance.** A palette names materials, not perceived hues,
so conformance is checked on the **scene**: 14 ruled placements, each asserted to wear its ruled
material, no frame and no lighting argument. **This gate would have answered R33 in one run** — "drab
green absent from the room" cost three escalation rounds, a false finding of mine and a lighting
debate, and the question was only ever *is the material applied*.

**`Bunk2Pillow` is a RULED NAMED EXCEPTION (Allen, 2026-08-05) — and it is enforced, not annotated.**
The pillow stays pale (`ArtGrime`, not `#3A4230`) because the occupied-read outranks rule purity;
the green would darken §1.4's one deliberate pale shape by 37.5%. It sits in `PALETTE_PLACEMENTS`
like any other placement, so **greening it FAILS the gate**. An exception living only in prose is one
refactor from being tidied into conformance by someone who never saw the ruling.

**The swatch did not become a value change:**

| | value | delta |
|---|---|---|
| ratified, pre-swatch | 43.90 | — |
| old box (where 43.9 was defined), post-swatch | 44.44 | **+0.54**, inside ±1.0 |
| re-baselined pure box, post-swatch | 38.29 | −0.01, inside ±1.0 |

The old box is the honest comparand because 43.9 was ratified on it; comparing across instruments
would be the error this whole sequence was about. **Limit, on the record:** every capture in the repo
is post-swatch, so this compares a post-swatch measurement to a pre-swatch ratified figure. No
before/after pair exists and none can be made without reverting the swatch.

### Batch 9 (2026-08-04) — R25 GRANTED, and two of the queue's premises are falsified

**R25 painterly read: Design-verified.** The fragility flag was recorded with it — the read is
lighting-assisted (4.68× rendered vs 2.17× albedo-only), so **any future shot that relights the desk
re-opens R19**. That line is now the reason, and it is worth defending.

**The mattress 37.36-vs-44.44 discrepancy — RESOLVED, and neither box is mis-framed.**

| capture | mattress mean |
|---|---|
| `standing-overview.png` (screens **lit**) | 44.44 / 44.49 |
| `conformance-room-screens-dark.png` (screens **dark**) | **38.41** |
| conformance, ungraded dark | 25.47 |

Same camera pose (both are eye `(0.300, 1.640, −1.400)`, +Z, 68°), same box, same surface. The gap is
**screens-lit vs screens-dark** — the mattress catches laptop/phone/TV light and the conformance set
silences all three by construction, so it *must* read ~6 lower. R32 supposed one box was not framing
its surface; neither is.

**The real defect was C25's:** a ratified number quoted without the capture it is defined on, so two
runs saying "the mattress" meant different quantities. R9-A now names its capture and lighting in its
own line and reports the screens-dark value beside it, labelled *not this test*.

**R33 is already done — the drab green is applied, at spec.** R33 says the swatch is absent and "all
four bunk/mattress materials remain warm neutral greys." Not so:

| material | linear | sRGB | |
|---|---|---|---|
| `BunkFrameGreen.mat` | (0.0423, 0.0545, 0.0296) | **#3A4230** | G>R, green |
| `ArtBunk2Shadow.mat` (bedding) | (0.0423, 0.0545, 0.0296) | **#3A4230** | G>R, green |
| `CouchGray.mat` | (0.172, 0.158, 0.132) | #736F66 | warm — correctly excluded |

`BunkFrame` is bound to six objects (both slabs, four posts). **Applying it again is a no-op.**

**R32's placement amendment has no region to land in.** It rules the fabric "reads its drab green
outside the pool's reach." Swept nine surface-pure patches across both bunks — slab ends, mids, posts,
lower rail, bedding — and **0 of 9 read green.** Every one is COOL (h 249–271°) or below the chroma
floor. The whole bunk assembly flanks the window and sits in its pool; there is no outside-the-pool
bunk surface to carry the swatch. **Routed back — not actionable as placement.**

**R31 recorded:** finish leads, value stays required. Reasoning is in `BuildMaterials`.

**Phone joins C13 coverage.** R28-am keeps the live `BookieFeed`, which removes the phone's structural
immunity to shipping superseded content inside a room capture. Room frames now carry **three** live
surfaces. Nothing may be authored onto that screen — live engine data only.

### Round 4 (2026-08-04) — three items, all answered by measurement, none by moving a colour

**1. Steel/conduit VALUE, re-measured against T48's neutral black point** — the re-measure C22.1
deferred. Every surface, screens-dark graded, sorted by L\*:

| L\* | surface |
|---|---|
| 9.56 | conduit drop — *metal* |
| 10.17 | ceiling plaster |
| 10.78 | wall (right) |
| **11.30** | **housing face — *metal*** |
| 11.59 | conduit (full width) — *metal* |
| 12.33 | floor (aisle) |
| 13.08 | wall (far) |
| 13.30 | bunk 1 |
| 18.83 | conduit ceiling run — *metal*, brighter than every room surface but the mattress |

Metal mean **10.43** vs room mean **11.93** — darker by **1.50 L\***, and the distributions
**overlap**: the housing sits above both the ceiling and the right wall. R19(b)-am made VALUE one of
the two channels carrying the institutional read after striking "colder". On this evidence **value
is carrying very little and FINISH is doing the work.** Routed to the DD, not fixed — darkening the
albedo to manufacture separation is exactly what R19(b)'s guard forbids.

**2. R19(c) drab green — placed correctly, does not read as green.** The couch is clean (chroma 0.34
and 0.06, far below the 1.5 floor), so nothing green landed where the ruling excluded it. But both
bunk frames read **COOL** — post h 266.0 / C 1.62, slab h 267.8 / C 2.71, cool ungraded too. They sit
in the window's pool and `#3A4230`'s chroma is too low to survive it. Mattress **44.44**, inside
43.9 ±1, so R19(c)'s hold survives. Boxes eye-confirmed on frame members.

**3. R20 chipped paint + battered desk — both read, provably.** New INFO block measures p95−p5
luminance spread against a benchmark that is the **ceiling stain**, the surface §1.7 names as the one
that demonstrably reads at review distance:

| spread | surface |
|---|---|
| 4.86 | ceiling (benchmark) |
| 2.00 | housing paint, flat — below benchmark |
| **8.49** | housing paint, most varied — **READS** |
| **10.72** | desk, mid — **READS** |
| **7.72** | desk, far/dark end — **READS** |

The housing's split is the design working: chips are 8–14% coverage by construction, so most patches
are intact paint and flat, while a patch containing a chip beats the benchmark by 1.7×. The desk
reads even away from the lamp pool — sampled twice for that reason, since a lighting gradient raises
spread just as wear does.

**Spread, not sd/mean, deliberately:** sd is dominated by the 86–92% of a surface that is intact.
That is how R7 shipped wear changing 1.92% of pixels against a 1.69% baseline and was believed fine.

### BezelBlack retired — and my finding for it was too strong

`TVBody` wore `BezelBlack #3C3C38`. I reported it as **not visible** and Allen retired it on that
basis — *"two body materials, not three; one of them invisible is a maintenance lie."* `TVBody` now
wears the same painted steel as the enclosure via a single shared factory
(`GrayboxRoomBuilder.HousingSteelMat()`), so the institutional metal has **one** definition; R19(b)
already had to un-drift that colour once and a second copy is how that recurs silently.

**The retirement is not a no-op and my claim was wrong.** Measured against the pre-retirement set:

| frame | max diff | pixels changed |
|---|---|---|
| conformance wide | 1 | 0 — unchanged |
| conformance seated | 92 | **170,389** |
| standing view | 153 | **28,520** |

The change is a narrow strip at the far-left frame edge plus corner slivers — the bezel's exposed
border. The housing covers it on the right and bottom, which is where I sampled and found rivets;
**on the left it is exposed.** The correct statement is **"not measurable"** — no surface-pure region
is obtainable where it is exposed, because there it is a thin strip against housing of near-identical
value — *not* "not visible." Allen re-confirmed the retirement on his authority after the correction,
and the orchestrator has it on the DD reconciliation list as a **visible change to a design-verified
room**. The R25 package is its re-review evidence.

Lesson worth keeping: *"I could not measure it"* and *"it is not there"* are different claims, and
the harness can only ever support the first.

### Gates 6–8 are VOID again, by design

Allen walked and passed them at `9e1b4e4`. The certification is keyed to the scene's content
fingerprint, and the retirement changed it, so it **expired itself**. Geometry is untouched and his
clearance verdict is very likely still good, but no tool may re-issue a human gate — re-certify with
`--certify-human-gates <commit>` only on a human's word.

### Suites, at the merge round

`EditMode 73/73`, `PlayMode 20/20`, 0 failed, 0 skipped, results XML written both times.

First attempt reported exit 0 with **no results file at all**: `-runTests` had been given `-quit`,
and the test runner closes the editor itself, so `-quit` raced it to the exit. Exit 0 proved nothing
— §9.4's own warning, walked into while checking for it elsewhere. **Never pass `-quit` with
`-runTests`,** and always confirm the XML exists before believing a green suite.

### Idempotence, measured at last — §1.5 holds in content, not in bytes

Law §1.5 says *"a rebuild reproduces the room exactly."* Every previous run of §9.2 ("run the builder
twice") **asserted** this without ever diffing the two outputs. Measured 2026-08-03:

```
committed  b16bbd38…      run1  73d29510…      run2  c4d033ee…      all three differ
```

**But normalise `fileID`s and anchors and the difference is exactly zero** — 13270 lines each way, 0
only-in-committed, 0 only-in-current. Every byte of drift is Unity reassigning anchor fileIDs on
rebuild. The room's *content* reproduces exactly; its *serialisation* does not.

Two consequences worth carrying:

1. **§9.2 can never be a byte comparison.** "Run the builder twice and compare" only means anything
   against a fileID-normalised multiset. A byte diff will always be red and always be meaningless.
2. **A rebuild buries a real change.** The post move committed at `9e1b4e4` carried ~9663 changed
   lines in `Room.unity`, of which exactly one mattered. A reviewer cannot see a genuine geometry
   change in that noise, which is worth remembering before trusting any scene diff by eye.

Not a defect in the room, and not something to "fix" in the builder — it is a property of Unity's
serialiser. But the law's wording invites the byte reading, and the byte reading is false.

**A rebuild also drops the working tree out of sync with the baked scene.** After any bare builder
run, `git checkout -- unity/SBR/Assets/Scenes/Room.unity` restores the committed, baked, gate-verified
scene. (`md5sum` will still disagree with the committed blob afterwards — the repo converts LF→CRLF on
checkout. `git status` is the authority, not the hash.)

### Open questions with other seats

1. **R22 walkthrough** — Allen. Gates 6–8 void until then. Geometry has landed (below), so this is
   unblocked and needs only the editor lease.
2. ~~`PhoneScreen` ownership~~ — **answered by R28**; narrow re-ask outstanding (see above).
3. ~~Gate 2 `m_IsActive`~~ — **answered by R29 and built.**
4. **Law 1.1's window pool** — see above. Still the live one.

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
