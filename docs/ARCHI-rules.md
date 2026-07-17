# Architecture Documentation Rules

[ARCHI.md](ARCHI.md) documents the Sports Betting Roguelite (SBR) architecture. After each
task (new feature, refactor, bug fix), determine if ARCHI.md needs updating.

ARCHI.md is the *how it is built* companion to the design bible — design rationale goes in
`design/` + `DECISIONS.md`, never here. Do not duplicate decision-log content; reference it.

## When to Update

Update after ANY change that alters:

- Project structure (new projects, directories, moved files) — §4 Project Structure
- Technology stack (new dependencies, framework/Unity/SDK version changes) — §3 Technology Stack
- The engine's public surface or run state machine (new verbs, phases, hook changes) — §9 Game Loop Architecture, §12 Effect Pipeline & Item System
- RNG streams, seeding, or draw-order guarantees — §10 RNG & Determinism
- Sim gates, audit flags, bots, or the holdout protocol — §13 Economy & Balance Simulation
- Unity surfaces, interaction seams, or the engine/client boundary — §8, §14
- Build, test, or deployment processes — §6, §18, §20
- Data flow between layers — §16 Data Flow Diagrams (update the mermaid, not just prose)

## How to Update by Change Type

### Major Feature / Refactor

Review: §2 Overview, §4 Project Structure, §5 Core Architecture Principles (only if a law
changed — laws also need a DECISIONS.md entry), the affected subsystem section (§8–§15),
§16 Data Flow Diagrams, §18 Testing Strategy.

### Minor Feature / Enhancement

Update: the one subsystem section it touched (§8–§15), plus §4 if files were added.

### Bug Fix

Usually no update needed, unless it reveals/fixes an architectural flaw (then update the
subsystem section and note the constraint the fix encodes).

### Tuning / Balance Changes

Numbers in §7 Configuration (payment schedule, comps rate, offer counts) must match
`RunConfig.cs` — update them with the change. Gate definitions moved? Update §13.

### Dependency Changes

Update: §3 Technology Stack, and any affected subsystem sections.

## Guidelines

- Be precise and factual — reflect the actual codebase, reference actual file paths (`engine/Run.cs`, not "the run class")
- Be concise — enough detail to understand, not implementation specifics; the code is the spec of last resort
- Keep the tuned numbers in §7 synchronized with `RunConfig.cs` — a stale payment schedule is worse than none
- Update diagrams when data flow changes; a diagram that lies is a bug
- The ten laws in §5 change only with a DECISIONS.md entry — ARCHI.md records laws, it does not create them
- Check size occasionally: `bash skills/TRIP-compact/count-tokens.sh docs/ARCHI.md`; consider `TRIP-compact` past ~20k tokens
