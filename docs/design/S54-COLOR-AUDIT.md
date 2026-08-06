# S54 — every `new Color(float…)` on the laptop surface

**SureThing UI lead · 2026-08-06 · HEAD `35292d9`**
**Scope:** the three surface files the palette guard already scans —
`SBR/Runtime/SportsbookApp.cs`, `LaptopOs.cs`, `LaptopScreen.cs`.

**27 instances. None of them can silently change a rendered colour on this surface, and two of them
cannot be answered from source at all.** Class by class, with the reason rather than the verdict.

## 1 — Transparent layout containers · 17 instances · not at risk by construction

`new Color(0f, 0f, 0f, 0f)`, at `SportsbookApp.cs` 182, 504, 509, 523, 646, 676, 705, 712, 1082,
1254, 1273 and `LaptopOs.cs` 78, 401, 459, 947, 951 (plus the verdict work area added this batch).

Alpha is zero, so nothing is drawn. These are scroll hosts, group rects and the verdict's work area —
objects that exist to hold a rect, not to paint. No colour is asserted, so none can be wrong.

## 2 — `ColorBlock` tint multipliers · 4 instances · not colours

`new Color(1.25f, 1.25f, 1.25f, 1f)` and `new Color(0.8f, 0.8f, 0.8f, 1f)` at `LaptopOs.cs`
777/778 and 1399/1400.

These are Unity `Selectable` **gain factors**, multiplied against whatever the target graphic already
is. The value above 1 is deliberate — it brightens on hover. They are not authored colours and a
future audit should not flag them as such.

## 3 — Token-derived, alpha-only authoring · 3 instances · renders as authored

`LaptopUi.Dim` (`LaptopOs.cs:1091`), the marked-entry wash (`LaptopOs.cs:875`), and the rewards
highlight (`SportsbookApp.cs:964`). Each takes `r`, `g`, `b` from an existing `Color32` palette token
and authors only the alpha. The channels are the token's own, and alpha is not colour-space
converted. Nothing here is a new colour.

## 4 — Dead · 1 instance · renders nothing because nothing calls it

`LaptopUi.FromRgb(uint)` (`LaptopOs.cs:1105`) builds a `Color` from a packed 8-bit RGB by dividing
each channel by 255.

**It has no call sites.** `grep` for `LaptopUi.FromRgb` across `Assets/` returns nothing; the live
`FromRgb` calls in `TvSweatScreen` resolve to `TheaterStage.FromRgb`, a separate helper on the TV
surface. **Recommend deleting it** — an unused helper that constructs colours by a different route
than the palette is exactly what gets picked up and misused later, and leaving it guarantees the next
audit re-flags it. Not deleted here: the ruling asked for a report.

## 5 — Live, float-authored, and **not answerable from source** · 2 instances

`LaptopScreen.idleEmission` = `(0.025, 0.035, 0.055)` and `attentionEmission` = `(0.28, 0.10, 0.55)`.

Three things make these the only real answer in this audit:

- **They are serialized public fields, so the source default is a fallback and the scene ships.**
  A source-only audit genuinely cannot tell you what renders. **Checked, not assumed:**
  `Room.unity:10012-13` carries `(0.025, 0.035, 0.055)` and `(0.28, 0.1, 0.55)` — identical to
  source, so for the laptop there is no drift. The *other* `idleEmission` in that scene
  (`Room.unity:7438`, `(0.02, 0.03, 0.06)`) belongs to **`PhoneScreen`**, a different component —
  resolved by script guid, not inferred from proximity.
- **They take a different path to the screen than everything else here.** `[ColorUsage(false, true)]`
  HDR emission, pushed through a `MaterialPropertyBlock` onto the lid renderer — not a UGUI vertex
  colour. None of this surface's measurements touch that path.
- **`attentionEmission` is a saturated violet** on the laptop lid, in a project that retired purple.
  Flagged, not touched: it is room lighting rather than the document, and it is not mine to rule.

## What this audit cannot see (C25)

**It reads source and scene. It does not read frames.** Classes 1 and 2 are safe by construction and
class 3 by derivation — those need no measurement. But the only way to *know* a colour renders as
authored is to measure it, and **no capture in the set shows the lid emission at all**: it is a room
lighting effect outside the flat canvas captures, and the angled camera states do not drive the
attention glow. If that gap matters, it needs a room-camera state with the glow active, and that
state does not exist.

The bespoke ground that started this is gone (S53-am). The only remaining textual match for it is
inside the comment recording its removal.
