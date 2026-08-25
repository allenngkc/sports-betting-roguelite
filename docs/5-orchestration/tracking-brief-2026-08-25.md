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

## Allen's refinements (2026-08-25, same day) — now part of the target

### Unit of work = the ticket, not the worktree
Today a worktree is seated with "do the whole slice." Retired as a pattern.
The orchestrator decomposes an approved brief into Linear tickets BEFORE any
seat opens (a Sonnet sub-agent drafts, the orchestrator reviews). Every
ticket carries: goal, acceptance criteria, evidence required, files/ownership,
and a size cap of one dispatch. Leads pull tickets from their lane's queue;
each sub-agent dispatch is one ticket; a worktree is only the venue where
tickets execute. Product tracking = the ticket graph, always current, and the
DD/leads reason about one bounded ticket at a time — the other half of the
hallucination fix.

### CI/CD — staged, because Unity is the hard part
- **Stage 1 (written today, `.github/workflows/ci.yml`):** GitHub Actions on
  push to main and on PRs — build every .NET project (engine, engine.tests,
  game-console, game-console.tests, sim) and run the engine + console test
  suites on a Windows runner with .NET 10. No Unity, no LFS checkout, fast.
- **Stage 2:** sim smoke on PRs (small seed count) + nightly full gate
  campaign (parallelized per the speed brief) posting its table to the
  Linear issue.
- **Stage 3:** Unity EditMode/PlayMode in CI. Two routes, decide then:
  GameCI with a Unity licence secret (heavy — URP, package restore, long
  runs) or a self-hosted runner on Allen's machine (shares the editor with
  the single-editor law — needs the lease scheduler). Until then Unity
  validation stays local per the clean-merge checklist.
- **CD:** a Windows player build artifact on every main merge (stage 3);
  WebGL/itch stays deferred by Allen's earlier call.
- **Merge flow:** the orchestrator moves from local merges to PRs
  (`gh` CLI — not installed yet: `winget install GitHub.cli` + `gh auth
  login`), branch protection on main requires CI green, and the clean-merge
  checklist gains "CI green" as a hard line.

### Design system rebuild = a human-readable quick reference
Audience: Allen, 30-second lookups — not agents. The rebuilt Claude Design
gallery gets an index that reads top-down: Surfaces (laptop, TV, room, phone,
console) → each screen with an annotated capture and what it's for → the
components used → tokens → the laws in plain language (amber = money, biro
blue = your pick, oxide red = the house's mark, no pure black, facts ≥13px…).
Every page is generated from repo canon; nothing is authored in the gallery.

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

## Progress log

- **2026-08-25 — Linear connected, dry run DONE.** Workspace: one team `SBR`,
  default workflow. `tools/register-export.py` exports the register: 580
  rows, 0 problems, 0 duplicate IDs; 51 rows classified as laws (stay in the
  constitution, never become tickets); 16 need a human eye. Ten sampled rows
  were imported by a headless session through the Linear MCP into the
  isolated project **"Register migration — dry run (2026-08-25)"** as
  SBR-5…SBR-14 (`docs/design/linear-dryrun-result.txt` holds the old-ID →
  identifier map). Decisions embedded in the dry run:
  - **State mapping onto Linear's default workflow** (no custom states yet):
    Exploration/Candidate/Parked → Backlog · Approved → Todo · In Build →
    In Progress · Implemented → In Review (awaiting DD verification) ·
    Design-verified/Closed → Done · Struck → Canceled.
  - **Lifecycle nuance rides as labels** (`design-verified`, `parked`,
    `implemented`…) plus a surface label (`tv`, `laptop`, `room`,
    `cross-surface`, `console`, `phone`). The 12 labels were auto-created.
  - **Full import can run through the MCP in batches** by a headless session
    — no Linear API key required unless it proves too slow.
  - Awaiting Allen: review the dry-run project, then go/no-go on the full
    580-row import (minus laws), and whether to add custom workflow states
    (e.g. a real "Design-verified" column) in Linear settings or keep labels.

- **2026-08-25 — template pass on the dry run.** `linear-templates.md` written
  (project context packs; ticket ends in Expected behavior; no dispatch without
  it). All 10 dry-run issues re-shaped to it by a headless Opus session; the
  dry-run project now carries a SAMPLE TV-surface context pack drawn from
  `tv-design.md`, `constitution.md` and the tv-theater handoff. Expected
  behavior derived on 8/10; marked `TO DERIVE` on SBR-10 (S81 is "recorded,
  not ruled") and SBR-11 (P7 is quarantined) — the rule fired exactly where it
  should. **Two fixes for the full import:** (1) the sample generator truncated
  bodies at 1,500 chars — the full import must carry spec text verbatim and
  untruncated; (2) Linear's markdown normalizer re-flows nested emphasis
  around inline code — cosmetic, but rulings should be checked for it.

## Phases

- P1 — connect MCP, write the export script, dry-run import, charter updates.
- P2 — full migration, freeze the register, roles switch to Linear, DD dockets
  by issue ID.
- P3 — design-system canon cleanup + gallery rebuild.
- P4 (later) — CI gates on merge.
