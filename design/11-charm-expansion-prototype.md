# 11 — Charm expansion for the combo prototype (DRAFT — awaiting Allen's picks)

**Goal (Allen, 2026-07-14):** 15–20 items so players make COMBO decisions — the first taste of the
stacking-strategies pillar (design/10 B2) before the 150+ fusion catalog. Prototype quality:
inspired by CloverPit's charm list (`clover-pit-charms-list.txt`); original designs come later.
Every item lands on an existing engine seam or a small named one; /sim gates re-run on the lot.

**The translation grammar** (how CloverPit reads in our four-number model):
symbols → odds bands (chalk ≤1.50 vs longshots ≥3.00) · patterns → parlays/leg counts ·
luck → FORBIDDEN as passive (bounded-p doctrine, design/10 E) — timed consumables only ·
interest/tickets → comps · 666/999 → cliff payments / monster wins · restocks → shop draws ·
Ankh → the Totem (already ours).

**Laws honored:** one PayoutMultiplier product slot — every payout passive is a ×(1+x) factor in
the product (composition stays superadditive, G5's evidence). Consumable timing never perturbs
the run seed (items needing RNG draw from dedicated `StableHash(seed#round#item)` substreams).
Scar's carrier/burn grammar is the ratchet precedent.

---

## Passives (11 new; 14 total; RelicSlots stays 5 → real slot pressure)

| # | Item | Inspired by | Effect (sim-tunable numbers) | Seam | Combo lines |
|---|---|---|---|---|---|
| P4 | **Chalk Eater** | Ace of Clubs, Midas Touch | Permanent ratchet: every settled WINNING leg at odds ≤ 1.50 adds +1pp to the payout product, forever. Never resets. | `OnTicketRealized`, count legs | Long chalk parlays feed it fast → Multiplier (3+ legs) + The System. The grinder's engine. |
| P5 | **Longshot Larry's Photo** | Golden Seven (scoped values) | Winning tickets containing ≥1 active leg at odds ≥ 3.00 pay ×1.6 extra. | realize-time check | **Profit Boost pushes a 2.4 leg over the 3.00 threshold** — the first explicit A+B→C combo. Free Bet covers the variance. |
| P6 | **Iron Hands** | Chastity Belt (gain on decline) | +4pp product per ticket that WINS at full ride (never cashed out). Any cash-out resets the stack to 0. | `OnTicketRealized` Won vs CashedOut | Anti-Parachute discipline build; loves Double-or-Nothing (cash-out disabled anyway). |
| P7 | **Golden Parachute** | Lucky Cat (interest payers) | Cash-outs pay ×1.08 (the book waives its margin — net ≈ fair value). | cash-out credit scale in `EffectEngine` | Paper-hands build; tension: cashing out burns Scar and zeroes Iron Hands. Mutually hostile with P6 — pick a lane. |
| P8 | **The Rake's Rebate** | Stonks (interest) | +10% interest on comps HELD at each shop open (floor). | shop-entry hook | Hoard economy with P9; fights every "spend comps now" instinct. |
| P9 | **Whale Card** | CloverPet, Lost Wallet | Payout product ×(1 + 0.5pp per comp held at lock). | read `Comps` at lock | P8+P9 = the VIP hoarder archetype: comps become a second bankroll you DON'T spend. Anti-shop tension. |
| P10 | **Bad Beat Jar** | Consolation Prize, Diesel Locomotive | +8pp permanent product per round where EVERY ticket you placed lost. Never resets. | settle-time round scan | Variance farming with Scar — the martyr axis (G6 is the watchdog). Longshot builds trip it naturally. |
| P11 | **House Key** | Evil Deal | All payouts ×1.4 — but every remaining payment +15% while owned (restored on sell). | product + payment array edit | The pure risk trade. Totem and Bookie's Marker are the safety net it demands. |
| P12 | **The System** (a three-ring binder) | Tarot Deck (streak ratchet) | +10pp product per consecutive PROFITABLE round; resets on a down round. | settle PnL check (gift counter's sibling) | The streak axis from the ratchet doc. Chalk builds sustain it; longshot builds can't. |
| P13 | **Comp'd Suite** | Chonky/Swole Cat (N-pattern payers) | A winning ticket with 4+ legs instantly pays +8 comps. | realize hook → `GrantComps` | Feeds P8/P9 hoards; pushes leg count with the Multiplier. |
| P14 | **Unopened Bobblehead** | Sardines | Does nothing. Sells back for 3× list instead of 50%. | sell-back override | Pure economy toy — exists for P15 and shop-flip lines. |
| P15 | **The Collection** | Dark Lotus, CloverField | Payout product ×(1 + 1pp per comp of total resale value of owned items). | inventory scan at lock | Full slots = power; selling anything hurts twice. Bobblehead is its favorite roommate. |

## Consumables (6 new; 8 total)

| # | Item | Inspired by | Effect | Seam | Combo lines |
|---|---|---|---|---|---|
| C3 | **Free Bet Token** | (design/10 D3, promoted) | Pre-lock, one ticket: stake refunded as cash if it loses. | flag at place, refund at settle | Longshot/Photo variance insurance. NOTE: the bust still feeds Scar and Bad Beat Jar — refunded martyrdom. G6 watch. |
| C4 | **Ask for the Manager** | D6, CrowBar (restocks) | Reroll the shop's offers (passives + consumable draw) once. | regenerate via `Rng.Shop`-keyed substream | The toolbox verb; makes 1-offer scarcity a decision instead of a coin flip. |
| C5 | **Double or Nothing Slip** | One Trick Pony, Ophanim | Pre-lock, one ticket: pays ×2 if it wins; cash-out offers never appear on it. | ticket flag; suppress offers | Iron Hands' best friend; Parachute's nightmare. Commitment as a purchase. |
| C6 | **Bookie's Marker** | Lost Briefcase (pays debt) | This round's payment −25% (played during betting). | edit `_payments[Round-1]` | The cliff-round valve; House Key's counterweight. Priced high — direct survival is the scarcest good. |
| C7 | **Ref's Whistle** | (design/10 D5, promoted) | Mid-sweat, player-fired: veto the event that just cratered your win prob; the drama generator re-samples it HONESTLY (may come out worse). | the intervention seam (design/05) — the one real engine build in this batch | The p-side moment design/10 E allows; pairs with big-slip saves alongside Mulligan. |
| C8 | **Same Game Special** | Angel's Hand (adds a pattern) | Adds a 7th matchup to this round's slate (fresh line to bet). | extra matchup from a dedicated substream — zero main-stream RNG consumed | More raw material for 4+ leg builds (P13, Multiplier); slate-reading skill. |

## The archetypes this creates (the decision space, pre-fusion)

- **Chalk grinder:** Chalk Eater + The System + Multiplier — long favorite parlays, streak preservation.
- **Longshot bomber:** Photo + Profit Boost (threshold combo) + Free Bet + Bad Beat Jar.
- **VIP hoarder:** Rake's Rebate + Whale Card + Comp'd Suite — never spend, always wager.
- **Iron hands:** Iron Hands + Double-or-Nothing — no exits, full rides.
- **Paper hands:** Golden Parachute + scar-burn timing + Ref's Whistle.
- **Shop flipper:** Bobblehead + The Collection + Ask for the Manager.
- **Leverage:** House Key + Totem + Bookie's Marker — buy power, finance the schedule.

## Open for Allen (then the sim campaign)

1. **Trim or ship all 17?** 15 = cut two of {Bobblehead+Collection pair counts as one line, Same Game Special, Comp'd Suite}.
2. **Offer draw at this pool size** (playtest #8's lesson, inverted): with ~14 passives and 8 consumables, the shop should probably show 3 passive + 2 consumable offers — pool dilution now does the scarcity work that offer-count restriction did at catalog size 5. Sim decides.
3. **Prices**: table numbers are sketches; the audit + G3 band re-tune them.
4. **G6 exposure**: three items feed on losing (Scar, Bad Beat Jar, Free Bet refunds). The martyr bot gets upgraded to farm all three before we trust the guard.
5. **Engine work beyond effects plumbing**: Ref's Whistle needs the drama re-sample (spec'd, unbuilt); Same Game Special needs slate append; everything else rides existing seams.
