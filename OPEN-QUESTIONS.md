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

## Balance & structure (from the Week 5 sim, 2026-07-09 — these gate the Week 6 verdict)
- [ ] **Failure model.** Hard per-round targets make survival geometric: even skilled play clears ~72% of a round under a flat-early curve → 0.72⁸ ≈ 7% runs won; S4 needs ~91%/round. Flatter targets alone moved skilled median death only 1→2. Options: (a) targets tracking the skilled-EV curve + much stronger relic compounding; (b) **debt-as-HP** — miss a target and the bookie floats the shortfall at punitive interest (added to future targets); miss while already in debt = death. (b) converts instant death into accumulating pressure, is thematically native (the bookie!), and echoes CloverPit's debt frame. Allen to decide — core-loop change.
- [ ] **Book pricing noise.** SlateGenerator prices at true p × (1+overround) proportionally, so two-way de-vigging recovers true p *exactly* → Tout Sheet and Sharp Eye carry mathematically zero informational edge (the sim's skilled bot proved it). Proposal: the book prices off a noisy estimate (p_book = true p + noise, σ ≈ 3–5pp config dial). This creates genuinely mispriced lines to hunt — the actual sharp fantasy — and makes the information axis real. Changes PRD F3's "book is sharp" premise; Allen to sign off.
- [ ] Info relic follow-up: with book noise, Tout Sheet/Sharp Eye become edge-finders (interval/exact truth vs the book's line) — re-audit after.
- [ ] Relic power re-audit after retune (current death-floor makes the audit unresolvable; provisional Δ-mean-survival ranking: early_payout, lucky_charm, promo_code lead; tout_sheet, sharp_eye, bankroll_insurance ≈ dead).

## Math (design/02)
- [ ] Correlation model for correlated parlays (shared latent momentum factor?)
- [ ] Odds display format default (lean: American for flavor, toggle for readability)
- [ ] Max information state: exact p or always a confidence interval? (lean: interval)
- [ ] Anti-frustration: bad-beat insurance as visible item vs any hidden pity system

## Mechanics (design/03)
- [ ] Consumables vs passives implementation (design/03, from playtest #1): slot pool sizes (straw man 3+3), sell-back fraction (straw man 50%), which of the current 10 convert to consumables (lean: lucky_charm, mulligan, promo_code), and WHEN — lean: build Week 5 sim first on the current system, then rework items, re-run sim, and hold the Week 6 verdict on the reworked system. Allen to confirm sequencing.
- [ ] Cap on resolution-warping relics per ticket?
- [ ] Guru accuracy estimation UX — how does the player track record without a spreadsheet feel?
- [ ] Insider-tip consequence table (book flags? sting events? how harsh?)
- [ ] "Early Payout" relic: Variant 1 (any hitting leg pays, order-independent) vs Variant 2 (sequential, stops paying when a leg dies)? Lean: Variant 2 — cheaper EV, matches serial sweat
- [ ] "Piggy Bank" redemption trigger: smash at will (lose future accrual)? auto-break on ticket bust (consolation engine)? end-of-round only? And does 2× survive Monte Carlo, or does the rakeback-grinder archetype need a cap/upkeep cost?

## The sweat (design/04)
- [ ] First playtest data (Week 4 implementation agent, 2026-07-08): the bad-beat near-miss (75% one event before a 0% whistle) already lands hard even in text; predicted first repetitiveness = calm Momentum beats on heavily-favored legs (cash-out barely moves → no decision pressure), likely felt by ticket 2–3. Watch for this in Allen's S1/S2 runs; candidate fix is compressing consecutive Calm beats, not faster pacing overall.
- [ ] Week 5 sim note: the --auto baseline's flat $50 stake cap under-represents a naive bettor (mathematically cannot clear round 1 from $500 with a 2-leg favorite parlay); the sim's naive bot should stake ~25% of bank uncapped instead.
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
