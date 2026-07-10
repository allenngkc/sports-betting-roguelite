# 01 — Core Loop

## Run structure (current straw man — challenge everything here)

A run = a **season** of N rounds (weeks). Each round:

1. **The board.** A slate of fictional matchups is generated with offered odds (vig included). Player sees surface stats; deeper truth requires information items.
2. **Build the ticket(s).** Player composes bets — singles up to 6-leg parlays — allocating limited bankroll. Relics/gurus/insider info modify the numbers. Concurrent tickets: baseline 3 per round, limits upgradable via shop/relics/events (DECIDED 2026-07-07). Tickets are a *portfolio* — raising the cap is a variance-management skill upgrade, not just volume.
3. **The sweat.** Legs resolve one by one with live cash-out offers (see `04-the-sweat.md`). This is the payoff phase.
4. **Settle up.** Meet the round's profit target (the "ante") or lose. Targets escalate faster than safe betting can match — forcing riskier tickets or better edges.
5. **The strip.** Between rounds: shop (relics, guru subscriptions, tools), events (insider approaches you, book offers a promo, guru drama), upkeep (subscriptions charge, debts compound).

## Pressure escalation (the difficulty ratchet)

Candidate ratchets, probably layered:

- **Profit targets grow geometrically** (the Balatro ante).
- **The book adjusts.** Win too much and you get **limited**: max stakes cut, best odds hidden — the real-world sharp's problem as a difficulty mechanic. Forces diversification into new books/bet types.
- **Vig creep.** Later rounds have worse baseline odds (Parlay does "fixed odds worsen over time"; ours should worsen *reactively*, which is smarter and more thematic).

## Failure state — debt-as-HP (DECIDED 2026-07-09, from Week 5 sim findings)

Missing a target no longer ends the run outright. With no debt: **the bookie floats you** — bank is topped up to the target (working capital to keep playing), and you now owe the shortfall plus juice (interest baked at borrow time; rate is a sim dial). Clear a later settle at target + debt and it's repaid in cash. Miss *while in debt* and the bookie collects — run over. No borrowing on the final round. This converts geometric per-round death into accumulating pressure (the Week 5 sim showed even optimal play under hard targets wins ~7% of runs; S4 needs ~91% per-round survival), and the failure fiction resolves itself: you were never playing against the sports — you were playing against your bookie.

## Meta progression between runs

Unlocks (new relics, bet types, leagues, guru roster) rather than power creep — Balatro model. Ascension-style difficulty tiers post-v1. (OPEN: how much meta is too much for scope?)

## Open questions for discussion

- Round count and session length target (Balatro run ≈ 30–60 min; right for us?)
- One slate per round or multiple betting windows?
- Is bankroll the only resource, or is there a second currency (reputation with books? guru credibility?)
- ~~Multiple concurrent tickets per round~~ — DECIDED 2026-07-07: baseline 3, upgradable via shop/relics/events.
- Where does arbitrage structurally live — needs 2+ books visible per matchup. When do books unlock?
