# EVIDENCE DOCK — THE A-REVEAL AND THE DECISIVE POOL (spec §2 and §3.5)

**Shot:** tv-theater lane, 2026-08-18 · **Build:** phase A `acd9d9f` · phase B `4a06b52` · phase C
reveal `d10a6f2` + the decisive pool (this commit)
**Seed:** `CORNERS-SWEAT-1` · `OVER 8.5 CORNERS` — **the same seed and line as the original
before-state**, so this reads directly against `dd-import/corners-sweat-2026-08-16`.
**176 frames, 20 windows.** Frames UNTRACKED; this README commits.

---

## 1. §2 — THE REVEAL, ON A FRAME

```
scene006 · moment-score01-reveal · clock 22' · score 'REGULATORS 0 — SPREADSHEETS 1'
```

**A goal renders on the corners arm's scorebug at 22 minutes** — on a ticket that does not ride on
goals.

**The before-state held `0 — 0` until `90'+1`, then delivered the result in two steps at the death**
(`count-sweat-read` §5: the corners player was shown a goalless match, for 86% of a watch, on a match
that finished 5–1).

**In-suite corroboration:** `[SCORE-REVEAL-GATE] matchGoals=2 revealedBeforeFullTime=True`.

### What the reveal did NOT change, deliberately

**The scene and the strip stay ticket-keyed.** At `score01-reveal` the grammar token is `CornerFor`
and the strip reads `Regulators settle in; the drift runs the other way.` — **the scorebug moved and
nothing else did.** §2 carves out the SCORE and names the panel, player detail and the strip as
continuing to follow the ticket; this is that, literally.

---

## 2. §3.5 — THE DECISIVE POOL, AND IT LANDS ON THE EXACT MINUTES THE DEFECT WAS MEASURED

| event | clock | count | distance | strip line |
|---|---|---|---|---|
| corner 1 | 1' | 2 | 7 | `Regulators pass it around, slow and mean.` |
| corner 2 | 11' | 4 | 5 | `Regulators settle in; the drift runs the other way.` |
| corner 3 | 30' | 6 | 3 | `Spreadsheets squeezing the half.` |
| **corner 4** | **43'** | **8** | **1 — THE APPROACH** | **`one short. the ledger is holding its breath.`** |
| **corner 5** | **53'** | **10** | **crossed — THE TURN** | **`that clears it. the line is beaten.`** |
| corner 6 | 56' | 11 | decided | `Regulators keeping the ball.` |
| corner 7 | 68' | 12 | decided | `Spreadsheets pin them deep — passes and patience.` |

**The measured defect, verbatim from `grammar-count-markets` §2:** *the approach (43') printed the
line from corner #1 — the least consequential event of the match — verbatim, and the crossing (53'),
the moment the bet was won, printed the line from corner #2.*

**Same seed. Same clock times. 43' and 53' now carry their own authored copy, and no ordinary corner
can reach it.** Disjointness is asserted as a set property in EditMode (`the decisive pool is
disjoint from the ordinary count-event pool`), so recycling onto a decisive beat is
**unconstructible rather than unlikely** — `T108` clause 1's standard, on copy instead of on a form.

**The count still reaches 12** — 2·4·6·8·10·11·12, identical to the before-state. §4's binding holds
under both new clauses: `[COUNT-COMMIT] 11 = 11`.

---

## 3. TWO PAIRINGS ROUTED — and they should be RULED TOGETHER, not separately

**This build answers neither. Both are one edit wide and both are marked in the code.**

### Pairing 1 — does a count-leg goal reach the SCENE, and does it reach the STRIP?

Built conservatively: **neither.** The scorebug moves alone.

- §2's letter supports that — it carves out the score and names the strip as ticket-keyed.
- §2's own compounding argument cuts the other way: *"a goal the corners player does not need is
  exactly the departure from calm his watch is missing."* A reveal that moves only the scorebug
  supplies less contour than that sentence implies.
- A build dispatch proposed letting goal WORDS stand when a quiet goal commits; **the lead reverted
  it** as making the strip follow the MATCH, and because the state-lie argument is weaker than it
  looks — `NoGoalLine` *selects a line that does not assert a goal*, and silence about a goal is not
  a contradiction of one. Its reasoning is preserved verbatim at the site.

### Pairing 2 — a goal riding a SHOWING COUNT SCENE vs one riding a QUIETED BEAT

**This build treats them identically** — both commit the score silently. They may not deserve to be.

- On a **showing count scene**, an event already carries the watch; a silent scorebug move sits
  underneath something.
- On a **quieted beat**, nothing else is carrying attention — and this is exactly where §2's
  "departure from calm" bites hardest. A goal arriving into calm is the contour the corners watch was
  missing.

**Both sites exist and are labelled** (`countSceneQuietGoal` on the count-scene path; `quietGoal` on
the fall-through). Ruling them apart is a two-line change; ruling them together is a one-line change.

---

## 4. AUTHORED BUT UNREACHABLE — stated so absence is not read as coverage

**Two of §3.5's four cells cannot fire in this build.** `APPROACH · UNDER` and `TURN · UNDER` are
authored exactly as the strings doc wrote them, but `gateEligible` hard-requires `countHelps`
(Over), so an under leg is never classified `Approach`/`Turn` — **spec §6 keeps the under mirror out
of the gate's scope.** They are in the pool, in the sweep, and unreachable until that mirror is
gated. **Nothing here is evidence that they read correctly.**

## 5. WHAT THIS SET DOES NOT CLAIM

- **Nothing about whether the watch is BETTER.** §7 is blind to it and so is this dock. The contour
  exists and the two decisive moments now carry their own words; whether they READ is a `C11`
  judgement at the acceptance view.
- **`C46` is NOT discharged for the new strings.** The four lines and their fallback rungs were added
  to the `Flavor` slot's enumerated pool but **the sweep has not been re-run since**. They are
  enumerated, not yet measured.
- **Duration is recorded, not compared.** This capture reports **35.04s** on its own instrument.
  **Do not set that against the probe's 39.84s or the read's 41.42s/35.40s table** — §8.1's own care
  applies: those are different measures on different baselines and must not share a sentence.
- **The `(2 in the spell)` suffix is gone from the two decisive lines** by construction (it is not
  appended to a decisive beat) but **survives on ordinary count beats**. `T110-am2` ruled it removed
  outright; that is queued and this set predates it.
- **No flat-frame or seated-view claim** (§1.3).
