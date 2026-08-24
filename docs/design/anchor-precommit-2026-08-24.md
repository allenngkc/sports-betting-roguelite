# The anchor window (step 4's two frames) — binding conditions and the pre-committed read

**Written:** Design Director seat, 2026-08-24, **BEFORE THE FRAMES EXIST AND BEFORE TV'S SCOPED ASK
ARRIVES.** Nothing below is shaped by either.
**Rulings under test:** `T163` (the anchor is ticket-and-fixture derived and admits *neither*) ·
`T163-am` / `T163-am2` (the club-free line set) · `K17-cl` (the same defect, ruled on the console).

**No existing pre-commitment covers this** — checked; the anchor was ruled at batch 167, after the
newest of them.

> ## ⚠ AMENDED 2026-08-24, batch 183 — AGAINST TV'S SCOPED ASK (`8d18e33`), BEFORE ANY FRAME EXISTS
>
> **The ask is accepted in full except one condition, and the exception is the one that decides
> whether LEG B can be read at all.**
>
> **LEG B NEEDS THREE ZONES, NOT TWO.** The ask requires *"the strip AND the scorebug
> SIMULTANEOUSLY"*. **That is one short: it cannot show WHICH SIDE HE BACKED.** The scorebug names
> both clubs and says which is away, and the strip names one — but the `●` backed marker renders on
> **moneyline legs only**, and LEG B is a Handicap. **So without the leg's own row (`{CLUB} ±1.5`)
> the frame shows an anchor with nothing to check it against.** §2's condition 6 stands unchanged and
> binds: **row + strip + scorebug, in one frame.**
>
> **VERIFIED AT THIS SEAT, so the ask's own claims are not merely relayed:** DoubleChance is
> **genuinely unshootable** — zero `offers.Add(…DoubleChance…)` remain in `MatchModel`, so the
> removal landed and no new ticket can carry one. Its surviving `AnchorSide` arm is what
> `spec-doublechance-removal` §1 required (grading outlives offering). `MatchModel.AnchorSide`
> throws on an unknown kind (`:635`) — `K17-cl`'s exhaustive-and-throw design, now engine-side.
> **The exclusion is a shape that no longer exists, exactly as the ask says.**
>
> **PlayerMultiScorer's exclusion is ACCEPTED** — *"its frame would test LEG B's proposition by
> LEG B's mechanism"* is correct scoping, and naming it is what keeps its absence from reading as an
> oversight.
>
> **THE CORNERS REASONING IS ACCEPTED AND IS SHARPER THAN §1's:** `SweatFlavor.For` returns
> `CornerLine`/`BookingLine` **early, before the anchor is used**. So no corners dock could ever have
> shown this **however or whenever it was shot** — a stronger statement than *those frames are old*,
> and it generalises to any future ask.
>
> **AND §5'S BEFORE-PAIR IS UPGRADED FROM AN ECONOMY TO EVIDENCE.** The ask says LEG A *"used to name
> the HOME club and must now name none"* — **`drawn-ending-t129-2026-08-19/arm2` is that before**:
> `UNDER 1.5 GOALS` + `BTTS — NO` on `GOALLESS-5`, docked, and predating `c24b32c` by construction.
> **It is what makes the change a FIX rather than a change** — the ask itself notes the suites prove
> no regression rather than correctness, and this is the evidence that closes that gap. Shooting
> LEG A on that seed and matchup costs nothing and buys it.

---

## 1. THE TWO FRAMES, AND WHAT EACH IS FOR

They are not two samples of one thing. **They test the two OPPOSITE branches of `T163` and neither
can substitute for the other.**

| frame | leg | branch under test |
|---|---|---|
| **A** | a **totals or BTTS** leg | **NEITHER** — no live leg names a side, so the strip must render a CLUB-FREE line |
| **B** | a **Handicap on the AWAY side** | **SIDE** — a leg that does name one, and the anchor must follow the side he BACKED |

**TV's reason for refusing the corners sets is correct and worth recording:** the corner family has
its own club-free lines (`CornerFor` / `CornerAgainst`), so **the anchor never reaches the strip on a
corners beat at all.** A corners capture cannot exercise this in either direction — it is not that
the frames are old, it is that the code path does not pass through the thing under test.

---

## 2. BINDING CONDITIONS

### Both frames

1. **THE STRIP LINE MUST COME FROM AN ANCHOR-INTERPOLATING TABLE** — `ScoreUp`/`ScoreDown`,
   `BigUp`/`BigDown`, `MomUp`/`MomDown`, or `NeutralLine`. **The lane asserts WHICH table produced
   the rendered line**, in the harness, not by eye. A frame whose strip happens to carry a
   count-family line proves nothing about the anchor and would read as a clean pass — `C55`'s shape
   and `C60`'s, one medium over.
2. **`C55` — the subject must be IN FRAME**, and for frame B the subject is an AGREEMENT between
   three zones (below), so all three must be legible in ONE frame.

### Frame A — the *neither* branch

3. **No live leg on that fixture names a side.** A totals or BTTS leg alone; if the ticket carries
   another live leg on the same fixture that DOES name a side, `T163`'s branch (1) fires instead and
   the frame tests the wrong thing.
4. **The rendered line must name NO club.** That is the whole assertion.

### Frame B — the side branch

5. **The Handicap leg backs the AWAY side.** Home would pass under the old defect as well as the new
   ruling and therefore proves nothing — **this is the same trap `T149-am` caught when a cash-out
   frame could not test the bust**, and it is the condition most likely to be met in spirit and
   missed in fact.
6. **THREE ZONES IN ONE FRAME: the leg's own row, the strip line, and the scorebug.** The defect
   `K17-cl` found on the console was exactly this disagreement — backing `MIDDLEMEN +1.5` while the
   strip narrated `Turnips` — and it is only visible when the row that names the backed club and the
   line that names a club are both readable, against a scorebug that says which club is away.

---

## 3. THE PRE-COMMITTED READ

Binaries and directions, never a number to land on (`C41`).

- **A1 — BINARY: the strip names no club** on a totals/BTTS beat.
- **A2 — DIRECTION: the club-free line reads as OBSERVATION, not as a gap.** `T163-am` authored
  these against *third person, flat, observed*; the risk is that a line with no subject reads as
  copy that failed to load rather than as the surface declining to take a side.
- **B1 — BINARY: the club the strip names is the club the leg backs.** On an away-backed handicap
  that is the AWAY club.
- **B2 — DIRECTION: the three zones read as agreeing.** Not merely *are they consistent* — whether a
  reader taking the row, the strip and the scorebug together comes away with one story.

### My lean, on the record and NOT binding

**I expect A1 and B1 to PASS** — both are mechanical and the build is `K17-cl`'s shape, already
proven on the console with a mutation-tested gate.

**A2 IS WHERE I EXPECT TROUBLE, and it is the reason frame A is worth a window at all.** The
club-free lines were authored for a surface that could not supply an actor; they have never been
seen beside the club-NAMING lines they sit among. **A strip that names clubs on most beats and
suddenly names none may read as an omission rather than as a choice.** If it does, the fix is copy
and it is mine — `T163-am` §5's set is where it lands.

**This seat's leans get overturned by frames more often than confirmed, which is why both halves are
written down.**

---

## 4. WHAT I WILL NOT CONCLUDE

- **I will not judge the neither-branch COPY from frame B**, or the side branch from frame A. They
  test opposite branches and each is silent about the other.
- **I will not re-open `T163`.** The frames test the build against the ruling, not the ruling.
- **I will not read the counter, the footer or the tape here.** `T165-am`, `T133` and `T166` have
  their own evidence and none of it is in this window.
- **I will not accept frame B on a HOME-backed handicap**, however clean it looks.

---

## 5. A FREE *BEFORE* FOR FRAME A — and it costs the window nothing

**`dd-import/drawn-ending-t129-2026-08-19/arm2` already carries the totals/BTTS composition**:
`UNDER 1.5 GOALS` + `BTTS — NO`, seed `GOALLESS-5`, matchup 0, stake 25, 150 contiguous frames.
**It predates step 4, so it is the BEFORE state of exactly frame A's subject.**

**Shoot frame A on that seed and matchup** and the pair is readable against each other — the
before-and-after discipline `T82` and `T89` used, for the cost of choosing a seed that already has a
docked predecessor.

**Not a condition, an economy.** If the lane has a reason to prefer another seed, the frame is still
valid; it just loses a free comparison.

> **One property of that seed to expect rather than discover:** `GOALLESS-5` ends 0–0, so **no goal
> beat fires** and the anchor is exercised through `MomUp`/`MomDown` and `NeutralLine` only. That
> satisfies condition 1 — those tables interpolate the anchor — but it means **frame A will not show
> a `ScoreUp`/`ScoreDown` club-free line.** If the read wants one, the seed must score.
