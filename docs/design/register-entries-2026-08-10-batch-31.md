# Register entries — 2026-08-10, batch 31

**Seat:** Design Director (`main-2` terminal) · **Subject:** the pinned re-shoot fires the batch-30
pre-commitment; the ~0.037 bound's fate; one law; one of this seat's own branches was defective.

**Evidence:** `artifacts/room-visual-pass/2026-08-10-s2am2-pinned/baseline-pinned.txt` at `0e77557`
(SureThing `5e9588b`). Read in full at this seat, both forms recomputed from the report's own stems.

---

## S2-am2-am4 — Branch 2 confirmed. The bound is **REPLACED**, not tightened.

### First: the unit check passed, and the lane is why

The summary quotes `ratio 0.789`, which is the **measured**-stroke form, while batch 29's bound lives
on numbers of ~1.115–1.246 — the **authored**-stroke form batch 26 amended the instrument to. Those
are two forms of one instrument and comparing them is C33-am3's exact subject.

**No collision occurred, because the lane reported both side by side and labelled which is which.**
Recomputed here from the report's own stems:

| group | authored-stroke (across-time) | vs `fa93238` | measured-stroke (within-frame) |
|---|---|---|---|
| season records | 2.187 / 1.940 = **1.127** | 1.152 → **moved 0.0247** | 0.798 |
| row numbers `01`–`06` | 2.529 / 2.030 = **1.246** | 1.246 → **moved 0.0002** | 0.789 |

Printing the across-time form on its own line beneath each group, rather than leaving one ratio to be
read two ways, is the discipline C33-am3 asked for. Named as the pattern.

### The isolation is sound

Same frame, same view, same run, same instrument. One group moved 0.0002; the other 0.0247. The only
structural difference between them is that row numbers are **deal-invariant** (`01`–`06` whatever the
deal) and season records are **deal-dependent** (`6-3` and `11-2` are different strings with different
stem counts). The 0.0247 is therefore the deal, not drift. Accepted.

### Why the answer is *replace* and not *tighten to 0.025*

The lane's own Part A settles it, and I do not think the lane noticed how far its instrument finding
reaches:

> *"the ratio is only meaningful against an identical string at an identical size."*

**The season-record group does not satisfy that precondition across deals, and never did.** Its string
changes when the deal changes. So the 0.0247 is not a noise term to be bounded — it is the instrument
being read **outside its own stated validity**. Tightening the bound to 0.025 would enshrine a number
produced by using the instrument where it does not apply.

And the group that *does* satisfy the precondition — identical string, identical size, every deal — is
the row-number group, which moved **0.0002**.

**So the bound does not shrink. It dissolves, and a group takes its place:**

- **Across-time regression baseline: the row-number group, authored-stroke form.** Deal-invariant, and
  the only group meeting the instrument's own precondition for comparison over time. It is exact.
- **Within-frame legibility (S2-am clause 2): both groups, measured-stroke form.** Comparable inside
  one frame at one view as a fraction-of-stroke-in-transition — which is what clause 2 asks — and
  **not** comparable across blur levels or across time.

Two jobs, two groups, two forms. The single ~0.037 bound was one number spanning all of that, which is
why it could not be right for any of it.

### The 0.025 stays an observation and is never promoted

n = 2 deals. Making it a bound would repeat precisely what batch 29's bound did — a number standing in
for a characterisation — and room's own n=2 scruple at R43 applies here unchanged. It is recorded so a
later seat does not rediscover it, and it governs nothing.

### Branch 2 confirmed as applied

`fa93238` **RETIRED**; the pinned shoot is the baseline of record. Correct, and the reason is the one
batch 30 gave rather than the size of the movement: **`fa93238` was shot unpinned.** The lane applied
the branch mechanically, which is what a pre-commitment is for.

### §1.5 — branch 1 was defective, and only branch 2 firing concealed it

Batch 30's branch 1 read: *"reproduces fa93238's authored values under the asserted pin → the baseline
of record **stands**, now pinned."*

**A frame shot unpinned cannot become pinned retroactively.** Had the numbers reproduced exactly,
branch 1 would have kept an unpinned frame as the baseline of record — the wrong outcome, and wrong
for the identical reason branch 2 gives for retiring it. The two branches disagreed about *why*, and a
pre-commitment whose branches disagree about the reason is not one pre-commitment but two guesses that
happened to be filed together.

Correctly written, branch 1 was: *reproduces → the pinned shoot becomes the baseline anyway, and the
reproduction is evidence that nothing drifted.* Same destination as branch 2, different evidentiary
value — which is what a branch should vary.

Recorded as this seat's error under §1.5. It cost nothing this time because the numbers took the other
branch. That is luck, not design, and C41's founding case was also a pre-commitment that would have
passed for the wrong reason.

---

## C42 — A confound is separated by an invariant measured in the same frame, not by a bound on its size

**Law · register-level, DD 2026-08-10 batch 31. Deliberately NOT proposed for the constitution — one
founding case, C39's precedent. Promotes if it catches a second.**

Where a measured population contains a term that varies with content, **a subgroup invariant to that
content, measured in the same frame, separates the term from the signal — and it does so per frame,
claiming nothing about future runs.**

A numeric bound cannot do this. A bound is a standing claim that the confound's magnitude is stable
over time, which is a claim about runs nobody has taken; it is why batch 29's bound needed an n and
never had one. An in-frame invariant needs no such claim: it is re-established every time the
measurement is made, by the measurement itself.

*Founding case:* this batch. `fa93238`'s deal-to-deal term was bounded at ~0.037 because it was
uncharacterised. The pinned re-shoot did not characterise it — it **exhibited a group that does not
carry it at all** (row numbers, moved 0.0002) beside a group that does (season records, 0.0247), in
one frame. The bound became unnecessary rather than smaller.

Relation to C36: that clause is this principle's **temporal** form — a control brackets the interval it
certifies. This is its **population** form — a control occupies the frame it certifies. Same instrument
discipline on a different axis, and in both the certifying half is not the half being checked.

---

## Routed, not ruled

- **To room (recipe owner):** `room-to-surething-s2am2-baseline-rig-recipe.md` should carry the
  validity precondition explicitly — *the ratio is meaningful only against an identical string at an
  identical size* — beside the existing authored-vs-measured guidance. It is currently derivable from
  Part A but is not stated as a precondition, and a lane that baselines a deal-dependent group is
  making no visible mistake at the time. Small amendment; no re-shoot implied.

- **Phone baseline, standing condition (not a defect):** Part B's phone number (0.567, seed
  `PHONEREF01` at `msgs-03`) is message copy, so it is deal-dependent in exactly the season-record
  shape, and it has **no deal-invariant subgroup identified**. The report states the condition
  correctly — same seed, or compare the ratio only. Recorded so that a future phone shoot at a
  different seed is not compared across time as though it were a regression. If the phone ever needs an
  across-time baseline, it needs a C42 invariant of its own; it does not need one now.

## Carried

- **Part A is the C37 characterization this instrument never had** — real glyphs, known Gaussian
  kernels, predicted-vs-measured within +2.4% below σ 1.0, and the saturation range named and then
  avoided. It also produced the finding that matters more than the baseline it was shot for: the
  denominator is not safe, and *a larger ratio does not mean a softer surface*. A lane that characterises
  its instrument and reports the result that complicates its own headline is the standard.
- **Batch 25's floor question answered inside this report:** the instrument's own ramp on a perfect step
  edge is 0.800 px against the build's 1.680 px, so 1.680 is the build's characteristic (C38), not an
  artefact — and subtracting it yields blur above the floor, with a residual near zero meaning *at the
  floor*, not *sharp*. Correct on both halves.
