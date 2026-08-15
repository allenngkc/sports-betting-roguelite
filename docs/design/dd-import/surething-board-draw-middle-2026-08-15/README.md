# The board's DRAW row, RE-SHOT on the corrected middle · 2026-08-15

**Ruling:** the DD's DRAW-frame read — **`−43 → −44.5`**, routed to this lane and built here.
**Built at:** `d618fcf` (`surething-ui-2`).
**Surface:** SureThing — the laptop, FORM lobby.
**Supersedes for the middle-position read:** `surething-board-draw-row-2026-08-15/` (same view, same
filename, pre-correction).

**NO READ IS OFFERED.** This set exists for the DD's read. It closes the two DRAW checks left open by
the previous dock: the middle-position disposition, and `DRAW {price}`'s entry into the C46
population.

---

## The frames

| file | what it is |
|---|---|
| `…-01-form-lobby-flat-1024x704.png` | **THE COMPARABLE.** The 1:1 canvas render — same view, same filename the previous set and S74-am were read on |
| `…-01-form-lobby-main-camera-1280x720.png` | **SUPPLEMENTARY**, same instant through the room camera. The harness writes the pair |

**This IS a true before/after pair**, which the previous dock explicitly was not. Its comparable was
ten days and a TMP migration older; this one differs from
`surething-board-draw-row-2026-08-15/…-01-form-lobby-flat-1024x704.png` by **one constant** and
nothing else — no other pixel moved between them, because Allen ruled the scrolling board stays
(slate six, pitch unchanged) and that was the only pixel the read moved.

### Provenance

Written by `SureThingVisualCaptureTests` in a filter-only run at **22:09:21**, off the tree at
`d618fcf`. No capture window was requested or spent. Build side-effects (ProjectSettings, two TMP
atlases) were reverted after the run and cannot reach the board's composition.

**Self-evidencing state, read off the frame:** `ROUND 1 OF 8` · `BANK $350` · `TARGET $60` ·
`TICKETS 0/3` · `SHEET 1 OF 1` · `MY MARKS — 0 SELECTIONS · 0 STAGED`. A fresh lobby, nothing marked,
so nothing in frame is a selection state.

---

## What changed, and why it is a derivation rather than a number

S74 rules the draw's middle position as **meaning** — *"the draw's line sits physically between the
two teams', attached to neither"*. The shipped constant did not deliver that claim:

```
AWAY  −8      gap to DRAW   35px
DRAW  −43     gap to HOME   38px      <- not the middle
HOME  −81
```

`−44.5` is exactly `(−8 + −81) / 2`, so the three cells are now **36.5px apart, evenly**:

```
AWAY  −8      gap   36.5px
DRAW  −44.5   gap   36.5px            <- the middle, by construction
HOME  −81
```

**It is written as the midpoint, not as −44.5.** `DrawCellY = (AwayCellY + HomeCellY) / 2f`, and all
four sites that place these cells read the constants — the three `MakeButton` calls and the biro
ring's own ternary. The ring's comment already named this hazard: *two elements agreeing by
convention rather than by construction*, the shape T95 caught on the TV. A literal moved in one place
and not the other is precisely that, so there is no literal left to move.

**Gated** by `Draw_price_cell_sits_exactly_between_the_two_team_cells`, which measures the **rendered**
cells rather than the constants — so a literal creeping back into any of the four sites fails — and
separately checks the ring did not stay behind at the old y.

---

## The C46 measurement, delivered — `DRAW {price}` joins the population

S74-am's own closing line left this owed: *"`DRAW` and its price are new strings in the canon face;
they measure against their cells like everything else and join the sweep's population under C46. That
sweep is the SureThing lane's instrument and was not run here."* It has now been run here.

Measured off the **rendered control** — its own font, size, tracking and cell width, never numbers
copied from the call site, because a cell's unstated assumption about its face is the whole of what
C46 is about.

| string | width | vs the 112px cell |
|---|---|---|
| widest on three independent boards (`DRAW  +253`, `+246`, `+240`) | 97.1px | **87%** |
| `AWAY  −341` — the existing comparable the cell was sized for | 91.4px | 82% |
| **format ceiling `DRAW  +10000`** | **115.5px** | **103% — would overflow** |

**Two things for the read, and the second is the one that needs a word.**

**(1) It fits today, and by less margin than the stamp forms are held to.** 87% clears the cell but
sits outside the ~20% headroom S77-am's rule reserves for exactly this — a face that measures wider
than the one the cell was sized against. `DRAW` is a wider word than `AWAY` at the same price length,
so the draw row is now the tightest string on the board.

**(2) The format can produce a string this cell cannot hold.** A five-digit price overflows at 103%.
**What is NOT established: whether a five-digit draw price is reachable.** Three boards produced
draws in the `+240 … +253` band, which is nowhere near it, and no sweep of the model's draw-odds
range was ordered or run. So this is a **representable** overflow, not a demonstrated one, and it is
named rather than left for a board that eventually prices one.

---

## NOT CLAIMED

- **No read of the middle position is offered.** Whether the corrected spacing scans as attached is
  the DD's call and this seat makes none.
- **Pre-commitment (2) is still not photographed.** Every generated matchup prices a draw, so the
  empty-line path remains proven in code and not in frame — unchanged by this correction.
- **The interior market list is untouched.** `MakeOfferRow` is a different surface and is not in
  these frames.
- **No claim that a five-digit draw price cannot occur** — only that none was observed in the sample
  measured, which is three boards.
