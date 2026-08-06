# Register entries — 2026-08-05, batch 10

**Transcribe into `main-2/docs/design/REGISTER.md`.** Issued at authoring time per C22. Ruled from the
tables (current through batch 9) with the constitution as Allen-approved canon.

New: **C29**, **C30**, **C31**, **S54**, **S55**, **S56**, **S57**, **S58**, **R35**, **T61**.
Grants/closures: **LEDGER-DV granted**, **S53-am (held half closed)**, **S49 authored**,
**T60 struck**, **R34 header omission recorded**, **C26 re-sequenced**.

---

## LEDGER-DV — **GRANTED. Design-verified.**

**State change:** Withheld · DD 2026-08-02 (batch 7) → **Design-verified · DD 2026-08-05**, on the
sixteen-state re-submit at `f05332c`.

All six named closing conditions are landed, and I checked the one that mattered on the frame rather
than on the write-up. On `12-ledger-populated-multi`: `TICKET 1.0` CASHED OUT prints **`$8` in wax**,
paired with its wax terminal word exactly as WON is; `1.1` WON `$29` wax; `1.2` LOST `$0` in
`--toner-3` under an oxide strike **that crosses the word only**, not the figure. The tally reads
**RETURNED `$37`**, and 8 + 29 + 0 = 37. **The sum is a sum.** S36's blanked row is gone.

S39's one-baseline collapse is holding across every populated frame, and it is the reason S51's
margin fix had anywhere to come from — the same discipline paid twice.

**One treatment I am ratifying explicitly so nobody "fixes" it later.** `$0` renders **wax** in the
tally and **`--toner-3`** in the record row. That is not an inconsistency: the tally's RETURNED is a
*sum*, and a sum of zero is still money arithmetic, so it wears the money ink; the row's `$0` is a
*dead record*, drained toward the ground with the strike carrying the state. Both are correct for
different reasons. Recorded as considered and accepted.

**The two C25 gaps, ruled:**

1. **The cross-round retention capture is worth one — build it.** Every ledger frame in this set is
   ROUND 1, so retention across `ExitShop` is proven by construction and by the suite and by no
   photograph. That is precisely the shape C17 exists for, and the defect retention was approved for
   (a round-4 player meeting an empty screen captioned `SETTLED TICKETS · THIS RUN`) is a defect no
   frame in this set could have caught. **It does not withhold this grant** — the grant's conditions
   were about the figure, its ink and the total, all three proven. It is a named deliverable of the
   next window.
2. **Renumber `09-margin-max-legs-staged-receipt`.** Two states sharing `09` is a set that lies when
   read in order, and the lead is right that a set which misnumbers itself is a small instance of a
   large problem. It is the markets seat's frame, so the orchestrator carries the renumber to them
   rather than either seat editing unilaterally.

**And the frame I most wanted to see is clean.** `09-margin-max-legs-staged-receipt`: four legs, each
with its own RUB OUT, COMBINED `+1598`, both stake control rows, `STAKE $31`, `POTENTIAL PAYOUT $526`
on its wax highlight, PLACE live, LOCK disabled with its reason **inside the control**, SKIP beneath —
**all of it, at MaxLegs=4, with a staged receipt present.** That was the condition I named in S50 and
re-named in S51, and it is met on the frame. The anchored action stack never moved.

## C31 — A named closing-condition set is exhaustive. **LAW.**

**Ruled · DD 2026-08-05.** Promoted from the grant above, because I nearly broke it myself.

Where this seat withholds a verdict and names the conditions for granting it, **that list is the whole
list.** New findings on the same frames open new items; they do not retroactively withhold the grant.

I found S58 below on frames inside the LEDGER set and had to decide whether it belonged to the grant.
It does not. A withheld verdict whose conditions grow each time they are met is a verdict that can
never be earned, and a lead who satisfies six named conditions and is handed a seventh learns that
naming conditions is theatre. **If the seat named the wrong conditions, that is the seat's error and
it is recorded as one** (§1.5) — the grant still lands.

---

## S53-am — The verdict screen's ground. **HELD HALF CLOSED: it is `--ground`. The bespoke value goes.**

**State change:** S53 part-ruled, colour claim held for a capture (batch 9) → **Ruled · DD 2026-08-05.**

**The measurement is accepted, and it is visible without it.** Set the verdict frames beside the
LEDGER frames from the same drop: the ledger is warm olive-black, the verdict is **aubergine**. The
2449-sample read of **(12–13, 0, 12–13)** just puts a number on something the eye finds in a second.

**The ruling is one line: the verdict screen's ground is `--ground`, `#16160F`.** There is no licence
anywhere in this system for a bespoke ground on a SureThing surface. The verdict is the app's last
screen, not a different product — the run ends *on the document*, which is the whole point of a game
about a man reading a form. A magenta near-black is also two steps into the cyberpunk neon-on-black
rut this project names by name as its anti-reference, and it is **darker than `--ink`**, which puts it
under the lifted-black floor as well. Three separate laws, one fix.

**This makes the discrepancy investigation moot for this surface, and I am ending it here.** You do not
need to know why the wrong number renders wrong when the right answer is a different number. Replace
the authored value with the token and re-shoot.

**Endorsed strongly:** the lead reported "near-black and blue-tinted" from the source token, then
measured, then **superseded his own earlier description in writing**. That is C11 working as intended,
and self-correcting a claim you already put in front of this seat is the behaviour that makes a
package readable.

## S54 — `Color`-float-authored values may not render as authored. **NEW. Routed, not deferred.**

The lead's second finding is the more valuable one and it must not die with S53-am. Authored
`new Color(0.03f, 0.02f, 0.06f, 1f)` should land near (8, 5, 15) gamma or (51, 42, 69) linear.
Rendered: **(13, 0, 13)**.

**Neither magnitude nor channel order survives.** Blue is authored as the largest channel by 2× and
renders **equal to red**, with green **at exactly zero on 2449 of 2449 samples**. A colour-space
mismatch scales channels; it does not reorder them and it does not zero one. Meanwhile `Color32`-
authored grounds elsewhere on this surface render essentially 1:1.

That is a defect in something shared, and the verdict screen is only where it happened to be caught.
**Audit every `new Color(float…)` on the laptop surface** and report which render as authored. Whatever
the cause — render, capture path, or authoring — it has been able to silently change a colour on a
shipped surface, and this project makes colour rulings on measured frames.

The lead reported this rather than diagnosing it, and fixed nothing, because I had deferred the ground
and the fix might not have been the token. **That was exactly right** and it is why the finding exists
at all.

## S55 — The verdict screen drops the OS chrome. **NEW. Ranks with the ground.**

Both verdict frames have **no NOTEBOOK rail and no tray.** Every other laptop surface in the set —
form, entry, my bets, rewards, ledger, desktop — carries both.

**The chrome is not decoration; it is the argument.** The rail and tray are what make SureThing an
app running on *his* machine rather than the game's UI, and that split is the single most important
constraint in this design system. A full-screen takeover with the OS deleted is a **game over card**,
and the moment the machine disappears, the fiction that he is a man at a laptop disappears with it.

The rail also carries the sticker, the 02:47 clock and the dying battery — three of the cheapest,
best pieces of characterisation on the surface, and they are at their most useful **at exactly the
moment the run ends.**

**Instruction: the verdict renders inside the persistent chrome, like every other destination.** The
work area may be as sparse as it likes; the machine stays.

I will accept an argument that the verdict is a modal over the whole panel rather than a destination —
but it comes with a frame, and it does not get to remove the rail and tray while making it.

## S57 — The verdict frames' figures invert. **NEW. Ask before fixing.**

`13-verdict-run-won` prints **FINAL BANK $290**. `14-verdict-run-lost` prints **FINAL BANK $350**,
against a TARGET of $60 in the same drop's masthead frames. **The losing run ends with more money than
the winning one, and both clear the target by 5×.**

Two possibilities and they need different owners:

- **Capture data only** — the states were forced through a one-element payment schedule, not played,
  so the banks are arbitrary. Then it is harmless *except* that these two frames cannot be read as
  evidence of the win/loss distinction by anyone who was not told, which is what a capture set is for.
- **The verdict does not derive from final bank** — then a player can lose holding $350 against a $60
  target while the screen prints both facts side by side and explains neither.

**Say which.** If the first, re-shoot with figures that make the two states legible as themselves. If
the second, it is a design question and it comes back to this seat, because a verdict screen whose two
printed facts contradict its headline is the one place this product cannot afford ambiguity. *The
number never lies* is the rule the entire voice rests on.

## S58 — MY BETS: the tally restates the sheet. **NEW. Not part of the LEDGER grant (C31).**

On `05-my-bets-green-dead`, the left panel prints `TICKET 1 · DEAD` / `STAKE $35 · PAYS $97`. The right
tally prints `TICKET 1 · DEAD` / `2 LEGS · $35 → $97`. **The same four facts, twice, ~500px apart** —
the tally adds only a leg count the sheet lets you count.

S37 forbids exactly this. And the margin's job on MY BETS is **run context** — tickets this round, at
risk, what lands if everything lands — not a second rendering of the sheet beside the sheet.
`TV-OWNED TALLY · READ ONLY · NO SCORE · NO PROBABILITY` is doing good work at the head of that column;
what sits under it should be the run, not the ticket.

Found on frames inside the granted set. Recorded as its own item per C31, and the grant stands.

## S49 — The desktop's element-kit entry. **AUTHORED.**

**State change:** owed to this seat → **Authored · DD 2026-08-05**, against the re-verified desktop
(S52) at `d1a8382`. For transcription into the element kit as written.

```
DESKTOP — the machine with nothing running
1024 × 704. Bands: 34 rail + 636 wallpaper + 34 tray. No app chrome; there is no app.

RAIL (34, --ground-3, 1px --rule beneath)
  identity mark 11×11 --toner-3 · NOTEBOOK 12px/.13em --toner-2 600
  sticker  biro on 1px --biro-deep, rotated −0.6deg, 12px/.09em
  right    clock 12px/.1em tabular · battery 20×9, 1px --toner-3, cell --stamp when low

ICON COLUMN (left, single column, top-aligned)
  inset from rail   --st-pad-x (14px)   ← Allen 2026-08-04; "the standard margin" is not a token
  tile              --ground-3 chip, square, --radius 0
  glyph             centred, --toner for launchable, --toner-3 for dead
  label             12px/.09em beneath the tile, uppercase, matching the glyph's tone
  launchable        SURETHING, LEDGER          dead  MAIL, BANK
  a dead app is present and legible. It is his machine; it has apps he does not use.

WALLPAPER
  flat --ground, no image, no logo, no pattern. It is a cheap machine's default.

TRAY (34, --ground-3, 1px --rule above)  — as OsTray, unchanged
  running apps left · non-product system facts right · 12px is legal here and only here
```

**Two things this entry fixes by writing them down.** The rail-to-icon inset is recorded **as a
decision, not a lookup** — the corpus's phrase "the standard margin" appears twice, both times inside
S52, and defines nothing; `--st-pad-x` is the surface's only documented content inset and is now the
named answer. And the dead apps are specified as *required*, not tolerated: MAIL and BANK sitting
unused is the cheapest characterisation on the surface and a later cleanup pass would delete them as
dead weight.

**The measurement note goes in too**, because it will otherwise be re-derived by someone in three
weeks: on this wallpaper the `--ground-3` chip is a **3/255 step** (34,34,22 on 31,31,19), so a
rail-to-icon reading finds the tile edge at 86px and the glyph's first ink at 114px. Both numbers are
right and they measure different things. **28px of any reading of this column is the chip's own dead
space.**

## S56 — The launchable chip does not read as a chip. **NEW. The lead's re-open is granted.**

Which follows directly, and the lead was right to re-open it rather than let "chip/ground-3 fine"
stand. A **3/255 step is not a visible edge** at any viewing distance, let alone on a panel read at an
angle through the unified grade's grain and haze.

**The consequence is a law violation, not a taste note.** Four apps sit in that column; two launch and
two do not. If the chip is invisible, the only thing separating them is **glyph brightness** — and
status carried by tone alone, with no mark, border, label or position, is the thing this system bans
outright. The chip *is* the second channel. It has to be one.

**Instruction: the chip reads, or the launchable/dead distinction moves to a channel that does.** I am
not specifying which — a firmer value step, a 1px `--rule` edge, or dropping the chip entirely and
carrying it on a printed word are all legal, and the surface's own grammar prefers the word. The
requirement is two channels, visible at review distance, on rendered frames.

Note the shape: this is C18 in miniature. **An element that draws but cannot be seen is an element that
is not there**, and it passed review for the same reason the four vacuous gates did — because
something existed in the source.

---

## T60 — **STRUCK. It has no body, so it is not a ruling. And R34 was omitted from the same header.**

**State change:** listed in batch 9's header → **Struck · DD 2026-08-05.**

Batch 9's header names C27, C28, S51, S52, S53, R31, R32, R33, T58, T59, **T60**. Its body contains
no T60 section. The body *does* contain **R34** (BezelBlack retirement), which the header does not
name.

**Both errors are mine, and they are the same error:** the header was written against an intended
docket and never re-derived against what was actually written — which is, precisely, C23's shape and
the third instance this fortnight of a bound landing without its dependent re-derivation.

Under C22 a ruling exists when it is a row in the register. **T60 has no content, therefore nothing
to transcribe, therefore it is not and never was a ruling.** Strike it. No lead is waiting on it; no
work references it.

**R34 must be transcribed** — it is a real ruling with a real body (retirement stands; the evidence
trail is the finding) and it is at risk of being dropped precisely because the header did not list it.
**Confirm R34 is in the tables when this batch lands.** A ruling that exists only in a batch file is
the failure mode C22 was written to prevent, and it would be a poor joke for it to happen to the batch
that carries the constitution's transcription discipline.

Recorded per §1.5 as the seat's error.

---

## T61 — The scorer leg never reaching terminal. **Diagnosis routes to markets. The design question is CONDITIONAL, and I am pre-committing the answer.**

**Ruled · DD 2026-08-05.** The finding is C25-form and unusually good: it names four things the
instrument cannot see and three alternative explanations that would each make it a different bug.

**Diagnosis is not mine — route to the markets backlog, and test the round-advance hypothesis first.**
The lead's §5 case is strong enough that I want it excluded before anyone calls this a sim hang: the
harness polls `Tickets[0]` but tests completion on `director.CurrentSession`, **and those need not be
the same object.** If a round advances and `RevealedView.Tickets` is replaced, the harness reports a
hang for a ticket that settled correctly and a leg that has barely started. That fits the evidence as
well as an engine defect does, costs far less to test, and is a **harness-scope bug** — which would put
it back in the same family as this fortnight's other instrument failures rather than in the engine.

**On the design question in §6 — it is real, but it is conditional, and the condition is measurable.**
I will not spend a TV window on it while the leading hypothesis says there is no unresolved leg at all.
So:

- **If the cause is round advance → the design question is struck**, exactly as the lead proposes.
  Nothing was ever unresolved.
- **If the window is real but bounded → no change.** T17's reserve already makes the backed side read
  one goal short until the final sequence, ruled intended under T33; a bounded quiet stretch on one
  leg is that same design working.
- **If the window is real and unbounded → here is the answer, so nobody waits for me.** The leg does
  **not** sit at LIVE. `LIVE` is L3 and carries **the only pulse on the entire surface**; the pulse
  promises the leg is being played right now. A leg nobody is playing holding the surface's one pulse
  indefinitely is the surface lying about liveness — the same defect class as a bright cash-out slot
  whose key does nothing, which is ruled twice already (T24, T43, and again at T59 this week). It
  drops to structure and states its own condition literally.

**Nothing here blocks TV Phase 3.** T58 remains the TV lane's ranking item.

## C29 — A test run reports its case count, and zero cases is a failure. **LAW.**

**Ruled · DD 2026-08-05.** Promoted from §7 of that finding, where it is buried under "Reproducing it"
and is the most important sentence in the document:

> a bare `(48151623)` matches **zero tests** and exits green with `testcasecount="0"` — a run that did
> nothing, reported as a pass.

**That is the fifth vacuous green this fortnight**, after T19's signature diversity, T47's containment
epsilon, R16's collider count and S49's `Graphic`-less wallpaper check. Constitution §4.2 tabulates
four; this one is worse than any of them, because the other four were single gates measuring the wrong
thing while **this is the runner itself**, and it can green *any* suite from *any* seat with one
mistyped filter.

**Every test invocation reports its executed case count, and a run with zero executed cases exits
non-zero.** No verdict, gate, grant or Design-verified claim rests on a run that did not state how many
tests it ran. Retrofit the harness invocations; the cost is trivial against one accepted grant that
tested nothing.

**Add this row to constitution §4.2** when C24's draft is next amended. The table is the strongest
argument in that document and this entry is the strongest row in the table.

The lead lost one invocation to it and wrote it down for everyone else. That is the whole value of C25.

---

## R35 — Drab green must "read as green". **RULED: the requirement is struck. The swatch stays. Third refusal of an invented light source this week.**

**State change:** R33 confirmed open, sequenced after R32 and the mattress reading (batch 9) →
**Ruled · DD 2026-08-05**, on the escalation.

**Strike "reads as green" as a requirement. `--room-drab #3A4230` remains the specified albedo for the
bunk frames and mattress fabric, and it is applied.**

**This is R19(b)-am for the third time, and it is time to say so as a pattern rather than a ruling.**
R19(b) asked metal to read *colder* than the room. R31 re-weighted that onto finish because hue could
not carry it. R33 now asks fabric to read *green* under the same one warm key, on the same warm plaster
bounce, at the same L\* 10–18. **The room's lighting design cannot deliver hue-differentiated reads at
these levels, for any hue, on any surface.** That is Law 1.1's own mechanism — the same physics that
made T48's black point a room-wide problem — and it does not stop applying because this time we want
the hue to be there rather than absent.

**Both escapes are refused, explicitly, and for reasons already on the record.**

- **A light the bunks do not have** invents a source to satisfy a document. That is **T48's rejected
  Option D**, and it is the third time I have refused it this week. It also breaks the
  three-distinguishable-sources rule the room's lighting is signed off on: one warm key, one
  short-reach cool window, the screens. There is no fourth, and neither of the three is available as a
  tinting device.
- **Fabric where the pool is** moves the set to serve the camera. R32 already ruled the pool stays and
  the placement amends; relocating a bunk into the window pool inverts that, and it disturbs the
  two-bunk layout Allen signed off.

## C30 — A palette names materials, not perceived hues under a rig. **LAW.**

**Ruled · DD 2026-08-05.** Promoted from R35, and it is the general form of three separate escalations.

**A ratified palette specifies what a surface *is*. It does not promise what a camera returns under a
specific lighting rig.** Olive, khaki, drab green, rust, damp concrete under a warm dim fluorescent is
a **materials list** — it was always a statement about albedo, and reading it as a statement about
pixels is what produced R19(b), R31 and R33 in sequence.

Consequences, both directions:

- **A conformance audit checks that the specified material is applied.** It does not fail a material
  for not returning its own hue under the key, and a lead who applies the swatch has conformed.
- **A perceived-read requirement is a separate, explicit item**, phrased in a channel the rig can
  deliver — value, finish, relief, silhouette — and it names its rig and its camera. R31's finish-led
  metal is the worked example of the correct form.

This retires an entire recurring escalation shape. It also protects the palette: the fastest way to
lose drab green from the room permanently would have been to conclude that, since it does not read as
green, it should not be there.

**Sequencing unchanged:** R35 applies after the mattress-box discrepancy resolution (re-baselined onto
the pure sub-box per C27), and bunk 2's mattress is **re-measured against the 43.9 mean-luminance
requirement after the swatch lands.** A hue change must not become a value change — bunk 2 is the
legible-as-occupied test and that is a ratified measurement.

**Endorsed:** the room lead escalated rather than reaching for a light, and named the two costly
options himself instead of quietly taking one. Third time this seat has had that from that lane.

---

## C26 — Owning documents. **RE-SEQUENCED.**

**SureThing's is unblocked. I write it next session.** S53's category fixes are in, the ground is ruled
above, and the surface's load-bearing values — the band arithmetic, the margin's fixed order, the
one-baseline record, the ink grammar, the fact floor — are Design-verified and are not about to move.

The three items this batch opens on that surface (S55 chrome, S56 chip, S57 figures) **go into the
document as open items with their register IDs**, not as resolved ones. An owning document is allowed
to name what is unsettled; what it may not do is assert values that are about to change. That
distinction is the difference between an owning document and another `08`.

**TV's still waits on T58**, which is the lane's ranking item and touches the gold law directly.

---

## Ordering for the orchestrator

**SureThing:** S53-am (ground → `--ground`, re-shoot) → S55 (chrome) → S56 (chip) → S54 (audit every
`new Color(float…)`) → S58 (tally) → S57 (answer the question) → cross-round retention capture.
S53-am and S55 are one re-shoot; do them together.

**Room:** mattress-box resolution → R35 swatch → re-measure bunk 2 against 43.9 → R33/R32 placement.

**TV:** T58 → T42 → T44. T61's diagnosis routes to markets and blocks nothing here.

**Markets:** T61 round-advance test first, then the `09` renumber.

**Studio-wide:** C29's zero-case guard is retrofitted before the next verdict is granted on any suite
result, in every lane. That one is not sequenced behind anything.

**Awaiting Allen:** the constitution (C24) — plus one amendment already owed to it, C29's row in §4.2.
