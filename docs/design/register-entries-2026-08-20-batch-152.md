# Register entries — batch 152 (2026-08-20)

**`T149`'s owed frame, taken. It IS shootable — the mechanism is built and I checked that first,
because the last two criteria in this thread died on frames that could not test them.** No window
requested yet: the pre-commitment is written and the conditions are settled first.

**Two rows.** **Destination table:** TV (`T149-am3`, `T157`).

**Pre-commitment:** `docs/design/t149-bust-precommit-2026-08-20.md`, written BEFORE the frames exist.

---

## The survey, run before asking for anything

| | |
|---|---|
| image docks in `dd-import` scanned | **77** |
| carrying a genuine TV lost leg | **1** — `drawn-ending-t129-2026-08-19/arm1`, scene003 |
| that candidate, usable? | **NO, twice over** — see `T149-am3` |
| remedy 1's commit | `11e4ad7`, **2026-08-19 23:16** |
| the candidate's frames, written | **2026-08-19 20:12** — three hours EARLIER |

**One frame was read** (`frame149`) rather than inferred from filenames, and it is what settled the
second failure.

---

## The rows

| T149-am3 | The bust frame is SHOOTABLE — and *not a cash-out* turns out to be NECESSARY AND NOT SUFFICIENT | **CONDITIONS ADDED, CRITERIA UNCHANGED — DD 2026-08-20 batch 152, written before the frames exist. `T149-am` states the three criteria *"stand exactly as written"* and they still do; nothing here re-authors them.** **FIRST, THE CHECK THAT SHOULD HAVE COME FIRST TWICE BEFORE AND DID THIS TIME (`C59`): THE MECHANISM IS BUILT. `TvSweatScreen.cs:3091-3092` reads `Strike.enabled = _cashOutPreview OR ticketSettled`, with `settledDead` requiring `_ticket.State == TicketState.Lost` AND `revealedLoss` (`:2966-2968`), and the commit's own comment says the leg *"stays struck once the ticket actually settles, BY CASH-OUT OR BY BUST"* (`11e4ad7`, 2026-08-19 23:16). **THIS IS THEREFORE NOT `T148-vf`'s CASE** — that criterion could not be asked at any exposure because the rung did not exist. This one can.** **SECOND, THE DOCK SURVEY, NEGATIVE AND WORTH THE RECORD: 77 image docks scanned; ONE TV candidate carries a genuine lost leg — `drawn-ending-t129-2026-08-19/arm1`, scene003, `grammar-LegFinalLost`. IT FAILS TWICE. (a) ITS FRAMES PREDATE THE MECHANISM BY THREE HOURS (written 20:12; `11e4ad7` at 23:16), and `frame149` read at this seat confirms the pre-remedy state on its face — the footer still says `RISK $25` and `PAYS $58` on a ticket that paid nothing, which is `T121`'s DEFECT rather than its fix. (b) **AND IT WOULD HAVE FAILED ANYWAY, BECAUSE THE TICKET HAS ONE LEG.** The set's own README names arm 2 as *"the only two-leg ticket in the set"*, and `frame149` shows `MIDDLEMEN ML +132 L` with `LEG 1 — DEAD` and NOTHING BENEATH IT.** **THAT SECOND HALF IS THE FINDING AND IT SHARPENS THE PIN. `T149-am` pinned the case to a bust on the ground that a cash-out has no lost leg to confuse the struck ones with. TRUE, AND INCOMPLETE: **A BUST WITH NOTHING AFTER THE LOSER IS EXACTLY AS UNTESTABLE AS A CASH-OUT** — the struck rows are the legs BEHIND the loser, and a one-leg ticket has none. *Not a cash-out* is NECESSARY AND NOT SUFFICIENT, and no ruling in this thread said so until now.** **FIVE BINDING CONDITIONS, in the pre-commitment: (1) at least two legs AND THE LOSER IS NOT THE LAST; (2) the loss is REVEALED in the frame, not merely settled in the engine — a burst opened at the bust will contain pre-reveal frames and those are not defects; (3) `C55` — the lost row and at least one struck row IN ONE FRAME, both legible, because the comparison IS the criterion and two frames cannot make it; (4) the chrome row and footer in frame, reading `STAKE` and `RETURNED $0` — if it still reads `RISK`/`PAYS` the settled branch did not run and the frame is not of this subject (`T133-am2`, restated); (5) forcing disclosed in the filename (`S3`).** **A RECIPE OFFERED, NOT IMPOSED — the lane owns the route, and this exists so the window is not spent searching: `GOALLESS-5` ALREADY SUPPLIES BOTH HALVES ON FRAMES THAT EXIST — a `DRAW` leg WINS on that seed (`DRAW +243 W`) and a team-backer leg LOSES on it (`MIDDLEMEN ML +132 L`), and the footer dock's own `S1` ticket carried THREE legs on the same seed. A ticket of MIDDLEMEN ML then DRAW then DRAW busts on leg 1 with two unplayed legs behind it, which is criterion (2)'s state on the seed and matchup already in use** | batch 152 |
| T157 | The cancelled row's BLANK CHIP — `T121`'s deferred copy call lands on this frame, and it is the specific way criterion (2) can still fail | **RAISED and PRE-COMMITTED, not ruled — DD 2026-08-20 batch 152, from a source read, before the frame exists.** **`T149`(3) recorded an expectation that criterion (2) would PASS. THAT EXPECTATION STANDS AND THE SOURCE READ STRENGTHENS IT: four independent channels separate a lost row from a cancelled one — text tier L1 against L2, chip `L` against BLANK, row background extinguished against not, and the strike off against on. `:3021` states the intent in the build's own words: *"the strike belongs to VOID and to nothing else; a struck W or L would read as cancelled, which is the one thing the strike must never say."*** **AND THE ONE WAY IT CAN STILL FAIL, WHICH IS WHY THIS IS A ROW AND NOT A LINE IN THE PRE-COMMITMENT: THE CANCELLED ROWS ARE THE ONLY ROWS ON THIS SURFACE THAT CARRY NO STATE WORD AT ALL. Every other row has one — `W`, `L`, `VOID`, `NEXT`. The build chose silence deliberately (`:3078-3081`: *"the chip falls silent rather than being re-authored — the strike is already this surface's mark for a cancelled leg and no new string is invented here"*) AND NAMED ITS OWN DEFERRAL IN THE SAME BREATH: *"`T121` left the dead ticket's copy to a frame."* **THIS IS THAT FRAME.*** **THE RISK STATED SO A FRAME CAN SETTLE IT: A STRIKE IS A MARK; A MISSING WORD IS AN ABSENCE. If the blank chip reads as *nothing happened to this leg* rather than as *this leg was cancelled*, criterion (2) fails in a way the strike CANNOT rescue, because the reader is being asked to infer a state from the absence of a label on a column where every other row is labelled.** **NOT RAISED AS A DEFECT AND NOT A PROPOSAL: no string is authored here, `T121`'s deferral is correct, and the silence may well read exactly as intended. What is recorded is that the copy call is LIVE ON THIS FRAME rather than settled by remedy 1, so it is read deliberately instead of noticed afterwards** | batch 152 |

---

## For the orchestrator

- **One capture window requested**, narrow: the conditions and the recipe are in
  `docs/design/t149-bust-precommit-2026-08-20.md` §2. **The mechanism is already built and shipped
  (`11e4ad7`)** — nothing is owed from TV before the shutter.
- **`T157` is free** — checked against the tables plus the full backlog (batches 137–152).
- **Batch 151's two pipe repairs are still owed** before transcription (`C56-am2` and `K17`,
  batch 144).

## Limits of this batch

- **Nothing is ruled about how anything reads.** Both rows are conditions and expectations; the
  reading happens on the frame.
- **One frame was read, at one exposure** — `frame149` of the drawn-ending set — and it was read to
  establish leg COUNT and the pre-remedy footer, nothing else.
- **The recipe is a belief about the lane's harness, not a measurement of it.** The two halves it
  rests on are evidenced on existing frames; that a ticket can be composed of them on that seed is
  the lane's to confirm.
