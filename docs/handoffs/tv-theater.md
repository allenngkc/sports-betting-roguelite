# tv-theater — lane handoff

**Created:** 2026-08-16 · **Worktree:** `tv-theater` (from main at HEAD) ·
**Lead:** Claude (Opus 5, max effort)

---

## 0-U1. UNIT 1 — THE RESOLVED-LEG COLUMN · window open 2026-08-16

**MEASURED THIS WINDOW, on this tree, after the main merge:**

| suite | measured | against baseline |
|---|---|---|
| engine | **306 / 306, 0 failed** | 306/306 — unchanged |
| EditMode | **260 executed / 259 passed / 0 failed / 1 skipped** | 255/254/1 — **+5, all this unit's new pins** |
| PlayMode | **133 executed / 115 passed / 0 failed / 18 skipped** | 126/112/14 |

**All three green. Full suites, no `-testFilter` on any gate** — the only filtered run
this window was the `[Explicit]` seed search, which is an instrument and gates nothing.

**Every one of the 18 PlayMode skips is `[Explicit]` by design** — enumerated from the
results XML rather than assumed: eleven capture entry points, four `Evidence_*`/`Probe_*`
pins, and this unit's own seed search. **No red is hiding in the skip count.**

**THE GATE FIRED ON REAL OBSERVATION, and this is the C29 evidence:**

```
[TRAP-GATE] seed=STATS-MULTI-5 frames=59 state1Cases=49 state2Cases=2
```

Both counts non-zero, so both `Assert.Greater(…, 0)` gates passed on states actually
reached rather than on a technicality. **`STAKE` was observed on the surface, on a
multi-leg ticket, with leg 2 still live — the state the whole spec was written for.**

> **RECORDED AS A LIVE RISK: `state2Cases=2` is TWO FRAMES.** The window where leg 2 is
> won-on-the-count but not yet whistled is genuinely narrow on this seed. The gate is
> real and non-vacuous, but it sits close to its own floor — **if beat pacing shifts,
> this gate goes red because the STATE stopped being reachable, not because the build
> broke.** A future seat meeting that red should re-run the seed search and re-pin
> before touching anything in `BuildTicketLegOutcomes`.

The engine line is today's number, not an inherited one: re-run here because
`tv-sweat` §4's own rule is that a baseline's honest failure is not being wrong but
not saying how old it is.

**Merged `main` twice this window** (both fast-forwards, docs-only, no conflicts):
batch 100 (`43b888b`) made the resolved-leg column canon, and batch 101 (`6ccd871`)
carries the ruling below.

> **THE STUDIO WAS PAUSED MID-UNIT AND THIS SEAT STOOD DOWN.** Allen's pause reached
> the repo while the lane was building; a resume tap that raced it was countermanded
> at cycle 372. The lane held idle — no suites, no commits, no dispatches — until
> Allen's own resume. Recorded because the tree was NOT "untouched" as the pause
> census recorded it: unit 1's build was already sitting in it, unverified.

### THE RULING — finding 1 went to the DD and came back CONFIRMED

`docs/design/ruling-t108-trigger-2026-08-17.md` (batch 101, canon at `6ccd871`)
answers this lane's routed question. **Build proceeds on the revealed-count reading**
— not as a deviation from the spec but as *clause 3 applied correctly to a field the
spec should not have named*. The DD verified the single-call-site claim at source
rather than taking it on report, and recorded its own §1.5: **naming a field is not
reading it** — the enum exists but does not carry the state at the moment the defect
occurs, and a state field's transition points are the whole of its meaning to a fix
keyed to a moment.

**Five things the ruling RATIFIES as built, so a later hand does not tidy them:** the
separate `RevealedLegOutcome` enum (must stay separate — the two answer different
questions); `LIMIT 0` staying; `TicketCannotLose`'s whole-ticket signature (clause 2's
trap closed structurally, *stronger than the spec asked*); `BuildTicketLegOutcomes`'
three-way composition; and the dead ticket deliberately not built.

**A THIRD STATE was ruled that `G1` never contemplated** — *decided, but not yet
resolved*. On a leg won by the revealed count before its whistle the statement line
**does not change**: `OVER 8.5 CORNERS` stays, because it reads as the market that was
bet and the line directly beneath it answers any reading of it as an outstanding ask.

**One correction binds, and it is in §6 below.**

### THE FINDINGS AS ROUTED (finding 1 now ruled above)

**1. `RevealedLegState` CANNOT BE THE TRIGGER, and a literal build of clause 1 is a
no-op that ships green.**

`T108` and the spec's §2 both say *"the surface has the information and is not
reading it"*, naming `RevealedLegState` / `RevealedTicketState`. Measured, that is
right about the revealed COUNTS and wrong about the enums:

- `RevealedView.ResolveLeg` has **exactly one call site** — `FinalSlam`, at full
  time. So on all three defect frames (48' / 66' / 71') the enum reads `Live`.
- `FinalSlam` advances `_resolvedThrough` in the same method, and
  `UpdateTicketColumn` blanks `Need`/`Progress` for every row below it. **So by the
  time the enum says `Won`, the row has already left the live form** — clause 1's
  `Won` and `Lost` rows are unreachable, and `{n} CORNERS • WON` would never render.

**The trigger is the revealed COUNT** (`_countLedger.Home/Away`), which is what
constructs `NEED 0` in the first place and is already in the describer's hand.
`k = threshold − total ≤ 0` *is* "the revealed count has cleared the line". That
satisfies **clause 3 more exactly than the enum does**: the enum follows the
*resolved* match arriving on a reveal frame; the count is the *revealed* state, and
it can never run ahead of the screen because it is the screen's own published value.

**2. `ResolveBeat` never updates the revealed mirror.** Only `FinalSlam` does, so on
a multi-leg ticket an intermediate leg's `RevealedLegState` never leaves `Live`.
**The laptop's MY BETS reads that mirror and the laptop is not this lane's surface —
ROUTED, not fixed.** It is also why the footer's leg-outcome list is built from the
same fields the rows themselves render from, behind the same `_resolvedThrough`
guard: the footer can then never contradict the chips the player is looking at.

**3. `T62`'s defect, on the count ledger instead of the score ledger.**
`RepaintRevealedScore` exists so one ledger advance repaints every mirror in the
same call, and `OnGoalPlayed` uses it. **`OnCountPlayed` does not** — it repaints the
scorebug and leaves the ticket column until the next beat's `RenderEvent`.

This refines `T62-am`. The DD checked the 66' frame, found the count tracks by 71',
and closed it — **the frames were read correctly and the conclusion holds**; the
mechanism is nonetheless T62's, and the count tracks *one beat late*. Fixed inside
this unit rather than routed, because the new `WON` string is a progress line and
§6.2 requires a progress line to land on the same frame as the revealed payload —
shipping it a beat late would breach canon on the very change being made.

### SCOPE — one deliberate extension and two deliberate omissions

- **EXTENDED:** the form-selection is applied to **every arm that constructs a
  remaining-count**, not only corners. `{n} GOALS • 0 MORE` is the identical lie from
  the identical clamp. Rule as built: *the outcome is derived wherever the revealed
  values decide the leg; the STRING changes only where the old string named a
  requirement or an allowance that no longer exists.* BTTS and scorer therefore take
  an outcome and keep their copy. **DD to rule** — clause 4 forbids tidying the
  column, not applying the ruled form to a sibling market.
- **NOT BUILT — the dead ticket** (spec §5). No losing ticket in the capture; the
  principle is ruled and the strings are owed on a frame. A ticket with a `Lost` leg
  keeps today's `RISK`, pinned as a deliberate omission.
- **`LIMIT 0` IS TRUE AND STAYS.** An under leg at zero slack is still live. It looks
  like `NEED 0` and is not, and it is pinned so a later seat does not "fix" it.

### THE GATE CORRECTION — the ruling's §5, and why the first gate was not enough

The every-frame poll is **the right instrument** and the DD said so: a moment where
two surfaces disagree cannot be caught by a sampled pin, and it reads the
player-visible text rather than re-deriving it. **But it could pass without ever
exercising clause 2** — the ticket comes from an unpinned `DemoTicketPolicy` draw, so
where the run never reached a decided leg the STAKE half logged and did not fire.

> **A gate whose central assertion is conditional on the draw certifies nothing about
> that assertion** — and the composition it guards, `BuildTicketLegOutcomes`'
> three-way split, is the one part of this fix **no signature protects.**

**RULED: two states, exercised BY CONSTRUCTION, not by luck —**

1. leg 1 resolved `Won` + leg 2 live and undecided → footer reads **`RISK`**
2. leg 1 resolved `Won` + leg 2 live and won ON THE REVEALED COUNT before its whistle
   → footer reads **`STAKE`**

`sawDecidedLeg` / `sawNextChip` become end-of-run assertions on a fixture built to
guarantee them; the every-frame poll is unchanged.

**THE CONSTRUCTION IS THE LANE'S CALL and this is it: measure, then pin** — the same
route that chose `STATS-MULTI-1`, and the only one available, because no hook exists
to drive the ledger and adding one to production to satisfy a gate is out of scope.

**RUN 2026-08-17, twelve candidates, and ONE carries both states:**

| seed | leg 0 won | state 1 `RISK` | state 2 `STAKE` |
|---|---|---|---|
| **`STATS-MULTI-5`** | **yes** | **yes** | **yes** ← pinned |
| `STATS-MULTI-1` · `-3` · `TRAP-2` · `TRAP-5` | yes | yes | **no** |
| `STATS-MULTI-2` | yes | **no** | yes |
| `48151623` · `-4` · `-6` · `TRAP-1` · `-3` · `-4` | **no** | no | no |

**One seed in twelve carries both, and that is the ruling's own argument made
arithmetic** — a gate left on an unpinned draw would have certified state 2 about one
run in twelve. `STATS-MULTI-1`, the seed the lane already trusted for multi-count work,
is one of the four that never reaches it.

**The OVER-only constraint was load-bearing:** an under leg has no early `Won` — its
only pre-whistle verdict is `Lost` — so an under fixture could not certify state 2 on
any seed. Without that the search would have reported all-false and read as "the state
is unreachable."

> **A PIN WAS DELETED AT DIFF REVIEW, and it is the reusable half of this window.**
> The dispatch also produced a *broader* pin — *any decided leg forces `RISK` while any
> other leg is undecided* — written against the pre-ruling brief and kept because it
> read as a safe superset. **It is false.** State 2 is exactly leg 0 decided, leg 1
> undecided-by-chip, footer correctly reading `STAKE`; that pin would have failed on the
> one state the whole fix exists to produce, and it would have failed *on the pinned
> seed*, so the suite would have gone red with the build correct.
>
> **A "broader" assertion over a state space you have not enumerated is not a stronger
> claim, it is an unenumerated one.** The agent met both briefs and still shipped the
> contradiction, because the ruling arrived mid-flight and superseded the assumption the
> first brief was written on — which is the standing reason this lane reviews the diff
> and not the summary.

### EVIDENCE OWED before Design-verified (spec §8, unchanged)

1. A won leg with match time remaining — the before-state is already in the set.
2. A multi-leg ticket, one leg won and one live.
3. A losing ticket, for §5.

**Frame claims stay frame claims:** whether `WON` and `STAKE` read at review distance
is C11 and neither gate states anything about it.

---

## 0-U2. UNIT 2 — THE CONSOLIDATED `C46` SWEEP · scoped, not yet built

`T111` binds it: **three families, ONE sweep**, under `S84` (size against the
ENUMERATED POOL's widest, never the sweep's widest measured) and batch 95 (the widest
string is a MEASUREMENT, never read off string lengths or type sizes).

Scoped against `Assets/SBR/Editor/TvExtentSweep.cs` this window. **Four concrete
findings, and three of them are the S84 failure mode already sitting in the
instrument:**

1. **`RiskPays` gains a WIDER string, and the spec did not name this.** §7 says the
   change *relieves* the box — true of the progress line, **false of the footer**.
   `RISK` → `STAKE` is 4 chars → 5, and the slot's pool is
   `{"RISK $13,639", "RISK $1,234", "RISK $50"}` with no `STAKE` form in it. The
   footer is one row with **both ends anchored**, so a wider left half eats the
   clearance to a right-anchored `PAYS` whose own maximum is eleven digits.
2. **`LegRowProgress0`'s pool is fabricated and always was.** It holds
   `"0-0, 62' PLAYED"`, `"NEEDS 1 MORE, 78'"`, `"2-1, 88' PLAYED"` — **none of which
   this model can emit.** The real forms are `LEADING 2–1` / `LEVEL` / `NOT LEVEL` /
   `SCORED` / `NOT YET` / `{n} GOALS • {k} MORE` / `{n} CORNERS • NEED {k}` /
   `• LIMIT {k}`, plus this unit's new `• WON` / `• LOST`. The column family's sweep
   has been **vacuous**, not merely incomplete.
3. **`Flavor`'s pool is three invented ALL-CAPS strings**
   (`"REGULATORS BREAK AWAY DOWN THE RIGHT"`…) while the strings that actually clip
   are lower-case authored lines from `SweatFlavor.cs` **plus a generated suffix** —
   `TvSweatScreen.cs:1695` appends `" ({n} in the spell)"`. That composition is the
   whole of `T110`, and no pool member contains it. The real pool is **ten authored
   arrays** in `SweatFlavor.cs`; the suffix reaches only the four count arrays.

   > **THE CHARACTER COUNTS BELOW ARE NOT THE MEASUREMENT, and are recorded only to
   > show the pool is wrong.** Batch 95's binding is that the widest string in a column
   > is a **measurement**, never something readable off string lengths or type sizes —
   > it cost the DD two wrong predictions in a week. So: the deck's longest authored
   > line is 54 characters and the suffix adds up to ~18 more, against a pool whose
   > longest member is 36. **That says the pool never contained the real strings. It
   > does NOT say by how many px the box overruns** — the sweep says that, and nothing
   > here anticipates its number.
4. **The stats panel has no slot in `TvExtentSweep` — but it DOES have an
   instrument**, and this seat's first reading of it was wrong. `T101`'s residual is
   served by a dedicated `[Explicit]` PlayMode pin,
   `Evidence_C46_the_stats_panel_strings_against_their_boxes`, and it is already built
   to the standard: population enumerated from source (the closed club pool through
   `SweatFlavor.Short`, the title and row labels read off their assign sites), face
   borrowed from the RENDERED components rather than a lookalike, and **it offers no
   fit verdict** — *C46 is a measurement lane, not a judgement.*

   **So the residual is not "no instrument", it is "never run and never docked."**
   Corrected here because the two call for completely different work, and the second
   is much cheaper. **Absence from `TvExtentSweep` is not absence of coverage** — the
   sweep is one instrument on this surface, not the only one.

### THE MEASUREMENTS — run 2026-08-17, both instruments, one window

**Measurements only. No fit verdict is offered and none is implied: `C46` is a
measurement lane and the DD rules.** Every number is TMP's own unconstrained
preferred width on the real component, the instruments' shared call.

**Family 1 — the ticket column (`T108`'s new strings)**

| slot | box | widest measured | result |
|---|---|---|---|
| `LegRowProgress0` | 249.0px | `CLEAN-SHEET PATH LIVE` 191.4px | fits, **57.6px spare** |
| `RiskPays` | 249.0px | `STAKE $13,639` 158.9px | fits, **90.1px spare** |

**The footer-widening risk this seat raised is measured and it is clear.** `RISK` →
`STAKE` was flagged as pressing a box the spec said it relieved; on the instrument it
costs 90.1px of headroom that was already there. **The flag was right to raise and the
measurement is what settles it** — neither the spec's "relieves" nor this seat's
"presses" was a measurement.

**And the new strings are not the widest in their own slot.** `LegRowProgress0`'s
widest is `CLEAN-SHEET PATH LIVE`, a BTTS line that predates this unit entirely.

**Family 2 — the flavour strip (`T110`) — THE ONE THAT OVERRUNS**

| slot | box | widest measured | result |
|---|---|---|---|
| `Flavor` | 651.0px | `yellow card in the spell — the picked number improves. (12 in the spell)` 745.8px | **OVERRUNS by 94.8px** |

**`T110` is confirmed and quantified.** The clip reported on frame was a corner line;
the widest reachable form is a **booking** line carrying the same suffix. The old pool
could not have found either — its widest member was an invented 36-character string.

**Family 3 — the stats panel (`T101`'s residual) — 142 strings, no overrun**

| slot | box | widest measured | spare |
|---|---|---|---|
| `StatsLabel1` | 111.0px | `CORNERS` 81.2px | 29.8px |
| `StatsTeamA` / `StatsTeamB` | 145.0px | `Spreadsheets` 115.3px | 29.7px |
| `StatsTitle` | 111.0px | `COUNTS` 88.5px | 22.5px |
| value cells `StatsA*`/`StatsB*` | 145.0px | `10` 22.0px | ≥123px |

### TWO FINDINGS THE SWEEP PRODUCED THAT NOBODY ASKED IT FOR

**1. `CashOut` OVERRUNS by 26.7px — and it is outside all three families.**
Box 241.0px, widest `MARKET SUSPENDED` at 267.7px. §6.1's money control, one of its
six ruled states, and `TV-12/13` gives that string the slot **exclusively**. Not this
lane's to fix and not in the consolidated sweep's scope — **routed, and named here so
it is not lost.**

**2. The sweep's own §4.2 invariant is currently FALSE.** It prints
`UNACCOUNTED FOR — this number must be 0` and reports **12**: every stats-panel slot
(`StatsA0-2`, `StatsB0-2`, `StatsLabel0-2`, `StatsTeamA/B`, `StatsTitle`). Those slots
ARE covered — by the panel pin above — but `TvExtentSweep`'s only category for a slot
it does not sweep is *declared unswept (renders no string)*, **which is false for all
twelve.** So the instrument cannot express "covered by a different instrument" and its
own must-be-zero line is lying rather than gapping. **A third category is owed; that is
an instrument change and the DD's call, so it is reported rather than made.**

### SCOPE OF THIS RUN — stated so absence is not read as coverage

- **Production runtime did not change.** The only edited file is
  `Assets/SBR/Editor/TvExtentSweep.cs`, an editor-only measurement tool that no test
  references as code. **The full-suite trigger did not fire and no suite was re-run
  this window** — said plainly rather than implied by silence.
- **Two pool members are deliberately unreachable today:** the whole-number-line bare
  forms (`{n} GOALS` etc.). The generator emits only half-integer lines, so the branch
  is defensive-only. Kept, and named, because a config change makes them real.
- **`C46` is a measurement lane. Nothing above is a verdict**, including the two
  overruns — what to do about them is the DD's.

---

## 1. Context (read in order)

- `docs/5-orchestration/STUDIO.md` — roles, ownership, merge protocol,
  delegation contract (bundle small items into one bounded Sonnet dispatch;
  audited mechanically).
- `docs/handoffs/tv-sweat.md` — the predecessor lane's contract. Its §4
  baselines and its two standing dispatch rules BIND you verbatim: never end
  a turn against a running Unity process; full suite, no `-testFilter`.
  Captures anchor to `dataPath` per the pinned rule; frame sets stay
  UNTRACKED (READMEs and canon text commit; frames never do).
- `docs/design/tv-design.md` — the owning doc. Canon binds.
- `docs/design/spec-resolved-leg-column-2026-08-16.md` — your first unit's
  approved spec.
- `docs/design/count-sweat-read-2026-08-16.md` — the evidence read behind it.

## 2. Scope — two units, in order

1. **The resolved-leg column spec** (approved by Allen): a settled leg's
   column stops naming risk and need — no word may name a jeopardy or payout
   that no longer exists. The spec names what moves and what is deliberately
   left alone (the hand-over direction is Allen's, not yours; the
   dead-ticket copy is ruled in principle but its strings come back on a
   frame). Build, suites green, shoot the resolved state, dock.
2. **The consolidated C46 string sweep** — three families in one sweep: the
   stats panel's strings (the open T101 residual), the flavour strip's
   (clipping mid-word on frame), and the column's. Widths against boxes,
   report as measurements; the DD rules.

## 3. Boundaries

- The engine is not yours. The laptop surface is not yours.
- Design questions route to the Design Director through the orchestrator;
  claims about how something reads are made against frames.
- Unity editor lease is serialized through the orchestrator — currently
  free; request before assuming on later windows.
- Known traps: `SBR.Engine.dll` checkout-restore after builds (never commit
  it); `URP.png` phantom-modified (never commit it); stage by explicit path.
- Report telegraphic, result-first: Done / Next / Risk / Need. Plain words
  to Allen; register codes stay in the docs.
