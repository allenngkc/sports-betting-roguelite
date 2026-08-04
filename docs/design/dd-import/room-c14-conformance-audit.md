# Room — C14 1:1 conformance audit

**From:** room-refinement lead · **Date:** 2026-07-31 · **Editor:** not used, and not needed to review
**Audited against:** `docs/design/room-design.md` (your doc) + `room-treatment-map.svg`
**Coverage:** ~90 normative statements across the direction contract, all 9 sections, and the
treatment map's implementation law. **22 gaps.** C13 (screen content) excluded as out of scope.

Readable without repo access — every value is quoted inline.

---

## 0. How to read this, and the one thing I need from you

C14 hardened to *1:1 is the bar; deviations only where physically impossible, each signed before
build*. That makes one question decisive for every gap, and it is the question I cannot answer
alone: **when the room and your doc disagree, which one is wrong?**

Your doc is the authority. Where it is right, the room is a defect and I fix it. Where it is
stale, the room is correct and the fix is a doc correction — **yours to make, not mine.** I have
proposed a verdict for each, but the proposal is mine and the ruling is yours.

| Verdict | Count | Who acts |
|---|---:|---|
| **ROOM WRONG** — build defect | 7 | me, needs an editor lease |
| **DOC STALE** — room is correct | 5 | you |
| **NEEDS YOUR JUDGEMENT** — genuinely ambiguous | 6 | you, then possibly me |
| **CROSS-SLICE** — not room-owned | 1 | escalation |
| **UNSIGNED DEVIATION** — needs signature under hardened C14 | 3 | you |

I verified the consequential findings myself rather than relaying them: the collider inventory by
parsing the built scene, every quoted line of your doc, and the shared-material claim.

## 1. ROOM WRONG — build defects, mine to fix

| # | Your doc says | The room does | Note |
|---|---|---|---|
| 1 | §1.6 "**27 colliders throughout**" | **31 physics colliders.** 27 BoxColliders (24 solid, 3 triggers) **plus 4 solid MeshColliders** on `TVScreen`, `WindowPane`, `LaptopScreen`, `PhoneScreen` | 2 of the 4 are load-bearing — see §3.1, the count is not a pure defect |
| 2 | §4 "meshes are built at **true world size with `localScale` 1**" | 5 objects scale a unit primitive by transform instead — the four screen quads plus the stencil plate | **Same root cause as #1**: one builder helper never got the collider-strip and true-size-mesh treatment that two sibling helpers both have. One fix closes both |
| 3 | "**The laptop is the opposite: his own machine** — personal, chosen, probably cheaper" | Laptop body, TV body and phone body all use the **identical material** (`#3C3C38`) | The sharpest one. Your story is institution-versus-personal; the personal object is materially indistinguishable from the institution's. Not a swatch nitpick — it is the story law |
| 4 | §2 palette: `Drab green #3A4230` — "bunk frames, mattress fabric" | The swatch appears **nowhere in the room**. All four bunk/mattress materials are warm neutral greys (`#736F66`, `#76756F`, `#4C4B45`, `#44423D`) | Checked for conflict with the retired-green law: none. §2's drab green is a surface colour, the retirement is the signal-green law. The swatch is legitimate and simply unused |
| 5 | §2: `Steel #3A3F42` (housing), `Conduit #22252A` (pipe runs) | `#585752` and `#373532` | Both ~30–60% lighter **and hue-flipped**: your swatches are B≥G>R (cool), the built values are R>G>B (warm) |
| 6 | §6: TV housing "**thick chipped paint**"; "a **battered** metal desk" | Both are flat untextured materials. No chipping, no battering anywhere on either | R7 wear was parked before it reached the housing or the desk |
| 7 | §7 "very low **exponential** fog" | `ExponentialSquared` | A different falloff curve. Trivial to change; flagging because 1:1 is the bar |

## 2. DOC STALE — the room is right, these are your corrections

| # | Your doc says | Reality |
|---|---|---|
| 8 | §1.2 names **three** light sources; §8 says "one overhead tube and **three local** sources" (four) | The room has **8 Light components**, and neither count has a slot for the **desk lamp** — which Allen approved at Phase 6 from concept render C, and which the code itself calls "the fourth source" |
| 9 | The light inventory omits `MoonDirectional` | It has existed since Phase 2: a cool **Directional** light, `#C4CEED`. See §3.2 — it may also bear on §1.1 |
| 10 | §3: "The TV's spill is **currently green `#59FF80`**" | Actual value is `#A0FFBC` — markedly less saturated. Not drift (C2 governs it), but the doc's factual claim about the present state is wrong |
| 11 | §9: R9 "approved with bounds", R10 "approved, bounce first" | Both are **implemented, gate-measured and merged**. R9 measured as a no-op; R10 landed on the grazing fallback at 1.24×, bounce having been tried and falsified twice. A reader of the doc alone would not know |
| 12 | §6 lists occupant-legibility as "the laptop, the ashtray of butts, the traffic path" | The room also has a paper stack, a dead can and a floor box. Harmless if the list is illustrative; a gap if exclusive — your call which you meant |

## 3. NEEDS YOUR JUDGEMENT

### 3.1 The collider count — 27, 29, or 31?

The four extra colliders are not equivalent:

- `LaptopScreen` and `PhoneScreen` sit on the **Interactable layer** and are very likely
  load-bearing for the interaction raycast. Removing them may break interacting with the laptop
  and phone.
- `TVScreen` and `WindowPane` sit on the **default layer** and are stray — redundant with the
  wall and body colliders already behind them.

Three coherent outcomes: correct §1.6 to **31** and accept all four; remove the two stray and
correct to **29**; or move interaction collision onto the bodies and genuinely reach **27**. I
recommend the middle one but it touches interaction, so it is not mine to choose.

**Related and material:** my own gate harness counts only `BoxCollider`. It is blind to this
entire class, which is why "27 colliders PASS" has been reported all session on a check that
could not see the other four. That is a defect in my tooling and I will fix it — but the expected
count encodes your answer above, so I am not setting it until you rule.

### 3.2 Does `MoonDirectional` breach law 1.1?

§1.1 makes "a blue-tinted room" the explicit failure mode. `MoonDirectional` is a cool-toned
**directional** light, which by construction reaches every surface uniformly. It is unaccounted
for in your doc, and it sits exactly where that failure mode would live.

I am not asserting it breaks the law — it has been present through R5, R6, R9 and R10, all of
which you design-verified or bounded. But it deserves an explicit ruling rather than continued
silence.

### 3.3 Chromatic aberration — 0.065 against "~0.08"

−19% against spec, and self-documented in code as a deliberate call citing **your own** clause:
"legibility outranks integration." I believe it is already compliant, but under 1:1 I would rather
you bless it explicitly than have it stand as an unexamined numeric miss.

### 3.4 Two soft reads I could not settle from source

The window skyline is generated as muted sodium and cold-office blocks rather than anything
reading as **neon** (your direction contract says "onto a neon skyline"); and the "painterly
semi-realistic, not stylised, not photoreal" register is a judgement only you can make.

## 4. CROSS-SLICE — red has no exception

§2 states "green/red is **retired**." Green has a tracked exception in your own register (C2, TV
spill). **Red has none anywhere in the document** — and the TV's runtime light actively performs
"a red wash on a DEAD leg."

The inconsistency is inside the codebase too: the room's phone light cites the palette law by name
to justify avoiding green, while the TV's red behaviour three lines away in a sibling file is never
reconciled with it. The TV file is not room-owned and I have not touched it. This needs either a
tracked red exception like C2, or a change on the TV side.

## 5. UNSIGNED DEVIATIONS — C14 hardening makes these owing

Previously accepted as "parked" or "good enough". Under *deviations only where physically
impossible, each signed*, parked is no longer a status.

**13. R7 wear shipped at a 1.92% pixel footprint** against a contract calling for peeling edges,
damp boundaries, rust streaks, drips, corner dirt and paint chips. Parked by Allen on my
recommendation — but parked is not signed-impossible. Either it resumes, or the deviation needs
your signature.

**14. The floor reads flat (2.45% relief), and this is the sharp one.** It is **not physically
impossible** to fix — it is a consequence of the approved one-tube lighting, which lights the floor
from above at θ ≈ 10–30°, near the worst case for revealing relief. So under C14 this is a fork:
either a signed deviation, or the lighting design changes. **"Impossible under the approved design"
is not the same as "physically impossible,"** and that distinction is yours to draw, not mine to
assume.

**15. Gate 8, walkable clearance, may certify geometry that did not exist when it was tested.**
The sign-off records it PASS on Allen's in-editor playtest — but in the conversation record that
playtest **predates the two-bunk layout brief**, and bunk 2 added three colliders including a slab
whose underside sits at y = 1.50 overhanging the walkable aisle by ~0.35 m, with posts inside the
lane. I wrote that sign-off, so this is my overstatement to correct. It now matters more, not less,
given the four undocumented solid colliders in §1. Cheapest resolution is a human walkthrough, or
Allen confirming he walked the two-bunk version.

Gates 6 and 7 (UI readability) were also verified on 2026-07-28; the room has changed substantially
since — R5, R6, R7, R9, R10 — and they have not been re-read by a human.

## 6. What no audit can close

**§1.1 currently self-reports as failing.** Your doc states the graded captures "read cool-blue
overall and must not be sampled for colour," and instructs the reader to judge colour against the
document rather than the frames.

That is the top law of the room reporting itself as unmet, in the authority document, with the
usual evidence route explicitly ruled out. It cannot be settled by this audit, by a screenshot, or
by me. It is the single largest C14 item in the slice and it needs your ruling on how conformance
is to be demonstrated at all.

## 7. What I need back

1. **Which side is wrong**, per gap — the room or the doc.
2. **The collider ruling** (§3.1), which also unblocks my gate harness fix.
3. **Signatures or resumptions** for the three unsigned deviations (§5).
4. **A route for §1.1** — how conformance to the room's top law should be demonstrated.

Nothing here is being built until you rule. Under hardened C14 a deviation is signed before build,
and the room is currently merged and holding.
