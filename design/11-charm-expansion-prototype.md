# 11 — The charm expansion (SHIPPED — reconciled with PLAN.md rev 5 + the tuning campaign)

**What shipped (Allen's mandate, 2026-07-14):** 17 new items — 12 passives + 5 consumables —
so players make COMBO decisions: the stacking-strategies pillar (design/10 B2) at prototype
scale. The catalog is now **15 passives + 7 consumables = 22 items**. CloverPit-inspired by
explicit mandate (`clover-pit-charms-list.txt`); original designs come at the 150+ fusion
stage. Locked via /grill-me-codex (5 review rounds, APPROVED — PLAN.md rev 5,
PLAN-REVIEW-LOG.md is the argument of record) and tuned by the gate campaign (sim-report-4,
14 iterations; all numbers below are the campaign's, re-tunable by any future campaign).

**The translation grammar** (how CloverPit reads in our four-number model):
symbols → odds bands (chalk ≤1.50 vs longshots ≥3.00) · patterns → parlays/leg counts ·
luck → FORBIDDEN as passive (bounded-p, design/10 E) — timed consumables only ·
interest/tickets → comps · 666/999 → cliff payments / monster wins · restocks → the Manager ·
Ankh → the Totem (already ours).

**Laws:** one PayoutMultiplier product slot — every payout passive is a NAMED ×(1+x) factor in
the ticket's factor map. ONE locked contract modifier per ticket (Free Bet xor DoN) — the
one-modifier law, mirror of the product slot. Consumable timing never perturbs the run seed
(DeriveRng substreams). Stateful passives reset on sale; nothing accrues while unowned; all
state is per-run.

## The dealt-hand shop

Every shop visit deals **4 passives from the unowned pool + 3 distinct consumables from the 7**
(fresh draw each visit; a purchased-ever Totem leaves the pool forever; short pools deal what
remains). Scarcity comes from pool dilution — a specific passive shows ~27% of visits. One-time
entry effects (Rake's Rebate interest) fire on ENTRY only; **Ask for the Manager** redeals the
hand once per visit through a derived stream, so future visits are untouched. Comps accounting
is integer tenths: wagering earnings pool raw and commit once at LOCK (before Whale/Collection
snapshots); interest/prices/sells move exact tenths.

## The 15 passives

| Item | Effect (as tuned) | Notes |
|---|---|---|
| The Multiplier | 3+ legs → ×1.6 | the static engine (1.5 → 1.6 in the campaign) |
| Scar Tissue | stake-scaled +5pp/bust ratchet; first-placed carries, burns on realize | unchanged |
| Totem of Undying | once-ever purchase; defers a non-final payment, juice 0.5 | unchanged |
| Chalk Eater | every FINALLY-Won leg at ≤1.50 → +2pp on everything, forever | winds at OnLegResolved (post-window: whistle rescues wind it; voids don't) |
| Longshot Larry's Photo | ≥1 active leg at ≥3.00 → ×1.6 | lock factor; toggles OFF if the last qualifying leg is voided; prices into cash-outs (design/02) |
| Iron Hands | +4pp per full-ride win; ANY cash-out resets to 0 | a DoN win counts (never cashed) |
| Golden Parachute | cash-outs pay ×1.08 | ceiling ×1.087 (the margin reciprocal) — above it cash-out prints money |
| The Rake's Rebate | +10% comps interest at each shop OPEN | never on a Manager redeal |
| Whale Card | ×(1 + 0.5pp per comp HELD), snapshot at lock | the hoard build; fights the shop for comps |
| Bad Beat Jar | +10pp per round where EVERY placed ticket lost, forever | cash-out disqualifies; a REFUNDED Free Bet loss still counts; zero-bet rounds never count |
| House Key | all payouts ×1.4; unpaid payments read ×1.15 while owned | getters only — never a mutation; totem surcharges book at BASE rates; selling just drops the factor |
| The System | +12pp per consecutive PnL>0 round; PnL ≤ 0 resets (zero-bet included) | the streak build |
| Comp'd Suite | a winning 4+ active-leg ticket pays +8 comps | feeds the hoard builds |
| Unopened Bobblehead | nothing; resells at 2× list | the flip (buy 2, sell 4) is intended free money — its resale margin is a G4 tuning knob |
| The Collection | ×(1 + 1pp per resale comp of owned PASSIVES, itself included) | consumables excluded; a spent Totem counts 0; lock snapshot |

## The 7 consumables

| Item | Effect | Price (comps) |
|---|---|---|
| Mulligan Slip | pending window: void the dead leg (needs ≥2 active legs) | 1.5 |
| Profit Boost | pre-lock: one leg's odds ×1.3 | 2.5 |
| Free Bet Token | LOCKED MODIFIER: stake back in cash if the ticket loses; the bust still feeds Scar and the Jar | 3 |
| Ask for the Manager | shop: redeal the hand, once per visit | 1 |
| Double or Nothing Slip | LOCKED MODIFIER: ×2 on a win; cash-out offers never appear; saves still allowed | 2 |
| Bookie's Marker | betting phase: this round's payment ×0.75, once per round | 3 |
| Ref's Whistle | pending window: the grading re-rolls ONCE at the pre-kill displayed prob. Overturned → the leg STANDS at full odds (this slip only — the shared result never bends; the session's revealed state repairs, cash-out comes back). Confirmed → dead. Works on single-leg tickets | 2 |

Gift pool: all 7, uniform (the Marker as a pity gift is the bookie's best line).

## What the campaign proved (sim-report-4 + holdout)

- All six gates on the 22-item catalog; the audit runs with paired-seed CIs (Bonferroni),
  declared exposure thresholds (UNDEREXPOSED blocks), and within-kind dominance contrasts.
- **G3 re-banded again by Allen (2026-08-08): median death ≥5, win 4.5–8%** — the floor moved down
  0.5pp because the economy sits at 5.4–5.5% and the gate could not adjudicate its own reading
  against a 5.0% edge. See `DECISIONS.md`. The 2026-07-15 band it supersedes, recorded as issued:
- **G3 re-banded by Allen (2026-07-15): median death ≥5, win 5–8%.** The dealt hand adds build
  variance BY DESIGN — the half without an early income engine dies at the R5 cliff; that
  spread is the roguelite shape. (Median ≥6 was jointly unreachable with the naive/noshop
  bands without gutting the dealt-hand identity; options were presented, Allen ruled.)
- **G4 gates on PASSIVE-ONLY counterfactual EV** (base odds, no refund leg, no DoN): the
  catalog's fantasy is cheap +EV promos, so full-contract EV measures time-to-first-promo,
  not the arc. Full-contract EV is the telemetry series beside it. (Codex-sanctioned branch;
  logged amendment.)
- Campaign findings of record: pre-engine BUY DISCIPLINE (skip low-tier hands, bank comps)
  is worth more than any single item; reserving toward FUTURE payments lowers survival —
  the income race punishes timidity; the Bobblehead flip funds early promos (its resale
  margin is an economy knob, not a toy).
- Payments retuned: [60, 70, 85, 105, **155**, 375, 710, 1350]; comps 0.10 → **0.12**/$.

## Cut / deferred

- **Same Game Special (slate manipulation): CUT by Allen 2026-07-14** ("not useful").
- Fusion, sport/player-scoped charms, the 150+ catalog (design/10 C), charged actives
  (Red Button grammar), paid rerolls beyond the Manager: future.
- Archetype VIABILITY as a gate: the chalk/hoarder/ironhands bots are telemetry only.
