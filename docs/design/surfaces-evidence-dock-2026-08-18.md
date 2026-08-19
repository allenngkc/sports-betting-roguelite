# EVIDENCE DOCK — the market surfaces, §8's owed frames

**Shot:** markets-pregame lane, 2026-08-18 · **Spec:** `docs/design/spec-market-surfaces-2026-08-17.md` §8
**Findings routed alongside these:** `docs/design/surfaces-build-findings-2026-08-17.md`

**Frame set:** `artifacts/surething-ui/20260818-074113-568-*` — six states, twelve files (each state
writes a flat `1024x704` and an angled `main-camera 1280x720`). **The frames are deliberately NOT
committed** — capture sets live on disk and the harness is what is versioned.

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
| S1a | `S1a-entry-contents-head` | contents block, head — RESULT · GOALS · CORRECT SCORE · CORNERS · CARDS | item 2 |
| S1b | `S1b-entry-contents-foot` | contents block, foot — CORRECT SCORE through PLAYERS | item 2 |
| S2 | `S2-entry-folio-at-extent` | PLAYERS scrolled to its foot, folio `76–82 of 82` | item 4 |
| S3 | `S3-entry-empty-group-correctscorefloor-0p08-NOT-SHIPPED` | `MULTI SCORER ….. no prices offered` | item 3 |
| S4 | `S4-entry-price-ink-A-toner` | the sheet with the price in toner | item 1 |
| S5 | `S5-entry-price-ink-B-amber` | the same sheet with the price in amber | item 1 |

**§8 item 2's "every destination populated" is covered by the destinations walk**, not by this set.
That walk reads `MarketDestinations.All` and shoots all six; every matchup prices all fifteen kinds
at the shipped config, so it is a full-vocabulary walk by construction.

### Why the contents takes TWO frames

Twenty-one printed lines — six destinations and fifteen markets — do not fit one 378px viewport.
One frame could only ever have shown part of the list, so the pair is required to cover all six
destinations **between them**, and the harness asserts exactly that.

### What the ranges show, checked against the sheet

`RESULT 1–13 · GOALS 14–31 · CORRECT SCORE 32–44 · CORNERS 45–54 · CARDS 55–64 · PLAYERS 65–82`,
contiguous, summing to the `1–82 of 82` the contents header prints. RESULT decomposes into
`MONEYLINE 1–3 · DOUBLE CHANCE 4–6 · HANDICAP 7–10 · WINNING MARGIN 11–13`. **The folio moved**
from `1–6 of 82` at rest to `76–82 of 82` at the extent — derived from the rendered window, which
is the whole claim of §5.1 and `S74-am3`.

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

## S4/S5 — THE AMBER COMPARISON

Same seed, same matchup, same destination, same scroll position, same session. **The only thing
that moves between the two frames is the price column's ink** — `PriceTakesAmber` off and on. Each
half asserts its own ink before the shutter, so the pair cannot silently be the same sheet twice.

**This seat does not decide it** (§4.4 / `S91` half two leaves it to the frame). One observation,
offered as a read and not a verdict:

**In the amber state there are two amber things on screen** — the price column and the `$0`
POTENTIAL PAYOUT figure in the slip. Amber currently reads as *money you might win*; giving it to
prices widens it to *any money figure*. That is the substance of §4.4's own worry — *if everything
is amber, nothing is* — now visible rather than argued. Against it, the seat's recorded lean holds
up in the frame: at one offer per row the amber does land as a single column down the right edge,
which reads as an annotation rail rather than scattered ink.

---

## FINDING 3 IS NOW VISIBLE — the casing

The routed casing inconsistency can be **seen** in S4/S5 rather than only measured: `Moose Jaw
Overheads` and `Denver Plumbers` sit in mixed case directly beneath an uppercase `MONEYLINE`
heading and beside an uppercase `DRAW`. In one full-width column it reads as drift. Pre-existing
and ruled verbatim by `A2`; the lane has not normalised it. See findings §3.

---

## WHAT THIS SET DOES NOT CLAIM

- **No claim that the sheet READS well.** These are `C11` frames for the DD to read; the gates are
  blind to the leaders, the amber and the density at a glance, exactly as spec §7 says.
- ~~The rail's pack is a TTF-replicated measurement, not a live TMP one.~~ **RESOLVED
  2026-08-18: the pack is now measured LIVE** — `672.76 of 700, 27.24px slack`, read off a rendered
  destination tab's own font and logged by the rail test on every PlayMode run
  (`evidence/logs/PlayMode-20260818-000748.log`). The replicated figures were within 0.10px on the
  total though up to 0.98px out on a single label; findings §2a carries both and the delta. **The
  DD may now rule on the number as well as the verdict.**
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
will pass on a frame that does not show it.** Three of this set's six states would have docked as
evidence of nothing.
