# Register entries — 2026-08-15, batch 78

**OPTION C SPECCED — THE MARGIN'S THREE ZONES.** Written at the DD seat on Allen's adoption of the
A-now / C-before-the-gesture / B-in-reserve sequence.

**Destination table: SureThing — the laptop.** **Rows shipped:** `S82` **RULED (Allen)** ·
`S83` (option C, the scrolling flow — specced, sized after A).

---

## S82 — RULED BY ALLEN (relayed 2026-08-15)

> **A now, C before the gesture ships, B in reserve behind A's measurement.**

**Recorded with the condition the orchestrator attached, because it is the part that protects the
product:** **if A's measured recovery covers less than 34.10px, B's nudge-row deletion returns to
Allen with A's numbers rather than being taken silently.** **A stake control is not a rounding
error, and a deficit does not authorise the next item on a list.** **D is not taken. No chrome is
touched.**

---

## S83 — OPTION C: THE MARGIN BECOMES THREE ZONES

**The mechanism is not invented.** `BuildScrollingBody` already carries the board's matchup list and
ENTRY's market body (S25-am), with S27's printed position rail — **and that rail was verified
proportional to the pixel at this seat this morning** (S81-am: thumb 365px against a predicted 365px).
**C reuses a ruled instrument that has just been measured. Nothing new is built to make this work.**

### THE SPLIT, and it is derived rather than chosen

| zone | contents | behaviour |
|---|---|---|
| **1 · HEAD** | `MY MARKS · n SELECTIONS · m STAGED` | **FIXED** |
| **2 · THE SLIP** | leg rows · THE HOUSE'S LINE · the relation statement · the `COMBINED`/`SAME MATCH` price row · the modifiers row | **SCROLLS** |
| **3 · THE COMMIT** | `STAKE` label + figure · the fraction chips · the nudge keys · `POTENTIAL PAYOUT` + figure + wax band · **PLACE · LOCK · SKIP** | **ANCHORED, RESERVED** |

**Zone 1 is the board's own grammar, one screen over.** `BoardTitle` is fixed and only the region
beneath it scrolls, because *"BoardTitle is a column head for the list below it, not itself a row of
that list"* (`SportsbookApp.cs:211-213`). **`MY MARKS · n SELECTIONS` is a column head for the legs
under it and is governed by the same sentence.** **A count that scrolls away from the things it
counts is a head that has become a row.**

**Zone 3 is the decisive clause and it answers the objection this seat raised against its own
option.** Batch 77 warned that *the payout can sit below the fold while he presses PLACE*. **That
would be the exact defect this surface's first laws exist to prevent** — S17/S73: *a cost he cannot
see at the point of spending*. **So it does not scroll: the two figures the commit is about — what
he stakes and what he would win — are anchored with the controls that commit them.**

**And the stake block is not split.** M-05 put the figure first and its controls beneath it because
*the figure is the fact*; **separating a figure from its own controls across a scroll boundary would
undo that ruling to save pixels the zone does not need to save.** The block moves whole.

**Zone 2 is what remains, and it is coherent rather than residual: it is the SLIP — what he picked,
how it is marked, why it is priced that way, and what it costs.** **The relation statement and the
price row it explains are in the same zone and are never separated** — S73 requires the surface to
STATE the reason, and an explanation that can be scrolled away from its own price would not.

### The numbers — provisional on A, and the spec says which are which

```
margin panel                            530
  zone 1  HEAD, fixed                  − 44
  zone 3  COMMIT, reserved             −318   = stake 34 + chips 34 + nudges 32
                                                + payout label 18 + payout 40   (158)
                                                + ActionBandReservedHeight      (160)
  ─────────────────────────────────────────
  zone 2  VIEWPORT                      168
```

**Zone 2's worst content, pre-A: `legs 140 + 4 + statement 36 + price 28 + modifiers 34 = 242`, so
it scrolls by ~74px** — which is the same ~70px the budget was ever over by, because **anchoring
changes WHICH content scrolls and never HOW MUCH.** That invariance is why the zone split can be
argued on meaning rather than on arithmetic.

**EVERY NUMBER ABOVE IS PROVISIONAL ON A**, and this is the sequencing clause Allen's ruling
already implies: **A reclaims 20–34px from the flow chain, all of it inside zone 2. C is sized
against the POST-A flow, and its viewport is derived once from the factored measurement — never
from this seat's arithmetic.**

**The reason it must land after A rather than beside it, stated because it is a real defect
otherwise:** at four legs with nothing else the content measures **370.10 against a 370 budget —
a scroll of one tenth of a pixel.** **A scrollbar that appears for a tenth of a pixel is worse than
no scrollbar.** After A, the ordinary compositions stop overflowing at all and **the scroll engages
only where it is genuinely needed**, which is the difference between a form that scrolls and a form
that is always slightly broken.

### RULED — the clauses

1. **Three zones, as above.** Zone 1 fixed, zone 2 scrolls, zone 3 anchored and reserved.
2. **`ActionBandReservedHeight` grows to include the stake and payout blocks.** T47 is not weakened
   by this — **it is extended**: the rule was always *the flow region and the action band can never
   meet*, and the band now contains everything the commit depends on. **PLACE, LOCK and SKIP do not
   move by a pixel.**
3. **THE HOUSE'S LINE MOVES INTO THE SCROLLING CONTENT.** It is drawn in the gutter from `legRowY`
   and spans its group's rows with spurs. **A mark that holds still while its rows scroll points at
   rows it has nothing to do with — which is the exact defect the spurs were ruled in to prevent
   (§3.1).** **It scrolls registered to its members or the mark is a lie.**
4. **The scroll rests at the TOP.** One mechanism, one behaviour with the board (S25-am/S27), and
   the head names what is under it. **PRE-COMMITTED: if frames show he routinely lacks the price row
   at the moment of commit, the remedy is the RESTING POSITION, never anchoring more content** —
   zone 3 is closed by clause 2 and does not grow to absorb a reading problem.
5. **S27's rail appears whenever content exceeds the viewport, and reports proportionally** — the
   same instrument measured at S81-am, and it is checked the same way.
6. **A 1px DEAD-BAND.** The scroll and the rail do not engage on an overhang below one pixel.
   **S51's known 0.10px kit residue must never engage a scroll**, and the dead-band is named here so
   the next reader knows the residue is the reason rather than drift.

### What the invariant becomes — and it replaces S80-am2's clause 5 for zone 2 only

**Once the flow may legally exceed its viewport, `flowBottom ≥ −MarginFlowBudget` stops being the
question.** The gate asserts instead:

1. **The content container's height EQUALS the measured content depth** — the rail's honesty is
   entirely this, and S81-am is the precedent for checking it by measurement rather than assumption.
2. **Nothing is drawn outside the content container's bounds** — a fact rendered past the container
   is a fact that cannot be scrolled to. **Reachability, not containment, is the property now.**
3. **The three zones never overlap**, and zone 3's reserved height is exact.
4. **The two-sided slack bound survives in zone 3 only**, where the height is fixed by construction.
   **It is retired for zone 2, because a scrolling region has no slack to bound** — that is the
   clause S80-am2 §5 gives up in exchange for the scroll, and it is given up knowingly.
5. **T53: it states that it measures `RectTransform` bounds and not glyphs**, so a sentence bleeding
   past its box is invisible to it — unchanged, and still the failure mode the width sweep guards.
6. **It builds the state the sweep names as deepest** (S80-am2 §5), which C does not change.

### The cost, stated plainly because Allen already accepted it

**The margin stops being one glance.** At a full slip he sees the head, part of the slip, and the
whole commit — and scrolls for the rest. **What he never loses is the stake, the payout, and the
three controls.** **That is the trade C was chosen for, and the spec's job was to decide WHICH half
he keeps in view; it keeps the half he spends from.**

---

**Routing.** **S83 → surething-ui, sequenced AFTER A and sized against A's measured flow** — not
before, per §"the reason it must land after A". **S82's condition stands: if A recovers less than
34.10px, B returns to Allen with A's numbers.** **Still open and unchanged: S74-am3's checks 2 and 5,
whose re-shot block is being shot; the C46 width sweep rides with batch 74's.**

**To Allen, in one line:** *the scrolling margin keeps the stake, the payout and the three controls
permanently in view and scrolls only the slip itself — you never lose sight of what you are spending
or what you would win, and the sequencing means the scroll only ever appears when it is genuinely
needed.*
