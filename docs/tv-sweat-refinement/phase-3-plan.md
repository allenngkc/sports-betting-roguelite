# Phase 3 — UI refinement: plan

**Lead:** TV sweat (Claude Opus 5) · **Written:** 2026-07-31 · **Baseline:** `842382d`
**Suites at baseline:** engine 160/160 · EditMode 129/129 · PlayMode 44/44
**Unblocked by:** C1 ruling (layout closed, `DESIGN.md` §6 governs) + the ownership contract landing
(`docs/handoffs/tv-sweat.md`, at the repo root as `handoff.md` until 2026-07-31)

---

## 1. What Phase 3 is

Phase 2 made the *stage* tell the truth in a varied way. Phase 3 makes the *screen around it* legible.
Everything the seven concept rounds settled — Layout B, the palette, the brightness ladder, the state
vocabulary — exists only as specification. **No production pixel has been built against it yet.**

That is the shape of the risk: this is the first phase where the design and the code have to meet.

## 2. Exit gate (PRD §5, verbatim) and how each is proven

| Gate item | Proof | Headless? |
| --- | --- | --- |
| Six information priorities readable from the couch, in order | Muted seated capture, read aloud in order | **No — GPU** |
| Every supported market has correct `NEED` and revealed-progress copy | EditMode, table-driven over all 8 market/side combinations | Yes |
| Eight states never reuse contradictory colours or labels | EditMode over the state matrix, asserting token + label uniqueness | Yes |
| Ticket changes never show stale score, count, callout, offer, tape, active-leg | PlayMode multi-ticket transition tests | Yes |

**Three of four close headless. The first cannot** — and it is the one the whole phase is for.

## 3. Work breakdown

Sequential where noted; everything touches `TvSweatScreen.cs`, so parallelism is limited to at most
two agents and only on disjoint files.

### 3A — `SweatActiveLegModel` (PRD §9): pure market copy
New file. Extracts `NEED` / `LIVE` progress formatting for all eight markets out of the orchestrator.
Pure, EditMode-testable, no scene. **Closes gate item 2.** Highest value per unit of risk — it is the
one piece that can be fully proven without a frame.
*Depends on nothing. Start first.*

### 3B — Palette debt: T9 + T10
`chromeCyan` retired to grey per `DESIGN.md` §4 (cyan has no role in the approved palette). The two
hardcoded emission rest values at `TvSweatScreen.cs` corrected to the agreed black floor — one is
currently *darker* than the floor, locally undoing the lift whose entire purpose is that nothing sits
below the panel's off state.
*Small, mechanical, independent of 3A. Can run alongside it.*

### 3C — Layout B build
The fixed layout grid, defined once in code, zone positions never computed from content. `DESIGN.md`
§6 is explicit that this discipline is what PRD §8.1's stability now rests on, since the retired LED
matrix used to give it for free. Ticket column at 26–28%, hairline rules or unlit gutters, no stroked
boxes, no zone resizing.
*Depends on 3A (the column renders what the model formats). The largest single piece.*

### 3D — State vocabulary
**Six** cash-out states in one non-reflowing rectangle; five leg states; the brightness ladder with
**at most one L4 element at any instant** — already enforced structurally by the HDR material being
given to only three graphics, which 3C must not weaken. **Closes gate item 3.**
*Depends on 3C.*

> **Corrected 2026-07-31 (T20/3D): this line said EIGHT cash-out states.** The cash-out rectangle
> holds **six** — PRD §8.5 lists six, PRD §14.3 says "all six states in §8.5", and `DESIGN.md` §6 and
> §8 both say six.
>
> The "eight" was real but attached to the wrong thing. PRD §5's Phase 3 exit gate names eight states
> across two different surfaces: *"Open, suspended, unavailable, pending-window, cashed-out, won,
> lost, and void states do not reuse contradictory colors or labels."* Five of those live in the
> cash-out slot and three are leg outcomes. Collapsing them into "eight cash-out states" is what
> produced a count no document supports.
>
> **Also note what the gate does NOT ask for.** Suspended and pending-window share one treatment
> deliberately (`DESIGN.md` §8: pending window is "As suspended"), so a uniqueness test over the eight
> would fail on a pair the design intends. The gate word is *contradictory*: a state that promises
> input must not wear the treatment of one that refuses it. `The_eight_gate_states_never_contradict_
> one_another` asserts that, not uniqueness.

### 3E — §8.8 stats panel and §8.10 held cash-out preview
The two authorized mid-sweat verbs. Both freeze playback per §4.4. The preview renders one brightness
level down and uses the `VOID` strike rather than the `LOST` extinguish, because legs being *cancelled*
must not read as legs *lost* at the exact moment a player is deciding.
*Depends on 3C + 3D.*

### 3F — §7.7 backed-player locator
See §4 — this is one of the two items that cannot close headless.
*Depends on 3C. Gated on visual evidence.*

## 4. The two PENDING-VISUAL-EVIDENCE items

Both were flagged as unprovable headless. Neither is a "run it and see" — each needs a *specific*
capture answering a *specific* question, so the plan states the question rather than hoping the
screenshot is self-evident.

### 4.1 TVS-H03 locator binding (§7.7)

**What is proven:** the data. `ScoreLedger.BindAnytimeScorer` binds the backed player's side and
roster index onto the committing final-plan goal at plan time; `ScorerFor` (copy) and `EnterStep`'s
`RoutePass` case (routing) read the same three fields off the same struct. Tests pin that the named
player and the routed actor are one identity.

**What is not proven:** that a human watching the screen sees the named player take the touch.

**Evidence procedure:**
1. Seated camera, muted, anytime-scorer leg, GPU session.
2. Record the final sequence. **Do not look at the name first.** Watch the pitch, note which dot takes
   the final touch, then read the reveal copy.
3. Pass = the dot you tracked is the player named. Fail = you cannot tell which dot took it — which is
   a *legibility* failure, not a binding failure, and routes to the Design Director, not to a code fix.
4. Repeat across both home and away backed sides; the binding is side-symmetric and the mirror is
   where a side-swap bug would hide.

**Why it must not be closed by assertion:** the data being right and the moment being readable are
different claims. Phase 1A's original finding was that identity was *fictional* — cosmetically
correct, causally unconnected. A screenshot proving the label matches the log would reproduce exactly
that error one level up.

### 4.2 Scorer-reveal gap

**The defect:** if a won anytime-scorer leg's backed-side goals are all spent before the final
sequence begins, **no reveal fires** — the player wins and never sees the moment. Pre-existing, not a
regression, deferred from Phase 1B by name.

**Why it is not a code fix yet:** closing it needs the whole-sweat identity contract §7.7 defers. The
alternative — manufacturing a reveal — would move the causal reveal point and break §4.1.

**Evidence procedure:**
1. **Headless first, and this part can be done now:** a PlayMode test that constructs the exact
   condition (won scorer leg, backed-side goal quota exhausted pre-final) and asserts reveal count is
   zero. That converts "we believe this case exists" into a pinned, reproducible fact and gives any
   future fix a red test to turn green.
2. **Then GPU:** run that seed seated and confirm what a player actually experiences — whether the win
   lands with no scorer moment at all, or whether the leg-settle ceremony already carries enough that
   the gap is invisible.
3. Step 2 decides severity, and severity is a **Design Director** call, not this lead's. It may be
   that a scorer bet winning silently is acceptable; it may be that it guts the market's whole appeal.

**Sequencing note:** step 1 is cheap, headless, and independent of the rest of Phase 3. It should run
early so the question reaching the Design Director is *"here is the case, reproduced"* rather than
*"we think this can happen."*

## 5. Visual-evidence blocker, stated plainly

**This worktree cannot produce any GPU capture.** `-nographics` rasterises no frame. Every item in §4,
plus gate item 1 and PRD §6.1.1's entire `PENDING-VISUAL-EVIDENCE` class, needs a session this
worktree does not have.

**Ask to the orchestrator:** schedule a GPU-backed interactive session, or name who owns one. Without
it Phase 3 can reach "all headless gates green" and still cannot honestly close, because its defining
gate — *readable from the couch* — is a human-eye claim.

The same session serves Phase 4's three muted couch sweats, so it is one booking, not two.

## 6. Risks

1. **Layout B has never been rendered in-engine.** It exists as five greybox concepts and seven
   direction renders. First contact between spec and Unity canvas is 3C, and it is where any
   unstated assumption in the brand book will surface.
2. **The one-L4 rule is currently enforced by construction** — only three graphics carry the HDR
   material. 3C rebuilds the canvas and could quietly widen that. Guard it with a test before 3C, not
   after.
3. **`SweatActiveLegModel` extraction touches the orchestrator's formatting paths**, which the
   cash-out tween tests exercise. Expect the documented flake to appear; judge on pattern, and hold to
   PRD §6.1's ≥10-run rule on both arms before calling any regression. This lead got that wrong once
   at n=3.
4. **Design rulings are in flight** — C3 coverage and the art-authority gap. 3D's brightness work is
   the piece most likely to be affected by the C3 answer. Sequence 3D after that ruling if it is close.

## 7. Proposed order

`3A` + `3B` in parallel → `4.2 step 1` (cheap, headless, unblocks a Design Director question) →
`3C` → `3D` → `3E` → `3F` + GPU evidence for §4.

Two agents maximum, one at a time on `TvSweatScreen.cs`.
