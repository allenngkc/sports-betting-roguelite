# Register entries — batch 39 — **Phase T's verdict**

**Design Director** · 2026-08-12 · docket: the Phase T before/after pair
(`dd-import/tv-phase-t-before-2026-08-11/` at `233bf7a` × `tv-phase-t-after-2026-08-12/` at `cb84278`)

**Destination tables:** `T83`, `T84`, `T85` new → **TV**. `T75-am4`, `G1-am3`, `T82-a` amend existing
**TV** rows.

---

## Verdict in one line

**The migration is GRANTED. Phase T is NOT Design-verified and does not ship** — landing the canon
face without re-deriving the layouts that depend on string extent has put a truncated requirement and
a collided money control on the surface, both demonstrated on clean matched pairs.

---

## T82's falsifier fired, and T82 survives it

Pre-committed in batch 38 before the set was opened: *within an equal-character-count group in the
right-anchored clock slot, the left ink edge MOVES if digits are proportional; if it is INVARIANT,
T82's premise is wrong and the whole ruling re-opens.*

Measured at this seat on the after-set's own `clock-strings.tsv` plus the frames:

| quantity | result |
|---|---|
| left-edge spread, 3-char `DD'` group | **22.0 px** (tabular predicts ~0) |
| correlation, ink width vs `hmtx` predicted advance | **+0.9824** |
| residual sd about the fit | **1.04 px** |
| narrowest `71'` → widest `90'` | 58.0 px → 80.0 px, **delta 22.0 px** |
| delta predicted from the font's own advance table | **21.2 px** |
| clock right edge across all measured frames | spread **5 px** (right-anchor holds) |

**The falsifier does not fire.** The left edge moves, and it moves by the amount the font's advance
table predicts, to within about a pixel. T82 is corroborated on rendered evidence by an instrument
that shares nothing with the two that produced it — a third measurement, in a third place.

**Coverage stated (C28):** 54 of 156 non-empty-clock frames measured, 37 in the clean `DD'` subset.
The rest were dropped where bloom merged the separator that isolates the clock. The instrument's
resolution is the 1.04 px residual, not the raw detection spread.

**T75-am3's corrected disposition is confirmed on frames:** the left edge moves, and it moves under
the canon face — so the mandate is unmet by the stack, exactly as ruled, and not by `Clock`'s face
assignment.

---

## T83 — the pair is a valid instrument on 130 of its 151 frames; 21 are excluded and named

**Twenty-one paired frames carry a DIFFERENT GRAMMAR between the halves.** They match on moment,
seed, scene and frame index — and differ on the scene grammar, which is content.

| moment | mismatched | of |
|---|---|---|
| `goal` | 6 | 40 |
| `sat-down` | 4 | 5 |
| `scorer-leg-dangerous-1` | 4 | 8 |
| `cashout-actionable` | 3 | 24 |
| `g1-column-all-markets` | **2** | **2** |
| `scorer-leg-dangerous-0` | 2 | 8 |

The README's claim — *"Every moment matches on count and on seed. Nothing was selected, dropped or
substituted"* — is **true as written and insufficient as a pairing claim.** Nothing was substituted;
the grammar simply is not one of the things the claim covers, and it is a variable that moves pixels.
**R43's mechanism one lane over:** the seed pins the DEAL, not the timeline, so the grammar occupying
frame *N* is not pinned by pinning the run.

**Exclusion authorised** under R23-am's test — named, measured, and resting on a real property of the
subject rather than on a threshold being slackened. It is also **self-detecting: the discriminator is
in the artifact's own filename** (C42's in-frame invariant, in its cheapest possible form).

**Two consequences that must travel with the set:**

- **`g1-column-all-markets` has NO clean pair** — both frames are mismatched. Any *comparative* claim
  about that moment is single-sided. (G1's verdict does not depend on it; see G1-am3.)
- **`sat-down` retains one clean pair of five.**

**On the 130 matched frames the migration is GRANTED:** the renderer swapped `UI.Text` → TMP, the §4
canon faces landed, and synthesised bold and italic are gone. That is T81's closed variable, and it is
verified.

*Not promoted to a law.* The general form — *a pairing claim names the variables it matched on, and
the ones you pinned are not necessarily the ones that move* — is close to R43's founding case but not
identical to it. Recorded here; it promotes if caught a third time.

---

## T84 — the extent-dependent layouts were not re-derived. §3.5. **BLOCKER**

Two violations, both on **clean grammar-matched pairs**, both new in the after half.

**(a) The NEED line truncates to a fragment.** Same seed, same ticket, same three legs:

| | NEED renders |
|---|---|
| before | `ONE TEAM BLANKED` |
| after | **`ONE TEAM`** |

`ONE TEAM` is not one of G1's authored forms. It is the word-boundary truncation backstop firing —
on the line that tells the player what has to happen for his money to land, and it has dropped the
operative word. The deck exists **so that truncation is never reached**; `ONE TEAM BLANKED` is itself
the authored fallback, and it overruns too. **S17's class:** truncation that drops the rule is
misleading at the point of spending.

**(b) The money control collides.** Same seed, scene, grammar, moment and frame — a true pair:

| | control renders |
|---|---|
| before | `CASH OUT $131`  ·  clear gap  ·  `HOLD E` |
| after | **`CASH OUT $131OLD E`** — the label overprinted by the figure, spilling past its field |

§6.1's money control, and T63/T68/T68-am/T71's subject across four batches. T68 closed on *"the money
control currently shows no instruction"*; the instruction is now present and unreadable for a
different reason.

**Mechanism, one line:** the canon face is wider, so every layout whose bounds or whose neighbour's
position was derived against the old face is now unverified.

**This is NOT T74 and must not be routed there.** T74 settles which size *authority* governs
(§4.1-ratio vs shipped-px). A collision is not a size-authority question. This is **constitution §3.5**
in its own words — *landing a cap is not landing the layout the cap implies; a bound added in one
place obliges the layout depending on it to be re-derived in the same commit.* T20, T47 and T51 made
it standing; this is the fourth instance and the first where the trigger is a **face** rather than a
cap, which is the only reason it was not anticipated.

**T81 does not cover this.** T81 ruled *"point size is HELD; rendered EXTENT will move — that is the
measurement, not a breach"*, and that is correct: a wider string is the measurement. **A string that
overprints its neighbour is not a wider string, it is a broken layout** — and T81's own guard says the
probe is an instrument and not a knob, which forbids tuning sizes inside the pair, not fixing a
collision after it.

**RULED: the surface does not ship with a truncated NEED line or a collided money control.** Phase T
is **not Design-verified** until both clear. The pair is unaffected — it is shot, the freeze is
lifted, and these fixes land after it, which is where they belong. Sequencing is the orchestrator's.

**Owed — a sweep, not two fixes (C18 §4.1):** every slot whose extent, or whose neighbour's position,
was derived against the pre-migration face, **inventory naming its members**. Two were found by eye in
two of nine moments. **Nobody has looked at the other seven**, and the seat states that as its own
blind spot rather than implying the list is two.

---

## T85 — a fourth variable joined a closed list, and nobody rules it

The after-set README names the variable as the type stack *"plus the rulings that ride with it —
T73's real Condensed Bold 700, T77's italic struck, **T-5's tracking**."*

**`--tv-track-name` at .02 em is not in T81's variable.** T81 closed it at exactly three things and
said *"nothing joins later"*: the renderer swap, the §4 canon face per slot, and the retirement of
synthesised styling. Tracking is none of the three.

**C43-am's separability test decides it, and decides against the fold.** The face has **no null arm** —
TMP renders from a baked named instance, so a face must be chosen. **Tracking has one:**
`characterSpacing` 0. It could have been sequenced out; by C43-am(3) it should have been.

**And it has no owner.** `tv-design.md` contains **no tracking clause at all** — zero occurrences of
tracking, character spacing or letter spacing anywhere in the owning document. So the surface now
carries a spacing value this seat never assigned. **T75's shape, one property over:** a value that
renders by defaulting rather than by ruling, in an inventory that does not name its members.

**Material, not bookkeeping:** by the lane's own measurement it is **~10 px of the NEED overrun** at 18
characters — the overrun T84 and G1-am3 are adjudicating.

**Ruled — the remedy order:**

1. **Remove the unruled tracking and re-measure.** You do not re-derive a layout, widen a span or
   shrink a ruled size to accommodate a parameter nobody ruled.
2. If the overrun survives at tracking 0, **then** span-or-size, routed to T74 per G1-am.
3. **The owning document owes a tracking clause for this surface** — written at this seat, against
   frames, after T84 clears. Until it exists, .02 em is not canon and does not become canon by having
   shipped.

**The pair is not void.** The addition is uniform across the slots it touches and nameable in one
line, which is the test C43-am(2) sets. **The pair's stated variable is CORRECTED to include it**, and
T81's closed list is recorded as breached — because a closed list that absorbs a fourth item silently
is not a closed list, and the next migration will read this row before it reads T81's.

---

## T75-am4 — the `CashOutStatus` pre-commitment is RETURNED UNADJUDICATED (§2.6)

T75-am2 pre-committed: *reads as two voices inside one control → moves to condensed; reads as
label-and-figure → the default stands.*

**Neither branch fires.** The two members **overlap** on the after frames — `CASH OUT $131OLD E`. A
question about whether a second face adds a voice cannot be read through a collision: the confound is
in the same channel as the answer.

This is a **void, not a null** (C37's distinction): the instrument could not have resolved either
outcome in this state, so its silence is not evidence for the default. The pre-commitment stands
unchanged and **re-reads on the frames that follow T84's fix** — no new ruling needed, which is the
point of having pre-committed it.

---

## G1-am3 — the re-certification result, recorded; and a correction to the gate's own failure text

**Result accepted in full.** The NEED column does not hold in the canon face: both at-budget forms
(`ONE TEAM SCORELESS`, `ONE TEAM BLANKED`) miss 249 px, as do three of six authored statements. The
compact column re-certifies at 143 px.

**The lane's digit-split is adopted and it is sharper than the ruling it answers.** G1-am2 made every
digit-bearing string provisional; the lane observed that **every NEED-column miss is digit-free, so
the failure is final and does not move when T82 lands**, while the compact column's *pass* is the
provisional half. Correct, and better stated than the ruling that asked for it.

**Copy is not reopened.** §4 / T24-am: *never the copy*. The remedy is span-or-size, in T85's order.

**Correction — the gate's failure message prescribes a remedy canon forbids.** It reads: *"if both
miss, the budget itself is wrong and the deck needs a third line, not a truncation."* A third line is
**re-authoring**, which G1-am ruled is the last resort and only after the span route fails. The gate
predates that ruling. **A gate's failure text is read as an instruction and must not prescribe a
remedy the owning document forbids** — the message is corrected in the same commit as the fix.
Recorded as a register-level observation; it promotes to a law on a second catch.

---

## What this pair certifies, and what it does not

**Certifies:** the type stack moved from legacy plus a defaulted instance to TMP plus the §4 canon
faces; synthesised bold and italic are gone; and the rendered delta of that move is on record.

**Does NOT certify** — carried forward from T81 and added to here:

- that the renderer is neutral (T81's stated blind spot; no later item may cite this pair for it);
- anything about the 21 grammar-mismatched frames, or any comparative claim about
  `g1-column-all-markets`;
- that .02 em tracking is correct, or ruled, or canon (T85);
- that the extent-dependent layouts hold — **they do not** (T84).
