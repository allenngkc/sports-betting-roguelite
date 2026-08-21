# SPEC — terse copy for the four blocked kinds

**Written:** Design Director seat, 2026-08-20, on **Allen's ruling**: take the terse-copy route for
the blocked kinds, hold the team totals.
**Authored here; THE LANE MEASURES.** No width below is asserted — `C58` (widths are measured in the
editor, never at this seat) and batch 95 (a width is measured, never read off a character count).

---

## 0. ONE RECONCILIATION, because the set moved after Allen ruled

Allen ruled on batch 151's disposition: **four kinds blocked on rungs** — `Handicap`,
`WinningMargin`, `PlayerMultiScorer`, `DoubleChance`.

**Batch 158's `T161` moved `DoubleChance` from *blocked* to *withdrawn*** on TV's measurement: its
NEED rung 1 clears **0** of 20 clubs and rung 2 clears **1**, and every truncation deletes the word
`DRAW` — so a double chance renders as a moneyline statement.

**The count does not change and neither does the instruction.** Terse copy IS re-authoring, which is
what a withdrawn form needs. **`DoubleChance` stays in this spec as the fourth target**; its job is a
new form rather than another rung. **The team totals stay held** — their four tokens are each doing
distinguishing work and shortening reaches `T156`'s collision, which is why they were withdrawn and
why Allen held them.

---

## 1. THE RULE THIS SPEC IS BUILT ON

**AUTHOR SO THE LAST TOKEN IS THE LEAST LOAD-BEARING.**

`FitToColumn` drops **whole words from the end** (`LastIndexOf(' ')`, `T155`). So the final token is
the first thing an overrun deletes. Every defect the measurements found is that rule being violated:

| form | truncates to | what was lost |
|---|---|---|
| `{CLUB} UNDER 4.5 CORNERS` | `{CLUB} UNDER 4.5` | the market (`T155`) |
| `{CLUB} TO WIN OR DRAW` | `{CLUB} WIN` | **the bet's terms** (`T161`) |
| `{SURNAME} TO SCORE 2+` | `{SURNAME} TO` | the verb and the quantity |
| `3+ GOALS APART AT FT` | `3+ GOALS APART AT` | nothing — it just breaks |

**This costs nothing to obey.** It is an ordering choice, made once, at authoring time.

---

## 2. THE THREE TESTS EVERY RUNG MUST PASS

Stated so the lane can reject a rung without a ruling, and so this seat can be checked.

1. **TRUNCATION-SAFE.** Dropping the final token must not **invert or narrow the bet's terms.**
   Losing the market's identity is bad (`T155`); **stating a requirement the player does not
   actually have is worse** (`T161`), because the surface then misreports his position.
2. **NON-COLLIDING.** The rung, **and every truncation of it**, must not equal a string another
   market on this surface can produce (`T156`'s test, run against `LegStatement`'s six shipped arms
   and against the other eight new kinds).
3. **THE STANDING CHECKS.** No engine root (`T87`); caps (§8); the distinctive word, not the generic
   one (`T69`); `NEED 0` unconstructible (`T108`); no term repeated across the NEED/progress pair
   (`T70`).

---

## 3. THE LADDERS, AUTHORED

Rungs are chosen **by measurement** (`FitOrFallback`), never by authoring intent. The lane measures
top-down and takes the first that fits, per club, over the closed 20-noun pool.

### 3.1 `PlayerMultiScorer` — one rung, and the cause is already measured

TV measured the cause: **the ` 2+` tail costs up to 36.4px** and takes the NEED from 1 raw overrun
to 6. It is the **literal**, not the surname pool — which is what batch 151 asked, so that a rung
would not be authored against the wrong half.

| | form |
|---|---|
| compact | `{SURNAME} 2+` *(shipped, clears)* |
| NEED rung 1 | `{SURNAME} TO SCORE 2+` *(as authored)* |
| **NEED rung 2 — NEW** | **`{SURNAME} 2+`** |
| progress | *(unchanged)* |

**Rung 2 is deliberately identical to the compact.** `LegStatement`'s own doc sanctions this —
*"where those two questions have the same answer, the two strings are IDENTICAL, and that is correct
rather than a duplication to design away."*

**REJECTED, and the reason is test 2 working:** `{SURNAME} SCORES 2+` is shorter than rung 1 and
reads well — **and it truncates to `{SURNAME} SCORES`, which is the shipped AnytimeScorer rung.**
A 2+ leg would render as an anytime leg. Rejected on collision, not on taste.

### 3.2 `WinningMargin` — a third rung, and it stays inside `T151`'s own vocabulary

Both authored rungs miss (380.8 and 283.2 against 261.0), so the floor renders
`3+ GOALS APART AT` — measured, not predicted.

| | form |
|---|---|
| compact | `MARGIN 2` · `MARGIN 3+` *(shipped, clears)* |
| NEED rung 1 | `2 GOALS APART AT FULL TIME` · `3+ GOALS APART AT FULL TIME` |
| NEED rung 2 | `2 GOALS APART AT FT` · `3+ GOALS APART AT FT` |
| **NEED rung 3 — NEW** | **`2 APART AT FT`** · **`3+ APART AT FT`** |

**`GOALS` is the token that goes**, because on a surface whose scorebug prints the scoreline two
slots away, *apart* is not ambiguous — and `T151` already chose `APART` as this market's
distinguishing word, having refused `DECIDED BY 2` for carrying an engine root (`T87`).

**Rung 3 truncates to `3+ APART`, which states the requirement and misstates nothing.**

> **`MARGIN` WAS CONSIDERED FOR RUNG 3 AND REFUSED**, and the refusal is recorded so it is not
> re-proposed: `MARGIN` is `MarketKind.WinningMargin`'s own root and `MarketSelection.WinningMargin`'s
> parameter name — and the laptop already ships **`YOUR MARGIN IS CLEAR`** (`SportsbookApp.cs:1738`),
> where the word means *winning comfortably*, not *the gap*. One word, two meanings, two surfaces.
> **`T151`'s compact keeps `MARGIN` and is not reopened** — a two-token identity line is a different
> job from a requirement line, and the compact is not near its ceiling.

### 3.3 `Handicap` — a third rung for the long half of the pool

Rung 2 rescues **short clubs only**: `MUSKRATS WITHIN 1` is the widest FITTING string in the whole
NEED band at 259.2px with **1.8px spare**, and longer clubs overrun even there.

| | form |
|---|---|
| compact | `{CLUB} -1.5` · `{CLUB} +1.5` *(shipped, clears)* |
| NEED rung 1 | `{CLUB} WITHIN 1 GOAL` · `{CLUB} TO WIN BY 2+` |
| NEED rung 2 | `{CLUB} WITHIN 1` · `{CLUB} BY 2+` |
| **NEED rung 3 — NEW** | **`{CLUB} +1.5`** · **`{CLUB} -1.5`** |
| progress | `TRAILING BY {n}` · `CLEAR BY {n}` *(unchanged)* |

**Rung 3 is the market's own notation and is identical to the compact** — same sanction as §3.1.
It is two tokens, so its only truncation is `{CLUB}`: an identity loss, never a misstatement.
**`±n.n` is used by no other kind on this surface**, so test 2 passes.

**The club cannot leave any rung.** `SweatFlavor.PickedHomeForPresentation` returns home
unconditionally for every non-moneyline kind, so nothing else on the row carries which side is
backed (`T152-am`, and `K17` on the console).

### 3.4 `DoubleChance` — RE-AUTHORED, not rung-extended

A ladder does not rescue it: rung 1 clears **0** of 20 and rung 2 clears **1**. That is not a form
needing another rung; it is the wrong form.

**The defect is structural: `{CLUB} TO WIN OR DRAW` puts `DRAW` LAST, and `DRAW` is the token that
distinguishes this market from the moneyline.** It is §1's rule violated in the worst possible way —
the most load-bearing word in the final position.

**THE NEW FORM — one word that IS the condition:**

| | form |
|---|---|
| **compact — NEW** | **`{CLUB} UNBEATEN`** *(replacing `{CLUB} OR DRAW`)* |
| **NEED rung 1 — NEW** | **`{CLUB} UNBEATEN AT FULL TIME`** |
| **NEED rung 2 — NEW** | **`{CLUB} UNBEATEN`** |
| the `12` variant, compact | `EITHER TEAM` *(unchanged — carries no club and clears)* |
| the `12` variant, NEED rung 1 | `A WINNER AT FULL TIME` *(unchanged)* |
| **the `12` variant, NEED rung 2 — NEW** | **`A WINNER AT FT`** |

**Why `UNBEATEN` and not a shorter `OR DRAW`:**

- **It is exactly the condition.** 1X is the home side unbeaten; X2 is the away side unbeaten.
  Nothing is approximated.
- **It is the sport's own word**, not bookmaker notation — `T69`'s distinctive-word convention, and
  the reason `X2` and `1X` are refused (`T86`(a) retired game-UI convention on this surface).
- **IT MAKES THE MISSTATEMENT UNREACHABLE, which is the whole point.** `{CLUB} UNBEATEN` is two
  tokens, so its only truncation is `{CLUB}`. **There is no intermediate form that says *win*.**
  Compare `{CLUB} TO WIN OR DRAW`, whose truncations pass through `{CLUB} WIN` — the string TV
  measured 19 of 20 clubs landing on.
- **No engine root** — `unbeaten` appears nowhere in `engine/` (checked). **No collision** — no
  shipped TV string contains it (checked).

---

## 4. WHAT THE LANE MEASURES

Per rung, over the closed pool, in the box it renders in — **compact statement 147.0px, NEED band
261.0px** — with `FitOrFallback` reached by reflection, as `323492d` did.

1. **How many of 20 clubs each new rung clears.** A rung that clears all 20 ends its ladder.
2. **Whether any rung's truncation is reachable at all.** If a rung fits for every club, truncation
   never fires and test 1 is moot for it — **that is the outcome to aim for.**
3. **Test 2, mechanically:** every rung and every truncation of it, compared against
   `LegStatement`'s six shipped arms and the other eight kinds' forms.

**If a rung still misses for the longest clubs, report it rather than authoring around it.** The
answer may be that the NEED band cannot hold a 12-character club plus a predicate — **which is a
finding for Allen's scope call and not something to fix by shortening further.**

---

## 5. HELD, AND NOT TOUCHED HERE

- **The three team totals** — withdrawn (`T152-am`), held by Allen. Not re-authored.
- **`CorrectScore` and `TotalGoalsOddEven`** — they clear every slot as authored (`T161`). Nothing
  owed.
- **`T155`'s build order** — `FitOrFallback` extended to `LegRowLine`. **This spec assumes it.**
  Every compact rung above is chosen by measurement, which the compact slot cannot do today.

---

## 6. OWED BEFORE THIS SHIPS

1. **`T155`'s compact ladder must exist**, or the compact rungs in §3.1 and §3.3 are unreachable and
   those slots stay on the truncation floor.
2. **The measurements in §4.**
3. **`C46` against the enumerated pool** for every form authored here, in both boxes.
4. **A frame is NOT owed for the copy itself** — these are string rulings testable by measurement.
   A frame is owed only if a rung's rendering is in question, which no measurement can answer.
