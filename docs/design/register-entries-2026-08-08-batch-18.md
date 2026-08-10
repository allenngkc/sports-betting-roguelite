# Register entries — 2026-08-08, batch 18

**Transcribe into `main-2/docs/design/REGISTER.md`.** Issued at authoring time per C22.

Closures: **G1**. New: **T70**. One deliverable: `tv-g1-authored-leg-statements-2026-08-08.md`.

---

## G1 — the authored leg statements. **DELIVERED. CLOSED.**

Owed by this seat since batch 17. The forms are in
`tv-g1-authored-leg-statements-2026-08-08.md` — eight NEED forms, eight compact forms, four authored
fallbacks, two conventions, one rule.

### The list corrected G1's premise, and that is why the deliverable is right

**G1 asked for one string per market. The surface renders two.** NEED (249px @ 28px, from
`DescribeActiveLeg`) and the compact statement (**143px @ 15px**, from `DisplayLabel` re-authored by
`LegStatement`). T69's escalation was about the live one; **authoring only that would have left the
tighter box exactly as it is.**

The compact box is 143px after the price and state chip take their reserved widths — and those
reservations are §5.1 working: fixed by the grid so six rows cannot go ragged. **The tight budget is a
consequence of a rule this seat wrote, which is the right reason for a budget to be tight.**

### The rule the forms follow

**NEED states the requirement. The compact statement states the identity.** Where the two questions
have the same answer — the totals markets — **the strings are identical, and that is correct.** Recorded
explicitly so a future pass does not "differentiate" them into a defect.

### Both naming conventions were already shipped

Neither is new: **clubs by their distinctive word** (T69 shipped `Atlanta Middlemen` → `MIDDLEMEN`) and
**players by surname** (the progress line already does `WAITING FOR {SURNAME}`). The scorebug carries
the fixture and `BACKED` carries the side, so no statement re-establishes who is playing whom. **143px
is only workable because of facts the surface already states elsewhere.**

### The four over-budget forms

| was | now | note |
|---|---|---|
| `{TEAM} TO WIN` — 24 | `{CLUB} TO WIN` — 16 | the variable was the whole problem |
| `{PLAYER} TO SCORE` — 21 | `{SURNAME} TO SCORE` — 16 | **this is the T69 case itself** |
| `BOTH TEAMS TO SCORE` — 19 | `BOTH TEAMS SCORE` — 16 | a permanently marginal constant, cleared |
| `KEEP ONE TEAM SCORELESS` — 23 | `ONE TEAM SCORELESS` — 18 | see below |

**`KEEP` was a register defect as well as a width defect.** It instructs the player about a thing he
cannot influence — §8 bans instruction, and the whole product's thesis is that he is watching, not
acting. The requirement is a state of the match, so the form names the state.

**Two constants overflowing with no variable in them** is the useful half of the lead's analysis: those
two could be authored to fit **once** and be permanently safe, which is what happened.

### Fallbacks, per §8's *shorter authored line*

`TO WIN` and `TO SCORE` are authored as complete sentences, not truncations — the subject is marked in
the scorebug and the row is its own subject. Also `ONE TEAM BLANKED` and, as a last resort, `CNRS`.

**`FitToColumn` is the authority, not my character counts.** Two forms sit exactly at budget; measure
them and **take the authored fallback rather than shaving a character**, which is how a form stops
reading as a sentence.

---

## T70 — the same fact named twice, vertically. **NEW — violation. Found while authoring the pair.**

**NEW · DD 2026-08-08.** NEED sits directly above the progress line and **the two are one authored
pair**, which is only visible when you author the top of it against the bottom:

> NEED `LANYARD TO SCORE` · progress `WAITING FOR LANYARD`

**The surname appears twice, three lines apart, and both lines say the same thing.** That is T69's
defect — a fact named twice in one statement — reproduced **vertically** instead of horizontally.

**Ruled:** AnytimeScorer's progress line becomes **`NOT YET`** (unscored) and **`SCORED`** (resolved).
The player is named once, by NEED, directly above.

Every other pair checks clean: `{CLUB} TO WIN` over `LEVEL 1–1`, `BOTH TEAMS SCORE` over
`1/2 TEAMS SCORED`, `OVER 2.5 GOALS` over `3 GOALS · 1 MORE`. **Requirement above, state below, no term
repeated** — that is now the check for any new market's pair.

---

## Ordering for the orchestrator

**TV:** land the forms and T70's progress lines together — they are one authored pair and splitting
them re-creates the duplication. `FitToColumn` verifies; take the authored fallback on a miss.

**Carried, unchanged from batch 17:** **S71** (one string, SureThing) · register WindowGlow and
ArtIndicator in the EMIT gate's ruled-value table · schedule the **R22 human walk** for room gates
6/7/8, VOID for three batches · **C13**, still Allen's.

**Owning-doc amendment owed:** TV's §8 references the copy deck for the authored forms and carries
**T70's rule — requirement above, state below, no term repeated across the pair.**
