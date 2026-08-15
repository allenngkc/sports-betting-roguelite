# The slip's scroll, on the state that ACTUALLY scrolls · 2026-08-15

**Ruling:** `S83` (option C, the margin's three zones) · **capture ordered by** DD batch 81.
**Built at:** `0dbdd62` · **shot at** the additive-gesture build on `surething-ui-2`.
**Surface:** SureThing — the laptop, FORM lobby, working margin.

**NO READ IS OFFERED.** The scroll's Design-verification closes on these frames.

---

## The state, and why it is this one

**Four legs plus a held consumable.** Batch 81's order was exact: a non-scrolling capture proves
nothing about a scroll, and **four legs ALONE does not scroll** — measured, 168.0 of content into a
177.9 viewport, clearing by 9.9px. The modifiers row is the 34px that puts it over.

| | |
|---|---|
| zone 2 content | **202.0px** |
| zone 2 viewport | **177.9px** |
| scrolls by | **24.1px** |

The consumable is **granted** by the capture rather than hoped for — the row is gated on pure run
state. The run is pinned to the same seed as the max-legs frame, so the board underneath is the one
that set is already read against.

**The capture asserts its own premise** before shooting: `ScrollRect.vertical` is true and S27's
`RailTrack` is drawn. A frame that had silently caught the non-scrolling case would be exactly the
evidence batch 81 called worthless.

---

## What the frame shows, and where to look

Read down the margin: `MY MARKS · 4 SELECTIONS · 0 STAGED`, four leg rows, `COMBINED +5640` — and
then **a pale bar with no label, immediately above `STAKE`.**

**That bar is the FREE BET row, clipped by the viewport boundary, and it is the scroll.** The
arithmetic says exactly how much of it should be visible:

```
legs                    0 … 140
COMBINED row          140 … 168
FREE BET row          168 … 198     <- the viewport ends at 177.9
                                       so 9.9px of this row is on screen
```

**9.9px of a ~30px control** is what a row cut by a scroll boundary looks like, and it is the same
9.9px the four-legs-alone state has as clearance — the harvest from option A, showing up as the
distance the boundary moved.

**The commit zone below it is untouched by the scroll**, which is the clause the option was argued
on: `STAKE $35`, the fraction chips, the nudge keys, `POTENTIAL PAYOUT $2,009` and `PLACE TICKET`
are all anchored. **The two figures the commit is about are on screen while PLACE is** — S17/S73's
cost-he-cannot-see, answered structurally rather than by hoping the content is short.

`LOCK IT IN` reads `PLACE OR CLEAR THIS WORKING SLIP`, and the run is pre-place, so nothing in frame
is a settled state.

---

## What this does NOT show, stated rather than implied

- **The scrolled-to-bottom view.** The scroll rests at the TOP by ruling (one behaviour with the
  board, S25-am/S27), so the frame is the resting state. No frame of the body scrolled down was
  ordered and none is offered.
- **The rail's thumb is 4px wide** (`RailReserve`) against a 324px panel, so it is present and
  asserted in code but effectively invisible at this render scale. **The clipped row is the legible evidence
  of the scroll in this frame; the rail is not.**
- **The relation statement is not in frame.** It renders in zone 2 with the legs, but this state has
  no same-match group — four legs across four matchups. The statement's own scrolling behaviour is
  unphotographed.
- **No read of whether the clipped row reads correctly.** Whether a control cut at 9.9px scans as
  "there is more below" or as an artifact is the DD's call and this seat makes none.
