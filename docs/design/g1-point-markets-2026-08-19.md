# `G1` — the three markets that break the grammar, authored

**Written:** Design Director seat, 2026-08-19 · **Batch 137**
**Discharges:** part of `G1` / `G1-am2` — three of the nine kinds `T130-vf` found unauthored.
**Binds:** `T70`, `T108`, `S22`, `T87`, `§8`, and `G1`'s own two-rung ladder.

**Widths are NOT measured here and cannot be.** `C58` rules the offline method invalid for a face
whose default instance is not the shipped one, and `ttf_faces.py` records Encode Sans as *"wrong on
BOTH axes."* **`T111`'s pattern applies: this seat authors, the lane measures, this seat rules on
the numbers.**

---

## 1. WHY THESE THREE BREAK IT — and the property is the engine's, not a matter of taste

The existing progress grammar is a **count against a limit** — `0 GOALS • LIMIT 1`, `4 CORNERS •
NEED 5` — or a **one-way flag**: `SCORED` / `NOT YET`.

**Both assume the quantity only moves one way. For these three it does not.**

| kind | its quantity | monotone? |
|---|---|---|
| TotalGoals · TotalCorners · TotalCards · TeamTotals · AnytimeScorer · PlayerMultiScorer | a count | **yes** — it only rises |
| Moneyline · DoubleChance · Handicap | the scoreline | effectively — resolved at the whistle |
| **CorrectScore** | the exact scoreline | **no — you can be ON it and drift off** |
| **TotalGoalsOddEven** | the parity | **no — every goal flips it** |
| **WinningMargin** | the margin | **no — a margin can SHRINK** |

**THE CLASSIFICATION: a monotone quantity gets a count-against-a-limit progress line. A
NON-MONOTONE requirement gets a satisfied/not-satisfied pair.** That is why exactly these three
broke the grammar and nothing else did — and it is read off the engine's own quantities rather than
chosen.

**One consequence worth stating because it is the player-facing point:** on these three the
requirement can be **currently satisfied and completely unsettled** — a correct-score backer at 3–1
in the 60th minute is exactly right and entirely unsafe. **No existing progress form can express
that**, which is the gap `T130-vf` photographed as a blank column.

---

## 2. THE PAIR — one new state pair covers all three

**`MET` · `NOT YET`.**

- **`NOT YET` is not new.** It is AnytimeScorer's existing progress word and it means the same thing
  here: the requirement is not currently satisfied.
- **`MET` is the only new word in this pass.** It reports that the requirement is satisfied **now**,
  and says nothing about whether it will hold — which is exactly the fact and exactly the limit of
  the fact.

**`HOLDING` was authored first and withdrawn, and the reason is recorded so it is not re-proposed:**
it collides at the root with **`HOLD E`**, the cash-out status word (`T88`), which can be on screen
in the same frame. **That is `T133`'s `PAID`/`PAY $60` shape** and this seat is not going to author
into it two batches after ruling on it. **`MET` also reads better against `§8`** — *the theatre
reports, it does not editorialise* — because `HOLDING` implies precariousness where `MET` states the
fact and leaves the temperature to the player.

---

## 3. THE FORMS

| kind | compact (identity) | NEED (while live) | NEED fallback | progress |
|---|---|---|---|---|
| **CorrectScore** | `EXACT 3-1` | `3-1 AT FULL TIME` | `3-1 AT FT` | `MET` / `NOT YET` |
| **TotalGoalsOddEven** | `TOTAL ODD` · `TOTAL EVEN` | `ODD TOTAL AT FULL TIME` · `EVEN TOTAL AT FULL TIME` | `ODD TOTAL AT FT` · `EVEN TOTAL AT FT` | `MET` / `NOT YET` |
| **WinningMargin** | `MARGIN 2` · `MARGIN 3+` | `2 GOALS APART AT FULL TIME` · `3+ GOALS APART AT FULL TIME` | `2 GOALS APART AT FT` · `3+ GOALS APART AT FT` | `MET` / `NOT YET` |

### 3.1 Why each compact is what it is

- **`EXACT 3-1`, not `SCORE 3-1`.** `SCORE` is the market's own noun and is unusable on a surface
  whose scorebug prints the score two slots away — it would read as the live scoreline. **`EXACT`
  names the market's defining property** and cannot be misread. `T69`'s convention: the distinctive
  word, not the generic one.
- **`TOTAL ODD`, not `ODD GOALS`.** *Odd goals* reads as *unusual goals*. The market's structure is
  total → parity, and the compact follows it.
- **`MARGIN 2`, not `2 GOALS`.** The engine's bare `2 GOALS` collides with the total-goals family's
  own forms on the same column. `MARGIN` is the distinguishing word and it is the market's own.

### 3.2 The NEED forms take the existing terminal shape

`AT FULL TIME` is established grammar for a requirement settled at the whistle — the draw's
`LEVEL AT FULL TIME`. All three of these are terminal in the same way. **The fallback rung shortens
`FULL TIME` → `FT`, which is the ladder the draw already uses** (`LEVEL AT FT`). Nothing new.

**`WINNING MARGIN` is team-agnostic in the engine** — `Fields` returns no subject — so the NEED says
`APART`, never *"win by"*. A form naming a team would assert something the market does not.

---

## 4. THE CHECKS, RUN

**`T70` — requirement above, state below, NO TERM REPEATED ACROSS THE PAIR.** Run on all three:

| pair | shared terms |
|---|---|
| `3-1 AT FULL TIME` / `MET` · `NOT YET` | **none** |
| `ODD TOTAL AT FULL TIME` / `MET` · `NOT YET` | **none** |
| `2 GOALS APART AT FULL TIME` / `MET` · `NOT YET` | **none** |

**This is the check `T126` found had never been run on the draw** (`LEVEL AT FULL TIME` over
`LEVEL`). It is run here before authoring rather than after shipping.

**`T108` — `NEED 0` must be UNCONSTRUCTIBLE.** None of the three can produce a zero requirement:
`WinningMargin`'s buckets are 1 / 2 / 3+, `OddEven` carries no number, and `CorrectScore`'s `0-0` is
a legitimate scoreline rather than a degenerate need.

**`T87` — `decisive` is an engine term and never prints.** `DECIDED BY 2` was considered for the
margin NEED and refused on that ground; `APART` carries no engine root.

**`§8` casing** — every authored line in this family is caps, as the surface's existing forms are.

---

## 5. OWED BEFORE THIS SHIPS — measurement, and it is a gate

1. **`C46` against the ENUMERATED pool** (`S84`, `S96-am`) for every form above, in the leg row's
   249.0px compact box and the NEED band. **The widest form authored here is
   `3+ GOALS APART AT FULL TIME` at 27 characters** — for comparison the shipped
   `{CLUB} TO WIN` reaches 33 at `San Francisco Spreadsheets`, so these are **plausibly inside the
   existing worst case.** **That is an observation and NOT a measurement** — batch 95's rule is that
   the widest string in a column is measured, never read off a character count, and this seat has
   been corrected on it.
2. **The two-rung ladder is selected BY MEASUREMENT** (`FitOrFallback`), never by authoring intent.
3. **`C58`: the measurement runs in the editor.** The offline `hmtx` route is invalid for this face.

---

## 6. NOT DONE HERE

- **The other six kinds** — DoubleChance, Handicap, and the three team-totals extend the existing
  grammar without breaking it; PlayerMultiScorer is AnytimeScorer's pair with a threshold. **They are
  the next pass and they are cheaper by construction.**
- **No width is claimed** (§5).
- **`T126`'s draw pair is not re-authored here.** It is the same family and the same defect, but it
  is a ruled item with its own row and it wants its own pass rather than riding this one.
