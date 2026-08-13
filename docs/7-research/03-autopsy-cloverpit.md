# 03 — Fun autopsy: CloverPit

**Autopsied:** 2026-08-12 · **By:** research lane (Claude, Opus 5) · **Status:** DRAFT → routed to Allen
**SCOPED:** the felt half only. The math is already canon — see §0.

## 0. Identity and instrument

| | |
|---|---|
| **Full title** | CloverPit |
| Developer / Publisher | Panik Arcade / Future Friends Games |
| Released / Price / Reviews | 2025-09-26 · $5.99 (from $9.99) · 25,496 · Very Positive |
| Store | https://store.steampowered.com/app/3314790/CloverPit/ · https://en.wikipedia.org/wiki/CloverPit |

**Evidence basis:** achievement funnel (30 rows, 2026-08-12) · review corpus (n=1,000 recent English,
2026-08-12) · `design/09` for all mechanics · **Allen has played this one.** No play by this lane.

**Confidence ceiling: MEDIUM-HIGH**, same shape as Balatro's.

**Prior canon — the non-duplication clause is live.** `design/09-cloverpit-math-comparison.md` (2026-07-12)
settled the payout formula, symbol weights, the Luck pity schedule, the 9-deadline requirement curve
(75 → 200 → 666 → 2,222 → 12,500 → 33,333 → 66,666 → 200,000 → 1,000,000, avg ×3.3), 7%/round interest, the
666 counterattack, and the charm taxonomy. `design/11` shipped 17 items translated from it. **None of that
is re-opened here, and nothing below contradicts it.** This autopsy adds only what design/09 did not ask:
what the thing feels like, who finishes it, and where players stop.

---

## 1. Result cadence

**Observed (structure, from `design/09`).** Nine deadlines × 3 rounds each, 3–7 spins per round. The unit
of play is one spin. The unit of *pressure* is the deadline.

**Observed (cadence shape).** Resolution is a slot spin: commit, reels stop, payout resolves. Decision
density is low per spin and concentrates between rounds, in the drawer/charm economy.

**UNREACHED — exact timings.** Not measurable from funnel or corpus. On Allen's sheet.

**Inferred.** CloverPit inverts Balatro. Balatro is decision-dense with a thin resolution; CloverPit is
decision-*sparse* with a thin resolution, and buys its tension from **the countdown between spins** — the
debt clock, not the reels. The funnel supports this directly: **56.0% "Near Death Experience — survive to
the Death Countdown."** More than half of all owners have been inside the game's dread mechanic, which sits
*between* results, not inside them.

**Falsifier:** if the spin itself carried the dread, the corpus would show tension language. It shows 1.2%.

**Against SBR — the most useful line in this autopsy.** CloverPit proves a game can be oppressive and
compulsive **with a resolution nobody watches for tension**, because the pressure lives in a persistent,
visible, always-worsening obligation. SBR has that obligation (the debt) *and* a watched resolution. Those
are two independent tension sources and canon currently treats the sweat as the only one.

---

## 2. Compulsion levers

**Observed — inventory** (mechanics per `design/09`; prevalence per the funnel).

| Lever | Present | Evidence |
|---|---|---|
| Debt / deadline pressure | ✅ **core** | the deadline curve; 56.0% reached the Death Countdown |
| Escalating target | ✅ core, fixed and public | ×3.3 per deadline, identical every run (`design/09`) |
| Interest / save-vs-spend | ✅ | 7%/round; funnel: "Deposit 1 Million" 68.7%, "1 Billion" 41.6%, "Interest ≥30%" 17.2% |
| Pity / deterministic mercy | ✅ **and learnable** | Luck schedule + 4-dead-spin rubber-banding (`design/09`) |
| Permanent in-run growth (ratchets) | ✅ core | Pentacle, Diesel Locomotive (`design/09`) |
| Jackpot made visible | ✅ | "Lucky Day — first Jackpot" 92.5%; "Ultimate Jackpot" 37.9% |
| The house counterattacks | ✅ distinctive | "The Number of the Beast — obtain a 666" 90.0%; "Lose 1 Million to a 666" 48.5% |
| Collection | ✅ | Memory Cards 15.4% full; all Lucky Charms 1.5% |
| Onboarding-to-failure | ✅ **designed** | "Aw Dangit! — Die for the first time" **97.0%** |
| Near-miss | UNREACHED | lexicon null (§8) |

**Observed — the corpus.** n=1,000 recent English, 89.7% positive (103 negatives).
`gambling_real` **32.4%** — four times Balatro's 8.3%, the highest in the set.
`addiction` 15.1%. `luck_vs_skill` 14.0%. `debt_pressure` 2.3%. `boredom_quit` 2.2%.

> "One of the most addictive roguelike gambling games to date, and i havnt found one better." — 53.4h
> "This game captures the same lighting in a bottle that balatro did but with gambling." — 0.6h
> *(negative)* "Kinda fun but... it is very much still mainly rng even with an optimized build, and for that reason I can not recommend it since it fails to even be a game in my eyes." — 66.3h
> *(negative)* "It's fun but the percentages are just a smokescreen." — 1.8h

**Inferred — what carries the weight.** (1) The deadline, (2) the learnable pity schedule, (3) the 666
counterattack. The first is what players *feel*; the second is what converts them from gamblers into
engineers — `design/09` already recorded the player phrase for it, *"no longer gambling — engineering
fate"*, which is almost verbatim SBR's own fantasy line in `00-vision` ("Not 'get lucky' — **engineer
luck**"). **Falsifier for (2):** if decoding the pity schedule were not the mid-game skill, the negative
corpus would not cluster on "still mainly rng" — the complaint of players who did *not* decode it.

**Pillar-4 read — the most transferable finding in this document.** A third of CloverPit's reviewers
discuss it explicitly as gambling. Its positive rate is 89.7% against Balatro's 97.2%. **Leaning into the
gambling theme costs roughly 7.5 points of positive sentiment and buys a much louder cultural conversation
— 1M+ copies in under two months.** That is the trade `00-vision` pillar 4 signs SBR up for, now with a
number attached rather than an assumption.

---

## 3. Session shape

**Observed — the funnel (2026-08-12).**

| Milestone | % |
|---|---|
| Die for the first time | **97.0** |
| First Jackpot | 92.5 |
| Obtain a 666 | 90.0 |
| Unlock Drawer 1 / 2 / 3 / 4 | 82.4 / 68.6 / 59.5 / 52.4 |
| Survive to the Death Countdown | 56.0 |
| Deposit 1 Billion | 41.6 |
| **"The Structure" — get out of the Room (the win)** | **30.9** |
| Ascension (+1 on the wall) | **9.1** |
| Unlock all Lucky Charms | 1.5 |

**Observed — playtime at review** (n=1,000): p10 2.5h · p25 4.4h · **median 9.1h** · p75 20.6h · p90 52.6h ·
max 453h. Under 2h: 8.3%. Over 50h: 10.6%.

**Inferred.** A clean four-stage funnel: everyone dies (97.0) → most reach the machine's spectacle
(92.5 jackpot, 90.0 six) → half hit the dread mechanic (56.0) → **fewer than a third ever escape (30.9)**
→ one in eleven climbs the ascension rung (9.1). Median lifetime 9.1 hours — roughly a third of Balatro's.
**Falsifier:** if the drop from 56.0 to 30.9 were pacing rather than difficulty, the negative corpus would
complain about length; it complains about RNG instead.

**Against SBR.** CloverPit is the *harder* of the two games Allen has played and it still lets **30.9% of
owners win**. That is the softest completion rate in the entire Tier-1 set, and it is four to six times
SBR's ruled band. See **RF-4**.

---

## 4. Meta hooks

**Observed.** Drawers unlock across runs (82.4 → 52.4 through four). Memory Cards are a collection surface
(15.4% complete it; 1.1% make them all rainbow holographic). Ascension is a single visible wall-number rung
(9.1%). Charms are the in-run power layer, not a between-run one.

**Inferred.** The meta is thin by Balatro's standard — one ascension rung against Balatro's eight stakes —
and the playtime data tracks it: median 9.1h vs 25.1h. **Falsifier:** if the thin meta were not the
constraint, CloverPit's >50h share would resemble Balatro's; it is 10.6% against 33.5%.

**Against SBR.** This is the cleanest natural experiment available. Two games, same genre, same era, both
with strong in-run item economies. The one with an eight-rung ladder retains ~2.75× the median hours of the
one with a single rung. That is correlational, not causal — but it is the second independent line of
evidence for **RF-5**.

---

## 5. The thing a summary would miss

The pressure is not in the machine, it is in the room. The debt counter, the phone, the drawers, the
countdown — the slot is where you *act*, but the tension is authored in the space around it. SBR already
owns a room. `room-design.md` and the phone surface are, on this reading, not set dressing for the sweat;
they are the second tension system, and CloverPit is the proof it works.

## 6. Transfer to SBR

**STEAL**

| What | Axis | Canon it lands in | Why it survives translation |
|---|---|---|---|
| Tension authored *between* results, in the room and on the phone | 4 Resolution / 5 | `room-design.md`, `phone-design.md`, `04-the-sweat.md` | Independent of resolution length — it works *alongside* the sweat |
| Celebrate the first loss (97.0% "Aw Dangit!") | 5 | `01-core-loop.md` failure state | Pure onboarding; costs nothing |
| An invisible-but-learnable mercy schedule as the mid-game skill | 1 Information | `10-economy-rework.md` E (bounded-p) | Our bounded-p doctrine forbids the *luck* version; a **pity on the information axis** is unexplored and is ours to invent |
| A fixed, public requirement curve that announces linear play is dead | 5 | `10-economy-rework.md` payment curve | Already how our debt curve behaves — make it *visible*, as CloverPit does |

**CONFLICT** — **RF-4** (win-rate band) and **RF-5** (ladder depth) in `06-mapping-onto-sbr.md`.

**REJECT**

| What | Why it fails for SBR |
|---|---|
| Luck-as-outcome-forcing | Already forbidden — `10-economy-rework.md` E, bounded-p doctrine. Correctly forbidden; the negative corpus shows the cost of a payout system players read as a "smokescreen" |
| A single ascension rung | The evidence in §4 says that is CloverPit's ceiling, not its virtue |

## 7. Comparison row

`CloverPit | 2025-09-26 | $5.99 | 25,496 Very Positive | UNREACHED (seconds) | low per spin, high between rounds | none during resolve | 9 deadlines × 3 rounds, 3–7 spins | 9.1h median lifetime | ×3.3 per deadline, fixed and public | debt deadline missed = run over | 4 unbounded multiplicative factors | YES — Luck schedule + rubber-band | drawers + one ascension rung + collection | 30.9% escaped | funnel + corpus + design/09 | MEDIUM-HIGH`

## 8. Sources

- Steam store + `appdetails` — https://store.steampowered.com/app/3314790/CloverPit/ — 2026-08-12.
- Steam global achievement stats — https://steamcommunity.com/stats/3314790/achievements/ — 2026-08-12 — settles the completion funnel; denominator is achievement-tracked owners.
- Steam `appreviews`, n=1,000 recent English — 2026-08-12 — settles reception language and playtime distribution; recent-filter skews to current build and sale cohorts.
- https://en.wikipedia.org/wiki/CloverPit — 2026-08-12 — settles release, platforms, sales claim (1M+).
- `design/09-cloverpit-math-comparison.md` (2026-07-12, internal) — all mechanics claims above. Not re-derived.
- **Instrument null (`C37`):** near-miss lexicon, 1 hit in 4,000 reviews across the set. Instrument failure, not absence. No claim here rests on it.
