# SureThing Design System

The studio design system for an unnamed **sports-betting roguelite**: across 8 rounds the player
builds parlays from a procedurally generated slate, stakes against a rising bank target, then sweats
each ticket leg by leg while a live cash-out offer taunts them. Surviving a round opens a relic shop.

The satire thesis, which nothing else in the category copies: **the bettor watches the app, not the
game.** The sportsbook interface is the protagonist surface — the place where hope is manufactured and
money dies. Dark comedy about the gambling industry, satirical toward it and never celebratory of it.

**The game itself has no name.** "The game name is deferred. Do not invent one." SureThing is the
in-world sportsbook; the fictional OS it runs on is NOTEBOOK. Both are originals — no real operator
marks, copy, iconography, screenshots or characteristic colour systems appear anywhere in this system.

## The three surfaces

Everything here belongs to one room, and the division between surfaces is a **hard product law**, not
a layout preference.

| Surface | Object | Register | Owns |
|---|---|---|---|
| **SureThing** | The laptop on the desk — *his own machine* | **Calm.** A tool you operate. | Slate, markets, working slip, stake, staging, lock, the relic shop, the record of placed tickets |
| **TV sweat** | A hardened industrial display *installed by an institution* | **Loud.** An instrument you watch. | Unrevealed drama: score, clock, win-probability movement, outcome reveals |
| **The room** | A cramped bunker at night in a wealthy high-tech city | Painterly semi-realistic | Light, material, atmosphere, and the unified grade every screen renders inside |

**The most important constraint in the whole system, and the one most likely to be got wrong:** the TV
was bolted into the wall by someone else — riveted steel, chipped paint, stencilled equipment code,
conduit continuous with the room's pipe runs. The laptop is the opposite: **personal, chosen, probably
cheaper, probably grubbier, possibly customised.** If SureThing reads as institutional hardware it has
become a second TV and it has failed.

If the two screens feel the same, one of them is wrong.

## Where this came from

Built from the design-inheritance corpus handed over on **2026-07-31** — every canonical design record
the studio had produced across three slices plus the studio-wide bible. Paths below are as they arrived
in the mounted folder `design-inheritance-2026-07-31/`. **No repo, Figma file or live URL was provided**;
there is no other source.

| Document | Role here |
|---|---|
| `surething/PRODUCT.md` | Product truth, surface ownership, accessibility floors, scope |
| `surething/direction-concepts/DESIGN.md` | **Approved SureThing visual contract** — colours, type, layout, components, states |
| `surething/direction-concepts/element-kit.html` | Every SureThing control at real pixel size, in every state |
| `surething/direction-concepts/direction-1-form-guide.html` | The approved lobby, rendered |
| `surething/direction-concepts/SHARED-SPEC.md` | Fixed artboard, the Round 3 slate, legibility contract |
| `surething/direction-concepts/DIRECTIONS.md` | Why this world won and which seven were considered |
| `surething/direction-concepts/assets/` | The generated ink sprites, and `ASSETS.md`, their pipeline |
| `tv/DESIGN.md` | **Approved TV visual system** — FINAL 2026-07-27 against concept render C/G |
| `tv/VISUAL-DESIGN.md` | Approved Layout B, type hierarchy, component copy, cash-out states |
| `tv/unified-grade-spec.md` | The single post-process pass covering room and TV together |
| `room/SIGNOFF.md` | Room acceptance — Direction B "Vice Grip", Palette 1, two-bunk layout |
| `room/graded/`, `room/concepts/` | Rendered captures (see the caveat under *Imagery*) |
| `studio/00-vision.md` | Pillars, tone, reference games, satire stance |
| `surething/surething-ui-revamp/` | The **superseded** violet package — retained as evidence of information architecture and behaviour only |

**Precedence, as the corpus itself defines it:** `PRODUCT.md` → `SHARED-SPEC.md` → the direction
contract plus element kit → `ASSETS.md`. `tv/DESIGN.md` supersedes `studio/08-art-direction.md`
(deprecated 2026-07-24 — the casino-neon/CRT/green-red-gold world is now an anti-reference).
`direction-concepts/` supersedes `surething-ui-revamp/`. `room/SIGNOFF.md` is the room authority.

---

## CONTENT FUNDAMENTALS

### The one rule

**The number never lies.** Every price, stake, payout, state and disabled reason is literal and
present. Satire never occupies a slot where a fact belongs.

### Register

Personality: **incisive, nocturnal, dry, orderly.** Anti-personality: celebratory gambling hype, fake
urgency, casino glitter, buddy-bro language, ambiguity. Voice is dark comedy, satirical toward the
gambling industry and **never celebratory of it**. Never imply a guaranteed win.

### Person and address

Copy is **impersonal and transactional** — it names the thing, not the reader. "2 selections", not
"you've picked 2". "Add a selection to unlock", not "you need to add a selection". Second person
appears only in imperatives that are genuinely instructions: `PLACE OR CLEAR THIS WORKING SLIP`.
First person appears exactly once, and it is not the product speaking — it is *him*, in the margin
he owns: **MY MARKS**. Nothing else in the system says "my" or "I".

### Casing

- **Tracked uppercase** for short labels, field keys, column heads, state words, action labels, tabs,
  and system chrome: `BANK`, `POTENTIAL PAYOUT`, `MONEYLINE · ENTRY 02`, `LOCK IT IN`, `DISK 61% FULL`.
- **Title case** on the masthead and the primary action as rendered in the approved concept:
  `SureThing Form`, `Place Ticket`, `Lock It In`, `My Marks` — set in the condensed face and then
  transformed to uppercase in the type, so the source copy stays readable.
- **Sentence case** for the few running-text lines: "Prices are final. Nothing you do moves them."
- Team names are printed uppercase in the form, as a directory would print them.

### Numbers

American odds always carry their sign (`-260`, `+180`, `+259`). Money always carries `$` and a
thousands comma (`$1,340`). Counts are literal fractions (`2/5`, `0/3`, `ROUND 3 OF 8`). Records are
`W-L` (`8-1`) and are described in the corpus as **a noisy signal** — the form prints them without
comment. Tabular figures throughout, so nothing twitches when a value changes in place.

### Examples

| Write this | Never this |
|---|---|
| `2 selections` | `You're 2 legs from glory!` |
| `Potential payout $718` | `Up to $718 — cash in now` |
| `Add a selection to unlock.` | `Nothing here yet 🙁` |
| `PLACE OR CLEAR THIS WORKING SLIP` | `Can't lock yet` |
| `MARKET SUSPENDED` | `Hang tight…` |
| `BANK TOO LOW` | `Something went wrong` |
| `Prices are final. Nothing you do moves them.` | `Odds may change — bet now!` |
| `LIVE • 2 GOALS • 1 MORE` | `So close!!` |
| `DEAD` / `GREEN` | 🟥 / 🟩 |

### Where satire is allowed

Only in non-critical flavour labels that state no product fact — a sticker on the laptop
(`property of nobody`), a tray readout (`NO UPDATES AVAILABLE`), a masthead aside, a relic
description (`Reveals one guru's pick per round. The guru is wrong more often than not.`). It never
replaces an odds label, a state, a price, a disabled reason or a consequence.

### Emoji

**Never.** Not in the product, not in labels, not in flavour text, not as status. The system has no
emoji anywhere and no place for them.

### Fiction

**Fictional leagues, teams and players only** — IP safety and the comedy both require it, and concept
renders came back with real clubs three times, so watch for it. The naming voice is deadpan
occupational and civic: *Bricklayers, Longhaulers, Foundry, Nighthawks, Kestrels, Pressmen, Saltmen,
Junction, Mallards, Startups, Yams, Tidewater.* Players follow the same register: *Marcus Vale,
Osric Kean, Ivo Tanager.*

### Errors and disabled states

A blocked action states **cause and remedy, in place, at 13px, inside the house's stamp**. Never a
tooltip, never a colour change alone, never a remedy the engine did not supply. When the engine or
director returns an error, print that error — do not re-validate rules in the UI and do not invent a
fix.

---

## VISUAL FOUNDATIONS

### The idea

**The Annotated Form Guide.** SureThing is the occupant's cheap, personal document reader at 2 a.m.
The house printed a dense inverted betting form; the player compares it, circles prices in ballpoint
blue, works the right margin, and commits. **Selection is annotation, not a pill that lights up.** The
document never changes; only his marks accumulate.

The document is *inverted* — his cheap reader's night mode — because a photocopied-white surface would
blast an olive room whose lighting design is already signed off, and because toner grain, registration
offset, stamp marks and biro all read *better* as scanned artefacts on a dark ground.

### Colour

Three grounds, three toners, two player inks, one house mark. See `tokens/palette-surething.css`.

- **Grounds** — `--ground #16160F` canvas, `--ground-2 #1C1C13` recessed bands, `--ground-3 #232319`
  raised chrome. Warm, olive-adjacent, lifted.
- **Toner** — `--toner #D9D4C5` facts, `--toner-2 #9C9888` secondary, `--toner-3 #6E6B5E` labels and
  the floor for readable text.
- **Biro `#5E86B8`** — anything *he* chose. **Wax `#D9A441`** — money and the primary action.
  **Stamp `#B4483A`** — the house acting on the document, and nothing else.
- **Nothing borrows another ink's meaning.** When one appears where its meaning does not apply, the
  surface has started lying.
- **No pure black anywhere.** Not on the laptop, not on the TV, not in the room's darkest shadow.

The TV runs its own palette: near-black substrate, cold white for fact, grey for context, dim grey for
structure, **gold rationed** to money/won/cash-out, and *muted* blue and pink confined to the pitch
dots. Team identity is carried by words in the ticket column; the dots only need to be separable.

Colour vibe of imagery: **warm, dim, desaturated, grainy.** Olive, khaki, drab green, rust, damp
concrete under a warm dim fluorescent, with one cool window whose reach is short and local. Cool and
saturated is the failure mode — the wall's own albedo physically cannot return it.

### Type

Two faces do all the work: a **document data face** for running text and labels, and a **condensed
face** for the masthead, figures, prices, team names and action labels. Production intent is
**Bell Centennial** — Carter's telephone-directory face, cut with ink traps for legibility at small
sizes on cheap absorbent paper, which is precisely this document's problem. The TV's face is still
open; its brief is a technical grotesque with **mandatory tabular numerals**.

Seven sizes, one floor: `31 / 26 / 21 / 19 / 16 / 13 / 12`. **13px is the floor for any text stating a
product fact.** 12px exists only for OS chrome carrying no product meaning. Nothing goes below 12px.
Short labels are tracked uppercase (`.08em`–`.15em`); factual copy stays literal.

### Layout

One fixed **1024 × 704** composition, mapped to a roughly 0.32 × 0.22 m world-space laptop surface and
read at an angle. Not responsive, not scrollable, not a web page. The bands are locked:

`34 rail + 38 tabs + 68 masthead + 530 work area + 34 tray = 704`, with the work area split
**700px house form / 324px player margin**. The margin's vertical order never changes: header, legs,
combined, stake, payout, actions. Only interior market lists may scroll. Persistent chrome — rail,
tabs, masthead, tray — is present on FORM, ENTRY, MY BETS and REWARDS and does not rebuild when a
destination changes.

The TV is **980 × 550**, Layout B "Ticket Rail": full-height ticket column at the left edge
(**26–28%** of the width, corrected down from ~37%), compact scorebug and stage filling the right,
cash-out anchored at the foot of the ticket. Every zone position comes from an explicit fixed grid
defined once in code, never computed from content, and **no zone resizes in response to content** —
reserved space stays reserved and simply goes dark, so nothing ever reflows.

### Backgrounds

Flat tonal grounds. **No gradients as decoration** — the only gradient in the system is the faint
90° biro wash behind a marked form entry, and it fades to transparent by 70%. The margin carries a
`repeating-linear-gradient` paper ruling at 25/26px. The whole screen sits under a local **toner grain**
layer at `0.05` opacity, which is the document's own grain and lives *beneath* the room's grade, never
instead of it. No hero imagery, no full-bleed photography, no illustration, no repeating decorative
pattern, no promotional art. Density earns trust; acquisition art is an anti-feature.

### Cards, borders, radii, elevation

**There are no cards.** This is a flat printed document, not a card system. Depth comes from three
tonal grounds, structural rules, the physical rail/tray, and player ink over house toner.

- **Corner radius is `0` everywhere** (`--radius: 0`). Controls are square unless an irregular source
  asset supplies the silhouette.
- Borders are **solid 1–2px** `--rule`. Never hairlines — they disintegrate on the angled surface.
  Dotted is used once (margin leg separators); dashed is used twice (the replacement underline and the
  unarmed SKIP).
- **No drop shadows, no bevels, no glassmorphism, no glossy anything.** The single shadow in the system
  is a 2px hard `--wax-deep` edge under PLACE TICKET, which is a button *edge*, not a shadow. The one
  exception outside the screen: flat mockups drop the whole 1024 × 704 panel onto its page ground with
  a soft shadow, because that is a presentation frame and not part of the interface.
- The only "highlight" is the **hand-laid wax band** behind the payout: 6px tall, `0.26` opacity,
  rotated `-0.5deg`. One figure at a time may carry it.
- On the TV: structural panels may sit on a subtly raised ground — a flat value step, never a floating
  card. A **stroked box around a zone is banned**; zones are separated by hairline rules or unlit
  gutters, both native to that substrate.

### Transparency and blur

**Blur is never used.** No backdrop-filter, no frosted panels, no glassmorphism. Transparency is used
in exactly three places and always for a material reason: the biro wash on a marked entry, the wax
highlight behind the payout, and the toner-grain overlay. **Low-opacity essential text is banned** —
on the TV, brightness *is* the semantic channel and is applied as opacity by tier, but every tier
except `L0` is a legible tier, and `L0` means the thing is dead.

### Hover, press, focus

| State | SureThing treatment |
|---|---|
| Hover, price | The figure lifts to `--wax-lit`. **No fill, no background change, no scale.** |
| Hover, ruled control | The border firms — `--rule` → `--toner-3`, `--biro` or `--stamp` depending on what the control does |
| Hover, primary | `--wax` → `--wax-lit` |
| Press, primary | The 2px `--wax-deep` edge drops and the button moves **2px down** |
| Focus | **2px `--wax` outline at 1px offset**, visible against every ground |
| Selected | A drawn biro ring over the figure, plus a leg written into the margin, plus the count |

Nothing shrinks, nothing scales, nothing glows, and there is no ripple. Hover never starts a loop —
"no false-urgency loop" is written into the state matrix.

### Motion

**The laid-ink rule.** Motion is continuous, hand-paced, and caused by document marking. The corpus
deliberately specifies **no duration and no easing curve** for SureThing and forbids inventing a flashy
motion system, so this system asserts none either: `--st-ease` exists as a plain, unremarkable curve
and nothing else. Never a bounce, never a spring, never a pulse loop, never confetti.

- Leg goes live → the entry lifts toward toner; his ring holds.
- Leg wins → the figure fills wax; the ring is re-inked over the top.
- Leg dies → `strike-a` is drawn across; the entry drops toward ground.
- Return changes → the margin tally is crossed out and rewritten beneath, in the same ink.

The TV is the deliberate opposite: its native motion is **panel refresh**. State changes are
**quantised** — a brightness level swaps in a discrete step, no 300ms colour drifts. Actors and the
ball move continuously; everything else changes discretely. One pulse kind exists on the whole
surface (`LIVE`), and concurrent live legs pulse **in phase off one shared clock**. Score, count and
event land on the **same frame** as their causal callback: a change that arrives early is a lie. No
camera shake, cut or zoom. Standing freezes everything, including bloom decay and pulse phase.

### Protection, capsules, fixed elements

No protection gradients and no capsules — nothing in this system floats over imagery, so nothing needs
protecting. Text sits on a flat ground with ≥4.5:1 contrast, full stop. Fixed elements are the OS rail,
the tab strip, the masthead and the tray on the laptop; the ticket column, scorebug and cash-out
rectangle on the TV. **Nothing floats in a HUD** — diegesis is non-negotiable: every interface is a real
screen on a real object in a real room, viewed at an angle, with glass, reflection and dust.

### The grade

Grain, haze, lifted blacks, bloom, chromatic aberration and vignette belong to a **single global
post-process pass** covering the room and every screen in it. One grade, one grain, one bloom curve,
one vignette, one aberration budget. **Nothing in this system may depend on being pixel-clean.** Local
document grain sits *under* that pass, never as a substitute for it.

### The rut — out of bounds

1. **The modern sportsbook app.** Navy ground, one saturated accent, rounded odds pills, aligned pill
   rows, a floating betslip drawer, promo rails. The previous package shipped exactly this in violet;
   it is the anti-reference.
2. **The retro terminal costume.** Phosphor green, scanlines, ASCII borders, monospace as decoration.
3. **Cyberpunk neon-on-black.** Cyan/magenta rim light, glitch displacement, katakana filler, HUD
   reticles. The city is neon; **the occupant's cheap laptop is not.**
4. **Skeuomorphic kitsch.** Torn edges, drop-shadowed paper, faux stitching. A world drawn from a
   physical object must become a real operable interface, not a photograph of that object with buttons
   pasted on.

Also banned: colour-only status, sub-floor product text, low-opacity facts, and the TV's institutional
vocabulary anywhere on the laptop.

---

## ICONOGRAPHY

**This system is almost entirely iconless, and that is a decision, not a gap.**

There is **no icon font, no SVG icon set, no PNG icon set, and no icon library** anywhere in the
inherited corpus, and none has been added. The corpus's own instruction is the opposite of an icon
system: `RUB OUT` is specified as *"an explicit 60 × 32px removal target, never a tiny unlabeled ×."*
Where another product would draw a glyph, this one **prints the word**.

### What actually exists

- **The ink sprites** (`assets/ink/`) — the only real image assets in the system. Six generated marks:
  `ring-price-a/-b/-c` (112 × 46), `ring-wide-a/-b` (176 × 46), `strike-a` (112 × 46), each at `@1x`
  and `@2x`. White RGB with **all the ink in the alpha channel**, tinted at runtime by `Image.color`,
  so one asset serves every ink. These are the system's iconography.
- **Six unicode glyphs, used sparingly and always beside a word or a number:** `✓` (a leg he checked),
  `⇄` (this selection will replace the existing one), `›` (MORE — enters event detail), `‹` (back),
  `·` and `•` (fact separators). Nothing else.
- **CSS primitives, not icons:** the 11px square identity mark on the OS rail, the 20 × 9 battery
  outline, the 5px tray-app dot, the TV's pitch markings, and the TV's actor cells. All drawn with
  borders and boxes because they are diagrammatic, not symbolic.
- **Emoji: never.** Not in the product, not in labels, not as status.

### Rules for consumers

- **Do not add an icon set.** No Lucide, no Heroicons, no Material, no substitution — none is defined
  by the source, and adding one would put glyphs where the system deliberately puts words.
- If you need to name an action, **write the word** in tracked uppercase at ≥13px.
- If you need a mark, use the ink sprites via `InkMark`, tinted to the ink whose meaning applies.
- Never draw a padlock. Two separate agents drew one on an alternative price and both were wrong —
  a selectable market on an already-marked matchup is a **replacement**, not a block.

### Logo

**There is no logo, wordmark file, or brand mark of any kind in the sources, and none has been
created.** Where a mark would go, the name is set in the condensed face — which is exactly what the
approved concept does. See `guidelines/brand-wordmark.card.html`. Do not draw, reconstruct or
approximate one.

### Imagery

The three captures in `assets/room/` are **evidence of the current shipped state, not visual
authority**. In them the laptop still shows the superseded violet package, the TV still ships green
against its approved cold white-grey, and the grade reads cool-blue overall — which the 2026-07-28
palette law calls the explicit failure mode. `assets/room/concept-b-tactile-pressure-box.png` is the
accepted room concept and is also cool-blue, predating that law. Use them to understand construction,
layout and mood; do not sample them for colour.

---

## Index

### Root

| File | What |
|---|---|
| `styles.css` | The single entry point consumers link. `@import` lines only. |
| `readme.md` | This guide. |
| `SKILL.md` | Agent-skill front matter, for using this system in Claude Code. |
| `register-entries-2026-07-31.md` | Design Director rulings, batch 1 — typeface, form-guide identity, lost-ticket oxide, naming, slip-strip. For transcription into `main-2/docs/design/REGISTER.md`. |
| `register-entries-2026-07-31-batch-2.md` | Design Director rulings, batch 2 — C3 HDR coverage, bloom legibility floor, momentum tape, R5/R6 design-verified, R9/R10, the R7 review questions. |
| `proposal-art-authority-2026-07-31.md` | Proposal for Allen: the two-tier replacement for the deprecated `08` art direction. |
| `thumbnail.html` | The system's homepage tile. |

### `tokens/`

`fonts.css` · `palette-surething.css` · `palette-tv.css` · `palette-room.css` · `typography.css` ·
`space.css` · `motion.css` · `semantic.css` · `base.css`

### `assets/`

`ink/` — the six generated ink sprites at `@1x` and `@2x`, plus the contact sheet.
`room/` — three graded runtime captures, the accepted room concept, and the room treatment map.

### Components

Grouped by concern. Each directory carries a `@dsCard` thumbnail; every component has a `.d.ts` props
contract and a `.prompt.md` usage note.

**`components/form/`** — the house's document
`PriceCell` · `InkMark` · `MoreButton` · `ColumnHead` · `FormEntry` · `MarketOffer`

**`components/margin/`** — his marks, the right 324px
`MarginHeader` · `MarginLeg` · `MarginRow` · `StakeButton` · `StakeControls` · `RubOutButton`

**`components/actions/`** — commitment and refusal
`PlaceAction` · `LockAction` · `SkipAction` · `StampReason`

**`components/figures/`** — money and counts
`RunFigure` · `PayoutFigure`

**`components/os-chrome/`** — his machine, around the app
`OsRail` · `SectionTabs` · `Masthead` · `OsTray`

**`components/records/`** — tickets, the revealed mirror, the shop, the Ledger
`TicketReceipt` · `RevealedLeg` · `RevealedState` · `OfferEntry` · `LedgerEntry`

**`components/tv/`** — the match theatre
`TvScorebug` · `TvLegRow` · `TvRiskPays` · `TvCashOutSlot` · `TvEventStrip` · `TvMomentumTape` ·
`TvStage` · `TvStatsPanel` · `TvTicketCard`
(plus `tiers.js`, the brightness ladder helper — not a component)

### UI kits

| Kit | What |
|---|---|
| `ui_kits/surething/` | The laptop sportsbook and its OS chrome at 1024 × 704 — FORM, ENTRY, MY BETS, REWARDS and LEDGER, click-through with real selection, replacement, staking, staging and locking |
| `ui_kits/tv-sweat/` | The match theatre at 980 × 550 — Layout B, one ticket swept beat by beat, all six cash-out states, stats panel, ticket interstitial |

### Foundation cards

`guidelines/` holds 28 specimen cards across **Colors** (9), **Type** (5), **Laws** (5), **Space** (4),
**Brand** (3), **Motion** (1) and **Voice** (1).
Start with `law-registers.card.html` (the two surfaces), `law-two-inks.card.html` (the colour
grammar) and `type-fact-floor.card.html` (the legibility contract).

### Intentional additions

Everything in `components/` maps to a family the sources define — the element kit's price cell, market
navigation, actions, stake controls and figures; the direction contract's rail, tabs, masthead, tray,
column head, entries, margin, receipt, revealed mirror, shop and ledger; and `tv/DESIGN.md` §7's
scorebug, leg rows, risk/pays, cash-out slot, event strip, stage and stats panel plus
`VISUAL-DESIGN.md` §9's ticket interstitial. Two additions were needed to make that inventory usable:

- **`InkMark`** — a wrapper for the six generated sprites. The sources define the assets and their
  geometry rule but no component; every ring and strike in the system needs one place that applies
  `ring = cell + 16px, offset −8/−8` and tints by meaning.
- **`RevealedState`** — the literal state word plus its mark. `ASSETS.md` specifies that the MY BETS
  "won" ring must be sized from the state text's own measured box rather than its container; that
  behaviour needs to live in a component or it will be re-broken.

No Toast, Avatar, Tabs-as-primitive, Modal, Tooltip or Switch exists here, because no source defines
one. Do not add them.

---

## Provisional and unratified — read before using

1. **SureThing's typeface is now ruled; the TV's is still open.** Ruled 2026-07-31 under S11 (no
   licence-encumbered typefaces anywhere in the product): SureThing uses **Archivo** + **Archivo
   Narrow**, SIL OFL 1.1, and **Bell Centennial is dropped**. Rationale in
   `register-entries-2026-07-31.md` (S11-A) and on `guidelines/type-faces.card.html`. The **TV face
   remains open** and must *not* be Archivo — the two screens have to feel like one hand doing
   different jobs, and a shared superfamily collapses that split; Archivo stands in on the TV until
   that pick is made. Note that Archivo is wider than Bell Centennial, so a number of labels carry
   `white-space: nowrap` to hold their one-line contract.
2. **Every TV hex is provisional.** `tv/DESIGN.md` §4 names roles — cold white, grey, dim grey, gold,
   muted blue, muted pink — but gives no values, and the table that does have values was explicitly
   superseded. The hexes in `tokens/palette-tv.css` are lifted or desaturated from that superseded
   table and need ratification against the real panel at the real seated camera distance.
3. **The TV type tables disagree with each other.** `DESIGN.md` §5's relative ratios (score 1.00,
   labels 0.22) and `VISUAL-DESIGN.md` §3's reference px (score 36, eyebrows 14–16) cannot both hold.
   Both are encoded — ratios as the hierarchy law, px as one provisional instantiation. The px table
   also predates the ticket column being narrowed from ~37% to 26–28%, which is why long statements
   are tight in the kit.
4. **The price-cell size has two accepted answers.** The element kit and `DESIGN.md` say 96 × 30 with
   a 112 × 46 ring; `ASSETS.md` (2026-07-30, fixing a real defect) says the shipped control is the
   112 × 32 AWAY/HOME button with a 128 × 48 ring. Both are tokenised (`--st-price-w` /
   `--st-price-w-runtime`), and `PriceCell` takes `size="kit" | "runtime"`. The +16px rule is the
   durable part.
5. **`LockAction`'s enabled treatment is an inference.** `DESIGN.md` specifies the disabled state
   exactly and calls LOCK a "52px ruled control", but does not describe it enabled. This system renders
   it ruled in both states with a 2px `--wax` border when live, so a second solid amber field never
   competes with PLACE. Confirm or correct.
6. **Room palette values are spec-derived, not sampled.** The wall albedo is converted from the
   linear value in `PRODUCT.md`; the rest of the room family is derived around it plus one sampled
   lit-wall value. The graded captures could not be sampled for colour because they contradict the
   palette law (see *Imagery*).
7. **Three conflicts were open in the register and have since been ruled** (C1 layout closed, C2
   interim on TV light spill, C3 HDR coverage — the last by this seat on 2026-07-31, see
   `register-entries-2026-07-31-batch-2.md`). Remaining documentation debt: TV PRD §14.1 still carries
   the deprecated `08` colour language, and four room documents still assert the revoked palette laws.
8. **The room is documented, not built here.** It is a 3D environment, so this system carries its
   palette, its laws and its captures — not a UI kit.
