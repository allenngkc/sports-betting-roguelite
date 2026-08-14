# 02 — Fun autopsy: Balatro

**Autopsied:** 2026-08-12 · **By:** research lane (Claude, Opus 5) · **Status:** DRAFT → routed to Allen

## 0. Identity and instrument

| | |
|---|---|
| **Full title** | Balatro |
| Developer / Publisher | LocalThunk / Playstack |
| Released / Price / Reviews | 2024-02-20 · $11.99 · 196,738 · Overwhelmingly Positive |
| Store | https://store.steampowered.com/app/2379780/Balatro/ · wiki: https://balatrowiki.org/ |

**Evidence basis:** achievement funnel (31 rows, pulled 2026-08-12) · review corpus (n=1,000 most-recent
English, `filter=recent`, pulled 2026-08-12) · wiki (blind structure) · **Allen has played this one.**
No play by this lane.

**Confidence ceiling: MEDIUM-HIGH.** The funnel and corpus are strong on session shape, difficulty
distribution and reception language. They are blind to moment-to-moment feel and to exact timings. Allen's
play raises the ceiling on anything he answers in `07-questions-for-allen-from-play.md`.

**Prior canon:** `01-core-loop.md` already borrows Balatro twice — "profit targets grow geometrically (the
Balatro ante)" and "unlocks rather than power creep — Balatro model" — without recording *why* either works.
This autopsy supplies the missing evidence, and it does not agree with one of the two borrowings.

---

## 1. Result cadence

**Observed (structure, wiki-verified).** A run is 8 antes; each ante is three blinds — Small (1× base
score), Big (1.5× base), Boss (2× base, some 4×). Beating the Ante-8 Showdown wins the run; Endless Mode
continues afterward. Within a blind the player has a fixed budget of hands and discards; each played hand
scores immediately.

**Observed (cadence).** Decision density is high and resolution is short: the player selects cards, plays,
and the score resolves as a chips × mult animation. Every hand is a decision; every decision resolves
within seconds.

**UNREACHED — exact timings.** Commit → resolution complete in seconds is not measurable from the funnel
or the corpus, and I did not play. Answerable in one minute by anyone with the game open; it is question 1
on Allen's sheet.

**Inferred.** The cadence is *decision-dense and resolution-light*: the drama is in choosing the hand, not
in watching it land. **Falsifier:** if the scoring animation is where players report their tension, the
reading is wrong — the corpus should then carry tension language. It carries almost none (`dread_tension`
0.4% of 1,000 reviews, the lowest of the four titles autopsied).

**Against SBR — this section is largely inapplicable, and that is the finding.** SBR's resolution is a
phase the player watches with a live cash-out; Balatro's is a beat the player triggers. Balatro can teach
SBR its economy and its difficulty ladder. It cannot teach SBR the sweat.

---

## 2. Compulsion levers

**Observed — inventory.**

| Lever | Present | Where it fires | Evidence |
|---|---|---|---|
| Escalating target | ✅ core | blind score requirement per ante | wiki: Small 1× / Big 1.5× / Boss 2× base, base rises per ante |
| Variable-ratio reward | ✅ | shop stock, Joker drops, pack contents | wiki |
| Permanent in-run growth | ✅ core | Jokers, planet-card hand levels, vouchers | funnel: "get any poker hand to level 10" 64.9% |
| Forced-choice regret | ✅ | limited Joker slots; packs offer more than you may take | wiki |
| Collection pressure | ✅ strong | discover-every-X achievements | Tarot 61.1%, Spectral 29.6%, Planet 21.2%, 100% collection 4.2% |
| Unlock drip across runs | ✅ | decks, Jokers, stakes | funnel ladder below |
| Difficulty ladder | ✅ **the retention engine** | 8 stakes, White → Gold | Red 43.7% · Black 30.2% · Gold 12.1% |
| Debt / interest | ➖ | money earns interest, but no debt and no failure-by-debt | corpus `debt_pressure` 0.1% |
| Near-miss | **UNREACHED** | — | lexicon null, see §8 |

**Observed — the corpus.** n=1,000 recent English, 97.2% positive (the highest of the four).
`addiction` language **18.8%** — the highest in the set. `luck_vs_skill` 7.5%. `boredom_quit` 1.1%.
Only 27 negatives in 1,000, and the dominant negative theme is RNG dependency.

Verbatim, helpfulness-ranked, playtime at review attached:

> "This game will tickle your brain and lure you into a cycle of deep addiction that will be all-consuming." — 108.2h
> "Its just a funny little card game. I wont get addicted right?" — 365.8h
> "When the game makes you so mad you lowk start to understand why people kill each other" — 209.5h
> *(negative)* "Got to Ante 7 and suddenly can't get past Ante 2? Too bad! Just get luckier next run..." — 4.1h

**Inferred — what carries the weight.** Ranked: (1) permanent in-run growth, (2) the stake ladder,
(3) collection. Not the escalating target. The corpus talks about builds and addiction, and almost never
about the ante requirement — which is what you would expect if the requirement is a *pacing device* rather
than a pressure lever. **Falsifier:** if the requirement were the lever, the funnel would show heavy
attrition across antes. It does not: 90.0% reach Ante 4 and 74.7% reach Ante 8.

**Pillar-4 read.** Balatro's gambling content is aesthetic, not thematic: 8.3% of reviews use
gambling/casino language, versus 32.4% for CloverPit. It took the PEGI 18 that `00-vision` anticipates,
while its own audience barely discusses it as gambling. Useful precedent: **the rating followed the imagery,
not the reception.**

---

## 3. Session shape

**Observed — the funnel (pulled 2026-08-12, % of achievement-tracked owners).**

| Milestone | % |
|---|---|
| Reach Ante 4 | **90.0** |
| Reach Ante 8 | **74.7** |
| **Win a run** (White stake) | **71.7** |
| Win on Red stake | 43.7 |
| Win on Black stake | 30.2 |
| Win on Gold stake | **12.1** |
| Win with every deck on Gold ("Completionist+") | 1.6 |
| Gold sticker on every Joker ("Completionist++") | 0.8 |

**Observed — playtime at review** (n=1,000): p10 3.9h · p25 8.9h · **median 25.1h** · p75 75.9h · p90 190.2h ·
max 1,708h. Under the 2h refund window: 3.3%. Over 50h: **33.5%**.

**Inferred — the shape.** Two games in one. A base game almost everyone finishes, and an eight-rung ladder
almost nobody finishes. Reach-Ante-8 → win-a-run converts at **96%** (74.7 → 71.7): *the last round is a
coronation, not a wall.* Attrition lives entirely in the optional ladder, which roughly halves each rung
(71.7 → 43.7 → 30.2 → 12.1). **Falsifier:** if the base game were the challenge, Ante-8 → win would leak.
It leaks 3 points.

**Against SBR.** `01-core-loop.md` leaves round count and session length open with "Balatro run ≈ 30–60 min;
right for us?" The number that matters more is the one beside it: **the median Balatro reviewer has played
25.1 hours, and 33.5% have played over 50.** A 30–60 minute run is not the retention unit — the stake
ladder is.

---

## 4. Meta hooks

**Observed.** Unlocks between runs are decks, Jokers, vouchers and stakes — content and difficulty, never
raw power carried into the next run. The ladder is 8 stakes deep. Collection is a first-class surface
(four "discover every X" achievements, 61.1% / 29.6% / 21.2% / 4.2%).

**Inferred.** The meta gates **difficulty and variety**, not power — and the difficulty ladder is doing
the retention work that the base game deliberately declines to do. **Falsifier:** if variety alone retained,
stake-win rates would not stratify so cleanly; they stratify almost perfectly by rung.

**Against SBR.** `01-core-loop.md` commits to "unlocks rather than power creep — Balatro model" and defers
ascension tiers to post-v1. **The evidence says the ascension ladder is not the optional half of the Balatro
model; it is the half that retains.** Deferring it post-v1 keeps the borrowing's name and drops its engine.
Raised as **RF-5** in `06-mapping-onto-sbr.md`.

---

## 5. The thing a summary would miss

Balatro is *generous*. Nearly three quarters of the people who launched it have beaten it, and the game's
own hardest content is opt-in. The genre reputation is "brutally difficult roguelike"; the funnel says the
opposite. Its difficulty is a menu, and the player orders it.

## 6. Transfer to SBR

**STEAL**

| What | Axis | Canon it lands in | Why it survives translation |
|---|---|---|---|
| A base difficulty most players beat, with the challenge in an opt-in ladder | 5 Economy/Meta | `10-economy-rework.md` §F (G3 band); `01-core-loop.md` meta | Nothing about it depends on instant resolution |
| The last round as coronation, not wall (96% Ante-8→win conversion) | 5 | `01-core-loop.md` "no borrowing on the final round" | Structural, not presentational |
| Collection as a retention surface separate from difficulty | 5 | `01-core-loop.md` meta progression | Our 22-item catalogue (`design/11`) already supports it |
| Requirement curve as pacing device, not pressure lever | 5 | `10-economy-rework.md` payment curve | The pressure can live in the debt instead — as it already does |

**CONFLICT** — see `06-mapping-onto-sbr.md`: **RF-4** (win-rate band vs run length) and **RF-5** (the
ascension ladder is the retention engine, not a post-v1 nicety).

**REJECT**

| What | Why it fails for SBR |
|---|---|
| Resolution-as-a-beat | Directly contradicts pillar 1; the whole reason SBR exists |
| Gambling-as-aesthetic-only | SBR's pillar 4 is satire *about* the industry — thematic, not decorative |

## 7. Comparison row

`Balatro | 2024-02-20 | $11.99 | 196,738 Overwhelmingly Positive | UNREACHED (seconds) | many/blind | none during resolve | 8 antes × 3 blinds | 25.1h median lifetime (per-run UNREACHED) | requirement rises per ante, exact curve UNREACHED | lose = fail a blind, restart free | chips × mult, multiplicative | not observed | content + difficulty ladder, no power creep | 71.7% won a run | funnel + corpus + wiki | MEDIUM-HIGH`

## 8. Sources

- Steam store + `appdetails` API — https://store.steampowered.com/app/2379780/Balatro/ — 2026-08-12 — settles identity, price, review count. Settles nothing about play.
- Steam global achievement stats — https://steamcommunity.com/stats/2379780/achievements/ — 2026-08-12 — settles the difficulty distribution across owners. Denominator is achievement-tracked owners, not sessions; it cannot give a per-run win rate.
- Steam `appreviews` API, n=1,000 recent English — 2026-08-12 — settles reception language and playtime-at-review distribution. Reviewers are not a random sample of players; recent-filter skews to the current build and to sale cohorts.
- balatrowiki.org `/w/Blinds` — 2026-08-12 — settles blind multipliers (1× / 1.5× / 2× base) and the win condition. **The per-ante base-chip table did not extract cleanly and is recorded UNREACHED** rather than asserted from memory.
- **Instrument null, reported as such (`C37`):** the near-miss lexicon matched 1 review in 4,000 across all four titles. That is a failure of the instrument — Steam reviews rarely narrate specific moments — **not evidence that near-miss levers are absent.** No near-miss claim in this lane rests on the corpus.
