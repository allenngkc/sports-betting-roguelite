# Design

<!-- impeccable:design-schema 1 -->

**Scope:** the TV sweat surface. This is the durable visual system for the match-theater screen —
the television inside the room. It does not govern the laptop/SureThing sportsbook, the phone, or
the room itself, all of which have their own authority.

**Supersedes:** `design/08-art-direction.md`, deprecated by Allen 2026-07-24. That world — casino
neon on black, CRT phosphor and scanlines, green/red/gold colour purity — is an explicit
anti-reference. Landing back on it means the redesign did not happen.

**Status:** written 2026-07-25, before the first build edit. Tokens marked *provisional* settle when
the first implementation lands.

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

## 2. The substrate: one LED matrix

This is the material, and it is load-bearing. Everything on the surface is drawn on a **single
fixed-pitch LED matrix** that spans the whole screen.

| Rule | Why |
|---|---|
| One pixel pitch, whole surface, no exceptions | A shared grid is what makes zones physically unable to drift. The stability PRD §8.1 demands stops being discipline and becomes geometry. |
| Unlit cells are visibly dark, not absent | The dark gaps between pixels are the material's signature. Remove them and this becomes a generic dark UI. |
| Halation between adjacent lit cells | Real LED bleeds. This is the world's native glow and is **not** the banned CRT phosphor treatment — it is per-cell bloom on a hard grid, not a soft screen-wide haze. |
| Type is drawn on the matrix, never off it | No anti-aliasing that ignores the grid. Letterforms sit on cells. |
| Brightness is quantised, not continuous | A handful of levels, not a smooth ramp. Quantisation is what makes state changes read as *events* rather than drifts. |
| No shadows, no depth, no bevels | An LED panel is flat and emissive. It has no lighting model. Drop shadows are the single fastest way to make this look like a web page. |

**Deliberately not prohibited**, because the world uses them natively: intense saturation, bloom,
full-brightness colour fields, monumental type scale. The failed density render was not too bold. It
was undisciplined — see §3.

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

**Strategy: committed, on true black.** The substrate is black because unlit LEDs are black. Colour
is emitted, never applied.

### Roles

| Role | Hue | Carries | Notes |
|---|---|---|---|
| **Team A** | Electric blue | Team identity, that team's dots, its name, its score | Identity only — never money meaning |
| **Team B** | Hot magenta | As above | |
| **Fact** | White | Score, clock, factual event copy, counts | The neutral truth channel. Highest legibility, no emotional load |
| **Live** | Cyan | Active leg, labels, chrome, "this is happening now" | Distinct from Team A blue by being lighter and greener |
| **Money & action** | Gold | Cash-out, payout, risk, won | The **only warm hue on the surface.** Maximum contrast against an all-cool palette, and it rhymes with the room's fluorescent just enough to belong |
| **Dead** | *(none)* | Lost legs, expired offers | See below |

### Two decisions worth defending

**Loss is not red. Loss is dark.** On an LED board the strongest available statement is a thing that
stops emitting. A lost leg goes to L0 and remains only as unlit pixel structure. This is
world-native, costs nothing, reads instantly at four metres, and is thematically exact — losing
returns you to the room. It also removes a real collision: at LED saturation, red and hot magenta
are hard to separate, and magenta is already a team.

Red survives only as a rare alarm, never as a state colour.

**The old green/red money language is not carried forward.** Allen released it on 2026-07-24. Two
reasons not to re-adopt it by reflex: phosphor green would rhyme with the room's sickly yellow-green
fluorescent and weaken the contrast the whole design depends on; and gold-versus-dark expresses the
same axis with better couch legibility. Money-good is gold. Money-bad is unlit.

Green returns in one place only — the pitch surface, which is a *place*, not a *state*, at L2.

## 5. Typography

Type on this surface is a **light-emitting object on a grid**, not text on a page.

**Required characteristics** rather than a locked file, since the face still needs choosing and
installing:

- Heavy condensed grotesque. Condensed because LED pixels are expensive and the world's own
  signage is condensed; heavy because thin strokes disintegrate on a matrix.
- **Tabular numerals, mandatory.** Scores, clocks, money, and counts all change in place. Non-tabular
  figures make the whole surface twitch.
- Uniform stroke weight, minimal contrast, closed apertures.
- Legible at 2 cells of stroke width.
- Uppercase for all labels and states. Mixed case only in long event copy, if anywhere.

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

- Every zone boundary falls on a matrix cell edge. No fractional positioning.
- Zones are separated by **unlit gutters**, never by drawn lines. On an LED board, absence is the
  divider. A stroked border is a web reflex.
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

- **The held cash-out preview.** The dealt staging proposed holding the key to see the settled future
  in place — bank updated, legs struck — releasing to revert with no residue. It fits the one
  decision this surface has, and confirming would merely keep what is already visibly true. It is
  arguably a new mid-sweat verb, and PRD §3 now permits exactly one, already spent on the stats
  panel. **Awaiting Allen's ruling.**
- **The typeface.** Characteristics are specified in §5; an actual file still needs choosing.
- **Exact brightness values and pixel pitch.** Provisional until seen on the real TV at the real
  seated camera distance.
- **Whether the room re-tints from TV light in-engine.** Briefed to the room artist lead; if the rig
  supports it, big payoffs should drive it, and §3's L4 rule extends into the room itself.
