# 04 — Fun autopsy: RACCOIN: Coin Pusher Roguelike

**Autopsied:** 2026-08-12 · **By:** research lane (Claude, Opus 5) · **Status:** DRAFT → routed to Allen

## 0. Identity and instrument

| | |
|---|---|
| **Full title** | **RACCOIN: Coin Pusher Roguelike** (canon records the short form "Raccoin") |
| Developer / Publisher | Doraccoon (Shanghai, 3 people) / **Playstack — Balatro's publisher** |
| Released / Price / Reviews | 2026-03-31 · $9.59 (from $11.99) · 4,495 · Very Positive |
| Store | https://store.steampowered.com/app/3784030/ · https://en.wikipedia.org/wiki/Raccoin:_Coin_Pusher_Roguelike |

**Evidence basis:** achievement funnel (23 rows, 2026-08-12) · review corpus (n=1,000 recent English,
2026-08-12) · Wikipedia gameplay + development. **Not played by anyone on this project.**

**Confidence ceiling: MEDIUM.** No first-hand play at all. Structure is wiki-sourced; everything about feel
is inference from corpus language.

**Prior canon:** `00-vision.md` cites it as a "third-wave juice standards, price point" reference;
`07-business-and-roadmap.md` cites "100K in 24h (Mar 2026)". Both hold. Neither records the publisher.

---

## 1. Result cadence

**Observed (structure, Wikipedia).** A run is **15 rounds**. Each round: shoot a limited supply of coins
into a coin pusher to hit a score target. Scored coins yield tickets and progress toward the target. Hit
the target → shop. Run out of coins short of the target → exchange tickets for coins, or the run ends.
Between rounds: buy special coins, expand the coin clip, buy prizes and chips. Some rounds carry "bad
coins" (debuffs); clearing them all grants a keychain.

**Observed (cadence).** The resolution is physical and cascading — a coin is launched, the pile shifts, coins
fall, score accrues. This is the longest resolution of the three named references and the only one with
genuine visual continuation after commit.

**UNREACHED — exact timings.** No play, no footage timing. Not on Allen's sheet either (he has not played
it); would need a footage pass to close.

**Inferred.** RACCOIN buys its feel from *cascade*: one input, many small results, arriving over a second or
two. That is a materially different pleasure from Balatro's single scoring beat, and it is the closest thing
in the named set to a resolution the player watches. **Falsifier:** if the cascade were the draw, the corpus
would show satisfaction language about the physics; the strongest negative in the corpus says the opposite
— "It's missing something, it's not that satisfying to play, the big scores feels underwhelming" (+89
helpful votes). That is evidence *against* my own reading, recorded rather than dropped.

**Against SBR.** A cascade is not a sweat. It is one commit resolving into many sub-results with no decision
in between — SBR's sweat has decisions inside it (cash-out). Closer than Balatro or CloverPit; still not it.

---

## 2. Compulsion levers

**Observed — inventory.**

| Lever | Present | Evidence |
|---|---|---|
| Escalating target | ✅ | per-round target across 15 rounds (wiki) |
| Onboarding-to-failure | ✅ **designed** | "At least you got an achievement... — Lose a run." **83.6%** |
| Permanent in-run growth | ✅ | chips, keychains, clip expansion (wiki) |
| Forced-choice regret | ✅ | shop economy on limited tickets (wiki) |
| Collection | ✅ **heavy** | Stickers 15.4% · all Coins 0.7% · all Chips 0.7% · 100% Collection 0.6% |
| Unlock drip across runs | ✅ | cabinets, cards, raccoons, **ticket colours that add run modifiers** (wiki) |
| Big-number spectacle | ✅ core | "coin score over 1,000,000" 28.2%; "score starts with 777" 34.1% |
| Second-chance economy | ✅ distinctive | tickets can be exchanged for coins to avoid failing a round (wiki) |
| Debt / interest | ❌ | corpus `debt_pressure` 0.3% |
| Pity | UNREACHED | not documented in available sources |

**Observed — the corpus.** n=1,000 recent English, **81.6% positive — the lowest of the four**, against an
all-time band of Very Positive (90%+ per the store). 185 negatives in the sample.
`luck_vs_skill` **17.8%** (highest in the set) · `addiction` 17.1% · `run_length` 13.5% (highest) ·
`boredom_quit` 3.3% (highest) · `gambling_real` 5.6%.

Negative themes among the 185: price 6.5% · repetitive/samey 3.8% · bugs 3.8% · grindy 3.2% · thin 2.2%.

> "number go up, big number good, big number fun, much play, much enjoyment" — 17.6h
> "A little more complex to learn than Balatro but just as fun once you understand it." — 8.8h
> *(negative, +130)* "Turning on telemetry with no proper notice and waiting for weeks after release to finally turn it off after getting the data they want with no apology..." — 30.4h
> *(negative, +89)* "It's missing something, it's not that satisfying to play, the big scores feels underwhelming" — 1.6h
> *(negative, +30)* "This game is so shallow and boring. If the demo still exists just get that, the $10 doesn't really get you more of anything fun." — 2.1h

**Inferred — read the sentiment drop carefully.** The single most-upvoted negative is a **telemetry/trust
incident, not a design failure.** It would be wrong to read RACCOIN's 81.6% as evidence of third-wave
fatigue, and `07-business-and-roadmap.md` names third-wave fatigue as a genre risk — so the misreading is
live. **What the corpus does support** is a separate and softer claim: the second and third most-upvoted
negatives are both about *satisfaction thinness* ("not that satisfying", "shallow"), and boredom language
runs at 3.3%, the highest in the set. **Falsifier:** if the drop were purely the telemetry incident,
satisfaction complaints would not out-rank every other design theme. They do.

**Pillar-4 read.** Almost none. 5.6% gambling language despite being a coin-pusher — the arcade framing
launders the gambling entirely. Instructive counter-example: **the same mechanical family reads as gambling
or not depending purely on dressing.**

---

## 3. Session shape

**Observed — the funnel (2026-08-12).**

| Milestone | % |
|---|---|
| **Lose a run** | **83.6** |
| **Win a run** | **42.5** |
| Single coin scoring 1,000,000 | 28.2 |
| Insert more than 10,000 coins | 21.1 |
| Complete all Milestones | 7.5 |
| Win with every character | 5.9 |
| Win with every character on a Golden Ticket | 0.9 |
| Complete the Collection 100% | 0.6 |

**Observed — playtime at review** (n=1,000): p10 1.4h · p25 3.7h · **median 7.6h** · p75 16.5h · p90 32.4h ·
max 194h. Under 2h: **14.1%**. Over 50h: **3.5%** — the lowest tail of the three compulsive titles.

**Inferred.** Lose-first (83.6%) then win-eventually (42.5%) — the same onboarding-through-failure shape as
CloverPit, at a softer difficulty. But the tail is thin: 3.5% over 50h against Balatro's 33.5% and
CloverPit's 10.6%. **RACCOIN converts well and retains worst.** **Falsifier:** the title is 4.5 months old,
so its long tail has had less time to accumulate than Balatro's 2.5 years. That is a real confound and it
is not fully separable from this data — CloverPit at 10.5 months sits between them, which is *consistent*
with age driving the ordering. Treat the retention ranking as **unproven**, and the win-rate figures — which
are age-insensitive once a title is months old — as sound.

**Against SBR.** 15 rounds per run is the longest structure in the set, and it pairs with the highest
boredom language and the shortest tail. Weak evidence, but it points one way: **more rounds is not more
game.** `01-core-loop.md`'s open "round count" question should not default upward.

---

## 4. Meta hooks

**Observed.** Between runs: cabinets, cards, raccoons, and ticket colours that apply run modifiers. Multiple
playable characters (5.9% have won with all of them). Collection is deep and almost nobody finishes it
(0.6–0.7% across three collection achievements).

**Inferred.** The meta is *variety-first with a modifier layer* — ticket colours are the closest thing to an
ascension ladder, and the funnel shows the ladder's top rung at 0.9% ("win with every character using a
Golden Ticket"). The gap between winning once (42.5%) and the ladder top (0.9%) is a factor of 47, with
little visible in between. **Falsifier:** if there were graded rungs, the funnel would show intermediate
percentages; it jumps from 42.5% straight to single digits.

**Against SBR.** Third independent data point for **RF-5**: the reference with the flattest ladder has the
thinnest tail.

## 5. The thing a summary would miss

Playstack — the publisher that made Balatro a phenomenon — looked at the post-Balatro field and backed a
three-person Chinese student studio building a *coin pusher*. Not cards, not slots. The read: the genre's
most successful publisher is betting on **novel physical-machine metaphors**, not on more card games. SBR's
metaphor — a laptop, a TV, a phone, a room — is at least as distinctive as a coin pusher, and this is
mild evidence that distinctiveness of metaphor is what the market's smartest money is buying.

## 6. Transfer to SBR

**STEAL**

| What | Axis | Canon it lands in | Why it survives translation |
|---|---|---|---|
| Celebrate the first loss (83.6%) — second independent instance | 5 | `01-core-loop.md` failure state | Free onboarding; both CloverPit and RACCOIN do it |
| Second-chance economy: spend a meta-currency to avoid failing a round | 3 Capital / 5 | `10-economy-rework.md` A (debt payments), F (COMPS) | **COMPS already exists** and is the natural vehicle |
| Cascade resolution — one commit, many small arriving results | 4 Resolution | `04-the-sweat.md` presentation beats | Our legs already arrive one at a time; the cascade grammar is the same shape |

**CONFLICT** — **RF-5** (ladder depth) and **RF-6** (round count should not default upward) in
`06-mapping-onto-sbr.md`.

**REJECT**

| What | Why it fails for SBR |
|---|---|
| Arcade dressing that launders the gambling | Pillar 4 requires the opposite — SBR is *about* the industry |
| 15 rounds per run | Highest boredom language and thinnest tail in the set |

## 7. Comparison row

`RACCOIN: Coin Pusher Roguelike | 2026-03-31 | $9.59 | 4,495 Very Positive | UNREACHED (seconds, cascade ~1-2s inferred) | low per launch, high in shop | none during resolve | 15 rounds | 7.6h median lifetime | per-round target, curve UNREACHED | miss target = run over, tickets buy a retry | coins × chips, multiplicative | UNREACHED | cabinets/cards/characters/ticket-colour modifiers | 42.5% won a run | funnel + corpus + wiki | MEDIUM`

## 8. Sources

- Steam store + `appdetails` — https://store.steampowered.com/app/3784030/ — 2026-08-12 — settles title, publisher (Playstack), price, reviews.
- Steam global achievement stats — https://steamcommunity.com/stats/3784030/achievements/ — 2026-08-12.
- Steam `appreviews`, n=1,000 recent English — 2026-08-12 — settles language and playtime; **recent-filter is doing real work here**, since the sample straddles a publicised telemetry incident. All-time sentiment is higher.
- https://en.wikipedia.org/wiki/Raccoin:_Coin_Pusher_Roguelike — 2026-08-12 — settles run structure (15 rounds), shop, keychains, between-run unlocks, studio and funding. Settles nothing about feel.
- **Instrument null (`C37`):** near-miss lexicon, 1 hit in 4,000 across the set — the single hit was in this corpus. Instrument failure, not absence.
- **Confound recorded, not resolved:** title age (4.5 months) is not separable from the retention-tail comparison in §3.
