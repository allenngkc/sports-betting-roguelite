# Concurrent legs on one match — investigation

**Date:** 2026-07-28 · **By:** TV sweat technical lead (performed directly; two dispatched agents
failed on infrastructure, one session limit and one watchdog stall)
**Gates:** PRD §4.3.1, and Phase 2B
**Method:** source reading only. No code changed. Every claim below carries a citation.

---

## Q1 — Can the engine produce two legs on one match today?

**No. It is explicitly forbidden and throws.**

`engine/Run.cs:181-182`, inside `PlaceTicket`:

```csharp
if (picks.Select(p => p.MatchupIndex).Distinct().Count() != picks.Count)
    throw new ArgumentException("A ticket cannot have two legs on the same matchup");
```

This is not an unexercised path or an accident of the current ticket policy. It is an **enforced
invariant with a purpose-written error message**, validated at placement before anything is
consumed.

`Pick` carries `MatchupIndex` (`engine/Domain.cs:316`), so the case is *representable* — and that is
precisely why the guard exists.

**This reframes §8.2A entirely.** It does not describe a bug in an existing feature. It describes a
feature that **does not exist and is actively prevented at the engine's front door.**

## Q2 — What happens at playback if it occurs?

**Moot.** It cannot occur. `PlaceTicket` is the only path to a ticket and it rejects the input, so no
playback path can ever receive one. There is nothing to trace and nothing broken downstream.

## Q3 — Why the guard exists, and what lifting it would actually cost

The guard is not defensive tidiness. **It protects the betting math.**

- `engine/Domain.cs:465` — `PotentialPayout` is `Stake × OddsMath.ParlayDecimal(legOdds) × PayoutMultiplier`
- `engine/OddsMath.cs:59` — `ParlayDecimal` is a straight **product** of the legs' decimal odds
- `engine/OddsMath.cs:70` — `ParlayProb` is a straight **product** of the legs' probabilities

Products are correct **only for independent events.** Two legs on the same match are not independent:
"Northgate to win" and "Over 2.5 goals" are correlated, and multiplying their standalone odds
misprices the combination. This is the same reason real sportsbooks either refuse same-game parlays
or price them through a dedicated correlation model.

*(The correlation argument is inference from the three citations above. No comment in the source
states the guard's motive; the pricing being a bare product is what makes it necessary.)*

So enabling §8.2A is **not a presentation feature.** It is, in order:

1. **A betting-math design problem.** A correlation model for same-match selections. This is
   `design/02-betting-math.md` territory, and design pillar 3 in `design/00-vision.md` is explicit:
   *"if we can't write down a mechanic's expected value for the Monte Carlo audit, it isn't designed
   yet."* Same-game parlays priced by independent product are undesigned by that standard.
2. **An engine change** — lifting the guard in `engine/Run.cs`, a PRD §11 forbidden file, plus
   whatever the correlation model requires.
3. **Re-validation** — the six gates against held-out seeds, since ticket pricing feeds run economy
   directly.
4. **Only then**, the presentation work: shared per-match event cursor, concurrent leg display,
   attribution across simultaneous count markets.

Steps 1–3 are entirely outside this worktree.

## Q4 — The fourth path

The three options in PRD §4.3.1 all assumed the feature was reachable and the problem was
sequencing. It is not reachable, so there is a fourth and better option:

**Reclassify §8.2A. It is a future game-design feature, not a TV sweat requirement.**

Concretely, for this slice:

- **Do not build concurrency plumbing** for a case the engine forbids. Merging event streams or
  synthesising a shared match cursor would be speculative machinery for an input that cannot arrive,
  and it would need rewriting anyway once a correlation model dictates how such legs actually behave.
- **Do keep the design tolerant.** Nothing in the ticket column, the planner, or the copy formatters
  should hard-code "exactly one live leg" in a way that requires a rewrite later. Tolerant is cheap;
  plumbing is not.
- **The `MatchIndex` key decision stands and stays.** It costs nothing today — every leg on a match
  supplies the same value whether or not concurrency is possible — and it is simply correct: a beat
  belongs to a match. It also means the key needs no revision if the feature ever lands.
- **`DramaEvent.Step` being leg-scoped stops being a defect.** With one leg per match, leg-scoped and
  match-scoped are the same thing. It only becomes a problem alongside a correlation model, and
  whoever builds that owns it.

**Phase 2B is unblocked by this reading.** The planner keys off event step, and event step is
unambiguous while one leg maps to one match.

## Recommendation — not a decision

Adopt the fourth path. Amend §8.2A from a requirement into a recorded future feature with its real
dependency chain (betting math → engine → re-validation → presentation), and carry "stay tolerant of
multiple live legs" as a design constraint on Phase 2 and Phase 3 rather than as work.

**Allen rules on this.** The alternative — treating §8.2A as in-scope — means opening a betting-math
workstream and an engine change before Phase 2B can start, which is a very different project from
the one this slice was chartered to do.

One thing worth his attention regardless: his original note said *"for later stages this will not be
the case. There could be 2 legs involved in a match."* That intent is real and worth keeping. This
investigation only establishes that delivering it starts in the betting math, not on the television.
