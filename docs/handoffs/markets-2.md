# Markets follow-up — lead ownership contract

**Created:** 2026-07-31 by the orchestrator, authorized by Allen
**Owner:** Claude (Opus 5) acting as markets/sim content lead
**Worktree:** `C:\Users\Allen\orca\workspaces\sports-betting-roguelite\markets-2`
**Branch:** `markets-2`
**Last updated:** 2026-08-07 (pricing variety accepted; merge-run verification) by the lead
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
`dotnet run --project sim -c Release -- --gates --runs 1000 --seed-prefix TUNE` →
**ALL GATES PASS, exit 0** — G7 went green when M1 narrowed its population to what it can
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
- **G6 cannot fail — the live one.** C32 made every gate state its resolution, and the
  martyr guard reads **±2.15pp against a 2pp band (0.9×)**: its tolerance is narrower than
  its own noise, it has passed all session, and it could not have caught anything. Worse
  than G3, the gate C32 was promoted from, because its error is the combined error of two
  measured rates. Fixing it means widening a balance band or raising the campaign's default
  `n` — both Allen's calls, and neither taken.
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
figure identical to the accepted tables ·
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

### OPEN — G6 cannot fail. The live defect.

The martyr guard — the check that loss-farming never becomes a winning strategy — has a band of
**2pp and a resolution of ±2.15pp**. Its tolerance is narrower than its own noise, so it would
report PASS straight through a real breach. It has passed all session and could not have caught
anything. It is worse than G3 (±1.43pp against 3pp), the gate C32 was promoted from, because its
error is the **combined** error of two measured rates rather than one.

It states this itself in every report, so it cannot go unnoticed again. **The fix is a dial —
widen the band or raise the campaign's default `--runs` — and both are Allen's, not a lead's.**

Read it as a standing caution on everything else in this file: *"the gates did not move"* is weaker
evidence than it looks while G6 is in this state, and any balance change landed before it is fixed
is landing past a blind guard.

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

## Allen ruling (2026-08-02, fired via orchestrator)

- **MaxLegs = 4.** The 6-leg overflow question is closed by construction: slips are
  capped at 4 legs, so the overflow state never renders. Supersedes the three costed
  overflow options in the lead's report. B1 proceeds under this cap.
