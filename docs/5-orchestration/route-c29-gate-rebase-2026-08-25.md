# Route: T94's seam invalidated a C29 gate's premise — TV → DD (2026-08-25)

State when routed: the seam is BUILT (`UpdateTicketColumn(_liveLegsShown)` at
both sites, `LegsOfFixtureAfter` deleted with its record left in place;
EditMode 342/341/0/1) but NOT YET COMMITTED — TV's confirming PlayMode run is
in flight and it will commit when it lands. Routed early so the DD can read
the finding while the run finishes. The commit SHA follows in the relay.

## What the seam caught

PlayMode surfaced a failure in the existing C29 gate
`TicketFooterWord_LegOneWon_RiskWhileLegTwoLive_StakeWhenLegTwoWonEarly`.
Clean signal: frames=59 and state1Cases=49 identical before and after the
seam; state2Cases 2 → 0. TV saved the seam as a patch, reverted, instrumented
the gate to record `_stageLeg` on state-2 frames, re-ran:

    [TRAP-GATE-DIAG] state2 at frame 4: _stageLeg=0 chip0='W' chip1='' footer='STAKE $25'
    [TRAP-GATE-DIAG] state2 at frame 5: _stageLeg=0 chip0='W' chip1='' footer='STAKE $25'

Both state-2 frames fired at `_stageLeg = 0`: leg 1 was lit as the live row
and read as already won while the stage and scorebug were still on leg 0's
match — and its "revealed count" at that moment was leg 0's, since
`_countLedger` only resets in `BeginStageLeg`. That is T94 verbatim; the gate
was certifying a footer word off it. The 2026-08-17 seed search that picked
STATS-MULTI-5 was measuring the same artefact — which is why one seed in
twelve carried both states.

## What TV did (flagged as a change in what the gate can construct)

- Per the pinned-seed comment's own instruction ("RE-RUN THE SEARCH — never
  widen the gate"), the search was re-run at run time. The re-base is an
  INVARIANT, not a relaxation: a state-2 frame now counts only when the stage
  is on leg 1's own fixture. The retired defect cannot satisfy the gate again.
- State 2 survives the fix: STATS-MULTI-2 produces 49 genuine state-2 frames
  with the stage on leg 1. What no longer exists is a seed carrying BOTH
  states. §5 requires both certified, never that one seed carry them, so each
  is certified on a seed that genuinely produces it.
- The original gate's 30 warm-up frames left 5 of 12 candidate seeds at
  frames=0 — blind on 42% of its own search. Dropped; the qualifying
  condition disqualifies early frames on its own.
- A pacing fix rides the same commit: `TimeScaleOverride` re-asserted per
  seed (it was not surviving `StartNewRun`, which blew the 600s budget).

## For the DD

Whether the re-based gate still certifies what C29 / §5 meant, and whether
"two seeds, one per state" is acceptable where the old gate had one seed for
both. The counter item (batch 198 §3) is unchanged: its fixture is committed
at `2ff03a6` and reads 1 where the player scored 2.
