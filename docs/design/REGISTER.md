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
| S6 | Lobby shell — annotated form guide | **Design-verified** · DD 2026-08-01, against eight flats + the angled in-room render — the laptop's first | merged `2e97d13` |
| S7 | Ink sprites — deterministic biro rings + strike, variant by matchup index | **Design-verified** · DD 2026-08-01 (same review) | merged `2e97d13` |
| S8 | OS chrome — one NotebookChrome, personal not institutional | **Design-verified** · DD 2026-08-01 (same review): two inks, no cards, ring-not-pill selection, chrome reads as his machine | merged `2e97d13` |
| S9 | Event detail, staged ticket, MY BETS, rewards, old slips | Approved (spec) · not implemented | `[ST] .../DESIGN.md` §Components |
| S10 | Sweat "loud register" for the laptop | Candidate | direction contract comment only; no built spec |
| S11 | Production typeface | **Closed** · DD 2026-07-31 — **Archivo + Archivo Narrow** (OFL 1.1). Chosen for the function Bell's ink traps served (small type surviving a degraded surface); true superfamily, shared metrics, tabular figures both widths. One `LoadFont` + two TMP assets | DD ruling S11-A |
| S12 | Rejected comparison — "The Catalogue Sleeve" | Exploration (closed) | `[ST] .../direction-2-catalogue-sleeve.html` |
| S13 | Earlier explorations — Tote Hall / Broadcast Alert / Night Board | Dumped · Allen 2026-07-28 | `[ST] .../DIRECTIONS.md`; do not revive |
| S14 | Form-guide identity — three build gaps, all already specified: two-voice type system unwired (condensed figures vs roman labels); 78px entry geometry drifted (pre-contract 660px board suspected); document layer missing (warm olive-black ground, warm bone toner, 1–2px rules, 0.05 grain under the room grade, biro wash, ink-sprite rings not pills) | **Spec issued** · DD 2026-07-31; reference implementation held in the DD seat — hand over, do not re-derive | DD ruling S12 |
| S15 | Lost-ticket treatment in Old Slips | **Ruled — violation** · DD 2026-07-31: LOST struck in oxide, row to `--toner-3`, returned figure `$0` in toner — not oxide, not wax | DD ruling S13 |
| S16 | App naming | **Closed** · DD 2026-07-31 — one name: **LEDGER**. "Old Slips" retired from copy (code identifier only); "SURETHING LEDGER" deleted (brands a machine-level app, drifts toward institutional hardware). S9 unblocked | DD ruling S14 |
| S17 | Offer descriptions | **Ruled** · Allen 2026-07-31 — an offer's rule text (especially cost/downside clauses) is never truncated; show fewer offers instead. Truncation that drops the rule is misleading at the point of spending | lead report + Allen draft |
| S18 | "LEAVE — NEXT ROUND" in biro | **Ruled — violation, S15 class** · DD 2026-07-31: a primary action is wax (field, `--wax-ink` type, 2px `--wax-deep` edge); biro is only what he chose. Rewards price is wax; its blocked reason stays oxide | DD batch-4 |
| S19 | Toner grain | **Ruled** · DD 2026-07-31: overlay-blend UI shader, scoped task, last element of S14's document layer, not blocking S9. Disabling the lightening-only implementation was correct; no reduced-opacity version ships (→ C10) | DD batch-4 |
| S20 | Weight channel | **Ruled** · DD 2026-07-31: variable-weight unaddressable in UGUI Text is a constraint, not debt — no weight tier on SureThing without moving the element to TMP named instances. **S2 amended:** a text box is at least one line of its production face tall; too-short boxes overflow, never render empty | DD batch-4 |
| S21 | S6/S7/S8 review | Held 07-31 (captures didn't travel) → superseded by the 08-01 verdict on S6/S7/S8 | DD batch-4 + addendum |
| S22 | Market label vocabulary (D-01) | **Ruled** · DD 2026-07-31: engine emits fields, surface composes; `DisplayLabel` becomes a per-surface composer; DS vocabulary verbatim; role is a first-class field printed as a word, not a bracketed tag | DD batch-4 |
| S23 | RIDING (D-02) | **Ruled** · DD 2026-07-31: RIDING kept; DS enums amended to `PENDING | RIDING | LIVE | GREEN | DEAD | VOID | CASHED OUT`. RIDING is ticket-level only, LIVE leg-level only — contractual split | DD batch-4 |
| S24 | One-sided scorer market (D-03) | **Ruled** · DD 2026-07-31: paired pricing describes two-outcome markets; scorer renders as a single-column offer list, never a paired row with a dead cell; no disabled state; replacement applies between rows | DD batch-4 |
| S25 | PLAYERS tab overflow (D-04) | **Ruled** · DD 2026-07-31: S17 binds it — fixed body + printed `N NOT SHOWN` at the fact floor; scroll only with a visible position indicator. General clause: a container's correctness may not depend on a config dial's current value — guard with a test. D-05 clean, no action | DD batch-4 |
| S26 | REWARDS findings | **Ruled — violations** · DD 2026-08-01: offer rule text truncates at the point of spending (S17 class — show fewer offers); the REWARDS-OPEN banner is drawn in biro over the rows — it is the product speaking: toner, in its own space, re-worded to state rather than exhort | DD addendum |

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
| R9 | Ambient rebalance | **Closed — measured no-op** · 2026-07-31 (`b1d2ccc`); **Design-verified** · DD 2026-08-01 against the 04→01 pair (warm, not the cool-blue failure mode) | `[RM] .../PHASE_B_INDIRECT_LIGHT.md` §7.1 |
| R10 | Couch-corner directional variation | **Closed** · 2026-07-31 (`72e0efb`) — bounce route tested and empirically failed (fills shadow, does not reveal surface); pre-authorised grazing fallback delivered **1.24×** via one value (CouchGraze 0.32→1.60, y=1.44 < 1.50 by construction). Mattress 43.97 unchanged; other regions ±0.07%. Couch 3.27% vs wall 9.85% — headroom exists, pushing further is a DD call | `[RM] .../PHASE_B_INDIRECT_LIGHT.md` §7.2 |
| R11 | All room art generated; nothing hand-authored | Approved (law) | `[RM] .../SIGNOFF.md` |
| R12 | Standing law — **surface detail is gated by lighting, not texture authoring** (promoted from R5's finding) | Law · DD 2026-07-31 | DD batch-2 |
| R13 | Room owning document — the surface's binding art authority under the two-tier system (C9) | **Approved** · Allen 2026-07-31 — canonical at `[M2] docs/design/room-design.md`, drafted by the DD; C7's four stale palette-law docs reconcile against it at integration | DD `room-design-2026-07-31-DRAFT.md` |
| R14 | Couch headroom | **Ruled — leave at 1.24×** · DD 2026-07-31: headroom unspent; the TV is the dominant light at that corner and ships a C2 placeholder. Re-open only after cold white-grey lands, and only if the couch still reads as a mass. `CouchGraze` stays 1.60 | DD batch-4 |
| R15 | Room re-review | **Ruled — slice closes** · DD 2026-07-31: the room's read is the bar. R7 parked, R8 unstarted, decals not-yet; floor accepted as a quiet surface under amended R12; R9's finding recorded (no flat fill remains — deeper shadow is a grade or per-light ask). **Design-verified** · DD 2026-08-01 against the 04→01 pair | DD batch-4 + addendum |

## TV — match theater

| # | Item | State | Spec |
|---|---|---|---|
| T1 | Visual direction — "maintained industrial equipment", concept render G | **Approved FINAL** · Allen 2026-07-27, seven rounds | `[TV] DESIGN.md` |
| T2 | Palette — concept C: cold white/grey facts, gold rationed to money/won/cash-out, muted blue+pink dots on pitch only | Approved · Allen 2026-07-27 | `[TV] DESIGN.md` §4 |
| T3 | `design/08-art-direction.md` — casino neon on black, CRT scanlines, green/red/gold purity | **Deprecated anti-reference** · Allen 2026-07-24 | `[TV] DESIGN.md` header |
| T4 | Held cash-out preview | Approved · Allen 2026-07-26 | `[TV] docs/.../PRD.md` §8.10 |
| T5 | Layout + five-zone stability (PRD "Decision A") | Settled · Allen 2026-07-31 — latest document governs (`DESIGN.md` §6); TV lead amends PRD §13/§14 | see C1 |
| T6 | Scene grammar — Phase 2A–2E: planner, 48-cell matrix, corner/booking, 6 near-miss endings, 5 buildup grammars, chance shapes + goal reactions | **Design-verified · GRANTED · CLOSED** · DD 2026-08-02 on the postC14 Set B: variation reads as variation at review scale (cluster shape, ball track, final-third occupancy differ across all six grammars); T18's compose-don't-multiply delivered; T19's risk retired for Phase 2. (Earlier: structure approved 07-31; visual half withheld, then refused on the manifest-less set — see T26) | `register-frame-review-2026-08-02.md` |
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
| T17 | Scorer-gap severity | **Ruled and RESOLVED** · fix committed `ea28c9b` (2026-07-31): goal reserved upstream in beat-spending, red test inverted, non-scorer paths bit-for-bit unchanged (verified by mechanism). Presentation consequence (backed side reads one goal short until the final) queued for DD eyeball | DD batch-3; `ea28c9b` |
| T18 | Standing law — **compose, don't multiply**: variety adds a value to a dimension, never a cell to a cross-product | Law · DD 2026-07-31 | DD batch-3 |
| T19 | Standing law — **rendered distinctness, not key distinctness**: variation claims are made against rendered frames at the review distance, or not made. Signature diversity may never again be cited as evidence variation reads | Law · DD 2026-07-31 | DD batch-3 |
| T20 | Ticket-column px scale re-derived for the corrected 26–28% column — NEED 28px unchanged; live progress 23→**19px**; resolved/pending rows 19→**15px** ("live rows are display, resolved rows are index"). Authored strings do not bend to stale measurements | Ruled · DD 2026-07-31 | DD batch-3; `TvLegRow.jsx` |
| T21 | §8.8 stats panel | **Ruled — drop** · DD 2026-07-31: two unsourceable rows removed; panel ships at authored height, no reserved space; a row returns only when the sim emits it as a first-class value, never computed in presentation | DD batch-4 |
| T22 | §8.10 confirm gesture | **Ruled** · DD 2026-07-31: hold to preview, release always abandons, release is never confirm — commit is an act on the laptop. Phase-3 fallback: a second key during the hold; no timer, no auto-commit. `[E]` retired from copy; the slot prints `HOLD E` | DD batch-4 |
| T23 | §7.7 backed-player locator treatment | **Ruled** · DD 2026-07-31: numeral OUT (sub-floor by construction; no glyph vocabulary). Detached 2px `--tv-fact` ring at the dot, hue unchanged, L3, held while the scorer leg is live, removed on the resolving frame. Never the L4 token; no pulse | DD batch-4 |
| T24 | TV row model | **Ruled** · DD 2026-07-31: **fixed rows stand; canon amends to Unity.** NEED one line; every leg slot authored at the live row's measured height (76px), reserved always — the slot was wrong, not the row. Live row carries no meta line: specified, not tolerated. Over-long NEED re-authored against a call-site-recorded measurement; never wrapped, never truncated. Closes inbox 9 + 9a | DD batch-4 |
| T25 | Seated-sweat review (Set A, `4597b60`) | Six findings · DD 2026-07-31. **25.1 RESOLVED** (`4e4585a`, 2026-08-01): the glass clips, nothing is built outside it — containment verified across five seeds (seed 01 complete, 02–05 partial on the harness deadline; frames genuine). 25.2–25.5 violations, 25.6 defect, 25.7 minor remain in TV's queue. Passed: gold rationing, score dominance, word-carried state, T20 scale | DD batch-4; `4e4585a`, `d6d4238` |
| T26 | T6 visual half | Withheld (07-31, Set B absent) → **Design-verified REFUSED** · DD 2026-08-01: composition identical across the set; grammars differ only by an event-strip sentence. Probable cause T25.1 — fix, re-capture, re-submit (five seeds, real manifest: scene index, grammar labels, named face); refusal expected to invert. Structural approval untouched. **Inverted 2026-08-02** — Design-verified granted on the postC14 Set B, recorded on the T6 line | DD batch-4 + addendum; `register-frame-review-2026-08-02.md` |
| T27 | TV idle copy | **Ruled — violation** · DD 2026-08-01: "PLACE YOUR BETS" is celebratory exhortation in a retired hue at L4 — banned on both counts. Idle prints `ROUND n OF 8 · BOARD OPEN` in `--tv-fact`; the bar carries no hue; the TV never instructs the player to bet | DD addendum |
| T41 | Multiple simultaneous L4 occupants | **C3 VIOLATION — BLOCKS TV Phase 3+** · DD 2026-08-02 (issued as "T22", renumbered): pure `#ffffff`/1.000 in the pitch + near-white 0.923 scorebug coexist in 3 of 4 sampled frames; actionable cash-out (the surface's only L4) measures 0.671 — third-brightest on its own surface. Fix: cap the stage under the ladder (§7 — ball L4 only at a payoff); gold wins by construction. Recorded, not acted on: shipped gold `#ffd12e` vs token `#F2BC45` — token is the intent, hex stays open pending palette ratification | `register-frame-review-2026-08-02.md` |
| T42 | Team hues in scorebug type, saturated | **§4 violation** · DD 2026-08-02 (issued as "T23"): team names at luminance 0.87–0.92, full chroma; hues must be muted, brightness-secondary, confined to pitch dots. Desaturate, cold-white names, hue for dots only; if sides inseparable at four metres the fix is form (filled vs hollow dot), never louder colour | `register-frame-review-2026-08-02.md` |
| T43 | `MARKET SUSPENDED` on full-brightness gold | **State lie** · DD 2026-08-02 (issued as "T24"): suspended renders on solid gold before dimming a frame later; dim state (`#484e54`) exists and works — transition ordering bug. Suspended is L1 unlit slate from its first frame; dim lands on the same frame as the label change | `register-frame-review-2026-08-02.md` |
| T44 | Event-strip copy drift | **Voice violation** · DD 2026-08-02 (issued as "T25"): "off the bar - a miracle brewing?!" — banned register (hype, `?!`). Audit authored lines against CONTENT FUNDAMENTALS; strip exclamations/superlatives/promises; trim "the crowd loses it"; normalise hyphen → em dash | `register-frame-review-2026-08-02.md` |
| T45 | Death re-tint drains room to navy | **Law 1.1 failure mode** · DD 2026-08-02 (issued as "T26"): `#0e121d` on LegFinalLost frames. Mechanism endorsed (C5 landing); colour forbidden. Keep the re-tint, retarget the drain to the room's darkest olive `#0F1108`. **SUBSUMED by T48** (batch 6, same day): navy-on-death is the same defect one luminance level down — do not work it; re-measure after T48 lands | `register-frame-review-2026-08-02.md`; batch 6 |
| T47 | Markets working-margin collision | **Ruled** · DD 2026-08-02 batch 6: **bound the flow region; the action stack stays anchored** (an un-anchored stack moves LOCK IT IN with leg count — the most consequential control in the game). MaxLegs=4 makes the reserved height computable — reserve it and the bands can never meet. Separate defect: LockReason renders above the Lock control, not inside it per `LockAction` — put it back and 14 of the 36px vanish; the reason must never sit on the payout. Third instance of "landing a cap ≠ landing the layout the cap implies" (T20 shape). Closes INBOX Need-Allen #4; unblocks markets B1 | `register-batch6.md` |
| T48 | Unified grade — §1.1 joint question | **Ruled — Option A** · DD 2026-08-02 batch 6, TV seat's agreement given: neutral black point at the same value; keep the lift. `#0a0c10` was one number doing two jobs (luminance floor + inherited TV-substrate hue) — a panel may be cool, plaster may not. B/C/D rejected. Subsumes T45. Requirement: **re-shoot the TV set screens-dark AND grade-bypassed**, or a shared-grade conclusion is T19 in a new colour | `register-batch6.md` |
| T49 | Bloom A/B (1.8 vs 1.4) | **Withheld — experiment confounded** · DD 2026-08-02 batch 6: pitch at `#ffffff`/1.000 with threshold ~0.9 → both arms bloom maximally, differ only below threshold. Re-run after T41 caps the stage | `register-batch6.md` |
| T50 | Encode Sans in situ | **Confirmed** · DD 2026-08-02 batch 6: T11 now stands on rendered evidence; column type items blocked by T46 | `register-batch6.md` |
| T51 | TV-15 — stacked label misses by 0.3px | **Ruled — the 0.3px yields, canon holds** · DD 2026-08-02 batch 6: never reorder an information hierarchy for three tenths of a pixel; re-deriving a grid constant once at design time is legal (§6 forbids runtime resize). Stacked label-above-value stays | `register-batch6.md` |
| T52 | TV-02 — tape shape | **T28 confirmed, one 28px strip** · DD 2026-08-02 batch 6: momentum is match-scoped; per-leg rows would add N competitors to the exact ladder pressure that produced T41 | `register-batch6.md` |
| T53 | Room collider count | **Ruled: 29** · DD 2026-08-02 batch 6 — the DD's own doc ("27") was the more wrong side: stale, never measured. Keep the two Interactable, remove the two stray. Tooling worse than the count: harness counts only BoxCollider — "27 PASS" was blind to four objects, third vacuous green gate this fortnight (T47's epsilon, T19's signature gate). **Standing instruction, studio-wide: every gate states what it cannot see** | `register-batch6.md` |
| T54 | Room Gate 8 | **Void** · DD 2026-08-02 batch 6: certified geometry that didn't exist yet — R9/R10 must not report 8/8 | `register-batch6.md` |
| T55 | Shared body material | **Ruled — violation, outranks all room polish** · DD 2026-08-02 batch 6: shared material collapses the two-register split | `register-batch6.md` |
| T56 | Drab green missing | **Ruled — room wrong** · DD 2026-08-02 batch 6: sequence AFTER T48; hold the 43.9 mattress value | `register-batch6.md` |
| T57 | Unit-primitive scaling | **Ruled** · DD 2026-08-02 batch 6: same five objects as T53 | `register-batch6.md` |
| T46 | Stage overdraws ticket column | **Layout defect, T21 class** · DD 2026-08-02 (issued as "T27"): scoreline/pitch painted over leg text (struck-through identities, `BIFF RACKET TO SCORE` cut mid-word). §6: fixed grid, no content-sized zones. Ticket column owns its width absolutely; stage clips to its region; assert per-frame edges | `register-frame-review-2026-08-02.md` |

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
| C8 | Bloom floor — risk/pays in the protected set; **amended** · DD 2026-07-31 against frames: the floor is measured on rendered frames at seated distance, never asserted from a boost value. Boost 1.8 holds provisionally pending the 1.8/1.4 A/B at the next capture window | DD batch-2 + batch-4 |
| C9 | **Art authority — two-tier approved:** a thin studio constitution plus one owning document per surface (one document across four registers is what killed `08`). DD drafts the room's owning doc; phone stays a stub | Approved · Allen 2026-07-31 | DD `proposal-art-authority-2026-07-31.md` |
| C10 | Standing law — **wrong in kind is not fixed by opacity**: an effect that fails on mechanism is disabled and re-scoped, never tuned toward invisibility (promoted from S19) | Law · DD 2026-07-31 | DD batch-4 |
| C11 | Standing law — **rendered evidence or no claim**: every design claim about how something reads, including Design-verified, is made against rendered frames at review distance, on every surface (T19 is this law's TV instance). A review package is its document plus its frames; a package without its frames is not in review | Law · DD 2026-07-31 | DD batch-4 |
| C12 | Review-evidence transport | **Ruled** · DD 2026-07-31: design review requires frames in the import, not in git; bundles are the correct vehicle; the interim binaries policy stands. The LFS question blocks no design item | DD batch-4 |
| C13 | Stale screen content in the room scene | **Ruled — violation** · DD 2026-08-01: the room renders the superseded violet laptop package and green TV content despite both surfaces having merged. **Integration item (orchestrator), not a room defect**; R15 unaffected. No room capture is evidence for either screen until re-taken | DD addendum |
| C14 | Fidelity standard | **Directive, hardened** · Allen 2026-08-01: all work is exceptional quality and a **1:1 match** to the intended designs — 1:1 is the bar, not the aspiration; deviations only where physically impossible, each DD-signed before build. (Original 07-31 form: as close as the platform allows.) Leads run Opus 5 at max effort per Allen | Allen, 07-31 + 08-01 |
| C15 | Type stack | **RULED — Option 1, TextMeshPro migration** · Allen 2026-08-02: both surfaces migrate to TMP (tracking, tabular figures, weight 600 become reachable). **Scheduled phase, not now** — sequenced after the current conformance wave (T41/T47/T48 land first); orchestrator schedules per surface. All signed type deviations stay in force until their surface migrates, then expire. Markets' ladder letter-spacing deviation dissolves at migration. Touches every slot, the HDR material path, C3's one-token invariant — leads scope in handoffs before their phase | Allen, 2026-08-02 |

---

## Working notes

- Item states move only on evidence: *Implemented* needs a commit; *Design-verified* needs a review
  note from this seat against the item's spec.
- 2026-07-31: C1 ruled (latest document governs — `DESIGN.md`), C2 given an interim ruling
  (shipped green tolerated; cold white-grey target lands with TV Phase 3). T8 ruled and
  resolved: both effects removed, verified `842382d`. No divergence items remain open.
