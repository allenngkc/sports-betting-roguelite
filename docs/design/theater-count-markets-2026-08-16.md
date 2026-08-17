# Theater for count markets — exploration

**Written:** Design Director seat, 2026-08-16 · **Mandate:** `dd-mandate-2026-08-16.md`, Phase 2 —
*corner-count tension building and booking drama* · **Status:** EXPLORATION. No spec, no ruling, no
material choice taken. Nothing here is buildable until Allen approves a direction.

---

## 0. One premise I am not yet treating as fact

The mandate's framing is that *"a corners or cards bet settles correctly but watches flat."* That is
almost certainly right and it is the reason this phase exists — but under C11 it is **a claim about
how something reads, and this seat has not read it on frames.** I am exploring against it because
exploration is what was asked for; I am not speccing against it until I have watched a corners
ticket sweat. Named so it is not later cited as measured. See §7.

---

## 1. Why a count market watches flat — the shape, not the volume

**A goal is punctuated. A corner is cumulative.** That is a difference in kind, and it is the whole
problem.

A goal is rare, instantaneous, and it changes **the surface's primary fact** — the scoreline moves,
and every element on screen that reads the score moves with it. The theater does not have to
manufacture significance; the event arrives carrying it.

A corner is one of eight to fourteen. On its own it means nothing. It means something only as the
**ninth**, and only against a line, and only on a ticket that named that line. **There is no moment
in a corners match that is intrinsically dramatic**, which is why treating a corner as a small goal
produces a surface that flashes fourteen times and says nothing.

**So the drama is not in the event. It is in the distance to the line, and it is in the clock.**

## 2. The central finding: the tension is a RATE, and the panel does not show it

`OVER 9.5 CORNERS`, with the count at 8:

| | |
|---|---|
| at 70' | comfortable |
| at 88' | agony |

**Same count. Same line. Opposite feelings.** The count alone cannot carry the tension, and neither
can the clock — **only the two together do.**

The COUNTS panel, as it now stands after the stats phase, shows the count. It shows it well: keyed
to the ticket, per-team, unrevealed rows marked *not yet*. **What it does not show is PACE.** It
tells the player where he is and never how fast he needs to go.

**The steal is from cricket.** A run chase is dramatically identical — a quantity that must reach a
threshold before a clock expires — and broadcast cricket solved it decades ago with the **required
run rate**: one number that fuses "how many more" and "how long left", moves continuously, and
climbs when things go badly. It is the single most tension-carrying object on a cricket screen, and
it is not an event at all.

**The diegetic form here is a sentence, not a number**, because the ticket column already speaks in
sentences — `LEADING 1–0`, `TRAILING 0–1` — and because a bare rate is a web-app object.

> `TWO IN TWELVE MINUTES`

carries tension where `CORNERS 8` does not, and it does it **continuously, between events**, which
is precisely the interval that currently watches flat. **The flat stretch is not the absence of
drama; it is the absence of anything that changes during it.** A rate line changes every minute.

## 3. Corners and cards are opposite problems and must not share a treatment

The mandate names both in one breath. They are not one job.

| | corners | cards |
|---|---|---|
| frequency | high (8–14) | low (0–5) |
| salience alone | ~none | **high** |
| what it needs | **accumulation made legible** | **the moment caught** |

A **booking is already dramatic** — a foul, a confrontation, a referee reaching into a pocket. It is
the one count event that arrives carrying its own significance, and it is closer to a goal than to a
corner. It needs the surface to *catch* it, not to inflate it.

A **corner needs the opposite**: it must not be inflated, because inflating it fourteen times is how
the surface becomes noise. Its treatment belongs in the accumulating line, not in a flash.

**Getting this backwards is the obvious failure mode**, and it is the one a single "count beat"
spec would walk straight into.

One consequence worth naming early: a booking lands on **two markets at once** — `TotalCards` and
`TeamTotalCards` — and on a ticket carrying both, one event moves two rows. The panel already has
the per-team split to express that; the beat has to not double-count the drama.

## 4. Valence is read off the TICKET, never off the event

**A corner is a threat to an UNDER and a gift to an OVER.** The identical event is good news and bad
news, and **only the ticket knows which.**

This is the ticket-keyed principle from batch 93 arriving in the theater. An event-keyed beat would
celebrate the thing that just killed his bet — **a state lie of exactly the `T103` class**, where
the screen asserts something the player's own position contradicts.

So: **no count beat may take its register from the event.** It takes it from the leg. The machinery
is already shaped for this — `_lastBeatUp` and `_lastBeatDelta` carry direction and magnitude, and
`_choreo.ResolveBeat` already receives the ledger.

## 5. The real cause of "flat": count legs die early, and often

This is the part I think the framing under-describes, and it may be the bigger half.

**An OVER that crosses at 60' has thirty minutes of nothing left.** It is won. Every subsequent
corner is confetti on a decided bet. **An UNDER that breaks at 60' is dead**, and the match plays on
for half an hour with nothing at stake.

Goals almost never do this — a match is rarely dramatically over. **Count markets go dead routinely
and early**, and a decided leg narrated as though it were live is worse than silence.

**So the theater's job is not only to dramatize the live leg. It is to HAND OVER when one dies.**
The ticket has several legs; at any moment one of them is the most in doubt. A surface that always
narrates the same leg will spend the last half hour narrating a corpse.

This also reframes the COUNTS panel's job: it is the natural place to express **which leg is still
live**, and it already has the rows.

## 6. The hardest beat, and we have already solved it once

**An UNDER wins by nothing happening.** Full time arrives, the count never got there, the bet is
good. There is no event to cut to — the win *is* an absence.

**Precedent exists and it is ours.** The goalless-draw work (batches 66–70) is this studio's solved
case of dramatizing an absence, and its answer was an authored terminal statement — `THE MATCH ENDS
LEVEL` — landing at L2 on the resolved scene rather than on a beat. `T97-am` then ruled the general
form: *the strip's words are licensed by the RESOLVED SCENE, never by the beat's own moment.*

**That ruling already covers this case and should not be re-derived.** An UNDER's win is the same
object: a terminal statement licensed by full time.

## 7. What already exists, so none of it gets reinvented

Read at source before writing any of the above:

- **Count events are already ranked above low-information beats.** `TheaterBeat` routes
  `nearMiss || spec.Count.HasValue` to `RevealBeatAudio`, and everything else to
  `RevealBeatChrome` — commented *"low-information beat: prob drifts, price stays live."* **Counts
  already reach the audio tier.** The gap is not that counts are ignored; it is that reaching the
  audio tier is all they do.
- `SweatFlavor.NeutralLine` / `NoGoalLine` is where an authored count line would live.
- `WonLegBeat` / `DeadLegBeat` already exist as terminal per-leg beats — §5's hand-over would hang
  off these rather than needing new machinery.
- The engine's `SelectionFamily.Corner` already groups the corner markets, and `MatchStatLine`
  carries home/away corner counts — the per-team split §3 needs is priced and present.

## 8. What this suggests, for Allen — five candidate beats

Exploration only. Each is a material choice and none is taken.

- **A · THE APPROACH.** At one from the line, the leg's line changes register. **Knowable in
  advance**, so unlike a goal it can be staged rather than caught.
- **B · THE TURN.** The count crosses the line. OVER wins, UNDER dies. **This is the count market's
  only genuinely terminal moment** and it should carry weight comparable to the goal flash — it is
  the one place a count market has a real event.
- **C · THE CLOSING CLOCK.** For an UNDER still alive, tension rises *as nothing happens*. The
  inverse ramp, and the thing §2's rate line delivers for free.
- **D · THE BOOKING.** Its own treatment, on §3's grounds — caught, not manufactured; and it must
  not double-count when it moves two rows.
- **E · THE HAND-OVER.** When a leg dies with time left, attention moves to the leg still in doubt.
  §5 argues this is the biggest single win available and it needs no new beat, only a choice of
  subject.

**My read, if it helps: §2 (the rate line) and §5 (the hand-over) are where the value is.** They
address the flat *stretches*, which is most of the match. A, B and D are moments, and moments were
never the problem — the ninety minutes between them were.

## 9. Owed before any of this becomes a spec

1. **Watch a corners ticket sweat on frames** (§0). The premise is almost certainly right; it is
   still a premise, and this seat has been wrong twice this week predicting instead of measuring.
2. Confirm on frames what a decided leg currently does for the rest of the match (§5) — I am
   inferring the corpse-narration from source, not from a screen.
3. Allen's direction on §8 before anything is specced.
