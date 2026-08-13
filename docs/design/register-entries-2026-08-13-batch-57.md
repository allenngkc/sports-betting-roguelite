# Register entries — batch 57 — **money is printed in full**

**Design Director** · 2026-08-13 · the money-abbreviation ruling owed at T74-am3, for `RiskPays`'
unbounded payout magnitude.

**Destination:** `C49` → **Cross-surface**. `T74-am4` → **TV**.

---

## The word "unbounded" is doing damage, and it is the wrong one

`PotentialPayout` is parlay-multiplied, so the sweep correctly reports it has **no bounded worst
case**. But **parlay multiplication is not unbounded — it is UN-ENUMERATED.**

`MaxLegs` is finite (4), the stake has a maximum, and every leg's odds come from a pricing model with
its own range. **The largest renderable payout is therefore arithmetic**, not a mystery: a product of
finite terms.

**Nobody has computed it, which is a different problem from it not existing** — and it is the problem
C18 §4.1 names, one level up: *an inventory that does not name its members*. **This seat is not
designing around infinity when the number is a multiplication away.**

**Owed, and it is cheap: the maximum renderable payout, computed from `MaxLegs`, the stake ceiling and
the pricing model's own odds range.** Then the box is sized against **the longest renderable form**,
which is C46's requirement and T89-B's, and this stops being a design question at all.

---

## C49 — money the player can win or lose is printed in FULL

**Law, cross-surface.** Sited here because money precision is a **product** property, not a surface
one — the laptop's ledger, its tallies and its verdict figures are governed by the same sentence as
the theatre's payout.

> **A money figure the player can win, lose, stake or be paid is printed in full: every digit, no
> abbreviation, no rounding, no `k`, no `M`, no cap.**

**Three refusals, each on its own ground rather than on taste:**

- **Abbreviation discards precision on the one fact his money depends on** — and it does so *only for
  large numbers*, so it degrades **exactly the wins that matter most.** `PAYS $12.3k` is not a payout;
  it is a description of one.
- **A format that changes with magnitude is a twitch of a different kind.** This surface has a tabular
  mandate precisely so figures stop moving as they change; a rendering that switches form at
  `$9,999 → $10.0k` re-introduces the instability from the other end, after T82 paid to remove it.
- **A cap is a lie about money.** Displaying less than he wins is a false factual claim, and a price
  or a payout is a fact. C19's *deliberate cap that prints its own count* governs **lists**, never
  amounts.

**Also refused, and already ruled elsewhere: the figure does not shrink.** `tv-design.md` §8 —
*copy truncates or chooses a shorter authored line; **it never shrinks*** — and a content-driven size
change is the runtime resize §5.1 and §6 both forbid.

**What is left is composition and span, which is where every other version of this problem has landed
today.**

**Scope, stated so it is not read wider than it is:** this governs **amounts**. It does not govern
counts, ordinals, clocks or scores, and it does not forbid a non-money aggregate elsewhere choosing a
compact form on its own ruling.

---

## T74-am4 — `RiskPays`: separate rows, and the figure is right-anchored

### The composition — the fourth instance of one answer today

`RISK $1,234     PAYS $12,340` is **two labels and two figures in one row**, measuring 296.5 in a
249.0px box. **N independent strings are not one row** — the intervention prompt became a list, the
money control's members took separate rows, `TakeoverSub` becomes a list, and this is the fourth.

**Ruled: `RISK` and `PAYS` take separate rows.** Each figure then has the row's full width instead of
sharing it with a sibling whose slack is its overrun.

### The anchoring, taken from this surface's own existing pattern

**Label left, figure RIGHT-ANCHORED.**

This is not invented: it is the clock's shape, measured on the Phase T pair — *right-anchored, ink
right edge constant at x≈2343–2348 while the left edge travels* (T75-am2). **A right-anchored figure
grows leftward into its own row's slack rather than overflowing rightward off the box**, so the
binding constraint becomes *figure meets label*, which is far later than *figure meets box edge* — and
it is a constraint the composition can state.

**It also pairs correctly with tabular figures:** T82 made digit advances equal, so a right-anchored
tabular figure grows in exact digit-width steps and the label's clearance is predictable rather than
content-dependent.

**`RISK` is the stable half** — bounded by the stake ceiling — and **`PAYS` is the variable one**, so
the two rows do not have the same problem and only one of them needs the headroom.

### Dispositions, pre-committed on the number nobody has computed yet

1. **The maximum renderable payout fits its own right-anchored row** → the box is sized against it,
   recorded as the longest renderable form, and **this closes with no further ruling.**
2. **It does not fit** → the span is re-derived **once at design time**, which §5.1 makes explicitly
   legal, or the row's composition changes again. **Abbreviation stays refused in both branches**, and
   a span that cannot be found makes this a **C16 classification** — a wider row is a design cost, not
   a platform impossibility, so it is *expensive*, never *impossible*.

**Neither branch reopens C49.**
