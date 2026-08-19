# S96's `C46` BINDING, DISCHARGED — the measurement, and what it costs §4.3

**Measured:** markets-pregame lane, 2026-08-18 · **Ruling:** `S96` (DD batch 113) — *the sheet
uppercases row names at the presentation layer* · **Binding:** `C46` — *uppercase is WIDER per
character, and `MOOSE JAW OVERHEADS OR DRAW` is the longest reachable form — MEASURE IT against the
row before it ships.*

**S96 is applied in the build and its gate is FAILING, deliberately.** The branch is not
merge-ready and should not be until this is ruled. Nothing here is fixed: truncation, widening the
cell and shortening the pool are all DD calls, and the spec already closes shrinking (§4.5, the 13px
floor is law).

**`S84`'s binding is separately DISCHARGED and clean** — see §4.

---

## 1. THE HEADLINE IS NOT THE COLLISION — IT IS THE LEADER DOTS

The collision count understates the damage, so it is not the number to rule on.

§4.3 ruled *the offer row is ONE statement, not two facts at opposite ends — leader dots carry the
name to its price.* A name that stops a few pixels short of the price cell does not collide, and
prints **no leaders at all**. It silently loses the ruled device.

Measured over **all 4,236 distinct reachable row names**, on the **692px scrolling row** (the real
case — every ENTRY destination overflows at the shipped config), name cell **480.00px**:

| leftover space | UPPERCASE (`S96`) | TITLE CASE (today) |
|---|---|---|
| `< 0` — name exceeds its cell | **5** (0.1%) | **0** |
| `0 – 18.50` — no dot fits | 7 | 0 |
| `18.50 – 39` — 1–4 dots | 65 | 0 |
| `39 – 78` — 4–10 dots | 375 | 25 |
| `> 78` — 10–67 dots | 3,784 | 4,211 |
| **rows printing ZERO leader dots** | **17** (0.4%) | **0** |
| **rows printing FEWER THAN SIX** | **139** (3.3%) | **2** (0.05%) |

**The number to rule on is 2 → 139, a ~70× increase in rows whose leader run is too short to read as
one.** Zero-dot rows go 0 → 17.

Threshold stated so it can be disagreed with: **six dots = a 39.0px run** (the leader step is
**6.500px** exactly — roman `.` at 13px with `LeaderTracking` .2). Below that the run is under 6% of
a 692px row and reads as debris between two facts rather than as a rule carrying one to the other.
Leaders sit inside two 10px clearances and the name cell already excludes the 8px annotation gap, so
**a dot needs 18.50px of leftover, not 6.50px.**

---

## 2. WHAT ACTUALLY HAPPENS TO THE FIVE — it is not an overlap

**Correcting this lane's own first report**, which called it a collision with the price:

`LaptopUi.MakeText` leaves `enableWordWrapping = true` and `overflowMode = Truncate` on the NAME
label — only the role and the leaders disable wrapping. An over-long name therefore **wraps to a
second line inside a 54px row and truncates.** It does not run into the price cell.

**The visible defect is a two-line, truncated name printing zero leaders** — worse for §4.3 than an
overlap would be, because it is quiet. Nothing crashes: `MakeLeaders` guards three times
(`span <= 0`, `span < unit`, `count <= 0`) and simply draws nothing.

---

## 3. THE MEASUREMENT, AND WHY THE SPOT-CHECK WOULD HAVE PASSED

**Longest reachable uppercase name: `SAN FRANCISCO SPREADSHEETS UNDER 4.5 CORNERS` — 493.68px**
against a 480.00px cell, over by **13.68px**. Tied with `…GRAVEDIGGERS…`.

**`C46`'s named candidate was never the worst case.** `MOOSE JAW OVERHEADS OR DRAW` measures
**318.06px with 161.94px spare** — it clears comfortably. **The worst kind is
`{CLUB} UNDER 4.5 CORNERS`, not `{CLUB} OR DRAW`,** so checking the named example would have passed
and shipped the defect. This is the binding earning itself: *the pool, not the sample.*

**The ruling is cleanly isolated as the cause.** Title case does not collide at all — 0 of 3,840
club forms, the same worst row at 435.75px with 44.25px spare, and its worst row still draws four
dots. **S96 adds +57.93px (+13.3%) and moves 452 rows out of the comfortable band in one step.**

**Only three market kinds are affected, all club-prefixed team totals:**

| zero-dot rows | kind | representative | leftover |
|---|---|---|---|
| 11 | `TeamTotalCorners` | `SAN FRANCISCO SPREADSHEETS UNDER 4.5 CORNERS` | −13.68 |
| 4 | `TeamTotalGoals` | `SAN FRANCISCO SPREADSHEETS UNDER 0.5 GOALS` | +13.57 |
| 2 | `TeamTotalCards` | `SAN FRANCISCO SPREADSHEETS UNDER 1.5 CARDS` | +12.73 |

Under-six band: `TeamTotalCorners` 81 · `TeamTotalGoals` 38 · `TeamTotalCards` 20. **No other kind
is affected at all.**

**Scorer rows are untouched** — 288 names, minimum 18 dots even against the widest role
(`MIDFIELDER`, 79.35px), because `MatchModel.Fields` already uppercases player names. **S96 changes
nothing on the PLAYERS sheet**, which is worth knowing before anyone reads a scorer frame for it.

---

## 4. `S84`'s BINDING — DISCHARGED, POOL CLEAN

Checked as a **pool, not a sample**, per the binding: **320 clubs** (16 cities × 20 nouns) and
**144 players** (12 first × 12 last), enumerated by reflection off the engine's own arrays rather
than by generating slates until the set stopped growing.

**Zero case-dependent names.** No lowercase particle, no apostrophe form, no internal capital; every
name is a pure length-preserving case fold. **Nothing returns to the DD on this binding.** The test
fails by name if the engine renames those arrays, so the check does not silently lapse.

---

## 5. WHAT THE LANE WILL NOT DECIDE

The levers are the DD's, and the spec closes one of them already:

- **Accept it** — 17 rows print a wrapped, truncated name with no leaders; 139 print a run under six
  dots. Confined to three team-total kinds.
- **Widen the name cell** by narrowing the 176px price cell. **This is measurable and the lane has
  not measured it** — the longest price on the sheet is a four-digit form (`+3286` was on the docked
  PLAYERS frame), so the cell may carry slack. Say the word and this is a short measurement, not a
  build.
- **Shorten the pool** — `SAN FRANCISCO SPREADSHEETS` is the worst of 320, and city and noun lists
  are enumerated constants.
- **Shorten the market suffix** on team totals (`UNDER 4.5 CORNERS`) — a vocabulary change and
  therefore `A2`/`S22` territory, since it changes the engine's WORDS rather than their typography.
- **Shrinking type is already closed** (§4.5, the 13px floor is law).

**Method note.** Widths are replicated offline from `ArchivoNarrow.ttf`'s `hmtx` (no editor lease —
TV holds it), validated to 2dp against three figures already recorded from real in-Unity runs: the
rail pack `672.86/700`, the `CORNERS` rail label at `77.84px`, and the leader step. **The shipped
gate calls the real `LaptopUi.MeasureWidth`**, so it will re-measure in-engine the moment the suite
next runs.
