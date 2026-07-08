# 00 — Vision

## One-line pitch

A roguelite about the life of a sports gambler: meet escalating profit targets by building parlays, then rig the game with relics, gurus, and insider tips — and survive the sweat as each leg resolves live.

## The fantasy

Not "get lucky" — **engineer luck**. The player starts as a degenerate picking favorites and ends a run as a sharp running arbitrage across crooked books, fading shill gurus, and cashing out at the exact right second. Real betting concepts (+EV, arbitrage, hedging, line shopping, getting limited) are the mastery vocabulary.

## Design pillars

Every feature must serve at least one; a feature that fights one gets cut.

1. **The sweat is sacred.** Leg-by-leg resolution with a live cash-out offer is the signature moment. Nothing may make resolution instant or skippable by default. All juice budget flows here first.
2. **Jargon is the mastery layer, not the entry fee.** A player who has never bet must have fun round one picking underdogs. Arbitrage/+EV/hedging are discovered through items, never taught in a tutorial wall.
3. **Every mechanic is mathematically legible.** The baseline bet is the four-number model (true probability, offered odds, stake, payout — see `02-betting-math.md`), but relics may rewrite the payoff function itself (per-leg partial payouts, rebates, accounting engines) or operate on tracked run-level quantities (vig paid, volume churned). The discipline: if we can't write down a mechanic's expected value for the Monte Carlo audit, it isn't designed yet. (Revised 2026-07-07 — the original "four numbers only" phrasing was too narrow; see DECISIONS.md.)
4. **Satire, not glorification.** Dark comedy about degenerate gambling culture — guru shills, "trust me bro" insiders, the book limiting winners. The game is *about* the industry, which is both the honest stance and the press-friendly one.

## Tone and setting

Fictional leagues and teams only (IP safety + comedy: procedurally named teams a la Parlay's "Atlanta Yams" — we need our own naming voice). Aesthetic direction TBD (see OPEN-QUESTIONS). Expect a PEGI 18 / gambling-theme rating like Balatro; accept it, don't fight it.

## Reference games

- **Balatro** — effect economy, jargon-as-mastery, one-screen scope
- **CloverPit** — debt/target pressure framing, oppressive-comic tone, juice density
- **Raccoin / Scritchy Scratchy** — third-wave juice standards, price point
- **Parlay (Urple, unreleased as of Jul 2026)** — direct competitor; watch its launch as free market research. Our differentiation: the sweat + cash-out (they resolve picks flatly), the information axis (gurus/insiders), and real betting-edge concepts as mechanics.

## Success criteria

- **Design success (Phase 0 gate):** the core loop is fun on paper to both of us and survives the math in `02-betting-math.md`.
- **Prototype success:** strangers on itch play a web build twice without being asked.
- **Commercial success (v1):** $15K net and 500 reviews is a strong first-game outcome. Balatro numbers are a lottery ticket, not the plan.

## Constraints

Solo developer + AI collaboration, built around school/co-op terms, effectively $0 cash budget (time only), Unity, first shippable slice targeted well under a year of part-time work.
