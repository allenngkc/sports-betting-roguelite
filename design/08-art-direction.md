# 08 — Art Direction (decided 2026-07-10, Allen)

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

> **AMENDED 2026-07-28 — this rule governs SCREENS, not the room's environment art.**
> The reserved-palette law above still holds everywhere a colour carries a money meaning: the
> sweat, the scorebug, the sportsbook OS, notifications. The plans that inherit it
> (`design/04`, `docs/1-plans/F_0.2.0…`, `F_0.3.0…`) are unaffected.
>
> What Allen revoked on 2026-07-25 (`DECISIONS.md`) is its reach into **static room art**. The
> room no longer has to be a cool-blue palette, and green/red/gold are no longer forbidden in
> environment materials, lighting and props. The shipped room is olive and khaki lit by a
> yellow-green fluorescent — none of which the original wording permitted.
>
> The distinction that matters: a colour is reserved when it *signals* money. A sickly fluorescent
> tube is not a money signal, and never was — the old wording just could not tell the difference.

## The room (Allen's spec — scope-locked)

Tokyo compact minimalist, **no kitchen**, CloverPit-sized, walkable in first person. Fixed prop list with interaction roles:
- **Bunk bed with couch under it** — the sweat-watching seat (sit → camera settles on the TV)
- **TV across the couch** — the sweat surface (live games, scorebug wall lategame)
- **Window beside** — time-of-day/mood light (state-driven)
- **Small desk table with laptop** — the book: ticket building, shop, run info
- **Mini fridge** — flavor interaction (state-driven contents: heater = stocked, buried = empty)
- **Phone** — bookie notifications, debt messages, cash-out buzz (audible anywhere in the room)

> **AMENDED 2026-07-28 — the built room diverges from this list. Signed off; see
> `docs/room-visual-pass/SIGNOFF.md`.**
> Interaction roles above are all still accurate and unchanged. What changed is form and tone:
>
> - **Tone: "Tokyo compact minimalist" is superseded by direction B, "Vice Grip"** — a grimy,
>   compressed bunker in olive and khaki. Chosen from a three-way concept round
>   (`docs/room-visual-pass/concepts/`), ratified in `DECISIONS.md` 2026-07-25.
> - **Two bunks, not one.** The second sits over the desk and is deliberately kept in shadow —
>   legible as occupied, never as empty. A parked design idea (a character who sleeps in the
>   bunker) would make that bunk their space; it is dressed to survive that decision either way.
> - **The TV is not a television.** It is bolted institutional equipment: riveted steel housing,
>   recessed glass, stencilled asset code, indicator lamp, conduit feeding in.
> - **The window is a load-bearing light source**, not just mood: it is the room's only cool light,
>   and deliberately short-throw so blue pools locally rather than tinting the room.
> - Mini fridge and its state-driven contents are built but currently undressed.

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
