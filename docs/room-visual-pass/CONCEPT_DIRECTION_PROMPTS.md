# Room Visual Pass — Concept Prompt Set

**Mode:** built-in image generation, using the current Unity captures as local image inputs  
**Purpose:** review-only environment concept references; not production room art  
**Shared input roles:**

- Image 1: edit target — exact standing overview composition.
- Image 2: supporting structural reference — exact seated TV/couch composition.
- Image 3: supporting structural reference — exact focused laptop/desk composition.

The output must transform Image 1 only. Images 2 and 3 clarify hidden geometry, prop identity, and screen-readability constraints.

Shared floor-plan correction used in the final generations:

- preserve the existing low cube/stool ahead of the player;
- the mini-fridge remains at world `(-0.95, 0.425, -1.65)` near the entry-left corner and is outside the standing image’s visible frame;
- do not invent or relocate a visible refrigerator beside the desk or TV;
- render screen contents only as neutral cyan/white placeholders with no generated text or UI.

## Direction A — Blue-Hour Pressure

```text
Use case: stylized-concept
Asset type: game environment concept art for a Unity indie production
Input images: Image 1: edit target, exact standing overview; Image 2: supporting structural reference, exact seated TV/couch view; Image 3: supporting structural reference, exact focused laptop/desk view
Primary request: visually develop the existing compact first-person apartment of a financially pressured sports bettor at night, applying Direction A “Blue-Hour Pressure” to Image 1
Scene/backdrop: Tokyo-sized minimalist room at night; bunk bed over couch; TV opposite couch; small desk with laptop and phone; mini fridge; window beside the TV zone
Style/medium: stylized 3D game-environment concept, believable PBR-lite materials, production-feasible indie scope
Composition/framing: preserve Image 1’s camera, perspective, field of view, room dimensions, wall openings, furniture geometry, prop placement, and central walkable path exactly; use Images 2 and 3 only to preserve the TV and laptop compositions
Lighting/mood: deep blue-black night, controlled screen glow, slightly dingy but believable, intimate rather than horror
Color palette: near-black blues and charcoal; dim cyan/white screen chrome; faded gray-brown fabric; green, red, and gold absent from static room art and reserved for monetary events
Materials/textures: worn painted wall, inexpensive dark laminate, old woven couch fabric, powder-coated bunk metal, fingerprints, dust, repaired patches, restrained bills and cable clutter
Constraints: no people; no kitchen; no luxury apartment; no casino interior; no copied game or sportsbook branding; no readable text; no watermark; do not redesign functional screens; do not move, add, or remove major furniture; keep screen surfaces unobstructed; preserve all walkable paths and screen readability
Avoid: horror-cell imagery, gore, occult motifs, trash piles, bright decorative accents, glossy luxury finishes, flattened matte painting look
```

The first Direction A generation misplaced the mini-fridge under the TV/desk and was rejected. The retained Direction A image received this surgical correction edit:

```text
Edit Image 1 only. Image 2 is the exact Unity layout reference.
Primary correction: remove the gray mini refrigerator/appliance incorrectly placed under and beside the desk beneath the TV in Image 1. That mini fridge belongs near the entry at the near-left side outside this camera frame, exactly as in the real Unity floor plan, so do not add a fridge anywhere else in the visible image.
Reconstruct the newly exposed area as the same dark painted wall, baseboard, floor, desk legs, subtle cable shadow, and open clearance seen around it. Preserve the existing TV, desk, laptop, phone, center cube/stool, window, bunk-over-couch, camera, perspective, lighting, materials, wear, and every other design decision of Image 1 as closely as possible. Do not move or resize any furniture or screen. Keep the central and right-side walkable paths clear.
No people, no text, no logo, no watermark, no new objects. This is a surgical layout-correction edit, not a redesign.
```

## Direction B — Tactile Pressure Box

```text
Use case: stylized-concept
Asset type: game environment concept art for a Unity indie production
Input images: Image 1: edit target, exact standing overview; Image 2: supporting structural reference, exact seated TV/couch view; Image 3: supporting structural reference, exact focused laptop/desk view
Primary request: visually develop the same compact apartment using an original tactile retro-indie “pressure box” direction: chunky readable forms, compressed atmosphere, rough material breakup, and screen-led economic pressure
Scene/backdrop: Tokyo-sized minimalist room at night; bunk bed over couch; TV opposite couch; small desk with laptop and phone; mini fridge; window beside the TV zone
Style/medium: stylized low-poly 3D environment concept; tactile grime; thick silhouettes; retro first-person indie atmosphere; original execution rather than a copy of any existing game
Composition/framing: preserve Image 1’s camera, perspective, field of view, room dimensions, wall openings, furniture geometry, prop placement, and central walkable path exactly; use Images 2 and 3 only to preserve the TV and laptop compositions
Lighting/mood: high-contrast screen pools with fast falloff; financially oppressive but still recognizably lived-in and intimate, not supernatural horror
Color palette: sooty blue-black, cold gray, faded neutral fabric, dim cyan/white; no static green, red, or gold
Materials/textures: chunky painted surfaces, exaggerated worn edges, thick bezels, powder-coated metal, rough old fabric, cable runs, wall repairs, restrained paper clutter
Constraints: no people; no kitchen; no luxury; no casino interior; no slot-machine imagery; no copied game motifs, assets, branding, or palette; no readable text; no watermark; do not redesign functional screens; do not move, add, or remove major furniture; keep screens unobstructed and the center aisle clear
Avoid: prison bars, occult symbols, gore, demonic imagery, extreme decay, visual noise around TV or laptop, realistic photorealism
```

## Direction C — Pixel Night

```text
Use case: stylized-concept
Asset type: game environment concept art for a Unity indie production
Input images: Image 1: edit target, exact standing overview; Image 2: supporting structural reference, exact seated TV/couch view; Image 3: supporting structural reference, exact focused laptop/desk view
Primary request: visually develop the same compact apartment as a coherent low-resolution 3D / pixel-art hybrid called “Pixel Night”
Scene/backdrop: Tokyo-sized minimalist room at night; bunk bed over couch; TV opposite couch; small desk with laptop and phone; mini fridge; window beside the TV zone
Style/medium: PSX-era-inspired low-resolution 3D environment, chunky geometry, consistent texel density, nearest-filtered pixel textures, restrained dithering and posterized shadows, production-feasible Unity treatment
Composition/framing: preserve Image 1’s camera, perspective, field of view, room dimensions, wall openings, furniture geometry, prop placement, and central walkable path exactly; use Images 2 and 3 only to preserve the TV and laptop compositions
Lighting/mood: deep pixel-blue night, crisp pools of screen light, slightly dingy and intimate rather than horror
Color palette: near-black navy, charcoal, desaturated cool neutrals, dim cyan/white; no static green, red, or gold
Materials/textures: low-resolution worn wall pixels, dark laminate bands, old couch fabric clusters, simple metal highlights, sparse pixel-scale stains, bills, and cables
Constraints: environment styling only; functional TV, laptop, and phone UI remains visually crisp and code-native rather than pixelated; no people; no kitchen; no luxury; no casino; no copied branding; no readable text; no watermark; do not move, add, or remove major furniture; keep screens unobstructed and preserve the walkable path
Avoid: full-screen pixelation over functional UI, unstable noisy dithering, tiny unreadable props, bright decorative patterns, 2D side-view composition, flattened matte painting look
```
