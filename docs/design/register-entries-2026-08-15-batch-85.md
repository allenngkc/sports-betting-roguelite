# Register entries — 2026-08-15, batch 85

**T99 CLOSES — all four checks PASS, and the ruling's standing condition earned its keep within hours
of being written.** Read at the DD seat against `dd-import/tv-statspanel-scorebug-2026-08-15/`.

**Destination table: TV — match theater.** **Rows shipped:** `T99` **DESIGN-VERIFIED** ·
`T100` (the panel's composition — raised, not ruled) · `T101` (the `TAB` key, docketed).

---

## T99 — THE FOUR CHECKS

**Binding condition met and asserted rather than hoped for:** `LOOPHOLES 0 — MIDDLEMEN 1` at `16'`.
**Non-level, one goal, not 0–0** — and the harness **fails the run** rather than shooting anyway if
the score never arrives, which is the discipline the condition was written to force.

### 1 — A DELIBERATE OVERLAY, not a panel that overshot. **PASS.**

**The panel's bottom edge is a structural line, not a stopping point.** It spans everything above the
persistent bottom row and stops exactly where that row begins — `CASH OUT $105 / HOLD E` left, the
event strip right, the run meta beneath. **It covers the ticket column and the stage entirely; it
clips neither.**

**That is what distinguishes an overlay from an overshoot: the edge lands on a boundary the
composition already had.**

### 2 — NO FRAGMENT OF THE SCOREBUG. **PASS.**

**The band is wholly behind the panel.** No sliver, no partial digits, no half-covered score.
**A half-covered scorebug would have been worse than a fully covered one** — a sliced score is a fact
rendered unreadable where a hidden one is merely deferred — and none is present.

### 3 — THE GOALS ROW DOES NOT READ AS THE SCORELINE. **PASS.**

**The two constructions are not alike in any channel.**

| | form |
|---|---|
| the scorebug | `LOOPHOLES 0 — 1 MIDDLEMEN` — one line, uppercase, large, **team-score-em-dash-score-team** |
| the panel's row | `GOALS` left-labelled, **two values in two columns** under sentence-case team heads in their T2 side hues, **among `CORNERS` and `CARDS` in identical treatment** |

**It reads as the first row of a table, which is what it is.** A statistic sitting in its own
register among its siblings, not a result.

#### One thing checked and CLEARED, recorded so it is not re-raised

**The panel leads with `Middlemen`; the scorebug leads with `LOOPHOLES`. The same two teams in
opposite orders.** **Checked, and it is not a defect:** the panel's order matches the **ticket
column's** — `MIDDLEMEN TO WIN / LEADING 1–0`, the backed side first — which is this surface's
existing convention for *the ticket's view of the match*, while **the scorebug is the neutral
record.** **Both orders already coexist on the closed frame and were verified in earlier phases. The
panel introduces nothing new**, and the column heads are labelled and hued at the point of reading.

### 4 — THE SCOREBUG RETURNS UNCHANGED. **PASS, measured across all 70 frames.**

| burst | frames | clock | score | strip |
|---|---|---|---|---|
| closed-before | 20 | **16′ → 17′ (f6) → 18′ (f15)** | `LOOPHOLES 0 — MIDDLEMEN 1` | unchanged |
| **open** | **30** | **18′, and NO change across all thirty** | unchanged | unchanged |
| closed-after | 20 | **18′ → 19′ (f5)** | unchanged | unchanged |

**Thirty contiguous frames on one clock value, then a resume FROM 18′ — the minute it stopped on,
with no catch-up.** **Stopped and continued, not paused-and-caught-up**, which is the difference
between a freeze and a hidden advance.

> **The licence is satisfied on the frames: time is frozen while the panel is open, so the covered
> facts could not move. T99 CLOSES, DESIGN-VERIFIED.**

**One note on the instrument, not the design:** the manifest reports `suspended=False` throughout,
**including the frozen burst** — that field is the match's own suspension state and is *not* the
freeze's indicator; **the clock column is.** Recorded so a later reader does not take the manifest to
say the opposite of what it shows.

---

## 5. THE FIRST RUN FAILED, AND THAT IS THE MOST VALUABLE THING IN THIS SET

**Its own per-frame log disproved its own claim: the score held while the minute ticked `18′ → 21′`
behind the panel.**

**The mechanism, and it is the class this seat has ruled on three times today.** `TickClock` advanced
on `Time.deltaTime`, while the `!_seated` guard is what actually froze the clock on stand-up —
**two expressions of one rule, agreeing by convention.** So when the panel added a *third* freeze
condition, **the clock never received it.**

**And the lane's own formulation is better than anything this seat wrote for the same class:**

> **A channel that never reads the authority is invisible to a pin on the authority.**

**PROMOTED. That is why the pin could not catch it** — the pin asserted `SeatedDeltaTime`, and
`SeatedDeltaTime` was correct. **The frames caught it.** The clock now reads the single authority, so
a future freeze condition reaches it by construction, and the harness asserts the clock across the
open burst so it cannot recur silently.

### The standing condition earned its keep, and that is worth recording

Batch 79 ruled the licence **conditionally** — *the panel may cover the scorebug FOR AS LONG AS TIME
IS FROZEN; if the match ever runs behind this panel, the scorebug must survive* — and wrote it as a
standing condition **because the danger was a later change that looks unrelated.**

**The first run was that exact case, hours later, and the condition is what made it a failure rather
than a shipped frame.** **A ruling written as *approved* rather than as *approved while X holds*
would have passed it.**

### And the event-strip question was answered before the shoot, correctly

**The strip is NOT covered.** The panel spans `y 0 → bottomY`; the bottom row begins at `bottomY`, so
the panel stops exactly where the strip starts — **pinned as non-overlap against the live rects, not
as remembered constants.** **The concern does not arise, and the reason it was worth raising stands:
a held statement is not static even when the clock is, so the freeze argument would not have covered
it.**

---

## T100 — THE PANEL'S COMPOSITION. Raised, not ruled — and the condition for ruling it is named.

**This seat has now seen the panel, so it can say something; it has seen ONE state, so it will not
rule it.**

**The observation: the panel is largely empty.** Three rows — `GOALS`, `CORNERS`, `CARDS` — in a
surface occupying most of the screen, with a large unoccupied region beneath `CARDS`.

**And the state is why, at least in part: this is a moneyline leg, so corners and cards carry the
unrevealed mark (`—`).** **That is ruled behaviour off a count leg and not a defect** — TV named it
— **but it means two of the three rows are showing their empty form, and a panel judged on that state
would be judged on its thinnest possible content.**

**NOT RULED. OWED BEFORE IT IS: one frame with a POPULATED count row** — a corners or cards leg, so
the table is carrying real values in every row. **Then the composition is ruled on what it actually
holds rather than on what this seed happened not to reveal.** **Same discipline as the 0–0 condition
on this very capture: a surface shot in its emptiest state cannot be read for how it fills.**

---

## T101 — THE `TAB` KEY. Docketed, unratified, and the lane was right to flag it.

**TV flagged the panel's key as its own pick and unratified, *as T88 flagged `ENTER`*.** **Correctly
raised and correctly not assumed.** **ENTER is the studio commit key by standing ruling; a panel
toggle is a different act and takes its own word.** **Docketed to Allen with T88's precedent; nothing
blocks on it and the panel ships behind it unchanged.**

**Also owed and unclaimed: the panel's own strings have not been swept under C46.** **Named here so
the gap is known rather than assumed covered** — the same standard S84 was held to.

---

**Routing.** **T99 CLOSED, Design-verified.** **T100 → TV: one capture with a populated count row,
then the composition is ruled.** **T101 → Allen (the key), and the C46 sweep of the panel's strings
→ TV.** **The freeze's stand-up path has no capture and is named as unphotographed by the lane —
accepted, since the clock now reads one authority and the harness asserts it.**

**To Allen, in one line:** *the stats panel is verified — thirty frames on one clock value, then a
resume from the minute it stopped on — and the condition I attached to that ruling caught a real
defect within hours: the first shoot let the match tick on behind the panel, and because the licence
was written as "only while time is frozen" it failed instead of shipping.*
