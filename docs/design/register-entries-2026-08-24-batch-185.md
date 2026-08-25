# Register entries — batch 185 (2026-08-24)

**Four of the "seven unauthored kinds" are ALREADY AUTHORED and two of those already MEASURED — so
most of this is a build order. The three that are genuinely unauthored are held by Allen, and their
fallback re-creates `T156`.**

**Two rows.** **Destination table:** TV (`T169`, `T168-am`).

**Spec:** `docs/design/spec-need-copy-and-club-naming-2026-08-24.md`.
**Six source reads. Nothing measured — the measurements are the lane's, per `T111`.**

---

## The rows

| T169 | The seven kinds, split — FOUR are a BUILD ORDER, THREE are held, and the held three's FALLBACK re-creates `T156` | **SPEC'D — DD 2026-08-24 batch 185, on the dock's routed item. `docs/design/spec-need-copy-and-club-naming-2026-08-24.md`.** **THE DOCK ROUTED *"seven offered kinds still have no authored NEED copy."* **TRUE OF THE BUILD, NOT OF THE REGISTER**, and the difference decides the job: `DescribeActiveLeg` carries seven arms, DoubleChance has left the offered set, so seven offered kinds reach the `default` — **but FOUR of them have copy already ruled and sitting unbuilt.** `C57`'s discriminator exactly: in the register, absent from the deck, absent from the build.** **THE FOUR ARE A BUILD ORDER, NOT AN AUTHORING JOB: `Handicap` — `G1-am11` rung 3 `{CLUB} ±1.5`, TV-measured **20/20 at 249.4px**. `PlayerMultiScorer` — rung 2 `{SURNAME} 2+`, **12/12 at 175.4px**. `TotalGoalsOddEven` — `T151`'s forms, `T161` reading the ladder as sufficient. `WinningMargin` — `T151` plus `G1-am11`'s rung 3. **Two are measured and buildable now; two want one sweep first**, and nothing is re-authored.** **AND `WinningMargin`'s FALLBACK IS ACTIVELY WRONG, WHICH RAISES ITS PRIORITY: `MatchModel.Fields` gives it `Line = "3+ GOALS"`, so `NameOf` renders its NEED as **`3+ GOALS`** — and `T151` authored `MARGIN`/`APART` for exactly this reason, *"the engine's bare `2 GOALS` collides with the total-goals family's own forms on the same column."* **The fallback is the precise string that ruling exists to prevent.*** **THE THREE HELD — `TeamTotalGoals`, `TeamTotalCorners`, `TeamTotalCards`. No copy may be authored: `T152-am` withdrew their forms and Allen holds them. **BUT THE FALLBACK IS NOT A NEUTRAL HOLDING STATE.** `MatchModel.Fields` gives a team total `Line = "{FULL TEAM NAME} {OVER\|UNDER} {n.n} {NOUN}"` — the FULL name, not the noun — so the NEED renders `San Francisco Spreadsheets UNDER 1.5 GOALS`. **TV measured the SHORT-club form at 449.5px against a 261.0px band; the fallback starts strictly longer, so truncation is CERTAIN**, `T155` says truncation takes the market noun first, and **goals and cards at the 1.5 line then both render `{CLUB} UNDER 1.5` — `T156`'s collision, arriving through the very fallback that fixed the silence.*** **SO THE THREE ARE NOW WHERE DOUBLECHANCE WAS BEFORE ALLEN RULED (b): their copy cannot be repaired by shortening (`T152-am`), the fallback re-creates the collision that caused the withdrawal, and both remaining moves are Allen's — **author terse copy against his own hold, or take them out of the offered set.** ESCALATED, NOT ANSWERED, and the one number that decides whether `T156` is live in the build TODAY is routed to the lane** | batch 185 |
| T168-am | `T168` RULED — the TV names clubs its own way whatever the source, and the fix is at the RENDER not the source | **RULED — DD 2026-08-24 batch 185, discharging `T168` (batch 184).** **RULED: EVERY CLUB NAME THE TV RENDERS PASSES THROUGH `SweatFlavor.Short` — the distinctive word, city dropped. That is `T69`'s shipped convention and the build states it in those words at `LegStatement`'s moneyline arm.** **`7dd5686` REPAIRED `T130`'s SILENCE BY ROUTING THE ROW THROUGH `MarketSheet` (`S96`, §6.5), AND THE SOURCE IS RIGHT — `MarketSheet` is the single authority on what a bet is CALLED, and `S96`'s one-composer rule is why the console and the laptop agree. **What came with it is the club FORM.*** **FRAME B CARRIES THE PROOF ON ONE SCREEN: the row reads `DULUTH AUDITORS`, the scorebug `AUDITORS`, the strip `Gravediggers` — **three renderings of a club name in two conventions, in one frame.*** **THE FIX IS AT THE RENDER, NOT AT THE SOURCE: take the identity from `MarketSheet`, apply `Short` to the club token. **NO NAMING AUTHORITY MOVES** — which is what keeps this from re-opening `S96`, and it is the same shape as `K17-cl`'s adapter rather than a deletion.** **SCOPE: every kind `7dd5686` re-routed, not `Handicap` alone — **and it reaches `T169`'s held three hardest**, where the full name is what makes truncation certain. **Applying `Short` there does NOT rescue them** (TV measured the short form at 449.5px against 261.0), so this row shortens a string without settling a market: `T169`'s escalation stands either way.** **NOT IN QUESTION: the repair. `T130`'s silence was the defect, the blank-row gate over 25 selections across 14 kinds is the right shape, and `B2` passed WITH the row as rendered — **the naming is a register question, never a legibility one** | batch 185 |

---

## For the orchestrator

- **Two kinds are buildable today** with no measurement owed — `Handicap` and `PlayerMultiScorer`.
- **One sweep unblocks two more** — `WinningMargin`'s rung 3 and `TotalGoalsOddEven`'s `AT FT` rung.
- **One number decides whether `T156` is LIVE in the shipped build**: what the team-total fallback
  truncates to at the 1.5 line, for goals and for cards. **Cheap, and it should be measured before
  the scope call rather than after.**
- **WITH ALLEN:** the three team totals, on the same fork DoubleChance took.
- **Backlog is 185.**

## Limits

- **Nothing measured here.** The 449.5px and 261.0px figures are TV's from `ee16f06`; the
  certainty of truncation is arithmetic on them plus the fallback being strictly longer, **not a
  measurement of the fallback itself** — which is why §5 asks for exactly that.
- **`T169` re-authors nothing.** The four kinds' forms stand as ruled.
- **`T168-am` shortens a string; it settles no market.**
