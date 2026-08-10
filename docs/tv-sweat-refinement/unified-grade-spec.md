# Unified grade — making the TV and the room one image

**Status:** specification only. **Implementing this crosses this worktree's file boundaries — see §6.**
**Date:** 2026-07-26 · **Requested by:** Allen · **Referenced by:** `DESIGN.md` §2A

---

## 1. The problem, precisely

The room is painterly: textured, decayed, with real light falloff, dust, and material response. The TV
sweat is a vector-crisp LED matrix on pure black. Both are good. Together they read as two assets in
one frame rather than one game.

This is not a design-language mismatch — the language was accepted. It is a **medium** mismatch. The
two surfaces do not share a camera, a lens, a sensor, or an atmosphere, so the brain files them as
separate images.

The fix is to put them through one camera.

## 2. The single highest-leverage change

**Lift the TV's blacks and put air between the camera and the screen.**

Pure `#000000` on a panel in a dim, dusty room is physically impossible. A real screen catches ambient
light from the fluorescent, the panel's own scattered emission, and the dust on its glass. The air
between the couch and the wall scatters the TV's own blue and magenta back toward the camera.

A screen whose blacks are darker than every shadow in the room is the single clearest signal that it
was composited rather than photographed. Everything else in this spec is refinement; this is the fix.

Two parts:

1. **Raise the black floor** so the TV's unlit pixels land at or just above the room's darkest
   shadow — never below it. Nothing in frame should be darker than the panel's off state.
2. **Add atmospheric haze** between camera and TV, so its emission has something to travel through.
   In a room this filthy, that haze is justified in-fiction.

Do these two and the pasted-on feeling largely resolves. The rest of the stack makes it good.

## 3. The stack

One volume, global, covering room and TV together. **The TV is inside the pass, never exempt from it.**
Ordered as they should be reasoned about, not necessarily as URP evaluates them.

| # | Effect | Starting point | Purpose |
|---|---|---|---|
| 1 | **Tonemapping** | Neutral — **not ACES** by default | Shared response curve. See §4, this is a real decision |
| 2 | **Shadows/Midtones/Highlights** | Shadows lifted so screen black ≈ `#0a0c10`, not `#000000` | The main fix. Nothing in frame darker than the panel's off state |
| 3 | **Atmospheric fog** | Exponential, very low density, tinted toward the room's olive | Air between couch and screen. Also gives TV light something to bleed into |
| 4 | **Bloom** | threshold ~0.9, intensity ~0.7, scatter ~0.7 | TV emission continues past the bezel instead of stopping at it |
| 5 | **Film grain** | intensity ~0.20, response ~0.7 | **The strongest single unifier.** One grain over both makes them share a sensor |
| 6 | **Chromatic aberration** | ~0.08 | Shared lens. Subtle — visible at edges only |
| 7 | **Vignette** | intensity ~0.30, smoothness ~0.40 | Shared framing; also pushes the eye toward the TV |
| 8 | **Colour adjustments** | slight desaturation of the room, none of the TV | Optional, and only if the TV still reads too separate after 1–7 |

Every value above is a **starting point for on-screen tuning, not a tested result.** Nothing here has
been rendered — this harness runs Unity with `-nographics` and cannot rasterise a frame. Treat these
as where to begin, not where to land.

## 4. Two decisions that need a real screen

**Tonemapping: ACES will fight this palette.** ACES desaturates and rolls off saturated primaries
hard, and the entire brand book rests on saturated electric blue, hot magenta, and gold. There is a
real risk ACES turns the LED board muddy while making the room look great. Start with **Neutral**,
try ACES deliberately, and compare — do not inherit whichever the project already had.

**The TV must emit above 1.0.** For bloom to treat the screen as a light source rather than a bright
texture, its emissive material needs HDR values greater than 1.0, with the `L4` full-brightness tier
pushed highest. If the TV renders as an unlit material clamped at 1.0, effects 3, 4, and 5 will all
appear to do nothing to it and the screen will stay conspicuously flat. **Check this first** — if the
TV canvas is unlit and clamped, no amount of volume tuning will fix the problem.

## 5. What must survive the grade

The grade may not break what the design depends on:

- **The brightness ladder must still read.** `DESIGN.md` §3 has five levels and at most one L4
  element. If lifted blacks and grain compress L0 against L1, the dead-leg treatment — which Allen
  confirmed works — stops working. Verify `LOST` still reads as dead after grading.
- **`L0` extinguished must stay clearly darker than `L1` dormant.** Lift the floor; do not flatten it.
- **Gold must stay the only warm hue.** A grade that warms the whole image kills the contrast gold
  depends on.
- **Text must stay legible at couch distance.** Grain and chromatic aberration both attack small type
  first. If the `NEED` line degrades, back both off — legibility outranks integration.

## 6. Decision gate — this crosses the boundary

Implementing this requires:

- a global volume placed in the scene → **`Room.unity`**, PRD §11 forbidden;
- possibly URP asset or renderer changes → outside this worktree's ownership;
- possibly the TV canvas material → depends on how the world-space canvas is set up.

Per the ownership contract, an agent on this slice does not cross into those files. **This is raised
as a decision gate, not actioned.**

Recommended: the room/environment owner implements it, since the volume lives in their scene and they
own the lighting rig already being briefed for the TV's blue/magenta spill
(`room-artist-brief.md`). This slice supplies the spec and reviews the result against §5.

## 7. How to verify

Not by looking at the TV. By looking at the room.

1. Render the seated camera view with the sweat running, graded and ungraded.
2. Cover the TV in both. **Do the two rooms look like the same room?** If the grade is doing its job,
   the room changes too.
3. Uncover it. Does the screen sit *in* the space, or *on* it?
4. Re-run the §5 checks — dead leg still dead, one L4 still dominant, gold still the only warmth,
   `NEED` line still legible from the couch.
5. Then the couch-readability review in `VISUAL-DESIGN.md` §12, since the grade can only reduce
   legibility, never improve it.
