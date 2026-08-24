# Register entries — batch 180 (2026-08-24)

**The sim's exposure, posed as ONE question — and it is far smaller than *"the sim bets this
market"*: two of the three consumers never bet it, they explain why they cannot PRICE it.**
The unknown was never volume. It is whether a refusal class loses its only constructor.

**One row.** **Destination table:** TV (`T161-am3`).

**Four source reads, each quoted from the code's own comment. Nothing measured, nothing estimated —
the estimate is the sim lane's and this row is the brief for it.**

---

## The row

| T161-am3 | The sim's exposure SIZED to one question — and the through-line is PLAYER-FACING GOES, INTERNAL STAYS | **BRIEF for the re-seated engine/sim lane · DD 2026-08-24 batch 180, on Allen's order to size it with them. **Allen's KEEP on the enum member is taken and matches the removal spec's §1.*** **NO EXPOSURE — `SkilledStrategy` AND `Analysis`. Neither BETS DoubleChance. `SkilledStrategy.cs:713-715`'s `CanPrice` EXCLUDES it from pricing alongside `AnytimeScorer`, `PlayerMultiScorer`, `CorrectScore` and `WinningMargin`, and `Analysis.cs:596-599` carries `G7`'s required rationale — *"its three selections OVERLAP — 1X and X2 both contain the draw — so normalizing the implied probabilities is double counting, not de-vig."* **Removing the offer makes the exclusion vacuous and the report line inert. IT DOES NOT MAKE THEM WRONG.*** **AND THIS IS WHERE I CORRECTED MY OWN DRAFT, because `C61` looked like it reached here and does not: `C61` removes a taxonomy row for an unofferable market **because a player-facing entry TEACHES A MARKET THAT DOES NOT EXIST.** `CanPrice`'s entry and `Analysis`'s reason are **INTERNAL** — a studio report, not a board — so they misinform nobody, and `T82-d`/`K18` govern instead: **an unreachable-but-correct guard is KEPT and its quietness recorded, never deleted for currently guarding nothing.** They also sit exactly where a revival would be made. **THE THROUGH-LINE: PLAYER-FACING GOES, INTERNAL STAYS** — and it is the same line Allen's KEEP on the enum draws.** **TRIVIAL — `SameMatchStrategy` CASE 2, *"THE RESULT SPINE"* (`:414-431`): a one-goal home win said four ways — `1X ⊇ 1`, the away `+line` covering a one-goal loss, the margin bucket exactly one, and the moneyline itself. Its own comment: *"heavy overlap is deliberate — these four kinds all read the same scoreline, which is precisely the correlation the joint exists to price."* **Drop DoubleChance and the case SURVIVES ON THREE**; Handicap, WinningMargin and Moneyline all remain and all still read that scoreline. Sizing: delete one `Pick`. **Coverage narrows and is NAMED rather than quietly lost — the widest overlap the joint is tested against goes from four kinds to three.*** **THE REAL QUESTION, AND IT IS THE ONLY ONE — `SameMatchStrategy` CASE 3 (`:645-656`). Its own comment: *"IMPOSSIBLE, by EXCLUSION: 12 is precisely 'not the draw', so the draw beside it wins on no outcome at all… **the one refusal cause on the board that is a SET-COMPLEMENT rather than an arithmetic conflict.**"* **CHECKED AT THIS SEAT: `SameMatchStrategy` exercises exactly TWO refusal causes — this one and *IMPOSSIBLE, by a FIXED TOTAL* (`:658`, a correct-score cell). So the removal HALVES the exercised refusal classes, two to one, and the survivor is the arithmetic one.*** **SO THE SIZING IS ONE QUESTION, AND THE LANE CAN ANSWER IT IN A READ: **CAN ANY SURVIVING MARKET PAIR CONSTRUCT A SET-COMPLEMENT CONTRADICTION?** If YES, case 3 re-authors from it and the removal is small at every site. **If NO, the refusal CAUSE stays correct in the engine and becomes UNREACHABLE — and that returns to THIS seat as a finding rather than sitting in an estimate**, because `K18`'s discriminator then governs: the branch is kept, its unreachability is recorded, and it is not deleted for being quiet.** **WHAT IS NOT ASKED FOR: an estimate of the whole removal. Three of the four sites are counted above and none is more than a deletion. **The unknown was never the sim's volume** | batch 180 |

---

## For the orchestrator

- **This row IS the sizing brief** — the re-seated lane needs no other document.
- **One question back**: can any surviving market pair construct a set-complement contradiction? A
  yes closes the sizing; **a no comes to this seat**, not into the estimate.
- **Allen's KEEP is taken** and extends further than the enum: the sim's internal exclusion entries
  stay too, on `T82-d`'s ground.
- **Backlog is 173–180.**

## Limits

- **Nothing is estimated here.** Every claim is a read of the sim's own comments, quoted.
- **"Exactly two refusal causes" is a count of `SameMatchStrategy`'s `IMPOSSIBLE, by` cases** — it is
  what that file exercises, not necessarily every cause `SameMatchModel.Refuse` can raise. **The
  distinction matters and the lane should confirm it**, because a cause with no test case is exactly
  what this question is about.
- **`C61` is not extended.** It governs player-facing taxonomy; this row states why it stops there.
