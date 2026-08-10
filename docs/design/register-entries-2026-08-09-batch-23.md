# Register entries — 2026-08-09, batch 23

**Seat:** Design Director (`main-2` terminal) · **Source:** `dd-import/phone-reference-set-2026-08-09/`
(README + six frames; `msgs-01-focused` and `msgs-03-focused` read at review distance by this seat),
seed `PHONEREF01` pinned and asserted (C34).

**The phone's five decisions, ruled against frames.** New IDs: **P1–P8**. The owning document is
unblocked and is this seat's to write.

---

## The set is accepted, and two of its findings are better than the frames they came with

Both desk findings **answer** contract questions rather than illustrate them, and both change what the
document says:

- **"Empty" is unreachable** — `RUN_START` appends before the player acts. My contract called empty
  "the state the player sees most"; it is a state that **never occurs.** §1.4's question, *what the
  surface is when the bookie has not spoken*, was a question about nothing. **One message is the
  floor**, and every composition ruling below is written against that floor rather than against zero.
- **The pool is 16 lines across 8 kinds, longest 60 characters, confirmed live on a frame.** So there
  is no wrapping strategy to specify — there is a **fixed line budget**, which is a materially
  different and much smaller document. **Refusing to compose a long string under R28-am was correct**
  and is the reason this is decidable now: an authored 200-character message would have produced a
  wrapping spec for a case the engine cannot reach.

**Recorded as the lane's, not the seat's:** three of my four contract states did not survive contact
with the engine, and the lane said so instead of shooting what it was asked for. **`msgs-03` is
honestly labelled "not proven to be the maximum"** — stopped on a step budget, not on the feed running
dry. P4 is written to be robust to N growing, precisely because that was disclosed.

## P1 — The bookie's lowercase register is RATIFIED. My batch-20 read was wrong (§1.5)

Batch 20 named "two voices on two screens the player sees in one glance" as the finding carrying
C26-am3. **On clean frames with legible content, that read does not survive.**

The phone carries a **character** — a person texting. The laptop carries a **product**. A loan shark
who texted in the laptop's uppercase would be the defect; a product that spoke in his lowercase would
be another. **Two speakers correctly sounding different is not drift, and I called it drift from a
soft frame holding one illegible message.**

**Ruled:** the lowercase, sentence-case, terminal-stopped register is the bookie's voice and it
**ships**. `"short. we're past texts now."` is the surface working — the medium is part of the threat.

**What survives from batch 20:** the **face** is still nobody's decision. A default humanist sans was
never chosen; it is what was there. Register ratified, face owed to the document (P8).

## P2 — The `BOOKIE` header's hue is STRUCK. Measured, not judged

The one saturated element on the surface renders **hue 232.1°, chroma 10.47** — everything else sits
at chroma 0.89–1.45, effectively neutral. Computed against the two references it could plausibly be,
in the same space (C33-am3):

| | L\* | chroma | hue |
|---|---|---|---|
| **phone `BOOKIE` header (measured)** | 39.93 | 10.47 | **232.1°** |
| retired `chromeCyan` (0.62, 0.86, 0.96) | 84.32 | 22.90 | **234.5°** |
| biro blue `#5E86B8` — the player's choice (S3) | 54.96 | 30.52 | **270.3°** |

**2.4° from the retired hue. 38.2° from the sanctioned one.** The eye could not have settled that and
two seats had already declined to try.

**Ruled — it goes.** Three reasons, and the third is the one that matters:

1. It is **chromeCyan's hue family at about half the chroma and half the lightness.** This studio has
   twice refused to let amplitude rescue a retired hue — S63's violet struck at any amplitude, R39's
   phone emissions struck as a blue quadrant at a *tenth* the amplitude. **A third instance is a
   pattern, not a coincidence.**
2. **Nothing sanctions it here.** T9 retired `chromeCyan` on the TV, not game-wide, so this is an
   **extension and I am recording it as one, not disguising it as an application.** The extension is
   cheap to justify: the phone is the one surface with no palette, and the first colour it puts on
   screen landed 2.4° from a hue another surface threw away. That is C9's prediction, on schedule.
3. **The meaning is inverted, and this outranks the hue.** `BOOKIE` is the **house's name**, printed
   in the cool quadrant, on the object S44 ruled the player owns, one glance from a surface where blue
   means *what he chose*. The house does not get to speak in the player's ink on the player's phone.

**Replacement: direction only, no value** (R41's shape — a value picked in isolation is the colour form
of "a bound is not a layout", §3.5). Direction: **the phone sits in the laptop's register (R19(a),
R28), so it inherits the laptop's ink meanings, and the bookie is the house.** Oxide — the house's
mark — is the candidate the existing vocabulary points at. **Decided in the owning document, against
the whole surface, not here.**

## P3 — The stack TOP-ANCHORS. The existing law decides it

Messages accumulate **upward from the foot**: at one message the screen is ~70% dead space above a
single bubble; at three it is still over half. The grammar is a chat thread's — newest at the bottom,
anchored where a compose field would be.

**There is no compose field, and there never will be.** The player cannot reply; this is a
one-directional feed of notices. Bottom-anchoring borrows a convention from a UI whose *reason* — a
keyboard, a thumb, an input box — does not exist on this screen. What it produces is a surface that
reads as **failed-to-load** rather than as *a phone with one message on it*.

**Ruled: content starts at the top; the remainder falls to the foot and reads as room to grow.**

The derivation matters more than the ruling. There were two ways to fix a void: re-anchor the content,
or bound the void with furniture (a thread header, timestamps, a dead compose strip). **R28-am forbids
the second outright** — nothing may be authored onto this screen. **One permissible route, and it is
the cheap one.** An existing law chose between two design options without my preference entering it.

**Overflow is part of this ruling, because 3 is not the ceiling.** The document states what happens
when the stack exceeds the panel, and **C19 binds**: every message the engine emits is reachable.
Not a cap that hides messages, and not a silent drop.

## P4 — The sender is named three times. T70-am's test, one surface over, one day later

Every bubble prints `ROUND-n · BOOKIE`, under a screen header that already says `BOOKIE`, on a feed
with **exactly one sender**.

**T70-am's corrected test — does the line carry information the line above does not? — answers it
without a new argument.** On a single-sender feed the sender's name carries **zero** information at
every occurrence after the first. Three renderings of one fact; the header's is the one that earns
its place.

**Ruled: the per-bubble sender name goes.** The bubble header carries the round tag alone. Returns
the width to the copy, which has a 60-character budget to spend.

**Worth noticing:** T70-am was written this morning to fix a rule phrased as vocabulary rather than
information, and its first application outside the market that produced it landed the same day, on a
different surface, with no adaptation. That is the test being right, and it is recorded as evidence
that the correction was worth making.

## P5 — `ROUND-1` is a third form of a fact with two agreed forms

The laptop prints `R1 · TICKET 01` (S62). The TV prints `ROUND n OF 8`. The phone invents
`ROUND-1` — hyphenated, a third rendering.

**Ruled: the phone is in the laptop's register (R19(a)), so it takes S62's form — `R1`.** With P4
removing the sender, the bubble header is `R1`. S62's own reason applies unchanged: the DS has a form
for this, and a surface that invents a fourth one makes the player learn a vocabulary twice.

## P6 — The phone is NOT a reading surface in the room. Ratified, and it opens a real gap

Seated, the phone subtends a few pixels and its text is not readable; with R39-am's glow carrying no
cue, **the surface's job in the room is not to be read from the couch.** Ratified from evidence — the
document can say this rather than hedge it.

**The gap this opens, named and not ruled:** the player has **no channel that tells him a message
arrived.** The glow route is closed by ruling (R39-am, pre-committed and fired), the text is
unreadable at the seated pose, and the count chip is only visible to someone already looking at the
phone.

Two honest answers exist and **the document must pick one**, with Allen where it is a direction call:

- **The player checks the phone between rounds.** Legitimate, and arguably right — the debt arrives
  whether he reads about it or not, and a bookie you can ignore is not a bookie. Costs nothing.
- **A non-glow channel exists.** Anything here is new design and goes to Allen.

**I am not inventing a notification requirement.** A surface that is missable may be missable on
purpose, and R39-am closed the obvious route deliberately.

## P7 — The ladder is NOT ruled, deliberately

The brightest element is message text at Rec.709 luma 229.78 in a bubble at 58.51, where the laptop's
brightest is money. The set poses this as a ruling for the seat. **It is not one yet.**

**C33(b): a ranking is asserted against a composition, and this surface has one content type.** With a
single occupant there is nothing to out-rank — "the bookie's voice is the brightest thing" describes a
screen holding only the bookie's voice. Ruling it a violation would rank an element against nothing,
which is the error C33 exists to prevent.

**It becomes a real question the moment P2's header takes a sanctioned ink**, because then two
elements compete. Sequenced there, in the document.

## P8 — What the owning document still owes, and what it no longer does

**Unblocked and reduced.** Still owed: the **face**, the **size floor** (with the focused view's
canvas→screen ratio of **1.286** now known, so a size decision converts to the player's eye directly),
P2's replacement value, and P6's decision.

**No longer owed, and this is the set's real yield:** a wrapping strategy (a fixed line budget
replaces it), an empty-state composition (the state does not exist), and the cyan question (measured).
**Three sections of a document deleted by two desk findings and one measurement.**

---

## Adopted — the ratio instrument's characterization (C37's precondition, discharged)

Recorded as standing, on the instrument itself, before it is used for anything:

- Recovers known Gaussian kernels to within **~4%**.
- Blurs add in **quadrature over a 1.680 px floor** — a true added blur is `√(measured² − 1.680²)`.
  **Consequence: the floor is not subtractive, and anyone reading these numbers linearly will
  overstate small regressions.**
- **`ramp ÷ stroke` saturates near 0.60 above σ≈1** — a **low-blur instrument only.** A future reading
  of 0.60 means *"badly blurred"*, not *"60% worse"*.

**The saturation limit is the valuable half and it was volunteered.** An instrument that reports its
own ceiling before anyone hits it is C32 and C37 working as intended — and it means S2-am2's baseline,
when taken, will be a number that stays meaningful instead of one that quietly tops out.
