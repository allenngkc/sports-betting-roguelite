# SPEC — the ticket footer's two money facts (for Allen; builds at the TV seat)

**Written:** Design Director seat, 2026-08-19 · **Batch 132**
**Ruled by Allen, relayed 2026-08-19:** **`T144` takes `T74-am3`'s separate rows.**
**Written for a lead who was not here.** The TV seat rotated at 98%; this spec assumes no memory of
the thread and carries every number it depends on.

---

## 0. THE FIRST THING TO KNOW — this was ruled seventy-one batches ago and never built

**`T144` is not a new finding.** `T74-am6` (batch 60) found it, measured it better than `T144` did,
and ruled it. Under `C22.1` — *where the same finding is ruled twice under two IDs, the earlier ID
governs* — **`T74-am6` governs and `T144` is its cross-reference.**

`T74-am6`, verbatim:

> **Bank $10,000: RISK 138.4 + PAYS 239.7 = 378.1 against 249.0, over by 129.1. TYPICAL: 124.7 +
> 145.9 = 270.6, OVER BY 21.6.** Each half fits ALONE; **it is the pair that does not.** … **the fact
> floor is NOT a tail case: `$1,234` staked paying `$12,340` is a plain 10× parlay, so THE FOOTER
> COLLIDES AT ORDINARY VALUES.**

**`T144` added one thing and it is worth having: the frame.** Batch 60 measured it; batch 131 saw it —
`RISK $25` and the figure drawn on top of each other, in every state including the incumbent.

**Why it was lost:** `T74-am3`, `T74-am5` and `T74-am6` are cited **eighteen times inside other rows**
and **none of them is a row.** `C22` says a ruling exists when it is a row in `REGISTER.md`; by the
studio's own law these three do not exist, and the cost of that is exactly what happened — the defect
was re-found seventy-one batches later by a different route.

---

## 1. THE GEOMETRY, MEASURED — everything the build needs

| | | source |
|---|---|---|
| `TicketFooterHeight` | **40px** | `TvSweatScreen.cs:867` — *"T20: 36 → 40 to hold 24px"* |
| `TypeRisk` | **24px** | `:917` |
| footer inner width | **249.0px** (`width − 16`) | `:5250`, `:5255` |
| `TicketHeaderHeight` | 24px | `:866` |
| `BottomRowHeight` | 52px | `:865` |
| `TicketRowSlots` | 6 | `:874` |
| `TicketRowHeight` | **`(bottomY − 24 − 40) / 6`** | `:1041` |

**Both halves are built as full-width rects at the same y**, one `UpperLeft`, one `UpperRight`
(`:5248`–`:5256`), each `grid.TicketFooter.width − 16`. **That is why each sweeps clean against 249.0
and the pair still collides: they share one row and the sweep measures each on its own.**

**And the line-box ratio on this face is 1.25, not the 1.18 the design constants assume** —
established at `T74-am3` (`:5449`), which is why *"the design constants predicted a fit."* **Use
1.25.** Two 24px rows are **60.0px**, against a 40px footer.

---

## 2. WHY SEPARATE ROWS IS THE ONLY REMEDY THAT REACHES THE PROBLEM

Stated because a cheaper lever exists and does **not** suffice, and a lead will find it.

- **Label scale.** `T74-am3` ruled *the status word rides at label scale beside the figure, never at
  money scale*, and today `RISK`/`PAYS` render at money scale. Dropping the two labels to label
  scale plausibly clears **ordinary** values (over by 21.6px) — **and does not touch the fact floor,
  which is over by 129.1px.**
- **Abbreviation** is refused (`C49`), copy is not reopened (`T24-am`), truncation is barred (`T69`),
  and the column's outer width is **locked** (`T46`, `R30`) — a change there is Allen's and he did not
  make it.
- **Separate rows is the only form where each fact gets the full 249.0px**, and at full width both
  clear their own enumerated worst case: `PAYS $73,318,376,502` = 239.7 ≤ 249.0, `RISK $13,639` =
  138.4 ≤ 249.0.

**So the ruling is not a preference between two adequate answers. It is the only composition inside
the locked column that carries the fact floor.**

---

## 3. THE RULING

**`RISK`/`STAKE` on the first row. `PAYS`/`RETURNED` on the second. Each row the full inner width.**

### 3.1 Both rows take the SAME anchor — left

The opposite anchoring is `T74-am5`'s device and it had one job: *"anchor `RISK` to the row's left
edge and `PAYS` to its right and the authored gap CEASES TO EXIST."* **On separate rows there is no
shared gap, so the device has no subject** — and keeping it would leave a stagger nobody chose.

**Left-anchored, both**, matching the money control's two members (`:5468` — *"Anchors are left
exactly as they were"*) and the column's other rows. **This is the ruling naming its alignment rather
than leaving it**, because leaving it is what `T74-am3` explicitly declined to do and the footer has
no equivalent reason to inherit that silence.

### 3.2 Reading order: what he put in, then what comes back

`STAKE` above `RETURNED` is `S38`'s own pair order on the laptop ledger. Unchanged for the live
states (`RISK` above `PAYS`).

### 3.3 The height, and it is the whole cost

Two 24px rows at the measured 1.25 ratio need **60.0px**. The footer is **40px**.

**`TicketRowHeight` is DERIVED from the footer's height** (`:1041`), so:

> **Every pixel the footer grows comes out of the six leg rows, at one sixth each. Growing 40 → 60
> costs each leg row 3.33px.**

**That is arithmetic, not a risk, and it is stated here because `T74-am5` is the case of it being
missed:** *"this seat ruled `RiskPays` into two rows on WIDTH and re-derived no HEIGHT, so two 24px
rows need 60.0 in a 40.0 footer, over by 20."* **`C46-am` requires the fit re-derived in the same
breath as the ruling. This section is that breath, and §4 is its gate.**

**Not pre-ruled:** whether the 20px comes from the leg rows, from `TypeRisk`, or from somewhere the
build finds. **`T20` grew this footer once already (36 → 40) to hold 24px type**, so growth is
precedented — but the leg rows pay for it and nobody has checked they can.

---

## 4. THE GATE — this must pass BEFORE the composition lands

1. **The live leg row still fits at the reduced `TicketRowHeight`.** A live row carries a compact
   line, a NEED and a progress line; `T90` ruled the NEED band and `T84` the extents. **Re-derive
   all three against the new row height and report the numbers.** This is the gate `T74-am5` did not
   have.
2. **The pair check returns for this control** (`T144`): assert `RiskPays` ink **+** `Pays` ink
   against the row they share — **or, once they no longer share one, assert each against its own row
   and assert the rows do not overlap.** Two independent green checks are what let this ship.
3. **Both worst cases at full width**, from the enumerated pool (`PayoutMaximumTests`), not the seed's.
4. **Measured at the real face with the 1.25 line-box ratio**, never the 1.18 constant.
5. **If the height does not clear, it comes back here with the number** — as a `C16` signed deviation
   with a named cost and expiry, the way `T74-am3`'s own 3.0px overrun was signed at `T84-am7`.
   **It is not absorbed silently and it is not solved by shrinking a money fact without a ruling.**

---

## 5. WHAT THIS SPEC DOES NOT DO

- **It does not reopen the copy.** `T133-am`'s `PAID` candidate and TV's root-collision objection with
  `PAY $60` are untouched and still open; they ride on the re-shoot this composition needs anyway.
- **It does not touch the column's outer width** (locked, `T46`/`R30`).
- **It does not change the money control** (`§6.1`) — `T74-am3` already moved it and it stays.
- **It does not rule the type size.** If the answer turns out to be smaller type on a money fact,
  that is a `§4` ruling and it returns to this seat.

---

## 6. EVIDENCE OWED

| | what it must show |
|---|---|
| `E1` | the footer at **ordinary values** — `$1,234` staked paying `$12,340`, `T74-am6`'s own case — both rows clean |
| `E2` | the footer at the **enumerated fact floor**, forced and disclosed in the filename (`S3`/`T133`'s convention) |
| `E3` | **a live leg row in the same frame**, so the height paid for §3.3 is visible where it was taken from |

**`E3` is the one to hold on.** The footer is easy to shoot and easy to like; **the cost lands
somewhere else on the same screen**, and a set that does not show both has not shown the change.
