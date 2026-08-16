# Register entries — 2026-08-16, batch 92

**T103 AND T102 VERIFIED ON ONE SET — and the 0.7px flag is the distance to the ALARM, not to the
failure.** Read at the DD seat against `dd-import/tv-statspanel-reordered-2026-08-15/`, measured
against its 564-wide predecessor.

**Destination table: TV — match theater.** **Rows shipped:** `T103` **DESIGN-VERIFIED** ·
`T102` **DESIGN-VERIFIED** · `T102-am` (the 0.7px headroom) · `T100-am3` (the widening's cost).

---

## T103 — THE REORDER: DESIGN-VERIFIED. Both axes agree on the frame.

**Scorebug `YAMS 0 — 0 ZAMBONIS`; panel `Yams | Zambonis`; `CORNERS 1 | 2` against a revealed home 2
/ away 1.** **The mirror is gone.**

**And the cause is now named at the source rather than matched by hand.** The scorebug composes
`{away} {awayScore} — {homeScore} {home}` (`TvSweatScreen.cs:2404`) — **AWAY → HOME** — and the panel
rendered **HOME → AWAY**. **The order is written at the panel's site as *the scorebug's*, citing that
composition line, so the two cannot be chosen independently again.** **That is the fourth time this
week a ruled value has been built as a derivation instead** — the DRAW cell's midpoint, the commit
zone's constants, the ink fraction below, and now this. **It has stopped being a correction and
become how this studio builds.**

### The clause that matters most is the one about not doing half of it

**Fixed on every row together — headers, `GOALS`, `CORNERS`, `CARDS`.** **Swapping the headers alone
would have put the right club names over the wrong numbers — a STATE LIE, and strictly worse than the
confusing order it replaced.** **Recorded because it is the failure mode a partial fix would have
produced, and it would have passed a casual look.**

---

## T102 — THE RE-DERIVATION: DESIGN-VERIFIED.

| | at `+16` | **at 80% ink** |
|---|---|---|
| label / value | 172 / 132 | **195 / 145** |
| panel width | 564 | **613** |
| panel area | 29.5% | **32.1%** |

**`MaxInkFraction = 0.8` is one named constant and both widths derive from it — 195 and 145 are never
restated, so a further ruling moves one number.** **`contentMargin` was REMOVED rather than left
dead**, which is the half most builds skip. **The resize win survives: still under a third of the
stage.**

### S84's pool binding, and it now GATES — which is the part worth the most

**20 clubs enumerated, every one re-measured through the panel's own rendered slot.** **And it is
deliberately NOT `[Explicit]`:** the C46 sweep is filter-only and never runs in routine suites,
**which is precisely how a 21st club could have overflowed unnoticed.** **A binding that only runs
when someone remembers to run it is not a binding.**

**And it reads `MaxInkFraction` off production through a debug hook rather than restating `0.8`, so
the test and the surface cannot disagree about the rule.** **That is S80-am2 §6's factored
measurement arriving on the second surface** — *where a gate and the thing it guards use the same
quantity, they share one source or they will drift.*

---

## T102-am — THE 0.7px FLAG. Accept as built, and the framing needs correcting before it alarms anyone.

**The lane flagged honestly: `Spreadsheets` 115.3 against a 116.0 limit is 79.52%, and the tightness
is a property of the rule — `ceil(115.3 / 0.8)` lands just above the widest string by construction.**

**THERE ARE THREE NUMBERS HERE AND ONLY ONE OF THEM IS 0.7:**

| | px | distance |
|---|---|---|
| widest ink (`Spreadsheets`) | 115.3 | — |
| **gate limit** (80% of box) | 116.0 | **0.7px — to the ALARM** |
| **box** | 145.0 | **29.7px — to the OVERFLOW** |

> **RULED: 0.7px is the distance to the WARNING, not to the failure. The gate fires 29.7px before
> anything breaks. That is what an early-warning pin is for, and it is working exactly as intended.**

### Why sizing the box to satisfy the ceiling *exactly* is correct

**The 20% is a FACE budget, not a pool budget, and it is fully intact.** A face 20% wider takes
`Spreadsheets` to 138.4 against a 145 box — **6.6px clear.** **The gate is not measuring face
headroom at all; it is measuring *has the pool's maximum moved*, which is a different alarm with a
different remedy — and the lane implemented that remedy correctly.**

### The one thing forbidden explicitly, because it is the obvious "fix"

> **DO NOT PAD THE BOX TO QUIET THE ALARM.** Padding spends the FACE budget to buy POOL slack —
> **mixing two budgets, which is precisely what rejecting `contentMargin` as a constant was about.**
> **One law, one budget.**

**And the remedy the gate prints is the load-bearing half and it is right: re-derive the box, never
shorten the name** — S84, because a generated string cannot be re-authored. **When it fires,
re-deriving from the new maximum re-establishes the ceiling AND the face budget in one move. The gate
is self-healing in the right direction.**

---

## T100-am3 — THE WIDENING'S COST, measured. Accepted, and it must not be tuned away.

**Measured against the 564-wide predecessor, same seed and beat:** the panel's right edge moved
**x ≈ 1452 → x ≈ 1551**, and **a pitch element visible at x 1516–1556 in the previous set is now
behind the panel** — the centre circle's left arc.

**Licensed:** §6.6 — *it expands over the column and stage.* **The stage is explicitly the panel's to
cover, and at 980 wide it covered all of it.**

> **RULED: accepted, and the right edge is a CONSEQUENCE of the ink budget, never a target.** **Tuning
> the width so it clears the centre circle would size a zone to its NEIGHBOUR rather than to its
> content** — §2's *derived, never chosen*, and the same arbitrariness `contentMargin` was rejected
> for. **The panel is as wide as its widest string requires, and where that lands is not a design
> decision.**

**And the offsetting half, which was not aimed at:** batch 89 found the panel reads by **occlusion**
rather than by its own field (2–3 luminance units). **It now occludes more, so it reads better as an
object than it did at 564.** **The widening bought legibility it was not trying to buy.**

---

## Standing, restated in one line each so this set closes cleanly

- **The 0px flush gap is unchanged and the batch-89 ruling stands: no gap.** An edge that cannot be
  seen cannot read as attached.
- **`CARDS` still carries the mark and no seed can fill it** — `_countLedger` carries one kind and
  resets per leg, so **two rows of three remains the panel's structural maximum.** **Unchanged by
  any of this, and still Allen's scope question from batch 87.**
- **These frames are `0 — 0` and are not used to re-read T99's four checks**, which live in their own
  dock and closed on their own evidence.

---

**Routing.** **T103, T102 CLOSED — Design-verified. T102-am and T100-am3 CLOSED — no build.**
**Nothing is owed at this seat.** **Open with Allen: the panel's scope (two rows of three), and
`T103`'s option (c) — keying the panel's rows to the ticket — recorded for the panel's next content
opening.**

**To Allen, in one line:** *the panel now agrees with the scorebug on both axes, its boxes are
derived from a rule the test reads off the build rather than restating, and the 0.7px anyone might
worry about is the distance to a warning that fires thirty pixels before anything could actually
break.*
