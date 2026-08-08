# TV — T63 needs a lever this seat does not hold. **QUESTION.**

**From:** TV sweat lead · **Filed:** 2026-08-07 · **Rides:** next inbox push
**Status:** T63's structural half is BUILT and PROVEN on frames. The remaining gap is unreachable
without a ruling. **Nothing is blocked behind this** — the build is green and shippable as it stands.

---

## 1. The measurement, and what it cannot see (C25)

**Unit: Rec.709 luma on display-encoded values (C33), quoted with every number.** Instrument
`tools/ladder_read.py`; zones from `LayoutGrid(980,550)`'s own constants, not eyeballed.
**Calibration:** this instrument reproduces all four of batch 13's own T63 figures exactly — ball
0.902, scoreline 0.874, band 0.827, column 0.786 — which is what makes the numbers below comparable
to the ruling rather than a second private scale.

Cash-out **actionable**, seed 27182818, post-fix:

| element | before | after | tier |
|---|---|---|---|
| quiet scoreline | 0.866 | **0.873** | L3 |
| **actionable band** (peak) | 0.827 | **0.844** | **L4 — the only sustained one** |
| band field (zone mean) | 0.696 | **0.746** | |
| event strip | 0.858 | **0.626** | L2 (ruled batch 14, landed) |

**The band rose 0.017 and the gap closed from 0.046 to 0.029. It is still below.**

**What this cannot see:** one seed, one frame per state, panel only (the room is V6). It cannot see
which element holds the HDR token at the instant of capture, and it cannot tell a zone's own content
from bloom entering the zone's box — see §4, which is a real consequence and not only an instrument
limit. **Resolution (C32):** 8-bit input, ~0.004 luma per code value; differences under ~0.01 are not
reported as ordering. The 0.029 gap is well above that and is real.

## 2. What was fixed, and it was structural

The HDR material sat on `_tCashOut` — the money **figure** — and never on `_cashOutField`, the gold
field. The field could not be boosted at all, so granting `HdrFocus.CashOut` moved a number and left
the band at rest. Measured, splitting the zone: **field 0.696, figure 0.827.** The 0.827 the ruling
measured was the figure; the field — what reads as "the band" at four metres — was the *dimmest* of
the four competitors, not the third-brightest.

Fixed: the field carries its own HDR material instance and `ApplyBoost`'s `CashOut` case drives both,
the same shape `Payout` has always used. One token, two graphics, one occupant.

## 3. Why the last 0.029 cannot be taken from here

**The obvious move was tried and it is measurably worse.** Painting the field `goldL4`
(1.84, 1.31, 0.29) — this surface's own L4 gold — fails twice:

- A canvas vertex colour is packed to **Color32**, so it clamps to (255, 255, 74): **hue 60°, lemon**,
  not gold. A hue change nobody ruled.
- At the 1.4 boost a full-width field that bright **blooms across the whole panel**. Measured: the
  band, the event strip *and* the risk/pays footer all read hue **60.0° at ~61% saturation**, because
  every zone's peak had become this field's bloom instead of its own content.

Reverted. The field is back on `gold`, which clamps to (255, 209, 46) and is still gold.

**The general result, and it is the reason this is a ruling and not a tuning task.** In C33's unit
`gold` is **0.844** and cold white `flavorColor` is **0.942**. Rec.709 weights green at 0.7152, and
gold is by definition low-blue and sub-maximal-green. **Within the 0–1 range a canvas colour is
clamped to, no gold can out-rank cold white at all** — reaching 0.942 requires G≈1.0, which is lemon.
So the band's brightness has to come from the boost, and **the boost is sealed at 1.4 (T49-cl, "do
not re-open")**.

Every lever is therefore either sealed or above this seat.

## 4. A consequence that needs a ruling either way

**When the field is lit it blooms into its neighbours' readings.** On actionable frames the event
strip peak rises 0.626 → 0.833 and risk/pays 0.430 → 0.840, both taking the field's hue. The elements
are **not repainted** — this is bloom entering the measurement box — but it is also what a viewer
sees, and it means the L4 element currently asserts itself partly by washing its neighbours rather
than by out-ranking them. This is new: before the fix the field was unboosted and sat under the bloom
threshold. It is a direct consequence of doing the ruled thing.

## 5. The question

**Which lever?**

- **(a) Unseal the boost for this one element.** Smallest change, lands the ruling directly. Costs
  re-opening T49-cl, which was sealed with "bloom was never the lever" — and §4 says bloom is now
  visibly part of how this element reads, which may be reason to look again or may be reason not to.
- **(b) Ratify a lighter gold.** Already an open quarantine item — shipped `#ffd12e` against token
  `#F2BC45`. A gold with more green reaches higher in the ruled unit without clamping to lemon, but
  it moves the money colour on every surface that shares it.
- **(c) Accept level-with.** The band at 0.844 against 0.873 is 0.029 below; at seated distance that
  may already read as co-equal. This is the only option needing no code, and it is a judgement only
  the DD can make against frames.
- **(d) Lower the quiet scoreline.** Named for completeness and **not recommended** — the score is
  the element §4.1 says nothing may outgrow, and dimming it to let money win inverts that.

**The seat's read, offered not taken:** (c) or (b). (a) buys the number but §4 suggests more bloom is
the wrong direction for an element already washing its neighbours.

## 6. Not blocked

The build is green — compile clean, engine 160/160, EditMode 237/237, PlayMode 64 passed with one
documented flake and five `[Explicit]` skips. **T65 is proven closed on frames** (eight won-leg frames
at hue 130.4°/40.4%/0.175 against a pre-fix 40.7°/71.1%/0.347), the event strip's L2 landed, and T64's
flicker deletions are in. T63's structural half stands whichever lever is chosen; only the last 0.029
waits.
