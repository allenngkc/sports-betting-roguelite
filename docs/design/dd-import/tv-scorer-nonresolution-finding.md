# Finding — the anytime-scorer leg does not reach a terminal state within the capture budget

**Filed by:** TV lead (`tv-sweat`), 2026-08-05, at the orchestrator's instruction.
**Routes to:** the markets lead's backlog (engine domain). **DD only if** the design question in §6 is
real.
**Form:** C25 — what was measured, and what the instrument cannot see, in the same breath.

---

## 1. The observation, in one line

In 4 of 5 capture seeds, the TV's revealed view **never shows leg 2 (the anytime-scorer leg) leave
`Live`/`Pending`**, while the director still reports a live session — across a 420 s ship-paced
budget, and again after the named moment was given a guaranteed 150 s of it.

## 2. What was measured

`TvSweatCaptureHarness.Capture_SeatedSweat_NamedMoments`, ship pacing, 2560×1440.

| run | seeds | result |
|---|---|---|
| T49 arm A (boost 1.8) | 5 | 4 failed, 1 passed (`27182818`) |
| T49 arm B (boost 1.4) | 5 | **same 4 failed, same 1 passed** |
| post-fix single seed (`48151623`) | 1 | failed |

Failing seeds: `48151623`, `42108675`, `30941771`, `16180339`. Assertion, verbatim:

> `the scorer leg never reached a terminal state — deadline reached with the session still LIVE,
> which is a genuine hang`

**The sweat is not frozen.** In the failing seeds the `goal` and `cashout-actionable` moments were
both reached and captured (8 frames each); 101 frames landed per arm across all five seeds. Whatever
this is, it is not the presentation stalling.

**It is not the capture harness starving its own wait.** That was the first hypothesis and it was
*partly* right: the failing seeds captured **zero** dangerous beats, so an opportunistic collector
loop consumed the whole shared budget and the scorer wait began with its deadline already gone. That
was fixed by partitioning the budget (150 s reserved). The instrumented re-run says:

> `dangerous-beat collector exited on budget with 0/3 captured; 150s left for the scorer wait (floor 150s).`

**The wait then got its full 150 s and the leg still did not reach a terminal state.** So the budget
was a real defect and it was not the whole story.

**It is not bloom-, boost-, or render-related.** Both A/B arms failed on the identical four seeds.

## 3. What the instrument reads

- Leg state: `screen.RevealedView.Tickets[0].Legs[2].State` — the TV's **presentation mirror**.
- Terminal := state is neither `Live` nor `Pending`.
- Liveness: `director.CurrentSession.IsComplete`.

## 4. What the instrument CANNOT see

1. **Engine state.** It reads the TV's revealed view, never the engine. This finding says *the TV
   never showed the leg settle*. It does **not** say the engine failed to settle it. Those are
   different claims and only the second is a sim defect.
2. **Whether the leg would ever resolve.** The budget is 420 s. Nothing here measures how long a full
   sweat needs at ship pacing, so the honest claim is **"not within this budget"**, never "never".
3. **Which session the liveness refers to.** The harness polls `Tickets[0]` but tests completion on
   `director.CurrentSession`. **These need not be the same object.** See §5 — this is the alternative
   I most want excluded before anyone treats it as a sim hang.
4. **Simulated vs wall-clock time.** The post-fix run had `Time.captureDeltaTime` active (the new
   frame-lock), so its 150 s of wall clock covers a different number of simulated seconds than the
   T49 runs did. The three runs are consistent in *outcome*, not in *simulated duration*.
5. **Why one seed passes.** `27182818` passed in both arms; it is also the only seed that reached
   `MaxDangerousBeats`. Whether that is cause, correlation, or coincidence is unmeasured.

## 5. Alternative explanations NOT excluded

Listed because each would make this a different bug — or none at all:

- **Round advance.** If the run advances a round and `RevealedView.Tickets` is replaced, then
  `Tickets[0]` becomes a *new* ticket whose leg 2 is legitimately `Live`, while `CurrentSession` is
  that new round's live session. The harness would then report a hang for a ticket that settled
  correctly and a leg that has barely started. **This is a harness-scope bug, not an engine one**, and
  it fits the evidence as well as a sim hang does. It is the first thing to test.
- **A ticket whose leg 2 is genuinely never played** — the ticket dies on an earlier leg and leg 2
  never runs. `WaitUntilOrAbsent` was written for exactly this and forgives it *only when the session
  completes*; if a later session is live, the forgiveness never fires.
- **Ship pacing simply exceeding 420 s** for these seeds.

## 6. The design question, if any (for the DD)

Only one, and it may be nothing: **if a scorer leg can stay unresolved while the surface continues,
what does the ticket column show for it, and for how long?** T17's reserve already makes the backed
side read one goal short until the final sequence (ruled intended, T33). If that window can be
long — or unbounded — the player may sit with a leg that never states an outcome. That is a
presentation question the TV owns and would act on. **If the cause turns out to be §5's round
advance, there is no design question at all** and this line should be struck.

## 7. Reproducing it

```
Unity.exe -batchmode -projectPath unity/SBR -runTests -testPlatform PlayMode \
          -testFilter ".*48151623.*" -testResults <xml> -logFile <log>
```

**Not `-nographics`** — the harness rasterises. **Use the regex form of the filter**: the quoted
parameterised name is what NUnit reports, and a bare `(48151623)` matches **zero tests** and exits
green with `testcasecount="0"` — a run that did nothing, reported as a pass. That cost one invocation
here and is worth knowing before it costs a window.

Harness: `unity/SBR/Assets/Tests/PlayMode/TvSweatCaptureHarness.cs` (TV-owned). The wait under test is
`WaitUntilOrAbsent` at the `ScorerLegIndex = 2` call site.
