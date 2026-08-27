# Register entries — batch 208 (2026-08-28)

**Item 1.1 ruled. NO BEAT is wanted at the boundary — and the thing that IS wrong is in the strip,
which carries the previous match's result through four minutes of the next one. The treatment is a
LINE, and a line costs no sweat seconds.**

**Two rows.** **Destination tables:** TV (`T140-am11`) · Cross-surface (`C65`).

**Evidence:** `dd-import/d2-fixture-boundary-2026-08-27`, five frames read at this seat across both
moments. **Nothing measured.**

---

## The rows

| T140-am11 | The fixture boundary wants NO BEAT — and the strip holds the previous match's result through the whole of the next one, which does want fixing | **RULED — DD 2026-08-28 batch 208, answering item 1.1 in all three parts.** **(1) NO BEAT, NO PAUSE, NO INTERSTITIAL SCREEN. **Nothing at the boundary is false**: the column switches with the scorebug (`T94-cl`), the counter advances `MATCH 1/2` to `MATCH 2/2`, and every zone names the fixture on screen. **A treatment cannot be a FIX where there is no defect** — and a pause would spend sweat seconds against the 60–90s law that `StagedGoal.Amount` exists to protect. `T140-am`'s *the boundary gets no treatment at all* is downgraded: **it needs none.*** **(2) BUT THE STRIP IS WRONG, AND IT IS THE PART NO ROW NAMED. Read across `M2`: at `1'`, `2'` and `4'` — forty frames — the strip reads **`LEG 1 — WON`**, unchanged. **The previous match's result sits over the whole of the new match until something happens in it.** Not a flicker and not a frame: four sim-minutes on this evidence.** **AND IT IS NOT STALE BY BUG — IT IS HOLDING BY DESIGN, which is why the fix is a write and not a guard: the strip carries the last statement written, `RevealBeatChrome` is the only thing that lands one, and **nothing speaks at a kickoff.** At `0—0` with no beat yet, the honest state is *nothing has happened here* — and the surface expresses it by describing a different match.** **(3) THE TREATMENT IS A LINE, NOT A BEAT, AND IT COSTS NOTHING: **the strip speaks at kick-off.** The slot already exists, the write happens during play, and it displaces a statement that has already had its hold. **AUTHORED: `KICK-OFF`.** Nothing more — the scorebug already names the clubs and the counter already says which match, so a longer form would restate a fact on the surface, which §7 bans in terms.** **`T87-am2` IS THE CHECK THAT MAKES THIS SAFE RATHER THAN A NEW RACE: it ruled *a statement replaced on its own entrance frame was never made*, and gave `LEG n — WON` its hold at the grade beat. **By kick-off that hold has long elapsed** — four minutes on this evidence — so displacing it takes nothing the player did not see, and the result stays permanently in the column's `W` chip regardless.** **ONE THING UNVERIFIED AND DELIBERATELY NOT RULED: **what the strip carries at the FIRST fixture's kick-off.** `D2` starts after a whistle and cannot show it. If it is blank there, the same line serves both and the lane should say so; **if it already speaks, this ruling applies only at a boundary** and the lane reports which | batch 208 |
| C65 | A LIVE requirement must belong to the fixture on screen; a RESULT need not — `T94`'s criterion bounded before it is over-applied | **LAW — DD 2026-08-28 batch 208, register-level, bounding a criterion that is about to be cited on a third zone.** **`T94-am2` RULED: *THE LIVE NEED'S FIXTURE MUST BE THE FIXTURE ON THE SCOREBUG.* With `T94` now closed and the strip found carrying `LEG 1 — WON` over a different match, **the obvious move is to read that as `T94` in a third zone. It is not, and the difference decides the remedy.*** **THE DISTINCTION: **`T94`'s criterion governs a CLAIM ABOUT WHAT IS STILL TO COME.** A NEED says *this is what your money still needs*, and a requirement for a match the surface has not introduced is a state lie — the player cannot act on it or even locate it. **A RESULT is a claim about what already happened, and a finished fact does not become false when the camera moves on.*** **SO `LEG 1 — WON` OVER A NEW FIXTURE IS TRUE, AND IT IS LEGIBLE: it names a LEG, not a match, and the ticket column two zones away carries that leg with its `W`. **A reader resolves it against the column, which is where leg numbers live.** Had it been a `T94` violation the remedy would be to blank it; because it is not, the remedy is that **something better should be written there** — which is `T140-am11`'s kick-off line.** **THE RULE, so the next zone is judged on the right axis: **ASK WHETHER THE STATEMENT IS LIVE OR SETTLED BEFORE ASKING WHICH FIXTURE IT BELONGS TO.** A live statement belongs to the fixture on screen. A settled one belongs to the thing it settled, and may outlive the shot — **but it may not OCCUPY a slot the current fixture needs**, which is a different fault with a different fix.** **AND THE CAUTION IS THE REASON THIS IS A ROW: `T94` cost five batches and moved three times. **A criterion that expensive gets cited widely, and a criterion cited beyond its subject produces confident wrong remedies** — here, blanking a true line and leaving a dead zone | batch 208 |

---

## What evidence would answer the temporal question

Batch 207 said the decisive property is temporal and a still set cannot carry it. **That question is now
largely dissolved rather than answered:** the boundary will no longer be unmarked, because the strip
speaks at kick-off. What remains is only *should there ALSO be a pause* — and **that wants a NUMBER,
not a film.**

- **Measure the sim-seconds between fixture f's last beat and f+1's first rendered beat.** If a gap
  already exists the question is what fills it, not whether to make one.
- **There is reason to think one does:** `drawnEndingHoldDuration`'s own note records that
  `scene002` *"already carried 0.62 sim-seconds of dead window by accident."*
- **No capture window.** This is a number a test can print. **Do not shoot frames for it** — eighty
  stills of a hard join look exactly like eighty of a gentle one, which is why batch 207 declined to
  ask for them.

## For the orchestrator

- **TV is unblocked:** one line, `KICK-OFF`, written to the strip when a fixture begins.
- **One thing to report, not build:** what the strip carries at the FIRST fixture's kick-off.
- **One cheap number, whenever convenient:** the gap between fixtures. Not a capture.
- **Files to stage, by explicit path:**
  `docs/design/register-entries-2026-08-28-batch-208.md` and `docs/design/REGISTER.md`.

## Limits

- **`KICK-OFF` is unmeasured** against the strip's box. It is shorter than every line the element
  already carries, so `T143-am7` says a FITS conclusion would survive — but none is claimed.
- **The four-minute figure is the SIM clock** read off three frames (`1'`, `2'`, `4'`), not a
  wall-clock duration, and the set ends at `4'` rather than at the first beat of that match.
- **I did not verify what displaces the strip on an ordinary in-match beat**; the kick-off write is
  ruled as a design property and where it lands in `BeginStageLeg` is the lane's.
