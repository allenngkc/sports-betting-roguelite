# 03 — Mechanics Catalog

## The five axes

Every mechanic slots into one axis (its primary job) and may touch others. This is how "so many ideas" stays coherent: a new idea first gets classified, then we check whether its axis is already crowded.

| Axis | Player question it answers | Four-number target |
|---|---|---|
| **1. Information** | "What is *actually* going to happen?" | reveals `p` |
| **2. Odds** | "Can I get a better price?" | improves `o` |
| **3. Capital** | "How much can I put at risk?" | expands `s` |
| **4. Resolution** | "Can I bend the sweat itself?" | changes `p` live, or payout rules |
| **5. Economy/Meta** | "How do I convert winnings into power?" | shop, subscriptions, debts |

## Axis 1 — Information (the game's soul, and our differentiation)

- **Guru channels.** Subscribe (recurring cost, Axis 5 interaction) to get pick signals. Each guru has a *hidden accuracy* the player estimates from track record — some are sharp, some are coin-flippers with marketing, some are **shills who fade their own audience** (their picks are anti-signal, which a savvy player exploits by betting the opposite: a delicious discovery). Guru drama events: hot streaks, blowups, "exposed" scandals that crater a subscription you paid for.
- **Insider tips.** Rare events: a stranger offers information about a match. Trust mechanics — the tip has hidden reliability; acting on it may carry consequences (the book flags suspicious bets → accelerates limiting; occasionally the "insider" is a sting). High variance by design.
- **Scouting/stats tools.** Boring-but-honest info: buy tighter confidence intervals on `p` for specific leagues. The workhorse the flashy options are balanced against.

Balance rule: information must never make betting safe — it narrows the interval on `p`, it doesn't eliminate the sweat (Pillar 1).

## Axis 2 — Odds

Line shopping (multiple books, unlockable), odds boosts (book promos with strings attached), promo abuse (real-world sharp behavior: milk signup bonuses — book retaliates), correlated-parlay mispricing (see `02-betting-math.md`), a relic that freezes yesterday's better line.

## Axis 3 — Capital

Stake limit raises, loans (satirical payday-lender flavor, compounding upkeep), "makeup" structures, insurance items, bankroll-percentage auto-sizing tools (teach Kelly-lite thinking through an item).

## Axis 4 — Resolution

The spiciest and most dangerous axis (can cheapen the sweat if overdone): one-leg mulligan relics, "void a leg, ticket degrades to smaller parlay," live-hedge unlock, cash-out margin reducers, items that inject momentum events (a "ref you bought" — changes live `p`, at discovery risk). Cap: at most one resolution-warping relic active per ticket? (OPEN)

**Payoff-structure relics** (sub-family, per the generalized payoff functions in `02-betting-math.md`): relics that rewrite *when and how* a ticket pays rather than its odds. Anchor example — **"Early Payout"** (Allen, 2026-07-07): each hitting leg pays immediately while the full parlay payout is retained. Amplifies the sweat (cash drips per green leg — a juice moment we otherwise don't have) and pairs with cash-out via the interaction rule in 02. This family is where the most original relic designs will live.

## Axis 5 — Economy / Meta

Shop between rounds, guru subscription upkeep, book reputation (win too much → limited; lose like a fish → VIP perks: the book *wants* losers, another honest satirical mechanic), unlock economy across runs.

**Accounting-engine relics** (sub-family): relics operating on tracked run-level quantities (vig paid, volume churned, tickets busted) rather than on any single bet. Anchor example — **"Piggy Bank" / rakeback** (Allen, 2026-07-07): accrues 2× cumulative vig paid, redeemable on a trigger (smash rules OPEN). At 2×, betting volume itself turns profitable, deliberately creating the **rakeback grinder archetype** — thematically grounded in real promo/rakeback grinding culture, and orthogonal to the +EV sharp archetype. Engine requirement: vig computed at ticket lock as a first-class stat (formula in 02).

## Relic timing classes (added 2026-07-07, Allen)

Orthogonal to the five axes, every relic has a timing class by which hooks it uses:

- **Pre-game** — acts before the lock: `OnSlateGenerated / OnOddsOffered / OnBetComposed / OnTicketLocked`
- **Live** — acts during the sweat: `OnLegStarted / OnMatchEvent / OnLegResolved / OnCashOutOffered` (includes the mid-sweat active charges from design/04's agency ladder)
- **Passive/economy** — acts at settlement or between rounds

Implementation law: **store the base, compute the effective.** Locked odds are the contract and are never mutated; live effects register modifiers evaluated through the effect pipeline, so replays stay deterministic and the UI can show the story ("locked 1.90 → boosted 2.28"). Live effects that change `p` mid-sweat go through the drama generator's intervention seam (design/05). Combo space this opens (Band 3 fuel): manipulate live probability, watch the cash-out offer spike, sell the ticket at the top — market manipulation as a game verb.

## Effect hook list (contract with `05-architecture.md`)

`OnSlateGenerated, OnOddsOffered, OnBetComposed, OnTicketLocked, OnLegStarted, OnMatchEvent, OnLegResolved, OnCashOutOffered, OnCashOutTaken, OnTicketSettled, OnRoundSettled, OnShopEntered, OnEventChoice, OnLimited, OnRunEnd`

Every mechanic above must be expressible as subscriptions to these hooks. If one can't be, the hook list (not the mechanic) gets reviewed.

## Idea intake protocol

New idea → one line in OPEN-QUESTIONS.md with its proposed axis → we discuss → it either gets a section here, gets merged into an existing mechanic, or gets a dated "cut, because…" note. Cut ideas stay visible; they're fuel for v2/v3 (prediction markets are already parked there).
