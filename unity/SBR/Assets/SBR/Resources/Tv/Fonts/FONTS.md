# TV sweat — typefaces

Canon names these; this directory is the build's copy of them.
`main-2/docs/design/design-system/tokens/fonts.css`:

```css
--font-tv:"Encode Sans",system-ui,sans-serif;
--font-tv-cond:"Encode Sans Condensed",var(--font-tv);
```

Approved by Allen 2026-08-01, OFL 1.1, licence files committed beside the fonts — the same shape
SureThing uses for Archivo (`Assets/SBR/Resources/SureThing/Fonts/`).

| File | Upstream | Note |
|---|---|---|
| `EncodeSans.ttf` | `google/fonts` `ofl/encodesans/EncodeSans[wdth,wght].ttf` | **Variable** font (wdth, wght). **45 named instances**, and its DEFAULT is `Condensed Thin` — wght 100, wdth 75, `OS/2 usWeightClass` 100. Never select by face index |
| `EncodeSans-Tabular.ttf` | **derived, not upstream** — `tools/tnum_font.py` from the row above | What the surface actually builds from (Phase T, T82). Only `cmap` differs: the ten digits address the glyphs `tnum` substitutes, so every digit advances 1000/2000 em. Glyph ids, metrics and variation data byte-identical |
| `EncodeSansCondensed.ttf` | `google/fonts` `ofl/encodesanscondensed/EncodeSansCondensed-Regular.ttf` | Static Regular; upstream ships no variable build. **No longer built from** — the variable family carries the whole condensed column including Bold 700, which this file cannot |
| `OFL-EncodeSans.txt` | `ofl/encodesans/OFL.txt` | Copyright 2020 The Encode Project Authors |
| `OFL-EncodeSansCondensed.txt` | `ofl/encodesanscondensed/OFL.txt` | Copyright 2012 The Encode Project Authors |

They live under `Resources/` deliberately: `TvSweatScreen.LoadFace` resolves the face with
`Resources.Load<TMP_FontAsset>("Tv/Fonts/EncodeSans SDF")`, and an asset outside `Resources/` is not
loadable that way. The four generated TMP assets sit beside the fonts, built by
`SBR/TV/Generate TMP font assets` and verified by `SBR/TV/Verify TMP font assets`.

**Re-deriving the tabular font**, which is a build product with its provenance in the repo rather
than a binary from somebody's tool session:

```
python tools/tnum_font.py \
  unity/SBR/Assets/SBR/Resources/Tv/Fonts/EncodeSans.ttf \
  unity/SBR/Assets/SBR/Resources/Tv/Fonts/EncodeSans-Tabular.ttf
```

## Why this mattered beyond fidelity

The TV had **never** rendered in its own typeface — `LoadFont` returned `LegacyRuntime.ttf`, and no
font asset existed anywhere in the repo. T20 re-derived the entire px scale from canon values
measured against Encode Sans and shipped them into a wider face. The seated captures show the
consequence: `MARKET SUSPENDED` clipped to `ARKET SUSPENDED`, leg copy running out of the ticket
column. The strings are correct and `DESIGN.md` §6 forbids shortening authored copy to fit a
measurement — the face was wrong.

## Closed by Phase T — both of the notes that stood here were false, one of them for nine days

**"The condensed face is committed but not yet wired."** It was wired at `c53d7ca` on 2026-08-02,
one day after this note was written, and the note then denied six live call sites until Phase T's
inventory counted them. Its predecessor had been corrected for claiming call sites that did *not*
exist; it spent its own life denying call sites that did. Corrected here in the commit that makes it
false a second time, which is the only moment anyone reliably notices.

**"T20's px derivation should be re-checked in the real face."** Re-checked, on all 20 text slots —
T84's sweep, `SBR/TV/T84 extent sweep`. The result is not a footnote: **six slots overrun their fixed
box** in the canon face, and two more collide as a pair inside one. The sizes were never the problem;
the boxes were sized against a face 20% narrower than the one canon names, which is C46's founding
case and constitution §3.5's converse.

## What the face turned out to be

`LoadFont` once returned `LegacyRuntime.ttf` and the surface had never rendered in its own typeface.
Correcting that was T20's subject. What Phase T then found is that **the correction had not landed
either**: this family's default instance is `Condensed Thin`, so legacy `Font` rendered the roman
voice narrower than the *condensed* face — 241px against 254px on the same string. No Regular 400 at
wdth 100 can do that.

Both faces are now resolved **by style name** at generation, never by index, and the generator
refuses rather than falling back:

| asset | instance | carries |
|---|---|---|
| `EncodeSans SDF` | Regular, wght 400 wdth 100 | the regular-face slots |
| `EncodeSans Bold SDF` | Bold, wght 700 wdth 100 | nothing yet — wired at weight 700, unruled for use |
| `EncodeSansCondensed SDF` | Condensed Regular | LegRowPrice, LegRowProgress |
| `EncodeSansCondensed Bold SDF` | Condensed Bold, 700 | LegRowLine, LegRowNeed, RiskPays, CashOut |

## Still open

- **Six slots overrun their box** and two pair-collide; magnitudes are T74's and T84's, and they hold
  the ship rather than the wiring. Re-measure with the sweep, never by eye.
- **`TakeoverSub` is unbounded.** It renders the engine's `DisplayLabel` list, which nothing on this
  surface limits, so its sweep row is a constructed worst case rather than a measurement.
