# Phase A — full PBR maps: what shipped, and why relief still does not read

**Date:** 2026-07-29 · **Scope:** emission bug fix + albedo/normal/mask/occlusion maps
**Outcome:** maps shipped and verified correct. The visual payoff did **not** land, for a reason
that is measured rather than guessed, and that is worth not re-deriving.

---

## 1. The emission bug — fixed, and it was silently breaking the room

`GrayboxRoomBuilder.Mat()` set the `_EMISSION` keyword and then set
`globalIlluminationFlags = MaterialGlobalIlluminationFlags.None` on the next line. URP's
`MaterialPostprocessor` recomputes that keyword from exactly that field on **every material
import**:

```
BaseShaderGUI.cs:946   shouldEmissionBeEnabled = (flags & AnyEmissive) != 0
BaseShaderGUI.cs:953   CoreUtils.SetKeyword(material, _EMISSION, shouldEmissionBeEnabled)
```

`None` carries no `AnyEmissive` bit, so the postprocessor stripped the keyword the builder had
just set — killing emission on the TV, laptop, phone, window and indicator lamp the next time
anyone opened the editor. It presented as five materials mysteriously going dirty after a clean
commit.

Fixed by using `RealtimeEmissive`, which keeps the keyword through import and bakes nothing
(this project has no lightmaps). Verified surviving a full build plus three further Unity
launches — the exact sequence that used to strip it.

**Do not "tidy" that flag back to `None`.**

## 2. The maps are correct. Measured, not assumed.

Four maps per surface — albedo, normal, metallic/gloss, occlusion — all derived from one
deterministic height field, so they describe the same surface without needing to see each other.

Normal map channel variance (a flat map is R=128, G=128, sd≈0):

| Surface | R sd | G sd |
|---|---:|---:|
| Plaster | 23.6 | 26.9 |
| Worn floor | 17.2 | 17.5 |
| Ceiling stain | **8.5** | **9.8** |
| Fabric weave | **79.0** | **81.8** |

## 3. Why relief still does not read — and the evidence that settles it

**The ceiling carries the weakest map in the room and shows the most relief. The couch fabric
carries by far the strongest and shows none.** That inverts every explanation based on map
strength, and it matches the physics exactly.

Lambertian sensitivity to a normal perturbation scales with **sin θ**, where θ is the light's
incidence angle off the surface normal:

- **θ → 0** (light perpendicular to the surface): sin θ → 0. Perturbing the normal changes nothing.
- **θ → 90°** (grazing): sin θ → 1. Maximum sensitivity.

The fluorescent hangs 0.25 m under a 2.3 m ceiling, so its light travels almost parallel to it —
θ ≈ 85–90°, peak sensitivity. The floor is lit from above at θ ≈ 10–30°, near-zero sensitivity.
The couch sits in shadow, so its excellent map has no light to modulate at all.

**Surface detail cannot be seen without light that varies across it. That is a lighting-geometry
property, not a texture property.**

### Two hypotheses tested and rejected

1. **Contrast clipping** (real bug, fixed). The height field was being built from the
   contrast-boosted albedo; at contrast 2.10 large areas clip to pure black or white, and a
   clipped region has zero gradient, so the Sobel found nothing where fine grain lives. The
   height field now always builds at contrast 1.0. Correct fix, but not the main cause —
   contrast says how dirty a surface *looks*, not how bumpy it *is*.
2. **Missing environment specular** (rejected). A reflection probe was added on the theory that
   rough surfaces with nothing to reflect cannot show normals. In a room this dark a probe
   reflects darkness and contributed essentially nothing. Kept because it is harmless and will
   matter if the room ever brightens, but it did not earn its place.

### One attempt reverted for breaking a ratified requirement

A grazing right-wall wash worked geometrically — `N·L = d / √(d² + h²)`, so a 0.26 m offset gives
θ ≈ 72°: nearly all the sensitivity of a true graze with ~6× the brightness of hugging the wall at
45 mm. But that offset necessarily places the light out into the room, and the only wall available
to graze has the second bunk in front of it. It lit the mattress and broke the ratified
"legible as occupied, never legible as empty" treatment.

Reverted, along with the fluorescent nudge that had the same side effect. **A ratified
requirement outranks an effect that has failed to land four times.**

A dim couch-side graze was kept: it sits below the bunk-1 slab, so it lights the couch without
touching either bunk, and gives the room's strongest normal map at least some raking light.

## 4. What this means for closing the gap to the concept render

The earlier estimate — "~60% textures, ~15% lighting" — was **wrong**. They are not independent
contributions; lighting is the *gate* on textures being visible at all. The maps are now in place
and will pay off the moment the lighting gives them something to catch.

Remaining levers, in order:

1. **Indirect light** — Adaptive Probe Volumes (URP 17) or a bake. Real bounce means light
   arriving at surfaces from many directions, which is what makes relief read everywhere at once
   rather than only where a lamp happens to rake. This is the highest-value remaining change.
2. **More grazing sources on walls the bunks do not occupy.**
3. **Accept it.** A room lit by one overhead tube genuinely has a flat-looking floor. The concept
   render has light arriving from several grazing directions — low window, desk lamp, side
   sources — and that, not its textures, is why its surfaces read.

Do **not** respond to flat-looking surfaces by strengthening the normal maps. The measurements in
§2 and §3 show that lever is already exhausted.
