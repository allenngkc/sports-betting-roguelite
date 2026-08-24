# Register entries — batch 177 (2026-08-24)

**Allen ruled (b): DoubleChance leaves the offered set. The removal is spec'd — and the interim is
DO NOTHING, because the form everyone assumed was shipping was never built.**

**One row.** **Destination table:** TV (`T161-am2`).

**Spec:** `docs/design/spec-doublechance-removal-2026-08-24.md`.
**Six source reads, cited to lines. Nothing measured, no frame.**

---

## The row

| T161-am2 | DoubleChance's REMOVAL spec'd — three lines leave, the ENUM STAYS, and the interim is nothing because nothing was ever built | **SPEC'D on Allen's ruling of option (b) · DD 2026-08-24 batch 177. `docs/design/spec-doublechance-removal-2026-08-24.md`. The lane assignment is Allen's.** **WHAT LEAVES: three unconditional calls at `engine/MatchModel.cs:160-162` — `HomeOrDraw`, `AwayOrDraw`, `HomeOrAway`. **Not config-gated**: unlike `HandicapLines` or `TeamGoalLines` there is no line list to empty, so the removal is those three lines.** **WHAT DOES NOT LEAVE, AND THE FIRST REASON BREAKS A SAVE IF IGNORED: the ENUM MEMBER and the grading/pricing arms (`:321`, `:460`, `:634`, `JointModel.cs`) all STAY. (1) **An in-flight run's DoubleChance legs must still GRADE — removing an offer is not removing a market's ability to SETTLE**, and conflating them strands every ticket already placed. (2) `EventText.BackedSide` is EXHAUSTIVE over fifteen kinds and THROWS on an unknown one — `K17-cl`'s deliberate design, chosen over a one-liner so a sixteenth kind could not be answered silently; **deleting a member turns that safety into a crash.** (3) **The kind died this way BEFORE** — `Domain.cs`'s own docstring: *"Dead under the no-draws constraint… alive since Allen lifted it 2026-08-12"* — so death-by-not-offering is the precedent already in the code, and the removal is RECORDED IN THAT DOCSTRING beside the previous death rather than in a config knob nobody reads.** **`C57`'s DISCRIMINATOR, ANSWERED IN ITS OWN THREE LINES: **BUILD — nothing to do**, `LegStatement` has NO DoubleChance arm (verified, zero hits) and the kind already falls to `default: leg.DisplayLabel`. **DECK — the forms LEAVE** (`T152`'s two, `G1-am11`'s `{CLUB} UNBEATEN`). **POOL — they leave `TvExtentSweep`**, and this is the line that bites: `C57-am` rules the pool follows what the DECK authors, so a deck entry for an unofferable market puts strings in the pool the surface can never print — *"a pool holding a string the code CANNOT emit is FABRICATED and its sweep is vacuous."* **REGISTER — everything STAYS**, so a revival re-authors from a record rather than from nothing, and that record already carries the finding that would govern it.** **THE INTERIM IS *DO NOTHING*, AND IT IS A RULING RATHER THAN A SHRUG — because the premise everyone was working from is false. **`{CLUB} UNBEATEN` WAS MEASURED BUT NEVER BUILT**; there is no arm, so nothing ships that needs reverting and the interim is not *a false statement until we fix it*. It is `T130`'s unauthored-kind fallback — **already ruled** at `T130-vf`, covering nine kinds, of which this is one and is about to be none. **Build no copy for a market that is leaving.*** **THE CONDITION THAT MAKES IT AN INTERIM RATHER THAN A DEFERRAL: a ruled violation about to be deleted is acceptable; one quietly postponed is not. **If (b) is not built within this phase, option (a)'s club-alone NEED returns as the stopgap and this seat is told rather than left to assume.*** **AND THE SIZE IS NAMED SO IT IS NOT DISCOVERED: **the SIM BETS THIS MARKET** (`SameMatchStrategy`, `SkilledStrategy`, `Analysis`), so removal reaches the economy gates — **the largest unknown here and not this seat's to size.** `MarketDestinations` keeps `DOUBLE CHANCE` in `TableOrder` and `KindsOf(Result)` regardless of offers (`:140-143` filters by destination, never by offer count), so the removal must also decide whether the kind leaves the TAXONOMY — **which the console spec's §4.1 answers for empty GROUPS and not for empty KINDS.*** **CHECKED AND NOT TRUE, recorded because it would have been a neat saving: the `RESULT` group does NOT go empty — DoubleChance shares it with Moneyline, Handicap and WinningMargin — so **`K18`'s `no prices offered` state is NOT made reachable by this and its forcing hook is still owed** | batch 177 |

---

## For the orchestrator

- **The spec is written; the lane assignment is Allen's**, as he said.
- **Before scheduling, the SIM lane should size §4** — it is the only part with an unknown, and it
  reaches the economy gates.
- **One question the removal exposes and does not answer:** whether an offer-less KIND leaves the
  taxonomy. §4.1 covers empty groups only. **It wants a ruling before the build, not during.**
- **`K18` is unaffected** — its forcing hook is still owed.
- **Backlog is 173–177.**

## Limits

- **Nothing measured and no frame read.** Every claim is a source read cited to a line: the offer
  site, the absent deck arm, `BackedSide`'s exhaustiveness, the enum docstring, `KindsOf`'s filter,
  and the `Result` grouping.
- **The sim's exposure is NAMED, not sized.** Three files reference the kind; what removing its
  offers does to the gates is the sim lane's to estimate.
- **`T161-am`'s measurements are unchanged and are not re-run here** — this row disposes of the
  market, not of the numbers that condemned it.
