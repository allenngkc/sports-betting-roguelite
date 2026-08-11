# Register entries — 2026-08-10, batch 30

**Seat:** Design Director (`main-2` terminal) · **Subject:** room's pinned determinism pair, and what
it does and does not discharge on the SureThing baseline.

**Evidence:** `artifacts/room-visual-pass/gate-runs/2026-08-10-pinned-determinism-pair.md` at `08e8f44`
(room `e3e7c61`, rig at `4dfb053`). Read in full at this seat. Recipe cross-checked at
`docs/design/dd-import/room-to-surething-s2am2-baseline-rig-recipe.md`.

Two rulings, because there are two subjects: room's rig, and SureThing's baseline. One ID each
(C22.1 — these are not one finding under two names).

---

## R43 — The pinned rig is deterministic where it is measured. **GRANTED.**

### The assertion clears C34 in full, and one detail exceeds what was asked

`ROOMREF01` on `RoomViewCapture.RoomSeed`, asserted in `PinRoomSlate()` by reading the seed **back**
from `director.Run.Rng.RunSeed` and comparing; **throws `InvalidOperationException`** on mismatch —
it refuses to shoot rather than warning and continuing. Play frame 1, poses on frame 8, so the assert
precedes every `Shoot()`. Both runs carry it once each. Three `wrote` lines per log, so neither set is
a partial shoot.

**The detail worth promoting: the log line is emitted only after the comparison passes.** Its presence
is therefore evidence that the assert *succeeded*, not evidence that the assert *exists*. That is
**C36's shape applied unprompted** — the half of the instrument that certifies is not the half being
checked. Named as the pattern to copy; it is the difference between a pin and a comment about a pin,
and this lane found it without being told.

### The verdict, and why the null is valid

**R9-A `(1582, 686, 1652, 710)` moves 0.00 across the pair** — max delta 0, the residual variation
does not reach the measured region at all. 38.20 / 38.20, identical to the pre-crash pinned pair.

**Room undersold its own strongest evidence.** C37 asks whether success would have been resolvable
before a null is allowed to mean anything. It was, and the proof is in room's own table: the
**unpinned** pair read **38.30 / 38.21**. The instrument detected movement when movement existed, at
the same n, on the same box, through the same code path. **The unpinned arm is the positive control
that makes the pinned arm's null valid.** That is the most load-bearing thing in the record and it is
not stated as such. Under C37 this null is not merely unobjectionable — it is *certified*.

### Room's n=2 caveat is correct, and it withholds a claim it does not touch

Room states: *"whether the residue stays ≤5/255 over more than two runs (n=2 — this is an observation,
not a characterized bound)."* **That is right, and it should stay exactly as written.** A residue
*ceiling over future runs* is not established by two runs.

But the measured null does not rest on the residue ceiling. R9-A moved 0.00 with a positive control
behind it; that verdict is independent of where the residue tops out elsewhere in the frame. **Do not
let a correct scruple about claim X withhold claim Y.** Both statements ship, and neither is softened:
the null is **ruled**, the ceiling remains an **observation**.

### Diagnosis accepted

*`StartNewRun` pins the deal, not the clock.* Slate reproduces — same teams, records, prices. What
varies is animation phase, elapsed time on a live panel, and sub-quantization rounding. That is a
different *kind* of residue from the unpinned case, where the content itself changed, and the
distinction is the whole point. The TV ticker inside the standing pose's localized zone
(`ROUND 1 OF 8 · BOARD OPE…`) is **correct behaviour caught by a good instrument**, not a defect.

### Two practices endorsed

- **Durable file over terminal.** The pre-crash pair's report died with the seat. This lane named that
  defect once already in its own words — *"the claim was the artifact and no run was reproducible"* —
  and correctly recognised a lost *report* as the same failure as a lost *number*. Standing: a
  determinism or baseline result is written to a file in the same step that produces it.
- **Bytes withheld, MD5s recorded.** Correct under the open evidence-in-git question (2 × 9.8 MB), and
  the hashes still let any future re-run be checked for byte-identity against this one. No objection.

---

## S2-am2-am3 — Batch 29's ~0.037 bound does **NOT** discharge on this record, and it was never room's to discharge

### What the pin actually did to the bound

Batch 29 bounded the `fa93238` baseline because *deal-to-deal variation is an uncharacterised term
inside these numbers*, and named the discharge condition as *"until a pinned-seed assertion exists."*

The pin does not **characterise** deal-to-deal variation. **It eliminates it.** The term named in
batch 29 is not now measured — it is absent from the instrument. Where that applies it applies
completely, and it is a stronger discharge than the bound asked for.

Where it does not apply, on this record, is the thing the bound was written on. Two independent
reasons, either sufficient:

**1. The S2 boxes were never measured across the pair.** Room says so plainly in §4: it checked R9-A,
and reports only that the *containing pose* has max delta 1/255. That is a strong indication and it is
not a measurement. It cannot be promoted into one here, because by the recipe's own §, `ramp ÷ stroke`
is **not monotonic in blur** (`0.710 → 0.774 → 0.686 → 0.583 → 0.653`) and **compresses above σ≈1**.
A pixel-level delta therefore carries no derivable bound into the ratio. Asserting that 1/255 cannot
move ramp÷stroke by 0.037 would be predicting a value off a frame this seat has not measured — **C41,
applied on the batch that wrote it.** Not doing that.

**2. The decisive one: `fa93238` was shot before the pin existed.** A determinism proof on today's
pinned rig does not retroactively pin a baseline shot unpinned. The bound is on *those numbers*, and
no property of the current rig reaches backwards into them.

**The bound stands unchanged and passes to SureThing's re-shoot.** It was never room's to discharge —
room owns the rig recipe, SureThing owns the baseline that uses it.

### Pre-committed disposition for the pinned re-shoot, stated before its frames land

Both branches decided now, so the result cannot shape the ruling:

- **If the re-shoot reproduces `fa93238`'s authored values under the asserted pin** — the baseline of
  record **stands**, now pinned, and the bound **discharges**.
- **If it does not** — `fa93238` is **retired** as the baseline of record and the pinned re-shoot
  **becomes** the baseline. **No reconciliation of the old numbers is attempted.** An unpinned baseline
  has no claim to being the true value, and arithmetic that "explains" the gap would be fitting a story
  to a frame nobody can ever re-shoot.

**Either branch discharges the bound.** What the frames decide is *which numbers are the baseline* —
not whether the bound lifts. 

**No expected value is stated, and none will be.** Batch 27 predicted a band off a frame that still
contained the element under test; C41 is the law that came out of it. The direction of travel is the
whole of the pre-commitment.

### Not withheld on room

R43 is granted **now**, not held for SureThing's capture window. Holding a complete deliverable hostage
to another lane's shoot is what *"one window, not three"* was written against, and room's half is
complete on its own terms.

---

## Carried

- **SureThing's pinned re-shoot** — lands next; ruled against the pre-commitment above, at this seat.
- **Residue ceiling (≤5/255)** stays an observation at n=2. It blocks nothing. It is characterised only
  if some future measurement box is found to intersect the live-panel zone — R9-A does not, and the
  S2 boxes sit on a pose whose max delta is 1/255. No work is owed for this; it is recorded so a later
  seat does not mistake the observation for a bound.
- **Evidence-in-git** remains open (Allen's storage ruling). Room handled it correctly in the meantime.
