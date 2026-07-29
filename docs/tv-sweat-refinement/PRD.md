# TV Sweat Refinement PRD

**Status:** `APPROVED WITH CHANGES` by Allen, 2026-07-24 — see §13 for the decision ledger  
**Product owner:** Allen  
**Technical product lead / reviewer:** Claude Code, Opus alias  
**Execution model after sign-off:** supervised Claude Sonnet 5 agents; the technical product lead may
dispatch at most **2 concurrent** Sonnet 5 agents, reasoning effort chosen per task by the lead  
**Baseline reviewed:** commit `d665438` on local branch `tv-sweat`  
**Requested implementation branch:** `slice/tv-sweat-refinement` (not present at the design gate)  
**Visual design:** [VISUAL-DESIGN.md](VISUAL-DESIGN.md)  
**Audit ledger:** [BUG-LEDGER.md](BUG-LEDGER.md)
**Current-state source audit:** [current-state-audit.md](current-state-audit.md)

## 1. Product outcome

Make the muted TV sweat feel like a fluid, varied, trustworthy match broadcast while keeping
the bet—not a simulated sport—as the star.

The player must be able to look at the TV and answer, in order:

1. Who is playing, and which team or market did I back?
2. What is the score and match time?
3. What still has to happen for this leg to win?
4. How much is at risk, and what does the ticket pay?
5. Can I cash out right now? If not, why not?
6. What event just changed the price?

Success means three different-market sweats can be watched from the couch, with audio muted,
without stuck playback, a false visual event, score/count disagreement, or a sequence that feels
like the same move replayed with a different lane.

## 2. Why this refinement exists

The existing theater look and broad feel were approved in playtests #10–#16. This is a
refinement, not a reset. The current implementation also exposes specific limits:

| Current evidence | Product implication |
|---|---|
| `ScenePlaybook` has 16 templates and `VariantCount = 3`. | The audit must exercise 48 template/variant cells before scene work starts. |
| `TheaterStage.BuildBeatScript` changes most variants through `Lane(variant)`—center, upper lane, lower lane—while retaining the same path and payoff. | Increasing the count would multiply positions, not movement ideas. Variation must be modeled as grammar, pressure, payoff shape, and reaction. |
| `TheaterStage` and `TvSweatScreen` are approximately 1,328 and 2,173 lines. | New behavior belongs in focused helpers; this work must not become a wholesale rewrite. |
| Existing stage PlayMode coverage plays 14 non-final templates with variant `0`, plus narrower final/pending tests; current TV flow coverage is broad but shallow. | Every current variant, both final templates, and the risky transition matrix need explicit audit and regression coverage. |
| Dangerous scenes suspend cash-out until the payoff and reopen it during the tail/gap. | The refined UI must preserve causal pricing while making open versus suspended states unmistakable. |
| The display already owns causal score and count ledgers. | New motion and UI must consume the same revealed facts; it may not create a second truth source. |
| Staff source review found different predicates for cash-out input reservation and legal acceptance. | Phase 1A must reproduce suspended/updating Interact behavior before any UI rework. |
| Core scene motion freezes when standing, but several ceremony, animation, and transition timers do not consult seating. | The audit must test literal pause across visible states, not only the event cursor. |
| The final scorer reveal names a player without binding that actor to the final touch. | Scorer identity requires an actor-level truth diagnostic and win/loss regression. |
| Playtest #16 parked the procedural-audio revisit. | Audio is explicitly deferred and cannot be used to rescue visual readability. |

These are baseline observations, not a claim that every suspected issue is a reproduced bug.
Only the audit ledger can promote a hypothesis to a confirmed defect.

## 3. Scope

### In scope

- A systematic, evidence-backed bug audit before feature work.
- Reliability fixes for confirmed TV/theater defects.
- Deterministic scene-planning variety:
  - central buildup;
  - wing progression;
  - switch of play;
  - counterattack;
  - through-ball, cross, cutback, rebound, and set-piece goal shapes;
  - block, interception, keeper save, clearance, post, and near-wide endings;
  - near-post, far-post, and cleared corner sequences;
  - pressure, spacing, and reaction variation.
- Market-attributed corner and card scenes, so the team that wins a corner or commits a foul is
  visible on the stage (§7.6).
- A backed-player locator for the anytime-scorer market (§7.7).
- A clearer TV information hierarchy and market-specific active-leg status.
- Competing TV layout concepts and a project brand book, produced in the visual-design track (§14)
  before the Phase 3 UI build.
- Smoother, state-safe ticket, final-leg, cash-out, pending-window, and settle transitions.
- TV/theater tests, audit diagnostics, and focused TV-specific helper classes.
- Presentation-only determinism and truth-contract tests.

### Explicitly out of scope

- Audio changes of any kind. `TvAudioDirector.cs` is untouched.
- Engine changes, balance changes, RNG-stream changes, or new sports outcomes.
- New bet types. **Amended 2026-07-25:** exactly one new mid-sweat verb is authorized — the match
  stats panel in §8.8. **Amended 2026-07-26:** a second is authorized — the held cash-out preview in
  §8.10. These two are the complete list; no further verb may be introduced in this pass.
- A full sports simulation, physics model, or AI opponent.
- `Room.unity`, room geometry, camera rig, couch position, or environment art.
- Laptop/SureThing changes.
- A complete `TvSweatScreen` or `TheaterStage` architectural rewrite.
- New camera cuts, rotation, or pitch-direction flips in this pass. Decision B is approved: the
  fixed top-down frame stays.
- **Refinement v2 candidate, explicitly deferred:** a FIFA-style camera that follows the ball
  carrier (Allen, 2026-07-24). Not designed, prototyped, or costed in this pass.
- **Deferred out of this worktree (Allen, 2026-07-27):** the bunkmate character — an occupant of the
  second bunk who occasionally gives the player advice or charms. Recorded in `PRODUCT.md`; the room
  lead is told to keep that bunk dark and suggestive, which does not foreclose the idea either way.
- **Deferred out of this worktree (Allen, 2026-07-25):** a degrading visual register, where the TV
  surface grows louder and more desperate as the run goes bad, mirroring the room-state health bar
  in `design/08-art-direction.md`. The register for this pass is **expensive and slick**, held
  constant. The degradation ladder is a strong idea and is parked deliberately, not rejected.

## 4. Product laws

### 4.1 Truth before drama

The presentation may elaborate an engine beat but may not contradict it.

- A goal visual requires a staged goal.
- A score changes only at that goal’s visible payoff callback.
- A corner or booking visual requires a staged count with a positive delta.
- A near miss never becomes a goal.
- A possession scene never shows a goal, booking, corner award, or decisive whistle.
- A chalked goal may cross the line, but the “NO GOAL” treatment and unchanged score must land
  together.
- A final scene must converge to the locked score/count endpoints through visible callbacks.
- Anytime-scorer identity may appear only at its causal reveal point and must belong to the actor
  that takes the visible final touch.

### 4.2 One revealed source of truth

Stage, scorebug, active-leg card, momentum tape, event callout, cash-out state, and
`RevealedView` must derive from the same revealed presentation facts. No helper may infer a
hidden outcome from the locked stat line earlier than the existing causal reveal permits.

### 4.3 Deterministic presentation, isolated from engine RNG

Every discrete choice introduced or changed here—movement grammar, lane, pressure, spacing,
payoff shape, actor routing, and reaction pattern—must be derived from a presentation key:

```text
run seed + round + ticket index + match index + event step + scene template + beneficiary
```

**Amended 2026-07-27: `leg index` → `match index`.** The original formula predates §8.2A. A scene beat
belongs to the *match*, not to any one leg, so with two legs live on one match a leg-scoped key would
hash the same beat two different ways — or pick a leg arbitrarily and go unstable as legs settle
underneath a still-live match. `Leg.Matchup.Index` is shared by every leg referencing that matchup, so
concurrent legs necessarily construct an identical key. Ticket index alone is insufficient because a
multi-match parlay carries several matches per ticket.

Corroborated by an existing engine pattern, verified in source: `RngHub.Derive(round, ticketId,
legIndex, action, ordinal)` at `engine/RngHub.cs:56` and `RngHub.DeriveMatch(round, matchupIndex,
purpose)` at `:61` already treat "which match" and "which leg's wager" as separate axes.

Leg identity remains real for leg-*specific* presentation — per-leg `NEED`/`LIVE` copy, the §7.7
locator. Those fold the leg index into the **channel name** they query rather than into shared key
material, so the base key stays match-scoped.

### 4.3.1 CLOSED 2026-07-28 — the event stream is leg-scoped

**Resolved. Phase 2B is unblocked.** The investigation in `concurrent-legs-investigation.md` found
that the engine forbids two legs on one matchup (`engine/Run.cs:181-182`), so leg-scoped and
match-scoped event steps are currently identical and `event step` in the key is unambiguous.

None of the three options below were taken; a fourth was — §8.2A is reclassified as a future feature
with a betting-math dependency, and this slice carries a tolerance constraint rather than
concurrency plumbing. The analysis below is retained because it becomes live again the moment
same-match legs are ever enabled.

### 4.3.1a Retained analysis — the event stream is leg-scoped (2026-07-27)

Amending the key removes *which leg* from key identity. It does not remove the deeper problem.

**`DramaEvent.Step` is documented at `engine/DramaEvent.cs:20` as "1-based step within the leg,"** and
`DramaEvent.LegIndex` sits beside it at `:18`. Each leg gets its own independently-stepped event
stream. So under §8.2A, two legs live on one match do not currently share a notion of *what the same
beat is* — and `event step` in the key above is fed from a per-leg counter.

**Consequence: §8.2A is not fully deliverable inside this slice's file boundaries.** A shared
per-match beat requires either an engine change or a presentation-layer merge. `engine/**` is §11
forbidden.

Options, none chosen:

1. **Engine change** — make the event stream match-scoped so all legs on a match share one cursor.
   Cleanest and correct, but crosses the boundary and needs Allen's authorization plus an owner.
2. **Presentation-layer merge** — the TV slice deterministically merges concurrent legs' streams into
   one match timeline. Stays inside the boundary but is complex, and risks contradicting engine facts,
   which §4.1 forbids.
3. **Scope §8.2A down** — concurrent legs are *displayed* together in the ticket column, but playback
   still follows a single leg's stream per match. Cheapest; needs a ruling on whether that satisfies
   the intent.

**This gates Phase 2B.** The planner keys off event step, so its semantics must be settled before the
planner is built. Investigation needed first: establish what the sweat actually does today when one
ticket carries two legs on the same match — whether it plays the match twice, or whether current
ticket generation simply never produces that case.

Requirements:

- No `RngHub` access and no engine RNG draw.
- No `UnityEngine.Random`, wall clock, or `Environment.TickCount` for a discrete scene choice.
- Use stateless, named hash channels (`grammar`, `lane`, `pressure`, `payoff`, `reaction`) so
  adding a new channel does not reshuffle existing channels.
- The same key and revealed history produce the same scene signature.
- Frame interpolation may vary with frame rate; the chosen story and its factual callbacks may not.
- Selection reads only already-revealed history. It cannot inspect future beats to improve variety.

### 4.4 Pausing is literal

Standing up freezes the exact presentation state: event cursor, scene step, ball, actors, clock,
probability animation, cash-out animation/offer, callout lifetime, resolution effect, transition,
and pending-window timer/state. Sitting resumes from that state. No hidden catch-up is allowed.

### 4.5 Stable direction and couch readability

The picked team continues to attack right. The camera remains fixed in this pass. Variation comes
from movement, formation, pressure, and payoff—not from making the player re-learn screen direction.

### 4.6 Audio independence

Every required state and payoff must read with master audio muted. No acceptance result may depend
on a whistle, sting, crowd swell, or spoken line.

## 5. Delivery phases and gates

### Gate 0 — design sign-off (this package)

Status as of 2026-07-24: **partially met — Phase 1A is unblocked, Phase 3 is not.**

| Requirement | State |
|---|---|
| PRD approved | ✓ `APPROVED WITH CHANGES`, changes applied in this revision |
| Fixed-camera recommendation approved or explicitly changed | ✓ Decision B approved |
| Audit severity definitions and evidence standard approved | ✓ Approved with the PRD; §6 unchanged |
| Target branch decision resolved | ✓ Decision F approved; `slice/tv-sweat-refinement` created at Phase 1A start |
| Visual hierarchy and state treatments approved | ✗ **Open.** Decision A requires competing layouts and a brand book first (§14) |

Phase 1A audits existing behavior and consumes no layout decision, so it may begin. Phase 3 may not
begin until the visual-design track closes Decision A.

### Phase 1A — bug audit only

The execution agent systematically runs the matrix in [BUG-LEDGER.md](BUG-LEDGER.md). It may add
diagnostic-only test seams if approved in the implementation brief, but it does not begin the
scene-variety or UI redesign.

Exit gate:

- All 48 current template/variant cells executed.
- Every required market and transition scenario executed.
- Every observed defect has a complete ledger row.
- Visual defects have a screenshot; timing/motion defects have a short video when a still cannot
  show the failure.
- Reproduction rate is measured, not guessed.
- Candidate fixes and practical regression tests are attached to the ledger.
- Source-confirmed candidates `TVS-H01` through `TVS-H03` are reproduced with full context or
  rejected with recorded counter-evidence.

### Phase 1B — bounded reliability repairs — **CLOSED 2026-07-27**

Signed off by Allen. Four confirmed defects fixed and reviewed against source: `TVS-H01` cash-out
input reservation, `TVS-H02` standing freeze (twice — the first fix left a coroutine handoff race
measured at 6 failures / 10 full-suite runs), `TVS-S01` corner and card team attribution, and
`TVS-H03` anytime-scorer identity binding. Each carries a regression test where the behaviour is
mechanically observable. Suites at the fixing commit `e2f4fc0`: engine 160/160, EditMode 88/88,
PlayMode 30/30.

**Gate item explicitly waived by Allen:** *"the audit is rerun against the fixing commit."* Not
performed. Recorded as a waiver rather than an omission. Partial mitigation: Phase 2's own exit gate
requires all 48 legacy template/variant cells to still complete and reveal exactly once, which
re-exercises most of the matrix against later code.

**Two items carried forward, on the record:**

1. An environmental test flake — roughly 2 in 10 full PlayMode runs fail `never observed the cash-out
   amount mid-tween` on abnormally slow runs (52–54s against a ~35s norm). Test-side timing
   fragility, not a product defect. Logged in `BUG-LEDGER.md` §4C.4.
2. **The scorer-reveal gap.** If a won anytime-scorer leg's backed-side goals are all spent before
   the final sequence begins, no scorer reveal fires — the player wins without seeing the moment.
   Pre-existing, not a regression. Closing it requires the whole-sweat identity contract that §7.7
   defers; the alternative moves the causal reveal point and breaks §4.1. **Severity: major, deferred
   to §7.7's work in Phase 3.**

### Phase 1B — original gate definition

Fix confirmed blockers and majors first. Polish defects may be combined with later scene/UI work
only when the ledger links the owning requirement.

Exit gate:

- No open blocker.
- No open major on pause/resume, causal reveal, cash-out state, endpoint convergence,
  multi-ticket transition, final-leg transition, or settle completion.
- Each fixed blocker/major has a regression test where the behavior is mechanically observable.
- The audit is rerun against the fixing commit.

### Phase 2 — scene variety

Add deterministic scene planning and new movement grammars without changing engine facts or
pacing law.

Exit gate:

- Truth-contract tests pass for every grammar/payoff combination.
- No identical scene signature appears on adjacent non-structural beats when at least two valid
  candidates exist.
- No identical movement grammar appears more than twice in a rolling four non-final scenes when
  the event constraints permit alternatives.
- All 48 legacy template/variant audit cells still complete and reveal exactly once.
- Same presentation key reproduces the same scene signatures in two runs.
- Engine outcome golden pins remain byte-identical; no engine file or DLL changed.

### Phase 3 — UI refinement

Implement the signed-off hierarchy in [VISUAL-DESIGN.md](VISUAL-DESIGN.md), using a focused
market-status helper and stable state slots.

Exit gate:

- All six information priorities are readable from the couch in the required order.
- Every supported market has correct “need” and revealed-progress copy.
- Open, suspended, unavailable, pending-window, cashed-out, won, lost, and void states do not
  reuse contradictory colors or labels.
- Ticket changes never show stale score, count, callout, offer, tape, or active-leg status.

### Phase 4 — integrated acceptance

Run automated gates, the audit rerun, and three full muted couch sweats across different market
families. Record all three in the ledger.

## 6. Audit protocol

### 6.1 Evidence standard

Every bug row contains:

- bug ID and build commit;
- run seed;
- round;
- ticket index and leg index;
- market and selection;
- scene template and current variant when relevant;
- playback state;
- expected behavior;
- actual behavior;
- reproduction rate as `failures / attempts`;
- screenshot or video path for visual/motion defects, subject to the harness policy in §6.1.1;
- severity: blocker, major, or polish;
- regression test name or a concrete reason a test is impractical;
- owner, status, fix commit, and verification result.

For a static hypothesis, seed/round/ticket/market are `NOT CAPTURED — STATIC REVIEW`, reproduction
is `NOT RUN`, and status is `HYPOTHESIS`. A hypothesis is not counted as a bug or gate failure.

Reproduction rules:

- Deterministic scene-harness failure: rerun at least 3 times.
- Timing, input, pause, or transition failure: at least 10 attempts.
- End-to-end seed failure: replay the same seed 3 times, then one neighboring seed where practical.
- Report both numerator and denominator. “Sometimes” and “always” are not valid rates.

### 6.1.1 Harness reality and the split evidence policy (Allen, 2026-07-24)

Phase 1A established what this environment can actually do. The PRD previously assumed no Unity or
runtime execution was available; that assumption was wrong.

**Executable here, verified:** engine tests (160/160), Unity EditMode (73/73), and Unity PlayMode
(20/20), including PlayMode tests that drive `TheaterStage` scene playback. The 48-cell matrix in
§6.2 is therefore executable.

**Not executable here:** Unity runs with `-nographics`, so no frame is ever rasterized. Screenshot
and video capture are impossible in this harness regardless of effort.

**Approved policy — split by defect class:**

| Defect class | Required evidence |
|---|---|
| Pause/resume, timing, input, state-machine, ordering, endpoint convergence | Pass/fail PlayMode or EditMode evidence is **sufficient**. These are mechanically observable; a screenshot adds nothing. Cite the test name and real result. |
| Anything about what is *drawn* — scene attribution (§7.6), the backed-player locator (§7.7), layout, colour, legibility, visual payoff correctness | Pass/fail is **not sufficient**. These rows are marked `PENDING-VISUAL-EVIDENCE` and routed to a GPU-backed interactive session. They must be cleared before Phase 1B closes. |

A row carrying `PENDING-VISUAL-EVIDENCE` is not a reproduced defect and is not counted as a gate
failure, but it also may not be closed as fixed.

**Standing build hazard.** Both `dotnet test engine.tests` and every Unity invocation mutate tracked
files as build/import side effects, including `SBR.Engine.dll` and several `ProjectSettings`/
`Settings` assets — all of them §11 forbidden files. This is a property of the build wiring, not an
agent error. Every dispatched agent must revert these with `git checkout --` after running tests,
and the reviewer verifies a clean tree before accepting any phase.

### 6.2 Current scene matrix

Run variants `0`, `1`, and `2` for:

- GoalFor / GoalAgainst;
- BreakawayFor / BreakawayAgainst;
- TerritoryFor / TerritoryAgainst;
- NearMissHope / NearMissScare;
- CalmPossession;
- LegFinalWon / LegFinalLost;
- Kickoff;
- Fallback;
- CornerFor / CornerAgainst;
- Booking.

Each cell verifies:

- starts;
- completes within its timeout;
- reveals exactly once;
- produces only its permitted payoff marker;
- leaves possession and actor state valid;
- survives stand/resume at early, pre-payoff, payoff-tail, and late positions;
- does not move score/count before the visual payoff;
- leaves the next scene able to start.

### 6.3 Required market and transition matrix

The audit must explicitly cover:

- goals and chalked goals;
- breakaways;
- calm and pressured possession;
- all near-miss endings;
- corners, including positive batches;
- bookings, including positive batches;
- moneyline;
- total goals Over and Under;
- both-teams-to-score Yes and No;
- total corners Over and Under;
- total cards Over and Under;
- anytime scorer won and lost, with identity verification;
- cash-out open → suspended → reopened;
- cash-out accepted during a legal open window;
- no cash-out acceptance during suspension or price animation;
- Mulligan → Void;
- Whistle → Won;
- Whistle → Lost;
- decline pending loss;
- stand and resume during ordinary scene, dangerous scene, suspended shot, resolution effect,
  ticket card, ticket transition, final sequence, and settle card;
- score and count endpoint convergence;
- two or more tickets with win/loss/cash-out permutations;
- final leg → leg slam → ticket settle;
- last ticket → round settle → next phase.

## 7. Scene-variety requirements

### 7.1 Scene plan, not variant count

A resolved beat produces a `ScenePlan` with independent, truth-constrained dimensions:

```text
ScenePlan
  fact contract      goal / count / miss / possession / final / structural
  movement grammar   central / wing / switch / counter / set piece
  chance shape       through ball / cross / cutback / rebound / direct
  pressure           low block / mid press / high press
  spacing            compact / balanced / stretched
  payoff             goal / block / interception / save / clearance / post / near wide
  reactions          step / chase / drop / recover / celebrate / collapse
  lane                center / near flank / far flank
```

The current `Variant` remains a compatibility input during migration; it is not the new variety
model and must not be inflated as the primary solution.

### 7.2 Truth-compatible grammar catalog

| Fact contract | Allowed movement | Allowed ending | Forbidden implication |
|---|---|---|---|
| Goal / breakaway goal | central, wing, switch, counter, set piece as compatible with template | through-ball finish, cross, cutback, rebound, direct/set-piece goal | miss, save that retains a no-goal result, uncounted corner/booking |
| Chalked goal | same as goal | ball enters goal, then immediate neutral no-goal treatment | score increment, red-card treatment |
| Near miss | central, wing, switch, counter, set piece | block, interception at the chance, keeper save, clearance, post, near wide | goal, score change, corner award unless a count fact also exists |
| Territory / calm | central recycle, wing progression, switch, controlled counter start | retained possession, forced back-pass, harmless interception/clearance with no stat | shot into goal, save call, corner award, booking |
| Corner | near-post, far-post, cleared | one representative corner payoff carrying the staged positive batch | goal or booking unless separately present in the fact contract |
| Booking | pressure/challenge setup | visible challenge and booking marker | goal, corner, injury, sending-off |
| Final | deterministic plan from final grade plus staged goal/count corrections | visible correction callbacks, whistle, win/loss/void reaction | hidden score/count jump or ending selected from stale `WinProbAfter` |
| Kickoff / fallback | structural restart or neutral possession | no statistical payoff | any score/count or decisive outcome |

### 7.3 Variety floor

The catalog must contain at minimum:

- 4 buildup grammars: central, wing, switch, counter;
- 5 scoring shapes: through ball, cross, cutback, rebound, **direct**;

  *Clarified 2026-07-28.* §7.1 named this fifth shape `direct` while this list called it `set piece`,
  which are not the same kind of thing — **set piece is a movement grammar, direct is a chance
  shape**, and a set piece resolves through a direct strike. The dimension keeps `direct`; the
  requirement is that a set-piece grammar paired with a direct chance shape is always available.

- 6 non-goal endings: block, interception, keeper save, clearance, post, near wide;
- 3 corner shapes: near post, far post, cleared;
- 3 pressure modes and 3 spacing modes;
- visibly different defending and reaction behavior per pressure mode;
- for each corner shape, a visible **win-the-corner** lead-in attributed to the staged beneficiary
  (§7.6) — the corner does not begin at the flag with no cause;
- booking setups for **both** sides, so the fouling team is never ambiguous;
- for anytime-scorer legs, every non-structural grammar remains legible with the backed-player
  locator active (§7.7).

Not every cross-product is valid. The planner filters by the truth contract, then ranks valid
candidates deterministically.

### 7.4 Repetition control

Maintain a revealed-history ring buffer of scene signatures. Candidate selection:

1. Build the valid set from the fact contract.
2. Rank each candidate with named presentation-hash channels.
3. Reject the immediately previous signature when another valid candidate exists.
4. Prefer a grammar absent from the last three non-structural scenes.
5. Prefer a payoff/reaction combination absent from the last two compatible scenes.
6. If constraints leave one candidate, play it and record the reason in diagnostics.

No hidden future beat may be read to arrange a better sequence.

### 7.5 Camera ruling

Recommended for this pass: fixed top-down camera, stable picked-team-right direction, static
scorebug and action rail. No zoom, crop, rotation, or camera cut is required for variety.

Camera/framing variation may return only after a couch test proves:

- team identity remains immediate;
- the whole payoff remains visible;
- actor dots and ball retain the approved apparent size;
- scorebug, active leg, and cash-out never move;
- direction never flips.

### 7.6 Market-attributed scenes (Allen, 2026-07-24)

Scene variety must cover the markets the game actually offers, not only goals and possession. Every
market whose decisive event is stageable gets scenes that visibly attribute the event to a team.

| Market family | Required visible attribution |
|---|---|
| Total corners | The scene shows **which team wins and takes the corner**: the winning team drives play into the attacking third, the ball goes out off the defending side, and the delivery is taken from that team's attacking corner. A corner may not appear as an unattributed set-piece cutaway. |
| Total cards | The scene shows **which team commits the foul** and which side's actor is booked. The challenge is visible before the booking marker. |
| BTTS | Goal scenes must make the **scoring side** unmistakable, since both sides' scoring state drives the leg. |
| Total goals | Existing goal shapes suffice; the scoring side must remain legible. |
| Moneyline | Existing goal/possession shapes suffice. |
| Anytime scorer | Governed by §7.7. |

Truth constraints:

- Attribution is read from the staged fact's beneficiary. The planner may not choose which team wins
  a corner or commits a foul; it may only stage the team the engine already committed.
- A positive count batch is still represented by one representative payoff per §7.2. Attribution
  applies to that representative payoff.
- Corner and booking scenes remain forbidden from implying a goal.

The `CornerFor` / `CornerAgainst` / `Booking` templates already carry the beneficiary. The audit in
Phase 1A must confirm the beneficiary is correct before Phase 2 builds attribution on top of it; if
the beneficiary is unreliable, that is a blocker, not a polish item.

### 7.7 Backed-player locator for anytime scorer (Allen, 2026-07-24)

**Problem:** the anytime-scorer market exists, but a player watching the TV cannot tell where their
backed player is, or whether that player is anywhere near the current chance.

Requirement: when the active leg is an anytime-scorer market, the backed player is continuously
identifiable on the stage, and the viewer can judge whether that player is involved in the chance
now on screen.

Candidate treatments to be resolved by the visual-design track in §14 (not decided here):

- jersey numerals rendered on actor dots, with the backed number emphasized;
- a persistent ring, chevron, or halo on the backed actor;
- a surname tag anchored to the backed actor;
- a combination, with the fallback being whichever survives couch-distance review.

Hard constraints on any treatment:

- **No outcome leak.** The locator reveals *position*, never *result*. The backed player appearing in
  a dangerous scene may not imply a goal is coming, and their absence may not imply it is not. This
  is §4.2, and a locator that lets a viewer predict the payoff before the causal reveal is a blocker.
- **Actor binding is now continuous, not reveal-only.** If the backed player is marked throughout the
  sweat, the marked actor must be the actor that takes the visible final touch at a scoring payoff.
  This raises the severity of `TVS-H03`: what was a reveal-time copy issue becomes a whole-sweat
  identity contract. Phase 1A must reproduce or reject `TVS-H03` before Phase 2 depends on it.
- **Deterministic.** Which on-field actor carries the backed identity is derived from the §4.3
  presentation key, not from engine RNG or frame timing.
- **Legible muted, at couch distance**, at the approved apparent dot size.
- **Scoped.** Non-scorer markets show no locator; the stage does not gain permanent numbering that
  competes with the ball.

"Close to scoring" is expressed through visible position and involvement only. No predictive meter,
heat value, or threat percentage may be derived for the backed player.

## 8. TV UI requirements

### 8.1 Stable layout zones

The signed-off layout uses five non-overlapping zones:

1. top scorebug;
2. theater stage;
3. active-leg card;
4. restrained event strip;
5. ticket-risk row plus stable cash-out/action slot.

System chrome (round, bank, payment, seed) remains lowest priority and may stay small.

### 8.2A Concurrent live legs — reclassified 2026-07-28

**Original statement (Allen, 2026-07-25):** a ticket can carry two or more legs riding on the same
match, live simultaneously, and the single-active-leg assumption must be removed.

**Investigation finding (2026-07-28, `concurrent-legs-investigation.md`): the engine explicitly
forbids this and throws.** `engine/Run.cs:181-182` rejects any ticket carrying two picks on the same
matchup index. It is an enforced invariant, not an unexercised path.

The guard protects the betting math. Payout is a bare **product** of the legs' decimal odds
(`Domain.cs:465`, `OddsMath.cs:59`), which is only valid for independent events. Two legs on one
match are correlated, so lifting the guard without a correlation model misprices every such ticket.

**Therefore this is a future game-design feature, not a TV sweat requirement.** Its real dependency
chain, in order:

1. a correlation model for same-match selections — `design/02-betting-math.md`, and required by
   design pillar 3, which says a mechanic whose EV cannot be written down for the Monte Carlo audit
   is not designed yet;
2. an engine change lifting the guard — §11 forbidden here;
3. six-gate re-validation on held-out seeds, since ticket pricing drives run economy;
4. only then, the presentation work.

Steps 1–3 sit entirely outside this worktree. **Allen's intent stands and is worth keeping — this
finding only establishes that delivering it starts in the betting math, not on the television.**

#### What this slice carries instead: a tolerance constraint, not plumbing

Do **not** build concurrency machinery for an input that cannot arrive. Do keep the design tolerant,
so the feature does not require a rewrite if it ever lands:

- Nothing hard-codes "exactly one live leg" in a structure that would need replacing — the ticket
  column, the planner, and the copy formatters should all read from a collection.
- The presentation key's **match index** decision (§4.3) stands. It costs nothing while one leg maps
  to one match, and is simply correct: a beat belongs to a match.
- `DramaEvent.Step` being leg-scoped is **not** a defect while one leg maps to one match — the two
  scopes are identical. It becomes a concern only alongside step 1 above, and whoever builds that
  owns it.

The requirements below are retained as the specification the feature would have to meet, and as the
tolerance bar for current work. Where they say "each live leg", today that collection has one member:

- The phrase `YOUR ACTIVE LEG`, singular, is retired. Copy must work for one live leg or several.
- The ticket column shows **every** live leg in its live treatment at once, each with its own `NEED`
  and its own revealed `LIVE` progress. There is no "the" active leg to expand.
- A single match event can move **multiple** legs at once. A goal may advance a total, settle a
  moneyline, and complete BTTS in the same callback. Each affected leg must update at that same
  causal reveal, and the event strip must not imply only one leg moved.
- Cash-out remains **per ticket**, never per leg. Multiple live legs do not create multiple offers.
- The stage serves all legs on that match simultaneously. It is not "the stage for the active leg."
- §7.7's backed-player locator applies when *any* live leg is an anytime-scorer market.
- §7.6 attribution must be correct for every live count market at once — a corners leg and a cards
  leg can both be live on one match.
- Layout must degrade gracefully as the number of concurrent live legs grows. The design bar: the
  player can still answer §1's six questions when three legs on one match are live.

**Consequence for the audit and tests:** every market/transition scenario in §6.3 must be exercised
with concurrent live legs on one match, not only with sequential single legs. Add to the required
matrix: two live legs on one match where one settles and the other continues; two live legs where a
single event moves both; a live leg settling while another remains live and the ticket does not
transition.

### 8.2 Active-leg card

Subject to §8.2A — where this section says "the active leg", read "each live leg".

The card always displays:

- `YOUR ACTIVE LEG`;
- market/odds;
- one plain-language `NEED` statement;
- one causal `LIVE` progress statement;
- backed-team identity for team markets, or explicit `MARKET PICK` treatment for non-team markets.

Market copy:

| Market | Need | Revealed live progress |
|---|---|---|
| Moneyline | `[TEAM] TO WIN` | `LEADING n–n`, `LEVEL n–n`, or `TRAILING n–n` |
| Total goals Over | `OVER x.x GOALS` | `n GOALS • m MORE` where the half-line permits exact remaining copy |
| Total goals Under | `UNDER x.x GOALS` | `n GOALS • LIMIT k` |
| BTTS Yes | `BOTH TEAMS TO SCORE` | `0/2`, `1/2`, or `2/2 TEAMS SCORED` |
| BTTS No | `KEEP ONE TEAM SCORELESS` | `CLEAN-SHEET PATH LIVE` or `BOTH HAVE SCORED` only when causally revealed |
| Total corners | `OVER/UNDER x.x CORNERS` | `n CORNERS • LIMIT/NEED k` |
| Total cards | `OVER/UNDER x.x CARDS` | `n CARDS • LIMIT/NEED k` |
| Anytime scorer | `[PLAYER] TO SCORE` | `WAITING FOR [SURNAME]`; `SCORED` only at the causal identity payoff |

The formatter may not read unrevealed endpoint values to create progress copy.

### 8.3 Scorebug

- Team names wear their existing deterministic theater colors.
- Moneyline’s backed team receives a persistent `BACKED` pill; non-team markets use no fake team pick.
- Score is the largest numeric element.
- Clock remains fixed at the right edge and uses PRE → minutes → 90'+n → FT.
- Ticket/leg index remains visible but subordinate.
- Records are removed from the primary scorebug during live playback; they add little to the six
  required questions.

### 8.4 Ticket risk and leg progress

- `RISK $x` and `PAYS $y` remain on screen throughout the sweat.
- Leg states use readable chips: `W`, `LIVE`, `NEXT`, `L`, `VOID`.
- Team-market chips use team identity color until resolution; money colors take over only at
  W/L/VOID.
- The momentum tape may remain as secondary memory, but it cannot displace leg status or shrink the
  risk/payout copy.

### 8.5 Cash-out/action slot

The same stable rectangle always owns the market action:

- Actionable: gold `CASH OUT $x  [E]`.
- Price animating: gold amount plus `UPDATING`; acceptance disabled.
- Dangerous scene: neutral gray `MARKET SUSPENDED`.
- Pending Mulligan/Whistle window: neutral gray `MARKET SUSPENDED`; intervention controls live in
  their own overlay, not in the cash-out label.
- Unavailable: muted `CASH OUT UNAVAILABLE` only when the absence needs explanation; otherwise the
  reserved slot remains visually quiet without reflow.
- Accepted: gold `CASHED OUT $x`, then ticket-settle transition.

`CashOutLive` and the stand-up suppression contract must agree with the visible state. A suspended
or updating offer is not “live.”

### 8.6 Event callout

- One line, no more than two visual rows.
- Lands at the same payoff callback as the price/score/count change.
- Names the factual price-moving event; no generic line may contradict a visible goal/count.
- Uses neutral broadcast white/cyan. Green/red/gold remain reserved for money outcome/action.
- Does not cover the pitch or active-leg card.
- Lifetime pauses when the player stands.

### 8.7 Pending intervention window

The frozen shot remains visible. A compact overlay states:

- `SHOT FROZEN`;
- `MULLIGAN — VOID LEG` when available;
- `SEND TO REVIEW (p%)` when available;
- `LET IT DIE`;
- market remains visibly suspended.

The overlay may not reuse the cash-out row or hide the score/time/active leg.

### 8.8 Match stats panel (Allen, 2026-07-25)

A clickable element on the TV opens a current-match stats view. The player can check the state of
the match beyond the six headline questions.

Required content:

- corners, per team (A and B separately, not a combined total);
- cards, per team;
- current formation for both sides;
- player stats;
- other revealed match state as the design track determines useful.

**This is a scope change.** §3 previously listed "new mid-sweat verbs" as out of scope. Allen has
added one deliberately. §3 is updated accordingly. It is the *only* new verb authorized; nothing
else in this pass may introduce another.

Constraints, all of them load-bearing:

- **Revealed facts only.** This panel is the single most likely place in the whole product to leak a
  hidden outcome. It may show only what §4.2's revealed presentation facts already contain. It may
  not read the locked stat line, endpoint totals, or any unrevealed value to populate a row. A stat
  that has not been causally revealed yet is absent or shown as `—`, never as its true final value.
  A leak here is a blocker, not a polish item.
- **Per-team counts must be real.** §7.6 established that the current code threads the bettor's
  Over/Under pick where a per-team beneficiary belongs, so "corners for team A" cannot be rendered
  correctly today. This panel therefore depends on the §7.6 fix and cannot ship before it.
- **Pausing — ruled by Allen 2026-07-25: opening the panel freezes playback exactly as standing up
  does.** The full §4.4 freeze contract applies: event cursor, scene step, ball, actors, clock,
  probability animation, cash-out animation and offer, callout lifetime, resolution effect,
  transition, and pending-window timer all hold. Closing the panel resumes from that state with no
  catch-up. The player can never miss a payoff by reading their stats, and the panel cannot be used
  to buy thinking time on a cash-out decision, because the offer is frozen too.
- **Cash-out interaction.** While the panel is open, the cash-out offer keeps moving. The panel may
  not obscure the cash-out state, and the input contract must not repeat `TVS-H01`: the key that
  opens and closes this panel must not be swallowed by, or swallow, the cash-out or stand controls.
- **Formation display** overlaps §7.7's backed-player locator. One identity model serves both; the
  panel's formation view and the stage's numbered dot must agree.

**Still open — one ruling needed before Phase 3:** is the panel available during a
pending-intervention window, or suppressed there? The freeze ruling above makes availability
defensible, since opening it cannot run down the intervention timer. Recommendation: allow it, and
keep the intervention overlay visible on top of the panel so the player never loses the choice they
are being asked to make.

### 8.10 Held cash-out preview (Allen, 2026-07-26)

Holding the cash-out key shows the settled future in place before committing to it. Releasing
without confirming reverts completely, with no residue. Confirming merely keeps what is already
visibly true.

What the preview shows while held:

- the bank at its post-cash-out value;
- every remaining live leg struck out, since cashing out ends them;
- the ticket in its cashed-out state;
- the accepted amount, which is the amount currently displayed.

Constraints:

- **It previews a consequence, not a match fact.** This is the reason it is admissible under §4.1.
  It shows what *this action* would do to the player's own position. It reveals nothing about the
  match that has not already been revealed, and it may not consult a locked endpoint to do it.
- **The previewed amount is the acceptable amount.** If the offer is mid-animation the preview may
  not be entered at all, for the same reason acceptance is refused there — the displayed and accepted
  numbers must never differ. The gate is `CanAcceptCashOutNow`, exactly as repaired in TVS-H01. If
  cash-out cannot be accepted right now, it cannot be previewed right now.
- **Release is a full revert.** No partial state, no lingering strike-throughs, no bank flicker.
- **Standing while held cancels the preview** and freezes per §4.4. The preview is not a way to hold
  the sweat still.
- **The preview is not a second truth source** (§4.2). It renders from the same revealed facts and
  the same offer the cash-out slot is already showing.

Rendering is governed by `DESIGN.md`. The preview is the one moment the surface deliberately shows a
state that is not yet true, so it must be unmistakably provisional and must never be confusable with
a settled ticket.

### 8.9 Ticket and settle transitions

- Ticket interstitial clears the prior stage, offer, score/count, tape, event callout, and
  intervention state before showing the next ticket.
- The next ticket card shows ticket index, leg summary, risk, and payout; no prior match identity.
- Final-leg slam completes before ticket settlement begins.
- Ticket settlement completes before `AdvanceSweat`.
- Last-ticket settlement completes before round settlement.
- Transitions use restrained fades/slides and retain a stable backing; no white flash or stale frame.
- Standing pauses transitions as part of the viewing contract.

## 9. Reliability and implementation seams

Recommended focused helpers:

- `PresentationSceneKey` — immutable deterministic key and named sub-hashes.
- `TheaterScenePlanner` — truth-constrained `SceneSpec` → `ScenePlan`; owns grammar catalog and
  repetition control.
- `TheaterScenePlan` / `TheaterStep` — plan data executed by `TheaterStage`.
- `SweatActiveLegModel` — pure formatting model for market need/progress and cash-out state.
- `TvSweatPlaybackSnapshot` — read-only diagnostic surface for tests and bug evidence.

Incremental integration:

- `TheaterChoreographer` continues to decide the factual template and ledger payload.
- The new planner elaborates that factual `SceneSpec`; it cannot change the template’s truth contract.
- `TheaterStage` executes plans and retains payoff callbacks.
- `TvSweatScreen` remains the session orchestrator but delegates active-leg formatting and scene
  planning.
- `SweatPresentationModel`, `ScoreLedger`, and `CountLedger` remain the revealed facts.

This is intentionally not a new global event bus or replacement playback framework.

## 10. Test and acceptance gates

### Automated

- Pure tests for deterministic key/hash channels and no-repeat selection.
- Truth-contract coverage for every allowed grammar/payoff combination.
- Same key → same `ScenePlan`.
- Neighboring event step produces legal variety without touching engine RNG.
- All 16 templates × 3 legacy variants start, reveal exactly once, and complete.
- Pause/resume parameterized across scene phases and non-scene transitions.
- Cash-out state machine: unavailable/open/updating/suspended/reopened/accepted.
- Pending window: Mulligan void, Whistle win, Whistle loss, decline, stand/resume.
- Anytime scorer: visible final-touch actor and label identity agree at the causal payoff.
- Goal playback count equals committed score delta; no-goal leaves score unchanged.
- Corner/card playback totals equal count delta; zero batch produces no count scene.
- Final displayed score/count equals the locked endpoint after all visible callbacks.
- Multi-ticket reset and last-ticket settle ordering.
- Playback timeouts fail with a diagnostic snapshot rather than hanging the suite.
- Existing engine tests remain green; no engine golden pin changes.

### Manual muted couch gate

Three full sweats, recorded in the ledger:

1. **Team/score sweat:** moneyline-led ticket; goals, possession, near miss, cash-out
   suspension/reopen.
2. **Identity/goal-market sweat:** anytime scorer plus total-goals or BTTS; scorer won and lost
   paths across the audit set.
3. **Count/transition sweat:** corners/cards, multiple tickets, pending intervention, final leg,
   ticket settle, and round settle.

For all three:

- no stuck playback;
- no false goal, miss, corner, booking, scorer, or result;
- no score/count disagreement at any callback or final endpoint;
- no stale prior-ticket UI;
- open/suspended cash-out state is obvious and truthful;
- player can state the active requirement within three seconds of looking at the TV;
- no adjacent sequence is called out as “the same move again” when the facts permit alternatives;
- audio muted throughout.

### Gate definition

Acceptance fails while any blocker or major remains open. A polish item may remain only with
Allen’s explicit deferral in the ledger.

## 11. Ownership constraints

### Owned

- `TvSweatScreen.cs`
- `TheaterStage.cs`
- `TheaterChoreographer.cs`
- `ScenePlaybook.cs`
- `SweatPresentationModel.cs`
- `SweatPacer.cs`
- `MomentumTape.cs`
- relevant TV/theater tests
- new TV-specific helper classes
- this PRD, visual spec, and audit ledger

### Must not be modified

- `TvAudioDirector.cs`
- `Room.unity`
- `GrayboxRoomBuilder.cs`
- Laptop/SureThing files
- `RunDirector.cs`
- `engine/**`
- `SBR.Engine.dll`

Any discovery that appears to require one of those files becomes a decision gate; the agent does
not cross the boundary.

## 12. Supervision and review protocol

**Execution model, revised by Allen 2026-07-24:** the project is on a full Claude workflow. The
`gpt-5.6-terra` execution model is retired. All bounded audit, implementation, test, and validation
work is dispatched to **Claude Sonnet 5** agents.

Dispatch limits:

- At most **2 concurrent** Sonnet 5 agents. The technical product lead does not exceed this to
  parallelize a phase.
- Reasoning effort is chosen per task by the technical product lead. Recommended defaults: `high`
  for the Phase 1A audit and any truth-contract or determinism work, `medium` for mechanical
  implementation and test authoring, escalating when a task returns weak evidence.
- Each dispatch names the allowed files, the forbidden files (§11), the required evidence, and the
  exit gate.

After sign-off:

1. The technical product lead dispatches a bounded Sonnet 5 task within the 2-agent limit.
2. The worker reports evidence and changed files.
3. The technical product lead reviews the ledger or diff against this PRD and source truth.
4. Blockers/majors are corrected by the execution agent, not silently rewritten by the reviewer.
5. Each phase closes only after its explicit gate.
6. No PR is declared ready until the three-sweat muted acceptance gate is recorded.

## 13. Sign-off ledger

Allen returned `APPROVED WITH CHANGES` on 2026-07-24.

| # | Decision | Ruling | Notes |
|---|---|---|---|
| A | **Layout** | **APPROVED 2026-07-25 — Layout B, "Ticket Rail"** | Chosen from five greybox concepts in `visuals/layout-concepts.html`. The bet slip is a permanent left column carrying all legs, the active leg expanded, and risk/pays; the stage takes the remaining width; cash-out anchors the foot of the ticket column. Allen's reasoning: the F-pattern puts first fixation on the ticket, which is where the product's value lives. The originally recommended right-rail layout is **not** adopted. |
| B | **Camera:** no camera/framing variation in this pass | **APPROVED** | Fixed top-down retained. A FIFA-style camera that follows the ball carrier is recorded as a refinement-v2 candidate and is explicitly out of scope here. |
| C | **Variety:** deterministic `ScenePlan` with truth filtering and revealed-history cooldown | **APPROVED** | Scope extended by §7.6 (market-attributed scenes) and §7.7 (backed-player locator). |
| D | **Sequence:** audit → reliability fixes → scene variety → UI → integrated gate | **APPROVED** | The visual-design track (§14) runs in parallel and does not touch production code. |
| E | **Audio:** explicitly deferred; no file, tuning, or validation work | **APPROVED (deferred)** | `TvAudioDirector.cs` remains untouched. |
| F | **Branch:** create/use `slice/tv-sweat-refinement` at implementation start | **APPROVED** | Branch is created when Phase 1A begins, not before. |

Decision A is the only open item. Because A is an input to Phase 3 and not to Phase 1A, it does not
block the bug audit.

Sign-off options for future gates:

- `APPROVED` — authorize the next phase only.
- `APPROVED WITH CHANGES` — list changes; the documents are revised first.
- `NOT APPROVED` — no implementation or audit execution begins.

## 14. Visual-design track (Allen, 2026-07-24)

Decision A is not settled by picking the layout already drawn in
[VISUAL-DESIGN.md](VISUAL-DESIGN.md). Allen accepts its intent but requires competing options and a
brand book before any UI is built. This track runs through the Impeccable design skill, in parallel
with Phase 1A, and touches **no production code**.

### 14.1 The brand book is a formalization, not a greenfield invention

`design/08-art-direction.md` was decided by Allen on 2026-07-10 and is binding product truth. The
brand book codifies it for the TV surface; it does not reopen it. Constraints carried in:

- casino neon on black; deep black / near-black blue base;
- phosphor green = money-good, hot red = money-bad, **used for nothing else**;
- gold = cash-out, jackpot, payout moments;
- dim cyan/white = chrome, clocks, filler ticker text;
- typography is the primary art asset — strong numerals, ticker fonts, parodied sportsbook iconography;
- CRT treatment: phosphor glow, scanline flicker, curvature/chromatic aberration on big hits;
- the TV is diegetic. It is a screen inside a room, viewed from a couch, and is not a desktop UI.

These are already echoed in §8.6 of this PRD. Any layout concept that breaks the color language is
rejected on sight.

### 14.2 Track sequence

1. **`PRODUCT.md`** — Impeccable requires a product record before visual work. Nearly all of it is
   already in `design/00-vision.md`, `design/08-art-direction.md`, `DECISIONS.md`, and `README.md`.
   It is drafted from that evidence, every inferred fact labeled, and approved by Allen rather than
   extracted through an interview he has effectively already answered.
2. **Brand book** — TV-surface design system: color tokens with the money-language rule encoded,
   type scale for couch distance, numeral treatment, state colors for open/suspended/unavailable/
   pending/cashed-out/won/lost/void, chip and pill specs, motion and CRT rules, and the dot/actor
   vocabulary the stage needs for §7.6 attribution and §7.7 locator treatments.
3. **Layout concepts** — at least three genuinely different TV layouts, each satisfying the six
   information priorities in §1 and the five-zone stability requirement in §8.1, each rendered
   against the brand book so they are comparable. The incumbent VISUAL-DESIGN.md layout is included
   as one option so Allen chooses against a real baseline, not a strawman.
4. **Allen selects.** Decision A closes. Selected layout is written back into VISUAL-DESIGN.md.
5. **Phase 3 UI build** begins only after step 4.

### 14.3 What the layout concepts must resolve

Beyond §8, every concept must show its answer for:

- where the §7.7 backed-player locator lives and how a surname or number reads at couch distance;
- how §7.6 corner/card attribution surfaces in the event strip without a second truth source;
- the stable cash-out slot across all six states in §8.5, with no reflow between them;
- the pending-intervention overlay from §8.7 without reusing the cash-out row.

### 14.4 Boundaries

This track produces documents, tokens, and mockups. It does not modify `TvSweatScreen.cs`,
`TheaterStage.cs`, or any other file in §11 until Phase 3 is authorized.
