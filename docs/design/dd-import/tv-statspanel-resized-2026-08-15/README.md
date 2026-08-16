# The stats panel RESIZED to its content, clearing the scorebug · 2026-08-15

**Ruling:** DD batch 87 — *"a surface that takes the entire stage and returns three rows hasn't earned
the stage"* — resized once at design time to its content. **Placement: option (B), Allen** — the
panel's top drops **below** the scorebug band, so the scorebug is never covered at all.

**Supersedes for composition:** `tv-statspanel-countrow-2026-08-15`, which shows the same ticket and
the same beat at the **oversized** panel. That set is not wrong; it is the before.

**NO READ IS OFFERED.** The composition is the DD's.

---

## The geometry, derived from the C46 sweep

| | before | after |
|---|---|---|
| panel rect | `(0, 0, 980, 480)` | **`(0, 62, 564, 246)`** |
| area | 470,400 px² | **138,744 px² — 29.5%, a 70.5% reduction** |
| label column | 300 | **172** |
| value columns | 150 | **132** |
| colA / colB | 450.8 / 666.4 | **236 / 400** |

Boxes are **widest measured ink + one margin**, taken from the committed sweep
(`Evidence_C46_the_stats_panel_strings_against_their_boxes`): `MATCH STATS` 155.8 → labelW 172,
`Spreadsheets` 115.3 → valueW 132. **`pad` (32) is the only spacing value** on the panel — left inset,
both gaps, right inset, bottom inset.

**`contentMargin = 16` is the ONE invented number and it is UNRATIFIED.** The DD ruled the
resize-to-content *principle*, not this margin. It is load-bearing in code — `labelW`/`valueW` compute
from it rather than restating literals — **so ruling it differently changes one number, not a copied
derivation.**

## The scorebug is clear, and the pin says so on live rects

```
ScoreBug   (265,  0, 715,  62)     y 0  → 62
Panel      (  0, 62, 564, 246)     y 62 → 308
```

**Zero overlap on either axis.** `Stats_panel_does_not_cover_the_scorebug` asserts **full 2D**
non-overlap against the built rects — not a single-axis comparison, so a later change that only
narrows the vertical gap while the columns still cross horizontally is still caught as **the partial
coverage it would be**. *A half-covered scorebug is worse than a fully covered one.*

**Flagged, not decided — the gap is exactly 0px.** The panel's top edge sits flush on the scorebug's
bottom edge. That is the same idiom the panel already uses against `CashOut` and `EventStrip` (it ends
exactly where they begin), so it is consistent rather than novel — **but an opaque panel flush against
the scorebug may read as attached to it.** Whether the composition wants a gap there is the DD's.

## What this means for T99

**T99's licence is now MOOT, and its freeze is not.** T99 permitted covering the scorebug *conditional
on the freeze*; with nothing covered there is nothing left for that licence to permit. **The freeze
survives untouched** on PRD §8.8's own pausing clause (Allen, 2026-07-25; re-ruled 2026-08-15 as *time
stops*), which never depended on coverage.

**The standing condition at `SeatedDeltaTime` is deliberately KEPT even though it now guards nothing.**
It is written where a future re-enlargement would be made, and that is exactly when it stops being
vacuous. *Deleting a guard because it currently guards nothing is how the T95 class recurs.*

## The set

Same harness, same ticket, same beat as the before-set — `Capture_StatsPanel_WithAPopulatedCountRow`,
seed `STATS-COUNT-1`, frame-contiguous.

```
T100 condition met: corners 2-1   score='YAMS 0 — ZAMBONIS 0'   clock='18''
T100 rows :: 'GOALS|0|0' :: 'CORNERS|2|1' :: 'CARDS|—|—'
```

**5 frames of 70 docked** (183 MB whole): the frame before opening, the overlay's first / middle /
last, and the return.

## NOT CLAIMED

- **No read of the composition** — that is the whole point of the set.
- **`CARDS` still carries the mark, and no seed can fix it.** `_countLedger` is null off a count leg,
  carries exactly one kind, and resets per leg, so **two rows of three is the panel's maximum fill.**
  Unchanged by the resize and restated because the composition must be ruled against it.
- **The scorebug is `0 — 0`** in this set — as in the before-set. These frames are for the panel, not
  for re-reading T99's four checks, which live in `tv-statspanel-scorebug-2026-08-15`.
- **The vertical rhythm was not touched** — title at `-pad`, rows at `-(pad + 56 + i·46)`. Only the
  horizontal budget and the panel's own rect moved.
- **`contentMargin` unratified**, above.
