# S6 / S7 / S8 — SureThing laptop: evidence for Design Director review

**From:** SureThing UI lead · **Date:** 2026-07-31 · **HEAD:** `7169c95` (+ one uncommitted change, noted below)
**Suites:** EditMode 75/75 · PlayMode 38/38
**Captures:** `surething-s6-s8-captures/` — eight flat 1024×704 renders of the real UI, plus one angled main-camera render in the room
**Visual status:** `PRE-TYPOGRAPHY` — every capture renders in Unity's `LegacyRuntime.ttf` fallback, **not** the ruled production faces. Read the closing section before drawing conclusions about type.

---

## Scope

| Item | What it covers | State |
|---|---|---|
| **S6** | Lobby shell — the annotated form guide | Implemented |
| **S7** | Ink sprites — deterministic biro rings and strike, variant by matchup index | Implemented |
| **S8** | OS chrome — fictional OS identity, clock, second affordance; personal not institutional | Implemented |

S9 (event detail, staged ticket, MY BETS, rewards, ledger) is partially built and **not** submitted here. Its screens appear in the captures because they share the chrome, and a defect list for them is at the end so the Director can see what is already known before spending review on it.

---

## What renders

**S6 — the form.** Six 78px two-line entries under a 26px column head, in a 700px house column with a 324px working margin at the right. Ruled, not carded: a 1px `rule-soft` separates entries, and there is no rounded-card shell anywhere on the surface. Selection is a drawn biro ring around the price, never a filled pill. A marked entry picks up a faint biro wash. Prices are locked and never animate.

**S7 — the ink.** Rings and the strike are generated raster assets — white RGB with all the ink in alpha, tinted at runtime by `Image.color`, exactly as the reference kit tints them by CSS mask. Variant is keyed to the matchup index and never randomised, so the board does not redraw itself when the player nudges a stake. Import settings are pinned by an EditMode test (`FullRect` mesh so the pen's overshoot is not clipped, mipmaps off, uncompressed), because a default reimport silently breaks them.

**S8 — the machine.** One rail across the top and one tray across the bottom, built once and shared by every screen. The machine is `NOTEBOOK`, the clock reads `02:47`, and a `PROPERTY OF NOBODY` sticker sits beside the machine mark. The tray carries two real apps: the running one reads pressed-in on ink ground, the backgrounded one raised and muted, and the running app's own slot drops to the desktop rather than pretending to launch what is already open.

Capture `06-ledger` is the evidence that the chrome is genuinely shared — it is a different app, and its rail and tray are identical.

---

## Defects found and fixed during the build

Recorded because the review should know what has already been through a correction pass, and because two of these were mistakes in the design work rather than the code.

| Defect | Measured | Fix |
|---|---|---|
| Disabled `LOCK IT IN` reason painted as two stray red glyphs | Skip button spans y 8–42, the reason sat at y 26–48 and was built later, so it drew on top. The string is 247px against Skip's 230px, so exactly one glyph escaped each edge | Reason moved above the button it explains |
| Every price he had **not** chosen rendered in his biro ink | Colours keyed off `replacement` (true for every *other* offer) instead of `selected` | Keyed off `selected`; the swap hint keeps its affordance without borrowing his ink |
| Wide selection ring did not close | With a top-left pivot a negative Y moves the rect **down**: `(-8,-8)` put the ring at y −8..−54 against a cell of 0..−32, under the number rather than around it | `(-8,+8)`, size `cell + 16` = 176×48 |
| Dead-leg strike ran past its word | Fixed 112px sprite-native box across a ~38px word | Derived from the text |
| Type floor breached 24 times in the approved direction | Worst was the disabled reason at 8.5px — the single most corrective string on the surface | All ≥13px; 12px whitelisted for OS chrome only |
| Records rendered at 19px | Team name and record shared one `Text`, so the spec's two-size split could not exist | Split; record now set 9px after its name per `FormEntry.line()` |
| Desktop taskbar ground was `rgba(.025,.02,.05,.94)` | Effectively black **and blue-tinted** — broke the lifted-black rule and the room's no-cool-colour rule at once | Lifted warm ground |

**Two were my own misreads, corrected against pixels rather than argument.** I reported a negative price as oxide red; sampling showed `RGB(202,153,59)` — wax, and correct. I reported the MY BETS ring as clipped by its panel; it spans x 637–674 inside a 700px panel. Both were withdrawn.

---

## What is deliberately not here

**Toner grain is implemented and disabled.** With it on, the ground measured `(24,24,16) → (52,52,48)`: more than double the luminance, and neutral grey where the ground is warm olive. White texels under normal alpha blending can only lighten, so it bleached the sheet instead of texturing it. Lowering the opacity only makes a fainter version of the same wrong thing — real grain has to darken as well as lighten, which needs an overlay blend and therefore a custom UI shader. **This is the one document-layer element still missing** and it is a scoped task, not a tuning pass.

The other three document-layer elements are in: the marked wash, the wax highlight behind the payout, and `--wax-ink` on the primary action.

---

## The typography caveat — read before judging any capture

Every image here renders in `LegacyRuntime.ttf`. The ruled faces (Archivo + Archivo Narrow, OFL 1.1) are committed but **not yet wired**; that needs an editor session, which is queued.

So these captures are evidence of **structure, hierarchy, geometry and colour**. They are **not** evidence of the surface's voice, and the direction's most recognisable trait — a condensed figure set against a roman data face — is absent from all of them. The two-voice seam is already wired structurally (`_font` / `_fontCond` route to the right elements) and both currently resolve to the same fallback, so the change will be one assignment and every glyph on the surface will move.

A second caveat on those faces: `google/fonts` ships no static instances, so both are variable fonts, and Unity's legacy UGUI `Text` renders only a font's default instance. The 400–700 weight range the design system treats as a usable channel is **not** addressable on this surface. It costs nothing today because tier is carried by size, colour and position — but if a spec asks for a weight tier, the answer is TextMeshPro font assets, not a TTF per weight.

**Recommendation:** review S6/S7/S8 on structure now, and treat type as a separate pass once the wiring lands. I will refresh this capture set at that point.

---

## Known S9 defects — not for review, listed so it is not re-reported

From my own audit of `07-rewards` and `06-ledger`:

- **Rewards prices render in oxide red** (`5 COMPS`, `6 COMPS`). A price is not the house's mark. The *blocked reason* beside it (`NEED 5 COMPS`) is legitimately oxide; the price is not.
- **`LEAVE — NEXT ROUND` is a saturated blue** — the loudest element on the screen, in the player's ink, for a primary action that should be wax.
- **`1 COMPS`** — number agreement, same class as the `1 SELECTIONS` already fixed.
- **Offer body copy truncates mid-sentence** with no ellipsis; the offer list also overruns the tray.
- **A banner draws over the offer rows** rather than in its own space.
- **Ledger says `READ ONLY` four times** on one screen, and its column heads sit above a caveat line rather than above the rows they head.

These are mine to fix under S9 and need no ruling.
