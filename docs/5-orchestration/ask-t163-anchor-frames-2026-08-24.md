# CAPTURE ASK: `T163`'s anchor, two legs — TV → DD (2026-08-24)

**The DD pre-commits before any shutter.** This document is the ASK and its SCOPE only: what is
built, why no dock supplies it, exactly which two legs, and what each frame must CONTAIN for it to
test anything. It authors no read and no criterion.

**Rulings in play:** `T163` (the three branches) · `T163-am` + `spec-neither-branch-lines-2026-08-21`
§5 (the club-free set) · `T96` (the draw is not a team) · Allen's split, 2026-08-24 (`AnchorSide`
where it answers, the HOME convention where it is NEITHER).

---

## 0. FIRST: THE MECHANISM IS BUILT — checked here before asking for anything (`C59`)

`T148-vf` died because a criterion was asked of a mechanism that did not exist. So, at this seat:

- **`c24b32c`** — `SweatFlavor.AnchorForTelling` composes `T163`'s three branches over
  `evt.LegIndices`; `For` / `GoalLine` / `NoGoalLine` / `NeutralLine` route to `NeitherLine` when
  there is no anchor; `PickedHomeForPresentation` is now
  `(MatchModel.AnchorSide(leg) ?? Side.Home) == Side.Home`.
- **`10907a8`** — the twelve club-free lines, pinned verbatim against §5.2.
- **EditMode 331/330/0/1 · PlayMode 149/124/0/25.**

**AND BOTH LEGS ARE REACHABLE ON A REAL BOARD**, which is the other half of the `T148-vf` check —
a frame cannot show a ticket the player cannot build. Read off `MatchModel`'s offered set:
**`Handicap` (4 selections), `TotalGoals` (2), `BothTeamsToScore` (2)** are all offered today.

> **AND ONE KIND THAT CANNOT BE SHOT, STATED SO IT IS NOT ASKED FOR.** `DoubleChance` is on the
> engine's disagreement list, but it **left the offered set** (`spec-doublechance-removal-2026-08-24`).
> Its `AnchorSide` rows survive only so in-flight legs still grade. **No new ticket can carry one, so
> no frame can show one.** Not a gap in this ask — a shape that no longer exists to shoot.

---

## 1. WHY NO EXISTING DOCK SUPPLIES IT — the survey, run before requesting a window

**28 capture sets in `dd-import` were scanned.** The three `corners-*` sets are the only TV sweats on
a `T163` branch-(3) kind, so they were the candidates. **They fail STRUCTURALLY, not by timing, and
that is the stronger finding:**

`SweatFlavor.For` returns `CornerLine` / `BookingLine` **early**, before the anchor is used, for
`TotalCorners` and `TotalCards` — and those tables were **already club-free**
(`"whipped into the corner — the count moves again."`). **The anchor never reaches the strip on a
corners leg.** No corners dock could ever have shown this, however it was shot or whenever.

**And every docked TV set predates `c24b32c` by construction**, so even a matching kind would show
the pre-change state. Useful as a BEFORE if the DD wants the pair; never as evidence of the fix.

---

## 2. THE SCOPE — two legs, and what each frame must CONTAIN

`C55` binds: a green capture proves nothing if the subject is not in the frame. Each leg below names
its **subject element**, and the beat types that route AWAY from the anchor are excluded by name so a
frame cannot be shot on one by accident.

### LEG A — a `TotalGoals` **or** `BothTeamsToScore` leg · branch (3), `AnchorSide` = NEITHER

| | |
|---|---|
| **subject** | the event strip, on a **`Score` / `BigPlay` / `Momentum`** beat |
| **must be in frame** | the strip element, non-empty, plus the ticket column so the leg's kind is legible |
| **what changed** | this leg used to name the HOME club as `{picked}`; it must now name **no club** |
| **EXCLUDED beats** | `LegFinal` (returns `"FINAL WHISTLE"`), `NearMiss` (already club-free), and any corners/cards leg (routes to `CornerLine`) — none of these touch the anchor |

### LEG B — a `Handicap` leg backed **AWAY** · `AnchorSide` = Away, old table said HOME

| | |
|---|---|
| **subject** | the event strip **and the scorebug, in the SAME frame** |
| **must be in frame** | both, simultaneously — a frame with one and not the other cannot test this |
| **what changed** | the strip used to name the HOME club while the away side was backed |
| **why both elements** | this leg is the case where prose and geometry could CONTRADICT. The claim is that they now name the **same** club, and one element alone cannot show agreement |

---

## 3. WHAT THE FRAMES WOULD TEST — two propositions, not an impression

1. **Branch (3) renders club-free** on a kind that shipped naming HOME.
2. **Where the anchor answers a real side, the strip and the scorebug name the SAME club** — which is
   the whole reason both halves of the split landed in one diff rather than two.

---

## 4. NOT CLAIMED, AND WHY THIS IS OWED AT ALL

- **The suites prove NO REGRESSION, not correctness.** The draw leg has a direct fixture
  (`SweatFlavorDrawAnchorTests`, re-based). **The other three cases on the engine's own enumerated
  disagreement list — `Handicap/Away`, `TotalGoals/Over`, `PlayerMultiScorer/Yes` — have no fixture
  that RENDERS them.** A green suite here means the adapter compiles into the ledger and a real sweat
  runs; it does not mean a totals leg's strip reads correctly.
- **This is the first step of the four that changes shipped copy on ORDINARY tickets.** Steps 1–3
  were no-ops on the shipping shape and were evidenced by suites alone, correctly. This one is not,
  and that difference is why a window is requested now and was not before.
- **No same-match ticket is involved.** That is deliberate: the subject is what `T163` does to the
  tickets players build today, not to the N-live shape. The same-match coverage gap is recorded
  separately in the handoff and is NOT part of this ask.
- **`PlayerMultiScorer`** is on the disagreement list and is offered, but is not asked for here: its
  frame would test the same proposition as LEG B by the same mechanism. Named so its absence is a
  scoping decision on the record rather than an oversight.

**Asked of the DD:** the pre-commitment. This seat authors no read, and will not shoot before it
exists — `dd-precommit-binds-the-capture-window` cost a re-shoot on 2026-08-16 by going the other way.
