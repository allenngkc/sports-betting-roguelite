# The BUST frame (`T149` criterion 2) — binding conditions and the pre-committed read

**Written:** Design Director seat, 2026-08-20, **BEFORE THE FRAMES EXIST** — no window has been
requested yet, and nothing below is shaped by evidence that does not exist.
**Rulings:** `T149` (the three criteria) · `T149-am` (the bust pin) · `T149-am2` (criterion 1
passes on `S1`; criterion 2 untested).

**`T149`'s three criteria are NOT re-authored here.** `T149-am` states they *"stand exactly as
written"* and they still do. **This document adds CONDITIONS, not criteria** — the conditions under
which the frame can test criterion (2) at all, which is the thing `S1` turned out not to satisfy.

---

## 0. FIRST: THE MECHANISM IS BUILT. This is NOT `T148-vf`'s case.

**Checked at this seat before asking for anything** (`C59`), because the last footer criterion died
on exactly this: `T148-vf` found `T133`'s rung 2 unbuilt, so *"does the ladder fire?"* could not be
asked no matter how many frames were shot.

`TvSweatScreen.cs:3091-3092`:

- `Strike.enabled = _cashOutPreview || ticketSettled`
- `ticketSettled = settledCashedOut || settledDead`, where
  `settledDead = _ticket.State == TicketState.Lost && revealedLoss` (`:2966-2968`)

**The commit is `11e4ad7`, 2026-08-19 23:16** — *"the settled ticket stops lying in both directions
— remedy 1's cancelled rows, AND the reveal gate."* Its own comment says the strike *"stays struck
once the ticket actually settles, BY CASH-OUT OR BY BUST."*

**So the bust path is built, the reveal gate is built, and this frame is shootable today.**

---

## 1. WHY NO EXISTING DOCK CAN SUPPLY IT — the survey, run before requesting a window

**77 image docks in `dd-import` were scanned.** One TV candidate carries a genuine lost leg:
`drawn-ending-t129-2026-08-19/arm1-both-endings-150f`, scene003,
`grammar-LegFinalLost__moment-goalless-team-backer-ending`. **It fails twice, and both failures are
worth having on the record.**

1. **It predates the mechanism by three hours.** Frames written 2026-08-19 **20:12**; `11e4ad7`
   landed **23:16**. Read at this seat, `frame149` shows the pre-remedy state exactly — footer still
   `RISK $25` / `PAYS $58` on a ticket that paid nothing, which is `T121`'s defect, not its fix.
2. **AND IT COULD NOT HAVE PASSED EVEN IF IT POSTDATED IT — the ticket has ONE LEG.** The set's own
   README: arm 2 is *"the only two-leg ticket in the set."* `frame149` shows `MIDDLEMEN ML +132 L`
   and `LEG 1 — DEAD`, with no rows beneath it. **A one-leg bust has no unplayed leg to strike, so
   there is nothing for criterion (2) to compare.**

**That second failure is the finding, and it is the one that shapes the brief:** the pin at
`T149-am` said *not a cash-out*. It is now clear that **not a cash-out is necessary and not
sufficient** — a bust with nothing after the loser is as untestable as a cash-out.

---

## 2. BINDING CONDITIONS

1. **THE TICKET CARRIES AT LEAST TWO LEGS, AND THE LOSING LEG IS NOT THE LAST ONE.** This is the
   condition `S1` and the drawn-ending set each missed from opposite directions. Rows *after* the
   loser are the struck ones; rows *before* it are resolved `W` and are not the subject.
2. **THE LOSS IS REVEALED IN THE FRAME.** `settledDead` requires `revealedLoss` — the surface's
   reveal, not the engine's outcome. **A burst taken at the moment of the bust will contain frames
   where the strike has not yet engaged**, and those are not defects. Shoot past the reveal and
   read the settled tail, as the drawn-ending set did for its tally.
3. **`C55` — THE LOST ROW AND AT LEAST ONE STRUCK ROW IN ONE FRAME, both legible.** The comparison
   IS the criterion; two frames cannot make it. Framing is not the risk here — the room camera
   renders `MIDDLEMEN ML +132 L` legibly in the drawn-ending set — **leg count is.**
4. **THE CHROME ROW AND THE FOOTER ARE IN FRAME.** On a bust the footer should read
   `STAKE` / `RETURNED $0`. If it still reads `RISK` / `PAYS`, the settled branch did not run and
   the frame is not of this subject — `T133-am2`'s mistake, restated for this state.
5. **FORCING, IF ANY, IS DISCLOSED IN THE FILENAME** (`S3`, `T133`'s set).

### The recipe this seat believes is cheapest — offered, not imposed

**`GOALLESS-5` already supplies both halves, evidenced on frames that exist:** on that seed a
`DRAW` leg **WINS** (`DRAW +243 W`, drawn-ending read) and a team-backer leg **LOSES**
(`MIDDLEMEN ML +132 L`, same set). The footer dock's own `S1` ticket carried **three legs** on this
seed.

**So a ticket of `[MIDDLEMEN ML, DRAW, DRAW]` on `GOALLESS-5` busts on leg 1 with two unplayed
legs behind it** — criterion (2)'s state, on the seed and matchup the lane is already shooting.
**The lane owns the route; this is offered so the window is not spent searching.**

---

## 3. THE PRE-COMMITTED READ

`T149`'s criteria, unchanged, with what this seat will actually look at:

- **(1) BINARY — every unplayed leg carries the VOID strike and no row prints `NEXT`.** Already
  passed on `S1`; re-confirmed here in the bust's presence, which is where it could still fail.
- **(2) DIRECTION — the struck rows must read as CANCELLED, not as LOST.** The whole window.

### What separates the two states, read from source so the frame is judged against the build

| | LOST leg | cancelled (unplayed) leg |
|---|---|---|
| text tier | **L1** | **L2** |
| state chip | **`L`** | **blank** |
| row background | **extinguished** | not extinguished |
| strike | **off** | **on** |

**Four channels, all differing.** `:3021` states the intent in the build's own words — *"the strike
belongs to VOID and to nothing else; a struck W or L would read as cancelled, which is the one
thing the strike must never say."*

### MY LEAN, ON THE RECORD AND NOT BINDING

**`T149`(3) expected criterion (2) to pass. That expectation STANDS and the source read strengthens
it** — four independent channels is more separation than the criterion needs.

**AND THE SPECIFIC WAY IT CAN STILL FAIL, which the source read is what surfaced: THE BLANK CHIP.**
The cancelled rows are the only rows on this surface carrying **no state word at all**. Every other
row has one — `W`, `L`, `VOID`, `NEXT`. **If a blank chip reads as *nothing happened here* rather
than as *cancelled*, criterion (2) fails in a way the strike cannot rescue**, because the strike is
a mark and the missing word is an absence, and an absence is not a mark.

**This seat's leans get overturned by frames more often than confirmed, which is why both halves are
written down.**

---

## 4. WHAT I WILL NOT CONCLUDE FROM THIS FRAME

- **I will not re-open criterion (1).** `T149-am2` discharged it on `S1`.
- **I will not rule the footer's copy here.** That is `T133`/`T148`'s thread and it has its own
  window. `STAKE` / `RETURNED $0` appearing correctly is condition 4's *screen-state check*, not a
  copy read.
- **I will not read the drawn ending, the grade or the room.** Nothing in this window bears on them.
- **I will not accept a frame where the struck row and the lost row are in different captures.**
  Condition 3, and it is the condition most likely to be met in spirit and missed in fact.

---

## 5. WHY THIS DOCUMENT EXISTS

Three of this thread's four criteria were spent on frames that could not test them: `E2`'s ladder
clauses were unexecutable because the rung was unbuilt (`T148-vf`), and criterion (2) was shot on a
cash-out that the pin had already said would prove nothing (`T149-am2`). **Both were knowable before
the shutter, and one of them — the one-leg ticket — was knowable from a dock already on disk.**
This document is the check run first instead of last.
