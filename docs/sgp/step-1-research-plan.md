# sgp lane — step 1 research plan

**Lane:** 2 (correlated parlays / same-game parlay) · **Lead:** Claude (Opus 5) · **Branch:** `sgp`
**Contract:** `docs/handoffs/sgp.md` · **Charter:** `docs/5-orchestration/next-slices-2026-08-12.md` Lane 2
**Status:** ACCEPTED (orchestrator relaying Allen, 2026-08-12). All three dispatches authorized and
running. D3's execute-against-the-engine ruling was granted explicitly — see §5.

Step 1 is docs-only. Nothing below edits `design/02-betting-math.md` (that is step 2) or touches
`engine/**` (step 3).

---

## 0. Grounding scan — what is already settled, so research does not re-derive it

Read before planning: `engine/MatchModel.cs`, `engine/Run.cs`, `engine/Domain.cs`,
`engine/OddsMath.cs`, `sim/Analysis.cs`. Four facts reframe the step:

**F1 — the sim already owns an exact joint match model.** `MatchDistributions`
(`MatchModel.cs:15-41`) caches, per matchup, an enumerated scoreline distribution
(`HomeWinScores` / `AwayWinScores`) plus truncated-Poisson count arrays for corners and cards.
`SampleStatLine` (`:155-166`) draws one winner, one conditional score, and four counts — the
whole match outcome comes from one joint object.

**F2 — the goal-family markets are deterministic functions of one shared scoreline.** Moneyline,
Total Goals, BTTS and Anytime Scorer all evaluate against the same enumerated score grid
(`TrueProbability`, `:180-232`). Their true *joint* probability is therefore **exactly computable
by enumeration** — a finite sum over the same grid. No copula, no simulation, no shared "momentum"
latent is required to know the truth.

**F3 — corners and cards are, in the current model, exactly independent of goals.** Their rates
derive from the matchup's fixed latents (`LatentsFor`, `:95-108`), but the draws themselves are
independent of the score draw. Within one matchup, corners ⟂ goals ⟂ cards. Their same-game joint
*is* the product today. That is correct for the model as written and false to real football — a
modelling gap, not a pricing bug, and the one place a copula-style construct may still earn a seat.

**F4 — pricing is single-selection and the parlay is a bare product.** Each offer prices at
`odds = 1/(p × (1 + Overround))` (`MatchModel.cs:145`); the ticket pays
`stake × Π(offered odds)` (`Domain.cs:475` → `OddsMath.ParlayDecimal`, `OddsMath.cs:59`). So a
naively-priced same-game parlay would pay `1/(Π pᵢ × (1+Ω)ⁿ)` — marginal probabilities multiplied,
vig compounded per leg.

**Consequence for canon.** `design/02-betting-math.md:24` says the correlated-parlay problem
"needs a correlation model in the sim (copula-lite: shared latent 'momentum' factor per match/team
is probably enough)". For the goal family that line is **obsolete** — the sim has something
strictly stronger than a copula, an exact joint. Step 2 amends that sentence. Research should
establish whether the copula idea survives at all, and my prior is that its only surviving job is
F3: *introducing* goal↔corner/card correlation the model currently lacks.

**Stale anchors — corrected, and these are now the accurate versions** (orchestrator, noted on the
board 2026-08-12). The one-pick-per-matchup guard is at **`Run.cs:192-193`**, not `Run.cs:181-182`
as recorded in TV PRD §8.2A and the F_0.4.0 plan. `Domain.cs` payout is at **:475**, not `:465`.
`OddsMath.cs:59` is still correct. Step 3 keys off the text, not any of these numbers — they drift.

**Reframing.** Step 1 is therefore not "invent a correlation model." The correlation is already in
the world. Step 1 is: *how do real books turn a joint model into a price, a margin, and a
settlement rule — and which of those choices do we want?*

---

## 1. Questions the research must answer

**Q1 — Pricing rule (the economy-defining choice).** Does the in-game book price an SGP from the
true joint (`1/(p_joint × (1+Ω_sgp))`), from the naive product of marginal odds, or from something
between? This decides whether same-game parlays are *just another market* or *a printing press*.
The design doc frames correlated parlays as "a real-world exploit that should be a discoverable
mechanic" (`02-betting-math.md:24`) — that framing only holds if the book prices naively. Real books
do not. Research must supply the real-world answer; the choice for our book is Allen's, through the
orchestrator, and I will present it as a ruling with a recommendation, not decide it here.

**Q2 — Margin policy.** How do books set the *extra* hold on an SGP relative to n singles, and by
how much? Target: a number we can calibrate `Ω_sgp` against, not an adjective.

**Q3 — Settlement, void, and restriction rules.** Three concrete sub-questions, each with a live
trap in our code:
- **Void.** `PotentialPayout` drops a voided leg by deleting its factor (`Domain.cs:455,475`). Under
  a joint price, deleting a leg is *not* deleting a factor — the residual must be **re-priced** off
  the conditional joint. Books have published rules for this. We need ours.
- **Restricted combinations.** Books block certain same-game pairs outright (mutually exclusive or
  too-correlated legs). We need to know the practice, because it is the cheap alternative to
  modelling the hard cases.
- **Cash-out on an SGP.** `02-betting-math.md`'s cash-out formula prices remaining legs as
  `Π pⱼ × oⱼ` — an independence assumption that breaks the moment two remaining legs share a match,
  and breaks *again* because a settled leg is information that shifts the surviving legs' `p`.

**Q4 — Which model do we adopt, and can its EV be written down?** Design pillar 3 / the standing
law: a mechanic whose EV cannot be written for the Monte Carlo audit is not designed yet. Every
candidate gets a written EV expression or it is disqualified.

---

## 2. Sources

**External** (read-only web research; the charter — "research on how it's done" — cannot be
satisfied offline). Fetched and indexed rather than pasted, so raw pages stay out of the lane's
context.

1. **Operator rules pages** — the primary source for Q3. Published same-game-parlay rules from the
   major books (settlement, void handling, correlated-leg restrictions, cash-out eligibility).
   These are the citable ground truth for settlement behaviour.
2. **Pricing-vendor and trading literature** — how the SGP products built by the odds vendors
   (BetBuilder-class products) construct a correlated price: joint simulation vs. analytic joint vs.
   correlation-matrix adjustment on top of marginals.
3. **Quantitative literature on correlated football markets** — Dixon–Coles (1997) and the
   bivariate-Poisson family (Karlis & Ntzoufras) for goal-model correlation structure; copula
   approaches to multi-leg pricing; any SGP-specific published work on effective hold.
4. **Empirical price comparisons** — practitioner analyses measuring SGP prices against the naive
   product. This is where Q2's calibration number comes from.
5. **Correlation-exploit write-ups** — which same-game combinations are historically mispriced, and
   why. This is the source material for the *discoverable mechanic* the design doc wants, and for
   what a relic in this space would actually do.

**Internal** — already read, no further reading needed: the four §1 docs, plus the five engine files
in §0.

---

## 3. Method

Per STUDIO.md, the bulk reading goes to bounded sub-agents (≤2 at once); I plan, dispatch, review,
and integrate. Three dispatches, each with named allowed files, an evidence requirement (every
factual claim carries a source and a quote), a forbidden list (`engine/**`, `sim/**`, `unity/**`,
`design/**` — read-only; writes confined to `docs/sgp/`), and an exit gate.

- **D1 — real-book practice.** Q1 (what books actually do), Q2, Q3. Output: a claims table, one row
  per claim, each with source. Exit gate: void, restricted-combination, and cash-out rules each
  answered with a citation, or explicitly recorded as not publicly documented.
- **D2 — quantitative literature.** Q4. Output: candidate models scored against our constraints —
  EV writable in closed form? deterministic and seed-stable? cheap enough to run inside a full gate
  campaign? degrades gracefully as legs grow? Exit gate: every candidate has a written EV expression
  or a recorded disqualification.
- **D3 — internal correlation reconnaissance (ruling requested, §5).** A throwaway computation over
  the existing `MatchModel` distributions at our configured latents and overround, answering: how
  far does the true joint diverge from the product, per market pair and per triple; and at what leg
  count does correlation beat the compounded vig `(1+Ω)ⁿ`. Numbers only — no product code, nothing
  shipped, nothing committed outside `docs/sgp/`.

Sub-agent side effects are checked (`git status` / `git log`) after every dispatch; a read-only
brief has not historically been self-enforcing.

---

## 4. Output shape

Lane folder `docs/sgp/`:

| File | Contents |
|---|---|
| `step-1-research-plan.md` | this document |
| `research-sgp-pricing.md` | findings for Q1–Q3, every claim cited; the claims table from D1 |
| `correlation-recon.md` | D3's numbers: divergence magnitudes per market pair, correlation-vs-vig break-even by leg count |
| `model-candidates.md` | Q4's scored candidate table, each with its written EV expression |

Ending with a **single recommendation** — one pricing rule, one margin policy, one void rule — plus
the rejected alternatives and the reason each was rejected, and an explicit list of what is Allen's
call rather than mine.

**Step-1 exit gate.** Every real-book claim cited; every candidate model carrying a written EV
expression; recon numbers computed against our own configured latents; one recommendation with
alternatives. Then stop at the orchestrator. No edit to `design/02-betting-math.md` — that is step 2.

---

## 5. Flagged now, not later

- **D3 — GRANTED (Allen, relayed 2026-08-12).** The docs-only reading yields: the numbers-only
  engine computation ships no code and is what makes step 2 a design rather than a guess. D3 runs.
  Its throwaway project lives in the session scratchpad, never in the repo, and compiles the engine
  *sources* rather than referencing the engine project — a project reference would trigger the
  post-build DLL copy into the Unity tree and dirty a tracked LFS asset.
- **UX consequence — QUEUED to the Design Director at the step-2 boundary** (orchestrator,
  2026-08-12), so the ruling exists before the pricing choice binds. The question: if our book
  prices SGPs correctly, the shown SGP price is *lower* than the product of leg odds a player can
  multiply off the board themselves. Real books have exactly this problem. Players read it as the
  game cheating. D3 measures the size of that gap (the `o_sgp / Π o_i` shortening ratio), so the DD
  gets a number to rule against rather than an adjective.
- **The gate campaign is G1–G7, not six — this is the accurate version** (orchestrator, noted on the
  board 2026-08-12). `sim/Analysis.cs` adds **G7 — market coverage** ("every shipped MarketKind is
  exercised by the skilled strategy"). Step 4's re-validation needs an SGP arm in the strategies or
  G7 cannot see the feature at all. The handoff's "six-gate" wording predates G7; read it as seven.
- **`DramaEvent.Step` is leg-scoped and becomes wrong here.** TV PRD §4.3.1a records that this is
  harmless only while one leg maps to one match, and that whoever builds step 1's dependency owns
  it. That is this lane. Noting it now so it is not discovered at step 5.
- **No-draws constraint interacts with this lane.** The stat-line sampler conditions the score on the
  already-drawn winner (`SampleStatLine`, `MatchModel.cs:157-158`), which is what makes draws
  unrepresentable. Any joint-probability enumeration must respect that same conditioning or it will
  compute a joint the sampler cannot produce.
