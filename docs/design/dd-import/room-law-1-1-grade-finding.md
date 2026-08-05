# Law 1.1 — the room reads cool, and the cause is the unified grade

**From:** room-refinement lead · **Date:** 2026-08-01
**Routed:** joint **Design Director + TV** — the unified grade spans both slices and neither can
answer this alone
**Instrument:** R23 conformance set, built and measured this window
**Captures:** `room-r23-conformance/` beside this file

Readable without repo access. Every figure is measured, stated inline, and reproducible by one
command.

---

## 1. What R23 was for, and what it found

Law 1.1 says a blue-tinted room is the explicit failure mode. It has been reporting **itself** as
failing — the doc tells readers the graded captures "read cool-blue overall and must not be sampled
for colour."

R23 diagnosed why that was unresolvable: the law was being judged on gameplay frames containing
three emissive screens and a green TV light. Those frames cannot separate *the room is cool* from
*the screens are cool*. The law was unfalsifiable on its own evidence.

The ruled set fixes that — screens dark, the room's own rig, its own grade, surfaces measured as
mean chroma and CIELAB hue angle. Built and run:

| surface | L* | chroma | hue | |
|---|---:|---:|---:|---|
| wall (right plaster) | 10.66 | 6.92 | 268.5° | **COOL** |
| wall (far plaster) | 13.05 | 9.47 | 272.9° | **COOL** |
| floor (aisle) | 12.21 | 8.26 | 271.8° | **COOL** |
| bunk (1 / couch side) | 13.23 | 6.65 | 268.6° | **COOL** |
| ceiling plaster | 10.05 | 7.64 | 270.2° | **COOL** |
| bunk (2 mattress) | 17.85 | 0.82 | 173.0° | neutral |

**Law 1.1 fails.** Five of six surfaces sit in the blue quadrant.

And it fails *worse* with the screens off than on — chroma rises on every surface when they are
darkened. R23's clause covers a cool cast that appears only when screens are lit, and calls that a
screen finding. **This is the inverse, so it is a room finding**, exactly as the ruling anticipated.

## 2. It is not the light. MoonDirectional is cleared.

R18 registered `MoonDirectional` as a bounded exception — a cool directional reaching every surface
uniformly, the exact mechanism of 1.1's failure mode — and set its bound as *"the measured cast of
the room's own surfaces under R23."*

That measurement now exists. One extra frame, identical except the grade is off:

| surface | graded | **ungraded — the room's light alone** |
|---|---|---|
| ceiling plaster | 270.2°, chroma 7.64 | **113.5°, chroma 0.55 — neutral** |
| bunk (1 / couch side) | 268.6°, chroma 6.65 | **179.4°, chroma 0.57 — neutral** |
| wall (right plaster) | 268.5°, chroma 6.92 | **110.8°, chroma 1.66 — neutral/warm** |
| floor (aisle) | 271.8°, chroma 8.26 | 273.3°, chroma 2.97 |
| wall (far plaster) | 272.9°, chroma 9.47 | 275.8°, chroma 5.49 |

The room's own light is **neutral to faintly warm** on every surface away from the window. The
grade multiplies chroma **4× to 14×** and rotates hue from ~110° to ~270°.

The two surfaces still cool ungraded are the far wall and the floor — precisely where the window's
cool short-throw pools, which is designed behaviour and locally bounded.

**`MoonDirectional` passes its R18 bound.** It is not the cause and should not be re-tuned to fix
this; doing so would darken the room to correct a problem living somewhere else entirely.

## 3. The cause, and why it is not a defect

The room's blue cast is a **faithful, intended consequence of the unified grade**.

The grade lifts the black floor so nothing in frame is darker than a screen's off state — a sound
goal, and the room's own build comment records the target: *"tinted slightly cool to land near the
spec's `#0a0c10` target rather than a grey."*

That target is itself blue:

| | L* | chroma | hue |
|---|---:|---:|---:|
| unified-grade black target `#0a0c10` | 3.30 | 2.08 | **273.8°** |
| a neutral black of the same value | 3.64 | 0.00 | — |

**273.8° is the hue the room's surfaces are reading.** The lift vector is `(0.99, 1.00, 1.03,
0.0075)` — the blue channel at 1.03 against red at 0.99, applied additively to shadows.

The room's surfaces sit at **L\* 10–18**. The black target sits at **L\* 3.3**. At that separation
the black point is not a floor underneath the image — **it is the image**. Everything in a room this
dark is shadow, so a cool black point is a cool room, by construction.

## 4. Why this is a joint question

Two ratified specs are in direct conflict, and both are behaving exactly as written:

- **Room law 1.1** — a blue-tinted room is the explicit failure mode.
- **The unified grade** — black lands near `#0a0c10`, a deliberately cool near-black, chosen so the
  panel's black never reads as the darkest thing in frame.

Neither is a mistake. They are incompatible in a room whose entire value range sits within 15 L\*
of the black point. Any fix trades one spec against the other, which is why this is not mine to
make and not the TV's to make alone.

## 5. Options, with the trade stated honestly

I have not implemented any of these, and I am not recommending one — the trade is a design call.

**A. Neutralise the lift's hue, keep its lift.** Take the black point to a neutral of the same
value. Preserves the stated *purpose* (nothing darker than the screen's off state) and removes the
cast. But it contradicts the spec's stated *target*: `#0a0c10` was chosen cool on purpose, so this
needs the TV seat's agreement that the value mattered less than the level.

**B. Split the grade.** A separate lift for the room. Removes the conflict outright and breaks the
premise — the grade is unified precisely so a surface graded with the room belongs to the room.

**C. Accept and amend 1.1.** Ratify the cool cast as the direction's intent. Cheapest, and it
contradicts the direction's core: the room is warm dirty plaster under a failing sodium tube, and
the anti-reference this project names by name is the cyberpunk-blue rut.

**D. Warm the room's lighting to counteract.** Fights the grade with light, costs the single-source
read, and leaves the black point still blue underneath. Cheapest to try and the one I would
caution against — it treats a measurement as a look.

## 6. What I have not done

I have not touched the grade. It is ratified, it is shared, and 1.1 is a DD law — naming the
parameter is mine, changing it is not. The room ships as measured, with the finding recorded.

## 7. The instrument is now repeatable without me

The measurement is folded into the room's gate harness and runs editor-free:

```
python tools/room_gate_check.py --scene <scene> --captures <dir> --conformance <r23 dir>
```

It reports per-surface L\*, chroma, hue and a warm/cool verdict, prints the graded and ungraded
columns side by side, and **fails the run** while any surface reads cool. Without the flag it skips
and leaves the ordinary exit code alone, so the everyday gate still means "did anything regress"
rather than sitting permanently red behind one known open finding.

R18 requires this re-measured whenever `MoonDirectional`'s colour or intensity moves. It now costs
one command instead of an editor lease.

## 8. Captures

**R26 addendum — the set is now captured twice, and the isolation is doubly attested.**

| file | pass |
|---|---|
| `conformance-seated-screens-dark.png` | graded — the seated rig the ruling names |
| `conformance-room-screens-dark.png` | graded — canonical for the region measurement |
| `conformance-seated-screens-dark-UNGRADED.png` | grade bypassed |
| `conformance-room-screens-dark-UNGRADED.png` | grade bypassed |

Same rig, same framing, same regions, same instrument; **only the grade differs between passes**, so
any difference between them is attributable to the grade and to nothing else.

Whole-frame cast, both poses:

| pose | graded | grade bypassed |
|---|---|---|
| seated 17° | chroma 7.75, hue 269.8° — **COOL** | chroma **1.09** — neutral |
| wide 68° | chroma 6.92, hue 272.8° — **COOL** | chroma **0.73** — neutral |

Bypassing the grade collapses chroma **7× to 9.5×**. **The room is essentially achromatic without
the grade** — and the two poses agree independently, which they could not do if this were a framing
artefact.

**The captures reproduce byte-for-byte.** Every frame shared with the earlier run has an identical
MD5, so this is a deterministic measurement rather than a single favourable render.

Compare `conformance-room-screens-dark.png` against its `-UNGRADED` twin. The difference between
those two images is the entire finding.

**One caution on the second datum.** The TV slice's green cast across 189 frames is *consistent*
with a shared-grade cause but is not yet comparable evidence: the room's cast is blue at ~270°, the
TV's is green, and green in that slice has known independent sources under C2 and C13. Those frames
were not captured screens-dark or grade-bypassed, so the two slices are not a matched pair. If the
TV set were captured on the same terms, the comparison would become direct — and worth having
before the grade session concludes.
