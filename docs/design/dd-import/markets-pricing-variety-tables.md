# Markets → Allen · Pricing variety (scorer flatness), before/after and the lead's read

**From:** markets/sim lead (`markets-2`) · **2026-08-06**
**This is not a merge request.** Committed on `markets-2` only; nothing lands until Allen rules.
Re-baselined against merged main with arm B in, as instructed.

---

## What was wrong

Scoring weight was **purely role-derived**: every forward on a team carried the same weight, so
every forward on that team priced identically. A 14-row scorer board therefore printed at most
**six distinct prices** — 2 teams × 3 roles. On that tab the player was choosing a *name*, not a
price, and the three forwards in front of him were one offer printed three times.

## The change

One dial: `RunConfig.ScoringWeightJitter` (0.35 = ±35%), a multiplicative spread around the role
weight, drawn per player. **Zero restores the previous behaviour exactly**, which is what makes
this a comparison rather than a rewrite.

Two deliberate properties:

- **Its own RNG stream.** The jitter draws from `DeriveMatch(round, i, "weights")`, never the
  roster stream. Sharing would have shifted every subsequent name draw, so before/after slates
  would differ in *names* as well as weights and no comparison could isolate the change. Player
  names are byte-identical across the two arms.
- **Role order survives by construction, not by luck.** The spread is symmetric and bounded, so at
  the shipped weights a jittered forward (3.0 × 0.65 = 1.95) still outranks a jittered defender
  (0.5 × 1.35 = 0.675) for every seed. A striker is still a striker. Weight is also clamped
  strictly positive — a zero weight would leave a player listed but unscoreable, which is worse
  than the flatness it replaces.

## Did it fix the thing it was for?

18,000 boards, counting prices **as printed** (rounded to the American odds the player reads —
two prices he cannot tell apart are one price to him):

| | distinct prices per 14-offer board | same-team same-role groups printing one identical price |
|---|---|---|
| before | min 3 · **median 6** · max 6 | **108,000 / 108,000 — 100%** |
| after | min 12 · **median 14** · max 14 | 124 / 108,000 — **0.1%** |

Six prices became fourteen. The residual 0.1% is two jittered players landing on the same rounded
odds by coincidence, which is a real board doing a normal thing, not the old structural identity.

## The gate tables

Identical parameters both sides: `--gates --runs 1000 --seed-prefix TUNE`, on merged main.

| Gate | Before | After |
|---|---|---|
| G1 honest gambling | PASS — median 4, won 0.0% | PASS — median 4, won 0.0% |
| G2 engine mandatory | PASS — median 5, won 0.0% | PASS — median 5, won 0.0% |
| G3 skilled + items wins | PASS — median 6, won 5.4% | PASS — median 6, won 5.4% |
| G4 the EV arc exists | PASS — crosses at R3 | PASS — crosses at R3 |
| G5 composition superadditive | PASS — synergy excess +0.2pp | PASS — synergy excess +0.2pp |
| G6 martyr guard | PASS — martyr-worst 6.9% vs skilled 5.4% | PASS — martyr-worst 6.9% vs skilled 5.4% |
| G7 market coverage | PASS — all shipped markets covered | PASS — all shipped markets covered |

**Every verdict and every figure identical.** That is less surprising than it looks: the bots are
policy-excluded from pricing anytime scorer, so redistributing weight *within* that market cannot
reach any gate. The gates are silent here because they are structurally blind to this market, not
because the change is provably harmless — which is exactly why the harness below is the instrument
that matters.

## The instrument that can see it

`--scorer-ev --runs 400 --seed-prefix SCORER` — calibration of priced probability against the
sampler's realised frequency, the one measurement no gate can be:

| band | before Δ / realised EV | after Δ / realised EV |
|---|---|---|
| 0–5% | +0.1pp / −3.6pp | 0.0pp / −3.7pp |
| 5–10% | 0.0pp / −4.6pp | 0.0pp / −5.0pp |
| 10–20% | 0.0pp / −4.8pp | 0.0pp / −4.6pp |
| 20–35% | +0.1pp / −4.5pp | +0.1pp / −4.5pp |
| 35%+ | +0.1pp / −4.5pp | +0.1pp / −4.5pp |

**Still calibrated** — every band within 2 SE, by role FW/MF/DF all within 0.1pp. The bands
repopulated (more offers now land in 0–5% and 35%+, which is the spread arriving) without pricing
drifting away from what the sampler realises.

### ⚠ RETRACTED — "longshot scorers are a point cheap" was my own instrument, not a finding

I reported the 0–5% band's realised EV (−3.6pp against the −4.76pp the overround intends) as a
real pricing defect, on the strength of a table that printed **±0.0pp** beside it. That SE belongs
to the **frequency** column. The EV column's own error is a different number, and at long odds it
is far larger: a hit pays `odds` and a miss pays 0, so one sample's EV variance is
`odds² · q(1−q)`, and at p≈4% the odds are ~24. Measured:

| band | realised EV | **EV's own SE** | gap to −4.76pp | verdict |
|---|---|---|---|---|
| 0–5% | −3.7pp | **±1.0pp** | 1.06pp | **1.06 SE — noise** |
| 5–10% | −5.0pp | ±0.6pp | 0.24pp | noise |
| 10–20% | −4.6pp | ±0.3pp | 0.16pp | noise |
| 20–35% | −4.5pp | ±0.2pp | 0.26pp | 1.2 SE — noise |
| 35%+ | −4.5pp | ±0.3pp | 0.26pp | noise |

**Every band is within 2 SE of −4.76pp. Scorer pricing returns the intended vig, and there is no
longshot discount.** The item is closed, not deferred.

Two things I chased before getting here, both wrong and both worth recording so nobody re-runs
them: the goal grid does **not** truncate the price (pricing and the sampler share the same
truncated distribution, so it cancels), and scorer attribution does **not** deviate from the
model's independence assumption (`SampleScorers` draws each goal independently with replacement,
exactly as `ScoreExpectation` assumes).

The harness now prints the EV column's own error next to it, and its takeaway judges EV against
that error rather than against the frequency's. It was reporting an uncertainty for one column and
inviting a conclusion about another — the precise shape of the vacuous-gate failure this studio has
spent a fortnight on, in an instrument I built to catch exactly that.

## The read

**Take it.** It does what it was for — six prices became fourteen — the gates do not move, and the
one instrument that can see this market says pricing is still honest.

**One caveat I want on the record rather than buried.** "The gates did not move" is weaker evidence
here than it sounds, for two independent reasons. First, the gates cannot see this market at all.
Second, **G6 still cannot reliably fail**: its tolerance (±2.15pp) is wider than its band (2pp), so
it would report PASS through a real breach. I raised that before this change and it remains open. I
do not think it endangers *this* change — the mechanism cannot reach the martyr path — but "all
gates green" should not be read as a clean bill of health from this seat until G6 is fixed.

**One test re-pinned, not silently.** `AnytimeScorerTests.Pricing_uses_the_weighted_goal_attribution_distribution`
pinned an exact scorer probability computed under flat weights; it moved 0.2146 → 0.2397. The pin is
a determinism guard and is expected to move when the model does, so it is re-pinned with the reason
written beside it. The assertion that carries the *meaning* — a forward outranks a defender — was
untouched and still passes.

## Scope of these measurements (C25)

Gates at `--runs 1000`; calibration at `--runs 400` (5.7M samples); the distinct-price count at
18,000 boards. One machine, one seed family per measurement. Nothing here exercises settlement, the
parlay path, or any presentation surface — the scorer board's *rendering* is unchanged by this and
was last photographed as `11-margin-max-legs-staged-receipt`'s sibling states. The distinct-price
count rounds to printed American odds deliberately; at full float precision every price differs and
the number would be meaninglessly flattering.

---

# AMENDMENT — 2026-08-07, found during the merge run

**From:** markets/sim lead (`markets-2`) · **Status of the change itself: unaffected.** Allen accepted
pricing variety on 08-07 and it is merge-ready. This amends one claim in the document above and one
sentence the harness was printing. Neither changes behaviour; the gates and the calibration re-ran
figure-for-figure identical afterwards.

## 1. "Role order survives by construction" was wider than its arithmetic

**What §"The change" claims above:** *"Role order survives by construction, not by luck. The spread is
symmetric and bounded, so at the shipped weights a jittered forward (3.0 × 0.65 = 1.95) still
outranks a jittered defender (0.5 × 1.35 = 0.675) for every seed. A striker is still a striker."*

**What is actually true.** That demonstration covers the forward-versus-defender pair and the
document generalised it to the whole role order without checking the pair next to it. **Forward and
midfielder overlap at the shipped dial:**

| pair | lower role's ceiling | upper role's floor | separated? |
|---|---|---|---|
| forward vs defender | 0.5 × 1.35 = **0.675** | 3.0 × 0.65 = **1.95** | yes, at every seed |
| midfielder vs defender | 0.5 × 1.35 = **0.675** | 1.5 × 0.65 = **0.975** | yes, at every seed |
| **forward vs midfielder** | 1.5 × 1.35 = **2.025** | 3.0 × 0.65 = **1.95** | **no — the bands cross** |

So a jittered midfielder can out-price a jittered forward. **Measured: 19 of 3,600 teams — 0.53%** —
across 300 seeds of generated rosters, counting a team as inverted if any midfielder on it carries a
higher scoring weight than any forward on it.

**The behaviour is not obviously wrong; the claim was.** An attacking midfielder pricing above a
fourth-choice forward is a normal thing for a board to say, and it is arguably the variety this
change exists to create. What was wrong was asserting "by construction, not by luck" over a range the
construction does not cover. **Whether the overlap is desirable is a design question and stays open —
it is not a defect this seat is fixing unasked.** If it should be closed, the lever is a narrower
jitter: the bands stop crossing below **j = 0.714**, and the shipped value is 0.35.

**Now guarded, so it cannot be re-asserted by accident.** Two tests in `engine.tests`:
`Role_weight_bands_are_disjoint_for_forward_over_defender_only` pins which pairs the construction
separates and which it does not; `Every_generated_roster_keeps_forwards_and_midfielders_above_defenders`
checks the two real invariants on generated rosters rather than inferring them from the arithmetic
that produced those rosters, and **reports the inversion rate while asserting nothing about it** — a
threshold there would turn red on a future narrower jitter, which would be an improvement, and an
unreported count is the vacuous-green shape this studio has spent a fortnight on.

## 2. The calibration harness described a model it no longer had

Its by-role section printed *"scoring weight (and so priced probability) is assigned purely by role"* —
the exact property this change deleted — and would have printed it under every future run, including
the runs quoted in the tables above. It now states what role sets, what the jitter spreads, and what
the split **cannot** see: a miss confined to one player inside a role pools away there and needs the
band table (C25).

## Why both are the same defect

They are the shape this branch already retracted once, in the EV column: **a description that outlived
the model it described.** The EV retraction, the "purely by role" sentence and the "by construction"
claim are three instances in one change. None was caught by a test — the first by arithmetic, the
second and third by reading the diff against the model it had just altered.

## Scope of the amendment's own numbers (C25)

The 0.53% is 300 seeds × 6 matchups × 2 teams = 3,600 teams, one seed family, counting per-team not
per-pair — a team with two inversions counts once, so this is a floor on pairs and an exact figure on
teams. It measures generated **rosters**, not boards a player sees: it says nothing about how often an
inversion is visible on a 14-row scorer tab, or whether it reads as wrong when it is. That would need
a rendered board, and no capture in this wave exercises one.

## Verification after both corrections

`dotnet test engine.tests` **183 executed / 183 passed / 0 failed / 0 skipped**, exit 0 (181 before
the two tests above) · Unity **EditMode 75/75**, **PlayMode 47/47**, 0 compile errors, C29 guard exit 0
with floors and a freshness limit · `--gates --runs 1000 --seed-prefix TUNE` **ALL GATES PASS**, every
verdict and figure identical to the tables above · `--scorer-ev --runs 400 --seed-prefix SCORER` all
five bands within 2 SE, FW/MF/DF within 0.1pp, worst band 1.2 SE.

**G6's caveat in §"The read" stands unchanged and is not affected by any of this:** the martyr guard's
tolerance (±2.15pp) is still wider than its band (2pp), so it would report PASS through a real breach.
That is a dial for Allen and it does not block this merge — the jitter mechanism cannot reach the
martyr path.
