# markets-pregame — lane handoff

**Created:** 2026-08-12 · **Branch:** `markets-pregame` (from main) · **Lead:** Claude (Opus 5)
**Charter source:** `docs/5-orchestration/next-slices-2026-08-12.md` Lane 1 (Allen's rulings, e141eed)

## 1. Studio context (read these, in order)

- `docs/5-orchestration/STUDIO.md` — roles, ownership rules, merge protocol, autonomy policy.
- `docs/5-orchestration/next-slices-2026-08-12.md` — your lane's charter, Allen's own words.
- `docs/handoffs/markets-2.md` — your predecessor lane's full record: traps (§6 — all of them
  cost time there), the gate campaign forms, the G-series history. markets-2's worktree is
  retired; its branch and this file are the memory.
- Register IDs live in `docs/design/REGISTER.md`; the G-rows and C-laws bind sim work too.

## 2. Scope — current

Grow the pre-game set (corners totals, goals totals, cards, scorer) to the full v1 pre-game
vocabulary. **Allen's batching doctrine: all markets in ONE campaign, ONE sim re-baseline** —
no piecemeal additions, no repeated restarts.

- **Step 1 is a plan, not code**: the plan grill decides which frozen second-wave markets
  unfreeze (handicap, team totals, double chance, correct score, HT/FT). Write the plan,
  route it through the orchestrator for Allen's grill before any market lands.
- The **no-draws-in-v1 constraint stands** (the stat-line sampler conditions on the drawn
  winner) unless the plan explicitly argues otherwise AND Allen ratifies.
- The market interface stays **EV-auditable** per the standing law — every payout's EV
  writable for the Monte Carlo audit.

## 3. State — fresh lane

Branch is main at creation (Phase T's docs included). No code yet. The editor is
TV's-priority; this lane is editor-light (sim/engine) — dotnet suite is your fast loop.
Known trap inherited from markets-2: **dotnet builds dirty the tracked Unity DLL** — restore
by `git checkout` (it is an LFS pointer now), verify by loading, never commit it dirtied.

## 4. Rules you inherit

- Gate campaigns are bare `--gates` (Allen ruled: no `--runs`).
- §7a settings churn: never commit; cmp-verify → checkout.
- Explicit-path staging; suites green before any merge request; handoff current at close.
- Design questions route to the Design Director through the orchestrator; sim/economy
  rulings that touch player-facing money language may also need the register.
- Report to the orchestrator: telegraphic, result-first, Done/Next/Risk/Need.

First action: read the four §1 documents, then write the market plan (step 1). Report the
plan's location when drafted.
