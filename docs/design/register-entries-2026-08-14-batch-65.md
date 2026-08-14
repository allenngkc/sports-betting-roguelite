# Register entries — 2026-08-14, batch 65

**THE TWO DRAW ITEMS**, taken at Allen's instruction, board row first. Ruled at the DD seat on
`surething-design.md`, S74/S24/C47, and the board as it actually renders —
`dd-import/phase-l-push/final/…-01-form-lobby-flat-1024x704.png`, read at this seat.

**Destination tables: SureThing — the laptop** (`S74-am`) · **TV — match theater** (`T87-am`).

**Rows shipped:** `S74-am` · `T87-am`. **No new IDs** — both are the composition owed by a ruling that
already exists, and C22.1 keeps them on their governing lines.

---

## S74-am — THE BOARD'S DRAW ROW: the composition, ruled from the board's own grammar.

S74 ruled the **form** — three offers, the draw in the middle, named `DRAW`, no team treatment, `1X2`
never reaching him — and `surething-design.md` §"The draw on the board" carries it. **What neither
carries is the LAYOUT**, and the owning document states only the negative: *a two-outcome paired row no
longer describes this market and must not be forced to.* **A negative is not a composition.** This is
that composition.

### The board's grammar, read off the frame rather than assumed

A matchup is **not** one row with two price cells. It is **two stacked lines**, each carrying one
side, with the price on the same line as the team it belongs to:

```
NO.  MATCHUP · SEASON RECORD          MONEYLINE       MORE
01   NOTARIES   4-5                   AWAY  −156     ┐
     FERRETS    5-4                   HOME  +127     ┘ MORE ›
```

**Two facts in that layout decide everything below.**

**1. The price cell already carries the OUTCOME word, not the team.** It reads `AWAY −156`, not
`NOTARIES −156`. The left column names **who**; the price cell names **which outcome**. **So the board
already has the draw's grammatical slot, and nothing is invented** — the same shape as S74's own
finding that `LEVEL` was already the product's word for a tied scoreline.

**2. The organising principle is *the price sits beside its subject*.** That is what makes the slate
scannable, and it is the thing a third offer can most easily break.

### RULED — the draw takes its own line, between the two teams

```
NO.  MATCHUP · SEASON RECORD          MONEYLINE       MORE
01   NOTARIES   4-5                   AWAY  −156     ┐
                                      DRAW  +240     │ MORE ›
     FERRETS    5-4                   HOME  +127     ┘
```

- **`DRAW` goes in the PRICE CELL**, exactly where `AWAY` and `HOME` live — the cell that names the
  outcome. **Not in the matchup column**, which names teams, and putting `DRAW` there is the category
  error S74 and the engine both refuse.
- **The matchup column is EMPTY on that line, and empty is the CORRECT rendering of "neither".**
  This is **not S24's dead cell**: S24 refused *an offer slot with no offer*; here the **subject** slot
  has no subject, because the draw has no team. **Naming anything there would invent the third
  competitor `Side` exists to refuse.**
- **The middle position is now literal.** S74 ruled the draw's position is *meaning, not borrowed
  convention* — **and in this layout it is meaning you can see**: the draw's line sits physically
  between the two teams' lines, attached to neither, which is exactly what the outcome is.
- **`MORE ›` spans the block, unchanged**, now three lines instead of two.
- **No team treatment on that line**: no dot, no crest, no hue (T2 gives muted blue and pink to the two
  sides; **a draw has no side and takes no hue**).

### The cost, named rather than discovered

**A third line per block is roughly a third fewer matchups visible.** Read off the frame at this seat —
line pitch **38px** inside a block, block pitch **78px**, list area **~490px**, so **six blocks today
and about four at three lines.** *(Eyeball readings off the rendered canvas, not measurements — the
lead measures them.)*

**This does not breach C19.** The interior market list **scrolls** (S25-am) with **S27's printed
position rail**, so every priced offer stays reachable and the rail states where he is. **Reachability
is preserved by a mechanism that already exists.**

**§2's yield order is NOT invoked and must not be** — spacing, then repetition, then nothing. **Nothing
here is a deficit to yield against**: a third outcome is a product fact arriving, not a layout
overflowing, and **§2's own sentence binds — nothing that states a product fact is deleted to make a
layout fit.**

### `MONEYLINE` stands as the column header

It names the **market**, not the number of outcomes, and the market's identity has not changed — only
its outcome count. **Renaming it would be a vocabulary change with no defect behind it**, and S22
governs: the surface composes and the role is printed as a word. **`1X2` remains out** (S74).

### Owed, and pre-committed so it costs one pass

**OWED: the block's re-derived height and the visible-count, measured** — a fixed grid constant
re-derived **once at design time is explicitly legal** (§2, T51, S40); **a zone resizing to content at
runtime is not**, and the block must be sized for three lines whether or not a given matchup prices a
draw.

**PRE-COMMITTED: (1) every match prices a draw** — the engine's `MatchResult {Home, Draw, Away}` says
it does — **→ every block is three lines, uniform, and this closes with no further ruling; (2) some
matches price no draw → the block is STILL three lines** and the draw's line renders empty, **because a
ragged board whose block height depends on the market is a zone resizing to content**, which §2 forbids
— and an empty line is honest where a collapsing block is not; **(3) on frames the blank matchup column
makes the draw's line read as detached or as a separator → the remedy is the PRICE CELL's own
treatment**, which already carries the word `DRAW`, **never a token in the matchup column.**

**Fit is not asserted** (S74's own closing line): `DRAW` and its price are new strings in the canon face,
they measure against their cells like everything else, and they join the sweep's population under C46.

---

## T87-am — THE SCORELESS DRAW: T87 REACHES IT. No new beat, one risk named, frames owed.

The question queued was whether **0–0 needs its own read.** It does not need its own **ruling**, and it
does need its own **look**. Those are different, and separating them is the whole of this row.

### T87's ruling reaches 0–0, and the reason is mechanical rather than a judgement call

T87 ruled the drawn beat is **the match ENDING LEVEL, STATED** — the scoreline holds at its level
value, the event strip states the fact at its own L2 tier (T66), the legs resolve to their words.

**Every mechanism it names is GOAL-INDEPENDENT**, so none of them is absent at 0–0:

- The scoreline **holds a value it already holds**; 0–0 is a level value like any other.
- The event strip **states a fact**, and the fact is available without a goal having occurred.
- The legs **resolve to their words** — S74's `LEVEL AT FULL TIME` / `LEVEL` for a draw-backer,
  `{CLUB} TO WIN` / `LEVEL 0–0` resolving LOST for a team-backer. **`LEVEL 0–0` is already rendering
  correctly**; this seat read it on the closing frames.
- **T65's settlement glow fires on SETTLEMENT, not on a goal** — room's own record confirms it is keyed
  to the moment rather than to a win — **so a goalless match still settles as a first-class settlement
  moment in the room.**

**Ruled: no new beat is authored for 0–0, and nothing existing may be narrowed to exclude it.** That
last clause is T87's own and it is repeated here because a goalless match is exactly the case a
narrowing would quietly drop.

### What 0–0 changes is the RISK, not the rule — and this is why it earned a read

T87 named two failure modes either side of the beat: **manufacturing a climax** (closed by T35/T40) and
**rendering nothing**, which *"makes a resolution draw as an absence, which reads as a bug and teaches
the player a false rule."*

**At 0–0 the second risk is at its maximum, and only at 0–0.** In a 1–1 the surface has punched, the
scorebug has changed, the room has moved — the ending arrives against a match that visibly happened.
**In a 0–0 nothing has punched all match**, so a quiet ending arrives against a quiet match, and
**the one state the surface must never be mistaken for is idle.**

**The checkable question, stated so the read has a subject: is the stated ending DISTINGUISHABLE from
the surface simply idling?** Not *is it loud* — T87 forbids that — but **is it legible as a
resolution.**

### And the half that matters most, restated because 0–0 is where it is easiest to lose

**A draw is quiet for the room and LOUD for one ticket.** The draw-backer **has won**, and he has won on
a match where nothing happened. **His leg lands like any other winning leg**, at the same treatment, on
the same settlement moment. **The worst outcome available here is a surface that reads a goalless match
as a non-event and drains the one player whose ticket just came in** — and 0–0 is the precise case where
that error is cheapest to make.

### NOT RULED — the read itself, because no frame shows it

**No 0–0 full-time frame exists in evidence at this seat.** The `LEVEL 0–0` readings this seat has are
**mid-match** (11', 32'), which is the progress line doing its job and says nothing about the ending.
**C11 binds: a claim about how the ending reads is made against a rendered frame of the ending.**

**OWED: a goalless match to full time — the settlement beat, the event strip's full-time line, the
room's glow, and both tickets' legs resolving** (a draw-backer's and a team-backer's, so the loud half
and the quiet half are in the same set).

**DISPOSITIONS PRE-COMMITTED so this costs one window and no round trip: (1) the ending is legible as a
resolution and the draw-backer's leg lands at full treatment → T87 covers 0–0 with no amendment and
this CLOSES; (2) the ending reads as the surface idling → it is T87's own named failure mode and the
remedy is the EVENT STRIP stating the fact**, which T87 already assigns it, **never a flourish** (T35,
T40 closed that on mechanism); **(3) the draw-backer's settlement is muted relative to a goal-won leg →
that is a defect and it is the serious one**, fixed at the settlement path and not by adding a beat.

**Nothing here blocks the markets lane.** The board row (S74-am) is buildable now and does not wait on
this capture.
