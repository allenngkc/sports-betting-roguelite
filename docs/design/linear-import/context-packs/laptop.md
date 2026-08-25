## What this is

The occupant's own cheap machine at 2 a.m. — the SureThing sportsbook app, the LEDGER, and the
NOTEBOOK OS chrome they run inside. **The Annotated Form Guide**: the house prints a dense inverted
betting form; the player compares it, circles prices in ballpoint, works the margin, commits.
**Selection is annotation.** The register is **calm — a tool you operate**. It owns slate, markets,
working slip, stake, staging, lock, shop and placed tickets. **The laptop decides; the TV reveals**
— MY BETS only mirrors what the TV already revealed (S35c). One fixed **1024 × 704** composition on
a ~0.32 × 0.22 m world-space surface, read at an angle. Not responsive, not a page.

## Canon (read before any ticket here)

- **Owning document:** `docs/design/surething-design.md` — APPROVED, Allen 2026-08-06 (C26-am) ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/design/surething-design.md
- **Constitution** (`docs/design/constitution.md` ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/design/constitution.md)
  **— clauses that bind here:**
  - **C9** — the owning doc above is this surface's binding art authority; precedence runs Allen → constitution → owning doc → register row → slice specs.
  - **C22 / C22.1** — a ruling exists only as a row in `REGISTER.md`; one finding, one ID.
  - **C11** — rendered evidence or no claim; how something *reads* is claimed on frames only.
  - **C12** — frames travel in the DD import, not in git.
  - **C17** — capture precedes rebuild; no verdict on a state no capture shows.
  - **C14 / C16** — 1:1 fidelity; only the platform makes a thing impossible, a design decision makes it expensive (a signed deviation with a named cost and expiry).
  - **C10** — wrong in kind is deleted and re-scoped, never tuned toward invisibility.
  - **C18 + C29** — a gate names its members and states its blind spot; every run reports its executed case count, and zero cases exits non-zero.
  - **C19** — a priced offer is reachable; lists scroll with S27's printed position rail.
  - **C23** — build-corrects-doc, but only for a named parameter with no measured law.
  - **§3.5** — a bound is not a layout; the dependent layout is re-derived in the same commit.
  - **C20** — cross-surface artefacts are ruled at the DD seat with both slices present.
- **Cross-surface register laws:** **C46 / -am / -am2** (a fixed box assumes the face it was sized against — sweep the population, retire champions), **C48** (copy is the input contract), **C49** (money is printed in full), **C47** (a bet has two outcomes; slip settlement language does not change), **C55** (a capture must contain its subject).
- **Design system:** `docs/design/design-system/` ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/design/design-system
- **Product laws:** PRODUCT.md §… — **TO CONFIRM**; DECISIONS.md entries — **TO CONFIRM**.

## Ownership

- **May touch:** `unity/SBR/Assets/SBR/Runtime/SportsbookApp.cs`, `.../Runtime/LaptopOs.cs`, this surface's tests (`SureThingEntryTests` and siblings), `docs/design/surething-design.md`, laptop specs under `docs/design/` (e.g. `spec-market-surfaces-2026-08-17.md`).
- **Must never touch:** `engine/**` (read-only here); `TvSweatScreen.cs`, theater/pacing, TV UI (TV lane); `game-console/**` (markets lane); `GrayboxRoomBuilder.cs`, `Assets/Scenes/Room.unity` (room lead); `ProjectSettings/**` and packages (integration-only, orchestrator with Allen); `docs/ARCHI.md`, `DECISIONS.md`, root plans (integration only).
- **Worktree / lane:** none active. `surething-ui` retired 2026-08-13, `surething-ui-2` retired 2026-08-16 (both merged, Design-verified); the laptop market-surfaces phase closed inside `markets-pregame`, seat EMPTY since 2026-08-23. Handoffs: `docs/handoffs/surething-ui.md`, `docs/handoffs/markets-pregame.md`.

## How work here is verified

- **Tests:** `dotnet test engine.tests/SBR.Engine.Tests.csproj -c Release`, then Unity through the C29 wrapper — `./tools/run-unity-tests.ps1 -Platform EditMode` / `-Platform PlayMode` (exit 3 = zero executed cases). **Laptop-only baselines: TO CONFIRM** — the recorded EditMode 342/341/0/1 · PlayMode 152/125/0/27 (2026-08-25) are whole-project figures from `docs/handoffs/tv-theater.md`.
- **CI:** `.github/workflows/ci.yml` must conclude `success` on merged `main`; `gh` is not installed — read it via the REST API or the browser.
- **Evidence:** frames at review distance (C11) carried in the DD import (C12); sets land under `docs/design/dd-import/<set>/` and **stay untracked**. **In-frame rule (C55):** assert the subject is in frame and measure in LOCAL space — this canvas is world-space and tilted.
- **Editor lease:** one Unity Editor across all worktrees, serialized through the orchestrator — request a window, never assume one. Warm-compile before `-executeMethod`; wait for the Unity process *and* `Temp/UnityLockfile` between runs.

## Standing risks / traps

- **`dotnet` builds copy `SBR.Engine.dll` into the Unity tree and dirty a tracked LFS asset.** Non-engine lane: `git checkout --` it after every build; never commit it.
- **`Assets/TutorialInfo/Icons/URP.png` is permanently phantom-modified;** a Unity run also dirties `ProjectSettings.asset` and `LiberationSans SDF - Fallback.asset`. Check them out; **stage by explicit path.**
- **The leader-dot residual was SEEN AND ACCEPTED — do not re-fix.** 59 rows print fewer than six dots, all club-prefixed team totals (`S96-am2`, DD-verified at close).
- **ENTRY geometry is derived, not free:** market column **700px**, price cell **160**, name cell **496** — slack that exists only because `WideBiroRing` is `Image.Type.Simple` with `preserveAspect` off. `SureThingEntryTests` pins that ring at **176 × 48**.
- **Bands are locked and none is headroom (R30).** Deficit yield order: spacing, repetition, nothing (S50).
- **Do not carry the TV's vocabulary here** — coarse grid, monumental type, institutional steel, brightness-only semantics, un-eased motion.
