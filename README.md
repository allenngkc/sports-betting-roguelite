# Sports Betting Roguelite — Design Workspace

**Working title:** TBD (see OPEN-QUESTIONS.md)
**Status:** Phase 1 — Weeks 1–3 done (engine core; drama + sweat + cash-out; effect system + 10 relics + shop; 128 tests green). Next: Week 4, the playable console client. Code lives in `engine/` (+ `engine.tests/`); `sim/` arrives Week 5.
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
| `DECISIONS.md` | Append-only decision log |
| `OPEN-QUESTIONS.md` | Parking lot for everything undecided |
