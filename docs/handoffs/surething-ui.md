# surething-ui — lane handoff (same-game ticket: the screen half)

**Created:** 2026-08-14 · **Branch:** `surething-ui-2` (from main) · **Lead:** Claude (Opus 5)
**Charter:** `docs/sgp/step-5-presentation-plan.md` — the SCREEN phases, under Allen's Option C
ruling (2026-08-14): split by layer — sgp takes the model (`BetslipModel`), this lane takes the
screen. **Plan:** F_0.6.0 step 5.

## 1. Studio context (read in order)

- `docs/5-orchestration/STUDIO.md` — roles, ownership, merge protocol, autonomy policy.
- `docs/sgp/step-5-presentation-plan.md` — the plan this lane executes half of. Read it whole.
- `design/02-betting-math.md` § *Same-game tickets* — the model the surface presents.
- `docs/design/surething-design.md` — the owning doc for this surface. Canon binds; the register
  (`docs/design/REGISTER.md`) is history, the owning doc is what leads read.
- `docs/handoffs/sgp.md` — the sibling lane's state; its two standing follow-ups.

## 2. Scope

1. **The screen phases of step 5** as the plan states them: the same-match mark, the priced
   statement (the engine's joint price, never a product of leg odds — S73 forbids the surface to
   show the multiplied figure), the refusal as a stamped Blocked state carrying cause AND remedy
   (refusals take STAMP with cause-and-remedy — S73-am4), and the void arm's screen half when it
   reaches the surface.
2. **The inherited laptop margin-pin repair.** A pre-existing red on main: the stake-figure margin
   pin (1.96px, owned by M-04's 26px stake figure) reads 4.748 — the pin was never re-sourced when
   M-04 landed. The test's own prescription is the remedy: re-source the pin at the call site with
   the new split written out; NEVER shrink a figure to fit a pin. This is this lane's first,
   smallest unit — it re-greens main's suite.

## 3. Boundaries — merge-critical

- **`engine/**` is NOT yours.** Nothing in this lane touches the engine.
- **`BetslipModel.cs` is sgp's for this slice** (Allen's Option C). Do not edit it, including
  whitespace. Your phases consume what the model emits. Sequencing against sgp's P1–P3 goes
  through the orchestrator.
- The seam is canon: **the model emits parts, presentation composes words.**
- The screen is register-heavy: stamp ink versus toner, oxide, tracking, control sizes. Money
  language: money never abbreviates (C49); where copy and input disagree on a money control, the
  input is corrected to match the copy (C48); a fixed box carries an unstated face assumption —
  sweep the population (C46).
- Design questions route to the Design Director **through the orchestrator**. Capture evidence for
  every Design-facing change; claims about how something reads are made against frames (C11).

## 4. Rules inherited

- All seats: Opus 5 at max effort (standing spec).
- §7a settings churn discipline; explicit-path staging; suites green before merge requests.
- **Unity editor lease is serialized through the orchestrator. TV holds priority right now** —
  request a window, never assume one.
- **Inherited trap, live:** `dotnet` builds copy `SBR.Engine.dll` into the Unity tree and dirty a
  tracked LFS asset. This is a NON-engine lane: restore with checkout after every build; never
  commit it. (`unity/SBR/Assets/TutorialInfo/Icons/URP.png` also shows phantom-modified — never
  commit it either.)
- Report telegraphic, result-first: Done / Next / Risk / Need. The final text of a report is the
  deliverable — no bare register codes to Allen; plain words.

## 5. Lane state

### Unit 1 — margin-pin repair: GREEN (`3dd93df`), verified 2026-08-14

**The handoff's diagnosis in §2.2 is wrong and is superseded.** It says the pin "was never
re-sourced when M-04 landed." It was: `ead9396` moved it 2.6 → 4.56 in the same commit that landed
M-04, and that value is on main. Re-sourcing it again would not have held.

The pin's quantity is `4.00px structural + sin(0.5°) × the wax highlight's width`. The band is
sized from the payout figure's *measured* width, and `RunDirector.seed` is blank in `Room.unity`,
so every boot prices a different board and renders a different money string. The pin was a function
of how much money was on the screen; 4.563 ↔ a 56.5px figure, 4.748 ↔ a 77.7px one. Draws supplied
the extra glyphs. Nothing entered the flow — no commit touched its layout in between.

The earlier acquittal of the highlight computed the band's **height** term (0.21px). Rotation is
about the top-left **pivot**, so the term that matters scales with **width**.

Repair: structural part DERIVED from the layout literals and pinned two-sided at 4.00 ± 0.05 (needs
no re-sourcing ever again); tilt bounded at 3.0px rather than pinned. Reservation untouched, no
element excluded. Test file only — no production pixel moved.

**Verified** (Unity window granted after TV released, 2026-08-14). The derived 4.00 held on the
first run — it was never measured.

- Margin invariant green on **four independent boots** — the class run plus three single-test runs.
  Each boot rolls its own seed, so each measured a different board, a different payout figure and
  therefore a different band width. That repetition IS the evidence: the old pin passed and failed
  on the same code depending on the price.
- **PlayMode 94/94 executed · 88 passed · 0 failed · 6 skipped.** All six skips are the TV lane's
  capture harness, marked run-by-filter-only. Pre-existing.
- **EditMode 252/252 executed · 251 passed · 0 failed · 1 skipped.** The skip is TV's void fit grant
  pending re-certification. Pre-existing.
- Engine/dotnet suite deliberately NOT run: this change touches one Unity test file and no engine
  source, so it cannot move that suite, and building would dirty the tracked `SBR.Engine.dll` for
  nothing. Stated rather than silently omitted.
- Churn from the runs reverted by explicit path: `ProjectSettings.asset` (Unity dropped
  `SENTIS_ANALYTICS_ENABLED` from the Standalone defines on boot — integration-only file, never a
  slice's to move) and the TMP `LiberationSans SDF - Fallback` atlas. Tree carries only the phantom
  `URP.png`.

**Run trap, hit and worth keeping:** the first invocation died with no results file — Unity's package
resolve failed (`IPC stream failed to read`) because a stale `Temp/UnityLockfile` survived the
killed editor. No compile error, nothing to do with the change. Clearing the stale lockfile with no
Unity process alive fixed it on the retry.

### Unit 2 — survey done. The plan's open risk is CONFIRMED, and it is bigger than the model

The step-5 plan flags: *"The interaction model may reach past the slip. If the board also assumes
one selection per match, P1 grows. Not yet surveyed."* **It does.** Seven sites, four shapes, all
confined to `SportsbookApp.cs` — no other surface calls the matchup-keyed accessors.

1. **Marked-state is asked in the singular** — `SelectionOn(matchup.Index)` at `:243`, `:244`
   (lobby moneyline pair) and `:685` (detail offer rows). It asks *what is the pick on this match*.
   With two legs on one match, at most one can ever draw as marked.
2. **The interaction replaces** — `Toggle(matchup.Index, …)` at `:271`, `:275`, `:760`.
3. **RUB OUT addresses a matchup, not a leg** — `Remove(matchupIndex)` at `:968`. With two legs on
   one match it cannot remove one of them. (The plan predicted this for the model; the margin calls
   it too.)
4. **The rule is DRAWN, and becomes a lie** — `:687` computes a `replacement` state and `:745`/
   `:761-768` render it as a `⇄` glyph prefixed to the price plus a 2px underline, on every *other*
   offer in a match once one is picked. That affordance tells the player the second pick will
   replace the first. Once same-match tickets exist it is false, and it is copy, not plumbing.

So the screen half of P1 is real work and it is **blocked on sgp's model P1** — it cannot be
written until the model addresses legs rather than matchups. Sequencing through the orchestrator.

### Unit 2 — the void arm (P6) is the one screen phase that is NOT blocked

`TicketState.Voided` appears on **no surface at all** — every render site is a Won/Lost/CashedOut
chain with a fallback else. It needs nothing from `BetslipModel`, and sgp's void re-pricing already
landed, so this is startable now. It is also not merely missing; the fall-through prints two
falsehoods in the ledger (`:2475-2489`):

- the state word falls to **`"OPEN"`** — a settled, refunded ticket reads as still live;
- the returned value falls to **`"—"`**, which S41 reserves for an amount that is *genuinely
  unknowable*. A voided ticket's return is exactly known: the stake. So the dash is false here and
  it spends a ruled token on the wrong case.

A voided ticket does reach the ledger — `:2197`/`:2201` collect on `State != Open` — so this is
rendered today, not merely unreachable.

**DONE and green (`658b685`).** All three fixed in the settled ledger. The word is `VOID` — not
invented, since C47 rules that a market returning the stake *is* a VOID and `LegStateWord` already
prints it for a voided leg. The value is `ticket.Stake`, never `PotentialPayout` (zero for a ticket
voided in full — it would print `$0` for a ticket that cost the player nothing). The ink needed no
ruling: S65 already holds a VOID leg at toner-2, it is not wax because a refund is not a winning,
and it does not dim to toner-3 because a returned stake is a fact, not an absence.

S41's em dash is **kept, not spent** — it still prints for the one case it was ruled for, a
cash-out with no retained figure. What was removed is a case that was never an absence.

Word factored to `OldSlipsApp.LedgerTicketStateWord` per S43, and the new test drives it over
`Enum.GetValues` rather than spot-checking VOID — the defect was a fallthrough that would swallow
*any* state added after the branch was written. PlayMode 95/95 · 89 passed · 0 failed.

**Stated, not claimed:** the RETURNED cell and total are not asserted. A `Voided` ticket cannot be
built from the test assembly (`Ticket.State` is `internal set`) and the only path to it is a
same-match ticket whose survivors re-price at or below evens. Recorded as a T53 blind spot; covered
when that is reachable.

**Out of this lane, named:** the TV's `RevealedTicketState` mirror has no `Voided` member
(`TvSweatScreen.cs` is tv-sweat's file); `game-console` is a dead prototype (T44);
`sim/RunPlayer.ScoreSwings` is not a screen.

### main merged — P1's screen half is now unblocked

Merged `main` at `abe6501` (carries sgp's model half `ee4fa03`, and my own unit 1 which main had
already taken at `957b8d6`). No conflicts. Both suites re-verified green on the merged tree, against
sgp's rebuilt `SBR.Engine.dll` — which this lane took via the merge and did **not** rebuild, per the
non-engine-lane rule.

The model now offers `AddLeg`, `RemoveLeg(legIndex)`, `RemoveSelection`, `LegIndicesOn(matchupIndex)`,
`LegCountOn`, `Contains`, `IsSameMatch`, and a structured `Refusal`. `CombinedOdds` is now just
`TicketOdds` off the engine, so the product-of-legs figure S73 forbids is gone from the model.

**Three screen rules binding on the P1 work (from sgp's testing, via the orchestrator):**

1. **Spend the WHOLE remedy set.** Remedies run up to three legs at the shipped `κ`; the stamp copy
   is plural in the first cut, never singular.
2. **Remove high index to low.** Removing low-first reindexes the legs above it mid-loop.
3. **`SideOn`/`SelectionOn` answer only the FIRST leg on a matchup.** Every same-match group must go
   through the leg-addressed accessors — this is exactly what makes the seven surveyed sites wrong,
   not merely incomplete.

### DD batches 66–67 (canon `c467df3`) — two of three landed (`5e7af3a`)

**S51 CLOSED — the band moved.** All three seating options I routed were refused; the DD ruled it a
kit-fidelity gap. `PayoutFigure.jsx` sets the band `bottom:-2px` against a 31px × 1.1 = 34.1px line
box, so the kit's band bottom is 36.1px below the figure's top and the build had 40px. **The band
moves, the block does not.** Written from the kit's tokens, not as a literal 30.1.

Pin **re-sourced once** per the ruling and still derived: **0.10px** = the kit's 36.10 against a
build box of 36.00. The tenth is written out, not rounded to zero, so it reads as the box-height
difference rather than drift. Held first run and across four boots.

**S75 replaced my tilt bound.** A transformed mark reserves its TRANSFORMED extent, so the tilt is
held to T47's 6px separation rather than a 3.0px number I picked. This is what earned the fix:
before, `4.00 + 0.0087·w` crossed 6px at `w > 229px`, reachable because money never abbreviates and
same-game lengthens the figure. After, it needs a 677px band in a 324px panel.

**S76 — VOID's binding negatives now have gates.** The row already matched the approved vocabulary;
what was missing is that the negatives were true only by construction. `LedgerTicketStateInk` and
`LedgerShowsDeadStrike` are factored (S65's reason) and asserted over the enum: never the oxide
strike, never DEAD's toner-3, never `Dim`'s .55, never wax.

**S41 — corrected toward the DD.** I had written it was "kept, not spent." For this row it is
**spent**: the dash is a binding negative. It still prints for the cash-out with a genuinely unknown
retained figure — that case was not before the DD.

**Owed, named, not built:**
- S75's design-time clearance constant: sweep the population (C46), take the widest renderable money
  string, pin the clearance as a CONSTANT. The gate still reads the band width at runtime, so it
  proves the boundary for *this boot's* string only. Recorded in the test's blind-spot list.
- The VOID row's third element, the entry **rubbed out** — the DD marks that treatment a candidate
  pending frames.

### P3 — BUILT (`2bbd722`). Copy is to canon; the FIT IS NOT, and the numbers are below

The surface was printing the model's machine token verbatim: a refused same-match slip stamped
`REFUSED:IMPOSSIBLECOMBINATION` on the PLACE control. `PlaceBlocker` returns that token *so that*
printing it is loud. Closed.

Composition is `SportsbookApp.RefusalStamp` — two authored cause forms by arity, a separate cause
for duplicates and for sub-evens, a conjunctive remedy spending the whole set, no banned connective,
removal order withheld. Legs are named by `MarginLegSubject`, **factored out of the margin leg row**
so the stamp and the row cannot drift apart.

#### The DD's numbers — measured on the real board, three independent boots

| | |
|---|---|
| control | **288.0 × 17px**, one line, no wrap |
| typical 2-leg refusal | **412–469px — 143–163%** of the control |
| worst renderable stamp | **1583–1722px — 5.5–6.0×, SIX LINES** at this width |

**The common case already overflows by half.** The worst case is a three-leg cause plus a three-leg
remedy, which occur at the shipped `κ`.

Nothing is truncated and fit is **not** asserted — sizing is the DD's call ("size the control for it
or author a shorter form"), and a truncated remedy is an unverified remedy, so the gate must not
quietly become a truncation test. It asserts only what is mine: the token never reaches the control,
no banned connective, the whole remedy set named, ≥13px, nothing ellipsised.

**Sizing is not free, and this is the part worth the DD's attention:** six lines at 13px is ~102px
against the PLACE band's current 44px. `PlaceBandH` feeds `ActionBandReservedHeight`, which feeds
`MarginFlowBudget` — growing the control shrinks the flow budget the margin invariant measures. A
control sized for the worst stamp would take ~58px out of a 370px budget that currently clears by
0.10px.

#### Second finding — a disjunction *inside a leg name*

The widest leg name on every board measured is of the form **`San Francisco Regulators OR DRAW`**
(the draws double-chance vocabulary). A remedy naming it reads:

> DROP TURNIPS AND TUSCALOOSA LONGHAULERS OR DRAW TO PLACE

which satisfies the letter of S73-am5's ban — the connective is `AND` — and defeats the reason for
it, because the reader cannot see where the leg name ends. **Reported, not ruled:** renaming a
market is copy.

It also had to be handled inside the gate. The banned-connective check runs against the stamp with
leg names **masked out**; checking the raw string would go red on whichever boot put a double chance
in a remedy, and would read as a copy violation rather than the flake it is.

#### P3 is HELD (Allen, 2026-08-14) — `SportsbookApp.StampComposedRefusal`, default OFF

Correct copy that overflows should not ship while sizing is with the DD. **The hold costs nothing
reachable:** a refusal only fires on a matchup with 2+ legs, and `Toggle` still REPLACES — the
additive gesture is a design decision nobody has made — so no player can build a same-match slip
through this surface. The held path is reachable only via the model's `AddLeg`, i.e. from tests.

**The P3 gate turns it ON deliberately** so the composition stays exercised rather than rotting into
dead code behind a false constant, and **asserts the default is OFF** so releasing it is a decision
rather than a drift. Cleared in a fixture `TearDown`, not at the end of the test that sets it — a
mid-body failure would otherwise release it for everything after.

### P4 — THE HOUSE'S LINE. BUILT and green (`7d06881`)

Where two picks are priced as related, the house marks the connection **in its own ink** (§3.1, S73)
— he picks in biro, the house marks in Stamp — drawn in the margin's left gutter, between the sheet
divider and the check column.

**DRAWN, NOT CAPTIONED**, and that negative is the gate's load-bearing half: it sweeps every text
node in the margin and fails if `HOUSE'S LINE` or `SGP` appears anywhere. §3.1: the name is what the
thing is *called*, never a tag on every occurrence.

**Spine plus a spur per member row.** The spurs are not ornament — slip order is insertion order, so
a connected pair can straddle a leg on a different match, and a bare spanning stroke would mark a row
it has nothing to do with.

Also gated: **one leg on each of two matchups draws NO mark.** A mark where there is no connection
teaches the opposite of the rule it exists to teach ("unmarked legs multiply; marked legs pay less").

**Geometry is a CANDIDATE, not canon.** §3.1 rules the ink, the connection and the absence of a
caption — implemented. Stroke weight and spur length want frames, as the VOID rub-out does.

The margin invariant still holds with the marks in the flow: they sit in the leg-row band, well above
the payout, so the deepest element is unchanged.

### P4 is COMPLETE — the mark (`7d06881`) and the instrument name (`16047df`)

`SAME MATCH` now names the instrument on the slip's price row. **That placement is forced, not
aesthetic:** canon rules a same-match ticket "its own instrument — never a parlay with an
adjustment", and `COMBINED` names the price as a combination arrived at by *multiplying*. On a
same-match slip that label was not silent about the instrument — it was **wrong** about it. The
figure beside it is already the engine's joint price, so the label was the last thing on that row
still describing a parlay.

Gated both ways: same-match reads `SAME MATCH`, untracked; an ordinary parlay keeps `COMBINED`,
because legs on different matches genuinely do multiply.

### P3's flag — RELEASED, and merged. Flagging a possible stale instruction

The orchestrator's 2026-08-15 relay says "the wiring stays flagged". It is **not** flagged: the
previous relay said *"Unflag the wiring once the stamp is rebuilt to that shape"*, S77's rebuild
did exactly that, and it is merged to main (`26d7baf`). `StampComposedRefusal` is a `const true`.
Reading that line as stale rather than as an instruction to undo merged, DD-sanctioned work —
**say so if it was meant literally and it goes back behind a hold in one commit.**

### P5 — the statement. DRAFTED, with the DD (`docs/design/dd-question-p5-relation-statements.md`)

Seven sentences across the four relations the model actually nominates. Not four: every relation
but `Implies` is emitted in **both signs**, and reinforcing/opposing are opposite claims about the
same shared thing, so one sentence per relation would state one falsely about the other.

Measured coverage that decides them, off 6,109 placeable same-match slips: `Implies`/Reinforcing
10.6%, `SharedScoreline` both signs 14.4%, `ScorerOfSide` both signs both sides 27.1%,
`SharedCount` Corner+Card both signs 1.8% — and **46.1% with no statable relation at all**, which is
canon working and is asked to be ruled correct before it reads as a gap.

Batch 70 (`26d7baf`) is TV-only and does not touch these; the word is still owed.

### P5 — what it needs once the copy is ruled

`SameMatchPricing.principal` is the nominated relation; **do not pick from the list** — choosing which
relation moved the price is a pricing claim only the model can make. The kinds that can reach a
placed slip are `Implies`, `SharedScoreline(sign)`, `SharedCount(family, sign)` and
`ScorerOfSide(side)` — `Independent` is never principal and `MutuallyExclusive` is a refusal.

So P5 needs **four authored sentences**, one per kind, in toner, once per slip, stating what the legs
*share* — never a formula, never a coefficient, never an English string from the engine. Canon gives
the constraints but not the sentences. **Lengthening is not remarked**, and the implication case is
*stated, not blocked*.

### Ruling (3) — remedy copy. The canon P3 was built to

Recorded here so it is not re-derived. From S73-am5 (canon `c467df3`):

- **The remedy is CONJUNCTIVE and authored PLURAL in both halves.** Removing only the first element
  leaves the slip refused, so it is a **set to remove, not a menu to choose from**. Remedies of up
  to **three legs** occur at the shipped `κ = 1` across 645 refusals.
- **`or` / `either` / `one of` / `any of` are BANNED in a remedy.** English's natural form for a
  list of fixes is disjunctive and the model's truth is not.
- **A remedy that names a fix which does not fix it is worse than no remedy** — S73-am4 requires a
  *verified* remedy.
- **The cause breaks too.** `… cannot both land` is two-valued; three or more legs take an authored
  `… cannot all land`. **Two authored forms chosen by arity, never one template with a substituted
  word.**
- **A duplicate and an impossibility take ONE treatment and TWO causes** — §3.3 wants a *literal*
  reason, and one vague sentence covering both is what that word exists to prevent.
- **Legs are named by the exact string on their own row**, so he never translates against the rows
  in front of him.
- **Fit is measured, not estimated** (C46): the population is the 645 refusals and the longest
  renderable remedy is computable today. **A truncated remedy is an unverified remedy** — size the
  control for it or author a shorter form.
- **Removal order never reaches the player.** High-to-low is an implementation constraint only.
