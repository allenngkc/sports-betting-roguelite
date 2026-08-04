# R22 — walkthrough staging note (for Allen)

**Why you and not a script:** gates 6, 7 and 8 are **VOID, not failed**. Gate 8 certified
pre-two-bunk geometry; gates 6–7 were read 2026-07-28 against a room since changed by R5–R10. The
only instrument is a human walking the two-bunk build. Nothing else clears them.

**Certified state:** branch `room-refinement`, HEAD `0654001`. Structural gates on this exact scene:
Gate 2 PASS (4/4 active), Gate 3 PASS (29 colliders, named), Gate 4 PASS (0 mismatches), Gate 5 PASS,
mattress 44.64 inside 43.9 ±1.

---

## 1. Run it

1. Open `unity/SBR/Assets/Scenes/Room.unity`.
2. Press **Play**.

**Do not run `GrayboxRoomBuilder`.** The committed scene is the one the gates certify. The builder is
deterministic so a rebuild *should* reproduce it, but a rebuild you did not need is a variable in an
evidence run.

## 2. Controls

| | |
|---|---|
| Move | **WASD** |
| Look | **mouse** |
| Interact | **E** (or gamepad South) |

Walk speed is **2.2 m/s** — deliberately slow; it is a small room.

## 3. What you are inhabiting

| | |
|---|---|
| Spawn | `(0.3, 0.02, −1.4)` — door end, facing **+Z** into the room |
| Eye height | **1.62 m** standing, FOV 68° |
| Body | capsule **1.70 m** tall, **0.30 m** radius (0.60 m wide), step offset 0.20 m |

Reference poses, if you want to compare against the captures: standing **1.64 m / 68°**, seated at TV
**1.15 m / 17°**, focused laptop **30°**.

## 4. The three gates, in walking order

### Gate 8 — structural: clearance and contact

This is the one the ruling is most specific about — *"slab at y=1.50 overhanging the aisle ~0.35 m,
posts in the lane."* The two-bunk layout changed the numbers. From the built geometry:

| | |
|---|---|
| Bunk 1 slab | x −1.30 → −0.50, underside **y 1.50** |
| Bunk 2 slab | x +0.50 → +1.30, underside **y 1.50** |
| Posts | bunk 1 at x −0.56→−0.50; bunk 2 at x +0.50→+0.56 |
| **Aisle between them** | **1.00 m clear** |
| **You** | **0.60 m wide** — so **0.20 m each side**, if you are centred |

Three things to feel:

1. **The aisle.** 0.20 m of slack per side is tight. Does it read as *cramped bunker* or as *badly
   built*? That distinction is the whole call and only a person can make it.
2. **The posts.** You spawn at **x = 0.30**, which is off-centre toward bunk 2 — your capsule's right
   edge sits at x 0.60 against a post face at x 0.50. Walking straight forward from spawn you should
   meet `Bunk2PostFront` around **z ≈ 0.57**. Expect the controller to slide you left rather than
   stop you dead. **Report which it does** — a snag that reads as a bug is a finding.
3. **The slabs are below your head.** Underside 1.50 m, your capsule top 1.70 m. You should be
   blocked before you can get under one. If you *can* get your camera under a slab, that is a defect
   — say so.

Also worth a look while you are down there: does anything read as floating rather than planted?

### Gate 6 — UI/HUD readability

Walk up to each interactable and press **E**. The interaction prompt is a screen-space overlay.

- Is the prompt legible at the distance you naturally stop at?
- The three screens carry live content — TV, laptop, and the **phone** (live `BookieFeed`, kept per
  your ruling this session). Can you read each from where you would actually stand or sit?

### Gate 7 — UI/HUD contrast

Same pass, different question: not *can I read it* but *does it hold against what is behind it*.

- Prompt against a bright wall pool vs. against dark plaster.
- Screen text against the graded room — grain and chromatic aberration are on. **Legibility outranks
  integration**: if grain or aberration is degrading text, that is the finding and both get backed
  off.

## 5. Two things to ignore

- **The TV's green light spill.** C2, known, interim-tolerated, corrected in TV Phase 3. Do not judge
  the room's colour against it.
- **The far wall and floor aisle reading cool.** Measured, understood, and already with the DD — it
  is the window's own short-reach pool, which §1.2 explicitly sanctions. It is a question about where
  law 1.1's test samples, not a room defect.

## 6. What I need back

Per gate: **pass / fail / needs work**, and for anything short of a pass, *where you were standing*.
A finding tied to a position is one I can reproduce; one without is one I have to hunt for.

If any of it is a geometry change rather than a tuning change, say so explicitly — that voids the
structural gates again and they re-run before anything else lands.
