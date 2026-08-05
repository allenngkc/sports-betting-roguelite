# DD frame review — 2026-08-02 (postC14 Set B + r23) — VERBATIM + renumber map

**Orchestrator note:** the DD session that issued this had stale register state
(knew only batches 1–3; thought the room doc unapproved). Its measurements are
CURRENT — source is `tv-setB-postC14-part1..8` + `room-r23-conformance`, today's
drag — but its IDs T22–T27 were already spent by DD batch 4 / addendum. Canon
IDs assigned at transcription:

| Issued as | Canon ID | Subject |
|---|---|---|
| T22 | **T41** | Multiple L4 occupants — cap the stage (C3, BLOCKS TV Phase 3+) |
| T23 | **T42** | Team hues saturated + in scorebug type (§4) |
| T24 | **T43** | MARKET SUSPENDED on gold field — state lie |
| T25 | **T44** | Event-strip copy voice violation |
| T26 | **T45** | Death re-tint drains to navy — retarget olive (Law 1.1) |
| T27 | **T46** | Stage overdraws ticket column (§6, T21 class) |

T6 closes as Design-verified (the T26-canon "refusal expected to invert" — it
inverted). DD priority order in canon IDs: **T41 → T43 → T46 → T42 → T44 → T45.**
Original text follows verbatim.

---

# Register entries — 2026-08-02, batch 4

**Transcribe into `main-2/docs/design/REGISTER.md`.** Batches 1–3 already transcribed.

Source: `uploads/tv-setB-postC14-part1…8/` — the seated-camera Set B captures (2560×1440), plus
`uploads/room-r23-conformance/`. This is the rendered evidence T6's Design-verified was withheld
pending. **Everything below is measured off delivered frames, not asserted from source.** Method per
T19/T21 addendum.

---

## T6 — Phase 2 scene grammar. **DESIGN-VERIFIED. GRANTED. CLOSED.**

**State change:** Design-approved (structure) · visual half withheld → **Design-verified · DD 2026-08-02.**

Variation reads as variation at review scale. Across `scene002 BreakawayFor`, `scene003
BreakawayAgainst`, `scene004`/`005 GoalAgainst`, `scene006 NearMissHope`, `scene007 LegFinalLost` and
`CalmPossession`, the actor distributions and ball paths are distinguishable frame-to-frame and
scene-to-scene — cluster shape, ball track and final third occupancy all differ visibly. The
compose-don't-multiply model (T18) delivered legible variety from 19 authored pieces.

The lead's own gate-passes-without-delivery finding (T19) is now retired as a live risk for Phase 2:
these frames show rendered distinctness, not just signature distinctness.

**This closes T6 only.** The conformance defects below were found in the same frames and are separate
items — none of them is a scene-grammar fault.

---

## T22 — Multiple simultaneous L4 occupants. **C3 VIOLATION. BLOCKING TV Phase 3.**

The one-full-brightness law is the TV's single most load-bearing rule, and it is broken in the
delivered build. Measured brightest-pixel luminance per region, four sampled frames:

| Frame | Pitch region | Scorebug region | Cash-out band |
|---|---:|---:|---:|
| `scene003 …cashout-actionable frame000` | **#ffffff / 1.000** | **#f1f7fc / 0.923** | #ffd12e / 0.671 |
| `scene002 BreakawayFor goal frame000` | **#ffffff / 1.000** | #e6f2fa / 0.872 | #ffd12e / 0.671 |
| `scene004 GoalAgainst dangerous-0 frame000` | **#ffffff / 1.000** | #f1f7fc / 0.923 | #484e54 / 0.075 |
| `scene007 LegFinalLost resolved frame007` | #474747 / 0.063 | #434446 / 0.058 | #0c0d0c / 0.004 |

**Two findings, both from the same numbers.**

1. **At least two elements sit at or near full brightness simultaneously.** Pure `#ffffff` in the
   pitch and near-white in the scorebug coexist in three of four frames. §3 permits one.
2. **The designated L4 element is the third-brightest thing on its own surface.** When cash-out is
   *actionable* — the state my own component spec calls "the surface's only L4 element, a solid gold
   field" — it measures **0.671** against a pitch at **1.000**. The promise-about-input law does not
   fail because the band is dim; it fails because **everything else is brighter than the one thing
   the player can act on.**

**Instruction.** Cap the pitch. §7 is explicit: the pitch is a place, not an event — markings L1–L2,
actors L3, and **the ball is the only object permitted L4, and only at a payoff.** In these frames a
non-payoff ball and the actor dots are running at 1.000 continuously. Bring the stage under the
ladder and the cash-out band becomes the brightest element by construction, with no change to gold.

**Also recorded:** shipped gold is **`#ffd12e`**; the provisional token is `#F2BC45`. I am not
updating the token to match — the token is the intent, the frame is the deviation. The gold hex stays
open pending the same ratification as the rest of the TV palette.

---

## T23 — Team hues are neither muted nor confined to the pitch. **§4 VIOLATION.**

Scorebug team names render in saturated team hue at **luminance 0.87–0.92** — brighter than almost
anything else on the surface, and at full chroma (a saturated blue and a saturated orange; one scene
substitutes a saturated purple). §4 requires the team hues **muted**, **secondary to brightness**,
and **confined to the pitch dots**; identity is carried by the words in the ticket column.

Three consequences visible in the frames: the scoreline reads as the loudest information on a surface
whose loudest information is supposed to be money; hue is doing semantic work that brightness is
supposed to do; and the warmth budget §4 spends entirely on gold is spent a second time by the orange
team.

**Instruction.** Desaturate both hues to the muted values, drop them out of the scorebug type (cold
white for names, hue for dots only), and re-check that the two sides remain separable at four metres —
if they do not, the fix is **form** (filled vs hollow dot), never louder colour.

---

## T24 — `MARKET SUSPENDED` renders on a full-brightness gold field. **STATE LIE.**

In the delivered sequence the cash-out band appears as a **solid saturated gold field carrying the
words MARKET SUSPENDED**, before dimming to slate on a later frame (measured dark at `#484e54 /
0.075` in the `scene004` frame, so the dim state does exist and works).

This is the exact failure the slot was specified to prevent: **the brightness of this slot is a
promise about input.** A full-brightness gold field that says the market is suspended is the surface
promising an action it is simultaneously refusing. `suspended` is L1 unlit slate with no amount, from
its first frame — not after a settle.

**Instruction.** The transition into `suspended` must dim on the same frame as the label change. A
change that arrives early is a lie; so is a change that arrives late.

---

## T25 — Event-strip copy has drifted into celebration. **VOICE VIOLATION.**

`off the bar - a miracle brewing?!` is in the delivered frames. Lowercase open, hype noun, `?!`. Every
clause of it is on the banned list: celebratory gambling hype, manufactured excitement, and a
punctuation mark this system does not own. The event strip explains the last move; it stays neutral
**even when the event helps**, because money semantics live on the leg rows and the cash-out slot.

The same build contains compliant lines — `Overheads poke one in at the near post. Ugly.` is exactly
right: dry, literal, faintly contemptuous. The register exists in the corpus; this line left it.

`Turnips break the line and finish - the crowd loses it.` is borderline and should be trimmed to its
first clause. The crowd is not a revealed fact.

**Instruction.** Audit the authored event lines against CONTENT FUNDAMENTALS and replace any line
carrying an exclamation, a superlative, or a promise. Also normalise the dash: the corpus uses an em
dash, these use a hyphen.

---

## T26 — The room re-tints to navy on a dead leg. **LAW 1.1 FAILURE MODE.**

Measured room background: **`#101f20` / `#162621` during play** (dark green-teal) → **`#0e121d` on the
`LegFinalLost` frames** (cool navy). The whole room shifts, not just the panel spill.

Two separate problems, and they must not be conflated:

- **The mechanism is arguably right.** C5 (room re-tint from TV light in-engine) was left open
  deliberately, with big payoffs as the trigger. A leg dying is a big payoff. If this is C5 landing,
  the *idea* is sound and I endorse it.
- **The colour is the one the palette law forbids.** Law 1.1: wall albedo is warm dirty plaster and
  **the room physically cannot return saturated cool colour; a blue-tinted room is the explicit
  failure mode.** Green-teal during play is the shipped `TvLight` green (C2 interim, tolerated).
  Navy on death is not tolerated by anything.

**Instruction.** Keep the re-tint, change its target. Death should drain the room toward its own
darkest **olive** (`#0F1108`), not toward blue. Loss is darkness, not a colour change — and certainly
not a cool one. Cross-check against `room-law-1-1-grade-finding` before implementing; if that finding
and this measurement disagree, the room lead's own instrumentation wins.

---

## T27 — Ticket-column text is overdrawn by the stage. **LAYOUT DEFECT.**

In the delivered frames the scoreline and the pitch panel are drawn **over** the ticket column's leg
text: `W Pawtucket Turnips ML — Scranton Regulators v Pawtucket Turnips` is struck through by
`MIDDLEMEN 0—1 OVERHEADS`, and `BIFF RACKET TO SCORE` is cut at the column boundary mid-word.

Same root cause as T21, one surface further out: **a fixed-canvas region is being sized by its
content instead of by the grid, and the overflow is landing on top of a neighbour.** §6 requires every
zone position to come from an explicit fixed grid defined once in code, with no zone resizing in
response to content.

**Instruction.** The ticket column owns its width absolutely; the stage clips to its own region and
never paints outside it. Long leg identities compress inside the column per T21's order — statement
first, never the price or the state. Assert it: for every frame, every ticket-column leaf's right edge
is inside the column, and no stage pixel is left of the column's right edge.

---

## Priority for the orchestrator

**T22 blocks TV Phase 3** — it invalidates the brightness ladder the whole surface is built on, and
every subsequent tuning pass done against an uncapped stage will be wrong twice.

Order: **T22 → T24 → T27 → T23 → T25 → T26.** T17 (scorer gap, batch 3) still ranks above every
Phase 3 *visual refinement*, but below T22, because T22 is a violation of the surface's constitutive
law rather than a defect within it.
