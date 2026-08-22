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
Engine-DLL rule for THIS lane (corrected 2026-08-12 — the checkout-restore advice was the
predecessor's explicitly RETRACTED rule and cost a lease window): an engine-changing lane
**rebuilds and COMMITS** the DLL with its engine change; checkout-restore is only for lanes
that did NOT intend an engine change. Verify any restored/rebuilt DLL by loading it.

## 4. Rules you inherit

- Gate campaigns are bare `--gates` (Allen ruled: no `--runs`).
- §7a settings churn: never commit; cmp-verify → checkout.
- Explicit-path staging; suites green before any merge request; handoff current at close.
- Design questions route to the Design Director through the orchestrator; sim/economy
  rulings that touch player-facing money language may also need the register.
- Report to the orchestrator: telegraphic, result-first, Done/Next/Risk/Need.

First action: read the four §1 documents, then write the market plan (step 1). Report the
plan's location when drafted.

## Deviation notice on record (delegation, 2026-08-15 - reaches the NEXT seat at seating)

The delegation audit read this lane at 0 spawns across 286 tool calls (4-day audit, 2026-08-15). Delegation is contract
(STUDIO.md sub-agents bullet, 496bc4d): the lead plans, dispatches, reviews and
integrates; sustained solo grunt work is a recorded deviation. The batching
pattern, named: small items are not an exemption - bundle related small items
into ONE bounded Sonnet dispatch (six string fixes = one agent carrying all
six, with per-item evidence, allowed/forbidden files, and an exit gate).
Audited daily by `python tools/delegation-audit.py`.

---

## CONSOLE SURFACES PHASE — built 2026-08-21, evidence docked, awaiting DD read

Built to `docs/design/spec-console-surfaces-2026-08-19.md` (Allen approved, batch 121) under
`docs/5-orchestration/console-build-dispatch-2026-08-20.md`. Four commits:

- `bb14ac3` — the 80×24 page and the market sheet (§3–§6, §10)
- `2c01d7b` — `K6`'s address grammar with `K16`'s beat-prefix fix riding it, as bound
- `43301f4` — the evidence hook (one env var, default-inert)
- `15ad83d` — `K17-cl`, the narration anchor

**ONE COMPOSER, TWO SURFACES.** `game-console` links `MarketSheet.cs` and `MarketDestinations.cs`
**by source** (see `SBR.ConsoleGame.csproj`). They are pure C# over `SBR.Engine`; linking rather
than moving them into the engine is deliberate — moving changes the engine and forces a rebuild of
the tracked `SBR.Engine.dll`, **which this lane must never commit.** The console therefore
re-derives nothing: destinations, order, folio, and the matchup-global line numbers that ARE the
pick addresses all come from the laptop's gated model.

**Evidence:** `docs/design/dd-import/console-build-2026-08-21/` — B1–B8 plus README. **Untracked,
and it stays untracked**; the harness is versioned, the capture set is not. B1–B7 were shot at
`4f1b01e`; **B8 was re-shot at `15ad83d`** because `K17` changed the beats it captures.

### Owed / open when this seat next reports

- **`B9` (colour) is NOT this lane's** — a piped transcript carries zero ANSI bytes (measured), so
  it needs a human at a real terminal. With Allen or a capture seat.
- **The neither-branch LINES are routed to the DD**, per `K17-cl`'s own "NOT RULED". `T163-am`'s
  momentum lines transfer verbatim; **its goal lines do not exist**, and its mechanism cannot
  transfer because `DramaEvent` carries no actor. What ships is assembled from already-authored
  clauses and is marked `ASSEMBLED-NOT-AUTHORED` in `EventText.cs`.
- **DoubleChance scope is Allen's** (batch 170). Nothing here is built on it.

### Two measured corrections to the spec, for whoever amends it

- **§3 says 15 leader dots at worst; the surface prints 16.** `RowGeometry.OfferRow` uses a
  one-space gap where `Page.Leadered` uses two. The conclusion is unaffected; the number is one low.
- **§14's `B4` folio cannot read `66–83 of 84`** at the shipped geometry — `BodyRows` is 20, so a
  first page is always 20 rows. Those numbers are §5's illustrative example. The pin is met.

### DOCKET — an ungated surface, out of this spec's scope, NOT fixed here

`§1`/`§15` exclude the shop and `SweatRenderer`'s composition, so these were reported rather than
touched — but they are real `§13` gate 1/2 breaches on screens a player sees:

- **422 lines over 80** across shop/sweat screens (commit 1's sweep, 1,983 lines) — shop relic and
  consumable descriptions run to **214 columns**.
- **The sweat command hint is 82 columns**, identical in the DD's own `console-read-2026-08-19/`
  `A4-sweat.txt` — pre-existing, and now *inside* the evidence set because `§14` `B8` forces a
  sweat capture.
- One sweat screen is **32 rows** against a 24-row page.
- **The fix is a three-line `line.Length <= 80` gate over those screens** (DD's own words).

### Cross-lane finding for the TV lane

**§3 of `spec-neither-branch-lines` must NOT be deleted.** `DramaEvent` carries `LegIndex, Step,
TotalSteps, Type, WinProbAfter, Tag` and **no actor** — no scorer, no possession side — so §1's
slot change is unimplementable on the TV too without an engine change.

### Traps this phase paid for

- **`BetslipModel.SideOn` cannot be called for non-moneyline kinds** — it short-circuits and answers
  *neither* for all five side-carrying kinds; `Pick.Side`/`Leg.Side` both throw. A ruling naming it
  names the SHAPE of the answer, not a call site.
- **A piped transcript's prompts carry no trailing newline.** A naive width sweep concatenates them
  with the next screen and reports false violations. Split them first.
- **`ConsoleColor` does not survive a redirect** — zero escape bytes. Colour is not self-shootable.
