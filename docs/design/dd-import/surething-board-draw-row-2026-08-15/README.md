# The board's DRAW row — S74-am built and on a frame · 2026-08-15

**Ruling:** `S74-am`, batch 65 (`register-entries-2026-08-14-batch-65.md`) — *"THE BOARD'S DRAW ROW:
the composition, ruled from the board's own grammar."*
**Built at:** `5724aa1` (`slice/tv-sweat-refinement`, pushed and remote-verified).
**Surface:** SureThing — the laptop, FORM lobby.

**NO READ IS OFFERED.** The one thing this set is for is the DD's read, and batch 65 pre-committed the
disposition for every way it can go (below), so the read costs one pass.

---

## The frames

| file | what it is |
|---|---|
| `…-01-form-lobby-flat-1024x704.png` | **THE COMPARABLE.** The 1:1 canvas render — the same view and the same filename S74-am was ruled on |
| `…-01-form-lobby-main-camera-1280x720.png` | **SUPPLEMENTARY**, the same instant through the room camera. Included because the harness writes the pair; the desk-distance question is not what pre-commitment (3) asks |

### Provenance, and it is checkable rather than asserted

Written by `SureThingVisualCaptureTests` **inside the verification PlayMode run**, not by a capture
window — no window was requested and none was spent. Run **03:03:39 → 03:09:00**; these frames
**03:05:51**. The two source files this build touched were last edited **02:55:59** and **02:57:54** —
*before* the run — and committed at **03:12:15**, unchanged in between. **So the frames are of exactly
the source that is in `5724aa1`.** The only files touched after the run were the reverted build
side-effects (ProjectSettings, two TMP font atlases, `SBR.Engine.dll`), none of which can reach the
board's composition.

**Self-evidencing state, read off the frame itself:** `ROUND 1 OF 8` · `BANK $350` · `TARGET $60` ·
`TICKETS 0/3` · `SHEET 1 OF 1` · `MY MARKS — 0 SELECTIONS · 0 STAGED`. A fresh lobby with nothing
marked, so nothing in frame is a selection state.

---

## What is in frame, clause by clause

```
NO.  MATCHUP · SEASON RECORD        MONEYLINE      MORE
01   NOTARIES   4-5                 AWAY  +123
                                    DRAW  +261    MORE ›
     FERRETS    5-4                 HOME  +209
```

| clause of S74-am | in frame |
|---|---|
| `DRAW` goes in the **PRICE CELL**, the cell that names the OUTCOME | **yes** — same column as `AWAY`/`HOME` |
| The **matchup column is EMPTY** on that line | **yes** — nothing beside `DRAW +261` |
| The middle position is **literal** — between the two teams, attached to neither | **yes** — the line sits between `NOTARIES` and `FERRETS` |
| **No team treatment** — no dot, no crest, no hue | **yes** |
| `MORE ›` **spans the block**, unchanged, now three lines | **yes** — centred on the draw's line |
| `MONEYLINE` **stands** as the column header | **yes** |

**Pre-commitment (1) FIRES, and the frame shows it:** every visible matchup prices a draw — `+261`,
`+243`, `+281`, `+293`. Traced in code as well: `DrawOdds` is set by **slate generation**
(`SlateGenerator.cs:91`, once the latents are known — the 1X2 triple cannot be priced before the
distributions exist) and by **neither `Matchup` constructor**. So on any generated board the block is
three lines, uniform, and **this closes with no further ruling.**

## The OWED measurement, delivered

S74-am left the block height and visible count owed and eyeballed them as *"six blocks today and about
four at three lines."* **Measured:**

| | before | after |
|---|---|---|
| line pitch inside a block | **38px** — the gap between the two `TeamLine` calls, not a number invented for this | unchanged |
| block pitch (`MatchupCardPitch`) | 78px | **116px** = 78 + one line pitch |
| list area (`BoardBody`, title strip excluded) | 530 − 26 = **504px** | unchanged |
| blocks fully visible | 504/78 = 6.46 → **6** | 504/116 = 4.34 → **4** |

**The measurement agrees with the DD's read off the frame**, and the frame agrees with the
measurement: blocks `01`–`04` are complete and `05` (`LONGHAULERS`) is cut at the viewport edge.

A fixed grid constant **re-derived once at design time is explicitly legal** (§2, T51, S40); a zone
resizing to content at runtime is not — so the block is 116px whether or not a matchup prices a draw.
**AWAY does not move at all, DRAW takes the slot HOME used to hold, and HOME moves down exactly one
pitch**, so the card's 3px of bottom slack is preserved by construction (81 + 32 = 113 against 116, as
43 + 32 = 75 was against 78). **A pure insertion, not a re-layout.**

**C19 is not breached and §2's yield order was not invoked.** The list scrolls (S25-am) with S27's
printed position rail — visible at the board's right edge — so every priced offer stays reachable by a
mechanism that already existed. Nothing was deleted to make a layout fit.

---

## THE DISPOSITIONS, pre-committed by batch 65 — so the read costs one pass

1. **Every match prices a draw** → three lines, uniform. **This is what fired.** Closed.
2. **Some match prices no draw** → the block is **still three lines** and the draw's line renders
   empty, because a ragged board whose block height depends on the market is a zone resizing to
   content. Built and reachable (a hand-built matchup carries `DrawOdds` 0), **but not exercised on
   these frames** — see the non-claims.
3. **The blank matchup column makes the draw's line read as DETACHED or as a SEPARATOR** → the remedy
   is **the price cell's own treatment**, which already carries the word `DRAW` — **never a token in
   the matchup column.** *This is the read this set exists for.*

## NOT CLAIMED

- **No read of (3) is offered.** Whether the draw's line scans as attached, detached, or as a
  separator is a design call and this seat makes none.
- **Pre-commitment (2) is not photographed.** Every generated matchup prices a draw, so the empty-line
  path is proven in code and **not** in frame.
- **Fit is not asserted** — S74's own closing line. `DRAW` and its price are new strings in the canon
  face; they measure against their cells like everything else and **join the sweep's population under
  C46**. That sweep is the SureThing lane's instrument and was not run here.
- **This is NOT a frame-locked before/after pair.** The newest lobby frame already docked is
  `surething-ledger-resubmit-2026-08-05/…-01-form-lobby-flat-1024x704.png` — **ten days and a TMP
  migration older**, so it differs by far more than the draw row. Comparing them would attribute the
  whole interval to this change. A true before would need the constant reverted and a second run;
  none was ordered and none was spent.
- **The interior market list is untouched.** S74-am rules the lobby board; `MakeOfferRow` is a
  different surface and is not in these frames.

## Boundary, stated because it is not this worktree's surface

`SportsbookApp.cs` is the **laptop**, which the tv-sweat contract lists as never-touched, and batch 65
names SureThing as the destination table. Built here on **Allen's explicit assignment**. The
`surething-ui-2` lane is live with unmerged commits in this same file — measured at lines **801+**
against this change's **243–300**, so **no overlap** — and **must merge main before its next push.**
