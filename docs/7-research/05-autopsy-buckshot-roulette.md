# 05 — Fun autopsy: Buckshot Roulette

**Autopsied:** 2026-08-12 · **By:** research lane (Claude, Opus 5) · **Status:** DRAFT → routed to Allen
**Added to Tier 1 by RF-1** (approved by Allen, 2026-08-12) as the reference for resolution tension —
the thing the three named titles do not have.

## 0. Identity and instrument

| | |
|---|---|
| **Full title** | Buckshot Roulette |
| Developer | Mike Klubnika |
| Released / Price / Reviews | 2024-04-04 · $2.99 · 123,710 · Overwhelmingly Positive |
| Store | https://store.steampowered.com/app/2835570/ · https://en.wikipedia.org/wiki/Buckshot_Roulette |

**Evidence basis:** achievement funnel (16 rows, 2026-08-12) · review corpus (n=1,000 recent English,
2026-08-12) · Wikipedia gameplay. **Not played by anyone on this project.**

**Confidence ceiling: MEDIUM.** No play. But this is the one title in the set whose funnel measures
*decisions under tension* directly, which is exactly the question it was added to answer — so the
instrument is unusually well matched to the target.

**Prior canon:** none. New reference.

---

## 1. Result cadence — the reason this title is in the set

**Observed (structure, Wikipedia).** Three rounds against a computer Dealer. At the start of each round the
Dealer loads a shotgun with a stated number of **red live shells and blue-grey blanks in random order** —
the counts are known, the order is not. On your turn you choose to shoot **the Dealer or yourself**. If the
shell is live, whoever was shot loses a life and the gun passes to the Dealer. **If you shoot yourself with a
blank, you keep the gun and go again.** Items modify information and outcomes. After the Dealer is beaten,
an endless **"Double or Nothing"** mode unlocks, with a running purse the player may **cash out** or risk.

**Observed — this is the only reference in the set where the player acts *inside* the tension**, and the
funnel proves players take that action:

| Behaviour | % of owners |
|---|---|
| **"Coin Flip" — successfully shoot yourself with a blank at 50/50 odds** | **48.8** |
| "Chasing Losses" — consume the Double or Nothing pills (opt into the endless/cash-out mode) | 39.3 |
| "Nope!" — cash out immediately in Double or Nothing | 12.7 |
| "140K" — double your earnings in Double or Nothing and win | 12.5 |
| "Overdose" — start Double or Nothing 10 times | 11.2 |
| "1000K" — cash out over 1,000,000 | 1.4 |
| "Know When To Quit" — lose more than 1,000K in Double or Nothing | 1.0 |

**Inferred — and this is the transferable law.** Half of all owners deliberately pointed the gun at
themselves on a coin flip. They did it because **the tense option is mechanically correct**: a self-shot on
a blank keeps the turn, so when blanks outnumber your remaining lives, self-shooting is the +EV play. The
maximum-drama action and the maximum-EV action are the *same action* by construction.

**Falsifier:** if self-shooting were merely flavour, its rate would sit near the rate of other optional
flourishes. The nearest optional-flavour achievements in this funnel run 5.7–9.6%. Self-shooting runs 48.8%
— roughly 5–8× higher. The reading survives.

**Against SBR — the direct application.** SBR's signature moment is a live cash-out during multi-leg
resolution. **If cashing out is always the correct play, the sweat is a formality and nobody will ever ride
it.** Buckshot's design answer is to make riding the risk *sometimes correct as arithmetic, not as bravado*.
Raised as **RF-7** in `06-mapping-onto-sbr.md`. It is the single most actionable finding this lane has
produced for `04-the-sweat.md`.

Note also the shape of the information: **counts known, order unknown.** That is a cheap, legible way to make
a resolution tense without hiding the math — and it is very close to SBR's own four-number model, where `p`
is known and the outcome is not.

**UNREACHED — exact timings.** Seconds per shell not measured. Unlike the other three, the structure here
tells us the useful part anyway: the tension is authored by *turn order and information*, not by animation
length.

---

## 2. Compulsion levers

**Observed — inventory.**

| Lever | Present | Evidence |
|---|---|---|
| Dread / anticipation | ✅ **core** | corpus `dread_tension` **2.2%** — highest of the four |
| Cash-out / ride-or-bank decision | ✅ **core, and unique in the set** | funnel rows above |
| Sunk-cost chasing | ✅ **named by the developer** | the D-o-N opt-in achievement is literally called **"Chasing Losses"** |
| Known-odds, unknown-order information | ✅ distinctive | wiki |
| Escalating target | ➖ | D-o-N purse doubles, but there is no requirement curve |
| Permanent in-run growth | ➖ weak | items only, per round |
| Debt | ❌ | corpus `debt_pressure` **0.0%** |
| Collection / unlock drip | ❌ | no between-run unlock economy |
| Variable-ratio reward | ✅ | item draws |

**Observed — the corpus.** n=1,000 recent English, 92.2% positive. **`addiction` 2.6% — one sixth of the
other three titles (15.1 / 18.8 / 17.1).** `dread_tension` 2.2% — the highest. `cash_out` 1.2% — the only
non-zero in the set. `run_length` 1.6% — the lowest.

> "A simple but incredibly intense game. Every round is full of tension, and the atmosphere is amazing." — 0.3h
> "got game because i can shoot myself without consequences and it was fantastic." — 7.9h
> *(negative)* photosensitivity complaints appear repeatedly — a real accessibility defect, and a cheap lesson for a game whose own juice budget is aimed at a high-arousal moment.

**Inferred — the finding that complicates SBR's pillar 1.** Buckshot has the highest tension language and
the lowest compulsion language in the set, and its median lifetime is 4.0 hours against Balatro's 25.1.
**Tension is not retention.** They are produced by different systems: tension by the resolution, retention
by the item/economy/ladder layer. **Falsifier:** if tension retained, Buckshot's >50h tail would not be
0.4% against Balatro's 33.5%. It is the largest gap in any measure across this study.

**Pillar-4 read.** Buckshot is the cleanest satire in the set and it does it with almost no text: it names
its compulsion mechanic **"Chasing Losses"** and its repeat-engagement achievement **"Overdose"**, and it
lets 39.3% of owners walk into both. The technique — *name the lever honestly in the UI and let the player
notice what they are doing* — is directly available to SBR and costs nothing.

---

## 3. Session shape

**Observed — the funnel.** Beat the game: **62.8%**. Then a sharp fall into the optional endless mode:
39.3% opt in, 12.5% double once and win, 11.2% return ten times, 1.4% bank over a million, 1.0% lose more
than 1,000K.

**Observed — playtime at review** (n=1,000): p10 0.8h · p25 2.4h · **median 4.0h** · p75 6.6h · p90 12.4h ·
max 645h. Under 2h: **21.5%** (highest in the set — it is a short game, not a churn signal). Over 50h: 0.4%.

**Inferred.** A complete, intense, ~4-hour experience with a thin optional tail. The 62.8% completion is
high because the game is short, not because it is easy. **Falsifier:** if difficulty drove the drop-off,
"beat the game" would sit well below 62.8% and the negative corpus would carry difficulty complaints; it
carries technical and photosensitivity complaints instead.

**Against SBR.** Buckshot is what SBR looks like if pillar 1 wins and the economy layer is thin: superb
moment, four hours, no ladder. `00-vision`'s commercial target is $15K net and 500 reviews — Buckshot cleared
that many times over at $2.99, so this is not a failure model. But it is not the model canon is aiming at
either, and the difference between the two is *entirely* the retention layer.

## 4. Meta hooks

**Observed.** Almost none by design. Double or Nothing is the only progression surface, and it is a score
chase rather than an unlock economy. No between-run power, no collection, no difficulty ladder.

**Inferred.** The thinnest meta in the set, and the shortest median lifetime. **Fourth independent data
point for RF-5**, and the cleanest one: zero rungs, 4.0h; one rung (CloverPit), 9.1h; a flat modifier layer
(RACCOIN), 7.6h; eight rungs (Balatro), 25.1h.

Recorded honestly: this is four observations with obvious confounds (price, length, genre, age, scope). It is
suggestive, not decisive — but all four point the same direction, and none points the other way.

## 5. The thing a summary would miss

The game is $2.99 and has 123,710 reviews. It is the highest review count per dollar and per development-hour
in this entire study, and it achieved that with **one mechanic, one room, and no progression.** For a solo
developer on a zero-cash budget — `00-vision`'s stated constraint — Buckshot is the most relevant business
case in the set, and it is the one canon never listed.

## 6. Transfer to SBR

**STEAL**

| What | Axis | Canon it lands in | Why it survives translation |
|---|---|---|---|
| **Make riding the risk sometimes +EV, not just brave** | 4 Resolution | `04-the-sweat.md`, `02-betting-math.md` | This is the cash-out design problem stated exactly; see **RF-7** |
| Known counts, unknown order | 1 Information / 4 | `04-the-sweat.md`, `03-mechanics-catalog.md` axis 1 | Our `p` is already known and the outcome unknown — same shape, already native |
| Name the compulsion lever honestly in the UI ("Chasing Losses") | 5 | `00-vision` pillar 4; `surething-design.md` §6 Voice | Free, and it *is* the satire |
| Tension from turn order and information, not from animation length | 4 | `04-the-sweat.md` pacing dials | Cheap for a solo dev; no juice budget required |

**CONFLICT** — **RF-7** (the cash-out must sometimes be wrong to take) and **RF-8** (tension ≠ retention;
pillar 1's "all juice budget flows here first" needs a second clause) in `06-mapping-onto-sbr.md`.

**REJECT**

| What | Why it fails for SBR |
|---|---|
| No meta layer | Four data points in this study say that caps the game at ~4 hours |
| High-frequency flashing as a tension device | Repeated photosensitivity complaints in the corpus — a defect, not a technique |

## 7. Comparison row

`Buckshot Roulette | 2024-04-04 | $2.99 | 123,710 Overwhelmingly Positive | UNREACHED (seconds); tension authored by turn order, not animation | 1 high-stakes decision per shell | FULL agency during resolution — the only one in the set | 3 rounds + endless | 4.0h median lifetime | none (purse doubles in D-o-N) | lose all lives = over | items only, no compounding economy | not observed | none | 62.8% beat the game | funnel + corpus + wiki | MEDIUM`

## 8. Sources

- Steam store + `appdetails` — https://store.steampowered.com/app/2835570/ — 2026-08-12.
- Steam global achievement stats — https://steamcommunity.com/stats/2835570/achievements/ — 2026-08-12 — **the load-bearing instrument for this autopsy**; it measures player decisions under tension directly, which is rare. Denominator is achievement-tracked owners. Six of the sixteen achievements have hidden descriptions and are not interpreted here.
- Steam `appreviews`, n=1,000 recent English — 2026-08-12 — settles reception language and playtime; a 2024 title's recent reviews skew to sale cohorts and to the multiplayer update.
- https://en.wikipedia.org/wiki/Buckshot_Roulette — 2026-08-12 — settles round structure, the blank/live rule, the self-shot turn-retention rule, and Double or Nothing. **The +EV reading of the self-shot in §1 is my inference from that rule, not a wiki claim.**
- **Instrument null (`C37`):** near-miss lexicon returned 0 here. Instrument failure, not absence.
