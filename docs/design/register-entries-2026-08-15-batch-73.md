# Register entries — 2026-08-15, batch 73

**S80 — THE RELATION STATEMENT'S PIXELS: the exact move**, specced at the DD seat on Allen's
*pay the pixels* ruling. **Destination table: SureThing — the laptop.** **Row shipped:** `S80`.

**The spec is buildable today in four of its five parts. The fifth — the donor's SIZE — turns on a
measurement this seat cannot take, and the reconstruction below says Allen's ~36px is the STATEMENT's
cost rather than the RESERVATION's.** That distinction is the whole reason to spec it rather than
apply the number.

---

## 1. The 36px, derived

`RelationStatementHeight = 30f`, consumed at `y -= RelationStatementHeight + 6f` — **30px box + 6px
separator = 36px.** That is Allen's figure and it is correct **as the statement's own cost.**

**The 30 is a TWO-LINE box and that is why it is 30 rather than 20.** The longest approved sentence —
`THE SAME TEAM'S GOALS SETTLE THESE OPPOSITE WAYS.`, 48 chars — measures ~338px at su's own 13px scale
(~7.05px/char, from `RUB OUT ALL THREE MARKS TO PLACE.` at 225.7px) against a ~296px content width.
**It wraps.** The shortest, `THE SAME GOALS SETTLE BOTH.`, does not.

**This is batch 71's owed measurement and it is now load-bearing.** **The constant is derived from the
longest sentence's measured height, not from this seat's arithmetic** — §2's *a fixed grid constant
re-derived once at design time is explicitly legal.* **If the longest wraps to two lines, 30 stands;
if the face measures wider and it reaches three, 30 is wrong and the whole reservation moves.**

## 2. The pad survives at 6px — non-negotiable

**S51 refused paying a content cost out of the separation budget six days ago; this is the same trade
and it gets the same answer.** The 6px in `ActionBandReservedHeight = PlaceBandY + PlaceBandH + 6f` is
the separation T47's anchoring exists to guarantee. **The statement does not eat it.**

## 3. The action band is NOT the donor — and this is arithmetic, not preference

```
ActionBandReservedHeight = 160
  SKIP   8 … 42   (h 34, --st-skip-h)      gap 42→52  = 10
  LOCK  52 … 104  (h 52, --st-lock-h)      gap 104→110 =  6
  PLACE 110 … 154 (h 44, --st-place-h)     pad 154→160 =  6
  controls 130  +  gaps 24  +  pad 6  =  160
```

**Absorbing 36 would put `PlaceBandY` at 74, which forces `LockBandY` to 16 and `SkipBandY` to −20 —
below the panel floor.** Three ruled §2.2 control heights plus 24px of separation occupy 154 of the
160. **There is nothing there to take.**

## 4. The donor must be cited by name (R30), and only one band can be cited

```
 34  OS rail    — BARRED: pixel-identical chrome across destinations (S48, S52)
 38  app tabs   — cannot yield 36 (--st-tab-h 27 inside it)
 68  masthead   — the only citable band
530  work area
 34  OS tray    — BARRED: same as the rail (S52)
```

**Proposed: masthead 68 → 32, work area 530 → 566.** Arithmetic still sums:
`34 + 38 + 32 + 566 + 34 = 704`. Then `MarginFlowBudget = 566 − 160 = 406` — **exactly 370 + 36.**

**Its virtue is the one that matters here: the action stack does not move at all.** The work area
grows *upward*; `PlaceBandY`, `LockBandY`, `SkipBandY` and all three control heights are untouched,
which is precisely what T47's anchoring is for.

**Its cost, stated plainly: the masthead is shared chrome, so every screen pays.** **That is the
material half and it takes Allen's word** — nothing else in this spec waits on it.

## 5. THE WARNING — ~36px is the statement's cost, and the RESERVATION may need ~70

**The cursor chain reconstructs exactly against the one measured datum, which is why it is worth
acting on.** Worst case, 4 legs:

| | px |
|---|---|
| header (`y = -44`) | 44 |
| 4 legs × `LegRowPitch 35` | 140 |
| line 1096 · 1127 | 4 + 28 |
| **modifiers row (1138)** | **34 — conditional** |
| lines 1180 · 1186 · 1191 · 1200 | 34 + 34 + 32 + 18 |
| payout block (1228) | 40 |
| **base, no modifiers, no statement** | **374** |

**374 reconciles exactly with S51's measurement** (`flowBottom = −374.56`) **and with su's own reading
that the payout figure's box bottom is flush with the budget at 370** — the last 4px is the wax band,
which S51's fix removes. **The base worst case fills 370 with ZERO slack.**

**The modifiers row is gated on `run.OwnsConsumable("free_bet") || OwnsConsumable("double_or_nothing")`
— pure RUN state, independent of leg count, slip contents and same-match status. It can co-occur with
four legs and a relation statement.** So:

| case | flow | vs 370 |
|---|---|---|
| base | 374 → **370** after S51 | flush |
| + statement | 406 | **+36** |
| + modifiers | 404 | **+34** |
| **+ both** | **440** | **+70** |

**Two consequences, and the second is the one that decides this spec.**

**(a) There may be a LATENT overrun today, before any relation statement** — 4 legs plus a held
consumable is +34 over budget, and the S51 pin measures a staged case that appears not to include
modifiers. **This seat asserts it as a reconstruction, not a fact.** **Measure it.**

**(b) A 36px donor under-provisions the reservation by 34px** — and **the masthead's ceiling is 36
(68 → 32).** **If the worst case is 440, the only citable band cannot cover it**, and the shortfall is
a scope call that comes back to Allen under R30 and §1.2 rather than being solved at this seat.

**OWED BEFORE THE CONSTANT IS FIXED: sweep the state space and report flow bottom for
legs {1..4} × modifiers {none, one, both} × statement {absent, present, longest sentence}.** That is
C46's discipline — **sweep the population, not the suspects** — and it is nine to thirty-six
measurements, not a judgement.

## 6. What the margin invariant becomes

**S51's two-sided equality retires into it.** The invariant asserts, **on the measured worst case**:

1. **`flowBottom` ≥ −`MarginFlowBudget`** — the flow fits, overrun ≤ 0. **No signed deviation, no
   recorded excursion**: S51 existed because an overrun had no owner, and this one has a spec.
2. **Slack ≤ one leg row (35px)** — still two-sided, in S51's own spirit. **A reservation that
   silently frees a leg's worth of space means content was deleted**, and that must fail rather than
   go quietly green.
3. **It states its blind spots** (T53), and it must say that it measures `RectTransform` bounds, so a
   sentence whose glyphs bleed past its 30px box is invisible to it — **which is exactly the failure
   mode §1's measurement is guarding against.**

---

**RULED, and buildable now:** the pad survives at 6px (§2); the action band is not the donor and does
not move (§3); the donor is cited as the masthead if 36 suffices (§4); the invariant takes the shape
in §6. **Blocked on measurement, not on judgement:** the statement's own height (§1) and the
reservation's true size (§5). **Nothing here re-opens the wording** — Allen ruled it stays exactly as
approved, and nothing in this spec suppresses a sentence at the limit.
