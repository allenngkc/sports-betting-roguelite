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
- **Design Director — Claude (Opus 5), its own dedicated session.**
  Positioned alongside the orchestrator, not under engineering. Direct conversations
  with Allen. Owns every design decision: UI design flows, interaction flows, visual
  direction, art direction, 3D modeling. Produces design specifications and does
  post-implementation design review. Does not assign production work to leads —
  approved specs flow through the orchestrator.
- **Worktree leads — Claude (Opus 5), one per active worktree.**
  Own the local plan, file ownership, delegation (at most two bounded sub-agents),
  review, verification, commits, and the integration handoff. Implement approved design
  specs; they make essentially no design decisions. Contract lives in the worktree's
  `handoff.md`.

## Worktree registry

| Worktree | Branch | Lead | State |
| --- | --- | --- | --- |
| `main-2` | `main` | Orchestrator seat | Integration |
| `surething-ui` | `surething-ui` | Claude (Opus 5) | Active |
| `room-refinement` | `room-refinement` | Claude (Opus 5) | Active |
| `tv-sweat` | `slice/tv-sweat-refinement` | Claude (Opus 5) | Active |
| `Documents/CodingProjects/sports-betting-roguelite` | `feat/soccer-markets` | — | Dormant (F_0.4.0 awaiting playtest); ignore for coordination |

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
- `docs/5-orchestration/STATUS.md` is the live board; the orchestrator updates it each
  sweep. No other status ledger exists.

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
