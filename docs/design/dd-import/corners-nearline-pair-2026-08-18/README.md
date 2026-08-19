# EVIDENCE DOCK — THE NEAR-LINE PAIR (spec §8 item 3)

**Shot:** tv-theater lane, 2026-08-18 · **Build:** unit 3 phase A (`acd9d9f`) + phase B (`4a06b52`)
**Seed:** `APPROACH-WATCH-2`, matchup #0 — **Moose Jaw Meatballs v Reno Longhaulers** · **line 8.5**

| arm | pick | outcome | frames |
|---|---|---|---|
| **A** | `OVER 8.5 CORNERS` — **gated** by T115 | **LOST**, one short | 156 |
| **B** | `UNDER 8.5 CORNERS` — **ungated** (§6) | **WON**, held exactly | 162 |

**Frames UNTRACKED** — README commits, the rolls do not.

---

## WHY THIS IS ONE SEED AND NOT TWO

The seed search (`engine.tests/NearLineSeedSearch.cs`, 40 candidates) returned the **same seed as
the best candidate for both shots**, which was not designed for:

```
BEST FOR A  seed=APPROACH-WATCH-2  OVER  matchTotal=8 threshold=9  margin=-1  Lost  OVER-MISS-BY-1
BEST FOR B  seed=APPROACH-WATCH-2  UNDER matchTotal=8 maxAllowed=8 margin= 0  Won   UNDER-HOLDS-EXACTLY
```

**One match, total 8, line 8.5. The over needed 9 and got 8; the under allowed 8 and got 8.** The
tightest possible miss and the tightest possible hold, on one fixture, differing only by **which side
of the line the ticket sits on.** That is the same instrument discipline the phase's whole read rests
on — the corners/goals pair differed only by market; these differ only by side.

**Both arms therefore run the identical count progression: 2 · 4 · 6 · 7 · 8.**

---

## THE HEADLINE — the flat watch and the contoured watch, side by side

**Arm B (UNDER, ungated): `CornerAgainst` on scenes 2 through 12 — ELEVEN consecutive windows, one
token.**

That is **the before-state's exact shape.** `count-sweat-read` §2 found `CornerFor` across fourteen
consecutive windows and called it *seven departures from nothing*. The under arm still reads that way,
because §6 puts it outside the gate.

**Arm A (OVER, gated): `CalmPossession` and `CornerFor` interleaved.**

| scene | arm A (gated) | scene | arm B (ungated) |
|---|---|---|---|
| 2 | **CalmPossession** | 2 | `CornerAgainst` |
| 4–5 | `CornerFor` | 4 | `CornerAgainst` |
| 6–7 | **CalmPossession** | 6 | `CornerAgainst` |
| 8–11 | `CornerFor` | 8 | `CornerAgainst` |
| 12+ | `LegFinalLost` | 10, 12 | `CornerAgainst` |

**This is the first time the change has been photographed against an unchanged control on the same
match.** The re-shot pair (`corners-sweat-after-2026-08-18`) compared after-vs-before across *time*;
this compares gated-vs-ungated across *one fixture*, simultaneously.

## THE SHOT §8 ITEM 3 ACTUALLY ASKED FOR

**Arm A reaches the approach and never gets its turn.** The count ends at **8 against a threshold of
9** — `distanceAfter == 1` on the final corner, which is `ApproachDistance`, and then the match ends.
`LegFinalLost`.

**Every frame we held before this was a comfortable winner.** This is the case the spec said the
ramp's whole value lives in, and it is now on disk.

---

## ⚠ WHAT THIS SET CANNOT ATTRIBUTE — read this before quoting any per-event claim

**The window token does NOT reliably name that event's own scene, and arm B proves it.**

Arm B is **ungated** — every count event must produce a count scene. Yet its shoot log shows **3 of 5
events carrying neutral/possession strip lines**, not corner lines. A capture window fires on the
*count change*, and the grammar token is `DebugSceneTemplate` sampled at that instant, which can still
be the *previous* scene.

**So neither the token nor the strip line alone identifies which events were quieted.** Per-event gate
classification needs the beat-level log, which this harness does not emit.

**What this set DOES establish unambiguously:** the count progressions, both outcomes, the window and
duration figures, and **the shape of each token stream** — which is the headline above and does not
depend on per-event attribution.

**Nothing here should be quoted as "event N was quieted."** That claim is not in evidence from this
set, and it is the one a reader will most want to make.

---

## MEASURED

| | arm A (over, gated) | arm B (under, ungated) | winning after-set |
|---|---|---|---|
| corner windows | 5 | 5 | 7 |
| dead-air windows | 8 | 9 | — |
| sim duration | **34.22s** | **34.68s** | 39.84s |
| final count | 8 | 8 | 12 |

**The two arms are within 0.46s of each other** on the same match — consistent with the gate removing
time only where it quiets a beat, and with arm B (ungated) keeping all of its.

## TWO DEFECTS STILL VISIBLE, both already ruled and neither built yet

- **The suffix survives.** Arm B's event 2 reads
  `deflected wide. corner to them, naturally. (2 in the spell)`. **`T110-am2` ruled the suffix
  REMOVED**; that is queued work and this set predates it.
- **The strip still recycles verbatim.** Arm B's events 1 and 3 both read
  `Spreadsheets counter at full sprint.` — the exact defect `count-sweat-read` §3 measured (one string
  three times in a 44-second watch). **On the ungated arm it is unchanged**, which is what an
  out-of-scope arm should look like.

## WHAT THIS SET DOES NOT CLAIM

- **Nothing about whether either watch is BETTER.** §7 is blind to it and so is this dock.
- **Arm B is a BEFORE-state, not an after.** §6 puts the under mirror out of the gate's scope —
  *"the mirror distance profile, not in evidence"* — so it shows the mirror **unchanged**. That is
  the evidence needed to decide whether it wants a ramp; gating it would have made this shot measure
  an invention rather than the question.
- **One seed, one line, one matchup.** Both arms share a single match by design, which is the pair's
  strength as a control and its limit as a sample.
- **Nothing about CARDS** (§6, never shot) and **no flat-frame or seated-view claim** (§1.3).
