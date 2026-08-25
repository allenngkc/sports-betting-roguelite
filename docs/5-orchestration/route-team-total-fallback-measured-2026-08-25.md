# Measured: the team-total NEED fallback — TV → DD (2026-08-25)

Against `docs/design/measurement-ask-team-total-fallback-2026-08-25.md`. **Report only — §4 of the
ask pre-committed the reading before the number existed, and this document authors none of it.**

## CONDITIONS (`C58-am2`), both stated

- **Commit measured at: `b60d2bd`** — read from the repo at run time, not hard-coded.
- **`T168-am` BUILT: NO.** Verified by absence: no reference to `T168` anywhere under
  `unity/SBR/Assets/**`. **The club token is therefore still the FULL name at render**, which is the
  state that makes the ladder longest. Every number below is a pre-`T168-am` number.

**Suite: EditMode 336/335/0/1**, measured through the shipped path — `LegStatement` and
`FitToColumn` reached by reflection, not reimplemented. NEED box **261.0px**, font asserted Encode
Sans (`T20`).

---

## THE FOUR CASES

| # | kind · line · club | input | in | **survivor** | out |
|---|---|---|---|---|---|
| 1 | `TeamTotalGoals` 1.5 · one-word city | `RENO FERRETS OVER 1.5 GOALS` | 390.0 | **`RENO FERRETS OVER`** | 258.0 |
| 2 | `TeamTotalCards` 1.5 · **same club** | `RENO FERRETS OVER 1.5 CARDS` | 390.1 | **`RENO FERRETS OVER`** | 258.0 |
| 3 | `TeamTotalGoals` 1.5 · two-word city | `MOOSE JAW SPREADSHEETS OVER 1.5 GOALS` | 549.7 | **`MOOSE JAW`** | 147.6 |
| 4 | `TeamTotalCorners` 4.5 · control | `RENO FERRETS OVER 4.5 CORNERS` | 424.3 | **`RENO FERRETS OVER`** | 258.0 |

**THE DISTINCTIVE WORD IS LOST IN ALL FOUR.** No `GOALS`, no `CARDS`, no `CORNERS`, and **no line**.

**`T46`'s backstop is NOT reached** — every survivor sits inside 261.0, so no single over-wide word
is returned whole.

---

## THREE FINDINGS

### 1. `T156` IS LIVE, and the survivor is now a string rather than an inference

Cases 1 and 2 are the `T156` pair. They survive as **`RENO FERRETS OVER`** — character-identical.
Batch 187 ruled `T156` live from the config plus a width; this is the surviving string itself.

**It also ends on a dangling `OVER` that qualifies nothing.** The row states a direction with no
quantity and no market.

### 2. THE CITY-ONLY SURVIVOR IS REAL, AND IT IS WIDER THAN `T156`

**Only the two-word city reaches it: `MOOSE JAW`** — the club's own noun gone. Stated plainly as the
ask requires: **a city-only survivor collides across EVERY market that club appears in**, not the
four pairs per match per club `T156` names. It is the inverse of `T69`'s shipped convention (keep the
distinctive word, drop the city).

**Rarer than the one-word case** — it needs one of `SlateGenerator`'s few two-word cities — but it is
reachable on a real slate, and it was reached here without searching for it.

### 3. ⚠ THE CONTROL COLLIDES TOO, AND THE ASK DID NOT EXPECT THIS

`TeamTotalCorners` at **4.5** was listed as *"ruled NOT to collide (unshared line) — measured to
confirm, cheap."* **It collides.** `RENO FERRETS OVER` at 4.5 is character-identical to
`RENO FERRETS OVER` at 1.5.

**The unshared-line protection cannot reach past a truncation that drops the line.** The ruling's
reasoning is sound — the 4.5 line IS unshared — but the line is gone three words before the survivor
is reached, so it distinguishes nothing on this surface. **Whatever fixes cases 1–3 must not assume
corners is already safe.**

---

## WHAT THIS LANE IS NOT CONCLUDING

- **§4's readings are the DD's**, including the falsification condition for batch 187. Case 1 shows
  the noun does NOT survive, so §4(a) is not triggered; that is reported as an observation, not read.
- **No copy is proposed.** The surviving strings are what the shipped ladder produces today, not a
  recommendation.
- **Every number is pre-`T168-am`.** If it is built, the club token shortens and all four rows must be
  re-measured — the shorter input may leave the distinctive word inside the box.

## PROVENANCE OF THE CORRECTION

**The first run of this measurement was wrong and is superseded.** It bucketed clubs by MATCHUP, so a
matchup whose two teams had different name lengths landed in both buckets: cases 1 and 3 measured the
same club and three distinct rows were reported as four. It also failed to read the commit, because
**`.git` is a FILE in a worktree**, not a directory. Both are fixed; this table is the corrected run.

**The city-only finding is unchanged by the correction** — it came from case 3, which was measured
correctly the first time and is the worst case the ask named.
