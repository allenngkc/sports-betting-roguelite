# 15 — Fun autopsy: Insider Trading

**Autopsied:** 2026-08-12 · research lane (Claude, Opus 5) · **Status:** DRAFT → routed to Allen
**Why it earned a full autopsy:** it is the nearest existing implementation of SBR's own stated soul —
`03-mechanics-catalog.md` Axis 1, Information, *"the game's soul, and our differentiation."*

## 0. Identity and instrument

| | |
|---|---|
| Full title | Insider Trading |
| Dev / Pub | Naiive (self-published) |
| Released / Price | 2026-02-18 · **$12.89** — inside SBR's $8–13 band |
| Reviews | **632 all-language, Very Positive** (508 pos / 124 neg = 80.4%) · 594 English |
| Store | https://store.steampowered.com/app/3166810/ |

**Evidence basis:** achievement funnel (37 rows, full) · review corpus (n=387 recent English — the full
pool) · store copy. Pulled 2026-08-12. Not played. **Confidence ceiling: MEDIUM**, and lower than usual on
sentiment: n=387 is the smallest corpus in the study and 632 all-language reviews is a thin base.

### Correction to my own §2 and §3 of `14-widened-reference-list.md` (§1.5, mine)

**I described this title as having "landed Mixed". That is wrong.** The census row read *Mostly Positive*
(English-only slice) and the all-language rating is **Very Positive** at 80.4%. I over-stated the band in
both places `14` mentions it, and `14` is corrected in the same commit as this file.

**The finding does not depend on the band, and is stronger without it.** See §3 — the funnel contains a
direct retention measurement that no sentiment figure can match.

**Prior canon:** none. New reference, surfaced by the widened census.

---

## 1. Result cadence

**Observed (structure, store copy).** A run is a sequence of **market days** building toward a **weekly
financial target**. Each day the player plays cards from a deck to move a stock price; between days comes
the **Aftermarket**, a draft where cards are added — *"often taking upside alongside risk. Some cards
strengthen your deck. Others weaken it."* A **Greed** mechanic accelerates gains and risk together, and —
the line that matters for us — *"only you can decide when to cash out and secure your profits."*
120+ cards, 60+ stackable pills/perks, 12+ playable characters with unique decks.

**Observed (cadence).** Decision-dense, resolution-light — Balatro's shape. The player commits a hand, the
price moves, the day resolves.

**UNREACHED — exact timings.** No play, no footage pass.

**Against SBR — the structural echo is close enough to be uncomfortable.**

| Insider Trading | SBR |
|---|---|
| weekly financial target | round profit target / debt payment (`10-economy-rework.md` A) |
| Greed: push harder, gain and risk rise together | stake sizing against a bounded `p` |
| **"you decide when to cash out"** | **the live cash-out — pillar 1's signature moment** |
| Aftermarket draft, some cards weaken you | the dealt-hand shop (`design/11`) |
| price you move yourself | odds you move with information |

This is the closest structural neighbour SBR has that has actually shipped — closer than Balatro,
CloverPit or Parlay on the mechanics, if not on the theme.

---

## 2. Compulsion levers

**Observed — inventory.** Escalating target ✅ (weekly targets, *"push too hard and the cost of entry
skyrockets"*). Permanent in-run growth ✅ (deck + pills). Forced-choice regret ✅ (draft, limited pill
slots). **Risk-doubling ✅ (Greed)**. **Cash-out ✅** — one of only two titles in this study to have one,
the other being Buckshot Roulette. Difficulty ladder ✅, six rungs. Collection ✅, 12+ characters.
Debt ❌. Pity ❌ / unobserved.

**Observed — the corpus** (n=387, frozen lexicon, contamination-checked — no pattern term appears in this
title's name or core verb):

| Family | % | Rank across the eleven titles measured |
|---|---|---|
| `luck_vs_skill` | **30.5%** | **1st** (D&DG 27.2, LbaL 24.9) |
| `onboard` | **11.4%** | **1st** (D&DG 8.0) |
| `depth_thin` | 20.7% | 3rd |
| `addiction` | 6.7% | 9th of 11 |
| `dread_tension` | 0.5% | low |

Negative themes among the 74 negatives: balance 12.2% · bugs/tech 10.8% · too hard 8.1% · early-access /
"waiting on updates" 5.4% · depth 4.1%.

> *(neg, +13, 6.5h)* "**Fun game but for like 30 mins**, it lacks balancing and progression."
> *(neg, +10, 0.7h)* "Its ok. **Great concept, but the dopamine hook just isnt there.** Will try again when updates and full Steam Deck support arrive."
> *(neg, +7, 1.5h)* "I didn't feel like the game was really going anywhere interesting. **Felt like I'd seen everything in the first hour**, and there wasn't meaningful content worth pursuing."
> *(pos, +33, 4.0h)* "**I rug-pulled myself.** Turned my balance into -200k."
> *(pos, +10, 0.5h)* "Jordan Gecco once said 'greed is good', but he forgot to mention it's even better when you've **crashed the global economy from your laptop while wearing pajamas!**"

**Inferred — what carries the weight, and what does not.** The two most-upvoted positives are both about a
**self-inflicted catastrophe the player caused and understood**. Neither is about information, prediction,
or being right. That is an *agency* pleasure, not an informational one.

**Falsifier:** if comprehension were the draw, the positives would celebrate a correct read of the market.
Across the corpus's top positives, none does.

**Pillar-4 read.** The satire lands — "crashed the global economy from your laptop while wearing pajamas"
is precisely the register `00-vision` pillar 4 asks for, achieved with theme and voice rather than lecture.
Cheap and repeatable.

---

## 3. Session shape — the measurement that makes this autopsy worth reading

**Observed — the full funnel** (37 achievements, floor 0.1%, pulled 2026-08-12):

| Milestone | % |
|---|---|
| Complete the tutorial | 90.6 |
| Pass week 3 | 58.1 |
| **Beat the game (as the Insider)** | **20.8** |
| Beat Intern difficulty | 14.1 |
| Win with over $1,000,000 | 10.1 |
| Beat Junior / Senior / Director / Chairman / Oligarch | 5.4 / 3.1 / 2.0 / 1.3 / 1.0 |
| **"VIP — Win three or more games"** | **1.7** |

**One in five owners beats this game. Fewer than one in fifty beats it three times.**

Set that against Balatro, whose funnel measures the same thing: 71.7% win a run, and 43.7% then win on Red
Stake — **61% of Balatro's winners climb at least one rung.** Insider Trading's equivalent conversion is
**1.7 / 20.8 = 8%.** Ninety-two percent of the people who beat this game beat it and put it down.

**And it is not for lack of content.** The funnel is 37 achievements deep with a 0.1% floor, twelve
characters, six difficulty rungs, 120+ cards and 60+ perks. Everything RF-5 says a game needs is present.
**The completion space exists and players do not enter it.**

**Observed — playtime** (n=387): median **3.5h** · under 2h **29%** (2nd highest in the study, after
Gambonanza's 34.4%) · over 20h 7% · over 50h 1.6%.

**Falsifier.** If the 8% conversion were caused by difficulty, "beat Intern" (14.1%) would not sit *below*
"beat the game" (20.8%) — a player who beats the game clearly can beat a rung. It is not a wall; it is a
lack of reason to return.

---

## 4. Meta hooks

**Observed.** Twelve-plus unlockable characters with unique decks and mechanics; six difficulty rungs; a
per-character × per-difficulty completion matrix (every `X+` achievement is "beat Oligarch as X", all at
0.1–0.5%). Structurally this is Balatro's stake × deck matrix.

**Inferred.** SBR's canon and my own RF-5 both assume the matrix *is* the retention engine. **Insider
Trading has the matrix and does not retain.** The matrix is therefore necessary-at-best, not sufficient —
a further correction to the RF-5 family, arriving from a different direction than Luck be a Landlord's.

---

## 5. The thing a summary would miss

The store page says *"This is not a trading simulator."* The developer went out of their way to promise the
game is **not** about modelling a market correctly — and then the top complaint is that it lacks progression
and the top praise is about blowing yourself up. The informational fantasy was deliberately subordinated to
the toy, by the developer, and the audience still wanted more toy. For a project whose canon calls
information *"the game's soul"*, that is the most useful sentence in this document.

## 6. Transfer to SBR

**STEAL**

| What | Axis | Canon | Why |
|---|---|---|---|
| **Self-inflicted catastrophe as a celebrated outcome** — let the player blow themselves up legibly, and let them tell the story | 4 Resolution / voice | `04-the-sweat.md`, `surething-design.md` §6 | The two top positives here are both this. Our sweat is built to deliver exactly it |
| Greed: one dial that raises gain and risk together, player-controlled | 3 Capital / 4 | `02-betting-math.md`, `04-the-sweat.md` | Legible, writable for the Monte Carlo audit, and it pairs with RF-7's cash-out constraint |
| Satire through voice and theme, not exposition | voice | `00-vision` pillar 4 | Free |
| A draft where some offers are **downside** taken alongside upside | 5 | `design/11` dealt-hand shop | Adds a real decision to a shop that currently only adds power |

**CONFLICT** — **RF-14** (`14` §3), sharpened here by the 8% winner-return conversion. Also feeds the
RF-5 correction thread.

**REJECT**

| What | Why |
|---|---|
| Relying on a character × difficulty matrix to retain | This title has one, 37 achievements deep, and converts 8% of its winners |

## 7. Comparison row

`Insider Trading | 2026-02-18 | $12.89 | 632 all-lang Very Positive (80.4%) | UNREACHED (seconds) | high per market day | none during resolve | days → weekly targets | 3.5h median lifetime | targets rise; entry cost scales with Greed | miss the weekly target = run over | cards × pills, multiplicative | not observed | 12+ characters × 6 difficulty rungs | 20.8% beat it, 1.7% beat it three times | funnel + corpus (n=387) + store copy | MEDIUM`

## 8. Sources

- Steam `appdetails` — https://store.steampowered.com/app/3166810/ — 2026-08-12 — settles structure claims in §1 (all from the developer's own copy, which is marketing and is treated as such), price, and the all-language rating.
- `steamcommunity.com/stats/3166810/achievements/` — 2026-08-12 — **the load-bearing instrument.** Settles the completion funnel and the winner-return conversion. Denominator is achievement-tracked owners.
- Steam `appreviews`, n=387 recent English (full pool) — 2026-08-12 — settles complaint themes and playtime. **Smallest corpus in the study; every percentage carries a wider error bar than the Tier-1 figures.**
- Contamination check run: no frozen-lexicon term appears in this title's name or core verb. `gambling_real` (1.0%) is clean here.
- **Known-dead families not reported:** `near_miss`, `cash_out` (`C37`).
- **Time-sensitivity:** multiple reviewers are explicitly waiting on patches. A re-pull in three months is the falsifier for RF-14 and costs one request.
