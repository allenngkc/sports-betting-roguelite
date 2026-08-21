# Register entries — batch 145 (2026-08-20)

**`C58` HOLDS FOR THE TV — AND I RULED IT FROM A DOCSTRING.** Ran the tool this time. The ruling
survives, its reason is now measured, and **it was under-specified in a way that would have let
someone get it wrong while following it.**

**One row.** **Destination table:** Cross-surface (`C58-am`).

---

## The row

| C58-am | `C58` verified for the TV — it is TWO AXES, not one, and a static file with the shipped asset's NAME sits beside the real source | **AMENDED — DD 2026-08-20 batch 145, §1.5. The ruling stands; its basis and its precision did not.** **`C58` ruled the offline method invalid for the TV on the strength of `ttf_faces.py`'s DOCSTRING — *"Encode Sans is worse… the default is wrong on BOTH axes"* — which I quoted rather than ran. Ran now, on all three TV fonts.** **CONFIRMED AND SHARPENED: `EncodeSans.ttf` and `EncodeSans-Tabular.ttf` are variable with axis defaults `wght=100, wdth=75` — instance [0] is *Condensed Thin* — and `TvTmpFontAssets.SourceFace` is `EncodeSans-Tabular`, resolving `"Condensed Regular"` (400/75) and `"Condensed Bold"` (700/75) BY STYLE NAME. Every leg-row slot passes `Face.Condensed`, so `G1`'s strings render at an instance the file's own `hmtx` does not describe.** **THE UNDER-SPECIFICATION, AND IT MATTERS: `C58` says an offline width *"needs `HVAR` applied at the resolved axis position."* ON THIS FACE THERE ARE TWO AXES. A reader applying weight alone would still be measuring a width-100 face against a width-75 render, and WIDTH VARIATION MOVES ADVANCES FAR MORE THAN WEIGHT DOES. The clause now reads: at EVERY axis the shipped instance names.** **AND THE TRAP THIS SURFACE ACTUALLY SETS, which `C58` did not name because I had not looked: `EncodeSansCondensed.ttf` SITS IN THE SAME FOLDER, IS NOT VARIABLE AT ALL (18 tables, no `fvar`, `usWeightClass` 400, subfamily Regular) — and it CARRIES THE NAME OF THE SHIPPED ASSET, `EncodeSansCondensed SDF`. It is not the source. The generator says so in terms: *"Both condensed faces come from the VARIABLE `EncodeSans.ttf`, not from the static `EncodeSansCondensed.ttf` sitting beside it… The static file is left in place because legacy `Font` still loads it until T-3 lands."*** **SO THE ONE FILE WHOSE `hmtx` WOULD BE SAFE TO READ IS THE ONE FILE THAT IS NOT RENDERED, AND ITS NAME IS THE ASSET'S. `C58`'s founding case was a default instance that was not the shipped one; THIS IS A WHOLE FILE THAT IS NOT THE SHIPPED ONE, WEARING THE SHIPPED ONE'S NAME — the same defect with better camouflage.** **WHAT WOULD UNBLOCK OFFLINE TV MEASUREMENT, named as an open question rather than guessed: whether the static `EncodeSansCondensed.ttf` is the SAME DESIGN CUT as the variable family's `Condensed Regular` 400/75 instance. If it is, it is a legitimate offline proxy and `G1`'s widths stop queueing behind an editor lease. IF IT IS NOT, IT IS THE WORST POSSIBLE PROXY BECAUSE IT LOOKS RIGHT. THIS SEAT DID NOT ATTEMPT IT — answering it needs `HVAR` applied at both axes, which is the silent-failure work `C58` exists to prevent, and a wrong answer here would be indistinguishable from a right one** | batch 145 |

---

## What this changes for `G1`

**Nothing, today.** `g1-measurement-brief-2026-08-20.md` §0 already routes the measurement to the
editor on `C58`'s authority, and that routing is now verified rather than asserted.

**What it adds is a warning the brief did not carry:** if anyone attempts the offline route anyway,
**the file they will reach for is the wrong one and its name will tell them it is right.**

---

## What is NOT in this batch

- **No attempt at the `HVAR` route.** Named as the open question, deliberately not guessed.
- **No claim about the static file's design cut** — that is the open question itself.
- **No change to `C58`'s ruling**, which holds on both surfaces; only its precision and its basis
  moved.
