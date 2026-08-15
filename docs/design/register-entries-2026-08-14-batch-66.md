# Register entries — 2026-08-14, batch 66

**THE 0–0 FRAMES, AND THE LAPTOP'S BAND.** Ruled at the DD seat (seated fresh 2026-08-14) against
`dd-import/tv-goalless-draw-2026-08-14/` — 120 frames, both tickets, one settlement — and against
`artifacts/surething-ui/…-16-margin-max-legs-staged-receipt-flat-1024x704.png`.

**Destination tables: TV — match theater** (`T87-am` ruled · `T96`, `T97` new · `T65` corrected) ·
**SureThing — the laptop** (`S51` CLOSED · `S75` new).

**Rows shipped:** `T87-am` · `T96` · `T97` · `T65` · `S51` · `S75`.

---

## T87-am — THE THREE PRE-COMMITTED DISPOSITIONS, RULED ON THE FRAMES

**Disposition (1) fires on the beat. It does not close the row** — and separating those two is the
whole of this entry. **T87 stands unamended.** Two defects are on the frames, and **both live in
mechanisms T87 explicitly leans on**, which is exactly where a narrowing hides.

### (1) The ending is legible as a resolution — YES

The standing check is T87's own: *is the stated ending legible as a resolution* — **not is it loud.**
Read off the set:

| | draw-backer (`scene001`, WON) | team-backer (`scene002`, LOST) |
|---|---|---|
| strip at the whistle | `LEG 1 — WON` from frame 000 | `LEG 1 — DEAD` from frame 031 |
| room | **+7.64 mean lift across 76.7% of room pixels**, onset f016, peak f017, back to rest ~f037 | **no added light at any frame**; dims and holds |
| slot | gold flood, `+$` tallying, confetti from f037 | nothing |

**Neither half can be mistaken for idle.** The one state the surface must never be mistaken for is
the one state it is not. **The check passes, and it passes on frames rather than on the mechanism's
promise.**

### (2) The ending reads as the surface idling — DOES NOT FIRE. Its remedy is owed anyway.

The trigger is absent (above). **But the remedy T87 pre-committed is owed for a different reason,
and this is the non-obvious half of the read.**

**§6.8 assigns the event strip the job of stating the fact. Across 120 frames it never does.**
`scene001` carries `LEG 1 — WON` in that slot for all 60 frames; `scene002` carries a goal line for
31 and then `LEG 1 — DEAD` for 29. **The strip goes straight to the leg's GRADE and never states the
MATCH's ending.**

**The ending is still legible, and the reason is worth writing down**: `FT` is in the scorebug's
clock slot and the scoreline holds `0 — 0` on every one of the 120 frames. **The fact is on the
surface — it is simply not on the tier T87 assigned it.** The ending is currently carried by the
settlement machinery and the bug alone.

**RULED: the strip's full-time statement is OWED at its L2 tier (T66), as T87 already assigns it —
never a flourish** (T35/T40 closed that on mechanism, and nothing here reopens it). This is the
pre-committed remedy, arriving on the pre-committed path, for a cause the pre-commitment did not
anticipate.

### (3) The draw-backer's settlement is muted relative to a goal-won leg — DOES NOT FIRE

**Structurally impossible, and the frames agree.** `WinBeat()` (`TvSweatScreen.cs:3343`) is one path
for every winning ticket: `EmissionFlash(goldL4)`, `RoomSettlementGlow()`, `PunchThenSettle`,
`WinConfetti()`, and a tally read off `_ticket.PotentialPayout`. **Nothing in it reads a market kind,
a goal count or a scoreline.** There is no branch that could mute a goalless win, and the frames show
every element firing.

**T65's glow fires on the goalless settlement — confirmed on frames, first time.** That was the half
of T87 most exposed to a quiet narrowing, and it is now evidence rather than inference.

### The set's own limit, stated because it bounds the above

`winTallyDuration`/`winConfettiDuration` are 2.0s; the capture is 1.2 sim-seconds. At frame 059 the
slot reads **`+$63` against `PAYS $86`** — the tally is ~73% through. **The set ends mid-tally**, so
the resting figure and the confetti's settle are **not in evidence**. Not a defect; a stated limit,
and the reason no claim here is made about how the beat RESTS.

---

## T96 — THE DRAW LEG PRINTS AS A TEAM BET. The worst thing on the frames.

**On the frames, at full time, on the same matchup:**

```
TICKET 1/2    MIDDLEMEN ML    +243    W        <- this is the DRAW ticket
TICKET 2/2    MIDDLEMEN ML    +132    L        <- this is the Middlemen ticket
```

**Two tickets, the same label, opposite grades.** The winning row tells the player he backed
Middlemen to win, on a match Middlemen did not win, and marks it WON.

**The grading is CORRECT** — the harness places `MarketSelection.MoneylineDraw()`, the draw won and
the team lost, and C47 holds exactly as ruled. **The defect is purely presentational, which is why it
survived a passing capture.**

**Cause, named:** `TvSweatScreen.LegStatement()` case `MarketKind.Moneyline` is a two-way —
`pickedHome ? Matchup.Home.Name : Matchup.Away.Name`, then `$"{club} ML"`. **There is no third
branch**, so a `MarketChoice.Draw` selection falls into the binary and is named after whichever club
the bool lands on.

**This is not a new ruling. It is an unimplemented one.** `tv-design.md` §8 already carries it, and
**it names the failing string verbatim**:

> The draw's forms are authored and live with the rest (S74): **NEED** `LEVEL AT FULL TIME`,
> **progress** `LEVEL` / `NOT LEVEL`, **compact** `DRAW`. … **The moneyline's other forms assume a
> team (`MIDDLEMEN ML`) and a draw leg has none, which is why it needed its own pair rather than
> inheriting.**

**RULED: the compact statement for a draw leg is `DRAW`, as already authored.** The Moneyline branch
takes a Draw case; the binary is the bug. **Nothing is designed here** — the deck has the word, the
surface never got it.

**Severity, stated plainly because T87 predicted this exact person:** the player this misprints on is
**the one whose ticket just came in.** T87's own line is *"the worst outcome available here is a
surface that conflates no goal with no result and drains the one player whose ticket just came in."*
**This is a nearer miss than that — his result is not absent, it is attributed to a bet he did not
place.**

---

## T97 — THE STRIP CARRIES A GOAL INTO A GOALLESS FULL TIME. C50's shape.

**`scene002`, frames 000–030 — 31 of 60, more than half the ending:**

```
        MALLARDS  0 — 0  MIDDLEMEN  •                    FT
        Mallards on the board; the slip flinches.
```

**Both lines are in the same frame.** The scorebug reads `0 — 0` at `FT` on all 60 frames of that
scene (one distinct state, measured); the strip beneath it asserts a goal.

**Cause, named:** the line is `game-console/EventText.cs:111`, a member of the **`ScoreDown` array —
the opponent-scored family.** It is a goal line by construction, and it is on screen at a full time
with no goals.

**This is C50's shape** — a slot asserting a beat that did not occur — and it is the same class the
capture's own README records four deleted runs for. **The harness cannot catch it: it asserts
plumbing, and both the score and the clock it does assert are correct.**

**It also breaches §8 directly.** The strip is *"one authored line explaining the latest move"* and
*"never duplicates the score."* **This does not duplicate the score — it contradicts it**, which §8
did not need to forbid because nothing had done it before.

**RULED: a defect, and it is the strip's own.** The remedy is not new copy: **the strip must not
carry a goal line into a state with no goal.** Whether the line is stale carry-over or freshly
selected is the lead's diagnosis to make and report — **both causes are defects and neither changes
the ruling.**

**T96 and T97 are one repair in practice.** T97's fix and the L2 full-time statement owed under
T87-am (2) land in the same slot; **rule them together or the strip gets touched twice.**

---

## T65 — THE OWED SETTLEMENT CAPTURE ARRIVED. It does NOT close the hue item.

**The open-items row reads *"Upper bound — owed a settlement capture."* The capture is here, and the
row stays open** — because a full-room capture is the wrong instrument, which is worth one paragraph
now rather than a second wasted window.

**Measured on `scene001`, room pixels only, against frame 015:**

| frame | added light, room | hue | lifted |
|---|---|---|---|
| 017 (peak) | +7.64 mean | **24.9°** | 76.7% |
| 020 | +6.21 | 31.4° | 70.9% |
| 025 | +4.01 | 44.2° | 58.0% |
| 030 | +2.08 | 60.4° | 36.8% |

**The authored value is `roomSettlementWarm` = (0.818, 1.000, 0.610) — hue 88.0° — at intensity
0.9**, and the code's own note fixes the acceptance band at **85–92°**.

**The measured hue cannot be compared to it, because the hue tracks DISTANCE FROM THE PANEL:**

| region, at f017 | hue | | at f025 | hue |
|---|---|---|---|---|
| near-TV top strip | 19.1° | | near-TV | 36.6° |
| far top edge | 24.8° | | far edge | 46.4° |
| far bottom-right corner | 31.5° | | far corner | 69.6° |

**A single light cannot cast three hues at three distances.** `EmissionFlash(goldL4)` fires on the
same frame as `RoomSettlementGlow()` in `WinBeat()`, and **its spill dominates near the panel and
falls off with distance — that gradient IS the confound.** The room reading is a mixture, not the
settlement light.

**RULED: T65's hue stays OPEN and is closed by the instrument that isolates it** — V6's printed hue
and the R23 `RoomViewCapture` path (`RoomViewCapture.cs:1918` already reads `tv.roomSettlementWarm`
directly), **never by another in-room settlement capture, however good.** The open-items row is
corrected accordingly.

**What this set DOES settle for T65: the glow fires on a goalless settlement, at a measurable
magnitude and with a clean decay to rest.** That is the T87 half, and it is closed.

---

## S51 — CLOSED. The owner is identified, the quantity decomposes exactly, and it is FIXED not re-signed.

**S51's signed deviation carried its own expiry: *when the owner of the 2.6px is identified — at
which point it is FIXED, not re-signed.* The condition is met.**

**The lane's geometry correction is right and this seat confirms it independently.** The earlier
acquittal of the wax highlight computed the band's **height** term (24px × sin 0.5° = 0.21px). **The
rotation pivots at the top-left, so the term that matters scales with WIDTH**, not height: for a
rect rotated θ about its top-left, the lowest corner descends by `w·sin θ` to first order. **The
acquittal was arithmetically wrong, and the highlight was never exonerated.**

**Verified from the layout literals, not from the lane's report:**

- payout figure — `MakeText(…, pos (14, y), size 300×36, 31px cond)` → **box bottom at `y − 36`**
- wax highlight — `MakePanel(…, pos (11, y − 34), size w × 6)` → **band bottom at `y − 40`**
- **structural drop = 4.00px, exactly.** Tilt = `w · sin(0.5°)`, `w = payout.preferredWidth + 8`.
- **4.563px ↔ a 56.5px figure. 4.748px ↔ a 77.7px one.** `RunDirector.seed` is blank in `Room.unity`,
  so every boot prices a different board. **The pin was a function of how much money was on screen.**
  **It was never a constant and could never have held one.**

### RULED — move the pixels. Move the BAND, not the payout block. It is a KIT-FIDELITY defect.

**The 4px is not a composition question at all.** The kit component the code cites places the band
**inside** the figure:

```jsx
// PayoutFigure.jsx — "a 6px amber band … behind the figure"
position:"absolute", left:"-3px", right:"-5px", bottom:"-2px", height:"var(--wax-highlight-h)"
```

- **Horizontally Unity already matches the kit exactly** — `−3` left, `+5` right overshoot. ✔
- **Vertically it does not.** Kit line box = `--st-size-payout` 31px × `--st-lh-fig` 1.1 = **34.1px**;
  `bottom:-2px` puts the band's bottom at **36.1px** below the figure's top. Unity puts it at
  **40px**. **The gap is 3.9px — and the measured structural overrun is 4.00px.**

**The entire structural overrun IS the kit gap.** Place the band where `PayoutFigure.jsx` places it
and the band's bottom lands flush with the figure's own box instead of 4px past it — **the overrun
closes at zero with no payout block moved, no reservation slackened, and no element excluded.**

**The frame corroborates it independently of the arithmetic:** on the MaxLegs staged receipt the band
reads as a **detached rule sitting below `$530`**, not as a highlighter behind it. **The code comment
says "behind the one loud figure." It is not behind it.** One cause, two symptoms — the measurement
and the look — and one fix.

### The two other options, refused

**(c) "a decorative underline is not flow content" — REFUSED.** S51's ruling forbade exclusion
because it *"would have gone green while the real overrun continued"*; the owner being known does
lapse that reason, so it is refused on two fresh grounds instead. **First, wax is money (S3) and this
is the money** — `PAYS` is the only wax on the receipt, the band marks the loudest figure on the
surface by intent, and it keeps its highlight even at `$0` because *the highlight marks the slot*.
**A mark this surface rules as meaning is not chrome.** **Second, the descent is UNBOUNDED in the
money string's width** — excluding it deletes the only instrument that would notice it growing, which
is precisely the drift that already happened silently (4.563 → 4.748, supplied by the draws).

**(a) "lift the payout block" — REFUSED.** It treats the symptom, cascades through the `y -= 40f`
cursor chain, and **bakes the kit gap in permanently** by building the layout around it.

### Why this is worth pixels now rather than later — the argument that carries

The reserved region is `PlaceBandY 110 + PlaceBandH 44 + 6 = 160`. **The overhang does not collide
with the PLACE band today — it eats the 6px pad above it**, leaving ~1.25px at the widest observed
string. **But `4.00 + 0.0087·w > 6` at `w > 229px`, and money never abbreviates (C49).** Same-game
parlays are in flight and produce larger payouts and wider strings. **This is a latent collision that
the work currently in the lane is actively pushing toward the boundary.** Fixing it while it is a
4px band placement is cheap; fixing it after it lands on the action stack is T47's whole problem.

**Also RULED: never shrink the figure to fit** — standing, restated only because it is the tempting
move and it stays refused.

### The test pin

The lane's interim repair — structural derived and pinned two-sided, tilt bounded rather than pinned
— **is correct as an interim and should land to re-green main.** It is **superseded by the production
fix**: once the band is placed per the kit, the pin becomes *the flow's lowest element is at or above
the budget*, the two-sided equality retires, and **S51 closes rather than being re-sourced a third
time.** Sequence it that way or the pin gets re-sourced twice.

---

## S75 — A HAND-LAID MARK RESERVES WITH THE FIGURE IT MARKS

**Generalised from S51 so the next mark does not repeat it.** A decorative mark that belongs to a
figure — highlight, underline, rub-out, ring — **is flow content: it is measured with its figure, it
is never excluded from a reservation, and its own extent is what must clear the boundary, not the
type's box.**

**Where the mark is transformed, the reserved extent is the TRANSFORMED extent** — a rotation about a
corner descends by `w·sin θ`, and **that is a width term, which is the trap S51 spent a fortnight
in.**

**Bound it at design time, never at runtime.** A mark sized from measured text makes the reservation
a function of content, which §2 forbids (a zone resizing to content). **Sweep the population (C46),
take the widest renderable money string, and pin the clearance as a constant** — a fixed grid
constant re-derived once at design time is explicitly legal; a zone that moves with the string is
not.

---

**Nothing in this batch blocks the markets lane, and nothing in it touches the engine.** T96 and T97
are surface repairs in `tv-sweat`; S51/S75 are a band placement and a test in `surething-ui`.
