# Register entries — batch 187 (2026-08-25)

**The measurement is not needed to decide. `T156` is LIVE in the shipped build, and the proof is
the same shape as `T156`'s own: the engine's config plus a width TV already took, with nothing
new measured.**

**Two rows.** **Destination tables:** TV (`T156-am`) · Cross-surface (`C58-am2`).

**Six source reads, all named inline. Nothing measured here — every width is TV's, per `T111`.**

---

## The rows

| T156-am | `T156` is LIVE IN THE SHIPPED BUILD — proven from the render chain and a width TV already took, so Allen's scope call does not wait on the queued measurement | **RULED — DD 2026-08-25 batch 187, on the fallback routed for measurement.** **THE ANSWER ARRIVES BEFORE THE NUMBER, and the number is still worth taking — for the COPY, not for the call.** **THE CHAIN, NAMED AT EVERY HOP, because batch 185 said "renders through `MatchModel.Fields`" and that is true only by a four-hop route a lane would otherwise patch at the wrong end: `DescribeActiveLeg`'s `default:` returns `new ActiveLegCopy(LegStatement(leg), string.Empty, …)` — **THE NEED FALLBACK IS THE EMPTY STRING**; `LegStatement`'s `default:` returns `SheetName(leg) ?? leg.DisplayLabel`; `SheetName` reads `MarketSheet.Build(...).AllRows` and returns `row.Name.ToUpperInvariant()`; `MarketSheet.NameOf` returns `fields.Line` where non-empty; and `MatchModel`'s team-total arm builds `Line` as `{tname} {ou} {line:0.0} {noun}` — **the FULL club name, city included.*** **THE EMPTY FALLBACK IS WHAT MAKES TRUNCATION CERTAIN RATHER THAN LIKELY. `FitOrFallback` reads: fits primary → primary; else non-empty fallback that fits → fallback; **else `FitToColumn(target, IsNullOrEmpty(fallback) ? primary : fallback)`.** With the fallback empty there is no middle rung — an over-wide team total goes STRAIGHT to the truncation backstop, which `FitToColumn` performs **by dropping whole words FROM THE END**. The last word is the market noun. **`GOALS` and `CARDS` are the first things deleted.*** **THE ARITHMETIC, ON TV's OWN ROW 65 of `route-nine-kind-widths-2026-08-20.md`: `SPREADSHEETS UNDER 4.5 CORNERS` measures **449.5px against a 261.0px box — over by 188.5**. That is the SHORT club form and it ALREADY CARRIES THE NOUN. The shipped fallback uses the FULL name, which is strictly longer by construction. **So at least one word drops in BOTH build states of `T168-am`, and the word that drops first is the noun.*** **AND ONCE THE NOUN IS GONE THE COLLISION HOLDS AT EVERY REMAINING DEPTH — `{CLUB} UNDER 1.5`, then `{CLUB} UNDER`, then `{CLUB}` — each identical for goals and for cards. There is no truncation depth at which the two markets are distinguishable again.** **WHICH PAIR, FROM `RunConfig`: `TeamGoalLines = {0.5, 1.5}`, `TeamCardLines = {1.5}`, `TeamCornerLines = {4.5}`. **The intersection is 1.5, so the live collision is `TeamTotalGoals` vs `TeamTotalCards` at 1.5, same team and same over/under. CORNERS DOES NOT COLLIDE** — its only line is unshared, which is worth stating because it means the defect is narrower than "the three team totals" and a fix may not need to touch corners at all.** **THE FIX SITE IS THE RENDER, NOT `MatchModel` — and this is the practical half. The name arrives through `MarketSheet`, which `S96` makes the ONE composer the TV, the laptop and the console all print through; shortening `fields.Line` would silently re-name the bet on all three surfaces. **`T168-am` already ruled this exact shape: take the identity from `MarketSheet`, apply `SweatFlavor.Short` at the render, move no naming authority.*** **WHAT THE QUEUED MEASUREMENT STILL BUYS: the EXACT surviving string — whether the tail that fits 261.0 is `{CLUB} UNDER 1.5` or shorter still. **That decides the COPY, which is mine and unwritten; it does not decide whether `T156` is live.** Leave it queued behind TV's part-C gate; do not expedite it** | batch 187 |
| C58-am2 | A ROUTED WIDTH IS MEANINGLESS WITHOUT THE BUILD STATE IT WAS TAKEN IN — `C58`'s shape, one level up: the hidden assumption is not which asset rendered it but which RULINGS had landed | **AMENDED — DD 2026-08-25 batch 187, and recorded because it nearly cost this seat a ruling.** **`C58` ruled that an offline width assumes THE FILE'S DEFAULT INSTANCE IS THE SHIPPED ONE. The same defect has a second axis: a width also assumes **THE STRING THE BUILD COMPOSES TODAY.** `T168-am` moves the club token from the full name to `SweatFlavor.Short` AT THE RENDER — so the identical row measures one way before that lands and another way after, **and the number carries no mark saying which.*** **THE CONCRETE NEAR-MISS: the team-total fallback was routed to TV for measurement while `T168-am` sat ruled and unbuilt. Had the number come back taken AFTER the build, it would have described the SHORT form; taken before, the FULL one. **Same request, same lane, two different strings, and nothing in the reply would have distinguished them** — §2.6's confounded measurement, arriving through correct process.** **THE CLAUSE: a routed width states (a) the commit it was measured at, and (b) any RULED-BUT-UNBUILT change to the string it measures. Without both, it may not close an item. **The lane cannot be expected to know (b) — the seat that routes the ask owns naming it**, which is where this seat fell short: the ask went out naming the number and not the pending rebase.** **HELD DELIBERATELY NARROW: this did NOT change the outcome above, because the team-total form overruns 261.0px in BOTH states and the ruling turns on that. **A law written off a near-miss should not be dressed as the reason the near-miss was survived** — it was survived by arithmetic that happened to be state-independent, and next time it may not be** | batch 187 |

---

## For the orchestrator

- **Allen's scope call on the three team totals is UNBLOCKED** — `T156` is live in the shipped
  build, and the collision is narrower than assumed: **goals vs cards at 1.5 only. Corners is
  clean.**
- **Do not expedite TV's measurement.** It is still wanted, for the copy, behind the part-C gate.
- **When it lands it must state the commit and whether `T168-am` was built** — per `C58-am2`, and
  that is on me to have said in the ask, not on TV.
- **Backlog is 187.** The scan (`node tools/register-scan.js`) will show it untranscribed.

## Limits

- **Nothing measured at this seat.** `449.5` and `261.0` are TV's, from row 65 of
  `route-nine-kind-widths-2026-08-20.md`.
- **"The full name is strictly longer than the short form" is arithmetic on what `Short` does**
  (drops the city), not a measurement of the full-name string. It is the one unmeasured step, and
  it is the reason the queued number is still worth taking.
- **I did not verify `T168-am`'s build state** — the ruling is written so it does not depend on it.
- **The copy is not authored here** and travels with `T169`.
