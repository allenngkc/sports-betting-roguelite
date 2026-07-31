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
| S11 | Production typeface | **Ruled** · Allen 2026-07-31 — no licence-encumbered type in the product; Bell Centennial dropped. Free-licence replacement specced by the Design Director with the identity work | `[ST] handoff.md` §6 |
| S12 | Rejected comparison — "The Catalogue Sleeve" | Exploration (closed) | `[ST] .../direction-2-catalogue-sleeve.html` |
| S13 | Earlier explorations — Tote Hall / Broadcast Alert / Night Board | Dumped · Allen 2026-07-28 | `[ST] .../DIRECTIONS.md`; do not revive |

## Room

| # | Item | State | Spec |
|---|---|---|---|
| R1 | Direction B — "Vice Grip", stylised, Palette 1 | Approved · Allen 2026-07-28 | `[RM] docs/room-visual-pass/SIGNOFF.md` |
| R2 | Two-bunk layout, riveted institutional TV housing, persistent `RoomArtRoot` | Approved · Implemented (8/8 gates) | `[RM] .../SIGNOFF.md` |
| R3 | Unified room/TV grade — one image, not two assets | Approved · Implemented room-side | `[TV] docs/tv-sweat-refinement/unified-grade-spec.md` |
| R4 | Cool-blue + money-colour palette laws | **REVOKED** · Allen 2026-07-25 | `DECISIONS.md`; four repo docs still assert them |
| R5 | Refinement A — full PBR material response (normal/smoothness/AO) | Implemented · review pending | `cd62855` |
| R6 | Refinement C — indirect light via Adaptive Probe Volumes (relief ×4–5 right wall) | Implemented · review pending | `[RM] .../PHASE_B_INDIRECT_LIGHT.md`, `fb44ac2` |
| R7 | Refinement B — localised wear, decals, contact grime | Approved (direction) · not started | `[RM] handoff.md` §6B |
| R8 | Refinement D — geometry detail, last priority | Approved (direction) · not started | `[RM] handoff.md` §6D |
| R9 | Ambient rebalance — lower flat ambient to let bounce carry relief | Candidate · needs 8/8 gate re-run | `[RM] .../PHASE_B_INDIRECT_LIGHT.md` §7.1 |
| R10 | Couch-corner grazing source — strongest normal map still reads at 2.3% | Candidate | `[RM] .../PHASE_B_INDIRECT_LIGHT.md` §7.2 |
| R11 | All room art generated; nothing hand-authored | Approved (law) | `[RM] .../SIGNOFF.md` |

## TV — match theater

| # | Item | State | Spec |
|---|---|---|---|
| T1 | Visual direction — "maintained industrial equipment", concept render G | **Approved FINAL** · Allen 2026-07-27, seven rounds | `[TV] DESIGN.md` |
| T2 | Palette — concept C: cold white/grey facts, gold rationed to money/won/cash-out, muted blue+pink dots on pitch only | Approved · Allen 2026-07-27 | `[TV] DESIGN.md` §4 |
| T3 | `design/08-art-direction.md` — casino neon on black, CRT scanlines, green/red/gold purity | **Deprecated anti-reference** · Allen 2026-07-24 | `[TV] DESIGN.md` header |
| T4 | Held cash-out preview | Approved · Allen 2026-07-26 | `[TV] docs/.../PRD.md` §8.10 |
| T5 | Layout + five-zone stability (PRD "Decision A") | Settled · Allen 2026-07-31 — latest document governs (`DESIGN.md` §6); TV lead amends PRD §13/§14 | see C1 |
| T6 | Scene grammar — Phase 2A–2E: planner, 48-cell matrix, corner/booking, 6 near-miss endings, 5 buildup grammars, chance shapes + goal reactions | Implemented · review pending | `220c5ec` and predecessors |
| T7 | Phase 3 — UI refinement to the approved hierarchy | Not started · gated on T5 | `[TV] docs/.../PRD.md` §5 |
| T8 | Scanline overlay + `DeadLegBeat` static-noise crawl in `TvSweatScreen.cs` | Resolved · removed and verified `842382d` (2026-07-31); dead-leg beat timing preserved | `[TV] DESIGN.md` §9A.1 |
| T9 | `chromeCyan` used broadly for leg/clock/records/chrome labels — retired hue, no role in §4 | Debt · Phase 3 | `[TV] DESIGN.md` §9A.2 |
| T10 | Two hardcoded emission rest values, one darker than the agreed black floor | Debt · Phase 3 | `[TV] DESIGN.md` §9A.4 |
| T11 | TV typeface — characteristics specified, file unchosen | Open | `[TV] DESIGN.md` §10 |
| T12 | Brightness values + pixel pitch | Provisional until seen on the real TV at seated distance | `[TV] DESIGN.md` §10 |
| T13 | Bunkmate character | Deferred out of worktree · Allen 2026-07-27 | `[TV] docs/.../PRD.md` |
| T14 | No camera shake/cut/zoom; fixed top-down framing | Approved (Decision B) | `[TV] docs/.../PRD.md` §13 |

## Cross-surface

| # | Item | State | Spec |
|---|---|---|---|
| C1 | **TV Decision A status** — ruled: latest document governs, so `DESIGN.md` §6 stands and the layout is closed. Phase 3 gate lifted; PRD §13/§14 to be amended. | Ruled · Allen 2026-07-31 | T5 |
| C2 | **TV light spill colour into the room** — interim ruling: shipped green tolerated for now; target remains `[TV] DESIGN.md` §5 cold white-grey, corrected in TV Phase 3. `DECISIONS.md` 2026-07-25 blue/magenta lock superseded. Merge does not auto-resolve this. | Interim · Allen 2026-07-31 | `[RM] docs/6-memo/2026-07-27-room-to-tv-sweat.md` |
| C3 | **TV canvas HDR** — capability fixed `1aa74c3` (2026-07-28): unclamped material float in `TvSweatHdrUI.shader` bypasses the UGUI `Color32` clamp. Remaining: coverage — which elements may exceed 1.0 under the one-full-brightness rule. Room unblocked. | Coverage → Design Director · 2026-07-31 | `[TV] docs/tv-sweat-refinement/c3-hdr-canvas-proposal.md` |
| C4 | **Money colour is now per-surface** — TV gold, SureThing wax amber, green/red retired game-wide. Coherence is a choice, not a constraint (Allen 2026-07-28). | Approved | `[ST] .../DIRECTIONS.md` |
| C5 | **Room re-tint from TV light in-engine** — if the rig supports it, big payoffs drive it | Open, deliberately | `[TV] DESIGN.md` §10 |
| C6 | **Stale colour law in TV PRD §14.1** — carries the deprecated `08` green/red/gold language as binding on the brand book | Documentation conflict · could misdirect Phase 3 | `[TV] docs/.../PRD.md` §14.1 vs `[TV] DESIGN.md` header |
| C7 | **Four repo documents still assert the revoked room palette laws** | Documentation debt | `[RM] .../SIGNOFF.md` |

---

## Working notes

- Item states move only on evidence: *Implemented* needs a commit; *Design-verified* needs a review
  note from this seat against the item's spec.
- 2026-07-31: C1 ruled (latest document governs — `DESIGN.md`), C2 given an interim ruling
  (shipped green tolerated; cold white-grey target lands with TV Phase 3). T8 ruled and
  resolved: both effects removed, verified `842382d`. No divergence items remain open.
