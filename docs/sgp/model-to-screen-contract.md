# SAME MATCH — the model→screen contract

**For:** the `surething-ui` seat taking P4–P6 · **From:** sgp lane (owns the slip model) · **2026-08-14**
**Ownership:** Option C — this lane owns `BetslipModel`, `surething-ui` owns the screen.
**Laws:** `docs/design/surething-design.md` §3.3 + the 2026-08-12 amendment, S73-am4, batch 48.
This document does not restate the laws. It says which call answers each one.

The model half is merged and green (291 headless tests). **You should not need to read
`BetslipModel.cs` to build the screen** — if you do, that is a gap in this document, so say so.

---

## The price — already correct, probably nothing to do

`CombinedOdds` still exists and `SportsbookApp.cs:995` already renders it. **Its computation changed
underneath you**: it is now the engine's ticket price, not a product of leg odds. `TicketOdds` is the
same figure under a name that says what it is; `ToWin` is `Stake × TicketOdds`.

- For any slip with **at most one leg per matchup** the value is **bit-identical** to before — proven
  with `==` over 4,800 slips, not to a tolerance. Ordinary tickets cannot have moved.
- For a same-match slip it is the joint price.
- **The model no longer computes a product-of-legs anywhere.** That is deliberate: S73 forbids the
  surface to display one, and the safest way to guarantee that is for the number not to exist.

So there is no "adjustment", "was/now", or deduction to suppress — there is nothing to suppress *from*.

## The mark — `IsSameMatch`

`IsSameMatch` is true when some matchup carries two or more legs. That is when `THE HOUSE'S LINE` is
drawn on the connected picks. **Drawn, not captioned** — the name never prints beside it.

## The statement — `SameMatchPricing`

`SameMatchPricing` is the `SameMatchPrice` for the current slip (null when not same-match). It carries
the relations and a nominated **`principal`**.

Batch 48 says the slip states the relation **once**. A four-leg same-match slip can carry six
pairwise relations, so the model nominates which one — that choice is a claim about what moved the
price, and only the pricing layer can make it. **Use `principal`; do not pick from the list
yourself.**

The model emits **structured relation data and no English**. Composing the sentence is yours. The
relation kinds are `MutuallyExclusive`, `Implies`, `SharedScoreline(sign)`, `SharedCount(family,
sign)`, `ScorerOfSide(side)`, `Independent`. `Independent` is never principal — there is nothing to
state — and `MutuallyExclusive` never reaches a placed slip because it is a refusal.

**Lengthening is not remarked.** A correlated price that pays *more* gets no badge.

## The refusal — `Refusal`, and two traps

`Refusal` is a `TicketRefusal` or null, computed **before commit**, without throwing. It is what turns
a Blocked control into a stamp with cause **and** remedy. `CanPlace` is false while it stands.
`PlaceBlocker` still returns its legacy strings for the ordinary blockers ("pick a side", "max N
tickets", "betting is closed") — those are unchanged.

Three kinds: `ImpossibleCombination`, `DuplicateSelection`, `SubEvens`. `CauseLegs` is the **minimal**
conflicting set — a two-leg conflict inside a four-leg slip names two legs, not four.

**Trap 1 — the remedy is a SET, and it is plural TODAY.** Not a contingency on the margin dial: on the
merged 15-market board, remedies of up to **three legs** occur at the shipped `κ = 1`, measured over
645 refusals. A surface that spends `RemedyLegs[0]` leaves the slip **still refused**. Spend the whole
set. The stamp copy has to read naturally for one leg *and* for several.

**Trap 2 — remove high index to low.** Removing an earlier leg first shifts the indices of the later
ones.

**The stamp copy is CONJUNCTIVE — DD ruling, batches 66–67.** Both halves state the whole set as one
instruction. A remedy must read *drop all of these*, never *drop any of these*: a menu-shaped remedy
fails when followed, because dropping one element of a three-leg remedy leaves the slip refused. And
the cause is **N-valued** — "cannot both land" is wrong, since three legs can be jointly impossible
with every pair among them fine. The sentence is *these cannot all win*.

The engine's own composed text already conforms — its leg lists join with "and", never "or", and the
impossible cause reads "These legs cannot all win". Verified 2026-08-14; no disjunctive phrasing
exists in any refusal-facing string.

Every one of those 645 remedies placed successfully after being spent, so a spent remedy is a
guarantee, not a suggestion.

## Reading legs — do not use the matchup-keyed accessors on a same-match group

`SelectionOn(matchupIndex)` and `SideOn(matchupIndex)` answer for the **first leg on that matchup in
slip order** and stop. The consequence is sharp and is pinned by test rather than left to be found:

> If that first leg is not a moneyline, **`SideOn` returns null even though a moneyline leg is on the
> slip.** The same two legs in the other order answer differently.

They are kept for compatibility and are correct for one-leg-per-matchup slips. For a same-match group
use the leg-addressed API: `LegCountOn`, `LegIndicesOn`, `Contains`, `RemoveLeg(legIndex)`,
`RemoveSelection(matchupIndex, selection)`.

## Building a same-match slip — and the decision that is yours

`AddLeg(matchupIndex, selection)` adds a leg to a matchup already on the slip. `MaxLegs` still binds.

**`Toggle` deliberately still *replaces*.** Whether clicking a second market on a match should replace
or add is an interaction-design decision belonging to the Design Director and this seat — not to the
lane that owns the pricing. The capability is exposed; the gesture is yours to choose. `Toggle`'s
current behaviour is pinned by test, so changing it is a decision someone makes on purpose rather
than a regression.

## Known gap you will meet

`TicketState.Voided` has no render arm anywhere — Unity, `game-console`, or
`sim/RunPlayer.ScoreSwings`. All compile; none show "VOID — stake returned". That is P6 and a standing
follow-up.
