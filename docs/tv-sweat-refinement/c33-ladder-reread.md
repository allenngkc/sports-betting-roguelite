# C33 — the brightness ladder, re-read in the ruled unit

**Unit: Rec.709 luma on display-encoded values.** Quoted with every number below, per C33.
**Instrument:** `tools/ladder_read.py`. **Set:** `tv-sweat-capture`, seed 48151623, boost1.4 (2026-08-06,
the current set and the one the DD measured T63 on). **No editor used.**

**Calibration.** The instrument reproduces all four of the ruling's T63 figures exactly — ball 0.902,
scoreline 0.874, band 0.827, ticket column 0.786. That is what makes everything below comparable to
batch 13 rather than a second private scale.

**Zones come from the production grid**, recomputed from `LayoutGrid(980, 550)`'s own constants
rather than measured off a frame. Two independent facts confirm the transcription: `CashOut` comes
out at canvas (0,480)-(265,532), the box the T63 bundle shipped and the DD validated by eye; and the
row height comes out at 69.33px, the "69.3px slot" the T24 re-measure was decided against.

---

## The ladder as it actually renders

`frame000`, cash-out **actionable**, one frame, one method. Headline figure is the 99.9th percentile;
the true max is one pixel and moves with antialiasing.

| element | Rec.709 | max | declared tier | |
|---|---|---|---|---|
| ball (payoff punch) | 0.902 | 0.902 | L4 only at a payoff | correct — C3 arbitration working |
| scoreline (quiet) | 0.866 | 0.874 | L3 quiet | the stable reference; unmoved since T41 closed |
| **event strip** | **0.858** | 0.870 | **L2 context** | **two tiers above its ruling** |
| **cash-out band** | **0.820** | 0.827 | **L4 — the only sustained one** | **fourth on its own surface** |
| risk/pays footer | 0.779 | 0.786 | L2 gold | level with L3 live rows |
| stage / pitch | 0.741 | 0.902 | L1–L2 markings, L3 actors | capped, T41 holds |
| ticket rows | 0.732 | 0.748 | L3 live | |
| momentum tape | 0.681 | 0.772 | L2 label, L1 history | |
| chrome strip | 0.365 | 0.444 | lowest priority | |
| ticket header | 0.123 | 0.132 | L1 structure | |
| substrate (darkest 2%) | 0.085 | — | L0 | consistent with T48's lifted black |

**The designated L4 element is FOURTH, not third.** The ruling had it third because the event strip
was not in the frame of the measurement. The gap to the scoreline is 0.046; to the ball, 0.082.

### Unit conversion, so the old numbers stay translatable

Same elements, same frame, three conventions:

| element | Rec.709 | RGB-average | linear |
|---|---|---|---|
| cash-out band | **0.820** | 0.656 | 0.674 |
| scoreline | **0.866** | 0.867 | 0.722 |
| ball | **0.902** | 0.902 | 0.791 |

Neutral elements read the same in Rec.709 and RGB-average and differ only in linear. **The saturated
warm element is the one that moves** — the band reads 0.164 lower in RGB-average, which is the whole
of the reported-0.21-versus-real-0.047 discrepancy. T41-cl's 0.737 quiet scoreline is the linear
column (0.722 here, different box); **the same element is 0.866 in the ruled unit.**

---

## Findings

**1. T63's root cause is structural, not a value.** The HDR material was assigned to `_tCashOut` —
the money *figure* — and never to `_cashOutField`, the gold field. The field could not be boosted at
all. It was also painted `gold`, the **L3** money colour, while `goldL4` already existed on this
surface for exactly this purpose. Two levels short by construction, which is why no amount of
re-measuring ever found the band at L4: it had never been there.

Separating the two elements in the same zone: **field 0.696, figure 0.827.** The 0.827 the ruling
measured is the figure. The field — what reads as "the band" at four metres — was the dimmest of the
four competitors, not the third-brightest.

Fixed: the field takes `goldL4` and shares the slot's one HDR material with the figure, so one token
moves both. Value unmeasured; if it overshoots the lever is the field's colour, **never the sealed
1.4 boost**.

**2. The event strip renders at the score's ink and alpha.** `_tFlavor.color = flavorColor` at
`TvSweatScreen.cs:1449`, `:1460`, `:1738` — raw, alpha 1.0. `WonLegBeat` paints the same element
`AtTier(flavorColor, TierL2)` citing TV-05. **One element, two treatments, and the main path is the
loud one.** Measured 0.858 against the scoreline's 0.866.

**RULED, batch 14: the strip goes L2, every site, one rule — the loud-while-running split is not
intended.** Built as a single painting point (`SetEventStrip`) that applies the tier itself, so hue
stays the caller's and the tier is taken away from the call site. All **14** assignments now route
through it, not only the seven: the other four (VOID intervention, confirmed loss, totem payment,
payment made) were also at raw alpha, and they are resolution states — leaving them loud would have
preserved the exact split the ruling struck. Flagged in the same breath rather than folded in
silently.

Untouched and separately flagged: the goal-scorer branch puts **gold** on the strip, which
contradicts TV-05 as quoted in `ResolveBeat` 1,150 lines below it. Batch 14 ruled the tier, not the
hue.

**3. A gate blind spot worth naming (C18 §4.2).**
`L4_canvas_elements_get_the_hdr_material_and_L3_elements_stay_default` checks which elements carry
the HDR *material*. It cannot see an element sitting at the L4 *tier value* without one — which is
exactly what the event strip does, and exactly why this gate was green while finding 2 was true.
Carrying the material and carrying the tier are different claims.

**4. risk/pays (L2 gold) at 0.779 sits level with the L3 live rows at 0.732** and above the tape.
Consistent with C8's bloom-protected floor, but a two-tier gap that measures 0.047 is thinner than
the tiers imply. Flagged, not acted on.

---

## Scope (C25)

Reads the **panel only** — blind to the room (that is V6), to which element holds the HDR token, and
to any state the capture did not force. **Resolution (C32):** 8-bit display-encoded input, so one
code value is ~0.004 luma; differences under ~0.01 are not reported as ordering.

**The one gap that matters: no frame in the current set shows the cash-out band holding the L4 token
uncontested.** Across the whole burst the band is actionable only on frames 000/001, and on 000 the
ball holds the token. The band reads 0.820/0.819 on both — identical whether or not the ball has the
token, which is itself the evidence for finding 1. **The band's true L4 value has never been
photographed.** The next capture owes a calm actionable frame.

---

# Part 2 — the ratified tiers, the closed items, and gold's headroom

## 5. The ladder's numbers are ALPHA COEFFICIENTS, not luminances

`AtTier(Color c, float tier)` does one thing: `c.a *= tier`. So `L4 1 · L3 0.7 · L2 0.4 · L1 0.15`
are **alpha multipliers applied to a per-element ink**, and the luminance that results depends on the
ink as much as on the tier. Two elements at the same declared tier land at different luminances
whenever their inks differ, and they do — `flavorColor` 0.942, `contextGrey` 0.527, `structureGrey`
0.149, `goldL2` 0.422, all Rec.709.

Measured, this is visible immediately: `structureGrey` at L1 lands **0.123**, while `goldL2` at L2
lands **0.779** — a nominally *higher* tier reading six times brighter. Both are behaving exactly as
written.

**This matters now because C33 ruled the ladder's unit to be Rec.709 luma, and the ladder's own table
is in a different quantity.** §2 of the owning document presents `L4 1.00 / L3 0.70 / L2 0.40 /
L1 0.15` as brightness values. They are not brightnesses; they are coefficients that only coincide
with brightness for elements sharing one ink. **Not raised as a defect** — the build is doing what
canon says — but a tuning target of the form "put this at L2" and a measurement of the form "this
reads 0.40" are not the same instruction, and after C33 they will read as though they are.

## 6. T41-cl (stage capped) — re-read, NOT re-opened

Stage zone, Rec.709, across 16 frames. The distribution is what matters, because area separates
markings (many pixels) from objects (few):

| statistic | value | what it is |
|---|---|---|
| P50 | 0.089 | the dark ground — the cap working |
| P90 | 0.095 | still ground: 90% of the stage is unlit |
| P99 | **0.704** | the markings |
| P99.9 | 0.729–0.745 | |
| max | **0.902–0.957** | a handful of pixels |

**Two things the closed record cannot settle, both stated rather than acted on.**

*(a)* The peak reads **0.945–0.957** on grammars containing no payoff, above T41-cl's closed
0.880–0.905. Located: canvas ~(599, 297) on `CalmPossession`, a ~32px neutral cluster at
`(0.953, 0.953, 0.953)` — ball-sized and ball-placed; and on `GoalFor` an arc of clusters spanning
canvas y 211–331 at 0.945, ring-shaped. **These are neutral (sat 0.0%), so the unit change does not
explain the gap** — in Rec.709 and RGB-average a neutral pixel reads the same. It is either
capture-set drift or a real change. **T41 is the DD's closed item and C31 makes a named closing set
exhaustive, so this is surfaced, not re-opened.** Note the scale: 32–127 frame pixels out of ~1.6M.

*(b)* T41-cl closed on "**zero saturated pixels** in any region of any frame". This instrument counts
**~32,000** stage pixels above sat 0.30 with a 0.40 luminance floor, on every frame. That is not a
contradiction — the team dots are ruled muted-but-saturated (T42 measured blue 0.483, pink 0.353), so
any sat>0.30 test over the whole stage must count them. **It means the closed claim is not
reproducible without its region and its threshold, and the record states neither.** C18 §4.2: a gate
that does not state its criterion cannot be re-run, only believed.

## 7. T58 (neutral goal flash) — re-read, ordering survives

Scoreline zone, Rec.709, across the goal burst:

| | Rec.709 P99.9 | max | saturation |
|---|---|---|---|
| quiet (7 frames) | 0.866 | 0.874–0.878 | 4.7–4.8% |
| **flash (`frame000`)** | **0.902** | **0.910** | **0.0%** |

**T58 holds exactly as ruled** — the flash is the brightest frame in the set and is perfectly
neutral. Absolute values shift ~0.02 against the ruling's 0.906/0.886 (different box); **the ordering
and the 0.0% are identical.** Still **one** flash frame in the burst, which is T58's own C25 note
unchanged: one burst is one sample of the flash instant.

## 8. Gold's headroom — the C33 finding in its purest form

Authored constants, both units:

| constant | Rec.709 | RGB-average | delta |
|---|---|---|---|
| `goldL4` | **1.349** | 1.147 | **+0.202** |
| `gold` | **0.844** | 0.717 | **+0.127** |
| `goldL2` | 0.422 | 0.358 | +0.064 |
| `flavorColor` (cold white) | 0.942 | 0.943 | −0.002 |
| `contextGrey` | 0.527 | 0.537 | −0.009 |
| `structureGrey` | 0.149 | 0.150 | −0.001 |
| substrate `#0A0C10` | 0.047 | 0.050 | −0.003 |

**Every neutral reads the same in both units (|delta| ≤ 0.009). Every gold reads higher in Rec.709,
in proportion to its brightness (+0.064 to +0.202).** The superseded unit was accurate for four
constants out of seven and wrong for exactly the three the ration lives in. That is the whole of the
reported-0.21 versus real-0.047 discrepancy, reproduced from the constants alone.

**And it settles T63's fix as the only in-palette option.** In the ruled unit `gold` (0.844) sits
**below** `flavorColor` (0.942). A field painted `gold` therefore **cannot** out-rank the cold-white
scoreline at any boost — the ruling was unsatisfiable by construction, not merely unmet. `goldL4`
(1.349) clears it by 0.407. The fix is not a preference between two plausible values; it is the only
authored colour on this surface that can satisfy the ruling.

## 9. A cost of T65 worth recording

On the leg-win flood frame the room's gold **measurably contaminates the panel's own readings**: the
stage's P50 rises 0.089 → 0.114 (+28%) and the sat>0.30 count at a 0.10 luminance floor jumps from
~43,000 to **634,953** — a 15× inflation that vanishes at a 0.25 floor (36,425, normal).

Two things follow. **Every panel measurement taken on a leg-win frame is biased**, so the pre-T65
capture set should not be used for panel work on those frames. And it is a third instance of the
low-luminance hue trap this slice has now hit three times: at a 0.10 floor the number is nonsense, at
0.25 it is fine. **Test the floor every time; assume nothing in either direction.**
