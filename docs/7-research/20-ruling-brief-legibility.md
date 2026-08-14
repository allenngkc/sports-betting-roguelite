# 20 — Ruling brief: legibility at the point of commitment

**Lane:** research · **Date:** 2026-08-13 · **Status:** RULING REQUESTED — one decision, one word
**Proposal:** RF-17 · **Sources:** `17` §6, and `05` `09` `10` `15` `16` beneath it
**Queues behind:** the retention word (`18`). This brief assumes nothing about how that lands — see §5.

---

## 1. The question

**The genre's dominant complaint is that players cannot steer against variance — do we answer it by
showing them the distribution they are committing against, and if so, does that law cover the shop as well
as the bet?**

---

## 2. The evidence, one line per source

**Method note:** the cell is the `luck_vs_skill` family's share of *that title's negative reviews* (`12` §1).
Buckshot has no such cell — its families are known-dead — and its row is qualitative from `05` §1.

| Source | What the player can see when they commit | Steering complaints |
|---|---|---|
| **Slots & Daggers** (`17` §1) | Runs built to be winnable by decision — *"every run is very winnable with good decision-making"* | **0 of 32 negatives — 0.0%**, the only zero in the study |
| **Sol Cesto** (`17` §3) | Store copy: *"choose one of four rows… **It's up to you to weigh the risks of each tile**"* — all four possible outcomes visible, the landing random | **1 of 41 — 2.4%** |
| **Buckshot Roulette** (`05` §1) | Live and blank shell **counts announced**, order hidden | qualitative; **48.8% of owners took the 50/50 voluntarily**, 92.2% positive |
| **Dungeons & Degenerate Gamblers** (`10`) | Deck outcomes opaque | **84 of 161 — 52%**, the highest measured |
| **Luck be a Landlord** (`09`) | Symbol pool opaque — *"there's no way I can see to increase your odds of getting specific symbols"* | **47 of 107 — 44%** |
| **Tharsis** (`17` §4) | Dice, nothing shown, no decision inside the roll | `luck_vs_skill` **38.8% overall — highest ever measured**; 70.1% positive, last in the study |
| **Insider Trading** (`15`) | A game *about* reading a market — and still 30.5% luck language, 2nd highest measured | **market legibility is not decision-point legibility** |
| **Nubby's Number Factory** (`16`) | Emergent physics decides the outcome | players attribute losses to the presentation — *"i always feel cheated"* |

**The pattern.** Six titles, four genres, two publishers in common with our references, and the split is
clean: **every low-complaint title makes the distribution explicit at the moment of choice; every
high-complaint title hides it.** None of the low-complaint titles gives the player more *control* —
Sol Cesto still lands you randomly. They give the player the ability to **price** the randomness.

---

## 3. Where SBR already stands

`02-betting-math.md`'s four-number model puts true probability, offered odds, stake and payout on the table,
and `00-vision` pillar 3 requires every mechanic be mathematically legible. **On bets, SBR is probably
already compliant** — the risk is whether the surface shows the maths at the moment of the decision, which
makes that half a UI requirement on `surething-design.md` and `tv-design.md`, not an economy change.

**The shop is the part that is not compliant.** `design/11` ships a dealt-hand shop: the player sees the
offer in front of them, not the distribution it was drawn from. **And that is precisely where the strongest
evidence sits** — Luck be a Landlord's 44% and D&DG's 52% are complaints about *item and deck streams*, not
about wagers. So the ruling turns on scope, and the scope question is the whole decision.

---

## 4. The options

| | Ruling | The law it adopts | Cost |
|---|---|---|---|
| **A** | **Adopt-wide** *(recommended)* | At **every** point where the player commits capital — bet **and** shop — the distribution they are committing against is visible on the same screen. Absorbs RF-12: the steering complaint is answered with visibility, not control. | UI + data. The bet half is likely already satisfied; the shop half is new surface work. **No economy change, no bounded-p exposure.** |
| **B** | **Adopt-narrow** | The law covers bet commitments only — the slip, the sweat, the cash-out. Shop legibility stays with RF-12. | Near zero, possibly already met. |
| **C** | **Audit first** | No law yet. Commission the gap list against `surething-design.md`, `tv-design.md` and `design/11`, then rule with a price attached. | One audit; defers the decision. |
| **D** | **Reject** | Pillar 3 already covers legibility; no second law needed. | Nothing. |

---

## 5. Recommendation: **A**

**The strongest evidence is not about bets.** LbaL and D&DG — the two biggest numbers in this study — are
complaining about item and deck streams. SBR's bets are the part that is *already* legible. A narrow law
therefore hardens what is not broken and leaves the actual gap open.

**Visibility is cheaper than control, and it is what the references actually bought.** RF-12's candidate
mechanisms are rerolls, banked picks, a wanted list, a guru who reads the shop — all economy changes.
Sol Cesto gets to 2.4% with none of them; it just shows you the four things that can happen before you
choose. That is UI and data.

**A is the right answer under every branch of the retention ruling** (§6), which B is not.

**The risk on A, stated plainly.** The references show the **immediate** distribution — Sol Cesto's four
tiles for *this* draw, Buckshot's counts for *this* round. A shop catalogue screen is not the same object:
*"here is everything that exists"* is not *"here is what this commitment is against."* If SBR ships a
catalogue and calls the law satisfied, it will be satisfied on paper and the complaint will survive.
**Adopting A means adopting it per-offer, not per-catalogue.**

**Two confounds, not resolved.** The low-complaint titles skew short — Buckshot 4.0h, Slots & Daggers 5.1h —
and a short game has less time to frustrate anyone; Sol Cesto (11.0h median, 28% over 20 hours) is the
counterweight and the only one of the three that is not short. And the widened probes rest on n=400 corpora
against the Tier-1 n=1,000.

---

## 6. The interlock with `18` — the reason this cannot wait long

| If the retention word is… | Then RF-17 should be… | Because |
|---|---|---|
| **B** — name the layer, ladder into v1, hold steering for RF-17 | **A** | B explicitly hands steering to this proposal. A narrow RF-17 would hand it back, and **steering would be owned by nobody.** |
| **A** — adopt fully, including a steering mechanism | **A** | Same target, cheaper mechanism. Build visibility first; it may retire the control mechanism entirely. |
| **C** or **D** — name-only, or reject | **A** | Then this is the only remaining answer to the genre's #1 complaint, and it is the one that costs no economy scope. |

---

## 7. Need Allen

**One word: A, B, C, or D.** Queues behind the retention word; nothing here changes that ruling's options.

**Falsifier, unchanged from `17`:** if SBR's surfaces already do this everywhere, the law is free and closes
on the spot. If they do not, the gap list *is* the work — and under A that list includes the shop.
