# 09 — Probe: Luck be a Landlord — *how does a rent-escalation curve fail after three years of tuning?*

**Probed:** 2026-08-12 · research lane (Claude, Opus 5) · Tier-2, one question · **Status:** DRAFT → Allen
**This probe falsified part of my own RF-5.** See §1b and `12-probe-set-findings.md`.

## 0. Identity and instrument

| | |
|---|---|
| Full title | Luck be a Landlord |
| Dev / Pub | TrampolineTales (self-published) |
| Released / Price / Reviews | 2023-01-06 · $9.99 · 11,517 · Very Positive |
| Store | https://store.steampowered.com/app/1404850/ |

**Evidence basis:** achievement funnel (**186 rows** — the largest in this study) · review corpus (n=1,000
recent English) · store copy. Pulled 2026-08-12. Not played. **Confidence ceiling: MEDIUM.**

Relevance: `10-economy-rework.md` §A moved SBR onto **debt payments deducted every settle**. LbaL is the
ancestor of that structure — rent due every N spins, escalating, miss it and the run ends — and it has had
three years of live tuning. Known failure modes of the ancestor are cheap insurance on a five-week-old
structural decision.

---

## 1. The one question — where does the curve actually fail?

**Observed — the failure is not the curve.** The corpus does not complain about the rent escalation.
`too_hard` language runs at **1.5%** (15 of 1,000) — the lowest of any title in this study. `too_easy` is
0.1%. After three years of tuning, the escalation curve is simply not a source of friction.

**Observed — what players complain about instead.** The single most-upvoted negative:

> *(negative, +14, 3.4h)* "**Symbols are too random, it's too hard to try and plan a build because there's no way I can see to increase your odds of getting specific symbols** so it just gets frustrating"

That is a **build-determinism** complaint, not a difficulty complaint. `luck_vs_skill` language runs 24.9%
overall and appears in 47 of the 107 negatives — **44% of all negative reviews**, the dominant negative
theme by a wide margin. `depth_thin` 14.7%.

Other corpus figures: 89.3% positive · median playtime **10.4h** · p90 52.8h · max 2,619h · 9.7% under two
hours · 10.6% over fifty. `addiction` 16.8% · `gambling_real` **24.2%** (second only to CloverPit's 32.4%).

> *(positive, +35)* "Feeds my gambling addiction without burning actual cash 10/10"
> *(positive, +26)* "a game where you must gamble to be able to stay in a place of living just like the real world. 10/10 for realism"

**Answer.** After three years of tuning, an escalating-payment curve stops being the failure point entirely.
**The failure migrates to the player's inability to steer toward a build** — you can survive the rent, but
you cannot reliably assemble the thing that survives it, and that is what people write reviews about. The
curve is solved; the *agency over the curve* is not.

**Falsifier.** If the curve were still the problem, `too_hard` would not be the lowest in the study at 1.5%
while `luck_vs_skill` sits at 24.9% and dominates the negatives. Both conditions hold.

**Direct application to SBR.** `design/11` ships a **dealt-hand shop** — the player is offered items rather
than choosing from a full catalogue. That is exactly the structure LbaL's top negative is about. SBR's
payment curve is sim-refereed and will get tuned; **the thing that will still be broken after tuning is
steering.** Raised as **RF-12**.

---

## 1b. The finding that corrects my own RF-5

**Observed — the full apartment-floor ladder** (win a game on floor N, % of owners, 2026-08-12):

| Floor | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 |
|---|---|---|---|---|---|---|---|---|---|---|
| % | 59.1 | 49.1 | 44.4 | 40.3 | 37.2 | 34.7 | 32.9 | 31.1 | 29.5 | 26.7 |

| Floor | 11 | 12 | 13 | 14 | 15 | 16 | 17 | 18 | 19 | 20 |
|---|---|---|---|---|---|---|---|---|---|---|
| % | 24.5 | 22.6 | 21.0 | 19.5 | 18.3 | 16.1 | 14.9 | 13.9 | 13.1 | **12.4** |

The gradient **decelerates**: −10.0 on the first step, then flattening to −0.7 by floor 20. "Landlord
Defeated" sits at 33.8%. 186 achievements, rarest 0.2%.

**Why this matters.** `06-mapping-onto-sbr.md` RF-5 argued *"ladder depth tracks retention"* from four
points (0 rungs → 4.0h, 1 rung → 9.1h, flat layer → 7.6h, 8 rungs → 25.1h). **LbaL is a fifth point and it
breaks the proxy: a twenty-rung ladder with a median lifetime of 10.4h — well under half Balatro's 25.1h.**

Both ladders terminate at almost exactly the same rarity (Balatro's Gold Stake 12.1%, LbaL's Floor 20
12.4%). Balatro gets there in **three** rung-steps; LbaL takes **nineteen**. The retention difference is
2.4× in Balatro's favour. **Rung count is not the variable.**

RF-5's *recommendation* survives — every title with no ladder at all sits at 4.0–7.6h — but the
strong-form claim does not, and I have withdrawn it. Restated in `12-probe-set-findings.md`.

## 6. Transfer to SBR

**STEAL**

| What | Axis | Canon | Note |
|---|---|---|---|
| A gentle first rung. LbaL's largest single drop is floor 1 → 2 (−10.0); every later rung asks less | 5 | RF-5 successor | If SBR ships rungs, front-load the difficulty of the *first* one and flatten after |
| Rent framing that reads as satire without a word of exposition — 24.2% gambling language, and the top positives are political jokes | 5 / voice | `00-vision` pillar 4 | The fiction does the satire; no lecture needed |

**CONFLICT** — **RF-12** (build steering in a dealt-hand shop) and the **RF-5 correction**, both in
`12-probe-set-findings.md`.

**REJECT**

| What | Why |
|---|---|
| A twenty-rung linear ladder | This probe's own data: 20 rungs, 10.4h. Length is not the lever |

## 8. Sources

Steam `appdetails` + `appreviews` (n=1,000 recent English) + `steamcommunity.com/stats/1404850/achievements/`
— 2026-08-12. The funnel settles the ladder shape precisely (186 achievements, floors individually tracked).
The corpus settles complaint themes; it cannot tell us what the developer changed across three years of
patches, so "after tuning" is inferred from the title's age and its live-service history, not from a changelog
I read.
