# THE PHASE-CLOSING FRAME — a SINGLE-COUNT ticket, and the blank slot · 2026-08-15

**Ruling:** DD batch 94 — *"one frame closes the phase: a single-count ticket, the state the blank-slot
remedy is about."* Titled `COUNTS` in the same pass.
**Harness:** `Capture_StatsPanel_WithAPopulatedCountRow` (unchanged), seed `STATS-COUNT-1`, ticket =
ONE corners leg, frame-contiguous, three bursts.

**NO READ IS OFFERED.**

---

## The state, on a frame for the first time

```
COUNTS
GOALS   | 0 | 0
CORNERS | 1 | 2
        |   |          <-- the CARDS slot: BLANK, not marked
```

Row dump, from the harness's own log: `'GOALS|0|0' :: 'CORNERS|1|2' :: '||'`

**The third row is empty strings, not the unrevealed mark.** That distinction is the batch 93 ruling
made visible: **absent means the ticket never bought it; the mark means bought but not yet revealed.**
No earlier set shows this — every prior shot either predates the keying or used a multi-count ticket,
where all three rows exist.

**This is the state the blank-slot question is about**, and it is the commonest shape after a
moneyline ticket: one count leg, one blank slot, in a panel whose height is fixed at build time.

## The title, and the box re-derived from measurement

**`COUNTS`** replaces `MATCH STATS`, which overstated the subject once the panel became ticket-keyed.

**The box was re-derived from a fresh sweep, not from an estimate** — and the subtlety is worth
recording, because the column holds four strings at TWO type sizes:

| slot | string | size | measured |
|---|---|---|---|
| title | **`COUNTS`** | 19px | **88.5px** ← binds |
| label | `CORNERS` | 15px | 81.2px |
| labels | `GOALS` / `CARDS` | 15px | 56.6px |

**The title still binds — but by only 7.3px over `CORNERS`.** A row label at a much smaller size came
within a hair of overtaking it, so the widest string in this column is a MEASUREMENT, never something
readable off string lengths or type sizes.

| | before | after |
|---|---|---|
| labelW | 195 | **111** |
| colA / colB | 259 / 436 | **175 / 352** |
| panel width | 613 | **529** |
| area vs the original 470,400 px² | 32.1% | **27.7%** (130,134 px²) |

**This seat's earlier illustrative rows were WRONG and are withdrawn.** They tabulated ink 100 → 543
and ink 60 → 493 as though `labelW` tracked the title alone. It does not — the column must also hold
the row labels, so those widths were unreachable. The DD struck them correctly.

**The measurement also disagrees with the DD's own estimate, and the measurement is what shipped:**
batch 94 expected a floor near 99px of ink and a panel near 542px. Measured **88.5px** and **529px** —
about 10px and 13px under. Reported rather than reconciled; a number in a ruling is an expectation,
and the sweep is the instrument.

Panel height is unchanged at 246. `valueW` is untouched at 145.

## The set

**5 frames of 70 docked** (183.7 MB whole) plus `FRAME-INVENTORY-all-70.txt`. Frames carry their own
seed, boost, scene index, grammar, moment and frame index.

## NOT CLAIMED

- **No read of the composition, or of the blank slot.**
- **The moneyline-ticket state (one row in three) is still not photographed** — this set shows the
  ONE-blank-slot case, not the two-blank case. Described from the build and pinned; a frame of it
  would be a further shot.
- **The 0px flush gap** at the scorebug's bottom edge is unchanged and still open.
- Suites green whole: engine 306/306, EditMode 255 (254/0/1), PlayMode 124 (113/0/11), with every
  `Stats_panel_*` pin passing by name.
