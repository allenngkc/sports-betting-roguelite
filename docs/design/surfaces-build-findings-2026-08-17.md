# FINDINGS — the market surfaces build, routed to the Design Director

**Written:** markets-pregame lane, 2026-08-17 · **Routed by:** Allen, 2026-08-17
**Building to:** `docs/design/spec-market-surfaces-2026-08-17.md` (S89–S92, batch 107)

Five findings from building the spec. **Four need a DD ruling; one is recorded as resolved
without one.** Everything quoted below is verbatim from the spec or measured from the build —
nothing here is a paraphrase, and where a number is measured the method is given.

**Nothing here blocks the build, and nothing here is waiting on you to proceed.** The rail
measurement §9 asked for has LANDED (§2a): the six destinations fit, so no ruling is needed to
ship them. The rulings sought are about what the measurements MEAN — the rail's exhausted
headroom, a contradiction the build had to resolve by choosing, and a vocabulary the lane will not
change by hand.

---

## 1. The spec contradicts itself on RAIL ORDER — needs a ruling

**§3's table** seats CORRECT SCORE **third**:

```
| destination | holds | ~rows |
|---|---|---|
| **RESULT** | Moneyline · DoubleChance · Handicap · WinningMargin | 18–20 |
| **GOALS** | TotalGoals · TeamTotalGoals · TotalGoalsOddEven · BothTeamsToScore | 16–18 |
| **CORRECT SCORE** | CorrectScore | 12–16 |
| **CORNERS** | TotalCorners · TeamTotalCorners | 10–12 |
| **CARDS** | TotalCards · TeamTotalCards | 10–12 |
| **PLAYERS** | AnytimeScorer · PlayerMultiScorer | ~15+ |
```

**§5.2's contents example** seats it **fifth** — its printed line ranges only work in the order
RESULT, GOALS, CORNERS, CARDS, CORRECT SCORE, PLAYERS:

```
GOALS ....................... 12–29
    TOTAL GOALS ............. 12–17
    TEAM TOTALS ............. 18–25
    ODD / EVEN .............. 26–27
    BOTH TEAMS TO SCORE ..... 28–29
CORNERS ..................... 30–41
CARDS ....................... 42–53
CORRECT SCORE ............... 54–69
PLAYERS ..................... 70–85
RESULT ...................... 1–11
```

**Built as:** §3's table, on the ground that a normative table outranks an illustration.
`MarketDestinations.All` is authored in that order and its order is pinned by a test.

**Why it is worth your ruling anyway.** §5.2's order is not obviously careless — it groups the
three countable statistics (GOALS, CORNERS, CARDS) adjacently and puts the abstract bet type
after them, which is the same statistic-vs-bet-type seam §3 draws to justify the taxonomy. If
that was the intent, §3's table is the stale half.

**Cost of changing it:** low and mechanical — one array's order. It shifts every printed line
range and every folio number, but all of those are derived, so nothing is hand-edited.

---

## 2. The spec's ~996px is wrong by the width of the betslip — and the rail's true capacity is SIX

**§4.3, verbatim:**

> A ~996px row with a name at one end and a price at the other **needs a device or it is just
> emptiness.**

**§9, verbatim:**

> - **The rail's ~9 capacity is arithmetic on authored constants and has NEVER been measured.** §3
>   fits six destinations into it with headroom, so **the spec does not depend on the number** —
>   but nobody should quote it as measured. **Measure it during the build.**

**Measured.** ENTRY is a two-column screen. The market column is authored `700f`
(`SportsbookApp.cs:476`, `:485`); the betslip `WorkingMargin` is `324f` anchored right
(`SportsbookApp.cs:991`); 700 + 324 = the 1024px canvas. **The 996 figure is 1024 minus 2×14px
padding — it counts the betslip's column as available to the market sheet.** It is not.

Two consequences, of very different weight:

- **§4.3's ruling survives its wrong premise.** At the true width the row is 700px (692 with the
  position rail), and the price cell is 176px + 14px pad, so the annotation gap is still ~490px.
  A gap that large still needs the device. **Leader dots are built; no ruling needed.**
- **§3's claimed headroom does not survive.** "Six destinations for fifteen kinds, against a rail
  that holds about nine" was `996 ÷ ~104px`. Against 700px the same arithmetic gives ~6.7, and
  `CORRECT SCORE` is materially wider than any label the rail carried — the strip this replaces
  only ever sized a tab 96f or 108f. Measured in §2a: it is the widest label on the rail by 59px.

### 2a. MEASURED RAIL WIDTHS — the six FIT, with zero headroom

**No ruling is needed to ship six. One is needed before a seventh is ever proposed.**

Measured on Archivo Regular at 13px with tracking `.14em` (`MakeButton`'s default, unchanged),
replicating `LaptopUi.MeasureWidth`'s formula against `Archivo.ttf`'s `hmtx` table. Padding is
`RailTabPadX = 12f` per side — the one authored number; every x below is derived from the measured
label and gated by `SportsbookApp.RequireDestinationRailFits`, which throws carrying every width.

| destination | label width | box (label + 12px/side) | x |
|---|---|---|---|
| RESULT | 62.52 | 86.52 | 14.00 |
| GOALS | 54.89 | 78.89 | 108.52 |
| **CORRECT SCORE** | **136.86** | **160.86** | 195.40 |
| CORNERS | 77.84 | 101.84 | 364.27 |
| CARDS | 55.15 | 79.15 | 474.11 |
| PLAYERS | 73.61 | 97.61 | 561.26 |

Labels 460.86 · boxes 604.87 · +5×8px gutter +2×14px margin = **672.86 of 700. Slack: 27.14px.**

**Finding: the real capacity is exactly SIX. §3's "the headroom is deliberate" is false at 700px.**
The 27.14px cannot hold a seventh destination — `MakeButton` floors a control at 44px, plus an 8px
gutter is 52px minimum. Even dropping the rail to `LaptopTrack.Tabs` (.11em) recovers only
16.77px, reaching 43.91px slack — still short of 52. **A seventh destination is a design problem,
not a build one, and it arrives the moment a sixteenth market kind needs a home that is not one of
these six.** §3 wrote the headroom argument to protect against exactly that case; the protection
does not exist.

**And it was a near miss.** The ceiling for symmetric padding is **14.26px/side**. The strip this
replaces ran its own tightest box at **15.08px/side** (CORNERS: a 108f box around 77.84px of type).
Carrying that grammar forward would have overflowed by ~9.8px — the six fit because the packing is
derived from measured labels, not because the old numbers had room in them.

**Nothing was shrunk, truncated, abbreviated, wrapped or scrolled to make this fit.** The levers
that would have been needed are all yours, and the spec closes three of them itself: type does not
shrink and the 13px floor is law (§4.5), a second rail tier is forbidden (§5.2), and a reflowing
rail would cost §3.1's constancy.

**One open design call, which does not change the verdict.** The rail keeps
`LaptopTrack.Actions` (.14em), matching the strip it replaces. `MakeTab`'s own comment — *a tab is
a place, not an act* — argues the rail should take `LaptopTrack.Tabs` (.11em). That is your call;
it narrows the pack by 16.77px and the six fit either way.

---

## 3. Row-name CASING is inconsistent in the engine's own fields — needs a ruling

The spec puts every offer in **one full-width column** (§4.1, `S92`). That column will mix casing,
because `MatchModel.Fields` does. Exact strings, measured from generated slates:

| kind | printed row name, verbatim |
|---|---|
| TotalGoals | `OVER 2.5 GOALS` |
| TeamTotalGoals | `Waterloo Notaries OVER 0.5 GOALS` |
| Handicap | `Fresno Gravediggers +1.5` |
| AnytimeScorer | `DARRYL LEDGER ANYTIME` |

**This is pre-existing and it is ruled.** `A2` fixes the row label as the engine's own DD-verbatim
string, and the shipped `BuildMarketLines` prints `MatchModel.Fields(...).Line` with no re-casing
(`SportsbookApp.cs:537-538`). **The lane has not normalised it and will not**, because doing so
would override a standing ruling by hand.

**Why it surfaces only now.** The mixed-case kinds are exactly the team-scoped and handicap
markets — nine of the fifteen were homeless (`S86`), so no surface has ever printed them beside
the uppercase ones. The inconsistency is not new; its visibility is.

**The ruling needed:** whether the sheet uppercases row names, whether the engine's fields are
re-cased at source, or whether the mix stands. Note that the surface's own voice is uppercase
throughout (`GOALS`, `CORNERS`, `MONEYLINE`), which is the argument for the first.

---

## 4. `no prices offered` is UNREACHABLE at the shipped config — affects §8's evidence

**§5.3 / `S89`, verbatim:**

> **A group with offers prints its count.** A group with none prints `no prices offered`. **A
> racecard prints the race even when it is abandoned.**

**Measured: zero empty groups across 18,000 matchups** at the shipped `RunConfig`. Floors
measured: CORRECT SCORE never drops below 11 rows, MULTI SCORER never below 3, and the other
thirteen kinds cannot be empty structurally. The only dial that reaches the state is
`CorrectScoreFloor` (default `0.02`):

| `CorrectScoreFloor` | CORRECT SCORE empty | MULTI SCORER empty |
|---|---|---|
| 0.02–0.03 (shipped) | 0/240 | 0/240 |
| 0.05 | 0/240 | 25/240 |
| 0.08 | 0/240 | 189/240 |
| 0.15 | 239/240 | 240/240 |

**`S89` is not undermined.** §3.1's argument — the rail is constant *because* empty groups print —
holds regardless of whether the state occurs today, and the form is what keeps a future config or
a removed market from reflowing the rail.

**But §8's evidence item 3 cannot be shot honestly on a shipped config.** The frame will be
captured at `CorrectScoreFloor = 0.08` (witness: `new Run("SHEET-EMPTY", new RunConfig {
CorrectScoreFloor = 0.08 })`, matchup 0, 69 rows, MULTI SCORER empty). **That frame will state the
non-default config on its face.** A frame that shows a state the shipped game cannot reach, without
saying so, misrepresents the build — `C11` is about rendered evidence, and evidence that needs a
caption must carry it. Flagging so the DD accepts the frame on those terms or asks for something
else.

---

## 5. The `TEAM TOTALS` label is asymmetric — small, needs a ruling

§5.2's contents example names `TeamTotalGoals` as `TEAM TOTALS`, while the corner and card
equivalents carry their statistic (`TEAM TOTAL CORNERS`, `TEAM TOTAL CARDS`). Inside a destination
the short form is unambiguous — a `TEAM TOTALS` row under CORNERS can only mean corners — so the
short form arguably belongs to all three, or the long form to all three.

**Built as:** spec-literal (`TEAM TOTALS` for goals, long form for the other two). The lane did
not harmonise it because the spec states it explicitly and it is the DD's vocabulary.

---

## 6. RESOLVED WITHOUT A RULING — recorded so it is not re-discovered

**BTTS printed two identical rows at two different prices.** Both offers emit
`Fields.Line = "BOTH TEAMS TO SCORE"` with an empty `Subject`, so a `Line`-else-`Subject`
composition printed the same sentence twice and never said YES or NO — the player cannot bet the
offer they mean. Caught by a no-two-rows-share-a-name gate on 100% of 3,000 measured matchups.

**No new call was needed:** the choice lives in `Fields.Market` (`BTTS — YES` / `BTTS — NO`), and
`SportsbookApp.BuildBothTeamsScore` already prints exactly that on the shipped surface under `A2`,
for the same stated reason. The sheet now matches it, and the exception is pinned narrowly — every
other kind still obeys `Line`-else-`Subject`.

---

## 7. Measurements the spec asked for (§9) — for the record

`§9` states the row counts are estimates and the ~80 is the mandate's figure. Measured over 18,000
matchups:

| destination | §3 estimate | measured min | max |
|---|---|---|---|
| RESULT | 18–20 | **13** | 13 |
| GOALS | 16–18 | **18** | 18 |
| CORRECT SCORE | 12–16 | **11** | 16 |
| CORNERS | 10–12 | **10** | 10 |
| CARDS | 10–12 | **10** | 10 |
| PLAYERS | ~15+ | **17** | 24 |

**Offers per matchup: 79–90, mean 84.78.** The mandate's ~80 is the floor, not the centre — the
folio's denominator reads up to 90, not 80.

RESULT is over-estimated by 5–7. Four of the six destinations are **structurally fixed height**
(RESULT 13, GOALS 18, CORNERS 10, CARDS 10 — set by `RunConfig`'s line arrays and never varying);
only CORRECT SCORE and PLAYERS move with the matchup. Offered because §5.4's scrolling argument and
any future rail thinking both depend on which destinations can grow.
