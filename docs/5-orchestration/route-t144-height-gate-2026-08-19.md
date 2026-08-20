# Route: `T144`'s §4 height gate — TV → DD (2026-08-19)

Against `docs/design/spec-ticket-footer-2026-08-19.md` (batch 132). Allen ruled the
composition; this is the §3.3 re-derivation its §4 gates, measured at the real face.

## THE VERDICT: THE GATE DOES NOT CLEAR. The composition is NOT built.

§4 says the gate passes **before** the composition lands, and §4.5 says a failure comes back
here with the number. It is short by **4.6px** in the most generous form available and by
**12.6px** in the form the footer is actually built in. Nothing about the composition was
landed; the spec's own §4.5 path is what this doc is.

**What DID ship** (`2f76062`, this branch): the gate instrument itself —
`TvSweatScreenLayoutGridTests.T144_the_two_row_footer_height_is_re_derived_against_the_live_row`,
report-only, riding the EditMode suite — and the stale two-legs-per-matchup clause struck at
`TvSweatScreen.cs:2858`. **The instrument is the thing `T74-am5` did not have** and it now
exists whichever way this is ruled.

## THE MEASUREMENT — EditMode, production face, `EditMode-20260819-215428.log`

Every number below is read off the built objects, never recomputed from the constants the code
under test uses.

| | measured |
|---|---|
| ticket column budget | **480.0px** (header 24.0 + six rows + footer) |
| row pitch today | 69.3px · footer today 40.0px · footer inner box 249.0px |
| **line box at `TypeRisk` 24** | **30.0px — observed ratio 1.25**, not the 1.18 `LineBox` constant |
| two rows, zero padding | **60.0px** |
| two rows, keeping today's 8px top inset | **68.0px** |
| live row ink | **58.8px** (NEED 35.0 + progress 23.8) |
| live row + `T24`'s pinned margin | **66.8px** |
| **the footer's ceiling** | **55.4px** — the largest footer that leaves every live row its `T24` margin |
| deficit, bare | **SHORT by 4.6px** |
| deficit, as built | **SHORT by 12.6px** |

§4.4 is satisfied and worth stating plainly: **the face measures 1.25**, exactly as `T74-am3`
established. The design constant would have predicted a fit and would have been wrong again.

### Where the 4.6px actually lands

Per leg row, not in aggregate: at a 60px footer each row is **66.0px** against live ink of
**58.8px** — **7.2px of clearance where `T24`'s pin demands 8.0px.** Short by **0.8px per row**,
six rows. That pin exists because a knife-edge clips glyphs on the real font, which no headless
run rasterises anything to reveal.

### The same deficit expressed as type, since §3.3 leaves the source open

- Two rows inside the 55.4px ceiling need **~22.2px type**, not 24 — and only with the footer's
  8px top inset removed entirely.
- **Keeping any top inset needs ~19px type.**
- §5 reserves the type size for this seat, so neither is TV's to take.

## THE OPTION SPACE, WITH COSTS NAMED — none of these is TV's to rule

1. **Sign a `C16` deviation.** Land at a 60px footer with no top inset; named cost **0.8px per
   leg row of `T24`'s 8.0px margin, six rows**; expiry the deferred sizing pass. Note this also
   spends the footer's top inset, so the money rows abut leg row 6 — a composition change in its
   own right, not a padding tweak.
2. **Drop the money type 24 → 22** (§5's own §4 ruling). With zero top inset this clears by
   **0.4px** — the only arrangement measured that passes no pin. 0.4px is itself a knife-edge.
3. **Take it from the header** (24px, ink 18.8 in a 20px box). Header 24 → 20 frees 4px and
   leaves the bare form **still short by 0.6px**. It does not close alone.
4. **Relax `T24`'s margin** 8.0 → 7.2. A pin change, and the pin is the anti-clipping contract.
5. **Refuse the composition.** §2's argument that no cheaper lever reaches the fact floor is not
   contradicted by anything measured here — the pair genuinely cannot share one row
   (`RISK` 138.4 + `PAYS` 239.7 = 378.1 against 249.0).

## A SECOND FINDING THE GATE PRODUCED — §2's full-width claim is word-dependent

§2 says separate rows lets both facts clear their enumerated worst case at full width. Measured
against the 249.0px box:

| string | width | |
|---|---|---|
| `RISK $13,639` | 138.4px | fits |
| `STAKE $13,639` | 158.9px | fits |
| `PAYS $73,318,376,502` | 239.7px | fits, 9.3px spare |
| `PAID $73,318,376,502` | 235.8px | fits, 13.2px spare |
| **`RETURNED $73,318,376,502`** | **300.9px** | **OVERRUNS by 51.9px** |

**`RETURNED` does not fit its own full-width row.** Separate rows fixes the *pair* collision;
it does not save the word that ships in the settled state today. So §2's claim holds for `PAYS`
and for `PAID`, and not for `RETURNED` — and `T133`'s copy ruling therefore still binds this
composition, not merely the one it replaces. (For completeness: `RETURNED` would need ~19.9px
type to fit 249.0, which is below even the height answer.)

The four widths this seat could cross-check against the record — 138.4, 239.7, 235.8, 300.9 —
reproduce it exactly. The instrument agrees with the sweep.

## A DEFECT IN §3.1's CITATION — the ruling stands, the reason does not

§3.1 rules both rows left-anchored and supports it by "matching the money control's two members
(`:5468` — *Anchors are left exactly as they were*)". **That comment means the anchors were left
UNCHANGED, not that they are left-ALIGNED.** In the code the two members are opposite:

- `TvSweatScreen.cs:5459-5462` — `CashOut`, pivot (0, 0.5), **`TextAnchor.MiddleLeft`**
- `TvSweatScreen.cs:5471-5474` — `CashOutStatus`, pivot (1, 0.5), **`TextAnchor.MiddleRight`**

So the money control is not a left/left precedent; it is the precedent for **keeping opposite
anchors when moving two members onto separate rows** — the reverse of what §3.1 uses it for, and
consistent with §3.1's own observation that `T74-am3` declined to name an alignment.

**This does not disturb the ruling.** §3.1's primary argument — that on separate rows there is
no shared gap, so `T74-am5`'s opposite-anchor device has no subject — stands on its own, and the
seat named the alignment deliberately. TV will build left/left as ruled. The citation is routed
so the register does not carry a precedent that points the other way.

## WHAT TV IS HOLDING

The composition, entire, pending this seat's ruling. Evidence `E1`/`E2`/`E3` is not shot: there
is nothing to shoot until a composition lands, and `E3` in particular — a live leg row in the
same frame as the footer — is the frame that would show the cost this doc is about.
