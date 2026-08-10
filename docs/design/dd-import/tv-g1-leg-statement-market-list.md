# G1 — the TV's leg-statement market list and measured columns

**From:** TV sweat lead · **For:** G1 (batch 17), the authored short forms · **No editor used**
**Build:** `112df65`, the batch-16 chain. Measurements from `seed-27182818`, seated acceptance view.

---

## 0. First thing: there are TWO statement strings per leg, not one

G1 asks for "the authored statement string for every market". **The TV renders two different
statements per leg, from two different sources, into two differently-sized boxes.** T69's escalation
— `RICO LANYARD TO` ending on a preposition — was about the **live** one. Authoring only one of them
would leave the other as it is.

| | source | when it shows | box |
|---|---|---|---|
| **NEED** (live row) | `SweatActiveLegModel.DescribeActiveLeg` → `copy.Need` | while the leg is live | **249px @ 28px** |
| **compact** (resolved / next / pending row) | `MatchModel.DisplayLabel`, re-authored on this surface by `LegStatement` | every other row state | **143px @ 15px** |

The compact box is the tight one and it is not close.

## 1. The measured columns

Canvas px, from `LayoutGrid(980, 550)` and the row builder — design-time constants, not eyeballed.
Ticket column 265 wide; rows `lineW = 265 − 16 = 249`.

| element | width | type | face | weight |
|---|---|---|---|---|
| **NEED** (live statement) | **249px** | 28px | Encode Sans Condensed | Bold |
| progress (beneath NEED) | 249px | 19px | Condensed | Normal |
| **compact statement** | **143px** | 15px | Condensed | Bold |
| — price column | 52px | 15px | | reserved |
| — state chip | 38px | 15px | | reserved |

The compact statement gets `249 − 38 chip − 52 price − 16 gaps = **143px**`. Those reservations are
fixed by §6 (no geometry from content) so the column's right edge cannot go ragged across six rows.

## 2. The empirical budget — what actually fit

Measured ink extents on the frame, so this is rendered width, not a font-table estimate:

| string | chars | rendered ink | box | result |
|---|---|---|---|---|
| `RICO LANYARD TO` | 15 | **204.5px** | 249 | fits (this is the truncated form) |
| `RICO LANYARD TO SCORE` | 21 | ~286px (extrapolated) | 249 | **overflows — the T69 defect** |
| `WAITING FOR LANYARD` (progress, 19px) | 19 | 183.3px | 249 | fits |

**≈13.6px per character at 28px Condensed Bold**, and **≈7.3px at 15px**. Both are averages taken
from one string each — `RICO LANYARD TO` is a narrow-glyph string (I, L, T, R), so a caps-heavy form
with M/W/G will run wider. Treat these as planning figures, not a law; the authoritative check is
`FitToColumn`, which measures the real string on the real element.

**Working budgets:** roughly **18 characters** at NEED (249px @ 28px), roughly **19 characters** at
the compact statement (143px @ 15px).

## 3. Every market, both strings

Six `MarketKind`s, but **eight authored NEED forms** — BTTS is two different sentences rather than a
parameter, and Over/Under are separate strings.

| MarketKind | **NEED** (live, 249px @ 28px) | **compact source** (`DisplayLabel`, before re-authoring) |
|---|---|---|
| Moneyline | `{TEAM} TO WIN` | `{PICKED} ML — {AWAY} v {HOME}` |
| TotalGoals · Over | `OVER {L:0.0} GOALS` | `OVER {L:0.0} GOALS — {AWAY} v {HOME}` |
| TotalGoals · Under | `UNDER {L:0.0} GOALS` | `UNDER {L:0.0} GOALS — {AWAY} v {HOME}` |
| BothTeamsToScore · Yes | `BOTH TEAMS TO SCORE` | `BTTS YES — {AWAY} v {HOME}` |
| BothTeamsToScore · No | `KEEP ONE TEAM SCORELESS` | `BTTS NO — {AWAY} v {HOME}` |
| TotalCorners | `{OVER\|UNDER} {L:0.0} CORNERS` | `{OVER\|UNDER} {L:0.0} CORNERS — {AWAY} v {HOME}` |
| TotalCards | `{OVER\|UNDER} {L:0.0} CARDS` | `{OVER\|UNDER} {L:0.0} CARDS — {AWAY} v {HOME}` |
| AnytimeScorer | `{PLAYER} TO SCORE` | `{PLAYER} ANYTIME — {AWAY} v {HOME}` |

`{TEAM}` and `{PLAYER}` are uppercased full names. `{L:0.0}` is always one decimal.

### The live row's second line, for context

NEED sits above a progress line in the same 249px column — the DD is authoring the top of a pair:

| MarketKind | progress line (19px) |
|---|---|
| Moneyline | `LEADING 2–1` / `TRAILING 2–1` / `LEVEL 1–1` (en dash) |
| TotalGoals · Over | `3 GOALS · 1 MORE` (half-lines) or `3 GOALS` |
| TotalGoals · Under | `3 GOALS · LIMIT 1` or `3 GOALS` |
| BTTS · Yes | `1/2 TEAMS SCORED` |
| BTTS · No | `BOTH HAVE SCORED` / `CLEAN-SHEET PATH LIVE` |
| TotalCorners / Cards | `7 CORNERS · 2 MORE` / `· LIMIT n` / `7 CORNERS` |
| AnytimeScorer | `SCORED` / `WAITING FOR {SURNAME}` |

## 4. Which forms are already over budget

Against ~18 chars at NEED, with a worst-case name:

| NEED form | example | chars | verdict |
|---|---|---|---|
| `KEEP ONE TEAM SCORELESS` | as written | **23** | **over, always — no variable involved** |
| `BOTH TEAMS TO SCORE` | as written | **19** | **marginal, always** |
| `{PLAYER} TO SCORE` | `RICO LANYARD TO SCORE` | 21 | **over** — the T69 case |
| `{TEAM} TO WIN` | `ATLANTA MIDDLEMEN TO WIN` | 24 | **over** on a long club |
| `UNDER 2.5 CORNERS` | | 17 | fits |
| `OVER 2.5 GOALS` | | 14 | fits |

**Two of these are constants** — `KEEP ONE TEAM SCORELESS` and `BOTH TEAMS TO SCORE` overflow with no
variable in them at all, so they can be authored to fit once and be permanently safe. The other two
scale with a name and need either a short form or a name rule (surname only, as the progress line
already does with `WAITING FOR {SURNAME}`).

## 5. Scope (C25)

Widths are design-time constants read from the builder and confirmed against a rendered frame; the
per-character figures are extrapolations from two measured strings and will be wrong for unusually
wide or narrow copy. This covers only the TV's ticket column — the console and the laptop render
`DisplayLabel` themselves and are not touched by anything authored here. The compact column's 143px
assumes the price and chip keep their reserved widths; changing either changes the budget.

**The truncation backstop is in and holds** — no shipped statement should reach it once these forms
land, which is exactly what G1 said.
