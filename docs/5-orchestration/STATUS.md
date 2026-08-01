# Studio Status — 2026-07-31 (evening)

- **main:** register + board current through the T8 ruling and C3 correction.
- **surething-ui:** 6 commits ahead of `cb83c90` — handoff contract (`5d1de82`),
  evidence cleanup + gitignore, ProjectSettings isolated commit (`63cf1bc`,
  Allen-approved), S7 ink pipeline complete (`1090527`), defect fixes (`4eb2cba`),
  red-on-dead-leg ruling (`8822971`). Tree clean. Next: S8 OS chrome, then S9.
  **S8 + S9 ink fixes landed and pixel-verified** — ink inversion (blue marks only
  his pick) and dead-strike sizing fixed; first-ever Ledger/Rewards/Old Slips
  captures, all eight states navigating. Wide ring: downscale hypothesis tested and
  **falsified** — generator untouched, diagnostic committed, fix from evidence in
  the next capture window rather than a third guess. DD rulings S11/S14/S15/S16
  dispatched into S9 planning. Lead: Claude (Opus 5).
- **room-refinement:** **R9 implemented and committed** (`9dce6f7` soot dropped +
  ambient rebalance; `13fedd1` gate-check harness for the R9/R10 acceptance
  re-runs) — awaiting its editor lease (third in queue) for build + bake + 8/8
  gate re-run. R10 planning next. R5/R6 Design-verified; R7 parked; owning doc
  approved (R13). Lead: Claude (Opus 5).
- **tv-sweat:** `842382d` — **T8 removed and verified** (engine 160/160, EditMode
  129/129, PlayMode 44/44 on rerun; single failure matched the documented cash-out
  flake signature in a path T8 doesn't touch). Contract/C1/T6/C3 at `4cdd98c`.
  **3C verified green, uncommitted** (warm compile clean, EditMode 193/193,
  PlayMode baseline-equal ex-spike; L4 guard held — the canvas rebuild widened
  nothing). Commit gated on the DD's Layout B ruling (inbox pending item 5).
  **Capture-harness spike succeeded both arms** — PlayMode capture survives domain
  reload; interactive GPU booking **stood down** (Allen, 2026-07-31). T15
  remediated; markup-aware palette scan written (and immediately caught the same
  class in `[ST] SportsbookApp.cs` — routed to SureThing). Sweat-capture harness
  built: `[Explicit]`-gated (protects the flake-prone suite), reuses room's exact
  seated camera through the live URP/HDR path, no hooks into gated files. **In the
  editor now verifying harness + scan.** 3D awaits the DD's C3 ruling.
  Lead: Claude (Opus 5).
- **feat/soccer-markets (Documents checkout):** Dormant — F_0.4.0 awaiting playtest.
- **Design Director:** batch 1 + batch 2 complete — **every routed item ruled**.
  Highlights: C3 ruled (3D unblocked, HDR set widened), T16 ruled (3C unblocked on
  tape restore), R5/R6 **Design-verified — the studio's first**, R9/R10 approved
  bounded, R12 standing law, two-tier art authority **approved by Allen** (C9; DD
  drafts the room's owning doc, phone stays a stub). Remaining on DD: TV typeface
  (not Archivo), review backlog (S6/S7/S8, T6), room owning doc. **Design system
  exported and landed** at `docs/design/design-system/` (canonical, on main):
  tokens, component library (incl. built `TvMomentumTape.jsx`), guideline-card
  laws, runnable UI kits for both surfaces, regenerated ink sprites, the
  art-authority proposal, and an adherence lint config. Leads reference it
  cross-worktree; do not fork copies.
- **Orchestrator:** Fable 5 session in `main-2`; lead channel chartered in
  `ORCHESTRATOR.md` §3a. Completion watchers armed on all three lead terminals and
  the Design Director session.
- **Unity queue:** surething-ui holds the slot (seven-change verification in
  dependency order + wide-ring dump read) → tv-sweat TVS-H02 regression
  investigation (freeze-contract regression; 3C held uncommitted until understood)
  → room R9 lease (build + bake + 8/8 gate re-run). Hold room's staged lease draft
  until granted.
- **Approved (Allen, 2026-07-31):** room owning doc — canonical at
  `docs/design/room-design.md` (R13). Room session context at ~507k tokens —
  recommend `/clear` at its next natural boundary; handoff.md + committed docs make
  it safe.
- **Blocked:** TV 3C merge blocked on the TVS-H02 verdict (its own hold, correct).
- **Integration plan (draft, for Allen when slices stabilise):** merge order
  1) `surething-ui` (most landed, all green, ProjectSettings changes approved and
  isolated), 2) `room-refinement` (after the R9 gate re-run passes), 3)
  `slice/tv-sweat-refinement` (after TVS-H02 is understood, 3C commits, and the
  T17 scorer-gap fix lands). Canonical Unity validation pass in `main-2` after
  each merge. Cross-tree conflict surface is small — each slice owns disjoint
  files; the shared risks are ProjectSettings (surething, approved) and
  `Room.unity` (room-owned; TV deliberately never touched it).
- **Rulings (Allen, 2026-07-31):** C1 — latest document governs, `DESIGN.md` §6
  stands, layout closed. C2 — interim: shipped green tolerated, cold white-grey
  target lands with TV Phase 3. T8 — remove: done, verified `842382d`. S11 — no
  licence-encumbered typefaces in the product; Bell Centennial dropped.
- **Watch:** Unity **segfaults on `-quit`** (exit path only; 0 errors, lockfile
  clears, nothing corrupted — observed on tv-sweat warm compile 2026-07-31). Every
  lease-holder must keep checking process count + lockfile at open. GPU booking
  remains stood down.
- **Need Allen:** nothing.

## Autonomous decisions (Allen veto window)

Autonomy authorized 2026-07-31 (STUDIO.md policy; ORCHESTRATOR.md §6). Every entry:
decision · evidence checked · reversal path.

- 2026-07-31 · Loop started. Policy dispatched to tv-sweat and surething-ui;
  room-refinement's copy rides its R9 lease grant (composer blocked by Allen's
  staged draft + survey prompt — not touched per composer rule). Reversal: Allen
  says stop.
- Heartbeat: watchers armed on TV (TVS-H02 investigation) and SureThing (next
  cycle) with Unity-process checks; 25-minute fallback heartbeat running.
- 2026-07-31 cycle 2 · **S11 verified closed** — spot-checked `ed07ee3` (fonts +
  OFL licences), `a3d8876` (rulings + document layer), `b820624` (markup guard),
  `7169c95` (policy in handoff); evidence matches report. SureThing's font-wiring
  slot queued third (TV investigation → room R9 → SureThing fonts). Room dispatch
  still blocked on its composer (Allen's staged draft + survey prompt). Reversal:
  reorder the queue or veto the S11 close.
- 2026-07-31 cycle 3 · **TVS-H02 verdict accepted** — pre-existing one-frame
  ordering quirk exposed (not introduced) by 3C's UPDATING state; fix landed
  uncommitted with a mechanism that predicts the observed rate; lead's own suspect
  disqualified on evidence. **Queue reordered:** TV verification slot before room
  R9 (room's channel is composer-blocked; editor must not idle). **TV session
  hygiene ordered before its slot** — 97% context; state written to handoff.md,
  then session clear, then re-seat and run. Reversal: veto the reorder; the fix
  diff is in TV's working tree, revertible.
- 2026-07-31 cycle 4 · TV state-write confirmed ("ready to be cleared", handoff
  +83 lines). Attempted remote composer clear for the `/clear` step: Esc, Ctrl+U,
  backspace all filtered by the send path; double-Esc opened the rewind menu
  (cancelled, no action taken). **Blocked on a human step** — Allen: Esc + `/clear`
  in the TV terminal. Loop resumes automatically on the fresh session. Standing
  note: staged composer drafts are the recurring channel blocker (TV now, room
  still) — under autonomy, decisions sent to the orchestrator instead keep lead
  channels clear.
- 2026-07-31 cycle 14 · **Allen away — full-auto confirmed** for surething-ui,
  tv-sweat, room-refinement; pings only for critical/DD items. **markets-2 spun up
  and PARKED per Allen** (worktree + branch from main `65a30d1`, handoff contract
  written, Opus lead seated but in manual permission mode — no dispatches until
  Allen returns). Dormant Documents checkout retired from the registry (fully
  merged, 56 behind). S17 ruled by Allen (offer rule-text never truncates — fewer
  offers instead). SureThing in verification slot; TV's T20 window queued behind
  it; T20 live-row deviation → DD inbox 9a. Reversal: unpark markets-2, reorder
  queue.
- 2026-07-31 cycle 13 · **T20 scope decision made autonomously** — TV surfaced a
  canon-vs-Layout-B conflict via option dialog; orchestrator selected its
  recommended option 1 (adopt the T20 ruled type scale within the no-reflow law,
  NEED one-line on Unity, deviation documented) because it amends no law and the
  later ruling governs over the canon reference (C1 precedent). Row-model question
  routed to DD as inbox item 9. Reversal: DD rules for expanding rows → option 2
  executes then. Also this cycle: T17 resolved (`ea28c9b`), scorer-gap closed.
- 2026-07-31 cycle 10 · **Phase 3C LANDED** (`4969eb1`: Layout B canvas +
  T16/C3/C8 + TVS-H02 fix; both n≥10 arms green, full suites green, stash
  round-trip byte-verified against a CRLF rewrite). Editor: room R10 cycle opened
  (Allen's staged lease fired at validity); TV implements T17 editor-free, window
  after SureThing's pixel check. Three automated-run traps recorded in TV's
  handoff §4. **Two Allen gates raised by TV:** LFS is inert repo-wide (macro in
  a non-root .gitattributes is ignored by git) and 49 T6 captures (28.8 MB) held
  out of commit pending the call. Reversal: revert `4969eb1`; reorder queue.
- 2026-07-31 cycle 9 · SureThing continue delivered: S9 audited and committed,
  build-out halted at its own unverified-stack line, and the **S6–S8 review bundle
  placed in the orchestrator tree and committed** (pre-typography caveat leads;
  includes withdrawn self-misreads; grain flagged as shader work with measurement;
  new violation logged for DD review — "LEAVE — NEXT ROUND" primary action drawn
  in saturated biro). Bundle ships to the DD after the Archivo capture refresh so
  type and structure review together. Room continue undeliverable — composer still
  holds Allen's staged R10 draft. Reversal: veto the bundle hold, ship it now.
- 2026-07-31 cycle 8 · **Stall caught and broken** — TV's stack arm died silently
  (~14:20: Temp mtime + upm.log at 14:17, zero Unity processes 34+ min, task list
  unmoved) while its driver-monitor waited on a log that would never grow —
  evidence contradicted the apparent "running" state. Woke the lead by firing
  Allen's staged report-request with the evidence appended; window remains TV's;
  lead decides rerun-vs-diagnose. R9 closed as measured no-op earlier this cycle
  (`b1d2ccc`); R10 prepped and queued. Reversal: none needed — no state changed.
- 2026-07-31 cycle 6 · **Loop resumed** — Allen's one-pass completed all three
  steps. Room in the editor (R9 gate re-run). TV re-seated post-clear on a fresh
  session: verified HEAD, stack, fix lines, and correctly held on seeing room's
  live run. SureThing parked third. Long-send truncation chartered
  (ORCHESTRATOR.md §3a); re-seat brief moved to the durable file channel. Allen's
  staged TV grant fires (with timing note appended) when room releases — logged
  here as the planned action; reversal: don't fire, hold TV.
- 2026-07-31 cycle 5 · **§6 stop tripped** — two consecutive cycles moved nothing:
  all three lead composers hold staged drafts (TV `clear the session`, SureThing
  `wire the fonts…`, room `Editor lease granted…`), every dispatch channel blocked,
  editor idle. Desktop notification sent with the one-pass fix (room: Enter ·
  SureThing: Esc · TV: Esc + `/clear`). Watchers stay armed; loop resumes on any
  composer change. No work lost; no state at risk.
