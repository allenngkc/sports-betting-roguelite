# Plan Review Log: Match Theater Sweat (F_0.2.0)

Plan: `docs/1-plans/F_0.2.0_match-theater-sweat.plan.md` — Phase 2 vertical slice
centerpiece: the sweat's renderer goes from text ticker to a beat-driven 2D match theater.
Classified HIGH. Reviewer: Sol (gpt-5.6-sol, xhigh), fresh thread, MAX_ROUNDS=5.
(The charm-expansion plan + log live in git history — this file is per-milestone.)

Act 1 (grill) complete — 2026-07-17, Allen's rulings: **soccer pitch** stage (sketch);
**fewer, bigger beats** (engine budgets cut so every beat earns a scene); **momentum tape
in the slice**; **WebGL/itch deferred to a follow-up plan**; beats 3–8s tension-paced,
60–90s per full-density 3-leg sweat, final-leg ×2 budget retained.

Environment note: Sol's workspace runner failed all process spawns in this session
(Windows error 1312 — no logon session for the sandbox runner). Rounds 1–5 were conducted
by pasting the line-numbered frozen plan plus a verified ARCHI/source digest into the
thread; Sol reviewed the pasted text. Round 0's blocked `NEEDS_REWORK` was environmental,
not substantive. Also fixed en route: `run_python` in `skills/codex-plan-review/scripts/
_common.sh` now probes interpreters with a no-op (the Windows Store `python3` alias stub
passed `command -v` but ran nothing, killing Codex's stdout pipe).

## Round 1 — REQUEST_CHANGES (3 P1, 2 P2 — all accepted)

1. **P1 Playbook key not implementable** (types/tags/revealed-state conflated; "30 keys"
   miscounted). Fix: ordered resolver over actual `DramaEvent` fields — LegFinal first,
   then NearMiss tag, then base-by-(Type, dir), LeadChange/Swing as playback overlays;
   total by construction over all 40 combos, unreachable set enumerated.
2. **P1 Ledger commit timing undefined for a pending killing LegFinal.** Fix: goals
   commit on scene-playback completion; ledger frozen while the window is open; commits
   on resolution from the final ticket-local grade; four-path tests.
3. **P1 `EventBoundsForRound` semantics unspecified.** Fix: exact formula (1-based round,
   interpolation denominator, AwayFromZero, clamps, multiplier order).
4. **P2 Acceptance targets inconsistent** (early rounds emit 8–16 beats, not 12–20).
   Fix: targets scoped to full-density rounds; declared percentile criterion.
5. **P2 Idle "never signifies" vs win-prob-biased territory.** Fix: idle restates last
   revealed state, never implies new information; win-prob bar static between beats.

## Round 2 — REQUEST_CHANGES (1 new P1; 2 partials from round 1)

- **P1 Whistle-success presentation-authority conflict** (resolver reads `WinProbAfter`,
  ledger adds unstaged goals after a save scene). Fix: a suspended LegFinal's
  continuation is chosen from the final grade, never `WinProbAfter`; correction goals are
  staged before committing; **goal-playback invariant** — every ledger increment maps 1:1
  to a goal that visibly played.
- Partial: `EventBoundsForRound` early return bypassed the clamps → normalization now
  applies to both branches. Partial: remaining unconditional "60–90s" statements aligned
  to the full-density criterion (M-T3 gate included).

## Round 3 — REQUEST_CHANGES (1 new P1)

- **P1 Correction goals unbounded and uncosted** (a multi-goal synthetic deficit must
  compress into a fixed 8s LegFinal or break the acceptance math). Fix, both halves:
  **live-lead clamp** (`MaxLiveLead = 1`) — overflow goals stage as chalked-off variants
  (no increment), bounding any correction to ≤2 goals by construction — plus explicit
  per-correction-goal costing (2.5s sub-scenes) and acceptance that replays actual
  ledger rules per path with a declared 110s worst case. Stated as drama law too: the
  theater tells one-goal-game stories.

## Round 4 — REQUEST_CHANGES (2 P1, 2 P2 — all accepted)

1. **P1 Killing goal vs clamp at entry lead −1.** Fix: the killing shot routes through
   the same clamp/correction computation — chalks off when the score already satisfies
   Lost; entry-lead matrix (−1/0/+1 × Won/Lost) tested.
2. **P1 13s LegFinal vs the 3–8s band.** Fix: correction goals classified as separately
   timed sub-scenes outside the beat-scene band; ruling note, pacer, and tests aligned.
3. **P2 Chalked-off variant missing on BigPlay scenes.** Fix: variant belongs to every
   goal-producing scene (#1–#4) and the killing shot; both types tested.
4. **P2 Red-card variant violates the reserved-red palette law.** Fix: VAR-disallow
   only, neutral broadcast chrome; rationale inline.

## Round 5 — APPROVED

All prior findings verified addressed; no new P1/P2. Plan frozen 2026-07-18, pending
Allen's approval gate before TRIP-2 implementation (M-T1).
