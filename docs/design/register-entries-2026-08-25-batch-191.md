# Register entries — batch 191 (2026-08-25)

**The measurement is a report and it holds. `T156` is live and WIDER THAN I RULED — batch 187's
"corners does not collide" is WRONG, and the error is mine rather than new information.**

**Two rows.** **Destination table:** TV (`T156-am2`, `T168-am3`).

**Report:** `docs/5-orchestration/route-team-total-fallback-measured-2026-08-25.md` (`968a250`),
measured at `b60d2bd` with `T168-am` NOT built — both conditions stated, `C58-am2` satisfied.
**Every number below is TV's.**

---

## For Allen — the scope call, re-framed

On the sweat screen a team-total bet gets cut down to the club and the word **OVER** or **UNDER** —
the number and the market word are both gone. So for one team on one side, **every team-total bet
reads the same string**, whichever of the three markets it is and whatever the number. I told you
last time this was confined to goals versus cards at 1.5 and that corners was clean. **That was
wrong, and it was my error rather than new information:** I had already shown the cut keeps going
past the number, then argued corners was safe because its number is different. The number is gone by
then, so a different number protects nothing. Separately, and only when a club's city is two words,
the cut goes further and deletes the club's own name — the screen says **MOOSE JAW** where it should
say **SPREADSHEETS**. Building the naming fix we already ruled would solve that second one; it would
not solve the first. **The first has only two fixes: write short copy for these three markets, or
take them out of the offered set.**

---

## The rows

| T156-am2 | `T156` CONFIRMED ON THE SURVIVING STRING — and WIDER than batch 187 ruled: the LINE drops too, so the unshared-line protection protects nothing and CORNERS COLLIDES | **RULED — DD 2026-08-25 batch 191, on TV's measured report at `b60d2bd`, `T168-am` unbuilt. **`§4(a)` IS NOT TRIGGERED: the market noun does NOT survive, so batch 187 is NOT retracted** — its finding that `T156` is live now rests on the string itself rather than on inference.** **THE FOUR SURVIVORS, TV's: `RENO FERRETS OVER 1.5 GOALS` (390.0) → **`RENO FERRETS OVER`** (258.0); `RENO FERRETS OVER 1.5 CARDS` (390.1) → **`RENO FERRETS OVER`**; `RENO FERRETS OVER 4.5 CORNERS` (424.3) → **`RENO FERRETS OVER`**. Box 261.0. **Character-identical across three markets and two different line values.*** **NOW THE CORRECTION, AND IT IS MINE. Batch 187 ruled *"CORNERS DOES NOT COLLIDE — its only line is unshared"*. **IT COLLIDES.** `TeamCornerLines = {4.5}` is genuinely unshared and the reasoning was sound as far as it went; **the line is dropped three words before the survivor is reached, so it discriminates nothing on this surface.*** **THE ERROR IS NOT THE MEASUREMENT'S ABSENCE — IT IS THAT I DID NOT FINISH MY OWN ARGUMENT. Batch 187 stated the ladder in terms: *"the collision holds at every remaining depth — `{CLUB} UNDER 1.5`, then `{CLUB} UNDER`, then `{CLUB}`"*. **I then evaluated corners at the INTERMEDIATE depth, using the line as a discriminator, in the same paragraph in which I had shown the line is dropped.** Goals-versus-cards was judged at the true depth and corners at a shallower one.** **THE GENERAL FORM, so it is findable: **ONCE TRUNCATION IS SHOWN TO DROP TOKENS FROM THE END, EVERY DISTINGUISHABILITY CLAIM MUST BE EVALUATED AT THE DEPTH ACTUALLY REACHED.** A discriminator that lives in a dropped token is not a discriminator. Cheap to check and I did not check it.** **SO THE SCOPE IS NOT TWO MARKETS AT ONE LINE: for a given club and side, **ALL THREE TEAM-TOTAL MARKETS AT ALL THEIR LINES RENDER ONE STRING.** `T156`'s own framing — four pairs per match per club — understates it, and so did mine.** **AND A FURTHER DEFECT THE ASK DID NOT NAME, TV's observation and it is right: the survivor ends on a dangling **`OVER`** — a direction qualifying nothing, with no quantity and no market. That is its own copy fault and does not need `T156` to be wrong.** **`T46`'s BACKSTOP IS NOT REACHED — every survivor sits inside 261.0, so `§4(d)` is not triggered either. **The only live readings are (b)-for-club-identity and (c)** | batch 191 |
| T168-am3 | THE CITY-ONLY SURVIVOR IS REAL — CONFIRMED on the two-word city, and `T168-am` fixes it while fixing nothing about the collision | **RULED — DD 2026-08-25 batch 191, superseding `T168-am2`'s flagged preliminary (batch 190), which said in terms that it must never be cited as the finding. **`§4(c)` IS TRIGGERED: `MOOSE JAW SPREADSHEETS OVER 1.5 GOALS` (549.7) → `MOOSE JAW` (147.6). The club's own noun is gone and the CITY is what survives** — the exact inverse of `T69`'s shipped convention.** **A PRECISION THAT MATTERS MORE THAN IT LOOKS, BECAUSE TWO DOCUMENTS USE ONE PHRASE FOR TWO THINGS: TV's headline reads *"the distinctive word never survives"* and means the MARKET noun (`GOALS`/`CARDS`/`CORNERS`), which is lost in all four cases. **The ask's `§4(c)` meant the CLUB's distinctive word, `T69`'s sense — and that is lost in ONE case only.** In cases 1, 2 and 4 the club is named CORRECTLY: `RENO FERRETS`. **A lane reading the headline as club-naming would over-fix three cases that are not broken**, so the two senses are separated here rather than left to context.** **SCOPE, MEASURED NOT ASSUMED: `SlateGenerator`'s city list holds exactly TWO two-word cities — `San Francisco` and `Moose Jaw` — out of sixteen. **Rare, and reachable on a real slate without searching for it**, which is how TV met it.** **THE FIX IS ALREADY RULED AND UNBUILT: `T168-am` applies `SweatFlavor.Short` at the RENDER, dropping the city entirely, so the club's own word is what remains and this defect cannot occur. **No new ruling is needed for it — this row confirms the defect `T168-am` was ruled against, on evidence.*** **AND THE SPLIT IS CLEAN, WHICH IS THE DECISION-RELEVANT PART: **`T168-am` FIXES THE CLUB IDENTITY AND DOES NOT FIX THE COLLISION.** Batch 185 records the short-club form measured at 449.5 against 261.0 — **over by 188.5 with the city already gone** — so the noun and the line still drop and `T156` survives the naming fix untouched. Two defects, one repair each, and only one of them is available without Allen** | batch 191 |

---

## For the orchestrator

- **Batch 187 stands** — `§4(a)` was the only retraction condition and it did not fire. **What is
  corrected is its SCOPE claim**, and `T156-am2` carries the correction in my own words.
- **Tell TV two things:** the collision reaches corners, so any fix must not assume corners is safe;
  and the club is correctly named in three of the four cases — `T168-am` is the repair for the
  fourth, not for all of them.
- **The re-measure TV names is still wanted** if `T168-am` builds — but the arithmetic already says
  the collision survives it, so it is confirmation, not a decision.
- **Allen's scope call is above, re-framed and with my error stated plainly.**
- **Backlog is 191.**

## Limits

- **Nothing measured at this seat.** Every width and every survivor is TV's, at `b60d2bd`.
- **The post-`T168-am` behaviour is arithmetic, not measurement** — 449.5 against 261.0 from batch
  185. It says the string still overruns; it does not say what the survivor becomes.
- **The dangling `OVER` is named, not ruled.** It is a copy fault that outlives whichever way the
  scope call goes, and it belongs with whatever copy replaces the fallback.
