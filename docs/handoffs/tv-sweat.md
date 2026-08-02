# TV Sweat — lead ownership contract

**Worktree:** `tv-sweat` · **Branch:** `slice/tv-sweat-refinement` · **Lead:** Claude (Opus 5)
**Contract authority:** `main-2/docs/5-orchestration/STUDIO.md` · **Board:** `main-2/docs/5-orchestration/STATUS.md`
**Written:** 2026-07-31 · **HEAD at writing:** `220c5ec`

Supersedes `handoff.draft.md`, which was a briefing rather than a contract and carried none of the
four sections STUDIO.md requires. Its briefing content is folded into §5 below; the draft may be
discarded.

---

## 1. File ownership

### Owned exclusively by this worktree

| Path | Note |
| --- | --- |
| `unity/SBR/Assets/SBR/Runtime/TvSweatScreen.cs` | Session orchestrator |
| `unity/SBR/Assets/SBR/Runtime/TheaterStage.cs` | Scene playback |
| `unity/SBR/Assets/SBR/Runtime/TheaterChoreographer.cs` | Factual template + ledger payload |
| `unity/SBR/Assets/SBR/Runtime/ScenePlaybook.cs` | `SceneSpec` |
| `unity/SBR/Assets/SBR/Runtime/SweatPresentationModel.cs` | Score/count ledgers |
| `unity/SBR/Assets/SBR/Runtime/SweatPacer.cs`, `MomentumTape.cs` | Pacing, tape |
| `unity/SBR/Assets/SBR/Runtime/TheaterScenePlanner.cs`, `TheaterScenePlan.cs`, `PresentationSceneKey.cs` | Phase 2 planner stack |
| `unity/SBR/Assets/SBR/Runtime/TvLight.cs` | **Ownership confirmed by Allen 2026-07-27** — was on neither list; room lead disclaims it |
| `unity/SBR/Assets/SBR/Runtime/Shaders/TvSweatHdrUI.shader` | Created by this worktree |
| `unity/SBR/Assets/Tests/PlayMode/TheaterStage*.cs`, `TvSweatScreenTests.cs` | TV/theater tests |
| `unity/SBR/Assets/Tests/EditMode/PresentationSceneKeyTests.cs`, `TheaterScenePlannerTests.cs`, `TvLightTests.cs`, `TvSweatScreenPaletteTests.cs` | |
| `DESIGN.md`, `PRODUCT.md` (root) | TV surface design system; product record |
| `docs/tv-sweat-refinement/**` | PRD, visual design, bug ledger, briefs, evidence |
| `docs/handoffs/tv-sweat.md` | This contract. Moved from the repo root 2026-07-31 — a committed root `handoff.md` collides across worktrees at merge time (studio convention) |

### Read-only (diagnosis permitted, edits are an escalation)

`engine/**`, `SBR.Engine.dll`, `RunDirector.cs`. Granted read access by Allen 2026-07-29 for
diagnosis; a needed change escalates to the orchestrator.

### Never touched by this worktree

`TvAudioDirector.cs` (audio deferred, PRD §3) · `Room.unity`, `GrayboxRoomBuilder.cs`, room
materials and lighting rig (room-refinement) · Laptop/SureThing files (surething-ui) ·
`ProjectSettings/**` and package manifests (**integration-only per STUDIO.md**) ·
`docs/ARCHI.md`, `DECISIONS.md`, root plans (**integration-only**; needed updates recorded in §6).

### Known boundary hazards

- **Build side effects.** Every `dotnet test` and Unity run dirties `SBR.Engine.dll`,
  `ProjectSettings/EditorBuildSettings.asset`, `ProjectSettings/ProjectSettings.asset` — two of which
  are integration-only. Revert with `git checkout --` after **every** run and verify `git status`
  before committing. This recurs constantly and is a property of the build wiring, not agent error.
- **`GrayboxRoomBuilder.Build()` regenerates `Room.unity` from scratch** and rewrites builder-owned
  material properties. Nothing hand-placed survives. Anything this worktree needs persistent in the
  room goes through the room lead.

## 2. Local plan

Approved sequence (PRD Decision D): audit → reliability → scene variety → UI → integrated gate.

| Phase | State |
| --- | --- |
| 0 Design gate | Closed — `APPROVED WITH CHANGES` |
| 1A Audit | Closed |
| 1B Reliability (TVS-H01/H02/S01/H03) | Closed, Allen signed off 2026-07-27; audit-rerun gate waived by name |
| 2A–2E Scene variety | **Closed** at `220c5ec`; automated gate met |
| **3 UI refinement (T7)** | **Unblocked** by the C1 ruling once this contract lands |
| 4 Integrated acceptance | Three muted couch sweats; needs GPU |

**Phase 3 contents:** Layout B build per `DESIGN.md` §6, brand-book palette and brightness ladder,
§8.8 stats panel, §8.10 held cash-out preview, §7.7 backed-player locator, plus the carried debts —
T9 `chromeCyan`, T10 emission rest values, and the deferred scorer-reveal gap (a won anytime-scorer
leg whose backed-side goals are spent before the final sequence produces no reveal).

**Held, not started:** T8 scanline overlay and `DeadLegBeat` static crawl. `DESIGN.md` §2 bans both
by name; removal is recommended and awaiting Allen. **Nothing further is built on either effect.**

## 3. Delegation bounds

- **At most two bounded sub-agents at once**, per STUDIO.md. Current practice has been one at a time
  for anything touching `TheaterStage.cs`, because every Phase 2 dispatch collided there.
- Every dispatch names allowed files, forbidden files, required evidence, and an exit gate.
- **Never invent a runtime result, seed, rate, or test outcome.** Honest "NOT RUN" beats a
  fabricated row.
- **A failing test is evidence, not an obstacle.** Deleting or weakening one to make a change pass
  requires this lead's explicit agreement. This has been attempted once (TVS-S01) and was caught in
  diff review.
- The lead reviews the diff, not the summary. Agent reports have been accurate and still wrong:
  TVS-S01's fix re-created its own bug in the opposite direction with all suites green.
- Sub-agents do not commit unless the dispatch says so, and never touch `.impeccable/`.

## 4. Verification procedure

**Unity is a single-instance studio-wide resource. A lease is a WINDOW, not a moment.**

Added 2026-07-31 after a queue violation: this lead confirmed the editor free at *close* but never at
*open*, and a still-exiting Unity process overlapped another worktree's granted slot. Transient and
harmless that time. The procedure below closes it.

0. **Before opening — every time, not just the first:**
   a. Hold an explicit grant from the orchestrator for the current slot. A general "queue is clear"
      from an earlier cycle is **not** a standing lease; a later sequencing note supersedes it.
   b. Confirm the editor is actually free: process count **and** `unity/SBR/Temp/UnityLockfile`.
      **The check must ABORT the run, not merely print.** Amended 2026-07-31 after a slot opened on a
      reported-free editor that read process count `1` — a straggler mid-exit. The check printed the
      1 and the batch proceeded anyway, which made it advisory rather than a gate. A coordinator's
      "verified free" and this lead's "free at my open" can differ by seconds.
   c. **Announce open** to the orchestrator.

   **Known editor fault, three occurrences 2026-07-31:** Unity segfaults on `-quit` shutdown and
   leaves a **stale `UnityLockfile` with zero processes**. Clear it (safe when process count is 0)
   before opening. `-runTests` runs are unaffected and have produced valid XML every time — the
   fault is on the shutdown path, not on results.
1. **After the last run — announce close**, and confirm process count and lockfile are clear before
   saying so. Unity exits lazily; a finished command is not a released editor.
2. The window between (0c) and (1) is yours and nobody else's. Anything that does not need the editor
   — reading source, writing tests, diagnosing from a results XML — belongs **outside** it. Diagnose
   from artifacts after closing rather than holding the editor open to think.
3. **A silent automated run is indistinguishable from a slow one.** Added 2026-07-31 after a driver
   sat dead for 35 minutes of a granted window while its process stayed `ALIVE` and its monitor,
   tailing a log nobody was writing, never woke. **Liveness is artifact mtime, not process
   aliveness.** Three named traps, all measured, all mine:
   - **`Unity.exe` is a GUI-subsystem binary**, so `& $unity` returns *immediately* — a loop that
     trusts it stacks overlapping editors inside your own window. Do **not** patch that with
     `Start-Process -NoNewWindow -Wait`: from a console-less parent (a `-WindowStyle Hidden` pwsh)
     that combination hangs forever *without ever spawning Unity*. `Start-Process -PassThru` then
     `Wait-Process -Id` is the pair measured to work.
   - **`$Args` is a PowerShell automatic variable.** `function Invoke-Unity([string[]]$Args)` leaves
     it empty, so Unity launches with **no arguments at all** — no project, no filter, no `-logFile`
     — and exits **0 in ~11s** having done nothing. It writes to the default
     `%LOCALAPPDATA%\Unity\Editor\Editor.log`, whose `COMMAND LINE ARGUMENTS:` block is how you
     prove it. Name the parameter anything else.
   - Both failures reported **success**. §4 step 3's "the XML must exist and be newer" is what caught
     each one; neither was visible from an exit code.
   **Measured costs at `5d61a04`:** warm compile ~106s; one filtered `TvSweatScreenTests` PlayMode run
   ~153s wall for ~31s of test time. Ten runs is ~26 min per arm — size batches against that, and
   prefer a foreground batch you can read over a background driver you must trust.

4. **NEVER END A TURN AGAINST A RUNNING CAPTURE.** Chartered 2026-08-01 after **three** silent
   capture-run deaths in one slice — not bad luck, a pattern:
   - A foreground `Start-Process` + `Wait-Process` inside a tool call dies when the call hits its
     10-minute cap, and **takes Unity with it**. Cost a run mid-seed-02.
   - A background waiter armed and then turn-ended was reaped twice, leaving the run unobserved.
   - A "no new frames" read on a healthy run was a false stall: seeds overwrite their own filenames,
     so **frame count is not a progress signal — mtime is.**

   The rule: **hold the wait IN-TURN** (a bounded polling loop inside one tool call — captures are
   ~60s per seed, well inside the cap), **or** self-re-arm a completion check each turn. Never both
   launch and hand the turn back. A capture nobody is watching is a capture that did not happen, and
   you will not learn that until the window is spent.

   Corollary, learned the same day: **launch detached** (`Start-Process` with no `-Wait`) whenever a
   run may outlive one tool call, so a harness timeout cannot kill the editor. Foreground reads more
   simply and is the wrong shape.

1. Warm compile: `Unity.exe -batchmode -nographics -projectPath unity/SBR -quit -logFile <log>`.
   `-runTests` and `-executeMethod` are **silently dropped** if scripts compile on the same run.
2. Suites, one at a time, waiting for the process and `Temp/UnityLockfile` to clear between:
   `dotnet test engine.tests` · Unity `-runTests -testPlatform EditMode` · `-testPlatform PlayMode`.
3. **Exit code 0 does not mean the run happened.** Verify the results XML exists and is newer than
   the edits under test.
4. `git checkout --` the three build side-effect files; confirm `git status` shows only intended
   changes before committing.

**Current baselines at `220c5ec`:** engine **160/160** · EditMode **129/129** · PlayMode **44/44**.

**Known flake — do not mistake it for a regression.** `TvSweatScreenTests` fails
`never observed the cash-out amount mid-tween (waited 20s)` on load-heavy runs; logged in
`BUG-LEDGER.md` §4C.4. Measured 2026-07-30: **HEAD 1 failure / 4 runs; Phase 2E-2 1 / 10.** This lead
wrongly called it a 2E-2 regression at n=3 and was corrected by measurement. **PRD §6.1 requires ≥10
attempts on both arms before claiming any timing regression.**

**Visual evidence.** `-nographics` rasterises no frame. Every visual claim is labelled
`PENDING-VISUAL-EVIDENCE`; couch-distance acceptance cannot be asserted from headless tests and
needs a GPU session. PRD §6.1.1 splits the evidence standard accordingly.

## 4A. Design system — spec of record

**`main-2/docs/design/design-system/`** is studio canon as of 2026-07-31, committed on `main`.
**Reference it cross-worktree; do not fork copies into this worktree.**

What this slice builds against:

| Path | Use |
| --- | --- |
| `components/tv/*.jsx` + `*.prompt.md` | Built references and their specs. **The `.prompt.md` is the spec of record** — it is consistently stricter and more precise than a ruling summary |
| `components/tv/tiers.js` | Canonical brightness tiers: **L4 1 · L3 0.7 · L2 0.4 · L1 0.15 · L0 0** |
| `tokens/palette-tv.css` | Colour tokens |
| `guidelines/` — `tv-brightness`, `type-tv` | The laws behind the tiers and the typeface |
| `ui_kits/tv-sweat/` | Runnable kit of the whole surface |

**Read the `.prompt.md` before implementing from a ruling line.** Concrete instance: T16's summary said
"no numerals, no hue, never above L2"; `TvMomentumTape.prompt.md` additionally splits the tiers —
label and current sample at L2, sample history at L1 — and states the reasoning that makes the rule
enforceable ("the moment it needs a numeral it has become the banned win-probability readout"). A
test written from the summary alone under-specified all three rules.

Where a Unity test must assert against canon it cannot import (a C# test cannot load a JS module),
mirror the values as named constants and **cite the source path in a comment** — never invent a
threshold that happens to pass.

## 4B. TVS-H02 verification — **CLOSED 2026-07-31.** Do not re-run; resume at §4C

Kept as the record of how the fix was judged, not as a live instruction. Outcome: both arms n=10,
**zero** `kept ticking while standing` in each, 3 documented mid-tween flakes in each — identical
rates, so the flake is not stack-induced. The defect failed 3 of 4 before the fix and 0 of 10 after.
Committed with 3C + T16/C3/C8 as `4969eb1`. **T17 is also done** (`ea28c9b`); see §4C for what is next.

### State

The working tree carries a large **uncommitted** stack on top of HEAD `5d61a04`:

- Phase 3C — Layout B canvas rebuild (`TvSweatScreen.cs`)
- The T16 / C3 / C8 Design Director rulings — momentum tape restored at the scorebug foot; HDR
  eligibility widened to five with a one-token invariant; risk/pays in the bloom-floor protected set
- A tape-coupling fix — `MomentumTape.Build` moved **out** of `if (theaterEnabled)`; it is scorebug
  furniture, not stage furniture, matching the ball flash's existing precedent
- The **TVS-H02 fix** described below
- Tests: `TvSweatScreenLayoutGridTests.cs` (new), additions to `TvSweatScreenPaletteTests.cs`
  (markup scan, one-token invariant, arbitration, tape rules)
- Docs: `DESIGN.md` §9A, PRD §7.2.1 authored inventory, and 49 captures staged at
  `docs/tv-sweat-refinement/visuals/phase-2-scene-grammar/` for the DD's T6 visual review

### The defect and the fix

`TvSweatScreenTests.Standing_Freezes_CashOutTween_NoResumeCatchUp` failed **3 of 4** runs with the
stack and **0 of 3** at clean HEAD.

**Mechanism (confirmed by static analysis, not yet by execution):** `StartCoroutine` runs a coroutine
body **synchronously up to its first `yield`**, before returning the handle assigned to
`_cashOutAnimation`. A new tween's first `RenderCashOut` therefore ran while `_cashOutAnimation` was
still `null`, and the stack's new `_cashOutAnimation != null ? "UPDATING" : "[E]"` ternary painted
the wrong branch for exactly one frame, self-correcting the next. If the test caught that frame, the
correction landed *after* standing — a text change with the dollar amount frozen throughout. This
predicts the observed 3/4 rather than 4/4, because it is frame-scheduling dependent.

**The amount never ticked; the freeze held.** A one-frame render bug that freezing captured. The
quirk pre-dated the stack; the new `UPDATING` state made text sensitive to it for the first time —
exposed, not introduced.

**Fix location:** `unity/SBR/Assets/SBR/Runtime/TvSweatScreen.cs` — new `bool _cashOutTweening`, set
`true` **before** `StartCoroutine` so the coroutine's own first render sees it, and `false` before
each settle-render. `RenderCashOut` and `DebugCashOutAnimating` read it instead of the handle.
`elapsed += SeatedDeltaTime` — the actual freeze primitive — is untouched, as are `_l4Holder`,
`RequestL4`, `ReleaseL4`.

**Disqualified suspect, do not re-investigate:** the ungated C3 tail in `AnimateCashOutTaunt`.
`CanAcceptCashOutNow()` requires `_cashOutAnimation == null`, so `actionable` was already `false`
mid-tween — the block behaves identically before and after standing in this scenario. It *is*
genuinely ungated by `_seated`, which is judged **correct**: standing means input is refused, so the
L4 actionable promise should end (§8.5, "brightness is a promise about input"). Carried, not a bug.

### Exit criteria — judge by failure MESSAGE, never by test name

Two failure modes share this test name and mean opposite things:

- `cash-out amount kept ticking while standing` → **the regression**
- `never observed the cash-out amount mid-tween (waited 20s)` → the documented load-correlated flake
  (`BUG-LEDGER.md` §4C.4; measured HEAD 1/4, 2E-2 1/10). **Permitted at its documented rate; not a
  miss.**

1. **≥10** filtered runs with the stack:
   `-runTests -testPlatform PlayMode -testFilter "SBR.Tests.PlayMode.TvSweatScreenTests"`
2. **≥10** at clean HEAD (`git stash push -- unity/SBR/Assets`, run, then **`git stash pop`** — the
   stack is uncommitted and must not be lost).
3. **Green = zero** `kept ticking while standing` in the stack arm.
4. Then full `dotnet test engine.tests` + EditMode + PlayMode.
5. On green: **commit 3C + T16/C3/C8 + the TVS-H02 fix**, then advance to **T17**.

Baselines before this stack: engine **160**, EditMode **194**, PlayMode **44** (+1 `[Explicit]`
capture harness, filtered out of routine runs).

## 4C. RESUME HERE — the slice is EVIDENCE-COMPLETE and on HOLD (2026-08-02)

**Do not start anything. Every remaining item is a DD verdict, not work.** Items 10–12 referenced
below were answered in Batch 4 (as T21/T22/T23) and the section after them is kept as the record of
how they were reasoned, not as a live queue.

**Landed since:** the C14 audit (42 gaps, 4 falsified) and its fix-now block — the brightness ladder
applied where it was declared but dead, both canon faces wired, the tape colourless under T16, the
cash-out field inverted with money and status split; T24 re-measured in Encode Sans (**the deficit
does not survive** — 59px against a 69.3px slot, so risk/pays stays in the footer and the ruling's
fallback never fires); T30's threshold predicates retired for verbatim constant matching; T25.1
containment; T38/T40/T32.1 from Addendum II; TV-14's three-span compact row; T39 as one scan; and the
tape's MOMENTUM label, which **did not exist** when its tier was corrected.

**Evidence delivered and awaiting verdicts:**

| Bundle | What |
|---|---|
| `tv-sweat-setB-and-bloomAB.zip` | Set B, five seeds, **nine grammars**, first set rendered in Encode Sans |
| `tv-sweat-bloom-AB.zip` | C8·a pair, 17+17, frame-for-frame parallel, **every frame carries its own boost token** |

**Awaiting DD, nothing else:** TV-02 (tape shape — canon's single 28px strip vs this build's per-leg
rows), TV-15 (**measured**: stacked needs a 56px footer and lands the slot at 66.7 against a measured
need of 67; side-by-side fits at 214w of a 265px column with the footer untouched — a 0.3px verdict
is the DD's), plus the frame verdicts on both bundles.

**Two rules this slice paid for, both now in §4 above:** never end a turn against a running capture,
and fix by RULE not by SITE — `WonLegBeat` kept a violation already fixed in its two sibling beats,
and T39's first pass left six strings for the same reason.

---

## 4D. Superseded — the Phase 3 gating record (kept for reasoning, not as a queue)

**T17 is CLOSED** (`ea28c9b`): the ledger reserves the backed side's last baked goal at configure
time, enforced in `CompleteGoal` and released by `PlanFinal`; `BindAnytimeScorer` is unchanged and
the causal reveal point did not move. The reproduction was inverted in place, and the DD's acceptance
property is `Every_won_anytime_scorer_leg_reveals_exactly_one_scorer_however_its_beats_ran`. Full
record, including the closure evidence, is in `BUG-LEDGER.md` under "T17".

**Open for the Design Director:** the reserve is player-visible — on a scorer leg the backed side's
score now holds one goal short until the final sequence. Intended by the ruling, but confirm it reads
right on screen.

**T20 is CLOSED** (`48a9fbd`). The ticket's "23→19px" described the *canon's* change, already applied
upstream in `tokens/typography.css`; neither number existed here. The Unity surface had never matched
that table, and the real blocker was structural — NEED and progress shared one `Text` at 12px, so the
re-derivation was not expressible until the row was split into `Line`/`Need`/`Progress`. The whole
surface is now on the canon scale, with two zone heights grown to hold it rather than type shrunk to
fit. **Deviation with the DD:** the live row has no market/price/state meta line — canon's three-line
row needs ~73px against a 69px fixed slot. Reasoning is in the `LegRowUi` doc comment; do not "fix"
it by shrinking NEED.

**Every size in T20 is verified analytically and none of it visually.** The next seated GPU capture is
unusually load-bearing: it is the first look at a surface whose every element changed size.

**3D is CLOSED.** The find was that §8's `VOID` treatment — "L2 cyan, **struck through** on the
matrix" — had never been implemented; colour alone was carrying the state that means *cancelled*,
against `W` gold and `L` dark. The strike is now a fixed-width hairline rule per row, enabled only on
a voided leg. Do not re-derive its width from the text: §6 forbids geometry from content, and a test
pins it across empty/short/120-char copy.

**Gate item 3's "eight states" is not eight cash-out states** — the rectangle holds six. PRD §5 names
eight across two surfaces: five cash-out slot states plus won/lost/void. `phase-3-plan.md` had
collapsed them into one count; both facts are recorded there now. The gate word is *contradictory*,
not unique — suspended and pending-window share a treatment on purpose, so a uniqueness test fails on
a pair the design intends.

**3E is PART-CLOSED** (`4597b60`). §8.10's preview is built, tested and **deliberately unbound** —
PRD §8.10 never says what *confirming* is, and today `E` accepts on `WasPressedThisFrame` (pinned by
TVS-H01), so hold-to-preview cannot coexist with press-to-accept: acceptance fires on frame one and
no hold is observable. Binding it is one call site once the DD rules. Do not guess the gesture; it
breaks a pinned contract either way.

**§8.8's stats panel is NOT built and should not be started blind.** Two of its five required rows
cannot be sourced: the engine has **no formation and no shots concept at all**, and `Player` carries
only `Name`/`Role`/`ScoringWeight`, where `ScoringWeight` is hidden generator truth — the only
per-player number in the game is itself the leak §8.8 calls blocker-class. Per-team corners/cards
*are* available (the §7.6 fix landed; `CountLedger` tracks `Home`/`Away`). So it is three-fifths
buildable, and shipping that is a scope call, not this lead's.

**Visual evidence now EXISTS.** 98 frames bundled repo-free at
`scratchpad/tv-sweat-evidence-4597b60.zip` (57 MB): 49 new seated-sweat captures through the live
URP path plus the 49 held T6 scene-grammar frames, with a manifest. **Two gaps stated there and worth
repeating:** the seed produces no VOID leg, so 3D's strike appears in no frame; and the §8.10 preview
is unreachable while unbound, so it cannot be captured at all yet.

**3F is PART-CLOSED** (`949c041`). §7.7's binding half is built, wired at kickoff and tested:
`DotIndexFor` is the single expression both `SetBackedPlayer` and `RoutePass` use, so "the marked
actor IS the final-touch actor" holds by construction rather than by two formulas that agree today.
The locator is **wired but invisible** — the treatment is DD item 12.

### The three open DD answers, and why each blocks rather than bends

Phase 3's remaining work is **all** behind these. Every one is a case where the spec ran out before
the work did, and guessing would have broken something already pinned:

| # | Question | Why it cannot be guessed |
| --- | --- | --- |
| 10 | §8.8's two unsourceable rows | The engine has **no formation and no shots**, and `Player` carries only `Name`/`Role`/`ScoringWeight` — the sole per-player number is hidden generator truth, i.e. the leak §8.8 calls blocker-class. Three of five rows are buildable; shipping that is a scope call |
| 11 | §8.10's confirm gesture | `E` accepts on `WasPressedThisFrame`, pinned by TVS-H01. Hold-to-preview cannot coexist with press-to-accept — acceptance fires frame one, so no hold is observable. Either binding breaks a contract |
| 12 | §7.7's locator treatment | `DESIGN.md` §7's "numbered cell" is justified by "the matrix gives legible small numerals for free"; §6 records that matrix as **retired**. `TheaterStage` has no `Text`/`Font` at all, so a numeral adds a font dependency on a dead rationale, while a ring is nearly free (`RingSprite()` exists) |

**Do not pick up a fragment while these are open.** Two of the three are one-line changes once
answered; starting adjacent work instead produces more half-built features, which is how this slice
accumulated three in a row.

**When answers land:** 12 first (smallest, and it makes the locator capturable), then 11, then 10.

**Blocked on Allen, studio-level:** `[attr]lfs` is declared in `unity/SBR/.gitattributes`, but git
honours attribute macros only in a **top-level** `.gitattributes`, and this repo has none at root —
that is what the `not allowed:` warning on every git command means. So `*.png`, `*.dll`, `*.fbx`,
`*.wav` go through **no** LFS filter anywhere, including inside `unity/SBR`. The 49 phase-2
scene-grammar captures (28.8 MB) are held out of history pending that call and Allen's decision on
where they should live.

### Superseded: the T17 dispatch note this section replaced

DD ruled the scorer-gap a **correctness defect**, above every Phase 3 visual refinement. Design
instruction: **reserve, don't spend** — a scorer leg claims its backed-side goal *before* ordinary
beats spend the baked goals. If binding is ever impossible, **stage the reveal; never suppress the
win, never synthesise a reveal after resolution.** Acceptance is a **test**, not a capture: every
settled anytime-scorer leg traceable to a staged, revealed scorer event that preceded or coincided
with its resolution. The existing reproduction
(`BindAnytimeScorer_binds_nothing_when_the_backed_sides_goals_are_spent_before_the_final`,
`ScoreLedgerTests.cs`) is the red test that fix turns green — **invert it, do not delete it.**

Then: T20 px re-derivation (live progress 23→19px, resolved rows 19→15px, NEED unchanged — and do
**not** shorten §6's authored strings to fit), then 3D → 3E → 3F.

## 5. Standing context

- **Routing:** design decisions → Design Director; critical/strategy → orchestrator → Allen. Never
  straight to Allen. This lead implements approved specs and makes essentially no design calls.
- **Reporting:** result-first, telegraphic, ending `Done / Next / Risk / Need Allen`. Evidence stays
  local; raw logs never travel upward.
- **C1** — ruled 2026-07-31: latest document governs, `DESIGN.md` §6 stands, layout closed. Recorded
  in PRD §13 row A and §14.
- **C2** — light-spill colour: interim. Shipped green tolerated; target is `DESIGN.md` §5 cold
  white-grey, corrected in Phase 3. `TvLight.idleColor` is already `(0.72, 0.75, 0.80)` at
  `1aa74c3`; if green persists in-scene the residue is the room-side rig, not this file.
- **C3** — TV canvas HDR: owned here, blocks room-side fidelity. Proposal in §6.
- **Deferred by Allen, not rejected:** FIFA-style follow-cam, degrading visual register, bunkmate
  character, same-match concurrent legs (PRD §8.2A — reclassified as a betting-math feature; the
  engine forbids it at `Run.cs:181`).

## 6. Owed to integration

Recorded here rather than edited directly, per STUDIO.md's shared-docs rule:

1. **`DECISIONS.md`** — needs the C1 ruling and the Phase 1B sign-off with its waived audit-rerun gate.
2. **`design/08-art-direction.md` is deprecated** (Allen, 2026-07-24) and the game has had **no
   art authority** since. `DESIGN.md` replaced it for the TV surface only; the room, laptop and phone
   have no owning document. Allen's "high-tech city, dystopian" direction (2026-07-26) is the seed of
   the replacement. **This is a studio-level gap, not a TV one** — flagged for the Design Director.
3. **Unified post-process grade** — spec at `docs/tv-sweat-refinement/unified-grade-spec.md`, needs a
   global volume in `Room.unity`. Room lead owns implementation; this worktree owns the spec.
4. **`UnityEngine.Random` survives at `TvSweatScreen.cs` `_emissSeed`** (idle emission flicker phase).
   Found while removing T8, which took out the other use. PRD §4.3 bans the API for a *discrete scene
   choice*; a flicker phase seed is not one, so it was left alone rather than widening T8's scope.
   Consequence worth knowing: the idle flicker differs run to run. Phase 3 decides whether to move it
   onto the presentation key.
