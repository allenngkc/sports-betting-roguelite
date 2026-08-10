# Register entries — 2026-08-09, batch 22

**Seat:** Design Director (`main-2` terminal) · **Verdict docket on the built wave since batch 19.**
Source verified at the lane refs, not in `main-2`'s tree — all three commits are on lane branches and
`main` correctly still carries the old strings.

**Two grants from the record, one split, one refusal-to-rule-without-frames, one new finding.**

---

## S37-cl — GRANTED · CLOSED (af0c42c)

Verified at `af0c42c:SportsbookApp.cs` — the masthead subline is `ROUND {n} OF {rounds}`, and the
LEDGER site at :2278 carries the identical corrected string.

**No frame required, and this is a proportionality call rather than a gap.** The change deletes text
from inside an oversized, left-anchored box; nothing moves by construction, and batch 21 ruled §3.5
inapplicable *before* the build rather than after. Confirmed on the next SureThing set that exists
**for any other reason** — a capture window is not spent photographing a deleted string.

**Noted, not opened:** the run scope is hand-composed at **two** sites. Both took the fix, which is the
outcome S64 did not get. The *structure* that permits one to be missed is unchanged, and it is already
S67's inventory subject — recorded here as a second data point for that inventory, not as a new item.

**Recorded:** the commit cites the batch at `:98` and `:850`, including a note that the old comment's
quoted string is now historical. Comments that name the ruling they execute are how a build argues
back, and they are why this verdict took one `git show` instead of a shoot.

## T70 — NOT YET / SCORED — GRANTED (41d5cbe)

Verified at source: `l.ScorerRevealed ? "SCORED" : "NOT YET"`, asserted at three call sites, **plus a
negative guard** — `Assert.IsFalse(body.Contains("WAITING FOR"))`. The guard is the right instrument
and the right lesson (S66: an unasserted pin is a comment). A positive assertion proves the string is
present; only the negative one prevents the defect returning.

**The one-commit requirement is satisfied** — G1's forms and T70's lines land together in `41d5cbe`,
as ruled. Splitting them would have re-created the duplication and it did not happen.

`SCORED` admissible only at the causal identity payoff (`input.ScorerRevealed`) is a correct reading
of a clause the ruling did not spell out. Endorsed.

## T70-am — The standing check is an INFORMATION test, not a word-overlap test

**Amended — the seat's own defect (§1.5)** · DD 2026-08-09. Found while verifying T70's own commit.

T70 wrote the standing check as *"requirement above, state below, **no term repeated across the
pair**."* That is a **vocabulary** test standing in for an **information** test, and the difference
decides cases. In the same commit, BTTS's pair reads:

- requirement `BOTH TEAMS SCORE` · live `{n}/2 TEAMS SCORED` · resolved `BOTH HAVE SCORED`

Under T70 as written, `{n}/2 TEAMS SCORED` is a violation — it repeats TEAMS and SCORED. **It is not
one.** The count is genuine state: `1/2` tells the player something the requirement line cannot.
Meanwhile `BOTH HAVE SCORED` repeats the same two terms and is **much closer to the real defect** —
it is the requirement restated in the past tense, which is exactly what `WAITING FOR LANYARD` was.

**The test, restated:** *does the state line carry information the requirement line does not?* If yes,
shared vocabulary is fine and often correct — the pair should sound like one sentence. If no, the line
is furniture wearing a state's clothes, however different its words.

**Consequence:** AnytimeScorer's fix stands, unaffected. **BTTS's resolved line is a candidate under
the corrected test** and I am not ruling it from a diff fragment — it needs the pair seen composed
(§2.5). Note the parallel the build already drew and then didn't follow: AnytimeScorer's resolved state
is `SCORED`, not `LANYARD HAS SCORED`. The same economy applied to BTTS answers it without a ruling
from me.

**Recorded as this seat's error, not the lane's:** the build applied T70 exactly as written, to the
market T70 named. A standing check phrased as a word rule cannot be expected to generalise as an
information rule, and every other market's pair was authored under that phrasing.

## G1 — leg-statement FIT — NEEDS FRAMES

The strings are verified present at source. **That is not the claim in question.** G1's own deck put
`FitToColumn` above character counts and said the two at-budget forms were *measured, not assumed*;
T69's founding defect was rows wrapping to three lines against §5.1's fixed slots. A test asserting a
string exists cannot see a column overflow (§2.5).

**Shoot the at-budget forms specifically — the longest statement each market can produce.** A short
statement rendering cleanly proves nothing about the one that fits by 2px, and it is the only case
worth photographing.

## T68-am + T71 — NEEDS FRAMES. Not rulable from the record, deliberately

The suites are green and I am not ruling on them. Three reasons, each sufficient on its own:

1. **9.68:1 was COMPUTED, not measured.** T68 closed only when measured in linear on a rendered
   frame — and this seat's own display-encoded computation of the same quantity was recorded as its
   error in C33-am3. Granting a computed contrast ratio here would repeat the exact mistake that law
   exists to prevent, one batch after writing it down.
2. **C35's V8 clause is the load-bearing check, and it is a per-beat property no suite sees.** Route 2
   was chosen *because* the flood is a sine pulse rather than a field. Moving the treatment into the
   slot works **iff the slot's ground is static across the beat.** That is precisely what C35 requires
   an inverting element to report, and it is invisible to a green test.
3. **Both siblings must be shot, or the batch reproduces its own founding defect.** T68-am and T71 were
   ruled in one breath *because* measuring one payoff state and not its sibling is what produced T68.
   Shooting the cash-out beat and inferring WinBeat would be the same error with the same shape at the
   same seat.

**Required:** one frame set per payoff beat — cash-out accept and WinBeat — each reporting **rendered
ink vs its own ground in LINEAR relative luminance** (C33-am3), and **whether that ground is static
across the beat** (C35 / V8). Frame-locked arms; state the scope.

---

## Scheduling — one TV window, not three

Everything needing frames above is **TV, and it is one shoot**: the two payoff beats (T68-am, T71),
the at-budget leg statements (G1), and the composed requirement/state pairs including BTTS (T70-am).
Same surface, same rig, one seed-pinned flow (C34).

**Nothing here is urgent and nothing is blocked behind it.** The two grants land now; the rest waits
for a window that was going to open anyway.

## Open question to the SureThing lane — S71's new gate

Not a frame ask; a one-line answer. **What does the gate assert, and what can it not see (C18 §4.2)?**

I closed S71 on Allen's frame with the scope stated explicitly: string and placement confirmed, **ink
read qualitatively and not measured.** If the new gate also checks only the string, my stated gap is
now sitting behind something that reads green — which is the vacuous-green shape, created by my own
close rather than caught by it. If the gate covers the ink, it closes that gap and I will say so on the
record.
