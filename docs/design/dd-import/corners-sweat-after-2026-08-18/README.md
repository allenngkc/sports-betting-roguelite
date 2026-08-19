# EVIDENCE DOCK — the corners sweat, AFTER the distance gate (spec §8 item 1)

**Shot:** tv-theater lane, 2026-08-18 · **Build:** unit 3 phase A (`acd9d9f`) + phase B (`4a06b52`)
**Seed:** `CORNERS-SWEAT-1` · **Line:** `OVER 8.5 CORNERS` · **182 frames, 21 windows**
**Pairs against:** `dd-import/corners-sweat-2026-08-16` — **same seed, same fixture, same line, one
variable.** That pairing is the instrument.

**Docked against `spec-count-theater-2026-08-17.md` §7 and §8, as amended by batches 108–109**
(`T118`, `T115-am2`, `T117`). §8.1's six read criteria are answered in its own priority order below.

**Frames are UNTRACKED** per standing practice — this README and the harness commit; the roll does
not.

---

## THE HEADLINE, in one comparison

**Before:** `CornerFor` on scenes 002 through 015 — **fourteen consecutive windows, one token.**

**After:**

| event | scene | grammar | count | strip line |
|---|---|---|---|---|
| corner01 | 2 | **CalmPossession** | 2 | `Regulators pass it around, slow and mean.` |
| corner02 | 4 | **CalmPossession** | 4 | `Regulators settle in; the drift runs the other way.` |
| corner03 | 7 | `CornerFor` | 6 | `Spreadsheets squeezing the half.` |
| corner04 | 9 | `CornerFor` | **8 — THE APPROACH** | `corner kick won. another little number for the ledger. (2 in the spell)` |
| corner05 | 11 | `CornerFor` | **10 — THE TURN** | `the flag goes up; pressure becomes a corner. (2 in the spell)` |
| corner06 | 12 | **CalmPossession** | 11 | `Regulators keeping the ball.` |
| corner07 | 14 | **CalmPossession** | 12 | `Spreadsheets pin them deep — passes and patience.` |

**The arm departs from calm and returns to it.** `count-sweat-read` §2's *"seven departures from
nothing"* is now a contour.

---

## §8.1's READ CRITERIA, in its own priority order

### 1. THE COUNT STILL REACHES 12 — **PASS.** The only correctness check here.

The count runs **2 · 4 · 6 · 8 · 10 · 11 · 12**, identical to the before-set, ending `corners=5-7`.
**No batch was consumed without being committed.** §4's binding, on frames rather than inferred.

Corroborated in-suite by the phase A invariant on its own fixture:
`[COUNT-COMMIT] revealedTotal=11 matchTotal=11`.

### 2. The quiet events produce no count scene and no count strip line — **PASS on the strip, FOUR-OF-FIVE on the scene.**

- **Strip: five of five.** Events 1, 2, 3, 6, 7 all take neutral/possession lines. **No count strip
  line survives on a quieted event.**
- **Scene: four of five.** Events 1, 2, 6, 7 render `CalmPossession`. **`corner03` keeps its
  `CornerFor` scene** despite an `Ordinary` distance of 3.

**This divergence was predicted before the shutter, not discovered in the frames.** `T113` measured
beat 3 of this seed as a `Score`/`Swing` beat, and phase B's gate is **`Momentum`-only**, so event 3
is not gate-eligible. See "the open question" below — it is one line to change and it is not this
lane's call.

*(Criterion 2 as amended by `T118` reads "not no scene" — a quieted beat may legitimately be upgraded
to a goal scene. **In this build it never can:** phase B suppresses goal staging on a quieted beat, so
the upgrade path is unreachable. See `T118` below.)*

### 3. Events 4 and 5 visibly different from the other five — **PASS on the strip, PARTIAL on the scene.**

**On the strip the separation is total:** events 4 and 5 are the **only** two carrying a corner line;
the other five carry possession lines. The approach and the turn are the only narrated moments.

On the scene, `corner03` joins them for the reason above, so the visual separation is 3-of-7 rather
than 2-of-7.

### 4. The strip falls to its CALM REGISTER rather than holding or blanking (`T117`) — **PASS.**

Every quieted event takes a line from the existing neutral deck. **The strip neither holds nor goes
blank**, and the measured before-defect — *a 65' line still showing at 71'* — does not recur.

**Nothing was authored for this and nothing was built for it.** `T117` is satisfied **by
construction**: a quieted batch rides `QuietCount`, not `Count`, so `countScene` is false and
`TvSweatScreen.cs`'s existing `T97-am` override (`if (countLeg && !countScene)`) already routes the
strip to `SweatFlavor.NeutralLine`. **The resting state is not written; it is what remains** — which
is `T117`'s own claim, arriving through a law that predates it.

### 5. The scoreline reveals independently of the market — **N/A, §2 NOT BUILT.**

The score holds `REGULATORS 0 — SPREADSHEETS 0` across all 103 probe samples, unchanged from the
before-state. **That is the current rule working, not a defect**, and it is the measurement §2 exists
to change. §2 and §3 were built independent per the spec's instruction, so this set says nothing for
or against it.

### 6. Duration — **RECORDED, not judged. And the register's figure needs correcting.**

| | before | after | delta |
|---|---|---|---|
| rendered frames | 2,221 | **1,992** | −229 |
| sim-seconds | 44.42s | **39.84s** | **−4.58s (−10.3%)** |

**`T115-am2` predicted ~5.5s / ~12.5%. Measured: 4.58s / 10.3%** — same direction, **smaller than
budgeted.** Recorded because the register now carries the estimate.

**The window count did NOT change: 21 windows / 182 frames, before and after, identical.** §8.1
expected re-tiling and ruled the read onto the seven count events for that reason. The ruling was
right to make and the risk did not materialise — **the tiling shifted in ORDER** (`corner01` now
precedes `deadair01`; before it followed) **without changing the count.**

*(Per §8.1's own care: the −10.3% is against the probe's 44.42s baseline. It must not be combined in
one sentence with the read's 41.42s/35.40s sweep-duration table — different measures.)*

---

## `T118` — the second door, closed by construction rather than by a call site

The amendment names a second loss path: a quieted beat reaching probability reconciliation, staging a
**goal**, and paying off through `OnGoalPlayed` — so the batch is consumed and the count never
commits.

**Phase B closes it upstream:** a quieted beat stages no goal at all
(`goal = pendingQuietCount.HasValue ? null : ledger.StageBeatGoal(...)`), so the goal payoff is
unreachable and the count commits through a path independent of **both** callbacks.

**The two remedies are NOT interchangeable and the DD should know which shipped.** The amendment's
cheaper suggestion — *a call site rather than a new path* — closes the count loss but **leaves the
goal**, and a quieted corner would then reveal a goal on a corners ticket. That is the §2 coupling the
spec forbids §3 from acquiring.

---

## THE OPEN QUESTION THIS SET RAISES — one line, and not this lane's call

**Should the gate widen from `Momentum`-only to all beat types?**

- **For:** §3.1 keys treatment on **distance**, not on having arrived, and event 3's distance is 3 —
  `Ordinary`. Widening makes the scene stream match the strip stream (five quiet, five neutral) and
  matches criterion 2's own "five".
- **The original reason against is now spent.** The restriction was justified on `§2`-independence: a
  `Score` beat can stage a goal. **The goal-suppression added later makes that argument redundant** —
  a quieted beat of any type now stages none.
- **Against:** a `Score` beat is real probability drama (`|Δp| ≥ 0.07`), and `T113` framed the
  reclaimable set as the `Momentum` beats specifically.

**Built as `Momentum`-only and routed rather than decided.** `ApproachDistance` and the eligibility
test are adjacent and both one edit wide.

---

## WHAT THIS SET DOES NOT CLAIM

- **Nothing about whether the watch is BETTER.** §7 says no gate can speak to it and neither can this
  dock. The contour exists; whether it reads is a `C11` judgement at the acceptance view.
- **One seed, one line, one side, a comfortable winner.** §8 item 3's **near-line watch — a leg that
  lands close to its line, or loses — is still owed and is not here.** The ramp's whole value is in
  the case never shot.
- **Nothing about CARDS.** Out of scope by §6; no cards arm has ever been shot.
- **Nothing about the UNDER mirror.** Out of scope by §6, not in evidence.
- **The `(2 in the spell)` suffix is still present** on events 4 and 5. `T110-am2` ruled it removed;
  that is queued work not yet built, and this set predates it.
- **No flat-frame claim.** These are the harness's own captures at the capture camera, not the seated
  in-room acceptance view (§1.3).

## A NOTE ON HOW THIS SET WAS SHOT

The first attempt was **killed mid-run by a harness stop and took the editor with it**, leaving 158
frames and a stale lockfile at process count 0 — the known shutdown fault. That partial set reached
`score02-reveal` but **was missing `full-time` and `sweat-ends`**, so it was discarded rather than
docked: all seven count events were present, but a set that does not close is not evidence.

**Re-shot detached** per §4's own corollary — a run that may outlive one tool call must survive a
harness timeout — and polled on artifact mtime rather than process aliveness. The docked set is that
second, complete run: `Passed`, 182 frames, closing on `moment-sweat-ends`.
