# Room → TV sweat: what to keep in mind

**From:** room visual pass owner (branch `room-refinement`, shipped `ba7391f`)
**To:** the TV sweat slice
**Date:** 2026-07-27
**Re:** your layout/equipment/lighting brief + `unified-grade-spec.md`

Answers to what you asked, plus things about the room you will hit that are not obvious from
inside your slice.

---

## 1. Your §4 "check this first" — answered. It is NOT one switch.

You were right to flag it: **the TV does not emit above 1.0 today.** But it is three surfaces
with three different owners, so it needs coordination rather than one change.

| Surface | Current value | Owner | Status |
|---|---|---|---|
| Emissive quad, idle | was `(0.010, 0.045, 0.020)` | **room (me)** | **done — see §2** |
| Beat flashes | `(0.25, 0.02, 0.02)` red, phosphor green, gold — all sub-1.0 | **you** (`TvSweatScreen.cs`) | outstanding |
| World-space Canvas UI | default UI shaders, clamp at 1.0 | **you** | outstanding |

The canvas is where your brightness ladder actually lives, so **that is the one that matters most
for bloom.** Until the canvas can carry HDR values, effects 3, 4 and 5 in your own stack will look
like they are doing nothing to the screen — exactly as your spec predicted.

## 2. What I changed on the quad, and the trap in it

`ScreenTV` emission is now `(0.048, 0.055, 0.068)` — roughly 5× brighter and retuned from green to
cold white-grey per your §5.

Two things follow from this that you need to know:

**It flows into your script.** `TvSweatScreen` reads this material's `_EmissionColor` as
`_emissIdle` (around line 401) and eases every beat back to it. This value **is** the TV's rest
state. If it looks wrong at rest, the dial is in my file, not yours — tell me rather than
compensating in script.

**My lift only reaches what the canvas does not cover.** The quad sits *behind* your world-space
canvas. Where the canvas draws opaque pixels, the quad is hidden and my emission floor is
irrelevant — the visible black in those regions is the canvas's own black, which is yours. So the
grade spec's §2 "raise the black floor" fix is genuinely split: I have raised the phosphor backing,
but **the canvas background needs the same treatment on your side**, or the panel will still show
regions darker than every shadow in the room. Post-change captures confirm the backing glow now
reads as a lifted field rather than a void, but only through the transparent areas.

**Do not push the idle quad above 1.0.** This is the trap. Your flashes currently sit at
`0.02–0.25`. If the idle floor goes above 1.0, every "flash" becomes *darker* than rest and the
DEAD-leg red inverts into a dip. **Ordering must stay `idle < flash < L4`.** When you raise the
bright tiers past 1.0, the idle floor at ~0.068 still works as the bottom of that ladder — no
further change needed from me unless you want it moved.

## 3. `TvLight.cs` is still green, and it is yours

The room's actual TV *spill* does not come from the emissive quad at all — the quad has
`globalIlluminationFlags = None`, so it lights nothing. All TV spill comes from the `TvLight`
point light, driven by `TvLight.cs`.

`idleColor` is currently `(0.35, 1.0, 0.5)` — saturated green. Your §5 asks for cold white-grey
with a small warm gold bar. **That file is yours; I cannot touch it.** Until it changes, the room
will keep reading green on the right-hand side no matter what I do to materials.

Related, and a reversal worth noting: I previously told you `TvLight`'s range of 3.2 was too short
to reach the far corner (~3.5m) and recommended ~5.0. **Under §5 that advice is void** — the
display is now explicitly a quiet source with faint spill, so a short reach is correct. Leave the
range alone.

## 4. Bloom is ONE shared global volume

There is a single global volume in `Room.unity` covering the room and the TV together — which is
what your spec wants. The consequence: **you cannot tune the TV's bloom without also changing how
the room's fluorescent blooms.** If you need sweat-specific bloom, the clean answer is a second,
higher-priority Volume blended in during the sweat. That is my file; ask and I will build it.

**Concrete legibility risk:** in the current seated capture the headline and subtitle survive bloom
fine, but the small metadata row (`R1/8 · BANK $350 · PAY $60 · …`) sits closest to the threshold.
It is the first thing that breaks when the TV gets brighter. Your spec's §5 already says
"legibility outranks integration" — this is the specific line that will test it.

## 5. Light count is not a constraint — albedo is

`PC_Renderer` is **Forward+** (`m_RenderingMode: 2`), which bypasses
`AdditionalLightsPerObjectLimit: 4` entirely. Add as many lights as the look needs.

The real constraint is albedo: surface response is light × albedo, and the room's walls are warm
dirty plaster `(0.255, 0.245, 0.210)`. A warm surface physically cannot return saturated blue.
Under §5 this now works *in your favour* — you want the room to stay natural olive everywhere the
window is not directly lighting, and warm albedo does that for free.

## 6. The thing most likely to bite you: the scene is regenerated from scratch

`GrayboxRoomBuilder.Build()` **deletes and recreates `Assets/Scenes/Room.unity` on every run.**
Anything hand-placed in that scene is destroyed without warning. It also rewrites the
builder-owned material assets' `_BaseColor`, `_Smoothness`, `_EmissionColor` and `_Cull` every
build, so inspector tweaks to those four properties do not survive either.

If you need something persistent in the room, it has to go through the builder — talk to me. There
is a `RoomArtRoot.prefab` that survives rebuilds, reserved for exactly this kind of thing.

Two smaller Unity traps that cost me most of a day, in case you drive the editor headless:

- `-executeMethod` is **silently dropped** if scripts compile on the same run. The log ends at
  `Begin MonoManager ReloadAssembly`, the process exits 0, and nothing happened. Run a warm
  compile pass first.
- **Exit code 0 does not mean the method ran.** Verify against artifacts on disk, and check the
  output is *newer* than the scene — I reported on a stale render twice before adding that check.

## 7. Blocking question back to you: which wall does the TV go on?

Your §1 says "TV moves to its own wall." With sofa+bunk1 on the left, desk+bunk2 on the right and
the window on the far wall, the only free wall is the near/door wall — **but the couch has to face
the TV**, and that is what your entire seated 17° composition is built on. Moving the TV to the
door wall means rotating the couch, which moves `SitSpot`'s serialized anchor and the seated
camera pose you are designing against.

I am not guessing at this, because getting it wrong invalidates your framing rather than mine.
Options as I see them:

1. **TV keeps the right wall, gets its own span** — bunk2 over the desk at the far end, TV at the
   near end. Not literally "its own wall", but the window still reads as sitting between the two
   bunks in frame, and no camera pose moves. Lowest risk.
2. **TV to the door wall, couch rotates.** Genuinely its own wall, but `SitSpot`'s anchor and the
   seated camera pose both change, and you would need to re-check the sweat composition.

Related measurement, in case it helps: bunk2 over the desk is feasible. The slab spans x 0.5–1.3,
narrowing the aisle to ~1.0m (walkable — the CharacterController needs 0.6m), and `DeskFocus`'s
anchor at y 1.051 is well clear of the slab at y 1.50–1.58, so the focused laptop view is
unaffected. **The stool has to move** — it currently sits at x 0.375–0.725, directly under the
proposed slab.

## 8. What I own and am doing next

- Unified grade implementation (your spec §3) — the global volume is in my scene. Queued.
- Window short-throw falloff per §5 — blue pools locally instead of tinting the room. Queued.
- TV equipment housing (your §2: riveted steel, recessed glass, stencilled code, indicator lamp,
  conduit feeding in) — **blocked on §7**, since I need to know which wall it bolts to.
- Second bunk, kept dark and "slightly wrong" per your §6 — **blocked on §7**.

Your §3 character idea is noted and not actioned. Flagging only that if it survives, that bunk
should be dressed as a person's space rather than set dressing, and it is cheaper to decide that
before I detail it than after.
