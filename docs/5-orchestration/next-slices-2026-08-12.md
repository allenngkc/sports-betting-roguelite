# Next slices — Allen's rulings, 2026-08-12

Recorded by the studio-architect session from Allen's own words. The orchestrator
plans and executes: create the worktrees, write their handoff contracts (STUDIO.md
four-section shape), register them, seat Opus 5 leads, sequence around the current
freeze. Report lanes-live to Allen when seated.

## Lane 1 — Pre-game market expansion (new worktree)

Current pre-game set (corners totals, goals totals, cards, scorer) grows to the
full v1 pre-game vocabulary. Allen's batching doctrine applies: **all markets in
one campaign, one sim re-baseline** — no piecemeal additions, no repeated
restarts. The plan grill decides which of the frozen second-wave markets
(handicap, team totals, double chance, correct score, HT/FT) unfreeze; the
no-draws-in-v1 constraint (stat-line sampler conditions on the drawn winner)
stands unless the plan explicitly argues and Allen ratifies otherwise. Market
interface stays EV-auditable per the standing law.

## Lane 2 — Correlated parlays / same-game parlay (new worktree)

Allen: "users should be able to do a same game parlay, which we would need to do
some research on how it's done." Research-first, then the recorded dependency
chain (TV PRD §8.2A) in order:

1. Research: how real books price and settle SGPs; candidate correlation models.
2. Correlation model designed into `design/02-betting-math.md` — every payout's
   EV must remain writable for the Monte Carlo audit (current payout is a bare
   product of decimal odds, valid only for independent events).
3. Engine change lifting the one-pick-per-matchup guard (`Run.cs`).
4. Six-gate re-validation on held-out seeds (ticket pricing drives run economy).
5. Presentation last.

## NOT scheduled — Allen's explicit holds

- **In-play micro-markets: revisit later.** TV-sweat feature work comes first —
  a live-stats tab and similar in-sweat features (the PRD §8.8 stats panel and
  held cash-out preview already carry design) are the TV worktree's next phase
  after its current run-table item and the freeze lift.
- **Charms/relic interactions with markets: strictly after Lanes 1 and 2
  complete** (vocabulary frozen first, verbs second).
- **WebGL parity check: deferred** (2026-08-12).
- TV juice pack (Band-3 spectacle, RedZone cut): parked for a later talk.

## Lane 3 — Research worktree (new; special governance)

Lead fans out research agents over roguelite compulsion-loop references —
Balatro, CloverPit, Raccoin (lead verifies titles), and close neighbors — one
"fun autopsy" per game (result cadence, compulsion levers, session shape, meta
hooks), then a mapping doc onto SBR. Docs-only output under `docs/7-research/`;
no Unity, no product code.

**Governance exception (Allen, 2026-08-12; routing corrected same day):** Allen
personally holds the Design *authority* for this lane — the DD seat is not in
its loop. Reporting still follows the normal chain: lead → orchestrator → Allen.
The orchestrator relays findings and design proposals up to Allen and his
rulings back down; it does not route any of it to the DD. The lane has a
standing creative mandate: it may propose overriding ANY existing design doc
where research argues something better. Proposals, not edits — Allen ratifies;
canon changes land through the normal register flow after his word. Write this
into the lane's handoff and note it in STUDIO.md when registering the worktree.

## Editor note

Lanes 1–3 are editor-light (engine/sim/research). TV keeps its lease priority.
