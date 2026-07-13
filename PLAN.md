# Plan: Economy Rework — debt payments, the 3+3 catalog, and the sim campaign

_Locked via grill — by Claude (Fable, game director) + Allen, 2026-07-12/13. Context docs:
design/09 (CloverPit research), design/10 (economy rework discussion). Scope: ENGINE + SIM +
CONSOLE ONLY — Unity keeps the old precompiled SBR.Engine.dll until a follow-up milestone
(the DLL boundary isolates it; do NOT recopy the DLL in this milestone)._

## Goal

Prove the math and the fun. Replace threshold targets with debt PAYMENTS (the income-rate race
that kills the soft mid-game), retire the 8-relic catalog, and ship a 6-item catalog (3 passives
+ 3 consumables) built for multiplicative stacking — then prove the whole economy against six
sim gates before any Unity work. Deliverables: reworked engine, upgraded /sim with new bots and
a gate table, playable console proof, sim-report-2.md.

## Locked design (every decision Allen-confirmed)

### 1. The payment model
- `Settle()`: `bank −= Payment[round]`. If `bank < Payment[round]`: Totem check (see below),
  else RUN OVER. Surplus carries. No floats — debt-as-HP is DELETED (RunConfig.DebtJuiceRate,
  Run.Debt/Requirement and all float paths removed; git history is the archive).
- Starting hypothesis: bank 500, payments ≈ [250, 425, 720, 1230, 2090, 3550, 6040, 10260]
  (~×1.7/round, 8 rounds). The sim GRID refines: growth ∈ {1.5, 1.6, 1.7, 1.8, 1.9} ×
  P₁ ∈ {200, 250, 300}; gates pick the cell.
- Fiction: the bookie is the CREDITOR (payments are settle-day with the book). Engine exposes
  payment telemetry (paid/shortfall/totem-fired per settle) for the later Unity/M5-phone remap.

### 2. The catalog (replaces all 8 old relics; old definitions retired, dead ops deleted)

PASSIVES (relic slots stay 5; only 3 passives exist v1):
- **The Multiplier** (static engine, $250): parlays of 3+ legs get PayoutMultiplier ×1.5.
  Full power at purchase; rewards the parlay exponential (o = Π legs).
- **Scar Tissue** (ratchet, $200): every busted ticket adds stacks; stack value scales with
  stake — `stackPp = 5.0 × min(1, stake / (0.25 × bankAtPlacement))` percentage points (a
  ≥25%-of-bank bust earns the full +5pp; a $10 token bust off $500 earns +0.4pp — the farming
  guard). Stacks are UNCAPPED, persist across rounds, render as a visible counter, and are
  CONSUMED by the next winning ticket (its PayoutMultiplier ×(1 + stacks/100), then reset to 0).
  Ratchets never unwind on their own.
- **Totem of Undying** ($300): purchasable ONCE per run, one charge. On an unpayable payment:
  the bookie covers the shortfall, the NEXT payment grows by shortfall × 1.5, totem consumed
  (the old float math, itemized). Does NOT trigger on the final payment (no next payment to
  surcharge — mirrors the old "no borrowing on the final round" rule); final-round shortfall is
  death, totem or not.
- Composition law: ALL payout effects multiply into the single `Ticket.PayoutMultiplier`
  product (Multiplier × Scar × any future feeder). Superadditivity is gate G5.

CONSUMABLES (2 consumable slots, separate from relic slots; sell-back 50% applies to both pools):
- **Mulligan Slip** ($100): play DURING the sweat when a leg has just gone dead (window: until
  the ticket settles) — that leg is voided, the ticket lives. Multi-leg tickets only. Reuses
  the mulligan void semantics (excluded from payout product and win condition).
- **Profit Boost** ($75): played at the betslip before lock — one chosen leg's odds ×1.3
  (compose-time base-odds rewrite, the boosted_odds op reshaped as single-use).
- **Timeout** ($75): played mid-sweat — the drama pauses and the cash-out offer HOLDS for the
  next 3 events (via the SweatSession.ApplyLiveEffect intervention seam, first real user).
  Sim proves non-degeneracy only (no guaranteed-profit pattern from held offers); its real
  value is decision quality → playtest-gated.
- Acquisition: the shop shows all unowned passives (≤3) plus 2 consumable offers per visit
  (deterministic draw, Shop RNG stream). BOOKIE GIFT channel: after 2 consecutive rounds of
  net-negative ticket P&L, next Betting phase grants a free consumable (deterministic pick,
  StableHash) if a slot is free — at most once per 2 rounds. (Console prints it as a bookie
  text; the phone renders it in the Unity follow-up.)

### 3. The gates (sim campaign acceptance — Allen-approved)
- **G1 honest gambling**: naive median death 3–4, win <1%.
- **G2 engine mandatory**: skilled NO-SHOP bot median death 5–6, win <2%.
- **G3 skilled wins**: skilled organic median death ≥7, win 10–15%.
- **G4 EV arc exists**: skilled organic mean per-ticket EV crosses zero in rounds 4–7.
- **G5 composition superadditive**: Δwin(Multiplier+Scar granted) > Δwin(Multiplier) + Δwin(Scar).
- **G6 martyr guard**: deliberate Scar-farming bot win ≤ organic skilled +2pp.
- Item flags (granted-free audit, all six): no DEAD (<+1pp win), no DOMINANT (>2× next best);
  Totem: Δmean rounds ≥ +0.3 AND trigger rate among buyers 25–60%.
- Report-only: swing p99/p50 ∈ 3–12×; top-1% final surplus ≥ 2× final payment with max ≫ p99;
  close-call rate (skilled deaths with bank within 20% of the missed payment) — watched.

### 4. Work plan
1. **Engine**: RunConfig (Targets→Payments; DebtJuiceRate gone; ConsumableSlots=2), Run
   (payment Settle, totem hook, telemetry), EffectEngine (product composition; Scar stack
   accounting incl. stake scaling + consumption; totem once-per-run purchase rule; consumable
   ops: void-on-demand, single-use compose boost, offer-hold), RelicCatalog (new 3+3 with
   consumable definitions distinct from relics), shop logic (all-passives + 2 consumables),
   gift trigger. Old ops/relics deleted.
2. **Engine tests** (rewrite the economy suites): payment settle paths (pay/surplus/short/
   totem/final-round-no-totem), Scar math (stake scaling, uncapped growth, consumption reset,
   product composition with Multiplier), consumable semantics (Mulligan window + multi-leg
   rule, Boost compose, Timeout hold — offer identical for 3 events), gift trigger cadence,
   sell-back, determinism (fixed universe untouched by all new items — purity tests).
3. **Sim**: new bots (no-shop skilled; martyr farmer; organic skilled taught consumable use —
   Mulligan on first dead leg of a live multi-leg ticket, Boost on the longest-odds leg, Timeout
   unused by bots), curve grid runner, pair-composition audit, gate table with PASS/FAIL,
   close-call + totem telemetry, sim-report-2.md generator.
4. **Console**: payment settle screen (PAID / SHORT / TOTEM beats), new shop (passives +
   consumables + sell-back), consumable verbs (betslip: [P]lay boost; sweat: [M]ulligan slip,
   [T]imeout), bookie-gift text line, Scar stack counter in the header.
5. **Campaign**: run the grid, pick the cell, iterate item numbers (Multiplier ×, Scar pp,
   prices) until gates pass; write sim-report-2.md; DECISIONS entry; Allen console playtest
   (#8) for the fun-feel verdict.

## Key decisions & tradeoffs
- Payments over thresholds: kills coasting; instant death returns, mercy is PURCHASED (Totem)
  — pity as an item, per design/02's open question. The curve and item power are one coupled
  tune; the grid + gates decide, not intuition.
- Stake-scaled Scar stacks over a hard min-stake gate: no arbitrary threshold, farming cost is
  proportional, G6 verifies.
- Timeout ships playtest-gated: bots can't fake cash-out judgment; sim only proves it can't be
  exploited mechanically.
- Unity isolation via the DLL boundary: the room runs the OLD economy until the follow-up
  milestone (do not recopy SBR.Engine.dll). M5 phone triggers remap in that follow-up.
- Old 8 relics fully retired (parked in git history + design/03 note), keeping the provable
  catalog small. Combo effects and sport-scoped charms are the committed NEXT wave (design/10
  B2/C), not this one.

## Risks / open questions
- The grid may have no cell passing G2 AND G3 at current item power — item numbers (×1.5,
  5pp, prices) are the adjustment knobs; expect 2–3 iterations.
- G5 superadditivity needs statistical care (10k+ runs/cell; report the margin, not just the
  boolean).
- Martyr bot design shapes G6's meaning — it should farm as ruthlessly as a human would
  (min-stake 2-leg longshots, cash-in timing on bank size).
- Consumable-use policies for the skilled bot inject designer judgment into G3 — keep policies
  simple and documented in the report.

## Out of scope
Unity/room changes of any kind (DLL not recopied), M5 phone trigger remap, combo effects,
sport-scoped charms, interest-on-savings, multiple books, sound, meta progression.
