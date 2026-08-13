# 10 — Probe: Dungeons & Degenerate Gamblers — *how does a real gambling game's vocabulary become a roguelite without a tutorial wall?*

**Probed:** 2026-08-12 · research lane (Claude, Opus 5) · Tier-2, one question · **Status:** DRAFT → Allen

## 0. Identity and instrument

| | |
|---|---|
| Full title | Dungeons & Degenerate Gamblers |
| Dev / Pub | Purple Moss Collectors / Yogscast Games |
| Released / Price / Reviews | 2024-08-08 · $7.49 (from $14.99) · 3,500 · Very Positive |
| Store | https://store.steampowered.com/app/2400510/ |

**Evidence basis:** achievement funnel (39 rows) · review corpus (n=1,000 recent English) · store copy.
Pulled 2026-08-12. Not played. **Confidence ceiling: MEDIUM.**

Relevance: `00-vision` pillar 2 — *"Jargon is the mastery layer, not the entry fee... Arbitrage/+EV/hedging
are discovered through items, never taught in a tutorial wall."* D&DG converts actual blackjack. No title in
the Tier-1 set solves this problem: Balatro's poker is cosmetic and CloverPit's slots need no vocabulary.

---

## 1. The one question — how is the vocabulary taught?

**Observed — confusion is not a complaint.** `onboard` language (tutorial / learn / confusing / didn't know /
explain / rules) appears in **8.0%** of 1,000 reviews, and in only 15 of the 161 negatives. For a game that
requires the player to hold real blackjack rules *and* a deckbuilder's rule-breaking on top, that is low.
`blackjack` language runs 37.5% — players discuss the vocabulary constantly and complain about it rarely.

**Observed — three mechanisms carry it.**

1. **A named-opponent chain where each opponent is both a rung and a lesson.** The funnel is a clean
   descending sequence of people, not levels: Manager **88.5%** → Bouncer 58.3 → Alucard 48.6 →
   Celebutante 44.7 → Pit Boss 39.9 → CEO 34.2 → **win a run 32.2** → Dracon 29.2 → Deity of Hope 25.3 →
   Deity of Despair 23.3. The first opponent is beaten by 88.5% of owners: the tutorial is a *person you
   beat*, and beating them is the achievement.
2. **The beginner's mistake is rewarded, not punished.** `"Clumsy — Get a blackjack then hit and bust"` is
   held by **40.9%** of owners. The single most classic blackjack error has an achievement attached. Two
   Tier-1 titles already celebrate the first loss (CloverPit 97.0%, RACCOIN 83.6%); D&DG goes further and
   celebrates *the specific error that teaches the specific rule.*
3. **The rules are allowed to be absurd, which licenses not knowing them.** The most-upvoted review in the
   corpus (+105) is the whole finding in one line:

   > "the best way to describe it is thats its like the yugioh anime where **everyone is basically making up
   > the rules as they go**." — 5.1h

   If the rules are visibly improvised, a player who does not know them is not behind — they are on time.

**The caveat that limits the transfer, and it is a large one.** D&DG starts from a vocabulary essentially
every player already owns. Blackjack needs no tutorial because the culture is the tutorial. **SBR's
vocabulary has no such prior**: arbitrage, +EV, hedging, line shopping and getting limited are not
common knowledge, and `00-vision` names them as the mastery layer. So D&DG is evidence that *mechanism*
works, not that the problem is the same size. **Pillar 2 is doing considerably more work for SBR than it
did for D&DG**, and this probe cannot tell us how much more. Raised as **RF-13**.

**Observed — the rest of the corpus.** 83.9% positive · median playtime **7.0h** · 13.9% under two hours ·
7.2% over fifty. `luck_vs_skill` 27.2% (84 of 161 negatives — **52% of all negatives**, the dominant theme,
same shape as Luck be a Landlord's). `depth_thin` 19.3%.

> *(negative, +16)* "all of the addictiveness of balatro with none of the coherent deckbuilding satisfaction"
> *(negative, +9)* "**Runs take way too long** with BS rng that can keep you trapped fighting one guy for 30 mins, not rewarding or fun."
> *(positive, +36)* "As much RNG and overall annoying chance exists in this game, you always know that there too is a chance for you to win and that prospect will keep you crawling back for more."

**Falsifier.** If the vocabulary were a barrier, `onboard` language would cluster in the negatives. It sits
at 15 of 161 while luck/skill sits at 84 of 161. The barrier is agency over variance, not comprehension.

**Second appearance of the pacing complaint.** "Runs take way too long... trapped fighting one guy for 30
mins" is the second most-upvoted negative here, and Gambonanza's top negative was the same class of
complaint at 24 minutes played. Feeds **RF-10**.

## 6. Transfer to SBR

**STEAL**

| What | Axis | Canon it lands in | Why it survives translation |
|---|---|---|---|
| **A named-opponent chain as the tutorial** — each early bookie/guru is a person you beat, and beating them teaches one concept | 1 Information / 5 | `00-vision` pillar 2; `01-core-loop.md` round structure; guru roster | SBR already has a cast (gurus, insiders, the bookie). The rungs are free |
| **Give the classic beginner error an achievement** — e.g. taking a -EV favourite parlay, or cashing out a ticket that then wins | 5 | `01-core-loop.md` failure state; extends **S1** | Costs one achievement and one line of voice |
| **Let the rules read as improvised** so not knowing them is not being behind | voice | `surething-design.md` §6; `00-vision` pillar 4 | Fits the satire exactly — a crooked book *should* look like it makes rules up |

**CONFLICT** — **RF-13** (pillar 2's load is larger than its reference case) and **RF-10** (pacing), both in
`12-probe-set-findings.md`.

**REJECT**

| What | Why |
|---|---|
| Assuming the vocabulary comes free | The caveat above. Blackjack is cultural; +EV is not |

## 8. Sources

Steam `appdetails` + `appreviews` (n=1,000 recent English) + `steamcommunity.com/stats/2400510/achievements/`
— 2026-08-12. The funnel settles the opponent chain and the "Clumsy" rate. The corpus settles that confusion
is not a complaint; it cannot settle whether players *learned* blackjack or merely already knew it — which
is precisely the caveat, and it is unresolvable from public data.
