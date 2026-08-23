# theater-engine — lane contract (seated 2026-08-21)

**Mandate:** PHASE 1 of the drawn-ending arm A — the engine's sweat-session and
probability-path restructure from per-leg to per-(ticket, fixture): N legs on one
match told ONCE, graded at one whistle. Allen ruled the fork (A) on 2026-08-21;
the spec (`docs/design/spec-drawn-ending-2026-08-19.md`) is approved as written.

## Read first, in order

1. `docs/5-orchestration/STUDIO.md` (roles, delegation contract, Unity lease).
2. `docs/design/t140-arm-a-plan-2026-08-21.md` — §3 PHASE 1 is your scope; §4 is
   the design input you need (all of it now RULED: §4.1–4.3 are batch 167).
3. `docs/5-orchestration/route-t140-cost-2026-08-19.md` — the TV lane's costing
   and the restructure table (SweatSession, DramaGenerator, _countLedger,
   live-leg locals). Two pre-build spec gaps are asserted in the plan's §2.
4. `docs/handoffs/sgp.md` — the joint model (`SameMatchModel`, `SameMatchPrice`,
   `Ticket.SameMatch`) is yours to build on, not re-derive.
5. Register rows named in the plan (`docs/design/REGISTER.md`, page by bytes):
   T140, T87-am2, T143, S85, T115-am, T142.

## Ownership

- **Owns:** `engine/**` for this restructure; `SBR.Engine.dll` — this lane
  **rebuilds AND COMMITS** the DLL with every engine change (stage it by explicit
  path; verify by loading it). Never `git add` a directory.
- **Does not own:** `unity/SBR/Assets/SBR/Runtime/**` (TV lane — phases 2–3
  land there AFTER this phase); `game-console/**` (markets lane);
  `ProjectSettings/**`, packages (integration-only). Never commit `URP.png`.
- Coordination: the TV lane's presentation (phase 3) consumes your session
  contract — publish the contract's shape in this file BEFORE changing it, and
  name every call site in `unity/` that your change breaks rather than fixing
  them yourself.

## Contract with design (already ruled — do not re-derive)

The telling contract (T140); grades land in LEG ORDER after ONE hold (T87-am2,
spec §3.2); the pending-loss window opens ONCE PER WHISTLE after every grade on
that fixture, naming every leg that died, and states when no single call saves
the ticket BEFORE the offer is presented (T143, S85); cash-out is a TICKET-level
fact — no leg's probability is ever shown alone (T143). §4.1–4.3 (batch 167): the
prose anchor under N live legs, the displayed win-probability's seed, the leg
counter (coupled to T91-cl).

## Evidence and gates

- Sim-harness first: the engine is fully testable without Unity (`dotnet test`
  under `engine.tests`; the gate campaign is bare `--gates`, floor 10,000 runs).
  Prove byte-identity on every ticket shape that does NOT contain a same-match
  pair — the restructure must be a no-op there.
- A new gate for the same-match pair: N grades at one whistle, one hold,
  leg-order grading, the window once. Report counts, not adjectives.
- Unity lease: you should not need the editor; if you do, ask the orchestrator.
- Delegation (STUDIO.md): bundled Sonnet dispatches; the lead plans, reviews,
  integrates. Sustained hands-on volume with zero spawns is a recorded deviation.

## Report

Result-first, telegraphic: Done / Next / Risk / Need Allen. Design questions →
orchestrator → DD. Scope/architecture → orchestrator.

---

# THE SESSION CONTRACT — published BEFORE the change (per the coordination clause)

**Published 2026-08-21, theater-engine lead, ahead of any edit.** The TV lane's phase 3 consumes
this. Every item below is stated as it is TODAY and as it will be AFTER phase 1, so the presentation
work can be planned against the target rather than the source.

**The one-sentence shape:** `DramaGenerator.BuildTicketPaths` returns one event path per **LEG**
today and one per **(ticket, FIXTURE)** after; `SweatSession` walks a leg cursor today and a fixture
cursor after. Everything else is an addition or a meaning-shift that is a **no-op on any ticket with
at most one leg per matchup**.

## 1. What is ADDED — nothing breaks on these

| member | type | what it is |
|---|---|---|
| `SweatSession.TicketWinProbability` | `double` | **The displayed win-probability (`T164`).** The live TICKET-level win prob — the same quantity the cash-out prices off. At t=0 it equals the ticket's sold probability: **exactly** `Π TrueProb` on an ordinary ticket, and within a few ulp of `Ticket.SameMatch.PTicket` on a same-match one — that slack is `JointModel`'s already-documented goal-family slack, not new. **This is the only probability presentation may show.** |
| `SweatSession.FixtureCount` | `int` | Tellings on this ticket. `== Ticket.Legs.Count` unless the ticket has a same-match pair. |
| `SweatSession.CurrentFixtureIndex` | `int` | Which telling is in flight (0-based, first-appearance order). |
| `SweatSession.CurrentFixtureLegs` | `IReadOnlyList<int>` | The ticket-order leg indices LIVE right now. Length 1 on an ordinary ticket. **This is the live set phase 3 renders.** |
| `SweatSession.PendingDeadLegIndices` | `IReadOnlyList<int>` | Every leg that died at this whistle, in ticket order (`T143`: the window NAMES them all). Empty when no window is open. |
| `SweatSession.NoSingleCallSaves` | `bool` | True when ≥2 legs died at this whistle, so neither a Mulligan nor a Whistle can rescue the ticket. **`S85`: the surface states this BEFORE the offer is presented.** |
| `SweatSession.PendingLossTicketProbBefore` | `double` | **The window's DISPLAY quantity (`T143-am`).** The TICKET's win-prob frozen from before the killing beat. Pairs with `PendingLossProbBefore`, which stays the LEG's and stays the Whistle's roll target. |
| `SweatSession.LiveLegProbability(int legIndex)` | `double` | One leg's live prob. **Engine/consumable use only — `T143` forbids showing a leg's probability alone.** Present because the Whistle's roll is per-leg. |
| `DramaEvent.FixtureIndex` | `int` | Which telling this beat belongs to. |
| `DramaEvent.LegIndices` | `IReadOnlyList<int>` | The legs live on this telling, ticket order. |
| `DramaEvent.LegProbs` | `IReadOnlyList<double>` | Their live probs after this beat, parallel to `LegIndices`. |

## 2. What CHANGES MEANING — compiles, moves only on a same-match ticket

| member | today | after phase 1 |
|---|---|---|
| `DramaEvent.LegIndex` | the leg this beat belongs to | the telling's **anchor leg** — the lowest ticket-order leg on that fixture. **Identical on every one-leg fixture.** Re-point to `LegIndices`/`FixtureIndex`. |
| `DramaEvent.WinProbAfter` | the leg's live win prob | the **anchor leg's** live win prob. Identical on one-leg fixtures. **Must not be displayed** — display reads `TicketWinProbability`. |
| `DramaEvent.Step` / `TotalSteps` | position within the LEG | position within the **FIXTURE telling**. Identical on one-leg fixtures. This is the clock that must not regress. |
| `SweatSession.PendingDeadLegIndex` | the one dead leg | the **FIRST** dead leg at this whistle. Retained for compile-compat; `PendingDeadLegIndices` is the honest one. |
| `SweatSession.PendingLossProbBefore` | the LEG's displayed win-prob before the killer | **UNCHANGED in meaning — now per dead leg**, via `PendingLossProbBefore` (the first dead leg) and `PendingLossProbBefore(legIndex)`. See §7: the display `T143` re-bases was already removed at batch 46/47, so the property's only live consumer is the Whistle's roll and re-basing it would be a silent economy change. Routed to the DD. |

## 3. The ONE structural break

`DramaGenerator.BuildTicketPaths(ticket, drama, config, round)` keeps its signature and returns
`IReadOnlyList<IReadOnlyList<DramaEvent>>` as before — but **`paths.Count` becomes the FIXTURE count,
not the leg count.** `SweatSession`'s constructor guard moves with it. Any caller that indexes
`paths[legIndex]` is wrong after this change; there are none outside `engine/` today, and the
constructor is `internal`.

## 4. What is UNCHANGED

`IsComplete`, `HasPendingLoss`, `CanMulliganPendingLoss`, `MoveNext`, `DeclinePendingLoss`,
`RevealedLegState`, `CashOutFair`, `CashOutOffer`, `AcceptCashOut`, `ApplyLiveEffect`,
`DramaEventType`, `TensionTag`, and every `DramaGenerator` constant. `Run`'s whistle/mulligan seams
keep their signatures.

**And the money is unchanged.** A ticket with at most one leg per matchup prices, pays, voids and
settles bit-identically — the sgp lane's invariant, extended to the drama beats themselves.

## 5. Why byte-identity is STRUCTURAL rather than measured

- Legs are grouped by `SameMatchModel.GroupByMatchup` (`engine/JointModel.cs:1347`), the same
  first-appearance grouping the joint price already uses. `Ticket.SameMatch == null` ⟺ no two legs
  share a `Matchup` ⟺ **every group is a singleton** ⟺ fixture order == leg order.
- `BuildTicketPaths` keeps drawing from `Rng.Drama` **exactly as today** — per leg, in ticket order,
  same four draws (count, per-step noise, near-miss roll, conditional near-miss step). Only the
  ASSEMBLY changes: each leg's track is resampled onto the fixture's shared clock `K = max(kᵢ)`.
  A one-leg group has `K = k`, so the resample is the identity map and the emitted beats are
  bit-identical. **The drama stream is never re-ordered, so later tickets in the round stay aligned
  too.**
- Cash-out partitions by telling stage but builds every leg list in **TICKET order** —
  `EnsureJoints`' bit-identity to `SameMatchPrice.PTicket` depends on that ordering, and the
  `SameMatch == null ? Product : Joint` structural guard is preserved verbatim.

## 6. Call sites this breaks — NAMED, not fixed (they belong to other lanes)

**None of these are touched by this lane.** Every one is in `unity/SBR/Assets/**`. They are listed by
what the change does to them, not by file, so phase 3 can be planned against the failure mode.

### 6a. Assumes ONE leg resolves per final beat — breaks on N grades at one whistle

| site | what it does |
|---|---|
| `TvSweatScreen.cs:2056` | `RevealedView.ResolveLeg(evt.LegIndex, grade)` — one leg. Must loop `evt.LegIndices`. |
| `TvSweatScreen.cs:2057` | `_tape?.ResolveLeg(evt.LegIndex, grade)` — same. |
| `TvSweatScreen.cs:2085`, `:4030` | `_resolvedThrough = evt.LegIndex + 1` — a high-water mark that assumes ticket order == resolution order. |
| `TvSweatScreen.cs:2086`, `:4031` | `UpdateTicketColumn(evt.LegIndex + 1)` — same assumption. |
| `TvSweatScreen.cs:2087`, `:4009` | `int k = evt.LegIndex + 1` — the resolved-leg count. |
| `SweatPresentationModel.cs:56-58` | `if (evt.LegIndex != _anchorLeg)` — a single anchor leg per beat. |

### 6b. Assumes the beat's leg IS the subject — needs `T163`'s anchor rule

| site | what it does |
|---|---|
| `TvSweatScreen.cs:1721`, `:3466`, `:4008` | `Leg leg = _ticket.Legs[evt.LegIndex]` — the flavour's subject leg. Under N-live this is `T163`: ticket-and-fixture derived, and it admits *NEITHER*. |
| `TvSweatScreen.cs:3470-3472` | `_flavorLegSeen` — flavour re-keyed on leg change. |
| `SweatFlavor.cs:25-64`, `:201`, `:216` | `PickedHomeForPresentation`-derived `picked`/`other`. **`T163`'s neither-branch line set is owed from the DD and is phase 3's, not this lane's.** |
| `TvSweatScreen.cs:1683` | `bool onFinalLeg = evt.LegIndex == lastLeg` — should be the final FIXTURE. |
| `TvSweatScreen.cs:3515-3516` | `BeginStageLeg(evt.LegIndex, leg, evt.TotalSteps)` — the stage is keyed per leg; becomes per fixture. |

### 6c. Displays a LEG's probability — `T164`/`T143` forbid it

| site | what it does |
|---|---|
| `TvSweatScreen.cs:76` | `WinProbability = HasTicket ? (float)current.Legs[0].TrueProb : 0f` — `RevealedView.Reset`, the seed `T164` names. **Re-point to `SweatSession.TicketWinProbability`.** |
| `TvSweatScreen.cs:3517` | `_stage.SetLiveProb((float)evt.WinProbAfter)` |
| `TvSweatScreen.cs:3526` | `_probTarget = (float)evt.WinProbAfter` |
| `TvSweatScreen.cs:3497`, `:3511` | `_prevProb` / `_pendingProb` from `evt.WinProbAfter` |
| `TheaterChoreographer.cs:217`, `:235`, `:285` | `evt.WinProbAfter` into `StageBeatGoal` and the final template pick. **Goal staging is a per-leg fact and may legitimately stay leg-scoped — but it must read `LegProbs`, not `WinProbAfter`, or it silently reads the anchor leg's number for every leg.** |

### 6d. The leg counter — `T165`, and it lands with `T91-cl`

`TvSweatScreen.cs:3468` — `_tLeg.text = $"LEG {evt.LegIndex + 1}/{_ticket.Legs.Count}"`. Under arm A
this counts the wrong thing and has no single answer to print. `T165` rules the referent moves to the
FIXTURE; the form and width are TV's, and `T165` says land it with `T91-cl` or the element moves twice.

### 6e. The golden pin — this lane's strongest external check, and it must stay GREEN

`unity/SBR/Assets/Probe/GoldenReplay.cs:110-116`, `:149-153` assert `(LegIndex, Step, Type, Tag)`
exactly and `WinProbAfter` to `1e-6` over the first ten beats, plus a fold hash over the whole path.
**It is a byte-identity pin on drama events that already exists, written by another lane.** If the
golden ticket carries no same-match pair this probe must pass unchanged after the restructure — that
is §5's structural claim, checkable by a third party. This lane will not touch the file; the same
assertion is mirrored into `engine.tests` so it can be proven without a Unity lease.

### 6e-bis. Unity test files that call `BuildTicketPaths` directly — VERIFY, do not assume

`unity/SBR/Assets/Tests/EditMode/ScoreLedgerTests.cs:251` and
`unity/SBR/Assets/Tests/EditMode/TheaterChoreographerTests.cs:208` build paths themselves. They are
unaffected **if and only if** their tickets carry no same-match pair — on those, `paths.Count` is
still the leg count and the beats are bit-identical. Named for the TV lane to confirm under a Unity
lease; this lane does not hold one and will not edit them.

`unity/SBR/Assets/Tests/EditMode/ScoreLedgerTests.cs` also constructs `DramaEvent` with the 6-argument
constructor at nine sites. **That constructor is retained unchanged** for exactly this reason.

### 6f. Already fixture-ready — a reduction, named so phase 3 does not rediscover it

- `PresentationSceneKey.cs:70-82`, `:110-122` — the scene key is **already match-scoped**
  (`MatchIndex`, sourced from `Leg.Matchup.Index`), and its author wrote the note asking for exactly
  this change: *"a fully shared, single event cursor per match … is an engine concern outside this
  file's boundary."* After phase 1 `DramaEvent.Step` IS that shared cursor, and the doc note can be
  discharged rather than worked around.
- `TvSweatScreen.UpdateTicketColumn`'s doc comment — `T142` struck its stale half ("the engine forbids
  two legs on one matchup"); **that clause becomes false in code with this change.** Its other half —
  the column reads legs as a collection and is N-live-capable by construction — stands and is
  load-bearing for phase 3.

### 6g. `game-console/` — the markets lane's, and it has the SAME per-leg drive loop

Found by the call-site survey; an earlier grep of mine truncated before reaching these files, so they
are recorded here in full rather than summarised.

| site | what it does |
|---|---|
| `game-console/SweatRenderer.cs:73-75` | `int lastLeg = ticket.Legs.Count - 1` — the console's own final-leg scalar. |
| `game-console/SweatRenderer.cs:83-90` | `Leg leg = ticket.Legs[evt.LegIndex]; if (evt.LegIndex != legSeen) { legSeen = …; prevProb = leg.TrueProb; }` — the same single-leg-cursor drive loop as `TvSweatScreen.PlaySweat`. |
| `game-console/SweatRenderer.cs:129` | `bool onFinalLeg = evt.LegIndex == lastLeg` |
| `game-console/SweatRenderer.cs:153`, `:158` | `session.PendingLossProbBefore` in `PromptSave` — **the one surviving reader of that property anywhere outside the engine.** It still reads the LEG's number, which is what §7a preserves. |
| `game-console/SweatRenderer.cs:165-192` | `int k = e.LegIndex + 1`; `session.RevealedLegState(e.LegIndex)` |
| `game-console/EventText.cs:14` | `For(DramaEvent e, Leg leg, double prevProb)` — one leg per call, same shape as `SweatFlavor`. |

**On a ticket with no same-match pair none of this moves.** The console reaches the new shape only
when a player builds a same-game parlay there.

### 6h. Comments that cite `SweatSession.cs` by LINE NUMBER — they go stale silently

`TvSweatScreen.cs:2933` cites `SweatSession.cs:252-253, :503-508`; `:2940-2942` cites
`SweatSession.cs:136-140, :150-154, :184-185`. These are prose, so nothing fails — they just stop
describing the engine. Named because a line-number citation into a file another lane is restructuring
cannot survive it, and a reader will trust it.

### 6i. Inside this lane, fixed here (`sim/**`)

| site | what it does, and why it matters |
|---|---|
| `sim/RunPlayer.cs:249-252` | `evt.LegIndex == 0` / `== ticket.Legs.Count - 1` buckets same-match cash-outs EARLY/LATE. |
| `sim/SkilledStrategy.cs:473-486` | `EstHoldEv` partitions legs into resolved (`j < cur`), live (`j == cur`) and unstarted (`j > cur`) off `evt.LegIndex`. Mis-partitions a shared telling — it would price two live legs as one live and one unstarted. |
| `sim/SameMatchStrategy.cs:169-192` | **The severe one.** The EARLY/MID/LATE cash-out probe is `cursor == 0` / `cursor >= 1 && cursor < lastLeg` / `cursor == lastLeg`, evaluated on SAME-MATCH tickets — exactly the population arm A restructures. A 2-leg same-match ticket now has ONE telling, so `evt.LegIndex` is 0 for every beat: **MID and LATE would never fire and `G7-SGP`'s cash-out coverage would quietly go vacuous — a passing gate that had stopped testing anything.** Re-pointed to position WITHIN the telling (`evt.Step` against `evt.TotalSteps`), which is the honest reading of "early/late in the sweat" once a fixture is the unit and works unchanged for multi-fixture tickets. |
| `sim/IStrategy.cs:90` | Doc comment asserts `evt.WinProbAfter` is "the on-screen live win% of the current leg". Now the anchor leg's, and no longer the on-screen number at all (`T164`). Corrected. |

**This is a finding, not a chore.** A gate that cannot fail is worse than a missing gate, and the
restructure would have produced one silently.

## 7. The three design items — ROUTED AND ANSWERED (DD batch 169, at HEAD)

All three were built on stated assumptions; **all three assumptions hold** and no rework follows.

### 7a. `PendingLossProbBefore` — the split is RIGHT (`T143-am`, ground `S67`)

The finding that raised it: `T143` re-bases the window's probability to the ticket, reasoning from
`PendingLossProbBefore` being *"the leg's displayed win-prob from before the killer."* **That display
was removed at batch 46/47** (`TvSweatScreen.cs:2405` — *"the probability GOES … an offer states its
COST rather than its odds"*), and the property has **zero consumers outside `unity/`** — grepped
across `unity/**` and `game-console/**`. Its only live reader is the engine's own Whistle roll,
`roll.NextDouble() < _pendingLossProb`. Re-basing it would have displayed nothing new and quietly
weakened the Ref's Whistle on every multi-leg ticket — a consumable re-balance.

**Ruled:** display goes ticket-level, the roll keeps the leg's prob. Built literally, as two frozen
values captured before the killing beat: `PendingLossTicketProbBefore` (display) and
`PendingLossProbBefore` (roll, now one per dead leg). The economy does not move.

### 7b. `T164`'s "moves no number on any screen shipping today" — false, and re-scheduled (`T164-cl`)

The TV shows the live LEG's probability today on a multi-leg ticket, so re-pointing the display to
the ticket's number **is** a visible change. The ruling stands; it is re-scheduled as a visible one.
Nothing changes in this lane's build — `TicketWinProbability` is purely additive here and the
re-point is phase 3's.

### 7c. Two or more legs dead at one whistle — saves stay LEGAL

Neither a Mulligan nor a Whistle can rescue the ticket, but the player may still spend one.
**This lane builds the FLAG only** — `NoSingleCallSaves`. The present-with-warning affordance
(`S85`: stated before the offer) is phase 3's, not this lane's.


---

# FOR ALLEN — arm A retires the certainty carve-out (via the orchestrator)

**One ruling has been made unreachable by another, and that is his to know rather than mine to
absorb.**

On **2026-08-14** Allen ruled the CERTAINTY CARVE-OUT: *a certainty never quotes below its worth.*
It exists for one shape — a **settled** leg that ENTAILS a **live** one, a settled `OVER 3.5` beside a
live `OVER 2.5`. There `P(L | S)` is 1 while the drama's number for the live leg is the board's
unconditional marginal and is not, so the cash-out re-weight would scale the quote DOWN on a leg that
cannot lose. The carve-out drops the re-weight in exactly that case and quotes the pure conditional.

**`T140` arm A makes that state unreachable.** Entailment is only ever between two legs on the SAME
match — legs on different matchups are independent by construction, and `JointProbabilityOf`
factorises over matchups, so for a settled set `S` and a live set `L` on disjoint matchups
`P(L | S)` is just `P(L)`, the live legs' own marginal, which no sellable board price approaches 1.
And under arm A two legs on one match are **never in different stages**: they are one telling, live
together and graded at one whistle. The pairing the carve-out exists for cannot occur. This is not
inferred from a failing test — it is the test's own construction: `PlaceEntailment` builds precisely
that settled-entails-live pair, and the state it walks to no longer exists.

**What is lost: nothing in money.** The harm the carve-out prevented was under-quoting a certainty,
and a certainty can no longer be half-settled. Within a shared telling the correlation is still
carried exactly, by the joint in the quote's denominator; each live leg drifts on its own number
against its own baseline, so no leg is scaled by another's marginal. The guard becomes dead code
rather than wrong code, and this lane has **left it in place** — it is still correct if the shape ever
returns, and deleting a ruling is not a lane's call.

**What is worth knowing anyway**, because it is the same physics surfacing somewhere new: the drama
now runs entailed legs as two independent tracks on one clock, so a shared telling can show
`OVER 2.5` reading *below* `OVER 3.5` for a beat — incoherent as a pair, though the quote is
unaffected because the pricing correlation never comes from those tracks. Nobody has ruled what a
shared telling's per-leg numbers owe each other. Not a defect against any current spec, and not this
phase's to fix; recorded so it is found on purpose rather than in a capture.

**Asked of Allen:** nothing blocking. Only whether the 2026-08-14 ruling should be recorded as
SUPERSEDED-BY-CONSTRUCTION, so a later reader does not go looking for behaviour that cannot fire.


---

# WHAT THE EVIDENCE ACTUALLY COVERS — and the one clause it does not

The contract asks this phase to prove four things about a shared telling. Three are measured. The
fourth is not measurable from outside the engine, and is recorded as what it is rather than folded
into the green.

| clause | status |
|---|---|
| **N grades at ONE whistle** | **MEASURED.** `SharedTellingTests` asserts exactly one `LegFinal` for a two-leg fixture and that both legs are revealed at it, each to its own result; `G8-ARMA` counts it across the campaign. Before arm A that fixture emitted two whistles — the falsifier fires on the old behaviour. |
| **ONE hold, not N** | **MEASURED, as far as the engine owns it.** The hold itself is presentation (`T87-am2`, TV's); the engine's half is that there is one whistle beat to hold on, which is the row above. |
| **the window ONCE per whistle** | **MEASURED.** One window opens after every grade on the fixture has landed, naming every leg that died, with `NoSingleCallSaves` true where more than one did and — the half that makes it a real distinction — false where only one did. |
| **grades land in LEG ORDER** | **NOT MEASURED. Construction and review only.** |

**Why the last one is not measured, stated rather than glossed.** The order grades land in is the
order `_effects.OnLegResolved` is called, and that is not reachable from outside `SBR.Engine`:
`EffectEngine.Add` builds its behaviours through `RelicBehavior.Create(def)` from the shipped
catalogue, so a test cannot register an observer without putting test scaffolding into product code.
Nothing in the public surface records the order afterwards — `RevealedLegState` reports a leg's final
state, not when it got it.

**What supports the clause instead:** `ResolveFixtureFinal` iterates `_fixtures[_currentFixture]`
directly, and `SameMatchModel.GroupByMatchup` appends leg indices in ascending ticket order, so the
walk is ticket order by construction. `FixturePathTests` pins the parallel fact on the data — that
`DramaEvent.LegIndices` is ascending ticket order, including when the fixture's legs are NOT
contiguous in the ticket. Between them the claim is well-founded; it is still an argument and a
code-read, not a measurement, and it should not be reported as one.

**If it must be measured**, the cheapest honest route is an `internal` test seam on `EffectEngine`
plus `InternalsVisibleTo` for `SBR.Engine.Tests` — a real change to the engine's public shape for the
sake of one assertion, which this lane did not make on its own authority. Route it if the phase's
review wants the clause measured rather than argued.


---

# PHASE 1 — LANDED

**Suite: 324 passed, 0 failed, 1 skipped** (baseline 307/0/1, +17 new tests). `engine`, `sim` and
`game-console` all build clean in Release. `SBR.Engine.dll` rebuilt in Release and committed.

| commit | what |
|---|---|
| `d28f36b` | the session contract, published BEFORE the change, with every broken call site named |
| `e8492b5` | the restructure — one telling per (ticket, fixture) |
| `388ac16` | session-level pins on the shared telling; the carve-out escalation |
| `45b8224` | same-match quotes re-pointed; `G8-ARMA`; the rebuilt DLL |

## The evidence, and what each piece actually proves

- **Byte-identity on every ticket without a same-match pair — STRUCTURAL, then confirmed twice.**
  The drama stream is untouched by construction (§5). `GoldenSeedTests`' 14-beat `(LegIndex, Step,
  Type, Tag)` pin plus its first-ten `WinProbAfter` and settled bank — written by another lane before
  this one existed — is **green, unmodified**. `FixturePathTests`' stream-position test passed on its
  first run: a same-match ticket leaves the drama stream exactly where an ordinary ticket of the same
  leg count would, asserted with `==`.
- **N grades at ONE whistle** — `SharedTellingTests` (session mechanics) and `G8-ARMA` (population).
- **The clock never regresses** — asserted inside a telling AND across a fixture boundary.
- **One window per whistle**, naming every dead leg, with `NoSingleCallSaves` true on multi-death and
  **false on single-death** — the half that makes it a distinction rather than a constant.
- **Grades in LEG ORDER** — argued, not measured. See the section above; do not report it as measured.

## `G8-ARMA` has TWO coverage arms, and the second one was missing at first

The same-match probe builds only SINGLE-fixture tickets, so on its batch `tellings ==
sharedTellings`: it witnesses N-legs-one-whistle and never a fixture BOUNDARY. The gate's first cut
nevertheless described itself as checking multi-fixture tickets. **The claim was false, so the gate
was fixed rather than the sentence** — the clock rules are asserted a second time over the skilled
batch, where ordinary multi-matchup parlays live, gated on `ArmAMultiFixtureTickets > 0`.

`T140-am` is why this matters and not merely tidy: a gate written *per ticket* instead of
*per (ticket, fixture)* would FAIL a correct multi-fixture broadcast — the exact over-reach the spec
was corrected for — and only a population that HAS boundaries can rule that out.

## What phase 1 does NOT do

- **No `unity/` runtime change.** Every broken call site is named in §6 and belongs to the TV lane.
  Nothing outside `engine/` was edited except `sim/` (this lane's, for the gate) and `engine.tests/`.
- **`§6.7` at the fixture boundary is NOT here.** `T140-am` scoped it out of `T140` entirely; the
  engine now makes the boundary *addressable* (`FixtureIndex` advances, `CurrentFixtureLegs` changes)
  but draws nothing.
- **Phases 2 and 3 are untouched** — the count ledger's N-live lifecycle, the live set, the pulse, and
  `T165`'s fixture counter (which lands with `T91-cl`, or the element moves twice).
- **`T163`'s *neither*-branch flavour lines** are owed from the DD and belong to phase 3. The engine
  supplies the RULE's inputs (`CurrentFixtureLegs`, `LegIndices`); it authors no copy.

## The one thing the next lane should not have to rediscover

`SweatSession.TicketWinProbability` is the ONLY probability presentation may show (`T143`, `T164`).
`DramaEvent.WinProbAfter` is the anchor leg's and is a pricing input — it survives because cash-out
and the Whistle's roll need a per-leg number, not because anything should display it.


---

# THE PLUGIN DLL WANTS A CLEAN RELEASE BUILD — a trap this lane walked into

**The rule:** an engine-owning lane must build `SBR.Engine.dll` with a **clean** Release build before
committing it — wipe `engine/obj/Release` and `engine/bin/Release` first. Never commit the output of
an incremental one.

**Why, measured rather than assumed.** The DLL committed at `45b8224` was not reproducible: a clean
Release build of the *same, unchanged* source produced different bytes. Two readings were possible
and they want opposite responses — a non-deterministic compiler (harmless churn, ignore it) or a
stale artifact (Unity is running the wrong engine). So it was measured: **two builds from wiped
`obj/` and `bin/` are byte-identical**, so the build is deterministic and the churn was a STALE
ARTIFACT. Re-committed clean at `0f00122`; engine source unchanged since `e8492b5`, so no behaviour
moved.

**Where the staleness comes from, and it is the routine practice itself.** Test runs in this repo use
`-p:SbrUnityPluginDir=<scratch>` so the tracked Unity DLL stays clean during iteration — correct, and
recorded practice. But those runs leave intermediates in `engine/obj`, and a later incremental
`dotnet build engine -c Release` layered on top of them emits a binary that no clean build reproduces.
The habit that protects the working tree is exactly what poisons the artifact at the end.

**How to verify before committing:** hash the output and confirm it is identical in all three places
it lands — `engine/bin/Release/netstandard2.1/`, `sim/bin/Release/net10.0/`, and
`unity/SBR/Assets/Plugins/SBR/`. If they disagree, something rebuilt between them and the commit
would capture whichever ran last.


---

# THE GATE CAMPAIGN — RUN AND PASSED (2026-08-22)

**Bare `--gates`, the ruled floor of 10,000 runs. 9 gates evaluated, 9 passed, 9 produced a verdict.**
Artifact: `docs/theater-engine/gate-campaign-arm-a-2026-08-22.md` (the report in full, reproducible —
the sim pins its seeds, so the same arguments reproduce the body byte-for-byte).

| gate | verdict | reading |
|---|---|---|
| `G1` | PASS | median 4, won 0.1% |
| `G2` | PASS | median 5, won 0.0% |
| `G3` | PASS | median 5, won 5.2% (0.7pp from the nearest band edge) |
| `G4` | PASS | EV arc crosses at R3 |
| `G5` | PASS | synergy excess +3.0pp |
| `G6` | PASS | martyr-worst 4.8% vs skilled 5.2% |
| `G7` | PASS | all shipped markets covered |
| `G7-SGP` | PASS | placed 106,419 · settled 71,056 · kinds 15/15 · no-label fallbacks 0 · cashed out 35,363 (14,967 early / 10,395 mid / 10,001 late) |
| **`G8-ARMA`** | **PASS** | **tellings 106,419 · shared tellings 106,419 · whistles 71,056 · extra whistles 0 · clock faults 0 · grades at shared whistles 179,669 of 179,669 expected · mismatches 0 · windows opened 12,173 · multi-death windows 5,666** |
| | | **boundary arm (skilled): multi-fixture tickets 29,019 · tellings 81,559 · clock faults 0 · extra whistles 0** |

## What the numbers say, read rather than glanced at

- **THE FALSIFIER FIRED AND HELD. `extra whistles 0` over 106,419 shared tellings.** Before arm A
  every one of those fixtures emitted N `LegFinal` beats; a single leftover per-leg path anywhere in
  the campaign would have shown here. This is `T140` arm A landing, counted.
- **`grades landed at shared whistles 179,669 of 179,669 expected`** — every leg on every shared
  fixture graded at its own whistle, none early, none missed. 179,669 against 106,419 tellings is the
  N-legs-per-fixture ratio made visible: **73,250 legs that used to need their own telling no longer
  do.**
- **`clock faults 0` on BOTH arms.** `T135`'s rewind is gone inside a telling (same-match arm) AND
  across a fixture boundary (skilled arm, **29,019 multi-fixture tickets**) — the second arm being
  the one added because the same-match probe alone never witnesses a boundary, and `T140-am` warns
  that a badly scoped gate would FAIL a correct multi-fixture broadcast rather than pass it.
- **`multi-death windows 5,666`** — `S85`'s state is real and common, not theoretical. In 5,666 cases
  two or more legs died at one whistle and `NoSingleCallSaves` was true, which is the fact phase 3's
  affordance must state before presenting the offer.
- **THE ECONOMY DID NOT MOVE.** `G1`–`G6` are the economy gates and all six pass at their pre-arm-A
  criteria — `G3`'s skilled win rate at 5.2% inside its band, `G6`'s worst-case loss-farmer 2.4pp
  from the breach line. The restructure is presentation-shaped and the campaign says so in numbers.
- **`G7-SGP` still covers what it covered**: 15/15 market kinds, zero no-label fallbacks, and 35,363
  same-match cash-outs split 14,967 / 10,395 / 10,001 across early / mid / late. **That split is the
  proof the re-pointed EARLY/MID/LATE probe did not go vacuous** — the defect this lane caught in
  `sim/SameMatchStrategy` would have collapsed mid and late to zero while the gate still passed.

## One flag, pre-existing and not this lane's

`⚑ UNDEREXPOSED: Chalk Eater (0 wound-up runs < 200)` — an item-exposure flag from the audit table,
not a gate verdict. It is unrelated to arm A and was present before this phase; recorded so it is not
mistaken for fallout.

## A note on running it

The campaign took two wall-clock days across three attempts, and none of that was the engine's fault:
the first two runs were killed by session teardown (a detached `Start-Process` survives an agent
restart but not the harness's own cleanup), and the machine slept overnight mid-run. **`--workers 16`
did not speed it up meaningfully** — `WorkerPolicy`'s own note is right that this workload reaches
~5–6 cores whatever the degree, so the worker count was never the ceiling. Budget a campaign in
compute-hours on a machine that stays awake, not in wall time.
