# Systems & product tracking — proposal (Allen asked for "a notch up")

Recorded by the studio-architect session. Status: PROPOSAL awaiting Allen's
three inputs (bottom). Nothing here executes until he says go.

## Diagnosis — why the Design Director "hallucinates"

The studio's entire tracking system is one markdown table plus a pile of
dated files:

- `docs/design/REGISTER.md`: 1.15 MB, 546 rows, average 1,700 characters per
  row, longest row 20,500 characters. It exceeds the read cap, so every DD
  session pages through it ~75 lines at a time and reconstructs the world
  from partial reads.
- 167 batch / register-entries files in `docs/design/`, plus precommits,
  reads, and briefs — rulings live in file #167, not on the item they rule.
- State vocabulary is inconsistent (RULED / Ruled / CLOSED / Closed /
  GRANTED / Granted / Design-verified …), so "what state is T65 in" is a
  reading-comprehension task, not a lookup.
- Recorded incidents of exactly this failure: batch 5 never transcribed and
  the DD re-ruled four items blind; a stale-numbered DD session issued
  T22–T27 that had to be renumbered T41–T46; unescaped pipes silently deleted
  ruling text from five rows; repeated row-count disputes at seating.

A fresh DD session that must rebuild continuity from a paged 1 MB table will
invent continuity. That is a substrate problem, not a model problem, and no
prompt fixes it.

## Target: Linear is the system of record for WORK; repo docs stay canon for TRUTH

- **Linear**: one team. Projects = lanes/slices (the F_0.x milestones).
  Issues = every trackable item — the 546 register rows migrate 1:1, prefix
  becomes a label (S laptop, T TV, R room, C cross-surface, K console,
  P phone, G gates). Workflow states = the design lifecycle: Exploration →
  Candidate → Approved → In Build → Implemented → Design-verified → Closed,
  plus Parked and Struck. Rulings are comments ON the issue; evidence is a
  link (commit SHA, frame path); the owning-doc section is linked in the
  description. Need-Allen items carry an `allen` label.
- **Repo keeps** the truth: owning docs (`constitution.md`, `*-design.md`),
  `docs/design/design-system/` canon, `DECISIONS.md`, plans. `REGISTER.md`
  freezes as an archive after migration; batch/entries files stop.
- **Orca** already links worktrees to Linear issues natively (worktree
  records carry `linkedLinearIssue` fields) — lane ↔ issue wiring is built in.

## Agent roles on Linear (the factory ingestion loop)

- **Orchestrator = intake.** Creates issues from Allen's rulings and briefs,
  assigns the lane, and reads the Linear queue per lane each cycle (state +
  assignee) instead of STATUS.md prose. STATUS.md shrinks to heartbeat +
  narrative. Dispatch = assign the issue + tap the lead.
- **DD** receives a docket as issue IDs, reads those issues plus the linked
  owning-doc section — never the register — and rules by comment + state
  change. Small, structured, verifiable context every session.
- **Leads** work from issues assigned to their lane: In Build → link commits
  → Implemented with evidence links. DD verifies → Design-verified.
- **Allen** reviews `allen`-labelled issues in Linear's UI, or via the
  orchestrator's plain-language relay, and rules by comment.

## Access

Linear ships an official MCP server (verified against Claude Code and
Linear docs, 2026-08-25). One command, machine-wide — user scope makes it
visible in every worktree, and the OAuth token is per-user, so ONE
authentication covers the orchestrator, DD, and every lead seat:

    claude mcp add --transport http linear --scope user https://mcp.linear.app/mcp
    claude mcp login linear        # browser OAuth once (or /mcp inside a session)

Tools exposed: find/search issues and projects, create issues and projects,
update state and metadata, comment. A read-only endpoint exists
(`https://mcp.linear.app/mcp/readonly`) if a seat should never write. The
migration script uses a Linear API key instead of OAuth.

## Migration (non-destructive; REGISTER.md is never deleted)

1. `tools/register-export.py` — parse REGISTER.md → JSON (id, surface, title,
   normalized state, batch, ruling text, links). Validate the count (546)
   and the state normalization table before anything touches Linear.
2. `tools/linear-import.py` — create issues with labels/states/comments;
   write an old-ID → Linear-ID map file for traceability.
3. **Dry run on 10 rows first**; Allen eyeballs them in Linear; then full
   import.
4. Freeze REGISTER.md with an archive banner; charters point at Linear.

## Design system: one canon, one gallery

- **Canon = repo** `docs/design/design-system/`, restricted to `tokens/`,
  `components/`, `guidelines/`, `ui_kits/`, `assets/`, `styles.css`, readme.
  Process debris currently mixed in (`DD Ruling Batch *.dc.html`,
  `register-entries-*.md`, `*-DRAFT.md`, `SKILL.md`) moves to
  `docs/design/archive/`. That mixing is why the Claude Design project reads
  as "messed up" — it mirrored the mess.
- **Claude Design project = read-only gallery**, rebuilt from canon: delete
  everything remote (`uploads/`, dd-inbox/outbox, batch files), push canon
  only, one-way repo → gallery, regenerated by the DD via DesignSync when
  canon changes. Never edited in Claude Design.
- **Figma: not now.** It is a hand-authoring surface; this studio generates
  design as code/HTML kits. Adding it creates a second source of truth with
  manual drift. Revisit only if Allen wants to hand-draw (Figma has an MCP
  when that day comes).

## Not doing

Vercel AI SDK or any custom agent runtime. Orca + Claude Code already IS the
runtime; the pieces a "software factory" has that this studio lacks are the
tracker (this brief) and CI gates (a later brief: tests/lint/sim summary
posted to the issue on merge). Rebuilding the runtime buys nothing.

## Needs from Allen — only these three

1. A Linear workspace/team (new or existing) and one API key for the
   migration script.
2. OAuth the Linear MCP in the orchestrator seat when it prompts.
3. Go on (a) the 10-row dry-run migration and (b) wiping + rebuilding the
   Claude Design project from repo canon.

## Phases

- P1 — connect MCP, write the export script, dry-run import, charter updates.
- P2 — full migration, freeze the register, roles switch to Linear, DD dockets
  by issue ID.
- P3 — design-system canon cleanup + gallery rebuild.
- P4 (later) — CI gates on merge.
