# SureThing — the laptop sportsbook

A click-through recreation of the approved **Annotated Form Guide** direction: the fictional NOTEBOOK
OS chrome plus the SureThing sportsbook app, at the exact runtime canvas of **1024 × 704**.

Source of truth: `surething/direction-concepts/DESIGN.md`, `element-kit.html` and
`direction-1-form-guide.html` in the inherited corpus, with content and states from
`SHARED-SPEC.md`. Nothing here is a new design.

## Files

| File | What |
|---|---|
| `index.html` | The kit. Loads the design-system bundle, then the screens. |
| `app.jsx` | Persistent chrome, run state, routing between surfaces. |
| `screens.jsx` | FORM, ENTRY, MY BETS, REWARDS, LEDGER bodies (the left 700px). |
| `margin.jsx` | The 324px player margin — working and passive variants. |
| `data.js` | The Round 3 slate, event-detail markets, shop offers, settled ledger. |
| `betmath.js` | American-odds helpers, so figures move truthfully when you click. |

## What is interactive

- **Circle a price** on FORM. The other side of that matchup becomes a *replacement* control, never a
  disabled one — one selection per matchup, and picking the other side swaps it.
- **RUB OUT** removes a leg and recalculates combined odds and payout immediately.
- **Quick fractions and nudge keys** move the stake; stake and payout are always read together.
- **PLACE TICKET** stages the slip as a numbered receipt and clears the marks. Up to 3 per round.
- **LOCK IT IN** is dead until at least one ticket is staged and the working slip is empty, and it
  states cause *and* remedy in place. Locking freezes the board and routes to MY BETS.
- **MORE ›** opens ENTRY. Switching destination (Goals / BTTS / Corners / Cards / Players) replaces
  only the market body; the matchup header and the margin persist.
- **MY BETS** mirrors a simulated broadcast reveal. In the real product this reads
  `TvSweatScreen.RevealedView` only — never engine state, and never ahead of the TV.
- **LEDGER** in the tray opens the read-only settled record.
- **SKIP ROUND** takes two presses.

## What is deliberately not here

- No score, clock, win probability or unrevealed result on MY BETS. The TV owns those.
- No rounded cards, floating betslip drawer, promo rail or accent-on-navy shell.
- No disabled odds control. v0 has no limiting, so no market can honestly be unavailable.
- No date on a staged receipt, no invented settlement, no fabricated error remedy.

## Known divergences from the shipped build

The current Unity implementation still ships the superseded violet package, a 660px board with a
right-hand slip, sub-13px product text, `LegacyRuntime.ttf`, BTTS nested under Goals, lock-with-
working-slip behaviour and no separate skip confirmation. This kit renders the **contract**, not the
current build.
