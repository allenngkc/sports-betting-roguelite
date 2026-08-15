# Speed levers — Allen's rulings, 2026-08-15

Recorded by the studio-architect session. Orchestrator executes all three.

## 1. Fast mode on ALL leads (conditional on plan support)

Allen approves fast mode for every lead seat — but he is on the Max plan and
suspects `/fast` may require pay-as-you-go usage credits. **Verify before
rolling out:** toggle `/fast` in ONE lead terminal. If it activates cleanly,
enable it on all lead seats (DD and orchestrator stay normal speed) and add
`/fast` to the STUDIO.md seat spec so reseats inherit it. If Claude Code
reports it needs extra-usage credits or is unavailable on this plan, stand
down, leave the spec unchanged, and tell Allen what it said.

## 2. Sim-harness parallelization (commissioned)

Route to the lane that owns the sim harness as a bounded engine task:
parallelize the gate-campaign across CPU cores. Requirements:

- **Determinism is load-bearing.** Seeded runs must produce byte-identical
  campaign results regardless of worker count — parallelize across
  independent seeds/runs, never inside one run's RNG sequence, and make
  aggregation order stable. Prove it: one campaign run serial vs parallel,
  results diffed identical, before the fast path is trusted for gates.
- **Idle-aware worker scaling.** Detect user presence via Windows last-input
  time (GetLastInputInfo or equivalent): input within the last ~5 min →
  low-worker mode (leave the machine responsive); idle 10+ min → scale to
  (cores − 2). Re-check between batches so a campaign adapts mid-run when
  Allen sits down or walks away.
- **Manual override.** A `--workers N` flag beats the auto detection.
- Baseline to beat: a full campaign currently costs ~85 minutes.

## 3. Wall-clock time audit (quick)

Produce a plain-language table for Allen: over roughly the last three days,
where did the studio's wall-clock actually go? Categories at minimum:

- Unity editor lease runs (suites, captures, bakes)
- Sim/gate campaigns
- DD verdict turnaround (docket staged → rulings landed)
- Lead build/implementation time
- Idle waiting on Allen (rulings, walkthroughs, playtests)
- Idle waiting on a dependency (editor queue, cross-lane holds)

Sources: STATUS.md cycle stamps and autonomous-decisions log, git commit
timestamps across worktrees, your own dispatch records. Estimates are fine —
the point is finding the biggest bucket, not accounting precision. Deliver
as a short plain-language report to Allen (no register codes).
