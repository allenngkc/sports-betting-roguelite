# Register entries — batch 125 (2026-08-19)

**`C57` WAS RULED YESTERDAY'S-BATCH-AGO AND THE INSTRUMENT ALREADY DISAGREED WITH IT — CORRECTLY.**
Working the two unreachable Under cells, I found `TvExtentSweep` sweeps them **on purpose**, with the
reasoning written out. Its rule is better than the one I ruled, and `C57`'s discriminator is wrong in
a way that would have sent someone to strip a correct pool.

**Three rows.** **Destination tables:** Cross-surface (`C57-am`) · TV (`T134`, `G1-am12`).

**Recorded under constitution §1.5 — the seat's own errors are its own.** `T131`, `T132` and `T133`
are unaffected: all three are the case `C57` got right.

---

## The rows

| C57-am | The discriminator is AUTHORSHIP, not emissibility — the pool follows the DECK, never the BUILD | **CORRECTED — DD 2026-08-19 batch 125, §1.5. `C57` stands; its test does not.** **I ruled that *a pool missing a string the code CANNOT YET emit is CORRECT*, and treated "the build can emit it" as the discriminator. `TvExtentSweep`'s `DecisiveBeatLines` pool refutes it, deliberately and in writing:** it carries `ApproachUnder` and `TurnUnder` — **two cells `TheaterChoreographer.ResolveBeat`'s `gateEligible` can never fire, because it requires `countHelps` (Over) and §6 keeps the Under mirror out of scope** — and its own comment gives the reason: ***"a pool that includes a not-yet-reachable string is caught the moment anyone looks at it; a pool that OMITS one is invisible in the frames until the day it becomes reachable and nobody re-swept for it."*** **THAT IS RIGHT AND MY RULE WOULD HAVE CALLED IT A FABRICATED POOL.** **THE REAL DISCRIMINATOR, and the two cases are not the same thing wearing different clothes: (1) AUTHORED-AND-GATED — the string EXISTS IN THE SOURCE DECK and a scope gate keeps it from firing (`SweatFlavor.cs:297-319`). The pool SHOULD hold it: measuring it costs nothing and the day the gate opens the box is already certified. (2) RULED-AND-UNWRITTEN — the string exists in a REGISTER ROW and nowhere in the source (`RETURNED`, bare `CASHED OUT`, the suffix's removal). The pool MUST NOT hold it: that is `T111-am`'s fabrication, and a sweep that passes on a string the surface cannot construct measured nothing.** **SO: THE POOL FOLLOWS THE DECK — what is AUTHORED IN SOURCE — NEVER THE BUILD, which is only what a gate currently lets through.** **`C57`'s three-line test survives and sharpens: in the deck but not the build → the pool SHOULD hold it; ruled but not in the deck → the pool MUST NOT.** **And the lesson underneath is `C11`'s in a new place: I ruled a law about an instrument WITHOUT READING THE INSTRUMENT'S OWN REASONING.** `T111-am` found this sweep's pools fabricated two days ago and I generalised from that finding to a rule the same file already argues against, three hundred lines further down. **The instrument had thought about it more carefully than the law did** | batch 125 |
| T134 | The two unreachable Under cells — `C46` IS discharged; what is owed is `C11`, and it is SCOPE-BLOCKED | **RE-CLASSIFIED; the item is PARKED, not queued** · DD 2026-08-19 batch 125. Batch 120's close carried *"the two unreachable cells"* as an owed item without saying what kind of debt it was. **It is two debts and only one exists.** **`C46` IS DISCHARGED: `ApproachUnder` and `TurnUnder` — and both their fallback rungs — are IN `DecisiveBeatLines` and were measured against the `Flavor` box by the batch-104 sweep.** Their widths are certified; **nothing about them is owed to an instrument, and re-running the sweep for them would re-report a number it already has** (`T131`, `C57`). **WHAT IS OWED IS `C11`: nobody has SEEN them, and nobody can.** `gateEligible` requires `countHelps`, so an Under leg is never classified `Approach`/`Turn` in this build — **the strings are authored, measured, unreachable and unread, which is a legitimate state and not a defect.** Batch 120 said exactly this and the dock said it too; **what was missing was the classification, and without it the item reads as a queueable measurement.** **RULED: the debt is `C11` alone, and it is blocked by `spec-count-theater-2026-08-17.md` §6's scope, not by any lane, instrument or build. It unblocks when — and only when — the UNDER MIRROR comes into scope, at which point it is a capture and not a sweep.** **AND THE AUTHORING WAS RIGHT TO RUN AHEAD OF THE SCOPE:** `strings-owed-2026-08-17.md` §4.2 authored all four cells against the mirror the spec defers rather than against what today's build can reach — **which is what makes `C46` dischargeable now and the gate's opening cheap later.** Recorded so a later reader does not mistake the deliberate overreach for drift | batch 125 |
| G1-am12 | `G1`'s scope grew by NINE MARKETS and nobody re-derived it — and its original blocker is discharged | **RE-SCOPED — DD 2026-08-19 batch 125. `G1` is this seat's own standing debt and its scope statement is now false.** **`G1` (batch 17, 2026-08-08) reads: *"the DD authors the statement form for every market a TV leg can show (ML, Total Goals, BTTS, Total Corners, Total Cards, Anytime Scorer)."* SIX MARKETS — correct on the day, and `MarketKind` has carried FIFTEEN since F_0.5.0. `T130` measures the consequence: the same six are authored and the other NINE fall to a `default`.** **`G1`'s own words are the binding half — *"every market a TV leg can show"* — so the scope was never the list in the bracket; the list was the enumeration OF the scope on the day it was written.** The scope did not change; **its enumeration went stale, and that is `C56` arriving on a register row instead of on a switch.** **THE ORIGINAL BLOCKER IS DISCHARGED IN BOTH HALVES:** `G1` was *"blocked on the market list + width from the TV lane (dispatched)"* — **the market list is `MarketKind`, fifteen members, and the widths exist: `T111-am` measured `LegRowProgress0` at 249.0px and the `LegRowNeed` band is swept by the same instrument.** Neither is outstanding. **THE NEW BLOCKER, NAMED SO THE ITEM IS NOT READ AS RUNNABLE: `T130` is FLAGGED under `C17` and its frame is arm 3 of the window shooting now.** Authoring eighteen forms against a source read is exactly what `C17` exists to stop, and `T96` is the case in point — a copy ruling authored ahead of its evidence still shipped a defect. **NOT AUTHORED HERE, AND THE SIZE IS STATED SO THE ORCHESTRATOR CAN PLAN RATHER THAN DISCOVER: nine markets × two forms (compact + NEED) × `G1`'s two-rung ladder = a deck pass, not a corner of the drawn-ending phase.** **`G1-am`'s scorer re-scope stands and is not reopened** | batch 125 |

---

## The ledger, after four batches of working it

| item | kind of debt | owner | state |
|---|---|---|---|
| `T112` | measurement | — | **CLOSED** (`T112-cl`, batch 124) — the number existed at `TvExtentSweep.cs:484` |
| the `(2 in the spell)` suffix | **build** | TV | queued behind the shoot |
| `T114-am` / `T112-am` cash-out amount | **build** | TV | queued — and the 14.6px overrun ships until it lands (`T132`) |
| `T121` `STAKE`/`RETURNED` | **build** | TV | queued — **must not ship without `T133`'s measurement** |
| the four new strings' `C46` | *not a debt* | — | `RISK`/`STAKE` swept; `RETURNED` unwritten, correctly absent (`C57-am`) |
| the two unreachable cells | **`C11`**, scope-blocked | — | **PARKED** (`T134`) — `C46` already discharged |
| cards never shot | capture, out of §6 scope | — | parked |
| the seated acceptance view | standing studio gap | studio | not this phase's debt |
| `T130` — nine unauthored kinds | **flagged**, `C17` | — | arm 3 of the window |
| `G1` | **DD seat** | me | re-scoped to fifteen; blocked on `T130`'s frame |

**Nothing on this ledger is runnable today that is not already queued.** The sweep stays un-queued
(`C57`), and the three builds are the whole of the actionable list.

---

## What is NOT in this batch

- **No authoring**, and `G1-am12` says why in terms: eighteen forms against a source read is the
  failure `C17` exists to stop and `T96` is the case in point.
- **No sweep re-run**, unchanged from batch 124.
- **`C57` is not withdrawn.** Its three-line test survives and is sharper; only its discriminator was
  wrong, and `T131`/`T132`/`T133` all sit on the half it got right.
