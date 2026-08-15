# sgp step 5 — presentation plan (SAME MATCH on the slip)

**Lane:** 2 · **Plan:** F_0.6.0 · **Date:** 2026-08-14 · **Status:** proposed, not started
**Design authority:** `docs/design/surething-design.md` §3.3 + the 2026-08-12 amendment (S73/S74/C47),
S73-am4, batch 48. Those are canon and this plan implements them; it invents no design.
**Engine contract:** `design/02-betting-math.md` § *Same-game tickets*.

---

## The finding that changes what step 5 is

**A player cannot build a same-match ticket today.** The engine guard came out in step 3, but the
slip's own data model still enforces it. `BetslipModel.Toggle` says so in its doc comment:

> *"Adds a market leg, replaces a different selection for the same matchup, or removes the leg when
> the same selection is clicked again. **A matchup contributes at most one leg to the slip.**"*

Picking a second market on a match *replaces* the first. So step 5 is not "render a new ticket type."
It is **make the instrument constructible, then render it.** Everything else follows from that.

Three more structural facts, all in `unity/SBR/Assets/SBR/Runtime/BetslipModel.cs`:

- **The slip is keyed by matchup index.** `SideOn(int)`, `SelectionOn(int)`, `Remove(int)` all assume
  one leg per matchup. Two legs on one match cannot be addressed, removed, or displayed separately.
- **`CombinedOdds` multiplies the legs** — `OddsMath.ParlayDecimal(...)` — and `ToWin` is built on it.
  That figure is both *wrong* for a same-match ticket and, by S73, **the one number the surface is
  forbidden to display**. It has to be replaced by the engine's ticket price, not corrected.
- **`PlaceBlocker` returns a string** and never consults the engine's refusal. An impossible
  combination is not caught before commit; `Place()` would throw at `PlaceTicket`. A Blocked state
  needs cause *and* remedy, which a sentence fragment cannot carry.

**No surface anywhere references `SameMatch`, `Relation`, or `principal`** — a source-wide search
matched only build artifacts. Everything steps 3–4 built for presentation is currently unconsumed.

## What the engine already hands over (built, tested, idle)

Nothing new is needed from the engine for the core of this. Step 3 built the surface's half of the
contract deliberately:

- the ticket's **locked price** off the exact joint — the number the slip shows;
- **`(p_joint, relations[], principal)`** — `principal` exists precisely because batch 48 says the
  slip states one relation, and choosing which is a pricing claim the surface cannot make;
- **`Run.RefusalFor(picks, boost)`** — non-throwing, structured, carrying minimal cause and a
  *verified* remedy. Built for exactly this: stamping a Blocked control **before** the player commits;
- **`TicketState.Voided`** for the refund case.

## Ownership — the ruling this plan needs first

`unity/SBR/Assets/SBR/Runtime/BetslipModel.cs` and the laptop screen belong to **`surething-ui`**, an
active lane. My charter's step 5 lands inside their boundary, and STUDIO.md makes per-lane file
ownership authoritative. Three ways through:

| Option | Shape | Cost |
|---|---|---|
| **A** | `surething-ui` implements from a spec I write | cleanest per STUDIO.md's design flow; slowest, and hands the joint-pricing contract to a lane that has not carried it |
| **B** | `sgp` leases the named files for this slice, `surething-ui` reviews | fastest; puts a non-owner inside a register-heavy surface |
| **C** | **split by layer** — `sgp` takes `BetslipModel` (the model), `surething-ui` takes the screen (the pixels) | recommended |

**C is recommended on the merits, not as a compromise.** The model change is engine-shaped — it is
about the joint price, the relation vocabulary, and structured refusals, all of which this lane
designed and tested. The screen change is register-shaped — stamp ink versus toner, oxide, tracking,
control sizes — which `surething-ui` owns properly and this lane would get wrong. The seam already
exists in canon: *the model emits parts, presentation composes words.* This splits along the same
line.

## Phases

Each ends with the suites green and, where a phase touches Unity, a capture for Design review.

### P1 — make it constructible (model)
`BetslipModel` stops keying on matchup index. `Toggle` adds rather than replaces when the selection
differs on a match already in the slip; `Remove` and the accessors address a **leg**, not a matchup.
`MaxLegs` still binds.

### P2 — the price stops being a product (model)
`CombinedOdds`/`ToWin` are replaced by the engine's locked ticket price. This deletes the
product-of-legs figure from the model, so the surface *cannot* render what S73 forbids even by
accident. **Ordinary tickets must be unaffected to the bit** — the same invariant that governed
step 3, and for the same reason.

### P3 — refusal becomes a Blocked state (model, then screen)
`PlaceBlocker` grows into a structured verdict fed by `Run.RefusalFor`, so an impossible or duplicate
combination is refused *at the slip* rather than at commit. The screen renders it per §3.3: **stamp
ink**, literal reason ≥13px, **cause and remedy**, inside the control, never a disabled control, and
the refused leg stays reachable on its own.

### P4 — the mark (screen)
`THE HOUSE'S LINE` in oxide on the connected picks. **Drawn, not captioned** — the name never prints
beside it. `SAME MATCH` as the instrument name, uppercase, untracked.

### P5 — the statement (screen)
One relation per slip, in toner, composed from `principal` — never a formula, never a coefficient,
never an English string from the engine. The implication case gets its own space, stated not blocked.
Lengthening is **not** remarked.

### P6 — the void arm (screen; closes a standing follow-up)
`TicketState.Voided` renders as VOID with the stake returned. Currently missing in Unity,
`game-console`, and `sim/RunPlayer.ScoreSwings` — all compile, none render it.

## Risks

- **Ownership is blocking.** P1–P3 cannot start until the split is ruled.
- **The interaction model may reach past the slip.** If the board also assumes one selection per
  match, P1 grows. Not yet surveyed — the survey is the first task once ownership lands.
- **Plural remedy — LIVE NOW, not a contingency (corrected 2026-08-14).** The earlier reading was
  that `κ = 1` always has a one-leg fix and only a raised dial needs more. Measurement on the merged
  15-market board says otherwise: **remedies of up to three legs occur at the shipped `κ = 1`**,
  across 645 refusals. The stamp copy must handle a plural remedy today. Two rules bind the screen:
  spending only the first element leaves the slip refused, and the set must be removed **high index
  to low** or earlier removals shift later indices. Both are pinned by assertion.
- **`SideOn`/`SelectionOn` are order-dependent on a same-match group.** They answer for the *first*
  leg on that matchup in slip order and stop — so `SideOn` returns null when a moneyline leg is on
  the slip but sits second behind, say, a totals leg. The same two legs in the other order answer
  differently. Pinned by test as the contract rather than left to be discovered; the screen needs
  leg-addressed accessors for a same-match group, not the matchup-keyed ones.
- **Unity lease.** One editor instance across all worktrees, TV has priority. P4–P6 need scheduling,
  P1–P3 do not.
- **`game-console` is a dead prototype** (T44) — out of scope except as a named follow-up.

## Out of scope, and named

Conditional cash-out (still prices same-match off the naive product) stays a standing follow-up. The
TV sweat's concurrent-leg presentation is `tv-sweat`'s §8.2A work, not this plan's.
