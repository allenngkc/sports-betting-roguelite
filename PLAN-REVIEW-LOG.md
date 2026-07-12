# Plan Review Log: M5 — the phone is the bookie

Act 1 (grill) complete — plan locked with the user. MAX_ROUNDS=5.
Grill resolved: phone = bookie's voice (text thread, immediate buzz, visual-only cue); UX = second
DeskFocus top-down (pick-up deferred); voice = person, warm→cold, deadpan; triggers = all seven
beats (with the one-round-loan correction folded in — see PLAN.md Key decisions).

## Round 1 — Codex
Review blocked: the Windows sandbox denied every read command at process creation
(`CreateProcessAsUserW failed: 5`), so neither `PLAN.md` nor repository files were accessible.
Fix: restore read-capable shell execution for the workspace and rerun the review; approving an
uninspected plan would be unsound.

VERDICT: REVISE

### Claude's response
Environment finding, not a plan finding. Codex's Windows sandbox ("elevated" in config) cannot
spawn processes from this non-elevated session. Not weakening the sandbox (hard rule: Codex never
writes). Instead Round 2 resumes the same thread with the materials INLINE: full PLAN.md +
engine/Run.cs (debt logic) + RunDirector.cs + BuildPhone + RoomSmokeTests expectations. With its
shell unusable, Codex cannot write anything - the strictest read-only available.

## Round 2 — Codex

1. The current run seed is not exposed. `RunDirector.seed` is only the configured first-run seed; blank starts and every later `NEW RUN` use a local `runSeed`, so hashing `seed#round#trigger` will use stale or empty data.
Fix: Add `CurrentRunSeed` to `RunDirector`, assign it in `StartNewRun(string runSeed)`, and include it in every model snapshot.

2. Snapshot polling has an unspecified deduplication contract. `LastSettle` persists through Shop and the following Betting phase, so condition-based detection can emit `FLOAT`, `CLEARED`, `COLLECTION`, or `VERDICT` every frame and again after `ExitShop`.
Fix: Track processed settle identity as `(RunGeneration, LastSettle.Round)` and betting reminders as `(RunGeneration, Run.Round)`, emitting each trigger once.

3. Settle and current-round numbers diverge after `ExitShop`. `LastSettle.Round` remains the settled round while `Run.Round` increments, so a delayed float/clear observation can receive the wrong timestamp and hash key.
Fix: Derive settle-trigger rounds exclusively from `LastSettle.Round`; use `Run.Round` only for `DEBT_BETTING`.

4. Valid simultaneous triggers are not defined. Clearing debt on the final round produces both `DebtCleared == true` and `Outcome == RunWon`; an `else-if` state machine drops one, while independent checks need deterministic ordering and unread behavior.
Fix: Specify ordered multi-emission per snapshot, at minimum `CLEARED` before winning `VERDICT`, and test both entries exactly once.

5. `message count + focus` is not a valid render signature. A new run can replace a one-message old thread with a one-message welcome while preserving the signature, leaving stale text rendered.
Fix: Give the model a monotonically increasing `Revision` changed on append, reset, and read-state changes, and render from that revision.

6. `DeskFocus` is not generic as claimed: its prompt is hard-coded to `"Use laptop"`, so the phone will advertise the wrong action.
Fix: Add a serialized prompt label or interaction noun and configure the phone as `"Check phone"`.

7. Two `DeskFocus` instances can concurrently acquire camera control. `OnInteract` never checks `DeskFocus.Active`, and `Active` is not claimed until the focus-in coroutine finishes, allowing overlapping glides and corrupted return poses/cursor state.
Fix: Reserve global focus ownership before starting `FocusIn`, reject other instances until release, and clear ownership on all abort/disable paths.

8. “Phone focused” has no explicit identity wiring. Treating any non-null `DeskFocus.Active` as focused would make opening the laptop clear phone unread messages.
Fix: Give `BookieFeed` a reference to the phone’s specific `DeskFocus` and clear unread only when `DeskFocus.Active == phoneFocus`.

9. The FLOAT copy contradicts the surface split and may report the wrong amount. The plan says numbers stay off the phone, while FLOAT includes an amount; additionally `DebtAfter` is principal plus juice, not the amount actually “covered.”
Fix: Remove the numeric amount, or explicitly choose debt owed versus principal and add the required calculation and invariant money formatting.

10. The advertised determinism key is incomplete. Warm/cold FLOAT, normal/final `DEBT_BETTING`, and won/lost `VERDICT` select different pools despite sharing `seed#round#trigger`, so the claim “same tuple means same line” is false without hidden state.
Fix: Include the semantic subtype/tone tier in the stable key or represent these outcomes as distinct trigger kinds.

11. Initial welcome behavior depends on initialization order. If the model initializes its remembered generation from the already-started director, it will never observe a generation change and the welcome integration test becomes script-order-dependent.
Fix: Define the first valid snapshot as a run-start event regardless of prior sentinel value, with null-`Run` snapshots ignored safely.

12. New-run reset semantics are incomplete. Resetting only the message list leaves `floatCount`, processed-settle keys, unread count, or render revision capable of leaking across runs.
Fix: Specify and test an atomic reset of all per-run state before appending the new welcome.

13. The proposed tests do not cover the main polling risks. Trigger-path walks omit repeated identical snapshots, Shop-to-Betting persistence, delayed processing after `ExitShop`, final clear-plus-win, and warm-tier restoration after reset.
Fix: Add exact-sequence tests that poll every snapshot multiple times and assert ordered messages, counts, rounds, and tiers after each transition.

14. The sole PlayMode integration assertion only proves startup wiring. It does not validate actual engine-driven float/reminder timing, deduplication, phone-specific read clearing, prompt text, focus exclusion, or equal-count thread rebuilds.
Fix: Add PlayMode flows for one real float-to-Betting transition, phone versus laptop focus/read behavior, and a new-run render replacement.

15. “RNG-purity check” is not defined strongly enough to support the claim. Repeated-line determinism does not prove no engine stream advanced.
Fix: Compare two same-seed engine runs, observe one through the feed only, then drive both through subsequent slate/outcome/shop generation and assert identical engine outputs.

VERDICT: REVISE

### Claude's response (rev 2 of PLAN.md)
ACCEPTED (12): #2 dedup keys specified ((gen, LastSettle.Round) / (gen, Run.Round)); #3 settle
triggers stamp+hash LastSettle.Round; #4 ordered multi-emission (CLEARED before VERDICT_WON,
final-round debt-clear win tested emitting both); #5 monotone Revision replaces count+focus
signature; #6 DeskFocus gains a serialized prompt field; #7 focus ownership claimed BEFORE the
glide, cleared on out/disable/domain-reset; #8 BookieFeed holds the phone's DeskFocus, unread
clears only on that instance; #10 subtypes became distinct kinds (FLOAT_WARM/COLD, DEBT_BETTING/
NO_MORE_FAVORS, VERDICT_WON/BUST) so the hash key is complete; #11 first non-null snapshot IS run
start; #12 atomic per-run reset, Revision never resets; #13 all listed test gaps added verbatim;
#15 RNG purity upgraded to the two-run compare-through-lock form.
REDIRECTED (1): #1 — the seed already exists as Run.Rng.RunSeed (used by the TV chrome); plan
pins that as the source instead of adding director API.
REJECTED with reasons (1.5): #9 first half — the FLOAT text keeps its dollar amount: a bookie
naming what you owe is voice, not a dashboard; the "which number" half is ACCEPTED and pinned to
SettleReport.DebtAfter. #14 partially — PlayMode will not re-drive full engine float flows: the
model state machine is exhaustively covered in EditMode with deterministic synthetic reports,
and engine-driven floats in PlayMode need seed-hunting (nondeterministic value per cost); PlayMode
instead gains the focus-identity, prompt, unread-clearing and ownership-claim flows Codex asked
for in the same finding.

## Round 3 — Codex

Arbitration (a) is sound: `Run.Rng.RunSeed` supplies the actual current seed without consuming RNG. Arbitration (b) is coherent: `DebtAfter` accurately names principal plus juice, and invariant money formatting resolves presentation ambiguity. Arbitration (c) is reasonable for state-machine logic, but leaves one integration boundary uncovered below.

1. `Revision` cannot distinguish message arrival from reset/read changes. Buzzing on every revision causes a buzz when messages are read; buzzing only when count increases misses an equal-count new-run welcome.
Fix: Add a monotone `ArrivalSequence` incremented only when messages append, and drive buzz/pulse from it while rendering from `Revision`.

2. Delayed observation can expose a float report and the following indebted Betting phase in the same snapshot, but ordering is undefined; `DEBT_BETTING` could appear before `FLOAT_WARM`.
Fix: Define snapshot processing order as run reset, unseen settle triggers, then unseen betting trigger, and assert FLOAT precedes the reminder in the delayed-observation test.

3. The ownership wording can lock out the active instance’s own `"Back"` interaction if the global claim check rejects whenever any owner exists.
Fix: Apply the ownership rejection only when entering from `Idle` and the owner is another instance; always permit the owning focused instance to begin `FocusOut`.

4. The ownership test covers “engaged” focus but not the race window the change is intended to close: acquisition begins before the first glide completes, and ownership must also persist through focus-out.
Fix: Test a second interaction immediately after starting focus-in and again during focus-out, asserting both are rejected until completion.

5. Clearing ownership in `OnDisable` without unwinding active camera control can strand the controller, cursor, or camera if a focused object is disabled mid-transition.
Fix: In `OnDisable`, stop glides and restore cursor/controller state before releasing ownership, with a test for disabling during focus-in or focused state.

6. `BookieFeedModelTests` bypass the actual `BookieFeed → RunDirector` snapshot adapter, while the welcome PlayMode assertion exercises only run generation; a wrong mapping of `LastSettle`, debt, round, or seed would leave all trigger tests green.
Fix: Add one narrow adapter-level integration test using a real `RunDirector` settle report; it need not replay every float scenario or duplicate the model suite.

VERDICT: REVISE

### Claude's response (rev 3 of PLAN.md)
ALL SIX ACCEPTED: #1 ArrivalSequence (append-only, buzz keys on it; Revision renders); #2 snapshot
processing order pinned (reset -> settle -> betting) + delayed-observation order test; #3 claim
rejects only Idle-entry by a non-owner, the owner's Back always allowed; #4 race-window tests
during focus-in AND focus-out; #5 OnDisable unwinds (stop glides, restore camera/cursor/controller)
before releasing the claim, with mid-glide disable test; #6 one narrow adapter test - RunDirector
gains a public StartNewRun(seed) overload, a pinned seed + scripted losing bet produce a real
deterministic float, asserted through the real BookieFeed adapter (round stamp + DebtAfter).

## Round 4 — Codex

The prior ownership, ordering, reset, and adapter-boundary findings are otherwise addressed. Four concrete gaps remain:

1. `PhoneScreen` must read `ArrivalSequence`, but `BookieFeed` only exposes `Messages`, `UnreadCount`, and `Revision`.
Fix: Expose `ArrivalSequence` as a read-only `BookieFeed` property and explicitly key both emission pulse and light blink on it.

2. The tests never assert the new counter’s defining semantics, so buzzing on read/reset could regress while all listed tests remain green.
Fix: Assert that each append increments `ArrivalSequence`, read/reset alone do not, and reset-plus-equal-count welcome increments it exactly once.

3. The adapter integration’s “fast-forward” path conflicts with `RunDirector`: calling `Run.FastForwardRound()` moves the engine to `Settlement`, after which `RunDirector.FinishAndSettle()` returns early and never creates `LastSettle`.
Fix: Drain each `Run.Sweats` session with `MoveNext`, leave the phase at `Sweat`, then call `RunDirector.FinishAndSettle()` once.

4. Making the formerly private `StartNewRun(string)` public exposes an unvalidated path for null, whitespace, and untrimmed seeds; its existing callers previously sanitized input before entry.
Fix: Normalize inside the public method with the same blank-seed generation and trimming rules, or make the deterministic overload internal to tests.

VERDICT: REVISE

### Claude's response (rev 4 of PLAN.md)
ALL FOUR ACCEPTED: #1 BookieFeed exposes ArrivalSequence; pulse AND blink key on it, render on
Revision; #2 ArrivalSequence semantics test added (append increments; read/reset alone do not;
reset+equal-count welcome exactly once); #3 adapter flow corrected - drain Run.Sweats sessions via
MoveNext leaving Phase==Sweat, then one director.FinishAndSettle() (FastForwardRound would finish
internally and starve LastSettle); #4 public StartNewRun(string) normalizes like first-run entry
(trim; blank => fresh random seed).

## Round 5 — Codex

No material flaws remain. Rev 4 aligns with the supplied engine transitions, closes the trigger deduplication and ordering gaps, and tests both the pure model and critical Unity integration boundaries.

VERDICT: APPROVED

### Resolution
APPROVED on round 5/5 (round 1 was an environment block, not a plan finding). Rev 4 is final.
Act 1 locked intent with Allen (4 decisions); Act 2 produced 25 findings across rounds 2-4:
22 accepted, 1 redirected to existing API (Run.Rng.RunSeed), 1.5 rejected with logged reasons
(FLOAT keeps its dollar amount as bookie voice; PlayMode does not re-drive full engine floats).
Awaiting Allen's sign-off to implement.
