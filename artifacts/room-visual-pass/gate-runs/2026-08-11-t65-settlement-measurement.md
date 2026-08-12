# T65 — the settlement re-tint, measured in the correct space

**Room lead · 2026-08-11 · desk work, no editor.** Batch 34 routed T65's owed settlement capture to
the room and TV lanes after declining to measure it at the DD's seat (wrong space). Measured against
TV's Phase T before-set (`dd-import/tv-phase-t-before-2026-08-11/`, 151 frames at `233bf7a`) and
against the authored value in source.

**Headline: the capture exists and the glow fires as ruled. The re-tint's hue was derived in HSV, and
in CIELAB it is 125.7°, not 88°.** The band it must hit is a CIELAB band.

---

## 1. The authored value — this is the C33-am3 measurement

`TvSweatScreen.roomSettlementWarm = (0.818, 1.000, 0.610)`, `roomSettlementIntensity = 0.9`.

| space | hue | chroma / sat | in the ruled 85–92° band? |
|---|---|---|---|
| **HSV** — the space the derivation used | **88.0°** | sat 39 % | yes |
| **CIELAB from linear authored — C33-am3** | **125.7°** | chroma 26.62, L\* 97.36 | **no — GREEN (110–175°)** |
| CIELAB if the triple were sRGB instead | 126.0° | chroma 52.86 | no |

**The 88.0° is arithmetically an HSV hue**, and the source comment shows its own derivation:
`60 × (2 + (B−R)/(max−min))` on (0.818, 1.000, 0.610) returns exactly 88.0. That is the hexcone
formula, not a CIELAB hue angle.

**The result is robust to the one thing I was unsure of.** Whether the triple is read as linear or as
sRGB changes the answer by **0.3°** (125.7 vs 126.0) — both far outside 85–92°. So the verdict does
not depend on resolving the authoring-space question.

**The band is CIELAB.** Through the same converter (`linear_to_lab`, the room's own, shared with the
emission instrument), a representative warm room emission `(0.038, 0.032, 0.024)` returns **83.3°** —
the family T65 cites as the room's palette (lid 85.1–85.3°, phone 85.4°, laptop 84.3°). A colour at
125.7° is not in that family; it is green.

## 2. The frames — what they do and do not settle

Region: the room's **plaster wall**, box `(148, 350, 186, 1100)`, sd/mean **0.111**, and
**eye-confirmed** (visible plaster relief). Three earlier candidate boxes were rejected: each
straddled the dark surround, a smooth lit edge band, and the wall — sd/mean 0.22–0.52, and the
picture showed why. Averaged in **linear**, converted once at the end.

| group | n | L\* | chroma | hue | Rec.709 luma (display-encoded) |
|---|---|---|---|---|---|
| rest (14 sampled non-payoff frames) | 13 | 15.06 | 9.84 | 167.2° | 37.12 |
| `t68am-accept-slot` peak | 30 | 18.93 | 12.60 | **142.6°** | 45.05 |
| `t71-win-tally-slot` peak | 30 | 18.93 | 12.58 | **142.6°** | 45.03 |

Within-beat ranges: chroma 11.5–12.6, hue 132.3–149.1°.

**What this settles:** the glow **fires on the two settlement beats and nowhere else**, it moves the
wall by a real amount (L\* +3.9, chroma +2.8, hue −24.6°), and the two beats agree to two decimals —
**T65 built as ruled on the firing question.** Note both beats are `grammar-BreakawayAgainst` and
`grammar-LegFinalLost`; the glow is keyed to the settlement **moment**, not to a win, which is clause
4's *"fires on settlement, not per leg"* holding.

**What this does NOT settle, and I am not claiming it does:**

1. **The cast-band check needs TV's own box.** The derivation's numbers came from a `housing above
   panel` region; mine is the plaster wall. Different regions on one frame legitimately give
   different hues, and comparing across them is the two-instruments error this lane has already paid
   for twice. **TV owns that box** — this is the half of the joint task that is theirs.
2. **The absolute hue in these frames is confounded by the panel's own green.** `TvLight` is the C2
   green and it is live in every one of these frames; the R23 recipe disables it precisely so a
   surface's own cast can be separated from the screen's. So the rendered **deltas** above are sound
   evidence of firing; the rendered **absolutes** are not a clean read of the room's cast.

## 3. What is routed, and to whom

**To TV, jointly:** the cast on the derivation's own `housing above panel` box, and one question —
**does V6's gate print HSV hue or CIELAB hue?** The source comment says the gate is what bounds the
value (*"V6 catches both edges because it prints the hue"*). If it prints HSV it reads 88.0° and
passes; if CIELAB it reads 125.7° and fails. **A gate that passes in one space and fails in another
is not bounding anything**, and this is checkable at a desk before the after-set is shot.

**To the DD:** T65's provisional value in the correct space is **hue 125.7°, chroma 26.62** (linear
authored, × 0.9 intensity: 125.7° / 25.70 / L\* 93.44). Whether that discharges or re-opens clause 4
is a ruling, not a measurement, and it turns on §3's gate question.

**Not proposed, not touched:** nothing here changes C2, T9, T10 or T61. The T80 freeze is respected —
C2's green is named only as a confound on the rendered absolutes, which is an observation about *this
measurement*, not a change to that item.

## 4. Scope (C25)

*Reads:* one authored constant at HEAD, and one eye-confirmed plaster region across 57 frames of a
pinned in-room set. *Cannot see:* the cast on TV's own region; any frame outside this set; whether
the amplitude window `[0.78, 1.06]` quoted in source was itself computed in HSV (it sits in the same
comment as the 88.0°, so it should be re-derived in whichever space the gate settles on).
