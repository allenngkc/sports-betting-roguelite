# Register entries — 2026-08-13, batch 63

**THE PHASE CLOSES.** Ruled at the DD seat on `dd-import/tv-phase-t-closing-2026-08-13/`
(66 frames, tree `5dadc24`), read at review distance at this seat, against the pair and the refused set.

**Destination table for every row: TV — match theater.**

**Rows shipped:** `T89-cl` · `T91-am2` · `T95` · `G1-am8`.

---

## T89-cl — PHASE T IS DESIGN-VERIFIED.

**Both named defects clear on frames, at review distance, on the closing tree.**

### A(a) — the NEED line. CLEARED.

The frame the refusal was written on is in this set **under the identical filename**, and it is the
third reading of one frame across three trees:

| tree | NEED line |
|---|---|
| before (`tv-phase-t-before-2026-08-11`) | `ONE TEAM BLANKED` — complete |
| refused (`tv-phase-t-afterframes-2026-08-13`) | **`ONE TEAM`** |
| **closing (`tv-phase-t-closing-2026-08-13`)** | **`ONE TEAM BLANKED` — complete, unobstructed, caption gone** |

Confirmed on `g1-column-all-markets` frame000 as well. **The caption is gone from the column, the box
runs to 261.0, and nothing prints over the string's tail.** T90-am's two levers landed exactly as
ruled.

**The gate was the PROPERTY, not the string, and the property holds.** The sweep reports **1 of 22
overrunning across 48 slots with 0 unaccounted for** — the one being `MARKET SUSPENDED`, which is on
T74's table by name and was never part of this gate. **The truncation backstop fires on nothing in the
NEED line.**

### A(b) — the money control. CLEARED, re-verified on THIS tree rather than inherited.

`CASH OUT $74` and `CASHED OUT $199` render complete and inside their field on the closing frames,
figure and status word on separate rows. **Not carried over from the earlier grant — read again here**,
because a gate certifies the geometry it ran against (C18 §4.1) and the tree moved twice since.

### B / C / D / E / F — all as recorded at T89-am, and nothing regressed.

**Suites (C29): EditMode 250 discovered / 250 executed**, 249 passed, 0 failed, 1 ignored (G1's grant,
held); **PlayMode 94 / 94 executed**, 88 passed, 0 failed, 6 by-design capture skips.

### Also landed and read on these frames, though none was a gate condition

- **T91-am's leg row:** `−280   NEXT` reads as **two facts**. The zero-gap adjacency is gone.
- **T91-am's band:** on the **widest** scoreline, `SPREADSHEETS 0 — 0 MUSKRATS`, the clock now stands
  clear where the refused set had them jammed. **The partition works on the case that founded it.**
- **T92-am:** `TakeoverSub` carries no leg list; the centre panel shows `TICKET 1 OF 1` and
  `$87 TO WIN $1,490` and nothing clipped. The run-on string naming the team three times is gone.
- **T93:** `MomentumLabel` is gone from the column.

### THE GRANT, and what it is a grant OF

> **Phase T is Design-verified.**

**Stated precisely, because the two claims are different and conflating them would be the error:
Design-verified is a statement about THIS PHASE'S OWN VARIABLE — the type stack moved from
`UI.Text` + a defaulted instance to TMP + the §4 canon faces, and the rendered delta is
accounted for. IT IS NOT A CERTIFICATE THAT THE SURFACE IS READY TO SHIP.** T83 fixed the pair's
resolution and T89 fixed its exclusions; both stand unchanged and are restated in the review note.

**T95 below is a real defect on these frames and it must land before ship.** It does not withhold this
grant, and the reason is **C31, which this seat wrote and is bound by**: the closing-condition set was
named before the evidence existed, it is exhaustive, and **new findings on the same frames open new
items rather than retroactively withholding a grant these conditions have earned.** Withholding now on
a finding outside the named set would make the gate unbounded — the exact failure C31 exists to
prevent, and this seat said so in advance at batch 60 when T91, T92 and T93 were opened the same way.

**The last step was this seat's and it is one document.** The review note at
`tv-phase-t-review-note-2026-08-13.md` is that document.

---

## T95 — the scoreline renders TWO OFFSET COPIES on score-change beats.

**NEW, opened under C31 on the closing frames** · DD 2026-08-13 batch 63. **Read at this seat at review
distance; not measured.**

On beats where the score changes, the scoreline renders as **two horizontally offset overprinted
copies** and is **not legible**:

| frame | beat | reads |
|---|---|---|
| `t68am-accept-slot` frame008 | `BreakawayAgainst`, event line *"— LEAD CHANGE"* | `SPREADSHEETS 1 — 0 MUSKRATS` doubled into an unreadable run |
| `t70am-live-pair` frame000 | `LegFinalWon`, 90'+1 | `ZAMBONIS 0 — 1 REGULATORS` doubled |

**Adjacent frames are clean singles** — `t70am-live-pair` frame001 (`FT`) and frame002, and
`g1-column-all-markets` frame000 — so it is **confined to the transition, not the steady state.**

**WHAT THIS SEAT CANNOT SAY, stated rather than glossed (§2.5, C41):** whether it is **new**. The
refused set's frame at the same index is a clean single, **but its run had diverged to a different
match state and was not on a score change** — so it is not a matched comparison, and **the absence of
doubling there is not evidence of absence.** This seat looked and did not find a matched transition
frame in the earlier sets. **A regression is a plausible reading, not a demonstrated one.**

**The mechanism, offered as the first thing to check and NOT as a finding:** T38 is the ruled scorebug
crossfade. A crossfade that dissolves in place superimposes exactly; **one that shows a horizontal
offset is placing its two layers differently** — and **T91-am re-bounded the `Matchup` box one batch
ago**, which under §3.5 obliges re-deriving everything depending on that box's centre. **If the second
layer holds a stale rect, this seat's own partition ruling is the cause.** Check that first; it is the
cheapest hypothesis and the one this seat is on the hook for.

**PRIORITY: it lands before ship.** An illegible scoreline on the goal beat is the single most-watched
element on the surface at the moment it matters most, and **T84(b)'s class — glyph on glyph** — is
what made that item a blocker. **It does not gate Phase T** (C31) **and it is not thereby minor.**

**OWED: the two beats captured deliberately rather than incidentally** — a transition frame from each
of the goal, lead-change and leg-resolution beats — **plus whether the second layer's rect is derived
from the same box as the first.**

---

## T91-am2 — the partition's OTHER side: the widest scoreline now sits tight against the ticket column.

**This seat bounded one side and left the other flush (§1.5).** T91-am's arithmetic ran *"the stage
runs −223.8 → 491.2 = 715.0px"* and gave the scoreline a territory **starting exactly at the ticket
column's right edge** — a floor against the clock, **none against the column.**

**On the frames:** with the widest scoreline, `SPREADSHEETS 0 — 0 MUSKRATS` now begins immediately
right of the leg row's state word `W`, **visibly tighter than the refused set**, where the same
scoreline sat further right and the collision was with the clock instead. **The defect moved sides.**

**Not a collision at review distance — the two are adjacent with a small positive gap** — but it is the
same class one edge over, and it is the widest renderable case, so there is nothing in reserve.

**RULED: the 2px ink floor set at T90-am applies to BOTH sides of the ticket column's edge** — it was
written for elements inside the column and it binds elements outside it abutting the same edge, since
**an edge has two sides and a floor on one of them is half a rule.** The scoreline's territory starts
2px right of −223.8.

**Consequence for the partition's arithmetic, corrected here:** the usable stage is **711.0px**, not
715.0 (2px at each end), against a widest scoreline of 583.3 — **the partition still exists**, and the
clock's territory bound tightens from ≤129.7 to ≤127.7. **The conclusion is unchanged; the margin was
overstated by 4px and this seat is naming it rather than letting the next fix discover it.**

---

## G1-am8 — the scorer arm: rung 2 authored. `{SURNAME} SCORES`.

**TV routed this rather than shipping it, and it is the same defect one arm over.** The scorer arm's
rung 2 is the bare form **`TO SCORE`**, which **names no player** — the exact property G1-am7 retired
bare `TO WIN` for, one batch ago, for T94's reason.

**And it is WORSE here, on this surface's own evidence:** the backed-side marker renders **only on
moneyline legs** (`isMl && …`, TV's own answer at G1-am6), so **a scorer leg has no marker at all** —
nothing else on the surface names the player. **G1's original ruling makes it decisive:** the
AnytimeScorer pair-defect was fixed by having the progress line read `NOT YET`/`SCORED` **precisely
because the surname is named once, by the NEED line above it.** Retire the surname from the NEED line
and it is named **nowhere**.

**RULED — the same two-rung ladder, chosen by measurement:**

| rung | form |
|---|---|
| 1 | **`{SURNAME} TO SCORE`** |
| 2 | **`{SURNAME} SCORES`** |

**Bare `TO SCORE` is RETIRED as this arm's fallback** and must not be reachable on a scorer leg.

**The rung-2 rule generalises across both arms and is stated once: drop the infinitive marker and
conjugate to the subject.** Clubs are plural and take `WIN`; a surname is singular and takes `SCORES`.
**One rule, two arms, no new vocabulary** — and it keeps the slot's established terse-declarative
register (`ONE TEAM BLANKED`, `ONE TEAM SCORELESS`: subject + required state).

**It also sits correctly against the progress line already ruled:** `PAVEMENT SCORES` above
`NOT YET` / `SCORED` reads as requirement-then-state, which is what the pair is for.

**NOT MEASURED AT THIS SEAT (§2.5, C41).** `PAVEMENT TO SCORE` is TV's measured 264.9 against 261.0 —
over by 3.9 — and dropping `TO ` while adding the conjugating `S` **should return more than that on the
arithmetic**, so rung 2's worst case looks comfortable rather than marginal. **A direction of travel,
not a number to land on.**

**OWED: `{SURNAME} SCORES` measured across all twelve surnames against 261.0.** Seconds.

**PRE-COMMITTED: (1) all twelve fit → the ladder is final, nothing returns here; (2) any surname still
overruns → it returns with the widths and this seat authors rung 3, and THE REMEDY WILL BE THE PHRASE,
NEVER THE SURNAME** — abbreviating a player's name is refused in every branch, being the coined-short-form
class (T88-am, T84-am4) applied to the one string a player is most entitled to see in full.

**Does not gate Phase T** — correctly, and TV's reasoning for saying so is adopted: **the gate is that
the truncation backstop does not fire, and an authored fallback rendering complete is the ladder
WORKING.** T89-A's own example, `ONE TEAM BLANKED`, is exactly that. **A fallback that renders complete
but names the wrong amount of the world is a VOICE defect, not a fit defect** — which is why it is
ruled here and not at the gate.

**One honest note carried into the review note: rung 2 of NEITHER arm has been seen rendered.** The
seed's ticket carries `MUSKRATS` and `PAVEMENT`, so `{CLUB} WIN` and `{SURNAME} SCORES` do not appear
in any frame the studio holds. **Their FIT is computed, which T84-am2 expressly permits** (*the
screening may be computed; the boxes it flags take rendered confirmation*), **and their VOICE is this
seat's own authorship rather than a ratification of someone's description** (T88's standard). **Their
rendered read is owed at the next capture carrying a long club or a long surname** — a note, not a
condition, and not a reason to hold a phase whose gate is fit.
