# Ink assets — the annotated form guide

Generated, not hand-drawn, and reproducible. Regenerate with:

```
python tools/art/make-biro-rings.py
```

A given seed always produces the same mark, so regenerating never silently changes a shipped
asset. Retune by editing `VARIANTS` at the foot of that script.

## The files

| File | Size @1x | Use |
|---|---|---|
| `ring-price-a` · `-b` · `-c` | 112×46 | Selection mark around a moneyline price cell |
| `ring-wide-a` · `-b` | 176×46 | Selection mark around a wider market row (event detail) |
| `strike-a` | 112×46 | Struck-through dead leg during the sweat |

Each ships at `@1x` and `@2x`. Import the **`@2x`** and size the RectTransform to the 1× value —
the laptop is read at an angle and the extra sample density survives the perspective.

`_preview-rings.png` is a contact sheet for eyeballing, not a shipped asset.

## They are white sprites, and that is deliberate

RGB is pure white throughout; **all the ink lives in the alpha channel.** Colour comes from
`Image.color` at runtime, so one asset serves every ink:

| Mark | Tint | Meaning |
|---|---|---|
| Selection ring | `--biro` `#5E86B8` | His own mark. Anything he chose. |
| Strike | `--stamp` `#B4483A` | The **house's** mark, and nothing else. Never "you lost". |

This is also why the mockup uses a CSS mask rather than a plain `<img>` — it tints the same way
Unity will, so what you see in the browser is what the build does.

## What makes them read as ballpoint

Worth knowing before anyone "cleans them up":

- The path is an ellipse deformed by three low-frequency harmonics — no ring is truly round.
- **The pen overshoots the start.** People do not stop where they began. Removing the overshoot is
  the fastest way to make these look like clip art.
- A second partial pass retraces part of the ring, sharing the first pass's shape and sitting a
  hair inside or outside it. An early version let the retrace generate its own shape; it cut
  across the interior and read as a smear. Shape is decided once per ring, not per pass.
- Ink density varies along the path, ramps in at the start, and lifts into a tail at the end.
- Short skips drop out where the ball fails to deposit.
- Crossings accumulate, so the doubled section is visibly darker.

Pen radius is deliberately generous. The legibility floor bans hairlines because they disintegrate
on the angled surface, and an ink mark is not exempt from that rule.

## Unity import settings

| Setting | Value | Why |
|---|---|---|
| Texture Type | Sprite (2D and UI) | |
| Sprite Mode | Single | |
| Mesh Type | **Full Rect** | Tight meshes clip the overshoot |
| Alpha Is Transparency | On | |
| Generate Mip Maps | **Off** | UI sprite at fixed size |
| Wrap Mode | Clamp | |
| Filter Mode | Bilinear | |
| Compression | None / High Quality | These are tiny; block compression eats the thin ink |

On the `Image` component: Type **Simple**, Preserve Aspect **off** (the RectTransform is sized
exactly), Raycast Target **off** — the ring is decoration sitting over the real button.

## Picking a variant

Choose deterministically from the matchup index:

```csharp
Sprite ring = _ringVariants[matchupIndex % _ringVariants.Length];
```

**Do not randomise per frame or per canvas rebuild.** `SportsbookApp` rebuilds its canvas on
state change, and a per-rebuild random would make the ring visibly redraw itself every time the
player adjusted a stake. Keyed to the matchup index it stays put, and adjacent rows still differ
so the board never looks stamped.

## Sizing

The ring box is larger than the price cell it surrounds — the overshoot needs somewhere to go.
The rule is additive, not proportional: **ring = cell + 16px on width and height, offset −8/−8**
(8px overshoot on every edge). Apply that rule to whatever the real cell is — do not assume 96×30
if the control that carries the price is wider than that.

```
generic cell   W × H
ring box      (W+16) × (H+16)      offset  x −8, y −8   (8px overshoot per edge)
```

The lobby's moneyline price cell is the AWAY/HOME odds **button itself**, which is 112×32 (it was
widened from an original 96×30 assumption so a 3-digit American price like "AWAY  -341" has room
to sit inside it without crowding the edge). Ring the *button*, not a narrower notional cell:

```
moneyline price cell   112 × 32
ring box                128 × 48      offset  x −8, y −8
```

Defect fixed 2026-07-30: the ring was still being built at the *old* 112×46 size — identical to
the button's own width — so the ring had zero horizontal overshoot and its widest point landed
exactly on the button's rightmost digit. `SportsbookApp.BuildMatchupCard` now derives the ring's
rect from the button's actual position/size plus the 8px rule above, instead of a hand-eyeballed
offset.

For the wider market rows in event detail, use `ring-wide-*` at 176×46 against the 160×32 offer
cell — already consistent with the +16px rule and not affected by the defect above.

The MY BETS mirror reuses the price-ring family as a small "won" mark around the `GREEN` state
word. That word is much narrower than any price cell, and it right-aligns flush against its own
box's edge, so a ring sized to the *box* (rather than the word) has the same failure mode as the
price-cell bug: its widest point lands on the box's edge, which is also where the text's last
letters sit. `SportsbookApp.InkRingGeometry` sizes and places that ring from the state text's own
measured width/height (`Text.preferredWidth/Height`) plus the same 8px-per-edge overshoot, rather
than a fixed box — so it tracks whatever word ("GREEN") is actually on screen.
