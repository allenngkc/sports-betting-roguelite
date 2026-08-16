# The stats panel: columns reordered, boxes re-derived at 80% ink · 2026-08-15

**Rulings:** DD batch 89 — **(1)** the column reorder (Allen), **(2)** `T102` re-derivation under
ink ≤ 80% of box, **(3)** `S84`'s binding that the value column sizes against the **enumerated** club
pool. Built as one bundle, shot on the same ticket and beat as its two predecessors.

**Supersedes:** `tv-statspanel-resized-2026-08-15` — same size story, but that set carries the **wrong
column order**. This is the one to read.

**NO READ IS OFFERED.** The composition is the DD's.

---

## (1) The reorder — and why it was invisible until now

The scorebug composes its line as `{away} {awayScore} — {homeScore} {home}` (`TvSweatScreen.cs:2404`),
so it reads **AWAY → HOME**. The panel rendered **HOME → AWAY**. At 2–1 the screen genuinely read
`YAMS 2 — 1 ZAMBONIS` above and `Zambonis 1 | Yams 2` below.

> **The resize did not create this — it revealed it.** While the panel covered the scorebug the two
> orders were never on screen together. Option (B) put them side by side, and the contradiction became
> visible the moment it could be seen at all.

**Fixed on every row together — headers, GOALS, CORNERS, CARDS.** Swapping the headers alone would
have put the right club names over the wrong numbers, which is a **state lie** and strictly worse than
the confusing order it replaced. The order is now written at the site as *the scorebug's*, citing that
composition line, so the two cannot be chosen independently again.

On the frames: scorebug `YAMS 0 — ZAMBONIS 0`, panel `Yams | Zambonis`, `CORNERS 1 | 2` against a
revealed home 2 / away 1. **Both axes agree.**

## (2) T102 — boxes re-derived under ink ≤ 80% of box

| | at +16 margin | **at 80% ink** |
|---|---|---|
| label column | 172 | **195** |
| value columns | 132 | **145** |
| colA / colB | 236 / 400 | **259 / 436** |
| panel width | 564 | **613** |
| panel area | 29.5% of original | **32.1%** (150,798 of 470,400 px²) |

**`MaxInkFraction = 0.8` is one named constant and `labelW`/`valueW` derive from it** — 195 and 145
are never restated as literals, so a further ruling moves one number. **`contentMargin` is removed
rather than left dead.** The resize win survives, exactly as the DD said it would: the panel is still
under a third of the stage.

Panel height and top placement are **unchanged** — the vertical rhythm was not in scope and was not
touched.

## (3) S84 — the pool binding, and it now GATES

| | |
|---|---|
| club pool | **20**, enumerated |
| widest | **`Spreadsheets` 115.3px** |
| box | **145.0px** |
| limit at 80% | 116.0px |
| **ratio** | **79.52%** |

`Stats_panel_value_column_holds_the_full_club_pool_at_max_ink_fraction` re-measures **every** club
through the panel's own rendered slot and asserts the widest clears the limit. It also asserts both
value columns are the same width, and it reads `MaxInkFraction` off production through a debug hook
rather than restating 0.8 — **the test and the surface cannot disagree about the rule.**

**It is deliberately NOT `[Explicit]`.** The C46 sweep is filter-only and therefore never runs in
routine suites, which is precisely how a 21st club could have overflowed unnoticed. This one gates.

**FLAGGED — the headroom is 0.7px.** 115.3 against a 116.0 limit is 79.52%, and that tightness is a
property of the rule itself: `ceil(115.3 / 0.8)` lands just above the widest string by construction.
**This is now the tightest gate on the surface**, and it will fire on any pool addition — which is its
job, but it means the box is re-derived rather than the name shortened, and the failure message says
so. Named so the DD is not surprised by it later.

## The set

`Capture_StatsPanel_WithAPopulatedCountRow`, seed `STATS-COUNT-1`, frame-contiguous, three bursts.

```
T100 condition met: corners 2-1   score='YAMS 0 — ZAMBONIS 0'   clock='18''
T100 rows :: 'GOALS|0|0' :: 'CORNERS|1|2' :: 'CARDS|—|—'
```

**5 frames of 70 docked** (182.6 MB whole): the frame before opening, the overlay's first / middle /
last, and the return.

## NOT CLAIMED

- **No read of the composition.**
- **`CARDS` still carries the mark and no seed can fill it** — `_countLedger` is null off a count leg,
  carries one kind, and resets per leg. **Two rows of three remains the panel's maximum fill**,
  unchanged by any of these three items and restated because the composition must be ruled against it.
- **The scorebug is `0 — 0` here**, as in both predecessor sets. These frames are for the panel; T99's
  four checks live in `tv-statspanel-scorebug-2026-08-15`.
- **The 0px flush gap** between the panel's top and the scorebug's bottom edge is unchanged and still
  with the DD.
- The vertical rhythm and the panel's placement were not touched by this bundle.
