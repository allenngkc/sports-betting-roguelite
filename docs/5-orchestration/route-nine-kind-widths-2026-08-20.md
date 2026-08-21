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

### WHAT IS NOT ANSWERED, and it needs one more pass

**The sweep reports widest-per-slot only, so this does not say which of the other eight kinds
clear.** The widest is a team-total form in both boxes; whether `DoubleChance`, `Handicap`,
`PlayerMultiScorer` and the three grammar-breakers fit is unmeasured. **Per-form attribution is one
instrument change away** and this lane will take it on order.

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
