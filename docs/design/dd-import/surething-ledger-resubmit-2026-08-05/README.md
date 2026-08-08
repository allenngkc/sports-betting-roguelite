# LEDGER re-submit — the same state set, re-shot after S41 and the main merge

**From:** SureThing UI lead · 2026-08-05 · HEAD `f05332c`
**For:** the Design-verified grant withheld on LEDGER, to be made on this set.

The grant's closing condition was S38, S39, S40, S41, S34 and S37's live instance. **All are landed.**
S41 was the last, and it needed engine retention, which reached this tree with the main merge.

## What S41 changed, and how to check it without trusting me

On `12-ledger-populated-multi`:

- **`TICKET 1.0`, CASHED OUT — RETURNED prints `$8`, in wax.** It was an em dash in `--toner-3`.
  The figure is paired with its wax terminal word exactly as WON is.
- `TICKET 1.1`, WON — `$29`, wax. `TICKET 1.2`, LOST — `$0` in `--toner-3` under an oxide strike that
  crosses the *word* only.
- **The margin's RETURNED total reads `$37`, and never an em dash.** 8 + 29 + 0 = 37. Under S36 a
  single cashed-out ticket blanked that row; the unknown is gone, so the sum is a sum.

The one case retention does not cover is built as ruled: if a record's amount is ever genuinely
unknowable, the total prints the **known** sum and the absence stays in that record's own cell.

**Also consumed — the defect retention was approved for.** The ledger read `run.Tickets`, which
`ExitShop` clears every round, so a player who bet in rounds 1–3 and opened the LEDGER in round 4 met
an empty screen captioned `SETTLED TICKETS · THIS RUN`. It reads the retained history now, unioned
with the current round so a ticket that goes terminal mid-round (a cash-out, or a dead-leg loss under
S43) does not vanish until the round settles.

## The set: 16 states, flat and through the room camera

The twelve the grant was withheld on, plus four that did not exist then:

- `02b-entry-players-scrolling-rail` and `09-margin-max-legs-staged-receipt` — the markets seat's,
  arriving with the merge.
- `13-verdict-run-won` and `14-verdict-run-lost` — the run-verdict screen, whose ground is still
  open with you (`surething-verdict-ground-2026-08-04`).

## What this set does not cover (C25)

1. **No state exercises retention across rounds.** Every ledger frame here is ROUND 1. The figure,
   the colour and the total are proven; that the ledger now carries *earlier rounds'* tickets is
   proven by construction and by the suite, not by any photograph. No capture drives a run past
   `ExitShop` with settled tickets behind it. **That capture does not exist and is worth one** — say
   so and it gets built.
2. **Two states share the number 09.** `09-rewards-affordable` and `09-margin-max-legs-staged-receipt`.
   Nothing collides on disk, but if you read the set in order, the numbering lies. The second is the
   markets seat's test, so renumbering wants their nod rather than a unilateral edit here.
3. The verdict screen's ground is unresolved and deliberately untouched — see the separate drop.

Suites at this commit: **EditMode 76/76, PlayMode 55/55** (the post-merge baseline; it was 76/47 on
this branch and 75/47 on main, and the union is 55 because 8 tests were unique to each side).
