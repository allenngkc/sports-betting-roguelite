# The count sweat — the read, against a matched control

**Written:** Design Director seat, 2026-08-16 · **Against:**
`dd-import/corners-sweat-2026-08-16/` (182 frames, 21 windows) and
`dd-import/goals-control-2026-08-16/` (136 frames, 17 windows) · **Pre-commitment:**
`capture-precommit-2026-08-16.md` §3

The pair is the instrument: same seed, same fixture, same pacing, same window predicates, same
stake, **identical final scoreline** — one variable, `OVER 8.5 CORNERS` against `OVER 1.5 GOALS`.
The control's defining property is asserted rather than described (`countEvents=0 · countLedger=-1`),
so the two arms cannot quietly differ by more than their market. **This is a properly built control
and it earned the verdict below.**

Condition 1.2 is met and **this set is not a null**: approach, crossing, and a leg decided with
match time still to run all occurred.

---

## 1. THE PREMISE IS CONFIRMED — AND ITS OBVIOUS EXPLANATION IS REFUTED

*"A corners bet settles correctly but watches flat."* Confirmed. **But not for the reason the
framing implies, and the control is what proves it.**

| | corners | goals |
|---|---|---|
| **event windows** | **7** | **3** |
| dead-air windows | **9** | **11** |
| sweep duration | 41.42s | 35.40s |

**The corners arm has more than twice the events and fewer dead stretches.** By every count of
*things happening*, the corners watch is the busier of the two — and it is the one that watches
flat. **Event scarcity is not the cause.** Any spec written to "add more count beats" would be
solving a problem this pair says does not exist.

## 2. THE CAUSE: THE CORNERS ARM HAS NO RESTING STATE

The grammar token rides in every filename, so each frame states its own beat classification. Laid
side by side, the two arms are a different shape entirely:

**Corners — `CornerFor` on scenes 002 through 015. Fourteen consecutive windows, one token.**

**Goals — `CalmPossession` → `GoalAgainst` → **back to** `CalmPossession` → `LegFinalWon`.**

The goals arm has a **baseline it departs from and returns to.** That return is what makes the
departure legible: `CalmPossession → GoalAgainst → CalmPossession` is a *contour*, and a contour is
what a watch is made of.

**The corners arm has no calm.** From the first corner to the last it is one classification, so
there is nothing for an event to be an event *against*. Seven departures from nothing are flat in
exactly the way three departures from a baseline are not.

**This is the finding, and it is structural rather than a matter of volume.** It also predicts the
fix is smaller than "author a set of count beats": the corners watch needs a **resting state**
before it needs anything else.

## 3. THE STRIP REPEATS ITSELF, VERBATIM, THREE TIMES

Seven count events produced **four distinct strings**:

| # | sim | line |
|---|---|---|
| 1 | 4.50s | `corner kick won. another little number for the ledger. (2 in the spell)` |
| 2 | 8.88s | `the flag goes up; pressure becomes a corner. (2 in the spell)` |
| 3 | 12.72s | `Spreadsheets squeezing the half.` |
| 4 | 17.24s | **= #1, verbatim** |
| 5 | 21.24s | **= #2, verbatim** |
| 6 | 25.62s | `whipped into the corner — the count moves again.` |
| 7 | 29.46s | **= #1**, less its parenthetical |

String #1 appears three times in a 44-second watch, twice verbatim, about 13 sim-seconds apart.

*Recorded carefully:* the dock's prose reads *"Seven distinct lines across the whole watch"* while
the table immediately above it shows they are not distinct. **The table is right and the sentence is
wrong** — noted without blame, because the dock supplied the evidence that corrects it, which is the
system working. But the sentence is the kind that gets quoted later.

## 4. A STATE LIE, AND IT IS THE MOST SERIOUS THING IN THE SET

**On the frame at 66' — thirteen minutes after the bet could no longer lose:**

> `OVER 8.5 CORNERS`
> **`10 CORNERS • NEED 0`**
> `RISK $25`  ·  `PAYS $29`

The leg was won at 53' when the count crossed 8.5 at ten. At 66' the surface still prints:

- **`NEED 0`** — the same construction as `NEED 1` with a different number. It reads as *a
  requirement that happens to be satisfied*, not as *you have won*. A player scanning the column
  sees the shape of an outstanding requirement.
- **`RISK $25`** — **there is no risk.** The leg cannot lose. The word is false at the moment it is
  printed, and it is printed in money amber.
- A strip reading `whipped into the corner — the count moves again`, narrating a count that has
  stopped mattering to the only ticket on screen.

**RULED — this is a defect, not a preference, and it does not need a new direction to fix.** It is
the `T103` class exactly: the surface asserts a state the player's own position contradicts. `T103`
was ruled on a mirrored column order that *could* mislead; this one **states a live requirement and
a live risk that do not exist.**

Two amber words in the ticket column — `RISK` and the `NEED n` construction — go stale the instant
the leg resolves and stay stale for a quarter of the match. **My §5 hand-over proposal is confirmed
and upgraded: it is not attention-management, it is a correctness fix.**

## 5. THE CORNERS PLAYER WATCHED A DIFFERENT MATCH — AND A FALSE ONE

Same seed, same fixture, both arms finishing `REGULATORS 5 — SPREADSHEETS 1`:

| arm | revealed scoreline |
|---|---|
| **corners** | `0 — 0` **held to `90'+1`** → `0 — 1` → `90'+2` `5 — 1` |
| goals | **`1 — 0` at 30'** → `90'+1` `1 — 1` → `90'+2` `5 — 1` |

**The corners arm never showed the 30' goal at all.** The scoreline read `0 — 0` for **86% of the
watch**, on a match that finished 5–1.

This explains the dock's routed finding #2 — the score stepping `0—1` → `5—1` across two beats —
and it is not a rendering bug: the revealed ledger discloses what the **ticket rides on**, and a
corners ticket does not ride on goals. So five goals arrive at once because none was ever revealed.

**But the consequence is severe and compounds §2.** The corners player is shown a goalless match
that was not goalless, then handed the real result in two steps at the death. The stage is drawing
players and a pitch the whole time; **the one fact that would have made the watch a match was
withheld**, and the arm with a resting state is also the arm that got to see a goal.

**This is the largest design question in the phase and it is Allen's**, because it is a direction
rather than a defect: *does the theater show the MATCH, or only the parts of the match the ticket
rides on?* Today it is the latter, and the cost is that every non-goal ticket watches a nothing-match.

## 6. THE OTHER ROUTED FINDING — CONFIRMED ON FRAME

**The flavour strip clips mid-word.** At 48' it renders

> `corner kick won. another little number for the ledger. (2 in the spe`

The line overruns its box and the backstop cuts inside the final word. This is `C46`'s family — a
fixed box carrying an unstated assumption about the string it was sized for — and `G1`'s own
standard applies: **authored fallbacks so truncation is never reached.** The strip's strings have
never been swept against their box.

Related and already owed: `T101`'s residual, *the panel's own strings have not been swept under
C46*. **Same defect class, second surface. They should be swept together.**

## 7. WHAT THE READ CHANGES ABOUT MY OWN EXPLORATION

- **§2 (the rate line) is refined, not confirmed.** I wrote that the panel *"shows the count and not
  the pace."* On frame the ticket column **does** carry distance — `8 CORNERS • NEED 1`. What is
  absent is **time**. So the proposal narrows: distance exists, the clock is not in the sentence,
  and `NEED 1` at 48' says nothing about whether that is comfortable or desperate.
- **§5 (the hand-over) is confirmed and promoted** — see §4. It is now a correctness item.
- **§2 is demoted below §2-of-this-document.** The resting state is the cheaper and more structural
  fix, and it is what the control isolates.
- **Room exists for all of it.** The ticket column is empty from below `NEED n` to `RISK/PAYS` —
  roughly **46% of the column's height on a one-leg ticket**.

## 8. NOT CLAIMED

- **Nothing about cards.** No cards arm was shot. Booking drama is untouched by this read and its
  absence here is not evidence about it.
- **One line each, both comfortable winners.** A leg that lands *near* its line, or loses, is a
  different watch and is not in evidence.
- **The `LegFinalWon` flip timing is not fully explained.** The goals arm flips at scene012, before
  its second goal is revealed at scene014 — which suggests the token follows the *resolved* match
  rather than the *revealed* one. Consistent with §5, not established by it.
- **No claim from the room-graded frames about the scorebug's left edge.** It appears clipped, but
  these are camera captures through the TV housing and grade; that needs a flat frame to judge.
