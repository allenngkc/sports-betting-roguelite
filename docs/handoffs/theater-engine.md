# theater-engine — lane contract (seated 2026-08-21)

**Mandate:** PHASE 1 of the drawn-ending arm A — the engine's sweat-session and
probability-path restructure from per-leg to per-(ticket, fixture): N legs on one
match told ONCE, graded at one whistle. Allen ruled the fork (A) on 2026-08-21;
the spec (`docs/design/spec-drawn-ending-2026-08-19.md`) is approved as written.

## Read first, in order

1. `docs/5-orchestration/STUDIO.md` (roles, delegation contract, Unity lease).
2. `docs/design/t140-arm-a-plan-2026-08-21.md` — §3 PHASE 1 is your scope; §4 is
   the design input you need (all of it now RULED: §4.1–4.3 are batch 167).
3. `docs/5-orchestration/route-t140-cost-2026-08-19.md` — the TV lane's costing
   and the restructure table (SweatSession, DramaGenerator, _countLedger,
   live-leg locals). Two pre-build spec gaps are asserted in the plan's §2.
4. `docs/handoffs/sgp.md` — the joint model (`SameMatchModel`, `SameMatchPrice`,
   `Ticket.SameMatch`) is yours to build on, not re-derive.
5. Register rows named in the plan (`docs/design/REGISTER.md`, page by bytes):
   T140, T87-am2, T143, S85, T115-am, T142.

## Ownership

- **Owns:** `engine/**` for this restructure; `SBR.Engine.dll` — this lane
  **rebuilds AND COMMITS** the DLL with every engine change (stage it by explicit
  path; verify by loading it). Never `git add` a directory.
- **Does not own:** `unity/SBR/Assets/SBR/Runtime/**` (TV lane — phases 2–3
  land there AFTER this phase); `game-console/**` (markets lane);
  `ProjectSettings/**`, packages (integration-only). Never commit `URP.png`.
- Coordination: the TV lane's presentation (phase 3) consumes your session
  contract — publish the contract's shape in this file BEFORE changing it, and
  name every call site in `unity/` that your change breaks rather than fixing
  them yourself.

## Contract with design (already ruled — do not re-derive)

The telling contract (T140); grades land in LEG ORDER after ONE hold (T87-am2,
spec §3.2); the pending-loss window opens ONCE PER WHISTLE after every grade on
that fixture, naming every leg that died, and states when no single call saves
the ticket BEFORE the offer is presented (T143, S85); cash-out is a TICKET-level
fact — no leg's probability is ever shown alone (T143). §4.1–4.3 (batch 167): the
prose anchor under N live legs, the displayed win-probability's seed, the leg
counter (coupled to T91-cl).

## Evidence and gates

- Sim-harness first: the engine is fully testable without Unity (`dotnet test`
  under `engine.tests`; the gate campaign is bare `--gates`, floor 10,000 runs).
  Prove byte-identity on every ticket shape that does NOT contain a same-match
  pair — the restructure must be a no-op there.
- A new gate for the same-match pair: N grades at one whistle, one hold,
  leg-order grading, the window once. Report counts, not adjectives.
- Unity lease: you should not need the editor; if you do, ask the orchestrator.
- Delegation (STUDIO.md): bundled Sonnet dispatches; the lead plans, reviews,
  integrates. Sustained hands-on volume with zero spawns is a recorded deviation.

## Report

Result-first, telegraphic: Done / Next / Risk / Need Allen. Design questions →
orchestrator → DD. Scope/architecture → orchestrator.
