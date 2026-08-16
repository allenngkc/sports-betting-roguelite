# THE PHASE-CLOSING FRAME — an unbought row is NO row · 2026-08-16

**Ruling:** DD batch 95 — *"an unbought row is not a silent row, it is NO row; the panel's height
follows its rows."* Blank-versus-mark kept as ruled correctly done.
**Harness:** `Capture_StatsPanel_WithAPopulatedCountRow` (unedited), seed `STATS-COUNT-1`, ticket =
ONE corners leg, frame-contiguous, three bursts.

**NO READ IS OFFERED.**

---

## The binary criterion, met and measured

> **Nothing beneath CORNERS.**

```
COUNTS
GOALS   | 0 | 0
CORNERS | 1 | 2
                  <-- the panel ENDS here. No slot, no reserved space.
```

Row dump from the unedited harness: `'GOALS|0|0' :: 'CORNERS|1|2' :: ''` — the third row prints as
nothing because **it does not exist**: `DebugStatsRow(2)` returns `null`, the absent signal, where a
bought-but-unrevealed row would return the mark.

**Proven numerically, not by eye:** the new pin measures the panel's **bottom inset against its top
inset off live rects** — both `32px`. A reserved slot would put 46px of dead space between the last
row and the panel's edge and the pin would go red.

## TWO ROWS, and GOALS stays (Allen, this cycle)

The ruling's prose said *"ONE ROW tall"*. A corners-only ticket carries **GOALS + CORNERS** under
batch 93's row set, so it is **two**. Raised before the shoot rather than resolved silently, and
**Allen ruled two rows with GOALS retained** — the binary criterion adjusts, the row set does not.
*Dropping GOALS to reach a count would have contradicted batch 93 and produced a frame worth
rejecting.*

## Height now follows rows

| ticket | rows | panel height |
|---|---|---|
| moneyline-only | 1 — GOALS | **154** |
| **corners-only (this set)** | **2** — GOALS + CORNERS | **200** |
| multi-count | 3 | **246** — now the MAXIMUM, no longer a fixed value |

**One formula, one application site.** `StatsPanelHeight(rowCount)` derives from the last row's own y,
and `ResizeStatsPanel()` is the single place that applies it — called from `ComputeStatsRowSet()` at
**ticket adoption**, which is where the row set becomes known. The panel is no longer a fixed-size
build-time object, and that is the batch-87-to-93 tension resolved rather than worked around.

**Rows are CONTIGUOUS, not hidden in place.** Slot indices are assigned by a running counter, so
CORNERS sits directly under GOALS whether or not CARDS exists. Unused slots are deactivated **and
collapsed onto the last active row's y** — hiding them in place would have left the hole the ruling is
about, and would have quietly broken the symmetry pin, which reads inactive children too.

**Width is untouched at 529.** Only height and row occupancy moved.

## A CORRECTION TO THIS LANE'S DOCK TEMPLATE

**Four earlier docks list the panel's 0px flush gap as OPEN. It closed at batch 89.** The line rode
forward because this seat's NOT-CLAIMED section had become a template copied between docks without
re-checking each line against canon. **It is struck here and struck from the template.**

> **A disclosure block that is not re-verified per dock stops being a disclosure and becomes a false
> claim with a wide readership.** Recorded rather than quietly dropped: the four sets that carry it
> — `…-scorebug-…`, `…-reordered-…`, `…-ticket-keyed-…`, `…-single-count-…` — **are wrong on that
> line, and it must not be re-opened from them.**

## The set

**5 frames of 70 docked** (183.3 MB whole) plus `FRAME-INVENTORY-all-70.txt`. Every filename carries
its seed, boost, scene index, grammar, moment and frame index.

**Supersedes `tv-statspanel-single-count-2026-08-15`**, which is the same ticket and beat with the
empty slot still reserved — i.e. the before.

## NOT CLAIMED

- **No read of the composition.**
- **The moneyline-only panel (one row, 154px) is NOT photographed** — it is derived and pinned, not
  shot. A frame of it would be a further shot.
- **`CARDS` absent here is the ticket's doing, not the leg's.** A bought-but-unrevealed cards row
  would still show the mark; that distinction is unchanged and was ruled correctly done.
- Suites whole: engine 306/306 · EditMode 255 (254/0/1) · PlayMode 126 (112/0/14), all 11
  `Stats_panel_*` pins passing by name and every capture entry point still Skipped.
