# T100 — the stats panel with a POPULATED count row · 2026-08-15

**Ruling:** `T100`, batch 85 — *"one frame with a POPULATED count row … then the composition is ruled
on what it actually holds rather than on what this seed happened not to reveal."*
**Harness:** `Capture_StatsPanel_WithAPopulatedCountRow`, seed `STATS-COUNT-1`.

**NO READ IS OFFERED.** The composition is the DD's, and this set exists so it is ruled on a filled
table rather than an empty one.

---

## The rows, as shot

```
GOALS   | 0 | 0
CORNERS | 2 | 1        <-- populated
CARDS   | — | —
```

**The ticket carries a CORNERS leg**, and the selection was **read off the board** rather than
constructed — the corners line is generated per matchup, so an invented `TotalCorners(9.5, over)`
would be a selection this matchup may not offer. The run **waits** for the count ledger to reveal
something and **fails rather than shooting the empty form a second time**.

| burst | frames | clock | score |
|---|---|---|---|
| `countrow-closed-before` | 20 | 18 → 20 (running) | `YAMS 0 — ZAMBONIS 0` |
| `countrow-open` | 30 | **20, and only 20** | same |
| `countrow-closed-after` | 20 | 20 | same |

T99's standing condition holds here too and is asserted in the harness: **thirty contiguous frames on
one clock value.**

**Docked 5 frames of 70** (177.3 MB whole) — the frame immediately before opening, the overlay's
first / middle / last, and the return. `MANIFEST-all-70-frames.txt` carries every frame plus the
harness's own row dump.

---

## THE ONE THING TO KNOW BEFORE RULING COMPOSITION

**`CARDS` still carries the mark, and no seed can fix that.** T100 asks for *"real values in every
row"*, and **that is not reachable on one leg by construction:**

> `_countLedger` is **null unless the live leg is a corners or cards leg**, and when it exists it is
> **configured for exactly ONE of them** (`ConfigureEndpoint(statLine, kind, beatCount)`). It is also
> **reset per leg**.

So a corners leg fills `CORNERS` and leaves `CARDS` empty; a cards leg does the reverse. **A ticket
carrying both still shows only one at a time, because only one leg is live at any instant.** This set
is therefore **the maximum fill the panel can currently reach: two rows of three.**

**This is the same structural fact TV reported when the row set was fitted** — §4D's *"per-team
corners/cards are available"* is true only inside a count leg of that kind. It is stated here because
**the composition would otherwise be ruled against a fill the surface cannot produce.**

**Not proposed, not designed** — whether the panel should carry counts across legs, or show only the
live leg's count, or say something else in the empty row, is composition and it is the DD's.

## NOT CLAIMED

- **No read of the composition.**
- **This set's scorebug is `0 — 0`**, so it must NOT be used to re-read T99's four checks — T99's
  binding condition was a non-level scoreline and that set (`tv-statspanel-scorebug-2026-08-15`) is
  the one those checks belong to. Here the subject is the table, not the covered band.
- **`GOALS 0|0` is revealed data, not the mark** — it is the honest revealed score of a goalless match
  so far, and it is what distinguishes a populated zero from an unrevealed row.
- **The panel's strings are still unswept under C46** (T101's second item), queued behind this.
- One seed, one ticket, one leg.
