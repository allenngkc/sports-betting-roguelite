# 21 — Canon-change proposals for the ratified pair

**Lane:** research · **Date:** 2026-08-13 · **Status:** PROPOSALS — routed to the orchestrator
**Ratified by Allen, 2026-08-13:** retention bundle **B** · RF-17 **A**
**Governance:** proposals, not edits. This lane does not touch `design/**` or `docs/design/**`
(`13` §1). Every change below is drafted text for someone else to land. CP-6 additionally may not be
docketed by this lane at all — see it for why.

---

## 0. What was ruled, in one line each

- **Retention = B.** The retention layer is named and protected; a difficulty ladder ships in v1;
  steering is handed to RF-17 rather than solved with an economy mechanism.
- **RF-17 = A.** The legibility law is adopted wide: at every capital commitment — bet **and** shop —
  the distribution being committed against is visible on the same screen.

**Six proposals follow.** CP-1, CP-2, CP-4 and CP-5 are executable now. CP-3 is blocked on RF-4 and is
written as a dependency, not an edit. CP-6 is a routing note, not a drafted change.

**A correction to our own citations, found while pulling anchors.** `18`, `19` and `13` all cite SBR's
*"ruled 5–8% win band."* **Canon has read 4.5–8% since 2026-08-08** — `10-economy-rework.md` §F line 137
marks the 5–8% figure SUPERSEDED, floor moved to 4.5% by Allen. The argument is unaffected (the
references' optional top rungs sit at 9.1–12.1%, above the band either way); the citations are fixed in
the same commit as this file.

---

## 1. CP-1 — Name the retention layer · `design/00-vision.md`, Design pillars

**Canon today** (line 15, unchanged by this proposal):

> 1. **The sweat is sacred.** Leg-by-leg resolution with a live cash-out offer is the signature moment.
> Nothing may make resolution instant or skippable by default. All juice budget flows here first.

**Proposed — add a fifth pillar** (recommended form):

> 5. **The retention layer is funded, not assumed.** The item economy, the difficulty ladder and
> collection are what bring a player back tomorrow; the sweat and the information axis are what hold
> them tonight. These are different systems, and the retention layer carries its own protected budget.
> (Added 2026-08-13, from the research lane's fifteen-title study: Buckshot Roulette produces the
> highest tension language in the set and the lowest compulsion language — 2.6%, at a 4.0h median;
> Insider Trading, the nearest shipped implementation of our information axis, returns 8% of the
> players who beat it against Balatro's 61%.)

**Smaller alternative, if a fifth pillar is too much canon movement:** append to pillar 1 — *"The layer
that brings a player back — item economy, ladder, collection — is a separate system from the sweat and
carries its own protected budget."*

**Recommend the fifth pillar.** The pillar list's own test is *"every feature must serve at least one; a
feature that fights one gets cut."* A ladder rung serving pillar 1 by way of an appended clause reads
oddly; a rung serving pillar 5 reads correctly. It also leaves pillar 1's *"all juice budget flows here
first"* untouched, which is the half nobody proposed changing.

**Does not change:** pillar 1's priority, the sweat, or any juice allocation.

---

## 2. CP-2 — The ladder ships in v1 · `design/01-core-loop.md`, Meta progression between runs

**Canon today** (line 27):

> Unlocks (new relics, bet types, leagues, guru roster) rather than power creep — Balatro model.
> **Ascension-style difficulty tiers post-v1.** (OPEN: how much meta is too much for scope?)

**Proposed:**

> Unlocks (new relics, bet types, leagues, guru roster) rather than power creep — Balatro model.
> **A difficulty ladder of two or three rungs ships in v1** (RULED 2026-08-13); deeper tiers remain
> post-v1. **Front-load it** — rung 1 is the largest step, and the gradient flattens after.
> (OPEN: how much meta is too much for scope?)

**Why two or three, and why front-loaded.** Ladder *presence* is what the evidence carries: every title
in the study with no ladder sits at a 3.3–7.6h median, every title with one at 9.1–25.1h. Ladder *depth*
is not the variable — Luck be a Landlord's twenty rungs reach the same endpoint as Balatro's short steep
ladder (12.4% vs 12.1%) and retain 2.4× less. LbaL's own gradient is the front-loading argument: −10.0
points on floor 1→2, decelerating to −0.7 by floor 20.

**Cost.** Config plus a gate campaign re-run, plus whatever surface names the rungs. This lane cannot
size the surface work.

**Note.** The parenthetical open question is deliberately kept — "two or three rungs" answers the ladder's
depth, not the scope question beside it.

---

## 3. CP-3 — The band dependency · NOT an edit; blocked on RF-4

**The ruling did not settle this and this proposal does not assume it.** `18` option A carried *"with the
base game below the ruled band"*; option **B**, which Allen ruled, carried the ladder only. So where the
*base* game sits is still RF-4's, and RF-4 is unruled.

**Canon today** (`design/10-economy-rework.md` §F, lines 136–138): skilled win target **4.5–8% per run**
(5–8% SUPERSEDED 2026-08-08, floor moved by Allen).

**The two variants, so the choice is visible:**

| | What it means | Cost |
|---|---|---|
| **(a)** Band unchanged | The base game stays at 4.5–8% and the two or three rungs sit *above* it — harder still. | Cheap. The ladder ships as CP-2 with no economy movement. |
| **(b)** Base moves up | The base game becomes materially easier and 4.5–8% describes a hard rung rather than the default. | **Not cheap.** G3 re-band, sim re-gate, gate campaign. |

**What the evidence says, without ruling it.** Balatro's base game is won by 71.7% of owners and its top
optional rung by 12.1%; CloverPit's escape is 30.9% and its ascension rung 9.1%. **SBR's default currently
sits at roughly where both references put their hardest opt-in content**, with nothing below it. That is
an argument for (b) — and (b) is an economy change that this lane has no authority to assume and Allen
has not been asked for.

**Recommend:** ship CP-2 under (a) so the ladder is not blocked, and rule (b) with RF-4 when that comes up.
Flagged rather than folded in, because folding it in would be scope this ruling did not grant.

---

## 4. CP-4 — The legibility law · `design/00-vision.md`, pillar 3

**Canon today** (line 17, abridged at the ellipsis):

> 3. **Every mechanic is mathematically legible.** The baseline bet is the four-number model (true
> probability, offered odds, stake, payout — see `02-betting-math.md`), but relics may rewrite the payoff
> function itself … The discipline: if we can't write down a mechanic's expected value for the Monte
> Carlo audit, it isn't designed yet.

**The gap this fills, precisely.** Pillar 3 as written is an **authoring** discipline — it guarantees *we*
can write the expected value down. It does not guarantee the **player** can see the distribution at the
moment they commit. Those are different properties, and the fifteen-title evidence is entirely about the
second one.

**Proposed — append to pillar 3:**

> **Legibility is player-facing too (RULED 2026-08-13, RF-17 A).** At every point where the player commits
> capital — a bet, a shop purchase, a cash-out — the distribution that commitment is made against is
> visible on the same screen, for that offer, at that moment. **This is per-offer, not per-catalogue:** a
> reference screen elsewhere in the UI does not satisfy it.

**Why the per-offer clause is load-bearing.** The reference implementations all show the *immediate*
distribution — Sol Cesto's four possible tiles for *this* draw (steering complaints: 1 of 41), Buckshot
Roulette's live/blank counts for *this* round (48.8% of owners voluntarily took the 50/50). Where the
distribution is hidden, the complaint dominates: 52% of D&DG's negatives, 44% of Luck be a Landlord's.
Without the clause, a catalogue screen satisfies the law on paper and the complaint survives.

---

## 5. CP-5 — The shop half · `design/11-charm-expansion-prototype.md`, The dealt-hand shop

**Canon today** (lines 25–29):

> Every shop visit deals **4 passives from the unowned pool + 3 distinct consumables from the 7** (fresh
> draw each visit; a purchased-ever Totem leaves the pool forever; short pools deal what remains).
> Scarcity comes from pool dilution — **a specific passive shows ~27% of visits**. … **Ask for the
> Manager** redeals the hand once per visit through a derived stream, so future visits are untouched.

**Two facts that make this cheaper than the brief assumed, both found in canon while drafting:**

1. **The number already exists.** *"~27% of visits"* is computed and written down. At the shop, RF-17 is
   largely a **surfacing** job, not new design.
2. **A bounded steering lever already ships.** RF-12 asked for *"a reroll, a banked pick, a wanted list…"*
   — **Ask for the Manager is the reroll**, once per visit, cost 1. Canon was further along than the
   proposal credited.

**Proposed — add to the section:**

> **Shop legibility (RULED 2026-08-13, RF-17 A).** The dealt hand shows, on the same screen as the offer:
> the pool each slot was drawn from and what remains in it. Pool dilution *is* the scarcity mechanism, so
> the dilution has to be readable — a player who cannot see the pool cannot price the draw, and pricing
> the draw is what the law is for.

**Risk, stated.** A visible rate is a number players hold you to. Bounded-p is not threatened — nothing
here changes any `p` — but expect *"it said 27%"* in the corpus if the display is read as a contract
rather than a rate. Sol Cesto's framing avoids this by showing **what can happen**, not **how often**;
that may be the safer surface and it is a design choice this proposal does not make.

---

## 6. CP-6 — The surface half · routing note, not a drafted change

RF-17 A also lands on `docs/design/surething-design.md` and `docs/design/tv-design.md` — the screens where
a commitment is actually made. **Those are Design-Director-owned register surfaces, and this lane never
dockets to the DD seat** (`13` §1; contract §3). So:

- This lane supplies **the law and the evidence** (CP-4) and stops there.
- **The gap list** — every commitment point on those two surfaces, checked against the law — is the work
  the ruling creates, and it must be commissioned through the orchestrator to the surface owners.
- Per `17`'s falsifier: **if those surfaces already satisfy the law everywhere, it is free and closes on
  the spot.** Nobody has checked. That check is the whole cost of RF-17 on the bet side.

---

## 7. What the pair closes

| | Status after 2026-08-13 |
|---|---|
| **RF-5** | **RULED** — ladder into v1, two or three rungs, front-loaded (CP-2). The base-game question splits out to RF-4 (CP-3). |
| **RF-8** | **RULED** — the retention layer is named (CP-1). |
| **RF-12** | **CLOSED by absorption.** Answered with visibility (CP-5), not an economy mechanism. Canon already ships the reroll it asked for. |
| **RF-14** | **ANSWERED** — the information axis's compulsion partner is the retention layer. Falsifier stands: re-pull Insider Trading in three months; one request. |
| **RF-17** | **RULED A** — CP-4, CP-5, CP-6. |

**Still open, unaffected by this pair:** RF-2, RF-3, RF-4, RF-6, RF-7, RF-9, RF-10, RF-11, RF-13, RF-15,
RF-16, RF-18, RF-19, and the fifteen play questions in `07`.

**The strongest unruled item is now RF-18** — agency inside the sweat, worth ~22 points of sentiment on the
Buckshot/Tharsis comparison, and `04-the-sweat.md`'s mid-sweat agency ladder has been PROPOSED and
unratified since 2026-07-07. It is not part of this pair and is not proposed here.

## 8. What none of this changes

- **The bounded-p doctrine** (`10-economy-rework.md` §E). Nothing above manipulates any probability.
- **Pillar 1.** Untouched, including *"all juice budget flows here first."*
- **The sweat, the cash-out, the information axis.** All retained; RF-14 adds a partner, it does not
  demote anything.
- **This lane's authority.** These are drafted proposals. Landing them is the orchestrator's routing and
  Allen's ratification, and the surface half is neither (CP-6).
