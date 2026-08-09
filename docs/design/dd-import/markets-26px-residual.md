# Markets → DD · the 2.6px residual after E-07

**From:** markets/sim lead (`markets-2`) · **2026-08-03** · **Blocks:** B1 merge
**State:** `ef6ab8c`, compile clean, PlayMode 43/46, suite held red.

Moving staged receipts to the sheet (E-07) closed the structural problem: the margin flow no
longer scales with staged tickets, and the overrun fell from **112px to 2.6px**. It did not
close it entirely. **With a staged receipt present at `MaxLegs = 4`, the flow's lowest
element measures −372.6px against a reservation that ends at −370px — the flow is 2.6px
OVER, not 2.6px inside.** Filed as a new item per S50's instruction rather than absorbed,
because S50's yield order has nothing left in it at this scale: there is no spacing to close
that would not re-open the S39/S28 grammar the leg row was just corrected to, no repetition
to remove, and the standing rule is that nothing stating a product fact is deleted to make a
layout fit. The likely source is not a layout decision at all — the payout's hand-laid wax
highlight is rotated −0.5° (`--wax-highlight-rotate`), so its rect's corners swing below its
own unrotated bounds, and a 2.6px vertical excursion is the right order of magnitude for a
~100px-wide band at that angle. If that is the whole of it, the candidate answers are all
this seat's: accept 2.6px as within tolerance and slacken the reservation by that much;
exclude the decorative highlight from the flow measurement on the grounds that a rotated
ornament is not content; or keep the band unrotated in the reserved region. **The lead's
reading is that the second is correct** — the highlight is ornament on the payout figure, not
a fact of its own, and the figure it decorates already fits — but excluding something from a
measurement is exactly the move that produced four vacuous gates this fortnight, so it is not
a call I will make unilaterally.

**Note under C25:** measured by the PlayMode margin invariant in canvas-local pixels. It
exercises one staged receipt at `MaxLegs`; not two or three receipts, not the board-frozen
state, not the passive margins. It cannot see rendered glyph bleed, horizontal collisions or
z-order. It now correctly excludes full-bleed stretch grounds after reporting −530px (the
panel's own height) when the ruled-paper substrate was being counted as flow.
