# sgp lane — step 1 recommendation

**Lane:** 2 · **Lead:** Claude (Opus 5) · **Plan number:** F_0.6.0 · **Date:** 2026-08-12
**Inputs:** `research-sgp-pricing.md` (D1), `model-candidates.md` (D2), `correlation-recon.md` (D3).
**Status:** step-1 exit gate met. Stops here for the orchestrator. This document recommends; it does
not amend `design/02-betting-math.md` — that is step 2.

---

## The one-paragraph version

Lifting the one-pick-per-matchup guard without a joint price is not a mispricing, it is a
**correctness defect**: 22 two-leg shapes and 57 further three-leg shapes have joint probability
exactly zero on every matchup, and naive product pricing sells them at finite odds up to a mean
decimal of 2070.70. They cannot win. A further 22 two-leg shapes are logical implications where the
second leg adds no risk and the naive product charges a full extra decimal for it, paying the player
+6.1% to +274.2% EV with a 100% hit rate. Recommendation: **price the exact joint** — the engine can
compute it exactly and cheaply, which real books cannot — apply the **existing proportional margin**
to it, **block the zero-probability combinations** at slip construction, and **re-price on the
surviving legs when a leg voids**. The live question for Allen is not the mechanism; it is whether
the book should be honest, because pricing correctly deletes the "correlated parlay as discoverable
exploit" idea currently recorded in `02-betting-math.md:24`.

---

## R1 — Pricing rule: exact joint enumeration

**Recommend:** `p_joint` computed by exact enumeration over the engine's own finite distributions;
price `o_sgp = 1/(p_joint × (1 + Ω_sgp))`.

**Why this and not the alternatives:**

| Alternative | Verdict |
|---|---|
| **Naive product of leg odds** (what lifting the guard gives for free) | **Disqualified on correctness.** Sells 3.49% of pairs / 13.26% of triples / 27.48% of four-leg tickets that cannot win, and pays +EV on 16.1% / 27.1% / 28.4% of the rest. Not a safe placeholder, not a tunable starting point. |
| **Correlation-factor table on marginals** (what real books actually do — Sportradar's `factor × P1 × P2 × …`) | **Rejected.** It is an approximation built around an information limit we do not have: books cannot compute the joint of a real football match, so they estimate a scalar per combination offline. Ours is exact, O(1) in leg count over a cached grid. Adopting the approximation would also produce non-unit ratios on cross-family pairs that are *exactly* independent to 4.4e-14 — importing error the engine does not contain. |
| **Block all correlated combinations, offer only independent ones** | **Rejected as an alternative, adopted as a component.** It deletes the feature — the shapes players want to build are the correlated ones. But blocking is still required for the zero-probability set, since those have no finite price. |

**Consequences step 2 must carry:**

- **Payout can no longer be derived from per-leg odds.** `Ticket.PotentialPayout` (`Domain.cs:475`)
  is `stake × Π(leg odds)`. Under joint pricing the ticket must carry a **locked SGP price** as its
  own field. This is a structural change to `Domain.cs`, not a formula swap.
- **A `p_joint = 0` guard is mandatory at slip construction.** This replaces the current
  one-pick-per-matchup guard rather than simply removing it (`Run.cs:192-193`).
- **The scorer inclusion–exclusion needs an exact `g < k → 0` guard.** D3 found it cancels to ~1e-17
  rather than 0 in IEEE double, which silently misclassified 12 impossible triple shapes until
  guarded. Any production correlation code inherits this trap.
- **Two of the impossible pairs are v1 model artefacts, not football** — `BTTS YES + Under 2.5` and
  `Under 2.5 ⊂ BTTS NO` exist only because draws are unrepresentable. If draws are ever added, these
  decisions need revisiting.

## R2 — Margin: keep proportional scaling, make `Ω_sgp` an explicit dial

**Recommend:** apply the engine's existing proportional margin to `p_joint`, with a **separate,
tunable `Ω_sgp`**, initialized so the SGP carries at least the vig of the equivalent independent
parlay, and tuned by the step-4 gate campaign.

**Why the mechanism is settled:** D2's exit-gate finding. Every more sophisticated method — Shin,
power, odds-ratio — solves for a parameter across a complete book whose implied probabilities sum to
`1+Ω`. An SGP has no such book. Forced onto a synthetic two-outcome book, Shin collapses to the
additive method, which requires `p ≥ Ω/2` for the miss side to be a valid probability — below 2.5%
at our configured margin it produces **no valid price at all**, and realistic 3–4 leg parlays live
below 2.5% routinely. Proportional scaling wins by elimination, and it is already what
`MatchModel.cs:145` does. Extending it to `p_joint` is a consistency move, not a new mechanism.

**Why `Ω_sgp` must be its own dial, and why it should not simply equal `Ω`:** D3's most
counter-intuitive result is that **correct pricing lengthens 78.07% of two-leg tickets**, with a
median ratio of exactly 1.050 — because the modal two-leg ticket is genuinely independent and the
naive product charges vig twice for a bet that only carries it once. If `Ω_sgp = Ω`, same-game
parlays become strictly better value than the existing cross-game parlay product. That inverts the
economy: `02-betting-math.md:23` has parlays as "mathematically terrible, which is the joke and the
tension." It also runs against the real world — D1 found books charge *more* on SGPs, not less.

**What research could not settle:** D1 found **no operator or regulator publishes an SGP-specific
hold figure anywhere**; every number in circulation is inferred from a blended parlay bucket mixing
same-game with cross-game. So there is no real-world value to calibrate to, only a direction. D2's
Cortis reference argues directionally that margin should rise with leg count. `Ω_sgp` is therefore a
free design dial with a floor, and step 4 is where it gets its value. **This is the lane's largest
tuning risk** — ticket pricing drives run economy, and this dial moves every EV number in D3.

## R3 — Void: re-price on the surviving legs

**Recommend:** when a leg voids, re-derive the ticket price from the **conditional joint of the
surviving legs**. Never drop a leg to odds 1.0.

**Why:** D1 found that "drop the leg, odds go to 1.0" — which is what `PotentialPayout`'s
`ActiveLegs` filter does today — is what **no** real book does. Books split two ways:
recalculate-on-remaining (BetMGM, theScore Bet, FanDuel by structural absence of a carve-out) or
full-ticket void (DraftKings, Caesars). Sportradar's documented mechanism precomputes a
correlation-adjusted price for every single-void scenario at placement time — a genuine re-price,
not a strip-and-multiply.

Re-pricing over full-void because the price was a statement about a joint event: remove a leg and
the event has changed, so the price must. Full-void is simpler but hostile in a roguelite, where a
void the player did not choose can end a run — D1 surfaced the notorious real case of a bettor
losing 8 winning legs of 9 to a full void.

**The hard part, flagged now:** cash-out. `02-betting-math.md:45-48` prices remaining legs as
`Π p_j × o_j`, which assumes independence. On a same-game ticket that breaks twice — surviving legs
are correlated with each other, and a settled leg is *information* that shifts the survivors'
conditional probabilities. Live cash-out therefore needs the joint **conditioned on revealed
in-match state**, which is materially harder than pre-match pricing. D1's cheapest real-world
precedent is theScore Bet, which simply excludes SGPs from cash-out. Recommend conditioning rather
than excluding — cash-out is a core mechanic — but this is the most expensive item in the lane and
step 2 should cost it explicitly.

---

## What is Allen's call, not mine

1. **Should the book be honest?** Pricing the joint correctly means correlation is no longer an
   exploit, which deletes the "real-world exploit that should be a discoverable mechanic" framing at
   `02-betting-math.md:24`. **My recommendation: yes, price honestly, and move the exploit from base
   pricing into relics.** The book being correct is what makes a relic that breaks it feel earned,
   and it preserves the Band-3 "rigged it back" arc without shipping a pricing bug. D1 supplies a
   better story than anything invented: DraftKings had a correlation safeguard fail on a
   configuration bug, a bettor stacked 27 correlated parlays, and the Massachusetts Gaming Commission
   voted 5–0 to force payment of $934,137.
2. **`Ω_sgp` policy** — charge margin once (SGPs become the best value on the board) or compound
   with leg count (recommended).
3. **Void policy** — re-price (recommended) or full-void.
4. **The no-draws coupling** — two impossible shapes and two implication shapes are artefacts of
   draws being unrepresentable. Accept, or let this lane's findings count as evidence toward
   revisiting that v1 constraint.

## For the Design Director — the queued question, reframed

The question queued at the step-2 boundary was framed backwards and should be re-put. It is **not**
"correct pricing shows shorter odds and players will read it as cheating." Correct pricing **lengthens**
most tickets — 78.07% at two legs, 57.50% at three.

The real problem is **inconsistency**: the shapes players most want to build are exactly the ones
that shorten. The classic favourite-moneyline + Over 2.5 + that team's forward-to-score shape prices
at roughly **32% shorter** than the legs multiplied off the board (ρ median 1.63 at three legs),
while a mixed corners-and-cards ticket prices *longer*. So the player learns no stable rule, and the
one intuition they can form — "related legs pay less" — is wrong for the majority of tickets they
could build. That is the design problem, and it is harder than uniform shortening would have been.

---

## Reconciliation note (the concurrency gamble, settled)

D2 quarantined its real-book-dependent conclusions in a delimited section rather than scattering
them. With D1 landed, all four resolve without touching D2's mathematics: `Ω_sgp` has no published
value to calibrate to (stays a dial, per R2); books do layer margin on top of a correlation-adjusted
probability, consistent with D2's mechanism finding; restricted-combination policy is undocumented
and enforced at the bet slip, so blocking complements pricing rather than substituting for it; and
Cortis stays directional. Cost of running the three concurrently: this paragraph. No redo.

## Step-1 exit gate — met

Real-book claims carry citations, with a 10-item gaps list where they could not. Every candidate
model carries a written EV expression or an explicit disqualification. Recon numbers are computed
against our own configured latents and passed a verification gate at 2.554e-15 max deviation over
437,832 checks. One recommendation, with alternatives and the reason each was rejected. **Step 2
does not begin until this is accepted.**
