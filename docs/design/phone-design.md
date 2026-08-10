# Phone — the bookie's channel

**Owning document** under C9's two-tier authority · **Status:** DRAFT for Allen · **Drafted:** Design
Director, 2026-08-09
**Canonical home on approval:** `main-2/docs/design/phone-design.md`
**Companion:** `docs/design/constitution.md` (authority and evidence) · **Siblings:**
`room-design.md` (R13), `surething-design.md`, `tv-design.md`

---

## 0. Scope and precedence

This is the binding art authority for **the phone** — the object on the desk and the one screen it
shows. It carries the colour, type, composition and voice law that the constitution deliberately
excludes.

Precedence: Allen → the constitution → **this document** → the register's ruling for the item → the
slice's specs.

**This surface was a deliberate stub until today.** C9 made it one and C26 closed on that basis; the
stub was honest exactly while nobody was asked to make the surface good. C26-am3 expired it when Allen
put the phone in scope. Every clause below transcribes a ruled row — P1–P8 (batch 23), R19(a), R28,
R28-am, R39, R39-am, S31-am, S44, T70-am. **Nothing here is new law.**

**Evidence base, stated plainly (C25).** This document stands on **one** reference set:
`phone-reference-set-2026-08-09`, seed `PHONEREF01` pinned and asserted, three message states, two
views, controls bit-identical. The laptop's document had a fortnight of sets behind it; this one has an
afternoon. Where a clause rests on a single frame it says so, and §10 lists what the set could not see.

---

## 1. What this surface is

**His phone, on his desk, in the laptop's register** (R19(a), R28) — not the TV housing's, not the
house's equipment. A cheap personal device that happens to be the channel his bookie uses.

It shows exactly one thing: **the bookie's messages.** No apps, no other threads, no contacts. The
player never replies.

### 1.1 The constraint most likely to be got wrong: the emptiness is the design

At one message the screen is mostly empty, and **that ships.** He has one contact and it is the man he
owes. A phone that is nearly empty is characterisation, not an unfinished screen, and the first
instinct of anyone opening this file will be to fill it.

**Every way of filling it is already forbidden.** R28-am permits live engine data only — no authored
content — so a compose field, avatars, read receipts, a nav bar, a battery row or invented timestamps
are all out by law before taste enters. What remains is arranging what exists, which is §2.

### 1.2 Ownership boundary

- **Room owns the object** — geometry, material, placement, and its named interaction `MeshCollider`
  (R16). Not this document's.
- **This document owns the treatment** of what the screen shows: composition, colour, type, voice.
- **Nobody owns the content.** It is engine-emitted and stays that way (R28-am). A message this
  document does not like is a ruling against the engine's copy, made in the register, never a string
  edited into the surface.

---

## 2. Composition

### 2.1 The stack top-anchors (P3)

**Content starts at the panel's top content origin. The remainder falls to the foot and reads as room
to grow.**

The build bottom-anchored, borrowing a chat thread's grammar — newest nearest the thumb, above where a
compose field sits. **There is no compose field and there never will be**, because the player cannot
reply. A one-directional feed of notices takes an inbox's grammar, not a conversation's, and
bottom-anchoring produced a surface that reads *failed-to-load* rather than *a phone with one message*.

**Recorded, because the derivation matters more than the result:** two fixes existed — re-anchor the
content, or bound the void with furniture. **R28-am forbids the second outright.** An existing law
chose between two design options with no preference of this seat's entering it.

### 2.2 One message is the floor; zero does not exist

`RUN_START` appends before the player acts, so **the empty state is unreachable** (measured on a
frame, 2026-08-09). This document specifies no empty composition, and a future one would need a design
decision to *create* the state, not a capture to find it.

### 2.3 A fixed line budget, not a wrapping strategy

The pool is **16 authored lines across 8 message kinds, longest 60 characters** before substitution,
confirmed live. **The copy cannot grow**, so this surface has a budget rather than an overflow policy —
the distinction that made T69 and TV-12/13 expensive on surfaces whose copy *could* grow.

**Bubbles are sized to the budget.** At the shipped width a 60-character line occupies two body lines;
the bubble is the identity line plus one or two body lines and nothing else.

**A list item varying with its content is not a content-sized zone.** §3.5 and T51 forbid a *zone*
resizing at runtime; message bubbles in a list are the list working. **The panel is fixed. Items vary
inside it.** Stated because the clause will otherwise be cited at this.

### 2.4 Overflow, when it comes (C19)

**Three messages is not the proven ceiling** — the reference run stopped on a step budget, not on the
feed running dry. So:

- **Every message the engine emits is reachable.** No cap that hides one, no silent drop.
- When the stack exceeds the panel, the list scrolls with **a printed position indicator, present iff
  it scrolls** — S27's rule form, this surface's own instance. Never fades, never auto-hides.

### 2.5 The count chip earns its place or leaves

The header's count chip currently prints the **total**, beside the messages it counts. **On a fully
visible list that is zero information** — T70-am's test, and the same defect as §4.2's.

**Ruled: the chip is present iff the list does not fully fit** — the one condition under which a total
tells the player something he cannot see. Same form as the position indicator, same reason.

---

## 3. Colour

### 3.1 The one-ink rule — this surface's first law

**The phone is a neutral surface. One ink on two grounds, and no fourth value.**

The laptop runs two inks because the player marks it. **Nobody marks the phone.** He does not choose
here, nothing is transacted here, and the house is not acting on a document here — it is talking. So
the three inks that carry meaning next door all have nothing to do:

- **biro** is what *he chose* (S3) — he chooses nothing on this screen;
- **wax** is money *as an amount he can act on* — `"$60 due at the first settle"` is **prose**, the
  bookie's words, not a money control. **A figure inside a sentence is not a money element.**
- **oxide** is the house *acting on a document* (S3, S53) — a message is not an action on a document.

### 3.2 The `BOOKIE` header (P2)

The build's header was the surface's one saturated element and it is **struck**:

| | L\* | chroma | hue |
|---|---|---|---|
| **built header (measured)** | 39.93 | 10.47 | **232.1°** |
| retired `chromeCyan` | 84.32 | 22.90 | **234.5°** |
| biro blue `#5E86B8` (the player's choice) | 54.96 | 30.52 | **270.3°** |

**2.4° from a retired hue; 38.2° from the only sanctioned one.** Amplitude has never rescued a retired
hue at this studio — S63's violet, R39's phone emissions, and now this.

**The header takes the neutral ink at a reduced step.** It is a **non-interactive label naming an app
on his machine**, which is the object S31-am already ruled: LEDGER's tab strip persists non-interactive
at the drained toner step because it is *a destination on the machine, not a section of the
sportsbook*. Same object, same answer, one device over.

This resolves the meaning problem by dissolving it: **the header is not the house speaking — it is the
machine labelling a channel.** The house speaks in the bubbles.

### 3.3 Values

Three neutral values and no fourth (S70's refusal, this surface's instance): **panel ground**, **bubble
ground**, **ink**. The header takes the ink's neutral at the drained step.

Measured on the reference set and ratified as read: panel ground L\* 17.96, bubble ground L\* 42.72,
message ink at Rec.709 luma 229.78 (display-encoded, C33). **The built token values are recorded in
this document at the first build that touches them** — they are ratified here as relationships that
read, not yet as constants.

**Bound: no canvas region on this surface exceeds chroma 3.0.** Every region except the struck header
already measures 0.89–1.45.

### 3.4 The ladder

With the header neutral, the brightest element on the phone is **the bookie's words**, and that is
correct: it is the only content the surface has, and on his own machine the man he owes is the loudest
thing on the screen.

**Batch 23 declined to rule this while the surface had one content type** (C33(b) — a ranking is
asserted against a composition, and one occupant is not one). §3.2 gives it a second element, so it is
ruled here: **ink out-ranks chrome; nothing out-ranks the message.**

---

## 4. Type

### 4.1 Face

**Archivo** — S11's family, the machine's face.

**A text message renders in the recipient's device's face.** That is what a phone does, and it is why
the bookie's voice is carried by diction and case and never by a font.

- **Not Encode Sans** — that is the TV, the house's equipment (T11). The phone is his.
- **No third family.** Licensing is Allen's call and this surface does not need one.

### 4.2 The identity line, and what it does not say

The bubble's identity line is **`R1`** — S62's form, uppercase.

Two corrections land in one line:

- **`ROUND-1` is struck (P5).** The laptop prints `R1 · TICKET 01` and the TV prints `ROUND n OF 8`; a
  hyphenated third rendering makes the player learn one fact's vocabulary twice.
- **The per-bubble sender name is struck (P4).** Every bubble printed `· BOOKIE` under a header that
  already says `BOOKIE`, on a feed with **exactly one sender** — zero information at every occurrence
  after the first (T70-am).

### 4.3 The fact floor, expressed in the output channel

**This surface's floor is stated where the player's eye is, not in canvas space.** The laptop's 13px
floor was set in canvas space and consequently never enforced (S2-am, S2-am2) — the phone's is not
repeating that, and it can afford not to because its ratio is known.

- **Focused view: 1.286 canvas-px → screen-px.** Every size decision converts through it.
- **Interim floor, evidence-based:** the message body's shipped size **is** the floor — all three
  states were read at review distance on the reference frames. **Nothing on this surface renders
  smaller** until the number exists.
- **Owed:** the measured value, taken at S2-am2's baseline pass, recorded here. A floor this document
  cannot quote is a floor that will drift.

### 4.4 Case

Identity line uppercase; **body in the bookie's lowercase** (§5). No tracked uppercase on body copy —
S68's law holds here: short labels are tracked uppercase, factual copy stays literal, and a text
message is the most literal copy in the game.

---

## 5. Voice

### 5.1 The bookie is a person, and he sounds like one (P1)

**Lowercase, sentence case, terminal full stops.** Ratified and shipping.

This is a **deliberate divergence from the laptop**, recorded so nobody harmonises it: the laptop is a
*product* and speaks in its uppercase product register; the phone carries a *character*. A loan shark
texting in UI caps would be the defect. `"short. we're past texts now."` is the surface working — the
medium is part of the threat.

**The seat's own error, recorded (§1.5):** batch 20 called this "two voices in one glance" and named it
as a finding. It was read off a soft frame holding one illegible message. Two speakers correctly
sounding different is not drift.

### 5.2 Second person is legitimate here, and only here

The laptop permits second person only in genuine imperatives (S71 and its §6). **The phone is
exempt** — a person addressing him directly is the entire content. `"you know the schedule"` is correct
and is not an S71 violation.

### 5.3 What the bookie never does

- **He never tells the player to bet.** He collects; he does not advertise. The TV does not instruct
  the player to bet (T27) and neither does the man he owes — an exhortation here would be the house
  selling through a character's mouth, which is worse than selling in its own voice.
- **No hype, no exclamation marks, no superlatives** (T39/T44's register, studio-wide).
- **No promise of a win, in any voice** (S45's shape, C10's class).

---

## 6. The phone in the room

### 6.1 It is not a reading surface (P6)

Seated, the phone subtends a few pixels and its text is not readable. **Its job in the room is to read
as a phone**, not to be read from the couch. Its content is reached in the focused view.

### 6.2 The phone does not summon

**There is no channel that tells the player a message arrived, and that is the design, not an
oversight.** Stated as a positive law so it is not "fixed" by someone reading it as a gap:

- The glow route is **closed by ruling** — R39-am's in-Play A/B fired its pre-committed disposition:
  the phone's emission is unobservable at runtime, and **no cue, state or gameplay signal is ever built
  on it.**
- The text is unreadable at the seated pose (§6.1).
- The count chip is visible only to someone already looking.

**The player checks the phone between rounds.** The debt arrives whether he reads about it or not, and
a bookie you can ignore is not a bookie. **Any notification channel is new design and goes to Allen**
(§10).

### 6.3 Emission

R39's granted values stand, unchanged and not re-opened here: warm near-neutral **R ≥ G > B**, one
chromaticity family with the laptop (phone 85.4°/5.0 vs laptop 84.3°/5.3), amplitudes 1/3/15 off one
shared base. **Emission is out of this document's scope** except to record that it is settled.

---

## 7. Motion

**None.** The phone does not pulse, breathe, flash or animate a message's arrival.

R37 struck a breathing glow on the laptop; the TV has exactly one pulse and it is the TV's. A device in
peripheral vision that moves is casino urgency in an unaudited channel, and §6.2 has already ruled that
this surface does not summon. **A message appears between one frame and the next, or it does not
appear.**

---

## 8. Out of bounds

- **No authored content** (R28-am). Live engine data only.
- **No invented furniture**: no compose field, no avatars, no read receipts, no nav bar, no signal or
  battery row, no timestamp the engine does not emit.
- **No third typeface.**
- **No saturated colour** — chroma 3.0 is the bound (§3.3).
- **No second ink.** Biro, wax and oxide have no work on this surface (§3.1).
- **No cue, state or signal on the glow** (R39-am, closed).
- **No motion** (§7).
- **No bottom-anchored stack** (§2.1).

---

## 9. Gates

Real gates, per C9. Each states its instrument and, per C18 §4.2, **what it cannot see**. Every
invocation reports its executed case count and exits non-zero on zero cases (C29).

| # | Gate | Instrument | Blind to |
|---|---|---|---|
| P-G1 | No canvas region exceeds chroma 3.0 | CIELAB from linear on a rendered frame, regions eye-confirmed (C27) | whether the composition is right; anything off-canvas |
| P-G2 | The topmost message sits at the panel's content origin at every message count | layout assert, canvas-local px | whether the messages are legible or correct |
| P-G3 | The sender name appears at most once per screen | rendered string scan | duplication in any other form; whether it reads |
| P-G4 | Rendered message count == engine-emitted count, **or** the position indicator is present (C19) | list length vs feed length | whether the indicator rendered or reads |
| P-G5 | No text element below the ratified body size | constant check against the slot table | the rendered result — TMP point size and screen px are not the same quantity (L2's lesson) |
| P-G6 | No animated property on any phone canvas element | source scan for tween/lerp/Hz on the panel's tree | motion driven from outside the panel's tree |

**Capture convention for this surface:** shoot **on message-count change**, name each set by the count
it holds, pin and assert the seed (C34). The reference set established it and it is adopted.

---

## 10. Open items

Named here rather than left implicit, per C31 — this list is the whole list.

1. **The measured body-size value** (§4.3), taken at S2-am2's baseline pass and recorded here.
2. **The built token values** for the three neutrals and the header's drained step (§3.3) — ratified as
   relationships, not yet as constants.
3. **Whether 3 is the feed's ceiling.** The reference run stopped on a step budget. §2.4 is written to
   survive N growing, so this withholds nothing.
4. **P6's notification question, if Allen wants it re-opened.** This document rules that the phone does
   not summon. That is a decision, not a default, and it is cheap to reverse.
5. **Other seeds' content.** The 16-line pool is seed-selected; this set saw one seed's draw. The line
   *budget* is a property of the pool and does not vary.
