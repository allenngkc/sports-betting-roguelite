# 22 — Ruling brief: agency inside the sweat

**Lane:** research · **Date:** 2026-08-13 · **Status:** RULING REQUESTED — one decision, one word
**Proposal:** RF-18 · **Sources:** `17` §6 and §4, `05`, `16`, plus `design/04-the-sweat.md` read in full
**Queues with:** the `21` canon-change proposals. Independent of them — nothing here touches B or A.

> **Read §3 before §4.** Drafting this brief against canon changed the lane's own recommendation, and it
> turned up two errors in `17`'s statement of RF-18. Both are corrected below.

---

## 1. The question

**SBR's sweat already ships one live decision — so does a second band of mid-sweat verbs come into v1
now, or does canon's deliberate cash-out-only isolation stand, with a named trigger for when it ends?**

---

## 2. The evidence, one line per source

| Source | Agency inside the resolution | Result |
|---|---|---|
| **Buckshot Roulette** (`05`) | One decision class, made correct by the rules — self-shot on a blank retains the turn | **92.2% positive**; **48.8% of owners** took the 50/50 voluntarily |
| **Tharsis** (`17` §4) | **Zero** — dice, no decision inside the roll | **70.1% positive, last in the study**; `luck_vs_skill` 38.8%, `too_hard` 13.0%, `dread_tension` 6.0% — all the highest ever measured |
| — the delta — | Both are high-dread random resolutions; agency is the variable | **~22 points of sentiment** |
| **Nubby's Number Factory** (`16`) | Zero — a watched physics resolution | **97.4% positive.** The counterweight, and it is decisive for the law's shape — see below |
| **Balatro** (`02`) / **CloverPit** (`03`) | None during resolve | Both retain well; `dread_tension` 0.4% / 1.2% |
| **Insider Trading** (`15`) | A cash-out — *"only you can decide when to cash out"* | Its two most-upvoted positives are both catastrophes the player chose |
| **RACCOIN** (`04`) | None during the cascade | 81.6% recent — weak evidence, confounded by a telemetry incident |

**The law is not "dread without agency reads as unfairness."** Nubby has no agency inside its resolution
and is the second-best-liked title in the study — but its dread language is **0.4%**. Balatro and CloverPit
are the same shape. **Agency inside the resolution matters in proportion to the dread of the resolution**,
and it is Tharsis — highest dread, zero agency, last place — that pays for it.

**That refinement makes the finding apply to SBR rather than exempt it.** Pillar 1 commits us to a
high-arousal watched resolution by design, which is the regime where the variable bites.

---

## 3. What canon already says — the part RF-18 did not credit

`design/04-the-sweat.md`, read in full while drafting `21`:

- **The ladder is already designed, in three bands** (§Mid-sweat agency ladder, PROPOSED 2026-07-07):
  Band 1 *mark* — *"full cash-out only. The powerlessness IS the tension (thematically honest)"*;
  Band 2 *operator* — Timeout, Ref's Whistle (veto + reroll one event), momentum boost, **partial
  cash-out**, director remote; Band 3 *rigger* — stacked actives as a control panel.
- **The cost objection is already answered:** *"Engine cost: near zero — active charges are
  player-initiated effects on the existing `OnMatchEvent` / `OnCashOutOffered` hooks."*
- **The main risk is already fenced:** *"Hard rule: **no QTEs.** Mid-sweat actions are options, never
  prompts… Required input converts tension into task."*
- **Half the structure is already Allen-dated:** progressive sweat density (playtest #1, 2026-07-08)
  ramps presentation complexity *"mirroring the agency ladder"* and is called the onboarding mechanism.
- **The deferral is deliberate, not drift:** *"v0 note: prototype ships cash-out-only **on purpose**
  (isolates the anticipation+one-decision hypothesis). Evaluation should record **when** sweat
  repetitiveness first appears (which run #) as design input for this ladder."* The open question at the
  foot of the file leans the same way: *"Lean: cash-out only in v1."*

**Two corrections to `17`, both this lane's:**

1. **"Still unratified after thirteen months" is wrong — it is five weeks.** The ladder is dated
   2026-07-07; today is 2026-08-13. The error inflated the urgency argument and is fixed in `17` in the
   same commit as this brief.
2. **RF-18's framing over-claimed.** `17` called the ladder *"the difference between Buckshot and
   Tharsis."* It is not. Tharsis has **zero** decisions inside its resolution; SBR's Band 1 ships **one**,
   live throughout — canon's anti-boredom invariant is explicit: *"the player is never purely waiting —
   the cash-out offer is a live decision held open through the whole sweat, and every event moves it."*
   **On that comparison SBR is already on Buckshot's side of the line**, provided the one decision is real.
   Whether it is real is RF-7 — see §6.

---

## 4. The options

| | Ruling | What it commits to | Cost |
|---|---|---|---|
| **A** | **Band 2 into v1** | Ratify the operator band now — actives, partial cash-out, director remote. Band 3 stays post-v1. | Engine near zero by canon's own note; the cost is surface, content and tuning, which this lane cannot size. |
| **B** | **Isolation stands, with a trigger** *(recommended)* | Cash-out-only holds for v0/v1 as canon intends — but the evaluation note becomes a **named, owned gate**: the metric is recorded, and Band 2 lands when it trips. | A line of canon and an owner. |
| **C** | **Ratify the design, defer the scope** | Close the PROPOSED status — the ladder is canon — without dating it into v1. | Nothing now; removes a stale PROPOSED tag. |
| **D** | **Reject** | Band 1 is the game. *"The powerlessness IS the tension"* is thematically honest and canon already says so. | Nothing. |

---

## 5. Recommendation: **B**

**SBR is not the Tharsis case, so the 22-point argument does not license overriding a deliberate
experiment.** The comparison that produced RF-18 is between one-decision and zero-decision resolutions.
SBR has one. What the evidence supports is that the count must not be zero — and it is not.

**Canon's deferral is an isolation experiment with a stated hypothesis.** Shipping Band 2 into v1 now
destroys the thing it was set up to measure — whether anticipation plus one decision carries the sweat —
and this lane has **no play data on SBR at all**. Overriding a designed experiment on the strength of two
external titles is the same over-reach that cost RF-5 its strong form.

**B is cheap and it fixes the actual defect, which is that the trigger is not owned.** The v0 note says
evaluation *"should record when sweat repetitiveness first appears (which run #)."* Nobody is named, no
threshold is set, and no date is attached. **That is how a deliberate deferral turns into drift** — and it
is precisely what this lane mistook for drift when it wrote "thirteen months."

**What B should say, concretely.** Name the metric (run number at which sweat repetitiveness first
appears), name who records it in playtest, and state that Band 2 lands if it trips before a threshold.
**This lane cannot set the threshold** — that needs play data it does not have. Allen or whoever runs the
evaluation sets the number.

**The risk on B, stated.** If the metric is never actually recorded, B is D with better manners. The whole
value is in the trigger having an owner and a number; without those, rule **A** instead and take the
engine's near-zero cost.

**Why not A.** Only if you already believe the one-decision hypothesis has failed. Nothing in this study
can tell you that — it is a fact about SBR, and SBR has not been played enough to produce it.

---

## 6. RF-7's status changed while drafting this — and it is the load-bearing half

`06` RF-7 said the cash-out's arithmetic was *"not specified anywhere I can find"* and wrote its own
falsifier: *"If the offer is already specified this way somewhere I have not read, this is closed and I
would want the pointer."*

**Here is the pointer.** `04` §Presentation beats, item 4: *"**The cash-out counter** is on screen from leg
one, ticking with **live fair value minus margin**."*

**It is specified, and in the opposite direction from RF-7's fear.** The offer sits *below* fair value, so
riding is the higher-EV play by construction and cashing out is the −EV one. RF-7 worried the cash-out
would always be correct; on this line it is never correct **on EV alone**.

**Which is why the decision is still live, and why the constraint should be restated rather than dropped.**
SBR is not an EV-maximising game — it is survival-constrained. Under debt-as-HP (`01-core-loop.md`: *"Miss
*while in debt* and the bookie collects — run over"*), reducing variance to clear a settle can be correct
despite being −EV. **The real constraint is that cashing out must sometimes be right for survival reasons
in a named, reachable, reasonably common class of ticket states** — which is writable for the Monte Carlo
audit exactly as RF-7 asked, just in survival terms instead of EV terms.

**No ruling requested here.** RF-7 stays open, restated; it is not part of this brief's word. Flagged
because it is the thing that makes Band 1's single decision real, and B rests on that decision being real.

---

## 7. Need Allen

**One word: A, B, C, or D.**

If **B**, one thing more than the word is needed: **the threshold, or a name to set it.** B without a
number is not a ruling, it is a note.
