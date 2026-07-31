# SureThing production faces

**Ruled 2026-07-31 (Design Director, S11-A), under S11.** No licence-encumbered typeface ships in
this product. Bell Centennial was the original intent and is **dropped for good** — that is a closed
decision, not a pending one. Do not revive it.

| File | Family | Licence | Source |
|---|---|---|---|
| `Archivo.ttf` | Archivo (variable: `wdth`, `wght`) | SIL OFL 1.1 — `OFL-Archivo.txt` | `google/fonts` `ofl/archivo/` |
| `ArchivoNarrow.ttf` | Archivo Narrow (variable: `wght`) | SIL OFL 1.1 — `OFL-ArchivoNarrow.txt` | `google/fonts` `ofl/archivonarrow/` |

Fetched 2026-07-31 with Allen's approval, recorded against the closed S11 ruling. The OFL permits
redistribution, so both licence files are committed beside the fonts and must stay with them — that
is a condition of the licence, not tidiness.

## The two voices

The design system (`main-2/docs/design/design-system/tokens/fonts.css`) defines these as one type
system, not a pairing:

- `--font-data` → **Archivo** — roman. Labels, copy, OS chrome, tabs.
- `--font-cond` → **Archivo Narrow** — condensed. Figures, prices, team names, terminal-state words.

They are a real superfamily sharing proportions, metrics and one hand, which is what makes the
document read as a printed directory rather than as two unrelated faces. Both carry tabular figures,
which matters because values change in place and non-tabular digits make the surface twitch.

Archivo was chosen for the *function* Bell Centennial's ink traps served — holding small type
together, there on cheap absorbent paper, here on an angled, graded, bloomed screen — rather than for
its look. No OFL face reproduces ink traps.

## Runtime seam

`LaptopScreen.LoadFont` resolves `_font` (data) and `_fontCond` (condensed); every builder takes a
`Font` by parameter. Swapping a face is one assignment in `Awake` plus the asset — nothing else in
the UI names a typeface, and it should stay that way.

## Known caveat — variable fonts on legacy UGUI

Both files are **variable** fonts; `google/fonts` ships no static instances for these families.
Unity's legacy `UnityEngine.UI.Text` renders through FreeType and uses the font's *default instance*,
so the `wght` 400–700 range the design system treats as a usable channel is **not** addressable here.
Today that costs nothing, because the current UI expresses tier through size, colour and position
rather than weight.

It becomes real if a spec asks for a weight tier. The fix then is TextMeshPro font assets, which can
carry named instances — not a second TTF per weight, and not faking it with `FontStyle.Bold`.
