# R23 settlement cast — Route B, frame-locked. **Disposition 3: UNADJUDICATED.**

**Room lead · 2026-08-12 · `458d6f9` · one editor window, closed clean.**

**The pre-committed disposition that fired is №3: the instrument cannot separate the anchors, so the
result is returned UNADJUDICATED — a void, not a null (C37), and specifically NOT evidence that the
cast conforms.**

The subject's own numbers look like a pass. **They are not reported as one**, and that is the whole
reason §5 was written before the frames existed.

---

## 1. The pair — how "the glow and nothing else" was achieved

`SBR.RoomViewCapture.CaptureSettlementPair`, one batchmode run, **pin asserted**:
`[RoomViewCapture] slate PINNED and asserted: ROOMREF01`.

**Four halves, all rendered inside ONE editor callback.** `Update()` does not run between two
statements of one callback, so no clock, ticker or animation advances between halves — the pair is
frame-locked by construction, not by luck. A pair shot as two runs would carry R43's measured
residue (≤5/255, largest on the panel, which is where the subject sits).

**The ON half is not a simulation.** `TvLight.Update()` at `_flash01 = 1` reduces to
`color = Lerp(rest, flash, 1) = _flashColor` and `intensity = Lerp(restI, flashI, 1) = _flashIntensity`,
with no other term (T64's flicker multiplier is gone and its comment says so). Logged verbatim:

```
OFF: pointLight color=(0.350, 1.000, 0.500) intensity=0.5000     <- the C2 green at rest
ON : pointLight color=(0.818, 1.000, 0.610) intensity=0.9000     <- roomSettlementWarm x 0.9
```

**Panel live in both halves**, as the precondition requires — `TvLight` is the subject, not a
confound to be switched off.

**Anchors needed their own halves.** Neither anchor is a boxable surface: the lid's emissive face
sits behind the SureThing canvas (S63-am3's finding) and the key tube is outside the standing
frustum. Both are **contributions**, which is how batch 13 measured them. So four halves —
`BASE`, `GLOW`, `NOLID`, `NOKEY` — and each contribution is a linear difference against the same
BASE, converted once through the room's shared `linear_to_lab`.

## 2. The measurements — chroma beside every hue (§4)

| what | contribution | box | sd/mean | L\* | **chroma** | **hue** | verdict |
|---|---|---|---|---|---|---|---|
| **anchor LOW** — lid | `BASE − NOLID` | lid's own patch | 0.398 | 0.08 | **0.03** | 70.2° | **achromatic — no hue supportable** |
| **anchor HIGH** — key | `BASE − NOKEY` | pool core | 0.120 | 5.82 | 7.26 | **101.4°** | warm yellow |
| " | " | pool core ×3 | 0.176 | 3.02 | 3.76 | 103.0° | warm yellow |
| **subject** — glow | `GLOW − BASE` | wall-right plaster | 0.193 | 6.87 | 6.51 | **92.7°** | warm yellow |
| " | " | ceiling plaster | 0.128 | 2.42 | 2.28 | **87.3°** | warm yellow |

## 3. Why disposition 3 and not disposition 1

**Three independent reasons, any one sufficient:**

**(a) The low anchor is unmeasurable.** The lid's contribution is **chroma 0.03** — two orders below
the instrument's own 1.5 floor, at which it refuses a hue verdict. Independently confirmed here:
`BASE` vs `NOLID` differs **only** on a 35×101 px patch of the lid itself, extrema 4/3/2. **The lid's
emission does not reach the room** — exactly what R40-am measured, now reproduced by a different
method. There is no low anchor to sit above.

> **Corroborated by byte-identity, which needs no instrument at all.** On the focused-laptop and
> seated poses, `BASE`, `NOLID` and `NOKEY` are the **same file** — one MD5 between all three
> (`65afcd87…` focused, `c6b0627f…` seated). Zeroing the lid's emission changed *nothing* in a pose
> whose camera sits 0.52 m off the lid's own normal, because the SureThing canvas is 4 mm in front of
> it and opaque — S63-am3's unphotographable cue, reproduced. `GLOW` differs on all three poses.

**(b) The high anchor does not return its recorded value.** C44's test is to feed the bound its own
founding value. The record says the key sits at **~92°**; the instrument returns **101.4°** — a
**9.4° miss on a band 7° wide**. The instrument is therefore *not* calibrated on the band it is
judging, which is the precise defect that voided every historical V6 verdict.

**(c) The subject cannot be pinned to the band's width anyway.** Two surface-pure boxes in one frame
give **87.3°** and **92.7°** — **5.4° apart, 77 % of the band's total width**. A measurement whose
own spread nearly fills the band cannot adjudicate membership of it.

**So the subject's 87.3–92.7° is NOT reported as conforming.** It is what disposition 3 names in
advance: *not a pass, not a failure, and specifically not evidence that the cast conforms.*

## 4. Routed — the high anchor's recorded value may itself be wrong

Two independent figures agree with each other and disagree with the record:

| | hue |
|---|---|
| key tube authored `(0.92, 0.86, 0.42)` through `linear_to_lab` | **102.7°** |
| key's **rendered contribution**, this capture, pool core | **101.4°** |
| **the value in the record, and the band's upper anchor** | **~92°** |

Authored and rendered agree to **1.3°** — which is also a check on this method, since an
authored-to-rendered gap that small is what a clean isolation should produce. Both sit ~9–11° above
the recorded anchor.

**This is the same shape as the V6 provenance finding one docket earlier:** the ~92° entered the
record at batch 13 with no stated derivation, and T65-am2 then made it the band's **upper anchor**.
If it is mis-derived, the band's top is in the wrong place — and every judgement against 85–92°
inherits that, including this one. **Not ruled here; I cannot see which surface or pose the original
~92° was taken on.** Routed to the DD alongside this result.

## 5. What the re-run needs (disposition 3's own remedy)

Disposition 3 says the recipe re-runs with more signal and **states what changed**. On this evidence:

- **The low anchor cannot be rescued by exposure or box size.** It is not weak, it is absent — the
  emission does not reach the room. Either the band's low anchor is re-derived from something that
  does reach the room, or the ordering test needs a different low reference. **This is a ruling, not
  a capture parameter.**
- **The high anchor is fine as a measurement** and disagrees only with the record. Settle §4 first.
- **The subject wants one box, agreed in advance**, not two that differ by 5.4°. The DD's own
  `housing above panel` region is the obvious candidate and is TV's to specify.

## 6. Scope (C25)

*Reads:* one pinned, asserted, frame-locked four-half capture at the ratified standing pose;
contributions isolated by linear difference and converted once by the room's shared converter.
*Cannot see:* the seated and focused poses (shot, not measured — the standing pose is the only one
containing both anchors' pools and the subject); TV's own `housing above panel` box; and the
provenance of the recorded ~92°.

**T80 freeze respected** — nothing here changes C2, T9, T10 or T61. `TvLight`'s rest colour is
reported as measured because it is the OFF half of the pair, not proposed for change.
