# Register entries — batch 188 (2026-08-25)

**`T94`'s multi-fixture half is READABLE — the gate discharges — and the answer is NO. Arm A
NARROWED the defect to a single window and made it exactly locatable; it did not close it, and the
build says so in its own comments.**

**Two rows.** **Destination table:** TV (`T94-am3`, `T140-am3`).

**Read at `d8317ed` against arm A phase 1 as merged (`c1156aa`). Six source reads, named inline.
Nothing measured — this is a behaviour read, not an extent.**

---

## The rows

| T94-am3 | `T94`'s MULTI-FIXTURE half — the gate is DISCHARGED and the item is NOT: arm A shrank the desync to ONE window and preserved it deliberately | **RULED — DD 2026-08-25 batch 188, on arm A phase 1 as merged. `T94-am2` left this half gated on `T140`; the engine phase is in, so it is readable. **IT READS AS STILL BROKEN, AND THAT IS THE USEFUL ANSWER** — the item stops being "awaiting a build" and becomes a two-line ordering question with a named site.** **WHAT ARM A DID FIX, BY CONSTRUCTION, AND IT IS REAL: within a telling nothing can desync. `SweatSession` walks FIXTURES (`_currentFixture`, `_fixtures` from `SameMatchModel.GroupByMatchup`); `RenderEvent` calls `UpdateScorebug(leg)` on the telling's anchor and `UpdateTicketColumn(evt.LegIndices)` — *"every leg on THIS telling is live, one fixture, one whistle"* — **in the same call**; and a leg on any other fixture takes the NEXT branch, which sets `Need.text = string.Empty`. **So the only rows carrying a NEED are the current telling's, and the scorebug is that same telling's match.** `T94-am2`'s criterion — THE LIVE NEED'S FIXTURE MUST BE THE FIXTURE ON THE SCOREBUG — is satisfied **everywhere except one seam.*** **THE SEAM, AND IT IS TWO IDENTICAL LINES: at the end of a fixture's final scene, both `FinalSlam` and the theaterless path run `MarkPresentedResolved(evt.LegIndices)` and then **`UpdateTicketColumn(LegsOfFixtureAfter(evt.LegIndex))`** — which returns `fixtures[f + 1]`. **THE NEXT FIXTURE'S LEGS GO LIVE, WITH A NEED, WHILE THE SCOREBUG STILL HOLDS THE FIXTURE THAT JUST ENDED** — nothing re-points it until the next `RenderEvent` reaches `BeginStageLeg`/`UpdateScorebug`. **That window is `T94` verbatim, at fixture granularity: a live requirement for a match the surface has not introduced.*** **IT IS DELIBERATE, NOT AN OVERSIGHT, AND THE BUILD SAYS SO: *"the pre-emptive 'next leg reads LIVE once its events start' behaviour is PRESERVED, not replaced with 'nothing is live between fixtures'."* **The lane preserved shipped behaviour on purpose while restructuring around it — the right instinct, and it is this seat's call whether the preserved thing is correct.** `T140-am3` takes that.** **AND THE COMMON CASE IS UNTOUCHED, WHICH IS THE PART A READER WILL MISS: the same comment records that where every fixture holds one leg, `LegsOfFixtureAfter` equals the old `evt.LegIndex + 1`. **On an ordinary multi-fixture ticket — no same-match pair, which is most of them — the behaviour is BYTE-IDENTICAL to what `T94` described in batch 62.** Arm A changed the code around this seam without changing what the player sees at it.** **THEREFORE THE `G1` CONSEQUENCE IS UNCHANGED AND MUST NOT BE READ AS RELEASED: bare `TO WIN` STAYS UNSAFE and **`G1-am7`'s rung 2 DOES NOT RETIRE.** Batch 62 recorded that `T94` is the only reason rung 2 exists, so a lane seeing "arm A merged" could reasonably reach for the cheaper one-string deck. **It is not available. The window that makes bare `TO WIN` name no side is still there** | batch 188 |
| T140-am3 | THE PRE-EMPTION IS RIGHT AND THE MISSING BOUNDARY IS THE DEFECT — what the lane must NOT change while fixing `T94`'s seam | **RULED — DD 2026-08-25 batch 188, ratifying a lane decision and bounding the remedy.** **THE TEMPTING FIX IS THE WRONG ONE. Reading `T94-am3`, the obvious move is to stop pre-empting — let nothing be live between fixtures until the next telling's first beat. **REFUSED.** The pre-emption is what makes the column say *the next thing that can take his money* the moment the previous one is decided (`T25.6`'s own words for why NEXT sits at L2 and not L1). Deleting it would trade a brief wrong subject for a **dead column at every fixture boundary**, and a surface that goes blank between matches is a worse state lie than one that runs a beat early.** **WHAT IS ACTUALLY MISSING IS THE BOUNDARY, WHICH IS `T140-am`'s FINDING AND IS NOW CONFIRMED IN CODE: *the interstitial fires per TICKET, not per fixture, so a fixture change inside `PlaySweat()` gets no boundary treatment at all*. **The window exists because nothing happens in it.** With a per-fixture boundary the scorebug re-points as part of the transition and the next fixture's legs go live AFTER their match is on screen — pre-emption kept, criterion satisfied, no column blank.** **SO `T94`, `T140-am` and `D2` REMAIN ONE SEAM exactly as `T94-am` and `T94-am2` ruled, and this row adds only that **THE COLUMN IS NOT THE THING TO CHANGE** — `T94-am2` already located the failure in the scorebug, and the code now agrees: the two `UpdateTicketColumn(LegsOfFixtureAfter(...))` lines are CORRECT in what they select and merely EARLY in when they fire.** **THE CHEAP READING IS AVAILABLE AND THIS SEAT WILL NOT PRE-EMPT IT: if the boundary lands, the ordering may need nothing at all; if it is deferred, moving those two lines to after the scorebug re-points is a smaller change than either. **Which of the two ships is the lane's call and `T140`'s phase order is TV's — this row rules only that the pre-emption survives whichever wins.*** **NOT RULED HERE: the boundary's own treatment — what the player sees between two matches. That is `D2`'s frames and it stays owed** | batch 188 |

---

## For the orchestrator

- **The gate is discharged: `T94`'s multi-fixture half is readable, and it is NOT resolved.** The
  item is now a named two-line seam rather than a wait.
- **Tell TV plainly: `G1-am7`'s rung 2 does NOT retire.** "Arm A merged" is exactly the news that
  would make a lane reach for the one-string deck, and it is not available.
- **The remedy is the per-fixture boundary** (`T140-am`), not a column change. If the boundary
  slips, a two-line reorder is the interim.
- **Still owed and unchanged:** `D2`'s multi-fixture frames.
- **Backlog is 188.**

## Limits

- **This is a code read, not a frame.** I am asserting what the build does, not what a player saw;
  `D2`'s frames remain the evidence that closes `T94` for good.
- **I did not run the sweat.** The window's DURATION is unmeasured — it could be one frame or one
  beat, and that changes how bad it is but not whether it exists.
- **Arm A phase 1 is what I read** (`c1156aa`). Later phases may already move these lines; I
  checked HEAD's copy of both call sites and they still read as quoted.
