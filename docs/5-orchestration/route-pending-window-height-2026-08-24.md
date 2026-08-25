# Route: the pending window cannot take `T143` or `S85` as a new row — TV → DD (2026-08-24)

`T143` says the window NAMES EVERY DEAD LEG. `S85`'s general rule says the surface states
`NoSingleCallSaves` BEFORE the offer. **Each needs a row. The zone has no row to give.**

Measured on the production face, EditMode, `EditMode-pendzone2.xml` — font asserted Encode Sans, not
the fallback (`T20`). **Nothing below is authored copy and no height is ruled here.**

---

## THE ZONE: 635.0 × 90.0, AND IT HOLDS EXACTLY THREE ROWS

Height is **linear at 27.5px per row**, measured across four row counts rather than inferred:

| rows | 2 | 3 | 4 | 5 |
|---|---|---|---|---|
| height | 55.0 | **82.5** | 110.0 | 137.5 |

**90.0 admits three.** The shipped worst case uses all three at 82.5 — **7.5px spare**, and a fourth
row costs 20.0px.

> **THE 20.0px IS NOT NEW.** The zone's own build note already records it: *"Title + three options is
> 110.0px — over by 20.0px."* This is the same ceiling, re-derived. **The zone has been at its limit
> since that batch**, so neither ruling is what broke it — they are what arrived at a full zone.

## EVERY COMPOSITION, MEASURED

| composition | rows | height vs 90.0 | widest row vs 635.0 |
|---|---|---|---|
| shipped worst case (both consumables) | 3 | **82.5 FITS** | 523.8 fits |
| + one dead leg named | 4 | **over by 20.0** | 523.8 fits |
| + two dead legs named | 4 | **over by 20.0** | **699.5 — OVERRUNS by 64.5** |
| + two dead + no-save | 5 | **over by 47.5** | **699.5 — OVERRUNS** |
| no-save line only (`S85` minimum) | 4 | **over by 20.0** | 523.8 fits |

**Neither ruling fits, and not even one of them alone.**

---

## THE THREE OPTIONS, PRICED

### OPTION 1 — A ROW YIELDS. **Priced, and it fails on `C46`.**

The spending rows render only when the run OWNS that consumable — the count is `1 + canM + canR`. So
the overrun is CONDITIONAL:

| ownership | + dead leg | + dead leg AND no-save |
|---|---|---|
| both consumables | 4 rows — **over by 20.0** | 5 rows — **over by 47.5** |
| one consumable | 3 rows — **82.5 FITS** | 4 rows — **over by 20.0** |
| no consumables | 2 rows — **55.0 FITS** | 3 rows — fits |

**It fits in the common case and fails in the worst one — which is precisely what this zone's own
note forbids**, in its own words: *"three option rows … fit in EVERY ownership combination, not only
when one consumable is held — **C46 forbids leaning on the common case**."*

**And the row that would yield is a player ACTION.** All three are: two spending options and the
decline. §7c rules saves stay **LEGAL** — *"the player may still spend one"* — so dropping a spending
row removes an affordance the ruling explicitly preserves. **This lane will not propose it.**

### OPTION 2 — THE ZONE GROWS. **NOT MEASURED, and deliberately so.**

+20.0px for four rows, +47.5px for five. **What that displaces is not measured here**, because the
zone is placed off `grid.Stage` and §6's grid does not resize to content — what yields on the far side
is a §6 question this lane may not answer. Priced only as the delta.

### OPTION 3 — THE COPY SHARES THE DECLINE ROW. **Priced, and it FITS — with one exception.**

Zero height cost; spends width on a row with room:

| shared row | rows | height | width vs 635.0 |
|---|---|---|---|
| one dead leg · decline | 3 | **82.5 FITS** | **528.4 fits** (106.6 spare) |
| no-save line · decline | 3 | **82.5 FITS** | **563.7 fits** (71.3 spare) |
| **two dead legs · decline** | 3 | 82.5 fits | **870.4 — OVERRUNS by 235.4** |

**The one-leg and no-save forms fit in every ownership combination**, which is the test option 1
fails. **The two-leg form does not, and it is not close** — 235.4px over, and it already overruns as
a row of its own at 699.5.

---

## THE ASYMMETRY THAT MAKES STAGING REAL, NOT A COMPROMISE

**`T143`'s dead-leg naming fires on EVERY whistle. `S85`'s no-save line fires only when ≥2 legs die at
one whistle — which NO TICKET SHIPPING TODAY CAN PRODUCE.** It needs a same-match ticket, the shape
arm A created and nothing yet deals.

So the two rulings do not have to land together. **`T143`'s one-leg form fits the decline row today,
at 528.4 of 635.0.** The two-leg form and the no-save line are both N-live-only and can land with the
shape that triggers them.

## WHAT THIS LANE IS NOT DECIDING

- **The height.** §6's grid does not resize to content, and the last overrun here was ROUTED rather
  than absorbed. Which row yields, or whether the zone grows, is the DD's.
- **The copy.** The strings above are SHAPES at worst-case length, not authored lines. `DULUTH
  AUDITORS +1.5 IS DEAD` is a measuring stick; the real form is unruled.
- **Option 2's displacement**, which is unmeasured and marked as such rather than estimated.

**Asked of the DD:** which option, and — if option 3 — what the two-leg form does, since it overruns
in both the shared and the standalone shape.

**Not blocking:** the once-per-whistle verification (that the window opens once after every grade on
the fixture, not once per leg) is composition-free and this lane can build it while this is ruled.
