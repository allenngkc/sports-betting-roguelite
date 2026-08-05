# Room Visual Pass — Concept Prompt Set v2 (unconstrained)

**Supersedes:** `CONCEPT_DIRECTION_PROMPTS.md`
**Author:** Claude Opus 5, room-art lead
**Date:** 2026-07-24
**Status:** awaiting generated samples → Allen picks one direction

## What changed from v1

Allen lifted two constraints that shaped every v1 prompt:

1. The room no longer has to be a cool-blue palette. Palette is open.
2. "Static room art stays cyan/white; green/red/gold are money-only" no longer applies.

The v1 prompts had that palette law written into their `Color palette` line, which is why all three v1 concepts came back as variations of the same blue room. With the law lifted, the three directions are re-conceived around **three different emotional theses**, not three material treatments of one mood.

The TV's green idle screen is a placeholder owned by another workstream. All prompts below render screens as plain neutral light panels.

## How to run these

Attach **three** images to each generation, in this order:

1. `baseline/standing-overview.png` — **the edit target.** This is the only image that gets transformed.
2. `baseline/seated-tv-couch.png` — structural reference for the TV.
3. `baseline/focused-laptop-desk.png` — structural reference for the desk, laptop, and phone.

Images 2 and 3 exist to stop the model inventing screen geometry. Image 1 is very dark — that is correct and is part of the problem being solved, but if a model refuses to read the geometry, say "the input is deliberately underexposed; read the room layout from it and relight it freely."

Generate one direction per run. Do not blend them.

### Known failure mode

The v1 Direction A generation invented a mini fridge beside the desk and had to be surgically corrected. The real fridge sits at world `(-0.95, 0.425, -1.65)`, at entry-left, **outside this camera frame**. Every prompt below explicitly forbids a visible refrigerator. Check each result for one.

---

## A — Sodium Hour

**Thesis:** loneliness. The world outside is warm and awake without him.
*(v1 called this "Blue-Hour Pressure." The believable direction gets far more interesting when it stops being blue.)*

```text
Use case: stylised game environment concept art
Asset type: first-person environment concept for a Unity indie roguelite

Input: the attached standing-overview capture is the edit target. Preserve its camera
position, focal length, perspective, room proportions, wall/ceiling/floor planes, window
position, and the placement and scale of every existing object: the bunk-bed-over-couch on
the left, the wall-mounted TV on the right, the small desk with open laptop and phone at
the far right, the low cube stool in the centre, and the clear central walking lane. The
two supporting images show the TV and desk compositions; use them only to keep that
geometry honest.

Primary request: repaint this exact room as "Sodium Hour" — 3:17 AM in the apartment of a
sports bettor who is losing. Tell the whole story through a collision of two light sources:
dirty amber sodium streetlight pushing in through the window from a world that is warm and
awake without him, and the cold clinical glow of his own screens holding him in place.

Lighting: a low raking sodium-orange key from the window, laying long warm bars across the
floor and up the left wall and catching dust in the air. Cold white-cyan screen light from
the TV and laptop cutting hard against it, with a visible temperature boundary where the
two meet in the middle of the room. Deep soft shadow in the corners that still holds
detail. No overhead light — the ceiling fixture is off, and that is the point.

Colour: the warm side is nicotine amber, dishwater beige and rust; the cold side is bone
white and pale cyan. Let the two contaminate each other where they overlap. Rich and
filmic. Not desaturated, not monochrome.

Materials: walls painted a colour that was cream a decade ago, now smoke-stained, with one
lighter rectangle where something used to hang. Cheap scuffed vinyl or parquet floor with a
worn traffic path from bed to desk. Couch in a coarse brown-orange seventies weave, seat
cushions permanently compressed on one side only. Powder-coated bunk frame, chipped, one
thin blanket. Cheap desk with a softened front edge. Restrained storytelling clutter: a
small drift of unopened envelopes, a takeaway container, a mug ring, cables sagging in a
loop under the desk, a charger cable that does not quite reach the bed.

Mood: intimate, lonely, financially cornered. Lived-in and specific. Not abandoned, not
squalid, not horror, not a prison cell, not stylish poverty.

Do not: add people, a kitchen, or a refrigerator anywhere in frame. No luxury finishes, no
casino imagery, no posters with readable text, no brand marks, no watermark. Do not move,
resize, add or remove furniture. Do not narrow the central walking lane. Render the TV,
laptop and phone as plain neutral light panels with no interface, text or graphics on them.
```

---

## B — Vice Grip

**Thesis:** entrapment. The sportsbook has physically colonised the apartment.

```text
Use case: stylised game environment concept art
Asset type: first-person environment concept for a Unity indie roguelite

Input: the attached standing-overview capture is the edit target. Preserve its camera
position, focal length, perspective, room proportions, wall/ceiling/floor planes, window
position, and the placement and scale of every existing object: the bunk-bed-over-couch on
the left, the wall-mounted TV on the right, the small desk with open laptop and phone at
the far right, the low cube stool in the centre, and the clear central walking lane. The
two supporting images show the TV and desk compositions; use them only to keep that
geometry honest.

Primary request: repaint this exact room as "Vice Grip" — a chunky, tactile, high-pressure
box. The thesis is that the betting app has physically colonised the apartment: its violet
light has soaked into every surface until the room is no longer a home, it is an extension
of the machine.

Style: stylised low-poly 3D with heavy readable silhouettes, thick chamfered edges,
exaggerated wear exactly where hands and bodies touch, and dense contact shadows. Tactile
and physical — the grain of every surface should be legible. Original execution, not a
copy of any existing game.

Lighting: hard, close, and fast falloff. A failing fluorescent tube above the desk throwing
a sick green-yellow. Saturated violet-magenta bounce from the laptop washing the right wall
and the underside of the desk. The TV a cold white slab. Almost no ambient — objects fall
away to black within a metre of any source. Hard-edged pools of light, not soft gradients.

Colour: bruised violet, sick fluorescent yellow-green, sooty near-black, and one note of
dried-maroon in the couch fabric. Saturated and oppressive — deliberately uncomfortable,
while still reading as somewhere a person actually sleeps.

Materials: thick painted concrete or plaster with chunky trowel texture and patched
repairs. Heavy black conduit and cable bundles stapled along the walls and running to the
TV, sagging between fixings. A fat chunky TV bezel. Metal bunk frame with visible weld
lumps and chipped powder coat. Coarse matted couch fabric with one crude repair. Vinyl
floor worn into a hollow in front of the desk. Clutter with weight: a squared stack of
paper, a dead energy drink can, a full ashtray, a tangle of cable.

Mood: compressed, oppressive, tactile, claustrophobic — but unmistakably an apartment.
Not a prison, not a bunker, not supernatural horror. No bars, no gore, no occult marks.

Do not: add people, a kitchen, or a refrigerator anywhere in frame. No luxury finishes, no
casino or slot-machine imagery, no posters with readable text, no brand marks, no
watermark. Do not move, resize, add or remove furniture. Do not narrow the central walking
lane. Render the TV, laptop and phone as plain neutral light panels with no interface, text
or graphics on them.
```

---

## C — Low-Res Nocturne

**Thesis:** dissociation. He has stared at the board so long that reality renders in its format.
*(v1 called this "Pixel Night." The pixel direction only sings if the palette is deliberately limited, so this version commits to a fixed ramp.)*

```text
Use case: stylised game environment concept art
Asset type: first-person environment concept for a Unity indie roguelite

Input: the attached standing-overview capture is the edit target. Preserve its camera
position, focal length, perspective, room proportions, wall/ceiling/floor planes, window
position, and the placement and scale of every existing object: the bunk-bed-over-couch on
the left, the wall-mounted TV on the right, the small desk with open laptop and phone at
the far right, the low cube stool in the centre, and the clear central walking lane. The
two supporting images show the TV and desk compositions; use them only to keep that
geometry honest.

Primary request: repaint this exact room as "Low-Res Nocturne" — the same apartment
rendered as a deliberate low-resolution 3D / pixel hybrid, as if reality has quantised into
the machine's own format.

Style: PSX / early-3D inspired. Chunky simplified geometry with visible facets. Every
environment texture at a consistent, obviously low texel density with hard nearest-neighbour
pixel edges — a pixel should be the same physical size on the floor, the walls and the
furniture. Posterised lighting in visible stepped bands rather than smooth gradients, with
restrained ordered dithering only in the transitions.

Colour: a tight, deliberately limited palette of roughly sixteen values. Deep indigo and
violet-black base, two mid blues, and a small number of hot accents — magenta, sodium
amber, one cold cyan. Bold and graphic. Every colour should feel chosen from a fixed ramp
rather than sampled from reality.

Lighting: crisp chunky pools of screen light with hard posterised edges. The window is a
small bright rectangle of pixel city — a dark grid of tiny lit windows, a few of them
amber. Strong graphic separation between lit and unlit: the room should still read as a
striking silhouette composition when squinted at.

Materials: low-res worn wall pixels with visible dither in the gradients, dark laminate
floor in chunky bands with a scuffed path, couch fabric as clustered pixel noise, simple
hard highlights on the bunk metal. Sparse pixel-scale storytelling: a few taped papers on
the wall, a small stack of bills on the desk, a cable in blocky segments.

Critical: the TV, laptop and phone screens must stay clean, crisp and smooth. Render them
as plain neutral light panels with no pixelation, no interface, no text. Only the
environment is low-resolution.

Mood: dissociated, nocturnal, graphic, faintly melancholy — stylish rather than grim. Not
horror, not cute, not a voxel toy world.

Do not: add people, a kitchen, or a refrigerator anywhere in frame. No luxury finishes, no
casino imagery, no posters with readable text, no brand marks, no watermark. Do not move,
resize, add or remove furniture. Do not narrow the central walking lane.
```

---

## What to look for when the samples come back

Rank on these, in order. The first one is the one that actually matters.

1. **Does the still read as *this game* and no other?** The v1 Direction A image failed this — it was a competent dark bedroom that could belong to any project.
2. **Does the room tell the money story without a screen?** Wear, clutter, deferred maintenance, the specific cheapness of the objects.
3. **Does the TV still win the frame?** It is the sweat surface. Nothing static should out-shout it.
4. **Is it buildable in Unity at indie scope?** Simple meshes, authored materials and decals doing the work — no dependency on a flattened AI image.
5. **Is the fridge absent from frame, and are the screens blank neutral panels?**

Save results to `artifacts/room-visual-pass/concepts-v2/` as `a-sodium-hour.png`,
`b-vice-grip.png`, `c-low-res-nocturne.png`.

---

# Blends — round 2

Allen's read on round 1: **A is right but too realistic. C is right but the purple is wrong.**
Colour follows Sodium Hour. Both blends below are stylised but explicitly **not pixel art**.

Two things changed for these prompts:

- **Purple is out project-wide.** The SureThing UI workstream has killed violet, and its
  replacement palette is not chosen yet. Both prompts therefore render the laptop as a neutral
  cold-white panel so the room does not pre-commit to a brand colour that is still open.
- **B needed a new thesis engine.** Its original idea was *the violet app has colonised the
  room*. With violet gone, the colonising agent becomes a failing fluorescent tube — same
  entrapment story, no dependency on a dead colour.

## The stylisation rule

Both prompts carry an identical block that does the heavy lifting. It is written to get
stylisation **without** the pixel look:

> Stylisation comes from exactly three things — a small number of flat colour planes, hard
> posterised steps in the lighting, and simplified low-detail geometry. It does NOT come from
> resolution.

The failure mode to check for in the results is a visible pixel or texel grid on the walls and
floor. If one appears, the model has ignored the rule; regenerate and put the stylisation rule
at the very top of the prompt.

---

## AC — Sodium Print

**Thesis:** loneliness, told as a screen-print. A's warm/cold light collision and A's palette,
C's flatness and posterised light, none of C's resolution.

```text
Use case: stylised game environment concept art
Asset type: first-person environment concept for a Unity indie roguelite

Input: the attached standing-overview capture is the edit target. Preserve its camera
position, focal length, perspective, room proportions, wall/ceiling/floor planes, window
position, and the placement and scale of every existing object: the bunk-bed-over-couch on
the left, the wall-mounted TV on the right, the small desk with open laptop and phone at the
far right, the low cube stool in the centre, and the clear central walking lane. The two
supporting images show the TV and desk compositions; use them only to keep that geometry
honest.

Primary request: repaint this exact room as "Sodium Print" — 3 AM in the apartment of a
sports bettor who is losing, rendered as a bold graphic illustration rather than a photoreal
render. The story is a collision of two lights: dirty sodium-orange streetlight pushing in
through the window from a world that is warm and awake without him, and the cold clinical
glow of his own screens holding him in place.

STYLISATION RULE — read this carefully. The stylisation comes from exactly three things: a
small number of flat colour planes, hard posterised steps in the lighting, and simplified
low-detail geometry. It does NOT come from resolution. Do not use pixel art, nearest-
neighbour texture filtering, visible texel or pixel grids, dither patterns, or a PSX /
retro-console look. Surfaces render smooth and clean. Think screen-print, risograph or
gouache illustration built in 3D — graphic and flat, but never pixelated.

Lighting: a low raking sodium key from the window laying long hard-edged bars of warm light
across the floor and up the left wall. Cold white screen light from the TV and laptop cutting
against it, with a crisp visible boundary where the two meet in the middle of the room. Light
falls off in two or three discrete posterised steps rather than a smooth gradient. Shadows
are large flat shapes that still hold a little detail. No overhead light — the ceiling
fixture is off, and that is the point.

Colour: a tight palette of roughly eight to ten flat values. Sodium orange and warm amber on
the lit side; nicotine cream and rust in the mid-tones; deep warm brown-black in shadow; bone
white with a touch of pale cyan reserved for the screens only. Absolutely no purple, violet,
magenta or indigo anywhere in the image.

Materials and forms: simplified confident shapes with clean silhouettes and a little chamfer
— reduced detail, not reduced resolution. Walls a flat warm off-cream gone dirty, with one
lighter rectangle where something used to hang. Floor in flat bands with a worn path from bed
to desk. Couch as a simple mass in coarse warm brown-orange, cushions compressed on one side
only. Simple metal bunk frame, one thin blanket. Sparse storytelling clutter read as clean
graphic shapes: a small drift of envelopes, a takeaway container, a mug ring, a cable sagging
in a loop under the desk.

Mood: lonely, warm-lit, financially cornered, quietly stylish. Lived-in and specific. Not
abandoned, not squalid, not horror, not a prison cell.

Do not: add people, a kitchen, or a refrigerator anywhere in frame. No luxury finishes, no
casino imagery, no posters with readable text, no brand marks, no watermark. Do not move,
resize, add or remove furniture. Do not narrow the central walking lane. Render the TV,
laptop and phone as plain neutral light panels with no interface, text or graphics on them.
```

---

## BC — Fluorescent Grip

**Thesis:** entrapment, told as a poster. B's compression and tactility, C's flatness and
posterised light, no violet and no pixels.

```text
Use case: stylised game environment concept art
Asset type: first-person environment concept for a Unity indie roguelite

Input: the attached standing-overview capture is the edit target. Preserve its camera
position, focal length, perspective, room proportions, wall/ceiling/floor planes, window
position, and the placement and scale of every existing object: the bunk-bed-over-couch on
the left, the wall-mounted TV on the right, the small desk with open laptop and phone at the
far right, the low cube stool in the centre, and the clear central walking lane. The two
supporting images show the TV and desk compositions; use them only to keep that geometry
honest.

Primary request: repaint this exact room as "Fluorescent Grip" — a compressed, tactile,
high-pressure box rendered as a bold graphic illustration rather than a photoreal render. The
thesis is entrapment: this is not a home, it is a machine the player is inside of, lit by a
failing fixture nobody is coming to replace.

STYLISATION RULE — read this carefully. The stylisation comes from exactly three things: a
small number of flat colour planes, hard posterised steps in the lighting, and simplified
low-detail geometry. It does NOT come from resolution. Do not use pixel art, nearest-
neighbour texture filtering, visible texel or pixel grids, dither patterns, or a PSX /
retro-console look. Surfaces render smooth and clean. Think screen-print or bold poster
illustration built in 3D — graphic and flat, but never pixelated.

Lighting: hard, close and unforgiving, falling off fast in two or three discrete posterised
steps. A failing fluorescent tube above the desk throwing sick green-yellow across the right
side of the room. The TV a cold bone-white slab. Almost no ambient — objects drop to
near-black within a metre of any source. Hard-edged pools of light with crisp boundaries, and
dense flat contact shadows anchoring every object to the floor.

Colour: a tight palette of roughly eight flat values. Sick fluorescent yellow-green dominant,
sooty near-black, cold bone white, one desaturated olive mid-tone, and a single hot signal red
used sparingly on one small object. Absolutely no purple, violet, magenta or indigo anywhere
in the image.

Materials and forms: heavy chunky masses with thick chamfered edges and strong readable
silhouettes — reduced detail, not reduced resolution. Exaggerated wear exactly where hands
and bodies touch. Thick painted concrete or plaster walls with patched repairs read as flat
shapes. Heavy black conduit and cable bundles stapled along the walls and running to the TV,
sagging between fixings. A fat blocky TV bezel. Metal bunk frame with chipped coating. Coarse
matted couch fabric with one crude repair. Floor worn into a hollow in front of the desk.
Clutter with weight, read as clean graphic shapes: a squared stack of paper, a dead can, a
full ashtray, a tangle of cable.

Mood: compressed, oppressive, tactile, claustrophobic — but unmistakably an apartment someone
sleeps in. Not a prison, not a bunker, not supernatural horror. No bars, no gore, no occult
marks.

Do not: add people, a kitchen, or a refrigerator anywhere in frame. No luxury finishes, no
casino imagery, no posters with readable text, no brand marks, no watermark. Do not move,
resize, add or remove furniture. Do not narrow the central walking lane. Render the TV,
laptop and phone as plain neutral light panels with no interface, text or graphics on them.
```

---

## Judging the blends

The five criteria above still apply. Two additions specific to this round:

6. **No pixel grid.** Check the walls and floor at full size for texel steps or dither.
7. **No purple.** Any violet, magenta or indigo means the model leaked round 1's palette.

Save results to `artifacts/room-visual-pass/concepts-v2/` as `ac-sodium-print.png` and
`bc-fluorescent-grip.png`.

---

# B2 — Vice Grip, stylised (round 3)

**Allen's ranking, round 2:** B, A, C, AC, BC. **B wins.** Two notes carried forward: it should
not be this photoreal, and CloverPit-style stylisation would fit better as inspiration. The
blends ranked last because of their texture.

## What the generated B got right — preserve these

- The **cable and conduit routing** is the strongest element in the image. Runs along the
  ceiling, down the walls, stapled at intervals, sagging between fixings, feeding the TV.
  Keep it and keep it prominent.
- The compression and the low ceiling.
- The failing fluorescent as a real practical fixture with a visible housing.
- The chunky TV bezel and the heavy metal bunk frame over the couch.

## What to fix

1. **Too photoreal.** See the stylisation rule below.
2. **The window died.** In the generated image it reads as a dark boarded recess with no light.
   The real room has a lit window pane on the far wall and it is the only connection to outside.
   It must glow.
3. **It drifted institutional.** Wet concrete floor plus peeling plaster plus a dead window
   reads bunker, not apartment. Stylisation will pull it back, but the prompt re-asserts it.

## The stylisation correction

Round 2's blends stylised by *flattening* — screen-print, poster, flat colour planes. Allen
rejected that texture. The correct axis is the opposite:

> **Keep the rich grimy surface texture. Stylise through form, not through flatness.**
> Exaggerate proportions, thicken edges, simplify detail into bold chunky shapes, push the
> lighting saturated and high-contrast. Remove photographic micro-detail, not material richness.

---

## The prompt

Run this with the three baseline PNGs attached. Pick one PALETTE block — see below.

```text
Use case: stylised game environment concept art
Asset type: first-person environment concept for a Unity indie roguelite

Input: the attached standing-overview capture is the edit target. Preserve its camera
position, focal length, perspective, room proportions, wall/ceiling/floor planes, window
position, and the placement and scale of every existing object: the bunk-bed-over-couch on
the left, the wall-mounted TV on the right, the small desk with open laptop and phone at the
far right, the low cube stool in the centre, and the clear central walking lane. The two
supporting images show the TV and desk compositions; use them only to keep that geometry
honest.

Primary request: repaint this exact room as "Vice Grip" — a compressed, tactile, high-pressure
box where a sports bettor is losing at 3 AM. The thesis is entrapment: this is not a home, it
is a machine he is inside of, lit by a failing fixture nobody is coming to replace.

STYLISATION RULE — read this carefully, it is the most important instruction here. This must
NOT be photoreal. Stylise through FORM, not through flatness. Exaggerate proportions so
objects read slightly too heavy and too thick. Thicken every edge and give it a visible
chamfer. Simplify fine detail into bold chunky shapes with strong readable silhouettes. Push
the lighting saturated and high-contrast. Keep the surface texture rich, grimy and tactile —
do NOT flatten the image into poster art, screen-print, flat colour planes or cel shading, and
do NOT use pixel art or visible texel grids. Remove photographic micro-detail, not material
richness. The result should look like a hand-built game environment with weight and grain to
it: chunky, saturated, slightly diorama-like, deliberately not a photograph.

Tone reference: take broad inspiration from the compact, grimy, oppressive first-person room
feel of the indie game CloverPit — its heaviness, its saturated practical lighting, its
tactile chunk. Do not reproduce its props, its slot machine, its branding, its layout or its
palette. This is an original apartment, not that room.

Lighting: hard, close and unforgiving with fast falloff. A failing fluorescent tube in a
visible metal housing above the desk, flickering-cold, throwing its colour across the right
wall and ceiling. The TV a bright slab. Almost no ambient fill — objects drop away toward
black within a metre of any source. Hard-edged pools of light and dense contact shadows
anchoring every object to the floor. The window on the far wall glows: it is the only
connection to outside and must read clearly as a lit window, not a boarded panel — a dim
rectangle of night city beyond dirty glass.

[INSERT PALETTE BLOCK HERE]

Materials and forms: heavy chunky masses with thick chamfered edges. Exaggerated wear exactly
where hands and bodies touch — the couch arm, the desk edge, the bunk ladder, the light
switch. Painted plaster walls with patched repairs and one taped fix, grimy but not
catastrophically decayed. Heavy black conduit and cable bundles stapled along the ceiling and
walls, sagging between fixings, running down to the TV and the desk — make this prominent, it
is the signature detail. A fat blocky TV bezel. Metal bunk frame with chipped coating over a
low couch in coarse matted fabric with one crude repair. A cheap desk with a softened front
edge. Dry worn floor with a hollow tracked in front of the desk. Clutter with weight, few
pieces but heavy ones: a squared stack of paper, a dead can, a full ashtray, a tangle of
cable.

Mood: compressed, oppressive, tactile, claustrophobic — but unmistakably an apartment someone
sleeps in every night. Not a prison, not a bunker, not a basement, not supernatural horror.
No bars, no gore, no occult marks, no standing water, no flooding, no catastrophic decay.

Do not: add people, a kitchen, or a refrigerator anywhere in frame. No luxury finishes, no
casino imagery, no slot machines, no posters with readable text, no brand marks, no
watermark. Do not move, resize, add or remove furniture. Do not narrow the central walking
lane. Render the TV, laptop and phone as plain neutral light panels with no interface, text
or graphics on them.
```

---

## Palette blocks — pick one

### PALETTE 1 — Sick Fluorescent (recommended, no purple)

```text
Colour: the failing fluorescent tube is the colonising light — a sick yellow-green washing the
right wall and ceiling. The TV and laptop are cold clinical bone-white. Everything else falls
into sooty near-black and desaturated olive-grey. One hot signal red used sparingly on a
single small object. No purple, violet, magenta or indigo anywhere in the image.
```

**Why recommended:** the violet in your generated B came from the laptop, and the laptop is
SureThing — whose redesign has killed purple and has not yet chosen a replacement. A cold-white
laptop keeps the room from pre-committing to a brand colour that is still open, and the sick
green against clinical white is a nastier contrast than green against violet.

### PALETTE 2 — Keep the violet

```text
Colour: two colonising lights fighting. Saturated violet-magenta from the laptop washing the
right wall and the underside of the desk; sick yellow-green from the failing fluorescent above
it. The TV a cold bone-white slab. Everything else sooty near-black with a bruised violet cast.
One hot signal red used sparingly on a single small object.
```

**Cost of this choice:** it locks the room to a violet laptop, which contradicts the SureThing
workstream's decision to drop purple. If SureThing later lands on a different accent, the room's
key light and the app disagree.

---

## Judging B2

Criteria 1–5 above still apply. Four specific to this round:

6. **Not photoreal** — does it read as a built game environment rather than a photograph?
7. **Texture retained** — grimy and tactile, not flattened into poster art?
8. **Window glowing** — is it clearly a lit window to a night city?
9. **Still an apartment** — no bunker, no standing water, no prison read?

Save to `artifacts/room-visual-pass/concepts-v2/` as `b2-vice-grip-stylised.png`.
