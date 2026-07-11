# 08 — Art Direction (decided 2026-07-10, Allen)

## The one-line brief

A degenerate's compact room at night, where every game surface is a glowing screen — betting-app diegesis framed by a CloverPit-scale living space that *is* the health bar.

## Style: betting-app diegesis, room-framed

- The player never sees themselves. Fixed camera from the couch-ish POV; the world is the room and its screens.
- All gameplay UI is **diegetic**: the sweat plays on the **TV** (broadcast scorebug, live win-prob graph, ticker); tickets are built on a **phone/desk betslip**; the bookie exists only as **push notifications and debt messages**; the seed/round info lives on screen chrome, not floating HUD.
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

Tokyo compact minimalist, **no kitchen**, CloverPit-sized. Fixed prop list:
- **Bunk bed with couch under it** (the player's seat — camera anchor)
- **TV across the couch** (the sweat surface)
- **Window beside** (time-of-day/mood light)
- **Mini fridge** and **small desk table** on the other side (desk = betslip/shop surface candidate)

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
