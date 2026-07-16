# Sports Betting Roguelite — Design Workspace

**Working title:** TBD (see OPEN-QUESTIONS.md)
**Status:** PHASE 2 — Unity vertical slice (gate: strangers replay it unprompted). Unity 6 project at `unity/SBR` (6000.5.3f1, scene `Room`). M1–M5 ✓; economy rework ✓ (playtest #8). **CURRENT: the CHARM EXPANSION is shipped and holdout-validated — 22 items (15 passives + 7 consumables), the DEALT-HAND shop (4+3 per visit, Ask for the Manager redeals), locked contract modifiers (Free Bet / Double-or-Nothing, one per ticket), Bookie's Marker, and Ref's Whistle (the pending window's gambling save, [M]/[R] on the TV). ALL SIX GATES PASS on held-out seeds** (sim-report-4-holdout2.md, 7.55M runs: skilled 7.0% in Allen's 5–8% band, median 5 — G3 re-banded: the dealt hand's build variance is the roguelite shape; G4 gates on passive-only counterfactual EV; martyr-worst guard holds; Manager is playtest-gated). Process: /grill-me-codex (5 adversarial rounds to APPROVED, PLAN-REVIEW-LOG.md) → Claude built it → 14-iteration tuning campaign → frozen holdout validation. Engine 144/144; Unity 40/40; console `dotnet run --project game-console`. Design story: design/09 → design/10 → design/11 (the expansion + campaign amendments). **Playtest #9 PASSED (2026-07-15): the strategy pillar lands ("comparing which relic works nice with another relic"), Ask for the Manager RATIFIED KEEP; Free Bet/Parachute feel + R5 cliff fairness carried as standing checks.** Backlog: progressive sweat density, mid-sweat agency ladder, WebGL golden-seed check, the 150+ fusion catalog. Phase 1 verdict: CONTINUE (2026-07-10, DECISIONS.md).
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
