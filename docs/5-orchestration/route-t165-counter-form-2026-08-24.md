# Route: `T165`'s counter form — TV → DD (2026-08-24)

`T165` (batch 167) moved the counter's referent from the LEG to the FIXTURE and left the form to
this lane: *"THE FORM IS TV's TO PROPOSE"*, with no width asserted — *"a 14.3px deficit that a
shorter counter may or may not close, and **only measurement decides**."*

**Measurement is done. It does not decide — every candidate fits, with room.** So the choice is a
vocabulary question, which is the DD's, and this doc hands it over with a recommendation.

---

## THE MEASUREMENT — EditMode, production face, `EditMode-t165-measure.xml` (322/321/0/1)

`T165_price_the_fixture_counter_candidates_against_the_ticket_header`, report-only, ran and passed
under its own name. Ink math copied from `T91`'s own block rather than re-derived, so these numbers
are commensurable with the ones `T91-cl` ruled on. Font asserted Encode Sans, not the fallback face
(`T20`'s mistake), and measured in canvas-LOCAL space.

`MaxLegs` is 4 and `FixtureCount ≤ Legs.Count`, so `n/m` never exceeds `4/4`; digits are tabular
(`T82`'s atlas working), so `4/4` measures equal to `1/1` and is each candidate's widest form.

| form | ink | clearance to `TicketHeader` | vs the 2px floor |
|---|---|---|---|
| `LEG 4/4` — incumbent | 66.9px | 86.8px | FITS |
| `GAME 4/4` | 84.6px | 69.1px | FITS |
| **`MATCH 4/4`** | **96.5px** | **57.2px** | **FITS** |
| `TELLING 4/4` | 108.2px | 45.6px | FITS |
| `FIXTURE 4/4` | 109.4px | 44.3px | FITS |

`TicketHeader` ink ends at x −386.7; `Leg` is right-aligned with its ink edge pinned at −233.0 and
grows LEFTWARD. **Available ink is therefore ~149.7px**, and the widest candidate uses 109.4px.

### A PREDICTION THIS LANE GOT WRONG, STATED SO IT IS NOT REPEATED

Before the run this lane predicted `MATCH` would *"land near zero clearance and likely fail the
floor."* **It clears by 55px.** The error: subtracting an ink WIDTH (66.9px) from a CLEARANCE
(86.8px) and calling the difference headroom. They are different quantities — the clearance already
IS the headroom. The element being right-aligned is what makes the distinction matter.

Recorded because it is `T144`'s lesson recurring: *the design constant would have predicted a fit and
would have been wrong again.* Here the arithmetic predicted a FAILURE and was wrong. **The direction
of the error is not the point; reasoning where an instrument exists is.**

---

## THE RECOMMENDATION: `MATCH n/m`

Not on geometry — four forms clear the floor — but on vocabulary the surface already owns.

- **`MATCH` is shipped player-facing copy on this exact screen.** `THE MATCH ENDS LEVEL` is authored
  in the event strip (`TvSweatScreen.cs:2144`, `:3757`, `T87-am`/`T87-am2`), and the scoreline slot
  is named `Matchup`. The word is established, not invented.
- **`GAME` appears in NO shipped copy** — grepped across `Runtime/**`. It would introduce a second
  word for a concept the surface already names, which is `T94`'s family: two stories about one thing.
- **`FIXTURE` and `TELLING` are engine vocabulary.** `TELLING` in particular is the session
  contract's word and the player has never seen it.
- **It reads as a pair with its neighbour**: `TICKET 2/2` beside `MATCH 3/4` — same slot pair, same
  pattern, same shape.

### WHY THE WORD HAS TO MOVE AT ALL

`LEG n/m` counting fixtures is FALSE on a same-match ticket: four legs, three tellings, a counter
reading `2/3` beside a column rendering four rows. The referent ruling forces the word.

### THE CAVEAT WORTH RULING WITH

**On an ordinary ticket nothing visibly changes but the word** — leg count equals fixture count, so
`MATCH 2/4` says exactly what `LEG 2/4` said. The form only starts telling a different story on a
same-match ticket, which is also the first frame where `LEG n/m` would have been a lie.

So this is a ruling whose *effect* is invisible today — the shape `T164-cl` corrected itself over.
**Stated up front here so it is not later discovered and read as a reason the change was cheap.**

---

## WHAT THE BUILD DIFF OWES, WHICHEVER WORD IS RULED

**THE WIDEST FORM IS HARD-CODED IN THREE PLACES AND THEY MUST MOVE TOGETHER:**

1. the code's format string — `TvSweatScreen.cs:2747` and `:3730`;
2. the `T84` pool — `TvExtentSweep.cs:701`, `{ "LEG 4/4", "LEG 1/4", "LEG 1/1" }`;
3. the instrument's fixture — `TvSweatScreenLayoutGridTests.cs:1437`, `("Leg", "LEG 4/4")`.

**`T158` asserts (3) against (2) — it CANNOT see (1).** Update the pool and the instrument, miss the
code, and the pin stays GREEN while measuring a string the surface can never render. That is
precisely how the fifth phantom (`TICKET n OF m`) survived, and the lane handoff says to assume a
sixth. **This is the sixth, pre-positioned.** The build diff will carry a pool↔code pin — drive the
render, assert the emitted text is in the pool for its slot — closing the edge `T158` structurally
cannot.

**The candidates were deliberately kept OUT of the `T158` fixture table** for the same reason: they
are not in the pool, because the surface cannot render them. Adding them there to make them
measurable would manufacture the phantom the pin exists to catch. Only the winning form earns a pool
entry, in the diff that teaches the code to emit it.

## THE AGREEMENT `T165` ATTACHED, AND IT IS STILL OPEN

`T165`: *"a fixture counter and the §6.7 interstitial at the fixture boundary DESCRIBE THE SAME
EVENT. The boundary is where the counter increments. If they disagree the surface is telling two
stories about one seam."*

**Item `1.1` — the §6.7 interstitial — is still HELD** (its fork-independence argument lapsed when
Allen ruled arm A). Shipping the counter alone creates no disagreement, since there is no
interstitial today either way. Keying the counter on `evt.FixtureIndex` makes the shared referent
STRUCTURAL rather than coincidental, so `1.1` lands against it without a second move.

## NOT CLAIMED

- **No form is ruled by this lane.** The probe logs and asserts nothing about which word wins; its
  own output says so.
- **Nothing is built.** The counter still emits `LEG n/m` from the leg referent at HEAD. The probe is
  held UNCOMMITTED pending the ruling, per Allen.
- **No frame has been shot.** This is a measurement of the live face under EditMode, not a capture.

**Asked of the DD:** the word. `MATCH` recommended; `GAME` is the only other candidate that is not
engine vocabulary.
