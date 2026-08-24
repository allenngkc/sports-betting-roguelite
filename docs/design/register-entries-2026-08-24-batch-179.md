# Register entries — batch 179 (2026-08-24)

**An offer-less kind LEAVES the taxonomy — and §4.1 is not amended, it is BOUNDED. Its case is a
group empty for a MATCHUP; this is a market empty BY CONSTRUCTION, and they are different facts.**

**One row.** **Destination table:** Cross-surface (`C61`).

**One source read and one law re-read. Nothing measured.**

---

## The row

| C61 | Transiently empty PRINTS; permanently empty LEAVES — the taxonomy is what the game offers | **Law (register-level, `C46`/`C57`'s standing — not proposed for the constitution) · DD 2026-08-24 batch 179, on the question `T161-am2`'s removal spec exposed. Allen's lane assignment rides it.** **THE QUESTION: with DoubleChance offering nothing, does `DOUBLE CHANCE` still print as a labelled kind inside `RESULT` — a group that stays populated by Moneyline, Handicap and WinningMargin? `MarketDestinations.KindsOf` filters `TableOrder` by DESTINATION and never by offer count (`:140-143`), so today it would.** **§4.1 DOES NOT ANSWER IT, AND THE REASON IS THE RULING: *"Empty groups still print — `CORNERS … no prices offered`. **A racecard prints the race even when it is abandoned**, and it means the destination set is a constant — it never varies by matchup, so the page is authored once and never reflows."* **THAT WAS RULED ABOUT A GROUP EMPTY FOR ONE MATCHUP. It was never asked about a market removed from the game, because until (b) there had never been one.*** **THE ANALOGY BREAKS EXACTLY THERE, AND IT IS THE WHOLE RULING: an ABANDONED race is on the card and was not run TODAY. **A market removed from the offered set is a race THE MEETING NO LONGER HOLDS — and a racecard does not print those.** Printing `DOUBLE CHANCE … no prices offered` in every matchup forever does not report an absence; **it teaches a market that does not exist**, and it advertises to the player a thing he can never take, which is `S85`'s ground — *an offer that cannot be taken is not an offer.*** **AND §4.1's LOAD-BEARING REASON SURVIVES REMOVAL INTACT, which is what makes this safe rather than a trade: the constancy clause says the set *"never varies BY MATCHUP, so the page is authored once and never reflows."* **A SMALLER CONSTANT IS STILL A CONSTANT.** `TableOrder` is a `static readonly` array (`:125`); dropping a member leaves it just as invariant across matchups, the page still authored once, still never reflowing. **§4.1's layout argument does not require keeping the kind — it requires the set not to vary in play, and it does not.*** **RULED, AS A DISCRIMINATOR SO THE NEXT REMOVAL NEEDS NO RULING: **EMPTY FOR A MATCHUP → PRINTS**, with §4.1's `no prices offered`; the taxonomy is the constant and the emptiness is DATA. **EMPTY BY CONSTRUCTION → LEAVES THE TAXONOMY**; the taxonomy is what the game OFFERS, and a kind that offers nothing in any matchup is not part of what the game offers. **The test is not *is it empty* but *can it ever be non-empty*.*** **SO `DOUBLE CHANCE` COMES OUT OF `TableOrder`, OUT OF `KindsOf(Result)`, OUT OF `For()` AND OUT OF THE LABEL MAP (`:161`) — the contents block and the destination page follow from those and need no separate edit.** **§4.1 IS NOT AMENDED. Its sentence is correct for its case and this row does not touch it; what is added is the boundary it never had, and `T161-am2`'s removal is the first thing to reach it.** **THE SCOPE CONSEQUENCE ALLEN ASKED FOR, and it is the reason this ruling sizes the lane: **the removal now touches `MarketDestinations.cs` as well as `MatchModel.cs`, and BOTH SURFACES re-derive from it** — the laptop's ENTRY sheet and the console's contents page. **Three fewer offers per matchup means the console's matchup-global line numbers RENUMBER below `RESULT`, and §5.1 rules those numbers ARE the pick addresses** (*"the printed line number is also the ADDRESS — and this is the whole design"*). Nothing persists an address, so nothing breaks — **but every pin on a count, a range or a folio moves with them, and §13's gates should be RE-RUN rather than assumed** | batch 179 |

---

## For the orchestrator

- **The taxonomy ruling and the removal now have one scope**, as Allen said: `MatchModel.cs` (three
  offers) **plus** `MarketDestinations.cs` (`TableOrder`, `For`, the label) **plus** a re-run of
  §13's gates on both surfaces.
- **The address renumbering is expected, not a defect** — but it is the part most likely to be read
  as one when a gate moves.
- **The sim's exposure is still unsized** (`T161-am2` §4) and is the only unknown left before
  scheduling.
- **Backlog is 173–179.**

## Limits

- **`C61` rules the DISCRIMINATOR, not the mechanism.** Whether the kind is deleted from
  `TableOrder` or gated out of it is the lane's call; the ruling is that it must not print.
- **The renumbering claim rests on §5.1's own words plus `KindsOf`'s unconditional filter.** I did
  not run the sheet or count the shipped offers.
- **§4.1 is untouched.** If a group ever becomes permanently empty, this row's discriminator reaches
  it too — but no group is in that position today.
