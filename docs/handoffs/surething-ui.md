# surething-ui — lane handoff (same-game ticket: the screen half)

**Created:** 2026-08-14 · **Branch:** `surething-ui-2` (from main) · **Lead:** Claude (Opus 5)
**Charter:** `docs/sgp/step-5-presentation-plan.md` — the SCREEN phases, under Allen's Option C
ruling (2026-08-14): split by layer — sgp takes the model (`BetslipModel`), this lane takes the
screen. **Plan:** F_0.6.0 step 5.

## 1. Studio context (read in order)

- `docs/5-orchestration/STUDIO.md` — roles, ownership, merge protocol, autonomy policy.
- `docs/sgp/step-5-presentation-plan.md` — the plan this lane executes half of. Read it whole.
- `design/02-betting-math.md` § *Same-game tickets* — the model the surface presents.
- `docs/design/surething-design.md` — the owning doc for this surface. Canon binds; the register
  (`docs/design/REGISTER.md`) is history, the owning doc is what leads read.
- `docs/handoffs/sgp.md` — the sibling lane's state; its two standing follow-ups.

## 2. Scope

1. **The screen phases of step 5** as the plan states them: the same-match mark, the priced
   statement (the engine's joint price, never a product of leg odds — S73 forbids the surface to
   show the multiplied figure), the refusal as a stamped Blocked state carrying cause AND remedy
   (refusals take STAMP with cause-and-remedy — S73-am4), and the void arm's screen half when it
   reaches the surface.
2. **The inherited laptop margin-pin repair.** A pre-existing red on main: the stake-figure margin
   pin (1.96px, owned by M-04's 26px stake figure) reads 4.748 — the pin was never re-sourced when
   M-04 landed. The test's own prescription is the remedy: re-source the pin at the call site with
   the new split written out; NEVER shrink a figure to fit a pin. This is this lane's first,
   smallest unit — it re-greens main's suite.

## 3. Boundaries — merge-critical

- **`engine/**` is NOT yours.** Nothing in this lane touches the engine.
- **`BetslipModel.cs` is sgp's for this slice** (Allen's Option C). Do not edit it, including
  whitespace. Your phases consume what the model emits. Sequencing against sgp's P1–P3 goes
  through the orchestrator.
- The seam is canon: **the model emits parts, presentation composes words.**
- The screen is register-heavy: stamp ink versus toner, oxide, tracking, control sizes. Money
  language: money never abbreviates (C49); where copy and input disagree on a money control, the
  input is corrected to match the copy (C48); a fixed box carries an unstated face assumption —
  sweep the population (C46).
- Design questions route to the Design Director **through the orchestrator**. Capture evidence for
  every Design-facing change; claims about how something reads are made against frames (C11).

## 4. Rules inherited

- All seats: Opus 5 at max effort (standing spec).
- §7a settings churn discipline; explicit-path staging; suites green before merge requests.
- **Unity editor lease is serialized through the orchestrator. TV holds priority right now** —
  request a window, never assume one.
- **Inherited trap, live:** `dotnet` builds copy `SBR.Engine.dll` into the Unity tree and dirty a
  tracked LFS asset. This is a NON-engine lane: restore with checkout after every build; never
  commit it. (`unity/SBR/Assets/TutorialInfo/Icons/URP.png` also shows phantom-modified — never
  commit it either.)
- Report telegraphic, result-first: Done / Next / Risk / Need. The final text of a report is the
  deliverable — no bare register codes to Allen; plain words.
