# Plan Review Log: Charm Expansion — 16 combo items + the dealt-hand shop

Act 1 (grill) complete — plan locked with the user. MAX_ROUNDS=5.
The grill resolved: Whistle semantics (grading re-roll in the pending window, full odds on a
save — option c), the sim acceptance bar (ratified gates + audit flags; archetype bots are
telemetry — option a), gift pool (all 7), dealt-hand shop rules, DoN interactions, House Key
buy/sell math, Iron Hands global reset, lock-time snapshots, Bad Beat Jar counting, offered-odds
thresholds, full sim number authority. Same Game Special cut by Allen before the grill.
(The prior economy-rework plan + log live in git history — this file is per-milestone.)

## Round 1 — Codex (resumed after a client-side timeout; full repo read, 21 findings)

The plan is not implementation-ready. The item counts are consistent—14 passives, 7 consumables, 91 passive pairs—but several lifecycle, simulation, and schema gaps can produce incorrect behavior while still passing the proposed gates.

1. **The effect lifecycle is unspecified.** The new effects require acquisition, sale, lock, pre-credit win, post-realization, round-end, shop-entry, and cash-out hooks, while [RelicEffects.cs](/C:/Users/Allen/Documents/CodingProjects/sports-betting-roguelite/engine/RelicEffects.cs:112) only supports placement, bust, and realization; ad-hoc ID checks in `Run` will become ordering-dependent.
   
   Fix: Define one typed effect pipeline with explicit `OnAcquire`, `OnSell`, `OnLock`, `BeforeWinCredit`, `AfterRealization`, `OnRoundResolved`, `OnShopEnter`, and cash-out quote hooks.

2. **Longshot Photo cannot be implemented correctly with the current ordering.** [Run.cs](/C:/Users/Allen/Documents/CodingProjects/sports-betting-roguelite/engine/Run.cs:249) credits `PotentialPayout` before calling `OnTicketRealized`; applying Photo there is too late, while applying it at lock incorrectly boosts cash-outs despite being described as a winning-ticket bonus.
   
   Fix: Specify whether Photo affects cash-outs and, if win-only, apply it in a pre-credit win hook after voided legs are known.

3. **Free Bet has no exactly-once loss-settlement seam.** Tickets can become `Lost` inside `SweatSession.Bust`, after which [FinishSweat skips them](/C:/Users/Allen/Documents/CodingProjects/sports-betting-roguelite/engine/Run.cs:244); refunding in both places risks duplication, while refunding only in `FinishSweat` misses early busts.
   
   Fix: Centralize every ticket’s terminal realization in an idempotent ledger and issue Free Bet refunds once after all sweats but before round PnL and payment calculation.

4. **Whistle-only players never receive the promised window.** [SweatSession](/C:/Users/Allen/Documents/CodingProjects/sports-betting-roguelite/engine/SweatSession.cs:142) opens pending loss only when a Mulligan is held and at least two active legs remain, so a held Whistle alone—or possibly a one-leg ticket—busts immediately.
   
   Fix: Open a generic pending-loss window from the union of legal actions and explicitly decide/test Whistle eligibility on single-leg tickets.

5. **A successful Whistle is incompatible with the current grading schema.** [Leg.State](/C:/Users/Allen/Documents/CodingProjects/sports-betting-roguelite/engine/Domain.cs:105) derives exclusively from the shared `Matchup.Result`, so changing only the revealed state cannot make `Ticket.GradesWon` accept a locally rescued leg.
   
   Fix: Add a ticket-local grading override used by `GradesWon`, leaving `Matchup.Result` immutable, and test two tickets sharing the rescued matchup.

6. **The advertised Whistle probability is not currently available.** `MoveNext` overwrites `_liveProb` with the killing final event’s `WinProbAfter`—zero—before resolving the loss in [SweatSession.cs](/C:/Users/Allen/Documents/CodingProjects/sports-betting-roguelite/engine/SweatSession.cs:112).
   
   Fix: Capture a `PendingLossContext` containing the displayed probability immediately before the killing event and roll against that immutable value.

7. **The deterministic substream specification is collision-prone and references a nonexistent API.** The repo has `Fnv1a64` and named PCG streams, not `StableHash`; `seed#round#item` also gives identical rolls to multiple tickets, legs, or copies in one round.
   
   Fix: Add a documented `DeriveRng` API keyed by seed, round, ticket ID, leg index, action, and use ordinal, with golden tests for collision independence.

8. **Consumable stacking and usage limits are undefined.** Held consumables may contain duplicates, while the plan only mentions “one each per round” in the Unity section; it does not settle multiple Markers, repeated Managers, multiple Free Bets/DoNs, or Free Bet plus DoN on one ticket.
   
   Fix: Add an engine-enforced legality matrix and per-round/per-shop usage state, and validate all selected consumables atomically before consuming any.

9. **Manager can accidentally replay shop-entry effects.** Reusing `GenerateShopOffers` for rerolling risks granting Rake’s Rebate again, and a rerolled Manager can enable chaining unless “once” is enforced independently of inventory.
   
   Fix: Separate one-time `EnterShop` effects from a pure `DealOffers` function and enforce one Manager reroll per visit.

10. **Rake’s Rebate can reduce fractional comps.** With `Comps` stored as a `double`, interpreting “×1.10 (floor)” as `floor(Comps * 1.10)` turns 1.5 comps into 1.
    
    Fix: Define it as `Comps += floor(Comps * 0.10)` or convert comps to an integer currency before implementing interest.

11. **House Key invites a parallel simulation race and lossy restoration.** `Harness` shares one mutable `RunConfig` across `Parallel.For`; mutating `Config.Payments` would cross-contaminate runs, while multiplying and later dividing live values is fragile once Totem surcharges and future modifiers intervene.
    
    Fix: Keep an immutable base schedule plus run-local keyed payment modifiers, never mutate `RunConfig`, and test buy–Totem surcharge–sell under parallel execution.

12. **Bobblehead and Collection lack a common resale schema.** [SellRelic](/C:/Users/Allen/Documents/CodingProjects/sports-betting-roguelite/engine/Run.cs:370) hardcodes the global 50% calculation, while both new items require exactly the same resale valuation—including overrides—to drive gameplay.
    
    Fix: Centralize `GetResaleValue` and use it for selling and Collection snapshots, explicitly defining self-value and spent-Totem treatment.

13. **The sim cannot choose between Mulligan and Whistle.** `IStrategy` exposes only cash-out decisions, and [RunPlayer](/C:/Users/Allen/Documents/CodingProjects/sports-betting-roguelite/sim/RunPlayer.cs:120) greedily plays Mulligan before consulting a strategy.
    
    Fix: Add a `ChoosePendingLossAction` strategy hook returning Mulligan, Whistle, or Decline and record the chosen action.

14. **The promised zero-DEAD audit is impossible with the stated bot work.** The generic bot change covers passive purchase priority, but Bobblehead only works when sold, Manager must be activated in the shop, and every new timed consumable needs an explicit use policy; current skilled play never sells anything.
    
    Fix: Give every active/economy item an honest audit policy and require nonzero exposure/activation counts before interpreting its win-rate delta.

15. **The audit compares incomparable doses.** Passives are granted once, but [consumables are refilled every round](/C:/Users/Allen/Documents/CodingProjects/sports-betting-roguelite/sim/RelicGrant.cs:40), then all items share one DOMINANT ranking; this can label a perpetual Marker stronger than a one-time passive without saying anything about shop price or scarcity.
    
    Fix: Separate passive and consumable dominance checks and add organic, price-aware per-acquisition value metrics alongside granted-power tests.

16. **G6 is diluted by shop luck rather than testing the exploit.** Upgrading an organic martyr bot still leaves Scar, Jar, and Free Bet gated by random dealt-hand availability, so a broken three-item loop can pass simply because the bot rarely assembles it.
    
    Fix: Add a worst-case G6 batch with Scar and Jar granted plus a controlled Free Bet supply, retaining the organic martyr batch as separate telemetry.

17. **The required shop telemetry has no implementation step.** Current `rm.Buys` is net inventory change, so selling Bobblehead, consuming Manager, and replacing items can report zero or negative “buys”; there are no offer-impression, trigger, refund, or value-granted counters.
    
    Fix: Emit per-item events for offered, bought, sold, used, triggered, stacks changed, and value granted, and compute conversion from those events.

18. **G4’s EV calculation becomes semantically false.** [TrueTicketEvAtLock](/C:/Users/Allen/Documents/CodingProjects/sports-betting-roguelite/sim/Metrics.cs:80) models only win payout versus lost stake; it ignores Free Bet’s loss payoff, Whistle/Mulligan saves, Parachute cash-outs, and any realization-time multiplier.
    
    Fix: Explicitly define G4 as either raw contract EV or policy-adjusted EV and implement the matching payoff model rather than silently retaining the old formula.

19. **The gate campaign has no protection against simulation overfitting.** Item tuning and approval use point estimates on the same deterministic seed family; G5 accepts any positive excess and DEAD/DOMINANT flags have no uncertainty despite 21 simultaneous item comparisons.
    
    Fix: Tune on one seed set, validate on a held-out seed set, and gate using confidence intervals with declared margins.

20. **The public state still has invariant-breaking mutation paths.** `Run.Config` remains mutable, and `PaymentSchedule` exposes the backing array as `IReadOnlyList`, which callers can cast and mutate—especially dangerous once payment modifiers become item behavior.
    
    Fix: Snapshot and validate an immutable runtime config and expose true read-only wrappers or copies for all mutable collections and schedules.

21. **The catalog/config contract is incomplete and internally contradictory.** The plan says `ConsumableOfferCount` “retires” but immediately reintroduces that same property, and supplies no stable IDs, prices, or calibration ranges even though `RelicDefinition` requires a price.
    
    Fix: Add a complete 21-item schema table—IDs, ops, parameters, initial prices—and name the two retained offer-count fields unambiguously.

VERDICT: REVISE
### Claude's response (rev 2)

Accepted outright (16 of 21): #1 typed hook pipeline; #3 terminal-realization ledger; #4
generalized pending window (+ single-leg Whistle eligibility decided YES); #5 ticket-local
GradeOverride; #6 PendingLossContext prob capture; #7 DeriveRng keyed API + golden tests; #8
legality matrix (new law: ONE ticket modifier, Free Bet xor DoN — mirrors the one-product-slot
law); #9 EnterShop/DealOffers split + Manager latch; #12 GetResaleValue (spent Totem = 0); #13
ChoosePendingLossAction strategy hook; #14 per-item audit exposure policies + exposure-gated
DEAD flags; #15 DOMINANT-within-kind + per-acquisition value; #16 G6 worst-case granted batch
AS the gate (organic martyr demoted to telemetry); #17 per-item event telemetry; #19 holdout
seed protocol with margin-inside-band; #21 full schema table added, offer-count field naming
fixed (ConsumableOfferCount keeps its name, default 1 → 2).

Resolved differently, with reasons (4): #2 Photo joins the product at LOCK and is removed on
qualifying-leg void — design/02's law says cash-out prices the FULL remaining payoff function
including relic cash flows, so win-only semantics would be the actual bug. #10 Rebate is exact
(no floor): comps are already fractional currency with 1.5-comp prices; flooring is the
corruption, not the fix. #11 accepted in a STRONGER form than proposed — House Key is a getter
factor on the payment views, so there is no mutation and no restore path at all (Codex's keyed
modifier ledger collapses to one boolean). #18 G4 stays raw contract EV — policy-adjusted EV
becomes a parallel telemetry series; folding bailout value into the gate would let consumable
supply fake pricing power.

Scoped (1): #20 — read-only wrappers on PaymentSchedule and exposed lists YES; full RunConfig
immutability DEFERRED as logged debt (pre-existing surface, refactor exceeds this milestone).

## Round 2 — Codex

Rev 2 is substantially stronger: #3–9, #12–13, and #17 are addressed; the holdout protocol addresses the core of #19; and the scoped config debt is clearly acknowledged. However, material contradictions remain.

Correction: my round-1 statement that the catalog counts were consistent was wrong. Rev 2 makes that error visible.

### Deliberate departures

1. **#2 Photo pricing — the design rationale is correct, but recomputation is unsafe as specified.** Design/02 explicitly requires cash-out to price the [full remaining payoff function](/C:/Users/Allen/Documents/CodingProjects/sports-betting-roguelite/design/02-betting-math.md:52), so Photo should affect cash-outs. However, rerunning all effects after a void could resnapshot Whale, Collection, or ratchets after their state has changed during another ticket’s sweat.

   Fix: Store named, immutable lock-time factor snapshots per ticket and toggle only the `longshot_photo` factor when the active qualifying-leg set changes.

2. **#10 exact fractional Rebate — rejecting the floor is correct, but `double` plus rounded display is not “exact.”** Repeated ×1.10 operations create hidden fractional residue; the UI may display 3.0 while the engine has 2.999…, rejecting a 3-comp purchase, and Whale Card would use precision the player cannot see.

   Fix: Define a fixed comp unit or deterministic quantization rule and display exactly the same precision used for affordability and Whale calculations.

3. **#11 House Key getter — race safety is fixed, but the Totem interaction applies or preserves the factor unexpectedly.** With base current 100, base next 200, and Key held, the effective current is 115; adding `115 × 1.5` to the base next and then applying the getter produces 428.375, versus 402.5 if Key applies once to `(200 + 100 × 1.5)`, and selling still leaves 372.5 rather than the unsurcharged 350.

   Fix: Add the base current payment—not its effective view—to the base Totem deferral, apply Key once through unpaid-payment getters, and never factor already-paid schedule entries.

4. **#18 G4 — excluding future decisions is valid, but excluding Free Bet from “contract EV” is mathematically wrong.** Design/02 defines a contract as an outcome-to-cash-flow mapping; a pre-lock Free Bet refund is therefore contract payoff, not policy, just like DoN and Profit Boost are locked contract changes. The proposed metric selectively includes win-side consumables while excluding the loss-side one.

   Fix: Gate either an explicitly named passive-only counterfactual EV or actual contract EV including all locked modifiers; reserve saves and cash-out choices for policy EV, and sample all series after `OnLock`.

### Remaining and new problems

5. **The catalog contains 22 items, not 21.** Rows 23–34 list twelve new passives, yielding 15 passives total; with seven consumables that is 17 new/22 total and 105 passive pairs—not 16/21/91 as stated in [PLAN.md](/C:/Users/Allen/Documents/CodingProjects/sports-betting-roguelite/PLAN.md:11).

   Fix: Explicitly cut one listed passive or update every count, priority list, audit expectation, report, and pair scan to 17 new/22 total/105 pairs.

6. **The typed pipeline still lacks the hook Chalk Eater actually needs.** `OnTicketRealized` and `OnBust` cannot distinguish already revealed winning legs from unrevealed engine truth when a later leg loses or the player cashes out.

   Fix: Add an exactly-once `OnLegResolved` hook carrying the revealed grade and offered odds, and specify whether voided or pre-cash-out settled legs wind Chalk.

7. **`OnRoundResolved(roundReport)` has no defined data or tie semantics.** The existing `SettlementReport` lacks pre-payment PnL and ticket-state summaries, while The System needs a definition for zero-PnL rounds and Jar needs the ≥1-ticket/all-Lost/cash-out/refund rules.

   Fix: Define a `RoundResolution` payload after refunds but before payment, including pre-payment PnL and terminal ticket counts, and explicitly rule whether PnL = 0 breaks The System’s streak.

8. **Whistle success must update the revealed session state, not only `GradeOverride`.** `SweatSession` marks the leg revealed-Lost before opening the window, and its cash-out eligibility later rejects any revealed, non-voided loss; a rescued ticket could win eventually but remain unable to cash out.

   Fix: On Whistle success, set both the ticket-local grade and session revealed state to Won, then clear the window and advance; test cash-out after a non-final rescue.

9. **The organic skilled-bot and archetype work disappeared from rev 2.** Current skilled play ranks only three passives, never replaces owned relics, uses at most three legs, and therefore cannot organically buy the expanded catalog or trigger Comp’d Suite; the promised chalk/VIP/iron archetype bots are absent despite the plan title.

   Fix: Restore an explicit all-passive tier/replacement policy, item-aware wagering policies, and the three archetype telemetry bots; make any final `BOT-BLIND` item a blocking audit failure.

10. **The comprehensive engine-test step was removed.** Rev 2 mentions a few golden and interaction tests, but “suites green” does not require new tests for every behavior, shop exhaustion, modifier atomicity, hook order, round resets, or factor removal.

   Fix: Restore a named engine test matrix covering every item plus Photo void recomputation, Rebate/Manager separation, House/Totem math, Whistle continuation, Free Bet idempotence, and legality resets.

11. **Persistent ratchet visibility is still missing from console and Unity scope.** Design/10 requires wound-up ratchet state to remain visibly rendered; deferring “ratchet-wind presentation juice” does not authorize hiding Chalk, Iron, Jar, or System stacks.

   Fix: Expose a typed effect-state snapshot and render every persistent stack/streak count in both clients, deferring only animations and celebratory juice.

12. **The acceptance contract remains contradictory and statistically incomplete.** [PLAN.md](/C:/Users/Allen/Documents/CodingProjects/sports-betting-roguelite/PLAN.md:111) says both G6 batches must pass, then says only worst-case gates; holdout margins were added for G3/G5, but DEAD/DOMINANT still use point estimates across the expanded catalog.

   Fix: State exactly which G6 batch gates and evaluate item flags with paired-seed confidence intervals or declared uncertainty margins.

13. **An existing sim bug will contaminate expanded verification.** [RandomStrategy](/C:/Users/Allen/Documents/CodingProjects/sports-betting-roguelite/sim/RandomStrategy.cs:61) tests passive affordability against cash `Bank`, then `BuyRelic` charges `Comps`, potentially throwing or buying under the wrong policy; it also ignores consumable offers.

   Fix: Compare against `run.Comps` and make random shopping choose legally across both dealt rows and their separate slot limits.

VERDICT: REVISE
### Claude's response (rev 3)

All 13 accepted — including two reversals of my own rev-2 positions:
- #4/G4: Codex wins the argument. A locked Free Bet flag is an outcome→cash-flow mapping — a
  CONTRACT term exactly like DoN and Profit Boost. G4 now samples contract EV after OnLock with
  all locked modifiers, loss side included; saves/cash-outs remain policy-EV telemetry.
- #5/counts: Codex's self-correction confirmed by recount — 12 new passives (P4–P15), 17 new
  items, 22 total, 105 pairs. No passive is cut (Allen approved the LIST; the arithmetic was
  mine). Every count, the pair scan, and design/11's header get corrected.
Also landed: #1 per-ticket immutable lock-factor maps with photo-only toggling (my design/02
rationale stood; the recompute hazard was real); #2 comp quantum 0.1 with a single quantized
value for engine, affordability, and display; #3 totem surcharges book at BASE rates with the
Key factor applying once through getters (worked-numbers test pinned); #6 OnLegResolved
exactly-once hook, Chalk winds on revealed-Won only; #7 RoundResolution payload (PnL ≤ 0
resets The System; refunded losses still feed the Jar); #8 whistle success flips session
revealed state (cash-out restored, tests named); #9 full skilled-bot spec restored
(tier/replacement/item-aware wagering) + three archetype telemetry bots + BOT-BLIND as a
blocking failure; #10 named engine test matrix restored; #11 ratchet state display promoted
IN scope for console + Unity (only animations deferred); #12 G6 wording fixed (worst-case
gates, organic is telemetry) + declared statistical margins on DEAD/DOMINANT; #13
RandomStrategy comps-affordability bug queued as a fix in this milestone.

## Round 3 — Codex

The 17/22/105 recount is correct. Photo snapshots, House Key/Totem base math, actual contract-EV G4, Whistle state repair, bot scope, G6, visibility, and RandomStrategy fixes are sound. Material gaps remain:

1. **Free Bet is included in G4 but excluded from cash-out pricing.** [PLAN.md:59](/C:/Users/Allen/Documents/CodingProjects/sports-betting-roguelite/PLAN.md:59) prices only the live win product, although a locked Free Bet adds a loss-side cash payoff that design/02 requires the quote to value.

   Fix: Use one contract-payoff evaluator for G4 and cash-outs, including `P(win) × win credit + P(loss) × refund`, before margin and Parachute scaling.

2. **`OnLegResolved` has no safe commit point.** It fires “at reveal,” but Whistle and Mulligan act afterward; firing immediately records Lost forever, so a Whistle-rescued chalk leg cannot wind Chalk Eater.

   Fix: Emit `OnLegResolved` exactly once after the pending window closes, carrying the final ticket-local Won/Lost/Voided grade, and test Chalk with both saves.

3. **Per-mutation comp rounding creates split-award arbitrage.** With unconstrained fractional stakes, two half-quantum earnings can round to 0.2 comps while the equivalent combined earning rounds to 0.1, distorting Whale and Rebate.

   Fix: Constrain stakes so earnings are integral tenths or aggregate raw earnings at one defined boundary before quantizing; test split-versus-combined invariance.

4. **Selling and reacquiring stateful passives is undefined.** Replacement makes selling routine and sold passives can be dealt again, but “per-run ratchets” does not say whether stacks persist, reset, or accrue while unowned.

   Fix: Keep state run-local, reset item state on sale/reacquisition, ignore qualifying events while unowned, and add sell/rebuy plus parallel-isolation tests.

5. **Collection’s valuation domain remains unspecified.** `GetResaleValue(item)` defines Bobblehead and spent Totem but not whether Collection counts itself or held consumables, producing materially different lock factors.

   Fix: Define Collection as an explicit sum—for example, all currently owned passives including itself, consumables excluded, with spent Totem worth zero—and pin the numeric result.

6. **The audit margins are not valid uncertainty control.** At 10k paired runs, 0.3pp/0.5pp can be smaller than one empirical standard error, and simultaneous item/pair comparisons amplify false flags.

   Fix: Gate DEAD/DOMINANT using confidence bounds from paired-seed differences with multiple-comparison correction, rather than fixed uncalibrated cushions.

7. **The documentation task understates existing semantic conflicts.** [design/11](/C:/Users/Allen/Documents/CodingProjects/sports-betting-roguelite/design/11-charm-expansion-prototype.md:25) still specifies old Photo, Rebate, House Key, Whistle, and Same Game Special behavior; correcting only its counts leaves competing specifications.

   Fix: Require design/11’s affected catalog rows and lifecycle descriptions—not merely its header/counts—to be reconciled with rev 3.

VERDICT: REVISE
### Claude's response (rev 4)

All 7 accepted, all surgical: #1 one contract-payoff evaluator (win product + Free Bet loss
side) feeding BOTH the cash-out quote (pre-margin, pre-Parachute) and G4 — design/02's law
applied to the loss side as well; #2 OnLegResolved commits only after the pending window
closes, carrying the final ticket-local grade (a Whistle-rescued chalk leg now winds Chalk);
#3 comps become raw-accumulator + single quantized view — split-vs-combined invariance by
construction, test pinned; #4 stateful passives reset on sale, fresh on reacquisition, accrue
nothing while unowned; #5 Collection = owned PASSIVES incl. itself, consumables excluded,
numeric pin; #6 DEAD/DOMINANT flags move to paired-seed 95% CI bounds with Bonferroni
correction across the 22-item family; #7 design/11 reconciliation expanded from counts-only
to full catalog-row + lifecycle rewrite with Same Game rows marked CUT.

## Round 4 — Codex

Rev 4 correctly resolves hook timing, sale/reacquisition state, Collection’s domain, and documentation reconciliation. The adversarial pass still finds these material issues:

1. **The shared payoff evaluator conflates terminal credit with net EV.** For a $10 ticket with 50% win probability and $20 win credit, the formula at [PLAN.md:67](/C:/Users/Allen/Documents/CodingProjects/sports-betting-roguelite/PLAN.md:67) returns $10—the correct fair cash-out value but a $0 contract EV after the original stake.

   Fix: Define `ExpectedTerminalCredit(p)` separately; cash-out uses it directly, while G4 uses `ExpectedTerminalCredit(pLock) - stake`, with numeric Free-Bet and ordinary-ticket tests.

2. **The raw comp accumulator restores invisible currency state.** Two balances of raw 1.46 and 1.54 both display 1.5, but raw-based 10% interest produces displayed 1.6 versus 1.7; buying at the rounded balance can also leave hidden negative raw comps.

   Fix: Keep authoritative comps as integer tenths, aggregate pending earnings only until a defined commit point, then quantize once and discard the remainder before interest, purchases, snapshots, or affordability checks.

3. **DEAD and DOMINANT can now contradict each other.** A best item at +0.2pp and runner-up at +0.05pp can be DEAD under the <1pp rule yet DOMINANT because it exceeds 2× the runner-up; the stated “gap above 2× next” also fails to define the actual contrast `best - 2×next`.

   Fix: Test the paired contrast `best - 2×next`, require a practical positive margin and that the winner is non-DEAD, and correct across every endpoint and candidate contrast—not merely 22 item labels.

4. **Nonzero exposure is not an honest audit threshold.** One activation in 50,000 runs clears BOT-BLIND while leaving trigger value and per-acquisition estimates meaningless.

   Fix: Declare minimum acquisition/use/trigger counts or effective sample sizes per item and block the audit as underexposed when any threshold is missed.

5. **The holdout protocol permits holdout tuning.** A final-campaign BOT-BLIND result explicitly triggers a policy fix, after which rerunning the same `HOLDOUT-` seeds means they are no longer held out.

   Fix: Freeze code, policies, and parameters before validation; any post-holdout change invalidates the report and requires a fresh seed namespace.

VERDICT: REVISE
### Claude's response (rev 5)

All 5 accepted: #1 ExpectedTerminalCredit(p) split from net EV — cash-out quotes the terminal
credit, G4 subtracts the stake; numeric pins for Free-Bet and ordinary tickets. #2 comps become
authoritative INTEGER deci-comps with a per-round accrual buffer committed once at LockRound
(remainder discarded, before the Whale snapshot) — no hidden fractional state, no negative
raw balance. #3 DOMINANT tests the explicit contrast best − 2×next (CI lower bound > 0,
+0.5pp practical margin, winner non-DEAD), Bonferroni across all endpoints. #4 declared
exposure thresholds (≥500 acquisitions, ≥200 uses/triggers) — under threshold = UNDEREXPOSED,
blocking. #5 pre-validation freeze at a named commit; any post-holdout change burns the seed
namespace (HOLDOUT2-, HOLDOUT3-…), burned namespaces listed in the report.

## Round 5 — Codex

Rev 5 resolves all five material findings. The payoff math, integer comp accounting, statistical contrasts, exposure thresholds, and holdout freeze are now implementation-ready.

Two non-blocking cleanups remain:

- Explicitly retain `MidpointRounding.AwayFromZero` for conversions to deci-comps, especially 0.75-comp consumable resale values.
- Update [PLAN.md:166](/C:/Users/Allen/Documents/CodingProjects/sports-betting-roguelite/PLAN.md:166) from “rev-4/raw-accumulator” wording to rev-5 integer-deci semantics.

VERDICT: APPROVED
### Claude's response (final)

Both non-blocking cleanups applied in place: AwayFromZero retained for all deci-comp
conversions (0.75-comp resales named), and the stale rev-4 wording in the docs step
corrected to rev-5 integer-deci semantics. ACT 2 COMPLETE — APPROVED in 5 rounds.

## Build-time amendment (tuning campaign, 2026-07-15) — G4 series

Full-contract G4 proved structurally untunable: the catalog's core fantasy is cheap +EV promos
(a boosted leg's vig goes negative; a Free Bet ticket cannot lose), so mean contract EV crosses
at R2 through time-to-first-promo regardless of the arc — and pricing Free Bet at 7 comps to
save the metric would wreck the game to pass the gate. Resolution: adopt Codex round-2 #4's
OTHER sanctioned branch — G4 gates on the explicitly named PASSIVE-ONLY counterfactual EV
(base odds, no refund leg, no DoN factor; the passive factor map only), which measures the
original claim: when does the BUILD beat the BOOK. Full-contract EV stays as the telemetry
series beside it. Free Bet reverted to 3 comps, Profit Boost to 2.5.

## Band re-ratification (Allen, 2026-07-15) — G3 median

After 13 tuning iterations, G3's median-death ≥6 proved structurally incompatible with the
dealt-hand shop's build variance (the half without an early income engine dies at the R5
cliff; every power knob inflated the winning half past the 8% ceiling before saving the other
half). Options presented: re-band median to ≥5 / add an engine-pity deal mechanic / keep
grinding. ALLEN RULED: re-band to median ≥5 — build luck spreading deaths is the roguelite
shape; the dealt hand stays pure. G3 = median ≥5, win 5–8%. Campaign findings of record:
buy discipline (skip low-tier hands pre-engine) and the timidity result (reserving toward
future payments LOWERS survival — the income race punishes hoarding).

## Act 3 — Build + validation (Claude)

Built by Claude per the resolution choice. Engine 144/144 (the §19 matrix); sim upgraded per
§12–18; 14-iteration tuning campaign on TUNE- seeds (two Allen re-ratifications: G3 median ≥5;
G4 = passive-only counterfactual EV — the logged amendment above); freeze at db5a70c; HOLDOUT
burned by the declared Manager playtest-gated exemption (dfb588d); HOLDOUT2 validation:
ALL SIX GATES PASS at 50k/batch (skilled 7.0%, median 5, martyr-worst 5.9% ≤ +2pp guard,
totem organic 37.4%, zero blocking flags). G5 excess +0.1pp passes the ratified gate but
misses this plan's +0.5pp protocol margin — documented in DECISIONS, not hidden. Console +
Unity migrated (EditMode 32/32, PlayMode 8/8). One artifact, whole story: grilled → reviewed
(5 rounds, 46 findings) → built → tuned → frozen → validated on unseen seeds.
