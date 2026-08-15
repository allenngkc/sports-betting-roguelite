# Register entries — 2026-08-15, batch 75

**THE DRAW ROW READ — three of five pre-committed checks pass, and the two that fail are one defect
with a one-constant fix.** Read at the DD seat against
`dd-import/surething-board-draw-row-2026-08-15/…-01-form-lobby-flat-1024x704.png`, measured at this
seat with its own instrument rather than read off the README.

**Destination table: SureThing — the laptop.** **Rows shipped:** `S74-am3` (the read) ·
`S81` (the board's density, measured for Allen — not ruled here).

**The submission is the standard again.** No read offered, every clause traced to source, provenance
checkable to the minute, and **five things explicitly not claimed**. **The one thing it says it is
for — *whether the draw's line scans as attached, detached, or as a separator* — is the thing this
batch answers, and the answer is neither of the three it anticipated.**

---

## S74-am3 — THE READ

| # | pre-committed check (batch 74) | verdict |
|---|---|---|
| 1 | the DRAW line reads as an **OUTCOME**, not a third team | **PASS** |
| 2 | the middle position reads as **between**, not third-in-a-list | **FAILS — measured** |
| 3 | `DRAW  {price}` sits in its 112px cell with its siblings' clearance | **PASS** on the frame |
| 4 | no team treatment leaks | **PASS** |
| 5 | the three cells read as **one column of three offers at one pitch** | **FAILS — same defect as 2** |

---

### 1 — PASS, and the reason is worth recording because it is what makes the empty column legal

**The blank subject cell does not read as a missing team.** The two columns answer two different
questions and their cardinalities differ *for a reason the frame makes visible*:

```
NOTARIES  4-5        AWAY  +123
                     DRAW  +261
FERRETS   5-4        HOME  +209
```

**The price column's three words are SELF-SUFFICIENT** — `AWAY`, `DRAW` and `HOME` each name an
outcome without needing a subject, which is exactly why S74-am's *"the price cell is the one that
names the OUTCOME"* was the right slot and why nothing had to be invented. **The subject column is
answering *who is playing*, and the answer is two names.** A reader looking for a third team would
have to believe the board had lost one; nothing on the frame invites that, because **the draw's
price is not sitting in a row that has a subject-shaped hole — it is sitting in a column that never
promised a subject per line.**

**Pre-commitment (3) of the README therefore does not fire.** The line reads neither as detached nor
as a separator. **No treatment is owed and no token goes in the matchup column.**

### 2 and 5 — THE DEFECT. The draw is not in the middle, and the frame says so in negative space.

**Measured at this seat off the flat 1024×704 render** (luminance profile across the price column;
the cell field is `LaptopOs.Ink` at ~20, the card ground at ~27, so the cells' own edges are
directly readable):

| | y | height |
|---|---|---|
| `AWAY` cell | **174 – 205** | 32 |
| **gap** | 206 – 208 | **3px** |
| `DRAW` cell | **209 – 240** | 32 |
| **gap** | 241 – 246 | **6px** |
| `HOME` cell | **247 – 278** | 32 |

**The gap above the draw is 3px and the gap below it is 6px — exactly double.** It reconciles to the
source: the three cells sit at card-local `−8`, `−43`, `−81`, so the centre pitch is **35 then 38**,
and the block pitch measures 116 on the frame as built.

**PROXIMITY IS DOING THE OPPOSITE OF WHAT THE RULING SAYS.** Negative space is what groups a column,
and a 2:1 ratio in the two gaps groups **AWAY + DRAW against HOME**. **S74-am's basis is that *the
middle position is LITERAL — attached to neither*. The frame shows it attached to the one above it.**
This is not a tolerance question: it is the one clause the composition rests on, failing in the one
channel that carries it.

#### The cause, named exactly — and it is an honest slip, not a misread of the ruling

**The build moved `HOME` down by the TEAM line's pitch (38) and inserted `DRAW` at the slot `HOME`
used to hold (−43). But the PRICE column's own pitch was never 38 — it was 35.** The two columns
have always run on different rhythms (`TeamLine` at −6/−44, prices at −8/−43), and with two items
each the 3px divergence was invisible. **Inserting a third item is what made the two rhythms
disagree on screen.** The commit's *"one more line is one more pitch"* is true of the team column
and not of the price column, and nothing in the ruling flagged that they differ.

#### RULED — the draw's cell is CENTRED IN THE SPAN ITS TWO SIBLINGS DEFINE

**Both siblings are pinned and neither may move**: `AWAY` at −8 and `HOME` at −81 each hold their
inherited offset to their own team line (2px and 1px, unchanged since before draws existed, and not
this item's business). **So the span is fixed at 41px between cell bottoms and tops, the cell is 32,
and the slack is 9.**

- **`DrawOdds` moves from `−43f` to `−44.5f`** — gaps **4.5 / 4.5**, symmetric.
- **If the build declines half-pixels, `−44f`** — gaps **4 / 5**. **Acceptable**: the residual 1px is
  smaller than the 2px the team and price columns already differ by, and it is the same direction.
- **Nothing else moves. Not the block pitch, not `HOME`, not the team lines, not the card's 3px
  bottom slack.** **A one-constant change, and it is the only geometry this read asks for.**

**Verifiable on the next frame with no further reading: the two gaps either match within a pixel or
they do not.**

### 3 — PASS on the frame; the C46 sweep is unchanged and still owed

Measured at the `AWAY` row: **the cell field spans x 462–573 and the glyphs span 472–563** — 10px
clear left, 10px clear right, and `DRAW  +261` and `HOME  +209` sit on the same bounds. `HOME  −115`
(block 03) also clears. **The frame shows generous clearance and asserts nothing about the widest
renderable string** — batch 74's two questions stand exactly as written, and this is one frame's
prices, not the population.

### 4 — PASS

No dot, no crest, no hue on the draw's line. T2's muted blue and pink stay on the two sides.

---

## 6 — ONE THING THE FRAME PRODUCED THAT WAS NOT PRE-COMMITTED: `MORE ›` has stopped straddling

**Measured:** `MORE ›` occupies **y 202–245** (74 × 44, centre 223.5). **The `DRAW` cell is y 209–240,
centre 224.5.** **The control now brackets the draw's cell and shares its centre line to within a
pixel.**

**It did not before.** At the 78px block, `MORE ›` sat at y 183–227 with its centre on 205 — **inside
the 3px gap between the two price cells, belonging to neither.** The block's centre used to be
whitespace. **Now the block's centre is a row, and a block-level control centred on the block lands
on it.**

**ACCEPTED, and the reasoning is recorded so it is not rediscovered as a defect:**

- **`MORE` is a COLUMN HEAD** — the board's own header reads `NO. · MATCHUP · SEASON RECORD ·
  MONEYLINE · MORE`, so the control sits under a heading that names its scope. **A control under a
  column head takes its meaning from the column, not from which row it happens to align with.**
- **It is treated as a different kind of object**: 74px against the price cells' 112, its own raised
  chrome, a chevron, one per block where prices come three to a block.
- **Centring a block-level control on its block is correct.** Moving it off-centre to avoid a
  coincidence would be an arbitrary offset with nothing to derive it from — **and an unexplained
  nudge is worse than a coincidence with a reason.**

**Re-read it if the block's line count changes again** — that is the condition under which this
acceptance expires, and it is the density question below.

---

## S81 — THE BOARD'S DENSITY: measured for Allen, NOT ruled here

Batch 74 put this to Allen ahead of the composition checks. **It stays with him. What this seat adds
is three measurements that were not available when the question was asked, and the third may change
the answer.**

### (a) What the frame actually shows

`BoardBody` runs y 166–670 (504px) at a 116px block pitch:

| block | y | shown |
|---|---|---|
| 01–04 | 166 – 630 | **complete** |
| **05** `LONGHAULERS` | 630 – 746 | **40px of 116 — its `AWAY +140` and nothing else** |
| 06 | 746 – 862 | **absent** |

### (b) The board never truncated a matchup before this change. Now it always truncates two.

**Six blocks at 78 is 468 against 504 — the whole slate fitted, with 36px to spare.** The list's
scroll existed for staged receipts (S25-am), **not because the matchups needed it.** **So the change
is not "the board scrolls a bit more"; it is "the board began scrolling."**

### (c) THE HAZARD IS THE PARTIAL BLOCK, and it is new

**Block 05 shows one of its three prices.** A matchup rendered as `LONGHAULERS 6-3 · AWAY +140` with
its draw and its home price below the fold is **a market presented as one offer** — the shape S24
refused in a different form, arriving here through geometry rather than through a layout decision.
**Before this change no block could be partial**, because none was ever cut.

### (d) AND THERE IS NO GEOMETRIC ESCAPE — which is the fact that decides what kind of question this is

**Six blocks in 504px requires a block pitch of 84.** **Three price cells at 32px are 96px before any
pad, gap, team line or rule.** **Six-visible is unreachable at the board's own price-cell size, by
arithmetic, at every possible layout.**

**So this is not a layout call and no yield order applies to it.** The levers that exist are all
outside this seat:

1. **Accept a scrolling board** — the slate stops being takeable-in-at-once.
2. **`MatchupsPerSlate` 6 → 4** (`engine/RunConfig.cs:40`) — restores see-it-all; **a game-design
   dial, and Allen's.**
3. **A smaller price cell** — that is the price face at 19px, and **§8's *no shrinking type* stands**;
   named only so the list is complete, not offered.

**RECORDED, NOT RULED. The measurement is delivered; the choice is Allen's, and (d) means he is
choosing between a scrolling board and a shorter slate, never between two layouts.**

---

**Routing.** **S74-am3 → surething-ui** (the file is the laptop's, and TV's branch is merged):
**`DrawOdds` −43 → −44.5, one constant, nothing else moves.** The C46 width sweep rides with batch
74's, unchanged. **S81 → Allen**, with (c) and (d) as the new information. **Checks 1, 3 and 4 are
CLOSED; 2 and 5 close on one re-shot block, and the two gaps either match within a pixel or they do
not.**

**To Allen, in one line:** *the draw's line is right in every way the ruling described and is three
pixels above where the ruling meant — it sits closer to AWAY than to HOME, which reads as a pair plus
one instead of three, and it is one number to fix; separately, three offers cannot fit six matchups
on that board at any layout, so the board either scrolls or the slate gets shorter.*
