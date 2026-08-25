# Register entries — batch 198 (2026-08-25)

**§1: TV IS RIGHT AND BATCH 197 WAS WRONG. A bare deletion would have shipped `T94` INVERTED — the
ended fixture lit too long instead of the next lit too early. The repaint is ratified.**

**Three rows.** **Destination table:** TV (`T140-am5`, `T151-am4`, `T140-am6`).

**Route:** `docs/5-orchestration/route-t94-repaint-2026-08-25.md`. Both of TV's code claims verified
at this seat. **Nothing measured here.**

---

## The rows

| T140-am5 | `T94`'s seam is a SUBSTITUTION, not a deletion — batch 197 confused STATE with PAINT, and the bare deletion inverts the defect | **RULED — DD 2026-08-25 batch 198, on TV's §1, which blocks the build and should.** **BATCH 197 SAID: *`MarkPresentedResolved` runs on the line ABOVE the pre-emptive advance, so the ended fixture's legs stop being live there.* **They stop being live IN THE PREDICATE. `MarkPresentedResolved` sets `_presentedResolved[i] = true` AND REPAINTS NOTHING** — verified at this seat, it is a bare loop over a flag array. **The screen learns it only at the next `UpdateTicketColumn`. I read a predicate and called it a frame.*** **AND TV'S READ OF THE CONSEQUENCE IS THE SHARP PART: with nothing repainting between the mark and the next `RenderEvent`, the ended fixture's rows **KEEP THEIR LAST PAINT — LIVE, STILL PULSING — FOR THE WHOLE WHISTLE-AND-SLAM BEAT.** That is `T94` INVERTED: fixture f lit too long instead of f+1 lit too early. **Same defect class, opposite direction, and a bare deletion would have SHIPPED it.*** **RATIFIED AS PROPOSED: `UpdateTicketColumn(_liveLegsShown)` at both sites. It drops the advance — 197's objective is met and `LegsOfFixtureAfter` still goes dead and is still deleted — **it REPAINTS, so the ended legs render RESOLVED and `IsLive` clears, and it leaves nothing live through the beat**, which is exactly what the pre-committed frame expects. It is `T62`'s idiom already used verbatim at the score-repaint site, and `ReferenceEquals(liveLegs, _liveLegsShown)` makes the self-pass a repaint-without-copy BY DESIGN rather than by luck.** **197's WORDING IS CORRECTED IN TERMS: **the fix is a SUBSTITUTION, not a deletion, and not an ordering change either.** And TV asking for the call rather than substituting silently is the right instinct — **a lane that quietly improves a ruling leaves the register describing a build that does not exist**, which is the failure `C22` exists to prevent.** **THE PRE-COMMITTED FRAME'S READING (b) IS SPLIT, because as written it would have MISDIAGNOSED this exact outcome: **(b1) f+1 reads LIVE while the scorebug holds f → the advance survived. (b2) f's OWN legs still read LIVE through the beat → no repaint happened.** Two failures, two causes, and the old (b) would have sent the lane hunting a third advance site that does not exist. **(a) and (c) stand unchanged | batch 198 |
| T151-am4 | Bucket 1's ladder ends a rung early on 6.1px — NO GATE OWED, and the reason is the CLIFF, not the size | **RULED — DD 2026-08-25 batch 198, on TV's §2.** **THE FACT: rung 2 `1 GOAL APART AT FT` fits at **254.9 against 261.0**, so rung 3 never renders for bucket 1. TV asks whether a slender-margin gate is owed the way `T143-am8` required one.** **NO, AND THE DISTINCTION IS PRINCIPLED RATHER THAN A JUDGEMENT ABOUT SIZE. `T143-am8` demanded a gate because what lay past that margin was an **OVERRUN** — the composition had nothing beneath it to fall to, so crossing the line broke the zone. **Here what lies past the margin is the NEXT AUTHORED RUNG.** If the 6.1px goes, `1 APART AT FT` renders: shorter, authored, ruled. **No cliff, and therefore nothing to gate.*** **THE RULE, so the next slender margin does not have to be argued from scratch: **A SLENDER MARGIN OWES A GATE WHEN WHAT LIES PAST IT IS AN OVERRUN, AND OWES NOTHING WHEN WHAT LIES PAST IT IS THE NEXT AUTHORED RUNG.** The number to watch is not the margin's size but what happens when it is crossed.** **AND THE ASYMMETRY IS THE LADDER WORKING, NOT AN INCONSISTENCY: bucket 2's plural pushes `2 GOALS APART AT FT` past the box, so rung 3 LIVES for bucket 2 and is DEAD for bucket 1. **A ladder that adapts per string is what a ladder is for** — a rung that never renders on one form is not a wasted rung, it is headroom that form did not need | batch 198 |
| T140-am6 | The multi-scorer counter reads only the ANCHOR leg — arm A's N-live class, its own item, and it retires "the last N-live site" | **RULED — DD 2026-08-25 batch 198, on TV's §3, answering the scope question it asked.** **NOT `T169-am`'s SCOPE, AND THE DIFFERENCE DECIDES WHO FIXES IT: `T169-am`'s open item is a MISSING TEST — no PlayMode fixture drives the multi-scorer arm through a goal. **This is a STRUCTURAL single-leg assumption that predates the build.** A fixture would EXPOSE it; a fixture cannot fix it.** **CONFIRMED IN CODE AT THIS SEAT: `OnGoalPlayed` reads `_ticket.Legs[_stageLeg]` at four sites, and `_stageLeg` is set from `BeginStageLeg(evt.LegIndex, …)` — **the telling's ANCHOR leg.** So on a same-match ticket a multi-scorer leg that is NOT the anchor is never inspected and its counter never moves. **TV's reading is right and the diagnosis is its own.*** **IT IS ARM A'S N-LIVE CLASS — the same shape as `DramaEvent.LegIndex`'s redefinition to the anchor: a site that assumes ONE live leg per fixture, and which a build that finally puts a NUMBER on screen makes visible as a wrong number rather than as nothing at all.** **AND IT RETIRES A CLAIM THAT SHOULD NOT STAND: the pending window was called *the last N-live site on this surface*. **IT WAS NOT.** Recorded as a correction to the map and not as a fault — the sweep was thorough, and this site hides behind a FIELD rather than a call, which is exactly what a call-site sweep misses. **No one should treat the N-live sweep as closed.*** **THE REMEDY IS NOT RULED HERE and this row does not pre-empt TV's run: what is settled is the SCOPE — its own item, arm A's class, not a test gap. **`_scorerRevealedForActiveLeg` carries the same shape and should be read in the same pass** | batch 198 |

---

## For the orchestrator

- **§1 unblocks TV immediately** — the substitution is ratified as proposed, at both sites.
- **§2 needs nothing built.** No gate.
- **§3 is its own item**; TV's verification run proceeds and the remedy comes back to me with it.
- **Files to stage, by explicit path:**
  `docs/design/register-entries-2026-08-25-batch-198.md` and `docs/design/REGISTER.md`.

## Limits

- **Both of TV's code claims were verified here**, but by reading — I have not run the sweat, and
  the inverted-paint behaviour is deduced from `MarkPresentedResolved` doing no painting, not seen.
- **The 254.9 and 6.1px are TV's**, at its stated commit.
- **`_scorerRevealedForActiveLeg` is named, not read.** I am asserting it shares the shape from its
  name and the one site I saw, which is weaker than a read and is flagged as such.
