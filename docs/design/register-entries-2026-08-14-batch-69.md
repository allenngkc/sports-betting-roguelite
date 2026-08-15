# Register entries — 2026-08-14, batch 69

**THE ACCEPTANCE LIST, APPLIED TO THE DOCKED SET — one of three passes** — and the laptop's refusal
stamp sizing. Read at the DD seat against the reshot
`dd-import/tv-goalless-draw-2026-08-14/` (frames stamped 23:06–23:07) and TV's tree at `1be8140`.

**Rows shipped:** `T96` **VERIFIED** · `T97` **FAILS ON FRAMES** · `T87-am2` **FAILS ON FRAMES,
and the ruling was under-specified — that half is mine** · `S77` (stamp sizing).

---

## 1. T96 — VERIFIED. Design-verified, with one clause not verifiable from this set.

| check | result |
|---|---|
| draw ticket's row reads `DRAW` | **PASS** — `TICKET 1/2 · DRAW · +243 · W` |
| team ticket still reads `MIDDLEMEN ML` (**no regression on the Home/Away branch**) | **PASS** — `TICKET 2/2 · MIDDLEMEN ML · +132 · L` |
| the two rows no longer print one string with opposite grades | **PASS** |
| draw-backer's settlement still at full treatment (**no regression**) | **PASS** — gold flood, tally `+$3` → `+$63`, confetti, room glow |

**NOT VERIFIABLE FROM THIS SET, and the README asserts it anyway:** the live **NEED** `LEVEL AT FULL
TIME` over progress `LEVEL`. **Both tickets are settled in all 120 frames**, so the live row is never
in shot. **This is a supplemental shot** — any mid-match frame of a draw-backed leg carries it. It is
not a defect; it is a claim without a frame (C11).

---

## 2. T97 and T87-am2 — BOTH FAIL ON THE FRAMES, and the build is not the excuse

**The strip is byte-for-byte unchanged in behaviour from the pre-ruling set:**

| | pre-ruling set | reshot set |
|---|---|---|
| `scene001` strip | 1 state: `LEG 1 — WON`, all 60 frames | **1 state: `LEG 1 — WON`, all 60 frames** |
| `scene002` strip | 2 states at frames **[0, 31]** — the goal line, then `LEG 1 — DEAD` | **2 states at frames [0, 31] — the same goal line, then `LEG 1 — DEAD`** |

**`Mallards on the board; the slip flinches.` is still on screen over a `0 — 0 · FT` scorebug for 31
of 60 frames. `THE MATCH ENDS LEVEL` appears in none of the 120.**

### The build is not the excuse, and this is the important part

**Both fixes are present and correctly written** — `TvSweatScreen.cs:2803` sets the line on
`LegFinal && _ledger.Picked == _ledger.Opponent`; `:1528-1531` guards `Score` and `BigPlay`, excludes
`NearMiss`, gates on `!spec.Goal.HasValue`, and routes to `NoGoalLine`, which keeps the danger-only
`BigPlay` members and otherwise falls to `NeutralLine`. **That is the ruling, implemented as ruled.**

**And the build that shot these frames contained it.** `DRAW`, `THE MATCH ENDS LEVEL` and
`BigUpAssertsGoal` **all entered in one commit, `1be8140`** — and **`DRAW` renders on the frames.**
**So the strip code was compiled and live and still did not reach the screen.** A stale build would
have failed all three; this failed exactly the two that share a slot.

### Diagnosis — offered as a strong hypothesis, not a frame claim

**`_pendingFlavor` and `_tFlavor` have several writers, and the authored ones are not last.**

- **The line:** computed into `flavor` (`:2804`), stashed to `_pendingFlavor` (`:2821`), landed by
  `RevealBeatChrome` (`:2818`). **But the leg's grade writes `_tFlavor.text` directly** at `:3268` /
  `:3304`. On a `LegFinal` beat both fire, and **the frames say the grade wins.**
- **The goal line:** the guard writes `_pendingFlavor` at `:1531`; `RenderEvent` writes
  `_pendingFlavor = flavor` **unguarded** at `:2821`. **If `RenderEvent` runs last, it restores the
  goal sentence** — which is exactly what the frames show.

**One diagnostic settles it:** log every write to `_tFlavor.text` and `_pendingFlavor`, with its call
site, across one `LegFinal` beat. **This seat cannot execute the code and does not claim the ordering
as fact** — the frames are the fact; this is where to look.

**Batch 68 said *rule them together or the strip gets touched twice.* The strip was touched twice, in
two places, and the later writer won.**

---

## 3. T87-am2 — MY RULING WAS UNDER-SPECIFIED. The missing clause, supplied.

**This half is the DD seat's error and it is stated plainly rather than routed.**

Batch 68 ruled the line *"holds until the leg's own grade displaces it."* **That assumed a window
exists. On a won draw-backer it does not** — `scene001` shows `LEG 1 — WON` already up at **frame
000, the whistle itself.** **The match ending and the leg's resolution are the same instant**, so a
line that yields to the grade yields before it is ever seen. **The ruling described a sequence the
beat model does not provide.**

### RULED — the drawn match's line takes a MINIMUM HOLD, and the grade may not land inside it

**Two statements share one slot and the ruling must say which yields and for how long.** It does now:

- **`THE MATCH ENDS LEVEL` holds for a stated minimum before the leg's grade may displace it.**
- **The grade must never land on the same frame as the line.** A statement replaced on its own
  entrance frame was never made.
- **The hold is a NAMED serialized constant, not a literal**, and takes the surface's existing
  authored-statement hold as its precedent — **`ticketDeadConsolationDuration = 1.0f`**, which is the
  same kind of thing: a statement the player must read before the beat moves on. **Match it rather
  than invent a number.**
- **There is room.** `scene002` already shows **31 frames — 0.62 sim-seconds — of dead window** between
  the whistle and its leg grade. The hold makes explicit a gap that already exists by accident.

**It is verifiable on frames, which is the point of stating it this way: the line must be VISIBLE in
the capture, for multiple frames, BEFORE the grade appears.** The next set passes or fails on that
without any further reading.

---

## 4. S77 — THE REFUSAL STAMP OVERFLOWS. Ruled with the margin coupling in view.

**The lane asked for sizing, not copy, and the coupling is why the answer is neither.**

### The box, and the coupling, measured

The Blocked stamp lives inside the PLACE control: **`296 × 44` at 17px**, `StampReason` at `.04em`
(`SportsbookApp.cs:1103`), against §2.2's ruled **`--st-place-h 44 / min 200`**. And:

```
ActionBandReservedHeight = PlaceBandY 110 + PlaceBandH 44 + 6 = 160
MarginFlowBudget         = 530 − 160 = 370
```

**Every pixel of control height comes 1:1 out of the flow budget** — and **S51 has just shown that
budget is already overhung**, with the wax band eating the 6px pad. **A copy problem must not be paid
for out of a geometry budget that is already over.** That is the coupling, and it decides the ruling.

### What may not yield

**`≥13px` does not yield** — the product-fact floor is a cross-surface law. **`cause AND remedy` does
not yield** — S73-am4; cause-only was the original defect. **Truncation does not yield** — batch 67:
*a truncated remedy is an unverified remedy.* **Shrinking type does not yield** — §8, standing.

### RULED — the stamp states the ACT and its ARITY; the legs are MARKED in the flow, never named in the stamp

**The overflow is caused by leg NAMES, and the names do not belong there.** Up to three names inside a
296px box is unbounded in the worst case; **the instruction is not.**

- **The stamp carries the act and how many** — *these three cannot all land · drop them* in the
  authored forms — **and the legs it refers to are MARKED on their own rows in the flow directly
  above.**
- **This is T69/T70's principle, one control over: the subject is already on screen, so do not
  reprint it.** Batch 67 required the remedy to name legs *by their row's exact string* precisely so
  he would not have to translate — **marking the rows serves that goal better**, because the referent
  is not merely worded identically, it is pointed at.
- **The check that makes it safe, and it passes:** the flow is bounded by **MaxLegs = 4** in a 370px
  region and does not scroll, **so every marked row is on screen whenever the stamp is.** A mark that
  could scroll out of view would fail this and the ruling would not stand.
- **The surface already has the mark vocabulary** — biro ring, oxide, the `RUB OUT` control at 60×32
  on each row. **Nothing new is invented.**
- **The control does NOT grow.** The margin budget is untouched and S51's overhang is not made worse.

### What this does to the measurement the lane is bringing

**It collapses the population.** The stamp's strings become **a handful of authored forms keyed by
arity (two, three)** instead of 645 name-bearing compositions. **Measure those forms against
`296 × 44` at 17px** — a trivial sweep — and bring the arity distribution across the 645 separately,
which is what actually needs measuring.

**If the authored forms still miss**, the order is: **(1)** a shorter authored form; **(2)** two lines
inside the existing 44px box at ≥13px — **44px carries two 13px lines with leading and this is a real
option, not a last resort**; **(3)** only then geometry, and that comes to Allen with the flow-budget
cost stated, because it is S51's budget.

**Wiring stays behind the lane's flag until the measured forms come back.**

---

**Routing:** T97 and T87-am2 return to **tv-sweat** with the write-order diagnostic and the new hold
clause — **one supplemental shot covers both, plus the live-NEED frame T96 still owes.** S77 goes to
**surething-ui**.
