# Register entries — batch 148 (2026-08-20)

**`S74-am2` IS NOT OPEN. ITS FRAME WAS DOCKED FIVE DAYS AGO, TWICE, AND ALL FIVE OF ITS CHECKS
PASSED AT BATCH 79.** Its row still reads *"PRE-COMMITTED (frame not yet docked)"* because the
closure was written into **a neighbouring row's fourth cell** — the Batch column, which is the one
column a reader treats as provenance rather than state.

**One row.** **Destination table:** SureThing (`S74-am2-cl`).

---

## The row

| S74-am2-cl | `S74-am2` CLOSED — and its closure lives in `S74-am3`'s BATCH CELL, which is the worst hiding place the register has | **CLOSED — DD 2026-08-20 batch 148, on Allen's instruction to check whether the frame was already docked. IT WAS, TWICE.** **`dd-import/surething-board-draw-row-2026-08-15/` holds the original flat render and its main-camera pair; `dd-import/surething-board-draw-middle-2026-08-15/` holds the RE-SHOOT after the fix. Four frames across two sets.** **THE WORK IS FULLY DISCHARGED: `S74-am3` (batch 75) delivered the read — checks 1 and 3 passing, checks 2 and 5 failing on a measured 3px/6px gap asymmetry, cause diagnosed to two columns running on 35px and 38px pitches since before draws existed, remedy one constant. `S74-am3`'s BATCH CELL then records: *"CLOSED — CHECKS 2 AND 5 PASS · batch 79 … `AWAY` y 174–205 · gap 5px · `DRAW` y 211–242 · gap 4px · `HOME` y 247–278 … The pre-committed criterion was the two gaps match within a pixel or they do not; they differ by one. PASS … All five checks passed."*** **AND THE BUILD BEAT THE RULING, which that cell also records and is worth carrying forward: this seat ruled a VALUE (`−44.5f`) and the lane built a RELATIONSHIP (`DrawCellY = (AwayCellY + HomeCellY) / 2f`), gated on the rendered cells — *"a constant that happens to equal the right answer is a constant that will stop equalling it."*** **WHY THE SCREEN COULD NOT SEE IT, and this is `T7-am3`'s WORST CASE: `T41`'s closure at least had its own `-cl` ROW, findable by an ID search. `S74-am2`'s closure is not a row and is not in a state cell — it is APPENDED TO A NEIGHBOUR'S BATCH CELL, after 1,400 characters of that neighbour's own ruling. `S74-am2`'s own state cell and its own batch cell (`batch 74`) are both untouched and both say the work is outstanding.** **RULED AS BOOKKEEPING ONLY — no design ruling is made, amended or reopened here. What changes is the register's account of itself** | batch 148 |

---

## The consequence for batch 147's list

**`T7-am3` counted eight genuinely open items. It is now at most seven, and my hand-triage got at
least one wrong** — which means the remaining seven each need the same check before anyone works
them:

`T63` · `T86` (b)(c) · `T91` · `T94` (confirmed open at `T94-am`) · `T100` · `T105` · `S80`

**The check is cheap and it is now specified: for each, search the register for its ID and read the
STATE cell AND THE BATCH CELL of every row that mentions it.** `T41`'s closure was a `-cl` row;
`S74-am2`'s was a neighbour's batch cell; **neither is reachable by reading the item's own row, which
is what a reader does.**

---

## What is NOT in this batch

- **No design ruling.** `S74`, `S74-am`, `S74-am3` and their build are all untouched.
- **No convention change.** `T7-am2` declined that as register-wide and Allen's; this adds the second
  data point, not a proposal.
- **No work on the remaining seven** — the check is specified, not run.
