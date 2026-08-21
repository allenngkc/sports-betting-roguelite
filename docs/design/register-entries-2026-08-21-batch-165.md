# Register entries — batch 165 (2026-08-21)

**`T140` ARM A IS RULED BY ALLEN. The plan is written — and the useful half is what it does NOT
need: phase 2 needs no ruling at all, and four of phase 1's six inputs are already ruled.**

**One row.** **Destination table:** TV (`T140-am2`).

**Plan:** `docs/design/t140-arm-a-plan-2026-08-21.md`.

**Three rulings are owed from this seat and are named; nothing else in the plan waits on design.**

---

## The row

| T140-am2 | Arm A ruled — the plan, and the three rulings this seat owes before the phases can start | **PLANNED — DD 2026-08-21 batch 165, on Allen's ruling of the fork. `T140` stands as written; `T140-cost`'s LARGE is not re-costed and the phase order is TV's. What this row carries is the DESIGN state of each phase.** **RELEASED BY THE RULING, from the fork-independent split's §2: gates 1 and 2, *every leg on the fixture live for the whole telling*, *`NEXT` leaves the legs on this fixture*, and the `D1`/`D3` captures. **Two of those were UNASSERTABLE under arm (B) and would have been wrong there** — the clock runs backwards under (B) by design and the ending line fires once per leg — which is why they were held rather than built.** **THE GATES, WITH ONE CONDITION ON HOW THEY ARE WRITTEN: both must be *per (ticket, fixture)*, not *per ticket*. A gate reading *per ticket* FAILS A CORRECT MULTI-FIXTURE BROADCAST, where the clock legitimately resets between fixtures and the ending line legitimately fires twice — the exact over-reach `T140-am` corrected in the spec's §3.1. Gate 1 is arm A's own FALSIFIER: if the rendered clock is not non-decreasing per fixture, the restructure did not land.** **PHASE 2 NEEDS NO NEW RULING, and this is the plan's cheapest finding: `T115-am` (batch 109) deferred concurrent live count legs as *"NOT REACHABLE, SO NOT BUILT — the engine forbids two legs on one matchup"* AND PRE-COMMITTED THE PRINCIPLE FOR THE DAY THAT CHANGED — *"the scene takes the SMALLEST distance across live legs, which is the hand-over falling out of the same rule."* **`T142` STRUCK THAT PREMISE**: `SameMatchModel`, `SameMatchPrice` and `Ticket.SameMatch` all ship and SGP is delivered. **The deferral's condition has fired, so the principle stands unamended and phase 2 is a BUILD.** Its other clauses may not vary without a new ruling: the two-rung stepped shape, state-based quiet, ticket-derived valence, and the clock's exclusion.** **PHASE 1's INPUTS ARE FOUR-SIXTHS RULED — the telling contract (`T140`), grades in LEG ORDER after ONE hold (spec §3.2), the pending-loss window opening ONCE PER WHISTLE after every grade on that fixture and naming every leg that died (`T143`), and the cash-out as a TICKET-level fact with no leg's probability shown alone (`T143`). **The lane must not be asked to re-derive any of it.*** **THE THREE OWED, and they are this seat's: (1) THE PROSE ANCHOR UNDER N LIVE LEGS — `PickedHomeForPresentation` answers *which team the prose anchors on* and returns one side PER LEG; with N live on one fixture there are N candidates and no rule. Not `K17` and not `T152-am` — the question arm A creates. (2) THE DISPLAYED WIN-PROBABILITY'S SEED — `T143` NAMED this seam and deliberately left it: `_liveProb` seeds per-leg and `RevealedView.Reset` from `Legs[0]`, which `T143` called *"arguable"* sequentially and *"a VISIBLE LIE"* under N-live. **Arm A makes it N-live, so a reserved question becomes a defect the day phase 1 lands.** (3) THE LEG COUNTER — `LEG n/m` says which leg is live, and under arm A a fixture can carry several at once, so *which leg* stops being a single number. **ZERO register rows govern it** (searched at batch 158).** **AND (3) IS COUPLED TO `T91-cl`, WHICH IS THE OPERATIONAL POINT: `T91-cl` measured that counter's ink overprinting the scoreline by 41.7px and showed a clearance rule alone cannot fix it — 569.0px available against 583.3px needed, a 14.3px deficit — so its POSITION must change while arm A is changing what it SAYS. **LAND THEM TOGETHER; done separately the element moves twice and the second move re-opens the measurement the first one settled.*** **WHAT ARM A DOES NOT DISCHARGE: `T94`'s multi-fixture half. The same-fixture half goes by construction (`T94-am`) — under one telling there is no *next leg stages* moment inside a fixture — but the fixture BOUNDARY still has no `§6.7` treatment, that work is fork-independent and already routed, and `T140-am`'s scope statement that `T140` does not include it stands** | batch 165 |

---

## For the orchestrator

- **Phase 1 needs an ENGINE-OWNING LANE** — `engine/` is read-only to TV, so it is not executable
  there at all. Its design contract is the plan's §4.
- **Phase 2 can be scheduled without waiting on this seat.**
- **Phase 3 waits on one ruling** (the leg counter) and should be planned alongside `T91-cl`'s
  remedy, which TV already holds.
- **Three rulings owed here.** They come as their own batch and block only phases 1 and 3.
- **Backlog is 164–165.**

## Limits

- **Nothing is re-costed.** `T140-cost`'s LARGE and the three-phase shape are TV's.
- **Nothing is measured or shot in this batch.**
- **The plan does not narrow the spec** — everything Allen approved stays approved; it is split by
  dependency only.
