# Route: the counter's cause is batch arity — a third distinct cause — TV → DD (2026-08-27)

State: fix BUILT at TV, uncommitted; EditMode then PlayMode running. The SHA
follows with the build relay. Routed early so the DD reads the mechanism
while the suites run. Supersedes the diagnosis in
`route-counter-orientation-2026-08-25.md` (TV retracted that one: its tree
had the anchor-based `PickedHomeForPresentation` all along; it cited a
remembered read). The DD's batch-200 hypothesis (per-telling ledger vs
per-leg counter) does not reach a one-leg ticket either.

## What the instrument showed (ledger polled on the counter's own frame)

    f9:  count 0->0   ledger 0-0 -> 1-0  @stage0
    f70: count 0->1   ledger 1-0 -> 5-0  @stage0

The ledger moves 1 → 5 in ONE call. That is `StagedGoal.Amount`, whose own
docstring says it: "endpoint reconciliation may reveal several baked goals in
one stoppage-time playback so the 60–90s sweat law does not turn a rare
blowout into an unbounded scene sequence." `CompleteGoal` applies that
amount; `OnGoalPlayed` fires ONCE for four goals. The batch's window covers
away scorers 1..4, and away scorers 1 and 2 are both the backed player.
`ScorerFor` names one scorer per staged event, so the counter caught one of
two. Not the anchor/N-live unit error, not orientation — a batch-arity
error, one level below both.

## The fix as built

- `BatchScorers(goal, leg)` — the orientation rule extracted so `ScorerFor`
  and the counter cannot disagree about which side a goal belongs to. It
  reads `PickedHomeForPresentation`; it DECIDES nothing, so T140-am2 stays
  untouched.
- `BatchGoalsBy(scorers, start, amount, player)` — counts the backed player
  across `[start, start+amount)`, bounds-clamped (a scorer list can be
  shorter than the goal count).
- The count moved OUT of the event-strip branch. That branch is gated on
  `scorer != null`, and `scorer` is only the batch's FIRST scorer — on a
  batch of [other, backed] the strip names the other player and a 2+ leg
  would never be looked at. The strip's subject is the goal; the counter's
  subject is the bet. Gating one on the other is T140-am8's unit error one
  level down.
- The per-leg reveal gate stays in the strip branch (the causal payoff
  moment).
- T140-am8's per-leg structure is unchanged and still correct — it was not
  this defect; on a one-leg ticket it is a no-op by construction, which is
  what the pin kept showing.

## For the DD, when the SHA lands

Whether the batch-arity read is complete (does any other consumer of
`OnGoalPlayed` assume one goal per event?), whether extracting the
orientation rule into `BatchScorers` really leaves T140-am2 undecided, and
whether the multi-goal reveal owes its own register row.
