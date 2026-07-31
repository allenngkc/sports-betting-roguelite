# Studio Status — 2026-07-31 (evening)

- **main:** register + board current through the T8 ruling and C3 correction.
- **surething-ui:** 6 commits ahead of `cb83c90` — handoff contract (`5d1de82`),
  evidence cleanup + gitignore, ProjectSettings isolated commit (`63cf1bc`,
  Allen-approved), S7 ink pipeline complete (`1090527`), defect fixes (`4eb2cba`),
  red-on-dead-leg ruling (`8822971`). Tree clean. Next: S8 OS chrome, then S9.
  **S8 landed** (`0b693f9`); S9 triage done via two bounded sub-agents — 2 of 4
  claimed ink defects real and fixed (ring-crescent geometry, strike overhang), 2
  disproved by the sub-agent sampling pixels instead of trusting the lead's
  description (wax amber misread as red; ring inside panel bounds). New capture
  states built for Ledger, Rewards, Old Slips (first-ever pixels). All unverified —
  slot requested, queued behind room's verification cycle. Two new DD questions
  routed (inbox items 7–8). Lead: Claude (Opus 5).
- **room-refinement:** handoff tracked (`49a7c55`); R7 wear plan committed and
  **Tier 1b committed with an honest record** — ceiling clean and verified; four
  decal defects fixed (stale-cache versioning, default shadow casting, URP BlendMode
  enum confusion — additive where multiply was meant, and alpha-blend vs multiply
  for stains). Mips-off hypothesis tested and disproved in-window. FluorescentSoot
  held back — unresolved contradiction, lead refused to guess a fifth time. Earlier
  coverage numbers withdrawn (unvalidated metric). **Need Allen: R7 steer** — keep
  iterating quads, escalate the URP Decal Renderer Feature (shared-renderer =
  integration decision), or park R7. Lead recommends against more quad iteration.
  Lead: Claude (Opus 5).
- **tv-sweat:** `842382d` — **T8 removed and verified** (engine 160/160, EditMode
  129/129, PlayMode 44/44 on rerun; single failure matched the documented cash-out
  flake signature in a path T8 doesn't touch). Contract/C1/T6/C3 at `4cdd98c`.
  **Phase 3A/3B landed all-green and committed** (engine 160/160, EditMode 188/188,
  PlayMode 44/44, no flake). Scorer-gap confirmed twice — structurally (the
  `Picked < _targetPicked` guard in `PlanFinal`) and by corrected reproduction; the
  first test was vacuous and would have reported "gap absent." Graphics experiment:
  batchmode without `-nographics` yields a real D3D12 device, PlayMode 9/9 on GPU —
  the open question is only a capture harness surviving domain reload. 3C (Layout B
  build) green-lit, no editor needed; one timeboxed slot approved for the
  capture-harness spike; interactive booking only if the spike fails. 3D awaits the
  DD's C3 ruling. Lead: Claude (Opus 5).
- **feat/soccer-markets (Documents checkout):** Dormant — F_0.4.0 awaiting playtest.
- **Design Director:** inbox memo at `docs/design/INBOX.md` — C3 coverage rule,
  studio art-authority gap (no binding authority for room/laptop/phone since `08`
  was deprecated 2026-07-24), SureThing form-guide identity, R9, R10. Review backlog:
  S6/S7, R5/R6, T6. Nothing Design-verified yet.
- **Orchestrator:** Fable 5 session in `main-2`; lead channel chartered in
  `ORCHESTRATOR.md` §3a. Completion watchers armed on all three lead terminals and
  the Design Director session.
- **Unity queue:** surething-ui holds the slot (S9 capture verification) →
  tv-sweat one window (3C verification + capture-harness spike). TV's 3C commit is
  additionally gated on DD inbox item 9 (win-prob display + momentum tape ruling).
- **Blocked:** none.
- **Rulings (Allen, 2026-07-31):** C1 — latest document governs, `DESIGN.md` §6
  stands, layout closed. C2 — interim: shipped green tolerated, cold white-grey
  target lands with TV Phase 3. T8 — remove: done, verified `842382d`. S11 — no
  licence-encumbered typefaces in the product; Bell Centennial dropped.
- **Watch:** TV's GPU booking is ON HOLD — its investigation found the machine
  rasterizes fine (room's `RoomViewCapture.cs` proves it); its own runs were
  `-nographics` by inherited convention. One lease tests batchmode-with-graphics +
  PlayMode-harness domain-reload survival; booking only if that fails (needs:
  PlayMode live, temporal capture around payoffs, seated 17° pose, ~6 recordings
  covering Phase 3 evidence + Phase 4 sweats).
- **Need Allen:** nothing.
