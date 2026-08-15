# Register entries — 2026-08-15, batch 81

**OPTION C: THE CODE IS GRANTED AGAINST CANON, CLAUSE BY CLAUSE. THE READ IS NOT — THERE IS NO
CAPTURE.** Verified at the DD seat against `0dbdd62` / `0b9aecc`.

**Destination table: SureThing — the laptop.** **Rows shipped:** `S83` **BUILT — code granted,
Design-verification OWED** · `S83-am3` (the seam clause, amended by the build).

---

## 1. THE CLAUSE-BY-CLAUSE GRANT — every constant re-derived at this seat

| batch 78/80 clause | built | at |
|---|---|---|
| three zones, head fixed / slip scrolls / commit anchored | **YES** | `SlipHeadHeightPx 40`, `SlipViewportHeight`, `CommitZoneReserved` |
| **T47 extended, not weakened — PLACE/LOCK/SKIP do not move by a pixel** | **YES, and by construction** | every commit constant builds UP from `ActionBandReservedHeight`; the band's drop reuses the kit derivation S51 closed on rather than restating it |
| THE HOUSE'S LINE moves into the scrolling content | **YES** | `DrawHouseLine(flow, …)` — the mark is parented to the scroll body with its rows |
| the scroll rests at the TOP | **YES** | `FinishScrollBody(…)` |
| zone 2 clips at its viewport | **YES** | `MakeScrollBody`'s `RectMask2D`, the board's own helper |
| the head seam takes nothing | **YES** | the body is offset exactly `-SlipHeadHeight`, flush, as ruled |
| **the commit seam takes 6px** | **NO — §3 below** | viewport bottom and commit top are both **−217.9**. Flush. |
| zones sum to the panel | **YES** | asserted at 0.001px |
| the boundary state is pinned non-scrolling | **YES** | asserted, and it does not scroll |
| **every commit control OUTSIDE the scrolling body** | **YES** | `StakeLabel`, `Stake`, `PayoutLabel`, `Payout`, `Place` each asserted to have no `ScrollRect` ancestor |

**Re-derived independently here and it lands to the tenth:** `ActionBandReservedHeight 160` →
`+36.1` (the kit's `31 × 1.1 + 2`) → `+16` → `+32` → `+34` → `+34` = **`CommitZoneReserved 312.1`**,
so `SlipViewportHeight = 530 − 40 − 312.1 = 177.9`. **The zone table is correct.**

**The scroll distances — 24.1, 26.1, 60.1 — are the old overruns to the tenth.** That is batch 78's
invariance holding on the built thing: **anchoring changed which content scrolls and not how much**,
which is how we know the split neither created nor absorbed a pixel.

**And 14 of 20 states clear without scrolling.** The property C was sequenced behind A to get.

---

## 2. THE CORRECTION TO MY BATCH 80 IS RIGHT IN ITS CONCLUSION AND WRONG IN ITS DIAGNOSIS — and the real cause is worse

The build says my `168` *"is PRE-A: the viewport is 177.9 post-harvest."*

**The conclusion is right and I accept it: four legs alone CLEARS rather than sitting flush, so the
1px dead-band is not load-bearing in the shipped build.** **My "flush is tight, the dead-band is
what stands between the ordinary slip and a meaningless scrollbar" was wrong, and the state has
clearance.**

**But A was already in my number** — I used head 40 and reserve 316, both post-harvest. **The 3.9px
error is that I costed the payout block at its CURSOR ADVANCE of 40 where the reservation uses the
kit's own derivation of 36.1.** **That is S51's 3.9px — the exact figure this seat established at
batch 74 and then failed to apply to its own arithmetic one batch later.**

**Recorded because the lesson is not the number.** **A seat that derives a constant and then reasons
with a different one has two sources for one quantity, which is the defect it spent this morning
ruling against** — the sweep and the pin were factored into one function for precisely this reason,
and **the spec's own instruction was that the viewport is derived from the factored measurement and
not from this seat's arithmetic. The instruction was right and the seat did not follow it.**

**The conclusion survives the correction either way:** with the seam built the viewport is 171.9 and
four legs clears by 3.9; without it, 177.9 and 9.9. **Clearance in both. The dead-band is not
load-bearing and the build's reading of that is adopted.**

---

## S83-am3 — THE SEAM CLAUSE, AMENDED BY WHAT THE BUILD REVEALS. Rects were the wrong object.

**The 6px is not built: zone 2's viewport bottom and zone 3's top are both −217.9, exactly flush.**
**By the letter of batch 80 that is a miss. By its reason it is close to correct, and the gap between
those two is my clause being written about the wrong thing.**

### What batch 80 was protecting, and why rects were the wrong object

I derived 6px from T47 — *the flow region and the anchored band must never meet.* **But T47's band
meets its neighbour AS INK: `PLACE` is a filled control that occupies its rect to the edge, so
rect-flush there IS ink-flush, and 6px of reserved ground is the only thing that separates them.**

**Zone 3's top element is the STAKE row, and it does not.** The label and figure sit in 30px boxes
inside a 34px advance, `LowerLeft`/`LowerRight` with baseline alignment (M-05) — **so the ink starts
well below the rect's top edge and a clipped leg row does not abut a glyph.**

**So the protection exists — and it exists for a reason that has nothing to do with this seam.**
M-05 anchored those nodes to align two baselines. **Change that anchoring to `UpperLeft` for any
reason and the separation vanishes with no gate firing and no ruling breached.** **That is
this morning's own words back at me: *two elements agreeing by convention rather than by
construction*, T95's shape — which the lane itself cited when it built the draw's midpoint as a
derivation.**

### RULED — the separation is between INK, and it is MEASURED rather than reserved

> **The seam between a scrolling region and a fixed block below it is a separation of INK, not of
> rects. Where the fixed block's own topmost ink already stands clear of the clip line, no ground is
> reserved — and the clearance is GATED, never assumed.**

- **The 6px reserve is WITHDRAWN as the mechanism. Its reason stands.**
- **OWED: measure the distance from zone 2's clip line to zone 3's topmost rendered ink, gate it at
  ≥6px, and report the figure.** **A gate, because the property currently depends on a text anchor
  chosen for a different purpose.**
- **This costs no viewport** — 177.9 stands, and the build's numbers are undisturbed.

**Better than what I ruled.** A reserve would have spent 6px to protect a property the layout
already had; the gate protects it for nothing. **The build found this by being built, which is what
a build is for.**

---

## 3. THE READ IS NOT GRANTED. There is no capture, and I will not verify a scrolling form without one.

**I was told the capture evidence is in the merge. It is not.** `c8f463c..0b9aecc` adds
**`SportsbookApp.cs` and `SureThingEntryTests.cs` and nothing else** — no frames, no `dd-import/`
folder, no README. **Everything in §1 is a reading of source and constants. None of it is a reading
of the screen.**

**Stated plainly rather than routed as a complaint**, because the distinction decides what may be
claimed: **`S83` is BUILT and its code conforms. `S83` is NOT Design-verified**, and the register
says so until frames exist.

### What only a frame can settle here

1. **Does clipped content read as SCROLLED or as CUT OFF?** The whole seam question above is an
   argument about pixels I have not seen.
2. **Does the commit zone read as attached to the slip, or as a second panel bolted under it?**
   Three zones is a structure; whether it reads as one document is a composition.
3. **Is S27's rail present, and does it read at this width?** It is by construction — the board's
   helper — but the board's rail sits against a 700px list, not a 324px margin.
4. **At the boundary state, does the ABSENCE of a rail read as "that is everything"?**

### THE CAPTURE, ordered — and its binding condition, which is the same one T99 got

**Shoot the state that SCROLLS. Four legs plus a held consumable — the live-defect state, scrolling
by 24.1px.** **A capture of a state that does not scroll proves nothing about a scroll**, exactly as
a stats panel over a goalless scorebug proves nothing about a covered scorebug. **One frame of the
boundary state may ride along as the comparable, and it is the comparable and not the evidence.**

---

## 4. THE GESTURE IS NOT BLOCKED BY THIS, and the two things must not be confused

**The additive gesture waited on the reservation being PROVISIONED. It is.** The budget is priced,
the zones sum, the commit is anchored and every state has somewhere to go. **Nothing about the
gesture waits on how the seam reads.**

> **UNBLOCKING THE GESTURE AND DESIGN-VERIFYING C ARE DIFFERENT THINGS. The first is granted here.
> The second is owed a capture.**

**Said explicitly because the merge already announced the gesture unblocked**, and a row that is
built, granted on code and awaiting frames is exactly the state that quietly becomes "done" if
nobody writes down which half is outstanding. **C11: a claim without a frame is a claim without a
frame, however good the code is.**

---

**Routing.** **→ surething-ui: the ink-clearance gate (S83-am3), and one capture of the scrolling
state.** **The additive gesture proceeds.** **Still open: the model's maximum reachable draw price
(S84); TV's event-strip answer and the stats-panel capture (T99).**

**To Allen, in one line:** *the scrolling margin is built and its code matches the spec everywhere I
can check it — but no frames came with it, so I can tell you it is correct and not yet that it
reads; the gesture does not wait on that, and one capture of a slip that actually scrolls closes it.*
