# Register entries — batch 98 (2026-08-16)

**Written at the DD seat.** Bookkeeping batch: no design ruling is made, amended or reopened
here. One row closes on evidence already in hand; one structural defect is repaired; one blocker
goes to Allen.

---

## 1. THE CANON PAGE WAS LOSING 75 OF 343 ROWS — repaired

Found while carrying out Allen's instruction to move the misfiled stats-phase rows.

**The mechanism.** A GFM table is terminated by a blank line. The register had accumulated blank
lines *between* table rows — 39 of them. Every row after the first blank in a section was
therefore not a table row at all: with no header and no separator above it, it rendered as a
**literal paragraph of pipe-delimited text**.

**Measured with cmark-gfm — the library GitHub renders with — not asserted:**

| | before | after |
|---|---|---|
| rows not rendering as table rows | **75** | **0** |
| — SureThing | 38 of 120 | 0 |
| — TV | 37 of 115 | 0 |
| rows rendered as pipe-prose paragraphs | 40 | 0 |
| rendered rows with ≠ 4 cells | 0 | 0 |

Everything from `S51-cl` onward on the laptop, and the whole late TV run, was off the canon page.
Under C22 the tables ARE the canon and the rendered table is what a lead reads, so this is the
same defect class as batch 97's pipe escapes and roughly fifteen times the size.

**The repair is whitespace only.** Proof, not claim: the multiset of non-blank lines is
**byte-identical** before and after. No ruling text was touched.

**Why it matters beyond the fix:** this is the second render-level defect in two days, and both
were invisible to every reader because a broken table still *looks* like a table until you count.
The seating routine now runs a render audit, not just a cell-count scan.

## 2. THE STATS PHASE MOVED TO THE TV TABLE — `T99`–`T106`, thirteen rows

`T99`, `T99-cl`, `T100`, `T100-am2`, `T106`, `T102`, `T102-am`, `T100-am3`, `T103`, `T101`,
`T104`, `T104-am`, `T105` were transcribed into the **SureThing** table and are moved to **TV**,
appended after `T98` in their authored order.

**Bookkeeping under C22.1 — every ID is unchanged**, on the `C43` precedent (moved TV →
Cross-surface at batch 34). Not a re-ruling; not a re-opening.

The cost of the misfiling was concrete: the TV table ended at `T98`, so **a TV lead reading their
own table found nothing about the panel that had just shipped** — while the rows sat in a section
they had no reason to open. Section counts: SureThing 120 → 107, TV 115 → 128.

`R38` stays in SureThing and is correct there — it is a SureThing item with an `R` prefix, already
recorded under §1.5 as a mis-*prefix*, not a mis-placement.

## 3. `C26` — OWNING DOCUMENTS OWED — CLOSED

Closed against the documents themselves. All four exist and carry Allen's approval in their own
headers: `room-design.md` (2026-07-31, R13), `surething-design.md` (2026-08-06, C26-am),
`tv-design.md` (2026-08-07, C26-am2), `phone-design.md` (2026-08-09, C26-am3).

What the row owed was the documents, and that debt is paid. Its sequencing was advice about WHEN
to write them, not a further condition — **and it was not followed: TV's document was approved
with `T41` still open.** The document is good anyway; recorded openly rather than re-argued,
because a sequence overtaken by events should be retired in daylight.

Verified by reading the four headers at the seat — not off the row, and not off the constitution's
§1.1 table, which was itself found stale twice in one day on 2026-08-09.

**The row stood open for a week after its last condition was met, because nothing re-read it.**
That is `C51`'s shape landing on the row whose entire subject is what the studio owes itself.

---

## 4. TO ALLEN — `C51` IS A GENUINE ID COLLISION, AND THE MOVE IS HELD ON IT

> **ANSWERED at batch 99, same day: `C52` taken, as recommended.** Authorised as bookkeeping on
> C22.1's earlier-ID-governs precedent and logged to Allen. Batch 64's law keeps `C51`; batch 97's
> law is now `C52` and sits in the Cross-surface table. The section below is kept as the record of
> the finding and of why the move was held.

**Two different laws are both numbered `C51`:**

| | |
|---|---|
| `C51` (batch 64, Cross-surface table) | *A cross-element invariant is an ASSERTION or it does not exist* |
| `C51` (batch 97, filed into the SureThing table) | *A disclosure block that is not re-verified per dock becomes a false claim* |

The C-series runs 1–51 with **no gaps**, so batch 97's promotion took a number that was already
four weeks old and in force.

**This is why the second half of Allen's instruction is not executed.** Filing batch 97's law into
Cross-surface as ordered would put two rows carrying the same ID into one table — a visibly broken
state, and worse than the misfiling it would cure. The row is therefore **left where it is,
flagged, and not moved.**

**RECOMMENDED — batch 97's law renumbers to `C52`; the batch-64 `C51` keeps the ID.** C22.1's
principle is that the earlier ID governs, and the earlier law here is also the one already cited
elsewhere. But a law's ID is what leads cite, so **this is Allen's word to give, not a bookkeeping
call this seat should take on its own** — which is exactly the distinction C22.1 draws.

The renumber is cheap: batch 97's `C51` is cited in its own entry file, in this register row, and
in `register-sweep-2026-08-16.md`. Nothing has been built against it.

**Until Allen rules, `C51` is ambiguous in citation** — any lead citing it today means one of two
laws, and the register cannot say which.
