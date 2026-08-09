# Register entries — 2026-08-08, batch 16

**Transcribe into `main-2/docs/design/REGISTER.md`.** Issued at authoring time per C22. Ruled from the
tables (current through batch 15), not from batch files.

New IDs: **T68**, **T69**. Closures: **T67**, **R41**, **R39-am**. Amendments: **R41-am** (my
direction's ambiguity, and my false choice), **C33-am2**.

---

## T68 — `HOLD E` is invisible on the lit band. **NEW — BLOCKER. The money control has no readable label.**

**NEW · DD 2026-08-08.** Found in the T67 capture, which was delivered to answer a different question.

Measured on `01-seated-BAND-LIT.png`, the acceptance view:

| | luma | rgb |
|---|---|---|
| gold field, clear area | **0.807** | 223, 216, 54 |
| `HOLD E` darkest ink | **0.793** | 216, 211, 73 |
| **contrast ratio** | **1.02 : 1** | |
| the same label against spec `goldInk #0A0C10` | 15.3 : 1 | — |

**The label is not punched out of the field. It is the same value as the field.** On the surface's
only L4 element, the money control, at the only view the owning doc accepts.

### The diagnosis is in the comparison, not the number

The **unlit** state's label is fine. Same slot, same frame pair:

| state | field | label | CR |
|---|---|---|---|
| `updating` (dark) | 0.106 | 0.759 | **5.19 : 1** — legible |
| `actionable` (lit) | 0.807 | 0.793 | **1.02 : 1** — invisible |

**The field inverts; the type does not.** §6.1 specifies *"gold at L4, inverted field with dark type
punched out"* — the inversion is a two-part operation and only one part is implemented. The label keeps
its light ink and the field rises to meet it.

### It is not caused by T63, and it is worse than T63 left it

Pre-fix the field measured 0.696 against a 0.827 figure — **1.17:1, already a failure.** T63's
structural fix raised the field and narrowed it to 1.02:1 at this view. **The defect predates the fix;
the fix made it marginally worse.** I am not recording this as a regression, and the grant stands —
the field's HDR material was genuinely missing and genuinely needed fixing.

### Why nobody saw it, and the standing consequence

**Every T63 measurement compared the band to other elements** — scoreline, ball, ticket column, event
strip. Three submissions and two batches of ladder work, all of it inter-element. **Nothing ever
measured the band against its own label.**

**C33-am2 — an L4 ranking gate cannot see internal contrast.** The ladder answers *which element
dominates*; it is silent on *whether that element can be read*. A dominance gate and a legibility gate
are different instruments, and this surface had only the first. **V1 gains a companion: every element
that inverts reports the contrast between its field and its own ink.** That is the sixth vacuous-green
shape this fortnight and the first one inside a single element.

**Ruled: the label inverts with the field.** `HOLD E` and the amount take `goldInk` on `actionable`
and `accepted`; the unlit states keep the light ink they already have correctly. **Blocker — this is
the confirm-gesture copy T22/T36 ruled, and the player currently sees a solid gold field with no
instruction on it.**

---

## T67 — the bloom into the event strip. **CLOSED. No change. One structural guard.**

The capture is exactly what was ordered: the seated acceptance view at the pose `RoomViewCapture`
already owns, on the shipped build, **no Unity launched and the editor lease returned unused.**

**Ruled: the strip does not visibly warm at that distance.** The judgement was reserved to me and this
is it, on these frames:

- The strip's **mean moves +0.006** — under the instrument's own ~0.01 resolution (C32).
- **The strip's copy does not warm at all.** The authored line's ink begins at canvas x≈439, where
  d(mean) and d(peak) are both **0.000**. The glyphs are black in the difference.
- What warms is **the empty left margin of the strip's zone**, and only for 40px. Zero from x=365.
- At 1× the two frames are indistinguishable to the right of the boundary. The halo needs 6×.

**Naming the peak/mean split correctly is what settles it.** Peak takes gold because peak is being set
by a *neighbour's* bloom rather than by the element's own ink — C33-am's class, one step over, applied
by the lead against their own headline number. A strip that had warmed would move its mean.

### The uncovered case is real and gets a structural answer

A centred line only clears the halo while it is short. A near-full-width authored line would put its
first glyph inside the 40px halo, and **no such line is in this capture** — correctly reported as an
uncovered case rather than dressed up as a measured pass. T46's shape, named by the lead.

**Ruled: the strip's text zone begins 40px past the boundary** — canvas x 305–980 rather than 265–980.
One constant. Every line, at any length, then begins outside the measured bloom reach. **Nothing moves
visibly for a short line** (the centre shifts 20px), no bloom value is touched, and the band this seat
just granted is not dimmed. That is the pre-committed remedy — separation — applied structurally
instead of relying on the seeds to keep producing short lines.

---

## T69 — the leg statement restates its own team and overruns its column. **NEW — violation.**

**NEW · DD 2026-08-08.** Also from the T67 frame. Two defects in one string:

> `Atlanta Middlemen ML — Atlanta Middlemen v Tulsa Startups`
> `Yonkers Auditors ML — Yonkers Auditors v Reno Muskrats`

1. **The backed team is named twice in one row.** The pick and the fixture are being concatenated
   without noticing they share a term. S37's shape on the TV: the row states its scope, then restates
   it. `ATLANTA MIDDLEMEN ML · v TULSA STARTUPS` carries both facts once.
2. **The rows wrap to three lines.** §5.1: *every leg slot is authored at the live row's measured
   height, reserved always* — and T24 is explicit that **authored strings do not bend to measurements;
   an over-long string is re-authored against a call-site-recorded measurement.** Three-line wrapping
   is the string exceeding a fixed slot, which is the one thing that section forbids.

The live statement `RICO LANYARD TO SCORE` also terminates at the column rule mid-word. **I am not
recording an overprint:** ink past the rule measures 5 columns at this view, which is inside what
bloom and chromatic aberration produce here, and TV-12/13 wants truncation **on a word boundary**
regardless. The fix for both is the same — re-author against the measured column.

**Scope (C25):** one seed, one frame. The duplication is structural (it is how the string is built, not
what this seed produced); the wrap is seed-dependent and a longer team pair would be worse.

---

## R41 — the lamp. **CHROMA GRANTED. The luminance halving is REVERSED — take the alternative.**

### The chroma fix is right and it is ratified law, not taste

| pose | before | after |
|---|---|---|
| standing | chroma **43.0**, 41.8° | chroma **5.7**, 82.1° |
| seated | chroma **48.6**, 40.6° | chroma **6.5**, 59.5° |

Against the room's band — ScreenLaptop 5.3–5.5, ScreenPhone 5.0. It was ten times the room; it is now
in it. Albedo is the ratified `--room-rust` swatch, emission is rust's hue at the screens' authored
chroma, and **nothing was picked by eye.** Controls bit-identical on every pose. **Granted.**

### R41-am — my direction contained a false choice, and the lead resolved it correctly

I offered *"the rust end or the screens' 83–85°, chroma bounded against the room's other emitters"*.
**Rust's chromaticity cannot meet a chroma-5 bound at any amplitude** — the lead's ladder shows it
still at chroma 8.1 when it has already fallen to L\* 4.54, i.e. black. Dimming a saturated
chromaticity drops lightness faster than chroma.

Treating **the bound as the ruled constraint and the hue as the choice between two sanctioned ends** is
the correct resolution, and taking rust's hue rather than the screens' keeps the lamp distinguishable
from them. Recorded as the seat's error (§1.5): I named two ends without checking that either could
meet the bound I attached to them.

### The luminance is reversed, and the declaration is why this took one batch instead of three

**The lead changed a value I did not rule, said so at the top of the submission, gave both numbers, and
had the alternative ready.** That is exactly the behaviour that makes a lead's judgement worth
extending — and the self-catch is sharper still: *"the opposite of what I did on the phone, where I
preserved the amplitude ladder to ±1 L\* precisely so it could not."*

**Ruled: restore the original luminance at the banded chroma.**

```
(0.3292, 0.2770, 0.2572)     L* 60.49   chroma 5.4   hue 49.7°
```

Three reasons, in order of weight:

1. **A standby lamp that does not read as lit is the broken register.** T1's whole ground is
   *maintained industrial equipment that works perfectly*. I looked at
   `R41-lamp-BEFORE_vs_AFTER-seated.png` as instructed: the after reads as a **dull grey-brown patch,
   not a lit lamp.** A dying indicator is T8's world one object over.
2. **My ruling was explicitly about colour, not presence** — *"struck as a colour, kept as an object …
   it stays an object; it stops being the only saturated thing in the room."* Halving its luminance
   takes away the half I said to keep.
3. **R35's caution is mine and it binds here.** A hue change must not become a value change.

**R41-am, second clause — the ambiguity is mine.** *"Warm, **dark**, low-chroma"* described the
**swatch** — rust is a dark swatch — and was read as an instruction to dim the lamp. Reasonable
reading of an imprecise sentence. **When a direction names a swatch, the swatch supplies hue and
chroma; luminance is the element's own and does not travel with it.** Standing.

---

## R39-am — the phone. **CLOSED. The pre-committed disposition fires.**

| pose | changed | > JND |
|---|---|---|
| standing | 0.02% | **0.01%** (a 44 × 21 px box) |
| seated | 0.00% | **identical** |
| focused | 0.00% | **identical** |

**And it was caught mid-buzz** — the live value at capture was `Amp(15)`, **fifteen times rest and the
loudest state the phone ever reaches.** If the buzz cannot be seen, idle cannot. The accident makes
this a stronger result than the test I asked for, and reporting the accident is why it counts.

**Disposition, as pre-committed in batch 15:** the granted colours **stand** — they govern Edit-Mode
captures, the material, and every bake-adjacent path — and **no cue, state or gameplay signal is ever
built on the phone's glow.** Same disposition as the lid, for the same measured reason. **Closed, and
it needed no second ruling from me, which was the point of pre-committing it.**

---

## Adopted from this submission

**The bake is a measurable no-op for a small-object albedo change.** Every ratified region held at
**ΔL\* ±0.00** across both the albedo change and the bake. Carried forward: a small-object albedo change
does not need a bake. It still voids gates 6–8 through the builder rewriting the scene, and **no tool
re-issues a human gate** (C28).

**The control failure was reported, diagnosed and fixed at the cause.** `control-a != control-z` on the
in-Play set, max delta 90, **confined to the phone's own 44 × 21 px box** — the seated pose bit-identical,
so the room never drifted. `ShootArm` writes an emission-only property block, so writing the value back
is not a restore; the applicable local control (`phone-on` == `phone-restored`, bracketing `phone-off`)
passed.

Endorsed specifically, and it is the general rule: **"a control that fails for a known harmless reason
is a control everyone learns to ignore."** Fixed in the second place the defect lived rather than
annotated — and both controls shipped in `frames/` so the claim is checkable. That sentence belongs
alongside C32.

---

## Ordering for the orchestrator

**T68 is the blocker and it outranks everything else in this batch.** The money control has no readable
label at the acceptance view. One change — the label inverts with the field on `actionable` and
`accepted`.

**TV, in order:** T68 → T69 (re-author the leg statement against its measured column) → T67's 40px
strip inset. All three are one file's worth of work and none needs a new capture.

**Room:** R41's luminance restore is the one-line alternative already prepared. R39-am is closed and
needs nothing.

**Gates:** V1 gains the internal-contrast companion (C33-am2). **Every element that inverts reports the
contrast between its field and its own ink** — that is what would have caught T68 two batches ago.

**Owning-doc amendments owed, orchestrator-side:**
- **TV's:** add T68, T69; record T67 closed with its structural guard; amend §6.1 to state that the
  inversion is a two-part operation, field **and** type; amend §9's V1 for C33-am2.
- **Room's (R13):** record R41 closed at the restored luminance, R39-am closed with its disposition,
  and R41-am's swatch-versus-luminance clause.

**Standing:** **C13**, unchanged and still Allen's.
