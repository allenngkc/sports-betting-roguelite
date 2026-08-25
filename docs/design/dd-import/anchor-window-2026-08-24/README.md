# EVIDENCE DOCK — `T163`'s ANCHOR, THE TWO BRANCHES (the anchor window)

**Shot:** tv-theater lane, 2026-08-24 · **120 frames, 60 per burst.** Frames UNTRACKED; this README commits.
**Against:** `docs/design/anchor-precommit-2026-08-24.md` (DD, amended batch 183 against the lane's
scoped ask `8d18e33`).
**Build:** `c24b32c` (the anchor splits) + `7dd5686` (the live-row fix this window forced).

| frame | leg | branch under test | frames |
|---|---|---|---|
| **A** | `UNDER 1.5 GOALS` + `BTTS — NO`, seed `GOALLESS-5`, matchup 0, stake 25 | **NEITHER** — no live leg names a side | 60 |
| **B** | `Handicap` backed **AWAY**, seed `ANCHOR-B` | **SIDE** — a leg that does name one | 60 |

---

## THE READS, AS RENDERED

**Frame A** — `[ANCHOR-A]`:

```
strip='a goal in the churn; the number moves.'   home='Middlemen'  away='Mallards'  clock=1'
```

Neither club named, on a beat from an anchor-interpolating table. That is `T163` branch (3).

**Frame B** — `[ANCHOR-B]`:

```
row=' | DULUTH AUDITORS | '
strip='Gravediggers pass it around, slow and mean.'
score='AUDITORS 0 — GRAVEDIGGERS 0'   home='Gravediggers'  away='Auditors'  clock=1'
```

**That single line IS condition 6's three-zone agreement**, and it is why the DD amended the lane's
two-zone ask: the row names the club he BACKED, the scorebug says which club is AWAY, and the strip's
`{other}` slot names the home club on a down-beat. One story, readable together. **Two zones could
not have shown it** — the `●` backed marker renders on moneyline legs only, so on a handicap nothing
else says which club is his.

## C55 — all four subjects in frame, local space

| burst | subject | local x | verdict |
|---|---|---|---|
| A | `Flavor` | −173.0 … 478.0 | IN FRAME |
| B | `LegRowLine0` | −482.0 … −335.0 | IN FRAME |
| B | `Flavor` | −173.0 … 478.0 | IN FRAME |
| B | `Matchup` | −223.0 … 398.0 | IN FRAME |

Canvas −490.0 … 490.0 × −275.0 … 275.0.

---

## ⚠ FIVE THINGS THE FRAMES DISPROVED, AND ONE THEY FOUND

**Every one came from a BINDING CONDITION REFUSING A SUBSTITUTION.** Recorded because the tidy
version of this dock would hide all of them.

### 1. Frame A's moment is NOT arm 2's, and §5's before-pair is weaker than it hoped

§5 offered `drawn-ending-t129-2026-08-19/arm2` as a free BEFORE — same composition, predating the
change. **Its burst runs 150 frames FROM THE WHISTLE**, and on a drawn match the strip at FT carries
`THE MATCH ENDS LEVEL`, written DIRECTLY at the call site in `FinalSlam`/`RenderEvent`, never drawn
from a table (`SweatFlavor.For` returns `FINAL WHISTLE` on a LegFinal for the same reason).

**That line is club-free by AUTHORSHIP, not by the anchor.** A frame matching arm 2's moment would
satisfy §5 and FAIL condition 1 while looking perfectly clean. So frame A shares arm 2's seed,
matchup, stake and picks but bursts at a beat MID-SWEAT.

**Consequence for the pair, stated so it is not over-read: it compares COMPOSITIONS and the COLUMN,
not the anchor's effect on the strip.** §5 is explicitly *"not a condition, an economy"*; condition 1
binds.

### 2. The goalless seed DOES reach the goal family — both the DD and the lane predicted otherwise

The pre-commitment expected no goal beats on `GOALLESS-5`, and the lane repeated it. **Frame A caught
`NeitherGoalUp[1]` at 1'.** A `Score` beat plays in FULL on a match that finishes 0–0 — the ledger's
live-lead clamp stages it as the CHALKED-OFF variant — so **the narrative beat and the scoreline are
different things.** Frame A covers the goal family, not just momentum.

### 3. B1's literal wording holds only on UP-beats

B1 reads *"the club the strip names is the club the leg backs."* The tables interpolate `{picked}`
when the number rises and `{other}` when it falls, so **on a down-beat the correct line names the club
he did NOT back.** The first shot failed on `Gravediggers pass it around, slow and mean.` — `MomDown`'s
`{other}` slot working exactly as ruled.

**Re-derived, direction-free and stronger:** the rendered line must be producible at anchor AWAY and
**not** producible at anchor HOME. The sets are disjoint line by line — at anchor Home that template
reads `Auditors pass it around`. **This identifies WHICH ANCHOR rendered the line rather than which
club appears in it**, which is what `K17-cl` was about: its defect looked like an ordinary line naming
the wrong club.

### 4. Condition 6 must be read PER ROW, not per span

Read as the compact line alone it returned the empty string. **A LIVE row blanks its compact line by
design** and carries identity on NEED — `T130`'s own rule, *"emptiness of a SPAN is normal and
correct; emptiness of the WHOLE ROW is the defect."* Same law, same question.

### 5. `MarketSheet` uppercases and `SweatFlavor.Short` does not

The row read `DULUTH AUDITORS`; the club noun is `Auditors`. A case-sensitive comparison failed on a
row that named the backed club perfectly well — **a fixture bug that would have read as a build
defect.**

### AND THE ONE THE WINDOW FOUND: A SHIPPING DEFECT ON HALF THE BOARD

Condition 5 refused a home-backed substitution, which forced a **live `Handicap` row that no test had
ever rendered.** It came back **blank in all three spans** — `T130`'s defect, on a market the board
prices with four selections.

Two causes, one chain: `DescribeActiveLeg`'s `default:` returned an all-empty copy (**item `1.3`'s
defect surviving on SEVEN offered kinds** — `1.3` added the `CorrectScore` arm and left the default
standing), and under it `LegStatement`'s `default: leg.DisplayLabel` gave the bare word `Handicap`.
**The console had already ruled that second one:** `SweatLines.LegName` — *"Nothing here falls back to
the enum name: THAT FALLBACK IS K16/T130."* The TV kept what the console removed.

Fixed in `7dd5686`, both reading `MarketSheet` (`S96`, §6.5), with an exhaustive blank-row gate over
**every selection the board prices — 25 across 14 kinds.**

---

## WHAT THIS DOCK DOES NOT CLAIM

- **Nothing about A2 or B2.** Those are DIRECTIONAL reads and they are the DD's, per the
  pre-commitment's §3. This dock supplies frames and the binaries; it authors no read.
- **Neither frame speaks about the other's branch** (§4). They test opposite branches.
- **Nothing about the counter, the footer or the tape** (§4) — `T165-am`, `T133` and `T166` have
  their own evidence and none of it is here.
- **No same-match ticket is involved**, deliberately: the subject is what `T163` does to the tickets
  players build TODAY. The same-match coverage gap is recorded in the lane handoff and is not part of
  this window.
- **Seven offered kinds still have no authored NEED copy** and now fall back to naming the bet rather
  than the requirement. That compromise fixes the SILENCE; the copy is routed to the DD and unruled.
