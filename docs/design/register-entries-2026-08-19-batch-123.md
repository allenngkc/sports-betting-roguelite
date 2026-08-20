# Register entries — batch 123 (2026-08-19)

**NINE OF FIFTEEN MARKET KINDS ARE UNAUTHORED ON THE TV — AND THE LAPTOP MADE THEM BETTABLE FOUR
BATCHES AGO.** Found at source while pre-committing arm 3's read, before the window shoots.
**`TvSweatScreen` authors six kinds; nine fall to a `default`. The live leg row goes BLANK and the
compact row prints a C# enum identifier.**

**Two rows.** **Destination tables:** TV (`T130`) · Cross-surface (`C56`).

**Pre-commit:** `docs/design/drawn-ending-precommit-2026-08-19.md` §0 — **written before the frames
exist**, and it makes shooting arm 3 **unauthored** a binding condition.

---

## Why this is a batch of its own and not a line in the window

**It is live now.** The surfaces phase closed at batch 119 with all fifteen kinds reachable and
takeable on the laptop's ENTRY sheet. **A player can back `CORRECT SCORE 0-0` today**, and if that
ticket reaches the theater the TV has no words for it. The window only happens to be the thing that
would have found it.

**And the third instance makes it a law.** Same expansion, same nine kinds, three surfaces, **three
different failure modes** — which is exactly why finding it twice said nothing about the third.

| surface | how the nine fail | found |
|---|---|---|
| laptop ENTRY | **homeless** — no reachable destination | `S86`, batch 100 |
| console | **unbettable** — printed, priced, and the parser has no token | `K2`, batch 121 |
| TV theater | **unauthored** — the row goes blank, or prints the enum name | `T130`, here |

---

## The rows

| T130 | Nine of fifteen market kinds are UNAUTHORED on the TV — the live row goes BLANK and the compact row prints a C# enum identifier | **FLAGGED under `C17` — source read, no frame; the capture is arm 3 of the window shooting now** · DD 2026-08-19 batch 123. **`TvSweatScreen` authors SIX kinds — Moneyline (incl. `T96`'s draw branch), TotalGoals, BothTeamsToScore, TotalCorners, TotalCards, AnytimeScorer. NINE fall through a `default`: DoubleChance · Handicap · TeamTotalGoals · TeamTotalCorners · TeamTotalCards · CorrectScore · WinningMargin · TotalGoalsOddEven · PlayerMultiScorer.** **TWO DEFAULTS, FAILING DIFFERENTLY, WHICH IS WHY THIS IS TWO DEFECTS AND NOT ONE.** **(1) THE COMPACT ROW: `LegStatement`'s `default:` returns `leg.DisplayLabel`, whose OWN `default:` is `selection.Kind.ToString()` — THE RAW C# ENUM NAME. A correct-score leg's row prints `CorrectScore`. Camel case, an identifier, on the player's screen.** **(2) THE LIVE ROW IS WORSE AND THE MECHANISM IS THE ROW'S OWN CORRECT DESIGN: `DescribeActiveLeg`'s `default:` returns `ActiveLegCopy(string.Empty, string.Empty, false, string.Empty)`, and the call site at `:2892` CLEARS the compact form deliberately — *"the live form replaces the compact one entirely — statement, price and chip all clear… the live row's NEED carries the statement"* (`T24`). So on the sweating leg the surface correctly removes the compact form and the NEED that should replace it DOES NOT EXIST. THE LIVE LEG ROW IS ENTIRELY BLANK — the player sweating a correct-score ticket sees an empty rail where his bet should be.** **THE DEBT IS NAMED IN SOURCE AND HAS A DATE ON IT: `MatchModel.DisplayLabel`'s own comment reads *"Retained behaviourally unchanged for `TvSweatScreen.cs` (another lead's surface, forbidden to this ruling's batch) pending that lead's own migration to `Fields`."* `S22`'s migration reached the console and the laptop and NEVER REACHED THE TV** — the legacy packed string is still this surface's fallback, four months on. **NO VERDICT IS TAKEN UNTIL THE FRAME LANDS** — `S86` and `S93` both sat here and the discipline is the same; the source read may be wrong and I have not traced every branch of the describe path. **PRE-COMMITTED AT `drawn-ending-precommit-2026-08-19.md` §4.1 so the frame can overturn it. AND THE BINDING CONDITION THAT FOLLOWS: ARM 3 IS SHOT AS IT STANDS — no correct-score form is authored before the shoot, because the unauthored state IS the evidence and authoring copy from the build is the failure `A2` and `T96` both exist to prevent (*a copy ruling lands in the deck or it has not landed*).** **NOT AUTHORED HERE AND MUST NOT BE SMUGGLED INTO THIS PHASE: nine kinds × two forms is `G1`'s ladder and a deck pass of its own, sized once the measurement lands** | batch 123 · precommit §0, §4 |
| C56 | A SWITCH over a growing enum is a FIXED BOX in a different unit — and every surface's `default` becomes reachable on the same day | **Law (register-level, `C46`/`C39`/`C42`/`C43`'s standing — not proposed for the constitution)** · DD 2026-08-19 batch 123, **promoted from three instances in one week and not from an argument.** **WHERE A SURFACE SWITCHES ON AN ENUM THE ENGINE OWNS, THAT SWITCH CARRIES AN IMPLICIT CLAIM — that its cases cover the enum — AND THE CLAIM IS NEVER WRITTEN DOWN. When the enum grows, EVERY such switch's `default` arm becomes reachable AT THE SAME INSTANT, on every surface, silently.** `C46`'s converse in a different unit: **a fixed box assumes a widest string; a fixed switch assumes a closed enum. Neither assumption is stated and nobody re-derives what they never noticed they declared.** **THE CONSEQUENCE THAT COST THIS STUDIO THE MOST, and it is the half that will be missed: THE SURFACES FAIL DIFFERENTLY, SO FINDING IT ON ONE IS NOT EVIDENCE ABOUT THE OTHERS AND FIXING IT ON ONE CLOSES NOTHING ELSEWHERE.** F_0.5.0 added nine members to `MarketKind`; the laptop's ENTRY went **homeless** (`S86`), the console went **printed-but-unbettable** (`K2`), the TV went **blank-or-enum-name** (`T130`). **Three surfaces, three symptoms, one cause — and each was found separately, weeks apart, by someone looking at that surface for another reason.** **THE TEST, and it is `C46`'s clause (1) transposed: SWEEP THE POPULATION, NOT THE SUSPECTS — after any enum the engine owns gains a member, the set at risk is EVERY SURFACE THAT SWITCHES ON IT, not the one where the defect was noticed.** **AND THE SECOND CLAUSE, from `T130`'s own shape: A `default` THAT RETURNS SOMETHING IS MORE DANGEROUS THAN ONE THAT THROWS.** `LegStatement` falls back to a legacy label and `DescribeActiveLeg` falls back to empty strings — **both ship, neither fails, and a build that prints `CorrectScore` at the player is green.** A `default` arm is a place a surface promises to have thought about a case it has not seen. **Relation to the neighbours: `C19` says a priced offer is reachable; `C56` says the surface that receives it must have WORDS for it — reachability and authorship are two different debts and closing one does not close the other** | batch 123 · precommit §0 |

---

## What is NOT in this batch

- **No authoring.** The nine kinds' forms are `G1`'s ladder and a phase of their own. **Sizing them
  before `T130`'s frame lands would be authoring against a source read**, which is the thing `C17`
  exists to stop.
- **No verdict on `T130`.** It is flagged, not ruled. Arm 3 settles it and the pre-commitment is
  written.
- **No claim that the laptop or console is worse or better.** `S86` is closed, `K2` is spec'd and
  with Allen; this row is the third surface and it is the one nobody had looked at.
- **Nothing about the drawn ending itself.** Batch 122 carries that and none of it is touched here.
