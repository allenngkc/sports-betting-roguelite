# SPEC — the *neither*-branch flavour lines

**Written:** Design Director seat, 2026-08-21, on `T163` (batch 167) and Allen's direction.
**For:** the TV lane, phase 3 of `T140` arm A. **Not needed by phase 1.**

---

> ## ⚠ AMENDED 2026-08-21, batch 171 — §1's MECHANISM IS UNIMPLEMENTABLE ON BOTH SURFACES
>
> **`DramaEvent` carries `LegIndex`, `Step`, `TotalSteps`, `Type`, `WinProbAfter` and `Tag` —
> and NO ACTOR.** No scorer, no possession side. Measured by the markets lane while building
> `K17-cl`, and it is true of the TV as well as the console: **§1's slot change has nothing to
> read.** It is not buildable on either surface without an engine change.
>
> **So the document inverts.** §3 — which this spec called a fallback and told the lane to
> **delete if unused** — **IS WHAT SHIPS, on both surfaces.** §1 stands as the design that would
> be right if the event carried an actor, and as the statement of what an engine change would buy.
>
> **§2's three replacements still hold as PHRASES.** They interpolate a slot, so they cannot be
> used verbatim where there is no actor — but *"score against the slip"* was authored precisely
> because it states the goal works against the ticket **without naming a side it works for**, and
> that is *more* true with no slot than with one. The console's `a goal against the slip.` is that
> phrase landing where it always belonged.
>
> **§4's casing rule is CORRECTED — see §5.** *"Match the table they join"* was written for one
> file and produced the wrong result on transfer to another.
>
> **The full club-free line set, authored, is §5.**

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

---

## 5. THE CLUB-FREE LINE SET — authored in full (batch 171)

**Two of these ship already**, assembled by the markets lane and flagged `ASSEMBLED, NOT AUTHORED`
in `EventText.cs`. **Both are endorsed as authored** — see `K17-cl-vf`. The rest are added here
because **each goal table shipped with ONE variant**, and `variants[step % variants.Length]` on a
single-element table means every goal beat in the branch reads identically.

### 5.1 Casing — the corrected rule

**A club-free line takes the casing its own FILE uses for club-free lines.**

Not *"the table it joins"*: a table whose other lines open with an interpolated club noun has no
casing of its own to match, which is how four lines in one branch ended up split two capitalised,
two lowercase. `EventText.cs` establishes lowercase for club-free copy — `a goal in the churn — not
the backed scorer.`, `off the bar and away.` — **so on the console the whole branch is lowercase**,
momentum lines included. On the TV, whose club-free convention is its own, they take that.

### 5.2 The set

| table | variant | line |
|---|---|---|
| **goal, number up** | 1 | `a goal — the number ticks with it.` *(ships)* |
| | 2 | `a goal in the churn; the number moves.` |
| | 3 | `one goes in — the slip gains.` |
| **goal, number down** | 1 | `a goal against the slip.` *(ships)* |
| | 2 | `a goal; the slip flinches.` |
| | 3 | `one goes in, the wrong way for the slip.` |
| **momentum, up** | 1 | `the half tightens.` |
| | 2 | `territory, and the clock with it.` |
| | 3 | `the pitch shrinks.` |
| **momentum, down** | 1 | `the ball stays in midfield.` |
| | 2 | `slow through the middle, and no one in a hurry.` |
| | 3 | `sideways, and the clock with it.` |

**Every line names no club and states no verdict on the match** (`T39`, `T44`, §8). Where a line
speaks of the ticket it uses the surface's own word — `the slip` — which `ScoreDown` already ships
in *"the slip flinches"*.

**`a goal in the churn`** is borrowed from `EventText.cs`'s own scorer branch rather than invented,
and **`territory` / `sideways`** are deliberately paired so the two momentum directions read as one
axis rather than two moods.
