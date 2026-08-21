# Register entries — batch 147 (2026-08-20)

**THE LEDGER IS NOT DRY — I SAID IT WAS.** A derived scan of all 482 rows returns eight genuinely
open items I have never opened, and one of them — `T94` — is the same seam as `T140`, has frames it
never had, and carries a consequence for `G1` that is in flight right now.

**Two rows.** **Destination table:** TV (`T94-am`, `T7-am3`).

---

## The rows

| T94-am | `T94` is HALF-RESOLVED by `T140`, its other half is `T140-am`'s residue, and it already has frames it was never shown | **AMENDED — DD 2026-08-20 batch 147; `T94` stays open and its shape is now known.** **`T94` (batch 62) ruled that the column's live row advances to leg N+1 on leg N's resolution (`TvSweatScreen.cs:1652-1653`) while the scorebug holds leg N's fixture until the next leg stages — *"the surface STATES A REQUIREMENT ABOUT ONE MATCH WHILE DISPLAYING ANOTHER"* — and deliberately did not choose between two readings without seeing the beat.** **IT HAS THE BEAT NOW AND DID NOT KNOW: `drawn-ending-t129-2026-08-19/arm2` at f066 shows the column carrying leg 1 RESOLVED (`UNDER 1.5 GOALS +204 W`) and leg 2's NEED ALREADY LIVE (`ONE TEAM BLANKED` / `CLEAN-SHEET PATH LIVE`) while the scorebug reads `FT` and the strip reads `LEG 1 — WON`. I read that frame at `T139` §2.2 and recorded it as *the ending sharing a frame with the next leg's jeopardy* WITHOUT CONNECTING IT TO `T94`.** **BUT IT IS ONLY HALF THE CASE, AND THE DISTINCTION IS THE RULING: arm 2's two legs ride ONE FIXTURE, so the scorebug is not describing a different match — it is describing the same one. `T94`'s defect proper needs legs on DIFFERENT fixtures.** **SO: `T140` RESOLVES `T94`'s SAME-FIXTURE HALF BY CONSTRUCTION. Under one telling per (ticket, fixture) there is no *next leg stages* moment within a fixture — every leg is live throughout — so the column cannot run ahead of the scorebug there.** **THE RESIDUE IS THE MULTI-FIXTURE BOUNDARY, WHICH IS EXACTLY `T140-am`'s FINDING: the interstitial fires per TICKET, not per fixture, so a fixture change inside `PlaySweat()` gets no boundary treatment at all — and `T94`'s desync is what happens in that gap.** **`T94`, `T140-am` and the spec's `D2` ARE ONE SEAM. `T94` owes *"the won-leg and dead-leg beats ON FRAMES"*; `D2` asks for a multi-fixture ticket with the interstitial firing between fixtures. THE SAME CAPTURE DISCHARGES BOTH and neither row knows about the other.** **THE `G1` CONSEQUENCE, LIVE RIGHT NOW: `T94` records that this desync is *"the ONLY reason bare `TO WIN` is unsafe"* and that resolving toward synchronisation would let `G1-am7`'s rung 2 retire. HALF-RESOLUTION IS NOT RESOLUTION — the multi-fixture case keeps the desync reachable, so bare `TO WIN` STAYS UNSAFE and no rung retires.** **`T152`'s forms are unaffected and were checked before this was written: `{SHORT} TO WIN OR DRAW` and `{SHORT} TO WIN BY 2+` both carry the club, so neither depends on `T94` resolving** | batch 147 |
| T7-am3 | The open-items screen returns 24 hits of which ~8 are real — `T7-am2`'s cost, measured | **RECORDED — DD 2026-08-20 batch 147, and it is `T7-am2` quantified rather than asserted.** **`T7-am2` found that the studio records closures as separate `-cl` rows WITHOUT amending the row they close, so *"a reader who finds `T41` and stops has found a blocker."* THIS IS THAT COST WITH A NUMBER.** **Ran the scan a next seat would run — every row carrying an open marker (`OWED`, `FLAGGED`, `NOT RULED`, `RAISED`, `BLOCKS`, `capture owed`) with no closed prefix — across all 482 rows in the five surface tables. IT RETURNS 24. Triaged by hand, ROUGHLY EIGHT ARE GENUINELY OPEN.** **The false positives are of two kinds and both are structural: rows discharged by a later `-cl`/`-vf`/`-am` row that never touched them (`T130` still reads *FLAGGED under `C17`* although `T130-vf` ruled it three batches ago; `T109` still reads *NOT RULED* although `T109-cl` carries Allen's answer; `T102` still reads *NOT RATIFIED* against `T102-am`'s *ACCEPT AS BUILT*), and CLOSURE rows whose own text quotes the debt they discharged.** **SO THE REGISTER'S OPEN SET CANNOT BE DERIVED — IT CAN ONLY BE READ. At 482 rows that is a real cost and it grows with every batch.** **NOT PROPOSED AS A CONVENTION CHANGE: `T7-am2` already declined that as register-wide and Allen's, and nothing here reopens it. What is added is the measurement it lacked — and the honest consequence, which is that THIS SEAT DECLARED THE LEDGER DRY WHILE EIGHT ITEMS SAT OPEN, because it was working from memory of what it had touched rather than from the tables** | batch 147 |

---

## The eight, named so the next seat does not re-derive them

**Genuinely open and unexamined at this seat:** `T63` (cash-out invert-before-label, flagged) ·
`T86` (b) and (c) (tracking strings — one question, one check) · `T91` (the gap between two
neighbours is not a swept property) · **`T94`** (this batch) · `T100` (stats panel composition,
raised) · `T105` (MATCH STATS may no longer be the title, flagged, coupled to `T100`) · `S80` (the
relation statement — four of five parts buildable) · `S74-am2` (pre-committed, frame not yet docked).

**`S74-am2` is checkable today** — it waits on a frame, and this seat has twice found the frame
already docked (`T58-vf`, `T153-cl`).

---

## What is NOT in this batch

- **No ruling on `T94`.** It stays open, correctly — its residue needs the multi-fixture frame and
  `T140`'s build, and `T94`'s own restraint (*"not choosing between two interaction readings without
  seeing the beat"*) still governs the half that survives.
- **No convention change** (`T7-am3`).
- **No work on the other seven** — named, not started.
