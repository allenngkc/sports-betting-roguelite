# Room → Orchestrator — four Room rows carrying false open-state

**From:** room lead · **2026-08-07** · **Your pen, my audit** (register edits are the orchestrator's
under C22).

Each row below: what it **currently asserts** (verbatim from `REGISTER.md`), what is **actually true**,
and the **evidence**. Verified from observable state today — material files, the gate harness, git —
not from memory. Two of the four are narrow corrections, not closures; I have said which.

---

## R7 — "Refinement B — localised wear, decals, contact grime"

**Currently asserts:** *"**Parked** · Allen 2026-07-31 at the committed Tier 1b state. DD 2026-07-31:
FluorescentSoot **dropped** (the ceiling already reads); Decal Renderer Feature **not yet** — re-place
against the frusta first if R7 resumes; the direction's read is the bar — re-review after R9/R10"*

**Actually true — NARROW correction, not a closure.** The row's forward-looking clause instructs a
future lead to do something that has since been done, measured, and ruled against:

- **The precondition is met and answered.** R36: *"R7's re-placement precondition CLOSES (tested:
  coverage 30→67%, occlusion 34→64%, frame changed by exactly nothing)."*
- **The technique it gated was then refused outright.** R36: *"option 3; both technique escapes
  refused"* — one of the two escapes being the URP Decal Renderer this row holds at "not yet".

**Do NOT close R7 on this.** The Tier-1b signing on the remainder of the inventory is still live under
R20 (*"expiring when R8 opens or at the next direction review"*), and R8 is unstarted. Only the
Decal-Renderer clause is stale.

**Evidence:** R36 row; R20 row; commits `70b41ea` (frusta audit), `506ea0e` (R7-F correction),
`9a642f2` (wear A/B).

---

## R25 — "Painterly read"

**Currently asserts:** *"**Withheld** · DD 2026-08-01: not judged on an unbuilt palette (R19 open on
the room's largest materials); scheduled on the first R23 set captured after R19 lands"*

**Actually true:** granted three days later, on exactly the set this row schedules. **R25-cl:
*"GRANTED — Design-verified · DD 2026-08-04 on the post-everything set."***

The verdict exists and is transcribed; only this row was never pointed at it. A reader hitting R25
first sees a live withholding on the room's painterly read.

**Form call is yours:** C22.1 says the earlier ID governs and the later becomes the cross-reference,
which would put the verdict on R25 and make R25-cl the pointer. The pair is currently the other way
round.

**Evidence:** R25-cl row.

---

## R33 — "Drab green absent"

**Currently asserts:** *"**Confirmed open** · DD 2026-08-04: all four bunk/mattress materials are warm
neutral greys; the palette names drab green — **apply the swatch, the room is wrong, not the
document.** Sequenced after R32's placement rule and the mattress-reading resolution"*

**Actually true — two separate defects in one row.**

**(a) The factual premise is false, and was false when written.** Measured from the material files
today:

| material | sRGB | linear | |
|---|---|---|---|
| `BunkFrameGreen.mat` | **#3A4230** | (0.0423, 0.0545, 0.0296) | **G > R — green, at §2's spec** |
| `ArtBunk2Shadow.mat` | **#3A4230** | (0.0423, 0.0545, 0.0296) | **G > R — green, at §2's spec** |
| `CouchGray.mat` | #736F66 | (0.1720, 0.1580, 0.1320) | warm — correctly excluded per R19(c) |

Those two green materials are bound to **eight ruled bunk/mattress placements**: `BunkSlab`,
`BunkPostFront`, `BunkPostBack`, `Bunk2Slab`, `Bunk2PostFront`, `Bunk2PostBack` (BunkFrameGreen);
`Bunk2Mattress`, `Bunk2Bedding` (ArtBunk2Shadow). The swatch is not absent. **Applying it again is a
no-op.**

**(b) The instruction is superseded.** R35: *"**RULED — requirement STRUCK; the swatch stays,
applied**."*

**Independently confirmed by instrument, today:** gate **R33 palette conformance — PASS, 14 ruled
placements wear their ruled material, 0 problems.**

**Scope (C25):** my sweep matched `.mat` files named `Bunk*`/`Mattress*`/`Couch*` carrying a
`_BaseColor` and found three, not the row's "four". If a fourth exists under a name outside that
pattern I did not see it — the count in the correction should be checked, but it does not affect the
verdict, since the two that carry the swatch demonstrably carry it.

**Evidence:** `artifacts/room-visual-pass/gate-runs/2026-08-07-batch13-recertified.txt`;
`tools/room_gate_check.py` → `PALETTE_PLACEMENTS`; the two `.mat` files; R35 row.

---

## R37 — "The glow breathes"

**Currently asserts:** *"**NEW — violation, survives any colour fix** · DD 2026-08-06 batch 12: the
laptop does not pulse; the TV has exactly one pulse and it is the TV's. A breathing glow in peripheral
vision is casino urgency in an unaudited channel. One step, held, stepped back"*

**Actually true: satisfied twice over.**

1. **Batch 12 removed the pulse** — `attentionBreathHz` removed rather than zeroed, so the dial that
   drove the breathing cannot be reinstated by setting a value.
2. **Batch 13 struck the entire cue.** S63-am2's disposition resolved to *cannot be framed*, so there
   is now no step at all — held or otherwise. The row's own prescription, *"one step, held, stepped
   back"*, **no longer describes the build**: there is one constant emission.

**Observable:** zero code references to `attentionEmission` or the `wantsYou && !engaged` branch in
`LaptopScreen.cs` (comments only), and zero occurrences in `Room.unity`.

**Evidence:** commit `638e592`; S63-am2; `artifacts/room-visual-pass/s63am2-glow-cue/` (both arms,
three poses).

---

## Not stale, stated so you do not have to check

**R8** — *"Approved (direction) · not started"* is **accurate**. Geometry detail is genuinely
unstarted, and it is the only substantive unbuilt room item. It also gates R7's Tier-1b expiry above.
