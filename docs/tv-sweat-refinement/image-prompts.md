# TV Sweat — image-generation prompts for the three cultural homes

**Purpose:** see all three candidate worlds before committing, so the choice is made on renders
rather than on my prose. Generated externally — this harness has no image generation.

**Date:** 2026-07-25 · **Decision this feeds:** cultural home, then PRD Decision A (layout)

---

## How to use this file

1. Generate **Prompt A** for all three styles first. A is the in-room shot and it is the one that
   answers your actual question — does this TV belong in that room, or does it look stapled on?
2. Generate **Prompt B** only for the styles that survive A. B is the flat screen render where you
   judge the UI itself.
3. Keep the same model and seed across all three within a set, or the comparison is worthless.
4. Aspect ratio: **16:9** for both A and B.

**Text will come out garbled.** Every image model mangles small UI text. Judge these on layout,
colour, hierarchy, material, and light — not on whether the words are spelled right. Where a prompt
names copy, it is there to shape the composition, not to be read.

**Scenario is identical across all six prompts** so the three styles are compared like-for-like.
Team names are placeholders; the product requires fictional teams and the naming voice is still
undecided.

---

## Shared scenario

> Soccer match, NORTHGATE 1 — 1 CALDER CITY, 67th minute. The viewer has a bet slip riding on it:
> moneyline, Northgate to win, third leg of four, fifty dollars at risk returning seven hundred
> eighty. A cash-out offer of three hundred twelve dollars is currently live and is the only
> action available.

---

## Shared room block

Paste this into any Prompt A. It is the locked room art.

> A cramped bunker-like room at night, painterly semi-realistic concept art. Heavily peeling paint
> on walls, ceiling and floor. Exposed black conduit and pipes running across every surface, bolted
> steel brackets. A heavy riveted industrial bunk bed frame on the left with a worn patched couch
> underneath it. A small deep-set window in the far wall showing a dark city skyline with scattered
> warm-lit windows. A battered metal desk on the right with an open laptop, a phone lying flat, and
> an ashtray full of cigarette butts. A metal stool. A coiled black cable on the floor. One wall-
> mounted fluorescent strip light casting sickly yellow-green light. Overall palette olive, khaki,
> drab yellow-green and deep shadow — desaturated, damp, institutional, oppressive. A large flat
> television in a heavy frame mounted on the right wall.

---

# STYLE 1 — RACE TELEMETRY

Modern motorsport broadcast graphics. Sector timing, gap deltas, angular geometry, precise
condensed numerals. Bets on being the best at dense live state.

### Prompt A — in room

```
A cramped bunker-like room at night, painterly semi-realistic concept art. Heavily peeling paint on walls, ceiling and floor. Exposed black conduit and pipes running across every surface, bolted steel brackets. A heavy riveted industrial bunk bed frame on the left with a worn patched couch underneath it. A small deep-set window in the far wall showing a dark city skyline with scattered warm-lit windows. A battered metal desk on the right with an open laptop, a phone lying flat, and an ashtray full of cigarette butts. A metal stool. A coiled black cable on the floor. One wall-mounted fluorescent strip light casting sickly yellow-green light. Overall room palette olive, khaki, drab yellow-green and deep shadow — desaturated, damp, institutional.

The large flat television on the right wall is ON and is the most alive object in the frame: a motorsport-style live telemetry broadcast, rendered with sub-pixel precision. Pure black screen ground with hard angular parallelogram panels, razor-thin bright rules, and a vertical timing tower down the left edge listing four ranked rows with delta times. Huge condensed sans-serif score numerals centred at the top. A small top-down green soccer pitch with bright tracked dots in the middle of the screen. Electric cyan, hot magenta and signal white accents — intensely saturated cool colours that contrast violently against the warm olive room. A single gold-amber control at the bottom edge, the only warm element on the screen. Crisp geometric motion streaks. The screen throws sharp cyan light onto the peeling wall and the wet floor.

Cinematic wide interior, deep shadow, one dominant screen light source. Photographic depth of field, 16:9.
```

### Prompt B — screen only

```
Flat-on UI design render, 16:9, filling the frame edge to edge. A motorsport-style live telemetry broadcast graphic for a soccer betting sweat. Pure black ground. Hard angular parallelogram panels with razor-thin luminous rules, no rounded corners anywhere. Vertical timing tower down the left edge: four ranked rows, each a bet leg, with delta figures and coloured state bars — green, yellow, violet. Enormous condensed sans-serif scoreline across the top, the largest element by far. Centre: a top-down soccer pitch in dark green with small bright tracked dots, one dot ringed and numbered. Right edge: a compact card reading a market requirement and live progress. Bottom edge: a full-width gold-amber armed control with a dollar figure, the only warm colour on the surface, seated in a machined bezel. Palette electric cyan, hot magenta, signal white, gold accent, on black. Extremely precise, expensive, engineered. Broadcast television graphics, not a website, no browser chrome, no rounded cards.
```

---

# STYLE 2 — HIGH-LIMIT ROOM

Premium casino machine design. Black glass, brushed metal, restrained gold, illuminated numerals.
Bets on being the sharpest irony — an immaculate expensive object in a condemned room.

### Prompt A — in room

```
A cramped bunker-like room at night, painterly semi-realistic concept art. Heavily peeling paint on walls, ceiling and floor. Exposed black conduit and pipes running across every surface, bolted steel brackets. A heavy riveted industrial bunk bed frame on the left with a worn patched couch underneath it. A small deep-set window in the far wall showing a dark city skyline with scattered warm-lit windows. A battered metal desk on the right with an open laptop, a phone lying flat, and an ashtray full of cigarette butts. A metal stool. A coiled black cable on the floor. One wall-mounted fluorescent strip light casting sickly yellow-green light. Overall room palette olive, khaki, drab yellow-green and deep shadow — desaturated, damp, institutional.

The large flat television on the right wall is ON and reads as an immaculate luxury machine faceplate that does not belong in this room: deep black glass with a brushed stainless bezel, machined seams, and precise illuminated numerals seated beneath the glass like a high-end instrument. A restrained scoreline in cool white illuminated digits. A column of small lit state lamps down one edge. A single gold illuminated control at the bottom in a machined recess, glowing, obviously touchable. A small top-down soccer pitch inset behind the glass. Everything on the screen is flawless, costly, and precisely made — total contrast against the crumbling plaster around it. Cool white and restrained gold light spills onto the peeling wall.

Cinematic wide interior, deep shadow, one dominant screen light source. Photographic depth of field, 16:9.
```

### Prompt B — screen only

```
Flat-on UI design render, 16:9, filling the frame edge to edge. A premium casino machine faceplate for a live sports bet, designed as a luxury object. Deep black glass ground with subtle reflection. Brushed stainless steel bezels and machined seams dividing the surface into fixed physical regions that could not possibly reflow. Cool white illuminated numerals seated beneath the glass showing a soccer scoreline, large and precise, like a high-end instrument display. A vertical column of small lit state lamps down the right edge, each a bet leg, in white, amber and dim red. Centre: a small dark top-down soccer pitch inset behind glass with fine bright dots, one ringed and numbered. Bottom: a single gold illuminated control in a machined recess showing a dollar figure, the only touchable-looking element, softly glowing. Restrained, expensive, exact. Black glass, brushed metal, cool white, restrained gold. Luxury hardware product design, not a website, no rounded app cards, nothing gaudy, nothing neon.
```

---

# STYLE 3 — STADIUM LED

The screen becomes the room's light source. Perimeter boards and jumbotron systems. Bets on
couch legibility and the strongest dopamine hit.

### Prompt A — in room

```
A cramped bunker-like room at night, painterly semi-realistic concept art. Heavily peeling paint on walls, ceiling and floor. Exposed black conduit and pipes running across every surface, bolted steel brackets. A heavy riveted industrial bunk bed frame on the left with a worn patched couch underneath it. A small deep-set window in the far wall showing a dark city skyline with scattered warm-lit windows. A battered metal desk on the right with an open laptop, a phone lying flat, and an ashtray full of cigarette butts. A metal stool. A coiled black cable on the floor. One wall-mounted fluorescent strip light casting sickly yellow-green light. Room surfaces olive, khaki, drab yellow-green — desaturated, damp, institutional.

The large flat television on the right wall is ON at overwhelming brightness and is unmistakably the dominant light source in the room, drowning out the fluorescent: a stadium LED perimeter-board display, emissive and saturated, with visible LED pixel structure and bloom. Enormous blocky scoreline type at perimeter-board scale, readable from anywhere. Broad fields of intensely saturated electric blue and hot magenta colour. A top-down soccer pitch rendered as a glowing emissive field with bright dots. Colour spills dramatically across the entire room — the peeling walls, the couch, the bunk frame and the wet floor are all washed in saturated blue and magenta light, completely overriding the olive palette. The room becomes the reaction shot.

Cinematic wide interior, extreme contrast, the television as the only meaningful light. Photographic depth of field, 16:9.
```

### Prompt B — screen only

```
Flat-on UI design render, 16:9, filling the frame edge to edge. A stadium LED perimeter-board display showing a live soccer bet. Emissive, saturated, high brightness, with visible LED pixel structure and gentle bloom between pixels. Enormous blocky condensed scoreline type at monumental scale dominating the upper half, readable from across a large room. Broad flat fields of intensely saturated electric blue and hot magenta, edge to edge, no timid whitespace. A top-down soccer pitch as a glowing emissive green field with bright dots, one dot ringed and numbered. A horizontal band of four saturated colour blocks as bet-leg states. A full-width bottom band in brilliant gold showing a cash-out dollar figure, taking over the whole width. Everything at maximum scale and saturation. Stadium signage, not a website, no rounded cards, no small text, no subtle greys.
```

---

---

# STYLE 3 — DENSITY TEST (run this one next)

Style 3 passed the atmosphere and alignment checks on the first render. What it has **not** been
tested for is its named risk: whether a perimeter-board language can carry six facts at once plus a
market-specific NEED line, or whether the spectacle crowds the information out.

This prompt puts the real sweat content on the screen. It is the last render needed before the
world locks.

**Two corrections from the first pass.** The model put real club names on screen — the product
requires fictional teams for IP safety and for the comedy, so the names are now stated positively
in the prompt rather than only banned in the negative. And the first render showed pre-match 1X2
odds, which is laptop/sportsbook content; the TV sweat is a live match with a ticket riding on it.

```
Flat-on UI design render, 16:9, filling the frame edge to edge, no room, no bezel, no environment. A stadium LED perimeter-board display showing a LIVE soccer match with a bet slip riding on it. Emissive and saturated, visible LED pixel structure with gentle bloom between pixels, deep black between the lit pixels.

Layout, top to bottom, in five non-overlapping horizontal bands:
1. Top band: enormous blocky condensed scoreline, the invented team names NORTHGATE and CALDER CITY in large LED type either side of the figures 1 and 1, with a match clock reading 67 at the far right edge. This is the largest element on the screen.
2. Main band, the biggest region: a top-down soccer pitch rendered as a glowing emissive field with pitch markings in bright thin lines, scattered with small bright dots as players in two colours, one single dot ringed and carrying a jersey number.
3. A narrow right-hand column beside the pitch: a compact active-leg panel reading a market requirement line and a live progress line in smaller LED type.
4. A thin band of four saturated colour blocks in a row, each a bet leg state, in green, bright cyan, dim grey and dim red.
5. Bottom band, full width: a brilliant gold LED band showing a cash-out dollar figure, taking over the entire width, clearly the one actionable element.

Palette electric blue, hot magenta, cyan, brilliant gold accent, emissive green pitch, on deep black. Everything at large scale and high saturation. Stadium signage language, information-dense but bold. Invented fictional soccer teams only.
```

Judge one question only: **can you read all five bands at a glance, or does the spectacle eat the
information?** If band 3 or band 4 disappears into the noise, Stadium LED needs a density
discipline borrowed from somewhere else, and I will bring that back as a specific proposal rather
than a new world.

---

## Negative prompt (append to any of the above if your model supports it)

```
website, web page, browser chrome, browser window, mobile app screenshot, rounded rectangle cards, material design, bootstrap, dashboard template, stock photo, watermark, logo, real football clubs, Premier League, Liverpool, Manchester City, Arsenal, Chelsea, real team names, real brand names, real sponsor logos, scanlines, CRT curvature, phosphor glow, vaporwave, retro arcade, 1980s nostalgia, cream paper, parchment, newsprint, vintage
```

The second half of that list matters: it bans the two ruts already rejected — the retro-CRT look
from the deprecated `design/08-art-direction.md`, and the old-school paper look from the Coupon
re-roll.

---

## What to judge

In order. Stop at the first failure.

1. **Alignment.** Does the TV look like it lives in that room's universe, or like two different
   games shoved together? This is the question that killed the last two rolls.
2. **Contrast.** Is the TV clearly the alive, expensive, rewarding thing against a dead room?
3. **Legibility.** Could you read a score and a requirement from across that room, squinting?
4. **Rut check.** Does it look like a sportsbook app, or like a website? Either is a fail.
5. **Density.** Is there room for six facts plus a market-specific NEED line, or does the style
   fight the information?

Styles 1 and 3 are opposite failure risks: Telemetry risks being too dense and too web-like,
Stadium LED risks being too loud to carry the information. High-Limit sits between them and risks
being cold.
