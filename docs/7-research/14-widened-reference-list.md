# 14 — The widened reference sweep: a census, a new cohort, and three findings

**Lane:** research · **Date:** 2026-08-12 · **Status:** PROPOSAL — routed to Allen
**Mandate:** Allen, 2026-08-12 — *"the four games he named were EXAMPLES, not the boundary. Widen the
survey… grow the mapping doc's proposal set from the wider pool."*

---

## 1. Method — a census, not a longer hand-picked list

I hand-picked the Tier-2 set and got Gambonanza's genre wrong from its name (`08` §0). So this pass
enumerates the population instead. Steam's tag-filtered search endpoint, tag IDs harvested from the store
pages of four known titles, paged 100 at a time, review counts parsed inline from each row.

**Re-derivable, 2026-08-12:**
`store.steampowered.com/search/results/?query&start=N&count=100&infinite=1&category1=998&tags=<ids>`
Tag IDs — Gambling **16250** · Roguelite **3959** · Roguelike Deckbuilder **1091588** · Card Game **1666**.

### The population

| Slice | Titles |
|---|---|
| Tag: Roguelike Deckbuilder | **1,608** |
| Tag: Gambling | **1,132** |
| Gambling ∩ Card Game | 362 |
| **Gambling ∩ Roguelite** | **305** |
| **Gambling ∩ Roguelike Deckbuilder** | **93** |
| Union of the last two (this sweep's pool) | 323 |
| …of those, clearing **250 English reviews** | **26** |

**~92% of the gambling-roguelite population never reaches 250 English reviews.**
`07-business-and-roadmap.md` says *"third-wave fatigue is visible in reviews"* — that line now has a
number behind it. See **RF-16**.

### Three caveats, disclosed

1. **The census is a sample, not the population.** **Balatro does not appear in Gambling ∩ Roguelike
   Deckbuilder** despite being the genre's defining title — Steam's tag AND-filter runs on community tag
   *weights*, not ground truth. Any title whose Gambling tag ranks low is invisible to this slice. The
   counts above are therefore floors.
2. **These review counts are English-only** and run roughly half the all-language figures used in
   `06`/`12` (Buckshot 35,594 here vs 123,710 there). **Do not mix the two columns.** Internally
   consistent for ranking within this document.
3. **A parser defect was caught and fixed before publication.** `data-ds-appid` sits *before*
   `class="search_result_row"`, so splitting on the class shifted every appid by one row. The corrected
   parser anchors on the `/app/<id>` href and was verified by round-tripping six already-studied titles
   back to their own names. **No table in this document was published from the broken parse.**

---

## 2. The new cohort

Twenty new titles cleared the bar. Tiered by what each can answer that the existing eight cannot.

### Promote to full autopsy (2)

| Title | Why |
|---|---|
| **Insider Trading** (2026-02-18, $12.89, **632 all-language, Very Positive / 80.4%**) | **The nearest existing implementation of SBR's own differentiator** — a roguelike deckbuilder about market manipulation and information, in our price band. See **RF-14**; this is the most consequential find of the sweep. |
| **Nubby's Number Factory** (2025-03-07, **$4.99**, 16,573 EN reviews, Overwhelmingly Positive) | A plinko roguelike by a **solo self-publishing developer**. Canon's business comps are Balatro (5M units, "a lottery ticket, not the plan") and CloverPit (small team + publisher). **Nubby is the comp that matches `00-vision`'s actual constraint** — solo, $0 cash budget — and it is absent from canon. |

### Probe (5)

| Title | The one question |
|---|---|
| **Bills Must Be Paid** (2026-07-29, $6.99, 2,701, Overwhelmingly Positive) | Two weeks old, debt-themed, and the extreme case of the rarity-floor instrument. **See RF-15** — partially answered below. |
| **Slots & Daggers** (2025-10-24, $7.99, 4,386) | The best-reviewed slot-roguelite after CloverPit — what does it do that CloverPit does not? |
| **Dungeon Clawler** (2026-04-30, $14.99, 2,297) | Physical-machine metaphor at the **top** of the price band, unlike RACCOIN and Scritchy Scratchy. Tests RF-11's revenue half. |
| **Sol Cesto** (2026-04-10, 2,177) | "Mastering your luck" as the stated pitch — the steering question (RF-12) in another game's words. |
| **Tharsis** (2016-01-11, 1,437, **Mostly Positive**) | The genre's oldest dice-pressure design, and the only pre-third-wave title in the pool. What aged badly is cheap to learn. |

### Watchlist, no spend (13)

Umamusume: Pretty Derby (41,471 — horse racing gacha, largest in the pool, low transfer) · SOVL: Fantasy
Warfare · Menherarium: Deadly Dice · Bingle Bingle · Spin Hero · Big Winner · Fhtagn Simulator · Space
Warlord Baby Trading Simulator · LuckLand · Roll · Plinbo: Roguelike Plinko · Idle Dice 2 · Slot & Dungeons.

---

## 3. First new findings

### RF-14 — The information axis did not produce compulsion in the one place it has been tried

> **Corrected 2026-08-12, same day:** I first wrote that this title "landed Mixed". It did not — the
> all-language rating is **Very Positive (80.4%)**; the census row I read was the English-only slice, and I
> over-stated it. **The finding does not rest on the sentiment band and is stronger without it** — see
> `15-autopsy-insider-trading.md` §3, where the funnel shows 20.8% of owners beat the game and **1.7% beat
> it three times**, an 8% winner-return conversion against Balatro's 61%.

**Canon.** `03-mechanics-catalog.md`: Axis 1 Information is *"the game's soul, and our differentiation."*
`00-vision`: our differentiation is *"the sweat + cash-out, the information axis, and real betting-edge
concepts as mechanics."*

**Observed — Insider Trading** (n=387 recent English, funnel 37 achievements, pulled 2026-08-12).
A roguelike deckbuilder where you *"bend the market to your will. Synergize perks, manipulate stock prices,
and trade wisely."* That is SBR's Axis 1 with the sport swapped for a stock.

| Measure | Insider Trading | Where it ranks across the eleven titles measured |
|---|---|---|
| `luck_vs_skill` language | **30.5%** | **highest in the entire study** (next: D&DG 27.2%, LbaL 24.9%) |
| `onboard` friction | **11.4%** | **highest in the study** (next: D&DG 8.0%) |
| `depth_thin` | 20.7% | 3rd |
| Under 2h | **29%** | 2nd (Gambonanza 34.4%) |
| Positive rate (recent EN corpus) | 80.9% | 2nd lowest (Gambonanza 75.1%); **all-language is 80.4%, banded Very Positive** |
| Median playtime | 3.5h | 2nd shortest |

Funnel: tutorial 90.6% → pass week 3 58.1% → **beat the game as the Insider 20.8%**. Difficulty ladder
present and steep — Intern 14.1%, Junior 5.4%, Senior 3.1%. Rarity floor 0.1%, so the depth is real.

> *(negative, +13, 6.5h)* "**Fun game but for like 30 mins**, it lacks balancing and progression."
> *(negative, +10, 0.7h)* "Its ok. **Great concept, but the dopamine hook just isnt there.**"
> *(positive, +33)* "I rug-pulled myself. Turned my balance into -200k."

**What the research argues.** The nearest thing to SBR's stated soul, built by someone else, in our price
band, with a real difficulty ladder and genuine depth behind it, **produced the study's highest
luck-complaint rate and its highest onboarding friction, and returns only 8% of the people who beat it.**
An information axis gives players something to *understand*; on this evidence it does not by itself give
them a reason to start another run.

**What I am not claiming.** One title, n=387, a small team, and reviewers explicitly waiting on patches
("will try again when updates arrive") — this reads as under-baked as much as mis-designed. And note the
top positive: the moment players *do* celebrate is a **catastrophe they caused themselves** ("I rug-pulled
myself"), which is an agency signal, not an information one.

**Ruling requested.** Not "drop the information axis." Rather: **name its compulsion partner explicitly.**
`06` RF-8 already argues tension ≠ retention; this adds that *comprehension* ≠ retention either. SBR's
retention would then rest on the item economy and the ladder — the same two systems RF-5 and RF-12 are
about. If that is right, the three proposals are one decision.

**Falsifier.** If Insider Trading's problem is balance rather than structure, a later patched version
should move its numbers. Cheap to re-check; I would re-pull in three months.

### RF-15 — The honest short game is a strategy we have not considered

**Observed — Bills Must Be Paid** (2026-07-29, $6.99, 2,701 EN reviews, **Overwhelmingly Positive**,
n=500 corpus, 96.2% positive). *"You're broke, smash piggy banks, bills must be paid. A short active
incremental game."*

**Its rarity floor is 54.8%** — the highest ever measured on this instrument, by a factor of four and a
half over the previous extreme (Scritchy Scratchy, 12.1%). Its twelve most-common achievements all sit
above 93%. **There is essentially nothing in this game that most owners do not see.** Median playtime
4.5h; over 20h, 1%.

> *(positive, 6.1h)* "A fun game that **lasts about 5 hours**. Like all good incrementals it gets out of control after a while which is a good thing."
> *(positive, 5.9h)* "Short, cute, and fun. **Doesn't overstay it's welcome** even when aiming for 100% achievements."
> *(negative, 5.5h)* "Skills and Progression are mostly irrelevant. You just buy everything and move your mouse a bit."

**What the research argues.** Three titles in this study are Overwhelmingly or near-Overwhelmingly
Positive while being explicitly shallow and short: Bills Must Be Paid (4.5h, $6.99), Buckshot Roulette
(4.0h, $2.99), Scritchy Scratchy (11.7h, $6.99, 2.5% refund-window share). **Priced honestly against
their length, short games are not punished — they are loved.** SBR's canon assumes the opposite shape
throughout: `07-business-and-roadmap.md` bands at $8–13, and the whole economy rework is built for
long-run retention.

**Ruling requested.** Is "a tight, honest 5-hour game at $7" a strategy Allen wants on the table, or is it
explicitly rejected? It is not obviously wrong for a solo developer with a $0 cash budget and a first
shippable slice targeted under a year — `00-vision`'s own constraints. This extends **RF-11**; the two
should be ruled together.

### RF-16 — The saturation number, and what it does to the differentiation argument

**Observed.** 305 gambling-tagged roguelites; **26 of 323 in the sweep pool clear 250 English reviews**.
Roughly **92% never get there**. `00-vision`'s commercial target is 500 reviews.

**What the research argues.** The market is not "getting crowded" — it is crowded, and the survival rate
is under one in ten. This does not change SBR's design; it changes what `06` **RF-3** is worth. In a field
of 305, a store page that describes itself the way the competitor describes itself is not a marketing
detail, it is the whole filter. RF-3 was parked; I would un-park it and rule it with RF-11 and RF-15, since
all three are the same question — *what is this product, how long is it, and what does it cost.*

**Also recorded for `07-business-and-roadmap.md`.** Its comps are Balatro (5M) and CloverPit (1M). The
sweep surfaced a better one: **Nubby's Number Factory — solo developer, self-published, $4.99, 16,573
English reviews, Overwhelmingly Positive.** Canon's own constraint is "solo developer + AI collaboration,
effectively $0 cash budget." Nubby is that constraint, shipped and successful, and it is not in the doc.

---

## 4. Need Allen

Three new, none urgent against the twelve already pending:

1. **RF-14** — name the information axis's compulsion partner. Merges with RF-5, RF-8 and RF-12; I now
   believe those four are one decision, not four.
2. **RF-15 + RF-11 + RF-3 together** — product shape, length, price, and store-page legibility.
3. **RF-16** — accept the saturation number into `07-business-and-roadmap.md`, and add Nubby's Number
   Factory as the solo-dev comp.

**Next on my own initiative unless redirected:** full autopsies of Insider Trading and Nubby's Number
Factory, then the five probes. State for a fresh seating is in `13-lane-state.md`.
