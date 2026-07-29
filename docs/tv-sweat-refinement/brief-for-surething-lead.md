# Room + TV sweat art direction — context for the SureThing lead

**From:** TV sweat slice · **Date:** 2026-07-28 · **Status of what follows:** the room and TV
directions are **approved and final** (Allen, 2026-07-27, after seven concept rounds)

**What this is.** Context so the laptop can sit in the same world, not a specification for your
surface. Where this brief and your own direction disagree about SureThing, yours wins — you own that
screen. What is genuinely binding across all three surfaces is called out in §5.

**Read §4 first if you read nothing else.** It is the part most likely to be got wrong, and the part
where copying us would actively hurt you.

---

## 1. The world all three surfaces live in

A cramped bunker room at night, in a wealthy high-tech city that has no use for the occupant. The
room is rotting; the city outside the window is neon and functioning; the screens are the only things
that work. Dark comedy about the gambling industry — the machines are nicer than the life.

Rendering register: **painterly semi-realistic**, not stylised, not photoreal.

## 2. The room

**Construction.** Peeling paint on walls, ceiling and floor. Exposed black conduit and pipes across
every surface. Riveted steel, bolted brackets, chipped institutional paint. Two heavy bunk frames —
one over the sofa, one over the desk. A deep-set window on the far wall onto a neon city skyline.
Battered metal desk with the laptop and an ashtray of cigarette butts. Metal stool. Coiled cable on
the floor.

**Palette.** Olive, khaki, drab green, rust, damp concrete. Desaturated and natural. Wall albedo is
warm dirty plaster, roughly `(0.255, 0.245, 0.210)` — which means **the room physically cannot return
saturated cool colour**, and that is deliberate.

**Lighting — three sources that must stay distinguishable:**

| Source | Character |
|---|---|
| Fluorescent strip | Slightly yellow, warm, dim |
| Window | Cool blue, bright at source, **short reach** — pools locally near the window, does not tint the room |
| Screens | Quiet. Faint spill |

The room stays natural olive everywhere the window is not directly lighting. A blue-tinted room is
the failure mode and was explicitly rejected.

**The second bunk, over the desk, stays dark** — dressed as though someone sleeps there, never lit
enough to confirm whether anyone does. Legible as *occupied*, never as *empty*. A deliberate unease.

## 3. The TV sweat

### The object

**Not a television.** A hardened industrial display bolted into the wall: heavy steel housing,
visible rivets, thick chipped painted bezel, glass recessed into the frame, a small stencilled
equipment code, one physical indicator lamp, conduit feeding in continuous with the room's pipe runs.

The governing sentence: **installed by an institution, not bought by the occupant.**

### Fidelity — "old but maintained"

Not failing, not new. A decade-old instrument that works perfectly.

- Sharp and legible on a **visibly coarse grid** — crisp forms with slightly chunky edges, never
  sub-pixel smooth
- Solid drawn 1–2px rules; **no delicate hairlines**
- Technical uppercase type, medium weight, slightly condensed
- Slight unevenness in the backlight, soft bloom
- **Banned outright:** scanlines, screen curvature, phosphor haze, dithering, interference noise.
  These were the signature of a deprecated direction (`design/08-art-direction.md`) and landing back
  on them means the redesign did not happen.

### Palette

Near-black ground. **Cold and quiet, with one warm bar.**

| Role | Colour | Where |
|---|---|---|
| Fact | Cold white | Score, clock, live leg names, market lines |
| Context | Grey | Labels, odds, risk/payout, pitch markings |
| Structure / pending | Dim grey | Not-yet legs, dividers |
| Dead | **Unlit** | Lost legs. Nearly extinguished |
| Money & won | **Gold** | Won leg names, payout figures |
| Action | **Gold, inverted** | Cash-out band only — solid gold field, dark type punched out |
| Team identity | **Muted** blue and **muted** pink | Pitch dots, and nowhere else |

Real committed values, if useful as anchors:

```
black floor        (0.048, 0.055, 0.068)   — nothing on screen darker than this
dead / extinguished(0.045, 0.05,  0.065)   — sits BELOW the floor, deliberately
gold L4 (HDR)      (1.84,  1.31,  0.29)
TV light spill     (0.72,  0.75,  0.80)    — near-neutral cool grey-white
```

**Three rules that make it work:**

1. **Gold is rationed.** Only money. When everything is gold, gold means nothing — we rendered that
   mistake and rejected it. The scarcity *is* the signal.
2. **Team hues are quiet and local.** They are the least prominent colours on the display, not the
   most. Identity is carried by the ticket naming the team in words; the dots only need to be
   separable.
3. **Everything else is colourless.** White and grey do the work. That is what makes it read as
   instrumentation rather than an app, and what gives the one gold bar its force.

### Brightness is the primary semantic channel

Five tiers — L4 full, L3 active, L2 present, L1 dormant, L0 extinguished — and **at most one L4
element exists at any instant.** If two things want full brightness, the design has not decided what
matters. On our surface this is now enforced in code: only three elements can carry the HDR material
at all.

Loss is **darkness**, not red. A dead leg drops to L0 and survives as unlit structure. Thematically
exact: losing returns you to the room.

### Layout and motion

Full-height ticket rail on the left at ~26–28% of the width, stage right, cash-out anchored at the
foot of the ticket column. Reading starts at the left, so the bet is the first fixation — the product
is about the bet, not the sport.

Motion is **panel refresh, not animation.** State changes are quantised discrete steps; no eased
tweens between poses. One pulse kind on the whole surface (live legs), synchronised in phase.

## 4. What should and should NOT carry over to SureThing

**This is the part where copying us would hurt you.**

Our constraints come from **four metres, in the dark, muted, from a couch, with no agency** — the
player cannot influence what the TV shows. Yours come from **forty centimetres, leaning at a desk,
with full agency** — SureThing is where the player *builds*.

That difference should be visible.

### Do not carry over

- **The coarse grid and monumental type.** Correct at four metres, wrong at forty centimetres. At
  laptop distance that reads as artificially crude rather than as legible hardware.
- **The institutional register.** The TV was installed by someone else. **The laptop is the
  occupant's own machine** — personal, chosen, probably cheaper, probably grubbier, possibly
  customised. That is a real characterisation difference and worth using.
- **Brightness as the sole semantic channel.** We lean on it because hue is nearly absent and reading
  distance is long. Up close you can afford finer distinctions, and the one-L4 rule is likely too
  blunt for a surface with many simultaneous controls.
- **Our motion discipline.** Quantised, no easing, one pulse — that suits a broadcast instrument you
  watch. A tool you *operate* wants responsive, continuous feedback.

### Do carry over

- **The world.** Grimy, industrial, night, a functioning neon city outside and a rotting room inside.
- **The unified grade.** One post-process over the whole game — grain, haze, lifted blacks, bloom,
  chromatic aberration, vignette. **This is the single biggest thing that makes disparate surfaces
  feel like one product.** Your screen goes inside that pass, not exempt from it. Spec is in
  `unified-grade-spec.md`; the room lead owns the volume.
- **Lifted blacks.** No screen in this room shows pure `#000000`. A screen whose blacks beat every
  shadow in the room reads as composited rather than photographed. This is the single strongest
  "belongs / does not belong" signal we found across seven rounds.
- **The money language.** Gold means money and action. Green-means-good and red-means-bad are
  **retired game-wide** — please do not reintroduce them.
- **Diegesis.** Every interface is a real screen on a real object in a real room, viewed at an angle,
  with glass, reflection and dust. Nothing floats in a HUD.

### The register difference worth making explicit

The TV is **hot** — it is the sweat, you cannot influence it, and the design is built to make one
thing at a time unmistakable. SureThing is **calm** — it is where you think, compare, and commit.

If those two screens feel the same, one of them is wrong. They should feel like the same *world* and
the same *hand*, doing different jobs.

## 5. Actually binding across all three surfaces

Not style preferences — product constraints, from `PRODUCT.md`:

1. **Diegesis.** Screens are objects in the room. Nothing floats.
2. **Fictional leagues, teams and players only.** IP safety and the comedy both require it. Concept
   renders came back with real clubs three times; watch for it.
3. **Green/red as money language is retired.** Gold is money.
4. **The unified grade covers everything.**
5. **Voice:** dark comedy, satirical toward the gambling industry, never celebratory of it.
6. **The game name is deferred** — do not invent one.

## 6. Where to read more

- `DESIGN.md` — the TV sweat's full system, authoritative and final
- `docs/tv-sweat-refinement/unified-grade-spec.md` — the shared post-process
- `docs/tv-sweat-refinement/room-layout-update.md` — the room brief as sent to its owner
- `PRODUCT.md` — product truth, brand commitments, what is binding versus released

Happy to talk through any of it, and genuinely interested in what you conclude *differs* for a
surface people operate up close — that comparison would sharpen our side too.
