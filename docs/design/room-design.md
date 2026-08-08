# Room — Design

<!-- impeccable:design-schema 1 -->

**Scope:** the room. Geometry, light, material, atmosphere, dressing, and the grade every surface in
it renders through. It does not govern the TV panel's contents (`[TV] DESIGN.md`), the laptop's
contents (`[ST] direction-concepts/DESIGN.md`), or the phone.

**Status:** DRAFT for Allen's approval. First surface document under the two-tier art authority
approved as C9. Assembled from ratified material — it derives nothing new.

**Supersedes:** `design/08-art-direction.md` for the room, deprecated by Allen 2026-07-24. That world
— casino neon on black, CRT phosphor, green/red/gold purity — is an explicit anti-reference here as
it is on the TV. It also supersedes the **revoked** cool-blue and money-colour palette laws (R4,
revoked by Allen 2026-07-25); four repo documents still assert them and are wrong.

**Sources:** `SIGNOFF.md` (acceptance, 2026-07-28), `PRODUCT.md` §Operating Context (the palette law
and the light law, 2026-07-28), `PHASE_A_FINDINGS.md` (the sin θ result), `PHASE_B_INDIRECT_LIGHT.md`
(R6), `unified-grade-spec.md`, and the R5/R6/R7 review package (2026-07-31).

---

## Direction contract

**THESIS.** A cramped bunker at night in a **wealthy high-tech city that has no use for the
occupant.** The room is rotting; the city outside the window is neon and functioning; the screens are
the only things that work. **The machines are nicer than the life.** It refuses the arrangement this
genre always ships: a poor world. He is not in one. He is in a rich one, in the part of it nobody
maintains.

**OWN-WORLD.** Warm dirty plaster, olive and khaki and rust, damp concrete, exposed black conduit,
riveted steel, chipped institutional paint, two heavy bunk frames, a battered metal desk. One warm
dim fluorescent overhead; one cool window with short reach; three screens that emit and barely spill.
Rendering register: **painterly semi-realistic** — not stylised, not photoreal.

**STORY.** The occupant is at the desk at 2 a.m. with his own cheap laptop, watching an institution's
panel across the room. Nothing in the space was chosen by him except the machine he works on.

**FIRST VIEWPORT.** Standing 68°: bunks left, deep-set window centre onto a neon skyline, desk and
laptop right, the riveted display housing bolted into the right wall with conduit continuous with the
room's own pipe runs.

**FORM.** Direction B — *Vice Grip*, stylised, Palette 1. Approved by Allen 2026-07-28 after an
A/B/C round, accepted at 8/8 gates.

---

## 1. The laws

**1.1 The room physically cannot return saturated cool colour.** Wall albedo is warm dirty plaster,
linear `(0.255, 0.245, 0.210)`. **A blue-tinted room is the explicit failure mode**, and the current
graded captures fail it — they read cool-blue overall and must not be sampled for colour. Judge
colour against this document, not against the frames.

**1.2 Three light sources stay distinguishable.** A warm dim fluorescent; a cool window with **short
reach** that pools locally and does not tint the room; and the screens, which are **quiet, with faint
spill.** If a viewer cannot say which light is doing what, the rig has failed regardless of how the
frame reads.

**1.3 Nothing in frame is darker than a screen's off state.** Lifted blacks are a whole-image law, not
a screen law. The room's darkest shadow sits above the panel's black floor.

**1.4 Legible as occupied, never legible as empty.** Bunk 2's mattress is the ratified test, currently
**43.9 mean luminance**. Anything that lights it breaks a ratified requirement. Four attempts at
corner lighting have been reverted on exactly this.

**1.5 Nothing in the scene is hand-authored.** All art is generated, deterministic and idempotent.
A rebuild reproduces the room exactly.

**1.6 Collision is not art's to change.** 27 colliders throughout; dressing and wear add none.

**1.7 Surface detail is gated by lighting, not by texture authoring.** (R12, recorded from R5.)
Lambertian sensitivity to a normal perturbation scales with **sin θ**, so light arriving perpendicular
reveals no relief and grazing light reveals the most. The measured inversion proves it: the ceiling
stain has the *weakest* normal map (sd ≈ 9) and is the most visible surface in the room because the
fluorescent rakes it at θ ≈ 87°; the couch fabric has the *strongest* (sd ≈ 80) and is invisible
because it sits in shadow. **Asking for "more surface detail" will not produce it. Ask for light that
varies across a surface.**

---

## 2. Colour

Olive, khaki, drab green, rust, damp concrete, under a warm dim fluorescent. No saturated cool.

| Role | Value | Where |
|---|---|---|
| Plaster | `#8A887E` | wall albedo, converted from the linear spec value |
| Khaki | `#6E6A4E` | dressing, fabric |
| Concrete | `#55534C` | damp floor, sills |
| Olive | `#4A4A32` | shadowed wall |
| Drab green | `#3A4230` | bunk frames, mattress fabric |
| Rust | `#6B3A24` | corroded metal, radiator |
| Steel | `#3A3F42` | the riveted display housing |
| Conduit | `#22252A` | pipe runs, brackets |
| Fluorescent | `#D8C48A` | the warm dim key itself |
| Lit wall value | `#524D35` | what the key returns off plaster |
| Window | `#5679C2` | cool, short reach, pools locally |
| City | `#D8C68A` | neon and functioning, far away |
| Darkest shadow | `#0F1108` | still lighter than any screen's off state |

**Colour is per-surface (C4).** The TV keeps gold, SureThing keeps wax amber, green/red is retired
game-wide. The room does not arbitrate between them and must not tint toward either.

---

## 3. Light

The rig is the room's primary art tool — §1.7 makes it the *only* tool that produces surface detail.

- **Key:** one warm dim fluorescent, hung 0.25 m below the ceiling, raking it at θ ≈ 87°. This is why
  the ceiling reads and it is the room's best-lit surface. Do not "fix" it.
- **Window:** cool, short-throw, local pool. It must not reach across the room; §1.1 depends on it.
- **Screens:** emissive, quiet, faint spill. The TV's spill is currently green `#59FF80` against its
  own approved cold white-grey — **C2 interim: tolerated, corrected in TV Phase 3.** Do not compensate
  for it here; anything tuned against the green will be wrong twice.
- **Indirect:** six static lights are Mixed so their indirect contribution bakes into an Adaptive
  Probe Volume; direct light and shadow stay realtime, so every pool tuned during the approved pass is
  untouched. TV and phone stay fully realtime.

**Indirect light is the lever that works on this room.** R6 raised right-wall relief contrast ×6.3 and
whole-frame ×1.67 **with mean luminance held** (33.0→32.0, 38.6→38.4, 29.0→28.5). The room did not get
brighter; the texture became visible. Reach for bounce before reaching for another lamp.

**Open, approved with bounds:** R9 (ambient rebalance, ~30–40%, 8/8 gate re-run, mattress 43.9 ±1,
region means within 10%) and R10 (couch corner — **bounce first**, grazing source as fallback at
`y < 1.50`).

---

## 4. Material

Generated from one deterministic height field: albedo, normal, metallic/gloss, occlusion, for plaster,
worn floor, ceiling stain and fabric weave. World-scale UVs — 1 unit = 1 UV — so tiling is literally
repeats-per-metre and texel density is uniform for free.

Meshes are built at true world size with `localScale` 1. Scaling a unit cube stretches its bevel, so a
thin wall and a chunky post would carry visibly different edge widths.

**Localised wear is parked** at the committed Tier 1b state: skirting grime, radiator rust, window
condensation, floor traffic path, stool scuff, conduit drip — **1.92% of pixels changed against a
1.69% baseline, i.e. very nearly invisible.** Diagnosis: **placement versus camera**, not technique.
Wear was placed against physical causes without checking those causes against what the cameras see.

Two rulings stand on that:

- **Ceiling soot is dropped.** The ceiling is the one surface where relief already reads. A soot halo
  would flatten the read that works, on the room's most visible surface.
- **The URP Decal Renderer Feature is not justified yet.** Re-place existing wear against the three
  camera frusta first — cheap, no shared-renderer change. If wear that is genuinely in frame still
  under-reads, that is the evidence that buys the change across three worktrees.

---

## 5. Composition

Two-bunk layout, signed off 2026-07-28. Three review poses, fixed: **standing 68°**, **seated-at-TV
17°**, **focused-laptop 30°**. All Play Mode, identical exposure, comparable frame to frame.

The standing camera sits at y = 1.64 looking level, so **the floor only enters frame from z = +1.03**
and the couch/bunk assembly occludes half the far-wall skirting. Any dressing placed below or behind
those is invisible regardless of quality — this is the specific error R7 made.

Two of the three poses are narrow-FOV close-ups framed on emissive screens with almost no GI-lit
surface in frame, so they barely move under lighting work (R6: seated ×1.00, laptop ×1.04). **That is
expected. Do not treat a flat close-up as a failed lighting pass.**

---

## 6. Props and their register

**The split that everything else hangs from.** The TV is a hardened industrial display **installed by
an institution** — riveted steel frame, thick chipped paint, recessed glass, a stencilled equipment
code, one physical indicator lamp, conduit continuous with the room's own pipe runs. Making the
enclosure part of the building's construction is what finally seated the screen in the room; every
earlier concept read as a nice TV pasted onto a bad wall.

**The laptop is the opposite: his own machine** — personal, chosen, probably cheaper, probably
grubbier, possibly customised. It must never be dressed as institutional hardware.

Everything else in the room belongs to the institution, not to him: bunk frames, radiator, desk,
stool, conduit, fixtures. The occupant's presence is legible only in what he brought and what he wore
out — the laptop, the ashtray of butts, the traffic path.

---

## 7. Atmosphere — the unified grade

**One global post-process pass over the room and every screen in it.** One grade, one grain, one bloom
curve, one vignette, one chromatic aberration budget. A surface graded with the room belongs to the
room, whatever its design language. The room owns the volume.

Order to reason about, with starting points that settle on screen: neutral tonemapping (**not ACES by
default** — it desaturates saturated primaries and will fight the panel); shadows lifted so screen
black lands near `#0a0c10`; very low **ExponentialSquared** fog (density 0.085, the built and
design-verified curve — doc corrected per R27/C23; every verified frame R5/R6/R9/R10/R15 contains
it) tinted toward olive; bloom threshold ~0.9,
intensity ~0.7; **film grain ~0.20 — the strongest single unifier**; chromatic aberration ~0.08;
vignette ~0.30.

Two corrections already paid for, recorded so they are not re-derived:
- Lifting blacks belongs in **`LiftGammaGain`**, not Shadows/Midtones/Highlights — that component
  multiplies, and any multiplier times pure black is still pure black.
- **URP scales lift roughly 7× harder than the raw value implies.** The spec's starting value produced
  a flat mid-grey panel that failed the spec's own ladder checks.

**What the grade may not break:** the TV's five-level brightness ladder must still read; `L0` must stay
clearly darker than `L1`; gold must stay the only warm hue on the panel; and text must stay legible at
couch distance. **Legibility outranks integration** — if grain or aberration degrades the `NEED` line,
back both off.

**How to verify: by looking at the room, not the TV.** Render the seated view graded and ungraded,
cover the TV in both, and ask whether the two rooms look like the same room. If the grade is working,
the room changes too.

---

## 8. Do's and don'ts

**Do** reach for light before texture. **Do** hold the three sources distinguishable. **Do** keep the
mattress test on every lighting change. **Do** place dressing against the camera frusta, not only
against physical causes. **Do** state relief claims as measured contrast, not as impressions.

**Don't** tint the room cool. **Don't** light bunk 2. **Don't** hand-author art. **Don't** add a lamp
where bounce would do. **Don't** compensate for the TV's temporary green. **Don't** chase the concept
render's multi-directional light — the room has one overhead tube and three local sources, and
inventing light the construction does not have is how a painterly room becomes a lie. **Don't** trust
the graded captures for colour.

---

## 9. Open

| # | Item | State |
|---|---|---|
| R7 | Localised wear | Parked at Tier 1b; re-place against frusta before any technique change |
| R8 | Geometry detail | Approved direction, not started; waits on the R9/R10 re-gate |
| R9 | Ambient rebalance | Approved with bounds |
| R10 | Couch corner | Approved, bounce first |
| C2 | TV light spill colour | Interim — green tolerated, cold white-grey at TV Phase 3 |
| C5 | Room re-tint from TV light in-engine | Open deliberately; if the rig supports it, big payoffs drive it |
| C7 | Four documents still assert the revoked palette laws | Documentation debt |

---

## Amendment — 2026-08-08 (batch 15, transcribed by the orchestrator)

**R39 closed — exact values granted on the adopted emission instrument.** The
phone's isolated contribution reads 85.4°/chroma 5.0 against the laptop's
84.3°/5.3: one chromaticity family in render, held **by construction** — all
three phone states are `Amp(1/3/15)` off `LaptopScreen.GrantedLidEmission`.
**R39-am:** the "these are observable" line is struck (the phone's canvas sits
1.5mm over the emissive quad, the lid's arrangement); owed an in-Play A/B with
the disposition **pre-committed** — if unobservable at runtime, the granted
colours stand and no cue is ever built on the phone's glow.

**R40 closed** — the material carries the granted value from the shared
constant. **R40-am:** the DD's bake premise is falsified (every ratified
region within ΔL* 0.13; `Mat()` sets `RealtimeEmissive`, which bakes nothing);
the Edit-Mode half was load-bearing and stands. Emission-only changes need no
bake but still void gates 6–8 through the builder; no tool re-issues a human
gate (C28).

**New ruled items:**

- **R41** — the art indicator is struck **as a colour, kept as an object**: at
  chroma 43–49 rendered it is ten times more saturated than any other emitter
  and loses on scarcity, not area; C4/T34 admit no red-in-light exception. It
  moves into the room's warm family (the rust end or the screens' 83–85°,
  never signal-red), chroma bounded against the room's other emitters on the
  instrument.
- **R42** — WindowGlow is **ratified as textured**: the emission map governs
  the window's colour (the night-city sodium is R24's contract). Standing
  clause: on a textured emitter the authored value is **a multiplier, not a
  colour**, and stays near-neutral. The gate detects emission maps and
  annotates those surfaces.

**The emission instrument is adopted as the room's standing emission gate** —
controls `a == b == z` bit-identical per pose, an independent
authored-chromaticity cross-check, footprint coherence (the lid predicts 0px
seated / 51.18% focused), sub-2-code-value regions reported UNCOVERED, and
ON | OFF | DIFF×6 crops as delivered evidence. The first instrument in the
studio that reads light rather than pixels or constants.

---

## Amendment — 2026-08-08 (batch 16, transcribed by the orchestrator)

**R41 closed at the restored luminance.** Chroma granted (43–49 → 5.7–6.5, in
the room's emitter band, every value from ratified law); the luminance halving
reversed — the lamp carries `(0.3292, 0.2770, 0.2572)`, L\* 60.49, chroma 5.4,
hue 49.7°. A standby lamp that does not read as lit is the broken register.

**R41-am, standing law:** when a direction names a swatch, **the swatch
supplies hue and chroma; luminance is the element's own and never travels
with it.** (And the DD's two-ends direction is recorded as a false choice —
rust's chromaticity cannot meet a chroma-5 bound at any amplitude; treating
the bound as the constraint was the correct resolution.)

**R39-am closed — the pre-committed disposition fired.** The phone's emission
is invisible at runtime even mid-buzz at `Amp(15)`: granted colours stand
(they govern Edit-Mode captures, the material, and every bake-adjacent path),
and **no cue, state or gameplay signal is ever built on the phone's glow.**

**Adopted:** a small-object albedo change needs no bake (measured ΔL\* ±0.00
on every ratified region) — it still voids gates 6–8 through the builder, and
no tool re-issues a human gate (C28). And the control rule, alongside C32:
*a control that fails for a known harmless reason is a control everyone
learns to ignore* — fix it at the cause.
