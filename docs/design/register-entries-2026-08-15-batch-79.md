# Register entries — 2026-08-15, batch 79

**THE DRAW ROW CLOSES ON THE FRAME · C SIZED ON A'S SPLIT · B REFRAMED · AND THE STATS PANEL'S
LICENCE IS THE FREEZE, NOT THE GOALS ROW.** Written at the DD seat against `0666a51`.

**Rows shipped:** `S74-am3` **CLOSED — checks 2 and 5 PASS** · `S84` (the price cell under C46) ·
`S83-am` (C sized against A's per-block split) · `S82-am` (B reframed — it may not be needed) ·
`T99` (the stats panel over the scorebug).

---

## S74-am3 — CHECKS 2 AND 5 PASS. Measured, binary, as pre-committed.

**Measured at this seat off the re-shot flat render, same instrument and same x-strip as the read
that found the defect** — luminance profile of the price column, cell field `LaptopOs.Ink` ~20
against card ground ~27:

| | before (`−43`) | **after (`−44.5`)** |
|---|---|---|
| `AWAY` cell | y 174 – 205 | **y 174 – 205** |
| **gap** | **3px** | **5px** |
| `DRAW` cell | y 209 – 240 | **y 211 – 242** |
| **gap** | **6px** | **4px** |
| `HOME` cell | y 247 – 278 | **y 247 – 278** |

**The pre-committed criterion was *the two gaps either match within a pixel or they do not*. They
differ by one. PASS.**

**The 5/4 residue is the grid, not a miss.** `−44.5` on an odd 41px span rasterises to 5 above and 4
below; the cell's centre lands at 226.5 against its siblings' midpoint of 226.0. **Half a pixel, and
it cannot be improved** — batch 77 anticipated exactly this in ruling `−44f` acceptable at 4/5.
**The 2:1 proximity ratio that grouped AWAY with DRAW is gone.**

### The build did better than the ruling, and the difference is worth recording as the standard

**This seat ruled a VALUE. The lane built a RELATIONSHIP:** `DrawCellY = (AwayCellY + HomeCellY) / 2f`,
with all four placing sites reading the constants and a gate that measures the **rendered** cells
rather than the constants.

**That is strictly better and it closes a hazard the ruling left open.** A number centred by intent
goes stale the moment either team line moves; a midpoint carries the draw with it. **The ring's own
comment had already named the class — *two elements agreeing by convention rather than by
construction*, T95's shape on the TV — and the fix leaves no literal to move.**

> **RECORDED: where a ruled position is DERIVABLE from positions already ruled, it is built as the
> derivation and gated on the rendered result. A constant that happens to equal the right answer is
> a constant that will stop equalling it.**

**S74-am3 CLOSES. All five checks passed.**

---

## S84 — `DRAW {price}` UNDER C46. The 80% rule is NOT extended, and the reason is that its remedy does not exist here.

**The measurement, delivered off the rendered control:**

| string | width | vs the 112px cell |
|---|---|---|
| widest observed over three boards (`DRAW  +253`) | 97.1px | **87%** |
| `AWAY  −341` — what the cell was sized for | 91.4px | 82% |
| format ceiling `DRAW  +10000` | 115.5px | **103%, overflows** |

### Why S77-am's 80% rule does not simply extend to this cell

**It is tempting and it is wrong.** S77-am ruled *every future stamp form is measured at 13px against
296px and stays under 80%* — **and its remedy clause is the whole rule: *a form over 80% is
RE-AUTHORED, never accommodated.***

**A stamp form is authored copy. A price is not.** `DRAW  +253` is a fixed label plus a generated
numeral, and **no one can re-author a number the model emits.** **Extending a rule whose only remedy
is unavailable would manufacture a permanent violation instead of a fix** — the cell would sit
in breach at 87% with nothing anyone is allowed to do about it.

### RULED — the discipline for a GENERATED string in a fixed cell

> **A cell holding an authored string is sized against the authored population and re-authored when
> it misses. A cell holding a GENERATED string is sized against the REACHABLE MAXIMUM OF ITS
> GENERATOR — and what is reachable is a question for the model, never a sample taken off the
> surface.**

**Three boards is C46's own failure mode stated in C46's own words: *sweep the POPULATION, not the
suspects.*** `+240 … +253` is three suspects. **The population is whatever `OddsMath.OverroundOdds`
can return for the draw arm over the reachable latent range**, and the engine can enumerate that
exactly where the surface can only sample it.

### OWED — one query, not a sweep of boards

**Report the MAXIMUM REACHABLE DRAW PRICE from the model**, not a digit count. Then the cell is
judged against that one string and this closes.

**Why the answer is not obvious in either direction, stated so nobody shortcuts it:** a five-digit
American price needs decimal odds ≥ 101, i.e. **a draw under 1% probability** — and the draw is
structurally the least extreme of the three outcomes, since even a total mismatch keeps a 0–0 or 1–1
live. **So it is probably unreachable — but "probably" is not a measurement**, and the surface must
not certify a cell on a plausibility argument about a model it does not own.

**And the four-digit case is not free either:** at ~9.2px per digit, `DRAW  +9999` measures ~106px —
**95% of the cell.** **It fits and it is the tightest string on the board**, which is a fact for the
record rather than a defect.

**PRE-COMMITTED so this costs one pass. (1) Max reachable ≤ 4 digits → the cell holds, S84 CLOSES
with the tightness recorded and no change. (2) Max reachable is 5 digits → the cell must grow, and
that is a BOARD-GEOMETRY change on a board Allen has just fixed, so it returns to him rather than
being taken here.** **Not ruled: what a grown cell would cost. It does not exist until (2) fires.**

---

## S83-am — OPTION C, SIZED ON A'S PER-BLOCK SPLIT

**The split is why the per-block report was asked for, and it lands in three different zones:**

| A's recovery | block | **C zone** | effect |
|---|---|---|---|
| −4.00 | header gap 8 → 4 | **zone 1 · HEAD (fixed)** | head 44 → **40** |
| −4.00 | the bare post-leg gap 4 → 0 | **zone 2 · THE SLIP (scrolls)** | content −4 |
| −2.00 | payout label 18/16 | **zone 3 · THE COMMIT (anchored)** | reserve 318 → **316** |

```
margin                 530
  zone 1  HEAD        − 40
  zone 3  COMMIT      −316
  ───────────────────────
  zone 2  VIEWPORT     174     (was 168)
```

**Zone 2's worst content falls to 238, so the scroll is ~64px** — and **the exact figure comes from
the factored measurement, not from this arithmetic**, which reads ~60.10 for the same state. **The
4px is the cursor-sum against measured bounds, established at batch 74 and unchanged. The factored
number governs.**

### And the marginal-scroll hazard C was sequenced to avoid is now MEASURABLY GONE

Batch 78 held C behind A because *a scrollbar that appears for a tenth of a pixel is worse than no
scrollbar*. **Post-A, in zone 2:**

| state | content | vs the 174 viewport |
|---|---|---|
| 4 legs alone | 168 | **fits — no scroll at all** |
| + a held consumable | 202 | scrolls 28 |
| + the statement | 204 | scrolls 30 |
| + both | 238 | scrolls 64 |

**The ordinary composition does not scroll. The scroll engages only where a consumable is held or a
sentence is present** — which is precisely the property batch 78 asked A to deliver, now measured
rather than hoped for. **C's sequencing condition is DISCHARGED and C is clear to build.**

---

## S82-am — THE B CALL, REFRAMED. C closes the live bill too, so B may not be needed at all.

**Disposition 2 fired correctly: A recovered 10.00 of 34.10 and the nudge row goes to Allen.** **But
the question in front of him is not the one it looks like, and this seat owes him the reframe before
he answers it.**

**B deletes a stake control to remove a 24.10px overrun. C removes the same overrun by making the
region scroll — and C is already approved, already specced, and now measured clear to build.** **A
scrolling flow has no overrun to fix.**

**So B is needed only in the WINDOW between now and C**, and the real question is:

> **Does the live defect — four legs plus a held consumable, colliding into T47's pad — have to be
> fixed before C can land?**

**That is a scheduling question, not a design one**, and this seat states its recommendation rather
than pretending otherwise:

**RECOMMENDED: bring C forward and do not take B.** **Its sequencing condition is discharged** (§S83-am),
**it closes the live bill and the statement's together**, and **it costs no product fact where B
costs a control.** **B stays in reserve for the case where C slips** — which is exactly where it was
put, and nothing about A's shortfall moves it out.

**What this seat will NOT do: take B's silence as consent.** The orchestrator's own condition was
that the nudge-row call reaches Allen with numbers rather than being taken quietly. **It has, and
this is the design read that goes with them.**

---

## T99 — THE STATS PANEL OVER THE SCOREBUG. The freeze is the licence. The GOALS row is not.

**TV's argument is sound and one of its two limbs is load-bearing while the other must not be
relied on.**

### The overlay is PERMITTED, and the FREEZE is what permits it

**Time is frozen while the panel is open, so the scorebug's facts cannot change behind it.** **A
covered fact that cannot move is deferred; a covered fact that can move is lost.** That is the whole
distinction, and the ruling rests on it entirely.

> **RULED: the stats panel may cover the scorebug band FOR AS LONG AS TIME IS FROZEN WHILE IT IS
> OPEN. If the match ever runs behind this panel, the scorebug must survive the overlay.**

**Stated as a standing condition rather than a one-time approval, because the danger is a future
change that looks unrelated** — "let the match play while he reads the stats" is a plausible later
improvement that would silently break this ruling. **The condition is written where that change would
be made.**

### The GOALS row is NOT the justification, and arguing it that way would cost more than it gains

**A statistic is not a result.** The scorebug's `0 — 0` is **the match's standing**; a stats table's
GOALS row is **one measure among possession, shots and corners.** They carry the same digits in
different roles.

**The GOALS row stays — goals-for is a legitimate row in a stats table and nothing here removes
it.** **But it must not be offered as the reason the scorebug can be covered**, because that claim
makes the panel a REPLACEMENT for the scorebug, and a replacement owes everything the original
carries: the score in its own form, the clock, and T38's single-frame change with no intermediate
state. **The panel does none of that and should not be asked to.**

**Nothing is lost because the match is not moving — not because the score is printed twice.**

### THE CAPTURE, ordered — and its one binding condition

**One capture, and it must NOT be at 0–0.** **A stats panel over a goalless scorebug proves nothing:
the covered scorebug is carrying no information, so no reading of it can fail.** **Shoot it at a
scoreline that is not level and after at least one goal**, so the thing being covered is a fact the
player would want.

**PRE-COMMITTED, so the frame closes this in one pass:**

1. **The panel reads as a DELIBERATE overlay, not as a panel that overshot its zone** — the edge
   lands somewhere the composition explains.
2. **The covered band does not show a fragment of the scorebug.** **A half-covered scorebug is worse
   than a fully covered one**: a sliced score is a fact rendered unreadable, where a hidden one is
   simply deferred.
3. **The GOALS row does not read as the scoreline** — it sits in the table's own register, among its
   siblings, and is not styled to stand in for what is behind the panel.
4. **On close, the scorebug returns with its values unchanged** — the freeze, visible rather than
   asserted.

**Not ruled: the panel's own composition, which this seat has not seen.** **And one thing to confirm
rather than assume — whether the EVENT STRIP is also covered.** T66 puts authored statements there
and T87-am2 gave the drawn ending a minimum hold; **a statement that fires and holds behind an opaque
panel was never made.** **If the strip is inside the covered zone, say so and it takes its own
clause** — the freeze protects the scorebug because its facts are static, and **a timed statement is
not static even when the clock is.**

---

**Routing.** **S74-am3 CLOSED.** **S84 → the engine/model lane: one query, the maximum reachable draw
price.** **S83-am → surething-ui: C is clear to build, sized on the split above, viewport derived from
the factored measurement.** **S82-am → Allen with batch 77's numbers and this reframe.** **T99 → TV:
the ruling, its standing condition, one capture at a non-level scoreline, and the event-strip
question answered before it is shot.**

**To Allen, in one line:** *the draw sits in its middle and the board closes; the nudge row is
probably not a decision you have to make, because the scrolling margin you already approved removes
the same overrun without deleting a control — and the stats panel may cover the scorebug only for as
long as the clock is stopped.*
