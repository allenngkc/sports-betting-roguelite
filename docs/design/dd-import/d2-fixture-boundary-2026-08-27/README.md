# EVIDENCE DOCK — `D2`'s FIXTURE BOUNDARY: `T94`'s seam, after the fix

**Shot:** tv-theater lane, 2026-08-27 · **80 frames, 40 per moment.** Frames UNTRACKED; this README commits.
**Against:** DD batch 197's pre-commitment, as amended by `T140-am5` (batch 198 — reading (b) split b1/b2).
**Build:** `d08672a` (HEAD at shoot) · the seam itself is `83bd2f1` (`UpdateTicketColumn(_liveLegsShown)` at both sites, `LegsOfFixtureAfter` deleted).
**Seed:** `D2-BOUNDARY-1`, **searched not chosen** — see §"Why the seed is searched".

| moment | subject | frames |
|---|---|---|
| **M1** | after fixture f's whistle, during the result beat | 40 |
| **M2** | fixture f+1's first beat | 40 |

**Ticket:** two moneyline legs on **two different matchups**, asserted by matchup REFERENCE (the rule
`TicketFixtures` groups by), never by index — a re-seeded slate could hand back one matchup twice and
satisfy an index check while destroying the very thing this window exists to photograph.

- fixture **f** = `Refunds/Zambonis` · fixture **f+1** = `Notaries/Turnips`

---

## THE READS, AS RENDERED

**M1 — the result beat:**

```
chip0='W'   chip1='NEXT'   live0=False  live1=False
scorebug='REFUNDS  1 — 2  ZAMBONIS ●'
line0='ZAMBONIS ML'   line1='TURNIPS ML'   need0=''  need1=''
```

**M2 — fixture f+1's first beat:**

```
chip0='W'   chip1=''   live0=False  live1=True
scorebug='NOTARIES  0 — 0  TURNIPS ●'
need1='TURNIPS TO WIN'
```

---

## AGAINST THE PRE-COMMITMENT — observations only; the reading is the DD's

Batch 197 §"Pre-committed readings" was written before these numbers existed and this document
authors none of it. What was observed:

| branch | pre-committed condition | observed |
|---|---|---|
| **(a)** | M1: f resolved, **nothing live**, f+1 at `NEXT`, bug on f · M2: bug on f+1 **and** its legs live | **every clause holds** — see the two blocks above |
| **(b1)** | f+1 reads LIVE while the scorebug still holds f → the advance survived | `live1 = False` at M1 |
| **(b2)** | f's OWN legs still read LIVE through the beat → no repaint happened | `live0 = False` at M1 |
| **(c)** | the column reads BLANK rather than resolved-plus-`NEXT` | `chip1 = 'NEXT'` **literally**, and both rows print their statements |

**On (c), a correction to this lane's own expectation:** a `NEXT` row does **not** carry a blank chip
— it prints the word `NEXT`. This seat had assumed the chip was blank in both states and added
`DebugLegLine` so the compact statement could tell them apart. The accessor is still the general
discriminator and stays, but here the chip settles it outright.

---

## `C55` — every subject in frame, LOCAL space, before each shutter

Five subjects asserted at both moments — `LegRowState0`, `LegRowState1`, `LegRowNeed0`,
`LegRowNeed1`, `Matchup` — all IN FRAME against canvas `x -490..490, y -275..275`. A green burst
proves nothing if the subject scrolled off.

---

## WHY THE SEED IS SEARCHED, AND WHY THE FIRST THREE ATTEMPTS PRODUCED NOTHING USABLE

Recorded because each failure is a different way a capture window can be green and worthless.

1. **A trigger keyed on CHANGE is not a trigger keyed on the SUBJECT.** M2 first waited for
   `DebugMatchupText != <M1 value>`. The scorebug is **cleared between tellings**, so that fired on
   the blank and photographed a teardown frame — `scorebug='' chip0='' chip1=''` — while passing
   green in 110s. It now waits for f+1's own club to be NAMED.
2. **NUnit's default per-test timeout is 180s.** Every other capture in this harness carries its own
   `[Timeout]`; this one did not. Fixing the trigger made the test honest and therefore slower,
   straight into that ceiling. **A green run got slower by becoming honest.** Now `[Timeout(1200000)]`.
3. **THE MOMENT DID NOT EXIST.** Both legs were backed HOME on an unsearched seed and fixture f's leg
   **LOST**. A parlay dies at its first dead leg, so the sweat ENDED on fixture f and there was no
   boundary at all — the run failed with *"the scorebug never NAMED fixture f+1 ('AUDITORS')"*.
   **A capture window can be perfectly built and still photograph nothing, because the moment it
   wants never occurred.**

So fixture f's leg is now required to WIN, found by locking throwaway `Run`s and asking the ENGINE's
own grader (`Matchup.Grades`) rather than comparing goals here. The search is sound because
`Run.LockRound` samples every matchup from `Rng.Outcomes`, a stream the betting path never draws
from.

**Also why that first `chip1=''` was not a `NEXT` row:** on the dead-ticket seed leg 1 was cancelled,
not next. The two states look alike in a one-field log and are not alike on the surface.

---

## WHAT THIS SET DOES NOT CARRY

- **`T152-am3`'s arity>1 strip frame (DD batch 203) did NOT occur** — no stoppage batch of two or
  more goals arose in this sweat. It was **watched for and never forced or seed-hunted**, per that
  batch's instruction; the watcher matches the rendered `^\d+ GOALS$`, which the arity-1 forms cannot
  collide with. **Its absence is a fact about this sweat, not a failure of this window**, and the
  strip build remains BUILT-not-verified.
- **No existing set could have settled this**, checked before shooting: 52 sets, 2,376 files, and
  only `anchor-window-2026-08-24` is a TV set on more than one leg — its two bursts are both ANCHOR
  moments at `clock=1'`, on ONE fixture, and it predates the seam by three days. Reading 1 is
  precisely the behaviour `83bd2f1` changed, so no pre-fix frame can carry it.
- **No ruling.** Which branch these observations satisfy is the DD's call.
