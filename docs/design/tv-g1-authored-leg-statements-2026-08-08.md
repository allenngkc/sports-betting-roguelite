# G1 — the TV's authored leg statements

**From:** Design Director · **2026-08-08** · closes **G1** (batch 17)
**Against:** `tv-g1-leg-statement-market-list.md`, TV sweat lead, build `112df65`
**Canonical home on approval:** the TV slice's copy deck, referenced from `docs/design/tv-design.md` §8

---

## 0. The list corrected G1's premise, and the correction is the reason this works

G1 asked for *"the authored statement string for every market"*. **There are two per leg**, from two
sources, into two boxes:

| | source | shows | box | budget |
|---|---|---|---|---|
| **NEED** | `DescribeActiveLeg` → `copy.Need` | while the leg is live | 249px @ 28px | ~18 chars |
| **compact** | `DisplayLabel`, re-authored by `LegStatement` | every other row state | **143px @ 15px** | ~19 chars |

Authoring one would have left the other exactly as it is. **The compact box is the tight one** — 143px
after the price and state chip take their reserved widths — and it is the box T69's fix already lands
in.

---

## 1. The two jobs

**NEED states the requirement. The compact statement states the identity.**

That is the whole rule, and everything below follows from it. A live row asks *what does my money still
need*; every other row asks *which bet is this*. Where those two questions have the same answer — the
totals markets — **the two strings are identical, and that is correct, not a duplication to be
designed away.**

## 2. Two conventions, both already shipped on this surface

Neither is new. T69's fix and the existing progress line already established both:

- **Clubs are named by their distinctive word**, city dropped. `Atlanta Middlemen` → `MIDDLEMEN`.
  This is what T69 shipped.
- **Players are named by surname.** `Rico Lanyard` → `LANYARD`. This is what `WAITING FOR {SURNAME}`
  already does.

**The scorebug carries the fixture and the `BACKED` marker carries the side**, so neither statement
needs to re-establish who is playing whom. That is what makes 143px workable at all.

---

## 3. The forms

`{CLUB}` = distinctive word, uppercase. `{SURNAME}` = uppercase. `{L:0.0}` = always one decimal.

| MarketKind | **NEED** (live) | **compact** (all other states) |
|---|---|---|
| Moneyline · Home/Away | `{CLUB} TO WIN` | `{CLUB} ML` |
| **Moneyline · Draw** | **`LEVEL AT FULL TIME`** | **`DRAW`** |
| TotalGoals · Over | `OVER {L:0.0} GOALS` | `OVER {L:0.0} GOALS` |
| TotalGoals · Under | `UNDER {L:0.0} GOALS` | `UNDER {L:0.0} GOALS` |
| BTTS · Yes | `BOTH TEAMS SCORE` | `BTTS YES` |
| BTTS · No | `ONE TEAM SCORELESS` | `BTTS NO` |
| TotalCorners | `{OVER\|UNDER} {L:0.0} CORNERS` | `{OVER\|UNDER} {L:0.0} CORNERS` |
| TotalCards | `{OVER\|UNDER} {L:0.0} CARDS` | `{OVER\|UNDER} {L:0.0} CARDS` |
| AnytimeScorer | `{SURNAME} TO SCORE` | `{SURNAME} ANYTIME` |

### AMENDED 2026-08-14 (batch 68) — the draw's row, and why its absence shipped a defect

**This deck was written 2026-08-08. S74 authored the draw's forms on 2026-08-12 and this table was
never amended.** `tv-design.md` §8 has since said the draw's forms *"are authored and live with the
rest"* — **against this file that sentence was false**, and it is true only as of this amendment.

**That absence is the whole cause of T96.** The build's `LegStatement()` Moneyline branch is a
two-way `pickedHome ? Home : Away` — **which is exactly what the row above it said**. The surface was
faithful to the deck; **the deck was the defect**, and it is a DD-owned file, so this is the fix
rather than a request for one.

**The forms are S74's, unchanged and not re-opened here:** NEED `LEVEL AT FULL TIME`, **compact
`DRAW`**, progress `LEVEL` / `NOT LEVEL`. Nothing is invented — `LEVEL` is already this surface's word
for a tied scoreline (T62). **`1X2` never reaches the player** (S74).

**The progress pair does NOT breach T70** (T70-am, this batch): T70's *no term repeated across the
two* governs the **SUBJECT**, not the predicate — its own example is `LANYARD TO SCORE` over
`WAITING FOR LANYARD`, a **name** printed twice, *"T69's defect turned vertical"*, and T69 is the
backed team printed twice. **A binary state answering its own requirement in the requirement's word is
not redundant identification — it is the progress line doing its job.** Avoiding `LEVEL` below would
force a second word for one thing and break the one-name-per-thing convention T62 established.

**`DRAW` is the shortest compact form in this deck** — 4 chars against a ~19-char budget. **No
fallback is needed and none is authored**, which is the honest entry rather than a defensive one.

**`LEVEL AT FULL TIME` is 18 chars and sits AT the NEED budget**, exactly where `ONE TEAM SCORELESS`
sits. §4 governs: **measure it, and take the authored fallback if it misses.**

### What changed, and why

- **`{TEAM} TO WIN` → `{CLUB} TO WIN`.** 24 chars becomes 16 on the measured example. The variable was
  the whole problem and the convention already existed.
- **`{PLAYER} TO SCORE` → `{SURNAME} TO SCORE`.** 21 → 16. **This is the T69 case itself** — the string
  that produced `RICO LANYARD TO`.
- **`BOTH TEAMS TO SCORE` → `BOTH TEAMS SCORE`.** 19 → 16. Dropping one word clears a permanently
  marginal constant.
- **`KEEP ONE TEAM SCORELESS` → `ONE TEAM SCORELESS`.** 23 → 18. `KEEP` is an instruction to the
  player about a thing he cannot influence — §8's register problem as well as a width problem. The
  requirement is a state of the match, so name the state.

## 4. Fallbacks — the shorter authored line, per §8

§8 says copy *truncates or chooses a shorter authored line; it never shrinks*. Every form that can
overflow on an unlucky variable gets its shorter line authored now, so truncation is never reached:

| form | overflows when | shorter authored line |
|---|---|---|
| `{CLUB} TO WIN` | a long club — `GRAVEDIGGERS TO WIN` is 19 | **`TO WIN`** |
| `{SURNAME} TO SCORE` | a long surname | **`TO SCORE`** |
| `ONE TEAM SCORELESS` | if 18 measures over at 28px Bold | **`ONE TEAM BLANKED`** |
| `{OVER\|UNDER} {L:0.0} CORNERS` | `UNDER 10.5 CORNERS` is 18 | **`{OVER\|UNDER} {L:0.0} CNRS`** — *last resort; prefer the full word* |
| `LEVEL AT FULL TIME` | 18 at the budget, same class as `ONE TEAM SCORELESS` | **`LEVEL AT FT`** — `FT` is **this surface's own clock token**, printed in the scorebug, not jargon (added batch 68) |

**`TO WIN` and `TO SCORE` are complete, not truncated.** The backed side is already marked in the
scorebug and the leg's own row is the subject — the sentence has a subject, it is just not repeated.

**`FitToColumn` is the authority, not my character counts.** The lead's ~13.6px/char at 28px and
~7.3px/char at 15px are planning figures from two strings, and a caps-heavy form with M/W/G runs wider.
Two forms sit at exactly the budget (`ONE TEAM SCORELESS`, `UNDER 10.5 CORNERS`); **measure them, and
take the authored fallback if either misses.** Do not shave a character off a form to make it fit —
take the fallback, which is authored to read as a whole sentence.

---

## 5. One defect in the pair, found while authoring the top of it

NEED sits directly above the progress line, and **the two are one authored pair.** Checked against the
lead's progress table:

> NEED `LANYARD TO SCORE` · progress `WAITING FOR LANYARD`

**The surname appears twice, three lines apart, and both lines say the same thing.** That is T69's
defect — a fact named twice in one statement — reproduced vertically instead of horizontally.

**Ruled: the progress line for AnytimeScorer becomes `NOT YET` (unscored) and `SCORED` (resolved).**
The player is named once, by NEED, directly above it.

Every other pair checks out — `{CLUB} TO WIN` over `LEVEL 1–1`, `BOTH TEAMS SCORE` over
`1/2 TEAMS SCORED`, `OVER 2.5 GOALS` over `3 GOALS · 1 MORE`. Requirement above, state below, no term
repeated.

---

## 6. Scope (C25)

These are the TV's ticket column only. **The console and the laptop render `DisplayLabel` themselves
and are untouched** — this is re-authoring on this surface, which is T42's shape and the same boundary
T69 held.

Two forms are at the budget line and are verified by `FitToColumn`, not by me. The per-character
figures they were checked against are extrapolations from two measured strings.

**Not covered:** what a leg statement should say in a market this list does not contain. Six
`MarketKind`s, eight NEED forms, and a seventh market would need one authored before it ships.
