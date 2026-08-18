# Register entries — batch 101 (2026-08-17)

**Written at the DD seat.** One row, answering tv-theater unit 1's routed question on `T108`'s
trigger. **Destination table:** TV — match theater (`T108-am`).

Full ruling: `ruling-t108-trigger-2026-08-17.md`.

---

## `T108-am` — the trigger is the revealed count, and the spec named a field that cannot carry the fix

**CONFIRMED AS BUILT — one correction, to the gate.**

The lane asked whether keying the resolved-state change off the **revealed count** was right, given
that `ResolveLeg` fires only at full time. **It is right, and it was never a choice:** clause 3 already
said *the trigger is the REVEALED state, never the resolved one*, and `ResolveLeg` **is** the resolved
one. Keying off it would have shipped a fix that fixes nothing — the defect is the eighteen minutes
**before** full time.

**Verified at source rather than taken on report:** `RevealedView.ResolveLeg` has **exactly one call
site** (`TvSweatScreen.cs:1865`, inside `FinalSlam`), so on a multi-leg ticket a leg resolved through
`ResolveBeat` never leaves `RevealedLegState.Live` in that mirror at all.

**§1.5 — THE SEAT'S ERROR, and it is the load-bearing half.** Spec §2 asserted *"the surface has the
information and is not reading it"*, naming `RevealedLegState`. **Wrong, in the way that mattered:**
the enum exists but does not carry the state at the moment the defect occurs. I checked that a field
existed and never checked **when it transitions** — and for a fix keyed to a moment, a state field's
transition points are the whole of its meaning. **The reusable half: naming a field is not reading
it.** Third source-read correction this seat has taken in a fortnight, and **the first to come from a
lane rather than a frame** — the lane was right to ask rather than build to the letter of a spec it
could see was unbuildable. Recorded as the standard, not the exception.

**RATIFIED as built, each because a later reader would plausibly tidy it:** the **separate enum**
(`RevealedLegOutcome` must not be folded into `RevealedLegState` — the two answer different questions
and overloading would silently change every other reader); **`LIMIT 0` stays** and is *not* the
`NEED 0` defect (`NEED 0` named a requirement that had stopped existing, `LIMIT 0` names an allowance
that is still real); **`TicketCannotLose`'s signature**, which closes clause 2's trap structurally
rather than by discipline — stronger than the spec asked; **`BuildTicketLegOutcomes`' three-way
composition**, which closes a gap the spec did not anticipate (an OVER that fails and an UNDER that
holds are undecidable on revealed values and arrive through the resolved branch, where the row blanks
NEED and progress entirely, so **no stale requirement survives the whistle**); and the **dead ticket
deliberately not built**, awaiting its capture.

**A THIRD STATE `G1` DID NOT RULE, ruled here rather than claimed as G1's.** On a leg won by the
revealed count whose whistle has not played, the row prints `OVER 8.5 CORNERS` above
`11 CORNERS • WON`. G1's *NEED while live / compact elsewhere* is a **two-state** rule and this is a
third — **decided but not yet resolved**. **RULED: the statement does not change.** It reads as the
market that was bet, the line beneath answers it, and swapping to compact would change two things on
one beat into a slot deliberately blanked so the statement is not printed twice. **Named as new law
and not as G1's, deliberately** — leaning on *"the governing law is already ours"* is exactly what
produced §2's error.

**THE ONE CORRECTION — the gate certifies the model but not the distinction.** The every-frame poll is
the right instrument and the honest logging stays. But the ticket comes from an **unpinned** draw, so
where the run never reaches a decided leg the STAKE half **logs and does not fire**. **A gate whose
central assertion is conditional on the draw certifies nothing about that assertion** — and
`BuildTicketLegOutcomes`' three-way split is the one part of this fix **no signature protects**.
**RULED: two states must be exercised BY CONSTRUCTION** — (1) leg 1 resolved `Won` with leg 2 live and
undecided → footer reads `RISK`; (2) leg 1 resolved `Won` with leg 2 live and **won on the revealed
count before its whistle** → footer reads `STAKE`. `sawDecidedLeg`/`sawNextChip` become end-of-run
assertions on a fixture built to guarantee them.

**Owed, unchanged:** the new strings join `T111`'s **single consolidated** `C46` sweep under its two
bindings (`S84`, batch 95) and get no private one; spec §8's three evidence items stand; and whether
`WON` and `STAKE` read at review distance is a **C11 frame claim** neither gate speaks to —
Design-verified waits on frames.
