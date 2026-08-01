# Interaction specification

## First-time flow

1. Open SureThing → **LOBBY**. Persistent chrome shows `R2/5`, BANK, DUE, and COMPS. The slate has no promo rail.
2. Scan matchup cards: away/home names, season W-L, start context, and primary moneyline odds in aligned targets.
3. Select prices from distinct matchups → each becomes violet + outlined + `✓ SELECTED`; the working slip updates leg count/combined odds/stake/payout. A matchup holds one selection only; this is not same-match parlay support.
4. Open **DETAILS** → the same matchup header and working slip persist. GOALS, BTTS, CORNERS, CARDS, and PLAYERS are top-level destinations.
5. Switch destinations → only rows change. Matchup and slip remain unchanged. Selecting a market replaces only that matchup’s existing selection (current `BetslipModel` one-selection-per-matchup law); announce “Replaced Nighthawks ML +135 with Over 9.5 Corners +105.”
6. Remove a leg via its explicit `× REMOVE` control; drawer economics recalculate immediately.
7. Adjust stake with `10% / 25% / 50% / MAX` or `−$10 / +$10`. Combined odds, stake, and potential payout update together on every input.
8. **Proposed UI policy — requires user approval:** a valid working slip enables `PLACE TICKET` and disables `LOCK IT IN` with “PLACE OR CLEAR THIS WORKING SLIP.” Placing creates a staged ticket/receipt and empties the working slip; then `LOCK IT IN` enables. With no staged tickets, `LOCK IT IN` says “PLACE AT LEAST ONE TICKET TO LOCK.” Preserve the engine’s empty-round option as a separate secondary `SKIP ROUND` with a two-step confirmation.
9. During sweat, board controls freeze and default route is **MY BETS**. It mirrors only `TvSweatScreen.RevealedView`: ticket identity, stake, potential payout, leg labels, and revealed per-leg state. The TV remains the only owner of score, clock, odds movement, probability, and unrevealed outcomes.

## State model

`Lobby.Empty → Lobby.WorkingSlip → Ticket.Staged → Round.Locked` preserves `matchupId`, `stake`, and board state. `Detail.Goals|BTTS|Corners|Cards|Players` preserves working slip. `Slip.Empty|Valid` plus `stagedTicketCount` derives the proposed action state. `Board.Open|Frozen|Shop` gates interactions. `MyBets.NoMirror|LiveMirror|ResolvedMirror` renders only `RevealedView` payload; it never queries engine session or RunDirector for live truth.

### Contract boundary

Current `RevealedView` exposes ticket/leg presentation state and also score/clock/probability. This proposed design needs no `RevealedView` expansion: ticket labels, stake, payout, odds, and states are already present. This revamp intentionally **does not render** score/clock/probability in MY BETS. Any future data request requires principal contract approval; do not read engine data directly or alter reveal timing.
