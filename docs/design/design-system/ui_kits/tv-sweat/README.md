# TV sweat — the match theatre

A click-through recreation of the **FINAL** TV visual direction (approved by Allen 2026-07-27 against
concept render G) on its **980 × 550** reference canvas, in **Layout B "Ticket Rail"** (approved
2026-07-25).

Source of truth: `tv/DESIGN.md` (the visual system) and `tv/VISUAL-DESIGN.md` §2, §5–§9 (layout,
components, copy, states). Nothing here is a new design.

## Files

| File | What |
|---|---|
| `index.html` | The kit. Loads the design-system bundle, then the app. |
| `app.jsx` | Layout B composition plus a beat-by-beat sweep of one ticket. |
| `data.js` | The ticket, its legs, and seven authored beats with their revealed facts. |

## What is interactive

- **Beat ◄ / ►** steps the match. Score, clock, leg states, the event line and the cash-out offer all
  change together, on the same beat — a change that arrives early is a lie.
- **The cash-out band** accepts when it is actionable. It is the surface's only L4 element, and its
  brightness is a promise about input: if it is bright, the key works right now.
- **Stats** opens the panel from the head of the ticket column and freezes playback. It expands over
  the column and stage without moving either.
- **Ticket card** shows the interstitial between tickets.

## What the composition is doing

- Reading starts at the left, so the **ticket column** is there: the first thing the eye lands on is
  the bet, which is what the product is about. The match is what the bet is made of, not the subject.
- The column holds **26–28% of the width** (corrected down from ~37%). Density in the column, room on
  the stage.
- **Brightness is the semantic channel.** Exactly one L4 element exists at a time. A lost leg drops to
  L0 and remains as unlit pixel structure — loss is darkness, not red.
- **Gold is rationed** to won legs, risk/pays and the cash-out band. Nothing else is warm.
- **Team hues are muted and local** — the pitch dots only. Identity is carried by the words in the
  ticket column.
- The pitch is a **place, not an event**: markings sit at L1–L2 so the ball and the actors are what
  the eye finds.

## What is deliberately not here

- No scanlines, screen curvature, phosphor haze, interference noise or any other treatment that says
  *broken*. The display is a decade old and works perfectly.
- No drop shadows, bevels, glassmorphism, gradient-filled buttons or stroked boxes around zones.
- No second pulse kind. `LIVE` is the only pulse, and concurrent live legs share one clock.
- No per-leg risk and payout. One ticket-level `RISK` and one `PAYS`, per PRD §8.4 — the approved
  render gets this wrong and the PRD wins.
- No enclosure. The riveted steel housing, the glass, the dust, the light escaping the bezel and the
  unified grade are **room props and a rendering obligation**, not part of this canvas. The flat view
  here is a design reference; the in-room render at the seated camera is the only valid acceptance view.

## Known debt in the shipped build

`TvSweatScreen.cs` still runs a static-noise crawl and a `_scanlines` overlay, both banned by name;
`chromeCyan` is still used broadly for a role that no longer exists; and two hardcoded emission rest
values bypass the room-owned idle, one of them darker than the agreed black floor.
