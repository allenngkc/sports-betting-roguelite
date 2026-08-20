# Register entries — batch 136 (2026-08-19)

**THE INSTRUMENT WAS RIGHT AND I OVERRODE IT.** The pre-transcription check reported **two collisions
with live register rows**. I found the cause, wrote *"two apparent hits are narrative tables, not
rows"*, and cleared it. **The cause was the finding.**

**One row.** **Destination table:** Cross-surface (`C22-am2`).

---

## The row

| C22-am2 | An anomaly EXPLAINED is not an anomaly CLEARED — and `C22-am`'s "zero collisions" is false in canon | **CORRECTED and RULED — DD 2026-08-19 batch 136, §1.5.** **FIRST, THE FACTUAL CORRECTION, because it is now in the tables: `C22-am` states *"THE CHECK HAS NOW BEEN RUN PROPERLY — 66 rows, 66 distinct IDs, ZERO duplicates, ZERO COLLISIONS WITH LIVE ROWS."* THE LAST CLAUSE IS FALSE. The check reported TWO collisions with live register rows — `T112` and `G1`, both real rows (batches 104 and 17) — and batch 135 recorded them as *"two apparent hits are narrative tables, not rows"* and moved on.** **THE DIAGNOSIS WAS CORRECT AND THE CONCLUSION WAS BACKWARDS.** They ARE narrative rows: batch 125's owed-ledger table is four columns wide, so its `T112` and `G1` entries parse as register rows. **That fact is not a reason to dismiss the reading — IT IS THE PROOF THAT THE EXTRACTOR INCLUDES NARRATIVE ROWS AND WILL THEREFORE TRANSCRIBE THEM.** It did: both landed in the TV table on the transcription pass, and **only the post-transcription duplicate scan — run for a different reason — caught them.** Removed; the remaining 65 verified byte-identical to source. **THE GENERAL FORM, and it is distinct from its neighbours: `C18 §4.2` is a check that cannot see; `C55-am` is a check looking at the wrong thing; THIS IS A CHECK THAT SAW CORRECTLY AND WAS OVERRULED BY ITS READER. WHEN AN INSTRUMENT FLAGS AN ANOMALY AND YOU FIND ITS CAUSE, THE CAUSE IS A FINDING ABOUT THE INSTRUMENT — NEVER A CLEARANCE FOR THE READING. Knowing WHY a check fired is where the question starts, not where it ends.** **AND THE COST IS THE HALF WORTH RECORDING: THE DISMISSAL WAS WRITTEN INTO A BATCH THAT WAS THEN TRANSCRIBED, SO A FALSE CLEARANCE BECAME CANON.** An unexamined explanation is more dangerous than a raw anomaly precisely because it is quotable. **`C22-am`'s substantive ruling — that an ID check must read the tables PLUS the backlog — stands unamended; only its self-report was wrong** | batch 136 |

---

## The pattern, stated once rather than three times

Three assertions of mine about instruments were wrong today and all three were caught by a scan
rather than by me:

| | what I said | what was true |
|---|---|---|
| `C57` | the pool follows what the build can emit | it follows what the **deck** authors (`C57-am`) |
| `C22-am` | the ID check is clean | it was blind to fourteen untranscribed batches |
| **`C22-am2`** | the two collisions are false positives | they were the extractor including narrative rows |

**The common shape is not carelessness about instruments — it is describing what I expected a check
to look at instead of reading what it looked at.** Recorded here as one line rather than relitigated
in three rows, and offered as the thing to watch at this seat rather than a new law.

---

## What is NOT in this batch

- **No change to `C22-am`'s ruling**, which stands; only its self-report is corrected.
- **No new C-number.** The general form is stated inside `C22-am2` and earns promotion only if it
  recurs on a different instrument.
- **No re-audit of the transcription.** The 65 rows are verified byte-identical to source and the
  register scans clean at 469 rows, zero duplicates, zero malformed.
