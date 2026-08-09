# TV — the event strip's tier. **QUESTION, not a violation claim.**

**From:** TV sweat lead · **Filed:** 2026-08-07 · **Rides:** next inbox push
**Surfaced by:** the C33 ladder re-read ordered in batch 13 · **No editor used**

**This is filed as a question because the canon does not answer it.** `tv-design.md` §8 rules the
event strip's *content and voice* at length and never assigns it a **tier**; §3's token table lists
`--tv-fact` (score, clock, live leg names, market lines) and `--tv-context` (labels, odds, risk/pays,
pitch markings) and the strip is in **neither**. So there is no rule here to be in violation of, and
the seat is not treating the louder reading as a defect on its own authority.

---

## 1. The measurement, and what it cannot see (C25)

**Unit: Rec.709 luma on display-encoded values (C33).** Instrument `tools/ladder_read.py`, zones from
`LayoutGrid(980,550)`'s own constants. Set `tv-sweat-capture`, seed 48151623, boost1.4 — the current
set, and the one batch 13 measured T63 on. Calibration: this instrument reproduces all four of the
T63 figures exactly (ball 0.902, scoreline 0.874, band 0.827, column 0.786).

`frame000`, cash-out actionable, 99.9th percentile:

| element | Rec.709 | hue | sat | declared tier |
|---|---|---|---|---|
| ball (payoff punch) | 0.902 | — | 0.0% | L4 at a payoff |
| scoreline (quiet) | 0.866 | 202.2° | 4.8% | L3 quiet |
| **event strip** | **0.858** | **199.2°** | **4.3%** | **not ruled** |
| cash-out band | 0.820 | 57.4° | 75.9% | L4, the only sustained one |

**The strip renders within 0.008 of the scoreline** — below this instrument's ~0.01 resolution, so
the honest statement is that **the strip and the score are not separated at all**, not that one
narrowly leads.

**What this measurement cannot see:** one frame, one seed, one grammar. It reads the panel only —
blind to the room. It cannot see which element holds the HDR token, and it cannot see whether the
strip's line on this frame is a long one (a short line has less ink but the same peak, so the peak
statistic is stable — that is why it is the one quoted). It does not establish that the strip *reads*
as competing with the score at seated distance; that is a rendered-evidence judgement (C11) and it is
the DD's, not this instrument's.

## 2. What the build does — the complete inventory, not the first three hits

`_tFlavor.color` is assigned at **14 sites**. Split:

**Tiered to L2 — 3 sites, all leg-resolution beats, all citing TV-05:**

| site | beat |
|---|---|
| `ResolveBeat` (VOID branch) | `LEG k — VOIDED, THE TICKET LIVES` |
| `WonLegBeat` | `LEG k — WON` |
| `DeadLegBeat` | `LEG k — DEAD` (on `contextGrey`) |

**Full-brightness `flavorColor`, alpha 1.0 — 7 sites, all match narration:**
the main `RenderEvent` path (two), `VAR — NO GOAL`, the goal-scorer line, the leg-reinstated line,
the idle/attract line, and the takeover card. (`flavorColor` is `(0.90, 0.95, 0.98, 1f)` — the same
ink and the same alpha the scoreline uses.)

Remaining sites are hue decisions already ruled: gold for money, `contextGrey` for a confirmed loss,
`chromeCyan` for VOID.

**The split is not random.** Every beat that *resolves a leg* dims the strip; every beat that
*narrates the match* does not.

**But it is probably not deliberate either, and the file says so in its own words.** `AtTier`'s
doc comment records the TV-S1 defect it was written to fix:

> "The ladder ... was declared here but applied to exactly one element, so score, clock, NEED,
> progress **and the event strip** all rendered at identical maximum brightness and the ladder
> carried no hierarchy at all. Every slot now states its tier at the point it is built."

**The strip is named in that list.** The three tiered sites all cite TV-05 and all landed later, with
the leg-resolution copy work. So the likeliest history is that TV-S1's sweep reached score, clock,
NEED and progress, the resolution beats were tiered afterwards for a different reason, and the seven
narration sites were simply never revisited — the strip is the one element in its own fix's list that
the fix did not finish.

That is evidence, not a ruling, and it does not settle *which* tier. It does mean the DD should
probably not weigh "the split looks intentional" very heavily.

## 3. The question

**Does the event strip sit at L2 like the beats that resolve a leg, or at the score's level like the
beats that narrate the match?**

- **If L2** — the seven narration sites take `AtTier(flavorColor, TierL2)` and the strip drops from
  0.858 to roughly 0.40. One-line change per site, one rule. Consequence: the strip becomes markedly
  quieter than the score, which is consistent with §2's "brightness is the primary semantic channel"
  and with §4.1's "nothing outgrows the score", and it removes a sustained competitor from the top of
  the ladder.
- **If the score's level** — the three resolution beats are the drift, and they should come *up*, not
  the seven come down. Consequence: the surface carries two sustained elements at its top brightness,
  which is a live question against §2's "at most one L4 element exists at any instant" — though note
  the strip carries no HDR material, so it can reach the L4 *value* but never exceed 1.0.
- **If it is deliberately split** (loud while the match runs, quiet when a leg resolves) — then it is
  a rule and belongs written down, because nothing currently records it and the next edit will pick
  one arm by accident.

**The seat's read, offered and not acted on:** L2 for all seven. It matches what the three
resolution beats already do, it matches TV-05's "the strip stays neutral", and it is the only arm
that leaves the ladder with a single element at the top. But dimming a major readable line by two
tiers across the whole match is a visible change to the surface's register, and §5 of this seat's
contract puts that with the DD.

## 4. What is NOT blocked

**T63 proceeds regardless and is already built.** The band's fix is structural — the L4 boost was
wired to the money *figure* and never to the gold *field*, so the field could not be boosted at all
and was additionally painted in the L3 gold. Once the band clears the quiet scoreline as ruled, it
clears the strip too, since scoreline > strip. **This question changes nothing about T63's fix and
does not gate the editor window.**

## 5. One gate blind spot, worth naming (C18 §4.2)

`L4_canvas_elements_get_the_hdr_material_and_L3_elements_stay_default` has been green throughout. It
checks which elements carry the HDR **material** — and it explicitly asserts the strip does *not*,
with the comment "only one L4 element at a time".

**Carrying the material and carrying the tier are different claims.** An element without the material
can still sit at the L4 tier *value* (alpha 1.0); it simply cannot exceed 1.0. That is precisely what
the strip does, and it is why a gate written against this exact risk read green while the condition
it describes was true. Recorded here rather than quietly fixed, because the same gap will exist on
any surface whose gate checks materials instead of composited luminance.
