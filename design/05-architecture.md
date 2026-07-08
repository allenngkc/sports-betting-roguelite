# 05 — Architecture

## Decisions already made (see DECISIONS.md)

Unity 6 (LTS) + C#. Rationale: largest community/tutorial corpus, C# is a top AI-assisted language, Asset Store for juice, cleanest mobile path for the v2 wave. Unity Personal free under $200K revenue; splash optional.

## The two-project split (the load-bearing decision)

```
/engine    — pure C# class library. ZERO UnityEngine references. netstandard2.1.
/game      — Unity project. References engine as a local package/DLL. Presentation only.
/sim       — console runner over /engine: Monte Carlo, balance reports, regression seeds.
```

`/engine` owns: four-number model, slate generation, odds/vig, drama generator (event sequences as data), relic effect resolution, run state machine, economy, RNG.
`/game` owns: rendering the event stream, input, juice, save UI, Steam integration.
`/sim` owns: the questions in `02-betting-math.md` — runs 100K seasons overnight, prints survival curves and relic power audits.

Why this is the hill to die on: AI writes and tests `/engine` at full speed with no editor in the loop; balance is empirical instead of vibes; itch web build and mobile port reuse the core untouched; determinism bugs are reproducible from a seed.

## Effect system

- Hooks per `03-mechanics-catalog.md`. Relics = data (JSON/ScriptableObject) declaring subscriptions + parameterized effects; a small effect-op vocabulary (AddOdds, RevealP, MultPayout, InjectEvent, ModifyCashoutMargin, …) interpreted by the engine.
- Content as data means AI can draft 50 relic candidates into JSON, `/sim` power-audits them, and we keep the interesting third.
- Ordering: deterministic effect resolution order (acquisition order, Balatro-style), explicit and visible to player.

## RNG discipline

Named, separately seeded streams: `outcomes`, `drama`, `slate`, `shop`, `events`. Run seed shown on the run-over screen from day one (debug + community sharing + daily challenge later, free).

## The sweat as a steppable process (Week 2 shape requirement)

The sweat is a cursor over drama events, not a pre-baked immutable list. Cash-out requires this anyway (input is polled between events); it also creates the **intervention seam** live relics need later: `ApplyLiveEffect` at a step boundary → recompute honest conditional `p` → re-sample the remaining outcome from the outcomes stream → drama generator re-authors the remaining events toward the (possibly new) result. Explicit and visible, per design/04's integrity rule. v0 builds the stepping structure but never calls the seam — no live relics in the prototype (PRD scope), no door closed for them either.

## Presentation layer notes

- Engine emits an ordered **event stream** per sweat; Unity plays it back on a timeline with pacing control. The game is fundamentally a fancy event-stream player — keep it that way.
- UI: prefab-based, UGUI or UI Toolkit (OPEN — UI Toolkit is newer/cleaner, UGUI has 10× the community answers; given AI-heavy workflow, lean UGUI).
- Juice via PrimeTween + Feel (see `06-vfx-and-juice.md`).

## Workflow & tooling

- Git from commit one; Unity .gitignore; LFS for binaries. Repo layout = the three projects above + `/design`.
- Tests: xUnit/NUnit on `/engine` (odds math gets exhaustive tests — it's the trust foundation), golden-seed regression tests for the drama generator.
- AI collaboration rule: logic lives in plain C# files; scenes stay thin; anything AI should iterate on must not live in serialized scene state.
- CI later (GitHub Actions running `/engine` tests + `/sim` smoke) — not Phase 1.

## Open questions

- Save system: engine-state snapshot as JSON (lean yes — trivially portable).
- Steamworks wrapper: Facepunch.Steamworks vs Steamworks.NET (decide at Steam-page phase).
- Web build for itch: Unity WebGL is heavy (~30–60MB) but acceptable; confirm engine determinism under IL2CPP/WebGL early with a golden-seed test.
