## What this is

The hardened display bolted to the bunker wall, and everything that renders on it. **Maintained
industrial equipment** (T1): a decade-old panel that **works perfectly** and was **installed by an
institution** — riveted steel, chipped paint, stencilled code, conduit continuous with the room's
pipe runs. The register is **loud — an instrument you watch**. This is the **only** surface
permitted to show score, clock, win-probability movement or an outcome. **The laptop decides; the
TV reveals.** The housing, glass, dust and unified grade are room props: a flat capture is a design
reference, and **the in-room render at the seated camera is the only valid acceptance view.**

## Canon (read before any ticket here)

- **Owning document:** `docs/design/tv-design.md` — RATIFIED, Allen 2026-08-07 (C26-am2), amended
  2026-08-11 (batch 32, Phase T) ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/design/tv-design.md
- **Constitution** (`docs/design/constitution.md` ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/design/constitution.md)
  **— clauses that bind here:**
  - **C9** — the owning doc above is this surface's binding art authority.
  - **C22 / C22.1** — a ruling exists only as a register row; one finding, one ID (this doc's own C34→C35 renumber is that clause working).
  - **C11** — rendered evidence or no claim, Design-verified included.
  - **T19** — rendered *distinctness*: seed counts, signature diversity and enum breadth are never evidence that variation reads.
  - **T18** — compose, don't multiply: variety adds a value to a dimension, never a cell to a cross-product.
  - **C17** — capture precedes rebuild; T26 dissolved on frames and a rebuild was cancelled.
  - **C12** — frames travel in the DD import, not in git.
  - **C14 / C16** — 1:1 fidelity; only the platform makes a thing impossible, a design decision makes it expensive.
  - **C10** — wrong in kind is deleted, not dimmed (the full-field gold and oxide washes, T40).
  - **C18 §4.2 + C29** — a gate states its blind spot, and every run reports its executed case count; two of the four founding vacuous greens were this surface's.
  - **C36 / C37 / C41** — the instrument laws: a control brackets only what its samples enclose; a null is void unless success was resolvable; a prediction off a contaminated frame is a floor, not a target (C41's founding case is this surface's `_goldFlood`).
  - **§2.5 / §2.6** — measure the rendered thing, not the source; a confounded measurement closes nothing (T49).
  - **§3.5** — a bound is not a layout; T20, T47 and T51 are all this surface's.
  - **C19 / C20 / C23** — reachability; cross-surface artefacts ruled at the DD seat; build-corrects-doc, bounded.
- **Cross-surface register laws:** **C46 / -am2** (a fixed box assumes a face; the two sweep directions are coupled — retire champions and generate every producible form), **C47** (a draw is a match outcome, never a third bet outcome), **C55** (a capture must contain its subject).
- **Design system:** `docs/design/design-system/` ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/design/design-system
- **Anti-reference:** `design/08-art-direction.md`, deprecated here by T3 (Allen 2026-07-24).
- **Product laws:** PRODUCT.md §… — **TO CONFIRM**; DECISIONS.md entries — **TO CONFIRM**.

## Ownership

- **May touch:** `unity/SBR/Assets/SBR/Runtime/TvSweatScreen.cs`, theater/pacing code and TV UI, this surface's tests (`TvSweatScreenPaletteTests`, `TvSweatScreenLayoutGridTests` and siblings), `docs/design/tv-design.md`.
- **Must never touch:** `engine/**` and the laptop surface (`SportsbookApp.cs`, `LaptopOs.cs`) — the handoff's boundary reads *"the engine is not yours. The laptop surface is not yours."*; `game-console/**` (markets lane); `GrayboxRoomBuilder.cs`, `Assets/Scenes/Room.unity` (room lead); `ProjectSettings/**` and packages (integration-only); `docs/ARCHI.md`, `DECISIONS.md`, root plans (integration only).
- **Worktree / lane currently executing:** `tv-theater`, seated 2026-08-16, Claude (Opus 5, max effort) — handoff `docs/handoffs/tv-theater.md`. Predecessor `tv-sweat` retired 2026-08-16 — `docs/handoffs/tv-sweat.md`.

## How work here is verified

- **Tests, in order:** `dotnet test engine.tests/SBR.Engine.Tests.csproj` (golden byte-identity pin, `SharedTellingTests`, `TicketWinProbabilityTests`), then `./tools/run-unity-tests.ps1 -Platform EditMode`, then `-Platform PlayMode`.
- **Baseline at the 2026-08-25 rotation: EditMode 342 / 341 / 0 / 1 · PlayMode 152 / 125 / 0 / 27** (`docs/handoffs/tv-theater.md`); the PlayMode skips are `[Explicit]` by design.
- **CI:** `.github/workflows/ci.yml` must conclude `success` on merged `main` (`gh` is not installed — REST API or browser).
- **Evidence:** frames at review distance, in the DD import (C11/C12); sets land under `docs/design/dd-import/<set>/` and **stay untracked**. Acceptance is the in-room render at the seated camera, never a flat capture. **In-frame rule (C55):** assert the subject is in frame.
- **Editor lease:** one Unity Editor across all worktrees, serialized through the orchestrator (this lane has held priority). Warm-compile before `-executeMethod`; wait for the Unity process *and* `Temp/UnityLockfile` between runs.

## Standing risks / traps

- **PlayMode takes ~1000s and will always "time out" against the wrapper's default limit.** A timeout verdict means the wrapper gave up, **not** that the run failed — read the results file.
- **A difference is not a discriminator.** Two gates written to prove a new arm was reached were both false (`id != SheetName(leg)`; `Identity != "MARKET PICK"`). Assert what can only be true one way.
- **A source-scanning test breaks when you split a method.** `TvSweatScreenPaletteTests`' T69 scan is anchored to `private string LegStatement(` — re-point the anchor, don't widen the window.
- **`FitOrFallback` is reached by reflection from four gates;** widening its signature compiles and breaks all four silently. Check reflection callers before changing any private signature.
- **A Unity run dirties more than `URP.png`** — also `ProjectSettings.asset` and `LiberationSans SDF - Fallback.asset`. `git checkout --` them; **stage by explicit path.**
- **Carried risk, named not hidden:** `_pickedScorerGoals` is unproven on a beat — the model arm is gated in EditMode, the counter is not.
