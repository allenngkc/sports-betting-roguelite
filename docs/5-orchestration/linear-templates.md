# Linear templates — projects as context, tickets as action (Allen, 2026-08-25)

Principle: **an agent must be able to act on a ticket without hunting.** The
project carries the standing context (canon, laws, ownership, how to verify);
the ticket carries the one bounded change and ends with the expected behavior.
Nothing gets dispatched without an Expected behavior section — that is the
ticket's exit gate, not decoration.

## Project taxonomy

- **Surface projects** (standing): `Laptop — SureThing`, `TV — match theater`,
  `Room`, `Phone`, `Console`, `Cross-surface`. Each carries its owning design
  document and the laws that apply. Migrated register items land here.
- **Slice projects** (temporary): one per feature slice / F_0.x plan
  (`Pre-game market expansion`, `Same-game parlay`, …). Carry the plan and
  brief. Closed when the slice merges.
- An issue lives in exactly one project; the **surface label** still tags every
  issue so a slice ticket that touches the TV surface is findable from both
  sides. Cross-links (`blocked by`, `relates to`) carry dependencies.

## Project description — the context pack (markdown, top of every project)

```
## What this is
One paragraph in product terms: the surface or slice, who sees it, what it does.

## Canon (read before any ticket here)
- Owning document: <repo path + GitHub link>  (the authority for this surface)
- Constitution clauses that bind here: <C-numbers with one-line gloss each>
- Design system: docs/design/design-system/ (tokens, components, kits)
- Product laws: PRODUCT.md §… ; DECISIONS.md entries: …

## Ownership
- Files this project may touch: <paths>
- Files it must never touch: <paths>  (and who owns them)
- Worktree / lane currently executing: <name> (handoff: docs/handoffs/<lane>.md)

## How work here is verified
- Tests: <exact commands and the baseline counts>
- Evidence: <what captures/measurements are required, where they go, in-frame rule>
- Editor lease rules if Unity is involved.

## Standing risks / traps
Bullet list of known traps for this surface (flake signatures, build side
effects, stale docs to ignore).

## Plan / brief (slice projects only)
Link to the F_0.x plan and the Allen brief that authorized it.
```

## Issue description — the ticket template (sections in this order)

```
## Context
Two to four sentences: what this is in product terms, why it exists now, and
where it came from (Allen ruling / DD batch / brief / legacy register ID).
Enough that a fresh agent understands the ask without opening anything else.

## References
- Project context pack: (implicit — read the project description first)
- Owning-doc section: <path#section>
- Related issues: blocked by …, relates to …, supersedes …
- Evidence of record: <paths> (frames, measurements, prior verdicts)

## Scope
- In: the exact change.
- Out: what is explicitly NOT part of this ticket (name the adjacent things).
- Files: allowed <paths> · forbidden <paths>
- Size: one dispatch (if it isn't, split it before it leaves Todo).

## Spec / ruling
The authoritative text: the DD ruling or spec delta verbatim, with the exact
values (px, hex, ms, counts). Never paraphrased.

## Verification recipe
- Commands to run and the expected counts (e.g. EditMode 250/250).
- Evidence to produce: which captures, which measurement, where it lands.
- Who verifies: lead (self-check) → DD (design verification) → Allen (only if
  on the gated list).

## Expected behavior
Concrete, observable, checkable statements — the acceptance criteria. Each
line is something a reviewer can confirm true or false in the product or in a
measurement. Use "Given / when / then" where a flow is involved. This section
is REQUIRED before the issue may move to Todo; a ticket without it is a note,
not work.

---
Legacy ID: <old register id> · Lifecycle: <state> · Batch: <n> · Surface: <label>
```

## Rules

1. **No Expected behavior, no dispatch.** The orchestrator refuses to move an
   issue to Todo until the section exists and is checkable; the DD or the
   orchestrator writes it, never the implementing agent (who would grade its
   own homework).
2. **Verbatim spec.** Rulings are copied, not summarized. Summaries are how
   hallucinations enter.
3. **One dispatch per ticket.** A sub-agent receives one issue URL plus the
   project context pack; its exit gate is the Expected behavior section.
4. **Migration filling.** Migrated register rows get Context/Spec/References
   populated from the register automatically; Expected behavior is derived
   only where the ruling states an observable outcome, otherwise it is marked
   `TO DERIVE` and the ticket stays in Backlog until someone fills it.
5. **Project first, always.** A seat starting a ticket reads the project
   description before the ticket — the ticket assumes it.
