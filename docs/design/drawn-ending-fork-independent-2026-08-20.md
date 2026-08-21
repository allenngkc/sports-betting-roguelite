# The drawn-ending spec — what builds INDEPENDENT of the `T140` fork

**Written:** Design Director seat, 2026-08-20, on Allen's approval of
`spec-drawn-ending-2026-08-19.md` **as written**, with the `T140` A/B fork **not yet ruled**.

**For TV.** Everything below is buildable now and stays correct whichever arm Allen takes.
Everything in §2 is not — and one item there would be actively wrong under arm (B).

---

## 0. The fork, restated so the split is checkable

- **(A)** `T140` as ruled — one telling per (ticket, fixture). **LARGE**, engine escalation; phase 1
  is not executable by this lane (`engine/` is read-only to it).
- **(B)** `§6.7` at the **leg** boundary — presentation-only, one existing device at one more site.
  The clock still runs backwards; the boundary reads as a boundary.

**The test for independence:** does the item hold under both? Not *is it small*.

---

## 1. BUILDS NOW — four items

### 1.1 `§6.7`'s interstitial at the FIXTURE boundary — the big one, and it is required under BOTH arms

**Under (A)** this is the work `T140` explicitly does **not** include (`T140-am`: multi-fixture
tickets *"may need `§6.7` applied at the fixture boundary, which is presentation-only work that
`T140` does NOT include"*). **Under (B)** it is a strict subset — (B) puts `§6.7` at *every* leg
boundary, and every fixture boundary is a leg boundary.

**So it is never wasted, and it is the only item on this list that is also on the critical path of
something else.**

`§6.7`'s clause is already written and is a checklist: *appears only once the stage and active-leg
card have cleared; no score, clock, tape, event line, suspended label or prior offer survives into
it; never resembles an active leg or a live cash-out offer.*

**Today it fires ONCE PER TICKET.** `PresentRound`'s loop is `TicketCardBeat()` → `PlaySweat()` →
`SettlementBeat()` → `AdvanceSweat()`, and `PlaySweat()` runs every leg of the ticket inside one
call with no boundary treatment between them. **The site is the fixture change inside `PlaySweat()`.**

> **AND IT DISCHARGES `T94`.** `T94-am2` (batch 155) ruled that `T94`'s residual defect is **not in
> the ticket column** — the column's look-ahead is correct — but in the scorebug, which holds the old
> fixture across a boundary that has no treatment. **This is that treatment.** `T94`, `T140-am` and
> the spec's `D2` are one seam (`T94-am`), and this is the build that closes it.

### 1.2 `T130`'s gate — a rendered leg row is never empty

Gate item 4 has two halves. *"Every leg on the fixture carries a NEED for the whole telling"* is
arm (A)'s consequence and waits. **The half that does not wait is the assertion underneath it —
a rendered leg row is never empty** — which is `T130`'s own gate and which, in the spec's words,
*"would have caught arm 3 before it shot."*

### 1.3 The correct-score arm's copy — unblocked since batch 158

The spec's §4 lists the `0-0` correct-score ending as *"nothing — the column is blank"*, blocked on
`T130`/`G1`'s nine unauthored kinds and explicitly **not this spec's to author**.

**That block is gone.** `T161` (batch 158) disposed the nine on TV's measurements:
**`CorrectScore` is one of only two kinds that CLEARS IN EVERY SLOT.** Its forms are authored at
`T151` — compact `EXACT 3-1`, NEED `3-1 AT FULL TIME`, fallback `3-1 AT FT`, progress `MET` /
`NOT YET` — and they need no rung and no re-authoring.

**Build them and the blank column on a correct-score leg stops being blank.** Independent of the
fork in both directions: no arm changes what a single-leg correct-score ending shows.

### 1.4 Gate item 5 — executed-case count reported and non-zero (`C29`)

Harness discipline. Applies to any capture under either arm.

---

## 2. DOES **NOT** BUILD YET — and one of these would be wrong under (B)

| item | why it waits |
|---|---|
| **Gate 1 — no clock regression within a ticket** | **This is the one to be careful with. Under (B) THE CLOCK STILL RUNS BACKWARDS BY DESIGN.** Asserting this gate now pins the surface to arm (A) before Allen has chosen it — a gate that fails on a legal arm is not a gate, it is a vote. |
| **Gate 2 — one `THE MATCH ENDS LEVEL` per (ticket, fixture)** | Under (B) there is one per leg, correctly. Same problem as gate 1. |
| **§3.2 — every leg on the fixture live for the whole telling** | Arm (A)'s consequence. Under (B) each leg keeps its own telling. |
| **§3.2 — `NEXT` leaves the legs on this fixture** | Same. Under (B) `NEXT` stays meaningful: there IS a telling you have not seen yet. |
| **`D1`, `D3`** | They test the ruling. `D1` is the frame the phase holds on and it cannot be shot before the arm is known. |

### `D2` is the exception on this list

`D2` — a multi-fixture ticket, two fixtures, the interstitial firing between them, `§6.7`'s list
clean — **is shootable as soon as §1.1 builds, under either arm.** It is the capture that discharges
`T94`'s multi-fixture half, and it does not wait on the fork.

---

## 3. The three bet types the spec does NOT fix, and where they actually sit

Recorded so nothing here is built twice. §4's own finding is that *the drawn ending's copy problems
are not the drawn ending's; they are the vocabulary's, arriving here first.*

- **1X2 draw-backer** → `T126`, and `T126-cl` (batch 138) has already settled it.
- **team-backer** → `T128-cl`, `T108`'s trigger unchanged, queued in its own thread.
- **count legs settling level** → §3, which **is** the fork.
- **correct score** → **now buildable, §1.3 above.**

---

## 4. What this note does not do

- **It does not narrow the spec.** Everything approved stays approved; this splits it by dependency,
  nothing more.
- **It does not rule the fork**, and §1.1 must not be read as leaning toward (B) because it shares a
  device with it. It is required under (A) too, by `T140-am`'s own scope statement.
- **It orders no capture.** `D2` is named as shootable-after-§1.1, not requested here.
