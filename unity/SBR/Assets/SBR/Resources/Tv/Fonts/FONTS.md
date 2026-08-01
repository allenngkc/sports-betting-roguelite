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
| `EncodeSans.ttf` | `google/fonts` `ofl/encodesans/EncodeSans[wdth,wght].ttf` | **Variable** font (wdth, wght). Unity's legacy `Font` renders its default instance |
| `EncodeSansCondensed.ttf` | `google/fonts` `ofl/encodesanscondensed/EncodeSansCondensed-Regular.ttf` | Static Regular; upstream ships no variable build |
| `OFL-EncodeSans.txt` | `ofl/encodesans/OFL.txt` | Copyright 2020 The Encode Project Authors |
| `OFL-EncodeSansCondensed.txt` | `ofl/encodesanscondensed/OFL.txt` | Copyright 2012 The Encode Project Authors |

They live under `Resources/` deliberately: `TvSweatScreen.LoadFont` resolves the face with
`Resources.Load<Font>("Tv/Fonts/EncodeSans")`, and a font outside `Resources/` is not loadable that
way.

## Why this mattered beyond fidelity

The TV had **never** rendered in its own typeface — `LoadFont` returned `LegacyRuntime.ttf`, and no
font asset existed anywhere in the repo. T20 re-derived the entire px scale from canon values
measured against Encode Sans and shipped them into a wider face. The seated captures show the
consequence: `MARKET SUSPENDED` clipped to `ARKET SUSPENDED`, leg copy running out of the ticket
column. The strings are correct and `DESIGN.md` §6 forbids shortening authored copy to fit a
measurement — the face was wrong.

## Still open

- **The condensed face is committed but not yet wired.** Canon splits the surface: `--font-tv-cond`
  carries the price, the progress line and the compact leg rows, `--font-tv` the rest. This build has
  a single `_font` field, so wiring the split needs a second field threaded through `MakeText`'s call
  sites. Until then every element renders in the regular face, which is **narrower than Legacy but
  wider than canon intends for the condensed slots** — so copy fit there is still not final.
- **T20's px derivation should be re-checked in the real face**, now that one exists. The 19px
  progress conclusion was reasoned from canon's own note; it has never been observed in Encode Sans
  on this surface.
