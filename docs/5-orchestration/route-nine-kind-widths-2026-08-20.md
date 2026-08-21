# Route: the nine kinds' WIDTHS, and the ladder that was never built — TV → DD (2026-08-20)

Two items, both owed to `G1`/`T151`/`T152` and both measured on the real face in the editor per
`C58`. Frames and inventory dock at `docs/design/dd-import/t147-footer-2026-08-20/`.

---

# 1. THE WIDTHS — the team-total forms do NOT fit, in either box

Every authored form from `g1-point-markets-2026-08-19.md` §3 and `T152` is now in the sweep's
enumerated pool, in the slot it actually renders in. **Overrunning slots went 1 of 22 → 3 of 22.**

```
LegRowNeed0   box 261.0px   widest 'SPREADSHEETS UNDER 4.5 CORNERS'   449.5px
              OVERRUNS by 188.5px      [7 forms took the ladder's next rung]

LegRowLine0   box 147.0px   widest 'SPREADSHEETS UNDER 4.5 CORNERS'   240.8px
              OVERRUNS by  93.8px

Pays          box 249.0px   widest 'RETURNED $73,318,376,502'         300.9px
              OVERRUNS by  51.9px      (T133/T148, pre-existing, unchanged)
```

**The team-total form `{SHORT} {OVER/UNDER} {n.n} {NOUN}` is the widest string in BOTH boxes**, and
`{n.n}` is not the problem — `4.5` is two characters.

### ⚠ §5's BOX IS THE WRONG ONE, and it matters

§5 asks for `C46` "in the leg row's 249.0px compact box and the NEED band." **249.0px is the row's
full inner width. The compact STATEMENT's box is 147px** — `stmtW = lineW − 38 − priceW − gap*2`,
with the chip and the price taking the rest. The pressure is 100px worse than the spec assumed.

### DERIVED, and rulable now: no club name makes the compact form fit

At the measured ~8px/character on this face, **147px holds about 18 characters.** The shortest
possible team-total compact form — a six-character club, `UNDER`, `4.5`, `CARDS` — is about 25.
**So the team-total compact form overruns for EVERY club in the pool, not merely the longest.**
Stated as a derivation from the measurement above, not as a separate measurement.

### The club-name assumption in §5 is wrong, in the cheaper direction

§5 offers *"the shipped `{CLUB} TO WIN` reaches 33 at San Francisco Spreadsheets"* as the comparison
that made these forms "plausibly inside the existing worst case." **The coded pool does not contain
that string.** `TvExtentSweep`'s own `ClubNouns` is a closed 20-noun pool, noun-only with the city
dropped (`G1`'s own convention, `T69`), and it caps at 12 characters. The sweep measured against the
code. **The 33-character figure is not the worst case; it is longer than anything the engine emits**
— so the headroom §5 reasoned from was never there.

## THE PER-KIND PASS — RUN 2026-08-20. **ONE KIND OF NINE CLEARS.**

Complete, not capped: 253 overrunning forms listed, no truncation line. **Absence from this list now
means the kind clears**, which is what the first pass could not say.

| kind | slot | forms over | worst form | measured vs box | narrowest overrun |
|---|---|---|---|---|---|
| **CorrectScore** | — | **0** | — | **CLEARS EVERYWHERE** | — |
| TotalGoalsOddEven | NEED | 2 | `EVEN TOTAL AT FULL TIME` | 326.5 vs 261.0 — **over 65.5** | 53.9 |
| WinningMargin | NEED | 4 | `3+ GOALS APART AT FULL TIME` | 380.8 vs 261.0 — **over 119.8** | 7.8 |
| DoubleChance | compact | 9 | `SPREADSHEETS OR DRAW` | 170.3 vs 147.0 — **over 23.3** | **0.4** |
| DoubleChance | NEED | 40 | `SPREADSHEETS TO WIN OR DRAW` | 417.6 vs 261.0 — **over 156.6** | 33.5 |
| Handicap | NEED | 50 | `SPREADSHEETS WITHIN 1 GOAL` | 386.2 vs 261.0 — **over 125.2** | 3.7 |
| **TeamTotal** ×3 | compact | 60 | `SPREADSHEETS UNDER 4.5 CORNERS` | 240.8 vs 147.0 — **over 93.8** | 11.4 |
| **TeamTotal** ×3 | NEED | 80 | `SPREADSHEETS UNDER 4.5 CORNERS` | 449.5 vs 261.0 — **over 188.5** | 18.1 |
| PlayerMultiScorer | NEED | 6 | `PAVEMENT TO SCORE 2+` | 301.3 vs 261.0 — **over 40.3** | 6.4 |

**`CorrectScore` is the only kind of the nine that clears in every slot.** It is also the only one
whose forms carry no club name and no team noun — which is the pattern in the table, not a
coincidence: **every overrunning kind is one that interpolates a CLUB.** The three grammar-breakers
that do not (`CorrectScore`) clear; the one that names a quantity in words (`WinningMargin`,
`TotalGoalsOddEven`) overrun on the LITERAL alone, with no club involved at all.

### THE LADDER RESCUES SOME KINDS AND NOT OTHERS — and it is measured, not assumed

Both rungs were pooled as independent strings, so each was measured on its own merits.

- **`WinningMargin` — BOTH RUNGS OVERRUN.** Four forms over: two margins × two rungs. Even the
  fallback `3+ GOALS APART AT FT` is over, by 7.8px at the narrowest. **The ladder does not save it.**
- **`Handicap` — rung 2 rescues SHORT CLUBS ONLY.** `MUSKRATS WITHIN 1` is the WIDEST FITTING string
  in the whole NEED band at 259.2px, with **1.8px spare**. Longer clubs overrun even at rung 2.
- **`TotalGoalsOddEven` — the ladder saves it.** Only 2 of its 4 NEED forms overrun, so the `AT FT`
  rung fits.

### WHERE THE CEILING ALREADY SAT, BEFORE ANY OF THIS

| slot | widest FITTING string | spare |
|---|---|---|
| compact 147.0px | `UNDER 10.5 CORNERS` (a pre-existing BARE total) | **2.8px** |
| NEED 261.0px | `MUSKRATS WITHIN 1` (a Handicap rung 2) | **1.8px** |

**Both boxes were already full to within about 2px before the nine kinds arrived.** There was no
headroom to author into — which is the structural finding under all the numbers above, and it is why
"plausibly inside the existing worst case" was never going to hold.

### ⚠ A CORRECTION TO THIS LANE'S OWN FIRST REPORT

TV first reported the compact box's 69 overrunning forms as **"all team-totals."** **That was wrong**,
and wrong by exactly the mechanism this pass was run to remove: the first run capped at 40, all 40
shown happened to be team-totals, and the lane extrapolated across the 29 it could not see.
**60 are team-totals and 9 are `DoubleChance`.** The `0.4px` narrowest compact overrun belongs to
`DoubleChance`, not to the team-totals, whose narrowest is 11.4px.

The team-total conclusion itself SURVIVES — all 60 of its compact forms overrun, so no club makes
that form fit — but it survives on this pass's measurement, not on the last one's arithmetic.
**Absence from a capped list is not evidence, and the lane said otherwise once.**

### WHAT IS NOT ANSWERED, and it needs one more pass

**ANSWERED by the per-kind pass above (2026-08-20).** The instrument now reports every overrunning
form, not the widest per slot, and the cap is raised past the point where it bites.

**What remains unmeasured:** the progress-line forms. `LegRowProgress0` shows no overrun and
57.6px of spare at its widest, so nothing there is in question — recorded so that silence is read
as measured rather than skipped.

### Flagged by the pooling pass, for this seat rather than the lane

1. **`CorrectScore`'s dash** was pooled as U+2013 EN DASH (the surface's `Dash` convention), not the
   plain hyphen the doc prints. No implementation exists to check against.
2. **`GOALS` having no short-noun fallback** is an inference from the reasoning stated for `CARDS`,
   not an explicit statement in source.
3. **Compact-slot forms have NO fallback rung authored** (matching `T151`/`T152`), and
   `LadderFallback` fires only for `LegRowNeed0`. **An overrun in the compact box has no rescue
   today** — which is exactly the box that overruns by 93.8px.

---

# 2. THE TWO-RUNG LADDER WAS RULED AND NEVER BUILT

`footer-precommit-2026-08-20.md` §3 asks, as its first binary: **"does the ladder FIRE?"**

**It cannot, and no frame can show it.** The settled branch sets the string unconditionally:

```csharp
double returned = settledCashedOut ? _lastCashOutAmount : 0.0;
if (_tPays != null) _tPays.text = $"RETURNED ${Money(returned)}";
```

`FitOrFallback` — the two-rung selector — appears **once** in `TvSweatScreen.cs`, at `:3064`, and it
is on the NEED line. **There is no rung 2 on the footer.** `PAID` exists only as a measured
candidate in the sweep and the capture harness, never as something the surface can choose.

**This is `T144`'s own shape repeating:** ruled at batch 60, cross-referenced at `T133`, and still
unbuilt. The precommit expects a mechanism the build does not have, so §3's binary stays open no
matter how many frames are shot.

**Not built here** — it is `T133`'s copy call and a build order this lane does not hold. Routed so
the expectation and the code stop disagreeing.

---

# 3. WHAT THE FRAMES DO ANSWER

Docked at `docs/design/dd-import/t147-footer-2026-08-20/` — six representative frames, one per
burst, with `FRAME-INVENTORY-all-36.txt` listing the full set.

- **`E3` is satisfied**: `t147-E1E3-unforced-live-row` carries a live leg row and the footer in one
  unforced frame.
- **The settled state is a REAL settle**, taken through the player's own preview-then-accept path —
  `STAKE $25` / `RETURNED $42` with **both leg chips blank**, which is `T147`'s cancelled-row
  treatment and has never been shot before.
- **The alignment arm is answered on measurement.** Left/left: `RETURNED` at the fact floor overruns
  its box by 51.9px and spills rightward, surviving the mask. Right-anchored: the same ink runs
  `-533.9..-233.0` against a canvas edge at `-490` and **43.9px is clipped off the left** — the
  opening characters destroyed. **Left/left overruns visibly; right/right destroys characters.**
- **Leg-row content is TOP-anchored** (`AnchorTopLeft(row, ColumnInkFloor, 4f)`), which the precommit
  names as the thing it would look at first: the extra ~40px pools beneath each row rather than
  distributing around it.
