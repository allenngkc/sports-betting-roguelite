# Register entries — batch 106 (2026-08-17)

**Written at the DD seat**, during the hold on Allen's calls. **Read at this seat on frames already
docked — no capture was commissioned.** **Destination table:** TV — match theater (`T95-cl`, `T114`,
`G1-am`).

Evidence: `dd-import/tv-t95-transitions-2026-08-13/`, frames `…t68am-accept-slot__frame008` and
`__frame029`.

---

## `T95-cl` — CLOSED, on the exact frames it was named on

`SPREADSHEETS 0 — 0 MUSKRATS` **renders as one string. No offset copy, no doubling**, at 11' and
again at 15'. Read at review distance on the fixed tree, on the same frame the defect was originally
read on — **identical, not analogous.**

**The cause was found and it was the TV lane's own:** `Score` and `_tMatchup` both `UpperCenter` in
different boxes — 593.0/centre 92.7 against 675.0/centre 133.7, **delta 41.0px, exactly
`scoreCentreShift` from `T91-am`.** *The ruling was sound and the implementation did not re-derive the
mirror when the box moved.* Fixed by construction (both boxes 621.0, both centres 88.7, delta 0.0) and
**pinned by an assertion** rather than a convention.

**The sweep listed this as open and needing "a measured close." It was fixed, merged (`da1b5fa`),
pinned and re-captured on 2026-08-13** — three days before the sweep read it.

**Not settled here, named so absence is not read as coverage:** these are room-graded camera captures,
so **no pixel claim about the scorebug's left edge is available** — consistent with
`count-sweat-read` §8's own refusal. At review distance the ticket column's `W` chip and the
scoreline's first glyph read as separate registers and do not merge; looked at, not raised.

## `T114` — the cashed-out ticket's footer, and the state my spec never tabled

**On both frames the footer prints `RISK $87` and `PAYS $1,490` directly above a banner reading
`CASHED OUT $199`. Held across all thirty frames, 11' to 15' — not a transient, checked before it was
claimed.**

**Both words are false.** `RISK $87` — the position is closed, nothing is at risk. `PAYS $1,490` — it
will never pay that; the player took $199. **`T108`'s principle verbatim:** *no word may name a
jeopardy or a payout that no longer exists.*

**§1.5 — the omission is mine.** The spec's §2 **named** `RevealedTicketState { Riding, Won, Lost,
CashedOut }` and then clause 2's table covered **three of its four members.** I quoted an enum and
tabled part of it. **Naming a state is not ruling it** — the same error shape as `T108-am`'s *naming
a field is not reading it*, one week and one clause apart.

**And the build cannot express it, structurally:** `StakeWord` takes leg outcomes, and **a cash-out is
a player action not derivable from leg outcomes at all.** That is not a flaw in the lane's build,
which is correct for the case it was given — **it is the consequence of a table with a missing row.**

**Ruled: clause 2's table gains its fourth state.** The exact strings are copy and `C11` authors copy
on a frame — **the frame now exists**, so unlike §5's dead ticket this one is not blocked.

**A third stale word on the same frame, routed with it:** the cancelled legs still read `NEXT`, and
`T25.6` defines `NEXT` as *"the next thing that can take his money."* After a cash-out nothing can.
No strike is visible, but a thin rule may not survive this grade — **the strike is not judged here;
the word is legible and it is false.**

## `G1-am` — the scorer residual re-scoped, and the seed is known

**The sweep says `{SURNAME} SCORES` is "still unmeasured across the twelve surnames." That half was
discharged at batch 63** — the build comment states it: *measured, all twelve against 261.0; rung 2
overruns for none; `PAVEMENT SCORES` is 238.4px with 22.6px spare.* Only the **rendered read** is
genuinely owed.

**And its cost just collapsed.** Rung 2 is reachable on one surname in twelve, so the frame needs a
run whose backed scorer is `PAVEMENT` — which reads as a seed *search* and is not one: **the docked
set shows `PAVEMENT ANYTIME` on seed `48151623`, verified at this seat on frame.** Known seed, already
pinned, already used by a docked entry point, and the harness has `scorer-leg-dangerous` moments. On
those frames the leg is `NEXT`, which renders the compact form — **so the existing set does not
already contain the shot.**

**Ruled: it does not earn its own capture window. It rides the next TV capture carrying a scorer
leg**, pinned to `48151623` at a moment the leg is live.

## The pattern — four for four, and it changes how the sweep is worked

`C26` already closed · `S80-am2` clause 7 half discharged · `T95` fixed three days early · `G1`'s
first clause discharged at batch 63. **Every one falls in the direction the sweep itself declared as
its blind spot.**

**The sweep reports an upper bound on what is open. It is not a worklist**, and every item must be
verified against its row's tail and against source before work is done against it. A warning block to
that effect is now at the head of the sweep document.
