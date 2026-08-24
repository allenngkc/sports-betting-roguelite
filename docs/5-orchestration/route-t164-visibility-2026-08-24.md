# Route: `T164-cl`'s premise, and where `T163-am`'s direction actually comes from — TV → DD (2026-08-24)

Raised while planning phase 3 step 1 (§6c of `docs/handoffs/theater-engine.md`). Two findings.
**Neither blocks the build** — `T164`'s ruling is untouched by both — but the first changes what
evidence step 1 owes, and the second changes what step 4 has to build.

---

## FINDING 1 — `T164-cl` withdrew a claim that appears to have been true

`T164` (batch 167) ruled the displayed win-probability seeds from the TICKET, and justified landing
it early on the ground that *"it moves no number on any screen shipping today."*

`T164-cl` (batch 169) **withdrew that ground**, on the theater-engine lead's report that the TV shows
the live leg's probability on a multi-leg ticket. The DD's own closing note flagged the exposure:
*"`T164-cl` rests on the lead's report of what the TV shows today, which this seat did not [verify]."*

**Read at the surface, nothing renders it.** The number has no visual anywhere.

| where | what it says |
|---|---|
| `TvSweatScreen.cs:675-678` | `_probTarget` — *"data-only now (RevealedView.WinProbability) — Layout B carries **no standalone win% visual**; DESIGN.md §7's component list has no slot for one, and the ticket column's NEED/LIVE copy is the PRD-sanctioned channel."* |
| `TvSweatScreen.cs:4987-4989` | `_tSubtitle` — *"never shown during a live sweat — DESIGN.md §7's component list has **no standalone win%**/subtitle slot for the live grid."* |
| `SweatFlavor.cs:50-51` | *"a probability is the house's opinion, and a line announcing that it crossed 50% is **the deleted win-prob numeral's** MEANING without its digits."* |

And the consumer set is closed. `WinProbability` occurs in `unity/SBR/Assets/**` in exactly two
files — `TvSweatScreen.cs` and `Tests/PlayMode/LaptopOsTests.cs`. `RevealedView.SetProbability` has
no caller outside `TvSweatScreen`. `SportsbookApp.cs` and `LaptopOs.cs` do hold a `RevealedView`, but
read `HasTicket`, `Tickets` and `MarketSuspended` from it — **never `WinProbability`**.

**So `RevealedView.WinProbability` has exactly one runtime consumer today:**
`TvSweatScreen.cs:4546` — `_audio.SetTension(1f - Mathf.Abs(2f * RevealedView.WinProbability - 1f), …)`,
the crowd-tension bed. It is audible, not visible.

### What this seat is NOT claiming

This is read off the code and off the code's own citations of `DESIGN.md §7`. **This seat has not
read `DESIGN.md §7` itself and has shot no frame.** The claim is *"no site in `Assets/**` renders
this float as type or as a bar"*, which is a closed-set grep result, not a design reading.

### What follows either way

- **The RULING stands and is being built.** `T164` rests on `T143` (*no leg's probability is ever
  shown alone*), never on the change being invisible. `T164-cl` said the same. Nothing here reopens it.
- **What may be wrong is only the SCHEDULING half** — `T164-cl` re-scheduled `T164` as a visible
  change and paired its first multi-leg frame with `T163-am`'s premise evidence. If nothing renders
  the number, **that frame shows nothing and the capture window buys nothing.** Step 1's honest
  evidence is the engine gate plus the EditMode/PlayMode suites, which need no capture.
- **One consequence that IS real, and this lane has absorbed it rather than routed it:** left alone,
  re-pointing `WinProbability` to the ticket would flatten the tension bed on every multi-leg ticket
  (a product near 0.1 sits far from the coin-flip peak the curve is built around). Tension is a
  per-match dramatic fact, like the pitch's territory, so step 1 re-points it to the picked side's
  live leg prob (`DramaEvent.LegProbs`). **No audible change today; correct under N-live.** Recorded
  as a lane decision, not a question.

**Asked of the DD:** whether `T164-cl` should carry a correction of its own. Not blocking.

---

## FINDING 2 — `T163-am`'s direction is NOT the displayed probability, and step 4 must build it

`T163-am` (batch 168) and `spec-neither-branch-lines-2026-08-21.md` §5 both rest on one sentence:

> **There is a single `up`/`down` to select a table ONLY because the displayed probability is the
> TICKET's.** Were the number still seeded per leg, legs in disagreement would have no single
> direction and NO LINE COULD BE WRITTEN AT ALL.

**The premise is right. The mechanism named for it is not the one in the code.** The table-selecting
direction is computed in two places, and neither reads the displayed probability:

| site | how direction is derived |
|---|---|
| `SweatPresentationModel.cs:56-64` | `_prevProb` anchors to `leg.TrueProb` on a leg change; `delta = evt.WinProbAfter - _prevProb`; `up = delta >= 0.0`. Returned as `_lastBeatUp` and stored on every `BeatRecord`. |
| `SweatFlavor.cs:25` | its own, independently: `bool up = e.WinProbAfter >= prevProb`, with `prevProb` seeded from `leg.TrueProb` at `TvSweatScreen.cs:3485`. |

Both are **leg-scoped and read `evt.WinProbAfter`**, which after the restructure is the ANCHOR leg's
number. `RevealedView.WinProbability` is a different quantity on a different path. **Re-pointing the
display, which is all `T164` governs, moves neither of them.** `T163-am` does not come for free with
`T164`; step 4 has to re-base the direction itself.

### The arithmetic, so step 4 is not planned against a guess

`TicketWinProbability` is a product of positive per-leg factors, so it is **monotone increasing in
every leg's probability**. Two consequences:

- **While one fixture is live, re-basing direction to the ticket changes NO `up`/`down`** — the sign
  of the ticket delta equals the sign of the moving leg's delta. The score-attribution law
  (*a Score/BigPlay beat up ⇒ picked-team goal*) survives unchanged on every ticket shipping today.
- **It DOES compress every `delta`**, by the product of the other legs' probabilities. That feeds
  `SweatPresentationModel.MagnitudeBand`, whose 0.04 / 0.10 thresholds were tuned against leg-scale
  moves. On a multi-leg ticket the tape's dots would drop a band. **That is visible**, it is on the
  tape rather than in a numeral, and it is the first thing in this whole area that actually is.
- Under N-live with two legs disagreeing, the ticket delta's sign is the single honest answer —
  which is `T163-am`'s point, restated at the site that will have to implement it.

**Asked of the DD:** nothing yet. Recorded now, while the reading is fresh, so step 4 costs the
re-base and the band question once rather than discovering both mid-build.

---

## PROVENANCE

Read at `60a89f5` + the step-1 working tree. Engine members verified by reflecting the **compiled**
`unity/SBR/Assets/Plugins/SBR/SBR.Engine.dll` (2026-08-23 08:48), not `engine/` source — the trap the
predecessor recorded. Engine gate green at baseline: **324 passed, 1 skipped, 0 failed.**
