# Register entries — 2026-08-16, batch 97

**THE STATS PHASE CLOSES.** Read at the DD seat against
`dd-import/tv-statspanel-rows-removed-2026-08-16/`, measured against its predecessor.

**Destination table: TV — match theater.** **Rows shipped:** `T106` **DESIGN-VERIFIED — PHASE
CLOSED** · `C51` (a law promoted from the lane's own dock correction).

---

## T106 — THE CRITERION IS MET, measured at this seat rather than read off the log

**The binary line was: a corners-only ticket makes the panel exactly two rows, `GOALS` and `CORNERS`,
with nothing beneath them.**

**Measured, same seed and beat, against `tv-statspanel-single-count-2026-08-15`:**

| | panel's bottom edge | |
|---|---|---|
| before | **y = 792** | field 25.26 → ground 22.45 |
| after | **y = 691** | field 25.43 → ground 22.74 |
| | **−101px** | **exactly one 46px row pitch at this frame's scale** |

**The panel shrank by one row and not by a number someone chose.** **And a pitch feature now reads at
y 696–741 where the panel covered it — the stage is given back, measurably, not just arithmetically.**

**The absent signal is distinct from the mark at the source**: `DebugStatsRow(2)` returns `null` where
a bought-but-unrevealed row returns the mark. **The blank-versus-mark distinction the lane built is
kept, and now sits UNDERNEATH a slot distinction instead of substituting for it** — which is what
batch 95 asked for and why both halves were worth having.

### The mechanism, and the way it is built is the half that lasts

**One formula, one application site.** `StatsPanelHeight(rowCount)` derives from the last row's own
y; `ResizeStatsPanel()` is the single place it is applied; it is called from `ComputeStatsRowSet()`
**at ticket adoption — where the row set becomes known.** **246 stops being a fixed value and becomes
a maximum**, which is the batch-87-to-93 tension resolved rather than worked around.

**And the lane caught a second-order defect inside its own fix:** rows are assigned by a running
counter and unused slots are deactivated **and collapsed onto the last active row's y**. **Hiding
them in place would have left the exact hole the ruling is about — and would have quietly broken the
symmetry pin, which reads inactive children too.** **A fix that passes its own gate by accident is
the failure this studio has spent the week naming; this one was found before it could.**

---

## C51 — PROMOTED, from the lane's own correction of its dock template

**Four earlier docks carried the panel's 0px flush gap as OPEN. It closed at batch 89.** **The lane
found the mechanism rather than the instance: its `NOT CLAIMED` section had become a TEMPLATE, copied
between docks without re-checking each line against canon.** **It is struck here and struck from the
template.**

> **C51 — A disclosure block that is not re-verified per dock stops being a disclosure and becomes a
> false claim with a wide readership.**

**Promoted to a register-level law because every dock in this studio carries a `NOT CLAIMED` section,
and every one of them is a template in waiting.** **The honest-gaps discipline this seat has praised
all week is the thing at risk: a stated non-claim is trusted precisely because stating it costs
something, and a copied one costs nothing.**

**And the lane named the four wrong sets explicitly rather than quietly dropping the line** —
`…-scorebug-…`, `…-reordered-…`, `…-ticket-keyed-…`, `…-single-count-…` — **so the false line cannot
be re-opened from them.** **That is the correction done properly.**

**It is batch 86's lesson from the other side.** That one was *a ruling recorded in a commit message
is recorded nowhere a reader looks*; this one is *a disclosure copied forward is a claim nobody
made*. **Both are the artifact drifting from the truth it says it carries, and both are caught by
re-reading rather than by remembering.**

---

## THE PHASE CLOSES — what it moved

| | at the start | now |
|---|---|---|
| panel | `980 × 480` — 470,400 px² | **`529 × 200` — 105,800 px², 22.5%** |
| the scorebug | **covered**, under a conditional licence | **never covered** |
| rows | three, fixed | **`GOALS` standing, count rows keyed to the ticket** |
| height | fixed at build time | **derived from the rows at ticket adoption** |
| title | `MATCH STATS` | **`COUNTS`** |
| the unrevealed mark | *this is not in your ticket* | ***your ticket rides on this and it is not yet revealed*** |

**The panel takes just over a fifth of the stage it began with, covers nothing its neighbours already
say, and its remaining rows are the ones the player's own ticket bought.**

**Not photographed, and not ordered:** the moneyline-only panel — one row, 154px. **The mechanism is
proven at two row counts and the third is the same formula, so a frame of it would confirm arithmetic
rather than a design.** **Named so the gap is known.**

---

**Routing.** **T106 CLOSED — Design-verified. THE STATS PHASE IS CLOSED.** **C51 → the register as a
law; the four docks carrying the struck line are named in it.** **Nothing is owed at this seat.**

**To Allen, in one line:** *closed — the panel is a fifth of the size it started at, covers nothing
the screen already tells you, and shows exactly the counts your own ticket bought; and the lane
closed it by catching that its own disclosure list had become a template, which is worth more than
the frame.*
