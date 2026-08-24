# Register entries — batch 181 (2026-08-24)

**Both corrections taken, and they close the sizing rather than extend it: the cause stays reachable,
so the reservation I held back does NOT fire and nothing returns to this seat.**

**One row.** **Destination table:** TV (`T161-am4`).

**Four source reads. The corrections are the sim lane's; the errors are mine.**

---

## The row

| T161-am4 | `T161-am3`'s count and its uniqueness claim, both CORRECTED — and the sizing closes with them | **CORRECTED — DD 2026-08-24 batch 181, §1.5, on the sim lane's sizing. **`T161-am3`'s own caveat named this risk and the lane collected on it**, which is the caveat working rather than an excuse.** **(1) THE COUNT IS FOUR, NOT TWO, and the sites are: `:625` *IMPOSSIBLE, ON THE SCORELINE* (BTTS-yes beside UNDER-low) · `:635` *DUPLICATE* (the same corner line twice) · `:645` *IMPOSSIBLE, BY EXCLUSION* (DoubleChance's 12 beside the draw) · `:658` *IMPOSSIBLE, BY A FIXED TOTAL* (a correct-score cell).** **MY INSTRUMENT MATCHED A STRING, NOT THE PROPERTY: I grepped the literal `IMPOSSIBLE, by` and reported the hit count as *the refusal causes this file exercises*. **It missed `:625` on a PREPOSITION — *on* the scoreline, not *by* — and missed `:635` entirely because a duplicate is a different RULE and never says *impossible*.** That is batch 161's shape inverted: that scan OVER-matched on possessives, this one UNDER-matched on a preposition, and both are a pattern asserting a shape the corpus does not guarantee.** **(2) THE UNIQUENESS CLAIM IS FALSE AS I USED IT, and the distinction is exact: the code's own comment — *"the one refusal cause on the board that is a SET-COMPLEMENT rather than an arithmetic conflict"* — is DEFENSIBLE STRICTLY, because `12` beside the draw is a true complement while BTTS-yes beside UNDER-low is merely DISJOINT (2–0 lies in neither). **I used it as *the only constructor of an EMPTY INTERSECTION*, and `:625` is exactly that without any complement relation.** The claim I quoted was narrower than the claim I built on it.** **AND I CONFLATED RULE WITH CAUSE, WHICH THE FILE ITSELF ALREADY SEPARATES — `SameMatchStrategy.cs:577`: *"Only two RULES are reachable … **but a rule is not a cause**."* `RefusalKind` has THREE members (`ImpossibleCombination`, `DuplicateSelection`, `SubEvens`); the four sites above are FOUR CONSTRUCTIONS of two of them. **My row said *refusal classes* and counted constructions.*** **SO THE SIZING CLOSES, AND MY OWN RESERVATION DOES NOT FIRE: `ImpossibleCombination` REMAINS REACHABLE after the removal — `:625` and `:658` both construct it without DoubleChance. **`K18`'s unreachable-cause finding does NOT apply, and nothing returns to this seat.** What the removal costs is ONE CONSTRUCTION STYLE — the strict set-complement — not a cause, not a rule, and not coverage of the refusal path.** **THE CORRECTED SIZE, unchanged in shape and now with no unknown: two INERT internal entries KEPT (`T82-d`, and `T161-am3`'s player-facing-goes/internal-stays line stands) · one `Pick` deleted from case 2 · case 3 re-authored from a surviving disjoint pair or retired, **the lane's call and no longer a blocker.*** **AND ONE THING CHECKED BEFORE CLAIMING IT, recorded because the check is the point: I was about to raise `SubEvens` as *the refusal cause with no test case* — the very thing my question was hunting. **THE LANE ALREADY DOCUMENTS IT**: `Report.cs:164-171` and `SameMatchStrategy.cs:577` state it reads zero at the shipped `κ = 1` BY CONSTRUCTION and needs `κ ≳ 1.3`. **It would have been the fifth re-raise this rotation that a check caught first** | batch 181 |

---

## For the orchestrator

- **The sizing is CLOSED.** No unknown remains and nothing comes back to this seat; the engine
  lane's build does not wait on design.
- **`T161-am3` stands except for its count and its uniqueness sentence** — its dispositions (internal
  stays, player-facing goes; case 2 trivial) are unchanged.
- **Backlog is 173–181.**

## Limits

- **The four sites are `SameMatchStrategy`'s annotated cases**, read at this seat. Whether other sim
  files construct refusals is not examined — and that is the same gap that produced the error being
  corrected, so it is named rather than assumed closed.
- **Whether the strict-complement construction is worth preserving is the lane's judgement**, not a
  design ruling; this row only removes the claim that it was the only one of its kind.
