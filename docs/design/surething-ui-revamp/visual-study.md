# SureThing visual study — 2026-07-23 America/Los_Angeles

## Evidence and limits — reviewed 2026-07-23, America/Los_Angeles

All four supplied official references were checked: [bet365 App Store](https://apps.apple.com/us/app/bet365-sportsbook-casino/id1465717844), [bet365 Football](https://www.bet365.com/hub/en-gb/football), [FanDuel App Store](https://apps.apple.com/us/app/fanduel-sportsbook-casino/id1413721906), and [FanDuel NFL](https://sportsbook.fanduel.com/navigation/nfl). The FanDuel NFL endpoint did not render inspectable live HTML here, so statements about its pictured NFL surface are direct screenshot observations, not interaction claims. App Store benefit text is marketing evidence only.

## Screenshot-level observations

- **bet365 App Store frames:** persistent search/sport navigation, compact market grids, grouped same-game-parlay/bet-builder selections, and a live tracker are visible. Its listing claims favorites, search, live markets, and in-place parlay construction; that claim is not proof of a particular flow.
- **FanDuel official NFL surface:** the supplied surface depicts two-team event rows with stable odds columns, category tabs, and More Wagers/Stats. Its screenshot treatment shows a selected odds cell, a persistent collapsed betslip, and expanded WAGER/TO WIN economics; the inaccessible endpoint prevents testing those interactions.

## Read

- **Hierarchy:** dense books earn scan speed with a stable sport/league/event/market ladder; approachable books use a calmer event header and obvious primary choices. SureThing needs both, with run/bank first and no acquisition banners.
- **Event anatomy:** team identity, records, start context, then aligned prices make comparison fast. The market name must survive a selection moving into the slip.
- **Progressive disclosure:** the lobby shows primary lines; one event owns its market tabs. Switching tabs must not silently clear a selection or change match context.
- **Feedback/economics:** a picked price needs a persistent visual state and a persistent ticket summary. Stake and potential return should be paired, never hidden behind a confirmation step.
- **Live/My Bets:** the useful idea is a recognizable ticket strip with state; SureThing’s causal rule is stricter: it may only repeat information already revealed by the TV.

| Reference pattern | SureThing decision | Why |
|---|---|---|
| Compact aligned price rows | ADAPT | Faster at 50% scale than promotional cards. |
| Event-detail market tabs | ADAPT | Holds matchup and slip context while exposing depth. |
| Persistent betting economics | ADAPT | Make working slip, staged receipt, stake, payout, and next action legible before commitment. |
| Favorites/search/personalization | DEFER | Not needed for a small authored slate. |
| Auth chrome, boost rails, carousel promos, casino/acquisition art, wasted whitespace | REJECT | Conflicts with the slate-as-decision-surface principle. |
| Real operator logos, wording, or brand cues | REJECT | Original fictional product and no copied assets. |
| Live score/clock/probability panels inferred from engine | REJECT | The TV owns unrevealed drama; MY BETS reads only `RevealedView`. |

The resulting language is deliberately original: a restrained violet ledger, blunt transaction copy, and small satire in noncritical text—not a visual or lexical imitation of either reference.
