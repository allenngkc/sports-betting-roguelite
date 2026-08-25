# The Linear switch — what changes the moment the full import lands

Status: READY, NOT ACTIVE. Apply in one pass immediately after
`tools/linear-import.py --all` completes and the checkpoint shows all six
surface projects populated. Until then the register stays live.

## 1. Freeze the register

Prepend to `docs/design/REGISTER.md`:

> **ARCHIVED 2026-08-25.** This table is frozen. Every row was migrated to
> Linear (team SBR, one project per surface); the old-ID → issue map is
> `docs/design/linear-import/checkpoint.json`. Laws (51 rows) were not
> migrated — they live in `constitution.md`. Do not add rows here. Rule on
> issues, not on this file.

Stop `register-entries-*.md` batch files. `tools/register-scan.js` retires.

## 2. Charter edits

### STUDIO.md — Decision routing
Replace the line `docs/5-orchestration/STATUS.md is the live board …` with:

> - **Linear is the system of record for work.** Every trackable item is an
>   issue in team SBR under a surface or slice project; the project's
>   description is the context pack an agent reads before any ticket. States
>   follow the design lifecycle (Backlog / Todo / In Progress / In Review /
>   Done / Canceled, with lifecycle + surface labels). Rulings are comments
>   on the issue. Nothing moves to Todo without an Expected behavior section
>   (`linear-templates.md`). `STATUS.md` remains the heartbeat and the
>   narrative board; it no longer tracks items.

### ORCHESTRATOR.md — §3b becomes "Design Director dockets"
> A docket is a list of issue identifiers, sent to the DD terminal as one
> message. The DD reads those issues and the linked owning-doc sections —
> nothing else — and rules by commenting on each issue and moving its state.
> The orchestrator lands any file edits the DD requests. No batch files.

### ORCHESTRATOR.md — §6a audit, "does dispatchable work exist" bullet
> Read the Linear queue per lane (state Todo, lane assignee or project). An
> issue in Todo without Expected behavior is not dispatchable — route it to
> the DD to fill first. Dispatch = assign the issue + tap the lead with the
> issue URL. New work from an Allen brief is decomposed into tickets under a
> slice project BEFORE any seat opens (a Sonnet sub-agent drafts, the
> orchestrator reviews against the template).

### DESIGN-DIRECTOR.md — §3 last paragraph and §6a seating prompt
Replace "Keep a one-line-per-item design-state register … No ticketing
system beyond that." with:
> Your state of record is Linear. You receive dockets as issue identifiers;
> you read the issue, its project context pack, and the owning-doc section
> it links; you rule by commenting on the issue and setting its state, and
> you fill Expected behavior on anything heading to Todo. You never read the
> archived register.

Seating prompt: drop the register read and `register-scan.js`; add "Confirm
the Linear MCP is connected (`/mcp`) and report the count of issues in
In Review across all surface projects."

### Lead handoffs (`docs/handoffs/*.md`)
Add to every active lane's §2 local plan:
> Work is pulled from Linear: issues assigned to this lane in Todo. Move to
> In Progress on start; link the commit; move to In Review with evidence
> links when done. One sub-agent dispatch = one issue URL + the project
> context pack; the Expected behavior section is the sub-agent's exit gate.

## 3. Seats need the MCP

MCP servers load at session start. After activation, the orchestrator and DD
seats must be rotated once (keeper reseat or manual) so the Linear tools are
present; lead seats pick it up at their next rotation.

## 4. Reverting

Delete the six surface projects in Linear, remove the archive banner, revert
the charter commits. The register was never modified by the import.
