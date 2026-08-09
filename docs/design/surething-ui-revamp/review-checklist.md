# Staff review checklist

| Gate / target-flow step | Evidence |
|---|---|
| Empty slip: PLACE disabled, LOCK cause/remedy, separate SKIP | `01-lobby-empty.svg` |
| Valid two-leg, multi-market ticket, stake controls, PLACE, and blocked LOCK reason | `02-lobby-selected-slip.svg` |
| Detail has five market destinations and retains ticket | `03-event-detail-goals-btts.svg` |
| Corners replacement alters only Nighthawks leg | `04-event-detail-alt-market.svg` |
| Staged receipt enables LOCK IT IN | `05-ticket-staged-lock-ready.svg` |
| Honest MY BETS, no TV spoilers | `06-my-bets-live-honest.svg` |
| Exact 1024×704 and 50% readability | all SVG viewBoxes; inspect `index.svg` at 50% |
| Original, non-promotional visual language | brand book + all SVGs |

## Acceptance gate

- User confirms the six screens convey the intended hierarchy and tone.
- User approves the proposed staging policy: PLACE TICKET first, LOCK only after staging, separate two-step SKIP ROUND.
- User confirms market selection semantics (one selection per matchup replacement) are acceptable.
- Implementer confirms all critical text sizes and target minimums map to LegacyRuntime UGUI.
- Implementer verifies MY BETS reads only `RevealedView` and omits score/clock/probability.

## Open decisions and risks

1. Current market enum has Goals/Corners/Cards/Players; the proposed concept requires a later BTTS destination extension.
2. PLACE/LOCK staging policy is a SportsbookApp UI recommendation requiring user approval; no implementation is authorized.
3. The 1024×704 canvas at a 0.32×0.22m world surface needs an in-room cursor/readability pass; SVG approval is not a perspective proof.
4. Existing code has a 660px board and right slip; implementation should preserve the persistent economics rule while reconciling current layout constraints.
5. Observed branch risk: checkout is currently `surething-ui`, not the requested `slice/surething-ui-revamp`; implementation must not start until branch intent is resolved.
