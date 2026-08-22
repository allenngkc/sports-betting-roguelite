# Register entries — batch 171 (2026-08-21)

**`K17` is fixed and verified — and the lane found my ruling named a function that CANNOT BE CALLED.
It read the ruling for its SHAPE rather than working around it, and that reading is now the standing
one.** Plus a second live defect found by building, and my own spec inverted by a field list.

**Two rows.** **Destination tables:** Console (`K17-cl-vf`) · TV (`T163-am2`).

**Merged:** `15ad83d` (the fix) · `888cc6d` (the phase handoff). **Spec amended:**
`spec-neither-branch-lines-2026-08-21.md` §5.

---

## The rows

| K17-cl-vf | `K17` FIXED and VERIFIED — and my ruling named `SideOn`, which short-circuits for exactly the kinds the ruling is about | **DESIGN-VERIFIED · DD 2026-08-21 batch 171, on markets' report at `15ad83d` and a source read at this seat.** **THE DEFECT ON A REAL FRAME, exactly as `K17-cl` predicted it: backing `TUSCALOOSA MIDDLEMEN +1.5` — Handicap, the AWAY side — narrated every beat as `DULUTH TURNIPS` (*"Turnips slot it home"*, *"Goal for Turnips"*) **while the leg's own verdict row named MIDDLEMEN.** Two zones of one surface disagreeing about whose side he is on, which is what the ruling called `T59`'s family.** **§1.5 — MY RULING NAMED A CALL SITE THAT DOES NOT EXIST. `K17-cl` said *"the anchor takes the backed side from `BetslipModel.SideOn`"*. **`SideOn` SHORT-CIRCUITS `if (Kind != Moneyline) return null`, so it answers NEITHER for ALL FIVE side-carrying kinds the ruling is about.** Called literally it would have removed the false HOME anchor without ever naming the correct side — the lane's phrase is the right one: **a half-fix wearing the ruling's clothes.** Its signature also scans a slip by matchup index while `EventText` holds one `Leg`, and `Pick.Side`/`Leg.Side` both THROW for non-moneyline.** **I RULED IT FROM A DOCSTRING. `PickedHomeForPresentation`'s comment describes `SideOn` as *"reports WHICH SIDE YOU BACKED"* and I took a description for a call site. **`C58-am` verbatim — a docstring is not a measurement** — and it is the third time this rotation a check that was one grep away was replaced by a sentence about it.** **THE SALVAGE IS BLESSED AND IS THE STANDING READING FROM HERE: a ruling that names `SideOn` names the SHAPE of the answer — `Side?`, where the honest answer can be NEITHER — and not a call site. The lane read it that way, built the console its own per-selection function, and REPORTED the gap rather than quietly routing around it.** **`EventText.BackedSide(MarketSelection)` ENDORSED, and the REJECTED alternative is the part worth recording: the elegant `s.Team ?? ChoiceToSide(s.Choice)` is correct for all fifteen kinds today and was refused because **it would silently answer for a sixteenth — *which is exactly how this defect happened*.** That is `T130-am`'s silent-`default` class and `T158`'s unreachable-branch class, refused at authoring time instead of found later. The shipped function is exhaustive over fifteen kinds and THROWS on an unknown one.** **A SECOND LIVE DEFECT, FOUND BY BUILDING THE FIX RATHER THAN BY LOOKING FOR IT: THE MONEYLINE DRAW WAS ANCHORED ON AWAY. The struck predicate sent `Choice.Draw` to the away club, and the draw row is printed and bettable on this surface today. **That is a live `T96` violation** — the row that ruled a draw ticket must never borrow a team's — and it is now NEITHER, on the engine's own *"Has no Side by construction"*.** **THE GATE IS A MODEL AND SHOULD BE COPIED: its assertions RECONSTRUCT the expected beat rather than sighting a string, **because a beat may legitimately name the opponent and containment cannot see this defect.** 2,040 priced selections, 5 of 5 side-carrying kinds reached on AWAY, 9,504 AWAY-backed beats reconstructed, 20,736 anchored and 52,704 neither beats swept — and **MUTATION-TESTED**: reinstating the struck predicate fails 2 tests, a silent `_ => Side.Home` fails 1. That is `C40`'s *the proxy is not the property*, answered inside a test rather than argued.** **`PickedHomeForPresentation` untouched and nothing under `unity/` or `engine/` modified, as the ruling required. THE CONSOLE IS PLAYER-READY ON THIS AXIS** | batch 171 |
| T163-am2 | `spec-neither-branch-lines` INVERTS — §1 is unimplementable on BOTH surfaces, §3 is what ships, and the line set is authored in full | **AMENDED — DD 2026-08-21 batch 171, §1.5, on the markets lane's field read. Spec §5 carries the corrected set.** **`DramaEvent` CARRIES `LegIndex`, `Step`, `TotalSteps`, `Type`, `WinProbAfter` AND `Tag` — AND NO ACTOR.** No scorer, no possession side. **So §1's slot change — which that spec called *"the real mechanism"* — has nothing to read, and it is unbuildable on the TV as well as the console without an engine change.** **§1.5: I told the lane §3 might be *"dead on arrival"* and to DELETE it if the event carried an actor. **It carries none. §3 is the only part that ships, and the section I called a fallback is the section that survives.** I asserted what a type carries without reading its fields, which were one grep away — `C59` again, and the second instance in this batch.** **WHAT SURVIVES AND WHY IT IS NOT A LOSS: §2's phrases hold even though its templates do not. *"score against the slip"* was authored precisely because it states the goal works against the ticket WITHOUT NAMING A SIDE IT WORKS FOR — **that is MORE true with no slot than with one**, and the console's `a goal against the slip.` is the phrase landing where it always belonged.** **THE TWO ASSEMBLED LINES ARE ENDORSED AS AUTHORED. `a goal — the number ticks with it.` takes `ScoreUp`'s own shipped clause and swaps its club-naming subject for the file's own club-free one (`a goal in the churn`, the scorer branch); `a goal against the slip.` is §2's phrase. No new idiom was introduced, the derivation is named per field, and both are flagged `ASSEMBLED, NOT AUTHORED` in source — **which is the opposite of the silent-default class and should be read as the discipline it is.*** **ONE REAL DEFECT IN WHAT SHIPPED, AND IT IS MINE TO FIX: EACH GOAL TABLE HAS ONE VARIANT. `variants[step % variants.Length]` on a single-element table means **every goal beat in the branch reads identically** — the other tables carry two or three. §5 authors two more for each, plus a third momentum variant per direction.** **AND §4's CASING RULE IS CORRECTED. *"Match the table they join"* was written for one file and produced the wrong result on transfer: the momentum lines went to `EventText.cs` capitalised while its club-free copy is lowercase, leaving four lines in one branch split two and two. **THE RULE IS: A CLUB-FREE LINE TAKES THE CASING ITS OWN FILE USES FOR CLUB-FREE LINES** — a table whose other lines open with an interpolated club noun has no casing of its own to match. On the console the whole branch is lowercase** | batch 171 |

---

## For the orchestrator

- **`K17` is closed.** The console is player-ready on the anchor axis; the moneyline draw is fixed in
  the same commit.
- **A CROSS-LANE ITEM FOR TV, and it reverses what I told them:** **§3 of
  `spec-neither-branch-lines` must NOT be deleted.** `DramaEvent` carries no actor on the TV either,
  so §1 is unbuildable there too. TV should not wait on an answer about a possession side — **there
  isn't one.**
- **The console's neither-branch wants §5's added variants** — a one-line table repeats every beat.
- **Two spec corrections received from markets** and not yet applied: §3's leader-dot count is one
  low (16, not 15 — `RowGeometry.OfferRow` uses a one-space gap where `Page.Leadered` uses two), and
  §14's `B4` folio cannot read `66–83 of 84` at the shipped geometry (`BodyRows` is 20). **Neither
  changes a ruling**; both are amendments to an Allen-approved spec and I will make them as one pass.
- **The over-80 docket stands** (batch 166) and markets has now measured it: 422 lines, shop relics
  to **214 columns**, one sweat screen at **32 rows**, and the sweat command hint at 82 — the last
  of which sits inside the DD's own `console-read-2026-08-19/` set.

## Limits

- **No console frame was read at this seat.** `K17-cl-vf` rests on markets' report plus source reads
  of `EventText.cs`, `BetslipModel.SideOn`'s short-circuit and the draw branch.
- **The gate's numbers are markets'**, not re-run here.
- **§5's added lines are unmeasured** — they are shorter than or comparable to what ships, and the
  console's gate is `line.Length <= 80`, which is the lane's to assert.
