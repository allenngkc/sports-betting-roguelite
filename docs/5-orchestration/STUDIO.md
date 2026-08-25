# Studio Orchestration — v2 (2026-07-30)

Replaces the disposed principal-tech-lead coordinator (`~/orca/coordination/`) and the
Orca Control Desk dashboard (`tools/orca-dashboard/`), both removed 2026-07-30 on Allen's
decision. Claude holds all leads; the brief handoff of leads to GPT/Codex (2026-07-28) is
reverted.

## Roles

- **Allen — Creative Director, Studio Owner, final authority.**
  Vision, priorities, design acceptance, scope, licensing, what merges.
- **Orchestrator — Claude (Fable 5), seated in the `main-2` session.**
  Milestone planning, worktree assignment, cross-worktree dependencies, merge order,
  integration approval, Unity scheduling. Does not implement slice code. Treated as a
  scarce resource: enters for planning, disputes, architecture, and integration — not
  routine status relay.
- **Design Director — Claude Code (Opus 5, max effort), its own Orca terminal in
  `main-2` (seat moved from Claude Design per Allen, 2026-08-08; the
  claude.ai/design project stays as Allen's visual gallery via DesignSync).**
  Positioned alongside the orchestrator, not under engineering. Direct conversations
  with Allen. Owns every design decision: UI design flows, interaction flows, visual
  direction, art direction, 3D modeling. Produces design specifications and does
  post-implementation design review. Does not assign production work to leads —
  approved specs flow through the orchestrator.
- **Worktree leads — Claude (Opus 5, max effort), one per active worktree.**
  Opus 5 max is the standing seat spec (Allen, 2026-08-01; re-affirmed for all
  new seats 2026-08-12) — whoever seats a lead sets `/model opus` + `/effort max`
  as part of seating, every time. Fast mode for leads is approved in principle
  (Allen, 2026-08-15) but NOT yet in the spec — pending confirmation it works on
  the Max plan without usage credits; see speed-brief-2026-08-15.md item 1.
  Own the local plan, file ownership, delegation, review, verification, commits, and
  the integration handoff. Implement approved design specs; they make essentially no
  design decisions. Contract lives at `docs/handoffs/<worktree>.md` (root
  `handoff.md` retired 2026-07-31 — every worktree root is the same repo path, so
  committed root contracts collide at merge).
- **Sub-agents — Sonnet 5 by default, at most two per lead at once.**
  Delegation is the expected operating mode, not an option (Allen, 2026-07-31):
  implementation, testing, validation, bulk reading, and other grunt work go to
  bounded sub-agents; the lead plans, dispatches, reviews diffs, and integrates. A
  lead doing sustained grunt work itself is a contract deviation. Use an Opus 5
  sub-agent only for genuinely hard dispatches (architecture-adjacent, subtle
  concurrency, gnarly debugging). Every dispatch names allowed files, forbidden
  files, required evidence, and an exit gate; sub-agents never commit unless the
  dispatch says so. **Small items are not an exemption (Allen, 2026-08-15):**
  the register workflow produces many small tasks, and "too small to delegate"
  one-at-a-time is how a lead ends up doing 400 edits by hand — bundle related
  small items into ONE dispatch (six string fixes = one Sonnet agent carrying
  all six, with per-item evidence). Delegation is measured mechanically
  (`tools/delegation-audit.py`); sustained hands-on volume with zero spawns is
  a recorded contract deviation.

## Worktree registry

| Worktree | Branch | Lead | State |
| --- | --- | --- | --- |
| `main-2` | `main` | Orchestrator seat | Integration |
| `tv-theater` | `tv-theater` | Claude (Opus 5) | Active (seated 2026-08-16 - the resolved-leg column spec + the consolidated C46 sweep; handoff `docs/handoffs/tv-theater.md`) |
| `surething-ui` | `surething-ui` | Claude (Opus 5) | Retired 2026-08-13 — Phase L merged and Design-verified; worktree deleted on Allen's order; baseline evidence preserved under `docs/design/evidence-of-record/` |
| `surething-ui-2` | `surething-ui-2` | Claude (Opus 5) | Retired 2026-08-16 — same-game screens complete end to end and Design-verified; fully merged; worktree deleted on Allen's order |
| `room-refinement` | `room-refinement` | Claude (Opus 5) | Retired 2026-08-14 — room arc closed cycle 219, fully merged, zero commits ahead; worktree deleted on Allen's order (T65 closed by V6, no room dependency remained) |
| `tv-sweat` | `slice/tv-sweat-refinement` | Claude (Opus 5) | Retired 2026-08-16 — Phase T + stats-panel phase both Design-verified and closed; fully merged; worktree deleted on Allen's order (future TV phases re-seat fresh) |
| `markets-2` | `markets-2` | Claude (Opus 5) | Retired 2026-08-11 — fully merged (42d3f5a, validated); worktree deleted on Allen's order |
| `markets-pregame` | `markets-pregame` | Claude (Opus 5) | Seat EMPTY (rotated out 2026-08-23 at 688k after the CONSOLE SURFACES build merged - handoff `docs/handoffs/markets-pregame.md`; worktree kept for the lane's next phase; re-seat fresh) |
| `theater-engine` | `theater-engine` | Claude (Opus 5) | Retired 2026-08-25 - arm A phase 1 (per-fixture sweat, 9/9 campaign), the DoubleChance removal (9/9) and the AnchorSide single source all merged; no work pending; worktree + branch deleted on Allen's order; future engine work re-seats fresh |
| `sgp` | `sgp` | Claude (Opus 5) | Retired 2026-08-16 — F_0.6.0 complete: engine, gates (parallel harness), conditional cash-out all shipped and validated; fully merged; worktree deleted on Allen's order |
| `research` | `research` | Claude (Opus 5) | Active (seated 2026-08-12; Lane 3 — compulsion-loop reference research, docs-only. **GOVERNANCE EXCEPTION: Allen personally holds the DD role for this lane; findings route to Allen, not the DD seat; standing creative mandate to PROPOSE overriding any design doc — proposals, never edits**) |
| `Documents/CodingProjects/sports-betting-roguelite` | `feat/soccer-markets` | — | Retired 2026-07-31 — fully merged into main (56 behind); superseded by `markets-2` |

## Ownership rules

- Per-file boundaries: each active worktree's `handoff.md` file-ownership section is
  authoritative.
- Exclusive (one owner at a time): scenes, prefabs, ScriptableObjects, Input Actions,
  `ProjectSettings/**`, package manifests, shared serialized assets.
- Integration-only: `ProjectSettings/**` and package changes are decided by the
  orchestrator with Allen, never toggled casually inside a slice.
- Shared canonical docs (`docs/ARCHI.md`, `DECISIONS.md`, root plans): edited only at
  integration; leads record the needed update in their handoff instead.

## Decision routing

- **Design** (UI flows, interaction, visual direction, art, 3D modeling): the Design
  Director decides; Allen approves material choices. Leads almost never make design
  calls — spec gaps or ambiguity escalate lead → Design Director.
- **Critical / strategy** (scope, architecture, shared interfaces, ProjectSettings,
  package dependencies, merge order, cross-worktree conflicts): lead → orchestrator →
  Allen. Leads do not take these to Allen directly.
- **Routine implementation** inside a lead's file boundary: the lead decides.
- Design flow: Allen ↔ Design Director → exploration → Allen approval → approved spec →
  orchestrator plans and assigns → lead implements → evidence → Design Director review →
  Allen final approval.
- Leads report result-first, telegraphic, ending with `Done / Next / Risk / Need Allen`.
- Anything that reaches Allen is plain language: describe work by what it is in
  the product; register/tracking codes stay between the orchestrator, leads, and
  docs (at most one trailing parenthetical tag). See ORCHESTRATOR.md §2a.
- `docs/5-orchestration/STATUS.md` is the live board; the orchestrator updates it each
  sweep. No other status ledger exists.

## Autonomy policy (Allen, 2026-07-31)

Per-phase and per-gate approval by Allen is retired. The orchestrator runs an
autonomous sweep–dispatch–verify loop (ORCHESTRATOR.md §6) and involves Allen
only as Creative Director or for genuinely critical calls.

**Stops for Allen — nothing else does:**

- New or materially changed design direction (the Design Director brings it);
  within-direction spec detail no longer waits for him.
- Scope or milestone changes, licensing, spending money, anything that leaves
  the machine.
- A merge into `main` that fails the clean-merge checklist below. A merge that
  passes it proceeds autonomously and is logged.
- Anything irreversible: reverting approved work, deleting assets. (Force-push
  and history rewrites stay banned outright — not escalatable.)

**Clean-merge checklist** (autonomous merge only if all hold): tests at the
lead's stated baseline; no integration-only file drift (ProjectSettings,
packages) or drift already justified and reverted; handoff current; no open
conflict-register item touching the branch; merge applies without conflicts.
Any miss → Allen.

**Evidence-based gates:** a phase advances when the exit criteria in the lead's
approved plan are met with evidence artifacts. The orchestrator verifies and
advances — leads do not park waiting for a per-phase nod. Every autonomous
decision lands in `STATUS.md` under **Autonomous decisions (Allen veto
window)** with enough context to veto after the fact; a veto rolls that
decision back without reopening this policy.

**Design review is async:** the Design Director seat is reached through Allen
(Claude Design), so implementation does not block on DD review. The
orchestrator batches review requests at natural checkpoints so Allen relays
one bundle, not per-item pings. Only new-direction calls block work.

## Context hygiene

- The orchestrator and Design Director consume summaries and evidence artifacts only —
  never raw logs, tool-call transcripts, or a lead's conversation history.
- Leads keep detailed evidence (captures, reports, test output) in their own worktree
  and report telegraphically.
- An escalation carries the decision needed plus the minimum context to make it, not
  the full trail.

## Unity

- One Unity Editor instance open at a time across all worktrees.
- Warm-compile before `-executeMethod`; between batch runs wait for both the Unity
  process and `Temp/UnityLockfile` to clear (known trap).
- Batch/test runs: one worktree at a time; leads announce runs in their updates so the
  orchestrator can sequence them.

## Merge and integration

- Leads commit only on their own branch; no cross-branch merges by leads.
- Orchestrator sets merge order into `main`; integration and the canonical Unity
  validation pass happen in `main-2` after each merge.
- No force-push, no history rewrites, no auto-merge.
