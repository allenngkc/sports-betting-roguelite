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
