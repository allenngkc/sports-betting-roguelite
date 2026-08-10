# TV sweat — the match theatre

> **APPROVED — Allen, 2026-08-07.** Canon under C9's two-tier authority. This is the Design
> Director's batch-19 revision, landed verbatim by the orchestrator with one bookkeeping correction:
> the new element-and-ground law was issued as "C34" and is transcribed as **C35** (C34 =
> reproducibility, batch 14; earlier ID governs).

**Owning document** under C9's two-tier authority · **Status:** RATIFIED — Allen 2026-08-07
**Canonical home:** `main-2/docs/design/tv-design.md` · **Revised:** Design Director, 2026-08-08 (batch 19)
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

### 4.1 Hierarchy

**The score is the largest element on the surface at all times. Nothing outgrows it, cash-out
included.** Ratios are the law (score 1.00 · cash-out 0.70 · team 0.55 · clock 0.50 · need 0.50 ·
progress 0.40 · risk 0.40 · event 0.36 · leg 0.34 · label 0.22).

Column px, re-derived for the corrected 26–28% column (T20): **NEED 28px** unchanged, **live progress
23→19px**, **resolved and pending rows 19→15px** — *live rows are display, resolved rows are index*.
**Authored strings do not bend to stale measurements** (T24-am: every measurement predating the
production face was re-taken; the deficit dissolved at `64ccf53`).

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

### 6.2 Leg rows

One row per leg in **ticket order**, brightness carrying the state. Rows are pushed, never reordered.
Multiple legs may be live at once — **L3 is a tier, not a slot** — and concurrent live legs pulse **in
phase off one shared clock**. `LIVE` is the only pulse on the surface.

**Progress lines are driven from the revealed payload and land on the same frame as it** (T62).

**A leg statement names each fact once** (T69). Concatenating the pick and the fixture restates the
backed team — `ATLANTA MIDDLEMEN ML · v TULSA STARTUPS`, never `Atlanta Middlemen ML — Atlanta
Middlemen v Tulsa Startups`. Statements are **re-authored against a call-site-recorded measurement**
and never wrap: a row that wraps to three lines is a string exceeding a fixed slot, which §5.1 forbids.

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

**NEED and the progress line beneath it are one authored pair** (T70): requirement above, state below,
**no term repeated across the two.** `LANYARD TO SCORE` over `WAITING FOR LANYARD` named the player
twice — T69's defect turned vertical. Truncation on a word
boundary is the structural backstop against broken glyphs — **it is not the remedy**, and shipped copy
should never reach it: a clean cut still ends on a dangling word. §5.1's *re-authored, never truncated*
governs, and truncation only guarantees the failure is not ugly.

**The TV never instructs the player to bet** (T27). Idle prints **`ROUND n OF 8 · BOARD OPEN`** in
`--tv-fact`, and the bar carries no hue. *"PLACE YOUR BETS"* was celebratory exhortation in a retired
hue at L4 — banned on all three counts.

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
| V6 | **Room re-tint stays inside the room's palette** | room-region hue/sat/luma across an event burst | the panel's own content |
| V7 | Variation reads as variation at review distance | rendered frames, five seeds, named manifest | anything asserted from signature diversity (T19) |

Every invocation reports its **executed case count** and exits non-zero on zero cases (C29). Every
measurement is reported **with its scope and its resolution** attached (C25, C32).

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
   cannot both hold. Ratios are the law; the px table is one provisional instantiation, and it
   predates the column narrowing to 26–28%.

**Open items:**

| Item | State |
|---|---|
| **T70** AnytimeScorer progress line repeats NEED's surname | Ruled batch 18; **built with the G1 deck** (`41d5cbe`) |
| **T68-am / T71** `accepted` and `WinBeat` into the slot | Ruled batch 19; **building — one commit, not two** |
| **SureThing blur on the laptop** | R22 walk finding; hunt live, C13's first-capture defect |
| **T65** settlement re-tint value (hue 88.0°, intensity 0.9) | Upper bound — owed a settlement capture |
| **T9** `chromeCyan` retired-hue debt | Phase 3 |
| **T10** two hardcoded emission rest values, one below the black floor | Phase 3 |
| **T25.2–25.7** seated-sweat findings | TV's queue |
| **C2** light spill colour into the room — shipped green tolerated, target cold white-grey | Interim, Allen |
| **C6** PRD §14.1 carries deprecated `08` colour law | Documentation conflict |
| **C15** TMP migration | **Phase L merged to main (`5903750`)**; Phase T scheduled, orchestrator-side |

**Closed and verified end to end:** T41 (stage capped), T48 (grade black point), T49 (bloom 1.4,
sealed), T58 (goal flash neutral), T6 (scene grammar), T50 (face in situ), **T63** (band's HDR material
structural — the field was unboosted, not third-brightest), **T64** (idle flicker deleted), **T65**
(room quiet on a leg win — eight frames indistinguishable from rest), **T66** (event strip at L2, one
painting point), **T67** (bloom does not warm the strip's copy at the acceptance view; a 40px zone inset
covers the long-line case), **T68** (both halves of the inversion — 1.19:1 → 7.95:1 linear), **T69**
(statement names each fact once, no wrap, no mid-word cut). **The ladder is verified from substrate to
L4 — and T68 is why a verified ladder never meant a readable element.**
