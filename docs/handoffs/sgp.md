# sgp — lane handoff (correlated parlays / same-game parlay)

**Created:** 2026-08-12 · **Branch:** `sgp` (from main) · **Lead:** Claude (Opus 5)
**Charter:** `docs/5-orchestration/next-slices-2026-08-12.md` Lane 2 · **Plan:** F_0.6.0
**Design authority:** `design/02-betting-math.md` § *Same-game tickets — the correlation model*

## 1. Studio context (read in order)

- `docs/5-orchestration/STUDIO.md` — roles, ownership, merge protocol, autonomy policy.
- `docs/5-orchestration/next-slices-2026-08-12.md` — the charter.
- `design/02-betting-math.md` § *Same-game tickets* — the model, amended through batch 48.
- `docs/1-plans/F_0.6.0_same-game-parlays.plan.md` — step-3 phasing and the invariant.

## 2. Scope — RESEARCH-FIRST, strict order

1. **Research** — DONE, accepted 2026-08-12. Five docs in `docs/sgp/`.
2. **Correlation model into `design/02-betting-math.md`** — DONE, accepted 2026-08-12.
3. **Engine change lifting the one-pick-per-matchup guard** — IN PROGRESS (phases 1–2 landed,
   phase 3 running).
4. **Gate re-validation on held-out seeds** — NOT STARTED. Note the campaign is **G1–G7**, not the
   charter's "six": G7 is market coverage, added after the charter was written.
5. **Presentation** — NOT STARTED.

## 3. State

**Landed on branch:** exact joint evaluator with S73 relation labels (`engine/JointModel.cs`, new);
joint ticket pricing with the `κ` dial; the one-pick-per-matchup guard **replaced** by a
`p_joint = 0` validity rejection; sub-evens refusal.

**Verification:** `dotnet test engine.tests` → **211 passing, 0 failed** (183 at branch start).
Joint evaluator agrees with `MatchModel.TrueProbability` to 2.442e-15 over 432,000 checks; zero
correlated-but-unlabelable combinations over 151,200 pairs and 128,520 triples; the no-label
fallback counter reads 0 across 21,528 priced same-match tickets.

**The invariant everything rests on:** a ticket with at most one leg per matchup prices, pays, voids
and settles **bit-identically to before this lane**. Enforced structurally — the old
product-of-leg-odds path is kept verbatim and still runs for those tickets — and pinned by `==`
assertions over swept populations. Any change that routes ordinary tickets through the joint path
breaks the gate baseline and must not be made for tidiness.

**Not yet done, and known:** void re-pricing (phase 3, running); conditional cash-out (phase 4,
ruled to land **after** step 4's gates). Until phase 4, same-match cash-out prices off the naive
product — which is why Allen approved that step 4's coverage strategy **holds same-match tickets to
settlement** rather than cashing them out. That is a named, accepted coverage gap, closed by a
re-run after phase 4.

## 4. File ownership — merge-critical, read before sequencing

- **Owned by this lane:** `engine/JointModel.cs` (new file, no other lane touches it).
- **Shared, edited here:** `engine/Domain.cs` (ticket price + void scenarios), `engine/Run.cs`
  (`PlaceTicket` validity + pricing), `engine/RunConfig.cs` (one appended property, `SgpMargin`),
  `design/02-betting-math.md`, `engine.tests/**`.
- **`engine/MatchModel.cs` is deliberately UNTOUCHED by this lane.** markets-pregame (Lane 1) is
  editing it to introduce draws. Every dispatch here has been forbidden from changing it, including
  whitespace. The joint evaluator lives in its own file specifically so the two lanes do not collide
  in that file. Preserve this if further work is dispatched.
- **Known textual collision:** `design/02-betting-math.md:24` was rewritten here in step 2 and also
  edited at integration (4917dc7 lineage). Both carry the honest-book framing, so it is textual, not
  semantic — but it wants a hand at merge.

## 5. Draws (Lane 1) — the standing dependency

Draws were greenlit 2026-08-12 and are being introduced by markets-pregame. **Do not assume their
timeline; sequence through the orchestrator.** The model is draws-agnostic by construction: the
goal-family sum runs over the model's outcome partition rather than a hard-coded home/away pair, and
the partition is discovered at type-load, so a third class costs no edit here and a mismatch throws
loudly instead of mispricing. `MarketChoice` has no `Draw` member yet, so a three-way moneyline
throws a clear error at merge rather than silently mispricing.

**Every measured figure in the design section is pre-draws** — the impossible-shape counts, `ρ`
ranges, and the independent share all need re-measuring once draws land. The method is recorded in
`docs/sgp/correlation-recon.md` §1, so that is a re-run, not a rebuild.

## 6. Rules inherited

- §7a settings churn discipline; explicit-path staging; suites green before merge requests.
- Money-language and market-presentation questions route to the Design Director through the
  orchestrator. The open DD question is `docs/sgp/dd-question-same-game-pricing.md` (relayed;
  amended after relay as canon landed).
- Report telegraphic, result-first: Done / Next / Risk / Need.
- **Inherited trap, live:** `dotnet` builds copy `SBR.Engine.dll` into the Unity tree and dirty a
  tracked LFS asset. Restore with checkout after every build; never commit it.
