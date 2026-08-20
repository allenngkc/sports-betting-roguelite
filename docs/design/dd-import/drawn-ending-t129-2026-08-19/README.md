# EVIDENCE DOCK — `T129`, THE DRAWN ENDING'S SECOND HALF (three arms)

**Shot:** tv-theater lane, 2026-08-19 · **Seed `GOALLESS-5`, matchup 0, stake 25.0 — all three arms**
**608 frames.** Frames UNTRACKED; this README commits.

| arm | subject | frames |
|---|---|---|
| **1** | both goalless endings re-shot at 150 | 308 (8 live-need + 150 + 150) |
| **2** | count legs settling level — `UNDER 1.5 GOALS` + `BTTS — NO` | 150 |
| **3** | **correct score `0-0`** — no capture of this market has ever existed | 150 |

---

## THE FIVE BINDING CONDITIONS — met, and how

- **(a) same seed, matchup, stake on the re-shoot.** Arm 1's entry point changed **only** the frame
  count, 60 → 150. All three arms share `GOALLESS-5` / matchup 0 / stake 25.0, so they are readable
  against each other as well as against the docked set.
- **(b) `C55`, subject in frame; pin-or-force the correct-score string.** Arm 3 locates the `0-0`
  cell explicitly off the board (`ScoreHome == 0 && ScoreAway == 0`) and fails as a **re-seed** if
  the board does not price it. Not dealt for.
- **(c) frame-contiguous.** Every burst is `CaptureBurst(..., 150, 0f)`. No interval anywhere.
- **(d) the room band captured, not cropped.** Every burst uses `Camera.main`, the seated in-room
  camera.
- **(e) runs past its own tally.** Verified per frame from the payout slot, below — **not assumed
  from the frame count.**

---

## 1. THE GAP `T125` MEASURED IS CONFIRMED — the old window ended BEFORE the tally began

**Arm 1, draw-backer ending, payout slot per frame:**

```
f001–f067   (no tally)
f068        +$1        ← THE TALLY STARTS
f127        +$86       ← last change, then SETTLES to f150
```

**The old window was 60 frames. The tally starts at f068.** So the docked set could not have shown
the tally **at all** — not a fraction of it, none of it. `T125`'s measurement is confirmed on frames
and the re-shoot was necessary rather than precautionary.

**Arm 3 closes condition (e) most cleanly:** the tally runs `+$4` (f068) → **`+$256` (f127)**, and
`+$256` is exactly the ticket's own `PAYS $256`. **The payout slot changes and then settles inside
the window** — the ending resolves, and the frames say so.

---

## 2. `T128`'s CARRIED QUESTION — ANSWERED, and the answer is the same in all three arms

`T128` asked whether `RevealedLegState` agrees with the screen's own words at full time, and said
**either answer produces the same ruling.**

**It does not agree, and the window is 51 frames — identical across all three arms:**

| arm | f001 – f051 | f052 → |
|---|---|---|
| 1 · draw-backer | `RISK $25` · need `LEVEL AT FULL TIME` · chip `''` | **`STAKE $25` · need cleared · chip `W`** |
| 2 · under + BTTS-No | `RISK $25` · need `UNDER 1.5 GOALS` · prog `0 GOALS • LIMIT 1` · chip `''` | `RISK $25` · need cleared · chip `W` |
| 3 · correct score | `RISK $25` · (no progress line) · chip `''` | **`STAKE $25`** · chip `W` |

**From f001 the screen already reads `0 — 0`, `FT` and `THE MATCH ENDS LEVEL`** — the facts that
decided every one of these legs. **For 51 frames (1.02 sim-seconds) the ticket column still prints a
live requirement and a live risk beside them.**

**`T108`'s fix WORKS on a drawn ending** — the flip happens, at f052 — **but it lands one full second
after the screen's own words have already settled the leg.** `T108` clause (3) keys the form to the
**revealed** state; these frames show the revealed *scoreline*, *clock* and *ending line* arriving a
second ahead of the revealed *leg state*.

**This is the drawn ending's own version of the defect `T108` was written for**, and it is why
`T128` asked for it here: on the corners material the stale form passes through in flight, and on a
drawn ending **it sits still at full time where the player is looking.**

---

## 3. ARM 2's OWN FINDING — the multi-leg case behaves differently, and correctly

**Arm 2's footer never reaches `STAKE`.** It stays `RISK $25` through all 150 frames, and the
cash-out slot still carries a live **offer** (`CASH OUT $67` at f088) at full time on a settled 0–0.

**That is `T108` clause 2 working exactly as ruled** — `RISK` is a **TICKET** word, and this is the
only two-leg ticket in the set. The footer may not flip while any leg remains unrevealed, and leg 1
has not been revealed inside this window.

**So it is correct and it is also the multi-leg form of `T128`:** the match is over, both legs will
win, and the surface still offers to buy the ticket back. **Named, not ruled** — whether a cash-out
offer may stand at full time on a settled match is a design call this lane does not hold.

---

## 4. NEW TERRITORY — the correct-score arm

**`CorrectScore` had no reachable home until `S95`, so nothing of this market has ever been
photographed.** Arm 3 is the first.

`PAYS $256` on a `$25` stake — **the longest price on the board settling on the quietest possible
match**, and the phase's stated extreme case. The leg carries **no progress line at all** (the market
has no running quantity), so at full time the column is a statement and a price and nothing else.

---

## WHAT THIS SET DOES NOT CLAIM

- **Nothing about whether the ending READS.** `T127` recorded that the hold's only motion is the
  pitch — *"for one second at full time, on a screen reading `FT` and `THE MATCH ENDS LEVEL`, the
  only moving thing is the players still playing"* — and explicitly did **not** rule whether the
  territory view should hold, settle or clear. **These frames are the material for that call and do
  not make it.**
- **One seed, one matchup, one stake.** Shared by all three arms by design — that is the set's
  strength as a comparison and its limit as a sample.
- **No 1–1 or 2–2 arm**, deliberately, per `T129`: §6.8 rules this the DRAWN match's line rather than
  the goalless one, so a non-goalless draw is a real question — **but it is a question about
  GENERALITY, and generality is not what was missing.**
- **Nothing about cards.**
- **No flat-frame or acceptance-view claim** (§1.3) — these are the seated in-room camera.
