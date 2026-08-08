# Register entries — 2026-08-07, batch 13

**Transcribe into `main-2/docs/design/REGISTER.md`.** Issued at authoring time per C22. Ruled from the
tables (current through batch 12), not from batch files.

New IDs: **T64**, **T65**, **C33**, **R38**. Closures: **S59**, **S60**, **S61**, **S62**, **T62**.
Rulings: **T63**, **S63-am2**. Amendments: **C26-am2**. Housekeeping: **batch-12's file issued**.

---

## 0. Housekeeping — batch 12's owed file

`register-entries-2026-08-06-batch-12.md` is issued with this batch, reconstructed from the tables and
labelled as such. The tables were never at risk; the audit trail ran through a relay for one cycle
instead of a document. **Recorded as the seat's error per §1.5** — C22 binds the seat first.

---

## SureThing — the four fixes. **ALL FOUR GRANTED. S59, S60, S61, S62 CLOSE.**

Measured on the batch-11 frames at `afb39ce`, one run, EditMode 76/76 + PlayMode 56/56 with counts
reported.

### S60 — the margin header is biro. **GRANTED · CLOSED.**

| | measured | token |
|---|---|---|
| MY BETS margin title | **96, 135, 185** | `--biro` |
| LEDGER `RECORD` title | **96, 136, 186** | `--biro` |
| the rule beneath, y181–182 | **64,104,149 / 63,103,148**, 2px | `--biro-deep` |

One component, one rendering, both destinations. The two titles now differ by one unit in one channel
— which is the grade, not the build.

**Endorsed:** collapsing both margins onto one shared `MakeMarginHeader` rather than fixing the wrong
one. S60 caught a component rendering two ways in a single submission; leaving two copies would have
been the third drift of that kind on this surface.

### S61 — scope stated once. **GRANTED · CLOSED.**

`TV-OWNED TALLY` → **`TALLY`**, margin subline gone. The board header and its subline stay.

The lead's reasoning is the ruling restated better than I wrote it: *"'TV-OWNED' was the third
assertion of ownership on the screen, and after S60 the biro marks the column anyway. What remains
names what the column contains — the one thing nothing else on the screen says."*

**The two rulings finish each other**, and the build noticed: the header returns its own height now, so
the hand-kept `-70` offset is gone. That offset would have been the next thing to drift.

### S62 — `R1 · TICKET 01`. **GRANTED · CLOSED.**

Frame 15 reads `R1 · TICKET 01` above `R2 · TICKET 01`, legs counting `1. 2. 3.` beneath. Zero-indexed
decimals gone.

**Two judgements above the fix, both right:**

- **The engine is deliberately untouched.** `Ticket.Id` is the DeriveRng key component; reformatting it
  would change what the game rolls. The key is read and translated, never printed. **That is the whole
  shape of the defect** — a legitimate internal key that reached the page — and it is fixed at the page,
  not at the key.
- **The round qualifier appears where it disambiguates and nowhere else.** `R1 ·` on the LEDGER, whose
  list spans rounds; bare `TICKET 01` on a staged receipt, always the current round, whose masthead
  already says which. Printing it there would have restated the run's scope — **S37 applied unprompted,
  against the lead's own fix**.

Also right: re-pointing the fixtures at the production formatter instead of restating its expression.
The old fixtures computed the identity the same way the render did and **would have asserted `1.0`
forever** — a test that cannot fail for the thing it exists to catch (C32, one surface over).

### S59 — the losing verdict drains as a group. **GRANTED · CLOSED.**

| | headline | subline |
|---|---|---|
| **won** (untouched) | 221,167,65 `--wax` | 221,216,201 `--toner` |
| **lost** (corrected) | **159,155,139** `--toner-2` | **112,108,97** `--toner-3` |

`NEW RUN` measures 221,167,65 on **both** screens. The joke keeps making itself.

**The finding from building the gate outranks the fix, and it corrects my ruling's reasoning.** The
obvious assertion — *headline outranks subline* — **fails on the winning screen**: wax measures 0.66
Rec.709 luminance against toner's 0.83. **Emphasis on this surface is not one scalar.** Wax outranks
toner by **chroma**; toner-2 outranks toner-3 by **value**. The losing screen is the one where both
elements are neutral and value alone does the ranking — **which is exactly why the inversion happened
there and nowhere else.**

That explains S59 completely, and it explains it better than "the seat set a value and never re-derived
the composition". The general form is the lead's and it is promoted below as **C33's second half**:
**a per-element value check cannot see a ranking.**

### Not covered, correctly stated (C25)

The MY BETS frame is still the fully-dead-ticket state, so the tally is photographed reading
`1 / $0 / $0`. The riding count in the label keeps `$0` self-explaining, but it is **not shown doing
its job**. A riding-state capture still does not exist. **Wanted** — it is the last unphotographed
state on the surface.

### R38 — a rig string is printing in a player-facing slot. **NEW — violation, T31 class.**

**NEW · DD 2026-08-07.** On `14-verdict-run-lost` the verdict subline prints:

> `FINAL BANK $40 · SEED verdict-RunLost`

`verdict-RunLost` is the **harness state name**, not a seed. T31 settled this class on the TV: seeds
are spec'd chrome and stay, but **harness seeds print in run-seed numeric form**. The same rule was
never applied to this surface, so the capture rig's own label is rendering where the run's identity
belongs.

It does not disturb S59 — the colour measurements are unaffected, and this is the third time a rig
string has surfaced in a player-facing slot (T31, S57's `TARGET $1`, now this). S57's lesson stated
plainly: **a capture whose rigging is legible is not evidence of the shipped product.** Here it very
nearly was.

**Ruling:** forced capture states take a numeric run seed like any other run. The state name belongs in
the filename, where it already is.

---

## T63 — the cash-out band never reaches L4. **RULED. And the instrument was mis-reading gold.**

### The isolation is accepted, in full

**The invert-before-label defect is not real, and the disproof is better than the claim was.** Three
independent checks, any one sufficient — code (one call sets flag, label and field; pinned by a named
test, green in 228/228), geometry (`_cashOutField` is sized to `grid.CashOut` and cannot paint outside
its own rect), and luminance (gold pixels above a 0.40 floor: 60,950 in the real field, **zero** in
frame 006).

**The box derivation answers my batch-12 error properly.** Solving the canvas→frame scale on *both*
axes and checking they agree to three decimals — 2.2204 against 2.2236 — is a real check that the
panel was framed and not the room. I verified `t63-box-validation.png` renders `MARKET SUSPENDED`
squarely inside the box. My own boxes had no such check, which is why they landed on the wall.

**The instrument-error disclosure is the standard.** A near-black warm pixel at (0.12, 0.09, 0.05)
scores 0.58 saturation at hue 34° and registers as gold — and the lead named it as *the second time in
this slice a low-luminance hue test produced a false positive*, then drew the right conclusion: in
T42's case raising the floor did **not** explain the residual; here it explains it completely. **Same
trap, opposite outcome — which is why it must be tested each time and never assumed in either
direction.** That sentence is worth more than the finding it retracts.

### The scope note is wrong, and correcting it changes the number

The note says the two absolute scales differ by a systematic ~0.14 offset between two box derivations,
unresolvable here. **It is not a box-derivation offset and it is not unresolvable.** Measuring the
lead's own box, on the lead's own frames:

| element | RGB-average (lead's unit) | **Rec.709 luma** |
|---|---|---|
| cash-out band, f000 peak | 0.663 | **0.827** |
| scoreline, f000 peak | 0.875 | **0.874** |
| scoreline, f006 peak | 0.890 | **0.889** |

The unit is **mean-of-RGB**, applied consistently to both columns — I reproduced every figure in the
table to three decimals, so the measurement is sound and the table is internally honest.

But the two elements are not comparable under it. **The scoreline is near-neutral (217,224,228) and
the band is saturated yellow (230,221,55).** RGB-average weights the near-zero blue channel at one
third; Rec.709 weights green at 0.72. **The unit systematically under-reports saturated warm colour —
which is precisely the colour the entire gold ration lives in.**

**Consequence: the reported gap of ~0.21 is 0.047.** And my own earlier 0.737 for the quiet scoreline
was neither unit — it was **linear-space** luminance. Three conventions have been in play across
T41/T58/T63, the thread that exists to decide whether gold reaches L4.

### The finding survives, and the real competitor is a different element

Under one unit, one frame, one method — `frame000`, cash-out **actionable**:

| element | Rec.709 peak |
|---|---|
| **the ball**, canvas 921,343 | **0.902** |
| scoreline | 0.874 |
| **actionable cash-out band** | **0.827** |
| ticket column | 0.786 |

**The designated L4 element is third on its own surface.** The finding stands — at 0.047 and 0.075,
not 0.21.

**The scoreline is not the defect.** Its quiet peak is 0.874, which is T41-cl's closed value of 0.737
in linear, unchanged — the scoreline has not moved since T41 closed. **It is the band that never
reached L4.**

**The ball is not the defect either.** `frame000` is grammar `GoalFor` — a payoff — and §7 makes the
ball L4-eligible at a payoff. C3's arbitration says a momentary punch preempts a sustained state, and
at 0.902 against 0.827 that is what the frames show. **C3's arbitration is working.**

### Ruling

**The actionable cash-out band renders above the quiet scoreline.** It is the surface's only sustained
L4 element and it must out-rank every sustained element around it. Sequenced after T64/T65 below,
which are louder.

**Re-read the whole ladder in the ruled unit before building anything.** Every ladder number taken in
RGB-average under-reports gold against neutral, so the ratified L-tier values and the gold ration's
own headroom both need re-reading. T41-cl and T58 are closed on ordering conclusions that survive a
unit change; **their absolute values do not, and neither do any tuning targets derived from them.**

---

## C33 — one ladder, one unit; and a ranking is not a per-element check. **LAW.**

**Ruled · DD 2026-08-07.** Two halves of one instrument failure, arriving the same day from two
surfaces.

**a. The ladder's unit is Rec.709 luma on display-encoded values, studio-wide, quoted with every
number.** Three conventions were in simultaneous use — RGB-average, Rec.709 luma, linear luminance —
producing a 0.21 gap where the real one is 0.047. RGB-average and linear both mis-rank a saturated
warm element against a neutral one, and this project's semantics put *money* in the saturated warm
element. **A brightness comparison that does not name its unit is not a measurement.**

**b. A per-element value check cannot see a ranking.** S53 was correct element-by-element and produced
an inverted composition; the SureThing lead then found that *headline outranks subline* fails on the
winning screen because wax reads 0.66 against toner's 0.83. **Emphasis is not one scalar** — wax
outranks toner by chroma, toner-2 outranks toner-3 by value. A ranking is asserted against the
composition, in the channel that carries it.

Joins C25 (scope), C28 (coverage) and C32 (resolution) as the fourth reporting axis: **unit**.

---

## T64 — the TV's idle emission flickers at 9 Hz. **RULED — struck.**

**NEW · DD 2026-08-07.** Raised by the room lead as read-only to that lane and correctly not touched:

`TvSweatScreen.idleEmissionFlicker = 0.05` drives a **9 Hz Perlin flicker on the TV's idle emission**.

**Struck.** It fails three separate laws, any one sufficient:

1. **The display is a decade old and works perfectly.** A flickering panel is the *broken* register —
   T8's exact ground, one channel over. Scanlines and the static crawl were removed for this reason.
2. **One pulse kind on the whole surface, and it is `LIVE`.** A second animated channel — running
   *underneath* the first, permanently — is R37 on the TV.
3. **It is always on.** Unlike the laptop's attention glow it has no fire condition, so it is a
   continuous involuntary motion in the player's peripheral vision for the entire sweat.

**Removed, not zeroed** — per R37's own reasoning, a dead dial invites the flicker back.

Worth naming: this is the second animated-emission defect found in a week, on two surfaces, by the
lane that owns neither. **Emission is not covered by any palette or motion gate on either surface** —
it reaches the player as light, and every instrument the studio has scans pixels. That is C18 §4.2's
largest remaining hole and it is now named on both owning docs.

---

## T65 — a leg win floods the room gold. **NEW — violation, T40 class, relocated.**

**NEW · DD 2026-08-07.** Found in T63's own frames. Neither report mentions it.

Measured, room regions **outside** the panel, same burst, same seed:

| region | `frame000` (GoalFor) | `frame006` (LegFinalWon) |
|---|---|---|
| wall above panel | 30,50,33 · hue **129°** · sat 40% · L 0.175 | 112,86,32 · hue **40.5°** · sat **71.4%** · L 0.345 |
| wall left | 33,48,38 · hue 140° · sat 31% · L 0.173 | 90,74,38 · hue 41.5° · sat 57.8% · L 0.293 |
| wall right | 27,35,30 · hue 142° · sat 23% · L 0.129 | 62,50,29 · hue 38.2° · sat 53.2% · L 0.201 |
| wall below | 30,43,32 · hue 129° · sat 30% · L 0.155 | 90,70,31 · hue 39.7° · sat 65.6% · L 0.280 |

**On a leg win the entire room rotates ~90° of hue, roughly doubles in luminance, and reaches 71%
saturation.**

### It is not light spill, and the frames prove it

**The room's gold is inversely related to the panel's gold.** In `frame000` the panel carries a large
solid gold cash-out field and the room is green. In `frame006` the cash-out band is **suspended and
dark**, the panel's only gold is one won-leg row and the L2 risk/pays footer — **and the room is
flooded**. A physical spill cannot run backwards from its source. This is an event-triggered re-tint.

A hue rotation of 90° is also not an exposure change. Exposure moves luminance; it does not rotate hue.

### Ruling

**This is T40's deleted full-field gold wash, relocated to a larger surface.** T40's words apply
without amendment: *a full-field wash spends the whole gold ration in one frame and is a celebration;
the win is carried where it is already carried.* Deleting it from the canvas and firing it into the
room is not a fix.

It is also, in effect, **a fifth light** — the loudest element in the composite frame, in a room whose
three sources are signed off and whose palette law names olive, khaki, drab green, rust and damp
concrete under one warm dim fluorescent. Sat 71.4% at 40.5° is in no room document.

**Following T45's precedent exactly — the mechanism stays, the colour goes:**

1. **Keep the re-tint.** C5 leaves it deliberately open and T45 endorsed the mechanism.
2. **It fires on settlement, not on a leg.** A leg win is not a payoff; there are three or four per
   ticket, and a room that floods on each of them has no register left for the one that pays.
3. **It stays inside the room's palette** — the room's own warm key sits at ~92°, and the laptop lid's
   sanctioned contribution at 85.1–85.3°. A saturated 40° amber is a new hue, not a warming.
4. **Bounded by measurement, not by eye**, and the bound is stated on the gate's own line — **V6** on
   TV's owning document.

**Standing consequence:** every previous TV frame review measured the panel and cropped the room. The
loudest thing the TV does to this game had been outside every box the studio has drawn. Room-region
readings join the TV's capture contract.

---

## S63-am2 — the lid glow. **BUILD GRANTED. The ~3× ceiling is STRUCK. The cue is SUSPENDED, not valued.**

**Amended · DD 2026-08-07.** Both zips arrived; both are measured.

### The rule is met, clause by clause, and verifiably

| ruled | built |
|---|---|
| warm near-neutral, **R ≥ G > B** | `0.038 ≥ 0.032 > 0.024`, preserved under ×3 |
| amplitude only | attention **is** idle × 3 — identical chromaticity *by construction* |
| ~3× max | **3.00×** exact |
| idle carries the same defect | both ends one colour; idle was the cool half of the old pair |
| **no pulse** | one step, held, stepped back; `attentionBreathHz` **removed, not zeroed** |

**Granted.** Chroma **68.97 → 0.24**; the room cast **355.7° red → 85.1°**, against the room's key at
92°. The violet is gone from the lid and from the room. R37 is satisfied — and refusing a lerp with a
duration ("R37 wearing a shorter clock") is the ruling read correctly rather than complied with.

### The ~3× ceiling is struck — its premise is falsified by the lead's own measurement

I set ~3× to protect §1.2's *quiet, with faint spill* — to stop the lid becoming a fourth light. The
measurement:

> Into the room, all three warm builds are **identical**: chroma 12.3–12.4 at hue 85.1–85.3°. Only the
> struck violet differed.

**The lid's contribution to room colour does not vary with amplitude at all.** The desk lamp owns that
pool. A bound that does not bind the thing it was set to bind is not a bound — R19(b)-am2's shape, and
this time the falsified premise is mine.

### But the ceiling is not what is stopping the cue, so I am not raising it

Exhibit `01` is the honest version and I have looked at it: **idle, 3.00× and 4.07× are hard to tell
apart.** The step is 51.13% of the pose changing at **mean magnitude 6.5/255** — about 2.5%, near JND —
and the ruled ceiling roughly halves an already-marginal step. Raising 3.00× to 4.07× moves it by 4.8.

**Tuning a sub-threshold cue upward is C10 run backwards.** The cue does not fail because the ceiling
is low. It fails on mechanism, in two ways the lead named himself:

- **When it fires, it is not in frame.** `Glow()` runs on `wantsYou && !engaged` — the player at the TV.
  The struck-vs-built diff on the ratified **seated** pose is **0.00%, bit-identical**.
- **It is only visible in a mode the player never sees.** In Play Mode the lid's emission sits *behind
  the SureThing canvas* and what reaches the room is swamped by the desk lamp. *"This is the only state
  in which the thing being ruled is visible at all."*

### Ruling

1. **The colour is granted and ships now** — both ends. `idleEmission` is always on and must not be
   cool for 99% of the running time; that half is unconditional.
2. **The ~3× ceiling is struck.** The value is reopened, not answered.
3. **The cue is SUSPENDED pending one frame: Play Mode, `wantsYou && !engaged` true, a pose that
   contains the laptop.** Disposition pre-committed, so nobody waits on me twice:
   - if it reads → take the amplitude that reads, bounded by the room-cast measurement, not by ~3×;
   - if it cannot be framed → **the cue is struck** and only the idle colour remains.

**I am not setting a second value blind.** S59 is three weeks old and was exactly that.

### Recorded

The lead reported, unprompted, that the first strike **was never built** — changing a public field's
default does not touch an already-serialized component, so the scene kept the violet and the first A/B
captured the value it was supposed to be replacing. Caught only because the method logs each value as
it writes it. **That is C18 §4.2 and constitution §2.5 in one paragraph**, self-diagnosed, and it is the
second self-correction from that lane in this item.

**Standing, unchanged:** room captures near the laptop are contaminated by C13 until the surfaces'
content is re-integrated.

---

## T62 — the progress line. **GRANTED on one beat. Scope stated.**

`frame000` shows a live leg reading **`REGULATORS TO WIN / LEADING 1-0`** against a scoreline of
**`ZAMBONIS 0 — 1 REGULATORS`** at 64', with Regulators backed. **The progress line agrees with the
scoreline in the same frame** — the exact disagreement T62 recorded.

**Scope (C25):** one beat, one seed, one grammar, from a capture built for another purpose. The lag was
a sustained multi-beat error, so a single agreeing frame is evidence and not proof. **Granted
provisionally; a live-leg beat in the next TV set closes it.**

---

## C26-am2 — owning documents. **TV's DRAFTED. SureThing's needs a one-line amendment.**

**Amended · DD 2026-08-07.**

**TV's owning document is drafted:** `tv-design-2026-08-07-DRAFT.md`, canonical home
`docs/design/tv-design.md` on approval. Ten sections — the register split and the never-broken rule,
**the brightness ladder and its unit**, the rationed-gold colour law, type and hierarchy, Layout B and
the fixed grid, the six-state cash-out slot and the rest of the component law, quantised motion, the
event-strip register, **seven real gates each naming what it cannot see**, and a quarantine section for
everything still provisional.

The quarantine is longer than SureThing's and that is honest: every TV hex is unratified against the
real panel (T12), the shipped gold disagrees with its token, `goldInk` sits below the surface's own
black floor, and the two type tables disagree. **Section 10 also reconciles one live canon conflict:
C3's "boost stays 1.8" is superseded by T49-cl's sealed 1.4** — later, explicit, and matching the
current capture set. Recorded as reconciliation, not a new ruling.

**SureThing's owning document (Allen-approved, canon) needs one amendment:** S59–S62 move out of §10's
open items into the body, and **R38** joins them. One edit, orchestrator-side.

---

## Ordering for the orchestrator

**Louder than the thing that was reported:** **T65** (room floods gold on a leg win) outranks
everything else in this batch. **T64** (9 Hz idle flicker) is a one-line deletion and should go with it.

**TV, in order:** T65 → T64 → re-read the ladder in C33's unit → T63's band. **Nothing tunes against a
ladder number taken in the old unit.**

**SureThing:** S59–S62 close; **R38** is one line in the capture rig. Wanted: a riding-state MY BETS
capture — the last unphotographed state on the surface.

**Room:** the corrected glow colour ships now, both ends. The cue waits on one Play-Mode frame from a
pose that contains the laptop; the disposition is pre-committed either way.

**Awaiting Allen:** TV's owning document. Nothing is blocked behind it.
