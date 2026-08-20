# Route: the settled ticket's rows still say NEXT — TV → DD (2026-08-19, diagnosis per Allen)

The PlayMode red found while running `T144`'s gate, diagnosed to source. **Inherited from
`e8cb38e`** (the three ruled string builds — the settled footer, `T121`/`T114-am`); the pin that
catches it predates it (`acd9d9f` / `4e45464`). Attribution was measured: both this seat's files
stashed, tree recompiled, pin re-run filtered — **1 of 1 executed, still failed.**

```
FAIL SBR.Tests.PlayMode.TvSweatScreenTests
     .TicketFooterWord_NeverDisagreesWithAnyRow_AndNoLiveRowEverPrintsNeedZero
     "leg N shows the NEXT chip but the footer reads 'STAKE'" — expected RISK
```

## THE MECHANISM — two sources of truth, and the session stops between them

The footer's settled branch reads the **ENGINE's** `_ticket.State` (`TvSweatScreen.cs:3011-3012`).
The rows read the **SURFACE's** reveal cursor `_resolvedThrough` (`:2897`, `:2899`).

Both engine paths that settle a ticket set the state **and end the session in the same breath**:

| | |
|---|---|
| `SweatSession.cs:252-253` | `Bust()` — `_ticket.State = Lost; _complete = true;` |
| `SweatSession.cs:503-508` | cash out — `_ticket.State = CashedOut; _complete = true;` |
| `SweatSession.cs:136-140` | `if (_complete) { evt = null; return false; }` — **no further drama events** |

So once a ticket settles, **the remaining legs are never resolved on the surface.** They are
neither `i < _resolvedThrough` nor live, so they fall to `UpdateTicketColumn`'s final `else` and
print the **NEXT** chip — *"a NEXT leg is not dead — it is the next thing that can take his
money"* (`T25.6`, at the site). Meanwhile the footer, correctly, says the position is closed.

**And the bust is INSTANT on the first losing leg** (`SweatSession.cs:185`, *"No save held → the
bust is instant"*). `DemoTicketPolicy` deals 2 or 3 legs. So on most tickets that do not win
outright, the column ends the sweat saying both things at once.

### It is a steady state, not a race — which is why the pin trips early

The pin failed at **frame 16** in one run and **frame 51** in another, out of hundreds sampled.
That is not a transient: with the test's `TimeScaleOverride = 0.0001f` fast-forward the sweat
settles within a few sampled frames, and every frame after settlement fails. **The disagreement
is permanent for the rest of the sweat**, not a moment during it.

## WHO IS WRONG: THE ROWS, NOT THE FOOTER

`T121`'s footer is right, and by its own principle — no word may name a jeopardy or a payout that
no longer exists. **The row is naming one.** A leg that can never be played is not "the next thing
that can take his money"; it is cancelled.

### §8.10 already has the vocabulary, and it is gated on the wrong flag

The site says it in terms: a pending leg ended by a cash-out is **STRUCK** with the VOID strike,
never the LOST extinguish, because *"a leg being CANCELLED must not read as a leg LOST at the
exact moment the player is deciding whether to cancel it."* Correct — but the strike is gated on
**`_cashOutPreview`** (`:2986`), which is true only while the player is *deciding*.

> **The surface marks the leg cancelled while the player is deciding, and un-marks it the moment
> it actually is cancelled.** The same strike never fires at all for a bust.

### `T121` knew this boundary and stopped one step short

Its own comment justifies reading `_ticket.State` because *"a cash-out is a PLAYER ACTION and is
not derivable from leg outcomes at all, so `StakeWord` structurally cannot see it — T114 says so
in terms."* **That is right for `CashedOut`.** It was then extended to `Lost`, which *is*
derivable from leg outcomes and *is* subject to reveal timing. The justification does not cover
the branch it was applied to.

## A SECOND RISK, FLAGGED AND NOT VERIFIED

Because the footer reads engine truth on **every** repaint, any repaint landing between the
engine's bust and the surface's reveal of the losing leg would **announce the death before the
theater shows it** — an unrevealed outcome reaching the surface, which is the exact class
`SweatActiveLegModel`'s no-leak law exists to prevent. The reveal-driven sites advance cursor and
repaint together (`:2033-2034`, `:3922-3923`), but there are eight other call sites and this seat
did not trace their ordering. **The steady-state defect above does not depend on this**; it is
worth settling on its own because a leak is blocker-class where a disagreement is not.

## THE REMEDIES, COSTED — and none is TV's to rule

`T121` ruled only the PRINCIPLE for the dead ticket and left its strings to a frame. **The row
treatment on a settled ticket is equally unruled**, so this comes here rather than being built.

1. **Rows follow the ticket** — on a settled ticket the remaining legs take §8.10's strike
   permanently, instead of only while previewing. Smallest change, existing vocabulary, **no new
   string to author**, and it satisfies the pin as written. Cost: it changes what a dead ticket
   looks like, which is a frame decision.
2. **Footer waits for the reveal** — derive the settled word from revealed outcomes rather than
   `_ticket.State`. **Rejected on `T114`'s own grounds:** it cannot see a cash-out at all, and it
   reinstates the state-lie `T121` killed for the window between death and reveal.
3. **Narrow the pin** to exempt a settled ticket. Cheapest, and it deletes the invariant rather
   than satisfying it — the surface would keep saying two things at once, unwatched.

**TV's read: (1).** It is the only one that makes the column tell one story, and §8.10 already
wrote the sentence. Not built — the frame call is this seat's.
