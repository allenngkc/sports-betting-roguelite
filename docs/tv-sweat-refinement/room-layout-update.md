# Room update — layout change and the TV enclosure

**To:** room / environment lead · **From:** TV sweat slice · **Date:** 2026-07-26
**Decided by:** Allen, from concept renders A/B/C

Copy-paste from the line below.

---

**Subject: Room layout update — two bunks, and the TV becomes a piece of equipment**

Two changes came out of the sweat-UI concept round. Both affect the room, one of them meaningfully.

## 1. Layout — two bunk frames, not one

The approved layout now has **two bunk frames instead of one**:

- one over the **sofa** (as before);
- one over the **desk**.

Window sits between them on the far wall, still looking onto the neon city skyline at night. TV moves
to its own wall. This makes the room read as genuinely built for more than one occupant, which is a
better fit for a bunker and — see §3 — is now load-bearing for a game-design idea.

Everything else about the room is unchanged: peeling paint, exposed conduit, riveted steel
construction, patched sofa, battered desk with laptop and ashtray, metal stool, coiled cable, single
sickly fluorescent strip, olive and khaki palette.

## 2. The TV is no longer a television — it is bolted equipment

This is the bigger change and it is a **prop change, not a screen change.**

The wall-mounted consumer flat-screen is replaced by a **hardened industrial display housing**:

- heavy steel frame bolted flush into the wall, **visible rivets** around the bezel;
- thick painted metal surround, scuffed and chipped, same wear language as the bunk frames;
- the glass slightly **recessed** into the housing rather than flush;
- a small **stencilled equipment code** on the frame — the concept render used `SYS-7B 534-2A`, and
  that style of marking is what we want, not that exact string;
- one small **physical indicator lamp** beside the panel;
- conduit visibly feeding into the housing, continuous with the room's existing pipe runs.

The intent: this display was installed by an institution, not bought by the occupant. It is the same
construction language as the bunks — riveted, painted, industrial, maintained. It should look like it
would survive the building.

This matters because the previous concepts kept reading as a nice TV pasted onto a bad wall. Making
the enclosure part of the room's own construction is what finally seated it.

## 3. Parking-lot idea from Allen, not a request

Allen's note, recorded so it is not lost: with two bunks there is room for **a character who sleeps in
the bunker** and occasionally gives the player advice, charms, or tips.

This is a **game-design idea, not an art request.** Nothing to action. Flagged only because if it
survives, the second bunk stops being set dressing and becomes a character's space — which would
change how it should be dressed, lit, and detailed. Worth knowing before that bunk gets finalised.

Allen owns this decision; it sits outside the TV sweat slice.

## 4. Still outstanding from the earlier brief

The unified post-process grade in
[unified-grade-spec.md](unified-grade-spec.md) is still the open item and is the single highest-value
thing for making the screen and the room read as one image. It needs a global volume in `Room.unity`,
which the TV slice is not permitted to touch — so it needs an owner on your side.

## 5. Lighting — final, and it is a three-source system

We sent two conflicting notes on emission colour before this. Both are void; apologies for the churn.
Ignore anything earlier and use this. Allen confirmed it 2026-07-27 against concept render C.

The room is lit by **three sources that must stay distinguishable from each other**:

| Source | Colour | Note |
|---|---|---|
| **Fluorescent strip** | Slightly yellow, warm | Allen likes it as it is. Keep. |
| **Window** | Cool blue, bright at the source, **short reach** | See the falloff note below — this is the one that needs care. |
| **The display** | Predominantly cold white-grey with a small warm gold bar | Not an amber wash and not saturated blue/magenta. It is a mostly colourless screen; its spill is faint and cool, with a touch of gold near the cash-out band. |

The cool light in this room comes from the **window**, not the display. The display is a quiet
screen; its spill is faint.

### Window falloff — the note that matters (Allen, 2026-07-27)

Our previous brief asked for "more blue from the window" and the concept render took that as licence
to tint the whole room. Allen's response: **too blue, keep it natural.**

The correction is about **reach, not intensity**:

- The window can be **bright and saturated at the source** — the neon skyline outside stays vivid.
- Its light must **fall off fast**. Blue pools on the sill, the floor and wall immediately around the
  window, and the near edge of the closest bunk. It does not reach the far wall, the display, or the
  whole floor.
- **The room's own surfaces stay natural** — olive, khaki, drab green, rust, damp concrete. Those are
  the colours of the room, and they should still read as themselves everywhere the window is not
  directly lighting. A blue-teal cast across every surface is the failure mode.

Think of the window as a directional source with a short throw, not an ambient fill. The room is a
dim natural olive space with a bright cold hole in one wall — not a blue room.

## 6. The second bunk should stay dark, and slightly wrong

Allen's note on concept C, recorded because it is a deliberate effect and not an accident of
lighting: the bunk above the desk being darker than the rest of the room **implies another occupant**,
and it reads as faintly unsettling. He likes that.

Keep it. That bunk stays in shadow, dressed as though someone uses it, never lit enough to confirm
whether anyone is there. It should be legible as *occupied* without ever being legible as *empty*.

This connects to the parked game-design idea in §3 — if that character is real, this bunk is already
doing the setup work for them.
