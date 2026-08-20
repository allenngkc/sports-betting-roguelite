# Register entries — batch 132 (2026-08-19)

**`T144` WAS RULED SEVENTY-ONE BATCHES AGO AND I DID NOT FIND IT — BECAUSE IT IS NOT A ROW.**
`T74-am6` (batch 60) measured the footer collision, measured it **better** than `T144` did, and ruled
it. It is cited eighteen times inside other rows and **has no row of its own**, along with `T74-am3`
and `T74-am5`. `C22` predicted the cost and the cost was paid.

**Three rows.** **Destination table:** TV (`T144-cl`, `T146`, `T147`).

**Spec:** `docs/design/spec-ticket-footer-2026-08-19.md` — **FOR ALLEN; routes to the incoming TV lead
on approval, and is written to be read cold.**

---

## The rows

| T144-cl | `C22.1` — `T74-am6` GOVERNS; `T144` becomes its cross-reference, and it kept one thing worth having | **CORRECTED — DD 2026-08-19 batch 132, §1.5.** **`T144` ruled the footer collision as a new defect. It is not new. `T74-am6` (batch 60) ruled it, and its measurement is BETTER: *"Bank $10,000: RISK 138.4 + PAYS 239.7 = 378.1 against 249.0, over by 129.1. TYPICAL: 124.7 + 145.9 = 270.6, OVER BY 21.6… the fact floor is NOT a tail case: `$1,234` staked paying `$12,340` is a plain 10× parlay, so THE FOOTER COLLIDES AT ORDINARY VALUES."*** **`T144` reported the collision at the ENUMERATED MAXIMUM; `T74-am6` had already reported it at ORDINARY VALUES — which is worse, more urgent, and the half that matters.** **`C22.1`: the earlier ID governs; `T144` folds into `T74-am6` as a cross-reference.** **WHAT `T144` ADDS AND KEEPS: THE FRAME. Batch 60 measured it; batch 131 SAW it — `RISK $25` and the figure drawn on top of each other, in every state including the incumbent `PAYS`, which is what proves the collision is not about the word.** `C11` was satisfied for the first time on this item. **AND THE SECOND THING IT KEEPS: the diagnosis of WHY two green gates cannot see it — each half is swept against the full row on its own, so the pair is never checked.** That mechanism is not in `T74-am6` and it is what the gate needs | batch 132 |
| T146 | `T74-am3`, `T74-am5` and `T74-am6` are cited EIGHTEEN TIMES IN THE TABLES and none of them is a row | **RAISED — a `C22` defect with a measured cost, and the second of this class this week** · DD 2026-08-19 batch 132. **Counted: `T74-am3` 15 mentions (12 in the tables), `T74-am5` 5 (4), `T74-am6` 3 (2). Only `T74` has a row.** Their substance survives in the transcription log and in source comments — **unlike `S103`'s `A2`, nothing here is lost** — but `C22` is explicit: **a ruling exists when it is a row in `REGISTER.md`.** **THE COST IS NOT HYPOTHETICAL AND IT IS THIS BATCH: I swept the TV table for open items at batch 125, read 163 rows, and did not find `T74-am6` — because it is not one. Seventy-one batches later the same defect was re-found from a frame, re-measured worse than it had already been measured, and ruled under a new ID.** **`C22`'s own words are *"not when it is written, not when it is sent, not when a lead has built against it"* — and these three were written, sent, and built against.** **NOT RULED AS A FIX, because transcription is the orchestrator's and this seat does not re-key its predecessors' work unasked: what is ruled is that the three carry LIVE dispositions — `T74-am3`'s separate rows, `T74-am5`'s withdrawal, `T74-am6`'s fact floor — and the footer spec now depends on all three.** **`S103` (`A2`) and this row are one class and it is worth naming as a pattern rather than twice as an incident: THE STUDIO'S AMENDMENTS ARE ITS MOST-CITED AND LEAST-RECORDED RULINGS** | batch 132 |
| T147 | The footer takes separate rows — and the cost is HEIGHT, paid by the six leg rows | **RULED — ALLEN, relayed 2026-08-19; spec'd at `spec-ticket-footer-2026-08-19.md` and FOR HIS APPROVAL** · DD batch 132. **`RISK`/`STAKE` on the first row, `PAYS`/`RETURNED` on the second, each at the full 249.0px inner width. At full width both clear their own enumerated worst case — `PAYS $73,318,376,502` = 239.7 and `RISK $13,639` = 138.4, against 249.0.** **SEPARATE ROWS IS NOT A PREFERENCE BETWEEN TWO ADEQUATE ANSWERS — IT IS THE ONLY COMPOSITION INSIDE THE LOCKED COLUMN THAT CARRIES THE FACT FLOOR.** The cheaper lever is named and refused on the numbers: **dropping the two labels to label scale (`T74-am3`'s own rule — *the status word rides at label scale, never at money scale*) plausibly clears ORDINARY values, over by 21.6px, and does not touch the fact floor, over by 129.1px.** Abbreviation stays refused (`C49`), copy is not reopened (`T24-am`), truncation is barred (`T69`), and the column's outer width is locked (`T46`, `R30`). **BOTH ROWS TAKE THE SAME ANCHOR, LEFT, AND THE RULING NAMES IT RATHER THAN INHERITING A SILENCE: the opposite anchoring was `T74-am5`'s device for making a SHARED gap unauthored, and on separate rows there is no shared gap — the device has no subject, and keeping it would leave a stagger nobody chose.** **THE COST IS HEIGHT AND IT IS THE WHOLE RULING: two 24px rows at the MEASURED 1.25 line-box ratio (`T74-am3`, not the 1.18 the design constants assume) need 60.0px against a 40px footer. `TicketRowHeight` is DERIVED from the footer's height (`TvSweatScreen.cs:1041`), so EVERY PIXEL THE FOOTER GROWS COMES OUT OF THE SIX LEG ROWS AT ONE SIXTH EACH — 40 → 60 costs each leg row 3.33px.** **STATED HERE BECAUSE `T74-am5` IS THE CASE OF IT BEING MISSED — *"this seat ruled `RiskPays` into two rows on WIDTH and re-derived no HEIGHT"* — and `C46-am` requires the fit re-derived in the same breath as the ruling.** **GATED, NOT ASSUMED: the live leg row's compact line, NEED (`T90`) and progress line are re-derived against the reduced row height and REPORTED BEFORE the composition lands; if it does not clear it returns here as a `C16` signed deviation with a named cost and expiry, the way `T74-am3`'s own 3.0px was signed at `T84-am7`.** **`E3` — a live leg row in the same frame as the footer — is the evidence to hold on: the footer is easy to shoot and easy to like, and the cost lands somewhere else on the same screen** | batch 132 · spec §3 |

---

## For the incoming TV lead

**The spec is written to be read cold** — it carries every constant it depends on with its source
line, because the seat that held this thread rotated at 98%.

**Three things are true at once and all three are in the spec:** the composition is ruled, the height
is not solved, and **the height is the reason this exact ruling was withdrawn once before**
(`T74-am5`, batch 59). **§4's gate is not ceremony — it is the specific failure this item has already
had.**

---

## What is NOT in this batch

- **No copy ruling.** `T133-am`'s `PAID` and the `PAY $60` root collision are open and ride on the
  re-shoot this composition needs anyway.
- **No type ruling.** If the height's answer is smaller type on a money fact, that is `§4` and it
  returns here.
- **No re-keying of `T74-am3/5/6`.** Transcription is the orchestrator's; `T146` raises it and stops.
