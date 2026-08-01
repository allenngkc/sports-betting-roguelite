# S9 — defect list from pixel audit

**Audited 2026-07-31** against captures `20260731-134050-061-*` and `20260731-134054-508-*`.
Rewards and the ledger had never been looked at before this pass; both were built without anyone
seeing them render.

These need no ruling. Every one is a build defect against a law or spec already closed.

## Rewards (`07-rewards`)

**1. Offer prices render in oxide red.** `5 COMPS`, `6 COMPS`, `2.5 COMPS` are all `MoneyBad`.
A price is not the house's mark — Law One forbids oxide as a general cost or "bad" tint. Note the
distinction that makes this subtle: the blocked reason beside it, `NEED 5 COMPS`, *is* legitimately
oxide, because a blocked action is the house acting. The price beside it is not. Prices belong in
wax (money) or toner. Fixing this by recolouring both would be wrong in the other direction.

**2. `LEAVE — NEXT ROUND` is a saturated blue.** It is the primary action on the screen and the
loudest element on it, rendered in biro — the player's ink. Law Two: wax is money and the primary
action, biro is anything *he* chose. A phase-advancing button is neither his mark nor optional.
Should be wax, matching `PLACE TICKET`.

**3. `1 COMPS`.** Number agreement. Same class as the `1 SELECTIONS` already fixed — reuse
`Pluralize`.

**4. Offer body copy truncates mid-sentence.** "the whole payment is DEFERRED - your bank is
untouched, and the" simply stops. Same class as the leg lines already fixed: either shorten the
composed string or truncate deliberately with an ellipsis. These are rules text — the player is
being asked to spend on them, so a sentence that stops mid-clause is a product defect, not a
cosmetic one.

**5. The offer list overruns the tray.** `MULLIGAN SLIP` is cut by the taskbar at the bottom of the
screen. The list neither scrolls in its own panel nor paginates. The shared spec permits a long
market list to scroll inside its own panel; nothing may simply run off the sheet.

**6. A banner draws over the offer rows.** "REWARDS IS OPEN — spend your comps before the next
payment" renders across the middle of the list rather than in its own space, in biro, over content.
Same defect class as the `LockReason` occlusion: a message drawn on top of a row instead of beside
it.

## Ledger (`06-ledger`, and `08-old-slips` — the same screen by two routes)

**7. `READ ONLY` appears four times.** Header line, sub-caption, right margin, and footer. Plus
`CURRENT RUN · SETTLED TICKETS ONLY · READ ONLY` and `SETTLED CURRENT-RUN RECORDS · READ ONLY` say
the same thing 64px apart. The honesty is right and worth keeping — this screen is deliberately
careful about not implying cross-run history — but it should be said once, well.

**8. Column heads are orphaned.** `TICKET / STATE / STAKE / PAYOUT` sit above a caveat line rather
than above the rows they head, so the header row and the prose are interleaved.

## Not defects

The empty state reads well and is honest: "NO SETTLED TICKETS IN THE CURRENT RUN" with the two
supporting lines is exactly the right register, and `KNOWN WIN PAYOUTS $0` is correctly in wax.
The chrome is identical to the sportsbook's, which is the shared `NotebookChrome` working.

## Sequencing note

Do **not** start these until the typography wiring has landed and been verified. Every one of these
screens will re-flow when the real faces are assigned — several are text-fit defects, and fixing a
fit against the fallback face means measuring against type that is about to be replaced.
