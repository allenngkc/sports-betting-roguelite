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
| `SweatSession.TicketWinProbability` | `double` | **The displayed win-probability (`T164`).** The live TICKET-level win prob — the same quantity the cash-out prices off. At t=0 it equals the ticket's sold probability (`Ticket.SameMatch.PTicket` bit-for-bit on a same-match ticket). **This is the only probability presentation may show.** |
| `SweatSession.FixtureCount` | `int` | Tellings on this ticket. `== Ticket.Legs.Count` unless the ticket has a same-match pair. |
| `SweatSession.CurrentFixtureIndex` | `int` | Which telling is in flight (0-based, first-appearance order). |
| `SweatSession.CurrentFixtureLegs` | `IReadOnlyList<int>` | The ticket-order leg indices LIVE right now. Length 1 on an ordinary ticket. **This is the live set phase 3 renders.** |
| `SweatSession.PendingDeadLegIndices` | `IReadOnlyList<int>` | Every leg that died at this whistle, in ticket order (`T143`: the window NAMES them all). Empty when no window is open. |
| `SweatSession.NoSingleCallSaves` | `bool` | True when ≥2 legs died at this whistle, so neither a Mulligan nor a Whistle can rescue the ticket. **`S85`: the surface states this BEFORE the offer is presented.** |
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

### 6g. Inside this lane, fixed here

`sim/RunPlayer.cs:251-252` — `evt.LegIndex == 0` / `== ticket.Legs.Count - 1` buckets same-match
cash-outs early/late. Re-pointed to the fixture cursor by this lane.

## 7. The one contract item where the evidence contradicted the plan

`T143` rules that the pending-loss window displays the TICKET's probability, reasoning from
`PendingLossProbBefore` being *"the leg's displayed win-prob from before the killer."*

**That display no longer exists.** `TvSweatScreen.cs:2405` records batch 46/47: *"the probability
GOES … an offer states its COST rather than its odds."* The window prints the whistle's cost. Grepped
across `unity/**` and `game-console/**`: **`PendingLossProbBefore` has zero live consumers** — the
only code that reads it is the engine's own Whistle roll, `roll.NextDouble() < _pendingLossProb`.

So re-basing it to the ticket would display nothing new and would silently make the Ref's Whistle
much weaker on multi-leg tickets — a consumable re-balance, which is Allen's call, not a
presentation ruling's side effect.

**This lane builds the reversible reading:** the roll keeps the leg's own pre-kill probability
(now one per dead leg); the ticket-level quantity `T143` and `T164` want is available as
`TicketWinProbability` for any surface that wants to show it. Routed to the DD; if the answer is that
the roll itself must re-base, that is a one-line change plus a gate re-baseline.
