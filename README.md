# Sports Betting Roguelite — Design Workspace

**Working title:** TBD (see OPEN-QUESTIONS.md)
**Status:** PHASE 2 — Unity vertical slice (gate: strangers replay it unprompted). Unity 6 project at `unity/SBR` (6000.5.3f1, scene `Room`). M1–M5 (engine determinism, graybox room, live TV sweat, in-room betting loop, bookie phone) ✓ through playtest #7. **CURRENT: economy proven and playtested — playtest #8 ("I like the game loop") landed two changes: Timeout CUT (catalog is 3 passives + 2 consumables) and the Totem reworked to FULL DEFERRAL (bank untouched, payment × 1.5 onto the next one). All six gates re-held after retune** (sim-report-3.md, 50k/batch: skilled 6.2% in the 5–8% band, organic totem fires 50.3%, zero item flags; offer draw 2 → 1, consumable slots 2 → 3, mulligan 1.5 comps). Console proof: `dotnet run --project game-console` (payments, comps shop, mulligan window [M], scar counter). The Unity room runs the same economy (bookie as creditor texts, [M] mulligan window on the TV, BOOST at the betslip). Design story: design/09 (CloverPit research) → design/10 (rework + Allen's rulings + playtest #8 amendments + the 150+ charm fusion pillar). Phase 1 artifacts still live: console `dotnet run --project game-console`; sim `dotnet run --project sim -- --gates --runs 10000`. Phase 2 backlog carried: progressive sweat density, mid-sweat agency, WebGL golden-seed check. Phase 1 verdict: CONTINUE (2026-07-10, DECISIONS.md).
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
