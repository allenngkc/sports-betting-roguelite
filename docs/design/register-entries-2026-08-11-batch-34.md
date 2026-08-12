# Register entries — 2026-08-11, batch 34

**Seat:** Design Director (`main-2` terminal) · **Docket:** TV Phase T BEFORE-set
(`dd-import/tv-phase-t-before-2026-08-11/`, 151 frames, shot at `233bf7a`).

**No verdict, by the set's own correct declaration.** This is half an instrument; the verdict pairs
with the after-set at phase close. What follows is baseline characterization, two ratifications, one
instrument specification, and **one finding that puts the pair at risk and needs acting on before
the after-set is shot.**

---

## The set is ACCEPTED as half an instrument

Every C34/C43 obligation is met, and one of them is met better than asked. The set **pre-commits the
single moved variable before the fact** — renderer only, UGUI `Text` → `TMP_Text` at preserved
rendered size per T74 — written down in the README before any after-frame exists. C43 was ruled one
batch ago; this is its first use by a lane, and the lane used it the way it was written rather than
the way that would have been convenient.

Also correct, and recorded so the practices stay: pin carried **in every filename** so provenance is
greppable out of the frame's own name; the C29 guard catching the NUnit filter as `executed 0 of 0`
rather than a green empty run — the runner law doing exactly the job C29 was written for; frames
organised by **moment** rather than by seed, on the stated reasoning that every question here is a
question about a slot and a slot lives in a moment. That reasoning is right and it is why this set
is readable.

---

## The absent moment — TV's reasoning RATIFIED, no action

`scorer-leg-resolved` is missing from all five seeds, declared by the harness in its own log rather
than left as a hole to be found. TV's three-part account is correct and I adopt it:

1. It used to occur on `27182818`; it does not now — **T61's** standing subject.
2. Merging main would not restore it — checked rather than assumed (`engine/` and `RunDirector.cs`
   unchanged between this commit and main; only `sim/` differs, which is not the runtime).
3. The pair stays **balanced**, so nothing in the comparison is one-sided.

Adding what the register can: the **slot** is still covered. `scorer-leg-dangerous-0/1` carry the
AnytimeScorer progress line in its unscored state; what is absent is one *string value* (`SCORED`,
T70-am's resolved half), not the slot. For a migration certifying **rendered size preservation**,
slot coverage is what the pair needs, and it has it. Balanced-absent is a real limit on what the
pair can say about that string, and it is now stated (C25) rather than discovered.

The mid-phase caveat TV attached — *if T61 is fixed mid-phase that is a second variable and the pair
must be re-shot* — is exactly right. **It is also the general case, which is the next item.**

---

## T80 — the pair's freeze list. Four more variables can move between the halves, and three are scheduled to.

**NEW — ruled. This is the finding of the docket.**

TV flagged T61 as a mid-phase second-variable risk. That instinct is correct and it does not stop at
T61 — the register names **four** further items that would move a variable inside the pair, and
three of them are parked against a phase that is **already unblocked**:

| Item | State in the register | Effect on the pair if it lands mid-phase |
|---|---|---|
| **C2** | TV light spill: *shipped green tolerated for now; target cold white-grey, **corrected in TV Phase 3*** — still **Interim**, never closed | Moves the ground under **every** value in the after-set |
| **T9** | `chromeCyan` on leg/clock/records/chrome labels — *Debt · **Phase 3*** | Recolours slots the pair is measuring |
| **T10** | Two hardcoded emission rest values, one below the black floor — *Debt · **Phase 3*** | Moves the substrate the ladder is read against |
| **T61** | Scorer leg never terminal — TV's own flag | Restores an absent moment on one side only |

**Phase 3 is not gated.** T7 reads *"not started · gated on T5"*; T5 is settled and **T41-cl
explicitly unblocked TV Phase 3** on 2026-08-04. So the three Phase-3-parked items are free to start
at any time, and Phase T is running now.

**Ruled — the design constraint:** for the Phase T pair to be an instrument, **C2, T9, T10 and T61
are frozen from the shooting of the before-set until the after-set is shot.** If any of them lands
in between, the pair is void and re-shoots — not "is read with a caveat". C43 does not admit a
partial pair: a second moved variable destroys the pair's power to certify *either* change, and
these four move the ground rather than a slot, which is the worst version of it.

**Not a scheduling order.** The constraint is the DD seat's; the sequencing that satisfies it is the
orchestrator's, with Allen where it touches milestone order. Routed, not decided here.

**Why this is the seat's to catch and not the lane's:** the risk is not visible from inside Phase T.
Every one of these items is closed-looking or parked from where the TV lane stands — C2 is an
*interim* ruling from July, T9/T10 are labelled *debt*. Only the register shows all four pointing at
the same unblocked phase. That is C39's shape — enumerate the whole path, not the segment you own —
applied to a schedule rather than a signal chain.

---

## T75-am2 — the two live carve-outs: what this half can and cannot say

### `Clock` — the before half supplies BASELINE ONLY. It cannot answer the carve-out, by C15's own premise.

T75 asks for tabular *verified on the built face*. **The built face does not exist in this half.**
`233bf7a` generated the four TMP assets and wired none of them (that is T-3), so these frames are
the legacy `UI.Text` render — and C15's founding premise is that *tabular figures become reachable
at TMP*. Tabular is **unreachable** in the before half by construction. This half therefore
contributes baseline widths and nothing else; the carve-out reads on the **after half alone**, with
the before half as contrast. The README's line that `goal` and `sat-down` "carry it" is true of the
baseline, not of the verification.

**What is solid, across 100+ frames:** the clock slot is **right-anchored**, ink right edge constant
at **x ≈ 2343–2348** while the left edge travels. So a width change surfaces at the **left edge**,
and the slot's anchoring is itself stable enough to measure against.

**What is confounded, reported as confounded (§2.6):** I attempted to group clock strings by
character count via column-gap glyph segmentation, to test width-invariance within a group. **Bloom
merges adjacent glyphs into single runs** — run-count is not character-count at this bloom level,
and the resulting groups are not what they claim to be. The measurement is returned unadjudicated. I
am not converting it into a number, and the apparent width spread it produced is **not** evidence
the face is or is not tabular.

**Instrument owed for the after-set, and it is cheap:** have the harness **emit the clock string
with each frame** (filename or sidecar, either). Then grouping is exact, and the test is one
subtraction with no inference from pixels:

> Within a set of clock strings of **equal character count**, in a right-anchored slot, the **left
> ink edge is invariant** iff the digits are tabular.

Both branches pre-committed now, before the after-set lands: **left edge invariant within a
character-count group → the carve-out discharges; left edge moves → it does not, and regular is
wrong for this slot** regardless of how the default was reached.

### `CashOutStatus` — baseline recorded; the disposition is asked about the right thing

Confirmed on `cashout-actionable`: the control's two members render **in one face today**,
separated by **value and size only** — `CASH OUT $131` in gold against the state word (`HOLD E`;
`MARKET SUSPENDED` in the same slot on `sat-down`) in a low-value grey.

That matters for how the pre-committed disposition is read at phase close. The after-set does not
introduce a distinction into an undifferentiated control — it adds a **face** split on top of a
**value** split that already exists and already works. So the question the pair answers is not
*"are these two members distinguishable"* (they are, today, without any face difference) but
**"does a second face add a voice the control did not have"**. T75's pre-commitment stands
unchanged; this states what it is being asked about, so the answer is not credited to a
distinction the before half already shows.

---

## Incidental — T65's owed settlement capture may be dischargeable from this set

Measured across all 151 frames: the room wall is constant at RGB ~(30.2, 51.8, 34.1) and **does not
track panel brightness** — except at the two payoff beats, `t68am-accept-slot` and
`t71-win-tally-slot`, where it rises to ~(51.4, 65.1, 43.2) and ~(43.8, 62.0, 40.6). That is
`RoomSettlementGlow()` firing on settlement and nowhere else — **T65 built as ruled**, visible in
the room, on a pinned in-room set.

T65 closed with its re-tint value **(88.0°, 0.9) provisional until a settlement capture exists —
owed, does not withhold (C17)**. This set appears to contain that capture.

**Deliberately not measured here.** Hue and chroma on an emissive re-tint are **CIELAB on linear
authored values** under C33-am3, and the numbers above are crude display-encoded RGB from a
different instrument in a different space — the three ladders are never compared, which is the law's
whole point. Routed to the room and TV lanes to measure in the correct space and close T65's owed
item if it holds. Named so nobody shoots a settlement window that already exists.

---

## Summary

| ID | Disposition |
|---|---|
| — | Before-set **ACCEPTED as half an instrument**; one-variable pre-commitment, pin-in-filename and C29 catch all endorsed |
| — | `scorer-leg-resolved` absence — TV's reasoning **RATIFIED**; slot still covered, one string value uncovered, limit stated |
| **T80** | **NEW · RULED** — C2, T9, T10, T61 **frozen** for the duration of the pair; any landing mid-phase voids it and forces a re-shoot. Phase 3 is unblocked, so this is live. Sequencing routed to the orchestrator |
| **T75-am2** | `Clock`: before half is baseline only (tabular unreachable pre-TMP); right-anchor at x≈2343–2348 solid; glyph-segmentation instrument **confounded by bloom, returned unadjudicated**; harness to emit the clock string; both branches pre-committed. `CashOutStatus`: baseline is one face split by value — the pair is asked whether a second face adds a voice |
| — | **T65** owed settlement capture likely present in this set; not measured here (wrong space); routed to room + TV |

**To TV:** the set is good and it is accepted. Nothing here blocks T-3. The one thing needed back
before the after-set is the clock string alongside each frame. The freeze is not yours to enforce —
it is going to the orchestrator.
