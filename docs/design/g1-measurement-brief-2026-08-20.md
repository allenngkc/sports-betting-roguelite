# `G1` — the nine kinds, to TV for measurement

**Written:** Design Director seat, 2026-08-20 · **Authored at:** batches 137 (`T151`) and 138 (`T152`)
**For:** the TV lane, behind the `T147-am` composition build. **Dispatch carried by Allen.**
**Pattern:** `T111` — *the lane measures and this seat rules.* **Nothing here asks for a design call.**

---

## 0. WHY THIS CANNOT BE MEASURED AT THE DD SEAT

**`C58`.** The offline `hmtx` route is valid only where the font file's default instance is the
shipped one. `tools/ttf_faces.py` records Encode Sans as *"worse — its axis defaults are wght=100
wdth=75… the default is wrong on BOTH axes."* **Any offline width for this face would be measured
against a face the surface does not use** — `C46`'s founding defect, committed by the instrument
meant to catch it.

**Measure in the editor, on the built objects, at the real face.**

---

## 1. THE GOOD NEWS FIRST — the TV shortens clubs, and it changes the risk

`SweatFlavor.Short()` and `SweatActiveLegModel.Surname()` **both take the last word only**
(`SweatFlavor.cs:411-415`, `SweatActiveLegModel.cs:594-599`).

| | widest constructible | chars |
|---|---|---|
| club, TV short form (the noun alone) | `SPREADSHEETS` · `GRAVEDIGGERS` | **12** |
| player surname | `PAVEMENT` | **8** |

**So the laptop's 26-character champion never reaches this surface.** The nine new forms are far
shorter than `S96-am2`'s worst case — **but that is a reason to expect a pass, never a reason to skip
the measurement** (batch 95: the widest string in a column is a MEASUREMENT, never read off a
character count, and this seat has been corrected on it).

---

## 2. WHAT NEEDS NO NEW MEASUREMENT — stated first so it is not swept twice

**Five of the nine add NO progress string at all.** These reuse shipped forms verbatim and their
widths are already in the sweep:

| kind | progress line |
|---|---|
| DoubleChance | the moneyline's `LEADING {score}` / `TRAILING {score}` / `LEVEL {score}` |
| TeamTotalGoals · TeamTotalCorners · TeamTotalCards | `DescribeCount`'s `{n} {NOUN} • NEED {m}` / `• LIMIT {m}` / `• WON` / `• LOST`, on that team's count |
| PlayerMultiScorer | `DescribeCount`'s, on the player's goals |

**Only these progress strings are new:** `MET` (3) · `NOT YET` (7, already shipped for AnytimeScorer)
· `CLEAR BY {n}` · `TRAILING BY {n}`. **All four are short and none is expected to bind** — measure
them anyway; they are cheap.

---

## 3. THE FORMS TO MEASURE, WITH THEIR CHAMPION CANDIDATES

**Champions are CONSTRUCTED from the closed pools** (`S84`, `S96-am` — the pool, never the seed's
champion). **They are candidates to measure, not a claim about which is widest.**

### 3.1 Authored at `T152` (batch 138)

| kind | form | champion candidate | chars |
|---|---|---|---|
| **DoubleChance** | compact | `GRAVEDIGGERS OR DRAW` · `EITHER TEAM` | 20 · 11 |
| | NEED | `GRAVEDIGGERS TO WIN OR DRAW` · `A WINNER AT FULL TIME` | 27 · 21 |
| | fallback | `GRAVEDIGGERS WIN OR DRAW` · `A WINNER AT FT` | 24 · 14 |
| **Handicap** | compact | `GRAVEDIGGERS -1.5` | 17 |
| | NEED | `GRAVEDIGGERS WITHIN 1 GOAL` · `GRAVEDIGGERS TO WIN BY 2+` | 26 · 25 |
| | fallback | `GRAVEDIGGERS WITHIN 1` · `GRAVEDIGGERS BY 2+` | 21 · 18 |
| | progress | `TRAILING BY {n}` · `CLEAR BY {n}` | ~13 |
| **TeamTotalGoals** | compact = NEED | `GRAVEDIGGERS UNDER 1.5 GOALS` | 28 |
| **TeamTotalCorners** | compact = NEED | **`GRAVEDIGGERS UNDER 4.5 CORNERS`** | **30** |
| | fallback | `GRAVEDIGGERS UNDER 4.5 CNRS` | 27 |
| **TeamTotalCards** | compact = NEED | `GRAVEDIGGERS UNDER 1.5 CARDS` | 28 |
| **PlayerMultiScorer** | compact | `PAVEMENT 2+` | 11 |
| | NEED | `PAVEMENT TO SCORE 2+` | 20 |

### 3.2 Authored at `T151` (batch 137)

| kind | compact | NEED | fallback | progress |
|---|---|---|---|---|
| **CorrectScore** | `EXACT 3-1` | `3-1 AT FULL TIME` | `3-1 AT FT` | `MET` / `NOT YET` |
| **TotalGoalsOddEven** | `TOTAL EVEN` | `EVEN TOTAL AT FULL TIME` | `EVEN TOTAL AT FT` | `MET` / `NOT YET` |
| **WinningMargin** | `MARGIN 3+` | `3+ GOALS APART AT FULL TIME` | `3+ GOALS APART AT FT` | `MET` / `NOT YET` |

### 3.3 The one to look at first

**`GRAVEDIGGERS UNDER 4.5 CORNERS` — the team totals are the width risk, and they are the ones I
called *"cheaper by construction."*** They are cheap to **author** (zero new progress strings) and
they are the **widest to measure**. Against the shipped NEED band's current occupants —
`GRAVEDIGGERS TO WIN` (19) and `ONE TEAM SCORELESS` (18) — **30 characters is meaningfully wider
than anything the band carries today.**

**Enumerate the reachable LINES rather than taking mine.** I wrote 4.5 for team corners and 1.5 for
team goals and cards from a single frame; the engine's offered ladders are the authority and this
seat did not read them.

---

## 4. THE BOXES

- **compact row** — 249.0px (`T111-am`'s figure for `LegRowProgress0`; confirm it is the same rect
  the compact renders in).
- **NEED band** — `T90` ruled its width belongs to the FACT rather than the furniture; take the
  measured figure, not the authored constant.
- **Both are the ROW's, and `T147-am` is changing the row's HEIGHT** (`TicketRowSlots` 6 → 4,
  `TicketFooterHeight` 40 → 60). **Widths are unaffected — but if the composition changes the NEED
  band's width for any reason, this measurement is void and must re-run.** Stated so it is checked
  rather than assumed.

---

## 5. WHAT TO REPORT

1. **Every form's width against its box**, with fits/overruns and the spare.
2. **Which forms take the ladder's second rung**, selected BY MEASUREMENT (`FitOrFallback`), never by
   authoring intent.
3. **Any form with no adequate second rung** — that returns here for authoring rather than being
   truncated (`T69`) or shrunk.
4. **The blind spot** (`C18 §4.2`): what the sweep did not cover and why. In particular, say whether
   the reused progress lines were re-measured on the TEAM-scoped and PLAYER-scoped counts or only on
   the match-scoped ones — **they are the same strings with different numerators and I do not know
   that the pools cover them.**

---

## 6. TWO BINDINGS THAT MUST NOT BE SKIPPED

- **`S84`/`S96-am`** — the pool, never the seed's champion. The candidates in §3 are constructed;
  the sweep should construct its own and report if it finds a wider one than mine.
- **`C57-am`** — **the pool follows the DECK, not the build.** These nine forms are authored and
  unbuilt. **They belong in the sweep's pool now**, exactly as `SweatFlavor`'s unreachable Under
  cells do, and for the same stated reason: *a pool that includes a not-yet-reachable string is
  caught the moment anyone looks at it; a pool that omits one is invisible until the day it becomes
  reachable and nobody re-swept for it.*

---

## 7. NOT ASKED FOR

- **No design call.** Every string above is ruled; the measurement decides only which ladder rung
  renders.
- **No build.** `T152`/`T151` are authored, not implemented, and the composition build has priority.
- **No fix for `T153`** — the existing count grammar's `1 GOALS` is raised, not ruled, and must not
  ride this pass.
