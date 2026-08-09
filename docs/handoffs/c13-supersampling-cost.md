# C13 — samples per pixel: what it changes, what it costs, who pays

**From:** SureThing UI lead · 2026-08-09 · scoping only, nothing built
**For:** Allen's call, via the orchestrator.

---

## 1. The shape of the evidence

A **fixed ~1.68px screen-space ramp**, constant across glyph sizes, **present at 1:1 with no
magnification**, on everything the canvas draws — bitmaps simply cannot witness it, because the ink
sprites are hand-drawn soft-edged strokes measuring **6.50px** of ramp against a glyph's 2.92px.

**A single-sampled rasteriser cannot produce an edge narrower than ~1px.** Measured ramps are
1.0–2.0px. The surface is already within 1–2× of the floor, and the only thing that beats a
sampling floor is more samples.

## 2. Every material-level lever is dead, and one argument covers all of them

`TMP_SDF.shader:186` — `scale *= abs(texcoord0.w) * _GradientScale * (_Sharpness + 1)`.

**`_Sharpness`, `_GradientScale`, atlas padding and sampling point size all feed the same `scale`
term.** I doubled that term directly (`_Sharpness` 0 → 1, measured on three generated arms) and the
ramp moved **×0.88–1.06** where the algebra predicts ×0.5.

**If doubling `scale` does not halve the ramp, `scale` is not the limiter — and no other lever on
`scale` can be either.** That retires sampling point size (90pt) without a further experiment; it was
still on room's list and it is the same term.

## 3. Options

### A — URP render scale > 1 (supersample the whole frame)

`PC_RPAsset` is live at **`m_RenderScale: 1`, MSAA off**. Raising it renders everything larger and
downsamples, which puts real samples behind every edge in the game.

| render scale | pixels rendered | ~ramp in final image |
|---|---|---|
| 1.0 (today) | ×1 | 1.68px |
| 1.5 | **×2.25** | ~1.12px |
| 2.0 | **×4.0** | ~0.84px |

- **Fixes it, and fixes it everywhere** — the room's own geometry edges gain too.
- **The room pays the entire cost**, for a defect that only the UI exhibits. The room carries bloom,
  APV indirect light and the unified grade; those all scale with pixel count.
- **It touches every ruled room measurement.** Grades, bloom thresholds and the T41/T48/T49 ladder
  were all measured at render scale 1. **This is a re-baseline of the room, not a setting.**
- Cheapest to implement — one value — and by far the most expensive to verify.

### B — Canvas → RenderTexture on the lid (the UI pays alone)

Render the laptop canvas into a 2048×1408 RT and sample it on the lid mesh.

- **Cost is confined to the UI.** ~11.5MB for the target, plus a canvas render pass.
- **It re-architects the surface**, and the input path is the real bill: the laptop's UI runs through
  the world-space canvas's own `GraphicRaycaster`. Under an RT that stops working — every control
  needs the physics hit's UV mapped back into canvas space and pointer events synthesised.
  **That is PLACE TICKET, LOCK IT IN, the offer rows, S27's scroll rail, and the ScrollRect that
  already needed special handling twice** (`LaptopOs.cs:1088`, `SportsbookApp.cs:545`).
- **It also changes how the lid meets the grade.** The laptop is currently world geometry lit and
  graded with the room; as a textured quad it becomes a different kind of object, and R40 is already
  live on that material.
- **It makes C13's original premise true after the fact** — the laptop would finally *be* a content
  package, having spent this whole item establishing that it is not one.

### C — Accept ~1.68px

- Free. The surface is within 1–2× of the sampling floor and no ruled item is violated: **L2's fact
  floor was amended precisely because 12px and 13px both render 9px of ink at this resolution.**
- The cost is that the laptop reads softer than the room's own hard edges, which is what Allen saw.

## 4. My recommendation, and it is mine rather than measured

**A at 1.5, or C.** Not B.

B buys a smaller blast radius on paper and spends it on the input path, an architecture change and a
new interaction between the lid and the grade — three risks against one benefit, on a surface whose
chrome is Design-verified and whose controls are individually ruled.

Between A and C, the honest position is that **A is a room-wide re-baseline bought for a UI-only
symptom**, and I cannot tell you it is worth it — that is exactly the judgement Allen's eye is for. If
it goes ahead, **1.5 reaches the floor and 2.0 spends ×4 to go below a floor nobody can see past.**

## 5. What this scoping cannot tell you (C25)

- **I have not measured GPU cost.** The multipliers above are pixel counts, not frame times; the
  room's bloom and APV may scale worse than linearly. **Room owns that measurement and it should
  exist before the call, not after.**
- **I have not verified that render scale actually narrows the ramp**, only that it is the one
  mechanism whose shape fits. It is one value and one capture to prove — cheaper than this document.
- **The ~1px floor is theory plus four measurements at 1:1**, not a swept curve.
- **Whether ~1.68px is even wrong is not a technical question.** Every material candidate died; what
  is left is a choice about how a screen should read in a room, which is the DD's and Allen's.
