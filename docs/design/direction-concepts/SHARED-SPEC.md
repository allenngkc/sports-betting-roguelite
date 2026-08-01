# Shared spec — SureThing direction concepts

Every direction concept renders **the same screen with the same data**. Only the visual world
changes. This is what makes the three comparable.

Read `PRODUCT.md` at the repo root before building. It is the authority on product truth.

---

## The artboard

- **Exactly 1024 × 704 px.** Fixed. No responsive behavior, no breakpoints, no scrolling page.
  This is a Unity UGUI canvas mapped to a laptop screen in a 3D room, not a web page.
- The whole composition must fit in that box with nothing clipped and nothing scrollable except
  where the real UI would scroll (a long market list may scroll inside its own panel).
- Deliver **one self-contained `.html` file**. All CSS inline in a `<style>` block. No external
  fonts, no CDN links, no images from the network, no JS frameworks. SVG and CSS gradients drawn
  inline are encouraged. A small amount of vanilla JS for a hover/selected demo is fine.
- Center the 1024×704 artboard on a neutral dark page ground so it reads as a screen.

## Hard legibility constraints

These come from the laptop being viewed **in perspective, at an angle, at reduced scale**.

- Critical values survive a **50% thumbnail check**: bank, target, matchup names, the selected
  price, leg count, stake, potential payout, the primary action, and any disabled reason.
- Normal text holds **at least 4.5:1** contrast against its own background.
- **Never signal status by color alone** — pair every color state with a glyph, label, border,
  or position change.
- Cursor targets are at least **44×32 px**; no two targets closer than **8 px**.
- No hairline strokes and no low-opacity essential text. Both disintegrate on the angled surface.
- **13px is the floor for any text stating a product fact** — prices, records, field labels,
  state labels, disabled reasons, market navigation, column heads. 12px exists only for OS chrome
  carrying no product meaning (clock, taskbar, disk readout). Nothing goes below 12px at all.
  This was written after an audit found 24 sub-floor rules in a direction that had already
  passed review, including a disabled reason at 8.5px.

## Typography note

Unity ships **custom TextMeshPro fonts** for the real build, so you may design to a real face.
In the HTML mockup, approximate it with a system stack, then state the intended production face
in the direction contract comment at the top of the file. Do not reach for the exhausted
defaults: Inter-as-display, Space Grotesk, Space Mono, IBM Plex, DM Sans, Plus Jakarta, Outfit,
Fraunces, Playfair. Pick a face that belongs to your assigned world.

---

## The content to render — Round 3 lobby

Author this exact data. It is product-truthful.

### Persistent OS chrome (the laptop)

The redesign covers the **laptop OS chrome as well as the app**. The window frame, taskbar or
equivalent, and system affordances are yours to design in the world's grammar — they must not
look like a Windows or macOS clone. Include:

- A system clock reading `02:47`
- The machine's identity mark (the fictional OS name is yours to invent — it is not "SureThing";
  SureThing is the sportsbook app running on it)
- The SureThing app occupying the main working area
- At least one other affordance implying a real OS (a second app, a tray, a notification)

### Run context (persistent, always visible)

| Field | Value |
|---|---|
| Round | `3 / 8` |
| Bank | `$1,340` |
| Target | `$1,900` |
| Debt | none this run |
| Relics held | `2 / 5` |
| Tickets placed this round | `0 / 3` |

### The slate — 6 soccer matchups, moneyline shown on the lobby

Odds are **locked** — they never move after the slate is generated. Records are a noisy signal.

| # | Away (W-L) | Odds | Home (W-L) | Odds |
|---|---|---|---|---|
| 1 | Yams (7-2) | `-145` | Startups (4-5) | `+125` |
| 2 | Mallards (3-6) | `+210` | Bricklayers (8-1) | `-260` |
| 3 | Nighthawks (5-4) | `+135` | Foundry (6-3) | `-155` |
| 4 | Longhaulers (6-3) | `+180` | Tidewater (2-7) | `-215` |
| 5 | Saltmen (4-5) | `-110` | Junction (5-4) | `-110` |
| 6 | Kestrels (8-1) | `-300` | Pressmen (3-6) | `+240` |

Each matchup row needs an obvious entry to its **event detail** (where Goals, BTTS, Corners,
Cards and Players markets live). One selection per matchup — picking a second market on the same
matchup replaces the first.

### The betslip — currently 2 legs

- Leg 1: **Bricklayers −260** (Moneyline, matchup 2)
- Leg 2: **Longhaulers +180** (Moneyline, matchup 4)
- Combined odds: **+259**
- Stake: **$200**
- Potential payout: **$718**
- Stake controls: `10% / 25% / 50% / MAX` and `−$10 / +$10`
- Each leg has an explicit remove control

### Action states — render all of these visibly

1. **`PLACE TICKET`** — enabled. The slip is valid.
2. **`LOCK IT IN`** — **disabled**, and it must state cause *and* remedy in place:
   `PLACE OR CLEAR THIS WORKING SLIP`
3. **`SKIP ROUND`** — a separate secondary action, two-step confirmation.
4. Show at least one **selected** odds control in its selected state, and one **disabled** one.

### Voice

Transactional copy is literal and exact: "2 selections", "Potential payout $718", "Add a
selection to unlock." Satire is allowed only in non-critical flavor labels. Never imply a
guaranteed win. Never use a real operator's name, mark, or wording.

---

## The world this screen lives in — non-negotiable

Added 2026-07-28, after the room and TV directions were approved and finalised.

A cramped bunker room at night, in a **wealthy high-tech city that has no use for the occupant.**
The room is rotting; the city outside the window is neon and functioning; the screens are the only
things that work. Dark comedy about the gambling industry — **the machines are nicer than the
life.** Room palette is olive, khaki, drab green, rust, damp concrete, under a warm dim
fluorescent. The room physically cannot return saturated cool colour.

**The laptop is the occupant's own machine.** This is the single most important constraint and the
one most likely to be got wrong. The TV in the same room is a hardened industrial display
*installed by an institution* — steel, rivets, stencilled equipment code. The laptop is the
opposite: **personal, chosen, probably cheaper, probably grubbier, possibly customised.** If your
direction reads as institutional hardware, it has become a second TV and it has failed.

- **Lifted blacks.** Nothing on screen may be pure `#000000`. A screen whose blacks beat every
  shadow in the room reads as composited rather than photographed.
- **Diegesis.** This is a real screen on a battered metal desk, seen at an angle, with glass,
  reflection and dust. Nothing floats. The whole surface renders inside a unified post-process
  grade — grain, haze, bloom, chromatic aberration, vignette — so avoid anything that only works
  when perfectly clean.
- **Screen light is quiet, with faint spill.** A predominantly bright white surface would blast an
  olive room the lighting design has already approved. Design to a dark or mid ground.
- **Colour is yours to decide** (Allen, 2026-07-28). The TV uses gold for money; you are not
  bound by that, though coherence with it is worth pursuing by choice.
- **Fictional teams only.** Voice is dark comedy, satirical toward the gambling industry, never
  celebratory. Do not invent a name for the game.

## Register: calm to build, loud to sweat

This is a confirmed product decision and the hardest thing to get right.

- The lobby you are rendering is the **calm** state — the player is constructing a ticket and
  needs to think. Calm means **composed and confident, not plain, empty, or timid.** The surface
  should still be unmistakably designed and distinctive.
- The app is explicitly **not** a slot machine. Overstimulation is a failure condition. No
  pulsing, no confetti, no manufactured urgency on this screen.
- Your world must nonetheless own a **native loud register** it can escalate into during the
  sweat. In a comment block at the bottom of the file, describe in 3–4 lines what the loud state
  looks like in your world's own grammar — using motion the world actually has, not effects
  bolted on.

## Direction contract — required, at the top of the file

Open the HTML file with a comment block, five short blocks, 150 words max:

- **THESIS** — the one idea this surface owns, and the category-default arrangement it refuses.
- **OWN-WORLD** — palette and component language, specific enough to be recognizable with all
  content removed.
- **STORY** — what the player understands, believes, and does.
- **FIRST VIEWPORT** — the exact composition: what is where, at what scale, where the primary
  action sits.
- **FORM** — the world you were assigned and the production typeface you intend for Unity.

If a block reads like a mood rather than a decision, the direction is not decided yet.

---

## What the category always ships — stay out of it

The rut, which no direction may drift back into:

- **The modern sportsbook app:** near-black or navy ground, one saturated accent, rounded cards,
  aligned odds pills, a floating betslip drawer, promo rails. The previous design package already
  shipped exactly this in violet; it is the anti-reference.
- **Its predictable opposite:** green-phosphor CRT, scanlines, ASCII borders, monospace
  everything as a costume. Reaching for it is the same failure wearing different clothes.
- **The dystopian-city default:** cyberpunk neon-glow-on-black, cyan/magenta rim light, glitch
  displacement, katakana filler, HUD reticles. A brief that says "neon city, dystopian" makes this
  the single most predictable place to land, which is exactly why it is out of bounds. The city is
  neon; **the occupant's own cheap laptop is not.**
- **Skeuomorphic kitsch.** A world drawn from a physical object must become a real operable
  interface, not a photograph of that object with buttons pasted on. Torn edges, drop-shadowed
  paper and faux-leather stitching are the failure mode of every material-led direction.

A world may legitimately use terminal or broadcast grammar if it commits to that grammar as a
real interface language across navigation, content, controls, and states — not as texture
sprinkled over the rut.
