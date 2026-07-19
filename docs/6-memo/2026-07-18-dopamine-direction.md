# Dopamine direction memo — answering playtest #13's "could feel boring"

_Allen, 2026-07-18: "for CloverPit the dopamine is actively there because players get immediate
results after spinning... since this is a roguelite, many runs will be played. How do we ensure
this dopamine for our game?" Recorded per design/04's v0 note, which explicitly asked for the
run # where sweat repetitiveness first appears as design input for the agency ladder. This memo
is CONTEXT + candidate directions; anything adopted gets grilled into a plan._

## The diagnosis, in CloverPit terms (design/09)

CloverPit's dopamine loop has two gears we currently lack:

1. **Result cadence.** A slot spin resolves every ~3 seconds — stake → outcome → payout, complete
   micro-loop. Our smallest complete loop is a LEG (~15–25s) and the money truth only lands at
   ticket settle (60–90s). Everything between is anticipation with one standing decision.
2. **Number-go-up compounding.** CloverPit's payout formula explodes multiplicatively and the
   game SHOWS the explosion every spin. Our equivalent (Band 3 "sanctioned brokenness",
   design/02) exists in math but has no spectacle yet.

Two structural truths in our favor: sports betting's native dopamine is the *swing* (the live
number moving against your heart rate), and our satire thesis (design/04) is that the bettor
watches the APP, not the game. The show doesn't need to be football; it needs to make the
NUMBERS feel alive. Also: the boredom was partly *scheduled* — chrome v2's taunting counter and
the momentum tape are M-T4, audio (crowd as anticipation engine) is M-T5. Judge after those land.

## Candidate directions, ordered by leverage

### 1. Micro-markets: the cadence fix (the big one)
Corners, next-goal, "anytime scorer" player props — side bets that RESOLVE MID-LEG, every
10–20s. This is the true CloverPit-cadence equivalent: many small stake→result loops nested
inside the big sweat arc. The theater already stages corners-shaped events (near-misses,
territory); micro-markets make them payable. Engine cost is real (new bet types, settle hooks,
sim re-audit) — this is the "corners, player props" expansion Allen already suspects, now
justified as the DOPAMINE feature, not content filler. Candidate for the post-slice plan.
Design guard: micro-stakes must stay pocket change vs the parlay (the sweat stays the boss).

### 2. Chrome v2 + audio (M-T4/M-T5): make the number the slot machine
Already planned, now understood as the dopamine milestone rather than polish:
- Cash-out counter that TAUNTS (design/04 law): ticks per beat, pulses on round numbers,
  flashes before drops — loss-aversion pinball. The counter is our reel.
- Momentum tape: visible streak/heater texture; a hot leg should LOOK hot.
- Settle celebration with a rolling payout tally (cash confetti, receipt tally) and the
  bad-beat sting (silence → satirical consolation) — end-of-loop payoffs worth chasing.
- Crowd bed swelling with danger (M-T5): anticipation is mostly audio.

### 3. Mid-sweat agency ladder Band 2 (design/04, already ratified direction)
Decisions are dopamine. Active charges (momentum boost, the existing Whistle) give lategame
sweats verbs. The ladder was explicitly parked until "sweat repetitiveness first appears" —
it has now appeared (run: playtest #13).

**RULING (Allen, 2026-07-18): partial cash-out REJECTED as a baseline verb** — real books
don't offer it, and the book's mechanics hold to the same realism bar as the presentation.
It may return RELIC-GATED (a relic selling a non-real-book power is the established doctrine
— see Ask for the Manager); parked until the Band-2 work.

### 4. Spectacle scaling with Band 3 (number-go-up)
When a build gets broken (Multiplier stacks, payout ×N), the SHOW should break with it:
payout lines rolling up like an odometer during green legs, the TV light escalating, the
theater celebrating harder. Zero engine change; renderer reads existing payoff data.

### 5. Lategame density: the RedZone cut
Multi-ticket rounds currently sweat serially. Band 3's "director remote" (design/04) — cutting
between two live sweats picture-in-picture — multiplies events-per-minute exactly when the
run's stakes peak. Presentation-heavy, no engine change (sessions already exist in parallel).

## Suggested sequencing

M-T4 + M-T5 first (already planned; they ARE gear 2). Then partial cash-out as the cheapest
Band 2 rung. Then the micro-markets plan (engine + sim campaign — a real TRIP HIGH). RedZone
and Band-3 spectacle ride along wherever they fit. Playtest after each: the question to ask is
"did you ever feel pure waiting?" (the design/04 anti-boredom invariant, verbatim).
