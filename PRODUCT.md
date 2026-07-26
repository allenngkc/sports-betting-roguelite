# Product

<!-- impeccable:product-schema 1 -->

> **Source of record.** This file is an index for design tooling, not a second authority. Every fact
> below is drawn from `design/00-vision.md`, `design/08-art-direction.md`, `DECISIONS.md`,
> `README.md`, `PLAYTESTS.md`, and `docs/tv-sweat-refinement/PRD.md`. Where this file and those
> disagree, **they win** and this file is wrong and must be corrected.
>
> Facts marked `[INFERRED]` were derived from repository evidence rather than stated by Allen, and
> are awaiting his confirmation. Drafted 2026-07-24.

## Platform

web

Confirmed by Allen 2026-07-24, with an important split:

- **The design workflow is web.** Tokens, components, layout iteration, and mockups are produced and
  reviewed the way a website would be. Impeccable's `web` tooling path is correct.
- **The artifact must not feel like web.** The TV sweat is a broadcast on a television inside a
  room, viewed from a couch. It must not read as a webpage, a dashboard, or an app screen. Any
  layout that could be mistaken for a website has failed, regardless of how well it scores on
  ordinary web craft heuristics.

Build environment is **Unity 6 (6000.5.3f1)**, project at `unity/SBR`, scene `Room`. Distribution
targets a web build on itch for the prototype gate (`design/00-vision.md`) with a desktop storefront
implied by the commercial v1 target.

## Users

The player is a single-player roguelite fan, playing at a desk or on a couch, in sessions built
around one run.

Two audiences must both be served by the same run, and pillar 2 makes their priority explicit:

- **The player who has never placed a bet.** Must have fun in round one picking underdogs. Betting
  literacy is not the price of entry.
- **The player who knows betting.** Finds +EV, arbitrage, hedging, line shopping, and getting
  limited as the mastery vocabulary — discovered through items, never taught in a tutorial wall.

The prototype gate is measured on strangers, not on Allen: strangers replay the vertical slice
unprompted.

## Product Purpose

A roguelite about the life of a sports gambler. The player meets escalating profit targets by
building parlays, then rigs the game with relics, gurus, and insider tips — and survives the sweat
as each leg resolves live.

The fantasy is **engineer luck, not get lucky**. A run starts with a degenerate picking favorites
and ends with a sharp running arbitrage across crooked books, fading shill gurus, and cashing out at
the exact right second.

Success is defined at three levels:

- **Design (Phase 0 gate):** the core loop is fun on paper and survives the math in
  `design/02-betting-math.md`. Met — Phase 1 verdict CONTINUE, 2026-07-10.
- **Prototype (Phase 2 gate, current):** strangers on itch play a web build twice without being asked.
- **Commercial (v1):** $15K net and 500 reviews. Balatro numbers are a lottery ticket, not the plan.

## Positioning

The direct competitor is **Parlay** (Urple, unreleased as of Jul 2026), which resolves picks flatly.
Three things a neighboring product could not truthfully copy without rebuilding:

1. **The sweat and live cash-out.** Leg-by-leg resolution with a live, causally-priced cash-out
   offer is the signature moment, not a results screen.
2. **The information axis.** Gurus and insiders make *information quality* a mechanic — shill tips,
   "trust me bro" sources, and the book limiting winners.
3. **Real betting-edge concepts as mechanics.** +EV, arbitrage, and hedging are the item design
   space, not flavor text.

The stance is satire: dark comedy about degenerate gambling culture, aimed at the industry. This is
both the honest position and the press-friendly one.

## Operating Context

The game is played from inside a **first-person, walkable compact room at night** (CloverPit-scale,
Tokyo minimalist, no kitchen). The character is invisible — presence through movement, not portrayal.

All gameplay UI is **diegetic**. There is no floating HUD; seed and round info live on screen chrome:

| Surface | Role |
|---|---|
| **TV**, across the couch | Watch the sweat — broadcast scorebug, live win-prob, ticker. Sit on the couch under the bunk bed and the camera settles on the TV. |
| **Laptop** on the desk | The book — build tickets, browse the shop, read run info. |
| **Phone** | Bookie notifications, debt messages, cash-out buzz. Audible anywhere in the room. |
| **Window** | Time-of-day and mood light, state-driven. |
| **Mini fridge** | Flavor interaction; contents track run state. |

**The room is the health bar.** Four states — Baseline, Heater, Sweating, Buried — triggered by
bank, streak, and debt. Scope guard: the Phase 2 slice ships state 1 plus a lighting/prop-decal
layer faking states 3–4. Full four-variant art lands after the slice's gate.

The signature viewing posture is **the couch, from a distance, potentially with audio muted**. That
posture, not a desktop-monitor posture, sets the legibility bar for everything on the TV.

## Capabilities and Constraints

**Architecture.** Headless C# core under `engine/`, Unity presentation layer at `unity/SBR`, console
harness at `game-console` (`dotnet run --project game-console`). Effect hooks and data-driven
content. Engine RNG is a controlled resource; presentation determinism is derived separately and may
not draw from engine RNG.

**Shipped and validated as of 2026-07-24.** Charm expansion is holdout-validated: 22 items (15
passives, 7 consumables), the DEALT-HAND shop (4+3 per visit, Ask for the Manager redeals), locked
contract modifiers (Free Bet / Double-or-Nothing, one per ticket), Bookie's Marker, and Ref's
Whistle. All six gates pass on held-out seeds. Engine 144/144, Unity 40/40.

**Betting markets currently implemented:** moneyline; total goals Over/Under; both-teams-to-score
Yes/No; total corners Over/Under; total cards Over/Under; anytime scorer.

**Terminology** that future work must use consistently: run, round, ticket, leg, parlay, the sweat,
cash-out, bank, debt, charm, relic, guru, insider, vig, the pending window, Mulligan, Ref's Whistle,
Bookie's Marker.

**Hard constraints:**

- Fictional leagues, teams, and players only — IP safety and comedy both require it.
- Presentation may elaborate an engine beat but may never contradict it.
- Solo developer plus AI collaboration, scheduled around school/co-op terms.
- Effectively $0 cash budget; time is the only currency.
- First shippable slice targeted well under a year of part-time work.
- Expect and accept a PEGI 18 / gambling-theme rating, as Balatro did.

**Explicitly undecided:**

- The game's name. SBR is a codename until Phase 3 (decided 2026-07-10).
- Character visibility — hands, reflection in the TV during static. Mood garnish, decided during the slice.
- Whether the room grows with meta-progression. Parking-lot idea, not committed.
- TV layout for the sweat — Decision A, open as of 2026-07-24, resolved by the visual-design track.

## Brand Commitments

**`design/08-art-direction.md` was deprecated by Allen on 2026-07-24.** Its casino-neon-on-black
palette, its green/red/gold color-purity rule, and its CRT-treatment prescriptions are **no longer
binding**. They are retained as evidence of what the product is, and as an explicit *anti-reference*
for what it should no longer look like. The TV sweat refinement is a **redesign**: a new visual world
is invented rather than the old one polished.

What survives as genuinely binding:

- **Name:** deferred. Do not invent one.
- **Diegesis is non-negotiable.** Every interface is a screen inside the room, seen from a couch.
  Nothing floats in a HUD. This is a product-truth constraint, not a style choice.
- **Voice:** dark comedy, satirical toward the gambling industry, never celebratory of it.
- **Fictional leagues, teams, and players only.**
- **Typography carries heavy load**, because the surface is dense with numbers read at distance.
  *How* it does so is open.

Deliberately released, 2026-07-24 — the new visual world may reinvent all of these:

- the color language, including whether green/red/gold retain money meanings at all;
- the CRT/phosphor/scanline treatment;
- the "casino neon on black" register;
- the marketing money-shot composition.

Reference games, recorded as calibration rather than instruction: Balatro (effect economy,
jargon-as-mastery, one-screen scope), CloverPit (debt pressure, oppressive-comic tone, juice
density), Raccoin / Scritchy Scratchy (juice standards, price point).

## Evidence on Hand

Real, in-repository, and usable — do not fabricate around these:

- `PLAYTESTS.md` — human playtest log through #16. Playtest #9 passed 2026-07-15; the strategy
  pillar landed, quoted as "comparing which relic works nice with another relic." Ask for the
  Manager ratified KEEP. Theater look and broad feel approved in #10–#16. Playtest #16 parked the
  procedural-audio revisit.
- `sim-report-4-holdout2.md` — 7.55M-run frozen holdout validation; skilled win rate 7.0%, inside
  Allen's 5–8% band, median round 5.
- `DECISIONS.md` — append-only decision log with dates and reasoning.
- `PLAN-REVIEW-LOG.md` — five adversarial review rounds to APPROVED.
- `docs/tv-sweat-refinement/` — current TV sweat PRD, visual design, bug ledger, source audit.
- `design/00`–`design/11` — the design bible.

**Absences that must not be papered over:** there are no real customers, revenue figures, press
quotes, wishlist counts, or store reviews. There is no runtime performance benchmark for the TV
sweat. The `TVS-H01`–`TVS-H03` items in the bug ledger are source-confirmed *candidates*, not
reproduced bugs, and no seed, reproduction rate, screenshot, or test result exists for them yet.

## Product Principles

1. **The sweat is sacred.** Leg-by-leg resolution with a live cash-out offer is the signature
   moment. Nothing may make resolution instant or skippable by default. Juice budget flows here first.
2. **Truth before drama.** The presentation may elaborate an engine beat but may not contradict it.
   One revealed source of truth; no helper infers a hidden outcome earlier than its causal reveal.
3. **Jargon is the mastery layer, not the entry fee.** Round one must be fun without betting
   literacy; the vocabulary is discovered through items.
4. **Every mechanic is mathematically legible.** If a mechanic's expected value cannot be written
   down for the Monte Carlo audit, it is not designed yet.
5. **Satire, not glorification.** The game is about the industry, not an advertisement for it.

## Accessibility & Inclusion

- **Audio independence is a product requirement, not a courtesy.** Every state and payoff must read
  with master audio muted. No acceptance result may depend on a whistle, sting, crowd swell, or
  spoken line. Audio may never be used to rescue visual readability.
- **Couch-distance legibility.** The TV is read from across a room, not from a monitor. The standing
  bar: a player can state their active requirement within three seconds of looking at the TV.
- **Color-as-sole-channel is NOT a binding constraint.** Released by Allen 2026-07-24; the visual
  world may use color however it wants. Note that this is a released *mandate*, not a released
  *problem*: PRD §1 still requires a player to answer six questions from across a room, and PRD §8.5
  still requires open/suspended/unavailable/cashed-out to be unmistakable. Whatever channels the new
  world chooses must clear that bar on their own merits.
- **Pausing is literal.** Standing up freezes exact presentation state and sitting resumes it, which
  also serves players who need to interrupt a session at any moment without losing the sweat.
