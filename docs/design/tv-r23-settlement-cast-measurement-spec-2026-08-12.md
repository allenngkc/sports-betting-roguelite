# The settlement cast on the housing box — measurement spec, pre-committed

**Design Director** · 2026-08-12 · discharges the desk half of T65's remaining owed item
(T65-am, batch 36: *"still owed, unchanged"*; T65-am2, batch 37: the check moves to the **rendered**
cast). **Dispatchable now. The verdict follows the frames without a further ruling.**

**The question, exactly:** does the room's settlement re-tint, **as rendered on the room's own
surface**, sit inside the room's warm family — the ruled band **CIELAB hue 85–92°**?

Not the authored constant. T65-am2 settled that the authored value is **the knob** and the band is
checked on the **rendered cast**, because the authored-to-rendered gap is real and non-uniform
(1.9° on the lid, 10.7° on the key). **Nobody authors to the band's number and assumes it renders
there.**

---

## 0. PRECONDITION — resolve this before booking the window, or the capture is void

**Is `RoomSettlementGlow()` independent of `TvLight`?**

R23's recipe requires `TvLight` **disabled**, because room's rendered absolutes were confounded by the
live panel green (§2.6). But T65's own causal record says the settlement re-tint was *originally*
fired by `WonLegBeat` calling **`tvLight.Flash(gold)`**, and the fix routed it through one painting
point, `RoomSettlementGlow()`.

**If that painting point still drives `TvLight`, then disabling `TvLight` disables the thing being
measured.** The capture would show no cast, and a reader could take that for "the re-tint does not
fire" or record a null. **C37 forbids exactly this: a null is invalid unless success would have been
resolvable** — with the subject switched off, success could not appear however the code behaved.

This is a **source read, not a finding** — cheap, and it decides which of the two routes below is
even possible. Do it first and state the answer in the capture's own header.

- **Independent** → **Route A**, the ruled route.
- **Coupled** → Route A is impossible, and **Route B** is the answer. Do not improvise a third.

---

## 1. Route A — the ruled route: R23's recipe, `TvLight` disabled

**Subject box:** the housing above the panel — the derivation's own box, unchanged from T65-am.

**Why this box is the right probe, and its one weakness.** The housing is near-neutral and dark
(R31: chroma 0.52, L\* 11.30), so its rendered hue is dominated by the incident light rather than by
its own albedo — which is what makes it a cast probe at all. **The weakness is the same fact: hue
angle on a dark, near-neutral patch is unstable, because hue is poorly determined as chroma
approaches zero.** That is a resolution problem and §4 below is how it is handled rather than ignored.

**Space and converter:** CIELAB on linear values, via the room's **shared `linear_to_lab`**.
**Never a second implementation** — V6 ruled the converter shared and never forked, because a second
one is how a two-space defect regrows invisibly to the test that caught it.

---

## 2. Route B — the fallback, if and only if the precondition fails

**Control pair, panel live in both halves, glow firing in one:** the glow's own contribution is the
**linear difference** between the two frames, and that difference is a radiance triple whose
chromaticity is the glow's. Convert *that* through `linear_to_lab`.

**This is room's own method, already endorsed** — T65-am accepted the **deltas** as sound evidence of
firing while correctly returning the **absolutes** as confounded. Route B measures the glow's
chromaticity without ever needing the room in a non-shipping state.

**Its own conditions, and they are strict:** the two halves must differ by the glow **and nothing
else** — pinned seed, asserted at shoot time, R43's read-back comparison, no clock or animation
residue between them. A control pair that is not frame-locked measures the match, not the glow.

---

## 3. The measurement carries its own calibration IN FRAME (C42, C44)

**This is the part that makes the result survive an absolute calibration error, and it costs one
frame.**

The ruled band's two anchors are themselves measured room values: **the laptop lid at 85.1–85.3°**
(bottom) and **the warm key tube at ~92°** (top) — both rendered CIELAB, both from batch 13, and both
named in V6's provenance trace as the band's real endpoints.

**Put both anchors and the subject in the same frame, under the same recipe, measured by the same
converter.** Then:

- **C44's cheap test runs for free:** feed the bound its own founding values. If the instrument
  returns the lid at ~85° and the key at ~92°, it is calibrated **on the very band it is judging**. If
  it does not, it is reading a different quantity — which is the defect that voided every historical
  V6 verdict, caught here before it can do it again.
- **The verdict becomes a within-frame ORDERING, not an absolute** (C42's in-frame invariant): the
  question *"does the cast sit between the lid and the key"* re-establishes itself at every
  measurement and survives any uniform error in exposure, grade or conversion.

An absolute number that agrees with a band measured in another session is a weaker claim than an
ordering measured in one frame. Shoot the anchors.

---

## 4. Resolution, stated before the shoot, not after (C32, C37)

**The band is 7° wide.** The subject is a dark near-neutral patch. **Establish that the instrument can
separate the anchors before trusting it on the subject.**

**The test is already in the frame:** the lid and key sit ~7° apart, which is the band's own width. If
the instrument cannot cleanly separate the lid from the key in this capture, **it cannot adjudicate a
7° band on a darker patch either**, and this is settled before any verdict is written.

**Report chroma beside every hue.** A hue angle quoted without its chroma is a number whose
uncertainty is unstated; on a near-neutral patch it may be arbitrary.

---

## 5. The dispositions — PRE-COMMITTED, before any frame exists

Written now so the verdict is not chosen after seeing the number (C41's discipline; the failure it was
written for was expressing a pre-commitment as a value).

1. **The instrument separates the anchors, and the subject's cast falls BETWEEN them** →
   **the re-tint is inside the room's warm family. T65's remaining half CLOSES.** The authored
   constant stands as the knob that produced a conforming render, and no further authoring is asked
   for.
2. **The instrument separates the anchors, and the subject's cast falls OUTSIDE that interval** →
   **the re-authored value has not landed the family.** The authored constant is re-derived — and per
   T65-am2 **nobody authors to the band's number**: the new value is chosen, rendered, and re-checked
   on this same instrument, because the authored-to-rendered gap is non-uniform.
3. **The instrument CANNOT separate the anchors** → **returned UNADJUDICATED (§2.6), and it is a void,
   not a null (C37).** Not a pass, not a failure, and specifically **not evidence that the cast
   conforms.** The recipe re-runs with more signal — brighter subject exposure, a larger box, or a
   less neutral probe surface — and the re-run states what changed.

**Direction of travel, per C41, stated instead of a target number:** if the re-authoring worked, the
cast reads **warmer than the green it was struck for** and **no cooler than the lid**. That sentence is
the expectation. A number to land on is exactly what C41 forbids here.

---

## 6. What this measurement CANNOT see — stated in its own result (C18 §4.2)

- **It is one box.** T65 recorded point-light falloff across the room (+44.5 → +20.0 → +9.5). The
  cast's hue on the housing does not certify its hue elsewhere.
- **Route A is not the shipping state.** With `TvLight` disabled the room is not what the player sees.
  This answers **"is the authored re-tint inside the palette"** — a conformance question. It does
  **not** answer "how does settlement read", which is the glow **plus** the panel, and **no later item
  may cite this capture for that claim.**
- **It does not re-derive the amplitude window.** `[0.78, 1.06]` and the *130° at zero falling to
  ~45.5°* trajectory were voided as evidence at V6 and re-derive **against the converted gate**, not
  against this frame. If this capture is later cited for the window, that is the same error twice.

---

## 7. Owed alongside, and separable — V6's verdict inventory

V6 ruled that **TV owes an inventory of V6 verdicts already acted on, in both directions**, and that
*"we'll re-run it"* is not the deliverable — **the members are** (C18 §4.1).

Shape, so it can be produced without another round trip: **every verdict V6 issued that anyone acted
on, each with the value it judged, the direction it judged (in-band or out), what was done on that
basis, and whether that action still stands under the converted gate.** In-band verdicts certified
nothing **and** out-of-band verdicts are equally void, because the regions were disjoint rather than
shifted — so **a rejected value may have been a correct warm one**, and the inventory is as likely to
recover work as to discard it.

Independent of the capture. Dispatchable in parallel.
