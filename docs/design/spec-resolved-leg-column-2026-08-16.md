# SPEC — the ticket column after a leg resolves (`NEED 0` / `RISK`)

**Written:** Design Director seat, 2026-08-16 · **Authorised:** Allen, relayed 2026-08-16 ·
**Evidence:** `dd-import/corners-sweat-2026-08-16/`, read at `count-sweat-read-2026-08-16.md` §4 ·
**Surface:** TV — match theater

This is a **defect fix, not a direction.** It needs no new treatment, no new colour and no new
vocabulary. Every word it uses is already in the product.

---

## 1. The defect, measured on three frames

The ticket is one leg, `OVER 8.5 CORNERS`. The count crossed 8.5 at ten corners on **53'**. From
that moment the leg cannot lose.

| frame | clock | ticket column |
|---|---|---|
| `scene010 deadair05` | 48' | `8 CORNERS • NEED 1` · `RISK $25` · `PAYS $29` — **correct** |
| `scene013 corner06` | 66' | `10 CORNERS • NEED 0` · `RISK $25` · `PAYS $29` |
| `scene014 deadair07` | 71' | `11 CORNERS • NEED 0` · `RISK $25` · `PAYS $29` |

**Eighteen minutes after the bet could no longer lose, the column still prints a requirement and a
risk.** Two falsehoods, both in money amber:

- **`NEED 0`** — the same construction as `NEED 1` with a different number, so it reads as *a
  requirement that happens to be satisfied*. The player scanning the column sees the shape of
  something outstanding.
- **`RISK $25`** — there is no risk. The word is false at the instant it is printed.

**The count itself is correct and keeps tracking** (10 → 11). `T62` — *progress line lags the
revealed value* — **does not reopen**: the `10` seen at 66' was a mid-event frame inside the
`corner06` window, and 71' reads `11`. Checked before it was claimed.

## 2. THE GOVERNING LAW IS ALREADY OURS — this invents nothing

`G1` authored **two strings per leg**, and drew the line this fix is about:

> **NEED** = the requirement **while live**. **compact** = identity **elsewhere**.

**A resolved leg is not live, so by G1's own definition the `NEED` form is the wrong string for it.**
The build is not missing a rule; it is failing to apply one it already has. This is the same shape as
batch 91 — *it is not a new rule, it is S69's* — and it is why §7's *nothing new to learn* is
satisfied trivially: there is nothing new here at all.

The state is already present too. `TvSweatScreen.RevealedLegState { Pending, Live, Won, Lost,
Voided }` and `RevealedTicketState { Riding, Won, Lost, CashedOut }` both exist. **The surface has
the information and is not reading it.**

## 3. THE RULING — four clauses

### Clause 1 — the progress line follows `RevealedLegState`

| leg state | line |
|---|---|
| `Live` | `{n} CORNERS • NEED {k}` — **unchanged** |
| `Won` | `{n} CORNERS • WON` |
| `Lost` | `{n} CORNERS • LOST` |

`WON` is **the surface's own word** — settlement already prints `LEG 1 — WON`. Nothing is coined.

**The count keeps its place on a resolved leg**, because it is the fact that decided it. §3.1's
standard applies: the mark is **drawn, not captioned** — the column shows *eleven corners, won*, and
does not explain the relationship.

**`NEED 0` must be UNCONSTRUCTIBLE, not guarded against.** The form is selected by leg state, so
`k = 0` is unreachable because a resolved leg never takes the `NEED` form — not because a guard
catches it. This is the studio's standing preference, most recently the draw cell built as a
derivation rather than a constant (`S74-am3`): *a constant that happens to equal the right answer is
a constant that will stop equalling it.* A guard that suppresses `NEED 0` would leave the wrong form
selected and merely hide its symptom.

### Clause 2 — `RISK` follows `RevealedTicketState`, and it is a TICKET word

**This is the clause most likely to be built wrong**, because in the evidence the leg and the ticket
resolve at the same instant — there is one leg. **They are different things and must not be keyed
together.** `RISK $25 · PAYS $29` is the ticket's stake and return; it is governed by the ticket, and
on a multi-leg ticket one leg winning changes nothing about it.

| ticket state | pair |
|---|---|
| `Riding` | `RISK ${stake}` · `PAYS ${return}` — **unchanged** |
| cannot lose (every leg `Won`, settlement pending) | **`STAKE ${stake}`** · `PAYS ${return}` |
| dead (any leg `Lost`) | **see §5 — derived, not evidenced** |

**`RISK` → `STAKE` is a one-word change and the word is already in the product** — the laptop's
margin prints `STAKE $35`. Same figure, same position, same amber, same box. The stake is still a
true fact; it is simply no longer *at risk*, and `STAKE` is the neutral word for that.

**The amber does not change and no colour is added.** The status is carried by the word, per the
standing law that status is never carried by colour alone.

### Clause 3 — the trigger is the REVEALED state, never the resolved one

**The column may not tell the player he has won before the surface has shown him the fact that won
it.**

This is not pedantry here. `count-sweat-read-2026-08-16.md` §5 measured the two diverging by most of
a match: the corners arm's ticket rode on a result that existed at lock, while the revealed scoreline
sat at `0 — 0` until `90'+1`. A column keyed to the resolved match would have announced outcomes the
screen had not yet justified.

Batch 93's mark already draws this line — `—` means *not yet revealed*, not *not applicable*. The
column takes the same clock.

### Clause 4 — nothing else in the column moves

Named because a fix of this shape invites tidying:

- The count keeps updating after resolution. It is true, and freezing it would be a second lie.
- `PAYS` is untouched in the riding and cannot-lose states.
- The `NEED` form is unchanged for live legs — this spec does not touch the live case at all.
- No new glyph, no new colour, no rule, no field, no icon.

## 4. OUT OF SCOPE, and named so its absence is not read as "checked"

**The flavour strip's staleness is NOT fixed here.** At 71' the strip still reads `whipped into the
corner — the count moves again` — the 65' line, six minutes old, narrating a count that stopped
mattering at 53'. That is the **hand-over** question (`theater-count-markets-2026-08-16.md` §8E), it
is a direction rather than a defect, and it is with Allen. **This spec deliberately leaves it**, so a
lane must not "while we're in here" it.

Likewise untouched: the missing resting state (`count-sweat-read` §2), the rate line (§7), and the
reveal question (§5).

## 5. THE DEAD-TICKET CASE — derived, not evidenced

The capture contains **no losing ticket**, so the dead state's exact strings are not settled by
evidence and are not ruled here. What *is* ruled is the principle that generates them:

> **No word in the pair may name a jeopardy or a payout that no longer exists.**

On a dead ticket both words are false today — there is no risk left and it pays nothing. The
symmetric reading is `STAKE ${stake}` beside a return that states zero rather than a promise, but
**`PAYS $0` versus some other form is a copy decision and it is authored on a frame, not here**
(C11). **Capture owed**, and until it lands a lane must not invent the string.

## 6. THE GATE, and what it cannot see (C18 §4.2)

- Assert the progress-line **form** is selected by `RevealedLegState`, so `NEED` cannot be
  constructed for a non-`Live` leg. Assert `NEED {k}` is never emitted with `k = 0`, as the
  *consequence* of that selection rather than as the mechanism.
- Assert the pair's first word is a function of `RevealedTicketState` **and not of any leg's state** —
  this is the clause-2 trap, and a single-leg fixture cannot catch it. **The gate must exercise a
  multi-leg ticket with one leg won and one live**, or it certifies nothing about the distinction it
  exists to protect.
- Assert both read off the **revealed** ledger.

**What the gate is blind to:** whether `WON` and `STAKE` read correctly at review distance, and
whether the shortened strings sit right in the column. Those are frame claims (C11) and the gate
states nothing about them.

## 7. `C46` — the strings must be swept against their box

`{n} CORNERS • WON` is shorter than `{n} CORNERS • NEED {k}`, so the change relieves the box rather
than pressing it — but **relief is not a measurement**, and the column's strings have never been
swept. `T101`'s residual already owes exactly this sweep for the stats panel's strings, and
`count-sweat-read` §6 found the flavour strip clipping mid-word on the same surface. **Three string
families on one surface, none swept. They should be swept in one pass**, and this spec's new strings
join that sweep rather than getting a private one.

## 8. Evidence owed before Design-verified

1. A won leg with match time remaining — **the before-state already exists** in this set, so the
   after can be read against it directly, same seed, same fixture.
2. **A multi-leg ticket, one leg won and one live** — clause 2's distinction is untestable on the
   evidence we have.
3. A losing ticket, for §5.
