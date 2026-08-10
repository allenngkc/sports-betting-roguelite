# C13 — joint pipeline read on the in-room glyph blur

**Append-only.** Two halves, two authors, one file. Neither lead edits the other's section.

| half | scope | author | state |
|---|---|---|---|
| Part 1 | canvas → camera | **SureThing UI lead** | filed 2026-08-08 |
| Part 2 | camera → output (resolve, upscaling, final blit, output resolution) | **Room lead** | reserved below |

**Acceptance as issued:** a candidate must predict a fixed **~1.68 px** screen-space ramp that
**survives 1.5× supersampling**.

**Every ramp figure in Part 1 is in pixels, and the whole finding turns on *which* pixels** — see
§0.1. Read-only throughout; nothing built, no editor taken.

---

## Part 1 — SureThing side: canvas → camera

### 0. Headline, stated against the acceptance

**The "post-resolve only" clause is too strong, and the exception is on my side of the line, by a
citable code path.**

TMP's SDF shader normalises its antialiasing ramp against `_ScreenParams`. URP binds `_ScreenParams`
from `camera.pixelWidth` — the **display** viewport — and keeps a *separate* `_ScaledScreenParams`
for the render-scaled target. The two are set on adjacent lines and are deliberately different
values.

**So a TMP SDF ramp is invariant under URP render scale by construction, and it is pre-resolve.**
Room's null result is not a locating constraint against it — it is the exact behaviour TMP predicts.

I am not claiming this *is* the cause. I am claiming the search space did not shrink the way the null
was read, and that the prime suspect is back on the canvas side where a cheap experiment can settle
it (§4).

### 0.1 A unit that needs its reference stated — C33-am3's shape, one axis over

A ramp width in "px" is meaningless without the resolution it is referred to. At render scale 1.5,
**1.68 px render-target-referred and 1.68 px display-referred are different physical widths** — they
differ by exactly the factor the whole experiment turns on. Room's prediction (1.12) and my
derivation below disagree *only* about which reference the shader uses.

This is C33-am3's lesson on a different quantity: **state the unit and its reference, not just the
number.** Recommended to the DD as an amendment rather than assumed — spatial measurements on this
project now have the same failure mode that luminance had.

Convention for Part 1: **all ramp figures are display-referred** unless marked otherwise.

### 1. The path, stage by stage — read from source and scene, not from prior notes

| # | stage | evidence | fixed-resolution intermediate? | scaled by render scale? |
|---|---|---|---|---|
| 1 | Canvas, `RenderMode.WorldSpace` | `LaptopScreen.cs:102` | no — world geometry | yes |
| 2 | Canvas rect 1024 × 704 authored px | `:96-97`, scene `referencePixelsWide: 1024`, `screenWorldSize: {0.32, 0.22}` | no — a layout grid, not a raster | n/a |
| 3 | Canvas scale 0.32 / 1024 = **3.125e-4 m per authored px** | `:114` | no | n/a |
| 4 | `worldCamera = Camera.main` | `:79` | no — raycast target for `GraphicRaycaster`, not a render path | n/a |
| 5 | Camera: perspective, vertical FOV 68 | scene `field of view: 68`, `orthographic: 0` | no | yes |
| 6 | **Camera target texture: none** | scene `m_TargetTexture: {fileID: 0}` | **no** | yes |
| 7 | **Dynamic resolution: off** | scene `m_AllowDynamicResolution: 0` | no | n/a |
| 8 | URP PC asset: render scale 1, MSAA 1, upscaling filter Auto | `PC_RPAsset.asset:28-31` | no | — |
| 9 | TMP text → `TMP_SDF` shader | `Assets/TextMesh Pro/Shaders/TMP_SDF.shader` | **see §2 — this is the one** | **no** |
| 10 | Ink sprites → `Image`, fixed-resolution textures | — | **yes**, magnified by M (§3) | no (also invariant) |

**Structural result: there is no rasterisation intermediate on the SureThing side.** No
RenderTexture, no camera target, no dynamic resolution, no `CanvasScaler` on this canvas. Everything
this surface submits is geometry rasterised at the camera's own render resolution.

That makes rows 9 and 10 the only two stages on my side that can host a width fixed in display
pixels — and **both do**, for two different reasons.

*Not to be confused:* the project's one `ScreenSpaceOverlay` canvas is `InteractionHud`
(`InteractionHud.cs:54-62`), which is a different surface. An overlay canvas composites at output
resolution and would be render-scale-invariant for a third, unrelated reason. No laptop text is on
it.

### 2. The derivation — why a TMP SDF ramp survives supersampling

`TMP_SDF.shader:183-186`:

```hlsl
float2 pixelSize = vPosition.w;
pixelSize /= float2(_ScaleX, _ScaleY) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
float scale  = rsqrt(dot(pixelSize, pixelSize));
scale       *= abs(input.texcoord0.w) * _GradientScale * (_Sharpness + 1);
```

The fragment resolves alpha as `saturate(d · scale − bias)`, so **the alpha ramp spans `1/scale` in
SDF distance units**. `scale` is therefore "rasterised pixels per SDF unit, as the shader believes
them to be."

**`_ScreenParams` is the only resolution term in that expression.** `UNITY_MATRIX_P` carries FOV,
aspect, near and far — none of which change with render scale — and `_ScaleX`/`_ScaleY` are TMP's
manual override, 1 by default.

Now the binding, `ScriptableRenderer.cs:221-222` and `:292-293`:

```csharp
float cameraWidth  = (float)camera.pixelWidth;      // display viewport
float cameraHeight = (float)camera.pixelHeight;
...
cmd.SetGlobalVector(ShaderPropertyId.screenParams,       new Vector4(cameraWidth, cameraHeight, ...));
cmd.SetGlobalVector(ShaderPropertyId.scaledScreenParams, new Vector4(scaledCameraTargetWidth, scaledCameraTargetHeight, ...));
```

`camera.pixelWidth` is the camera's viewport in **display** pixels and does not carry URP's render
scale; the scaled target is bound separately as `_ScaledScreenParams`. **The existence of the second
uniform is the proof that the first is not scaled** — and URP overrides `cameraWidth` with the scaled
target in exactly one branch, XR (`:236-237`), which does not apply here.

Let R be render scale and D the display resolution. The glyph rasterises across R·D.

| case | `scale` | ramp, render-referred | ramp, display-referred | at R = 1.5 |
|---|---|---|---|---|
| **`_ScreenParams` = D** (URP's actual binding) | independent of R | ∝ R | **constant** | 1.68 → **1.68** |
| counterfactual: `_ScreenParams` = R·D | ∝ R | constant | ∝ 1/R | 1.68 → 1.12 |

**The second row is the prediction room tested. The first row is what URP actually binds, and it is
what room measured** — 1.686 → 1.815, against a predicted constant. (Room's residual ×1.077 is
unclaimed by them and unclaimed by me; a 1.5× downsample of an already-soft edge is a plausible
source of a small positive residual, but I have not modelled it and it is near their stated
precision.)

### 3. Magnification, for completeness — it governs row 10, not row 9

One authored canvas px is 3.125e-4 m. For output height `H` px and camera distance `d` m at vertical
FOV 68:

```
M  =  3.125e-4 · H / (2 · d · tan 34°)  =  2.3165e-4 · H / d      [display px per authored px]
```

**1:1 falls at d = 25.0 cm for a 1080-tall output, 33.4 cm at 1440.** Nearer than that the surface is
magnified, and every *bitmap* on it — the ink rings and strikes — carries a bilinear ramp of about
one source texel, which is likewise display-referred and likewise renderscale-invariant. This is
consistent with the ink ring measuring softer than the glyphs and is **not** evidence about the
glyphs either way. It is recorded so the two invariances are not conflated: row 9 and row 10 survive
supersampling for entirely different reasons.

**I have not measured the desk-pose distance** — the pose lives in room's rig, so M at the acceptance
view is theirs to state. The formula is supplied so it costs them one number.

### 4. What this does to my own `_Sharpness` null — it does not survive

The `_Sharpness` arms (`fb8e248`, and the cost scoping at `f53c8ee`) concluded that `scale` is not
what limits the edge, on the grounds that doubling `scale` did not halve the ramp. **That conclusion
was run at the one render scale where its predicted effect is unobservable, and I am withdrawing it
as unsupported.**

At the 13 px column head: 1.68 px, predicted ×0.5 → **0.84 px**, against a **~1 px floor for
single-sampled rasterisation** that the same document states. The experiment could not have resolved
a success. The 26 px slot (2.04 → predicted 1.02) sits *at* the floor and is the only marginally
informative arm; it did not move, but its three readings were non-monotonic (2.04 / 1.83 / 2.00)
inside a quantisation room independently characterised as ±25%.

**So `scale` is not exonerated, and neither are the levers retired with it** — `_GradientScale`,
atlas padding and sampling point size were all retired by the argument that they feed the same
`scale` term. That argument is sound; the measurement it rested on is not.

This is the second control on this hunt to fail on the same fault: the bitmap control was withdrawn
because ink strokes have no hard edge to lose, and the `_Sharpness` control now falls because its
predicted result sits under the instrument's floor. **Both were nulls read as exclusions.**

### 5. The one experiment that discriminates, and it is cheap

**Sweep `_Sharpness` 0 → 1 at render scale 1.5.** Room's harness already does the runtime render-scale
override (`RoomViewCapture.cs`, `5c9ad05`); the font-asset arms already exist on this side.

At R = 1.5 the rasterisation floor is ~0.67 px display-referred, so the predicted 1.68 → 0.84 becomes
observable. It resolves both open questions in one shoot:

| outcome | reading | consequence |
|---|---|---|
| ramp narrows toward ~0.84 | `scale` **is** the limiter; `_Sharpness` is a live lever, and §4's floor explains the earlier null | the fix is a material constant. **No room re-baseline, no re-architecture — options A and B both become unnecessary** |
| ramp holds ~1.68 | `scale` genuinely is not the limiter, independently of the floor | the hunt moves downstream, and Part 2 is properly motivated rather than motivated by an inference §2 undermines |

**This should be shot before anything is decided on A/B/C.** The cost-scoping doc (`f53c8ee`) put
Allen's choice between a room-wide re-baseline and accepting the softness; if the top row lands,
neither is the answer and the choice does not need making.

### 6. The control Part 2's null still needs — a question, not a claim

**Did anything in the frame narrow at 1.5?**

Room reports the controls bit-identical, which I read as proving the override was cleanly restored —
a different and necessary check. What the null additionally needs is a **positive** control: a hard
room-geometry edge, measured in the same pair of frames, that narrows by ×0.667 as ordinary
supersampling requires.

If a geometry edge narrowed and the glyph ramp did not, the null is physics and §2 explains it. **If
nothing in the frame narrowed, the override did not reach the captured path** and the result is
instrument rather than evidence. That distinction is room's to make and I cannot make it from here.

### 7. What this read cannot tell you (C25)

- **Nothing here is a measurement.** Part 1 is source, scene and shader algebra only. Every number
  quoted from a frame is room's or my predecessor's, cited as theirs.
- **I have not verified the `_ScreenParams` binding at runtime**, only in URP's source at the version
  in this project's package cache (`com.unity.render-pipelines.universal@276396f56b3f`). A frame-debug
  capture would confirm it; the code path is unambiguous but it is still a read, not an observation.
- **§2 explains an invariance, not a width.** It says a TMP ramp *would* survive supersampling. It
  does not say the ~1.68 px came from TMP rather than from something in Part 2's scope, and §4 is
  what would separate those.
- **I have not measured M at the acceptance view** (§3), and the desk-pose distance is not mine.
- **Whether ~1.68 px is wrong at all remains unmeasured by anyone** — it is a judgement about how a
  screen should read in a room, and it is the DD's and Allen's. Nothing in this note changes that.

**— SureThing UI lead, 2026-08-08**

---

## Part 2 — Room side: camera → output

*Reserved for the room lead: the resolve, upscaling filter and final blit, display scaling, and the
capture path's own resolution. §6 above is a question for this half, not a finding against it.*

*Filed by the room lead, 2026-08-08. Read-only: source, scene and asset files. No builds, no frames
taken for this section. Everything below is a source fact or a stated inference from one.*

### 0. Headline against the acceptance

**The acceptance is: predict a fixed ~1.68 px screen-space ramp that SURVIVES 1.5× supersampling.**
Anything running **before or during** the resolve is supersampled and must narrow, so the whole of the
pre-resolve stage fails the acceptance by construction. That eliminates most of this half in one line
and leaves a genuinely small space.

**I found no post-resolve stage in this project's configuration that predicts a 1.68 px ramp.** What I
did find are two corrections to the board and one uncovered path, below.

### 1. The configuration, as read

| | value | source |
|---|---|---|
| active RP asset | `PC_RPAsset` (guid `4b83569d…`, from `GraphicsSettings`) | `ProjectSettings/GraphicsSettings.asset` |
| render scale | **1.0** | `PC_RPAsset` |
| MSAA | **1** (off) | `PC_RPAsset` |
| upscaling filter | **0 = Automatic** | `PC_RPAsset` |
| HDR | on, `HDRColorBufferPrecision 0` | `PC_RPAsset` |
| colour grading | **LDR**, LUT size **32** | `PC_RPAsset` |
| renderer features | **ScreenSpaceAmbientOcclusion**, one only | `PC_Renderer` |
| intermediate texture | `0` = Auto | `PC_Renderer` |
| camera AA | **SMAA** (`m_Antialiasing: 2`) | `Room.unity` |

**Pre-resolve, therefore failing the acceptance:** SSAO, bloom, colour grading/LUT, chromatic
aberration, vignette, tonemapping, SMAA. All are supersampled at 1.5 and must narrow. *(SMAA was also
tested and measured: ramp 1.677 → 1.574 with it off, ~6%.)*

**Post-resolve, therefore eligible:** URP's final pass — film grain, dithering, and the final blit —
plus anything downstream of the camera's target.

### 2. ⚠️ Correction to the board: my "not the grade" elimination was PARTIAL

**`PC_RPAsset` carries a global default volume profile — `SampleSceneProfile` (guid `10fc4df2…`).**
It is active independently of any scene volume:

| component | state | values |
|---|---|---|
| Bloom | **active** | threshold 1, intensity 0.25, scatter 0.5, highQualityFiltering **1** |
| Vignette | **active** | intensity 0.2 |
| Tonemapping | **active** | mode 1, paperWhite 234 |
| MotionBlur | inactive | — |

**My grade bypass disabled only the `RoomPostFx` scene volume.** This second, global volume stayed on
in *both* arms. So "not the grade" was measured against one volume of two and **goes back on the board
as partial** — though note every component above is pre-resolve and so fails the acceptance anyway,
which is why I am recording it as a correction rather than a new candidate.

### 3. ⚠️ The uncovered path, and it is mine

**Every number I have reported was measured through the capture harness, not the display path.**
`Shoot()` renders the camera into a 2560×1440 `RenderTexture` and `ReadPixels` back. That RT is
created with Unity's default filter mode and is itself an output-resolution stage that **no control I
have run covers** — the control pairs prove two captures agree with each other, which a constant
instrument contribution would also satisfy.

So a ~1.7 px ramp introduced by the capture path would be: fixed in screen pixels ✅, invariant to
glyph magnification ✅, and **surviving 1.5× supersampling** ✅ — it meets all three acceptance
conditions, and it is the only stage I can name that does.

**I am not claiming this is the cause.** Allen sees blur on his own display, which the harness is not
in. But the harness could be adding a ramp on top, in which case the number the two halves are
reasoning from is partly mine. It is testable without a build: capture the same pose twice with the
RT's `filterMode` set `Point` versus `Bilinear`, and separately compare against a screenshot taken
outside the harness.

### 4. What this half cannot tell you (C25)

- **Nothing here is a measurement.** Source, scene and asset files only; every frame number quoted is
  from earlier runs and cited as such.
- **I have not confirmed which URP path applies film grain and dithering in this build** — UberPost at
  render resolution or the final pass at output resolution. That distinction decides whether grain is
  eligible at all, and resolving it needs a frame debugger, which is a build.
- **Film grain is noise, not a blur.** Even if it is post-resolve, it would corrupt a ramp measurement
  rather than create a smooth 1.68 px transition. I record it as eligible-by-position, not as a
  mechanism that predicts the shape.
- **The render-scale null it all rests on is itself unvalidated** — §6's geometry control was never
  run, and the frames were discarded before it was asked for. It is being re-shot now.

---

## Part 3 — Closing: the answer, and what each half got right

**Filed by the SureThing UI lead, 2026-08-09.** Appended; nothing above is edited. Part 1's §6
geometry control is now moot and is answered here instead.

### 1. The answer

**Allen's Game view had *Low Resolution Aspect Ratios* enabled.** Turned off, both surfaces read clear
to his eye. **No build change is needed.**

It fits every constraint the hunt accumulated, simultaneously — which is precisely why it survived
nine eliminations:

| constraint | why this satisfies it |
|---|---|
| fixed ~1.68 px, constant across glyph sizes | it is a resample of the finished image, referred to the display |
| survives 1.5× supersampling | it acts at output resolution, downstream of the scaled target entirely |
| frame-wide — glyphs **and** geometry floor | it is applied to the whole view, not to any draw |
| both surfaces, on separate canvases | same reason |
| no material property reaches it | the glyph is already correct before it happens |
| the harness could not clear itself | **it was never in the game at all** |

### 2. The scorecard, stated plainly

**Room's locating inference was right and mine was the objection.** *After the resolve, at output
resolution* is exactly where this lives.

**Part 1 §2 remains true and was not the cause.** TMP's SDF ramp really is normalised to
`_ScreenParams`, URP really does bind that from `camera.pixelWidth`, and a TMP ramp really is
render-scale-invariant by construction. It was a sound derivation from a true premise that was not
the operative one. §0 said at the time that it was not a claim of cause; that hedge is the only
reason it cost nothing.

**The thread that mattered was §6**, and not for the reason it was written. It asked whether *anything*
in the 1.5× pair narrowed, and said that if nothing did, **the null is instrument rather than
physics.** That was right, and the word *instrument* was one layer too narrow: the defect was not in
the capture harness but in the **display path the complaint arrived through**. Room's Part 2 §4
records that the geometry control was never run and its frames were discarded — so the question stayed
open to the end, and the answer arrived from Allen's eye rather than from either of our halves.

### 3. The two-surface observation

`surething-form-blurry.png` and `phone-bookie-blurry.png` — Allen's own playtest frames, 2026-08-09.

**The phone blurs identically on a separate canvas.** `PhoneScreen` is its own world-space canvas on
the same camera (`PhoneScreen.cs:61-62`, `:165`), sharing no font asset configuration, no material and
no layout code with the laptop. **That kills every surface-specific candidate in one frame** — and it
did so before anyone knew the cause, which is what a good discriminator does.

Recorded as a method note rather than a trophy: **the cheapest discriminator in the whole hunt was
noticing that a second surface had the same symptom.** It cost one `ls` of an import directory. Nine
measured eliminations preceded it.

### 4. The un-retired verdicts — the lesson, and it is now three

Two verdicts were un-retired during this hunt, both nulls that had been read as exclusions:

- **The bitmap control** — *bitmaps stay sharp while glyphs are soft* eliminated canvas render scale
  and world-space pixel density. The ink rings are hand-drawn soft-edged strokes measuring 6.50 px of
  ramp against a glyph's 2.92 px. **They have no hard edge to lose, so they could show neither
  softening nor sharpening.** The control could not witness the failure it guarded.
- **The `_Sharpness` null** — doubling the SDF `scale` term did not halve the ramp, which retired
  `_Sharpness`, `_GradientScale`, atlas padding and sampling point size together. It was run at the one
  render scale where the predicted result (1.68 → 0.84 px) sits under the ~1 px single-sample floor.
  **The experiment could not have resolved a success.**

Both are adopted as law in batch 19: *a control must be able to witness the failure it guards*, and
*a null is invalid if success would sit under the instrument's own floor.*

**The resolution adds a third, and it is the one that would have shortened this hunt most:**

> **A search space is a claim, and it needs a control like any other.** Every elimination here was
> sound *inside the pipeline*, and the defect was outside it. Nine correct negatives cannot find a
> cause that the boundary excluded before the first measurement — and nothing in the method was
> capable of noticing that the boundary was the assumption.

The tell was present and was read as a limitation rather than as evidence: **the harness could not run
the decisive case**, and the complaint had arrived through a path no instrument covered. Batch 19
recorded that *an instrument that cannot run the decisive case does not get to return a verdict.*
Stated forward, it is stronger: **when the instrument cannot reach where the complaint came from, that
gap is the first place to look, not a caveat to carry.**

### 5. What survives, and what this closing cannot say (C25)

**Survives, and should not be undone:**

- **The `_TextureWidth` mirror fix (`6bd6da2`) was a real defect and stays.** Exonerated as the *cause*,
  never as a defect. It is now gated: the bootstrap's mirror line reads `1024x1024 against configured
  1024x1024 — AGREE` on both faces, verified again on freshly generated assets 2026-08-09.
- **The `_Sharpness` margin stays unspent, as ruled.** Maxed it buys 9.6%, not 50%. Spending it would
  convert a diagnosable defect into a marginal one that still fails and no longer points anywhere.
- **Part 1 §2's derivation** is a true property of this type stack and is worth keeping on the record
  independently of this hunt: **a TMP SDF ramp cannot be narrowed by URP render scale.** Anyone who
  later proposes supersampling to sharpen text on this project should be handed that table.

**This closing cannot say:**

- **Why the setting was on**, or for how long, or whether any earlier frame in the evidence set was
  shot through it. **No frame in any bundle has been re-examined against this.** If a past measurement
  was taken from Allen's Game view rather than from a harness capture, it inherits the resample — that
  is a scoping question for the DD, not a finding.
- **Nothing here re-opens a granted item.** Raised as a boundary, not a claim.

**— SureThing UI lead, 2026-08-09**
