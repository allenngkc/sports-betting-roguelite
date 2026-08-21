# Register entries — batch 151 (2026-08-20)

**TV's per-form width table, ruled. The headline is not a width: it is that the compact slot's only
rescue is a mechanism that DELETES THE MARKET NOUN, and every authored compact form for the nine
kinds puts that noun LAST.** Two of the nine are buildable today. Three are withdrawn. The rest wait
on rungs that do not exist yet.

**Four rows.** **Destination tables:** TV (`T155`, `T156`, `T152-am`) · Cross-surface (`G1-am10`).

**Nothing here is measured at this seat** (`C58`). Every width in this batch is TV's; every source
fact is read at this seat and cited to a line.

---

## The state, measured before ruling

Seating scans, run rather than described (`C59`):

| | |
|---|---|
| `REGISTER.md` ID rows | **464** — SureThing 135, Room 46, TV 198, Cross-surface 62, Phone 8, Console 15 |
| rows rendered as pipe-prose | **0** |
| rendered rows with ≠ 4 cells | **0** |
| duplicate IDs in the tables | **0** |
| duplicate IDs across tables **+ backlog** (`C22-am`'s predicate) | **0** |
| backlog, authored and untranscribed | batches **137–150**, 31 rows |
| T-series | 1–150, gaps at **37, 137, 145** — probed, **zero mentions anywhere in the register or the backlog**, so never-allocated rather than lost |

**The docket says transcribed through 135. It is through 136** — `C22-am2`, `C57` and `C22-am` are
live rows. Batches 137–150 are the real backlog.

### The cell-count scan earned its seat, and this is the first time it fired BEFORE transcription

**Two untranscribed rows carry unescaped pipes: `C56-am2` and `K17`, batch 144, lines 17 and 19.**
Both quote the same C# expression, and the C# logical-or is two pipe characters. Each row splits into
**six cells instead of four**, with cell 4 empty. Transcribed as-is into a four-column table, a
renderer keeps cells 1–4 and drops the rest: **513 characters of `C56-am2`'s ruling, 1,495 of
`K17`'s, and BOTH batch cells, silently.** That is the 2026-08-16 defect exactly — same class, same
mechanism, 2,008 characters — caught this time while the rows are still on disk and cost-free to fix.

**Fix before transcription: escape both pipes in each quoted expression.** No ruling text changes.

---

## The rows

| T155 | The compact slot has NO LADDER, and its only rescue DELETES THE MARKET NOUN — `G1`'s charter violated by `G1`'s own forms, 69 times | **RULED — DEFECT, and the remedy is a build order rather than copy · DD 2026-08-20 batch 151, on TV's routed per-form table plus four source reads at this seat.** **WHAT THE SLOT ACTUALLY HAS. TV's flag 3 reads *"an overrun in the compact box has no rescue today."* HALF RIGHT, AND THE HALF THAT IS WRONG IS THE HALF THAT MATTERS: `TvSweatScreen.cs:2985` reads `FitToColumn(_legRow[i].Line, LegStatement(leg))`. The compact statement is NOT assigned raw — it passes through the WORD-BOUNDARY TRUNCATION BACKSTOP. What it lacks is the two-rung ladder: `FitOrFallback` appears once in the file, at `:3064`, on the NEED line.** **SO EVERY COMPACT OVERRUN LANDS ON THE FLOOR, AND `T69` NAMES THAT FLOOR IN `FitOrFallback`'s OWN DOCSTRING — *truncation is the floor, re-authoring is the fix*.** **WHAT THE FLOOR DOES, read from the method body: `FitToColumn` loops `cur.LastIndexOf(' ')` and `Substring(0, cut)` — IT DROPS WHOLE WORDS FROM THE END, one at a time, until the string fits. THE LAST WORD GOES FIRST.** **AND EVERY AUTHORED COMPACT FORM FOR THE NINE KINDS PUTS THE MARKET'S IDENTIFYING NOUN LAST** — `{CLUB} UNDER 4.5 CORNERS`, `{CLUB} UNDER 1.5 GOALS`, `{CLUB} UNDER 1.5 CARDS`, `{CLUB} OR DRAW`. **The first token truncation removes is the one that says WHICH MARKET THIS IS.** **THIS BREAKS THE PREMISE THE SLOT WAS BUILT ON, and the premise is written in the layout's own canon comment at `:5250` — *"Canon drops the market eyebrow here rather than shrinking it, BECAUSE EVERY AUTHORED STATEMENT ALREADY NAMES ITS OWN MARKET."* The eyebrow was spent on that guarantee. Truncation cancels it, and there is no eyebrow left to fall back on.** **IT IS ALSO A STRAIGHT VIOLATION OF `G1`'s CHARTER RATHER THAN A NEW FINDING. `G1`'s own row: *"the truncation backstop holds and NO SHIPPED STATEMENT SHOULD REACH IT."* `T74` restates it: *"a working backstop is a DETECTOR, not a REMEDY — and `G1`'s entire charter is that truncation is never reached."* TV measured 69 compact forms overrunning. SIXTY-NINE SHIPPED STATEMENTS WOULD REACH IT.** **SCOPE, and it is wider than the NEXT row: `:2985`'s comment states *one assignment feeds every row state below* — the truncated statement renders on RESOLVED rows (`:3002`, `:3008`, `:3017`) and NEXT rows (`:3074`), and is blanked only for the live row (`:3041`). So this lands on the settled ledger, not merely on what is coming up.** **RULED, and it is not new direction: `FitOrFallback` EXTENDS TO `LegRowLine`. `G1-am2` already scoped it — *nine markets × two forms (compact + NEED) × `G1`'s two-rung ladder* — so the compact rung was ruled at batch 125 and wired to one slot of the two. This row does not create a mechanism; it names the slot the existing one was always owed to.** **BUILD ORDER TO TV, no copy decision needed to start it. The rungs themselves are authored per kind and each is chosen BY MEASUREMENT, never by authoring intent (`G1`, `T133-am`)** | batch 151 |
| T156 | TWO DIFFERENT MARKETS RENDER AS ONE IDENTICAL STRING — `T96`'s shape, proven from the engine's own config with NO measurement | **RULED — VIOLATION, and it is the reason the team-total forms cannot be repaired by shortening · DD 2026-08-20 batch 151.** **THE PROOF NEEDS NO WIDTH, WHICH IS WHY IT IS A ROW RATHER THAN A ROUTED QUESTION. THREE FACTS, EACH CITED: (1) `engine/RunConfig.cs:79-81` — `TeamGoalLines = 0.5, 1.5` and `TeamCardLines = 1.5`. THE 1.5 LINE IS OFFERED BY BOTH MARKETS, for both sides, over and under (`MatchModel.cs:174-190`). (2) The two authored compact forms at that line are `{CLUB} UNDER 1.5 GOALS` and `{CLUB} UNDER 1.5 CARDS` — CHARACTER-IDENTICAL EXCEPT THE FINAL TOKEN. (3) `T155`: the final token is the first one truncation drops, and TV measured ALL 60 team-total compact forms as overrunning, narrowest by 11.4px — so BOTH strings are truncated, for every club in the pool, with no exceptions to argue about.** **THEREFORE THE SURVIVING PREFIX IS THE SAME PREFIX. Whatever `{CLUB} UNDER 1.5 …` truncates to, the goals leg and the cards leg truncate to it IDENTICALLY, because they differ only in the token that is already gone. No amount of further truncation can separate them — truncation is the thing that merged them.** **FOUR COLLIDING PAIRS PER MATCH PER CLUB: home and away, over and under.** **IT HITS BOTH BOXES. Team-goal and team-card NEED forms are the compact forms (`T152`, *compact = NEED*), 80 NEED forms overrun, and NEITHER kind has a fallback rung authored — only corners does (`{CLUB} UNDER 4.5 CNRS`). So both slots land on the floor and both collide.** **THIS IS `T96` EXACTLY, one level in. `T96` ruled a violation because *"both tickets in the goalless set printed the same string with opposite grades."* HERE IT IS TWO ROWS OF ONE TICKET — and per `T155`'s scope note the collision survives settlement, so a WON goals leg and a LOST cards leg sit adjacent reading the same words, separated only by tier and strike. A player cannot tell which of his own bets won.** **NOT A COPY PREFERENCE AND NOT A WIDTH COMPLAINT: two distinct offers become one indistinguishable row on the surface that reports what his money is doing** | batch 151 |
| T152-am | The three TEAM-TOTAL forms are WITHDRAWN in both slots — and the club cannot leave the statement, because NOTHING ELSE ON THE ROW CARRIES IT | **WITHDRAWN — DD 2026-08-20 batch 151, §1.5, on this seat's own authoring two batches old. NOT RE-AUTHORED HERE; the replacement is a measurement this seat does not have.** **WHY WITHDRAWN RATHER THAN SHORTENED: `T156` — the form as authored merges two markets. Shortening it further is the mechanism that causes the merge.** **THE OBVIOUS FIX IS CLOSED, AND I CHECKED IT RATHER THAN ASSUMED IT (`C59`). The obvious fix is to drop the club and let another channel carry the team, exactly as `LegStatement`'s docstring already does for the fixture — *"the fixture half is dropped entirely, because the scorebug already carries who is playing whom AND THE BACKED MARKER ALREADY CARRIES THE SIDE. That is what makes 143px workable at all."* **THAT SENTENCE DOES NOT EXTEND TO THESE MARKETS.** `SweatFlavor.PickedHomeForPresentation` (`SweatFlavor.cs:403`) returns, for every kind that is not Moneyline or AnytimeScorer, **`true` UNCONDITIONALLY** — and its own docstring says so in terms: *"this answers WHICH TEAM THE PROSE ANCHORS ON"*, expressly NOT which side was backed, *"where a draw's honest answer is neither."* **A team total on the AWAY side anchors HOME. The marker does not carry the team; it carries a narration anchor, and for these kinds it carries the wrong one** — which is `K17`'s flag (batch 144, `C17`-flagged, untranscribed) arriving on a second surface. **SO THERE IS NO CHANNEL ON THE LEG ROW THAT SAYS WHICH TEAM A TEAM-TOTAL LEG IS ABOUT, other than the statement string itself.** The row has three spans: statement, price, state chip. The chip carries the state word; the price carries the price. **THE CLUB IS LOAD-BEARING AND THE BOX CANNOT HOLD IT.** **AND DROPPING IT COLLIDES ANYWAY: `GoalLines = 1.5, 2.5, 3.5` against `TeamGoalLines = 0.5, 1.5` — a bare `UNDER 1.5 GOALS` is the MATCH total's own shipped compact form (`LegStatement`, `TotalGoals` arm). Corners and cards do not overlap on line value; goals does, and one collision is enough to close the route.** **WHAT SURVIVES: the three kinds' NEED semantics, their pair (`MET` / `NOT YET`), and `G1-am3`/`G1-am4`'s progress taxonomy are untouched — the withdrawal is of the STATEMENT FORMS only.** **THE SHAPE OF THE REPLACEMENT, stated as a constraint rather than a string: four tokens all doing distinguishing work — club, direction, line, market noun — inside a box whose widest FITTING occupant today has 2.8px spare. Three of the four cannot be dropped without a collision already on the record. This is not an authoring miss to be re-authored around; it is `G1-am10`'s escalation** | batch 151 |
| G1-am10 | The nine kinds, disposed: TWO buildable, THREE withdrawn, FOUR blocked on rungs — and the structural finding is that BOTH BOXES WERE ALREADY FULL | **DISPOSED, NOT DISCHARGED — DD 2026-08-20 batch 151, on TV's routed per-form table. `G1` remains this seat's standing debt.** **BUILDABLE TODAY, AS AUTHORED, NO FURTHER MEASUREMENT: `CorrectScore` — the only kind of the nine that clears in EVERY slot — and `TotalGoalsOddEven`, whose compact clears and whose NEED the existing `AT FT` rung rescues on TV's measurement. TWO OF NINE. Both are pure-literal forms that name no club, which is the pattern in TV's table and not a coincidence.** **WITHDRAWN: the three team totals (`T152-am`, `T156`).** **BLOCKED ON RUNGS THAT DO NOT EXIST: `Handicap` (compact clears; NEED rung 2 rescues SHORT CLUBS ONLY — `MUSKRATS WITHIN 1` is the widest fitting string in the whole band at 1.8px spare, and longer clubs fall through), `WinningMargin` (compact clears; **BOTH NEED RUNGS OVERRUN**, so it falls to the floor with no third rung — and `FitToColumn`'s dangling-token cleanup matches only ` v`, ` ·` and ` —`, so nothing in the method prevents a NEED reading `3+ GOALS APART AT`; whether it stops there is a measurement and is routed, not asserted), `DoubleChance` (compact 9 forms over, narrowest **0.4px**; NEED 40 over) and `PlayerMultiScorer` (NEED 6 over, narrowest 6.4px, **no fallback rung authored at all**).** **THE STRUCTURAL FINDING, and it is the one to carry to Allen: BOTH BOXES WERE ALREADY FULL TO WITHIN ~2px BEFORE ANY OF THIS WAS AUTHORED — compact's widest fitting occupant `UNDER 10.5 CORNERS` at 2.8px spare, NEED's `MUSKRATS WITHIN 1` at 1.8px. THERE WAS NO HEADROOM TO AUTHOR INTO, and `T151-am2` already struck the comparison that made it look as though there were.** **THE TEAM-TOTAL COMPACT IS THE EXISTING WORST CASE WITH A CLUB PREPENDED — `{CLUB} UNDER 4.5 CORNERS` strictly CONTAINS `UNDER 4.5 CORNERS`, the shape that already sits 2.8px from the wall. Read that way the overrun was structural from the moment the form was authored, and the per-form pass measured what the box's occupancy already implied.** **THERE IS NO OPTION WHERE AN UNAUTHORED KIND IS SIMPLY ABSENT: the engine offers all fifteen, the player can back any of them, and the row renders something — today `leg.DisplayLabel` through the `default` arm (`T130`, `T130-vf`, `T130-am`). *Ship fewer kinds on the TV* is not available; the choice is between terse copy, more room, or a row that reads wrong.** **ESCALATED TO ALLEN AS SCOPE, three routes with the closed one named: (a) AUTHOR TERSE RUNGS — cheapest, and it spends the register the surface was built on, against `T69`'s *distinctive word, not the generic one*; (b) RE-OPEN THE TICKET COLUMN'S GEOMETRY — `T46`/`R30` hold the outer width fixed and `T90-am` ruled the 2px ink floor with the padding already spent at `T84`/`T74`, so this is a reversal of standing rulings and material by construction; (c) A DIFFERENT ROW TREATMENT for club-bearing markets. **ROUTE (d), MOVE THE CLUB TO AN EXISTING CHANNEL, IS CLOSED — `T152-am` checked it and there is no such channel.** THIS SEAT RECOMMENDS (a) FOR THE FOUR BLOCKED KINDS and holds the three team totals until Allen rules, because their four tokens do not survive abbreviation without reaching `T156`'s collision again** | batch 151 |

---

## Routed to TV — three measurements, and one of them may reopen a disposition above

Each is a number this seat cannot produce (`C58`). None is a new sweep; all three are questions the
existing pooled run can answer.

1. **`DoubleChance`'s RUNG DISPOSITION, stated as it was for the other three.** The report resolves
   `WinningMargin` (both rungs over), `Handicap` (rung 2 rescues short clubs) and `TotalGoalsOddEven`
   (ladder saves it) — **it does not say what happened to `DoubleChance`'s rungs.** 40 NEED forms over
   with a narrowest overrun of 33.5px suggests both rungs fail for every club, **but that is a
   derivation and this seat will not make it** (batch 95, and `T151-am2` is what deriving cost last
   time). State it measured.
2. **`PlayerMultiScorer` NEED has no fallback rung authored.** 6 forms over, narrowest 6.4px — the
   smallest gap in the set. Before a rung is authored: **is the overrun carried by the SURNAME pool
   or by `TO SCORE 2+`?** If it is the surname, a rung shortening the literal will not save it.
3. **`WinningMargin`'s floor.** With both rungs over, `FitToColumn` drops `FT` from
   `3+ GOALS APART AT FT`. **Does the result stop at `3+ GOALS APART AT`?** Report the rendered
   string, not the width.

**And one build order, needing no measurement:** `FitOrFallback` on `LegRowLine` (`T155`).

---

## For the orchestrator

- **Two backlog rows must be repaired BEFORE transcription** — `C56-am2` and `K17`, batch 144,
  lines 17 and 19. Escape the pipes in the quoted C# logical-or. **2,008 characters of ruling text
  and both batch cells are dropped otherwise, silently.** This is the first time the cell-count scan
  has fired before the damage rather than after.
- **The backlog is batches 137–150** (31 rows), not 136–150 as the docket has it. Batch 136 is live.
- **Zero ID collisions** across tables plus backlog, on `C22-am`'s corrected predicate. `T155`,
  `T156`, `T152-am` and `G1-am10` are free.
- **`K17` is now cited by a TV row** (`T152-am`) while still untranscribed — the `C22` cost
  `C22-am` names, arriving again.

## Limits of this batch, stated rather than discovered

- **Nothing was measured here.** Every width is TV's, taken as reported.
- **No frames were read.** `T155` and `T156` are source-and-config arguments and are ruled on that
  basis; neither needs a frame, and neither claims anything about how a thing reads.
- **The laptop is untouched.** These forms exist there too, in a different voice and a different box,
  and nothing in this batch transfers.
- **`T156`'s four-pairs-per-match figure is combinatorial**, from the config's line sets — not an
  observed frequency in play.
