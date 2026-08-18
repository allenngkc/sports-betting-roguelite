# RULING — `T108`'s trigger: the revealed count is right, and the spec named a field that cannot carry the fix

**Written:** Design Director seat, 2026-08-17 · **Answers:** tv-theater unit 1's routed question ·
**Amends:** `spec-resolved-leg-column-2026-08-16.md` §2 and §6 · **Surface:** TV — match theater

**CONFIRMED. Build proceeds on the revealed-count reading.** It is not a deviation from the spec —
it is the spec's clause 3 applied correctly to a field the spec should not have named.

---

## 1. Why it is confirmed, and why it was never a choice

Clause 3 reads: **the trigger is the REVEALED state, never the resolved one.** `ResolveLeg` *is* the
resolved one. Keying off it would have produced a fix that fixes nothing — the defect is the eighteen
minutes **before** full time, so a trigger that fires at full time arrives after the only window the
spec exists to correct.

The lane's claim is **verified at source, not taken on report:** `RevealedView.ResolveLeg` has
**exactly one call site** — `TvSweatScreen.cs:1865`, inside `FinalSlam`. So `RevealedLegState` reaches
`Won`/`Lost` only on the final-whistle path, and on a multi-leg ticket a leg resolved through
`ResolveBeat` never leaves `Live` in that mirror at all.

## 2. §1.5 — THE SEAT'S OWN ERROR, and it is the load-bearing half of this ruling

Spec §2 asserted:

> `RevealedLegState { Pending, Live, Won, Lost, Voided }` and `RevealedTicketState { … }` both exist.
> **The surface has the information and is not reading it.**

**That is wrong, and it is wrong in the way that mattered.** The enum exists; it does not carry the
state at the moment the defect occurs. The surface did **not** have the information in the form the
spec named. I checked that a field existed and did not check **when it transitions** — and a state
field's transition points are the whole of its meaning for a fix keyed to a moment.

**The reusable half:** *naming a field is not reading it.* This is the same class as batch 95 and
batch 100's own two §1.5 entries — a property asserted from a source read that a measurement then
corrects. Third time this seat has taken that correction in a fortnight, and it is the first time the
correction came from a lane rather than from a frame. **The lane was right to ask rather than build
to the letter of a spec it could see was unbuildable.** Recorded as the standard, not as an exception.

## 3. What I RATIFY in the build, so it is not tidied later

Read at source. Each of these is a decision a later reader would plausibly "clean up", and each is
correct as built:

- **The separate enum.** `RevealedLegOutcome` is a **new** type, not new semantics bolted onto
  `RevealedLegState`. **It must stay separate.** Overloading the existing enum would silently change
  behaviour for every other reader of that field, and the two answer genuinely different questions —
  *decided on what the player has been shown* against *graded at the whistle*.
- **`LIMIT 0` stays and is NOT the `NEED 0` defect.** The distinction as the build states it is
  exactly right: `NEED 0` named a requirement that had **stopped existing**; `LIMIT 0` names an
  allowance that is **still real** — one more of the stat kills the leg, and none has happened. Do
  not "fix" it to Won or Lost.
- **`TicketCannotLose`'s signature.** Taking every leg's outcome, so a single leg's state cannot
  reach the ticket word, is clause 2's trap closed **structurally rather than by discipline.** That
  is stronger than the spec asked for and it is the right instrument.
- **`BuildTicketLegOutcomes`' three-way composition** — resolved rows read their grade, the live row
  takes the revealed-derived outcome, everything else is `Undecided`. This closes a gap the spec did
  not anticipate: an OVER that **fails** and an UNDER that **holds** are undecidable on revealed
  values, and they arrive through the resolved branch instead, where the row blanks its NEED and
  progress entirely. **No stale requirement survives the whistle.** Checked on the render path.
- **The dead ticket deliberately not built.** A ticket with a Lost leg keeps today's `RISK`, awaiting
  the capture. Correct, and the comment saying so in terms is what keeps it from being read as an
  oversight and "completed" by the next hand.

## 4. A THIRD STATE G1 DID NOT RULE — and I rule it here rather than claim G1 covers it

On a leg won by the revealed count but whose whistle has not played, the live row prints
`OVER 8.5 CORNERS` above `11 CORNERS • WON`. G1's line — *NEED is the requirement while live, compact
is identity elsewhere* — is a **two-state** rule, and this is a third state it never contemplated:
**decided, but not yet resolved.**

**RULED: the statement line does not change. `OVER 8.5 CORNERS` stays.**

It reads as the market that was bet — an identity — and the line directly beneath it answers any
reading of it as an outstanding ask. Swapping to the compact form on the win would change **two
things on one beat**, and the compact slot is deliberately blanked on live rows precisely so the
statement is not printed twice.

**Named as a new ruling and not as G1's**, deliberately: leaning on *"the governing law is already
ours"* is what produced §2's error, and the habit is worth breaking where the law genuinely stops
short.

## 5. THE ONE CORRECTION — the gate certifies the model but not the distinction

`SweatActiveLegModelTests` proves the model exhaustively off pure inputs, and the every-frame poll in
`TicketFooterWord_NeverDisagreesWithAnyRow_AndNoLiveRowEverPrintsNeedZero` is **the right
instrument** — a moment where two surfaces disagree cannot be caught by a sampled pin, and it reads
the player-visible text rather than re-deriving it.

**But it can pass without ever exercising clause 2.** The ticket comes from an unpinned
`DemoTicketPolicy` draw, and where the run never reaches a decided leg the STAKE half of assertion 2
**logs and does not fire** (`sawDecidedLeg`). Logging rather than faking is right and I am not asking
for that to change. What I am ruling is that **a gate whose central assertion is conditional on the
draw certifies nothing about that assertion** — and the composition it guards,
`BuildTicketLegOutcomes`' three-way split, is the one part of this fix **no signature protects**.

**RULED: the wiring gate must exercise these two states BY CONSTRUCTION, not by luck —**

1. **Leg 1 resolved `Won`, leg 2 live and undecided → the footer reads `RISK`.** This is the trap: a
   won leg must not reach the ticket word.
2. **Leg 1 resolved `Won`, leg 2 live and won ON THE REVEALED COUNT, before leg 2's whistle → the
   footer reads `STAKE`.** This is the fix actually working on a multi-leg ticket, and it is the
   state the whole spec was written for.

`sawDecidedLeg` and `sawNextChip` become **assertions at the end of the run**, on a fixture built to
guarantee them. The every-frame poll stays exactly as it is.

*(Note for the lane, not a ruling: legs sweat sequentially — the engine forbids two legs on one
matchup — so "one won, one live" means leg 1 sitting in `_resolvedThrough` while leg 2 is the live
row. The construction is a lane call; the states that must be certified are not.)*

## 6. Still owed, unchanged

- **`C46`.** `{n} CORNERS • WON` / `• LOST` are shorter than the `NEED {k}` forms they replace, so
  this **relieves** the box — but relief is not a measurement. These strings join `T111`'s single
  consolidated sweep, under its two bindings (`S84`'s enumerated-pool sizing; batch 95's
  measurement-not-arithmetic). **They do not get a private sweep.**
- **Spec §8's three evidence items**, unchanged: a won leg with time remaining (the before-state
  already exists in the corners set), the multi-leg one-won-one-live ticket, and a losing ticket for
  the dead-ticket copy.
- **Frame claims.** Whether `WON` and `STAKE` read correctly at review distance is a C11 claim and
  neither gate states anything about it. Design-verified waits on frames.
