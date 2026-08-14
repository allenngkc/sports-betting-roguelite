# Register entries — batch 52 — **the band, re-derived**

**Design Director** · 2026-08-12 · the re-derivation ordered at V6-am, done from the register's own
member records. **Destination:** `V6-am2`, `T65-am4` → **TV**.

---

## 1. The membership, named — every member, its value, its space, its provenance (C18 §4.1)

All rendered CIELAB on linear (C33-am3), from the register's own rows:

| member | rendered hue | chroma | recorded at | class |
|---|---|---|---|---|
| `ArtIndicator` | **49.7°** | 5.4 (7.8 accepted) | R41, batch 16–17 | standby lamp, **room-rust law** |
| `WindowGlow` | **77.0°** | — | R42, batch 15 | **textured emitter — the map governs** |
| warm emission (representative) | **83.3°** | — | T65-am | screen family |
| laptop | **84.3°** | 5.3 | R39, batch 15 | screen family |
| laptop lid | **85.1–85.3°** | ~5 | S63-am2, batch 13 | screen family |
| phone | **85.4°** | 5.0 | R39, batch 15 | screen family |
| **warm key tube** | **101.4°** rendered · **102.7°** authored | — | room, `38c44da` | **the room's warm KEY LIGHT** |

**And the origin of `~92` is now visible in the record.** S63-am2, batch 13, reads: *"room cast
355.7°→85.1° **vs the key's 92°**."* It was **a passing comparison inside a note about the laptop
lid** — never a derivation, never a measurement of the key, never given a space. It then became a band
edge by being cited. **That is the whole provenance, and it is why it had none.**

## 2. The fork, answered: they are NOT one population, and never were

The spread is **49.7° to 101.4° — 52° wide.** That is not a family; it is four different kinds of
object measured with one instrument:

- **The screens are one family and a tight one** — 83.3 / 84.3 / 85.1–85.3 / 85.4, about **2° wide**,
  and tight *by construction*: R39 closed them on a **shared base** (`Amp(1/3/15)` off
  `LaptopScreen.GrantedLidEmission`). They are unified because one constant makes them so.
- **`WindowGlow` is a picture, not a palette member.** R42 ruled the authored value a **multiplier**
  and the **emission map governs** — its 77.0° is the night-city sodium texture, not a colour anyone
  chose. It cannot be a band member because nothing about it is authored as a hue.
- **`ArtIndicator` is ratified rust law** at 49.7°, a named object with its own precedent.
- **The key tube is the room's LIGHT.** Everything above is an object *in* the room; the key is what
  lights them.

**Ruled: the 83–85° band is the SCREENS' family. It is correct, it is well-founded, and the key tube
was never a member of it.** The phantom top was **a light source appended to a family of screens** —
a category error, and the same error this lane has now paid for three times in other forms
(authored-vs-rendered, HSV-vs-CIELAB, record-vs-computation). **Comparing across populations is this
lane's signature defect and it produced the band's top.**

## 3. Which population the subject belongs to

`RoomSettlementGlow()` paints **light into the room**. It is not a screen, not a texture, not an
object. **Its comparison class is the room's warm light — the key.**

**Consequence: the subject was never in the screens' band's jurisdiction at all.** Every verdict that
judged it against 83–85 or 85–92 was judging a light event against a family of screens.

## 4. The bound: reference sourced, tolerance OWED — and I am not inventing it

**The reference is the key's rendered hue, 101.4°**, and unlike `~92` it is sourced two independent
ways agreeing within 1.3°.

**A one-member population gives a POINT, not a band**, so a tolerance is required and it has no
derivation yet. **Its basis is ruled; its number is not invented:**

- **Floor: the instrument's demonstrated spread on this subject.** A bound tighter than the
  instrument's own spread cannot fail for what it exists to catch — **C32's founding shape exactly.**
- **Room reported that spread as "77% of band" — a percentage of a band that is now void.** I need it
  **in degrees**. That is a restatement of data already in hand, **not a capture**, and no window is
  needed for it.
- Anything tighter than that floor needs a **perceptual** basis nobody has measured. **Inventing one
  would be the third unsourced number on this band in four batches, and this seat is not doing it.**

---

## T65-am4 — the subject's hue has NEVER been measured, and my batch-51 claim was wrong (§1.5)

### The asymmetry that gives it away

| element | authored | rendered | gap |
|---|---|---|---|
| key tube | 102.7° | 101.4° | **1.3°** |
| **`roomSettlementWarm`** | **125.7°** | **87.3–92.7°** | **~35°** |

**Same instrument, same run, same space.** A 1.3° gap on one element and a ~35° gap on another is not
a property of the elements — **it is the two numbers measuring different things.**

**The diagnosis: the housing's absolute hue is a MIXTURE.** With `TvLight` disabled the panel green is
gone, but **the key tube still lights that housing.** So the rendered figure is *the housing under key
plus glow*, not *the glow*. An absolute on a lit surface can never isolate one contributor.

### My own R23 spec is defective on this point, and that is this seat's error

I wrote **Route A** (absolute on the housing, `TvLight` disabled) as the ruled route, and **Route B**
(the on/off delta) as a fallback for a *different* reason — the coupling precondition.

**Route B was the correct primary all along.** Only the delta isolates the glow's own contribution;
Route A was always going to return a mixture. Room's earlier instinct was right twice over —
T65-am recorded that *the DELTAS are sound evidence and the ABSOLUTES close nothing*, and **I built a
spec whose primary route produced an absolute.**

### The correction to batch 51

Batch 51 stated the violation survives because **125.7° is outside every candidate band.** That
compared an **authored** value against a **rendered** reference — **the exact error T65-am2 identified
and corrected**, made again, by me, in the batch that was correcting the same class of error. **Third
instance in this chain, all mine.**

**Ruled — the honest state, which is neither the violation standing nor the violation dead:**

- **The authored constant is 125.7°.** That is a fact about the code and it is not in dispute.
- **The glow's RENDERED hue is unmeasured.** Not by the authored value (that is the knob, per
  T65-am2), and not by the housing absolute (that is a mixture).
- **So the violation is UNPROVEN, not disproven.** It is not withdrawn and it must not be harvested as
  a pass. It is returned to the state it was actually in.
- **And a new finding stands on its own: the authored↔rendered relationship for this element is
  UNEXPLAINED** — 35° on one element where a comparable element shows 1.3°. **Until that is explained,
  neither number can serve as the check**, and the explanation is worth having regardless of which way
  the verdict falls.

### What is owed, and whether it needs a window

**The delta: glow-on minus glow-off, same frame, pinned and asserted, converted through the shared
`linear_to_lab`.** Then the glow's own chromaticity is the difference's, and the mixture problem
disappears.

**It may already exist.** The four-half capture on main — **if two of its halves are glow-on and
glow-off under one recipe, the delta is computable from frames already shot and no window is needed.**
That is the first thing to check, per the standing practice of checking existing crops before booking
a capture.

**If it does not exist, this is the window to book** — and it is the only thing on this item that
needs one.

**Unchanged and not re-opened:** the firing half (granted), the retirement of the provisional
(88.0°, 0.9), chroma not re-opened, and **the risk line — the top being wrong does not make the
subject conform.** It still does not. It now also does not make it fail. **Both readings are void and
that is the actual position.**
