# The drawn-ending window — binding conditions and the pre-committed read

**Written:** Design Director seat, 2026-08-19, **BEFORE THE FRAMES EXIST** · **Window:** granted by
Allen, three arms, shooting now.
**Read this before shooting.** Precedent: `T89`, `T99` and `S74-am2` all pre-committed and the
pre-commitment did its job each time; batch 27 is the case where expressing one as a *value* inverted
its own test.

**`C41` is respected throughout — every criterion below is a DIRECTION OF TRAVEL or a BINARY, never a
number to land on.**

---

## 0. WHAT CHANGED SINCE `T129` WAS WRITTEN — read this first, it rewrites arm 3

`T129` framed arm 3 as *"new territory — no capture of any kind exists"* for `CorrectScore`.
**Checked at source before pre-committing a read on it, and it is worse and wider than that.**

**`TvSweatScreen` authors SIX market kinds. Nine fall through to a `default`.**

| | handled | falls through |
|---|---|---|
| `DescribeActiveLeg` — the NEED and progress pair | Moneyline (incl. `T96`'s draw branch), TotalGoals, BothTeamsToScore, TotalCorners, TotalCards, AnytimeScorer | **DoubleChance · Handicap · TeamTotalGoals · TeamTotalCorners · TeamTotalCards · CorrectScore · WinningMargin · TotalGoalsOddEven · PlayerMultiScorer** |
| `LegStatement` — the compact row | same six | same nine |

**And the two defaults fail differently, which is why this is two findings and not one:**

- **`DescribeActiveLeg` returns `new ActiveLegCopy(string.Empty, string.Empty, false, string.Empty)`.**
  The left rail's NEED and its progress line are **blank**.
- **`LegStatement` returns `leg.DisplayLabel`**, whose own `default:` is
  **`selection.Kind.ToString()` — the raw C# enum name.** A correct-score leg's row prints
  **`CorrectScore`**. Camel case, an identifier, on the player's screen.

**`MatchModel.DisplayLabel`'s doc comment says why, and it names the debt:** *"Retained behaviourally
unchanged for `TvSweatScreen.cs` (another lead's surface, forbidden to this ruling's batch) pending
that lead's own migration to `Fields`."* **`S22`'s migration reached the console and the laptop and
never reached the TV.**

**THIS IS NOT HYPOTHETICAL AND IT IS NOT WAITING ON THIS WINDOW.** The surfaces phase closed at batch
119 with all fifteen kinds reachable and takeable on the laptop's ENTRY sheet. **A player can back
`CORRECT SCORE 0-0` today, and if that ticket reaches the theater the TV prints an enum identifier
and a blank NEED.** Raised as its own row (`T130`, batch 123) rather than left inside this window,
because it is live now and the window only happens to be the thing that would have found it.

**Consequence for the shoot, and it is a BINDING CONDITION (see §1.6): arm 3 is shot AS IT STANDS.**
Do not author a correct-score form before shooting it. **The unauthored state is the evidence**, and
authoring copy from the build is the failure `A2` and `T96` both exist to prevent — *a copy ruling
lands in the deck or it has not landed.*

---

## 1. BINDING CONDITIONS ON THE WINDOW ITSELF

The five from `T129`, restated so there is one place to check, plus a sixth from §0.

### 1.1 The re-shoot is COMPARABLE or it is worthless

Same seed (`GOALLESS-5`), same matchup, same stake, same boost as
`dd-import/tv-goalless-draw-2026-08-14/`. **The entire value of arm 1 is that it can be differenced
against the docked set.** A re-shoot on a fresh seed answers a different question.

### 1.2 `C55` — the subject must be IN FRAME

For arms 2 and 3 the subject is **a specific leg row**, not a panel. Pin or force the matchup and the
selection; do not deal for them. A green capture of a ticket that did not carry the leg proves
nothing, and the docked set's own README records four passing captures that showed the wrong beat.

### 1.3 Frame-contiguous

The harness's existing convention. The README's fourth failure was realtime spacing against sim time
per rendered frame, and it produced frames labelled with a beat they did not show — `C50`'s shape.

### 1.4 The ROOM BAND IS CAPTURED, not cropped

`§6.8`'s central claim lives in the room, and `T125` could only be measured because the docked frames
happen to include the surround. **If the frame is tightened to the screen, the phase loses the only
channel that carries the settlement.**

### 1.5 Every ending runs PAST its own tally

Verified by the payout slot **changing and then settling**. `T87-am` measured the tally at 2.0s
against a 1.2s window; 150 frames at 0.02s is 3.0s and should clear it. **A window that ends
mid-tally cannot answer whether the ending resolves, and I will not read one that does.**

### 1.6 Arms 2 and 3 are shot UNAUTHORED (new, from §0)

No copy is written for the nine unauthored kinds before the shoot. The blank NEED and the enum
identifier are the before-state, and they are what `T130` is ruled on.

---

## 2. PRE-COMMITTED READ — ARM 1, the re-shoot at 150 frames

### 2.1 The binary that decides `T125`

**Does the room move on the WIN, anywhere in 150 frames?**

- **YES** → `T125` is confirmed as a *window* defect, `§6.8`'s verification is restored on live
  frames, and the settlement-glow claim stops resting on a set that no longer exists.
- **NO** → the settlement glow is genuinely absent on a drawn ending. That is a far larger finding
  than `T125` claims and it reopens `§6.8`'s central reassurance.

**I pre-commit that I expect YES**, and I am recording the expectation precisely so a frame can
overturn it. This seat's week is four desk reads corrected by frames against one confirmed
(`S101-cl`).

### 2.2 The direction that matters more than the magnitude

**The win's room response and the loss's must differ in KIND, not merely in size.** Today the loss
moves the room −6.61 and the win moves it 0.02.

**Pre-committed: I read the win as GAINING and the loss as LOSING.** If both endings move the room
the same direction, that is a finding whatever the magnitudes are — and it is the one result that
would make `§6.8`'s *"quiet for the room, LOUD for one ticket"* false rather than merely unverifiable.

**No number is pre-committed.** `C41`: the docked set contains the defect (a truncated window), so any
value read off it is a floor, not a target.

### 2.3 The identity measurement, re-run

Winner's frame against loser's frame, zone by zone, across all 150.

**Pre-committed: the zero-difference interval SHRINKS but does not vanish.** It cannot vanish — the
hold is ruled and both halves share it. **What is being measured is how long the two players watch
the same broadcast once the complete ending is visible.**

- Still ≈1.0s → `T124` stands unchanged.
- Materially shorter, because settlement arrives inside the hold → **`T124` softens and I will say
  so plainly.** The finding is the interval, not the grievance.

### 2.4 What I will NOT conclude from arm 1

- **I will not conclude the hold is too long from the fact that it is still.** Stillness is
  measurable; *too long* is a read, and the read needs the complete ending plus the direction Allen
  is holding. **That judgement is not mine to make in this window.**
- **I will not author a treatment.** The hold-not-climax direction is with Allen and the spec waits
  on it.
- **I will not re-open `T87-am2`, `T96`, `T97-am` or `T98`.** All four hold on the docked set and
  nothing in this window is aimed at them.

---

## 3. PRE-COMMITTED READ — ARM 2, count legs settling level

### 3.1 The collision I expect to find, named before the frames

**`T123` and `T87-am2` disagree about whether `THE MATCH ENDS LEVEL` should print on a corners
ticket, and arm 2 is the frame that shows it.**

- **`T123` (batch 120):** *treatment is earned by DISTANCE TO THE LINE, and a goal has no distance to
  a corners line — so by the grammar's own rule a goal earns nothing on a count ticket.* Applied
  consistently, **a drawn ending has no distance to a corners line either.**
- **`T87-am2`:** the line *fires at the whistle of a drawn match*, unconditionally.

**My lean, on the record and NOT binding: `T87-am2` wins and the line stands.** The strip's L2
statement is about **the MATCH**, not about the leg — `§6.8` says its job is *to say what the score
and clock cannot* — and a corners backer is still watching a match that ended level. `T123` governs
what a **beat** earns; this is not a beat.

**If the frames make that read look wrong, the lean loses.** It is written down so it can.

### 3.2 The two families, which must be shot and read separately

- **Goal-count legs** — `UNDER 1.5 / 2.5 / 3.5 GOALS`, `BTTS — NO`, `TOTAL GOALS EVEN`. All win on
  a 0–0. **For these the drawn ending's line IS their result restated**: `THE MATCH ENDS LEVEL` at
  0–0 is *why* `UNDER 1.5` won. **Pre-committed: I expect `T69`/`T70`'s family here** — the strip
  and the leg's grade naming one fact — and I am looking for it rather than hoping not to find it.
- **Non-goal count legs** — corners, cards. **For these the line is TRUE AND IRRELEVANT.** This is
  §3.1's collision in its clean form.

**One ticket carrying an under leg and a BTTS-NO leg covers the first family; a corners leg covers
the second.** They may ride together or separately, but **the read is separate either way.**

### 3.3 The binary

**Does the count leg's ending read as its own resolution, or as the match's?** A count leg that wins
on a drawn match has two true statements available and the surface currently makes only one of them.
Whether the player can tell *why his leg won* from the ending is the question, and it is answerable
from frames.

---

## 4. PRE-COMMITTED READ — ARM 3, correct score `0-0`

### 4.1 What the frame will show, predicted at source rather than hoped for

Per §0: **the leg row prints `CorrectScore` and the NEED pair is blank.** I am predicting it from the
two `default` arms, and **the frame is what makes it a finding rather than a source read** — `C17`,
and the discipline `S86` and `S93` were both held to.

**If the frame shows something else, the source read was wrong and I will say so.** That outcome is
live: `_ledger` and the describe path have branches this seat has not traced exhaustively.

### 4.2 The shape that is genuinely new, and why it is worth the arm

**Correct score's NEED is the only one in the vocabulary that is met continuously and unmet the
instant anyone scores.** Every other market's need is monotone (a count climbing to a line), terminal
(full time), or one-way (a scorer who cannot un-score). `0-0` is **true from kick-off and destroyed
by a single event** — and on a goalless draw it is true for ninety minutes and then simply stops
being provisional.

**That is the phase's extreme case and it is why this arm is worth shooting even though §0 already
tells us the row is unauthored:** the authoring problem is `T130`'s, but **what a continuously-met
need should SAY while it is being met is a design question no existing market poses.**

### 4.3 `C46`, and it is not optional

`CORRECT SCORE 0-0` and its siblings, measured against the leg row's **249.0px** box (`T111-am`'s own
figure) — **against the ENUMERATED POOL, never the sweep's widest measured** (`S84`, `S96-am`).
`WINNING MARGIN` and `TEAM TOTAL CORNERS` are the wide ones in that family and they are in the same
nine.

**Batch 95's correction binds: the widest string in a column is a MEASUREMENT, never something
readable off string lengths or type sizes.** I will not accept a width I could have guessed.

### 4.4 What I will NOT conclude from arm 3

- **I will not author the nine kinds' forms from this window.** That is `G1`'s ladder and a deck
  authoring pass, sized once `T130`'s measurement lands, and it is a phase of its own — **nine kinds
  × two forms is not a corner of the drawn-ending phase and must not be smuggled into it.**
- **I will not rule the correct-score NEED's wording** on one match's frames. §4.2 is a question this
  arm poses; answering it needs the losing case too — a `0-0` need destroyed by a goal — which this
  window does not shoot.

---

## 5. WHY THIS DOCUMENT EXISTS

Two reasons, both paid for.

**A read written after the frames arrive is shaped by them.** Batch 27's pre-commitment failed not
because it existed but because it was expressed as a value; every criterion above is a direction or a
binary for that reason.

**And a binding condition that lives only in a register row gets missed.** The last window cost a
re-shoot because its conditions were not in one place the shooter would look. **This is that place.**
