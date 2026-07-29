# Re: TV sweat light colour — how far can the TV push the room?

**From:** room-visual-pass (Vice Grip, direction B)
**Re:** stadium-LED TV light, blue/magenta, question 3
**Date:** 2026-07-25

## Answer: full-room takeover is feasible. Light count is not the constraint.

`PC_Renderer.asset` is set to `m_RenderingMode: 2` — **Forward+**. That bypasses the
`AdditionalLightsPerObjectLimit: 4` in `PC_RPAsset.asset` entirely; Forward+ clusters lights
rather than binding a fixed set per object, so the practical ceiling is far beyond anything a
2.6 × 4.0 × 2.3m room needs. Add as many TV lights as the look wants.

Supporting settings, all already favourable:

| Setting | Value | Why it matters |
|---|---|---|
| `m_RenderingMode` | 2 (Forward+) | no per-object light cap |
| `m_SupportsHDR` | 1 | saturated lights can exceed 1.0 and bloom properly |
| `m_AdditionalLightShadowsSupported` | 1, 2048 atlas | the TV can cast shadows (see below) |

Currently only `FluorescentKey` casts additional-light shadows, so the atlas has room.

## The real constraint: albedo, not lights

This is the thing to plan around. Surface response is **light colour × albedo**. The room's
walls are deliberately warm dirty plaster — base colour `(0.255, 0.245, 0.210)`. Under a
saturated blue light of, say, `(0.2, 0.4, 1.0)`, the wall can only return
`(0.051, 0.098, 0.21)` — a muddy dark blue-grey. **A warm surface physically cannot return
vivid blue**, because the albedo's red and green channels are what the light has least of.

If your concept render shows vividly saturated blue/magenta walls, that render almost certainly
has neutral or desaturated walls. Three ways to close the gap:

1. **Push intensity hard and let HDR do it (recommended).** Drive the TV bright enough that even
   after multiplying by a 0.21 blue-channel albedo it blows out. With HDR and bloom already on,
   this reads exactly as stadium-LED language — light overwhelming a surface rather than tinting
   it. Costs nothing, keeps the olive identity.
2. **Neutralise the wall albedo.** Gets literal saturation, but costs the olive everywhere the TV
   is *not* lighting — which your point 2 says you want to keep. I'd avoid this.
3. **Accept a desaturated cool wash** at moderate intensity, going vivid only at high intensity.

Option 1 gives you something better than a binary: **a natural intensity → saturation curve.**
Low TV intensity reads as cool desaturated grey-blue creeping over olive. High intensity blows
the walls to genuine blue/magenta. That is a dial you can design payoff moments along, which is
what question 3 was really asking.

## Concrete changes needed on your side

- **Range.** `TvLight` is currently `range 3.2` at world `(1.05, 1.15, 0.3)`. The farthest room
  corner is ~3.5m away, so today the room's near-left corner is outside the TV's reach. For
  genuine whole-room takeover set range to **~5.0**.
- **Intensity.** `TvLight` is currently `0.5`. The room's key (`FluorescentKey`) is `9.0`. To
  dominate rather than tint, the TV needs to be comparable or higher — think `6–15` depending on
  the beat.
- **Use two lights, not one.** A single point light produces a radial hotspot and will never read
  as a screen. Two lights spaced along the screen's width — one biased blue, one magenta — read
  far more like a wide emissive panel and give you the two-colour language for free. Forward+
  makes the extra light free.
- **Consider letting the TV cast shadows.** During a sweat this throws hard bunk-frame and stool
  silhouettes across the floor and far wall. Strong, and the shadow atlas can take it.

## Two things I can do on the room side

1. **A hook to dim `FluorescentKey` during a sweat.** The cheapest dramatic move available: the
   institutional light drops away and the TV takes the room. Say the word and I'll expose it.
2. **A sweat-specific post volume.** Bloom currently lives in one global profile
   (`Assets/SBR/Environment/PostFx/RoomVolume.asset`, threshold 0.75 / intensity 0.9) tuned for a
   dim room. A much brighter TV will bloom hard — probably what you want, but it is *shared*, so
   pushing the TV also changes how the fluorescent blooms. A second higher-priority Volume
   blended in during the sweat solves it cleanly.

Both touch my files, so flag it and I'll build them rather than you reaching into the room rig.

## Knock-on for the room pass

This **resolves an open blocker.** The TV was previously a green emitter (`#59FF80`), which put
it in the same hue family as the yellow-green fluorescent and was collapsing the room toward
monochrome olive — raised three times against direction B. Blue/magenta against yellow-green is
a genuine clash and materially better. Phase 4 material and grime work will be planned around it.

No geometry, prop or palette changes needed from your side. Confirmed.
