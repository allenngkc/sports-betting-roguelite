# TV Sweat — lead ownership contract

**Worktree:** `tv-sweat` · **Branch:** `slice/tv-sweat-refinement` · **Lead:** Claude (Opus 5)
**Contract authority:** `main-2/docs/5-orchestration/STUDIO.md` · **Board:** `main-2/docs/5-orchestration/STATUS.md`
**Written:** 2026-07-31 · **HEAD at writing:** `220c5ec`
**Remote:** `origin/tv-sweat` · **PR:** [#3](https://github.com/allenngkc/sports-betting-roguelite/pull/3) — whole slice → `main`, opened 2026-08-08 at `97350ae` (289 commits; this window is the last commit alone, 9 files)

Supersedes `handoff.draft.md`, which was a briefing rather than a contract and carried none of the
four sections STUDIO.md requires. Its briefing content is folded into §5 below; the draft may be
discarded.

---

## 0-B93. THE STATS PANEL SHIPPED, AND THE LANE LEARNED TO DELEGATE · 2026-08-15

**Branch `4fb6756`, pushed, remote-verified, and merged to main. Tree clean, editor released.**
Eleven commits on top of §0-B85. §8.8's panel went from mechanism to a ticket-keyed feature across
DD batches 87, 89 and 93, **and every build item after the first was executed by a dispatched agent.**

| suite | measured | when |
|---|---|---|
| EditMode | **255 executed / 254 passed / 0 failed / 1 ignored** (G1's grant) | this window |
| PlayMode | **122 executed / 111 passed / 0 failed / 11 by-design skips** | this window |
| engine | 292 / 292, 0 failed | **earlier this window, NOT re-run since**; the branch is behind main |

### THE TWO STANDING DISPATCH RULES — and both gaps were in MY BRIEFS, not the agents' judgement

**1. NEVER END A TURN AGAINST A RUNNING UNITY PROCESS.** A dispatch launched a warm compile and ended
its turn saying it would continue "when the notification arrives". Nothing was watching it. My brief
said *"launch detached and poll"* and never said *never hand the turn back* — the agent honoured the
letter and missed §4 rule 4. **Resumed with the rule rather than restarted; its work survived intact.**

**2. FULL SUITE, NO `-testFilter`, WHENEVER PRODUCTION CHANGED.** I asked for *"PlayMode 0 FAILED with
every `Stats_panel_*` pin passing by name"*. A dispatch satisfied that with a filter on one class,
25/25 — **a fair reading of what I wrote.** The whole suite then failed on a new pin. **A filtered run
is not a suite; a gate that a filter can satisfy is a gate that will be satisfied by one.**

> **Both now go verbatim into every Unity dispatch. An agent that meets a brief exactly and still
> ships a hole has found a defect in the brief, and writing it down is cheaper than remembering it.**

### THE ONE THAT COST A CAPTURE, and it generalises past this surface

> **A CHANNEL THAT NEVER READS THE AUTHORITY IS INVISIBLE TO A PIN ON THE AUTHORITY.**

The panel freezes time by adding a term to `SeatedDeltaTime`. That was pinned; the pin passed; **the
pin was correct.** T99's capture then showed the panel over a frozen scoreline **with the minute
ticking `18' → 21'` behind it** — `TickClock` advanced on `Time.deltaTime`, and the `!_seated` guard
above it was what actually froze the clock on stand-up. Two expressions of one rule, so a third freeze
condition reached only one of them. **The frames caught what the pin structurally could not.**

### THE KEYING ARCHITECTURE — read this before touching the panel

- **The row set comes from the TICKET, derived ONCE at adoption** (`ComputeStatsRowSet`, called right
  after `_ticket = director.CurrentTicket`) and stored. `RenderStatsPanel` reads the stored flags,
  never the live leg's kind. **It must not recompute per leg** — a table whose rows appear and vanish
  under the player is the defect this replaced, not a variant of it.
- **`_countLedger` IS PER-LEG, HOLDS EXACTLY ONE KIND, AND IS REPLACED ON EVERY LEG.** That single
  fact drives everything else here.
- **So revealed counts are RETAINED per kind for the ticket's life**, cleared in
  `ResetForNewSession` and **never on a leg change**. Without it a filled row reverts to the mark when
  the next leg starts — **a revealed fact un-revealing itself.**
- **The mark means "bought but NOT YET REVEALED". An unbought row is ABSENT** (rendered as empty
  strings). Two different states, and the distinction is the ruling.
- **Two rows of three is still the maximum SIMULTANEOUS fill** — one count kind is live at a time.
  Unchanged by the keying and restated because composition keeps getting ruled against it.
- Read `Home`/`Away`, **never** `TargetHome`/`TargetAway` — the locked endpoint sits one property away
  and §8.8 calls a leak here blocker-class.

### THE GEOMETRY CHAIN, so a future change knows what moves what

```
widest measured ink --(MaxInkFraction 0.8)--> labelW / valueW --> colA / colB --> panel width
panelW = labelW + 418      (418 = 4*pad + 2*valueW, independent of the title string)
```

**`MATCH STATS` is coupled 1:1 to the panel's width** through `labelW` — every pixel off the label's
ink is a pixel off the panel. **The club-pool gate has 0.7px of headroom** (`Spreadsheets` 115.3
against a 116.0 limit, 79.52%); it is the tightest gate on the surface and fires on any pool
addition — by design, and the message says re-derive the BOX, never shorten the name.

### OPEN — all four with the DD, none of it this lane's to start

| item | where it sits |
|---|---|
| **The blank-row consequence** — content is now variable while height is build-time fixed, so a moneyline ticket is ONE row in a THREE-row panel and batch 87's oversized finding reopens on the commonest ticket | docked, leads the README of `tv-statspanel-ticket-keyed-2026-08-15` |
| **`MATCH STATS`** overstates a ticket-keyed panel; measured coupling supplied, no label authored | batch 93 |
| **The 0px flush gap** at the scorebug's bottom edge | batch 87 |
| **The panel's composition** itself | batch 87 / 93 |

**Three docked sets, deliberately scoped so one cannot be read for another's checks:**
`…-scorebug-…` carries T99's four checks (non-level scoreline) · `…-reordered-…` the size and column
order · `…-ticket-keyed-…` the keying and its cost. **Each README names what it does NOT claim.**

### DELEGATION — the deviation and the correction

**The audit read this lane at ZERO spawns across 1,473 tool calls and 397 hands-on edits.** The
loophole was that no single item ever looked big enough; **the batching rule closes it.** Five
dispatches followed and every one was reviewed at the diff, not the summary — which is how a filtered
suite, a stalled compile and an agent's own red pin were all caught before they were committed.
**The lead plans, dispatches, reviews, integrates. Docking and committing stayed with the lead** —
an agent that both makes a change and certifies it is how an unreviewed decision ships.

---

## 0-B85. THE DRAW ROW, THE STATS PANEL, AND A FREEZE THAT A PIN COULD NOT SEE · 2026-08-15

**Branch `832eae7`, pushed and remote-verified. Tree clean.** Seven commits on top of §0-B69's fold,
all merged to main as they landed.

| suite | discovered | executed | passed | failed | skipped |
|---|---|---|---|---|---|
| engine | 292 | 292 | 292 | 0 | 0 |
| EditMode | 255 | 255 | 254 | 0 | 1 ignored — G1's grant, held |
| PlayMode | 111 | 111 | 101 | 0 | 10 by-design |

**BOTH INHERITED REDS ARE GONE**, cleared by merging main exactly as predicted: the engine's 55
(`c82aefe`'s joint-model repair) and the laptop margin pin. **§0-B69's expected-red rows are closed.**

### THE ONE TO CARRY, and it cost a capture to learn

> **A CHANNEL THAT NEVER READS THE AUTHORITY IS INVISIBLE TO A PIN ON THE AUTHORITY.**

§8.8's stats panel freezes time by adding one term to `SeatedDeltaTime`, the single expression every
channel §4.4 lists reads. That was pinned, the pin passed, and **the pin was correct**. Then T99's
capture shot the panel over a frozen scoreline **with the minute ticking `18' → 21'` behind it**.

`TickClock` advanced on `Time.deltaTime`, and the `!_seated` guard above it is what actually froze the
clock on stand-up — **two expressions of one rule, agreeing by convention**, so a third freeze
condition reached only one of them. **T95's law arriving from the other side:** when a ruling adds a
condition, every mirror moves too, and the mirrors are found by grepping the quantity, never by
remembering. The clock now reads the authority. *The frames caught what the pin structurally could
not — which is the argument for ordering captures instead of accepting assertions, and the DD said so
in closing T99.*

### What landed

| item | state |
|---|---|
| **S74-am — the board's DRAW row** | Built (`5724aa1`), docked, **Design-verified**. Own line between the two teams, matchup column empty, `MatchupCardPitch` 78 → 116 re-derived from the 38px line pitch; six blocks → four, measured and confirmed on the frame |
| **C46 — the DRAW cell's widths** | Reported as widths, no verdict. Whole difference is the WORD (`DRAW` 49.88 vs `HOME` 49.02); the numeral contributes nothing — all three families top out at four characters |
| **§8.8 stats panel** | Mechanism + three rows + four pins. `TAB` **ratified** (T101) |
| **T99 — panel over the scorebug** | **CLOSED, Design-verified.** Four checks passed on the docked set |
| **T100 — populated count row** | Shot and docked (`832eae7`), with the DD |

### THE STRUCTURAL FACT THE NEXT SEAT WILL TRIP ON

**`CARDS` and `CORNERS` cannot both be populated. Ever, on one leg, by construction.**
`_countLedger` is **null unless the live leg is a corners or cards leg**, is configured for **exactly
one** of them, and **resets per leg**. So **two rows of three is the maximum fill the panel can
reach**, and §4D's *"per-team corners/cards are available"* is true only inside a count leg of that
kind. **A summary that reads as capability, describing something conditional and mostly absent** —
checked at the assignment site, which is the only reason it was caught before the composition was
ruled against a fill the surface cannot produce.

### A CONTRACT DEVIATION, recorded because the record is the point

**The 4-day delegation audit read this lane at ZERO sub-agent spawns across 1,473 tool calls and 397
hands-on edits.** Delegation is the operating mode, not an option: the lead plans, dispatches,
reviews, integrates. **The loophole actually being used was that no single item ever looked big
enough to delegate** — and the batching rule closes it: *small items are not an exemption; bundle
related small items into ONE bounded dispatch.* Corrected at the next item (the C46 panel-string
sweep) rather than at the next window. **Audited daily from here.**

### OPEN

| item | state |
|---|---|
| **T100's composition** | with the DD — ruled once it reads a filled table. **The `CARDS` fact above is what it must be ruled against** |
| **C46 sweep of the panel's strings** | T101's second item, running under dispatch |
| **The panel's scorebug overlap** | **CLOSED** — T99 permits it *only while time is frozen*; the condition is written at `SeatedDeltaTime`, where the change that would break it would be made |
| `RiskPays`' fact floor — 378.1 vs a LOCKED 249.0 | **Allen's**, unchanged |
| `CashOut`'s `MARKET SUSPENDED`, 26.7px | T74's table |
| The stand-up freeze path | **unphotographed**, named as such and accepted by the DD |

---

## 0-B69. THE HOLD, THE GUARD'S REAL GATE, AND THE DIAGNOSTIC THAT SETTLED BOTH · SEAT ROTATES HERE · 2026-08-15

**Branch `3652418`, pushed and remote-verified. Tree clean, Unity zero.** **This seat is at context
exhaustion and stands down on this commit — the next seat starts here.**

| suite | discovered | executed | passed | failed | skipped |
|---|---|---|---|---|---|
| EditMode | 254 | 254 | 253 | 0 | 1 ignored — G1's grant, held |
| PlayMode | 95 | 95 | 87 | **1 — inherited, EXPECTED, not ours** | 7 by-design |

### THE LAPTOP PIN IS EXPECTED RED — and read this before "fixing" it

`SureThingEntryTests.Working_margin_contains_its_content_at_the_legal_maximum_leg_count` fails at
`4.74798583984375` vs a signed 4.56. **It is measured against a SUPERSEDED test.** The surething-ui
lane's repair merged to main **after** this branch's last main merge, so this tree still runs the old
price-dependent pin. **Expected red. Not this lane's. No action** — it clears on the next merge from
main.

*Recorded because this seat got it wrong twice in one day:* first reported green (a single anomalous
run), then corrected to "deterministically red and unexplained". **Both were wrong about the cause.**
The value is stable because the test is stale, not because anything regressed.

### Batch 69 — both fixes, and the diagnostic that made them facts

The DD asked for every strip write logged with its call site across a `LegFinal` beat and said plainly
*"this seat cannot execute the code and does not claim the ordering as fact."* **It was run.**
`TvSweatScreen.TraceFlavorWrites` (off by default, set only by the harness) is left in place for the
next question of this shape.

**BOTH STANDING HYPOTHESES WERE WRONG IN THE SAME DIRECTION — they assumed a race.**

**1. `THE MATCH ENDS LEVEL` was never written to the strip at all.**

```
RenderEvent stash LegFinal  <- 'THE MATCH ENDS LEVEL'
grade WON                   <- 'LEG 1 — WON'
```

**No LAND between them.** `RevealBeatChrome` — the only thing that lands `_pendingFlavor` — lives
inside `TheaterBeat`'s `evt.Type != LegFinal` branch, **so on the whistle the stash is simply
dropped.** The line was correct, reachable and never displayed; there was no race to lose. Now written
**directly** in `FinalSlam` and **held** for `drawnEndingHoldDuration` (1.0f, matched to
`ticketDeadConsolationDuration` as ruled) before the grade beats run.

**2. The guard was gating on the wrong quantity, and that half was this seat's.** The trace read
`T97 guard goal=True` on **every** `Score` beat of a match that finished 0–0.

> **`spec.Goal.HasValue` is the beat's STAGED INTENT. `spec.Goal.Value.Commits` is what the scene
> RESOLVES INTO** — `Commits == false` is the chalk-off that prints `VAR — NO GOAL`. The law says the
> words are licensed by what the resolved scene CONTAINS; the first build implemented *what it
> staged*. **Reading a law and implementing it are different acts.**

### The set evidences itself now

Every captured frame logs the **strip text** beside score and clock, because T87-am2 is verifiable
only as *"visible, for multiple frames, before the grade"* — a claim about frames that the frames
should answer without a second instrument. Across **128 frames the strip holds exactly three states**:

| frames | strip |
|---|---|
| **111** | **`THE MATCH ENDS LEVEL`** |
| 9 | `LEG 1 — WON` |
| 8 | the mid-match shot |

The line holds frame 000 → ~050 of each ending at `clock='FT'`; the grade appears only in the last
handful. **No goal sentence in 128 frames.** The supplemental mid-match shot (`clock='30'`, 0–0)
carries T96's live NEED clause — the clause the previous README asserted while every frame was settled.

### ROUTED to the DD, not fixed

**`— LEAD CHANGE` renders over a `0 — 0` scorebug on 8 frames** (`TensionTag.LeadChange`,
`SweatFlavor.cs:47`, appended to any line). It asserts a change of lead the match never had —
**plausibly T97's law a third time** — *unless* it means a WIN-PROBABILITY lead change, which is a real
betting fact and legitimately reportable. **Two readings, two different remedies**, and the strip's
words are the DD's. Frames: `goalless-draw-backer-live-need__frame000`–`007`.

### OPEN — none of it this lane's to start

| item | state |
|---|---|
| The docked set (128 frames) | with the DD |
| `— LEAD CHANGE` over 0–0 | routed, awaiting the DD's reading |
| **The board's DRAW row** (S74-am) | HELD for Allen's word |
| **T94** — column and scorebug describing different legs | HELD for the DD's read |
| **`RiskPays`' fact floor** — 378.1 vs a LOCKED 249.0 column | **Allen's item** |
| `CashOut`'s `MARKET SUSPENDED`, 26.7px | T74's table |
| The laptop pin | **superseded test — clears on the next main merge** |

### FOR THE NEXT SEAT — the operational facts that cost this one the most

1. **`Time.captureDeltaTime` ties SIM time to RENDERED frames.** A capture burst spaced in REALTIME
   advances the match by however many frames the host rendered. **Frame-contiguous (interval 0) is the
   control.** This produced four passing captures of the wrong beat.
2. **`Ticket.State` does not leave `Open` until ROUND settlement — after ALL sweats.**
3. **A pick addresses `Matchup.Index`, not the slate position.**
4. **`DemoTicketPolicy`'s stake sizes ONE bet against the whole bank.**
5. **Scan to a real end marker, never a character count** — a 2200-char window silently stopped
   covering its target when a comment grew above it. Third instance in this lane.
6. **`ProjectSettings.asset` is integration-only Unity boot churn** — revert it by explicit checkout,
   never commit it.

---

## 0-B68. THE DRAW'S ROW, THE STRIP'S GOAL GUARD, THE DRAWN MATCH'S LINE — all three verified in one set · 2026-08-14

**Branch `1be8140`, pushed and remote-verified. Tree clean, Unity zero.** Batch 68's three rulings are
built and shot; the set is docked and **awaiting the DD's acceptance list**, which it applies to the
frames directly. A supplemental shot comes back here only if a criterion needs one.

| suite | discovered | executed | passed | failed | skipped |
|---|---|---|---|---|---|
| EditMode | 254 | 254 | 253 | 0 | 1 ignored — G1's grant, held |
| PlayMode | 95 | 95 | 88 | 0 | 7 by-design (the goalless capture joined them) |

**The laptop margin pin inherited from main is GREEN again** — the surething-ui lane's work on its
window, **not this lane's**. §0-DR's inherited-failure row is closed by them, not by us.

### Three rulings, two touches, one capture — Passed, 120 frames, all at `MALLARDS 0 — MIDDLEMEN 0` `FT`

**T96 — the draw is its OWN row.** Compact `DRAW`; live NEED `LEVEL AT FULL TIME` over
`LEVEL`/`NOT LEVEL`, with `LEVEL AT FT` as the authored shorter line. `Identity` is the MARKET PICK,
not a team — **a draw ticket has no backed side**, which is what let both tickets print `MIDDLEMEN ML`
with opposite grades.

> **A COPY RULING LANDS IN THE DECK OR IT HAS NOT LANDED** (the DD's own, and the reusable half). S74
> authored the draw's forms, the owning doc carried them, and the build still shipped a defect —
> because the deck sat between the doc and the build and nobody amended it. **The build was faithful
> to the artifact it was told to read.**

**T97 — the second instance of one law**, built as the guard that already existed one market family
over: **a beat's WORDS are licensed by what the RESOLVED SCENE CONTAINS, never by the beat's TYPE
LABEL alone.** The count families got this at F_0.4.0 P3 r2; the goal families never did, so a beat
typed `Score` or `BigPlay` printed a goal sentence whether or not a goal was staged. `NearMiss` is
excluded because its overrides were already right — they assert no goal and are used exactly where
none occurred, which is the model this copies.

**T87-am — `THE MATCH ENDS LEVEL`** at the whistle of a drawn match. A decided match ends ON a goal so
its final beat's line IS its ending; **a drawn match ends on nothing**, so the last line is stale by
construction. Read from the **revealed ledger**, never the locked `StatLine` — at the whistle they
agree, which is why the honest source costs nothing.

### THE SWEEP, and it is encoded as DATA rather than left in prose

All twelve strings of the four goal-asserting arrays. **Nine assert a goal; three assert only a
dangerous move** — matching the DD's own enumeration.

| array | asserts a goal | danger only |
|---|---|---|
| `ScoreUp` | **3/3** | — |
| `ScoreDown` | **3/3** — incl. `on the board`, the line that shipped over the 0–0 | — |
| `BigUp` | **2/3** — *…and finish*, *…and score* | *counter at full sprint* |
| `BigDown` | **1/3** — *walk it in* | *go the length of the pitch*, *rip through on the break* |

Recorded in code as `BigUpAssertsGoal` / `BigDownAssertsGoal`. **The three danger-only lines stay
REACHABLE** — the ruling's scope is "the parts that finish", and a big play that did not finish is
still a big play. **Kept as a parallel table rather than by reordering**, because a line is chosen
positionally by step and reordering would silently change which sentence an existing seed prints.

**Diagnosed as asked: a FRESH MIS-SELECTION, not a stale carry.** The line is chosen per beat from the
type's own array; nothing cached it.

### A THIRD FIXED-WINDOW TRAP — this time in a TEST, not an instrument

`T69_the_row_statement_is_re_authored_against_its_column` scanned `Substring(at, 2200)`. T96's draw row
added a dozen lines to `LegStatement` and pushed `{club} ML` past 2200, so **the assertion failed while
the string it asserts sat exactly where it belongs.**

> **A scan that stops covering its target reports the absence of its own window as a defect in the
> code.** Re-pointed at the method's next sibling — a real end marker, never a character count. Third
> instance in this lane; the rule has earned its place.

### OPEN — unchanged, none of it this lane's to start

| item | state |
|---|---|
| The docked 0–0 set | with the DD, acceptance list applied to the frames directly |
| **The board's DRAW row** (S74-am) | HELD for Allen's word |
| **T94** — column and scorebug describing different legs | HELD for the DD's read |
| **`RiskPays`' fact floor** — 378.1 vs a LOCKED 249.0 column | **Allen's item** (exceeds a locked dimension) |
| `CashOut`'s `MARKET SUSPENDED`, 26.7px | T74's table |

---

## 0-DR. POST-PHASE: main merged · the draw arm fixed · THE 0–0 SET SHOT AND DOCKED · 2026-08-14

**Branch `b3b5820`, pushed and remote-verified. Tree clean. Unity ZERO — and the editor window is
RELEASED to the surething-ui lane** for its pin verification, so this lane takes no editor work until
it comes back.

**TOP LINE: the post-phase unit is complete.** `origin/main` is merged, `SweatFlavor`'s draw arm is
fixed and pinned, G1-am8's scorer ladder is built, and T87-am's goalless set is docked. **The DRAW row
and T94 HOLD** pending the re-seated DD's read of the docked set and Allen's word.

| suite | discovered | executed | passed | failed | skipped |
|---|---|---|---|---|---|
| EditMode | 254 | 254 | 253 | 0 | 1 ignored — G1's grant, held |
| PlayMode | 94 | 94 | 87 | **1 INHERITED** | 6 |

Sweep: **1 of 22 overrunning** (`MARKET SUSPENDED`, T74's table) · 48 slots · 0 unaccounted for.

### The merge — 295 commits, and the TV surface did not move

`TvSweatScreen.cs`, `SweatFlavor.cs`, `SweatActiveLegModel.cs`, `TvExtentSweep.cs` and every TV test
came through **untouched** — this lane's work was already in main, so it merged with itself. Verified
by measurement (sweep identical, EditMode green), not by inspection.

**ONE INHERITED FAILURE, and it is not this lane's.**
`SureThingEntryTests.Working_margin_contains_its_content_at_the_legal_maximum_leg_count` — the LAPTOP
surface, 4.748px against a signed 4.56px. **Reachability argument, accepted by the orchestrator:** this
branch's three commits touched five files and **none of them can reach the laptop's margin flow.** The
flow changed on main via `ead9396` (*"the stake figure leads its own block"*, M-04/M-05) and the test's
own message names the owner — *"1.96px OWNED (M-04's 26px stake figure)"*. **Do not touch it**; the
owning lane is retired and its routing is Allen's after the shoot.

### `SweatFlavor`'s draw arm — a fall-through, not a decision (`ada9a84`)

Routed here **by name** from the markets lane's class sweep (`a3d184c`: *"SweatFlavor:206 — draw counts
as away for flavour, ROUTED → tv-sweat"*). **It survived that sweep because it lives in this surface's
file, not theirs** — a cross-lane sweep scoped by OWNERSHIP misses exactly the code another lane owns.

The anchor asked `Choice == Home` and let everything else be false. Correct while a moneyline could
only be Home or Away; **`MarketChoice.Draw` made the inference wrong without the line being touched.**
The fix is the rule the function already stated: a leg with no picked TEAM anchors home and lets the
market label carry the pick, exactly as O/U and BTTS always have.

**Deliberately NOT the null the markets lane used.** `BetslipModel.SideOn` returns null for a draw and
is pinned for it — that answers *which side you backed*, where "neither" is honest. This answers *which
team the prose anchors on*, where every leg needs an answer. **One finding, two functions, two correct
shapes.**

*Routed, not authored:* whether the flavour's VOICE reads right on a draw-backed leg is the DD's.

### G1-am8 + T92-am (`7ca92ca`)

Scorer ladder built: rung 2 `{SURNAME} SCORES`, **0 of 12 overrunning**, widest `PAVEMENT SCORES` 238.4
with 22.6 spare; bare `TO SCORE` retired. **The existing EditMode pin caught the contract change and
moved with it deliberately.** T92-am's 10.9 was **already closed** by the batch-61 widening — measured,
not assumed: box 695.0, deferral line 665.9, fits by 29.1.

### T87-am — the 0–0 set (`b3b5820`), docked at `dd-import/tv-goalless-draw-2026-08-14`

**`Atlanta Middlemen 0 – 0 Scranton Mallards`, seed `GOALLESS-5`. Passed, 120 frames, and EVERY ONE
reads `MALLARDS 0 — MIDDLEMEN 0` at `clock='FT'`** — 60 contiguous frames per ending.

| set | ticket | outcome |
|---|---|---|
| `goalless-draw-backer-ending` | the DRAW | **WINS** on a match where nothing happened |
| `goalless-team-backer-ending` | Home | **LOSES** to the same 0–0 |

**Captured and docked — no read offered.** The three dispositions are pre-committed at the DD seat.

The seed was **found, not hoped for**: `engine.tests/GoallessDrawSeedTests` searched 400 seeds through
the same path the capture takes and found eight goalless matches (draws 28 of 114). `LockRound`
resolves every game whether bet or not, so **the tickets are placed onto a result that already exists
rather than steering it** — and the 0–0 is asserted at lock so a drifted seed fails loudly.

### FOUR RUNS PASSED WHILE SHOWING THE WRONG BEAT — the durable part of this section

Each was diagnosed rather than guessed, and each was a real defect. **A passing capture is not a
capture of the thing.**

1. **A pick addresses `Matchup.Index`, NOT the slate position.** The draw ticket graded against a
   different fixture and came back *LOST on a 0–0*.
2. **`DemoTicketPolicy`'s stake sizes ONE bet against the whole bank**, so a second ticket does not
   fit. The symptom is silent: the sweat loop simply has nothing to advance to.
3. **`Ticket.State` does not leave `Open` until ROUND settlement — after ALL sweats.** Waiting on it
   captures a screen already cleared to Shop. The diagnostic said so exactly:
   `SweatIndex=1 phase=Shop session=null`.
4. **`Time.captureDeltaTime` ties SIM time to RENDERED frames, so a burst spaced in REALTIME advances
   the match by however many frames the host happened to render.** At 0.12s spacing an "ending" read
   `FT`, then `PRE`, `11'`, `30'`, `55'`, `74'` — four frames of the whistle and then the whole NEXT
   match. **Frame-contiguous (interval 0) is the control**, and 60 frames is 1.2 sim-seconds.

> **Both tickets sat on the same matchup, so the replay looked superficially plausible.** That is
> C50's shape — frames labelled with a beat they do not show — and it is why the 48 frames from the
> previous attempt were DELETED rather than staged.

### OPEN — all held, none of it this lane's to start

| item | state |
|---|---|
| **The board's DRAW row** (S74-am) — ruled buildable, waits on nothing | HELD for Allen's word |
| **T94** — column and scorebug describing different legs; its beats are on the closing frames | HELD for the DD's read |
| **`RiskPays`' fact floor** — 378.1 max / 270.6 typical vs a LOCKED 249.0 column | **Allen's item** — it exceeds a locked dimension |
| `CashOut`'s `MARKET SUSPENDED`, 26.7px | T74's table |
| The laptop's margin pin | inherited from main; not this lane's |

---

## 0-T95. PHASE T IS DESIGN-VERIFIED · the crossfade's rect was stale and it was MINE · T95 SHOT · 2026-08-13

**PHASE T IS DESIGN-VERIFIED (T89-cl, batch 63).** The migration is certified on its own variable,
on the closing capture, against a bar pre-committed before the evidence existed. The closing set is
**merged to main at `a487e1e`**; the T95 fix followed at **`c1bee90`**.

**Branch `c1bee90`, pushed and remote-verified.** Suites: **EditMode 251/251 executed, 250 passed, 0
failed, 1 ignored** (+1 — the new T95 pin) · **PlayMode 94/94 executed, 88 passed, 0 failed, 6 skips.**

### T95 — the rect finding, and the attribution belongs here

The DD read a **doubled, illegible scoreline** on score-change beats and offered the cheapest
hypothesis: *"if the second layer holds a stale rect, this seat's own partition ruling is the cause."*

**Measured:**

```
Matchup   box 593.0   centre  92.7
Score     box 675.0   centre 133.7      CENTRE DELTA 41.0px
```

`Score` is the punch overlay and **its own build comment states the invariant verbatim** — *"Same
text, SAME RECT, same face as `_tMatchup` … so superimposing it."* Both are `UpperCenter`, so each
centres its string in **its own** box, and **two centred layers with different boxes do not
superimpose — they offset by the difference of their centres.**

**41.0px is exactly this lane's own `scoreCentreShift` from T91-am.** So: **the ruling was sound and
the implementation was not.** T91-am moved `Matchup`; the mirror was never re-derived, which §3.5
obliges — with the file warning about it in prose at the exact site. **New, a regression, and mine.**

**Fixed by CONSTRUCTION** — one position, one size, both layers, hoisted into shared locals. The same
remedy T68 needed for an ink with five authors and T62 for one value with two repaint schedules.
Measured after: **centre delta 0.0px, superimposed.**

**PINNED:** `T95_the_punch_overlay_and_the_scoreline_share_one_rect` asserts width, height, position
and alignment. **A shared local is a convention; an assertion is a contract** — and this defect was
invisible to every instrument this surface has, caught only at review distance on frames. It fails
against the broken state by construction (593 vs 675 on the first assert); not re-run against a
reverted tree.

> **THE TRANSFERABLE RULE: when a ruling moves a box, every layer that mirrors it moves too — and the
> mirror is found by grepping for the rect, not by remembering.** Two elements agreeing by convention
> is a defect waiting for the next ruling.

### T91-am2, folded in the same pass

**The 2px ink floor applies to BOTH sides of the ticket column's edge** — *"an edge has two sides and
a floor on one of them is half a rule."* Territories now derive from a usable stage of **711.0**, not
the band's raw 715.0. `Matchup` starts at **−221.8, exactly 2.0px** right of the column edge; widest-
scoreline clearance to the clock **31.3px**.

### The T95 capture — SHOT AND STAGED, 159 frames across two entry points

**Both entry points were needed and neither substitutes for the other:** `Capture_Batch22_…` carries
the two frames the defect was read on — `t68am-accept-slot` **frame008** (lead-change) and
`t70am-live-pair` **frame000** (leg-resolution) — **under identical filenames**, and only
`Capture_SeatedSweat_NamedMoments` carries the **`goal`** moment.

| set | result | frames |
|---|---|---|
| `batch22-payoff-and-live-pair/` | **Passed 1/1** | 66 · one seed, one boost |
| `namedmoments-goal-five-seeds/` | **Passed 5/5** | 93 · five seeds, one boost |

Staged at `dd-import/tv-t95-transitions-2026-08-13/`, two subdirectories because their pins differ,
one README carrying the beat map, the before/after rect arithmetic and the non-claims.

**All three beats covered:** lead-change (30), leg-resolution (4 + 38 across three moments), goal (40
across all five seeds).

### A DETACHED CAPTURE OUTLIVES THE SESSION THAT LAUNCHED IT — verify before re-running

**A 403 killed the driving session mid-poll on the second run.** The capture did not die with it: it
had been launched **detached** — the rule §4 wrote after three silent capture deaths — and it ran to
completion on its own, writing its results XML (`Passed 5/5`) and logging `capture complete` for all
five seeds.

**The resumed seat verified that from the artifacts rather than re-running**, which saved a ~20-minute
window. **The check is the results XML and the harness's own completion lines, not the frame count** —
93 frames matching an earlier run is suggestive and is not proof.

**And the dead-partial trap was live here:** the capture directory held a *previous* interrupted
attempt's frames as well. The two runs are separated at **23:13:50** and were scoped by mtime on
either side of it, so the passed set and the abandoned one were never mixed — §0-FR's recorded
instrument failure, avoided rather than rediscovered.

### OPEN, none of it gating the verified phase

| item | where |
|---|---|
| **`RiskPays`' fact floor** — 378.1 max / 270.6 typical vs a **locked** 249.0 column. **The one item that reaches Allen**, because it exceeds a locked dimension | T74-am6 / batch 63 |
| `CashOut`'s `MARKET SUSPENDED`, 26.7px | T74's table |
| bare `TO SCORE` names no player — the ladder working, not a backstop firing | routed batch 62 |
| **T94** — column and scorebug describing different legs | batch 62 |
| `SweatFlavor` renders a DRAW as AWAY flavour | markets lane, queued |

---

## 0-CL. THE CLOSING CAPTURE — shot and staged · the phase awaits the DD's re-read · 2026-08-13

**Branch `5dadc24`, pushed and REMOTE-VERIFIED. Tree clean, Unity ZERO, no lockfile — the granted
window is SPENT, on one pass, as ruled.**

**TOP LINE: every ruling through batch 62 is built, measured and shot. Nothing in this lane is open
that gates Phase T.** The phase now waits on **one thing: the DD's re-read against T89** of
`dd-import/tv-phase-t-closing-2026-08-13/`. A fresh seat reads §0-PV for the verdict's shape, this
section for standing, and waits.

| suite | discovered | executed | passed | failed | skipped |
|---|---|---|---|---|---|
| EditMode | 250 | 250 | 249 | 0 | 1 ignored — G1's grant, held |
| PlayMode | 94 | 94 | 88 | 0 | 6 by-design capture skips |

**Sweep: overrunning 1 of 22 · 48 slots · 0 unaccounted for.** The one survivor is `CashOut`'s
`MARKET SUSPENDED` (26.7px), on T74's table by name and **not part of T89's gate**.

### G1-am7 — the moneyline ladder, measured BEFORE it was built

The DD explicitly declined to assert rung 2 (§2.5/C41): the arithmetic suggested *"somewhere near
247px"* and called that **a direction of travel, not a number to land on**. Measured, all twenty
against the 261.0px column:

> **RUNG 2 OVERRUNS FOR 0 OF 20.** Widest form actually reached: **`SPREADSHEETS WIN` 249.5px, 11.5px
> spare.** Rung 1 overruns for 5, so exactly those five fall to rung 2.

**Pre-commitment 1 fired: the ladder is final and nothing returned to the DD.** Built as
`needFallback: $"{club} WIN"`; bare `TO WIN` is retired and now unreachable on this arm, which is the
point — T94's desync means a bare form would name no side during the very window that made naming
necessary.

### THE INSTRUMENT LESSON THAT CAME WITH IT — and it is the durable part

**A ladder-blind sweep certifies boxes against strings the surface can COMPOSE and can never DRAW.**
`SPREADSHEETS TO WIN` at 289.9px is never rendered — `FitOrFallback` picks the next rung — so
measuring it is the `BRICKLAYERS ANYTIME` error **arriving from the opposite direction**: not a string
that cannot exist, but one that exists and cannot reach the box.

The sweep applies **ladder selection before measurement** now, derived rather than hard-coded as
"these five use rung 2" — a hard-coded split goes stale the moment the box or the face moves, which
this lane has already paid for once.

**AND MODELLING ONE LADDER EXPOSED THE OTHERS. Each time a rung was modelled the false overrun moved
one arm over:**

```
moneyline laddered  ->  widest became `ONE TEAM SCORELESS` 272.4   (also a ladder)
BTTS laddered       ->  widest became `PAVEMENT TO SCORE`  264.9   (also a ladder)
scorer laddered     ->  widest is `ONE TEAM BLANKED` 252.5 — FITS by 8.5
```

**THREE arms of the NEED deck are ladders, not one.** I had written in that very table that the
scorer arm need not be modelled *"because no surname form reaches the box at all"* — **the next run
falsified it.** Corrected in place; all three are transcribed from `ActiveLegCopy`'s construction
sites rather than inferred.

### The capture

`Capture_Batch22_StatementFit_And_PayoffBeats`, with graphics, launched **detached** and waited
**in-turn**. **Passed 1 of 1 · 66 frames · 189.7 MB**, staged as a directory with a README.

**One capture, five fixes — no separate verification pass**, per T90-am's own economics: a
verification window that is not the closing window spends a capture to learn what the closing capture
reports anyway.

**THE DECISIVE FRAME IS IN IT UNDER THE IDENTICAL FILENAME** —
`…scene002__grammar-LegFinalWon__moment-t70am-live-pair__frame000.png`, the frame T89-am read the
refusal on. Before: `ONE TEAM BLANKED`. Refused set: `ONE TEAM`. **This set: `ONE TEAM BLANKED`,
complete and unobstructed.** Same entry point, seed, ticket, scene, grammar, moment and frame index as
66 of the pair's 151 frames — identical, not analogous.

Pins asserted: one seed (`48151623`), one boost (1.4), one scene and one grammar per payoff beat,
mtime-scoped against a directory holding 254 PNGs.

### THE GATE, as T90-am ruled it

**The property, not the string: the truncation backstop fires on NOTHING in the NEED line.** Measured
across the whole deck over the closed pools — every arm's widest *rendered* form fits 261.0, the
widest being `ONE TEAM BLANKED` at 252.5.

### OPEN, and NONE of it gates the phase

| item | where |
|---|---|
| **bare `TO SCORE`** — the scorer arm's rung 2 names no player, the same property G1-am7 retired bare `TO WIN` for, one arm over, same T94 reason. **Not a gate failure: the gate is that the TRUNCATION BACKSTOP does not fire, and an authored fallback rendering complete is the ladder working** — T89-A's own example is exactly that. Authoring it is the DD's; G1-am7 scoped itself to the moneyline | routed batch 62 |
| `CashOut`'s `MARKET SUSPENDED`, 26.7px | T74's table |
| `RiskPays`' fact floor, 378.1 max / 270.6 typical vs 249.0 | T74-am6 |
| **T94** — the column and scorebug describing different legs; its won/dead beats are ON these frames (`t70am-live-pair` LegFinalWon, `t71-win-tally-slot` LegFinalLost), owed after the close | batch 62 |
| `SweatFlavor` renders a DRAW as AWAY flavour | markets lane, queued behind the close |

---

## 0-BU. BUILT, NOT CAPTURED — the phase waits on ONE authored string · 2026-08-13

**Branch `43896ac`, pushed and REMOTE-VERIFIED. Tree clean. Unity ZERO, no lockfile — the granted
capture window is UNSPENT and deliberately so.**

**TOP LINE: batch 61's whole set is built, measured and green. The closing capture is HELD.** Phase T
now waits on exactly two things, in order: **one authored string from the DD (G1-am6), then one
capture.** Nothing else in this lane is open.

| suite | discovered | executed | passed | failed | skipped |
|---|---|---|---|---|---|
| EditMode | 250 | 250 | 249 | 0 | 1 ignored — G1's grant, held |
| PlayMode | 94 | 94 | 88 | 0 | 6 by-design capture skips |

**Sweep: overrunning 5 → 2 of 22 · population 48 slots, 0 unaccounted for.**

### What batch 61 ruled and what landed

| ruling | built | measured |
|---|---|---|
| **T90-am** lever 1 | `MomentumLabel` **dropped** — field and both geometry constants with it | the 88.0px overlap is gone |
| **T90-am** lever 2 | `LegRowNeed0` **249.0 → 261.0** | **`ONE TEAM BLANKED` (252.5) fits with 8.5px spare, unobstructed** |
| **T91-am** leg row | `LegRowState0` **38.0 → 44.0**, right edge AT the floor | ink clearance **1.3 → 7.3px**; its own 4.7px overrun **retires** |
| **T91-am** band | `Clock` **140.0 → 80.0**, `Matchup` **675.0 → 593.0**, centred in its own territory | widest scoreline **−13.7px COLLISION → 27.3px clear** |
| **T92-am** | the leg list **left** `TakeoverSub`; slot **655 → 695** | overrun 10.9 → fits; no cap, no growth, no deviation |

**Shortening was never a route and the ruling struck it:** the caption's overlap was **positional** —
the box is placed, not sized, by its string — so a zero-width caption would have left all 88.0px.

**T92-am's pre-commitment was CHECKED, not assumed:** `RenderTicketCard` does not populate the column
because `ResetForNewSession → RenderPregame` already did, so the column is showing those legs when the
card draws. The list was **not** load-bearing.

### NEW LAW TO CARRY: the ticket column's side padding is RULED

**8px nominal, and NO element's ink comes within 2px of the column edge.** It stopped being informal
the moment two independent fixes proposed to spend the same allowance on different rows of one
column — C46's disease exactly, an implicit contract nobody wrote down. It lives as `ColumnInkFloor`
in `BuildTicketColumn` with the ruling written at the site. **A third consumer is ruled against it
rather than discovering it is gone.**

### THE GATE MOVED: it is the PROPERTY, not the string

T89-A's parenthetical named `ONE TEAM BLANKED`; **T90-am ruled the gate is that the backstop does not
fire on the NEED line at all.** Narrowing it to one string after seeing which string failed would be
reading the condition against the evidence. **Both arms must clear on the closing frames.**

- **`ONE TEAM BLANKED` arm — CLEARS.** Built and measured above.
- **`{CLUB} TO WIN` arm — DOES NOT, and cannot until the DD authors.** 5 of 20 clubs still overrun.

### G1-am6 — the owed fact answers **NO**, so disposition 2 fired

> *Does the marker identify the backed side, and is the NEED leg always on the fixture the scorebug is
> showing?*

**First half YES** — `isMl && pickedHome`, moneyline-only, which is exactly this arm.
**Second half NO**, and it is a code path, not a guess: at **`TvSweatScreen.cs:1652–1653`** the
column's live row (the only row that renders NEED) advances to leg **N+1** the instant leg N resolves,
while the scorebug keeps leg **N**'s fixture until the next leg stages. **The window spans the whole
won/dead beat with the column on screen throughout.** So `TO WIN` alone would leave the live leg's
side unnamed. **The club must be named. Nothing was authored here.**

**The pool went to the DD as WIDTHS, not words** — `dd-import/tv-g1am6-pool-2026-08-13.md`. Length
would have picked the wrong champion: `GRAVEDIGGERS` and `SPREADSHEETS` are both 12 characters and
differ by **0.4px**, while `LONGHAULERS` and `BRICKLAYERS` are both 11 and differ by **17.0px**.

| overruns 261.0 | width | over |
|---|---|---|
| `SPREADSHEETS TO WIN` | 289.9 | +28.9 |
| `GRAVEDIGGERS TO WIN` | 289.5 | +28.5 |
| `LONGHAULERS TO WIN` | 282.4 | +21.4 |
| `BRICKLAYERS TO WIN` | 265.4 | +4.4 |
| `REGULATORS TO WIN` | 264.1 | +3.1 |

15 of 20 fit; the authored bare form `TO WIN` is **93.7px**. **Two of the five miss by under 5px**, so
any rule keyed on word length would leave those failing — **the selector has to be measurement**,
which is already the mechanism (`FitOrFallback` picks the authored form if it fits, never truncates
to choose).

### WHY THE CAPTURE IS HELD, and it is the lane's own standing argument

Firing now would produce frames **known in advance to fail the gate** on the moneyline arm, and would
guarantee a second window. That is the cost C17's economics avoided when this residual was flagged
*before* the last window rather than after it — the DD credited it as the most valuable thing in that
submission, and shooting anyway would spend exactly what the flag saved.

### WHAT THE NEXT SEAT DOES WHEN THE STRING LANDS — in order, and the capture is not step one

1. **Implement the authored moneyline NEED form.** Selection stays by MEASUREMENT — `FitOrFallback`,
   never the backstop.
2. **Re-measure the pool.** `SBR/TV/T88 prompt composition` already prints all 20 against the box; the
   number to see is **0 of 20 overrunning**.
3. **Suites**, both, to the counts above.
4. **Then the capture** — `Capture_Batch22_StatementFit_And_PayoffBeats` is the entry point that
   carries the BTTS-NO leg and therefore the NEED line's authored form; **the named-moments ticket
   cannot render it** (moneyline + moneyline + AnytimeScorer). Launch **detached**, poll **in-turn**,
   scope by **mtime**, assert the pins.
5. **Stage as a directory** with a README, alongside the two sets already in `dd-import`.

### STILL OPEN AND NOT GATING

`CashOut`'s `MARKET SUSPENDED` at 26.7px over — T74's table, added there by name at batch 60 after the
two-row change did **not** retire it (it was already exclusive in its slot, so sharing was never its
problem). `RiskPays`' fact floor. And the markets lane's `SweatFlavor` draw-as-away, queued behind
the phase close (§0-PV).

---

## 0-PV. THE PHASE VERDICT — five conditions MET, one REFUSED · T90 is the last blocker · 2026-08-13

**Branch `f343df3`, pushed and REMOTE-VERIFIED.** Tree clean, Unity zero, editor released. Two commits
on top of §0-CC.

**TOP LINE: Phase T is NOT Design-verified, and it waits on ONE thing — the NEED line rendering its
authored form complete on frames (T90).** Everything else T89 named is met. **C31 binds both ways and
the DD said so plainly: that is the whole remaining list.** T91/T92/T93 are new items opened on these
frames and **none of them withholds the grant** — they are the next docket.

| suite | discovered | executed | passed | failed | skipped |
|---|---|---|---|---|---|
| EditMode | 250 | 250 | 249 | 0 | 1 ignored — G1's grant, held |
| PlayMode | 94 | 94 | 88 | 0 | 6 by-design capture skips |

Sweep: **5 of 23 overrunning, 0 unaccounted for.**

### What the verdict settled

**GRANTED on frames:** the money control (A-b) — `CASH OUT $131OLD E`, the collision T63/T68/T68-am/T71
chased across four batches, **is gone and structurally cannot recur**; the two-row composition was
judged better than the span would have been. **SIGNED:** the 3px, branch 1, C16 deviation (cost 3.0px
of vertical margin, expiry the deferred sizing pass) — *"the question was never the 3px, it was INK
versus LINE BOX."* **DISCHARGED:** all three parked pre-commitments — `CashOutStatus` stays Regular
(the row split did the work a face swap was being considered for), `Clock` corroborated, `G1-am4`'s
digit rows re-certified. **B/C/D/F all MET.**

**REFUSED:** A(a), the NEED line. On the after-set it renders **`ONE TEAM`**. T89-A pre-committed this
refusal in advance — *"frames showing a tidy truncation are not frames showing a fix"* — and the DD
held itself to C31 rather than softening a condition on seeing the evidence.

### A SCOPING ERROR OF MINE, corrected by the DD, and the direction matters

I reported T89-A's *"same seed **and** ticket as the pair's own frames"* as **jointly unsatisfiable**,
on the premise that the pair was the named-moments capture. **The pair is the UNION of BOTH entry
points** — 85 named-moments frames + 66 batch-22 frames = T83's own 151 — and the pair's README says
so. So the batch-22 after-set is the same entry point, seed, ticket, scene, grammar, moment and frame
index as **66 of the pair's 151 frames**, and the before half of that very frame renders
`ONE TEAM BLANKED`. **The condition was met exactly as written; I under-claimed my own coverage.**

**Adding a BTTS-NO leg to the named-moments ticket is REFUSED** — it would change the pair's own
construction after the pair is shot, voiding the instrument. **Nothing is owed.** Recorded as a
scoping error rather than a fidelity failure: the dangerous version is claiming a condition met that
is not, and this was its mirror.

**Also corrected upward, and it is the DD's own (§1.5):** `LegRowNeed0` was routed into T74 — a pass
T89 explicitly defers out of the phase — which put a closing condition inside a deferred pass. **It
comes OUT of T74's table.** T84's distinction governs: a span change is not a size-authority question.

### T90 — what I measured, and TWO corrections to the ruling's premises

| element | x | width |
|---|---|---|
| `TicketColumnZone` | −488.8 → −223.8 | 265.0 |
| `LegRowNeed0` | −480.8 → −231.8 | 249.0 |
| `MomentumLabel` | −319.8 → −223.8 | 96.0 |

`MomentumLabel` sits **entirely inside the ticket column**, overlapping the NEED line by **88.0px of
box** in a shared y band. It is **right-pivoted**, so its 10.4px overrun spills **left, further into
the column** — the NEED line is clear for only **150.6px of its 249.0px box**. The tape itself is not
in the band; only its caption is.

**TWO DEFECTS AT ONCE:** `ONE TEAM BLANKED` is **252.5px, over its box by 3.5**, so the word-boundary
backstop drops `BLANKED` and ships `ONE TEAM` (130.1px) — **the only form that also clears the
caption**. It is truncated *and* its tail would have been overprinted anyway.

1. **"Shortened" is not one of the three routes.** The overlap is **positional** — the box is placed,
   not sized, by its string — so a zero-width caption leaves all 88.0px. Only re-placing or dropping
   works, and the cheapest-looking option was the one that does not exist.
2. **The ≥40.9px threshold does not decompose as written.** Retiring the caption recovers **98.4px of
   CLEAR RUN and 0px of BOX**, because NEED already spans the column's full usable width. **The 3.5px
   survives the furniture change.** Both levers are needed; both are available.

**PROPOSED (composition explicitly the DD's):** drop `MomentumLabel`; widen `LegRowNeed0` 249.0 →
261.0 inside the column's own 8px padding — needs +3.5, has +16, outer width unmoved (T46/R30).

**THE RESIDUAL, flagged before a capture window rather than after it:** `SPREADSHEETS TO WIN` is
**289.9px — 24.9 past even the full 265.0 column**, and seed `48151623`'s frames carry `SPREADSHEETS`,
so the pair's own ticket probably shows the moneyline arm truncated on the row above. **Under G1-am,
the span route demonstrably failing is the stated condition that reopens authoring — for that arm
only.** Routed, not authored.

### T91 — measured, and worse than the frames showed

| pair | box gap | INK clearance |
|---|---|---|
| price → state | 6.0 clear | **1.3px** |
| scoreline → clock | 130.0 overlap | 2.5px read seed · **−13.7px on the widest scoreline** |

**Both are ALIGNMENT failures, and the box gap is the wrong quantity.** The leg row's clearance does
not move with the price (`-280` and `+1200` both 1.3) because right alignment pins the ink to the box
edge — **the binding element is `NEXT` overrunning its own 38.0px box leftward.** Lever: grow
`LegRowState0` 38.0 → 46.0 rightward into padding; retires its 4.7px overrun in the same move.

**The scorebug is not a pure clearance fix:** bounding `Matchup` at the clock's edge gives 545.0px
against a 583.3px widest — 38.3 too small. Numbers supplied, composition routed.

### T92 — BUILT. Width solved, height routed

`TakeoverSub`'s entries take G1's compact forms, one per row — `LegStatement` is the deck, no new
authoring. **One row 760.8 → 204.6px; the slot's overrun 2479.6 → 10.9px** — and the remaining 10.9 is
a **different string**, the deferral line at 665.9.

**Height is a call and I did not invent one:** 2 rows fit (45.0/60.0), 3 over by 7.5, 4 (`MaxLegs`)
over by 30.0. **A cap of 2 legs plus a count row is 3 rows, which also overruns** — the cap that fits
is 1 leg + a count, which is not a list.

### THE INSTRUMENT TRAP THIS SECTION PAID FOR

**`GetWorldCorners` is degenerate on an inactive canvas.** The first cut of the geometry block used it
and reported **every rect 0.0px wide at x≈1.2, with every gap "touching"** — numbers that would have
read as a finding. `rect.size` IS valid (it is sizeDelta with fixed anchors, and it had already
produced every box width in the file); only the world transform is unresolved without a layout pass.
**Positions must be accumulated up the parent chain.** Every width in this section cross-checks against
the sweep, which is how the bad numbers were caught.

**And T93 adopted a standard that started here:** *print an UNACCOUNTED-FOR count, and it must be
zero* — C18 §4.1 built into the instrument rather than promised by it.

### QUEUED BEHIND THE PHASE CLOSE — routed in from the markets lane, no action taken

**`SweatFlavor` renders a DRAW as AWAY flavour** — a two-way assumption living in this surface's code.
Routed here (Allen's word) because **it survived the markets lane's own class sweep for the reason
that matters: it is in TV's file, not theirs.** A cross-lane sweep scoped by ownership misses exactly
the code another lane owns, which is the same shape as a sweep scoped by a list rather than by the
population.

**It joins this lane's queue AFTER Phase T closes. Nothing is owed now and nothing was investigated.**

**Likely site, from a read already in hand this window and NOT verified against the defect:**
`SweatFlavor.PickedHomeForPresentation` resolves to
`leg.Selection.Kind != MarketKind.Moneyline || leg.Selection.Choice == MarketChoice.Home` — a
Home/not-Home predicate with no third arm, so a Draw selection would fall to the away branch. Treat
that as a pointer for whoever picks it up, not as a diagnosis.

---

## 0-CC. THE CAPTURE CLOSE — batch 59 determined · both frame sets staged · 2026-08-13

**Branch `6916d7c`, pushed and REMOTE-VERIFIED.** Tree clean, **Unity zero, lockfile clear, editor
released.** One commit on top of §0-FP.

**TOP LINE: there is no build work and no capture work left in this lane.** Phase T's evidence is
staged. **Five items are open and every one is a DD call.** A fresh seat reads §0-PT for the phase's
shape, §0-FP and this section for standing, and then waits.

| suite | discovered | executed | passed | failed | skipped |
|---|---|---|---|---|---|
| EditMode | 250 | 250 | 249 | 0 | 1 ignored — G1's grant, held |
| PlayMode | 94 | 94 | 88 | 0 | 6 by-design capture skips |

Sweep: **5 of 23 overrunning** (the population grew — see the residual below).

### Batch 59's two rulings, both DETERMINED

**T74-am5 — `RiskPays` is ONE ROW, both ends anchored. BUILT.** `RiskPays` keeps its name and carries
the RISK half — it is in the C8 protected set and a LayoutGrid test finds it by that name, the same
reason the money figure kept `CashOut` — and a new `Pays` element takes the right-anchored half. The
five-space spacer is gone; it was the thing being measured, not the content. Each half now fits alone
(138.4 and 239.7 in 249.0).

**BRANCH 2 fires on the pair, and the fact floor is not a tail case:**

| | RISK + PAYS | vs 249.0 |
|---|---|---|
| bank $10,000 | 138.4 + 239.7 = **378.1** | over by 129.1 |
| **typical** | 124.7 + 145.9 = **270.6** | **over by 21.6** |

**The footer collides at ORDINARY values** — a $1,234 stake paying $12,340 is a plain 10× parlay. So
this is T74 proper with a fact floor of **378.1px against a 249.0px row**, and it is not reachable only
at the maximum. C49 stands; nothing here reopens abbreviation.

**T84-am6 — the 3px lands in the control's own PADDING. BRANCH 1.** Measured on the face's own metrics
rather than a mesh:

```
figure  ink 27.9px (cap 21.5 → descent -6.4) in a 34.0px rect — 6.1px spare
status  ink 14.4px (cap 11.1 → descent -3.3) in an 18.0px rect — 3.6px spare
```

**The 3.0px was always a LINE-BOX figure** — `GetPreferredValues` returns the typographic line
including leading. The ink never leaves either rect, so it never reaches the zone edge, let alone
`TicketFooter` on the other side of it. **A magnitude, not a collision → signed C16 deviation: cost
3.0px of vertical margin, expiry the deferred sizing pass. The phase proceeds.**

### The two residuals — and one of them found a live defect

**T89-B §4.2, DERIVED not asserted.** The report used to say "N of 20 swept" beside "48 slots exist",
which invites exactly one wrong reading: that 28 slots go unexamined. The sweep now classifies every
slot it can see and prints an unaccounted-for count **that must be zero**:

```
49 text slots · 23 swept · 25 the same construction at another row index
· 1 declared unswept (BigAmount — it renders no string at all) · 0 UNACCOUNTED FOR
```

**Closing that gap found a defect.** Two of the three slots it named were trivially enumerable, so
they were swept rather than excused — and **`MomentumLabel` is `MOMENTUM` at 106.4px in a 96.0px box.
A CONSTANT WITH NO VARIABLE IN IT**, over budget, overflowing on every frame it has ever drawn,
invisible because nobody had ever swept the slot. Same class G1 found twice. (`Leg` fits, 52.5/140.0.)

**`TakeoverSub` HAS A BOUND** — "unbounded" was the payout maximum's error a second time. Enumerated
over the same 648,000 offers (`engine.tests/TakeoverSubBoundTests`): longest entry 91 chars, joined
worst case 385 chars = **3134.6px in a 655.0px box**. And **the list ruling does not reach it: one row
alone is 760.8px, over by 105.8px.** The entry is the engine's concatenated Moneyline label — T69's
"a fact named twice", still rendered raw here. **The remedy is the ENTRY, not the composition: a list
of over-wide rows is still over-wide.**

### The capture window — CLOSED, both sets staged

Both launched **detached** and waited **in-turn** throughout (§4 rule 4), with graphics.

| set | harness | result | frames |
|---|---|---|---|
| `tv-phase-t-afterframes-2026-08-13/` | `Capture_Batch22_StatementFit_And_PayoffBeats` | Passed 1/1 | 66 · 187.8 MB |
| `tv-phase-t-afterframes-namedmoments-2026-08-13/` | `Capture_SeatedSweat_NamedMoments` | **Passed 5/5** | 93 · 245.9 MB |

Both in `main-2/docs/design/dd-import/`, **directories not zips** (paths on disk; no transport cap),
each with a README carrying pins, coverage and what is not claimed. **Pins asserted, not assumed** —
the capture directory accumulates (254 PNGs live there), so each set is scoped by **mtime to its own
window** and then pinned: one boost across both, one seed and one grammar and one scene per payoff
beat, frames contiguous.

**THE STRUCTURAL FACT WORTH CARRYING, because it reversed my own report mid-window:**

> **The two harness entry points build DIFFERENT TICKETS.** Batch-22 is
> **BothTeamsToScore(NO)** + moneyline + TotalGoals + AnytimeScorer. Named-moments is
> **moneyline + moneyline + AnytimeScorer**.

`ONE TEAM SCORELESS` / `ONE TEAM BLANKED` is the **BTTS-NO leg's** NEED line, so **the named-moments
set cannot render it on any seed, at any moment, ever** — and the batch-22 set carries it by
construction. I told the DD the opposite before checking the ticket, and corrected it in both READMEs
rather than editing the wrong claim away.

**A T89-A condition that is jointly unsatisfiable, stated rather than worked around:** it asks for the
NEED line *"on the same seed AND ticket as the pair's own frames."* Same seed is met (batch-22 runs on
`48151623`, the pair's first). **Same ticket cannot be met by any run of this harness**, because the
string does not exist on the pair's ticket. Satisfying both would mean adding a BTTS-NO leg to the
named-moments ticket — a change to the pair's own construction, and therefore the DD's call, not
something to slip in under a capture window.

### THE FIVE OPEN ITEMS — all DD calls

1. **`RiskPays`' fact floor** — 378.1px max / **270.6px typical** against a 249.0px row (T74 proper).
2. **The money control's 3px** — branch 1 determined; the **signed C16 deviation** is the DD's to sign.
3. **`MomentumLabel`** — 10.4px over, a constant with no variable in it.
4. **`TakeoverSub`'s entry** — the composition does not fix it; the engine's Moneyline label does.
5. **The BTTS-leg question** above, if ticket identity is load-bearing for T89-A's comparison.

### Two operational traps this window paid for

- **Unity's working directory in batchmode is the PROJECT path, not the repo root.** Capture output
  lands in `unity/SBR/artifacts/tv-sweat-capture`, and my first poll watched `<repo>/artifacts` and
  reported `files=0` for a run that was writing frames the whole time. Liveness is artifact mtime —
  of the *right* artifact.
- **Capture pacing, measured:** ~5 minutes per seed at ship pacing; the five-seed set took ~24 minutes
  wall. That is three polling calls, not one — size the window accordingly and never hand the turn
  back against a live capture.

---

## 0-FP. THE FIX PASS — batches 56/57/58 built · TWO COMPOSITION CALLS WITH THE DD · 2026-08-13

**Branch `db5157f`, pushed and REMOTE-VERIFIED.** Tree clean, Unity zero. Two commits on top of
§0-GC: `f84d431` (the fix pass) and `db5157f` (the payout maximum).

**TOP LINE: the fix pass is built and green. Two composition calls are routed and nothing else is
open in this lane.** §0-GC's four DD calls are all ANSWERED by batch 56 — do not chase them.

| suite | discovered | executed | passed | failed | skipped |
|---|---|---|---|---|---|
| EditMode | 250 | 250 | 249 | 0 | 1 ignored — G1's grant, held |
| PlayMode | 94 | 94 | 88 | 0 | 6 by-design capture skips |

Sweep: **5 of 20 overrunning**, and **no pair collisions on the money control** — retired by
construction, not by a number.

### What landed

1. **`N LET IT DIE`** — no `HOLD`. It rode in under *"unchanged"*, which was true, and never met
   T88(c), the ruling that changed its class. The prompt was printing `HOLD` over a press: **C48's own
   defect surviving inside the pass that fixed C48.** *A string that never meets the ruling that
   changed its class is the same defect one level up* — and it was caught in the "listed so the set is
   the whole set" section, which is why that section exists.
2. **`SHOT FROZEN` left the zone**, and the zone did not grow. Three option rows are 82.5 in 90.0 and
   fit **in every ownership combination**, not only when one consumable is held (C46 forbids leaning on
   the common case). Its optional home is measured and available — 156.4px on the 651.0px event strip,
   494.6 spare — and deliberately **not built**, because it is not required.
3. **Attract states 4+5 → `BOARD CLOSED`**, `flavorColor` in both. `won` no longer selects an ink
   there; the §3.1 gold ration violation is gone.
4. **The money control's members stopped sharing one rectangle.** All three pair collisions retired,
   including the 198.5 this lane created.

### THE PAYOUT MAXIMUM — the arithmetic nobody had done, and it holds

*"Unbounded" was the wrong word: parlay multiplication is **UN-ENUMERATED**.* Enumerated over the
generator's own offer space (`engine.tests/PayoutMaximumTests`, re-runnable):

```
648,000 offers priced · 3,000 seeds · max single-leg odds 52.0359 (true prob 0.018302)
produced by AnytimeScorer — the one-way YES-only board, the only market with no
complementary side to bound it. Last improved at seed 1051 of 2999.
PARLAY TERM = 52.0359^4 = 7,331,837.65 x stake
```

**The stake ceiling is the BANK** (`MaxStakeFraction` 1.0) — run state, not config — so the figure is
stated per bank assumption rather than as one number that buries its dependency. Tabular condensed
Bold 700 digits advance **13.6px each**, so the 249.0px row holds **eleven digits and not twelve**:

> **Any payout below $100,000,000,000 — i.e. any stake up to $13,639.** The payment schedule tops out
> at $1,350 and totals $2,910 across all eight rounds, so the threshold sits **~4.7× above the entire
> run's payments. Width CLOSES with no further ruling.**

Two caveats, both moving the threshold **down**: a bank above $13,639 is reachable after one large
win (the box fails at twelve digits and nowhere before), and **`PayoutMultiplier` — up to eleven named
relic factors — is a further multiplier ON TOP and is deliberately excluded**, because the DD's formula
named `MaxLegs`, the stake ceiling and the odds range. Folding in a relic product nobody asked for
would overstate the figure while looking more rigorous.

### THE TWO CALLS WITH THE DD — measured, stopped at, not built around

| call | the number | state |
|---|---|---|
| **`RiskPays` two rows** (T74-am4) | width CLOSES (above); **height needs 60.0px in the 40.0px footer — over by 20.0** | batch 57's branch-2 span re-derivation, pre-authorised. Not built: it also needs four elements rather than one |
| **The money control's two rows** (T74-am3) | **55.0px in the 52.0px grid row — over by 3.0** | BUILT and rendering (allocated 34/18, both components Overflow), but it does **not** fit by the sweep's standard and is not claimed to |

**One ruling expectation was falsified and is recorded as such:** `MARKET SUSPENDED` did **not** retire
on the two-row change. It is 267.7 in 241.0 and was **already exclusive** in its slot (T43), so sharing
was never its problem. The 26.7 stands as a plain width overrun.

### THE REUSABLE FACT, and it will bite any height arithmetic on this surface

> **`LineBox` is 1.18. TMP's real advance ratio on this face is 1.25.**

Every height computed from the design constant understates by ~6%. It is exactly why the money control
"fit" on paper (29×1.18 + 15×1.18 = 51.9 in 52.0) and overruns in fact (36.3 + 18.8 = 55.0).
**Measure heights with `GetPreferredValues`; never derive them from `LineBox`.**

### A REAL BUG THE PASS SURFACED — a keyboard that leaves mid-window

`PendingWindowBeat` checks `Keyboard.current` **once** at entry, and every frame of its loop then
dereferenced it. A device going away mid-window — a wireless keyboard sleeping, a controller swap —
threw a `NullReferenceException` **inside the coroutine**, killing the beat and leaving the pending
window hanging **on an irreversible money decision**. **Pre-existing:** the press-era code dereferenced
it the same way. Found only because a gesture test added a virtual device and removed it. Now re-read
every frame and treated as the entry guard treats it: no keyboard, no way to decide, so the window
declines rather than hangs.

### Two instrument corrections in the same family as §0-GC's three

- **A check that outlives its composition.** The sweep's `Pair()` for `CashOut`+`CashOutStatus` kept
  measuring a shared rectangle **after the members moved to separate rows** — it would have reported
  the old build with the new build's confidence. Retired with the composition it measured.
- **My own test assertion went stale against my own copy change** (`HOLD N LET IT DIE`). It failed
  loudly with the actual string in the message, which is the only reason it took one run to see.

---

## 0-GC. THE GESTURE CONNECTED · T89's deliverables sent · BUILD COMPLETE, EVERY OPEN ITEM IS THE DD's. 2026-08-13

**Branch `47d3f7e`, pushed and REMOTE-VERIFIED** (`git ls-remote` reads `47d3f7e` on
`refs/heads/tv-sweat`; the exit code is not the check on this branch — see §0-FR's push caveat).
Tree clean, Unity zero, main local. Six commits: `ec5508c` → `3ce2e60` → `643b96a` → `8dfcb0c` →
`ea82af2` → `47d3f7e`.

**TOP LINE: there is no build work left in this lane.** Phase T's migration, T88's gesture, T89-B's
sweep and T89-C's pass are all landed and pinned. **Four things are open and every one of them is a
DD call, not a task.** A fresh seat should read §0-PT for Phase T's shape, this section for where it
stands, and then wait rather than build.

### Suites — the numbers to reproduce before touching anything

| suite | discovered | executed | passed | failed | skipped |
|---|---|---|---|---|---|
| EditMode | 250 | 250 | 249 | 0 | **1 ignored — G1's grant, held deliberately** |
| PlayMode | 94 | 94 | 88 | 0 | 6 by-design capture skips |

PlayMode was 91/85 before this window; the +3 are the three new gesture pins. **Run PlayMode WITH
graphics** or SureThing's capture tests fail environmentally and read as regressions.

### C48 is law, and what it made this lane do

Batch 50 (`c9ceb77`): *where copy and input disagree on a money control, **the input is corrected to
match the copy**, never the reverse.* T88 then ruled the gesture — hold previews, **release always
abandons**, **a second key during the hold commits**, **no timer, no auto-commit**, a press commits
nothing; and T88(c) keeps the decline at one press, because the weight of the gesture matches the
weight of the act.

**THE FIND, and it is why this was small: §8.10's hold-to-preview was already BUILT and had no
production caller.** `EnterCashOutPreview`, its full-revert twin, `PreviewedBank`, the stepped-down
rows — render-aware, EditMode-pinned, and the only thing that had ever called it was a test, by
reflection. Beside it sat `if (_interact.WasPressedThisFrame()) TryCashOut()`. **The gesture was
never missing; it was disconnected.** Look for this shape before building anything on this surface.

**The asset's own Hold is deliberately NOT used.** `Interact` carries `"interactions": "Hold"`, and
the Input System documents `WasPressedThisFrame` as true on the press *"even if there is an
interaction on the action that has not yet performed"* — which is how a declared hold went unobserved
for a whole phase. Honouring it is the OTHER wrong repair: a HoldInteraction performs on a DURATION,
and T22/T36 rule no timer. The hold is read as a STATE; the commit comes from a key.

**The room boundary is ANSWERED — do not re-litigate it.** `SitSpot` acts on `WasPressedThisFrame`,
**press, never release** (room's source, merged `c8525d1`), and `PlayerInteractor`'s press-poll
deliberately bypasses the action's Hold. My first design guarded a release-path stand that cannot
happen and *introduced* a defect — release E, press E again to stand, stand swallowed. It is gone.
`CashOutLive()` is `CanAcceptCashOutNow() || _cashOutPreview`: it covers the real hazard (a fresh
press arriving during a hold) and not one frame past it.

### THE FOUR DD CALLS, with where the evidence is

All four are staged in `main-2/docs/design/dd-import/` (untracked there, which is that folder's
convention; nothing was committed in another lane's worktree).

| # | the call | evidence |
|---|---|---|
| 1 | **The prompt zone's 20.0px height deficit.** Zone is 635.0 × 90.0 and carries exactly THREE rows at 22px; title + three options is 110.0px. Either +20.0px of zone or `SHOT FROZEN` leaves it. §6's grid does not resize to content, so it is not absorbable here | `tv-batch50-strings-2026-08-12.md` §3 |
| 2 | **The confirm key and three unratified strings.** T22/T36 both say "a second key" and neither names one; `ENTER` is this seat's pick because it is bound to nothing in the room's asset. Strings: `HOLD M MULLIGAN (ONE MULLIGAN SLIP)`, `ENTER CONFIRMS · RELEASE ABANDONS`, `ENTER TO CASH OUT` | same file, §4–5 |
| 3 | **T86(b): Attract's three non-compliant strings.** `SIT TO WATCH THE SWEAT`, `THE HOUSE BLINKS FIRST`, `THE BOOKIE COLLECTS` — outside T27's letter, inside its class. **189.8px of headroom**, so this slot can afford authored copy | `tv-t86b-attract-states-2026-08-12.md` |
| 4 | **The held-preview pair collision, 198.5px**, which THIS PASS created on a known-blocked box (45.0 at rest). Disclosed, not absorbed | `tv-t74-table-2026-08-12.md` pair table |

Plus the standing T74 five and T89's conditions A–F: `tv-t74-table-2026-08-12.md` (tree-stamped),
`tv-traceability-pass-2026-08-12.md`, `tv-tabular-inventory-2026-08-12.md`.

**T89 (batch 54, `0b39315`) pre-commits the closing bar** and C31 binds it both ways: that list is the
whole list, and new findings open new items rather than retroactively withholding a grant. Build to
the bar, not to an opinion.

### THE PASS's transferable finding — an invented string CONCEALS the under-generation beneath it

`LegRowLine0`'s set carried `BRICKLAYERS ANYTIME`, a club noun in the surname slot, which
`{Surname} ANYTIME` cannot emit. It was also **the widest member of its own set** — so it SET the
certified worst case and the real widest producible form was never reached.

> **An over-generated string is only harmless when it is not the maximum. While it IS the maximum it
> hides exactly the under-generation the other direction is looking for — so the two directions
> cannot be swept separately, and finding one does not clear the other.**

Champions are retired: the sweep now GENERATES every form over the engine's closed pools
(`SlateGenerator.Nouns` 20, `PlayerLast` 12), so a name added to either cannot be missed by a champion
nobody re-picked. `LegRowLine0` 152.8 → **144.3, fits by 2.8** (batch 54's first arm: relief
unnecessary, STANDS, no revert). `LegRowNeed0` 272.4 → **289.9, 40.9 over** — it was certified 17.5px
better than it is.

### THE PINS — their seeds and their preconditions

**Three pins, and every assertion is preceded by a precondition, so none can pass by never running.**

- **Cash-out (2 pins), no seed pin.** `DemoTicketPolicy` picks; waits on
  `SitSpot.InteractStandSuppressed()`, which is the public signal that an offer is live and
  acceptable. Preconditions asserted: `kb.eKey.isPressed`, and `_cashOutPreviewAmount > 0` — the
  latter is what makes them falsifiers, since before T88 that field's setter had no caller and reads 0.
- **Intervention M/R, seed `GOLDEN-W2`.** **Not searched for — reused from `CharmExpansionTests`,**
  which pins the same seed and the same hand-built pair and records *"leg 0 (matchup 1, Home) dies;
  leg 1 (matchup 0, Away) would win"*. Ticket is hand-built (`Pick(1,Home)`, `Pick(0,Away)`, stake 20)
  for the capture harness's reason: `DemoTicketPolicy`'s picks are moneyline-only. **Both consumables
  are GRANTED, and that is not convenience — the session suspends into a pending loss only when a
  legal save is HELD** (Mulligan needs ≥2 active legs; Whistle covers any ticket). No save, no window,
  on any seed.
- **Batch mode needs `LetDevicesRunUnfocused()`.** It is never focused, and the Input System's
  documented response to lost focus is `ResetDevice` on every device — so a held key is wiped between
  frames without it. `InputSystem.AddDevice` lives in the RUNTIME assembly, not the test framework,
  which is why a headless run can hold a key at all. The device is added per-test, never in a
  `[SetUp]`: `PendingWindowBeat` declines immediately when `Keyboard.current == null`, and that is
  what stops batch autoplay hanging on the pending window.

### THREE INSTRUMENT LESSONS, all of them mine and all of them the same family

1. **THE VACUOUS FILTER — a test asserting "nothing happened" passes when nothing was attempted.**
   The first press pin went GREEN while the key was never down. S51's shape (green by recording
   nothing), in the pin written to close a coverage hole. **Assert the precondition before the
   property**, every time.
2. **WHAT IS SHOWN IS THE PRECONDITION.** The M/R pin first waited on `HasPendingLoss`, which goes
   true the instant the *session* suspends — but the theatre reaches `PendingWindowBeat` some frames
   later, so the key hit a surface that had not drawn the prompt. **The instrument caught this one
   rather than a human:** it failed with `text='<null>' promptEnabled=False` because the assertion
   named what it needed. Engine state is not rendered state; a gesture acts on what is shown.
3. **THE FACE COLUMN — `TMP_Text.font` names the PRIMARY asset, not the arm that renders.** A slot
   built at `FontWeight.Bold` draws through the bold asset wired by `WireBold` while `font.name` still
   reads the regular one. My new T74 face column therefore misreported, and **the DD's own batch-38
   record was right where my instrument was wrong** ("condensed Bold 700 carries CashOut"). **Three of
   the five survivors are Condensed Bold 700**: `CashOut`, `RiskPays`, `LegRowNeed0`. The sweep now
   prints the resolved weight beside the face.

Two smaller ones in the same family: a **hard-coded expected value goes stale and then reads as a
DEFECT rather than as silence** (a pin of 1015.0 survived one copy change and reported the instrument
broken); and a label doing two jobs — `no digits` printed both for a string with no figures and for
one whose figures are already tabular, **hiding the very confirmation T89-B asks for**.

### Instruments, all committed and re-runnable

`SBR/TV/T88 prompt composition` **(new)** — the prompt's widths, the zone's height, the money
control's pair in all three states. It **refuses to measure** if the copy in `TvSweatScreen.cs` stops
matching (comment-stripped whole-file scan), and reproduces the sweep's own figure as a cross-check.
`SBR/TV/T84 extent sweep` — now generates from the closed pools and prints face, resolved weight,
tracking and tabular basis per row. Plus §0-PT's list, unchanged.

### An operational trap worth one line

**`Set-Location` to the repo root before every Unity invocation.** The PowerShell tool's cwd persists
between calls, and a relative `-projectPath` from a subdirectory silently opens the wrong project —
Unity then wrote `unity/` and `evidence/` trees plus `.meta` files INSIDE `Assets/SBR/Runtime/`.
Harmless once found, invisible until `git status`.

---

## 0-IP. THE INTERVENTION PROMPT — three correct rulings, one box 1.6x its width. 2026-08-12, seat rotating at 97%

**Branch `0a2ef90`, pushed and remote-verified. Tree clean, Unity zero, main local.** Suites
unchanged: EditMode 250 / 249 / 0 / 1 ignored, PlayMode 91 / 85 / 0 / 6 skips.

Read §0-PT below for Phase T's state; this section is the one slot that moved after it, because the
shape of what happened to it is the thing worth carrying.

### The arc

    found          51.1px over its 635px box
    T86(a)        209.8px over   the bracketed-key form retired: [M]/[R]/[N] -> HOLD M/R/N,
                                 in T22's established form ("print the word, not the glyph")
    batch 46      380.0px over   the win-prob GOES, the cost takes its place:
                                 SEND TO REVIEW (99%) -> SEND TO REVIEW (ONE REF'S WHISTLE)

**Every one of those rulings is right, and each made the box worse.** The slot is now 1015.0px in a
635.0px box. Nothing new failed at any step — overrunning slots held at 7 of 20 throughout — the same
one simply got further from fitting. **It needs a T74 answer that is not copy**; there is no wording
at 635px that says what three rulings require it to say.

`(99%)` was T16's quantity, not a different one: `PendingLossProbBefore` is documented in the engine
as *"the leg's displayed win-prob"*. The cost is named from `RelicCatalog`'s own "Ref's Whistle"
rather than a bare `WHISTLE` — an abbreviation nobody authored is G1's class, and this lane withdrew
three findings built on invented strings before learning it.

**MULLIGAN was deliberately left alone.** Identical shape — an offer whose cost of one Mulligan Slip
goes unstated — so if the basis-must-be-an-offer form binds generally it binds there. The ruling
named SEND TO REVIEW. Flagged upward rather than fixed by inference; that restraint is the same one
T73's four-of-eleven bold sites needed.

### TWO OPEN ITEMS ON THIS SLOT, both with the DD

**1. The prompt COMMITS from the theatre.** Traced from source and answered to the DD:

| key | call | commits |
|---|---|---|
| `M` | `Run.PlayMulliganSlip` | `ConsumeConsumable` → `ResolvePendingLossAsMulligan` → `RefreshPhotoFactor` |
| `R` | `Run.PlayRefsWhistle` | `ConsumeConsumable` → `Rng.Derive(...)` → `ResolvePendingLossWithWhistle` |
| `N` | `Session.DeclinePendingLoss` | closes the window irrevocably |

A keypress at the TV spends run inventory, draws from the run RNG, and resolves a leg's grading. No
laptop step, no confirmation, nothing reversible. **Both verbs open with `RequirePhase(Phase.Sweat)`,
so the engine sanctions a Sweat-phase commit — and mid-sweat the theatre is its only possible
operator.** T22's "commit-is-the-laptop's" and this mechanic cannot both stand as written. Ruling
queued.

**2. The copy says HOLD; the input is `wasPressedThisFrame`.** There is no hold gate anywhere on this
surface. That mismatch is this seat's, introduced by applying T86(a)'s "print the word" form to a
press verb, and it folds into the T22 ruling: a press committing an irreversible spend under an
instruction to hold is the concrete case that ruling exists for.

### On the board, not started

Draws reaching the theatre (batch 49): the final beat is the match **ending level, stated** — no
climax, no absence, `decisive` must never reach a slot. Two DD-authored strings join the sweep
population under C46 when they land. Measured headroom for them: **`Flavor` has 140.2px spare** and
can take a stated ending line; **the leg-row state chip has none** — it is already 4.7px over on
`NEXT`, so any new chip word arrives as a T84 blocker on day one. Trace new strings from their
assignment site, including whatever chooses between them.

---

## 0-PT. PHASE T — the type migration SHIPPED AND BLOCKED. 2026-08-12, seat rotating at 97%

**Branch `9459348`, pushed and remote-verified. Tree clean, Unity zero, main local.** Suites on
every commit of this phase: **EditMode 250 discovered / 249 passed / 0 failed / 1 ignored**
(G1's grant, held), **PlayMode 91 / 85 / 0 / 6 by-design capture skips**.

**Phase T's build work is complete. The surface does not ship.** Seven slots overrun their fixed box
and the money control collides; those are T74/T84 rulings, not this seat's to close.

### What landed

TMP behind an unchanged `MakeText` seam — the signature keeps `TextAnchor`/`FontStyle` so all 22 call
sites read as authored. The canon face resolved **by style name**, never index. T73's real Condensed
Bold 700 at its four sites; T77's synthesised italic struck; T85's unruled .02em withdrawn (Label
.16em and Meta .10em ratified as built, NEED 0 on doctrine). T82's tabular figures **derived, wired
and confirmed at spread 0.0000** across four faces and ten measurements.

### The three traps, all of which cost a cycle before they were seen

1. **The face was never what the file said.** `EncodeSans.ttf`'s default instance is
   `Condensed Thin` — wght 100, wdth 75. Legacy `Font` rendered the roman voice **narrower than the
   condensed face**, 241px against 254px on one string. No Regular 400 can do that. Resolve by style
   name; the generator refuses rather than falling back.
2. **A Dynamic TMP asset serializes no character or glyph table.** Anything written there at build
   time is discarded on save — `m_Unicode` entries: 0. That is why `tnum` is resolved into a derived
   font (`tools/tnum_font.py`, cmap only) instead of into the asset.
3. **`GetPreferredValues(s, 0f, 0f)` on a wrapping component returns the widest GLYPH.** It killed
   the compact statement's truncation backstop from T-3 until T84 measured the measurer. Measure
   unconstrained.

### The blockers, tabular-screened and re-traced (batch 44)

| slot | box | widest | over |
|---|---|---|---|
| `TakeoverSub` | 655.0 | 829.8 | 174.8 — **CONSTRUCTED**, `DisplayLabel` is unbounded |
| `InterventionPrompt` | 635.0 | 844.8 | 209.8 — after T86(a) |
| `RiskPays` | 249.0 | 296.5 | 47.5 — payout magnitude unbounded |
| `CashOut` | 241.0 | 267.7 | 26.7 — `MARKET SUSPENDED`, alone in its slot (T43) |
| `LegRowNeed0` | 249.0 | 272.4 | 23.4 |
| `LegRowLine0` | 147.0 | 152.8 | **5.8 — the landed relief is insufficient** |
| `LegRowState0` | 38.0 | 42.7 | 4.7 — widest is `NEXT`; there is no `PEND` |

Plus the money control's two pair collisions, 45.0 and 47.8, neither member overrunning alone.

**`LegRowLine0` cannot be finished by span.** The only slack left is the price column's, and
`OddsFormat.American` returns `+{a}` with nothing bounding `a`. Its own default arm is
`DisplayLabel`, so like `TakeoverSub` **it has no bounded worst case at all.**

### THE LESSON THIS SEAT PAID FOR THREE TIMES

**A sweep is only as sound as its string sets, and I invented sets instead of enumerating them.**
`PEND` was measured on a chip that renders `VOID / NEXT / W / L / ""`. `LANYARD TO SCORE` and
`BOTH TEAMS SCORE` were measured on a slot whose `LegStatement` emits `{SURNAME} ANYTIME` and
`BTTS YES`. `RiskPays` was measured with a three-space separator where the format string has five.
Each was reported to the DD as a finding; each was withdrawn by the pass that read the source.

**Enumerate from the assignment site — grep the field, read every `.text =`, follow every
indirection — and where content is engine-generated, say UNBOUNDED rather than constructing a worst
case and forgetting you built it.**

### Owed, and not this seat's

- **T74**: the seven magnitudes. `LegRowLine0` and `TakeoverSub` need an answer that is not a span.
- **T86(a)**: the wording beyond the retired bracketed form is the DD's to ratify.
- **Attract's three strings** — `SIT TO WATCH THE SWEAT`, `THE HOUSE BLINKS FIRST`,
  `THE BOOKIE COLLECTS` — are T27's class (instruction, celebratory editorial) and need authored
  replacements. `ROUND n OF m · BOARD OPEN` is T27's own ruled string and is compliant.
- **T16 boundary**: `(99%)` is T16's quantity, not a different one — `PendingLossProbBefore` is
  documented in the engine as *"the leg's displayed win-prob"*. Whether T16's ban reaches past the
  momentum tape is routed to the DD.
- **Probe hygiene**: `SBR/TV/T84 extent sweep` and the digit probe have twice exited
  `-1073741819` after printing. Data intact both times and verifiable — results go to `-logFile` in
  the same step that produces them, never a terminal buffer.

### The instruments, all committed and re-runnable

`tools/ttf_faces.py` (a font's real instances) · `tools/tnum_font.py` (derive the tabular font) ·
`SBR/TV/T84 extent sweep` (every box against its longest renderable form) · `SBR/TV/Probe digit
advances` · `SBR/TV/Probe type parity` (UGUI vs TMP) · `tools/tv-phase-t-bootstrap.ps1`
(generate + verify). **The pair is at `dd-import/tv-phase-t-before-2026-08-11/` and
`…-after-2026-08-12/`** — 151 paired frames each side, plus the after-set's unpaired
`scorer-leg-resolved` and its 159-row clock-string manifest.

---

## 0-FR. FLOOD REMOVAL VERIFIED — 2026-08-10. T40 enforced, punch intact, seat rotating

**Converged on new main and re-converged after.** Commits this stretch: `1e02d42` (T40 enforced, the
washes struck) → merged to main at `2217107` → `c6458a0` (font LFS fix, fast-tracked to main) →
`0bab25e` (§1 correction). `origin/tv-sweat` at `0bab25e`. Frames staged at
`dd-import/tv-flood-removal-2026-08-10.zip` (18.9 MB). **DD verdict pending on the frames.**

### The flood removal, measured — three things, one window

Batch 27 struck both full-screen gold washes (T40 enforced). This window verified it on frames, at
the seated acceptance view, both payoff beats, 30 frames each at 1/50s.

| | before (batch 22) | after |
|---|---|---|
| accept — ink across the beat | 0.064 → **0.384** | **0.030 → 0.037** (spread 0.007) |
| accept — CR | 6.47 → **1.70 : 1** | **7.92 – 8.49 : 1**, every frame |
| win tally — CR | 6.58 → **1.86 : 1** | **7.52 – 8.42 : 1**, every frame |
| flood region | 0.063 → 0.507 | **0.028 – 0.034 flat** — gone, not dimmed |

**The third measurement is the one that mattered, and it came from the DD mid-window.** Batch 27
found the flood *redundant* with §6.1's L4 punch rather than carrying it — so "the punch left with
the flood" was the live regression, and it would have degraded **silently while every contrast
number above still landed**:

```
accept      L4 0.6883 -> L3 0.5847    step 0.1036 (15.1%)   at frame 21
win tally   L4 0.6927 -> L3 0.5870    step 0.1056 (15.2%)   at frame 21
```

**Intact on both beats** — one step, held after, at frame 21 = 0.42s = `hdrPunchDuration` at the
capture step. **Carry this shape forward: when an effect is removed because something else was doing
its job, measure the thing that was doing its job.** Contrast reads cannot see it.

### Why the CR landed ABOVE batch 27's pre-commitment, not equal to it

Batch 27 predicted the shipping value from the old capture's frame 0 — *"frame 0 IS the
flood-at-alpha-0 state"* — giving 6.47 accept / 6.58 win. It came in at 7.92–8.49 / 7.52–8.42.

**Frame 0 was not a flood-free frame.** It is one capture step *into* the beat, so `FloodPulse` had
already run its first `SetAlpha` and the wash was faintly up: the flood region reads **0.063** there
against **0.029** now. The whole difference is in the ink, and the ground confirms it — old ground
0.6881, new 0.6877, four ten-thousandths apart. Only the denominator moved: ink **0.0640 → 0.0371**,
which is `goldInk` through the grade with nothing added on top of it. Run the numbers and both fall
out exactly: (0.6881+0.05)/(0.0640+0.05) = 6.47, (0.6877+0.05)/(0.0371+0.05) = 8.47.

**So the pre-commitment was a floor, not a target** — it was measured on the least-contaminated frame
that existed rather than an uncontaminated one, because before the removal no uncontaminated frame
could exist. **A prediction taken from the cleanest available sample of a thing you are deleting
inherits whatever is left of it.** Landing above such a prediction is the expected direction; landing
*at* it would have meant a residue of the flood survived.

### THE INSTRUMENT FIX — a capture directory accumulates runs, and filenames do not separate them

**The first pass over these frames reported the accept beat as 60 frames with CR alternating
8.47 / 1.70 frame by frame. It was measuring TWO RUNS AT ONCE.**

The previous capture's accept frames carry a **different scene-grammar token**, so they do not
overwrite the new ones — both sets sit in the same directory, and a glob on `*moment-<name>__frame*`
collects both. **It would have reported the pre-removal defect as still present**, in the window
whose entire purpose was to show it gone.

**Fix, now in `tools/fr_measure.py` and the rule for any capture measurement:** scope by **mtime to the
current window**, then keep the **newest file per frame index**. Never trust the moment name alone.

**Promoted out of scratch 2026-08-10 on Allen's instruction** — it had lived only in a session temp
directory that is swept on restart, so the "fix" above was one reboot from being a paragraph about a
file nobody had. `tools/fr_measure.py` reproduces every number in this section exactly and hardens
the *selection*, which was the part carrying the risk. The rolling `time.time() - 45*60` cutoff is
gone: the window is anchored, and each run prints a `--since/--until` line that replays its own
selection (round-trip verified). `--expect` (default 30) asserts the count and contiguous indices.
**And the real guard turned out not to be the window at all** — seed, boost, scene and grammar are
all in the filenames, so C34.1's *"an unasserted pin is a comment"* is buildable: the run asserts one
seed, one grammar, one scene, one boost across the selected set and refuses to print numbers
otherwise. Newest-per-index could never have caught **two runs inside one window with a short newer
run** — indices past the new run's end backfill from the old one and the count still reads full. The
pin assert catches that regardless of mtime.

This is the same family as the two fixed scan windows (§0-BW) and the hard-coded material list: an
instrument that silently stops covering what it claims. It is the third distinct shape and the first
where **stale data, not stale code, was the vector**.

### §1's restore rule was wrong two days after writing it — corrected at `0bab25e`

While converging onto new main I **corrupted `SBR.Engine.dll`** by applying §1's own restore method
reflexively. The blob had become an **LFS pointer** at the round; the fast-forward smudged it
correctly to 94,720 bytes; `cat-file` then overwrote a working assembly with the pointer's 130-byte
text. `Bad IL format`.

**And the cmp-verify passed while it was broken** — it hashed the restored file against the same blob
it had just copied from. Pointer against pointer, identical, green.

Two rules out of it, both now in §1:

1. **Check what the blob IS before restoring from it.** `git cat-file -s`: ~130 bytes = a pointer,
   use `checkout`; full size = a raw binary, use `cat-file` through **cmd**. The correct method is
   **opposite** in the two states, and this repo has both.
2. **Verify a restore by USING the artefact, not by hashing it.** `LoadFile` must report
   `SBR.Engine` and a plausible type count. **A comparison against the thing you just wrote proves
   only that the copy succeeded.**

### Fonts converted to LFS — `c6458a0`, already in main

`EncodeSans.ttf` and `EncodeSansCondensed.ttf` were raw blobs under a live `filter=lfs` rule, so any
lane's `git add -A` silently produced **dangling pointers with no object behind them**. Room hit it
first (`a0469b9`, 31 textures); this is the same remedy on the two files this lane owns. Explicit
two-path `--renormalize`, **oids verified resolvable in the local store at full size before
committing**, LFS objects pushed, no ref.

### State at rotation

Converged, clean tree, Unity zero. **Open, none of it this lane's to close:** the DD verdict on the
flood frames; `_tBigAmount`'s inventory call (orphaned since both payoff figures moved into the slot,
flagged at its declaration); `docs/ARCHI.md:267` still asserting the superseded 5–8% G3 band;
`Room.unity`'s orphaned `winFloodDuration: 1`.

**A push caveat worth knowing:** the branch now carries 300+ commits and ~10 MB of LFS objects. The
first `git push` died with `send-pack: unexpected disconnect` and printed `Everything up-to-date`
from the LFS hook — **which reads like success and is not**. The ref had not moved. Check the remote
SHA after every push here; a retry carried it.

---

## 0-BW. THE BLOCKER WAVE — batches 16–19. T67/T68/T69 closed, G1 built, T68-am+T71 landed

**Four batches, one arc: a money control with no readable label, and everything that fell out of
finding it.** Commits `112df65` (batch 16), `41d5cbe` (G1), `04f7739` (T68-am + T71). Suites at the
end: compile clean · engine 160/160 · **EditMode 247/247** · PlayMode 70 executed, 65 passed, 0
failed, 5 `[Explicit]` skips.

### T68 — the blocker. `HOLD E` was invisible on its own field

The DD found it in the T67 capture, which had been shot to answer a different question. **The
inversion is a two-part operation and only the field was inverting**: the type kept its light ink and
the field rose to meet it. At the acceptance view the label measured **1.02 : 1** against the field
it sits on.

**It predates T63 and T63 made it marginally worse** (1.17 : 1 → 1.02 : 1), and the grant stands —
the field's HDR material was genuinely missing.

**Why no instrument caught it, and this is the transferable part:** every T63 measurement compared
the band to *other* elements — scoreline, ball, ticket column, event strip. Three submissions and two
batches of ladder work, **none of it comparing an element to its own ink**. A dominance gate is
silent on legibility; they are different instruments and this surface had only the first (C33-am2).

Fixed by rule: the ink derives **with** the field, from the same predicate, in one authority. Four
scattered ink sites removed — the defect was never the value, it was that the value had five authors.
The per-frame taunt is gated on `!_cashOutFieldLit`; unguarded it repaints the amount gold-on-gold
every Update and undoes the punch-out, which is the same repaint T43 caught once already.

**Verified on frames at 7.95 : 1.**

### The near-miss, recorded because the DD called it the most valuable thing in the submission

On the contact sheet `HOLD E` looked light grey and **this seat was about to file the label as
unfixed**. Measured, it was dark ink at 6.99 : 1. The eye was wrong and the measurement corrected it.

The explanation matters as much as the outcome: the label reads lighter than the amount because it is
**small and thin** — antialiasing and the lit field's own bloom fill in its strokes, so its darkest 2%
reaches 0.350 where the large bold amount reaches 0.216. **Same ink, different stroke weight,
different rendered floor.** On a bloomed field, stroke weight changes what a contrast reading means.

**Standing:** the label is the thinner margin of the two and fails first if the field ever brightens.

### C33-am3 — three instruments, three spaces (and my number was the right one)

The DD computed T68's contrast from Rec.709 luma on display-encoded values and got 3.18 : 1 against
this seat's 8.12 : 1. **A contrast ratio is undefined outside linear space**, so the linear figure was
correct. Ruled into law:

| measurement | quantity | space |
|---|---|---|
| brightness ladder (dominance) | Rec.709 luma | **display-encoded** |
| contrast ratio (legibility) | `(L1+0.05)/(L2+0.05)` | **linear** |
| emission hue/chroma (palette) | CIELAB | **linear authored** |

**Every measurement states its space, not only its unit.** Fifth reporting axis after scope (C25),
coverage (C28), resolution (C32) and unit (C33). **The three ladders are never compared to each other.**

### T69 → G1 — truncation cannot produce good copy

The leg row named its backed team twice (`Atlanta Middlemen ML — Atlanta Middlemen v Tulsa Startups`)
and wrapped to three lines. Only Moneyline had that shape — verified in `MatchModel.DisplayLabel`;
every other market names no team in its own half. **The engine is untouched**: `DisplayLabel` is
shared with the console and the laptop, so it is read and re-authored on this surface (T42's shape).

The live statement's mid-word cut was found **mid-window** — `RICO LANYARD TO SCO` lives on the NEED
element, not the one first changed. Fixed to a word boundary, and **the result was the argument for
the real fix**: it read `RICO LANYARD TO`, ending on a dangling preposition.

> *Truncation can stop broken glyphs; it cannot produce a sentence.*

Escalated rather than absorbed, and **granted**: §5.1 says NEED is re-authored, T69 said truncate on a
word boundary, and those are not equal remedies — **truncation is the floor, re-authoring is the fix.**

### G1 — and the deck works because the list corrected its premise

G1 asked for "the authored statement string for every market". **There are two per leg**, from two
sources into two boxes — NEED (249px @ 28px, the requirement, live) and compact (**143px @ 15px**, the
identity, everywhere else). Authoring one would have left the other exactly as it was, and the one
that broke was NEED. Six `MarketKind`s produce **eight** NEED forms: BTTS is two different sentences,
not a parameter.

Built: `{CLUB} TO WIN`, `{SURNAME} TO SCORE`, `BOTH TEAMS SCORE`, `ONE TEAM SCORELESS`, totals
unchanged; compact `{CLUB} ML`, `{SURNAME} ANYTIME`, `BTTS YES/NO`, totals identical to NEED **and
that identity is correct, not a duplication to design away**. Compact is built from the *selection*,
not parsed out of `DisplayLabel`, and the fixture is dropped entirely — the scorebug carries who is
playing whom.

**Two of the old forms were CONSTANTS over budget with no variable in them**, so they had been
overflowing on every frame they ever drew. `KEEP` was also a §8 register problem: an instruction about
a thing the player cannot influence.

**MEASURED, because `FitToColumn` is the authority and not character counts:**

```
NEED col 249.0px   'ONE TEAM SCORELESS'   MISSES  -> ships ONE TEAM BLANKED
compact  143.0px   'UNDER 10.5 CORNERS'   FITS    -> the CNRS last resort unused
```

**One of the two at-budget forms missed.** The fallback mechanism takes it at runtime.

The DD's own pair-defect, found while authoring the top of it: NEED said `LANYARD TO SCORE` over a
progress line of `WAITING FOR LANYARD` — **T69's "a fact named twice" reproduced vertically.** The
scorer progress line is now `NOT YET` / `SCORED`.

### T68-am + T71 — the accepted half, and why it could not be built as first ruled

T68 said the ink takes `goldInk` on `actionable` **and** `accepted`. On `accepted` that produces
**1.08 : 1** for most of the beat, because the slot is hidden there and the figure renders over a
**sine-pulsing** flood. Filed with the numbers; the DD verified them figure-for-figure and recorded
the seat's error (*a value ruled for one context does not carry to another without checking what it
lands on*).

**The two inks are exactly complementary:** gold is 12.47 : 1 at flood alpha 0 and 1.71 : 1 at the
0.55 peak; `goldInk` is the reverse. **Neither static ink is right because the ground moves.**

Ruled route (2), built: both payoff figures move **into the slot**, §6.1's own spec, where the field
is stable and the inversion is already measured. Interpolating the ink (a hue lerp on the payout
figure — §7 is quantised) and inventing a new opaque ground were both refused. **The flood stays**
and stops being what money reads against. T71 takes `WinBeat` the same way — 1.83 : 1 at its 0.50
peak, the same defect one beat over — **and ruling them together is the point**, since two payoff
moments drifting apart in treatment is exactly what produced T68.

**C35** (transcribed from the DD's C34, renumbered on collision): *where an element and the surface
behind it are driven by one control, no brightness change can make the element readable.* Promoted
from this seat's §3 — `ApplyBoost`'s `Payout` case drove the text and its ground together, so their
ratio was preserved at any amplitude.

### THE LESSON WORTH CARRYING: a fixed scan window quietly stops covering its target

**Twice now.** A source-scan guard took a fixed character window from a method head — 500 chars in
batch 16, 4000 in batch 19 — and both times the method's own comments grew past it, so the scan
silently stopped reaching the code it existed to check. The batch-16 one threw
`ArgumentOutOfRangeException`; the batch-19 one **failed green-side**, asserting absence of a string
that was present just beyond the window.

> **Rule: search the whole source, or search to a real end marker. Never a character count.**

Same family, same wave, three instances of an instrument not covering what it claims:

- **`MaterialsAtL4` counted a hard-coded list of five that omitted `_cashOutFieldHdrMat`** — so from
  T63 until batch 19 the one-token instrument could not see the field that T68's blocker was about.
  A counter with a hard-coded list stops covering whatever is added next.
- **A scan that matched the very comment recording why a string was retired** (batch 16's T69 guard).
  Strip comment lines, or the documentation fails the test.
- The two fixed windows above.

### Two consequences caught in diff review, before they ran

1. **The gold flood silently lost its punch.** Moving the payoff figure from `HdrFocus.Payout` to
   `CashOut` left *nothing* requesting `Payout`, so `_goldFloodHdrMat` would have stopped boosting and
   the celebration ground would have rendered ~40% dimmer — against a ruling that says the flood is
   untouched. The flood rides the `CashOut` focus now. **Not C35's coupling:** that law is about an
   element and the ground *behind* it, and the figure's ground is now the slot's field.
2. **`_tBigAmount` is orphaned** — built, cleared on reset, never given content. **Not deleted:** its
   name is in the DD-gated `SanctionedL4Elements`, whose own gate says route before editing, and it is
   the element the accepted treatment would return to if revisited on frames. Flagged at its
   declaration **in the same commit that orphaned it** — the difference between a named consequence
   and what `_wonFlood` became.

### State

`04f7739` on `origin/tv-sweat`; PR #3 carries the wave. Working tree clean apart from the inert
`SBR.Engine.dll` line. **Nothing owed from this seat** — open with the DD: `_tBigAmount`'s inventory
call, and the blur bundle (not this lane's).

---

## 0-VW. VERIFY WINDOW — 2026-08-07/08. BATCHES 13+14 GREEN, T65 CLOSED ON FRAMES, editor released

**Compile CLEAN · engine 160/160 · EditMode 237/237 · PlayMode 64 passed / 1 documented flake / 5
`[Explicit]` skips.** Every count C29-guarded; both Unity runs reported executed == discovered.
Staged `tv-verify-window-2026-08-07.zip` (11.7 MB). The conformance era closes here.

### T65 — the room flood is GONE, proven on frames

| state | hue | sat | Rec.709 luma |
|---|---|---|---|
| pre-fix, leg win | **40.7°** | **71.1%** | **0.347** |
| post-fix, leg win (8 frames, 2 scenes) | **130.4°** | **40.4%** | **0.175** |
| post-fix, resting | 130.4° | 40.4% | 0.175 |

The room does not move on a leg win. **Mechanism was identified causally, not inferred:**
`WonLegBeat` fired `tvLight.Flash(gold, 3.0f)`, and `gold`'s hue computes to **39.6°** against a
measured room of 37.5–40.7°. Fixed by rule — one painting point, `RoomSettlementGlow()`, settlement
only, carrying a room-palette warm. No call site names a colour.

**Still owed: the new re-tint has never been photographed FIRING.** The capture harness has no
settlement moment in its named-moment list, so `roomSettlementWarm` (hue 88.0°) and intensity 0.9
remain an **upper bound**. The in-band amplitude window is roughly [0.78, 1.06] — ±15% — and the
cast runs monotonically from ~130° at zero to ~45.5° as amplitude rises, crossing 85–92° exactly
once. **That needs a harness addition, not an editor window.**

### T64 — struck on BOTH channels

`TvSweatScreen.idleEmissionFlicker` (9 Hz) and, by rule, `TvLight.flickerAmp/flickerHz` (11 Hz —
the channel that lights the *room*). **Removed, not zeroed**: deleting the field is what actually
kills the value serialized in `Room.unity`, which is the trap batch 13 recorded from the room lane
in the same breath. Side effect: this surface now calls `UnityEngine.Random` **nowhere**, so §6.4's
owed-to-integration item is discharged and the idle spill is identical run to run.

### Event strip → L2 (batch 14), landed

0.858 → **0.626** at hue 199°, now well clear of the scoreline's 0.873. Built as ONE painting point
(`SetEventStrip`) that applies the tier itself; **all 14 assignments** route through it, not the
ruled seven — the other four were resolution states also sitting at raw alpha, and leaving them loud
would have preserved exactly the split the ruling struck. Hue stays the caller's; the tier does not.

### T63 — structural half FIXED and proven; the value is NOT met and cannot be met from this seat

The HDR material sat on `_tCashOut`, the money **figure**, and never on `_cashOutField`. The field
could not be boosted at all, so granting the token moved a number and left the band at rest.
Splitting the zone: **field 0.696, figure 0.827** — the 0.827 the ruling measured was the figure,
and the field was the *dimmest* of the four competitors, not the third-brightest.

Fixed: the field carries its own HDR instance and `ApplyBoost`'s `CashOut` case drives both, the
shape `Payout` has always used. Band 0.827 → **0.844**; scoreline 0.873. **Still 0.029 short.**

**`goldL4` was tried and REVERTED — measured, not assumed.** A canvas vertex colour packs to
Color32, so (1.84, 1.31, 0.29) clamps to **hue 60° lemon**, and at 1.4 boost a full-width field that
bright **bloomed the whole panel**: band, event strip *and* risk/pays all reading 60.0° at ~61% sat.
Worse than the defect. **The general result:** in the ruled unit `gold` is 0.844 and cold white
0.942, and within the 0–1 range a canvas colour is clamped to, **no gold out-ranks cold white** —
reaching 0.942 needs G≈1.0, which is lemon. The brightness must come from the boost, and the boost
is sealed (T49-cl). **Every lever is sealed or above this seat.** Filed C25-form as
`dd-followup-tv-t63-lever.md`.

**New consequence, ruled either way:** with the field lit, bloom enters its neighbours' boxes — event
strip peak 0.626 → 0.833, risk/pays 0.430 → 0.840, both taking the field's hue. The elements are
**not repainted**, but it is what a viewer sees, and it is new: before the fix the field was
unboosted and sat under the bloom threshold.

### FOUR ERRORS, all caught inside the window — the useful part

1. **Read a STALE frame and concluded the flood survived.** Exit code 0, file present, numbers
   plausible — and a day old. The artifact-mtime check caught it. That run had resolved the leg
   **lost**, so no won-leg frame existed yet. *§4 step 3 earns its keep on the day it fires.*
2. **Concluded the fix had broken the field**, from `HOLD E` showing with no gold. Wrong: the status
   word is `_cashOutTweening ? "UPDATING" : "HOLD E"`, so `HOLD E` never implied an actionable
   field. The frame was consistent and T43's tests were green throughout. **A contradiction between
   two things I believed was resolved by reading the line that sets one of them.**
3. **On that false diagnosis I split the slot's shared material into two instances.** The sharing was
   **never proven harmful**; the change is kept because it matches `Payout`'s precedent, but it
   fixed nothing and is not claimed to have. *Recorded because a change that survives for a bad
   reason is how a codebase accumulates folklore.*
4. A source-scan test window of 500 chars was too short for my own added comment and threw
   `ArgumentOutOfRange`. Fixed in the test, not the code.

### Two corrections owed upward

- **The batch-13 ruling's four room regions are named "wall". They are the panel's own riveted
  HOUSING.** My boxes reproduce the ruling's numbers to the digit and rendering them shows rivets.
  **The conclusion is unaffected** — red gain falls off with distance across the right margin
  (+44.5 → +20.0 → +9.5) and the one surface facing away does not respond (+1.8), so it is a point
  light's profile and a room event. The seated capture pose is correct; it is the FOV that crops the
  room to near-field.
- **§0-W's claim about `SBR.Engine.dll` was wrong today.** It says the lingering status line is
  merely the inert-`[attr]lfs` artifact. Today `dotnet test` produced a **real rebuild**, and
  `git checkout` could **not** restore it — the LFS smudge filter cannot run while the macro is
  inert, and each attempt wrote different bytes. Restored binary-safely via `git cat-file`; the DLL
  is byte-identical to HEAD (blob sha matches) and loads as `SBR.Engine`, 74 types. **A .NET rebuild
  can never hash-match: the MVID is fresh every build.** Check the blob sha, never the status line.

### Two new instruments, both permanent

- **`tools/v6_room_region.py`** — gate V6. Room-region hue/sat/Rec.709 across an event burst. Boxes
  derived from the canvas→frame mapping, **validated by rendering them and looking**, and calibrated
  against the ruling's own numbers. Regions are named for what they sit on, not what one wishes they
  framed.
- **`tools/ladder_read.py`** — the ladder in C33's unit, zones from `LayoutGrid`'s own constants.
  Reports all three conventions side by side so the studio's existing numbers stay translatable.
  Reproduces all four of batch 13's T63 figures exactly.

Full re-read: `docs/tv-sweat-refinement/c33-ladder-reread.md`. **Its most reusable finding: the
ladder's `L4 1 / L3 0.7 / L2 0.4 / L1 0.15` are ALPHA COEFFICIENTS, not luminances** — `AtTier` does
`c.a *= tier`, so equal tiers land at unequal brightness whenever inks differ (`structureGrey` at L1
reads 0.123; `goldL2` at L2 reads 0.779). After C33 ruled the unit, "put this at L2" and "this reads
0.40" will look like the same instruction. They are not.

### State

**UNCOMMITTED.** Four source files (`TvSweatScreen.cs`, `TvLight.cs`, `TvSweatScreenPaletteTests.cs`,
`TvLightTests.cs`), the two new tools, the C33 write-up, and a one-line plan-doc fix
(`F_0.2.0_...plan.md:376`, G3's floor 5% → 4.5% per Allen 2026-08-08). Editor released: 0 processes,
lockfile clear, `SBR.Engine.dll` verified byte-identical to HEAD.

**`docs/ARCHI.md:267` still asserts the old 5–8% band as current law.** Integration-only per §1, so
it is recorded here rather than edited — it needs the same one-line fix from whoever owns it. The
`sim-report-*` / `PLAYTESTS.md` / `DECISIONS.md` instances are **historical records and must keep
5–8%**; rewriting them would falsify the record, not update it.

---

## 0. WINDOW RESULTS — 2026-08-03, editor released

**Correction to the brief this answers: no capture sets landed. Not the lighting-comparison pair,
not the bloom A/B. There are no paths or sizes to report, because there are no files.** Code and
suites are green; the evidence half of the window produced nothing. Both reasons are below and
neither is a retry-and-it-works.

### Merge — done, `e2143e6`

`main` → `slice/tv-sweat-refinement`. Was 51 ahead / 190 behind. Three conflicts:

| file | kind | resolution |
|---|---|---|
| `SBR/Resources.meta` | folder-GUID collision | took main's; **verified** nothing references the dropped GUID |
| `Runtime/Shaders.meta` | folder-GUID collision | same |
| `PRODUCT.md` | **add/add** (no merge base) | took main's |

`PRODUCT.md` is mine per §1 and I dropped my version deliberately: it was a 2026-07-24 TV-only draft
carrying facts now false (`Engine 144/144, Unity 40/40`; "Decision A, open as of 2026-07-24" — C1
closed it 07-31), and its own header says the source documents win. Main's is the studio-wide record
covering three surfaces. **Recoverable at tag `pre-main-merge-2026-08-03`** if that call was wrong.

### Compile — CLEAN

Zero `error CS`. Not inferred from exit code 0: `SBR.Game.dll` and `SBR.Tests.EditMode.dll` rebuilt
**22:08** against sources last edited **21:54** — newer, checked. T43/T46/T42 compile as written.

### Suites

| suite | result | vs baseline |
|---|---|---|
| engine | **160 / 160** | matches |
| EditMode | **222 / 222**, 0 failed, 0 skipped | see note |
| PlayMode | 70 total, **62 passed, 3 failed, 5 skipped** | 3 failures are not TV's |

**The 133 target was wrong and so was my 134.** Both were pre-merge arithmetic off a 129 baseline.
The merge brought main's SureThing EditMode suites in, so the real number is **222**. Nothing is
missing — all five new tests passed by name: `T46_right_hand_zone_content_is_owned_and_clipped_by_its_own_zone`,
`T42_the_only_team_hues_are_canons_two_muted_ones`, `T43_suspending_dims_the_whole_slot_on_the_same_frame_as_the_label`,
`T43_the_gold_taunt_cannot_repaint_a_suspended_slot`, `T43_a_tweening_price_never_lights_the_field_or_takes_the_L4_token`.

**No TV regression.** Every `TvSweatScreenTests` case passed, including
`Interact_DuringSuspendedMarket_StandsAndDoesNotCashOut` — the one T43's rewrite most threatened —
and the flake-prone `Standing_Freezes_CashOutTween_NoResumeCatchUp`. The regression string
`kept ticking while standing` occurs **0** times in the results XML. The documented mid-tween flake
did not fire this run.

**The 3 PlayMode failures are SureThing's `SureThingVisualCaptureTests`, and they are mine to
explain:** I ran `-nographics`, and they die in `UnityEngine.Camera:Render` /
`Rendering.Blitter.Initialize`. Environmental, not code — and it is the rig recipe's own warning
("Never `-nographics`. Post-processing needs a graphics device"). **SureThing's captures need a
graphics-enabled run.** Their untracked outputs are at `artifacts/surething-ui/` and
`unity/SBR/artifacts/`.

### T48 — STILL BLOCKED, and the merge did not fix it

Attempted, exit 1, `-outDir` empty. Log, verbatim:

> `executeMethod method 'CaptureConformance' in class 'SBR.RoomViewCapture' could not be found.`

**The rig recipe documents a harness that is not on `main`.** `CaptureConformance` and
`DarkenScreens` are on `room-refinement` (4 matches) and absent from `main` (0). The
`RoomViewCapture.cs` the merge delivered exposes only `CaptureEditMode()` and `CaptureAll()`.

So the merge did unblock the *other* two preconditions — `tools/room_gate_check.py` and `RoomPostFx`
in `Room.unity` are both present now — but not this one. **T48 needs the room lead's
`RoomViewCapture.cs` on main.** That is a room-side merge, not an editor window, and not mine to
take: `room-refinement` is another worktree's in-flight branch. The recipe's own §0 says the code
wins where the two disagree; this is that case, and the room lead asked to be told.

### T49 — not attempted, deliberately

`HdrBoostL4` is `private const float = 1.8f`. It is not runtime-settable, so the A/B is: edit the
const, warm compile, shoot a ship-paced `[Explicit]` set, edit back, compile, shoot again. Two source
edits and two recompiles around two capture sets. That does not fit a window with SureThing queued
behind it, and starting a set I could not finish is the precise failure §4 rule 4 was chartered
against — a half-set is not evidence (C11/C17), it is a wasted arm.

### Editor released

0 Unity processes. The stale `UnityLockfile` — the documented segfault-on-`-quit` fault, safe to
clear at 0 processes — was cleared. `EditorBuildSettings.asset` reverted;
`SBR.Engine.dll` restored to HEAD's bytes and **`cmp`-verified identical**, so its lingering
`git status` line is the inert-`[attr]lfs` artifact (§4D), not a real change.

## 0-T63i. T63 ISOLATION — the invert-before-label defect is NOT real (no editor used)

**Answer: the cash-out FIELD never coexists with a `MARKET SUSPENDED` label.** Isolated code-side
first, as instructed; frames were needed only to confirm what the gold actually was. Bundle restaged
with the correction.

**Three independent checks, any one sufficient:**

1. **Code.** T43's fix sets the flag, the label and the field in **one call** — `live = slotVisible
   && !_cashOutSlotSuspended`, so `fieldLit` is false whenever the label reads SUSPENDED. Pinned by
   `T43_suspending_dims_the_whole_slot_on_the_same_frame_as_the_label`, green in 228/228.
2. **Geometry.** The suspect gold spans canvas y **428–555**. `_cashOutField` is sized exactly to
   `grid.CashOut` (**480–532**) and cannot paint outside its own rect.
3. **Luminance.** Gold pixels inside the zone, by floor:

| frame | floor 0.10 | 0.25 | **0.40** | |
|---|---|---|---|---|
| 000 (real invert) | 64,131 | 60,950 | **60,950** | the field — mean 0.569, peak 0.663 |
| 006 (suspect) | 24,112 | 5,093 | **0** | vanishes entirely |

The real field peaks 0.66 and clears 0.40 trivially. Frame 006 has **zero** gold above it. What it
has is the won-leg row's L3 gold, the goldL2 RISK/PAYS footer, and the column's warm-tinted dark
ground — all sub-0.40, none of it the field.

**Instrument error, disclosed (C25).** My first reading used a saturation test with a luminance floor
of **0.10**. A near-black warm pixel like `(0.12, 0.09, 0.05)` scores saturation 0.58 at hue 34° and
registers as "gold". **This is the second time in this slice a low-luminance hue test produced a false
positive** — and the instructive part is that in T42's case raising the floor did *not* explain the
residual (§0-VP2 records that theory being tested and failing), while here it explains it completely.
Same trap, opposite outcome. It has to be tested each time and assumed in neither direction.

**What this does not touch:** the §0-W finding stands unchanged — fully inverted, the band peaks
0.660–0.663 against a same-frame scoreline of 0.875–0.877, so the designated L4 element is still
~0.21 off being the brightest thing on its surface.

---

## 0-W. WINDOW — T61+T62 verified, EditMode 228/228, T63 MEASURED, editor released

**EditMode 228 / 228, zero failed** (C29 guard clean: 228 executed, 228 discovered). T61's two and
T62's two all pass. Staged `t63-cashout-band-invert.zip` (8.2 MB).

### T63 — the finding, and it is not closed by T41 or T58

Cash-out band, its own region, across the invert burst:

| state | band mean | band peak | hue | sat | **scoreline peak, same frame** |
|---|---|---|---|---|---|
| **fully inverted** (frames 000/001) | 0.569 | **0.660–0.663** | 47–58° | 65–75% | **0.875–0.877** |
| partial invert (006) | 0.143 | 0.271 | 40.8° | 73.5% | 0.890 |
| suspended slate (002–005/007, sat-down) | 0.104 | 0.166–0.169 | 205–216° | 11–15% | 0.875–0.890 |

**The designated L4 element is still not the brightest thing on its own surface — by ~0.21.** T41
capped the stage; T58 took the hue off the flash; **this gap is neither of those and survives both.**

**Scope (C25) — read the ORDERING, not the absolute scale.** My quiet-scoreline peak reads **0.875**
where the DD's earlier figure for the same thing was **0.737**. There is a systematic offset between
the two instruments (different box derivations) which I can name but not resolve from here, so
absolute values must **not** be cross-compared between the two measurements. The within-frame
comparison is unaffected — both numbers come from one frame, one method, one run.

**The box derivation ships with the numbers**, because the previous attempt's boxes framed the wall:
canvas→frame scale solved on both axes (2.2204 / 2.2236 — agreeing to three decimals is the check
that the panel was framed, not the room), CashOut zone taken from `LayoutGrid` rather than eyeballed,
and **validated by rendering the box and looking at it** (`t63-box-validation.png` is in the bundle).

**What the burst actually shows:** the market suspends and reopens *inside* the 1.05 s burst — full
invert at 000/001, slate through 002–005/007, partial invert at 006. The band is not in one state
"across the invert"; **the trajectory is the measurement.**

### A latent flake found and removed — `TvLightTests`

The first run came back 227/228. `Flash_and_SetRest_still_drive_the_wired_light` failed with
intensity 0.844 against `> 1.0`. **Not a regression** — nothing in this session touches TV lighting,
and it fails in isolation too. `Update()` decays the flash *before* reading it
(`_flash01 = MoveTowards(_flash01, 0, flashDecay * Time.deltaTime)`), so with `flashDecay = 2.6` the
assertion needs **`Time.deltaTime < 0.308 s`** — an input no EditMode batch run controls. The measured
0.844 back-solves to `_flash01 = 0.137`, `dt = 0.332 s`.

It passed for months on fast frames and failed the first time four unrelated tests pushed the frame
past ~308 ms. Fixed by pinning `flashDecay = 0` for the assertion: **the assertion is unchanged, only
the uncontrolled input is removed.** Flash still has to reach the wired Light *through* `Update()` to
pass. Decay-rate coverage is not lost, because this test never asserted it — it calls `Update()` once
and checks a lower bound.

---

## 0-B12. BATCH 12 — T58 CLOSED · T62 fixed (UNCOMPILED) · T63 owed an editor slot

**T58: GRANTED, Design-verified, CLOSED** — measured by the DD personally, perfectly neutral flash,
gold back to money. The owning doc (C26) is the next DD session.

### T62 — FIXED. One ledger, two mirrors, one repaint

The DD found it **on this slice's own T58 proof frames**: the live leg's progress line printed the
pre-goal score for a whole beat while the scoreline above printed the goal — same revealed value,
same frame, correcting 51 match-minutes later.

**Mechanism.** `OnGoalPlayed` advances the revealed score at `_ledger.CompleteGoal(goal)`, then
repainted the **scorebug only**. The live leg row reads *the same* `_ledger.Picked/Opponent` (via
`DescribeActiveLeg`) but was repainted only by `UpdateTicketColumn`, which next runs at the following
beat's `RenderEvent`. Not two sources — **one source, two repaint schedules.**

**Fixed at the ledger-advance site**, not by adding another call to another path:
`RepaintRevealedScore(leg)` does scorebug + column together, and that is what `OnGoalPlayed` calls.
Same rule as T43's slate and T59's gate — one value, one repaint, no window where mirrors disagree.

**I had this frame and did not call it.** §0-VP2 records the column reading `LEADING 1–0` against a
scoreline of `0 — 0` and I treated it as two matchups rather than one contradiction. The verdict pass
looked at *type* and *hue* and never asked whether the two score readings agreed. Worth carrying: a
pass scoped to one property will not see a defect in another, however plainly it is in frame.

**Full audit of every ledger-advance site**, since fix-by-rule means checking the siblings:

| site | status |
|---|---|
| `_ledger.CompleteGoal` (goal) | **the defect — fixed** |
| `ResetForLeg`/`ConfigureEndpoint` (kickoff) | **reasoned benign** — the column's live row is still the previous, resolved leg, so there is nothing to contradict. Changing it would alter *when* a row reads LIVE, which is a design call and not taken unruled |
| `PlanFinal` (T17's reserve release) | **covered by construction** — it mutates no score, it emits a plan; those staged goals advance the ledger through `CompleteGoal` and so through the fix |

Two EditMode guards: `T62_advancing_the_ledger_repaints_every_mirror_of_it` (scans every
`CompleteGoal` site) and `T62_the_repaint_helper_drives_both_mirrors` (the helper cannot quietly lose
its column call). **Both are SOURCE scans and that is weaker than it looks** — they pin the call, not
the pixels. The rendered pin needs a PlayMode goal and is owed with T63.

### T63 — owed, needs the editor slot (after SureThing and room)

Measure the **cash-out band's own region** across the invert burst. The DD's boxes hit the wall behind
the panel — the same framing failure this slice hit twice in §0-VP and once more in §0-VP2, now from
the other seat. **Deliver the box derivation with the numbers, not just the numbers**, and validate
the framing by rendering it before trusting it; that is the only method that has actually worked here.

### Status

**UNCOMPILED — no editor grant.** Pending compile: T61's contract fold-in *and* T62. Expect EditMode
**228** (224 + T61's 2 + T62's 2).

---

## 0-T61. T61 FOLDED IN — contract taken from markets' test, not from a green re-run (UNCOMPILED)

Markets diagnosed T61 and the answer is sharper than the hypothesis I filed. **My harness is fixed
against their contract, and a contract test now sits in my own suite** — deliberately instead of
re-running and taking green as proof.

### Why a green re-run would have proved nothing

Markets tested my round-advance hypothesis and found it **half right — the wrong half being the
useful one**:

| ticket outcome | what ticket 0 does mid-sweat | what a ticket-keyed poller sees |
|---|---|---|
| **dies** (dead-leg Lost) | settles immediately | early **false "done"** |
| **survives** | stays `Open` until `FinishSweat` | **no signal at all** |

So whether ticket 0 is terminal mid-sweat depends on the **OUTCOME, not the position** — which is
exactly why this harness failed four seeds and passed one **from identical code**. A seed that happens
to lose would have made a re-run look fixed while the defect sat untouched. **The defect is
seed-decided, so only a contract can settle it.** That is the whole reason this section exists.

Confirmed separately: `Run.Tickets` is the round's working set and is **cleared at ExitShop**, so a
held `Tickets[0]` becomes permanently terminal while `Tickets[0]` names a different, open ticket.

### The contract, and where it now lives

> **Completion is a property of the RUN's phase, never of any one ticket or session. Phase leaves
> `Sweat` exactly once, after every session is drained.**

- `TvSweatCaptureHarness.SweatEnded(director)` → `director.Run.Phase != Phase.Sweat`. Replaces
  `CurrentSession.IsComplete`, which stopped **too early** (sessions drain while the phase is still
  `Sweat`).
- Two EditMode tests pin it TV-side: `T61_sweat_completion_is_a_phase_property_not_a_ticket_property`
  and `T61_a_captured_ticket_reference_goes_stale_across_a_round`. Engine-side proof is markets'
  `SweatPollingContractTests`.
- The drain helper **asserts it actually drained** — an unfinished drain would make both tests pass
  while proving nothing about the state they claim to reach. Same failure mode C29 names, one level in.

### The grace window matters MORE now, not less

A surviving ticket settles at `FinishSweat` — i.e. at the very edge of the phase change. So the
legitimate resolution and the stop signal now arrive **within a frame of each other**. Without the
existing 2 s settle window the harness would miss exactly the moment it exists to photograph, on
precisely the seeds that win. Kept and re-commented, not removed.

### Status

**UNCOMPILED — no editor grant held.** Needs a warm compile and EditMode (expect **226**: 224 + 2).
The capture harness itself needs no re-run to prove this: the contract is pinned by tests, which is
the point.

---

## 0-B10. BATCH 10 — C29 guard retrofitted, and T58's evidence needs NO window

### T58 is already demonstrated on frames in hand

**The next editor window is not required for T58.** Staged
`main-2/docs/design/dd-import/t58-goldflash-fix.zip` (10.9 MB — quiet reference, the flash frame, two
more goal frames, and `MEASUREMENTS.txt`).

Same statistic the DD used — scoreline peak pixel, canvas x285–960 y8–56:

| | hue | saturation | luminance |
|---|---|---|---|
| DD pre-fix, **gold flash** | **56–58°** | **48.7–66.8%** | 0.72 |
| DD pre-fix, quiet | 205° | 5.2% | 0.737 |
| **post-fix, all 8 goal frames** | **198–207°** | **4.4–5.2%** | 0.875–0.877 |

The goal moment is now **indistinguishable from the quiet reference**. The gold is gone.

`frame000`, first of the goal burst, reads **`#e8e8e8` at 0.0% saturation, luminance 0.910** against a
0.875 rest: perfectly neutral and the brightest frame in the set. That is the punch carried entirely
by brightness with no hue — T58's instruction, executed.

**Scope (C25):** a PNG cannot confirm the `_tScoreFlash` overlay was enabled on that frame. It is
identified as the flash by position in the burst, by being brightest, and by being exactly neutral.
**One burst = one sample of the flash instant.** If the DD wants n>1 the window is still worth taking;
if the measurement suffices, it is not.

### C29 — zero-case guard retrofitted

`tools/assert_test_run.py`. **Run it on every results XML before any verdict, gate or Design-verified
claim rests on the run.**

```
python tools/assert_test_run.py <results.xml> [--min N] [--expect-passed N]
```

Verified against real artifacts, all four branches:

| case | source | result |
|---|---|---|
| the run that caused C29 | `hz.xml`, 0 cases | **exit 1** |
| a good run | `em3.xml`, 224/224 | exit 0 |
| missing report | absent path | **exit 2** |
| expectation unmet | `--expect-passed 999` | **exit 1** |

`total` (executed) is what decides, not `testcasecount` (discovered) — they differ exactly when a
filter matches names that never run, which is the case C29 exists for. Both are printed so the gap is
never invisible. A **missing** report fails too: a run whose report never appeared demonstrated
nothing, and treating that as neutral is the same mistake one level up.

The failure message names the cause that actually bit here — a parameterised `-testFilter` needs its
quotes, or use the regex form `".*<seed>.*"`.

### Also from batch 10

- **T60 struck** — no body, so never a ruling (C22). Nothing was built against it.
- **T61** (my scorer finding, ruled): diagnosis routes to markets, **round-advance hypothesis first** —
  the harness-scope explanation I named first and asked to have excluded. The design answer is
  pre-committed for every outcome. **Blocks nothing here.**

---

## 0-VP2. VERDICT PASSES — all four PASS, on the post-T58/T59 frames (no editor used)

Run against the 17-frame set from the post-batch-9 build (seed `48151623`, `boost1.4`), which is the
current shipped state. **All four pass.** Two new items surfaced and are below as candidate build
items — neither is a failure of the four.

| pass | verdict | basis |
|---|---|---|
| T46 right-zone clipping | **PASS** | boundary read at magnification + EditMode structural test |
| T42 team hues | **PASS** | measured, 8 goal frames |
| T44 event-strip copy | **PASS** | strip read at magnification |
| T50 column type in situ | **PASS** | column read at magnification |

**T46.** The pitch's left edge sits at canvas ~268 and every stage element stays right of it — penalty
area, dots, goal, momentum tape. Leg text is unobscured. **NetRipple, which used to reach ~155px past
the stage edge, is contained.** The gold cash-out field stops at the column edge (a few px of bloom
halo, not geometry). *Limit:* no worst-case long-fixture frame exists in this set — the seeds did not
produce one. That case is covered by the EditMode canary, not by these frames.

**T42.** Pitch interior, 8 goal frames: **blue mean saturation 0.483** against canon `--tv-team-a`
0.452, **pink 0.353** against `--tv-team-b` **0.354 — exact**. 97.8% of saturated pitch pixels fall in
the two canon bands, and **no dot is rendered in a retired hue.**

**T44.** `Zambonis settle in; the drift runs the other way.` — the T39-corrected line. No second
person, no hype, no superlative, no promise; one line, cold white per TV-05's neutral strip. Other
frames in the set show `THE BOARD IS SET` (this slice's casing fix) and
`Meatballs rip through on the break. — LEAD CHANGE`.

**T50.** Encode Sans and Encode Sans Condensed render in situ with the hierarchy intact — header,
then identity, then price/state. `−228` uses U+2212 MINUS and `+380` its sign, per S30. RISK/PAYS
gold at the footer. No team hue anywhere in the column. T11 stands on rendered evidence.

### The instrument was wrong THREE times — and the pattern is the point

T42's residual 2.17% took three attributions, two of which were wrong:

1. **"Edge fringe of my crop box."** Partly true for the loose box, false for the tight one.
2. **"Dark pixels where hue is meaningless."** Testable and **tested: false.** Raising the luminance
   floor from 0.06 → 0.25 moved the residual 2.18% → 2.15%. The theory predicted it would collapse.
3. **Marking the pixels and looking at them.** Correct: violet is antialiasing on pink dot rims
   (violet 250–290° lies *between* blue 215° and pink 319°, so a blend lands there by construction),
   and orange is a thin **pitch marking line** at the right penalty area.

Every time, the thing that settled it was rendering the question as an image. **Three numeric
theories, one look.** C11 says rendered evidence or no claim; this is that law aimed at the
instrument rather than the design, and it is now the third distinct framing/threshold error this
slice has caught in its own measurements (see §0-VP for the first two).

### Two candidate build items, surfaced by the passes — DD calls, not taken

**(a) Two dashes for one fact, both visible in a single frame.** The ticket column prints
`LEADING 1–0` using **U+2013 EN DASH** (`SweatActiveLegModel.cs:160`, `const char Dash = '–'`), while
the scorebug prints `0 — 0` using **U+2014 EM DASH** (`TvSweatScreen.cs:1448` and `:1817`). Same fact,
same surface, same frame. Defensible on typographic role — an en dash is the range dash — but S30
ruled signed numbers with "no per-region exception", and TV-32 called the em dash "the system's own
dash". **Not fixed unilaterally: which dash a score takes is a typography ruling.**

**(b) A pending leg's market label wraps to four lines.** `TROY MUFFIN ANYTIME — Tuscaloosa
Spreadsheets v Tulsa Muskrats` sets as four wrapped lines. It fits inside its fixed 69.3px slot, so
§6 is not breached and nothing reflows — but T20 rules that "live rows are display, resolved rows are
index", and a four-line wrapped label reads as display. **Flagged for the DD's eye, not called a
violation.**

---

## 0-VP. VERDICT-PASS PREP — T42 measured on frames; T46/T44 read on frames (no editor used)

Done while markets held the lease. **T42, T46 and T44 are all already implemented and green; what
they lack is Design-verified, which is the DD's act against rendered frames (C11).** So this is the
evidence, measured off the 101 frames already on disk — and the measurement itself needed correcting
twice, which is the most useful thing in this section.

### T42 — the dots measure as canon

Pitch interior, saturated pixels (sat > 0.30), five goal frames, one per seed:

| band | count (5 seeds) | mean saturation | canon |
|---|---|---|---|
| blue 190–240° | 93,648 | 0.479–0.501 | `--tv-team-a` **0.452** @ 215.5° |
| pink 300–340° | 63,497 | **0.348–0.354** | `--tv-team-b` **0.354** @ 319.0° |
| retired orange 15–40° | 2,975 | — | not in the TV palette at all |
| retired violet 250–290° | 824 | — | not in the TV palette at all |

**Pink lands on canon almost exactly** (0.348–0.354 against 0.354). Blue reads ~0.03–0.05 high, which
is what a bloom-and-grade lift does to the more luminous of the two. Blue and pink together are ~97%
of the pitch's saturated pixels, and **no dot is rendered in a retired hue** — checked by marking the
orange pixels red and looking at them.

**The residual is not dots.** Two-thirds of the orange disappeared when the box was inset 3%,
identifying it as fringe along the box's own edge; what remains is sub-pixel antialiasing between the
dots, the white ring and the near-black pitch, at counts orders below anything dot-shaped.

### The instrument was wrong twice before it was right — the useful part

1. **First box framed the room, not the screen.** Derived from an inter-moment diff, on the assumption
   that only the TV changes between moments. **The TV's own light spill changes the whole room**, so
   the box swallowed the walls and reported a median hue of **128° — green** — on a surface whose
   palette contains no green. Caught because green was impossible, not because the number looked odd.
2. **Second box overran the pitch into the panel surround**, inflating the retired-orange count
   threefold with edge fringe.

Both are the exact failure `rig-r23-recipe.md` §6 names: *"a box that no longer frames the intended
surface would still report a plausible-looking number."* The fix that actually worked was **looking at
the rendered frame** rather than reasoning about coordinates — C11's own point, turned on the
instrument instead of the design. **Any region box quoted from this slice should state how it was
framed and how that was checked.**

### T46 and T44 — read on the frame, not measured

On `seed-16180339 … moment-goal`: the stage's left edge is a **hard vertical line at the ticket
column boundary**, leg text (`BRICKLAYERS TO WIN`, the two leg rows, `RISK $87  PAYS $705`) is
unobscured, and the NetRipple — the element that used to reach ~155px past the stage — is fully
contained. **T46 reads as fixed.** The event strip prints
`Meatballs rip through on the break. — LEAD CHANGE`: observational voice, em dash, no hype. **T44
reads as fixed.** Both stated as *read*, not measured — no pixel criterion was applied.

### And the frames confirm T58 the DD found

The same frame shows the scoreline `MEATBALLS 1—0 BRICKLAYERS` in **gold**. These frames predate the
T58 fix, so that is exactly the defect batch 9 ruled — visible, and consistent with the fix now
written but not yet re-shot. **The next capture is what closes it.**

---

## 0-HD. HARNESS DEBT — both items written, UNCOMPILED (2026-08-04, markets held the editor)

Both of the DD's C25 disclosures are addressed. **No editor was available, so none of this is
compiled.** It needs a warm compile plus one filtered `TvSweatCaptureHarness` run to prove out.

### 1. Frame-locked A/B arms

The T49 pair could not answer the question it was shot for: its arms did not share sim state, so the
whole-frame diff measured **actors that had moved**, not bloom, and the DD fell back to fixed-box
region statistics. Three things had to be pinned, **all presentation-local** — the engine was always
deterministic from the run seed, which is exactly why the arms' *events* matched while their *pixels*
did not:

| source | was | now |
|---|---|---|
| `TheaterStage` presentation RNG | `Environment.TickCount * 31 + salt` | `PresentationSeedOverride` when set |
| idle emission flicker phase | `UnityEngine.Random.value` | derived from the same override |
| `Time.deltaTime` | real frame time | `Time.captureDeltaTime = 1/50` |

**The third is the one that is easy to miss.** Pinning the RNG makes both arms take the same
*decisions*; it does not make them integrate the same *motion*, because a real frame time varies run
to run. Without the fixed step the actors still drift apart and the per-pixel diff stays invalid.

Two traps closed while writing it, both of which would have produced a frame-lock that silently did
nothing while looking correct:

- **`StableSeed` is FNV-1a, not `string.GetHashCode`.** .NET randomises string hashing per process,
  so two arms shot in separate editor runs would have seeded differently.
- **The override path does not consume `s_seedSalt`.** The salt is a static counter, so mixing it in
  would make the seed depend on how many stages the session had already built — i.e. on seed *order*.
  Two arms that reordered or skipped a seed would quietly stop being locked.

`[TearDown]` releases both. `Time.captureDeltaTime` is global and session-lived; leaving it set would
put every later PlayMode test on a synthetic clock, which is the kind of cross-suite contamination
that is very hard to attribute once it bites.

**Named cost, because it works against item 2:** wall-clock per simulated second now depends on
render speed, and these are 2560×1440 frames. Ship pacing is preserved in the sense that matters —
same simulated seconds, same per-frame step — but the 420 s wall budget may cover fewer of them.

### 2. The 420 s shared budget

**Proven, not assumed:** in the T49 run the four failing seeds captured **zero** dangerous beats — all
24 `scorer-leg-dangerous-*` frames belonged to the single passing seed. So in every failing seed that
loop ran from entry to the wall doing nothing, and the named moment after it began with its deadline
already gone.

The loop is **opportunistic** (gather what the sweat offers); the scorer wait is a **named moment** the
set is expected to contain. An opportunistic collector must never starve a named one. So the budget is
now *partitioned*, not enlarged — `ScorerWaitFloorSeconds = 150f` is reserved and the total stays
420 s, because raising it past the NUnit `[Timeout]` would replace the harness's own diagnostic
message with an opaque framework kill.

The loop also now logs **why** it exited — `cap reached` / `leg resolved` / `budget` — with the time
left for the wait. C18: a scorer-leg failure after this line is now about the sweat, not about this
loop having eaten the clock.

**What this does not claim:** it does not prove the four seeds will now resolve. Their legs were
genuinely still LIVE. It makes the failure *trustworthy* — if it still fires with a reserved floor,
that is a real finding about the sweat and worth escalating rather than a budget artefact.

---

## 0-B9V. BATCH 9 VERIFIED — EditMode 224/224, input contract re-pinned, editor released

**T58 / T59 / T49-lock compile clean and pass.** EditMode **224 / 224**, zero failed, zero skipped
(222 → 224 is the two new tests). Assemblies verified fresh, 23:01 against sources at 22:55.

**T49's ruling broke a TEST INSTRUMENT, not the code — and it is worth keeping the reason.** The
first EditMode run came back 222/224 with both C3 one-token tests failing `Expected: 1, But was: 0`.
`MaterialsAtL4` counted materials with `_HdrBoost > 1.5f` — a threshold hand-calibrated to the old
1.8 — so a correct L4 element at the newly-ruled **1.4** was invisible to it and the helper reported
*zero elements lit* on a surface that was behaving perfectly.

That is **T30's lesson landing a second time**: an approximated threshold is always wrong at some
boundary, and a ruling eventually walks the value past it. The fix is not a looser number — that
would just relocate the boundary. `ConstBoost()` now reads `HdrBoostL4` off the production constant
by reflection and matches it verbatim (epsilon is float representation only), so the instrument
cannot go stale whatever the DD rules next. Swept the suites for other boost thresholds: none.

**T59 was verified where EditMode cannot see it.** An input-contract change is invisible to a test
that never presses a key, so a filtered PlayMode run on `TvSweatScreenTests` followed: **10/10**,
with all three `Interact_*` contract tests green — including
`Interact_DuringLegalOpenOffer_CashesOutAndDoesNotStand`, the one the new refusal term most
threatened. `kept ticking while standing` occurs **0** times, so TVS-H02 holds.

Editor released: 0 processes, lockfile clear, side-effects reverted.

---

## 0-B9. BATCH 9 — T58 + T59 + T49's ruling WRITTEN, unverified (2026-08-04)

**Ladder is LAW-CLEAN and Phase 3 is open.** T41 Design-verified CLOSED (zero saturated pixels),
T48 CLOSED, **T49 RULED 1.4 and SEALED**. Written this session, **no editor grant held — a work
assignment is not a lease (§4 step 0a), so none of this is compiled.**

| item | change | file |
|---|---|---|
| **T58** | goal-flash overlay gold → **cold white** (`flavorColor`, same as `_tMatchup`) | `BuildScoreBug` |
| **T59** | `CanAcceptCashOutNow` now reads `_cashOutSlotSuspended` — one value drives slot AND key | `CanAcceptCashOutNow` |
| **T49** | `HdrBoostL4` 1.8 → **1.4**, sealed | `TvSweatScreen.cs:577` |

Two new EditMode tests: `T58_the_goal_flash_carries_no_hue_of_its_own`,
`T59_a_suspended_slot_refuses_the_key`.

**T58 — the fix is structural, not a colour swap.** The punch overlay now carries the *same* colour
as the scoreline it superimposes, so boosting it can only brighten what is already there and
releasing settles back. There is no hue to change **by construction** — a future edit cannot
reintroduce one without making the two elements visibly disagree at rest, which the test pins.

**Where the gold was NOT:** `_ballFlash` is already cold white and fires only when the goal does
*not* commit, so it is mutually exclusive with the score punch; the stage's own ball is `Color.white`
(cyan only on VOID) and T41's closure holds. **`_tScoreFlash` was the only gold at the goal moment.**
The ruling's "and the ball dot goes with it" is therefore either the gold scoreline's bloom halo or
the ball read in the same region — worth confirming on the next frames, but there is no second gold
source in the code to fix.

**T59 — this is the question T43 routed up, answered.** The presentation flag now gates the accept.
`suspended`/`pending` refuse E, `actionable` accepts, `updating` refuses via the existing
`_cashOutAnimation` term. TVS-H01 survives by construction because `CashOutLive` and `TryCashOut`
both read this one predicate — pinned by the new test. **The "refused press draws nothing" clause
already held**: `TryCashOut` opens with `if (!CanAcceptCashOutNow()) return;` and has no refusal
branch, so nothing flashes and nothing explains. Nothing added.

**T49 — sealed means sealed.** The constant carries the ruling and the reason inline, including the
finding worth more than the pick: a ±0.4 bloom change moves nothing on this surface except one
element that was the wrong colour. **Bloom is not the lever for any future finding.**

### Harness debt, both from the DD's own C25 disclosure — NOT started

1. **Frame-locked A/B arms.** The two arms do not share sim state: actor positions differ at the same
   seed/scene/grammar/frame index, so the whole-frame per-pixel diff (2.5–3.3% of pixels, mean 44–59)
   **cannot be attributed to bloom** — most of it is actors that moved. Region statistics on fixed
   boxes were the only valid instrument on the pair I shipped. *An A/B whose arms are not frame-locked
   cannot support a per-pixel comparison,* and the bigger, more impressive number was the invalid one.
2. **The 420 s shared budget** (diagnosed in §0-FULL): the dangerous-beats loop needs its own
   sub-budget, or the scorer wait needs a reserved floor.

### Next, per batch 9's order

T58 → T59 → **T46** → T42 → T44 → T50 (T50's column items are blocked by T46). T46/T42/T44 are
already *implemented* here; what batch 9 queues is their verdict pass on frames that now show them.

---

## 0-FULL. FULL WINDOW — 2026-08-04. T48 SHOT, T49 SHOT, editor released

**Everything the window was granted for landed.** Four zips staged in `main-2/docs/design/dd-import`,
all under 20 MB. Boost const verified back at **1.8**, `git diff` on that file empty.

| zip | contents | size |
|---|---|---|
| `t48-conformance-screens-dark.zip` | 4 frames (graded + `-UNGRADED`, seated + room) **+ the measured gate report** | 10.6 MB |
| `t49-bloom-ab-goal.zip` | 3 matched pairs at the score's L4 punch | 16.8 MB |
| `t49-bloom-ab-cashout.zip` | 3 matched pairs at the cash-out L4 token | 16.3 MB |
| `surething-captures-graphics-enabled.zip` | 26 frames, all 3 tests green | 11.0 MB |

### T48 — the grade is EXONERATED, and the numbers reproduce the room's own reference

Screens-dark, graded | ungraded:

| region | graded | ungraded |
|---|---|---|
| wall (right plaster) | chroma 0.92, hue 112.0° neutral | 1.55, 112.1° neutral |
| **wall (far plaster)** | **3.56, 275.5° COOL** | **5.53, 275.7° COOL** |
| **floor (aisle)** | **1.64, 272.1° COOL** | **2.94, 272.8° COOL** |
| bunk (1 / couch side) | 0.34, 203.7° neutral | 0.58, 182.4° neutral |
| bunk (2 mattress) | 6.21, 99.4° WARM | 7.73, 100.1° WARM |
| ceiling plaster | 0.29, 118.1° neutral | 0.48, 117.1° neutral |

R23 verdict: **FAIL — 2 COOL regions.** But read the pair, which is the whole point of the set:
**both COOL regions are cool UNGRADED TOO, and more chromatic without the grade** (5.53→3.56,
2.94→1.64). The grade *reduces* chroma. **The coolness is in the light, not the grade** — R26's
"ungraded-cool exonerates it" branch, on evidence. The numbers sit within noise of the recipe's
known-good reference (far plaster 3.56/275.6 graded, 5.49/275.8 ungraded), so the rig reproduced.

**Room-side finding, not mine to fix, flagged: `R9-A bunk 2 mattress luminance FAIL — 37.36 against
43.9 ± 1.0.`** R19(c) was explicitly told to hold 43.9. R19(a) has since landed albedo work. The room
lead should know a freshly-shot set reads 6.5 low.

### T49 — shot both arms, 101 frames each, pairing EXACT

Arm A at the shipped 1.8 **first and unedited**, per §0A — so a dead window would have left the
shipped value as the surviving arm. Then the const edit to 1.4, warm compile, arm B on the same seeds
in the same order. **Every 1.8 frame has a 1.4 twin at identical seed/scene/grammar/moment/frame**,
verified by comparing the two filename sets with the boost token masked. Const restored and verified.

Staged the two L4 moments — `goal` and `cashout-actionable` — because those are the *only* frames
where the arms can differ: §0A's arithmetic says the stage is under the bloom threshold and L3 gold is
identical in both arms, so the single L4 token holder is the entire experiment. 555 MB of frames
exist; the 20 MB cap buys ~7, so they were spent where the difference lives rather than sampled flat.

### The harness fails 4 of 5 seeds, and it is NOT this window's doing

`Assert.Fail("the scorer leg never reached a terminal state — deadline reached with the session still
LIVE, which is a genuine hang")` on 4 seeds, both arms. **Frames are unaffected — 101 landed per arm
across all five seeds.** Mechanism, read off the harness:

- Line 273 sets **one shared 420 s budget** for every wait in a seed.
- Line 314's dangerous-beats loop runs `while (realtime < deadline && captured < MaxDangerousBeats)`
  with `MaxDangerousBeats = 3` — so a seed that never produces three dangerous beats **spins out the
  entire shared budget**.
- The scorer-leg wait at line 350 then starts with the deadline already gone, and
  `WaitUntilOrAbsent`'s absent-branch only fires when the session has COMPLETED. Still LIVE → fail.
- The one passing seed (`27182818`) hit dangerous-0/1/2 and exited the loop early, leaving budget.

Not caused by this window: the merge changed **nothing** in `engine/` or `SweatPresentationModel.cs`
(empty diffstat), and T43/T46/T42/T44 touch presentation only — no ledger, no session, no beat
spending. **This is a defect in my own file** (`TvSweatCaptureHarness.cs`) and a follow-up: the
dangerous-beats loop needs its own sub-budget, or the scorer wait needs a reserved floor. Filed here
rather than fixed mid-window.

### SureThing — 3/3 green with graphics enabled

Confirms last window's diagnosis: `-nographics` was the whole cause. 26 frames written.

---

## 0A. NEXT WINDOW — pre-flight, done outside the window on purpose

### T49 is no longer confounded, and here is the arithmetic that says so

Read off the merged `RoomVolume.asset` and this file's own constants. **Predicted from constants, not
measured on a frame** (C25 — that is this instrument's scope; it says the experiment will separate,
not what the frames look like).

Bloom: `threshold 0.9`, `intensity 0.7`, `scatter 0.7`, active.
Tiers: `L4 1.0 · L3 0.7 · L2 0.4`. Boosts: `HdrBoostL3 = 1.0`, `HdrBoostL4 = 1.8`.
`gold = (1.15, 0.82, 0.18)` — brightest channel **1.15**, already over threshold before any boost.

| what | brightest channel | vs threshold 0.9 | differs between arms? |
|---|---|---|---|
| stage markings (L2) / actors, ball (L3), post-T41 | ≤ 0.665 | **under** — no bloom at all | no |
| L3 gold, not holding the token | 1.15 × 1.0 = **1.15** | over, faintly | **no** — `HdrBoostL3` is 1.0 in both arms |
| **the one L4 token holder** | **1.8 arm: 2.07 · 1.4 arm: 1.61** | both over | **YES — the only thing that does** |

That is the isolation batch 6 asked for. Last time the pitch sat at `#ffffff`/1.000 and bloomed
maximally in both arms, drowning the comparison; **T41's cap is what put the stage under 0.9**, so
the only element whose bloom changes between arms is now the single L4 holder that C3's one-token
invariant guarantees is exactly one. Everything else is constant by construction, which is what makes
a difference in the frames attributable.

Caveat to carry into the read: URP blooms the HDR buffer *before* tonemapping, so colour × boost is
the right input — but the grade still sits between that buffer and the PNG. If the two arms look
identical, suspect the tonemap flattening the top end, not the experiment.

### T49 procedure — the const-edit dance, in order

`HdrBoostL4` is `private const float = 1.8f` at `TvSweatScreen.cs:565`; there is no runtime knob.

1. Shoot **arm A at 1.8 first, unedited** — if the window dies, the surviving arm is the shipped value.
2. Edit `565` to `1.4f` → warm compile → shoot arm B, **same seeds, same order**.
3. **Edit back to `1.8f` and compile again before releasing.** If this step is skipped the surface
   ships at 1.4. Verify with `grep -n "HdrBoostL4 = " TvSweatScreen.cs` and by `git diff` being empty.
4. Frames are already self-evidencing — C8·a put the boost token in every filename off
   `DebugHdrBoostL4`, so an arm cannot be mislabelled after the fact.

### T48 — check the harness exists BEFORE launching

The last attempt cost an invocation to discover the method was absent. One line, first:

```
grep -c "CaptureConformance" unity/SBR/Assets/SBR/Editor/RoomViewCapture.cs
```

**0 means the room's merge has not landed — stop, do not invoke.** The recipe's §1 invocation is
otherwise correct as written.

### Rules that bit this slice and apply to both shoots

- **Never `-nographics` for captures.** The recipe says it, and it is what killed SureThing's three
  capture tests in my PlayMode run.
- **Never end a turn against a running capture** (§4 rule 4). Hold the wait in-turn, or re-arm a
  completion check each turn — never both launch and hand the turn back.
- **Liveness is artifact mtime, not process aliveness**, and seeds overwrite their own filenames, so
  frame count is not progress.

---

## 1. File ownership

### Owned exclusively by this worktree

| Path | Note |
| --- | --- |
| `unity/SBR/Assets/SBR/Runtime/TvSweatScreen.cs` | Session orchestrator |
| `unity/SBR/Assets/SBR/Runtime/TheaterStage.cs` | Scene playback |
| `unity/SBR/Assets/SBR/Runtime/TheaterChoreographer.cs` | Factual template + ledger payload |
| `unity/SBR/Assets/SBR/Runtime/ScenePlaybook.cs` | `SceneSpec` |
| `unity/SBR/Assets/SBR/Runtime/SweatPresentationModel.cs` | Score/count ledgers |
| `unity/SBR/Assets/SBR/Runtime/SweatPacer.cs`, `MomentumTape.cs` | Pacing, tape |
| `unity/SBR/Assets/SBR/Runtime/TheaterScenePlanner.cs`, `TheaterScenePlan.cs`, `PresentationSceneKey.cs` | Phase 2 planner stack |
| `unity/SBR/Assets/SBR/Runtime/TvLight.cs` | **Ownership confirmed by Allen 2026-07-27** — was on neither list; room lead disclaims it |
| `unity/SBR/Assets/SBR/Runtime/Shaders/TvSweatHdrUI.shader` | Created by this worktree |
| `unity/SBR/Assets/Tests/PlayMode/TheaterStage*.cs`, `TvSweatScreenTests.cs` | TV/theater tests |
| `unity/SBR/Assets/Tests/EditMode/PresentationSceneKeyTests.cs`, `TheaterScenePlannerTests.cs`, `TvLightTests.cs`, `TvSweatScreenPaletteTests.cs` | |
| `DESIGN.md`, `PRODUCT.md` (root) | TV surface design system; product record |
| `docs/tv-sweat-refinement/**` | PRD, visual design, bug ledger, briefs, evidence |
| `docs/handoffs/tv-sweat.md` | This contract. Moved from the repo root 2026-07-31 — a committed root `handoff.md` collides across worktrees at merge time (studio convention) |

### Read-only (diagnosis permitted, edits are an escalation)

`engine/**`, `SBR.Engine.dll`, `RunDirector.cs`. Granted read access by Allen 2026-07-29 for
diagnosis; a needed change escalates to the orchestrator.

### Never touched by this worktree

`TvAudioDirector.cs` (audio deferred, PRD §3) · `Room.unity`, `GrayboxRoomBuilder.cs`, room
materials and lighting rig (room-refinement) · Laptop/SureThing files (surething-ui) ·
`ProjectSettings/**` and package manifests (**integration-only per STUDIO.md**) ·
`docs/ARCHI.md`, `DECISIONS.md`, root plans (**integration-only**; needed updates recorded in §6).

### Known boundary hazards

- **Build side effects.** Every `dotnet test` and Unity run dirties `SBR.Engine.dll`,
  `ProjectSettings/EditorBuildSettings.asset`, `ProjectSettings/ProjectSettings.asset` — two of which
  are integration-only. Revert after **every** run and verify `git status` before committing. This
  recurs constantly and is a property of the build wiring, not agent error.

  **Chartered convention (Allen, 2026-08-09): nobody commits the Sentis/ShaderGraph settings churn.
  Cmp-verify, then checkout.** It is studio-wide, not a TV rule.

  **HOW to cmp-verify here, because the obvious two ways are both wrong on this repo:**

  1. **Not the `git status` line.** `SBR.Engine.dll` shows ` M` permanently. `[attr]lfs` is declared
     in `unity/SBR/.gitattributes` but git honours attribute macros only in a **top-level**
     `.gitattributes`, and this repo has none at root — that is what the `not allowed:` warning on
     every git command means. The filter never binds, so the file reads as modified whatever its
     content.
  2. **Not `git hash-object` either, and this one is newer.** It applies the same broken clean filter
     and returns **different hashes for identical bytes between invocations** — measured 2026-08-09:
     it reported `83f8a7de…` against HEAD's `b57d25c5…` on a file that was byte-for-byte identical.
     A check that disagrees with itself is worse than no check.
  3. **Compare the FILE BYTES.** `Get-FileHash -Algorithm SHA256` on the working file against HEAD's
     blob extracted to a temp path, or `fc /b`. That agreed with HEAD immediately in the same test.

  **RESTORING: check what the blob IS before you restore from it.** This is the rule, and the
  previous version of this note got it wrong in a way that corrupted the DLL on 2026-08-10.

  A tracked binary here is in one of two states, and **the correct restore is opposite in each**:

  | HEAD's blob | `git checkout -- <path>` | `git cat-file -p <sha> > <path>` |
  |---|---|---|
  | **a raw binary** (pre-round) | writes different bytes each time — broken | **correct** |
  | **an LFS pointer** (post-round) | **correct** — smudges to the real file | writes the 130-byte POINTER TEXT — corrupts it |

  So: `git cat-file -s $(git rev-parse "HEAD:<path>")`. **~130 bytes means a pointer** — use
  `checkout`. A full-size blob means a raw binary — use `cat-file` through **cmd** redirection
  (PowerShell's `>` re-encodes as text and corrupts binaries).

  **What went wrong, because the failure mode is the point.** After the round, `SBR.Engine.dll`
  became pointer-backed. The fast-forward smudged it correctly to 94,720 bytes. Then the old rule
  above was applied by reflex — `cat-file` — which overwrote a working assembly with the pointer's
  own text. `Bad IL format`.

  **And the cmp-verify PASSED while the file was broken**, because it hashed the restored file
  against the same blob it had just copied from: pointer against pointer, identical, green. **A
  comparison against the thing you just wrote proves only that the copy succeeded.**

  **So verify a restore by USING the artefact, not by hashing it.**
  `[Reflection.Assembly]::LoadFile` must report `SBR.Engine` and a plausible type count. That check
  caught this; the hash endorsed it.

  **A .NET rebuild can never hash-match its predecessor** — the MVID is regenerated every build — so
  a genuinely rebuilt DLL always needs a restore; only an untouched one is already identical.
- **`GrayboxRoomBuilder.Build()` regenerates `Room.unity` from scratch** and rewrites builder-owned
  material properties. Nothing hand-placed survives. Anything this worktree needs persistent in the
  room goes through the room lead.

## 2. Local plan

Approved sequence (PRD Decision D): audit → reliability → scene variety → UI → integrated gate.

| Phase | State |
| --- | --- |
| 0 Design gate | Closed — `APPROVED WITH CHANGES` |
| 1A Audit | Closed |
| 1B Reliability (TVS-H01/H02/S01/H03) | Closed, Allen signed off 2026-07-27; audit-rerun gate waived by name |
| 2A–2E Scene variety | **Closed** at `220c5ec`; automated gate met |
| **3 UI refinement (T7)** | **Unblocked** by the C1 ruling once this contract lands |
| 4 Integrated acceptance | Three muted couch sweats; needs GPU |

**Phase 3 contents:** Layout B build per `DESIGN.md` §6, brand-book palette and brightness ladder,
§8.8 stats panel, §8.10 held cash-out preview, §7.7 backed-player locator, plus the carried debts —
T9 `chromeCyan`, T10 emission rest values, and the deferred scorer-reveal gap (a won anytime-scorer
leg whose backed-side goals are spent before the final sequence produces no reveal).

**Held, not started:** T8 scanline overlay and `DeadLegBeat` static crawl. `DESIGN.md` §2 bans both
by name; removal is recommended and awaiting Allen. **Nothing further is built on either effect.**

## 3. Delegation bounds

- **At most two bounded sub-agents at once**, per STUDIO.md. Current practice has been one at a time
  for anything touching `TheaterStage.cs`, because every Phase 2 dispatch collided there.
- Every dispatch names allowed files, forbidden files, required evidence, and an exit gate.
- **Never invent a runtime result, seed, rate, or test outcome.** Honest "NOT RUN" beats a
  fabricated row.
- **A failing test is evidence, not an obstacle.** Deleting or weakening one to make a change pass
  requires this lead's explicit agreement. This has been attempted once (TVS-S01) and was caught in
  diff review.
- The lead reviews the diff, not the summary. Agent reports have been accurate and still wrong:
  TVS-S01's fix re-created its own bug in the opposite direction with all suites green.
- Sub-agents do not commit unless the dispatch says so, and never touch `.impeccable/`.

## 4. Verification procedure

**Unity is a single-instance studio-wide resource. A lease is a WINDOW, not a moment.**

Added 2026-07-31 after a queue violation: this lead confirmed the editor free at *close* but never at
*open*, and a still-exiting Unity process overlapped another worktree's granted slot. Transient and
harmless that time. The procedure below closes it.

0. **Before opening — every time, not just the first:**
   a. Hold an explicit grant from the orchestrator for the current slot. A general "queue is clear"
      from an earlier cycle is **not** a standing lease; a later sequencing note supersedes it.
   b. Confirm the editor is actually free: process count **and** `unity/SBR/Temp/UnityLockfile`.
      **The check must ABORT the run, not merely print.** Amended 2026-07-31 after a slot opened on a
      reported-free editor that read process count `1` — a straggler mid-exit. The check printed the
      1 and the batch proceeded anyway, which made it advisory rather than a gate. A coordinator's
      "verified free" and this lead's "free at my open" can differ by seconds.
   c. **Announce open** to the orchestrator.

   **Known editor fault, three occurrences 2026-07-31:** Unity segfaults on `-quit` shutdown and
   leaves a **stale `UnityLockfile` with zero processes**. Clear it (safe when process count is 0)
   before opening. `-runTests` runs are unaffected and have produced valid XML every time — the
   fault is on the shutdown path, not on results.
1. **After the last run — announce close**, and confirm process count and lockfile are clear before
   saying so. Unity exits lazily; a finished command is not a released editor.
2. The window between (0c) and (1) is yours and nobody else's. Anything that does not need the editor
   — reading source, writing tests, diagnosing from a results XML — belongs **outside** it. Diagnose
   from artifacts after closing rather than holding the editor open to think.
3. **A silent automated run is indistinguishable from a slow one.** Added 2026-07-31 after a driver
   sat dead for 35 minutes of a granted window while its process stayed `ALIVE` and its monitor,
   tailing a log nobody was writing, never woke. **Liveness is artifact mtime, not process
   aliveness.** Three named traps, all measured, all mine:
   - **`Unity.exe` is a GUI-subsystem binary**, so `& $unity` returns *immediately* — a loop that
     trusts it stacks overlapping editors inside your own window. Do **not** patch that with
     `Start-Process -NoNewWindow -Wait`: from a console-less parent (a `-WindowStyle Hidden` pwsh)
     that combination hangs forever *without ever spawning Unity*. `Start-Process -PassThru` then
     `Wait-Process -Id` is the pair measured to work.
   - **`$Args` is a PowerShell automatic variable.** `function Invoke-Unity([string[]]$Args)` leaves
     it empty, so Unity launches with **no arguments at all** — no project, no filter, no `-logFile`
     — and exits **0 in ~11s** having done nothing. It writes to the default
     `%LOCALAPPDATA%\Unity\Editor\Editor.log`, whose `COMMAND LINE ARGUMENTS:` block is how you
     prove it. Name the parameter anything else.
   - Both failures reported **success**. §4 step 3's "the XML must exist and be newer" is what caught
     each one; neither was visible from an exit code.
   **Measured costs at `5d61a04`:** warm compile ~106s; one filtered `TvSweatScreenTests` PlayMode run
   ~153s wall for ~31s of test time. Ten runs is ~26 min per arm — size batches against that, and
   prefer a foreground batch you can read over a background driver you must trust.

4. **NEVER END A TURN AGAINST A RUNNING CAPTURE.** Chartered 2026-08-01 after **three** silent
   capture-run deaths in one slice — not bad luck, a pattern:
   - A foreground `Start-Process` + `Wait-Process` inside a tool call dies when the call hits its
     10-minute cap, and **takes Unity with it**. Cost a run mid-seed-02.
   - A background waiter armed and then turn-ended was reaped twice, leaving the run unobserved.
   - A "no new frames" read on a healthy run was a false stall: seeds overwrite their own filenames,
     so **frame count is not a progress signal — mtime is.**

   The rule: **hold the wait IN-TURN** (a bounded polling loop inside one tool call — captures are
   ~60s per seed, well inside the cap), **or** self-re-arm a completion check each turn. Never both
   launch and hand the turn back. A capture nobody is watching is a capture that did not happen, and
   you will not learn that until the window is spent.

   Corollary, learned the same day: **launch detached** (`Start-Process` with no `-Wait`) whenever a
   run may outlive one tool call, so a harness timeout cannot kill the editor. Foreground reads more
   simply and is the wrong shape.

1. Warm compile: `Unity.exe -batchmode -nographics -projectPath unity/SBR -quit -logFile <log>`.
   `-runTests` and `-executeMethod` are **silently dropped** if scripts compile on the same run.
2. Suites, one at a time, waiting for the process and `Temp/UnityLockfile` to clear between:
   `dotnet test engine.tests` · Unity `-runTests -testPlatform EditMode` · `-testPlatform PlayMode`.
3. **Exit code 0 does not mean the run happened.** Verify the results XML exists and is newer than
   the edits under test.
4. `git checkout --` the three build side-effect files; confirm `git status` shows only intended
   changes before committing.

**Current baselines — measured 2026-08-15, batch 93 (§0-B93):**
EditMode **255 executed / 254 passed / 0 failed / 1 ignored** (G1's grant, held) ·
PlayMode **122 executed / 111 passed / 0 failed / 11 by-design skips**.

**engine: 292 / 292, 0 failed — measured EARLIER in the same window and NOT re-run since**, and the
branch has taken main merges after it. **Treat it as the last known good, not as today's number**,
and re-run before any claim rests on it. *Recorded this way deliberately: the honest failure of a
baseline is not that it is wrong, it is that it does not say how old it is.*

**PlayMode grew 95 → 122 across this window** — the merges brought the screen lane's suites, and this
lane added the stats panel's pins, the club-pool gate, the capture-path pin and two capture entry
points. **The `[Explicit]` skips grew with them**: the count is by-design opt-in evidence, never a
regression to chase.

**ALL THREE SUITES ARE GREEN, AND THAT IS NEW.** Batch 70's block carried two expected reds and told
the next seat to expect them; **both cleared on the merge from main, exactly as it predicted** — the
engine's 55 by `c82aefe`'s joint-model repair, the laptop margin pin by the surething-ui lane's own
fix arriving. **The rows are kept below as the record of a prediction that held**, not as live state.

> **THE PREDICTION WAS THE VALUABLE PART.** Both reds were diagnosed as inherited, each with the
> commit on main that already fixed it, and each cleared without a line of repair in this lane. A red
> that is *understood* costs a sentence; a red that is merely *observed* costs a seat.

| CLEARED — the record, not live | the failure | how it closed |
|---|---|---|
| **engine, 55 of 260** | one `TypeInitializationException` repeated — `JointModel`'s static ctor throws *"the outcome partition needs exactly one residual class, found 2"* | `c82aefe` on main, *"270/270 green (was 55 failing)"*, naming this exact count. Verified inherited by re-running with this lane's change stashed: **identical 55/205/260** |
| **PlayMode, 1 of 95** | `SureThingEntryTests.Working_margin_contains_its_content_at_the_legal_maximum_leg_count` — `4.74798583984375` against a signed 4.56 | a **SUPERSEDED test**; the surething-ui lane's repair came across on the merge |

> **A STALE BASELINE TURNS AN INHERITED RED INTO A DIAGNOSIS.** The engine line here read
> **160/160** from 2026-08-09 until batch 70 — taken *before* the main merge that brought the joint
> model in at all — so the first seat to run `dotnet test engine.tests` after that merge meets 55
> failures against a baseline claiming zero, with nothing to tell it the number is simply old.
> **Re-measure a baseline in the window that finds it wrong, and record the DISPOSITION beside the
> number rather than the number alone.**

*(Superseded: `220c5ec`'s 160 / 129 / 44, then 2026-08-09's 160 / 247 / 70. EditMode grew 129 → 222
on the main merge and has moved with the batches since — 224 (batch 9), 228 (T61/T62), 237 (batches
13+14), 247, 250, 251 (T95), 254, 255 (T98's pin). PlayMode reads 95 rather than 44 because the
suite now includes SureThing's and the `[Explicit]` capture seeds — **run it WITH graphics or
SureThing's three capture tests fail environmentally and look like regressions**.)*

**Known flake — do not mistake it for a regression.** `TvSweatScreenTests` fails
`never observed the cash-out amount mid-tween (waited 20s)` on load-heavy runs; logged in
`BUG-LEDGER.md` §4C.4. Measured 2026-07-30: **HEAD 1 failure / 4 runs; Phase 2E-2 1 / 10.** This lead
wrongly called it a 2E-2 regression at n=3 and was corrected by measurement. **PRD §6.1 requires ≥10
attempts on both arms before claiming any timing regression.**

**Visual evidence.** `-nographics` rasterises no frame. Every visual claim is labelled
`PENDING-VISUAL-EVIDENCE`; couch-distance acceptance cannot be asserted from headless tests and
needs a GPU session. PRD §6.1.1 splits the evidence standard accordingly.

## 4A. Design system — spec of record

**`main-2/docs/design/design-system/`** is studio canon as of 2026-07-31, committed on `main`.
**Reference it cross-worktree; do not fork copies into this worktree.**

What this slice builds against:

| Path | Use |
| --- | --- |
| `components/tv/*.jsx` + `*.prompt.md` | Built references and their specs. **The `.prompt.md` is the spec of record** — it is consistently stricter and more precise than a ruling summary |
| `components/tv/tiers.js` | Canonical brightness tiers: **L4 1 · L3 0.7 · L2 0.4 · L1 0.15 · L0 0** |
| `tokens/palette-tv.css` | Colour tokens |
| `guidelines/` — `tv-brightness`, `type-tv` | The laws behind the tiers and the typeface |
| `ui_kits/tv-sweat/` | Runnable kit of the whole surface |

**Read the `.prompt.md` before implementing from a ruling line.** Concrete instance: T16's summary said
"no numerals, no hue, never above L2"; `TvMomentumTape.prompt.md` additionally splits the tiers —
label and current sample at L2, sample history at L1 — and states the reasoning that makes the rule
enforceable ("the moment it needs a numeral it has become the banned win-probability readout"). A
test written from the summary alone under-specified all three rules.

Where a Unity test must assert against canon it cannot import (a C# test cannot load a JS module),
mirror the values as named constants and **cite the source path in a comment** — never invent a
threshold that happens to pass.

## 4B. TVS-H02 verification — **CLOSED 2026-07-31.** Do not re-run; resume at §4C

Kept as the record of how the fix was judged, not as a live instruction. Outcome: both arms n=10,
**zero** `kept ticking while standing` in each, 3 documented mid-tween flakes in each — identical
rates, so the flake is not stack-induced. The defect failed 3 of 4 before the fix and 0 of 10 after.
Committed with 3C + T16/C3/C8 as `4969eb1`. **T17 is also done** (`ea28c9b`); see §4C for what is next.

### State

The working tree carries a large **uncommitted** stack on top of HEAD `5d61a04`:

- Phase 3C — Layout B canvas rebuild (`TvSweatScreen.cs`)
- The T16 / C3 / C8 Design Director rulings — momentum tape restored at the scorebug foot; HDR
  eligibility widened to five with a one-token invariant; risk/pays in the bloom-floor protected set
- A tape-coupling fix — `MomentumTape.Build` moved **out** of `if (theaterEnabled)`; it is scorebug
  furniture, not stage furniture, matching the ball flash's existing precedent
- The **TVS-H02 fix** described below
- Tests: `TvSweatScreenLayoutGridTests.cs` (new), additions to `TvSweatScreenPaletteTests.cs`
  (markup scan, one-token invariant, arbitration, tape rules)
- Docs: `DESIGN.md` §9A, PRD §7.2.1 authored inventory, and 49 captures staged at
  `docs/tv-sweat-refinement/visuals/phase-2-scene-grammar/` for the DD's T6 visual review

### The defect and the fix

`TvSweatScreenTests.Standing_Freezes_CashOutTween_NoResumeCatchUp` failed **3 of 4** runs with the
stack and **0 of 3** at clean HEAD.

**Mechanism (confirmed by static analysis, not yet by execution):** `StartCoroutine` runs a coroutine
body **synchronously up to its first `yield`**, before returning the handle assigned to
`_cashOutAnimation`. A new tween's first `RenderCashOut` therefore ran while `_cashOutAnimation` was
still `null`, and the stack's new `_cashOutAnimation != null ? "UPDATING" : "[E]"` ternary painted
the wrong branch for exactly one frame, self-correcting the next. If the test caught that frame, the
correction landed *after* standing — a text change with the dollar amount frozen throughout. This
predicts the observed 3/4 rather than 4/4, because it is frame-scheduling dependent.

**The amount never ticked; the freeze held.** A one-frame render bug that freezing captured. The
quirk pre-dated the stack; the new `UPDATING` state made text sensitive to it for the first time —
exposed, not introduced.

**Fix location:** `unity/SBR/Assets/SBR/Runtime/TvSweatScreen.cs` — new `bool _cashOutTweening`, set
`true` **before** `StartCoroutine` so the coroutine's own first render sees it, and `false` before
each settle-render. `RenderCashOut` and `DebugCashOutAnimating` read it instead of the handle.
`elapsed += SeatedDeltaTime` — the actual freeze primitive — is untouched, as are `_l4Holder`,
`RequestL4`, `ReleaseL4`.

**Disqualified suspect, do not re-investigate:** the ungated C3 tail in `AnimateCashOutTaunt`.
`CanAcceptCashOutNow()` requires `_cashOutAnimation == null`, so `actionable` was already `false`
mid-tween — the block behaves identically before and after standing in this scenario. It *is*
genuinely ungated by `_seated`, which is judged **correct**: standing means input is refused, so the
L4 actionable promise should end (§8.5, "brightness is a promise about input"). Carried, not a bug.

### Exit criteria — judge by failure MESSAGE, never by test name

Two failure modes share this test name and mean opposite things:

- `cash-out amount kept ticking while standing` → **the regression**
- `never observed the cash-out amount mid-tween (waited 20s)` → the documented load-correlated flake
  (`BUG-LEDGER.md` §4C.4; measured HEAD 1/4, 2E-2 1/10). **Permitted at its documented rate; not a
  miss.**

1. **≥10** filtered runs with the stack:
   `-runTests -testPlatform PlayMode -testFilter "SBR.Tests.PlayMode.TvSweatScreenTests"`
2. **≥10** at clean HEAD (`git stash push -- unity/SBR/Assets`, run, then **`git stash pop`** — the
   stack is uncommitted and must not be lost).
3. **Green = zero** `kept ticking while standing` in the stack arm.
4. Then full `dotnet test engine.tests` + EditMode + PlayMode.
5. On green: **commit 3C + T16/C3/C8 + the TVS-H02 fix**, then advance to **T17**.

Baselines before this stack: engine **160**, EditMode **194**, PlayMode **44** (+1 `[Explicit]`
capture harness, filtered out of routine runs).

## 4C-0. RESUME HERE — T43/T46/T42/T44 written, **NOT COMPILED** (2026-08-03, canon through batch 8)

**Open the next editor window with a warm compile BEFORE anything else.** Four rulings were
implemented in one pass with no editor available; none of it has been through a compiler, let alone a
suite. 555 insertions across five files. Treat a green compile as the first deliverable of that
window, then engine → EditMode → PlayMode, then T49/T48.

**Baselines to beat (at `1128a91`):** engine 160 · EditMode 129 · PlayMode 44. Five new EditMode
tests were added, so EditMode should read **134** if all pass.

**One defect this fix introduced was caught in diff review, before it ran** — recorded because the
mechanism will recur. Moving the cash-out slot's derivation out of `Update` (which is what fixes the
transition frame) also put `RenderCashOut` on the path into it — including the `RenderCashOut` that
runs *synchronously inside* `StartCoroutine`, before the handle is assigned to `_cashOutAnimation`.
`CanAcceptCashOutNow` reads that handle, so mid-tween it answered "acceptable" for exactly one frame
and lit the gold field **at L4 during a price update**. TVS-H02's quirk, one element over, re-opened
by the fix for a state lie. `ApplyCashOutSlotState` now reads `_cashOutTweening` — the flag that
exists for precisely this — and never the handle. Pinned by
`T43_a_tweening_price_never_lights_the_field_or_takes_the_L4_token`.

**Standing lesson: any predicate that reads `_cashOutAnimation` is wrong for one synchronous step.**
There is a flag for it. Read the flag.

| Item | State | Where |
|---|---|---|
| T43 | Written. Three instances found, not one | `TvSweatScreen.cs` `ShowMarketSuspended`/`ApplyCashOutSlotState` |
| T46 | Written. Stage + score bug + event strip now clip to their own zones | `TvSweatScreen.cs` `ZoneRoot`, `TheaterStage.BuildInternal` |
| T42 | Written. Scorebug half was already landed by T32.1; the dot pool was the live half | `TvSweatScreen.cs` `teamHueA`/`teamHueB` |
| T44 | Written for the TV event strip. **The console twin is untouched and still holds every string T39 fixed** | `SweatFlavor.cs`, `TvSweatScreen.cs` |

### Window plan — compile is ready, T48 is NOT, T49 needs one frame of rig proof first

Checked against the rig recipe **before** the window rather than inside it. Three findings, one of
which would have burned the slot:

**1. T48 cannot run on this branch.** The recipe's mechanism is sound and its scene preconditions
mostly hold here — `TvLight`, `PhoneBuzzLight`, `TVScreen`, `LaptopScreen`, `PhoneScreen` are all in
this branch's `Room.unity`. But three things it needs are on `main` and **not on
`slice/tv-sweat-refinement`**:

| needed | here? | on `main`? |
|---|---|---|
| `unity/SBR/Assets/SBR/Editor/RoomViewCapture.cs` | **no** | yes |
| `tools/room_gate_check.py` | **no** | yes |
| `RoomPostFx` in `Room.unity` — the volume the whole bypass toggles | **no** | yes |

This branch is **51 ahead / 190 behind `main`**. Copying the two files across is not enough: the
harness throws on a missing `RoomPostFx` *by design* ("a set missing half its pair would silently
look complete" — do not soften that, and I will not). **T48 is blocked on an integration merge, not
on an editor window.** That is an orchestrator call — a 190-commit merge into a slice with 51 commits
concentrated in `TvSweatScreen.cs` is not a thing to start unilaterally at the head of a granted slot.

**2. T49 can run here — its harness is TV-side.** `TvSweatCaptureHarness` loads `Room` and stamps
`boost{X.X}` into every filename off `DebugHdrBoostL4`; it never touches `RoomPostFx`. Nothing blocks
the A/B.

**3. But prove the rig before spending the window on 34 frames.** T49 was withheld once already as
*confounded* — both arms bloomed maximally over an uncapped pitch. If bloom lives in `RoomPostFx` and
that object is absent here, the re-run measures nothing and gets withheld a second time for a second
reason. The previous A/B did produce bloom from this branch, so it is present by some route — but
"present by some route" is not a measurement. **First deliverable of the window: one frame at each
boost, confirm they differ, then shoot the set.** A confounded measurement closes nothing (C24 §2.6).

### T43 — the suspended slate was three defects, and only one was the one-frame kind

The DD's "dims a frame later" is real and was the *smallest* of the three. All three came from the
slot's four elements (figure, gold field, status word, L4 token) being derived in `Update` while its
state changed in coroutines, which run after `Update`:

1. **One frame of gold field** under `MARKET SUSPENDED` — the ruling's finding, exactly.
2. **`HOLD E` for the whole suspension.** The old guard cleared the status only when the slot was
   *invisible*, so a suspended-but-visible slot kept instructing the player to hold a key the accept
   gate refuses. TV-12/13 violation ("suspended owns the slot exclusively"), and not time-boxed.
3. **The word itself painted gold, for the whole pending-loss window.** `AnimateCashOutTaunt`
   repaints the figure gold every frame and was gated on `_marketSuspended` alone — but §8.7's
   pending window renders the suspended slate while the market is still *open* (`ResolveBeat` never
   calls `SuspendMarket`). So the literal words `MARKET SUSPENDED` rendered in full-brightness gold
   for as long as the player took to decide. **This is the likeliest thing the DD photographed.**

Fixed by rule: `ShowMarketSuspended()` is the one slate and both authoring sites call it;
`ApplyCashOutSlotState()` is the one derivation and every transition calls it as well as `Update`.
Eight `_tCashOut.enabled = false` sites route through `HideCashOutSlot()` for the same reason.

**`_cashOutSlotSuspended` is deliberately NOT wired into `CanAcceptCashOutNow`.** That predicate is
TVS-H01's input contract. Which leads to the one thing needing a ruling:

> **NEED DD/ALLEN — `E` still cashes out during the pending-loss window.** The slot says
> `MARKET SUSPENDED`, §8.7's doc comment says the market is suspended for the duration, and the
> engine happily accepts. Either the market really is suspended there (then `SuspendMarket()` should
> be called on that path and `E` must be refused — a gameplay change) or it is not (then the slate is
> the wrong copy). The presentation is now self-consistent either way; the input question is not
> this lead's to decide.

### T46 — the grid was never wrong

`Stage` and `ScoreBug` start at exactly `TicketColumn`'s right edge (265px of 980). Three structural
facts produced the overdraw, none of them a number: every zone's content was a direct child of the
canvas so no zone owned anything; `MakeText` builds with `HorizontalWrapMode.Overflow`, so a long
fixture centred in the score bug's 675px box spills ~200px per side and crosses into the column; and
the right-hand zones are built *after* `BuildTicketColumn`, so they win the z-fight where they reach.

`ZoneRoot()` makes each right-hand zone the parent of its own content and gives it a `RectMask2D`;
children moved to zone-local anchors (identical pixels, offsets now dropping the zone origin).
`TheaterStage` gets its own mask — its rect was always correct, but `NetRipple` sits at 0.485 of the
padded width and scales to 1.7, carrying its outer edge ~155px past the stage edge and, on a
left-side flash, into the ticket column.

**T25.1's canvas mask could never have caught any of this**: its bound is the glass, and this
overdraw never leaves the glass — it leaves its *zone*.

**Look at this on the first capture (C11):** the punch animations scale their element about its
centre — the event strip to 1.12, the score bucket to 1.18 — and those elements now sit inside a
clip rect. `Flavor` is 691px wide in a 715px zone, so at 1.12 it reaches 774px and **its edges now
clip where they used to paint across x=265 into the ticket column.** That is the fix working, and on
ordinary short lines (`LEG 2 — WON`) the ink is nowhere near the edge, so nothing should read
differently. A long authored line under a punch is the case to look for on frames. Verified by
reasoning, not by eye — the distinction §6.1.1 draws.

**The edge test is structural, and the reason is worth keeping.** `RectMask2D` clips at render time
and does not move a `Graphic`'s rect, so `GetWorldCorners` reports the same overflowing box masked or
not. A corner-based "containment" assertion here would pass identically before and after the fix — a
fifth vacuous green gate (C18 §4.2). The test asserts ownership + mask + zone bounds, and carries a
canary that fails if the overflow ever stops reproducing, so it cannot go quietly vacuous.

### T42 — the scorebug half had already landed; the dots had not

`4293baa` ("the scoreline goes cold") landed T32.1 *after* the DD's frames were shot, so the
name-hue finding is already fixed. What survived is the second clause: `TheaterPalette.TeamPool` is
five **fully saturated** hues (`#3D7BFF` `#E84DD0` `#FF8A2B` `#9B5CF6` `#F0F3F6`) assigned by a hash
of the team name — two of which are not in the TV palette at all — where
`tokens/palette-tv.css:22-23` names exactly two muted ones and its own header says "Team hues are
muted and confined to the pitch dots". `TvStage.prompt.md` types an actor's side as `team:"a"|"b"`.

**The pool is left alone on purpose: `SportsbookApp.cs` (the laptop, another worktree) draws its
matchup cards from it.** The TV took its own two constants instead, which is what canon's per-surface
token files are for (C4's shape). Two dead hue sources were retired with it — an unused
`homeRgb/awayRgb` fetch inside `UpdateScorebug` and a `TeamColor(Leg, bool)` with no callers.

> **Consequence for the DD:** a club no longer keeps a colour across matches — canon has two hues and
> they mean "backed side / other side". Identity is carried by the cold-white name. If the sides read
> as inseparable at four metres, T42 already names the remedy and it is form, never louder colour.

### T44 — T39 fixed by DIRECTORY, which is the same mistake one level up

The event strip is clean now: em dashes for the four ASCII sentence dashes T39 left *in the file it
edited*, the `…` character, `Disaster` and `Ugly.` and `That one hurt.` (superlative/editorial),
`This is happening.` (a prediction — CF's "never imply a guaranteed win"), `THAT'S YOUR MAN` → `THE
BACKED SCORER` (CF's impersonal address; `BACKED` is §7.7's own word), and `the board is set.` →
`THE BOARD IS SET` (CF puts state words in tracked uppercase; every sibling on that element already
is).

**T44 CLOSED 2026-08-03.** The orchestrator ruled (Allen-fired): `game-console` is a dead
prototype, sweep it anyway. Done — and it is the one part of this session's work that is actually
**verified**, because `game-console` is a dotnet project: `dotnet build SBR.ConsoleGame.csproj`
succeeds, 0 warnings, 0 errors. (The build dirtied `SBR.Engine.dll` exactly as §1's hazard note
predicts; reverted.)

`EventText.cs` took all thirteen: both lines T44 quotes by name, the six second-person strings T39
had fixed only in the Unity copy, `IT'S IN!`, and the superlative/editorial/prediction set. Two of
its lines are console-only (the scorer branch has no Unity twin) and were fixed by the rule rather
than mirrored. Tables were **not** made identical to `SweatFlavor.cs` — only violations were
touched; cosmetic drift in a dead file is churn.

**Widened by one step beyond the named file, deliberately:** a scan of the whole directory found the
same second-person violation in three sibling strings — `BettingScreen.cs:92`, `GameLoop.cs:170`
(`YOU WON`, also a celebration), `GameLoop.cs:176`. Fixing `EventText.cs` and leaving those would
have been fix-by-site for the third time in this ruling's history. `game-console` now scans clean for
second person, `!`/`?!`, superlatives and ASCII sentence dashes. The `y/n:` prompts are kept — CF
permits second person in genuine instructions.

**Superseded record of the blind spot:** `game-console/EventText.cs` is `SweatFlavor.cs`'s
byte-for-byte ancestor, is live in `SBR.slnx`, and **still contains every string T39 rewrote** —
including both lines T44 quotes by name, `"off the bar — a miracle brewing?!"` and
`"the crowd loses it"`, verbatim. T39 scoped itself to "owned runtime source", so `game-console/`
was never opened. That directory is on none of §1's three lists.

> **NEED ROUTING — who owns `game-console/`?** If it ships, it is a T44 violation with the ruling's
> own quoted strings still in it. If it is a dead prototype, it should be said so once and stop
> showing up in scans. Not touched pending that call.

Also found and NOT actioned (outside the event strip, judgement calls that belong to the DD): the
settlement consolation lines `"so close. they always are."` and `"the model remains extremely
confident."`, and `"the book thanks you for your patronage."` — all arguably CF's sanctioned satire
rather than drift, and all in fact-free flavour slots. `SportsbookApp.cs`'s raw-hex `<color=#…>`
markup on team names is T15's class and already routed to SureThing.

---

## 4C. Superseded — T41 landed; next is T43 (2026-08-02, batch 6 current)

**T41 is CLOSED (`3b5153e`) and Phase 3+ is unblocked.** The stage sits under the ladder: markings L2,
actors/keepers/ball L3, L4 reserved to the payoff overlay. The markings' **hue** is untouched on
purpose — canon's `--tv-pitch` is green, the build is cold white-grey, and T41 ruled the *tier*.

**Do next, in DD priority order:** T43 (MARKET SUSPENDED on a gold field — a state lie) → T46 (stage
overdraws the ticket column) → T42 (team hues saturated + in scorebug type) → T44 (event-strip copy
voice). T45 is room's.

**Two captures owed, both cheap, both needing an editor:**
- **T49** — re-run the 1.8/1.4 bloom A/B. It was *confounded by the uncapped pitch*; now that T41
  caps it the comparison is finally meaningful. Boost token already rides every filename.
- **T48** — re-shoot screens-dark + grade-bypassed. **BLOCKED, do not guess:** these are ROOM-side rig
  settings (R23's purpose-built set, R26's bypass pair). The orchestrator is having the rig documented
  by the room lead. Wait for that doc.

**Batch 6 answers, so these are no longer open:** T51 — the 0.3px yields, **stacked stays**, re-derive
the grid constant once at design time (that is the TV-15 answer, and it is legal). T52 — the tape is
one 28px strip, Phase 4. T50 — Encode confirmed in situ, T11 stands; column type after T46.

**Guard note for whoever picks this up:** the T41 luminance guard measures *brightest channel × alpha*,
not alpha. The first version asserted alpha, flagged a near-black background at 0.95, and would have
passed real violations while failing innocent ones. If you extend it, keep the axis.

## 4C-i. Superseded — the evidence-complete hold record (2026-08-02)

**Do not start anything. Every remaining item is a DD verdict, not work.** Items 10–12 referenced
below were answered in Batch 4 (as T21/T22/T23) and the section after them is kept as the record of
how they were reasoned, not as a live queue.

**Landed since:** the C14 audit (42 gaps, 4 falsified) and its fix-now block — the brightness ladder
applied where it was declared but dead, both canon faces wired, the tape colourless under T16, the
cash-out field inverted with money and status split; T24 re-measured in Encode Sans (**the deficit
does not survive** — 59px against a 69.3px slot, so risk/pays stays in the footer and the ruling's
fallback never fires); T30's threshold predicates retired for verbatim constant matching; T25.1
containment; T38/T40/T32.1 from Addendum II; TV-14's three-span compact row; T39 as one scan; and the
tape's MOMENTUM label, which **did not exist** when its tier was corrected.

**Evidence delivered and awaiting verdicts:**

| Bundle | What |
|---|---|
| `tv-sweat-setB-and-bloomAB.zip` | Set B, five seeds, **nine grammars**, first set rendered in Encode Sans |
| `tv-sweat-bloom-AB.zip` | C8·a pair, 17+17, frame-for-frame parallel, **every frame carries its own boost token** |

**Awaiting DD, nothing else:** TV-02 (tape shape — canon's single 28px strip vs this build's per-leg
rows), TV-15 (**measured**: stacked needs a 56px footer and lands the slot at 66.7 against a measured
need of 67; side-by-side fits at 214w of a 265px column with the footer untouched — a 0.3px verdict
is the DD's), plus the frame verdicts on both bundles.

**Two rules this slice paid for, both now in §4 above:** never end a turn against a running capture,
and fix by RULE not by SITE — `WonLegBeat` kept a violation already fixed in its two sibling beats,
and T39's first pass left six strings for the same reason.

---

## 4E. C15 — TextMeshPro migration scope (SCHEDULED, no build work yet)

Ruled by Allen, Option 1, both surfaces; sequenced by the orchestrator after the conformance wave.
Scoped here while the detail is fresh. **This is a scope, not a plan to execute.**

**What it buys.** The three deviations this surface carries as signed-and-impossible become simply
buildable, and then **expire**: tabular figures (which `fonts.css` calls *"mandatory and
non-negotiable"*), letter-spacing (`--tv-track-label .16em`, score `.02em`), and weight 600. All three
are unreachable in legacy `UI.Text` — no OpenType feature control, no tracking property, `FontStyle`
offers Normal/Bold only. They are the reason C15 exists.

### The four risks, in the order they will bite

**1. Every measured constant is a `UI.Text` number and must be re-measured.** This is the big one.
T24's live row (59px), T15's risk/pays cell (57×48), the `LineBox` 1.18 estimate, the slot arithmetic
— all taken with `UI.Text.preferredHeight` in Encode Sans. TMP measures differently (its own metrics,
margins, and extra-padding behaviour). **Treat every px in this slice as invalid on migration day**
and re-run the two measurement tests first — `T24_the_specified_live_row_measured_in_the_production_face_fits_its_slot`
and `T15_measure_the_risk_pays_cell_in_the_production_face` are already written for exactly this and
both refuse to measure outside Encode Sans.

**2. The HDR path is custom and does not come across.** Five graphics carry an instance of
`TvSweatHdrUI.shader` with an unclamped `_HdrBoost` — the mechanism that lets §3's L4 exceed 1.0.
TMP ships its own shader family, so the boost must be re-implemented against a TMP shader. The same
shader also carries `#pragma multi_compile_local _ UNITY_UI_CLIP_RECT` + `UnityGet2DClipping`, which
is what makes T25.1's `RectMask2D` containment bind the brightest layer. **Both properties must
survive together** — a TMP material with the boost but without clip-rect support re-opens T25.1 on
exactly the elements that most need clipping.

**3. C3's one-token invariant rides on material instances.** `RequestL4`/`ReleaseL4`/`_l4Holder` are
renderer-agnostic and should survive, and the tests that pin arbitration use focus keys rather than
renderers. But `L4_canvas_elements_get_the_hdr_material_and_L3_elements_stay_default` inspects
materials directly and **will need rework**, and TMP's material-preset model shares materials by
default — a naive port can give two elements one material and silently break "at most one element at
L4", which is the surface's most load-bearing rule.

**4. TMP font assets are new binaries.** TMP needs SDF Font Assets generated from the TTFs. Those are
binary, and `[attr]lfs` is still inert repo-wide (macros only bind in a top-level `.gitattributes`,
and there is none at root). Same open question as the captures — worth settling before the migration
rather than during it.

### Slot inventory

~18 standalone `Text` elements plus 3 per leg row × 6 rows. Two faces to carry across
(`EncodeSans` / `EncodeSansCondensed`), assigned per slot via the `Face` enum — that mapping is
already read off the component references and cited at each call site, so it ports directly.

### Sequencing note

Migrate **after** the conformance wave, as ruled. Doing it during would invalidate the measured
numbers the current rulings are being settled against — T51's grid re-derivation and T49's bloom A/B
both depend on `UI.Text` metrics holding still.

---

## 4D. Superseded — the Phase 3 gating record (kept for reasoning, not as a queue)

**T17 is CLOSED** (`ea28c9b`): the ledger reserves the backed side's last baked goal at configure
time, enforced in `CompleteGoal` and released by `PlanFinal`; `BindAnytimeScorer` is unchanged and
the causal reveal point did not move. The reproduction was inverted in place, and the DD's acceptance
property is `Every_won_anytime_scorer_leg_reveals_exactly_one_scorer_however_its_beats_ran`. Full
record, including the closure evidence, is in `BUG-LEDGER.md` under "T17".

**Open for the Design Director:** the reserve is player-visible — on a scorer leg the backed side's
score now holds one goal short until the final sequence. Intended by the ruling, but confirm it reads
right on screen.

**T20 is CLOSED** (`48a9fbd`). The ticket's "23→19px" described the *canon's* change, already applied
upstream in `tokens/typography.css`; neither number existed here. The Unity surface had never matched
that table, and the real blocker was structural — NEED and progress shared one `Text` at 12px, so the
re-derivation was not expressible until the row was split into `Line`/`Need`/`Progress`. The whole
surface is now on the canon scale, with two zone heights grown to hold it rather than type shrunk to
fit. **Deviation with the DD:** the live row has no market/price/state meta line — canon's three-line
row needs ~73px against a 69px fixed slot. Reasoning is in the `LegRowUi` doc comment; do not "fix"
it by shrinking NEED.

**Every size in T20 is verified analytically and none of it visually.** The next seated GPU capture is
unusually load-bearing: it is the first look at a surface whose every element changed size.

**3D is CLOSED.** The find was that §8's `VOID` treatment — "L2 cyan, **struck through** on the
matrix" — had never been implemented; colour alone was carrying the state that means *cancelled*,
against `W` gold and `L` dark. The strike is now a fixed-width hairline rule per row, enabled only on
a voided leg. Do not re-derive its width from the text: §6 forbids geometry from content, and a test
pins it across empty/short/120-char copy.

**Gate item 3's "eight states" is not eight cash-out states** — the rectangle holds six. PRD §5 names
eight across two surfaces: five cash-out slot states plus won/lost/void. `phase-3-plan.md` had
collapsed them into one count; both facts are recorded there now. The gate word is *contradictory*,
not unique — suspended and pending-window share a treatment on purpose, so a uniqueness test fails on
a pair the design intends.

**3E is PART-CLOSED** (`4597b60`). §8.10's preview is built, tested and **deliberately unbound** —
PRD §8.10 never says what *confirming* is, and today `E` accepts on `WasPressedThisFrame` (pinned by
TVS-H01), so hold-to-preview cannot coexist with press-to-accept: acceptance fires on frame one and
no hold is observable. Binding it is one call site once the DD rules. Do not guess the gesture; it
breaks a pinned contract either way.

**§8.8's stats panel is NOT built and should not be started blind.** Two of its five required rows
cannot be sourced: the engine has **no formation and no shots concept at all**, and `Player` carries
only `Name`/`Role`/`ScoringWeight`, where `ScoringWeight` is hidden generator truth — the only
per-player number in the game is itself the leak §8.8 calls blocker-class. Per-team corners/cards
*are* available (the §7.6 fix landed; `CountLedger` tracks `Home`/`Away`). So it is three-fifths
buildable, and shipping that is a scope call, not this lead's.

**Visual evidence now EXISTS.** 98 frames bundled repo-free at
`scratchpad/tv-sweat-evidence-4597b60.zip` (57 MB): 49 new seated-sweat captures through the live
URP path plus the 49 held T6 scene-grammar frames, with a manifest. **Two gaps stated there and worth
repeating:** the seed produces no VOID leg, so 3D's strike appears in no frame; and the §8.10 preview
is unreachable while unbound, so it cannot be captured at all yet.

**3F is PART-CLOSED** (`949c041`). §7.7's binding half is built, wired at kickoff and tested:
`DotIndexFor` is the single expression both `SetBackedPlayer` and `RoutePass` use, so "the marked
actor IS the final-touch actor" holds by construction rather than by two formulas that agree today.
The locator is **wired but invisible** — the treatment is DD item 12.

### The three open DD answers, and why each blocks rather than bends

Phase 3's remaining work is **all** behind these. Every one is a case where the spec ran out before
the work did, and guessing would have broken something already pinned:

| # | Question | Why it cannot be guessed |
| --- | --- | --- |
| 10 | §8.8's two unsourceable rows | The engine has **no formation and no shots**, and `Player` carries only `Name`/`Role`/`ScoringWeight` — the sole per-player number is hidden generator truth, i.e. the leak §8.8 calls blocker-class. Three of five rows are buildable; shipping that is a scope call |
| 11 | §8.10's confirm gesture | `E` accepts on `WasPressedThisFrame`, pinned by TVS-H01. Hold-to-preview cannot coexist with press-to-accept — acceptance fires frame one, so no hold is observable. Either binding breaks a contract |
| 12 | §7.7's locator treatment | `DESIGN.md` §7's "numbered cell" is justified by "the matrix gives legible small numerals for free"; §6 records that matrix as **retired**. `TheaterStage` has no `Text`/`Font` at all, so a numeral adds a font dependency on a dead rationale, while a ring is nearly free (`RingSprite()` exists) |

**Do not pick up a fragment while these are open.** Two of the three are one-line changes once
answered; starting adjacent work instead produces more half-built features, which is how this slice
accumulated three in a row.

**When answers land:** 12 first (smallest, and it makes the locator capturable), then 11, then 10.

**Blocked on Allen, studio-level:** `[attr]lfs` is declared in `unity/SBR/.gitattributes`, but git
honours attribute macros only in a **top-level** `.gitattributes`, and this repo has none at root —
that is what the `not allowed:` warning on every git command means. So `*.png`, `*.dll`, `*.fbx`,
`*.wav` go through **no** LFS filter anywhere, including inside `unity/SBR`. The 49 phase-2
scene-grammar captures (28.8 MB) are held out of history pending that call and Allen's decision on
where they should live.

### Superseded: the T17 dispatch note this section replaced

DD ruled the scorer-gap a **correctness defect**, above every Phase 3 visual refinement. Design
instruction: **reserve, don't spend** — a scorer leg claims its backed-side goal *before* ordinary
beats spend the baked goals. If binding is ever impossible, **stage the reveal; never suppress the
win, never synthesise a reveal after resolution.** Acceptance is a **test**, not a capture: every
settled anytime-scorer leg traceable to a staged, revealed scorer event that preceded or coincided
with its resolution. The existing reproduction
(`BindAnytimeScorer_binds_nothing_when_the_backed_sides_goals_are_spent_before_the_final`,
`ScoreLedgerTests.cs`) is the red test that fix turns green — **invert it, do not delete it.**

Then: T20 px re-derivation (live progress 23→19px, resolved rows 19→15px, NEED unchanged — and do
**not** shorten §6's authored strings to fit), then 3D → 3E → 3F.

## 5. Standing context

- **Routing:** design decisions → Design Director; critical/strategy → orchestrator → Allen. Never
  straight to Allen. This lead implements approved specs and makes essentially no design calls.
- **Reporting:** result-first, telegraphic, ending `Done / Next / Risk / Need Allen`. Evidence stays
  local; raw logs never travel upward.
- **C1** — ruled 2026-07-31: latest document governs, `DESIGN.md` §6 stands, layout closed. Recorded
  in PRD §13 row A and §14.
- **C2** — light-spill colour: interim. Shipped green tolerated; target is `DESIGN.md` §5 cold
  white-grey, corrected in Phase 3. `TvLight.idleColor` is already `(0.72, 0.75, 0.80)` at
  `1aa74c3`; if green persists in-scene the residue is the room-side rig, not this file.
- **C3** — TV canvas HDR: owned here, blocks room-side fidelity. Proposal in §6.
- **Deferred by Allen, not rejected:** FIFA-style follow-cam, degrading visual register, bunkmate
  character, same-match concurrent legs (PRD §8.2A — reclassified as a betting-math feature; the
  engine forbids it at `Run.cs:181`).

## 6. Owed to integration

Recorded here rather than edited directly, per STUDIO.md's shared-docs rule:

1. **`DECISIONS.md`** — needs the C1 ruling and the Phase 1B sign-off with its waived audit-rerun gate.
2. **`design/08-art-direction.md` is deprecated** (Allen, 2026-07-24) and the game has had **no
   art authority** since. `DESIGN.md` replaced it for the TV surface only; the room, laptop and phone
   have no owning document. Allen's "high-tech city, dystopian" direction (2026-07-26) is the seed of
   the replacement. **This is a studio-level gap, not a TV one** — flagged for the Design Director.
3. **Unified post-process grade** — spec at `docs/tv-sweat-refinement/unified-grade-spec.md`, needs a
   global volume in `Room.unity`. Room lead owns implementation; this worktree owns the spec.
4. **`UnityEngine.Random` survives at `TvSweatScreen.cs` `_emissSeed`** (idle emission flicker phase).
   Found while removing T8, which took out the other use. PRD §4.3 bans the API for a *discrete scene
   choice*; a flicker phase seed is not one, so it was left alone rather than widening T8's scope.
   Consequence worth knowing: the idle flicker differs run to run. Phase 3 decides whether to move it
   onto the presentation key.
