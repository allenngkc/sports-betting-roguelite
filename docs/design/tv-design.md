# TV sweat — the match theatre

> **APPROVED — Allen, 2026-08-07.** Canon under C9's two-tier authority. This is the Design
> Director's batch-19 revision, landed verbatim by the orchestrator with one bookkeeping correction:
> the new element-and-ground law was issued as "C34" and is transcribed as **C35** (C34 =
> reproducibility, batch 14; earlier ID governs).

**Owning document** under C9's two-tier authority · **Status:** RATIFIED — Allen 2026-08-07
**Canonical home:** `main-2/docs/design/tv-design.md` · **Revised:** Design Director, 2026-08-08 (batch 19)
**Amended:** Design Director, 2026-08-11 (batch 32 — TV Phase T). Touches §4 (the face split, all 23
slots, and the no-synthesised-styling law), §4.1 (order vs fit), §6.2 (the VOID matrix rule), §9 (gate
V9) and §10 (item 5, and four open rows). Those clauses transcribe **T72–T78 + C43**, authored
2026-08-11; the register transcription ran in parallel, so the tables are the authority if the two ever
read differently (C22).
**Companion:** `docs/design/constitution.md` (authority and evidence)
**Siblings:** `docs/design/room-design.md` (R13) · `docs/design/surething-design.md` (C26-am)

---

## 0. Scope and precedence

This is the binding art authority for **the TV** — the hardened display bolted to the bunker wall and
everything that renders on it. It carries the colour, type, layout, brightness and motion law that the
constitution deliberately excludes.

Precedence: Allen → the constitution → **this document** → the register's ruling for the item → the
slice's specs (`DESIGN.md`, PRD).

`design/08-art-direction.md` — casino neon on black, CRT scanlines, green/red/gold purity — is a
**deprecated anti-reference** (T3, Allen 2026-07-24). Where the PRD's §14.1 still carries that
language it is stale (C6). `DESIGN.md` §6 governs layout (C1); the PRD's §13/§14 amend to it.

Every clause below transcribes a ruled register row. Where a clause states a value, that value was
measured on a rendered frame. **Nothing here is new law.** Unratified values are quarantined in §10.

---

## 1. What this surface is

**Maintained industrial equipment** (T1, approved FINAL after seven rounds against concept render G).
A decade-old hardened display that **works perfectly** and was **installed by an institution** — not
bought by the occupant. Riveted steel, chipped paint, stencilled equipment code, conduit continuous
with the room's pipe runs.

### 1.1 The register split

| | The TV | The laptop |
|---|---|---|
| Object | installed by an institution | **his own machine** |
| Register | **Loud.** An instrument you watch. | **Calm.** A tool you operate. |
| Owns | **unrevealed drama** — score, clock, probability movement, outcome reveals | slate, markets, slip, stake, staging, lock, shop, placed tickets |
| Motion | **panel refresh** — quantised, discrete | laid ink — continuous, hand-paced |

**The TV is the only surface permitted to show score, clock, win-probability movement or an outcome.**
MY BETS mirrors what this surface has already revealed and never runs ahead of it. **The laptop
decides; the TV reveals.**

If the two screens feel the same, one of them is wrong. Do not carry this surface's vocabulary — its
coarse grid, monumental type, institutional steel, brightness-only semantics, un-eased motion — onto
the laptop.

### 1.2 Never broken

The display is old and it works. **Banned by name:** scanlines, screen curvature, phosphor haze,
static crawl, interference noise, glitch displacement, animated idle flicker, and every other
treatment that says *broken* (T8, removed and verified `842382d`; the idle-emission flicker struck at
T64). Also banned: drop shadows, bevels, glassmorphism, gradient-filled buttons, and **a stroked box
around a zone** — zones are separated by hairline rules or unlit gutters, both native to the substrate.

### 1.3 Not the enclosure

The riveted housing, the glass, the dust, the light escaping the bezel and the unified grade are
**room props and a rendering obligation**, not part of this canvas. A flat capture is a design
reference. **The in-room render at the seated camera is the only valid acceptance view.**

---

## 2. The brightness ladder — this surface's first law

**Brightness is the primary semantic channel. Hue is secondary.**

| Tier | Value | Means |
|---|---|---|
| **L4** | 1.00 | *now* — actionable, or a momentary payoff punch |
| L3 | 0.70 | live, current, true at this instant |
| L2 | 0.40 | readable context |
| L1 | 0.15 | structure, and the not-yet |
| L0 | 0 | dead — unlit pixel structure, not an outcome |

**At most one L4 element exists at any instant.** If two things want full brightness the design has
not decided what matters. This single rule is what separates this world from the render Allen
rejected.

**Eligibility is not simultaneity** (C3). §3's four L4 occupants and §7's ball are *eligible*; only
the score-at-goal and the ball-at-payoff carry the shader. The live-leg pulse stays out — **scarcity
is what makes L4 mean *now***. An explicit one-token invariant enforces it, and the arbitration is:
**a momentary punch preempts a sustained state.**

**L0 is an unlit slot, not an outcome** (T29). Loss is carried by the extinguished ground and the
strike, never by dimming type; a resolved row is *index*, and index must read. The Lost cap is set by
contrast against `--tv-extinguished`.

### 2.1 The ladder's unit

**Rec.709 luma on display-encoded values.** One unit, studio-wide, quoted with every ladder number
(C33). RGB-average and linear-space luminance both mis-rank this surface's own palette and are not the
ladder's unit — see §10.

**Where an L4 candidate is a filled field and its competitor is type, dominance is judged on zone mean
and peak together** (C33-am). Peak-versus-peak silently compares a field against a glyph, and a field
always loses that comparison while dominating the frame. **Three measurements, three spaces** (C33-am3), never compared: the **ladder** is Rec.709 luma on
**display-encoded** values; a **contrast ratio** is relative luminance in **linear** space; **emission
hue and chroma** are CIELAB from the **linear authored** value. Every number states its unit *and* its
space.

**An element and its ground must not be driven by one control** (C35). `ApplyBoost`'s `Payout` case
drives the payout text and the flood behind it together, preserving their ratio — **no brightness
change can separate a pair that moves together.** Legibility comes from a stable ground and an inverted
ink, never from boosting both sides of a contrast.

**A ranking gate is silent on legibility** (C33-am2): the ladder answers which element dominates, never
whether that element can be read. Dominance and internal contrast are two instruments, and this surface
shipped only the first until T68. **No gold can out-rank cold white on peak
luma at all** — within the range a `Color32` canvas colour is clamped to, matching cold white's 0.942
requires G ≈ 1.0, which is lemon. A gold field's L4 standing is therefore established by field area,
zone mean and boost, never by out-peaking white type.

---

## 3. Colour

Cold and quiet, with one warm bar (T2, concept C).

| Token | Value | Role |
|---|---|---|
| `--tv-substrate` | `#0A0C10` | lifted screen black — **never `#000000`** |
| `--tv-fact` | cold white | score, clock, live leg names, market lines |
| `--tv-context` | grey | labels, odds, risk/pays, pitch markings |
| `--tv-structure` | dim grey | NEXT legs, dividers, ticket header |
| `--tv-gold` | gold | **money only** |
| `--tv-team-a/b` | muted blue / muted pink | **pitch dots only** |
| `--tv-pitch` | L1–L2 green | the pitch is a *place*, not an event |

### 3.1 Gold is rationed

Gold appears on **won legs, payout figures, and the cash-out band. Nothing else on this surface is
warm, and the scarcity is the signal.**

- **No full-field washes** (T40). The LEG-WON gold wash and the dead-leg oxide wash are *deleted, not
  dimmed* — a full-field wash spends the whole ration in one frame and is a celebration. The win is
  carried where it is already carried.
- **This extends into the room** (T65, closed on frames). An event-driven room re-tint is a full-field
  wash on a larger surface. The mechanism is open (C5) and endorsed (T45). **It fires on settlement
  only, from one painting point, carrying a room-palette warm — and no call site names a colour.**
- **CASHED OUT $x prints in the cash-out slot at L3** (T35). A 96px full-screen figure is a
  celebration *and* resizes a zone to content — forbidden on both counts independently.
- The goal flash is a **brightness event on the cold-white channel** (T58, closed on frames at 0.0%
  saturation). Gold never carries drama.

### 3.2 Team hue

Muted, brightness-secondary, and **confined to the pitch dots** (T2, T42, T32.1). **Team names are
`--tv-fact`** — identity is carried by the words in the ticket column, not by tinting them. If the two
sides are inseparable at four metres **the fix is form** (filled vs hollow dot), **never louder
colour**.

### 3.3 Retired hues

**Green and red are retired game-wide** (C4). No red exception is granted anywhere (T34): DEAD is the
strike, the word and the extinguished ground — **never hue**. `chromeCyan` has no role in §3 and is
debt (T9).

Red and green live in **light** as well as in pixels, which no early scan covered. The palette scan
covers runtime colour fields, source-scanned locals and parameters, colour arrays, **rich-text markup**,
and **light colours** — one instrument, four blind spots closed (T15, T30, T34).

---

## 4. Type

**Encode Sans + Encode Sans Condensed**, SIL OFL 1.1 (T11, confirmed in situ at T50). Tabular figures
**measured, not assumed** — Saira was disqualified for lacking `tnum` despite the best character.
Deepest width axis of any qualifying free family, so one face covers ticket column and scoreline. An
engineered screen face against the laptop's Archivo text face keeps *one hand, different jobs*
explicit.

**Tabular numerals are mandatory.** Scores, clocks, money and counts all change in place; non-tabular
figures make the whole surface twitch on every tick.

**How they are obtained — the atlas carries them, because the runtime cannot ask for them** (T82,
2026-08-12, measured three ways at `cb84278`). T11 was right and is vindicated at the file level:
`tnum` is present in the GSUB of both faces, and the tabular figures are genuinely drawn. They are
nonetheless unreachable at runtime — TextMeshPro's `OTL_FeatureTag` declares only `kern`, `liga`,
`mark` and `mkmk`, and no rich-text tag or component property exposes a `tnum`. **The default digit
set both faces ship is proportional** at 0.242 em (Regular) and 0.244 em (Condensed), widest `0` and
narrowest `1` in every face at every size, which is **46–93 px of spread across ten digits** at the
sizes this surface renders numbers. The laptop's S29 escape does not exist here: Archivo Narrow's
digits are equal-advance by construction, so a face assignment satisfied that mandate without a
feature, and **both TV faces are proportional** — no face assignment on this surface can make the
mandate true.

So the mandate is delivered **at the font asset, not at the slot and not at runtime**: the atlas is
built from the font's own tabular glyphs, with the substitution resolved at generation time and
U+0030–0039 mapped to the tabular glyph indices. These are the figures the type designer drew. A
forced uniform advance (`<mspace>`) is **not** an acceptable substitute — it imposes a metric the
family does not contain, which is this section's *no synthesised styling* rule in the spacing
channel, and it hits letters in mixed strings such as `CASH OUT $183`.

Two standing consequences. **Digits alone satisfy the mandate** — digits are the characters that
change, so `$`, `:` and separators need nothing; the requirement is equal advance *among digits*, not
constant string width (a score going 9 to 10 adds a character and widens, and that is layout).
**Every generated asset that any figure slot renders on takes the tabular set, and the inventory
names its members.** The surface generates **four** TMP assets, all from the derived
`EncodeSans-Tabular.ttf` as their single source face:

| asset | instance | weight | carries |
|---|---|---|---|
| `EncodeSans SDF` | Regular | 400 | `Attract`, `LegRowState0`, `TakeoverSub`, `Flavor` |
| `EncodeSans Bold SDF` | Bold | 700 | the roman's bold arm, via `WireBold` |
| `EncodeSansCondensed SDF` | Condensed Regular | 400 | the condensed primary |
| `EncodeSansCondensed Bold SDF` | Condensed **Bold 700** | 700 | **`CashOut`, `RiskPays`, `LegRowNeed0`** |

*(Corrected 2026-08-13, T75-am5. This clause previously reasoned about "a third" asset and the
register's T75-am says the surface generates **exactly three** — it generates four. Naming a subset is
the defect the inventory rule exists to prevent, so the members are listed rather than counted.)*

**Read the resolved WEIGHT, never `TMP_Text.font` alone.** `font` names the *primary* asset, so a slot
built at `FontWeight.Bold` renders through the bold arm while `font.name` still reads the regular one —
which mis-attributed all three Condensed Bold 700 sites once already. The sweep now prints `w700`
beside the face so the misreading cannot recur.

### Tracking — marks a label, never a fact

*(Clause added 2026-08-12, T85-am2. This document had no tracking rule of any kind until now, which is
why three values reached the surface without an owner. Its inventory is **incomplete** — see the
bottom of this subsection.)*

**Tracking marks a LABEL. It never marks a FACT.** A tracked label says *this is furniture, read past
it*; tracking a fact spaces out the very thing the player is meant to read. The rule is the laptop's,
already canon at `surething-design.md` §4.3 — *short uppercase labels are tracked, factual copy stays
literal, and a label-plus-instruction is factual copy* — and it transfers because it is a rule about
voice, not about a typeface.

Ruled members:

| slot | tracking | basis |
|---|---|---|
| `Label` | **.16 em** | label-class; ratified as built on frames |
| `Meta` | **.10 em** | label-class, lighter value for a smaller and quieter slot; ratified as built |
| `NEED` | **0** | **a fact, not a label** — the requirement statement is what has to happen for the player's money to land |
| `TakeoverTitle` | **0** | carries money and leg facts |
| `TakeoverSub` | **0** | carries money and leg facts |
| `Subtitle` | **0** | carries money and leg facts |
| `Consolation` | **0** | **authored voice is COPY, not furniture** — a written line is read, not scanned past; if it must sit apart, T77's remedy is size, value or position |

**Classify a slot against its STRINGS, not its name.** A slot's name describes its position; only its
strings reveal its voice. S68 is the case that earned this line — a tracking value applied by class to
strings whose class turned out to be different from what the slot suggested.

`NEED` carried .02 em briefly and it was withdrawn twice over: procedurally, as a fourth variable
inside an open verification pair (T85), and substantively, because tracking a requirement is tracking
a fact. **It does not return when the pair closes.**

**Exceptions take their own named token and are listed here.** A named exception with one member is
still named (S70's shape).

| `InterventionPrompt` | **0** | instruction form, and it carries a figure |
| `Attract` | **0** | three of its four strings are sentences, one of them an instruction |

**Every slot not named above is 0 — by rule, not by default.** Tracking on this surface is **opt-in
and named**: silence in this table is a ruling, not a gap. §4 names 23 slot types and nine appear
here, so the other fourteen are ruled to 0 rather than left to default — which is the state T75 caught
across half the surface's type and the reason this clause is written as a total rule with named
exceptions rather than as an inventory forever chasing its members.

*(Clause CLOSED 2026-08-12, T85-am4, on the completed string-set enumeration. It no longer states a
gap because it no longer has one.)*

**No synthesised styling on this surface** (T73, T77). Weight comes from a real named instance of the
family; slant is not used at all. A synthesised bold is a smear and a synthesised italic is a shear —
neither is a letterform the family contains, and retiring them is what the TMP migration is *for*
(C15). Encode Sans carries **Condensed Bold at `wght=700 wdth=75`** in the file already committed, so
real weight costs no asset and no licence decision. The family has **no italic on any axis**, so an
italic here would mean a second family for one slot, against §4's *one hand, different jobs*.

**Every slot's face is ruled, none defaults.** The split below is the whole surface — 23 slot types.
Before T75 the canon named 10 of them and the other 13 rendered regular by *defaulting*, which is an
inventory that does not name its members (C18).

**Condensed** — the ticket column and the money control:

| slot | weight | canon role |
|---|---|---|
| `LegRowLine{i}` | **Bold 700** | compact statement |
| `LegRowPrice{i}` | Regular | price |
| `LegRowNeed{i}` | **Bold 700** | NEED |
| `LegRowProgress{i}` | Regular | progress |
| `RiskPays` | **Bold 700** | risk / pays |
| `CashOut` | **Bold 700** | cash-out figure |

**Regular — canon-named:**

| slot | weight | canon role |
|---|---|---|
| `LegRowState{i}` | Regular | state chip |
| `Flavor` | Bold | event line |
| `Score` | Bold | SCORE figures |
| `Matchup` | Bold | **splits into name / score / name spans (T72)** |

**Regular — ruled at T75**, not defaulted: `TicketHeader`, `Leg`, `Clock`, `CashOutStatus`,
`Attract`, `TakeoverTitle`, `TakeoverSub`, `Subtitle`, `BigAmount`, `Consolation`,
`InterventionPrompt`, `Chrome`, and `MomentumLabel` (which cited canon already).

Three of those carry conditions:

- **`Clock` and `BigAmount` are named by the tabular mandate above** — a clock and a money figure both
  change in place. Regular is the face the mandate wants, so the default confirms rather than
  conflicts. *(Verification clause corrected 2026-08-12, T75-am3: it read "verified tabular on the
  built face, **per slot**, on frames", and both halves of that are now known wrong. The property
  lives on the **font asset**, not the slot — T75-am — and on the shipped stack no slot and no face
  can be tabular at all, so a per-slot frame check could never have returned a pass. `Clock` stays
  Regular; the tabular property arrives with the atlas above, never with a face swap. The frame
  evidence is corroboration, and the acceptance test is the harness's clock string: within a set of
  equal-character-count strings in this right-anchored slot, the left ink edge is invariant iff the
  digits are tabular.)* A figure slot ruled by default and never checked for `tnum` is S29's defect,
  and S29 is why nobody assumes this twice — the check was right to demand, and what it found is
  T82.
- **`CashOutStatus` sits inside the money control** (§6.1, six states) beside a figure that is
  condensed and Bold 700. Its face is shown on the Phase T pair with the disposition pre-committed:
  reads as two voices inside one control → it moves to condensed; reads as label-and-figure → regular
  stands.
- **`Consolation`** loses its italic and renders regular (T77). If that line needs to sit apart from
  its neighbours, size, value or position carry it — never a letterform the family does not have.

**The scoreline is three spans, not one string** (T72). Canon puts team names on condensed and SCORE
figures on regular, and a single `Text` cannot hold both. Ruling the whole line condensed would move
the surface's largest element off its ruled face and put the most-changing figures on a figure set
nobody has measured; ruling it regular would render team names two ways depending on where they
appear, which is S60's defect. The three-span shape already exists on this surface — TV-14 used it for
the compact leg row.

### 4.1 Hierarchy

**The score is the largest element on the surface at all times. Nothing outgrows it, cash-out
included.** That sentence is the law, and it is a **ranking**.

**The ratio line and the px line are two instruments answering different questions, and neither is a
size authority** (T74).

- **Ratios encode ORDER** — score 1.00 · cash-out 0.70 · team 0.55 · clock 0.50 · need 0.50 ·
  progress 0.40 · risk 0.40 · event 0.36 · leg 0.34 · label 0.22. What binds is the ordering they
  describe: **score > cash-out > team ≈ clock ≈ need > risk > event > progress ≈ leg > label.** It is
  asserted **against the composition**, never as ten per-element size checks — C33(b), *a per-element
  value check cannot see a ranking*, on the size axis instead of the brightness axis.
- **Px encode FIT** — what a 26–28% column and a legibility floor actually produce. Shipped and
  re-derived at T20: **NEED 28px**, **live progress 23→19px**, **resolved and pending rows 19→15px**
  — *live rows are display, resolved rows are index*.

The two do not reconcile: no single base satisfies the ratios, and the implied score-size runs 36 →
68. Read literally against a 36px score the ratios would give label 7.9px and a ticket column shrunk
past legibility. **That is why neither table governs sizing on its own, and why §10.5 stays
quarantined rather than being resolved by picking a winner.**

**The reconciliation is owed and deferred, deliberately.** Re-authoring this surface's type scale is a
sizing pass with its own frames. It is not a font-stack swap and does not ride inside one (C43).

**Authored strings do not bend to stale measurements** (T24-am: every measurement predating the
production face was re-taken; the deficit dissolved at `64ccf53`). Where a weight or face change makes
a string overrun, the remedy is the size or the span — **never the copy**; §8's authored forms exist
so truncation is never reached.

---

## 5. Layout — Layout B, "Ticket Rail"

**980 × 550** reference canvas. Closed (C1, T5): `DESIGN.md` §6 governs.

- **Ticket column at the left edge, full height, 26–28% of the width** (corrected down from ~37%).
  Reading starts at the left, so the first thing the eye lands on is **the bet** — which is what the
  product is about. The match is what the bet is made of, not the subject. Density in the column, room
  on the stage.
- **Compact scorebug + stage** fill the right.
- **Cash-out anchored at the foot of the ticket column.**
- **The event strip's text zone begins at canvas x 305**, 40px past the ticket-column boundary (T67).
  The lit gold field blooms 40px into the strip's empty left margin and zero beyond x=365; starting the
  text zone past that reach means **any authored line, at any length, begins outside the halo.** The
  strip's copy does not warm at the acceptance view and no bloom value is touched.
- **Momentum tape**: one 28px strip at the scorebug foot, a MOMENTUM label, one shared centre line —
  no numerals, no hue, never above L2 (T16, T28, T52). Match-scoped: a row per leg would be a second
  ticket column re-introducing team hue by construction. The **win-probability numeral is out** (§7
  bans duplicating it; locked odds make the read the player's job).

### 5.1 Fixed grid

Every zone position comes from **an explicit fixed grid defined once in code**, never computed from
content. **No zone resizes in response to content** — reserved space stays reserved and goes dark
(T21, T35, T46). The ticket column owns its width absolutely; the stage clips to its own region and
asserts its edges per frame.

**Fixed rows** (T24): every leg slot is authored at the live row's measured height, reserved always.
NEED is **one line** — never wrapped, never truncated; an over-long NEED is re-authored against a
call-site-recorded measurement. A live row carries **no meta line** — specified, not tolerated.

Re-deriving a grid constant **once at design time** is legal; a runtime resize is not. **Never reorder
an information hierarchy for three tenths of a pixel** (T51) — the stacked label-above-value stays.

**A locked band is not headroom** (R30).

**A fixed box carries an unstated assumption about the face it was sized against** (C46, T84). Because
this grid fixes widths rather than deriving them from content, **every reserved box silently claims
that the longest string it can hold fits, in the face it was sized against** — and a change of face,
weight, tracking or figure set invalidates every one of those claims at once, because nothing in the
box refers to the face it assumed.

Two consequences, both paid for in Phase T:

- **After any change to type metrics, sweep the POPULATION, not the suspects.** The set at risk is
  *every fixed box*. Phase T's three defects — a truncation, a collision and a solo clipping — issued
  from one assumption in three unrelated boxes, and were found by eye in two of nine moments before
  the structural reading found the class.
- **Test the longest RENDERABLE form, not the current content.** A box passes on what is in it today
  and fails on the string nobody captured. **Every tested string is traceable to a call site, in both
  directions** — a generator that can invent an unrenderable string can equally miss a real one, and
  only the second failure is invisible in the frames.

---

## 6. Components and states

### 6.1 The cash-out slot

**One fixed rectangle owning all six states.** It never reflows.

| State | Treatment |
|---|---|
| actionable | gold at **L4**, inverted field, **dark type punched out — field AND type invert** (T68) |
| updating | gold at **L3** — never L4: brightness must not promise what input refuses |
| suspended | **L1 unlit slate from its first frame** (T43), `MARKET SUSPENDED`, no amount |
| pending | as suspended; intervention controls live in their own overlay, never in this row |
| unavailable | L1, quiet, no reflow |
| accepted | brief L4 punch, then `CASHED OUT $x` **in the slot** at L3, inverted — the gold flood is a celebration ground, **never the field a money figure is read against** (T68-am) |

**The brightness of this slot is a promise about input.** L4 means the key works *right now*. If the
slot is bright and the press does nothing, the surface has lied.

**`accepted` renders in the slot, not over the flood** (T68-am). T43 retires the *offer* — the amount,
the instruction, the actionable field — not the rectangle: the slot is the surface's furniture and
`accepted` is one of its six states. Over the flood the ground is a sine pulse (alpha 0 → 0.55 → 0),
so gold reads **12.47:1** at the ends and **1.71:1** at the peak while `goldInk` reads 1.08:1 and
7.87:1 — **complementary, and neither static ink is correct.** An L3 gold field gives **9.68:1**.
**`WinBeat`'s `+$X` tally takes the same treatment** (T71); two payoff moments must not diverge.

**The inversion is a two-part operation** (T68). On `actionable` and `accepted` the field takes gold
**and the label and amount take `goldInk`**. Inverting the field alone raises it to meet a light label:
measured at the acceptance view, `HOLD E` on the lit field read **1.19:1** where the punched-out build
reads **7.95:1** (linear). The unlit states keep their light ink and are correct as they stand.

**The label is the thinner margin of the two and fails first.** Rendered ink lands near 0.222 against an
authored `goldInk` of 0.046 — bloom, antialiasing and T48's black lift all raise it — and a small thin
label's strokes fill in further than a large bold amount's. **If the field is ever brightened, check the
label before the amount.**

**Display state and input state are the same state, read from one value** (T59). `suspended`,
`pending` and `updating` refuse `E`; `actionable` accepts. A refused press draws nothing — the slot is
already dark and labelled. Accepting a declared-refused input on a money control is the worst
available outcome: the player gets a price the display is not showing.

**`MARKET SUSPENDED` owns the slot exclusively** (TV-12/13) — no actionable offer beside it.

**A lit field blooms into its neighbours** (T67). Risk/pays taking gold is not a ration event; **the
event strip taking gold is**, because T27 keeps the bar neutral. This is judged at the seated in-room
render and nowhere else (§1.3) — bloom through real glass at four metres is what that view exists for.
If the strip warms at that distance the remedy is **separation**: a gutter between field and strip.
Never a bloom change (sealed) and never dimming the band.

**Confirm gesture** (T22, T36): **hold to preview; release always abandons; release is never confirm**
— commit is an act on the laptop. The bounded fallback is a second key during the hold. **No timer, no
auto-commit.** Copy is `CASH OUT $183` and **`HOLD E`**; `[E]` is retired.

**The gesture is not local to this slot — it governs every irreversible spend on this surface**
(T88, C48). The theatre *may* carry a commit: T22's fallback is exactly that shape, and a frozen-moment
mechanic earns it, because a frozen shot that requires walking to the laptop is not a frozen shot.
What the theatre may **not** do is take money on one frame of input.

- **Every spending option takes the fallback gesture, unchanged.** Hold previews — and **the preview
  shows what the option does AND what it costs** (T86-am: the basis for a decision is an offer, not an
  opinion). Release abandons, always. A second key during the hold commits. **A press does not
  commit.**
- **A declining option is not a spend and does not take the gesture.** Where the option costs nothing
  and is already what happens if the player does nothing, a single press is proportionate.
  **The weight of the gesture matches the weight of the act.**

**A control's copy IS its input contract** (C48). Where a label names a gesture, that gesture is what
the input implements — **the label is not a description of the control, it is the contract the player
acts on.** A label naming a *safety property* the input does not provide is the most dangerous copy a
control can carry, because it is relied upon precisely when the player is careless, and the reliance
is invisible until it costs him.

**Founding case, recorded because the repair direction is the non-obvious half:** the intervention
prompt printed `HOLD` over a `wasPressedThisFrame` input that committed an irreversible spend on the
first frame — no preview, no abandon path, on a surface the player is *watching* rather than
operating. **Where copy and input disagree on a money control, the INPUT is corrected to match the
COPY — never the copy to match the input.** Relabelling ships an honest description of an unsafe
control, which is worse than either half alone. **If the gesture cannot land, the control does not
ship.**

### 6.2 Leg rows

One row per leg in **ticket order**, brightness carrying the state. Rows are pushed, never reordered.
Multiple legs may be live at once — **L3 is a tier, not a slot** — and concurrent live legs pulse **in
phase off one shared clock**. `LIVE` is the only pulse on the surface.

**Progress lines are driven from the revealed payload and land on the same frame as it** (T62).

**A leg statement names each fact once** (T69). Concatenating the pick and the fixture restates the
backed team — `ATLANTA MIDDLEMEN ML · v TULSA STARTUPS`, never `Atlanta Middlemen ML — Atlanta
Middlemen v Tulsa Startups`. Statements are **re-authored against a call-site-recorded measurement**
and never wrap: a row that wraps to three lines is a string exceeding a fixed slot, which §5.1 forbids.

**The VOID mark is a drawn matrix rule, not a text strikethrough** (T76). Canon asks for a row *struck
through on the matrix*, and those are different objects: a strikethrough is a property of a string, a
matrix rule is a mark the board makes across a cell — the institution striking a line through a row,
which is this surface's whole register. The laptop rules its analogue the same way (the oxide strike is
the house's mark, drawn — S3, S15-am). TMP makes a native strikethrough *available*; that does not make
it correct. It is content-derived geometry by construction, which §6 forbids, and a fixed-width rule is
also the only one whose length does not move when the face changes. The rule's width is set against the
column: it does not move under a face migration, and if a later sizing pass changes the column it
re-derives with it once, at design time.

### 6.3 Risk and pays

**One ticket-level `RISK` and one `PAYS`**, gold at L2 (PRD §8.4). Never per leg — that is not how a
parlay works, and the approved concept render got it wrong. Risk/pays is in the bloom-protected set
(C8), and that floor is **measured on rendered frames at seated distance, never asserted from a boost
value**.

### 6.4 The stage

Fixed top-down pitch, picked team attacks right, **camera never moves** — no shake, no cut, no zoom
(T14). Scene variation is read from movement.

**Pitch markings sit at L1–L2** because the pitch is a place, not an event; drawing them bright turns
the stage into a test pattern. Actors are single lit cells in muted team hue at L3. **The ball is the
only object permitted L4, and only at a payoff.** The stage is capped under the ladder (T41, closed on
frames).

Actors and the ball move **continuously**; everything else on the surface changes **discretely**. That
separation is what keeps the match legible against a static information surface.

### 6.5 Backed-player locator

A **detached 2px `--tv-fact` ring at the dot**, hue unchanged, L3, held while the scorer leg is live
and removed on the resolving frame (T23). **Never the L4 token, no pulse.** The 10px numeral is
deleted — sub-floor by construction, and this surface has no glyph vocabulary.

### 6.6 Stats panel

Opens from the head of the ticket column and **freezes playback**. It expands over the column and
stage **without moving either**, so everything is where it was when it closes. Ships at **authored
height with no reserved space** (T21).

**Revealed-ledger values only.** Formations (no engine concept) and player stats (generator truth — a
blocker-class leak) are dropped; per-team corners and cards ship from `CountLedger` (T36). **A row
returns only when the sim emits it as a first-class value, never computed in presentation.**

### 6.7 Ticket interstitial

Appears only once the stage and active-leg card have **cleared**. No score, clock, tape, event line,
suspended label or prior offer survives into it. Round settlement may reuse the treatment but must
never resemble an active leg or a live cash-out offer.

### 6.8 The drawn match

*(Added 2026-08-12, T87 / C47, on the draws work — the moneyline is now three-way.)*

**The final beat is the whistle, not a verdict — and not nothing either.** A drawn match has no goal
to end on, and both obvious treatments are already banned:

- **Manufacturing a climax** is celebration (T35, T40) — a flourish spends the ration on a moment that
  did not earn it.
- **Rendering nothing** makes a resolution draw as an *absence*, which reads as a bug and teaches a
  false rule.

**The beat is the match ending level, STATED.** The scoreline holds at its level value; the event
strip states the fact at its own L2 tier (T66); the legs resolve to their words. **The theatre
reports. It does not editorialise about a quiet ending.**

**This reaches 0–0, and no separate beat is authored for it (T87-am).** Every mechanism above is
goal-independent: the scoreline holds a value it already holds, the strip states a fact that needs no
goal, the legs resolve, and **T65's settlement glow fires on settlement, not on a goal.** **Nothing
here may be narrowed to exclude a goalless match** — that is the case a narrowing would quietly drop.

**What 0–0 changes is the risk, not the rule.** The *rendering nothing* failure mode above is at its
maximum, and only there: in a 1–1 the surface has punched and the room has moved, so the ending arrives
against a match that visibly happened; **in a 0–0 nothing has punched all match.** The standing check
is therefore **is the stated ending legible as a resolution** — not *is it loud*, which is banned —
**because the one state this surface must never be mistaken for is idle.**

**A draw is quiet for the room and LOUD for one ticket.** A draw-backer has won, and his leg lands
like any other winning leg. **The absence of a goal is not the absence of his result** — and the worst
outcome available here is a surface that conflates *no goal* with *no result* and drains the one
player whose ticket just came in. The settlement machinery already handles this: the room's settlement
glow fires **on settlement, not on a goal** (T65), so a drawn match is already a first-class
settlement moment. **Nothing existing may be narrowed to exclude it.**

**Result language needs no third word** (C47). **The match has three outcomes; a bet has two.** Back
the draw and it draws — **you won**. Back a team and it draws — **you lost**. The state enum is
unchanged and needs nothing added; **inventing a `DREW` leg state would model the match inside the
bet's vocabulary**, the same category error the engine already refuses by keeping `Side` two-valued.
A market that *returns the stake* on a draw is a **VOID**, which the enum already carries — a
different market's rule, never a third result.

**VERIFIED ON FRAMES, 2026-08-14 (T87-am, batch 66)** — `dd-import/tv-goalless-draw-2026-08-14/`,
a 0–0 to full time with a draw-backing and a team-backing ticket on one settlement. **The beat
holds and this section needs no amendment.** The ending is legible as a resolution in both halves
(the standing check above): the draw-backer's leg lands at full treatment — one `WinBeat()` path,
no goal conditioning anywhere in it — and **T65's settlement glow fires on the goalless
settlement**, measured at +7.64 mean lift across 76.7% of the room. **Neither half reads as idle.**

**Two defects were found in mechanisms this section relies on, and they are repairs, not
amendments:**

- **The event strip never states the fact.** Across all 120 frames it carries the LEG's grade and
  never the MATCH's ending. The ending survives on the settlement machinery and the scorebug's
  `FT` alone. **The L2 statement this section assigns the strip is OWED** (T87-am) — never a
  flourish.
- **The strip carried a goal line INTO the goalless full time** (T97) — `ScoreDown` copy over a
  `0 — 0 · FT` scorebug, in the same frame, for 31 of 60 frames. C50's shape.
- **The draw leg printed as `MIDDLEMEN ML`** (T96) — the exact string §8 names as what a draw leg
  must not inherit. **An unimplemented ruling, not a new one.**

**AUTHORED 2026-08-14 (batch 68), discharging the owed statement above — the drawn match's ending
line is `THE MATCH ENDS LEVEL`.** It fires at the whistle of a **drawn** match into the event strip
at L2 and holds until the leg's grade displaces it; **the existing window is not shortened to make
room for it.**

**`FULL TIME — LEVEL` was the obvious form and is REFUSED**: the scorebug prints `FT` one slot above,
and **stating the same fact twice one slot apart is §8's duplication rule with a different
neighbour.** The line takes the shape of this surface's own beat statements (`THE BOARD IS SET`,
`THE TOTEM BURNS`) and their casing — **every authored line in the strip is caps.**

**It is the DRAWN match's line, not the goalless one.** True at 0–0 and at 2–2 alike; **a
goalless-only line would be exactly the narrowing this section forbids**, making 0–0 a special case
again. **Only a draw needs one, and the reason is structural: a decided match ends ON a goal, so its
final beat's line IS its ending — a drawn match ends on nothing, so the last beat's line is stale by
construction.** That is why the strip's silence at a draw is a gap rather than an accident.

**And the strip's words are licensed by the RESOLVED SCENE, never by the beat's type alone** (T97) —
`NeutralLine` already stands in where a count-market beat's scene carries no count event, and **the
goal families were simply never given the same override.** Sol's F_0.4.0 P3 r2 finding, one noun
changed.

---

## 7. Motion

**Panel refresh.** State changes are **quantised** — a brightness level swaps in a discrete step, not
a 300ms colour drift.

- **The score changes on one frame, one discrete step, no intermediate state** (T38). The superseded
  crossfade superimposed outgoing and incoming digits under a yellow strike: illegible at the exact
  moment it matters, and a fourth ink.
- Score, count and event **land on the same frame as their causal callback**. A change that arrives
  early is a lie.
- Dim lands on **the same frame as** the label change (T43).
- Durations: punch 120–180ms · event entrance 120–180ms · progress 180–240ms · ticket crossfade
  300–450ms · action-state swap 120–180ms.
- **One pulse kind on the whole surface** (`LIVE`), one shared clock.

Standing freezes everything, including bloom decay and pulse phase.

---

## 8. Voice

The event strip is **one authored line explaining the latest move** — explanation, not commentary
theatre. It never duplicates the score, never carries two unrelated clauses, never uses money hues,
and never covers the pitch. Copy **truncates or chooses a shorter authored line; it never shrinks**.
NEXT statements truncate on a **word boundary within their measured column** and never overprint the
stage (TV-12/13).

**Register** (T39, T44): second person only in genuine instructions. **No hype, no exclamations, no
superlatives, no promises.** One casing, one dash (em). *"off the bar - a miracle brewing?!"* is the
banned shape, verbatim.

**Statements are authored to fit their measured column at the source** (T69). The authored forms live
in the copy deck (`tv-g1-authored-leg-statements-2026-08-08.md`, G1): **NEED states the requirement,
the compact statement states the identity**, clubs are named by their distinctive word and players by
surname, and every form that can overflow has its shorter line authored rather than truncated.

**The draw's forms are authored and live with the rest** (S74, 2026-08-12): **NEED** `LEVEL AT FULL
TIME`, **progress** `LEVEL` / `NOT LEVEL`, **compact** `DRAW`. Nothing was invented — `LEVEL` is
already this surface's word for a tied scoreline (T62). The moneyline's other forms assume a team
(`MIDDLEMEN ML`) and a draw leg has none, which is why it needed its own pair rather than inheriting.
**`1X2` is industry jargon and never reaches the player** — the surface composes and the role prints
as a word.

**They live there as of 2026-08-14 and did not before** (T96, batch 68). S74 authored them on
2026-08-12; **the G1 deck was written 2026-08-08 and was never amended**, so it carried a single
two-way `| Moneyline | {CLUB} TO WIN | {CLUB} ML |` row and **zero occurrences of the word "draw"** —
which is precisely what the build built. **The sentence above was false against the very file it
points builders at**, and the deck is amended now. **A copy ruling lands in the deck or it has not
landed** — folding it into this document was not enough, and that is the reusable half of T96.

**NEED and the progress line beneath it are one authored pair** (T70): requirement above, state below,
**no term repeated across the two.** `LANYARD TO SCORE` over `WAITING FOR LANYARD` named the player
twice — T69's defect turned vertical. **That rule governs the SUBJECT, not the predicate** (T70-am,
2026-08-14): both its example and T69's are **a name printed twice**, and **a binary state answering
its own requirement in the requirement's own word is the progress line doing its only job**, not
redundant identification. **S74's `LEVEL AT FULL TIME` over `LEVEL` therefore stands** — forcing a
different word below would put a second name on one thing and break the one-name-per-thing convention
T62 established, so the cure would be the worse defect. Truncation on a word
boundary is the structural backstop against broken glyphs — **it is not the remedy**, and shipped copy
should never reach it: a clean cut still ends on a dangling word. §5.1's *re-authored, never truncated*
governs, and truncation only guarantees the failure is not ugly.

**The TV never instructs the player to bet** (T27). Idle prints **`ROUND n OF 8 · BOARD OPEN`** in
`--tv-fact`, and the bar carries no hue. *"PLACE YOUR BETS"* was celebratory exhortation in a retired
hue at L4 — banned on all three counts.

**The bracketed-key form is retired surface-wide** (T22, T86). `[E]` went and the slot prints
`HOLD E`; the same applies to **every** key-bound affordance — `[M] MULLIGAN` and its siblings take the
same form. The reasoning was never local to the cash-out gesture: **a bracketed key is game-UI
convention, and this surface is maintained industrial equipment** (§1).

**The theatre prints facts and offers. It does not print opinions** (T16, T23, T32, T86-am). **A price
is an offer** — the house stands behind it and the player transacts against it. **A probability is the
house's opinion**, with nothing attached: he can take or leave a price, but he can only agree or
disagree with an opinion, and this surface does not ask him to. That is why the win-probability
numeral, the backed-player numeral and the 10px numeral all went, and it is one rule rather than three
deletions that happened to rhyme. **Where a decision needs a basis, print the COST** — `SEND TO
REVIEW — $40` is a decision; `SEND TO REVIEW (99%)` is an instruction wearing a number.

**No engine term ever reaches a player-facing slot** (T31, R38, T87). The engine reasons with
partitions the player has no concept of — *decisive* means *not a draw* inside the engine and means
nothing to a man watching a scoreline. Beat selection may read such a flag internally; **what is
refused is the word reaching a slot.** Fourth instance of a rig string in a player slot, and the
cheapest class of defect to prevent.

The strip stays **neutral even when the event helps or hurts**; money semantics live on the leg rows
and the cash-out slot.

Fictional leagues, teams and players only.

---

## 9. Gates

Real gates, per C9. Each states its instrument and, per C18 §4.2, **what it cannot see**.

| # | Gate | Instrument | Blind to |
|---|---|---|---|
| V1 | **One L4 token at a time** | one-token invariant + per-frame ladder scan in Rec.709 luma, **zone mean AND peak** (C33-am) | **internal contrast — see V8** |
| V8 | **Every element that inverts reports field-vs-own-ink contrast** (C33-am2) **and whether its ground is static across the beat** (C35) | contrast ratio **in linear relative luminance** (C33-am3), per state, at the acceptance view | elements that never invert |
| V2 | Gold appears only on won legs, payout figures, cash-out | palette scan incl. markup, light colours, colour arrays | gold reaching the player as **room light** (V6) |
| V3 | No retired hue anywhere — verbatim constant match | `LooksLikeRetiredRed`-class scan over four surfaces | near-misses; the guard missed `#FF4038` by 0.00098 |
| V4 | No zone resizes to content; stage clips to its region | per-frame edge assertion | z-order and overdraw between correctly-sized zones |
| V5 | Display state == input state on the cash-out slot | one-value read + T43 same-frame test | whether the rendered field agrees with the flag |
| V6 | **Room re-tint stays inside the room's palette** — **checked against the value's OWN FIXTURE FAMILY, never against one room-wide band** (V6-am3) | room-region hue in **CIELAB via the room's shared `linear_to_lab`** — the converter is shared and **never forked** (C20) | **a value belonging to NO fixture family** — it is not gate-able by a family bound and is ruled at the DD seat against the property |
| V7 | Variation reads as variation at review distance | rendered frames, five seeds, named manifest | anything asserted from signature diversity (T19) |
| V9 | **The §4.1 size ORDER holds on the built surface** (T74) | rendered size per slot, compared as a **ranking** against the composition — not ten per-element checks (C33(b)) | **whether any individual size is right.** The order holding says nothing about fit, legibility, or whether a slot's px is the value it should have — that is the deferred sizing pass, not this gate |

Every invocation reports its **executed case count** and exits non-zero on zero cases (C29). Every
measurement is reported **with its scope and its resolution** attached (C25, C32).

**Any gate carrying a numeric bound runs its own founding values through itself once, and records the
result beside the bound** (C44). It costs one run, needs no frames, and it is the only check in this
family that catches a **disjoint** band rather than a merely loose one — a shifted band still passes
its founding values; a disjoint one cannot. **V6 is this clause's founding case and the first place it
was not applied:** the band it policed carried an upper anchor that its own key tube missed by more
than the band was wide, and four batches were spent before a measurement said so.

**A bound is defended by the reasoning that groups its members, never by how tightly they happen to
cluster** (V6-am3). Clustering is evidence a grouping *might* exist; it is never the grouping's
justification. Every wrong turn on the room band — a phantom top, a lights-versus-screens carve, three
cross-population comparisons — came from **treating proximity as membership**.

---

## 10. Provisional, unratified, and open

**Quarantined — do not treat as ratified:**

1. **Every brightness value in §10's px table and the shipped hexes are provisional** (T12). §3 names
   roles; `DESIGN.md` §4 gives no hexes, and the table that does was explicitly superseded. They
   settle **against the real panel at the real seated camera distance**.
2. **Shipped gold `#ffd12e` vs token `#F2BC45`** — the token is the intent; the hex stays open pending
   palette ratification (T41).
3. **`goldInk #0A0C10` sits below `DESIGN.md`'s own black floor** — a canon self-contradiction, both
   sides of it this document's.
4. **C3's "boost stays 1.8" is superseded by T49-cl's ruling of 1.4, sealed.** Recorded here as a
   reconciliation, not a new ruling: T49-cl is later, explicit and sealed, and the current capture set
   is `boost1.4`. C3's other clauses stand.
5. **The two DESIGN.md/VISUAL-DESIGN.md type tables disagree** — relative ratios versus reference px
   cannot both hold. The px table is one provisional instantiation, and it predates the column
   narrowing to 26–28%. **T74 resolves how to read them without resolving the sizes**: the ratios
   govern order, the px govern fit, neither is a size authority, and §4.1 now carries that split. The
   quarantine stands — the px table is still not ratified, and promoting it is the deferred sizing
   pass's business, not this document's.

**Open items:**

| Item | State |
|---|---|
| **T70** AnytimeScorer progress line repeats NEED's surname | Ruled batch 18; **built with the G1 deck** (`41d5cbe`) |
| **T68-am / T71** `accepted` and `WinBeat` into the slot | Ruled batch 19; **building — one commit, not two** |
| **SureThing blur on the laptop** | R22 walk finding; hunt live, C13's first-capture defect |
| **T65** settlement re-tint value (hue 88.0°, intensity 0.9) | **Open — and NOT closable by an in-room capture** (batch 66). The settlement capture arrived (`tv-goalless-draw-2026-08-14`) and the glow is confirmed firing on a goalless settlement, but the room's measured hue **tracks distance from the panel** (19.1° near-TV → 31.5° at the far corner, one frame): `EmissionFlash(goldL4)` fires on the same frame in `WinBeat()` and its spill is the confound. **Closed by V6's printed hue / the R23 `RoomViewCapture` path, which reads `roomSettlementWarm` directly — never by another room capture** |
| **T96** the draw leg prints as a team bet | **Ruled batch 66 — a repair, not a design call.** `LegStatement()`'s Moneyline branch is a two-way `pickedHome ? Home : Away`, so a `MarketChoice.Draw` leg renders `MIDDLEMEN ML`. §8 already authored the compact form as `DRAW` and names the failing string verbatim. On frames both tickets carried the same label with opposite grades |
| **T97** the strip carried a goal into a goalless full time | **Ruled batch 66 — a defect.** `EventText.ScoreDown` copy over a `0 — 0 · FT` scorebug, same frame, 31 of 60. **Land it with T87-am's owed L2 full-time statement — one slot, one repair, or the strip gets touched twice** |
| **T9** `chromeCyan` retired-hue debt | Phase 3 |
| **T10** two hardcoded emission rest values, one below the black floor | Phase 3 |
| **T25.2–25.7** seated-sweat findings | TV's queue |
| **C2** light spill colour into the room — shipped green tolerated, target cold white-grey | Interim, Allen |
| **C6** PRD §14.1 carries deprecated `08` colour law | Documentation conflict |
| **C15** TMP migration | **Phase L merged to main (`5903750`)**. **Phase T ruled at batch 32** (T72–T78): it is a **face** migration and **preserves rendered size** at every product-fact slot — the bar Phase L was granted on. Tokenising a raw integer at its identical value is a no-op on the frame and is in scope; changing one is not (8 of 23 slots carry raw ints today) |
| **T78** which instance the surface actually renders | **Open on the before-set.** The file's default instance is Condensed Thin (`wght=100 wdth=75`) — measured, not disputed. The inference that the surface therefore *renders* it is **refused**: existing frames at `7ab60b8` show the non-bold slots at a solid regular-class weight, and weight 100 at 19–28px is a hairline. The **width axis is not settled** and this seat offers no read on it. Both dispositions pre-committed in batch 32 before the frames land |
| **The sizing pass** | **Owed, deferred by ruling** (T74/C43). The ratio and px tables reconcile in a pass with its own frames, after T78 names the face — never inside the migration, because a pair spanning two variables cannot attribute a difference to either |
| **`FONTS.md` + `tools/ttf_faces.py`** | Both state Unity's default-instance render behaviour as fact; it is an inference and is contradicted by frames. Label it as an inference in both (C40). The tool's measured output is untouched and stays |

**Closed and verified end to end:** T41 (stage capped), T48 (grade black point), T49 (bloom 1.4,
sealed), T58 (goal flash neutral), T6 (scene grammar), T50 (face in situ), **T63** (band's HDR material
structural — the field was unboosted, not third-brightest), **T64** (idle flicker deleted), **T65**
(room quiet on a leg win — eight frames indistinguishable from rest), **T66** (event strip at L2, one
painting point), **T67** (bloom does not warm the strip's copy at the acceptance view; a 40px zone inset
covers the long-line case), **T68** (both halves of the inversion — 1.19:1 → 7.95:1 linear), **T69**
(statement names each fact once, no wrap, no mid-word cut). **The ladder is verified from substrate to
L4 — and T68 is why a verified ladder never meant a readable element.**
