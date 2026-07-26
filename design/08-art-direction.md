# 08 — Art Direction (decided 2026-07-10, Allen) — **DEPRECATED 2026-07-24**

> **DEPRECATED by Allen, 2026-07-24.** This document is no longer binding. The TV sweat refinement
> is a redesign, not a polish pass, and the visual world below is being replaced rather than
> extended. Nothing here — the casino-neon-on-black palette, the green/red/gold purity rule, the
> CRT/phosphor/scanline treatment, the capsule composition — constrains new work.
>
> This file is retained for two reasons: it is **evidence** of what the product is and what it was
> reaching for, and it is an explicit **anti-reference** — landing back on this look means the
> redesign did not happen.
>
> What survived the deprecation is recorded in `PRODUCT.md` under Brand Commitments: diegesis,
> voice, fictional-league constraint, and typography carrying heavy load. The room spec below
> (prop list, room-state health bar) is **product truth about the space, not art direction**, and
> remains in force until Allen says otherwise.
>
> The replacement world is decided in `docs/tv-sweat-refinement/` and recorded in `DESIGN.md`.

## The one-line brief

A degenerate's compact room at night, where every game surface is a glowing screen — betting-app diegesis framed by a CloverPit-scale living space that *is* the health bar.

## Style: betting-app diegesis, room-framed (REVISED 2026-07-10, Allen)

- **First-person controllable character** — the player walks around the room (CloverPit model). The character is invisible (no body, no animation pipeline; optional hands as later garnish). Presence through movement, not portrayal.
- **Screens are the interaction surfaces, and any screen can access the book**: walk to the **TV** to watch the sweat (broadcast scorebug, live win-prob graph, ticker), the **laptop on the desk** to build tickets and browse the book/shop, the **phone** for the bookie's notifications and debt messages. All gameplay UI is diegetic; seed/round info lives on screen chrome, not floating HUD.
- Screens get the treatment: phosphor glow, scanline flicker, CRT curvature/chromatic aberration on big hits. Typography is the primary art asset — strong numerals, ticker fonts, sportsbook-app iconography parodied.

## Palette: casino neon on black

| Token | Use |
|---|---|
| Deep black / near-black blues | base, room shadows, screen bezels |
| Phosphor green | money, wins, bank, GREEN legs |
| Hot red | losses, DEAD legs, DEBT, the bookie |
| Gold | cash-out, jackpots, payout moments |
| Dim cyan/white | chrome, clocks, filler ticker text |

Rule: green/red never used for anything but money-good/money-bad — the gambling color language stays pure.

## The room (Allen's spec — scope-locked)

Tokyo compact minimalist, **no kitchen**, CloverPit-sized, walkable in first person. Fixed prop list with interaction roles:
- **Bunk bed with couch under it** — the sweat-watching seat (sit → camera settles on the TV)
- **TV across the couch** — the sweat surface (live games, scorebug wall lategame)
- **Window beside** — time-of-day/mood light (state-driven)
- **Small desk table with laptop** — the book: ticket building, shop, run info
- **Mini fridge** — flavor interaction (state-driven contents: heater = stocked, buried = empty)
- **Phone** — bookie notifications, debt messages, cash-out buzz (audible anywhere in the room)

### Room state = the health bar (~4 variants)

| State | Trigger | Reads as |
|---|---|---|
| 1. Baseline | run start | dingy but okay; neutral light |
| 2. Heater | win streak / bank ≫ target | new couch, a plant, sunlight through the blinds |
| 3. Sweating | debt > 0 | darker, bills on the desk, red notification glow |
| 4. Buried | deep debt / near-death | boarded window, unpaid bills pile, room lit only by the TV |

Production plan (scope guard): the Phase 2 vertical slice ships with **state 1 only, plus a lighting/prop-decal layer** that fakes states 3–4 (red wash, bill decals, darker exposure). Full four-variant art lands after the slice's gate. Room art sourcing: asset-kit bash + AI-gen concept passes, cleanup by hand; if the slice validates, commission a proper environment pass.

## Juice mapping (design/06 effects re-expressed in this style)

- Leg GREEN: TV flash + phosphor bloom + scanline jump; DEAD: TV cuts to static for a beat, room darkens one notch
- Cash-out taken: gold sweep across the TV, register *ka-chunk*, phone buzz
- Bookie float: the room dims, red notification glow, phone rattles on the desk
- Big payout: neon spill from the TV lights the whole room — the room is the reaction shot
- Progressive sweat density (design/04): early rounds = calm TV, simple scorebug; late rounds = picture-in-picture wall, ticker crawl, notification spam

## Capsule / marketing note

The money shot is the room at night, face-lit... except there's no face — the empty couch, the glowing TV showing 61%, the gold CASH OUT prompt. Distinctive against both real-app screenshots and pixel-art competitors; reads at capsule size because it's one light source and three colors.

## Deferred

- Game name: SBR codename until Phase 3 (decided 2026-07-10).
- Character visibility (hands? reflection in the TV during static?): mood garnish, decide during the slice.
- v2: room grows with meta-progression? (parking-lot idea, not committed)
