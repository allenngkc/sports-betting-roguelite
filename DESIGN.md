# Design

<!-- impeccable:design-schema 1 -->

**Scope:** the TV sweat surface. This is the durable visual system for the match-theater screen —
the television inside the room. It does not govern the laptop/SureThing sportsbook, the phone, or
the room itself, all of which have their own authority.

**Supersedes:** `design/08-art-direction.md`, deprecated by Allen 2026-07-24. That world — casino
neon on black, CRT phosphor and scanlines, green/red/gold colour purity — is an explicit
anti-reference. Landing back on it means the redesign did not happen.

**Status: FINAL, approved by Allen 2026-07-27** against concept render G. The visual direction is
closed after seven concept rounds. Numeric values marked *provisional* still settle against the real
TV at the real seated camera distance, but the world, fidelity, palette, layout and state vocabulary
are decided and are not reopened without an explicit ruling.

**One implementation note that the approved render gets wrong.** Concept G displays risk and payout
*per leg*. That is not how a parlay works and is not what this product does — PRD §8.4 specifies a
single ticket-level `RISK` and `PAYS`. The render's composition is approved; that detail follows the
PRD, not the render. Implementers: one risk figure, one payout figure, at the foot of the ticket
column.

---

## Direction contract

**THESIS.** The TV is the only thing in that room that still works, and it is beautiful on purpose —
the expensive machine glowing in a condemned space, which the player paid for and keeps paying for.
It refuses the arrangement this category always ships: rounded cards floating on dark grey, an
accent colour, and a bottom action bar.

**OWN-WORLD.** One stadium LED matrix. Black substrate, light as the only ink, visible pixel pitch,
halation between lit cells. Brightness carries meaning; hue carries identity. Electric blue and hot
magenta are the two teams, gold is money and action, white is fact, and death is pixels going dark.

**STORY.** The player reads their ticket first and the match second, understands what the leg still
needs, and knows at a glance whether they can get out.

**FIRST VIEWPORT.** Layout B. Full-height ticket column at the left edge, compact scorebug and stage
filling the right, cash-out anchored at the foot of the ticket.

**FORM.** Stadium LED, **user-pinned** — chosen by Allen from a three-option cultural-home steer
after two re-rolls (seed `17c08d3d`), not roll-assigned. The dealt staging, a held preview before
commitment, remains unresolved; see §10.

---

## 1. The physical scene, and what it forces

A person sits on a torn couch in a dark, damp room at night with the lights off, roughly four metres
from a wall-mounted panel, watching money resolve, with the sound off.

That sentence decides more than any preference could:

- **Dark ground is not a style choice.** A bright surface in that room would be painful and would
  destroy the contrast the product depends on.
- **Everything must survive four metres.** Nothing small enough to require leaning forward may carry
  required information.
- **The surface is a light source, not a picture.** It throws blue and magenta into the room. The
  room's lighting rig is being briefed to carry this.
- **Muted is the default, not the accessible fallback.** No state may depend on sound.

## 2. The substrate: maintained industrial equipment

**Revised twice. This is the settled version, 2026-07-26.** First the LED matrix, then briefly a
high-end consumer panel, now this. Allen's ruling after the A/B/C exploration: **B's enclosure, and a
display fidelity between A and C.**

### The enclosure

The screen is **not a television**. It is a hardened industrial display bolted into the wall —
riveted steel frame, thick chipped paint, recessed glass, a stencilled equipment code, one physical
indicator lamp, conduit feeding into it continuous with the room's own pipe runs.

This is the change that finally seated the screen in the room. Every earlier concept read as a nice
TV pasted onto a bad wall. Making the enclosure part of the building's own construction — the same
riveted, painted, institutional language as the bunk frames — removes the seam. **The display was
installed by an institution; it was not bought by the occupant.**

The enclosure is a room prop and is briefed to the room lead in `room-layout-update.md`. It is not
this slice's to build, but the design depends on it.

### The fidelity target: old but maintained

Direction A was too degraded, C too high-fidelity. The target sits between them, and the framing that
resolves it:

> **The display is a decade old and works perfectly. It is not failing, and it is not new.**

| | Too far toward A | **Target** | Too far toward C |
|---|---|---|---|
| Resolution | Dithered, chunky, low-res blobs | Sharp forms on a visibly **coarse** grid — crisp, but never sub-pixel smooth | Fine hairlines, high-DPI, anti-aliased everything |
| Colour | Near-monochrome amber only | **Restricted but real**: amber, white, two team hues, black. No gradients, no subtle greys | Full range with soft tonal steps |
| Type | Chunky low-res letterforms | Technical, uppercase, medium weight, slightly condensed | Light weights, generous tracking, editorial |
| Rules | Absent or noisy | Solid 1–2px, visibly drawn | Anti-aliased hairlines |
| Imperfection | Signal noise, interference banding, scanlines | Slight uneven backlight, amber bloom | Pristine |

**Still banned, unchanged:** scanlines, screen curvature, phosphor haze, interference noise, and any
other treatment that says *broken*. The deprecated `08-art-direction.md` world is still the
anti-reference. "Old but maintained" is a long way from "failing", and A's degradation is what put it
too close to the rut.

### Colour is settled separately

Fidelity and palette are **independent axes**, and treating them as one produced two failed concepts
in a row. Coarsening the rendering does not imply narrowing the colour, and vice versa.

Fidelity: coarse, per the table above. Palette: **§4**, taken from concept render C.

**The boundary that still holds.** The deprecated `08` world is banned for its *artifacts* —
scanlines, screen curvature, phosphor haze, interference noise, degradation. A coarse grid is not a
CRT. Keep the coarseness, keep the ban.

| Rule | Why |
|---|---|
| Crisp vector rendering, high resolution | The panel is expensive. Aliasing, chunky pixels, and low-res artefacts all read as cheap and break the thesis. |
| Emissive, not lit | The screen is a light source. It has no lighting model of its own; brightness is emission, not illumination. |
| Thin rules and hairline dividers are **native** and permitted | The matrix banned drawn lines in favour of unlit gutters. That rule dies with it: on a high-DPI panel a hairline rule is the correct, native divider, and P5 uses them well. |
| Structural panels may sit on a subtly raised ground | Not floating cards with drop shadows — a flat value step. Depth comes from value, never from a shadow. |
| Brightness stays quantised — see §3 | This is the one matrix rule that **survives and matters most**. State changes must read as events, not as smooth drifts. |
| No drop shadows, no bevels, no glassmorphism, no gradient-filled buttons | These are the tells that turn a broadcast overlay into a web page. The panel is flat; its depth is value and emission. |

### What the substrate change costs, recorded honestly

The matrix gave PRD §8.1's zone stability for free: a shared grid meant zones could not drift. On a
smooth panel that guarantee is gone and must be **replaced by an explicit fixed layout grid enforced
in code** — column and row positions defined once and never computed from content. This is now a
build requirement rather than a property of the material, and it is the thing most likely to erode
during implementation. Reviewers should check it specifically.

**Deliberately not prohibited**, because the surface uses them natively: intense saturation, bloom,
full-bleed colour fields, monumental type scale, hairline rules. The failed density render was not
too bold. It was undisciplined — see §3.

## 2A. The panel is an object in a room

Added 2026-07-25 after the first renders. The design language was accepted; what felt wrong was that
the surface rendered as a **flawless graphic** while the room renders as a painterly, decayed,
physical space. A perfect vector LED board dropped onto that wall reads as pasted on.

The screen is a real panel, hanging in a filthy room, seen from a couch. It must be rendered as one:

- **Glass, not a layer.** The panel has a surface. The room's yellow-green fluorescent reflects
  faintly across it, strongest at the top edge nearest the fixture.
- **Light escapes the bezel.** Blue and magenta bleed onto the peeling wall around the frame. The TV
  lighting the room is not a separate effect; it is the same emission continuing past the edge.
- **The glass is dirty**, because everything in that room is. Faint dust and smear, visible only
  where bright pixels sit behind it.
- **Seen off-axis.** The seated camera is not square to the panel. Whatever perspective the real
  in-room camera gives, the surface accepts.
- **Panel falloff.** Slight brightness loss toward the edges, as real large-format panels have.

None of this changes a token or a layout. It is a rendering obligation, and the flat comps in
`docs/tv-sweat-refinement/` are design references, not the shipping look. **The in-room render is the
only valid acceptance view for this surface.**

### The unified grade

Allen's proposal, adopted as a law: a **single post-process pass applied to the whole game** — room
and TV together — is what will make these two read as one product rather than two assets.

One grade, one grain, one bloom curve, one vignette, one chromatic aberration budget, over
everything. The TV is inside that pass, not exempt from it. A surface that has been graded with the
room is a surface that belongs to the room, whatever its design language.

This is owned jointly with the room team and sits outside this worktree's file boundaries, but it is
recorded here because the TV's design assumes it. Without it, expect the pasted-on feeling to persist
no matter how the screen is composed.

## 3. The one law that makes this work

**Brightness is the primary semantic channel. Hue is secondary.**

The first density render failed because six hues sat at maximum brightness simultaneously, so
nothing receded and nothing led. On a real stadium board, importance is carried by *how hard a thing
is lit*, and hue only says *what kind of thing it is*.

Four brightness levels. Provisional values; they settle against a real panel.

| Level | Use | Roughly |
|---|---|---|
| **L4 — full** | Exactly one element at a time. The current focus: the score at a goal, the cash-out when actionable, the payoff at its callback. | 100% |
| **L3 — active** | Live, current, true-now information. Score, clock, active leg, the ball. | ~70% |
| **L2 — present** | Context that must be readable but is not the subject. Inactive legs, labels, pitch markings, the event strip at rest. | ~40% |
| **L1 — dormant** | Structure and the not-yet. Grid, dividers, `NEXT` legs, empty states. | ~15% |
| **L0 — extinguished** | Dead. Lost legs, resolved-and-gone, unavailable. Visible only as unlit pixel structure. | 0% |

**At most one L4 element exists at any instant.** If two things want full brightness, the design has
not decided what matters. This single rule is what separates this world from the render Allen
rejected.

## 4. Colour

**Settled 2026-07-27 against concept render C**, which Allen selected. Two wrong turns preceded it:
one restored saturated blue/magenta, one went fully monochrome amber. Both are void. The palette is
described below **as it appears in C**, not as a direction to interpret.

**Strategy: cold and quiet, with one warm bar.** A near-black display carrying cold white and grey,
gold reserved strictly for money, and *muted* team hues appearing only on the pitch.

| Role | Treatment | Where |
|---|---|---|
| **Fact** | **Cold white** at L3 | Score, clock, live leg names, market lines |
| **Context** | **Grey** at L2 | Labels, odds, risk and payout figures, pitch markings |
| **Structure / pending** | **Dim grey** at L1 | Not-yet legs, dividers, ticket header |
| **Dead** | **L0** | Lost legs. Nearly extinguished |
| **Money & won** | **Gold** at L3 | A won leg's name and its `WON` marker, payout figures |
| **Action** | **Gold, inverted** at L4 | The cash-out band only — a solid gold field with dark type punched out |
| **Team identity** | **Muted blue and muted pink**, desaturated, small | Player dots on the pitch, and nowhere else |

### The three rules that make this palette work

**Gold is rationed.** It appears on won legs, payout figures, and the cash-out band. Nothing else on
the surface is warm. F failed by making everything amber, which destroyed gold's meaning — when
everything is gold, gold means nothing. The scarcity *is* the signal.

**Team hues are quiet and local.** Blue and pink are desaturated and confined to the pitch dots. They
are the least prominent colours on the display, not the most. E failed by making them vivid and
dominant. Team identity is carried primarily by the ticket column, which names the team in words; the
dots only need to be *separable*, not loud.

**Everything else is colourless.** White and grey do the work. This is what makes the surface read as
cold instrumentation rather than as an app, and it is what gives the single gold bar its force.

### Carried forward

Loss is still darkness — a lost leg drops to L0 and nearly extinguishes. The old green/red money
language stays retired.

**Watch at couch distance:** muted blue and pink dots on a coarse grid, four metres away, in a dark
room. This is the one place the palette could fail. If the two teams are not separable, the fix is
**form** — filled dots versus hollow rings, as monochrome radar has always done — not louder colour.
Adding saturation here is what produced E.

## 5. Typography

Type on this surface is a **light-emitting object on a grid**, not text on a page.

**Required characteristics** rather than a locked file, since the face still needs choosing and
installing:

*Revised 2026-07-26 for the panel substrate. The matrix forced heavy condensed forms because thin
strokes disintegrated on a grid. A high-DPI panel removes that constraint — finer weights and a wider
range are now available, and P5 used them well.*

- A grotesque with a strong technical character. Condensed for the ticket column where density
  matters; the scoreline can breathe wider.
- **Tabular numerals, mandatory, and non-negotiable.** Scores, clocks, money, and counts all change
  in place. Non-tabular figures make the whole surface twitch on every tick. This is the one
  typographic rule that survives every substrate change.
- Weight is now a usable channel alongside brightness — light weights are legible on this panel and
  were not on the matrix. Use it for the L1–L2 tiers rather than relying on dimming alone.
- Uppercase for labels, states, and team names. Mixed case is permitted in longer event copy, which
  the matrix could not really carry.

**Not** any of: Space Grotesk, Space Mono, Inter as display, DM Sans, Outfit, Plus Jakarta Sans,
Instrument Sans, IBM Plex. These are the defaults that mean the search stopped early.

### Scale

Hierarchy is fixed; sizes are provisional against the 980 × 550 reference canvas and settle in build.

| Role | Relative | Level |
|---|---|---|
| Score numerals | 1.00 — largest thing on the surface | L3, L4 at a goal |
| Team names | 0.55 | L3, team hue |
| Clock | 0.50 | L3 |
| Cash-out amount | 0.70 | L4 when actionable |
| `NEED` statement | 0.50 | L3 |
| Live progress | 0.40 | L3 |
| Risk / pays | 0.40 | L2, gold |
| Leg rows | 0.34 | per state |
| Event strip | 0.36 | L2, punches to L3 at reveal |
| Labels, eyebrows | 0.22 | L1–L2, tracked |

The score is the largest element on the surface at all times. Nothing outgrows it, including
cash-out — the failed render inverted this and it was immediately wrong.

## 6. Layout

Layout B, "Ticket Rail", approved 2026-07-25. Full structure and zone rules live in
`docs/tv-sweat-refinement/VISUAL-DESIGN.md` §2; this file governs how it is rendered, not where the
boxes are.

Rendering rules:

- **Every zone position comes from an explicit fixed layout grid defined once in code**, never
  computed from content. On the retired matrix this was free; on a smooth panel it is the single
  most important build discipline, and it is what PRD §8.1's stability now rests on.
- Zones may be separated by **hairline rules or by unlit gutters**, both native to this substrate.
  What remains banned is a *stroked box* around a region — an outlined card is the web reflex, a
  dividing line is not.
- No zone resizes in response to content. Reserved space stays reserved and simply goes dark, so
  nothing ever reflows — this is how PRD §8.5's six cash-out states share one rectangle.
- The ticket column has a fixed width across every market.

**Ticket column width — corrected 2026-07-25.** The first renders drew it at roughly 37% of the
surface and Allen read it as too heavy, correctly. Target is **26–28%**, and the stage takes the
recovered width. Two reasons beyond taste: Phase 2 exists to make scene movement legible, and a
cramped stage undercuts the work before it starts; and the leg rows were vertically airy in the
render, so the column can carry more legs in less width once the rows tighten. Density in the
column, room on the stage.

**Pitch markings — corrected 2026-07-25.** The renders drew them at L3 bright lime, which reads as a
test pattern and competes with the actors. They are **L1–L2** as specified. The pitch is a place, not
an event; the ball and the players are what the eye should find.

## 7. Components

**Scorebug.** Team names in their hues either side of white tabular figures, clock at the far right.
Ticket/leg index at L1, present but subordinate. Records do not appear during live playback.

**Ticket column.** One row per leg, stacked, in ticket order. Risk and pays sit at the foot in gold
at L2.

**Multiple legs can be live at once (PRD §8.2A).** Two or more legs may ride the same match
simultaneously, so the column cannot be built around one expanded row. Rules:

- Every live leg wears the live treatment at the same time. L3 is a tier, not a slot, and it holds
  as many rows as are genuinely live.
- Each live row expands in place to carry its own `NEED` and its own revealed progress. Rows below
  are pushed, never reordered — ticket order is fixed so the player's spatial memory survives.
- Resolved and pending rows compress to a single line, so vertical budget goes to what is live. A
  won leg does not need the same height as a leg still in play.
- One match event may light several rows at the same callback. They update together, on that frame.
- If concurrency ever exceeds the column's height, resolved rows collapse first, then pending. A live
  row is never truncated.

**Leg rows.** Brightness is the state. See §8.

**Stage.** Pitch markings at L1–L2 in green as a *place*. Actors are single lit cells or small cell
clusters in team hue at L3; the ball is the only object permitted L4, and only at a payoff. The
backed player under PRD §7.7 carries a numbered cell — the matrix gives legible small numerals for
free, which is precisely why this world suits that requirement.

**Event strip.** One line, white, L2 at rest, punching to L3 at its reveal callback and settling
back. It never uses money hues, and it never covers the pitch.

**Cash-out slot.** One fixed rectangle at the foot of the ticket column, owning all six states.

**The held preview (PRD §8.10).** This is the only moment the surface deliberately shows something
that is not yet true, so it must read as provisional at a glance and must never be mistakable for a
settled ticket. The world already has the vocabulary:

- **Preview lives one brightness level down.** The previewed bank and the struck legs render at L2,
  not at the L3 they would occupy once real. On this surface, dimmer means *less true*, which is a
  distinction the LED substrate can carry that a colour change could not.
- **The held key is the only L4 element** for as long as it is held. The action being contemplated is
  the subject; the consequence is context.
- **Struck legs use the `VOID` strike**, not the `LOST` extinguish. They are being *cancelled*, not
  lost, and the two must not be confused at the moment a player is deciding.
- **Release restores in one discrete step**, no ease. Consistent with §9: this world changes state,
  it does not tween between poses. An eased revert would read as the surface settling into the
  preview rather than abandoning it.
- **No pulse.** The `LIVE` pulse continues on live legs beneath, unaffected. The preview adds no
  motion of its own, because motion here would read as commitment.

**Stats panel (PRD §8.8).** Opens from the head of the ticket column and freezes playback. It expands
over the ticket column and stage without moving either — when it closes, everything beneath is
exactly where it was. Per-team rows use team hues; all values are revealed-ledger values only.

## 8. State vocabulary

### Leg states

| State | Treatment |
|---|---|
| `NEXT` | L1, structure only. Not yet powered. |
| `LIVE` | L3 white, the surface's only slow pulse. Nothing else pulses, so this is unmistakable. |
| `W` | L3 gold, solid, no pulse. |
| `L` | **L0.** Goes dark. Remains as unlit pixel structure. |
| `VOID` | L2 cyan, struck through on the matrix. |

### Cash-out slot states, PRD §8.5

| State | Treatment |
|---|---|
| Actionable | Gold at **L4** — the one full-brightness element on the surface. Amount and key prompt. |
| Price animating | Gold at L3, amount visibly settling, `UPDATING`. Never L4: it is not yet acceptable, and brightness must not promise what input will refuse. |
| Suspended | L1, unlit slate, `MARKET SUSPENDED`. The rectangle holds its space and goes nearly dark. |
| Pending window | As suspended. Intervention controls live in their own overlay, never in this row. |
| Unavailable | L1, quiet, no reflow. Copy only when the absence needs explaining. |
| Accepted | Gold, brief L4 punch, then `CASHED OUT $x` at L3 into the settle transition. |

**The brightness of this slot is a promise about input.** L4 means the key will work right now. This
is the visual half of the TVS-H01 contract currently being repaired — if the slot is bright and the
press does nothing, the surface has lied.

## 9. Motion

The world's native motion is **panel refresh**, not animation. Things change state; they do not ease
between poses.

- State changes are **quantised** — a brightness level swaps in a discrete step. No 300ms colour
  drifts.
- The ball and actors move continuously; everything else changes discretely. This separation is what
  keeps the match legible against a static information surface.
- One slow pulse *kind* exists on the whole surface: `LIVE`. Adding a second kind destroys the first.
  When several legs are live at once (PRD §8.2A) they all pulse **in phase, off one shared clock**, so
  the surface reads as one system breathing rather than several things blinking independently. That
  shared phase is also what makes concurrency legible — legs that pulse together are legs that are
  live together.
- Score, count, and event all land **on the same frame** as their causal callback. PRD §4.1 is not a
  timing preference; a change that arrives early is a lie.
- Standing freezes everything, including bloom decay and pulse phase — PRD §4.4 is literal, and
  Phase 1B is currently repairing the 21 timers that ignore it.
- No camera shake, cut, or zoom. Decision B holds.

## 10. Open, deliberately

- ~~The held cash-out preview.~~ **Approved by Allen 2026-07-26**, PRD §8.10. Rendering rules are in
  §7 above under *Cash-out slot*.
- **The typeface.** Characteristics are specified in §5; an actual file still needs choosing.
- **Exact brightness values and pixel pitch.** Provisional until seen on the real TV at the real
  seated camera distance.
- **Whether the room re-tints from TV light in-engine.** Briefed to the room artist lead; if the rig
  supports it, big payoffs should drive it, and §3's L4 rule extends into the room itself.
