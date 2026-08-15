# Register entries — 2026-08-15, batch 83

**S84's RESIDUAL CLOSES — the population is bounded on both sides and the cell holds across all of
it.** Ruled at the DD seat on the engine lane's corner answer, taken from the existing sweep with no
run spent.

**Destination table: SureThing — the laptop.** **Row shipped:** `S84-am` (the residual, closed).

---

## S84-am — BOTH ENDS MEASURED. The cell holds across the whole reachable range.

**The minimum reachable draw price is decimal 3.3527 — and unlike the 4.2058 supremum it is
ATTAINED**, at low-end tempo with interior `p`, both reachable. **In the face's own units:**

| | decimal | American | digits |
|---|---|---|---|
| minimum, **attained** | 3.3527 | **`+235`** | 3 |
| maximum, **supremum** | 4.2058 | **`+320`** | 3 |

**Three digits at both ends, and tabular digits make every string between them identical in width**
(S29: spread 0, every digit 41.05). **So `DRAW {price}` measures 97.1px — 87% of its 112px cell —
for every price the model can emit, at either extreme and everywhere in between.**

**And the two-sided worry is answered rather than argued away: no negative American price exists,
because the minimum decimal is 3.3527 and a negative price needs under 2.0.** **Batch 82 reasoned
that structurally and said a plausibility argument is not a measurement. It is now a measurement.**

**Corroborated by every frame this seat has read** — `+240 +243 +246 +253 +261 +281 +293` across four
boards, all interior to `[+235, +320]`. **The observed sample sits inside the measured population,
which is what a correct bound looks like from the outside.**

**S84's residual CLOSES. S84 is closed on both ends. No further measurement is owed and none was
spent.**

---

## The lane's caveat is right, and the rule it implies is already this surface's — third instance

> *If any surface treats the supremum and the reachable minimum as the same kind of boundary, that
> distinction is the one place the answer could mislead.*

**Correct, and worth the line it took.** **One end is attained and the other is not**, and a spec, a
test or a comment that lists them as a matched pair would be quietly claiming the board can print
4.2058 when it cannot.

**At the FACE the distinction is immaterial, and the reason is worth stating:** the supremum and
every value one ulp inside it **format to the same rendered string**. **The surface never meets the
decimal; it meets `+320`.** So the cell's sufficiency does not depend on which end is attained.

**But that immateriality is a CONSEQUENCE, not a licence**, and the rule that preserves it is one
this surface has now reached three times from three directions:

> **Pin what is RENDERED, never what is REACHABLE in the model.**

- the DRAW cell's own gate **measures the rendered cells rather than the constants** (batch 79);
- the C46 width figures were taken **off the rendered control, never numbers copied from the call
  site** (batch 79);
- and now: **a boundary in the model is not a boundary on the face until it has been through the
  formatter.**

**RULED: the model's bounds are recorded as model facts and are never pinned as strings.** **Anything
asserting a widest or narrowest rendered price measures the RENDERED price** — which also means the
supremum/attained distinction can be stated honestly in the record without ever having to be
reproduced in a test.

**Batch 82's standing trigger is unchanged and gains one clause:** the cell's sufficiency rests on
the draw arm's decimal supremum staying under ~101; **if that bound moves, the re-check is a
RE-MEASUREMENT OF THE RENDERED STRING, not a comparison of decimals.**

---

**Routing.** **S84 CLOSED, both ends. → surething-ui and the engine lane for the record; no build, no
change, nothing measured.** **The one added clause goes into the same source comment batch 82
already ordered.**

**Still open and unchanged: `S83`'s Design-verification (the four-legs-plus-consumable capture,
queued behind the gesture) and `T99` (TV's event-strip answer, then the stats-panel capture at a
non-level scoreline).**

**To Allen, in one line:** *both ends of the draw price are now measured and the cell holds across
every price the game can deal — and the lane's own caveat earned a rule: a bound in the model is not
a bound on the screen until it has been through the formatter, so we pin what is printed, never what
is possible.*
