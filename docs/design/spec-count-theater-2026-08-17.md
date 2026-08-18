# SPEC — the theater for count markets (Phase 2, FINAL)

**Written:** Design Director seat, 2026-08-17 · **Authority:** Allen's calls relayed 2026-08-17,
landed as `T109-cl` and `T115` · **Evidence:** `count-sweat-read-2026-08-16.md` ·
`grammar-count-markets-2026-08-17.md` · `T113` (the calm-beat probe) · **Surface:** TV — match theater

**Two clauses, and they are deliberately separable.** §2 is the reveal; §3 is the grammar. **The
grammar does not depend on the reveal** (`T113` §9), so a change to §2 changes nothing below it.

---

## 1. WHAT WAS WRONG, MEASURED ON A MATCHED PAIR

Same seed, same fixture, same pacing, same predicates, same stake, **identical final scoreline**, one
variable — `OVER 8.5 CORNERS` against `OVER 1.5 GOALS`.

- **The corners arm had MORE events and FEWER dead stretches** (7 against 3; 9 against 11) **and it is
  the one that watches flat. Event scarcity is not the cause.**
- **The two arms received beat-for-beat IDENTICAL drama, probabilities within thousandths** (`T113`).
  **The stream is innocent. The flatness was entirely presentation routing.**
- **Up to six of eight beats were calm beats the count branch spent on corners carrying no tension.**
- Against the 8.5 line, distance ran **7 → 5 → 3 → 1 → crossed → decided → decided**, and **all seven
  corners got one treatment.**
- **The corners player was shown `0 — 0` for 86% of a match that finished 5–1**, then handed the
  result in two steps at the death.

## 2. CLAUSE ONE — THE REVEAL (`T109-cl`) · RULED, CLOSED

**RULED — ALLEN, FINAL, 2026-08-18: THE REVEAL IS A. The revealed scoreline is never withheld. Goals
reveal on the true clock whether or not the ticket rides on them.**

**Confirmed three times** — his *"stay personalized"* word, then twice against a staged draft. **This
clause is no longer conditional and no longer carries a flag; build it.**

**Everything else stays ticket-keyed** — the stats panel's rows, player detail, and the flavour
strip's subject continue to follow the ticket. **Personalization is preserved as the governing
principle; the score is carved out of it because a scoreline is the match's primary fact and the
surface asserts it rather than withholding it.**

**What it fixes:** the false `0 — 0`, the two-step result at `90'+1`/`90'+2`, and the fact that the
arm with a resting state was also the only arm that got to see a goal.

**What it costs, stated plainly:** the ledger's rule stops being *"reveal what the ticket rides on"*
and becomes *"the score is always true, the rest follows the ticket."* **A rule changing shape, not a
tweak** — every reader of the revealed ledger inherits it.

> **CLOSED 2026-08-18.** This spec carried a flag here: *"stay personalized"* discriminated cleanly
> against option B but not between A and C, and *stay* is a preservation word. **Allen has ruled A,
> final. The flag is discharged and nothing in this spec is conditional on it.**
>
> **What the flag built is KEPT, because it was good architecture and not only insurance: §3 does not
> read from §2, and it must not acquire a dependency on it during the build.** The reveal and the
> grammar are separately true, separately testable, and separately reversible. **That property was
> worth having before the answer and is still worth having after it.**

**And it compounds with §3 rather than overlapping it:** a goal the corners player does not need is
exactly the **departure from calm** his watch is missing. §2 supplies contour from outside the count
grammar; §3 stops spending contour inside it. **Neither substitutes for the other.**

## 3. CLAUSE TWO — THE GRAMMAR (`T115`)

### 3.1 The rule

**An event earns its treatment from its DISTANCE TO THE LINE, not from having arrived.**

**A ramp, not a switch.** The market's whole tension is a continuous quantity — this is the cricket
required-run-rate steal landing on the **scene grammar** rather than on a line of text.

### 3.2 The mechanism, found at source

`TheaterChoreographer` takes the count branch **first**: on a corners/cards leg, any non-`LegFinal`
beat that stages **`TotalDelta > 0`** returns a `CornerFor`/`CornerAgainst` scene and **returns**,
short-circuiting the only table that can produce `CalmPossession`.

**So calm does not lose a competition — it is never reached.** The beats were **tagged `Calm` by the
stream and rendered `CornerFor` by the routing.**

**The change is to GATE THE COUNT BRANCH'S ENTRY**, not to compute both and prefer calm. Stated
because the probe's own wording (*"overwritten"*) invites the more expensive edit.

### 3.3 Where the significance comes from — nothing is invented

**The ticket column already computes it.** `SweatActiveLegModel` derives `threshold − total` from
**revealed** values and printed `8 CORNERS • NEED 1` at 48'. **The theater asks the question the
column already answers.**

**It reads the REVEALED count, never the locked target** — `T108`'s standard, and the no-leak law in
that file already enforces the provenance.

### 3.4 What the rule buys — three findings, one change

1. **The resting state returns.** It is **not authored** — it is what remains when buildup stops being
   spent, out of scenes that already exist and already play.
2. **The approach and the turn become the only weighted moments**, which is what makes them read as
   moments at all.
3. **The corpse stretch ends as a consequence.** A resolved leg's corners have **no distance to any
   line**, so the ~20 seconds of post-win narration stops without its own fix.

### 3.5 The strings — the two decisive beats may not take a recycled line

**Measured, and it is the worst possible assignment:** of seven count events, **the approach (43')
printed the line from corner #1 — the least consequential event of the match — verbatim**, and **the
crossing (53'), the moment the bet was won, printed the line from corner #2.**

**RULED: the approach and the turn draw from an authored pool that ordinary count events cannot
reach.** A ramp in treatment with a flat string pool would still narrate the win with a line the
player has already read twice.

**The words themselves are COPY and `C11` authors copy on a frame — they are not written here.** What
is ruled is that the pool is **disjoint**, so recycling onto a decisive beat is unconstructible rather
than unlikely (`T108` clause 1's standard).

### 3.6 THE RAMP — ruled, and the threshold is 2 rather than 1

**Added 2026-08-17 (`T115-am`), answering tv-theater unit 3.**

#### The shape: TWO RUNGS, not a gradient

**Weighted or quiet. There is no third treatment and no smooth curve.**

**Correcting this seat's own phrasing:** `T115` says *"a ramp, not a switch"*, and that invites a
gradient. **It should not.** The **ramp is in the significance FUNCTION** — distance is continuous.
**The TREATMENT is stepped.**

**Why, from the evidence rather than from taste:** the measured failure was **no contour**, and seven
slightly-different treatments is seven flavours of one thing — which is the same flatness with more
machinery. **The control arm's contour was `calm → event → calm`: three departures from a baseline
read where seven departures from nothing did not.** Two weighted moments against five quiet ones is a
stronger contour than a gradient across all seven.

#### The threshold: **2**, and this is not a preference

The spec named distance 1. **Checked against the three corner lines the generator actually produces,
on the observed delta pattern (five events of +2, then +1):**

| line | approach @1 | approach @2 | turn |
|---|---|---|---|
| `OVER 8.5` | ev4 | ev4 | ev5 |
| **`OVER 9.5`** | **NEVER FIRES** | **ev4** | ev5 |
| `OVER 10.5` | ev5 | ev5 | ev6 |

**On a 9.5 line the count steps 8 → 10 and never sits at 9, so a distance-1 approach is SKIPPED and
the turn arrives with no build at all.** Threshold 2 **changes nothing on the other two lines** and
fixes this one. **Strictly better, at no cost, on the whole line grid.**

**This is the batching hazard one level up, and this seat named it and then did not apply it:**
`grammar-count-markets` §2 records that *a step of 2 can jump the line without ever landing on it* —
about the LINE. **The same property defeats a distance-1 THRESHOLD, and the spec was written without
noticing.** §1.5.

**The general form, which is what the constant should carry:** *the approach fires when the leg is
within reach of the next count event* — and with batching, "within reach" is not 1.

#### The other three rungs

- **The turn fires on the crossing beat**, as specced. Unchanged.
- **Quiet-once-decided is a STATE check, never distance arithmetic.** Read `RevealedLegOutcome` —
  the type `T108` already built. **A decided leg is quiet unconditionally**, and expressing that as a
  distance would invite a negative-number branch that means nothing.
- **UNDER takes the same threshold** — its distance is the `LIMIT n` the column already prints, and at
  2 one more event kills it. **But no turn fires on an UNDER's WIN:** an under wins by absence at full
  time, which is `T97-am`'s resolved-scene statement and not a beat. **Only the UNDER's death is a
  turn.**

#### The clock stays OUT of the trigger — for now

Tension is a rate, and `OVER 9.5` at 8 is comfortable at 70' and agony at 88'. **It is still not going
into the trigger this pass:** the approach fires once per leg either way, and adding a second variable
to a trigger **before the first has been seen on a single frame** is the predict-instead-of-measure
error this phase has already paid for twice.

**Named as the obvious next lever, and the reason the constant exists.**

#### What the constant may vary — and what it may not

**The constant is a TUNING knob, not a DESIGN knob.**

- **May vary freely:** the threshold's value. 2 → 3 is tuning and needs no ruling.
- **May NOT vary without a new ruling:** the two-rung stepped shape, the state-based quiet, the
  ticket-derived valence, and the exclusion of the clock.

Stated because *"a different ramp is one edit"* is true of the number and **not** true of the shape,
and a knob that can silently change a design is how a ruling gets lost.

#### Not reachable, so not built

**Concurrent live count legs.** The engine forbids two legs on one matchup and legs sweat
sequentially, so two count legs are never live together. **The principle, if that ever changes: the
scene takes the SMALLEST distance across live legs — the leg nearest to being decided governs**, which
is the hand-over falling out of the same rule. **Do not build it now.**

## 4. THE BINDING — A QUIET CORNER MUST STILL COUNT

**This is the gating condition, not a footnote, and it is what turns one gate into real work.**

`StageBeat()` **advances its cursor unconditionally** — it consumes the batch on the beat it is
called. `CompleteCount` fires from `OnCountPlayed`, **the scene's payoff callback.** So a beat that
takes a batch and falls through to calm **consumes the count without committing it: the column stops
tracking and the match ends short of its own total.**

**RULE: no beat may consume a count batch without committing it.** A corner that earns no scene is
still a corner — **the count is a fact; only the drama is discretionary.** The arrangement is the
lane's call; that it must hold is not.

> **AMENDED 2026-08-18 (`T118`) — THERE IS A SECOND DOOR AND THIS SECTION NAMED ONLY THE FIRST.**
> A quieted beat does not always become calm. Because a corners leg's `_goalSense` is **0**, a
> fallen-through Momentum beat reaches the **probability-reconciliation branch and may stage a
> GOAL** — in which case the scene's payoff is `OnGoalPlayed`, **not** `OnCountPlayed`, **so the
> batch is consumed and the count is still not committed.** The calm fall-through and the
> goal-upgrade lose it the same way, and a fix aimed only at the first leaves the second open.
>
> **But the cost is LOWER than budgeted below:** the count commit is already factored into a method
> independent of `OnCountPlayed` (which keeps its own copy of the same guard), **so this needs a
> CALL SITE rather than a new path.**

**Budgeted here rather than discovered: this is one gate PLUS a commit path that does not exist
today.**

## 5. ALREADY BUILT — DO NOT RESPECIFY

- **Valence off the ticket is DONE.** `countHelps` is set from `leg.Selection.Choice`, and
  `ScoreLedgerTests` asserts it: *`CornerFor`/`CornerAgainst` is the bettor's MOOD, not team*, and
  mood must never drive routing. **The earlier proposal is withdrawn — it exists and is gated.**
- **Calm scenes exist and play**, with their own pacing, excluded from buildup. Nothing to author.
- **Zero batches already fall through.** The path this spec widens is already there and already
  correct.
- **The UNDER's win by absence is `T97-am`'s**: the strip's words are licensed by the **resolved
  scene**, never the beat's own moment. Do not re-derive it.

## 6. OUT OF SCOPE — named so absence is not read as coverage

- **CARDS.** The opposite problem — a booking arrives carrying its own significance and needs
  **catching**, not ramping. Distance-to-line is the wrong instrument for it. **No cards arm has ever
  been shot** and nothing here is evidence about booking drama.
- **The UNDER case.** The mirror distance profile, not in evidence.
- **The flavour strip's overrun** — `T110-am`, its own ruling, its own measurement.
- **The resolved column's strings** — `T108` and its amendments.
- **The cashed-out footer** — `T114`.

## 7. THE GATE

1. **Assert a count event below the significance threshold does NOT produce a count scene** — and that
   the beat reaches the base table.
2. **Assert the count is committed on every staged batch, scene or no scene.** §4's binding, and it is
   the assertion that matters most: **a fixture running a full sweat must finish with the column's
   total equal to the match's own.**
3. **Assert significance is computed from REVEALED values** — no path from the locked target.
4. **Assert the decisive-beat string pool is disjoint** from the ordinary pool (§3.5).
5. If §2 is built: **assert the scoreline reveals independently of the ticket's market**, on a
   corners fixture whose match scores.

**Blind to:** whether the watch is better. **That is the whole point of the phase and no gate can
speak to it** — §8.

## 8. EVIDENCE OWED BEFORE DESIGN-VERIFIED

1. **A corners sweat re-shot on the same seed and line as `corners-sweat-2026-08-16`.** The
   before-state exists, so the after can be read against it directly — **same seed, same fixture, one
   variable.** That pairing is the instrument and it is already half-built.
2. **The scoreline's behaviour on that arm**, for §2.
3. **A near-line watch** — a leg that lands close to its line, or loses. Every frame we hold is a
   comfortable winner, and the ramp's whole value is in the case we have never seen.

### 8.1 THE DURATION CONSEQUENCE — named by TV before the frames, and folded into the read

**Added 2026-08-17 (`T115-am2`).** Quieting the corners arm **shortens the sweat by ~5.5s (~12.5%)**.
That is an unstated consequence of §3 and it will be visible on the re-shot pair. **TV named it rather
than letting the frames surprise this seat, which is the behaviour, and it is recorded as such.**

**It is probably the right direction.** The corners arm ran **41.42s against the control's 35.40s** —
**a 6.02s gap on the same measure** — so quieting **converges duration on the arm that works**, closing
that gap to **~0.5–0.9s**. The removed time comes entirely out of the five quieted events (~1.1s
each); **the two weighted beats keep their room by construction.** Length without contour was the
complaint, so removing length that carried nothing is removing dead weight, not content.

**One care, stated so the figure is not quoted wrong: two different measures are in circulation.** The
**12.5%** is against the probe's **44.42 sim-seconds**; the **control comparison** is against the
read's **41.42 / 35.40 sweep-duration table.** **The convergence holds on either, but the percentage
and the comparison are not on the same baseline** and must not be combined in one sentence.

#### The threat this creates to the comparison — and the fix

**A 12.5% shorter sweat re-tiles the windows.** `deadairNN` fires on *2.5 sim-seconds elapsing since
the last window of any kind*, so **the after-set will not have the same window count as the
before-set.** The pairing my §8 asks for would be comparing sets with different spines.

**RULED: the read anchors on the SEVEN COUNT EVENTS, not on windows, frames or time.** The events are
pinned by the seed and by the pre-planned batch schedule; **they do not move.** Windows, frame counts
and duration all shift, and none of them is the spine.

#### Read criteria for the re-shot pair, in priority order

1. **THE COUNT STILL REACHES 12.** §4's binding, and **the only correctness check here.** If the
   after-set's final total is short of the before-set's, **a quiet corner was consumed without being
   committed** and the fix has shipped a counting bug. **Cheap to check and it fails silently
   otherwise — check it first.**
2. **The five quiet events produce no COUNT scene and no count strip line.** ⚠ **Amended 2026-08-18
   (`T118`): NOT "no scene."** A quieted beat falls through to the base table, where it may
   legitimately be upgraded to a **goal** scene by probability reconciliation. **As first written this
   criterion would have failed a correct build.** The claim is that the count branch stops
   pre-empting — never that the beat goes silent. The resting state is legible as
   *absence*, and this is what the phase is for.
3. **Events 4 and 5 are visibly different from the other five.** The approach and the turn are the
   whole claim; if they do not separate, the ramp did not land.
4. **The strip falls to its calm register rather than holding or blanking** (`T117`).
5. **The scoreline reveals independently of the market** — only if §2 was built.
6. **Duration is RECORDED, not judged.** It is an output of the change, not a criterion, and **no
   pass/fail hangs on it.**

**Criterion 1 outranks the rest because it is the only one where being wrong ships a defect rather
than a disappointment.**

## 9. NOT CLAIMED

- **The probe measured SCHEDULING**, one seed, one line, with `ResolveBeat`'s interception untouched —
  **which is exactly what §3 changes.** It proves the calm beats exist and are spent. **It does not
  prove that reclaiming them yields a good watch**; that is a `C11` frame claim awaiting item 1 above.
- **No frame has been read for this spec.** §1's numbers are the capture's own log and the probe's.
- **`Territory` is excluded by arithmetic** (`Swing` Δp ≥ 0.10 against `Momentum` under 0.07) and holds
  for every seed; **the fallback arm's unreachability is a property of TODAY'S config** and would not
  survive a config change unexamined.
