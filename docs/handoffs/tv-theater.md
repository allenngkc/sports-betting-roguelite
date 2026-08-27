# tv-theater — lane handoff

**Created:** 2026-08-16 · **Worktree:** `tv-theater` (from main at HEAD) ·
**Lead:** Claude (Opus 5, max effort)

---

## 0-ROT6. LANE STATE 2026-08-28 — READ THIS FIRST

**Written on the orchestrator's order, not at a handover — the seat has not rotated.** State is
`03150e7` (lane work through `5c96435`), merged, CI green. **EditMode 344/343/0/1 · PlayMode
155/127/0/28.** Editor closed, procs 0. Tree clean but `URP.png` (irreducible — see below) and the
untracked `artifacts/`.

### ⚠ THE CHAIN IS EMPTY — `T94` IS CLOSED AND NOTHING IS BUILDABLE

**`T94-cl` (DD batch 207): every condition met.** The code closed at `83bd2f1`, the frames docked at
main-2 `a433e48` — both moments holding against batch 197's pre-commitment as amended by
`T140-am5` — and the ruling landed on them. **Nothing is owed from this lane on it.**

**TWO ITEMS DISCHARGED AS SIDE EFFECTS, neither aimed at**, and both are worth reading before the
next seat plans anything against them: **`T158` fell out of the seam itself**, and **`T169-am2` fell
to the multi-leg fixture** built to close this lane's own flag. That is the good direction of the
pattern this stretch kept producing in reverse — a fix reaching further than intended, for once
usefully rather than as a double-swap or a collapse in four places.

### WHAT LANDED SINCE `0-ROT5`

`83bd2f1` the batch-arity fix · `d08672a` the strip at arity>1 + `C62`'s comment · `b19f5c9` the
NEITHER sweep + the handicap double-swap + the sign gate · `ab6a882` the `TeamName` guard ·
`5c96435` the multi-leg counter fixture + `TeamColor`'s deletion. Frames docked in main-2 at
`a433e48`. Rulings folded: batches **196–207**.

### ⚠ OWED ON THE TV SURFACE — the whole list, in one place

1. **`T152-am3`'s arity>1 STRIP FRAME** — the strip is BUILT, not verified. It needs a sweat that
   naturally throws a **stoppage batch of two or more goals**, and batch 203 forbids forcing or
   seed-hunting it. It did NOT occur in the D2 shoot. **The watcher is already in the capture
   harness** (`WaitWatchingForMultiGoalStrip`, matching the rendered `^\d+ GOALS$`), so any future
   capture run picks it up for free — **do not build a new window for it; wait for one.**
2. ~~**`RevealedLeg.TeamColor`**~~ — **DONE, `5c96435`.** Deleted: field, write, and its now-dead
   inputs. This lane reported it as a TREATMENT question (a colour has no *neither* value, so picking
   one looked like the laptop's call) and **that was wrong — the reasoning assumed the field should
   EXIST.** It had no reader anywhere, so `T42` applied. **The deletion CASCADED, which is the
   evidence "no reader" was true rather than merely greppable:** `TheaterPalette.TeamColors` and the
   `pickedHome` local fed nothing else and went with it, **removing the last
   `PickedHomeForPresentation` call in that composer.**
3. ~~**`T94`'s SEAM**~~ — **CLOSED, `T94-cl`, DD batch 207.** See the top of this section.
4. **Item `1.1` — WITH THE DD, AND IT IS A DESIGN JOB BEFORE IT IS A BUILD.** §6.7's interstitial
   at the fixture boundary, uncoupled from `T94` by `T140-am4`. Batch 207: **the DD designs it first
   — three pieces, TEMPORAL evidence, and NO CAPTURE WINDOW YET.** Its site is the fixture change
   inside `PlaySweat()`. **Do not build against it until the spec lands**, and note that temporal
   evidence is not what the D2 harness shoots: those bursts freeze a moment, and this needs duration.
5. ~~**The multi-scorer counter's PER-LEG structure**~~ — **DONE, `5c96435`, and it discharged
   `T169-am2` on the way.** Two live legs on one telling, two players on OPPOSITE sides, each count
   pinned to its own man; mutation-proven green → RED (*"leg 1 backs 'Deke Gasket', who scored 1, and
   its counter peaked at 0"*) → green. **The assertion is per leg against its own man, never "both
   counters moved"** — two legs counting the SAME player would satisfy that while BEING the defect.

**SO THE ONLY LIVE ITEM IS 1, AND IT HAS NO ACTION EVEN IN PRINCIPLE:** the strip frame arrives free
with whatever capture run happens next, and building a window for it is the seed-hunting batch 203
rules out. **A seat picking this lane up has nothing to build until the DD's `1.1` spec lands.**

### THE THREE LAWS THIS STRETCH PRODUCED, AND WHAT EACH COST

**`C62` — CODE CITES RULINGS, AND CITATIONS GO STALE.** `c24b32c` deleted
`PickedHomeForPresentation`'s fifteen-kind table; **three comments quoting it survived.** The first
two misled a reader — including this seat, whose orientation diagnosis was retracted as *a
remembered read*. **The third JUSTIFIED WRONG CODE.** Batch 205's sharpest clause is about
propagation: **when a claim is deleted from code, its copies do not go with it** — one sentence was
pasted into three sites and each reasoned from it independently. The grep runs AT THE MOMENT the
function changes, not periodically.

**BATCH 206's LAW — ASK THE PROPERTY (`AnchorSide`), NEVER THE KIND.** Every defect in the sweep was
one predicate standing in for another: `Kind == Moneyline` for *does this leg name a side*, `isMl`
for *is there a backed club to mark*. **A kind check is a proxy, and a proxy is wrong for exactly the
case nobody pictured** — here, the one moneyline choice that backs nobody.

**`C63` — A GATE THAT CHECKS FORM CANNOT SEE AN INVERTED SENSE.** The handicap double-swap printed
`CLEAR BY n` where `TRAILING BY n` belonged, on a live bet, **and passed 342 EditMode + 153 PlayMode
tests across two commits** — because the string was WELL-FORMED. Right template, right span, no
blank row, no overrun: every property the suite asks about. **Where a string asserts a FACT about the
player's position, the gate must relate the word to the fact, not to the template.**

**AND ITS COROLLARY, which this lane hit: A MUTATION TEST PROVES NOTHING UNLESS THE RUN CONTAINED THE
CASE THE MUTATION BREAKS.** The sign gate went green → RED → green, `K17`'s standard. But the red
landed only on an **away** leg: a home-backed leg is SYMMETRIC under the swap and passes UNDER the
bug. **`Assert.Greater(awayLegsChecked, 0)` is load-bearing** — without it a seed run meeting only
home legs produces a GREEN mutation test, certifying a gate with a run that could not have failed.

### ⚠ TRAPS THIS SEAT PAID FOR — ordered by what they cost

1. **A GATE CAN BE GREEN FOR THE WRONG REASON, AND I SHIPPED THREE THAT WERE.** `T169`'s two
   discriminators (`id != SheetName` — false once `T168` made them agree; `Identity != "MARKET PICK"`
   — false because that IS every non-team market's identity), and the draw-name pin, which passed
   only on CASING because `DisplayLabel` says `Loopholes` where the assert looked for `LOOPHOLES`.
   **The habit that catches it: when a gate passes, READ ITS LOG and ask what would have to change
   for it to fail.** Print the value, do not merely assert it.
2. **`TvSweatScreen.cs` HAS MORE TEST SURFACE REACHED BY REFLECTION THAN BY THE COMPILER.** Four
   run-time-only failures this stretch: two callers when `DescribeActiveLeg` gained a leg index,
   `CopyTicket`'s TYPE (it is on `RevealedView`), and its SCOPE (`RevealedView` is a NAMESPACE
   SIBLING declared above the screen in the same file, not a nested type). Each compiled green.
   **Every reflective lookup here carries an `Assert.IsNotNull` saying it fails rather than silently
   checking nothing — that is the only reason they were cheap.**
3. **THREE SOURCE-SCANNING GATES FIRE WHEN CODE MOVES, NOT WHEN IT BREAKS** — `T69`'s anchor, `G1`'s
   window, and `T62`'s TWICE on one commit: first my counter block sat between `CompleteGoal` and
   `RepaintRevealedScore`, then my COMMENT about not putting things in that gap sat in it. **Both
   times the fix was to move code, never to widen the scan.**
4. **A CAPTURE WINDOW CAN BE PERFECTLY BUILT AND PHOTOGRAPH NOTHING.** D2 took four attempts: a
   trigger keyed on CHANGE rather than the SUBJECT (fired on the scorebug being CLEARED between
   tellings, shot a teardown frame, passed green in 110s); a missing `[Timeout]` against NUnit's
   180s default, which only bit once the trigger became honest and therefore slower; and **the
   moment not existing at all** — both legs backed HOME on an unsearched seed, fixture f LOST, and a
   parlay dies at its first dead leg.
5. **THE LEDGER WILL NOT OUTRUN THE LOCKED RESULT.** `CompleteGoal` clamps every commit to the
   endpoint, so a test cannot force a scoreline. The sign gate's first premise did exactly that and
   asserted CLEAR against a leg that was genuinely TRAILING — **the code was right and the
   expectation was invented.** Derive the expected sense FROM the locked scoreline instead.

### OPERATIONAL — a Unity run's failure has FOUR signatures, and one poisons the well

| what you see | cause |
|---|---|
| `TIMED OUT after 900s` in the runner's stdout | `run-unity-tests.ps1`'s own `-TimeoutSeconds`; **it announces itself** |
| `Timeout value of 180000 ms was exceeded` | NUnit's per-test default — add `[Timeout(ms)]` |
| editor log truncates mid-stream, **no marker at all** | terminated outside its own control flow |
| stalls at *Compiling Scripts*, `More than one copy of bee_backend` | **a stray editor from an earlier timeout** |

**A STRAY EDITOR SURVIVES THE RUNNER'S OWN TIMEOUT.** It then holds `bee_backend` and every later run
stalls. The habit is to check procs BEFORE a run; the failure mode is a corpse left AFTER one.
**Check for a stray `Unity.exe` after any timeout.**

**AND `URP.png` CANNOT BE RESTORED AT ALL** — it is a JPEG under an LFS `*.png` attribute whose index
entry is a raw blob, so the clean/smudge round-trip can never match and `git checkout --` REFUSES.
Dirty by construction. `ProjectSettings.asset`, the TMP fallback and the three TV SDF atlases DO
restore and must be, after every suite.

---


## 0-ROT5. LANE STATE 2026-08-25 (third) — READ THIS FIRST

**Written while HOLDING, not at a handover — the seat has not rotated.** State is `7b28fa8`, merged.
**EditMode 342/341/0/1 · PlayMode 153/126/0/27.** Editor closed, procs 0. Tree clean but `URP.png`
(permanent phantom) and the untracked `artifacts/`. If work lands before the seat actually rotates,
this section is the base to amend rather than the record to replace.

### THE CHAIN IS DONE EXCEPT ONE ITEM, WHICH IS HELD ON A RULING

`638df13` routed the two questions the NEED build raised · `5e5348e` the `T156` re-take post-`T168` ·
`ca4f410` `WinningMargin` bucket 1 · `2ff03a6` `T169-am`'s owed fixture · `7b28fa8` **`T94`'s seam,
closed.** Rulings folded this stretch: batches **196** (`T169-am`, `T151-am3`, `T152-am2`), **197**
(`T140-am4`, since corrected), **198** (`T140-am5`, `T151-am4`, `T140-am6`).

**⚠ OPEN AND HELD: the multi-scorer counter.** `T140-am6` rules it arm A's N-live class and **its own
item** — a structural single-leg assumption that predates the build, not `T169-am`'s missing test.
**Do not start the remedy until that ruling lands.** Deliberately not in `7b28fa8`.

### THE COUNTER'S NUMBER, AND IT IS WORSE THAN THE ITEM SAYS

`T169-am`'s owed fixture is built and green:
`TvSweatScreenTests.T169_am_the_multi_scorer_counter_is_driven_by_a_real_goal`.

```
[MULTI] DRIVEN: 'Darryl Ledger' scored 2 on seed 'MULTI-0' … _pickedScorerGoals PEAKED AT 1 …
        _stageLeg took -1/0.  EXPECTED if wired: 2.  ** DOES NOT MATCH **
```

**IT IS WRONG ON THE SIMPLEST CASE THERE IS — a ONE-LEG ticket, `_stageLeg` never leaving 0.** So the
row would read `1 GOALS • NEED 1` on a leg already won. `T140-am6`'s anchor-only reading is real and
is confirmed in code, **but it is not the whole defect**, and a remedy aimed only at the N-live shape
would leave this one standing. Reproduced identically on two separate runs.

> **AND MY OWN ROUTED HYPOTHESIS WAS NOT WHAT THIS IS.** I predicted the `_stageLeg` hole — a
> multi-scorer leg that is not the anchor never counting. That is a real shape and the DD confirmed
> it; **it is simply not the cause of this number.** A fix aimed at the thing I routed would have
> missed the thing the fixture found.

**The fixture searches its own seed and proves the search valid**: nothing lets a test choose who
scores, so throwaway `Run`s are locked per seed until a repeat scorer appears, and the room run is
pinned to that seed. That is only sound if the stat line is independent of what was bet
(`Rng.Outcomes` is a stream the betting path never draws from), so it **re-asserts the scoreline
after placing a different ticket** rather than trusting the reading.

### `T94`'s SEAM — CLOSED, AND THE GATE THAT FAILED WAS CERTIFYING THE DEFECT

Built to **batch 198**, not 197: `UpdateTicketColumn(_liveLegsShown)` at both sites,
`LegsOfFixtureAfter` deleted. **A SUBSTITUTION, not a deletion** — `MarkPresentedResolved` sets flags
and repaints nothing, `UpdateTicketColumn` is the only writer of `_legRow[i].IsLive`, and
`AnimateLegPulse` reads that cached flag every frame, so a bare deletion leaves the ENDED fixture lit
and pulsing through the whole beat: **`T94` inverted.**

**`TicketFooterWord_…_StakeWhenLegTwoWonEarly` went red, and the failure was the fix working.**
`frames=59` and `state1=49` were IDENTICAL before and after; only `state2` moved, 2 → 0. Two frames.
Instrumented, both fired at **`_stageLeg = 0`** — leg 1 lit and read as ALREADY WON while the stage
and scorebug were still on leg 0's match, its "revealed count" being leg 0's, because `_countLedger`
only resets in `BeginStageLeg`. **The gate had been certifying a footer word off `T94` itself**, and
the 2026-08-17 seed search that picked `STATS-MULTI-5` measured the same artefact.

### THE GATE'S NEW SHAPE — a self-re-running search, and an INVARIANT rather than a wider assertion

- **It searches at run time** over 12 candidate seeds instead of pinning one, so it cannot silently
  go stale again. The pinned-seed comment's own instruction is what governs: *"RE-RUN THE SEARCH —
  never widen the gate."*
- **A state-2 frame counts ONLY when the stage is on leg 1's own fixture.** That is the re-base:
  what the ruling makes invariant, not a looser claim. Neither state's definition is relaxed.
- **`Assert.AreEqual(0, preemptS2)` per seed** fails if a state-2 frame is ever seen with the stage
  still on leg 0 — **the retired defect can never satisfy this gate again.**
- **Both states end in end-of-run assertions** that fail when the state is absent
  (`Assert.Greater(state1Cases, 0)` / `…(state2Cases, 0)`). The `[TRAP-GATE]` line reports beside
  them and carries nothing.
- Two seeds, one per state, is sanctioned — `StakeWord` is a pure function. §5 requires both states
  certified, never that one seed carry them.

```
STATS-MULTI-5: frames=90 state1=49 state2=0 | STATS-MULTI-1: frames=52 state1=29 state2=0
STATS-MULTI-2: frames=82 state1=11 state2=50
```

> **A CORRECTION TO WHAT I FIRST REPORTED:** I said no seed carries both states any more.
> **`STATS-MULTI-2` carries both** — 11 and 50. It read `state1=0` in the first search run only
> because of the warm-up I then removed. **The claim was an artefact of my own instrument.** The
> two-seed shape stands, but its reason is the max-picking, not an impossibility.

### ⚠ THE TWO SEARCH-SHAPED BUGS — both mine, both in a test that had never searched before

**A gate that becomes a SEARCH inherits none of its single-seed habits safely.** Everything the old
gate could do once, it now does N times, and three things that were harmless became defects:

1. **THE 30 WARM-UP FRAMES WENT BLIND ON 42% OF THE SEARCH.** *"Let the first beat render a
   scorebug"* — harmless on one pinned seed, and at `TimeScaleOverride = 0.0001f` it **consumes an
   entire short sweat**: the first search run reported `frames=0` on **FIVE of twelve** candidates.
   Dropped entirely; the qualifying condition needs `chip0 == "W"`, which cannot be true before leg 0
   resolves, so early frames disqualify themselves.
2. **`TimeScaleOverride` DOES NOT SURVIVE `StartNewRun`.** Set once before the loop it silently stops
   applying, and every subsequent sweat runs at wall-clock. **This blew a 600s budget twice** before
   it was found. Re-assert it at the top of each iteration.
3. **A SEARCH MULTIPLIES ITS FAILSAFE BY ITS CANDIDATE COUNT.** The gate's 60s hang-guard × 12 seeds
   is a twelve-minute wait that reads as a hang, not a slow test. Now 20s — still twenty times the
   honest budget, and a seed needing more has something wrong worth finding fast.

### THE OTHER THINGS THIS STRETCH ESTABLISHED

- **`T156` post-`T168`**: the pair is UNCHANGED (`FERRETS OVER 1.5` for goals AND cards), the
  city-only survivor is GONE (`MOOSE JAW` → `SPREADSHEETS`), and **the control no longer collides** —
  the line survives now, so `FERRETS OVER 4.5` ≠ `FERRETS OVER 1.5`. Two of the first run's three
  findings retired by the build; the DD rules §4.
- **Bucket 1's ladder ends a rung early**, rung 2 at 254.9 vs 261.0 — **6.1px**. `T151-am4`: **no gate
  owed**, and the rule is worth carrying — *a slender margin owes a gate when what lies past it is an
  OVERRUN, and owes nothing when what lies past it is the next authored rung.* Watch the cliff, not
  the size.
- **`{n}` in `CLEAR BY {n}` is ratified as built** (`T152-am2`) — the goals that must CHANGE. Its own
  ruling warns a later seat against "correcting" it to the adjusted margin, which would print
  half-goals. **The one line offered for changing should not be changed.**
- **A Unity run dirties FIVE files beyond `URP.png`** — `ProjectSettings.asset`, the TMP
  `LiberationSans` fallback, and the three TV `EncodeSans*` SDF atlases (those only on runs that
  measure text). They appear AFTER a suite, which is exactly when you are about to stage.
  `git checkout --` them; stage by explicit path, every time.

---


## 0-ROT4. SEAT ROTATION 2026-08-25 (second) — READ THIS FIRST

**Wrapped on Allen's order before a transition, not at a context limit.** Two commits:
`1c0e400` (measure) and `64b3f70` (build). **EditMode 342/341/0/1 · PlayMode 152/125/0/27.** Editor
closed, procs 0. Tree clean but `URP.png` (permanent phantom) and the untracked `artifacts/`.

### WHAT IS DONE — the whole chain except `T94`

**`T169`'s four kinds are BUILT in both slots** — `Handicap`, `PlayerMultiScorer`,
`TotalGoalsOddEven`, `WinningMargin` — compact identity in `AuthoredStatement`, NEED + progress in
`SweatActiveLegModel`. Zero new copy: every string is `T151`/`T152`/`G1-am11` §3 verbatim.
**`T168-am` is built** (`ShortenSubject`). **`T143-am9` is built** (`PendingLegName` takes the
authored arm). **`T143` §3 is built** with `S85-am3`'s `N GO ON`. **`T143-am8`'s gate is in.**

### ⚠ NEXT: `T94`'s SEAM — NOT STARTED, AND IT IS ONE BUILD WITH ITEM `1.1`

Confirmed by reading, not assumed. `T140-am3` names the defect as *"the interstitial fires per
TICKET, not per fixture, so a fixture change inside `PlaySweat()` gets no boundary treatment at
all"* — **that is item `1.1`'s site, word for word** (§6.7's interstitial at the fixture boundary,
held since rotation 2). The predecessor flagged them as possibly one thing; they are.

- **What must NOT change** (`T140-am3`): the pre-emption is RIGHT. Deleting it trades a brief wrong
  subject for a dead column at every fixture boundary. **The missing BOUNDARY is the defect.**
- **The seam is two identical lines**: `FinalSlam` and the theaterless path both run
  `MarkPresentedResolved(evt.LegIndices)` then `UpdateTicketColumn(LegsOfFixtureAfter(evt.LegIndex))`.
- **`1.1`'s own caveat still stands**: re-read the split doc against the contract before building —
  its fork-independence argument lapsed when Allen ruled (A).

### THINGS THIS SEAT MEASURED THAT CHANGE WHAT YOU CAN LEAN ON

1. **BATCH 195's 3.4px MARGIN WAS A STAND-IN, AND THE REAL NUMBER IS 120.4px.** The withdrawal of
   batch 189's ≥3 escalation rests on two names + separator at **631.6 vs 635.0**. That was
   `SPREADSHEETS UNDER 3.5` twice — **a club plus a line, a form neither the old `PendingLegName`
   nor `T143-am9` can emit.** Through the shipped methods the worst SAME-FIXTURE pair is
   `UNDER 10.5 CORNERS · PAVEMENT ANYTIME` at **514.6 vs 635.0**. `T143-am7` says a `FITS`
   conclusion survives a stand-in LONGER than the real copy — it was longer — **so the withdrawal
   stands a fortiori.** The gate is pinned to 514.6, not 631.6.
2. **`T143-am7`'s RULE HAS A MIRROR AND IT BREAKS THE OTHER WAY.** A `FITS` conclusion does NOT
   survive a stand-in SHORTER than the real copy. Batch 195's could have been either; **which it was
   is not readable off the string, only measurable.** Ask that question of every inherited width.
3. **Every authored rung CLEARS and nothing truncates**, over saturated pools. `WinningMargin` rung
   3 `2 APART AT FT` 181.7 / `3+ APART AT FT` 196.1 vs 261.0 (rungs 1 and 2 both miss, reproducing
   `T161`'s 380.8/283.2 to the decimal). **`TotalGoalsOddEven`'s rung 1 NEVER RENDERS** — 314.9 and
   326.5 vs 261.0 — which is what confirming rather than inheriting `T161` bought.
   `Handicap` 20/20, `PlayerMultiScorer` 12/12, reproducing `T169`'s inherited 249.4 and 175.4.
4. **BATCH 192's GUARD IS LOAD-BEARING AND NOW HAS A NUMBER**: through `LegStatement`'s `default:`,
   **16 of 369** names overrun the 635.0 zone (worst 692.5). Through the authored arms, **0 of 125**.
5. **`T156`'s four cases re-ran unchanged** at `1c0e400` — pre-`T168` baseline. **They are now OWED
   a re-take**: `T168` is BUILT, the route doc's own last bullet says all four must be re-measured
   because the shorter input may leave the distinctive word inside the box. **Not done this seat.**

### OWED / ROUTED — none of it blocking `T94`

- **`WinningMargin` BUCKET 1 IS OFFERED AND HAS NO AUTHORED COPY.** `MatchModel.BuildOffers` runs
  `m = 1..TopMarginBucket` unfiltered; `MarketSelection` says margins 1 and 2 are EXACT. Verified
  against a real board (12 seen in one run), not read off source. `T151`/`G1-am11` §3.2 stop at 2.
  It falls through to `default:` → `1 GOAL` (89.2px, fits, renders silently) — **the exact
  total-goals collision `T151` authored `MARGIN`/`APART` to prevent.** DD's.
- **`{n}` IN `CLEAR BY {n}`/`TRAILING BY {n}` IS NOT RULED.** `T152` authored the forms and never
  defined the number. Built as `ceil(|margin + line|)` — the goals that must change to flip the leg.
  **One line to change.** DD's.
- **`T156`'s re-take** (above), and the **§3 re-measure is NOT owed** — the gate covers it.
- **The three team totals** stay held; `T169`'s escalation is unaffected by `T168`.

### ⚠ TRAPS THIS SEAT PAID FOR — two are new, and both were MY OWN GATES

1. **A DISCRIMINATOR THAT MERELY LOOKS DIFFERENT IS NOT A DISCRIMINATOR.** Two gates I wrote to
   prove "the new arm was reached" were both false. (a) `id != SheetName(leg)` — **with `T168`
   built the sheet name for a handicap is `MEATBALLS -1.5`, character-identical to `G1-am11` rung
   3.** Two correct strings agreeing is not a fall-through. (b) `Identity != "MARKET PICK"` — that
   IS the identity for every non-team market by design (`T96`). Replaced with things that can only
   be true one way: **the DECK's exact string**, and **a non-empty progress line** (the `default:`
   arm passes empty). *"Did my case occur"* needs a discriminator, not a difference.
2. **A SOURCE-SCANNING TEST BREAKS WHEN YOU SPLIT A METHOD, AND ITS PREMISE MAY BE FINE.**
   `TvSweatScreenPaletteTests`' `T69` scans `private string LegStatement(` to its next sibling; the
   split moved `{club} ML` out of the window. **Re-pointed, not widened** — the premise never
   changed, only the anchor. Its own comment says to do exactly that. *(Note: the file's OTHER scan
   survived, because its end marker is `FitOrFallback` and the window still spans both methods.)*
3. **`FitOrFallback` IS REACHED BY REFLECTION FROM FOUR GATES** with three boxed args. Widening it
   to `params` compiles and breaks all four silently. `FitLadder` was added beside it and
   `FitOrFallback` delegates — **check reflection callers before changing any private signature.**
4. **A UNITY RUN DIRTIES MORE THAN `URP.png`.** This seat's runs also touched
   `ProjectSettings.asset` (a scripting-define) and `LiberationSans SDF - Fallback.asset`.
   **`git checkout --` them; stage by explicit path, every time.**

### RISK CARRIED FORWARD — stated, not hidden

**`_pickedScorerGoals` IS UNPROVEN ON A BEAT.** `PlayerMultiScorer`'s progress line needed a
revealed per-player goal count that did not exist; it is now incremented in `OnGoalPlayed`'s
named-scorer branch. **The model arm is gated in EditMode; the COUNTER is not** — no PlayMode
fixture drives a multi-scorer leg through a goal. That is this lane's own trap #1 and it is named
rather than left to be discovered. **A gate for it is the cheapest thing on this list.**

---


## 0-ROT3. SEAT ROTATION 2026-08-25 — READ THIS FIRST

Rotating at 97%. **Phases 2-3 are DONE and merged — all four steps, both `onFinalLeg` twins, the
anchor split, the neither-branch set, and the pending window.** The tree is clean but `URP.png`
(permanent phantom). No editor running. Engine DLL clean.

### THE CHAIN, IN ORDER

1. **NEED-copy build — NEXT, NOT STARTED.** `docs/design/spec-need-copy-and-club-naming-2026-08-24.md`
   + `T168`. **Read it, MEASURE, then build** — that order, not the reverse. This seat was told to
   plan each before building and every measurement changed the plan.
2. **`T94`'s seam** — batch 188. `T94-am3`: arm A narrowed the multi-fixture desync to ONE window and
   PRESERVED it deliberately; **the gate discharges but the ITEM does not.** `T140-am3` binds what
   must NOT change: **the pre-emption is right, the MISSING BOUNDARY is the defect.**
   > **A CONNECTION WORTH CHECKING BEFORE PLANNING IT SEPARATELY:** `T165` bound the counter and
   > §6.7's interstitial to ONE event — *"the boundary is where the counter increments."* Item `1.1`
   > (that interstitial) has been HELD since rotation 2. **If `T94`'s defect is the missing boundary,
   > `1.1` and `T94`'s seam may be the same absent thing seen from two sides** — plan them together
   > or the boundary gets built twice, which is what `T165` warned about.

### RULINGS FOLDED IN THIS SEAT — the two that change how you BUILD

**BATCH 192 (`T143-am5`) — BUILD TO THE RULING, NOT THE BUILD STATE.** `T168-am` is RULED and
UNBUILT. A new surface that imitates today's unbuilt behaviour **has to be fixed twice**. Apply the
ruled form at your own call site. Concretely: do NOT reach a club name through `LegStatement`, whose
`default:` goes `SheetName` → `MarketSheet` → the FULL club name.

**BATCH 194 (`T143-am7`) — A PLACEHOLDER'S ERROR IS ASYMMETRIC.** A placeholder LONGER than the
authored copy fails in ONE direction only: **a `FITS` conclusion survives** (the real string is
shorter, so it holds a fortiori) and **an `OVERRUNS` conclusion does NOT** — the overrun may be the
placeholder's own length. **Ask which direction a width was used in before relying on it.** Two
widths in this seat's own `route-pending-window-height-2026-08-24.md` fail that test (`699.5`,
`870.4`); the two `FITS` conclusions stand.

### ⚠ THE TRAPS THIS SEAT PAID FOR — in the order they cost most

1. **A GATE THAT RAN WHILE ITS CASE DID NOT — four separate times.** `T130` green for weeks without
   ever rendering its market; the TV twin's compatibility test that an off-by-one mutant PASSED; the
   part-C fixture whose first legal combination could not distinguish the two rules; and a counter
   fixture that would have rendered a FALLBACK. **A green is not coverage.** Assert the case occurred
   — and when it cannot, FAIL, do not pass.
2. **`_session` UNSET RENDERS A FALLBACK AND PASSES.** `FixtureTotal()` is
   `_session != null ? _session.FixtureCount : _ticket.Legs.Count`; `AnchorForTelling` and the pending
   window read the session too. A fixture that sets only `_ticket` tests the fallback.
3. **A SPLICE BETWEEN A DOC COMMENT AND ITS `[Test]` ORPHANS THE DOC — and it COMPILES GREEN.**
   Done once in `TvSweatScreenLayoutGridTests.cs`, shipped, found only because an unrelated compile
   error made this seat look. **Anchor above the doc, never between doc and method.**
4. **`.git` IS A FILE IN A WORKTREE**, holding `gitdir:`. Any tool reading `HEAD` must handle it —
   this lane always works in a worktree, so the file form is the NORMAL case here.
5. **A RULING CAN SUPERSEDE A TEST'S PREMISE.** Three re-based this seat: `SweatFlavorDrawAnchorTests`
   (`T163`), `SweatPresentationModelTests` (`T164`), `T88`'s prompt pin (`T143`). **Re-base to what the
   ruling makes INVARIANT** — `T88` now pins `N LET` … `DIE`, not a club name that varies by seed.

### OPEN, ROUTED, NOT BLOCKING

- **The `N>=2` composition is a deliberate TODO** in `PendingWindowBeat` (batch 193 cited in place).
  Its number is TAKEN — two bare names + separator = **631.6 vs 635.0, FITS, 3.4px spare**, reading
  (A) — so `T143` stands unamended at every reachable N. **The copy is the DD's to author.**
- **Club-only collisions in that future list**: two dead legs on the SAME club print identical names.
  `T156`'s shape in a second place. Lands the moment §3 does.
- **`T156` is LIVE and wider than it names** — `route-team-total-fallback-measured-2026-08-25.md`. The
  distinctive word never survives; a two-word city truncates to the CITY ALONE, colliding across every
  market that club appears in. **And the CONTROL collides**: corners' unshared line cannot protect it,
  because the line is dropped three words before the survivor.
- **Seven offered kinds still have no authored NEED copy** — they name the bet, not the requirement.

---

## 0-ROT2. SEAT ROTATION 2026-08-24 — READ THIS FIRST

Rotating at 97% context, with **phases 2–3 planned and none of it started**. Section 1 of the
drawn ending is closed except `1.1`, which is held on purpose. The tree is clean apart from
`URP.png` (permanent phantom). **Steps 1 and 2 below are heavy `TvSweatScreen` diffs — that is why
this seat stopped rather than starting one.**

### WHAT SHIPPED, AND WHAT EACH ONE ACTUALLY FIXED

| item | commit | the part worth knowing |
|---|---|---|
| `T147-am`/`am2` | `83d8072` | footer 40→60, slots 6→4. **The slot count was reserving two rows `RunConfig.MaxLegs = 4` makes unfillable** — `Run.cs:190-191` ENFORCES it. That is what paid for the taller footer with `T24`'s margin intact. |
| settled-ticket fix | `11e4ad7` | remedy 1 **plus** the reveal gate. Remedy 1 alone propagated the leak into the rows and turned the pin GREEN while still leaking. |
| `T91-cl` | `a8d9a73` | `LEG n/m` left the scorebug for the ticket header. Ink clearance −41.7 → **+86.8px**. |
| phantom pin `T158` | `3126ee5` | every measured fixture must be renderable in its slot. Caught a fourth phantom on its first run. |
| `1.3` correct score | `d90f122` | the arm AND the caller wiring. **The arm alone would not have fixed it** — the caller's `default:` returned an empty copy, which IS the blank column. |
| `1.2` `T130`'s gate | `9ffe399` | a RENDERED row is never empty, per ROW not per SPAN. |

**Suites at rotation: EditMode 320/319/0 · PlayMode 149/124/0.**

### TWO CORRECTIONS THIS SEAT CARRIED WRONG — fixed, and stated so they are not re-inherited

1. **`T163`'s neither-branch lines are LANDED, NOT OWED.**
   `docs/design/spec-neither-branch-lines-2026-08-21.md`, batches 168/171, §5 authors the club-free
   set in full. This seat's earlier plan called step 4 BLOCKED on the DD. **It is not. It is
   buildable.**
2. **`DramaEvent` carries NO possession side — re-checked AFTER phase 1.**
   Fields are `LegIndex`, `FixtureIndex`, `IsSharedTelling`, `LegIndices`, `LegProbs`, `Step`,
   `TotalSteps`, `Type`, `WinProbAfter`, `Tag`. The spec's §3 says *"if `DramaEvent` already carries
   a possession side, §3 is dead and should be deleted rather than shipped unused."* **It does not.
   §3 IS LIVE and the momentum fallback ships.** That is the answer the spec asked this lane for.

---

## PHASES 2–3 — THE PLAN, AGAINST THE PUBLISHED CONTRACT

`docs/handoffs/theater-engine.md` §6 is the break list. **Read it before touching anything.** Phase 1
is merged; the DLL in the tree is the engine lane's rebuild.

**THE GATE FOR EVERY STEP, and it needs no editor lease:**
`dotnet test engine.tests` — `GoldenSeedTests` mirrors the golden byte-identity pin, plus phase 1's
`SharedTellingTests` and `TicketWinProbabilityTests`. Then EditMode, then the 80 TV PlayMode cases.
**Run the engine gate FIRST on every step** — it is the cheapest and it is the one another lane wrote.

### STEP 1 — §6c, the probability sites
Display re-points to `SweatSession.TicketWinProbability`. `RevealedView.Reset` (`TvSweatScreen.cs:76`)
stops seeding from `Legs[0].TrueProb`. `_stage.SetLiveProb`, `_probTarget`, `_prevProb`/`_pendingProb`
all stop reading `evt.WinProbAfter`.

> **THE SUBTLE ONE IS `TheaterChoreographer` (`:217`, `:235`, `:285`).** The contract's own words:
> goal staging may legitimately stay leg-scoped, **but it must read `LegProbs`, not `WinProbAfter`,
> "or it silently reads the anchor leg's number for every leg."** Silent wrongness, not a crash —
> nothing fails, every leg just shows one leg's probability.

### STEP 2 — §6a, N grades at one whistle
Loop `evt.LegIndices` at the resolve sites (`:2056`, `:2057`, and the `_resolvedThrough` /
`UpdateTicketColumn` / `int k` triples at `:2085-2087` and `:4030-4031`, `:4009`).

> **A DEPENDENCY THE CONTRACT DOES NOT NAME, AND IT IS THIS LANE'S OWN.** The settled-ticket reveal
> gate derives `revealedLoss` by scanning `i < _resolvedThrough`. Grades land in LEG ORDER after one
> hold (`T87-am2`), so the high-water mark itself may survive — **but its UPDATE,
> `_resolvedThrough = evt.LegIndex + 1`, is wrong under N-live and must become `max(LegIndices) + 1`.**
> Get it wrong and the footer resumes announcing deaths before the reveal: the `T144`-era leak
> reopening through a different door. **Re-verify with the armed assertion** (remove the gate, watch
> it fail at frame 23), never by inspection — that assertion exists because inspection missed it once.

### STEP 3 — §6d, the leg counter (`T165`)
Half-done. `T91-cl` already moved the element to the ticket header and its width is measured
(66.9px ink, 86.8px clearance). This repoints the REFERENT to `CurrentFixtureIndex`/`FixtureCount`.
**`T165` says land it with `T91-cl` or the element moves twice** — `T91-cl` has landed, so this rides
soon or the cost is paid again.

### STEP 4 — §6b, the flavour subject. **BUILDABLE — this seat's "blocked" was wrong.**
`T163`'s anchor rule plus the landed line set. Sites: `_ticket.Legs[evt.LegIndex]` (`:1721`, `:3466`,
`:4008`), `_flavorLegSeen` (`:3470-3472`), `SweatFlavor`'s `picked`/`other` (`:25-64`, `:201`, `:216`),
`onFinalLeg` (`:1683` — should be the final FIXTURE), `BeginStageLeg` (`:3515-3516` — per fixture now).

**Ship §3's momentum fallback**: `DramaEvent` has no possession side, so a momentum beat in the
neither branch takes the club-free line. **Do NOT re-case the lines** — §4 says they match the table
they join, sentence case with a terminal period, and re-casing a shipped table silently is exactly
what that clause forbids.

### THE FREE REDUCTION — §6f, and one correction owed
- `PresentationSceneKey` (`:70-82`, `:110-122`) is **already match-scoped** and its author's note asks
  for exactly this change. After phase 1, `DramaEvent.Step` IS that shared cursor — **discharge the
  note rather than working around it.**
- **`UpdateTicketColumn`'s doc comment is now FALSE IN CODE.** `T142` struck its stale half ("the
  engine forbids two legs on one matchup"); phase 1 makes the case real. Its other half — the column
  reads legs as a collection and is N-live-capable by construction — **stands and is load-bearing.**

**Sequencing:** steps 1–3 are independent of each other and of the DD. **1 and 2 both touch
`TvSweatScreen` heavily — separate diffs, never one.** Step 4 is the largest and touches `SweatFlavor`.

---

### FIVE THINGS THAT COST THIS SEAT TIME

1. **The runner ABANDONS Unity on timeout rather than killing it.** Both suites reported
   *"TIMED OUT — executed case count UNKNOWN"* and then finished and wrote clean results. **A timeout
   verdict from `run-unity-tests.ps1` means the WRAPPER gave up, not that the run failed** — and every
   abandonment leaves an editor holding the lock for the next run to fight. Two such editors from
   20:34 squatted the lock for FOUR HOURS. **Launch detached and poll the results artifact.**
2. **`Get-Process` lies about Unity; `Get-CimInstance Win32_Process` does not.** Exited-but-unreaped
   entries still list. The orchestrator's "Unity procs 0" census was wrong twice on this basis.
3. **PlayMode now takes ~1000s**, against the wrapper's default limit. It will always "time out".
4. **A blank leg row is CORRECT when no ticket is rendered** (`ClearLegRow` via the null-ticket
   branch). `T130`'s gate scopes itself on the footer being non-empty for that reason; an unguarded
   version fails on legal frames.
5. **Read the compiled DLL, not `engine/` source.** This seat wrote `HomeGoals` from source when the
   compiled member is `ScoreHome`. The engine lane owns and commits `SBR.Engine.dll`; **it is no
   longer a "never stage" file** — discard local build side-effects so theirs lands.

### OWED, AND NOT STARTED

- **`1.1`** — §6.7's interstitial at the fixture boundary. **HELD DELIBERATELY**: its site is the
  fixture change inside `PlaySweat()`, which is what phase 1 restructured. Now that phase 1 has
  landed, **re-read the split doc against the contract before building** — its fork-independence
  argument ("a strict subset under (B)") lapsed when Allen ruled (A).
- **`T130`'s gate has not met its subject.** The run dealt `kinds=[Moneyline,Moneyline]`, so it has
  never seen a `CorrectScore` row — the market `1.3` un-blanked. **Sound but unproven against the one
  kind this section fixed.**
- **`T158` guards test↔pool agreement, NOT pool↔code.** A phantom that lives in the POOL is invisible
  to it — that is how the fifth phantom (`TICKET n OF m`) survived. Five have been found; assume a sixth.
- **DoubleChance still does not fit** — 16/20 clubs truncate to the club alone. With Allen for scope.
- **The scorebug ink collision** is fixed on the leg-counter side only; `Matchup`→`Clock` was always
  clear at +31.3px.

---

## 0-U23. PART C's DIRECTION MEETS ITS SHAPE — AND THE ARGUMENT IT SHIPPED ON WAS INCOMPLETE · 2026-08-25

**EditMode 335/334/0/1.**

```
[PARTC-DIR] seed PARTC-DIR-A matchup 3 OVER 1.5 + UNDER 2.5 | legs 2 fixtures 1
sameMatchBlock True p(t=0) 0.2169 | beats 4 · divergent-leg 3 · SIGN-DIFFERING 2
|| pool examined 10 diverged 10
```

### THE JUSTIFICATION PART C SHIPPED ON DOES NOT COVER THE CASE PART C EXISTS FOR

`0-U16` recorded the reason: *"the ticket's probability is a product of positive per-leg factors, so
it is monotone in each — while one telling is live the sign of the ticket delta equals the sign of
the moving leg's."*

**That is true of ORDINARY tickets and FALSE of same-match ones.** There the ticket's probability is a
**JOINT** (`Ticket.SameMatch.PTicket`, via `JointModel`), not a product, so monotonicity in each leg's
probability does not follow. **The code was never known to be wrong; the ARGUMENT was incomplete**,
and only a fixture settles it.

**Now measured: the ticket's sign departed from the anchor leg's on 2 of 4 beats.** The joint IS
non-monotone in its anchor, which is exactly why the spec's *"a single up/down ONLY because the
displayed probability is the TICKET's"* is load-bearing rather than decorative.

### ⚠ THE FIRST VERSION SHIPPED RED, ON PURPOSE, AND THAT IS THE LESSON

The brief's stopping rule was *the first LEGAL same-match combination*. It landed on
`PARTC-DIR-A / matchup 0 / OVER 1.5 + UNDER 2.5`: **4 beats, legs diverging on 3, sign-differing 0.**
On that fixture the retired leg-scoped rule and `T164`'s ticket-scoped rule print the IDENTICAL
direction on every beat — **a green there would have proven nothing.** Observed, not predicted:
`EditMode-partc-dir.xml`.

**The delegate refused to hunt a greener fixture** on the ground that it would make the assertion
tautological, and reported it instead. That was the right call and it is why this gate is real.

### THE REMEDY, AND THE DISTINCTION IT TURNS ON

The stopping rule became *the first combination that DISCRIMINATES*. **That is selecting a CASE, not
an OUTCOME**, and the difference is not rhetorical:

- The claim is an EXISTENCE one — the joint's sign CAN depart from its anchor's. A fixture where it
  cannot happen cannot test it, exactly as `T130`'s gate could not test `CorrectScore` while the
  policy dealt moneylines.
- **THE POOL IS REPORTED**: `pool examined N diverged k` on every run. A fixture chosen because it
  discriminates is honest only if the size of the pool it came from is stated.
- **"NONE FOUND" IS A RESULT, NOT A REASON TO KEEP LOOKING.** The search's failure message says so:
  if no combination discriminates, the re-base is UNOBSERVABLE and that is a finding about part C.
  *A search that runs until it finds agreement is not evidence of anything.*

**It settled on matchup 3 of the FIRST seed — two matchups from the blind one — with 10 of 10
examined combinations diverging.** Common, not exotic.

### `T166` IS PINNED HERE TOO

The gate asserts `MagnitudeBand` still partitions at 0.04/0.10 on both signs. Ticket-level deltas are
COMPRESSED by the other legs' probabilities, so moving those thresholds is the natural "fix" for a
later seat who notices the tape quietening — and `T166` ruled the quietening TRUE. The guard stops a
future correction from undoing a ruling.

---

## 0-U22. THE COUNTER MEETS ITS SHAPE — THE FIRST SAME-MATCH FIXTURE · 2026-08-24

**EditMode 334/333/0/1** (+1, so the file recompiled and the gate ran).

```
[T165-SAMEMATCH] legs 3 · fixtures 2 · counter rendered 'MATCH 1/2'
```

**Three legs, two tellings, and the counter says TWO.** Under the retired LEG referent it would have
printed `MATCH 1/3` — the leg total the ticket column contradicts, which is the incoherence `T165`
was ruled to fix. **First time the surface has been observed doing it.**

### THE GAP THIS CLOSES, AND THE TWO IT LEAVES A PATTERN FOR

`0-U13` recorded it: *"NO TEST RENDERS THE COUNTER ON A SAME-MATCH TICKET… the ruling exists for the
shape no test builds."* Every other fixture in both suites is ordinary, where fixture count equals leg
count and the two referents are indistinguishable.

**This is the FIRST fixture in either suite to build AND RENDER a same-match ticket.** The recipe is
reusable and the other two argument-only claims should copy it:

- the nested goal pair on ONE matchup (over the higher line entails over the lower — pure set
  containment, so no board change refuses it) plus a moneyline on ANOTHER;
- `Assert.Less(FixtureCount, Legs.Count)` **before rendering anything**;
- **set `_session`, not only `_ticket`.**

### ⚠ THE `_session` POINT IS THE TRANSFERABLE ONE

`FixtureTotal()` is `_session != null ? _session.FixtureCount : _ticket.Legs.Count`. **A fixture that
sets only `_ticket` renders the FALLBACK and passes** — `MATCH 1/3` on this very ticket — while
proving nothing about the referent. `AnchorForTelling` and the pending window read the session too.

**Same shape as `T130` never meeting its market and the TV twin's compatibility test passing under a
mutant: the gate ran, the case did not.** The reflection sets both fields and fails loudly if either
name moves.

### THE DISCRIMINATOR IS THE SECOND ASSERTION, NOT THE FIRST

`AreEqual("MATCH 1/{FixtureCount}")` is true under BOTH referents on an ordinary ticket — alone it is
a restatement. `AreNotEqual("MATCH 1/{Legs.Count}")` is what fails the moment the referent regresses.

**And the evidence line is logged BEFORE the verdicts**, deliberately: a `Debug.Log` under a failing
assert never runs, so evidence written last is lost exactly when it is needed.

---

## 0-U21. THE PENDING WINDOW — ONCE-PER-WHISTLE IS **VERIFIED, NOT BUILT** · 2026-08-24

**Closed without a gate, deliberately, and accepted by the orchestrator.** Recorded here because a
DECISION NOT TO BUILD looks identical to an omission six weeks later.

### THE PROPERTY IS ALREADY PINNED, ENGINE-SIDE

`engine.tests/SharedTellingTests.Both_legs_dead_at_one_whistle_opens_ONE_window_naming_both()`
asserts all three clauses the order named:

```
Assert.Equal(1, windows);                                     // ONCE per whistle
Assert.Equal(new[] { 0, 1 }, session.PendingDeadLegIndices);  // NAMES every dead leg
Assert.True(session.NoSingleCallSaves);                       // STATES when no call saves
Assert.Equal(LegState.Lost, session.RevealedLegState(0/1));   // AFTER every grade
```

### AND THE TV IS ONCE-PER-WHISTLE BY CONSTRUCTION

- `HasPendingLoss` is a **session-level** flag the engine opens once; both TV entry sites guard on it.
- `PendingWindowBeat` loops `while (_session.HasPendingLoss)` until it is drained.
- **The two entry sites are MUTUALLY EXCLUSIVE**: `PlaySweat` runs
  `if (_stage != null) { yield return TheaterBeat(evt); continue; }`, so the theaterless block is
  unreachable whenever a stage exists.
- Both open the window AFTER `ResolveBeat`/`FinalSlam` — the "after every grade" half.

### ⚠ WHY A TV GATE WAS REFUSED, AND IT IS THE LANE'S OWN LESSON

**It would be VACUOUS.** On every ticket shipping today exactly ONE leg dies per whistle, so "opened
once" is trivially true — the gate would pass without ever touching the N-live case it names.

**This lane has paid for that pattern twice in one day**: the TV twin's compatibility test that an
off-by-one mutant PASSED (its ticket busted on leg 0, so neither predicate ever fired), and `T130`
reporting green for weeks while never rendering the market it was written for. **A gate that cannot
fail on the case it names converts an open question into a false answer**, which is worse than
leaving the question open.

**If it is ever built, it lands WITH the same-match fixture that could make it fail** — that gap is
recorded in `0-U16`/`0-U13` and is the same one the counter and part C carry. Allen holds a veto and
may order it built anyway.

### WHAT THE TV ACTUALLY LACKS IS CONSUMPTION, NOT VERIFICATION

It reads `HasPendingLoss` and **nothing else** — never `PendingDeadLegIndices`, never
`NoSingleCallSaves`. That is `T143`/`S85`, and it is **blocked on the zone's height**, routed at
`route-pending-window-height-2026-08-24.md`: the zone is 635.0 x 90.0, holds exactly three rows, and
neither ruling fits as a fourth.

---

## 0-U20. THE CAPTURE WINDOW FOUND A SHIPPING DEFECT — TWICE, IN ONE CHAIN · 2026-08-24

**EditMode 332/331/0/1 · PlayMode 152/125/0/27** (+2 skipped = frames A and B, `[Explicit]`).

**The anchor window was granted to EVIDENCE a change. Its binding conditions found two defects no
suite could reach.** That is the argument for the conditions, not just for the frames.

### DEFECT 1 — `DescribeActiveLeg`'s `default:` RETURNED AN ALL-EMPTY COPY

A LIVE row blanks its compact line by design, so NEED and progress are the only spans it has. An
empty copy therefore rendered **a leg of the player's ticket as a completely blank row** — `T130`'s
defect exactly.

**It is item `1.3`'s defect surviving on other kinds.** `1.3`'s own record: *"the arm AND the caller
wiring — the arm alone would not have fixed it, the caller's `default:` returned an empty copy, which
IS the blank column."* `1.3` added the `CorrectScore` arm **and left the `default:` standing**.

**SEVEN OFFERED KINDS REACHED IT:** `Handicap` (4 selections), `TeamTotalGoals`/`Corners`/`Cards` and
`TotalGoalsOddEven` (2 each), `WinningMargin` and `PlayerMultiScorer` (1 each). Half the board.

### DEFECT 2 — AND THE CONSOLE HAD ALREADY RULED IT

Fixing (1) exposed `LegStatement`'s `default: leg.DisplayLabel`, which gave a live `Handicap` row the
bare word **`Handicap`** — a leg naming its market TYPE instead of the player's bet.

**`SweatLines.LegName` states the rule outright:** *"Nothing here falls back to the enum name:
**THAT FALLBACK IS K16/T130**."* The console removed it; **the TV kept it.** Same defect, on the
surface that did not get the ruling.

Both now read **`MarketSheet`** — the one composer this surface, the laptop and the console all print
through (`S96`, §6.5) — so an unauthored kind names the bet **in the words the board offered it in**.
No copy is invented, which is what `G1` actually asks for.

### THE GATE IS EXHAUSTIVE OVER THE BOARD, AND THAT SHAPE IS THE POINT

`ActiveLegCopyIsNeverBlankTests` sweeps **every selection the board prices** across three seeds —
measured at **25 selections, 14 kinds** — and treats a THROW as a failure too, since
`SweatActiveLegModel.Describe` throws on unarmed kinds and the caller's `default:` was what turned
that into silence.

**`T130` walks what the policy DEALS; its forced sibling walks ONE kind chosen in advance. The defect
was on a kind nobody thought to choose.** Enumerating the board is the only form that catches the
next one — it fails when a market joins the offered set without copy, rather than when someone
remembers to add a case.

### FOUR CHECKS, EACH CATCHING WHAT THE PREVIOUS ONE STRUCTURALLY COULD NOT

1. `T130` — proved a blank row is caught, on the one kind the policy deals.
2. The **mutation audit** — proved the gate DETECTS the defect. Says nothing about coverage.
3. The **forced-kind fixture** — proved `CorrectScore`. One chosen kind.
4. The **capture window** — forced a market NO test had rendered, and found it live on seven.

**A mutant proves detection, never coverage. A forced fixture proves the kind you chose, never the
one you did not. Only enumeration catches the market nobody thought about — and only a capture forced
it into view.**

### OWED, ROUTED NOT INVENTED

**Seven offered kinds have no authored NEED copy** and now fall back to the row's identity string.
NEED asks *"what does my money still need"*; the fallback answers *"which bet is this"*. **That is a
compromise and it is deliberate** — authoring NEED lines for those kinds is a DESIGN question and
belongs with the DD. What is fixed is the SILENCE.

---

## 0-U19. `T130` MEETS ITS SUBJECT — THE FORCED-KIND GATE · 2026-08-24

**PlayMode 150/125/0/25** — +1, so the file recompiled and the gate really ran.

```
[T130] legs=2 kinds=[Moneyline,Moneyline]     framesSampled=27  framesAsserted=27
[T130] legs=2 kinds=[CorrectScore,Moneyline]  framesSampled=22  framesAsserted=22
```

**THE SECOND LINE IS THE POINT.** `T130` had never rendered a `CorrectScore` row — the market item
`1.3` un-blanked, and the only one the drawn-ending section fixed. `DemoTicketPolicy` dealt moneyline
three times running. **`d90f122` shipped behind a gate that could not see its own subject**, and
that is now closed: 22 frames asserted on a rendered correct-score row.

### WHY A SECOND TEST RATHER THAN CHANGING THE FIRST

The policy-driven gate walks **what the game actually deals**, and that has its own value — if the
policy ever starts dealing something new, it shows there. The forced gate walks **what the ruling is
about**. Both share `WalkSweatAssertingNoEmptyRow` so the assertion cannot drift between them.

### THE ANTI-VACUITY GUARD IS THE LOAD-BEARING PART

The forced gate **asserts `CorrectScore` is on the ticket before walking a single frame**. Without
it the test could pass having rendered two moneylines — *which is exactly the hole it exists to
close*, and the shape that let the original report green for weeks. It also fails loudly if
`RefusalFor` rejects the pairing, rather than silently walking a different ticket.

**A mutant could never have found this.** Mutation proves a gate DETECTS its defect; it says nothing
about which inputs the gate has seen. Those are two different kinds of blindness and they need two
different checks.

---

## 0-U18. THE GATE AUDIT — EVERY GATE THAT PASSED FIRST TIME, MUTATED · 2026-08-24

**A GATE THAT PASSED FIRST TIME IS UNAUDITED.** This seat shipped one that was vacuous (the TV
twin's compatibility test, which an off-by-one mutant PASSED), so the rest were put through the same
check. **All five die under their own mutants. No source change ships from this** — every mutant was
reverted and the `.cs` files are byte-identical to `c24b32c`, which is stronger than a re-run.

| mutant | gate it had to kill | verdict |
|---|---|---|
| `IsPresentedResolved` ignores the set — the retired high-water mark restored | `A_leg_whose_fixture_has_not_been_told...` | **killed** |
| one §5.2 line reworded | `The_neither_branch_emits_spec_5_2_verbatim` | **killed** |
| a momentum variant duplicated | `Each_table_carries_three_distinct_variants` | **killed** |
| one line capitalised | `Every_neither_line_is_lowercase...` | **killed** |
| `LegStatement` returns empty | `T130_a_rendered_leg_row_is_never_empty` | **killed**, and it was the ONLY failure of 34 |

### THE ONE THAT MATTERED

**Step 2's interleaved gate had only ever failed on its PRECONDITION.** Its two actual claims — an
untold leg renders no verdict, the footer does not read its settled form — had never been observed
firing. The first mutant restores the exact defect the per-leg set replaced, and both fired. **The
gate the whole of step 2 rests on is real.**

### A SECOND GATE IS SENSITIVE TO `ticketSettled`, found as collateral

`A_previewed_leg_is_struck_and_dimmed_one_level_never_extinguished` also died under that mutant. The
preview strike reads `_cashOutPreview || ticketSettled`, and marking every leg presented-resolved
drove `ticketSettled` true. Not a defect — worth knowing that the strike has a second dependency on
that state.

### ⚠ WHAT NO MUTANT CAN CLOSE — `T130`'s MARKET COVERAGE, NOW MEASURED AND WORSE THAN RECORDED

The audit proves `T130` DETECTS a blank row. It says nothing about WHICH markets it has seen, and the
failure message answered that question directly:

> *Leg kinds this run: **Moneyline, Moneyline, Moneyline***

The handoff recorded `kinds=[Moneyline,Moneyline]`. **A third leg was dealt and it was moneyline
again.** `DemoTicketPolicy.Choose` is still handing this gate the one kind that cannot exercise it,
so **`T130` has STILL never rendered the `CorrectScore` row that item `1.3` un-blanked** — the single
market the drawn-ending section fixed.

**A mutant cannot close this and neither can another run.** It needs a fixture that FORCES the kind
rather than accepting whatever the policy deals. Recorded as owed, and it is the one gate on this
list whose green still overstates what it covers.

---

## 0-U17. STEP 4 PARTS A+D — THE ANCHOR SPLITS · SHIPPED 2026-08-24

**EditMode 331/330/0/1 · PlayMode 149/124/0/25.** No case-count change; two tests re-based.

### THE SPLIT, RULED BY ALLEN — `AnchorSide` WHERE IT ANSWERS, HOME WHERE IT IS NEITHER

The contract's site list named four flavour sites. **`PickedHomeForPresentation` had FOURTEEN
consumers**, and beyond prose it decides `ConfigureEndpoint`'s `_targetPicked`/`_targetOpponent` —
**the scoreline the ledger drives toward** — plus the stage's attack direction and colours, the
scorebug identity, the stats panel.

**The two functions have different types**, which is the whole problem: `PickedHomeForPresentation`
returns `bool` and always answers; `MatchModel.AnchorSide` returns `Side?` and admits NEITHER. Ten
sites need a binary. **A scoreline cannot be drawn toward "neither."**

| | |
|---|---|
| `AnchorSide` answers a side | use it EVERYWHERE — prose and geometry. This is what fixes `Handicap/Away` and `PlayerMultiScorer`, which the old table named HOME. |
| `AnchorSide` is NEITHER | prose goes club-free (`NeitherLine`); geometry keeps the HOME convention `ConfigureEndpoint` already documents. |

**The one-anchor invariant survives** (`ConfigureEndpoint`: *"this MUST be the exact same 'picked'
anchor the stage and every other renderer use"*) wherever an anchor EXISTS. The surfaces diverge only
where there is none — and there the prose declines to name a club, so it cannot contradict the
colours.

### THE SEQUENCING THIS SEAT PLANNED WAS WRONG, AND CHANGED MID-BUILD

The plan was prose first, geometry as its own diff. **Shipping prose alone would have put a
`Handicap/Away` leg in the exact contradiction the split exists to prevent** — the strip naming the
away club while the scorebug coloured home. Half of this rule is not a smaller step, it is an
incoherent one. Both halves landed together.

### THE GEOMETRY HALF IS ONE LINE, AND IT DELETES A DUPLICATE FIFTEEN-KIND TABLE

`PickedHomeForPresentation` is now `(MatchModel.AnchorSide(leg) ?? Side.Home) == Side.Home` — an
ADAPTER, not a table. All fourteen consumers keep their binary and pick up the real side wherever the
engine answers one. **The engine owns the rule; this surface owns the fallback.**

### TWO TESTS RE-BASED, BOTH STATED RATHER THAN QUIETLY ADJUSTED

- **`SweatFlavorDrawAnchorTests`** pinned *"the draw leg's flavour names the home club"* — the
  PRE-`T163` rule, which that class's own summary states outright. `T163` replaced it and the engine
  cites `T96`: **the draw is not a team, ever.** **What the test was written to stop, it still
  stops:** its defect was the AWAY club appearing, and now NO club appears, which forbids it by
  construction rather than by anchoring on the other one. Its geometry assertion survives untouched —
  annotated in place so it does not read as stale.
- **`SweatFlavorLeadChangeTests`** now passes an explicit `Side.Home`: its subject is the BASE
  TABLES, and a null anchor would send it down the club-free set where it would stop measuring what
  it is named for.

### ⚠ THE GREEN PROVES NO REGRESSION, NOT THAT THE CHANGE IS RIGHT

**This is the first step in the sequence that changes shipped copy on ORDINARY tickets.** The draw
leg has a direct fixture. **The other three cases on the engine's own disagreement list —
`Handicap/Away`, `TotalGoals/Over`, `PlayerMultiScorer/Yes` — have NO fixture that renders them.**
What is proven is that the adapter compiles into the ledger and a real sweat still runs green.

**OWED, and the corners docks CANNOT serve it:** `SweatFlavor.For` returns `CornerLine` early for
corners and cards, and those tables were ALREADY club-free — the anchor never reaches the strip on a
corners leg. The narrow ask is two legs: a Score/Momentum beat on a **totals or BTTS** leg, and one
on a **`Handicap/Away`** leg.

---

## 0-U16. STEP 4 PART C — THE DIRECTION RE-BASE · SHIPPED 2026-08-24

**EditMode 331/330/0/1 · PlayMode 149/124/0/25.** No case-count change — part C adds no tests, it
re-bases existing ones. All 55 cases across the four affected suites pass.

### THERE WERE TWO DIRECTION RULES, AND ONE OF THEM WAS ABOUT TO BE WRONG

`SweatPresentationModel`'s own summary said *"the shared rule — one authority."* It was not:
`SweatFlavor.For` derived its OWN `up` from a `_prevProb` that `TvSweatScreen` tracked separately.
Two copies can disagree, and after `T164` **they would have** — the model's is the TICKET's move, a
local recomputation off `WinProbAfter` is the ANCHOR LEG's. `For` now TAKES `up`; `_prevProb` and
`_flavorLegSeen` are deleted as dead. The reorder in `RenderEvent` (record, then flavour) is what
makes one authority true rather than merely claimed.

### THE PER-TELLING RE-ANCHOR IS GONE, AND ITS ABSENCE IS THE SUBSTANTIVE CHANGE

It existed because a new leg's `WinProbAfter` starts at that leg's own price, so differencing across
the seam compared two different legs' numbers. **The TICKET's probability has no such seam** — it
moves continuously across a fixture boundary, and a leg resolving IS a real move of it. The anchor is
now taken ONCE at `ResetForTicket(ticketProbAtStart)` and simply tracks. The seed must be a real
number, not the old `0.0`: with no re-anchor, a zero seed makes the first beat's delta the whole
probability.

### WHY THE PINS SURVIVED, AND WHAT EACH SUITE NEEDED

- **`SweatPresentationModelTests`** uses SINGLE-LEG tickets, where `T164` says in terms *"a one-leg
  ticket's win probability IS that leg's probability"* — so the assertions hold unchanged and only
  the referent is named honestly.
- **`ScoreLedgerTests` / `TheaterChoreographerTests`** drive `BuildTicketPaths` **one LEG at a time**,
  so they now `ResetForTicket(leg.TrueProb)` per leg to keep measuring each leg's own move. **That is
  also why their green does NOT exercise this change** — the explicit reset reproduces their old
  inputs deliberately.
- **`SweatFlavorLeadChangeTests`** got strictly clearer: it already had `up` in scope and was
  manufacturing probabilities to straddle an anchor purely to communicate it.

### THE BLAST RADIUS, MEASURED RATHER THAN ASSUMED

Outside `MagnitudeBand`, `delta` is used **only as a SIGN test** — the band-reconcile's
`delta >= 0.0` / `<= 0.0`, whose own comment says *"SIGN-COMPATIBILITY, not the tie-broken bool."*
The ticket's probability is a product of positive factors, so it is monotone in each: **while one
telling is live the sign of the ticket delta equals the sign of the moving leg's.** Score attribution
and band reconciliation are therefore untouched. Only MAGNITUDE compresses, and `T166` ruled
`MagnitudeBand`'s thresholds STAY.

**And `impliedLead` stays LEG-scoped** — it compares `probAfter` against the reconcile bands, and
`probAfter` is `evt.LegProbs[0]` by step 1's class-B split. **Step 1's split is what protects this:**
re-pointed to the ticket, a parlay's product would reconcile the scoreline wrongly on every
multi-leg ticket.

**One comment corrected, not deleted:** `SweatPresentationModel.cs`'s *"their |delta| ≥ 0.07 means
they are never actually flat"* was true of a LEG's move. The NUMBER is now false on a multi-leg
ticket; **the CONCLUSION survives** — a non-zero leg move times positive factors is still non-zero.
A stale number in a comment is trusted exactly as far as a fresh one.

### ⚠ WHAT THE GREEN DOES NOT COVER

**No fixture in either suite builds a same-match ticket**, so the case this re-base EXISTS for — two
legs on one telling wanting opposite things, a goal helping one and killing the other — **is
unproven by test.** The green establishes that sign is preserved and nothing regressed on the
shipping shape; the N-live behaviour rests on the monotonicity argument, not on a fixture. Pairs with
step 3's same-match counter gap; both want the `[A,B,A]` treatment once parts A/D can be built.

---

## 0-U15. STEP 4 — THE SHAPE THE TV NEEDS FROM THE ENGINE'S ANCHOR TABLE · 2026-08-24

**Allen ruled the ENGINE owns the backed-side table and the TV consumes it.** This is the consumer's
side of that contract, written here because the engine lane could not find it at HEAD — it existed
only in a seat report. **`T163`'s three branches and the §6b subject sites are BLOCKED on this.**

### ⚠ THE TV DOES NOT NEED `EventText.BackedSide`. IT NEEDS A SECOND FUNCTION.

`game-console/EventText.cs:138` already answers *which side did the player BACK*, exhaustive over
fifteen kinds, throwing rather than guessing (`K17-cl`, gated by `SweatAnchorGateTests`). **Shipping
that to the TV as-is would be wrong**, and its own doc comment says why:

> *"The player markets … this still answers NEITHER, because the question is which side he BACKED: a
> man can score in a 3–1 defeat and the leg wins, so his club is not the player's side. **The TV's
> `PickedHomeForPresentation` does anchor these on `PlayerSide` — that is the other question answered
> correctly for itself, and the divergence is the two shapes working as ruled.**"*

**Two questions, two answers, and `T163` needs the second.** The anchor controls which club the PROSE
names, not which side pays. `T163` branch (1) states its own compatibility test — *"this subsumes
today's single-leg case exactly, so nothing on screen changes before arm A lands"* — and today an
`AnytimeScorer` leg anchors on `PlayerSide`. **A table answering NEITHER there would change the
screen and break `T163`'s own claim.**

### THE SHAPE — `Side?` per LEG, and it must take a `Leg`, not a `MarketSelection`

Player markets need `Matchup.PlayerSide(PlayerIndex)`, which is not reachable from the selection.

| kind | anchor answer | why |
|---|---|---|
| `Moneyline` | Home / Away / **null** on Draw | the draw is not a team, ever (DD batch 49, `T96`) |
| `Handicap` | Home / Away | the line is applied TO THE BACKED SIDE; read `Choice` back |
| `DoubleChance` | `HomeOrDraw`→Home, `AwayOrDraw`→Away, `HomeOrAway`→**null** | the backed side is the ONE club in the union; 12 holds both, so neither |
| `TeamTotalGoals` / `Corners` / `Cards` | `Selection.Team` | it is a NAMED field; read it, do not decode it |
| `AnytimeScorer` / `PlayerMultiScorer` | **`Matchup.PlayerSide(PlayerIndex)`** | **THE DIVERGENCE.** The prose anchors on the club the man plays for. `BackedSide` answers null here and is right for its own question. |
| `TotalGoals`, `BothTeamsToScore`, `TotalCorners`, `TotalCards`, `CorrectScore`, `WinningMargin`, `TotalGoalsOddEven` | **null** | `T163` branch (3) names this set outright |
| a sixteenth kind | **THROW** | a `default:` that guesses a side IS `K17-cl`. Never a fallback. |

### WHAT THE TV COMPOSES ON TOP — `T163`'s fixture rule, not the engine's to build

Over the fixture's LIVE legs, collect the non-null answers: **all the same side → that side is
`picked`; two different sides → NEITHER; none at all → NEITHER.**

### WHY THIS IS NOT A REFACTOR — `PickedHomeForPresentation` IS WRONG TODAY, ON SHIPPING TICKETS

`SweatFlavor.cs:403` returns **HOME unconditionally for every kind except Moneyline and
AnytimeScorer**. So on a totals, BTTS, correct-score, corners, cards, margin, odd/even,
DoubleChance, team-total **or `PlayerMultiScorer`** leg, the flavour already names the home club as
`{picked}` when no side is backed at all — **on ordinary single-leg tickets, today.**

**Steps 1–3 were no-ops on the shipping shape. STEP 4 IS NOT.** It changes shipped copy on tickets
that exist now, so it needs evidence beyond a green suite. Check the docked `dd-import` sets for a
totals-leg sweat before asking for a capture window.

### WHAT LANDED AHEAD OF THE TABLE

`SweatFlavor.NeitherLine` + the four club-free tables (spec §5.2, twelve lines) and
`SweatFlavorNeitherBranchTests`. **Authored but NOT WIRED** — nothing calls `NeitherLine` until the
anchor rule exists. Pinned so the transcription cannot drift from the spec, which is the `K21`/`C60`
failure in miniature: rows that were ruled in batches 174–175 and unfindable in `docs/` because
transcription lagged from batch 154.

**§5.1's casing was left to this lane and is answered: lowercase with a terminal period**, this
file's own club-free convention (`"off the bar and away."`), NOT the casing of the table each line
joins — that rule is what split a branch two-capitalised/two-lowercase elsewhere.

### STILL OWED ON STEP 4

- **The direction re-base (part C)** — `SweatPresentationModel.RecordBeat` and `SweatFlavor.For` both
  compute direction off `evt.WinProbAfter`; the spec's §1 note says a single `up`/`down` exists ONLY
  because the displayed probability is the TICKET's. **Blast radius measured and it is small:**
  outside `MagnitudeBand`, `delta` is used only as a SIGN test (the band-reconcile's `delta >= 0.0`),
  and sign survives the re-base. `T166` rules the `MagnitudeBand` thresholds STAY.
  **Two cautions:** `RecordBeat` has four test callers and `SweatPresentationModelTests` PINS the
  current leg-scoped semantics, so that pin re-bases with it; and the comment at
  `SweatPresentationModel.cs:336` (*"their |delta| ≥ 0.07 means they are never actually flat"*) goes
  numerically FALSE under the re-base while its conclusion survives — a stale citation to fix, not a
  behaviour to preserve.
- **`impliedLead` must stay LEG-scoped.** It compares `probAfter` against the reconcile bands, and
  `probAfter` is `evt.LegProbs[0]` by step 1's class-B split. Re-point it to the ticket and a
  parlay's product reconciles the scoreline wrongly on every multi-leg ticket.

---

## 0-U14. THE TV's `onFinalLeg` TWIN — AND THE MUTATION THAT AUDITED THE GATE · 2026-08-24

**EditMode 325/324/0/1 · PlayMode 149/124/0/25.** +2 on step 3: the two twin gates.

### A SCOPE CORRECTION THIS SEAT OWES, because it ranked this wrong first

Step 2 reported `onFinalLeg` as *"a live defect, worse than its §6b placement suggests"* and this seat
ranked it the most urgent of the three. **Read properly it is NARROWER than the console twin.**
`TvSweatScreen`'s `onFinalLeg` feeds ONLY `PacingFor`'s final-telling slowdown, and it sits inside the
`_stage == null` branch — the theaterless fallback. On the shipping theater path `PlaySweat` hands off
to `TheaterBeat`, which owns pacing and never calls `PacingFor`.

**The console twin gated a stated RULE on the shipping path** (no fast-forward through the final
match). **This one loses pacing on a fallback path.** Both real; only one was reachable in a shipped
sweat. Fixed for correctness and symmetry, not urgency.

### THE FIX

`TvSweatScreen.OnFinalFixture(e, session)` — `e.FixtureIndex == session.FixtureCount - 1`. PUBLIC for
the same reason `SweatLines` is public in the console: the value is computed inside a coroutine that
sleeps, waits on seating and plays scenes, so a test that could only reach it by driving that loop is
a test that cannot run. Dead `lastLeg` removed.

### ⚠ THE MUTATION FOUND A HOLE IN THE GATE, NOT ONLY IN THE CODE — READ THIS ONE

The mutant killed the interleaved gate as intended. **It also PASSED the compatibility test**, which
is how that test was exposed as vacuous.

The ordinary-ticket fixture took the first two-leg ticket it could build. **That ticket busted on leg
0**, so the sweat never reached leg 1, so NEITHER predicate ever fired — `4 beats, 4 in agreement`
was four comparisons of `false == false`. It would have gone green against any predicate that never
fires, including the one it exists to rule out.

Armed: the fixture now searches for a ticket that reaches its final leg AND asserts the predicate
actually fires there. **4 beats → 10 beats, predicate true on 6.** The moved beat count IS the
evidence the search did something.

**A GATE THAT PASSED FIRST TIME IS UNAUDITED.** Two gates this seat added are proven able to fail
because they DID fail first — step 2's interleaved gate (its precondition) and the pool↔code pin (the
incomplete pool). The rest passed on their first run, which is exactly the state this one was in.
**Owed: mutate them.**

### THE PATTERN BEHIND THREE SEPARATE FAILURES TODAY

Step 2's gate failed its precondition (`LockRound` does not settle a ticket); the console gate's first
fixture busted at fixture 0; this one agreed vacuously for the same reason. **All three are one
shape: THE DRAMA ENDED BEFORE THE CASE UNDER TEST COULD OCCUR.** A fixture that does not survive far
enough is indistinguishable from a passing test. **Every sweat-driving fixture needs an explicit
"and it got that far" assertion**, not just "and it ran".

---

## 0-U13. STEP 3 — §6d / `T165`, THE COUNTER COUNTS MATCHES · SHIPPED 2026-08-24

**EditMode 323/322/0/1 · PlayMode 149/124/0/25.** EditMode is +2 on step 2: the width probe and the
pool↔code pin. Verified BY NAME.

### THE WORD WAS RULED ON MEASUREMENT THAT DECIDED NOTHING

`T165` left the form to TV — *"only measurement decides."* **It didn't.** All five candidates cleared
`T91-cl`'s 2px ink floor:

| form | ink | clearance |
|---|---|---|
| `LEG 4/4` (retired) | 66.9px | 86.8px |
| **`MATCH 4/4`** | **96.5px** | **57.2px** |
| `GAME 4/4` | 84.6px | 69.1px |
| `TELLING 4/4` | 108.2px | 45.6px |
| `FIXTURE 4/4` | 109.4px | 44.3px |

`Leg` is RIGHT-ALIGNED: its ink edge is pinned at x −233.0 and grows LEFTWARD to `TicketHeader`'s ink
at −386.7, so **~149.7px is available** and the widest candidate spends 109.4px.

**THIS LANE PREDICTED `MATCH` WOULD FAIL, AND WAS WRONG BY 55px** — it subtracted an ink WIDTH from a
CLEARANCE and called the difference headroom. `T144`'s lesson with the sign flipped: reasoning where
an instrument exists is the error, whichever way it lands. `T165-am` (batch 178) ruled `MATCH` on
vocabulary — it is already shipped copy here (`THE MATCH ENDS LEVEL`; the scoreline slot is
`Matchup`), where `GAME` appears in NO shipped copy and `FIXTURE`/`TELLING` are engine words.

### ⚠ THE SIXTH PHANTOM — AND IT IS THE OTHER KIND

`T158`'s own dichotomy: *"either the fixture is a phantom or the pool is incomplete — BOTH are
findings."* All five phantoms before this were the first kind. **This is the first of the second.**

The new pin caught it on its FIRST run: the code emits `MATCH 1/2` on a two-leg ticket; the pool held
only `4/4`, `1/4`, `1/1`. **The `LEG` forms it replaced had the IDENTICAL gap, since the slot was
created.** It could not surface because `T158` compares the measured fixture against the pool and
**nothing had ever compared the pool against the CODE.** Now enumerated in full — `m ∈ 1..MaxLegs`,
`n ∈ 1..m`, ten forms. Digits are tabular, so no measured number moved.

**The gate that found it was added in the same diff that created the risk.** That is the argument for
writing the pin with the change rather than after it.

### WHAT SHIPPED

- Both counter sites read `evt.FixtureIndex + 1` / `FixtureTotal()`; the denominator is
  `SweatSession.FixtureCount` — **the same grouping the joint price uses**, so the surface and the
  price cannot disagree about what a match is — falling back to leg count only with no session.
- `T165_the_counter_the_code_emits_is_in_the_pool`: drives the real `RenderPregame` and asserts the
  EMITTED string is pooled. **Closes the one edge of code↔pool↔instrument that `T158` cannot see.**
- The three rejected words are registered in `MeasuredCandidates` with their measured widths. That
  table's assertion is INVERTED — adopting one fails the pin rather than passing silently.
- `LEG 4/4` is out of the probe entirely: the surface can no longer emit it, so measuring it would
  itself be a phantom.

### OWED — AND THE GREEN DOES NOT COVER IT

- **NO TEST RENDERS THE COUNTER ON A SAME-MATCH TICKET.** Every fixture in both suites is ordinary,
  where fixture count equals leg count and `MATCH n/m` renders exactly what `LEG n/m` did —
  `evt.FixtureIndex` never diverges from `evt.LegIndex`. **The ruling exists for the shape no test
  builds.** Wants the `[A,B,A]` treatment step 2's gate got.
- **`onFinalLeg`'s TV twin is still unfixed** (`TvSweatScreen.cs:1718`) — same referent, same class,
  and the console half is now fixed while this one is not.

---

## 0-U12. STEP 2 — §6a, N GRADES AT ONE WHISTLE · SHIPPED 2026-08-24

**EditMode 321/320/0/1 · PlayMode 149/124/0/25.** EditMode is +1 on baseline: the interleaved gate.
Verified by NAME, not by a clean total — "0 failed" and "the gate ran" are different claims.

### THE ROTATION DOC'S PLANNED FIX WAS UNSAFE, AND IT WOULD HAVE REOPENED `T144`

The plan said `_resolvedThrough = evt.LegIndex + 1` becomes `max(LegIndices) + 1`. **It must not.**

`JointModel.GroupByMatchup` (`:1352`) is a plain first-appearance partition; `BetslipModel` appends
picks in tap order with no sort (`_picks.Add`); `Run.PlaceTicket` builds legs in pick order. So
**a fixture's legs need not be CONTIGUOUS**: `[matchA, matchB, matchA]` gives fixture 0 = legs
**{0, 2}**, and it is told FIRST. `max + 1` is 3 there, which marks leg 1 — a leg whose match has not
been told at all — as presented-resolved. `revealedLoss` (`:3018`) then reads leg 1's raw
`GradesWon` and the footer announces the death before its scene plays. **The `T144` leak, arriving
through the remedy rather than through omission.**

**No scalar can express "0 and 2 resolved, 1 untold."** That is why the fix is a per-leg set.

### WHAT SHIPPED

- `_resolvedThrough` (int) → `bool[] _presentedResolved`, with bounds-safe `IsPresentedResolved(i)`.
  Four readers re-pointed, three writers now mark every index in `evt.LegIndices`.
- `UpdateTicketColumn(int liveLegIndex)` → `UpdateTicketColumn(IReadOnlyList<int> liveLegs)`;
  `_liveLegIndexShown` → `_liveLegsShown`. **A self-aliasing guard was required** — three call sites
  pass the cache field itself, so an unconditional clear-then-copy empties the set being re-asserted.
- The two `+ 1` "next leg reads LIVE" calls become **the next FIXTURE's legs**, via one
  first-appearance grouping helper mirroring `GroupByMatchup`. Duplicated in presentation only
  because that helper is `internal` to the engine — **if the two ever disagree, the sweat's idea of
  a fixture and the joint price's are two implementations of one rule**, which the contract forbids.
  Worth routing: making `GroupByMatchup` public would delete this copy.
- **`FinalSlam`'s `grade` is the ANCHOR's, and the legs on one fixture can grade DIFFERENTLY.**
  Looping `LegIndices` with that one grade would be a silent lie on a mixed fixture. Each leg's
  `ResolveLeg` now derives its own outcome; the ceremony branches keep the passed-in grade so the
  beat is unchanged.
- `SweatPresentationModel._anchorLeg` → `_anchorFixture`, keyed on `evt.FixtureIndex`. **Its
  probability arithmetic is untouched** — that is step 4's, and `T166` has ruled the `MagnitudeBand`
  thresholds STAY (the tape's quietening on multi-leg tickets is TRUE, not a defect to compensate).

### THE GATE, AND THE VACUOUS PASS IT ALMOST WAS

`A_leg_whose_fixture_has_not_been_told_is_never_rendered_resolved_or_leaked_to_the_footer`.
It renders the column with presented-resolved `[true, false, true]` — **a set no high-water mark can
produce** — and asserts leg 1 shows no verdict and the footer does not read its settled `STAKE` form.

**Its first run FAILED on its own precondition, and that failure is the point.** `LockRound` does NOT
settle the ticket — the bust happens when `SweatSession.MoveNext` delivers the `LegFinal`, which is
the very race the reveal gate exists for. With `State` never `Lost`,
`settledDead = State == Lost && revealedLoss` **cannot be true for ANY implementation**, so the
footer assertion would have gone green against a leaking scalar. The fixture now drains the session
(declining every save), refuses any candidate that does not settle `Lost`, and voids legs 0/2 AFTER
the drain so engine truth and presentation genuinely disagree. **A gate that cannot fail is worse
than a missing gate** — write the precondition that proves it is armed.

### ⚠ THE REFLECTED-SEAM TRAP BIT AGAIN, SAME SHAPE, DIFFERENT SUITE

Step 1 broke two PlayMode tests; step 2 broke `TvSweatScreenPaletteTests.cs:1226-1228`, which
reflects `_resolvedThrough` by string and invokes `UpdateTicketColumn` with an `int`. **Both
compiled clean in all three assemblies.** The lesson is now written into that helper's doc comment:
**before changing any `internal` signature on `TvSweatScreen`/`RevealedView`, grep `Assets/**` for
the member name as a STRING LITERAL.** The scalar-parameter shim was kept so all six existing call
sites stay verbatim; a test needing a non-contiguous set uses `RenderTicketColumnSet` instead.

### THREE DEFECTS FOUND AND DELIBERATELY NOT FIXED

- **`onFinalLeg` NEVER FIRES on an interleaved ticket.** `TvSweatScreen.cs:1718` —
  `evt.LegIndex == _ticket.Legs.Count - 1`. On `[A,B,A]` the anchors are only 0 and 1, never 2, so
  `PacingFor(evt, onFinalLeg)` **loses final-leg pacing entirely.** Honest referent is
  `evt.FixtureIndex == _session.FixtureCount - 1`. The contract files this under §6b/step 4; it is
  worse than that placement suggests and should be pulled forward.
- **`_stageLeg` configures the ledger from the ANCHOR only** (`BeginStageLeg` →
  `ConfigureEndpoint`). On a shared telling carrying two markets on one match, the non-anchor leg has
  no ledger behind its progress line.
- **The pending-loss window is still scalar.** This surface reads `HasPendingLoss` at four sites and
  never `PendingDeadLegIndices`, so `NoSingleCallSaves` and `S85`'s "state it BEFORE the offer" are
  unbuilt. That is the other N-live site and it is the largest one left.

### OWED

- **`int k = evt.LegIndex + 1`** feeds `LEG k — VOIDED, THE TICKET LIVES`. What a shared telling's
  copy calls itself is **NOT RULED**; inventing a form here would be this lane deciding it. Left as
  the anchor's number, commented. Owed to the DD with the `SweatRenderer` twin.
- **`game-console/SweatRenderer.cs` carries the same per-leg drive loop** (§6g) and the same
  contiguity assumption. Routed to the DD for the next markets seating.

---

## 0-U11. STEP 1 — §6c, THE PROBABILITY SITES · SHIPPED 2026-08-24

**EditMode 320/319/0/1 · PlayMode 149/124/0/25 — both at baseline.** Engine gate green at
`324/1/0` before the change and NOT re-run after: step 1 touched zero files under `engine/`, and
EditMode's `DeterminismEditModeTests.Golden_seed_event_stream_is_pinned` is the Unity-side mirror
of the same pin.

### THE SHAPE THAT MATTERS: §6c'S SITE LIST MIXES TWO QUANTITIES

The contract lists six sites under *"displays a LEG's probability."* They are not one thing, and
building them as one would have been wrong:

- **CLASS A — the displayed number → `SweatSession.TicketWinProbability`.** `RevealedView.Reset`'s
  seed (the site `T164` names), the pregame and ticket-card seeds, the `_pendingProb` stash, the
  theaterless branch.
- **CLASS B — the picked side's per-match prob → `evt.LegProbs[0]`.** `_stage.SetLiveProb` and the
  three `TheaterChoreographer` sites. `TheaterStage._prob` is *"picked side's last revealed win prob
  — territory truth"* (`TheaterStage.cs:83`), driving `PitchLayout.TerritoryX` and the possession
  share. **Pointing territory at a parlay's product would pin the pitch to one end for the whole
  sweat.** The contract's own rubric for the choreographer applies verbatim; `SetLiveProb` is the
  same animal and belongs here, not in class A.

**CLASS B SHIPS ZERO BEHAVIOUR CHANGE AND THAT IS PROVABLE.** `DramaEvent.cs:54-58` materialises
`LegProbs => _legProbs ?? new[] { WinProbAfter }`, and the shared-telling ctor
(`DramaGenerator.cs:243`) passes the anchor's prob as `WinProbAfter` with `legProbs[0]` the same
anchor. **`evt.LegProbs[0] ≡ evt.WinProbAfter` on every event, shared or not.** Class B is a
statement of intent that becomes load-bearing at step 2.

### ONE SITE THE CONTRACT DOES NOT NAME, AND IT WAS WRONG

`FinalSlam` snapped `_probTarget` to `1f` on a won leg. Under a ticket-level number **one leg winning
does not make the ticket certain** — that was announcing a certainty the ticket does not have, mid-
ticket. Now reads the session. The engine already lands exactly `1.0` when every leg is won and
exactly `0.0` on a revealed dead leg with no save held (`engine.tests/TicketWinProbabilityTests`), so
the terminal values are unchanged where they were already right. **The `Won`/`Lost` guard shape is
load-bearing** — VOID must fall through and keep its pre-kill number.

### THE DEFECT THIS SEAT PUT IN AND CAUGHT — TIMING, NOT VALUE

Re-pointing the crowd-tension bed looked like a one-line no-op. It was not. The bed read
`RevealedView.WinProbability`, which lands **at the reveal**; a referent set in `RenderEvent` moves
when the beat is CONSUMED. **The crowd would have swelled before the pitch showed the story** — and
on a dangerous scene the mirror's number is pinned to hold (`LaptopOsTests`, *"the reveal owns it"*)
while the bed would not. An audible tell, M-T3.1 exactly.

Fixed by stashing (`_pendingTensionProb`) and landing at the same instant `_probTarget` lands. **The
referent now moves on exactly the mirror's six seams and no others** — `Reset`, both `Clear`s,
`FinalSlam`, the theaterless branch, `RevealBeatChrome`. Verify by grepping both lists and diffing
them; they must stay one-to-one. `TheaterStage`'s territory keeps its own pre-existing early timing
(scene playback supersedes it) and is deliberately NOT folded in.

### ⚠ THE TRAP WORTH INHERITING: A REFLECTED SEAM IS INVISIBLE TO THE COMPILER

Adding a parameter to `RevealedView.Reset` compiled clean in **both** assemblies and then failed two
PlayMode cases at runtime with `TargetParameterCountException`.
`SureThingMyBetsTests.cs:37` and `SureThingVisualCaptureTests.cs:201` reach that seam through
`InvokeView(view, "Reset", …)` — reflection by method-name string.

**Before changing ANY `internal` signature on `RevealedView`, grep `Assets/**` for the method name as
a STRING LITERAL, not just as a call.** `grep -rn '"Reset"'` finds in one second what a green build
will not tell you. Exactly two reflected callers exist today; both now pass the ticket product via a
documented `TicketProbAtStart` helper. **This is the same family as the phantom fixtures: a green
compile proving something it never checked.**

### OWED FROM THIS STEP

- **`T164-cl` may be wrong**, and it changes what step 1 owed. Nothing in `Assets/**` renders this
  float — `_probTarget` is *"data-only … no standalone win% visual"*, `SportsbookApp`/`LaptopOs` never
  read `WinProbability`, and `SweatFlavor.cs:50` calls it *"the deleted win-prob numeral."* Its only
  runtime consumer was the tension bed. Routed:
  `docs/5-orchestration/route-t164-visibility-2026-08-24.md`. **The RULING is untouched either way.**
- **`T163-am` does NOT come for free with `T164`** — same route doc, finding 2. The direction that
  selects the flavour table is computed at `SweatPresentationModel.cs:56-64` and `SweatFlavor.cs:25`,
  both off `evt.WinProbAfter`, neither of them the displayed number. **Step 4 must build that
  re-base.** `T166` (batch 173) has since ruled the `MagnitudeBand` thresholds STAY: the tape's
  quietening on multi-leg tickets is true, not a defect to compensate.
- **The tension referent is a lane decision, not a ruling.** Recorded in the route doc.

---

## 0-ROT. SEAT ROTATION 2026-08-19 — READ THIS FIRST

Everything below `0-ROT` is the record of what shipped. **This section is what a fresh seat needs
that the record does not already say.** Both charter units and unit 3 are complete and merged; the
lane is idle with nothing half-built and no dirty tree beyond the two permanent-`M` files
(`SBR.Engine.dll`, `URP.png` — never stage either) and the untracked `unity/SBR/artifacts/`.

> **SINCE THIS ROTATION (see `0-U9`):** `T144`'s height gate was run and **FAILED** — the two-row
> footer is short 4.6px against the live row's pinned margin; the composition is HELD and routed.
> **The stale two-legs clause below is STRUCK** (`2f76062`). A **PlayMode red on main** was found and
> proven inherited from `e8cb38e`.

### THE FOUR LIVE CALLS — all with the DD, none of them this lane's to decide

1. **`T133`'s ROOT COLLISION.** `T133` is **closed on width and only on width.** Measured on the real
   slot, box 249.0px: `PAYS $73,318,376,502` 239.7px (9.3px spare) · **`RETURNED $73,318,376,502`
   300.9px — OVERRUNS by 51.9px** · **`PAID $73,318,376,502` 235.8px — fits with 13.2px, MORE room
   than the incumbent.** Frames shot `S99`-style at
   `dd-import/t133-pays-rung2-2026-08-19/`. **The remaining objection is batch 108's: `PAID` collides
   at the root with `PAY $60` on the same screen. That is copy and the DD holds it.**
   ⚠ **Until it is ruled, `RETURNED` ships and the cashed-out worst case overruns.** The DEAD case is
   safe (`RETURNED $0`, 102.5px spare); **the exposure is the cashed-out case alone.**
2. **`T140`'s cost** — estimated, routed at `docs/5-orchestration/route-t140-cost-2026-08-19.md`.
   **LARGE, multi-phase, and NOT executable by this lane alone: it needs `SweatSession` and
   `DramaGenerator` changes, and `engine/**` is READ-ONLY here — an edit is an escalation.** Two spec
   gaps must close before any build: **whose probability the cash-out prices off with N legs live**
   (`_liveProb` is one scalar), and **what the pending-loss window does with N simultaneous finals.**
3. **The two `T129` pairings** — scene-vs-strip, and a goal riding a **showing count scene** vs one
   riding a **quieted beat**. The build treats the second pair identically and may not deserve to.
4. **`T127`** — whether the ending's territory hold should hold, settle or clear. The `T129` frames
   are the material; they deliberately do not make the call.

### A DEFECT FOUND WHILE COSTING `T140`, AND IT IS LOAD-BEARING

**STRUCK 2026-08-19 (`2f76062`) — the record of why is kept here.** **`TvSweatScreen.cs:2858` stated *"the engine forbids two legs on one matchup"*. THAT IS STALE.**
The sgp lane shipped same-game parlays (F_0.6.0 — engine, gates, conditional cash-out), and
`JointModel` explicitly models *"two legs on one match plus a third elsewhere"* with a `SameMatch`
block. **The comment is the stated justification for "at most one row is ever live", so the column's
own reasoning now rests on something untrue.** Routed with the `T140` cost. The predecessor did not strike it (the BUILD it describes is the
same work `T140` prices); **this seat struck the CLAUSE only** — the comment no longer
asserts something false, and the per-fixture restructure it points at is still unbuilt and
still T140's.

### WHERE A FRAME SET DOCKS — corrected 2026-08-20

**The harness WRITES to `unity/SBR/artifacts/tv-sweat-capture/` (untracked, `dataPath`-anchored).
THE DOCK IS SOMEWHERE ELSE: `docs/design/dd-import/<set>-<date>/`** — a README, a
`FRAME-INVENTORY-all-<n>.txt` listing every frame, and a CURATED few PNGs, not the whole set. This
seat left the T147 README beside the artifacts and Allen built the dd-import dir from it. **Writing
a set is not docking it.**

### THE INSTRUMENTS THIS SEAT BUILT — use them, do not rebuild them

- **`SBR/TV/T84 candidate measure (pre-authoring)`** — measures strings that are NOT yet in the
  product, against the real slot's real box and face. **Measure-before-you-author has now been a
  precondition on `T112`, `T114-am` and `T133`.** It is separate from the sweep on purpose: the
  sweep's pools may hold only strings the code can already emit, and a candidate is by definition one
  it cannot.
- **`TvSweatScreen.ForcePaysTextForCapture` + the `FORCED-` filename discipline** — `S99`-style
  forcing for states that cannot be dealt for. **A forced frame that does not disclose its forcing is
  evidence for a state the product does not have.**
- **`engine.tests/NearLineSeedSearch.cs`** and **`CalmBeatReachabilityProbe.cs`** — pure-engine
  searches that need NO editor lease. **Reach for these before requesting a window.**

### FIVE TRAPS THAT COST THIS SEAT TIME

1. **A wait-for-editor loop must CLEAR a stale lockfile at zero processes, never wait on it.** The
   `-quit` segfault leaves one behind; `while (procs OR lockfile)` then spins forever, **Unity is
   never launched and no log is written to explain it.** Cost a window slot.
2. **Shoot DETACHED and poll on artifact mtime.** A foreground wait died with the tool call twice and
   took the editor with it, truncating a set that then had to be re-shot.
3. **Removing a string means removing it from the sweep IN THE SAME CHANGE.** This seat removed the
   flavour suffix from the code, left it in the pool, and the next sweep dutifully reported a 94.8px
   overrun **on a string the surface could no longer emit** — `T111-am`'s own finding, committed by
   the lane that had just ruled on it.
4. **`PlaceTicket` refuses an unoffered selection at runtime** (`"Market selection is not offered"`).
   A fixture cannot be re-pointed to an invented line — that is the engine enforcing
   never-invent-a-selection, and it rejected this seat once.
5. **A per-leg accessor silently changes subject mid-sweat.** `_countLedger` is replaced when the
   next leg goes live, so `DebugRevealedCountHome/Away` stop reading the leg you think they read. A
   pin built on it watched the CARDS leg while asserting about corners. Read the RETAINED row, or use
   a one-count-leg fixture.

### THE OWED QUEUE — nothing blocking, nothing started

- **CARDS** — untouched and out of scope by §6; distance-to-line is the wrong instrument for a
  booking and **no cards arm has ever been shot.**
- **§3.5's two UNDER cells** — authored, in the pool, **unreachable** until the under mirror is gated.
- **`T129`'s §8 item 3 successor questions** — a non-goalless draw (1–1 / 2–2) was **deliberately not
  requested**: it is a question about GENERALITY, and generality was not what was missing.

---

## 0-U10. `T147-am`/`am2` — THE COMPOSITION BUILT, SHOT, AND C55-CERTIFIED · 2026-08-20

**SHIPPED `83d8072` (build) + `8e02d13` (captures).** The re-ruling paid for itself out of two row
slots the engine can never fill.

| | |
|---|---|
| gate | ran FIRST, composition ABSENT from the tree — pitch 99.0 vs a live row needing 66.8, **CLEARS by 32.2px** |
| built | `TicketRowSlots` 6→4, `TicketFooterHeight` 40→60, both money rows left-anchored at the full 249.0px |
| verified | `column 480.0 = header 24.0 + 4 x pitch 99.0 + footer 60.0` · **BUILT MATCHES RULED** |
| suites | engine 307/0/1 · EditMode 316/315/0/1 · PlayMode 147/123/0/24 |
| frames | 30, five bursts, `README-t147.md` docked beside them |

### THE ALIGNMENT ARM IS ANSWERED ON MEASUREMENT — and it is not close

Row 2's box spans canvas-local **`-482..-233`**; the canvas spans **`-490..490`**. At the fact floor:

| arm | `RETURNED $73,318,376,502` ink | verdict |
|---|---|---|
| **left/left (shipped)** | `-482.0 .. -181.1` | over its own box by **51.9px**, spilling RIGHTWARD into the neighbouring zone — **survives the mask** |
| **right-anchored (`T147-am2`'s counter-arm)** | `-533.9 .. -233.0` | **43.9px CLIPPED off the left by the `RectMask2D`** — the opening characters are destroyed |

**Left/left overruns visibly; right/right destroys characters.** The DD asked for this read on the
SETTLED state exactly because `RISK`/`PAYS` are four characters each and align either way, while
`STAKE`/`RETURNED` are five and eight. **It does not close `T133`** — the 51.9px overrun is
untouched by the composition and the word is still the DD's.

### THREE STALE READERS IN ONE DAY, ALL THE SAME SHAPE, TWO OF THEM MINE

1. **The footer reader.** Both instruments inferred footer height as `riskPays box + 8` — true only
   while the footer held ONE row. After the split it reported **38.0 for a 60px footer**, a NEGATIVE
   derived header, and a "ruled" pitch BELOW the built one. **The suite was green throughout**,
   because nothing asserts on a report. Now read from row 1's top to row 2's bottom.
2. **The ink reporter.** It assumed LEFT alignment and computed `boxLeft + inkW` for every case, so
   on the right-anchored arm it printed *"ink survives the mask"* for a string the mask cuts. **That
   frame set was deleted and re-shot rather than kept.**
3. **The gate's own fixture** inherited `T24`'s `MARCUS VALE TO SCORE` — retired by `T69`;
   `SweatActiveLegModel.cs:551` emits `{Surname} TO SCORE`. It read as a 39.3px overrun that cannot
   occur. Re-pointed at `ONE TEAM BLANKED` (252.5, matching the record) and **log labels bound to
   the measured constants** so name and value cannot drift apart again.

> **THE LESSON, AND IT IS THE DAY'S:** a stale READER is the same defect as a stale COMMENT, and
> nothing catches it, because a report is not an assertion. **`T24`'s own pin still carries the
> phantom string** — its assertion is height-only so nothing is wrong with it, but the fixture
> measures something the product cannot emit. Not fixed here; it is another pin's evidence.

### C55 ON THIS SURFACE — both axes, and why

Asserted before EVERY burst, in the canvas's LOCAL space: both money rows plus the live row's own
two lines. **The laptop's helper judges vertically only** — correct there, where the horizontal term
produced false negatives on a scrolling list. **Here nothing scrolls and the change moved things
BOTH ways** (footer 40→60, pitch 69.3→99.0), so a vertical-only verdict would not have been testing
this change at all.

### READS OWED TO THE DD, off the docked frames

- **Does the 99.0px row read SPARSE** for 58.8px of live ink? The ruling already names the next rung
  if so: a fixed pitch with the remainder dark below the last leg.
- **The canon THREE-line live row now CLEARS by 13.5px** at this pitch (`T24` cut it for want of
  room at 70px). Reported by the gate; restoring it is a ruling.
- **The footer's 8px top inset is SPENT** — 60px holds exactly two 30.0px line boxes. 68 is
  affordable (rows would go 99→97) if a frame says the money rows crowd leg row 4.

---

## 0-U9. `T144` — THE TICKET FOOTER'S TWO ROWS · GATE RUN, GATE FAILED, COMPOSITION HELD · 2026-08-19

**Ordered by Allen:** build `spec-ticket-footer-2026-08-19.md` (`T144` takes `T74-am3`'s separate
rows), height re-derived per §3.3 and gated by §4, evidence to include `E3`. **The gate did not
clear and §4 says the gate passes BEFORE the composition lands, so the composition is NOT built.**
Routed at `docs/5-orchestration/route-t144-height-gate-2026-08-19.md`.

**SHIPPED (`2f76062`):** the gate instrument, and the stale clause struck.

| | |
|---|---|
| `T144` gate | `TvSweatScreenLayoutGridTests.T144_the_two_row_footer_height_is_re_derived_against_the_live_row` — report-only, rides EditMode |
| the strike | `TvSweatScreen.cs:2858`'s "the engine forbids two legs on one matchup" — struck, with why |

### THE NUMBERS — and the ceiling is the one to carry

Column budget **480.0** = header 24.0 + six rows + footer. Line box at `TypeRisk` 24 is **30.0px,
ratio 1.25 measured** (§4.4 satisfied; 1.18 would have predicted a fit again). Live row ink
**58.8** (NEED 35.0 + progress 23.8); with `T24`'s pinned 8px margin, **66.8**.

> **THE FOOTER'S CEILING IS 55.4px.** Two rows need 60.0 bare and 68.0 as the footer is actually
> built (8px top inset). **SHORT by 4.6px and 12.6px.** Per row that is **0.8px off `T24`'s 8.0px
> margin**, six rows.

**As type:** the two rows fit the ceiling at **~22.2px** with zero top inset, **~19px** with any
inset. §5 reserves type for the DD, so neither was TV's to take.

### `RETURNED` DOES NOT FIT ITS OWN ROW — §2's full-width claim is word-dependent

`RISK $13,639` 138.4 · `STAKE $13,639` 158.9 · `PAYS $…502` 239.7 · `PAID $…502` 235.8 — all fit
249.0. **`RETURNED $…502` is 300.9 — OVERRUNS by 51.9.** Separate rows fixes the PAIR; it does not
save the word that ships in the settled state. **`T133`'s copy ruling binds this composition too**,
not just the one it replaces. Four of the five widths reproduce the record exactly — the
instrument agrees with the sweep.

### §3.1's CITATION IS INVERTED — the ruling stands, the reason does not

§3.1 cites the money control as a left/left precedent via `:5468`'s *"Anchors are left exactly as
they were."* **That means left UNCHANGED, not left-ALIGNED:** `CashOut` is `MiddleLeft`
(`:5459-5462`), `CashOutStatus` is `MiddleRight` (`:5471-5474`). The money control is the
precedent for KEEPING opposite anchors across a move to separate rows. **The ruling is unaffected**
— §3.1's real argument (no shared gap, so the device has no subject) stands alone — and TV will
build left/left as ruled. Routed so the register does not carry a backwards precedent.

### ⚠ AN INHERITED PLAYMODE RED, ON MAIN, PROVEN NOT THIS SEAT'S

```
FAIL SBR.Tests.PlayMode.TvSweatScreenTests
     .TicketFooterWord_NeverDisagreesWithAnyRow_AndNoLiveRowEverPrintsNeedZero
     "leg N shows the NEXT chip but the footer reads 'STAKE'" — expected RISK
```

**`StakeWord` landed in `e8cb38e`** (the three ruled string builds — the settled footer, `T121`);
**the pin that catches it predates it** (`acd9d9f` / `4e45464`). `0-U8` records that build's SWEEP
and **no suite numbers** — the full-suite rule was not discharged on it, and this is the cost.

**Attribution was MEASURED, not argued:** both this seat's files were stashed, the tree recompiled,
and the pin re-run filtered — **1 of 1 executed, still failed.**

**DIAGNOSED to source on Allen's order** — `docs/5-orchestration/route-settled-ticket-rows-2026-08-19.md`.
**Two sources of truth, and the session stops between them.** The footer's settled branch reads the
ENGINE's `_ticket.State` (`:3011-3012`); the rows read the SURFACE's reveal cursor `_resolvedThrough`.
Both engine settle paths set the state AND `_complete = true` in the same breath
(`SweatSession.cs:252-253` bust, `:503-508` cash out), and `:136-140` then emits no further drama
events — so **the remaining legs are never resolved on the surface**, fall to `UpdateTicketColumn`'s
final `else`, and print **NEXT** while the footer correctly says the position is closed.

**A STEADY STATE, NOT A RACE.** The bust is instant on the first losing leg (`SweatSession.cs:185`)
and `DemoTicketPolicy` deals 2-3 legs, so most non-winning tickets end there and STAY there. That is
why the pin trips at frame 16 / frame 51 of hundreds: the fast-forward settles the sweat in a few
sampled frames and every frame after settlement fails.

**THE ROWS ARE WRONG, NOT THE FOOTER** — by `T121`'s own principle. And **§8.10 already has the
vocabulary, gated on the wrong flag:** a pending leg takes the VOID strike (never the LOST
extinguish), but only while `_cashOutPreview` is true. **The surface marks the leg cancelled while
the player is deciding, and un-marks it once it actually is** — and never strikes it at all on a
bust. `T121` justified reading `_ticket.State` because a cash-out is a player action leg outcomes
cannot see; **right for `CashedOut`, extended to `Lost`, which is derivable and reveal-timed.**

**BUILT AND VERIFIED 2026-08-19 (`11e4ad7`), on Allen's order — remedy 1 PLUS the leak it hid.**

**Remedy 1:** the settled fact is read ONCE and hoisted; `isLive` takes `!ticketSettled` so the leg
after the loser stops rendering a NEED on a ticket that cannot pay; a pending leg's chip falls
silent and takes §8.10's strike permanently instead of only while previewing. **The line drawn:**
`NEXT` is TRUE while he is deciding (he can decline and the leg plays on) and FALSE once the ticket
settles — so the preview keeps the word and strikes it; the settle takes the word away. A fully WON
ticket stays `Open` in the engine, so its rows are untouched.

### ⚠ THE LEAK WAS REAL, AND REMEDY 1 ALONE WOULD HAVE SHIPPED IT GREEN

**`SweatSession.MoveNext` resolves a `LegFinal` and busts BEFORE it hands the event back**
(`SweatSession.cs:150-154`, `:184-185`), while `_resolvedThrough` advances only in `FinalSlam`,
after the whole final scene has played. **Three of ten repaint sites land in that gap** —
`RenderEvent` (called straight off `MoveNext`), `RepaintRevealedScore` (stoppage-time goals during
the final scene) and `ExitCashOutPreview` (polled every frame). A footer reading raw `_ticket.State`
prints `STAKE` / `RETURNED $0` **during the scene that kills him.**

> **AND REMEDY 1 PROPAGATED IT INTO THE ROWS.** `ticketSettled` read `_ticket.State`, so the rows
> went silent in the same gap. **The existing pin then passed** — it compares footer to ROWS, and
> settling the rows early makes both agree while both are still early. **A green suite, still
> telling the ending.** This is the sharpest example this lane has of a consistency pin certifying
> a correctness defect.

**THE FIX IS AT THE SOURCE OF TRUTH, NOT AT THE CALL SITES.** `settledDead` is now REVEAL-GATED off
the same test the resolved row renders its `L` chip from, so footer and rows cannot disagree by
construction and **all three sites close at once**. `CashedOut` is deliberately NOT gated: a player
action has no hidden outcome behind it and settles synchronously (`T114`'s own argument).

**THE NEW PIN IS PROVEN ARMED** — assertion 3, the reveal gate, compares the footer to the REVEAL
rather than to the rows. **With the gate removed it fails at frame 23 of a real sweat; with it in,
silent.** That probe is also what turned the leak from a source-read into a MEASUREMENT.

### SUITES — this tree, this window

| suite | measured | against baseline |
|---|---|---|
| engine | **307 passed / 0 failed / 1 skipped** (308) | +1, growth |
| EditMode | **314 executed / 313 passed / 0 failed / 1 ignored** | +59 since batch 96; +1 is `T144` |
| PlayMode | **146 executed / 122 passed / 1 FAILED / 23 skipped** | the 1 was the inherited red above |
| PlayMode *(after `11e4ad7`)* | **146 executed / 123 passed / 0 failed / 23 skipped** | red cleared; +1 is the reveal-gate assertion |

### TRAPS THIS WINDOW ADDED

1. **The suite runs dirty FIVE side-effect files, not three.** Three TMP font SDF assets
   (`EncodeSans Bold`, `EncodeSansCondensed Bold`, `EncodeSansCondensed`), the TMP fallback, and
   `ProjectSettings.asset` — the dynamic atlas populates on any newly-measured glyph run, so a
   measurement instrument dirties them by existing. `git checkout --` all five before staging.
2. **`float.PositiveInfinity` is the wrong "unconstrained" for TMP.** `GetPreferredValues` folds
   the constraint into its layout maths and returns infinities. The codebase's own constant is
   `100000f`, in four places. A bounded agent caught this one against the lead's instruction.
3. **`-quit` segfaulted again** (`0xC0000005`) with a clean compile — procs 0, stale lockfile,
   cleared. The known fault, on the shutdown path only; the compile and the results were valid.

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

### `T108-am2`'s ROUTED ITEM — closed, and it found a hole in the pool above

Batch 102 routed one item into `T111`: every count arm formats `{total} {NOUN}` with
the noun a fixed literal and **no arity branch anywhere in the file**, so a revealed
total of one renders **`1 GOALS`**.

**It was missing from the pool this lane had just shipped.** The first cut instantiated
totals at 0, 3, 5, 10, 16, 24 and 40 — every edge it could think of except one, and
`1` is the one that carries a grammar defect rather than a width one. **The DD found it
by source read while this seat was reporting the pool complete.** Added and re-swept.

**The measurement says it is NOT a width problem.** With the singular forms enumerated,
`LegRowProgress0`'s widest is unchanged — `CLEAN-SHEET PATH LIVE` at 191.4px, **57.6px
spare**, and no new overrun anywhere. So the sweep's contribution here is to make the
form *visible*, not to condemn it: **whether `1 GOALS` is acceptable copy is grammar and
the DD holds it.**

**The corners singular is kept even though this seed cannot reach it** — its batch
deltas were 2 throughout, and a step of 2 can jump the line without landing on it.
**Reachability is a property of the generator, not of one capture**, and a pool sized to
one seed's deltas is `S84`'s failure in miniature.

> **The reusable half: an edge-case pool is only as good as the edges someone thought
> of, and "every edge I could think of" is not an enumeration.** The first cut was
> built by reading the source and still missed the case the source makes trivial —
> `{total}` with `total = 1`.

---

## 0-U8. THE THREE RULED STRING BUILDS · SHIPPED 2026-08-19 (`e8cb38e`, merged)

Built directly on Allen's **authorised deviation from the delegation contract** after the bundled
dispatch stalled; the agent was **wound down first** so it could not land conflicting edits.

| ruling | built |
|---|---|
| `T110-am2` | the flavour suffix `" ({n} in the spell)"` **removed outright** |
| `T114-am` + `T112-am` | banner **drops its amount** — bare `CASHED OUT`, at acceptance *and* in the held preview |
| `T121` | the settled footer — **`STAKE $x` / `RETURNED $x`**, on both the dead and cashed-out cases |

**THE SWEEP PRICES THEM; THE LANE DOES NOT ASSERT THEM:**

```
Flavor    577.2px / 651.0px   fits, 73.8px spare   T110-am2 + T110-am closed
CashOut   221.5px / 241.0px   fits, 19.5px spare   T112-am confirmed
Pays      300.9px / 249.0px   OVERRUNS by 51.9px   T133 — LIVE, routed
overrunning: 1 of 22 (was 2)
```

**`C46` is now ALSO discharged for §3.5's four decisive lines** — they sit in the same `Flavor` pool,
which measures clean. That had been outstanding since phase C.

### THE THINGS WORTH CARRYING

1. **`T110-am2` is NOT a width fix and must not be summarised as one.** Width is the **fifth and
   least** of its five reasons; the first four are that *spell* is never explained, it misreads as a
   running total, the count is already shown in the column (*drawn, not captioned*), and the widest
   string said `spell` **twice** one clause apart.
2. **`T112-am` needed no separate fix at all.** Batch 108 had already ruled the banner drops its
   amount, on independent grounds and **four hours before** the overrun was routed past it.
3. **`T133` IS LIVE AND WAS NOT DODGED.** `RETURNED` is **eight characters where `PAYS` is four**, on
   the one slot whose worst case was established by enumeration over 648,000 priced offers. **The
   dead case is safe** (always `$0` — 146.5px, 102.5px spare); **the cashed-out case is not bounded
   to `$0` and overruns by 51.9px.** Shortening the word is a **copy** decision and `C11` puts copy on
   a frame — recorded at the site, routed, not worked around.
4. **A DEFECT OF THIS SEAT'S OWN, CAUGHT BY THE INSTRUMENT ONE RUN LATER.** The first re-sweep still
   reported `Flavor`'s 94.8px overrun because the suffix was removed from the **code** and left in
   the **pool**. **A pool that outlives its strings measures a phantom** — `T111-am`'s own finding,
   committed by this seat. **Whenever a string is removed, remove it from the sweep in the same
   change.**

---

## 0-U7. `T129` — THE DRAWN ENDING'S SECOND HALF · THREE ARMS SHOT AND DOCKED 2026-08-19

**Merged `f7d55ca`.** Dock: `dd-import/drawn-ending-t129-2026-08-19/` — 608 frames, **one seed
(`GOALLESS-5`), one matchup, one stake across all three arms** so they read against each other as
well as against the docked set.

### THE TWO FINDINGS THAT MATTER

**1. `T125`'s gap is confirmed, and the old set could not have shown the tally AT ALL.**
The draw-backer's tally starts at **f068** and last changes at **f127**, then settles.
**The old window was 60 frames.** Not a fraction of the tally was in it. Arm 3 closes condition (e)
most cleanly: `+$4` → **`+$256`**, which is exactly the ticket's own `PAYS $256` — it changes, then
settles, inside the window.

**2. `T128`'s carried question is ANSWERED, identically in all three arms, at 51 frames.**

| | f001 – f051 | f052 → |
|---|---|---|
| arms 1, 3 | `RISK $25` · a live NEED · chip `''` | **`STAKE $25`** · NEED cleared · chip `W` |
| arm 2 | `RISK $25` · `UNDER 1.5 GOALS` / `0 GOALS • LIMIT 1` | `RISK $25` · cleared · chip `W` |

**From f001 the screen already reads `0 — 0`, `FT` and `THE MATCH ENDS LEVEL`** — the facts that
decided every one of these legs — while the column prints a live requirement and a live risk beside
them for **1.02 sim-seconds**.

> **`T108`'s fix WORKS on a drawn ending; it lands one second late.** On corners material the stale
> form passes through IN FLIGHT. On a drawn ending it **sits still at full time, where the player is
> looking.** That is why `T128` asked for it here rather than taking the corners verification.

### ARM 2's OWN FINDING — correct, and the multi-leg form of `T128`

Its footer **never reaches `STAKE`** across 150 frames, and a live cash-out **offer** still stands at
f088 on a settled 0–0. **That is `T108` clause 2 working exactly as ruled** — `RISK` is a TICKET word
and leg 1 is unrevealed inside the window. **And** the match is over, both legs will win, and the
surface still offers to buy the ticket back. **Named, not ruled.**

### NEW TERRITORY

**Arm 3 is the first capture of `CorrectScore` that has ever existed** (no reachable home until
`S95`). `PAYS $256` on `$25`, **no progress line at all** — at full time the column is a statement
and a price and nothing else.

### NOT CLAIMED
**Nothing about whether the ending READS.** `T127` recorded that the hold's only motion is the pitch
and deliberately did not rule whether the territory view should hold, settle or clear. **These frames
are the material for that call and do not make it.**

---

## 0-U6. UNIT 3 PHASE C — THE REVEAL AND THE DECISIVE POOL · SHIPPED AND DOCKED 2026-08-18

**Merged:** `d10a6f2` (§2, the A-reveal) · `817066a` (§3.5, the disjoint pool + the frame set).
**Both clauses land on one set** — `dd-import/corners-reveal-and-decisive-2026-08-18`,
same seed and line as the original before-state.

| | before | after |
|---|---|---|
| scorebug | `0 — 0` held to `90'+1` | **goal at 22'** |
| approach, 43' | corner #1's line, verbatim | **`one short. the ledger is holding its breath.`** |
| crossing, 53' | corner #2's line, verbatim | **`that clears it. the line is beaten.`** |

At the reveal window the token is `CornerFor` and the strip is a possession line: **the scorebug
moved and nothing else did**, which is §2 literally.

**Suites:** EditMode **304 / 303 / 0 failed / 1** · PlayMode **142 / 122 / 0 failed / 20**.
**All three invariants hold TOGETHER** — `[COUNT-COMMIT] 11=11` (§4), `[QUIET-COUNT-GATE]` (§3),
`[SCORE-REVEAL-GATE]` (§2). That co-existence was the real risk: each phase could have broken the
one before it, which is why the pins were built in that order.

### THE THINGS A LATER SEAT NEEDS

1. **"The true clock" does not mean what it sounds like.** `MatchStatLine` carries only FINAL
   totals — **the engine records no goal times at all** — and the TV's clock advances toward each
   beat's baked minute. §2 means the corners arm reveals on **the same beat schedule the goals arm
   already uses**, which the count branch was pre-empting. **Do not go looking for a goal minute.**
2. **§2 is §4's binding applied to goals**, and is built as Phase A's mirror — a staged goal commits
   when the beat plays, independent of any payoff callback. **Do not add a second architecture
   beside it.**
3. **Significance rides `SceneSpec` as a NULLABLE and the null is load-bearing** — it means
   UNCLASSIFIED, not `Ordinary`. A count scene also reaches the screen from an **ungated** beat
   (cards, Under, a `Score`-typed beat, a whole-number line) which is genuinely ordinary. **A bool
   would hand an ordinary corner the decisive copy** — §3.5's own defect from the opposite
   direction.
4. **Phase B's goal suppression is REPLACED, not deleted.** It existed only while §2 was unbuilt and
   conditional.

### OWED, AND NOT THIS LANE'S TO DECIDE

- **`C46` is NOT discharged for the four new strings.** They are enumerated in the sweep's `Flavor`
  pool; **the sweep has not been re-run since.** Enumerated, not measured.
- **Two of §3.5's four cells are authored but UNREACHABLE** — `APPROACH·UNDER` / `TURN·UNDER`.
  `gateEligible` hard-requires `countHelps`, and §6 keeps the under mirror out of scope.
- **TWO PAIRINGS, to be ruled TOGETHER not separately** — (1) does a count-leg goal reach the SCENE,
  and the STRIP? (2) a goal riding a **showing count scene** vs one riding a **quieted beat**. This
  build treats (2) identically in both cases, and §2's *"departure from calm"* bites hardest on the
  quieted one, where nothing else carries attention.

### AN OPERATIONAL FIX WORTH CARRYING

**A wait-for-editor loop must CLEAR a stale lockfile at zero processes, never wait on it.** The
`-quit` segfault fault leaves one behind, and `while (procs OR lockfile)` then spins forever: Unity
is never launched, **no log is written to explain it**, and the call burns to its timeout. Cost one
window slot here.

---

## 0-U5. UNIT 3 — THE THEATER BUILD · PHASES A + B SHIPPED AND DOCKED 2026-08-18

**State:** phase A `acd9d9f`, phase B `4a06b52`, the evidence dock `f88b00f` — **all three
merged to `main` and pushed.** Phase C (§3.5's disjoint pool, §2's reveal) not started.

**The result, on frames:** before, `CornerFor` on scenes 002–015 — fourteen consecutive
windows, one token. After, the arm departs from calm and returns to it:

```
corner01 CalmPossession  2      corner05 CornerFor  10  ← the turn
corner02 CalmPossession  4      corner06 CalmPossession 11
corner03 CornerFor       6      corner07 CalmPossession 12
corner04 CornerFor       8  ← the approach
```

**§8.1's six criteria:** 1 **PASS** (the count still reaches 12 — the only correctness
check) · 2 **PASS on strip, 4-of-5 on scene** · 3 **PASS on strip, partial on scene** ·
4 **PASS** · 5 **N/A** (§2 not built) · 6 **recorded, and it corrects the register** —
predicted ~5.5s/~12.5%, **measured 4.58s / 10.3%**.

Full read: `dd-import/corners-sweat-after-2026-08-18/README.md`. **Frames untracked, and now
structurally so** — a local `*.png` guard was added because `dd-import`'s own `.gitignore`
covers only `*.zip`, which is the gap behind the 487MB incident.

**Suites at ship, full and unfiltered, post-merge:** EditMode **303 / 302 / 0 failed / 1** ·
PlayMode **137 / 119 / 0 failed / 18**.

### THE THREE THINGS A LATER SEAT NEEDS FROM THIS UNIT

1. **`T117` cost nothing, and the reason is structural.** A quieted batch rides `QuietCount`,
   **never `Count`** — so `countScene` is false and `T97-am`'s existing override already routes
   the strip to `NeutralLine`. **Do not "wire up" the strip; it is already correct, and the
   field separation is what makes it so.**
2. **`T118`'s second door is closed UPSTREAM and the two remedies are NOT interchangeable.**
   A quieted beat stages no goal at all, so the `OnGoalPlayed` payoff is unreachable. The
   amendment's cheaper call-site fix closes the count loss but **leaves the goal**, and a
   quieted corner would then reveal a goal on a corners ticket — the §2 coupling §3 must not
   acquire.
3. **ROUTED, one line wide: widen the gate from `Momentum`-only to all beat types?** §3.1 keys
   on **distance**, not on having arrived, and the original objection (a `Score` beat can stage
   a goal) is **spent** now that goal-suppression covers every type. That single restriction is
   the whole 4-vs-5 divergence in criterion 2.

### OWED, AND NOT THIS LANE'S TO DECIDE

- **§8 item 3, the near-line watch** — a leg that lands close to its line, or loses. **Every
  frame we hold is a comfortable winner and the ramp's whole value is in the case never shot.**
  The DD's read decides whether it precedes the close.
- **Phase C** — §3.5's disjoint decisive-beat pool (structure buildable, contents owed) and §2's
  reveal, to be built **independent** of §3 per the spec's own instruction.

### NEWLY UNBLOCKED BY BATCH 108 — queue, not this window

- **`T114-am`** — the footer and banner authored **as one job**: `STAKE $87` / `RETURNED $199`,
  the banner shedding its amount. ⚠ **`_tPays`'s box has never been measured** — the sweep
  measured the other component. **Measure before authoring**, as `T112` required.
- **`T110-am2`** — the flavour suffix **REMOVED**, with the overrun deliberately the least of
  four reasons. Still present on events 4 and 5 of the docked set, which predates it.

---

## 0-U5-PRE. UNIT 3 — the groundwork, recorded before the build

Spec: `docs/design/spec-count-theater-2026-08-17.md` (FINAL, canon `fc5a1f5`).

### THE PHASING, and why §4 goes first

| phase | contents |
|---|---|
| **A** | **§4 — the quiet-corner commit path.** The prerequisite. |
| **B** | §3.1–3.4 — the distance gate. |
| **C** | §3.5's disjointness structure (pool owed) + §2 the reveal. |

**§4 is first because §3 is unsafe without it.** `StageBeat()` advances its cursor
unconditionally while `CompleteCount` fires from the scene's payoff, so the moment a beat
can decline a batch, it consumes a count without committing one — *"the column stops
tracking and the match ends short of its own total."* **Phase A is deliberately a NO-OP on
today's behaviour**: nothing yet declines a batch, so every suite must stay at its exact
numbers. The invariant lands before the change that can break it.

### §2 AND §3 ARE BUILT INDEPENDENT, BY INSTRUCTION

The DD flagged its own reading of Allen's *"stay personalized"*: it discriminates against
option B but **not between A and C**, and *"stay" is a preservation word* reading toward
C — no change at all. So the spec says: *"If Allen meant C, delete §2 and build §3
unchanged… **Do not let §3 acquire a dependency on §2 during the build** — that is the one
thing that would make the flag expensive."* Honoured: they are two independent changes.

### SIGNIFICANCE IS COMPUTABLE WITH NO LEAK

`distance = threshold − (revealed Home + Away + this batch's delta)`, with `threshold`
derived from `leg.Selection.Line` — a **betting-time** fact — and the counts from the
revealed ledger. **No path touches `TargetHome`/`TargetAway`**, so §3.3's *reads the
REVEALED count, never the locked target* holds by construction, exactly as `T108` does.

**Distance is measured where the beat LEAVES the count, not where it found it.** The
grammar doc's own table reads distance after each event (43', total 8, *"one short — THE
APPROACH"*), so the staged batch is included before the comparison.

### THE THRESHOLD — A SPEC GAP, ROUTED AND BUILT ONE EDIT WIDE

§3.1 says **"a ramp, not a switch"**; §7's gate 1 asserts events **"below the significance
threshold"** produce no count scene. **The two pull opposite ways and no value is named.**

Routed to the DD. Built meanwhile on Allen's word — **the explicit cases the spec does
name**: the approach, the turn, and *"a resolved leg's corners have no distance to any
line, so they earn nothing."* The distance sits in a **named constant, not a literal**, so
the ramp is one edit wide. **A silent constant must not stand in for a ruling.**

Consistent with the measured shape: 7 count events, 2 weighted, 5 going quiet — inside
`T113`'s "up to six of eight" ceiling.

### WHY THE CORNERS ARM SHOWED `0 — 0` — the mechanism, found at source, and it is NOT a reveal rule

**Established reading `SweatPresentationModel.cs` and `TheaterChoreographer.cs`:**

1. `ConfigureEndpoint(leg)` sets `_goalSense = 0` for corners/cards — *"Corners/cards legs
   keep the neutral home-anchored goal decoration."* So a corners leg does **not** take the
   market early-return; it takes the **moneyline** goal path, which stages on
   `Score`/`BigPlay` **and** on probability reconciliation *"regardless of its type."*
2. **But `ResolveBeat`'s count branch RETURNS before `ledger.StageBeatGoal(...)` is ever
   called.**
3. `T113` measured that **every non-final beat on that arm staged a non-zero count**, so
   every one took the count branch.
4. **Therefore goal staging was never reached on a single non-final beat**, and `PlanFinal`
   reconciled the whole match at the whistle.

**That is the `0 — 0` for 86% and the two-step result at `90'+1`/`90'+2`, exactly.**

> **AND IT REFINES §2's STATED COST.** The spec describes the fix as the ledger's rule
> changing shape — *from "reveal what the ticket rides on" to "the score is always true"* —
> and calls that *"a rule changing shape, not a tweak."* **At source the corners arm's
> goals are not withheld by a reveal RULE at all: they are never STAGED, because the count
> branch pre-empts the call.** The reveal rule and the pre-emption are two different
> mechanisms and only the second is what these frames measured. **Routed — it may make §2
> cheaper or differently shaped than costed, and that is the DD's to judge, not this
> lane's.**

**It also made a correction to this lane's own Phase B brief load-bearing.** The brief
justified confining the gate to `Momentum` beats on the ground that *a `Momentum` beat
cannot stage a goal* — **true only on the `_goalSense != 0` path, which corners legs do not
take.** Momentum beats are precisely the ones the reconciliation branch can fire on. So
without a further rule, **quieting a corner would have revealed goals on a corners ticket —
delivering part of §2 by accident, through §3, which the spec forbids in terms.**

**The rule added: a quieted count beat must not stage a goal.** Quieting is a scene
decision, never a score decision; a quieted beat leaves the revealed scoreline exactly where
it found it. `StageBeatGoal` is a pure read (the mutation is in `CompleteGoal`), so
suppressing the call is side-effect free.

### AN UNSTATED CONSEQUENCE — GOING QUIET ALSO SHORTENS THE WATCH

`cornerSeconds` 4.5 against `calmSeconds` 3.0, × `paceMultiplier` 0.75 = **1.125 sim-s
reclaimed per quieted beat.** At ~5 quieted that is **~5.6s off a 44.42s sweat, ~12.5%.**

Probably the right direction — the evidence had corners at 41.42s against the goals
control's 35.40s, so quieting the arm also converges its **duration** on the control. But
**the spec does not mention pacing**, and it will be visible on the re-shot pair §8 asks
for. **Flagged before the frames, not discovered after them.** Routed to the DD.

---

## 0-U4. THE RULED QUEUE — batches 103-106 · 2026-08-17

### `T112` — the cash-out constant · MEASURED, then BUILT

**Measured before authoring, because `T112` made that a precondition** (*"very probably is
the word this studio has been wrong on twice this week"*):

```
CashOut 'MARKET SUSPENDED'  267.7px against box 241.0px  OVERRUNS by 26.7px
CashOut 'SUSPENDED'         152.3px against box 241.0px  fits, 88.7px spare
```

The DD's named candidate clears with **88.7px spare**. Re-authored, box unchanged, and
the state is still **stated** rather than carried by grey.

**A new instrument came out of it: `SBR/TV/T84 candidate measure (pre-authoring)`.**
Measure-before-you-author has now been a precondition on two consecutive rulings, and it
could not use the sweep — the sweep's pools may contain only strings the code can already
emit, and **a candidate is by definition one it cannot emit yet.** Mixing them would put
an invented string in the population, which is the exact defect `T111-am` ruled on. So it
is a separate entry point that measures candidates against the real slot's real box and
real face, and **rules nothing.**

### `T110-am` — the flavour strip · MEASURED, and the measurement REMOVED THE BUILD

The ruling made one measurement owed first and said it *"may remove the work entirely"*.
It did:

```
Flavor box 651.0px
  WITHOUT suffix: widest 577.2px  fits, 73.8px spare
  WITH suffix:    widest 745.8px  OVERRUNS by 94.8px
  suffix cost:    168.7px between the two widest
```

**The base decks FIT, with 73.8px to spare. The suffix alone causes the overrun** — it
costs 168.7px, nearly twice the overrun itself.

**So no deck is re-authored, and the four rung-2 fallbacks the remedy would have needed
are not written.** Per the ruling: *"authoring four fallbacks before taking it is the
predict-instead-of-measure error this lane exists to prevent."*

**AND THE REMEDY IS NOW A COPY DECISION THIS LANE MAY NOT TAKE.** With the decks
exonerated, everything that overruns does so because of `" ({n} in the spell)"` — and the
suffix's fate is explicitly **not ruled** (`C11` authors copy on a frame; the DD's lean is
on record that it goes). **The overrun stays live and the fix is with the DD.** This lane
built nothing here on purpose.

*(The DD also noted the widest string prints `spell` twice — `…in the spell — …(12 in the
spell)` — `T69`/`T70`'s family inside a single string. Also copy, also not this lane's.)*

**BUILT — and the fix was necessary but NOT SUFFICIENT. `CashOut` STILL OVERRUNS.**

With `MARKET SUSPENDED` retired, the sweep's next run reports the slot over on a
different member:

```
CashOut  box 241.0px  widest 'CASHED OUT $1,240'  255.6px  OVERRUNS by 14.6px
```

**It was always over. It was hidden behind a bigger overrun** — the sweep reports the
widest per slot, and 267.7 masked 255.6. This is the instrument's own recorded pattern
turned around: *an over-generated string suppresses the under-generation it sits on top
of*, and here a genuinely-reachable over-long string suppressed a second one.

**NOT FIXED, ROUTED.** `CASHED OUT $x` is authored copy (`T35`, `T68-am` — it prints in
the slot at L3), so shortening it is a copy decision and `C11` puts that on a frame. **The
lane measured it, refused to author it, and routes it** — the same disposition as `T112`
itself before its candidate was measured.

> **The reusable half: a per-slot sweep that reports only the widest can only ever find
> ONE defect per box.** Clearing the widest is what makes the next one visible, so a slot
> that has just been fixed is exactly the slot most worth re-sweeping — which is why this
> was found at all.

### `C53` — the sweep's third category · BUILT, all four clauses, verified on a run

```
POPULATION: 60 text slots exist · 22 swept · 25 the same construction at another row
            index · 1 declared unswept · 12 delegated to a named other instrument ·
            0 unaccounted for
DELEGATED — COVERED: 12 slots covered by
  SBR.Tests.PlayMode.TvSweatScreenTests.Evidence_C46_the_stats_panel_strings_against_their_boxes
  (evidence 2026-08-17: 142 strings over these 12 slots, no overrun, tightest 22.5px)
  — StatsA0 … StatsTitle
```

- **Clause 1** — `DELEGATED` exists and names its instrument.
- **Clause 2** — the name is resolved by reflection across
  `AppDomain.CurrentDomain.GetAssemblies()` (the Editor assembly does not reference the
  PlayMode test assembly, so `Type.GetType` alone cannot find it), and an unresolved name
  **logs an error AND throws**, so the run fails both visibly and non-zero. Renaming or
  deleting that pin now breaks the sweep loudly.
- **Clause 3** — `UNACCOUNTED FOR` is **genuinely 0**, not absorbed: delegated slots are
  excluded from `uncovered` by classification, and report on their **own** line as a
  positive statement with count, list and instrument.
- **Clause 4, the trap** — `DelegationStatus { Covered, Scheduled }` with an evidence
  date. `Covered` = has run and been docked. `Scheduled` prints
  **"NOT YET RUN, not docked — do not read this as measured"**. Without this the panel
  would have reported `COVERED` while `T101`'s residual was, in this same window,
  *never run and never docked* — the identical lie one level up, inside the fix for it.

### ROUTED, not this lane's

- **`docs/design/tv-design.md` §6.1 names `MARKET SUSPENDED`.** DD's owning document; a
  canon update is owed. Not opened.
- **`SportsbookApp.cs:2107`** prints `TV REVEAL IN PROGRESS · MARKET SUSPENDED` on the
  **laptop's** mirror — a different surface and a different box, and the charter says the
  laptop is not this lane's. Unmeasured here. **`S88`'s cousin.**
- **Two sibling TV instruments carried the retired string in their own pools** —
  `TvTypeParityProbe.cs` (×2) and `TvPromptComposition.cs`. **Fixed here rather than
  routed**, because leaving them would have recreated `T111-am`'s exact defect —
  *a measurement instrument whose pool outlives its string measures a phantom* — in two
  more places, on the same day it was ruled.

### `T114` — ARRIVED, RULED, AND **NOT BUILDABLE YET**

A cashed-out ticket's footer prints `RISK $87` · `PAYS $1,490` above `CASHED OUT $199` —
both false, held across all thirty frames. `T108`'s principle verbatim.

**The DD recorded the omission as its own:** the spec named
`RevealedTicketState { Riding, Won, Lost, CashedOut }` and tabled three of its four
members. *Naming a state is not ruling it* — the same shape as `T108-am`'s *naming a field
is not reading it*, one week apart.

**And it names why this lane's build cannot express it, correctly:** `StakeWord` takes
`IReadOnlyList<RevealedLegOutcome>`, and **a cash-out is a player action not derivable
from leg outcomes at all.** Not a flaw in the build — the consequence of a table with a
missing row.

**WHY IT IS NOT BUILT HERE: the strings are not authored.** The row says the exact strings
are copy, that `C11` authors copy on a frame, and that the frame now exists so it is *no
longer blocked* — but **"no longer blocked" is not "authored".** §5's standing rule binds:
*until it lands a lane must not invent the string.* **Owed from the DD: the fourth row's
strings.** The wiring gap is known and small once they land.

*(Routed with it and also not built: the cancelled legs still read `NEXT` after a
cash-out, and `T25.6` defines `NEXT` as "the next thing that can take his money". The DD
did not judge the strike's presence — only that the word is legible and false.)*

---

## 0-U3. THE §8 CALM-BEAT CHECK — SETTLED · 2026-08-17

**The question** (`grammar-count-markets-2026-08-17.md` §8, the one item blocking that
direction from becoming a spec): *are `Momentum` beats tagged `Calm` actually SCHEDULED
during a corners sweat?* If the stream never emits them, widening the count gate would
yield `Territory` or `Fallback` — not calm.

### THE ANSWER: YES, and it is most of the leg

**Measured on the docked capture's own seed `CORNERS-SWEAT-1`, both arms, one variable:**

| | corners | goals (control) |
|---|---|---|
| `Momentum` / `Calm` | **6** | **6** |
| `Score` / `Swing` | 1 | 1 |
| `LegFinal` / `Decisive` | 1 | 1 |
| **total beats** | **8** | **8** |

**Six of eight beats on the corners leg are `Momentum` + `Calm`** — the tag that maps to
`CalmPossession`. The branch is not merely reachable; it is the majority of the leg.

### AND THE TWO ARMS ARE BEAT-FOR-BEAT IDENTICAL — which sharpens the finding

Same seed, **same matchup**, `OVER 8.5 CORNERS` against `OVER 1.5 GOALS`, both `Won`.
The type/tag distribution is not merely similar, it is **the same in every cell**, and
the per-beat probabilities track within a few thousandths.

> **So the difference between the two watches is not in the drama stream at all. It is
> entirely presentation routing.** `count-sweat-read` §2 concluded *the corners arm has
> no resting state*, which is correct — and the mechanism is now exact: **six beats were
> tagged `Calm` by the stream and rendered `CornerFor` by the routing.** All seven
> non-final beats staged a count, so every one took the count branch.

> **CORRECTED BY `T113`, and the correction changes the fix.** This section first read
> *"generated six times and overwritten six times"*. **That is loose and the DD was right
> to catch it: the count branch returns BEFORE the base table runs, so `CalmPossession`
> is never constructed at all. Nothing is overwritten — the calm branch is never
> reached.** The distinction matters because "overwritten" implies a scene exists to
> suppress, and the actual fix is about a branch that never executes.

**`T113` ruled the finding** and put it at its strongest: *"count markets are simply less
eventful" is not merely unproven — it is refuted on a matched pair.*

**This is `grammar-count-markets` §1 confirmed by measurement rather than by source read,
and it quantifies that direction's own proposal: up to six of eight beats in this sweat
are calm beats already being spent.**

### `Territory` AND `Fallback` — the two outcomes §8 feared, checked

- **`Territory` never occurs in either arm.** Zero `Momentum` beats carried a non-`Calm`
  tag. Structurally it needs a `LeadChange` — a sub-`0.07` step that crosses `0.5` —
  because `Swing` requires `|Δp| ≥ 0.10` and `Momentum` requires `|Δp| < 0.07`, so
  **`Swing` is impossible on a `Momentum` beat** and `NearMiss`/`LegFinal` both return
  before the scene table.
- **`Fallback` is unreachable today.** It is the `_ =>` arm for future `DramaEventType`
  additions; all four current members are handled above it.

**So §8's worry inverts: `CalmPossession` is the DEFAULT for a momentum beat and
`Territory` is the narrow case, not the other way round.**

### WHY FRAMES COULD NOT HAVE ANSWERED THIS

The capture's filenames carry the **scene** token. The count branch returns *before* the
`(Type, Tag)` scene table, so a `Calm`-tagged beat and a `Swing`-tagged one both render
`CornerFor`. **The token stream is blind to exactly the distinction §8 asks about** — the
evidence had to come from the beat stream, which is pure engine and deterministic.

### NOT CLAIMED

- **One seed, one line, one side, both arms `Won`.** A leg that lands near its line, or
  loses, has a different probability path and is not in evidence.
- **This does not model the count branch.** It answers only whether the stream *schedules*
  the beat. **A second gate exists and is untouched here:** even with calm beats
  scheduled, `ResolveBeat` intercepts any beat coinciding with a nonzero staged count —
  which is precisely what the proposed change would alter, and what §5's
  consume-without-committing binding is about.
- **No frame was read.** Nothing here is a claim about how anything looks.

**Instrument:** `engine.tests/CalmBeatReachabilityProbe.cs`, `[Fact(Skip=…)]` — opt-in,
so the engine baseline stays **306 passed / 0 failed** (307 total, 1 skipped). It reads
`Type` and `Tag` straight off each emitted `DramaEvent` and **never recomputes them from
the thresholds** — recomputing would prove only that the rule can be reimplemented.

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
