# Open Questions — parking lot

Anything undecided lives here so it can't get lost. When resolved: move the outcome to the relevant doc + DECISIONS.md, strike it here with a date.

## Identity
- [ ] Game name / working title
- [ ] Art direction: pixel art vs clean vector/flat vs CRT-degenerate aesthetic (interacts with juice plan and asset sourcing)
- [ ] Player character fiction: silent avatar, or a characterized degen with a life sim frame (apartment decays/improves with bankroll)?

## Core loop (design/01)
- [ ] Run length target and round count
- [ ] Failure-state fiction (satirical, not grim — but what exactly?)
- [ ] Second resource beyond bankroll: book reputation? guru credibility? none?
- [ ] When do multiple books unlock (prerequisite for arbitrage mechanics)?
- [x] ~~Multiple concurrent tickets~~ — resolved 2026-07-07: baseline 3/round, upgradable via shop/relics/events (DECISIONS.md)
- [x] ~~Prestige/carry-over between runs~~ — resolved 2026-07-07: breadth-only unlocks, prestige-as-power dropped (DECISIONS.md)

## Math (design/02)
- [ ] Correlation model for correlated parlays (shared latent momentum factor?)
- [ ] Odds display format default (lean: American for flavor, toggle for readability)
- [ ] Max information state: exact p or always a confidence interval? (lean: interval)
- [ ] Anti-frustration: bad-beat insurance as visible item vs any hidden pity system

## Mechanics (design/03)
- [ ] Cap on resolution-warping relics per ticket?
- [ ] Guru accuracy estimation UX — how does the player track record without a spreadsheet feel?
- [ ] Insider-tip consequence table (book flags? sting events? how harsh?)
- [ ] "Early Payout" relic: Variant 1 (any hitting leg pays, order-independent) vs Variant 2 (sequential, stops paying when a leg dies)? Lean: Variant 2 — cheaper EV, matches serial sweat
- [ ] "Piggy Bank" redemption trigger: smash at will (lose future accrual)? auto-break on ticket bust (consolation engine)? end-of-round only? And does 2× survive Monte Carlo, or does the rakeback-grinder archetype need a cap/upkeep cost?

## The sweat (design/04)
- [x] ~~Presentation proposal~~ — signed off 2026-07-07 (DECISIONS.md); detail design is a Phase 2 workstream
- [ ] Mid-sweat agency ladder (design/04, PROPOSED): confirm the band-tiered verb approach; then design the active-charge relic set and decide whether partial cash-out is baseline or an early unlock
- [ ] Fast-forward policy (anti-frustration vs Pillar 1)
- [ ] Live betting during the sweat: v1 or deferred? (lean: deferred; cash-out is the only live decision in v1)
- [ ] Announcer/sound direction

## Technical (design/05)
- [ ] UGUI vs UI Toolkit (lean: UGUI for community-answer density)
- [ ] Steamworks wrapper choice (decide Phase 3)
- [ ] WebGL determinism golden-seed test — schedule early in Phase 2

## Deferred by design (not questions — future pillars)
- v2: prediction markets (bet on anything — elections, weather; broadens the satire)
- v2: live betting mid-sweat
- v2: player prop bets ("X to score 25+") — the "praying for a guy" feeling at leg level; drama-generator event vocabulary must keep player entities addressable now so props attach later
- v2+: mobile port
- v3: TBD (daily challenge seeds are nearly free given RNG discipline — maybe v1.x)
