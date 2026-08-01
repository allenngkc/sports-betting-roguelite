# Markets follow-up — lead ownership contract

**Created:** 2026-07-31 by the orchestrator, authorized by Allen
**Owner:** Claude (Opus 5) acting as markets/sim content lead
**Worktree:** `C:\Users\Allen\orca\workspaces\sports-betting-roguelite\markets-2`
**Branch:** `markets-2`
**Last updated:** 2026-07-31 (end of phase 1 + §4 first pass) by the lead
**Mission:** carry the F_0.4.0 soccer-market expansion to near-final against the
redesigned product.

## 1. Studio context (read these, in order)

1. `main-2/docs/5-orchestration/STUDIO.md` — roles, ownership rules, Unity rules.
2. `main-2/docs/5-orchestration/STATUS.md` — live board.
3. `main-2/docs/design/REGISTER.md` — every design ruling.
4. `main-2/docs/design/design-system/` — **spec-of-record** (tokens, components, laws).

Studio policy: autonomous loop. Evidence meets exit criteria → the orchestrator
advances you. Stops-for-Allen: design direction, scope/licensing/money,
failed-checklist merges, irreversibles. Report telegraphic, result-first, ending
`Done / Next / Risk / Need Allen`.

## 2. Scope — current

**Own:** market math, odds generation, market-type definitions, settlement logic,
sim support for markets, market content/data, and — since the widening below — the
market-owned **presentation** on the LEDGER surface. Tests for all of it.

**Scope widened 2026-07-31** (Allen via orchestrator) to the §4 presentation
reconciliation. The original "forbidden until `surething-ui` merges" gate is **spent**:
`surething-ui` merged at `2e97d13` and `room-refinement` at `bb457af`. The branch was
cut at `65a30d1`, 58 behind, and was fast-forwarded to main so the audit described
live code — if you re-seat and find yourself behind again, sync before auditing
anything.

**Still not yours:** `ProjectSettings/**` and package manifests (integration-only,
orchestrator + Allen). Scenes/prefabs owned elsewhere. Anything match-theater (`tv-sweat`).

## 3. State — what is done

Deliverable #1, the reconciliation gap-list, is committed and is the living document
for this seat: **`docs/1-plans/F_0.4.0_reconciliation-gap-list.md`**. Read it before
anything else; it carries every finding, correction and open question with evidence.

| Commit | What |
|---|---|
| `ebeac09` | gap-list (deliverable #1) |
| `cc40e8a` | M-01 — unique player names across the whole scorer board |
| `bf8a03e` | M-03 arm A — G7 market-coverage gate |
| `82011e1` | M-02 — scorer grading trap closed; F_0.4.0 doc debt cleared |
| `32b234c` | §4 first pass — market type/state conformance to spec-of-record |
| `25ef360` | editor-lease verification of that pass |

**Verified baselines** (re-establish these before trusting any change):
`dotnet test engine.tests` → **165/165** · Unity EditMode **75/75** · PlayMode
**39/39** · Unity compile 0 errors.
`dotnet run --project sim -c Release -- --gates --runs 1000 --seed-prefix TUNE` →
G1–G6 PASS, **G7 FAIL by design**, verdict NOT DONE, **exit code 1**.

## 4. Open, parked, and routed — do not restart these blind

- **Arm B — PARKED for Allen.** Broadening the skilled bot's market selection. It
  moves G2–G6; a gate flipping there is a balance finding about the economy, not a
  regression to tune back to green. Bring before/after gate tables side by side.
- **Scorer EV harness — milestone-level, queued with arm B.** Bots are policy-excluded
  from pricing AnytimeScorer, so no gate — G7 included — will ever cover it. G7 makes
  the hole visible; it does not close it.
- **D-01…D-08 are with the Design Director.** Market label vocabulary, the `RIDING`
  state word, the one-sided scorer market, the PLAYERS-tab overflow treatment, the
  `STAKE` label/figure split, ledger legs excluded from the lost treatment, blocker
  reasons lacking a remedy. **Build around them; do not pre-empt.** §4's conformance
  pass deliberately touched none of them.
- **Mid-sweat capture window — QUEUED** behind TV's T25.1 re-capture. Needed to close
  the one visually-unverified change: the live-leg toner colour (compiled, test-green,
  but no captured state exercises a `Live` leg).
- **Genuinely outstanding docs**, itemised in the plan's to-dos: `design/12` was never
  created, `DECISIONS.md` has no F_0.4.0 freeze entry, `ARCHI.md` covers Phases 1–3
  only, `PLAYTESTS.md` has zero F_0.4.0 entries.

## 5. Rules you inherit

- Delegation is the operating mode: bounded Sonnet sub-agents, at most two at once;
  every dispatch names allowed/forbidden files, evidence, and an exit gate; sub-agents
  never commit. **Review every diff yourself** — across five dispatches, two needed
  lead intervention, one of which was a real shipped-bug catch.
- One Unity editor studio-wide. Request a lease from the orchestrator and wait for the
  grant. Confirm process count + `Temp/UnityLockfile` at open; announce open and close.
- Evidence in `evidence/` (gitignored); bulk captures stay out of git.
- Commit only on this branch. No pushes, no merges, no history rewrites.
- Design questions route to the Design Director via the orchestrator.

## 6. Traps — all of these cost time here

- **`unity/SBR/Assets/Plugins/SBR/SBR.Engine.dll` is how the engine reaches Unity —
  DO NOT revert it.** Unity does not compile `engine/**`; it binds to this prebuilt
  binary, which `SBR.Engine.csproj:19-26` copies on every build *"so the Plugins DLL can
  never go stale"*. **When engine source changes, rebuild and COMMIT the DLL.** Build
  **Debug** (the default) — that is the committed convention; a Release build produces
  different, smaller bytes and would look like drift.
  *An earlier version of this file said the opposite — "`git checkout --` it before every
  commit". That was wrong and cost a full lease window: reverting it made Unity compile
  against a stale engine, so `MatchModel.Fields` did not exist and batch 4 failed to
  build. It also means any Unity run done after such a revert verified the OLD engine.*
- **Unity's exit code is not a completion signal.** A `-batchmode -quit` launch
  reported exit 0 while the editor kept importing for another ~16 minutes. Judge
  liveness by CPU time, log mtime/size and `Library` file count — not the exit code,
  and not the absence of output.
- **Serialize anything that builds.** Two agents running `dotnet` in one worktree race
  on `engine/obj` and the shared plugin DLL.
- **Look inside a directory before deleting it.** `artifacts/` holds 55 *tracked*
  room-visual-pass files alongside untracked capture output; `git check-ignore` says
  nothing about that. (Recovered byte-identical via `git checkout --`.)
- **`artifacts/` is not gitignored** while the SureThing capture tests write into
  `artifacts/surething-ui/`. One `git add -A` after a PlayMode run commits megabytes of
  PNGs. A fix must be targeted — `artifacts/room-visual-pass/**` is legitimately tracked.
- Unity segfaults on `-quit` sometimes (exit path only) — check state at open.
- Unity asmdef code is **invisible** to `dotnet build`. A green engine suite says
  nothing about whether the Unity project compiles.
- `--nologo` is not a valid `sim` flag.
