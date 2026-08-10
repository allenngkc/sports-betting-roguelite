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
| Moneyline | `{CLUB} TO WIN` | `{CLUB} ML` |
| TotalGoals · Over | `OVER {L:0.0} GOALS` | `OVER {L:0.0} GOALS` |
| TotalGoals · Under | `UNDER {L:0.0} GOALS` | `UNDER {L:0.0} GOALS` |
| BTTS · Yes | `BOTH TEAMS SCORE` | `BTTS YES` |
| BTTS · No | `ONE TEAM SCORELESS` | `BTTS NO` |
| TotalCorners | `{OVER\|UNDER} {L:0.0} CORNERS` | `{OVER\|UNDER} {L:0.0} CORNERS` |
| TotalCards | `{OVER\|UNDER} {L:0.0} CARDS` | `{OVER\|UNDER} {L:0.0} CARDS` |
| AnytimeScorer | `{SURNAME} TO SCORE` | `{SURNAME} ANYTIME` |

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
