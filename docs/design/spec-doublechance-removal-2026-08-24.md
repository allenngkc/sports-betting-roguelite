# SPEC — DoubleChance leaves the offered set

**Allen ruled option (b)** (batch 170's fork, restated 2026-08-24): **DoubleChance leaves the offered
set.** It is the only market whose requirement this surface could not state — `T161-am` measured the
shortest authorable predicate missing by **70.9px** — and terse copy was exhausted rather than
insufficient.

**Written:** Design Director seat, 2026-08-24. **The lane assignment is Allen's once this is written.**

---

## 1. WHAT LEAVES THE OFFERED SET

**Three unconditional lines**, `engine/MatchModel.cs:160-162`:

```
offers.Add(Offer(matchup, MarketSelection.DoubleChance(MarketChoice.HomeOrDraw), config));   // 1X
offers.Add(Offer(matchup, MarketSelection.DoubleChance(MarketChoice.AwayOrDraw), config));   // X2
offers.Add(Offer(matchup, MarketSelection.DoubleChance(MarketChoice.HomeOrAway), config));   // 12
```

**They are not config-gated.** Unlike `HandicapLines` or `TeamGoalLines` there is no line list to
empty — the removal is these three calls.

### What does NOT leave, and each has a reason

- **`MarketKind.DoubleChance` — the ENUM MEMBER STAYS.**
- **The grading and pricing arms stay** — `MatchModel.cs:321`, `:460`, `:634`, and `JointModel.cs`.

**Three reasons, and the first is the one that breaks a save if ignored:**

1. **An in-flight run's DoubleChance legs must still GRADE.** Removing an offer is not removing a
   market's ability to settle, and conflating the two strands every ticket already placed.
2. **`EventText.BackedSide` is EXHAUSTIVE over fifteen kinds and THROWS on an unknown one** — that
   was `K17-cl`'s deliberate design, chosen over a one-liner precisely so a sixteenth kind could not
   be answered silently. **Deleting a member turns that safety into a crash.**
3. **The kind died this way before.** `Domain.cs`'s own docstring records it: *"Dead under the
   no-draws constraint (1X was the moneyline exactly and 12 priced at 1.000); alive since Allen
   lifted it 2026-08-12."* **Death-by-not-offering is the precedent already in the code.**

**RECORD THE REMOVAL IN THAT DOCSTRING**, beside the previous death. Cheaper than a config knob
nobody reads, and it puts the revival note where the last revival's history already lives.

---

## 2. WHAT STAYS AUTHORED — `C57`'s DISCRIMINATOR, APPLIED

`C57`'s test: **"the owed list carries THREE lines, not one — is it in the DECK, is it in the BUILD,
is it in the POOL."** Answered in order:

| | disposition | why |
|---|---|---|
| **BUILD** | **nothing to do** | `LegStatement` has **NO DoubleChance arm** — verified, zero hits. The kind already falls to `default: leg.DisplayLabel` |
| **DECK** | **the forms LEAVE** | `T152`'s `{CLUB} OR DRAW` / `{CLUB} TO WIN OR DRAW` and `G1-am11`'s `{CLUB} UNBEATEN` |
| **POOL** | **the forms LEAVE `TvExtentSweep`** | the line that actually bites |
| **REGISTER** | **everything STAYS** | the record is not the deck |

**Why the deck must drop them:** `C57-am` rules that the pool follows **what the deck authors**. A
deck entry for a market that cannot be offered therefore puts strings in the pool that the surface
can never print — and `C57` names exactly that as the defect: *"a pool holding a string the code
CANNOT emit is FABRICATED and its sweep is vacuous."* **Leave the forms in the deck and every future
sweep measures a market that does not exist.**

**Why the register keeps everything:** `T152`'s authoring, `T161-am`'s withdrawal, `G1-am11`'s
`UNBEATEN` re-authoring and TV's measurement at `ee16f06` all stand. **A revival re-authors from a
record rather than from nothing** — and that record already carries the finding that would govern
it: *the NEED band cannot hold a 12-character club plus any predicate.*

---

## 3. THE INTERIM — DO NOTHING, AND IT IS A RULING RATHER THAN A SHRUG

**Nothing ships today that needs reverting.** `{CLUB} UNBEATEN` was measured but **never built** —
`LegStatement` has no arm for the kind — so the interim is not *"a false statement ships until we fix
it"*. It is `T130`'s unauthored-kind fallback (`leg.DisplayLabel`), a defect **already ruled** at
`T130-vf`, covering nine kinds, of which DoubleChance is one and is about to be none.

**So: build no copy for a market that is leaving.** Authoring the club-alone NEED now is work
deleted on arrival.

> **THE CONDITION, and it is what makes this an interim rather than a deferral:** a ruled violation
> we are about to delete is acceptable; a ruled violation we have quietly postponed is not.
> **If (b) is not built within this phase, option (a) — the club-alone NEED — returns as the
> stopgap, and this seat should be told rather than left to assume.**

---

## 4. WHAT THE REMOVAL TOUCHES BEYOND THREE LINES

Named so the lane assignment is **sized rather than discovered**. Every consumer of `DoubleChance`:

- **`sim/SameMatchStrategy.cs`, `sim/SkilledStrategy.cs`, `sim/Analysis.cs` — THE SIM BETS IT.**
  Removing the offer changes what the sim can construct, which reaches the economy gates.
  **This is the largest unknown here and it is not this seat's to size** — it wants the sim lane's
  estimate before the work is scheduled.
- **`engine/JointModel.cs`** — the SGP pricing arms. Same disposition as §1: they stay.
- **`unity/…/MarketDestinations.cs`** — **`DOUBLE CHANCE` stays in `TableOrder` and in
  `KindsOf(Result)` regardless of offers**: `:140-143` filters by destination, never by offer count.
  **So the removal must decide whether the kind leaves the TAXONOMY too**, and the console spec's
  §4.1 answers this for empty **groups** and not for empty **kinds**. A gap the removal exposes.
- **`game-console/EventText.cs`** — `BackedSide`'s arm stays (§1, reason 2).

### One thing checked because it looked true and is not

**The `RESULT` group does NOT go empty.** DoubleChance shares `MarketDestination.Result` with
Moneyline, Handicap and WinningMargin, so **`K18`'s `no prices offered` state is NOT made reachable
by this removal** and its forcing hook is still owed. Recorded because the opposite would have been
a neat saving and it is not there.

---

## 5. WHAT IT COSTS, PLAINLY

- **Three offers per matchup leave the board** (1X, X2, 12).
- It is **the only market whose requirement this surface could not state**, which is why it is going
  rather than being shortened again.
- **It has died this way once before and come back** — so the removal is reversible by construction,
  and §1 puts the note where the last revival's history already is.
