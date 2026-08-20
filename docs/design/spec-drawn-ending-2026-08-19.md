# SPEC — the drawn match's ending (Phase 2, for Allen)

**Written:** Design Director seat, 2026-08-19 · **Batch 128**
**Mandate:** `docs/5-orchestration/dd-mandate-2026-08-18.md` Phase 2
**Ruled by Allen, relayed 2026-08-19:** **the ending belongs to the TICKET.**
**Evidence:** `dd-import/drawn-ending-t129-2026-08-19/` — 608 frames, three arms, read at batch 127.
**Binds:** `tv-design.md` §6.7, §6.8 · `T87-am2`, `T96`, `T97-am`, `T98`, `T108`, `T123`, `T124`,
`T130`, `T135`.

**Status: FOR ALLEN.** The ruling is his; this works out what it means and what it costs.

---

## 0. A CORRECTION TO THE OPTION I PUT TO HIM

I offered the fork as *"the ending belongs to the TICKET — hold the finality line until every leg on
that fixture has swept, **and the rewind stops being visible**."*

**The clause in bold is wrong and I need to say so before speccing to it.** Holding the line does not
stop the rewind: **the clock still reads `FT` at leg 1's whistle and `1'` sixteen frames later**, and
`FT` is itself a finality claim. Suppressing the sentence leaves the contradiction and removes the
only words that were honest about it.

**The ruling is not affected — its intent is unambiguous — but the remedy I attached to it was.**
§3 specs what actually delivers *the ending belongs to the ticket*, and §2 derives why nothing
cheaper can.

---

## 1. THE SUBJECT

How a drawn match's ending plays, for every bet type riding on it. **In scope:** the whistle, the
hold, the leg grades, the settlement, and the boundary between legs. **Out of scope and named:**
cards, the under mirror, the shop, and the acceptance view.

---

## 2. THE DEFECT, DERIVED — a per-leg sweat CANNOT avoid the rewind

`T135` measured it: on a two-leg ticket the clock reads `FT · FT · FT · 1' · 2' · 5' · 9'`.

**This is not a fault in the transition. It is what per-leg sweats MEAN when two legs need the same
whistle.**

- Arm 2's legs are `UNDER 1.5 GOALS` and `BTTS — NO`. **Both are settled at full time.** Neither can
  resolve earlier — no ordering, no gating and no early-decision path changes that.
- A per-leg sweat therefore has to run **each** leg to that same whistle.
- Two runs to one whistle is the match played twice.

**So the rewind is not removable while the sweat is per-leg.** Any remedy that keeps per-leg sweats
is hiding a contradiction rather than resolving one.

### 2.1 `§6.7` ALREADY SPECIFIES THIS BOUNDARY — and it is the one place it does not fire

> **§6.7:** *"Appears only once the stage and active-leg card have **cleared**. **No score, clock,
> tape, event line, suspended label or prior offer survives into it.**"*

Measured at arm 2's leg boundary, f066 → f068:

| §6.7 says must not survive | what actually survives |
|---|---|
| score | survives — `MALLARDS 0 — 0 MIDDLEMEN` |
| clock | survives **and rewinds** — `FT` → `1'` |
| event line | survives — `LEG 1 — WON` → *"Middlemen pin them deep — passes and patience."* |
| suspended label / prior offer | survives — `SUSPENDED` → `CASH OUT $56` |
| the active-leg card | never clears |

**Every item on §6.7's list survives the one boundary that replays the same fixture.** The studio has
a specified device for separating two tellings; it fires between **tickets**, and the leg boundary
gets a hard cut.

### 2.2 And the ending shares a frame with the next leg's jeopardy

At **f066** the surface reads, simultaneously: scorebug **`FT`**, strip **`LEG 1 — WON`**, and in the
column beneath the resolved leg, **`ONE TEAM BLANKED` / `CLEAN-SHEET PATH LIVE`** — leg 2's need,
already live, on the match that just ended. **The word `LIVE` on a finished match**, one slot from the
word `FT`. `T108`'s law, in the same frame as the ending it contradicts.

---

## 3. THE RULING — ONE TELLING PER (TICKET, FIXTURE)

**A fixture is broadcast ONCE per ticket. Every leg of that ticket riding that fixture resolves at
that single whistle.**

This is *the ending belongs to the ticket*, stated so it is buildable. Three consequences, and the
third is what makes it correct rather than merely tidy:

1. **The clock never runs backwards inside a ticket.** There is no second run, so there is nothing
   to rewind.
2. **`THE MATCH ENDS LEVEL` fires once, at that whistle**, and `T87-am2`'s hold is unchanged —
   the line is not re-authored, re-timed or re-scoped. **The ruling costs `T87-am2` nothing.**
3. **The finality claim becomes true.** `FT`, the ending line and the grades all describe a match
   that is over and stays over. Today the surface makes a claim and unmakes it; **`T135`'s point was
   that a finality line followed by a rewind is worse than no line**, and this is the form that
   retires the objection instead of muting it.

### 3.1 Legs on DIFFERENT fixtures are unaffected — and the distinction is the ruling's edge

A four-leg ticket across three fixtures gets **three tellings.** The clock resetting between them is
honest — it is a different match.

> **CORRECTED 2026-08-19, batch 130 (`T140-am`), §1.5.** This section originally read *"`§6.7`'s
> interstitial is already the device that marks that boundary — it fires between fixtures, as it does
> today between tickets."* **It does not fire between fixtures.** `PresentRound`'s loop is
> `TicketCardBeat()` → `PlaySweat()` → `SettlementBeat()`: the interstitial is **once per ticket**,
> and `PlaySweat()` runs every leg of that ticket inside one call with no boundary treatment between
> them, same fixture or not.
>
> **So the hard cut `T139` measured is every leg boundary, not only the same-fixture one** —
> same-fixture is where it produces the rewind, different-fixture is where it would produce an
> unannounced change of match. **Not claimed as a defect: no capture has ever carried a multi-fixture
> ticket, which is what `D2` asks for.** The consequence for this spec is that **multi-fixture tickets
> may need `§6.7` applied at the fixture boundary, and `T140` does not include that work.**

**So the rule is per (ticket, fixture), not per ticket and not per leg.** Stated that way because
"the ending belongs to the ticket" read literally would suppress two genuine endings on a
three-fixture ticket, which is not what the frames argue for and not what the phrase means.

### 3.2 What the shared telling shows

**Nothing new is authored. The column already does this.** Arm 2's f040 shows both legs at once —
`UNDER 1.5 GOALS` / `0 GOALS • LIMIT 1` live, and `BTTS NO −119 NEXT` beneath. **The multi-leg column
is built and shipping; what is missing is that it only ever has one leg live at a time.**

- **Every leg on the fixture is live for the whole telling.** Each carries its own NEED and progress
  line, as the column already renders them.
- **At the whistle, the grades land in leg order**, after `T87-am2`'s hold. N legs, N grades, one
  hold — not N holds.
- **`NEXT` disappears from legs on this fixture.** It means *a telling you have not seen yet*, and
  under §3 there is no such thing within a fixture. It stays for legs on later fixtures.

### 3.3 What this does NOT change

- **`T87-am2`** — the line, its casing, its L2 tier and its minimum hold: untouched.
- **`T97-am`** — the resolved-scene guard: untouched.
- **`T123`** — a goal earns nothing on a count ticket: untouched, and `T129`'s arm 2 confirmed the
  neighbouring question (`§3.1` of the pre-commitment) rather than reopening it.
- **`T108`** — its trigger and its forms: untouched. `T128-cl`'s one-second lateness is a property of
  the hold, not of this ruling, and it survives it.
- **The cash-out slot's behaviour** within a telling: untouched. §5.2 raises one question about it and
  does not rule it.

---

## 4. THE FOUR BET TYPES — what each ending needs, from the frames

The mandate asked for the ending arc for every bet type riding on the draw. All four are now
photographed. **Three need nothing from this spec beyond §3; one is blocked elsewhere.**

| bet type | at the whistle today | what it needs |
|---|---|---|
| **1X2 draw-backer — WINS** | `LEVEL AT FULL TIME` / `LEVEL`; room **+5.65 at f068**; tally to `+$86` | **`T126`** — `LEVEL` repeated across the NEED pair, against `T70`'s standing check. Re-authoring is `G1`'s |
| **team-backer — LOSES** | `MIDDLEMEN TO WIN` / `LEVEL 0–0`; room **−6.62 at f052** | **`T128-cl`** — the NEED names a requirement that cannot be met, for 51 frames. `T108`'s trigger, unchanged |
| **count legs settling level** | `UNDER 1.5 GOALS` / `0 GOALS • LIMIT 1`, `BTTS NO` | **§3** — this is the arm the rewind was found on |
| **correct score `0-0`** | **nothing — the column is blank**, then `CorrectScore` | **`T130`/`G1-am2`** — nine unauthored kinds. **Not this spec's to author** |

**The count arm is the only one whose ending is fixed by §3.** The other three are fixed by rulings
that already exist and are queued elsewhere — which is the useful finding: **the drawn ending's
copy problems are not the drawn ending's; they are the vocabulary's, arriving here first.**

---

## 5. RAISED, NOT RULED

### 5.1 The hold's own structure — still the open direction

`T127` measured that during the hold *the only moving thing is the players still playing*, and did
not rule whether the territory view should hold, settle or clear at the whistle. **`T124-am`
confirmed the hold is 51 frames of a broadcast identical for winner and loser.**

**That question is unchanged by this ruling and is still with Allen.** §3 makes the hold happen once
per fixture instead of once per leg; **it does not give the hold structure**, and the pre-commitment's
recorded lean — *the arc is built by giving the hold structure, not the resolution volume* — is still
a lean and still not ruled.

### 5.2 A live cash-out offer at full time on a settled match

Arm 2 carries `CASH OUT $56` at f068 — after full time, on a 0–0 both legs will win. The lane named
it and correctly declined to rule it.

**Under §3 it changes shape rather than disappearing:** with both legs live to one whistle, the offer
stands until that whistle and the question becomes *may the house offer to buy back a ticket whose
match has ended?* **Not ruled here** — it is a cash-out question (`§6.1`), not an ending question, and
it wants its own frame.

### 5.3 The footer never reaching `STAKE`

`T108` clause 2 working as ruled: `RISK` is a ticket word and may not flip while any leg is
unrevealed. **Under §3 every leg on the fixture reveals at one whistle**, so the footer flips there
if it flips at all. **Recorded as a consequence, not a change** — no new rule, and the arithmetic
should be checked on the re-shoot rather than assumed.

---

## 6. THE GATE

1. **No clock regression within a ticket.** Assert the rendered clock is non-decreasing across a
   ticket's whole sweat, per fixture. **This is `T135`'s own measurement turned into a gate**, and it
   is a string comparison on frames the harness already logs.
2. **One `THE MATCH ENDS LEVEL` per (ticket, fixture)**, counted — not one per leg.
3. **`§6.7`'s clause asserted at fixture boundaries:** no score, clock, tape, event line, suspended
   label or prior offer survives into the interstitial. It is a list and it should be a checklist.
4. **Every leg on the fixture carries a NEED for the whole telling** — which is `T130`'s gate too:
   **a rendered leg row is never empty**, and that assertion would have caught arm 3 before it shot.
5. **Executed-case count reported, non-zero** (`C29`).

---

## 7. EVIDENCE OWED BEFORE DESIGN-VERIFIED

| | what it must show |
|---|---|
| `D1` | the arm-2 ticket re-shot under §3 — **one telling, clock non-decreasing, both legs live throughout, both grades at one whistle** |
| `D2` | a **multi-fixture** ticket — two fixtures, the interstitial firing between them, `§6.7`'s list clean |
| `D3` | the same as `D1` with **one leg losing**, so the shared telling is seen resolving in both directions |

**`D1` is the one to hold the phase on.** It is the only frame that tests the ruling, exactly as `B3`
held the console phase and `S99`'s pin held the laptop's.

**Not requested:** a re-shoot of arms 1 or 3. §3 does not touch a single-leg fixture, and their
endings are already photographed at 150 frames.

---

## 8. NOT CLAIMED

- **No amendment to `§6.8`.** Its reasoning is untouched; §3 changes how often its beat fires, not
  what it says.
- **No copy is authored.** `T126`'s pair and `T130`'s nine kinds are both `G1`'s, and `G1-am2` holds
  that scope.
- **No claim that the hold READS correctly** (§5.1). Still open, still Allen's.
- **No estimate of build cost.** §3 restructures the sweat loop from per-leg to per-fixture and that
  is a real change; **the lane sizes it, and if it comes back expensive the fork returns to Allen
  with a number rather than being quietly narrowed here.**
