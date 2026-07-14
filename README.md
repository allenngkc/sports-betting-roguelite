# Sports Betting Roguelite — Design Workspace

**Working title:** TBD (see OPEN-QUESTIONS.md)
**Status:** PHASE 2 — Unity vertical slice (gate: strangers replay it unprompted). Unity 6 project at `unity/SBR` (6000.5.3f1, scene `Room`). M1–M5 (engine determinism, graybox room, live TV sweat, in-room betting loop, bookie phone) ✓ through playtest #7. **CURRENT: the ECONOMY REWORK is proven — payment model + COMPS currency + the 3+3 catalog, all six sim gates PASS** (sim-report-2.md, 170k runs: naive 0% wins, engine mandatory, skilled 6.4% in the 5–8% band, EV arc at R3, farming guard holds, $74k Band-3 tails). Console proof: `dotnet run --project game-console` (payments, comps shop, mulligan window [M], timeout [T], scar counter). The Unity room now RUNS the proven economy (payments/comps/3+3, bookie remapped to creditor texts, [M] mulligan window + [T] timeout on the TV, BOOST at the betslip) — 40/40 room tests. Awaiting playtest #8 in the room. Design story: design/09 (CloverPit research) → design/10 (rework + Allen's rulings + the 150+ charm fusion pillar). Phase 1 artifacts still live: console `dotnet run --project game-console`; sim `dotnet run --project sim -- --runs 10000 --strategy all`. Phase 2 backlog carried: consumables/passives rework (+re-sim), mulligan fix, progressive sweat density, mid-sweat agency, WebGL golden-seed check. Phase 1 verdict: CONTINUE (2026-07-10, DECISIONS.md).
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
