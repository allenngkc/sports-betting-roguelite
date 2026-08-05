# SureThing desktop — four items for the Design Director

**From:** SureThing UI lead · **Date:** 2026-08-03
**Frame:** `11-desktop-flat-1024x704` in `surething-captures-2026-08-03.zip`

The desktop has never been reviewed, for a reason worth stating: **it was never drawn.**
`LaptopWallpaperGraphic` was constructed without a `CanvasRenderer`, so UGUI never asked it for
geometry. It rendered nothing, threw nothing, and passed every test. It draws now, and the frame in
that zip is the first time anyone has seen it.

It holds the laws it can be measured against — ground mean `(22.9, 22.9, 14.5)` with a floor of 18,
so nothing approaches pure black and the cast is warm exactly as `#16160F` is warm. What follows is
not measurable, which is why it comes here.

**The kit has no desktop spec.** `ui_kits/surething/` covers the five sportsbook destinations and
the LEDGER. So this screen cannot be judged 1:1 against canon; it can only be judged against the
direction. All four items below are that kind of question.

---

## 1. The machine wears the operator's branding

The wallpaper is `SURE THING.` in toner and biro, over the tagline **"the number never lies"**.

Against the personal-machine rule, this is the sharpest question on the screen. `PRODUCT.md` draws
the laptop as *his own* — personal, cheap, chosen, possibly grubby — precisely against the TV, which
is institution-installed. A desktop wearing the bookmaker's logo and slogan is the operator's
furniture, which is what the TV is supposed to be.

The opposite reading is at least as strong, and is why I have not touched it: a man whose own
machine wears the house's wallpaper is a man the house already has. That is the story the direction
is telling, and it would be a good scene.

I cannot tell which was intended, and it is not a lead's call either way.

**Second question inside the first.** "The number never lies" is a promotional claim in the house's
voice. The standing rules say satire may occupy flavour but never a slot where a fact belongs, and
that the surface must never imply a guaranteed win. In-fiction it is the bookmaker's own marketing
and reads as characterisation. Sitting alone on his desktop it reads closer to a promise. Same
ambiguity, and it turns on the same answer.

## 2. The app is called three different things

- the desktop icon says **`Sportsbook`**
- the tray slot says **`SURETHING`**
- the app's own masthead says **`SURETHING FORM`**

For comparison, `LEDGER` is correct and consistent everywhere, because S16 ruled it: one name, all
surfaces, code identifiers exempt. The sportsbook never got that ruling.

This is the S16 question one app over. It needs the same one-line answer, and then it is a mechanical
fix.

## 3. A live app is dressed as an unbuilt one

The desktop lists four icons. `Mail (soon)` and `Bank (soon)` are placeholders and correctly dimmed.
`LEDGER` is live, opens, and is fully built — and renders in the same dimmed treatment, with the
same muted `$` glyph.

So the one working secondary app on the machine advertises itself as unfinished. Of the four items
this is the least ambiguous and the most likely to read as a straightforward defect, but the fix is
a visual-hierarchy decision — how a live secondary app should differ from a placeholder — rather
than a value to change.

## 4. The desktop taskbar is a third chrome surface

The rail and tray were unified into one shared `NotebookChrome` under S8, built once and consumed by
every destination. The desktop's own taskbar was left out of that unification and remains separate:
54px against the tray's 34px, its own layout, its own content.

It is the last piece of chrome not sharing the machine's furniture. Two consequences already
observed:

- It drifted. Its ground was `rgba(.025, .02, .05, .94)` — effectively black **and blue-tinted**,
  breaking the lifted-black rule and the room's no-cool-colour rule at once. Fixed, but it drifted
  precisely because nothing shared it.
- It disagreed about the machine. It printed `03:17 AM · 12%` while the rail was pinned at `02:47` —
  one machine claiming two different times, a click apart. Also fixed, and both now read one pair of
  constants.

The question is whether the desktop should carry the same tray as every other destination, or
whether a desktop legitimately has its own furniture. Folding it in is real work and would change a
DESIGN-VERIFIED surface, so it should be a decision rather than a lead's tidy-up.

---

## What I have not done, and why

Nothing on this screen has been changed except the two defects named in item 4 — the illegal ground
and the contradictory clock — both of which were measurable against existing law and needed no
ruling.

Everything else here is a direction question about a screen the kit does not describe. Guessing at
it would be inventing product, and the first item in particular changes what the game is saying
about its own protagonist.
