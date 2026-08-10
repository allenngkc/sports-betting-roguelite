# Register entries — batch 29 · 2026-08-10

**Design Director** · the flood-removal verdict. Frames: `tv-flood-removal-2026-08-10.zip`,
converged tree `c6458a0`. Measured at this seat, in linear relative luminance (C33-am3), not taken
from the lane's table.

---

## T68-am + T71 — **GRANTED · CLOSED.** The flood is gone and money stopped moving with it.

The defect was never the ink's colour: the punched ink **tracked the flood**, 0.064 → 0.384 across
the beat, and contrast collapsed to 1.70:1 (accept) and 1.86:1 (win tally) at peak.

**Measured at this seat, independently:**

| | field core | ink core | CR | ink moved f000 → old flood peak |
|---|---|---|---|---|
| accept f000 / f020 | 0.6833 | 0.0372 | **8.41:1** | **0.0000** |
| win tally f000 / f020 | 0.6920 / 0.6973 | 0.0360 | **8.63 / 8.69:1** | **0.0000** |

Before: the same ink moved **0.3200**. **It now moves by nothing at the exact frame the flood used to
peak.** That is the ruling satisfied, not approximated.

**The wash is deleted, not dimmed (C10 / §3.3), confirmed three ways.** Frame median luminance
**0.0105**; only **3.95 %** of the frame sits near field level at the old peak, against 3.98 % at
frame 0 — a surviving wash would move that number, and it does not. The 6×6 difference map is
**localised** on accept (5/36 cells, on the slot). Read at review distance on `accept-frame020`: the
pitch, the panel and the chrome are all dark and **gold appears only inside the CASHED OUT slot** —
which is T40's actual requirement, gold confined to where money is.

**The win-tally beat's wide top-band difference is live match content, not residue.** It read as a
full-width warm lift (R +1.32, G +1.06, B +0.36) and I treated it as a possible surviving wash until
the difference map resolved it into **discrete moving blobs — stadium lights and player markers**. A
wash lifts a field smoothly; this is objects moving 0.4 s apart. Recorded because the colour statistic
alone pointed the wrong way and only the picture settled it (C11, again).

**T40 enforced. T71 rides closed on the same evidence** — both siblings measured, which is the whole
reason they were ruled together.

---

## T68-am-2 — **the punch survived, and it is the finding that needed a frame**

`§6.1`'s L4-then-settle, measured here: accept **0.6833 → 0.5976**, win tally **0.6920 → 0.5912** —
one step, at frame 024, ≈ −15 % on both. The lane's own reads (0.6883 → 0.5847, 0.6927 → 0.5870,
15.1 / 15.2 %, step at frame 21 = `hdrPunchDuration` at the 1/50 s step) reproduce at this seat.

This was pre-committed as the failure most likely and least likely to be volunteered: had the punch
left with the flood, **every contrast number above would look exactly as it does** and the beat would
have lost its punctuation silently. The lane measured it because the pre-commitment asked, and the
disclosure of *why* it mattered came back with it. That is the pre-commitment instrument working as
designed, and it is the second time in three days.

---

## T68-am-3 — **batch 27's "pre-verified fix" was this seat's error (§1.5)**

Batch 27 ruled: *"Frame 0 is the flood-at-alpha-0 state… with the flood gone that is the shipping
value — a pre-verified fix, needing confirmation rather than discovery,"* and fixed 6.47 / 6.58 as the
expected outcome. **This seat then bound its own verdict to that number.** Both were wrong, in the
same way, and the frames say so:

> **An element at zero alpha is not an absent element.**

`_goldFlood` carries `MakeHdrMaterial()`. At alpha 0 it still contributed **0.0269** of ground
luminance — the ground reads 0.0640 with it present-but-invisible and 0.0371 with it deleted. The
numerator never moved (0.6881 vs 0.6877). The lane's arithmetic is checkable and checks:
`(0.6881+0.05)/(0.0640+0.05) = 6.47` and `(0.6877+0.05)/(0.0371+0.05) = 8.47`.

So **6.47 was a floor carrying flood residue, not a target** — the best value obtainable while the
thing under test was still in the tree. Landing *at* the pre-committed band would have meant residue
survived. **The pre-commitment inverted its own test.**

It still earned its place: it is what forced the explanation instead of letting a pleasing number
through, and the correction arrived as checkable arithmetic within the window. The lesson is about the
threshold, not the practice — **a pre-commitment computed from a contaminated frame inherits the
contamination**, and this one should have been expressed as a direction (*ground falls, ink stops
moving*) rather than a value.

**Proposed C41 (constitution §2, awaiting Allen):** *a predicted value derived from a frame that still
contains the element under test is a floor, not a target, and is stated as a direction of travel.*
Sibling to C35-am — both say the layers' authored state is not what reaches the eye. Not canon until
Allen approves; recorded here so the ruling above can be read against it.

---

## Findings carried, none blocking the grant

1. **Two frames named in the manifest did not travel (C12).** The bundle's FRAMES section lists
   `accept-frame028` and `wintally-frame028` — *"held at L3"* — and neither is in the zip (7 PNGs, 9
   named). The step itself is verified f000/f020 → f024, but **"held after, not a drift" is the one
   claim resting on frames I do not have.** Send the two, or restate the claim as measured-not-framed.
2. **`_dimOverlay` is unconfirmed.** Batch 27 explicitly scoped the removal to the two floods and named
   the dim as *not* struck. Nothing in the bundle speaks to it, and it is not frame-verifiable without
   a reference. **A source-side confirmation is owed** — one line, not a window.
3. **The mtime rescope needs its index count.** The disclosed glob error produced *alternating* values
   (8.47 / 1.70 frame by frame). I can report that specific corruption is **absent from what
   travelled** — ink is identical across both traveled frames on both beats, no alternation. That is
   not proof all 30 indices carry the new run's mtime; newest-per-index is silent if the new run wrote
   fewer indices than the old. **Confirm 30/30, not just that the glob was narrowed.**

**The disclosure itself is the right behaviour and is recorded as such** (C25): the lane caught an
instrument error that would have reported the defect still present, said so unprompted, and named the
general trap — any capture directory that accumulates runs, where the filename cannot separate them.

---

## Disposition audit — against `flood-removal-disposition-precommit-2026-08-09.md`

| # | pre-committed | fired |
|---|---|---|
| 1 | grant inside ±0.35 of 6.47 / 6.58 | **inverted — see T68-am-3.** Explain-first ran, explanation checked, band retired |
| 2 | refuse if dimmed / z-ordered, no partial credit | not triggered — deleted, confirmed three ways |
| 3 | refuse if one sibling only | satisfied — both measured |
| 4 | refuse if §6.1's punch left with the flood | **did not fire; the punch is intact.** The one that needed asking |
| 5 | void if confounded (C36) | lane self-disclosed and rescoped before the verdict |
| 6 | finding if `_dimOverlay` went too | **unresolved — finding 2** |
| 7 | re-derive layout if a bound moved | no bound moved |

---

## S2-am2-am2 — **the baseline shoot PINS ITS SLATE.** Ruled on SureThing's recipe question.

Baseline recorded at `fa93238`; across the flood removal, authored form 1.115 → 1.152 and
1.243 → 1.246, measured form 0.775 → 0.788 and 0.789 flat. **Floor unmoved — accepted**, and the
authored denominator behaved as the amendment intended: constant through a ground change, which is the
whole reason it was adopted.

**The question:** the harness deals an unpinned slate, and a record's x tracks the team name beside it
(`TeamLine` sets the record's origin from `nameText.preferredWidth`), so no fixed record box survives a
re-deal. SureThing re-cut per frame and left room's batch-26 boxes untouched via overrides — correct
handling of a live problem, and the right thing to escalate rather than absorb.

**Ruled: the shoot pins the slate. Per-frame boxes are the fallback, not the method.**

Two reasons, neither new — both already in the instrument's own record:

- **The ratio is only meaningful against an identical string.** The room lead's Part A says exactly
  that: *"the ratio is only meaningful against an identical string at an identical size, which is the
  comparison S2-am2 actually asks for — same surface, same view, over time."* A re-deal changes which
  digits are measured; `5-4` and `7-2` are not the same measurement, and the authored denominator does
  not rescue this because the **numerator** is measured on whatever glyphs the deal produced.
- **C34 already requires it.** The recipe's own reproducibility clause: *"If the surface is
  content-dependent, pin and assert the seed before shooting."* The surface is content-dependent. The
  clause was written for this and was not applied because nobody had hit the case yet.

**Named consequence, and it bounds what the recorded baseline can be used for.** The numbers above
were taken across an unpinned deal, so **deal-to-deal variation is an uncharacterised term inside
them.** At this coarse level the read is safe — nothing collapsed, and a real regression of the kind
this baseline exists to catch would dwarf the movements seen. But **+0.037 is not resolvable against a
noise term nobody has measured**, so under C37 no future movement of that size may be called a change
or a null on this baseline as it stands. Pinning removes the term outright and costs one seed
assertion; characterising it would cost a run of deals. **Pin.**

**Not owed: a re-shoot now.** `fa93238` stands as recorded, with its scope stated — the next baseline
shoot pins, and that one becomes the comparand with resolution. Re-shooting a baseline whose coarse
verdict is not in doubt spends a window to buy nothing (C17's discipline, pointed the other way).

**Recipe authority:** this amends `room-to-surething-s2am2-baseline-rig-recipe.md` §2a and §3. Room's
batch-26 boxes stay untouched and correct — they were cut against a frame whose deal is now history,
which is precisely the problem being closed. Room lead to fold; the recipe is theirs to hold.
