# Route: the counter's remaining defect is an orientation mismatch — TV → DD (2026-08-25)

State: T140-am8's remedy is BUILT and correct (per-leg counters and reveal
gates; `OnGoalPlayed`'s scorer branch walks every leg of the telling; score
repaint and punch stay on `_stageLeg` per the discriminator; the shared flag
retired; EditMode 342/341/0/1). NOT YET COMMITTED — TV is waiting for a
PlayMode run it can explain (see §3). The remedy was right; it was not this
defect.

## 1. What the diagnostic shows

One transition, at frame 70 of 74: the ledger's revealed endpoint is
`picked=5 opponent=0` against a 0-5 statline with the backed player AWAY.
`ScorerFor` takes its index from one authority and its list from another,
and for a player market they disagree:

- the index is `_ledger.Picked` — keyed to AWAY here (picked=5 on 0-5);
- the list is chosen by `SweatFlavor.PickedHomeForPresentation(leg)`, which
  returns true unconditionally for every kind that is not Moneyline or
  AnytimeScorer — so it says HOME, and `HomeScorers` is empty.

An away-keyed index into a home-keyed list. Darryl sits at away indices 1
and 2, consecutive, and the whole reveal arrives in one burst — which is why
the count moves once and stops rather than never moving.

## 2. Why TV stopped

`_ledger.Picked` and `PickedHomeForPresentation` are two different answers to
"which side is picked" for a market that backs a player rather than a team —
T140-am2's prose-anchor item and `MatchModel.AnchorSide`'s ground.
Reconciling them inside `ScorerFor` would decide it. TV stopped rather than
reach for it.

Orchestrator's note for the DD: `PickedHomeForPresentation`'s kind test
("not Moneyline or AnytimeScorer → home") is the shape the console gate
at `9c6df0f` already treats as NEITHER for player-scorer kinds against
`AnchorSide`. Whether this is (a) the orientation question itself (Allen's),
or (b) a stale kind-list in `PickedHomeForPresentation` that the multi-scorer
kinds were never added to — with the orientation question untouched — is
the DD's read. If (b), the counter can close without Allen.

## 3. PlayMode flaky under load

Four runs, three different unrelated failures: T88 timing out at 180s,
PhoneTests "did not release cleanly (waited 10s)", T88 again on a preview
that never entered. All wall-clock assertions, none in touched code; the
touched tests passed every run. The machine has run back-to-back Unity
sessions all day. TV treats them as load artefacts and will not claim green
or commit against a red suite it has not explained.
