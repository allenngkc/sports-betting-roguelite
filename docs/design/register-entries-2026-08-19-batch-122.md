# Register entries — batch 122 (2026-08-19)

**THE DRAWN ENDING, MEASURED — AND THE WINNER AND THE LOSER ARE WATCHING THE SAME BROADCAST.**
Read on the docked 128-frame set, **no capture window used**. Across all 51 frames of the ruled hold,
winner's screen against loser's screen: **scorebug 0 changed pixels, event strip 0, foot ledger 0,
room surround 0.** The entire difference between winning and losing a drawn match, for 85% of its
ending, is **two lines of small type in the left rail.**

**Six rows.** **Destination table:** TV (`T124`–`T129`).

**Read:** `docs/design/drawn-ending-read-2026-08-19.md`.
**Set:** `docs/design/dd-import/tv-goalless-draw-2026-08-14/`.

---

## Three things a reader should take from this batch

**1. `§6.8`'s own worst case is what the frames show.** That section names it in terms — *"the worst
outcome available here is a surface that conflates no goal with no result and drains the one player
whose ticket just came in."* **Nothing is violated; the thing it warned about is measurable.**

**2. The set has been outrun by a ruling, and that is why the phase needs a window.** Batch 69's
minimum hold pushed the entire win sequence to the last 0.18 seconds of a 1.2-second capture.
**`§6.8`'s central reassurance — the `+7.64` room lift — was measured on the batch-68 set, which was
overwritten in place.** It is not refuted. **It is no longer checkable**, and the payout slot proves
the window is the reason: `PAYS $86` is unchanged through frame 059, where the superseded set had the
tally mid-run.

**3. The arc's material is the HOLD, not the climax.** `§6.8` bans a manufactured climax (`T35`,
`T40`) and bans rendering nothing. **The one second between them has never been designed** — one
static line, a pitch still playing a finished match, and an identical screen for both outcomes.

---

## The rows

| T124 | The drawn ending's two halves are PIXEL-IDENTICAL outside the ticket column — for 51 of 60 frames | **RULED — the phase's founding measurement** · DD 2026-08-19 batch 122, on `dd-import/tv-goalless-draw-2026-08-14/`. **Winner's frame against loser's frame at EVERY index 000–050, ¼ sampled, threshold 12/255: scorebug 0 changed px, event strip 0, foot ledger 0, room surround 0** — not "small", **zero, across all 51 frames.** The only differences are the ticket column's own two lines (~1,300–1,600 px) and the pitch (~4,600 px, the same match at its own drift). **Everything the THEATRE says — the scoreline, the closing line, the ledger, the room — is the same for the man who just won and the man who just lost.** **AND THE ENDING IS TWO STATES AND ONE CUT, not an arc: frame-to-frame change during the hold is 170–260 of 230,400 sampled pixels — 0.1% — for forty-five consecutive frames**, then everything changes at f051, then nothing again. **THE MANDATE'S FRAMING IS CONFIRMED AND SHARPENED: it is not *one authored line at the whistle*, it is ONE LINE FOR 85% OF THE ENDING AND THEN A SUBSTITUTION.** **NOT A VIOLATION OF `§6.8` — IT IS `§6.8`'s OWN STATED WORST CASE ARRIVING**, and that section names it in terms: *"the worst outcome available here is a surface that conflates NO GOAL with NO RESULT and drains the one player whose ticket just came in."* Recorded as the measurement the phase is built on rather than as a defect, because **what to do about it is a design question and `§6.8` is Design-verified** | batch 122 · `drawn-ending-read-2026-08-19.md` §3 |
| T125 | The room's only gesture in 120 frames is on the LOSS — and the win's is OUTSIDE THE WINDOW | **RULED — a WINDOW defect, NOT a regression, and the distinction is the whole row** · DD 2026-08-19 batch 122. **Room-surround mean luminance across all 60 frames of each ending: draw-backer (WINS) 35.07–35.09 — a range of 0.02 of 255, NOTHING; team-backer (LOSES) 35.08 holding to f051 then −6.61 to 28.47 at f052, green channel −8.1, creeping back to 28.96 by f059.** **Within this evidence set the room responds to the loss and not to the win, which inverts `§6.8`'s own sentence — *"a draw is quiet for the room and LOUD for one ticket."*** **I AM NOT CLAIMING THE SETTLEMENT GLOW REGRESSED, AND THE ARITHMETIC SAYS WHY.** `§6.8`'s verification records it at **+7.64 mean lift across 76.7% of room pixels, onset f016, peak f017** — **measured on the batch-68 set, which this set REPLACED and which the README records as *overwritten in place*.** Batch 69 then ruled the minimum hold; batch 70's build holds `THE MATCH ENDS LEVEL` for `drawnEndingHoldDuration` (1.0f) **before the grade beats run**, so the grade lands at f051 = **1.02s of a 1.2s capture** and **the whole win sequence is pushed into the last 0.18 seconds.** **CONFIRMED AT THE PAYOUT SLOT: `RISK $25 · PAYS $86` is UNCHANGED THROUGH FRAME 059, where `T87-am` recorded the superseded set reading `+$63` mid-tally. On this set the tally has not started.** **THE SET DOES NOT LIE — IT HAS BEEN OUTRUN BY A RULING.** `C36`'s shape (a control that brackets only the beginning cannot see the middle) arriving on a **capture window** instead of a control pair. **THE CONSEQUENCE IS WHY THE PHASE NEEDS A WINDOW AT ALL: `§6.8` is Design-verified and its central reassurance is now backed by frames that no longer exist.** Nothing is wrong; nothing can be checked either | batch 122 · §4 |
| T126 | `T70`'s standing check never ran on the draw — and the draw's pair FAILS it | **RULED — VIOLATION** · DD 2026-08-19 batch 122, read on frames. **`T70` ruled a STANDING CHECK FOR ANY NEW MARKET: *requirement above, state below, NO TERM REPEATED ACROSS THE PAIR*.** **The draw-backer's pair is `LEVEL AT FULL TIME` over `LEVEL`. `LEVEL` is repeated across the pair.** **The draw IS a new market (`S74`, 2026-08-12), so the check applied and was not run** — and `§6.8`'s batch-70 verification records this exact pair **as evidence of a PASS**, because it was checking `T96`'s live-NEED clause and nobody was holding `T70` at the time. **AND THERE IS A SECOND HALF, WHICH IS `§6.8` REFUSING A FORM AND THEN PRINTING IT ONE COLUMN OVER.** That section refused `FULL TIME — LEVEL` for the strip on the explicit ground that *"the scorebug prints `FT` one slot above, and stating the same fact twice one slot apart is §8's duplication rule with a different neighbour."* **`LEVEL AT FULL TIME` prints `FULL TIME` in the same frame as that same `FT`.** The refusal was reasoned correctly and applied to one of the two slots. **THE COUNT, ON ONE SCREEN AT ONE INSTANT: `0 — 0`, `FT`, `LEVEL AT FULL TIME`, `LEVEL`, `THE MATCH ENDS LEVEL` — THE SAME FACT, FIVE TIMES.** `T69`/`T70`'s family at a scale neither row anticipated. **The re-authoring is the DD seat's under `G1` and is not ruled here** — it is authored against the measured column with the re-shoot's frames, because a pair re-written blind is how this one arrived | batch 122 · §6.1 |
| T127 | The hold's only motion is a match that has already ended | **RECORDED — finding, not ruled** · DD 2026-08-19 batch 122. Accumulated frame-to-frame change over frames 010–040, ¼ sampled: **the entire motion bbox is y 0.28–0.68, x 0.43–0.87 of the frame, 90% of the mass in y 0.33–0.64, x 0.46–0.85 — the pitch, and nothing else.** No text moves, no chrome moves, the room does not move. **For one second at full time, on a screen reading `FT` and `THE MATCH ENDS LEVEL`, the only moving thing is the players still playing.** **NOT RULED, deliberately: whether the territory view should hold, settle or clear at the whistle is a design call, and it is the single largest unclaimed surface in the ending.** Recorded now because it is measured on frames that exist, and because **it is the material `T129`'s window has to show** — a hold whose only motion contradicts its own scorebug is either the ending's problem or its opportunity, and one second of frames at the right length will say which | batch 122 · §5 |
| T128 | `T108`'s defect is on the drawn ending too — and this set is a ready-made BEFORE | **RECORDED — a second instance and a before-set; NOT reported as unfixed** · DD 2026-08-19 batch 122. At `FT` on a settled 0–0, for 51 frames: **`MIDDLEMEN TO WIN`** — a requirement that can no longer be met — and at f059, **on a ticket that has WON, `RISK $25` still prints.** `T108` ruled both (*NEED is the requirement WHILE LIVE*; *`RISK` → `STAKE`*). **THE DATES CLEAR THE SET: `T108` was ruled batch 100 on 2026-08-16 and these frames were shot 2026-08-15.** It is a before, and **it is valuable as one — `T108`'s fix has been verified on the corners material and NOT on a drawn ending, where the NEED sits at full time for a full second rather than passing through.** **THE LIVE QUESTION, and this read does not answer it: `T108` clause (3) keys the form to the REVEALED state, and during f000–050 the screen has ALREADY shown `0 — 0`, `FT` and `THE MATCH ENDS LEVEL` — the facts that decided the leg.** Whether `RevealedLegState` agrees with the screen's own words at that moment is the lane's diagnosis. **Either answer produces the same ruling**, exactly as `T97` handled stale-carry against fresh selection. **Carried into `T129`'s window as an assertion rather than left to be noticed** | batch 122 · §6.2 |
| T129 | The window — sized on the measurement, with the third gap nobody knew about | **REQUESTED, sized, and PRE-COMMITTED before the frames exist** · DD 2026-08-19 batch 122. **THREE ARMS. (1) RE-SHOOT BOTH EXISTING ENDINGS AT 150 FRAMES (3.0 sim-seconds) — the gap `T125` found: the hold consumes 1.02 of the current 1.2s and the win's tally, flood and room glow all fall outside it. Same seed, same matchup, same stake as the docked set or it is not comparable and the point is lost.** **(2) COUNT LEGS SETTLING LEVEL — a goalless draw settles a whole family the set has never carried: `UNDER 1.5 / 2.5 / 3.5 GOALS` all win, `BTTS — NO` wins, `TOTAL GOALS EVEN` wins on zero. None has been shot at its ending. One ticket carrying an under leg and a BTTS-NO leg covers it.** **(3) CORRECT SCORE `0-0` — new territory: `CorrectScore` had no reachable home until `S95`, so NO CAPTURE OF ANY KIND EXISTS. The longest price on the board settling on the quietest possible match is this phase's extreme case.** **BINDING CONDITIONS, written before the frames per `T89`/`T99`/`S74-am2`, with `C41` respected throughout — every criterion is a DIRECTION OF TRAVEL OR A BINARY, never a number to land on: (a) same seed, matchup and stake on the re-shoot; (b) `C55` — the subject IN FRAME, and for the correct-score arm the subject is a specific STRING, so pin or force the matchup rather than dealing for it; (c) frame-contiguous, the README's own fourth failure being realtime spacing that produced frames labelled with a beat they did not show; (d) THE ROOM BAND IS CAPTURED, NOT CROPPED — it carries `§6.8`'s central claim and this read could only measure it because the docked frames happen to include it; (e) every ending runs PAST its own tally, verified by the payout slot changing and then settling, because a window that ends mid-tally cannot answer whether the ending resolves.** **NOT REQUESTED, deliberately: a second seed, or a 1–1 / 2–2 arm. `§6.8` rules this is the DRAWN match's line and not the goalless one, so a non-goalless draw is a real question — but it is a question about GENERALITY, and generality is not what is missing. What is missing is the ending's own second half** | batch 122 · §8 |

---

## The direction, recorded but NOT ruled

`§6.8` bans both obvious moves — **manufacturing a climax** is celebration (`T35`, `T40`), and
**rendering nothing** reads as a bug. The mandate asks for *"a full ending arc as a first-class
broadcast moment."* **Those pull against each other and that tension is the phase.**

The measurement says where the room is, and it is neither place the argument has been happening:

- **not the climax** — banned, and `T87-am` established the win path is already full-treatment and
  goal-independent;
- **not the words** — one authored line is correct and correctly rationed;
- **THE HOLD** — one second, 85% of the ending, carrying one static line, a pitch still playing a
  finished match, and a broadcast identical for both outcomes. **The longest deliberate pause this
  surface takes, ruled into existence for a good reason, never designed.**

**The lean, on the record and not binding: the ending arc is built by giving the HOLD structure, not
by giving the RESOLUTION volume** — the only move available that does not reopen `T35`/`T40`.

**No treatment is authored and none should be**, until the window can show its own subject. A
treatment written now would be written against a capture that cannot contain it, which is the exact
error `T125` just recorded.

---

## What is NOT in this batch

- **No amendment to `§6.8`.** Everything found is a repair to a mechanism it relies on, or a gap in
  evidence. The section's reasoning stands.
- **No regression claim** on the settlement glow (`T125` §4.1).
- **No read of how the hold FEELS.** That it does not move is measured; whether it reads as gravity
  or as a hang is a `C11` claim and waits for the re-shoot.
- **Nothing on the laptop's MY BETS at a drawn settlement** — `S88`'s territory, still owed its own
  capture.
