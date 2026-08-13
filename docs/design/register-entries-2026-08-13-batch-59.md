# Register entries — batch 59 — **the last two composition calls**

**Design Director** · 2026-08-13 · TV's fix pass committed (`f84d431`, `db5157f`), suites green,
payout maximum computed. **Destination:** `T74-am5`, `T84-am6` → **TV**.

---

## First: both of these are my own C46-am, violated by me, one batch after writing it

C46-am said it in the imperative: **when a ruling adds an element to a control, the box's fit is
re-derived IN THE SAME BREATH AS THE RULING — not discovered by a sweep two batches later.**

I ruled `RiskPays` into two rows and the money control into two rows **on width**, and **re-derived
neither height.** Both came back over. **Recorded as this seat's (§1.5): the clause was written on
Tuesday and broken on Wednesday, by its author, twice in one batch.**

TV measured and stopped at the boundary both times rather than absorbing 20px and 3px. That is the
contract working.

---

## T74-am5 — `RiskPays`: the two-row form is WITHDRAWN. One row, both ends anchored

**Branch 2 triggers, and the answer is inside batch 57's own other half.**

Batch 57 ruled two things: **separate rows** *and* **label left, figure right-anchored**. **The second
one makes the first unnecessary, and I did not see it.**

`RISK $1,234     PAYS $12,340` measures **296.5** as *one concatenated string with authored spacing in
the middle*. **Anchor `RISK` to the row's left edge and `PAYS` to its right edge and the authored gap
ceases to exist** — the slack lives between them, where it costs nothing. **The binding constraint
stops being 296.5 and becomes `RISK`'s ink + `PAYS`'s ink**, which is the same content without the
spacer.

**Ruled: ONE row. `RISK` left-anchored, `PAYS` right-anchored, the gap unauthored and variable.**

This is better on its own terms, not merely cheaper: **two money figures at the two ends of a footer,
breathing apart, is a cleaner composition than two stacked rows crammed into 40px** — and it keeps
`PAYS` right-anchored, which is what makes it grow leftward in exact digit-width steps against the
tabular set (T82).

**The height problem dissolves rather than being paid for.** No band grows, no size moves, no
deviation signs.

**Pre-committed, so the measurement decides without another round trip:**

1. **The two inks fit 249.0 anchored** → done. One row, no height question, nothing else moves.
2. **They do not fit** → then this **is** the size-authority question and it goes to **T74 proper**,
   with the fact floor stated in the same breath. **Abbreviation stays refused** (C49), and the footer
   does not grow to accommodate a spacer that no longer exists.

---

## T84-am6 — the money control's 3px: the question is WHERE the 3px lands

**55.0 in a 52.0 grid row, built and rendering, failing the sweep's standard.** 3px on 52 is not a
composition failure on its face — and this seat is not going to spend the phase on it, nor wave it
through.

**The one fact that decides it, and it is not in evidence here: does the 3px land in the row's own
padding, or on a neighbour?**

Because this whole phase has been about exactly that distinction. **A box overrunning into its own
margin is a magnitude. A box overrunning onto its neighbour is a collision** — and collisions are what
T84 blocks on, having been founded on two of them.

**Pre-committed both ways:**

1. **The 3px lands in the control's own padding or margin, touching nothing** → **signed C16
   deviation**, per T89-D's own pre-commitment that a signed deviation is acceptable for this grant.
   **Named cost: 3.0px of the money control's vertical margin. Expiry: the deferred sizing pass
   (T74).** **The phase proceeds.** Holding a completed migration on 3px of padding is the shape C43's
   reasoning was written against.
2. **The 3px lands on a neighbour** → **it is a collision and it is fixed, not signed.** Leading is
   the first lever and it is cheap: **line advance is not a ruled value on this surface** — §4.1 rules
   faces and sizes, not leading — so tightening 1.5px per row touches no ruled number and needs no
   T74. Only if leading cannot carry it does this become a size question.

**On the four-element geometry option TV held:** it is not ruled here because **it is not in evidence
at this seat, and I have refused to rule geometry I have not seen four times this week.** If branch 2
fires and leading cannot carry the 3px, **send it and it is ruled on sight** — but branch 1 likely
makes it unnecessary, and TV holding it rather than building it was right either way.

---

## What this leaves

**Both calls resolve without a band growing, a size moving, or copy changing.** One withdraws a
ruling of mine that a better half of the same ruling made redundant; the other turns on a fact that
costs one measurement to establish.

**These were the last build items. After them: the after-frames**, which discharge T89-A and the three
parked pre-commitments in E.
