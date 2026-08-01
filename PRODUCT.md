# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

Recorded as `web` because no native-mobile design language applies. The real runtime is **Unity
UGUI on desktop** (keyboard + mouse), rendered to a world-space canvas. Treat it as a fixed-size
screen surface, not a browser: no responsive breakpoints, no DOM, no CSS.

## Users

Allen — solo developer and the primary playtester — plays complete 30-minute runs at a desktop
with keyboard and mouse. He is simultaneously the designer, so playtest notes are the highest
authority on whether a surface works. The eventual audience is roguelite players who know the
genre's run/shop/relic vocabulary but are not necessarily sports bettors; the game must teach
betting mechanics through the interface rather than assume fluency.

## Product Purpose

A sports-betting roguelite. Across 8 rounds the player builds parlays from a procedurally
generated slate, stakes against a rising bank target, and then sweats each ticket leg by leg
while a live cash-out offer taunts them. Surviving a round opens a relic shop. Success means the
build-then-sweat loop stays tense and re-playable across many runs.

## Positioning

The satire thesis, which no neighboring product copies: **the bettor watches the app, not the
game.** The sportsbook interface is the protagonist surface — the place where hope is
manufactured and money dies. Supporting mechanisms that are ours: debt-as-HP (a missed target
floats the bank and books the shortfall at 1.5×, so a bad round wounds instead of killing);
a single sharp book with no exploitable mispricing except through relics; and locked odds —
prices freeze at slate generation and never move, so the player's read is the only variable.

## Operating Context

### The world all three surfaces share

Confirmed by the room/TV workstream 2026-07-28; room and TV directions are **approved and final**
(Allen, 2026-07-27, after seven concept rounds).

A cramped bunker room at night, in a wealthy high-tech city that has no use for the occupant. The
room is rotting; the city outside the window is neon and functioning; the screens are the only
things that work. Dark comedy about the gambling industry — **the machines are nicer than the
life.** Rendering register: **painterly semi-realistic** — not stylised, not photoreal.

Room construction: peeling paint, exposed black conduit and pipes, riveted steel, bolted
brackets, chipped institutional paint, two heavy bunk frames, a deep-set window onto a neon
skyline, a battered metal desk holding the laptop and an ashtray of cigarette butts. Room
palette is olive, khaki, drab green, rust, damp concrete; wall albedo is warm dirty plaster
(~`0.255, 0.245, 0.210`), so **the room physically cannot return saturated cool colour.**

Three light sources that must stay distinguishable: a warm dim fluorescent strip; a cool blue
window with **short reach** that pools locally and does not tint the room; and the screens, which
are **quiet, with faint spill.** A blue-tinted room is the explicit failure mode.

### The laptop is the occupant's own machine

The single most important characterisation split, and the one most likely to be got wrong. The TV
is a hardened industrial display **installed by an institution, not bought by the occupant** —
steel housing, rivets, stencilled equipment code, conduit continuous with the room's pipe runs.

The laptop is the opposite: **his own machine.** Personal, chosen, probably cheaper, probably
grubbier, possibly customised. SureThing must never read as institutional hardware, or it becomes
a second TV.

**Register split:** the TV is *hot* — it is the sweat, the player cannot influence it, and its
design makes one thing at a time unmistakable. SureThing is *calm* — it is where the player
thinks, compares, and commits. The two screens must feel like the same world and the same hand
doing different jobs. If they feel the same, one of them is wrong.

**Do not carry over from the TV:** its coarse grid and monumental type (correct at four metres,
crude at forty centimetres); its institutional register; brightness as the sole semantic channel;
and its quantised, un-eased motion. A tool you operate wants responsive, continuous feedback.

### Surface division

The player sits in the room; the **laptop on the desk** runs a fictional operating system whose
primary app is the SureThing sportsbook, and the **TV across the room** broadcasts the match.
This division is a hard product law, not a layout preference:

- The **laptop** owns everything the player controls — slate, markets, betslip, stake, staging,
  locking, the relic shop, and the record of placed tickets.
- The **TV** owns unrevealed drama — score, clock, win-probability movement, and outcome reveals.
- The laptop's MY BETS surface may only mirror what the TV has already revealed. It must never
  read engine state directly or run ahead of the broadcast.

The laptop screen is a **1024×704** canvas mapped onto a roughly 0.32×0.22m world-space surface
viewed in perspective, so the player reads it at an angle and at reduced effective scale. Board
controls freeze while a round is being swept.

## Capabilities and Constraints

- `/engine` is netstandard2.1 with zero Unity references and owns all rules; UI reads from it and
  never re-derives odds, probability, or outcomes.
- Six markets exist and are priced today (`engine/Domain.cs`): Moneyline, TotalGoals,
  BothTeamsToScore, TotalCorners, TotalCards, AnytimeScorer.
- **One selection per matchup.** Choosing a second market on the same matchup replaces the first
  (`BetslipModel.cs`). Same-game parlays are not supported and changing this is an engine project.
- `RevealedView` already exposes ticket identity, legs, states, stake, payout, plus score, clock,
  and win probability. Rendering the last three on the laptop is a design choice the current
  direction declines — the contract needs no expansion either way.
- Round flow: build a working slip → PLACE TICKET stages it → LOCK IT IN commits the round →
  sweat → settle → shop. Up to 3 tickets per round, $10 minimum stake, stakes uncapped to bank.
- The current UI ships a 660px-wide board with a right-hand slip; the redesign is not bound by it.
- **Asset budget (confirmed 2026-07-25):** custom fonts and sprites are authorized —
  TextMeshPro font assets, sprite atlases, 9-slice borders, simple shaders.
- **Redesign scope (confirmed 2026-07-25):** the SureThing sportsbook app *and* the laptop OS
  chrome around it. `TvSweatScreen` is out of scope.
- **No image generation exists in this harness.** Concept imagery must be hand-authored as code,
  or generated by Allen from written prompts. Never promise rendered comps.

## Brand Commitments

- The in-world sportsbook is named **SureThing**. The laptop runs a fictional OS that must not
  imitate a real one closely enough to read as a clone.
- Voice: transactional copy stays literal and unambiguous — selections, prices, stake, payout,
  states, and disabled reasons say exactly what is true. Satire is permitted in non-critical
  labels and flavor text. Never imply a guaranteed win.
- No real operator marks, copy, iconography, screenshots, or characteristic color systems.
  Borrow task-level principles only: density, hierarchy, progressive disclosure, persistence.
- **Not binding:** the violet/purple palette from the previous design package. Allen lifted the
  palette constraint on 2026-07-25; color is fully open. Earlier docs asserting a purple ledger
  or reserving specific colors for money events are superseded.

Binding across all three surfaces (room, TV, laptop):

- **Diegesis is non-negotiable.** Every interface is a real screen on a real object in a real
  room, viewed at an angle, with glass, reflection and dust. Nothing floats in a HUD.
- **Lifted blacks.** No screen in this room shows pure `#000000`. A screen whose blacks beat every
  shadow in the room reads as composited rather than photographed — reported as the single
  strongest belongs/does-not-belong signal found across seven concept rounds.
- **The unified grade** covers every surface: grain, haze, lifted blacks, bloom, chromatic
  aberration, vignette. Spec at `docs/tv-sweat-refinement/unified-grade-spec.md` in the `tv-sweat`
  worktree; the room lead owns the volume. SureThing renders inside this pass, not exempt from it.
- **Fictional leagues, teams, and players only.** IP safety and the comedy both require it;
  concept renders came back with real clubs three times, so watch for it.
- **Voice:** dark comedy, satirical toward the gambling industry, never celebratory of it.
- **The game name is deferred.** Do not invent one.
- **Game-wide art direction** (Allen, 2026-07-26): high-tech city, dystopian. The player is not in
  a poor world — they are in a wealthy one that has no use for them.

**Colour language — SureThing owns its own (Allen, 2026-07-28).** The TV brief's §5 asserts
"green/red retired game-wide, gold is money" as binding on all three surfaces, but the TV
worktree's own `PRODUCT.md` lists colour language — *including whether green/red/gold retain
money meanings at all* — as deliberately released and undecided, and states that the laptop has
no owning art document and that the TV worktree "must not unilaterally define" one. Allen ruled
that SureThing decides its own colour language. The TV keeps gold. Cross-surface coherence is a
goal to pursue by choice, not an inherited constraint.
- **Energy register (confirmed 2026-07-25):** calm while building a ticket, loud during the
  sweat. "Calm" means composed and confident, not plain or visually empty — the build surface
  should still be distinctive. The app is explicitly *not* the slot machine; overstimulation is
  a failure condition.

## Evidence on Hand

- `PRD-prototype-v0.md` — signed-off prototype scope, relic catalog, tuning defaults.
- `DECISIONS.md`, `OPEN-QUESTIONS.md`, `PLAYTESTS.md` — ratified rulings and playtest history.
- `sim-report*.md`, `sim-smoke-p*.md` — Monte Carlo balance evidence.
- `docs/6-memo/2026-07-18-dopamine-direction.md` — the result-cadence diagnosis behind the
  loud-sweat register.
- `docs/design/surething-ui-revamp/` — the prior purple design package. Retained as evidence of
  product structure and information hierarchy; superseded as visual authority.
- No real users, revenue, telemetry, or external validation exists. Do not fabricate any.

## Product Principles

1. **The number never lies.** Every price, stake, payout, and disabled reason is literal and
   present. Satire never occupies a slot where a fact belongs.
2. **The laptop decides, the TV reveals.** Control lives on one surface, drama on the other, and
   neither runs ahead of the other.
3. **Locked odds make the read the game.** Prices never move after slate generation, so the
   interface's job is comparison and judgment, not chasing.
4. **Cadence is the dopamine.** Small resolutions nested inside the long sweat arc are what keep
   a run alive; the interface must make resolution legible the instant it happens.
5. **Density earns trust.** A slate is a decision surface. Promotional rails, acquisition art,
   and manufactured urgency are anti-features.

## Accessibility & Inclusion

The laptop is read in perspective at reduced scale, so legibility is a functional requirement,
not a preference. Critical values — bank, matchup, selected price, leg count, stake, payout,
primary action, and any disabled reason — must survive a 50% thumbnail check. Normal text holds
at least 4.5:1 contrast. Status is never communicated by color alone. Cursor targets are at
least 44×32px with no two targets closer than 8px. Avoid hairline strokes and low-opacity
essentials, which disintegrate on the angled surface.
