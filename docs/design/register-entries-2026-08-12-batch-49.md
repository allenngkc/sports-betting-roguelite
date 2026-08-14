# Register entries — batch 49

**Design Director** · 2026-08-12 · docket: the three theatre questions from the draws work
(`markets-pregame` D1, `3dc7a03` — *"DRAWS. The moneyline is 1X2"*).

**Destination tables:** `S74` → **SureThing**. `T87` → **TV**. `C47` → **Cross-surface**.

---

## The engine fact that decides two of the three questions

> `MatchResult {Home, Draw, Away}`. **`Side` is untouched and stays two-valued: it is a TEAM
> designator** … `Pick.Side` / `Leg.Side` **now throw on Draw**.

**The draw is not a team.** The engine ruled that structurally and the surface must not undo it in
presentation. A draw rendered as a third team is a category error the code has already refused, and
`Side` throwing on `Draw` is the compiler saying so.

---

## S74 — the draw on the board, and the forms it needs

### It renders BETWEEN the two teams, and it is not named as one

**S24 predicted this exact moment:** *"paired pricing describes two-outcome markets; scorer renders as
a single-column offer list, never a paired row with a dead cell."* The moneyline **is no longer a
two-outcome market**, so the paired row no longer describes it and must not be made to by force.

**Ruled: three offers, the draw in the middle.** Its position is not convention borrowed from real
books — it is *meaning*: the draw sits between the two teams because it is the outcome where **neither
wins**. Putting it at either end would make it look like a third competitor, which is precisely what
`Side` refuses to let it be.

**It is named as what it is — `DRAW` — never as a team, never with a team's treatment.** No pitch dot
(T2 gives the muted blue and pink to the two sides; a draw has no side and takes no hue), no crest, no
team-coloured anything.

### `1X2` never reaches the player

Industry jargon, exactly as `SGP` was. **S22 governs**: the surface composes, the role is printed as a
word. The board prints the three offers as words. `1X2` is a code word for a market shape and it stays
in the code.

### G1's deck owes a draw form, and this is that authoring

G1's own scope line said *"a seventh market needs a form authored before it ships."* The draw is not a
seventh market — it is a **third outcome on an existing one** — but the moneyline's authored forms
**assume a team** (`MIDDLEMEN ML`), and a draw leg has none. So the deck is short two strings and they
are authored here, as G1 is this seat's.

**And the word already exists on this surface.** T62 records the live leg reading `LEVEL 0–0 at 14'`
— **`LEVEL` is already the product's word for a tied scoreline.** Nothing is invented:

| form | string | note |
|---|---|---|
| **NEED** (requirement, while live) | `LEVEL AT FULL TIME` | states the requirement, not the team |
| **progress** (state, beneath) | `LEVEL` / `NOT LEVEL` | the state now |
| **compact** (identity elsewhere) | `DRAW` | the board's own word |

**T70's pair check passes, and by T70-am's own test rather than by luck:** the check is an
*information* test, not word-overlap — *shared words that carry different facts are legal*. `LEVEL AT
FULL TIME` is a requirement at full time; `LEVEL` is the state right now. Different facts, same word,
legal.

**Fit is not asserted.** These are new strings in the canon face and they measure against their
columns like everything else — they join the sweep's population under C46, and any overrun routes to
T74 with the rest.

---

## T87 — the final beat of a drawn match, and the word that must not reach the player

### `decisive` is an engine term and it never prints

The engine's `P(home | decisive)` is exact and correct **inside the engine**, where *decisive* means
*not a draw*. On the surface it is a rig string in a player slot — **T31 and R38's class, the fourth
instance.** A player has no concept of a decisive match; he has a scoreline.

**Ruled: no player-facing slot ever prints `decisive`, or any partition term the engine reasons
with.** If the TV's beat selection reads a `decisive` flag internally, that is fine and invisible.
What is refused is the word reaching a slot.

### The beat itself: the whistle, not a verdict — and not nothing either

A drawn match has no goal to end on. Two failure modes sit on either side of this and both are already
ruled against:

- **Manufacturing a climax** — a flourish to give the ending weight it does not have. That is
  celebration, and T35 and T40 both closed it (a full-field wash spends the whole ration on a moment
  and is forbidden on mechanism, not on taste).
- **Rendering nothing** — the match simply stops. A resolution that draws as an absence reads as a
  bug, and it is the same defect as an implication leg that changes nothing (T84's class): the surface
  looks broken and the player learns a false rule.

**Ruled: the beat is the match ending level, STATED.** The scoreline holds at its level value; the
event strip states the fact at its own L2 tier (T66); the legs resolve to their words. The theatre
reports; it does not editorialise about a quiet ending.

### The half that matters most: a draw is quiet for the room and LOUD for one ticket

**A draw-backer has won.** His leg lands like any other winning leg, and **the absence of a goal is not
the absence of his result.** The single worst outcome available here is a surface that conflates *no
goal* with *no result* and drains the one player whose ticket just came in.

**The machinery already handles this correctly and it should not be re-solved:** T65's settlement glow
**fires on settlement, not on a goal** — room's own record notes both observed beats are *losses*,
confirming it is *"keyed to the moment, not to a win."* A drawn match settles, so it is already a
first-class settlement moment in the room. **Nothing new is needed; nothing existing may be narrowed
to exclude it.**

---

## C47 — the match has three outcomes; a bet has two

**Law, cross-surface.** Sited here rather than in a surface table because it governs **both** owning
documents' settlement language, and a cross-surface law transcribed into a surface's table is
invisible to anyone reading the laws end-to-end — C43's founding defect, now seen twice.

> **A draw is a third MATCH outcome. It is not a third BET outcome. Result language needs no third
> word: a leg either landed or it did not.**

**Q3 answered: no.** Back the draw and it draws — **you won.** Back a team and it draws — **you lost.**
Two words still cover every bet that can be placed, and `S23`'s enum
(`PENDING · RIDING · LIVE · GREEN · DEAD · VOID · CASHED OUT`) is **unchanged and needs nothing
added.**

**What this prevents, which is why it is a law and not an answer:** inventing a `DREW` leg state would
be **modelling the match inside the bet's vocabulary** — a category error, and the same one `Side`
already refuses in the engine. A leg has no opinion about how the match ended; it knows only whether
its condition held.

**The one legitimate neighbour, so it is not mistaken for a counter-example:** a market that *returns
the stake* on a draw (draw-no-bet's family) is a **VOID**, which the enum already carries. That is a
different market's rule, never a third result.

**Laptop echo, ruled by the same line: slip settlement language does not change.** No new word, no new
state, no new column.
