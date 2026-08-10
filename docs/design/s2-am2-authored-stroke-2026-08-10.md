# S2-am2 baseline — the authored-stroke denominator

**Ruled:** Design Director, 2026-08-10 · **For:** SureThing lead · **Answers:** the one value the lane
is held on. **Scope (Allen, 2026-08-10):** the rig's boxed season records and row numbers. **Not
TicketLeg**, and not price figures — neither has a value here and neither should be given one by
analogy.

The room lead's recipe is right that this lane could not supply it: *"It is your face, your point
size."* Here it is, with its derivation, because a denominator recorded without one is a number a
future seat cannot check.

---

## The two values

| element group | face · instance · size | **`--authored-stroke`** |
|---|---|---|
| season records (`5-4`, `6-3`, …) | Archivo **Regular 400**, 13 px canvas | **1.94** frame px |
| row numbers `01`–`06` | Archivo Narrow **Regular 400**, 15 px canvas | **2.03** frame px |

Pass them per group. **Do not pool the groups** — different face, different size, different stroke;
§2a of the recipe already forbids pooling and this is the same error one level up.

---

## Derivation, so it can be re-derived rather than trusted

**Source of the sizes** — `SportsbookApp.cs`: the record is `13, … record, _font, LaptopTrack.Records`
(`TeamLine`, ~line 335); the row number is `15, … ToString("00"), _fontCond` (`BuildMatchupCard`,
~line 250). Tracking does not enter — letter-spacing moves glyphs apart, it does not thicken a stem.

**Stem metric** — median stem width over digits `0`–`9`, measured on the outline at 100× the shipped
ppem, 50 % threshold, middle 30–70 % of the glyph band (the same convention as the instrument's
`crossings()`). Archivo Regular **0.09231 em**; Archivo Narrow Regular **0.08400 em**. At the shipped
sizes: 1.200 and 1.260 canvas px.

**Canvas → frame scale = 1.61483 frame px per canvas px.** From `LaptopScreen`: `screenWorldSize`
0.32 × 0.22 m over `referencePixelsWide` 1024 → 3200 canvas px/m (704/0.22 agrees exactly, so the
mapping is uniform). Camera at 0.52 m, 30° vertical FOV, 1440 px tall → 0.278667 m of world across
the frame height → 5167.5 frame px/m. 5167.5 ÷ 3200 = 1.61483.

**Three independent checks on that scale**, because everything downstream multiplies by it:

1. The screen renders 1654 × 1137 px = **79 % of frame height**; `LaptopScreen.cs`'s own S63-am2 note
   describes the focused pose as *"the lid filling ~80% of the frame."*
2. Six row numbers span 5 × 78 + 56 = 446 canvas px → **720 frame px**; the room lead's eye-confirmed
   box measured **730** tall, which is 720 plus the pad a hand-cut box carries.
3. Measured ÷ authored comes out **1.44×** and **1.58×** — two groups, one surface, one blur,
   consistent inflation. See below for why that check is load-bearing.

**No material dilation.** `normalStyle: 0` and `_WeightNormal: 0` on both SDF assets, so the shipped
stroke is the outline's stroke. (`boldStyle: 0.75` reaches bold-tagged runs only; neither group is one.)

---

## The trap this nearly walked into — read it before re-deriving anything

**`Archivo.ttf` is a variable font whose default instance is SemiBold 600, not Regular.** Rasterising
it without selecting an instance yields **2.66 frame px** for the season records — 37 % high — and
nothing about that number looks wrong on its face.

This is **S29's trap exactly**, and the generator's own source already carries the scar:
`SureThingTmpFontAssets.cs:20` — *"Archivo.ttf's faceIndex 0 reports SemiBold, and its Regular sits at
a named-instance index. The first cut of this generator took the default and so shipped the surface's
roman voice at weight 600 for the whole migration, unchosen."* Ruled Regular 400 by Allen; the one
deliberate 600 is the wordmark (S20).

**What caught it was not the source note — it was check 3.** At weight 600 the inflations were 1.05×
and 1.58×: two groups on one frame under one blur, disagreeing by half. That incoherence is what sent
me back to the instance. **Keep that check whenever this value is re-derived** — a wrong denominator
is silent in every other direction.

---

## Two things about the number that will otherwise be misread

**1. The authored form is a DIFFERENT QUANTITY from batch 26's ratified pairing. Never compare them.**

Batch 26's `0.775` / `0.789` are `ramp ÷ MEASURED stroke`. Against the authored denominator the same
frames give **1.115** and **1.243**. Nothing got worse between those two lines — the denominator
changed. A future seat reading `0.775 → 1.115` as a collapse would be reading an artefact of the
amendment that was made to remove artefacts. **Record the form beside the number, every time.**

**2. Both groups land above 1.0, and that is a real statement, not an error.**

Ratio > 1 means the transition is wider than the whole intended stroke: at 13 px the stroke is
1.94 frame px and the build's own screen-space ramp floor is ~1.68 px (C38, already ruled a
characteristic). No part of the smallest fact reaches full intensity. **It is still not a finding
against the surface** — S2-am3 struck the legibility claim as an artefact, and both groups were read
and verdicted *reads: yes* at review distance on the 1:1 crop.

So the unit S2-am2 asks for is **"1.115 and it reads"**, not "1.115". A future 1.4 is worse than this;
a future 1.115 with a *no* verdict is worse than this. The number alone orders nothing.

---

## What this value does and does not survive

- **Constant across the flood removal.** That is the entire point of the amendment — the denominator
  cannot be moved by the thing under test. Re-shoot, re-cut the boxes if the board moved (recipe §2a),
  pass the same two values.
- **Not constant across the pose.** These are frame px at the ratified acceptance view. Any other pose
  rescales them: `stem_canvas_px × 1.61483 × (new scale ÷ 1.61483)`. **Record the pose beside the
  value** — a screen-px stroke without its view is the same defect as a gate without its geometry (C18).
- **Not constant across a type change.** Any size or instance change re-derives from §Derivation.
  S2-am2 clause 4 discharged type re-authoring permanently; this is not a licence to move type, only
  the recipe for the value if Allen ever does.

---

**Owed nothing further.** The lane is unblocked. If the re-shoot returns a ratio the lane thinks is
bad, that is a finding to route to this seat — not a licence to change type (recipe §4).
