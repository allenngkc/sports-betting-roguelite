# DD follow-up — markets working-margin collision (2026-08-02, `28b63a0`)

MaxLegs=4 is ruled and landed (balance-neutral, G1–G6 byte-identical to baseline).
It closes the overflow — **but a separate collision remains and blocks B1.**

Measured, canvas-local pixels at 4 legs:

| Element    | Extent        | Collision                     |
|------------|---------------|-------------------------------|
| Payout     | -378..-414    |                               |
| LockReason | -400..-420    | overlaps Payout by 14px       |
| Place      | -418..-462    | overlaps LockReason by 2px    |
| Lock       | -426..-478    | overlapped by Place by 36px   |
| Skip       | -488..-522    |                               |

The capture shows the oxide reason line sitting on the $1,124 payout and
"LOCK IT IN" inside the PLACE TICKET button. Structural cause: Place flows down
from the legs while Lock/Skip are bottom-anchored — at 4 legs the flowing
content reaches the fixed band. Nothing escapes the panel (containment holds).

**The call (C16 class):** Place must either flow into a bounded region, or the
action stack must stop being half-anchored. Exact pixel offsets available from
the markets lead. The branch's suite is intentionally red (PlayMode 45/46) so
B1 cannot merge until this is ruled and fixed.

Also for the record: the lead found and fixed vacuous containment tests
(GetWorldCorners against a 0.5f epsilon on a world-space canvas — ~12× the
panel; no layout could ever have failed). The Phase-A offer-container check had
been passing vacuously since it landed; both now measure in canvas-local pixels.
Only the capture caught this — exactly what C17 exists for.
