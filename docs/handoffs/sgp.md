# sgp — lane handoff (correlated parlays / same-game parlay)

**Created:** 2026-08-12 · **Branch:** `sgp` (from main) · **Lead:** Claude (Opus 5)
**Charter source:** `docs/5-orchestration/next-slices-2026-08-12.md` Lane 2 (Allen's rulings, e141eed)

## 1. Studio context (read these, in order)

- `docs/5-orchestration/STUDIO.md` — roles, ownership, merge protocol, autonomy policy.
- `docs/5-orchestration/next-slices-2026-08-12.md` — your charter. Allen: "users should be
  able to do a same game parlay, which we would need to do some research on how it's done."
- `design/02-betting-math.md` — the document your step 2 amends.
- TV PRD §8.2A — the recorded dependency chain you execute in order.

## 2. Scope — RESEARCH-FIRST, strict order

1. **Research**: how real books price and settle SGPs; candidate correlation models.
   Docs-only output; stage findings for the orchestrator.
2. **Correlation model designed into `design/02-betting-math.md`** — every payout's EV must
   remain writable for the Monte Carlo audit. The current payout is a bare product of
   decimal odds, valid ONLY for independent events — that invalidity is the whole reason
   this lane exists.
3. **Engine change** lifting the one-pick-per-matchup guard (`Run.cs`).
4. **Six-gate re-validation on held-out seeds** — ticket pricing drives run economy; the
   gates are not optional.
5. **Presentation last.**

Do not start a numbered step before its predecessor is accepted. Steps 2+ each stop at the
orchestrator before build.

## 3. State — fresh lane

Branch is main at creation. Nothing built. Editor-light; dotnet is your loop. Inherited
trap: dotnet builds dirty the tracked Unity DLL (LFS pointer — checkout to restore, verify
by loading, never commit).

## 4. Rules you inherit

- §7a settings churn discipline; explicit-path staging; suites green before merge requests.
- Money-language and market-presentation questions route to the Design Director through the
  orchestrator (presentation is step 5 — far away; flag early anyway if research finds UX
  consequences).
- Report telegraphic, result-first, Done/Next/Risk/Need.

First action: read §1, then begin step-1 research. Report your research plan (sources,
questions, output shape) before diving.
