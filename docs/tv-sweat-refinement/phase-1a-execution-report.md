# Phase 1A Execution Report

**Executed by:** Sonnet 5 execution agent
**Branch:** `slice/tv-sweat-refinement` (already checked out at task start)
**Baseline commit:** `d665438`
**Date of execution:** 2026-07-24/25 (session clock)
**Scope:** audit only. No production or test source file was modified. This document and
`BUG-LEDGER.md` are the only files changed by this agent.

A note on working-tree hygiene: this session found `design/08-art-direction.md` already modified
(untracked-from-baseline) before any command in this session ran — that change was **not** made by
this agent and is left untouched. Separately, opening/testing the Unity project in this environment
mutated three tracked files as a pure side effect of Unity's asset pipeline
(`unity/SBR/Assets/Plugins/SBR/SBR.Engine.dll`, `unity/SBR/Assets/Settings/UniversalRenderPipelineGlobalSettings.asset`,
`unity/SBR/ProjectSettings/EditorBuildSettings.asset`, `unity/SBR/ProjectSettings/ProjectSettings.asset`,
`unity/SBR/ProjectSettings/ShaderGraphSettings.asset` across the three Unity invocations below). Each
was reverted with `git checkout --` immediately after being discovered so the working tree carries no
side effects from this audit. This is itself a Part 1 finding — see §1.4.

---

## Part 1 — What can actually be executed

### 1.1 Engine test suite — RUNS

Command actually executed:

```
dotnet test engine.tests
```

Real output:

```
Test run for C:\Users\Allen\orca\workspaces\sports-betting-roguelite\tv-sweat\engine.tests\bin\Debug\net10.0\SBR.Engine.Tests.dll (.NETCoreApp,Version=v10.0)
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:   160, Skipped:     0, Total:   160, Duration: 508 ms - SBR.Engine.Tests.dll (net10.0)
```

160 passed, 0 failed. (`docs/4-unit-tests/TESTING.md` states "144 tests as of the charm expansion" —
the real run shows 160; this is a stale doc count, not an audit finding, and is noted only so the
number isn't mistaken for a discrepancy in the code under audit.)

**Side effect and correction:** the `SBR.Engine.csproj` build step copies the freshly built DLL to
`unity/SBR/Assets/Plugins/SBR/SBR.Engine.dll` as a normal post-build action. `SBR.Engine.dll` is on
the PRD §11 "must not be modified" list. The copy changed the binary's bytes (same length, different
content — almost certainly a PE timestamp/build-id difference from a non-bit-reproducible build, not
a functional change) without changing engine test outcomes. This agent ran
`git checkout -- unity/SBR/Assets/Plugins/SBR/SBR.Engine.dll` immediately after discovering it via
`git status`, restoring the tracked bytes. **Any future engine-test run in this repo layout will
reproduce this same side effect** — it is a property of the build wiring, not of this session. Future
agents/CI should either run engine tests from a worktree that doesn't share the Unity project, or plan
to `git checkout --` the DLL after every `dotnet test engine.tests` invocation.

### 1.2 Unity project — OPENS AND IS LICENSED

This environment has Unity `6000.5.3f1` installed at
`C:/Program Files/Unity/Hub/Editor/6000.5.3f1/Editor/Unity.exe`, exactly matching
`unity/SBR/ProjectSettings/ProjectVersion.txt` (`m_EditorVersion: 6000.5.3f1`). No `Library/` existed
before this session (first-ever headless open of this project on this machine).

Command actually executed:

```
Unity.exe -batchmode -nographics -quit -projectPath <repo>\unity\SBR -logFile <log>
```

Real result: Unity resolved a **Unity Personal** license via the local Licensing Client
(`License group: Product: Unity Personal, Type: Assigned, Expiration: Unlimited`), imported the
project from scratch, and exited cleanly:

```
Batchmode quit successfully invoked - shutting down!
...
Exiting batchmode successfully now!
Exiting without the bug reporter. Application will terminate with return code 0
```

**This changes the Part 1 answer from what the PRD anticipated ("Unity may not be installed,
licensed, or headless-runnable here").** In this environment Unity batchmode does run and is
licensed.

**Real side effect found:** this first open alone modified three tracked files purely from opening
the project (no test run involved): `unity/SBR/Assets/Settings/UniversalRenderPipelineGlobalSettings.asset`,
`unity/SBR/ProjectSettings/EditorBuildSettings.asset`, `unity/SBR/ProjectSettings/ShaderGraphSettings.asset`.
All three were reverted with `git checkout --`. This is a real property of running this Unity version
headlessly against this checked-in project state (likely a render-pipeline/asset-database
normalization Unity performs on first import) and will recur for any future headless open — a harness
that runs Unity in CI against this repo needs to either accept and re-normalize these files, or treat
them as expected transient diffs to discard after each run.

### 1.3 EditMode test suite — RUNS, REAL RESULTS

Command actually executed:

```
Unity.exe -batchmode -nographics -projectPath <repo>\unity\SBR -runTests -testPlatform EditMode -testResults <path>\editmode-results.xml -logFile <path>\editmode.log
```

Real result (from the NUnit3 XML `test-run` root element, not summarized from the log):

```
testcasecount="73" result="Passed" total="73" passed="73" failed="0" inconclusive="0" skipped="0"
```

Fixtures that actually executed (from the results XML): `AnytimeScorerBetslipTests`,
`BetslipModelTests`, `BookieFeedModelTests`, `DemoTicketPolicyTests`, `DeterminismEditModeTests`,
`OddsFormatTests`, `PitchLayoutTests`, `ScoreLedgerTests`, `SweatPresentationModelTests`,
`TheaterChoreographerTests`. All passed. This is real, not inferred.

Side effect: this run modified `unity/SBR/ProjectSettings/EditorBuildSettings.asset` and
`unity/SBR/ProjectSettings/ProjectSettings.asset` again; both were reverted.

### 1.4 PlayMode test suite — RUNS, REAL RESULTS, INCLUDING `TheaterStage` SCENE PLAYBACK

Command actually executed:

```
Unity.exe -batchmode -nographics -projectPath <repo>\unity\SBR -runTests -testPlatform PlayMode -testResults <path>\playmode-results.xml -logFile <path>\playmode.log
```

Real result (from the NUnit3 XML `test-run` root element):

```
testcasecount="20" result="Passed" total="20" passed="20" failed="0" inconclusive="0" skipped="0"
duration="20.4388265"
```

Every test case, by full name and real result, read directly from the results XML:

```
Passed  SBR.Tests.PlayMode.LaptopOsTests.Mirror_reveals_nothing_unseated_and_lands_engine_truth_at_settle
Passed  SBR.Tests.PlayMode.LaptopOsTests.Os_boots_switches_apps_and_keeps_chrome_over_every_tab
Passed  SBR.Tests.PlayMode.MomentumTapeTests.Tape_accumulates_dots_and_collapses_to_caps
Passed  SBR.Tests.PlayMode.PhoneTests.Disabling_focus_mid_glide_restores_camera_controller_and_cursor
Passed  SBR.Tests.PlayMode.PhoneTests.Laptop_focus_does_not_clear_unread_but_phone_focus_does
Passed  SBR.Tests.PlayMode.PhoneTests.Real_adapter_walks_a_no_bet_run_to_the_cliff_and_the_collection_text
Passed  SBR.Tests.PlayMode.PhoneTests.Second_focus_is_rejected_during_focus_in_and_focus_out
Passed  SBR.Tests.PlayMode.RoomSmokeTests.Room_LoadsWiredAndSurvivesSixtyFrames
Passed  SBR.Tests.PlayMode.TheaterStageTests.Final_scene_stages_the_plan_goals_then_completes
Passed  SBR.Tests.PlayMode.TheaterStageTests.Freezing_holds_the_exact_frame
Passed  SBR.Tests.PlayMode.TheaterStageTests.Goal_playback_reports_commit_and_chalked_variants
Passed  SBR.Tests.PlayMode.TheaterStageTests.Goal_scenes_reveal_with_the_goal_before_scene_end
Passed  SBR.Tests.PlayMode.TheaterStageTests.One_scene_per_template_plays_to_completion_and_reveals_once
Passed  SBR.Tests.PlayMode.TheaterStageTests.Pending_window_suspends_at_the_shot_and_resumes_each_way
Passed  SBR.Tests.PlayMode.TheaterStageTests.Pulse_kicks_territory_toward_the_beneficiary_then_decays
Passed  SBR.Tests.PlayMode.TheaterStageTests.Territory_restates_the_live_probability
Passed  SBR.Tests.PlayMode.TvAudioDirectorTests.Director_builds_fires_and_ducks_without_throwing
Passed  SBR.Tests.PlayMode.TvSweatScreenTests.FullRound_TwoTickets_SweatsSeriallyToSettleAndShop
Passed  SBR.Tests.PlayMode.TvSweatScreenTests.StandingMidSweatFreezesTheEventCursor
Passed  SBR.Tests.PlayMode.TvSweatScreenTests.ZeroTicketLockSettlesOnTheSpot
```

Side effect: this run again modified `unity/SBR/ProjectSettings/EditorBuildSettings.asset` and
`unity/SBR/ProjectSettings/ProjectSettings.asset`; both were reverted again.

### 1.5 Can PlayMode tests that exercise `TheaterStage` scene playback run at all?

**Yes, confirmed by actual execution, not inference.** `TheaterStageTests` (8 test cases — final-scene
staging, freeze-holds-exact-frame, goal commit/chalk variants, goal reveal ordering, the existing
one-scene-per-template-at-variant-0 sweep, pending-window suspend/resume both ways, territory pulse,
territory probability restatement) and `TvSweatScreenTests` (3 test cases, including the two-ticket
full-round serial sweat and the standing-freezes-the-event-cursor test) all ran headlessly under
`-nographics -batchmode` and all passed. This directly contradicts the PRD's working assumption
("Unity may not be installed, licensed, or headless-runnable here... Almost certainly not") for this
specific environment. **The correct, verified-by-execution statement for this environment is: Unity
PlayMode tests, including ones that drive `TheaterStage` scene playback end-to-end, run here.** This
is the single most consequential Part 1 finding — see the final summary.

Important scope boundary: this confirms the **harness can execute** PlayMode scene-playback tests. It
does not mean the PRD §6.2 48-cell matrix has been executed — only the pre-existing 8+3 PlayMode
tests were run, unchanged, exactly as they already existed in the repo. Building the actual 16×3
enumeration, the stand/resume-at-four-positions parameterization, and the full §6.3 market/transition
matrix is new test-authoring work this agent's scope (audit only, no test seams beyond what's
"approved in the implementation brief") does not include. What Part 1 establishes is that a future
Phase 1B/1A-continuation agent authoring those tests will find them **runnable** in this environment,
not blocked.

### 1.6 Can any screenshot or video evidence be captured in this environment?

**No.** This is a headless `-nographics -batchmode` Unity invocation with a Null graphics device
(confirmed in the Unity log: `Forcing GfxDevice: Null`, `NullGfxDevice: Version: NULL 1.0 [1.0]`,
`Renderer: Null Device`). A null device does not rasterize frames, so there is nothing to capture —
`ScreenCapture.CaptureScreenshot` or a render-texture readback would either throw, return a blank/black
buffer, or silently no-op depending on Unity version behavior; none of that was tested because there
is no display pipeline underneath it to validate against. No screenshot or video was captured or
attempted to be captured in this session, and none is claimed. Nothing in this repo's CI/harness setup
(no headless-with-GPU runner, no software rasterizer flag, no Unity Recorder invocation) was found
that would change this. **Visual and motion evidence per PRD §6.1/§6.2 cannot be produced in this
environment as configured.** A different harness (a GPU-backed runner, or an interactive Editor
session) is required for that evidence class specifically — this does not block source-verdict work
or pass/fail assertions from test runners, only screenshot/video artifacts.

### 1.7 What this means for the PRD §6.2 48-cell matrix

- The matrix's **mechanical** requirements (starts, completes within timeout, reveals exactly once,
  produces only its permitted payoff marker, leaves possession/actor state valid, does not move
  score/count before the visual payoff, leaves the next scene able to start) are the kind of thing an
  automated PlayMode test can assert without rendering a frame — see §1.4-RESULT for whether the
  existing suite already covers any of this and whether new automated cells could run here.
- The matrix's **evidence** requirements (screenshot for visual defects, video for motion/timing
  defects) cannot be produced in this environment per §1.6. Any bug row this agent could file from
  real execution would have to omit the screenshot/video artifact and say so honestly, not substitute
  a description for the required image.
- Full 16×3 enumeration, stand/resume-at-four-positions, and the market/transition matrix in §6.3 are
  execution work, not source-analysis work, and were not attempted beyond what the existing suite
  already covers, per this agent's Part 1 scope (establish what's executable, don't silently start
  executing the full matrix without sign-off on the harness gap in §1.6).

---

## Part 2 — TVS-H01 / TVS-H02 / TVS-H03 source verdicts

Per PRD §6.1, all three remain: seed/round/ticket/market = `NOT CAPTURED — STATIC REVIEW`,
reproduction = `NOT RUN`, status = `HYPOTHESIS` unless and until a runtime session reproduces them.
Nothing below is a claim of observed behavior. `BUG-LEDGER.md` §4 carries these verdicts.

### 2.1 TVS-H01 — cash-out input reservation vs. legal acceptance

**Verdict: CONFIRMED-BY-SOURCE.** Two different predicates gate the same physical Interact press, and
they diverge in exactly the two states the PRD names (suspended, price-animating).

**Predicate 1 — input reservation** (`TvSweatScreen.cs:442-443`):

```csharp
private bool CashOutLive()
    => _session != null && !_session.IsComplete && _eventsEmitted >= 1 && _session.CashOutOffer().HasValue;
```

Wired as the stand-suppression hook at `TvSweatScreen.cs:414`:
`SitSpot.InteractStandSuppressed = CashOutLive;`. `SitSpot.cs:83-89` consumes it:

```csharp
else if (_state == State.Seated)
{
    if (InteractStandSuppressed != null && InteractStandSuppressed())
        return;
    StartCoroutine(StandUp());
}
```

`SitSpot.cs:25-28`'s own doc comment states the intent plainly: *"while this returns true (a live
cash-out offer is showing), an Interact press must NOT stand the player up."* `CashOutLive()` never
reads `_marketSuspended` or `_cashOutAnimation`.

**Predicate 2 — legal acceptance** (`TvSweatScreen.cs:1755-1762`):

```csharp
private void TryCashOut()
{
    if (_marketSuspended) return; // the book is off the market mid-scene (M-T3.1)
    if (_cashOutAnimation != null) return; // the price is settling — the displayed and
                                           // accepted number must never differ (Sol, M-T4)
    if (!_seated || _session == null || _session.IsComplete || _eventsEmitted < 1) return;
    double? offer = _session.CashOutOffer();
    if (!offer.HasValue) return;
    ...
```

Both predicates respond to the **same** Interact press: `TvSweatScreen.cs:1751-1752`
(`if (_interact != null && _interact.WasPressedThisFrame()) TryCashOut();`) fires every frame
regardless of what `SitSpot.OnInteract` decided about standing.

**What this produces:** whenever an offer exists (`CashOutOffer().HasValue`) but `_marketSuspended` is
true, or `_cashOutAnimation != null` (mid-tick), `CashOutLive()` still reports `true`, so
`InteractStandSuppressed()` returns `true` and `SitSpot` refuses to start `StandUp()` on that press.
In the same frame, `TryCashOut()` bails at its first or second guard and performs no cash-out. Net
effect: the press does **neither** — it neither stands the player up nor accepts a cash-out — during
exactly the suspended/updating windows the PRD calls out. The expected contract
(`BUG-LEDGER.md` TVS-H01 row) is that in those states the player "retains the normal stand contract";
source shows that contract is not retained.

### 2.2 TVS-H02 — literal-pause coverage across ceremony/cash-out/effect/tally/transition timers

**Verdict: CONFIRMED-BY-SOURCE, with a precise boundary.** `TheaterStage.cs`'s entire `Update()` is a
single top-level seating gate — every timer inside the stage class is correctly paused. The violation
is entirely inside `TvSweatScreen.cs`'s own coroutines and per-frame animators, which run independent
of the stage's frozen state. `SweatPacer.cs` holds no runtime state (pure duration lookup table); it
is not itself a pause-law participant.

**`TheaterStage.cs` — compliant by construction:**

`TheaterStage.cs:427`: `if (!_live || _frozen) return;` is the *only* gate in `Update()`, and it wraps
`UpdateFlash(dt)`, `UpdateScene(dt)`/`UpdateIdle(dt)` — i.e. every stage-internal timer (`_stepT`
step-advance at line 455-456, flash/goal/corner/booking timers, idle behavior) is unreachable while
`_frozen`. `_frozen` is driven every frame from seating at `TvSweatScreen.cs:1747`:
`_stage.SetFrozen(!_seated);`. No further citation needed per timer inside this file — one gate covers
all of them.

**`TvSweatScreen.cs` — the enumerated timer/coroutine/accumulator table:**

| # | Timer / coroutine | Location | Advances via | Consults `_seated`? | Reachable while standing |
|---|---|---|---|---|---|
| 1 | `TickClock` (match clock) | `TvSweatScreen.cs:815-823` | `Time.deltaTime` (line 820) | **Yes** — line 818 `if (!_seated \|\| !_stage.ScenePlaying \|\| _stage.SuspendedAtShot) return;` | No — correctly gated |
| 2 | `SeatedHold(ms)` | `TvSweatScreen.cs:1865-1874` | `Time.deltaTime`, but only decremented `if (_seated)` (line 1871) | **Yes**, by construction | No — correctly gated |
| 3 | `WaitSceneDone` | `TvSweatScreen.cs:678-689` | polls `_stage.ScenePlaying` (itself gated, see above) | Indirectly yes | No — correctly gated |
| 4 | `ScaledWait(seconds)` | `TvSweatScreen.cs:1876-1881` | `t += Time.deltaTime` (line 1880) | **No** | **Yes** — advances while standing |
| 5 | `WaitRealtime(seconds)` | `TvSweatScreen.cs:1883-1887` | `t += Time.deltaTime` (line 1886) | **No** | **Yes** |
| 6 | `AnimateCashOut` (cash-out number tick/interpolation) | `TvSweatScreen.cs:1451-1483` | `elapsed += Time.deltaTime` (line 1466) | **No** | **Yes** |
| 7 | `FloodPulse` (green/gold flood used by leg-won, win, cash-out) | `TvSweatScreen.cs:1706-1719` | `t += Time.deltaTime` (line 1713) | **No** | **Yes** |
| 8 | `GreenLegBeat` | `TvSweatScreen.cs:1548-1555` | delegates to `FloodPulse` (#7) | **No** | **Yes** |
| 9 | `DeadLegBeat` static-regen loop | `TvSweatScreen.cs:1557-1569` | `WaitRealtime` (#5) × `staticRegens` | **No** | **Yes** |
| 10 | `DeadLegBeat` red-line hold | `TvSweatScreen.cs:1578` | `ScaledWait` (#4) | **No** | **Yes** |
| 11 | `TicketDeadBeat` dim-overlay ramp | `TvSweatScreen.cs:1586-1592` | `t += Time.deltaTime` (line 1589) | **No** | **Yes** |
| 12 | `TicketDeadBeat` silence hold | `TvSweatScreen.cs:1594` | `ScaledWait` (#4) | **No** | **Yes** |
| 13 | `TicketDeadBeat` consolation hold | `TvSweatScreen.cs:1608` | `ScaledWait` (#4) | **No** | **Yes** |
| 14 | `WinBeat` payout tally | `TvSweatScreen.cs:1623-1631` | `elapsed += Time.deltaTime` (line 1627) | **No** | **Yes** |
| 15 | `WinBeat` post-tally hold | `TvSweatScreen.cs:1633` | `ScaledWait` (#4) | **No** | **Yes** |
| 16 | `WinConfetti` physics loop | `TvSweatScreen.cs:1667-1683` | `elapsed`/`storyDt += Time.deltaTime` (lines 1671-1672) | **No** | **Yes** |
| 17 | `CashOutFloodBeat` | `TvSweatScreen.cs:1695-1704` | delegates to `FloodPulse` (#7) | **No** | **Yes** |
| 18 | `SettlementBeat` cash-out-flood hold | `TvSweatScreen.cs:905` | `ScaledWait` (#4) | **No** | **Yes** |
| 19 | `SettleCardBeat` hold | `TvSweatScreen.cs:1214` | `ScaledWait` (#4) | **No** | **Yes** |
| 20 | `PendingWindowBeat` post-decision hold (Mulligan/Whistle confirm line) | `TvSweatScreen.cs:864, 884` | `ScaledWait` (#4) | **No** (the key-press decision itself IS gated on `_seated` at lines 855/867/887; the *confirmation hold after the decision* is not) | **Yes**, for the hold only |
| 21 | `ApplyEmission` (idle emission flicker/decay) | `TvSweatScreen.cs:1786-1794` | `Time.deltaTime` (line 1789), `Time.time` (line 1791) | **No** — called unconditionally every `Update()` (line 1726) | **Yes** |
| 22 | `AnimateBar` (win-prob bar breathing/punch) | `TvSweatScreen.cs:1802-1813` | `Time.deltaTime` (lines 1805, 1809) | **No** — called unconditionally every `Update()` (line 1727) | **Yes** |
| 23 | `AnimateFlavorPunch` | `TvSweatScreen.cs:1831-1836` | `Time.deltaTime` (line 1834) | **No** — called unconditionally every `Update()` (line 1728) | **Yes** |
| 24 | `AnimateCashOutTaunt` (cash-out text scale/flash) | `TvSweatScreen.cs:1817-1829` | `Time.deltaTime` (lines 1820-1822) | **No** — called unconditionally every `Update()` (line 1729) | **Yes** |

Rows 4–24 (21 of 24 enumerated timers) advance from raw `Time.deltaTime` with no `_seated` check and
are reachable while the player is standing — matching and substantially extending the
`current-state-audit.md` / `BUG-LEDGER.md` TVS-H02 finding ("`ScaledWait`, `WaitRealtime`, cash-out
interpolation, ticket-dead dimming, win tally/confetti, and multiple flood/punch timers"). Rows 1–3
are the correctly-gated counterexamples that prove the mechanism (`_seated`-gated accumulation, or
gating via `_stage.ScenePlaying`) is known and used elsewhere in the same file — this is not a missing
capability, it is inconsistent application of an existing pattern.

### 2.3 TVS-H03 — final scorer identity vs. the visible final-touch actor

**Verdict: CONFIRMED-BY-SOURCE — no identity binds both, in any current code path, for any market.**
This is stronger than the ledger's existing phrasing ("the named scorer is therefore not bound to the
visible final touch"); source shows there is no mechanism by which it *could* be bound today, not
merely a gap in wiring one call.

**1. The actor-naming call is structurally unreachable during any final path.**
`TvSweatScreen.cs:568` gates the entire non-final beat-resolution block:
`if (evt.Type != DramaEventType.LegFinal) { ... PrepareScoringActor(leg, spec.Goal.Value); ... }`
(call at line 596). The final path is `TvSweatScreen.cs:643-673` (`TheaterBeat`'s pending-loss branch
and its non-pending `else` branch) — neither branch calls `PrepareScoringActor` anywhere in that
range. Since `PrepareScoringActor` only exists inside the `!= LegFinal` block, it is unreachable for
any goal that plays during a final sequence, for any market.

**2. For the one market that most needs it, the call is a no-op even when reached.**
`ScorerFor` (`TvSweatScreen.cs:1082-1094`), which `PrepareScoringActor` calls first
(`TvSweatScreen.cs:1071`), contains:

```csharp
if (leg.Selection.Kind == MarketKind.AnytimeScorer && !_finalSequenceActive) return null;
```

(`TvSweatScreen.cs:1088`.) `PrepareScoringActor` bails immediately if `scorer == null`
(`TvSweatScreen.cs:1072`). So for an `AnytimeScorer` leg: during non-final beats `_finalSequenceActive`
is false, so `ScorerFor` returns `null` and `PrepareScoringActor` does nothing; during final beats,
point 1 means `PrepareScoringActor` is never called at all. **`SetScoringActor` is never invoked for
an anytime-scorer leg, at any point in the sweat, in the current code.**

**3. Even if invoked, `SetScoringActor` has no causal link to which actor visually scores.**
`TheaterStage.cs:344-349`:

```csharp
public void SetScoringActor(bool home, int rosterIndex, string playerName)
{
    Image[] dots = home ? _homeDots : _awayDots;
    if (dots == null || dots.Length == 0) return;
    dots[Mathf.Abs(rosterIndex) % dots.Length].gameObject.name = playerName;
}
```

This sets a Unity `GameObject.name` — an internal engine object identifier, not a rendered value.
`_homeDots`/`_awayDots` are plain `Image` circles with only `.color` ever set
(`TheaterStage.cs:220-221, 268-269`); no `Text`/`TextMeshProUGUI` component is attached to a dot
anywhere in `TheaterStage.cs` (confirmed by exhaustive search — the only "Text" hits in the file are
an unrelated code comment). The renamed dot's identity has no rendered presence for the player to see.

Separately, which dot actually carries the ball/takes the shot is chosen entirely by spatial proximity
to authored waypoints — `EnterStep` (`TheaterStage.cs:479-525`) picks `_routeDotIx` via
`NearestOutfield`/`NearestBackLine` (lines 491, 501), and `RouteShot` (lines 505-516) does not even
select a dot — it drives `_stepBallLocal` to a fixed local target point independent of any actor.
`CompleteStep` (`TheaterStage.cs:529-580`) then updates `_carrierHome`/`_carrierIx` purely from
`s.Route`/`s.AtkPicked`. Nothing in this selection path reads `rosterIndex`, the renamed dot, or any
value `SetScoringActor` touches.

**4. The displayed scorer name is a separate read from the locked stat line, independent of the
stage's route/carrier state.** `OnGoalPlayed` (`TvSweatScreen.cs:738-769`) calls `ScorerFor` again at
payoff (line 741) and builds the flavor-line text from `scorer.Name` (lines 764-766) — this reads
`leg.Matchup.StatLine.HomeScorers`/`AwayScorers` indexed by `_ledger.Picked`/`_ledger.Opponent`
(`TvSweatScreen.cs:1091-1093`), a pre-baked list keyed by goal count, with zero reference to which dot
the stage animated as the shot-taker.

**5. `TheaterChoreographer.cs` (the third owning file) confirms it has no player-identity concept at
all** — its full `ResolveBeat`/`ResolveFinal` surface (`TheaterChoreographer.cs:41-145`) operates only
on `SceneTemplate`, team-side beneficiary (`ScoredByPicked`), and count/goal staging; no `Player`,
roster index, or actor reference appears anywhere in the file.

**Conclusion:** the named scorer (text) and the visible final-touch actor (route/carrier selection)
are two entirely disconnected systems today, not a partially-wired one. There is no scenario, current
or hypothetical-with-today's-code, in which the two agree by anything other than coincidence.

---

## Part 3 — Two new areas (§7.6, §7.7), source analysis only

### 3.1 §7.6 — corner/card team attribution

**Finding: the beneficiary carried through to `TheaterStage` is NOT team attribution — it is the
bettor's Over/Under pick, which has no team meaning for these markets. This is unreliable exactly as
the PRD anticipated, and per the PRD's own words ("if the beneficiary is unreliable, that is a
blocker, not a polish item") this is a Phase 2 blocker, not a Phase 1A bug row.**

The engine-level selection for these two markets is Over/Under only — there is no team-scoped choice:
`engine/Domain.cs:21` — `public enum MarketChoice { Home, Away, Over, Under, Yes, No }` — and
`engine/Domain.cs:75-79`:

```csharp
public static MarketSelection TotalCorners(double line, bool over)
    => new MarketSelection(MarketKind.TotalCorners, line, over ? MarketChoice.Over : MarketChoice.Under);
public static MarketSelection TotalCards(double line, bool over)
    => new MarketSelection(MarketKind.TotalCards, line, over ? MarketChoice.Over : MarketChoice.Under);
```

`TheaterChoreographer.ResolveBeat` (`TheaterChoreographer.cs:44-74`) stages the count scene from:

```csharp
bool countHelps = leg.Selection.Choice == MarketChoice.Over;   // line 59
...
SceneTemplate countTemplate = corners
    ? (countHelps ? SceneTemplate.CornerFor : SceneTemplate.CornerAgainst)
    : SceneTemplate.Booking;                                    // lines 66-68
...
return new SceneSpec(countTemplate, variant, countIntro, evt.Tag == TensionTag.Swing,
    countHelps, null, count, null, market, ...);                // line 70-72, countHelps → ForPicked
```

`countHelps` is *"does this count event help the Over/Under bettor,"* not *"which team earned it."*
`ScenePlaybook.cs:49-52`'s own doc comment on the `SceneSpec.ForPicked` field it becomes states this
plainly: *"Whether the beat's beneficiary (the side running the move) is **the picked team**."* For a
team-neutral Over/Under market, "the picked team" is not a real team — it is a synonym for the
bettor's Over/Under direction being reused as if it were a side of the pitch.

The true per-team fact **does** exist and **is** computed: `SweatPresentationModel.cs:373-386`'s
`StagedCount` struct carries `HomeDelta`/`AwayDelta` explicitly, and
`SweatPresentationModel.cs:440-443` sources the season/match totals honestly from the locked stat
line: `ConfigureEndpoint(statLine.HomeCorners, statLine.AwayCorners, beatCount)` /
`(statLine.HomeCards, statLine.AwayCards, beatCount)`. But `TheaterStage.cs` never reads
`HomeDelta`/`AwayDelta` anywhere (confirmed by exhaustive search of the file — zero matches) when
building the corner/booking scene. Instead, `TheaterStage.cs:851-866` (`CornerFor`/`CornerAgainst`)
and `TheaterStage.cs:868-879` (`Booking`) pick attacking direction from the template chosen above
(`CornerAgainst` → `core = Mirror(core)` at line 864) or from `atkPicked: spec.ForPicked` directly
(line 871-876, the same `ForPicked`/`countHelps` value) — the same Over/Under flag, not
`HomeDelta`/`AwayDelta`.

**Consequence:** the visual "team that wins the corner / commits the foul" required by §7.6 is
currently derived from whether the total-count movement helps an Over or Under bettor, which is
unrelated to which team the engine actually credited with that specific corner/card. A beat where
`AwayDelta` is the only nonzero delta could still stage a `CornerFor` scene showing the picked side
(home, say) driving into the attacking third and winning the corner, if `countHelps` happens to be
true — because the two values are computed independently and neither reads the other. This is not an
occasional edge case; it is the only mechanism that exists, so it is wrong by construction whenever
"which team actually earned the delta" and "does the delta help Over/Under" diverge, which they can on
any beat. **This must be resolved — by threading `HomeDelta`/`AwayDelta` (or an equivalent true
per-event team fact) into the scene builder — before Phase 2 builds market-attributed corner/card
scenes on top of the current `ForPicked` plumbing.**

### 3.2 §7.7 — backed-player locator: what exists vs. what's structurally missing

Per the task brief, this is a structural inventory only — no locator design is proposed here.

**What already exists and is usable today:**

- The backed player's **identity** (not outcome) is already known independent of any goal event:
  `engine/Domain.cs:82-83` — `MarketSelection.AnytimeScorer(int playerIndex)` — and
  `engine/Domain.cs:283` — `Matchup.PlayerAt(int playerIndex)` — resolve `leg.Selection.PlayerIndex` to
  a `Player` (with `.Name`) at leg-start, well before any final reveal. This is exactly what already
  powers the existing `[PLAYER] TO SCORE` active-leg copy per PRD §8.2 and the betslip identity path
  (`AnytimeScorerBetslipTests`). A locator does not need new plumbing to know *who* is backed.
- A per-actor rename hook exists in principle: `TheaterStage.SetScoringActor(bool home, int
  rosterIndex, string playerName)` (`TheaterStage.cs:344-349`).

**What is structurally missing:**

1. **No rendering surface on any stage actor.** `_homeDots`/`_awayDots` are plain `Image` circles
   created by `MakeDot` (`TheaterStage.cs:1289`, instantiated at lines 220-221) with only `.color` ever
   set. There is no `Text`/`TextMeshProUGUI` component attached to a dot anywhere in the file, so
   nothing today can render a jersey number, surname tag, or ring/chevron/halo without adding a new
   child element and draw path per dot.
2. **No stable, continuous roster-to-dot identity.** Dot "roles" are reassigned every scene-step by
   spatial-nearest-neighbor selection (`NearestOutfield`/`NearestBackLine`,
   `TheaterStage.cs:479-524`), not by any persistent binding of "dot index 3 is player X." The only
   existing identity-tagging call (`SetScoringActor`) is reveal-scoped and one-shot (fires once per
   goal beat, non-final only, per §2.3 above) — it is not a continuous per-frame binding, which is
   exactly the new hard constraint PRD §7.7 adds ("actor binding is now continuous, not reveal-only").
   No mechanism in the current code tracks "this dot represents the backed player" across an entire
   sweat.
3. **No jersey/shirt-number field exists on the engine `Player` model at all.**
   `engine/Domain.cs:184-198`:
   ```csharp
   public sealed class Player
   {
       public string Name { get; }
       public PlayerRole Role { get; }
       public double ScoringWeight { get; }
       ...
   }
   ```
   There is no number field. `engine/**` is on the PRD §11 "must not be modified" list and engine
   changes are explicitly out of scope (§3). A "jersey numerals" treatment (one of the PRD's own
   candidate options) cannot source a real per-player number from engine data under the current
   ownership boundary — any numeral would have to be a presentation-side synthesis (e.g. a
   deterministic hash of roster index), not authentic squad-number data, unless this constraint is
   escalated as a decision gate.
4. **The one existing "identity on an actor" mechanism is the same one §2.3 shows is unbound from the
   actual scoring touch.** Reusing `SetScoringActor` as-is for a continuous locator would inherit the
   exact defect TVS-H03 documents — the named/marked dot has no causal relationship to which dot the
   route/carrier logic actually moves through a chance or a shot. PRD §7.7 explicitly raises TVS-H03's
   severity for this reason ("what was a reveal-time copy issue becomes a whole-sweat identity
   contract") — this agent's Part 2 finding is the concrete evidence behind that severity raise:
   Phase 2 cannot build a continuous, outcome-safe locator on top of `SetScoringActor` until the
   final-touch binding gap is closed, because a locator inherits the same unbound mapping and would
   risk an actual outcome leak (marking one dot as "backed" while a structurally-unrelated dot performs
   the goal) rather than merely a cosmetic mislabel.

No feature design is proposed here per the task boundary; the above is what the design track has to
build against and what it cannot assume already exists.

---

## Summary for the technical product lead

- **Executed, not guessed:** engine tests (160/160 pass), Unity project open (licensed, clean import),
  EditMode tests (73/73 pass), PlayMode tests including `TheaterStageTests`/`TvSweatScreenTests`
  (20/20 pass) — all with real commands and real output pasted above, and all incidental tracked-file
  side effects reverted before this report was written.
- **The PRD's Part 1 premise needs correcting for this environment:** Unity is installed, licensed
  (Unity Personal), and runs EditMode/PlayMode headlessly here, including scene-playback PlayMode
  tests. The one confirmed hard blocker is screenshot/video evidence — this environment runs Unity
  with a Null graphics device (`-nographics`), so no frame is ever rasterized and no visual/motion
  evidence for PRD §6.1/§6.2 can be captured here. A GPU-backed or interactive-Editor harness is needed
  for that evidence class specifically; it is not needed for pass/fail test execution.
- **TVS-H01, TVS-H02, TVS-H03: all three CONFIRMED-BY-SOURCE**, each with exact `file:line` quotes
  above and no runtime claims attached (reproduction remains `NOT RUN` per PRD §6.1).
- **§7.6 is a Phase-2 blocker per the PRD's own escalation rule:** the corner/card "beneficiary" piped
  into `TheaterStage` is the bettor's Over/Under pick, not the engine's true per-team delta
  (`HomeDelta`/`AwayDelta`, which is computed and then never read for this purpose).
  **§7.7 has no continuous per-actor identity or rendering surface to build on** and would inherit the
  TVS-H03 defect if it reused the existing `SetScoringActor` mechanism as-is.
