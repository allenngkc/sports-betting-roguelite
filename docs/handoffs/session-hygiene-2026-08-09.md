# Room lane — session hygiene handoff, 2026-08-09

**Written at seat rotation. Verifiable state only; everything unverified is flagged as such.**

---

## 1. State, verified this minute

| | |
|---|---|
| branch / HEAD | `room-refinement` @ **`b152741`** |
| vs `main` | **2 behind, 1 ahead** — main moved after my merge; converge with `git merge main` |
| working tree | **clean** |
| editor | free, Unity process count 0 |
| gates 6/7/8 | **certified** `2026-08-08` at `c975cc2` — Allen's walk, re-issued on his word |
| gate run | 10 PASS · 0 FAIL · 0 VOID · 5 SKIP · 1 INFO, coverage 10/16 |

**The emission era is merged to main** (`7cb5344`). Everything below `b152741` is on main already.

## 2. What is open

**Nothing is blocked and nothing is owed by this lane.** Two items sit elsewhere:

- **Phone owning document** — the reference set is with the DD
  (`main-2/docs/design/dd-import/phone-reference-set-2026-08-09/`). Two of its findings may amend the
  contract; if they do, **re-shoot the delta, not the set** (orchestrator's instruction).
- **Build-side ~1.6 px softness floor** — with the DD as a *measured characteristic*, not a defect.
  Allen's acceptance bar is already met (C38 closed). `_Sharpness` at max buys 9.6%, so if it is ever
  ruled to move, **it is not one constant**.

**Explicitly NOT owed:** the §6 two-surface baseline. Its gating characterization is done (§4 below);
the baseline pass itself was never scheduled.

## 3. Flagged unverified — do not inherit these as facts

- **3 messages is not the feed's ceiling.** The phone run stopped on *my step budget*, not the engine.
  The contract asked for "as many as the feed will produce". Anyone reasoning about stack composition
  from that set must know 3 is a floor on the maximum, not the maximum.
- **The 16-line / 60-char message pool is a source enumeration.** The 60-char line was observed live,
  so the ceiling is confirmed — but the *pool size* is read from `BookieScript.cs`, not exercised.
- **"Empty is unreachable" is verified for a pinned run start.** I did not test every path into the
  surface (e.g. post-`ResetForRun` edge cases).
- **The grade elimination is PARTIAL**, and this one has bitten before: `PC_RPAsset` carries a global
  default volume (`SampleSceneProfile` — Bloom, Vignette, Tonemapping active) that my bypass never
  touched. All are pre-resolve, so they fail the acceptance anyway, but the elimination is not clean.
- **`certified_at` is Allen's walk date, not the stamp date.** Gate 6 covers **the room, not the
  current laptop UI** — three SureThing kit commits landed after his walk. That limit is in the gate's
  own basis line; read it before quoting gate 6 at anyone.

## 4. Instruments, and what they are worth

- **`tools/glyph_ramp_ratio.py`** — ramp ÷ stroke, sub-pixel. **Characterized:** recovers known
  Gaussian kernels to within **~4%**; blurs add in **quadrature** with a floor of **1.680 px**, so a
  true added blur is `√(measured² − 1.680²)`. **Saturates near 0.60 above σ≈1** — a low-blur
  instrument only. A future reading of 0.60 means "badly blurred", not "60% worse".
- **EMIT gate** (`tools/room_gate_check.py`) — 4 judged / 0 unruled; the textured emitter is judged on
  its multiplier's neutrality, not on a value, so a saturated multiplier FAILS rather than passing as
  "textured". Tested in both directions.
- **`RoomViewCapture`** — nine capture entry points. Every A/B carries `control-a`/`control-b`, and
  Edit-Mode sets carry `control-z` as well.

## 5. The lesson worth carrying, named

**Two of my errors this session had the same shape, and neither was about the subject under test.**

- **The backbuffer arm.** I called it load-bearing, then ran `ScreenCapture` in `-batchmode` — the one
  mode where it cannot work. It returned null.
- **The phone driver.** I drove a whole run inside **one** `EditorApplication.update` callback, so
  `Update()` never ticked between the director's verbs and the feed never processed them. Three
  "states" came back holding the same single message.

**Both were assumptions about the HOST — batch mode, the frame loop — not about the room, the shader
or the engine.** Both produced results that looked like findings. The subject was fine each time.

**Practical rule for the next seat:** before believing a null or a flat result, ask *what does this
measurement assume about the environment it runs in* — not just about the thing it measures. The
guard that saved the second one was a loud warning on a missing arm; the guard that saved the first
was writing the prediction down before the frames existed.

**Related and already chartered:** never commit the Sentis/ShaderGraph settings churn — cmp-verify the
diff is only the known line, then `checkout`. I cleared it three times before it became a convention.

## 6. Canon born in this lane

**C36** (a control must bracket the interval it certifies, and be checked by the other half of the
instrument) and **C37** (a null whose success would fall below the instrument's own resolution proves
nothing) — constitution 4.4/4.5. **Both founding cases were my own failures**, which is the only
reason they are worth anything.
