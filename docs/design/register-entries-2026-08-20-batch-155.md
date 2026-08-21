# Register entries — batch 155 (2026-08-20)

**`T94`'s same-fixture half is DISCHARGED ON FRAME — the look-ahead reads CORRECT — and the frame
relocates the remaining defect: it is not in the column, it is in the fixture boundary.**
`T94` opened as a question about the column's behaviour. The column's behaviour is right.

**Two rows.** **Destination table:** TV (`T94-am2`, `T158`).

**The frame is one that already existed** — `T94-am` (batch 147) identified it and did not read it as
`T94`'s evidence. Read at this seat, no window spent.

---

## The corollary, run in full first (Allen's instruction)

`T94` scanned against canon, now that batches 137–154 are transcribed: **two own rows** (`T94`
batch 62, `T94-am` batch 147), **two citing rows** (`G1`'s batch cell at `G1-am8`; `T91-am3`,
mine). **No closure hidden anywhere.** `T94` is genuinely open — the first of batch 150's seven
that survived the check.

### `T94`'s line citation is seven days stale, and the mechanism had to be re-found

`T94` cites `TvSweatScreen.cs:1652–1653`. **Those lines are now `TicketCardBeat`'s attract string** —
unrelated code, about a thousand lines of drift. The mechanism at HEAD:

| what | where |
|---|---|
| clock forced to `FT` | `:2049`, first act of `FinalSlam` |
| column advances to leg N+1, which renders LIVE | `:2085–2086` |
| the won-leg / dead-leg beat plays | `:2092` / `:2098` — **inside the desync** |
| scorebug takes leg N+1's fixture | `:3527`, only on leg N+1's first rendered event |

*(Narrative table — three cells. The rows are below.)*

---

## The rows

| T94-am2 | `T94`'s SAME-FIXTURE half — DISCHARGED ON FRAME, the look-ahead reads CORRECT, and the residual defect moves from the COLUMN to the FIXTURE BOUNDARY | **RULED — DD 2026-08-20 batch 155, on `drawn-ending-t129-2026-08-19/arm2`, `grammar-LegFinalWon` frame 066, read at this seat. `T94` (batch 62) deliberately declined to choose between two readings *without seeing the beat*; THE BEAT IS NOW SEEN, and the restraint is discharged rather than overruled.** **WHAT THE FRAME CARRIES, all four zones in one unforced capture: scorebug `MALLARDS 0 — 0 MIDDLEMEN` at `FT`; leg row 1 `UNDER 1.5 GOALS  +204  W`; beneath it leg 2's LIVE pair `ONE TEAM BLANKED` over `CLEAN-SHEET PATH LIVE`; strip `LEG 1 — WON`; footer `RISK $25  PAYS $72`. This is exactly the composition `T94` described from a line number.** **RULED: ON A SAME-FIXTURE TICKET THE LOOK-AHEAD IS CORRECT, AND THE REASON IS NOT AESTHETIC. `CLEAN-SHEET PATH LIVE` IS TRUE OF THE MATCH ON SCREEN — the same 0–0 that just settled leg 1 is what keeps leg 2's requirement alive. The column is not running ahead of the scorebug at all; it is stating the OTHER requirement THIS match carries. And nothing on the frame contradicts anything: three zones agree about leg 1's conclusion and the fourth states what is still owed.** **`T94`'s alternative is refused on the same evidence: holding the NEED on a leg already settled would make the screen say a requirement is live when it has been decided, which `T94` itself called *a state lie of its own* (`T43`'s class). The shipped behaviour is the better of the two AND it is not merely the lesser evil — it is right.** **NOW THE PART THAT CHANGES THE ITEM. `T94-am` drew the distinction that the defect proper needs legs on DIFFERENT fixtures. The frame shows WHY that distinction is the whole thing: what makes the look-ahead legible here is that THE NEED'S SUBJECT IS ON SCREEN. Change the fixture and every word on the frame stays put except that one fact — the scorebug reads `FT` about a match that is finished and is NOT the match the NEED is about, and the player is given a live requirement for a fixture the surface has not introduced.** **SO THE CRITERION IS NO LONGER *which of two readings* — IT IS A PROPERTY: THE LIVE NEED'S FIXTURE MUST BE THE FIXTURE ON THE SCOREBUG. Same fixture satisfies it today, for free. The multi-fixture case fails it, and it fails it in the SCOREBUG, not in the column — the column is doing the right thing in both cases.** **THEREFORE THE REMEDY MOVES: `T94` does NOT want the column to stop looking ahead. It wants the fixture boundary the interstitial does not provide — which is `T140-am`'s finding exactly (*the interstitial fires per TICKET, not per fixture, so a fixture change inside `PlaySweat()` gets no boundary treatment at all*). `T94`, `T140-am` and `D2` remain ONE SEAM as `T94-am` ruled; what is new is that the seam's OWNER is now named — the boundary, not the ticket column.** **STILL OWED, UNCHANGED IN SUBSTANCE AND NARROWER IN SCOPE: the multi-fixture beat on frames, which `D2` discharges. `T140` is with Allen; if it builds, `T94-am`'s by-construction resolution covers the same-fixture case that this row has now settled by evidence instead.** **AND THE `G1` CONSEQUENCE IS UNCHANGED: half-resolution is not resolution, the multi-fixture desync stays reachable, bare `TO WIN` STAYS UNSAFE and `G1-am7`'s rung 2 does not retire** | batch 155 |
| T158 | Two code paths encode OPPOSITE answers to `T94`'s question, and which one the player gets depends on a build toggle | **RAISED — source read, no frame needed and none claimed · DD 2026-08-20 batch 155, found while re-locating `T94`'s mechanism.** **`FinalSlam` (`:2047`) advances the ticket column BEFORE the won-leg or dead-leg beat plays — `_resolvedThrough` and `UpdateTicketColumn(evt.LegIndex + 1)` at `:2085–2086`, the beat at `:2092`/`:2098`. `ResolveBeat` (`:4006`) plays the beat FIRST and advances AFTER, at `:4030–4031`. **THE SAME TWO STATEMENTS IN THE OPPOSITE ORDER, AND THE ORDER IS PRECISELY WHAT `T94` IS ABOUT** — whether the column runs ahead during the beat.** **WHICH ONE RUNS IS DECIDED BY `theaterEnabled`. `PlaySweat` (`:1676–1679`) reads *if (\_stage != null) { yield return TheaterBeat(evt); continue; }* — the `continue` skips `ResolveBeat` entirely — and `_stage` is built only inside `if (theaterEnabled)` at `:5009–5011`. **SO `ResolveBeat` IS NOT DEAD CODE; IT IS THE THEATRE-OFF PATH, and it answers `T94` the other way.*** **AND ITS COMMENT ASSERTS THE POLICY THAT DOES NOT SHIP: `:4031` reads *"next leg reads LIVE once its events start"* — a description of the behaviour where the column waits for the next leg, which is exactly what the theatre does NOT do. A reader checking `T94` against that line would conclude the desync does not exist.** **NOT RULED AND DELIBERATELY: which ordering is right is `T94-am2`'s subject and it has just been answered FOR THE THEATRE on a frame; whether the theatre-off path should match is a question about a configuration this seat has never seen rendered, and `C17` forbids ruling a surface from a source read. What is raised is only that ONE OF THE TWO IS WRONG, that no row records the divergence, and that the comment on the losing path would mislead the next reader of `T94` — as it nearly did this one.** **ROUTED TO TV with the narrow question: is `theaterEnabled` false a SHIPPED configuration or a development toggle? If development only, this is a comment fix and a note; if shipped, the two orderings need reconciling and `T94-am2`'s ruling extends to both** | batch 155 |

---

## For the orchestrator

- **No capture window requested.** The frame was already on disk — the existing-frames rule paying
  out a fifth time this rotation, on a frame `T94-am` had already named without reading.
- **`T94` is now half-discharged on evidence.** The remainder is `D2`'s capture and is gated on
  `T140`, which is with Allen. **Nothing further is askable until that fork resolves.**
- **One narrow question to TV** (`T158`), answerable without a shutter: is `theaterEnabled` false a
  shipped configuration?
- **The DD's open docket is `T91` (queued at TV) and `T94`'s multi-fixture half (gated on `T140`).**

## Limits of this batch

- **One frame, one exposure, one seed.** `GOALLESS-5`, arm 2, the only two-legs-on-one-match ticket
  in the corpus — which is what makes it the same-fixture case and also what makes it a sample of
  one. The ruling is about a composition, not about a frequency.
- **Nothing is claimed about the multi-fixture case from this frame.** Its reading is argued as a
  consequence of one fact changing; that argument is why the capture is still owed rather than a
  substitute for it.
- **`T158` is a source read.** No claim is made about how either path renders.
