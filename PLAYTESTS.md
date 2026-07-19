# Playtest Log

Human playtests only (agent impressions live in OPEN-QUESTIONS). Newest first. Each entry: build, what happened, S-criteria signals, actions taken.

## #12 — Allen, 2026-07-18 (build: MT-3.1, commit d076bbf) — M-T3 MID-GATE REVIEW 2

Goals, defense sense: LIKED. Three findings, all landed same day (MT-3.2, Luna's first
sandboxed implementation run after the codex launcher fix):
1. **"Add regular time + stoppage time to know when the game ends."** DONE: pre-final
   minutes cap at 89'; a final sequence opens at 90' (structural — no outcome leak), each
   staged goal ticks 90'+n, the slam lands FT.
2. **"25% faster — slightly too slow."** DONE: SweatPacer.paceMultiplier = 0.75, one dial
   scaling every scene and correction sub-scene (stage playback included via
   TheaterStage.paceScale). Duration acceptance re-verified at the new tempo: median
   61.1s — the 60–90s per-sweat law now sits at its floor; any further speedup needs the
   band re-ruled.
3. **"Dots feel rigid/fake; midfielders don't defend."** DONE: per-dot reaction-lag
   personalities + independent wander clocks + top-speed caps (runners, not magnets);
   territory shifts ripple through the shape line by line (backs react last); the
   defending team's nearest dots engage the carrier goal-side (3 in scenes, 2 on
   breakaway chases, a mild single press in idle) while the rest hold the block.

Direction standing: the M-T3 gate bar remains "reads as a real match."

## #11 — Allen, 2026-07-18 (build: MT-3 scenes, commit 6f4473b) — M-T3 MID-GATE REVIEW

Score visibility from #10 confirmed fixed ("Nice that I see the score now"). Two crucial
findings, both fixed same day (MT-3.1, commit d076bbf):
1. **The chrome spoils the show.** Win-prob/cash-out repriced at the engine step, then the
   goal played ~4.5s later — "suddenly the winrate goes to 90%, only then my team scores."
   FIXED: causal reveal — the beat's chrome lands at the scene's payoff moment (the goal /
   the save / the whistle), never before. Cash-out ruling (Allen ratified the proposal):
   the market SUSPENDS while a scene plays (real-book behavior on a dangerous attack) and
   reopens at the reveal with the fresh price — no stale-price accepts, no spoiler price.
2. **Ball movement meant nothing.** Passes crossed teams, "passes to the goalkeeper and
   scores." FIXED: sticky possession (one carrier, teammate passes, visible interception
   turnovers), scene waypoints routed through the actual attacking dots, shots aimed at
   the corner away from the keeper (keepers only save or concede), defending team drops
   into a compact block, nearest defenders chase on breakaways, off-ball forward runs.

Direction ratified in the same conversation: the show must read as a REAL match —
formations, attacking sequences, defense. That is now the M-T3 gate bar.

## #10 — Allen, 2026-07-18 (build: MT-2 stage, commit 0cd53d1) — M-T2 GATE REVIEW

_(Numbering note: the F_0.2.0 plan labeled the M-T5 slice feel gate "playtest #10"; the
intermediate theater gates take the running numbers, so the slice feel gate shifts to
whatever number it lands on.)_

**"I like the graphic of the show"** — the neon stage look is ratified; the M-T2 slice's
deliberate gaps are the findings. Four notes, three actions:
1. **The puppetshow is too fast — can't follow what's going on.** Expected at this slice:
   M-T2 kept the text ticker's beat cadence. The fix is M-T3's SweatPacer (3–8s tension-
   driven beat-scenes). ACTION: proceed to M-T3.
2. **Moneyline legibility gap: which color is my team, and what's the score?** Team
   identity (names ↔ dot colors ↔ the picked side) and the running score must live on
   screen. The full scorebug is M-T4 chrome, but the minimal version (colored team names,
   pick marker, running score) is PULLED FORWARD into M-T3 — the score ledger logic lands
   there anyway.
3. **Legs resolve green/red with no goals seen.** Correct diagnosis of the slice: M-T2
   stages no scenes, so resolution has no on-pitch cause. M-T3's ScenePlaybook is the fix —
   goals visibly staged (buildup → shot → net), and the goal-playback invariant means the
   ledger can never move without one.
4. Stage graphics approved (the couch-readability half of the M-T2 gate).

Gate verdict: **look PASS, watchability deferred to the M-T3 gate** — the M-T2 questions
"dots hold territory coherently / nothing signifies falsely" were overshadowed by the
missing scene layer; they get re-asked at the M-T3 editor sweat.

## #9 — Allen, 2026-07-15 (build: charm expansion, commit 10b6135) — EXPANSION REVIEW

**"Just playtested the game, nice!"** — first hands-on with the 22-item catalog, the dealt-hand
shop, and the modifier/Marker/Whistle verbs. Two of the four standing questions answered:
1. **The strategy pillar LANDS.** "It feels like I am comparing which relic works nice with
   another relic, I got the strategy building process." That is the exact sentence this
   milestone existed to earn — building around the dealt hand reads as combo strategy, not
   as a vending machine. No changes requested.
2. **Ask for the Manager: KEEP.** "Keep ask for the manager I like this." The playtest gate
   resolves opposite to Timeout's (#8 cut it): the Manager's ≈0 bot audit stays permanently
   exempt as a HUMAN-AGENCY item — its worth is the choice, which bots can't monetize. The
   sim keeps reporting it as an ℹ note (never blocking), now labeled ratified.

Carried as standing checks for a future session (not answered this run):
- Do Free Bet and Golden Parachute feel like traps or tools? (Bots use them to under-win —
  insurance crowds out compounding. Watch, don't nerf.)
- Does the R5 payment cliff ($155 → $375) read as fair now that build luck spreads deaths?

## #8 — Allen, 2026-07-13 (build: economy rework in the room, commit 18d1b88) — ECONOMY REVIEW

**"I like the game loop"** — first hands-on with payments + comps + the 3+3 catalog in the room; "everything else good" beyond two findings, both landed same day:
1. **Timeout is useless — remove it.** Matches the sim audit (≈0 Δ across the board; it was
   playtest-gated and the playtest voted no). CUT end to end: catalog (now 3 passives + 2
   consumables), engine verb, console [T], TV [T]/HOLD hint. The engine's live-intervention
   seam (`ApplyLiveEffect`/`OfferHoldEffect`) stays, pinned by a seam test — future actives can
   buy the hold back if a design wants it.
2. **Totem fired "but didn't really give me any capital — my bank is still at $0."** Correct
   read: the old totem paid the shortfall by taking the whole bank, leaving a zombie round
   (can't even place the $10 min stake). REWORKED to full deferral: the payment is skipped, the
   bank is UNTOUCHED, and payment × 1.5 lands on the next one. Mercy that leaves you playing.
   Re-tuning fallout (sim-report-3.md, 50k runs/batch, ALL GATES PASS): the win-rate jump came
   NOT from the totem (+0.3pp audit) but from cutting Timeout — a 2-item catalog with 2 offer
   slots guaranteed a Mulligan Slip every shop. Knobs moved: consumable offers 2 → 1 per shop
   (draw scarcity restored), consumable slots 2 → 3 (bank a save for the cliff), mulligan
   2 → 1.5 comps. Skilled lands 6.2% (Allen's 5–8% band), organic totem fire rate 50%.

## #7 — Allen, 2026-07-12 (build: M5 bookie phone, commit 2ef5cae) — MILESTONE REVIEW

Phone thread readable, **"nice top-down view"** — the DeskFocus reuse call paid off. One finding:
1. **"E - Back" prompt + crosshair overlaid the thread while reading the phone.** The interactor's
   ray still hits the grab volume from the focused camera. FIXED same day: the overlay HUD hides
   entirely (crosshair + prompt) while any DeskFocus owns the camera — the E-toggle is learned
   from the pre-engage prompt and hold-move always backs out.
Note: buzz salience unverified — Allen's session didn't float. Standing check for a future run:
lose a round on purpose and see whether the desk-side blue blink registers from the couch during
the TV settle card (too subtle / right / distracting).

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
