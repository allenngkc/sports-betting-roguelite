# Routed: `WinningMargin` bucket 1, and what `{n}` counts — TV → DD (2026-08-25)

**Two questions raised by building `T169`'s four arms. Report only — this lane proposes no copy and
rules nothing. Both are recorded in the build's own comments at the sites they affect.**

## CONDITIONS (`C58-am2`), stated because the build state moved between them

- **Measured at `1c0e400`** — before `T169`'s arms existed. Read from the repo at run time.
- **Built at `64b3f70`**, merged to `main`. **Every width below is therefore a PRE-build number
  describing what the UNAUTHORED path renders** — which is exactly what both questions are about, so
  the ordering is deliberate rather than a staleness to discount.
- **Suites at the build: EditMode 342/341/0/1 · PlayMode 152/125/0/27.**
- Measured through the shipped path — `LegStatement`, `Fits` and `FitToColumn` reached by
  reflection, never reimplemented. NEED box **261.0px**, font asserted Encode Sans (`T20`).

---

## QUESTION 1 — `WinningMargin` BUCKET 1 IS OFFERED AND HAS NO AUTHORED FORM

### The fact, taken off a real board rather than read off the source

`MatchModel.BuildOffers` runs `for (int m = 1; m <= TopMarginBucket; m++)` **unfiltered** — no
probability floor, unlike the correct-score grid and the multi-scorer board. `MarketSelection`'s own
docstring is explicit that this is not an edge: *"margin 1 and 2 are EXACT; the top bucket is 'that
many OR MORE' so the buckets partition the space."*

**Asserted, not inferred.** `T169_measure_the_four_kinds_authored_rungs` walks a generated slate and
fails if bucket 1 is absent, so this finding cannot quietly evaporate the day the board changes:

```
[RUNGS-MARGIN] buckets OFFERED on a real board: 1, 2, 3
```

A second gate (`T169_the_four_new_arms_are_reached_by_real_offered_selections`) counted **12
bucket-1 offers in a single run** across two seeds.

### What the deck authors, and what it does not

`T151` (batch 137) and `G1-am11` §3.2 (batch 159) both author **2** and **3+** only — compact,
NEED rung 1, rung 2, and the new rung 3. **Bucket 1 has no form in either slot.**

`T151`'s own `T108` check names all three buckets — *"`WinningMargin`'s buckets are 1 / 2 / 3+"* —
so the count was known at authoring time. This lane reports the gap and reads nothing into it.

### What it renders today, measured

Unauthored, it reaches `LegStatement`'s `default:` → `MatchModel.Fields` → **`1 GOAL`**, measured at
**89.2px against the 261.0px band — it FITS and renders silently.** There is no overrun to notice it
by.

> **That is the string `T151` was written to prevent.** That row authored `MARGIN`/`APART` because
> *"the engine's bare `2 GOALS` collides with the total-goals family's own forms on the same
> column."* `1 GOAL` is the same collision one bucket down, and `T169` already raised
> `WinningMargin`'s priority for exactly this shape at buckets 2 and 3+.

### What the build does with it — deliberately nothing

`AuthoredStatement` returns **null** for bucket 1 and `DescribeActiveLeg` takes `goto default;`, so
the two sites agree by construction rather than by a duplicated condition. **No `MARGIN 1` was
coined** — a short form nobody wrote is `G1`'s defect class, and it is the improvisation that rule
exists to stop.

The pending window inherits the same answer: `PendingLegName` gets null and takes the club-alone
path, exactly as `T143-am9` leaves the unauthored kinds.

### The shape of the decision, stated without taking it

This is `C57`'s discriminator with a different answer from `T169`'s four: **absent from the register,
absent from the deck, absent from the build.** It is an authoring question, not a build order. The
moves this lane can see are the same three `T161-am2` enumerated for `DoubleChance` — author a form,
rule the fallback acceptable, or take the bucket out of the offered set — **and all three are the
DD's or Allen's.**

---

## QUESTION 2 — `T152` AUTHORED `CLEAR BY {n}` / `TRAILING BY {n}` AND NEVER DEFINED `{n}`

### The gap

`T152` (batch 138) authored the handicap progress pair and gave its reasoning in full — the
moneyline's report is REFUSED here because *"at `1-0` with `-1.5` the bet is LOSING while
`LEADING 1-0` says otherwise,"* which is `T108`'s family. It also struck `ON THE LINE` as
unconstructible, since the engine offers ±1.5 only and the adjusted margin is never zero.

**What it does not say is what the number counts.** The forms are authored; the quantity is not.

### What was built, and it is one line

**`{n}` = `ceil(|margin + line|)` — the goals that would have to change to flip the leg.**

At `2-0` on `-1.5` the adjusted margin is `+0.5`, one goal against flips it, and the row reads
`CLEAR BY 1`. At `0-0` on `-1.5` the adjusted margin is `-1.5`, two goals are needed, and it reads
`TRAILING BY 2`.

### The readings this lane checked and did not take, with the reason for each

| reading | at `2-0` on `-1.5` | why not |
|---|---|---|
| **`ceil(\|adjusted\|)`** — goals to flip | `CLEAR BY 1` | **built** — revealed-only, actionable, whole goals |
| the raw adjusted margin | `CLEAR BY 0.5` | puts a **half-goal** on a surface that counts in goals |
| `floor(\|adjusted\|)` | `CLEAR BY 0` | `T108`'s class — a requirement of zero, which that row rules unconstructible |
| the plain scoreline margin | `CLEAR BY 2` | this is the moneyline's report, which `T152` refused **by name** |

**Every one of these is one line at `SweatActiveLegModel.DescribeHandicap`.** The build's own comment
says so at the site and marks the reading as the lane's, routed.

### What is NOT in question

The two forms themselves, the refusal of the moneyline report, and `ON THE LINE`'s striking. `T152`
settled all three and nothing here reopens them.

---

## ALSO OWED, CAUSED BY THIS BUILD — not a question, a consequence

`route-team-total-fallback-measured-2026-08-25.md` closes with its own condition: *"Every number is
pre-`T168-am`. If it is built, the club token shortens and all four rows must be re-measured — the
shorter input may leave the distinctive word inside the box."*

**`T168-am` is now built** (`64b3f70`, `ShortenSubject`). **The four `T156` cases are owed a
re-take** and this seat did not take it. The four re-ran unchanged at `1c0e400` (`390.0 → 258.0`,
`549.7 → 147.6`, `424.3 → 258.0`), which is the pre-`T168` baseline to compare against.

Whether the distinctive word now survives **decides whether `T156` is still live in the build**, and
that is the question batch 185 §5 asked in the first place.

---

## WHAT THIS LANE IS NOT CONCLUDING

- **No copy is proposed for bucket 1**, and none may be until it is ruled.
- **No reading is taken on either question.** Question 1 is an authoring call; question 2 is a copy
  call this lane implemented in order to ship a working row and has flagged at its own site.
- **The `T156` re-take is named, not performed** — the numbers above are its baseline, not its result.
