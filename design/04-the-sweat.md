# 04 — The Sweat (signature moment spec)

The reason this game exists. Everything else can be adequate; this must be exceptional.

## The core technical insight: simulate drama, not sports

We do **not** build a sports simulation. We build an **outcome-first narrative generator**:

1. The headless engine samples the final outcome of each leg from true `p` (plus any live modifiers). Decided *before* presentation begins.
2. A **drama generator** then writes a plausible event sequence (score changes, momentum swings, injuries, "they're mounting a comeback") that *arrives at* the predetermined outcome.
3. The drama generator has explicit **pacing dials**: near-miss frequency, lead-change budget, when the decisive moment lands (late by default). Tension is directed, not emergent — which is better *and* enormously cheaper than a real sim.

Integrity rule: the drama never changes the sampled outcome (unless an Axis-4 relic explicitly and visibly does). Live `p` shown to the player is the honest conditional probability given events so far — the cash-out math stays truthful even though the storyline is authored. Players will reverse-engineer the system; it must be defensible.

This makes multi-sport support cheap: a "sport" is a reskin of the event vocabulary (touchdowns vs goals vs runs) over the same drama engine. v1 ships with 2–3 fictional sports for slate variety at near-zero marginal cost.

## Presentation: show the stakes, not the sport (PROPOSED 2026-07-07, pending Allen's sign-off)

Allen's worry: "build a parlay, wait, hit or not" is boring without footage. The answer is not simulating footage — it's fidelity to the real fantasy: **a bettor doesn't watch the game, he watches the app.** The live win-probability graph, the scorebug, the cash-out number — betting apps and ESPN have already trained the audience to sweat from exactly this interface. For our satire it's also the joke: the degenerate stares at the betslip, not the game.

Technical basis: the drama generator emits a **typed event stream** (`clock, event_type, score_delta, win_prob_delta, tension_tag`) — it is not a "text generator." Text is merely v0's renderer. The renderer ladder, same engine underneath:

1. **v0 — text ticker** (prototype): validates pacing and tension only.
2. **Shipped target — broadcast HUD:** animated scorebug, live win-probability graph drawing itself, crowd audio swells, shake on big events. ~~No field, no players — Balatro shows no poker table.~~
3. **Optional garnish — abstract momentum viz:** drive-chart / tug-of-war bar showing territory. Cheap, sport-reskinnable.
4. ~~**Full 2D sport simulation: CUT.** Content-pipeline trap; adds cost, subtracts focus from the stakes.~~

**SUPERSEDED 2026-07-18 (Allen, plan F_0.2.0 — the MATCH THEATER):** after nine playtests,
text is not enough for the near-final signature moment. Tier 3 matures into a **stage with
actors**: a top-down neon-on-black pitch where anonymous team-colored dots act out the beat
stream — the ball's field position IS territory, spatialized. This is *not* tier 4 returning:
the theater is a **renderer of the drama stream, never a simulation**, held by the theater
laws — (1) every staged moment is keyed by a `DramaEvent`; idle filler never signifies (it
restates the last revealed state, never implies new information); (2) presentation-local RNG
only; (3) causal presentation — goals commit on scene playback, suspended finals resolve from
the final ticket-local grade, every scoreboard increment maps 1:1 to a staged goal, live-lead
clamped to ±1 (the theater tells one-goal-game stories); (4) the stakes chrome stays dominant
— tier 2's scorebug/win-prob/cash-out frame the stage, and the palette law extends onto it
(no red team dots, no money-green pitch, VAR-disallow chrome never a red card). Scene
vocabulary is capped (~15 templates, ordered resolver); sports remain reskins — a stage
prefab + scene grammar per sport. Full spec: `docs/1-plans/F_0.2.0_match-theater-sweat.plan.md`.

**Multi-ticket presentation — "the wall":** the round's games display as a sportsbook-lobby wall of mini scorebugs; a **director system** cuts focus to the highest-tension moment (the RedZone pattern), driven by the drama generator's tension tags. This resolves the serial-vs-parallel question as a hybrid: parallel wall, directed serial focus. Engine requirement: drama generator emits tension scores; director is a presentation-layer policy over them.

**Player props (parked, v2):** "X to score 25+" bet types deliver the "praying for a guy" feeling at leg level. Event vocabulary must keep player entities addressable so props can attach later without engine surgery.

Anti-boredom invariant: the player is never purely waiting — the cash-out offer is a live decision held open through the whole sweat, and every event moves it.

## Mid-sweat agency ladder (PROPOSED 2026-07-07 — answers "is cash-out alone enough?")

Principle: **mid-sweat agency is a progression axis.** The verbs available *during* the sweat grow over the run, mirroring the EV arc (design/02). Repetition is defeated by progression changing what the sweat is, not by baseline busywork.

| Band | Player during the sweat | Verbs |
|---|---|---|
| 1 — mark | watches, prays | full cash-out only. The powerlessness IS the tension (thematically honest) |
| 2 — operator | intervenes | relic **active charges**: Timeout (pause drama, lock offer for 3 events), Ref's Whistle (veto + reroll one event), momentum boost; **partial cash-out** (real sportsbook feature — take 50%, let half ride); director remote in multi-ticket rounds |
| 3 — rigger | manipulates | stacked actives turn the sweat into a control panel — the "rigged it" fantasy expressed inside the signature moment |

Hard rule: **no QTEs.** Mid-sweat actions are options, never prompts; the sweat stays fully watchable hands-off. Required input converts tension into task.

**Progressive sweat density (Allen, playtest #1, 2026-07-08):** early sweats felt too fast to follow on first contact. For Unity: sweat *presentation complexity* should ramp with the run, mirroring the agency ladder — early rounds get shorter, simpler, slower-read sweats (fewer events, bigger type, one thing moving); lategame gets the full dense broadcast wall. This is also the onboarding mechanism: the player learns the sweat's grammar in Band 1 before Band 3 turns up the noise. Engine already supports it (DramaConfig event budgets can scale by round); it's a renderer + config concern, no architecture change.

Engine cost: near zero — active charges are player-initiated effects on the existing `OnMatchEvent` / `OnCashOutOffered` hooks.

v0 note: prototype ships cash-out-only *on purpose* (isolates the anticipation+one-decision hypothesis). Evaluation should record **when** sweat repetitiveness first appears (which run #) as design input for this ladder.

## Diegetic viewing (M3, 2026-07-11)

The sweat plays on the room's TV (design/08 diegesis), and watching is physical:

- **Sitting on the couch starts/resumes the sweat; standing pauses it mid-event, offer frozen.** Events only advance while you watch — the sweat is appointment viewing, not ambient. Not exploitable: outcomes are baked at lock and the offer only moves on events, so pausing pauses everything.
- **While a cash-out offer is live, Interact (E) accepts it** — the couch's stand-up is suppressed; standing during a live sweat is hold-move. The one-button cash-out stays law even seated.
- **The room is the reaction shot** (design/08): a TV-driven light washes the room green on a GREEN leg, red on a DEAD leg, gold on cash-out.
- Presentation consumes no engine RNG — the TV only calls MoveNext/CashOut; static, flicker and noise use presentation-local randomness.

## Presentation beats (v1 target)

1. **Ticket lock-in.** Stamp/receipt-print moment. Commitment device.
2. **Legs resolve sequentially, not in parallel.** Even if matches are "simultaneous" in fiction, we present serially — serial tension is the whole point. (Fictional framing: you're watching the multi-cast, one screen at a time.)
3. **Per leg:** ticker of drama events; live win probability bar breathing with each event; leg slams GREEN (juice crescendo) or DEAD (harsh cut, ticket visually burns at the corner).
4. **The cash-out counter** is on screen from leg one, ticking with live fair value minus margin. It must *taunt* — pulse when it hits round numbers, flash when it's about to drop. The decision is one button, always live.
5. **The final leg** gets 2–3× the drama budget: slower events, closer scores, the near-miss dial maxed.
6. **Settlement:** full payout celebration (confetti of cash, receipt tally rolling up) or the bad-beat sting (silence beat, then the satirical consolation — a guru tweet: "tough beat fam, tomorrow's picks in the channel 🔒").

## Pacing rules of thumb (tune via playtest)

- A 3-leg ticket sweat: **60–90 seconds at full density** (revised 2026-07-18 with the match
  theater — Allen wants time to watch the show and weigh the cash-out; the stage gives the eye
  somewhere to live, which text never could). Beats run 3–8s, tension-paced (Calm fast,
  Decisive slow); ledger-correction goals are separately timed sub-scenes on the final whistle.
  Early rounds are deliberately shorter: the engine's progressive-density ramp
  (`DramaConfig.EventBoundsForRound`) starts at 2–4 events/leg and reaches the full 3–5 band by
  round 3 — the onboarding mechanism from playtest #1, now engine-real. Never make the player
  wait without a decision or an event.
- Skippable? Fast-forward unlocks only after settlement of first N runs, and never during the final leg. (OPEN — anti-frustration vs Pillar 1.)

## Open questions

- ~~Serial vs parallel tickers~~ — superseded 2026-07-07 by the wall + director hybrid above (pending Allen's sign-off on the presentation proposal).
- Sound design direction — crowd noise? announcer barks? (Text-to-bark is cheap and funny.)
- Does live betting *during* the sweat exist in v1, or is cash-out the only live decision? (Lean: cash-out only in v1. Live betting is a v2 pillar candidate.)
