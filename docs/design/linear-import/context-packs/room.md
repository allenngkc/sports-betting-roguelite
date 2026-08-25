## What this is

The bunker the whole game is played from, and the grade every surface in it renders through:
geometry, light, material, atmosphere, dressing. **A cramped bunker at night in a wealthy high-tech
city that has no use for the occupant** — the room rots, the city outside is neon and functioning,
**the machines are nicer than the life.** Direction B *Vice Grip*, Palette 1, painterly
semi-realistic. It owns the TV, laptop and phone **objects** and the light they throw, never their
screens' contents. The light rig is the primary art tool: relief is gated by lighting, so "more
surface detail" is answered with light that varies across a surface.

## Canon (read before any ticket here)

- **Owning document:** `docs/design/room-design.md` — APPROVED, Allen 2026-07-31 (R13), the first
  surface document under C9; amended batches 15/16/17 (2026-08-08) ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/design/room-design.md
- **Constitution** (`docs/design/constitution.md` ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/design/constitution.md)
  **— clauses that bind here:**
  - **C9** — the owning doc is this surface's binding art authority.
  - **C22 / C22.1** — a ruling exists only as a register row; one finding, one ID.
  - **C11 / T19** — rendered evidence or no claim; that wear *reads* is claimed on frames, never on
    coverage counts.
  - **C17** — capture precedes rebuild. **C12** — frames travel in the DD import, not in git.
  - **§2.5 / §2.6** — measure the rendered thing, not the source (R19(a)'s "warm steel" was albedo
    arithmetic that did not survive to frame); a confounded measurement closes nothing.
  - **C18 §4.1 / §4.2 + C29** — an inventory names its members (colliders ratified at **29**, 27 Box
    + 2 Mesh, on `LaptopScreen` and `PhoneScreen`); every gate states its blind spot and reports its
    case count — `R7-F` was ruled the sixth vacuous green here: in-frame ≠ visible.
  - **C14 / C16** — 1:1 fidelity; only the platform makes a thing impossible.
  - **C20** — the unified grade is cross-surface, ruled at the DD seat, tuned by no lane alone.
  - **C55** — a capture must contain its subject.
- **Product laws:** `PRODUCT.md` §Operating Context carries the room palette and the light law
  (https://github.com/allenngkc/sports-betting-roguelite/blob/main/PRODUCT.md) — but the **cool-blue
  and money-colour palette laws (R4) are REVOKED** (Allen 2026-07-25) and four repo documents still
  assert them (C7 debt). `DECISIONS.md` — **TO CONFIRM**.
- **Anti-reference:** `design/08-art-direction.md`, deprecated for the room by Allen 2026-07-24 (T3).
- **Design system:** `docs/design/design-system/` ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/design/design-system

## Ownership

- **May touch:** `unity/SBR/Assets/SBR/Editor/{ProceduralSurfaceTextures,RoomArtDressing,ChamferedBoxMesh,ConduitMesh}.cs`,
  `Assets/SBR/Environment/**`, room-only materials, `tools/room_gate_check.py`, room tests and
  evidence, `docs/design/room-design.md`.
- **Shared hotspots, this lane's while it executes:** `GrayboxRoomBuilder.cs` and
  `unity/SBR/Assets/Scenes/Room.unity` — minimal isolated edits, announced first.
- **Must never touch:** `engine/**`; `SportsbookApp.cs`, `LaptopOs.cs`, other SureThing files;
  `TvSweatScreen.cs`, theater/pacing, TV UI; `RunDirector.cs`; `ProjectSettings/**`; `docs/ARCHI.md`,
  `DECISIONS.md`, root plans. The phone's **content** is nobody's — live engine data only (R28-am).
- **Worktree / lane:** `room-refinement` (handoff
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/handoffs/room-refinement.md) —
  **retired** at STATUS cycle 304; "recreate on demand, nothing pending". Lead is Claude (Opus 5).

## How work here is verified

- **Gate harness:** `python tools/room_gate_check.py`, **`--report PATH` on every run** — before it
  existed the claim *was* the artifact. Certify/revoke require a validated `--certified-at
  YYYY-MM-DD`, never defaulted. **Human gates 6–8** key to the scene's content fingerprint and expire
  when it changes; no tool may re-issue them — `--certify-human-gates <commit>` only on a human's
  word, and the basis must say fresh walk or standing verdict.
- **Builder validation** (handoff §9): warm-compile → run the builder twice, waiting for the Unity
  process *and* `Temp/UnityLockfile` → one `RoomArtRoot`, post-FX volume and expected lights →
  dressing adds zero colliders → collider dims and clearance unchanged.
- **Tests:** `./tools/run-unity-tests.ps1 -Platform EditMode`, then `-Platform PlayMode`, then
  `python tools/check_test_results.py <results.xml> --min-cases "…"`. **Never pass `-quit` with
  `-runTests`** — the runner closes the editor itself and `-quit` races it to exit 0 having written
  nothing. This lane's last figures were **EditMode 73/73, PlayMode 20/20** (2026-08-07); `main` is
  now far higher (342 / 152, `docs/handoffs/tv-theater.md`) — **re-baseline before quoting either.**
- **Wear:** `SBR.RoomViewCapture.CaptureWearAB -outDir <dir>` then `python tools/wear_ab_diff.py
  <dir>` — the only instrument that answers *does the wear read*.
- **Evidence:** the three ratified poses (standing 68°, seated TV 17°, focused laptop 30°) at
  matching exposure and framing, in the DD import (C11/C12); sets under
  `docs/design/dd-import/<set>/`, **untracked**. In-frame rule (C55).
- **Editor lease:** one Unity Editor across all worktrees, serialized through the orchestrator.
  **CI:** `.github/workflows/ci.yml` must conclude `success` on merged `main`.

## Standing risks / traps

- **Judge colour against the document, not the frames.** The graded captures read cool-blue and must
  not be sampled for colour (§1.1). **Captures near the laptop are contaminated** — that region
  renders C13's stale superseded package, not this room's light.
- **§1.5 idempotence is content, not bytes.** Unity reassigns anchor `fileID`s every rebuild, so a
  byte diff is always red and always meaningless — and a rebuild **buries** real changes. After any
  bare builder run, `git checkout -- unity/SBR/Assets/Scenes/Room.unity`; `md5sum` still disagrees
  (LF→CRLF), so `git status` is the authority.
- **The first Edit Mode render lands before the pipeline settles** (magenta TV panel); `WarmRender`
  discards three. A determinism control passed *twice* on a fault that repeats every run.
- **Frustum coverage is not visibility.** `ConduitDrip` went 30%→67% coverage and 34%→64% occluded;
  the frame changed by nothing. Use spread (p95−p5), not sd/mean.
- **A Unity run dirties more than `URP.png`** — also `ProjectSettings.asset` and
  `LiberationSans SDF - Fallback.asset`. `git checkout --` them; **stage by explicit path.**
- **No image generation in this harness** (handoff §7), and **don't compensate for the TV's temporary
  green spill** (C2 interim) — anything tuned against it is wrong twice.
