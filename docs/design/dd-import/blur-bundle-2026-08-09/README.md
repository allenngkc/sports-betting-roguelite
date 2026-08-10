# The laptop blur — the whole chain

**From:** room lead · **2026-08-09** · filed in **C25 form** · read-only assembly, no editor used.

**The finding in one line:** the surface has a **real ~1.6 px softness floor in the build**, and
**Allen's display path was adding roughly 56% more on top of it**. Two contributions, stacked. Every
previous round measured only one of them.

**RESOLVED 2026-08-09.** The display half was `Low Resolution Aspect Ratios` enabled in his Game
view; with it off his verdict is *"everything is clear now"* on both surfaces, and the acceptance bar
is met. The build-side floor stands as a measured characteristic — §1 for what that leaves to rule
on.

**Start with `crops/split-allen-display-vs-harness.png`** — the same price rows, same content, from
both paths, resampled to a common height so the eye compares like with like.

---

## 1. The split — the result that reframes everything

Allen's playtest frame against my capture of the same screen at the same zoom:

| | ramp px | stroke px | **ramp ÷ stroke** |
|---|---|---|---|
| **Allen's display path** | 2.610 | 4.258 | **0.613** |
| my harness (`rt-bilinear`) | 1.674 | 3.475 | **0.482** |

**Ratio, not pixels**, because the two frames are at different scales — his UI is 1330 px wide
against my 1675 px. The ratio is scale-independent, which is why it was chosen in advance as the
comparison.

**Three outcomes were pre-committed before his frame existed.** The third fired:

| outcome | meaning | result |
|---|---|---|
| match ≈0.48 | build floor real, harness clean | — |
| his **lower** | my harness was adding blur; retract everything | — |
| **his higher** | build floor real **AND** his path adds more | ✅ **this one** |

**Consequences, in order of importance:**

1. **My harness is exonerated.** It reads *sharper* than the real display path, so it was never adding
   the blur. **Nothing in this hunt retracts.**
2. **The build floor is real** — ~1.6 px, independent of my instrument.
3. **His display path adds ~56% on top** (1.674 → 2.610 px). This is the piece no measurement before
   his frame could see, because my harness is not in the loop that produced his complaint.

**And the arithmetic points at an upscale.** His frame is *less* magnified than mine (1.30× against
1.64× of the authored 1024 px artboard), so a ramp that scaled with content would be **narrower** in
his — predicted **1.327 px**. Measured **2.610 px**, or **1.97× the prediction**: almost exactly a
factor of two, on the smaller frame. A 1330 px UI also implies a full render only ~2033 px wide where
mine is 2560.

**✅ CLOSED — the display-path half is identified and fixed.** Allen found it himself:
**`Low Resolution Aspect Ratios` was enabled in his Unity Game view.** That setting renders the frame
at a reduced resolution and enlarges it to fill the view — an upscale, applied after the frame is
drawn, at output resolution.

> **Allen, on turning it off: *"everything is clear now."***

**His eye is the acceptance bar, and it now reads CLEAR on both surfaces** — the laptop and the phone.

**Both halves of the split were correctly attributed.** The cause matches the measurements in
direction *and* magnitude: an upscale is exactly what widens a ramp without the content growing, which
is why his frame measured **1.97× the magnification-scaled prediction** while being the *smaller*
frame; and a reduced-resolution render is why his UI came out 1330 px where mine was 1675 px at the
same pose. *(I inferred a ~2033 px full render from that ratio. The direction and the ~2× factor are
confirmed; I am not claiming the exact pixel figure was, since the setting's own reduction factor was
never read.)*

**What this leaves:** the **~1.6 px build-side floor is a measured characteristic of the build**, not a
defect anyone has ruled on. With the acceptance bar already met, whether it needs anything at all is
purely the DD's read.

## 2. What the build-side floor is NOT — six exonerations, each measured

| # | candidate | evidence | verdict |
|---|---|---|---|
| 1 | the room's grade | whole frame **1.44×** sharper ungraded, UI region only **1.14×** — it softens the *room* more than the UI, and the room was reported fine | not it *(see §4 — partial)* |
| 2 | ~~the bitmaps~~ | **WITHDRAWN.** The ink ring measured **2.488 px** against glyphs' 1.683 — 1.48× *softer*, not sharper | unsupported, back on the board |
| 3 | SDF atlas geometry | 1024², 90 pt, `_GradientScale` **10** against padding **9** — correct as authored | not it |
| 4 | `_TextureWidth`/`_TextureHeight` mirror | real defect (1×1 against a 1024² atlas), fixed by SureThing; glyph strip **1548.1 → 1557.6 → 1533.4** across three builds, and a forced `ImportAsset(ForceUpdate)` in-session changed nothing | not it |
| 5 | SMAA | toggled off: ramp **1.677 → 1.574**, 6%. Sits at 1.574 px with antialiasing entirely off | not it |
| 6 | render scale | 1.5× supersampling: ramp **1.686 → 1.815**, no narrowing at all against a predicted 0.667× | not it |

**The positive result that survived all six:** the ramp is **fixed in screen pixels at ~1.6 px** and
does not scale with the glyph — 1.679 / 1.683 / 1.743 px across 1.00× / 1.25× / 1.50× magnification
while the stroke grew **36%**. Anything applied before or during the resolve is supersampled and must
narrow; **this does not, so it is applied after the resolve, at output resolution.**

**Confirmed frame-wide, not glyph-specific:** at 1.5× a hard *geometry* edge narrowed only **0.912×**
where physics requires 0.667×. Glyphs and geometry are floored alike.

**`_Sharpness` moves it and does not fix it:** 0.00 → 1.00 gives **1.799 → 1.626 px**, monotonic,
**9.6%**. Real, and nowhere near the 50% a material-constant fix would need. **The fix is not one
constant.**

## 3. The two instrument laws, with their founding cases

*Adopted per batch 19; register IDs held at the DD seat.*

**A control must bracket the interval it certifies — and be checked by the other half of the
instrument, never asserted by the half being checked.**
*Founding case:* an emission set passed `control-a == control-b` **while the room was being mutated
underneath it**. The opening pair bracketed only the warm-up; a capture was resetting renderers to
their shared-material value instead of restoring their own state, so every later frame was shot
against a changed room. A closing `control-z` catches it; the opening pair cannot. Two capture sets
were discarded to learn this.

**An instrument must resolve the band it judges — C32, applied to one's own null.**
*Founding case, twice:* whole-pixel ramp counting carried **±25%** on a 2 px ramp, so a three-point
trend built on it was not a trend. And my `_Sharpness` null at render scale 1.0 was **invalid**: a
successful halving lands at 0.84 px, *under* the ~1 px single-sample floor, so that test could never
have shown success either way. Both were my own numbers, and the second was caught by SureThing, not
by me.

## 4. Corrections I owe, unprompted

- **The grade elimination is PARTIAL.** `PC_RPAsset` carries a **global default volume**
  (`SampleSceneProfile`: Bloom, Vignette, Tonemapping all active) which my bypass never touched — I
  disabled only the `RoomPostFx` scene volume. All three are pre-resolve and so fail the acceptance
  anyway, which is why it is filed as a correction rather than a live candidate.
- **The ink ring was never a valid edge reference** and I reported it as one off a 4× crop. A thin
  high-contrast line reads crisp to the eye while measuring softer than the type beside it. My eye,
  not a measurement. It is the only claim in this hunt that had to be withdrawn, and the only one that
  had no number behind it.
- **I claimed the poses matched to 1.5% and they are 21% apart.** I treated a remembered *area*
  fraction as a *width* fraction. The ratio comparison survives it because it is scale-independent —
  which is the only reason §1 stands.
- **The backbuffer arm was my error.** I named it load-bearing and then ran it in `-batchmode`, the one
  mode where `ScreenCapture` cannot work. Allen's frames replaced it.

## 5. Scope (C25)

*What this reads:* the ratified focused-laptop pose, Play Mode, live canvas asserted present before
any arm (a blank frame and a soft frame score identically sharp). Ramp is the sub-pixel 10–90%
crossing distance; stroke is 50%-to-50%. Controls bit-identical on every arm reported here.

*What it could not see, and what closed it:* the cause of the 56% was outside every instrument here —
it was a Game-view setting on Allen's machine, found by him, not by measurement. My frames could
establish that a display contribution existed and size it; they could never have named it. **That
division is the durable lesson of this hunt:** the harness bounded the problem, the human closed it.
His frame is also a **crop** (aspect 1.4536 against the UI's authored 1.4545), so the crop itself
contributed nothing.

## 6. Contents

- `crops/split-allen-display-vs-harness.png` — **start here.** Same rows, both paths, common height.
- `crops/glyph-edge-harness-4x.png` — the build-side edge at 4×.
- Allen's originals, **referenced not copied**:
  `main-2/docs/design/dd-import/allen-playtest-2026-08-09/surething-form-blurry.png` (1330×915) and
  `phone-bookie-blurry.png` (586×1197).
- Frame sets on `room-refinement`: `blur-ab/`, `blur-verify/`, `blur-reimport/`, `smaa-ab/`,
  `glyph-scale-v2/`, `sharp-sweep/`, `harness-audit/`.
- Instrument: `tools/glyph_ramp_ratio.py`.

**The phone:** Allen's second frame was the phone, blurry-but-readable, and he wanted it clear too.
**Both surfaces now read clear to him with the Game-view setting off** — so the phone needed no
separate investigation, and the reference it would have required was never built. Queuing it behind
the display-path half was the right order by his own ruling; it dissolved with the same fix.
