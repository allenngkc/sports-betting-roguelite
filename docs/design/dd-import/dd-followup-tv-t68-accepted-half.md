# TV — T68's "accepted" half is unbuilt, and a static `goldInk` would be a new defect

**From:** TV sweat lead · **Filed:** 2026-08-08 · **Rides:** next DD push
**Status:** T68's `actionable` half is CLOSED on frames (7.95:1). This is the other half of that
ruling, deliberately not built. **Not blocking.**

---

## 1. Why it was not built

T68 ruled: *"`HOLD E` and the amount take `goldInk` on `actionable` and `accepted`."* The actionable
half is shipped and verified. The accepted half has no site to apply it to, and applying it to the
site that exists would create a fresh invisibility.

**The cash-out slot is HIDDEN during accept.** `CashOutFloodBeat` calls `HideCashOutSlot()` on its
first line — *"T43: nothing of the offer outlives the accept"* — so `_tCashOut` and `_tCashOutStatus`
are disabled and there is no lit field for anything to be punched out of.

`CASHED OUT $x` renders instead on **`_tBigAmount`**, over **`_goldFlood`** — a full-canvas image
whose alpha is a **sine pulse: 0 → 0.55 → 0** across `cashOutFloodDuration`. The ground under that
text is not a field. It is a ground that rises and falls.

## 2. The numbers, and they are COMPUTED, not measured (C25)

**No capture of the accepted state exists.** These are authored values converted to linear relative
luminance — the space a contrast ratio requires (C33-am3) — not a rendered measurement. The composite
figures additionally assume a linear blend; if the pipeline composites elsewhere the midpoint moves,
though the endpoints do not.

| element | authored | linear rel. luminance |
|---|---|---|
| `gold` as a canvas vertex colour (Color32-clamped to 255,209,46) | — | **0.6705** |
| `goldInk` `#0A0C10` | — | **0.0037** |
| substrate / graded floor (≈0.085 display) | — | **≈0.0078** |

**The two inks are exactly complementary across the beat:**

| flood alpha | ground | `gold` text | `goldInk` text |
|---|---|---|---|
| **0** (start and end) | substrate ≈0.008 | **12.5 : 1** — legible | **1.08 : 1** — invisible |
| **0.55** (peak) | ≈0.372 | **1.71 : 1** — poor | **7.9 : 1** — legible |

**Gold works at the ends and fails at the peak. `goldInk` works at the peak and fails at the ends.**
Neither static ink is correct for a ground that moves. Shipping `goldInk` here would trade a poor
1.71:1 at one instant for an invisible 1.08:1 across most of the beat.

## 3. And the boost does not help — it is T68's own mechanism again

`ApplyBoost`'s `Payout` case drives **both** `_bigAmountHdrMat` and `_goldFloodHdrMat`. So when
`RequestL4(HdrFocus.Payout)` fires on accept, **the text and the ground behind it are boosted
together**, and their ratio is roughly preserved.

That is structurally the same thing T68 was: an element and the thing it sits on moving in lockstep,
so no amount of brightness separates them. It is worth ruling on the same understanding.

## 4. What this seat would need told

1. **Should the ink track the flood** — interpolate `gold`→`goldInk` against the flood's own alpha,
   so the text is dark exactly while the ground is bright? Mechanically small; it is a lerp on a
   value already in hand. It does make the money word change colour mid-beat, which is a motion the
   surface does not otherwise have.
2. **Or should the accepted treatment move into the slot**, as §6.1 actually specifies — *"accepted:
   brief L4 punch, then `CASHED OUT $x` at L3"*? That reads as a slot state, and a slot gives it a
   stable field to invert against, exactly like `actionable`. The build diverged from the spec here
   and the divergence is what created the problem.
3. **Or does the flood simply not belong under the text** — the text taking its own stable ground?

**The seat's read, offered not taken:** (2). It is what §6.1 already says, it reuses the inversion
that is now built and verified, and it removes a moving ground rather than compensating for one.
But this is a treatment decision and the divergence from §6.1 is old, so it is filed rather than
assumed.

## 5. The same shape one beat over

`WinBeat` renders the ticket's payout tally `+$X` on the same `_tBigAmount`, in the same gold, over
the same `_goldFlood` at peak alpha 0.50, with the same paired boost. **Whatever is ruled here should
probably be ruled for both**, or the two payoff moments will diverge — and this filing exists because
that class of divergence is what T68 was.

**Nothing is blocked.** The actionable half is closed and shipped; this is a state the player reaches
only after pressing, and it has been rendering this way since before T63.
