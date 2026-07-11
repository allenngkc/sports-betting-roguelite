# Playtest Log

Human playtests only (agent impressions live in OPEN-QUESTIONS). Newest first. Each entry: build, what happened, S-criteria signals, actions taken.

## #3 — Allen, 2026-07-11 (build: M1+M2 graybox room, commit b9c3ea0) — MILESTONE REVIEW

First walk of the Unity room. Verdict: **M2 approved with two fixes.**
- Sit-down camera ease + seated TV framing: "loved it" — the signature camera moment lands.
- **Stand-up camera: rejected** — lerping back to the pre-sit pose swings the view away; FIXED same day: standing keeps the current look direction, travels position only.
- Room scale/movement: good, no wall clipping. Hover loop (crosshair/tint/prompt/pulse): good for prototype. Lighting: readable.
- **Mini fridge collided with the stool** — FIXED: moved to the door-end left corner (~1m left of spawn).
- Art expectations set: graybox approved as prototype; real art direction later per design/08.
- Gate: M3 (the TV plays the sweat) green-lit.

## #2 — Allen, 2026-07-10 (build: debt-as-HP patch, commit 6d36fd9)

**"Pressure is real that the run is dead."** The debt mechanic lands as pressure, not as a safety net — the exact feel question the patch had to answer, answered positive. No change requests. This playtest closed the loop on the Week 6 evaluation: verdict CONTINUE (DECISIONS.md).

## #1 — Allen, 2026-07-08 (build: Week 4 console, commit 4fca35f)

**Verdict signal: "I feel this is a fun game!"** First S2-relevant positive from a human.

Findings and actions:

1. **Max stake cap felt bad → LIFTED** (same day): stakes now uncapped to the whole bank; High Roller redesigned to an all-in payout bonus (+15% when staking ≥ half the bank). DECISIONS.md 2026-07-08.
2. **5/5 passive relics made the run feel too easy.** Design direction captured (design/03): split items into one-time-use **consumables** (e.g., single-use Lucky Charm, a "bet reset") vs permanent **passive skills**, separate slot pools, sell-back support. Implementation timing parked in OPEN-QUESTIONS (lean: after Week 5 sim exists, so the rebalance is measured, then re-run sim before the Week 6 verdict).
3. **Early sweats felt too fast / hard to follow at first.** No v0 change; logged the **progressive sweat** presentation principle for Unity in design/04 — simpler/shorter sweats early game, fuller drama lategame. Consistent with the existing mid-sweat agency ladder (Band 1 simple → Band 3 dense).

S-criteria notes: S2 leaning positive (fun signal on run 1; cash-out hover not yet reported). S1 counting starts now — log each voluntary run here.
