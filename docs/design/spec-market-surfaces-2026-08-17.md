# SPEC — the market surfaces (Phase 1, FINAL)

**Written:** Design Director seat, 2026-08-17 · **Authority:** Allen's calls relayed 2026-08-17,
landed as `S89`–`S92` · **Evidence:** `entry-read-2026-08-16.md` (`S86`, `S87`) ·
`markets-surface-research-2026-08-16.md` · **Surface:** SureThing — the laptop

**This spec is buildable.** Everything in it is either an Allen-approved call, an existing ruling
applied, or a treatment decision within this seat's authority. **Two things are deliberately left to
a frame and are named as such** (§4.3, §9).

---

## 1. THE SUBJECT — ENTRY's market body, not the FORM board

**The ~80 offers do not live on the board.** The FORM board is the **slate**: six matchups at a 116px
block pitch, three prices and a `MORE ›` head, and **its density is closed** — `S81`/`S81-am`, Allen at
batch 76. **This spec does not reopen it and must not be read as doing so.**

**ENTRY is the market sheet**, reached through `MORE ›`. Everything below is ENTRY's.

The moneyline appears on both surfaces. **That is not a duplication defect:** the board is a
*preview* of the slate, ENTRY is where the bet is taken, and `C19` requires every priced offer to be
reachable **on the betting surface**. Named so it is not flagged later.

## 2. THE PROBLEM, MEASURED

| | |
|---|---|
| `MarketKind` members | **15** |
| with a reachable home | **6** |
| **homeless** | **9** (`S86`, measured on frames) |
| offers reachable on ENTRY | **~35** against the mandate's ~80 |
| row pitch | **54px** (`S87`) |
| market body viewport | ~422px → **~7.8 rows per screen** |
| the full vocabulary | **~10 screens, not four** |
| rail usable width | **700px — MEASURED by the build, 2026-08-17** (the spec's ~996px was arithmetic on authored constants and was wrong; §9's flag fired) |

**The presentation pass is not a polish job. It is how nine priced market kinds get a home.**

## 3. THE DESTINATIONS — five, fixed, and always printed

**The hybrid taxonomy transfers and every book converged on it:** statistic names where the thing is
physical and countable, bet-type names where it is abstract. **Do not purify it.**

| destination | holds | ~rows |
|---|---|---|
| **RESULT** | Moneyline · DoubleChance · Handicap · WinningMargin · **CorrectScore** | 30–36 |
| **GOALS** | TotalGoals · TeamTotalGoals · TotalGoalsOddEven · BothTeamsToScore | 16–18 |
| **CORNERS** | TotalCorners · TeamTotalCorners | 10–12 |
| **CARDS** | TotalCards · TeamTotalCards | 10–12 |
| **PLAYERS** | AnytimeScorer · PlayerMultiScorer | ~15+ |

**Five destinations for fifteen kinds, against a rail measured at 700px.**

**Amended 2026-08-17 (`S94`) — this table said SIX and the build measured it into overflow.** The
spec assumed ~996px of usable width; **it is 700px**, and six tabs carry 43 characters against
today's working five at 28. **The cause was the sixth destination, not the longest name** — so
`CORRECT SCORE` folded into `RESULT` rather than being abbreviated, tiered or shrunk. **The new five
carry 30 characters against today's 28: it fits by the same arithmetic that already works.**

**Three options were offered and all three refused** — shrinking type (the 13px floor is law),
tiering (§5.2 rules the rail stays one level), and abbreviating (treats the symptom). **§9's flag
worked; the number it guarded was wrong; the "deliberate headroom" claim rested on it. §1.5.**

**Three derivations, so the grouping is not read as taste:**

- **Team-splits fold into their statistic, never into a "TEAM TOTALS" group.** Countables take
  statistic names, and a player asking *how many corners will they get* looks under CORNERS. Each
  stat group is then a **complete** answer to its statistic.
- **BTTS folds into GOALS.** It is a two-row market about goals; it does not earn a destination. **It
  does not become hard to find — §5 names it in the contents.**
- **CORRECT SCORE folds into RESULT** (`S94`). **It is taxonomically better, not a compromise:**
  Correct Score IS a result market — the most specific one there is — `RESULT` is a bet-type name
  holding abstractions, and `WinningMargin` is already inside it as its near cousin. It makes `RESULT`
  a 30–36 row destination that scrolls, **which is already ruled and already demonstrated.**

### 3.1 The rail is a CONSTANT — and `S89` is what makes it one

**Because empty groups print, the destination set never varies by matchup.** Every book generates its
rail per event; **ours cannot, and must not.**

This is worth more than it costs: the rail is **authored once, measured once, and never reflows** —
which delivers the research's *no layout wobble, blink-then-settle* transfer for free, on a surface
where reflow would destroy the diegesis instantly.

## 4. THE ROW

### 4.1 One offer per row (`S92`)

Ratified. Two-up pairs are not restored. The row is full width: **market name left, price right.**

### 4.2 Name first (`S91`, half one)

**The market name leads in the typeset layer; the price is set right-aligned beside it.** This is
largely what the build already does — it arrived by migration drift and is now a ruling, because a
board built price-first is not cheaply reversed.

The seam is the one the colour law already draws: **the LINE is printed, fixed, part of the form; the
PRICE is the house's mutable offer.**

### 4.3 The gap carries the eye — leader dots (this seat's call)

A ~996px row with a name at one end and a price at the other **needs a device or it is just
emptiness.** `S92` ruled that the width is the **annotation gap** rather than waste; a gap that is
doing work should look like it.

**RULED: the offer row is ONE statement, not two facts at opposite ends. Leader dots carry the name to
its price.**

**This invents nothing** — `S89` already puts leader dots in the product (`CORNERS ….. 11`). This is
the same device one level down, which is exactly how a racecard uses it.

### 4.4 The amber — NOT ruled here (`S91`, half two)

**Does the price take the money-amber?** The law says amber = money and a price is money; against it,
**~80 amber prices on one sheet, and amber is also the ACTION colour — if everything is amber, nothing
is.**

**RULED: one sheet rendered BOTH WAYS, decided on the frame. The comparison frame lands with this
build** rather than being commissioned separately. **Build the sheet so the price's ink is a single
switch**, or the comparison costs a second build.

**Seat's lean, on the record and not binding: yes** — and at one offer per row the amber lands in a
single column down the right edge, which reads as an annotation rail rather than scattered ink.

### 4.5 Unchanged, named so nothing is "tidied"

Type does not shrink; the **13px product-fact floor is law**; row pitch stays 54px; status is never
carried by colour alone; **suspended is greyed, non-clickable AND stated.**

## 5. THE CONTENTS BLOCK AND THE FOLIO (`S90`)

### 5.1 The folio

**The position rail prints as a folio: `46–66 of 80`.** A **fact**, not a scrollbar's proprioceptive
hint.

**The numbers are DERIVED from the rendered list, never authored.** `S74-am3`'s standard: *a constant
that happens to equal the right answer is a constant that will stop equalling it.* **A folio that
lies is worse than no folio**, because its whole value is that it is true inside a game about being
lied to.

### 5.2 The contents block — two levels, and why that is not the double rail

**The contents lists the destination AND the markets inside it**, each with its printed line range:

```
RESULT ...................... 1–33
    MATCH RESULT ............ 1–3
    DOUBLE CHANCE ........... 4–6
    HANDICAP ................ 7–12
    WINNING MARGIN .......... 13–18
    CORRECT SCORE ........... 19–33
GOALS ....................... 34–51
    TOTAL GOALS ............. 34–39
    TEAM TOTALS ............. 40–47
    ODD / EVEN .............. 48–49
    BOTH TEAMS TO SCORE ..... 50–51
CORNERS ..................... 52–63
CARDS ....................... 64–75
PLAYERS ..................... 76–91
```

**This is the move that makes §3's grouping safe, and `S94` is the first time it has been asked to
prove it.** Every market is named in the contents regardless of which destination holds it — so
**BTTS inside GOALS, and CORRECT SCORE inside RESULT, cost the player nothing.** **The two-level
contents was built precisely so the RAIL could flex without the vocabulary paying for it**, and when
the measured width forced a destination out, that is exactly what happened.

**And it is not the double-tiered rail.** The **RAIL stays ONE level** — DraftKings ships two and is
rated down for it by every comparison in the corpus, and we do not build tier two. **A printed
contents list is not a navigation tier; it is a page you read.** That distinction is available to us
only because we are made of paper, and it is the whole advantage.

**Worst-case navigation: three interactions** (contents → destination → row) against DraftKings' ~7.

### 5.3 Empty groups (`S89`)

**A group with offers prints its count.** A group with none prints `no prices offered`. **A racecard
prints the race even when it is abandoned.**

### 5.4 Scrolling

**Most destinations scroll and that is correct** — `S25-am` rules interior market lists scroll with
`S27`'s rail, `S81-am` measured it proportional to the pixel, and PLAYERS demonstrates it working at
1.88× today. **The answer to ~80 offers is more destinations plus scrolling within them, never denser
rows.**

**Do not virtualise.** At this scale it buys nothing and it **breaks the folio's honesty** — a rail
reading `46 of 80` must be backed by 80 real rows.

## 6. WHAT THIS SPEC DOES NOT DO

- **Does not touch the FORM board.** `S81` is closed (§1).
- **Does not add search.** §5.2 replaces it. No search field: it is a web register, foreign to ruled
  paper.
- **Does not price anything.** Which markets exist and what they pay is the engine's.
- **Does not restore two-up rows** (`S92`).
- **Does not settle the amber** (§4.4).
- **The cross-event pivot stays dead** — Allen, 2026-08-16. Match-first, and it is not parked.

## 7. THE GATE — what must be ASSERTED

`C51`: *a cross-element invariant is an assertion or it does not exist.*

1. **Every `MarketKind` member resolves to exactly one destination.** Exhaustive over the enum, so
   **adding a kind without a home fails the build rather than hiding on the surface.** This is `C19`
   made structural instead of promised.
2. **The destination set is constant across matchups** — assert it does not vary with what is priced.
3. **The folio's numbers are derived**: assert `first–last of total` against the rendered row list,
   not against constants.
4. **The contents block's line ranges match the rendered rows** for every destination. A contents list
   that disagrees with the page is the same class of defect as `NEED 0`.
5. **Rendered product-fact size ≥ 13px**, measured where it lands rather than where it is authored
   (`S2-am`).

**What the gate is blind to:** whether the sheet READS — the leaders, the amber, the density at a
glance. Those are `C11` frame claims and no gate speaks to them.

## 8. EVIDENCE OWED BEFORE DESIGN-VERIFIED

1. **The amber comparison frame** — one sheet both ways (§4.4). Lands with the build.
2. **A full-vocabulary sheet** on a matchup pricing all fifteen kinds, showing every destination
   populated.
3. **A sheet with at least one empty group**, for `S89`'s `no prices offered` form.
4. **The contents block and folio at a scroll extent**, for §5.1's derived numbers.

## 9. NOT CLAIMED

- ~~**The rail's ~9 capacity is arithmetic on authored constants and has NEVER been measured.**~~
  **RESOLVED 2026-08-17 — and it was wrong.** The build measured **700px**, not the ~996px assumed,
  and **six destinations overflowed.** `S94` folded `CORRECT SCORE` into `RESULT`; §3 now carries five
  and the amended reasoning. **The flag did its job — but note what it did NOT protect: §3's
  "the headroom is deliberate" was a design claim resting on the unmeasured number, and flagging a
  figure is not the same as declining to build on it.** The remaining claim stands: **the build gates
  at real width and reports overflow rather than resolving it, and that gate is the arbiter** — if five
  still overflows, abbreviation becomes live and returns to this seat.
- **The ~80 figure is the mandate's, not this seat's.** What was measured is ~35 on ENTRY today.
- **Row counts in §3 are estimates from the kinds' own shapes**, not measurements. `CorrectScore`'s
  12–16 is its own source comment.
- **No claim about what the sheet looks like.** Nothing here has been seen; it is a spec, not a read.
