# Register entries — batch 174 (2026-08-24)

**TV's coupled item recorded — CORRECTED: the assumption is not at the address given, and the real
one is worse. And checking it turned up a LIVE `T164` VIOLATION on the console that nobody routed.**

**Two rows.** **Destination table:** Console (`K21`, `K22`).

**Five source reads at this seat, cited to lines. No frames, nothing measured.**

---

## The rows

| K21 | TV's coupled item, CORRECTED — the console's leg-state read is already per-leg; the real arm-A defect is `onFinalLeg`, and it SKIPS THE CLIMAX | **RAISED for the next markets seating · DD 2026-08-24 batch 174, on TV's route at `f44ab44`, verified at source rather than relayed.** **TV'S CLAIM AS PHRASED IS NOT IN THAT FILE, and recording it as phrased would have cost the next seat a search: `SweatRenderer.cs:389` reads `session.RevealedLegState(e.LegIndex)`, and that is backed by a PER-LEG ARRAY — `engine/SweatSession.cs:434` returns `_revealed[legIndex]`. **There is no scalar resolved-through in the console.** Its verdicts are driven off the event stream leg by leg, which is the shape TV is moving TO.** **`legSeen` (`:247`, `:254-257`) WAS THE OBVIOUS CANDIDATE AND IT IS CLEAN — recorded so it is not re-raised. I expected a fixture's interleaved legs to flip it repeatedly and re-anchor `prevProb` mid-telling. **They cannot: phase 1 REDEFINED `LegIndex`** — `engine/DramaEvent.cs:19-21`, *"`LegIndex` and `WinProbAfter` … describe the telling's ANCHOR leg — the lowest ticket-order leg on that fixture"* — so every event of one telling carries the SAME index and `legSeen` changes once per telling, which is exactly what it detects.** **THE REAL DEFECT IS `onFinalLeg` (`:296`), AND ITS CONSEQUENCE IS NOT PACING TRIVIA: `bool onFinalLeg = evt.LegIndex == lastLeg;` compares the telling's ANCHOR leg against the ticket's LAST leg index. **If the last leg shares its fixture with any earlier leg, the anchor for that telling is the EARLIER leg, so the comparison is FALSE for the very telling that resolves the last leg.** And `onFinalLeg` gates the fast-forward: `if (fastForward && onFinalLeg) fastForward = false; // reached the final leg — it must be sweated`. **THE FINAL TELLING IS THEN FAST-FORWARDED INSTEAD OF SWEATED — the sweat's climax skipped, on exactly the same-match multi-leg ticket SGP shipped as the expected shape.*** **THE CLASS IS TV's — a scalar index standing in for a set — but the SITE and the CONSEQUENCE are different, so the next seat should be sent to `:296` and not to a resolved-through mark that is not there. **TV's own remedy for the TV is not in question and is not touched here.*** | batch 174 |
| K22 | The console DISPLAYS the anchor leg's probability — a live `T164` violation, and the engine already exposes the right source | **RULED — VIOLATION · DD 2026-08-24 batch 174, found while checking `K21` and NOT ROUTED BY ANYONE.** **`SweatRenderer.cs:151` — `MeterProbability(DramaEvent e) => $" {Ui.Pct(e.WinProbAfter)}%"` — and `:149` — `MeterBar(DramaEvent e) => Ui.Bar(e.WinProbAfter, BarWidth)`. **The console prints the anchor leg's probability as a percentage AND as a bar, on every beat.*** **THE ENGINE'S OWN DOCSTRING FORBIDS IT IN TERMS, and names the replacement — `engine/DramaEvent.cs:23-26`: *"**`WinProbAfter` is not a display quantity.** `T143` and `T164` rule that the shown win-probability is the TICKET's and that no leg's probability is ever displayed alone; **presentation reads `SweatSession.TicketWinProbability`.**"* **`TicketWinProbability` EXISTS (`SweatSession.cs:466`) AND THE CONSOLE REFERENCES IT NOWHERE** — zero hits across `game-console/`.** **IT IS LIVE AND REACHABLE TODAY, not latent. The same docstring says *"on a ticket with at most one leg per matchup every telling has exactly one leg, so all four agree and nothing moves"* — **so they agree only until a ticket carries two legs on one matchup, which is what SGP shipped as the headline shape.** On a same-match ticket the console shows one leg's probability while the player's money rides all of them.** **AND IT IS `T164`'s OWN SUBJECT ARRIVING ON A SECOND SURFACE, which is why it is ruled here directly rather than assumed to be covered (`S67`): `T164` was ruled on the TV's `_liveProb` and `RevealedView.Reset`. **This is the same defect, the same quantity and the same remedy, at a call site `T164` never routed through** — `T86-am`'s pattern, and `K17-cl`'s from four batches ago.** **THE FIX IS A SOURCE SWAP, NOT A DESIGN QUESTION: both sites read `SweatSession.TicketWinProbability` instead of `e.WinProbAfter`. **The bar and the percentage must take the SAME source** — a bar scaled to one quantity beside a number printed from another is worse than either alone.** **NOT RULED: whether the meter should show a probability at all on this surface. §6.3 rules the console is the only surface printing all four of the four-number model and that the true-probability COLUMN stays; the sweat meter is a different slot and this row does not reach it** | batch 174 |

---

## For the orchestrator

- **Both are for the next markets seating**, coupled as TV asked — but `K21` sends it to
  `SweatRenderer.cs:296`, **not** to a resolved-through mark.
- **`K22` is the heavier one and nobody routed it.** It is a two-line source swap with the correct
  source already in the engine, and it is live on any same-match ticket.
- **`legSeen` is checked and clean** — recorded so the next seat does not spend the search.
- **Backlog is 173–174.**

## Limits

- **`K21`'s consequence is derived from the code path**, not observed: the anchor-leg redefinition is
  quoted from `DramaEvent`'s docstring and the fast-forward gate from `SweatRenderer.cs:296-298`.
  **No console capture was shot**, and a frame would settle it in one run.
- **`K22` asserts what the two lines pass**, which is on the face of them; whether `Ui.Bar` and
  `Ui.Pct` are otherwise correct is not examined.
- **Nothing here touches TV's own fix**, which is Allen-approved and not in question.
