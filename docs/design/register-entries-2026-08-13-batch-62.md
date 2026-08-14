# Register entries — 2026-08-13, batch 62

**THE LAST AUTHORING.** Ruled at the DD seat on `dd-import/tv-g1am6-pool-2026-08-13.md`
(tree `43896ac`, suites green).

**Destination table for both rows: TV — match theater.**

**Rows shipped:** `G1-am7` · `T94`.

---

## G1-am7 — THE MONEYLINE NEED ARM, AUTHORED. A two-rung ladder, selected by measurement.

### The answer that decided it, accepted in full

**First half YES** — `awayMark`/`homeMark` render **only on moneyline legs**, exactly this arm, and mark
the **picked** side: T23's backed locator and T42's form-not-colour discriminator, already built.

**Second half NO** — and the mechanism is named at the line: `TvSweatScreen.cs:1652–1653` advances the
column's live row to leg **N+1** the instant leg N resolves, while the scorebug holds leg **N**'s
fixture until the next leg stages. **The window is not a frame** — it spans the whole won-leg or
dead-leg beat, with the ticket column on screen throughout.

**So bare `TO WIN` is unsafe, exactly as disposition 2 pre-committed.** During that window it would
name no side at all, and the marker it would have leaned on is pointing at a different fixture.
**The club must be named. Disposition 2 fires.**

### The authored ladder

**RULED — two rungs, and the surface picks between them by MEASUREMENT, never by truncation
(`FitOrFallback`, already the mechanism):**

| rung | form | when |
|---|---|---|
| 1 | **`{CLUB} TO WIN`** | fits 261.0 — **15 of the 20** |
| 2 | **`{CLUB} WIN`** | the other five |

**Bare `TO WIN` is RETIRED as this arm's fallback.** It must not be reachable on a moneyline leg. It
was the cheap answer and disposition 2 is precisely the finding that it is not available.

### Why `{CLUB} WIN` and not something shorter or cleverer

**It keeps the club, which is the whole reason authoring reopened.** Every alternative that fits by
touching the club word fails a standing rule:

- **Abbreviating the noun is REFUSED** — `SPRDSHTS` is a coined short form, and T88-am's standard is
  explicit (*no abbreviation coined — G1's defect class avoided rather than survived*); T84-am4 ruled
  the same for a state chip. **The noun is already the club's short form** — the compact rows print
  `MUSKRATS ML`, not `Tulsa Muskrats` — so there is nothing left to shorten without inventing.
- **`HOME`/`AWAY` is REFUSED** — it binds to the fixture on screen, and during the very window that
  made this necessary the fixture on screen is the wrong one. It fails in exactly the case it is
  needed.
- **`{CLUB} ML` is REFUSED** — `ML` is a market label, not a requirement. The NEED line states **what
  must happen**; a market name does not.
- **Wrapping to a second line is REFUSED** — the band's geometry was settled one batch ago at T90-am,
  and re-deriving it again to buy a string is the shape §3.5 exists to stop.

**And it is the slot's OWN register, not a new one.** The NEED deck is already terse declarative —
`ONE TEAM BLANKED`, `ONE TEAM SCORELESS` are **subject + required state**, with no infinitive and no
verb "to be". **`SPREADSHEETS WIN` is that same shape**, and it is grammatical because **every noun in
the pool is plural.** The ambiguity worth checking — *could it read as a result rather than a
requirement* — is answered by the slot itself: `ONE TEAM BLANKED` has carried that identical risk since
G1 and reads correctly, because the NEED slot only ever states requirements and the progress line
directly beneath carries the live state (`LEVEL 0–0`, `LEADING 1–0`).

### The measurement, owed before the window and cheap

**Not asserted here (§2.5, C41): this seat has not measured `{CLUB} WIN`.** From the pool's own
figures the club-plus-space budget is **167.3px** (261.0 − 93.7) and the worst club runs **196.2px**,
so rung 2's worst case lands **somewhere near 247px** on the arithmetic — **a direction of travel, not
a number to land on**, and the residual against a 261.0 box looks comfortable rather than marginal.

**OWED: `{CLUB} WIN` measured for all twenty, against 261.0.** Seconds, not a window.

**PRE-COMMITTED so it cannot cost a round trip: (1) all twenty fit → the ladder is final, the window
fires, nothing returns here; (2) any club still overruns → it returns with the widths and this seat
authors rung 3 — and the remedy will be THE PHRASE, never the club, since abbreviating the noun stays
refused in every branch.**

### The window — FIRE IT, as one pass

**Answering TV's question directly: no separate build-verification pass.** All five fixes are built;
fold them into the single closing capture. A verification window that is not the closing window spends
a capture to learn something the closing capture reports anyway — **the same economics that made
flagging this residual early worth so much.**

**TV's decision to HOLD the capture was correct and is endorsed.** Running it against a string known in
advance to fail the gate would have produced frames that could not close the phase and guaranteed a
second window. **A lane that declines to spend a window on a foreseeable failure is doing the job the
capture budget exists for**, and it routed the call rather than making it alone.

---

## T94 — the ticket column and the scorebug describe DIFFERENT LEGS at the same time.

**NEW — surfaced by TV's answer to G1-am6, and it is the reason the cheap answer was unavailable** ·
DD 2026-08-13 batch 62.

The column's live row advances to leg **N+1** on leg N's resolution (`TvSweatScreen.cs:1652–1653`)
while the scorebug holds leg **N**'s fixture until the next leg stages. **For the length of the
won-leg or dead-leg beat, the surface states a requirement about one match while displaying another.**

**T59's family, on the ZONE axis.** T59 ruled *display state and input state are the same state, read
from one value*; C48 extended it to the gesture axis. **This is two display zones rendering two
different legs of one ticket simultaneously** — the surface disagreeing with itself, where every prior
instance of this family was the surface disagreeing with its input.

**NOT RULED, and deliberately: the current behaviour may be correct.** A column that looks ahead to
what is next while the scorebug finishes the beat is a defensible reading, and its alternative —
holding the NEED line on a leg already settled — **is a state lie of its own** (T43's class: a slot
describing a condition that no longer applies). **This seat is not choosing between two interaction
readings without seeing the beat**, which is the same restraint applied at T84-am6, T90 and T91-am.

**OWED when the phase is closed: the won-leg and dead-leg beats on frames**, so the two readings can be
compared where they actually happen rather than argued from a line number.

**Consequence recorded so it is not lost: this desync is the ONLY reason bare `TO WIN` is unsafe.**
The marker already identifies the backed side on exactly this arm. **If T94 resolves toward
synchronisation, bare `TO WIN` becomes legal again and G1-am7's rung 2 can retire** — a cheaper deck by
one string. **Neither is done now, and G1-am7 does not wait on it**, because the phase closes on the
string that is safe today rather than on one that would be safe after an unruled interaction change.

**Does not gate Phase T** (C31 — a new item on evidence already in hand), and it is not on the closing
capture's critical path.
