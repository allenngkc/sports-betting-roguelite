# Studio Status — 2026-07-31 (orchestrator sweep)

- **main:** `fdb8db2`. Untracked `docs/design/` (Design Director register + charter) —
  recommend committing it so the register survives; see Need Allen.
- **surething-ui:** `cb83c90` (annotated form-guide lobby shell). Substantial uncommitted
  Unity work in tree. **Flag:** modified `ProjectSettings/*` (EditorBuildSettings,
  ProjectSettings, ShaderGraphSettings) and URP global settings — integration-only files
  under STUDIO.md ownership rules; lead must justify or revert before merge. Stray test
  XML/log files at `unity/SBR/` root need cleanup; `handoff.md` still untracked.
  Lead: Claude (Opus 5).
- **room-refinement:** `5329c0f` (Phase B evidence captures). Tree clean except untracked
  `handoff.md` (lead should commit it). R5 (PBR maps) and R6 (Adaptive Probe Volumes)
  implemented, design review pending. Lead: Claude (Opus 5).
- **tv-sweat:** `220c5ec` (Phase 2E-3 chance shapes + goal reactions). Tree clean except
  `.impeccable/`. **Flag:** no `handoff.md` — the per-worktree ownership contract
  STUDIO.md requires is missing; lead to author one next session. Phase 3 (T7) gated on
  the C1 ruling. Lead: Claude (Opus 5).
- **feat/soccer-markets (Documents checkout):** Dormant, clean — F_0.4.0 awaiting
  playtest.
- **Design Director:** Claude Design seat (moved 2026-07-31). Register:
  `docs/design/REGISTER.md`. Nothing Design-verified yet; implemented-but-unreviewed
  backlog: S6 (lobby shell), R5/R6 (room refinements), T6 (TV scene grammar 2A–2E).
- **Orchestrator:** Fable 5 session in `main-2`; takeover procedure in
  `ORCHESTRATOR.md` §3.
- **Blocked:** TV Phase 3 gated on C1. C3 (TV canvas cannot carry HDR) blocks room
  fidelity work on the TV wall.
- **Need Allen:**
  1. Rulings on the three recorded design conflicts (`docs/design/REGISTER.md`):
     C1 TV "Decision A" layout status; C2 TV light-spill colour; T8 scanlines/static
     shipped against the approved design.
  2. surething-ui `ProjectSettings/*` + URP global settings changes — approve as
     deliberate, or direct the lead to revert them.
  3. OK to commit `docs/design/` on `main`.
