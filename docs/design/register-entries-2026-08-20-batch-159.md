# Register entries — batch 159 (2026-08-20)

**Allen's three rulings, actioned: the terse copy is AUTHORED, and the approved drawn-ending spec is
SPLIT so TV can start on the half that survives either arm of the `T140` fork.**

**Two rows.** **Destination tables:** Cross-surface (`G1-am11`) · TV (`T162`).

**Specs:** `docs/design/spec-terse-copy-2026-08-20.md` ·
`docs/design/drawn-ending-fork-independent-2026-08-20.md`

**Nothing measured here.** Every form below is authored against measurements the lane took at
`323492d`; every new form goes back to the lane for measurement before it ships (`C58`, batch 95).

---

## The rows

| G1-am11 | The terse copy AUTHORED for the four blocked kinds — and the rule underneath it is an ORDERING rule that costs nothing | **SPEC'D — DD 2026-08-20 batch 159, on Allen's ruling (terse copy for the blocked kinds, team totals held). `docs/design/spec-terse-copy-2026-08-20.md`. AUTHORED HERE; THE LANE MEASURES — no width is asserted at this seat.** **ONE RECONCILIATION FIRST, because the set moved after Allen ruled: he ruled on batch 151's FOUR BLOCKED KINDS, and batch 158's `T161` had since moved `DoubleChance` from *blocked* to *withdrawn* on TV's measurement (NEED rung 1 clears 0 of 20, rung 2 clears 1, every truncation deletes `DRAW`). **THE COUNT AND THE INSTRUCTION BOTH SURVIVE: terse copy IS re-authoring, which is what a withdrawn form needs.** `DoubleChance` stays the fourth target with a NEW FORM rather than another rung; the team totals stay held, as ruled.** **THE RULE THE SPEC IS BUILT ON, and it is the reusable half: **AUTHOR SO THE LAST TOKEN IS THE LEAST LOAD-BEARING.** `FitToColumn` drops whole words FROM THE END (`T155`), so the final token is the first casualty of any overrun — and every defect the measurements found is that rule violated. `{CLUB} UNDER 4.5 CORNERS` loses the market; `{CLUB} TO WIN OR DRAW` loses THE BET'S TERMS; `{SURNAME} TO SCORE 2+` loses the verb and the quantity. **This is an ordering choice made once at authoring time and it costs nothing.*** **THREE TESTS EVERY RUNG MUST PASS, so a rung can be rejected without a ruling: (1) TRUNCATION-SAFE — dropping the final token must not invert or narrow the bet's terms; losing identity is bad, STATING A REQUIREMENT THE PLAYER DOES NOT HAVE IS WORSE. (2) NON-COLLIDING — the rung AND EVERY TRUNCATION OF IT against `LegStatement`'s six shipped arms and the other eight kinds (`T156`). (3) The standing checks — `T87`, §8 casing, `T69`, `T108`, `T70`.** **THE LADDERS: `PlayerMultiScorer` takes ONE new rung, `{SURNAME} 2+`, identical to its compact and sanctioned by `LegStatement`'s own doc — **and `{SURNAME} SCORES 2+` was REJECTED because it truncates to `{SURNAME} SCORES`, the shipped AnytimeScorer rung; test 2 working rather than taste.** `WinningMargin` takes a third rung `3+ APART AT FT`, dropping `GOALS` and staying inside `T151`'s own chosen word — **`MARGIN` was considered and REFUSED: it is `MarketKind.WinningMargin`'s root AND the laptop ships `YOUR MARGIN IS CLEAR` meaning *winning comfortably*, one word with two meanings on two surfaces.** `Handicap` takes a third rung `{CLUB} ±1.5`, the market's own notation, two tokens so its only truncation is the bare club — an identity loss, never a misstatement.** **`DoubleChance` IS RE-AUTHORED TO `{CLUB} UNBEATEN` in both slots. It is exactly the condition (1X is the home side unbeaten, X2 the away), it is the sport's own word rather than bookmaker notation, `unbeaten` appears nowhere in `engine/` and in no shipped TV string — both checked — **AND IT MAKES THE MISSTATEMENT UNREACHABLE, which is the whole point: two tokens, so the only truncation is `{CLUB}`, and there is NO INTERMEDIATE FORM THAT SAYS *WIN*.** That is the string TV measured 19 of 20 clubs landing on.** **OWED BEFORE IT SHIPS: `T155`'s compact ladder must exist or the compact rungs are unreachable; the §4 measurements; `C46` against the enumerated pool. **And if a rung still misses for the longest clubs the lane REPORTS it rather than shortening further — that the band may not hold a 12-character club plus a predicate is a finding for Allen's scope call, not a thing to author around** | batch 159 |
| T162 | The approved drawn-ending spec, SPLIT by fork dependency — four items build now, and one of the deferred ones would be WRONG under arm (B) | **ROUTED — DD 2026-08-20 batch 159, on Allen's approval of `spec-drawn-ending-2026-08-19.md` AS WRITTEN with the `T140` A/B fork NOT yet ruled. `docs/design/drawn-ending-fork-independent-2026-08-20.md`. NOTHING IS NARROWED — everything approved stays approved; this splits it by dependency so TV is not idle while the fork is with Allen.** **THE TEST APPLIED IS *does it hold under BOTH arms*, not *is it small*.** **BUILDS NOW — FOUR. (1) **`§6.7`'s INTERSTITIAL AT THE FIXTURE BOUNDARY, and it is required under BOTH arms**: under (A) it is the work `T140` explicitly does NOT include (`T140-am`), under (B) it is a strict subset, since (B) puts `§6.7` at every leg boundary and every fixture boundary is one. The site is the fixture change inside `PlaySweat()`, which today runs every leg of a ticket in one call with no boundary treatment. **AND IT DISCHARGES `T94`: `T94-am2` ruled the residual defect is not in the ticket column but in the scorebug holding the old fixture across a boundary that has no treatment — this is that treatment, and `T94`/`T140-am`/`D2` are one seam.** (2) `T130`'s gate, the half that does not wait: A RENDERED LEG ROW IS NEVER EMPTY — the spec's own words are that it *would have caught arm 3 before it shot*. (3) **THE CORRECT-SCORE ARM'S COPY, unblocked since batch 158**: §4 lists that ending as *"nothing — the column is blank"* and blocked on `G1`'s unauthored kinds; `T161` disposed the nine and **`CorrectScore` is one of only two that CLEARS IN EVERY SLOT**, with its forms already authored at `T151`. (4) Gate 5, the executed-case count (`C29`).** **DOES NOT BUILD YET, AND ONE IS A TRAP: **GATE 1 — *no clock regression within a ticket* — WOULD BE WRONG UNDER ARM (B), WHERE THE CLOCK STILL RUNS BACKWARDS BY DESIGN** (`T140-cost`). Asserting it now pins the surface to arm (A) before Allen has chosen it, and **a gate that fails on a legal arm is not a gate, it is a vote.** Gate 2 has the same shape. §3.2's *every leg live for the whole telling* and *`NEXT` leaves the legs on this fixture* are both (A)'s consequences. `D1` and `D3` test the ruling and cannot be shot before the arm is known.** **`D2` IS THE EXCEPTION and is shootable as soon as item (1) builds, under either arm — it is the capture that discharges `T94`'s multi-fixture half.** **AND ITEM (1) MUST NOT BE READ AS LEANING TOWARD (B) because it shares a device with it: it is required under (A) too, by `T140-am`'s own scope statement, and this row rules nothing about the fork** | batch 159 |

---

## For the orchestrator

- **TV is unblocked on four items** without the fork being ruled — the biggest is `§6.7` at the
  fixture boundary, which is also `T94`'s remedy.
- **The terse copy is authored and goes to the lane for measurement**, not to a build.
- **Console spec (batch 121) is APPROVED and markets is building** — noted, nothing owed from this
  seat. The `SPECIMEN.txt` question was answered by Allen directly (illustrative; the spec's tables
  bind) and needs no row.
- **Backlog is 155–159.** Batch 151's `G1-am5` → `G1-am10` re-key is applied in the batch file, in
  `REGISTER.md`'s row, in its one citation and in the transcription log (`C22-am3`).
- **Three ID collisions remain and are not this seat's to re-key** — `G1-am2`, `G1-am3`, `G1-am4`
  (`C22-am3`).

## Limits

- **No width is asserted.** Every form in the terse-copy spec is a candidate for the lane to measure.
- **`T162` orders no capture and rules nothing about the fork.**
- **`G1-am11` assumes `T155`'s compact ladder.** Without it the compact rungs cannot be selected by
  measurement and those slots stay on the truncation floor.
