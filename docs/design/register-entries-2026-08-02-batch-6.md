# Register entries — 2026-08-02, batch 6

**Transcribe into `main-2/docs/design/REGISTER.md`.** Numbering resumes at **T47** — my session's
T22–T27 were renumbered T41–T46 at transcription and I am reading from that canon.

---

## T47 — Markets working-margin collision at 4 legs. **RULED: bound the flow region. The action stack stays anchored.**

**State change:** C16-class, open · B1 blocked → **Ruled · DD 2026-08-02.**

**The action stack does not un-anchor.** PLACE, LOCK IT IN and SKIP ROUND are the commitment
controls, and their position is load-bearing. If the stack flows, the most consequential control in
the game sits at a different height depending on how many legs the player marked — **LOCK IT IN moves
because you bet more.** This system already spends 60×32px and a written word on RUB OUT specifically
because a mis-click here costs money; it will not then let the lock button wander. Anchored, always.

**So the flow region is bounded — and MaxLegs=4 is what makes that computable.** Reserve the exact
height that 4 legs + combined + stake + payout need, place the anchored action band directly beneath
it, and the two can never meet. No scrolling (the margin's leg list is not an interior market list,
and at a hard cap of 4 it never needs to), no runtime resize, no overlap.

**Name the recurring failure, because this is its third instance.** A bound was added in one place and
the layout depending on it was never re-derived — identical in shape to T20 (px table never re-derived
after the ticket column narrowed from ~37% to 26–28%). **Landing a cap is not the same as landing the
layout the cap implies.** Any future change to MaxLegs re-derives this reserved height in the same
commit.

**A second, separate defect in the same numbers.** `LockReason` measures **−400..−420** while `Lock`
measures **−426..−478** — they do not overlap, so **the reason line is rendering above the LOCK
control rather than inside it.** `LockAction` specifies the reason *within* the ruled control's own
box. That is why it is colliding with the payout at all: it is floating in the margin's flow instead
of travelling with the button it explains. Put it back inside the control and 14 of the 36px of
trouble disappears without touching layout.

And it must never sit on the payout under any circumstance — that figure is the one element in the
margin carrying the hand-laid wax highlight, and the house's oxide stamp landing on the player's
money figure is the two-ink rule collapsing in the most visible place on the surface.

**Endorsed without reservation:** the lead found and fixed **vacuously passing containment tests** —
`GetWorldCorners` against a 0.5f epsilon on a world-space canvas, ~12× the panel, where no layout
could ever have failed. The Phase-A offer-container check had been green since it landed while
measuring nothing. Only the capture caught it. That is precisely what C17 is for, and it is the same
class as T19: **a green gate is not evidence; a measured frame is.** Both checks now measure in
canvas-local pixels, which is the right unit.

---

## T48 — §1.1 vs the unified grade. **RULED: Option A. Neutralise the lift's hue, keep its lift.**

**State change:** joint DD+TV, open → **Ruled · DD 2026-08-02, with the TV seat's agreement given
here explicitly (the grade spec is TV's pen and I hold it).**

**The ruling.** Take the shadow-lift's black point to a **neutral of the same value** — L\* 3.3,
chroma 0. Equalise the lift vector's channels (`0.99, 1.00, 1.03` → equal RGB), keep the lift itself
and its 0.0075 offset untouched.

**Why A and not the others.** The grade's stated purpose is a **level** requirement: nothing in frame
darker than a screen's off state. The hue was never the requirement — it was inherited by writing the
level as `#0a0c10`, a value that happens to be the TV substrate. **One number was doing two jobs**:
naming a luminance floor and, accidentally, naming a hue. A panel is allowed to be cool because it is
a cold display. The room is warm dirty plaster. Splitting that number into two is not a contradiction
of the spec, it is a correction of an over-specification.

- **B (split the grade)** breaks the premise. One grade over room and screens is the strongest
  belongs / does-not-belong lever in the project. Not spent on a hue.
- **C (amend 1.1)** ratifies the cyberpunk-blue rut this project names by name as its anti-reference.
  Unavailable at any price.
- **D (warm the lighting)** treats a measurement as a look, costs the single-source read, and leaves
  the black point blue underneath. The lead's caution is correct and I adopt it.

**Amend `unified-grade-spec.md`** to state the black point as **a level with an explicit neutral
hue**, and to record that `#0a0c10` is retired as the grade's shadow-lift target while remaining the
TV substrate value. The two were always separate ideas.

**The diagnosis is the valuable part, and it is exactly right:** room surfaces sit at L\* 10–18, the
black point at L\* 3.3, so at that separation **the black point is not a floor underneath the image,
it is the image.** Everything in a room this dark is shadow, so a cool black point is a cool room by
construction. That sentence should survive into the spec.

**T45 is subsumed — do not work it separately.** T45 (room drains to navy `#0e121d` on a dead leg) and
T48 are the same defect at two luminance levels: as the leg dies and the emissives drop, more of the
frame falls into the lifted-shadow region and the blue black point takes over. **Re-measure T45 after
T48 lands; expect it to resolve with no separate change.** If a residual re-tint survives, the earlier
instruction stands — keep the mechanism, retarget death toward the room's own darkest olive
`#0F1108`, never toward blue.

**Endorsed, and worth the board seeing why.** The room lead isolated the variable with a
grade-bypassed twin at identical rig and framing, cleared `MoonDirectional` against the R18 bound
rather than reaching for the nearest knob, proved determinism by identical MD5, folded the instrument
into an editor-free one-command gate, and **declined to change a ratified parameter that was not
theirs.** That is the standard. Note also that the room lead reported a finding whose headline is
"my slice's top law fails" — self-reporting against your own sign-off is what makes an audit worth
reading.

**One requirement before the grade session concludes**, adopting the lead's own caution as a rule:
**the TV set must be re-captured screens-dark and grade-bypassed** on the same terms. The room's cast
is blue at ~270°, the TV's is green with known independent sources under C2/C13; until both are shot
on matched terms the two are not comparable evidence, and a shared-grade conclusion drawn from an
unmatched pair would be T19 again in a new colour.

---

## T49 — C8 bloom A/B (1.8 vs 1.4). **VERDICT WITHHELD — the experiment is confounded, not the evidence thin.**

**State change:** awaiting verdict → **Returned unadjudicated · DD 2026-08-02.**

I will not pick a bloom intensity off these frames, and the reason is a measurement, not caution.
Measured on the Set B captures: **pure `#ffffff` at luminance 1.000 across the pitch region in three
of four sampled frames** (T41). The grade's bloom threshold sits near 0.9. **A stage already clipped
to pure white blooms maximally at either intensity** — 1.8 and 1.4 differ only where the signal is
below threshold, which is not the region the A/B is trying to judge. Both arms are being compared
through a saturated channel.

**Re-run the A/B after T41 caps the stage under the brightness ladder.** Then the pair differs where
it is supposed to, and a verdict is worth having. Same discipline I applied to myself in the T21
addendum: **do not let a confounded measurement close a visual question.**

## T50 — T25.2–25.7 type re-check. **PARTIAL: the face is confirmed; the column judgements are blocked by T46.**

Encode Sans / Encode Sans Condensed is **confirmed in situ at the seated camera** — it holds at
distance, the condensed column reads, and tabular figures behave (scores, clocks and money change in
place without the surface twitching, which was the whole basis of the T11 ruling). **T11 stands, now
on rendered evidence rather than advance measurement.**

Per-item judgements about type *inside the ticket column* are **not** grantable from this set: T46
has the stage overdrawing that column, so anything I concluded about leg-row type would be a
conclusion about the overdraw. **Re-check T25.2–25.7's column items after T46.** Everything outside
the column — scorebug, event strip, risk/pays, cash-out band — is clean to judge and passes.

---

## T51 — TV-15, stacked label-above-value misses by 0.3px. **RULED: the 0.3px yields. Canon holds.**

66.7 against 67px is **not a design fact** — it is a rounding artefact of a face substitution and a
line-height computation. Canon says stacked because stacked encodes a **reading order**: what it is,
then what it says. Side-by-side changes an information hierarchy to serve three tenths of a pixel.

**Never reorder information to satisfy a sub-pixel shortfall.** Grant the region 68px, or set an
explicit line-height that lands the stack at ≤67 — either is fine, and either is legal: §6 forbids a
zone **resizing in response to content at runtime**, not re-deriving a fixed grid constant once at
design time. That is exactly what T20 did.

**Standing rule (companion to T21):** a sub-pixel shortfall is a measurement to absorb, not a reason
to change a hierarchy. T21 said an element that cannot lose width without losing its meaning must not
be the first asked to; this is the same principle one axis over.

## T52 — TV-02, momentum tape shape. **T28 CONFIRMED: one 28px strip. Phase 4 may build on it.**

Per-leg momentum rows are refused on three grounds. **Momentum is a match-scoped fact, not a
leg-scoped one** — per-leg rows would assert a property the data does not have. They would **multiply
one reading into N the player must integrate**, on a surface whose whole argument is that the bet is
legible at a glance. And they would **add N more elements competing on the brightness ladder** — which
is the precise pressure that produced T41, in the same window. One strip. Confirmed explicitly, as
asked.

---

## T53 — Room collider count. **RULED: 29. Remove the two stray, keep the two interactable, correct §1.6.**

**Both sides are partly wrong, and my doc is the more wrong of the two.** §1.6's "27 colliders" is
**stale and was never measured** — I carried it from the inherited corpus. The room has **31**. Rule
per the lead's own (correct) distinction, which is that the four extras are not equivalent:

- `LaptopScreen` and `PhoneScreen` — **Interactable layer, keep.** These are how the player addresses
  the two machines. Interaction collision is function, and this system does not remove function to
  make a number match a document.
- `TVScreen` and `WindowPane` — **default layer, stray, remove.** Redundant with the wall and body
  colliders already behind them. They collide with nothing that needs colliding with.

**§1.6 is corrected to 29**, and its sentence is amended to say what it actually means: *dressing and
wear add no colliders; interaction may, and each one is named.* The original wording implied a frozen
total, which was never the point — the point was that art does not get to change collision.

I take the lead's recommendation, and I note why it was right to route it rather than act: it touches
interaction, which is not the room's to move.

**The tooling finding is more serious than the count.** The gate harness counts only `BoxCollider`
and is blind to `MeshCollider` — so **"27 colliders PASS" has been green all session on a check that
could not see four of the objects it claimed to be counting.** That is the same failure as the markets
lead's vacuous containment epsilon (T47) and as the signature-diversity gate in T19: **three
independent green gates this fortnight that were measuring nothing.** Fix the harness to count all
physics colliders, set the expected count to 29, and — the general instruction — **every gate states
what it cannot see.** A check's blind spot is part of its result.

## T54 — Room Gate 8 (walkable clearance) certification is void. **RULED: re-certify.**

The sign-off records PASS on an in-editor playtest that **predates the two-bunk layout brief**, and
bunk 2 then added three colliders including a slab whose underside sits at y = 1.50, overhanging the
walkable aisle by ~0.35 m with posts inside the lane. **A certification cannot cover geometry that did
not exist when it was issued.** Gate 8 returns to unproven.

Cheapest resolution stands: a human walkthrough, or Allen confirming he walked the two-bunk version.
Until one of those, Gate 8 is not a pass, and the R9/R10 re-gate must not be reported as 8/8.

The lead wrote that sign-off and volunteered the correction. Recorded as such.

---

## T55 — The laptop, the TV and the phone share one body material. **RULED: the sharpest gap in the audit, and it must be fixed.**

`#3C3C38` on all three bodies collapses **the single most important constraint in the entire design
system.** The TV is a hardened display an institution bolted into the wall; the laptop is his own
machine — personal, chosen, cheaper, grubbier, possibly customised. That split is what the whole
two-register architecture rests on, and a shared material erases it in the one channel a player reads
without being told: material.

The room doc's own §6 says this in as many words. The room is currently contradicting it.

**Instruction.** Three distinct materials. The TV and its housing stay institutional — riveted steel,
thick chipped paint, stencilled equipment code. **The laptop diverges: cheaper plastic, warmer,
grubbier, worn where hands sit.** The phone is his too and follows the laptop's register, not the
TV's. This ranks above every remaining room polish item; it is not a dressing note.

## T56 — `Drab green #3A4230` appears nowhere in the room. **RULED: the room is wrong, not the doc.**

All four bunk/mattress materials are warm neutral greys. §2's palette names drab green for bunk frames
and mattress fabric, and the palette is ratified law under the 2026-07-28 instruction — olive, khaki,
**drab green**, rust, damp concrete. **Apply the swatch.**

Two cautions, because this touches a ratified measurement. Bunk 2's mattress is the **legible-as-
occupied test** and currently sits at 17.85 L\*, chroma 0.82 — **re-measure it after the material
change and hold the 43.9 mean-luminance requirement**; a hue change must not become a value change.
And do this **after T48**, or the grade's blue black point will misreport the new swatch exactly as it
has been misreporting the plaster.

## T57 — Five objects scale a unit primitive instead of building at true world size. **RULED: fix; same root cause as the collider strays.**

Four screen quads plus the stencil plate. §4's rule exists for a stated reason — scaling a unit
primitive stretches its bevel, so a thin wall and a chunky post carry visibly different edge widths.
The four screen quads are also where the four stray/interactable `MeshCollider`s live, so **T53 and
T57 are one piece of work on the same five objects.** Do them together.

---

## Ordering for the orchestrator

**T41 → T47 → T48 (T45 subsumed) → T43 → T46 → T55 → T53/T57 → T54 → T42 → T56 → T44 → T49/T50.**

T41 still leads: it invalidates the ladder every later TV tuning pass would be measured against.
**T47 unblocks B1 and no other work is waiting behind it, so it goes second regardless of size.**
T48 gates T42, T44's re-shoot and T56, because all three are colour judgements the grade is currently
distorting. T49 and T50 sit last by construction — they are re-checks that only become meaningful
once T41 and T46 land.

**Standing items 7 (constitution; laptop/TV owning docs) not addressed this pass** — they need a
clear session, and I would rather write them against a settled grade and a settled ladder than
against six open colour rulings.
