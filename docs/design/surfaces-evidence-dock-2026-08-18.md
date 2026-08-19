# EVIDENCE DOCK — the market surfaces, §8's owed frames

**RE-SHOT 2026-08-19** against the surface as batch 113 left it. **Spec:**
`docs/design/spec-market-surfaces-2026-08-17.md` §8 · **Findings:**
`docs/design/surfaces-build-findings-2026-08-17.md` · **S96's measurement:**
`docs/design/s96-casing-measurement-2026-08-18.md`

**Frame set:** `artifacts/surething-ui/20260819-034625-465-*` — **five states, ten files** (each
state writes a flat `1024x704` and an angled `main-camera 1280x720`). **The frames are deliberately
NOT committed** — capture sets live on disk and the harness is what is versioned. The
2026-08-18 set it replaces is deleted, not archived: it shows a surface that no longer exists.

**Why re-shot — every ruling in batch 113 changed the pixels.** `S95` reordered the rail, `S96`
uppercased every row name, `S97` retired the amber half of the pair, `S98` relabelled three markets,
and the price cell narrowed 176 → 160 to pay for `S96`. Suites green before the shutter: **EditMode
303 executed / 302 passed / 0 failed; PlayMode 139 executed / 121 passed / 0 failed.**

**Seed `54435761`, matchup 0 (Denver Plumbers @ Moose Jaw Overheads), for every frame.** Eight
digits and scattered, per `R38`: a seed spelled like a label is a rig string in a player slot. The
state name lives in the filename, never in the seed. One seed across the whole set so the frames
differ only in what they are meant to differ in.

**Harness:** `SureThingVisualCaptureTests.Capture_the_market_sheet_evidence_for_the_surfaces_spec`,
run with `SBR_SHOOT=1`. Executed 1 of 1, passed.

---

## The frames

| # | file stem | subject | §8 item |
|---|---|---|---|
| S1a | `S1a-entry-contents-head` | contents block, head — RESULT through CORNERS | item 2 |
| S1b | `S1b-entry-contents-foot` | contents block, foot — GOALS through PLAYERS | item 2 |
| S2 | `S2-entry-folio-at-extent` | PLAYERS scrolled to its foot, folio at the extent | item 4 |
| S3 | `S3-entry-empty-group-correctscorefloor-0p08-NOT-SHIPPED` | `MULTI SCORER ….. no prices offered` | item 3 |
| S4 | `S4-entry-price-ink-toner` | the sheet as ruled — price in toner | item 1 |

**There is no S5 any more.** `S97` closed §4.4 on the old pair: the price does NOT take the amber.
The comparison is discharged, so a single toner frame stands in place of the two, and the capture
still asserts the ink before the shutter as a regression check.

**§8 item 2's "every destination populated" is covered by the destinations walk**, not by this set.
That walk reads `MarketDestinations.All` and shoots all six; every matchup prices all fifteen kinds
at the shipped config, so it is a full-vocabulary walk by construction.

### Why the contents takes TWO frames

Twenty-one printed lines — six destinations and fifteen markets — do not fit one 378px viewport.
One frame could only ever have shown part of the list, so the pair is required to cover all six
destinations **between them**, and the harness asserts exactly that.

### What the ranges show, checked against the sheet

**In `S95`'s order:** `RESULT 1–13 · GOALS 14–31 · CORNERS 32–41 · CARDS 42–51 ·
CORRECT SCORE 52–64 · PLAYERS 65–82`, contiguous, summing to the `1–82 of 82` the contents header
prints. RESULT decomposes into `MONEYLINE 1–3 · DOUBLE CHANCE 4–6 · HANDICAP 7–10 ·
WINNING MARGIN 11–13`. **The folio is derived** — `1–6 of 82` at rest against the extent's own
reading, which is the whole claim of §5.1 and `S74-am3`.

**`S98` is visible in the same frame and this is what it was for:** `TEAM TOTAL GOALS 20–27`,
`TEAM TOTAL CORNERS 38–41`, `TEAM TOTAL CARDS 48–51` — **three distinct scannable entries** where
the short form would have printed `TEAM TOTALS` three times with three different line ranges.

---

## THE TWO DISCLOSURES ON S3

**1. `CorrectScoreFloor` is 0.08 in that frame. The shipped default is 0.02.** It is in the
filename, ending `NOT-SHIPPED`. It is **not** captioned into the pixels: a caption would put rig
state in a player slot, which is `R38`'s own subject and its third recorded instance. In the
filename it is inseparable from the frame and invisible to a player.

**Why a non-shipped config is needed at all:** `no prices offered` is UNREACHABLE at the shipped
config — measured, **zero empty groups across 18,000 matchups**, because the 0.02 floor always
leaves CORRECT SCORE ≥ 11 rows and MULTI SCORER ≥ 3. `S89` is not undermined by this (§3.1's
constant rail does not depend on the state occurring), but the form cannot be photographed on a
shipped run, and `S57` requires that a capture whose figures are arbitrary say how it was made.

**2. The raised floor thins CORRECT SCORE from 13 rows to 6**, and the sheet totals 71 rows rather
than 82. **Two things differ between S3 and the shipped sheet, not one.** The thinner CORRECT SCORE
is a consequence of the floor, not a defect.

---

## S4 — THE PRICE INK, NOW RULED

**`S97` closed this on the old pair and the ruling is built.** The price stays in TONER. The
`PriceTakesAmber` switch and its `PriceInk` indirection are deleted, and the rebuild-signature term
this lane added to carry the comparison went with them — a signature term for a constant is dead
weight.

**The comparison did its job and is recorded so the frame is not re-litigated:** on the old pair the
amber made the price the most saturated element in the market column and **inverted the name-first
hierarchy `S91` had ratified**; it put two amber things on one screen meaning different things (the
price column and the `$0` POTENTIAL PAYOUT); and eighty amber marks is not marking. **This seat's
recorded lean was *yes*, and the frame overturned it** — which is why §4.4 sent it to a frame
instead of deciding it at a desk.

**Named, not ruled, and carried forward:** amber's real claim is the **SELECTED** price — the moment
a price stops being the house's offer and becomes the player's stake. That belongs with the
selection treatment and is not settled here.

## THE CASING, RULED AND BUILT — and what it cost

`S96` ruled it after seeing the drift on the old pair: title-case club names sat in the same column,
at the same size, beneath an uppercase `MONEYLINE` and beside an uppercase `DRAW`. **On `S4` the
column now reads consistently** — `MOOSE JAW OVERHEADS`, `DRAW`, `DENVER PLUMBERS`, `EITHER TEAM`.

**It was not free, and `C46`'s binding is what caught the bill.** Uppercase is wider per character.
Measured over the whole reachable pool, the longest row name is
`SAN FRANCISCO SPREADSHEETS UNDER 4.5 CORNERS` — **493.69px live**, and it overflowed the old
480px name cell. The DD's named candidate, `MOOSE JAW OVERHEADS OR DRAW`, has 161.94px spare and was
never the worst case: **a spot-check of it would have passed and shipped the defect.**

**Resolved as ruled — the name cell widened out of the price cell's slack**, 176 → 160, because a
176px cell was carrying 46.61px of type (73.5% empty) and its ring stretches rather than breaking.
Collisions 5 → 0, zero-dot rows 17 → 5, under-six-dot rows 139 → 59, headroom now **2.31px**.

**Still open, and not repaired here:** §4.3's leader device is not fully restored — the longest names
still print no leaders, and six dots on the longest would need a 544.68px name cell, i.e. a ~111px
price cell and a ring 30.7% off native aspect. **The arithmetic is available; the design is not.**
Full numbers in `docs/design/s96-casing-measurement-2026-08-18.md`.

**`S84`'s binding is discharged clean** — 320 clubs and 144 players enumerated in-engine as a pool,
not a sample; zero case-dependent names.

## WHAT THIS SET DOES NOT CLAIM

- **No claim that the sheet READS well.** These are `C11` frames for the DD to read; the gates are
  blind to the leaders, the amber and the density at a glance, exactly as spec §7 says.
- **The rail pack is measured LIVE** — `672.76 of 700, 27.24px slack`, read off a rendered
  destination tab's own font and logged by the rail test on every PlayMode run. `S95`'s reorder
  does not change the total, only the seats.
- **§4.3's leader device is NOT fully restored** and this set should not be read as showing it
  intact — 59 rows still print fewer than six dots, all club-prefixed team totals. See the casing
  section.
- **C55 is now LAW**, promoted from this set's own harness fault (below). The in-frame assertions it
  requires are built into this capture and ran green here.
- **S3 is not a shipped state.** See the disclosures.

---

## A HARNESS FAULT FOUND AND FIXED — recorded, because it nearly shipped as evidence

**The first shoot passed and produced two frames that did not contain their own subjects.** S3's
`no prices offered` sits behind fourteen scorer rows and was below the fold; S1's PLAYERS line was
below the fold. Both assertions used `GetComponentsInChildren`, which finds objects that have
scrolled out of view. **Existence in the hierarchy is not the claim a capture makes**, and a green
run said otherwise.

The gate now asserts **in frame**, and the lists are scrolled to their feet. Two further runs then
failed on that stricter gate — correctly, and for a reason worth recording: `IsInFrame` first
compared **world-space** corners. This is a world-space canvas on a physical laptop in a room, so
the entire 704px viewport spans about 0.1 world units and every row rounds together; worse, the
screen is **tilted**, so a world-space horizontal comparison is taken across a rotated plane and
means nothing. It now measures in the viewport's **local space** — the space the layout is authored
in, whose units are the pixels every other constant here is written in.

Recorded because the failure mode is general: **a capture harness that asks whether a thing EXISTS
will pass on a frame that does not show it.** Three of that set's six states would have docked as
evidence of nothing.

**PROMOTED TO LAW as `C55` (DD batch 113)**, with the world-space half named as the part that gets
missed. It now sits in a family of three instrument laws — `C53` a classification with no category
for the truth, `C54` an instrument reporting only the extremum, `C55` an instrument asserting the
wrong predicate — *all three making claims wider than their evidence, all three green while doing
it.* The gate it produced is what carried this re-shoot: every state here asserts its subject is in
frame, in local space, before the shutter.
