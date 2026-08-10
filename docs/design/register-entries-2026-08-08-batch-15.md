# Register entries — 2026-08-08, batch 15

**Transcribe into `main-2/docs/design/REGISTER.md`.** Issued at authoring time per C22. Ruled from the
tables (current through batch 14), not from batch files.

New IDs: **S68**, **S69**, **S70**, **T67**, **R41**, **R42**.
Closures: **C15**, **S20**, **S28**, **S29**, **T63**, **T64**, **T65**, **T66**, **R39**, **R40**.
Amendments: **C33-am**, **R39-am** (instrument), **R40-am** (my premise), **L2** (gate instrument).
Re-verified: **S8**, **S52**.

---

## C15 Phase L — the TMP migration. **GRANTED. Every signed type deviation expires.**

Verified on `final/` (Regular 400, S29 closed), against the pinned UGUI `before/` set — the first
before/after pair in this project's history where both halves are C34-compliant.

**Measured, every product-fact slot, before against after:**

| slot | before (UGUI) | final (TMP Regular 400) |
|---|---|---|
| `NO. MATCHUP · SEASON RECORD` | 9px ink, y147–155 | **9px ink, y147–155** |
| `ROUND 1 OF 8 · PRICES FINAL` | 9px, y112–120 | **9px, y112–120** |
| season record `4-5` | 9px, y182–190 | **9px, y182–190** |
| `POTENTIAL PAYOUT` | 9px, y346–354 | **9px, y346–354** |
| `DISK 61% FULL` (12px chrome) | 9px, y682–690 | **9px, y682–690** |

**Identical ink heights at identical scanlines through a complete text-stack replacement.** Not one
product fact moved a pixel. That is what a 1:1 migration looks like and it is rare.

**Expiring now, as requested:**

- **S28** — tracking. Spent across six groups. **Closed.**
- **S29** — tabular figures. **Closed** — see below; the close is better than the deviation was.
- **S20** — weight 600. **Closed**, spent on one word.
- **Markets' ladder letter-spacing deviation** — dissolves at migration per C15. **Closed.**

**S52 re-verified:** rail band **0 differing of 34,816**; tray **0 of 22,576**. One chrome consumed
twice, through a type-stack replacement, a weight change *and* a face change. **S8 re-verified on this
evidence** — the OS chrome's Design-verified status stands.

### S29's close is the model

The roman figures were tabular **by accident** — Archivo's default face is SemiBold, near-tabular at
spread 0.1875. Correcting the roman voice to Regular 400 exposed a proportional spread of 4.7656
(1.112px at 21px), and TMP cannot fix it: the face declares no `tnum`.

**The fix was to read the owning doc, which already assigns both "figures" and "masthead" to the
condensed face** — spread 0, every digit 41.05. Measured on the frame: run figures 389px → 330px, right
edge held at x1006, inside a fixed band. **A conformance gap closing, not a redesign** — and the third
time this fortnight a lead has resolved an apparent design question by finding it already answered
(S64, S29, and S68 below).

Also recorded, because it corrects the register: **the old "spread 0" reading in S29 was measured
against the wrong face.** The deviation was signed on a number taken from a weight nobody had chosen.

### S20 — the one deliberate weight

The rail identity held **30.65% → 30.65%, 0.00pp**, while every other roman element dropped 3.6–4.6pp.
**It was 600 by accident and is now 600 by choice, and it is the only roman element that did not
lighten.** Confirming a weight ruling by predicting which single element would *not* move is a better
instrument than measuring the one that did.

---

## L2's gate — its instrument cannot resolve the thing it checks. **AMENDED.**

The lead has carried "the 13px fact floor is source-checked, not frame-checked" as an owed item across
three submissions, honestly, and named it as the one L-gate the phase had not touched. **I have now
tried to frame-check it and it cannot be done as the gate is written.**

Archivo's cap height is 0.73em. At the canvas-local 1024×704 resolution:

- **13px → 9.49px cap → renders 9px of ink.**
- **12px → 8.76px cap → renders 9px of ink.**

**Both floors produce the same measurement.** The room camera is coarser still. So the L2 gate as
written in the owning doc — *"no text below 13px states a product fact; instrument: rendered
measurement at review distance"* — **specifies an instrument whose resolution is coarser than the
distinction it exists to make.** That is C32 exactly, and this time the gate is mine.

**L2's instrument amends:** the gate checks **which constant each slot uses**, names the slots it
covers, and states in its own line that **it cannot separate 12px from 13px on a rendered frame.**
`MakeText`'s `Mathf.Max(13, …)` clamp is the instrument, and the lead's caveat that TMP point size and
UGUI pixel size are not the same quantity is the right one to carry — the clamp is checked against the
face's own metrics, once, at design time.

**The owed item is retired, not deferred.** It was never obtainable, and three submissions carried it
as a debt because I specified it wrong.

---

## S68 — the tracked-label treatment reached factual copy. **RULED — conformance, and the kit answers it.**

**NEW · DD 2026-08-08.** The orchestrator routed the SKIP ROUND headroom collapse (45px → 7px) as *"a
design call"*. **It is not a design call — it is the same shape as S64, and the kit already specifies
the answer.**

Measured on the final lobby frame, margin action stack:

| string | before | final |
|---|---|---|
| `SKIP ROUND — PRESS TWICE` | ~189px | **~228px** |
| `PLACE TICKET` | ~97px | **~122px** |
| `LOCK IT IN` | ~68px | **~91px** |

S28's tracking became reachable at migration and was applied at the **action-label** value across the
stack. **The kit does not specify that value for these strings:**

- `SkipAction.jsx` — `letterSpacing: var(--st-track-rec)` = **.08em**, not `.14em`.
- `StampReason.jsx` — `letterSpacing: ".04em"`.

And the owning doc §4.3 states the principle the kit is expressing: **short labels are tracked
uppercase; factual copy stays literal.** `SKIP ROUND — PRESS TWICE` is a label *plus an instruction*;
`PLACE AT LEAST ONE TICKET` is a label *plus a remedy*. Both are factual copy. Tracking a
cause-and-remedy sentence to a label's value degrades the legibility of the exact string T47 and S43
exist to make readable.

**Ruled: apply the kit's values — `.08em` on SKIP, `.04em` on stamped reasons.** The headroom recovers
by construction (roughly 17px on the SKIP line), **no authored string is shortened and no geometry
moves.** T24's rule holds in both directions: authored strings do not bend to measurements, and a
measurement that leaves 7px of 296 is a coincidence, not a fit.

**Why this outranks its size:** S50's yield order is *spacing, then repetition, then nothing*. This
deficit was created by spending spacing in a place the kit did not authorise, and it was about to be
paid for out of a string.

---

## S69 — the disabled action grounds are inverted against the kit. **RULED — conformance, low priority.**

**NEW · DD 2026-08-08.** On the lobby frame, empty slip: **LOCK IT IN carries a `--ground-3` fill and
no border; PLACE TICKET carries neither.** The kit is the other way round —
`PlaceAction` disabled fills `--ground-3`; `LockAction` disabled is transparent with a `1px --rule`
border, and the owning doc §2.2 calls LOCK *"a 52px ruled control in both states"*.

**Present in the `before` set too — this is not a migration regression** and it does not disturb the
Phase L grant. Filed because the ruled distinction is doing real work: PLACE is a field that has gone
inert, LOCK is a rule that has not yet been earned. Sequence it behind S68; they are one commit in the
same file.

---

## S70 — the three untracked kit values. **RULED.**

**NEW · DD 2026-08-08.** Left untracked rather than guessed, which was right — inventing a token to
paper over an unmatched value is how a design system stops being the authority.

1. **`LedgerEntry.jsx:17` `.02em`** on the legs line — **becomes `--st-track-name` (.03em)**. It is a
   run of names and prices, which is what that token is for; `.02em` matches nothing and the 0.01em
   difference is below anything this surface can resolve.
2. **`OsRail.jsx:17` `.13em`** on the identity mark — **stays, and gets a token: `--st-track-chrome`
   (.13em).** The rail is OS chrome, not product copy; it is the one element carrying S20's deliberate
   600 and it is legitimately its own thing. A named exception with one member is still named.
3. **The staged-receipt header** — the build renders one string where the kit splits it across three
   trackings. **The kit is right and the build conforms to it.** Three trackings on one header line is
   the receipt's grammar (identity / count / state), and collapsing it to one string loses the
   distinction between what the house printed and what the sweat added.

---

## T63 — the last 0.029. **RULED (c), and my batch-13 ruling was impossible as stated.**

### The structural half is granted, and it re-reads the original finding

The HDR material sat on the money **figure** and never on the gold **field**. Splitting the zone:
**field 0.696, figure 0.827.** So the 0.827 batch 13 measured *was the figure* — and the field, which
is what reads as "the band" at four metres, was **the dimmest of the four competitors, not the
third-brightest.** My finding was right about the ordering and wrong about which object it had
measured. Granted, fixed, proven: field 0.696 → 0.746, band peak 0.827 → 0.844.

### The remaining gap cannot be closed, and that is arithmetic

I verified the lead's claim independently. Rec.709 weights green at 0.7152. Cold white `flavorColor`
(0.90, 0.95, 0.98) computes to **0.9415**. To reach it with gold's R=1.0 and B=0.18 requires
**G = 1.0016** — outside the range a `Color32` canvas colour is clamped to. **Within the clamp, no gold
out-ranks cold white in this unit.** Not difficult. Impossible.

**That makes my batch-13 ruling — "the actionable cash-out band renders above the quiet scoreline" —
unachievable as written, and it is the seat's error (§1.5).** C16 is explicit that only the platform
makes a thing impossible; `Color32` packing and the luma coefficients are the platform. I set a target
without deriving whether the ruled unit permitted it, one batch after ruling the unit.

**Fourth instance of the same seat error** — T20, T47, T51, S59, now this: *a bound is not a layout,
and a target is not a check that the target is reachable.*

### Ruled: (c), and reframed so it is not an acceptance of a defect

**(a) unseal the boost — refused.** T49-cl is sealed with *"bloom was never the lever"*, and §4 shows
this element is already reading partly by washing its neighbours. More bloom is the wrong direction.
**(d) lower the scoreline — refused**, as the lead recommended: the score is the element §4.1 says
nothing may outgrow, and dimming it so money wins inverts the law rather than satisfying it.
**(b) a lighter gold — not now.** It stays in TV's §10 quarantine where it already sits, because it
moves the money colour on every surface that shares it, and 0.029 is not a reason to.

**The band is the surface's L4 element and it reads as one.** The 0.029 deficit is an artefact of the
comparison, not a property of the composition: **it compares a 588 × 115px solid field's peak against
thin glyph strokes' peak.** The field's zone mean is 0.746; the scoreline's zone is mostly substrate.
A large lit field at 0.844 dominates thin type at 0.873 — which is exactly what §4's bloom finding
demonstrates from the other direction, and what the seat's own read (c) was pointing at.

### C33-am — peak luma cannot see which element dominates

**Amended · DD 2026-08-08.** C33(b) said a per-element value check cannot see a ranking. Extended, from
the same failure one surface over:

**Where an L4 candidate is a filled field and its competitor is type, dominance is judged on zone mean
and peak together, and the gate reports both.** Peak-versus-peak silently compares a field against a
glyph, and a field always loses it while dominating the frame. TV's gate **V1** takes both columns.

This is the same class as C33(a) — an instrument that mis-ranks the specific thing this project cares
about — and it is the second time the ladder's unit has needed correcting to see money properly.

---

## T67 — the lit band blooms into its neighbours. **NEW — named, judged in the room.**

**NEW · DD 2026-08-08.** Raised as *"a consequence needing a ruling either way"*, correctly.

With the field lit, bloom enters neighbouring boxes: **event strip peak 0.626 → 0.833, risk/pays 0.430
→ 0.840, both taking the field's hue.** The elements are not repainted.

**Risk/pays is not a breach** — it is gold at L2 and gold entering gold is not a ration event.

**The event strip is the live question.** It is cold white and T27 requires *the bar carries no hue*.
An event strip visibly taking gold on every actionable frame is the gold ration reaching an element the
law keeps neutral — T40's shape, arriving as bloom rather than paint.

**Ruled: judged at the seated in-room render, not on the flat panel.** This is what owning doc §1.3 is
for — *a flat capture is a design reference; the in-room render at the seated camera is the only valid
acceptance view.* Bloom through real glass at four metres is precisely the thing the flat panel cannot
answer, in either direction. **Do not act on the panel-local number**, and do not tune bloom (sealed).

**Owed:** an actionable-cash-out frame at the seated camera, with and without the band lit. If the
strip visibly warms at that distance, the remedy is separation — a gutter between the field and the
strip — never a bloom change and never dimming the band this ruling just granted.

---

## T65 — the room re-tint. **CLOSED on frames.**

| state | hue | sat | Rec.709 |
|---|---|---|---|
| pre-fix `LegFinalWon` | 40.7° | 71.1% | 0.347 |
| **post-fix `LegFinalWon`** | **130.4°** | **40.4%** | **0.175** |
| post-fix resting | 130.4° | 40.4% | 0.175 |

**Eight `LegFinalWon` frames across two scenes read identically to rest.** The room does not move on a
leg win.

**The mechanism is causal, not inferred:** `WonLegBeat` fired `tvLight.Flash(gold, 3.0f)`, and gold's
hue computes to 39.6° against a measured room of 37.5–40.7°. **Fixed by rule** — one painting point,
`RoomSettlementGlow()`, settlement only, carrying a room-palette warm, and **no call site names a
colour.** That last clause is the part that stops it recurring.

**Accepted as an upper bound:** the new re-tint has not been photographed firing, because no settlement
moment exists in the harness's named-moment list. The value (hue 88.0°, intensity 0.9) is provisional
until a settlement capture exists. **Owed, does not withhold** — C17's shape, and the closure is on the
defect's absence, which eight frames do establish.

### The correction to my own ruling

**My four "wall" regions are the TV's own riveted housing.** The lead's boxes reproduce my numbers to
the digit and rendering them shows rivets. Recorded as the seat's error (§1.5) — I named four regions
from coordinates without rendering them, which is the check I demanded of the T63 boxes one batch
later.

**The conclusion is unaffected, and the lead's reasoning is why:** red gain falls off across the right
margin (+44.5 → +20.0 → +9.5 over three bands) — a point light's profile — and the one surface facing
away does not respond at all (+1.8). **A housing artefact would not fall off with distance and would
not spare the away-facing surface.** It was a room event measured on housing, not a housing artefact
mistaken for a room event.

**T64 landed** (flicker deletions in). **T66 landed and is verified:** event strip 0.858 → 0.626, hue
unchanged at 199°, built as **one painting point** with all 14 assignments routed through it — the
structure the ruling asked for, not fourteen edits.

---

## R39 — the phone's emission. **EXACT VALUES GRANTED. CLOSED.**

The instrument I held these for now exists, validated itself, and delivers.

| emitter | pose | footprint | hue | chroma |
|---|---|---|---|---|
| **ScreenPhone** | standing | 608 px | **85.4°** | **5.0** |
| **ScreenLaptop** | standing | 3460 px | **84.3°** | **5.3** |
| ScreenLaptop | focused | 51.18% | 85.0° | 5.5 |

**His two screens land on one chromaticity in render — 85.4° against 84.3°, chroma 5.0 against 5.3.**
That is what *"joins the laptop's granted family"* was ruled to mean, and it could not be shown until
this set existed. Granted.

**The structure is why this closes rather than needing re-checking:** the three phone states are
`Amp(1/3/15)` off **one shared base — `LaptopScreen.GrantedLidEmission`** — so `R ≥ G > B` and a single
hue hold **by construction**, and the material and the component cannot drift. `HousingSteelMat()` is
the cited precedent, and R19(b) is the case that earned it. The amplitude ladder is preserved
(ΔL\* +1.03 / −0.30 / +0.26): **a hue change and not a value change, answered by construction rather
than by assertion** — which is R35's caution met exactly.

**The `ScreenPhone` material was folded in and was not named in R39.** Right call: it carried the
identical blue, and striking the runtime field while leaving the material would have left the authored
blue in place for precisely the audience R40 is about.

**`design/08` deleted as an authority** from `GrayboxRoomBuilder` and `PhoneScreen`'s class summary,
with the values it licensed.

**Endorsed specifically:** reporting `ScreenPhone` as *"matches proposal"* and never PASS, because I
held the value for this instrument — *"a green there would be the gate ratifying my own guess."* That
is a gate declining to launder its author's proposal, and it is the cleanest C18 §4.2 discipline this
studio has produced.

### R39-am — the instrument line is struck

**Amended · DD 2026-08-08.** R39 says *"unlike the lid, these are observable, so the frame is
obtainable."* **On the lead's own correction that does not hold in Play Mode.**
`PhoneScreen.BuildSkeleton` puts a world-space canvas **1.5mm above the emissive quad with an opaque
backing — structurally the same arrangement as the lid**, which is what made the glow cue
unphotographable. In Play Mode that region reads the canvas, not the emitter.

**The batch-13 line R39 cited — "the rendered reading confirms the authored one", from L\* 16.66 →
36.31 — is struck.** It was Edit-Mode-dark against Play-Mode-lit: emission-off/on conflated with
canvas-absent/present. The lead caught it, in their own filed finding, in the submission that depended
on it.

**Instrument choice, since I was asked to make it and not to duck it: the in-Play A/B**, on the
`CaptureLidEmissionInPlay` model that freezes everything but the value under test. Edit Mode has
already given me the authored-value conformance; what is unknown is whether any of this reaches the
player.

**Disposition pre-committed, so nobody waits twice:** if the phone's emission is as unobservable at
runtime as the lid's, **the granted colours stand** — they are authored values that govern Edit-Mode
captures, the material and every future bake-adjacent path — **and no cue, state or gameplay signal is
ever built on the phone's glow.** Same disposition as the lid, for the same measured reason.

---

## R40 — the laptop material. **CLOSED. My bake premise is falsified; the conclusion stands.**

The material is corrected to the granted value from the same shared constant. **Closed.**

**R40-am — the seat's error (§1.5).** R40 asserted *"what does see the material's own value: the APV
bake, and every Edit-Mode capture. So it has been baked into the room's indirect light."* **The bake
half does not survive measurement.** Every ratified region held to within ΔL\* 0.13:

wall right 12.68 → 12.71 · wall far 13.29 → 13.42 · floor 13.04 → 13.07 · bunk 1 14.07 → 14.15 ·
mattress 20.15 → 20.22 · ceiling 11.18 → 11.23 · laptop body control 53.11 → 53.06.

And the mechanism agrees: `Mat()` sets `RealtimeEmissive`, whose own comment says it *"bakes nothing —
this project has no lightmaps"*; `PhoneBuzzLight` is Realtime. **I asserted a bake path from the
existence of a bake, without checking whether these emitters feed it.**

**The Edit-Mode half was the load-bearing half and it is correct** — the green material is what every
Edit-Mode capture rendered, including the captures that settled the lid colour a batch earlier. R40's
general form stands untouched: *a runtime override that hides a wrong authored value does not make the
value right — it makes it invisible to the one audience that can report it.*

**Endorsed, and it is the harder call:** the APV payload bytes *did* change, and the lead declined to
read that as counter-evidence, citing this project's own measured case of the builder emitting three
md5s for identical content. **Refusing to treat a byte diff as a finding, in the direction that would
have vindicated the DD, is the right instinct** and it is why §9.2 can never be a byte comparison.

**Consequence adopted:** a future emission-only change does not need the bake. It still voids gates
6–8 — through the builder rewriting the scene, not through the bake — and no tool re-issues a human
gate (C28).

---

## R41 — ArtIndicator is the only saturated emitter in the room. **RULED — struck as a colour, kept as an object.**

**NEW · DD 2026-08-08.** The lead measured both numbers and explicitly declined to weigh them, which
was correct — the area is the mitigating fact and the hue is the violation.

| pose | footprint | hue | chroma |
|---|---|---|---|
| standing | 185 px (0.005%) | 41.8° | **43.0** |
| seated | 1487 px (0.040%) | 40.6° | **48.6** |

Authored `(0.85, 0.14, 0.08)` — **chroma 63.1**, the same magnitude as the struck laptop violet's 64.1.

I ruled this on `ArtIndicator-seated-ON_OFF_DIFFx6.png`, as instructed. **It reads as a small standby
lamp on institutional equipment**, which is diegetically ordinary for T1's register — a piece of
maintained industrial kit with a power lamp. That case is real and I am not dismissing it.

**It loses on scarcity, not on area.** Every other emitter in this room has been driven to chroma
5.0–5.5. A chroma-48 lamp is **ten times more saturated than anything else in the room**, which makes
it the loudest colour event in the frame regardless of how few pixels it occupies. This project
rations gold on the TV for exactly this reason: scarcity is what makes a colour mean something, and an
unruled decorative lamp spends it.

**And C4 and T34 leave no room.** Red is retired game-wide. T34's own words name this precise gap:
*"no red exception is granted anywhere … Red lives in light, which no scan covered."* Granting one here,
one week after ruling that, would make T34 a preference.

**No exception is needed, because the palette already holds the answer.** `--room-rust #6B3A24` is
ratified room law — warm, dark, low-chroma, and what a standby lamp actually looks like. R35's shape:
strike the requirement that needs an escape, apply the swatch that exists.

**Direction only, exact value on the instrument that now exists:** the room's warm family — the rust
end or the screens' 83–85°, **never the signal-red end** — with chroma bounded against the room's other
emitters rather than by eye. It stays an object; it stops being the only saturated thing in the room.

---

## R42 — WindowGlow. **RULED — ratified as textured; the authored value is a multiplier.**

**NEW · DD 2026-08-08.** Authored 290.5°, rendered 77.0° — a 213° gap that is **not a defect**, and the
lead was right not to report it as one. WindowGlow is the room's only emitter with an **emission map**,
so it emits colour × texture and what reaches the frame is the generated night-city map's sodium.

**Ratified as it stands.** §1.2 sanctions a cool window with short reach, R24 amended the contract to
*"a high-tech city at night — sodium and office-block light, its neon distant and unresolved"*, and the
rendered sodium is that contract. **The map governs the window's colour.**

**One clause so this cannot drift:** on a textured emitter the authored value is **a multiplier, not a
colour**, and it stays near-neutral — 290.5° at chroma 3.9 qualifies. A saturated multiplier there
would tint the whole skyline and no palette audit would see it, because the audit reads the authored
value and the frame reads the product.

**The gate now detects emission maps and annotates those surfaces**, which is the instrument knowing
its own limits — adopted, and it is what kept a 213° non-defect off my desk as a defect.

---

## The emission instrument — adopted as the room's standing instrument

**Three runs, two defects, both in the instrument and neither in the room.** A still-settling
reference, then a "restore" that reset renderers to their shared-material value instead of their own
state. It earned its numbers before it reported them.

**What makes it usable, and what other instruments in this studio should copy:**

- **`control-a == control-b == control-z`, bit-identical on every pose.** The opening pair shows the
  pipeline had settled; the closing one shows the room ended as it began, so **no capture mutated the
  scene for the captures after it.** Nothing else in this project has demonstrated that.
- **An independent cross-check that caught run two:** every untextured emitter's isolated contribution
  must carry its own authored chromaticity. Run two had the laptop at 248.9° against an authored 83.3°.
- **A coherence check the lead trusts most, and so do I:** the lid reads **0 px in the seated pose** —
  out of frustum, so it cannot change a pixel — and **51.18% in the focused pose**, which it fills.
  Both were wrong in both earlier sets. A gate that predicts a zero is worth more than one that
  measures a value.
- **Anything below 2 code values is reported UNCOVERED, never "clean"** — C32 applied by a lead,
  unprompted, against their own instrument.
- **`crops/` as ON | OFF | DIFF×6 around the emitter's footprint**, because a 608px contribution is
  invisible in a 2560×1440 frame. That is C11 taken seriously: the view at which a ruling is *possible*
  is part of delivering the evidence.

**Adopted as the room's emission gate.** It closes the channel batch 14 named as the largest uncovered
one in the project — three defects in eight days, three surfaces, none found by a gate. **This is the
first instrument in the studio that reads light rather than pixels or constants.**

---

## Housekeeping

**TV's owning document is Allen-approved (2026-08-07).** My draft's status line is stale and is
corrected in `tv-design-2026-08-08.md` — status ratified, T63/T64/T65/T66 moved out of §10 into the
body, T67 added, gate V1 amended for C33-am, and R42's textured-emitter clause cross-referenced.

**Owning-doc amendments owed, orchestrator-side:**
- **SureThing's:** add S68, S69, S70; **amend §9's L2 gate** to the instrument ruled above; record
  C15/S20/S28/S29 closed and the type deviations expired.
- **Room's (R13):** add R41, R42; record R39 and R40 closed, the emission instrument adopted, and
  R40-am's falsified bake premise.

---

## Ordering for the orchestrator

**SureThing:** S68 (kit trackings — recovers the SKIP headroom by construction) + S69 + S70 are one
commit in one file. **Nothing is a design call; all three are conformance to the kit.**

**TV:** T63 closes with no further build. **Owed: one seated-camera frame pair, band lit and dark**
(T67) — that is the only open TV evidence, and it is a capture, not a change.

**Room:** R41 (indicator into the warm family) on the new instrument. Then the in-Play A/B for the
phone (R39-am) — disposition pre-committed, so it needs no second ruling from me either way.

**Standing:** **C13.** Still live, still on this batch's frames, and it has now contaminated three
consecutive rulings' evidence by the room lead's own count. It is the oldest open item touching two
surfaces and it belongs on Allen's list, not mine.
