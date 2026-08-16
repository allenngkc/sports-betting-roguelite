# The stats panel, ROWS KEYED TO THE TICKET · multi-count set · 2026-08-15

**Ruling:** DD batch 93 item (c) — the panel's rows key to the ticket it opens from; it becomes a
count-ticket feature, available where it has something to say.
**Harness:** `Capture_StatsPanel_MultiCountTicket`, seed `STATS-MULTI-1`, frame-contiguous, three
bursts (20 / 30 / 20).

**NO READ IS OFFERED.** The composition is the DD's.

---

## START HERE — the consequence the ruling did not price

> **The ruling made the panel's CONTENT variable. Its HEIGHT is still fixed at build time.**

An absent row is rendered as three empty strings (`SetStatsRow(i, "", "", "")`) — **the slot persists,
blank**. So the row set now varies with the ticket while the panel does not:

| ticket | rows with content | panel |
|---|---|---|
| corners **and** cards | 3 of 3 | full |
| one count leg | 2 of 3 | one blank row |
| **moneyline only** | **1 of 3** | **two blank rows** |

**So a moneyline ticket now yields ONE row in a THREE-ROW panel — and batch 87's oversized finding
quietly reopens on exactly the surface that ruling closed.** *"A surface that takes the entire stage
and returns three rows hasn't earned the stage"* was answered by sizing to content; keying the rows to
the ticket makes that content a variable the size no longer tracks.

**Not designed around, and deliberately not resolved here.** The obvious moves each cost something the
DD should weigh rather than inherit: sizing the panel at placement means the panel is no longer built
once; collapsing blank rows means the table's shape shifts under the player, which is close to what
"derives once and never changes under the player" exists to prevent; leaving it means the panel is
oversized again for the commonest ticket. **This set exists so the composition is ruled knowing that.**

## What the frames show

```
T100-style row dump, on the multi-count ticket:
  'GOALS|0|0'  ::  'CORNERS|2|4'  ::  'CARDS|—|—'
```

**Both count rows are PRESENT because the ticket bought both** — and `CARDS` carries the mark rather
than being absent, which is the whole point: **the mark's meaning has shifted from "not in your
ticket" to "not yet revealed."** A row that exists but has nothing to say yet is now distinguishable
from a row that was never bought, and this frame is where those two states are visible side by side.

`CORNERS` is populated from the corners leg; `CARDS` waits on its own leg.

## The build, for the read

- The row set derives **once, at ticket adoption** (`ComputeStatsRowSet()`, called immediately after
  `_ticket = director.CurrentTicket`), and is stored. `RenderStatsPanel` reads the stored flags and
  never the live leg's kind, so **the set cannot change under the player** — the ruling's own words.
- **Revealed counts are RETAINED for the life of the ticket**, in a per-kind store cleared only in
  `ResetForNewSession` and never on a leg change. Without that, a `CORNERS` row filled during the
  corners leg would revert to the mark once the cards leg went live — **a revealed fact
  un-revealing itself**, which is strictly worse than the behaviour the ruling replaces. Revealed
  totals only; the locked endpoint (`TargetHome`/`TargetAway`) is never read.

## `MATCH STATS` — measured, not changed

The DD flagged the label as overstating the subject once the panel is ticket-keyed, and it is coupled
to the box: `labelW = ceil(ink / 0.8)`, and `panelW = labelW + 418` where 418 is `4·pad + 2·valueW`,
both independent of the title. **So the coupling is 1:1 — every pixel off the label's measured ink is
a pixel off the panel's width.**

| label ink | labelW | panel width |
|---|---|---|
| 155.8 (`MATCH STATS`, today) | 195 | **613** |
| 100.0 | 125 | 543 |
| 60.0 | 75 | 493 |

**Illustrative arithmetic, not proposals.** No label was authored — copy is the DD's. Height is
unaffected; this is a width-only coupling.

## The set

**5 frames of 70 docked** (186.8 MB whole): the frame before opening, the overlay's first / middle /
last, and the return. `FRAME-INVENTORY-all-70.txt` lists every frame — each filename carries its seed,
boost, scene index, scene grammar, moment and frame index, so the set is self-identifying.

**Not a per-frame score/clock manifest.** The two earlier sets carry one; this run's harness log lives
in the dispatch's own scratchpad and is not in hand, so the inventory is filenames only. Said rather
than quietly shipping a thinner artifact under the same name.

## NOT CLAIMED

- **No read of the composition** — including the blank-row consequence above, which is stated as a
  measurement and a cost, not a recommendation.
- **Distinct seed and moment names** (`STATS-MULTI-1`, `multicount-*`), so this set neither overwrites
  nor is confusable with `tv-statspanel-reordered-2026-08-15`.
- **A moneyline-ticket frame is NOT in this set.** The one-row-in-three case is described from the
  build and from the re-authored pin that asserts it, not photographed. If the DD wants that state on
  a frame it is a second shot.
- **The 0px flush gap** between the panel's top and the scorebug's bottom edge is unchanged and still
  open.
- **`CARDS` is marked here, not populated** — its leg had not gone live at the shot. That is the
  ruled behaviour, and the retention store means it stays populated once it does.
