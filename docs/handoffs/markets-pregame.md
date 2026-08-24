# markets-pregame — lane handoff

**Created:** 2026-08-12 · **Branch:** `markets-pregame` (from main) · **Lead:** Claude (Opus 5)
**Charter source:** `docs/5-orchestration/next-slices-2026-08-12.md` Lane 1 (Allen's rulings, e141eed)

> **STATE AT ROTATION — 2026-08-22. READ THIS BEFORE §2 AND §3, WHICH ARE STALE.**
> §2 and §3 are the 2026-08-12 charter and still say *"No code yet"* and *"Step 1 is a plan, not
> code."* Both were discharged long ago. **Three phases have shipped from this lane** — the v1
> pre-game vocabulary, the SureThing market surfaces (Design-verified), and the console betting
> surface (built and evidenced). The current state is at the BOTTOM of this file, newest last.
> **Nothing is in flight and nothing is owed by this seat.**


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

---

## SURFACES PHASE (the LAPTOP) — CLOSED, Design-verified 2026-08-19

Built to `docs/design/spec-market-surfaces-2026-08-17.md`. Closed on the worst-case-row frame.
Rulings applied: `S89`–`S92`, then `S95`–`S98` and `S102` as they landed.

**THE ONE THING A SUCCESSOR MUST NOT UNDO.** §4.3's leader dots are **not fully restored** — 59 rows
print fewer than six dots, all club-prefixed team totals — and **that residual was SEEN ON THE FRAME
THAT SHOWS IT AND ACCEPTED** (`S96-am2`, and the DD verified it at close). Full restoration needs a
544.68px name cell, i.e. a ~111px price cell and a ring 30.7% off native aspect: **the arithmetic is
available, the design is not.** It reads as a defect in a distribution table and was ruled not to be
one. **Do not re-fix it.**

**Geometry that is ruled and derived, not free to re-author:** ENTRY's market column is **700px**
(the betslip takes the other 324 of the 1024 — the spec's "~996px" forgot it); the price cell is
**160** and the name cell **496**, because `S96`'s uppercase overflowed 480 and the name cell was
widened out of the price cell's slack. That lever only worked because `WideBiroRing` is
`Image.Type.Simple` with `preserveAspect` off — **a fixed or 9-sliced ring would have made the slack
unusable.** `SureThingEntryTests` pins that ring rect at 176×48.

---

## SINCE THE CONSOLE PHASE CLOSED — additions, 2026-08-22

- **The two spec corrections are LANDED** (DD batch 172), so the section above headed *"for whoever
  amends it"* is discharged. Recorded because the numbers themselves still matter: §3's worst-case
  leader run is **16 dots, not 15** (a one-space vs two-space gap), and §14's `B4` folio numbers
  (`66–83 of 84`) are **unreachable at the shipped geometry** — `BodyRows` is 20, so a first page is
  always 20 rows.

- **THE DOCKED EVIDENCE REPRODUCES FROM A CLEAN BUILD — measured 2026-08-22, and worth keeping.**
  A hazard landed from the theater-engine lane: the `-p:SbrUnityPluginDir=<scratch>` habit leaves
  intermediates in `engine/obj`, and a later incremental Release build there emits a binary **no
  clean build reproduces** (see [[plugin-dll-needs-clean-release-build]] in memory). Every transcript
  in this lane was shot from Release builds sitting on exactly those intermediates. **Tested rather
  than assumed:** `engine/obj/Release` and `engine/bin/Release` wiped, clean Release rebuild, `B3`
  and `B8` re-shot from their documented commands — **both byte-identical to the docked files.**
  The hazard did not reach this evidence.

- **THIS LANE IS NOT ENGINE-OWNING AND SHOULD STAY THAT WAY.** The console links `MarketSheet.cs`
  and `MarketDestinations.cs` **by source** precisely so no engine change is needed and the tracked
  `SBR.Engine.dll` is never committed from here. If a future phase genuinely needs an engine change,
  the handoff §3 rule applies (rebuild AND commit the DLL) — but read the clean-build hazard first,
  because the two halves of that decision look identical in `git status` and want opposite responses.

- **Process note for whoever dispatches here.** Three sub-agents died on transient
  `ENOTFOUND` API errors this phase; all were recoverable by `SendMessage` resume, and in every case
  the tree was clean because the work lands in the repo rather than in the agent. **A resumed agent
  counts against the two-at-once cap** — I once had three running by forgetting that a resume is a
  spawn. Check `git status` after every dispatch regardless; one agent created a stray directory from
  a path-escaping bug and cleaned it, and the only reason I know it cleaned it is that I looked.

- **Still owed to the ORIGINAL charter, never started:** the match theater has no drawn ending —
  TV's lane, routed to the DD, three questions named in `docs/1-plans/F_0.5.0_*` §12.7.

> **From integration (2026-08-24): K21 binds the next seat, and it wants a GATE - not a capture.**
> Read batches 174-175 first. The console does NOT carry the scalar resolved-through TV found -
> `SweatRenderer.cs:389` reads `RevealedLegState(e.LegIndex)`, backed by a per-leg array; do not go
> looking for one. The defect is `SweatRenderer.cs:296`: `onFinalLeg = evt.LegIndex == lastLeg`
> compares the telling's ANCHOR leg against the highest leg index, so on a two-leg same-match
> ticket (anchor 0, lastLeg 1) it is false for every telling - the final telling is
> fast-forwarded instead of sweated, and the "(the final leg must be sweated)" refusal never
> prints. Assert it, do not shoot it (C60): the state sits behind a keypress and Hold
> short-circuits on redirected input, so a piped transcript comes back CLEAN. Mutation-test it as
> K17's gate was. B9 (colour) is a separate real-terminal item - pair only if such a sitting
> is scheduled for other reasons.
