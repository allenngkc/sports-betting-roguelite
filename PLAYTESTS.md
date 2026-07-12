# Playtest Log

Human playtests only (agent impressions live in OPEN-QUESTIONS). Newest first. Each entry: build, what happened, S-criteria signals, actions taken.

## #6 — Allen, 2026-07-11 (build: M4 betting loop, commit 43d7758) — MILESTONE REVIEW

**"Functionality everything is working nice! Really good for functional prototype."** Betslip click flow: feels good. Art/sweat/graphics headroom acknowledged for later phases (expected at graybox). Three findings, all landed same day:
1. **Records read as mystery numbers** ("3-6, 7-2 — what does these mean?"). FIXED: records now attach to their team in parens — "LIONS (3-6) @ SHARKS (7-2)" — with a "( ) = SEASON W-L" legend on the slate header; TV scorebug records got the parens too.
2. **American odds as the default** (+200 style). FIXED: new `OddsFormat.American` (display only, engine stays decimal) used on the slate buttons, slip legs, combined line, TV ticket cards and slip strip. EditMode-tested against book convention (2.00→+100, 1.87→−115).
3. **Can't see what's riding during the sweat** (stake at risk, legs, odds). FIXED: an always-on slip strip on the TV during the sweat — "RISK $125 → PAYS $312" plus every leg with its odds, colored by presented status (green W / red L / cyan VOID / white LIVE / dim pending). Status follows the presentation cursor, never engine truth, so baked outcomes never leak early.

Gate: M5 (phone bookie) awaits Allen's confirm on the fixed build.

## #5 — Allen, 2026-07-11 (build: M3 + review fixes, commit 47da286)

**"Overall feels much nicer now!"** — the #4 fixes (seated zoom framing, unmirrored text, bar inset) land. One finding:
1. **Seated look-around should be clamped (still free-feeling).** The clamp existed (±60° yaw / ±40° pitch) but was tuned for the unzoomed view — at 17° FOV it allowed 3+ screen-widths of swing, reading as no clamp. FIXED same day: seated limits tightened to ±12° yaw / ±8° pitch (a glance that keeps the TV in view), and seated mouse deltas scale by the zoom ratio (seatedFov/standingFov ≈ 0.25) so look speed stays constant in screen space instead of slamming the tight clamp in one flick. Both remain SitSpot dials.

## #4 — Allen, 2026-07-11 (build: M3 TV sweat, commit 162291c) — MILESTONE REVIEW

First sit-and-sweat in Unity. Verdict: **feel is right for a prototype** ("room for huge improvements in the final prototype" — expected and accepted at graybox fidelity). Three fixes, all landed same day:
1. **Seated framing: TV too far/small** — the view should hold just the TV. FIXED: the sit now zooms to a 17° seated FOV (TV fills ~85% of the view, slim room frame so the TvLight reaction still reads) and the seat anchor aims at the screen's exact center. FOV eases in/out with the sit/stand transition; `SitSpot.seatedFov` is a dial.
2. **TV text mirrored/unreadable** — the world-space canvas faced the couch with its +Z, showing its back face. FIXED: canvas +Z now points into the wall.
3. **Win-prob bar fill popped out of the TV** — the fill was offset by −barWidth/2 from a left-edge anchor (a center-anchor assumption), hanging 346px outside the screen. FIXED: 4px inset from the left anchor.

Gate: M4 (real betting loop) awaits Allen's confirm on the fixed build.

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
