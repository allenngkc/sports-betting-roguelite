# Register entries — 2026-08-07, batch 14

**Transcribe into `main-2/docs/design/REGISTER.md`.** Issued at authoring time per C22. Ruled from the
tables (current through batch 13), not from batch files.

New IDs: **S64**, **S65**, **S66**, **S67**, **T66**, **C34**, **R39**, **R40**.
Closures: **R38**, **S63** (entire). Amendments: **S63-am3**. Grants: riding, pending, R38 frames.

---

## 0. Seat error — R38 is mis-prefixed

I allocated **R38** in batch 13 to a **SureThing** finding. `R` is the room series; the room's last
item is R37. The studio has already adopted R38 for the seed item and re-keying delivered work costs
more than it saves, so **R38 stays where it is and is a SureThing item despite its prefix**. The
room's next items take **R39** and **R40** in this batch. Recorded per §1.5 so the collision does not
land later.

---

## S63-am3 — the lid glow cue. **STRUCK. The pre-committed disposition fires.**

Batch 13 suspended the cue on one frame and pre-committed both outcomes so nobody waited on me twice.
The frames arrived. **Play Mode, all three ratified poses, cue-on against cue-off:**

| pose | changed | mean │d│ | max │d│ |
|---|---|---|---|
| `focused-laptop-desk` | **0.00%** | — | **0** |
| `seated-tv-couch` | **0.00%** | — | **0** |
| `standing-overview` | **0.01%** | 5.13 | **9 / 255** |

Two of the three are **bit-identical**. The third changes one twentieth of one percent of the frame,
inside a 47 × 101 px box, at a peak delta of 3.5%.

**The disqualifying result is the seated pose.** The cue fires on `wantsYou && !engaged` — *the player
at the TV*. `seated-tv-couch` **is** that condition, and it produces **zero changed pixels**. The cue
is invisible in the exact circumstance it exists for.

**And `focused-laptop-desk` is bit-identical too** — the pose that frames the laptop directly. In Edit
Mode that same pose moved 51.13%. The collapse from 51.13% to 0.00% between Edit and Play Mode
confirms the lead's own diagnosis: **in the shipped product the lid's emission is behind the SureThing
canvas.**

**Struck.** Not tuned — R36's precedent is directly on point, where wear sitting **0.90% above JND**
was ruled *absent, not faint*. This is an order of magnitude below that. Raising the amplitude is C10
run backwards, and the ceiling was never what stopped it.

**What stands, unchanged:** `idleEmission` and the granted warm near-neutral colour ship. That half
was always unconditional — the lid is always on, and it must not be cool for 99% of the running time.
`attentionEmission` as a *colour* stays correct for as long as the field exists; it is the *cue* that
has no observable effect.

**S63 CLOSES.** Struck violet → granted warm colour → struck cue. Three sessions, and the only thing
that survived is the half that never needed a frame.

### Recorded, from the same frames

**C13 is live and it contaminated this evidence.** The laptop in `cue-on-standing-overview` is
rendering the **superseded violet package** — visibly purple, with a magenta action bar. The room lead
has flagged this contamination twice and was right both times. This is an integration item, but it
means every room capture taken near the laptop, **including the ones that just settled this ruling**,
photographs a surface that has been retired for a week. The ruling survives it — a diff of cue-on
against cue-off cancels whatever both frames share — but nothing else about the laptop in a room frame
does.

---

## SureThing — R38 and the two captures. **GRANTED. R38 CLOSES.**

### R38 — the rig string. **CLOSED.**

Measured on the frames: `FINAL BANK $290 · SEED 40719355` and `FINAL BANK $40 · SEED 68204137`.
All-digit, 8 characters, ordinary members of `NewSeed`'s space. **The rig vocabulary is gone from the
one slot on that screen where a product fact belongs.**

Right, and for the stated reason: **numeric rather than merely seed-shaped**. T31 is the precedent —
a harness seed shaped like a label (`TVCAPTURE01`) was read as a debug token and cost a withdrawn
finding. A seed the player could have been dealt cannot be misread as apparatus by anyone, including
this seat.

**Two guards added because nothing was watching the line** — the seed must be numeric, and the subline
must actually print it. Named exactly: *"the suite read the headline's colour through two rounds of
review and never read the line underneath it."* That is a C18 §4.2 self-diagnosis, and the second
guard is the better one.

**S59 re-verified on the new frames:** lost headline 159,155,139 `--toner-2` over subline 112,108,97
`--toner-3`; won headline 221,167,65 wax. **`NEW RUN` full wax on both.** Unmoved.

**`15-ledger-across-rounds`'s rig seed: leave it.** Recommendation accepted, and the reasoning is
sound — the seed renders on the verdict screen and nowhere else, leg count is a function of the seed,
so re-seeding re-rolls the content of a frame this seat has already granted. **Cost with no visible
benefit.** Noting the asymmetry deliberately rather than hiding it: a rig label survives in a state
whose own screen never prints it.

### `04a-my-bets-riding` and `04b-my-bets-pending`. **GRANTED.**

The state a ticket wears for its **entire life until settlement** had never been in a frame, on the
screen whose whole subject is tickets in flight. Now it has, along with the leg-level state that
precedes it.

Measured on the pending/riding pair:

| element | measured | token |
|---|---|---|
| margin header `TALLY` | 96,135,185 | `--biro` ✓ |
| rule, 2px | 65,107,153 | `--biro-deep` ✓ |
| `TICKETS THIS ROUND 1` | 219,214,199 | `--toner` |
| **`AT RISK · 1 RIDING  $35`** | **219,214,198** | **`--toner`** ✓ |
| **`IF EVERYTHING LANDS  $85`** | **220,166,65** | **`--wax`** ✓ |
| leg `GREEN` | 221,167,66 | `--wax` ✓ |
| leg `LIVE` | 218,213,198 | `--toner` ✓ |

**This is the first frame to show the ratified stake-toner / payout-wax split on figures that are not
zero.** Batch 10 ratified it against `$0 / $0`; it now stands on `$35 / $85`. S60 and S61 both hold on
the new frames.

**The three MY BETS states are one ticket's whole life on one slate** — PENDING·PENDING → GREEN·LIVE →
GREEN·DEAD, with the tally going `$35 / $102` → `$35 / $102` → `0 RIDING · $0 · $0`. Read as a
sequence, they are a better argument than any of them alone.

**Endorsed:** the letter suffix rather than renumbering a set the register, the owning doc and the
handoff all cite by name. **Delivered evidence does not get renamed.**

---

## S64 — the MY BETS mirror never received S62. **RULED — violation.**

**NEW · DD 2026-08-07.** Three sites on this surface print a ticket identity; S62 reached two.

| Site | Prints |
|---|---|
| LEDGER — `TicketIdentity(…, withRound: true)` | `R2 · TICKET 02` |
| Staged receipt — `TicketIdentity(…, withRound: false)` | `TICKET 02` |
| **MY BETS mirror — hand-built string, helper never called** | **`TICKET 1`** |

**Ruled: the mirror routes through `LaptopUi.TicketIdentity(…, withRound: false)`** — the staged
receipt's exact call, for the staged receipt's exact reason. MY BETS mirrors the current round, whose
masthead already states the scope, so a round qualifier there is S37 restatement.

**The padded form is not a judgement call and the lead is right that the kit already settled it.**
`TicketReceipt.prompt.md` names this screen by name — *"shown on ENTRY after PLACE TICKET, in MY BETS
during the sweat, and in the Ledger once settled"* — and `TicketReceipt.d.ts` gives the form as
`TICKET 01`. **One component, three screens, one identity.** The "nothing to align to" reading was
the better of the two offered, and it is still wrong: alignment was S62's *illustration*, not its
reason. The reason is that an identity is one object wherever it appears.

**The self-supersedure is the most valuable thing in this filing.** The lead raised it as an open
question with two defensible readings, then read the kit, found it answered, and **superseded their own
report an hour later rather than letting me rule on a question that did not exist**. And they named
why it mattered: the C14 audit made this exact mistake on this exact surface six days ago — concluding
the kit did not specify the LEDGER because there was no `components/ledger/` directory. **Same error,
same surface, caught only because filing the question required going to look.**

**The shape, now four deep on this surface** — S33's margin header, S34's ruled ground, S60's biro
header, now this. Each time a second call site hand-builds what a shared helper already builds
correctly. The lead's formulation is exact and is promoted into S67:

> **A ruling can only reach the call sites that route through the thing it was ruled on.**

---

## S65 — PENDING renders one step too bright. **RULED — violation. Measured on the frame.**

**NEW · DD 2026-08-07.** The lead reported this as a source-and-kit reading and stated plainly that
they had **not** sampled the rendered pixel. **I have.** On `04b-my-bets-pending-flat`, both legs:

| | measured | token |
|---|---|---|
| `PENDING` state word | **158, 154, 138** | **`--toner-2`** |
| S43 and `RevealedState.jsx` both require | — | **`--toner-3`** (110,107,94) |

`SportsbookApp.cs:1342` drops PENDING into the `else` branch with VOID. **The build collapses two
states the kit distinguishes, and its own comment cites the kit file it does not match.**

**Ruled: `--toner-3`.** And ruled at the composition level, not as a token swap — S59 is three weeks
old and was exactly that mistake:

The state column currently ranks GREEN (wax 221) → LIVE (toner 218) → PENDING (toner-2 158) → DEAD
(toner-3 + strike). Moving PENDING to `--toner-3` puts it level with DEAD, and the lead correctly
escalated that rather than shipping it.

**It survives, for three reasons that hold together:**

1. **DEAD carries three channels to PENDING's one** — the word, the oxide strike across it, and the
   row drained to .55. Owning doc §3.3. They are not confusable.
2. **`--toner-3` is the label tone** — field keys, column heads. A PENDING leg *is* structure: it has
   not happened. Putting it at the tone this system uses for structure is correct, not a demotion.
3. **The TV already solves this exact adjacency the same way.** `NEXT` is L1 and `L` is L0 — the
   not-yet and the dead sit in adjacent tiers, separated by the extinguished ground and the strike,
   never by type tone. Two surfaces, one answer, arrived at independently.

**Shooting it as built was right.** Fixing first would have left no photograph of the violation, which
is the evidence needed to confirm it — and confirming it is what my measurement just did.

---

## S66 — the capture set has never been reproducible. **RULED. Pin every flow.**

**NEW · DD 2026-08-07. This outranks every capture in the drop, and the lead says so first.**

`RunDirector.seed` ships **blank**, which the director reads as *roll a fresh 8-char seed*. Every
`Boot()`-based capture flow has dealt **a different slate on every run**. Measured, same state, two
consecutive runs of the same code:

| Run | `05-my-bets-green-dead` |
|---|---|
| batch 11, `a235bfc` | `Tulsa Plumbers v Pawtucket Ferrets` · −516 · **PAYS $71** |
| next run, `6ece398` | `Sheboygan Bricklayers v Waterloo Zambonis` · −410 · **PAYS $85** |

**Measured, not inferred** — which is the difference between this filing and a suspicion.

**Ruled: pin every capture flow, one flow per commit, next slot.** The recommendation is accepted as
given. The betting flow is pinned at `52830174` and **asserts the run is carrying it before shooting**
— the assert is the part that matters, because a pin nobody checks is a comment.

### What this does and does not do to the record

**No grant is withdrawn, and here is the scope of that claim.** Every verdict this seat has issued on
SureThing frames has been a **treatment** judgement — colour tokens, ink assignment, tone ranking,
layout, copy. **Treatment is seed-stable**; the lead's own framing, and it is correct. S34's rule
pitch, S59's drain, S60's biro, S61's scope, S62's identity form and this batch's tally split are all
independent of which teams were dealt.

**What was never reliably testable is anything content-dependent** — string widths, overflow,
truncation, long-name collisions. There is a recorded instance: the latent width flake in
`SureThingEntryTests`, which passed on every run whose generated names happened to be short and failed
on the first long one. **That class of finding could appear and vanish between submissions with
nothing changed**, and no one would have been able to tell which.

### The self-correction

The lead told the orchestrator that `05`'s content was *"identical to the granted frame"* and had not
checked — asserting reproducibility from the shape of the change rather than from the frames, in the
same submission that then disproved reproducibility outright. Reported unprompted, with the table that
disproves it. The decision it was offered to justify was harmless; **the reason given was false, and
saying so is worth more than the decision being fine.**

---

## C34 — evidence that cannot be reproduced is not a set. **LAW.**

**Ruled · DD 2026-08-07.** Promoted from S66.

**A capture set is a set only if re-running it produces the same frames.** Where a flow rolls its own
content, "the same twelve states" names twelve *states*, not twelve *frames*, and any comparison
across two runs of it is a comparison of different subjects.

Consequences, standing:

1. **Every capture flow pins its seed and asserts the run is carrying it before shooting.** An
   unasserted pin is a comment.
2. **A finding that depends on generated content is not established by one run of an unpinned flow**,
   in either direction — neither its presence nor its absence.
3. A submission that compares frames across runs **states whether the flow was pinned**, in the same
   breath as the comparison.

This is C11's precondition rather than an extension of it: rendered evidence has to be evidence *of
the same thing twice*. Joins C25 (scope), C28 (coverage), C32 (resolution) and C33 (unit) — the fifth
axis is **reproducibility**.

---

## S67 — the helper-bypass sweep. **RULED — its own item, as asked.**

**NEW · DD 2026-08-07.** The lead declined to scope it inside S64 on the grounds that *"naming it
inside this one would bury it."* **Correct, and granted as its own item.**

Sweep the laptop surface for **strings composed by hand where a shared `LaptopUi` helper already
produces the value** — identities, money, odds, state words, counts, scope lines. Report the
inventory; do not fix inside the sweep.

**Why it outranks the fix it came from:** four instances of this shape have now been found one at a
time, each by a capture built for another purpose, over six days. The defect is not any one string —
it is that **a ruling lands on a helper and silently misses every screen that does not call it**. An
inventory converts that from a recurring discovery into a bounded list.

Per C18: the sweep **names its members and states what it cannot see** — in particular whether a hand
-built string is a bypass or a legitimately different value.

---

## T66 — the event strip's tier. **RULED — L2, all seven narration sites.**

**NEW · DD 2026-08-07.** Filed as a question because the canon does not answer it, and it is right
that it did not: `tv-design.md` §8 rules the strip's content and voice at length and never assigns it a
tier; §3's token table does not list it. **The seat correctly did not treat the louder reading as a
violation on its own authority.**

**Measured, in C33's unit, on `frame000`:**

| element | Rec.709 | declared tier |
|---|---|---|
| ball (payoff punch) | 0.902 | L4 at a payoff |
| scoreline (quiet) | 0.866 | L3 |
| **event strip** | **0.858** | **not ruled** |
| cash-out band | 0.820 | L4, the only sustained one |

The instrument reproduces all four of my T63 figures exactly, which is the calibration that makes the
new number usable. And the honest statement is the one given: at 0.008 the strip and the score are
**not separated at all** — below the instrument's resolution, so neither leads (C32, applied without
being asked).

**Ruled: `AtTier(flavorColor, TierL2)` on all seven narration sites.**

Three reasons, in order of weight:

1. **§4.1 — nothing outgrows the score.** A sustained element sitting level with the scoreline for the
   entire match is the ladder carrying no hierarchy at that end, which is the exact defect `AtTier`'s
   own doc comment was written to fix. **The strip is named in that comment's list of offenders**, and
   the fix reached score, clock, NEED and progress and stopped. It is the one element in its own fix's
   list that the fix did not finish.
2. **§2 — brightness is the primary semantic channel.** The strip explains; the score is the fact.
   Explanation at the same brightness as fact is a claim that they rank equally.
3. **It matches what the three resolution beats already do**, and those cite TV-05's *"the strip stays
   neutral"*.

**The split is not ratified as a rule.** The seat flagged the third possibility — deliberately loud
while the match runs, quiet when a leg resolves — and asked for it to be written down if intended. **It
is not intended.** One tier, seven sites, one rule; the three resolution beats already have it.

**Recorded, and the seat should not have had to weigh this:** the history is legible from the file
itself, and the lead read it correctly and then told me **not to weight it heavily**. That is the right
handling of a mechanism-versus-intent question.

**T63 is not gated by this, as stated, and the diagnosis inside is worth more than the note it sits
in:** the L4 boost was wired to the money *figure* and never to the gold *field*, so the field could
not be boosted at all **and** was painted in the L3 gold. That is a complete causal account of T63,
volunteered in a filing about something else.

### The gate blind spot — C18 §4.2, and it is a new kind

`L4_canvas_elements_get_the_hdr_material_and_L3_elements_stay_default` has been green throughout, and
it **explicitly asserts the strip does not carry the material**, commented *"only one L4 element at a
time."*

**Carrying the material and sitting at the tier are different claims.** An element without the HDR
material can hold the L4 *value* at alpha 1.0; it simply cannot exceed 1.0. The strip does exactly
that. **A gate written against this precise risk read green while the condition it names was true.**

This is not a vacuous green — it executed and asserted something true. It is narrower and more
dangerous: **a gate whose name describes a proposition stronger than the one it tests.** Added to §4.2
as its own row, and it generalises to any surface whose ladder gate checks materials instead of
composited luminance. **TV's owning-doc gate V1 already specifies composited Rec.709 luma** and should
be read as superseding the material check, not supplementing it.

---

## R39 — the phone's emission. **RULED — struck, same family as S63.**

**NEW · DD 2026-08-07.** Filed in C25 form throughout, and calibrated against the register's own
ratified figure — the struck laptop violet reproduces at chroma 64.1 / hue 312.4° against the
register's 64.1 / 312°. **A measurement that first reproduces a known value is a measurement I can
use.**

| value | L\* | chroma | hue | order |
|---|---|---|---|---|
| **granted laptop idle** | 21.09 | 5.4 | **83.3°** | **R>G>B** ✓ |
| `idleEmission` — **always on** | 20.06 | **14.5** | **278.9°** | B>G>R |
| `unreadEmission` — **live in the batch-13 frame** | 37.80 | **18.0** | **264.5°** | B>G>R |
| `buzzEmission` | 75.22 | **31.9** | 271.4° | B>G>R |
| `PhoneBuzzLight` | 90.58 | 16.3 | 241.7° | B>G>R |

**The phone's always-on rest state carries 2.7× the chroma of the laptop's granted rest state, in the
blue quadrant §1.1 names as its own failure mode.**

**Ruled, and it needs no frame for the same reason S63 needed none:**

1. **`idleEmission` and `unreadEmission` are struck.** They join the laptop's granted family — the
   phone is **his** (§6), personal register, same as the laptop. Warm near-neutral, **R ≥ G > B**.
   `unreadEmission` is not hypothetical: it was **live in the batch-13 capture**, logged at the moment
   of the shot, and it is the normal state during Betting with an unread feed.
2. **`buzzEmission` and `PhoneBuzzLight` are struck as colours and kept as an event.** The lead is
   right that a 0.55s flash is not R37's continuous pulse and right not to flatten them together. But
   a blue flash at chroma 31.9 driving a real `Light` is a cool source in a room whose palette law
   forbids one, intermittent or not.
3. **The rendered reading confirms the authored one**: the phone's face goes L\* 16.66 → **36.31** and
   chroma 1.61 → **7.84** between screens-dark and screens-lit, while the laptop-body control is flat
   across the same pair. **That is what an emitter looks like against what a body looks like.**

**Exact values wait on the same instrument S63's did.** Direction only: the screen's own emitted
family, warm, low chroma, R ≥ G > B. **I am not setting three values blind** — and unlike the lid,
these are observable, so the frame is obtainable.

**`design/08` is cited inline as the authority** for `GrayboxRoomBuilder.cs:886`'s cyan blink —
*"a tiny cyan/white blink is chrome, never money-green (design/08 palette law)."* **`design/08` is T3,
the deprecated anti-reference, dead since 2026-07-24.** A live comment in shipped code citing a
deprecated document as its licence is C7's shape inside source rather than docs. Delete the citation
with the value.

### The blind spot, and it is the finding

**No instrument in this studio reads an emission value and judges it.** Four look like they would, and
each is silent for its own correct reason: R23 forces the panel emissions to black *by construction*;
R33 checks which material asset is referenced; T30 matches named constants verbatim (`PhoneBuzzLight`
is `(0.55,0.82,1.0)`, `chromeCyan` is `(0.62,0.86,0.96)` — same family, hand-typed differently,
invisible); and R19's only phone region is read on the **screens-dark** set.

**The sole region that samples the phone reads it with its emission silenced, and the sole rendered
§1.1 instrument silences it on purpose.** That is how S63 could rule *"idleEmission is the same defect
unaudited — fix both"* about the laptop's two ends while a third emitter sat 15cm away.

**This is now the third emission defect in eight days** — the laptop lid (S63), the TV idle flicker
(T64), the phone (R39) — **on three surfaces, none found by a gate.** Emission reaches the player as
light and every instrument the studio owns scans pixels or source constants. **It is the largest
uncovered channel in the project.** Named on all three owning documents; an instrument for it is
scoped as its own item, not folded into a fix.

---

## R40 — the laptop's *material* emission contradicts the granted colour. **RULED — violation.**

**NEW · DD 2026-08-07.** `ScreenLaptop`'s material emission is `(0.025, 0.055, 0.035)` — **hue 155.5°,
chroma 13.5, G>B>R.** The granted lid colour is **83.3° / 5.4 / R ≥ G > B.**

**The material disagrees with the ruling by 72° of hue at 2.5× the chroma, and it is green-dominant in
a project where green is retired game-wide (C4).**

At runtime the property block overrides it, so the *player* sees the granted colour. **What does see
the material's own value: the APV bake, and every Edit-Mode capture.** So it has been baked into the
room's indirect light, and it was the value present in the Edit-Mode captures that settled the glow's
colour one batch ago.

**Ruled: the material carries the granted value.** Correctly not fixed by the lead — changing a baked
emission re-opens the bake and the structural gates, which is a sequencing call and mine. **Sequence
it with R39** so the bake is re-opened once for both, and re-walk what the bake voids (C28: Gates 6–8
expire on a content-fingerprint change; no tool re-issues a human gate).

**The general form, and it is why this is its own item rather than a footnote:** a runtime override
that hides a wrong authored value **does not make the value right — it makes it invisible to the one
audience that can report it.** Every previous check of this surface looked at what the player sees.

---

## Ordering for the orchestrator

**Room, one bake:** R39 (phone emissions + the `design/08` citation) and R40 (laptop material) open the
APV bake together, once. Re-walk what the bake voids. **The glow cue is struck — no build.**

**TV:** T66 is seven one-line changes and one rule. It does not gate T63, which proceeds as built.

**SureThing, in order:** S66's pinning (one flow per commit) **before** anything that will be
photographed — otherwise the re-shoots land on fresh slates again. Then S65 (one token), S64 (one call
site), and S67's sweep as its own item.

**Standing:** **C13.** The room still renders the retired violet laptop package, confirmed on this
batch's own frames. Every room capture taken near the laptop photographs a surface retired a week ago.
Integration item, and it is now old enough to be named on its own line.

**Awaiting Allen:** TV's owning document (batch 13). Nothing is blocked behind it.
