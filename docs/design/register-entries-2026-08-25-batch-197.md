# Register entries — batch 197 (2026-08-25)

**Something IS owed, and it is mine: `T140-am3` protected the very behaviour `T94` is about. The
pre-emption is not a feature to preserve around the defect — IT IS THE DEFECT.**

**One row.** **Destination table:** TV (`T140-am4`).

**Read at HEAD against `TvSweatScreen`. Nothing measured. A frame is pre-committed below, and it is
wanted to VERIFY rather than to decide.**

---

## The row

| T140-am4 | THE PRE-EMPTION IS `T94`'s DEFECT ITSELF — `T140-am3` refused to delete it and offered a worse interim; both are WITHDRAWN, and no boundary is needed to close `T94` | **RULED — DD 2026-08-25 batch 197, correcting this seat's own row before the lane builds against it.** **`T140-am3` SAID: the pre-emption is right, the missing boundary is the defect, and if the boundary slips then moving the two lines after the scorebug re-points is the interim. **ALL THREE CLAIMS ARE WRONG, and one read of the sequence shows it.*** **THE PRE-EMPTION IS THE DEFECT, IN ITS ORIGINAL WORDS. Batch 62 raised `T94` as *the column advances to leg N+1 while the scorebug holds leg N*. The build's own comment records that where each fixture holds one leg, `LegsOfFixtureAfter(evt.LegIndex)` **IS** `evt.LegIndex + 1`. **So the two pre-emptive calls ARE the behaviour `T94` was raised about — I preserved it while ruling on it.*** **DELETING THEM PRODUCES THE CORRECT STATE, AND THE CODE ALREADY PROVES IT RATHER THAN PROMISING IT: `isLive = IsLiveShown(i) AND NOT IsPresentedResolved(i) AND NOT ticketSettled`. `MarkPresentedResolved(evt.LegIndices)` runs on the line ABOVE the pre-emptive advance, so the ended fixture's legs stop being live there. **With no advance, NOTHING IS LIVE during the whistle-and-slam beat — and that is right, because that beat is ABOUT the fixture that just ended.*** **AND THE NEXT FIXTURE LIGHTS AT EXACTLY THE RIGHT MOMENT WITH NO NEW CODE: `RenderEvent` already calls `UpdateScorebug(leg)` and `UpdateTicketColumn(evt.LegIndices)` **in the same pass**, so fixture f+1's legs go live as its match appears. **The fix is a DELETION, not an ordering change and not a boundary.*** **WHAT I GOT WRONG, PRECISELY: I called the alternative *a dead column at every fixture boundary*. **The column is not dead — it is fully populated: the ended legs read RESOLVED, the coming legs read `NEXT` at L2.** What is absent is the LIVE state, and its absence is TRUTHFUL at a moment when no match is on the pitch. `T25.6` governs how bright a `NEXT` row is, **not whether a leg may be lit before its match exists** — I borrowed a ruling about brightness to defend a ruling about timing.** **SO `T94` DOES NOT WAIT FOR THE BOUNDARY. `T140-am3` bound them together and that was the load-bearing error: **the boundary is a separate design question — what the player sees BETWEEN matches, which is `D2`'s and stays owed — and `T94` closes without it.*** **SCOPE, EXACTLY TWO CALL SITES: the pre-emptive `UpdateTicketColumn(LegsOfFixtureAfter(evt.LegIndex))` in `FinalSlam` and in the theaterless path. **`LegsOfFixtureContaining(0)` AT SESSION START IS KEPT** — `BeginStageLeg` sets the scorebug in the same breath, so that call is not pre-emptive and removing it would blank the opening column.** **WHAT SURVIVES OF `T140-am3`: nothing operative. Its refusal, its interim and its coupling of `T94` to the boundary are all withdrawn. **`T94-am3`'s LOCATION of the seam stands unchanged** — the two lines it named are the two lines to delete | batch 197 |

---

## Pre-commitment — the frame `D2` owes, and what each outcome rules

Written before the capture, per standing practice. **The frame is wanted to VERIFY, not to decide;
the deletion is ruled on the code above.**

**What must be shot:** a multi-fixture ticket, two moments.

1. **After fixture f's whistle, during the result beat** — expect: f's legs RESOLVED, **no leg in
   the LIVE state**, f+1's legs at `NEXT`, and the scorebug still on f.
2. **Fixture f+1's first beat** — expect: scorebug on f+1 **and** f+1's legs live, in the same
   frame.

**Pre-committed readings:**

- **(a) Both hold** → **`T94` CLOSES.** Its multi-fixture half is discharged on frame, and
  `G1-am7`'s rung 2 becomes retirable — batch 62 recorded that `T94` is the only reason bare
  `TO WIN` is unsafe, so this is where that cheaper deck is finally available. **It is not
  retired by this row; it becomes askable.**
- **(b) A leg reads LIVE while the scorebug still holds f** → the deletion did not land, or a
  third site advances the column. `T94` stays open and that site is named.
- **(c) The column reads BLANK rather than resolved-plus-`NEXT`** → **my "not dead" claim is
  wrong and the boundary IS required after all.** This is the falsification condition for this
  row, and it reinstates `T140-am3`'s reasoning rather than my correction of it.

## For the orchestrator

- **TV's side is a two-line deletion**, not a build. It can go ahead of the T156 re-take if that
  is cheaper to schedule.
- **Nothing else is owed from me on `T94`** — the criterion is `T94-am2`'s, the site is
  `T94-am3`'s, and the remedy is now this row's.
- **`D2`'s frames are still owed**, but for verification and for the boundary's own treatment,
  which `T94` no longer blocks on.
- **Backlog is 197.**

## Limits

- **This is a code read, not a frame** — which is why the frame is pre-committed rather than waived.
- **I have not measured the result beat's duration.** It carries a whistle, a slam and a leg beat,
  so it is not a single frame, but how long it holds is unmeasured and does not change the ruling.
- **`ticketSettled` and the cash-out preview paths were not re-read** against the deletion; both
  suppress live independently, so neither should be affected, but that is reasoning and not a test.
