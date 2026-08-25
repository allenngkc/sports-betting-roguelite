## What this is

The studio-wide project: **the items that belong to no single surface.** Three families — the
**C-numbered clauses and their amendments** (authority, evidence, deviation, and the register's own
bookkeeping), the **G-series** gate and copy items (`G1`, the leg-statement short forms, plus
`G1-am10`–`G1-am15`), and the **M-series** market items (`M1`, BTTS structural unreachability).
**Nothing here renders on its own.** A cross-surface ruling is *executed* in whichever lane owns the
surface it touches; it lives here because **no lane may settle it alone** — `C20`: cross-surface
artefacts are ruled at the DD seat with both slices present, the unified grade is one, no slice tunes
it unilaterally and no slice is blocked from escalating about it.

**Scale:** 56 items (`docs/design/linear-import/cross-surface.json`). **Pure standing laws are
deliberately excluded from the import** (`laws-excluded.json`, 51 rows — `C10`, `C11`, `C16`–`C20`,
`C22`, `C24`, `C25`, `C27`, `C29`–`C36`, `C39`, `C41`, `C42`, `C45`, `C51`, `C53`, `C55`, `C60`,
`C61` among them). They are not tickets; they bind every project from the constitution.

## Canon (read before any ticket here)

- **Owning document:** `docs/design/constitution.md` — **APPROVED, Allen 2026-08-03** (authority
  `C24`) ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/design/constitution.md
  It is the **authority-and-evidence layer**: who decides, what counts as evidence, what happens when
  a document and a build disagree. Deliberately thin, and in its own words it **contains no colour,
  no type, no layout and no palette** — *"It governs none of them. It governs how they are
  governed."* Nothing in it is new; every clause was already ruled and already in the register.
- **The sections a ticket here lands in:**
  - **§1 Authority** — `C9` two tiers (constitution + one owning document per surface); **§1.2
    precedence: Allen → this constitution → the surface's owning doc → the register row → the
    slice's specs**; `C1` latest document governs; **`C22` the tables are the canon** — a ruling
    exists when it is a row in `REGISTER.md`, not when it is written, sent, or built against;
    `C22.1` one ruling, one ID (the earlier governs); **§1.5** the seat's own errors are recorded as
    its own, naming the ruling as the defect and never the lead who implemented it faithfully.
  - **§2 Evidence** — `C11` rendered evidence or no claim, Design-verified included (a review package
    is its document **plus its frames**); `T19` rendered distinctness, never counts or enum breadth;
    `C17` capture precedes rebuild; `C12` frames travel in the import, not in git; **§2.5** measure
    the rendered thing, not the source; **§2.6** a confounded measurement closes nothing.
  - **§3 Deviation** — `C14` 1:1 fidelity is the bar, not the aspiration, deviations DD-signed before
    build; `C16` **only the platform makes a thing impossible, a design decision makes it expensive**
    (a signed deviation carries a named cost and an expiry); **§3.5** a bound is not a layout.
- **Owning documents this project does NOT own** (§1.1): `room-design.md`, `surething-design.md`,
  `tv-design.md`, `phone-design.md` — and **the console has none, by `K15`**.
- **Register:** `docs/design/REGISTER.md`, `## Cross-surface` section ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/design/REGISTER.md
- **Design system:** `docs/design/design-system/` ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/design/design-system
- **Product laws:** `PRODUCT.md` §… — **TO CONFIRM**; `DECISIONS.md` entries — **TO CONFIRM**.

## Ownership

- **May touch:** `docs/design/constitution.md`; `docs/design/REGISTER.md` (transcription only, per
  `C22` — the orchestrator transcribes a batch and reports the ID list back, **and the DD then reads
  the tables and never its own batch files**); `docs/design/register-entries-<date>-batch-N.md`;
  `tools/register-scan.js` ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/tools/register-scan.js
- **Must never touch:** any surface's owning document or implementation code — **a cross-surface
  ruling is built in the owning lane, by that lane's seat**; `ProjectSettings/**` and packages
  (integration-only, orchestrator with Allen); `docs/ARCHI.md`, `DECISIONS.md`, root plans
  (integration only — a lead records the needed update in its handoff instead).
- **Seats** (`docs/5-orchestration/STUDIO.md`): the **Design Director** (Claude Opus 5, max effort,
  own terminal in `main-2`) owns every design decision and rules cross-surface artefacts; the
  **orchestrator** (`main-2`) transcribes batches, sets merge order, holds the Unity schedule;
  **Allen** is final authority. **No single executing worktree — the surface lane named on the row
  executes.** Which lane, per item: **TO CONFIRM**.

## How work here is verified

- **Register integrity:** `node tools/register-scan.js` from the repo root — the DD seat's four
  standing scans in one runnable file; **exit code is non-zero if any check fails**, so it can gate a
  commit or an export. `C22-am7` requires the register be reconciled **files-to-log-entries**, never
  by reading the log's tail, and an export must run against a register whose **transcription backlog
  is ZERO** or it exports a partial world.
- **Code-carrying items verify in the lane that builds them:** `dotnet test
  engine.tests/SBR.Engine.Tests.csproj`, `dotnet test
  game-console.tests/SBR.ConsoleGame.Tests.csproj`, and `./tools/run-unity-tests.ps1 -Platform
  EditMode` then `-Platform PlayMode` for the Unity surfaces. **Baseline counts belong to the
  executing lane's handoff (`docs/handoffs/<lane>.md`), not to this pack.**
- **CI:** `.github/workflows/ci.yml` must conclude `success` on merged `main` — **CI green is in the
  clean-merge checklist** (STUDIO.md, 2026-08-25). `gh` is not installed: REST API or browser.
- **Evidence:** rendered frames at review distance, in the DD import, **untracked** (`C11`/`C12`);
  **the capture must contain its subject** (`C55`); a claim that variation reads is never made from
  counts (`T19`).
- **Editor lease:** one Unity Editor across all worktrees, serialized through the orchestrator.
  Warm-compile before `-executeMethod`; wait for the Unity process **and** `Temp/UnityLockfile`
  between runs.

## Standing risks / traps

- **A batch file is a draft.** Nothing is "canonical alongside" the register, no matter who has built
  against it (`C22`).
- **Duplicate IDs are this register's recurring defect** — `C22-am`…`C22-am5` ended at **nine
  collisions**, and each instrument that counted them carried the same blindness it was counting
  (backlog-blind, then inline-amendment-blind, then over-reporting by the exact mirror of how it
  under-reported). **Sweep BEFORE transcribing** (`C22-am6`).
- **A transcription gap in the MIDDLE is invisible** — the newest rows being present makes the
  register look current. It has already misled the lane that reported it (`C22-am7`).
- **The state cell is stale by construction**, and curating the open set by hand does not fix it
  (`C59-am`).
- **`C59` is the seat's own failure mode**, promoted from ~15 §1.5 corrections in one session:
  describing what a check *would* find instead of running it.
- **Constitution §1.1's table has gone stale repeatedly** — three of four rows in one correction, and
  twice in a single day (2026-08-09). **Check it against the owning documents at every seating;**
  that was ruled the remedy rather than a new clause.
- **`REGISTER.md` blows the Read token cap** — page it by bytes, not lines, and beware unescaped
  pipes in quoted values, which silently delete ruling text.
- **A ruling can cite stale code** — grep a property's real consumers before implementing anything
  that re-bases it.
