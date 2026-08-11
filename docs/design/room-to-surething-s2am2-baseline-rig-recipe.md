# Rig recipe — re-shooting the S2-am2 clause-2 baseline

**Written by:** room lead (`room-refinement`), 2026-08-09, at Allen's request, for the **SureThing
seat**. **Source of truth:** `tools/glyph_ramp_baseline.py`, `tools/glyph_ramp_ratio.py`
(`crossings()`), and `unity/SBR/Assets/SBR/Editor/RoomViewCapture.cs`. **Where this document and the
code disagree, the code is right — tell me and I will correct this file.**

Companion to `rig-r23-recipe.md`, which covers the screens-dark conformance set. Same house rules,
different instrument.

---

## 0. What this is for, and what it is not

**S2-am3 is CLOSED.** The legibility claim was struck as an artifact (batch 26) on the frames this
lane staged. **You are not re-litigating that.**

What survives and needs re-shooting over time is **clause 2's baseline**: the ratio *paired with a
verdict*, so a future reading has something to be worse than. The immediate trigger is batch 27 —
the full-field flood is struck and deleted. **That changes what sits behind the type, so the baseline
must be re-taken once it lands.** A ground change is exactly the kind of thing this number exists to
catch.

**Batch 26's ratified pairing, which your re-shoot is compared against:**

| element group | ramp ÷ stroke | reads? | comparable across deals? |
|---|---|---|---|
| **row numbers `01`–`06`** | **0.789** | **yes** | **YES — this is the across-time baseline** |
| season records (smallest product facts) | 0.775 | yes | **no** — within-frame only (§2a-i) |
| price figures (bundle's headline) | 0.482 | yes | not the floor's type |

**Record the verdict with the number or the number is not usable.** *0.789-and-it-reads* is the unit.

**Compare the row numbers across shoots. Do not compare the season records across shoots** — batch 31
replaced the ~0.037 bound for exactly that reason, and §2a-i says why in full. All three numbers
remain valid *within* batch 26's frame.

## 1. Shoot the frame

The measurement is only as good as the frame, and **the frame must not come from the Game view.**

```
Unity.exe -batchmode -projectPath <project>
          -executeMethod SBR.RoomViewCapture.CaptureAll
          -outDir <ABSOLUTE path>
```

- **No `-quit`.** The harness exits the editor itself; `-quit` races it.
- **Never `-nographics`.** Post-processing needs a graphics device.
- `-outDir` must be absolute; the harness throws without it.
- Produces the three ratified poses. **`focused-laptop-desk.png` is the one you want** — 0.52 m along
  the lid normal, 30° FOV, 2560×1440.

**Why not a Game-view grab.** `Shoot()` renders `PlayerCamera` into a `RenderTexture` and reads it
back (`RoomViewCapture.cs:1883`). It never touches the Game view, so it cannot carry *Low Resolution
Aspect Ratios*. That toggle is **per-user Editor state and nothing in the repo holds it off** — it is
what contaminated `surething-form-blurry.png` and cost the studio the whole blur hunt. A harness frame
is immune by construction; your own screen grab is not.

### 1a. Three traps that cost this lane a cycle on 2026-08-09

1. **Exit 0 arrives before the frames do.** The shell returns while the editor is still booting, so
   *exit 0 with an empty `-outDir`* is what a **healthy** run looks like for its first minute or two.
   It is indistinguishable from a real failure. **Poll until `Temp/UnityLockfile` clears AND the PNGs
   exist**, then confirm the log carries one `[RoomViewCapture] wrote` line per frame.
2. **Serialize runs.** A second editor launched while the project is still held exits **0** after
   licensing having opened nothing — a ~47-line log with no error in it.
3. **`CaptureAll` is not byte-deterministic.** It renders live Play-Mode content, so two runs give
   byte-different frames. That is not a fault and a byte-comparison control on it will always read red
   and always mean nothing. **The measured number is the control** — two runs agreed to 0.09 on R9-A.

## 2. Measure it

```
python tools/glyph_ramp_baseline.py --report artifacts/.../<name>.txt \
       [--authored-stroke <PX>]
```

Part A characterizes the instrument, Part B records the two-surface baseline, Part C measures the
smallest product facts. **Part A runs first and gates the rest** — if the instrument stops tracking a
known kernel within 15%, Part B does not run (C37).

### 2a. PIN THE SLATE. The boxes are downstream of it

**Amended · DD batch 29.** **The baseline shoot pins its slate. Re-cutting boxes per frame is the
fallback, not the method.**

**The mechanism, which SureThing hit and this recipe originally missed:** a season record sits
*immediately after* the team name, so **its x tracks the name's length**. `MIDDLEMEN 5-4` and
`GRAVEDIGGERS 5-4` put the record in different places. **No fixed record box survives a re-deal** —
and it will not fail loudly when it stops surviving one. It will land on the wall behind the digits,
or on half a glyph, and return a number.

So: **pin the deal before shooting.** Same slate, same strings, same positions, and the boxes below
stay valid shoot after shoot — which is the whole point of a baseline that a future reading can be
worse than.

> This recipe already carried the right rule in §3 — *"if the surface is content-dependent, pin and
> assert the seed before shooting"* — but filed it under **reporting**, as something to state after
> the fact. It is not a reporting clause. It is the shoot's method, and it belongs here, before the
> boxes. C34 filed one step too late is C34 not applied.

**Fallback, when the slate genuinely cannot be pinned:** re-cut every box against that frame and
eye-confirm each one (C27). Treat it as a one-frame measurement, not as a baseline — an unpinned
number cannot be compared to a later one, because you cannot tell a softness change from a re-deal.

`SMALLEST` in `glyph_ramp_baseline.py` holds 12 season-record boxes and one row-number column, in
frame pixels, **cut against the batch-26 frame and correct for it**. They are not a template. **If
the flood removal moves the board, or the deal differs, they are wrong and will silently measure the
wrong thing.**

Two of mine needed re-cutting even on their own frame: one clipped a glyph, one caught a sliver of the
team name behind the digits. **Neither failed loudly** — a clipped stem just quietly biases the median
— which is why C27 is eye-confirmation and not a variance threshold. Crop them, look at them, then
measure.

**Do not pool the small type with larger type.** The ramp is fixed in screen px and the stroke scales
with size, so including team names or price figures inflates the denominator and flatters the result.
That is exactly how the hunt's headline 0.482 came to be quoted for a floor it did not describe.

### 2a-i. THE INSTRUMENT'S VALIDITY PRECONDITION — state it, do not derive it

**Amended · batch 31.** **`ramp ÷ stroke` is only meaningful against an IDENTICAL STRING at an
IDENTICAL SIZE.** Outside that, the comparison is not weak — it is undefined.

This was derivable from Part A's characterization and was never written down, which is the whole
problem: **a lane that baselines a deal-dependent group makes no visible mistake at the time.** The
boxes are eye-confirmed, the stems are plentiful, the sd/mean is clean, the number is plausible and
lands next to the last one. Nothing anywhere goes red. The error only surfaces later, as a "change"
that is really two different strings measured under one label.

**Consequence, ruled (batch 31) — the ~0.037 bound is REPLACED:**

| group | across deals | why |
|---|---|---|
| **row numbers `01`–`06`** | **THE across-time baseline** | the string is identical in every deal, at a fixed size and position — the precondition holds by construction |
| season records (`5-4`, `9-0`, …) | **within-frame only** | the string itself changes with the deal, *and* its x tracks the team name beside it. Comparing it across deals was the instrument read outside its validity |

**Season records are not demoted as a measurement.** Within one frame they remain the smallest
product fact and the honest worst case, and batch 26's 0.775-and-it-reads stands as that. What they
cannot be is a *time series*.

**Pinning the slate does not rescue them.** §2a's pin makes two shoots of the **same** deal
comparable; it does nothing for two shoots of **different** deals, which is what any re-seed or
content change produces. Pin *and* precondition are separate requirements and neither implies the
other.

### 2b. Which denominator — this is the amended part, so read it

**S2-am2-am (batch 26):**

- **Within one frame at one view: the MEASURED stroke.** A fraction-of-stroke-in-transition,
  comparable across elements in the same frame. Batch 26's three rows are this form.
- **Across time: the AUTHORED stroke** — the face's own metric at the shipped point size, in screen px
  at this view. Pass it as `--authored-stroke`. The tool reports it beside the measured form.

**Why.** `ramp ÷ measured stroke` **is not monotonic in blur**: 0.710 → 0.774 → 0.686 → 0.583 → 0.653
→ 0.677 → 0.229 across σ 0.0–2.0, because blur merges adjacent glyphs and the falling 50% crossing
that ends a stroke runs on into the next gap, so the denominator outruns the numerator. **A larger
ratio does not mean a softer surface.** A constant denominator cannot be inflated by that artefact.

**You must supply the authored stroke — this lane cannot.** It is your face, your point size. The tool
prints a loud note when the flag is absent and reports only the within-frame form.

### 2c. Two numbers not to misread

- **`1.680` is the BUILD's floor, not an instrument artefact.** Measured: the instrument's own ramp on
  a synthetic hard-edged bar is **0.800 px** — the linear-interpolation limit. `1.680` is 2.1× that,
  so it is C38's real ~1.6 px screen-space ramp, already ruled a characteristic. Subtracting it in
  quadrature isolates blur *above the known floor*, which is right for a regression — but **a residual
  near zero means "at the floor", not "sharp".**
- **Saturation.** The ratio compresses above σ≈1, where the measured ramp reaches ~3.2 px. Batch 26's
  numbers sit below that. **A future reading near the ceiling means "badly blurred", not a
  proportional worsening.**

## 3. Report it

Ratio **and** verdict, per element group, with: the view, the space, the boxes, the frame's
provenance, and whether the authored-stroke form was used. `--report` tees the whole run to a file —
pass it every time. C11 wants the evidence, C17 wants it retained, C25 wants its scope attached.

**Reproducibility (C34):** the frame is not byte-reproducible, so state the run, the commit and the
pose rather than a hash. **State the slate you pinned and assert it before shooting** — see §2a, where
that requirement now lives, because it governs the shoot and not merely the write-up. Record whether
the boxes were the pinned-slate set or a per-frame fallback cut; a reader cannot tell the two apart
from the numbers, and only one of them is comparable to a later shoot.

## 4. What is NOT asked

No fix, no re-authoring, no tuning. **S2-am2 clause 4 discharged type re-authoring permanently** and
nothing here re-opens it. If the re-shoot comes back worse, that is a finding to route — not a licence
to change type.
