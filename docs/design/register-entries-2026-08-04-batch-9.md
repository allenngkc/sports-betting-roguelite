# Register entries — 2026-08-04, batch 9

**Transcribe into `main-2/docs/design/REGISTER.md`.** Issued at authoring time per C22. Ruled from the
tables (current through batch 8) with the constitution as Allen-approved canon.

New: **C27**, **C28**, **S51**, **S52**, **S53**, **R31**, **R32**, **R33**, **T58**, **T59**, **T60**.
Amendments/closures: **T41 closed**, **T48 closed**, **T49 ruled**, **R25 granted**, **R28-am**,
**R19(b)-am** reasoning corrected, **S8 re-verified**, **R23** test amended.

Every number below is measured off the delivered frames. Method: brightest-pixel and mean relative
luminance per region, hue/saturation on the peak pixel, darkest-2% mean for black points.

---

## T41 — Cap the stage. **VERIFIED ON FRAMES. CLOSED.**

**State change:** Ruled (batch 6, as T22) · blocking TV Phase 3 → **Design-verified · DD 2026-08-04.**

The cap landed and the ladder is intact. Measured across all six A/B frames, both arms:

| region | before (batch 4) | now |
|---|---:|---:|
| pitch, peak | **#ffffff / 1.000** | #f1f1f1–#f4f4f4 / **0.880–0.905** |
| saturated pixels (≥250 all channels), any region | present | **0.00%** |
| scoreline, quiet frames | 0.923 | 0.737 |

Pure white is gone from the surface. **Zero saturated pixels in any sampled region of any frame** —
that is the clean result, because a single clipped pixel would have kept the bloom question
unanswerable. TV Phase 3 is unblocked.

## T58 — Gold is on the score at the goal flash. **NEW. §4 VIOLATION. Ranks above T42.**

The cap fixed the brightness; it exposed a hue defect underneath. Measured on the scoreline region,
peak pixel:

| frame | hex | hue | saturation | luminance |
|---|---|---:|---:|---:|
| quiet, seed 16180339 | #d9e0e5 | 205° | 5.2% | 0.737 |
| quiet, seed 30941771 | #dae0e4 | 204° | 4.4% | 0.738 |
| **goal flash, seed 16180339** | #e8e177 | **56°** | **48.7%** | 0.723 |
| **goal flash, seed 27182818** | #e8e24d | **58°** | **66.8%** | 0.721 |

At rest the scoreline is cold white, exactly as specified. **At the goal moment it turns saturated
gold** — 56–58° at up to 67% saturation — and the ball dot goes with it. Gold is rationed to money:
won legs, payout figures, the cash-out band. **A goal is not money.** It is the event that may
eventually produce money, which is precisely the distinction the rationing rule exists to hold; the
whole reason gold means something on this surface is that the score never wears it.

**And it re-creates T41's defect in a second channel.** At the goal flash the scoreline reads
**0.72** while the actionable cash-out band reads **0.62**. The designated L4 element is again not the
brightest thing on its own surface, at the exact moment of peak drama — which is the moment the
player is most likely to reach for the key. Capping the stage fixed the continuous case; the flash
case was never measured.

**Instruction.** The goal flash stays a brightness event on the cold-white channel: punch the
scoreline's existing white toward L4 for the flash duration and settle it back. No hue change. If the
flash needs more presence than white can carry, it takes it from the event strip's punch and the leg
row's resolve — both of which already own that moment — never from gold.

## T49 — Bloom A/B, 1.8 vs 1.4. **RULED: 1.4. And the A/B is very nearly a null result.**

**State change:** Returned unadjudicated (batch 6) · C21-held → **Ruled · DD 2026-08-04.**

The confound is cleared, so the pair is now answerable. It answers smaller than anyone expected.
Region statistics, all six pairs:

| region | 1.8 vs 1.4 |
|---|---|
| pitch | peak within ±0.009; **means identical to 3 decimal places** |
| cash-out band | peak within ±0.007; means identical |
| halo region around the band | means identical to 3 dp in all six pairs |
| **goal-flash scoreline** | **1.8 = 0.755–0.757 · 1.4 = 0.721–0.723** |

**Everywhere except the goal flash the two arms are indistinguishable at the instrument's resolution.**
The single region that measurably differs is the one carrying the T58 defect, and there 1.8 pushes an
already-offending gold scoreline ~4.7% hotter.

**So 1.4, on the ladder rather than on taste:** the arms are equivalent everywhere the surface is
behaving, and where they differ, 1.8 widens the gap between the designated L4 element and the thing
outshining it. When two settings are otherwise equal, take the one that does less damage to the law.

**The finding worth more than the pick:** bloom intensity was never the lever this question assumed
it was. A ±0.4 change in bloom boost moves nothing on this surface except one element that is the
wrong colour. **Do not re-open bloom to fix the goal flash** — fix T58, then leave bloom alone.

**My own C25 disclosure, and it limits the above.** The two arms' paired frames do **not** share sim
state — actor positions differ between 1.8 and 1.4 at the same seed, scene, grammar and frame index.
So the whole-frame per-pixel diff (2.5–3.3% of pixels differing by >2, mean 44–59 on those) **cannot
be attributed to bloom**; most of it is actors that moved. Region statistics on fixed boxes are the
only valid instrument on this pair, and they are what I ruled from. **An A/B whose arms are not
frame-locked cannot support a per-pixel comparison** — worth fixing in the harness before the next
one, and worth noting that the larger, more impressive-looking number here was the invalid one.

**Also recorded:** the harness `Assert.Failed` on 4 of 5 seeds against a shared 420s budget, disclosed
unprompted, with the frames confirmed complete and the assert confirmed non-gating. Correct handling —
the disclosure is what let me use the set without wondering.

## T59 — TV input contract during suspension. **RULED: suspension gates the input.**

The slot's brightness is a promise about input. That is already ruled twice (T24, T43), and it is not
a statement about display — it is a statement that **display state and input state are the same
state.** A dark slot reading MARKET SUSPENDED while E still cashes out is the same lie as a bright
slot that refuses the key, inverted: the surface tells the player it will not act, and then acts.

The consequence decides it past any aesthetic argument. **A player who presses E during suspension
receives a cash-out they were just told was unavailable, at a price the display is not showing.** On a
money control, accepting an input you have declared refused is the worst available outcome — worse
than refusing an input you appeared to offer, because the player cannot even see what they got.

**Instruction.** One gate, one source of truth: the slot's state **is** the input's state, read from
the same value. `suspended` and `pending` refuse E; `actionable` accepts it; `updating` refuses it,
because the offer is not yet acceptable and L3 already says so. When a press is refused, nothing
flashes and nothing explains — the slot is already dark and already labelled, and a refusal
animation would be the surface apologising for a rule it should simply hold.

The lead was right that this was not his to move. Recorded as such.

---

## S51 — Markets' 2.6px residual. **RULED: all three candidates refused — the premise is falsified. B1 merges under a signed, expiring deviation.**

**State change:** blocks B1 → **Ruled · DD 2026-08-04.** Three worktrees unblocked.

**The diagnosis is wrong, and the frames say so.** The lead attributed the 2.6px to the payout's
wax highlight, rotated −0.5°, on the reasoning that a ~100px band at that angle swings ~2.6px below
its unrotated bounds. Measured in the delivered captures, the highlight is **23–24px wide** — form
lobby y 380–388, x 717–739; staged receipt y 450–466, x 717–739. It hugs the figure, exactly as
specified. **At 0.5° a 24px band's total vertical extent grows by 0.21px.** That is **twelve times
too small** to be the 2.6px. The arithmetic in the note requires a rotated element ~300px wide; the
ornament is 24px.

So all three candidate answers fail together, because each assumes the highlight is the source:

- **Accept 2.6px and slacken the reservation** — slackens a bound to accommodate a cause nobody has
  identified.
- **Exclude the ornament from the measurement** — excludes an element that is not doing it. This
  would have gone green while the real overrun continued, and would have become the fortnight's
  **fifth** vacuous gate. **The lead's instinct to refuse this unilaterally was exactly right, and
  better founded than he knew** — he withheld it on principle; the frames show it would also have
  been factually empty.
- **Keep the band unrotated in the reserved region** — removes a specified ornament to fix something
  else.

**One lead, offered as a lead and not a diagnosis:** 292px × sin(0.5°) = 2.55px, and **292px is the
measured width of the PLACE TICKET wax field** (x 716–1007, y 380–436, in the entry-selected frame).
That is a suggestive coincidence and nothing more. I am labelling it as such rather than replacing
the lead's plausible story with my own — which is the trap this item has already caught one person in.

**How B1 merges anyway, today.** 2.6px on a 704px canvas, on a world-space laptop read at an angle,
inside the room's grade, is not a visible defect — **no frame in any delivered set shows anything
overlapping**, and the invariant is reporting a reservation breach, not a collision. So under C16
this is expensive, not impossible:

> **Signed deviation, DD 2026-08-04.** B1 merges with the flow's lowest element 2.6px outside its
> reservation with a staged receipt at `MaxLegs = 4`. **Named cost:** one un-owned 2.6px excursion in
> the margin's reserved region. **Expiry:** when the owner of the 2.6px is identified — at which point
> it is fixed, not re-signed. The suite goes green by **recording the deviation**, never by slackening
> the reservation and never by excluding an element from the measurement.

**Owed with it:** the capture of this state. The lead noted correctly that `09-margin-max-legs` stages
no receipt, so no frame of the overrun exists. Add it — not to illustrate the number, but because the
next person to hunt the 2.6px will need to see it, and because C17 says the capture is the thing that
settles what a source read only suggests.

**Endorsed:** the invariant now excludes full-bleed stretch grounds after reporting −530px when the
ruled-paper substrate was counted as flow. That is an instrument catching itself.

## S52 — S8 desktop, after the chrome fold. **RE-VERIFIED, with one required change.**

**State change:** Design-verified · returned to review by S48 → **Design-verified · DD 2026-08-04.**

**The fold is verified, and by the right instrument.** The rail band is 100% pixel-identical between
the desktop and the in-app screen across 17,408 samples; the tray past the app slots likewise across
10,268. That is a comparison, not an absolute colour check against a hand-computed token — the one
kind of check that has never misled anyone on this surface (S31-am is why). One chrome consumed
twice, demonstrated rather than asserted.

**Required change — the gap closes, and it is larger than reported.** The lead reports 86px between
the rail and the first icon. Measured on the flat capture, the rail ends at y=34 and the first drawn
icon pixel is at **y=148 — a 114px gap.** No operating system puts its first desktop icon 114px below
its menu bar. **That space was the wordmark's, and it should leave with the wordmark**: the icon
column starts at the standard margin below the rail. The lead's judgement that a second mark would
re-introduce exactly the duplication the fold removed is correct and I am not overriding it — the
answer is to close the hole, not to fill it.

**Two dismissals, both raised honestly and both fine:**

- **MESSAGES and its badge on the desktop: keep.** The tray is his machine's furniture and it is
  present on every surface by S8's own logic; a tray that shows MESSAGES in the app and hides it on
  the desktop is two trays, which is what the fold just removed. It also happens to be the best piece
  of characterisation on the screen — an unread message at 02:47 that he is not opening.
- **Icon chips and chrome sharing `--ground-3`: not a defect.** Measured: chip #212115, rail #222216,
  tray #222216, wallpaper #16160d. The chip is a full value step above the ground it sits on, which is
  what makes it read as a tile; it shares a value with the chrome, but the two are **never adjacent** —
  chips live in the work area, chrome at the edges. Shared value only becomes a defect at an edge.

**Recorded, not actioned:** the work area measures 99.7% at or below ground level. The desktop is
essentially empty apart from the icon column, and that is right — it is a cheap laptop at 02:47 with
four applications on it. Emptiness here is characterisation, not an unfinished screen. Once the gap
closes, the composition is doing what it should.

## S53 — The run-verdict screen. **PART-RULED from law; the colour claim held for a capture.**

**Ruled · DD 2026-08-04.** The lead asked for a ruling *or* a decision that it does not need one yet.
It is both, split along a line the note itself draws well.

**Two of the three findings need no frame, because they are category questions, not measurements:**

- **`THE BOOKIE COLLECTS` in oxide is a violation. Change it.** Oxide is the house acting on the
  document — a blocked action, the strike on a dead leg or a lost ticket. **A run ending is not the
  house marking the document**; it is the run ending. Oxide as a generic "bad" tint is the exact use
  the law names and forbids. On this surface a lost outcome is carried by **value** — the headline
  drops to `--toner-3` — and, if it carries a mark at all, by the strike sprite over it. Loss is
  darkness here as it is on the TV, arrived at by the laptop's own grammar rather than borrowed.
- **`NEW RUN` as a biro-filled field is a violation. Change it.** Biro is only ever what *he* chose.
  A primary action is a wax field, wax-ink type, 2px `--wax-deep` edge (S18). Starting a new run is
  the most consequential action on the screen and it wears the primary treatment.

**And the question the lead was unsure of, answered: wax is money, not mood.** A run's verdict states
a money outcome, so the *winning* headline in wax is correct — it is naming what he won. Which also
settles the losing headline: it states a money outcome too, and the honest way to render a money
outcome of zero is **not** a different colour but the absence of the one that means money. Wax when
there is money; dimmed toner when there is none. Mood is what the value does, not what the hue does.

**Held for evidence: the `rgba(0.03, 0.02, 0.06, 1)` ground.** That is a measurement claim and it does
not get ruled from source. This surface has twice produced a source read that a frame dissolved (S32,
T26), the one absolute check ever made against a hand-computed token here was wrong because the
project renders linear (S31-am), and this canvas sits inside the room's grade with bloom. **The lead
identified all three reasons his own finding might be wrong, before reporting it.** That is the
standard, and it is why I am not ruling on it.

**Instruction: yes, add the capture** — force `RunWon` and `RunLost` as capture states and put them in
front of the next drag. Fix the two category defects now; the ground waits for its frame. Nothing is
blocked either way, and the lead's decision to correct only the app's name in S46 and stop rather than
restyle an unruled surface was correct.

---

## R25 — The painterly read. **GRANTED**, with a fragility recorded.

**State change:** Withheld · DD 2026-08-01 (precondition: first R23 set after R19) → **Design-verified ·
DD 2026-08-04.** Precondition met: post-R19, post-T48, post-R22, post-retirement, nothing re-used.

The room reads painterly semi-realistic at both poses. Plaster carries surface without becoming
texture-mapped detail, the three light sources stay separable — warm tube pool on the right wall, the
window's local cool pool, the city's warm points beyond it — and the whole frame sits in a narrow
dark value band without collapsing into mud. It reads as a photographed place rather than an
assembled one.

**R19's value separation holds where it matters:** laptop/housing **4.68×**, laptop/phone 3.18×,
phone/housing 1.47×. The two personal machines and the institutional one are separated by value, which
is what R19(b)-am put the read on.

**The fragility, flagged by the lead unprompted and recorded because it will matter later:** the
albedo-only ratio is **2.17×**, so the rig roughly doubles the separation — the laptop sits in the desk
lamp's pool and the housing sits in shadow. **The ruling is satisfied as the room reads, and the read
is lighting-assisted.** Move the laptop out of that pool and the separation falls back toward 2.17×.
Any future shot that relights the desk re-opens R19, and this line is the reason.

## T48 — The unified grade's black point. **VERIFIED. CLOSED.** / R23's test amended.

**State change:** Ruled Option A (batch 6) → **Design-verified · DD 2026-08-04.**

**The neutral black point landed.** Darkest-2% mean, measured on the delivered pair:

| frame | graded | ungraded |
|---|---|---|
| wide 68° | **rgb(23,23,24)** — sat 4.2%, one unit of blue in 23 | rgb(4,4,6) — sat 33% |
| seated 17° | **rgb(21,21,21)** — sat 0%, exactly neutral | rgb(3,3,3) |

The lift still does its level job (luminance 0.0013 → 0.0086, ~6.6×) and its hue is now neutral to
within a quantisation step. `#0a0c10` is retired as the shadow-lift target and survives as the TV
substrate value, which is what splitting one number into two was for.

**R23 still FAILs, and its test is now wrong rather than the room.** Two surfaces read cool: far
plaster (chroma 3.56, hue 275.5°) and floor aisle (1.64, 272.1°). **Both are cool ungraded too** —
5.53 at 275.7° and 2.94 at 272.8°. So this is the window's own light, and **§1.2 sanctions exactly
that pool**: a cool window with short reach that pools locally and does not tint the room.

**Amend R23's instrument to exclude the window's sanctioned pool**, and state the exclusion and its
boundary in the gate's own line. Law 1.1 is about the room's cast; a gate that fails the room for
having the window the lighting design specifies is measuring a feature as a violation.

**This is the one exclusion I will authorise this session, and the contrast with S51 is the point:**
here the excluded thing is *identified*, *measured*, and *sanctioned by name in a ratified document*.
In S51 the proposed exclusion was of an element that turned out not to be causing the overrun at all.
**An exclusion is legitimate when you can name what you are excluding and why it belongs there — never
when it merely makes a number go green.**

**T45 confirmed subsumed.** No separate navy-drain finding survives in this set. Batch 6 predicted it
would resolve with T48 and it has; the standing retarget-to-olive instruction lapses unused.

## R19(b)-am — Reasoning corrected on the record. **Conclusion stands.**

**Amended · DD 2026-08-04 · my error, T47-am style.**

The room lead reports cool metal is measurably reachable, and the frames agree: the conduit drop reads
**COOL at 269.2° graded and 269.5° ungraded**, the steel housing reads **neutral at chroma 0.52** —
below the instrument's own 1.5 floor — and only the tube-raked ceiling run reads warm. Same albedo,
opposite verdicts, in both columns.

**So the amendment's physical premise was wrong.** I wrote that under one warm key on warm plaster the
room cannot return cool colour and extended that from Law 1.1's plaster to the metal. Rendered hue
tracks **which light reaches the surface**, not albedo alone, and a dark fixture in shadow lit by
window bounce is not the same case as a plaster wall in the tube's pool. I over-extended a correct law
past its subject.

**The conclusion is unchanged and stands on the grounds that were always the real ones:** value and
finish carry the institutional read because they *carry it robustly* — through any lighting change,
any grade revision, and any camera pose — where hue carries it only while a particular light happens
to reach a particular face. R25's own numbers make the case: value separates 4.68×, and hue on the
same three bodies gives three different verdicts.

**Also standing: no lighting instrument for this.** R12's grazing class reveals relief, not colour
temperature, and adding a cool light to tint metal remains T48's rejected Option D in new clothes.
That refusal never depended on the falsified premise.

**Recorded because it matters more than the correction:** the lead challenged a ruling's reasoning
while implementing its conclusion, and marked it non-urgent so it would not block. That is how a
premise gets fixed without a lane stopping.

## R28-am — Phone content. **Amended by Allen. Reconciled.**

**State change:** R28 ruled (batch 8) → **Amended · Allen 2026-08-03 · reconciled DD 2026-08-04.**

The phone renders the **live BookieFeed** and stays. My dark-stub default was guarding against
invented UI on a surface with no owning document; the content is live engine data, which is not the
thing being guarded against. The narrow re-ask surfaced it — **exactly why R28 said to re-ask if the
question was narrower than the answer.** The guard was right and the case was outside it.

**R28's structure survives:** the room owns the object, geometry, material and its named interaction
`MeshCollider`; the phone follows the laptop's personal register, not the TV housing's; and the phone
still has no owning document, so **nothing may be authored onto that screen** — live engine data only.

**One consequence to route, flagged by the orchestrator and correct:** a lit phone screen re-opens the
**C13 surface-content question for room captures**. R28's dark stub made the phone structurally immune
to shipping superseded content inside a room frame; a live feed removes that immunity. Room captures
now carry three live surfaces, and per R23 a cool cast that appears only when screens are lit remains
a screen finding, never a room finding. **Add the phone to whatever C13 currently covers for the
laptop and TV.**

## R31 — Institutional metal: **re-weighted to finish-led.** Value stays a requirement.

**Ruled · DD 2026-08-04.** Granted, and it follows R19(b)-am's correction rather than contradicting it.

On the corrected lighting, finish is carrying the read. That is coherent: the housing measures
**neutral, chroma 0.52, L\* 11.30, sd/mean 0.020** — a very dark, very flat, very even surface, where
value has little room left to signify because everything around it is also dark. **Finish is what
still has range there**: a tight bright specular against the laptop's dull plastic, hard-edged chipped
paint against worn smooth plastic, rivets that catch a highlight the plastic cannot.

**Finish leads; value remains required, not optional.** The ≥2-channel rule stands with finish primary
and value secondary. Value is what survives a camera pose that puts no specular in frame, and R25 has
just recorded that this room's value separation is partly lighting-assisted — dropping value would
leave the read resting on the one channel that is also rig-dependent.

## R32 — Drab green vs the window pool. **RULED: the placement amends. The green stays, the pool stays.**

**Ruled · DD 2026-08-04.** Three candidates were offered; the third is right.

**The green does not move.** `#3A4230` is ratified palette law under the 2026-07-28 instruction, and
R33 below is about getting it into the room at all. A swatch that has never been applied does not get
revised for failing to survive a light it was never placed under.

**The pool does not change.** It is the window's sanctioned short-reach cool pool — §1.2 names it, T48
just declined to fail the room for it, and R23's instrument is being amended to respect it. Changing
the pool to save a mattress would spend a lighting law on a dressing problem.

**So the placement amends:** the bunk fabric reads its drab green **outside** the pool's reach, and
where the pool does fall on it the fabric reads as pool-lit fabric — which is what fabric does under a
cold window. **A palette does not promise that every swatch reads at full chroma in every square metre
of the room**; it promises the room is built from those colours. One surface partly desaturated by a
sanctioned light is the palette behaving, not failing.

**Hold the value:** bunk 2's mattress is the legible-as-occupied test at 43.9 ±1.0, and it currently
measures **37.36 in the T48 set against 44.44 in the R25 set** — the two runs disagree by 7 points on
the same nominal check. **Resolve that before touching the fabric**: one of the two boxes is not
framing the surface it thinks it is, which is the failure mode that gate's own blind-spot line names.
A hue change decided against an unreliable value reading would be two problems wearing one number.

## R33 — Drab green absent from the room. **Confirmed open, sequenced after R32 and the mattress reading.**

Carried from batch 6 (T56 as then keyed). All four bunk/mattress materials remain warm neutral greys;
the palette names drab green for bunk frames and mattress fabric. **Apply the swatch** — the room is
wrong, not the document. Sequence after R32's placement rule and after the 37.36-vs-44.44
discrepancy is resolved, for the reason given above.

## R34 — BezelBlack retirement. **Re-reviewed. Retirement stands. The evidence trail is the finding.**

**Reconciled · DD 2026-08-04** (Allen re-confirmed on his authority 2026-08-03).

The retirement is right on its merits: `TVBody` now wears the same painted steel as the enclosure,
which is what §6 describes — one installed institutional object. A third body material on the same
object was a maintenance lie whether or not it was visible.

**But the room changed on a finding that was wrong, and that is what goes in the register.** "Not
visible" was too strong. Measured against the pre-retirement captures: conformance seated **170,389
pixels changed, max diff 92**; standing view **28,520 pixels, max diff 153**; wide frame unchanged.
The correct statement is **"not measurable"** — the bezel is exposed only as a thin strip at the far
left frame edge, adjacent to housing of near-identical value, so no surface-pure region is obtainable
where it shows. The regions that found rivets were sampled on the right and bottom, where the housing
covers it.

**Not measurable and not visible are different claims, and only one of them was true.** A
design-verified room was changed on the stronger one. The outcome survives review; the reasoning did
not. The lead measured the diff after the fact, found his own finding overstated, and reported it
against his own change — which is the only reason this is a register line instead of an unexplained
delta someone finds in three weeks.

## C27 — sd/mean does not establish surface purity. **LAW.**

**Ruled · DD 2026-08-04.** Promoted from the room package's instrument finding.

The rejected bezel box scored **sd/mean 0.038**, comfortably inside the ≤0.15 bar, while straddling
two surfaces. **Bezel and housing share a value, and variance cannot separate equals.** A
low-variance box proves the sampled pixels are uniform; it does not prove they belong to one surface.

**Every surface-pure region is confirmed by eye as well as by variance**, and the criterion's limit is
stated wherever the criterion is quoted — this harness and `rig-r23-recipe.md` both. This is C18 §4.2
in a new instance: the check was not lying, it was answering a narrower question than the one being
asked of it.

## C28 — Verdict coverage is reported, never inferred. **LAW.**

**Ruled · DD 2026-08-04.** Promoted from the gate reports, which now do this correctly and should keep
doing it.

Both delivered gate runs state: **"4 PASS, 2 FAIL, 4 SKIP, 3 VOID, 1 INFO — verdict coverage 6/14"**,
and spell out that SKIP means not run, VOID means a ruling took it off the board, INFO means measured
but never judged, and **none of the three is a pass.**

**"No FAIL" is not "all passed."** A gate suite reports how many of its checks produced a verdict at
all, names every check that did not, and says why. Eight of fourteen produced no verdict in these
runs — a suite that reported only its four passes would have been describing a different build.

This is the natural completion of C18 §4.2: that clause made a gate state its blind spots; this one
makes a *suite* state its coverage. **A summary line that hides non-verdicts is the vacuous gate at
suite scale.**

**Standing with it:** Gates 6–8 are VOID again — Allen walked and passed them at `9e1b4e4` and the
retirement changed the content fingerprint, so the certification expired itself. **That is the
mechanism working correctly, not a regression.** Geometry is untouched and his verdict is very likely
still good, but no tool may re-issue a human gate (C18, R22). Do not report these runs as N/N, and do
not let three VOIDs read as three passes on the merge checklist.

## Kit amendment — placed tickets draw on both screens. **SIGNED.**

**Signed · DD 2026-08-04**, per Allen's ruling 2026-08-03.

Placed tickets draw on the main betting screen as well as the event screen. Allen's reason is the
right one and worth keeping in the register: **seeing your stakes while you are picking is the moment
you need them.** Built as one shared component consumed twice — same discipline as `NotebookChrome`,
and the same reason: two copies that resemble each other is what S52 just verified the surface out of.

Note the interaction with **S51**: receipts on the sheet is E-07's placement and remains correct.
Drawing them on the betting screen too must not put them back into the 324px margin's flow, or the
112px overrun E-07 closed returns by another door.

---

## Ordering for the orchestrator

**Merges/unblocks available now:** markets B1 (S51, signed deviation), TV Phase 3 (T41 closed),
room R19(a) complete (R25 granted).

**Then:** T58 (gold on the score — ranks above T42, same channel, worse instance) → T59 (input gate) →
T46 → S52's gap → S53's two category fixes → R31 → mattress 37.36-vs-44.44 → R32/R33 → R23 instrument
amendment → T42 → T44 → T50.

**Re-checks that only become meaningful after the above:** T50's column-type items (blocked by T46),
S53's ground (blocked on its capture), T49 sealed — **do not re-open bloom**.

**Owed captures:** markets' staged-receipt overrun at `MaxLegs`; SureThing's `RunWon`/`RunLost`;
LEDGER twelve-state re-submit (post-merge, per its own sequencing).

**Still owed and still not written: SureThing's and TV's owning documents (C26).** Both surfaces are
closer to settled than they were — TV's ladder is verified and the grade is closed — but T58 is open on
the TV and S53 is unruled on the laptop. Sequence unchanged: after those two.
