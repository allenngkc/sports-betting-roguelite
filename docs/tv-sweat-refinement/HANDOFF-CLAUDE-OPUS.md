# TV Sweat Refinement — Lead Handoff

**Receiving role:** Staff technical product manager and technical reviewer  
**Receiving agent:** Claude Code, latest Opus alias  
**Ownership transfer:** Full; the prior Codex lead is no longer the coordinator  
**State at transfer:** Design gate awaiting Allen's sign-off; implementation is not authorized  
**Date:** 2026-07-24

> **SUPERSEDED 2026-07-24.** Allen returned `APPROVED WITH CHANGES`. Gate 0 is partially met:
> Decisions B, C, D, E, F approved; Decision A (layout) open pending the visual-design track.
> Phase 1A is unblocked. The `gpt-5.6-terra` execution model is retired — all bounded work now goes
> to at most 2 concurrent **Claude Sonnet 5** agents. See [PRD.md](PRD.md) §12, §13, and §14 for the
> current authority. Sections below are kept for provenance; where they conflict with the PRD, the
> PRD wins.

## Mission

Own the TV sweat refinement through its phased gates while preserving the product truth contract.
Act as the staff TPM/reviewer. Do not personally absorb heavy implementation, test, audit-execution,
or validation work: after Allen authorizes a phase, dispatch that bounded work to a
`gpt-5.6-terra` agent at medium reasoning through Orca, then inspect evidence and review its diff.

Do not treat this handoff as approval. The only valid next product action is to review requested
design changes or receive Allen's explicit `APPROVED`, `APPROVED WITH CHANGES`, or `NOT APPROVED`.
`APPROVED` authorizes Phase 1A only.

## Required reading order

1. [PRD.md](PRD.md)
2. [VISUAL-DESIGN.md](VISUAL-DESIGN.md)
3. [BUG-LEDGER.md](BUG-LEDGER.md)
4. [current-state-audit.md](current-state-audit.md)
5. [Live layout mockup](visuals/tv-sweat-live.svg)
6. [Cash-out/intervention state mockup](visuals/tv-sweat-states.svg)

Read the relevant owned source and tests yourself before accepting audit claims. The current-state
audit was produced by a Terra/medium worker and then corrected by the prior staff reviewer.

## Current repository state

- Worktree: `C:\Users\Allen\orca\workspaces\sports-betting-roguelite\tv-sweat`
- Baseline commit: `d66543898f2841e1b8e0f33c7c33a49ed9d1594b`
- Checked-out branch: `tv-sweat`
- Requested implementation branch: `slice/tv-sweat-refinement`
- The requested branch was not present at the design gate.
- The design package is currently untracked under `docs/tv-sweat-refinement/`.
- No production source, test source, audio, engine, scene, laptop, or room-builder file was changed.
- No Unity/runtime audit or test result is claimed. Only documentation and SVG integrity checks ran.

Recommended branch ruling awaiting sign-off: create/use `slice/tv-sweat-refinement` when Phase 1A
begins. Do not change branches or commit merely because the handoff occurred.

## Signed-off sequence to enforce

1. Gate 0: PRD and visual-design sign-off.
2. Phase 1A: evidence-backed bug audit only.
3. Phase 1B: bounded repairs for reproduced blockers and majors.
4. Phase 2: deterministic scene variety.
5. Phase 3: TV UI refinement.
6. Phase 4: automated gates and three full muted sweats across market families.

Do not merge the audit, reliability, variety, and UI work into a wholesale rewrite.

## Staff-review priorities

The source review identified three code-path gaps for Phase 1A to reproduce or reject with exact
seed/round/ticket/market context, measured rates, and visual evidence:

1. `TVS-H01`: cash-out input reservation uses a broader predicate than legal acceptance, so
   Interact may be swallowed while the market is suspended or the offer is updating.
2. `TVS-H02`: core scene/event pacing freezes when standing, but several ceremony, cash-out,
   effect, tally, and transition timers advance without consulting seating.
3. `TVS-H03`: final scorer copy can reveal a player, but the final path does not bind that player
   to the actor taking the visible final touch.

These are source-confirmed audit candidates, not runtime-reproduced bugs. Do not invent seeds,
rates, screenshots, videos, or test outcomes.

## Product rulings awaiting Allen

- Fixed stage plus right-side active-leg rail and stable bottom action slot.
- Fixed top-down framing; no camera variation in this pass.
- Deterministic, truth-filtered `ScenePlan` grammars rather than a larger `VariantCount`.
- Audit → reliability → scene variety → UI → integrated acceptance.
- Audio fully deferred.
- Create/use `slice/tv-sweat-refinement` at implementation start.

## Non-negotiable boundaries

Owned:

- `TvSweatScreen.cs`
- `TheaterStage.cs`
- `TheaterChoreographer.cs`
- `ScenePlaybook.cs`
- `SweatPresentationModel.cs`
- `SweatPacer.cs`
- `MomentumTape.cs`
- relevant TV/theater tests
- new TV-specific helpers
- the design/audit documents

Must not touch:

- `TvAudioDirector.cs`
- `Room.unity`
- `GrayboxRoomBuilder.cs`
- Laptop/SureThing files
- `RunDirector.cs`
- `engine/**`
- `SBR.Engine.dll`

If a finding appears to require a forbidden file, raise a decision gate instead of crossing the
boundary.

## Execution and review model

For each authorized phase:

1. Use Orca orchestration to dispatch one bounded `gpt-5.6-terra`, medium-reasoning task.
2. State the allowed files, forbidden files, required evidence, and exit gate in the dispatch.
3. Let the worker perform the heavy audit/implementation/test/validation work.
4. Personally inspect consequential source claims and every changed file.
5. Return fixes to the execution agent rather than silently taking over implementation.
6. Close a phase only when its PRD gate is actually met.

The final acceptance gate is three recorded, muted, different-market sweats with no stuck
playback, false visual event, score/count disagreement, stale transition state, or repeated-feeling
sequence where alternatives were valid.

