# SPEC — the seven kinds' NEED copy, and the club-naming convention (`T168`)

**Written:** Design Director seat, 2026-08-24, on Allen's order to spec them together.
**Author; the lane measures** (`T111`).

**The dock routed *"seven offered kinds still have no authored NEED copy."* That is true of the
BUILD and it is not true of the REGISTER — and the difference decides the whole spec.**

---

## 1. THE SEVEN, NAMED AND SPLIT

`SweatActiveLegModel.DescribeActiveLeg` carries **seven arms**: Moneyline, TotalGoals,
BothTeamsToScore, TotalCorners, TotalCards, CorrectScore, AnytimeScorer. Fifteen kinds exist,
DoubleChance has left the offered set, so **seven offered kinds reach the `default`**:

| kind | state | what it needs |
|---|---|---|
| **Handicap** | **authored + MEASURED** — `G1-am11` rung 3 `{CLUB} ±1.5`, TV **20/20 at 249.4px** | **BUILD** |
| **PlayerMultiScorer** | **authored + MEASURED** — `G1-am11` rung 2 `{SURNAME} 2+`, TV **12/12 at 175.4px** | **BUILD** |
| **TotalGoalsOddEven** | authored (`T151`); `T161` — *"the ladder saves it"* | build, **confirm the rung** |
| **WinningMargin** | authored (`T151` + `G1-am11` rung 3 `2 APART AT FT` / `3+ APART AT FT`) | **MEASURE, then build** |
| **TeamTotalGoals** | **WITHDRAWN** (`T152-am`), held by Allen | **§3 — nothing may be authored** |
| **TeamTotalCorners** | **WITHDRAWN**, held | §3 |
| **TeamTotalCards** | **WITHDRAWN**, held | §3 |

**So four of the seven are a BUILD ORDER, not an authoring job** — `C57`'s discriminator exactly: the
copy is in the register, absent from the deck, absent from the build. Only the last three are a copy
question, and their answer is that copy is forbidden until Allen rules.

---

## 2. THE FOUR — BUILD WHAT IS ALREADY RULED

Nothing is re-authored here. The forms stand as ruled at `T151`, `T152` and `G1-am11`, and each rung
is selected **by measurement** (`FitOrFallback`), never by authoring intent.

**Owed before the last two ship:**

- **`WinningMargin`'s rung 3 was never measured.** TV's `ee16f06` reported two ladders ending —
  Handicap and PlayerMultiScorer — and `WinningMargin` was not among them. **Measure `2 APART AT FT`
  and `3+ APART AT FT` over the pool before either ships.**
- **`TotalGoalsOddEven`'s `AT FT` rung** is ruled sufficient on `T161`'s reading of TV's per-form
  pass. **Confirm it in the same sweep** rather than inherit it.

> **AND THE FALLBACK IS ACTIVELY WRONG FOR `WinningMargin`, WHICH RAISES ITS PRIORITY.**
> `MatchModel.Fields` gives it `Line = "3+ GOALS"`, so `NameOf` renders the NEED as **`3+ GOALS`** —
> and `T151` authored `MARGIN` / `APART` precisely to avoid that: *"the engine's bare `2 GOALS`
> collides with the total-goals family's own forms on the same column."* **The fallback is the exact
> string that ruling was written to prevent.**

---

## 3. THE THREE HELD — THE FALLBACK RE-CREATES `T156`, AND THE INTERIM HAS RUN OUT

**No copy may be authored for these.** `T152-am` withdrew their forms and Allen holds them.

**But the fallback is not a neutral holding state, and this is the finding:**

`MatchModel.Fields` gives a team total
`Line = "{FULL TEAM NAME} {OVER|UNDER} {n.n} {NOUN}"` — **the full name, not the noun.** So the
fallback NEED renders `San Francisco Spreadsheets UNDER 1.5 GOALS`.

1. **It cannot fit.** TV measured the SHORT-club form `SPREADSHEETS UNDER 4.5 CORNERS` at **449.5px
   against a 261.0px band**. The fallback starts from a strictly longer string. **Truncation is
   certain; where it lands is the only open number.**
2. **Truncation drops the market noun first** (`T155` — `FitToColumn` takes whole words from the
   end).
3. **So `TeamTotalGoals` and `TeamTotalCards` at the 1.5 line both render `{CLUB} UNDER 1.5`** —
   **`T156`'s collision, exactly, arriving through the fallback that was meant to fix the silence.**

**Stated plainly: these three are now where DoubleChance was before Allen ruled (b).** Their copy
cannot be repaired by shortening (`T152-am`), the fallback re-creates the collision that caused the
withdrawal, and the remaining moves are both Allen's — **author terse copy against the hold, or take
them out of the offered set.**

**Nothing in this spec resolves that. It is escalated, not answered.**

---

## 4. `T168` — THE TV NAMES CLUBS ITS OWN WAY, WHATEVER THE SOURCE

**RULED: every club name the TV renders passes through `SweatFlavor.Short` — the distinctive word,
city dropped.** That is `T69`'s shipped convention and the build states it at `LegStatement`'s
moneyline arm in those words.

`7dd5686` repaired `T130`'s silence by routing the row through `MarketSheet` (`S96`, §6.5). **The
source is right — `MarketSheet` is the single authority on what a bet is CALLED — and the club FORM
that came with it is not.** Frame B carries the proof on one screen: the row reads `DULUTH AUDITORS`,
the scorebug `AUDITORS`, the strip `Gravediggers`. **Three renderings, two conventions.**

**The fix is at the render, not at the source:** take the identity from `MarketSheet`, apply `Short`
to the club token. **No naming authority moves.**

> This reaches **every kind `7dd5686` re-routed**, not Handicap alone — and it applies to the §3
> fallback too, which is where the full name does the most damage.

---

## 5. WHAT THE LANE MEASURES

1. **`WinningMargin` rung 3**, both forms, over the pool.
2. **`TotalGoalsOddEven`'s `AT FT` rung**, confirming rather than inheriting.
3. **The §3 fallback, as it will actually render** — with `Short` applied per §4 and without —
   **and specifically what it truncates to at the 1.5 line for goals and for cards.** That number
   decides whether `T156` is live in the build today.

**Not asked for:** any measurement of the four kinds' already-measured rungs, or of copy for the
held three.
