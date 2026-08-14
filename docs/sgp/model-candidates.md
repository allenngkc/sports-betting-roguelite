# SGP model candidates — quantitative literature review (D2 / Q4)

**Dispatch:** D2, lane 2 (`sgp`), step 1. **Scope:** Q4 only — candidate models/methods for pricing
correlated multi-leg soccer bets, scored against our project's real constraints. Docs-only,
read-only against the repo except this file. Companion docs: `step-1-research-plan.md` (the plan
this dispatch executes), `research-sgp-pricing.md` (D1, real-book practice), `correlation-recon.md`
(D3, numbers computed against our own engine). This document does not repeat D1's or D3's work and
defers to them where explicitly noted below.

---

## 0. Engine baseline, verified against source (not literature)

Read directly, this dispatch, read-only: `engine/MatchModel.cs`, `engine/OddsMath.cs`,
`engine/RunConfig.cs`. Four facts, all confirmed exactly as briefed — no drift found:

- **The goal family has an exact joint already.** `EnumerateScores` (`MatchModel.cs:375`) enumerates
  a winner-conditioned scoreline grid over `0..MaxGoalsGrid` per side (`RunConfig.cs:55`,
  `MaxGoalsGrid = 8`, so a 9×9 grid before the win/loss split). `TrueProbability` (`:180`) and
  `SampleStatLine` (`:155`) both key off this same cached grid. Moneyline, Total Goals, BTTS and
  Anytime Scorer are all sums or expectations over it (`ScoreProbability`/`ScoreExpectation`,
  `:428-446`).
- **Corners and cards are independent truncated-Poisson arrays, independent of goals and of each
  other.** `RawPoisson` (`:417`) builds each array out to `MaxCornerGrid = 20` / `MaxCardGrid = 12`
  (`RunConfig.cs:56-57`) from the matchup's latents; `SampleFromRaw` draws each axis independently
  in `SampleStatLine` (`:160-163`). Confirmed: in the model as written, corners ⟂ goals ⟂ cards
  within one matchup.
- **Single-leg pricing is the proportional/multiplicative margin method.** `Offer` (`:142-153`) sets
  `odds = 1/(p × (1 + Overround))`. Algebraically this is the exact inverse of what the
  margin-removal literature (§2 below) calls the **basic method**: if `r = 1/odds`, then
  `r = p×(1+Ω)`, and normalizing `r` back down recovers `p` exactly. **Our engine already implements
  the basic/proportional method for every single-leg market.** This matters for J2 — it is the
  starting point every other method is a departure from, not a blank slate.
- **A parlay is the bare product.** `OddsMath.ParlayDecimal` (`:59-67`) multiplies leg decimal odds
  with no adjustment. `OddsMath.Ev(p, stake, decimalOdds) = stake × (p × decimalOdds − 1)`
  (`:52-57`) is the existing EV primitive — it already has the right shape for a joint-priced bet;
  what's missing is a correct joint `p` and a correct SGP odds `O`, which is exactly what J1/J2/J3
  supply.

**A cost fact worth carrying into J1 and J3 both, derived from the code, not the literature.**
`ScoreProbability` takes an arbitrary `Func<int,int,bool> predicate` and sums once over the cached
grid. Combining *N* goal-family leg conditions into one predicate (`(h,a) => cond1(h,a) &&
cond2(h,a) && ...`) and summing once is **exact and O(1) in leg count** — the enumeration a 2-leg
goal-family SGP requires is the same single pass over the same ≤81-cell cached grid as an 8-leg one.
Handicap, team totals and correct score are all pure functions of the same `(h,a)` grid too, so they
are free in the same sense. **Half-time/full-time is the one named second-wave market that is not**
— it needs a half-time score, a dimension the model doesn't have today. The real cost driver is not
"more legs" in the abstract; it's (a) crossing families (goal↔corner, goal↔card — see J1) or (b)
adding a genuinely new time dimension (HT/FT). If corners/cards/goals were fully correlated with a
non-parametric joint table, the cell count is `81 × 441 × 169 ≈ 6.0M` (corner grid `21×21`, card
grid `13×13`) versus `81 + 21 + 21 + 13 + 13 = 149` numbers today — that six-million-cell table is
what J1 would cost if done the naive (non-parametric) way, which is exactly why the copula-style
candidates below (bolt a low-parameter dependence structure onto marginals you already have,
instead of building the full table) are the ones worth taking seriously.

---

## 1. How to read the table

Notation used throughout: `n` legs on one matchup; `p` = true joint probability all `n` legs hit
under the row's model; `Ω` = margin (fraction); `O` = offered decimal odds; `S` = stake.
`EV = S × (p × O − 1)` throughout (this is `OddsMath.Ev` evaluated at the joint).

Two rows can't both vary at once, so the table holds one side fixed when scoring the other:
- **J1 rows** (candidates for computing `p`) are scored assuming `O` comes from the row 2.1
  (proportional) method downstream — that isolates what each J1 candidate buys you.
- **J2 rows** (candidates for turning `p` into `O`) are scored assuming `p` is exact-enumeration
  today's-model `p` (independent corners/cards) — that isolates the margin-application question from
  the correlation question.
- **J3 rows** are scored on their own terms — cost and accuracy of *computing or approximating* `p`,
  independent of which generative model produced it.

Column definitions: **Cost** means marginal cost per SGP price, evaluated inside a gate campaign of
tens of thousands of runs — not one-time calibration cost, which is called out separately where it
applies. **Deterministic/seed-stable** means: given the same engine seed, does this candidate
reproduce bit-identical output, with no RNG draws of its own and no floating-point ordering
sensitivity.

---

## 2. The scored candidate table

| # | Candidate | Mechanism | EV expression | Deterministic / seed-stable | Cost | Degradation with leg count (2 / 4 / 8) | Applicability here |
|---|---|---|---|---|---|---|---|
| B0 | **Current engine** (independent draws + proportional margin) | Goal family exact via enumeration; corners/cards independent truncated-Poisson; parlay = bare odds product | `EV = S×(p_goal-exact × Π p_corner/card-indep × O − 1)`, `O = Π oᵢ` (bare product, no SGP-level margin at all today) | Yes — no RNG at pricing time | Trivial — already shipping | Flat for goal-only legs (O(1), see §0); flat for corner/card legs (independent factors, no cross term) | Baseline. The "printing press" risk in `02-betting-math.md` is really **two** separate gaps: (1) no correlation modeled for corner/card legs (J1), (2) no SGP-level margin applied at all (J2) — see §0 bullet 4, the product has **zero** extra vig today, not just naively-computed vig |
| 1.1 | Dixon–Coles τ adjustment (1997) | Multiplies the four low-score cells (0-0,1-0,0-1,1-1) of an independent-Poisson grid by a correction `τ(h,a;ρ)` fitted to historical draw-inflation, then renormalizes | Same shape as B0 with τ-adjusted cell probabilities feeding the same enumeration | Yes, once `ρ` is calibrated offline | Calibration: offline, historical MLE fit, one-time. Runtime: identical to B0 (still one grid pass) | Flat, same as B0 | **Redundant.** Built to fix independent-Poisson's under-prediction of low-scoring draws — we don't use independent Poisson for the joint (we enumerate exactly) and, per the research plan, our winner-conditioned sampler makes draws unrepresentable anyway. Nothing here to adopt for the goal family |
| 1.2 | Karlis–Ntzoufras bivariate Poisson (2003 / software 2005) | `BP(λ1,λ2,λ3)`: home/away goals share a third Poisson term `λ3` (the covariance); standard pmf `P(h,a) = e^{-(λ1+λ2+λ3)}(λ1^h/h!)(λ2^a/a!) Σ_{k=0}^{min(h,a)} C(h,k)C(a,k)k!(λ3/λ1λ2)^k` | Same shape as B0, cell probabilities from the BP pmf instead of the independent product | Yes, once `λ1,λ2,λ3` calibrated offline (EM algorithm, Karlis & Ntzoufras 2005) | Calibration: offline EM fit. Runtime: pmf sum over `k=0..min(h,a)` per cell — cheap, still one grid pass | Flat for goal-only legs | **Redundant for goals** — same reason as 1.1, we already have the exact joint. **The construction (shared-covariance term) is the transferable idea**, not the goal-goal application: it only works pairwise and only for two Poisson-family counts, so it's a candidate mechanism for corner-corner or card-card cross-team correlation, not for goal↔corner |
| 1.3 | McHale–Scarf discrete copula (2007, refined 2011) | Glue two arbitrary count marginals with a copula `C(·,·;θ)`: `P(h,a) = C(F1(h),F2(a);θ) − C(F1(h-1),F2(a);θ) − C(F1(h),F2(a-1);θ) + C(F1(h-1),F2(a-1);θ)`, fit `θ` by MLE. General dependence structure, not a single correlation number — 2011 shows dependence strength itself varies with competitive balance | Same shape as B0, cell probabilities from the copula construction | Yes, once `θ` (and, per the 2011 refinement, its dependence-on-competitive-balance) calibrated offline | Calibration: offline MLE. Runtime: a handful of copula-CDF evaluations per cell, or per predicate if you skip building the full cell table (see J3 3.3) — cheap | Flat if evaluated leg-combined rather than as a full table | **This is the mechanism, not the paper's own application, that answers J1.** Their published use is home-goals↔away-goals (redundant with our exact joint, same as 1.2). But a copula only needs two marginals and one dependence parameter — we already have an exact goal marginal and independent corner/card marginals ready to glue. **Top candidate for J1**: reuse this construction to link goals↔corners and goals↔cards without building the ~6M-cell full table (§0) |
| 1.4 | Titman, Costain, Ridall & Gregory joint goals+bookings (2015) | Multivariate counting-process / proportional-hazards model: goal and booking (card) event *intensities* jump dynamically in response to score state and prior cards (e.g., a red card raises the non-penalised team's scoring intensity) | Not a static end-of-match pmf — EV would require integrating a hazard process over match time, not summing a table | **SIMULATION-ONLY as posed** — architecturally a dynamic in-play model, not a pre-match joint we can enumerate | Calibration: offline, needs in-play/event-time-stamped historical data we likely don't have. Runtime: would require simulating a within-match process, a different engine architecture than "roll the whole match once" | Cost grows with match-time resolution, not leg count — a different axis entirely | **This is the direct hit for "goals + cards, jointly, in the literature" — and it doesn't fit our architecture.** Our model draws one static stat line (`SampleStatLine`, one Pcg32 pass); this model wants time-resolved event sequences. Adopting it as-is means rebuilding the sampler, not bolting on a parameter. Confirms the gap is real; the mechanism is the wrong shape for us |
| 1.5 | Yip, Zou, Hung & Yiu compound-Poisson corners (2024) | Corner count as a compound (geometric-batch) Poisson process — corners arrive in clusters, not one at a time; Bayesian geometric-Poisson variant handles match-to-match serial correlation | Marginal-only pmf, closed form via the compound-Poisson pmf (a convolution — see J3 3.5, Panjer recursion is the fast exact evaluator for exactly this pmf family) | Yes, once fitted offline | Calibration: offline. Runtime: comparable to or cheaper than a raw truncated Poisson once the recursion is set up | Doesn't address joint-with-goals; improves the corner *marginal's* realism only | Doesn't do J1's cross-market job. Relevant if we ever decide our flat truncated-Poisson corner model itself (not just its independence from goals) is worth improving — a second, smaller, decision |
| 1.6 | Philipson COM-Poisson copula, cards (2026) | Bivariate mean-parameterized Conway–Maxwell–Poisson linked by a copula, applied to cards data across the "Big 5" leagues for referee-consistency analysis | Same general shape as 1.3, but COM-Poisson marginals instead of plain Poisson | Yes, once fitted offline | Calibration offline. **Runtime cost is real and specific to COM-Poisson**: its normalizing constant `Z(λ,ν) = Σ_x λ^x/(x!)^ν` has no closed form and needs a truncated numerical sum per evaluation — strictly more expensive per cell than plain Poisson | Same shape concern as 1.3 | Confirms copula-linked cards modeling is active literature, but I could not confirm from the abstract alone whether the bivariate is home-cards↔away-cards or a cross-referee/cross-season comparison — **do not assume this paper's bivariate axis is goals↔cards** without reading the full text. COM-Poisson's extra per-cell cost is a real mark against it versus 1.3's plain-Poisson-marginal version for our purposes |
| 1.7 | UNVERIFIED — compound-Poisson penalty cards (believed Gómez-Déniz, Spanish league 2013–14) | Compound Poisson model for yellow/red cards as a function of match-context covariates | Believed similar shape to 1.5 but for cards | Unconfirmed | Unconfirmed | Unconfirmed | Believed to exist (see bibliography note); author spelling, exact year and co-authors could not be confirmed independently — do not cite this one further without re-verifying |
| 2.1 | **Proportional / multiplicative ("basic") method** | `p → O = 1/(p×(1+Ω))`, i.e. margin scales every probability by the same factor. Confirmed as the literature's "basic method" — Lindström 2023 vignette; algebraic inverse of what our engine already does for singles (§0) | `EV = S×(p×O−1) = S×(1/(1+Ω) − 1) = −S·Ω/(1+Ω)` — a **constant**, independent of `p`. Closed form, exact | Yes — no fitting, no solve, pure arithmetic | Free — O(1), same cost as today's single-leg pricing | **Flat.** Composes per-outcome; needs nothing about the rest of a "book" | **Top candidate for J2.** The only method here that operates pointwise on a single probability with no notion of a "complete book" — see §4 for why that specifically matters for a one-sided SGP price |
| 2.2 | Shin's method / balanced book (Shin 1991, 1992, 1993) | Assumes a proportion `z` of bettors are insiders; bookmaker sets odds to maximize profit (Shin) or minimize worst-case loss (balanced book) against them; `z` and the fair probabilities are solved **simultaneously across a full quoted book** | Defined at the book level, not per-outcome — **no closed form for a single isolated `p`** without first defining what "the rest of the book" is | Deterministic given a converged solve, but the standard solve is iterative numerical root-finding (closed-form exists only in special cases, e.g. n=2 — see 2.5) | Iterative solve per book; cheap for small books, but there is no natural "book" around one SGP price (see below) | Undefined without a book; degrades to 2.5's analysis at n=2 | **Does not transfer cleanly.** Requires a full mutually-exclusive-and-exhaustive outcome set summing to `1+Ω`. An SGP has no independently quoted "miss" side. Two honest options: (a) construct an artificial 2-outcome hit/miss book, which is **provably equivalent to 2.5 (additive)** — see below; or (b) import a `z` fitted on the correlated single-leg book and apply Shin's functional form to the joint `p` — a coherent extension, but **not something I found stated in the literature for parlays specifically; flag as our own construction if adopted** |
| 2.3 | Power method | `p_book = r^(1/k)`, `k` chosen so probabilities sum to 1 across the book | Same book-dependency problem as Shin: closed form exists **given** `k`, but `k` is normally fit from a multi-outcome book, not derivable from one probability | Deterministic given `k` | Cheap given `k`; the fitting step needs a book | Same "no natural book" problem as 2.2 | Same transfer problem as Shin. Would need `k` imported from the leg-level book, same caveat as 2.2(b) — untested extension, not a literature-stated method for this use |
| 2.4 | Odds-ratio method (Cheung 2015, applied empirically by Cain/Law/Peel 2003) | Models `OR = p(1−r)/r(1−p)` book-wide, solves for `OR` so book sums to 1 | Same shape as 2.2/2.3 | Deterministic given `OR` | Same profile as 2.2/2.3 | Same "no natural book" problem | Same transfer caveat as 2.2/2.3. Cain/Law/Peel (2003) is evidence this family recovers probabilities with better predictive accuracy than naive normalization **across real multi-way books** — not evidence for the single-joint-probability case |
| 2.5 | Additive method (≡ Shin, n=2) | `p_book = r − (Σr−1)/n` — subtract the margin equally across outcomes. **Confirmed equivalent to Shin's method exactly when n=2** (Lindström 2023 vignette, consistent with the Jullien & Salanié 1994 comment on Shin) | Inverted for pricing (fair `p` → offered `r'`, 2-outcome hit/miss): `r'_hit = p + Ω/2`, `O = 1/(p+Ω/2)`. `EV = S×(p/(p+Ω/2) − 1) = −S·(Ω/2)/(p+Ω/2)` — closed form | Yes, pure arithmetic | Free | **Breaks exactly, not just approximately, at low `p`, which is exactly the SGP regime.** Verified: `p=0.01`, `Ω=0.05` → `O_hit=28.6` vs. fair `100` vs. proportional `95.2` — a flat `Ω/2=0.025` add is `2.5×` the size of `p` itself. Worse: the implicit miss side, `r'_miss=(1−p)+Ω/2`, exceeds 1 (an invalid probability) whenever `p < Ω/2` — at our 5% single-leg margin, that's any joint probability below **2.5%**, which a realistic 3–4 leg SGP will routinely be. Below that threshold the method doesn't just misprice, it has no valid price to offer (`O_miss<1.0`, exactly the guard our own `Offer()` throws on) | **This is the literature's own two-outcome answer to J2's exact question, and it is a bad fit for us.** Confirms that "just apply Shin's method to the SGP" is not a distinct, safe option — it is additive, and additive doesn't merely degrade but breaks outright in a large part of the SGP probability range |
| 2.6 | Cortis EV/variance framework for combination bets (2015) | Derives bookmaker expected profit and payout variance as a function of margin and wager structure; shows bookmaker profitability *increases* with more legs/multiples offered, and that implied probabilities must sum to ≥1 book-wide or arbitrage exists | Theoretical EV/variance identities for the bookmaker side, not a specific `p→O` transform — complements 2.1 rather than competing with it | N/A — analytical, no runtime component | N/A | Not leg-count-scored; it's a justification for scaling `Ω` up with leg count, not a pricing mechanism itself | Answers "why/whether `Ω_sgp` should exceed the compounded single-leg margin" from pure EV mathematics — the piece that lets us justify a chosen `Ω_sgp > Ω` without needing D1's commercial numbers first (though D1's numbers should still calibrate the actual value — see §5) |
| 3.1 | Exact conditional / chain-rule factorization | No new library: combine all goal-family leg predicates into one lambda and sum once over the cached grid (§0); for legs spanning today's independent corner/card axes, multiply independent per-axis sums | `EV = S×(p_exact × O − 1)`, `p_exact` computed exactly, no approximation error at all | Yes — this is what the engine already does, just generalized to N-predicate lambdas | Free — O(1) in leg count for goal-family legs (verified from code, §0); O(1) per independent axis otherwise | **Flat at 2/4/8** for goal-family-only legs. Only grows if legs cross into a genuinely higher-dimensional *correlated* joint (i.e., only if J1 is adopted non-parametrically) | **Top candidate for J3 — because in most cases it means J3 isn't needed yet.** The "combinatorial explosion" risk named in the brief is real only for (a) cross-family correlation adopted the expensive way, or (b) HT/FT's new time dimension — not for leg count or most second-wave markets |
| 3.2 | Monte Carlo simulation of the joint | Draw N samples from (an extended, correlated) `SampleStatLine`; `p̂ = (1/N)Σ 1[all legs hit]` | `EV = S×(p̂×O−1)` — **SIMULATION-ONLY for `p̂`** itself, per the brief's hard gate | Deterministic/reproducible **only if** seeded from a fixed, isolated sub-stream of the master seed, isolated so it doesn't perturb other RNG consumers | Standard error `≈ √(p(1−p)/N)`. Worked example: `p=0.02`, `N=10,000` → SE≈0.0014, ~7% relative error — too coarse to price with. Getting to ~1% relative error needs roughly 50× more samples (~500K) per price. Multiply by (SGPs priced per run) × (tens of thousands of gate-campaign runs) and this is not free | Gets *harder* with leg count, since `p` shrinks multiplicatively and relative sampling error at fixed `N` grows | **Disqualified as a default** by the project's own EV-must-be-written-down law, and it's the fallback of last resort even setting that aside — 3.1 is exact and cheaper wherever it applies |
| 3.3 | Copula-based semi-analytic approximation | Reuse 1.3/1.6's construction: fit a low-parameter copula linking existing exact/independent marginals; evaluate the specific joint predicate needed via numerical integration instead of building a full cell table | `EV = S×(p_copula×O−1)`, closed form given the fitted copula family and `θ` | Yes, deterministic numerical integration, no RNG, given `θ` fixed offline | A handful of copula-CDF evaluations per price — cheap, avoids the ~6M-cell full table (§0) | Flat-ish; cost driven by number of *distinct correlated pairs* touched by a leg combination, not raw leg count | **Directly downstream of J1's top candidate (1.3).** If corner/card correlation is ever adopted, this — not Monte Carlo, not the full table — is how you'd keep pricing it cheaply |
| 3.4 | Multivariate-normal / Gaussian-latent approximation (Genz 1992) | Approximate the discrete joint via a fitted multivariate normal on latent scores, evaluate the needed orthant probability `Φₙ(bounds; Σ)` via Genz's transformation-based quadrature | `EV = S×(p_Φ×O−1)`, closed form given `Σ` | Deterministic **if** implemented with a fixed quadrature rule; many practical implementations use randomized quasi-Monte-Carlo for variance reduction at higher dimensions, which needs explicit reseeding to stay reproducible — flag this if adopted | Sub-millisecond for small `n`, scales gently — much cheaper than a full grid cross-product | Scales gently with number of correlated dimensions, unlike a raw table | **Weak fit for us specifically**: cards (0–12) and corners (0–20) are small-count discrete distributions; a continuous Gaussian approximation is a poor match at these counts (unlike e.g. large-count markets where CLT-style approximations work well). Latent candidate only if dimensionality genuinely explodes and 3.3 isn't available |
| 3.5 | Panjer recursion (Panjer 1981) | Recursively builds the pmf of a compound sum `S=ΣXᵢ` where the count `N` follows Poisson/binomial/negative-binomial | N/A to the core J3 ask as posed — computes a **sum's** distribution; we need a **conjunction-of-predicates** probability, a different object | Yes, deterministic recursion | Cheap, `O(max count)` per evaluation | N/A | **Redundant for the stated J3 problem.** Becomes relevant only if a compound-count marginal model is adopted (e.g., 1.5's corner model is exactly a Panjer-recursible compound Poisson) — then it's the fast exact evaluator for *that* marginal, not for the cross-market joint |
| 3.6 | FFT convolution for aggregate/multivariate claims | Evaluate a sum's distribution via characteristic-function inversion instead of direct convolution or recursion; multivariate extensions exist for dependent claim types | Same category mismatch as 3.5 — built for sums, not conjunctions | Deterministic given a fixed truncation/tilting scheme | Built for large state spaces (thousands+ of cells); our current per-axis grids (≤21 values) don't need it | N/A | **Not needed at current scale** (§0's cell counts are small). Would only become relevant if the market catalog grows enough that a genuinely large joint state space must be evaluated repeatedly — i.e., only after 3.3/3.4 are already insufficient |
| 3.7 | Saddlepoint approximation (Daniels 1954) | Approximate a distribution's tail/density via the moment generating function's saddlepoint, avoiding full enumeration or simulation | Same category mismatch as 3.5/3.6 for our conjunction problem; would apply to a **sum** (e.g., total combined count across markets), not a joint AND | Deterministic, closed-form-ish (solves one equation numerically per evaluation point) | Fast once set up; setup cost is the saddlepoint equation, non-trivial for a genuinely multivariate case | N/A | Same verdict as 3.5/3.6: general, real, well-established technique; **latent, not currently applicable** given our grid sizes and the fact that our core question is a conjunction, not a sum |

---

## 3. J1 — Introducing correlation the model lacks

The brief's framing survives research: for the goal family, the literature (Dixon–Coles 1997;
Karlis–Ntzoufras 2003/2005; McHale–Scarf 2007/2011) exists to solve a problem we don't have — none of
it beats an exact enumeration, and our engine already enumerates exactly. Every one of these is
**redundant in its original application** (rows 1.1–1.3).

What the literature is actually for, given that, is narrower and more specific than "correlation
modelling" in general: it is the **copula-as-glue construction** (McHale & Scarf 2007's actual
technical contribution, independent of what they applied it to) as a way to link **marginals we
already have** — the exact goal marginal, the independent corner and card marginals — without
building the full non-parametric joint table (§0's ~6M-cell number). That's row 1.3, and it's the
top J1 candidate.

The literature search for a paper that does the specific thing asked — **jointly model goals,
corners and cards together** — came back empty. The closest hits are pairwise and structurally
different from each other:
- **Goals + cards**: Titman, Costain, Ridall & Gregory (2015) is the one direct hit, and it's a
  dynamic in-play hazards model (card events shift goal-scoring *intensity* mid-match), not a static
  end-of-match joint pmf. It confirms real football has this dependency and quantifies it (red cards
  measurably raise the non-carded team's scoring rate) but is the wrong computational shape for an
  engine that draws one stat line per match. Philipson (2026) links cards via a copula but — from the
  abstract alone — I could not confirm its bivariate axis is goals↔cards rather than a
  referee/season comparison; don't assume it transfers without reading the full paper.
- **Corners alone**: Yip, Zou, Hung & Yiu (2024) is a strong, recent, well-specified marginal model
  (compound Poisson, handles serial clustering) but doesn't touch goals at all.
- **No trivariate (goals × corners × cards) model was found.** This is an honest absence, not a
  search failure I'm papering over — I ran multiple targeted queries for it.

**Recommendation for J1, mathematics only:** if goal↔corner and goal↔card correlation is worth
adding, do it as a copula bolt-on (McHale–Scarf's construction) fit to a small number of dependence
parameters (one `θ` per pair is the minimum viable version), evaluated via 3.3, not as a full joint
table. This keeps the J1 win from creating the J3 problem.

---

## 4. J2 — Applying margin to a joint probability (exit gate)

**The answer.** Every margin-application method in the literature beyond simple proportional scaling
— Shin (1991/1992/1993), the power method, the odds-ratio method — is constructed to solve for a
parameter (`z`, `k`, `OR`) **across a complete, mutually-exclusive-and-exhaustive book** whose raw
implied probabilities sum to `1+Ω`. An SGP is not a member of such a book: the market quotes "SGP
hits" and implicitly nothing else — there is no independently-priced "SGP misses" side to solve
against. This is exactly the trap the brief warned about ("a two-way market's margin methods may not
transfer cleanly to a one-sided parlay price"), and it is real, not hypothetical.

**What does transfer cleanly.** The proportional/multiplicative method (row 2.1) is the one method
in this literature that is defined pointwise, per-outcome, with no reference to the rest of a book —
`p → p×(1+Ω)` needs nothing but `p` and a chosen `Ω`. It composes onto a single joint probability
exactly as it composes onto a single leg probability, because it's the same operation. And — this
is the useful coincidence — it is *already* what our engine implements for every single-leg market
(§0). Extending it to `p_joint` is a one-line consistency move, not a new mechanism: `O_sgp =
1/(p_joint × (1+Ω_sgp))`, with `Ω_sgp` a design choice (possibly larger than the single-leg 5%; see
row 2.6 and §5) rather than a derived quantity.

**What doesn't transfer, and why, precisely.** I tested the obvious workaround — treat "SGP hits vs.
SGP misses" as an artificial 2-outcome book and run Shin's method on it — algebraically. The
literature itself (confirmed via the `implied` R package documentation, consistent with the Jullien
& Salanié 1994 comment on Shin) states that **Shin's method is exactly equivalent to the additive
method when there are only two outcomes.** The additive method subtracts (equivalently, when
pricing rather than recovering probabilities, adds) a flat amount of probability — `Ω/2` for a
2-outcome book — rather than scaling proportionally. I verified the arithmetic for a plausible
4-leg SGP: `p_joint=0.01`, `Ω=0.05` → additive gives odds of `28.6` against a fair value of `100`
(proportional-margin fair-plus-vig would be `95.2`). Worse than the compression itself: the
implicit "miss" side of the synthetic book, `r'_miss=(1−p)+Ω/2`, is only a valid probability
(`≤1`) when `p ≥ Ω/2`. Below that threshold — **2.5% at our configured 5% single-leg margin** — the
additive method has no valid price to offer at all; it demands `O_miss<1.0`, exactly the condition
our own `Offer()` guard (`MatchModel.cs:148-151`) treats as a pricing-time failure. A realistic
3–4 leg SGP will routinely land under 2.5%. So the sophisticated insider-trading-family methods,
applied the only way they can be applied to a single joint probability, don't just produce worse
behavior than naive proportional scaling — over a large part of the SGP probability range they
produce no valid price at all. This is not a hedge or a guess; it follows from a formula the
literature states and I verified by direct substitution.

**So the exit-gate answer is:** proportional/multiplicative margin application is the mathematically
appropriate method for a single joint SGP probability, not because the alternatives weren't
considered, but because the alternatives are provably either undefined (need a book that doesn't
exist) or actively worse (collapse to a method with a documented failure mode in exactly our
regime) when forced onto a one-sided price. The open questions this does *not* answer —
specifically, **how much larger `Ω_sgp` should be than the compounded single-leg margin** — are
partly Cortis's territory (row 2.6, pure EV mathematics: profitability increases with more legs
offered, which argues for `Ω_sgp` scaling up, not staying flat) and partly D1's territory (§5).

---

## 5. J3 — Approximation, if exact enumeration stops being viable

The most useful J3 finding is that the premise likely doesn't trigger as easily as the brief
worried. Reading `MatchModel.cs` closely (§0) shows the existing enumeration is **exact and O(1) in
leg count** for any combination of goal-family markets (moneyline, totals, BTTS, scorer, and the
"free" second-wave markets — handicap, team totals, correct score, all pure functions of the same
cached `(h,a)` grid). Leg count and most of the named second-wave markets are not the cost driver.

Two things genuinely would be:
1. **Half-time/full-time markets** — the one second-wave market that needs a dimension (a half-time
   score) the model doesn't have. This is a modelling-scope decision, not a J3 approximation
   problem — you'd need to decide how to generate a HT score before any pricing-cost question
   arises.
2. **Adopting J1's correlation the expensive way** — building a full non-parametric joint table
   across goals/corners/cards is the ~6M-cell scenario (§0). This is avoidable: J1's own top
   candidate (the copula bolt-on, row 1.3) and J3's top candidate for the approximate regime (row
   3.3, the same construction evaluated per-predicate rather than as a full table) are the same
   mechanism looked at from two angles. Adopt correlation as a copula, not a table, and J3's worry
   mostly dissolves along with it.

The heavier actuarial machinery — Panjer recursion (3.5), FFT convolution (3.6), saddlepoint
approximation (3.7) — is real, well-established, and **currently the wrong tool**: all three are
built to evaluate the distribution of a **sum** of many small contributions over large state spaces;
our problem is a **conjunction of predicates** over grids with at most 21 values per axis. They stay
on the table honestly (with citations) rather than being dropped, because they become the right
tool the moment either (a) a compound-count marginal model is adopted for corners or cards (row 1.5
is exactly a Panjer-recursible distribution), or (b) the joint state space genuinely grows past what
3.3/3.4 can handle. Neither condition holds today.

Monte Carlo (3.2) is the fallback of last resort, not a default: it's simulation-only by the
project's own EV-must-be-written-down law, its sampling error is worst exactly where SGP
probabilities live (small `p`, shrinking multiplicatively with leg count), and 3.1 is both exact and
cheaper everywhere it applies.

---

## 6. Depends on real-book practice (D1)

Everything below has a conclusion that depends on real-book commercial data D1 is gathering, not on
mathematics. Flagged here, not scattered above, per the dispatch's structural requirement.

- **The magnitude of `Ω_sgp`.** §4 establishes proportional scaling as the right *mechanism* and
  Cortis (2015) establishes *directionally* that margin should increase with leg count from pure EV
  theory. Neither gives a number. D1's Q2 (real-book margin calibration) is what turns "`Ω_sgp >
  Ω`" into an actual value to configure.
- **Whether books use anything beyond proportional scaling in practice.** Search results surfaced
  practitioner claims (blog-level, not academic — OpticOdds, Wizard of Odds, and similar sources)
  that some SGP products use "Gaussian copulas" or "correlation matrices" commercially. I did not
  verify these against primary sources — that verification belongs to D1's Q1, with D1's citation
  standard, not mine. Do not treat my mention of this here as confirmation.
- **Whether restricted-combination policies make J1's hard cases moot.** If real books simply refuse
  to offer highly-correlated same-game combinations (D1's Q3) rather than pricing them, the
  copula-modelling investment in §3 may be lower-priority than settlement/void correctness. That
  prioritization call needs D1's findings.
- **Cortis's "more multiples increases profitability" finding**, cited in §4/row 2.6 as pure EV
  theory, was reached from a general bookmaker-payout model, not from soccer SGP data specifically.
  Whether it holds at the margins and leg counts our design is considering is an empirical question,
  not one this document answers.

---

## 7. What contradicts, and what confirms, the brief's essential context

**Nothing found contradicts the brief.** Every line-numbered claim (`EnumerateScores :375`,
`SampleStatLine :155`, `TrueProbability :180`, the independence of corners/cards, the proportional
single-leg pricing formula, the bare-product parlay) checked out exactly against source, with no
drift.

**One refinement worth flagging explicitly:** the brief frames the enumeration's cost risk as "the
leg cap rises or the second-wave markets land." §0 and §5's close reading of the code found this
isn't quite the right threat model — leg count and most second-wave markets are free (O(1), same
grid pass). The two things that actually cost money are crossing correlation into corners/cards the
expensive way, and half-time/full-time's new time dimension. This doesn't change any conclusion, but
it changes where step 2's caution should be aimed.

---

## 8. Bibliography

### Confirmed (title, authors, year, venue and URL/DOI independently verified this session)

1. Dixon, M.J. & Coles, S.G. (1997). "Modelling Association Football Scores and Inefficiencies in
   the Football Betting Market." *Journal of the Royal Statistical Society Series C (Applied
   Statistics)*, 46(2), 265–280. DOI: [10.1111/1467-9876.00065](https://doi.org/10.1111/1467-9876.00065).
2. Karlis, D. & Ntzoufras, I. (2003). "Analysis of Sports Data by Using Bivariate Poisson Models."
   *Journal of the Royal Statistical Society Series D (The Statistician)*, 52(3), 381–393. DOI:
   [10.1111/1467-9884.00366](https://doi.org/10.1111/1467-9884.00366).
3. Karlis, D. & Ntzoufras, I. (2005). "Bivariate Poisson and Diagonal Inflated Bivariate Poisson
   Regression Models in R." *Journal of Statistical Software*, 14(10), 1–36. DOI:
   [10.18637/jss.v014.i10](https://doi.org/10.18637/jss.v014.i10).
4. McHale, I. & Scarf, P. (2007). "Modelling Soccer Matches Using Bivariate Discrete Distributions
   with General Dependence Structure." *Statistica Neerlandica*, 61(4), 432–445. DOI:
   [10.1111/j.1467-9574.2007.00368.x](https://doi.org/10.1111/j.1467-9574.2007.00368.x).
5. McHale, I. & Scarf, P. (2011). "Modelling the Dependence of Goals Scored by Opposing Teams in
   International Soccer Matches." *Statistical Modelling*, 11(3). DOI:
   [10.1177/1471082X1001100303](https://doi.org/10.1177/1471082X1001100303).
6. Titman, A.C., Costain, D.A., Ridall, P.G. & Gregory, K. (2015). "Joint Modelling of Goals and
   Bookings in Association Football Matches." *Journal of the Royal Statistical Society Series A:
   Statistics in Society*, 178(3), 659–683. DOI:
   [10.1111/rssa.12075](https://doi.org/10.1111/rssa.12075).
7. Yip, S., Zou, Y., Hung, R.T.H. & Yiu, K.F.C. (2024). "Forecasting Number of Corner Kicks Taken in
   Association Football Using Compound Poisson Distribution." *Journal of the Operational Research
   Society*, 75(11). DOI: [10.1080/01605682.2024.2306170](https://doi.org/10.1080/01605682.2024.2306170).
   Preprint: [arXiv:2112.13001](https://arxiv.org/abs/2112.13001).
8. Philipson, P. (2026). "Yellow Fever: An Investigation into Referee Consistency in the 'Big 5'
   Leagues of European Football Using a Bivariate Mean-Parameterized Conway–Maxwell–Poisson Copula
   Model." *Journal of the Royal Statistical Society Series A: Statistics in Society*, advance
   article (published online 17 Feb 2026). DOI:
   [10.1093/jrsssa/qnag014](https://doi.org/10.1093/jrsssa/qnag014).
9. Shin, H.S. (1991). "Optimal Betting Odds Against Insider Traders." *The Economic Journal*,
   101(408), 1179–1185. [Oxford Academic](https://academic.oup.com/ej/article-abstract/101/408/1179/5190137).
10. Shin, H.S. (1992). "Prices of State Contingent Claims with Insider Traders, and the
    Favourite-Longshot Bias." *The Economic Journal*, 102(411), 426–435.
    [Oxford Academic](https://academic.oup.com/ej/article-abstract/102/411/426/5157042).
11. Shin, H.S. (1993). "Measuring the Incidence of Insider Trading in a Market for
    State-Contingent Claims." *The Economic Journal*, 103(420), 1141–1153. DOI:
    [10.2307/2234240](https://doi.org/10.2307/2234240).
12. Jullien, B. & Salanié, B. (1994). "Measuring the Incidence of Insider Trading: A Comment on
    Shin." *The Economic Journal*, 104(427), p. 1418ff.
    [Oxford Academic](https://academic.oup.com/ej/article-abstract/104/427/1418/5158774).
13. Cain, M., Law, D. & Peel, D. (2003). "The Favourite-Longshot Bias, Bookmaker Margins and
    Insider Trading in a Variety of Betting Markets." *Bulletin of Economic Research*, 55(3),
    263–273. DOI: [10.1111/1467-8586.00174](https://doi.org/10.1111/1467-8586.00174).
14. Fingleton, J. & Waldron, P. (1999). "Optimal Determination of Bookmakers' Betting Odds: Theory
    and Tests." Trinity College Dublin Economics working paper 96/9 (rev. Dec 1999); also CEPR
    Discussion Paper No. 1623. [PDF](https://www.maths.tcd.ie/~pwaldron/pdf/tp969.pdf) — **working
    paper, not peer-reviewed**, cited as such.
15. Štrumbelj, E. (2014). "On Determining Probability Forecasts from Betting Odds." *International
    Journal of Forecasting*, 30(4), 934–943.
    [ScienceDirect](https://www.sciencedirect.com/science/article/abs/pii/S0169207014000533).
16. Clarke, S.R., Kovalchik, S.A. & Ingram, M. (2017). "Adjusting Bookmaker's Odds to Allow for
    Overround." *American Journal of Sports Science*, 5(6). DOI:
    [10.11648/j.ajss.20170506.12](https://doi.org/10.11648/j.ajss.20170506.12).
17. Cortis, D. (2015). "Expected Values and Variances in Bookmaker Payouts: A Theoretical Approach
    Towards Setting Limits on Odds." *Journal of Prediction Markets*, 9(1), 1–14.
    [ubplj.org](https://www.ubplj.org/index.php/jpm/article/view/987).
18. Lindström, J.C. (2023). "Introduction to the `implied` package." CRAN vignette, R package
    `implied`. [cran.r-project.org](https://cran.r-project.org/web/packages/implied/vignettes/introduction.html)
    — **software vignette, not a peer-reviewed paper**; cited for its precisely-stated, directly
    tested formulas (basic/proportional, additive, power, odds-ratio, Shin/balanced-book methods)
    and its own literature list, which independently corroborates entries 10, 11, 13, 16 above.
19. Panjer, H.H. (1981). "Recursive Evaluation of a Family of Compound Distributions." *ASTIN
    Bulletin*, 12(1), 22–26.
    [Cambridge Core](https://www.cambridge.org/core/journals/astin-bulletin-journal-of-the-iaa/article/recursive-evaluation-of-a-family-of-compound-distributions/BD6628F3853600A8F79C6A45E92938CB).
20. Genz, A. (1992). "Numerical Computation of Multivariate Normal Probabilities." *Journal of
    Computational and Graphical Statistics*, 1(2), 141–149. DOI:
    [10.1080/10618600.1992.10477010](https://doi.org/10.1080/10618600.1992.10477010).
21. Daniels, H.E. (1954). "Saddlepoint Approximations in Statistics." *Annals of Mathematical
    Statistics*, 25(4), 631–650. DOI:
    [10.1214/aoms/1177728652](https://doi.org/10.1214/aoms/1177728652).
22. Davis, J., Dawson, J. & Krieger, K. (2018). "Correlated Parlay Betting: An Analysis of Betting
    Market Profitability Scenarios in College Football." *Journal of Prediction Markets*, 12(2).
    [ubplj.org](https://www.ubplj.org/index.php/jpm/article/view/1562). Mentioned in §3 context
    only — an empirical market-efficiency study, not a pricing-methodology paper; not a table row.
23. Buraimo, B., Forrest, D. & Simmons, R. (2010). "The Twelfth Man? Refereeing Bias in English and
    German Soccer." *Journal of the Royal Statistical Society Series A: Statistics in Society*,
    173(2), 431–449. [Oxford Academic](https://academic.oup.com/jrsssa/article-abstract/173/2/431/7077578).
    Mentioned in §3 context only (established precedent for bivariate card modelling); author
    initials not independently confirmed in this session, surnames and venue/vol/pages
    cross-confirmed across three independent-domain sources.

### Practitioner / non-academic sources (cited accurately as such, not disguised as papers)

- Buchdahl, J. "Using the Wisdom of the Crowd to Find Value in a Football Match Betting Market."
  [football-data.co.uk](https://www.football-data.co.uk/wisdom_of_crowd_bets). Origin of the
  "weights proportional to the odds" and "power method" naming used in row 2.3 and the `implied`
  package.
- Cheung, K. (2015). "Fixed-Odds Betting and Traditional Odds."
  [sportstradingnetwork.com](https://www.sportstradingnetwork.com/article/fixed-odds-betting-traditional-odds/).
  Origin of the odds-ratio method (row 2.4).

### UNVERIFIED — believed to exist, could not fully confirm (count: 2)

- **"Fast Fourier Transform for Multivariate Aggregate Claims."** *Computational and Applied
  Mathematics* (Springer), published online 20 April 2016. DOI:
  [10.1007/s40314-016-0336-6](https://doi.org/10.1007/s40314-016-0336-6). Title, venue, year and
  DOI confirmed via Springer and ResearchGate listings (independent domains); **author names could
  not be confirmed** — three fetch attempts (Springer direct, Springer via redirect, ResearchGate)
  were paywalled or blocked (HTTP 403 / auth redirect). Referenced only narratively in row 3.6, not
  relied on for any conclusion.
- **"Modelling Penalty Cards in Football with Applications."** *Electronic Journal of Applied
  Statistical Analysis*. Journal's own listing shows the author surname as "Gónez-Déniz"; this may
  be a transcription artifact for "Gómez-Déniz" (a real, published statistics author), but I could
  not independently confirm the correct spelling, co-authors, or exact publication year across two
  search attempts. Referenced only as row 1.7, explicitly marked unverified, and not used to support
  any conclusion in the prose sections.
