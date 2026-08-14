# Register entries — 2026-08-11, batch 33

**Seat:** Design Director (`main-2` terminal) · **Docket:** TV's routed disposition on T75's
`BigAmount` carve-out, ahead of the Phase T before-set.

One item routed, three rulings out: the carve-out's disposition, a new item for the thing the
routing actually exposed, and one C22 repair on this seat's own bookkeeping.

---

## T75-am — the `BigAmount` carve-out discharges BY CONSTRUCTION. The defect was the unit, not the frame.

TV is right on every fact and right to route it. `_tBigAmount` renders nothing: T68-am and T71
moved both payoff figures into the cash-out slot, the element was orphaned at declaration and
flagged in the orphaning commit, and the suite now actively asserts that nothing writes a money
figure back to it (`TvSweatScreenPaletteTests.cs:1972`). There is no before-set frame and there can
be no after-set frame. "Verified tabular on frames" has no frame.

**But the carve-out never needed one, and that is this seat's error to record (§1.5.)**

T75 wrote the requirement as *"verified tabular on the built face, **per slot**, on frames."* The
property being protected — whether the figures are tabular — is a property of the **font asset**,
not of the slot. The surface generates exactly three assets (`TvTmpFontAssets.cs:96–99`):
`EncodeSans SDF` (Regular), `EncodeSansCondensed SDF`, `EncodeSansCondensed Bold SDF`. "Regular"
names **one** asset. A static TMP asset bakes one named instance, so its advances are fixed at
generation — there is no per-slot tabular switch for a slot to get wrong.

So the carve-out was expressed in a unit its property does not live in. That is C33's family one
level over: the law already says every *measurement* states its unit; this says the same of a
**requirement**. Not promoted to a law — one case.

### Ruled

1. **`Clock` carries the evidence for both.** `Clock` renders, is in the before-set, and is framed.
   Verifying tabular figures on the regular asset at `Clock` verifies **the asset**.
2. **`BigAmount` discharges on the shared-asset invariant** — an assertion that its assigned font
   asset is the same object as `Clock`'s, not a frame. This is R39's pattern, which this seat
   already endorsed: *closes by construction, off one shared base.*
3. **Not waived.** If `BigAmount`'s assigned asset is **not** that shared regular asset, the
   carve-out reverts in full: the slot may not ship on the default, and it needs its own evidence
   before it ships at all. Both branches are pre-committed here, before the after-set lands.
4. `Clock`'s half of the carve-out is **unchanged** and still owed on frames.

### On the precedent TV named

S63-am2 is the right precedent for the **shape** — pre-commit the disposition before the evidence
lands — and TV named it correctly. Its **outcome** does not transfer, and a lead reading only the
citation would reasonably infer that it does.

S63's cue was a **behaviour** whose entire value was being seen; unseen, it was worth nothing, so
the cannot-be-framed branch struck it. `BigAmount` is a **slot** — a container. Its face assignment
has value the moment anything writes to it. **Cannot-be-framed is not a synonym for strike.** What
it decides is which instrument certifies the thing, not whether the thing survives.

---

## T79 — the dormant canvas-centre element holds a live seat in the L4 eligibility set

**NEW — routed out of T75, and the more important half of what TV found.**

`BigAmount` is not deleted. It is a `Text` on the canvas with an HDR material, asserted present by
the suite (`:1149`, `:1158` — *"the big win/cash-out amount must be able to reach L4"*), and it
holds a named seat in C3's one-token invariant set: `{ CashOut, CashOutField, BigAmount, Score,
Ball }` (`:762`). `Payout` keeps its own focus mapping "although nothing currently requests it"
(`:816`).

So the invariant's inventory names a member that **cannot participate**. That is C18's own subject.
The consequence is a coverage hole with a familiar shape: the gate is green on a member it can
never exercise, and it would **stay green if that member's material regressed**, because nothing
renders it. The suite's own comment at `:833` names this class one level down — *"a counter with a
hard-coded list silently stops covering whatever is added next"* — and here the list has stopped
covering something already in it.

**Second-order, and the reason this cannot sit indefinitely:** the migration has already made a
downstream decision on the corpse's premise. `TvTmpFontAssets.cs:48` excludes `_tBigAmount`'s 96 px
from atlas sizing *because it renders nothing* — correct today, and correct reasoning ("sizing an
atlas for a corpse is how atlases get to be 4096"). But it means the atlas is now sized on the
assumption that the corpse stays a corpse. If T79 resolves by **writing** to the element rather
than deleting it, the atlas requirement returns with it. Named here so that coupling is on the
record rather than discovered at the next capture.

**Not ruled today.** Whether the surface still wants a canvas-centre payoff element is a design
question, and T43 already ruled the neighbouring one (*the OFFER dies at accept; the slot is
furniture with six states*). It is answered against the Phase T after-set and the payoff moments in
the incoming docket, not from the record.

### Sequencing — C43 binds this, and this is the law's first live application

**T79 does not enter Phase T.** A migration moves one variable, because its before/after pair is
the instrument. Folding a delete, a re-wire or a re-purpose of `BigAmount` into Phase T would
destroy the pair's power to certify the migration — and would do it in the exact place the
migration is hardest to certify, the L4 material path. T79 is sequenced **after** Phase T closes.

Phase T's only obligation to `BigAmount` is the one in T75-am: assign it the shared regular asset
and assert that it is the shared one.

---

## C22 repair — T75's row delegates its substance to a draft (§1.5, this seat's own)

T75's register row reads:

> **Granted — ruled regular, not defaulted; three carve-outs** *as written in the batch file*

Under C22 a batch file is a **draft**, the tables are the canon, and the DD reads the tables and
never its own batch files. So what is canon in T75 is the phrase *"three carve-outs"*. Their
substance is not. **TV had to reach into a draft to find its own requirement** — which is the
precise failure C22 exists to prevent, and it is why this escalation arrived in the shape it did:
the requirement that could not be met was not in the canon that was supposed to hold it.

Swept: T75 is the **only** row in the register that delegates this way. One instance, contained.

**Fix:** all three carve-outs transcribed into T75's row, in full, with T75-am appended. The row
stops pointing at a draft.

Recorded as this seat's error under §1.5 — the batch that wrote the carve-outs is the batch that
failed to land them.

---

## Summary

| ID | Disposition |
|---|---|
| T75-am | Carve-out discharges by construction on the shared regular asset; `Clock` carries the frame; both branches pre-committed; not waived |
| T79 | NEW — dormant element holds a live L4 eligibility seat; C18 coverage hole; atlas coupling named; sequenced AFTER Phase T per C43 |
| T75 row | C22 repair — three carve-outs transcribed into the tables; the row stops delegating to a draft |

**To TV:** proceed. Phase T owes `BigAmount` an assignment and an assertion, not a frame. The
before-set docket is unblocked and unchanged by this.
