# Re-take: the team-total NEED fallback, POST-`T168-am` — TV → DD (2026-08-25)

Against `docs/design/measurement-ask-team-total-fallback-2026-08-25.md`, which binds as before.
**Report only — §4 pre-committed the reading before the number existed, and this document authors
none of it.** Supersedes nothing: `route-team-total-fallback-measured-2026-08-25.md` remains the
pre-`T168` record and is the baseline this compares against.

## CONDITIONS (`C58-am2`), both stated

- **Commit measured at: `638df13`** — read from the repo at run time.
- **`T168-am` BUILT: YES** — `64b3f70`, `TvSweatScreen.ShortenSubject`. **Verified in-run, not
  asserted in prose.** The first run verified the negative by absence of the string `T168` under
  `Assets/**`; a positive cannot be verified that way, so the test now measures **both** forms and
  **FAILS if they are identical** on a club whose city is genuinely present. A before/after pair
  with nothing between it is a green that measured one string twice.
- **Suite: EditMode 342/341/0/1**, measured through the shipped path — `LegStatement` and
  `FitToColumn` reached by reflection, never reimplemented. NEED box **261.0px**, font asserted
  Encode Sans (`T20`).

> **BOTH FORMS COME FROM THIS RUN, AT THIS COMMIT.** The pre-`T168` input is reconstructed from
> `MatchModel.Fields(...).Line.ToUpperInvariant()` — which is character-for-character what
> `SheetName` returned before `ShortenSubject` was inserted, because `MarketSheet.NameOf` returns
> `fields.Line` for every team total. **So the delta cannot be attributed to anything else that
> moved between `b60d2bd` and now.**

---

## THE FOUR CASES, BEFORE AND AFTER

| # | kind · line · club | | input | in | **survivor** | out |
|---|---|---|---|---|---|---|
| 1 | `TeamTotalGoals` 1.5 · one-word city | pre | `RENO FERRETS OVER 1.5 GOALS` | 390.0 | `RENO FERRETS OVER` | 258.0 |
| 1 | | **now** | `FERRETS OVER 1.5 GOALS` | 314.4 | **`FERRETS OVER 1.5`** | 227.2 |
| 2 | `TeamTotalCards` 1.5 · **same club** | pre | `RENO FERRETS OVER 1.5 CARDS` | 390.1 | `RENO FERRETS OVER` | 258.0 |
| 2 | | **now** | `FERRETS OVER 1.5 CARDS` | 314.5 | **`FERRETS OVER 1.5`** | 227.2 |
| 3 | `TeamTotalGoals` 1.5 · two-word city | pre | `MOOSE JAW SPREADSHEETS OVER 1.5 GOALS` | 549.7 | `MOOSE JAW` | 147.6 |
| 3 | | **now** | `SPREADSHEETS OVER 1.5 GOALS` | 396.1 | **`SPREADSHEETS`** | 190.1 |
| 4 | `TeamTotalCorners` 4.5 · control | pre | `RENO FERRETS OVER 4.5 CORNERS` | 424.3 | `RENO FERRETS OVER` | 258.0 |
| 4 | | **now** | `FERRETS OVER 4.5 CORNERS` | 348.7 | **`FERRETS OVER 4.5`** | 227.2 |

**`T46`'s backstop is NOT reached** in any case, before or after — every survivor sits inside 261.0.

### The two flags, reported separately — and the first run conflated them

The ask hangs different readings on the **market noun** (§4(a)) and on the **club's distinctive
word** (§4(c)). The first run took "the distinctive word" to be the input's last token, which for
`RENO FERRETS OVER 1.5 GOALS` is `GOALS` — **the market noun.** They are separated here.

| # | market noun | club's distinctive word |
|---|---|---|
| 1 | **LOST** before and after | SURVIVES before and after |
| 2 | **LOST** before and after | SURVIVES before and after |
| 3 | **LOST** before and after | **LOST before · SURVIVES now** |
| 4 | **LOST** before and after | SURVIVES before and after |

---

## THREE OBSERVATIONS. `T168` MOVED TWO OF THE FIRST RUN'S FINDINGS AND LEFT ONE STANDING

### 1. `T156`'s PAIR IS UNCHANGED — cases 1 and 2 are still character-identical

**`FERRETS OVER 1.5`** for goals and for cards. The string is shorter and better than
`RENO FERRETS OVER` — it keeps the club and now keeps the LINE — **and it is still one string for
two markets.** The market noun is lost in both, before and after.

**`T168` shortened the input; it did not separate the pair**, which is exactly what `T168-am` said
it would not do (*"this row shortens a string without settling a market"*).

### 2. THE CITY-ONLY SURVIVOR IS GONE — case 3 now keeps its distinctive word

`MOOSE JAW` → **`SPREADSHEETS`**. The first run's finding 2 — *the inverse of `T69`'s convention* —
**is not reproducible at this commit.**

Stated as an observation and not read: **§4(c) was satisfied by case 3 before `T168` and is not
satisfied by any case now.**

**What remains at case 3 is a different shape, and it is reported rather than classified:** the
survivor is the **bare club**, with no direction, no line and no noun — so it carries less than
cases 1, 2 and 4, which retain `OVER {line}`. Only the two-word cities reach it.

### 3. THE CONTROL NO LONGER COLLIDES — the first run's finding 3 is retired by `T168`

The first run reported case 4 colliding with cases 1/2, because all three truncated to
`RENO FERRETS OVER` with the line already dropped. **Post-`T168` the LINE SURVIVES:**
`FERRETS OVER 4.5` against `FERRETS OVER 1.5`.

**The unshared-line protection works again.** The first run's warning — *"whatever fixes cases 1–3
must not assume corners is already safe"* — **was correct then and is discharged by this number**,
which is a change in the build rather than a correction to that reading.

---

## WHAT THIS LANE IS NOT CONCLUDING

- **§4's readings are the DD's**, including the falsification condition for batch 187. The market
  noun does NOT survive in any case, so **§4(a) is not triggered** — reported as an observation, not
  read. Which of (b)/(c)/(d) now applies, and what the two retired findings mean for the scope call,
  are the DD's.
- **No copy is proposed.** The three team totals are held by Allen (`T152-am`); the surviving strings
  are what the shipped ladder produces, never a recommendation.
- **No fix.** `T168-am`'s repair is already at the render, which is the site §5 names.
- **The `T156` ruling is untouched by this lane.** Cases 1 and 2 remaining character-identical is
  reported as the fact it is.
