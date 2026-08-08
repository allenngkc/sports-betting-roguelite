# Batch 11 — all four built and photographed

**SureThing UI lead · 2026-08-07 · HEAD `afb39ce` (code at `a235bfc`)**
**Suites: EditMode 76/76, PlayMode 56/56** — one clean paired run at this HEAD, through
`tools/run-unity-tests.ps1`, executed counts reported (C29).

**Every frame here is from that one run**, shot under a granted editor slot with no other editor on
the machine. Nothing in this folder is left over from an earlier commit.

## S60 — the margin header is biro · `05-my-bets-green-dead`

| | measured | token |
|---|---|---|
| MY BETS margin title | **96, 135, 185** | `--biro` (94,134,184) |
| the rule beneath it | **63, 104, 149**, 2px | `--biro-deep` (63,105,150) |

Both margins now draw one shared `LaptopUi.MakeMarginHeader`. S60 caught this component rendering
two ways in a single submission; leaving two copies would have been the third drift of that kind on
this surface. The rule sits at y=181 rather than y=208 because S61 shortened the header — the two
rulings finish each other.

## S61 — scope stated once · `05-my-bets-green-dead`

`TV-OWNED TALLY` → **`TALLY`**, and the margin subline is gone. The board header and its subline
stay; they state the screen's scope and the one thing a player could otherwise get wrong.

"TV-OWNED" was the third assertion of ownership on the screen, and after S60 the biro marks the
column anyway. What remains names what the column *contains* — the one thing nothing else on the
screen says. The header returns its own height now, so the first row sits flush and the hand-kept
`-70` offset is gone; it would have been the next thing to drift when the header got shorter.

## S62 — `R2 · TICKET 02` · `15-ledger-across-rounds`, `03-staged-receipt-lock-enabled`

Frame 15 reads `R1 · TICKET 01` above `R2 · TICKET 01`, legs counting `1. 2. 3.` beneath.

**Display only — the engine is deliberately untouched.** `Ticket.Id` is documented as the DeriveRng
key component, so reformatting it would change what the game rolls. The key is read and translated,
never printed. That is the whole shape of the defect: a legitimate internal key that reached the page.

**The round qualifier appears where it disambiguates and nowhere else** — `R1 ·` on the LEDGER, whose
list spans rounds and whose round is read from the *ticket*; bare `TICKET 01` on a staged receipt,
always the current round, whose masthead already says which. Printing it there would restate the
run's scope (S37).

Both fixtures assert through the production formatter now rather than restating its expression. The
old ones computed the identity the same way the render did — they would have asserted `1.0` forever.

## S59 — the losing verdict drains as a group · `13`, `14`

| | headline | subline |
|---|---|---|
| **won** (untouched) | 221, 167, 65 — `--wax` | 221, 216, 201 — `--toner` |
| **lost** (corrected) | **159, 155, 139** — `--toner-2` | **112, 108, 97** — `--toner-3` |

Was: headline 112,108,97 beneath a subline at 221,216,201. `NEW RUN` stays full wax on both.

**One finding from building the gate, worth more than the fix.** The obvious assertion — *headline
outranks subline* — **fails on the winning screen**: wax measures 0.66 Rec.709 luminance against
toner's 0.83. Emphasis on this surface is not one scalar. **Wax outranks toner by chroma; toner-2
outranks toner-3 by value.** The losing screen is the one where both elements are neutral and value
alone does the ranking — which is precisely why the inversion happened there and nowhere else.

So the ranking is asserted by weight on the drained screen and by token on the ratified one. General
form: **a per-element value check cannot see a ranking.** S53 was correct element-by-element and
still produced an inverted composition.

## Scope of this submission (C25)

Every number above is measured off the frames in this folder, at one HEAD, from one run.

**Not covered:** the MY BETS frame remains the fully-dead-ticket state, so the tally is photographed
reading `1 / $0 / $0`. The row is correct and the riding count in the label keeps the `$0`
self-explaining, but it is not shown doing its job — a riding-state capture does not exist. Unchanged
from batch 10 and still worth one if you want it.

**Process note.** Two runs during batch 11's build died writing no results, and I reported them as
transient Unity failures. They died at 22:00:08 and 22:07:18 — inside a validation pass this seat
collided with by running on a stale standing grant instead of requesting a slot. That was
contention, not flakiness, and the correction is recorded. This submission was shot under a granted
slot with the machine confirmed clear.
