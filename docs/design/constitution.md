# The studio constitution
> **APPROVED — Allen, 2026-08-03.** All three new clauses (seat's errors recorded as its own; measure the rendered thing; a confounded measurement closes nothing) approved explicitly. This file is canon; the DRAFT file is the preserved draft.


**Status:** DRAFT for Allen · **Drafted:** Design Director, 2026-08-03 · **Authority:** C24
**Canonical home on approval:** `main-2/docs/design/constitution.md`

---

## 0. What this is, and what it is not

This is the **authority-and-evidence layer**: who decides, what counts as evidence, and what
happens when a document and a build disagree. It is deliberately thin.

**It contains no colour, no type, no layout and no palette.** Those live in the owning documents,
one per surface. `design/08-art-direction.md` failed because one document tried to govern four
registers at once; this document governs none of them. It governs how they are governed.

Nothing here is new. Every clause is a law already ruled and already in the register, gathered so a
lead can read the operating rules in one sitting instead of reconstructing them from 200 rows.

---

## 1. Authority

### 1.1 Two tiers (C9)

**This constitution**, plus **one owning document per surface**. A surface's owning document is the
binding art authority for that surface. Current owning documents:

| Surface | Owning document | State |
|---|---|---|
| Room | `docs/design/room-design.md` | Approved · Allen 2026-07-31 (R13) |
| SureThing — the laptop | `docs/design/surething-design.md` | Approved · Allen 2026-08-06 (C26-am) |
| TV — match theatre | `docs/design/tv-design.md` | Approved · Allen 2026-08-07 (C26-am2) |
| Phone | `docs/design/phone-design.md` | **DRAFT for Allen** · 2026-08-09 (C26-am3) |

*(Table corrected 2026-08-09 — factual bookkeeping, not a clause amendment. Three of four rows were
stale: both approvals were recorded in the register at C26-am/C26-am2 and never reached this table,
which is C7's shape inside the constitution itself.)*

An owning document keeps its own real gates. A stub is a legitimate state: it says the surface has
no authority yet, which is honest, where an empty section pretends to authority it does not have.
**The phone's stub was legitimate until Allen put the surface in scope** — at which point the surface
needed an authority to be judged against, and C26-am3 expired the stub rather than letting it stand as
cover.

### 1.2 Precedence

1. **Allen.**
2. **This constitution**, for questions of authority and evidence.
3. **The surface's owning document**, for everything about that surface.
4. **The register's ruling for that item** (`REGISTER.md`).
5. **The slice's own specs and PRDs.**

Two corollaries, both already exercised:

- **Latest document governs** where two documents of equal standing conflict (C1).
- **Cross-surface artefacts are ruled at the DD seat with both slices present.** The unified grade is
  one: no slice tunes it unilaterally, and no slice is blocked from escalating about it (C20).

### 1.3 The tables are the canon (C22)

**A ruling exists when it is a row in `REGISTER.md`.** Not when it is written, not when it is sent,
not when a lead has built against it.

- Each batch ships once, as `register-entries-<date>-batch-N.md`, at authoring time.
- The orchestrator transcribes it and reports the ID list back.
- **The DD then reads the tables and never its own batch files.** A batch file is a draft.
- The tables carry a transcription log. A batch not on that line is not canon, no matter who has
  built against it.
- Nothing is "canonical alongside" the register.

### 1.4 One ruling, one ID (C22.1)

Where the same finding is ruled twice under two IDs, **the earlier ID governs** and the later becomes
a cross-reference. Additions made under the later ID fold into the governing line. This is a
bookkeeping rule with a real cost behind it: two live IDs for one finding is two leads building two
fixes.

### 1.5 The seat's own errors are recorded as its own

Where a ruling was wrong, the amendment says so and names the ruling as the defect — not the lead who
implemented it faithfully. Precedents: S15-am (the kit was precise, the ruling was not), S25-am, S31-am,
T31 (a spec'd value misread as a debug token). A register that hides the seat's errors is not an audit
trail.

---

## 2. Evidence

### 2.1 Rendered evidence or no claim (C11)

**Every design claim about how something reads is made against rendered frames at review distance, or
it is not made.** This includes Design-verified.

A review package is **its document plus its frames**. A package without its frames is not in review.

### 2.2 Rendered distinctness, not key distinctness (T19)

A claim that variation *reads* is made against rendered frames. **Signature diversity, seed counts,
matrix cell counts and enum breadth are never evidence that variation reads.** T6 spent two review
cycles on this: refused on identical composition, granted only when the frames differed.

### 2.3 Capture precedes rebuild (C17)

**No rebuild verdict on a state no capture shows.** Where a source read suggests a defect, the capture
is a named deliverable of the next window and the verdict waits for it.

This law has paid for itself twice — T26 and S32 both dissolved on frames, one rebuild cancelled
outright. When a capture and an earlier source read disagree, the handoff states which happened:
fixed between HEADs, or misread at source.

### 2.4 Evidence transport (C12)

Design review requires frames **in the import**, not in git. Bundles are the vehicle.

### 2.5 Measure the rendered thing, not the source

A source string is not a rendered element. Where a build step, a bundler or a cache sits between the
source and the frame, **an assertion about the source is not a measurement of the surface.** Verify
by measuring the rendered element.

### 2.6 A confounded measurement closes nothing

Where an instrument is saturated, out of range, or measuring through a channel the question does not
live in, the measurement is returned unadjudicated and the experiment re-run. T49 is the standing
example: an A/B between two bloom intensities, run against a stage already clipped to pure white,
compares two arms through a saturated channel and cannot answer the question asked.

---

## 3. Deviation

### 3.1 Fidelity (C14)

All work is **exceptional quality and a 1:1 match to the intended designs.** 1:1 is the bar, not the
aspiration. Deviations only where physically impossible, and each one **DD-signed before build.**

### 3.2 Impossible versus expensive (C16)

**Only the platform makes a thing impossible. A design decision makes it expensive.**

The expensive kind is a **signed deviation** carrying a **named cost** and an **expiry**. It is
classified at the DD seat and never assumed by a lead.

Worked examples in force: S28 (tracking unreachable in `UI.Text` — signed, expires at the TMP
migration), R21 (2.45% floor relief impossible under the approved one-tube lighting, not impossible
physically — lapses if a shot puts the floor in subject position).

### 3.3 Wrong in kind is not fixed by opacity (C10)

**An effect that fails on mechanism is disabled and re-scoped, never tuned toward invisibility.** A
lightening-only grain shader does not ship at reduced opacity. A forbidden line is deleted, not
softened. A full-field wash is removed, not dimmed.

### 3.4 Build-corrects-doc, bounded (C23)

Where a document and a build disagree on a **named parameter** with **no measured law** and **no
stated bound**, and **every design-verified frame contains the build's value** while the document's
value **has never been seen** — the build is the spec and the document corrects, quoting the value
inline.

**Never** for law-measured values. **Never** for values unread at review distance. This is C11's
converse and it is narrow by construction: it applies where the frames the studio already accepted
contain the build's number.

### 3.5 A bound is not a layout

**Landing a cap is not landing the layout the cap implies.** A bound added in one place obliges the
layout depending on it to be re-derived in the same commit. Three instances inside one fortnight
(T20, T47, T51) make this a standing rule, not an observation.

Re-deriving a fixed grid constant **once at design time** is legal. What is forbidden is a zone
resizing in response to content at runtime.

---

## 4. Inventories and gates

### 4.1 An inventory names its members (C18)

A bare count is not an inventory. **A gate certifies the geometry it ran against, and any change to
that geometry voids the certification.** A stale gate and a bare count are one defect: a claim that
does not say what it covers.

R22 is the worked case — Gate 8 certified walkable clearance against pre-two-bunk geometry, so it
certifies nothing about the room that exists.

### 4.2 Every gate states what it cannot see (C18, batch-6 fold)

**A check's blind spot is part of its result.** A gate reports what it measures *and* what it is
blind to, in the same breath.

This clause was written after **four green gates in one fortnight were found to be measuring nothing**:

| Gate | Was green | Could not see |
|---|---|---|
| Signature diversity (T19) | all session | whether frames differed |
| Offer containment (T47) | since it landed | anything — 0.5f epsilon on a canvas ~12× the panel |
| Collider inventory (R16) | all session | `MeshCollider` — four of the objects it counted |
| Wallpaper graphic (S49) | every run | whether a `Graphic` drew at all — no `CanvasRenderer` |
| The test runner itself (C29) | any suite, any seat | that it ran zero tests — a bare filter matching nothing exits green with `testcasecount="0"` |

**C29 (amendment, Allen-approved 2026-08-05):** every test invocation reports its executed case
count, and a run with zero executed cases exits non-zero. No verdict, gate, grant or
Design-verified claim rests on a run that did not state how many tests it ran.

Only captures caught any of them. A suite that cannot tell whether something drew states nothing.

### 4.3 Reachability (C19)

**An offer the engine prices is reachable on the surface.** Hiding it misrepresents the slate. Lists
scroll with a printed position rail (S27). A deliberate cap is a ruled exception that prints its own
count.

### 4.4 A control must bracket the interval it certifies (C36)

> **APPROVED — Allen, 2026-08-09.** Canon.

**A control certifies only the interval its samples enclose, and it is checked by the other half of
the instrument — never asserted by the half being checked.** An opening control pair brackets the
warm-up and nothing after it. A closing control is what certifies the run.

*Founding case:* an emission set passed `control-a == control-b` while the room was being mutated
underneath it — a capture step reset renderers to their shared-material value instead of restoring
their own state, so every later frame was shot against a changed room. The opening pair could not see
it by construction. Two capture sets were discarded learning this.

This is §4.2's temporal form: a control that brackets only the beginning cannot see the middle, and
its green says so if anyone reads it.

### 4.5 A null is invalid unless success would have been resolvable (C37)

> **APPROVED — Allen, 2026-08-09.** Canon.

Extends C32 from positive results to negative ones. C32 governs what a gate reports; this governs
when a gate's *"no effect"* is allowed to mean anything.

**Before a null is recorded, the instrument must be able to resolve the success it was looking for.
Where a successful outcome would land under the instrument's own floor, the test could not have shown
success in either direction, and its null is void — not a pass, and not evidence of absence.**

*Founding cases:* whole-pixel ramp counting carried ±25% on a 2 px ramp, so a three-point trend built
on it was never a trend; and a `_Sharpness` null was invalid because a successful halving would have
landed at 0.84 px, under the ~1 px single-sample floor — that arm could not have shown success however
the code behaved.

Its first application **un-retired two verdicts**: this clause recovers work as often as it discards
it, which is the correct shape for an instrument law.

---

## 5. Variety

### 5.1 Compose, don't multiply (T18)

**Variety adds a value to a dimension, never a cell to a cross-product.** Phase 2 delivered six
legible grammars from nineteen authored pieces on this rule.

---

## 6. Amendment

- This document is amended by the DD seat with Allen's approval, and every amendment cites the
  register item that motivated it.
- A clause here that contradicts an owning document is a **conflict to escalate, not a licence** —
  the owning document is not overridden silently.
- Clauses are promoted **from rulings**, never invented. Each one above names its source item, so a
  lead can read the case that produced it.

---

## Appendix — the source items

`C9` two-tier authority · `C10` wrong in kind · `C11` rendered evidence · `C12` transport ·
`C14` fidelity · `C16` impossible vs expensive · `C17` capture precedes rebuild ·
`C18` inventories and gates, and gate visibility · `C19` reachability · `C20` grade authority ·
`C22` the tables are the canon · `C22.1` one ruling one ID · `C23` build-corrects-doc ·
`C36` a control brackets what it certifies · `C37` a null needs a resolvable success ·
`T18` compose don't multiply · `T19` rendered distinctness.

Not carried here, and deliberately: every colour, type, layout, motion and palette law. Those are
the owning documents' content, and the reason this document is short.
