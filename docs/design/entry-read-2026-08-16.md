# ENTRY at HEAD — the read

**Written:** Design Director seat, 2026-08-16 · **Against:** `dd-import/entry-at-head-2026-08-16/`
(6 states, flat 1024×704) · **Pre-commitment:** `capture-precommit-2026-08-16.md` §2

The capture met binding condition 1.1 properly — S27's rail was measured per tab **at the shutter**,
so "nothing else is on this tab" is a property of the frame rather than an inference from one
screenful. Four tabs produced no rail; PLAYERS did and was shot at both ends.

---

## 1. THE C19 QUESTION — ANSWERED, AND IT IS A FAIL

**Fifteen `MarketKind` members. Six have a reachable home. Nine do not.**

| kind | home | |
|---|---|---|
| Moneyline | **FORM board** (`AWAY`/`DRAW`/`HOME`) | reachable |
| TotalGoals | GOALS — `OVER/UNDER 1.5 · 2.5 · 3.5` | reachable |
| BothTeamsToScore | BTTS — two rows | reachable |
| TotalCorners | CORNERS — `OVER/UNDER 8.5 · 9.5 · 10.5` | reachable |
| TotalCards | CARDS — six rows | reachable |
| AnytimeScorer | PLAYERS — ~15 rows, scrolls | reachable |
| **DoubleChance** | — | **none** |
| **Handicap** | — | **none** |
| **TeamTotalGoals** | — | **none** |
| **CorrectScore** | — | **none** (12–16 rows by its own doc comment) |
| **WinningMargin** | — | **none** |
| **TotalGoalsOddEven** | — | **none** |
| **TeamTotalCorners** | — | **none** |
| **TeamTotalCards** | — | **none** |
| **PlayerMultiScorer** | — | **none** — PLAYERS carries only `ANYTIME`, verified at scroll extent |

**I predicted eight and it is nine.** I assumed `PlayerMultiScorer` would sit under PLAYERS; at the
list's scroll extent every row reads `{NAME} ANYTIME {POSITION}` and none is a multi-scorer.
Recorded under §1.5 — the miss is mine, and it is the direction that matters: **the gap is wider
than the seat guessed, not narrower.**

**Measured offer count on ENTRY: 6 + 2 + 6 + 6 + ~15 = ~35**, plus the board's three moneyline
prices. Against the mandate's *"~80 offers per matchup"* — **a figure I have not verified myself** —
that is roughly half the priced slate with nowhere to be shown.

**This is C19 on frames**, and it is no longer a source read: *an offer the engine prices is
reachable on the surface; hiding it misrepresents the slate.* It is the debt the markets lane
shipped knowing surfaces were owed, so it is expected — but it is now measured, and it sets the
phase's floor. **The presentation pass is how nine market kinds get a home.**

## 2. DENSITY — MEASURED, AND MY OWN PRE-COMMITMENT WAS WRONG

| measurement | value |
|---|---|
| offer row pitch | **54px** |
| row ink height | 14px |
| market body viewport | y ≈ 250–672, **~422px** |
| **rows per screen** | **~7.8** |
| GOALS / CORNERS / CARDS | **6 rows used** — ~1.8 rows of slack |
| BTTS | 2 rows used — ~5.8 rows of slack |
| PLAYERS | ~15 rows — **scrolls at 1.88× viewport** (rail x 696, thumb 225px on a 422px track) |

**§2.2 of my pre-commitment expected "substantial unused vertical space." That is wrong.** Three of
the five tabs are close to full at six rows; one scrolls; only BTTS is roomy. The sheet is not
spacious, and the phase cannot be planned as though it were.

**The researcher's arithmetic was wrong by ~2.7×** and I was right not to carry it in: it assumed a
26px pitch and the rendered pitch is **54px**. At 54px, ~80 offers is **~10 screens, not four.**
This is batch 95's lesson exactly — *the widest string is a measurement, never something readable
off type sizes* — arriving on row pitch instead of column width.

## 3. THE STRUCTURE CHANGED AT THE MIGRATION, AND THE STALE FRAME MISLED ON IT

The `s6-s8` frame showed **two-up pairs** — `OVER 1.5` left, `UNDER 1.5` right, sharing one row.
That is gone. At HEAD **every offer is its own full-width row**, market name left, price right.

**Six offers took three rows before and take six now.** Whatever the migration's merits, the sheet's
offer density **halved**, and nothing in the register records that as a decision. Flagged, not
ruled: it may be a deliberate consequence of the TMP work or an unnoticed one, and the difference
matters because §1 needs those rows back.

Three things confirmed at HEAD, against §2.3's checks:

1. **Two-way markets no longer share a row** — check 1 is REFUTED, see above.
2. **The price is bare right-aligned type** — no field, no rule beneath, not amber. The stale
   frame's ruled price cells are gone.
3. **The tab rail is a single level.** It must stay one — the research is unambiguous that the
   double-tiered rail is DraftKings' most-criticised feature.

## 4. WHAT THE EVIDENCE SETTLES ABOUT THE THREE MATERIAL CALLS

The calls remain Allen's. The frames change what they cost.

- **§5.3 — invert the price-first hierarchy.** **Largely already true and it was not recorded as a
  ruling.** ENTRY leads with the market name in the typeset layer and sets the price as plain
  right-aligned type. The inversion I proposed as a change is mostly the build's existing behaviour;
  what is *missing* is the amber annotation layer — the price is not money-coloured. So the call
  shrinks from *"invert the hierarchy"* to **"ratify the hierarchy the build already has, and decide
  whether the price takes the wax."**
- **§5.1 — print the empty groups.** Cheap here: BTTS already has 5.8 rows of slack, and a contents
  block would sit in it.
- **§5.2 — the folio and contents block.** The rail is verified working on PLAYERS at 1.88×, so the
  mechanism exists and is measured. But at ~7.8 rows per screen, **nine destinations at six-plus
  rows each will mean most tabs scroll**, which makes the folio more valuable, not less.

## 5. THE ARITHMETIC THE SPEC MUST START FROM

Nine homeless kinds must land somewhere. The rail holds **about nine destinations** before it
overflows (authored tab origins ~104px apart against ~996px — arithmetic on constants, still
unmeasured). Nine destinations × ~7.8 rows = **~70 row-slots**, and `CorrectScore` alone wants 12–16.

**So the vocabulary does not fit without scrolling, and that is fine:** `S25-am` already rules that
every interior market list scrolls with S27's rail, `S81-am` measured the rail proportional to the
pixel, and PLAYERS demonstrates it working today. **The answer is more destinations plus scrolling
within them — not denser rows**, because §8's no-shrinking-type stands and the 13px floor is law.

## 6. NOT CLAIMED

- No claim about a different slate. One board, one fixture, one round.
- **No measurement of the rendered product-fact size against the 13px floor.** The 54px pitch and
  14px row ink are measured; the type size itself is not, and §2.2 asked for it. Owed.
- The ~80-offers figure is the mandate's, not mine. What I measured is ~35 on ENTRY.
