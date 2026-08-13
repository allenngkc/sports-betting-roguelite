# 06 — Mapping the references onto SBR

**Lane:** research · **Lead:** Claude (Opus 5) · **Date:** 2026-08-12
**Status:** PROPOSAL — routed to Allen (this lane's design authority). Nothing here edits canon.
**Built from:** `02-autopsy-balatro.md`, `03-autopsy-cloverpit.md`, `04-autopsy-raccoin.md`,
`05-autopsy-buckshot-roulette.md`. All figures pulled 2026-08-12; sources in each autopsy §8.

---

## 1. The cross-game table

Assembled from each autopsy's §7 row. No re-analysis.

| | Balatro | CloverPit | RACCOIN | Buckshot Roulette |
|---|---|---|---|---|
| Released / price | 2024-02-20 · $11.99 | 2025-09-26 · $5.99 | 2026-03-31 · $9.59 | 2024-04-04 · $2.99 |
| Reviews | 196,738 | 25,496 | 4,495 | 123,710 |
| Rounds per run | 8 antes × 3 blinds | 9 deadlines × 3 rounds | 15 rounds | 3 rounds + endless |
| **Ever won a run** (% of owners) | **71.7** | **30.9** | **42.5** | **62.8** |
| Median lifetime (playtime at review) | **25.1h** | 9.1h | 7.6h | 4.0h |
| Over 50h | 33.5% | 10.6% | 3.5% | 0.4% |
| Ladder rungs above the base win | **8 stakes** | 1 (Ascension) | flat modifier layer | 0 |
| Agency during resolution | none | none | none | **full** |
| Escalation curve | rises per ante | ×3.3 per deadline, fixed + public | per-round target | none |
| Upside-variance rescue | Legendary Jokers, shop luck | **Luck pity + rubber-band** | tickets → coins retry | items |
| `addiction` language | 18.8% | 15.1% | 17.1% | **2.6%** |
| `dread_tension` language | 0.4% | 1.2% | 1.4% | **2.2%** |
| `gambling_real` language | 8.3% | **32.4%** | 5.6% | 5.1% |
| Positive rate (recent 1,000) | **97.2%** | 89.7% | 81.6% | 92.2% |

**Two patterns run across every column.**

**Pattern A — ladder depth tracks retention.** 0 rungs → 4.0h · 1 rung → 9.1h · flat layer → 7.6h ·
8 rungs → 25.1h. Four observations, same direction, none opposed. Confounded by price, length, genre and
title age (recorded, not resolved) — suggestive, not decisive.

**Pattern B — tension and compulsion are produced by different systems.** Buckshot has the highest tension
language and the lowest compulsion language in the set, and the shortest tail by an order of magnitude. The
three retentive titles buy retention from item economies and ladders, none of which touches their resolution.

---

## 2. STEAL — no ruling needed, cheap, consistent with canon

| # | What | From | Lands in | Cost |
|---|---|---|---|---|
| S1 | **Celebrate the first loss.** Give the first bust an achievement and a line of voice. | CloverPit 97.0% "Aw Dangit!"; RACCOIN 83.6% "At least you got an achievement..." | `01-core-loop.md` failure state; `surething-design.md` §6 | trivial |
| S2 | **Second-chance economy** — spend a meta-currency to survive a missed payment instead of dying. | RACCOIN (tickets → coins) | `10-economy-rework.md` A + F — **COMPS already exists and is the natural vehicle** | small, sim-auditable |
| S3 | **Author tension between results, in the room and on the phone**, not only inside the sweat. | CloverPit: 56.0% reached the Death Countdown; tension lives in the room, not the reels | `room-design.md`, `phone-design.md`, `04-the-sweat.md` | already-built surfaces |
| S4 | **Known counts, unknown order.** State the odds fully; hide only the sequence. | Buckshot | `04-the-sweat.md`, `02-betting-math.md` | native — our `p` is already known |
| S5 | **Name the compulsion lever honestly in the UI.** Buckshot calls its sunk-cost mode "Chasing Losses" and its repeat achievement "Overdose". | Buckshot | `00-vision` pillar 4; voice sections | free, and it *is* the satire |
| S6 | **Make the requirement curve public.** CloverPit tells you linear play is dead by deadline 4. | CloverPit | `10-economy-rework.md` payment curve | presentation only |

---

## 3. CONFLICT — proposals against canon, for Allen's ruling

### RF-4 — The win-rate band and run length are one decision, not two

**Canon, quoted.** `10-economy-rework.md` §F / sim gate G3: *"skilled + items wins: median death ≥5,
**win 5–8%** (re-banded by Allen 2026-07-15)"*. `sim-report-5.md` reports **7.6%**.
`01-core-loop.md`, still open: *"Round count and session length target (Balatro run ≈ 30–60 min; right for
us?)"*.

**What the research argues.** Per-run win rate and cumulative owner win rate are different quantities, and
converting between them is where the decision lives. At 7.6% per run, the expected number of skilled runs
to a first win is **13.2**, and cumulative win probability reaches each reference's observed rate at:

| Reference (% of owners who ever won) | Skilled SBR runs needed at 7.6%/run |
|---|---|
| CloverPit 30.9% | ~5 |
| RACCOIN 42.5% | ~7 |
| Buckshot 62.8% | ~13 |
| Balatro 71.7% | ~16 |

**So the band is defensible — conditionally.** It is defensible if 5–16 runs fit comfortably inside a median
player's lifetime with the game. Multiply through by candidate run lengths, against the measured median
lifetimes of the two closest comps (CloverPit 9.1h, RACCOIN 7.6h):

| Run length | 13.2 skilled runs | Fits inside a 7.6–9.1h median lifetime? |
|---|---|---|
| 20 min | 4.4h | yes, comfortably |
| 30 min | 6.6h | yes, tightly |
| 45 min | 9.9h | **no — exceeds both comps' entire median lifetime** |
| 60 min | 13.2h | **no — by a wide margin** |

And that is *before* the unskilled ramp: G1 (naive) and G2 (skilled, no shop) both measure **0.0%**, so the
runs before a player learns the item engine contribute nothing to the count.

**Ruling requested.** Not "lower the win rate." Rather: **rule the run length and the G3 band together, as
one decision.** If 5–8% stands, a run should target **≤30 minutes**, which is at the short end of the
"Balatro ≈ 30–60 min" straw man and should be stated as a constraint rather than left open. If runs land at
45–60 minutes, the band needs re-banding.

**Falsifier.** If SBR's real median lifetime lands near Balatro's 25.1h rather than the closer comps' 7.6–9.1h,
the 45-minute row stops being a problem. Nothing in the current evidence predicts which of those SBR gets.

---

### RF-5 — The ascension ladder is the retention engine, not a post-v1 nicety

**Canon, quoted.** `01-core-loop.md`: *"Unlocks (new relics, bet types, leagues, guru roster) rather than
power creep — Balatro model. **Ascension-style difficulty tiers post-v1.**"*

**What the research argues.** The borrowing keeps the Balatro model's name and drops the half that retains.
Balatro's base game is beaten by **71.7%** of owners; attrition lives entirely in the eight-stake ladder
(71.7 → 43.7 → 30.2 → 12.1), and Balatro's median lifetime is **25.1h**. The three references with a
shallower or absent ladder sit at 9.1h, 7.6h and 4.0h. Pattern A in §1: four points, one direction.

Note the sharper form of this: SBR's ruled 5–8% band is **close to Balatro's Gold Stake (12.1%) and
CloverPit's Ascension rung (9.1%)** — that is, SBR currently proposes to ship its *default* difficulty at
roughly where the references put their *optional top rung*, with no rungs below it.

**Ruling requested.** Move a difficulty ladder from post-v1 into v1 scope — even two or three rungs — and
let the base game sit materially below the ruled band. This is a scope claim as much as a design one, and
scope is Allen's.

**Falsifier.** If the ladder were cosmetic, stake-win rates would not stratify cleanly by rung. They halve
almost exactly, four times in a row.

---

### RF-6 — Every reference has an upside-variance rescue. SBR has none.

**Canon, quoted.** `10-economy-rework.md` §E, bounded-p doctrine (reaffirmed 2026-07-12); `design/11`:
*"luck → FORBIDDEN as passive (bounded-p) — timed consumables only"*. `design/09` records CloverPit's Luck
as outcome-forcing with a deterministic pity schedule and 4-dead-spin rubber-banding.

**What the research argues.** Every reference in the set can rescue a losing run through good variance:
Balatro through a Legendary Joker or a shop that offers the missing piece (57.5% of owners have found a
Legendary), CloverPit through an explicit pity schedule that *guarantees* mercy after four dead spins,
RACCOIN through spending tickets to retry a failed target. SBR's gates measure naive and no-shop-skilled
play at **0.0%** — there is no lucky-win path at all.

**This is not an argument against bounded-p.** Bounded-p forbids manipulating `p`, and correctly so —
CloverPit's own negative corpus is full of players who concluded the percentages were "a smokescreen".
A rescue can live on other axes: a payout ratchet that arms on a losing streak, a rebate that grows while
you are behind, or COMPS buying a partial payment (which is also **S2**).

**Ruling requested.** Is an explicit, bounded, *non-p* upside-variance rescue in scope for the economy?
It is the one lever shared by all four references that SBR has deliberately excluded, and the exclusion
looks like a side effect of the bounded-p ruling rather than a decision anyone made.

---

### RF-7 — The cash-out must sometimes be the wrong play

**Canon, quoted.** `00-vision` pillar 1: *"Leg-by-leg resolution with a **live cash-out offer** is the
signature moment."* `04-the-sweat.md` specifies the presentation and the pacing dials; the arithmetic of the
offer is not specified anywhere I can find.

**What the research argues.** Buckshot Roulette is the only reference where the player acts inside the
tension, and **48.8% of all owners deliberately took a 50/50 shot at themselves** — roughly 5–8× the rate of
that funnel's optional-flourish achievements. They did it because the rule makes it correct: a self-shot on
a blank retains the turn, so when blanks outnumber lives, the maximally tense action *is* the maximally +EV
action. Drama and arithmetic point the same way by construction.

**If SBR's cash-out offer is priced at or above the fair value of the remaining legs, cashing out is always
correct and the sweat becomes a formality the optimal player skips.** Pillar 1 says nothing may make
resolution skippable by default — but a cash-out that is always right is exactly that, in economic form.

**Ruling requested.** Adopt as a binding constraint on the cash-out's pricing: **the offer must be
−EV against riding it in a named, reachable, and reasonably common class of ticket states**, so that riding
the sweat is sometimes the correct play and not merely the brave one. This is writable for the Monte Carlo
audit, which pillar 3 requires anyway.

**Falsifier.** If the offer is already specified this way somewhere I have not read, this is closed and
I would want the pointer.

---

### RF-8 — Tension is not retention; pillar 1 needs a second clause

**Canon, quoted.** `00-vision` pillar 1: *"The sweat is sacred… **All juice budget flows here first.**"*

**What the research argues.** Buckshot Roulette is the sweat, isolated and executed superbly: the highest
tension language in the set (2.2%), the lowest compulsion language (**2.6%**, one sixth of the other three),
and a median lifetime of **4.0h** with a 0.4% tail past 50 hours. The three retentive titles produce
compulsion (15–19% addiction language, 7.6–25.1h medians) from item economies and ladders that never touch
their resolution.

**The pillar is not wrong — it is incomplete.** "All juice budget flows here first" is the right priority
for making the moment great. It provides no budget for the systems that make a player come back tomorrow,
and on this evidence those are strictly different systems.

**Ruling requested.** Add a second clause to pillar 1, or a companion pillar, naming the retention layer
(item economy + ladder + collection) as an equally protected budget. Nothing is cut; the omission is named.

---

### RF-9 — The final round is designed as a wall; the best reference makes it a coronation

**Canon, quoted.** `01-core-loop.md`: *"**No borrowing on the final round.**"*

**What the research argues.** Balatro's funnel converts reach-Ante-8 → win-a-run at **96%** (74.7 → 71.7).
Its last round is the payoff for a run already won, not a final filter. SBR removes the safety net exactly
where Balatro removes the difficulty. Both are defensible; they are opposite bets, and canon took its side
without a reference behind it.

**Ruling requested.** Confirm the wall is intended, or flip the final round to a coronation and move the
last real filter one round earlier. Cheap either way; it is one line of config and one sim re-gate.

---

### RF-3 — Differentiation legibility (carried from `00-reference-list-proposal.md` §5, parked to here)

Parlay is still `coming soon` with no demo (verified 2026-08-12). Its store copy matches SBR canon on six of
seven structural elements — 8 rounds, 3 sets of picks, up to 6 legs, procedural fictional teams, debt to a
bookie as the fail fiction, limited buff slots, worsening odds. Nothing was copied in either direction; both
model real sports betting.

**But on a store page the two products currently describe themselves the same way**, and SBR's stated
differentiators — the sweat, the information axis, getting limited — are the three things a store page is
worst at conveying. RF-7 and RF-8 both bear on this: the sweat is the differentiator, and it is also the part
this research says will not, by itself, retain.

**Ruling requested.** None yet — noted, as Allen directed. Re-open when a capsule or trailer is scoped.

---

## 4. What this lane did not answer

- **Exact resolution timings** for all four titles. UNREACHED — no play access, no footage pass run.
  Two are answerable by Allen from play (`07-questions-for-allen-from-play.md`), two would need footage.
- **The near-miss lever, everywhere.** The review-corpus lexicon returned **1 hit in 4,000 reviews**. That is
  an instrument failure, reported as one per `C37` — reviewers do not narrate moments. **No claim in this
  lane rests on it**, and the most-cited compulsion lever in gambling design is therefore unmeasured here.
  It would be the strongest single argument for the parked literature probe (RF-2).
- **Balatro's per-ante base-chip curve.** The wiki table did not extract; recorded UNREACHED rather than
  asserted from memory. The escalation-rate comparison against CloverPit's ×3.3 is consequently not made.
- **Retention ranking causality.** Pattern A is confounded by price, length, genre and title age. Stated as
  suggestive.

## 5. Need Allen

Seven rulings, in the order I would take them:

1. **RF-4** — rule run length and the G3 band together. *Recommend: hold 5–8% and constrain runs to ≤30 min.*
2. **RF-7** — adopt the cash-out −EV constraint. *Recommend: yes; it is the cheapest and it protects pillar 1 arithmetically.*
3. **RF-5** — pull a difficulty ladder into v1. *Recommend: yes, two or three rungs; this is a scope call.*
4. **RF-8** — second clause on pillar 1. *Recommend: yes; naming it costs nothing.*
5. **RF-6** — is a non-`p` upside-variance rescue in scope? *Recommend: yes, and S2/COMPS is the cheapest form.*
6. **RF-9** — wall or coronation on the final round? *Recommend: coronation; move the filter one round earlier.*
7. **S1–S6** — the steal list needs no ruling, only a yes to schedule. *Recommend: S1, S3, S5 immediately — all three are nearly free.*

**Next, on your word:** the four Tier-2 probes (Gambonanza's soft landing, Luck be a Landlord's tuned debt
curve, Dungeons & Degenerate Gamblers' vocabulary onboarding, Scritchy Scratchy's price point).
