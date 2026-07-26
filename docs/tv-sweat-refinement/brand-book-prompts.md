# Brand book — image prompts

Renders `DESIGN.md` so it can be judged by eye instead of by reading. Generate externally; this
harness has no image generation.

**Generate P1 and P2 as a pair.** They are the same screen at two different moments, and the point
is the *difference* between them — that is the brightness law made visible. P3 is a reference sheet
for the state vocabulary.

Aspect ratio **16:9**, same model and seed across all three.

**Text will garble.** Judge light, hierarchy, colour, and material. The copy is there to shape
composition, not to be read.

---

## Fixed scenario, identical in all three

> NORTHGATE 1 — 1 CALDER CITY, 67 minutes. A four-leg ticket: Northgate moneyline **won**, over 2.5
> goals **live** (this is the active leg), Vale anytime scorer **next**, over 8.5 corners **lost**.
> Fifty dollars at risk, seven hundred eighty to pay. Cash-out offered at $286.40.

All four leg states appear in one image, which is why this ticket was chosen.

---

## P1 — Layout B, cash-out actionable

The resting state. Cash-out is live, so it is the single brightest thing on the surface.

```
Flat-on UI render, 16:9, edge to edge, no room, no bezel, no border. A stadium LED video board interface on pure black. The entire surface is drawn on one visible fixed-pitch LED dot matrix — individual circular lit pixels with dark gaps between them, gentle halation bleeding between adjacent lit dots. Flat and emissive, absolutely no drop shadows, no bevels, no depth, no gradients.

LAYOUT: a full-height vertical ticket column occupying the left quarter of the screen. The remaining three quarters hold, from top: a compact scoreline strip, then a large top-down soccer pitch, then a single thin event line. A wide horizontal cash-out band sits across the bottom of the left column. Regions are separated by unlit black gutters, never by drawn lines or boxes.

LEFT TICKET COLUMN, four stacked leg rows top to bottom:
- row 1, warm gold lit dots, solid, a moneyline leg marked won
- row 2, the active leg, white lit dots, brighter and taller than the others, expanded to show a market line and a progress line beneath it in cyan
- row 3, very dim barely-lit dots, almost structure only, a pending leg
- row 4, completely unlit — visible ONLY as dark unlit pixel structure, black, dead, no colour at all
Below them, small gold figures for risk and payout.

RIGHT REGION: compact scoreline with team names in electric blue and hot magenta either side of large white tabular numerals reading 1 and 1, a white clock at the far right. Below it a top-down soccer pitch with markings in dim green lit dots, scattered small dots as players in electric blue and hot magenta, one white ball dot. At the bottom a single thin white event line.

BRIGHTNESS, the most important instruction: the gold cash-out band across the bottom left is unmistakably the BRIGHTEST element in the entire image, blazing, clearly ahead of everything else. The white score numerals are the second brightest and visibly dimmer than the cash-out. The active leg and pitch players are mid brightness. Pitch markings, labels and the pending leg are dim. The lost leg is fully black. This descending brightness ladder is the whole subject of the image.

Palette: pure black ground, electric blue, hot magenta, white, cyan, warm gold, dim green pitch. Type is heavy condensed uppercase grotesque with tabular figures, drawn on the dot grid. Expensive, precise, engineered. Stadium signage, not a website, not an app.
```

---

## P2 — Layout B, goal payoff

The same screen one moment later. A goal has landed, so full brightness moves to the score, and the
cash-out is suspended because a dangerous scene suspends the market.

```
Flat-on UI render, 16:9, edge to edge, no room, no bezel, no border. A stadium LED video board interface on pure black. The entire surface is drawn on one visible fixed-pitch LED dot matrix — individual circular lit pixels with dark gaps between them, gentle halation bleeding between adjacent lit dots. Flat and emissive, absolutely no drop shadows, no bevels, no depth, no gradients.

Identical layout to a resting state: a full-height ticket column on the left quarter with four stacked leg rows and a wide horizontal band at its foot, and on the right three quarters a compact scoreline strip, a large top-down soccer pitch, and a thin event line. Nothing has moved or resized. Regions separated by unlit black gutters.

THIS IS THE MOMENT A GOAL LANDS.

BRIGHTNESS, the subject of the image: the white score numerals, now reading 2 and 1, are blazing at absolute maximum brightness, overwhelmingly the brightest thing on the surface, with heavy halation blooming around them. Everything else has receded. The horizontal band at the foot of the ticket column is now almost completely dark and unlit, a nearly black slate reading a suspended market message — it is NOT gold and NOT bright. The pitch shows a bright white ball dot inside the goal area. The event line is moderately lit in white. The leg rows are dim: one gold won row, one white active row, one barely-lit pending row, and one completely black dead row.

Compare to a resting state: full brightness has moved from the bottom band to the score, and the bottom band has gone dark. Exactly one element is at full brightness.

Palette: pure black ground, electric blue and hot magenta team names, white, cyan, dim green pitch, and gold only on the single won leg row. Type is heavy condensed uppercase grotesque with tabular figures on the dot grid. Expensive, precise, engineered. Stadium signage, not a website, not an app.
```

---

## P3 — State vocabulary sheet

A reference sheet rather than a screen. Useful for checking the states are separable at a glance.

```
Flat-on design reference sheet, 16:9, pure black background, entirely drawn on one visible fixed-pitch LED dot matrix with circular lit pixels, dark gaps between them, and gentle halation between adjacent lit dots. Flat, emissive, no shadows, no depth, no gradients.

TOP HALF: five short horizontal sample rows stacked vertically, each a bet leg in a different state, each clearly at a different brightness:
1. very dim, barely lit, structural only — pending
2. bright white, the brightest of the five — live
3. solid warm gold, bright but flatter than the white — won
4. completely unlit, pure black, visible only as dark unlit pixel structure — lost
5. dim cyan with a horizontal strike-through line drawn across it — void

BOTTOM HALF: four wider horizontal bands stacked vertically, each the same rectangle at a different market state, each a different brightness:
1. blazing warm gold at maximum brightness with a large dollar figure — actionable
2. gold but clearly dimmer, with the figure mid-change — updating
3. almost completely dark, nearly black slate, faint dim text — suspended
4. quiet and very dim, empty — unavailable

Every band is exactly the same size and position width, only brightness and colour change between them. Heavy condensed uppercase grotesque type with tabular figures on the dot grid. Palette pure black, white, cyan, warm gold. Precise, engineered, expensive.
```

---

---

# P4 — in-room acceptance render (the one that matters)

Added 2026-07-25 after the first three renders. P1–P3 confirmed the brightness law, the dead-leg
call, and gold-as-money. They also surfaced four corrections, and this single image tests all of
them at once — in the room, which `DESIGN.md` §2A now names as **the only valid acceptance view for
this surface.**

What it is testing:

1. **Panel physicality.** The earlier comps rendered a flawless vector LED board. The room is
   painterly and decayed. This render must show a real panel hanging in a filthy room — glass sheen,
   the fluorescent reflecting on it, light escaping the bezel, dust, seen off-axis from the couch.
2. **Narrower ticket column.** ~27%, not the ~37% the first render drew.
3. **Dimmer pitch markings.** L1–L2, a place rather than an event.
4. **Two legs live at once**, per PRD §8.2A — the structural correction. Both live rows in the live
   treatment simultaneously.

Fictional teams only. The first three renders returned real clubs; the names below are stated
positively rather than only banned.

```
A cramped bunker-like room at night, painterly semi-realistic concept art, cinematic wide interior. Heavily peeling paint on walls, ceiling and floor. Exposed black conduit and pipes across every surface, bolted steel brackets. A heavy riveted industrial bunk bed frame on the left with a worn patched couch underneath. A small deep-set window in the far wall showing a dark city skyline with scattered warm-lit windows. A battered metal desk on the right with an open laptop, a phone lying flat, and an ashtray full of cigarette butts. A metal stool. A coiled black cable on the floor. A wall-mounted fluorescent strip light casting sickly yellow-green light. Room surfaces olive, khaki, drab yellow-green, damp and institutional.

Mounted on the right wall, seen slightly off-axis from the couch rather than square on, is a large flat LED television, switched on, showing a live soccer betting broadcast. THE TELEVISION IS A REAL PHYSICAL OBJECT IN THIS ROOM, not a flat graphic pasted on: its glass surface carries a faint sheen with the yellow-green fluorescent tube reflecting across the upper edge, there is faint dust and smear on the glass visible where bright pixels sit behind it, and its blue and magenta light bleeds past the bezel and spills onto the peeling wall around the frame, the desk, the couch and the wet floor.

ON THE SCREEN, drawn as a fine visible LED dot matrix on pure black with halation between lit dots: a narrow vertical ticket column occupying only about one quarter of the screen width on the left, and a large soccer pitch filling the remaining three quarters on the right with a compact scoreline above it reading the invented team names NORTHGATE and CALDER CITY in electric blue and hot magenta either side of large white numerals. The pitch markings are DIM, low-brightness green dotted outlines, quiet and recessive, while small bright blue and magenta player dots and one white ball dot sit clearly on top of them.

In the narrow left column, TWO separate bet legs are live at the same time, both rendered in bright white and both expanded to show a requirement line and a progress line, stacked one above the other. Above them one dim gold row marked won. Below them one completely unlit black row, dead. Small gold risk and payout figures. At the foot of the column a horizontal gold band showing a cash-out dollar figure, and this band is the single brightest element on the whole screen.

The television is by far the dominant light source in the room, overwhelming the fluorescent. Deep shadow elsewhere. Photographic depth of field, 16:9.
```

## What to judge

Only one question, and it is the one that has been unresolved since the room art landed:
**does the TV look like it belongs in that room, or like a graphic pasted onto a wall?**

If it still reads as pasted on, the remaining fix is the unified post-process grade in `DESIGN.md`
§2A — one grain, one bloom, one colour grade over room and TV together — and that is a joint task
with the room team rather than a change to this surface's design.

Secondary: is the narrower column better balanced, and can you still read both live legs at a glance?

---

## Negative prompt

```
website, web page, browser chrome, mobile app screenshot, rounded rectangle cards, drop shadow, bevel, emboss, glassmorphism, gradient background, material design, dashboard template, 3d render, depth of field, real football clubs, Premier League, real team names, real brand names, scanlines, CRT curvature, screen curvature, vaporwave, retro arcade, cream paper, newsprint, vintage
```

`scanlines, CRT curvature, screen curvature` matter — the halation in this world is **per-pixel bloom
on a hard grid**, which is not the same thing as the screen-wide phosphor haze from the deprecated
`design/08-art-direction.md`. If a render comes back soft and glowing overall rather than sharp dots
with local bleed, it has drifted into the old world.

---

## What to judge

1. **Does the brightness ladder read?** In P1, is the cash-out obviously the brightest thing? In P2,
   has that clearly moved to the score? If both images have five things equally bright, the law is
   not landing and the brand book needs rework.
2. **Does the dead leg read as dead?** A fully unlit row is the boldest call in the book. If it just
   looks like a rendering error rather than a loss, that decision fails.
3. **Is gold enough to carry money?** It is the only warm hue on an all-cool surface. If it reads as
   decoration rather than as *the money*, green may need to come back after all.
4. **Does the matrix hold?** Sharp dots with dark gaps, or has it drifted into soft neon glow?
5. **Is the score still the biggest thing** even when the cash-out is the brightest? Size hierarchy
   and brightness hierarchy are separate channels and must not collapse into each other.

For an in-room version, paste the room block from
[image-prompts.md](image-prompts.md) and describe the TV as showing the P1 screen.
