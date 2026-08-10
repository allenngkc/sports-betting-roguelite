# Room → SureThing — the glyph blur: four measured negatives, and my diagnosis was wrong

**From:** room lead · **2026-08-08** · filed in **C25 form**

Handing this over because it is your surface and my instrument has run out of things it can rule out.
**Everything below is measurement, not illustration** — the main camera came back bit-identical on a
control pair in both sessions, which also answers the routed C34 question for this camera and pose.

---

## The symptom

Allen's walk of the post-C13 tree: *"everything else is great"*, one finding — **the SureThing UI on
the laptop is VERY BLURRY.** Whole surface uniformly soft, text legible but fuzzy.

## What it is NOT — four negatives, each measured

**1. Not the room's grade.** Graded against grade-bypassed, one Play session, same pose, only the
Volume flag differing:

| | ungraded vs graded |
|---|---|
| whole frame | **1.44×** sharper |
| **UI region only** | **1.14×** |

The grade softens **the room more than the UI**. A cause cannot explain a symptom it affects less
than the thing reported as fine. (Film grain also *inflates* the graded score by adding
high-frequency noise, so 1.14× is generous to the grade.)

**2. Not the bitmaps.** At 4× on one frame, the biro ink-ring box is a **crisp 1px hairline** while
`AWAY +181` directly below it has **feathered, grey-haloed edges**. Sprites sharp, glyphs soft. Your
surface's only true bitmaps are fine.

**3. Not the SDF atlas geometry.** 1024×1024, point size 90, `_GradientScale` **10** against padding
**9** — which is exactly what TMP expects. That parameter is correct.

**4. Not `_TextureWidth`/`_TextureHeight`, and this one was my call and it was wrong.** I found all
three faces describing a 1×1 texture against a 1024² atlas, and predicted that was the cause. Your
generator fix corrected it. Measured across three builds:

| region | before fix | after fix | after fix + forced reimport |
|---|---|---|---|
| UI panel | 334.3 | 335.1 | 338.4 |
| glyph strip | 1548.1 | 1557.6 | **1533.4** |
| ink-ring (control) | 722.0 | 706.8 | 706.8 |

**0.99× on the glyph strip.** I forced `ImportAsset(..., ForceUpdate)` on all three faces in the same
editor session as the render, so a stale Library import is excluded too.

**Your fix is correct as authored and is not the cause.** It was a real mismatch and worth fixing;
it simply is not this. Nobody should read "no change" as "wrong fix".

**5. Not the sampling point size — and this is the one hard positive result.** Same string at
**1.00× / 1.25× / 1.50×** magnification (FOV varied at a fixed camera, so magnification is the only
variable), controls bit-identical at every FOV, and a span reference confirming each arm really is a
magnification:

| mag | stroke px | **ramp px** | ratio |
|---|---|---|---|
| 1.00× | 3.509 | **1.679** | 0.478 |
| 1.25× | 4.256 | **1.683** | 0.395 |
| 1.50× | 4.758 | **1.743** | 0.366 |

**The stroke grows 36%; the ramp does not move.** An intrinsic SDF ramp would have reached 2.52 px at
1.5×. A ramp fixed in screen pixels is not a glyph-rendering property, so **sampling point size is not
the lever and arm B should not be produced.** Threshold and both predicted values were in the script
before the frames existed.

**6. Not SMAA.** The camera ships `SubpixelMorphologicalAntiAliasing`, which was the obvious candidate
for a fixed screen-space blur. Toggled off, same rig, control passed:

| region | SMAA on | SMAA off | change |
|---|---|---|---|
| UI glyphs | 1.677 | **1.574** | 0.938× |
| room, standing | 2.741 | 2.378 | 0.868× |

**6% of the ramp.** It sits at 1.574 px with antialiasing entirely off — still pinned, still far above
a hard edge. *(That also closes the look trade-off without a DD ruling: the room hardens more than the
UI gains, and it would not fix the UI anyway.)*

**7. Not render scale — and this one narrows the space rather than just closing a door.** Set at
runtime, controls passed, restored and read back:

| | glyph ramp |
|---|---|
| `renderScale 1.0` | 1.686 px |
| `renderScale 1.5` | **1.815 px** |

Predicted **0.667×** (~1.12 px) if the ramp were set by render resolution. Measured **1.077×** — no
narrowing at all, and if anything a slight widening. *(That 0.13 px rise is within my ±0.1 px
precision, so I am not claiming the widening is real; what is solid is the absence of narrowing.)*

**THE LOAD-BEARING FACT.** At 1.5 the frame is rendered at 1.5× internally and resolved down, so
anything applied **before or during** the resolve gets supersampled and should narrow. **This ramp did
not.** A ~1.7 px ramp that survives 1.5× supersampling is applied **AFTER the resolve, at output
resolution.** That is a small space, and it is the first positive constraint this hunt has produced
about *where* rather than *what*.

## What survives

**The ramp is FIXED IN SCREEN PIXELS at ~1.6 px, does not scale with the glyph, and survives 1.5×
supersampling.** That is the hard measurement, it is reproducible under controls, and it now carries a
location: after the resolve, at output resolution.

**No hypothesis is offered against it.** Two confident mechanisms have already been wrong here, and the
next step is the leads reading this against the pipeline together rather than my eighth guess.

The space I have not narrowed: canvas render mode and how a world-space canvas resolves against the
camera, the UI shader path, and render-target resolution between the canvas and the frame.
**I am deliberately not picking one.** Four slots produced two confident wrong answers — a mechanism
that fits every observation still is not evidence, and the remaining space is on your side of the seam.

## Scope (C25)

*What these read:* the focused-laptop pose at 2560×1440, Play Mode, live canvas (218 Graphics
asserted present before any arm — a blank frame and a soft frame score identically "sharp"). Focus
metric is variance-of-Laplacian, and it is **confounded by noise**: film grain raises it, so it
understates the grade's softening rather than overstating it.

*What they cannot see:* Allen's own display path — his screenshot measures **1.23× softer** than my
2560 capture of the same content at the same scale, so some of what he saw is his view resolution and
not the build. His eye at the desk pose is the acceptance bar regardless.

## Evidence

Committed on `room-refinement`: `artifacts/room-visual-pass/blur-ab/` (graded + ungraded),
`blur-verify/` (post-fix, with control pair), `blur-reimport/` (post-fix + forced reimport, with
control pair). Each set carries `control-a`/`control-b`; both came back bit-identical.
