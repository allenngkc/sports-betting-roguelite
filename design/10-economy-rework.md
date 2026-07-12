# 10 — Economy Rework: the Debt Model, Ratchets, and the 3+3 Item Slate

_Discussion doc opened 2026-07-12 (Allen + Claude), following design/09's CloverPit research.
Status: DISCUSSION — the grill locks it. The sim (S3/S4 gates + combo scan) is the referee for
every number in here._

## A. The debt-payment model (Allen's proposal — the structural change)

**Proposal:** replace threshold targets ("hold ≥ X at settle") with DEBT PAYMENTS deducted from
the bank every settle (`bank −= D_r`; can't pay = run over). Remove debt-as-HP (the float).

**Why it's right:** the sim proves the threshold model goes soft mid-game — skilled play banks a
surplus by R4 and coasts. Payments confiscate the cushion every round, converting the game into
an income-rate race: per-round return must outpace debt growth FOREVER. Stake sizing stays
meaningful all run (you size against post-payment working capital), and Band 3 becomes legible
("payments that should be lethal, covered without blinking").

**The fiction (reframe): the bookie is the creditor.** You start the run in the hole to him —
the payment schedule is his, settle day is paying the book, the final payment clears you (run
win). This REPLACES the float-based bookie beats and makes the M5 phone stronger:

| Old trigger (float world) | New trigger (creditor world) |
|---|---|
| RUN_START welcome | schedule reminder ("first payment friday. don't be a stranger.") |
| FLOAT_WARM / FLOAT_COLD | (gone — no floats) |
| DEBT_BETTING / NO_MORE_FAVORS | payment-due warning; "final payment. all of it." |
| CLEARED | payment receipt, cold ("received.") |
| COLLECTION | the short-payment death text |
| (new) | bookie GIFT texts — consumable delivery channel (see D) |

**Consequences owned:**
- Instant death on a missed payment returns (the float existed to soften exactly this —
  DECISIONS 2026-07-09; playtest #2 validated the float's pressure). The new mercy valve is the
  bookie-gift channel: the book keeps cold bettors alive because he wants you PAYING — pity
  system, retention satire, and phone content in one mechanic.
- Full curve reconstruction, not a retune. Sketch for the grill (numbers are sim fodder, not
  proposals): start bank 500, D_1 ≈ 200–300, growth ×1.6–2.0/round → D_8 ≈ 8k–25k. The binding
  gate stays S4 (skilled median ≥7 must remain ACHIEVABLE with items); S3 (naive dies 3–4) is
  nearly automatic under payments.
- Engine work: Settle() rewrite, RunConfig (Targets → Payments), sim strategies re-taught,
  TV settle card copy, M5 trigger remap, HANDOFF of debt-as-HP tests.
- Interest-on-savings (CloverPit's 7%/round) becomes genuinely interesting under payments
  (save toward the next payment vs bet it) — candidate passive axis, see B.

## B. Ratchet catalog (candidates — the grill picks; all must be /sim-auditable)

Law (agreed 2026-07-12): **ratchets persist their wound-up state visibly** — stack counts render
on the laptop relic strip. A hidden ratchet is a hidden system.

| Candidate | Trigger (winds) | Effect (permanent for the run) | Factor | Notes |
|---|---|---|---|---|
| **Streak Engine** | each consecutive settle with all payments made + ≥1 winning ticket | +X% to PayoutMultiplier per stack | payout (product slot) | Allen-liked; streak breaks do NOT unwind (ratchets never unwind — tension comes from the trigger, not loss aversion) |
| **Green Winder** | every green leg presented | +small% PayoutMultiplier per N legs | payout | the "every spin matters" CloverPit feel; sweat becomes engine fuel |
| **Scar Tissue** | every busted ticket | next ticket +X% payout per stack, consumed on win | payoff rewrite | converts pain into the comeback bet; loss-triggered = anti-frustration built into the engine |
| **The Vig Ledger** | per $100 cumulative vig paid | +1% permanent odds boost on all legs | o | volume play; makes long parlays cheaper over time (design/09 §3a: unlocks the parlay exponential) |
| **Settle-Up Interest** | each settle with surplus banked | +1pp interest rate on post-payment balance | economy | the CloverPit interest analog, discipline-flavored; pays weekly |
| **All-In Callus** | each all-in ticket placed (≥ bank/2) | High Roller threshold bonus grows +2%/stack | payout | feeds the existing PayoutMultiplier slot; stacks with High Roller's fantasy |

Grill question seeded: 3 passive slots probably = one ratchet + one static multiplicative engine
+ one economy/compounding piece (design/09 §4). Which ratchet, and does the static engine feed
the same PayoutMultiplier product (composition!) or a different factor?

## C. Scoped charms and combo effects (Allen's extensions — logged for v2, not this rework)

- **Sport/player-scoped charms**: a charm applies only to legs of a given sport (or player, once
  props exist — design/04 parked props for v2). The parlay receives the boost iff it contains a
  qualifying leg → composition puzzle: build slips that ACTIVATE your charms. Needs sport
  identity on matchups (multi-sport is a planned reskin of the event vocabulary — design/04),
  so this lands naturally WITH the second sport, not before.
- **Combo effects (A + B in one parlay → effect C)**: set-collection inside the slip. Powerful
  with scoped charms (cross-sport parlays as the activation cost). The /sim combo scan already
  exists to audit exactly this shape.

## D. The consumable slate (converging; grill finalizes)

Channels (agreed 2026-07-12): **shop = the reliable channel** (separate consumable slots,
playtest #1's split pools); **bookie gifts via the phone = the pity/flavor channel** (cold
streak → the book texts you a promo — he wants you betting); **no random drops from play**.
Sell-back: yes, 50% of price (also opens resell-scaling design space later — CloverPit's Dark
Lotus pattern).

Candidates for the 3 slots:
1. **Mulligan Slip** (timed p-safety): play DURING the sweat when a leg goes dead — leg voided,
   ticket lives (multi-leg only). The ±0.0pp passive becomes a dramatic timed save.
2. **Profit Boost** (pre-lock): one leg's odds ×1.3, single use. The literal sportsbook promo;
   teaches `o` by isolation. (No-Vig Token CUT — Allen 2026-07-12: 5% vig is invisible to the
   player fantasy; real bettors don't feel vig, a spreadsheet benefit is a dead consumable.)
3. **Free Bet** (pre-lock alt): stake refunded as cash if the ticket loses. The other canonical
   promo; downside protection with the true cost hidden in vig — satirically on-message.
4. **Timeout** (mid-sweat, design/04 Band-2 ladder): freeze the drama + hold the cash-out offer
   for 3 events. First live-agency verb; engine seam (ApplyLiveEffect) already exists unused.
5. **Ref's Whistle** (mid-sweat alt): veto one just-fired event, re-sample via the intervention
   seam. Bends the storyline honestly; louder than Timeout, costs more sim care.

Grill picks 3 of the 5 (leans: 1 + 2 + 4 — one per moment: sweat-save, pre-lock, live-agency).

## E. Bounded-p doctrine (reaffirmed 2026-07-12)

p stays bounded and saturating; CloverPit had to invent 666 to re-inject threat after Luck made
wins guaranteed — we never remove the sweat's uncertainty in the first place. p-side effects are
consumables (timed, scarce), never passive engines. Scaling lives on o (parlay product), the
PayoutMultiplier product slot, payoff rewrites, and economy flows — with bankroll compounding as
the native exponential underneath (design/09 §3b).

## Open (for the grill)

1. Payment curve numbers + start bank (sim grid: curve × item power; S3/S4 re-gated).
2. Grace on a missed payment: none (clean death), or one bookie-gift intervention?
3. The 3 passives: which ratchet; static engine on the same product slot or a second factor?
4. The 3 consumables: final pick of the 5; shop prices; bookie-gift trigger conditions.
5. Consumable slot count (2? 3?) and whether gifts can overflow slots.
6. Sweat presentation of ratchet winds (a +1 stack ping during the sweat = new juice moment).
