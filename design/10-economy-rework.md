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
| **Scar Tissue** | every busted ticket | +5%/stack to the NEXT winning ticket's payout, uncapped, consumed by that win (Allen's spec 2026-07-12: 20 busts → one safe parlay cashes +100%) | payoff rewrite | the martyr archetype — eat variance early, cash the scar late. FARMING GUARD needed: min-stake busts are cheap in dollars (real cost = ticket slots/time); grill picks a qualifier (stake ≥ fraction of bank, or stacks scale with stake) + combo scan |
| **The Vig Ledger** | per $100 cumulative vig paid | +1% permanent odds boost on all legs | o | volume play; makes long parlays cheaper over time (design/09 §3a: unlocks the parlay exponential) |
| **Settle-Up Interest** | each settle with surplus banked | +1pp interest rate on post-payment balance | economy | the CloverPit interest analog, discipline-flavored; pays weekly |
| **All-In Callus** | each all-in ticket placed (≥ bank/2) | High Roller threshold bonus grows +2%/stack | payout | feeds the existing PayoutMultiplier slot; stacks with High Roller's fantasy |

**The 3-passive portfolio principle** (clarified 2026-07-12): three different power CURVES so
buy order and run stage matter — a RATCHET (weak at purchase, monstrous late; earns power from
play), a STATIC multiplicative engine (full power instantly, e.g. "3+ leg parlays pay ×1.5"),
and an ECONOMY/protection piece (grows or guards the bankroll outside tickets). Composition
lean: static engine and ratchet both feed the SAME Ticket.PayoutMultiplier product slot, so
owning both MULTIPLIES (×1.5 static × 2.0 wound scar = ×3.0) — the stacking-strategy fun.

**TOTEM OF UNDYING (Allen 2026-07-12) — the death-save as a relic, resolving open question #2:**
one charge, and **purchasable only ONCE per run** (no re-buys after it burns — you get one
mercy, ever); when a payment can't be met, the totem triggers and the run survives. Answers
design/02's "pity as an item, not a hidden system." Visible protection changes betting behavior
(a real decision layer), the shop charging for mercy is peak bookie satire, and the sim already
predicts survival items audit strongest (Insurance was DOMINANT). Grill parameter: on trigger,
payment waived clean vs the bookie covers it and the NEXT payment grows by shortfall ×1.5 (the
old float math, itemized — lean). This likely claims the economy/protection slot.

## B2. Direction of record (Allen, 2026-07-12): stacking strategy IS the fun

"A majority of the fun of this game should be players thinking about strategies on how to stack
it up." Consequences: the item catalog GROWS along the payout-composition axis over time (more
multipliers, more feeders into the product slot, more combo shapes); every future item is judged
first by "does it create a stacking decision," not merely "is it balanced"; and the combo-effect
space (C below) is a committed future direction, not a maybe. This sharpens design/02's economy
doctrine — Band 3 brokenness is REACHED through composition skill, and composition skill is the
game's primary strategic verb outside the sweat.

## C. Scoped charms and combo effects (Allen's extensions — committed future direction, post-rework)

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
4. **Timeout** (mid-sweat, design/04 Band-2 ladder): PLAYER-FIRED, never random (clarified
   2026-07-12) — pressed at a moment the player chooses: the drama freezes and the cash-out
   offer HOLDS for 3 events, buying the take-it-or-ride decision without the number crashing
   mid-hesitation. Skill = firing it at the offer's peak. Engine seam (ApplyLiveEffect) exists.
5. **Ref's Whistle** (mid-sweat alt): also player-fired — veto the event that just cratered
   your win prob; the drama generator re-samples it honestly through the intervention seam (the
   re-roll may still come out bad). The "rigging the game" fantasy on your thumb. Random
   versions of either would be drama-generator CONTENT (variance), not agency — different tool,
   maybe someday, not these slots. Hard rule stands: options, never QTE prompts.

Grill picks 3 of the 5 (leans: 1 + 2 + 4 — one per moment: sweat-save, pre-lock, live-agency).

## E. Bounded-p doctrine (reaffirmed 2026-07-12)

p stays bounded and saturating; CloverPit had to invent 666 to re-inject threat after Luck made
wins guaranteed — we never remove the sweat's uncertainty in the first place. p-side effects are
consumables (timed, scarce), never passive engines. Scaling lives on o (parlay product), the
PayoutMultiplier product slot, payoff rewrites, and economy flows — with bankroll compounding as
the native exponential underneath (design/09 §3b).

## F. Campaign round-1 rulings (Allen, 2026-07-13) + the COMPS currency

Sim round 1 (sim-report-2.md) surfaced five rulings; Allen's calls:
1. **Naive band accepted as found** (dies R5, 0.0% wins — "no one would really play naively").
2. **Skilled win target: 5–8% per run** (final-product realism) — G3 re-banded.
3. **SECOND CURRENCY ADOPTED — "COMPS"**: items are bought with comps, earned by WAGERING
   VOLUME (like a real book's loyalty program — the satire writes itself: chasing comps is
   −EV cash, exactly like real VIP programs). This also fixes round 1's discovered flaws in one
   stroke: the cash bank shrinks to ~2–3 payments (betting becomes mandatory — no more idling
   through round 5 on a fat bank, Allen's core observation), the item-price-vs-capital bind
   dissolves, and the engine arrives organically at R2–3 (Band 1 restored, G4 re-fixed).
4. **Current 3+3 catalog = basic-loop proof only.** The future is 150+ charms/consumables —
   at that scale the sim's audits become genuinely load-bearing.
5. **G5 measurement fix approved** (fixed-discipline bot isolates composition from the
   engine-tempts-aggression artifact — itself a keeper finding: owning the engine makes bots
   AND players bet bigger and die faster while winning more).

**LONG-TERM PILLAR (Allen): 150+ unique charms manipulating the factors, with FUSION — players
fuse distinct charms into powerful combinations as the main fun.** Extends B2/C: the composition
axis goes from "stack products" to "craft new items from pairs." Fusion design is its own future
grill; the one-product-slot law and the sim combo scan are its foundations.

## G. Playtest #8 amendments (Allen, 2026-07-13) — Timeout cut; Totem = full deferral

1. **Timeout CUT** (Allen: "useless"; the audit always read ≈0 and it was playtest-gated —
   the playtest voted no). The slate is Mulligan Slip + Profit Boost. The live-intervention
   seam (ApplyLiveEffect/OfferHoldEffect) survives with its own test: Ref's Whistle or a
   returned hold can buy it back some day, per D's ladder.
2. **The Totem defers instead of draining.** Old: pay what you have, bank → $0, shortfall × 1.5
   surcharged. Allen fired it and read $0 as "no capital" — correctly: below the $10 min stake
   the saved round is unplayable, mercy in name only. New: the WHOLE payment defers — bank
   untouched, payment × (1 + juice) lands on the next payment, never the final round. The
   D-section trigger question (waive-clean vs itemized-float) is thereby RE-RESOLVED to
   waive-with-surcharge after the float variant failed contact with a player.
3. **Rebalance where the cause was** (sim-report-3.md, 50k/batch, ALL GATES PASS): the G3
   breach after the cut (9.2%) came from the offer draw — 2 offer slots over a 2-item catalog
   made Mulligan Slip (+20pp audit) a guarantee every shop. Knobs: consumable offers 2 → 1,
   consumable slots 2 → 3 (answers Open #5: bank saves for the cliff), mulligan 2 → 1.5 comps,
   juice stays 0.5. Skilled 6.2%, organic totem fires 50.3%, zero flags.

## Open (for the grill)

1. Payment curve numbers + start bank (sim grid: curve × item power; S3/S4 re-gated).
2. ~~Grace on a missed payment~~ → RESOLVED direction: the Totem relic (see B); grill picks
   waive-clean vs itemized-float on trigger.
3. The 3 passives: Totem likely takes the economy/protection slot; which ratchet (Scar Tissue
   front-runner, farming guard TBD); the static engine's shape — and confirm both multipliers
   feed the one PayoutMultiplier product.
4. The 3 consumables: final pick of the 5 (leans 1+2+4); shop prices; bookie-gift triggers.
5. Consumable slot count (2? 3?) and whether gifts can overflow slots.
6. Sweat presentation of ratchet winds (a +1 stack ping during the sweat = new juice moment).
7. Scar Tissue farming guard: min-stake qualifier vs stake-scaled stacks vs none (let the combo
   scan judge).
