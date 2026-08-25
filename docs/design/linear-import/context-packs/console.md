## What this is

The **playable text client** — `game-console/`, run with `dotnet run --project game-console`. It is
the studio's **fastest full-loop playtest surface** (`docs/ARCHI.md` §15): run and shop loop,
moneyline board, the `M n` market sheet, the pick grammar, the ticket confirmation and the sweat's
beats, all on an **80-column × 24-row monospace page**. It exercises every engine verb the Unity
room does, and **new engine features land here first.** It is **not a fifth in-fiction surface**: it
has no register of its own to invent, it borrows the laptop's.

## Canon (read before any ticket here)

- **Owning document: NONE — by `K15`** (RULED, DD 2026-08-19 batch 121): *"NO OWNING DOCUMENT. THE
  CONSOLE INHERITS."* Its **words, taxonomy, order and row grammar are the laptop's**
  (`docs/design/spec-market-surfaces-2026-08-17.md` plus `S89`–`S102`); its **evidence and authority
  rules are the constitution's**, unchanged. **IF IT EVER GROWS PAST THAT LIST IT HAS STOPPED
  INHERITING AND THE QUESTION RETURNS TO ALLEN.**
- **Product canon:** `design/00-vision.md` ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/design/00-vision.md — four pillars,
  and a feature that fights one gets cut: **the sweat is sacred** (never instant or skippable by
  default), **jargon is the mastery layer**, **every mechanic is mathematically legible**, **satire,
  not glorification**.
- **Constitution:** `docs/design/constitution.md` ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/design/constitution.md
  **— clauses that bind here:**
  - **§1.2 / `C22` / §1.5** — precedence is Allen → constitution → owning doc (none here) → the
    register row → the slice spec; a ruling exists only as a row in `REGISTER.md`; the seat's own
    errors are recorded as its own.
  - **`C19`** — priced offers are reachable. `K2` fails it *on the take, not the read*: `ShowDetail`
    prints all **84 offers**, `ParseOne` accepts **six kinds of fifteen**. **`C47`** — the match has
    three outcomes, a bet has two: **`MONEYLINE DRAW +259` prints and cannot be taken.**
  - **`C46`** — a fixed box assumes the face it was sized against (`K1`: the 62-column rule came from
    a title box, not from content). **§3.5** — 80 and 24 are stated constants; every layout derives.
  - **`C11` / `C12` / `C55` / `C60`** — rendered evidence or no claim; frames travel in the DD
    import, not git; a capture must contain its subject; a piped transcript cannot carry everything.
    **`C18` §4.2 / `C29`** — a gate states its blind spot; zero executed cases is a failure.
- **Medium spec — the authority for what the console differs on, short by construction:**
  `docs/design/spec-console-surfaces-2026-08-19.md` (Allen-approved, batch 121) — the page, the
  address, the pagination, the refusal points.
- **Design system:** `docs/design/design-system/` is the graphical surfaces' tokens; this one renders
  monospace text off the laptop's spec, so whether anything there binds is **TO CONFIRM**. **Product
  laws:** `PRODUCT.md` §… and `DECISIONS.md` entries — **TO CONFIRM**.

## Ownership

- **May touch:** `game-console/**` ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/game-console and
  `game-console.tests/**` ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/game-console.tests
- **One composer, two surfaces:** `game-console` links `MarketSheet.cs` and `MarketDestinations.cs`
  **by source** (`SBR.ConsoleGame.csproj`) — **moving them changes the engine and forces a rebuild of
  the tracked `SBR.Engine.dll`, which this lane must never commit.**
- **Must never touch:** `engine/**` (**not engine-owning, and should stay that way**); `unity/**`;
  `ProjectSettings/**` and packages (integration-only); `docs/ARCHI.md`, `DECISIONS.md`, root plans.
- **Lane:** worktree `markets-pregame`, **seat EMPTY** — rotated out 2026-08-23 after the console
  surfaces build merged; worktree kept, **re-seat fresh.** Handoff ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/handoffs/markets-pregame.md

## How work here is verified

- **Tests:** `dotnet test game-console.tests/SBR.ConsoleGame.Tests.csproj` — **baseline 24 / 0 / 0**;
  then `dotnet test engine.tests/SBR.Engine.Tests.csproj` — **329 / 0 / 1 skipped** (both
  `docs/handoffs/theater-engine.md`, 2026-08-25). Build with **`-p:SbrUnityPluginDir=<scratch>`** so
  the tracked Unity DLL stays clean; CI does the same via `SBR_PLUGIN_DIR`.
- **CI:** `.github/workflows/ci.yml` builds the five .NET projects and runs the engine and console
  test steps; must conclude `success` on merged `main` (`gh` is not installed — REST API or browser).
- **Evidence is self-shootable:** pipe stdin into the exe — **no capture window, no editor lease.**
  Sets land under `docs/design/dd-import/<set>/` and **stay untracked** (`C12`). Set of record:
  `console-build-2026-08-21/` (B1–B8 + README).
- **Not shootable:** colour (`B9`) and any state behind a keypress need a human at a real terminal.
  **Assert those in a gate (`C60`); do not shoot them.**

## Standing risks / traps

- **A piped transcript has NO VIEWPORT** (`K1`'s stated blind spot): it shows what the surface
  *printed*, never what the player *saw*. **`ConsoleColor` does not survive a redirect** — zero
  escape bytes, measured. **Prompts carry no trailing newline**, so a naive width sweep joins a
  prompt to the next screen and reports false violations; split them first.
- **`Hold` short-circuits on redirected input**, so keypress-gated states come back CLEAN.
  Mutation-test the gate, as `K17`'s was.
- **`BetslipModel.SideOn` cannot be called for non-moneyline kinds** — it answers *neither* for all
  five side-carrying kinds; `Pick.Side` / `Leg.Side` both throw.
- **`SBR.Engine.dll` goes dirty on its own** (it embeds HEAD's SHA) — `git checkout --` it, never
  commit it from here, and stage by explicit path.
- **Known open, excluded from the surfaces phase by scope:** the shop/sweat screens are ungated —
  **422 lines over 80 columns**, shop text to **214**, one screen at **32 rows**; the fix is a
  three-line `line.Length <= 80` gate. And **`K21`**: `SweatRenderer.cs:296` compares the telling's
  anchor leg against the highest leg index, so on a two-leg same-match ticket the final telling is
  fast-forwarded instead of sweated — **it wants a gate, not a capture.**
- **Batch 172's corrections:** the worst-case leader run is **16 dots, not 15**, and `B4`'s folio
  (`66–83 of 84`) is unreachable at the shipped geometry (`BodyRows` is 20).
