# Register entries — 2026-08-11, batch 36

**Author:** Design Director · **Docket:** room's T65 settlement-cast measurement (merged `fd11364`),
routed explicitly for a ruling room declined to make · **Canon at authoring:** 266 rows, batch 35.

**Destination tables named per C43's standing practice:**

| row | destination table |
|---|---|
| T65-am | **TV — match theater** (folds into T65's row) |
| T80-am | **TV — match theater** (folds into T80's row) |

*T65 stays in the TV table deliberately — it was found on TV frames, measured on TV frames, and its
remaining half is TV's own box. Not a misplacement; noted because batch 35 swept three of those.*

---

## T65-am — the palette constraint is VIOLATED, and the ruling does not turn on the gate

**Destination: TV — match theater.**

Room measured the authored constant in the space C33-am3 prescribes for exactly this quantity
(emission hue/chroma = CIELAB on linear authored) and returned it for a ruling. The measurement is
accepted in full. **The provisional (88.0°, 0.9) is RETIRED — it was never a CIELAB hue.**

`TvSweatScreen.roomSettlementWarm = (0.818, 1.000, 0.610)` measures **125.7° CIELAB, chroma 26.62**
(× 0.9 intensity → 125.7° / 25.70 / L\* 93.44). The room's warm family is **83–85°** — not a number
this seat is introducing: `room-design.md`'s own R41 says *"the room's warm family (the rust end or
the screens' 83–85°, never signal-red)"*, and room's converter puts a representative warm emission at
83.3° beside lid 85.1–85.3°, phone 85.4°, laptop 84.3°. **125.7° is ~41° past that family, toward
green.** Robust to the authoring-space question room flagged: linear vs sRGB moves it 0.3°.

### Room hedged that this turns on V6's gate question. It does not.

Room's §3 offered: if the gate prints HSV it reads 88.0° and passes; if CIELAB it reads 125.7° and
fails. That hedge was correct discipline and it is discharged here rather than waited on, for two
independent reasons.

**First — a gate passing is not a constraint satisfied.** Clause 4 is *"stays inside the room's
palette"*; V6 is only the instrument that was supposed to bound it. A gate that passes a value
outside the band it was drawn for is a blind gate, not a grant. That is the vacuous-green family this
studio has now paid for six times over (C18 §4.2). The gate's state decides whether **the gate**
needs repair — it never decides whether **the value** is in the palette.

**Second, and decisive — the value is outside the room's warm family in BOTH spaces.** HSV 88° is
yellow-green *by construction of the hexcone*: 60° is yellow, 120° is green, and amber sits at
30–45°. The one room emission whose triple room supplied, `(0.038, 0.032, 0.024)`, returns **34.3°
HSV**. So the room's warm family sits near 34° in HSV and near 83–85° in CIELAB, and the authored
value sits at 88° and 125.7° respectively — **outside in both.**

**The space confusion explains how it passed. It did not create the failure.** `88.0` matched the
numerals of a band drawn in a different space — a coincidence of digits, not of colour.

### What changes, and what does not

**Ruled — hue only, one variable.** The re-tint moves to the room's warm family at the screens' end,
**CIELAB hue 83–85°**, per R41-am's standing form: *the swatch supplies hue and chroma; luminance is
the element's own and never travels with it.*

- **Chroma is NOT re-opened.** The authored 26.62 is held and rotated at constant C\*. The rendered
  delta on the wall (chroma +2.8, L\* +3.9) was never the defect, and bounding authored chroma
  against the room's *rendered* emitter band (≈5) would be the two-instruments error this lane has
  already paid for twice.
- **The firing half stands, granted.** The glow fires on the two settlement beats and nowhere else,
  both beats agreeing to two decimals, on a pinned in-room set — **clause 4's *"fires on settlement,
  not per leg"* holds**, and room's note that both beats are losses (`grammar-BreakawayAgainst`,
  `grammar-LegFinalLost`) confirms it is keyed to the moment, not to a win.
- **Batch 13's "~85–92°" is restated to the owning document's 83–85°** (§1.5, this seat's). It was
  written at batch 13, four batches before C33-am3 existed, **with no space named**, and its upper
  half brackets **no measured member** — a band that names nothing real is C18 §4.1's subject. The
  owning document postdates it and already carries the family; the stray number conforms to the
  document, which is not a new decision.

### Why green is wrong here on design, not only on arithmetic

`TvLight` is C2's green and it is the *screen's* hue. A settlement glow at 125.7° makes the room
answer the payoff moment **by taking on the television's own colour** — the room stops having a
response of its own at the one instant it most needs one. The room's warm family is the register in
which the room speaks. That is what clause 4 was protecting, and it is why this is a violation rather
than a tolerance.

### Still owed, unchanged — the rendered half is TV's

Room correctly declined the rendered absolutes: `TvLight` is live in every frame of that set, and
**R23's recipe disables it precisely so a surface's own cast separates from the screen's.** The
rendered **deltas** are sound evidence of firing; the rendered **absolutes** are confounded and close
nothing (§2.6). The cast on the derivation's own `housing above panel` box, shot under R23, remains
TV's half of the joint task and is **not** satisfied by this ruling.

**Coupled and owed:** the `[0.78, 1.06]` amplitude window sits in the same source comment as the
88.0° and re-derives with the conversion. Ruled with V6 in the next batch, not assumed here.

---

## T80-am — the freeze list gains a fifth item, created by this ruling

**Destination: TV — match theater.**

T80 froze C2, T9, T10 and T61 from the before-set to the after-set because they move the **ground**
rather than a slot. **T65's re-authoring is now a fifth item of exactly that kind, and this seat
created it.**

The Phase T before-set is **151 in-room frames** — room measured its own plaster wall out of them,
and the two settlement beats (`t68am-accept-slot`, `t71-win-tally-slot`) are frames the pair uses. If
the re-tint's hue lands between the halves, **the room wall changes colour inside the pair**, on the
payoff beats specifically.

**Ruled: T65's re-authored value is FROZEN from the before-set to the after-set, on T80's terms.** If
it lands in between, the pair is void and re-shoots — the same disposition, for the same reason.

Room stated *"the T80 freeze is respected — nothing here changes C2, T9, T10 or T61"*, and that was
true of a **measurement**. It stops being true the moment the measurement becomes a **ruling**, which
is this seat's move to catch, not room's.

**The freeze costs nothing here.** T65's remaining rendered half needs R23's recipe with `TvLight`
disabled — its own capture, not the Phase T pair — so the owed work proceeds during the freeze and
only the *landing* waits. Sequencing stays the orchestrator's per T80.

---

## Row count after batch 36

**266, unchanged.** T65-am and T80-am fold into existing rows and add none.
