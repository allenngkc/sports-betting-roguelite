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

## 3. THE DESTINATIONS — six, fixed, and always printed

**The hybrid taxonomy transfers and every book converged on it:** statistic names where the thing is
physical and countable, bet-type names where it is abstract. **Do not purify it.**

**Rail order is the table's order** (`S95`). Row counts below are **measured over 18,000 matchups**,
not estimated.

| # | destination | holds | rows (min–max) |
|---|---|---|---|
| 1 | **RESULT** | Moneyline · DoubleChance · Handicap · WinningMargin | **13** (fixed) |
| 2 | **GOALS** | TotalGoals · TeamTotalGoals · TotalGoalsOddEven · BothTeamsToScore | **18** (fixed) |
| 3 | **CORNERS** | TotalCorners · TeamTotalCorners | **10** (fixed) |
| 4 | **CARDS** | TotalCards · TeamTotalCards | **10** (fixed) |
| 5 | **CORRECT SCORE** | CorrectScore | **11–16** |
| 6 | **PLAYERS** | AnytimeScorer · PlayerMultiScorer | **17–24** |

**Offers per matchup: 79–90, mean 84.78.** The mandate's ~80 is the **floor, not the centre** — the
folio's denominator reads up to 90. **Four of six destinations are structurally fixed height**; only
CORRECT SCORE and PLAYERS move with the matchup, which is what §5.4's scrolling argument turns on.

**Six destinations for fifteen kinds, against a rail measured at 700px — and they FIT.**

**Amended twice; read this before quoting the history.** `S94` (2026-08-17) folded `CORRECT SCORE`
into `RESULT` on the ground that six overflowed. **`S94-cl` (2026-08-18) WITHDREW that: six do not
overflow.** Measured — labels 460.86, boxes 604.87, plus 5×8px gutter and 2×14px margin =
**672.86 of 700, slack 27.14px**, and the live gate confirms the pack fits in-engine.

> **~~The headroom is deliberate.~~ STRUCK as false (`S94-cl`).** 27.14px cannot hold a seventh:
> `MakeButton` floors a control at 44px plus an 8px gutter, and dropping the rail to `LaptopTrack.Tabs`
> reaches only 43.91px. **The real capacity is exactly six.** **RULED: a seventh destination requires
> a DD ruling BEFORE it is proposed.** The protection §3 originally claimed does not exist, and
> **naming that is worth more than a fold that would only have deferred it.**

**Nothing was shrunk, truncated, abbreviated, wrapped or scrolled to make six fit** — the packing is
derived from measured labels. **A near miss worth keeping:** the ceiling for symmetric padding is
14.26px/side, and the strip this replaces ran its tightest box at 15.08px/side. **Carrying the old
grammar forward would have overflowed by ~9.8px.**

**Three derivations, so the grouping is not read as taste:**

- **Team-splits fold into their statistic, never into a "TEAM TOTALS" group.** Countables take
  statistic names, and a player asking *how many corners will they get* looks under CORNERS. Each
  stat group is then a **complete** answer to its statistic.
- **BTTS folds into GOALS.** It is a two-row market about goals; it does not earn a destination. **It
  does not become hard to find — §5 names it in the contents.**
- **CORRECT SCORE keeps its own destination** (`S94-cl` — the earlier fold is withdrawn), **and it is
  seated FIFTH, not third** (`S95`). Two reasons pointing one way: the order runs the three countable
  statistics **adjacently** — `GOALS · CORNERS · CARDS` — which is the same statistic-vs-bet-type seam
  this taxonomy is built on; and **the rail is popularity-led**, so one of the least-bet markets does
  not take third place. **The original table interleaved bet-type/statistic/bet-type and contradicted
  its own reasoning.**

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

A row with a name at one end and a price at the other **needs a device or it is just emptiness.**
`S92` ruled that the width is the **annotation gap** rather than waste; a gap that is doing work
should look like it.

**Corrected 2026-08-18: this section originally said "a ~996px row." The row is 700px** (692 inside
the position rail), and with a 176px price cell plus 14px pad **the gap is still ~490px.** **The
ruling survives its wrong premise** — a 490px gap needs the device just as much — but the number was
wrong and is not left standing. **Confirmed on frame (`S4`): the leaders run the full gap and the
column reads as one statement rather than two facts at opposite ends.**

**RULED: the offer row is ONE statement, not two facts at opposite ends. Leader dots carry the name to
its price.**

**This invents nothing** — `S89` already puts leader dots in the product (`CORNERS ….. 11`). This is
the same device one level down, which is exactly how a racecard uses it.

### 4.4 The amber — RULED ON THE FRAME (`S97`, closing `S91` half two)

**Does the price take the money-amber?** The law says amber = money and a price is money; against it,
**~80 amber prices on one sheet, and amber is also the ACTION colour — if everything is amber, nothing
is.**

**DECIDED ON THE FRAME, 2026-08-18 (`S97`): NO. THE PRICE STAYS IN TONER.** Read on `S4` against
`S5` — same seed, same matchup, same scroll, one variable.

**The decisive reason was not the expected one: amber UNDOES §4.2.** In the amber state the prices
become **the most saturated element in the market column** and out-compete the market names beside
them. **Name-first is already ruled; the half that is ruled wins.** Plus: it dilutes amber, which
today says *money you might win* (the `$0` payout) and would widen to *any money figure*; and
**eighty amber marks on one sheet is not marking anything.** The toner sheet already works — the
leaders carry the eye and the column reads as a quiet annotation rail.

**The best argument FOR amber is on the frame too and is answered rather than ignored:** the slip
says `CIRCLE A PRICE TO START A TICKET`, so the price *is* the action. **True of every price equally
— which is exactly why it cannot mark any of them.**

**Seat's recorded lean was YES and the frame overturned it.** That is why §4.4 sent it to a frame.

**Named, not ruled: amber's real claim is the SELECTED price** — where a price stops being the
house's offer and becomes the player's stake. It belongs with the selection treatment.

### 4.5 Unchanged, named so nothing is "tidied"

Type does not shrink; the **13px product-fact floor is law**; row pitch stays 54px; status is never
carried by colour alone; **suspended is greyed, non-clickable AND stated.**

### 4.6 Row-name CASING — the sheet uppercases (`S96`)

**Row names are uppercased at the presentation layer.** Confirmed in pixels on `S4`/`S5`:
`Moose Jaw Overheads` sits in title case in the same column, at the same size, beneath an uppercase
`MONEYLINE` and beside an uppercase `DRAW` and `EITHER TEAM`.

**`A2` is NOT overridden, and the distinction is the whole ruling: `A2` fixes the WORDS — the
engine's own field, verbatim in content. Case is TYPOGRAPHY, and typography is the surface's.** The
surface already sets its own face, size, tracking and colour on that string; case is the same kind of
thing.

**And it is not *proper nouns keep their case*:** the engine is inconsistent with itself —
`DARRYL LEDGER ANYTIME` uppercases a player's name while `Waterloo Notaries OVER 0.5 GOALS` does not
uppercase a club's.

**Two bindings before it ships.** **`C46`:** uppercase is wider per character, and
`MOOSE JAW OVERHEADS OR DRAW` is the longest reachable form — **measure it against the row.**
**`S84`:** check the **enumerated** club and player pools for any name whose meaning depends on its
case. The measured sample shows none; **the check is the pool's, not the sample's.**

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
RESULT ...................... 1–13
    MONEYLINE ............... 1–3
    DOUBLE CHANCE ........... 4–6
    HANDICAP ................ 7–10
    WINNING MARGIN .......... 11–13
GOALS ....................... 14–31
    TOTAL GOALS ............. 14–19
    TEAM TOTAL GOALS ........ 20–27
    ODD / EVEN .............. 28–29
    BOTH TEAMS TO SCORE ..... 30–31
CORNERS ..................... 32–41
    TOTAL CORNERS ........... 32–37
    TEAM TOTAL CORNERS ...... 38–41
CARDS ....................... 42–51
    TOTAL CARDS ............. 42–47
    TEAM TOTAL CARDS ........ 48–51
CORRECT SCORE ............... 52–62
PLAYERS ..................... 63–82
    ANYTIME SCORER .......... 63–79
    MULTI SCORER ............ 80–82
```

**`TEAM TOTAL GOALS`, not `TEAM TOTALS` (`S98`).** The long form carries the statistic in all three
places. The short form is unambiguous *on the sheet* — a `TEAM TOTALS` row under CORNERS can only
mean corners — **but this contents block is a flat printed list, and the short form puts the same
label in it three times, disambiguated only by indentation.** A contents list exists to be scanned.
**General form: where a market name appears in both the sheet and the contents, it is authored for
the contents — the harder reading — and the sheet inherits it.**

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
  **RESOLVED 2026-08-18. The width was wrong; the capacity claim was wrong; the six FIT anyway.**
  Measured: **700px**, not ~996 (the 996 counted the betslip's 324px column as available to the
  market sheet). The pack is **672.86 of 700, slack 27.14px**, live-gated in-engine. **§3's
  "the headroom is deliberate" is STRUCK** — the real capacity is exactly six, and **a seventh
  destination now requires a DD ruling before it is proposed** (`S94-cl`).

  **Recorded because it is the sharper lesson: §9 flagged the number as unmeasured, and this seat
  then ruled a taxonomy change (`S94`) on a MISREAD of a status relay, in the same turn it docketed
  the document that would have corrected it.** Flagging a figure is not the same as declining to
  build on it, **and a ruling that turns on a measurement waits for the measurement.**
- **The ~80 figure is the mandate's, not this seat's.** What was measured is ~35 on ENTRY today.
- **Row counts in §3 are estimates from the kinds' own shapes**, not measurements. `CorrectScore`'s
  12–16 is its own source comment.
- **No claim about what the sheet looks like.** Nothing here has been seen; it is a spec, not a read.
