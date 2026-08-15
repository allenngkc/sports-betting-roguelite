# SureThing — the laptop surface
> **APPROVED — Allen, 2026-08-06.** Canon under C9's two-tier authority; open items (S55/S56/S57-era leftovers named inside) tracked in the register, not re-litigated here. The DRAFT file is the preserved draft.


**Owning document** under C9's two-tier authority · **Status:** APPROVED — Allen 2026-08-06 (C26-am) · **Drafted:** Design Director, 2026-08-06
**Canonical home:** `main-2/docs/design/surething-design.md`

*(Status line corrected 2026-08-12 at seating — it read `DRAFT for Allen` beneath this document's own
APPROVED banner. Factual bookkeeping, not a clause amendment: the same drift `room-design.md` carried
for ten days and had repaired at the 2026-08-10 seating, and the third instance of the class the
constitution's §1.1 note names. This was the last of the four owning documents still carrying it —
the §1.1 table itself is correct and was checked against all four headers at this seating.)*
**Companion:** `docs/design/constitution.md` (authority and evidence) · **Sibling:** `docs/design/room-design.md` (R13)

---

## 0. Scope and precedence

This is the binding art authority for **the laptop** — the SureThing sportsbook app, the LEDGER, and
the NOTEBOOK OS chrome the two run inside. It carries the colour, type, layout and state law that the
constitution deliberately excludes.

Precedence: Allen → the constitution (authority and evidence) → **this document** (everything about
this surface) → the register's ruling for the item → the slice's specs.

Every clause below is a transcription of a ruled register row. Where a clause states a value, that
value has been measured on a rendered frame, and the frame is named. **Nothing here is new law.**

---

## 1. What this surface is

The occupant's own cheap machine at 2 a.m. **The Annotated Form Guide** (S1, approved 2026-07-28,
world 4 of 7): the house prints a dense inverted betting form; the player compares it, circles prices
in ballpoint, works the right margin, commits. **Selection is annotation.** The document never changes;
only his marks accumulate.

### 1.1 The register split — the constraint most likely to be got wrong

| | The laptop | The TV |
|---|---|---|
| Object | **his own machine** — personal, chosen, cheaper, grubbier, possibly customised | a hardened display **an institution bolted to the wall** |
| Register | **Calm.** A tool you operate. | **Loud.** An instrument you watch. |
| Owns | slate, markets, working slip, stake, staging, lock, shop, placed tickets | unrevealed drama: score, clock, probability, outcome reveals |
| Motion | laid ink — continuous, hand-paced | panel refresh — quantised, discrete |

**Do not carry the TV's vocabulary here.** Not its coarse grid, monumental type, institutional steel,
brightness-only semantics or un-eased motion. Anything on this surface built from that vocabulary has
become a second TV and has failed.

MY BETS may only mirror what the TV has already revealed (S35c). It never reads engine state and never
runs ahead of the broadcast. **The laptop decides; the TV reveals.**

### 1.2 Ownership boundary

**The house owns the app. The player owns the machine.** (S44) The wallpaper, the rail, the tray, the
icon set and the sticker are his; SureThing and LEDGER are what he runs on it. The machine never wears
the house's brand — the wordmark is drawn in biro, and biro is only ever what *he* chose.

---

## 2. Canvas and bands — locked

**1024 × 704**, one fixed composition on a ~0.32 × 0.22 m world-space surface, read at an angle
(S2). Not responsive, not a web page.

```
 34  OS rail          --st-band-rail
 38  app tabs         --st-band-tabs
 68  masthead         --st-band-mast
530  work area        --st-band-body     = 700 house form + 324 player margin
 34  OS tray          --st-band-tray
───
704
```

**A locked band is not headroom** (R30). No band is spare space regardless of what it currently draws —
the tray draws little and carries the entire personal-machine register. A proposal to grow into a band
**cites the band by name**.

**THE RELATION STATEMENT'S PIXELS — NO BAND CAN PAY** (S80 as amended by S80-am, 2026-08-15, on
Allen's *pay the pixels* ruling). **The action band is not the donor, and this is arithmetic**: its
160 carries three ruled control heights (130) plus 24px of separation plus the 6px pad, so absorbing
36 would put `SkipBandY` at −20, below the panel floor. **Rail and tray are BARRED** — pixel-identical
chrome across destinations (S48, S52) — and **tabs cannot yield 36** with `--st-tab-h 27` inside 38.
**And the masthead — the one band batch 73 cited — cannot pay either, before any judgement is
applied: its 68 IS its content.** `FormMasthead` carries two stacked text lines occupying 48px
(`Brand` 26px, `Run` "ROUND n OF m" 13px, `Figures` BANK/TARGET/TICKETS 21px alongside), between an
8px top pad and an 8px clear above the 2px rule. **Spacing yields ~8px; deleting the round-number
subline yields ~22 and costs S37's once-on-the-surface fact; a one-line masthead yields ~26. 36 is
unreachable.** **S50 already refused this trade on this budget** — it cited the locked
`34+38+68+530+34=704` as the reason *there is no unused screen on this surface*, and the masthead's
68 is a term in that sum; its granted remedy took all 44px out of the FLOW. **R30 is a bar, not a
door: citing a band by name is the minimum to be heard, never the argument that wins.** **And a
Design-verified masthead changes only by closing a conformance gap, never by redesign** (Allen,
2026-08-08). **The 6px pad survives**: S51 refused paying a content cost out of the separation
budget, and this is the same trade. **A deficit that survives the yield order below returns to the
DD seat and then to Allen — it does not become a donation from chrome.**

**Yield order for any layout deficit: spacing, then repetition, then nothing** (S50). Nothing that
states a product fact is deleted to make a layout fit, and no hierarchy is reordered for a shortfall
(T51). A deficit that survives that list returns to the DD seat.

Re-deriving a fixed grid constant **once at design time** is legal (T51, S40). A zone **resizing in
response to content at runtime** is not.

### 2.1 The margin

324px, ruled paper at **26px** (S34 — measured on frame `15-ledger-across-rounds`, rules at
y 425/451/477/503, identical on working and passive margins). Vertical order never changes:

**header → legs → combined → stake → payout → actions.**

The action stack is **anchored** (T47). It does not flow: an un-anchored stack moves LOCK IT IN
depending on how many legs were marked — the most consequential control in the game moving because you
bet more. The flow region above it is bounded by MaxLegs = 4 and the reserved height is re-derived in
the same commit as any change to that cap.

The margin **does not scroll**. Interior market lists do (S25-am), with S27's rail.

### 2.2 Control sizes — exact

`--st-place-h 44 / min 200` · `--st-lock-h 52 / min 280` · `--st-skip-h 34 / min 230` ·
`--st-price 96×30` (kit) / `112×32` (runtime) · `--st-more 74×44` · `--st-tab-h 27` ·
`--st-quick 68×32` · `--st-nudge 88×32` · `--st-rub 60×32` · `--st-entry-h 78`.

`--radius: 0`. **There are no rounded corners and no cards on this surface.**

RUB OUT stays 60×32 with its word: an explicit removal target, never a tiny unlabelled ×. A mis-click
here costs money.

Ink geometry: **ring = cell + 16px, offset −8/−8**, additive, derived from the real control's measured
rect — never proportional, never eyeballed.

---

## 3. Colour

### 3.1 The two-ink rule — the surface's first law

| Ink | Value | Means | Never |
|---|---|---|---|
| **Wax** | `#D9A441` | money and the primary action | mood, celebration, emphasis |
| **Biro** | `#5E86B8` | anything **he** chose | a product fact, a primary action (S18) |
| **Stamp** | `#B4483A` | the house acting on the document | generic loss, generic error, decoration |

**Nothing else may borrow any of the three.** `--wax-ink #1A1305` is the type punched out of a solid
wax field; `--wax-deep #8A6620` is the 2px pressed edge.

**`THE HOUSE'S LINE` is a named use of Stamp** (S73, Allen 2026-08-12). Where two of his picks are
priced as related, **the house marks the connection between them in Stamp** — he picks in Biro, the
house marks in its own ink. That is this table's third row doing exactly the job it was written for:
*the house acting on the document*. **A correlation is an annotation**, which is what this whole
surface is.

**The mark is DRAWN, not CAPTIONED.** The line carries no label; the name is what the thing is
*called* — rules copy, the ledger, a first-encounter explanation — never a tag beside every
occurrence. A mark that needs a caption every time is a mark that is not working, and the house does
not narrate its own presence on his document (S44).

Money grammar, ratified on frames and **not to be "fixed"**:
- **Stake reads toner; payout reads wax.** In the MY BETS tally that is `AT RISK` toner (218,213,198)
  and `IF EVERYTHING LANDS` wax (220,167,65) — the working margin's own grammar, one screen over.
- **`$0` wears wax in a tally** (a sum of zero is money arithmetic) and **`--toner-3` in a row**
  (a dead record is drained). Both correct (batch 10).

### 3.2 Grounds and toner

`--ground #16160F` canvas · `--ground-2 #1C1C13` recessed · `--ground-3 #232319` raised ·
`--rule #3C3C2C` · `--rule-soft #2C2C20`
`--toner #D9D4C5` facts · `--toner-2 #9C9888` secondary · `--toner-3 #6E6B5E` labels, **floor for
readable text**

**No pure black anywhere** (S2). The ground is lifted so the room's shadows stay darker than the
laptop — the strongest belongs / does-not-belong signal in the project.

**Every screen shares `--ground`.** The verdict is the app's last screen, not a different product
(S53-am): measured 22,22,13 on `13-verdict-run-won`, matching the desktop's own ground. **No screen
authors a bespoke ground.**

### 3.3 Status is never colour alone (S2)

Every state carries a mark, glyph, word, border or position change as well as its tone.

| State | Carried by |
|---|---|
| Selected | biro ring over the figure + a leg in the margin + the count |
| Replacement | biro ⇄ + dashed underline — **never** rendered disabled |
| Blocked | muted label + stamped literal reason ≥13px, cause **and** remedy, inside the control |
| GREEN | the word + wax figure + ring re-inked in wax |
| DEAD | the word in `--toner-3` + the oxide strike across it + the row drained to .55 |
| VOID | the word + the stake printed as a KNOWN sum + the entry rubbed out — **never the oxide strike, never drained to DEAD's .55** (S76; treatment is a candidate pending frames) |

Measured on `05-my-bets-green-dead`: DEAD glyph rows at (110,106,95), strike rows at (181,74,59).
**Only the strike is oxide** (S15-am). Leg sub-rows carry **no per-outcome hue** at all (S35c, S40).

**A refused leg combination is a Blocked state and takes Blocked's treatment** (S73-am4, correcting
the register). Where two picks cannot both land, the second is refused at the slip with a **stamped
literal reason ≥13px, cause AND remedy, inside the control** — this table's own row, not a new
treatment. **Never a disabled control** (S24 bans the disabled state outright; S56 bans a distinction
carried in a channel he cannot see), and the leg **stays reachable on its own**, because the engine
prices the leg and refuses only the *combination*.

Cause and remedy, both, because this row has always required both: *these two cannot both land* names
the cause; what to drop names the remedy.

**The remedy is CONJUNCTIVE and both halves are authored for the PLURAL** (S73-am5, 2026-08-14, on
sgp's measurement: **remedies of up to three legs occur at the shipped `κ = 1`**, across 645 refusals,
for duplicates and impossible combinations both). **Removing only the first element leaves the slip
refused**, so the remedy is a set to remove, not a menu to choose from. **`or` / `either` / `one of` /
`any of` are BANNED in a remedy** — English's natural form for a list of fixes is disjunctive and the
model's truth is not, so the idiomatic phrasing is the wrong one. **A remedy that names a fix which
does not fix it is worse than no remedy**: S73-am4 requires a *verified* remedy, and a failing
instruction at the point of spending is S17's own subject.

**The cause is plural too, and has no honest degradation.** `… cannot both land` is two-valued;
three or more legs take an authored `… cannot all land`. **Two authored forms chosen by arity, never
one template with a substituted word.** **A duplicate and an impossibility take ONE treatment and TWO
causes** — §3.3 requires a *literal* reason, and one vague sentence covering both is what that word
exists to prevent.

**The stamp states the ACT and its ARITY; the legs are MARKED in the flow, never named in the stamp**
(S77, 2026-08-14 — the built stamp overflowed its control). **The overflow is caused by leg NAMES, and
the names do not belong there**: up to three names inside the PLACE control's `296 × 44` at 17px is
unbounded in the worst case, and **the instruction is not.** So the stamp carries the act and how
many, and **the legs it refers to are marked on their own rows in the flow directly above.**

**This is T69/T70's principle one control over — the subject is already on screen, so do not reprint
it.** Marking serves the no-translation goal *better* than matching strings, because the referent is
pointed at rather than merely worded alike. **The check that makes it safe, and it passes: the flow is
bounded by MaxLegs = 4 in a 370px region and does not scroll, so every marked row is on screen
whenever the stamp is.** A mark that could scroll out of view would fail this and the rule would not
stand. **The mark vocabulary already exists** — biro ring, oxide, the `RUB OUT` control on each row.

**THE CONTROL DOES NOT GROW, and the coupling is why.** `ActionBandReservedHeight = PlaceBandY 110 +
PlaceBandH 44 + 6 = 160`, so `MarginFlowBudget = 530 − 160 = 370` — **every pixel of control height
comes 1:1 out of the flow budget**, and **S51 has just shown that budget is already overhung.**
**A copy problem is not paid for out of a geometry budget that is already over.**

**13px IS RATIFIED, AND THE REASON IS HEADROOM** (S77-am, 2026-08-15, measured). At 17px the widest
authored form measures **295.1px in a 296px control — nine tenths of a pixel** — and the second widest
288.2px. At 13px the widest is **225.7px, 76%**. **13px is not merely the floor being respected; it is
the only one of the two with headroom, and headroom is what stops the next authored form from
reopening this.** **RULED: every future stamp form is measured at 13px against 296px and stays under
80%.** **C46 is why there is a budget at all** — a fixed box carries an unstated assumption about the
face it was sized against, and ~20% is what absorbs a face that measures wider. **A form over 80% is
re-authored, never accommodated.**

**A REMEDY NAMES AN ACT.** Where no removal fixes the slip, the remedy names **the act that does** —
clearing and starting again — in the actual control's own word. **A remedy slot filled with a
cause-shaped string** (*no rub out fixes this slip*) tells him only that what he was about to try will
not work and **leaves him with no act at all**; a refusal that closes every door is the one case where
he most needs to be told which door is open.

**Nothing else yields**: `≥13px` is the cross-surface product-fact floor; **cause AND remedy** is
S73-am4; truncation is refused (*a truncated remedy is an unverified remedy*); shrinking type is
refused (§8). **If the authored forms still miss, the order is (1) a shorter authored form, (2) two
lines inside the existing 44px box at ≥13px — a real option, not a last resort — and (3) only then
geometry, which comes to Allen with the flow-budget cost stated.**

**Where a remedy does name a leg, it uses the exact string on that leg's own row**, so he never has to
translate an instruction against the rows in front of him. **Fit is measured, not estimated: the population is the
645 refusals and the longest renderable remedy is computable today** (C46). If it does not fit, the
control is sized for it or a shorter form is **authored** — **a truncated remedy is an unverified
remedy.** Removal order is an implementation constraint and **never reaches the player**.

**A bet that cannot win must never be purchasable.** A price is a factual claim about an outcome, and
selling a finite price on an impossible event is the product lying in the one place it has promised
not to.

**A legal-but-pointless leg is NOT a Blocked state and does not take Stamp.** Where a second leg adds
no risk — one pick already contains the other — the leg is **legal, correctly priced, and added**; the
machine simply **states the fact in toner, in its own space**: *this adds nothing; the first already
contains it.* Silence there would let him be quietly charged for a leg that cannot lose, which is a
cost he cannot see at the point of spending (S17). **Blocking it is refused** — a redundant leg is a
legal bet, and **a house that prevents him from being stupid is not this product; a house that tells
him and lets him do it anyway is.**

**THIS AND THE `Implies` RELATION STATEMENT ARE ONE STATEMENT, NOT TWO** (S78, 2026-08-15). The model
emits `RelationKind.Implies` as a principal on 10.6% of placeable same-match slips, and it fires on
**this same situation** — one pick contains the other. Both are toner, both are once per slip, and
**two code paths would otherwise ship two toner sentences for one fact** — T69/T70's defect in a third
place. **The statement above governs, because it carries the COST.** A statement of the entailment
alone (*one of these already covers the other*) states the structure and **omits the consequence**,
and the consequence is the only part he can act on — S17's *quietly charged for a leg that cannot
lose* is the whole reason this line exists. **The single authored line states that the leg adds
nothing AND which leg it is**; withholding which is right for every other relation and wrong here,
because here the naming is the actionable part.

---

## 4. Type

### 4.1 Faces

**Archivo + Archivo Narrow**, SIL OFL 1.1 (S11). One superfamily, shared metrics, tabular figures in
both widths. Condensed for figures, prices, names, masthead, action labels; roman for running text and
labels. Resolved through `LaptopScreen.LoadFont` alone.

No licence-encumbered typeface ships in this product, ever (Allen, S11).

### 4.2 Scale and the fact floor

`31` payout · `26` masthead / stake · `21` bank, target · `19` names, prices · `16` legs, actions ·
**`13` product-fact floor** · `12` OS chrome only.

**13px is the floor for any text a player must read to make a decision.** Prices, records, field keys,
state words, disabled reasons, market navigation. 12px exists only for OS chrome carrying no product
meaning — the tray's `DISK 61% FULL`, the clock.

`NOT INSTALLED` sits at the machine's own register and renders at the tray's tone (measured 111,107,96
= `--toner-3`, identical to the tray's system facts). It states what is **true**, never what is planned.

Every critical value survives a 50% thumbnail check.

### 4.3 Tracking

`.03` names/prices/masthead · `.08` records · `.11` tabs · `.12` field keys · `.14` actions ·
`.15` margin header. Signed numbers use **U+2212 MINUS**, no per-region exception (S30).

Tracking is a **signed C14 deviation** in `UI.Text` (S28), expiring at the C15 TMP migration. Signed
only where the colour split and two-voice type split are 1:1.

---

## 5. Chrome and destinations

**One `NotebookChrome`**, consumed by the app, the LEDGER, the desktop and the verdict alike (S48,
S52 — rail band 100% pixel-identical desktop-vs-app across 17,408 samples; tray likewise).

- **Rail** — identity mark, his sticker, clock, battery. 12px. Never institutional hardware.
- **Tabs** — FORM · ENTRY · MY BETS · REWARDS. **No tab is active on LEDGER** (S31-am): the LEDGER is
  a destination on the machine, not a section of the sportsbook — the strip persists non-interactive
  at `--toner-3`.
- **Masthead** — carries **the run's scope**. Board header carries **the screen's scope**. **Nothing
  restates either** (S37). The live round number appears exactly once.
- **Tray** — other apps, non-product system facts.

**The chrome is the argument.** It renders on every screen including the verdict (S55). A full-screen
takeover is a game-over card, and this product does not have those.

### 5.1 One name

**SURETHING**, everywhere the player sees it — icon, tray, masthead (S46). `Sportsbook` and the
taskbar full stop are gone; FORM is a screen, not part of the name. **LEDGER** is the one name for the
settled record (S16); "Old Slips" and "SURETHING LEDGER" are retired from copy. Code identifiers exempt.

### 5.2 The desktop

Wallpaper is the lifted ground plus toner grain, nothing above the fact floor. **The machine does not
wear the house's brand** (S44). MAIL and BANK are **required, not tolerated** — dead apps are the
machine having a life outside the app (S49-au).

Two icon states, two channels, both measured on `11-desktop`: glyph tone **220 vs 112**, and the
printed word at **85 above ground** where the retired chip managed 3 (S47, S56).

---

## 6. Voice

**The number never lies.** Copy is impersonal and transactional — it names the thing, not the reader.
"2 selections", not "you've picked 2". Second person only in genuine instructions.

First person appears **exactly once** in the whole surface, and it is not the product speaking — it is
him, in the column he owns: **MY MARKS**.

Personality: incisive, nocturnal, dry, orderly. Never celebratory of gambling, never fake urgency,
never an implied guaranteed win **in any voice** (S45).

Satire is permitted only in non-critical flavour labels that state no product fact — the sticker, a
tray readout, a relic description. **It never occupies a slot where a fact belongs**, and on an
otherwise empty screen the slot is the whole screen (S45).

Fictional leagues, teams and players only.

Errors state **cause and remedy, in place, at 13px, inside the house's stamp** — and inside the control
they explain (T47). Never a tooltip, never colour alone, never a remedy the engine did not supply.

---

## 7. Motion

**The laid-ink rule.** Continuous, hand-paced, caused by marking the document. **No duration and no
easing curve is specified, deliberately** — do not invent a motion system here.

Leg goes live → the entry lifts toward toner, his ring holds. Wins → the figure fills wax, the ring is
re-inked over it. Dies → the strike is drawn across, the entry drops toward ground. Return changes →
the tally is crossed out and rewritten beneath, in the same ink. PLACE press → 2px down, the wax-deep
edge drops.

Banned: confetti, pulse loops, casino urgency, celebration, any full-field wash.

---

## 8. Out of bounds

1. **The modern sportsbook app** — navy ground, one saturated accent, rounded odds pills, a floating
   betslip drawer, promo rails. The superseded violet package shipped exactly this; it is the
   anti-reference.
2. **The retro terminal costume** — phosphor green, scanlines, ASCII borders.
3. **Cyberpunk neon-on-black** — the city is neon; **his cheap laptop is not.**
4. **Skeuomorphic kitsch** — torn edges, drop-shadowed paper, faux stitching.

Also banned outright: colour-only status · sub-floor product text · low-opacity facts · hairline
essential strokes · pure black · a disabled odds control (v0 has no limiting, so no market can honestly
be unavailable — the correct treatment is *replacement*) · a padlock on an alternative price ·
a toast over a read-only mirror (S35b) · promotional rails · acquisition art.

---

## 9. Gates

Real gates, per C9. Each states its instrument and, per C18 §4.2, **what it cannot see**.

| # | Gate | Instrument | Blind to |
|---|---|---|---|
| L1 | Band arithmetic sums to 704; every band named | layout assert, canvas-local px | rendered glyph bleed |
| L2 | No text below 13px states a product fact | **which constant each slot uses** (`MakeText`'s `Mathf.Max(13, …)` clamp, checked against the face's own metrics once at design time), slots named — *amended batch 15: a rendered frame cannot separate 12px from 13px (both render 9px of cap ink at canvas-local resolution), so the frame-check instrument was specified coarser than the distinction it exists to make (C32)* | the rendered result; TMP point size and UGUI pixel size are not the same quantity |
| L3 | Two-ink conformance — no wax, biro or stamp outside its meaning | palette scan incl. markup + rendered frame | HDR emission (§10, no capture path exists) |
| L4 | Every state carries ≥2 channels | rendered frames, both states side by side | states without a forced capture |
| L5 | Chrome is pixel-identical across all destinations | per-band sample comparison | anything outside the two bands |
| L6 | Anchored stack never moves at MaxLegs with a staged receipt | PlayMode margin invariant | horizontal collisions, z-order, `Graphic`-less elements |
| L7 | Every priced offer reachable (C19) | list-length vs engine offer count | whether the rail rendered |

Every invocation reports its executed case count and exits non-zero on zero cases (C29).

---

## 10. Open items

| Item | State |
|---|---|
| **S59** verdict hierarchy — lost headline dimmer than its own subline | Ruled batch 11; unbuilt |
| **S60** MY BETS margin header renders toner, not biro (S33) | Ruled batch 11; unbuilt |
| **S61** read-only scope stated four times on MY BETS (S37) | Ruled batch 11; unbuilt |
| **S62** `TICKET 1.0` — zero-indexed, decimal-ambiguous ticket identity | Ruled batch 11; unbuilt |
| **S63** `attentionEmission` saturated violet on the lid | Struck batch 11; replacement waits on a frame that does not yet exist |
| **C15** TMP migration — S28/S29 deviations expire at it | Scheduled, orchestrator-side |
| **S10** sweat "loud register" for the laptop | Candidate; no built spec |

**Not open:** S53-am, S55, S56, S57, S58 (figures), S34, S49 — all granted on the batch-10 frames.

---

## Amendment — 2026-08-07 (C26-am2, orchestrator-side per the DD's instruction)

S59, S60, S61 and S62 are **closed on measured frames** (batch 13) and move from
§10's open items into body law: one biro margin header on every destination;
scope stated once; ticket identity prints as `R2 · TICKET 02` (the engine key is
read and translated, never printed; the round qualifier appears only where it
disambiguates); the losing verdict drains as a group (headline `--toner-2`,
subline `--toner-3`), NEW RUN full wax on both screens. **R38 joins the open
items:** forced capture states take a numeric run seed — a rig string never
prints in a player-facing slot.

---

## Amendment — 2026-08-08 (batch 15, transcribed by the orchestrator)

**C15 Phase L is GRANTED and merged to main (`5903750`).** The TMP migration
verified on the final set against the pinned before-set — every product-fact
slot identical ink at identical scanlines. **The signed type deviations are
expired: S28 (tracking), S29 (tnum), S20 (weight 600), and markets' ladder
letter-spacing.** S8 and S52 re-verified on the same evidence; the OS chrome's
Design-verified status stands. The roman voice is **Regular 400** (S20closed —
the rail identity's 600 is the one deliberate weight); **masthead run figures
render in the condensed face** per §4.1's own assignment (S29 closed — the TMP
Regular face declares no `tnum`).

**New ruled items:**

- **S68** — tracking values are the kit's, not the category's: `.08em` on SKIP
  (`--st-track-rec`), `.04em` on stamped reasons. §4.3's principle: short
  labels are tracked uppercase; **factual copy stays literal** — a
  label-plus-instruction is factual copy. Recovers the SKIP headroom by
  construction.
- **S69** — disabled action grounds conform to the kit: PLACE disabled fills
  `--ground-3`; LOCK disabled is transparent with a `1px --rule` border (§2.2:
  a 52px ruled control in both states). One commit with S68.
- **S70** — the three untracked values ruled: LedgerEntry legs line →
  `--st-track-name` (.03em); the rail identity's `.13em` **stays and is
  tokenised as `--st-track-chrome`** (a named exception with one member is
  still named); the staged-receipt header renders the kit's **three
  trackings** (identity / count / state — the receipt's grammar).

**§9's L2 gate is amended in place** (see the gate table): the constant check
is the instrument; the frame cannot resolve 12px from 13px, and the gate now
says so in its own line. The owed frame-check is retired, not deferred.

---

## Amendment — 2026-08-08 (batch 17, transcribed by the orchestrator)

**S68, S69, S70 GRANTED and closed** on the re-shot receipt frames. Inside
S70's grant: the ticket's identity had rendered in the money ink — `PAYS` is
now the only wax on the receipt (S3 enforced where S70 never looked).

**Three questions RATIFIED AS BUILT — do not re-open, do not "fix":**

1. **The receipt footer stays 13px throughout.** The receipt is index, not
   display (T29's distinction); the margin's `POTENTIAL PAYOUT` is where this
   screen shouts, and a printed form sets key and value at one size.
2. **The `$0` payout keeps its wax highlight at zero selections.** A sum of
   zero is money arithmetic; the highlight marks the slot, not the amount.
3. **The disabled PLACE fill (4/255) is never deepened.** It is a supporting
   channel; the dimmed label and the stamped reason carry the state, and a
   deeper value would be a fourth ground in a three-ground palette.

**New ruled item — S71:** the margin's empty state spoke with two voices
(`MY MARKS` is him; `YOUR MARGIN IS CLEAR` addresses him). Ruled: name the
state, not the owner — `NO MARKS ON THIS SHEET`. §6 unchanged: second person
only in genuine imperatives; first person exactly once, and it is him.

**Three instruments, three spaces (C33-am3, studio-wide):** the brightness
ladder is Rec.709 luma on display-encoded values; a contrast ratio is
relative luminance in linear space; emission hue/chroma is CIELAB on linear
authored values. Every measurement states its space as well as its unit.

---

## Amendment — 2026-08-12 (S73, S74, C47 — the same-match ticket and the draw)

### `SAME MATCH` is its own instrument — never a parlay with an adjustment

Two or more legs on one match are correlated, so the ticket **cannot be priced by multiplying the
legs**. The book prices the true joint. **The surface never displays a product-of-legs figure for one
of these tickets, and never displays an adjustment, a correlation discount, a was/now, or any
deduction line.**

**The reasoning, because it will be argued with:** the "nerf" reading is manufactured by the
comparison, and the comparison is manufactured by showing the product. A slip printing
*1.85 × 2.10 × 3.40 = 13.21 · adjustment −32% · price 8.98* has **literally rendered a number being
taken away from him.** No copy fixes that. **There is nothing to deduct from if nothing is presented
to deduct from** — and a same-match ticket is one bet on one compound outcome, which *has a price*.

**`SAME MATCH`** is the instrument's name (Allen, 2026-08-12), uppercase like the rest of the market
vocabulary — a role printed as a word, a fact rather than a brand. **`SGP` is industry jargon and
never reaches him.**

### What he sees, and the rule it teaches

**The mark, and no arithmetic.** With `THE HOUSE'S LINE` on the connected picks (§3.1), his complete
and *correct* mental model needs no maths at all:

> **unmarked legs multiply; marked legs pay less.**

**The mark is what MAKES his multiplication work** rather than what replaces it — it tells him exactly
when his own arithmetic applies. Independent tickets price *identically* to the multiplied number, so
he is right about half the time; the mark is what tells him about the other half.

**Where a statement is needed it states the RELATION, not the name** — what the legs *share*, in
toner, once per slip. **The pricing model must therefore emit a nameable relation, not only a
coefficient: where a correlation cannot be labelled, the price does not move.** A price that shortens
for a reason the surface cannot state is a cost he cannot see at the point of spending (S17).

**THE AUTHORED SENTENCES** (S78, 2026-08-15 — approved against the model's emitted `principal` over
6,109 placeable same-match slips). **Four relations, seven sentences: sign is not decoration**, and
reinforcing and opposing are opposite claims about the same shared thing, so one sentence per relation
would state one of them falsely about the other.

| relation | reinforcing | opposing |
|---|---|---|
| `SharedScoreline` | `THE SAME GOALS SETTLE BOTH.` | `THE SAME GOALS SETTLE THESE OPPOSITE WAYS.` |
| `ScorerOfSide` | `THE SAME TEAM'S GOALS SETTLE BOTH.` | `THE SAME TEAM'S GOALS SETTLE THESE OPPOSITE WAYS.` |
| `SharedCount` · corner | `THE SAME CORNERS SETTLE BOTH.` | `THE SAME CORNERS SETTLE THESE OPPOSITE WAYS.` |
| `SharedCount` · card | `THE SAME CARDS SETTLE BOTH.` | `THE SAME CARDS SETTLE THESE OPPOSITE WAYS.` |

**These are a FAMILY, deliberately, and are never re-authored apart.** The shape is not a template
applied to save effort — **the shape IS the claim**: every one of these relations is *one shared thing
settles both legs*, and the sentences differ **exactly where the relations differ** and are identical
**exactly where the relations are identical**. **That is what distinguishes a family from templating:
the variation tracks the meaning.** The payoff is his — canon already fixes his model as one idea
(*unmarked legs multiply; marked legs pay less*), so **after the first encounter he reads only the
DIFFERENCE**; four idioms would make him re-parse a whole sentence to learn what he already knows.
**`GOALS / CORNERS / CARDS` is a triple of countable match events and that parallelism is what makes
the family read as one** — `SCORELINE` was considered and refused for breaking it.

**`ScorerSide` is carried by the model and is NOT spoken.** Naming the team would be a name where the
rubric asks for the relation, and **the team is on both rows in front of him** — the same reasoning as
S77's *mark, don't name*.

**And no mark treatment is owed for it** (S78-am, 2026-08-15 — an earlier disposition sending this to
the mark is **withdrawn as misconceived**). **The statement's job is to explain the PRICE, not to
identify the teams**, and *the same team's goals settle both* discharges that completely: two legs
riding on one team's scoring, therefore correlated, therefore shorter than multiplying. **Which club
it is changes nothing about why the price moved.** **The decisive fact is that HE CHOSE BOTH LEGS** —
Biro is this surface's ink for *anything he chose*, so **he cannot be uninformed about which picks he
just made.** The mark could not have carried it in any case: **the mark is DRAWN, not CAPTIONED** and
already means one thing, **a side-tinted line is barred by S2**, the ink table forbids a fourth ink,
and a shape variation is a code he must learn (S56's class). **Where the relation runs through the club
the visible row does not name, he may read *the same team's* as the club he can see — weighed and
ACCEPTED**: it does not affect the decision (the legs pull against each other, which the sentence
states), and `ONE TEAM'S …` would break the family's shared opening for a low-stakes sub-case.

**Fit is not asserted.** The seven measure against their slot like everything else and join the
sweep's population under C46.

**THE SILENCE IS CORRECT AND IS NOT A GAP TO FILL** (S79). **46.1% of placeable same-match slips emit
no statable relation**, and that is this rule working rather than failing: **a null principal means
the price did NOT move, so there is no cost to disclose because nothing was taken.** The statement
exists to explain a price that shortened; **where nothing shortened, nothing is owed — the silence is
not an absence of copy, it is the absence of a thing to explain.** **A high blank rate is what a
correctly-behaving model looks like from the surface**: if every slip carried a statement, the model
would be labelling relations it cannot name, which is the precise failure this rule forbids. **No
statement is ever authored to fill it, and no review reopens it on the strength of the number alone.**

**The lengthening is not remarked.** No badge, no "better value", no flag. A product congratulating
itself for charging less is exhortation, and nothing ever claimed the price was a product.

**No formulae on the face.** No coefficient, no multiplier, no percentage — values appear when the sim
emits them as first-class, never computed in presentation.

**The fairness fact is true and stays off the slip.** A same-match ticket carries the same house edge
as the equivalent cross-game parlay; **a house that explains its own fairness at the point of spending
reads as a house with something to explain** (S44's boundary).

### The draw on the board

The moneyline is three-way. **Three offers, the draw in the MIDDLE** — its position is *meaning*, not
borrowed convention: the draw is the outcome where **neither wins**, and either end would make it look
like a third competitor. **It is not a team**, the engine having ruled that structurally, and the
surface does not undo it: named `DRAW`, no team treatment, no team hue.

**A two-outcome paired row no longer describes this market** (S24) and must not be forced to.

**`1X2` never reaches him** — a code word for a market shape, which stays in the code.

#### The composition (S74-am, 2026-08-14)

A matchup is **not one row with two price cells**. It is **two stacked lines, one per side, with the
price on the same line as the team it belongs to** — and **the price cell carries the OUTCOME word,
not the team**: `AWAY −156`, never `NOTARIES −156`. The left column names **who**; the price cell names
**which outcome**. **The board therefore already has the draw's grammatical slot, and nothing is
invented.**

```
NO.  MATCHUP · SEASON RECORD          MONEYLINE       MORE
01   NOTARIES   4-5                   AWAY  −156     ┐
                                      DRAW  +240     │ MORE ›
     FERRETS    5-4                   HOME  +127     ┘
```

- **`DRAW` goes in the price cell**, exactly where `AWAY` and `HOME` live. **Never in the matchup
  column**, which names teams.
- **The matchup column is empty on that line, and empty is the correct rendering of "neither".** This
  is **not S24's dead cell** — S24 refused an *offer* slot with no offer; here the *subject* slot has no
  subject. Naming anything there invents the third competitor `Side` refuses.
- **The middle position is literal**: the draw's line sits physically between the two teams', attached
  to neither, which is what the outcome is.
- **No team treatment on that line** — no dot, no crest, no hue.
- `MORE ›` spans the block, now three lines.

**The block is three lines whether or not a given matchup prices a draw.** A block height that depends
on the market is a zone resizing to content, which §2 forbids; an empty line is honest where a
collapsing block is not.

**A third line per block shows fewer matchups at once. That is not a deficit to yield against** — a
third outcome is a product fact arriving, not a layout overflowing, and §2 binds: nothing that states a
product fact is deleted to make a layout fit. **Reachability holds by a mechanism that already exists**
— the interior list scrolls (S25-am) with S27's printed position rail.

**`MONEYLINE` stands as the column header.** It names the market, not the number of outcomes.

### Settlement language does not change (C47)

**The match has three outcomes; a bet has two.** Back the draw and it draws — **he won**. Back a team
and it draws — **he lost**. **No third result word, no new state, no new column.** Inventing a `DREW`
state would model the match inside the bet's vocabulary. A market that returns the stake on a draw is
a **VOID**, which the enum already carries.

---

## Amendment — 2026-08-14 (batch 66: S51 CLOSED, S75 — the hand-laid mark)

### S51 is closed. The wax highlight was the owner after all.

**The 4px was never a mystery and never a composition question — it is a KIT-FIDELITY gap.**
`PayoutFigure.jsx` places the band `bottom:-2px` against a line box of `--st-size-payout` 31px ×
`--st-lh-fig` 1.1 = 34.1px, so **the kit's band bottom sits 36.1px below the figure's top, inside
the figure's own box.** The build places it at 40px. **The 3.9px difference is the 4.00px structural
overrun**, and the horizontal `−3 / +5` overshoot already matches the kit exactly.

**The frame shows the same thing without the arithmetic:** the band reads as a **detached rule under
the figure**, not the highlighter behind it that its own source comment describes. **One cause, two
symptoms, one fix — place the band per the kit.** The overrun then closes at zero with **no payout
block moved, no reservation slackened and no element excluded.**

**The earlier acquittal was arithmetically wrong**: it computed the band's HEIGHT term (24px ×
sin 0.5° = 0.21px). **A rotation about a top-left pivot descends by `w·sin θ` — a WIDTH term.** The
band is sized from the payout figure's measured width and `RunDirector.seed` is blank in
`Room.unity`, so **the pin was a function of how much money was on screen and could never have held
a constant** (4.563 ↔ a 56.5px figure, 4.748 ↔ a 77.7px one; the draws supplied the extra glyphs).

**Why it earned pixels now:** the overhang eats the 6px pad above the anchored action stack rather
than colliding with it — **but `4.00 + 0.0087·w > 6` at `w > 229px`, money never abbreviates (C49),
and same-game parlays are in flight.** It is a latent collision that the work in the lane is pushing
toward T47's boundary. **Never shrink the figure to fit** — standing, and still refused.

### S75 — a hand-laid mark reserves with the figure it marks

**A decorative mark that belongs to a figure — highlight, underline, rub-out, ring — is flow
content.** It is measured with its figure, **never excluded from a reservation**, and **its own
extent is what must clear the boundary, not the type's box.** Wax is money (S3): the band marks the
loudest figure on the surface by intent and keeps its highlight even at `$0`, because the highlight
marks the slot. **A mark this surface rules as meaning is not chrome.**

**Where the mark is transformed, the reserved extent is the TRANSFORMED extent** — and for a
rotation about a corner that is a **width** term.

**Bound it at design time, never at runtime.** A mark sized from measured text makes the reservation
a function of content, which §2 forbids. **Sweep the population (C46), take the widest renderable
money string, and pin the clearance as a constant** — a fixed grid constant re-derived once at design
time is legal; a zone that moves with the string is not.
