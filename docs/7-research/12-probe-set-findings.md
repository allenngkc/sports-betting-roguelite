# 12 — Tier-2 probe set: findings, corrections, and the eight-title table

**Lane:** research · **Date:** 2026-08-12 · **Status:** PROPOSAL — routed to Allen
**Built from:** `08`–`11` (Gambonanza, Luck be a Landlord, Dungeons & Degenerate Gamblers, Scritchy Scratchy)
**Amends:** `06-mapping-onto-sbr.md` RF-5.

---

## 1. Headline — the genre's dominant complaint is not difficulty, price, or content

It is **agency over variance**. Across the probe set, luck-and-skill language is the top negative theme in
every title where it was measured, by a wide margin:

| Title | Luck/skill language as a share of that title's negative reviews |
|---|---|
| Dungeons & Degenerate Gamblers | **52%** (84 of 161) |
| Luck be a Landlord | **44%** (47 of 107) |
| Gambonanza | 40% (71 of 176) |
| RACCOIN (Tier 1) | overall 17.8% — highest of the Tier-1 four |
| CloverPit (Tier 1) | the two most-upvoted negatives are both this complaint |

The recurring sentence is not "this is too hard." It is Luck be a Landlord's most-upvoted negative:
*"there's no way I can see to increase your odds of getting specific symbols."*

**This lands directly on SBR.** `design/11` ships a **dealt-hand shop** — items are offered, not chosen —
and `10-economy-rework.md` §E holds the **bounded-p doctrine**, which correctly forbids buying probability.
Together they mean the player's steering wheel is narrow by construction. The genre's own review corpora say
that is the thing players resent most. See **RF-12**.

---

## 2. Correction — RF-5's strong form is withdrawn (§1.5, mine)

`06-mapping-onto-sbr.md` RF-5 argued **"ladder depth tracks retention"** from four points: 0 rungs → 4.0h,
1 rung → 9.1h, flat layer → 7.6h, 8 stakes → 25.1h.

**Luck be a Landlord is a fifth point and it breaks the proxy: a twenty-rung ladder, median lifetime 10.4h.**
Both ladders end at the same place — Balatro's Gold Stake 12.1%, LbaL's Floor 20 12.4% — but Balatro gets
there in three rung-steps and LbaL takes nineteen, and Balatro retains 2.4× as long. **Rung count is not the
variable.** I withdraw the strong claim.

**What survives, stated at its real strength.** Every title in the study with *no* ladder at all sits at
4.0–7.6h median (Buckshot 4.0, D&DG 7.0, RACCOIN 7.6, Gambonanza 3.3). Every title with one sits at
9.1–25.1h. That supports **having** a ladder. It does not support any particular depth, and I no longer
claim it does. **RF-5's recommendation — pull two or three rungs into v1 — stands on the weaker evidence.**

One shape observation, offered as a hypothesis rather than a finding: LbaL's largest drop is its *first*
rung (−10.0 points, floors 1→2) and the gradient decelerates to −0.7 by floor 20. Balatro's rungs roughly
halve each time. If SBR ships rungs, front-loading the first one is the cheaper design.

---

## 3. The eight-title table

| | Balatro | CloverPit | RACCOIN | Buckshot | LbaL | D&DG | Scritchy | Gambonanza |
|---|---|---|---|---|---|---|---|---|
| Released | 2024-02 | 2025-09 | 2026-03 | 2024-04 | 2023-01 | 2024-08 | 2026-03 | 2026-05 |
| Price now | $11.99 | $5.99 | $9.59 | $2.99 | $9.99 | $7.49 | $5.59 | **$14.99** |
| Reviews (all lang) | 196,738 | 25,496 | 4,495 | 123,710 | 11,517 | 3,500 | 14,629 | 1,601 |
| **Ever won a run** | **71.7%** | 30.9% | 42.5% | 62.8% | 59.1% (fl.1) | 32.2% | — | **28.4%** |
| Median playtime | **25.1h** | 9.1h | 7.6h | 4.0h | 10.4h | 7.0h | 11.7h | 3.3h |
| Over 50h | **33.5%** | 10.6% | 3.5% | 0.4% | 10.6% | 7.2% | 3.6% | 0.8% |
| Under 2h | 3.3% | 8.3% | 14.1% | 21.5% | 9.7% | 13.9% | **2.5%** | **34.4%** |
| Positive (recent) | **97.2%** | 89.7% | 81.6% | 92.2% | 89.3% | 83.9% | 96.9% | **75.1%** |
| Achievements | 31 | 30 | 23 | 16 | **186** | 39 | 34 | 53 |
| **Rarity floor** | 0.8% | 1.1% | 0.6% | 1.0% | 0.2% | 0.2% | **12.1%** | 0.1% |
| `addiction` language | 18.8% | 15.1% | 17.1% | **2.6%** | 16.8% | 8.1% | **19.5%** | 6.1% |

All figures pulled 2026-08-12. Sources in each autopsy/probe §8.

### Instrument audit — read before comparing cells (`C44`)

**The lexicon drifted between the Tier-1 and Tier-2 pulls, and I did not freeze it. That is my error.**
Three consequences, disclosed rather than smoothed:

1. **`gambling_real` is contaminated in two cells and must not be compared across the table.** The Tier-2
   pattern added `scratch`, which is *Scritchy Scratchy's core verb* — its 26.0% is mechanically inflated.
   Both tiers include `degenerate`, which appears in *Dungeons & Degenerate Gamblers'* own title — its 17.9%
   is inflated the same way. **Neither number is usable.** The Tier-1 cells (CloverPit 32.4%, Balatro 8.3%,
   RACCOIN 5.6%, Buckshot 5.1%) remain mutually comparable.
2. **`boredom_quit` differs between tiers** (Tier-2 added `samey`/`monoton`, dropped `burn out`). Cross-tier
   comparison of that family is approximate.
3. **`addiction` drifted trivially** (Tier-1 also matched `crack`, `cannot stop`). The row above is usable;
   treat differences under ~2 points as noise.

**Fix for any future probe: freeze the lexicon in `01-autopsy-template.md` and re-run Tier 1 against it
before adding a ninth title.** Not done in this pass — flagged, not hidden.

### New instrument proposed — the rarity floor

The rarest achievement a title has. It measures how much of a game is reachable by almost everybody, and it
separates *engagement* from *completion space* in one HTTP request. Scritchy Scratchy's floor is **12.1%**
against 0.1–1.5% for every other title in the study: nothing in it is hidden from the median player, and its
tail is 3.6% over fifty hours despite an 11.7h median. **A high median with a low tail is the signature of a
game everyone finishes.** Proposed as a standing field in the template's §3 funnel block.

---

## 4. New proposals

### RF-10 — Resolution speed needs a named control, not an implied permission

**Canon.** `00-vision` pillar 1: *"Nothing may make resolution instant or skippable **by default**."*

**Evidence.** Two of the four probes have a pacing complaint at the very top of their negatives, from
players who had barely started: Gambonanza's most-upvoted negative is *"It's way too slow, needs an option to
speed up the animations and gameplay"* at **0.4 hours played**; D&DG's second is *"Runs take way too long...
trapped fighting one guy for 30 mins."* Neither game has a resolution as long as SBR's.

**The pillar's own wording already permits a control** — "by default" concedes it. The evidence says the
control is not a nicety: it is the difference between a slow resolution reading as *tension* and reading as
*waiting*, and the judgement is made inside the first half hour.

**Ruling requested.** Promote it from implied permission to named requirement: a speed control that exists,
is discoverable in the first session, and carries no penalty — while the default stays slow, as the pillar
demands. Cheap, and it protects the pillar rather than weakening it.

### RF-11 — The price band's premise, not its number

**Canon.** `07-business-and-roadmap.md`: *"$8–13 launch price band (genre-proven)."*

**Evidence.** Scritchy Scratchy lists at **$6.99**, below the band, and posts the study's **highest**
compulsion language (19.5%), **lowest** refund-window share (2.5%), second-highest positive rate (96.9%), and
14,629 reviews in five months. Buckshot Roulette at $2.99 holds 123,710. Gambonanza, the study's **most
expensive** title at $14.99, has its lowest positive rate (75.1%), its highest under-2h share (34.4%), and a
top-upvoted review telling buyers to purchase the $6 mobile version instead.

**What this does not show: revenue.** $6.99 needs roughly twice the units of $13 for the same net, and
`00-vision`'s target is $15K net. **The claim is that "genre-proven" is not established for the band's floor
— not that the band's number is wrong.**

**Ruling requested.** Re-word the band as a decision rather than a proven fact, or commission the revenue
half properly. This lane deliberately did not use third-party sales estimators, because their method is not
re-derivable (`C34`).

### RF-12 — Give the player a bounded way to steer toward a build

**Canon.** `design/11` — the dealt-hand shop. `10-economy-rework.md` §E — bounded-p doctrine.

**Evidence.** §1 above. Steering, not difficulty, is the genre's dominant complaint — top negative theme in
every probe where it was measured, at 40–52% of negatives.

**This is not an argument against bounded-p.** Bounded-p forbids buying probability of *match outcomes*. It
says nothing about the *shop's* distribution. A reroll, a banked pick, a "wanted list" that biases future
offers, a guru who tells you what the shop will stock — the last is native to SBR's information axis and
exists nowhere else in the genre.

**Ruling requested.** Is bounded steering of the item stream in scope? It is also the cheapest available
answer to **RF-6** (upside-variance rescue), and the two could be one mechanism.

### RF-13 — Pillar 2 is carrying more weight than its reference case

**Canon.** `00-vision` pillar 2: *"Jargon is the mastery layer, not the entry fee... discovered through
items, never taught in a tutorial wall."*

**Evidence.** D&DG converts real blackjack and confusion is *not* a complaint — onboarding language sits at
8.0%, and just 15 of 161 negatives. Three mechanisms carry it (see `10-probe...`): a named-opponent chain
where each opponent is a rung and a lesson, an achievement for the classic beginner error (40.9% hold
"Clumsy"), and rules that visibly improvise so not knowing them is not being behind.

**The caveat is the finding.** Blackjack needs no tutorial because the culture already taught it. SBR's
vocabulary — arbitrage, +EV, hedging, line shopping, getting limited — has no cultural prior at all. D&DG is
evidence the *mechanism* works; it is not evidence that the problem is the same size, and this lane cannot
size the gap from public data.

**Ruling requested.** None urgent. Recorded so pillar 2 is not treated as solved-by-analogy. If it is ever
tested, the cheap version is the named-opponent chain, which SBR can build from a cast it already has.

---

## 5. What the probes changed

| | Before | After |
|---|---|---|
| RF-5 | "ladder depth tracks retention", 4 points | strong form **withdrawn**; recommendation stands on weaker evidence |
| Gambonanza's role | "the genre's failure case" | **wrong genre** — it is a chess roguelike; corrected in `08` |
| Price band | untested | premise challenged (**RF-11**) |
| The genre's main flaw | unidentified | **steering, not difficulty** (**RF-12**) |
| Instruments | funnel + corpus | **+ rarity floor**; and a lexicon-drift defect found in my own method |

## 6. Need Allen

Nothing here is urgent against the six proposals already with you. In priority order when you get to them:

1. **RF-12** — bounded build steering. Strongest cross-title evidence in the whole study, and it merges with RF-6.
2. **RF-10** — the speed control. Cheap, and it protects pillar 1 rather than weakening it.
3. **RF-11** — re-word the price band, or commission the revenue half.
4. **RF-13** — no action; recorded so pillar 2 is not assumed solved.
5. Note the **RF-5 correction** when you rule on the ladder.

Still parked with you and unaffected by this pass: the mapping rulings RF-4/5/6/7/8/9, your play answers,
and the literature scope (RF-2).
