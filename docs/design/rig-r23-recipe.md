# R23/R26 rig recipe — how to shoot a screens-dark + grade-bypassed set

**Written by:** room lead (`room-refinement`), 2026-08-03, at the orchestrator's request.
**For:** the TV seat's T48 re-shoots. T48 requires *"re-shoot the TV set screens-dark AND
grade-bypassed, or a shared-grade conclusion is T19 in a new colour."*
**Source of truth:** `[RM] unity/SBR/Assets/SBR/Editor/RoomViewCapture.cs` (`CaptureConformance`,
`ShootConformanceSet`, `DarkenScreens`) and `[RM] tools/room_gate_check.py`. Where this document and
that code disagree, the code is right — tell me and I will correct this file.

---

## 0. What the set is for, in one line

A gameplay frame cannot separate *"the room is cool"* from *"the screens are cool"*, because three
emissive panels and a coloured screen light are pouring into it. The set removes the screens from the
measurement, then shoots the same rig twice — graded and grade-bypassed — so any difference between
the two frames is attributable to the grade **and to nothing else**.

The **graded pass is canonical.** The grade *is* the surface, not a layer over it. The bypassed pass
exists only to answer the question the graded frames cannot: if it reads cool, is that the **light**
or the **grade**?

---

## 1. Invocation

```
Unity.exe -batchmode -quit -projectPath <project>
          -executeMethod SBR.RoomViewCapture.CaptureConformance
          -outDir <ABSOLUTE path>
```

- **Edit Mode on purpose.** No Play Mode, no domain reload, so it completes in a single
  `-executeMethod` call and is reliable in batch. The Play Mode harness (`CaptureAll`) exists to show
  live screen content — which is precisely what this set must not contain, so the domain-reload dance
  buys nothing here and costs reliability.
- **Never `-nographics`.** Post-processing needs a graphics device. (`CaptureAll` additionally needs
  *no* `-quit` — it exits the editor itself. `CaptureConformance` takes `-quit` normally.)
- `-outDir` must be absolute; the harness throws without it.
- **Exit 0 arrives before the frames do. Never read the output directory to decide whether a run
  worked.** The shell returns while the editor is still booting, so *exit 0 with an empty `-outDir`*
  is exactly what a **healthy** run looks like for its first minute or two — and it is
  indistinguishable from a real failure. `CaptureAll` is the sharp case, because Play Mode adds a
  domain reload before anything is shot, but the trap belongs to the harness, not the mode: it
  signals completion to nobody.

  **Wait, then verify.** Poll until `Temp/UnityLockfile` has cleared *and* the expected PNGs exist,
  then confirm the log carries one `[RoomViewCapture] wrote` line per frame.

  **Serialize runs.** A second editor launched while the project is still held exits **0** after
  licensing, having opened nothing — a ~47-line log and an empty directory, with no error anywhere.

  This cost a cycle on 2026-08-09: the empty directory was read as failure, the run was relaunched in
  a second host mode, and for a few minutes two arms of one comparison existed on two instruments —
  the batch-9 defect, re-committed by someone who had just finished reading about it. §9.4's *"exit
  code 0 alone does not prove the method ran"* has a twin worth stating in its own words: **exit
  code 0 with no artifacts does not prove it failed.**

## 2. Scene and camera state

| what | value | why |
|---|---|---|
| Scene | `Assets/Scenes/Room.unity`, opened `OpenSceneMode.Single` | |
| Camera | the object named **`PlayerCamera`**; throws if absent | |
| `FirstPersonController` | **disabled** before shooting | stops the controller writing the transform between the pose being set and the render |
| Resolution | 2560 × 1440, `RenderTexture` ARGB32, depth 24, **`antiAliasing = 1`** | |
| Readback | `ReadPixels` → `Texture2D` RGB24 → `EncodeToPNG` | |

## 3. Screens-dark — how the emitters are silenced

`DarkenScreens()`. Two halves, and the second is the one people forget.

**a. Emission, per renderer, via `MaterialPropertyBlock`:**

```csharp
mr.GetPropertyBlock(block);
block.SetColor("_EmissionColor", Color.black);
block.SetColor("_BaseColor", new Color(0.010f, 0.010f, 0.012f, 1f));
mr.SetPropertyBlock(block);
```

Applied to `MeshRenderer`s named exactly **`TVScreen`**, **`LaptopScreen`**, **`PhoneScreen`**, found
with `FindObjectsInactive.Include`.

> **Use a property block, never edit the materials.** The block overrides per *renderer*, so the
> shared material assets on disk are untouched and the next ordinary build is unaffected. Editing the
> materials would silently corrupt the room for every other capture — and the emission keyword it
> would disturb is the one that already broke this project once (see `Mat()` in
> `GrayboxRoomBuilder.cs`, which sets `RealtimeEmissive` specifically because URP's postprocessor
> recomputes `_EMISSION` and stripped it).

**b. The two screen-driven lights are disabled:** `TvLight` and `PhoneBuzzLight`.

That is screen colour arriving by another route, and the whole point is that **no screen's colour
enters the measured cast**. `TvLight` in particular is the C2 green — leave it on and you are
measuring the TV, not the surface.

## 4. Grade bypass — how it is switched

```csharp
var vol = GameObject.Find("RoomPostFx").GetComponent<Volume>();
// ... graded pass ...
vol.enabled = false;
// ... bypassed pass, identical rig and framing ...
vol.enabled = true;
```

- If `RoomPostFx` is missing the harness **throws**, deliberately: *"a set missing half its pair would
  silently look complete."* Do not soften that.
- **`RoomPostFx` is the unified grade (C20).** It is one global pass over the room *and every screen in
  it*. So the TV panel is inside this volume — toggling it is toggling the same artefact the room
  toggles. That is exactly why T48 wants the TV set shot this way, and why no slice tunes it alone.

## 5. Framing — identical between passes, or the pair isolates nothing

The bypassed pass is written with an **`-UNGRADED`** filename suffix so the two land side by side
under otherwise identical names. Room poses, for reference:

| frame | eye | look | FOV |
|---|---|---|---|
| `conformance-seated-screens-dark[-UNGRADED].png` | `(-0.950, 1.150, 0.300)` | at `(1.232, 1.100, 0.300)` | 17° |
| `conformance-room-screens-dark[-UNGRADED].png` | `(0.300, 1.640, -1.400)` | `+Z`, up `+Y` | 68° |

**Two frames, and the second is not padding.** The ruling names the seated rig *and* requires wall,
floor and bunk to be reported — a 17° close-up on a dark panel cannot contain three surfaces. Both are
needed to satisfy both halves of one sentence.

**TV should choose its own poses.** Copy the *mechanism*, not the coordinates. The one rule that does
not bend: framing must be **byte-identical between the graded and bypassed passes**, or the pair stops
being an isolation and becomes two pictures.

## 6. Measuring it — do not eyeball the result

```
python tools/room_gate_check.py --scene <Room.unity> --captures <dir> \
       --conformance <conformance dir> \
       --report artifacts/.../gate-runs/<name>.txt
```

Reads `conformance-room-screens-dark.png` and, when present, its `-UNGRADED` twin, and prints
**L\*, chroma and CIELAB hue angle per region** with a WARM / COOL / neutral verdict in side-by-side
columns.

Three properties of the instrument worth inheriting rather than re-deriving:

- **Averages in linear light and converts once, at the end.** Hue is an *angle*: 10° and 350° average
  to 180°, the opposite of both. Averaging the linear tristimulus first is the only correct form.
- **Chroma floor of 1.5.** Below that a hue angle is the direction of a vector too short to trust, and
  the verdict returns `neutral` rather than calling a near-grey surface cool.
- **Bands:** warm `20–110°`, cool `200–300°`. Anything between is reported neutral, not forced.

**Every region must be a single surface.** Validate with **sd/mean of luminance ≤ ~0.15**. A box
straddling two surfaces makes the test meaningless in both directions — a bright outlier dominates the
mean, so a real change gets diluted below threshold while a trivial shift in the bright element swings
it past. Two of the room's original boxes did exactly this (0.374 and 0.406, containing the fixture and
the window) and were replaced.

**Pass `--report`.** Until 2026-08-03 this harness printed to stdout only, and every number that
reached the register got there by being hand-copied out of a terminal — the claim was the artifact and
no run was reproducible. C11 wants the evidence, C17 wants it retained, C25 wants its scope attached.

## 7. Two things that will bite

1. **Reproducibility is a feature and it has been verified.** At `d126318` every frame shared with the
   earlier run had an **identical MD5** — the isolation is deterministic, not one favourable render. If
   your re-shoot does not reproduce, something in the rig moved; find it before trusting the numbers.
2. **A cool cast that appears only when screens are lit is a screen finding (C2/C13), never a surface
   finding.** That is R23's ruling and it cuts both ways — if the TV's set reads cool only with its
   panel live, the finding belongs to the panel, not to the grade.

---

## Known-good reference numbers (room, post-T48, at `a9ad7ab`)

Useful as a sanity check that the rig is behaving before trusting a new surface's numbers.

| region | graded | ungraded |
|---|---|---|
| wall (right plaster) | chroma 0.97, hue 110.5° neutral | chroma 1.66, hue 110.8° |
| wall (far plaster) | chroma 3.56, hue 275.6° **COOL** | chroma 5.49, hue 275.8° **COOL** |
| floor (aisle) | chroma 1.66, hue 272.6° **COOL** | chroma 2.97, hue 273.3° **COOL** |
| bunk (1 / couch side) | chroma 0.33, hue 200.9° neutral | chroma 0.57 |
| bunk (2 mattress) | chroma 6.33, hue 99.4° WARM | chroma 7.84, hue 100.1° |
| ceiling plaster | chroma 0.33, hue 114.1° neutral | chroma 0.55 |

Note the two COOL regions are **cool ungraded too**, which is what convicts the window's own pool
rather than the grade — and is why law 1.1's remaining failure is currently a DD question about where
the test samples, not a build fix.
