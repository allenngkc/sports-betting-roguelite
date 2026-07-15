# Plan: Charm Expansion — 17 combo items, the dealt-hand shop, and the archetype sim

_Locked via grill — by Claude (Fable, game director) + Allen, 2026-07-14. Rev 5 after Codex
rounds 1–4 (see PLAN-REVIEW-LOG.md). Context: design/11 + Allen's rulings, design/10 laws,
DECISIONS.md (playtest #8). Scope: ENGINE + SIM + CONSOLE + UNITY, gates re-held before
playtest #9._

## Goal

Give the player their first real COMBO decisions — the stacking-strategies pillar (design/10
B2) at prototype scale. Ship **17 new items (12 passives + 5 consumables; Same Game Special
CUT)** — catalog becomes **15 passives + 7 consumables = 22 items, 105 passive pairs** — onto
the payment/comps economy, with the DEALT-HAND shop (3 passives + 2 consumables per visit),
the Ref's Whistle grading re-roll, and a sim upgraded to audit 22 items honestly. All counts in
this plan derive from the schema table below (rev-1/2 said 16/21/91 — arithmetic error, fixed).

## The catalog schema (IDs, ops, params, opening prices — all numbers are sim bait)

| ID | Name | Kind | Op | Params (initial) | Price (comps) |
|---|---|---|---|---|---|
| `the_multiplier` | The Multiplier | passive | ParlayPayoutMult (existing) | legs ≥3 → ×1.5 | 5 |
| `scar_tissue` | Scar Tissue | passive | ScarTissue (existing) | stake-scaled +5pp/bust | 4 |
| `totem_of_undying` | Totem of Undying | passive | TotemOfUndying (existing) | defer, juice 0.5 | 6 |
| `chalk_eater` | Chalk Eater | passive | LegBandRatchet | revealed-Won leg, offered odds ≤1.50 → +1pp permanent | 5 |
| `longshot_photo` | Longshot Larry's Photo | passive | LegBandProductFlag | leg odds ≥3.00 → ×1.6 lock factor, void-toggled | 5 |
| `iron_hands` | Iron Hands | passive | FullRideRatchet | +4pp/full-ride win; ANY cash-out resets | 5 |
| `golden_parachute` | Golden Parachute | passive | CashOutQuoteScale | ×1.08 (hard ceiling ×1.087) | 4 |
| `rakes_rebate` | The Rake's Rebate | passive | ShopEnterCompsInterest | +10%, comp-quantized | 6 |
| `whale_card` | Whale Card | passive | CompsHeldProduct | ×(1 + 0.005/comp), LOCK snapshot | 6 |
| `bad_beat_jar` | Bad Beat Jar | passive | AllLossRoundRatchet | +8pp/qualifying round, permanent | 4 |
| `house_key` | House Key | passive | ProductPlusPaymentFactor | ×1.4 product; unpaid-payment getter ×1.15 | 7 |
| `the_system` | The System | passive | StreakRatchet | +10pp/consecutive PnL>0 round; PnL≤0 resets | 5 |
| `compd_suite` | Comp'd Suite | passive | LegCountCompsPay | win with ≥4 active legs → +8 comps | 4 |
| `bobblehead` | Unopened Bobblehead | passive | NoOp + ResaleOverride | resale = 3.0 × list | 2 |
| `the_collection` | The Collection | passive | ResaleValueProduct | ×(1 + 0.01/resale comp), LOCK snapshot | 5 |
| `mulligan_slip` | Mulligan Slip | consumable | PendingLossVoid (existing) | — | 1.5 |
| `profit_boost` | Profit Boost | consumable | LegOddsBoost (existing) | ×1.3 one leg | 1.5 |
| `free_bet` | Free Bet Token | consumable | TicketLossRefund (locked contract modifier) | stake ×1.0 on Lost | 2 |
| `ask_manager` | Ask for the Manager | consumable | ShopRedeal | once per visit (latch) | 1 |
| `double_or_nothing` | Double or Nothing | consumable | TicketCommitDouble (locked contract modifier) | ×2 product; no cash-out | 2 |
| `bookies_marker` | Bookie's Marker | consumable | PaymentRelief | this round ×0.75; once per round | 3 |
| `refs_whistle` | Ref's Whistle | consumable | PendingLossReroll | grading re-roll at captured prob | 2 |

Shop config: `PassiveOfferCount = 3`, `ConsumableOfferCount = 2` (field keeps its name; default
moves 1 → 2). **Comp accounting: integer deci-comps + a per-round accrual buffer (Codex r3 #3
+ r4 #2).** The authoritative balance is an INTEGER count of tenths (`long _deciComps`) — no
hidden fractional state, ever. Wagering earnings accumulate in a raw per-round buffer and
COMMIT ONCE at `LockRound` (quantize to tenths, discard the remainder) — before the Whale
snapshot reads the balance. Interest, grants, purchases, and sells each operate on the integer
balance in exact tenths at their own single commit points; every conversion INTO deci-comps
(incl. 0.75-comp consumable resales) uses `MidpointRounding.AwayFromZero`. Split-vs-combined
invariance within a round holds by construction; affordability, snapshots, and display are the
same integer value. Invariance + no-negative-balance tests pinned.

## Approach

1. **Typed effect pipeline.** `EffectEngine` hooks, fixed documented order: `OnAcquire`,
   `OnSell`, `OnLock(run)`, `OnLegResolved(leg, finalGrade, offeredOdds)` — **exactly once per
   leg, emitted only after any pending window has CLOSED (Codex r3 #2)**, carrying the final
   ticket-local grade (Won / Lost / Voided): a Whistle-rescued chalk leg winds Chalk Eater;
   voided legs never wind; legs unrevealed at cash-out never fire the hook — then `OnBust`,
   `OnTicketRealized`, `OnRoundResolved(RoundResolution)`, `OnShopEnter`, `CashOutQuoteScale`.
   No ID checks inside `Run`. **Stateful passives reset on SALE; a reacquired item starts
   fresh; qualifying events while unowned accrue nothing (Codex r3 #4)** — sell/rebuy and
   parallel-isolation tests pinned.
2. **Per-ticket lock-factor snapshot (Photo safety) + ONE contract-payoff evaluator (Codex
   r3 #1).** At lock, each ticket stores a named, immutable factor map (`multiplier`, `photo`,
   `whale`, `collection`, `don`, …). A Mulligan void that removes the last qualifying ≥3.00
   leg toggles ONLY the `photo` factor off — nothing else recomputes. A single evaluator owns
   the ticket's outcome→cash-flow map via `ExpectedTerminalCredit(p) = p × win credit (full
   product) + (1−p) × refund (Free Bet)`. **The cash-out quote uses `ExpectedTerminalCredit`
   directly (before margin and Parachute); G4 uses `ExpectedTerminalCredit(pLock) − stake`
   (Codex r4 #1 — terminal credit and net EV are different numbers).** Numeric tests pin both
   a Free-Bet ticket and an ordinary ticket. design/02's law, loss side priced too.
3. **`RoundResolution` payload.** Emitted after the terminal-realization ledger (refunds
   resolved) and BEFORE the payment: pre-payment PnL, per-ticket terminal states, refund
   totals. The System: streak extends iff PnL > 0; PnL ≤ 0 (including zero-bet rounds) resets.
   Bad Beat Jar: qualifies iff ≥1 ticket placed AND every ticket Lost (cash-out disqualifies;
   a REFUNDED Free Bet loss still counts as Lost — refunds are cash flow, not redemption).
4. **Terminal-realization ledger.** One idempotent settle-time pass owns terminal accounting;
   Free Bet refunds fire exactly once (early busts included; `Refunded` latch).
5. **Pending window generalized.** Opens when any legal save is held: Mulligan (multi-leg,
   voids) or Whistle (ANY ticket incl. single-leg, re-rolls). `PendingLossContext` captures
   the displayed pre-kill win-prob at suspension; the Whistle rolls against that immutable
   value from a derived stream. **On success: ticket-local `GradeOverride` = Won AND the
   session's revealed state flips to Won, the window clears, play advances, cash-out
   eligibility is restored** (test: cash-out after a non-final rescue; test: two tickets
   sharing the rescued matchup — only the whistled slip bends).
6. **`DeriveRng(seed, round, ticketId, legIndex, action, ordinal)`** over the existing
   PCG/FNV plumbing; golden tests pin main-stream isolation and cross-key independence.
7. **Consumable legality matrix (engine-enforced, atomic).** Free Bet and DoN are LOCKED
   CONTRACT MODIFIERS — at most one modifier per ticket (mirror of the one-product-slot law);
   Marker once per round; Manager once per visit (latch independent of inventory); one save
   per pending window. Duplicates in slots legal. Illegal plays throw before consuming.
8. **Shop = `EnterShop` (one-time: Rebate) + pure `DealOffers`.** Manager → `DealOffers` only.
   3 distinct unowned passives + 2 distinct consumables per visit; Totem excluded forever
   after purchase; short pools deal what remains.
9. **House Key = getter factor; Totem surcharges stay BASE (Codex r2 #3).** `_payments` holds
   base + surcharges only. Payment getters (`CurrentPayment`/`NextPayment`/`PaymentSchedule`
   unpaid entries) apply ×1.15 iff owned. **A Totem deferral books the BASE current payment ×
   (1 + juice) into the base next payment** — the Key factor applies once, through the getter,
   never compounded into surcharges; selling drops the factor and leaves base + surcharge.
   Pinned test: buy → totem fire → sell reproduces Codex's worked numbers (402.5 / 350 shape).
10. **`GetResaleValue(item)`** single truth for sells + Collection; Bobblehead 3× in its
    definition; a fired Totem resells at 0 and counts 0. **Collection's domain (Codex r3 #5):
    the sum of resale values of all currently owned PASSIVES, itself included; consumables
    excluded** (they're ammo, not collection). Numeric pin test: a defined 5-passive inventory
    produces one exact factor.
11. **Read-only hardening (scoped).** Exposed lists/schedules return read-only views; full
    `RunConfig` immutability deferred as logged debt.
12. **Sim — strategy surface.** `ChoosePendingLossAction(ctx) → Mulligan | Whistle | Decline`;
    RunPlayer records the choice. **RandomStrategy bug fixed**: affordability checks
    `run.Comps` (not cash), and random shopping picks legally across both dealt rows.
13. **Sim — skilled bot, full catalog.** Tier list over all 15 passives + replacement policy
    (with slots full, sell the lowest-tier owned item when a higher-tier is dealt, respecting
    resale economics); item-aware wagering: builds 4+ leg tickets when Suite/Multiplier owned,
    prefers chalk legs when Chalk owned, carries one ≥3.00 leg when Photo owned, holds comps
    when Whale/Rebate owned (target floor sim-tuned); modifier policy (Free Bet on longest
    odds, DoN on shortest); Marker at cliff rounds; Manager when the dealt hand has no
    tier-1/2 item.
14. **Sim — archetype telemetry bots restored**: chalk grinder, VIP hoarder, iron hands —
    telemetry only, reported alongside skilled.
15. **Sim — honest audit.** Per-item exposure policies (Bobblehead sold next shop; Manager
    every visit; Marker at cliffs; Free Bet/DoN/Whistle per policy); audit reports exposure
    counts; DEAD requires nonzero exposure; **any item still BOT-BLIND at the final campaign
    is a BLOCKING failure** (fix the policy, not the flag). DOMINANT ranks within kind;
    per-acquisition value reported. **Flags use paired-seed confidence bounds, not fixed
    cushions (Codex r3 #6, r4 #3)**: per-seed deltas give an empirical SE; DEAD requires the
    95% CI upper bound of Δwon below 1.0pp (and |Δmean| CI within ±0.05); DOMINANT tests the
    explicit contrast `best − 2×next` — CI lower bound > 0, a practical margin of +0.5pp, and
    the winner itself non-DEAD (no contradictory labels); Bonferroni-corrected across ALL
    tested endpoints (every DEAD test + each within-kind dominance contrast), not just item
    labels. **Exposure thresholds are declared before the campaign (Codex r4 #4)**: an item's
    audit is valid only with ≥500 acquisitions and ≥200 uses/triggers in its batch; anything
    under threshold reports UNDEREXPOSED and BLOCKS (fix the policy or enlarge the batch —
    never interpret the delta).
16. **Sim — telemetry events.** Per-item: offered, bought, sold, used, triggered, stacks,
    comps granted; conversion (offered → bought) feeds the starvation watch.
17. **Gates.** G1, G2, G3, G5 as ratified. **G4 = CONTRACT EV sampled after `OnLock`,
    including all LOCKED modifiers** (offered odds incl. boosts, DoN double, Free Bet's
    loss-side refund — Codex r2 #4 accepted: a locked flag is contract, not policy);
    saves and cash-out decisions stay in a parallel POLICY-EV telemetry series. **G6 = the
    worst-case granted batch gates** (Scar + Jar granted, Free Bet refilled each round,
    ≤ skilled + 2pp); the organic upgraded-martyr batch is telemetry beside it.
18. **Anti-overfit protocol with a freeze (Codex r4 #5).** Tune on `TUNE-` seeds. Before the
    validation campaign: code, bot policies, and all numbers FREEZE at a named commit hash,
    recorded in the report. Validation runs once on `HOLDOUT-` seeds at 50k/batch; G3 must
    land ≥0.3pp inside the band; G5 excess > +0.5pp. ANY change after that run — policy fix,
    price nudge, anything — invalidates the report and burns the namespace: the rerun uses
    `HOLDOUT2-` (then `HOLDOUT3-`…), with burned namespaces listed in the final report.
19. **Engine test matrix (named, per item + interactions).** One behavior test per item (17
    new), plus: Photo void-toggle recompute; Rebate-vs-Manager separation (no interest on
    redeal); House Key × Totem worked-numbers pin; Whistle continuation + shared-matchup
    isolation + single-leg eligibility; Free Bet idempotence across both bust paths; legality
    matrix (modifier exclusivity, Marker/Manager/window latches, reset timing per round);
    dealt-hand draws (distinct, unowned-only, totem exclusion, exhaustion); DeriveRng goldens;
    comp-quantum invariants; determinism pins (no consumable perturbs Outcomes).
20. **Ratchet visibility is IN scope (Codex r2 #11).** Engine exposes a typed effect-state
    snapshot (id → stacks/streak/factor). Console prints it in the betting header; Unity shows
    it on the laptop header strip and TV chrome (Scar pp precedent extends to Chalk pp, Iron
    stacks, Jar pp, System streak). Only animations/celebration juice stay deferred.
21. **Console**: dealt-hand shop (two rows + Manager), [M]/[R] window, modifier flags at
    ticket entry, Marker prompt, effect-state header.
22. **Unity**: LaptopScreen 5-card hand + Manager button; TV [M]/[R] window; betslip Free
    Bet/DoN toggles (engine enforces exclusivity); effect-state chrome; DLL rebuild; suites
    green.
23. **Docs + proof**: design/11 FULLY reconciled with this plan (Codex r3 #7) — catalog rows
    rewritten to rev-5 semantics (Photo lock-factor + cash-out pricing, Rebate integer-deci
    interest, House Key getter, Whistle window + revealed-state repair), Same Game Special
    rows marked CUT, counts corrected; DECISIONS entry; sim-report-4.md (tune) +
    sim-report-4-holdout.md (validation); README; PLAYTESTS gate note → playtest #9.

## Key decisions & tradeoffs

- **Whistle = grading re-roll in the pending window (Allen: c)**; session revealed-state flips
  on success — the shared universe never bends, cash-out comes back to life.
- **Sim bar = ratified gates + audit hygiene (Allen: a)** with sharpened instruments: G6
  worst-case gates, holdout seeds, margined flags, BOT-BLIND blocking.
- **Photo prices into cash-outs at lock via immutable per-ticket factor maps** — design/02 law
  upheld; recompute risk closed with the snapshot-and-toggle design.
- **G4 includes locked modifiers** — Codex's challenge accepted; the gate measures the
  contract you signed, the telemetry measures how you played it.
- **One modifier per ticket** (Free Bet xor DoN); **House Key getter + base-rate surcharges**;
  **comp quantum 0.1**; offered post-boost odds everywhere; Iron Hands global reset; gift pool
  all 7; per-run ratchets; lock snapshots for Whale/Collection.

## Risks / open questions

- **Multiplicative pileup vs G3** (9 product sources): DOMINANT-within-kind + number authority.
- **Skilled-bot complexity is now the long pole in /sim** — replacement policy + item-aware
  wagering is real code; budget a tuning round for bot policy before believing item flags.
- **Whistle honesty**: captured prob is revealed-state only; goldens pin the roll inputs.
- **Dealt-hand starvation**: conversion telemetry + Manager + gifts.
- **Accepted debt**: full RunConfig immutability deferred; `Run.Config` documented
  do-not-mutate.

## Out of scope

- Same Game Special / slate manipulation (CUT by Allen).
- Fusion, scoped charms, 150+ catalog, charged actives.
- Paid rerolls beyond the Manager; full immutable-config refactor.
- Ratchet-wind ANIMATIONS (state display is in scope; celebration juice is not).
