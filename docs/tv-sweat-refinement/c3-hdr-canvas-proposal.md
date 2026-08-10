# C3 — TV canvas HDR: status correction and proposed approach

**To:** Studio Orchestrator · **From:** TV sweat lead · **Date:** 2026-07-31
**Status:** the blocker's premise is out of date; scope is smaller than the board records

---

## Correction first

The board records C3 as *"TV canvas cannot carry HDR."* **That was true on 2026-07-27 and was fixed
on 2026-07-28 in `1aa74c3`.** The canvas can carry HDR today.

What was actually wrong, and what the fix was:

- The clamp was **not** the camera or the URP asset. UGUI bakes `Graphic.color` into a `Color32`
  vertex attribute, which clamps at 1.0 no matter how the pipeline is configured. Verified against
  the shipped `com.unity.ugui` source rather than assumed.
- `Assets/SBR/Runtime/Shaders/TvSweatHdrUI.shader` is Unity's `UI/Default` plus one unclamped
  `_HdrBoost` float multiplied into the fragment output. Material floats are not vertex-baked, so
  they are not clamped.
- Canvas backgrounds were also lifted to `(0.048, 0.055, 0.068)` — matching the room team's emissive
  quad exactly — closing the half of the black-floor fix that sat on our side of the canvas.

**So the room worktree is not blocked on capability.** It is blocked, if at all, on *coverage*.

## What actually remains

The shader is deliberately applied to **three graphics only** — the cash-out amount, the big payout
amount, and the gold flood. Boost sits at `1.0` and rises to `1.8` only at a genuine L4 moment.

That narrowness is a design constraint, not an oversight. `DESIGN.md` §3 permits **at most one
full-brightness element at any instant**, and giving the shader to only the elements that can
legitimately be L4 enforces that rule *by construction* rather than by discipline. Routine text
physically cannot reach L4.

So the open question is not "can it" but **"what else should be bright enough for the room's bloom to
catch, without breaking the one-L4 rule?"** That is a design question, not an engineering one.

## Proposed approach

1. **Close C3 as an engineering blocker.** The mechanism exists, is committed, and is tested
   (`TvSweatScreenPaletteTests` asserts the three L4 graphics carry the material and routine text
   does not).
2. **Reopen the residue as a design question for the Design Director**, phrased as: which elements
   besides cash-out and payout should emit above 1.0 — the score at a goal? the live-leg pulse? —
   and what does that do to the one-L4 rule. This lead should not decide it.
3. **The room worktree can proceed now** against the current coverage. If bloom looks thin, that is
   evidence for step 2, not a blocker on step 1.
4. **One genuine dependency remains, and it is the room's, not ours.** Bloom is a single shared
   global volume in `Room.unity`. We cannot tune the TV's bloom without changing how the room's
   fluorescent blooms. If sweat-specific bloom is needed, the clean answer is a second
   higher-priority volume blended during the sweat — the room lead offered to build it and this lead
   has not requested it, preferring one shared pass until a render proves it insufficient.

## Related, and worth the orchestrator's attention

The room lead flagged a concrete legibility risk: the small metadata row — round, bank, payout, seed
— sits closest to the bloom threshold and breaks first as the TV brightens. **The TV has since
brightened.** Nobody has looked at it since.

This lead's position, from `docs/tv-sweat-refinement/room-lead-reply.md`: that row is **system
chrome**, PRD §8.1 puts it at lowest priority, and **it is allowed to degrade**. What may not: the
score, the clock, each live leg's `NEED` line, and the cash-out state. If bloom forces a choice,
sacrifice the chrome.

Confirming that needs a seated capture on a GPU-backed session, which this worktree cannot produce.
