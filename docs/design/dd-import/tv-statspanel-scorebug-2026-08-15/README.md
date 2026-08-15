# T99 — the stats panel over a NON-LEVEL scorebug · 2026-08-15

**Ruling:** `T99`, batch 79 — *"the stats panel may cover the scorebug band FOR AS LONG AS TIME IS
FROZEN WHILE IT IS OPEN."* **The freeze is the licence; the GOALS row is not.**
**Built at:** the commit this set is docked with. **Harness:** `Capture_StatsPanel_OverANonLevelScorebug`.

**NO READ IS OFFERED.** The four checks are pre-committed at the DD seat.

---

## The binding condition — met, and asserted rather than hoped for

> **`LOOPHOLES 0 — MIDDLEMEN 1` at `16'`.** Non-level, one goal, **not 0–0.**

The harness **waits** for a revealed non-level score with at least one goal and **fails the run** if it
never arrives — *"this is a RE-SEED, never a reason to shoot anyway."* A stats panel over a goalless
scorebug proves nothing, because the covered band carries no information and no reading of it can
fail. The condition is read from the **revealed ledger**, never the locked stat line.

## What was shot — three bursts, because check 4 is a COMPARISON

**Frame-contiguous (interval 0).** `Time.captureDeltaTime` ties sim time to *rendered* frames, so a
burst spaced in realtime advances the match by however many frames the host rendered — four passing
captures of the wrong beat in this lane. It matters doubly here: **the set's whole claim is that time
is stopped**, so a set that let the match move between frames would argue against itself.

| burst | frames | clock | score |
|---|---|---|---|
| `statspanel-closed-before` | 20 | **16 → 18** (running) | `LOOPHOLES 0 — MIDDLEMEN 1` |
| `statspanel-open` | 30 | **18, and only 18** | `LOOPHOLES 0 — MIDDLEMEN 1` |
| `statspanel-closed-after` | 20 | **18 → 19** (resumes) | `LOOPHOLES 0 — MIDDLEMEN 1` |

**That middle row is the ruling, visible.** Thirty contiguous frames, one clock value. And the third
row is the other half: the clock **resumes from 18′, the minute it stopped on** — stopped and
continued, with no catch-up.

**Docked: 6 frames of 70** (181.5 MB whole; the spend goes where the difference lives, not sampled
flat) — the band before, the frame immediately before opening, the overlay's **first / middle / last**
so the 30-frame hold is visible rather than asserted, and the return. **`MANIFEST-all-70-frames.txt`
carries every frame's score, clock and strip text**, which is what answers checks 2 and 4 numerically
without a second instrument.

## THE FIRST RUN OF THIS CAPTURE FAILED, and that is why the set exists

The first shoot passed its harness and **its own per-frame log disproved its claim**: the score held
while **the minute ticked `18' → 21'` behind the panel.**

`TickClock` advanced on `Time.deltaTime`, and the `!_seated` guard above it is what actually froze the
clock on stand-up — **two expressions of one rule, agreeing by convention.** So when §8.8's panel
added a *third* freeze condition, the clock never got it.

> **A covered fact that CAN move is LOST** — the exact case T99's licence does not reach. The clock now
> reads `SeatedDeltaTime`, the single authority, so a future freeze condition reaches it by
> construction.

**The pin could not have caught this.** It asserted `SeatedDeltaTime`, and `SeatedDeltaTime` was
correct — *a channel that never reads the authority is invisible to a pin on the authority.* **The
frames caught it.** The harness now asserts the clock across the open burst too, so it cannot recur
silently.

## The event-strip question, answered before the shoot

**No — the event strip is not covered.** The panel spans `y 0 → bottomY`; the whole **bottom row**
begins at `bottomY` — `CashOut` left, `EventStrip` right — so the panel stops exactly where the strip
starts. The DD's concern was the right one and does not arise: *a held statement is not static even
when the clock is*, so the freeze argument would not have covered it. Pinned as **non-overlap against
the live rects**, not as remembered constants.

## THE FOUR CHECKS — pre-committed, and what to read them on

1. **A deliberate overlay, not a panel that overshot its zone** — `open__frame000`, `frame014`,
   `frame029`.
2. **The covered band shows no FRAGMENT of the scorebug** — the same three. *A half-covered scorebug
   is worse than a fully covered one.*
3. **The GOALS row does not read as the scoreline** — `open__frame014`.
4. **On close the scorebug returns unchanged** — `closed-before__frame019` against
   `closed-after__frame000`, and the manifest for all 70.

## NOT CLAIMED

- **No read is offered on any of the four.**
- **The panel's own composition is not claimed** — the DD has not seen it, and batch 79 explicitly did
  not rule it.
- **The key is `TAB` and is UNRATIFIED** — this seat's pick, flagged as T88 flagged `ENTER`.
- **One seed, one ticket, one leg.** The panel is shot over a moneyline leg, so **corners and cards
  carry the unrevealed mark** — that is the ruled behaviour off a count leg, not a defect, but this set
  shows no populated count row.
- **Fit is not asserted** for the panel's own strings; they have not been swept under C46.
