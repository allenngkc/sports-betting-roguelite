# SureThing brand book

## Promise

**SureThing. The number never lies.** A fast, skeptical betting desk for a rigged-looking roguelite world: it tells the player exactly what is selected, what it costs, and what will happen next.

Personality: incisive, nocturnal, dry, orderly. Anti-personality: celebratory gambling hype, fake urgency, casino glitter, buddy-bro language, ambiguity.

## Voice and satire

Transactional copy is literal: “2 selections”, “Potential payout $84”, “Add a selection to unlock.” Satire may live in labels such as “THE HOUSE HAS NOTES” or “numbers pending their inevitable betrayal,” but never replaces an odds label, state, price, disabled reason, or consequence. Never imply a guaranteed win.

## Tokens

| Token | Hex | Use |
|---|---:|---|
| Ink | `#0B0814` | canvas/background |
| Surface | `#161123` | cards/drawer |
| Raised | `#231A35` | controls |
| Violet | `#9B5CF6` | brand, selected, primary action |
| Violet light | `#D8C3FF` | selected outline/focus |
| Text | `#F4F0FB` | primary text |
| Muted | `#AAA2B8` | supporting text |
| Gold | `#F3C969` | money/payout only |
| Good | `#5AD59A` | revealed won only |
| Bad | `#F16D7A` | revealed lost/error only |
| Line | `#403452` | dividers |

Violet is never the only selection cue: selected controls use violet fill, 2px light-violet border (at least 3:1 against its adjacent surface), and a `✓ SELECTED` label. Ink `#0B0814` is required on Violet `#9B5CF6` (4.96:1); never use Text on Violet for normal text. Gold/green/red retain semantic money/state meaning.

## Type, density, distance

Implement with Unity `LegacyRuntime.ttf`; use all-caps only for short labels. At the 1024×704 artboard: display 26px, section 16px, row primary 15px, price 16px bold, body 12–13px. Selected labels, market navigation, and disabled reasons are at least 13px; critical numbers/labels never use 11px. Baseline grid: 8px; component padding 12/16px; card gap 10px. Cursor targets: minimum 44×32px, preferred 48×40px; never place two targets closer than 8px.

At the 50% thumbnail check, the bank, matchup, selected odds, leg count, stake, payout, action, and disabled reason remain readable. Favor high contrast (minimum 4.5:1 for normal text), no color-only status, and a 2px focus ring. Avoid thin strokes and low-opacity essentials on the perspective-mounted laptop.

## Component laws

- Chrome keeps run/bank context fixed; page context sits below it.
- Lobby cards carry teams, records, primary price rows, and one clear details entry.
- Event tabs alter only the market body; matchup header and slip persist.
- Slip always shows leg count, combined odds, stake, potential payout, and the next action/reason. The expanded lobby slip exposes target-sized 10% / 25% / 50% / MAX and −$10 / +$10 controls; compact detail states may collapse those controls to a labeled stake summary without losing the value or selections.
- Proposed approval policy: PLACE TICKET stages a valid working slip; LOCK IT IN commits only with staged ticket(s) and an empty working slip. SKIP ROUND is a separate two-step confirmation.
- Disabled primary action says both cause and corrective action in place.
- MY BETS contains ticket identity, stake, potential payout, leg labels, and only `RevealedView` states. No inferred clock, score, probability, or next event.

## Originality guardrails

Do not use operator marks, proprietary copy, iconography, screenshots, characteristic color systems, or exact layouts. Borrow only task-level principles: density, hierarchy, disclosure, persistence. The violet ledger, ticket language, and cynical microcopy are SureThing originals.
