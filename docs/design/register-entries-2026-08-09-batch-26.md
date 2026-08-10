# Register entries — 2026-08-09, batch 26

**Seat:** Design Director (`main-2` terminal) · **Source:**
`dd-import/s2am3-legibility-reread-2026-08-09/` (room lead) — full frame, 1:1 native-scale crop,
measured boxes, `baseline-report.txt`.

**Batch 25's first pre-committed outcome fires. No second ruling was needed — which is the point of
pre-committing.**

---

## S2-am3-cl — The legibility claim was an ARTIFACT. STRUCK

**Read at review distance on the 1:1 native-scale crop, this seat, 2026-08-09.** Judged on the honest
crop, not the 3×/4× nearest-neighbour ones — the lane was right to warn that nearest upscaling makes a
soft edge look like a hard staircase and flatters exactly the thing under test.

**Every season record reads: `5-4 · 5-4 · 2-7 · 3-6 · 5-4 · 6-3 · 5-4 · 4-5 · 4-5 · 3-6 · 7-2 · 2-7`.
Every row number reads: `01`–`06`.** Twelve records and six numbers, none guessed at, none ambiguous.

They read **softly** — the digits carry no crisp core and the strokes bloom rather than terminate. They
are nonetheless unambiguous, and *unambiguous* is the test. **This is outcome 1, not outcome 3:** I am
not hedging a pass into "marginal". Marginal would mean I was guessing at a glyph. I was not.

**So S2-am's legibility claim is struck**, recorded as this seat's error (§1.5): *at or below
legibility* was read off a frame carrying ~27% more softness in ratio than the path the player uses,
and it did not survive the clean one.

**Two independent confirmations, and they agree:**

- **My pixel read**, above — the pixels contain the information.
- **Allen's own acceptance verdict** on the clean path — *"everything is clear now"*, both surfaces,
  given before this frame existed and without reference to it.

**Stated as a limit on my own instrument (C39's lesson, applied to myself):** I read a PNG through my
display path, which is not Allen's eye at his desk. What I can rule is that **the pixels resolve the
glyphs**. That Allen reads them is a separate question with a separate instrument, and he has already
answered it. The two agree, which is why this closes rather than escalating.

**On provenance:** the set correctly records that `surething-form-blurry.png`'s capture time is still
unresolved. **It does not need resolving.** The contamination was established by *measurement* — 0.613
against the harness's 0.482 — not by timestamp. Measurement beats provenance here, and a frame that
demonstrably carries more softness than the player's path is disqualified as evidence about the
player's path whatever its clock says.

## Clause 2 STANDS — and now, for the first time, it is calibrated

Batch 25 ruled clause 2 independent of the legibility read, on the mechanism. **That ruling is
confirmed and this batch adds what it never had: a number with a verdict attached.**

| element group | ramp ÷ stroke | reads? |
|---|---|---|
| season records (smallest product facts) | **0.775** | **yes** |
| row numbers `01`–`06` | **0.789** | **yes** |
| price figures (bundle's headline) | 0.482 | yes |

**Batch 25's arithmetic prediction is confirmed by measurement:** the smallest type carries the worst
ratio, and the hunt's headline 0.482 was taken on type larger than the floor it was being used to
reason about. **The surface's true worst case is 0.789, and it passes.**

That pairing is the yield. A bare baseline number is a number; **0.789-and-it-reads is a number a
future reading can be worse than.** S2-am2's baseline records both — the ratio and the verdict — or it
records nothing usable.

## S2-am2-am — The baseline's instrument amends: ramp ÷ AUTHORED stroke

**Amended** · DD 2026-08-09, on the lane's non-monotonicity finding, which is load-bearing and was
raised unprompted.

`ramp ÷ stroke` **wanders and then collapses** as blur grows — 0.710 → 0.774 → 0.686 → 0.583 → 0.653 →
0.677 → 0.229 across σ 0.0–2.0 — because blur merges adjacent glyphs and the falling 50% crossing that
ends a stroke runs on into the next gap, so the **denominator outruns the numerator**. A larger ratio
does not mean a softer surface; a smaller one does not mean a sharper one.

**This is batch 20's Owed·2, confirmed with a mechanism.** I flagged that Allen's stroke denominator
moved 1.23× where geometry predicted 0.79× and named two candidate causes. It is the first one, and it
is systematic rather than incidental.

**Clause 2 expresses the enforcement half in this ratio, and across time that is unsound** — which is
precisely the regression use S2-am2's baseline exists for. Amended:

- **Within one frame at one view the ratio stands as-is.** It is a fraction-of-stroke-in-transition,
  comparable across elements in the same frame, which is what the three rows above are. **This batch's
  numbers are unaffected.**
- **Across time, the denominator is the AUTHORED stroke** — the face's own metric at the shipped point
  size — **not the measured one.** A constant cannot be inflated by the artefact that inflates a
  measurement, so the quantity becomes monotonic in blur while keeping clause 2's design meaning:
  *what fraction of the intended stroke is transition.*
- The lane's *"regress on the ramp"* is right about which quantity is well-behaved (±2.4% against seven
  kernels) and would have cost the design meaning — a 2.2 px ramp is unremarkable on a 10 px stroke and
  fatal on a 2.8 px one. **Authored-stroke denominator keeps both.**

## Adopted — three corrections to the instrument's own record

1. **`1.680` is the BUILD's floor, not an instrument artefact.** The instrument's own ramp on a
   synthetic hard edge is 0.800 px; 1.680 is 2.1× that, and it is C38's ~1.6 px characteristic already
   ruled real. Quadrature subtraction stays correct for isolating blur *above* the floor — **and a
   residual near zero means "at the floor", never "sharp".** Recorded here so no later document reads
   as though the instrument had been corrected for.
2. **`1.680` now has a retained derivation.** It previously existed in one handoff's prose and nowhere
   else — a number with no derivation is C18 §4.1's defect in scalar form. Re-run, tabled, retained.
3. **C27 working, in its own right:** two boxes were re-cut after a clipped stem and a sliver of team
   name got inside them. **Neither would have failed loudly** — a clipped stem quietly biases a median.
   That is exactly why C27 is eye-confirmation rather than a variance threshold, and it is the
   clause's best instance to date.

---

## Recorded

The lane returned numbers and frames and **refused to adjudicate**, because the disposition was
pre-committed and turned on a read that is this seat's and not its crop. It then raised the one finding
that undermines the instrument its own numbers were produced with.

**Two pre-commitments have now fired in two days without a second ruling** — the blur's three-outcome
direction test, and this one. Both were written before the deciding evidence existed, and both closed
on arrival. That is the practice paying for itself, and it is worth saying plainly while it is cheap
to notice.
