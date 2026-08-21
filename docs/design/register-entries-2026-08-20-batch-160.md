# Register entries — batch 160 (2026-08-20)

**The three `G1` collisions are resolved on a citation map — and the map found a FOURTH that batch
158's count missed, because the instrument that counted them required a digit.** Five in total, all
re-keyed, **zero duplicates remaining across 14 distinct `G1` amendment IDs.**

**One row.** **Destination table:** Cross-surface (`C22-am4`).

**A citation map and a `git`-clean diff. No frames, no measurements, no ruling reopened.**

---

## The map, and what it decided

For each collision: where each definition lives, and **which meaning every citation intends.**

| ID | inline defn | row | citations meaning INLINE | citations meaning ROW | outcome |
|---|---|---|---|---|---|
| `G1-am` | **batch 35** | batch 106 | `T85`, `T90` ×2, `G1`'s cell ×5, logs 35 + 61 | `G1-am12`'s row, log 106 | **row → `G1-am15`** |
| `G1-am2` | **batch 38** | batch 125 | `T74`, `G1`'s cell, log 38 | `T130-vf`, `T155`, log 125 | **row → `G1-am12`** |
| `G1-am3` | **batch 39** | batch 137 | `T85`, `T90`, `G1`'s cell ×5, logs 39/40/61 | `G1-am14`'s row, `T152-am`, log 137 | **row → `G1-am13`** |
| `G1-am4` | **batch 40** | batch 138 | `T82`, `T89` ×2, `G1`'s cell, logs 40 + 41 | `T152-am`, log 138 | **row → `G1-am14`** |
| `G1-am5` | **batch 60** | batch 151 | `T89`, `G1`'s cell | *(mine; none)* | **row → `G1-am10`**, batch 158 |

*(Narrative table — six cells. The row is below.)*

**The inline definition is older in all five and more heavily cited in all five**, and its citations
are contemporary and self-referential — the batch 35→38→39→40→60→61 fit-certification thread cites
itself throughout. **`T90` (batch 60) settles any doubt by naming its referent's batch in its own
text:** *"`G1-am3` recorded at batch 39 that three of six authored statements miss the column."*

---

## The row

| C22-am4 | The collisions are FIVE, not four — and the instrument that counted four had the same defect it was counting | **AMENDED and RESOLVED — DD 2026-08-20 batch 160, §1.5, the FOURTH correction to one instrument and the second in three batches.** **`C22-am3` (batch 158) enumerated FOUR `G1` collisions. THERE ARE FIVE. The missing one is the bare **`G1-am`** — inline definition at batch 35, row at batch 106 — and it was missed because the scan that produced that count matched `G1-am(\d+)`, **requiring at least one digit after `am`. A bare `G1-am` was invisible to an instrument written to find exactly this class of defect.** `C22-am`'s sentence, third time: the check asserted a proper subset of the property it was asked about.** **RESOLVED ON A CITATION MAP RATHER THAN A BLANKET RULE, and the map is in this batch. For each of the five: where each definition lives, and which meaning every citation intends, judged by the citing row's own batch and subject. **THE INLINE DEFINITION IS OLDER IN ALL FIVE AND MORE HEAVILY CITED IN ALL FIVE**, and its citations form a contemporary self-referential thread (batch 35 → 38 → 39 → 40 → 60 → 61, the fit-certification chain). `T90` at batch 60 removes any ambiguity by naming its referent's batch in its own text — *"`G1-am3` recorded at batch 39"*. **SO IN EVERY CASE THE INLINE KEEPS THE ID AND THE ROW MOVES.*** **THE RE-KEY: `G1-am` (row, batch 106) → `G1-am15` · `G1-am2` (row, 125) → `G1-am12` · `G1-am3` (row, 137) → `G1-am13` · `G1-am4` (row, 138) → `G1-am14` · `G1-am5` (row, 151) → `G1-am10`, done at batch 158. Applied in each row, in every citation that means the ROW, in the transcription-log entries and in the four source batch files. **The inline definitions and their contemporary citations — `T74`, `T82`, `T85`, `T89`, `T90` and `G1`'s own cell — are UNTOUCHED**, which is the point of doing this on a map instead of a `sed`.** **VERIFIED, NOT ASSERTED: the re-keyed register differs from its predecessor by NOTHING except these IDs (normalising the new keys back reproduces the prior file line for line), the line count is unchanged, and the inline-aware scan now reports **0 duplicates across 14 distinct `G1` amendment IDs**.** **THE NUMBERING IS NOT CHRONOLOGICAL AND IS NOT MEANT TO BE — `am10` is batch 151, `am12` is batch 125. An ID is a handle; `C22.1` re-keys preserve IDENTITY, not order, and the batch cell carries the date. Renumbering for chronology would have churned `am10` and `am11`, both already landed and cited, to buy a property the register does not rely on.** **AND THE CLAUSE `C22-am3` SHOULD HAVE CARRIED: an ID scan must match the ID SHAPE THE REGISTER ACTUALLY USES, which includes a bare stem with no ordinal. `C22-am` said tables plus backlog; `C22-am3` said rows plus inline; **this adds that the pattern itself must not assume a shape the corpus does not guarantee** — and all three failures are one failure, an instrument looking at a subset and reporting on the whole** | batch 160 |

---

## For the orchestrator

- **Nothing is reopened and no ruling changed.** Five identifiers moved; every ruling text is
  byte-identical apart from those identifiers.
- **`REGISTER.md` carries a re-key entry in the transcription log**, under `C22.1`, naming all five
  and the verification.
- **Four source batch files amended** — 106, 125, 137, 138 (151 was done at batch 158).
- **`C22-am3`'s enumeration of *four* is left as written** in batch 158. It records what was found at
  the time; this row records that it was short by one and why. Correcting it in place would erase
  the evidence for the finding.
- **Backlog is 155–160.**

## Limits

- **The map is a judgement about intent, made per citation** from the citing row's batch and subject.
  It is shown in full so it can be checked rather than trusted.
- **Only `G1` was mapped.** Whether other items carry inline/row collisions is unexamined — the
  inline-aware scan exists now and is cheap to point at another prefix, but pointing it is not this
  batch's claim.
