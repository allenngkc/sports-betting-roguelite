# Register entries — batch 172 (2026-08-22)

**The two console-spec corrections, made — and the second one is a defect in how the spec was
written rather than in what it ruled: it quoted its own illustration back as an evidence criterion.**

**One row.** **Destination table:** Console (`K20`).

**Both claims verified at source at this seat before the spec was touched. Neither correction
changes a ruling.**

---

## The row

| K20 | Two measured corrections to the console spec — one number one low, and one criterion that could never be met because it quoted an ILLUSTRATION | **CORRECTED — DD 2026-08-22 batch 172, on the markets lane's measurements of the built surface, **each re-verified at source here before an Allen-approved spec was edited.*** **(1) §3's LEADER-DOT COUNT WAS ONE LOW — 15, and the surface prints 16. THE CAUSE IS TWO GAP CONVENTIONS FOR ONE DEVICE: `RowGeometry.OfferRow` computes `dots = Field - head.Length - 1`, reserving **ONE** space, while `Page.Leadered` computes `Width - indent - left.Length - 1 - 1 - right.Length`, reserving **TWO**. So an offer row gets one dot more than a contents line for the same content. **THE RULING IS UNTOUCHED — 80 still clears and 62 still fails by one character** — and §6.4's `S100` guard clause is STRENGTHENED rather than weakened: the worst constructible row sits one dot FURTHER from the minimum-run guard than the spec claimed.** **(2) §14's `B4` COULD NOT BE MET AS WRITTEN, and this is the one worth the row. It required *"the folio reading `66–83 of 84`"*. **`Page.BodyRows` is `Height - ChromeRows` = 24 − 4 = 20**, so a first page is ALWAYS 20 offers and PLAYERS paginates `1–20` / `21–40` / … / `81–84`. **`66–83 of 84` cannot occur at any geometry this spec ruled.*** **WHERE THE NUMBER CAME FROM IS THE FINDING: it is §5's own ILLUSTRATIVE example of the folio's FORM, copied forward into the evidence table as a criterion. **THE SPEC QUOTED ITSELF AND TURNED A TEACHING NUMBER INTO A GATE** — and a gate that cannot pass is worse than no gate, because a lane meeting the actual requirement reports a failure against it.** **CORRECTED BOTH WAYS SO IT CANNOT RECUR: `B4` now reads *the folio carrying its OWN range and `[N]ext`, and its next page* — the property, not a value — and **§5's example is marked as an illustration in place**, because a reader at §5 had no way to know it was one.** **THE GENERAL CLAUSE, and it is why this is a row rather than two edits: **AN EXAMPLE IN A PROSE SECTION IS NOT A SPECIFICATION.** This register has ruled the neighbouring shapes — `C58-am` (a docstring is not a measurement), `T154` (a stated caveat is not a check), `P9` (a comment about a file is not the file) — and this is the same family: **a number printed to show the SHAPE of a thing, read later as the VALUE of it.** `B4`'s pin was met throughout; only the criterion was wrong.** **NEITHER CORRECTION TOUCHES A RULING, NO EVIDENCE IS INVALIDATED, AND `B4` IS SATISFIED. The spec's amendments are marked in place with their cause and their date** | batch 172 |

---

## For the orchestrator

- **`spec-console-surfaces-2026-08-19.md` amended in four places** — §3's table row, §6.4's guard
  clause, §14's `B4` criterion, and §5's example marked as illustrative. Each carries a dated
  `CORRECTED` note naming its cause.
- **Nothing markets built is invalidated.** Both corrections move the spec toward the surface, not
  the surface toward the spec.
- **Still open from the console phase:** the over-80 docket (batch 166 / 171), `B9`'s colour capture
  (needs a human at a real terminal — not self-shootable), and the neither-branch variants from
  batch 171 §5.
- **Backlog is 172.**

## Limits

- **`BodyRows` and both gap computations were read at source here**; the 422-line docket figure and
  the 16-dot observation are markets' and were re-derived, not re-measured.
- **No frame was read.** Both corrections are arithmetic over constants the spec itself ruled.
- **The `S100` guard was not re-tested** — the correction says it is further from firing, which is a
  weaker claim than that it never fires, and the lane's width sweep is what asserts the latter.
