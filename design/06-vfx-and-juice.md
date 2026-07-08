# 06 — VFX & Juice

## Division of labor (why "not an effects designer" is fine)

Game feel in this genre is ~80% *programmed timing* — easing curves, screen shake, hit-stop, flashes, number tweens, shaders — and ~20% raw art assets. The 80% is code, which Claude writes and iterates with Allen. The 20% is bought or generated (asset packs, procedural particles). Balatro's celebrated look is largely one CRT/distortion shader plus disciplined tweening, built by a programmer, not a VFX artist. That is the model.

**Working method:** Allen describes the feeling ("the leg slamming green should feel like a slot machine paying out, not a checkbox"), Claude implements shader/tween/feedback code, we iterate on dials in short loops. Reference vocabulary: *Juice it or lose it* (Jonasson/Purho) and *The Art of Screenshake* (Nijman) — both worth 30 minutes each early on.

## The stack

| Layer | Tool | Cost |
|---|---|---|
| Tweens | PrimeTween (or DOTween) | free |
| Feedback composition (shake/flash/haptics/sound triggers as reusable assets) | Feel (MMFeedbacks) | ~$40, the one purchase worth making |
| Shaders | Hand-written HLSL/ShaderGraph (CRT, chromatic aberration, glow pulses, burn/dissolve) | code |
| Particles | Unity particle system, procedural configs | code |
| Post | URP Volume: bloom, vignette pulses, screen distortion on jackpots | code |
| SFX | Freesound/asset packs + pitch-ramping in code (escalating combo pitch is a genre staple) | ~free |

## Effect inventory (maps 1:1 to `04-the-sweat.md` beats)

- Ticket lock: stamp thunk, paper receipt print (shader scroll), brief hit-stop
- Live probability bar: breathing/heartbeat easing, accelerates as p approaches 0 or 1
- Drama event tick: typewriter ticker, punch-scale on keywords, crowd-noise swell
- Leg GREEN: slam + shake + green flood + coin burst + pitch-ramped chime (per consecutive green, pitch up — the parlay ladder *sounds* like it's climbing)
- Leg DEAD: 200ms silence (the beat), harsh cut, ticket corner burn (dissolve shader)
- Cash-out counter: odometer roll, gold pulse on round numbers, warning shudder when live p drifts down
- Full payout: receipt tally roll-up, cash confetti, screen-space money rain, freeze-frame stat card (the shareable screenshot — design it as the marketing asset it is)
- Bad beat: desaturation, one sad guru notification. Restraint IS the joke.

## Budget rule

Juice ships with the prototype, not after it. A flat prototype play-tests as "boring math" even when the design is right; this genre cannot be evaluated dry. Minimum viable juice (tweens + shake + sound on leg resolution) is in scope for the very first itch build.
