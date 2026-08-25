# Register entries — batch 200 (2026-08-25)

**NEITHER (a) NOR (b) AS POSED. The stale kind-list is ALREADY GONE — `PickedHomeForPresentation`
delegates to `AnchorSide` and has no kind test at all. And the orientation question is not reached,
because one leg cannot disagree with itself.**

**One row.** **Destination table:** TV (`T140-am9`).

**Route:** `docs/5-orchestration/route-counter-orientation-2026-08-25.md`. **Four source reads at
HEAD. Nothing measured, and the named suspect is a hypothesis, not a finding.**

---

## The row

| T140-am9 | The counter does NOT wait for Allen — the kind-list is already deleted, and a SINGLE leg's side is unambiguous | **RULED — DD 2026-08-25 batch 200, answering the one question routed and refusing both of its options.** **THE ROUTE DESCRIBES CODE THAT NO LONGER EXISTS. It says `PickedHomeForPresentation` *"returns true unconditionally for every kind that is not Moneyline or AnytimeScorer"*. **At HEAD it is one line with no kind test whatever:** `(MatchModel.AnchorSide(leg) ?? Side.Home) == Side.Home`. **`c24b32c` deleted that table — its own message says so, *"the duplicate fifteen-kind table is gone"* — and it is merged.** So option (b)'s defect cannot be fixed because it is already fixed.** **AND `AnchorSide` ANSWERS FOR THE MULTI-SCORER KINDS EXPLICITLY: `AnytimeScorer` and `PlayerMultiScorer` share one arm returning `leg.Matchup.PlayerSide(selection.PlayerIndex)`. **`PlayerSide` returns `Side`, NOT `Side?` — it cannot be null, so the `?? Side.Home` collapse CANNOT fire on a player market.** For an away player the function returns AWAY. **The home-keyed list the route describes is not what HEAD selects.*** **SO OPTION (a) IS REFUSED TOO, AND THIS IS THE PART WORTH KEEPING: `T140-am2`'s orientation question is about **LIVE LEGS THAT NAME OPPOSITE SIDES** — `AnchorForTelling`'s branch (2), where the telling has no anchor because two bets disagree. **A SINGLE PLAYER LEG CANNOT DISAGREE WITH ITSELF.** Its side is its scorer's club, the engine publishes it, and reading it settles nothing that Allen holds. **The counter does not wait.*** **WHY TV WAS RIGHT TO STOP ANYWAY: the two authorities it names — `_ledger.Picked` and the presentation side — ARE two answers to one question, and reconciling them *inside `ScorerFor`* could have decided something above the lane. **Stopping at a line you cannot see past is correct even when the line turns out to be elsewhere.** The error is a stale reading, not a failure of judgement, and it is `T111`'s division working: the lane stopped, the seat read.** **THE SUSPECT I NAME, AS A HYPOTHESIS AND NOT A FINDING — the same unit error `T140-am8` ruled, one level down: **the LEDGER is per-TELLING while the counter is now per-LEG.** `BeginStageLeg` calls `_ledger.ConfigureEndpoint(leg)` on the ANCHOR leg, so `_ledger.Picked`/`Opponent` are oriented to the anchor's backed side — and `ScorerFor` indexes with them while choosing its list from the LEG's own side. **On a telling whose anchor backs the other way, those two disagree by construction.** Per-leg counters read through an anchor-oriented ledger is `T140-am8`'s defect wearing the ledger's clothes.** **NOT RULED: whether that is the cause. **I have not run it, and a hypothesis stated by this seat is worth exactly what the lane's diagnostic is worth — it must be measured before it is believed**, which is the standard I hold the lane to and cannot exempt myself from. Re-diagnose against HEAD's `ScorerFor`, not against the fifteen-kind table | batch 200 |

---

## For the orchestrator

- **The counter is unblocked and does not need Allen.** TV can finish it.
- **One instruction to TV:** re-read `PickedHomeForPresentation` at HEAD before re-diagnosing — the
  route's premise is a pre-`c24b32c` reading, and the fix it proposes is already in the tree.
- **The named suspect is the ledger's orientation**, offered for TV to confirm or kill, not to build
  against on my say-so.
- **TV's §3 is endorsed without a row:** refusing to commit against a red suite it cannot explain is
  right, and three unrelated wall-clock failures on a machine that has run Unity all day are load
  artefacts until something ties them to touched code.
- **Files to stage, by explicit path:**
  `docs/design/register-entries-2026-08-25-batch-200.md` and `docs/design/REGISTER.md`.

## Limits

- **I did not run the sweat or the fixture.** Everything here is read from HEAD's source.
- **The route's diagnostic numbers are not disputed** — `picked=5 opponent=0` against a 0–5 statline
  with the player away is a real observation; what I dispute is the mechanism inferred from it.
- **If TV's build was diagnosed against its own uncommitted tree**, some of what I read as stale may
  be a local revert rather than a misreading; that would change the account of HOW this happened and
  none of the ruling.
