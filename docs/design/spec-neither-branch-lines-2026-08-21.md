# SPEC — the *neither*-branch flavour lines

**Written:** Design Director seat, 2026-08-21, on `T163` (batch 167) and Allen's direction.
**For:** the TV lane, phase 3 of `T140` arm A. **Not needed by phase 1.**

---

## 0. THE FINDING THAT MAKES THIS CHEAP

**I expected to author eighteen lines. Fifteen of them already work.**

Read line by line, the six tables are **already anchor-neutral in their prose** — they observe the
match in the third person and say nothing about whose side anyone is on. `{picked} slot it home.`,
`{other} walk it in.`, `{picked} tighten the screws.` all read correctly whoever the club is.

**Only THREE lines encode a relationship to an "us":**

| table | line | what makes it anchor-dependent |
|---|---|---|
| `ScoreDown` | `{other} answer right back.` | *answer* asserts a prior goal by the other side |
| `MomUp` | `{picked} pin them deep — passes and patience.` | *them* implies an us |
| `MomDown` | `{other} settle in; the drift runs the other way.` | *the other way* is relative to us |

**So the job is not a new line set. It is one slot change and three replacements.**

---

## 1. THE SLOT CHANGE — what `{picked}` and `{other}` MEAN in the neither branch

No template uses both slots in one string; each line carries exactly one. **In the neither branch
the slot is filled from the EVENT, not from the anchor:**

- **`{picked}` / `{other}` resolve to the club the event BELONGS TO** — the scorer on a goal, the
  side with the ball on a possession beat.
- **The table is still selected by the ticket's direction** (`up` / `down`), unchanged.

**The tables keep their names and their tone; only the referent moves.**

> **THIS ONLY WORKS BECAUSE OF `T164`.** In the neither branch the live legs can WANT OPPOSITE
> THINGS — one leg on a clean sheet, another on a total — so a goal helps one and kills another.
> **There is a single `up`/`down` to select a table only because the displayed probability is the
> TICKET's** (`T164`, and `T143` downstream of it). Were the number still seeded per leg, legs in
> disagreement would have no single direction and no line could be written at all. The two rulings
> are load-bearing for each other and should not be separated.

## 2. THE THREE REPLACEMENTS

Authored to the same constraints as the lines they replace — **third person, flat, observed;
no second person, no hype, no verdict, no superlative, no taking the player's side** (`T39`, `T44`,
§8) — and in the register the strip already holds: *incisive, nocturnal, dry, orderly.*

| table | variant | **neither-branch line** |
|---|---|---|
| `ScoreDown` | 1 | **`{other} score against the slip.`** |
| `MomUp` | 2 | **`{picked} keep it deep — passes and patience.`** |
| `MomDown` | 3 | **`{other} settle in; the half goes quiet.`** |

- **`score against the slip`** says the goal works against the ticket **without naming a side it
  works for.** `the slip` is the strip's own established word for the ticket — `ScoreDown`'s third
  variant already ships *"the slip flinches"* — so this borrows vocabulary rather than inventing it.
- **`keep it deep`** drops *them* and keeps the rhythm and the phrase `T44` preserved.
- **`the half goes quiet`** replaces a drift measured from us with the same observation measured
  from the match.

## 3. THE MOMENTUM FALLBACK — needed, and it is the one open dependency

The slot change requires **the event's actor**. On a goal that is the scorer. **On a MOMENTUM beat
it is the side in possession, and if the event does not carry that, the momentum tables cannot be
filled in the neither branch at all.**

**If the actor is unavailable, the momentum beat takes a CLUB-FREE line.** Three variants, same
register:

| | line |
|---|---|
| up | **`The half tightens.`** |
| up | **`Territory, and the clock with it.`** |
| down | **`The ball stays in midfield.`** |
| down | **`Slow through the middle, and no one in a hurry.`** |

**Named as a fallback rather than the default** — a line that names the club is better than one that
does not, and this fires only where the actor genuinely is not knowable.

**The lane reports which it needs.** If `DramaEvent` already carries a possession side, §3 is dead
and should be deleted rather than shipped unused.

## 4. WHAT THIS SPEC DOES NOT SETTLE

- **CASING.** These tables are sentence case with a terminal period; the strip elsewhere carries
  authored caps (`THE MATCH ENDS LEVEL`, set directly at the call site). **The lines here match the
  table they join, exactly** — this seat is not silently re-casing a shipped table, and whether the
  two styles should converge is its own question and is not asked here.
- **When the neither branch FIRES.** That is `T163`'s three branches, ruled and unchanged.
- **The fifteen surviving lines.** Not re-authored, not re-ordered, not touched.
