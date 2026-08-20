# READ — the `T129` window, all three arms

**Written:** Design Director seat, 2026-08-19 · **Batch 127** · **the phase's gate**
**Set:** `dd-import/drawn-ending-t129-2026-08-19/` — 608 frames, seed `GOALLESS-5`, matchup 0,
stake 25.0, all three arms.
**Pre-commitment:** `drawn-ending-precommit-2026-08-19.md`, written before the frames existed.

**Every criterion below was written down before the shoot. Three held, one did not, and one thing
nobody wrote down is the largest finding in the set.**

---

## 0. TRANSPORT — the frames did not arrive with the README, and the convention says they never will

`main-2` received `README.md` and a `.gitignore` reading *"Frame sets stay UNTRACKED — README
commits, the rolls never do."* **The 608 PNGs are in the `tv-theater` worktree** and were read from
there directly.

**That worked only because both worktrees are on one machine.** `C12` requires frames **in the
import**, and the import carried a document. Recorded as a standing transport gap (`T138`), not as
this lane's fault — the lane followed the convention exactly and the convention has no vehicle.

---

## 1. PRE-COMMITTED CRITERIA, SETTLED

### §2.1 — the binary: does the room move on the WIN? **YES.**

Room-surround mean luminance, draw-backer, 150 frames:

| | |
|---|---|
| baseline f000–f067 | **35.08** |
| **f068** | **40.72 — peak, +5.65** |
| f080 | 37.0 |
| f090 → f149 | ~35.3, settled slightly above baseline |

**`T125` is confirmed as a WINDOW defect and closes.** `§6.8`'s central reassurance — the settlement
glow fires on a goalless settlement — is real and is now backed by frames that exist. **The old
60-frame window ended at f059, nine frames before anything happened.** The README's payout trace
agrees to the frame: the tally starts at **f068** and the room peaks at **f068**.

**My pre-committed expectation was YES and it held.**

### §2.2 — the direction: do the two endings differ in KIND? **YES.**

| | movement |
|---|---|
| draw-backer **wins** | **+5.65** (35.08 → 40.72) |
| team-backer **loses** | **−6.62** (35.08 → 28.46) |

Opposite directions, comparable magnitudes. **`§6.8`'s *"quiet for the room and LOUD for one
ticket"* is not falsified** — it was unobservable, and now it is observed.

### §2.3 — the identity interval: **I PREDICTED A SHRINK AND THERE IS NONE.**

Winner's frame against loser's frame, every index, first divergence per zone:

| zone | first differs |
|---|---|
| event strip | **f051** |
| scorebug · foot ledger · room | **f052** |

**Fifty-one frames, identical to the docked set.** §2.3 said *"the zero-difference interval SHRINKS
but does not vanish"* and committed to saying so plainly either way. **It did not shrink at all**, and
the reason is structural rather than surprising: the hold is the ruled 1.0 second and **nothing is
scheduled inside it**, so there was never anything there to arrive earlier. `T124` stands unamended.

### §3.1 — the `T123` / `T87-am2` collision: **MY LEAN HELD.**

`THE MATCH ENDS LEVEL` prints on the count ticket (arm 2, f010 and f050). **`T87-am2` governs the
MATCH's L2 statement; `T123` governs what a BEAT earns.** A corners or goals backer is still watching
a match that ended level, and the strip's job is to say what the score and clock cannot.

### §4.1 — arm 3: **CONFIRMED, BOTH HALVES, and worse on the frame than at source.**

Predicted from the two `default` arms; verified on `f040` against arm 1 and arm 2 at the same index.

---

## 2. THE THREE FINDINGS

### 2.1 `T130` — the correct-score column is EMPTY, and then prints a C# identifier

**At f040, on a `$25` ticket to win `$256`, the ticket column carries `TICKET 1/1` and nothing else.**
No NEED, no progress line, no statement, no price beside the leg. Measured against the other two arms
at the same frame index: arm 3's column holds **2.67%** ink against arm 1's **4.12%** and arm 2's
**4.32%** — and the missing third is exactly the statement block.

Side by side at f040:

| arm | the column says |
|---|---|
| 1 · draw-backer | `LEVEL AT FULL TIME` / `LEVEL` |
| 2 · under + BTTS-No | `UNDER 1.5 GOALS` / `0 GOALS • LIMIT 1` / `BTTS NO −119 NEXT` |
| **3 · correct score** | **— nothing —** |

**And at f055 the resolved row prints `CorrectScore`** — camel case, a C# enum identifier, **in the
money-amber of a won leg, at full weight, the most prominent text in the column.** The one string on
that screen which is not English is the one his win is announced with.

**`T130`'s `C17` flag is discharged and it is RULED.** The mechanism predicted at source is exactly
what the frame shows: `DescribeActiveLeg`'s `default:` returns empty strings, the call site at
`:2892` clears the compact form because *"the live row's NEED carries the statement"*, and the NEED
does not exist; `LegStatement`'s `default:` falls to `leg.DisplayLabel`, whose own `default:` is
`selection.Kind.ToString()`.

**This is nine market kinds, not one.** Arm 3 is the first of them ever photographed.

### 2.2 `T135` — THE MULTI-LEG REWIND: the ending is announced, and then the match plays again

**Nobody pre-committed this and it is the largest finding in the set.**

Arm 2 is the only ticket in the entire evidence corpus carrying **two legs on one match**
(`UNDER 1.5 GOALS` + `BTTS — NO`). Its clock:

| frame | f010 | f050 | f060 | f075 | f090 | f110 | f140 |
|---|---|---|---|---|---|---|---|
| clock | **FT** | **FT** | **FT** | **1'** | **2'** | **5'** | **9'** |

**Full time is announced. `THE MATCH ENDS LEVEL` holds for its ruled second. `LEG 1 — WON` lands.
And then the clock resets to `1'` and the same match is broadcast again**, in the present tense —
at f090 the strip reads *"Middlemen pin them deep — passes and patience."* while the resolved
`UNDER 1.5 GOALS +204 W` row sits above it.

**Isolated to the multi-leg case, and the other three endings prove it:** arm 1's two endings and
arm 3 all hold `FT` from f040 to f149 without exception. **The cause is the mechanism, not the
ending** — each leg gets its own sweat, so two legs on one fixture broadcast that fixture twice.

**Why it is this phase's defect rather than a general one, and it is the sharper half:** `T87-am2`
authored a **finality line** on the structural argument that *a drawn match ends on nothing, so the
last beat's line is stale by construction.* **A finality line followed by a rewind is worse than no
line at all** — the surface makes a claim and then unmakes it, where before it merely failed to make
one. `C50`'s shape at the largest available scale: not a slot asserting a beat that did not occur,
but **the whole surface asserting an end that then does not hold.**

**A two-legs-on-one-match ticket is an ordinary shape**, not an edge case — the engine's joint model
exists to price exactly it.

### 2.3 `T136` — the loss is acknowledged SIXTEEN FRAMES BEFORE the win

| | room departs baseline |
|---|---|
| team-backer **loses** | **f052** |
| draw-backer **wins** | **f068** |

**Sixteen frames — 0.32 sim-seconds — in which the losing screen has responded and the winning screen
has not.** Both players sat through the identical 51-frame hold; the one who lost is told first.

Not a violation of anything ruled: the loss reads off `ticketDeadConsolationDuration` at the grade,
and the win waits on the settlement sequence. **Recorded because `§6.8`'s stated worst case is
draining the player whose ticket just came in, and the order of arrival is a form of that nobody had
measured.**

---

## 3. WHAT `T128` ASKED, ANSWERED

`T128` carried the question *does `RevealedLegState` agree with the screen's own words at full time*,
and ruled that **either answer produces the same ruling.**

**It does not agree, and the interval is 51 frames in all three arms.** From f001 the screen reads
`0 — 0`, `FT` and `THE MATCH ENDS LEVEL` — the facts that decided every leg in the set — while the
column still prints a live requirement and a live risk. **`T108`'s fix WORKS on a drawn ending; it
lands one full second after the screen has settled the leg.**

On the corners material the stale form passes through in flight. **On a drawn ending it sits still,
at full time, where the player is looking.**

---

## 4. NOT CLAIMED

- **No read of whether the hold FEELS long.** `T127`'s question — should the territory view hold,
  settle or clear at the whistle — is still open, and these frames are its material rather than its
  answer. The direction is with Allen.
- **No treatment for anything here.** `T135` is ruled a defect; its remedy is not authored, and the
  choice between *suppress the ending until the ticket is done* and *end each leg's broadcast
  properly* is a real fork that wants Allen.
- **One seed, one matchup, one stake** across all three arms — the set's strength as a comparison and
  its limit as a sample, exactly as the lane states.
- **Nothing about cards**, and no 1–1 / 2–2 arm — generality was not what was missing.
- **`C46` on the correct-score label is moot rather than clear:** `CorrectScore` is twelve characters
  and fits, but it is the wrong string, so its width certifies nothing.
