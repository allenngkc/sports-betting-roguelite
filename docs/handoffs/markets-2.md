# Markets follow-up — lead ownership contract

**Created:** 2026-07-31 by the orchestrator, authorized by Allen
**Owner:** Claude (Opus 5) acting as markets/sim content lead
**Worktree:** `C:\Users\Allen\orca\workspaces\sports-betting-roguelite\markets-2`
**Branch:** `markets-2`
**Last updated:** 2026-08-07 (G6 sample-size dial ruled by Allen, built and verified — see §7a) by the lead
**Mission:** carry the F_0.4.0 soccer-market expansion to near-final against the
redesigned product.

## 1. Studio context (read these, in order)

1. `main-2/docs/5-orchestration/STUDIO.md` — roles, ownership rules, Unity rules.
2. `main-2/docs/5-orchestration/STATUS.md` — live board.
3. `main-2/docs/design/REGISTER.md` — every design ruling.
4. `main-2/docs/design/design-system/` — **spec-of-record** (tokens, components, laws).

Studio policy: autonomous loop. Evidence meets exit criteria → the orchestrator
advances you. Stops-for-Allen: design direction, scope/licensing/money,
failed-checklist merges, irreversibles. Report telegraphic, result-first, ending
`Done / Next / Risk / Need Allen`.

## 2. Scope — current

**Own:** market math, odds generation, market-type definitions, settlement logic,
sim support for markets, market content/data, and — since the widening below — the
market-owned **presentation** on the LEDGER surface. Tests for all of it.

**Scope widened 2026-07-31** (Allen via orchestrator) to the §4 presentation
reconciliation. The original "forbidden until `surething-ui` merges" gate is **spent**:
`surething-ui` merged at `2e97d13` and `room-refinement` at `bb457af`. The branch was
cut at `65a30d1`, 58 behind, and was fast-forwarded to main so the audit described
live code — if you re-seat and find yourself behind again, sync before auditing
anything.

**Still not yours:** `ProjectSettings/**` and package manifests (integration-only,
orchestrator + Allen). Scenes/prefabs owned elsewhere. Anything match-theater (`tv-sweat`).

## 3. State — what is done

Deliverable #1, the reconciliation gap-list, is committed and is the living document
for this seat: **`docs/1-plans/F_0.4.0_reconciliation-gap-list.md`**. Read it before
anything else; it carries every finding, correction and open question with evidence.

**B1 (the working margin on the redesigned surface) is MERGED to main** at `bbf9241` and
validated green. What follows it on this branch is the second wave.

| Commit | What |
|---|---|
| `ebeac09` | gap-list (deliverable #1) |
| `cc40e8a` | M-01 — unique player names across the whole scorer board |
| `bf8a03e` | M-03 arm A — G7 market-coverage gate |
| `82011e1` | M-02 — scorer grading trap closed; F_0.4.0 doc debt cleared |
| `9e55d0d` | engine retention — cash-out figure + the run-long settled record (S36) |
| `bbf9241` | **B1 merged to main** (margin, MaxLegs=4, T47 band, S50 layout, S51 deviation) |
| `bf2a4da` | merge main → markets-2, three conflicts resolved keeping both intents |
| `aa68ec6` | T61 — the poll-vs-session divergence is outcome-dependent; capture renumber 09→11 |
| `d1dd2c3` | scorer EV harness (`--scorer-ev`) — calibration, the instrument no gate can be |
| `71ac4aa` | **arm B** — market ties broken by sampling, not array position |
| `e215b13` | M1 measured BTTS exclusion; C32 gate resolutions |

**Verified baselines** (re-establish these before trusting any change):
`dotnet test engine.tests` → **183/183** (181 before the two role-order tests added 08-07) ·
Unity EditMode **75/75** · PlayMode **47/47** · Unity compile 0 errors — **the Unity figures on
this line carry §7's correction: they are pre-`7885f8e` and are not evidence for HEAD.**
`dotnet run --project sim -c Release -- --gates --seed-prefix TUNE` → **ALL GATES PASS, exit 0**.
**The `--runs 1000` that used to be on this line is gone deliberately**: the campaign now carries its
own ruled floor (10,000 — Allen 2026-08-07, §7a), so a bare `--gates` is the campaign and an
explicit `--runs` is how you escalate. G7 went green when M1 narrowed its population to what it can
honestly assert. It was red by design for the whole of the first wave; if you find it red
again, something regressed, it is not the old known state.
`dotnet run --project sim -c Release -- --scorer-ev --runs 400 --seed-prefix SCORER` →
calibrated, all bands within 2 SE.

## 4. Open, parked, and routed — do not restart these blind

- **Arm B — ACCEPTED by Allen 2026-08-06 and DONE.** It was never a policy exclusion:
  `IncludesMarketOffers` was already true, and the zero coverage was an EV tie broken by
  **array position** — `BuildOffers` emits moneyline → goals → BTTS → corners → cards, so
  goals won every tie and corners/cards could never be chosen at any seed. Ties between
  non-moneyline candidates now resolve by reservoir sampling on the bot's own rng; the
  moneyline persona is untouched. EV-neutral, confirmed not assumed: 5.5% → 5.4% at
  n=10,000 (0.44 SE). Tables at `main-2/docs/design/dd-import/markets-armB-gate-tables.md`.
- **Scorer EV harness — DONE** (`--scorer-ev`). Calibration is the one instrument a gate
  cannot be, because bots are policy-excluded from pricing the market. Verdict: calibrated.
  It surfaced one apparent finding — the 0–5% band landing realised EV at −3.6pp against −4.76pp,
  read as **longshot scorers being about a point cheap**. **RETRACTED, see §7:** that gap is
  1.06 SE once the EV column is judged against its own error instead of the frequency's. Closed,
  not deferred, and not Allen's call after all.
- **G6 cannot fail — RULED, BUILT, CLOSED 2026-08-07.** It read **±2.15pp against a 2pp band
  (0.9×)**: tolerance narrower than its own noise, passed all session, could not have caught
  anything. Allen took option 1 — raise `n` — and the gate now measures **±0.97pp with 1.3pp of
  clearance**, adjudicated. Full account in §7a, including the part of the old diagnosis that was
  wrong.
- **D-01…D-08 are with the Design Director.** Market label vocabulary, the `RIDING`
  state word, the one-sided scorer market, the PLAYERS-tab overflow treatment, the
  `STAKE` label/figure split, ledger legs excluded from the lost treatment, blocker
  reasons lacking a remedy. **Build around them; do not pre-empt.** §4's conformance
  pass deliberately touched none of them.
- **Mid-sweat capture window — QUEUED** behind TV's T25.1 re-capture. Needed to close
  the one visually-unverified change: the live-leg toner colour (compiled, test-green,
  but no captured state exercises a `Live` leg).
- **Genuinely outstanding docs**, itemised in the plan's to-dos: `design/12` was never
  created, `DECISIONS.md` has no F_0.4.0 freeze entry, `ARCHI.md` covers Phases 1–3
  only, `PLAYTESTS.md` has zero F_0.4.0 entries.

## 5. Rules you inherit

- Delegation is the operating mode: bounded Sonnet sub-agents, at most two at once;
  every dispatch names allowed/forbidden files, evidence, and an exit gate; sub-agents
  never commit. **Review every diff yourself** — across five dispatches, two needed
  lead intervention, one of which was a real shipped-bug catch.
- One Unity editor studio-wide. Request a lease from the orchestrator and wait for the
  grant. Confirm process count + `Temp/UnityLockfile` at open; announce open and close.
- Evidence in `evidence/` (gitignored); bulk captures stay out of git.
- Commit only on this branch. No pushes, no merges, no history rewrites.
- Design questions route to the Design Director via the orchestrator.

## 6. Traps — all of these cost time here

- **`unity/SBR/Assets/Plugins/SBR/SBR.Engine.dll` is how the engine reaches Unity —
  DO NOT revert it.** Unity does not compile `engine/**`; it binds to this prebuilt
  binary, which `SBR.Engine.csproj:19-26` copies on every build *"so the Plugins DLL can
  never go stale"*. **When engine source changes, rebuild and COMMIT the DLL.** Build
  **Debug** (the default) — that is the committed convention; a Release build produces
  different, smaller bytes and would look like drift.
  *An earlier version of this file said the opposite — "`git checkout --` it before every
  commit". That was wrong and cost a full lease window: reverting it made Unity compile
  against a stale engine, so `MatchModel.Fields` did not exist and batch 4 failed to
  build. It also means any Unity run done after such a revert verified the OLD engine.*
- **Unity's exit code is not a completion signal.** A `-batchmode -quit` launch
  reported exit 0 while the editor kept importing for another ~16 minutes. Judge
  liveness by CPU time, log mtime/size and `Library` file count — not the exit code,
  and not the absence of output.
- **Serialize anything that builds.** Two agents running `dotnet` in one worktree race
  on `engine/obj` and the shared plugin DLL.
- **Look inside a directory before deleting it.** `artifacts/` holds 55 *tracked*
  room-visual-pass files alongside untracked capture output; `git check-ignore` says
  nothing about that. (Recovered byte-identical via `git checkout --`.)
- **`artifacts/` is not gitignored** while the SureThing capture tests write into
  `artifacts/surething-ui/`. One `git add -A` after a PlayMode run commits megabytes of
  PNGs. A fix must be targeted — `artifacts/room-visual-pass/**` is legitimately tracked.
- Unity segfaults on `-quit` sometimes (exit path only) — check state at open.
- Unity asmdef code is **invisible** to `dotnet build`. A green engine suite says
  nothing about whether the Unity project compiles.
- `--nologo` is not a valid `sim` flag.
- `--scorer-ev` does **not** honour `--report`; it writes to stdout. Pipe it.

---

## 7. State at 2026-08-07 — read this first on re-seat

**Merged and certified live on main:** B1 (`bbf9241`) and arm B. Both validated green.

**On this branch — ACCEPTED by Allen 2026-08-07, cleared for merge:**

| Commit | What |
|---|---|
| `7885f8e` | per-player scoring weight — six prices on a scorer board became fourteen |
| `166a4b0` | the EV column needed its own error; "longshot cheap" retracted |

Tables and reads are staged in `main-2/docs/design/dd-import/`:
`markets-armB-gate-tables.md`, `markets-pricing-variety-tables.md`,
`markets-captured-string-flake.md` (carries its own correction).

### Verified baselines — and which half of them is not HEAD evidence

**Re-verified on 2026-08-07 against the tree being merged:**
`dotnet test engine.tests` → **183 executed, 183 passed, 0 failed, 0 skipped**, exit 0. The count is
stated because a suite that does not state its count states nothing (C29); it reads 183 and not 181
because this run added two tests, below ·
`--gates --runs 1000 --seed-prefix TUNE` → **ALL GATES PASS, exit 0**, all seven verdicts and every
figure identical to the accepted tables — *this is the n=1,000 invocation and it is a record of what
was run, not the current command; §7a supersedes it and explains what its resolution could not see* ·
`--scorer-ev --runs 400 --seed-prefix SCORER` → all five bands within 2 SE, FW/MF/DF within 0.1pp,
worst band 20–35% at 1.2 SE. Calibration **and** EV fairness both hold.

**The Unity numbers previously on this line were never HEAD evidence. This is the correction.**
EditMode 75/75 and PlayMode 47/47 were measured 21:11–21:28 on 08-06. The engine change committed at
**23:03**. Those runs predate not only the change but the *before* arm of its own measurement
(22:53) — they exercised a build with no jitter dial in it. Unity does not compile `engine/**`; it
binds the prebuilt DLL, and that DLL changed in `7885f8e`. **A green engine suite is not Unity
evidence.** A lease run is owed before any Unity number here is called verified.

That PlayMode "47/47" was also the **third attempt**, and must not be laundered into a clean number:

- `mr` and `mr2` failed the same assertion — **not a flake.** It was the stale hardcoded 280px
  receipt width, already diagnosed and fixed at `2db5c19` (21:29:39), which **is** an ancestor of
  HEAD. Both failures carry one signature: expected exactly two characters shorter, diverging at
  index 36 where the ellipsis sits. One deterministic bug meeting two different random strings.
- `mr3` went green at ~21:28, **before that fix was committed at 21:29:39** — so it ran on
  uncommitted working-tree source. All three logs record compilation activity, which is *consistent*
  with the source changing between runs but does not on its own establish it; the timestamps do, and
  they are enough. (Softened deliberately — the original wording here claimed the logs proved the
  three runs were different builds, which a bare count of compile lines does not show.)
- Both things are true at once: the defect is genuinely closed at HEAD, **and** no PlayMode run
  against any committed tree exists. The second is why the lease is owed, not the first.

### Corrected during the merge run (2026-08-07)

- **"Role order survives by construction" was wider than its arithmetic.** The claim went to Allen
  in the tables demonstrated on the forward-vs-defender gap alone (forward floors at 3.0 × 0.65 =
  1.95, defender ceilings at 0.5 × 1.35 = 0.675). The adjacent pair was never checked, and it
  overlaps: the **midfielder ceiling is 1.5 × 1.35 = 2.025, above the forward floor of 1.95**, so a
  jittered midfielder can out-price a jittered forward. **Measured: 19 of 3600 teams, 0.53%.**
  Forward-over-defender and midfielder-over-defender do hold at every seed, and those are now
  asserted on real rosters rather than inferred from the arithmetic that generated them
  (`Role_weight_bands_are_disjoint_for_forward_over_defender_only`,
  `Every_generated_roster_keeps_forwards_and_midfielders_above_defenders`). The inversion rate is
  **reported and asserted on nothing** — a threshold there would turn a future narrower jitter, which
  would be an improvement, into a red suite. The behaviour looks defensible; an attacking midfielder
  pricing above a fourth-choice forward is not obviously wrong. The *claim* was the defect, and it is
  the seventh instance this fortnight of a number quoted past what produced it.
- **The calibration harness described a model it no longer had.** Its by-role section printed
  "scoring weight … is assigned purely by role" — true before the jitter, false the moment it landed,
  and it would have printed that sentence under every future run. Now states what the split reads and
  what it cannot see: a miss confined to one player inside a role pools away there (C25).

## 7a. G6 — RULED by Allen, built and verified 2026-08-07

**Allen's ruling (option 1, via the orchestrator):** raise `n` to ~4,600 for ±1.0pp resolution,
inside the 2pp band, so the gate becomes able to fail. **Escalation path recorded: any near-line
result re-runs at ~18,500.**

### Built

- **The campaign's `n` is now ruled, not chosen.** `--gates` carries a ruled floor of
  `GateData.CampaignRuns = 10000` (Allen's second call, after the escalation — see correction 1);
  an explicit `--runs` still wins, which is how the escalation is invoked, and `--gates` *below* the
  floor now warns on stderr naming the ruling. The documented invocation is therefore
  `--gates --seed-prefix TUNE` with **no `--runs`**. G6 resolves **±0.65pp measured** at the floor.
- **G6's C32 cell became three tiers**, and the two thresholds land exactly on Allen's two rungs —
  which is why the rungs are worth keeping in that order rather than rounding them:
  **≥4×** resolves the whole band · **≥2×** can fail, but not for a reading nearer the line than its
  own resolution · **<2×** cannot reliably fail (what G6 was). The 10,000 floor buys tier 2 (3.1×)
  and 18,500 buys tier 1 (4.1×) — so the two-rung structure survives the floor change intact: the
  floor makes G6 able to FAIL, the escalation makes it able to ADJUDICATE a reading near its line.
- **Near-line detection added, and it is the load-bearing half.** A reading whose criterion edge
  falls *inside its own 95% interval* cannot reject "the true value is exactly on the line", so it
  decided nothing whichever way it fell. Those gates print **NOT ADJUDICATED** with the escalation
  command in their own cell, are named in the campaign's count line, and **drop the report's
  "ALL GATES PASS" banner**. The **exit code is deliberately unchanged** — Allen ruled a re-run, not
  a failure — so a green 0 keeps meaning "no gate failed" and never stands in for a verdict nobody
  reached. Flip it if you want the campaign to hard-stop; it is one condition in `Program.Run`.
- **The report asserts its seed (C34, batch 14).** The campaign was always pinned by construction —
  run i takes engine seed `"{prefix}-{i}"` — but the report never recorded *which* prefix, so every
  gate table's pinning lived in the prose wrapped around the artifact rather than in it. `--scorer-ev`
  printed its prefix; the campaign the gates are read off did not. Header and `--grid` now both
  state it. **Note for the next seat: batch 14 landed on 2026-08-07 and this seat was briefed
  "canon through batch 13" — check the register's transcription log before citing it, the canon
  moved mid-session.**

### Verified — bare `--gates`, TUNE seeds, exit 0, 7/7 PASS, run TWICE

`Runs per batch 4,600 · total 699,200` · `dotnet test engine.tests` → **183 executed, 183 passed,
0 failed, 0 skipped** · `--verify` determinism OK.

Run twice deliberately: the first campaign predated two later edits (a guarded stderr warning and
two comments), and this seat's own §7 correction is about exactly that — a green run claimed as
evidence for a tree it never executed. **The second run reproduces every gate figure identically.**
Only wall time differs, and that difference is the finding below.

Stated precisely rather than rounded up, because rounding it up is the habit being corrected: the
committed tree differs from the second run's tree **only in XML doc comments** — the corrections in
this section, which the compiler discards. Behaviour is identical by language guarantee, not by
judgement. What did re-run against the exact committed bytes: `dotnet build` (0 errors, 0 warnings),
`--verify`, and `engine.tests` 183/183.

**G6 is fixed and clean: ±0.97pp against the 2pp band (2.1×), margin +0.7pp — 1.3pp of clearance,
adjudicated.** Allen's predicted ±1.0pp came in at ±0.97pp measured.

### Three things the old write-up in this file got wrong

1. **"Raise the campaign's *default* `n`" mis-named the defect.** The tool's default was never
   1,000 — a bare `--gates` at `36122d6` ran **10,000**, which resolves G6 to ±0.65pp (measured).
   The ±2.15pp came from this seat invoking `--runs 1000` **by hand, all session**, and no code path
   objected. **CLOSED — Allen ruled the floor at 10,000 the same day**, once the escalation had
   settled. That is not a bigger guess than 4,600; it is the number a bare `--gates` always had.
   What changed is its status: an unremarked default anyone could undercut in silence became a
   ruled floor that says so when undercut. The value never needed fixing — the silence did.
2. **The G6 margin itself was mostly noise.** +1.5pp at n=1,000 became **+0.7pp at n=4,600** — a
   0.8pp move, inside the old ±2.15pp, exactly as the resolution warned. Anything read off that
   n=1,000 campaign inherits that error; the "0.5pp of clearance" this seat would have reported
   yesterday was never a measurement.
3. **A scaling claim I made and then falsified within the hour.** I wrote "cost does not scale
   linearly — 4.6× the runs cost 6.6× the wall time (121 s → 801 s)". The second 4,600 run, on
   **identical work**, came in at **625.78 s** — a 28% spread, putting the same ratio at 5.2×. The
   wall clock on this machine cannot resolve a 1.4× effect, so there was never a scaling finding
   there, only an unreplicated measurement. This is the ninth instance this fortnight and the
   fastest turnaround yet between stating a number and it being wrong — the single-measurement habit
   is the defect, not any one of the numbers it produces. **And the replacement I offered here — a
   ~42–54 min range for the escalation — was itself wrong within the day (actual 58.6 min).** Three
   attempts, three misses. Measured campaign wall times, and nothing else: **10.4 min and 13.3 min
   at n=4,600; 58.6 min at n=18,500.**

### G3 — near-line at 4,600, settled at 18,500. CLOSED.

The ruling fixed G6 and **surfaced the same defect one gate over**. G3 reads skilled **5.4% against
a 5.0% floor — 0.43pp of clearance on a ±0.67pp instrument.** Its *band* is fine (3pp = 4.5×
resolution, it resolves its whole band); it is this *reading* that sits on the line, which is
precisely the distinction the near-line half was built to catch. The campaign banner said
**7/7 PASS but G3 DID NOT ADJUDICATE** — the near-line half earning its place on the day it landed,
on a gate nobody had asked about.

### RESOLVED — the escalation ran. Every gate adjudicates.

Allen fired the escalation 2026-08-07. `--gates --runs 18500 --seed-prefix TUNE`, **2,812,000 total
runs, 3514.63 s, exit 0**:

| Gate | Reading | Resolution | Verdict |
|---|---|---|---|
| **G3** | won **5.5%**, 0.5pp above the 5.0 floor | **±0.33pp**, band 3pp = **9.0×** | **PASS — adjudicated**, resolves its whole band |
| **G6** | martyr-worst **6.0%** vs skilled 5.5%, margin **+0.5pp**, 1.5pp clearance | **±0.48pp**, band 2pp = **4.1×** | **PASS — adjudicated**, resolves its whole band |

`Gates evaluated: 7 · passed: 7 · produced a verdict: 7` — **ALL 7 GATES PASS, the economy holds.**
No unadjudicated gate remains. **G6 has gone from an instrument that could not fail to one that
resolves its entire band**, which is what the ruling was for.

**Predictions scored rather than quietly re-fitted** (they were written down before the run):
G3 → ±0.33pp predicted, **±0.33pp measured**; G6 → ~4.1× predicted, **4.1× measured**. Both hit.
G3's clearance came in at 0.5pp against the 0.43pp seen at 4,600, so it adjudicates with more room
than predicted, not less.

**The martyr margin converged as n rose: +1.5pp → +0.7pp → +0.5pp** (n = 1,000 → 4,600 → 18,500).
The n=1,000 figure was **roughly three times** the settled one — and "roughly" is load-bearing:
both figures are printed to 1dp, so the ratio is only pinned to about 2.6–3.4×, and a sharper number
than that is not available from the report either wrote. Loss-farming is not close to winning.

**Two caveats, both on this seat:**

- **Cost missed again, third time.** Predicted ~42–54 min, **actual 58.6 min** — 4.39–5.62× the two
  4,600 runs for a 4.02× increase in work. Wall-clock scaling here is not predictable from the
  handful of points this seat has, and the fix is not a better formula: **quote measured wall times
  only, and stop deriving ranges from two samples.**
- **The escalation report itself does not carry its seed line** (C34) — it was produced by the
  binary from before the header fix below. The run *was* pinned (`TUNE`, passed explicitly and
  recorded here), but the artifact does not assert it. It is the last campaign artifact with that
  gap.

### The floor, measured — and it trips its own escalation on a routine run

Allen ruled the floor at 10,000 (2026-08-07). Verified at the floor, 2026-08-08, bare `--gates
--seed-prefix TUNE`: **1,520,000 total runs, 1534.89 s (25.6 min), exit 0, 7/7 PASS.** First campaign
artifact to carry its own seed line — C34 satisfied in the file rather than in the prose beside it.

| Gate | Reading | Resolution | Band ÷ res | Verdict |
|---|---|---|---|---|
| **G6** | martyr-worst 5.8% vs skilled 5.4%, margin **+0.4pp**, 1.6pp clearance | **±0.65pp** | **3.1×** | **PASS — adjudicated** |
| **G3** | won 5.4%, **0.43pp** above the 5.0 floor | **±0.45pp** | 6.6× | PASS — **NOT ADJUDICATED** |

**`7/7 GATES PASS, but G3 DID NOT ADJUDICATE`.** This is the floor's real operating characteristic
and it needs stating plainly: **a routine campaign at the ruled floor demands a 58.6-minute
escalation.** G3's clearance (0.43pp) falls just inside its resolution there (0.45pp) — short by
0.02pp. The floor is roughly 1,000–3,000 runs below what G3 needs; resolution ≤0.43pp wants
n ≳ 11,000, and the clearance itself wanders 0.4–0.5pp between campaigns, so the requirement is not
a fixed number either.

**Do not read this as an argument for a higher floor.** Two campaigns then agreed that G3's reading
sits 0.4–0.5pp above a band edge, and raising `n` to chase a gap that small is a treadmill — each
step costs more wall time to adjudicate a gate whose *band* has been fine throughout (6.6×, 9.0×).
The question this keeps returning is **G3's band, or where the economy sits inside it**, and that is
Allen's, not a dial this seat should turn. **He took it the next day — §7b. This section is the
state that produced the escalation, kept because it is the argument the ruling answered.**

Also worth noting against my own prediction: I told Allen G6 would resolve **±0.68pp / 2.9×** at the
floor, arithmetic from 2.15/√10. **Measured ±0.65pp / 3.1×** — the scaled figure was stale because
the martyr-worst rate itself fell (6.9% → 5.8%) and a combined error tracks its inputs. Corrected
everywhere it was quoted, including in code. Tenth instance.

## 7b. G3 re-banded — RULED, BUILT, VERIFIED. The lane's last instrument defect closes.

**Allen, 2026-08-08: G3's floor moves 5% → 4.5%. Band is 4.5–8%.** He took the recommendation to
move the line rather than the sample size, after three campaigns established that no `n` resolves a
0.4pp gap whose own value wanders 0.4–0.5pp between runs.

**Built — the band and its C32 line as one fact.** They were two literals: `5.0`/`8.0` in the
criterion and a bare `3.0` handed to `BandVerdict`. That is one re-band away from a gate quoting a
width it no longer has — the §3.5 "a bound is not a layout" shape, in arithmetic. The width is now
derived (`ceiling - floor`), so the resolution line cannot drift from the band it describes. The
old band and the reason live in the gate's own description per the standing form, with the
2026-07-15 band kept beneath rather than overwritten.

**Verified — counted run at the ruled floor, `--gates --seed-prefix TUNE`, exit 0:**

`Runs per batch 10,000 · total 1,520,000 · wall 1212.81 s · seed-pinned TUNE` ·
**`Gates evaluated: 7 · passed: 7 · produced a verdict: 7`** · item flags none ·
`dotnet test engine.tests` → **183 executed, 183 passed, 0 failed, 0 skipped** · `--verify` OK.

> **ALL 7 GATES PASS — the economy holds.**

| Gate | Reading | Resolution | Band ÷ res | Verdict |
|---|---|---|---|---|
| **G3** | won 5.4%, **0.9pp** above the 4.5% floor | ±0.45pp | **7.7×** | **PASS — adjudicated**, resolves its whole band |
| **G6** | margin +0.4pp, 1.6pp clearance | ±0.65pp | 3.1× | **PASS — adjudicated** |

**Prediction scored, written down before the run:** clearance 0.43pp → ~0.9pp against ±0.45pp, twice
its resolution, G3 adjudicates without escalating and the banner returns to ALL 7 GATES PASS.
**All four hit.** No unadjudicated gate remains at the ruled campaign size, and a routine campaign
no longer demands a 58.6-minute escalation to be read as clean.

**One more datapoint against predicting this machine's wall clock:** identical work at n=10,000
measured 1534.89 s and 1212.81 s — a **27% spread**, replicating the 28% seen at 4,600 rather than
contradicting it. Measured costs now: 10.4 / 13.3 min at 4,600 · **20.2 / 25.6 min at 10,000** ·
58.6 min at 18,500.

**Routed and fixed, not this seat's:** `docs/1-plans/F_0.2.0_match-theater-sweat.plan.md:376` carried
the old 5–8% band as a re-hold criterion. The orchestrator routed it and tv-sweat committed the fix —
it now reads `Skilled band 4.5–8% (floor re-banded 5% → 4.5%, Allen 2026-08-08)`. **Verified present
on `slice/tv-sweat-refinement` and `origin/tv-sweat`; NOT yet on `main`, `main-2` or local
`tv-sweat`, which still read 5–8%.** Closed in tv-sweat's lane, one merge short of closed everywhere.
Recorded because main is where a future seat would read it, not because it is owed here.

## 7c. G5 — MEASURED FIRST, and the measurement inverted the case against it

**Allen, 2026-08-08: measure G5's error before setting any threshold.** The first of the three gates
in this family to get that order right; G6 and G3 were both set where their instruments could not
read them, and both cost campaigns to discover it.

**The number: ±0.06pp (2 SE, paired seeds). The reading is +0.1pp — 1.5× its own error, PASS,
adjudicated.** G5 is not the blind gate in the campaign. **At ±0.06pp it is the sharpest instrument
we have** — about 7× finer than G3's ±0.45pp and 11× finer than G6's ±0.65pp.

**Why it is that sharp, and why this seat guessed wrong.** The four arms (pair, soloA, soloB,
baseline) run on the SAME seed prefix, so run *i* is the same dealt hand in each and nearly all the
run-to-run noise cancels *inside* the combination. Differencing per seed is the honest instrument;
treating the four rates as independent gives roughly **±0.9pp**, at which +0.1pp would be invisible.

**The correction this section owes.** It previously said G5's "combined error at 18,500 is on the
order of ±0.6pp" — reasoned, never measured, and **wrong by more than a factor of ten in the
direction that mattered**. On that estimate G5 looked hopeless; measured, it passes cleanly. Every
threshold anyone would plausibly have picked in advance (0.5pp, 1pp) would have failed a gate whose
reading is genuinely and measurably positive. **This is the clearest case this fortnight for the
order Allen imposed**, and it is the mirror of the EV-column retraction: there the error quoted was
too small, here it was too large, and both came from reasoning about an instrument instead of
running it.

### What is left is a design question, not a measurement one

The statistical question is closed: the synergy is real and positive. What remains is **whether
+0.1pp is a large enough synergy to certify the composition pillar** — magnitude, not confidence,
and Allen's alone.

The retagged table gives that question its evidence. Every top-10 pair clears its own error; none is
noise:

| Pair | excess | ±2 SE | vs its own error |
|---|---|---|---|
| The Multiplier + House Key | **+2.96pp** | ±0.34 | 8.7× |
| Longshot Larry's Photo + House Key | +2.67pp | ±0.33 | 8.1× |
| The Multiplier + Whale Card | +2.17pp | ±0.29 | 7.4× |
| Longshot Larry's Photo + Whale Card | +2.04pp | ±0.29 | 7.0× |
| … | | | |
| **G5's pair — Multiplier + Scar Tissue** | **+0.1pp** | **±0.06** | **1.5×** |

**The composition pillar is real and strongly evidenced in this catalog — just not by the pair G5
checks.** Four pairs sit above +2pp at 7–8.7× their error; G5's exemplar is ~30× smaller than the
strongest and is the weakest real loop in the table. Whether the pillar should be certified on the
weakest measured loop is the question, and it arrives with its numbers rather than ahead of them.

### RULED and BUILT — exemplar moved, threshold set, family closed

**Allen, 2026-08-08: the exemplar moves to The Multiplier + House Key** — +2.96pp at 8.7× its own
error. The pillar certifies on real magnitude.

**Threshold `≥ 1.0pp`, set by this seat after the error was known** — and deliberately not invented:
1.0pp is the line the report's own taxonomy already draws between *marginal* and *superadditive*.
Against ±0.34pp it sits ~3× the error, so the gate can genuinely fail. Leaving `> 0` would have been
worse than before the move: a +2.96pp reading satisfies it trivially, and the day the exemplar
drifts it would certify the pillar on any positive noise. The criterion is **one-sided** — a floor
with no ceiling — so `BandVerdict` is still not called; a band ratio would invent a band this gate
does not have.

**Vacuous-gate guard, added unasked.** `combos.Find()` returns null on a typo or a catalog rename
and the old shape simply *skipped* G5 — the campaign would have reported six gates where seven were
intended, and passed. Twelfth instance of that shape in this lane. A missing exemplar now fails
loudly instead of absenting the gate.

**Verified — `--gates --seed-prefix TUNE` at the ruled floor, exit 0:** `1,520,000 runs · 1635.56 s ·
Gates evaluated: 7 · passed: 7 · produced a verdict: 7 · item flags none` — **ALL 7 GATES PASS**.
G5 reads **+3.0pp, ±0.34pp, clearing the 1pp floor by +1.96pp at 5.8× resolution.** Predicted
±0.34pp and ~5.8× before the run; both hit.

### The family, closed — all three now read what they claim

| Gate | Criterion | Resolution | Standing |
|---|---|---|---|
| **G3** | win 4.5–8% | ±0.45pp | band is **7.7×** — resolves its whole band |
| **G5** | synergy ≥ 1.0pp | ±0.34pp | clears by **5.8×** its resolution |
| **G6** | martyr ≤ skilled +2pp | ±0.65pp | band **3.1×** — fails on a breach ≥0.65pp past the line |

Every one of the three began this wave unable to fail for what it existed to catch: G6 at 0.9× its
own noise, G3 adjudicating nothing on a routine run, G5 with no resolution cell at all and a
threshold at zero. **None was fixed by the same move** — G6 took sample size, G3 took a band, G5
took an exemplar and a floor. The one thing common to all three was that **each was set before
anyone measured what its instrument could see**, and the order Allen imposed on the third is the
lesson the first two paid for.

**Also visible only now the error column exists:** rank by excess is not rank by reliability.
Multiplier + Longshot Photo (+0.76pp, ±0.45, **1.7×**) outranks Longshot + The System (+0.74pp,
±0.18, **4.1×**) on excess while being less than half as certain. The table ranked pairs on excess
alone for a fortnight, and G5 certifies a design pillar off one of its rows.

### CLOSED this wave, do not reopen

- **Longshot scorers "a point cheap" — RETRACTED.** It was 1.06 SE. The harness printed the
  *frequency's* error beside the *EV* number; at p≈4% the odds are ~24 and the EV column's error is
  ~24× larger. Every band is within 2 SE of −4.76pp. Pricing returns the intended vig.
- **BTTS coverage** — a false red (M1). A sharp declining an edgeless near-even market is the bot
  being correct. Excluded with a *measured* justification, expiring at v2 pricing.
- **The receipt "flake"** — never font-atlas state; a stale hardcoded 280px width left behind when
  E-07 moved receipts to the 700px sheet.

### Still open, lower priority

- The signed **2.6px** margin deviation (S51) — expires when its owner is identified.
- **Eight room-owned textures** show modified in every worktree since main activated the root
  `.gitattributes` LFS macro; they were committed as raw bytes where git now expects pointers. Not
  markets-owned, but it recurs on every sync.
- **LEDGER rebuild** — was blocked on a populated-ledger capture (S32/C17); main now carries
  `10-ledger-populated`, so **verify that before assuming it is still blocked**.
- **TMP migration** — sequenced laptop-first; no build work started.

### The pattern worth inheriting

Six times this fortnight a green check was measuring nothing, and **not one was caught by a test** —
captures and arithmetic caught all of them, twice in instruments this seat had just built. Three
were mine: the world-space epsilon, the ruled-paper ground counted as content, and the EV column
quoting the wrong error. State what an instrument cannot see, in the same breath as the number
(C25), and give every gate its resolution (C32). A number without its scope invites exactly the
conclusion it cannot support.

**Seventh and eighth, both from the G6 dial (2026-08-07), both mine.** The first: the new count line
said "1 NOT ADJUDICATED" while the Resolution column it pointed at named nothing — the tier check
returned early, so the weakest tier, the one where a reading is *most* likely to be sitting on its
own line, was the one place the near-line flag could not print. Caught by the first smoke run at
n=200, not by any test. The second is the older kind: this file called the defect "the campaign's
default `n`" when the default was 10,000 and the 1,000 was a hand-typed flag — **a diagnosis
inherited and repeated four times without once being checked against `CliOptions`.** Both say the
same thing: run the instrument before you describe it, and read the code before you name the cause.

## Allen ruling (2026-08-02, fired via orchestrator)

- **MaxLegs = 4.** The 6-leg overflow question is closed by construction: slips are
  capped at 4 legs, so the overflow state never renders. Supersedes the three costed
  overflow options in the lead's report. B1 proceeds under this cap.
