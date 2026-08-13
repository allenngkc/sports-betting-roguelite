# 11 — Probe: Scritchy Scratchy — *does the $8–13 band hold, and what is the ceiling of maximum juice with near-zero decision?*

**Probed:** 2026-08-12 · research lane (Claude, Opus 5) · Tier-2, one question (two halves) · **Status:** DRAFT → Allen

## 0. Identity and instrument

| | |
|---|---|
| Full title | Scritchy Scratchy |
| Dev / Pub | Lunch Money Games / Funday Games |
| Released / Price / Reviews | 2026-03-18 · **$6.99 list, $5.59 now** · 14,629 · Very Positive |
| Store | https://store.steampowered.com/app/3948120/ |

**Evidence basis:** achievement funnel (34 rows) · review corpus (n=1,000 recent English) · store copy.
Pulled 2026-08-12. Not played. **Confidence ceiling: MEDIUM.**

Relevance: named in `00-vision` as a "third-wave juice standards, price point" reference, and
`07-business-and-roadmap.md` sets an **"$8–13 launch price band (genre-proven)"**.

---

## 1a. The price half — the band does not hold as stated

**Observed.** Scritchy Scratchy lists at **$6.99**, below the band. Against that price it posts:

| Measure | Scritchy Scratchy | Where that ranks in this study (8 titles) |
|---|---|---|
| Positive rate, recent 1,000 | **96.9%** | 2nd — behind only Balatro's 97.2% |
| `addiction` language | **19.5%** | **1st — above Balatro's 18.8%** |
| Median playtime at review | **11.7h** | 3rd, above CloverPit 9.1, LbaL 10.4*, RACCOIN 7.6, D&DG 7.0 |
| Under the 2h refund window | **2.5%** | **lowest in the study** (next best Balatro 3.3%) |
| Negatives in 1,000 | **31** | fewest in the study |
| Reviews accumulated | 14,629 in ~5 months | 3rd, behind Balatro and Buckshot |

\* LbaL's 10.4h median is marginally above; the ordering of those two is inside noise.

**Answer to the price half.** The band's *floor* is not genre-proven. A **$6.99** title produced the highest
compulsion language and the lowest refund-window share in this entire study, and accumulated 14,629 reviews
in five months. `07-business-and-roadmap.md` also plans for review count as a success threshold (500
reviews) — and the two cheapest titles in the study, Buckshot at $2.99 and this at $6.99, hold the highest
review counts per dollar by a distance.

**What this does not establish: revenue.** Review count and sentiment are not net income, and `00-vision`'s
commercial target is *$15K net*. A $6.99 price needs roughly twice the units of a $13 one for the same
money. **This probe argues the band's premise is wrong, not that the band's number is wrong** — those are
different claims and only the first is supported. Raised as **RF-11**.

## 1b. The juice-ceiling half — and a new instrument

**Observed — the rarity floor.** Scritchy Scratchy's **rarest achievement of 34 is 12.1%**. Every other
title in this study has a rarest achievement between 0.1% and 1.5%:

| Title | Rarest achievement | Achievements | Median playtime | Over 50h |
|---|---|---|---|---|
| Gambonanza | 0.1% | 53 | 3.3h | 0.8% |
| Luck be a Landlord | 0.2% | 186 | 10.4h | 10.6% |
| Dungeons & Degenerate Gamblers | 0.2% | 39 | 7.0h | 7.2% |
| RACCOIN | 0.6% | 23 | 7.6h | 3.5% |
| Balatro | 0.8% | 31 | 25.1h | 33.5% |
| Buckshot Roulette | 1.0% | 16 | 4.0h | 0.4% |
| CloverPit | 1.1% | 30 | 9.1h | 10.6% |
| **Scritchy Scratchy** | **12.1%** | 34 | 11.7h | **3.6%** |

**The rarity floor measures how much of a game is reachable by almost everyone.** A floor of 12.1% means
there is essentially nothing in Scritchy Scratchy that most players do not see. I am proposing it as a
standing instrument for this lane — it is one HTTP request, it is public, and it separates "engagement" from
"completion space" cleanly.

**Answer to the juice half.** Maximum juice with near-zero decision **does not cap engagement** — 19.5%
addiction language, 11.7h median, 96.9% positive, all excellent. **It caps the *tail*.** Scritchy Scratchy
has a high median and a low tail (3.6% over 50h) — a game almost everyone finishes and then leaves. Balatro
has a high median *and* a high tail (33.5%) — a game almost nobody finishes.

The corpus says the same thing in words. Only 31 negatives in 1,000, and they cluster on exactly this:

> *(negative)* "Wish it was a little bit longer, more content, could have been a free title to build a fanbase. **Prestige is lazy game design**" — 14.6h
> *(negative)* "there is really not that much content in the full game that isn't already in the demo" — 8.6h
> *(negative)* "Not really an Idle game, Not really a skill based game, Not really a roguelike, Not really anything. Felt like they copied the idea of cloverpit a bit but left all fun things behind" — 10.4h

Note the playtimes on those complaints: 14.6h, 8.6h, 10.4h. **Nobody is complaining early.** The juice
works; it runs out.

> *(positive, +51)* "Game so fun it made me realize I have a gambling addiction, 10/10 definitely would recommend" — 15.4h
> *(positive, +27)* "Winning basically the universe and then going back to washing dishes is the catharsis that I needed." — 21.3h

Other corpus figures: `gambling_real` 26.0% · `idle_auto` 6.2% · `depth_thin` 11.3% · `luck_vs_skill` only
3.7% (lowest in the study — there are no build decisions to argue about).

**Falsifier.** If juice alone retained, the >50h tail would not sit at 3.6% while the median sits at 11.7h.
If juice did *not* work, the refund-window share would not be the lowest in the study at 2.5%. Both hold,
and they point at different halves of the curve.

## 6. Transfer to SBR

**STEAL**

| What | Axis | Canon it lands in | Why |
|---|---|---|---|
| **The rarity floor as a standing instrument** — check it on any reference, and eventually on ourselves | — | `01-autopsy-template.md` §3 (add to the funnel block) | One request; separates engagement from completion space |
| Repeated-death achievements as a comedy beat — "Die", "Die twice", "Die three times", "House always wins" (95.6 / 79.8 / 69.0 / 61.2%) | 5 / voice | extends **S1**; `00-vision` pillar 4 | Free, on-theme, and it normalises losing |
| "Don't read the fine print" — taking a loan, held by **81.4%** of owners | 5 | `10-economy-rework.md` A (debt) | Our debt is the core system; naming its first use honestly is the same trick as Buckshot's "Chasing Losses" (**S5**) |

**CONFLICT** — **RF-11** (the price band's premise) in `12-probe-set-findings.md`.

**REJECT**

| What | Why |
|---|---|
| Prestige as the retention layer | Its own audience names it "lazy game design", at 14.6h played |
| Near-zero decision density | SBR's whole differentiation is decisions on information; this is the opposite bet |

## 8. Sources

Steam `appdetails` + `appreviews` (n=1,000 recent English) + `steamcommunity.com/stats/3948120/achievements/`
— 2026-08-12. The funnel settles the rarity floor. The corpus settles sentiment, playtime and complaint
timing. **Neither settles revenue**, which is the half of the price question that actually decides
`07-business-and-roadmap.md` — that would need a sales estimator, and third-party estimates were not used
here because their method is not re-derivable (`C34`).
