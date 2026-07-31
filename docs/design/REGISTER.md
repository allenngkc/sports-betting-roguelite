# Design register

**Owner:** Design Director (`main-2`) · **Opened:** 2026-07-30
**States:** Exploration → Candidate → Approved (Allen) → Implemented → Design-verified

One line per item. Built from the three slices' own packages, not from the charter's summary.
Paths are worktree-relative: `[ST]` = `surething-ui`, `[RM]` = `room-refinement`, `[TV]` = `tv-sweat`,
`[M2]` = `main-2`.

**Nothing in the studio is Design-verified yet.** No implementation has been through design review
under this seat. That is the gap this register exists to close.

---

## SureThing — the laptop

| # | Item | State | Spec |
|---|---|---|---|
| S1 | Direction — "The Annotated Form Guide" (world 4 of 7 candidates) | Approved · Allen 2026-07-28 | `[ST] docs/design/direction-concepts/DIRECTIONS.md` |
| S2 | Artboard + legibility laws — 1024×704, 13px product-fact floor, 4.5:1, 44×32 targets, no pure black, status never colour alone | Approved · Allen 2026-07-28 | `[ST] .../SHARED-SPEC.md` |
| S3 | Colour language — olive-black `#16160F`, biro blue `#5E86B8` = player's choice, wax amber `#D9A441` = money/action, oxide red `#B4483A` = house's mark incl. dead-leg strike | Approved · Allen 2026-07-28; red amended 2026-07-30 | `[ST] .../DESIGN.md` §Colors |
| S4 | Design-to-UGUI contract — tokens, type, layout, components, do's/don'ts | Approved | `[ST] .../DESIGN.md` |
| S5 | Element kit — real-size component and state kit | Approved | `[ST] .../element-kit.html` |
| S6 | Lobby shell — annotated form guide | Implemented | `cb83c90` + uncommitted tree |
| S7 | Ink sprites — deterministic biro rings + strike, variant by matchup index | Approved (spec) · Implemented (partial) | `[ST] .../assets/ASSETS.md`; `SureThingInkImporter.cs` untracked |
| S8 | OS chrome — fictional OS identity, clock `02:47`, second affordance; personal not institutional | Approved (spec) | `[ST] .../SHARED-SPEC.md`; impl. state unconfirmed |
| S9 | Event detail, staged ticket, MY BETS, rewards, old slips | Approved (spec) · not implemented | `[ST] .../DESIGN.md` §Components |
| S10 | Sweat "loud register" for the laptop | Candidate | direction contract comment only; no built spec |
| S11 | Production typeface | **Closed** · DD 2026-07-31 — **Archivo + Archivo Narrow** (OFL 1.1). Chosen for the function Bell's ink traps served (small type surviving a degraded surface); true superfamily, shared metrics, tabular figures both widths. One `LoadFont` + two TMP assets | DD ruling S11-A |
| S12 | Rejected comparison — "The Catalogue Sleeve" | Exploration (closed) | `[ST] .../direction-2-catalogue-sleeve.html` |
| S13 | Earlier explorations — Tote Hall / Broadcast Alert / Night Board | Dumped · Allen 2026-07-28 | `[ST] .../DIRECTIONS.md`; do not revive |
| S14 | Form-guide identity — three build gaps, all already specified: two-voice type system unwired (condensed figures vs roman labels); 78px entry geometry drifted (pre-contract 660px board suspected); document layer missing (warm olive-black ground, warm bone toner, 1–2px rules, 0.05 grain under the room grade, biro wash, ink-sprite rings not pills) | **Spec issued** · DD 2026-07-31; reference implementation held in the DD seat — hand over, do not re-derive | DD ruling S12 |
| S15 | Lost-ticket treatment in Old Slips | **Ruled — violation** · DD 2026-07-31: LOST struck in oxide, row to `--toner-3`, returned figure `$0` in toner — not oxide, not wax | DD ruling S13 |
| S16 | App naming | **Closed** · DD 2026-07-31 — one name: **LEDGER**. "Old Slips" retired from copy (code identifier only); "SURETHING LEDGER" deleted (brands a machine-level app, drifts toward institutional hardware). S9 unblocked | DD ruling S14 |

## Room

| # | Item | State | Spec |
|---|---|---|---|
| R1 | Direction B — "Vice Grip", stylised, Palette 1 | Approved · Allen 2026-07-28 | `[RM] docs/room-visual-pass/SIGNOFF.md` |
| R2 | Two-bunk layout, riveted institutional TV housing, persistent `RoomArtRoot` | Approved · Implemented (8/8 gates) | `[RM] .../SIGNOFF.md` |
| R3 | Unified room/TV grade — one image, not two assets | Approved · Implemented room-side | `[TV] docs/tv-sweat-refinement/unified-grade-spec.md` |
| R4 | Cool-blue + money-colour palette laws | **REVOKED** · Allen 2026-07-25 | `DECISIONS.md`; four repo docs still assert them |
| R5 | Refinement A — full PBR material response (normal/smoothness/AO) | **Design-verified** · DD 2026-07-31 — the studio's first | `cd62855` |
| R6 | Refinement C — indirect light via Adaptive Probe Volumes (relief ×4–5 right wall) | **Design-verified** · DD 2026-07-31 | `[RM] .../PHASE_B_INDIRECT_LIGHT.md`, `fb44ac2` |
| R7 | Refinement B — localised wear, decals, contact grime | **Parked** · Allen 2026-07-31 at the committed Tier 1b state. DD 2026-07-31: FluorescentSoot **dropped** (the ceiling already reads); Decal Renderer Feature **not yet** — re-place against the frusta first if R7 resumes; the direction's read is the bar — re-review after R9/R10 | `[RM] handoff.md` §6B; Tier 1b commits |
| R8 | Refinement D — geometry detail, last priority | Approved (direction) · not started | `[RM] handoff.md` §6D |
| R9 | Ambient rebalance | **Closed — measured no-op** · 2026-07-31 (`b1d2ccc`): R6's bounce had already delivered the rebalance; lowering flat ambient moved nothing. 8/8 gates held, mattress 43.97. R10 unblocked early | `[RM] .../PHASE_B_INDIRECT_LIGHT.md` §7.1 |
| R10 | Couch-corner directional variation | **Closed** · 2026-07-31 (`72e0efb`) — bounce route tested and empirically failed (fills shadow, does not reveal surface); pre-authorised grazing fallback delivered **1.24×** via one value (CouchGraze 0.32→1.60, y=1.44 < 1.50 by construction). Mattress 43.97 unchanged; other regions ±0.07%. Couch 3.27% vs wall 9.85% — headroom exists, pushing further is a DD call | `[RM] .../PHASE_B_INDIRECT_LIGHT.md` §7.2 |
| R11 | All room art generated; nothing hand-authored | Approved (law) | `[RM] .../SIGNOFF.md` |
| R12 | Standing law — **surface detail is gated by lighting, not texture authoring** (promoted from R5's finding) | Law · DD 2026-07-31 | DD batch-2 |
| R13 | Room owning document — the surface's binding art authority under the two-tier system (C9) | **Approved** · Allen 2026-07-31 — canonical at `[M2] docs/design/room-design.md`, drafted by the DD; C7's four stale palette-law docs reconcile against it at integration | DD `room-design-2026-07-31-DRAFT.md` |

## TV — match theater

| # | Item | State | Spec |
|---|---|---|---|
| T1 | Visual direction — "maintained industrial equipment", concept render G | **Approved FINAL** · Allen 2026-07-27, seven rounds | `[TV] DESIGN.md` |
| T2 | Palette — concept C: cold white/grey facts, gold rationed to money/won/cash-out, muted blue+pink dots on pitch only | Approved · Allen 2026-07-27 | `[TV] DESIGN.md` §4 |
| T3 | `design/08-art-direction.md` — casino neon on black, CRT scanlines, green/red/gold purity | **Deprecated anti-reference** · Allen 2026-07-24 | `[TV] DESIGN.md` header |
| T4 | Held cash-out preview | Approved · Allen 2026-07-26 | `[TV] docs/.../PRD.md` §8.10 |
| T5 | Layout + five-zone stability (PRD "Decision A") | Settled · Allen 2026-07-31 — latest document governs (`DESIGN.md` §6); TV lead amends PRD §13/§14 | see C1 |
| T6 | Scene grammar — Phase 2A–2E: planner, 48-cell matrix, corner/booking, 6 near-miss endings, 5 buildup grammars, chance shapes + goal reactions | **Design-approved (structure)** · DD 2026-07-31; Design-verified **withheld** pending rendered captures (`[TV] .../visuals/` to the DD) — variation must read at four metres. Doc requirement: mark the non-renderable rebound→direct-strike cell in the authored inventory | `220c5ec` and predecessors |
| T7 | Phase 3 — UI refinement to the approved hierarchy | Not started · gated on T5 | `[TV] docs/.../PRD.md` §5 |
| T8 | Scanline overlay + `DeadLegBeat` static-noise crawl in `TvSweatScreen.cs` | Resolved · removed and verified `842382d` (2026-07-31); dead-leg beat timing preserved | `[TV] DESIGN.md` §9A.1 |
| T9 | `chromeCyan` used broadly for leg/clock/records/chrome labels — retired hue, no role in §4 | Debt · Phase 3 | `[TV] DESIGN.md` §9A.2 |
| T10 | Two hardcoded emission rest values, one darker than the agreed black floor | Debt · Phase 3 | `[TV] DESIGN.md` §9A.4 |
| T11 | TV production typeface | **Closed** · DD 2026-07-31 — **Encode Sans + Encode Sans Condensed** (SIL OFL 1.1). Tabular figures measured, not assumed — Saira disqualified (no `tnum`) despite the best character; deepest width axis of any qualifying free family, so one face covers ticket column and scoreline; an engineered screen face against Archivo's text face keeps "one hand, different jobs" explicit | DD batch-3; `tokens/fonts.css`, `type-tv.card.html` |
| T12 | Brightness values + pixel pitch | Provisional until seen on the real TV at seated distance | `[TV] DESIGN.md` §10 |
| T13 | Bunkmate character | Deferred out of worktree · Allen 2026-07-27 | `[TV] docs/.../PRD.md` |
| T14 | No camera shake/cut/zoom; fixed top-down framing | Approved (Decision B) | `[TV] docs/.../PRD.md` §13 |
| T15 | Slip-strip raw-hex markup in `UpdateSlipStrip` (retired green/red + dead chromeCyan as rich-text) | **Ruled — violation, T8 class** · DD 2026-07-31: remediate as T8; extend the palette scan to markup, not just serialised colour fields — that blind spot is why it survived. Remediated; scan then caught the same class in `[ST] SportsbookApp.cs` (routed) | `[TV] DESIGN.md` §9A.5 |
| T16 | Layout B occupants — **momentum tape IN** (PRD §4.2 names it in the one-revealed-source-of-truth law; a construction call can't narrow that), restored at the scorebug foot: no numerals, no hue, never above L2. **Win-probability numeral OUT** (§7 bans duplicating it; locked odds make the read the player's job). **3C commit unblocks once the tape is restored** | Ruled · DD 2026-07-31 | DD batch-2 |
| T17 | Scorer-gap severity | **Ruled** · DD 2026-07-31 — NOT an acceptable quiet win; a correctness defect that **ranks above every Phase 3 visual refinement**. Design instruction: *reserve, don't spend* — a scorer leg claims its backed-side goal before ordinary beats spend the quota; if binding is genuinely impossible, stage the reveal, never suppress the win, never synthesise one after resolution. Acceptance: every settled anytime-scorer leg traceable to a staged reveal that preceded or coincided — asserted as a test | DD batch-3 |
| T18 | Standing law — **compose, don't multiply**: variety adds a value to a dimension, never a cell to a cross-product | Law · DD 2026-07-31 | DD batch-3 |
| T19 | Standing law — **rendered distinctness, not key distinctness**: variation claims are made against rendered frames at the review distance, or not made. Signature diversity may never again be cited as evidence variation reads | Law · DD 2026-07-31 | DD batch-3 |
| T20 | Ticket-column px scale re-derived for the corrected 26–28% column — NEED 28px unchanged; live progress 23→**19px**; resolved/pending rows 19→**15px** ("live rows are display, resolved rows are index"). Authored strings do not bend to stale measurements | Ruled · DD 2026-07-31 | DD batch-3; `TvLegRow.jsx` |

## Cross-surface

| # | Item | State | Spec |
|---|---|---|---|
| C1 | **TV Decision A status** — ruled: latest document governs, so `DESIGN.md` §6 stands and the layout is closed. Phase 3 gate lifted; PRD §13/§14 to be amended. | Ruled · Allen 2026-07-31 | T5 |
| C2 | **TV light spill colour into the room** — interim ruling: shipped green tolerated for now; target remains `[TV] DESIGN.md` §5 cold white-grey, corrected in TV Phase 3. `DECISIONS.md` 2026-07-25 blue/magenta lock superseded. Merge does not auto-resolve this. | Interim · Allen 2026-07-31 | `[RM] docs/6-memo/2026-07-27-room-to-tv-sweat.md` |
| C3 | **TV canvas HDR coverage** — ruled: coverage OVER-enforces. §3's four L4 occupants and §7's ball are eligible but only two carry the shader — add score-at-goal and ball-at-payoff. Live-leg pulse stays out (scarcity is what makes L4 mean *now*). Eligibility ≠ simultaneity: explicit one-token invariant required; arbitration — momentary punch preempts sustained state. Boost stays 1.8, one value. **3D unblocked.** | Ruled · DD 2026-07-31 | DD batch-2 |
| C4 | **Money colour is now per-surface** — TV gold, SureThing wax amber, green/red retired game-wide. Coherence is a choice, not a constraint (Allen 2026-07-28). | Approved | `[ST] .../DIRECTIONS.md` |
| C5 | **Room re-tint from TV light in-engine** — if the rig supports it, big payoffs drive it | Open, deliberately | `[TV] DESIGN.md` §10 |
| C6 | **Stale colour law in TV PRD §14.1** — carries the deprecated `08` green/red/gold language as binding on the brand book | Documentation conflict · could misdirect Phase 3 | `[TV] docs/.../PRD.md` §14.1 vs `[TV] DESIGN.md` header |
| C7 | **Four repo documents still assert the revoked room palette laws** | Documentation debt | `[RM] .../SIGNOFF.md` |
| C8 | Bloom floor — "chrome may degrade" confirmed, amended: **risk/pays joins the protected set** (§12 makes it question 4; failures 1–5 are major) | Ruled · DD 2026-07-31 | DD batch-2 |
| C9 | **Art authority — two-tier approved:** a thin studio constitution plus one owning document per surface (one document across four registers is what killed `08`). DD drafts the room's owning doc; phone stays a stub | Approved · Allen 2026-07-31 | DD `proposal-art-authority-2026-07-31.md` (DD seat — export pending) |

---

## Working notes

- Item states move only on evidence: *Implemented* needs a commit; *Design-verified* needs a review
  note from this seat against the item's spec.
- 2026-07-31: C1 ruled (latest document governs — `DESIGN.md`), C2 given an interim ruling
  (shipped green tolerated; cold white-grey target lands with TV Phase 3). T8 ruled and
  resolved: both effects removed, verified `842382d`. No divergence items remain open.
