# READ — the drawn ending, measured on the docked set

**Written:** Design Director seat, 2026-08-19 · **Batch 122**
**Mandate:** `docs/5-orchestration/dd-mandate-2026-08-18.md` Phase 2
**Set:** `docs/design/dd-import/tv-goalless-draw-2026-08-14/` — 128 frames, seed `GOALLESS-5`,
`Atlanta Middlemen 0 – 0 Scranton Mallards`, both tickets on one settlement. **No capture window was
used for this read.**
**Binds:** `tv-design.md` §6.8 (Design-verified batch 70), `T87-am`, `T87-am2`, `T96`, `T97`,
`T97-am`, `T98`, `T70`, `T108`.

**Status: a READ, not a spec.** Two of the four bet types are unshot and the phase cannot be spec'd
whole until they are. What is settled here is **what the drawn ending IS**, with numbers instead of
adjectives — which is the thing the mandate's framing (*"one authored line at the whistle"*) asserted
without measuring.

---

## 1. METHOD — and why no window was needed

Profiled numerically before any frame was opened: file-size deltas to find structure, then
frame-to-frame pixel diffs at ¼ sampling to localise transitions, then **five frames read**. Every
figure below is measured over all 120 ending frames.

**`C11` is respected in both directions.** Where a claim is about how something *reads*, it is made
on a frame. Where it is about how much something *moves*, it is made on a measurement — and the
measurements are stated so the lane can reproduce them.

---

## 2. THE ARC, MEASURED

| | draw-backer — **WINS** | team-backer — **LOSES** |
|---|---|---|
| **f000–050** (1.02s) | `LEVEL AT FULL TIME` / `LEVEL` · strip `THE MATCH ENDS LEVEL` · room **35.08** | `MIDDLEMEN TO WIN` / `LEVEL 0–0` · strip `THE MATCH ENDS LEVEL` · room **35.08** |
| **f051** | leg row → `DRAW +243 W` · strip → `LEG 1 — WON` | leg row → `MIDDLEMEN ML +132 L`, dimmed onto a raised band |
| **f052** | *nothing* | **the room light goes out — luminance 35.08 → 28.47** |
| **f053–059** | *nothing* | room creeping back, 28.47 → 28.96 |

**Frame-to-frame change during the hold: 170–260 changed pixels of 230,400 sampled — 0.1%.**
Forty-five consecutive frames within a tenth of a percent of still.

**The ending is two states and one cut.** There is no arc to review. The mandate's framing is
confirmed and sharpened: it is not *one line at the whistle* — it is **one line for 85% of the
ending, then a substitution.**

---

## 3. THE FINDING — the two players watch the same broadcast

Winner's frame against loser's frame, **every index 000–050**, ¼ sampled, threshold 12/255:

| zone | changed pixels across all 51 frames |
|---|---|
| scorebug | **0** |
| event strip | **0** |
| foot ledger | **0** |
| room surround | **0** |
| ticket column | ~1,300–1,600 per frame (its two lines) |
| pitch | ~4,600 per frame (the same match at its own drift) |

**Outside the ticket's own two lines, the man who just won and the man who just lost are looking at
a pixel-identical broadcast for 51 consecutive frames.**

Everything the *theatre* says — the scoreline, the closing line, the ledger, the room — is the same.
The entire difference between winning and losing a drawn match, for 85% of its ending, is two lines
of small type in the left rail.

**This is not `§6.8` being violated. It is `§6.8`'s own stated worst case, arriving.** That section
names it exactly:

> *"the worst outcome available here is a surface that conflates **no goal** with **no result** and
> drains the one player whose ticket just came in."*

---

## 4. THE ROOM'S ONLY GESTURE IN 120 FRAMES IS ON THE LOSS

Room-surround mean luminance, all 60 frames of each ending:

| | range | movement |
|---|---|---|
| draw-backer (**wins**) | 35.07 – 35.09 | **0.02 of 255 — nothing** |
| team-backer (**loses**) | 35.08 → **28.47** at f052 | **−6.61**, green channel −8.1 |

**Within this evidence set, the room responds to the loss and not to the win.** That inverts §6.8's
own sentence — *"a draw is quiet for the room and LOUD for one ticket."*

### 4.1 And the reason is almost certainly the hold — which makes it a WINDOW problem, not a regression

**I am not claiming the settlement glow is gone.** §6.8's verification records it at **+7.64 mean
lift across 76.7% of room pixels, onset f016, peak f017** — and **that measurement was taken on the
batch-68 set, which this set replaced and which the README says was *overwritten in place*.**

Batch 69 then ruled the minimum hold, and batch 70's build wrote `THE MATCH ENDS LEVEL` directly in
`FinalSlam`, **held for `drawnEndingHoldDuration` (1.0f) before the grade beats run.** The grade now
lands at f051 = 1.02s of a **1.2s capture**.

**So the hold pushed the entire win sequence to the last 0.18 seconds of the window.** The payout
slot confirms it: **`RISK $25 · PAYS $86` is unchanged through frame 059** — where the superseded set
had the tally mid-run at `+$63`. On this set **the tally has not started.**

**The set does not lie. It has been outrun by a ruling.** `C36`'s shape — a control that brackets the
beginning cannot see the middle — arriving on a capture window instead of a control pair.

**Consequence, and it is the reason the phase needs a window at all:** §6.8 is Design-verified, and
its central reassurance — the winning draw-backer gets a full settlement — **is currently backed by
frames that no longer exist.** Nothing is wrong; nothing can be checked either.

---

## 5. THE HOLD'S ONLY MOTION IS A MATCH THAT HAS ENDED

Accumulated frame-to-frame change over frames 010–040, ¼ sampled: **the entire motion bbox is
y 0.28–0.68, x 0.43–0.87 of the frame, with 90% of the mass in y 0.33–0.64, x 0.46–0.85.** That is
the pitch, and nothing else moves — no text, no chrome, no room.

**For one second at full time, on a screen that says `FT` and `THE MATCH ENDS LEVEL`, the only
moving thing is the players still playing.**

Recorded as a finding, not a ruling: whether the territory view should hold, settle or clear at the
whistle is a design call this read does not make, and it is the single largest unclaimed surface in
the ending.

---

## 6. THREE DEFECTS THE SET CARRIES

### 6.1 `T70`'s standing check was never run on the draw — and it fails

`T70` ruled, on `G1`'s pair: **"requirement above, state below, no term repeated across the pair"**,
and made it a **standing check for any new market.**

The draw-backer's pair is **`LEVEL AT FULL TIME` over `LEVEL`.** `LEVEL` is repeated across the pair.

**The draw is a new market** (`S74`, 2026-08-12), so the standing check applied to it and was not
run. §6.8's batch-70 verification records this exact pair **as evidence of a pass**, because it was
checking `T96`'s live-NEED clause and not `T70`'s.

**And there is a second half.** §6.8 refused `FULL TIME — LEVEL` for the strip on the explicit ground
that *"the scorebug prints `FT` one slot above, and stating the same fact twice one slot apart is
§8's duplication rule."* **`LEVEL AT FULL TIME` prints `FULL TIME` in the same frame as that same
`FT`.** The refusal was applied to the strip and not to the NEED beside it.

**On one screen at one instant: `0 — 0`, `FT`, `LEVEL AT FULL TIME`, `LEVEL`, `THE MATCH ENDS LEVEL`
— the same fact, five times.**

### 6.2 `T108`'s defect is here too, and the set is a ready-made BEFORE

At `FT`, on a settled 0–0, for 51 frames: **`MIDDLEMEN TO WIN`** — a requirement that can no longer
be met — and at f059, on a ticket that has **won**, **`RISK $25`** still prints.

`T108` ruled both: *NEED is the requirement WHILE LIVE*, and *`RISK` → `STAKE`*.

**Not reported as unfixed.** `T108` was ruled batch 100 (2026-08-16); this set was shot 2026-08-15.
**It is a before-set, and it is valuable as one** — `T108`'s fix has been verified on the corners
material and **not on a drawn ending**, where the NEED sits at full time for a full second rather
than passing through.

**Whether the trigger fires correctly here is a live question and it is not answered by this read.**
`T108` clause (3) keys the form to the **revealed** state, and during f000–050 the screen has already
shown `0 — 0`, `FT` and `THE MATCH ENDS LEVEL` — the facts that decided the leg. Whether
`RevealedLegState` agrees with the screen's own words at that moment is the lane's diagnosis.
**Either answer produces the same ruling**, exactly as `T97` handled stale-carry versus fresh
selection.

### 6.3 What is NOT a defect, recorded so it is not re-raised

- **The line yielding to the grade at f051** is `T87-am2` working as ruled: *holds until the leg's
  grade displaces it*, with the batch-69 minimum. Confirmed, not faulted.
- **No goal sentence in 128 frames** — `T97-am` holds.
- **`— LEAD CHANGE`** is closed at `T98`; the 8 mid-match frames are its before.
- **The draw leg reads `DRAW`** — `T96` holds.

---

## 7. WHERE THE ARC ACTUALLY HAS ROOM — and it is not where the mandate's words point

§6.8 bans both obvious moves: **manufacturing a climax** is celebration (`T35`, `T40`), and
**rendering nothing** reads as a bug. The mandate asks for *"a full ending arc as a first-class
broadcast moment."* **Those pull against each other, and that tension is the phase.**

**The measurement says where the room is, and it is neither of the places the argument has been
happening.**

- **Not the climax.** Banned, and `T87-am` established the win path is already full-treatment and
  goal-independent — one `WinBeat()` for every winning ticket, reading no market kind.
- **Not the words.** One authored line is correct and correctly rationed; §6.8's reasoning for a
  single L2 statement is sound and `T87-am2` derived its form properly.
- **THE HOLD.** One second — 85% of the ending — currently carrying one static line, one moving
  pitch of a finished match, and **a broadcast that is pixel-identical for the winner and the
  loser.** It is already the longest deliberate pause this surface takes. **It is dead time that
  was ruled into existence for a good reason and has never been designed.**

**The recommendation, for Allen, stated as a direction and not a treatment:** the ending arc is built
by giving the hold *structure*, not by giving the resolution *volume*. That is the only move
available that does not reopen `T35`/`T40`, and it is the one the frames argue for.

**I am not proposing a treatment here.** `§6.8` is Design-verified and the phase's own evidence is
incomplete; a treatment authored now would be authored against a window that cannot show its own
subject.

---

## 8. THE WINDOW — sized, not guessed

Three gaps, and the third is the one nobody knew about.

| # | what | why |
|---|---|---|
| 1 | **the existing window is too short** | the hold consumes 1.02 of 1.2 sim-seconds; the win's tally, flood and room glow all fall outside it. **Re-shoot both existing endings at 150 frames (3.0s)** — hold 1.0 + resolution + the 2.0s tally `T87-am` measured. Same seed, same matchup, same stake, so it is directly comparable to the docked set |
| 2 | **count legs settling level** | a goalless draw settles a whole family the set has never carried: `UNDER 1.5 / 2.5 / 3.5 GOALS` all win, `BTTS — NO` wins, `TOTAL GOALS EVEN` wins on zero. **None has ever been shot at its ending.** One ticket carrying an under leg and a BTTS-NO leg covers it |
| 3 | **correct score `0-0`** | new territory — `CorrectScore` had no reachable home until `S95`, so **no capture of any kind exists.** The longest price on the board settling on the quietest possible match is the phase's extreme case |

**Binding conditions, pre-committed before the frames exist** (`T89`/`T99`/`S74-am2`'s precedent, and
`C41` respected — every criterion is a **direction of travel or a binary**, never a number to land
on):

1. **Same seed, same matchup, same stake** on the re-shoot, or it is not comparable to the docked
   set and the whole point is lost.
2. **`C55`: the subject must be IN FRAME.** For the correct-score arm the subject is a specific
   string; pin or force the matchup rather than dealing for it.
3. **Frame-contiguous**, per the harness's existing convention — the fourth failure in the README's
   own list was realtime spacing, and it produced frames labelled with a beat they did not show.
4. **The room band is captured**, not cropped away. It is the channel that carries §6.8's central
   claim and this read could only measure it because the docked frames happen to include it.
5. **Every ending runs past its own tally**, verified by the payout slot changing and then settling —
   a window that ends mid-tally cannot answer whether the ending resolves.

**Not requested, deliberately:** a second seed, a 1–1 or 2–2 draw, or any variation arm. §6.8 rules
this is **the drawn match's line, not the goalless one**, so a non-goalless draw is a real question —
**but it is a question about generality, and generality is not what is missing.** What is missing is
the ending's own second half.

---

## 9. NOT CLAIMED

- **No claim that the settlement glow regressed.** §4.1 says the opposite: it is out of the window,
  and the window is what needs fixing. A regression claim needs the re-shoot.
- **No claim about how the hold READS.** That it does not *move* is measured; whether it reads as
  gravity or as a hang is a `C11` claim and this seat has been corrected by frames four times in a
  week. It waits for the re-shoot.
- **No treatment proposed** (§7), and no amendment to `§6.8` — everything found is either a repair to
  a mechanism §6.8 relies on, or a gap in evidence.
- **Nothing about the laptop's MY BETS** on a drawn settlement, which is `S88`'s territory and still
  owed its own capture.
