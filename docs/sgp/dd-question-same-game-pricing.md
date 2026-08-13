# Design Director — decision needed: how a same-game ticket's price is presented

**From:** sgp lane lead (Lane 2, same-game parlays, F_0.6.0) · **Via:** orchestrator · **Date:** 2026-08-12
**Blocking:** nothing yet. Step 2's math is settled and does not wait on this. It binds step 5
(presentation), and one sub-question below could change a step-3 engine behaviour, so it is asked now
rather than at the presentation boundary.

Self-contained — no need to read the lane docs. Numbers are measured, not estimated: an exact
enumeration over the sim's own match model, verified to 2.554e-15 across 437,832 checks.

---

## What is already decided (not your call, context only)

Players will be able to put two or more legs on the same match. Those legs are correlated, so the
ticket cannot be priced by multiplying the legs' odds. The book now computes the true joint
probability exactly and prices that. Allen approved this on 2026-08-12.

The margin rule adopted has a consequence that shapes your question: **the price departs from the
board's product only where correlation actually exists.**

```
shown price ÷ (leg odds multiplied together) = 1 / ρ        ρ = the correlation ratio
```

- **51.4% of two-leg tickets are exactly independent** — corners with cards, cards with goals. These
  price *identically* to what a player gets multiplying the board themselves.
- **Positively correlated tickets pay less.** The classic shape — favourite to win + Over 2.5 + that
  team's forward to score — pays about **61% of the multiplied number**, i.e. roughly 39% less.
- **Negatively correlated tickets pay more.**

So there is a true, learnable rule available to the player: *legs that help each other pay less;
unrelated legs pay exactly what you'd expect.* That is a real property of the design, not a story
we'd be inventing.

**Correction to what was queued earlier:** this question was first escalated as "correct pricing
shows shorter odds and players will read it as cheating." That framing was wrong twice over — first
because most tickets don't shorten at all, and then because the adopted margin rule makes independent
tickets match the multiplied number exactly. Please disregard the earlier framing; this document
replaces it.

---

## Q1 — Does the player ever see the correlation, and how?

The price is honest but its derivation is invisible. A player who multiplies three legs off the board
and gets a bigger number than the slip shows has found a real discrepancy and deserves an answer.

Options, no recommendation attached — this is squarely yours:

- Show nothing. The slip price is the price.
- Show both numbers, with the correction named.
- Show a qualitative marker on correlated combinations, without arithmetic.

Worth knowing: a same-game ticket carries **the same house edge as an equivalent cross-game parlay**.
Correlation moves the odds shown, never the edge. If the surface makes any promise about fairness,
that one is true and defensible.

## Q2 — Redundant combinations: warn, block, or stay silent?

22 two-leg combinations are **logical implications** — one leg cannot happen without the other
already having happened. Over 1.5 goals with Over 2.5 goals. Under 8.5 corners with Under 9.5.

They are priced correctly, so nothing is broken. But the player is paying a second leg's margin for a
leg that adds no risk. It is a bad bet, correctly priced. Real books block these as "logically
redundant" at the bet slip.

Block them (paternalistic, and removes a way for the player to be clever elsewhere), warn (honest,
adds clutter to every slip), or stay silent (the player can be quietly milked by a trap they cannot
see)?

## Q3 — Impossible combinations: what does refusal look like?

Some same-game combinations **cannot both happen** and are rejected when the player builds them —
both teams to score together with Under 2.5 goals, for example. The slip has to say something.

This is a teaching moment or a dead end depending entirely on how it reads, and it is the one place
where the refusal text is the whole experience.

**One trap you need to know about, because it constrains the copy.** Our matches never end in draws —
a v1 simulation constraint. Two of the impossible combinations are impossible *only because of that*.
"Both teams scoring means at least three goals" is true in our game and false in real football, where
1–1 exists. So any explanatory copy on those two either exposes that draws don't exist, or says
something a football-literate player will know is wrong. Silence, a non-specific refusal, or accepting
the tell — your call, and it is the sub-question that could reach back into step 3.

---

## What I need back

Rulings on Q1–Q3. Q3 is the one with a build consequence; Q1 and Q2 can land any time before step 5.
Evidence behind every number here: `docs/sgp/correlation-recon.md` (sgp branch).
