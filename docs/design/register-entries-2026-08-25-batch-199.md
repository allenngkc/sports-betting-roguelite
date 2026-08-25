# Register entries — batch 199 (2026-08-25)

**The seam is closed and the gate re-base is sound. The two-seed shape SATISFIES §5 — and now that
one seed is known to carry both, it should not be kept, for a reason this very commit demonstrates.**

**Two rows.** **Destination table:** TV (`T140-am7`, `T140-am8`).

**Build:** `7b28fa8`; counter fixture `2ff03a6`; route `route-c29-gate-rebase-2026-08-25.md`.
**Gate read at HEAD in the file, not from the diff. Nothing measured.**

---

## The rows

| T140-am7 | The seam and the gate re-base ACCEPTED — and the two-seed shape, though §5-compliant, is blind to the exact defect this commit fixed | **RULED — DD 2026-08-25 batch 199, on `7b28fa8`, built to batch 198 rather than 197.** **THE SEAM IS RIGHT AND ITS MECHANISM IS NOW STATED PRECISELY, which is the half I got wrong: `MarkPresentedResolved` sets flags and repaints nothing, **`UpdateTicketColumn` is the ONLY writer of `_legRow[i].IsLive`, and `AnimateLegPulse` reads that CACHED flag every frame.** That last clause is why the bare deletion would not merely have looked stale — **it would have kept PULSING.** Accepted as built at both sites, with `LegsOfFixtureAfter` deleted and its record left standing.** **THE GATE RE-BASE IS AN INVARIANT AND I RATIFY IT: `Assert.AreEqual(0, preemptS2, …)` fires PER SEED across the whole search, so a state-2 frame observed with the stage on the wrong fixture FAILS the run. **The retired defect cannot satisfy the gate again, and it cannot hide in an unpinned seed either.*** **AND THE CONDITION I PRE-COMMITTED IS MET: `§5`'s actual defect was that the assertion *"logs and does not fire"*. The re-based gate closes with **`Assert.Greater(state1Cases, 0)` and `Assert.Greater(state2Cases, 0)`** — thresholds, ASSERTED. Neither state degrades to a log line. **That was the thing that had to hold and it holds.*** **`§5` DOES NOT REQUIRE ONE ARTEFACT, and its own closing note settles it: *"The construction is a lane call; the states that must be certified are not."* The gate comment wanting *a seed that demonstrates BOTH* was the SEARCH's choice, never the ruling's. **So the two-seed shape is compliant and TV was right to ask rather than assume.*** **BUT IT SHOULD STILL CHANGE, AND NOT ON PREFERENCE. TV's correction is the reason: **`STATS-MULTI-2` DOES carry both states, 11 of 50** — the first search's `state1=0` was the warm-up artefact, so the split is max-picking, not necessity. **TWO SEEDS CERTIFY THE WORD AT TWO STATES. ONLY ONE TICKET PASSING THROUGH BOTH CAN CATCH A VALUE THAT FAILS TO UPDATE ON THE TRANSITION** — a footer computed once and cached would read correctly on two separate runs and staleley on one. **A STALE CACHED FLAG IS PRECISELY THE DEFECT THIS COMMIT JUST FIXED**, one field away in the same file.** **RULED: WHERE A SINGLE SEED CARRIES BOTH STATES, PIN IT; fall to the split only when none does. The loop already breaks at `state1Cases > 0 AND state2Cases > 0`, so the change is to PREFER a both-states seed before accepting a split, not to search harder. **Frame count is not the thing to maximise — the assertion is `> 0`, and 11 satisfies it exactly as 49 does.** This narrows what the gate can miss; it widens nothing.** **THE DROPPED WARM-UP IS ENDORSED SEPARATELY: 5 of 12 candidates at `frames=0` is **`C29` in the SEARCH rather than in the gate** — an instrument blind to 42 percent of its own population, which `C46` names in another register. Removing it made the state1 count honest, and that is what exposed the max-picking | batch 199 |
| T140-am8 | The multi-scorer counter's remedy — the site's UNIT is wrong, not its reach: counters are per LEG, the anchor is per MATCH | **RULED — DD 2026-08-25 batch 199, closing `T140-am6`'s remedy on the evidence its fixture produced: **`2ff03a6` reads 1 where the player scored 2**, on `MULTI-0`, and the defect is confirmed rather than suspected.** **THE CAUSE IS NAMED AS A UNIT ERROR AND NOT AS A MISSING LOOP, because that is what stops it recurring at the next site: `OnGoalPlayed` resolves the scorer against `_ticket.Legs[_stageLeg]`, and `_stageLeg` is set from `BeginStageLeg(evt.LegIndex, …)` — **the telling's ANCHOR.** The anchor answers *which match is on the stage*. **It was asked *which bets are counting*, and those are different questions with the same index today only because arm A made them different yesterday.*** **RULED: **A PER-LEG QUANTITY IS COMPUTED PER LEG. The anchor selects the MATCH that is staged; it never selects the set of legs whose counters advance.** Every live leg on the telling advances its own counter from the same revealed goal, each subject to its OWN reveal gate — not one gate shared across N legs.** **AND `_scorerRevealedForActiveLeg` CARRIES THE SAME ERROR IN ITS NAME: *the active leg*, singular, is the phrase `SweatActiveLegModel` already retired in terms. **One flag cannot serve N legs**, and a per-leg counter behind a shared reveal flag would reveal one leg's scorer on another leg's causal payoff. It changes with the counter or the fix is half a fix.** **THE DISCRIMINATOR FOR THE REST OF THE SWEEP, so the lane does not change four sites where one is wrong: **`_stageLeg` IS CORRECT FOR ANYTHING DESCRIBING THE MATCH** — the score, the scorebug, the pitch — **AND WRONG FOR ANYTHING DESCRIBING A BET.** Of the four `_ticket.Legs[_stageLeg]` reads in `OnGoalPlayed`, the scorer resolution is a BET question; the score repaint is a MATCH question. **Apply the discriminator; do not sweep by the field name.*** **ONE THING DELIBERATELY NOT RULED HERE: the ORIENTATION question underneath the score repaint — two legs on one match may back opposite sides, so *picked* is ambiguous under N-live. **That is `T140-am2`'s owed prose-anchor item and `MatchModel.AnchorSide`'s ground, not this row's**, and folding it in would let a counter fix quietly settle a question Allen has not seen | batch 199 |

---

## For the orchestrator

- **The seam is closed.** `T94`'s multi-fixture half now needs only `D2`'s frames to verify, against
  batch 197's pre-commitment as amended by `T140-am5` — reading (b) is split into b1/b2.
- **One small change to the gate**, not a re-run: prefer a seed carrying both states before
  accepting a split.
- **The counter has its remedy**, with a discriminator so the sweep does not over-reach.
- **Files to stage, by explicit path:**
  `docs/design/register-entries-2026-08-25-batch-199.md` and `docs/design/REGISTER.md`.
- **Backlog is 199.**

## Limits

- **The gate was read at HEAD**, but I have not run it; the `preemptS2` assertion's behaviour is
  read from its call site and its message, not observed failing.
- **`11 of 50` and `49` are TV's**, and the preference ruling rests on TV's correction rather than
  on my own count.
- **The counter's remedy is a design property, not a patch.** How the per-leg counters are held is
  the lane's, and the reveal-gate half may cost more than the counter half.
