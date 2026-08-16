# Register entries — 2026-08-16, batch 94

**THE STATS PHASE CLOSES.** Four items in one read, against
`dd-import/tv-statspanel-ticket-keyed-2026-08-15/`.

**Destination table: TV — match theater.** **Rows shipped:** `T104-am2` (the blank rows — and two
clauses that are not in the build) · `T105` **RULED — `COUNTS`** · `T100-am2` **CLOSED, restated** ·
`T106` (the composition, granted).

---

## 1. THE BLANK-ROW CONSEQUENCE — real, and two thirds of it is a ruling not yet built

**On the frame: `GOALS 0 | 0` is still there.** TV's own table gives moneyline → **1 of 3**, and that
one row is `GOALS`.

**Batch 93 ruled two things that are not in this build:**

1. **`GOALS` is removed** — *"removed. Not conditionally, not keyed — removed."*
2. **Where the ticket gives no rows, the panel's affordance is unavailable and does not open.**

**Said without complaint — the lane's report is scrupulous and it is what surfaced this — but the
magnitude has to be corrected before it is weighed.** **With both clauses in:**

| ticket | rows | panel |
|---|---|---|
| corners **and** cards | **2 of 2** | full |
| one count leg | **1 of 2** | one blank |
| **moneyline only** | **0** | **no panel at all** |

> **The "one row in a three-row panel" case does not exist under the ruling. It exists under a build
> that kept a row the ruling removed.** **Batch 87's *earned the stage* finding does not reopen** —
> it reopens only against that.

### The residual is real, and it is a BUILD defect rather than a design cost

**A single-count ticket gives 1 of 2 — one blank slot.** **And the build produces it by conflating
two states that batch 93 made different:**

| state | what it should be |
|---|---|
| a row **not in the ticket's set** | **NO SLOT AT ALL** — it was never bought |
| a row **in the set, not yet revealed** | **keeps its slot, carries the mark** |

**The build renders absent rows as blank slots — `SetStatsRow(i, "", "", "")` — so an unbought row
occupies space and says nothing.** **That is the bug, and it is precisely the distinction that made
the mark worth keeping.**

### RULED — the panel's height follows its row set

> **The height derives from the row set, once, at ticket adoption — from the same computation that
> already derives the set.**

**`ComputeStatsRowSet()` is already called at adoption and stored.** **The height is a pure function
of it, so deriving it in the same place costs no new mechanism.** **§2 is satisfied by the argument
batch 93 already made and does not need a second one: a zone constant for the ticket's life cannot
change under the player — and if the SET may be per-ticket without breaching §2, so may a height that
is a function of the set.**

**None of the three remedies is taken as offered:**

- **(iii) leave it** — leaves a defect, not a cost.
- **(ii) collapse blank rows** — **right for absent rows, wrong for unrevealed ones**, and the build
  has it backwards. **An unrevealed row must keep its slot**, or the table grows when a count is
  first revealed, which is the shift under the player TV correctly fears.
- **(i) size at placement** — **correct, and cheaper than priced**: the derivation exists already.

---

## T105 — `MATCH STATS` → **`COUNTS`**. Ruled, and the coupling has a floor TV's table does not show.

### The floor, because it bounds the whole question

**`labelW` must hold the ROW labels as well as the title, and `CORNERS` is the longest of them.**
**So shortening the title below `CORNERS`'s ink buys nothing.**

Estimated from `MATCH STATS`'s measured 155.8 at ~14.2px/char, `CORNERS` ≈ **99px** → `labelW` floor
≈ **124** → **panel floor ≈ 542.** **The panel can shrink about 71px by title alone and no further.**
**TV's illustrative rows (ink 100 → 125, ink 60 → 75) sit below that floor and are unreachable.**
**The lane measures `CORNERS` exactly; the estimate is this seat's and is flagged as one.**

### RULED: `COUNTS`

- **It is this surface's own word for exactly these quantities** — `CountLedger`, count market, count
  leg. **Nothing invented.**
- **It sits in the label column's header cell, whose natural content is what the row labels ARE** —
  and under batch 93 they are `CORNERS` and `CARDS`: counts, both.
- **It drops `STATS`'s false breadth.** The panel never showed the match's stats and now explicitly
  shows a ticket-selected subset.
- **It is the register of every other label on this surface** — `GOALS`, `CORNERS`, `RISK`, `PAYS`:
  a bare uppercase noun.

**AND IT DELIBERATELY DOES NOT CAPTION THE TICKET CONNECTION.** §3.1: **the mark is DRAWN, not
CAPTIONED.** **The panel's belonging is expressed by where it opens from; a title restating it would
be the mark-that-needs-a-caption failure, one surface over.**

**Measure it and derive the box; the panel lands at its floor.**

---

## T100-am2 — THE 0px FLUSH GAP: **CLOSED, and it was closed at batch 89**

**Ruled then, measured: the panel's field is 2–3 luminance units above its surroundings, so there is
no visible boundary to attach with, and an edge that cannot be seen cannot read as attached. No gap
ordered.**

**Re-checked against the geometry that has changed since:** the T102 widening made the panel read
better as an object — **but by OCCLUSION at its RIGHT edge, against the pitch.** **The top edge still
borders near-black ground on both sides, so its own contrast is unchanged and the ruling is
untouched.**

**Recorded plainly because this item has been carried forward as *still open* across three docks.**
**A ruling that keeps being re-listed as open is a ruling that did not reach its reader** — which is
this seat's own batch-86 lesson arriving from the other direction, and the fix is the same: **it is
in the register, and the register is where it is settled.**

---

## T106 — THE COMPOSITION. Granted, subject to the three items above.

**The frame shows the thing (c) was for.** `CORNERS 2 | 4` populated **beside** `CARDS — | —`, on a
ticket that bought both:

> **Two states side by side — a row carrying a number, and a row carrying the mark because its leg
> has not gone live.** **The mark's meaning shift is VISIBLE rather than argued.** Batch 93 said the
> mark would stop meaning *irrelevant* and start meaning *not yet*; **this frame is that sentence
> rendered.**

**And the retention store is the lane's own find, made by building, and its reasoning is the
load-bearing half:** without it, **a `CORNERS` row filled during the corners leg would revert to the
mark when the cards leg went live — a revealed fact un-revealing itself**, which is strictly worse
than the behaviour the ruling replaced. **Revealed totals only; the locked endpoint is never read.**

**Also holding on this set:** the row set derives once at adoption and **never reads the live leg's
kind**, so it cannot change under the player; the column order agrees with the scorebug; the scorebug
is clear.

> **GRANTED. With `GOALS` removed, the height following the set, and `COUNTS` as the title, the panel
> is what it should have been from the start: a two-row table at its content's width, opening from
> the ticket column, showing the per-team split of exactly what the ticket rides on — and nothing
> its neighbours already say.**

---

**Routing.** **All four → TV as one change**: the `GOALS` row removed and the zero-row affordance
built (batch 93, outstanding); absent rows given no slot while unrevealed rows keep theirs; the
height derived from the set at adoption; the title `COUNTS` measured and the box derived.
**One frame closes it — a SINGLE-COUNT ticket, because that is the state the blank-slot fix is
about, and the multi-count case is already in hand.**

**THE STATS PHASE CLOSES on that frame.**

**To Allen, in one line:** *the panel is right — the frame shows a row with a number next to a row
saying "not yet", which is exactly what keying it to the ticket was for — and the oversized worry is
smaller than it looked, because the commonest ticket should have no panel at all under a clause that
has not been built yet.*
