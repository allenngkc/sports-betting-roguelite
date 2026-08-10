# Register entries — 2026-08-09, batch 20

**Seat:** Design Director (`main-2` terminal) · **Source:** `dd-import/blur-bundle-2026-08-09/`
(README, `crops/split-allen-display-vs-harness.png`), Allen's frames at
`dd-import/allen-playtest-2026-08-09/` (both read at review distance by this seat, 2026-08-09),
SureThing's separate-canvas observation (`PhoneScreen.cs:61-62, :165`).

**What this batch rules:** the blur's build-side half, the two instrument laws, and the design
consequences that do not depend on the open half. **Allen's check-3 shot and the phone's own
reference frame ride.** New IDs: **C36, C37, C38, S2-am, C26-am3**; **S71 closes**.

> ## AMENDED — 2026-08-09, later the same day: the open half closed
>
> Allen found the display-path cause himself: **Unity Game view "Low Resolution Aspect Ratios" was
> on.** Off, his verdict is **"everything is clear now" on both surfaces**. The acceptance bar — his
> eye, at the desk — is **MET**.
>
> Amended in place rather than shipped as batch 21, **per C22**: this file has not been transcribed,
> so it is still a draft, and transcribing a row that reads OPEN beside a row that closes it in the
> same commit serves nobody. **Nothing below is deleted.** The pre-closure text stands as written and
> the closure is appended to it, so the trail shows what this seat ruled *before* the cause was known
> — which is the only thing that makes §0's disposition audit worth anything.
>
> **Changes:** C38 **CLOSES** (§C38-cl). S2-am is right-sized by **S2-am2** — clause 3 retires, clause
> 4 discharges. C26-am3 **stands on a restated reason** (its trigger was spent by the fix; the reason
> that carries it was never about sharpness). **C39** is added — the law this hunt actually bought.
> **C36 and C37 are untouched**, which was pre-committed and is worth noticing: both survive the
> disappearance of the defect that produced them.
>
> Final new-ID list for transcription: **C36, C37, C38 (opened and closed), C39, S2-am, S2-am2,
> C26-am3**; **S71 closes**.

---

## 0. Disposition audit — which pre-committed branch fired

Filed before the bundle existed (`blur-disposition-precommit-2026-08-09.md`), four branches keyed on
Allen's display-path arm. Audited first, so what follows is visibly bound by what was written in
advance rather than fitted to the result.

- **Branch 3 — the instrument convicted — is FALSIFIED, and it was the expensive one.** The room
  lane's own pre-committed three-outcome test decided it in the one direction that exonerates:
  Allen's path measures **0.613** ramp÷stroke against the harness's **0.482**. An instrument that
  reads *sharper* than the path the complaint came from cannot be manufacturing the softness.
  **Nothing in the hunt retracts.** Batch 15's L2 finding does not return to unadjudicated on
  instrument grounds — and §3 below explains it rather than reopening it.
- **Branch 1 — real and shared-path — fires on the LOCUS axis** (C38·b).
- **Branch 4 — invalid null — does not fire here**, but the law behind it was independently
  rediscovered by the room lane against its own numbers and is promoted below as **C37**.

Two independent pre-commitments, one from each seat, resolved this without a second round. Recorded
because a pre-commitment nobody audits afterwards is a comment.

---

## C36 — A control must bracket the interval it certifies

**Law** · DD 2026-08-09, promoted from the blur hunt (adopted at batch 19 ahead of its founding
case; the case is what promotes it).

**A control certifies only the interval its samples enclose, and it is checked by the other half of
the instrument — never asserted by the half being checked.** An opening control pair brackets the
warm-up and nothing after it. A closing control is what certifies the run.

*Founding case:* an emission set passed `control-a == control-b` **while the room was being mutated
underneath it** — a capture step was resetting renderers to their shared-material value instead of
restoring their own state, so every later frame was shot against a changed room. The opening pair
could not see it by construction; a closing `control-z` catches it. Two capture sets were discarded
learning this.

Sits with C18 §4.2: this is the temporal form of a gate stating what it cannot see. A control that
brackets only the beginning cannot see the middle, and its green says so if anyone reads it.

## C37 — A null is invalid unless success would have been resolvable

**Law** · DD 2026-08-09, promoted from the blur hunt. **Extends C32** (a gate states its
resolution) from positive results to negative ones — C32 governs what a gate reports; C37 governs
when a gate's *"no effect"* is allowed to mean anything.

**Before a null is recorded, the instrument must be able to resolve the success it was looking for.
Where a successful outcome would land under the instrument's own floor, the test could not have shown
success in either direction, and its null is void — not a pass, not evidence of absence.**

*Founding cases, both the room lane's own numbers:*
- whole-pixel ramp counting carried **±25% on a 2 px ramp**, so a three-point trend built on it was
  never a trend;
- the `_Sharpness` null at render scale 1.0 was **invalid**: a successful halving lands at 0.84 px,
  *under* the ~1 px single-sample floor. That arm could not have shown success however the code
  behaved. Caught by SureThing, not by the lane that ran it.

**Two verdicts were un-retired on this law** — its first application recovered work rather than
discarding it, which is the correct shape for an instrument law and worth recording as such.

## C38 — The output-resolution softness floor

**Ruled — build-side half CLOSED; display-path half OPEN** · DD 2026-08-09. Cross-surface: it is
below every canvas and belongs to none of them.

**(a) The floor is real, and it is the build's.** ~1.6 px screen-space edge ramp, independent of the
instrument that found it. The harness is **exonerated by direction**, which is the strongest form
available: it reads sharper than reality, so it cannot be the source. The three-outcome test was
pre-committed before Allen's frame existed and two of its three outcomes would have retracted the
hunt — endorsed as the standard for any instrument auditing itself.

**(b) The locus is below the canvas.** Three lines, from three directions:

1. **Fixed in screen pixels** — 1.679 / 1.683 / 1.743 px across 1.00× / 1.25× / 1.50× magnification
   while the stroke grew **36%**. *Measured.*
2. **Geometry is floored alike** — a hard geometry edge narrowed only 0.912× at 1.5× where physics
   requires 0.667×. Not a type, SDF or atlas phenomenon. *Measured.*
3. **A second, separate world-space canvas blurs identically** — the phone
   (`PhoneScreen.cs:61-62, :165`), different canvas, different content, different material instance.
   *Observed, not measured* (the phone has no reference frame yet). **Corroborates; carries no load.**
   Lines 1 and 2 are measured and are sufficient on their own.

**The positive result outranks the six exonerations and subsumes them.** Anything applied before or
during the resolve is supersampled and must narrow; this does not, so it is applied after the resolve
at output resolution. That excludes the entire pre-resolve class *as a class*. Two consequences,
recorded as the general form: the untouched `PC_RPAsset` global volume (Bloom, Vignette, Tonemapping
— §4's own correction) is **not a hole**, and the withdrawn bitmap exoneration **reopens nothing**.
**An incomplete elimination inside a class a positive result has already excluded is bookkeeping, not
a gap.** Both corrections were filed unprompted and neither cost the conclusion anything.

**(c) `_Sharpness` is not the remedy, and no single constant is.** 0.00 → 1.00 buys **9.6%**,
monotonic, against a requirement near 50%. **C10 decides it** — an effect that fails on mechanism is
disabled and re-scoped, never tuned toward invisibility. The lane's own "the fix is not one constant"
is adopted verbatim as the ruling.

**(d) What is NOT ruled, and what that forbids.** The cause of Allen's additional ~56% is
unidentified; four checks are with him and **check-3 (the same shot at 100% scaling) is decisive**.
Until it lands: **no fix is sized, scoped or scheduled against the 56%, and no type is re-authored
against any number containing it.** Sizing work against an unattributed quantity is the shape of
S51's three refused candidates.

**(e) The acceptance condition, stated in the channel the defect lives in.** The mechanism's
signature — a ramp fixed in *output* pixels that survives supersampling — is a **resample's**
signature: the surface is being resampled rather than rendered at output resolution. The engineering
route is not mine. The acceptance condition is: **ramp ÷ stroke at the ratified acceptance view,
measured on the player's path, reported with its view and its space (C33-am3).** Not "sharper", not
a `_Sharpness` value, not an eye. This surface has spent a fortnight learning that an eye at 4× is
not a measurement (§4's own withdrawn ink-ring claim is the latest instance).

## C38-cl — The item CLOSES

**CLOSED** · DD 2026-08-09, same day, on Allen's own finding and his acceptance verdict.

**The cause of the display-path half: Unity Game view "Low Resolution Aspect Ratios" was on** — an
Editor view setting that renders at reduced resolution and upscales to the view. Off, Allen's verdict
is *"everything is clear now"* on both the laptop and the phone.

**The prediction and the cause match, and that is the strongest confirmation available.** §1 of the
bundle reasoned from arithmetic alone that the extra softness "points at an upscale," and put it at
**1.97× the geometric prediction — almost exactly a factor of two.** Low Resolution Aspect Ratios is
a literal halving. **A number derived before the cause was known, landing on the cause when it
arrived.** Recorded as what a well-formed instrument buys: the lane could not see the toggle, and its
arithmetic described it anyway.

Ruled:

1. **The build's ~1.6 px floor is a measured CHARACTERISTIC, not a defect.** The bar this studio
   rules against is the player's eye at the acceptance view (C11), and that bar is met on both
   surfaces by the only person who can set it. A number that is real, measured and below the bar is a
   property of the pipeline, not a fault in it.
2. **`_Sharpness` stays unspent.** It is a real lever worth 9.6% and it is now **reserve**. Spending a
   lever on a bar that is already met leaves nothing in hand when something genuinely regresses; C10's
   original reasoning is superseded by a better one — there is no longer a mechanism failing to fix.
   Not touched, not tuned, and its value is not changed on this ruling.
3. **No fix is scoped, and the six exonerations, the harness audit and the two discarded capture sets
   are not waste** — they produced C36, C37, C39, a real defect fixed on the way (`_TextureWidth`/
   `_TextureHeight` mirrored 1×1 against a 1024² atlas), and a characterized pipeline with a number
   in it. The cost is recorded honestly in C39, which is what it bought.
4. **C38 closes. What closure does not mean:** the floor is *characterized*, not removed. It is the
   pipeline's resting state, and it is now the value any future regression is measured against — which
   requires that the value exist on the player's path. That is S2-am2's one deliverable.

**The recurrence risk, named because it is not fixed — it is off:** the cause is a **per-user Editor
setting, not a committed value.** Nothing in the repo holds it off. A fresh clone, a reset layout, a
second machine or a new seat can turn it back on, and the surface will look broken again to whoever
hits it. **Cheap guard, owed to whoever owns the playtest recipe:** the setting is named in the
recipe, next to the resolution, so the next person reads it instead of re-running this hunt.

**And a capture-record consequence (C34):** any evidence frame shot **through the Editor Game view**
before 2026-08-09 carries an **unknown state of this toggle** — its capture environment was never
pinned. Harness frames go through a render texture and are a different path, so the frame record is
very likely unaffected; Allen's own walk PNG of 2026-08-08 is a confirmed instance, and it is the
frame that started this. **Owed as bookkeeping, not as a re-opening:** name any finding that rests on
a hand-shot Game-view frame rather than a harness frame. I expect the answer to be "none but Allen's
two," and it should be *stated*, not assumed — which is the whole of C34.

## S2-am — The product-fact floor is a canvas-space size against a screen-space cost

**Amended** · DD 2026-08-09. **S2's 13 px floor is not withdrawn and nothing is re-authored on this
ruling.** What is ruled is that the floor was expressed in the wrong channel and has therefore never
actually been enforced.

S2 fixes the product-fact floor at 13 px in the 1024×704 artboard. C38's ramp is fixed in **output**
pixels. A canvas-space size consequently guarantees nothing at the player's eye — the same 13 px fact
reads differently at every magnification, and no gate in the studio has ever measured it where it
lands.

Measured at the ratified acceptance view: a product fact carries a **~3.5 px stroke** against a
**~1.6 px ramp — 48% of the stroke is transition.** Read at review distance on Allen's own frame
(this seat, 2026-08-09): the season records (`6-3`, `4-5`) and the row numbers `01`–`06` sit **at or
below legibility**. The masthead, team names and price figures are unaffected.

Ruled:

1. **The 13 px authoring floor stands.**
2. **It is no longer sufficient alone.** The floor gains a second half in the output channel: at the
   acceptance view a product fact must carry a **solid core**, and ramp ÷ stroke is reported with the
   view and the space named.
3. **The pipeline fix is necessary and not sufficient — and the arithmetic says so before anyone
   builds it.** The physical floor for an antialiased edge is ~1 output pixel; at a 3.5 px stroke the
   best achievable ratio is **~0.29**. A perfect pipeline therefore moves 0.482 → ~0.29: real, worth
   having, **and still not crisp.** The remaining ground is **magnification, not sharpening** —
   either the acceptance view puts more screen pixels on the artboard, or the smallest facts grow in
   canvas space. That is a design decision, it is mine, and it **waits on check-3**, which may move
   the number it would be decided against. *(First-order estimate: it assumes the stroke measure is
   unbiased by the ramp — see §Owed·2.)*
4. **No type sizes are re-authored before check-3.**

## S2-am2 — Right-sized, once check-3 landed

**Amended same day** · DD 2026-08-09. Check-3 landed and moved the number S2-am's prescription would
have been decided against, exactly as clause 3 said it might. The hedge fires; the prescription goes.

- **Clause 1 STANDS** — the 13 px authoring floor is unchanged.
- **Clause 2 STANDS, right-sized.** The output-channel half is **not** a per-item gate. It is **one
  recorded baseline**: ramp ÷ stroke at the acceptance view, **measured on the player's path** — not
  the harness's, because the single most durable lesson of this hunt is that those are different
  paths and the difference was invisible until Allen looked. One number, recorded once, so that a
  future regression has something to regress *from*. Today the studio would have to re-run the entire
  hunt to know whether the surface had got worse.
- **Clause 3 RETIRES.** "The remaining ground is magnification, not sharpening" was a prescription for
  a bar that was failing. The bar is met. **Nobody re-poses the acceptance view and nobody grows the
  small facts** — the ~0.29 arithmetic was contingent, it is spent, and it is recorded here only so it
  is not rediscovered as an open question.
- **Clause 4 DISCHARGES.** No type is re-authored — not "not yet", but not at all, on this ruling.

**What survives, and why it is worth keeping after the defect evaporated:** a canvas-space size still
guarantees nothing at the player's eye. This instance resolved to an Editor toggle, but the
**detection gap is unchanged** — the studio came one walkthrough away from an illegible surface, and
not one of its gates could have seen it, because every one of them measures canvas space. A single
recorded baseline on the player's path closes that gap for the cost of one measurement. That is the
whole of the amendment.

## C39 — Enumerate the path before auditing the segment you own

**Law** · DD 2026-08-09, promoted from this hunt, and the most expensive thing it taught.

**Where a complaint arrives through a path, the candidate set is the whole path from build to eye —
not the segment the investigating seat owns. Enumerate every segment first and check the cheap ones
first; audit the segment you own last.**

*Founding case, this hunt.* The complaint came from Allen's screen. Six candidates were tested, a
harness was audited, two capture sets were discarded and an instrument was rewritten — **all inside
the build**, the one segment the investigating lane owned. The cause was a **checkbox in the Editor's
Game view**, a segment nobody had enumerated as a candidate until the build-side audit was already
complete. The lane did reach it — it is check 1 of the four it sent to Allen — but it reached it after
the expensive half, and the ordering is the lesson, not the omission.

Path segments are almost always **cheap to check** (a toggle, a resolution, a scale factor) and the
build is **expensive to audit**. Checking cheap-first is not merely efficient, it is *epistemically*
correct: an unenumerated segment silently contaminates every measurement taken inside the segment you
are auditing, which is what produced the 21% pose mismatch, the invalid `_Sharpness` null and the
withdrawn ink-ring claim in one bundle.

Sits beside R40's general form (a runtime override over a wrong authored value makes it invisible to
the only audience that can report it) and C13 (a defect living *between* slices belongs to neither).
**Three instances now of a fault that is real, visible to the player, and owned by no seat because it
falls between two.**

**Recorded as this seat's share (§1.5):** my own pre-committed disposition enumerated "Arm A — Allen's
display path" as a **single black box** and pre-committed four branches keyed on it, without once
asking what the box contained. The disposition was well-formed and it was still blind in exactly the
way this law names. Both seats made the same mistake in the same fortnight, which is what promotes it
from an observation to a clause.

**Deliberately NOT proposed for the constitution today.** C36 and C37 are, and C39 is not — it has one
founding case and two cousins (R40, C13), where those two have demonstrated cost behind them (two
capture sets discarded; two verdicts un-retired). The constitution's whole argument is that `08` died
of a document governing more than it had earned, and three new clauses in one day from one incident is
that mistake in miniature. **C39 lives in the register until it catches something a second time.** If
it does, it belongs in §2 — it is a law about where evidence is looked for, not about how gates
report.

## C26-am3 — The phone's stub expires

**Ruled** · DD 2026-08-09. C26 re-opens **for the phone only**; the other three surfaces stay closed.

C9 made the phone a deliberate stub and C26 closed on that basis. R28-am held the line: room owns the
object, nobody owns the content, live engine data only, nothing authored. That was correct and it
stays correct about *content*.

**Allen's sharp ruling now covers the phone. That converts the stub from a legitimate state into an
owed document.** A stub is honest precisely while nobody is asked to make the surface good; the
moment "make it clear too" is a requirement, the surface needs an authority to be clear *against* —
and it has none: no face, no size floor, no palette, no composition.

Read at review distance on Allen's frame, **stated as a read and not a measurement** (C11 — the frame
is soft and no reference exists yet):

- the message copy is **sentence-case lowercase with terminal full stops, in a face that is not the
  studio's**. One object away, the laptop is uppercase Archivo. Two voices on two screens the player
  sees in the same glance.
- the screen is **~85% empty**, one message panel at the foot, and that panel's ground is the
  brightest element on the phone.
- the `BOOKIE` header reads **cool blue/cyan**. **Not ruled — measured or nothing** (R39/R40's
  standard; T9's retired `chromeCyan` is live on another surface). Owed with the phone's reference
  frame.

Ruled:

1. **The phone's owning document is owed**, written at this seat, **sequenced after the phone has a
   reference frame**. Writing it now against an unsettled surface produces another `08` — C26's own
   founding reason.
2. **R28-am is untouched.** Live engine data only, nothing authored. What is owed is the **treatment**
   of that data, which has never had an owner and is a different thing.
3. **The phone's blur is C38's**, routed to the pipeline item; **no phone-local fix is scoped.**

**Restated reason — same day, after the fix.** The trigger I cited above ("Allen wants it clear too")
is **spent**: the phone is clear now, on his own verdict. A ruling whose stated reason has expired
gets re-argued or withdrawn, not quietly left standing — so, re-argued:

**What carries C26-am3 was never sharpness.** It is that the player sees the phone and the laptop
**in one glance**, and they are speaking in two different voices — uppercase Archivo one object away
from lowercase sentence-case in a face that is not the studio's. That was true before the blur, it is
true now that the blur is gone, and it is **more** visible now, not less, because a clear screen shows
its treatment where a soft one hid it. The screen is also ~85% empty with its brightest element at the
foot, and its header hue is unmeasured against a retired-hue register.

**C26-am3 stands, unchanged in substance and unchanged in sequencing** — the phone's owning document
is owed, written at this seat, after the phone has a reference frame. What changes is only that it is
no longer urgent, and it was never listed as urgent.

## S71 — CLOSED on frames

**CLOSED** · DD 2026-08-09. `NO MARKS ON THIS SHEET` prints in the margin's own row on Allen's
2026-08-09 frame — the state is named, the owner is not, no second voice in the column.

*Scope (C25):* a string-and-placement check read through the very softness C38 describes. Both are
unambiguous at this ramp; the ink was read **qualitatively** as toner and **not measured**. A measured
ink check rides with the next SureThing set and does not withhold this close.

---

## Owed back — observations, not rulings

**1. `PRICES FINAL` appears to have survived on the masthead subline.** Allen's 2026-08-09 frame
reads `ROUND 1 OF 8 · PRICES FINAL`. S37 (batch 7) ruled the subline is `ROUND 1 OF 8`; S50 (batch 8)
granted the deletion explicitly as an unexecuted S37 ruling worth 18 px. **Not opened as an item** — I
cannot tell from a playtest PNG whether Allen's build is current main. **Verify at HEAD.** If it is
still there it is an unexecuted ruling already granted twice, not a new finding, and it lands with the
next SureThing commit.

**2. The ratio instrument's behaviour under blur is uncharacterized — C37 turned on this batch's own
headline.** Allen's stroke denominator measures **1.23×** the harness's where his lower magnification
predicts **0.79×** — a 1.55× discrepancy that is either the ramp moving the 50%-to-50% measure or the
admitted 21% pose mismatch. **The conclusion is not disturbed:** the direction is large and visible to
the eye in the split crop, and it is what the exoneration rests on. The **magnitude** — the 56%,
already open — inherits this, and so does S2-am·3's ~0.29 estimate. Owed with check-3: **feed the
instrument a synthetically blurred frame of known kernel and confirm the ratio tracks.** An afternoon,
and it makes every future number from this instrument load-bearing.

**3. "The room looks fine" does not exonerate the pipeline.** Allen walked the room and called
everything but the UI great. That is consistent with a frame-wide cause, not evidence against one:
**UI type is the only content in the frame with detail at the ramp's scale.** A 1.6 px ramp is
invisible on matte plaster at L\* 10–18 and catastrophic on a 3.5 px stroke. Recorded so the walk is
not read as a counter-argument — and it explains why eight days of room measurement never surfaced
this: **every room instrument reads surfaces where this defect cannot show.**

**Amended same day, after the close:**

- **Owed·1 is unchanged.** A string is not affected by softness — `PRICES FINAL` was never a blur
  question and still needs verifying at HEAD.
- **Owed·2 is right-sized into S2-am2.** The instrument's bias under blur is no longer a standalone
  task; it becomes the **precondition of the one baseline measurement** — characterize the ratio
  against a known kernel *before* recording the number that future regressions will be judged
  against, or the baseline inherits the bias permanently.
- **Owed·3 is CONFIRMED, not retired — and it is now a demonstrated fact rather than an argument.**
  The cause was frame-wide by construction (the whole Game view was upscaled) and **only the UI read
  wrong to Allen.** That is precisely the prediction: UI type is the only content in the frame with
  detail at the ramp's scale. A frame-wide fault presenting as a single-surface complaint is now a
  worked case, not a hypothesis, and it is the reason the search started in the wrong half of the
  path (→ C39).

## Recorded, on the lane's conduct

Three unprompted corrections, one withdrawn claim, an invalid null caught in its own numbers, a
three-outcome test pre-committed before the deciding frame existed, and the one claim that had no
number behind it named as the only one that had to be withdrawn. **The bundle argues against its own
headline in four places.** That is the standard, and it is why the build-side half closes in one pass
instead of three.
