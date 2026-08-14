# Register entries — batch 45

**Design Director** · 2026-08-12 · docket: the SGP lane's step-2 boundary question
(`docs/sgp/step-1-recommendation.md`, `bbd67e6`, Allen-approved).

**Destination table:** `S73` (new) → **SureThing — the laptop**. Slip construction is the laptop's;
`surething-design.md` is the owning document. TV consequence noted, not ruled.

---

## The question, and where it actually lives

The lane re-put it correctly: **not** *"correct pricing shows shorter odds and reads as cheating"* —
correct pricing **lengthens** 78.07% of two-leg tickets. The problem is **inconsistency**: the shapes
players most want to build are the ones that shorten (~32% at three legs, ρ median 1.63), while a
mixed corners-and-cards ticket lengthens. So no stable rule forms, and the only intuition available —
*related legs pay less* — is wrong for most tickets.

**There are two effects superimposed, and the surface currently shows only their sum.**

1. **Correlation.** Related legs add less risk than they appear to, so the ticket shortens.
2. **Vig.** The naive product charges margin once per leg; the joint charges it once. Everything
   lengthens.

Independent tickets get only (2) and read longer. Correlated tickets are dominated by (1) and read
shorter. **The player cannot learn the rule because he is shown one number containing both, and only
one of the two is something he can act on.**

That is the whole design problem, stated in its own terms.

---

## S73 — the ruling

### 1. A same-game ticket is its own instrument. It is never a parlay with an adjustment

**The surface never displays a product-of-legs figure for a same-game ticket, and never displays an
adjustment, a correlation discount, a "was/now", or any deduction line.**

This is the ruling that does the most work, so the reasoning is on the record: **the nerf reading is
manufactured by the comparison, and the comparison is manufactured by showing the product.** A
surface that prints

> `1.85 × 2.10 × 3.40 = 13.21` · correlation adjustment −32% · **SGP price 8.98**

has *literally rendered a number being taken away from the player.* No copy fixes that. There is
nothing to deduct from if nothing is presented to deduct from.

A same-game ticket is **one bet on one compound outcome**, and it has **a price** — not a product,
not a product corrected. That is also simply true, which is why it is the frame the product can hold
honestly.

### 2. The relationship is marked — in the house's own ink, which already means this

**Legs the model prices as related carry the house's mark.** Oxide is *the house acting on the
document* (S3, and S15's strike is its worked case); biro is *what the player chose*. The player picks
his legs in biro; **the house marks the connection between them in oxide.**

Nothing is invented here. This is the existing ink law doing exactly the job it was written for, and
it is the identity of the whole surface — *The Annotated Form Guide*. **A correlation IS an
annotation.** The machine noticing that two of your legs tell one story, and saying so on the
document, is the most in-character thing this product could do with the fact.

It also solves the learnability problem without a formula: the player's rule becomes **"legs the house
has marked pay less"** — which is true, visible, and actionable.

### 3. The lengthening is NOT remarked. No badge, no "better value", no flag

78% of two-leg tickets get longer and **the surface says nothing about it.** The price is the price.

A product that congratulates itself for charging less is exhortation in the register T27 and S45 both
closed, and it would be the house speaking in a slot where a fact belongs. **Silence here is not
concealment** — the price shown is the price charged, and nothing on the surface ever claimed it was a
product of legs (per clause 1).

### 4. The dial constraint on step 2 — this is the part that binds pricing, not presentation

**Any ticket priced materially shorter than the naive product must carry a relationship the surface
can NAME.**

If the model shortens a ticket whose relatedness cannot be marked, the player has been charged for
something he cannot see at the point of spending — **S17's class exactly**, and S17 is the law that
says an offer's cost is never hidden at the moment money moves.

**So the correlation model must expose a NAMEABLE RELATION, not only a scalar ρ.** A number that
moves the price without a statement attached is not presentable on this surface. Step 2's dial is
free in magnitude and **constrained in kind**: where it shortens, a reason must exist that can be
printed in one line.

Two consequences the lane should cost now rather than discover at step 5:

- The model's output needs a **relation label** alongside its joint probability — enough to compose a
  sentence, not enough to be a formula.
- Where the model finds correlation it cannot label, **the price does not move.** That is a real
  constraint on pricing and it is deliberate.

### 5. Impossible combinations: refused at the slip, in words, once

The 22 two-leg and 57 three-leg zero-probability shapes are blocked at construction (R1). The
presentation of a refusal is a design question and it has existing answers:

- **Not a disabled control.** S24 bans the disabled state on this surface outright; S56 bans a
  distinction carried in a channel the player cannot see.
- **The leg stays reachable on its own** — C19 is untouched, because the engine does price the leg;
  what it refuses to price is the *combination*.
- **The refusal states the fact, in the house's ink, once:** *these two cannot both land.* A statement,
  not an error, and not a scold.

**A bet that cannot win must never be purchasable.** That is not a pricing nicety — a price is a
factual claim about an outcome, and selling a finite price on an impossible event is the product
lying in the one place it has promised not to.

### 6. An implication leg says so. It does not silently change nothing

For the 22 implication shapes, adding the second leg changes the price by approximately the margin
alone. **A leg that lands on the slip and moves nothing reads as broken**, and it teaches a false
rule.

**Ruled: the surface states it** — *this adds nothing; [first leg] already contains it.* In the
machine's voice, as a fact. This is the same annotation channel as clause 2 and it is the most
characterful moment in the whole feature: the bookie's machine telling the player his second leg is
riding free.

### 7. No formulae on the face

T21's standing principle: a value appears when the sim emits it as a first-class value, **never
computed in presentation.** The joint price is emitted; the relation label is emitted; nothing is
derived on the surface, and no ρ, no multiplier and no percentage is ever printed.

---

## Coupling to Allen's call #1, named because the lane's four questions are not independent

The lane asks Allen whether the book should be honest, recommending yes and moving the exploit to
relics.

**Clause 2 is coupled to that answer: marking the relationship is what destroys the exploit as a
discoverable mechanic.** If correlation were meant to stay exploitable, the surface would have to
*hide* the connection — and then clause 4's problem returns with no remedy.

**The design side of Allen's decision, which the lane did not make and which I think settles it:** the
exploit only exists under *incorrect* pricing, and incorrect pricing here means **selling tickets that
cannot win, at finite odds, up to a mean decimal of 2070.70.** That is not a discoverable mechanic. A
price is a fact, and S45 is the standing law that satire never occupies a slot where a fact belongs.

**Recommend to Allen: price honestly; the exploit moves to relics** — where a relic that breaks a book
the player has learned to trust is worth far more than a bug he found in a book that was never
correct.

---

## Consequence, flagged not ruled: the instrument needs a name

If it is its own instrument (clause 1) it needs its own word, and **"SGP" is industry jargon** — the
surface's vocabulary law (S22) has the engine emit fields and the surface compose them, with the role
printed as a word. `SAME MATCH` is the obvious direction and it is a fact rather than a brand.

**Not ruled here**: player-facing vocabulary on a new instrument is material, and Allen sees material
naming (S16 and S46 are both his). Named now so step 2 does not bake `SGP` into strings that reach the
player.

---

## TV consequence — noted, not ruled

A same-game ticket's legs appear in the TV's ticket column during the sweat. **The relationship mark
must survive that trip or the two surfaces disagree about what the ticket is.** TV's leg rows are
ruled at T24/T40/G1 and its ink register is its own; how the mark renders there is a TV question and
is **not** answered by this ruling. Raised so it is scheduled rather than discovered — and it does not
block step 2.
