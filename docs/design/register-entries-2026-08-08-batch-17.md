# Register entries — 2026-08-08, batch 17

**Transcribe into `main-2/docs/design/REGISTER.md`.** Issued at authoring time per C22. Ruled from the
tables (current through batch 16), not from batch files.

New IDs: **S71**, **C33-am3**, **G1**. Closures: **T67**, **T68**, **T69**, **S68**, **S69**, **S70**,
**R41-am**. Ratified-as-built (no change): the receipt footer, the `$0` highlight, the disabled PLACE
fill.

---

## T68 — the blocker. **CLOSED on frames. 1.19 : 1 → 7.95 : 1.**

Verified independently on `01-seated-ACTIONABLE-label-punched.png`, at the acceptance view, in
**linear relative luminance** — the space a contrast ratio requires:

| region | field | ink | CR |
|---|---|---|---|
| **before** (batch 16 frame, same slot) | 0.685 | 0.566 | **1.19 : 1** |
| whole slot | 0.683 | 0.042 | **7.95 : 1** |
| `CASH OUT $199` | 0.637 | 0.038 | 7.77 : 1 |
| `HOLD E` | 0.692 | 0.043 | 7.94 : 1 |

**Both parts of the inversion happen now.** The field takes gold and the type is punched out of it.
Confirmed by eye on the crop: dark ink on gold, unambiguous at 1×.

### The self-correction is the most valuable thing in the submission

The lead **nearly filed the label as unfixed** — on the contact sheet `HOLD E` looked light grey — then
measured it, found dark ink at CR 6.99:1, and reported the near-miss rather than the clean result:
*"The eye was wrong; the measurement corrected it."*

That is C11 running in the direction nobody enjoys. The explanation is also right and worth keeping:
the label reads lighter than the amount because it is **small and thin** — antialiasing and the lit
field's bloom fill in its strokes, so its darkest 2% only reaches 0.350 where the large bold amount
reaches 0.216. **Same ink, different stroke weight, different rendered floor.**

**Carried forward as stated:** rendered ink is 0.222 against an authored `goldInk` of 0.046 — bloom,
antialiasing and T48's black lift all raise it, which is why this reads 8.4:1 and not the 15.3:1 the
authored values predict. **The label is the thinner margin of the two and would fail first if the field
ever got brighter.** That belongs in the gate's line, not just in this batch.

---

## C33-am3 — three instruments, three spaces. **LAW. And I broke it myself this batch.**

**Ruled · DD 2026-08-08.** C33 named the unit for the *ladder*. It did not say that the studio's other
two measurements live in different spaces, and in one batch that omission produced two errors in two
lanes — **one of them mine.**

| measurement | quantity | space |
|---|---|---|
| **brightness ladder** (L0–L4, dominance) | Rec.709 luma | **display-encoded** |
| **contrast ratio** (legibility) | relative luminance, `(L1+0.05)/(L2+0.05)` | **linear** |
| **emission hue / chroma** (palette) | CIELAB | **linear authored** |

**My error:** I computed T68's contrast from Rec.709 luma on display-encoded values and got 3.18:1
against the lead's 8.12:1. **The lead's method was right and mine was wrong** — a contrast ratio is
undefined outside linear space. Recorded per §1.5.

**The room lead's error, same batch, opposite direction:** predicting the lamp's rendered luma by
scaling a display-encoded measurement by a linear ratio — over by 14.2% (predicted +49.02, measured
+56.00), because gamma compresses and rendered luma falls *less* than linearly. Self-diagnosed, with
the two ratios side by side (CIE Y ×0.698, dY′ ×0.797).

**Standing consequences:**

1. **Every measurement states its space, not only its unit.** C33's "quoted with every number" extends:
   unit **and** space.
2. **Luma-parity values cannot be derived by linear scaling.** They need a gamma-aware model or a
   measurement — the room lead's own conclusion, adopted verbatim.
3. **The three ladders are never compared to each other.** The room's EMIT gate already states this in
   its own output (*"Part B's Rec.709 luma is display-encoded and is a DIFFERENT ladder; the two are
   never compared"*). **The instrument had it right before either of us did.**

Fifth reporting axis, after scope (C25), coverage (C28), resolution (C32) and unit (C33): **space.**

---

## T69 — **CLOSED. And the lead is right that the fix is not the answer.**

Verified on the same frames: `MIDDLEMEN ML`, `AUDITORS ML` — one line each, backed side named once, no
wrap. The mid-word cut is gone: `RICO LANYARD TO SCO` → `RICO LANYARD TO`.

Scoping the duplication to Moneyline alone — verified in `MatchModel.DisplayLabel` as
`{Picked} ML — {Away} v {Home}`, with every other market naming no team in its own half — is the
correct blast radius, and **the engine is untouched**: `DisplayLabel` is shared with the console and
the laptop, so it is read and re-authored on this surface. T42's shape, applied without being told.

### The escalation is granted, and it is the right kind

> *"A clean word boundary is better than a split word, but `RICO LANYARD TO` ends on a dangling
> preposition. Truncation cannot produce good copy — it can only stop producing broken glyphs."*

**Correct, and it resolves the tension the lead names.** §5.1 says NEED is *re-authored*, never
truncated; T69 said *truncate on a word boundary*. Those are not equal remedies — **truncation is the
floor, re-authoring is the fix.** T69's clause was the guard against broken glyphs, never a licence to
ship a sentence that stops mid-thought.

**Ruled: leg statements are authored to fit their measured column at the source.** Truncation stays as
the structural backstop and should never be reached in shipped copy. **What a leg statement should say
is a copy decision, and it is mine, not the lead's** — correctly escalated rather than absorbed.

**Owed from this seat, not from the lane:** the statement forms. Filed as **G1** below rather than
guessed at here, because a good short form for `RICO LANYARD TO SCORE` needs the full market list in
front of me and I do not have it in this batch.

---

## T67 — **CLOSED.** The strip's text zone begins at canvas x 305, clear of the 40px reach, verified
with the band lit. Zone ground unchanged, bloom untouched, band not dimmed. Exactly the ruling.

---

## R41-am — the lamp. **Allen's value GRANTED. The lit-read is granted. The residual chroma is accepted.**

Shipped `(0.2334, 0.1924, 0.1769)` — L\* 51.84, chroma 5.4, hue 49.7° — **Allen's ruling of 2026-08-08,
superseding my batch-16 L\*-parity value.** Rendered-brightness parity over L\*-parity.

### The lit-read, which was reserved to me

I looked at `R41-lamp-FOUR-WAY-journey.png` as directed. **It reads as a lit standby lamp.** It is
clearly brighter than the housing around it and clearly warm, and it no longer announces itself as the
loudest colour in the frame. That is what R41 asked for — *"it stays an object; it stops being the only
saturated thing in the room."* **Granted.**

The 14% overshoot is accepted, and accepted for the stated reason: a two-point empirical fit suggested
L\* ≈ 47.2 would land nearer parity, but **that is the same class of reasoning that had just failed,
with one failure and no successes behind it.** Recommending acceptance over spending a slot on an
unvalidated model is the right call and Allen ruled it.

### The residual chroma — my call, and I accept 7.8

Rendered chroma seated is **7.8** against the room's other emitters at 4.9–5.5. Met in kind, not in
band, correctly declined by the lead rather than quietly rounded.

**Accepted, three reasons:**

1. **It was 48.6.** The gap to the band is 2.3 chroma units; the reduction achieved is 40.8. At 0.031%
   of frame, 2.3 units is not a colour event — which was the ruled quantity, not band membership.
2. **Brightness is not the lever** — the lead's own §3 correction: across authored L\* 30.00 / 60.49 /
   51.84 the rendered seated chroma moved 6.5 / 8.0 / 7.8. Closing the last 2.3 means desaturating
   further toward neutral, which trades the lit-read for conformance. **R41 kept the object; that
   trade gives it back.**
3. **The measurement's own spread is comparable to the gap** — 4.0 standing against 7.8 seated for one
   unchanged value, at 187px and 1151px footprints, with hue precision the lead reports as poor.
   **Pinning a band tighter than the instrument's own spread is C32.**

Recording the honest half: the correction *"I earlier reported that chroma tracks luminance — across
three builds it does not"* retracts a claim that had been load-bearing in my batch-16 reasoning. It is
the second self-retraction from this lane in two batches and both improved the ruling.

### R41-am's law, confirmed in place

*When a direction names a swatch, the swatch supplies **hue and chroma**; luminance is the element's
own and does not travel with it.* **Written at the value in code, not only in the batch file** — so the
next person to touch this number reads it there. That is where a law of this kind belongs.

---

## S68 / S69 / S70 — **ALL GRANTED. CLOSED.**

Verified on `03-staged-receipt-lock-enabled` and `11-margin-max-legs-staged-receipt`.

**S68** — `SKIP ROUND — PRESS TWICE` at `.08em`, stamped reasons at `.04em`. `PLACE OR CLEAR THIS
WORKING SLIP` on the max-legs frame reads cleanly at the review distance. **The headroom recovery is
arithmetic and correctly reported as invisible** (C25) — a frame of a string that fits looks identical
to a frame of a string that barely fits, and saying so beats claiming a visual win.

**S69** — measured on `03`: PLACE's disabled fill is **(25,25,18) against a (21,21,13) ground**, and
LOCK carries its 1px `--rule` edge with the reason nested inside. On max-legs PLACE is live wax, so
LOCK's ruled state is visible against an enabled sibling. §2.2 satisfied.

**S70** — the receipt header is the kit's grammar: `TICKET 01` left at action tracking, `2 LEGS` right
as the `key` cell, money facts in the footer row.

**The wax finding inside S70 is better than S70 was.** The header line was drawn entirely in `--wax`
because it carried the payout — **so the ticket's *identity* rendered in the money ink.** Confirmed on
the frame: `PAYS $102` is the only wax on the receipt now (211,160,62), with `STAKE $35` and
`COMBINED +192` in toner. That is S3 enforced in a place S70 never looked.

---

## Three parked questions — **RATIFIED AS BUILT. No change. Recorded so they do not return.**

### 1. The receipt footer's key/value sizes — **stays 13px throughout.**

Measured: keys and values both render **10px of ink** (y261–270 and y276–285, every column). No size
hierarchy. The kit puts values at 16px over 13px keys.

**Correctly parked, and the answer is to leave it.** The receipt is **index, not display** — T29's
distinction on the TV, where resolved rows are index and live rows are display. The margin's
`POTENTIAL PAYOUT` at 31px is where this screen shouts; a second loud payout figure on the same screen
breaks *only one figure at a time may be the loudest*. A printed form sets a key and its value at the
same size, which is what this is.

The reasoning for parking was also right on its own terms: raising only the new row would leave the
footer louder than the legs above it — **S59's exact shape, spotted before building it.**

### 2. The `$0` payout keeps its wax highlight at zero selections.

The kit gates the highlight on `legs.length > 0`; the build always draws it. **Ratified as built**,
on batch 11's precedent — *a sum of zero is money arithmetic*, and the payout slot is the payout slot
whether or not it currently sums to anything. The highlight marks the slot, not the amount.

### 3. The disabled PLACE fill is a supporting channel. **Do not deepen it.**

Measured at **4/255** against the ground — essentially S56's rejected 3/255 chip. So the honest answer
to *"does it read?"* is: **no, not on its own.**

It does not need to. The disabled state is already carried by two channels that do read — the dimmed
label and the stamped reason inside the control — so status-never-colour-alone holds without it, and
S69's PLACE-versus-LOCK distinction is carried by **LOCK's edge**, which is unambiguous.

**Ruled explicitly so it is not "fixed": do not darken the fill to make it read.** A deeper value here
is a new ground, and the palette has three.

---

## S71 — two speakers in one column, three lines apart. **NEW — violation.**

**NEW · DD 2026-08-08.** On `03-staged-receipt-lock-enabled`, the margin's empty state:

```
MY MARKS                    0 SELECTIONS · 1 STAGED
YOUR MARGIN IS CLEAR
```

**`MY MARKS` is him. `YOUR MARGIN IS CLEAR` is someone addressing him.** Three lines apart, in the one
column the owning doc says is his.

Voice §6 is explicit on both halves: copy is impersonal and transactional, it names the thing rather
than the reader, and **second person appears only in genuine imperatives.** `YOUR MARGIN IS CLEAR` is a
statement, not an instruction. §6 is equally explicit that first person appears **exactly once** in the
whole surface and that it is not the product speaking — it is him, in the column he owns. A second
voice in that column is the one place this defect costs the most.

**The kit already carries the model:** *"No marks on this sheet. Circle a price to start a ticket."* —
an impersonal statement followed by a genuine imperative.

**Ruled:** name the state, not the owner. `NO MARKS ON THIS SHEET` — the ownership is established by
the header directly above it and by the biro the column is drawn in.

---

## G1 — leg-statement short forms. **OWED BY THIS SEAT.**

**NEW · DD 2026-08-08.** T69's escalation is granted and the copy decision is mine. **I do not have the
full market list in this batch and I am not guessing at it** — a short form invented per-market from
one frame is how `Atlanta Middlemen ML — Atlanta Middlemen v Tulsa Startups` happened.

**Needed to close:** the authored statement string for every market the TV can show a leg for
(Moneyline, Total Goals, BTTS, Total Corners, Total Cards, Anytime Scorer), with the measured column
width. I will return the authored forms in one pass.

Until then the truncation backstop holds and no shipped statement should be reaching it.

---

## Gate divergence — two ruled values are still reported UNRULED

The room's EMIT gate reports:

```
WindowGlow     UNRULED - no value to compare  [TEXTURED]
ArtIndicator   UNRULED - no value to compare
```

**Both are ruled.** WindowGlow was ratified at **R42 (batch 15)** — *ratified as textured, the map
governs the window's colour, the authored value is a multiplier and stays near-neutral*. ArtIndicator
was ruled at **R41 (batch 16)** and amended by **Allen (R41-am)** to the value the gate is printing.

**The gate's behaviour is correct** — it reports rather than passing, which is C28 exactly, and it is
why this was visible at all. **The divergence is bookkeeping: the register moved and the gate's ruled-
value table did not.** Register both, with R42's multiplier clause attached to WindowGlow so a future
saturated value there fails rather than passes as "textured".

**Standing, and it is now three batches old:** room gates **6, 7 and 8 are VOID** — a human certified
them against a scene whose content fingerprint no longer matches, and **no tool can re-issue them**
(C28). The instrument is a human walking the room. Every room batch since has shipped with three gates
unassessed, correctly reported each time. **That walk belongs on the schedule.**

---

## Ordering for the orchestrator

**Nothing in this batch is blocking.** T68 closed; the TV has no open build item.

**SureThing:** **S71** is one string. That is the whole SureThing queue.

**Room:** register the two ruled emission values in the gate's table. Schedule the **R22 human walk** to
clear gates 6/7/8.

**This seat:** **G1** — the leg-statement short forms, owed by me, needs the market list.

**Owning-doc amendments owed, orchestrator-side:**
- **TV's:** record T67, T68, T69 closed; add to §9's V8 that **the label is the thinner margin and
  fails first if the field brightens**; add T69's *authored-to-fit, truncation-as-backstop* clause to §8.
- **SureThing's:** add S71; record S68/S69/S70 closed and the three ratified-as-built answers, so the
  footer sizes, the `$0` highlight and the PLACE fill are not re-opened.
- **Both, plus room's:** C33-am3's three-space table.

**Standing:** **C13**, unchanged and still Allen's.
