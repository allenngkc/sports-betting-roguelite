# 09 — CloverPit's Math vs Ours (research for the relic rework)

_Research 2026-07-12, for the 3-relic + 3-consumable rework. This doc is CONTEXT, not the design —
the rework itself gets grilled separately. Sources at the bottom; numbers verified against the
CloverPit wiki + mechanics guides current as of July 2026._

## 1. How CloverPit's math actually works

### The payout formula — four unbounded multiplicative factors

```
payout = SymbolValue × SymbolsMultiplier × PatternValue × PatternsMultiplier
```

Every charm touches one (occasionally two) of these four factors, and all four are UNBOUNDED.
This is the Balatro chips×mult architecture with two extra factors. Charms therefore compound:
two charms on different factors multiply each other's contribution; a run's power is the product
of everything you bought. "Number go up" is structural, not decorative.

### Symbol probability — weights, not percentages

Base weights: Lemon/Cherry 1.3 (~19.4% each), Clover/Bell 1.0 (~14.9%), Diamond/Treasure 0.8
(~11.9%), Seven 0.5 (~7.5%). A "+1 symbol" phone-call boost adds a FLAT +0.8 weight — so
probability-side buffs have built-in diminishing returns (~+10% first boost, ~+2.4% by the tenth).
Probability is deliberately the saturating axis; payout is the exploding one.

### Luck — outcome forcing with a deterministic pity schedule

Each Luck point FORCES one reel symbol to match (10 Luck ≈ guaranteed win, 15+ ≈ guaranteed
jackpot, unless 666 overrides). The "random" luck gifts follow a fixed schedule (one of five
pre-selected sequences in deadline 1; every 5–6 spins in deadline 2; 6–7 in deadline 3...), plus
rubber-banding: 4 consecutive dead spins grant +5 Luck, growing +1 per further loss. Players who
decode the schedule describe the shift as "no longer gambling — engineering fate." The pity
system is invisible-but-learnable, and learning it IS the mid-game skill curve.

### The requirement curve — brutally exponential, identical every run

Deadlines (3 rounds each, 3–7 spins per round): **75 → 200 → 666 → 2,222 → 12,500 → 33,333 →
66,666 → 200,000 → 1,000,000** — average ×3.3 per deadline over 9 deadlines (then a
super-exponential formula for endless). The curve is fixed and public: the game TELLS you that
linear play is dead by deadline 4-5 and multiplicative scaling is mandatory. The requirement
curve and the multiplicative item system are two halves of one design.

### The rest of the economy

- **Interest**: 7% per round on ATM deposits (raisable by charms) — banked coins compound, so
  save-vs-spend is a real decision with a growth payoff, not just safety.
- **666**: the house's counterattack — sixes can veto wins/jackpots, but bounded (triple-six
  capped at 30%, ~14.4%/spin for any six); risk items (Evil Deal: DOUBLE the 666 odds AND double
  both multipliers + interest + tickets) sell you variance on that bounded threat.
- **Consumable layer**: Red-Button charms carry charges (Midas Touch: 3-ticket relic, 4 charges,
  each permanently grows all symbol values); some effects are single-use, some until-end-of-round.

### The charm taxonomy that matters (with examples)

| Class | Example | Effect shape |
|---|---|---|
| Static adder | Ring Bell (2🎟) | +1 Symbols Mult per button charge, permanent |
| **Ratchet** (grows on trigger) | Pentacle (3🎟) | Symbols Mult +1, +1 more every spin with 5+ patterns |
| **Ratchet** | Diesel Locomotive (2🎟) | pattern values permanently +base after 3 dry spins |
| Cross-scaler | Dark Lotus (Legendary) | Symbols Mult = resell value of ALL owned charms |
| Cross-scaler | The Collector (Legendary) | Patterns Mult +1 per trait-bearing charm owned |
| Risk-double | Evil Deal (50🎟) | double 666 chance AND double mults/interest/tickets |
| Action economy | Cat Food (4🎟) | +2 spins per round |
| Probability nudge | Horseshoe | +10% win chance, 3 spins (community-rated "unnecessary") |

Note what the community tier lists say: the probability nudges rate LOW, the ratchets and
cross-scalers rate top. The player base has empirically discovered that bounded-axis items are
weak and unbounded-axis compounding items are the game.

## 2. Our model, restated with the sim's evidence

Four numbers per bet: `p` (true prob), `o` (offered odds), `s` (stake), `payout = s×o`.
`EV = p·s·(o−1) − (1−p)·s`; vig is the house edge (5% overround); targets
[400, 460, 520, 650, 800, 1000, 1500, 2800] ≈ **×1.32/round**; debt-as-HP floats one clean miss
at 1.5× juice.

Current 8 relics, restated as math objects — ALL are static, none ratchet, none cross-scale:

| Relic | Factor touched | Shape | Audit (Δwon%, skilled, granted free) |
|---|---|---|---|
| Boosted Odds | o (one leg) | static ×1.15 | +7.6pp (but −0.09 mean rounds: bot misuse) |
| Promo Code | o (vig removal, 1 ticket/round) | static, ≈+5% EV once | +4.7pp |
| High Roller | payout | static ×1.15 gated on s ≥ bank/2 | +7.7pp |
| Bankroll Insurance | s (downside) | refund half stake, 1/round | +4.3pp, DOMINANT survival |
| Mulligan | p (one leg → voided) | 1/round | **±0 — DEAD** |
| Lucky Charm | p (final leg +3pp) | static | +2.7pp |
| Early Payout | payoff function | +15% stake per green leg | +6.5pp |
| Piggy Bank | accounting | 2× vig banked, pays on bust | +1.7pp |

Sim ground truth (sim-report 2026-07-09): skilled mean per-ticket EV NEVER crosses zero (target
was ≈ round 4); it gets MORE negative every round (−$2 → −$148) because stakes grow while every
ticket still pays full vig. The EV arc's Band 2 ("crossing zero") and Band 3 ("sanctioned
brokenness") do not exist mathematically in v0. Win rate 11.5% comes from variance management +
debt-as-HP, not from engine building.

## 3. The comparison — what actually differs and why it matters

### 3a. The requirement curve and the item system are one design, and ours are mismatched

CloverPit: ×3.3/deadline requirement ⇒ multiplicative items are MANDATORY ⇒ buying/combining
charms is the game. Ours: ×1.32/round requirement ⇒ linear survival tools suffice ⇒ the sim's
best relic is INSURANCE (a variance damper), and nothing that builds an engine matters much.
**If the rework wants engine-feel, requirement growth and item growth must steepen together** —
items alone can't do it (they'd trivialize flat targets), targets alone can't either (they'd just
kill everyone). This is a coupled retune, and /sim exists to find the pair.

### 3b. Bounded vs unbounded axes — put scaling where the ceiling isn't

CloverPit puts ALL scaling on unbounded payout-side factors and gives the bounded axis
(probability) deliberately diminishing returns. Our four numbers split the same way:

- **Bounded**: `p` (≤1 — and our book already prices it, so +p is just vig erosion), `s` (≤bank).
- **Unbounded**: `o`-side improvements, payout multipliers, payoff-function rewrites (per-leg
  cash flows), accounting flows (rebates, interest).

Our own audit already agrees with CloverPit's community: the p-side relics (Mulligan ±0, Lucky
Charm +2.7pp) are bottom-tier; payout-side (High Roller +7.7pp, Early Payout +6.5pp) top the
win column. **Rework law candidate: passives scale unbounded axes; the p-side is for
consumables (timing-scoped safety), never for engines.**

### 3c. Additive vs multiplicative composition

Our 8 relics contribute independent, additive EV nudges — owning all 8 is the sum of 8 small
numbers. CloverPit charms multiply: SymbolValue ratchets × SymbolsMult ratchets × PatternsMult
cross-scalers. One multiplicative composition channel (e.g., a payout-multiplier product that
several items feed) is what makes a build feel like an ENGINE rather than a discount stack.
Balatro's entire genre-defining feel is chips×mult; CloverPit just added two more factors.

### 3d. Ratchets — permanent growth on an in-run trigger

Zero of our relics grow during a run. CloverPit's top items are all "permanently +X whenever Y"
(Pentacle, Midas, Diesel Locomotive, Dung Beetle). A ratchet converts play events into permanent
run-power, which (a) makes every spin matter beyond its own outcome, (b) creates the Band 2→3
trajectory ORGANICALLY — early buys are weak now, monstrous later — the EV arc as an emergent
property of item math rather than a tuning aspiration. **The single highest-leverage structural
idea to steal.** Natural triggers in our verbs: green legs, busts survived, cash-outs taken,
floats repaid, vig paid, all-ins placed.

### 3e. Determinism and pity — validated, and worth surfacing

CloverPit's scheduled luck + rubber-banding proves players ADORE decodable determinism (it
reads as skill, not as rigging). We already run a fixed outcome universe and deterministic
presentation — same philosophy. The transferable idea is bounded, learnable pity: bad-beat
protection as a visible ITEM (design/02's open question) rather than hidden mercy. Their 666
cap (30%) is the mirror lesson: the house's counterpunch must be bounded and legible too.

### 3f. What CloverPit has that we deliberately don't

- **Interest (7%/round on deposits)**: their save-vs-spend tension. Our closest is Piggy Bank
  (bust-triggered, insurance-flavored). A discipline-flavored compounding sink is a real
  candidate axis for the rework (it monetizes NOT betting the whole bank — a decision our
  uncapped stakes currently make strictly binary).
- **Action economy items** (+spins/round): our analog is tickets-per-round (baseline 3) — cap
  raises were already flagged as skill expression (DECISIONS 2026-07-07).
- **Their probability axis saturates by design** (+0.8 weight flat adds). If we ever buff p,
  same trick applies: additive-on-weights, not additive-on-percentage.

### 3g. Consumables vs passives (for the 3+3 split)

CloverPit's charge/single-use layer does the TIMING game (when to press the button) while
permanents do the BUILD game — exactly the split Allen called for after playtest #1. Mapped to
our verbs: consumables want to live at decision moments (before lock: odds/stake tools; during
the sweat: p-side interventions, the design/04 Band-2 actives like Timeout; after a bust:
recovery). A p-side effect that would be dead as a passive (Mulligan!) can be strong as a
consumable precisely because the player picks the moment — timing skill replaces always-on EV.

## 4. Implications for the 3-relic + 3-consumable rework (framing, not design)

1. **The retune is coupled**: pick target curve steepness and item scaling TOGETHER; sim S3/S4
   gates re-run per candidate pair. (S3 naive median death 3–4 must survive the change.)
2. **Three passives probably want**: one RATCHET (trigger → permanent growth), one
   multiplicative payout-side engine, one economy/compounding piece — all on unbounded axes.
3. **Three consumables probably want**: p-side safety with player timing (the Mulligan lesson),
   a pre-lock odds/stake tool, and a sweat-moment intervention (first Band-2 agency verb).
4. **Keep the audit law**: every item's EV must remain /sim-computable (design/02 rule) — ratchets
   and cross-scalers included; the combo scan (`--combos`) becomes mandatory once items multiply.
5. Open questions for the grill: steepen targets how much? does a multiplier product cap or
   ride uncapped per Band-3 doctrine ("gate when, not how high")? do consumables buy in the shop
   alongside relics or drop from play? sell-back (playtest #1 asked for it)?

## Sources

- CloverPit wiki (wiki.gg): [ATM / debt / interest](https://cloverpit.wiki.gg/wiki/ATM),
  [Lucky Charms](https://cloverpit.wiki.gg/wiki/Lucky_Charms)
- [NeonLightsMedia — CloverPit mechanics explained (weights, luck, 666, schedules)](https://www.neonlightsmedia.com/blog/cloverpit-guide-mechanics-explained-luck-secrets)
- [Steam Community — CloverPit Mechanics Explained](https://steamcommunity.com/sharedfiles/filedetails/?id=3577735332)
- [Steam Community — CloverPit Strategy Guide (payout formula, scaling builds)](https://steamcommunity.com/sharedfiles/filedetails/?id=3576880194)
- Community tier lists: [Pro Game Guides](https://progameguides.com/cloverpit/best-cloverpit-charms-tier-list/),
  [GameRant](https://gamerant.com/cloverpit-best-lucky-charms-tier-list/)
- Internal: design/02-betting-math.md, engine/RelicCatalog.cs, sim-report.md (2026-07-09)
