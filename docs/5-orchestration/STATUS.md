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
- **room-refinement:** handoff tracked (`49a7c55`); R7 wear plan committed and
  **Tier 1b committed with an honest record** — ceiling clean and verified; four
  decal defects fixed (stale-cache versioning, default shadow casting, URP BlendMode
  enum confusion — additive where multiply was meant, and alpha-blend vs multiply
  for stains). Mips-off hypothesis tested and disproved in-window. FluorescentSoot
  held back — unresolved contradiction, lead refused to guess a fifth time. Earlier
  coverage numbers withdrawn (unvalidated metric). **R7 parked per Allen
  (2026-07-31)** at the committed state; Decal Renderer Feature deferred to
  integration with DD input. **Design-review package committed** (`65b4122`,
  `[RM] docs/6-memo/2026-07-31-room-design-review-package.md`) — written for a
  reader with no repo access, framed as three design questions (pursue ceiling
  soot?; do decals justify the shared-renderer change?; is current state the
  target, or the concept?). Mid-cycle coverage correction recorded plainly (true
  figure 1.92% vs 1.69% baseline). Idle and holding; R8 waits on the review.
  Lead: Claude (Opus 5).
- **tv-sweat:** `842382d` — **T8 removed and verified** (engine 160/160, EditMode
  129/129, PlayMode 44/44 on rerun; single failure matched the documented cash-out
  flake signature in a path T8 doesn't touch). Contract/C1/T6/C3 at `4cdd98c`.
  **3C verified green, uncommitted** (warm compile clean, EditMode 193/193,
  PlayMode baseline-equal ex-spike; L4 guard held — the canvas rebuild widened
  nothing). Commit gated on the DD's Layout B ruling (inbox pending item 5).
  **Capture-harness spike succeeded both arms** — PlayMode capture survives domain
  reload; interactive GPU booking **stood down** (Allen, 2026-07-31); visual
  evidence is now repeatable and self-serve. Now: T15 remediation + markup-aware
  palette scan + seated-camera sweat capture build (no editor until verification).
  3D awaits the DD's C3 ruling. Lead: Claude (Opus 5).
- **feat/soccer-markets (Documents checkout):** Dormant — F_0.4.0 awaiting playtest.
- **Design Director:** **five rulings issued 2026-07-31**, transcribed to the
  register by the orchestrator — S11 closed (Archivo + Archivo Narrow, OFL), S14
  identity spec issued, S15 lost-ticket violation, S16 naming closed (LEDGER), T15
  slip-strip violation. **Seat lost its document mounts** — C3 (gates 3D), Layout B
  item (gates 3C commit), R9/R10, the art-authority gap, and the room review
  package all wait on Allen re-attaching files via Import. S14's reference
  implementation is held in the DD seat pending hand-over. Nothing Design-verified
  yet.
- **Orchestrator:** Fable 5 session in `main-2`; lead channel chartered in
  `ORCHESTRATOR.md` §3a. Completion watchers armed on all three lead terminals and
  the Design Director session.
- **Unity queue:** editor free (TV released clean). Next requests expected:
  SureThing capture window (wide-ring evidence read + S9 work), TV verification
  window (T15 + sweat capture) — first to announce takes it, orchestrator grants.
- **Blocked:** none.
- **Rulings (Allen, 2026-07-31):** C1 — latest document governs, `DESIGN.md` §6
  stands, layout closed. C2 — interim: shipped green tolerated, cold white-grey
  target lands with TV Phase 3. T8 — remove: done, verified `842382d`. S11 — no
  licence-encumbered typefaces in the product; Bell Centennial dropped.
- **Watch:** interactive GPU booking **stood down** (Allen, 2026-07-31) — the
  capture-harness spike succeeded; visual evidence is repeatable in batch. Only
  revisit if the seated-camera sweat capture fails.
- **Need Allen:** nothing.
