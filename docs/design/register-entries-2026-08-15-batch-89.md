# Register entries — 2026-08-15, batch 89

**T100 CLOSES — the resize is granted on the frames. Plus: no gap at the flush edge and the
measurement says why · `contentMargin` NOT ratified as a constant · and the placement has changed
what justifies the GOALS row.** Read at the DD seat against
`dd-import/tv-statspanel-resized-2026-08-15/`.

**Destination table: TV — match theater.** **Rows shipped:** `T100` **DESIGN-VERIFIED** ·
`T100-am2` (the flush edge) · `T102` (`contentMargin`) · `T103` (the GOALS row).

---

## T100 — DESIGN-VERIFIED. The panel is sized to what it carries.

| | before | after |
|---|---|---|
| rect | `(0, 0, 980, 480)` | **`(0, 62, 564, 246)`** |
| area | 470,400 px² | **138,744 px² — 29.5%** |

**Granted.** The boxes are **widest measured ink plus a margin, taken from a committed sweep** rather
than chosen; `pad` (32) is the only spacing value; the vertical rhythm was not touched. **That is
batch 87's ruling executed as ruled — sized once at design time to its content, §2's own legal
move** — and per batch 88 it is a **return** to §6.6's standing *authored height with no reserved
space* (T21) rather than a new requirement.

**The scorebug is clear and the pin is the right shape.** `ScoreBug (265, 0, 715, 62)` against
`Panel (0, 62, 564, 246)` — **zero overlap on BOTH axes, asserted as full 2D against live rects.**
**Deliberately not a single-axis comparison**, so a later change narrowing only the vertical gap
while the columns still cross is caught as **the partial coverage it would be.** *A half-covered
scorebug is worse than a fully covered one*, and the pin is built to that sentence.

**And the guard that now guards nothing was KEPT.** The standing condition at `SeatedDeltaTime`
protects a coverage that no longer happens — **retained because it is written where a future
re-enlargement would be made, which is exactly when it stops being vacuous.** **Deleting a guard
because it currently guards nothing is how the T95 class recurs**, and the lane said so itself.

---

## T100-am2 — THE FLUSH EDGE: **no gap ordered**, and the measurement is the reason

**TV flagged it honestly: the panel's top sits at 0px on the scorebug's bottom, and *an opaque panel
flush against the scorebug may read as attached.*** **Measured at this seat, the risk does not
occur — and not for the reason either of us expected.**

**The panel's field is barely a field.** Scanned across the panel's width between two rows: **~24–26
inside it against ~23 immediately outside.** **Two to three luminance units.** **There is no visible
boundary to attach with.**

> **An edge that cannot be seen cannot read as attached. No gap is ordered.**

**Adding ground there would separate two things that are not visually joined** — spending pixels to
solve a problem the frame does not have.

### The finding underneath it, which is the part worth keeping

**The panel's presence is carried entirely by OCCLUSION.** It reads as a panel because the pitch
stops at its edge, not because a panel is drawn. **Where it overlaps empty ground — the ticket
column's middle — it is invisible.**

**Recorded so it is not "fixed" from either direction:** **neither a gap at the scorebug (§above) nor
a border around the panel** should be added on the strength of this frame. **The composite reads:
the eye completes a rectangle from where the pitch is missing.** **But it is now a written property
rather than an accident, so a change that removes the occlusion — a panel opened over a cleared
stage, say — would leave nothing to read at all.** That is the condition under which this returns.

---

## T102 — `contentMargin = 16`: **NOT RATIFIED as a constant.** The margin is a proportion, and the law already exists.

**The lane named it correctly: one invented number, load-bearing, and unratified.** **It is not
ratified, and the reason is that a constant is the wrong FORM for what it does.**

### What 16px actually buys, measured against the boxes it produced

| box | widest ink | box | headroom |
|---|---|---|---|
| label | `MATCH STATS` 155.8 | 172 | **9.4%** |
| value | `Spreadsheets` 115.3 | 132 | **12.6%** |

**C46's risk is PROPORTIONAL — a face 10% wider makes every string 10% wider — so a fixed pixel
margin gives its least protection exactly where the risk is greatest, on the widest string.**
**Concretely: at 16px, `MATCH STATS` survives a face ~10% wider and overflows at 11%.**

### RULED — S77-am's budget governs, because C46 is one law and should not grow a second budget

**S77-am already rules this hazard on the other surface: *every future stamp form is measured against
its box and stays under 80%*, and its stated reason is C46 — *~20% is what absorbs a face that
measures wider.*** **C46 is a register-level, cross-surface law. Inventing a second headroom budget
for the same hazard on a different surface is one-name-per-thing broken at the level of rules.**

> **RULED: ink ≤ 80% of box, here as on the laptop.**

- `MATCH STATS` 155.8 → **labelW ≥ 195** (from 172)
- `Spreadsheets` 115.3 → **valueW ≥ 145** (from 132)
- **Panel ~564 → ~613 — still 37% of the original 980, so the resize's win is essentially intact.**

**`contentMargin` as a named constant is fine and should stay** — the derivation reads from it rather
than restating literals, which is the right shape. **What changes is that it is derived from the
ratio, not chosen.**

### AND S84 BINDS THE VALUE COLUMN, which is the half most likely to be missed

**`MATCH STATS` is authored copy. `Zambonis` and `Spreadsheets` are TEAM NAMES — generated.** **S84,
one surface over and eleven batches ago: *a cell holding a GENERATED string is sized against the
reachable maximum of its GENERATOR, never a sample taken off the surface.***

**`Spreadsheets` at 115.3 is the widest string the sweep MEASURED. Whether it is the widest team name
that EXISTS is a different question and the pool is a fixed list.** **OWED: confirm 115.3 against the
enumerated team pool, not against the boards the sweep happened to build.** **If a wider name exists,
`valueW` derives from that one.** **Cheap, exact, and it is the same discipline the draw price was
held to.**

---

## T103 — THE PLACEMENT CHANGED WHAT JUSTIFIES THE GOALS ROW. Ruled as a finding; the remedy is Allen's.

**At T99 this seat ruled the GOALS row legitimate — and it ruled it while the panel COVERED the
scorebug.** **Allen's placement removed that condition, and the row's justification went with it.**

### Two things are now true that were not

**(1) It is a duplication.** The scorebug is never covered and carries the score permanently and
prominently. **The panel states the same fact two slots below it. T87-am's rule, in its own words:**
*§8 forbids the strip duplicating the score, and duplicating the clock is the same error with a
different neighbour.* **Generalised: the panel's job is to say what the scorebug cannot.**

**(2) The two slots present that fact in OPPOSITE COLUMN ORDER, adjacent and simultaneous.**
Scorebug `YAMS 0 — 0 ZAMBONIS`; panel `Zambonis | Yams`. **At T99 this seat CLEARED the reversal —
explicitly because the panel covered the scorebug and the two never coexisted. That clearance no
longer applies.**

**At 2–1 the screen would read `YAMS 2 — 1 ZAMBONIS` above and `Zambonis 1 | Yams 2` below: the same
fact, mirrored, adjacent.**

> **AND THIS SET CANNOT SHOW IT, BECAUSE IT IS 0—0.** **The same trap T99's own capture condition was
> written to avoid** — a level scoreline carries no information, so no reading of it can fail.
> **Recorded rather than resolved on evidence that cannot bear it.**

### RULED, and the boundary of the ruling is stated

**The duplication is real and the row cannot stand on the justification T99 gave it.** **What
replaces it is not this seat's alone, because every remedy has a product consequence:**

| | remedy | consequence |
|---|---|---|
| **(a)** | **drop the GOALS row** — the panel says only what the scorebug cannot | `CARDS` can never populate off a corners leg and `CORNERS` never off a cards leg, so **the panel falls to at most ONE populated row.** Its existence becomes the question |
| **(b)** | **keep it and align the panel's column order to the scorebug's** | duplication remains but stops contradicting. Cheapest, and it closes (2) outright |
| **(c)** | **key the panel's rows to the TICKET** — it opens from the ticket column and this ticket is `OVER 8.5 CORNERS`, so corners is what it rides on | **probably the right answer and it is a redesign**, on a surface this seat has seen in three states |

**RECOMMENDED: (b) now, (c) considered when the panel's content is next opened.** **(b) is one
reordering and removes the only thing that can actively mislead. (a) is not recommended alone —
deleting a row to satisfy a rule, and leaving a panel that shows one number, answers the composition
and worsens the product.**

**The scope question from batch 87 is now sharper and it is Allen's: `_countLedger` carries exactly
one kind and resets per leg, so TWO ROWS OF THREE is the panel's structural maximum — and under (a)
it is one.**

---

**Routing.** **T100 CLOSED, Design-verified.** **T100-am2: no change — recorded.** **T102 → TV: boxes
re-derived at ink ≤ 80%, and `Spreadsheets` confirmed against the enumerated team pool.** **T103 →
Allen: (b) recommended, with (c) named.** **Still owed: `S85`'s treatment on a frame (surething-ui).**

**To Allen, in one line:** *the resized panel is verified and the scorebug is safe — but now that the
score is never covered, the panel's own GOALS row repeats it two slots down and in the opposite team
order, which at any score but 0–0 would read as a contradiction; reordering the columns fixes it
today, and the deeper answer is that a panel opening from the ticket should show what the ticket
rides on.*
