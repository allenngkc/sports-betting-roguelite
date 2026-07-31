# Register entries — 2026-07-31, batch 3

**Transcribe into `main-2/docs/design/REGISTER.md`.** Batches 1 and 2 are already transcribed.

---

## T11 — TV production typeface. **RULED. CLOSED.**

**State change:** Open · constraint added → **Closed · DD 2026-07-31.**

**Ruling: Encode Sans + Encode Sans Condensed. SIL OFL 1.1.**

**Tabular figures decided this, and they were measured rather than assumed.** §5 calls them mandatory
and non-negotiable, and names them the one typographic rule that survived every substrate change. I
rendered 0–9 in each candidate with `font-variant-numeric: tabular-nums` and compared advance widths:

| Family | Advance spread WITH `tnum` | |
|---|---:|---|
| Saira Condensed | **7.08px** | no `tnum` feature — **disqualified** |
| Saira Semi Condensed | **9.37px** | no `tnum` feature — **disqualified** |
| Fira Sans / Condensed | 0.00px | passes |
| Encode Sans / Condensed | 0.00px | passes |
| Barlow / Condensed | 0.00px | passes |

**Saira was the strongest candidate on character and it lost anyway.** An industrial grotesque with a
full width axis, from Archivo's own foundry — which would have made "the same hand" literal rather
than argued. Its condensed widths ship no tabular figures, so score, clock, money and counts would
twitch on every tick in the exact column where density matters. Character does not outrank the one
non-negotiable rule. Worth recording because Saira is the obvious pick on looks and someone will
propose it again.

**Chosen over the other two passers.** Fira Sans is humanist and slightly warm, and §4 spends the
surface's entire warmth budget on gold — a warm letterform quietly spends it a second time. Barlow's
rounded terminals soften precisely the technical read §5 asks for.

**Why Encode Sans specifically.** §5 asks for a technical grotesque, condensed for the ticket column
where density matters and wider for the scoreline. That is a **width-axis brief**, and this family
carries the deepest width range of any qualifying free family, so one face covers both jobs instead
of a pairing. Nine weights make weight a genuine channel for the L1–L2 tiers, which §5 explicitly
asks for now the panel is high-DPI. And it is a legibility-engineered *screen* face, which is the
TV's actual problem: read at four metres, muted, through bloom and grain.

**The "one hand, different jobs" claim, made explicit.** Archivo is an American-gothic **text** face
built for small running copy on a paper-like ground — that is why the laptop, which is a printed
document, uses it. Encode Sans is an engineered **screen** face with a width system, built for
signage-like hierarchy — that is why the panel uses it. Same discipline (legibility-first open
grotesques of the same era and lineage), different instrument. What T11 rules out is one superfamily
stretched across both screens, and this is not that.

Applied in the design system: `tokens/fonts.css`, `guidelines/type-tv.card.html`.

---

## T17 — Scorer-gap severity. **RULED: NOT an acceptable quiet win. Correctness defect.**

**State change:** new → **Ruled · DD 2026-07-31. Ranks above every Phase 3 visual refinement.**

An anytime-scorer bet that wins while the scorer is never revealed is **not a quiet win, it is the
surface asserting an outcome it never evidenced.** Three laws say so, and they are not close calls:

- **Principle 4, cadence is the dopamine.** "Small resolutions nested inside the long sweat arc are
  what keep a run alive; the interface must make resolution legible the instant it happens." A leg
  that greens with no cause shown is the one case where resolution is *illegible by construction*.
- **`VISUAL-DESIGN` §9's resolution ordering.** The on-pitch payoff completes, the callback lands,
  *then* the leg resolves. This defect resolves a leg with no payoff at all — it inverts the order by
  removing its first term.
- **The revealed-truth contract.** The TV owns outcome reveals and the laptop may only mirror what the
  TV has already revealed. Here MY BETS would mirror a settled scorer leg the TV never staged. "A
  change that arrives early is a lie" has a converse this defect proves: **a resolution that arrives
  without its cause is also a lie.**

**And it is worse in this market than in any other.** Moneyline and totals resolve on the scoreline,
which the TV always shows — the player can always see why. Anytime scorer is a bet on *who*, not
*whether*, and its entire appeal is the identity reveal; §6 already protects that with "`VALE SCORED`
appears only at the scorer identity payoff. A generic earlier goal cannot flip this copy." A scorer
leg that greens without the scorer appearing is the only leg type where the player cannot tell why
they won. It does not merely dent the market's appeal — it removes the thing the market *is*.

**Rejecting the on-theme argument before it is made.** One could argue that a book paying you without
showing why is good satire. No: satire is permitted in flavour and never in a slot where a fact
belongs, and this is the fact slot.

**Design instruction — reserve, don't spend.** A scorer leg must claim its backed-side goal *before*
ordinary beats spend the baked goals, so a causal reveal always exists to bind. If binding is ever
genuinely impossible, the correct behaviour is to **stage the reveal, not to suppress the win** —
and never to synthesise a reveal after resolution, which would break §9's ordering from the other end.

**Acceptance:** every settled anytime-scorer leg is traceable to a staged, revealed scorer event that
preceded or coincided with its resolution. Assert it as a test, not as a capture.

---

## T6 — Phase 2 scene grammar. **DESIGN-APPROVED ON STRUCTURE. NOT design-verified.**

**State change:** Implemented · review pending → **Design-approved (structure) · DD 2026-07-31;
Design-verified withheld pending the muted-couch gate.**

The register defines Design-verified as a review note against the item's spec. T6's spec question is
whether variation *reads as variation at four metres* — which the pack itself states is unproven,
because `-nographics` rasterises no frame. I will not mark verified on step data. **Send the captures
in `[TV] docs/tv-sweat-refinement/visuals/` and I will do the visual half.**

What I am approving, and four things worth recording:

**1. Compose, don't multiply — recorded as a standing law (T18).** Nineteen authored pieces across
three budgeted segments (grammar owns the approach, chance shape owns the delivery, payoff owns the
ending) instead of the ~150 a cross-product needs. **Future variety requests add to a dimension,
never to a matrix.** This is the property that made the phase deliverable and it is the one most
likely to be eroded by a well-meaning "just add a few more combinations."

**2. The gate-passes-without-delivery finding is the most valuable thing in the pack, and it becomes a
process ruling.** Signature diversity went green at `446ded7` while six near-miss payoffs still
rendered as one shape — a signature can differ while the motion is identical. **Signature diversity is
necessary but not sufficient, and may never again be cited as evidence that variation reads.** Any
future variation gate must assert *rendered* distinctness, not key distinctness. The lead surfacing
this against their own green gate is exactly the reporting this seat needs.

**3. Mood/physics independence — endorsed and recorded.** `CornerFor`/`CornerAgainst` and
`NearMissHope`/`Scare` are the bettor's hope and dread and drive only the mirror; which team
physically wins a corner reads from the staged fact. That is the laptop/TV ownership law applied
*inside* the TV, and it is correct. Both regression directions being pinned by test is the right
response to a bug that flipped during its own fix.

**4. `B × 1.00` — endorsed as a law.** "Grammar reshapes time; it never buys or spends it." Every
shape totalling exactly its budget is what keeps §9's resolution ordering and PRD §4.1's
same-frame callback honest.

**One documentation requirement.** The rebound → save/block collapse to a direct strike is **correct**
— two visibly stopped attempts would read as nonsense — but it currently lives only in code. A
designer reading the 48-cell matrix must be able to see that the cell is not renderable. Mark it in
the authored inventory.

---

## T18 — Compose, don't multiply. **STANDING LAW.** DD 2026-07-31.

Scene variation is modelled as grammar, pressure, payoff shape and reaction, composed as budgeted
segments in sequence. **Add to a dimension, never to a matrix.** A request for "more variety" is
answered with another value in one dimension, not another cell in a cross-product.

---

## T19 — Rendered distinctness, not key distinctness. **STANDING LAW.** DD 2026-07-31.

No variation gate may treat differing presentation keys, signatures or seeds as evidence that
variation is visible. Distinctness claims are made against rendered frames at the review distance, or
they are not made.

---

## T20 — The ticket column's px scale, re-derived. **RULED.** DD 2026-07-31.

`VISUAL-DESIGN` §3's px table was written against a ticket column at **~37%** of the surface and was
never revisited when `DESIGN.md` §6 corrected the column down to **26–28%**. At 28% the content box is
242px, and three of its values no longer fit the copy §6 itself authors — in the production face the
progress line and the shorter resolved statements both overflow by a few pixels, which is the worst
kind of clip.

**The ratio table is the law and resolves it** (§5: NEED 0.50, progress 0.40, leg rows 0.34 — a strict
order the px table does not preserve at this width). Re-derived, preserving that order:

| Element | Was | Now | |
|---|---:|---:|---|
| NEED / live statement | 28px | 28px | unchanged; may wrap to two lines, which §3 permits |
| Live progress | 23px | **19px** | subordinate to NEED, above the eyebrow |
| Resolved / pending row | 19px | **15px** | **live rows are display, resolved rows are index** |

**What was rejected:** shortening §6's authored strings to fit a stale measurement. That is how the
statement line was lost once already (T16's predecessor defect), and copy does not bend to a number
that was itself provisional.

The one remaining ellipsis is `MARCUS VALE TO SCORE` on a resolved row — the longest statement in the
product — which compresses honestly rather than clipping a short line by three pixels.
