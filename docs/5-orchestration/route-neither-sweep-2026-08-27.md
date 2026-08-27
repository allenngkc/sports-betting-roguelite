# Route: the NEITHER sweep found more than batch 202 anticipated — TV → DD (2026-08-27)

State: built at TV, suites running, SHA follows. Routed early so the DD reads
the classification before the diff lands.

## Classification — nine `TvSweatScreen.cs` call sites, sorted ORIENTS vs NAMES

- **ORIENTS — safe, untouched (5):** `ScoreOnlyLine` (:2510), `UpdateScorebug`'s
  scores (:3265), `BatchScorers` (:3532), `BeginStageLeg` → stage `pickedIsHome`
  (:3201), the stats panel (:5604). Each labels two counters, names no club.
  Plus `ConfigureEndpoint`, ratified untouched (batch 202).
- **NAMES but already guarded (2):** `DescribeActiveLeg`'s moneyline arm
  (:3309) and `AuthoredStatement`'s (:4757) — both behind T96's draw check.
- **NAMES, unguarded — the defect (2 fixed):** the scorebug's ● dot and
  `MatchupLine`'s ● dot. A draw moneyline is `isMl` and backs neither side, so
  `?? Side.Home` put the dot on the home club of a ticket that backed nobody.
  Both now ask `AnchorSide` directly: null means NEITHER, and NEITHER wears no
  dot — as a market leg does. T96's ruling reaching two zones its own fix never
  did.

## A real shipped defect the sweep exposed — the Handicap double-swap

Correcting the second stale C62 citation in `SweatActiveLegModel.cs` exposed
it. TV's Handicap arm was double-swapping: the dead comment said
`_ledger.Picked` is HOME for every non-moneyline kind, so the arm swapped the
pair for an away handicap. But `AnchorSide` reads a handicap's backed side
off its own choice, and `ConfigureEndpoint` orients the ledger through that
same helper — `_ledger.Picked` was already the backed side's goals. Swapping
again fed the arm the opponent's margin on every away-backed handicap:
`CLEAR BY n` where the row should read `TRAILING BY n`, and the inverse.
Fixed in this commit. C62's cost paid twice by the same dead sentence — once
as the retracted orientation diagnosis, once as a defect shipped into the
tree from it.

## Left alone deliberately — reported, not touched

`RevealedLeg.TeamName` (:176) has the same unguarded shape (a draw names the
home club), but `RevealedLeg` is composed here and consumed by the LAPTOP
(`SportsbookApp.cs:2905`). Inert today because `MarketLabel` always wins
there. Outside the TV lane; the DD places it.

`engine.tests/AnchorSideTests.cs:140` — dated by the orchestrator at `23409a8`
(comment-only, CI green).

## For the DD

Whether the two ● fixes and the Handicap fix are inside batch 202's ruling
(T163's NEITHER made reachable) or owe rows of their own; whether the
Handicap double-swap needs a register row as a shipped-and-fixed defect with
a C62 provenance; where `RevealedLeg.TeamName` goes (laptop lane is empty).
