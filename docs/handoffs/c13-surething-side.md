# C13 — the SureThing side. There is no content package, and that is the finding.

**From:** SureThing UI lead · 2026-08-08 · prepared at `d28412e`, no editor used
**For:** the joint window. Room integrates after Allen's walk closes; coordinate through the orchestrator.

---

## 1. What the room actually consumes

**The laptop is not a package.** `LaptopScreen` builds a `RenderMode.WorldSpace` Canvas with
`worldCamera = Camera.main` (`LaptopScreen.cs:101-103`, `:79`). **No RenderTexture, no baked screen
texture, no material atlas.** The main camera draws the live UI straight into the room.

So there is nothing for me to export, bake or hand over. **Whatever `SportsbookApp` / `LaptopOs` /
`LaptopScreen` the room's tree compiles is what the room renders**, plus a handful of `Resources`.

That reframes C13: it was never a stale *asset*. It is a stale *tree*.

## 2. The cause, measured

**`room-refinement` is 372 commits behind main** (and 23 ahead). It compiles a laptop surface from
before the redesign, before the TMP migration, and before the violet was struck. That is the whole of
"the room renders the superseded violet laptop package".

**Room merging main is the integration.** There is no step of mine in front of it.

## 3. The violet is room's own fix, and it is already done

Worth stating plainly so nobody looks for it on my side: room's 23 commits already strike it. Their
scene carries the warm family — phone `unreadEmission (0.114, 0.096, 0.072)`, `buzzEmission
(0.57, 0.48, 0.36)`, both **R > G > B** — and `attentionEmission` is **gone from their scene
entirely** (`0fdb378`, `638e592`, `801ccb9`).

**Main still carries the violet**, in the scene, not just as a source default:

```
Room.unity:10012  idleEmission:      {r: 0.025, g: 0.035, b: 0.055}   ← B > G > R
Room.unity:10013  attentionEmission: {r: 0.28,  g: 0.1,   b: 0.55}    ← the violet
```

**So merging main into room will not reintroduce it, and merging room into main is what removes it
from main.** Anyone re-shooting after only the first half and expecting the violet gone will file C13
a fourth time.

## 4. What room must change on its side

**One thing: merge main.** Then verify these survived, because if any is dropped **the laptop renders
nothing at all** — not a fallback, nothing:

| Must survive | Why |
|---|---|
| `Assets/SBR/Resources/SureThing/Fonts/` — 2 TTFs, **3 TMP assets** (`Archivo SDF`, `ArchivoNarrow SDF`, `Archivo SemiBold SDF`), both OFL licences | `LoadFont` resolves the TMP assets **by path**; a missing one falls back to TMP's default face and warns |
| `Assets/TextMesh Pro/` (essentials: settings, shaders, default material) | **Without `TMP_Settings` every TMP component resolves nothing.** This project had no TMP resources at all before Phase L |
| `Unity.TextMeshPro` in **four** asmdefs — `SBR.Game`, `SBR.Game.Editor`, both test assemblies | the test assemblies need it explicitly; they set `overrideReferences` |
| `Assets/SBR/Resources/SureThing/Ink/` — 6 sprites | `Resources.LoadAll<Sprite>("SureThing/Ink")`, prefix-filtered per variant |

**Recoverable if lost:** `tools/tmp-phase-l-bootstrap.ps1` regenerates all three font assets from the
TTFs and verifies them. Every atlas parameter is a named constant, so the rebuild is identical.

## 5. Reading the first joint capture

Check these before calling the laptop integrated — each fails *visibly* rather than loudly:

1. **Two voices.** Condensed figures and prices against roman labels. If the whole surface reads in
   one voice, the TMP assets did not travel and it is running on TMP's default face.
2. **`−` in the prices** — U+2212 (S30). A missing glyph renders as **nothing**, not a box; the atlas
   warm-up logs which characters failed at boot, so check the log rather than squinting.
3. **The rail reads `NOTEBOOK` at weight 600** with the sticker clear of it.
4. **Suites: EditMode 78, PlayMode 57.** Any other number is main's arithmetic, not a laptop defect.

## 6. The one merge hazard, and it is not what the forecast says

`git merge-tree main HEAD` in room's worktree reports **CLEAN**. **Do not trust that on
`LaptopScreen.cs`.** Four of room's commits edit that file (the emission fields, lines ~28-30) and my
Phase L migration rewrote its font seam (`_font`/`_fontCond`, `LoadFont`, `WarmFontAtlas`) **three
lines below them**. Adjacent hunks merge cleanly; adjacent hunks are also exactly where a textual
merge is least trustworthy.

**Read that file after merging.** The correct outcome is room's emission edits *and* the TMP font
seam, both present. This surface has already had one merge where a clean auto-resolution needed
reading (`SureThingVisualCaptureTests`), and one where a rename cleared a collision on one side and
created a new one against the other.

## 7. What I cannot see (C25)

- **I have not run room's scene.** All of the above is read from source, scene files and git; the
  emission values are the serialized ones, not measured from a frame.
- **The clean-merge claim is textual, not semantic.** §6 is the caveat, and it is the reason §6
  exists.
- **The laptop's *material* emission is R40 and is room's, sequenced with R39's bake.** Nothing here
  touches it, and the bake is what voids Gates 6–8 (C28) — that ordering is room's call, not mine.
- **A room capture is not evidence for the laptop until C13's own line is cleared** — C13 says so
  itself, and that clause outlives this note.
