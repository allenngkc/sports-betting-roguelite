# Sports Betting Roguelite — Design Workspace

**Working title:** TBD (see OPEN-QUESTIONS.md)
**Status:** PHASE 2 — Unity vertical slice (gate: strangers replay it unprompted). Unity 6 project at `unity/SBR` (6000.5.3f1, scene `Room`). M1 (engine DLL determinism proven in Unity) and M2 (walkable first-person graybox room, playtest #3 approved) done; M3 (the TV plays the sweat, live from the real engine, seated-viewing rule) built — sit on the couch and the demo channel sweats real tickets. Next: M4 (real betting loop — laptop betslip/shop, phone bookie). Phase 1 artifacts still live: console `dotnet run --project game-console`; sim `dotnet run --project sim -- --runs 10000 --strategy all`; 133 tests green. Phase 2 backlog carried: consumables/passives rework (+re-sim), mulligan fix, progressive sweat density, mid-sweat agency. Phase 1 verdict: CONTINUE (2026-07-10, DECISIONS.md) — S3/S4 pass by sim, S2/S5 by playtest+audit.
**Owner:** Allen. **Collaborator:** Claude (design discussion, math, architecture, VFX implementation).

## What this space is

A living design bible. We discuss topics here session by session, converge on decisions, and only then write code. The rule of engagement:

1. Discussion happens in conversation, anchored to one of the docs below.
2. When we converge, the doc gets updated and the decision goes in `DECISIONS.md` with the date and the why.
3. Anything unresolved goes in `OPEN-QUESTIONS.md` so it isn't lost.
4. A doc is never "done" — but Phase 0 exits when the gate in `design/07-business-and-roadmap.md` is met.

## Index

| Doc | What it holds |
|---|---|
| **`PRD-prototype-v0.md`** | **Execution spec for the Phase 1 prototype — the current focus. DRAFT, awaiting Allen's sign-off** |
| `design/00-vision.md` | Pitch, fantasy, design pillars, tone, references, success criteria |
| `design/01-core-loop.md` | Run structure, profit targets, failure state, meta progression |
| `design/02-betting-math.md` | The four-number model, odds/EV/parlay/cash-out math, balance simulation plan |
| `design/03-mechanics-catalog.md` | The five design axes; gurus, insiders, relics, events; how mechanics interlock |
| `design/04-the-sweat.md` | The signature moment: leg-by-leg resolution and live cash-out, spec'd in detail |
| `design/05-architecture.md` | Headless C# core, Unity layer, effect hooks, data-driven content, RNG |
| `design/06-vfx-and-juice.md` | The juice stack, effect inventory, who builds what |
| `design/07-business-and-roadmap.md` | Market research, phases, validation gates, launch funnel |
| `design/08-art-direction.md` | Betting-app diegesis in a compact room; palette tokens; room-as-health-bar; juice mapping |
| `DECISIONS.md` | Append-only decision log |
| `OPEN-QUESTIONS.md` | Parking lot for everything undecided |
| `PLAYTESTS.md` | Human playtest log — findings, S-criteria signals, actions |
