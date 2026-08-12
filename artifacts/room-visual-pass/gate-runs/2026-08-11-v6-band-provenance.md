# V6's band — where its two reference figures came from, and in what space

**Room lead · 2026-08-11 · desk work, no editor.** TV answered that V6 computes HSV and asked
whether its band's reference figures were established in CIELAB. **They were.** The provenance is in
this lane's records and the conclusion is reproducible from source in three independent ways.

---

## 1. Answer

| figure | where it entered the record | space |
|---|---|---|
| **laptop lid 85.1–85.3°** | `register-entries-2026-08-07-batch-13.md:319`, a **blockquote of this lane's own measurement**: *"all three warm builds are identical: **chroma 12.3–12.4** at hue 85.1–85.3°"* | **CIELAB** |
| **warm key ~92°** | `register-entries-2026-08-07-batch-13.md:285`, same ruling, same paragraph, uncited inline | **CIELAB** |

Both then travelled verbatim into `tools/v6_room_region.py:85-88` as the comment above
`BAND_LO, BAND_HI = 85.0, 92.0`.

## 2. Three independent lines, because one would not be enough

**(a) Vocabulary.** Every room quotation of these figures pairs the hue with a **chroma**
(12.3–12.4; 68.97 → 0.24). CIELAB has chroma; HSV has saturation, and the record says *saturation*
only where it is quoting the TV-side defect figure (*"40.5° amber at 71.4% sat"*). The two
vocabularies never mix inside one lane's numbers.

**(b) Instrument.** The room lane's only hue instrument is `linear_to_lab` in
`tools/room_gate_check.py` — CIELAB, shared by the emission instrument and the R23 cast. A search
for HSV across the room's tools and Editor code returns **exactly one** hit: `v6_room_region.py`
itself, added by **TV** at `97350ae`. The room lane has never computed an HSV hue.

**(c) Computation from source.** Reconstructing both constants:

| source constant | recorded | CIELAB | HSV | miss |
|---|---|---|---|---|
| `FluorescentKey` tube `(0.92, 0.86, 0.42)` | ~92° | **102.7°** | 52.8° | Lab 10.7° vs **HSV 39.2°** |
| `GrantedLidEmission` `(0.038, 0.032, 0.024)` | 85.1–85.3° | **83.3°** | 34.3° | Lab 1.9° vs **HSV 50.9°** |

CIELAB lands within 1.9° on the lid and 10.7° on the key; HSV misses by 39–51°. Those are not
measurement discrepancies, they are a different space. The residual Lab gaps are the expected
authored-vs-rendered difference — the record measured the **rendered cast**, not the constant.

## 3. The self-test that settles it

**Feed V6's band the two colours the band was derived from. Both fail it.**

```
warm KEY tube      (the ~92° source)        V6 reads HSV 54.0°  ->  OUT OF BAND
laptop LID         (the 85.1–85.3° source)  V6 reads HSV 36.2°  ->  OUT OF BAND
```

A gate whose own reference points fall outside its own band is not mis-calibrated. **It is reading a
different quantity than the one its bounds describe.**

## 4. The consequence is larger than "suspect", and it is directional

The band is **not a shifted or loosened version** of the intended one — it selects a **disjoint**
region of colour space. Going the other way: HSV 85–92°, what V6 admits, is **CIELAB ≈ 124–128° —
green**. The settlement colour demonstrates it exactly: `(0.818, 1.000, 0.610)` reads **HSV 88.0°,
comfortably in band**, and **CIELAB 125.7°, green**.

So V6, as written, **passes green casts and rejects the warm ones T65 clause 3 exists to protect**.
TV's "every in-band verdict is suspect" is right; the sharper statement is that in-band and
out-of-band are close to inverted for the warm/green question the band was drawn for.

## 5. On the smallest fix

TV names it correctly — V6's print line should name its hue space the way line 91 already names its
luma unit (*"C33's unit: Rec.709 luma on display-encoded values"*). **The asymmetry is visible inside
the file itself**: the luma unit is named, the hue space is not, and that is precisely where the
error entered.

**Naming is necessary and not sufficient.** A label makes the mismatch visible; it does not make the
bounds correct. Whichever space is chosen, one of the two halves has to move:

- **Convert the reading** — V6 measures in CIELAB via the room's existing shared `linear_to_lab`. The
  bounds stay exactly as ruled, and V6's numbers become comparable with every other room palette
  figure in the studio. *My recommendation, and it is a recommendation, not a ruling.*
- **Re-derive the bounds in HSV** — keeps V6's code, but the new numbers are no longer the ones T65
  clause 3 cites, and every room palette figure remains incomparable with V6's output.

Either way the amplitude window quoted in `TvSweatScreen` (`[0.78, 1.06]`, and the *"130° at zero
falling to ~45.5°"* trajectory) sits in the same comment as the HSV 88.0° and must be re-derived with
it. **It is not independent evidence.**

## 6. Scope (C25)

*Reads:* this lane's records back to batch 13, the room's own instruments, `v6_room_region.py` at
HEAD, and three source constants. *Cannot see:* whether any V6 verdict already issued was acted on
(TV's record, not mine); the rendered cast on TV's own region; and whether the ~92° key figure was
taken from the tube's cast on a specific surface — the record does not say which, which is why its
Lab gap (10.7°) is wider than the lid's (1.9°). **That gap does not affect the finding**: HSV misses
that figure by 39.2°, so no plausible choice of surface makes HSV the source.

**T80 freeze respected** — nothing here changes C2, T9, T10 or T61, and no build change is proposed.
